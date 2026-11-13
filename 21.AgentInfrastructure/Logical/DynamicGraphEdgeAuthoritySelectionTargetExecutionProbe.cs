// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Executes one already-admitted L3 selected target. Implementations receive owned association
/// evidence and must return only the identity and evidence of that direct target invocation.
/// </summary>
public interface IDynamicGraphL3SelectedTargetExecutor
{
    Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>> ExecuteAsync(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Carries the result of one direct selected-target invocation. It does not assert aggregate
/// quality, graph mutation, training, DPO, checkpoint, promotion, or lifecycle state.
/// </summary>
public sealed record DynamicGraphL3SelectedTargetExecutionResult
{
    private readonly string[] _evidenceIds;

    public DynamicGraphL3SelectedTargetExecutionResult(
        string TargetId,
        string ExecutionId,
        IReadOnlyList<string> EvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(EvidenceIds);

        this.TargetId = TargetId;
        this.ExecutionId = ExecutionId;
        _evidenceIds = EvidenceIds.ToArray();
    }

    public string TargetId { get; }

    public string ExecutionId { get; }

    public IReadOnlyList<string> EvidenceIds => Array.AsReadOnly(_evidenceIds.ToArray());
}

/// <summary>
/// Couples independently owned admission evidence to one independently owned direct target
/// execution result. The invocation count is the fixed fact established by this one-shot probe,
/// not an aggregate execution, quality, training, DPO, promotion, or lifecycle verdict.
/// </summary>
public sealed record DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt
{
    private readonly DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        _association;
    private readonly DynamicGraphL3SelectedTargetExecutionResult _executionResult;
    private readonly DynamicGraphTargetRunSpeedMeasurement _runSpeed;

    public DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement Association,
        DynamicGraphL3SelectedTargetExecutionResult ExecutionResult,
        DynamicGraphTargetRunSpeedMeasurement RunSpeed)
    {
        ArgumentNullException.ThrowIfNull(Association);
        ArgumentNullException.ThrowIfNull(ExecutionResult);
        ArgumentNullException.ThrowIfNull(RunSpeed);

        _association = CopyAssociation(Association);
        _executionResult = CopyExecutionResult(ExecutionResult);
        _runSpeed = CopyRunSpeed(RunSpeed);
    }

    public DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement Association
        => CopyAssociation(_association);

    public DynamicGraphL3SelectedTargetExecutionResult ExecutionResult
        => CopyExecutionResult(_executionResult);

    public DynamicGraphTargetRunSpeedMeasurement RunSpeed
        => CopyRunSpeed(_runSpeed);

    public int DirectTargetInvocationCount => 1;

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        CopyAssociation(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement source)
        => new(source.Target, source.EdgeAuthoritySelectionQualityEvidence);

    private static DynamicGraphL3SelectedTargetExecutionResult CopyExecutionResult(
        DynamicGraphL3SelectedTargetExecutionResult source)
        => new(source.TargetId, source.ExecutionId, source.EvidenceIds);

    private static DynamicGraphTargetRunSpeedMeasurement CopyRunSpeed(
        DynamicGraphTargetRunSpeedMeasurement source)
    {
        var target = source.Target;
        var measurement = source.RunSpeed;
        var axis = measurement.RunSpeed;
        return new DynamicGraphTargetRunSpeedMeasurement(
            new DynamicGraphMeasurementTarget(
                target.TargetId,
                target.Level,
                target.MechanismKind,
                target.DeterministicFinalAuthority),
            new DynamicGraphRunSpeedMeasurement(
                measurement.EvaluatedRowCount,
                measurement.CandidateSampleCount,
                measurement.BatchSampleCount,
                new DynamicGraphRunSpeedAxis(
                    axis.InferenceElapsedMs,
                    axis.ThroughputRowsPerSec,
                    axis.RoutingElapsedMs,
                    axis.PerCandidateElapsedMs,
                    axis.PerBatchElapsedMs)));
    }
}

/// <summary>
/// One-shot boundary that admits exact edge-authority selection evidence before invoking the
/// selected L3 target exactly once. It neither retries nor mutates the graph and makes no
/// aggregate-quality, training, DPO, checkpoint, promotion, or lifecycle claim.
/// </summary>
public sealed class DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe
{
    private const string ExecutorRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTOR_REQUIRED";
    private const string ExecutorRequiredMessage =
        "Dynamic graph L3 selected-target executor is required.";
    private const string TimeProviderRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_TIME_PROVIDER_REQUIRED";
    private const string TimeProviderRequiredMessage =
        "Dynamic graph L3 selected-target time provider is required.";
    private const string AlreadyStartedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_ALREADY_STARTED";
    private const string AlreadyStartedMessage =
        "Dynamic graph L3 selected-target execution has already started.";
    private const string AssociationRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_REQUIRED";
    private const string AssociationRequiredMessage =
        "Dynamic graph L3 selected-target association is required.";
    private const string AssociationInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_INVALID";
    private const string AssociationInvalidMessage =
        "Dynamic graph L3 selected-target association is invalid.";
    private const string CancelledCode = "CANCELLED";
    private const string CancelledMessage = "Cancelled.";
    private const string ExecutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_FAILED";
    private const string ExecutionFailedMessage =
        "Dynamic graph L3 selected-target execution failed.";
    private const string ResultRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_RESULT_REQUIRED";
    private const string ResultRequiredMessage =
        "Dynamic graph L3 selected-target execution result is required.";
    private const string ResultInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_RESULT_INVALID";
    private const string ResultInvalidMessage =
        "Dynamic graph L3 selected-target execution result is invalid.";
    private const string TargetMismatchCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_EXECUTION_TARGET_MISMATCH";
    private const string TargetMismatchMessage =
        "Dynamic graph L3 selected-target execution result does not match the admitted target.";
    private const string RunSpeedTimingInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_RUN_SPEED_TIMING_INVALID";
    private const string RunSpeedTimingInvalidMessage =
        "Dynamic graph L3 selected-target run-speed timing is invalid.";

    private readonly IDynamicGraphL3SelectedTargetExecutor _executor;
    private readonly TimeProvider _timeProvider;
    private int _started;

    private DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe(
        IDynamicGraphL3SelectedTargetExecutor executor,
        TimeProvider timeProvider)
    {
        _executor = executor;
        _timeProvider = timeProvider;
    }

    public static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe> Create(
        IDynamicGraphL3SelectedTargetExecutor? executor)
        => Create(executor, TimeProvider.System);

    public static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe> Create(
        IDynamicGraphL3SelectedTargetExecutor? executor,
        TimeProvider? timeProvider)
    {
        if (executor is null)
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe>(
                ExecutorRequiredMessage,
                ExecutorRequiredCode);
        }

        if (timeProvider is null)
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe>(
                TimeProviderRequiredMessage,
                TimeProviderRequiredCode);
        }

        return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe>.Ok(
            new DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe(
                executor,
                timeProvider));
    }

    public async Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunAsync(
            DynamicGraphMeasurementTarget? target,
            EdgeAuthorityExecutionResult? authorityResult,
            DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
            CancellationToken cancellationToken = default)
    {
        if (!TryStart())
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                AlreadyStartedMessage,
                AlreadyStartedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var association = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                target,
                authorityResult,
                noveltyRequest);
        if (!association.IsSuccess)
        {
            return association.MapError<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var admittedAssociation = association.Value!;
        var executorAssociation = CopyAssociation(admittedAssociation);
        var receiptAssociation = CopyAssociation(admittedAssociation);

        return await ExecuteAdmittedCoreAsync(
                executorAssociation,
                receiptAssociation,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a trusted in-process, already-admitted carrier seam after structural validation.
    /// This overload cannot prove provenance, re-measure evidence, or claim aggregate quality,
    /// training, DPO, promotion, or lifecycle state. It is not an HTTP or client boundary.
    /// </summary>
    public async Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunAdmittedAsync(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association,
            CancellationToken cancellationToken = default)
    {
        if (!TryStart())
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                AlreadyStartedMessage,
                AlreadyStartedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (association is null)
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                AssociationRequiredMessage,
                AssociationRequiredCode);
        }

        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement executorAssociation;
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement receiptAssociation;
        try
        {
            var (target, authorityEdgeId) = association.SnapshotTargetAuthorityEdgeBinding();
            if (target.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow
                || target.MechanismKind
                    != DynamicGraphMeasurementMechanismKind.LearningSubject
                || target.DeterministicFinalAuthority
                || !IsCanonicalIdentity(target.TargetId)
                || !IsCanonicalIdentity(authorityEdgeId)
                || !StringComparer.Ordinal.Equals(target.TargetId, authorityEdgeId))
            {
                return AssociationInvalid();
            }

            // These are deliberately independent siblings. There is no cancellation check
            // between their construction and executor invocation, preserving the raw seam's
            // admitted-to-executor race semantics.
            executorAssociation = CopyAssociation(association);
            receiptAssociation = CopyAssociation(association);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AssociationInvalid();
        }

        return await ExecuteAdmittedCoreAsync(
                executorAssociation,
                receiptAssociation,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        ExecuteAdmittedCoreAsync(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
                executorAssociation,
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
                receiptAssociation,
            CancellationToken cancellationToken)
    {

        OperationResult<DynamicGraphL3SelectedTargetExecutionResult>? execution;
        var startedTimestamp = _timeProvider.GetTimestamp();
        try
        {
            var executionTask = _executor.ExecuteAsync(
                executorAssociation,
                cancellationToken);
            if (executionTask is null)
            {
                return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                    ResultRequiredMessage,
                    ResultRequiredCode);
            }

            execution = await executionTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                ExecutionFailedMessage,
                ExecutionFailedCode);
        }

        var completedTimestamp = _timeProvider.GetTimestamp();

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (execution is null || (execution.IsSuccess && execution.Value is null))
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                ResultRequiredMessage,
                ResultRequiredCode);
        }

        if (!execution.IsSuccess)
        {
            return execution.MapError<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>();
        }

        DynamicGraphL3SelectedTargetExecutionResult ownedExecution;
        try
        {
            var result = execution.Value!;
            var resultTargetId = result.TargetId;
            var executionId = result.ExecutionId;
            var evidenceIds = result.EvidenceIds.ToArray();
            if (!IsCanonicalIdentity(resultTargetId)
                || !IsCanonicalIdentity(executionId)
                || evidenceIds.Length == 0
                || !AreCanonicalOrdinalUnique(evidenceIds))
            {
                return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                    ResultInvalidMessage,
                    ResultInvalidCode);
            }

            var admittedTargetId = receiptAssociation.Target.TargetId;
            var admittedEdgeId = receiptAssociation.EdgeAuthoritySelectionQualityEvidence
                .EdgeAuthorityControl.AuthorityEvidence.EdgeId;
            if (!StringComparer.Ordinal.Equals(resultTargetId, admittedTargetId)
                || !StringComparer.Ordinal.Equals(resultTargetId, admittedEdgeId))
            {
                return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                    TargetMismatchMessage,
                    TargetMismatchCode);
            }

            ownedExecution = new DynamicGraphL3SelectedTargetExecutionResult(
                resultTargetId,
                executionId,
                evidenceIds);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                ResultInvalidMessage,
                ResultInvalidCode);
        }

        long inferenceElapsedMs;
        try
        {
            var elapsed = _timeProvider.GetElapsedTime(
                startedTimestamp,
                completedTimestamp);
            if (elapsed < TimeSpan.Zero)
                return RunSpeedTimingInvalid();

            checked
            {
                var wholeMilliseconds = elapsed.Ticks / TimeSpan.TicksPerMillisecond;
                if (elapsed.Ticks % TimeSpan.TicksPerMillisecond != 0)
                    wholeMilliseconds++;
                if (wholeMilliseconds == 0)
                    wholeMilliseconds = 1;
                inferenceElapsedMs = wholeMilliseconds;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            return RunSpeedTimingInvalid();
        }
        catch (OverflowException)
        {
            return RunSpeedTimingInvalid();
        }

        // One completed direct target invocation is exactly one evaluated row.
        // RoutingElapsedMs is zero because no separate routing interval was observed; it is
        // not proof that routing consumed zero wall time. Empty candidate and batch banks mean
        // that no such samples were observed. This evidence makes no lifecycle or training claim.
        var runSpeed = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
            receiptAssociation.Target,
            new DynamicGraphRunSpeedObservation(
                EvaluatedRowCount: 1,
                InferenceElapsedMs: inferenceElapsedMs,
                RoutingElapsedMs: 0,
                CandidateElapsedMsSamples: Array.Empty<long>(),
                BatchElapsedMsSamples: Array.Empty<long>()));
        if (!runSpeed.IsSuccess)
        {
            return runSpeed.MapError<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var receipt = new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
            receiptAssociation,
            ownedExecution,
            runSpeed.Value!);
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
            receipt);
    }

    private bool TryStart()
        => Interlocked.CompareExchange(ref _started, 1, 0) == 0;

    private static bool AreCanonicalOrdinalUnique(IReadOnlyList<string> values)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Count; index++)
        {
            if (!IsCanonicalIdentity(values[index]) || !seen.Add(values[index]))
                return false;
        }

        return true;
    }

    private static bool IsCanonicalIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character)
                || character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

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

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        CopyAssociation(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement source)
        => new(source.Target, source.EdgeAuthoritySelectionQualityEvidence);

    private static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>
        Cancelled()
        => Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
            CancelledMessage,
            CancelledCode);

    private static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>
        AssociationInvalid()
        => Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
            AssociationInvalidMessage,
            AssociationInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>
        RunSpeedTimingInvalid()
        => Failure<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
            RunSpeedTimingInvalidMessage,
            RunSpeedTimingInvalidCode);

    private static OperationResult<T> Failure<T>(string message, string code)
        => OperationResult<T>.Fail(message, code);
}
