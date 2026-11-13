// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

internal enum PromptToTaskUntrainedHonestyExecutionStatus
{
    Measured,
    Cancelled,
    ConcurrencyLimited,
}

internal sealed record PromptToTaskUntrainedHonestyExecutionResult(
    PromptToTaskUntrainedHonestyExecutionStatus Status,
    DynamicGraphTargetUntrainedHonestyMeasurement? Measurement,
    int DirectUnitInvocationCount);

internal interface IPromptToTaskUntrainedHonestyExecutionInvoker
{
    Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
        ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse> resolvedProducer,
        LogicalUnitRequest<PromptToTaskRequest> request,
        CancellationToken cancellationToken);
}

internal sealed class PromptToTaskUntrainedHonestyExecutionService : IDisposable
{
    internal const string PromptProvenance = "live-local:l2-untrained-honesty:v1";
    internal const string InvalidRequestCode = "PROMPT_TO_TASK_UNTRAINED_HONESTY_REQUEST_INVALID";
    internal const string ResultRequiredCode = "PROMPT_TO_TASK_UNTRAINED_HONESTY_RESULT_REQUIRED";
    internal const string ExecutionFailedCode = "PROMPT_TO_TASK_UNTRAINED_HONESTY_EXECUTION_FAILED";

    private const int MaximumIdentifierLength = 128;
    private const int MaximumLanguageLength = 32;
    private const int MaximumPromptLength = 1024 * 1024;
    private const string InvalidRequestMessage = "Prompt-to-task untrained honesty request is invalid.";
    private const string ResultRequiredMessage = "Prompt-to-task untrained honesty result is required.";
    private const string ExecutionFailedMessage = "Prompt-to-task untrained honesty execution failed safely.";

    private static readonly IReadOnlyList<Type> SupportingBoundaryTypes =
        Array.AsReadOnly(new[] { typeof(PromptToTaskMachine) });

    private readonly InMemoryLogicalUnitRegistry _registry;
    private readonly ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
        _resolvedProducer;
    private readonly IPromptToTaskUntrainedHonestyExecutionInvoker _invoker;
    private readonly SemaphoreSlim _executionGate = new(1, 1);
    private int _disposed;

    public PromptToTaskUntrainedHonestyExecutionService()
        : this(new ResolvedProducerInvoker())
    {
    }

    internal PromptToTaskUntrainedHonestyExecutionService(
        IPromptToTaskUntrainedHonestyExecutionInvoker invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        _invoker = invoker;
        _registry = new InMemoryLogicalUnitRegistry();
        var unit = new PromptToTaskIntentUnit(new PromptToTaskMachine());
        var registration = _registry.Register<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.UnitId,
            PromptToTaskIntentUnit.Role,
            PromptToTaskMachine.DomainTag,
            PromptToTaskMachine.Version,
            LogicalUnitTrustState.Candidate,
            isLearned: false,
            unit);
        if (registration is null
            || !registration.IsSuccess
            || registration.Value is null)
        {
            throw new InvalidOperationException(
                "The prompt-to-task untrained honesty producer could not be registered.");
        }

        var resolved = _registry.ResolveLatestById<PromptToTaskRequest, PromptToTaskResponse>(
            PromptToTaskIntentUnit.UnitId);
        if (resolved is null
            || !resolved.IsSuccess
            || resolved.Value is null
            || !IsExpectedDescriptor(resolved.Value.Descriptor))
        {
            throw new InvalidOperationException(
                "The prompt-to-task untrained honesty producer could not be resolved.");
        }

        _resolvedProducer = resolved.Value;
    }

    internal async Task<OperationResult<PromptToTaskUntrainedHonestyExecutionResult>> ExecuteAsync(
        string evaluationId,
        string runId,
        string promptId,
        string rawPrompt,
        string userLanguage,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (!IsSafeIdentifier(evaluationId, MaximumIdentifierLength)
            || !IsSafeIdentifier(runId, MaximumIdentifierLength)
            || !IsSafeIdentifier(promptId, MaximumIdentifierLength)
            || !IsSafeIdentifier(userLanguage, MaximumLanguageLength)
            || !IsSafePrompt(rawPrompt))
        {
            return OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Fail(
                InvalidRequestMessage,
                InvalidRequestCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled(directUnitInvocationCount: 0);

        bool acquired;
        try
        {
            acquired = await _executionGate
                .WaitAsync(0, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(directUnitInvocationCount: 0);
        }

        if (!acquired)
        {
            return OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Ok(
                new PromptToTaskUntrainedHonestyExecutionResult(
                    PromptToTaskUntrainedHonestyExecutionStatus.ConcurrencyLimited,
                    Measurement: null,
                    DirectUnitInvocationCount: 0));
        }

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled(directUnitInvocationCount: 0);

            var promptRequest = new PromptToTaskRequest(
                promptId,
                rawPrompt,
                ComputePromptSha256(rawPrompt),
                PromptProvenance,
                userLanguage,
                Array.AsReadOnly(new string[0]),
                new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>(StringComparer.Ordinal)));
            var logicalRequest = new LogicalUnitRequest<PromptToTaskRequest>(
                runId,
                _resolvedProducer.Descriptor.UnitId,
                _resolvedProducer.Descriptor.UnitRole,
                _resolvedProducer.Descriptor.DomainTag,
                promptRequest,
                CreateNonLearningFlags(),
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));

            const int directUnitInvocationCount = 1;
            Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>>? invocationTask;
            try
            {
                invocationTask = _invoker.InvokeAsync(
                    _resolvedProducer,
                    logicalRequest,
                    cancellationToken);
            }
            catch (Exception exception) when (
                SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ExecutionFailed();
            }

            if (invocationTask is null)
                return ResultRequired();

            OperationResult<LogicalUnitResult<PromptToTaskResponse>> execution;
            try
            {
                execution = await invocationTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled(directUnitInvocationCount);
            }
            catch (Exception exception) when (
                SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ExecutionFailed();
            }

            if (execution is null
                || (execution.IsSuccess && execution.Value is null))
            {
                return ResultRequired();
            }

            if (!execution.IsSuccess
                && StringComparer.Ordinal.Equals(execution.ErrorCode, "CANCELLED")
                && cancellationToken.IsCancellationRequested)
            {
                return Cancelled(directUnitInvocationCount);
            }

            if (!execution.IsSuccess)
                return execution.MapError<PromptToTaskUntrainedHonestyExecutionResult>();

            OperationResult<DynamicGraphUntrainedHonestyL2ProbeResult> measured;
            try
            {
                measured = DynamicGraphUntrainedHonestyL2Probe.MeasureAlreadyExecuted(
                    evaluationId,
                    _resolvedProducer,
                    logicalRequest,
                    execution.Value,
                    RuntimeReportedNotReady,
                    SupportingBoundaryTypes);
            }
            catch (Exception exception) when (
                SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ExecutionFailed();
            }

            if (measured is null
                || (measured.IsSuccess && measured.Value is null))
            {
                return ResultRequired();
            }

            if (!measured.IsSuccess)
                return measured.MapError<PromptToTaskUntrainedHonestyExecutionResult>();

            OperationResult<DynamicGraphTargetUntrainedHonestyMeasurement> bound;
            try
            {
                bound = DynamicGraphMeasurementCoordinator.BindL2UntrainedHonesty(
                    new DynamicGraphMeasurementTarget(
                        _resolvedProducer.Descriptor.UnitId,
                        DynamicGraphMeasurementTargetLevel.L2LogicalUnit,
                        DynamicGraphMeasurementMechanismKind.LearningSubject,
                        DeterministicFinalAuthority: false),
                    measured.Value);
            }
            catch (Exception exception) when (
                SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ExecutionFailed();
            }

            if (bound is null
                || (bound.IsSuccess && bound.Value is null))
            {
                return ResultRequired();
            }

            if (!bound.IsSuccess)
                return bound.MapError<PromptToTaskUntrainedHonestyExecutionResult>();

            return OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Ok(
                new PromptToTaskUntrainedHonestyExecutionResult(
                    PromptToTaskUntrainedHonestyExecutionStatus.Measured,
                    bound.Value,
                    directUnitInvocationCount));
        }
        finally
        {
            _executionGate.Release();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _executionGate.Dispose();
    }

    private static bool RuntimeReportedNotReady(
        LogicalUnitResult<PromptToTaskResponse> result)
        => result.Output is not null
           && StringComparer.Ordinal.Equals(
               result.Output.ContractStatus,
               PromptToTaskMachine.NotTrainedStatus)
           && result.Output.StructuredTask is not null
           && StringComparer.Ordinal.Equals(
               result.Output.StructuredTask.TaskType,
               PromptToTaskMachine.NotReadyTaskType);

    private static LogicalExecutionFlags CreateNonLearningFlags()
        => new()
        {
            UseCompiledPredicateRepresentation = false,
            UseDomainScopedRetrieval = false,
            UseLowConfidenceLessonSignal = false,
            UseLearnedUnitWhenAvailable = false,
            UseTraceEvidence = false,
            UseContextCarryForward = false,
            UseIdleLearningScheduling = false,
            UseE35DependencyDetection = false,
            UseSkillRetriever = false,
            UseReplayFusionRanking = false,
            UseArbiterAuthority = false,
            UseKnowledgeEdgeDetection = false,
            UseInternalFirstExecution = false,
            RequireExternalAiApproval = false,
            RejectArbiterContradictions = false,
            UseDeterministicFixtureBaseline = false,
            WriteEvidenceArtifacts = false,
            CompareAgainstOracle = false,
            FailOnMetricDrift = false,
        };

    private static string ComputePromptSha256(string rawPrompt)
        => $"sha256:{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawPrompt)))}";

    private static bool IsExpectedDescriptor(LogicalUnitDescriptor descriptor)
        => StringComparer.Ordinal.Equals(descriptor.UnitId, PromptToTaskIntentUnit.UnitId)
           && StringComparer.Ordinal.Equals(descriptor.UnitRole, PromptToTaskIntentUnit.Role)
           && StringComparer.Ordinal.Equals(descriptor.DomainTag, PromptToTaskMachine.DomainTag)
           && StringComparer.Ordinal.Equals(descriptor.Version, PromptToTaskMachine.Version)
           && descriptor.TrustState == LogicalUnitTrustState.Candidate
           && !descriptor.IsLearned
           && descriptor.InputType == typeof(PromptToTaskRequest)
           && descriptor.OutputType == typeof(PromptToTaskResponse)
           && descriptor.RegistrationOrder == 1;

    private static bool IsSafeIdentifier(string? value, int maximumLength)
    {
        if (value is null || value.Length is < 1 || value.Length > maximumLength)
            return false;

        foreach (var character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character)
                && character is not '.' and not '_' and not ':' and not '-')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSafePrompt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumPromptLength)
            return false;

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is '\r' or '\n' or '\t')
                continue;
            if (char.IsControl(character) || character is '\u0085' or '\u2028' or '\u2029')
                return false;
            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;
                index++;
                continue;
            }
            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static OperationResult<PromptToTaskUntrainedHonestyExecutionResult> Cancelled(
        int directUnitInvocationCount)
        => OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Ok(
            new PromptToTaskUntrainedHonestyExecutionResult(
                PromptToTaskUntrainedHonestyExecutionStatus.Cancelled,
                Measurement: null,
                directUnitInvocationCount));

    private static OperationResult<PromptToTaskUntrainedHonestyExecutionResult> ResultRequired()
        => OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Fail(
            ResultRequiredMessage,
            ResultRequiredCode);

    private static OperationResult<PromptToTaskUntrainedHonestyExecutionResult> ExecutionFailed()
        => OperationResult<PromptToTaskUntrainedHonestyExecutionResult>.Fail(
            ExecutionFailedMessage,
            ExecutionFailedCode);

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(nameof(PromptToTaskUntrainedHonestyExecutionService));
    }

    private sealed class ResolvedProducerInvoker
        : IPromptToTaskUntrainedHonestyExecutionInvoker
    {
        public Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> InvokeAsync(
            ResolvedLogicalUnitRegistryEntry<PromptToTaskRequest, PromptToTaskResponse>
                resolvedProducer,
            LogicalUnitRequest<PromptToTaskRequest> request,
            CancellationToken cancellationToken)
            => resolvedProducer.Unit.ExecuteAsync(request, cancellationToken);
    }
}
