// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Caller-attested novelty evidence for one training source. Source hashes are opaque split
/// identities; pattern and token fingerprints are asserted by the caller under the request-wide
/// schemes and are not independently tokenized, normalized, hashed, or cryptographically verified.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
    string? TrainSourceRoot,
    string? CallerAttestedSourceHash,
    IReadOnlyList<string>? CallerAttestedPatternFingerprints,
    IReadOnlyList<string>? CallerAttestedTokenFingerprints);

/// <summary>
/// Minimal caller-attested novelty input for one already-produced paired held-out row. Source
/// identity is deliberately absent and is derived only from the validated paired carrier.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyRowInput(
    string? RowId,
    string? CallerAttestedPatternFingerprint,
    IReadOnlyList<string>? CallerAttestedTokenFingerprints);

/// <summary>
/// Retains a pure novelty-production request without traversing caller collections. Validation and
/// snapshotting occur in <see cref="DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce"/>
/// so admitted split errors always precede novelty-evidence traversal.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
    DynamicGraphEdgeAuthorityAblationEvaluationResult? PairedSelection,
    IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingBinding>? TrainingBindings,
    IReadOnlyList<string>? RequestedRowIds,
    IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowInput>? NoveltyRows,
    string? CallerAttestedFingerprintNormalizationId,
    string? CallerAttestedTokenizationId);

/// <summary>
/// Deep-owned atomic caller-attested fingerprints for one admitted training source.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyTrainingEvidence
{
    private readonly string[] _patternFingerprints;
    private readonly string[] _tokenFingerprints;

    internal DynamicGraphEdgeAuthorityNoveltyTrainingEvidence(
        string trainSourceRoot,
        string callerAttestedSourceHash,
        IReadOnlyList<string> patternFingerprints,
        IReadOnlyList<string> tokenFingerprints)
    {
        TrainSourceRoot = trainSourceRoot;
        CallerAttestedSourceHash = callerAttestedSourceHash;
        _patternFingerprints = patternFingerprints.ToArray();
        _tokenFingerprints = tokenFingerprints.ToArray();
    }

    public string TrainSourceRoot { get; }

    public string CallerAttestedSourceHash { get; }

    public IReadOnlyList<string> CallerAttestedPatternFingerprints
        => Array.AsReadOnly(_patternFingerprints.ToArray());

    public IReadOnlyList<string> CallerAttestedTokenFingerprints
        => Array.AsReadOnly(_tokenFingerprints.ToArray());
}

/// <summary>
/// Deep-owned per-row novelty evidence. All source identity comes from the validated paired
/// selection row; only the fingerprints are caller attestations.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyRowEvidence
{
    private readonly string[] _tokenFingerprints;
    private readonly string[] _seenTrainingTokenFingerprints;

    internal DynamicGraphEdgeAuthorityNoveltyRowEvidence(
        string rowId,
        string evalSourceRoot,
        string callerAttestedSourceHash,
        string traversalDocumentId,
        string traversalSourcePath,
        string callerAttestedLabelProvenanceId,
        string patternFingerprint,
        IReadOnlyList<string> tokenFingerprints,
        bool isNewPattern,
        IReadOnlyList<string> seenTrainingTokenFingerprints)
    {
        RowId = rowId;
        EvalSourceRoot = evalSourceRoot;
        CallerAttestedSourceHash = callerAttestedSourceHash;
        TraversalDocumentId = traversalDocumentId;
        TraversalSourcePath = traversalSourcePath;
        CallerAttestedLabelProvenanceId = callerAttestedLabelProvenanceId;
        CallerAttestedPatternFingerprint = patternFingerprint;
        _tokenFingerprints = tokenFingerprints.ToArray();
        IsNewPattern = isNewPattern;
        _seenTrainingTokenFingerprints = seenTrainingTokenFingerprints.ToArray();
    }

    public string RowId { get; }

    public string EvalSourceRoot { get; }

    public string CallerAttestedSourceHash { get; }

    public string TraversalDocumentId { get; }

    public string TraversalSourcePath { get; }

    public string CallerAttestedLabelProvenanceId { get; }

    public string CallerAttestedPatternFingerprint { get; }

    public IReadOnlyList<string> CallerAttestedTokenFingerprints
        => Array.AsReadOnly(_tokenFingerprints.ToArray());

    public bool IsNewPattern { get; }

    public IReadOnlyList<string> SeenTrainingTokenFingerprints
        => Array.AsReadOnly(_seenTrainingTokenFingerprints.ToArray());
}

/// <summary>
/// Deep-owned novelty evidence derived from one validated paired held-out carrier. Fingerprints,
/// source hashes, normalization, and tokenization identifiers remain caller attestations. This
/// result does not independently tokenize, normalize, hash, establish cryptographic provenance,
/// prove generalization or product accuracy, execute targets, train or produce DPO, or decide
/// quality coordination, promotion, cutover, or lifecycle state.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityNoveltyEvaluationResult
{
    public const string FingerprintAlgorithmId = "sha256-lowerhex-v1";
    public const string ProducedMaxAccuracyRuleId = "paired-heldout-top1-max-v1";

    private readonly string[] _requestedRowIds;
    private readonly string[] _missingRowIds;
    private readonly DynamicGraphEdgeAuthorityNoveltyTrainingEvidence[] _trainingEvidence;
    private readonly DynamicGraphEdgeAuthorityNoveltyRowEvidence[] _rows;
    private readonly DynamicGraphEdgeAuthorityAblationEvaluationResult _pairedSelection;
    private readonly DynamicGraphNoveltyObservation _observation;
    private readonly DynamicGraphNoveltyMeasurement _measurement;

    internal DynamicGraphEdgeAuthorityNoveltyEvaluationResult(
        string callerAttestedFingerprintNormalizationId,
        string callerAttestedTokenizationId,
        IReadOnlyList<string> requestedRowIds,
        IReadOnlyList<string> missingRowIds,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingEvidence> trainingEvidence,
        IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowEvidence> rows,
        DynamicGraphEdgeAuthorityAblationEvaluationResult pairedSelection,
        DynamicGraphNoveltyObservation observation,
        DynamicGraphNoveltyMeasurement measurement)
    {
        CallerAttestedFingerprintNormalizationId = callerAttestedFingerprintNormalizationId;
        CallerAttestedTokenizationId = callerAttestedTokenizationId;
        _requestedRowIds = requestedRowIds.ToArray();
        _missingRowIds = missingRowIds.ToArray();
        _trainingEvidence = trainingEvidence.Select(CopyTrainingEvidence).ToArray();
        _rows = rows.Select(CopyRowEvidence).ToArray();
        _pairedSelection = CopyPairedSelection(pairedSelection);
        _observation = CopyObservation(observation);
        _measurement = CopyMeasurement(measurement);
    }

    public string CallerAttestedFingerprintNormalizationId { get; }

    public string CallerAttestedTokenizationId { get; }

    public string MaxAccuracyRuleId => ProducedMaxAccuracyRuleId;

    public IReadOnlyList<string> RequestedRowIds
        => Array.AsReadOnly(_requestedRowIds.ToArray());

    public IReadOnlyList<string> MissingRowIds
        => Array.AsReadOnly(_missingRowIds.ToArray());

    public IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyTrainingEvidence> TrainingEvidence
        => Array.AsReadOnly(_trainingEvidence.Select(CopyTrainingEvidence).ToArray());

    public IReadOnlyList<DynamicGraphEdgeAuthorityNoveltyRowEvidence> Rows
        => Array.AsReadOnly(_rows.Select(CopyRowEvidence).ToArray());

    public DynamicGraphEdgeAuthorityAblationEvaluationResult PairedSelection
        => CopyPairedSelection(_pairedSelection);

    public DynamicGraphNoveltyObservation Observation => CopyObservation(_observation);

    public DynamicGraphNoveltyMeasurement Measurement => CopyMeasurement(_measurement);

    private static DynamicGraphEdgeAuthorityNoveltyTrainingEvidence CopyTrainingEvidence(
        DynamicGraphEdgeAuthorityNoveltyTrainingEvidence source)
        => new(
            source.TrainSourceRoot,
            source.CallerAttestedSourceHash,
            source.CallerAttestedPatternFingerprints,
            source.CallerAttestedTokenFingerprints);

    private static DynamicGraphEdgeAuthorityNoveltyRowEvidence CopyRowEvidence(
        DynamicGraphEdgeAuthorityNoveltyRowEvidence source)
        => new(
            source.RowId,
            source.EvalSourceRoot,
            source.CallerAttestedSourceHash,
            source.TraversalDocumentId,
            source.TraversalSourcePath,
            source.CallerAttestedLabelProvenanceId,
            source.CallerAttestedPatternFingerprint,
            source.CallerAttestedTokenFingerprints,
            source.IsNewPattern,
            source.SeenTrainingTokenFingerprints);

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult CopyPairedSelection(
        DynamicGraphEdgeAuthorityAblationEvaluationResult source)
        => new(
            source.EvaluationId,
            source.EdgeId,
            source.AuthorityUnitId,
            source.SplitProof,
            source.Rows,
            source.LearnedCorrectCount,
            source.LearnedSelectionAccuracy,
            source.BaselineCorrectCount,
            source.BaselineSelectionAccuracy,
            source.AccuracyDrop,
            source.NormalizedAccuracyDrop,
            source.MeetsQ11NumericalGate);

    private static DynamicGraphNoveltyObservation CopyObservation(
        DynamicGraphNoveltyObservation source)
        => new(
            source.EvaluationId,
            source.RequestedRows,
            source.RowsEvaluated,
            source.SelectionShortfall,
            source.NewPatternRate,
            source.SourceTokenSeenRate,
            source.MaxAccuracy,
            CopySplitProof(source.SplitProof));

    private static DynamicGraphNoveltyMeasurement CopyMeasurement(
        DynamicGraphNoveltyMeasurement source)
        => new(
            source.EvaluationId,
            source.RequestedRows,
            source.RowsEvaluated,
            source.SelectionShortfall,
            source.NewPatternRate,
            source.SourceTokenSeenRate,
            source.MaxAccuracy,
            source.MinimumNewPatternRate,
            source.MaximumSeenTokenRate,
            source.MeetsNoveltyGate,
            CopySplitProof(source.SplitProof));

    private static DynamicGraphMeasurementSplitProof CopySplitProof(
        DynamicGraphMeasurementSplitProof source)
        => new(
            Array.AsReadOnly(source.TrainSourceRoots.ToArray()),
            Array.AsReadOnly(source.EvalSourceRoots.ToArray()),
            Array.AsReadOnly(source.SourceHashes.ToArray()),
            source.CrossSplitNearDuplicateCount,
            source.SimhashOrMinhash,
            source.NearDuplicateThreshold);
}

/// <summary>
/// Purely derives novelty evidence from caller-attested atomic fingerprints and a validated paired
/// selection carrier. It performs no I/O, model invocation, tokenization, hashing, target execution,
/// quality coordination, training, DPO, promotion, cutover, or lifecycle transition.
/// </summary>
public static class DynamicGraphEdgeAuthorityNoveltyEvaluationProducer
{
    private const string RequestRequiredCode = "EDGE_AUTHORITY_NOVELTY_REQUEST_REQUIRED";
    private const string RequestRequiredMessage =
        "Dynamic graph edge authority novelty evaluation request is required.";
    private const string EvidenceInvalidCode = "EDGE_AUTHORITY_NOVELTY_EVIDENCE_INVALID";
    private const string EvidenceInvalidMessage =
        "Dynamic graph edge authority novelty evidence is invalid.";
    private const string PairedResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_EVALUATION_RESULT_INVALID";
    private const string PairedResultInvalidMessage =
        "Dynamic graph edge authority ablation evaluation result is invalid.";

    public static OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult> Produce(
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? request)
    {
        if (request is null)
            return Failure(RequestRequiredMessage, RequestRequiredCode);

        DynamicGraphEdgeAuthorityAblationEvaluationResult? pairedSelection;
        DynamicGraphMeasurementSplitProof? splitProof;
        try
        {
            pairedSelection = request.PairedSelection;
            splitProof = pairedSelection?.SplitProof;
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PairedResultInvalid();
        }

        var splitValidation = DynamicGraphHeldOutSplitGuard.Validate(splitProof);
        if (!splitValidation.IsSuccess)
            return splitValidation.MapError<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>();

        if (!DynamicGraphEdgeAuthorityAblationEvaluationRunner.TrySnapshotValidatedResult(
                pairedSelection,
                out var validatedPairedSelection))
        {
            return PairedResultInvalid();
        }

        try
        {
            return TryProduceValidated(request, validatedPairedSelection, out var result)
                ? result
                : EvidenceInvalid();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return EvidenceInvalid();
        }
    }

    private static bool TryProduceValidated(
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest request,
        DynamicGraphEdgeAuthorityAblationEvaluationResult validatedPairedSelection,
        out OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult> result)
    {
        result = null!;
        var fingerprintNormalizationId = request.CallerAttestedFingerprintNormalizationId;
        var tokenizationId = request.CallerAttestedTokenizationId;
        var requestedSource = request.RequestedRowIds;
        var trainingSource = request.TrainingBindings;
        var noveltySource = request.NoveltyRows;
        if (!IsCanonical(fingerprintNormalizationId)
            || !IsCanonical(tokenizationId)
            || requestedSource is null
            || trainingSource is null
            || noveltySource is null)
        {
            return false;
        }

        var requestedRowIds = requestedSource.ToArray();
        var trainingBindings = trainingSource.ToArray();
        var noveltyRows = noveltySource.ToArray();
        if (!IsCanonicalUnique(requestedRowIds))
            return false;

        var splitProof = validatedPairedSelection.SplitProof;
        var pairedRows = validatedPairedSelection.Rows;
        if (trainingBindings.Length != splitProof.TrainSourceRoots.Count
            || noveltyRows.Length != pairedRows.Count)
        {
            return false;
        }

        var globalTrainingPatterns = new HashSet<string>(StringComparer.Ordinal);
        var globalTrainingTokens = new HashSet<string>(StringComparer.Ordinal);
        var hashToPartitionRoot = new Dictionary<string, PartitionRoot>(StringComparer.Ordinal);
        var trainingEvidence = new DynamicGraphEdgeAuthorityNoveltyTrainingEvidence[
            trainingBindings.Length];
        for (var index = 0; index < trainingBindings.Length; index++)
        {
            var binding = trainingBindings[index];
            if (binding is null
                || !StringComparer.Ordinal.Equals(
                    binding.TrainSourceRoot,
                    splitProof.TrainSourceRoots[index])
                || !IsCanonical(binding.CallerAttestedSourceHash)
                || !TrySnapshotFingerprints(
                    binding.CallerAttestedPatternFingerprints,
                    out var patterns)
                || !TrySnapshotFingerprints(
                    binding.CallerAttestedTokenFingerprints,
                    out var tokens)
                || !hashToPartitionRoot.TryAdd(
                    binding.CallerAttestedSourceHash!,
                    new PartitionRoot("train", binding.TrainSourceRoot!)))
            {
                return false;
            }

            globalTrainingPatterns.UnionWith(patterns);
            globalTrainingTokens.UnionWith(tokens);
            trainingEvidence[index] = new DynamicGraphEdgeAuthorityNoveltyTrainingEvidence(
                binding.TrainSourceRoot!,
                binding.CallerAttestedSourceHash!,
                patterns,
                tokens);
        }

        var evalHashByRoot = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in pairedRows)
        {
            if (evalHashByRoot.TryGetValue(row.EvalSourceRoot, out var existingHash))
            {
                if (!StringComparer.Ordinal.Equals(existingHash, row.CallerAttestedSourceHash))
                    return false;
            }
            else
            {
                evalHashByRoot.Add(row.EvalSourceRoot, row.CallerAttestedSourceHash);
            }
        }

        foreach (var evalRoot in splitProof.EvalSourceRoots)
        {
            if (!evalHashByRoot.TryGetValue(evalRoot, out var evalHash)
                || !hashToPartitionRoot.TryAdd(evalHash, new PartitionRoot("eval", evalRoot)))
            {
                return false;
            }
        }

        if (hashToPartitionRoot.Count != splitProof.SourceHashes.Count
            || !hashToPartitionRoot.Keys.ToHashSet(StringComparer.Ordinal)
                .SetEquals(splitProof.SourceHashes))
        {
            return false;
        }

        var missingRowIds = new List<string>();
        var pairedIndex = 0;
        foreach (var requestedRowId in requestedRowIds)
        {
            if (pairedIndex < pairedRows.Count
                && StringComparer.Ordinal.Equals(requestedRowId, pairedRows[pairedIndex].RowId))
            {
                pairedIndex++;
            }
            else
            {
                missingRowIds.Add(requestedRowId);
            }
        }

        if (pairedIndex != pairedRows.Count)
            return false;

        var globalEvalTokens = new HashSet<string>(StringComparer.Ordinal);
        var rowEvidence = new DynamicGraphEdgeAuthorityNoveltyRowEvidence[pairedRows.Count];
        var newPatternCount = 0;
        for (var index = 0; index < pairedRows.Count; index++)
        {
            var pairedRow = pairedRows[index];
            var noveltyRow = noveltyRows[index];
            if (noveltyRow is null
                || !StringComparer.Ordinal.Equals(noveltyRow.RowId, pairedRow.RowId)
                || !IsFingerprint(noveltyRow.CallerAttestedPatternFingerprint)
                || !TrySnapshotFingerprints(
                    noveltyRow.CallerAttestedTokenFingerprints,
                    out var tokens))
            {
                return false;
            }

            var isNewPattern = !globalTrainingPatterns.Contains(
                noveltyRow.CallerAttestedPatternFingerprint!);
            if (isNewPattern)
                newPatternCount++;

            globalEvalTokens.UnionWith(tokens);
            var seenTrainingTokens = tokens
                .Where(globalTrainingTokens.Contains)
                .ToArray();
            rowEvidence[index] = new DynamicGraphEdgeAuthorityNoveltyRowEvidence(
                pairedRow.RowId,
                pairedRow.EvalSourceRoot,
                pairedRow.CallerAttestedSourceHash,
                pairedRow.TraversalDocumentId,
                pairedRow.TraversalSourcePath,
                pairedRow.CallerAttestedLabelProvenanceId,
                noveltyRow.CallerAttestedPatternFingerprint!,
                tokens,
                isNewPattern,
                seenTrainingTokens);
        }

        if (globalEvalTokens.Count == 0)
            return false;

        var seenTokenCount = globalEvalTokens.Count(globalTrainingTokens.Contains);
        var newPatternRate = (double)newPatternCount / pairedRows.Count;
        var sourceTokenSeenRate = (double)seenTokenCount / globalEvalTokens.Count;
        var maxAccuracy = Math.Max(
            validatedPairedSelection.LearnedSelectionAccuracy,
            validatedPairedSelection.BaselineSelectionAccuracy);
        var observation = new DynamicGraphNoveltyObservation(
            validatedPairedSelection.EvaluationId,
            requestedRowIds.Length,
            pairedRows.Count,
            missingRowIds.Count,
            newPatternRate,
            sourceTokenSeenRate,
            maxAccuracy,
            splitProof);
        var measurementResult = DynamicGraphNoveltyMeter.Measure(observation);
        if (!measurementResult.IsSuccess)
        {
            result = measurementResult.MapError<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>();
            return true;
        }

        result = OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>.Ok(
            new DynamicGraphEdgeAuthorityNoveltyEvaluationResult(
                fingerprintNormalizationId!,
                tokenizationId!,
                requestedRowIds,
                missingRowIds,
                trainingEvidence,
                rowEvidence,
                validatedPairedSelection,
                observation,
                measurementResult.Value!));
        return true;
    }

    private static bool TrySnapshotFingerprints(
        IReadOnlyList<string>? source,
        out string[] fingerprints)
    {
        fingerprints = [];
        if (source is null)
            return false;

        var snapshot = source.ToArray();
        if (snapshot.Length == 0)
            return false;

        for (var index = 0; index < snapshot.Length; index++)
        {
            if (!IsFingerprint(snapshot[index])
                || (index > 0
                    && StringComparer.Ordinal.Compare(snapshot[index - 1], snapshot[index]) >= 0))
            {
                return false;
            }
        }

        fingerprints = snapshot;
        return true;
    }

    private static bool IsFingerprint(string? value)
        => value is { Length: 64 }
           && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsCanonicalUnique(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
            return false;

        var unique = new HashSet<string>(StringComparer.Ordinal);
        return values.All(value => IsCanonical(value) && unique.Add(value));
    }

    private static bool IsCanonical(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    private static OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>
        PairedResultInvalid()
        => Failure(PairedResultInvalidMessage, PairedResultInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>
        EvidenceInvalid()
        => Failure(EvidenceInvalidMessage, EvidenceInvalidCode);

    private static OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult> Failure(
        string message,
        string code)
        => OperationResult<DynamicGraphEdgeAuthorityNoveltyEvaluationResult>.Fail(message, code);

    private sealed record PartitionRoot(string Partition, string Root);
}
