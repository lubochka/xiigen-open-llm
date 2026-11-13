// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Diagnostics.CodeAnalysis;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using Tokenizer;
using TrainingLoop;
using TransformerModel;
using TransformerModel.Config;

/// <summary>
/// Immutable startup configuration for the ContinuousLearning TP1 teacher.
/// The teacher family, access mode, provider, and license are not configurable:
/// they are fixed to local Qwen through Ollama under Apache-2.0.
/// </summary>
public sealed class ContinuousLearningTp1DpoConfiguration
{
    private const int AuditSaltBytes = 32;
    private readonly byte[] _auditHashSalt;

    private ContinuousLearningTp1DpoConfiguration(
        string modelId,
        string expectedOllamaManifestDigest,
        string expectedOllamaModelLayerDigest,
        int maxTeacherCalls,
        string localRetentionPolicyRef,
        string outputLicenseRef,
        byte[] auditHashSalt,
        int dpoMaxStepCount, // Keep optimizer limits independent of inference selection.
        LocalQwenOllamaInferenceProfileDescriptor authoringProfile,
        LocalQwenOllamaInferenceProfileDescriptor evaluationProfile)
    {
        ModelId = modelId;
        ExpectedOllamaManifestDigest = expectedOllamaManifestDigest;
        ExpectedOllamaModelLayerDigest = expectedOllamaModelLayerDigest;
        MaxTeacherCalls = maxTeacherCalls;
        LocalRetentionPolicyRef = localRetentionPolicyRef;
        OutputLicenseRef = outputLicenseRef;
        DpoMaxStepCount = dpoMaxStepCount;
        AuthoringDescriptor = authoringProfile; // No later environment or caller option can mutate the selected HTTP contract.
        EvaluationDescriptor = evaluationProfile;
        _auditHashSalt = auditHashSalt.ToArray();
    }

    public string ModelId { get; }
    /// <summary>
    /// Expected digest reported for the exact model tag by Ollama
    /// <c>/api/tags</c>. This pins the Ollama manifest identity; it is not a
    /// claim that this process rehashed every model blob or the model weights.
    /// </summary>
    public string ExpectedOllamaManifestDigest { get; }
    /// <summary>
    /// Expected layer digest carried by the single active <c>FROM</c> locator
    /// returned by Ollama <c>/api/show</c>. This is an identity pin, not a
    /// claim that this process rehashed the complete layer blob.
    /// </summary>
    public string ExpectedOllamaModelLayerDigest { get; }
    public int MaxTeacherCalls { get; }
    public string LocalRetentionPolicyRef { get; }
    public string OutputLicenseRef { get; }
    public int DpoMaxStepCount { get; }
    internal LocalQwenOllamaInferenceProfileDescriptor AuthoringDescriptor { get; } // The client reads the same immutable profile as documentation authoring.
    public TrainingTeacherInferenceProfileEvidence AuthoringProfile => AuthoringDescriptor.Evidence; // Consumers seal the selected ID/hash rather than assuming the legacy passport.
    internal LocalQwenOllamaInferenceProfileDescriptor EvaluationDescriptor { get; }
    public TrainingTeacherInferenceProfileEvidence EvaluationProfile => EvaluationDescriptor.Evidence;
    internal LocalQwenOllamaInferenceProfileDescriptor PreparedAuthoringDescriptor => AuthoringDescriptor.BindCpuAuthoringParameters // Preserve the separate legacy preparation contract unless CPU authoring was explicitly selected.
        ? AuthoringDescriptor : LocalQwenOllamaInferenceProfileAuthority.TokenBoundAuthoringDescriptor; // The CPU preparation and effect use one profile; old preparation retains its identity.

    public static OperationResult<ContinuousLearningTp1DpoConfiguration> Create(
        string modelId,
        string expectedOllamaManifestDigest,
        int maxTeacherCalls,
        ReadOnlySpan<byte> serverAuditHashSalt)
        => OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
            "ContinuousLearning TP1 requires explicit server-owned legal evidence references.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    public static OperationResult<ContinuousLearningTp1DpoConfiguration> Create(
        string modelId,
        string expectedOllamaManifestDigest,
        int maxTeacherCalls,
        string localRetentionPolicyRef,
        string outputLicenseRef,
        ReadOnlySpan<byte> serverAuditHashSalt,
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount)
        => OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
            "ContinuousLearning TP1 requires an explicit Ollama model-layer digest.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    public static OperationResult<ContinuousLearningTp1DpoConfiguration> Create(
        string modelId,
        string expectedOllamaManifestDigest,
        string expectedOllamaModelLayerDigest,
        int maxTeacherCalls,
        string localRetentionPolicyRef,
        string outputLicenseRef,
        ReadOnlySpan<byte> serverAuditHashSalt,
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount, // Preserve existing source calls and the default optimizer budget.
        TrainingTeacherInferenceProfileEvidence? authoringProfile = null,
        TrainingTeacherInferenceProfileEvidence? evaluationProfile = null)
    {
        var selectedProfile = LocalQwenOllamaInferenceProfileAuthority.ResolveAuthoringDescriptor(authoringProfile); // Reject unknown or mismatched evidence before constructing any client.
        if (!selectedProfile.IsSuccess || selectedProfile.Value is null) return selectedProfile.MapError<ContinuousLearningTp1DpoConfiguration>(); // Never downgrade a forged selection to legacy behavior.
        var selectedEvaluation = LocalQwenOllamaInferenceProfileAuthority.ResolveEvaluationDescriptor(evaluationProfile);
        if (!selectedEvaluation.IsSuccess || selectedEvaluation.Value is null) return selectedEvaluation.MapError<ContinuousLearningTp1DpoConfiguration>();
        if (serverAuditHashSalt.Length != AuditSaltBytes)
        {
            return OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
                "ContinuousLearning TP1 requires an exact 32-byte server audit salt.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }

        if (!IsCanonicalOllamaModelId(modelId)
            || !IsCanonicalOllamaManifestDigest(expectedOllamaManifestDigest)
            || !IsCanonicalOllamaModelLayerDigest(expectedOllamaModelLayerDigest))
        {
            return OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
                "ContinuousLearning TP1 requires a canonical local model identity.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }

        if (!LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(
                localRetentionPolicyRef)
            || !LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(
                outputLicenseRef))
        {
            return OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
                "ContinuousLearning TP1 requires canonical server-owned legal evidence references.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }

        var saltSnapshot = serverAuditHashSalt.ToArray();
        var authority = CreateTeacherPolicyBudgetAuthority(
            modelId,
            expectedOllamaManifestDigest,
            expectedOllamaModelLayerDigest,
            maxTeacherCalls,
            localRetentionPolicyRef,
            outputLicenseRef,
            saltSnapshot);
        if (!authority.IsSuccess)
            return authority.MapError<ContinuousLearningTp1DpoConfiguration>();
        var validatedDpoStepCount =
            StableDpoTrainingExecutor.ValidateDpoStepCount(dpoMaxStepCount);
        if (!validatedDpoStepCount.IsSuccess)
            return validatedDpoStepCount.MapError<ContinuousLearningTp1DpoConfiguration>();

        return OperationResult<ContinuousLearningTp1DpoConfiguration>.Ok(
            new ContinuousLearningTp1DpoConfiguration(
                modelId,
                expectedOllamaManifestDigest,
                expectedOllamaModelLayerDigest,
                maxTeacherCalls,
                localRetentionPolicyRef,
                outputLicenseRef,
                saltSnapshot,
                validatedDpoStepCount.Value, // Keep DPO configuration unchanged by an inference budget choice.
                selectedProfile.Value,
                selectedEvaluation.Value));
    }

    internal OperationResult<LegalTrainingTeacherAdmissionAuthority>
        CreateTeacherPolicyBudgetAuthority()
        => CreateTeacherPolicyBudgetAuthority(
            ModelId,
            ExpectedOllamaManifestDigest,
            ExpectedOllamaModelLayerDigest,
            MaxTeacherCalls,
            LocalRetentionPolicyRef,
            OutputLicenseRef,
            _auditHashSalt);

    private static OperationResult<LegalTrainingTeacherAdmissionAuthority>
        CreateTeacherPolicyBudgetAuthority(
        string modelId,
        string expectedOllamaManifestDigest,
        string expectedOllamaModelLayerDigest,
        int maxTeacherCalls,
        string localRetentionPolicyRef,
        string outputLicenseRef,
        byte[] auditHashSalt)
        => LegalTrainingTeacherAdmissionAuthority.Create(
            [new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                modelId,
                expectedOllamaManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                localRetentionPolicyRef,
                outputLicenseRef)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = expectedOllamaModelLayerDigest
            }],
            maxTeacherCalls,
            auditHashSalt);

    public static bool IsCanonicalOllamaManifestDigest(
        [NotNullWhen(true)] string? value)
        => value is { Length: 64 }
           && value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    public static bool IsCanonicalOllamaModelLayerDigest(
        [NotNullWhen(true)] string? value)
        => IsCanonicalOllamaManifestDigest(value);

    public static bool IsCanonicalOllamaModelId(
        [NotNullWhen(true)] string? value)
        => LegalTrainingTeacherAdmissionAuthority
            .IsCanonicalLocalOllamaModelIdForFamily(
                OpenWeightTeacherFamily.Qwen,
                value,
                requireTaggedModelId: true);

    /// <summary>
    /// Returns whether a legal-evidence reference uses the canonical
    /// <c>sha256:</c> prefix followed by exactly 64 lowercase hexadecimal
    /// characters.
    /// </summary>
    public static bool IsCanonicalLegalEvidenceRef(
        [NotNullWhen(true)] string? value)
        => LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(value);
}

/// <summary>
/// Real production composition boundary for the ContinuousLearning TP1 trainer.
/// Full ModelServer activation remains R1; this factory does not run a cycle.
/// </summary>
public static class ContinuousLearningProductionComposition
{
    private const string LegacyFactoryObsoleteMessage =
        "Use the server-owned legal teacher capability overload.";

    [Obsolete(LegacyFactoryObsoleteMessage, error: false)]
    public static OperationResult<IncrementalTrainer> CreateTp1Trainer(
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        ContinuousLearningTp1DpoConfiguration configuration,
        string adapterDirectory = "adapters",
        LoraConfig? loraConfig = null)
        => OperationResult<IncrementalTrainer>.Fail(
            "Server-owned legal teacher admission configuration is required.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);

    public static OperationResult<IncrementalTrainer> CreateTp1Trainer(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority,
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        ContinuousLearningTp1DpoConfiguration configuration,
        string adapterDirectory = "adapters",
        LoraConfig? loraConfig = null)
    {
        if (legalTeacherAdmissionAuthority is null
            || !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<IncrementalTrainer>.Fail(
                "Server-owned legal teacher admission configuration is required.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }

        ArgumentNullException.ThrowIfNull(configuration);

        try
        {
            return OperationResult<IncrementalTrainer>.Ok(
                new IncrementalTrainer(
                    storage,
                    sessionFactory,
                    legalTeacherAdmissionAuthority,
                    adapterDirectory,
                    loraConfig,
                    dpoMaxStepCount: configuration.DpoMaxStepCount));
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return OperationResult<IncrementalTrainer>.Fail(
                "ContinuousLearning TP1 trainer configuration is invalid.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }
    }
}

/// <summary>
/// Inactive, server-owned TP1 composition. It deliberately retains the
/// constructed trainer without exposing an execution surface; ModelServer can
/// therefore prove its wiring while training and cycle execution remain off.
/// </summary>
internal sealed class ContinuousLearningMaterializedTp1Composition : IDisposable
{
    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly IncrementalTrainer _trainer;
    private RetainedCheckpointBackedIncrementalTrainingSessionFactory?
        _retainedSessionFactory;

    private ContinuousLearningMaterializedTp1Composition(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        IncrementalTrainer trainer,
        RetainedCheckpointBackedIncrementalTrainingSessionFactory
            retainedSessionFactory)
    {
        _fileAccess = fileAccess;
        _trainer = trainer;
        _retainedSessionFactory = retainedSessionFactory;
    }

    internal static OperationResult<ContinuousLearningMaterializedTp1Composition> Create(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority,
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ModelConfig modelConfig,
        TrainingConfig trainingConfig,
        ContinuousLearningTp1DpoConfiguration teacherConfiguration,
        CancellationToken cancellationToken,
        Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
            resolveRepositoryStartRequest = null,
        Action? afterRetainedSessionFactoryBindForTesting = null,
        LoraConfig? loraConfig = null, // Accept caller-admitted LoRA settings; null keeps generic trainer defaults, while the declared root supplies zero-dropout settings.
        RepositoryOwnedFullLoraCheckpointDescriptor? initialFullCheckpoint = null) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    {
        if (legalTeacherAdmissionAuthority is null
            || !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<ContinuousLearningMaterializedTp1Composition>.Fail(
                "Server-owned legal teacher admission configuration is required.",
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
        }

        ArgumentNullException.ThrowIfNull(fileAccess);
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(trainingConfig);
        ArgumentNullException.ThrowIfNull(teacherConfiguration);

        RetainedCheckpointBackedIncrementalTrainingSessionFactory?
            retainedSessionFactory = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            fileAccess.EnsureRetained();
            var artifactValidation = ValidateBoundArtifacts(fileAccess, modelConfig, cancellationToken, initialFullCheckpoint); // Keep exact declared-artifact refusals typed before trainer/factory construction.
            if (!artifactValidation.IsSuccess) // A failed identity or schema check cannot fall back to legacy weights.
                return artifactValidation.MapError<ContinuousLearningMaterializedTp1Composition>(); // Propagate the exact artifact refusal before factory or trainer creation; the caller owns the failed-readiness transition.
            var effectiveLoraConfig = LoraApplicator.ValidateAndCanonicalize(
                loraConfig ?? IncrementalTrainer.DefaultConfig);
            cancellationToken.ThrowIfCancellationRequested();

            // The retained factory owns the RegistryRoot and AdapterRoot
            // capabilities for as long as this materialized composition lives.
            // It is retained only as an ownership boundary and is never exposed
            // to the trainer's execution path.
            var storage = new InactiveContinuousLearningSessionStorage();
            var retainedFactory =
                RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                fileAccess,
                storage,
                modelConfig,
                trainingConfig,
                resolveRepositoryStartRequest: resolveRepositoryStartRequest,
                repositoryCheckpointAdapterConfig: effectiveLoraConfig);
            if (!retainedFactory.IsSuccess || retainedFactory.Value is null)
            {
                return OperationResult<ContinuousLearningMaterializedTp1Composition>.Fail(
                    "ContinuousLearning inactive TP1 composition could not be materialized.",
                    ContinuousLearningModelServerStartup.MaterializationFailedErrorCode);
            }

            retainedSessionFactory = retainedFactory.Value;
            afterRetainedSessionFactoryBindForTesting?.Invoke();
            cancellationToken.ThrowIfCancellationRequested();

            var sessionFactory =
                new InactiveIncrementalTrainingSessionFactoryGuard();
            var trainer = ContinuousLearningProductionComposition.CreateTp1Trainer(
                legalTeacherAdmissionAuthority,
                storage,
                sessionFactory,
                teacherConfiguration,
                // This is deliberately not a filesystem path.  The private
                // factory below is the execution guard and fails before any
                // adapter path could be reached.
                adapterDirectory: "\0",
                loraConfig: effectiveLoraConfig);
            if (!trainer.IsSuccess || trainer.Value is null)
                return trainer.MapError<ContinuousLearningMaterializedTp1Composition>();

            fileAccess.EnsureRetained();
            var composition = new ContinuousLearningMaterializedTp1Composition(
                fileAccess,
                trainer.Value,
                retainedSessionFactory);
            retainedSessionFactory = null;
            return OperationResult<ContinuousLearningMaterializedTp1Composition>.Ok(
                composition);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ContinuousLearningMaterializedTp1Composition>.Fail(
                "ContinuousLearning inactive TP1 composition could not be materialized.",
                ContinuousLearningModelServerStartup.MaterializationFailedErrorCode);
        }
        finally
        {
            retainedSessionFactory?.Dispose();
        }
    }

    internal void EnsureRetained()
    {
        _ = _trainer;
        _ = Volatile.Read(ref _retainedSessionFactory)
            ?? throw new ObjectDisposedException(
                nameof(ContinuousLearningMaterializedTp1Composition));
        _fileAccess.EnsureRetained();
    }

    public void Dispose()
        => Interlocked.Exchange(ref _retainedSessionFactory, null)?.Dispose();

    private static OperationResult<bool> ValidateBoundArtifacts( // Return declared-artifact failures without throwing business exceptions.
        ServerOwnedContinuousLearningFileAccess fileAccess, // Read only through the existing retained capability owner.
        ModelConfig modelConfig, // Bind both checkpoint and tokenizer to the admitted shape.
        CancellationToken cancellationToken, // Preserve caller cancellation at the retained-read and import boundary.
        RepositoryOwnedFullLoraCheckpointDescriptor? initialFullCheckpoint) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    {
        using (var checkpoint = fileAccess.OpenRetainedArtifactForRead(
                   ContinuousLearningRetainedArtifactRole.BaseCheckpoint))
        {
            if (initialFullCheckpoint is null) // Use the unchanged exact base-only validator when no full-LoRA descriptor was supplied.
                LlmModel.ValidateFullCheckpointArtifact( // Reuse exact key, shape, payload, fingerprint and EOF checks.
                    checkpoint, modelConfig, // Supply the retained checkpoint stream and admitted model configuration to the legacy base-only validator.
                    ContinuousLearningModelServerConfigurationLoader.MaximumBaseCheckpointBytes, // Keep retained reads inside the admitted exact artifact size.
                    cancellationToken); // Preserve caller cancellation at the retained-read and import boundary.
            else // Only an explicit server-owned declaration selects full-LoRA schema admission.
            { // Keep a typed artifact refusal separate from the legacy exception-based overload.
                var declaredValidation = initialFullCheckpoint.ValidateArtifact( // Validate actual retained bytes and preserve exact codec errors.
                    checkpoint, modelConfig, // Supply the retained checkpoint stream and admitted model configuration to the declared full-LoRA validator.
                    ContinuousLearningModelServerConfigurationLoader.MaximumBaseCheckpointBytes, // Keep retained reads inside the admitted exact artifact size.
                    cancellationToken); // Preserve caller cancellation at the retained-read and import boundary.
                if (!declaredValidation.IsSuccess) return declaredValidation; // Do not construct a factory or trainer after a typed refusal.
            } // A successful declaration still grants no training execution path.
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var tokenizerStream = fileAccess.OpenRetainedArtifactForRead(
            ContinuousLearningRetainedArtifactRole.Tokenizer);
        _ = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
            tokenizerStream,
            modelConfig.VocabSize,
            cancellationToken);
        return OperationResult<bool>.Ok(true); // Both actual retained artifacts passed their unchanged exact validators.
    }
}

/// <summary>
/// A no-write session store used solely to satisfy the constructed inactive
/// trainer graph. Any future execution attempt fails rather than writing state.
/// </summary>
internal sealed class InactiveContinuousLearningSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Fail(
            "ContinuousLearning training is inactive.",
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE"));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "ContinuousLearning training is inactive.",
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}

/// <summary>
/// Execution guard for the materialized-but-inactive trainer. It is deliberately
/// independent of the retained factory so it cannot delegate into model or
/// adapter resolution, DPO, training, or artifact writes.
/// </summary>
internal sealed class InactiveIncrementalTrainingSessionFactoryGuard
    : IIncrementalTrainingSessionFactory
{
    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        _ = domain;
        return OperationResult<IncrementalTrainingSession>.Fail(
            "ContinuousLearning training is inactive.",
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE");
    }

    public OperationResult<IncrementalTrainingSession> Create(
        string domain,
        CancellationToken cancellationToken)
    {
        _ = domain;
        _ = cancellationToken;
        return OperationResult<IncrementalTrainingSession>.Fail(
            "ContinuousLearning training is inactive.",
            "CONTINUOUS_LEARNING_TRAINING_INACTIVE");
    }
}
