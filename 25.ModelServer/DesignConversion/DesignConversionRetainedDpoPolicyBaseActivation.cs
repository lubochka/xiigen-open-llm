// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Runtime.CompilerServices;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.Extensions.Hosting;

internal sealed class LifecycleCleanupArbiter
{
    private const int MaximumCleanupFailureCount = 16;
    private const string RetirementFailureDataKey =
        "CONTINUOUS_LEARNING_RETIREMENT_FAILURE";
    private readonly Exception?[] _cleanup =
        new Exception?[MaximumCleanupFailureCount];
    private int _cleanupCount;
    private Exception? _original;
    private Exception? _firstCleanup;
    private Exception? _firstFatalCleanup;

    internal void CaptureOriginal(Exception exception) => _original ??= exception;
    internal bool OriginalIsFatal => _original is not null &&
        IsFatalConservatively(_original);
    internal bool HasCleanupFailure => _firstCleanup is not null;
    internal void CaptureCleanup(Exception exception)
    {
        _firstCleanup ??= exception;
        if (_firstFatalCleanup is null && IsFatalConservatively(exception))
            _firstFatalCleanup = exception;
        for (var index = 0; index < _cleanupCount; index++)
        {
            if (ReferenceEquals(_cleanup[index], exception))
                return;
        }
        if (_cleanupCount < _cleanup.Length)
            _cleanup[_cleanupCount++] = exception;
    }

    internal void ThrowIfAny()
    {
        var originalFatal = _original is not null &&
            IsFatalConservatively(_original);
        var winner = originalFatal
            ? _original
            : _firstFatalCleanup ?? _original ?? _firstCleanup;
        if (winner is null)
            return;
        try
        {
            var later = new List<Exception>();
            var seen = new HashSet<Exception>(
                ReferenceEqualityComparer.Instance) { winner };
            AddExistingAttachment(winner, later, seen);
            AddFlattened(_original, later, seen);
            for (var index = 0; index < _cleanupCount; index++)
                AddFlattened(_cleanup[index], later, seen);
            if (later.Count > 0)
                winner.Data[RetirementFailureDataKey] =
                    later.Count == 1 ? later[0] : new AggregateException(later);
        }
        catch { }
        throw winner;
    }

    private static bool IsFatalConservatively(Exception exception)
    {
        try
        {
            return !SnapshotExceptionGraphClassifier.IsNonFatal(exception);
        }
        catch
        {
            return true;
        }
    }

    private static void AddExistingAttachment(
        Exception exception,
        List<Exception> attached,
        HashSet<Exception> seen)
    {
        try
        {
            if (exception.Data[RetirementFailureDataKey] is Exception existing)
                AddFlattened(existing, attached, seen);
        }
        catch { }
    }

    private static void AddFlattened(
        Exception? exception,
        List<Exception> attached,
        HashSet<Exception> seen)
    {
        if (exception is null || !seen.Add(exception))
            return;
        if (exception is AggregateException aggregate)
        {
            foreach (var inner in aggregate.InnerExceptions)
                AddFlattened(inner, attached, seen);
            AddExistingAttachment(aggregate, attached, seen);
            return;
        }
        attached.Add(exception);
        AddExistingAttachment(exception, attached, seen);
    }
}

/// <summary>
/// Sanitized outcome of resolving the optional retained DPO policy base.
/// A successful outcome may have no lease when the feature is not configured.
/// </summary>
public sealed class DesignConversionRetainedDpoPolicyBaseActivationResolution
{
    internal DesignConversionRetainedDpoPolicyBaseActivationResolution(
        bool isSuccess,
        string status,
        string errorCode,
        string errorMessage,
        DesignConversionRetainedDpoPolicyBaseActivationLease? lease)
    {
        IsSuccess = isSuccess;
        Status = status;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Lease = lease;
    }

    public bool IsSuccess { get; }
    public bool IsActivated => Lease is not null;
    public string Status { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public DesignConversionRetainedDpoPolicyBaseActivationLease? Lease { get; }
    public ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        LegalTeacherAdmissionAuthority => Lease?.LegalTeacherAdmissionAuthority;
}

/// <summary>
/// Server-lifetime ownership of the exact retained provider binding.
/// </summary>
public sealed class DesignConversionRetainedDpoPolicyBaseActivationLease
    : IDisposable, IAsyncDisposable
{
    private readonly ContinuousLearningRetainedObjectHostedActivator _activator;
    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly IDisposable _legalTeacherAdmissionAssociation;
    private readonly IDisposable _dpoTeacherAttestationAssociation;
    private readonly DesignConversionDpoTeacherAttestationRuntime
        _dpoTeacherAttestationRuntime;
    private readonly Func<Task>? _beforeStopForTesting;
    private readonly object _disposalGate = new();
    private Task? _disposalTask;

    internal DesignConversionRetainedDpoPolicyBaseActivationLease(
        ContinuousLearningRetainedObjectHostedActivator activator,
        ServerOwnedContinuousLearningFileAccess fileAccess,
        DesignConversionRetainedDpoPolicyBaseProviderGateway gateway,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority,
        DesignConversionDpoTeacherAttestationRuntime
            dpoTeacherAttestationRuntime,
        Func<Task>? beforeStopForTesting = null)
    {
        _activator = activator;
        _fileAccess = fileAccess;
        _beforeStopForTesting = beforeStopForTesting;
        Provider = gateway;
        LegalTeacherAdmissionAuthority = legalTeacherAdmissionAuthority
            ?? throw new ArgumentNullException(
                nameof(legalTeacherAdmissionAuthority));
        _dpoTeacherAttestationRuntime = dpoTeacherAttestationRuntime ??
            throw new ArgumentNullException(
                nameof(dpoTeacherAttestationRuntime));
        var legalTeacherAdmissionAssociation =
            DesignConversionRetainedDpoPolicyBaseActivation
                .AssociateLegalTeacherAdmissionAuthority(
                    Provider,
                    LegalTeacherAdmissionAuthority);
        try
        {
            _dpoTeacherAttestationAssociation =
                DesignConversionDpoTeacherAttestationAssociation.Associate(
                    Provider,
                    _dpoTeacherAttestationRuntime);
            _legalTeacherAdmissionAssociation =
                legalTeacherAdmissionAssociation;
        }
        catch (Exception original)
        {
            var failures = new LifecycleCleanupArbiter();
            failures.CaptureOriginal(original);
            try { legalTeacherAdmissionAssociation.Dispose(); }
            catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
            failures.ThrowIfAny();
            throw;
        }
    }

    public IDesignConversionDpoPolicyContinuationProvider Provider { get; }
    public ServerOwnedLegalTrainingTeacherAdmissionAuthority
        LegalTeacherAdmissionAuthority { get; }

    public void Dispose() => GetOrStartDisposal().GetAwaiter().GetResult();

    public ValueTask DisposeAsync() => new(GetOrStartDisposal());

    private Task GetOrStartDisposal()
    {
        lock (_disposalGate)
        {
            if (_disposalTask is null
                || (_disposalTask.IsCompleted
                    && !_disposalTask.IsCompletedSuccessfully))
            {
                _disposalTask = DisposeCoreAsync();
            }
            return _disposalTask;
        }
    }

    private async Task DisposeCoreAsync()
    {
        var failures = new LifecycleCleanupArbiter();
        try
        {
            if (_beforeStopForTesting is not null)
                await _beforeStopForTesting().ConfigureAwait(false);
        }
        catch (Exception exception) { failures.CaptureOriginal(exception); }
        var stopSucceeded = false;
        try
        {
            await _activator.StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
            stopSucceeded = true;
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        if (!stopSucceeded)
        {
            failures.ThrowIfAny();
            return;
        }
        try { _dpoTeacherAttestationAssociation.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { _dpoTeacherAttestationRuntime.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { _legalTeacherAdmissionAssociation.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { _fileAccess.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        failures.ThrowIfAny();
    }
}

internal sealed class DesignConversionRetainedDpoPolicyBaseActivationTestHooks
{
    internal ServerOwnedContinuousLearningFileAccessOptions? FileAccessOptions
    {
        get;
        init;
    }

    internal Action<DesignConversionRetainedDpoPolicyBaseProviderGateway>?
        GatewayCreated { get; init; }

    internal Action<DesignConversionRetainedDpoPolicyBaseProviderGateway>?
        AfterStartBeforeResolution { get; init; }

    internal Action? AfterGatewayBindingBeforeReadinessPromotion { get; init; }

    internal Func<Task>? BeforeLeaseStopAsync { get; init; }
}

/// <summary>
/// ModelServer-owned activation boundary for the optional retained DPO policy
/// base. Activation retains, hashes, and binds the checkpoint but never loads or
/// executes a model.
/// </summary>
public static class DesignConversionRetainedDpoPolicyBaseActivation
{
    private static readonly ConditionalWeakTable<
        IDesignConversionDpoPolicyBaseProvider,
        LegalTeacherAdmissionAuthorityRegistration>
        LegalTeacherAdmissionAuthorities = new();
    private static readonly object LegalTeacherAdmissionAssociationGate = new();

    public const string NotConfiguredStatus = "not_configured";
    public const string ActivatedStatus = "activated";
    public const string FailedStatus = "activation_failed";
    public const string NoErrorCode = "none";
    public const string NoErrorMessage = "none";
    public const string ActivationFailedErrorCode =
        "DESIGN_CONVERSION_RETAINED_DPO_POLICY_BASE_ACTIVATION_FAILED";
    public const string ActivationCancelledErrorCode =
        "DESIGN_CONVERSION_RETAINED_DPO_POLICY_BASE_ACTIVATION_CANCELLED";
    public const string ActivationFailedErrorMessage =
        "The retained DesignConversion DPO policy base could not be activated.";

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        ResolveLegalTeacherAdmissionAuthority(
            IDesignConversionDpoPolicyBaseProvider? provider)
    {
        if (provider is null)
            return null;

        lock (LegalTeacherAdmissionAssociationGate)
        {
            return LegalTeacherAdmissionAuthorities.TryGetValue(
                provider,
                out var registration)
                ? registration.Authority
                : null;
        }
    }

    internal static IDisposable AssociateLegalTeacherAdmissionAuthority(
        IDesignConversionDpoPolicyBaseProvider provider,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority authority)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(authority);
        var registration =
            new LegalTeacherAdmissionAuthorityRegistration(authority);
        lock (LegalTeacherAdmissionAssociationGate)
            LegalTeacherAdmissionAuthorities.Add(provider, registration);
        return new LegalTeacherAdmissionAssociationLease(
            provider,
            registration);
    }

    private static void RemoveLegalTeacherAdmissionAuthority(
        IDesignConversionDpoPolicyBaseProvider provider,
        LegalTeacherAdmissionAuthorityRegistration registration)
    {
        lock (LegalTeacherAdmissionAssociationGate)
        {
            if (LegalTeacherAdmissionAuthorities.TryGetValue(
                    provider,
                    out var current) &&
                ReferenceEquals(current, registration))
            {
                LegalTeacherAdmissionAuthorities.Remove(provider);
            }
        }
    }

    public static Task<DesignConversionRetainedDpoPolicyBaseActivationResolution>
        ActivateAsync(CancellationToken cancellationToken = default)
        => ActivateAsync(Environment.GetEnvironmentVariable, cancellationToken);

    internal static async Task<
        DesignConversionRetainedDpoPolicyBaseActivationResolution> ActivateAsync(
            Func<string, string?> readEnvironment,
            CancellationToken cancellationToken = default,
            DesignConversionRetainedDpoPolicyBaseActivationTestHooks? testHooks = null)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);

        var startup = ContinuousLearningModelServerStartup.Create(readEnvironment);
        var initialReadiness = startup.Readiness;
        if (!initialReadiness.ConfigurationAdmitted)
        {
            return string.Equals(
                initialReadiness.ConfigurationStatus,
                "disabled",
                StringComparison.Ordinal)
                ? NotConfigured()
                : Failed(initialReadiness);
        }
        if (!startup.LegalTeacherAdmissionAuthority.IsAvailable)
        {
            return Failed(
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode);
        }

        ServerOwnedContinuousLearningFileAccess fileAccess;
        try
        {
            fileAccess = startup.AcquireRetainedFileAccess(
                testHooks?.FileAccessOptions);
        }
        catch (Exception exception)
        {
            return await FailFileAccessAcquisitionAndRetireAsync(
                    startup,
                    exception)
                .ConfigureAwait(false);
        }

        var gateway = new DesignConversionRetainedDpoPolicyBaseProviderGateway();
        var activator = new ContinuousLearningRetainedObjectHostedActivator(
            fileAccess,
            gateway,
            startup);
        try
        {
            testHooks?.GatewayCreated?.Invoke(gateway);
            await activator.StartAsync(
                    cancellationToken,
                    testHooks?
                        .AfterGatewayBindingBeforeReadinessPromotion)
                .ConfigureAwait(false);
            testHooks?.AfterStartBeforeResolution?.Invoke(gateway);
            cancellationToken.ThrowIfCancellationRequested();
            var readiness = startup.Readiness;
            if (!readiness.StableDpoExecutorConfigured || !gateway.IsBound)
            {
                var failure = Failed(readiness);
                await StopAndDisposeAsync(activator, fileAccess).ConfigureAwait(false);
                return failure;
            }

            var teacherRuntime = CreateTeacherAttestationRuntime(
                startup,
                gateway);
            try
            {
                return Activated(
                    new DesignConversionRetainedDpoPolicyBaseActivationLease(
                        activator,
                        fileAccess,
                        gateway,
                        startup.LegalTeacherAdmissionAuthority,
                        teacherRuntime,
                        testHooks?.BeforeLeaseStopAsync));
            }
            catch (Exception original)
            {
                var failures = new LifecycleCleanupArbiter();
                failures.CaptureOriginal(original);
                try { teacherRuntime.Dispose(); }
                catch (Exception cleanup)
                {
                    failures.CaptureCleanup(cleanup);
                }
                failures.ThrowIfAny();
                throw;
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            var failure = Failed(ActivationCancelledErrorCode);
            await StopAndDisposeAsync(activator, fileAccess).ConfigureAwait(false);
            return failure;
        }
        catch (Exception exception)
        {
            return await FailAndDisposeAsync(
                    startup,
                    activator,
                    fileAccess,
                    exception)
                .ConfigureAwait(false);
        }
    }

    private static DesignConversionRetainedDpoPolicyBaseActivationResolution
        NotConfigured()
        => new(
            isSuccess: true,
            NotConfiguredStatus,
            NoErrorCode,
            NoErrorMessage,
            lease: null);

    private static DesignConversionRetainedDpoPolicyBaseActivationResolution
        Activated(DesignConversionRetainedDpoPolicyBaseActivationLease lease)
        => new(
            isSuccess: true,
            ActivatedStatus,
            NoErrorCode,
            NoErrorMessage,
            lease);

    private static DesignConversionRetainedDpoPolicyBaseActivationResolution Failed(
        ContinuousLearningProductionReadinessStatus readiness)
        => new(
            isSuccess: false,
            FailedStatus,
            SanitizeStableCode(readiness.ConfigurationErrorCode),
            ActivationFailedErrorMessage,
            lease: null);

    private static DesignConversionRetainedDpoPolicyBaseActivationResolution Failed(
        string errorCode)
        => new(
            isSuccess: false,
            FailedStatus,
            SanitizeStableCode(errorCode),
            ActivationFailedErrorMessage,
            lease: null);

    private static string SanitizeStableCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode)
            || string.Equals(errorCode, NoErrorCode, StringComparison.Ordinal)
            || errorCode.Length > 128
            || errorCode.Any(character =>
                character is not (>= 'A' and <= 'Z')
                    and not (>= '0' and <= '9')
                    and not '_'))
        {
            return ActivationFailedErrorCode;
        }

        return errorCode;
    }

    private static async Task StopAndDisposeAsync(
        ContinuousLearningRetainedObjectHostedActivator activator,
        ServerOwnedContinuousLearningFileAccess fileAccess,
        Exception? original = null)
    {
        var failures = new LifecycleCleanupArbiter();
        if (original is not null)
            failures.CaptureOriginal(original);
        try
        {
            await activator
                .StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { fileAccess.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        if (failures.HasCleanupFailure || failures.OriginalIsFatal)
            failures.ThrowIfAny();
    }

    private static async Task<
        DesignConversionRetainedDpoPolicyBaseActivationResolution>
        FailFileAccessAcquisitionAndRetireAsync(
            ContinuousLearningModelServerStartup startup,
            Exception original)
    {
        var failures = new LifecycleCleanupArbiter();
        failures.CaptureOriginal(original);
        try { startup.MarkRetainedFileAccessAcquisitionFailed(original); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try
        {
            await startup.AwaitRetirementAsync().ConfigureAwait(false);
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }

        if (failures.HasCleanupFailure || failures.OriginalIsFatal)
            failures.ThrowIfAny();
        return Failed(startup.Readiness);
    }

    private static async Task<
        DesignConversionRetainedDpoPolicyBaseActivationResolution>
        FailAndDisposeAsync(
            ContinuousLearningModelServerStartup startup,
            ContinuousLearningRetainedObjectHostedActivator activator,
            ServerOwnedContinuousLearningFileAccess fileAccess,
            Exception original)
    {
        var failures = new LifecycleCleanupArbiter();
        failures.CaptureOriginal(original);
        try
        {
            await activator
                .StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { fileAccess.Dispose(); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }

        var failure = Failed(startup.Readiness);
        if (failures.HasCleanupFailure || failures.OriginalIsFatal)
            failures.ThrowIfAny();
        return failure;
    }

    internal static DesignConversionDpoTeacherAttestationRuntime
        CreateTeacherAttestationRuntime(
            ContinuousLearningModelServerStartup startup,
            DesignConversionRetainedDpoPolicyBaseProviderGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(startup);
        ArgumentNullException.ThrowIfNull(gateway);
        var client = LocalQwenOllamaTeacherInferenceClient.Create(
            startup.TeacherConfiguration);
        if (!client.IsSuccess || client.Value is null)
            throw new InvalidOperationException(ActivationFailedErrorCode);

        return new DesignConversionDpoTeacherAttestationRuntime(
            client.Value,
            startup.TeacherConfiguration.ModelId,
            startup.TeacherConfiguration.ExpectedOllamaManifestDigest,
            startup.TeacherConfiguration.ExpectedOllamaModelLayerDigest,
            () => startup.IsLegalTrainingTeacherAdmissionActive() &&
                  gateway.IsBound);
    }

    private sealed record LegalTeacherAdmissionAuthorityRegistration(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority Authority);

    private sealed class LegalTeacherAdmissionAssociationLease : IDisposable
    {
        private LegalTeacherAdmissionAssociationState? _state;

        internal LegalTeacherAdmissionAssociationLease(
            IDesignConversionDpoPolicyBaseProvider provider,
            LegalTeacherAdmissionAuthorityRegistration registration)
        {
            _state = new LegalTeacherAdmissionAssociationState(
                provider,
                registration);
        }

        public void Dispose()
        {
            var state = Interlocked.Exchange(ref _state, null);
            if (state is not null)
            {
                RemoveLegalTeacherAdmissionAuthority(
                    state.Provider,
                    state.Registration);
            }
        }

        private sealed record LegalTeacherAdmissionAssociationState(
            IDesignConversionDpoPolicyBaseProvider Provider,
            LegalTeacherAdmissionAuthorityRegistration Registration);
    }
}

internal sealed class DesignConversionDpoTeacherAttestationHostedAssociation(
    ContinuousLearningModelServerStartup startup,
    DesignConversionRetainedDpoPolicyBaseProviderGateway gateway)
    : IHostedService, IDisposable
{
    private static readonly AsyncLocal<Action?>
        AfterRuntimeCreatedBeforeAssociationObserver = new();
    internal static Action?
        AfterRuntimeCreatedBeforeAssociationForTesting
    {
        get => AfterRuntimeCreatedBeforeAssociationObserver.Value;
        set => AfterRuntimeCreatedBeforeAssociationObserver.Value = value;
    }
    private readonly object _gate = new();
    private DesignConversionDpoTeacherAttestationRuntime? _runtime;
    private IDisposable? _association;
    private Task? _disposalTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_disposalTask is not null)
                return _disposalTask;
            if (_association is not null)
                return Task.CompletedTask;
            if (!gateway.IsBound ||
                !startup.Readiness.StableDpoExecutorConfigured)
                return Task.CompletedTask;

            DesignConversionDpoTeacherAttestationRuntime? runtime = null;
            IDisposable? association = null;
            try
            {
                runtime = DesignConversionRetainedDpoPolicyBaseActivation
                    .CreateTeacherAttestationRuntime(startup, gateway);
                AfterRuntimeCreatedBeforeAssociationObserver.Value?.Invoke();
                var teacherIssuer = startup.TrainingActivationRequested
                    ? startup.PrepareDesignConversionTeacherAssociationIssuer(
                        gateway) ?? throw new InvalidOperationException(
                        DesignConversionRetainedDpoPolicyBaseActivation
                            .ActivationFailedErrorCode)
                    : null;
                ContinuousLearningDesignConversionTeacherAssociationCapability?
                    teacherCapability = null;
                if (teacherIssuer is null)
                {
                    association =
                        DesignConversionDpoTeacherAttestationAssociation.Associate(
                            gateway,
                            runtime);
                }
                else
                {
                    var trainingAssociation =
                        DesignConversionDpoTeacherAttestationAssociation.Associate(
                            gateway,
                            runtime,
                            teacherIssuer);
                    association = trainingAssociation;
                    teacherCapability = trainingAssociation.Capability;
                }

                var finalized = teacherCapability is not null
                    && startup
                        .TryMarkDesignConversionTrainingIntakeTeacherRuntimeAssociated(
                            teacherCapability);
                if (startup.TrainingActivationRequested && !finalized)
                {
                    throw new InvalidOperationException(
                        DesignConversionRetainedDpoPolicyBaseActivation
                            .ActivationFailedErrorCode);
                }

                _association = association;
                association = null;
                _runtime = runtime;
                runtime = null;
            }
            catch (Exception original)
            {
                var failures = new LifecycleCleanupArbiter();
                failures.CaptureOriginal(original);
                if (startup.TrainingActivationRequested)
                {
                    ContinuousLearningDpoPolicyBaseWithdrawal withdrawal =
                        default;
                    try
                    {
                        withdrawal = startup
                            .TryBeginDesignConversionTrainingIntakeTeacherAssociationFailureWithdrawal(
                                gateway,
                                startup.LegalTeacherAdmissionAuthority);
                    }
                    catch (Exception cleanup)
                    {
                        failures.CaptureCleanup(cleanup);
                    }
                    try { gateway.Close(); }
                    catch (Exception cleanup)
                    {
                        failures.CaptureCleanup(cleanup);
                    }
                    if (withdrawal.Ticket is { } ticket)
                    {
                        try
                        {
                            if (!gateway.IsBound)
                            {
                                startup
                                    .CompleteDpoPolicyBaseBindingFailureWithdrawal(
                                        ticket);
                            }
                        }
                        catch (Exception cleanup)
                        {
                            failures.CaptureCleanup(cleanup);
                        }
                    }
                }
                try { association?.Dispose(); }
                catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
                try { runtime?.Dispose(); }
                catch (Exception cleanup) { failures.CaptureCleanup(cleanup); }
                failures.ThrowIfAny();
                throw;
            }
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => BeginDisposal();

    public void Dispose() => BeginDisposal().GetAwaiter().GetResult();

    private Task BeginDisposal()
    {
        lock (_gate)
        {
            if (_disposalTask is null
                || (_disposalTask.IsCompleted
                    && !_disposalTask.IsCompletedSuccessfully))
            {
                _disposalTask = DisposeCoreAsync();
            }
            return _disposalTask;
        }
    }

    private async Task DisposeCoreAsync()
    {
        await Task.Yield();
        IDisposable? association;
        DesignConversionDpoTeacherAttestationRuntime? runtime;
        lock (_gate)
        {
            association = _association;
            runtime = _runtime;
        }
        var failures = new LifecycleCleanupArbiter();
        var gatewayRetirementInitiated = false;
        try
        {
            gateway.Close();
            gatewayRetirementInitiated = true;
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        var startupRetirementInitiated = false;
        try
        {
            startup.MarkDpoPolicyBaseStopped();
            startupRetirementInitiated = true;
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        if (!gatewayRetirementInitiated || !startupRetirementInitiated)
        {
            failures.ThrowIfAny();
            return;
        }

        try { await startup.AwaitRetirementAsync().ConfigureAwait(false); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }
        try { await gateway.AwaitRetirementAsync().ConfigureAwait(false); }
        catch (Exception exception) { failures.CaptureCleanup(exception); }

        var associationDisposed = association is null;
        try
        {
            association?.Dispose();
            associationDisposed = true;
            if (association is not null)
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_association, association))
                        _association = null;
                }
            }
        }
        catch (Exception exception) { failures.CaptureCleanup(exception); }

        if (associationDisposed)
        {
            try
            {
                runtime?.Dispose();
                if (runtime is not null)
                {
                    lock (_gate)
                    {
                        if (ReferenceEquals(_runtime, runtime))
                            _runtime = null;
                    }
                }
            }
            catch (Exception exception) { failures.CaptureCleanup(exception); }
        }
        failures.ThrowIfAny();
    }
}
