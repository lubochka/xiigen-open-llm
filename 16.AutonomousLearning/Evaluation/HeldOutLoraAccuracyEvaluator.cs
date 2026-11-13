// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace AutonomousLearning;

using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed record HeldOutEvaluationCase(
    string CaseId,
    string SourceId,
    string Prompt,
    string ExpectedResponse);

public sealed record SourceDisjointHeldOutEvaluationSet(
    string Domain,
    IReadOnlyList<HeldOutEvaluationCase> Cases);

public sealed record HeldOutEvaluationLimits(
    int MaxDomains = 64,
    int MaxTrainingSources = 100_000,
    int MaxCases = 10_000,
    int MaxCaseIdChars = 256,
    int MaxPromptChars = 100_000,
    int MaxResponseChars = 100_000,
    long MaxCorpusChars = 100_000_000,
    long MaxBaseCheckpointBytes = 16L * 1024 * 1024 * 1024,
    long MaxTokenizerBytes = 512L * 1024 * 1024,
    long MaxAdapterBytes = 2L * 1024 * 1024 * 1024,
    long MaxAggregateArtifactBytes = 18L * 1024 * 1024 * 1024,
    int MaxVocabSize = 1_000_000,
    int MaxContextLength = 1_000_000,
    int MaxHiddenDimension = 65_536,
    int MaxLayers = 1_024,
    long MaxEstimatedParameters = 20_000_000_000,
    int MaxLoraRank = 4_096,
    int MaxLoraTargets = 4,
    long MaxEstimatedAdapterParameters = 2_000_000_000,
    long MaxLogitsElements = 100_000_000,
    long MaxLogitsBytes = 400_000_000,
    long MaxActivationElements = 500_000_000,
    long MaxOperationBudget = 1_000_000_000_000,
    long MaxActivationBytes = 2_000_000_000,
    long MaxCumulativeLogitsElements = 1_000_000_000,
    long MaxCumulativeLogitsBytes = 4_000_000_000,
    long MaxCumulativeActivationElements = 5_000_000_000,
    long MaxCumulativeActivationBytes = 20_000_000_000,
    long MaxCumulativeOperationBudget = 10_000_000_000_000);

internal enum HeldOutEvaluationProbePhase
{
    BeforeModelConstruction,
    BeforeAdapterApply,
    BeforeAdapterLoad,
    BeforeForward,
    BeforeArgMax
}

internal sealed record HeldOutCaseComputeEstimate(
    long LogitsElements,
    long LogitsBytes,
    long ActivationElements,
    long ActivationBytes,
    long Operations);

public interface IHeldOutEvaluationSetProvider
{
    OperationResult<SourceDisjointHeldOutEvaluationSet> Get(string domain);
}

/// <summary>
/// Snapshots configured cases. The opt-in ModelServer promotion-readiness
/// composition wires this production data boundary for inference-only evidence;
/// it does not train, activate, publish, or authorize product readiness.
/// </summary>
public sealed class ConfiguredHeldOutEvaluationSetProvider
    : IHeldOutEvaluationSetProvider
{
    private readonly IReadOnlyDictionary<string, SourceDisjointHeldOutEvaluationSet> _sets;

    public ConfiguredHeldOutEvaluationSetProvider(
        IEnumerable<SourceDisjointHeldOutEvaluationSet> sets,
        HeldOutEvaluationLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(sets);
        var appliedLimits = limits ?? new HeldOutEvaluationLimits();
        ValidateLimits(appliedLimits);
        var supplied = sets.Take(checked(appliedLimits.MaxDomains + 1)).ToArray();
        if (supplied.Length > appliedLimits.MaxDomains)
            throw new ArgumentException("HELD_OUT_DOMAIN_LIMIT_EXCEEDED", nameof(sets));

        var snapshots = new Dictionary<string, SourceDisjointHeldOutEvaluationSet>(
            StringComparer.Ordinal);
        foreach (var set in supplied)
        {
            ArgumentNullException.ThrowIfNull(set);
            if (string.IsNullOrWhiteSpace(set.Domain)
                || !StringComparer.Ordinal.Equals(set.Domain, set.Domain.Trim()))
            {
                throw new ArgumentException("HELD_OUT_DOMAIN_NONCANONICAL", nameof(sets));
            }

            if (!snapshots.TryAdd(
                    set.Domain,
                    set with
                    {
                        Cases = Array.AsReadOnly(
                            set.Cases?.ToArray()
                            ?? Array.Empty<HeldOutEvaluationCase>()),
                    }))
            {
                throw new ArgumentException("HELD_OUT_DOMAIN_DUPLICATE", nameof(sets));
            }
        }

        _sets = snapshots;
    }

    public OperationResult<SourceDisjointHeldOutEvaluationSet> Get(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return OperationResult<SourceDisjointHeldOutEvaluationSet>.Fail(
                "Held-out evaluation domain is required.",
                "HELD_OUT_DOMAIN_REQUIRED");
        }

        return _sets.TryGetValue(domain, out var set)
            ? OperationResult<SourceDisjointHeldOutEvaluationSet>.Ok(set)
            : OperationResult<SourceDisjointHeldOutEvaluationSet>.Fail(
                "No held-out evaluation set is configured for the requested domain.",
                "HELD_OUT_SET_NOT_FOUND");
    }

    internal static void ValidateLimits(HeldOutEvaluationLimits limits)
    {
        ArgumentNullException.ThrowIfNull(limits);
        if (limits.MaxDomains <= 0
            || limits.MaxTrainingSources <= 0
            || limits.MaxCases <= 0
            || limits.MaxCaseIdChars <= 0
            || limits.MaxPromptChars <= 0
            || limits.MaxResponseChars <= 0
            || limits.MaxCorpusChars <= 0
            || limits.MaxBaseCheckpointBytes <= 0
            || limits.MaxTokenizerBytes <= 0
            || limits.MaxAdapterBytes <= 0
            || limits.MaxAggregateArtifactBytes <= 0
            || limits.MaxVocabSize < 256
            || limits.MaxContextLength <= 0
            || limits.MaxHiddenDimension <= 0
            || limits.MaxLayers <= 0
            || limits.MaxEstimatedParameters <= 0
            || limits.MaxLoraRank <= 0
            || limits.MaxLoraTargets <= 0
            || limits.MaxEstimatedAdapterParameters <= 0
            || limits.MaxLogitsElements <= 0
            || limits.MaxLogitsBytes <= 0
            || limits.MaxActivationElements <= 0
            || limits.MaxOperationBudget <= 0
            || limits.MaxActivationBytes <= 0
            || limits.MaxCumulativeLogitsElements <= 0
            || limits.MaxCumulativeLogitsBytes <= 0
            || limits.MaxCumulativeActivationElements <= 0
            || limits.MaxCumulativeActivationBytes <= 0
            || limits.MaxCumulativeOperationBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limits));
        }
    }
}

/// <summary>
/// Provenance-bound held-out evaluator implementation. The opt-in ModelServer
/// readiness composition invokes it for evidence only; the evaluator does not
/// train, activate, publish, or by itself authorize product readiness.
/// </summary>
public sealed class HeldOutLoraAccuracyEvaluator : IAccuracyEvaluator
{
    private readonly ModelConfig _modelConfig;
    private readonly ModelConfigurationIdentity _modelIdentity;
    private readonly string _baseCheckpointPath;
    private readonly string _tokenizerPath;
    private readonly IHeldOutEvaluationSetProvider _sets;
    private readonly LoraConfig _loraConfig;
    private readonly LoraConfigurationIdentity _loraIdentity;
    private readonly HeldOutEvaluationLimits _limits;
    private readonly Action<HeldOutEvaluationProbePhase>? _phaseProbe;
    private readonly Func<FileStream, string>? _identityResolver;
    private readonly Action<LlmModel>? _beforeForwardModelObserver;

    public HeldOutLoraAccuracyEvaluator(
        ModelConfig modelConfig,
        string baseCheckpointPath,
        string tokenizerPath,
        IHeldOutEvaluationSetProvider sets,
        LoraConfig? loraConfig = null,
        HeldOutEvaluationLimits? limits = null)
        : this(
            modelConfig,
            baseCheckpointPath,
            tokenizerPath,
            sets,
            loraConfig,
            limits,
            phaseProbe: null,
            identityResolver: null,
            beforeForwardModelObserver: null)
    {
    }

    internal HeldOutLoraAccuracyEvaluator(
        ModelConfig modelConfig,
        string baseCheckpointPath,
        string tokenizerPath,
        IHeldOutEvaluationSetProvider sets,
        LoraConfig? loraConfig,
        HeldOutEvaluationLimits? limits,
        Action<HeldOutEvaluationProbePhase>? phaseProbe,
        Func<FileStream, string>? identityResolver = null,
        Action<LlmModel>? beforeForwardModelObserver = null)
    {
        _modelConfig = modelConfig ?? throw new ArgumentNullException(nameof(modelConfig));
        _baseCheckpointPath = baseCheckpointPath
            ?? throw new ArgumentNullException(nameof(baseCheckpointPath));
        _tokenizerPath = tokenizerPath
            ?? throw new ArgumentNullException(nameof(tokenizerPath));
        _sets = sets ?? throw new ArgumentNullException(nameof(sets));
        _loraConfig = LoraApplicator.ValidateAndCanonicalize(
            loraConfig ?? IncrementalTrainer.DefaultConfig);
        _modelIdentity = TrainingSourceProvenance.Identify(_modelConfig);
        _loraIdentity = TrainingSourceProvenance.Identify(_loraConfig);
        _limits = limits ?? new HeldOutEvaluationLimits();
        _phaseProbe = phaseProbe;
        _identityResolver = identityResolver;
        _beforeForwardModelObserver = beforeForwardModelObserver;
        ConfiguredHeldOutEvaluationSetProvider.ValidateLimits(_limits);
    }

    public OperationResult<AccuracyEvaluationResult> Evaluate(
        AccuracyEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var requestFailure = ValidateRequest(request);
        if (requestFailure is not null)
            return requestFailure;

        var setResult = _sets.Get(request.Domain);
        if (!setResult.IsSuccess || setResult.Value is null)
            return setResult.MapError<AccuracyEvaluationResult>();

        var snapshotResult = SnapshotSet(setResult.Value);
        if (!snapshotResult.IsSuccess || snapshotResult.Value is null)
            return snapshotResult.MapError<AccuracyEvaluationResult>();
        var evaluationSet = snapshotResult.Value;

        var setFailure = ValidateSet(request, evaluationSet);
        if (setFailure is not null)
            return setFailure;

        cancellationToken.ThrowIfCancellationRequested();
        var baseResult = HeldArtifact.Open(
            _baseCheckpointPath,
            _limits.MaxBaseCheckpointBytes,
            "BASE_CHECKPOINT",
            cancellationToken,
            _identityResolver);
        if (!baseResult.IsSuccess || baseResult.Value is null)
            return baseResult.MapError<AccuracyEvaluationResult>();
        using var baseArtifact = baseResult.Value;

        var tokenizerResult = HeldArtifact.Open(
            _tokenizerPath,
            _limits.MaxTokenizerBytes,
            "TOKENIZER_CHECKPOINT",
            cancellationToken,
            _identityResolver);
        if (!tokenizerResult.IsSuccess || tokenizerResult.Value is null)
            return tokenizerResult.MapError<AccuracyEvaluationResult>();
        using var tokenizerArtifact = tokenizerResult.Value;

        HeldArtifact? adapterArtifact = null;
        try
        {
            if (request.TargetAdapter is not null)
            {
                var adapterResult = HeldArtifact.Open(
                    request.TargetAdapter.AdapterPath,
                    _limits.MaxAdapterBytes,
                    "ADAPTER_CHECKPOINT",
                    cancellationToken,
                    _identityResolver);
                if (!adapterResult.IsSuccess || adapterResult.Value is null)
                    return adapterResult.MapError<AccuracyEvaluationResult>();
                adapterArtifact = adapterResult.Value;
            }

            long aggregateBytes;
            try
            {
                aggregateBytes = checked(
                    baseArtifact.Length
                    + tokenizerArtifact.Length
                    + (adapterArtifact?.Length ?? 0));
            }
            catch (OverflowException)
            {
                return Fail("Artifact size arithmetic overflowed.", "ARTIFACT_SIZE_OVERFLOW");
            }

            if (aggregateBytes > _limits.MaxAggregateArtifactBytes)
                return Fail("Aggregate artifact size exceeds the configured limit.", "ARTIFACT_AGGREGATE_LIMIT_EXCEEDED");

            var artifacts = new AccuracyEvaluationArtifactIdentity(
                baseArtifact.Sha256,
                tokenizerArtifact.Sha256,
                _modelIdentity.Sha256);
            var identityFailure = ValidateArtifactIdentity(
                request,
                artifacts,
                adapterArtifact?.Sha256);
            if (identityFailure is not null)
                return identityFailure;

            cancellationToken.ThrowIfCancellationRequested();
            var modelBoundFailure = ValidateModelBounds(
                _modelConfig,
                _loraConfig,
                request.TargetAdapter is not null,
                _limits);
            if (modelBoundFailure is not null)
                return modelBoundFailure;

            tokenizerArtifact.Rewind();
            var tokenizer = BpeTokenizer.Load(tokenizerArtifact.Stream);
            if (tokenizer.VocabSize < 256 || tokenizer.VocabSize > _modelConfig.VocabSize)
                return Fail("Tokenizer and model vocabularies are incompatible.", "HELD_OUT_TOKENIZER_INCOMPATIBLE");

            var preparedCasesResult = PrepareCases(
                tokenizer,
                evaluationSet.Cases,
                request.TargetAdapter is null ? null : _loraConfig,
                cancellationToken);
            if (!preparedCasesResult.IsSuccess || preparedCasesResult.Value is null)
                return preparedCasesResult.MapError<AccuracyEvaluationResult>();
            var preparedCases = preparedCasesResult.Value;

            // Tokenization and all per-case plus cumulative compute checks must
            // finish before any model or adapter allocation can occur.
            cancellationToken.ThrowIfCancellationRequested();
            if (!baseArtifact.VerifyUnchanged(cancellationToken)
                || !tokenizerArtifact.VerifyUnchanged(cancellationToken)
                || (adapterArtifact is not null
                    && !adapterArtifact.VerifyUnchanged(cancellationToken)))
            {
                return Fail("A held artifact changed during preflight.", "HELD_ARTIFACT_CHANGED");
            }

            _phaseProbe?.Invoke(HeldOutEvaluationProbePhase.BeforeModelConstruction);
            cancellationToken.ThrowIfCancellationRequested();
            baseArtifact.Rewind();
            var model = new LlmModel(_modelConfig);
            model.LoadCheckpoint(baseArtifact.Stream);
            cancellationToken.ThrowIfCancellationRequested();

            if (adapterArtifact is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _phaseProbe?.Invoke(HeldOutEvaluationProbePhase.BeforeAdapterApply);
                cancellationToken.ThrowIfCancellationRequested();
                LoraApplicator.Apply(model, _loraConfig);
                adapterArtifact.Rewind();
                _phaseProbe?.Invoke(HeldOutEvaluationProbePhase.BeforeAdapterLoad);
                cancellationToken.ThrowIfCancellationRequested();
                model.LoadAdapterCheckpoint(adapterArtifact.Stream);
            }

            // Evaluation is inference. Disabling gradients prevents the
            // autograd graph from retaining every projection workspace and
            // makes the preflight allocation formula authoritative.
            foreach (var parameter in model.Parameters())
                parameter.Value.RequiresGrad = false;

            long correctTokens = 0;
            long scoredTokens = 0;
            foreach (var evaluationCase in preparedCases)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var caseResult = ScoreCase(
                    model,
                    evaluationCase,
                    cancellationToken);
                if (!caseResult.IsSuccess || caseResult.Value is null)
                    return caseResult.MapError<AccuracyEvaluationResult>();
                try
                {
                    correctTokens = checked(correctTokens + caseResult.Value.CorrectTokens);
                    scoredTokens = checked(scoredTokens + caseResult.Value.ScoredTokens);
                }
                catch (OverflowException)
                {
                    return Fail("Token-count arithmetic overflowed.", "HELD_OUT_TOKEN_COUNT_OVERFLOW");
                }
            }

            if (scoredTokens <= 0)
                return Fail("Held-out evaluation produced no scored tokens.", "HELD_OUT_NO_SCORED_TOKENS");

            cancellationToken.ThrowIfCancellationRequested();
            if (!baseArtifact.VerifyUnchanged(cancellationToken)
                || !tokenizerArtifact.VerifyUnchanged(cancellationToken)
                || (adapterArtifact is not null
                    && !adapterArtifact.VerifyUnchanged(cancellationToken)))
            {
                return Fail("A held artifact changed during evaluation.", "HELD_ARTIFACT_CHANGED");
            }

            var score = (double)correctTokens / scoredTokens;
            if (!double.IsFinite(score) || score is < 0.0 or > 1.0)
                return Fail("Held-out evaluation produced an invalid score.", "INVALID_ACCURACY_SCORE");

            return OperationResult<AccuracyEvaluationResult>.Ok(
                new AccuracyEvaluationResult(
                    score,
                    artifacts,
                    adapterArtifact?.Sha256,
                    request.PlannedTrainingSources.Sha256));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail("Held-out model evaluation failed.", "HELD_OUT_EVALUATION_FAILED");
        }
        finally
        {
            adapterArtifact?.Dispose();
        }
    }

    private OperationResult<AccuracyEvaluationResult>? ValidateRequest(
        AccuracyEvaluationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Domain)
            || !StringComparer.Ordinal.Equals(request.Domain, request.Domain.Trim()))
            return Fail("Evaluation domain is non-canonical.", "HELD_OUT_DOMAIN_NONCANONICAL");

        if (request.PlannedTrainingSources is null
            || request.PlannedTrainingSources.SourceIds is null
            || request.PlannedTrainingSources.SourceIds.Count > _limits.MaxTrainingSources)
            return Fail("Training source manifest is missing or too large.", "TRAINING_SOURCE_MANIFEST_INVALID");

        var canonicalManifest = TrainingSourceProvenance.BuildManifest(
            request.PlannedTrainingSources.SourceIds);
        if (!canonicalManifest.IsSuccess || canonicalManifest.Value is null)
            return canonicalManifest.MapError<AccuracyEvaluationResult>();
        if (!StringComparer.Ordinal.Equals(
                canonicalManifest.Value.Sha256,
                request.PlannedTrainingSources.Sha256))
            return Fail("Training source manifest hash does not match its source ids.", "TRAINING_SOURCE_MANIFEST_HASH_MISMATCH");

        if (request.Phase == AccuracyEvaluationPhase.Candidate
            && request.TargetAdapter is null)
            return Fail("Candidate evaluation requires an adapter.", "CANDIDATE_ADAPTER_REQUIRED");
        if (request.Phase == AccuracyEvaluationPhase.Baseline
            && request.ExpectedArtifacts is not null)
            return Fail("Baseline evaluation cannot accept caller-supplied artifact identity.", "BASELINE_ARTIFACT_IDENTITY_UNEXPECTED");
        if (request.Phase == AccuracyEvaluationPhase.Candidate
            && request.ExpectedArtifacts is null)
            return Fail("Candidate evaluation requires baseline artifact identity.", "BASELINE_ARTIFACT_IDENTITY_REQUIRED");

        if (request.TargetAdapter is null)
            return null;
        if (!StringComparer.Ordinal.Equals(request.Domain, request.TargetAdapter.Domain))
            return Fail("Adapter domain does not match evaluation domain.", "ADAPTER_DOMAIN_MISMATCH");
        if (request.TargetAdapter.Provenance is null)
            return Fail("Adapter provenance is required.", "ADAPTER_PROVENANCE_REQUIRED");

        var provenance = request.TargetAdapter.Provenance;
        if (provenance.TrainingSources is null
            || provenance.Model is null
            || provenance.Lora is null
            || provenance.Lora.OrderedTargetModules is null)
            return Fail("Adapter provenance is invalid.", "ADAPTER_PROVENANCE_MISMATCH");
        var targetManifest = TrainingSourceProvenance.BuildManifest(
            provenance.TrainingSources.SourceIds);
        if (!targetManifest.IsSuccess || targetManifest.Value is null
            || !StringComparer.Ordinal.Equals(
                targetManifest.Value.Sha256,
                provenance.TrainingSources.Sha256))
            return Fail("Adapter training-source provenance is invalid.", "ADAPTER_PROVENANCE_MISMATCH");

        var planned = new HashSet<string>(
            request.PlannedTrainingSources.SourceIds,
            StringComparer.Ordinal);
        if (provenance.TrainingSources.SourceIds.Any(source => !planned.Contains(source)))
            return Fail("Planned training sources omit adapter training provenance.", "ADAPTER_TRAINING_SOURCE_UNDECLARED");
        if (request.Phase == AccuracyEvaluationPhase.Candidate
            && !StringComparer.Ordinal.Equals(
                provenance.TrainingSources.Sha256,
                request.PlannedTrainingSources.Sha256))
            return Fail("Candidate training provenance does not match the actual batch.", "CANDIDATE_TRAINING_MANIFEST_MISMATCH");

        if (!ModelIdentityEquals(provenance.Model, _modelIdentity)
            || !LoraIdentityEquals(provenance.Lora, _loraIdentity))
            return Fail("Adapter model or LoRA identity is incompatible.", "ADAPTER_CONFIGURATION_MISMATCH");
        if (!TrainingSourceProvenance.IsCanonicalSha256(provenance.AdapterSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(provenance.BaseCheckpointSha256)
            || !TrainingSourceProvenance.IsCanonicalSha256(provenance.TokenizerSha256))
            return Fail("Adapter artifact hashes are non-canonical.", "ADAPTER_PROVENANCE_MISMATCH");

        return null;
    }

    private OperationResult<AccuracyEvaluationResult>? ValidateSet(
        AccuracyEvaluationRequest request,
        SourceDisjointHeldOutEvaluationSet set)
    {
        if (!StringComparer.Ordinal.Equals(request.Domain, set.Domain))
            return Fail("Held-out set domain does not match request.", "HELD_OUT_DOMAIN_MISMATCH");
        if (set.Cases is null || set.Cases.Count == 0)
            return Fail("Held-out set is empty.", "HELD_OUT_SET_EMPTY");
        if (set.Cases.Count > _limits.MaxCases)
            return Fail("Held-out case count exceeds the configured limit.", "HELD_OUT_CASE_LIMIT_EXCEEDED");

        var trainingOrigins = new HashSet<string>(StringComparer.Ordinal);
        var trainingContents = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in request.PlannedTrainingSources.SourceIds)
        {
            if (!TrainingSourceProvenance.TryParseSourceId(source, out var identity)
                || identity is null)
                return Fail("Training source identity is malformed.", "TRAINING_SOURCE_ID_NONCANONICAL");
            trainingOrigins.Add(identity.OriginSha256);
            trainingContents.Add(identity.ContentSha256);
        }
        var caseIds = new HashSet<string>(StringComparer.Ordinal);
        var heldOutOrigins = new HashSet<string>(StringComparer.Ordinal);
        var heldOutContents = new HashSet<string>(StringComparer.Ordinal);
        long corpusChars = 0;
        foreach (var evaluationCase in set.Cases)
        {
            if (evaluationCase is null
                || string.IsNullOrWhiteSpace(evaluationCase.CaseId)
                || !StringComparer.Ordinal.Equals(evaluationCase.CaseId, evaluationCase.CaseId.Trim())
                || evaluationCase.CaseId.Length > _limits.MaxCaseIdChars
                || !TrainingSourceProvenance.IsCanonicalSourceId(evaluationCase.SourceId)
                || string.IsNullOrWhiteSpace(evaluationCase.Prompt)
                || string.IsNullOrWhiteSpace(evaluationCase.ExpectedResponse)
                || evaluationCase.Prompt.Length > _limits.MaxPromptChars
                || evaluationCase.ExpectedResponse.Length > _limits.MaxResponseChars)
                return Fail("Held-out case is invalid or non-canonical.", "HELD_OUT_CASE_INVALID");

            if (!TrainingSourceProvenance.TryParseSourceId(
                    evaluationCase.SourceId,
                    out var heldIdentity)
                || heldIdentity is null)
                return Fail("Held-out source identity is malformed.", "HELD_OUT_CASE_INVALID");
            if (!caseIds.Add(evaluationCase.CaseId))
                return Fail("Held-out case ids must be unique.", "HELD_OUT_CASE_ID_DUPLICATE");
            if (!heldOutOrigins.Add(heldIdentity.OriginSha256)
                || !heldOutContents.Add(heldIdentity.ContentSha256))
                return Fail("Held-out origin and content identities must be unique.", "HELD_OUT_SOURCE_ID_DUPLICATE");
            if (trainingOrigins.Contains(heldIdentity.OriginSha256)
                || trainingContents.Contains(heldIdentity.ContentSha256))
                return Fail("Held-out source overlaps the actual training batch.", "HELD_OUT_SOURCE_OVERLAP");
            try
            {
                corpusChars = checked(
                    corpusChars
                    + evaluationCase.CaseId.Length
                    + evaluationCase.SourceId.Length
                    + evaluationCase.Prompt.Length
                    + evaluationCase.ExpectedResponse.Length);
            }
            catch (OverflowException)
            {
                return Fail("Held-out corpus size overflowed.", "HELD_OUT_CORPUS_SIZE_OVERFLOW");
            }
            if (corpusChars > _limits.MaxCorpusChars)
                return Fail("Held-out corpus exceeds the configured limit.", "HELD_OUT_CORPUS_LIMIT_EXCEEDED");
        }
        return null;
    }

    private OperationResult<SourceDisjointHeldOutEvaluationSet> SnapshotSet(
        SourceDisjointHeldOutEvaluationSet set)
    {
        if (set.Cases is null)
            return OperationResult<SourceDisjointHeldOutEvaluationSet>.Fail(
                "Held-out set cases are missing.",
                "HELD_OUT_SET_EMPTY");

        try
        {
            var cases = new List<HeldOutEvaluationCase>();
            foreach (var item in set.Cases)
            {
                if (cases.Count >= _limits.MaxCases)
                    return OperationResult<SourceDisjointHeldOutEvaluationSet>.Fail(
                        "Held-out case count exceeds the configured limit.",
                        "HELD_OUT_CASE_LIMIT_EXCEEDED");
                cases.Add(item is null
                    ? null!
                    : new HeldOutEvaluationCase(
                        string.Concat(item.CaseId),
                        string.Concat(item.SourceId),
                        string.Concat(item.Prompt),
                        string.Concat(item.ExpectedResponse)));
            }
            return OperationResult<SourceDisjointHeldOutEvaluationSet>.Ok(
                new SourceDisjointHeldOutEvaluationSet(
                    string.Concat(set.Domain),
                    Array.AsReadOnly(cases.ToArray())));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<SourceDisjointHeldOutEvaluationSet>.Fail(
                "Held-out set could not be materialized.",
                "HELD_OUT_SET_SNAPSHOT_FAILED");
        }
    }

    private OperationResult<AccuracyEvaluationResult>? ValidateArtifactIdentity(
        AccuracyEvaluationRequest request,
        AccuracyEvaluationArtifactIdentity actual,
        string? adapterSha256)
    {
        if (request.ExpectedArtifacts is not null
            && request.ExpectedArtifacts != actual)
            return Fail("Base/tokenizer/model identity changed since baseline.", "EVALUATION_ARTIFACT_IDENTITY_CHANGED");

        if (request.TargetAdapter?.Provenance is not { } provenance)
            return null;
        if (!StringComparer.Ordinal.Equals(provenance.AdapterSha256, adapterSha256)
            || !StringComparer.Ordinal.Equals(provenance.BaseCheckpointSha256, actual.BaseCheckpointSha256)
            || !StringComparer.Ordinal.Equals(provenance.TokenizerSha256, actual.TokenizerSha256)
            || !StringComparer.Ordinal.Equals(provenance.Model.Sha256, actual.ModelConfigurationSha256))
            return Fail("Adapter provenance does not match held artifact bytes.", "ADAPTER_PROVENANCE_MISMATCH");
        return null;
    }

    private OperationResult<IReadOnlyList<PreparedEvaluationCase>> PrepareCases(
        BpeTokenizer tokenizer,
        IReadOnlyList<HeldOutEvaluationCase> evaluationCases,
        LoraConfig? candidateLora,
        CancellationToken cancellationToken)
    {
        var prepared = new List<PreparedEvaluationCase>(evaluationCases.Count);
        var cumulative = new HeldOutCaseComputeEstimate(0, 0, 0, 0, 0);
        foreach (var evaluationCase in evaluationCases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var prefix = tokenizer.Encode(string.Concat(evaluationCase.Prompt, "\n")).ToArray();
            cancellationToken.ThrowIfCancellationRequested();
            var sequence = tokenizer.Encode(string.Concat(
                evaluationCase.Prompt,
                "\n",
                evaluationCase.ExpectedResponse)).ToArray();
            if (prefix.Length == 0
                || sequence.Length <= prefix.Length
                || !sequence.AsSpan(0, prefix.Length).SequenceEqual(prefix))
            {
                return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Fail(
                    "Held-out response tokenization is invalid.",
                    "HELD_OUT_CASE_TOKENIZATION_INVALID");
            }

            var inputCount = sequence.Length - 1;
            if (inputCount > _modelConfig.ContextLen)
            {
                return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Fail(
                    "Held-out case exceeds context.",
                    "HELD_OUT_CASE_CONTEXT_EXCEEDED");
            }
            if (sequence.Any(token => token < 0 || token >= _modelConfig.VocabSize))
            {
                return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Fail(
                    "Held-out token is outside vocabulary.",
                    "HELD_OUT_TOKEN_OUT_OF_RANGE");
            }

            HeldOutCaseComputeEstimate estimate;
            try
            {
                estimate = EstimateCaseCompute(
                    _modelConfig,
                    inputCount,
                    candidateLora);
                cumulative = AddComputeEstimates(cumulative, estimate);
            }
            catch (OverflowException)
            {
                return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Fail(
                    "Held-out compute arithmetic overflowed.",
                    "HELD_OUT_COMPUTE_LIMIT_EXCEEDED");
            }

            if (!WithinPerCaseLimits(estimate, _limits)
                || !WithinCumulativeLimits(cumulative, _limits))
            {
                return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Fail(
                    "Held-out evaluation exceeds the configured compute budget.",
                    "HELD_OUT_COMPUTE_LIMIT_EXCEEDED");
            }

            prepared.Add(new PreparedEvaluationCase(
                evaluationCase.CaseId,
                sequence,
                prefix.Length,
                inputCount));
        }

        return OperationResult<IReadOnlyList<PreparedEvaluationCase>>.Ok(
            Array.AsReadOnly(prepared.ToArray()));
    }

    private OperationResult<TokenScore> ScoreCase(
        LlmModel model,
        PreparedEvaluationCase evaluationCase,
        CancellationToken cancellationToken)
    {
        var sequence = evaluationCase.Sequence;
        var inputCount = evaluationCase.InputCount;

        var values = new float[inputCount];
        for (var index = 0; index < inputCount; index++)
            values[index] = sequence[index];
        cancellationToken.ThrowIfCancellationRequested();
        _beforeForwardModelObserver?.Invoke(model);
        cancellationToken.ThrowIfCancellationRequested();
        _phaseProbe?.Invoke(HeldOutEvaluationProbePhase.BeforeForward);
        cancellationToken.ThrowIfCancellationRequested();
        var logits = model.Forward(
            new Tensor(values, new TensorShape(1, inputCount)),
            training: false,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var vocabSize = model.Config.VocabSize;
        if (logits.Data.LongLength != checked((long)inputCount * vocabSize))
            return OperationResult<TokenScore>.Fail("Unexpected logits shape.", "HELD_OUT_LOGITS_SHAPE_INVALID");

        long correct = 0;
        long scored = 0;
        for (var targetIndex = evaluationCase.PrefixCount; targetIndex < sequence.Length; targetIndex++)
        {
            _phaseProbe?.Invoke(HeldOutEvaluationProbePhase.BeforeArgMax);
            var predicted = ArgMaxFinite(
                logits.Data,
                checked((targetIndex - 1) * vocabSize),
                vocabSize,
                cancellationToken);
            if (predicted < 0)
                return OperationResult<TokenScore>.Fail("Model returned non-finite logits.", "HELD_OUT_LOGITS_INVALID");
            if (predicted == sequence[targetIndex])
                correct++;
            scored++;
        }
        return OperationResult<TokenScore>.Ok(new TokenScore(correct, scored));
    }

    private static int ArgMaxFinite(
        float[] values,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        var bestIndex = -1;
        var bestValue = float.NegativeInfinity;
        for (var index = 0; index < count; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            var value = values[offset + index];
            if (!float.IsFinite(value))
                return -1;
            if (bestIndex < 0 || value > bestValue)
            {
                bestValue = value;
                bestIndex = index;
            }
        }
        return bestIndex;
    }

    private static OperationResult<AccuracyEvaluationResult>? ValidateModelBounds(
        ModelConfig config,
        LoraConfig loraConfig,
        bool includeLora,
        HeldOutEvaluationLimits limits)
    {
        if (config.VocabSize < 256 || config.VocabSize > limits.MaxVocabSize
            || config.ContextLen <= 0 || config.ContextLen > limits.MaxContextLength
            || config.HiddenDim <= 0 || config.HiddenDim > limits.MaxHiddenDimension
            || config.NumLayers <= 0 || config.NumLayers > limits.MaxLayers
            || config.NumHeads <= 0 || config.NumKvHeads <= 0
            || config.HiddenDim % config.NumHeads != 0
            || config.NumHeads % config.NumKvHeads != 0
            || config.FfnHiddenDim <= 0)
            return Fail("Evaluation model dimensions exceed configured limits.", "HELD_OUT_MODEL_LIMIT_EXCEEDED");
        try
        {
            var hidden = (long)config.HiddenDim;
            var kvWidth = checked((long)config.NumKvHeads * config.HeadDim);
            var perLayer = checked(
                2L * hidden
                + 2L * hidden * hidden
                + 2L * hidden * kvWidth
                + 3L * hidden * config.FfnHiddenDim);
            var estimated = checked(
                2L * config.VocabSize * hidden
                + hidden
                + perLayer * config.NumLayers);
            if (estimated > limits.MaxEstimatedParameters)
                return Fail("Evaluation model parameter count exceeds the configured limit.", "HELD_OUT_MODEL_LIMIT_EXCEEDED");

            if (!includeLora)
                return null;
            if (loraConfig.Rank <= 0
                || loraConfig.Rank > limits.MaxLoraRank
                || loraConfig.TargetModules.Length > limits.MaxLoraTargets)
            {
                return Fail("LoRA configuration exceeds the configured limit.", "HELD_OUT_LORA_LIMIT_EXCEEDED");
            }

            long perLayerAdapter = 0;
            foreach (var target in loraConfig.TargetModules)
            {
                var outputWidth = target is "k_proj" or "v_proj"
                    ? kvWidth
                    : hidden;
                perLayerAdapter = checked(
                    perLayerAdapter
                    + (long)loraConfig.Rank * (hidden + outputWidth));
            }
            var adapterParameters = checked(perLayerAdapter * config.NumLayers);
            if (adapterParameters > limits.MaxEstimatedAdapterParameters
                || checked(estimated + adapterParameters) > limits.MaxEstimatedParameters)
            {
                return Fail("LoRA parameter count exceeds the configured limit.", "HELD_OUT_LORA_LIMIT_EXCEEDED");
            }
            return null;
        }
        catch (OverflowException)
        {
            return Fail("Model resource arithmetic overflowed.", "HELD_OUT_MODEL_LIMIT_EXCEEDED");
        }
    }

    internal static HeldOutCaseComputeEstimate EstimateCaseCompute(
        ModelConfig config,
        int sequenceLength,
        LoraConfig? candidateLora)
    {
        var tokens = (long)sequenceLength;
        var hidden = (long)config.HiddenDim;
        var heads = (long)config.NumHeads;
        var kvWidth = checked((long)config.NumKvHeads * config.HeadDim);
        var logits = checked(tokens * config.VocabSize);
        var logitsBytes = checked(logits * sizeof(float));

        var attentionMatrix = checked(heads * tokens * tokens);
        var perLayerActivations = checked(
            5L * tokens * hidden
            + 2L * tokens * kvWidth
            + 2L * attentionMatrix
            + 3L * tokens * config.FfnHiddenDim);
        long loraAllocatedElementsPerLayer = 0;
        long loraOperationsPerLayer = 0;
        if (candidateLora is not null)
        {
            foreach (var target in candidateLora.TargetModules)
            {
                var outputWidth = target is "k_proj" or "v_proj"
                    ? kvWidth
                    : hidden;
                var rank = (long)candidateLora.Rank;

                // LoraLinear inference allocation accounting per target:
                // explicit A/B transposes; both MatMul right-hand transpose
                // workspaces; A result; B result; Scale result; Add result.
                var transposedWeightsAndWorkspaces = checked(
                    2L * rank * checked(hidden + outputWidth));
                var intermediateAndOutputElements = checked(
                    tokens * rank + 3L * tokens * outputWidth);
                loraAllocatedElementsPerLayer = checked(
                    loraAllocatedElementsPerLayer
                    + transposedWeightsAndWorkspaces
                    + intermediateAndOutputElements);

                // Work units count each element moved by a transpose, each
                // multiply-accumulate performed by MatMul, and each Scale/Add
                // element operation. This is intentionally independent of SIMD
                // vector width.
                loraOperationsPerLayer = checked(
                    loraOperationsPerLayer
                    + transposedWeightsAndWorkspaces
                    + tokens * hidden * rank
                    + tokens * rank * outputWidth
                    + 2L * tokens * outputWidth);
            }
        }

        var activations = checked(
            logits
            + tokens * hidden
            + checked(perLayerActivations + loraAllocatedElementsPerLayer)
                * config.NumLayers);
        var activationBytes = checked(activations * sizeof(float));

        var projectionOps = checked(
            tokens
            * (2L * hidden * hidden
                + 2L * hidden * kvWidth
                + 3L * hidden * config.FfnHiddenDim));
        var attentionOps = checked(2L * heads * tokens * tokens * config.HeadDim);
        var operations = checked(
            (projectionOps + attentionOps + loraOperationsPerLayer)
                * config.NumLayers
            + tokens * hidden * config.VocabSize);
        return new HeldOutCaseComputeEstimate(
            logits,
            logitsBytes,
            activations,
            activationBytes,
            operations);
    }

    private static HeldOutCaseComputeEstimate AddComputeEstimates(
        HeldOutCaseComputeEstimate left,
        HeldOutCaseComputeEstimate right)
        => new(
            checked(left.LogitsElements + right.LogitsElements),
            checked(left.LogitsBytes + right.LogitsBytes),
            checked(left.ActivationElements + right.ActivationElements),
            checked(left.ActivationBytes + right.ActivationBytes),
            checked(left.Operations + right.Operations));

    private static bool WithinPerCaseLimits(
        HeldOutCaseComputeEstimate estimate,
        HeldOutEvaluationLimits limits)
        => estimate.LogitsElements <= limits.MaxLogitsElements
           && estimate.LogitsBytes <= limits.MaxLogitsBytes
           && estimate.ActivationElements <= limits.MaxActivationElements
           && estimate.ActivationBytes <= limits.MaxActivationBytes
           && estimate.Operations <= limits.MaxOperationBudget;

    private static bool WithinCumulativeLimits(
        HeldOutCaseComputeEstimate estimate,
        HeldOutEvaluationLimits limits)
        => estimate.LogitsElements <= limits.MaxCumulativeLogitsElements
           && estimate.LogitsBytes <= limits.MaxCumulativeLogitsBytes
           && estimate.ActivationElements <= limits.MaxCumulativeActivationElements
           && estimate.ActivationBytes <= limits.MaxCumulativeActivationBytes
           && estimate.Operations <= limits.MaxCumulativeOperationBudget;

    private static bool ModelIdentityEquals(
        ModelConfigurationIdentity left,
        ModelConfigurationIdentity right)
        => left == right;

    private static bool LoraIdentityEquals(
        LoraConfigurationIdentity left,
        LoraConfigurationIdentity right)
        => left.Rank == right.Rank
           && left.Alpha.Equals(right.Alpha)
           && left.Scaling.Equals(right.Scaling)
           && left.Dropout.Equals(right.Dropout)
           && StringComparer.Ordinal.Equals(left.Sha256, right.Sha256)
           && left.OrderedTargetModules.SequenceEqual(
               right.OrderedTargetModules,
               StringComparer.Ordinal);

    private static OperationResult<AccuracyEvaluationResult> Fail(
        string message,
        string code)
        => OperationResult<AccuracyEvaluationResult>.Fail(message, code);

    internal static bool PathIdentitiesEqualForTest(string left, string right)
        => HeldArtifact.PathIdentityEquals(left, right);

    private sealed record PreparedEvaluationCase(
        string CaseId,
        int[] Sequence,
        int PrefixCount,
        int InputCount);

    private sealed record TokenScore(long CorrectTokens, long ScoredTokens);

    private sealed class HeldArtifact : IDisposable
    {
        public FileStream Stream { get; }
        public string CanonicalPath { get; }
        public long Length { get; }
        public string Sha256 { get; }
        public string OpenedIdentity { get; }
        private long MaximumBytes { get; }
        private string ArtifactKind { get; }
        private Func<FileStream, string> IdentityResolver { get; }

        private HeldArtifact(
            FileStream stream,
            string canonicalPath,
            long length,
            string sha256,
            string openedIdentity,
            long maximumBytes,
            string artifactKind,
            Func<FileStream, string> identityResolver)
            => (Stream, CanonicalPath, Length, Sha256, OpenedIdentity, MaximumBytes, ArtifactKind, IdentityResolver) =
                (stream, canonicalPath, length, sha256, openedIdentity, maximumBytes, artifactKind, identityResolver);

        public static OperationResult<HeldArtifact> Open(
            string path,
            long maximumBytes,
            string artifactKind,
            CancellationToken cancellationToken,
            Func<FileStream, string>? identityResolver = null)
        {
            FileStream? stream = null;
            try
            {
                var appliedIdentityResolver = identityResolver ?? ResolveOpenedIdentity;
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                    return OperationResult<HeldArtifact>.Fail($"{artifactKind} path must be absolute.", $"{artifactKind}_PATH_INVALID");
                var canonicalPath = Path.GetFullPath(path);
                if (!StringComparer.Ordinal.Equals(path, canonicalPath))
                    return OperationResult<HeldArtifact>.Fail($"{artifactKind} path must be canonical.", $"{artifactKind}_PATH_NONCANONICAL");
                if (HasReparsePointInChain(canonicalPath))
                    return OperationResult<HeldArtifact>.Fail($"{artifactKind} path contains a reparse point.", $"{artifactKind}_REPARSE_POINT_REJECTED");

                stream = new FileStream(
                    canonicalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    FileOptions.SequentialScan);
                if (stream.Length <= 0 || stream.Length > maximumBytes)
                    return DisposeAndFail(stream, $"{artifactKind} size is invalid.", $"{artifactKind}_SIZE_INVALID");
                var length = stream.Length;
                var sha256 = Hash(stream, cancellationToken);
                var openedIdentity = appliedIdentityResolver(stream);
                if (stream.Length != length
                    || HasReparsePointInChain(canonicalPath)
                    || !PathIdentityEquals(openedIdentity, canonicalPath))
                    return DisposeAndFail(stream, $"{artifactKind} changed while opening.", $"{artifactKind}_CHANGED");
                return OperationResult<HeldArtifact>.Ok(
                    new HeldArtifact(
                        stream,
                        canonicalPath,
                        length,
                        sha256,
                        openedIdentity,
                        maximumBytes,
                        artifactKind,
                        appliedIdentityResolver));
            }
            catch (Exception exception)
                when (exception is PlatformNotSupportedException)
            {
                stream?.Dispose();
                return OperationResult<HeldArtifact>.Fail(
                    $"{artifactKind} identity is unsupported on this platform.",
                    $"{artifactKind}_IDENTITY_UNSUPPORTED");
            }
            catch (Exception exception)
                when (exception is IOException
                      && StringComparer.Ordinal.Equals(
                          exception.Message,
                          "HELD_ARTIFACT_IDENTITY_UNAVAILABLE"))
            {
                stream?.Dispose();
                return OperationResult<HeldArtifact>.Fail(
                    $"{artifactKind} identity could not be resolved.",
                    $"{artifactKind}_IDENTITY_UNAVAILABLE");
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                stream?.Dispose();
                return OperationResult<HeldArtifact>.Fail(
                    $"{artifactKind} could not be held for evaluation.",
                    $"{artifactKind}_OPEN_FAILED");
            }
        }

        public void Rewind() => Stream.Position = 0;

        public bool VerifyUnchanged(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Stream.Length != Length
                || HasReparsePointInChain(CanonicalPath)
                || !PathIdentityEquals(IdentityResolver(Stream), OpenedIdentity)
                || !StringComparer.Ordinal.Equals(Hash(Stream, cancellationToken), Sha256))
                return false;

            var reopenedResult = Open(
                CanonicalPath,
                MaximumBytes,
                ArtifactKind,
                cancellationToken,
                IdentityResolver);
            if (!reopenedResult.IsSuccess || reopenedResult.Value is null)
                return false;
            using var reopened = reopenedResult.Value;
            return reopened.Length == Length
                   && StringComparer.Ordinal.Equals(reopened.Sha256, Sha256)
                   && PathIdentityEquals(reopened.OpenedIdentity, OpenedIdentity);
        }

        public void Dispose() => Stream.Dispose();

        private static OperationResult<HeldArtifact> DisposeAndFail(
            FileStream stream,
            string message,
            string code)
        {
            stream.Dispose();
            return OperationResult<HeldArtifact>.Fail(message, code);
        }

        private static string Hash(Stream stream, CancellationToken cancellationToken)
        {
            var originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                var buffer = new byte[64 * 1024];
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;
                    hash.AppendData(buffer, 0, read);
                }
                return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static bool HasReparsePointInChain(string path)
        {
            var current = new FileInfo(path);
            if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
                return true;
            for (var directory = current.Directory;
                 directory is not null;
                 directory = directory.Parent)
            {
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                    return true;
            }
            return false;
        }

        internal static bool PathIdentityEquals(string left, string right)
            => (OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal)
                .Equals(left, right);

        private static string ResolveOpenedIdentity(FileStream stream)
        {
            if (OperatingSystem.IsWindows())
            {
                var buffer = new StringBuilder(32_768);
                var length = GetFinalPathNameByHandle(
                    stream.SafeFileHandle.DangerousGetHandle(),
                    buffer,
                    buffer.Capacity,
                    0);
                if (length == 0 || length >= buffer.Capacity)
                    throw new IOException("HELD_ARTIFACT_IDENTITY_UNAVAILABLE");
                var path = buffer.ToString();
                if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
                    path = @"\\" + path[8..];
                else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                    path = path[4..];
                return Path.GetFullPath(path);
            }

            if (OperatingSystem.IsLinux())
            {
                var descriptorPath = $"/proc/self/fd/{stream.SafeFileHandle.DangerousGetHandle().ToInt64()}";
                var target = File.ResolveLinkTarget(descriptorPath, returnFinalTarget: true)
                    ?? throw new IOException("HELD_ARTIFACT_IDENTITY_UNAVAILABLE");
                return Path.GetFullPath(target.FullName);
            }

            throw new PlatformNotSupportedException(
                "HELD_ARTIFACT_IDENTITY_UNSUPPORTED");
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFinalPathNameByHandle(
            IntPtr fileHandle,
            StringBuilder filePath,
            int filePathLength,
            uint flags);
    }
}
