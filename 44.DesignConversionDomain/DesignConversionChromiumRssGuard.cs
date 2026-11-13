// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

internal sealed record LocalChromiumBrowserRssPolicy
{
    internal const long DefaultMaxBrowserProcessRssBytes = 1L * 1024 * 1024 * 1024;
    private static readonly TimeSpan DefaultPollingIntervalValue = TimeSpan.FromMilliseconds(100);

    public LocalChromiumBrowserRssPolicy(
        long maxBrowserProcessRssBytes,
        TimeSpan pollingInterval)
    {
        if (maxBrowserProcessRssBytes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maxBrowserProcessRssBytes),
                "Browser process RSS ceiling must be positive.");
        if (pollingInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(pollingInterval),
                "Browser process RSS polling interval must be positive.");

        MaxBrowserProcessRssBytes = maxBrowserProcessRssBytes;
        PollingInterval = pollingInterval;
    }

    public long MaxBrowserProcessRssBytes { get; }

    public TimeSpan PollingInterval { get; }

    public static LocalChromiumBrowserRssPolicy Default { get; } =
        new(DefaultMaxBrowserProcessRssBytes, DefaultPollingIntervalValue);

    public static LocalChromiumBrowserRssPolicy FromMaxBytes(long? maxBrowserProcessRssBytes)
        => maxBrowserProcessRssBytes is null
            ? Default
            : new LocalChromiumBrowserRssPolicy(maxBrowserProcessRssBytes.Value, DefaultPollingIntervalValue);
}

internal sealed class LocalChromiumBrowserRssGuard
{
    internal const string LimitExceededCode = "BROWSER_PROCESS_RSS_LIMIT_EXCEEDED";
    internal const string InspectionFailedCode = "BROWSER_PROCESS_RSS_INSPECTION_FAILED";
    internal const string LimitExceededMessage = "Browser process exceeded the configured memory ceiling.";
    internal const string InspectionFailedMessage = "Browser process memory could not be inspected safely.";

    private readonly LocalChromiumBrowserRssPolicy _policy;
    private readonly Func<long> _sampleRss;
    private readonly Func<bool> _hasExited;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly Action _killProcessTree;
    private readonly object _sync = new();
    private OperationResult<bool>? _failure;
    private int _killInvoked;

    public LocalChromiumBrowserRssGuard(
        LocalChromiumBrowserRssPolicy policy,
        Func<long> sampleRss,
        Func<bool> hasExited,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        Action killProcessTree)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _sampleRss = sampleRss ?? throw new ArgumentNullException(nameof(sampleRss));
        _hasExited = hasExited ?? throw new ArgumentNullException(nameof(hasExited));
        _delayAsync = delayAsync ?? throw new ArgumentNullException(nameof(delayAsync));
        _killProcessTree = killProcessTree ?? throw new ArgumentNullException(nameof(killProcessTree));
    }

    public LocalChromiumBrowserRssMonitor Start(CancellationToken cancellationToken = default)
    {
        var exited = TryHasExited();
        if (!exited.IsSuccess)
            return LocalChromiumBrowserRssMonitor.Completed(RecordFailure(exited));
        if (exited.Value || cancellationToken.IsCancellationRequested)
            return LocalChromiumBrowserRssMonitor.Completed(CurrentFailure);

        var initial = InspectAndRecordFailure(cancellationToken);
        if (initial is not null || cancellationToken.IsCancellationRequested)
            return LocalChromiumBrowserRssMonitor.Completed(initial);

        return LocalChromiumBrowserRssMonitor.Running(RunAsync, cancellationToken);
    }

    private async Task<OperationResult<bool>?> RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _delayAsync(_policy.PollingInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return CurrentFailure;
            }

            if (cancellationToken.IsCancellationRequested)
                return CurrentFailure;

            var failure = InspectAndRecordFailure(cancellationToken);
            if (failure is not null)
                return failure;
        }

        return CurrentFailure;
    }

    private OperationResult<bool>? InspectAndRecordFailure(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return CurrentFailure;

        var exited = TryHasExited();
        if (!exited.IsSuccess)
            return RecordFailure(InspectionFailure());
        if (exited.Value == true)
            return CurrentFailure;

        long rssBytes;
        try
        {
            rssBytes = _sampleRss();
        }
        catch (Exception)
        {
            var active = TryHasExited();
            return active is { IsSuccess: true, Value: true }
                ? CurrentFailure
                : RecordFailure(InspectionFailure());
        }

        return rssBytes > _policy.MaxBrowserProcessRssBytes
            ? RecordFailure(LimitExceeded())
            : CurrentFailure;
    }

    private OperationResult<bool> TryHasExited()
    {
        try
        {
            return OperationResult<bool>.Ok(_hasExited());
        }
        catch (Exception)
        {
            return InspectionFailure();
        }
    }

    private OperationResult<bool>? RecordFailure(OperationResult<bool> failure)
    {
        lock (_sync)
        {
            _failure ??= failure;
        }

        if (Interlocked.Exchange(ref _killInvoked, 1) == 0)
        {
            try
            {
                _killProcessTree();
            }
            catch (Exception)
            {
            }
        }

        return CurrentFailure;
    }

    private OperationResult<bool>? CurrentFailure
    {
        get
        {
            lock (_sync)
                return _failure;
        }
    }

    private static OperationResult<bool> LimitExceeded()
        => OperationResult<bool>.Fail(LimitExceededMessage, LimitExceededCode);

    private static OperationResult<bool> InspectionFailure()
        => OperationResult<bool>.Fail(InspectionFailedMessage, InspectionFailedCode);
}

internal sealed class LocalChromiumBrowserRssMonitor
{
    private readonly CancellationTokenSource? _stopSource;
    private readonly Task<OperationResult<bool>?> _completion;

    private LocalChromiumBrowserRssMonitor(
        Task<OperationResult<bool>?> completion,
        CancellationTokenSource? stopSource)
    {
        _completion = completion ?? throw new ArgumentNullException(nameof(completion));
        _stopSource = stopSource;
    }

    public Task<OperationResult<bool>?> Completion => _completion;

    public static LocalChromiumBrowserRssMonitor Completed(OperationResult<bool>? outcome)
        => new(Task.FromResult(outcome), stopSource: null);

    public static LocalChromiumBrowserRssMonitor Running(
        Func<CancellationToken, Task<OperationResult<bool>?>> monitorAsync,
        CancellationToken outerCancellationToken)
    {
        var stopSource = CancellationTokenSource.CreateLinkedTokenSource(outerCancellationToken);
        return new LocalChromiumBrowserRssMonitor(monitorAsync(stopSource.Token), stopSource);
    }

    public async Task<OperationResult<bool>?> StopAndDrainAsync()
    {
        try
        {
            _stopSource?.Cancel();
            return await _completion.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
