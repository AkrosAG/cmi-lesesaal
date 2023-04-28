using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Utilities.Common.Helpers;
using Nest;

namespace CMI.Web.Frontend.api.Search
{
    public class ThesaurusTranslator : IFieldTranslator
    {
        public QueryContainer CreateQueryForField(SearchField field, UserAccess access)
        {
            return new BoolQuery
            {
                Must = new QueryContainer[]
                {
                    new MatchQuery
                    {
                        Query = field.Key,
                        Field = "descriptors.thesaurus"
                    },
                    new MatchQuery
                    {
                        Query = field.Value.Escape("descriptors"),
                        Field = "descriptors.name"
                    }
                }
            };
        }
    }
}