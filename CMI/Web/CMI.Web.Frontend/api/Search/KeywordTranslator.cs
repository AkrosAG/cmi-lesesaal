using CMI.Access.Sql.Lesesaal;
using CMI.Utilities.Common.Helpers;
using Nest;

namespace CMI.Web.Frontend.api.Search
{
    public class KeywordTranslator : IFieldTranslator
    {
        public QueryContainer CreateQueryForField(SearchField field, UserAccess access)
        {
            return new BoolQuery
            {
                Should = new QueryContainer[]
                {
                    new WildcardQuery
                    {
                        Value = field.Value.Escape(field.Key), 
                        Boost = 1.0,
                        CaseInsensitive = false,
                        Field = field.Key
                    }
                }
            };
        }
    }
}