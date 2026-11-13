namespace TranslationDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TransformerModel;

public sealed record TranslationDpoTrainEvaluateRunnerOptions(
    string OutputPath,
    string CheckpointDirectory,
    string TrainingDataArtifactRoot,
    int TrainStep = 1,
    int HeldOutEvaluationStep = 2,
    string? HandoffId = null,
    string? TrainBatchId = null,
    TranslationDpoTokenizationSettings? TokenizationSettings = null,
    bool AllowRepositoryOutput = false,
    DpoTrainingReproducibilityContext? ReproducibilityContext = null,
    TranslationRepositoryOwnedDpoDatasetContext? RepositoryOwnedDpoDatasetContext = null,
    TranslationRepositoryOwnedModelCheckpointContext? RepositoryOwnedModelCheckpointContext = null);

public sealed record TranslationDpoMetricsSnapshot(
    int Step,
    float Loss,
    float ChosenReward,
    float RejectedReward,
    float RewardMargin,
    float Accuracy);

public sealed record TranslationDpoTrainingDataArtifactEvidence(
    string ArtifactRoot,
    string ManifestPath,
    string ManifestRepoPath,
    string CorpusPath,
    string CorpusSha256,
    int CorpusRowsCount,
    int ManifestTrainingPairsCount,
    bool LegalCommonDpoManifestValidated,
    bool CommonTrainReady,
    bool DpoTrainReady,
    string SourceCategory,
    string DataClass,
    string StorageScope,
    string PrivacyScope,
    string ProvenanceRef,
    string SplitPolicy,
    string SourceDisjointGroup,
    string LicenseOrProjectInternalRef,
    string LegalVerdict);

public sealed record TranslationRepositoryOwnedDpoDatasetRunEvidence(
    bool Enabled,
    string ResolutionSource,
    string ManifestPath,
    string ManifestRelativePath,
    string ManifestSha256,
    string RequestIdentitySha256,
    int PairCount,
    bool TrainingPairsReopenedFromDisk,
    bool LegacyTrainingDataArtifactLoaded,
    string HeldOutEvaluationSource,
    string Boundary);

public sealed record TranslationRepositoryOwnedModelCheckpointRunEvidence(
    bool Enabled,
    string Authority,
    string StartMode,
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    int PublishedStep,
    string PublishedManifestPath,
    string PublishedManifestRelativePath,
    string PublishedWeightsPath,
    string PublishedWeightsRelativePath,
    string PublishedWeightsSha256,
    long PublishedWeightsByteLength,
    int? ParentStep,
    string? ParentManifestRelativePath,
    string? ParentWeightsSha256,
    long? ParentWeightsByteLength,
    string? ColdStartReason,
    IReadOnlyList<string> LoaderDiagnostics,
    string Boundary);

public sealed record TranslationDpoTrainEvaluateRunReport(
    string Pipeline,
    string ReportPath,
    string SourceCorpusPath,
    string SourceCorpusSha256,
    int SourceCorpusRowCount,
    int TrainSourceRowCount,
    int HeldOutSourceRowCount,
    TranslationDpoTrainingDataArtifactEvidence TrainingDataArtifactEvidence,
    string HandoffId,
    string SourceBatchId,
    int TrainStep,
    int HeldOutEvaluationStep,
    int TrainPairCount,
    int HeldOutCount,
    int HeldOutExcludedFromTrainingCount,
    int TrainExcludedFromHeldOutEvaluationCount,
    TranslationDpoMetricsSnapshot TrainMetrics,
    TranslationDpoMetricsSnapshot HeldOutMetrics,
    string? TrainerMetricsPath,
    bool TrainerMetricsWritten,
    string? CheckpointPath,
    bool CheckpointWritten,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> HeldOutEvaluationPairIds,
    IReadOnlyList<string> ExcludedTrainPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    bool LegalProvenanceValidated,
    bool ClosedTeacherRejected,
    bool HeldOutRowsPassedToStepMany,
    bool TrainRowsUsedForHeldOutEvaluation,
    bool SemanticQualityClaimed,
    bool SemanticQualityReadinessClaimed,
    string SemanticQualityReadinessClosedReason,
    string TrainerCallPath,
    string EvaluationCallPath)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DpoTrainingReproducibilityEvidence? ReproducibilityEvidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DpoTrainingSeededLoraTopologyApplicationEvidence? SeededLoraTopologyEvidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolicyCheckpointSha256 { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TranslationRepositoryOwnedDpoDatasetRunEvidence?
        RepositoryOwnedDpoDatasetEvidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TranslationRepositoryOwnedModelCheckpointRunEvidence?
        RepositoryOwnedModelCheckpointEvidence { get; init; }
}

public sealed record TranslationDpoCheckpointLifecycleRunnerOptions(
    string OutputPath,
    string ContinuationCheckpointDirectory,
    int FreshLoadEvaluationStep = 3,
    int ContinueTrainStep = 4,
    int PostContinueEvaluationStep = 5,
    TranslationDpoTokenizationSettings? TokenizationSettings = null,
    bool AllowRepositoryOutput = false,
    float LearningRate = 1e-5f,
    DpoTrainingReproducibilityContext? ReproducibilityContext = null);

public sealed record TranslationDpoCheckpointLifecycleRunReport(
    string Pipeline,
    string ReportPath,
    string SourceCorpusPath,
    string SourceCorpusSha256,
    int SourceCorpusRowCount,
    int TrainSourceRowCount,
    int HeldOutSourceRowCount,
    string PreviousRunnerReportPath,
    string LoadedCheckpointPath,
    bool CheckpointFileUsed,
    bool LoadedOrResumed,
    bool PolicyCheckpointLoaded,
    bool ReferenceModelRestoredFromCheckpoint,
    string ReferenceModelBoundary,
    string TrainerLoadCallPath,
    string HandoffId,
    string SourceBatchId,
    int FreshLoadEvaluationStep,
    int ContinueTrainStep,
    int PostContinueEvaluationStep,
    int ContinueTrainPairCount,
    int HeldOutCount,
    int HeldOutExcludedFromContinuationTrainingCount,
    int TrainExcludedFromFreshEvaluationCount,
    TranslationDpoMetricsSnapshot FreshLoadedHeldOutMetrics,
    TranslationDpoMetricsSnapshot ContinueTrainMetrics,
    TranslationDpoMetricsSnapshot PostContinueHeldOutMetrics,
    string? ContinuationTrainerMetricsPath,
    bool ContinuationTrainerMetricsWritten,
    string? ContinuationCheckpointPath,
    bool ContinuationCheckpointWritten,
    IReadOnlyList<string> ContinuedTrainingPairIds,
    IReadOnlyList<string> FreshHeldOutEvaluationPairIds,
    IReadOnlyList<string> PostHeldOutEvaluationPairIds,
    IReadOnlyList<string> ExcludedTrainPairIds,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> Directions,
    bool HeldOutRowsPassedToContinueTraining,
    bool TrainRowsUsedForFreshEvaluation,
    bool SemanticQualityClaimed,
    bool SemanticQualityReadinessClaimed,
    string SemanticQualityReadinessClosedReason,
    string LifecycleBoundary)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DpoTrainingReproducibilityEvidence? ReproducibilityEvidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DpoTrainingSeededLoraTopologyApplicationEvidence? SeededLoraTopologyEvidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LoadedPolicyCheckpointSha256 { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationPolicyCheckpointSha256 { get; init; }
}

public static class TranslationDpoTrainEvaluateRunner
{
    public const string MutationAdmissionBrokerRequiredErrorCode =
        "TRANSLATION_DPO_MUTATION_ADMISSION_BROKER_REQUIRED";
    public const string ReproducibilityContextRequiredErrorCode =
        "TRANSLATION_DPO_REPRODUCIBILITY_CONTEXT_REQUIRED";
    public const string ReproducibilityContextInvalidErrorCode =
        "TRANSLATION_DPO_REPRODUCIBILITY_CONTEXT_INVALID";
    public const string ReproducibilityEvidenceRequiredErrorCode =
        "TRANSLATION_DPO_REPRODUCIBILITY_EVIDENCE_REQUIRED";
    public const string ReproducibilityEvidenceInvalidErrorCode =
        "TRANSLATION_DPO_REPRODUCIBILITY_EVIDENCE_INVALID";
    public const string ReproducibilityEvidenceMismatchErrorCode =
        "TRANSLATION_DPO_REPRODUCIBILITY_EVIDENCE_MISMATCH";
    public const string SeededLoraTopologyPreparationFailedErrorCode =
        "TRANSLATION_DPO_SEEDED_LORA_TOPOLOGY_PREPARATION_FAILED";
    public const string SeededLoraTopologyEvidenceRequiredErrorCode =
        "TRANSLATION_DPO_SEEDED_LORA_TOPOLOGY_EVIDENCE_REQUIRED";
    public const string SeededLoraTopologyEvidenceInvalidErrorCode =
        "TRANSLATION_DPO_SEEDED_LORA_TOPOLOGY_EVIDENCE_INVALID";
    public const string PolicyCheckpointSha256RequiredErrorCode =
        "TRANSLATION_DPO_POLICY_CHECKPOINT_SHA256_REQUIRED";
    public const string PolicyCheckpointSha256MismatchErrorCode =
        "TRANSLATION_DPO_POLICY_CHECKPOINT_SHA256_MISMATCH";
    public const string RepositoryOwnedModelCheckpointDpoContextRequiredErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_DPO_CONTEXT_REQUIRED";
    public const string RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_OUTPUT_OVERLAPS_CALLER_CHECKPOINT_DIRECTORY";
    public const string RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_OUTPUT_OR_CALLER_CHECKPOINT_DIRECTORY_REPARSE_POINT";
    public const string RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_NOT_AUTHORIZED";
    public const string RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_POLICY_LOAD_FAILED";
    public const string RepositoryOwnedModelCheckpointPublishFailedErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLISH_FAILED";
    public const string RepositoryOwnedModelCheckpointVerifyFailedErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_VERIFY_FAILED";
    public const string RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PENDING_PUBLICATION_INVALID";
    public const string RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PENDING_PUBLICATION_CONFLICT";
    public const string RepositoryOwnedModelCheckpointPendingPublicationFileName =
        "translation-repository-owned-model-checkpoint-pending-publication.json";

    public const string Pipeline =
        "TranslationSeedCorpusLoader.Load -> TranslationTrainingDataArtifactStore.LoadValidatedTrainReadyPairs -> TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff -> DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple -> TranslationDpoTrainerBridge.TrainWithStepMany -> TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany";

    public const string RepositoryOwnedDpoTrainingPairsPipeline =
        "RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(training-data/dpo) -> TranslationDpoTrainingHandoffBuilder.BuildRepositoryOwnedDpoTrainHandoff(training-pairs) -> DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple -> TranslationDpoTrainerBridge.TrainWithStepMany -> TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(explicit-held-out-seed-source)";

    public const string RepositoryOwnedDpoAndModelCheckpointPipeline =
        "RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSeal(training-data/dpo) -> TranslationDpoTrainingHandoffBuilder.BuildRepositoryOwnedDpoTrainHandoff(training-pairs) -> DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple -> RepositoryOwnedModelCheckpointLoader.LoadLatestCompatible(training-artifacts/models) -> LlmModel.LoadCheckpoint(exact-verified-parent-stream-or-explicit-cold-start) -> TranslationDpoTrainerBridge.TrainWithStepMany -> TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(explicit-held-out-seed-source) -> RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(training-artifacts/models) -> RepositoryOwnedModelCheckpointManifestSealer.OpenSealed";

    public const string SemanticQualityReadinessClosedReason =
        "semantic_quality_claimed=false: train/evaluate DPO metrics are persisted as bounded training evidence only; semantic-quality readiness remains closed until independent semantic-quality proof and promotion gates exist.";

    public const string CheckpointLifecyclePipeline =
        Pipeline + " -> DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole(fresh-load,reference) -> TransformerModel.LlmModel.LoadCheckpoint -> DPO.DpoTrainer.ctor(policy,reference,checkpointDirectory,mutationAdmission,learningRate) -> TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany -> TranslationDpoTrainerBridge.TrainWithStepMany -> TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany";

    public const string TrainerLoadCallPath =
        "DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole(fresh-load,reference) -> TransformerModel.LlmModel.LoadCheckpoint -> DPO.DpoTrainer.ctor(policy,reference,checkpointDirectory,mutationAdmission,learningRate)";

    public const string ReferenceModelBoundary =
        "DpoTrainer policy checkpoints persist the policy model only; the lifecycle runner requires a caller-provided reference model and does not claim reference-model checkpoint restore.";

    public const string CheckpointLifecycleBoundary =
        "Fresh-load lifecycle proof only: a new DpoTrainer loads the runner-produced policy checkpoint, evaluates held-out rows, continues training on train rows, and keeps semantic-quality readiness closed.";

    private const string SelfAuthoredCategory = "synthetic_self_authored";
    private const string InternalLicense = "project_internal_self_authored";
    private const string NoTeacherValue = "none";
    private const string RepositoryOwnedModelCheckpointPendingPublicationSchema =
        "translation-repository-owned-model-checkpoint-pending-publication-v1";
    private const int RepositoryOwnedModelCheckpointPendingPublicationMaxBytes =
        64 * 1024;
    private const int HeldVerifiedRepositoryFileBufferSize = 81920;
    private static readonly UnixFileMode OwnerOnlyDirectoryMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute;
    private static readonly UnixFileMode GroupOrOtherDirectoryAccessMask =
        UnixFileMode.GroupRead |
        UnixFileMode.GroupWrite |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherWrite |
        UnixFileMode.OtherExecute;

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    internal static Action<string, string>? RepositoryOwnedModelCheckpointHeldFileValidationProbe { get; set; }

    [Obsolete(
        "Brokerless Run fails closed; use Run(string, Func<LlmModel>, Func<LlmModel>, Func<LlmModel>, IServerOwnedDpoTrainingLeaseBroker, TranslationDpoTrainEvaluateRunnerOptions).",
        error: false)]
    public static OperationResult<TranslationDpoTrainEvaluateRunReport> Run(
        string seedCorpusPath,
        DpoTrainer trainer,
        TranslationDpoTrainEvaluateRunnerOptions options)
    {
        if (trainer is null)
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                "Translation DPO train/evaluate runner requires a DpoTrainer instance.",
                "DPO_TRAINER_REQUIRED");
        return RunCoreAsync(
                seedCorpusPath,
                options,
                reproducibilityEvidence: null,
                (_, _) => OperationResult<AdmittedTrainerScope>.Fail(
                    "Translation DPO training requires the ModelServer-owned mutation-admission broker.",
                    MutationAdmissionBrokerRequiredErrorCode),
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    [Obsolete(
        "Brokered Run without a fresh-load factory fails closed; use Run(string, Func<LlmModel>, Func<LlmModel>, Func<LlmModel>, IServerOwnedDpoTrainingLeaseBroker, TranslationDpoTrainEvaluateRunnerOptions).",
        error: false)]
    public static OperationResult<TranslationDpoTrainEvaluateRunReport> Run(
        string seedCorpusPath,
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        IServerOwnedDpoTrainingLeaseBroker leaseBroker,
        TranslationDpoTrainEvaluateRunnerOptions options)
        => OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
            "Translation DPO train/evaluate runner requires policy, reference, and fresh-load model factories so the server-seeded LoRA topology can be prepared atomically before training.",
            "MODEL_FACTORY_REQUIRED");

    public static OperationResult<TranslationDpoTrainEvaluateRunReport> Run(
        string seedCorpusPath,
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        Func<LlmModel> createFreshLoadModel,
        IServerOwnedDpoTrainingLeaseBroker leaseBroker,
        TranslationDpoTrainEvaluateRunnerOptions options)
        => RunAsync(
                seedCorpusPath,
                createPolicyModel,
                createReferenceModel,
                createFreshLoadModel,
                leaseBroker,
                options,
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();

    public static async Task<
        OperationResult<TranslationDpoTrainEvaluateRunReport>> RunAsync(
        string seedCorpusPath,
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        Func<LlmModel> createFreshLoadModel,
        IServerOwnedDpoTrainingLeaseBroker leaseBroker,
        TranslationDpoTrainEvaluateRunnerOptions options,
        CancellationToken cancellationToken = default)
    {
        if (createPolicyModel is null ||
            createReferenceModel is null ||
            createFreshLoadModel is null)
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                "Translation DPO train/evaluate runner requires policy, reference, and fresh-load model factories.",
                "MODEL_FACTORY_REQUIRED");
        if (leaseBroker is null)
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                "Translation DPO training requires the ModelServer-owned mutation-admission broker.",
                MutationAdmissionBrokerRequiredErrorCode);

        var reproducibilityEvidence = ValidateReproducibilityContext(
            options.ReproducibilityContext);
        if (!reproducibilityEvidence.IsSuccess)
            return reproducibilityEvidence.MapError<
                TranslationDpoTrainEvaluateRunReport>();

        // Проверка контекста стоит до RunCore намеренно: RunCore уже читает
        // corpus, готовит артефакты и берёт право на mutation-admission.
        // Без серверного seed/config обучение перевода вообще не начинается.
        return await RunCoreAsync(
                seedCorpusPath,
                options,
                reproducibilityEvidence.Value!,
                (handoff, mutationSnapshot) => CreateAdmittedTrainer(
                    handoff,
                    mutationSnapshot,
                    () => PrepareInitialSeededModels(
                        createPolicyModel,
                        createReferenceModel,
                        createFreshLoadModel,
                        options.ReproducibilityContext!,
                        options.RepositoryOwnedModelCheckpointContext
                        ?? TranslationRepositoryOwnedModelCheckpointContext
                            .Disabled),
                    leaseBroker,
                    options.CheckpointDirectory,
                    learningRate: 1e-5f,
                    runPurpose: "initial",
                    options.TrainStep),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<
        OperationResult<TranslationDpoTrainEvaluateRunReport>> RunCoreAsync(
        string seedCorpusPath,
        TranslationDpoTrainEvaluateRunnerOptions options,
        DpoTrainingReproducibilityEvidence? reproducibilityEvidence,
        Func<TranslationDpoTrainingHandoff,
            TranslationDpoMutationBatchSnapshot,
            OperationResult<AdmittedTrainerScope>> createTrainer,
        CancellationToken cancellationToken)
    {
        var pathValidation = ValidateAndNormalizePaths(seedCorpusPath, options);
        if (!pathValidation.IsSuccess)
            return pathValidation.MapError<TranslationDpoTrainEvaluateRunReport>();

        var paths = pathValidation.Value!;
        var batchId = string.IsNullOrWhiteSpace(options.TrainBatchId)
            ? $"{Path.GetFileNameWithoutExtension(paths.SeedCorpusPath)}-train"
            : options.TrainBatchId!.Trim();
        var handoffId = string.IsNullOrWhiteSpace(options.HandoffId)
            ? $"{Path.GetFileNameWithoutExtension(paths.SeedCorpusPath)}-dpo-train-evaluate-handoff"
            : options.HandoffId!.Trim();

        var repositoryContext =
            options.RepositoryOwnedDpoDatasetContext ??
            TranslationRepositoryOwnedDpoDatasetContext.Disabled;
        var repositoryContextValidation = repositoryContext.Validate();
        if (!repositoryContextValidation.IsSuccess)
            return repositoryContextValidation
                .MapError<TranslationDpoTrainEvaluateRunReport>();

        var modelCheckpointContext =
            options.RepositoryOwnedModelCheckpointContext ??
            TranslationRepositoryOwnedModelCheckpointContext.Disabled;
        var modelCheckpointContextValidation =
            modelCheckpointContext.Validate();
        if (!modelCheckpointContextValidation.IsSuccess)
            return modelCheckpointContextValidation
                .MapError<TranslationDpoTrainEvaluateRunReport>();

        if (modelCheckpointContext.Enabled && !repositoryContext.Enabled)
        {
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                "Translation repository-owned model checkpoint publication requires the repository-owned DPO dataset context so the checkpoint can bind to an exact sealed DPO manifest.",
                RepositoryOwnedModelCheckpointDpoContextRequiredErrorCode);
        }

        var outputBoundaryValidation =
            ValidateRepositoryOwnedModelOutputIsOutsideCallerCheckpointDirectory(
                modelCheckpointContext,
                paths);
        if (!outputBoundaryValidation.IsSuccess)
        {
            return outputBoundaryValidation
                .MapError<TranslationDpoTrainEvaluateRunReport>();
        }

        RepositoryOwnedDpoDatasetResolution? repositoryResolution = null;
        var repositoryOwnedDpoRequiresCurrentSourceIdentity =
            repositoryContext.Enabled &&
            repositoryContext.CleanMissAuthoringContext is not null;
        if (repositoryContext.Enabled)
        {
            if (!repositoryOwnedDpoRequiresCurrentSourceIdentity)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (repositoryContext.RequestIdentity is null)
                {
                    var manifestIdentity = RepositoryOwnedDpoDatasetManifestSealer
                        .OpenManifestIdentityOnly(repositoryContext.Address!);
                    if (manifestIdentity.IsSuccess &&
                        manifestIdentity.Value is not null)
                    {
                        repositoryContext = repositoryContext with
                        {
                            RequestIdentity =
                                manifestIdentity.Value.RequestIdentity
                        };
                    }
                    else if (File.Exists(repositoryContext.Address!.ManifestPath))
                    {
                        // WHY: a present but unreadable manifest is corruption,
                        // not a clean miss. Never ask the teacher to overwrite it.
                        return manifestIdentity.MapError<
                            TranslationDpoTrainEvaluateRunReport>();
                    }
                }

                // Disk-first and creator-free: a caller-supplied sealed identity
                // may be reused without the open teacher only when this run is
                // not responsible for deriving that identity from current rows.
                if (repositoryContext.RequestIdentity is not null)
                {
                    var existing = RepositoryOwnedDpoDatasetResolver
                        .OpenExistingOrCreateAndSeal(
                            repositoryContext.Address!,
                            repositoryContext.RequestIdentity,
                            createPairs: null);
                    if (existing.IsSuccess && existing.Value is not null)
                    {
                        repositoryResolution = existing.Value;
                    }
                    else if (!IsCleanMissingRepositoryOwnedDpoDataset(existing))
                    {
                        return existing.MapError<
                            TranslationDpoTrainEvaluateRunReport>();
                    }
                }
            }
        }

        TranslationSeedCorpus corpus;
        try
        {
            corpus = TranslationSeedCorpusLoader.Load(paths.SeedCorpusPath);
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                $"Translation seed corpus could not be loaded for DPO train/evaluate run: {ex.Message}",
                "SEED_CORPUS_LOAD_FAILED");
        }

        var corpusValidation = ValidateCorpusBoundaries(corpus);
        if (!corpusValidation.IsSuccess)
            return corpusValidation.MapError<TranslationDpoTrainEvaluateRunReport>();

        TranslationTrainingDataArtifactLoadResult? artifact = null;
        var legacyTrainingDataArtifactLoaded = false;
        TranslationDpoTrainingHandoff handoff;

        try
        {
            if (repositoryContext.Enabled &&
                repositoryResolution is not null)
            {
                handoff = TranslationDpoTrainingHandoffBuilder
                    .BuildRepositoryOwnedDpoTrainHandoff(
                        handoffId,
                        batchId,
                        repositoryResolution.Dataset.Pairs,
                        corpus.HeldOutRows);
            }
            else
            {
                if (repositoryContext.Enabled)
                {
                    var authoring = repositoryContext
                        .CleanMissAuthoringContext;
                    if (authoring is null)
                    {
                        return OperationResult<
                            TranslationDpoTrainEvaluateRunReport>.Fail(
                                "A missing repository-owned Translation DPO dataset requires the admitted local Qwen authoring context; paid reference rows cannot be relabeled as teacher output.",
                                TranslationRepositoryOwnedDpoDatasetAuthoring
                                    .InvalidErrorCode);
                    }

                    // Репозиторный DPO сверяется с текущим seed-corpus напрямую:
                    // старый training-data artifact не является источником
                    // истины для clean miss или exact hit и не должен ломать
                    // повторный запуск, когда сохранённый набор уже лежит в Git.
                    var currentTrainRows = corpus
                        .ToTrainReadyPairs(TranslationSeedCorpusSplit.Train);
                    var prepared = TranslationRepositoryOwnedDpoDatasetAuthoring
                        .PrepareRequest(currentTrainRows, authoring);
                    if (!prepared.IsSuccess || prepared.Value is null)
                    {
                        return prepared.MapError<
                            TranslationDpoTrainEvaluateRunReport>();
                    }
                    if (repositoryContext.RequestIdentity is not null &&
                        !string.Equals(
                            repositoryContext.RequestIdentity
                                .RequestIdentitySha256,
                            prepared.Value.RequestIdentity
                                .RequestIdentitySha256,
                            StringComparison.Ordinal))
                    {
                        // WHY: an existing manifest can describe yesterday's
                        // prompt/input while sharing the same paid answer. The
                        // trainer may reuse only a dataset whose canonical
                        // request digest still matches the current source rows.
                        // Comparing the digest avoids false mismatches caused by
                        // two equal list values being held by different objects.
                        return OperationResult<
                            TranslationDpoTrainEvaluateRunReport>.Fail(
                                "The supplied Translation DPO request identity does not match the current source rows selected for repository-owned clean-miss authoring.",
                                RepositoryOwnedDpoDatasetManifestSealer
                                    .RequestIdentityMismatchErrorCode);
                    }

                    repositoryContext = repositoryContext with
                    {
                        RequestIdentity = prepared.Value.RequestIdentity
                    };
                    var sealedDataset = await RepositoryOwnedDpoDatasetResolver
                        .OpenExistingOrCreateAndSealAsync(
                            repositoryContext.Address!,
                            prepared.Value.RequestIdentity,
                            (address, token) =>
                                TranslationRepositoryOwnedDpoDatasetAuthoring
                                    .AuthorPairsAsync(
                                        address,
                                        prepared.Value,
                                        authoring,
                                        token),
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (!sealedDataset.IsSuccess ||
                        sealedDataset.Value is null)
                    {
                        return sealedDataset
                            .MapError<TranslationDpoTrainEvaluateRunReport>();
                    }

                    repositoryResolution = sealedDataset.Value;
                    handoff = TranslationDpoTrainingHandoffBuilder
                        .BuildRepositoryOwnedDpoTrainHandoff(
                            handoffId,
                            batchId,
                            repositoryResolution.Dataset.Pairs,
                            corpus.HeldOutRows);
                }
                else
                {
                    var trainingDataArtifact =
                        LoadValidatedTrainingDataArtifact(
                            paths.TrainingDataArtifactRoot);
                    if (!trainingDataArtifact.IsSuccess)
                        return trainingDataArtifact
                            .MapError<TranslationDpoTrainEvaluateRunReport>();

                    artifact = trainingDataArtifact.Value!;
                    legacyTrainingDataArtifactLoaded = true;
                    var artifactValidation =
                        ValidateTrainingDataArtifactMatchesCorpus(
                            corpus,
                            artifact);
                    if (!artifactValidation.IsSuccess)
                    {
                        return artifactValidation
                            .MapError<TranslationDpoTrainEvaluateRunReport>();
                    }

                    var trainBatch =
                        TranslationStructuredBatchBuilder.BuildGeneralBatch(
                            batchId,
                            artifact.TrainingPairs,
                            heldOut: false,
                            signatureMode: TranslationSignatureMode
                                .Structural);
                    handoff = TranslationDpoTrainingHandoffBuilder
                        .BuildSeedCorpusTrainHandoff(
                            handoffId,
                            corpus,
                            trainBatch,
                            artifact.TrainingPairs);
                }
            }
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoTrainEvaluateRunReport>.Fail(
                $"Translation DPO handoff could not be built: {ex.Message}",
                "HANDOFF_BUILD_FAILED");
        }

        RepositoryOwnedDpoDatasetManifestIdentity? dpoManifestIdentity = null;
        if (modelCheckpointContext.Enabled)
        {
            var identity =
                BuildRepositoryOwnedDpoDatasetManifestIdentity(
                    repositoryContext,
                    repositoryResolution);
            if (!identity.IsSuccess || identity.Value is null)
            {
                return identity.MapError<TranslationDpoTrainEvaluateRunReport>();
            }

            dpoManifestIdentity = identity.Value;
        }

        var mutationSnapshot = TranslationDpoTrainerBridge
            .BuildMutationBatchSnapshot(handoff, options.TokenizationSettings);
        if (!mutationSnapshot.IsSuccess || mutationSnapshot.Value is null)
        {
            return mutationSnapshot.MapError<
                TranslationDpoTrainEvaluateRunReport>();
        }

        var trainerResolution = createTrainer(handoff, mutationSnapshot.Value);
        if (!trainerResolution.IsSuccess || trainerResolution.Value is null)
            return trainerResolution.MapError<
                TranslationDpoTrainEvaluateRunReport>();
        using var trainerScope = trainerResolution.Value;

        TranslationRepositoryOwnedModelCheckpointPendingPublication?
            pendingPublication = null;
        TranslationRepositoryOwnedModelCheckpointRunEvidence?
            repositoryModelCheckpointEvidence = null;
        var checkpointPath = Path.Combine(
            trainerScope.CheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(options.TrainStep));
        var trainerMetricsPath = Path.Combine(
            trainerScope.CheckpointDirectory,
            DpoTrainer.BuildMetricsFileName(options.TrainStep));

        OperationResult<TranslationDpoTrainerBridgeResult> trainResult;
        if (modelCheckpointContext.Enabled)
        {
            var pending = BuildRepositoryOwnedModelCheckpointPendingPublication(
                modelCheckpointContext,
                trainerScope,
                dpoManifestIdentity!,
                DateTimeOffset.UtcNow,
                options.TrainStep);
            if (!pending.IsSuccess || pending.Value is null)
            {
                return pending.MapError<TranslationDpoTrainEvaluateRunReport>();
            }

            pendingPublication = pending.Value;
            var recovered =
                TryRecoverRepositoryOwnedModelCheckpointBeforeTraining(
                    pendingPublication,
                    trainerScope);
            if (!recovered.IsSuccess || recovered.Value is null)
            {
                return recovered.MapError<TranslationDpoTrainEvaluateRunReport>();
            }

            if (recovered.Value.Recovered)
            {
                repositoryModelCheckpointEvidence =
                    recovered.Value.Evidence;
                trainResult =
                    OperationResult<TranslationDpoTrainerBridgeResult>.Ok(
                        recovered.Value.TrainResult!);
            }
            else
            {
                var journal =
                    EnsureRepositoryOwnedModelCheckpointPendingPublicationJournal(
                        pendingPublication);
                if (!journal.IsSuccess)
                {
                    return journal.MapError<
                        TranslationDpoTrainEvaluateRunReport>();
                }

                trainResult =
                    TranslationDpoTrainerBridge.TrainAdmittedWithStepMany(
                        handoff,
                        trainerScope.MutationSnapshot,
                        trainerScope.Trainer,
                        options.TrainStep);
            }
        }
        else
        {
            trainResult = TranslationDpoTrainerBridge.TrainAdmittedWithStepMany(
                handoff,
                trainerScope.MutationSnapshot,
                trainerScope.Trainer,
                options.TrainStep);
        }

        if (!trainResult.IsSuccess)
            return trainResult.MapError<TranslationDpoTrainEvaluateRunReport>();

        if (modelCheckpointContext.Enabled &&
            pendingPublication is not null &&
            repositoryModelCheckpointEvidence is null)
        {
            var trainerArtifactBinding =
                BindRepositoryOwnedModelCheckpointPendingPublicationJournalToTrainerArtifacts(
                    pendingPublication);
            if (!trainerArtifactBinding.IsSuccess)
            {
                return trainerArtifactBinding.MapError<
                    TranslationDpoTrainEvaluateRunReport>();
            }
        }

        var heldOutResult = TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(
            handoff,
            trainerScope.Trainer,
            options.HeldOutEvaluationStep,
            options.TokenizationSettings);
        if (!heldOutResult.IsSuccess)
            return heldOutResult.MapError<TranslationDpoTrainEvaluateRunReport>();

        var splitValidation = ValidateSplitIsolation(trainResult.Value!, heldOutResult.Value!);
        if (!splitValidation.IsSuccess)
            return splitValidation.MapError<TranslationDpoTrainEvaluateRunReport>();

        var checkpointWritten = File.Exists(checkpointPath);
        var trainerMetricsWritten = File.Exists(trainerMetricsPath);
        var policyCheckpointSha256 = checkpointWritten
            ? ComputeSha256(checkpointPath)
            : null;
        if (modelCheckpointContext.Enabled &&
            repositoryModelCheckpointEvidence is null)
        {
            var modelCheckpointPublication =
                PublishRepositoryOwnedModelCheckpoint(
                    pendingPublication!,
                    trainerScope,
                    trainResult.Value!);
            if (!modelCheckpointPublication.IsSuccess ||
                modelCheckpointPublication.Value is null)
            {
                return modelCheckpointPublication
                    .MapError<TranslationDpoTrainEvaluateRunReport>();
            }

            repositoryModelCheckpointEvidence =
                modelCheckpointPublication.Value;
        }

        var repositoryOwnedEvidence =
            repositoryContext.Enabled && repositoryResolution is not null
                ? ToRepositoryOwnedDpoDatasetEvidence(
                    repositoryContext,
                    repositoryResolution,
                    paths.SeedCorpusPath,
                    legacyTrainingDataArtifactLoaded)
                : null;
        var trainingDataArtifactEvidence = artifact is not null
            ? ToArtifactEvidence(artifact)
            : ToRepositoryOwnedTrainingDataEvidence(
                repositoryContext,
                repositoryResolution!,
                paths);
        var trainSourceRowCount = repositoryResolution is not null
            ? repositoryResolution.Dataset.Pairs.Count
            : corpus.TrainRows.Count;

        var report = new TranslationDpoTrainEvaluateRunReport(
            Pipeline: modelCheckpointContext.Enabled
                ? RepositoryOwnedDpoAndModelCheckpointPipeline
                : repositoryContext.Enabled
                ? RepositoryOwnedDpoTrainingPairsPipeline
                : Pipeline,
            ReportPath: paths.OutputPath,
            SourceCorpusPath: paths.SeedCorpusPath,
            SourceCorpusSha256: ComputeSha256(paths.SeedCorpusPath),
            SourceCorpusRowCount: corpus.Rows.Count,
            TrainSourceRowCount: trainSourceRowCount,
            HeldOutSourceRowCount: corpus.HeldOutRows.Count,
            TrainingDataArtifactEvidence: trainingDataArtifactEvidence,
            HandoffId: handoff.HandoffId,
            SourceBatchId: handoff.SourceBatchId,
            TrainStep: options.TrainStep,
            HeldOutEvaluationStep: options.HeldOutEvaluationStep,
            TrainPairCount: trainResult.Value!.TrainPairCount,
            HeldOutCount: heldOutResult.Value!.HeldOutEvaluationCount,
            HeldOutExcludedFromTrainingCount: trainResult.Value.HeldOutExcludedCount,
            TrainExcludedFromHeldOutEvaluationCount: heldOutResult.Value.TrainExcludedCount,
            TrainMetrics: ToSnapshot(trainResult.Value.TrainerMetrics),
            HeldOutMetrics: ToSnapshot(heldOutResult.Value.EvaluationMetrics),
            TrainerMetricsPath: trainerMetricsWritten ? trainerMetricsPath : null,
            TrainerMetricsWritten: trainerMetricsWritten,
            CheckpointPath: checkpointWritten ? checkpointPath : null,
            CheckpointWritten: checkpointWritten,
            TrainedPairIds: trainResult.Value.TrainedPairIds,
            HeldOutEvaluationPairIds: heldOutResult.Value.HeldOutEvaluationPairIds,
            ExcludedTrainPairIds: heldOutResult.Value.ExcludedTrainPairIds,
            CapabilityIds: trainResult.Value.CapabilityIds
                .Concat(heldOutResult.Value.CapabilityIds)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Directions: trainResult.Value.Directions
                .Concat(heldOutResult.Value.Directions)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            LegalProvenanceValidated: true,
            ClosedTeacherRejected: true,
            HeldOutRowsPassedToStepMany: false,
            TrainRowsUsedForHeldOutEvaluation: false,
            SemanticQualityClaimed: false,
            SemanticQualityReadinessClaimed: false,
            SemanticQualityReadinessClosedReason: SemanticQualityReadinessClosedReason,
            TrainerCallPath: trainResult.Value.TrainerCallPath,
            EvaluationCallPath: heldOutResult.Value.TrainerCallPath)
        {
            ReproducibilityEvidence = reproducibilityEvidence,
            SeededLoraTopologyEvidence =
                trainerScope.SeededLoraTopologyEvidence,
            PolicyCheckpointSha256 = policyCheckpointSha256,
            RepositoryOwnedDpoDatasetEvidence = repositoryOwnedEvidence,
            RepositoryOwnedModelCheckpointEvidence =
                repositoryModelCheckpointEvidence
        };

        var writeResult = WriteReport(report, paths.OutputPath);
        if (!writeResult.IsSuccess)
            return writeResult.MapError<TranslationDpoTrainEvaluateRunReport>();

        if (pendingPublication is not null)
        {
            var cleanup =
                DeleteRepositoryOwnedModelCheckpointPendingPublicationJournal(
                    pendingPublication);
            if (!cleanup.IsSuccess)
            {
                return cleanup.MapError<TranslationDpoTrainEvaluateRunReport>();
            }
        }

        return OperationResult<TranslationDpoTrainEvaluateRunReport>.Ok(report);
    }

    private static bool IsCleanMissingRepositoryOwnedDpoDataset(
        OperationResult<RepositoryOwnedDpoDatasetResolution> resolution)
        => !resolution.IsSuccess &&
           string.Equals(
               resolution.ErrorCode,
               RepositoryOwnedDpoDatasetResolver.MissingErrorCode,
               StringComparison.Ordinal);

    private static bool IsCleanMissingRepositoryOwnedModelCheckpoint(
        OperationResult<RepositoryOwnedModelCheckpointLoadResult> resolution)
        => !resolution.IsSuccess &&
           (string.Equals(
                resolution.ErrorCode,
                RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode,
                StringComparison.Ordinal) ||
            string.Equals(
                resolution.ErrorCode,
                RepositoryOwnedModelCheckpointLoader
                    .NoSealedCandidateErrorCode,
                StringComparison.Ordinal));

    private static OperationResult<bool>
        ValidateRepositoryOwnedModelOutputIsOutsideCallerCheckpointDirectory(
            TranslationRepositoryOwnedModelCheckpointContext context,
            NormalizedRunnerPaths paths)
    {
        if (!context.Enabled)
            return OperationResult<bool>.Ok(true);

        var outputPathChainValidation =
            ValidateExistingPathChainHasNoReparsePoint(
                paths.OutputPath,
                "output path");
        if (!outputPathChainValidation.IsSuccess)
            return outputPathChainValidation;

        var checkpointDirectoryChainValidation =
            ValidateExistingPathChainHasNoReparsePoint(
                paths.CheckpointDirectory,
                "caller checkpoint directory");
        if (!checkpointDirectoryChainValidation.IsSuccess)
            return checkpointDirectoryChainValidation;

        if (IsInsideDirectory(paths.OutputPath, paths.CheckpointDirectory))
        {
            return OperationResult<bool>.Fail(
                "Translation repository-owned model checkpoint mode refuses to write the runner report inside the caller-supplied legacy checkpoint directory; use a separate output path outside that directory.",
                RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateExistingPathChainHasNoReparsePoint(
        string path,
        string pathRole)
    {
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception exception)
            when (IsPathChainInspectionFailure(exception))
        {
            return FailRepositoryOwnedModelCheckpointPathChainInspection(
                pathRole,
                exception);
        }

        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint mode could not prove the {pathRole} path root before training starts.",
                RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode);
        }

        var rootValidation =
            ValidateExistingPathSegmentHasNoReparsePoint(root, pathRole);
        if (!rootValidation.IsSuccess)
            return rootValidation;
        if (!rootValidation.Value)
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint mode could not prove the {pathRole} path root exists before training starts.",
                RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode);
        }

        var current = root;
        var relativePath = fullPath[root.Length..];
        var segments = relativePath.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            current = Path.Combine(current, segment);
            var segmentValidation =
                ValidateExistingPathSegmentHasNoReparsePoint(
                    current,
                    pathRole);
            if (!segmentValidation.IsSuccess)
                return segmentValidation;
            if (!segmentValidation.Value)
                return OperationResult<bool>.Ok(true);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool>
        ValidateExistingPathSegmentHasNoReparsePoint(
            string path,
            string pathRole)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                return FailRepositoryOwnedModelCheckpointReparsePath(pathRole);

            return OperationResult<bool>.Ok(true);
        }
        catch (FileNotFoundException)
        {
            return ValidateMissingPathSegmentIsNotObservableLink(
                path,
                pathRole);
        }
        catch (DirectoryNotFoundException)
        {
            return ValidateMissingPathSegmentIsNotObservableLink(
                path,
                pathRole);
        }
        catch (Exception exception)
            when (IsPathChainInspectionFailure(exception))
        {
            return FailRepositoryOwnedModelCheckpointPathChainInspection(
                pathRole,
                exception);
        }
    }

    private static OperationResult<bool>
        ValidateMissingPathSegmentIsNotObservableLink(
            string path,
            string pathRole)
    {
        var directoryLinkValidation =
            ValidateFileSystemInfoIsNotObservableLink(
                new DirectoryInfo(path),
                pathRole);
        if (!directoryLinkValidation.IsSuccess ||
            directoryLinkValidation.Value)
            return directoryLinkValidation;

        var fileLinkValidation =
            ValidateFileSystemInfoIsNotObservableLink(
                new FileInfo(path),
                pathRole);
        if (!fileLinkValidation.IsSuccess ||
            fileLinkValidation.Value)
            return fileLinkValidation;

        return OperationResult<bool>.Ok(false);
    }

    private static OperationResult<bool> ValidateFileSystemInfoIsNotObservableLink(
        FileSystemInfo info,
        string pathRole)
    {
        try
        {
            if (info.LinkTarget is not null)
                return FailRepositoryOwnedModelCheckpointReparsePath(pathRole);

            return OperationResult<bool>.Ok(false);
        }
        catch (FileNotFoundException)
        {
            return OperationResult<bool>.Ok(false);
        }
        catch (DirectoryNotFoundException)
        {
            return OperationResult<bool>.Ok(false);
        }
        catch (Exception exception)
            when (IsPathChainInspectionFailure(exception))
        {
            return FailRepositoryOwnedModelCheckpointPathChainInspection(
                pathRole,
                exception);
        }
    }

    private static OperationResult<bool>
        FailRepositoryOwnedModelCheckpointReparsePath(string pathRole)
        => OperationResult<bool>.Fail(
            $"Translation repository-owned model checkpoint mode refuses a {pathRole} whose existing path chain contains a symbolic link, junction, or reparse point; use ordinary directories so the report and caller checkpoint boundary is physically provable before training starts.",
            RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode);

    private static OperationResult<bool>
        FailRepositoryOwnedModelCheckpointPathChainInspection(
            string pathRole,
            Exception exception)
        => OperationResult<bool>.Fail(
            $"Translation repository-owned model checkpoint mode could not prove the {pathRole} path chain is free of symbolic links, junctions, or reparse points before training starts ({exception.GetType().Name}).",
            RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode);

    private static bool IsPathChainInspectionFailure(Exception exception)
        => exception is ArgumentException
            or IOException
            or NotSupportedException
            or UnauthorizedAccessException;

    private static OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>
        BuildRepositoryOwnedDpoDatasetManifestIdentity(
            TranslationRepositoryOwnedDpoDatasetContext context,
            RepositoryOwnedDpoDatasetResolution? resolution)
    {
        if (!context.Enabled ||
            context.Address is null ||
            resolution is null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>
                .Fail(
                    "Translation repository-owned model checkpoint publication requires an opened sealed DPO dataset manifest.",
                    RepositoryOwnedModelCheckpointDpoContextRequiredErrorCode);
        }

        return OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>.Ok(
            new RepositoryOwnedDpoDatasetManifestIdentity(
                context.Address.ManifestRelativePath,
                resolution.Dataset.Manifest.ManifestSha256));
    }

    private static OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
        PublishRepositoryOwnedModelCheckpoint(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication,
            AdmittedTrainerScope trainerScope,
            TranslationDpoTrainerBridgeResult trainResult)
    {
        if (pendingPublication is null ||
            trainerScope.RepositoryOwnedModelCheckpointStart is null)
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
                .Fail(
                    "Translation repository-owned model checkpoint publication requires an enabled model context and pre-training start evidence.",
                    RepositoryOwnedModelCheckpointPublishFailedErrorCode);
        }

        var request = pendingPublication.Request;
        var start = trainerScope.RepositoryOwnedModelCheckpointStart;
        var address = pendingPublication.Address;
        if (address is null)
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
                .Fail(
                    "Translation repository-owned model checkpoint publication requires the pre-training canonical next-step address.",
                    RepositoryOwnedModelCheckpointPublishFailedErrorCode);
        }

        if (!File.Exists(pendingPublication.TrainerPolicyPath))
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
                .Fail(
                    "Translation repository-owned model checkpoint publication requires the policy bytes written by DpoTrainer in the canonical next-step directory.",
                    RepositoryOwnedModelCheckpointPublishFailedErrorCode);
        }

        var journal =
            ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                pendingPublication.PendingJournalPath);
        if (!journal.IsSuccess || journal.Value is null)
        {
            return journal.MapError<
                TranslationRepositoryOwnedModelCheckpointRunEvidence>();
        }

        var journalValidation =
            ValidateRepositoryOwnedModelCheckpointPendingJournal(
                pendingPublication,
                journal.Value,
                requireTrainerArtifactBindings: true);
        if (!journalValidation.IsSuccess)
        {
            return journalValidation.MapError<
                TranslationRepositoryOwnedModelCheckpointRunEvidence>();
        }

        var heldTrainerPolicyResult = OpenHeldVerifiedRepositoryFile(
            pendingPublication.TrainerPolicyPath,
            journal.Value.TrainerPolicySha256!,
            journal.Value.TrainerPolicyByteLength!.Value,
            "repository-owned trainer policy checkpoint for publication",
            RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
        if (!heldTrainerPolicyResult.IsSuccess ||
            heldTrainerPolicyResult.Value is null)
        {
            return heldTrainerPolicyResult.MapError<
                TranslationRepositoryOwnedModelCheckpointRunEvidence>();
        }
        using var heldTrainerPolicy = heldTrainerPolicyResult.Value;

        var effectivePublication =
            WithJournalTimestampAndProvenance(
                pendingPublication,
                journal.Value);
        var sealedCheckpoint =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                effectivePublication.Address,
                destination => CopyHeldVerifiedRepositoryFileToSealer(
                    heldTrainerPolicy,
                    destination),
                effectivePublication.ManifestRequest);
        if (!sealedCheckpoint.IsSuccess ||
            sealedCheckpoint.Value is null)
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
                .Fail(
                    sealedCheckpoint.ErrorMessage,
                    string.IsNullOrWhiteSpace(sealedCheckpoint.ErrorCode)
                        ? RepositoryOwnedModelCheckpointPublishFailedErrorCode
                        : sealedCheckpoint.ErrorCode);
        }

        var reopened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(address);
        if (!reopened.IsSuccess ||
            reopened.Value is null ||
            !string.Equals(
                reopened.Value.Manifest.ManifestSha256,
                sealedCheckpoint.Value.ManifestSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                reopened.Value.WeightsSha256,
                sealedCheckpoint.Value.WeightsSha256,
                StringComparison.Ordinal) ||
            reopened.Value.WeightsByteLength !=
            sealedCheckpoint.Value.WeightsByteLength)
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
                .Fail(
                    "Translation repository-owned model checkpoint was not freshly reopened and verified after publication.",
                    RepositoryOwnedModelCheckpointVerifyFailedErrorCode);
        }

        return OperationResult<TranslationRepositoryOwnedModelCheckpointRunEvidence>
            .Ok(ToRepositoryOwnedModelCheckpointEvidence(
                request,
                start,
                address,
                reopened.Value.Manifest));
    }

    private static OperationResult<TranslationRepositoryOwnedModelCheckpointStart?>
        LoadRepositoryOwnedPolicyCheckpointAfterSeededTopology(
            LlmModel policy,
            TranslationRepositoryOwnedModelCheckpointContext context)
    {
        var validation = context.Validate();
        if (!validation.IsSuccess)
        {
            return validation
                .MapError<TranslationRepositoryOwnedModelCheckpointStart?>();
        }

        if (!context.Enabled)
        {
            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Ok(null);
        }

        var request = context.Request!;
        var pendingPublicationStart =
            TryBuildRepositoryOwnedModelCheckpointStartForPendingPublicationRecovery(
                policy,
                request);
        if (!pendingPublicationStart.IsSuccess)
        {
            return pendingPublicationStart
                .MapError<TranslationRepositoryOwnedModelCheckpointStart?>();
        }

        if (pendingPublicationStart.Value is not null)
        {
            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Ok(
                pendingPublicationStart.Value);
        }

        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                request.Address.RepositoryRoot,
                request.Address.DomainSlug,
                request.Address.DesignPatternSlug,
                request.Address.ModelIdSlug,
                request.Address.ModelVersion,
                request.RuntimeCompatibility);
        if (loaded.IsSuccess)
        {
            if (loaded.Value is null)
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                    "Translation repository-owned model checkpoint loader returned no opened parent checkpoint.",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
            }

            using var openedParent = loaded.Value;
            try
            {
                // WHY: LoRA adapters are created before this call, so the
                // repository stream can hydrate the policy role's exact saved
                // topology instead of training another base model from scratch.
                policy.LoadCheckpoint(openedParent.Weights);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                    $"Translation repository-owned policy checkpoint could not be loaded into the already seeded policy model: {exception.Message}",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
            }

            var start = BuildRepositoryOwnedModelCheckpointStart(
                ParentManifest: openedParent.Manifest,
                ColdStartReason: null,
                LoaderDiagnostics: openedParent.Diagnostics,
                request);
            if (!start.IsSuccess || start.Value is null)
            {
                return start
                    .MapError<TranslationRepositoryOwnedModelCheckpointStart?>();
            }

            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Ok(
                start.Value);
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded) &&
            request.AllowColdStart)
        {
            var start = BuildRepositoryOwnedModelCheckpointStart(
                ParentManifest: null,
                ColdStartReason: request.ColdStartReason,
                LoaderDiagnostics:
                [
                    string.IsNullOrWhiteSpace(loaded.ErrorMessage)
                        ? "No sealed repository-owned parent checkpoint exists for this exact Translation model identity."
                        : loaded.ErrorMessage
                ],
                request);
            if (!start.IsSuccess || start.Value is null)
            {
                return start
                    .MapError<TranslationRepositoryOwnedModelCheckpointStart?>();
            }

            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Ok(
                start.Value);
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded))
        {
            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                "Translation repository-owned model checkpoint cold start is denied: no sealed parent exists for this exact model identity and no explicit human cold-start reason was supplied.",
                RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode);
        }

        return loaded
            .MapError<TranslationRepositoryOwnedModelCheckpointStart?>();
    }

    private static OperationResult<TranslationRepositoryOwnedModelCheckpointStart?>
        TryBuildRepositoryOwnedModelCheckpointStartForPendingPublicationRecovery(
            LlmModel policy,
            TranslationRepositoryOwnedModelCheckpointRequest request)
    {
        var stepOne = request.Address.CreateStepAddress(1);
        if (!stepOne.IsSuccess || stepOne.Value is null)
        {
            return stepOne.MapError<
                TranslationRepositoryOwnedModelCheckpointStart?>();
        }

        var checkpointsRoot = Directory.GetParent(stepOne.Value.CheckpointRoot)
            ?.FullName;
        if (string.IsNullOrWhiteSpace(checkpointsRoot) ||
            !Directory.Exists(checkpointsRoot))
        {
            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Ok(null);
        }

        try
        {
            var pendingCandidates = new List<(
                RepositoryOwnedModelCheckpointAddress Address,
                TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
                    Journal)>();
            var compatibleParents =
                new List<RepositoryOwnedModelCheckpointManifest>();

            foreach (var directory in Directory.EnumerateDirectories(
                         checkpointsRoot))
            {
                var stepSlug = Path.GetFileName(directory);
                var journalPath = Path.Combine(
                    directory,
                    RepositoryOwnedModelCheckpointPendingPublicationFileName);
                var hasJournal = File.Exists(journalPath);
                if (!RepositoryOwnedTrainingArtifactLayout
                        .TryParseModelCheckpointStepSlug(stepSlug, out var step))
                {
                    if (hasJournal)
                    {
                        return OperationResult<
                            TranslationRepositoryOwnedModelCheckpointStart?>
                            .Fail(
                                "Translation repository-owned model checkpoint recovery found a pending-publication journal under a non-canonical step directory.",
                                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
                    }

                    continue;
                }

                var address = request.Address.CreateStepAddress(step);
                if (!address.IsSuccess || address.Value is null)
                {
                    return address.MapError<
                        TranslationRepositoryOwnedModelCheckpointStart?>();
                }

                if (hasJournal)
                {
                    var journal =
                        ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                            journalPath);
                    if (!journal.IsSuccess || journal.Value is null)
                    {
                        return journal.MapError<
                            TranslationRepositoryOwnedModelCheckpointStart?>();
                    }

                    if (!PendingPublicationJournalMatchesModelRequest(
                            journal.Value,
                            address.Value,
                            request))
                    {
                        return OperationResult<
                            TranslationRepositoryOwnedModelCheckpointStart?>
                            .Fail(
                                "Translation repository-owned model checkpoint recovery found a pending-publication journal that does not belong to the current model request.",
                                RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
                    }

                    pendingCandidates.Add((address.Value, journal.Value));
                    continue;
                }

                if (File.Exists(address.Value.ManifestPath))
                {
                    var opened =
                        RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                            address.Value);
                    if (!opened.IsSuccess || opened.Value is null)
                    {
                        // Этот обход ищет только незавершённую публикацию, которую можно
                        // продолжить, и собирает возможных родителей для неё. Он не является
                        // классификатором кандидатов: по договору загрузчика испорченный,
                        // подделанный или непрочитываемый шаг обязан получить код
                        // "кандидат недействителен" от RepositoryOwnedModelCheckpointLoader,
                        // который перечисляет шаги сразу после нас и закрывается отказом,
                        // не загрузив старые байты. Раньше мы возвращали наружу внутренний
                        // код "паспорт недействителен" и обрывали путь ещё до загрузчика,
                        // хотя журнала незавершённой публикации у этого шага нет вовсе.
                        // Поэтому непрочитываемый паспорт просто не становится родителем:
                        // шаг пропускается, судит его загрузчик.
                        continue;
                    }

                    if (RuntimeCompatibilityEquals(
                            opened.Value.Manifest.RuntimeCompatibility,
                            request.RuntimeCompatibility))
                    {
                        compatibleParents.Add(opened.Value.Manifest);
                    }

                    continue;
                }

                if (HasRepositoryOwnedModelCheckpointTrainerOrSealerArtifacts(
                        address.Value,
                        assumedTrainerStep: 1))
                {
                    return OperationResult<
                        TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                        "Translation repository-owned model checkpoint recovery found trainer or sealer artifacts without a pending-publication journal or final manifest, so it refuses to guess whether these bytes belong to the current request.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
                }
            }

            if (pendingCandidates.Count == 0)
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Ok(null);
            }

            if (pendingCandidates.Count > 1)
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                    "Translation repository-owned model checkpoint recovery found more than one pending-publication journal for the same model identity, so it refuses to choose one.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            }

            var pending = pendingCandidates[0];
            var parent = compatibleParents
                .OrderBy(static manifest => manifest.Step)
                .LastOrDefault();
            if (parent is null)
            {
                if (pending.Address.Step != 1 ||
                    pending.Journal.ParentStep is not null ||
                    !request.AllowColdStart)
                {
                    return OperationResult<
                        TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                        "Translation repository-owned model checkpoint recovery found a pending publication, but it is not the first cold-start step and no compatible sealed parent was proven.",
                        RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
                }

                return BuildPendingPublicationRecoveryStart(
                    policy,
                    request,
                    pending.Journal,
                    parentManifest: null);
            }

            var parentIdentity = ToParentIdentity(parent);
            if (pending.Address.Step != parent.Step + 1 ||
                !ParentIdentityEquals(pending.Journal, parentIdentity))
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                    "Translation repository-owned model checkpoint recovery found a pending publication that is not the actual next step after the latest compatible sealed parent.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            }

            return BuildPendingPublicationRecoveryStart(
                policy,
                request,
                pending.Journal,
                parent);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                $"Translation repository-owned model checkpoint recovery could not inspect pending-publication journals before strict loading: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static bool PendingPublicationJournalMatchesModelRequest(
        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
            journal,
        RepositoryOwnedModelCheckpointAddress address,
        TranslationRepositoryOwnedModelCheckpointRequest request)
        => string.Equals(
               journal.Schema,
               RepositoryOwnedModelCheckpointPendingPublicationSchema,
               StringComparison.Ordinal) &&
           string.Equals(
               journal.Runner,
               nameof(TranslationDpoTrainEvaluateRunner),
               StringComparison.Ordinal) &&
           string.Equals(
               journal.RepositoryRelativeCheckpointDirectory,
               address.CheckpointRootRelativePath,
               StringComparison.Ordinal) &&
           string.Equals(
               journal.RepositoryRelativeWeightsPath,
               address.WeightsRelativePath,
               StringComparison.Ordinal) &&
           string.Equals(
               journal.RepositoryRelativeManifestPath,
               address.ManifestRelativePath,
               StringComparison.Ordinal) &&
           string.Equals(journal.DomainSlug, request.Address.DomainSlug,
               StringComparison.Ordinal) &&
           string.Equals(journal.DesignPatternSlug,
               request.Address.DesignPatternSlug, StringComparison.Ordinal) &&
           string.Equals(journal.ModelIdSlug, request.Address.ModelIdSlug,
               StringComparison.Ordinal) &&
           string.Equals(journal.ModelVersion, request.Address.ModelVersion,
               StringComparison.Ordinal) &&
           journal.Step == address.Step &&
           RuntimeCompatibilityEquals(
               journal.RuntimeCompatibility,
               request.RuntimeCompatibility);

    private static bool HasRepositoryOwnedModelCheckpointTrainerOrSealerArtifacts(
        RepositoryOwnedModelCheckpointAddress address,
        int assumedTrainerStep)
        => File.Exists(address.WeightsPath) ||
           File.Exists(address.ManifestPath) ||
           File.Exists(Path.Combine(
               address.CheckpointRoot,
               DpoTrainer.BuildPolicyCheckpointFileName(assumedTrainerStep))) ||
           File.Exists(Path.Combine(
               address.CheckpointRoot,
               DpoTrainer.BuildMetricsFileName(assumedTrainerStep)));

    private static OperationResult<TranslationRepositoryOwnedModelCheckpointStart?>
        BuildPendingPublicationRecoveryStart(
            LlmModel policy,
            TranslationRepositoryOwnedModelCheckpointRequest request,
            TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
                journal,
            RepositoryOwnedModelCheckpointManifest? parentManifest)
    {
        if (parentManifest is not null)
        {
            var parentAddress = request.Address.CreateStepAddress(
                parentManifest.Step);
            if (!parentAddress.IsSuccess || parentAddress.Value is null)
            {
                return parentAddress.MapError<
                    TranslationRepositoryOwnedModelCheckpointStart?>();
            }

            var openedParent =
                RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                    parentAddress.Value);
            if (!openedParent.IsSuccess || openedParent.Value is null)
            {
                return openedParent.MapError<
                    TranslationRepositoryOwnedModelCheckpointStart?>();
            }

            var heldParentWeightsResult = OpenHeldVerifiedRepositoryFile(
                parentAddress.Value.WeightsPath,
                openedParent.Value.WeightsSha256,
                openedParent.Value.WeightsByteLength,
                "repository-owned parent checkpoint weights",
                RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            if (!heldParentWeightsResult.IsSuccess ||
                heldParentWeightsResult.Value is null)
            {
                return heldParentWeightsResult.MapError<
                    TranslationRepositoryOwnedModelCheckpointStart?>();
            }
            using var heldParentWeights = heldParentWeightsResult.Value;

            try
            {
                // Recovery starts from the same parent as the interrupted run:
                // if the journal only says "step two" but the policy model is
                // seeded from scratch, a crash-before-trainer retry would train
                // a different checkpoint line.
                policy.LoadCheckpoint(heldParentWeights.Stream);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointStart?>.Fail(
                    $"Translation repository-owned pending checkpoint parent could not be loaded before recovery: {exception.Message}",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
            }
        }

        var start = BuildRepositoryOwnedModelCheckpointStart(
            parentManifest,
            parentManifest is null ? journal.ColdStartReason : null,
            LoaderDiagnostics:
            [
                "Pending-publication journal found before strict loader; recovery will validate DPO identity, runtime, parent, provenance and trainer bytes before any new mutation.",
                "The pending publication remains the actual next checkpoint step, so retry must finish that step instead of advancing the model line."
            ],
            request);
        if (!start.IsSuccess || start.Value is null)
        {
            return start.MapError<
                TranslationRepositoryOwnedModelCheckpointStart?>();
        }

        return OperationResult<
            TranslationRepositoryOwnedModelCheckpointStart?>.Ok(start.Value);
    }

    private static OperationResult<TranslationRepositoryOwnedModelCheckpointStart>
        BuildRepositoryOwnedModelCheckpointStart(
            RepositoryOwnedModelCheckpointManifest? ParentManifest,
            string? ColdStartReason,
            IReadOnlyList<string> LoaderDiagnostics,
            TranslationRepositoryOwnedModelCheckpointRequest request)
    {
        int nextStep;
        try
        {
            nextStep = ParentManifest is null
                ? 1
                : checked(ParentManifest.Step + 1);
        }
        catch (OverflowException)
        {
            return OperationResult<TranslationRepositoryOwnedModelCheckpointStart>
                .Fail(
                    "Translation repository-owned model checkpoint parent step cannot be continued because the next step overflows Int32.",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
        }

        var address = request.Address.CreateStepAddress(nextStep);
        if (!address.IsSuccess || address.Value is null)
        {
            return address
                .MapError<TranslationRepositoryOwnedModelCheckpointStart>();
        }

        return OperationResult<TranslationRepositoryOwnedModelCheckpointStart>
            .Ok(new TranslationRepositoryOwnedModelCheckpointStart(
                ParentManifest,
                ColdStartReason,
                LoaderDiagnostics,
                address.Value));
    }

    private static void CopyHeldVerifiedRepositoryFileToSealer(
        HeldVerifiedRepositoryFile source,
        Stream destination)
    {
        source.Rewind();
        source.Stream.CopyTo(destination, HeldVerifiedRepositoryFileBufferSize);
    }

    private static OperationResult<HeldVerifiedRepositoryFile>
        OpenHeldVerifiedRepositoryFile(
            string path,
            string expectedSha256,
            long expectedByteLength,
            string fileRole,
            string mismatchErrorCode)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            string.IsNullOrWhiteSpace(expectedSha256) ||
            expectedByteLength <= 0)
        {
            return OperationResult<HeldVerifiedRepositoryFile>.Fail(
                $"Translation repository-owned model checkpoint recovery cannot consume {fileRole} because the expected file identity is incomplete.",
                mismatchErrorCode);
        }

        var pathValidation = ValidateExistingPathChainHasNoReparsePoint(
            path,
            fileRole);
        if (!pathValidation.IsSuccess)
        {
            return pathValidation.MapError<HeldVerifiedRepositoryFile>();
        }

        FileStream? stream = null;
        try
        {
            stream = new FileStream(
                path,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.Read,
                    BufferSize = HeldVerifiedRepositoryFileBufferSize,
                    Options = FileOptions.SequentialScan
                });

            var actualByteLength = stream.Length;
            var actualSha256 =
                Convert.ToHexString(SHA256.HashData(stream))
                    .ToLowerInvariant();
            if (actualByteLength != expectedByteLength ||
                !string.Equals(
                    actualSha256,
                    expectedSha256,
                    StringComparison.Ordinal))
            {
                stream.Dispose();
                return OperationResult<HeldVerifiedRepositoryFile>.Fail(
                    $"Translation repository-owned model checkpoint recovery refuses {fileRole} because the bytes no longer match the journal or manifest binding.",
                    mismatchErrorCode);
            }

            stream.Position = 0;
            // Tests use this hook to try a path swap after validation. The
            // production code must still consume this already-open stream, not
            // reopen the path and accidentally read the swapped file.
            RepositoryOwnedModelCheckpointHeldFileValidationProbe?.Invoke(
                fileRole,
                path);
            stream.Position = 0;

            var held = new HeldVerifiedRepositoryFile(
                path,
                actualSha256,
                actualByteLength,
                stream);
            stream = null;
            return OperationResult<HeldVerifiedRepositoryFile>.Ok(held);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<HeldVerifiedRepositoryFile>.Fail(
                $"Translation repository-owned model checkpoint recovery could not hold and verify {fileRole}: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    private static OperationResult<
            TranslationRepositoryOwnedModelCheckpointPendingPublication>
        BuildRepositoryOwnedModelCheckpointPendingPublication(
            TranslationRepositoryOwnedModelCheckpointContext context,
            AdmittedTrainerScope trainerScope,
            RepositoryOwnedDpoDatasetManifestIdentity dpoManifestIdentity,
            DateTimeOffset createdAtUtc,
            int trainerStep)
    {
        if (!context.Enabled ||
            context.Request is null ||
            trainerScope.RepositoryOwnedModelCheckpointStart is null)
        {
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointPendingPublication>
                .Fail(
                    "Translation repository-owned model checkpoint publication requires an enabled model context, DPO manifest identity, and pre-training start evidence.",
                    RepositoryOwnedModelCheckpointPublishFailedErrorCode);
        }

        var request = context.Request;
        var start = trainerScope.RepositoryOwnedModelCheckpointStart;
        var address = start.NextStepAddress;
        var manifestRequest = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: start.ParentManifest is null
                ? null
                : ToParentIdentity(start.ParentManifest),
            ColdStartReason: start.ParentManifest is null
                ? start.ColdStartReason!
                : string.Empty,
            DpoDatasetManifest: dpoManifestIdentity,
            RuntimeCompatibility: request.RuntimeCompatibility,
            CreatedAtUtc: createdAtUtc,
            ProvenanceRefs: BuildRepositoryOwnedModelCheckpointProvenanceRefs(
                request,
                start,
                dpoManifestIdentity,
                trainerStep));

        return OperationResult<
                TranslationRepositoryOwnedModelCheckpointPendingPublication>
            .Ok(new TranslationRepositoryOwnedModelCheckpointPendingPublication(
                request,
                start,
                address,
                manifestRequest,
                Path.Combine(
                    address.CheckpointRoot,
                    RepositoryOwnedModelCheckpointPendingPublicationFileName),
                Path.Combine(
                    address.CheckpointRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(trainerStep)),
                Path.Combine(
                    address.CheckpointRoot,
                    DpoTrainer.BuildMetricsFileName(trainerStep)),
                trainerStep));
    }

    private static OperationResult<
            TranslationRepositoryOwnedModelCheckpointRecoveryProbe>
        TryRecoverRepositoryOwnedModelCheckpointBeforeTraining(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication,
            AdmittedTrainerScope trainerScope)
    {
        var hasJournal = File.Exists(pendingPublication.PendingJournalPath);
        var hasTrainerPolicy = File.Exists(pendingPublication.TrainerPolicyPath);
        var hasTrainerMetrics = File.Exists(pendingPublication.TrainerMetricsPath);
        var hasSealerWeights = File.Exists(pendingPublication.Address.WeightsPath);
        var hasSealerManifest = File.Exists(pendingPublication.Address.ManifestPath);
        if (!hasJournal)
        {
            if (hasTrainerPolicy || hasTrainerMetrics || hasSealerWeights ||
                hasSealerManifest)
            {
                return OperationResult<
                        TranslationRepositoryOwnedModelCheckpointRecoveryProbe>
                    .Fail(
                        "Translation repository-owned model checkpoint recovery found trainer or sealer artifacts without the matching pending-publication journal, so it refuses to guess whether these bytes belong to the current request.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
            }

            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>.Ok(
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe
                    .NotRecovered);
        }

        var journal = ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
            pendingPublication.PendingJournalPath);
        if (!journal.IsSuccess || journal.Value is null)
        {
            return journal.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }

        var validation = ValidateRepositoryOwnedModelCheckpointPendingJournal(
            pendingPublication,
            journal.Value,
            requireTrainerArtifactBindings: false);
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }

        if (!hasTrainerPolicy && !hasTrainerMetrics && !hasSealerWeights &&
            !hasSealerManifest)
        {
            // Процесс мог умереть сразу после записи намерения, но до записи
            // весов. Такой журнал не является готовым результатом: текущий запуск
            // продолжает обычное обучение под тем же проверенным намерением.
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>.Ok(
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe
                    .NotRecovered);
        }

        if (!hasTrainerPolicy || !hasTrainerMetrics)
        {
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>
                .Fail(
                    "Translation repository-owned model checkpoint recovery requires both trainer policy bytes and trainer metrics before it can skip DpoTrainer.StepMany.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
        }

        var trainerBindingValidation =
            ValidateRepositoryOwnedModelCheckpointPendingJournal(
                pendingPublication,
                journal.Value,
                requireTrainerArtifactBindings: true);
        if (!trainerBindingValidation.IsSuccess)
        {
            return trainerBindingValidation.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }

        var heldTrainerPolicyResult = OpenHeldVerifiedRepositoryFile(
            pendingPublication.TrainerPolicyPath,
            journal.Value.TrainerPolicySha256!,
            journal.Value.TrainerPolicyByteLength!.Value,
            "repository-owned recovered trainer policy checkpoint",
            RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
        if (!heldTrainerPolicyResult.IsSuccess ||
            heldTrainerPolicyResult.Value is null)
        {
            return heldTrainerPolicyResult.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }
        using var heldTrainerPolicy = heldTrainerPolicyResult.Value;

        var heldTrainerMetricsResult = OpenHeldVerifiedRepositoryFile(
            pendingPublication.TrainerMetricsPath,
            journal.Value.TrainerMetricsSha256!,
            journal.Value.TrainerMetricsByteLength!.Value,
            "repository-owned recovered trainer metrics",
            RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
        if (!heldTrainerMetricsResult.IsSuccess ||
            heldTrainerMetricsResult.Value is null)
        {
            return heldTrainerMetricsResult.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }
        using var heldTrainerMetrics = heldTrainerMetricsResult.Value;

        if (hasSealerWeights)
        {
            var heldSealerWeightsResult = OpenHeldVerifiedRepositoryFile(
                pendingPublication.Address.WeightsPath,
                heldTrainerPolicy.Sha256,
                heldTrainerPolicy.ByteLength,
                "repository-owned orphan sealer weights",
                RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            if (!heldSealerWeightsResult.IsSuccess ||
                heldSealerWeightsResult.Value is null)
            {
                return heldSealerWeightsResult.MapError<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
            }
            using var heldSealerWeights = heldSealerWeightsResult.Value;
        }

        var effectivePublication =
            WithJournalTimestampAndProvenance(
                pendingPublication,
                journal.Value);
        OperationResult<RepositoryOwnedModelCheckpointManifest> sealedCheckpoint;
        if (hasSealerManifest)
        {
            var opened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                effectivePublication.Address);
            if (!opened.IsSuccess || opened.Value is null)
            {
                return opened.MapError<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
            }

            sealedCheckpoint =
                OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                    opened.Value.Manifest);
        }
        else
        {
            sealedCheckpoint =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    effectivePublication.Address,
                    destination => CopyHeldVerifiedRepositoryFileToSealer(
                        heldTrainerPolicy,
                        destination),
                    effectivePublication.ManifestRequest);
        }

        if (!sealedCheckpoint.IsSuccess || sealedCheckpoint.Value is null)
        {
            return sealedCheckpoint.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }

        var reopened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
            effectivePublication.Address);
        if (!reopened.IsSuccess ||
            reopened.Value is null ||
            !string.Equals(
                reopened.Value.Manifest.ManifestSha256,
                sealedCheckpoint.Value.ManifestSha256,
                StringComparison.Ordinal) ||
            !string.Equals(
                reopened.Value.WeightsSha256,
                sealedCheckpoint.Value.WeightsSha256,
                StringComparison.Ordinal) ||
            reopened.Value.WeightsByteLength !=
            sealedCheckpoint.Value.WeightsByteLength)
        {
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>
                .Fail(
                    "Translation repository-owned model checkpoint recovery could not reopen the sealed checkpoint it just completed.",
                    RepositoryOwnedModelCheckpointVerifyFailedErrorCode);
        }

        var trainMetrics = ReadRepositoryOwnedModelCheckpointRecoveredMetrics(
            heldTrainerMetrics,
            effectivePublication.TrainerStep);
        if (!trainMetrics.IsSuccess || trainMetrics.Value is null)
        {
            return trainMetrics.MapError<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>();
        }

        try
        {
            // Восстановленный путь не вызывает StepMany повторно: он загружает
            // уже записанные trainer-байты в policy-модель и дальше разрешает
            // только безопасную held-out оценку для нового JSON-отчёта.
            heldTrainerPolicy.Rewind();
            trainerScope.PolicyModel.LoadCheckpoint(
                heldTrainerPolicy.Stream);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointRecoveryProbe>
                .Fail(
                    $"Translation repository-owned model checkpoint recovery could not load the recovered trainer policy bytes: {exception.Message}",
                    RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode);
        }

        return OperationResult<
                TranslationRepositoryOwnedModelCheckpointRecoveryProbe>.Ok(
            new TranslationRepositoryOwnedModelCheckpointRecoveryProbe(
                Recovered: true,
                Evidence: ToRepositoryOwnedModelCheckpointEvidence(
                    effectivePublication.Request,
                    effectivePublication.Start,
                    effectivePublication.Address,
                    reopened.Value.Manifest),
                TrainResult: BuildRecoveredTranslationDpoTrainResult(
                    trainerScope.MutationSnapshot.BatchBuild,
                    trainMetrics.Value,
                    effectivePublication.TrainerStep)));
    }

    private static OperationResult<bool>
        EnsureRepositoryOwnedModelCheckpointPendingPublicationJournal(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication)
    {
        var existing =
            ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                pendingPublication.PendingJournalPath);
        if (existing.IsSuccess && existing.Value is not null)
        {
            return ValidateRepositoryOwnedModelCheckpointPendingJournal(
                pendingPublication,
                existing.Value,
                requireTrainerArtifactBindings: false);
        }

        if (File.Exists(pendingPublication.PendingJournalPath))
        {
            return existing.MapError<bool>();
        }

        var rootReady =
            EnsureRepositoryOwnedModelCheckpointRootForPendingJournal(
                pendingPublication.Address);
        if (!rootReady.IsSuccess)
        {
            return rootReady;
        }

        try
        {
            var temporaryPath = Path.Combine(
                pendingPublication.Address.CheckpointRoot,
                "." + RepositoryOwnedModelCheckpointPendingPublicationFileName +
                "." + Guid.NewGuid().ToString("N") + ".tmp");
            var journal = BuildRepositoryOwnedModelCheckpointPendingJournal(
                pendingPublication);
            var json = JsonSerializer.Serialize(journal, JsonOptions) +
                       Environment.NewLine;
            File.WriteAllText(temporaryPath, json, Utf8NoBom);
            File.Move(temporaryPath, pendingPublication.PendingJournalPath);
            return OperationResult<bool>.Ok(true);
        }
        catch (IOException) when (File.Exists(
                   pendingPublication.PendingJournalPath))
        {
            var raced =
                ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                    pendingPublication.PendingJournalPath);
            if (!raced.IsSuccess || raced.Value is null)
                return raced.MapError<bool>();
            return ValidateRepositoryOwnedModelCheckpointPendingJournal(
                pendingPublication,
                raced.Value,
                requireTrainerArtifactBindings: false);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint recovery could not write the pending-publication journal before DpoTrainer.StepMany: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static OperationResult<bool>
        EnsureRepositoryOwnedModelCheckpointRootForPendingJournal(
            RepositoryOwnedModelCheckpointAddress address)
    {
        var pathValidation = ValidateExistingPathChainHasNoReparsePoint(
            address.CheckpointRoot,
            "repository-owned pending checkpoint directory");
        if (!pathValidation.IsSuccess)
        {
            return pathValidation;
        }

        try
        {
            if (File.Exists(address.CheckpointRoot))
            {
                return OperationResult<bool>.Fail(
                    "Translation repository-owned model checkpoint pending journal cannot use a checkpoint path that is already a file.",
                    RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
            }

            if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(address.CheckpointRoot);
            }
            else
            {
                if (Directory.Exists(address.CheckpointRoot))
                {
                    File.SetUnixFileMode(
                        address.CheckpointRoot,
                        OwnerOnlyDirectoryMode);
                }
                else
                {
                    Directory.CreateDirectory(
                        address.CheckpointRoot,
                        OwnerOnlyDirectoryMode);
                }

                if (!HasOwnerOnlyUnixDirectoryMode(address.CheckpointRoot))
                {
                    return OperationResult<bool>.Fail(
                        "Translation repository-owned model checkpoint pending journal could not establish owner-only directory custody before writing recovery state.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint pending journal could not prepare its checkpoint directory: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static bool HasOwnerOnlyUnixDirectoryMode(string path)
        => OperatingSystem.IsWindows() ||
           ((File.GetUnixFileMode(path) & GroupOrOtherDirectoryAccessMask) == 0 &&
            (File.GetUnixFileMode(path) & OwnerOnlyDirectoryMode) ==
            OwnerOnlyDirectoryMode);

    private static OperationResult<bool>
        BindRepositoryOwnedModelCheckpointPendingPublicationJournalToTrainerArtifacts(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication)
    {
        var existing =
            ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                pendingPublication.PendingJournalPath);
        if (!existing.IsSuccess || existing.Value is null)
        {
            return existing.MapError<bool>();
        }

        var validation = ValidateRepositoryOwnedModelCheckpointPendingJournal(
            pendingPublication,
            existing.Value,
            requireTrainerArtifactBindings: false);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        if (!File.Exists(pendingPublication.TrainerPolicyPath) ||
            !File.Exists(pendingPublication.TrainerMetricsPath))
        {
            return OperationResult<bool>.Fail(
                "Translation repository-owned model checkpoint pending-publication journal cannot be bound because DpoTrainer has not written both policy and metrics artifacts.",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }

        var boundJournal = existing.Value with
        {
            TrainerPolicySha256 =
                ComputeSha256(pendingPublication.TrainerPolicyPath),
            TrainerPolicyByteLength =
                new FileInfo(pendingPublication.TrainerPolicyPath).Length,
            TrainerMetricsSha256 =
                ComputeSha256(pendingPublication.TrainerMetricsPath),
            TrainerMetricsByteLength =
                new FileInfo(pendingPublication.TrainerMetricsPath).Length
        };

        try
        {
            var temporaryPath = Path.Combine(
                pendingPublication.Address.CheckpointRoot,
                "." + RepositoryOwnedModelCheckpointPendingPublicationFileName +
                "." + Guid.NewGuid().ToString("N") + ".tmp");
            var json = JsonSerializer.Serialize(boundJournal, JsonOptions) +
                       Environment.NewLine;
            File.WriteAllText(temporaryPath, json, Utf8NoBom);
            File.Move(temporaryPath, pendingPublication.PendingJournalPath,
                overwrite: true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint recovery could not bind trainer artifact hashes into the pending-publication journal: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }

        var reread =
            ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
                pendingPublication.PendingJournalPath);
        if (!reread.IsSuccess || reread.Value is null)
        {
            return reread.MapError<bool>();
        }

        return ValidateRepositoryOwnedModelCheckpointPendingJournal(
            pendingPublication,
            reread.Value,
            requireTrainerArtifactBindings: true);
    }

    private static OperationResult<bool>
        DeleteRepositoryOwnedModelCheckpointPendingPublicationJournal(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication)
    {
        try
        {
            if (File.Exists(pendingPublication.PendingJournalPath))
                File.Delete(pendingPublication.PendingJournalPath);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                $"Translation repository-owned model checkpoint recovery sealed the checkpoint but could not remove the pending-publication journal: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static OperationResult<
            TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
        ReadRepositoryOwnedModelCheckpointPendingPublicationJournal(
            string pendingJournalPath)
    {
        try
        {
            if (!File.Exists(pendingJournalPath))
            {
                return OperationResult<
                        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
                    .Fail(
                        "Translation repository-owned model checkpoint pending-publication journal is absent.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
            }

            var info = new FileInfo(pendingJournalPath);
            if (info.Length <= 0 ||
                info.Length >
                RepositoryOwnedModelCheckpointPendingPublicationMaxBytes)
            {
                return OperationResult<
                        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
                    .Fail(
                        "Translation repository-owned model checkpoint pending-publication journal has an invalid size.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
            }

            var journal = JsonSerializer.Deserialize<
                TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>(
                    File.ReadAllText(pendingJournalPath, Utf8NoBom),
                    JsonOptions);
            return journal is null
                ? OperationResult<
                        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
                    .Fail(
                        "Translation repository-owned model checkpoint pending-publication journal could not be parsed.",
                        RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode)
                : OperationResult<
                        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
                    .Ok(journal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                    TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal>
                .Fail(
                    $"Translation repository-owned model checkpoint pending-publication journal is unreadable: {exception.Message}",
                    RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static OperationResult<bool>
        ValidateRepositoryOwnedModelCheckpointPendingJournal(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication,
            TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
                journal,
            bool requireTrainerArtifactBindings)
    {
        if (!string.Equals(
                journal.Schema,
                RepositoryOwnedModelCheckpointPendingPublicationSchema,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.Runner,
                nameof(TranslationDpoTrainEvaluateRunner),
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.RepositoryRelativeCheckpointDirectory,
                pendingPublication.Address.CheckpointRootRelativePath,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.RepositoryRelativeWeightsPath,
                pendingPublication.Address.WeightsRelativePath,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.RepositoryRelativeManifestPath,
                pendingPublication.Address.ManifestRelativePath,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.DomainSlug,
                pendingPublication.Address.DomainSlug,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.DesignPatternSlug,
                pendingPublication.Address.DesignPatternSlug,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.ModelIdSlug,
                pendingPublication.Address.ModelIdSlug,
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.ModelVersion,
                pendingPublication.Address.ModelVersion,
                StringComparison.Ordinal) ||
            journal.Step != pendingPublication.Address.Step ||
            journal.TrainerStep != pendingPublication.TrainerStep ||
            !string.Equals(
                journal.TrainerPolicyFileName,
                Path.GetFileName(pendingPublication.TrainerPolicyPath),
                StringComparison.Ordinal) ||
            !string.Equals(
                journal.TrainerMetricsFileName,
                Path.GetFileName(pendingPublication.TrainerMetricsPath),
                StringComparison.Ordinal) ||
            !RuntimeCompatibilityEquals(
                journal.RuntimeCompatibility,
                pendingPublication.ManifestRequest.RuntimeCompatibility) ||
            !DpoManifestIdentityEquals(
                journal.DpoDatasetManifest,
                pendingPublication.ManifestRequest.DpoDatasetManifest) ||
            !ParentIdentityEquals(
                journal,
                pendingPublication.ManifestRequest.ParentCheckpoint) ||
            !string.Equals(
                journal.ColdStartReason ?? string.Empty,
                pendingPublication.ManifestRequest.ColdStartReason ??
                string.Empty,
                StringComparison.Ordinal) ||
            journal.ProvenanceRefs is null ||
            !journal.ProvenanceRefs.SequenceEqual(
                pendingPublication.ManifestRequest.ProvenanceRefs,
                StringComparer.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "Translation repository-owned model checkpoint pending-publication journal does not match the current checkpoint address, DPO manifest, runtime compatibility, parent, or provenance.",
                RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
        }

        if (requireTrainerArtifactBindings)
        {
            if (string.IsNullOrWhiteSpace(journal.TrainerPolicySha256) ||
                journal.TrainerPolicyByteLength is null or <= 0 ||
                string.IsNullOrWhiteSpace(journal.TrainerMetricsSha256) ||
                journal.TrainerMetricsByteLength is null or <= 0 ||
                !File.Exists(pendingPublication.TrainerPolicyPath) ||
                !File.Exists(pendingPublication.TrainerMetricsPath))
            {
                return OperationResult<bool>.Fail(
                    "Translation repository-owned model checkpoint recovery requires the pending-publication journal to bind trainer policy and metrics by SHA-256 and byte length before it can skip DpoTrainer.StepMany.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            }

            if (!string.Equals(
                    ComputeSha256(pendingPublication.TrainerPolicyPath),
                    journal.TrainerPolicySha256,
                    StringComparison.Ordinal) ||
                new FileInfo(pendingPublication.TrainerPolicyPath).Length !=
                journal.TrainerPolicyByteLength.Value ||
                !string.Equals(
                    ComputeSha256(pendingPublication.TrainerMetricsPath),
                    journal.TrainerMetricsSha256,
                    StringComparison.Ordinal) ||
                new FileInfo(pendingPublication.TrainerMetricsPath).Length !=
                journal.TrainerMetricsByteLength.Value)
            {
                return OperationResult<bool>.Fail(
                    "Translation repository-owned model checkpoint recovery found trainer policy or metrics bytes that no longer match the pending-publication journal binding.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static TranslationRepositoryOwnedModelCheckpointPendingPublication
        WithJournalTimestampAndProvenance(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication,
            TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
                journal)
        => pendingPublication with
        {
            ManifestRequest = pendingPublication.ManifestRequest with
            {
                CreatedAtUtc = journal.CreatedAtUtc,
                ProvenanceRefs = Array.AsReadOnly(
                    journal.ProvenanceRefs.ToArray())
            }
        };

    private static TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
        BuildRepositoryOwnedModelCheckpointPendingJournal(
            TranslationRepositoryOwnedModelCheckpointPendingPublication
                pendingPublication)
    {
        var parent = pendingPublication.ManifestRequest.ParentCheckpoint;
        return new TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal(
            Schema: RepositoryOwnedModelCheckpointPendingPublicationSchema,
            Runner: nameof(TranslationDpoTrainEvaluateRunner),
            RepositoryRelativeCheckpointDirectory:
                pendingPublication.Address.CheckpointRootRelativePath,
            RepositoryRelativeWeightsPath:
                pendingPublication.Address.WeightsRelativePath,
            RepositoryRelativeManifestPath:
                pendingPublication.Address.ManifestRelativePath,
            DomainSlug: pendingPublication.Address.DomainSlug,
            DesignPatternSlug: pendingPublication.Address.DesignPatternSlug,
            ModelIdSlug: pendingPublication.Address.ModelIdSlug,
            ModelVersion: pendingPublication.Address.ModelVersion,
            Step: pendingPublication.Address.Step,
            ParentManifestRelativePath: parent?.RepositoryRelativeManifestPath,
            ParentStep: parent?.Step,
            ParentWeightsSha256: parent?.CheckpointSha256,
            ParentWeightsByteLength: parent?.CheckpointByteLength,
            ColdStartReason: pendingPublication.ManifestRequest.ColdStartReason,
            DpoDatasetManifest:
                pendingPublication.ManifestRequest.DpoDatasetManifest,
            RuntimeCompatibility:
                pendingPublication.ManifestRequest.RuntimeCompatibility,
            TrainerStep: pendingPublication.TrainerStep,
            TrainerPolicyFileName:
                Path.GetFileName(pendingPublication.TrainerPolicyPath),
            TrainerMetricsFileName:
                Path.GetFileName(pendingPublication.TrainerMetricsPath),
            TrainerPolicySha256: null,
            TrainerPolicyByteLength: null,
            TrainerMetricsSha256: null,
            TrainerMetricsByteLength: null,
            CreatedAtUtc: pendingPublication.ManifestRequest.CreatedAtUtc,
            ProvenanceRefs: pendingPublication.ManifestRequest.ProvenanceRefs
                .ToArray());
    }

    private static OperationResult<DpoMetrics>
        ReadRepositoryOwnedModelCheckpointRecoveredMetrics(
            HeldVerifiedRepositoryFile trainerMetrics,
            int expectedTrainerStep)
    {
        try
        {
            trainerMetrics.Rewind();
            var metrics = JsonSerializer.Deserialize<DpoMetrics>(
                trainerMetrics.Stream);
            if (metrics is null || metrics.Step != expectedTrainerStep)
            {
                return OperationResult<DpoMetrics>.Fail(
                    "Translation repository-owned model checkpoint recovery found trainer metrics that do not belong to the pending trainer step.",
                    RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode);
            }

            return OperationResult<DpoMetrics>.Ok(metrics);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DpoMetrics>.Fail(
                $"Translation repository-owned model checkpoint recovery could not read trainer metrics: {exception.Message}",
                RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode);
        }
    }

    private static TranslationDpoTrainerBridgeResult
        BuildRecoveredTranslationDpoTrainResult(
            TranslationDpoBatchBuildResult batchBuild,
            DpoMetrics trainMetrics,
            int trainerStep)
        => new(
            Step: trainerStep,
            TrainPairCount: batchBuild.TrainPairCount,
            HeldOutExcludedCount: batchBuild.HeldOutExcludedCount,
            TrainerMetrics: trainMetrics,
            TrainedPairIds: batchBuild.Records
                .Select(record => record.PairId)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            HeldOutEvaluationPairIds: batchBuild.HeldOutEvaluationPairIds,
            CapabilityIds: batchBuild.CapabilityIds,
            Directions: batchBuild.Directions,
            TrainerCallPath:
                TranslationDpoTrainerBridge.TrainerCallPath +
                ":recovered-from-pending-repository-checkpoint",
            NextEvaluationBoundary:
                TranslationDpoTrainerBridge.NextEvaluationBoundary);

    private static bool RuntimeCompatibilityEquals(
        RepositoryOwnedModelRuntimeCompatibility left,
        RepositoryOwnedModelRuntimeCompatibility right)
        => string.Equals(left.ConfigSha256, right.ConfigSha256,
               StringComparison.Ordinal) &&
           string.Equals(left.TopologySha256, right.TopologySha256,
               StringComparison.Ordinal) &&
           string.Equals(left.TokenizerSha256, right.TokenizerSha256,
               StringComparison.Ordinal) &&
           string.Equals(left.TrainerSha256, right.TrainerSha256,
               StringComparison.Ordinal) &&
           string.Equals(left.SchemaSha256, right.SchemaSha256,
               StringComparison.Ordinal) &&
           string.Equals(left.CompatibilityKey, right.CompatibilityKey,
               StringComparison.Ordinal);

    private static bool DpoManifestIdentityEquals(
        RepositoryOwnedDpoDatasetManifestIdentity left,
        RepositoryOwnedDpoDatasetManifestIdentity right)
        => string.Equals(
               left.RepositoryRelativeManifestPath,
               right.RepositoryRelativeManifestPath,
               StringComparison.Ordinal) &&
           string.Equals(
               left.ManifestSha256,
               right.ManifestSha256,
               StringComparison.Ordinal);

    private static bool ParentIdentityEquals(
        TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal
            journal,
        RepositoryOwnedModelCheckpointParentIdentity? parent)
    {
        if (parent is null)
        {
            return journal.ParentManifestRelativePath is null &&
                   journal.ParentStep is null &&
                   journal.ParentWeightsSha256 is null &&
                   journal.ParentWeightsByteLength is null;
        }

        return string.Equals(
                   journal.ParentManifestRelativePath,
                   parent.RepositoryRelativeManifestPath,
                   StringComparison.Ordinal) &&
               journal.ParentStep == parent.Step &&
               string.Equals(
                   journal.ParentWeightsSha256,
                   parent.CheckpointSha256,
                   StringComparison.Ordinal) &&
               journal.ParentWeightsByteLength == parent.CheckpointByteLength;
    }

    private static RepositoryOwnedModelCheckpointParentIdentity ToParentIdentity(
        RepositoryOwnedModelCheckpointManifest manifest)
        => new(
            manifest.RepositoryRelativeManifestPath,
            manifest.Step,
            manifest.WeightsSha256,
            manifest.WeightsByteLength);

    private static IReadOnlyList<string>
        BuildRepositoryOwnedModelCheckpointProvenanceRefs(
            TranslationRepositoryOwnedModelCheckpointRequest request,
            TranslationRepositoryOwnedModelCheckpointStart start,
            RepositoryOwnedDpoDatasetManifestIdentity dpoManifestIdentity,
            int trainerStep)
    {
        var refs = request.ProvenanceRefs
            .Concat([
                "translation-dpo-train-evaluate-runner",
                "trainer-step:" +
                trainerStep.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "dpo-manifest:" +
                dpoManifestIdentity.RepositoryRelativeManifestPath,
                start.ParentManifest is null
                    ? "model-start:cold"
                    : "model-start:parent-step-" +
                      start.ParentManifest.Step.ToString(
                          System.Globalization.CultureInfo.InvariantCulture)
            ])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return Array.AsReadOnly(refs);
    }

    private static TranslationRepositoryOwnedModelCheckpointRunEvidence
        ToRepositoryOwnedModelCheckpointEvidence(
            TranslationRepositoryOwnedModelCheckpointRequest request,
            TranslationRepositoryOwnedModelCheckpointStart start,
            RepositoryOwnedModelCheckpointAddress address,
            RepositoryOwnedModelCheckpointManifest manifest)
        => new(
            Enabled: true,
            Authority: RepositoryOwnedTrainingArtifactLayout
                .ModelArtifactRootRelativePath,
            StartMode: start.ParentManifest is null
                ? "cold_start_explicit"
                : "continued_from_latest_compatible_parent",
            RepositoryRoot: request.Address.RepositoryRoot,
            DomainSlug: request.Address.DomainSlug,
            DesignPatternSlug: request.Address.DesignPatternSlug,
            ModelIdSlug: request.Address.ModelIdSlug,
            ModelVersion: request.Address.ModelVersion,
            PublishedStep: manifest.Step,
            PublishedManifestPath: address.ManifestPath,
            PublishedManifestRelativePath:
                manifest.RepositoryRelativeManifestPath,
            PublishedWeightsPath: address.WeightsPath,
            PublishedWeightsRelativePath:
                manifest.RepositoryRelativeWeightsPath,
            PublishedWeightsSha256: manifest.WeightsSha256,
            PublishedWeightsByteLength: manifest.WeightsByteLength,
            ParentStep: start.ParentManifest?.Step,
            ParentManifestRelativePath:
                start.ParentManifest?.RepositoryRelativeManifestPath,
            ParentWeightsSha256: start.ParentManifest?.WeightsSha256,
            ParentWeightsByteLength: start.ParentManifest?.WeightsByteLength,
            ColdStartReason: start.ParentManifest is null
                ? start.ColdStartReason
                : null,
            LoaderDiagnostics: start.LoaderDiagnostics,
            Boundary:
                "Enabled Translation DPO model continuation derives the DpoTrainer checkpoint directory from the repository-owned training-artifacts/models next-step root; the caller-supplied legacy checkpoint directory receives no model, metric, lock, or report artifact.");

    [Obsolete(
        "Brokerless RunFreshLoadContinueEvaluate fails closed; use RunFreshLoadContinueEvaluate(string, TranslationDpoTrainEvaluateRunReport, Func<LlmModel>, Func<LlmModel>, IServerOwnedDpoTrainingLeaseBroker, TranslationDpoCheckpointLifecycleRunnerOptions).",
        error: false)]
    public static OperationResult<TranslationDpoCheckpointLifecycleRunReport> RunFreshLoadContinueEvaluate(
        string seedCorpusPath,
        TranslationDpoTrainEvaluateRunReport sourceReport,
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        TranslationDpoCheckpointLifecycleRunnerOptions options)
        => OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
            "Translation DPO continuation training requires the ModelServer-owned mutation-admission broker.",
            MutationAdmissionBrokerRequiredErrorCode);

    public static OperationResult<TranslationDpoCheckpointLifecycleRunReport> RunFreshLoadContinueEvaluate(
        string seedCorpusPath,
        TranslationDpoTrainEvaluateRunReport sourceReport,
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        IServerOwnedDpoTrainingLeaseBroker leaseBroker,
        TranslationDpoCheckpointLifecycleRunnerOptions options)
    {
        if (sourceReport is null)
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                "Translation DPO checkpoint lifecycle runner requires the source train/evaluate report.",
                "SOURCE_REPORT_REQUIRED");

        if (createPolicyModel is null || createReferenceModel is null)
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                "Translation DPO checkpoint lifecycle runner requires fresh policy and reference model factories.",
                "MODEL_FACTORY_REQUIRED");
        if (leaseBroker is null)
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                "Translation DPO continuation training requires the ModelServer-owned mutation-admission broker.",
                MutationAdmissionBrokerRequiredErrorCode);

        var reproducibilityEvidence = ValidateReproducibilityContext(
            options.ReproducibilityContext);
        if (!reproducibilityEvidence.IsSuccess)
        {
            return reproducibilityEvidence.MapError<
                TranslationDpoCheckpointLifecycleRunReport>();
        }

        // Старые JSON-отчёты читаются как раньше, но continuation по ним
        // запрещён: источник обязан доказать, с каким серверным контекстом был
        // создан initial checkpoint.
        var sourceReproducibilityValidation =
            ValidateSourceReportReproducibility(
                sourceReport,
                reproducibilityEvidence.Value!);
        if (!sourceReproducibilityValidation.IsSuccess)
        {
            return sourceReproducibilityValidation.MapError<
                TranslationDpoCheckpointLifecycleRunReport>();
        }

        var pathValidation = ValidateAndNormalizeLifecyclePaths(seedCorpusPath, options);
        if (!pathValidation.IsSuccess)
            return pathValidation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var paths = pathValidation.Value!;
        var sourceReportValidation = ValidateSourceReportForLifecycle(sourceReport, paths.SeedCorpusPath);
        if (!sourceReportValidation.IsSuccess)
            return sourceReportValidation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var loadedCheckpointPath = sourceReportValidation.Value!;
        TranslationSeedCorpus corpus;
        try
        {
            corpus = TranslationSeedCorpusLoader.Load(paths.SeedCorpusPath);
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                $"Translation seed corpus could not be loaded for DPO checkpoint lifecycle run: {ex.Message}",
                "SEED_CORPUS_LOAD_FAILED");
        }

        var corpusValidation = ValidateCorpusBoundaries(corpus);
        if (!corpusValidation.IsSuccess)
            return corpusValidation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        TranslationDpoTrainingHandoff handoff;
        try
        {
            var trainBatch = corpus.BuildStructuredBatch(
                sourceReport.SourceBatchId,
                TranslationSeedCorpusSplit.Train);
            handoff = TranslationDpoTrainingHandoffBuilder.BuildSeedCorpusTrainHandoff(
                sourceReport.HandoffId,
                corpus,
                trainBatch);
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                $"Translation DPO lifecycle handoff could not be rebuilt from the source report: {ex.Message}",
                "HANDOFF_REBUILD_FAILED");
        }

        var mutationSnapshot = TranslationDpoTrainerBridge
            .BuildMutationBatchSnapshot(handoff, options.TokenizationSettings);
        if (!mutationSnapshot.IsSuccess || mutationSnapshot.Value is null)
        {
            return mutationSnapshot.MapError<
                TranslationDpoCheckpointLifecycleRunReport>();
        }

        var trainerResolution = CreateAdmittedTrainer(
            handoff,
            mutationSnapshot.Value,
            () => PrepareContinuationSeededModels(
                createPolicyModel,
                createReferenceModel,
                loadedCheckpointPath,
                options.ReproducibilityContext!,
                sourceReport.SeededLoraTopologyEvidence!),
            leaseBroker,
            paths.ContinuationCheckpointDirectory,
            options.LearningRate,
            runPurpose: "continuation",
            options.ContinueTrainStep);
        if (!trainerResolution.IsSuccess || trainerResolution.Value is null)
        {
            return trainerResolution.MapError<
                TranslationDpoCheckpointLifecycleRunReport>();
        }
        using var trainerScope = trainerResolution.Value;
        var resumedTrainer = trainerScope.Trainer;

        var freshEvaluation = TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(
            handoff,
            resumedTrainer,
            options.FreshLoadEvaluationStep,
            options.TokenizationSettings);
        if (!freshEvaluation.IsSuccess)
            return freshEvaluation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var continueTraining = TranslationDpoTrainerBridge.TrainAdmittedWithStepMany(
            handoff,
            trainerScope.MutationSnapshot,
            resumedTrainer,
            options.ContinueTrainStep);
        if (!continueTraining.IsSuccess)
            return continueTraining.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var postContinueEvaluation = TranslationDpoTrainerBridge.EvaluateHeldOutWithEvaluateMany(
            handoff,
            resumedTrainer,
            options.PostContinueEvaluationStep,
            options.TokenizationSettings);
        if (!postContinueEvaluation.IsSuccess)
            return postContinueEvaluation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var freshSplitValidation = ValidateSplitIsolation(continueTraining.Value!, freshEvaluation.Value!);
        if (!freshSplitValidation.IsSuccess)
            return freshSplitValidation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var postSplitValidation = ValidateSplitIsolation(continueTraining.Value!, postContinueEvaluation.Value!);
        if (!postSplitValidation.IsSuccess)
            return postSplitValidation.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        var continuationCheckpointPath = Path.Combine(
            paths.ContinuationCheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(options.ContinueTrainStep));
        var continuationMetricsPath = Path.Combine(
            paths.ContinuationCheckpointDirectory,
            $"dpo_step_{options.ContinueTrainStep:D6}.json");
        var continuationCheckpointWritten = File.Exists(continuationCheckpointPath);
        var continuationMetricsWritten = File.Exists(continuationMetricsPath);

        if (!continuationCheckpointWritten || !continuationMetricsWritten)
            return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Fail(
                "Translation DPO lifecycle continue-training step did not write the expected policy checkpoint and metrics artifacts.",
                "CONTINUATION_ARTIFACTS_NOT_WRITTEN");

        var loadedPolicyCheckpointSha256 = ComputeSha256(loadedCheckpointPath);
        var continuationPolicyCheckpointSha256 =
            ComputeSha256(continuationCheckpointPath);

        var report = new TranslationDpoCheckpointLifecycleRunReport(
            Pipeline: CheckpointLifecyclePipeline,
            ReportPath: paths.OutputPath,
            SourceCorpusPath: paths.SeedCorpusPath,
            SourceCorpusSha256: ComputeSha256(paths.SeedCorpusPath),
            SourceCorpusRowCount: corpus.Rows.Count,
            TrainSourceRowCount: corpus.TrainRows.Count,
            HeldOutSourceRowCount: corpus.HeldOutRows.Count,
            PreviousRunnerReportPath: sourceReport.ReportPath,
            LoadedCheckpointPath: loadedCheckpointPath,
            CheckpointFileUsed: true,
            LoadedOrResumed: true,
            PolicyCheckpointLoaded: true,
            ReferenceModelRestoredFromCheckpoint: false,
            ReferenceModelBoundary: ReferenceModelBoundary,
            TrainerLoadCallPath: TrainerLoadCallPath,
            HandoffId: handoff.HandoffId,
            SourceBatchId: handoff.SourceBatchId,
            FreshLoadEvaluationStep: options.FreshLoadEvaluationStep,
            ContinueTrainStep: options.ContinueTrainStep,
            PostContinueEvaluationStep: options.PostContinueEvaluationStep,
            ContinueTrainPairCount: continueTraining.Value!.TrainPairCount,
            HeldOutCount: postContinueEvaluation.Value!.HeldOutEvaluationCount,
            HeldOutExcludedFromContinuationTrainingCount: continueTraining.Value.HeldOutExcludedCount,
            TrainExcludedFromFreshEvaluationCount: freshEvaluation.Value!.TrainExcludedCount,
            FreshLoadedHeldOutMetrics: ToSnapshot(freshEvaluation.Value.EvaluationMetrics),
            ContinueTrainMetrics: ToSnapshot(continueTraining.Value.TrainerMetrics),
            PostContinueHeldOutMetrics: ToSnapshot(postContinueEvaluation.Value.EvaluationMetrics),
            ContinuationTrainerMetricsPath: continuationMetricsPath,
            ContinuationTrainerMetricsWritten: continuationMetricsWritten,
            ContinuationCheckpointPath: continuationCheckpointPath,
            ContinuationCheckpointWritten: continuationCheckpointWritten,
            ContinuedTrainingPairIds: continueTraining.Value.TrainedPairIds,
            FreshHeldOutEvaluationPairIds: freshEvaluation.Value.HeldOutEvaluationPairIds,
            PostHeldOutEvaluationPairIds: postContinueEvaluation.Value.HeldOutEvaluationPairIds,
            ExcludedTrainPairIds: postContinueEvaluation.Value.ExcludedTrainPairIds,
            CapabilityIds: continueTraining.Value.CapabilityIds
                .Concat(freshEvaluation.Value.CapabilityIds)
                .Concat(postContinueEvaluation.Value.CapabilityIds)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Directions: continueTraining.Value.Directions
                .Concat(freshEvaluation.Value.Directions)
                .Concat(postContinueEvaluation.Value.Directions)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            HeldOutRowsPassedToContinueTraining: false,
            TrainRowsUsedForFreshEvaluation: false,
            SemanticQualityClaimed: false,
            SemanticQualityReadinessClaimed: false,
            SemanticQualityReadinessClosedReason: SemanticQualityReadinessClosedReason,
            LifecycleBoundary: CheckpointLifecycleBoundary)
        {
            ReproducibilityEvidence = reproducibilityEvidence.Value,
            SeededLoraTopologyEvidence =
                trainerScope.SeededLoraTopologyEvidence,
            LoadedPolicyCheckpointSha256 = loadedPolicyCheckpointSha256,
            ContinuationPolicyCheckpointSha256 =
                continuationPolicyCheckpointSha256
        };

        var writeResult = WriteReport(report, paths.OutputPath);
        if (!writeResult.IsSuccess)
            return writeResult.MapError<TranslationDpoCheckpointLifecycleRunReport>();

        return OperationResult<TranslationDpoCheckpointLifecycleRunReport>.Ok(report);
    }

    private static OperationResult<DpoTrainingReproducibilityEvidence>
        ValidateReproducibilityContext(
            DpoTrainingReproducibilityContext? context)
    {
        if (context is null)
        {
            return OperationResult<DpoTrainingReproducibilityEvidence>.Fail(
                "Translation DPO training requires a server-owned reproducibility context before files, factories, or mutation admission are touched.",
                ReproducibilityContextRequiredErrorCode);
        }

        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out var reason))
        {
            return OperationResult<DpoTrainingReproducibilityEvidence>.Fail(
                $"Translation DPO reproducibility context is invalid: {reason}",
                ReproducibilityContextInvalidErrorCode);
        }

        try
        {
            return OperationResult<DpoTrainingReproducibilityEvidence>.Ok(
                DpoTrainingReproducibilityEvidence.CreateCurrent(context));
        }
        catch (Exception ex)
        {
            return OperationResult<DpoTrainingReproducibilityEvidence>.Fail(
                $"Translation DPO reproducibility evidence could not be built: {ex.Message}",
                ReproducibilityContextInvalidErrorCode);
        }
    }

    private static OperationResult<bool> ValidateSourceReportReproducibility(
        TranslationDpoTrainEvaluateRunReport sourceReport,
        DpoTrainingReproducibilityEvidence expectedEvidence)
    {
        if (sourceReport.ReproducibilityEvidence is null)
        {
            return OperationResult<bool>.Fail(
                "Translation DPO continuation requires source report reproducibility evidence before loading any checkpoint or creating continuation artifacts.",
                ReproducibilityEvidenceRequiredErrorCode);
        }

        if (!DpoTrainingReproducibilityContextContract.TryValidateEvidence(
                sourceReport.ReproducibilityEvidence,
                out var reason))
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO source report reproducibility evidence is invalid: {reason}",
                ReproducibilityEvidenceInvalidErrorCode);
        }

        // Сравнение идёт по каноническому отпечатку, чтобы продолжение не
        // смешало отчёт от одного серверного seed/config с другой властью.
        if (!string.Equals(
                sourceReport.ReproducibilityEvidence.ContextSha256,
                expectedEvidence.ContextSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "Translation DPO continuation source report belongs to a different reproducibility context.",
                ReproducibilityEvidenceMismatchErrorCode);
        }

        if (sourceReport.SeededLoraTopologyEvidence is null)
        {
            return OperationResult<bool>.Fail(
                "Translation DPO continuation requires source report evidence that the initial policy/reference/fresh-load LoRA topology was prepared atomically before checkpoint publication.",
                SeededLoraTopologyEvidenceRequiredErrorCode);
        }

        if (!DpoTrainingSeededLoraTopologyContract
                .TryValidateApplicationEvidence(
                    sourceReport.SeededLoraTopologyEvidence,
                    expectedEvidence.Context,
                    out var topologyReason))
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO source report seeded LoRA topology evidence is invalid: {topologyReason}",
                SeededLoraTopologyEvidenceInvalidErrorCode);
        }

        if (string.IsNullOrWhiteSpace(sourceReport.PolicyCheckpointSha256))
        {
            return OperationResult<bool>.Fail(
                "Translation DPO continuation requires the SHA-256 of the source policy checkpoint before loading it.",
                PolicyCheckpointSha256RequiredErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<AdmittedTrainerScope> CreateAdmittedTrainer(
        TranslationDpoTrainingHandoff handoff,
        TranslationDpoMutationBatchSnapshot mutationSnapshot,
        Func<OperationResult<PreparedDpoModels>> prepareModels,
        IServerOwnedDpoTrainingLeaseBroker leaseBroker,
        string checkpointDirectory,
        float learningRate,
        string runPurpose,
        int mutationStep)
    {
        var prepared = prepareModels();
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<AdmittedTrainerScope>();

        var leaseResolution = leaseBroker.AcquireTranslationTrainingLease(
            mutationSnapshot.TrainingContent,
            BuildTrainingRunId(handoff, runPurpose, mutationStep));
        if (!leaseResolution.IsSuccess || leaseResolution.Value is null)
            return leaseResolution.MapError<AdmittedTrainerScope>();

        var lease = leaseResolution.Value;
        try
        {
            var capabilityResolution =
                lease.IssueDpoTrainerMutationAdmissionCapability();
            if (!capabilityResolution.IsSuccess ||
                capabilityResolution.Value is null)
            {
                lease.Dispose();
                return capabilityResolution.MapError<AdmittedTrainerScope>();
            }

            var trainerCheckpointDirectory =
                prepared.Value.RepositoryOwnedModelCheckpointStart?
                    .NextStepAddress.CheckpointRoot ??
                checkpointDirectory;
            var trainer = new DpoTrainer(
                prepared.Value.PolicyModel,
                prepared.Value.ReferenceModel,
                trainerCheckpointDirectory,
                capabilityResolution.Value,
                learningRate);
            return OperationResult<AdmittedTrainerScope>.Ok(
                new AdmittedTrainerScope(
                    trainer,
                    lease,
                    mutationSnapshot,
                    prepared.Value.SeededLoraTopologyEvidence,
                    prepared.Value.RepositoryOwnedModelCheckpointStart,
                    trainerCheckpointDirectory,
                    prepared.Value.PolicyModel));
        }
        catch (DpoMutationAdmissionException exception)
        {
            lease.Dispose();
            return OperationResult<AdmittedTrainerScope>.Fail(
                "Translation DPO mutation admission is not live.",
                exception.ErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            lease.Dispose();
            return OperationResult<AdmittedTrainerScope>.Fail(
                "Translation DPO admitted trainer could not be created.",
                "DPO_ADMITTED_TRAINER_CREATE_FAILED");
        }
    }

    private static OperationResult<PreparedDpoModels> PrepareInitialSeededModels(
        Func<LlmModel> createPolicyModel,
        Func<LlmModel> createReferenceModel,
        Func<LlmModel> createFreshLoadModel,
        DpoTrainingReproducibilityContext context,
        TranslationRepositoryOwnedModelCheckpointContext
            repositoryOwnedModelCheckpointContext)
    {
        try
        {
            var policy = createPolicyModel();
            var reference = createReferenceModel();
            var freshLoad = createFreshLoadModel();
            // Initial-run обязан подготовить все три роли вместе: policy
            // пойдёт в DpoTrainer, reference останется неизменяемым сравнением,
            // fresh-load позже примет checkpoint только после такой же LoRA-формы.
            var evidence = DpoTrainingSeededLoraTopologyPreparer
                .ApplyFreshSeededTriple(policy, reference, freshLoad, context);
            var repositoryStart =
                LoadRepositoryOwnedPolicyCheckpointAfterSeededTopology(
                    policy,
                    repositoryOwnedModelCheckpointContext);
            if (!repositoryStart.IsSuccess)
            {
                return repositoryStart.MapError<PreparedDpoModels>();
            }

            return OperationResult<PreparedDpoModels>.Ok(
                new PreparedDpoModels(
                    policy,
                    reference,
                    evidence,
                    repositoryStart.Value));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<PreparedDpoModels>.Fail(
                $"Translation DPO seeded LoRA topology could not be prepared before training: {exception.Message}",
                SeededLoraTopologyPreparationFailedErrorCode);
        }
    }

    private static OperationResult<PreparedDpoModels>
        PrepareContinuationSeededModels(
            Func<LlmModel> createPolicyModel,
            Func<LlmModel> createReferenceModel,
            string loadedCheckpointPath,
            DpoTrainingReproducibilityContext context,
            DpoTrainingSeededLoraTopologyApplicationEvidence sourceEvidence)
    {
        try
        {
            var freshPolicy = createPolicyModel();
            var reference = createReferenceModel();
            // Сначала доказываем, что fresh-load уже имеет серверную LoRA-форму
            // и правильный fresh-load domain; только затем checkpoint может
            // записать в эти слоты policy-веса из предыдущего запуска.
            _ = DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole(
                freshPolicy,
                context,
                DpoTrainingSeededLoraModelRole.FreshLoad);
            _ = DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole(
                reference,
                context,
                DpoTrainingSeededLoraModelRole.Reference);
            freshPolicy.LoadCheckpoint(loadedCheckpointPath);
            return OperationResult<PreparedDpoModels>.Ok(
                new PreparedDpoModels(
                    freshPolicy,
                    reference,
                    sourceEvidence,
                    RepositoryOwnedModelCheckpointStart: null));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<PreparedDpoModels>.Fail(
                $"Translation DPO continuation could not validate seeded fresh-load/reference topology before loading the policy checkpoint: {exception.Message}",
                SeededLoraTopologyPreparationFailedErrorCode);
        }
    }

    private static string BuildTrainingRunId(
        TranslationDpoTrainingHandoff handoff,
        string runPurpose,
        int mutationStep)
    {
        var identity = string.Join(
            "\n",
            "translation-dpo-training-run-v1",
            runPurpose,
            mutationStep.ToString(System.Globalization.CultureInfo.InvariantCulture),
            handoff.HandoffId,
            handoff.SourceBatchId,
            handoff.DomainTag,
            handoff.MachineId);
        return string.Concat(
            "translation-",
            runPurpose,
            "-",
            ComputeStringSha256(identity));
    }

    private sealed class AdmittedTrainerScope(
        DpoTrainer trainer,
        ServerOwnedLegalTrainingTeacherAdmissionLease lease,
        TranslationDpoMutationBatchSnapshot mutationSnapshot,
        DpoTrainingSeededLoraTopologyApplicationEvidence seededLoraTopologyEvidence,
        TranslationRepositoryOwnedModelCheckpointStart?
            repositoryOwnedModelCheckpointStart,
        string checkpointDirectory,
        LlmModel policyModel) : IDisposable
    {
        public DpoTrainer Trainer { get; } = trainer;

        public LlmModel PolicyModel { get; } = policyModel;

        public TranslationDpoMutationBatchSnapshot MutationSnapshot { get; } =
            mutationSnapshot;

        public DpoTrainingSeededLoraTopologyApplicationEvidence
            SeededLoraTopologyEvidence { get; } =
                seededLoraTopologyEvidence;

        public TranslationRepositoryOwnedModelCheckpointStart?
            RepositoryOwnedModelCheckpointStart { get; } =
                repositoryOwnedModelCheckpointStart;

        public string CheckpointDirectory { get; } = checkpointDirectory;

        public void Dispose() => lease.Dispose();
    }

    private sealed record PreparedDpoModels(
        LlmModel PolicyModel,
        LlmModel ReferenceModel,
        DpoTrainingSeededLoraTopologyApplicationEvidence
            SeededLoraTopologyEvidence,
        TranslationRepositoryOwnedModelCheckpointStart?
            RepositoryOwnedModelCheckpointStart);

    private sealed class HeldVerifiedRepositoryFile(
        string path,
        string sha256,
        long byteLength,
        FileStream stream) : IDisposable
    {
        public string Path { get; } = path;

        public string Sha256 { get; } = sha256;

        public long ByteLength { get; } = byteLength;

        public FileStream Stream { get; } = stream;

        public void Rewind() => Stream.Position = 0;

        public void Dispose() => Stream.Dispose();
    }

    private sealed record TranslationRepositoryOwnedModelCheckpointStart(
        RepositoryOwnedModelCheckpointManifest? ParentManifest,
        string? ColdStartReason,
        IReadOnlyList<string> LoaderDiagnostics,
        RepositoryOwnedModelCheckpointAddress NextStepAddress);

    private sealed record TranslationRepositoryOwnedModelCheckpointPendingPublication(
        TranslationRepositoryOwnedModelCheckpointRequest Request,
        TranslationRepositoryOwnedModelCheckpointStart Start,
        RepositoryOwnedModelCheckpointAddress Address,
        RepositoryOwnedModelCheckpointManifestRequest ManifestRequest,
        string PendingJournalPath,
        string TrainerPolicyPath,
        string TrainerMetricsPath,
        int TrainerStep);

    private sealed record TranslationRepositoryOwnedModelCheckpointRecoveryProbe(
        bool Recovered,
        TranslationRepositoryOwnedModelCheckpointRunEvidence? Evidence,
        TranslationDpoTrainerBridgeResult? TrainResult)
    {
        public static TranslationRepositoryOwnedModelCheckpointRecoveryProbe
            NotRecovered { get; } = new(
                Recovered: false,
                Evidence: null,
                TrainResult: null);
    }

    private sealed record TranslationRepositoryOwnedModelCheckpointPendingPublicationJournal(
        string Schema,
        string Runner,
        string RepositoryRelativeCheckpointDirectory,
        string RepositoryRelativeWeightsPath,
        string RepositoryRelativeManifestPath,
        string DomainSlug,
        string DesignPatternSlug,
        string ModelIdSlug,
        string ModelVersion,
        int Step,
        string? ParentManifestRelativePath,
        int? ParentStep,
        string? ParentWeightsSha256,
        long? ParentWeightsByteLength,
        string? ColdStartReason,
        RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifest,
        RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
        int TrainerStep,
        string TrainerPolicyFileName,
        string TrainerMetricsFileName,
        string? TrainerPolicySha256,
        long? TrainerPolicyByteLength,
        string? TrainerMetricsSha256,
        long? TrainerMetricsByteLength,
        DateTimeOffset CreatedAtUtc,
        string[] ProvenanceRefs);

    private static OperationResult<NormalizedRunnerPaths> ValidateAndNormalizePaths(
        string seedCorpusPath,
        TranslationDpoTrainEvaluateRunnerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(seedCorpusPath))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner requires a seed corpus path.",
                "SEED_CORPUS_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.OutputPath))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner requires an explicit JSON output path.",
                "OUTPUT_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.CheckpointDirectory))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner requires the DpoTrainer checkpoint directory in options.",
                "CHECKPOINT_DIRECTORY_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.TrainingDataArtifactRoot))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner requires a training-data artifact root.",
                "TRAINING_DATA_ARTIFACT_ROOT_REQUIRED");

        if (!Path.IsPathFullyQualified(options.OutputPath))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner output path must be absolute.",
                "OUTPUT_PATH_MUST_BE_ABSOLUTE");

        if (!Path.IsPathFullyQualified(options.CheckpointDirectory))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO trainer checkpoint directory must be absolute.",
                "CHECKPOINT_DIRECTORY_MUST_BE_ABSOLUTE");

        if (!Path.IsPathFullyQualified(options.TrainingDataArtifactRoot))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO training-data artifact root must be absolute.",
                "TRAINING_DATA_ARTIFACT_ROOT_MUST_BE_ABSOLUTE");

        if (options.TrainStep <= 0 || options.HeldOutEvaluationStep <= 0)
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO train and held-out evaluation steps must be positive.",
                "DPO_STEP_MUST_BE_POSITIVE");

        if (options.TrainStep == options.HeldOutEvaluationStep)
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO held-out evaluation step must differ from the train step so metric/checkpoint artifacts stay unambiguous.",
                "DPO_EVALUATION_STEP_MUST_DIFFER");

        var seedFullPath = Path.GetFullPath(seedCorpusPath);
        var outputFullPath = Path.GetFullPath(options.OutputPath);
        var checkpointDirectory = Path.GetFullPath(options.CheckpointDirectory);
        var trainingDataArtifactRoot = Path.GetFullPath(options.TrainingDataArtifactRoot);

        if (!File.Exists(seedFullPath))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation seed corpus file not found.",
                "SEED_CORPUS_NOT_FOUND");

        if (File.Exists(trainingDataArtifactRoot))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO training-data artifact root must be a directory.",
                "TRAINING_DATA_ARTIFACT_ROOT_IS_FILE");

        if (Directory.Exists(outputFullPath))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner output path must be a JSON file path, not a directory.",
                "OUTPUT_PATH_IS_DIRECTORY");

        if (!string.Equals(Path.GetExtension(outputFullPath), ".json", StringComparison.OrdinalIgnoreCase))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner output path must end with .json.",
                "OUTPUT_PATH_MUST_BE_JSON");

        if (string.Equals(seedFullPath, outputFullPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner output path must not overwrite the seed corpus.",
                "OUTPUT_PATH_OVERWRITES_SOURCE");

        var repositoryRoots = FindProtectedRepositoryRoots(
            seedFullPath,
            outputFullPath,
            checkpointDirectory);
        if (!options.AllowRepositoryOutput &&
            repositoryRoots.Any(repositoryRoot =>
                IsInsideDirectory(outputFullPath, repositoryRoot) ||
                IsInsideDirectory(checkpointDirectory, repositoryRoot)))
            return OperationResult<NormalizedRunnerPaths>.Fail(
                "Translation DPO runner refuses repository artifact output by default; pass an external temp/output path or explicitly allow repository output.",
                "REPOSITORY_OUTPUT_BLOCKED");

        return OperationResult<NormalizedRunnerPaths>.Ok(
            new NormalizedRunnerPaths(
                seedFullPath,
                outputFullPath,
                checkpointDirectory,
                trainingDataArtifactRoot));
    }

    private static OperationResult<NormalizedLifecyclePaths> ValidateAndNormalizeLifecyclePaths(
        string seedCorpusPath,
        TranslationDpoCheckpointLifecycleRunnerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(seedCorpusPath))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle runner requires a seed corpus path.",
                "SEED_CORPUS_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.OutputPath))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle runner requires an explicit JSON output path.",
                "OUTPUT_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(options.ContinuationCheckpointDirectory))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle runner requires a continuation checkpoint directory.",
                "CONTINUATION_CHECKPOINT_DIRECTORY_REQUIRED");

        if (!Path.IsPathFullyQualified(options.OutputPath))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle output path must be absolute.",
                "OUTPUT_PATH_MUST_BE_ABSOLUTE");

        if (!Path.IsPathFullyQualified(options.ContinuationCheckpointDirectory))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO continuation checkpoint directory must be absolute.",
                "CONTINUATION_CHECKPOINT_DIRECTORY_MUST_BE_ABSOLUTE");

        if (options.FreshLoadEvaluationStep <= 0 ||
            options.ContinueTrainStep <= 0 ||
            options.PostContinueEvaluationStep <= 0)
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO lifecycle steps must be positive.",
                "DPO_LIFECYCLE_STEP_MUST_BE_POSITIVE");

        if (new[]
            {
                options.FreshLoadEvaluationStep,
                options.ContinueTrainStep,
                options.PostContinueEvaluationStep
            }.Distinct().Count() != 3)
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO lifecycle evaluation and continue-training steps must differ so artifacts stay unambiguous.",
                "DPO_LIFECYCLE_STEPS_MUST_DIFFER");

        if (options.LearningRate <= 0f || float.IsNaN(options.LearningRate) || float.IsInfinity(options.LearningRate))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO lifecycle learning rate must be a positive finite value.",
                "DPO_LIFECYCLE_LEARNING_RATE_INVALID");

        var seedFullPath = Path.GetFullPath(seedCorpusPath);
        var outputFullPath = Path.GetFullPath(options.OutputPath);
        var checkpointDirectory = Path.GetFullPath(options.ContinuationCheckpointDirectory);

        if (!File.Exists(seedFullPath))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation seed corpus file not found.",
                "SEED_CORPUS_NOT_FOUND");

        if (Directory.Exists(outputFullPath))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle output path must be a JSON file path, not a directory.",
                "OUTPUT_PATH_IS_DIRECTORY");

        if (!string.Equals(Path.GetExtension(outputFullPath), ".json", StringComparison.OrdinalIgnoreCase))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle output path must end with .json.",
                "OUTPUT_PATH_MUST_BE_JSON");

        if (string.Equals(seedFullPath, outputFullPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle output path must not overwrite the seed corpus.",
                "OUTPUT_PATH_OVERWRITES_SOURCE");

        var repositoryRoots = FindProtectedRepositoryRoots(
            seedFullPath,
            outputFullPath,
            checkpointDirectory);
        if (!options.AllowRepositoryOutput &&
            repositoryRoots.Any(repositoryRoot =>
                IsInsideDirectory(outputFullPath, repositoryRoot) ||
                IsInsideDirectory(checkpointDirectory, repositoryRoot)))
            return OperationResult<NormalizedLifecyclePaths>.Fail(
                "Translation DPO checkpoint lifecycle runner refuses repository artifact output by default; pass an external temp/output path or explicitly allow repository output.",
                "REPOSITORY_OUTPUT_BLOCKED");

        return OperationResult<NormalizedLifecyclePaths>.Ok(
            new NormalizedLifecyclePaths(seedFullPath, outputFullPath, checkpointDirectory));
    }

    private static OperationResult<string> ValidateSourceReportForLifecycle(
        TranslationDpoTrainEvaluateRunReport sourceReport,
        string seedFullPath)
    {
        if (string.IsNullOrWhiteSpace(sourceReport.SourceCorpusPath) ||
            !string.Equals(
                Path.GetFullPath(sourceReport.SourceCorpusPath),
                Path.GetFullPath(seedFullPath),
                StringComparison.OrdinalIgnoreCase))
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle source report must belong to the same seed corpus path.",
                "SOURCE_REPORT_CORPUS_MISMATCH");

        if (!sourceReport.CheckpointWritten || string.IsNullOrWhiteSpace(sourceReport.CheckpointPath))
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle source report must contain a written policy checkpoint path.",
                "SOURCE_CHECKPOINT_REQUIRED");

        if (sourceReport.SemanticQualityClaimed || sourceReport.SemanticQualityReadinessClaimed)
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle refuses source reports that claim semantic-quality readiness.",
                "SOURCE_REPORT_SEMANTIC_CLAIMED");

        if (sourceReport.HeldOutRowsPassedToStepMany || sourceReport.TrainRowsUsedForHeldOutEvaluation)
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle refuses source reports with train/held-out leakage flags.",
                "SOURCE_REPORT_SPLIT_LEAKAGE");

        if (string.IsNullOrWhiteSpace(sourceReport.HandoffId) ||
            string.IsNullOrWhiteSpace(sourceReport.SourceBatchId))
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle source report must carry handoff and source batch identifiers.",
                "SOURCE_REPORT_HANDOFF_REQUIRED");

        var checkpointPath = Path.GetFullPath(sourceReport.CheckpointPath);
        if (!File.Exists(checkpointPath))
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle source checkpoint file was not found.",
                "SOURCE_CHECKPOINT_NOT_FOUND");

        if (!string.Equals(
                ComputeSha256(checkpointPath),
                sourceReport.PolicyCheckpointSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "Translation DPO checkpoint lifecycle source checkpoint SHA-256 does not match the source report.",
                PolicyCheckpointSha256MismatchErrorCode);
        }

        return OperationResult<string>.Ok(checkpointPath);
    }

    private static OperationResult<bool> ValidateCorpusBoundaries(TranslationSeedCorpus corpus)
    {
        if (corpus.Rows.Count == 0)
            return OperationResult<bool>.Fail(
                "Translation DPO runner requires at least one seed row.",
                "SEED_CORPUS_EMPTY");

        if (corpus.TrainRows.Count == 0 || corpus.HeldOutRows.Count == 0)
            return OperationResult<bool>.Fail(
                "Translation DPO runner requires both train and held-out rows.",
                "SEED_CORPUS_SPLIT_REQUIRED");

        foreach (var row in corpus.Rows)
        {
            if (row.SemanticQualityClaimed)
                return OperationResult<bool>.Fail(
                    $"Translation DPO runner refuses semantic-quality-claimed seed row '{row.SeedId}'.",
                    "SEMANTIC_QUALITY_CLAIMED");

            if (!string.Equals(row.SourceCategory, SelfAuthoredCategory, StringComparison.Ordinal) ||
                !string.Equals(row.License, InternalLicense, StringComparison.Ordinal))
                return OperationResult<bool>.Fail(
                    $"Translation DPO runner refuses illegal source/license on seed row '{row.SeedId}'.",
                    "ILLEGAL_SOURCE_OR_LICENSE");

            var provenance = row.TeacherProvenance;
            if (!string.Equals(provenance.SourceType, SelfAuthoredCategory, StringComparison.Ordinal) ||
                !string.Equals(provenance.ModelProvider, NoTeacherValue, StringComparison.Ordinal) ||
                !string.Equals(provenance.ModelName, NoTeacherValue, StringComparison.Ordinal) ||
                provenance.ExternalApiUsed ||
                provenance.ClosedProprietaryTeacherUsed)
                return OperationResult<bool>.Fail(
                    $"Translation DPO runner refuses closed/external teacher provenance on seed row '{row.SeedId}'.",
                    "ILLEGAL_TEACHER_PROVENANCE");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<TranslationTrainingDataArtifactLoadResult> LoadValidatedTrainingDataArtifact(
        string trainingDataArtifactRoot)
    {
        try
        {
            return TranslationTrainingDataArtifactStore
                .LoadValidatedTrainReadyPairsAsync(trainingDataArtifactRoot)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            return OperationResult<TranslationTrainingDataArtifactLoadResult>.Fail(
                $"Translation DPO runner failed to load the training-data artifact: {ex.Message}",
                "TRAINING_DATA_ARTIFACT_LOAD_FAILED");
        }
    }

    private static OperationResult<bool> ValidateTrainingDataArtifactMatchesCorpus(
        TranslationSeedCorpus corpus,
        TranslationTrainingDataArtifactLoadResult artifact)
    {
        var expectedPairs = corpus.ToTrainReadyPairs(TranslationSeedCorpusSplit.Train)
            .OrderBy(pair => pair.Id, StringComparer.Ordinal)
            .ToArray();
        var artifactPairs = artifact.TrainingPairs
            .OrderBy(pair => pair.Id, StringComparer.Ordinal)
            .ToArray();

        if (artifact.Manifest.TrainingPairsCount != expectedPairs.Length ||
            artifact.CorpusRowsCount != expectedPairs.Length ||
            artifactPairs.Length != expectedPairs.Length)
            return OperationResult<bool>.Fail(
                "Translation DPO runner requires the training-data artifact row count to match the seed train split.",
                "TRAINING_DATA_ARTIFACT_TRAIN_COUNT_MISMATCH");

        var expectedIds = expectedPairs.Select(pair => pair.Id).ToHashSet(StringComparer.Ordinal);
        var artifactIds = artifactPairs.Select(pair => pair.Id).ToHashSet(StringComparer.Ordinal);
        if (!expectedIds.SetEquals(artifactIds))
            return OperationResult<bool>.Fail(
                "Translation DPO runner requires training-data artifact pair ids to match the seed train split exactly.",
                "TRAINING_DATA_ARTIFACT_TRAIN_ID_MISMATCH");

        var expectedById = expectedPairs.ToDictionary(pair => pair.Id, StringComparer.Ordinal);
        foreach (var pair in artifactPairs)
        {
            var expected = expectedById[pair.Id];
            if (!SameTrainReadyPair(expected, pair))
                return OperationResult<bool>.Fail(
                    $"Translation DPO runner detected training-data artifact content mismatch for pair '{pair.Id}'.",
                    "TRAINING_DATA_ARTIFACT_CONTENT_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateSplitIsolation(
        TranslationDpoTrainerBridgeResult train,
        TranslationDpoHeldOutEvaluationResult heldOut)
    {
        if (train.TrainedPairIds.Intersect(heldOut.HeldOutEvaluationPairIds, StringComparer.Ordinal).Any())
            return OperationResult<bool>.Fail(
                "Translation DPO runner detected a held-out row in the StepMany training pair set.",
                "HELDOUT_LEAKAGE");

        if (heldOut.HeldOutEvaluationPairIds.Intersect(heldOut.ExcludedTrainPairIds, StringComparer.Ordinal).Any())
            return OperationResult<bool>.Fail(
                "Translation DPO runner detected a train row in the EvaluateMany held-out pair set.",
                "TRAIN_EVALUATION_LEAKAGE");

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> WriteReport<TReport>(
        TReport report,
        string outputPath)
    {
        try
        {
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var json = JsonSerializer.Serialize(report, JsonOptions) + "\n";
            var tempPath = outputPath + ".tmp";
            File.WriteAllText(tempPath, json, Utf8NoBom);
            File.Move(tempPath, outputPath, overwrite: true);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(
                $"Translation DPO runner failed to persist report: {ex.Message}",
                "REPORT_WRITE_FAILED");
        }
    }

    private static TranslationDpoTrainingDataArtifactEvidence ToArtifactEvidence(
        TranslationTrainingDataArtifactLoadResult artifact)
        => new(
            ArtifactRoot: artifact.ArtifactRoot,
            ManifestPath: artifact.ManifestPath,
            ManifestRepoPath: artifact.ManifestRepoPath,
            CorpusPath: artifact.CorpusPath,
            CorpusSha256: artifact.CorpusSha256,
            CorpusRowsCount: artifact.CorpusRowsCount,
            ManifestTrainingPairsCount: artifact.Manifest.TrainingPairsCount,
            LegalCommonDpoManifestValidated: TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(artifact.Manifest),
            CommonTrainReady: artifact.Manifest.CommonTrainReady,
            DpoTrainReady: artifact.Manifest.DpoTrainReady,
            SourceCategory: artifact.Manifest.SourceCategory,
            DataClass: artifact.Manifest.DataClass,
            StorageScope: artifact.Manifest.StorageScope,
            PrivacyScope: artifact.Manifest.PrivacyScope,
            ProvenanceRef: artifact.Manifest.ProvenanceRef,
            SplitPolicy: artifact.Manifest.SplitPolicy,
            SourceDisjointGroup: artifact.Manifest.SourceDisjointGroup,
            LicenseOrProjectInternalRef: artifact.Manifest.LicenseOrProjectInternalRef,
            LegalVerdict: artifact.Manifest.LegalVerdict);

    private static TranslationDpoTrainingDataArtifactEvidence
        ToRepositoryOwnedTrainingDataEvidence(
            TranslationRepositoryOwnedDpoDatasetContext context,
            RepositoryOwnedDpoDatasetResolution resolution,
            NormalizedRunnerPaths paths)
    {
        var manifest = resolution.Dataset.Manifest;
        return new TranslationDpoTrainingDataArtifactEvidence(
            ArtifactRoot: context.Address!.DatasetRoot,
            ManifestPath: context.Address.ManifestPath,
            ManifestRepoPath: context.Address.ManifestRelativePath,
            CorpusPath: paths.SeedCorpusPath,
            CorpusSha256: ComputeSha256(paths.SeedCorpusPath),
            CorpusRowsCount: manifest.PairCount,
            ManifestTrainingPairsCount: manifest.PairCount,
            LegalCommonDpoManifestValidated: true,
            CommonTrainReady: true,
            DpoTrainReady: true,
            SourceCategory: "repository_owned_common_dpo",
            DataClass: "dpo_pair",
            StorageScope: "repository_owned_training_data_dpo",
            PrivacyScope: "project_internal",
            ProvenanceRef: "repository-owned-dpo:" +
                           context.Address.ManifestRelativePath,
            SplitPolicy:
                "training_pairs_reopened_from_repository_owned_dpo; held_out_requires_explicit_seed_source",
            SourceDisjointGroup: context.Address.DomainSlug,
            LicenseOrProjectInternalRef: "repository-owned-request:" +
                                         context.RequestIdentity!
                                             .RequestIdentitySha256,
            LegalVerdict: "ACCEPT");
    }

    private static TranslationRepositoryOwnedDpoDatasetRunEvidence
        ToRepositoryOwnedDpoDatasetEvidence(
            TranslationRepositoryOwnedDpoDatasetContext context,
            RepositoryOwnedDpoDatasetResolution resolution,
            string heldOutSeedCorpusPath,
            bool legacyTrainingDataArtifactLoaded)
        => new(
            Enabled: true,
            ResolutionSource: resolution.Source.ToString(),
            ManifestPath: context.Address!.ManifestPath,
            ManifestRelativePath: context.Address.ManifestRelativePath,
            ManifestSha256: resolution.Dataset.Manifest.ManifestSha256,
            RequestIdentitySha256: context.RequestIdentity!
                .RequestIdentitySha256,
            PairCount: resolution.Dataset.Pairs.Count,
            TrainingPairsReopenedFromDisk: true,
            LegacyTrainingDataArtifactLoaded: legacyTrainingDataArtifactLoaded,
            HeldOutEvaluationSource: heldOutSeedCorpusPath,
            Boundary:
                "Common DpoPair rows are authoritative for training pairs only; held-out evaluation still requires the explicit seed corpus because common DpoPair does not carry expected held-out labels.");

    private static TranslationDpoMetricsSnapshot ToSnapshot(DpoMetrics metrics)
        => new(
            Step: metrics.Step,
            Loss: metrics.Loss,
            ChosenReward: metrics.ChosenReward,
            RejectedReward: metrics.RejectedReward,
            RewardMargin: metrics.RewardMargin,
            Accuracy: metrics.Accuracy);

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ComputeStringSha256(string value)
        => Convert.ToHexString(SHA256.HashData(Utf8NoBom.GetBytes(value)))
            .ToLowerInvariant();

    private static bool SameTrainReadyPair(
        TranslationTrainReadyPair expected,
        TranslationTrainReadyPair actual)
        => string.Equals(expected.Id, actual.Id, StringComparison.Ordinal) &&
           string.Equals(expected.Source, actual.Source, StringComparison.Ordinal) &&
           string.Equals(expected.Direction, actual.Direction, StringComparison.Ordinal) &&
           string.Equals(expected.PairFamily, actual.PairFamily, StringComparison.Ordinal) &&
           string.Equals(expected.Prompt, actual.Prompt, StringComparison.Ordinal) &&
           string.Equals(expected.ASide, actual.ASide, StringComparison.Ordinal) &&
           string.Equals(expected.Chosen, actual.Chosen, StringComparison.Ordinal) &&
           string.Equals(expected.Rejected, actual.Rejected, StringComparison.Ordinal) &&
           string.Equals(expected.ValidationOutcome, actual.ValidationOutcome, StringComparison.Ordinal) &&
           SameSequence(expected.ValidationErrors, actual.ValidationErrors);

    private static bool SameSequence(
        IReadOnlyList<string>? expected,
        IReadOnlyList<string>? actual)
        => (expected ?? Array.Empty<string>()).SequenceEqual(
            actual ?? Array.Empty<string>(),
            StringComparer.Ordinal);

    private static IReadOnlyList<string> FindProtectedRepositoryRoots(
        string seedFullPath,
        string outputFullPath,
        string checkpointDirectory)
    {
        var roots = new List<string>();
        foreach (var startPath in EnumerateRepositoryRootSearchStarts(
                     seedFullPath,
                     outputFullPath,
                     checkpointDirectory))
        {
            var root = FindRepositoryRoot(startPath);
            if (root is null || roots.Contains(root, StringComparer.OrdinalIgnoreCase))
                continue;

            roots.Add(root);
        }

        return roots;
    }

    private static IEnumerable<string> EnumerateRepositoryRootSearchStarts(
        string seedFullPath,
        string outputFullPath,
        string checkpointDirectory)
    {
        yield return Environment.CurrentDirectory;

        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
            yield return AppContext.BaseDirectory;

        var assemblyLocation = typeof(TranslationDpoTrainEvaluateRunner).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(assemblyLocation))
            yield return assemblyLocation;

        yield return seedFullPath;
        yield return outputFullPath;
        yield return checkpointDirectory;
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var directory = File.Exists(startPath)
            ? Directory.GetParent(Path.GetFullPath(startPath))
            : new DirectoryInfo(Path.GetFullPath(startPath));

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")) &&
                Directory.Exists(Path.Combine(directory.FullName, "27.TranslationDomain")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private static bool IsInsideDirectory(string path, string directory)
    {
        var directoryPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var candidatePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        return string.Equals(candidatePath, directoryPath, StringComparison.OrdinalIgnoreCase) ||
            candidatePath.StartsWith(EnsureTrailingSeparator(directoryPath), StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private sealed record NormalizedRunnerPaths(
        string SeedCorpusPath,
        string OutputPath,
        string CheckpointDirectory,
        string TrainingDataArtifactRoot);

    private sealed record NormalizedLifecyclePaths(
        string SeedCorpusPath,
        string OutputPath,
        string ContinuationCheckpointDirectory);
}
