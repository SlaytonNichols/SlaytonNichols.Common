using System;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;

namespace SlaytonNichols.Common.Infrastructure.Adls
{
    public interface IAdlsClient
    {
        DataLakeServiceClient GetDataLakeServiceClient(String clientID, string clientSecret, string tenantID);
    }
}