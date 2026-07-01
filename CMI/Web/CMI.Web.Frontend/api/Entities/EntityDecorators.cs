using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Utilities.Common.Helpers;
using CMI.Web.Common.api;
using CMI.Web.Common.Helpers;
using CMI.Web.Frontend.api.Elastic;
using CMI.Web.Frontend.api.Interfaces;
using CMI.Web.Frontend.api.Search;
using Nest;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Results;

namespace CMI.Web.Frontend.api.Entities
{
    public class EntityDecorator<T> where T : TreeRecord, new()
    {
        // Json output
        private const string ancestorsKey = "ancestors";
        private const string childrenKey = "children";
        private const string childrenPagingKey = "childrenPaging";

        // detailData fields
        private const string detailDataKey = "detailData";
        private const string detailDataPrefix = detailDataKey + ".";

        private readonly IElasticService elasticService;
        private readonly IElasticSettings elasticSettings;
        private readonly IEntityProvider entityProvider;
        private readonly IModelData modelData;

        public EntityDecorator(IElasticService elasticService, IElasticSettings elasticSettings, IEntityProvider entityProvider, IModelData modelData)
        {
            this.elasticService = elasticService;
            this.elasticSettings = elasticSettings;
            this.entityProvider = entityProvider;
            this.modelData = modelData;
        }

        public List<Entity<T>> GetAncestors(Entity<T> entity, UserAccess access, out int maxDepth)
        {
            var ancestors = new List<Entity<T>>();
            var entityId = entity.Data.ArchiveRecordId;

            maxDepth = 0;
            var items = entity.Data.ArchiveplanContext;

            if (items != null)
            {
                var depth = 0;
                foreach (var contextItem in items)
                {
                    var id = contextItem.ArchiveRecordId;
                    if (entityId.Equals(id))
                    {
                        continue;
                    }

                    var item = new Entity<T>
                    {
                        Data = new T
                        {
                            ArchiveRecordId = id,
                            Title = contextItem.Title,
                            ReferenceCode = contextItem.RefCode
                        }
                    };

                    var ancestorOptions = new EntityMetaOptions
                    {
                        SetDepth = depth
                    };

                    var context = GetAsDecoratedContext(item, access, ancestorOptions);
                    item.Context = context;

                    ancestors.Add(item);
                    maxDepth = Math.Max(maxDepth, depth);
                    depth += 1;
                }
            }

            if (ancestors.Count > 0)
            {
                ancestors = ancestors.OrderBy(anc => anc.Depth).ToList();
            }

            return ancestors;
        }


        public JObject GetAsDecoratedContext(Entity<T> entity, UserAccess access, EntityMetaOptions options = null)
        {
            var hasContext = false;

            var context = new JObject();

            entity.MetaData = GetMetadata(entity.Data, access);
            options = options ?? EntityMetaOptions.DefaultOptions;
            var depth = options.SetDepth;

            // ancestors
            if (options.FetchAncestors)
            {
                var ancestors = GetAncestors(entity, access, out depth);
                if (ancestors.Count > 0)
                {
                    hasContext = true;
                    context.Add(ancestorsKey, JArray.FromObject(ancestors));
                    depth += 1;
                }
            }

            // own depth
            entity.Depth = depth;
            depth += 1;

            // children
            if (options.FetchChildren)
            {
                var result = GetChildren(entity.Data, depth, access, options?.ChildrenPaging);
                if (result.Items.Count < result.Paging.Total)
                {
                    result.Items.Add(new Entity<T>
                    {
                        Data = new T
                        {
                            ArchiveRecordId = entity.Data.ArchiveRecordId,
                            ChildCount = 0,
                            Title = "...",
                            IsLeaf = true,
                            Level = "WeitereErgebnisseVorhanden"
                        },
                        Depth = depth,
                        
                    });

                }

                if (result.Items.Count > 0)
                {
                    hasContext = true;
                    JsonHelper.AddOrSet(context, childrenKey, JArray.FromObject(result.Items), true);
                    if (result.Paging != null)
                    {
                        JsonHelper.AddOrSet(context, childrenPagingKey, JObject.FromObject(result.Paging), true);
                    }
                }
            }

            return hasContext ? context : null;
        }

        public EntityResult<T> GetChildren(T entity, int setDepth, UserAccess access, Paging paging)
        {
            paging ??= new Paging { OrderBy = "treeSequence", SortOrder = "Ascending" };

            var result = new EntityResult<T>
            {
                Items = new List<Entity<T>>(),
                Paging = paging
            };

            if (entity.IsLeaf)
            {
                return result;
            }

            var query = new ElasticQuery();
            var id = entity.ArchiveRecordId;

            query.Query = new BoolQuery
            {
                Filter = new QueryContainer[]
                {
                    new TermQuery
                    {
                        Field = elasticSettings.ParentIdField,
                        Value = id
                    }
                },
                MustNot = new QueryContainer[]
                {
                    new TermQuery
                    {
                        Field = elasticSettings.IdField,
                        Value = id
                    }
                }
            };

            query.SearchParameters.Paging = paging;
            query.SearchParameters.Options = new SearchOptions { EnableAggregations = false, EnableExplanations = false, EnableHighlighting = false };

            var queryResult = elasticService.RunQuery<T>(query, access);
            if (queryResult.Entries != null)
            {
                result.Items = entityProvider.GetResultAsEntities(access, queryResult, new EntityMetaOptions
                {
                    SetDepth = setDepth
                });

                result.Paging.Total = queryResult.TotalNumberOfHits;
            }
            else
            {
                result.Paging.Total = 0;
            }

            return result;
        }

        private JObject GetMetadata(TreeRecord entity, UserAccess access)
        {
            if (entity?.Level == null)
            {
                return null;
            }

            var type = modelData.GetEntityType(entity);
            if (type == null)
            {
                Log.Information($"No type found for entity level {entity?.Level} and external template {entity.DisplayTemplateName}");
                return null;
            }

            var language = access.Language ?? WebHelper.DefaultLanguage;

            JObject metadata = null;
            var jsonEntity = JObject.FromObject(entity);
            var detailDatas = JsonHelper.GetTokenValues(jsonEntity, detailDataKey, true) ?? new JArray();
            var descriptors = JsonHelper.GetTokenValues(jsonEntity, "descriptors", true) ?? new JArray();

            var categories = type.MetaCategories ?? new List<ModelTypeMetaCategory>();

            foreach (var category in categories)
            {
                var attributes = new JObject();

                // Nur wenn die Sektion (category) Felder hat und wir mindestens ein öffentliches Feld haben, oder der Benutzer ein AMA Benutzer ist, gehen wir überhaupt weiter.
                // (Fall abfangen, dass eine Kategorie nur interne Felder hat
                if (category?.Fields != null && (category.Fields.Any(f => f.Visibility == (int)DataElementVisibility.@public) ||
                                                 access.RolePublicClient == AccessRoles.RoleAMA))
                {
                    foreach (var field in category.Fields)
                    {
                        // Interne Felder sind nur für AMA Benutzer sichtbar
                        if (field.Visibility == (int)DataElementVisibility.@internal && access.RolePublicClient != AccessRoles.RoleAMA)
                        {
                            continue;
                        }

                        JToken token = null;
                        var name = field.Name.ToLowerCamelCase();
                        if (name.StartsWith(detailDataPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var subName = name.Substring(detailDataPrefix.Length).ToUpper();
                            if (detailDatas.Children().Any(d => d.Children().Any(ch =>
                                    ch is JProperty { Name: "elementName" } jp && jp.Value.ToString().ToUpper() == subName)))
                            {
                                var children = detailDatas.Children().First(d => d.Children().Any(ch =>
                                    ch is JProperty { Name: "elementName" } jp && jp.Value.ToString().ToUpper() == subName));

                                token = MapDetailData(children);
                            }
                        }
                        else if (name.StartsWith("descriptors", StringComparison.OrdinalIgnoreCase))
                        {
                            MapDescriptors(descriptors, attributes);
                        }
                        else if (name.Contains("."))
                        {
                            token = jsonEntity.GetTokenByKey(name.Split('.').First(), true) ?? jsonEntity.GetTokenByKey(field.Key, true);
                            token = token.HasValues ?
                                ((JProperty)token.First.Children().First(
                                    ch => ch is JProperty jCh 
                                          && string.Equals(jCh.Name, name.Split('.')[1],
                                              StringComparison.CurrentCultureIgnoreCase))).Value
                                : token;
                        }
                        else 
                        {
                            token = jsonEntity.GetTokenByKey(name, true) ?? jsonEntity.GetTokenByKey(field.Key, true);
                        }

                        if (token != null)
                        {
                            var value = field.AsFieldValue(token, language);
                            if (value != null)
                            {
                                attributes.Add(name, value);
                            }
                        }
                    }
                }

                if (attributes.Children().Any())
                {
                    metadata = metadata ?? new JObject();

                    if (category.Labels != null && category.Labels.Any())
                    {
                        var title = category.Labels.ContainsKey(language) ? category.Labels[language] : category.Labels.Values.First();
                        JsonHelper.AddOrSet(attributes, "_title", title);
                    }

                    metadata.Add(category.Identifier.ToLowerCamelCase(), attributes);
                }
            }

            return metadata;
        }

        /// <summary>
        ///Only strings, integer, boolean, timePeriod and float values are mapped
        /// Another types throws exception
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>JToken with correct value</returns>
        private static JToken MapDetailData(JToken ch)
        {
            var toke = ch.Type == JTokenType.Object ? (ch as JObject).Children() : (JEnumerable<JToken>?)null;
            JToken token = null;
            var typeName = (toke.Value.First(p => (p as JProperty).Name == "typeName") as JProperty).Value
                .ToString();
            var sb = new StringBuilder();
            switch (typeName)
            {
                case "string":
                    var textValue = toke.Value.First(p => (p as JProperty).Name == "textValues") as JProperty;
                    sb = textValue.Value.Aggregate(sb, (current, text) => current.AppendLine(text.ToString()));
                    token = sb.ToString();
                    break;
                case "float":
                case "float?":
                    var floatValue =
                        toke.Value.First(p => (p as JProperty).Name == "floatValue") as JProperty;
                    sb = floatValue.Value.First.Aggregate(sb, (current, text) => current.AppendLine(text.ToString()));
                    token = sb.ToString();
                    break;

                case "int":
                case "int?":
                    var intValue = toke.Value.First(p => (p as JProperty).Name == "int64Values") as JProperty;
                    sb = intValue.Value.Aggregate(sb, (current, text) => current.AppendLine(text.ToString()));
                    token = sb.ToString();
                    break;

                case "ElasticTimePeriod":
                    var dateRangeValue = toke.Value.First(p => (p as JProperty).Name == "dateRangeValues") as JProperty;
                    sb = dateRangeValue.Value.First.HasValues ? dateRangeValue.Value.First.First.Aggregate(sb, (current, text) => current.AppendLine(text.ToString())) :
                        dateRangeValue.Value.First.Aggregate(sb, (current, text) => current.AppendLine(text.ToString()));
                    token = sb.ToString();
                    break;
                case "bool":
                case "bool?":
                    var boolValue = toke.Value.First(p => (p as JProperty).Name == "boolValue") as JProperty;
                    sb = boolValue.Value.Aggregate(sb, (current, text) => current.AppendLine(text.ToString()));
                    token = sb.ToString();
                    break;

                default:
                    throw new Exception($"Only strings, integer, boolean, timePeriod and float values are mapped! A new case must be added. Missing type {typeName}");
            }

            return token;
        }

        private static void MapDescriptors(JArray descriptors, JObject attributes)
        {
            StringBuilder stringBuilderRegister;
            var thesaurusHelper = new List<ThesaurusHelper>();
            foreach (var ch in descriptors.Children())
            {
                stringBuilderRegister = new StringBuilder();
                var toke = ch.Type == JTokenType.Object ? (ch as JObject).Children() : (JEnumerable<JToken>?)null;
               
                var descriptorName = string.Empty;
                var descriptorSource = string.Empty;
                var thesaurusType = string.Empty;
                var sortingNumber = -1;
                foreach (JProperty te in toke)
                {
                    switch (te.Name)
                    {
                        case "thesaurus":
                            thesaurusType += te.Value;
                            break;
                        case "idName":
                            descriptorName += te.Value;
                            break;
                        case "sortingNumber":
                            sortingNumber = Convert.ToInt32(te.Value);
                            break;
                        case "source":
                            descriptorSource += te.Value;
                            break;
                    }
                }

                if (sortingNumber == -1)
                {
                    continue;
                }

                // Add each descriptor in its own div
                stringBuilderRegister.Append("<div class=\"descriptor-item {0}\">");
                stringBuilderRegister.AppendLine($"{descriptorName}");
                if (!string.IsNullOrEmpty(descriptorSource))
                {
                    stringBuilderRegister.AppendLine($"{descriptorSource}");
                }
                stringBuilderRegister.Append("</div>");

                // Add it to the temporary helper
                thesaurusHelper.Add(new ThesaurusHelper
                {
                    thesaurusType = thesaurusType, 
                    sortNumber = sortingNumber,
                    text = stringBuilderRegister.ToString()
                });
            }

            if (thesaurusHelper.Count > 0)
            {
                SortDescriptors(attributes, thesaurusHelper);
            }
        }

        private static void SortDescriptors(JObject attributes, List<ThesaurusHelper> thesaurusHelper)
        {
            var thesaurusKindGroups = new List<List<ThesaurusHelper>>();
            var thesaurusKind = string.Empty;
            var register = new List<ThesaurusHelper>();
            TagLastFirstItemInCompleteDescriptors(thesaurusHelper);
            foreach (var thesaurus in thesaurusHelper.OrderBy(t => t.sortNumber))
            {
                if (!string.IsNullOrEmpty(thesaurusKind) && thesaurusKind != thesaurus.thesaurusType)
                {
                    thesaurusKindGroups.Add(register);
                    register = new List<ThesaurusHelper>();
                }

                thesaurusKind = thesaurus.thesaurusType;
                register.Add(thesaurus);
            }

            thesaurusKindGroups.Add(register);
            var stringBuilderRegister = new StringBuilder();
            foreach (var group in thesaurusKindGroups)
            {
                stringBuilderRegister.Clear();
                var length = group.Count;
                for (var index = 0; index < length; index++)
                {
                    var isFirst = index == 0;
                    var isLast = index == length - 1;
                    stringBuilderRegister.Append(isFirst && isLast ? string.Format(group[index].text, "first-item-ingroup last-item-ingroup")
                        : isFirst ? string.Format(group[index].text, "first-item-ingroup")
                        : isLast ? string.Format(group[index].text, "last-item-ingroup")
                        : group[index].text);
                }

                attributes.Add(group.FirstOrDefault().thesaurusType, stringBuilderRegister.ToString());
            }
        }

        private static void TagLastFirstItemInCompleteDescriptors(List<ThesaurusHelper> thesaurusHelper)
        {
            var helper = thesaurusHelper.OrderBy(t => t.sortNumber).Last();
            helper.text = helper.text.Replace("descriptor-item", "descriptor-item last-item");
            thesaurusHelper.Remove(thesaurusHelper.OrderBy(t => t.sortNumber).Last());
            thesaurusHelper.Add(helper);
            helper = thesaurusHelper.OrderBy(t => t.sortNumber).First();
            helper.text = helper.text.Replace("descriptor-item", "descriptor-item first-item");
            thesaurusHelper.Remove(thesaurusHelper.OrderBy(t => t.sortNumber).First());
            thesaurusHelper.Add(helper);
        }
    }

    struct ThesaurusHelper
    {
        public string thesaurusType;
        public int sortNumber;
        public string text;
    }
}