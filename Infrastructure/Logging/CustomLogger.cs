using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SlaytonNichols.Common.Infrastructure.Logging;

public sealed class CustomLogger : ILogger
{
    private readonly TextWriter _writer;
    private readonly string _categoryName;
    private readonly Func<CustomLoggerConfiguration> _getCurrentConfig;

    internal IExternalScopeProvider ScopeProvider { get; set; }

    public CustomLogger(TextWriter writer, string categoryName, IExternalScopeProvider scopeProvider, Func<CustomLoggerConfiguration> getCurrentConfig)
    {
        _writer = writer;
        _categoryName = categoryName;
        ScopeProvider = scopeProvider;
        _getCurrentConfig = getCurrentConfig;
    }

    public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        var message = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            level = logLevel.ToString(),
            EventId = eventId.Id,
            EventName = eventId.Name,
            Category = _categoryName,
            Exception = exception?.ToString(),
            Message = formatter(state, exception),
        };

        AppendScope(message.Scope, state);
        AppendScope(message.Scope);

        _writer.WriteLine(JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = false }));
    }
    private void AppendScope(IDictionary<string, object> dictionary)
    {
        ScopeProvider.ForEachScope((scope, state) => AppendScope(state, scope), dictionary);
    }

    private static void AppendScope(IDictionary<string, object> dictionary, object scope)
    {
        if (scope == null)
            return;

        if (scope is IReadOnlyList<KeyValuePair<string, object>> formattedLogValues)
        {
            if (formattedLogValues.Count > 0)
            {
                foreach (var value in formattedLogValues)
                {
                    if (value.Value is MethodInfo)
                        continue;

                    dictionary[value.Key] = value.Value;
                }
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        private NullScope() { }
        public void Dispose() { }
    }
}