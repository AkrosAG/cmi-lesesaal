using CMI.Access.Sql.Lesesaal;
using Nest;

namespace CMI.Web.Frontend.api.Search
{
    public interface IFieldTranslator
    {
        QueryContainer CreateQueryForField(SearchField field, UserAccess access);
    }
}