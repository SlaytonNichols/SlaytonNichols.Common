using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SlaytonNichols.Common.Infrastructure.Logging;

[ProviderAlias("CustomLogger")]
public sealed class CustomLoggerProvider : ILoggerProvider
{
    private readonly IDisposable _onChangeToken;
    private CustomLoggerConfiguration _currentConfig;
    private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new ConcurrentDictionary<string, CustomLogger>(StringComparer.Ordinal);


    public CustomLoggerProvider(IOptionsMonitor<CustomLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, category => new CustomLogger(Console.Out, category, _scopeProvider, GetCurrentConfig));
    }

    private CustomLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }
}