using System.Diagnostics.Metrics;

namespace DirectoryService.Infrastructure.Observability;

public sealed class DirectoryServiceMetrics
{
    public const string MeterName = "DirectoryService";

    private readonly Counter<long> _purgeRuns;
    private readonly Histogram<double> _purgeDuration;
    private readonly Counter<long> _purgedRecords;
    private long _lastSuccessfulPurgeUnixTimeSeconds;

    public DirectoryServiceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _purgeRuns = meter.CreateCounter<long>(
            "directory_service.soft_delete_purge.runs",
            description: "Number of completed soft-delete purge runs.");

        _purgeDuration = meter.CreateHistogram<double>(
            "directory_service.soft_delete_purge.duration",
            unit: "s",
            description: "Duration of a soft-delete purge run.");

        _purgedRecords = meter.CreateCounter<long>(
            "directory_service.soft_delete_purge.deleted_records",
            description: "Number of records permanently removed by the soft-delete purge.");

        meter.CreateObservableGauge(
            "directory_service.soft_delete_purge.last_success_time",
            () => Interlocked.Read(ref _lastSuccessfulPurgeUnixTimeSeconds),
            unit: "s",
            description: "Unix timestamp of the last successful soft-delete purge run.");
    }

    public void RecordPurgeRun(TimeSpan duration, bool isSuccess)
    {
        var status = isSuccess ? "success" : "error";
        var statusTag = new KeyValuePair<string, object?>("status", status);

        _purgeRuns.Add(1, statusTag);
        _purgeDuration.Record(duration.TotalSeconds, statusTag);

        if (isSuccess)
        {
            Interlocked.Exchange(
                ref _lastSuccessfulPurgeUnixTimeSeconds,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
    }

    public void RecordPurgedRecords(string entityType, int count)
    {
        if (count <= 0)
        {
            return;
        }

        _purgedRecords.Add(
            count,
            new KeyValuePair<string, object?>("entity.type", entityType));
    }
}
