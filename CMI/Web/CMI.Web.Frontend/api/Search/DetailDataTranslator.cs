using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Utilities.Common.Helpers;
using Nest;


namespace CMI.Web.Frontend.api.Search
{
    public class DetailDataTranslator : IFieldTranslator
    {
        public QueryContainer CreateQueryForField(SearchField field, UserAccess access)
        {
            return new NestedQuery
            {
                Path = "detailData",
                Query = new BoolQuery
                {
                    MinimumShouldMatch = 1,
                    Should = new QueryContainer[]
                    {
                        new BoolQuery
                        {
                            //Filter = new QueryContainer[]
                            //{
                            //    new TermsQuery
                            //    {
                            //        Field = "primaryDataFulltextAccessTokens",
                            //        Terms = access.CombinedTokens
                            //    }
                            //},
                            Must = new QueryContainer[]
                            {
                                new QueryStringQuery
                                {
                                    Query = field.Key,
                                    DefaultField = "detailData.elementName",
                                    DefaultOperator = Operator.And,
                                    AllowLeadingWildcard = false
                                },
                                new QueryStringQuery
                                {
                                    Query = field.Value.Escape(field.Key),
                                    DefaultField = "detailData.textValues",
                                    DefaultOperator = Operator.And,
                                    AllowLeadingWildcard = false
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}