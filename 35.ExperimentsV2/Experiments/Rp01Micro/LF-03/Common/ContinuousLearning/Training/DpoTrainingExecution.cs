// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;

internal sealed record DpoTrainingExecutionRequest(
    string Domain,
    IReadOnlyList<DpoPair> Pairs,
    LlmModel PolicyModel,
    LlmModel ReferenceModel,
    BpeTokenizer Tokenizer,
    LoraConfig LoraConfig,
    string CheckpointDirectory,
    ServerOwnedLegalTrainingTeacherAdmissionLease LegalTeacherAdmissionLease,
    int DpoStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount,
    AdamWPortableState? OptimizerStateToRestore = null,
    LoraPairRandomSeedAuthority? AdapterSeedAuthority = null,
    bool SeededTopologyPreparedByRepositoryStart = false,
    LlmModelRandomStateSnapshot? RandomStateToRestore = null,
    DpoRandomStateColdStartAuthorization?
        RandomStateColdStartAuthorization = null,
    Action<DpoTrainingPublicationBundle>? BeforePolicyFinalMove = null,
    Action<DpoTrainingExecutionArtifact>? BeforePolicyFinalMoveArtifact = null,
    Action? PublicationFailed = null,
    Func<DpoTrainingExecutionArtifact, OperationResult<bool>>?
        PostPublicationValidation = null,
    string MaterialVersion = DpoExactPairMaterial.LegacyVersion,
    DpoContinuationProof? ContinuationProof = null);

internal sealed record DpoTrainingExecutionArtifact(
    int ProcessedPairCount,
    int DpoStepCount,
    string PolicyCheckpointPath,
    string MetricsPath,
    string LegalTeacherPolicyIdentityRef,
    AdamWPortableState? PostStepOptimizerState = null,
    LogicalDpoTrainingOptimizerStateIdentity?
        PostStepOptimizerStateIdentity = null,
    string OptimizerStatePath = "",
    string OptimizerStateSha256 = "",
    long OptimizerStateByteLength = 0,
    int OptimizerStateStep = 0,
    int OptimizerStateFormatVersion = 0,
    DpoTrainingOptimizerSettings? OptimizerSettings = null,
    LlmModelRandomStateSnapshot? PostStepRandomState = null,
    string RandomStatePath = "",
    string RandomStateSha256 = "",
    long RandomStateByteLength = 0,
    int RandomStateStep = 0,
    int RandomStateFormatVersion = 0,
    bool RandomStateRequired = false,
    string RandomStateTopologySha256 = "",
    string RandomStateLoraConfigurationSha256 = "",
    string TrialWeightsPath = "",
    string TrialWeightsSha256 = "",
    long TrialWeightsByteLength = 0,
    DpoExecutionProfile? ExecutionProfile = null);

internal interface IDpoTrainingExecutor
{
    Task<OperationResult<DpoTrainingExecutionArtifact>> ExecuteAsync(
        DpoTrainingExecutionRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Production adapter from ContinuousLearning pairs to the existing stable
/// logical DPO unit, whose optimizer/checkpoint capability is 11.DPO/DpoTrainer.
/// </summary>
internal sealed class StableDpoTrainingExecutor : IDpoTrainingExecutor
{
    internal const int DefaultDpoStepCount = LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount;
    private static readonly AsyncLocal<Action<LogicalDpoTrainingRequest>?>
        LogicalRequestObserver = new();
    private readonly ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult> _trainingUnit;

    internal static Action<LogicalDpoTrainingRequest>?
        LogicalRequestObserverForTesting
    {
        get => LogicalRequestObserver.Value;
        set => LogicalRequestObserver.Value = value;
    }

    internal StableDpoTrainingExecutor()
        : this(new LogicalDpoTrainingUnit())
    {
    }

    internal StableDpoTrainingExecutor(
        ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult> trainingUnit)
    {
        _trainingUnit = trainingUnit ?? throw new ArgumentNullException(nameof(trainingUnit));
    }

    public async Task<OperationResult<DpoTrainingExecutionArtifact>> ExecuteAsync(
        DpoTrainingExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Pairs.Count == 0)
        {
            return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                "At least one admitted DPO pair is required.",
                "DPO_PREFERENCES_REQUIRED");
        }

        var requestedStepCount = ValidateDpoStepCount(request.DpoStepCount);
        if (!requestedStepCount.IsSuccess)
            return requestedStepCount.MapError<DpoTrainingExecutionArtifact>();

        var admissionCapabilities = request.LegalTeacherAdmissionLease
            .IssueAdmissionCapabilities();
        if (!admissionCapabilities.IsSuccess ||
            admissionCapabilities.Value is null)
        {
            return admissionCapabilities.MapError<
                DpoTrainingExecutionArtifact>();
        }
        var createdArtifacts = new DpoTrainingExecutionArtifactLedger(
            request.CheckpointDirectory);
        RepositoryOwnedDpoOptimizerStateFileReadback?
            preparedOptimizerState = null;
        RepositoryOwnedDpoRandomStateWriteReadback?
            preparedRandomState = null;
        DpoTrainingExecutionArtifact? preparedPublicationArtifact = null;
        var logicalRequest = TryBuildLogicalRequest(
            request with
            {
                PublicationFailed = () =>
                {
                    createdArtifacts.Cleanup();
                    request.PublicationFailed?.Invoke();
                }
            },
            requestedStepCount.Value,
            admissionCapabilities.Value,
            createdArtifacts.TryAcceptFinalArtifact,
            persistPublicationBundle: bundle =>
            {
                var state = bundle.OptimizerState;
                var persisted = RepositoryOwnedDpoOptimizerStateFile
                    .WriteAndReadBack(
                        request.CheckpointDirectory,
                        bundle.OptimizerStateStep,
                        state,
                        cancellationToken);
                    if (!persisted.IsSuccess || persisted.Value is null)
                    {
                        throw new InvalidOperationException(
                            persisted.ErrorMessage);
                    }

                    // Record ownership only after the create-new write and
                    // read-back have succeeded. A pre-existing state file that
                    // rejected this transaction must survive rollback.
                    createdArtifacts.RecordOptimizerState(bundle.OptimizerStateStep);
                    preparedOptimizerState = persisted.Value;
                    if (bundle.RandomState is not null)
                    {
                        var randomPath = Path.GetFullPath(Path.Combine(
                            request.CheckpointDirectory,
                            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                                bundle.OptimizerStateStep)));
                        var random = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                            randomPath,
                            bundle.RandomState);
                        if (!random.IsSuccess || random.Value is null)
                        {
                            throw new InvalidOperationException(
                                random.ErrorMessage);
                        }
                        createdArtifacts.RecordRandomState(
                            random.Value.OwnershipProof);
                        preparedRandomState = random.Value;
                    }
                    preparedPublicationArtifact = BuildPrePublicationArtifact(
                        request,
                        bundle,
                        persisted.Value,
                        preparedRandomState);
                    request.BeforePolicyFinalMoveArtifact?.Invoke(
                        preparedPublicationArtifact);
            });
        if (!logicalRequest.IsSuccess || logicalRequest.Value is null)
            return logicalRequest.MapError<DpoTrainingExecutionArtifact>();

        OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>> result;
        try
        {
            LogicalRequestObserver.Value?.Invoke(logicalRequest.Value.Input);
            result = await _trainingUnit.ExecuteAsync(
                logicalRequest.Value,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            CleanupFailedExecution(
                createdArtifacts,
                request.PublicationFailed);
            return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                "The stable DPO capability failed.",
                "DPO_EXECUTION_FAILED");
        }

        var finalPermit = request.LegalTeacherAdmissionLease
            .AcquireMutationPermit();
        if (!finalPermit.IsSuccess || finalPermit.Value is null)
        {
            CleanupFailedExecution(
                createdArtifacts,
                request.PublicationFailed);
            return finalPermit.MapError<DpoTrainingExecutionArtifact>();
        }
        using var permit = finalPermit.Value;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!result.IsSuccess || result.Value is null)
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return result.MapError<DpoTrainingExecutionArtifact>();
            }

            var output = result.Value.Output;
            if (output.PreferenceCount != request.Pairs.Count
                || output.DpoStepCount < LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount
                || output.DpoStepCount > requestedStepCount.Value
                || !output.IngredientIds.Contains(
                    "11.DPO:DpoTrainer",
                    StringComparer.Ordinal))
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                    "The stable DPO capability returned an incomplete execution result.",
                    "DPO_EXECUTION_INCOMPLETE");
            }

            var checkpointPath = Path.GetFullPath(Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(
                    output.DpoStepCount)));
            var metricsPath = Path.GetFullPath(Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(
                    output.DpoStepCount)));
            if (!TryGetDurableArtifactInRunDirectory(
                    checkpointPath,
                    request.CheckpointDirectory,
                    out checkpointPath)
                || !TryGetDurableArtifactInRunDirectory(
                    metricsPath,
                    request.CheckpointDirectory,
                    out metricsPath))
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                    "The stable DPO capability did not publish its durable artifacts.",
                    "DPO_EXECUTION_ARTIFACT_INCOMPLETE");
            }

            var optimizerState = output.PostStepOptimizerState;
            var optimizerStateIdentity = output.PostStepOptimizerStateIdentity;
            if (optimizerState is null || optimizerStateIdentity is null ||
                preparedOptimizerState is null)
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                    "The stable DPO capability did not return a post-step optimizer state.",
                    "DPO_EXECUTION_OPTIMIZER_STATE_INCOMPLETE");
            }

            if (output.PostStepRandomState is not null &&
                preparedRandomState is null)
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                    "The DPO capability returned RNG state without a repository-owned readback.",
                    "DPO_EXECUTION_RANDOM_STATE_INCOMPLETE");
            }

            if (optimizerStateIdentity.FormatVersion !=
                    preparedOptimizerState.State.FormatVersion ||
                optimizerStateIdentity.Step != preparedOptimizerState.State.Step ||
                !StringComparer.Ordinal.Equals(
                    optimizerStateIdentity.Sha256,
                    preparedOptimizerState.OptimizerStateSha256) ||
                optimizerStateIdentity.ByteLength !=
                    preparedOptimizerState.OptimizerStateByteLength)
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                    "The post-step DPO optimizer state identity does not match its repository-owned file.",
                    "DPO_EXECUTION_OPTIMIZER_STATE_INVALID");
            }

            var finalArtifact = new DpoTrainingExecutionArtifact(
                    output.PreferenceCount,
                    output.DpoStepCount,
                    checkpointPath,
                    metricsPath,
                    request.LegalTeacherAdmissionLease.Admission
                        .PolicyIdentityRef,
                    preparedOptimizerState.State,
                    optimizerStateIdentity,
                    preparedOptimizerState.OptimizerStatePath,
                    preparedOptimizerState.OptimizerStateSha256,
                    preparedOptimizerState.OptimizerStateByteLength,
                    preparedOptimizerState.State.Step,
                    preparedOptimizerState.State.FormatVersion,
                    output.OptimizerSettings,
                    output.PostStepRandomState,
                    preparedRandomState?.Path ?? "",
                    preparedRandomState?.Sha256 ?? "",
                    preparedRandomState?.ByteLength ?? 0,
                    preparedRandomState?.State.RandomStateStep ?? 0,
                    preparedRandomState is null
                        ? 0
                        : checked((int)preparedRandomState.State.FormatVersion),
                    output.PostStepRandomState is not null,
                    preparedRandomState?.State.TopologySha256 ?? "",
                    preparedRandomState?.State.LoraConfigurationSha256 ?? "",
                    preparedPublicationArtifact?.TrialWeightsPath ?? "",
                    preparedPublicationArtifact?.TrialWeightsSha256 ?? "",
                    preparedPublicationArtifact?.TrialWeightsByteLength ?? 0,
                    preparedPublicationArtifact?.ExecutionProfile);
            var postPublicationValidation = request.PostPublicationValidation?.Invoke(
                finalArtifact);
            if (postPublicationValidation is not null &&
                (!postPublicationValidation.IsSuccess ||
                 postPublicationValidation.Value != true))
            {
                CleanupFailedExecution(
                    createdArtifacts,
                    request.PublicationFailed);
                return postPublicationValidation.MapError<
                    DpoTrainingExecutionArtifact>();
            }

            return OperationResult<DpoTrainingExecutionArtifact>.Ok(finalArtifact);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            CleanupFailedExecution(
                createdArtifacts,
                request.PublicationFailed);
            return OperationResult<DpoTrainingExecutionArtifact>.Fail(
                "The stable DPO capability failed.",
                "DPO_EXECUTION_FAILED");
        }
    }

    internal static LogicalUnitRequest<LogicalDpoTrainingRequest> BuildLogicalRequest(
        DpoTrainingExecutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestedStepCount = ValidateDpoStepCount(request.DpoStepCount);
        if (!requestedStepCount.IsSuccess)
            throw new ArgumentOutOfRangeException(
                nameof(request),
                requestedStepCount.ErrorMessage);

        var admissionCapabilities = request.LegalTeacherAdmissionLease
            .IssueAdmissionCapabilities();
        if (!admissionCapabilities.IsSuccess ||
            admissionCapabilities.Value is null)
        {
            throw new InvalidOperationException(
                admissionCapabilities.ErrorMessage);
        }
        var logicalRequest = TryBuildLogicalRequest(
            request,
            requestedStepCount.Value,
            admissionCapabilities.Value);
        if (!logicalRequest.IsSuccess || logicalRequest.Value is null)
            throw new InvalidOperationException(logicalRequest.ErrorMessage);
        return logicalRequest.Value;
    }

    private static OperationResult<LogicalUnitRequest<
        LogicalDpoTrainingRequest>> TryBuildLogicalRequest(
        DpoTrainingExecutionRequest request,
        int validatedMaxStepCount,
        LogicalCapabilityTrainingAdmissionCapabilities admissionCapabilities,
        Func<DpoTrainerArtifactOwnershipProof, bool>?
            tryAcceptArtifactOwnership = null,
        Action<DpoTrainingPublicationBundle>? persistPublicationBundle = null)
    {
        // The server lease was acquired over DpoPairTrainingContentAuthority's
        // exact prompt/chosen/rejected projection. The teaching vector is a
        // deterministic local derivation of that admitted prompt, so preparing
        // once here binds every optimizer object transitively to the already
        // admitted carrier without changing the existing admission protocol.
        if (request.MaterialVersion == DpoExactPairMaterial.Version && request.Domain != "code-documentation")
            return OperationResult<LogicalUnitRequest<LogicalDpoTrainingRequest>>.Fail(
                "Exact response material is selected only by the CodeDocumentation producer.", DpoExactPairMaterial.InvalidErrorCode);
        var preferences = request.Pairs
            .Select(static pair => new LogicalDpoPreferenceExample(
                pair.Prompt,
                pair.Chosen,
                pair.Rejected)
            {
                // v5 переносит только typed kind: OpenTeacher паспорт остается
                // в богатой паре/lease, а v4 null честно падает в PrepareExecution.
                ChosenAuthorKind = pair.ChosenAuthorship?.AuthorKind,
                RejectedAuthorKind = pair.RejectedAuthorship?.AuthorKind
            })
            .ToArray();
        var preparedExecution = LogicalDpoTrainingUnit.PrepareExecution(
            preferences,
            request.Tokenizer,
            request.MaterialVersion);
        if (!preparedExecution.IsSuccess || preparedExecution.Value is null)
        {
            return preparedExecution.MapError<LogicalUnitRequest<
                LogicalDpoTrainingRequest>>();
        }

        var input = new LogicalDpoTrainingRequest(
            request.PolicyModel,
            request.ReferenceModel,
            request.Tokenizer,
            request.CheckpointDirectory,
            preferences,
            request.LoraConfig,
            MaxStepCount: validatedMaxStepCount,
            AdmissionCapabilities: admissionCapabilities,
            TryAcceptArtifactOwnership: tryAcceptArtifactOwnership,
            MutationAdmission:
                admissionCapabilities.LogicalMutationAdmissionCapability,
            OptimizerStateToRestore: request.OptimizerStateToRestore,
            PersistPublicationBundle: persistPublicationBundle,
            BeforePolicyFinalMove: request.BeforePolicyFinalMove,
            PublicationFailed: request.PublicationFailed)
        {
            PreparedExecution = preparedExecution.Value,
            MaterialVersion = request.MaterialVersion,
            ContinuationProof = request.ContinuationProof,
            SeededTopologyPreparedByRepositoryStart =
                request.SeededTopologyPreparedByRepositoryStart,
            RandomStateToRestore = request.RandomStateToRestore,
            RandomStateColdStartAuthorization =
                request.RandomStateColdStartAuthorization,
            AdapterSeedAuthority = request.AdapterSeedAuthority is null
                ? null
                : new LogicalDpoAdapterSeedAuthority(
                    request.AdapterSeedAuthority.RootInitializationSeed,
                    request.AdapterSeedAuthority.PolicyDropoutStreamDomain,
                    request.AdapterSeedAuthority.ReferenceDropoutStreamDomain)
        };
        return OperationResult<LogicalUnitRequest<
            LogicalDpoTrainingRequest>>.Ok(
            new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                $"continuous-learning-dpo-{Guid.NewGuid():N}",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                input,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>
                {
                    ["continuous_learning_domain"] = request.Domain,
                    ["legal_teacher_policy_ref"] =
                        request.LegalTeacherAdmissionLease.Admission
                            .PolicyIdentityRef,
                }));
    }

    internal static OperationResult<int> ValidateDpoStepCount(int dpoStepCount)
        => LogicalDpoTrainingUnit.ValidateMaxStepCount(dpoStepCount);

    private static void CleanupFailedExecution(
        DpoTrainingExecutionArtifactLedger createdArtifacts,
        Action? publicationFailed = null)
    {
        createdArtifacts.Cleanup();
        publicationFailed?.Invoke();
    }

    private sealed class DpoTrainingExecutionArtifactLedger
    {
        private readonly string _checkpointDirectory;
        private readonly Dictionary<string, DpoTrainerArtifactOwnershipProof>
            _createdFinalArtifacts;
        private readonly HashSet<int> _createdOptimizerStateSteps = [];
        private readonly Dictionary<string,
            RepositoryOwnedDpoRandomStateOwnershipProof> _createdRandomStates =
            new(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);

        internal DpoTrainingExecutionArtifactLedger(string checkpointDirectory)
        {
            _checkpointDirectory = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(checkpointDirectory));
            _createdFinalArtifacts = new Dictionary<
                string,
                DpoTrainerArtifactOwnershipProof>(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);
        }

        internal void RecordOptimizerState(int step)
        {
            if (step > 0)
                _createdOptimizerStateSteps.Add(step);
        }

        internal void RecordRandomState(
            RepositoryOwnedDpoRandomStateOwnershipProof proof)
        {
            ArgumentNullException.ThrowIfNull(proof);
            var expectedPath = Path.GetFullPath(Path.Combine(
                _checkpointDirectory,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(proof.Step)));
            if (!PathComparer.Equals(expectedPath, proof.Path) ||
                proof.ByteLength <= 0 ||
                string.IsNullOrWhiteSpace(proof.Sha256))
            {
                throw new InvalidOperationException(
                    "DPO_RANDOM_STATE_OWNERSHIP_PROOF_INVALID");
            }
            _createdRandomStates[proof.Path] = proof;
        }

        internal bool TryAcceptFinalArtifact(
            DpoTrainerArtifactOwnershipProof proof)
        {
            var expectedFileName = proof.Kind switch
            {
                DpoTrainerArtifactKind.PolicyFinal =>
                    DpoTrainer.BuildPolicyCheckpointFileName(proof.Step),
                DpoTrainerArtifactKind.MetricsFinal =>
                    DpoTrainer.BuildMetricsFileName(proof.Step),
                _ => string.Empty
            };
            if (proof.Step <= 0 ||
                proof.Length <= 0 ||
                string.IsNullOrWhiteSpace(proof.Sha256) ||
                expectedFileName.Length == 0)
            {
                return false;
            }

            var expectedPath = Path.GetFullPath(Path.Combine(
                _checkpointDirectory,
                expectedFileName));
            if (!PathComparer.Equals(expectedPath, proof.Path))
                return false;

            return _createdFinalArtifacts.TryAdd(proof.Path, proof);
        }

        internal void Cleanup()
        {
            foreach (var proof in _createdFinalArtifacts.Values.ToArray())
            {
                _ = DpoTrainer.TryDeleteFinalArtifactIfOwned(proof);
            }

            foreach (var step in _createdOptimizerStateSteps)
            {
                _ = RepositoryOwnedDpoOptimizerStateFile.DeleteIfOwned(
                    _checkpointDirectory,
                    step);
            }

            foreach (var proof in _createdRandomStates.Values.ToArray())
                _ = RepositoryOwnedDpoRandomStateFile.TryDeleteIfOwned(proof);

            _createdFinalArtifacts.Clear();
            _createdOptimizerStateSteps.Clear();
            _createdRandomStates.Clear();
        }

        private static StringComparer PathComparer
            => OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
    }

    private static DpoTrainingExecutionArtifact BuildPrePublicationArtifact(
        DpoTrainingExecutionRequest request,
        DpoTrainingPublicationBundle bundle,
        RepositoryOwnedDpoOptimizerStateFileReadback persisted,
        RepositoryOwnedDpoRandomStateWriteReadback? randomState)
    {
        var state = bundle.OptimizerState;
        // The trainer's optimizer step is a durable global counter: a
        // continuation restores (for example) step 1 and publishes step 2.
        // Policy/metrics checkpoints, however, are numbered within this run,
        // so the final artifact must use the local DPO step rather than the
        // post-restore optimizer step. Keeping those identities separate is
        // what lets the final continuation manifest bind both values exactly.
        var restoredOptimizerStep = request.OptimizerStateToRestore?.Step ?? 0;
        var dpoStep = checked(state.Step - restoredOptimizerStep);
        if (dpoStep < LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount ||
            dpoStep > request.DpoStepCount)
        {
            throw new InvalidOperationException(
                "The post-step DPO optimizer state does not map to a valid local training step.");
        }

        var policyPath = Path.GetFullPath(Path.Combine(
            request.CheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(dpoStep)));
        var metricsPath = Path.GetFullPath(Path.Combine(
            request.CheckpointDirectory,
            DpoTrainer.BuildMetricsFileName(dpoStep)));
        var identity = new LogicalDpoTrainingOptimizerStateIdentity(
            state.FormatVersion,
            state.Step,
            persisted.OptimizerStateSha256,
            persisted.OptimizerStateByteLength);

        return new DpoTrainingExecutionArtifact(
            request.Pairs.Count,
            dpoStep,
            policyPath,
            metricsPath,
            request.LegalTeacherAdmissionLease.Admission.PolicyIdentityRef,
            state,
            identity,
            persisted.OptimizerStatePath,
            persisted.OptimizerStateSha256,
            persisted.OptimizerStateByteLength,
            state.Step,
            state.FormatVersion,
            DpoTrainer.ProductionOptimizerSettings,
            randomState?.State,
            randomState?.Path ?? "",
            randomState?.Sha256 ?? "",
            randomState?.ByteLength ?? 0,
            randomState?.State.RandomStateStep ?? 0,
            randomState is null ? 0 : checked((int)randomState.State.FormatVersion),
            randomState is not null,
            randomState?.State.TopologySha256 ?? "",
            randomState?.State.LoraConfigurationSha256 ?? "",
            Path.GetFullPath(Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(dpoStep))),
            bundle.TrialWeightsSha256,
            bundle.TrialWeightsByteLength,
            bundle.ExecutionProfile);
    }

    private static bool TryGetDurableArtifactInRunDirectory(
        string path,
        string expectedRunDirectory,
        out string canonicalPath)
    {
        canonicalPath = string.Empty;
        try
        {
            var canonical = TrainingSourceProvenance.RequireCanonicalRegularFilePath(path);
            var expectedDirectory = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(expectedRunDirectory));
            if (!StringComparer.OrdinalIgnoreCase.Equals(
                    Path.GetDirectoryName(canonical),
                    expectedDirectory)
                || new FileInfo(canonical).Length <= 0)
            {
                return false;
            }

            canonicalPath = canonical;
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }
}
