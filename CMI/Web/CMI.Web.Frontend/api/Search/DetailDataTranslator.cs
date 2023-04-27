using CMI.Access.Sql.Lesesaal;
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
                    Must = new QueryContainer[]
                    {
                        new MatchQuery
                        {
                            Query = field.Key,
                            Field = "detailData.elementName"
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
            };
        }
    }
}