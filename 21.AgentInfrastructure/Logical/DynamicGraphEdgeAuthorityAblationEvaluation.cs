// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using SemanticCompilerAdapter;

/// <summary>
/// One caller-attested held-out source binding. The source hash and document provenance are
/// caller attestations used for exact structural correlation; they are not cryptographic proof,
/// independently verified authorship, content identity, or a contamination scan.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationSourceBinding(
    string? EvalSourceRoot,
    string? CallerAttestedSourceHash,
    string? TraversalDocumentId,
    string? TraversalSourcePath);

/// <summary>
/// One caller-labelled held-out row for edge-authority selection accuracy. Its provenance value is
/// caller-attested and does not prove independent label authorship. It does not claim target
/// execution, downstream product accuracy, novelty, full edge quality, trained state, physical or
/// transitive disablement, DPO, training, promotion, cutover, or lifecycle.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationEvaluationRow
{
    private readonly EdgeTraversalInput? _traversalInput;

    public DynamicGraphEdgeAuthorityAblationEvaluationRow(
        string? RowId,
        string? EvalSourceRoot,
        string? CallerAttestedSourceHash,
        EdgeTraversalInput? TraversalInput,
        string? ExpectedTopTargetId,
        string? CallerAttestedLabelProvenanceId)
    {
        this.RowId = RowId;
        this.EvalSourceRoot = EvalSourceRoot;
        this.CallerAttestedSourceHash = CallerAttestedSourceHash;
        _traversalInput = TraversalInput is null ? null : CopyTraversal(TraversalInput);
        this.ExpectedTopTargetId = ExpectedTopTargetId;
        this.CallerAttestedLabelProvenanceId = CallerAttestedLabelProvenanceId;
    }

    public string? RowId { get; }

    public string? EvalSourceRoot { get; }

    public string? CallerAttestedSourceHash { get; }

    public EdgeTraversalInput? TraversalInput
        => _traversalInput is null ? null : CopyTraversal(_traversalInput);

    public string? ExpectedTopTargetId { get; }

    public string? CallerAttestedLabelProvenanceId { get; }

    private static EdgeTraversalInput CopyTraversal(EdgeTraversalInput source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(source);
}

/// <summary>
/// Owns one ordered source-disjoint evaluation request. Row order is significant and is the exact
/// sequential execution order. The request is selection-accuracy input only and carries no broader
/// product-quality, learning, lifecycle, or physical-disablement claim.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationEvaluationRequest
{
    private readonly FrozenDynamicGraphEdge? _edge;
    private readonly LogicalExecutionFlags? _flags;
    private readonly DynamicGraphMeasurementSplitProof? _splitProof;
    private readonly DynamicGraphEdgeAuthorityAblationSourceBinding[]? _sourceBindings;
    private readonly DynamicGraphEdgeAuthorityAblationEvaluationRow[]? _rows;

    public DynamicGraphEdgeAuthorityAblationEvaluationRequest(
        string? EvaluationId,
        IDynamicGraphEdge? Edge,
        string? DomainTag,
        LogicalExecutionFlags? Flags,
        DynamicGraphMeasurementSplitProof? SplitProof,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationSourceBinding>? SourceBindings,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationEvaluationRow>? Rows)
    {
        this.EvaluationId = EvaluationId;
        _edge = Edge is null ? null : FrozenDynamicGraphEdge.Create(Edge);
        this.DomainTag = DomainTag;
        _flags = Flags is null ? null : Flags with { };
        _splitProof = SplitProof is null ? null : CopySplitProof(SplitProof);
        _sourceBindings = SourceBindings?
            .Select(binding => binding is null ? null! : CopyBinding(binding))
            .ToArray();
        _rows = Rows?.Select(row => row is null ? null! : CopyRow(row)).ToArray();
    }

    public string? EvaluationId { get; }

    public IDynamicGraphEdge? Edge => _edge?.Copy();

    public string? DomainTag { get; }

    public LogicalExecutionFlags? Flags => _flags is null ? null : _flags with { };

    public DynamicGraphMeasurementSplitProof? SplitProof
        => _splitProof is null ? null : CopySplitProof(_splitProof);

    public IReadOnlyList<DynamicGraphEdgeAuthorityAblationSourceBinding>? SourceBindings
        => _sourceBindings is null
            ? null
            : Array.AsReadOnly(
                _sourceBindings
                    .Select(binding => binding is null ? null! : CopyBinding(binding))
                    .ToArray());

    public IReadOnlyList<DynamicGraphEdgeAuthorityAblationEvaluationRow>? Rows
        => _rows is null
            ? null
            : Array.AsReadOnly(
                _rows.Select(row => row is null ? null! : CopyRow(row)).ToArray());

    private static DynamicGraphEdgeAuthorityAblationEvaluationRow CopyRow(
        DynamicGraphEdgeAuthorityAblationEvaluationRow source)
        => new(
            source.RowId,
            source.EvalSourceRoot,
            source.CallerAttestedSourceHash,
            source.TraversalInput,
            source.ExpectedTopTargetId,
            source.CallerAttestedLabelProvenanceId);

    private static DynamicGraphEdgeAuthorityAblationSourceBinding CopyBinding(
        DynamicGraphEdgeAuthorityAblationSourceBinding source)
        => new(
            source.EvalSourceRoot,
            source.CallerAttestedSourceHash,
            source.TraversalDocumentId,
            source.TraversalSourcePath);

    internal static DynamicGraphMeasurementSplitProof CopySplitProof(
        DynamicGraphMeasurementSplitProof source)
        => new(
            CopyStrings(source.TrainSourceRoots),
            CopyStrings(source.EvalSourceRoots),
            CopyStrings(source.SourceHashes),
            source.CrossSplitNearDuplicateCount,
            source.SimhashOrMinhash,
            source.NearDuplicateThreshold);

    private static IReadOnlyList<string> CopyStrings(IReadOnlyList<string>? source)
        => source is null ? null! : Array.AsReadOnly(source.ToArray());
}

/// <summary>
/// Immutable paired top-1 selection evidence for one held-out row. Match booleans are independent:
/// the learned prediction is never substituted into the deterministic baseline prediction.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationRowEvidence(
    string RowId,
    string EvalSourceRoot,
    string CallerAttestedSourceHash,
    string TraversalDocumentId,
    string TraversalSourcePath,
    string CallerAttestedLabelProvenanceId,
    string ExpectedTopTargetId,
    string? LearnedTopTargetId,
    string BaselineTopTargetId,
    bool LearnedTopMatchesExpected,
    bool BaselineTopMatchesExpected);

/// <summary>
/// Deep-owned paired selection-accuracy evidence. It explicitly does not prove target execution,
/// downstream product accuracy, novelty, full edge quality, trained state, physical or transitive
/// disablement, DPO, training, promotion, cutover, lifecycle, or any state transition.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationEvaluationResult
{
    public const string ProducedScorerId = "edge-authority-paired-top1-exact-match";
    public const string ProducedScorerVersion = "1.0.0";
    public const string ProducedThresholdSetId = "dynamic-graph-q11-selection-drop-v1";
    public const string ProducedNormalizationId = "round-12-away-from-zero";

    private readonly DynamicGraphMeasurementSplitProof _splitProof;
    private readonly DynamicGraphEdgeAuthorityAblationRowEvidence[] _rows;
    private string _scorerId = ProducedScorerId;
    private string _scorerVersion = ProducedScorerVersion;
    private string _thresholdSetId = ProducedThresholdSetId;
    private string _normalizationId = ProducedNormalizationId;

    internal DynamicGraphEdgeAuthorityAblationEvaluationResult(
        string evaluationId,
        string edgeId,
        string authorityUnitId,
        DynamicGraphMeasurementSplitProof splitProof,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence> rows,
        int learnedCorrectCount,
        double learnedSelectionAccuracy,
        int baselineCorrectCount,
        double baselineSelectionAccuracy,
        double accuracyDrop,
        double normalizedAccuracyDrop,
        bool meetsQ11NumericalGate)
    {
        EvaluationId = evaluationId;
        EdgeId = edgeId;
        AuthorityUnitId = authorityUnitId;
        _splitProof = DynamicGraphEdgeAuthorityAblationEvaluationRequest.CopySplitProof(splitProof);
        _rows = rows.Select(CopyRow).ToArray();
        LearnedCorrectCount = learnedCorrectCount;
        LearnedSelectionAccuracy = learnedSelectionAccuracy;
        BaselineCorrectCount = baselineCorrectCount;
        BaselineSelectionAccuracy = baselineSelectionAccuracy;
        AccuracyDrop = accuracyDrop;
        NormalizedAccuracyDrop = normalizedAccuracyDrop;
        MeetsQ11NumericalGate = meetsQ11NumericalGate;
    }

    public string EvaluationId { get; }

    public string EdgeId { get; }

    public string AuthorityUnitId { get; }

    public string ScorerId => _scorerId;

    public string ScorerVersion => _scorerVersion;

    public string ThresholdSetId => _thresholdSetId;

    public string NormalizationId => _normalizationId;

    public DynamicGraphMeasurementSplitProof SplitProof
        => DynamicGraphEdgeAuthorityAblationEvaluationRequest.CopySplitProof(_splitProof);

    public IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence> Rows
        => Array.AsReadOnly(_rows.Select(CopyRow).ToArray());

    public int RowsEvaluated => _rows.Length;

    public int LearnedCorrectCount { get; }

    public double LearnedSelectionAccuracy { get; }

    public int BaselineCorrectCount { get; }

    public double BaselineSelectionAccuracy { get; }

    public double AccuracyDrop { get; }

    public double NormalizedAccuracyDrop { get; }

    public bool MeetsQ11NumericalGate { get; }

    private static DynamicGraphEdgeAuthorityAblationRowEvidence CopyRow(
        DynamicGraphEdgeAuthorityAblationRowEvidence source)
        => new(
            source.RowId,
            source.EvalSourceRoot,
            source.CallerAttestedSourceHash,
            source.TraversalDocumentId,
            source.TraversalSourcePath,
            source.CallerAttestedLabelProvenanceId,
            source.ExpectedTopTargetId,
            source.LearnedTopTargetId,
            source.BaselineTopTargetId,
            source.LearnedTopMatchesExpected,
            source.BaselineTopMatchesExpected);
}

/// <summary>
/// Immutable transport input for importing an already-produced ablation carrier. This shape
/// carries caller-attested source and label fields and proves no source provenance, authorship,
/// contamination status, training, DPO, promotion, lifecycle state, target execution, or product
/// quality. Its fixed scorer metadata is validated as producer identity rather than accepted as
/// caller authority.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationEvaluationImport
{
    private readonly DynamicGraphMeasurementSplitProof? _splitProof;
    private readonly DynamicGraphEdgeAuthorityAblationRowEvidence[]? _rows;

    public DynamicGraphEdgeAuthorityAblationEvaluationImport(
        string? EvaluationId,
        string? EdgeId,
        string? AuthorityUnitId,
        string? ScorerId,
        string? ScorerVersion,
        string? ThresholdSetId,
        string? NormalizationId,
        DynamicGraphMeasurementSplitProof? SplitProof,
        IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence>? Rows,
        int LearnedCorrectCount,
        double LearnedSelectionAccuracy,
        int BaselineCorrectCount,
        double BaselineSelectionAccuracy,
        double AccuracyDrop,
        double NormalizedAccuracyDrop,
        bool MeetsQ11NumericalGate)
    {
        this.EvaluationId = EvaluationId;
        this.EdgeId = EdgeId;
        this.AuthorityUnitId = AuthorityUnitId;
        this.ScorerId = ScorerId;
        this.ScorerVersion = ScorerVersion;
        this.ThresholdSetId = ThresholdSetId;
        this.NormalizationId = NormalizationId;
        _splitProof = SplitProof is null
            ? null
            : DynamicGraphEdgeAuthorityAblationEvaluationRequest.CopySplitProof(SplitProof);
        _rows = Rows?
            .Select(row => row is null ? null! : CopyRow(row))
            .ToArray();
        this.LearnedCorrectCount = LearnedCorrectCount;
        this.LearnedSelectionAccuracy = LearnedSelectionAccuracy;
        this.BaselineCorrectCount = BaselineCorrectCount;
        this.BaselineSelectionAccuracy = BaselineSelectionAccuracy;
        this.AccuracyDrop = AccuracyDrop;
        this.NormalizedAccuracyDrop = NormalizedAccuracyDrop;
        this.MeetsQ11NumericalGate = MeetsQ11NumericalGate;
    }

    public string? EvaluationId { get; }

    public string? EdgeId { get; }

    public string? AuthorityUnitId { get; }

    public string? ScorerId { get; }

    public string? ScorerVersion { get; }

    public string? ThresholdSetId { get; }

    public string? NormalizationId { get; }

    public DynamicGraphMeasurementSplitProof? SplitProof
        => _splitProof is null
            ? null
            : DynamicGraphEdgeAuthorityAblationEvaluationRequest.CopySplitProof(_splitProof);

    public IReadOnlyList<DynamicGraphEdgeAuthorityAblationRowEvidence>? Rows
        => _rows is null
            ? null
            : Array.AsReadOnly(
                _rows.Select(row => row is null ? null! : CopyRow(row)).ToArray());

    public int LearnedCorrectCount { get; }

    public double LearnedSelectionAccuracy { get; }

    public int BaselineCorrectCount { get; }

    public double BaselineSelectionAccuracy { get; }

    public double AccuracyDrop { get; }

    public double NormalizedAccuracyDrop { get; }

    public bool MeetsQ11NumericalGate { get; }

    private static DynamicGraphEdgeAuthorityAblationRowEvidence CopyRow(
        DynamicGraphEdgeAuthorityAblationRowEvidence source)
        => source with { };
}

/// <summary>
/// Sequentially obtains paired learned/baseline predictions and computes only held-out top-1
/// selection accuracy. It has no scorer dependency and makes no target-execution, downstream
/// accuracy, novelty, full-quality, training, DPO, promotion, cutover, or lifecycle decision.
/// </summary>
public sealed class DynamicGraphEdgeAuthorityAblationEvaluationRunner
{
    private const string CancelledCode = "CANCELLED";
    private const string ProbeRequiredCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_REQUIRED";
    private const string RequestRequiredCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_REQUEST_REQUIRED";
    private const string RequestInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_REQUEST_INVALID";
    private const string ProbeExecutionFailedCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_EXECUTION_FAILED";
    private const string ProbeResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_PROBE_RESULT_INVALID";
    private const string ScoringInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_SCORING_INVALID";
    internal const string ResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_RESULT_INVALID";
    internal const string ResultInvalidMessage =
        "Dynamic graph edge authority ablation evaluation result is invalid.";
    private const string ExpectedProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";

    private readonly IDynamicGraphEdgeAuthorityAblationProbe? _probe;

    public DynamicGraphEdgeAuthorityAblationEvaluationRunner(
        IDynamicGraphEdgeAuthorityAblationProbe? probe)
    {
        _probe = probe;
    }

    /// <summary>
    /// Imports and independently owns only a self-consistent carrier produced under the fixed
    /// ablation scorer contract. Caller-attested source and label provenance remain unverified.
    /// This does not grant training, DPO, promotion, lifecycle, target-execution, provenance, or
    /// product-quality authority.
    /// </summary>
    public static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>
        ImportValidated(DynamicGraphEdgeAuthorityAblationEvaluationImport? import)
    {
        if (import is null)
            return EvaluationResultInvalid();

        try
        {
            var splitProof = import.SplitProof;
            var rows = import.Rows;
            if (!StringComparer.Ordinal.Equals(
                    import.ScorerId,
                    DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerId)
                || !StringComparer.Ordinal.Equals(
                    import.ScorerVersion,
                    DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerVersion)
                || !StringComparer.Ordinal.Equals(
                    import.ThresholdSetId,
                    DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedThresholdSetId)
                || !StringComparer.Ordinal.Equals(
                    import.NormalizationId,
                    DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedNormalizationId)
                || splitProof is null
                || rows is null)
            {
                return EvaluationResultInvalid();
            }

            var candidate = new DynamicGraphEdgeAuthorityAblationEvaluationResult(
                import.EvaluationId!,
                import.EdgeId!,
                import.AuthorityUnitId!,
                splitProof,
                rows,
                import.LearnedCorrectCount,
                import.LearnedSelectionAccuracy,
                import.BaselineCorrectCount,
                import.BaselineSelectionAccuracy,
                import.AccuracyDrop,
                import.NormalizedAccuracyDrop,
                import.MeetsQ11NumericalGate);
            if (!TrySnapshotValidatedResult(candidate, out var ownedSnapshot))
                return EvaluationResultInvalid();

            return OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>.Ok(
                ownedSnapshot);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return EvaluationResultInvalid();
        }
    }

    public async Task<OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>> RunAsync(
        DynamicGraphEdgeAuthorityAblationEvaluationRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var probe = _probe;
        if (probe is null)
        {
            return Failure(
                "Edge authority ablation probe is required.",
                ProbeRequiredCode);
        }

        if (request is null)
        {
            return Failure(
                "Edge authority ablation evaluation request is required.",
                RequestRequiredCode);
        }

        OperationResult<DynamicGraphMeasurementSplitProof> splitValidation;
        try
        {
            splitValidation = DynamicGraphHeldOutSplitGuard.Validate(request.SplitProof);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RequestInvalid();
        }

        if (!splitValidation.IsSuccess)
            return splitValidation.MapError<DynamicGraphEdgeAuthorityAblationEvaluationResult>();

        EvaluationSnapshot snapshot;
        try
        {
            if (!TrySnapshotRequest(request, splitValidation.Value!, out snapshot))
                return RequestInvalid();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RequestInvalid();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var predictions = new List<PairedPrediction>(snapshot.Rows.Length);
        foreach (var row in snapshot.Rows)
        {
            OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>? probeResult;
            try
            {
                probeResult = await probe.RunAsync(
                        snapshot.Edge.Copy(),
                        CopyTraversal(row.TraversalInput),
                        snapshot.DomainTag,
                        snapshot.Flags with { },
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return Failure(
                    "Edge authority ablation probe execution failed.",
                    ProbeExecutionFailedCode);
            }

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            if (probeResult is null || (probeResult.IsSuccess && probeResult.Value is null))
                return ProbeResultInvalid();

            if (!probeResult.IsSuccess)
                return probeResult.MapError<DynamicGraphEdgeAuthorityAblationEvaluationResult>();

            try
            {
                if (!TryExtractPrediction(
                        probeResult.Value!,
                        snapshot.Edge,
                        row,
                        out var learnedTop,
                        out var baselineTop))
                {
                    return ProbeResultInvalid();
                }

                predictions.Add(new PairedPrediction(row, learnedTop, baselineTop));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ProbeResultInvalid();
            }
        }

        if (!TryScore(predictions, out var score))
        {
            return Failure(
                "Edge authority ablation selection scoring is invalid.",
                ScoringInvalidCode);
        }

        var producedResult = new DynamicGraphEdgeAuthorityAblationEvaluationResult(
            snapshot.EvaluationId,
            snapshot.Edge.EdgeId,
            snapshot.Edge.AuthorityUnitId,
            snapshot.SplitProof,
            score.Rows,
            score.LearnedCorrectCount,
            score.LearnedAccuracy,
            score.BaselineCorrectCount,
            score.BaselineAccuracy,
            score.AccuracyDrop,
            score.NormalizedAccuracyDrop,
            score.MeetsGate);
        if (!TrySnapshotValidatedResult(producedResult, out var validatedResult))
            return EvaluationResultInvalid();

        return OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>.Ok(
            validatedResult);
    }

    /// <summary>
    /// Validates and owns only the self-consistency of the produced carrier and its producer-path
    /// invariants. Source hashes and label provenance remain caller-attested and noncryptographic.
    /// This boundary cannot reconstruct the original request or edge target set, establish
    /// provenance independently, or prove generalization, product accuracy, training, promotion,
    /// lifecycle state, target execution, or physical disablement.
    /// </summary>
    internal static bool TrySnapshotValidatedResult(
        DynamicGraphEdgeAuthorityAblationEvaluationResult? source,
        out DynamicGraphEdgeAuthorityAblationEvaluationResult ownedSnapshot)
    {
        ownedSnapshot = null!;
        if (source is null)
            return false;

        try
        {
            return TrySnapshotValidatedResultCore(source, out ownedSnapshot);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ownedSnapshot = null!;
            return false;
        }
    }

    private static bool TrySnapshotValidatedResultCore(
        DynamicGraphEdgeAuthorityAblationEvaluationResult source,
        out DynamicGraphEdgeAuthorityAblationEvaluationResult ownedSnapshot)
    {
        ownedSnapshot = null!;
        var evaluationId = source.EvaluationId;
        var edgeId = source.EdgeId;
        var authorityUnitId = source.AuthorityUnitId;
        var scorerId = source.ScorerId;
        var scorerVersion = source.ScorerVersion;
        var thresholdSetId = source.ThresholdSetId;
        var normalizationId = source.NormalizationId;
        var splitValidation = DynamicGraphHeldOutSplitGuard.Validate(source.SplitProof);
        if (!splitValidation.IsSuccess)
            return false;

        var rowSource = source.Rows;
        if (!IsCanonical(evaluationId)
            || !IsCanonical(edgeId)
            || !IsCanonical(authorityUnitId)
            || !StringComparer.Ordinal.Equals(
                scorerId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerId)
            || !StringComparer.Ordinal.Equals(
                scorerVersion,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerVersion)
            || !StringComparer.Ordinal.Equals(
                thresholdSetId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedThresholdSetId)
            || !StringComparer.Ordinal.Equals(
                normalizationId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedNormalizationId)
            || rowSource is null
            || rowSource.Count == 0)
        {
            return false;
        }

        var admittedSplit = splitValidation.Value!;
        var evalRoots = admittedSplit.EvalSourceRoots.ToHashSet(StringComparer.Ordinal);
        var trainRoots = admittedSplit.TrainSourceRoots.ToHashSet(StringComparer.Ordinal);
        var sourceHashes = admittedSplit.SourceHashes.ToHashSet(StringComparer.Ordinal);
        var rowIds = new HashSet<string>(StringComparer.Ordinal);
        var callerAttestedLabelIds = new HashSet<string>(StringComparer.Ordinal);
        var evaluatedRoots = new HashSet<string>(StringComparer.Ordinal);
        var mappingByRoot = new Dictionary<string, SourceTuple>(StringComparer.Ordinal);
        var rootByHash = new Dictionary<string, string>(StringComparer.Ordinal);
        var rows = new DynamicGraphEdgeAuthorityAblationRowEvidence[rowSource.Count];
        var learnedCorrectCount = 0;
        var baselineCorrectCount = 0;
        for (var index = 0; index < rowSource.Count; index++)
        {
            var row = rowSource[index];
            if (row is null
                || !IsCanonical(row.RowId)
                || !rowIds.Add(row.RowId)
                || !IsCanonical(row.EvalSourceRoot)
                || !evalRoots.Contains(row.EvalSourceRoot)
                || !IsCanonical(row.CallerAttestedSourceHash)
                || !sourceHashes.Contains(row.CallerAttestedSourceHash)
                || !IsCanonical(row.TraversalDocumentId)
                || !IsCanonical(row.TraversalSourcePath)
                || trainRoots.Contains(row.TraversalSourcePath)
                || !IsCanonical(row.CallerAttestedLabelProvenanceId)
                || !callerAttestedLabelIds.Add(row.CallerAttestedLabelProvenanceId)
                || !IsCanonical(row.ExpectedTopTargetId)
                || (row.LearnedTopTargetId is not null
                    && !IsCanonical(row.LearnedTopTargetId))
                || !IsCanonical(row.BaselineTopTargetId))
            {
                return false;
            }

            evaluatedRoots.Add(row.EvalSourceRoot);
            var tuple = new SourceTuple(
                row.CallerAttestedSourceHash,
                row.TraversalDocumentId,
                row.TraversalSourcePath);
            if (mappingByRoot.TryGetValue(row.EvalSourceRoot, out var priorTuple))
            {
                if (priorTuple != tuple)
                    return false;
            }
            else
            {
                mappingByRoot.Add(row.EvalSourceRoot, tuple);
            }

            if (rootByHash.TryGetValue(row.CallerAttestedSourceHash, out var priorRoot))
            {
                if (!StringComparer.Ordinal.Equals(priorRoot, row.EvalSourceRoot))
                    return false;
            }
            else
            {
                rootByHash.Add(row.CallerAttestedSourceHash, row.EvalSourceRoot);
            }

            var learnedMatches = row.LearnedTopTargetId is not null
                && StringComparer.Ordinal.Equals(
                    row.LearnedTopTargetId,
                    row.ExpectedTopTargetId);
            var baselineMatches = StringComparer.Ordinal.Equals(
                row.BaselineTopTargetId,
                row.ExpectedTopTargetId);
            if (row.LearnedTopMatchesExpected != learnedMatches
                || row.BaselineTopMatchesExpected != baselineMatches)
            {
                return false;
            }

            if (learnedMatches)
                learnedCorrectCount++;
            if (baselineMatches)
                baselineCorrectCount++;

            rows[index] = new DynamicGraphEdgeAuthorityAblationRowEvidence(
                row.RowId,
                row.EvalSourceRoot,
                row.CallerAttestedSourceHash,
                row.TraversalDocumentId,
                row.TraversalSourcePath,
                row.CallerAttestedLabelProvenanceId,
                row.ExpectedTopTargetId,
                row.LearnedTopTargetId,
                row.BaselineTopTargetId,
                learnedMatches,
                baselineMatches);
        }

        if (!evaluatedRoots.SetEquals(evalRoots))
            return false;

        var learnedAccuracy = (double)learnedCorrectCount / rows.Length;
        var baselineAccuracy = (double)baselineCorrectCount / rows.Length;
        var accuracyDrop = learnedAccuracy - baselineAccuracy;
        var normalizedAccuracyDrop = Math.Round(
            accuracyDrop,
            12,
            MidpointRounding.AwayFromZero);
        var meetsGate = normalizedAccuracyDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop;
        if (source.LearnedCorrectCount != learnedCorrectCount
            || !source.LearnedSelectionAccuracy.Equals(learnedAccuracy)
            || source.BaselineCorrectCount != baselineCorrectCount
            || !source.BaselineSelectionAccuracy.Equals(baselineAccuracy)
            || !source.AccuracyDrop.Equals(accuracyDrop)
            || !source.NormalizedAccuracyDrop.Equals(normalizedAccuracyDrop)
            || source.MeetsQ11NumericalGate != meetsGate)
        {
            return false;
        }

        ownedSnapshot = new DynamicGraphEdgeAuthorityAblationEvaluationResult(
            evaluationId,
            edgeId,
            authorityUnitId,
            admittedSplit,
            rows,
            learnedCorrectCount,
            learnedAccuracy,
            baselineCorrectCount,
            baselineAccuracy,
            accuracyDrop,
            normalizedAccuracyDrop,
            meetsGate);
        return true;
    }

    private static bool TrySnapshotRequest(
        DynamicGraphEdgeAuthorityAblationEvaluationRequest source,
        DynamicGraphMeasurementSplitProof admittedSplit,
        out EvaluationSnapshot snapshot)
    {
        snapshot = null!;
        var evaluationId = source.EvaluationId;
        var edgeSource = source.Edge;
        var domainTag = source.DomainTag;
        var flags = source.Flags;
        var bindingSource = source.SourceBindings;
        var rowSource = source.Rows;
        if (!IsCanonical(evaluationId)
            || edgeSource is null
            || !FrozenDynamicGraphEdge.TryCreateValid(edgeSource, out var edge)
            || !IsCanonical(domainTag)
            || flags is null
            || !flags.UseLearnedUnitWhenAvailable
            || bindingSource is null
            || bindingSource.Count == 0
            || rowSource is null
            || rowSource.Count == 0)
        {
            return false;
        }

        var evalRoots = admittedSplit.EvalSourceRoots.ToHashSet(StringComparer.Ordinal);
        var trainRoots = admittedSplit.TrainSourceRoots.ToHashSet(StringComparer.Ordinal);
        var admittedHashes = admittedSplit.SourceHashes.ToHashSet(StringComparer.Ordinal);
        var bindingRoots = new HashSet<string>(StringComparer.Ordinal);
        var bindingHashes = new HashSet<string>(StringComparer.Ordinal);
        var bindingsByRoot = new Dictionary<string, SourceBindingSnapshot>(StringComparer.Ordinal);
        for (var index = 0; index < bindingSource.Count; index++)
        {
            var binding = bindingSource[index];
            if (binding is null
                || !IsCanonical(binding.EvalSourceRoot)
                || !bindingRoots.Add(binding.EvalSourceRoot!)
                || !evalRoots.Contains(binding.EvalSourceRoot!)
                || trainRoots.Contains(binding.EvalSourceRoot!)
                || !IsCanonical(binding.CallerAttestedSourceHash)
                || !bindingHashes.Add(binding.CallerAttestedSourceHash!)
                || !admittedHashes.Contains(binding.CallerAttestedSourceHash!)
                || !IsCanonical(binding.TraversalDocumentId)
                || !IsCanonical(binding.TraversalSourcePath)
                || trainRoots.Contains(binding.TraversalSourcePath!))
            {
                return false;
            }

            bindingsByRoot.Add(
                binding.EvalSourceRoot!,
                new SourceBindingSnapshot(
                    binding.EvalSourceRoot!,
                    binding.CallerAttestedSourceHash!,
                    binding.TraversalDocumentId!,
                    binding.TraversalSourcePath!));
        }

        if (!bindingRoots.SetEquals(evalRoots))
            return false;

        var rowIds = new HashSet<string>(StringComparer.Ordinal);
        var callerAttestedLabelProvenanceIds = new HashSet<string>(StringComparer.Ordinal);
        var evaluatedRoots = new HashSet<string>(StringComparer.Ordinal);
        var rows = new EvaluationRowSnapshot[rowSource.Count];
        for (var index = 0; index < rowSource.Count; index++)
        {
            var row = rowSource[index];
            if (row is null
                || !IsCanonical(row.RowId)
                || !rowIds.Add(row.RowId!)
                || !IsCanonical(row.EvalSourceRoot)
                || !bindingsByRoot.TryGetValue(row.EvalSourceRoot!, out var binding)
                || !IsCanonical(row.CallerAttestedSourceHash)
                || !StringComparer.Ordinal.Equals(
                    row.CallerAttestedSourceHash,
                    binding.CallerAttestedSourceHash)
                || !IsCanonical(row.ExpectedTopTargetId)
                || !edge.ToNodeIds.Contains(row.ExpectedTopTargetId!, StringComparer.Ordinal)
                || !IsCanonical(row.CallerAttestedLabelProvenanceId)
                || !callerAttestedLabelProvenanceIds.Add(
                    row.CallerAttestedLabelProvenanceId!)
                || row.TraversalInput is null
                || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotTraversalInput(
                    row.TraversalInput,
                    out var traversal)
                || !TraversalMatches(edge, traversal, domainTag!)
                || !StringComparer.Ordinal.Equals(
                    traversal.Source.DocumentId,
                    binding.TraversalDocumentId)
                || !StringComparer.Ordinal.Equals(
                    traversal.Source.SourcePath,
                    binding.TraversalSourcePath)
                || trainRoots.Contains(traversal.Source.SourcePath))
            {
                return false;
            }

            rows[index] = new EvaluationRowSnapshot(
                row.RowId!,
                row.EvalSourceRoot!,
                row.CallerAttestedSourceHash!,
                traversal,
                row.ExpectedTopTargetId!,
                row.CallerAttestedLabelProvenanceId!);
            evaluatedRoots.Add(row.EvalSourceRoot!);
        }

        if (!evaluatedRoots.SetEquals(evalRoots))
            return false;

        snapshot = new EvaluationSnapshot(
            evaluationId!,
            edge,
            domainTag!,
            flags with { },
            DynamicGraphEdgeAuthorityAblationEvaluationRequest.CopySplitProof(admittedSplit),
            rows);
        return true;
    }

    private static bool TryExtractPrediction(
        DynamicGraphEdgeAuthorityAblationProbeResult result,
        FrozenDynamicGraphEdge edge,
        EvaluationRowSnapshot row,
        out string? learnedTop,
        out string baselineTop)
    {
        learnedTop = null;
        baselineTop = "";
        if (!DynamicGraphEdgeAuthorityAblationProbe.TrySnapshotValidatedResult(
                result,
                edge,
                row.TraversalInput,
                out var validated))
        {
            return false;
        }

        var learnedTargets = validated.Subject.Decision.OrderedTargets;
        var baselineOrder = validated.OrderedBaselineTargetIds;
        learnedTop = learnedTargets.Count == 0 ? null : learnedTargets[0].TargetSubPhaseId;
        baselineTop = baselineOrder[0];
        return true;
    }

    private static bool TryScore(
        IReadOnlyList<PairedPrediction> predictions,
        out SelectionScore score)
    {
        score = null!;
        if (predictions.Count == 0)
            return false;

        var rows = new DynamicGraphEdgeAuthorityAblationRowEvidence[predictions.Count];
        var learnedCorrect = 0;
        var baselineCorrect = 0;
        for (var index = 0; index < predictions.Count; index++)
        {
            var prediction = predictions[index];
            var learnedMatch = prediction.LearnedTop is not null
                && StringComparer.Ordinal.Equals(
                    prediction.LearnedTop,
                    prediction.Row.ExpectedTopTargetId);
            var baselineMatch = StringComparer.Ordinal.Equals(
                prediction.BaselineTop,
                prediction.Row.ExpectedTopTargetId);
            if (learnedMatch)
                learnedCorrect++;
            if (baselineMatch)
                baselineCorrect++;

            rows[index] = new DynamicGraphEdgeAuthorityAblationRowEvidence(
                prediction.Row.RowId,
                prediction.Row.EvalSourceRoot,
                prediction.Row.CallerAttestedSourceHash,
                prediction.Row.TraversalInput.Source.DocumentId,
                prediction.Row.TraversalInput.Source.SourcePath,
                prediction.Row.CallerAttestedLabelProvenanceId,
                prediction.Row.ExpectedTopTargetId,
                prediction.LearnedTop,
                prediction.BaselineTop,
                learnedMatch,
                baselineMatch);
        }

        var learnedAccuracy = (double)learnedCorrect / predictions.Count;
        var baselineAccuracy = (double)baselineCorrect / predictions.Count;
        var accuracyDrop = learnedAccuracy - baselineAccuracy;
        if (!double.IsFinite(learnedAccuracy)
            || !double.IsFinite(baselineAccuracy)
            || !double.IsFinite(accuracyDrop))
        {
            return false;
        }

        var normalizedAccuracyDrop = Math.Round(
            accuracyDrop,
            12,
            MidpointRounding.AwayFromZero);
        score = new SelectionScore(
            rows,
            learnedCorrect,
            learnedAccuracy,
            baselineCorrect,
            baselineAccuracy,
            accuracyDrop,
            normalizedAccuracyDrop,
            normalizedAccuracyDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop);
        return true;
    }

    private static bool TraversalMatches(
        FrozenDynamicGraphEdge edge,
        EdgeTraversalInput traversal,
        string domainTag)
        => IsCanonical(traversal.RunId)
           && StringComparer.Ordinal.Equals(traversal.EdgeId, edge.EdgeId)
           && traversal.Source is not null
           && StringComparer.Ordinal.Equals(traversal.Source.DomainLabel, domainTag)
           && traversal.Context is not null
           && traversal.UpstreamNodeOutput is not null
           && StringComparer.Ordinal.Equals(
               traversal.UpstreamNodeOutput.SourceNodeId,
               edge.FromNodeId)
           && traversal.UpstreamNodeOutput.UpstreamCandidateRefs is { Count: > 0 } references
           && references.All(IsCanonical)
           && references.Distinct(StringComparer.Ordinal).Count() == references.Count
           && references.SequenceEqual(
               references.OrderBy(value => value, StringComparer.Ordinal),
               StringComparer.Ordinal)
           && IsCanonical(traversal.UpstreamNodeOutput.UpstreamOutputHash)
           && StringComparer.Ordinal.Equals(
               traversal.UpstreamNodeOutput.ProjectionRule,
               ExpectedProjectionRule);

    private static EdgeTraversalInput CopyTraversal(EdgeTraversalInput source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(source);

    private static bool IsCanonical(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character));

    private static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult> Cancelled()
        => Failure("Cancelled.", CancelledCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>
        RequestInvalid()
        => Failure(
            "Edge authority ablation evaluation request is invalid.",
            RequestInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>
        ProbeResultInvalid()
        => Failure(
            "Edge authority ablation probe result is invalid.",
            ProbeResultInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>
        EvaluationResultInvalid()
        => Failure(ResultInvalidMessage, ResultInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult> Failure(
        string message,
        string code)
        => OperationResult<DynamicGraphEdgeAuthorityAblationEvaluationResult>.Fail(message, code);

    private sealed record EvaluationSnapshot(
        string EvaluationId,
        FrozenDynamicGraphEdge Edge,
        string DomainTag,
        LogicalExecutionFlags Flags,
        DynamicGraphMeasurementSplitProof SplitProof,
        EvaluationRowSnapshot[] Rows);

    private sealed record EvaluationRowSnapshot(
        string RowId,
        string EvalSourceRoot,
        string CallerAttestedSourceHash,
        EdgeTraversalInput TraversalInput,
        string ExpectedTopTargetId,
        string CallerAttestedLabelProvenanceId);

    private sealed record SourceBindingSnapshot(
        string EvalSourceRoot,
        string CallerAttestedSourceHash,
        string TraversalDocumentId,
        string TraversalSourcePath);

    private sealed record SourceTuple(
        string CallerAttestedSourceHash,
        string TraversalDocumentId,
        string TraversalSourcePath);

    private sealed record PairedPrediction(
        EvaluationRowSnapshot Row,
        string? LearnedTop,
        string BaselineTop);

    private sealed record SelectionScore(
        DynamicGraphEdgeAuthorityAblationRowEvidence[] Rows,
        int LearnedCorrectCount,
        double LearnedAccuracy,
        int BaselineCorrectCount,
        double BaselineAccuracy,
        double AccuracyDrop,
        double NormalizedAccuracyDrop,
        bool MeetsGate);
}

internal sealed class FrozenDynamicGraphEdge : IDynamicGraphEdge
{
    private readonly string[] _toNodeIds;
    private readonly string[] _excludedLeakageSignals;

    private FrozenDynamicGraphEdge(IDynamicGraphEdge source)
    {
        EdgeId = source.EdgeId;
        FromNodeId = source.FromNodeId;
        _toNodeIds = source.ToNodeIds?.ToArray() ?? [];
        Kind = source.Kind;
        ParallelEligible = source.ParallelEligible;
        AuthorityUnitId = source.AuthorityUnitId;
        AuthorityUnitLevel = source.AuthorityUnitLevel;
        DeterministicFinalAuthority = source.DeterministicFinalAuthority;
        BaselineUnitId = source.BaselineUnitId;
        BaselineRole = source.BaselineRole;
        _excludedLeakageSignals = source.ExcludedLeakageSignals?.ToArray() ?? [];
        SourceProof = source.SourceProof;
        CutoverProof = source.CutoverProof;
        AblationProofRef = source.AblationProofRef;
    }

    public string EdgeId { get; }

    public string FromNodeId { get; }

    public IReadOnlyList<string> ToNodeIds => Array.AsReadOnly(_toNodeIds.ToArray());

    public GraphEdgeKind Kind { get; }

    public bool ParallelEligible { get; }

    public string AuthorityUnitId { get; }

    public string AuthorityUnitLevel { get; }

    public bool DeterministicFinalAuthority { get; }

    public string? BaselineUnitId { get; }

    public string BaselineRole { get; }

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());

    public string SourceProof { get; }

    public string CutoverProof { get; }

    public string AblationProofRef { get; }

    internal static FrozenDynamicGraphEdge Create(IDynamicGraphEdge source) => new(source);

    internal static bool TryCreateValid(
        IDynamicGraphEdge source,
        out FrozenDynamicGraphEdge snapshot)
    {
        snapshot = new FrozenDynamicGraphEdge(source);
        return IsCanonical(snapshot.EdgeId)
               && IsCanonical(snapshot.FromNodeId)
               && snapshot.ToNodeIds.Count > 0
               && snapshot.ToNodeIds.All(IsCanonical)
               && snapshot.ToNodeIds.Distinct(StringComparer.Ordinal).Count()
                   == snapshot.ToNodeIds.Count
               && !snapshot.ToNodeIds.Contains(snapshot.FromNodeId, StringComparer.Ordinal)
               && (snapshot.Kind != GraphEdgeKind.ManyToMany
                   || snapshot.ToNodeIds.Count >= 2)
               && snapshot.Kind is GraphEdgeKind.FanOut
                   or GraphEdgeKind.ManyToMany
                   or GraphEdgeKind.MultiOutput
               && IsCanonical(snapshot.AuthorityUnitId)
               && StringComparer.Ordinal.Equals(snapshot.AuthorityUnitLevel, "L2")
               && !snapshot.DeterministicFinalAuthority
               && IsCanonical(snapshot.BaselineUnitId)
               && StringComparer.Ordinal.Equals(
                   snapshot.BaselineRole,
                   RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole)
               && DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                   snapshot.ExcludedLeakageSignals)
               && IsCanonical(snapshot.SourceProof)
               && IsCanonical(snapshot.CutoverProof)
               && IsCanonical(snapshot.AblationProofRef);
    }

    internal FrozenDynamicGraphEdge Copy() => new(this);

    private static bool IsCanonical(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character));
}
