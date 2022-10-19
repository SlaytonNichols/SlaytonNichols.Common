using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Files.DataLake;

namespace SlaytonNichols.Common.Infrastructure.Adls
{
    public class AdlsClient : IAdlsClient
    {
        public AdlsClient()
        {
        }

        public DataLakeServiceClient GetDataLakeServiceClient(String clientID, string clientSecret, string tenantID)
        {

            TokenCredential credential = new ClientSecretCredential(
                tenantID, clientID, clientSecret, new TokenCredentialOptions());

            string dfsUri = "https://snadls.dfs.core.windows.net";

            return new DataLakeServiceClient(new Uri(dfsUri), credential);
        }
    }
}