using System.Data.Linq;

namespace CMI.Access.Sql.Lesesaal
{
    public class LesesaalContext : DataContext
    {
        public LesesaalContext(string fileOrServerOrConnection) : base(fileOrServerOrConnection)
        {
        }

        public Table<OrderingFlatItem> OrderingFlatItem => GetTable<OrderingFlatItem>();
        public Table<OrderingFlatDetailItem> OrderingFlatDetailItem => GetTable<OrderingFlatDetailItem>();

        public Table<UserOverview> UserOverview => GetTable<UserOverview>();
    }
}