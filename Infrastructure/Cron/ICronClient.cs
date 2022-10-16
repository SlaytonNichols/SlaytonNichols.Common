using System;
using System.Threading;
using System.Threading.Tasks;

namespace SlaytonNichols.Common.Infrastructure.Cron
{
    public interface ICronClient
    {
        Task<CronInfo> AwaitNext(CancellationToken stoppingToken);
    }
}