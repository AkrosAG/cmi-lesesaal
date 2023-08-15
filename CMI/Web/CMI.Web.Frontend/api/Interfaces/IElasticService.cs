using System.Collections.Generic;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Web.Common.api;
using CMI.Web.Frontend.api.Elastic;
using Nest;

namespace CMI.Web.Frontend.api.Interfaces
{
    public interface IElasticService
    {
        ElasticQueryResult<T> QueryForRootNodes<T>(UserAccess access) where T : TreeRecord;
        ElasticQueryResult<T> QueryForId<T>(string id, UserAccess access) where T : TreeRecord;
        ElasticQueryResult<T> QueryWithFilter<T>(string id, UserAccess access, SourceFilter filter = default) where T : TreeRecord;
        List<TreeRecord> QueryForParentId(string id, UserAccess access);
        ElasticQueryResult<T> QueryForIds<T>(IList<string> ids, UserAccess access, Paging p = null) where T : TreeRecord;
        ElasticQueryResult<T> QueryForIdsWithoutSecurityFilter<T>(IList<string> ids, Paging p = null) where T : TreeRecord;
        ElasticQueryResult<T> RunQuery<T>(ElasticQuery query, UserAccess access, SourceFilter filter = default) where T : TreeRecord;
        string[] GetLaender();
        ElasticQueryResult<T> RunQueryWithoutSecurityFilters<T>(ElasticQuery query) where T : TreeRecord;
    }
}