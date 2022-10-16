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

        public async Task WriteJsonToAdls(string path, string json)
        {
            var serviceClient = GetDataLakeServiceClient(Environment.GetEnvironmentVariable("CLIENTID"),
                                                 Environment.GetEnvironmentVariable("CLIENT_SECRET"),
                                                 Environment.GetEnvironmentVariable("TENANTID"));
            var databricks = serviceClient.GetFileSystemClient("databricks");
            var dir = databricks.GetDirectoryClient(path);
            if (!dir.Exists())
                await dir.CreateAsync();

            var fileName = @$"{DateTime.Now:yyyy-MM-dd-HH-mm}.json";
            File.WriteAllText(fileName, json);
            var file = dir.CreateFile(fileName);
            var fileClient = dir.GetFileClient(fileName);
            FileStream fileStream = File.OpenRead(fileName);

            long fileSize = fileStream.Length;

            await fileClient.AppendAsync(fileStream, offset: 0);

            await fileClient.FlushAsync(position: fileSize);
            File.Delete(fileName);
        }
    }
}