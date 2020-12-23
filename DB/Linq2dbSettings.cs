using LinqToDB.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace LINQ2DB_MVC_Core_5.DB
{
    public class Linq2dbSettings : ILinqToDBSettings
    {
        private const string msDefaultProviderName = "SqlServer";
        public readonly IConnectionStringSettings mConnectionStringSettings;
        public Linq2dbSettings(IConfiguration configuration)
        {
            // Figure out the database name from the connection string.
            var sDBConnection = configuration["ConnectionStrings:DefaultConnection"];
            var arrDBConnName = sDBConnection.Split("Database=");
            var sDBName = arrDBConnName.Length > 1 ? arrDBConnName[1] : "DemoDB";
            var nEndIndx = sDBName.IndexOf(";");
            if (nEndIndx > 0)
            {
                sDBName = sDBName.Substring(0, nEndIndx);
            }
            var sProviderName = configuration["Authentication:Linq2db:ProviderName"] ?? "";
            if (sProviderName.Length == 0)
            {
                sProviderName = msDefaultProviderName;
            }

            mConnectionStringSettings = new ConnectionStringSettings
            {
                Name = sDBName,
                ProviderName = sProviderName,
                ConnectionString = sDBConnection
            };
        }

        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => mConnectionStringSettings.Name;

        public string DefaultDataProvider => "SqlServer";

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return mConnectionStringSettings;
            }
        }
    }
}
