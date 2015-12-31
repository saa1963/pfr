using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Configuration;

namespace pfr
{
    class ConnectionStringBuilder
    {
        public string getConnectionString(string login, string password)
        {
            var originalConnectionString = ConfigurationManager.ConnectionStrings["pfrEntities"].ConnectionString;
            var entityBuilder = new EntityConnectionStringBuilder(originalConnectionString);
            var factory = DbProviderFactories.GetFactory(entityBuilder.Provider);
            var providerBuilder = factory.CreateConnectionStringBuilder();

            providerBuilder.ConnectionString = entityBuilder.ProviderConnectionString;

            providerBuilder.Remove("User ID");
            providerBuilder.Add("User ID", login);
            providerBuilder.Add("Password", password);
            

            entityBuilder.ProviderConnectionString = providerBuilder.ToString();
            return entityBuilder.ToString();
        }
    }
}
