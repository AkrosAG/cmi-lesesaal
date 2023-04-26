using CMI.Access.Sql.Lesesaal;
using CMI.Utilities.Common.Helpers;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMI.Web.Frontend.api.Search
{
    public class KeywordTranslator : IFieldTranslator
    {
        public QueryContainer CreateQueryForField(SearchField field, UserAccess access)
        {
            return new BoolQuery
            {
                //Filter = new QueryContainer[]
                //{
                //    new TermsQuery
                //    {
                //        Field = "primaryDataFulltextAccessTokens",
                //        Terms = access.CombinedTokens
                //    }
                //},
                Should = new QueryContainer[]
                {
                    new WildcardQuery
                    {
                        Value = field.Value, // .Escape("formerReferenceCode")
                        Boost = 1.0,
                        CaseInsensitive = false,
                        Field = "referenceCode"
                    }
                }
            };
        }
    }
}