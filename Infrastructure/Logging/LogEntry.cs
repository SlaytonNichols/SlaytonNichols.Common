internal sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; set; }
    public string level { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string Category { get; set; }
    public string Exception { get; set; }
    public string Message { get; set; }
    // public string TraceId { get; set; }
    // public string SpanId { get; set; }
    public IDictionary<string, object> Scope { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}