using LinqToDB.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace LINQ2DB_MVC_Core_5.DB
{
    public class Linq2dbSettings : ILinqToDBSettings
    {
        public string DefaultConfiguration => "SqlServer";
        public string DefaultDataProvider => "SqlServer";
        public readonly IConnectionStringSettings mConnectionStringSettings;
        public Linq2dbSettings(IConfiguration configuration)
        {
            // Figure out the database name from the connection string.
            var sDBConnection = configuration["ConnectionStrings:DefaultConnection"];
            var sProviderName = configuration["Authentication:Linq2db:ProviderName"] ?? "";
            if (sProviderName.Length == 0)
            {
                sProviderName = DefaultDataProvider;
            }

            mConnectionStringSettings = new ConnectionStringSettings
            {
                Name = "DefaultConnection",
                ProviderName = sProviderName,
                ConnectionString = sDBConnection
            };
        }

        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return mConnectionStringSettings;
            }
        }
    }
}
