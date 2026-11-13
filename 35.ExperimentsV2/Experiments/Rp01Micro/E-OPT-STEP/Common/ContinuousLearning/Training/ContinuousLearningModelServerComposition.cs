// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ModelServer")]

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TrainingLoop;
using Tokenizer;
using TransformerModel.Config;

/// <summary>
/// Redacted status for the ModelServer-owned ContinuousLearning configuration.
/// Configuration admission is not graph materialization or execution authority.
/// </summary>
public sealed record ContinuousLearningProductionReadinessStatus(
    [property: System.Text.Json.Serialization.JsonPropertyName("configuration_admitted")]
    bool ConfigurationAdmitted,
    [property: System.Text.Json.Serialization.JsonPropertyName("configuration_status")]
    string ConfigurationStatus,
    [property: System.Text.Json.Serialization.JsonPropertyName("configuration_error_code")]
    string ConfigurationErrorCode,
    [property: System.Text.Json.Serialization.JsonPropertyName("teacher_lane")]
    string TeacherLane,
    [property: System.Text.Json.Serialization.JsonPropertyName("teacher_policy_configuration")]
    string TeacherPolicyConfiguration,
    [property: System.Text.Json.Serialization.JsonPropertyName("legal_evidence_configured")]
    bool LegalEvidenceConfigured,
    [property: System.Text.Json.Serialization.JsonPropertyName("legal_evidence_verified")]
    bool LegalEvidenceVerified,
    [property: System.Text.Json.Serialization.JsonPropertyName("local_retention_policy_ref")]
    string? LocalRetentionPolicyRef,
    [property: System.Text.Json.Serialization.JsonPropertyName("output_license_ref")]
    string? OutputLicenseRef,
    [property: System.Text.Json.Serialization.JsonPropertyName("ollama_manifest_digest_configured")]
    bool OllamaManifestDigestConfigured,
    [property: System.Text.Json.Serialization.JsonPropertyName("ollama_model_layer_digest_configured")]
    bool OllamaModelLayerDigestConfigured,
    [property: System.Text.Json.Serialization.JsonPropertyName("composition_materialized")]
    bool CompositionMaterialized,
    [property: System.Text.Json.Serialization.JsonPropertyName("path_identity_verified")]
    bool PathIdentityVerified,
    [property: System.Text.Json.Serialization.JsonPropertyName("retained_object_identity_bound")]
    bool RetainedObjectIdentityBound,
    [property: System.Text.Json.Serialization.JsonPropertyName("stable_dpo_executor_configured")]
    bool StableDpoExecutorConfigured,
    [property: System.Text.Json.Serialization.JsonPropertyName("training_enabled")]
    bool TrainingEnabled,
    [property: System.Text.Json.Serialization.JsonPropertyName("cycle_execution_available")]
    bool CycleExecutionAvailable,
    [property: System.Text.Json.Serialization.JsonPropertyName("performs_training")]
    bool PerformsTraining,
    [property: System.Text.Json.Serialization.JsonPropertyName("product_ready")]
    bool ProductReady)
{
    public ContinuousLearningProductionReadinessStatus(
        bool ConfigurationAdmitted,
        string ConfigurationStatus,
        string ConfigurationErrorCode,
        string TeacherLane,
        string TeacherPolicyConfiguration,
        bool CompositionMaterialized,
        bool PathIdentityVerified,
        bool RetainedObjectIdentityBound,
        bool StableDpoExecutorConfigured,
        bool TrainingEnabled,
        bool CycleExecutionAvailable,
        bool PerformsTraining,
        bool ProductReady)
        : this(
            ConfigurationAdmitted,
            ConfigurationStatus,
            ConfigurationErrorCode,
            TeacherLane,
            TeacherPolicyConfiguration,
            LegalEvidenceConfigured: false,
            LegalEvidenceVerified: false,
            LocalRetentionPolicyRef: null,
            OutputLicenseRef: null,
            OllamaManifestDigestConfigured: false,
            OllamaModelLayerDigestConfigured: false,
            CompositionMaterialized,
            PathIdentityVerified,
            RetainedObjectIdentityBound,
            StableDpoExecutorConfigured,
            TrainingEnabled,
            CycleExecutionAvailable,
            PerformsTraining,
            ProductReady)
    {
    }

    public void Deconstruct(
        out bool ConfigurationAdmitted,
        out string ConfigurationStatus,
        out string ConfigurationErrorCode,
        out string TeacherLane,
        out string TeacherPolicyConfiguration,
        out bool CompositionMaterialized,
        out bool PathIdentityVerified,
        out bool RetainedObjectIdentityBound,
        out bool StableDpoExecutorConfigured,
        out bool TrainingEnabled,
        out bool CycleExecutionAvailable,
        out bool PerformsTraining,
        out bool ProductReady)
    {
        ConfigurationAdmitted = this.ConfigurationAdmitted;
        ConfigurationStatus = this.ConfigurationStatus;
        ConfigurationErrorCode = this.ConfigurationErrorCode;
        TeacherLane = this.TeacherLane;
        TeacherPolicyConfiguration = this.TeacherPolicyConfiguration;
        CompositionMaterialized = this.CompositionMaterialized;
        PathIdentityVerified = this.PathIdentityVerified;
        RetainedObjectIdentityBound = this.RetainedObjectIdentityBound;
        StableDpoExecutorConfigured = this.StableDpoExecutorConfigured;
        TrainingEnabled = this.TrainingEnabled;
        CycleExecutionAvailable = this.CycleExecutionAvailable;
        PerformsTraining = this.PerformsTraining;
        ProductReady = this.ProductReady;
    }

    internal const string Tp1Lane = "tp1_local";
    internal const string Tp1PolicyConfiguration =
        "server_owned_qwen_local_ollama_apache20_configuration";

    internal static ContinuousLearningProductionReadinessStatus Disabled()
        => Closed("disabled", "CONTINUOUS_LEARNING_DISABLED");

    internal static ContinuousLearningProductionReadinessStatus Invalid(string errorCode)
        => Closed("invalid", errorCode);

    internal static ContinuousLearningProductionReadinessStatus ConfiguredUnverifiedInactive(
        string localRetentionPolicyRef,
        string outputLicenseRef)
        => new(
            ConfigurationAdmitted: true,
            ConfigurationStatus: "configured_unverified_inactive",
            ConfigurationErrorCode: "none",
            TeacherLane: Tp1Lane,
            TeacherPolicyConfiguration: Tp1PolicyConfiguration,
            LegalEvidenceConfigured: true,
            LegalEvidenceVerified: false,
            LocalRetentionPolicyRef: localRetentionPolicyRef,
            OutputLicenseRef: outputLicenseRef,
            OllamaManifestDigestConfigured: true,
            OllamaModelLayerDigestConfigured: true,
            CompositionMaterialized: false,
            PathIdentityVerified: false,
            RetainedObjectIdentityBound: false,
            StableDpoExecutorConfigured: false,
            TrainingEnabled: false,
            CycleExecutionAvailable: false,
            PerformsTraining: false,
            ProductReady: false);

    private static ContinuousLearningProductionReadinessStatus Closed(
        string status,
        string errorCode)
        => new(
            ConfigurationAdmitted: false,
            ConfigurationStatus: status,
            ConfigurationErrorCode: errorCode,
            TeacherLane: Tp1Lane,
            TeacherPolicyConfiguration: Tp1PolicyConfiguration,
            LegalEvidenceConfigured: false,
            LegalEvidenceVerified: false,
            LocalRetentionPolicyRef: null,
            OutputLicenseRef: null,
            OllamaManifestDigestConfigured: false,
            OllamaModelLayerDigestConfigured: false,
            CompositionMaterialized: false,
            PathIdentityVerified: false,
            RetainedObjectIdentityBound: false,
            StableDpoExecutorConfigured: false,
            TrainingEnabled: false,
            CycleExecutionAvailable: false,
            PerformsTraining: false,
            ProductReady: false);
}

/// <summary>
/// Серверный набор входов для repository-owned обучения.
/// HTTP-клиент не приносит модель, tokenizer, LoRA, путь репозитория или право
/// менять веса: все эти вещи берутся только из уже принятой конфигурации
/// ContinuousLearning и из удержанного сервером tokenizer-файла.
/// </summary>
internal sealed record ContinuousLearningRepositoryOwnedTrainingExecutionContext(
    RepositoryOwnedIncrementalTrainingSessionStartRequest RepositoryStartRequest,
    ModelConfig ModelConfig,
    BpeTokenizer Tokenizer,
    LoraConfig LoraConfig,
    ulong RootInitializationSeed,
    int DpoStepCount,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Defensive, path-free snapshot of the admitted policy-base identity and
/// model shape needed by the production ModelServer binding boundary.
/// </summary>
internal sealed class ContinuousLearningDpoPolicyBaseBindingConfiguration
{
    private readonly ModelConfig _modelConfig;

    internal ContinuousLearningDpoPolicyBaseBindingConfiguration(
        ModelConfig modelConfig,
        string modelId,
        string modelVersion,
        ulong? rootInitializationSeed,
        string trainingConfigurationFingerprint, // Capture the existing training configuration fingerprint for retained-provider and random-context identity.
        RepositoryOwnedFullLoraCheckpointDescriptor? initialFullCheckpoint = null) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    {
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(modelId);
        ArgumentNullException.ThrowIfNull(modelVersion);
        ArgumentNullException.ThrowIfNull(trainingConfigurationFingerprint);
        _modelConfig = CopyModelConfig(modelConfig);
        ModelId = modelId;
        ModelVersion = modelVersion;
        RootInitializationSeed = rootInitializationSeed;
        TrainingConfigurationFingerprint = trainingConfigurationFingerprint;
        InitialFullCheckpoint = initialFullCheckpoint; // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    }

    internal ModelConfig ModelConfig => CopyModelConfig(_modelConfig);
    internal string ModelId { get; }
    internal string ModelVersion { get; }
    internal ulong? RootInitializationSeed { get; }
    internal string TrainingConfigurationFingerprint { get; }
    internal RepositoryOwnedFullLoraCheckpointDescriptor? InitialFullCheckpoint { get; } // Carry the same immutable full-LoRA declaration through startup and later policy loading.

    private static ModelConfig CopyModelConfig(ModelConfig modelConfig)
        => new()
        {
            VocabSize = modelConfig.VocabSize,
            ContextLen = modelConfig.ContextLen,
            HiddenDim = modelConfig.HiddenDim,
            NumLayers = modelConfig.NumLayers,
            NumHeads = modelConfig.NumHeads,
            NumKvHeads = modelConfig.NumKvHeads,
            FfnHiddenDim = modelConfig.FfnHiddenDim,
            RopeTheta = modelConfig.RopeTheta,
            RmsNormEps = modelConfig.RmsNormEps,
        };
}

/// <summary>
/// Canonical, path-free identity for the scalar training configuration that
/// participates in one root training initialization.
/// </summary>
internal static class ContinuousLearningTrainingConfigurationFingerprint
{
    private const string DomainTag =
        "xiigen.continuous-learning.training-configuration-fingerprint/v1";

    internal static string Compute(TrainingConfig configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendUtf8Frame(hash, DomainTag);
        AppendInt32(hash, nameof(TrainingConfig.BatchSize), configuration.BatchSize);
        AppendInt32(hash, nameof(TrainingConfig.SeqLen), configuration.SeqLen);
        AppendFloat(hash, nameof(TrainingConfig.MaxLr), configuration.MaxLr);
        AppendFloat(hash, nameof(TrainingConfig.MinLr), configuration.MinLr);
        AppendInt32(hash, nameof(TrainingConfig.WarmupSteps), configuration.WarmupSteps);
        AppendInt32(hash, nameof(TrainingConfig.TotalSteps), configuration.TotalSteps);
        AppendInt32(hash, nameof(TrainingConfig.GradAccumSteps), configuration.GradAccumSteps);
        AppendFloat(hash, nameof(TrainingConfig.GradClipNorm), configuration.GradClipNorm);
        AppendFloat(hash, nameof(TrainingConfig.WeightDecay), configuration.WeightDecay);
        AppendInt32(hash, nameof(TrainingConfig.CheckpointEvery), configuration.CheckpointEvery);
        AppendInt32(hash, nameof(TrainingConfig.LogEvery), configuration.LogEvery);
        AppendOptionalUInt64(
            hash,
            nameof(TrainingConfig.RootInitializationSeed),
            configuration.RootInitializationSeed);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendInt32(
        IncrementalHash hash,
        string name,
        int value)
    {
        Span<byte> encoded = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(encoded, value);
        AppendPresentField(hash, name, encoded);
    }

    private static void AppendFloat(
        IncrementalHash hash,
        string name,
        float value)
    {
        Span<byte> encoded = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteUInt32BigEndian(
            encoded,
            unchecked((uint)BitConverter.SingleToInt32Bits(value)));
        AppendPresentField(hash, name, encoded);
    }

    private static void AppendOptionalUInt64(
        IncrementalHash hash,
        string name,
        ulong? value)
    {
        AppendUtf8Frame(hash, name);
        if (!value.HasValue)
        {
            AppendPresence(hash, present: false);
            AppendFrame(hash, ReadOnlySpan<byte>.Empty);
            return;
        }

        AppendPresence(hash, present: true);
        Span<byte> encoded = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(encoded, value.Value);
        AppendFrame(hash, encoded);
    }

    private static void AppendPresentField(
        IncrementalHash hash,
        string name,
        ReadOnlySpan<byte> value)
    {
        AppendUtf8Frame(hash, name);
        AppendPresence(hash, present: true);
        AppendFrame(hash, value);
    }

    private static void AppendPresence(IncrementalHash hash, bool present)
    {
        Span<byte> marker = stackalloc byte[1];
        marker[0] = present ? (byte)1 : (byte)0;
        hash.AppendData(marker);
    }

    private static void AppendUtf8Frame(IncrementalHash hash, string value)
        => AppendFrame(hash, Encoding.UTF8.GetBytes(value));

    private static void AppendFrame(
        IncrementalHash hash,
        ReadOnlySpan<byte> value)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(length, value.Length);
        hash.AppendData(length);
        hash.AppendData(value);
    }
}

/// <summary>
/// Immutable, path-free identity of the server-owned base checkpoint. This is
/// independent from the teacher identity used to admit TP1 inference calls.
/// </summary>
internal sealed record ContinuousLearningBaseCheckpointModelIdentity(
    string ModelId,
    string Revision);

/// <summary>
/// Opaque identity for one startup-owned policy-base binding generation.
/// </summary>
internal sealed class ContinuousLearningDpoPolicyBaseBindingToken
{
    private Action<ContinuousLearningDpoPolicyBaseBindingToken>?
        _revokeAfterTrustLoss;
    private object? _committedGatewayIdentity;

    internal ContinuousLearningDpoPolicyBaseBindingToken(long generation)
    {
        Generation = generation;
    }

    internal long Generation { get; }
    internal bool IsTrustLossRevocationRegistered
        => Volatile.Read(ref _revokeAfterTrustLoss) is not null;
    internal bool IsGatewayCommitted
        => Volatile.Read(ref _committedGatewayIdentity) is not null;

    internal bool IsGatewayCommittedBy(object gatewayIdentity)
    {
        ArgumentNullException.ThrowIfNull(gatewayIdentity);
        return ReferenceEquals(
            Volatile.Read(ref _committedGatewayIdentity),
            gatewayIdentity);
    }

    internal bool TryRegisterTrustLossRevocation(
        Action<ContinuousLearningDpoPolicyBaseBindingToken> revoke)
    {
        ArgumentNullException.ThrowIfNull(revoke);
        return Interlocked.CompareExchange(
            ref _revokeAfterTrustLoss,
            revoke,
            comparand: null) is null;
    }

    internal void RevokeAfterTrustLoss()
        => Volatile.Read(ref _revokeAfterTrustLoss)?.Invoke(this);

    internal bool TryMarkGatewayCommitted(object gatewayIdentity)
    {
        ArgumentNullException.ThrowIfNull(gatewayIdentity);
        return Interlocked.CompareExchange(
            ref _committedGatewayIdentity,
            gatewayIdentity,
            comparand: null) is null;
    }
}

/// <summary>
/// One-shot startup-owned issuer prepared for one exact DesignConversion route
/// surface. Only the ModelServer mapping boundary commits it after mapping.
/// </summary>
internal abstract class ContinuousLearningDesignConversionRouteMappingCapability
{
    private static readonly System.Runtime.CompilerServices
        .ConditionalWeakTable<Issuer, CommitState> CommitStates = new();
    private int _active = 1;

    internal ContinuousLearningDesignConversionRouteMappingCapability(
        Issuer issuer,
        object registryIdentity,
        object surfaceIdentity,
        object providerIdentity,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(registryIdentity);
        ArgumentNullException.ThrowIfNull(surfaceIdentity);
        ArgumentNullException.ThrowIfNull(providerIdentity);
        ArgumentNullException.ThrowIfNull(legalAuthority);
        if (!TryCommit(
                issuer,
                surfaceIdentity,
                providerIdentity,
                legalAuthority))
        {
            throw new InvalidOperationException(
                "The exact route mapping capability request is invalid or already consumed.");
        }
        Owner = issuer.Owner;
        MappingIssuer = issuer;
        RegistryIdentity = registryIdentity;
        SurfaceIdentity = surfaceIdentity;
        ProviderIdentity = providerIdentity;
        LegalAuthority = legalAuthority;
    }

    internal ContinuousLearningModelServerStartup Owner { get; }
    internal Issuer MappingIssuer { get; }
    internal object RegistryIdentity { get; }
    internal object SurfaceIdentity { get; }
    internal object ProviderIdentity { get; }
    internal ServerOwnedLegalTrainingTeacherAdmissionAuthority LegalAuthority { get; }
    internal bool IsActive => Volatile.Read(ref _active) == 1;

    internal void Revoke()
    {
        if (!IsActive)
            return;

        Owner.HandleDesignConversionRouteMappingCapabilityRevoked(this);
        RevokeWithoutOwnerCallback();
    }

    internal void RevokeWithoutOwnerCallback()
        => Interlocked.Exchange(ref _active, 0);

    internal sealed class Issuer
    {
        internal Issuer(
            ContinuousLearningModelServerStartup owner,
            object surfaceIdentity,
            object providerIdentity,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
        {
            Owner = owner;
            SurfaceIdentity = surfaceIdentity;
            ProviderIdentity = providerIdentity;
            LegalAuthority = legalAuthority;
        }

        internal ContinuousLearningModelServerStartup Owner { get; }
        internal object SurfaceIdentity { get; }
        internal object ProviderIdentity { get; }
        internal ServerOwnedLegalTrainingTeacherAdmissionAuthority
            LegalAuthority { get; }

    }

    private static bool TryCommit(
        Issuer issuer,
        object surfaceIdentity,
        object providerIdentity,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
    {
        if (!ReferenceEquals(issuer.SurfaceIdentity, surfaceIdentity)
            || !ReferenceEquals(issuer.ProviderIdentity, providerIdentity)
            || !ReferenceEquals(issuer.LegalAuthority, legalAuthority))
        {
            return false;
        }
        var state = CommitStates.GetValue(
            issuer,
            static _ => new CommitState());
        return Interlocked.CompareExchange(
            ref state.Committed,
            1,
            0) == 0;
    }

    private sealed class CommitState
    {
        internal int Committed;
    }
}

/// <summary>
/// One-shot issuer snapshotting the exact active route and provider generation.
/// The DesignConversion association boundary commits it only after registration.
/// </summary>
internal abstract class ContinuousLearningDesignConversionTeacherAssociationCapability
{
    private static readonly System.Runtime.CompilerServices
        .ConditionalWeakTable<Issuer, CommitState> CommitStates = new();
    private int _active = 1;

    internal ContinuousLearningDesignConversionTeacherAssociationCapability(
        Issuer issuer,
        object providerIdentity,
        object runtimeIdentity,
        object registrationIdentity)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(providerIdentity);
        ArgumentNullException.ThrowIfNull(runtimeIdentity);
        ArgumentNullException.ThrowIfNull(registrationIdentity);
        if (!TryCommit(issuer, providerIdentity))
        {
            throw new InvalidOperationException(
                "The exact teacher association capability request is invalid or already consumed.");
        }
        Owner = issuer.Owner;
        AssociationIssuer = issuer;
        RouteCapability = issuer.RouteCapability;
        BindingToken = issuer.BindingToken;
        Generation = issuer.Generation;
        ProviderIdentity = providerIdentity;
        RuntimeIdentity = runtimeIdentity;
        RegistrationIdentity = registrationIdentity;
    }

    internal ContinuousLearningModelServerStartup Owner { get; }
    internal Issuer AssociationIssuer { get; }
    internal ContinuousLearningDesignConversionRouteMappingCapability
        RouteCapability { get; }
    internal ContinuousLearningDpoPolicyBaseBindingToken BindingToken { get; }
    internal long Generation { get; }
    internal object ProviderIdentity { get; }
    internal object RuntimeIdentity { get; }
    internal object RegistrationIdentity { get; }
    internal bool IsActive => Volatile.Read(ref _active) == 1;

    internal void Revoke()
    {
        if (!IsActive)
            return;

        Owner.HandleDesignConversionTeacherAssociationCapabilityRevoked(this);
        RevokeWithoutOwnerCallback();
    }

    internal void RevokeWithoutOwnerCallback()
        => Interlocked.Exchange(ref _active, 0);

    internal sealed class Issuer
    {
        internal Issuer(
            ContinuousLearningModelServerStartup owner,
            ContinuousLearningDesignConversionRouteMappingCapability
                routeCapability,
            ContinuousLearningDpoPolicyBaseBindingToken bindingToken,
            long generation,
            object providerIdentity)
        {
            Owner = owner;
            RouteCapability = routeCapability;
            BindingToken = bindingToken;
            Generation = generation;
            ProviderIdentity = providerIdentity;
        }

        internal ContinuousLearningModelServerStartup Owner { get; }
        internal ContinuousLearningDesignConversionRouteMappingCapability
            RouteCapability { get; }
        internal ContinuousLearningDpoPolicyBaseBindingToken BindingToken { get; }
        internal long Generation { get; }
        internal object ProviderIdentity { get; }

    }

    private static bool TryCommit(Issuer issuer, object providerIdentity)
    {
        if (!ReferenceEquals(issuer.ProviderIdentity, providerIdentity))
            return false;
        var state = CommitStates.GetValue(
            issuer,
            static _ => new CommitState());
        return Interlocked.CompareExchange(
            ref state.Committed,
            1,
            0) == 0;
    }

    private sealed class CommitState
    {
        internal int Committed;
    }
}

internal enum ContinuousLearningDpoPolicyBaseWithdrawalOutcome
{
    Rejected,
    Withdrawn,
    AlreadyInactive,
}

internal readonly record struct ContinuousLearningDpoPolicyBaseWithdrawal(
    ContinuousLearningDpoPolicyBaseWithdrawalOutcome Outcome,
    ContinuousLearningModelServerStartup.DpoPolicyBaseWithdrawalTicket? Ticket);

internal enum ContinuousLearningDpoPolicyBaseLifecycleState
{
    None,
    BindingPending,
    ProviderBound,
    Failed,
    Stopped,
    TrustLost,
}

internal sealed class ContinuousLearningRetirementFailureArbiter
{
    private const int MaximumCleanupFailureCount = 18;
    private const string RetirementFailureDataKey =
        "CONTINUOUS_LEARNING_RETIREMENT_FAILURE";
    private readonly Exception?[] _cleanupFailures =
        new Exception?[MaximumCleanupFailureCount];
    private int _cleanupFailureCount;
    private Exception? _originalFailure;
    private Exception? _firstCleanupFailure;
    private Exception? _firstFatalCleanupFailure;

    internal void CaptureOriginal(Exception exception)
        => _originalFailure ??= exception;

    internal void CaptureCleanup(Exception exception)
    {
        _firstCleanupFailure ??= exception;
        if (_firstFatalCleanupFailure is null && IsFatalConservatively(exception))
            _firstFatalCleanupFailure = exception;
        for (var index = 0; index < _cleanupFailureCount; index++)
        {
            if (ReferenceEquals(_cleanupFailures[index], exception))
                return;
        }
        if (_cleanupFailureCount < _cleanupFailures.Length)
            _cleanupFailures[_cleanupFailureCount++] = exception;
    }

    internal Exception? Resolve()
    {
        var originalFatal = _originalFailure is not null &&
            IsFatalConservatively(_originalFailure);
        var winner = originalFatal
            ? _originalFailure
            : _firstFatalCleanupFailure ??
              _originalFailure ??
              _firstCleanupFailure;
        if (winner is null)
            return null;

        try
        {
            var later = new List<Exception>();
            var seen = new HashSet<Exception>(
                ReferenceEqualityComparer.Instance) { winner };
            AddExistingAttachment(winner, later, seen);
            AddFlattened(_originalFailure, later, seen);
            for (var index = 0; index < _cleanupFailureCount; index++)
                AddFlattened(_cleanupFailures[index], later, seen);
            if (later.Count > 0)
                winner.Data[RetirementFailureDataKey] = later.Count == 1
                    ? later[0]
                    : new AggregateException(later);
        }
        catch { }
        return winner;
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
/// Immutable startup result. The deferred plan is deliberately internal and
/// exposes no build, run, train, cycle, or filesystem operation.
/// </summary>
public sealed class ContinuousLearningModelServerStartup
{
    private const string ExactRouteMappingCapabilityAssemblyName =
        "ModelServer";
    private const string ExactRouteMappingCapabilityTypeName =
        "ModelServer.Program+MappedDesignConversionRouteCapability";
    private const string ExactTeacherAssociationCapabilityAssemblyName =
        "44.DesignConversionDomain";
    private const string ExactTeacherAssociationCapabilityTypeName =
        "DesignConversionDomain.DesignConversionDpoTeacherAttestationAssociation+RegisteredDesignConversionTeacherAssociationCapability";

    internal sealed class DpoPolicyBaseWithdrawalTicket
    {
        private int _completed;

        internal DpoPolicyBaseWithdrawalTicket(
            ContinuousLearningModelServerStartup owner,
            ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
        {
            Owner = owner;
            BindingToken = bindingToken;
        }

        internal ContinuousLearningModelServerStartup Owner { get; }
        internal ContinuousLearningDpoPolicyBaseBindingToken BindingToken { get; }
        internal bool TryComplete() =>
            Interlocked.Exchange(ref _completed, 1) == 0;
    }

    private static readonly AsyncLocal<Action<Task>?>
        RetirementTaskPublishedObserver = new();
    internal static Action<Task>? RetirementTaskPublishedObserverForTesting
    {
        get => RetirementTaskPublishedObserver.Value;
        set => RetirementTaskPublishedObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeRetirementPendingCompletionAllocationObserver = new();
    internal static Action?
        BeforeRetirementPendingCompletionAllocationObserverForTesting
    {
        get => BeforeRetirementPendingCompletionAllocationObserver.Value;
        set => BeforeRetirementPendingCompletionAllocationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeRetirementDependencySnapshotObserver = new();
    internal static Action?
        BeforeRetirementDependencySnapshotObserverForTesting
    {
        get => BeforeRetirementDependencySnapshotObserver.Value;
        set => BeforeRetirementDependencySnapshotObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeRetirementFailureStateAllocationObserver = new();
    internal static Action?
        BeforeRetirementFailureStateAllocationObserverForTesting
    {
        get => BeforeRetirementFailureStateAllocationObserver.Value;
        set => BeforeRetirementFailureStateAllocationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeRetirementCompletionLaunchObserver = new();
    internal static Action?
        BeforeRetirementCompletionLaunchObserverForTesting
    {
        get => BeforeRetirementCompletionLaunchObserver.Value;
        set => BeforeRetirementCompletionLaunchObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeReadinessSnapshotAllocationObserver = new();
    internal static Action?
        BeforeReadinessSnapshotAllocationObserverForTesting
    {
        get => BeforeReadinessSnapshotAllocationObserver.Value;
        set => BeforeReadinessSnapshotAllocationObserver.Value = value;
    }
    internal const string NamespacePathUnverifiedErrorCode =
        "CONTINUOUS_LEARNING_NAMESPACE_PATH_UNVERIFIED";
    internal const string RetainedObjectIdentityLostErrorCode =
        "CONTINUOUS_LEARNING_RETAINED_OBJECT_IDENTITY_LOST";
    internal const string MaterializationFailedErrorCode =
        "CONTINUOUS_LEARNING_MATERIALIZATION_FAILED";
    internal const string DpoPolicyBaseBindingFailedErrorCode =
        "CONTINUOUS_LEARNING_DPO_POLICY_BASE_BINDING_FAILED";
    internal const string DpoPolicyBaseStoppedErrorCode =
        "CONTINUOUS_LEARNING_DPO_POLICY_BASE_STOPPED";
    internal const string DesignConversionTrainingIntakeTeacherAssociationFailedErrorCode =
        "CONTINUOUS_LEARNING_DESIGN_CONVERSION_TRAINING_INTAKE_TEACHER_ASSOCIATION_FAILED";
    internal const string MutationAdmissionDomainAlreadyOwnedErrorCode =
        ContinuousLearningMutationAdmissionDomainAlreadyOwnedException
            .StableErrorCode;

    private readonly object _readinessGate = new();
    private readonly ContinuousLearningDeferredCompositionPlan? _deferredPlan;
    private readonly ServerOwnedLegalTrainingTeacherAdmissionAuthority
        _legalTeacherAdmissionAuthority;
    private ServerOwnedContinuousLearningFileAccess? _issuedFileAccess;
    private ServerOwnedContinuousLearningFileAccess? _fileAccess;
    private ContinuousLearningMaterializedTp1Composition? _materializedComposition;
    private ContinuousLearningMaterializedTp1Composition? _retiringComposition;
    private Task _retirementTask = Task.CompletedTask;
    private TaskCompletionSource<object?>? _pendingRetirementCompletion;
    private RetirementStartPlan? _pendingRetirementStart;
    private bool _retirementStarted;
    private readonly List<Func<Task>> _retirementDependencies = [];
    private ContinuousLearningProductionReadinessStatus _readiness;
    private bool _retainedObjectTrustLost;
    private bool _materializationAttempted;
    private RepositoryOwnedFullLoraCheckpointDescriptor? _initialFullCheckpoint; // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    private long _dpoPolicyBaseLifecycleGeneration;
    private ContinuousLearningDpoPolicyBaseLifecycleState
        _dpoPolicyBaseLifecycleState;
    private ContinuousLearningDpoPolicyBaseBindingToken?
        _activeDpoPolicyBaseBindingToken;
    private ContinuousLearningDpoPolicyBaseBindingToken?
        _pendingDpoPolicyBaseBindingToken;
    private ContinuousLearningDpoPolicyBaseBindingToken?
        _currentDpoPolicyBaseBindingToken;
    private DpoPolicyBaseWithdrawalTicket?
        _pendingDpoPolicyBaseWithdrawalTicket;
    private ContinuousLearningDesignConversionRouteMappingCapability.Issuer?
        _designConversionRouteMappingIssuer;
    private ContinuousLearningDesignConversionRouteMappingCapability?
        _designConversionRouteMappingCapability;
    private ContinuousLearningDesignConversionTeacherAssociationCapability.Issuer?
        _designConversionTeacherAssociationIssuer;
    private ContinuousLearningDesignConversionTeacherAssociationCapability?
        _designConversionTeacherAssociationCapability;

    private ContinuousLearningModelServerStartup(
        ContinuousLearningDeferredCompositionPlan? deferredPlan,
        ContinuousLearningProductionReadinessStatus readiness,
        CodeDocumentationOneLessonGrantCatalog oneLessonGrantCatalog)
    {
        _deferredPlan = deferredPlan;
        _readiness = readiness;
        OneLessonGrantCatalog = oneLessonGrantCatalog;
        _legalTeacherAdmissionAuthority = deferredPlan is null
            ? ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable()
            : ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .IssueFromAdmittedConfiguration(
                    deferredPlan.TeacherConfiguration,
                    IsLegalTrainingTeacherAdmissionActive);
    }

    /// <summary>
    /// Startup-owned one-lesson grant lookup. It only resolves a selector;
    /// it does not start training or authorize caller-provided inputs.
    /// </summary>
    public CodeDocumentationOneLessonGrantCatalog OneLessonGrantCatalog { get; }

    private string? _protectedTeachingDisclosureBytes; // Retain the exact process-captured V3 catalog bytes that distinguish protected startup.
    private int _protectedTeachingDisclosureRevoked; // Keep revocation separate from process text so unchanged configuration cannot revive a stopped owner.
    internal bool UsesProtectedTeachingDisclosureContractV3 => OneLessonGrantCatalog.IsTeachingDisclosureV3; // Select the V3 consumer contract only when the retained catalog validated as V3.

    /// <summary>Capture protected process configuration once; callback composition never acquires this provenance.</summary>
    public static ContinuousLearningModelServerStartup CreateProtectedFromProcessEnvironment() // Expose the existing no-argument process entry, never a caller-supplied authority reader.
    { // Capture the protected key once before composing any grant owners.
        var captured = Environment.GetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable); // Read the existing process grant key directly, without accepting callback provenance.
        var startup = Create(name => name == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable ? captured : Environment.GetEnvironmentVariable(name)); // Compose using that exact captured key while other configuration keeps its existing process reader.
        if (startup.OneLessonGrantCatalog.IsTeachingDisclosureV3) startup._protectedTeachingDisclosureBytes = captured; // Retain protected provenance only when the captured catalog passed the V3 decoder.
        return startup; // Return the same startup instance that owns the retained grant and approval objects.
    } // Finish process capture without adding an injectable protected startup factory.
    internal OperationResult<CodeDocumentationOneLessonTeachingDisclosureApproval> ResolveProtectedTeachingDisclosureApproval(CodeDocumentationOneLessonGrantV1 grant) // Resolve one approval from this startup's exact retained grant and revision.
    { // Keep provenance, membership and current revision refusals distinct.
        var approval = OneLessonGrantCatalog.FindTeachingApproval(grant); // Ask the catalog for approval membership of the actual retained grant object.
        if (_protectedTeachingDisclosureBytes is null) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable).MapError<CodeDocumentationOneLessonTeachingDisclosureApproval>(); // Refuse callback startup even if its catalog bytes equal the process configuration.
        if (approval is null) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch).MapError<CodeDocumentationOneLessonTeachingDisclosureApproval>(); // Refuse a grant without exact retained approval membership.
        var current = CheckProtectedTeachingDisclosureRevision(approval); // Recheck stop revocation and the current complete process revision before returning approval.
        return current.IsSuccess ? OperationResult<CodeDocumentationOneLessonTeachingDisclosureApproval>.Ok(approval) : current.MapError<CodeDocumentationOneLessonTeachingDisclosureApproval>(); // Return the retained approval only on a successful revision check, preserving actual refusal codes.
    } // Finish approval resolution without permitting caller JSON or hashes to mint ownership.
    internal OperationResult<bool> CheckProtectedTeachingDisclosureRevision(CodeDocumentationOneLessonTeachingDisclosureApproval approval, CancellationToken token = default) // Revalidate the same approval's current protected owner with the supplied cancellation policy.
    { // Check provenance and membership before comparing current process bytes.
        token.ThrowIfCancellationRequested(); // Honor caller cancellation; independent final callers explicitly supply None.
        if (_protectedTeachingDisclosureBytes is null) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable); // Refuse a startup that never captured protected process provenance.
        var grant = OneLessonGrantCatalog.ResolveApprovalOwner(approval); // Resolve only the actual approval object held by this catalog.
        if (grant is null) return CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch); // Reject foreign approvals even when their evidence digests match.
        return Volatile.Read(ref _protectedTeachingDisclosureRevoked) == 0 && string.Equals(_protectedTeachingDisclosureBytes, Environment.GetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable), StringComparison.Ordinal) // Require both unrevoked startup and ordinal equality of the complete current/captured process text.
            ? OperationResult<bool>.Ok(true) : CodeDocumentationOneLessonTeachingDisclosureApproval.Refuse(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale); // Return success only for that live revision; otherwise preserve the exact stale family.
    } // Finish revision checking without I/O or a new grant authority source.
    internal void RevokeProtectedTeachingDisclosureApprovals() => Interlocked.Exchange(ref _protectedTeachingDisclosureRevoked, 1); // Revoke once atomically; later stop/cleanup calls remain nonthrowing and cannot restore rights.

    internal bool HasDeferredCompositionPlan => _deferredPlan is not null;
    internal bool TrainingActivationRequested =>
        _deferredPlan?.TrainingActivationRequested == true;
    internal bool LocalIndependentPreferenceEvaluatorRequested =>
        _deferredPlan?.LocalIndependentPreferenceEvaluatorRequested == true;
    internal ContinuousLearningTp1DpoConfiguration TeacherConfiguration =>
        (_deferredPlan ?? throw new InvalidOperationException())
            .TeacherConfiguration;
    internal ServerOwnedLegalTrainingTeacherAdmissionAuthority
        LegalTeacherAdmissionAuthority => _legalTeacherAdmissionAuthority;

    internal OperationResult<ContinuousLearningRepositoryOwnedTrainingExecutionContext>
        PrepareRepositoryOwnedTrainingExecutionContext(
            string domainSlug,
            ServerOwnedContinuousLearningFileAccess fileAccess,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainSlug) ||
            !StringComparer.Ordinal.Equals(domainSlug, domainSlug.Trim()) ||
            fileAccess is null ||
            _deferredPlan is null)
        {
            return RepositoryOwnedTrainingContextUnavailable();
        }

        ContinuousLearningDeferredCompositionPlan plan;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted ||
                !_readiness.CompositionMaterialized ||
                !_readiness.PathIdentityVerified ||
                !_readiness.RetainedObjectIdentityBound ||
                !_readiness.LegalEvidenceVerified ||
                !_readiness.StableDpoExecutorConfigured ||
                !_readiness.TrainingEnabled ||
                !_readiness.CycleExecutionAvailable ||
                !_readiness.PerformsTraining ||
                _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound ||
                _activeDpoPolicyBaseBindingToken is null ||
                _designConversionRouteMappingCapability is not { IsActive: true } ||
                _designConversionTeacherAssociationCapability is not { IsActive: true } ||
                !ReferenceEquals(_fileAccess, fileAccess) ||
                _deferredPlan is null)
            {
                return RepositoryOwnedTrainingContextUnavailable();
            }

            plan = _deferredPlan;
        }

        var rootInitializationSeed = plan.RootInitializationSeed;
        if (rootInitializationSeed is null)
            return RepositoryOwnedTrainingContextUnavailable();

        var startRequest = plan.ResolveRepositoryStartRequest(domainSlug);
        if (!startRequest.IsSuccess || startRequest.Value is null)
        {
            return startRequest.MapError<
                ContinuousLearningRepositoryOwnedTrainingExecutionContext>();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            fileAccess.EnsureRetained();
            using var tokenizerStream = fileAccess.OpenRetainedArtifactForRead(
                ContinuousLearningRetainedArtifactRole.Tokenizer);
            var tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
                tokenizerStream,
                plan.ModelConfig.VocabSize,
                cancellationToken);
            var loraConfig = LoraApplicator.ValidateAndCanonicalize(
                _initialFullCheckpoint?.LoraConfig ?? IncrementalTrainer.DefaultConfig); // Use the explicit root adapter configuration while keeping legacy defaults unchanged.

            return OperationResult<
                ContinuousLearningRepositoryOwnedTrainingExecutionContext>.Ok(
                new ContinuousLearningRepositoryOwnedTrainingExecutionContext(
                    startRequest.Value,
                    plan.ModelConfig,
                    tokenizer,
                    loraConfig,
                    rootInitializationSeed.Value,
                    plan.TeacherConfiguration.DpoMaxStepCount,
                    [
                        "continuous-learning-startup:repository-owned-start-request",
                        "continuous-learning-file-access:retained-tokenizer-opened",
                        _initialFullCheckpoint is null // Choose LoRA evidence provenance according to whether startup carries a declared full checkpoint.
                            ? "continuous-learning-lora:server-default-canonicalized" // Record canonicalized generic server-default provenance when no full-checkpoint descriptor was admitted.
                            : "continuous-learning-lora:declared-initial-full-checkpoint" // Record declared-root provenance when startup carries the admitted full-checkpoint descriptor.
                    ]));
        }
        catch (OperationCanceledException)
        {
            return OperationResult<
                ContinuousLearningRepositoryOwnedTrainingExecutionContext>.Fail(
                "Repository-owned training context preparation was cancelled before a model could train.",
                "CONTINUOUS_LEARNING_REPOSITORY_OWNED_TRAINING_CONTEXT_CANCELLED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                ContinuousLearningRepositoryOwnedTrainingExecutionContext>.Fail(
                "ContinuousLearning could not prepare a repository-owned training context from server-owned retained artifacts.",
                "CONTINUOUS_LEARNING_REPOSITORY_OWNED_TRAINING_CONTEXT_UNAVAILABLE");
        }
    }

    private static OperationResult<
        ContinuousLearningRepositoryOwnedTrainingExecutionContext>
        RepositoryOwnedTrainingContextUnavailable()
        => OperationResult<
            ContinuousLearningRepositoryOwnedTrainingExecutionContext>.Fail(
            "ContinuousLearning repository-owned training context is not available from the admitted ModelServer configuration.",
            "CONTINUOUS_LEARNING_REPOSITORY_OWNED_TRAINING_CONTEXT_UNAVAILABLE");

    internal bool IsLegalTrainingTeacherAdmissionActive()
    {
        lock (_readinessGate)
        {
            return _readiness.ConfigurationAdmitted
                   && _materializedComposition is not null
                   && _readiness.CompositionMaterialized
                   && _readiness.PathIdentityVerified
                   && _readiness.RetainedObjectIdentityBound
                   && _readiness.LegalEvidenceVerified
                   && _dpoPolicyBaseLifecycleState
                       == ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                   && _activeDpoPolicyBaseBindingToken is not null
                   && _designConversionRouteMappingCapability is
                       { IsActive: true }
                   && _designConversionTeacherAssociationCapability is
                       { IsActive: true }
                   && _readiness.StableDpoExecutorConfigured
                   && _readiness.TrainingEnabled
                   && _readiness.CycleExecutionAvailable
                   && _readiness.PerformsTraining;
        }
    }
    public ContinuousLearningProductionReadinessStatus Readiness
        => ObserveReadiness();

    internal ServerOwnedContinuousLearningFileAccess AcquireRetainedFileAccess(
        ServerOwnedContinuousLearningFileAccessOptions? options = null)
    {
        var fileAccess = (_deferredPlan
                          ?? throw new ContinuousLearningPathIdentityException())
            .AcquireFileAccess(options, HandleRetainedObjectTrustLost);
        lock (_readinessGate)
            _issuedFileAccess = fileAccess;
        return fileAccess;
    }

    internal void BindRetainedFileAccess(
        ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        fileAccess.EnsureRetained();
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_issuedFileAccess, fileAccess))
                throw new ContinuousLearningPathIdentityException();
            if (_retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost)
            {
                throw new ContinuousLearningPathIdentityException();
            }
            if (ReferenceEquals(_fileAccess, fileAccess))
                return;
            if (_fileAccess is not null)
                throw new ContinuousLearningPathIdentityException();
            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var boundReadiness = _readiness with
            {
                ConfigurationStatus =
                    "retained_object_identity_bound_path_unverified_inactive",
                ConfigurationErrorCode = NamespacePathUnverifiedErrorCode,
                PathIdentityVerified = false,
                RetainedObjectIdentityBound = true,
            };
            _fileAccess = fileAccess;
            _readiness = boundReadiness;
        }
    }

    /// <summary>
    /// Materializes the inactive TP1 trainer exactly once after the hosted
    /// service has bound the retained objects. It has no public/run/cycle path.
    /// </summary>
    internal void RejectInactiveTp1Materialization() => MarkMaterializationFailed(); // Mark inactive materialization failed through the startup lifecycle and invalidate readiness.

    internal void MaterializeInactiveTp1Composition(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        CancellationToken cancellationToken = default,
        Action? afterRetainedSessionFactoryBindForTesting = null, // Allow the existing test hook to run after retained-session-factory binding and before subsequent materialization checks.
        RepositoryOwnedFullLoraCheckpointDescriptor? initialFullCheckpoint = null) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        cancellationToken.ThrowIfCancellationRequested();
        ContinuousLearningDeferredCompositionPlan plan;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                || !ReferenceEquals(_fileAccess, fileAccess)
                || _deferredPlan is null)
            {
                throw new ContinuousLearningPathIdentityException();
            }
            if (_materializedComposition is not null || _materializationAttempted)
                return;
            _materializationAttempted = true;
            plan = _deferredPlan;
        }

        if (initialFullCheckpoint is not null) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        { // The startup declaration stays bound to the admitted retained identity.
            var declared = initialFullCheckpoint.StartRequest; // Read the repository, model and runtime request captured in the full-checkpoint descriptor.
            var admitted = plan.ResolveRepositoryStartRequest(declared.DomainSlug); // Resolve the startup plan's repository-owned request for the descriptor's domain before comparing identities.
            if (!initialFullCheckpoint.MatchesModelConfig(plan.ModelConfig).IsSuccess // A typed shape refusal prevents inactive materialization and readiness.
                || plan.RootInitializationSeed != initialFullCheckpoint.RootInitializationSeed // Reject a startup root initialization seed that differs from the declared root seed.
                || !admitted.IsSuccess || admitted.Value is not { } start // Require the same repository, domain, model lane and runtime identity as startup.
                || start.RepositoryRoot != declared.RepositoryRoot || start.DomainSlug != declared.DomainSlug // Require the same repository, domain, model lane and runtime identity as startup.
                || start.DesignPatternSlug != declared.DesignPatternSlug || start.ModelIdSlug != declared.ModelIdSlug // Require the same repository, domain, model lane and runtime identity as startup.
                || start.ModelVersion != declared.ModelVersion || start.RuntimeCompatibility != declared.RuntimeCompatibility) // Require the same repository, domain, model lane and runtime identity as startup.
            { // The startup declaration stays bound to the admitted retained identity.
                MarkMaterializationFailed(); // Invalidate materialization readiness through the existing startup failure lifecycle.
                return; // Stop startup before retained factory binding and trainer construction after this declaration refusal.
            } // The startup declaration stays bound to the admitted retained identity.
            _initialFullCheckpoint = initialFullCheckpoint; // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        } // The startup declaration stays bound to the admitted retained identity.

        OperationResult<ContinuousLearningMaterializedTp1Composition>
            materialized;
        try
        {
            materialized = ContinuousLearningMaterializedTp1Composition.Create(
                _legalTeacherAdmissionAuthority,
                fileAccess,
                plan.ModelConfig,
                plan.TrainingConfig,
                plan.TeacherConfiguration,
                cancellationToken,
                plan.ResolveRepositoryStartRequest,
                afterRetainedSessionFactoryBindForTesting, // Forward the existing hook that runs immediately after retained-session-factory binding.
                loraConfig: initialFullCheckpoint?.LoraConfig, // Retain the canonical zero-dropout adapter topology from the admitted configuration.
                initialFullCheckpoint: initialFullCheckpoint); // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        }
        catch (OperationCanceledException)
        {
            MarkMaterializationFailed();
            throw;
        }
        if (!materialized.IsSuccess || materialized.Value is null)
        {
            MarkMaterializationFailed();
            return;
        }

        ContinuousLearningMaterializedTp1Composition? pendingComposition =
            materialized.Value;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                pendingComposition.EnsureRetained();
            }
            catch (ContinuousLearningPathIdentityException)
            {
                HandleRetainedObjectTrustLost();
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            lock (_readinessGate)
            {
                if (_retainedObjectTrustLost
                    || _dpoPolicyBaseLifecycleState
                        is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                        or ContinuousLearningDpoPolicyBaseLifecycleState.Failed
                        or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    || !ReferenceEquals(_fileAccess, fileAccess))
                {
                    return;
                }
                if (_materializedComposition is not null)
                    return;
                ContinuousLearningProductionReadinessStatus
                    materializedReadiness;
                try
                {
                    BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
                    materializedReadiness = _readiness with
                    {
                        ConfigurationStatus =
                            "materialized_policy_base_binding_pending_inactive",
                        ConfigurationErrorCode = "none",
                        LegalEvidenceVerified = true,
                        CompositionMaterialized = true,
                        PathIdentityVerified = true,
                        RetainedObjectIdentityBound = true,
                        StableDpoExecutorConfigured = false,
                        TrainingEnabled = false,
                        CycleExecutionAvailable = false,
                        PerformsTraining = false,
                        ProductReady = false,
                    };
                }
                catch
                {
                    _materializationAttempted = false;
                    throw;
                }
                _materializedComposition = pendingComposition;
                pendingComposition = null;
                _dpoPolicyBaseLifecycleGeneration = unchecked(
                    _dpoPolicyBaseLifecycleGeneration + 1);
                _dpoPolicyBaseLifecycleState =
                    ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending;
                Volatile.Write(ref _activeDpoPolicyBaseBindingToken, null);
                _readiness = materializedReadiness;
            }
        }
        catch (OperationCanceledException)
        {
            MarkMaterializationFailed();
            throw;
        }
        finally
        {
            pendingComposition?.Dispose();
        }
    }

    /// <summary>
    /// Captures the already-admitted, path-free DesignConversion policy-base
    /// configuration only while the exact retained objects remain verified.
    /// </summary>
    internal ContinuousLearningDpoPolicyBaseBindingConfiguration
        CaptureDpoPolicyBaseBindingConfiguration(
            ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        EnsureDpoPolicyBaseBindingRetained(fileAccess);

        ContinuousLearningDeferredCompositionPlan plan;
        lock (_readinessGate)
        {
            EnsureDpoPolicyBaseBindingStateLocked(fileAccess);
            plan = _deferredPlan!;
        }

        var snapshot = new ContinuousLearningDpoPolicyBaseBindingConfiguration(
            plan.ModelConfig,
            plan.BaseCheckpointModelIdentity.ModelId,
            plan.BaseCheckpointModelIdentity.Revision,
            plan.RootInitializationSeed,
            plan.TrainingConfigurationFingerprint, // Carry the deferred plan's exact training configuration fingerprint into retained-provider binding.
            _initialFullCheckpoint); // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        EnsureDpoPolicyBaseBindingRetained(fileAccess);
        return snapshot;
    }

    /// <summary>
    /// Revalidates that a bound ModelServer provider still belongs to the exact
    /// retained-object identity that passed inactive composition validation.
    /// </summary>
    internal void EnsureDpoPolicyBaseBindingRetained(
        ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        fileAccess.EnsureRetained();
        lock (_readinessGate)
            EnsureDpoPolicyBaseBindingStateLocked(fileAccess);
        fileAccess.EnsureRetained();
        lock (_readinessGate)
            EnsureDpoPolicyBaseBindingStateLocked(fileAccess);
    }

    internal ContinuousLearningDpoPolicyBaseBindingToken
        CaptureDpoPolicyBaseBindingToken(
            ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        EnsureDpoPolicyBaseBindingRetained(fileAccess);
        lock (_readinessGate)
        {
            EnsureDpoPolicyBaseBindingStateLocked(fileAccess);
            if (_dpoPolicyBaseLifecycleState
                != ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending)
            {
                throw new InvalidOperationException();
            }
            var bindingToken = _pendingDpoPolicyBaseBindingToken ??=
                new ContinuousLearningDpoPolicyBaseBindingToken(
                    _dpoPolicyBaseLifecycleGeneration);
            _currentDpoPolicyBaseBindingToken = bindingToken;
            return bindingToken;
        }
    }

    internal ContinuousLearningDpoPolicyBaseBindingToken
        CaptureFreshGatewayDpoPolicyBaseBindingToken(
            ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        EnsureDpoPolicyBaseBindingRetained(fileAccess);
        lock (_readinessGate)
        {
            EnsureDpoPolicyBaseBindingStateLocked(fileAccess);
            if (_dpoPolicyBaseLifecycleState
                != ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending)
            {
                throw new InvalidOperationException();
            }
            var bindingToken = new ContinuousLearningDpoPolicyBaseBindingToken(
                _dpoPolicyBaseLifecycleGeneration);
            _pendingDpoPolicyBaseBindingToken = bindingToken;
            _currentDpoPolicyBaseBindingToken = bindingToken;
            return bindingToken;
        }
    }

    internal bool TryMarkDpoPolicyBaseProviderBound(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken,
        Action? afterReadinessPublicationBeforeRetainedRevalidationForTesting =
            null)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        ArgumentNullException.ThrowIfNull(bindingToken);
        try
        {
            fileAccess.EnsureRetained();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return false;
        }

        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || !ReferenceEquals(_fileAccess, fileAccess)
                || _materializedComposition is null
                || !_readiness.CompositionMaterialized
                || !_readiness.PathIdentityVerified
                || !_readiness.RetainedObjectIdentityBound
                || _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending
                || bindingToken.Generation
                    != _dpoPolicyBaseLifecycleGeneration
                || !ReferenceEquals(
                    _pendingDpoPolicyBaseBindingToken,
                    bindingToken)
                || !ReferenceEquals(
                    _currentDpoPolicyBaseBindingToken,
                    bindingToken)
                || !bindingToken.IsTrustLossRevocationRegistered
                || !bindingToken.IsGatewayCommitted)
            {
                return false;
            }

            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var providerBoundReadiness = _readiness with
            {
                ConfigurationStatus = "materialized_inactive",
                ConfigurationErrorCode = "none",
                StableDpoExecutorConfigured = true,
            };
            _dpoPolicyBaseLifecycleState =
                ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound;
            _readiness = providerBoundReadiness;
            Volatile.Write(
                ref _activeDpoPolicyBaseBindingToken,
                bindingToken);
            _pendingDpoPolicyBaseBindingToken = null;
        }

        afterReadinessPublicationBeforeRetainedRevalidationForTesting?.Invoke();
        try
        {
            fileAccess.EnsureRetained();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return false;
        }

        lock (_readinessGate)
            return IsDpoPolicyBaseProviderBoundLocked(
                fileAccess,
                bindingToken);
    }

    internal ContinuousLearningDesignConversionRouteMappingCapability.Issuer?
        PrepareDesignConversionTrainingIntakeRouteMappingIssuer(
            object surfaceIdentity,
            object providerIdentity,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
    {
        ArgumentNullException.ThrowIfNull(surfaceIdentity);
        ArgumentNullException.ThrowIfNull(providerIdentity);
        ArgumentNullException.ThrowIfNull(legalAuthority);
        lock (_readinessGate)
        {
            if (!TrainingActivationRequested
                || !_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || !ReferenceEquals(
                    _legalTeacherAdmissionAuthority,
                    legalAuthority)
                || _designConversionRouteMappingCapability is not null
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost)
            {
                return null;
            }

            if (_designConversionRouteMappingIssuer is not null)
            {
                return ReferenceEquals(
                           _designConversionRouteMappingIssuer.SurfaceIdentity,
                           surfaceIdentity)
                       && ReferenceEquals(
                           _designConversionRouteMappingIssuer.ProviderIdentity,
                           providerIdentity)
                       && ReferenceEquals(
                           _designConversionRouteMappingIssuer.LegalAuthority,
                           legalAuthority)
                    ? _designConversionRouteMappingIssuer
                    : null;
            }

            return _designConversionRouteMappingIssuer =
                new ContinuousLearningDesignConversionRouteMappingCapability.Issuer(
                    this,
                    surfaceIdentity,
                    providerIdentity,
                    legalAuthority);
        }
    }

    internal bool TryBindDesignConversionTrainingIntakeRouteMapping(
        ContinuousLearningDesignConversionRouteMappingCapability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        lock (_readinessGate)
        {
            if (!TrainingActivationRequested
                || !_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || !capability.IsActive
                || !IsExactBoundaryCapability(
                    capability,
                    ExactRouteMappingCapabilityAssemblyName,
                    ExactRouteMappingCapabilityTypeName)
                || !ReferenceEquals(capability.Owner, this)
                || !ReferenceEquals(
                    capability.MappingIssuer,
                    _designConversionRouteMappingIssuer)
                || !ReferenceEquals(
                    capability.SurfaceIdentity,
                    capability.MappingIssuer.SurfaceIdentity)
                || !ReferenceEquals(
                    capability.ProviderIdentity,
                    capability.MappingIssuer.ProviderIdentity)
                || !ReferenceEquals(
                    capability.LegalAuthority,
                    _legalTeacherAdmissionAuthority)
                || _designConversionRouteMappingCapability is not null)
            {
                return false;
            }

            _designConversionRouteMappingCapability = capability;
            return true;
        }
    }

    internal ContinuousLearningDesignConversionTeacherAssociationCapability.Issuer?
        PrepareDesignConversionTeacherAssociationIssuer(object providerIdentity)
    {
        ArgumentNullException.ThrowIfNull(providerIdentity);
        lock (_readinessGate)
        {
            var routeCapability = _designConversionRouteMappingCapability;
            var bindingToken = _activeDpoPolicyBaseBindingToken;
            if (!TrainingActivationRequested
                || routeCapability is not { IsActive: true }
                || bindingToken is null
                || !ReferenceEquals(routeCapability.Owner, this)
                || !ReferenceEquals(
                    routeCapability.ProviderIdentity,
                    providerIdentity)
                || !bindingToken.IsGatewayCommittedBy(providerIdentity)
                || _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                || !_readiness.StableDpoExecutorConfigured
                || _designConversionTeacherAssociationCapability is not null)
            {
                return null;
            }

            if (_designConversionTeacherAssociationIssuer is not null)
            {
                return ReferenceEquals(
                           _designConversionTeacherAssociationIssuer.BindingToken,
                           bindingToken)
                       && ReferenceEquals(
                           _designConversionTeacherAssociationIssuer.ProviderIdentity,
                           providerIdentity)
                    ? _designConversionTeacherAssociationIssuer
                    : null;
            }

            return _designConversionTeacherAssociationIssuer =
                new ContinuousLearningDesignConversionTeacherAssociationCapability.Issuer(
                    this,
                    routeCapability,
                    bindingToken,
                    _dpoPolicyBaseLifecycleGeneration,
                    providerIdentity);
        }
    }

    /// <summary>
    /// Finalizes only an exact capability minted after provider/runtime
    /// registration. Raw provider, authority, or runtime references cannot
    /// activate the DesignConversion training-intake cycle.
    /// </summary>
    internal bool TryMarkDesignConversionTrainingIntakeTeacherRuntimeAssociated(
        ContinuousLearningDesignConversionTeacherAssociationCapability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        lock (_readinessGate)
        {
            var routeCapability = _designConversionRouteMappingCapability;
            var bindingToken = _activeDpoPolicyBaseBindingToken;
            if (!TrainingActivationRequested
                || !capability.IsActive
                || !IsExactBoundaryCapability(
                    capability,
                    ExactTeacherAssociationCapabilityAssemblyName,
                    ExactTeacherAssociationCapabilityTypeName)
                || routeCapability is not { IsActive: true }
                || bindingToken is null
                || !ReferenceEquals(capability.Owner, this)
                || !ReferenceEquals(
                    capability.AssociationIssuer,
                    _designConversionTeacherAssociationIssuer)
                || !ReferenceEquals(
                    capability.RouteCapability,
                    routeCapability)
                || !ReferenceEquals(capability.BindingToken, bindingToken)
                || capability.Generation != _dpoPolicyBaseLifecycleGeneration
                || !ReferenceEquals(
                    capability.ProviderIdentity,
                    routeCapability.ProviderIdentity)
                || !bindingToken.IsGatewayCommittedBy(
                    capability.ProviderIdentity)
                || !_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _materializedComposition is null
                || !_readiness.CompositionMaterialized
                || !_readiness.PathIdentityVerified
                || !_readiness.RetainedObjectIdentityBound
                || !_readiness.LegalEvidenceVerified
                || _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                || !_readiness.StableDpoExecutorConfigured
                || _designConversionTeacherAssociationCapability is not null)
            {
                return false;
            }

            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var activeReadiness = _readiness with
            {
                ConfigurationStatus =
                    "materialized_design_conversion_training_intake_active",
                ConfigurationErrorCode = "none",
                TrainingEnabled = true,
                CycleExecutionAvailable = true,
                PerformsTraining = true,
                ProductReady = false,
            };
            _designConversionTeacherAssociationCapability = capability;
            _readiness = activeReadiness;
            return true;
        }
    }

    /// <summary>
    /// Atomically withdraws the exact provider/surface capability when the
    /// hosted teacher runtime cannot complete its final association.
    /// The caller revokes the gateway only after this readiness withdrawal.
    /// </summary>
    internal ContinuousLearningDpoPolicyBaseWithdrawal
        TryBeginDesignConversionTrainingIntakeTeacherAssociationFailureWithdrawal(
            object providerIdentity,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalAuthority)
    {
        ArgumentNullException.ThrowIfNull(providerIdentity);
        ArgumentNullException.ThrowIfNull(legalAuthority);
        lock (_readinessGate)
        {
            if (_retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome
                        .AlreadyInactive,
                    Ticket: null);
            }
            if (!_readiness.ConfigurationAdmitted
                || _designConversionRouteMappingCapability is not
                    { IsActive: true } routeCapability
                || !ReferenceEquals(routeCapability.ProviderIdentity, providerIdentity)
                || !ReferenceEquals(
                    _legalTeacherAdmissionAuthority,
                    legalAuthority)
                || _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                || _activeDpoPolicyBaseBindingToken is not { } bindingToken
                || !ReferenceEquals(
                    _currentDpoPolicyBaseBindingToken,
                    bindingToken)
                || bindingToken.Generation != _dpoPolicyBaseLifecycleGeneration
                || !bindingToken.IsGatewayCommittedBy(providerIdentity))
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected,
                    Ticket: null);
            }

            var failedReadiness =
                CreateTeacherAssociationFailureReadiness();
            var ticket = new DpoPolicyBaseWithdrawalTicket(
                this,
                bindingToken);
            MoveToRetirementLocked(_materializedComposition);
            _pendingDpoPolicyBaseWithdrawalTicket = ticket;
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
            return new(
                ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Withdrawn,
                ticket);
        }
    }

    internal void MarkDesignConversionTrainingIntakeTeacherAssociationFailed(
        object providerIdentity)
    {
        ArgumentNullException.ThrowIfNull(providerIdentity);
        var withdrawal =
            TryBeginDesignConversionTrainingIntakeTeacherAssociationFailureWithdrawal(
                providerIdentity,
                _legalTeacherAdmissionAuthority);
        if (withdrawal is
            {
                Outcome:
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Withdrawn,
                Ticket: { } ticket,
            })
        {
            CompleteDpoPolicyBaseBindingFailureWithdrawal(ticket);
        }
    }

    internal void HandleDesignConversionTeacherAssociationCapabilityRevoked(
        ContinuousLearningDesignConversionTeacherAssociationCapability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        lock (_readinessGate)
        {
            if (!ReferenceEquals(
                    _designConversionTeacherAssociationCapability,
                    capability)
                || _dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound)
            {
                return;
            }
            var failedReadiness =
                CreateTeacherAssociationFailureReadiness();
            MoveToRetirementLocked(_materializedComposition);
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
        }
        StartRetirement();
    }

    internal void HandleDesignConversionRouteMappingCapabilityRevoked(
        ContinuousLearningDesignConversionRouteMappingCapability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        lock (_readinessGate)
        {
            if (!ReferenceEquals(_designConversionRouteMappingCapability, capability)
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost)
            {
                return;
            }
            var failedReadiness =
                CreateTeacherAssociationFailureReadiness();
            MoveToRetirementLocked(_materializedComposition);
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
        }
        StartRetirement();
    }

    private ContinuousLearningProductionReadinessStatus
        CreateTeacherAssociationFailureReadiness()
    {
        BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
        return _readiness with
        {
            ConfigurationStatus =
                "design_conversion_training_intake_teacher_association_failed_inactive",
            ConfigurationErrorCode =
                DesignConversionTrainingIntakeTeacherAssociationFailedErrorCode,
            LegalEvidenceVerified = false,
            CompositionMaterialized = false,
            PathIdentityVerified = false,
            RetainedObjectIdentityBound = false,
            StableDpoExecutorConfigured = false,
            TrainingEnabled = false,
            CycleExecutionAvailable = false,
            PerformsTraining = false,
            ProductReady = false,
        };
    }

    internal bool IsDpoPolicyBaseProviderBindingPublished(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        return ReferenceEquals(
                   Volatile.Read(ref _activeDpoPolicyBaseBindingToken),
                   bindingToken)
               && Volatile.Read(ref _readiness)
                   .StableDpoExecutorConfigured;
    }

    internal bool IsDpoPolicyBaseProviderBound(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        ArgumentNullException.ThrowIfNull(bindingToken);
        fileAccess.EnsureRetained();
        lock (_readinessGate)
        {
            if (!IsDpoPolicyBaseProviderBoundLocked(fileAccess, bindingToken))
                return false;
        }
        fileAccess.EnsureRetained();
        lock (_readinessGate)
            return IsDpoPolicyBaseProviderBoundLocked(fileAccess, bindingToken);
    }

    internal ContinuousLearningProductionReadinessStatus ObserveReadiness()
    {
        ServerOwnedContinuousLearningFileAccess? fileAccess;
        lock (_readinessGate)
            fileAccess = _fileAccess;

        if (fileAccess is not null)
        {
            try
            {
                fileAccess.EnsureRetained();
            }
            catch (ContinuousLearningPathIdentityException)
            {
                // The access object invokes the sticky callback outside its lock,
                // but an observer must synchronously publish the same sticky loss
                // if that callback has not completed yet.
                HandleRetainedObjectTrustLost();
            }
        }
        return Volatile.Read(ref _readiness);
    }

    internal void MarkRetainedFileAccessAcquisitionFailed(
        Exception? acquisitionFailure = null)
    {
        ContinuousLearningMaterializedTp1Composition? removedComposition;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed)
                return;
            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var failedReadiness = _readiness with
            {
                ConfigurationStatus = "retained_object_acquisition_failed_inactive",
                ConfigurationErrorCode =
                    acquisitionFailure is
                        ContinuousLearningMutationAdmissionDomainAlreadyOwnedException
                        ? MutationAdmissionDomainAlreadyOwnedErrorCode
                        : ContinuousLearningPathIdentityException.StableErrorCode,
                PathIdentityVerified = false,
                RetainedObjectIdentityBound = false,
                LegalEvidenceVerified = false,
                CompositionMaterialized = false,
                StableDpoExecutorConfigured = false,
                TrainingEnabled = false,
                CycleExecutionAvailable = false,
                PerformsTraining = false,
                ProductReady = false,
            };
            removedComposition = _materializedComposition;
            MoveToRetirementLocked(removedComposition);
            _retainedObjectTrustLost = true;
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost);
            _fileAccess = null;
            _readiness = failedReadiness;
        }
        StartRetirement();
    }

    /// <summary>
    /// Atomically withdraws materialized readiness when the required retained
    /// DesignConversion provider cannot be created or published.
    /// </summary>
    internal ContinuousLearningDpoPolicyBaseWithdrawal
        TryBeginDpoPolicyBaseBindingFailureWithdrawal(
            ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected,
                    Ticket: null);
            }
            if (_retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome
                        .AlreadyInactive,
                    Ticket: null);
            }
            if (_dpoPolicyBaseLifecycleState
                    != ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
                || !ReferenceEquals(
                    _activeDpoPolicyBaseBindingToken,
                    bindingToken)
                || bindingToken.Generation != _dpoPolicyBaseLifecycleGeneration)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected,
                    Ticket: null);
            }

            var failedReadiness = CreateDpoPolicyBaseBindingFailureReadiness();
            var ticket = new DpoPolicyBaseWithdrawalTicket(
                this,
                bindingToken);
            MoveToRetirementLocked(_materializedComposition);
            _pendingDpoPolicyBaseWithdrawalTicket = ticket;
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
            return new(
                ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Withdrawn,
                ticket);
        }
    }

    internal void CompleteDpoPolicyBaseBindingFailureWithdrawal(
        DpoPolicyBaseWithdrawalTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        lock (_readinessGate)
        {
            if (!ReferenceEquals(ticket.Owner, this)
                || !ReferenceEquals(
                    _pendingDpoPolicyBaseWithdrawalTicket,
                    ticket)
                || !ticket.TryComplete())
            {
                return;
            }
            _pendingDpoPolicyBaseWithdrawalTicket = null;
        }
        StartRetirement();
    }

    internal void CompletePendingDpoPolicyBaseWithdrawalForExactBinding(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        DpoPolicyBaseWithdrawalTicket? ticket;
        lock (_readinessGate)
        {
            ticket = _pendingDpoPolicyBaseWithdrawalTicket;
            if (ticket is null
                || !ReferenceEquals(ticket.BindingToken, bindingToken))
            {
                return;
            }
        }
        CompleteDpoPolicyBaseBindingFailureWithdrawal(ticket);
    }

    internal ContinuousLearningDpoPolicyBaseWithdrawal
        TryBeginDpoPolicyBaseBindingInvariantFailureWithdrawal(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        lock (_readinessGate)
        {
            if (_retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome
                        .AlreadyInactive,
                    Ticket: null);
            }
            if (!_readiness.ConfigurationAdmitted)
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected,
                    Ticket: null);
            }

            if (_dpoPolicyBaseLifecycleState
                == ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending)
            {
                if (!ReferenceEquals(
                        _pendingDpoPolicyBaseBindingToken,
                        bindingToken)
                    || bindingToken.Generation
                        != _dpoPolicyBaseLifecycleGeneration
                    || !bindingToken.IsGatewayCommitted)
                {
                    return new(
                        ContinuousLearningDpoPolicyBaseWithdrawalOutcome
                            .Rejected,
                        Ticket: null);
                }
            }
            else if (_dpoPolicyBaseLifecycleState
                     == ContinuousLearningDpoPolicyBaseLifecycleState
                         .ProviderBound)
            {
                var activeBindingToken = _activeDpoPolicyBaseBindingToken;
                if (!ReferenceEquals(
                        _currentDpoPolicyBaseBindingToken,
                        bindingToken)
                    || bindingToken.Generation
                        != _dpoPolicyBaseLifecycleGeneration
                    || !bindingToken.IsGatewayCommitted
                    || (activeBindingToken is not null
                        && !ReferenceEquals(activeBindingToken, bindingToken)))
                {
                    return new(
                        ContinuousLearningDpoPolicyBaseWithdrawalOutcome
                            .Rejected,
                        Ticket: null);
                }
            }
            else
            {
                return new(
                    ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected,
                    Ticket: null);
            }

            var failedReadiness = CreateDpoPolicyBaseBindingFailureReadiness();
            var ticket = new DpoPolicyBaseWithdrawalTicket(
                this,
                bindingToken);
            MoveToRetirementLocked(_materializedComposition);
            _pendingDpoPolicyBaseWithdrawalTicket = ticket;
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
            return new(
                ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Withdrawn,
                ticket);
        }
    }

    internal void MarkDpoPolicyBaseBindingFailed(
        ContinuousLearningDpoPolicyBaseBindingToken? bindingToken = null)
    {
        if (bindingToken is not null)
        {
            var pendingBindingFailed = false;
            lock (_readinessGate)
            {
                if (_readiness.ConfigurationAdmitted
                    && !_retainedObjectTrustLost
                    && _dpoPolicyBaseLifecycleState
                        == ContinuousLearningDpoPolicyBaseLifecycleState
                            .BindingPending
                    && ReferenceEquals(
                        _pendingDpoPolicyBaseBindingToken,
                        bindingToken)
                    && bindingToken.Generation
                        == _dpoPolicyBaseLifecycleGeneration
                    && bindingToken.IsGatewayCommitted)
                {
                    var failedReadiness =
                        CreateDpoPolicyBaseBindingFailureReadiness();
                    MoveToRetirementLocked(_materializedComposition);
                    InvalidateDpoPolicyBaseLifecycleLocked(
                        ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
                    _readiness = failedReadiness;
                    pendingBindingFailed = true;
                }
            }
            if (pendingBindingFailed)
            {
                StartRetirement();
                return;
            }

            var withdrawal = TryBeginDpoPolicyBaseBindingFailureWithdrawal(
                bindingToken);
            if (withdrawal is
                {
                    Outcome:
                        ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Withdrawn,
                    Ticket: { } ticket,
                })
            {
                CompleteDpoPolicyBaseBindingFailureWithdrawal(ticket);
            }
            return;
        }

        ContinuousLearningMaterializedTp1Composition? removedComposition;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost
                    or ContinuousLearningDpoPolicyBaseLifecycleState.Failed)
            {
                return;
            }
            if (_dpoPolicyBaseLifecycleState
                != ContinuousLearningDpoPolicyBaseLifecycleState.BindingPending)
            {
                return;
            }
            var failedReadiness =
                CreateDpoPolicyBaseBindingFailureReadiness();
            removedComposition = _materializedComposition;
            MoveToRetirementLocked(removedComposition);
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
        }
        StartRetirement();
    }

    private ContinuousLearningProductionReadinessStatus
        CreateDpoPolicyBaseBindingFailureReadiness()
    {
        BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
        return _readiness with
        {
            ConfigurationStatus = "dpo_policy_base_binding_failed_inactive",
            ConfigurationErrorCode = DpoPolicyBaseBindingFailedErrorCode,
            LegalEvidenceVerified = false,
            CompositionMaterialized = false,
            PathIdentityVerified = false,
            RetainedObjectIdentityBound = false,
            StableDpoExecutorConfigured = false,
            TrainingEnabled = false,
            CycleExecutionAvailable = false,
            PerformsTraining = false,
            ProductReady = false,
        };
    }

    internal void MarkDpoPolicyBaseStopped()
    {
        ContinuousLearningMaterializedTp1Composition? removedComposition;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    == ContinuousLearningDpoPolicyBaseLifecycleState.Stopped)
                return;
            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var stoppedReadiness = _readiness with
            {
                ConfigurationStatus = "dpo_policy_base_stopped_inactive",
                ConfigurationErrorCode = DpoPolicyBaseStoppedErrorCode,
                LegalEvidenceVerified = false,
                CompositionMaterialized = false,
                PathIdentityVerified = false,
                RetainedObjectIdentityBound = false,
                StableDpoExecutorConfigured = false,
                TrainingEnabled = false,
                CycleExecutionAvailable = false,
                PerformsTraining = false,
                ProductReady = false,
            };
            var pendingWithdrawal =
                _pendingDpoPolicyBaseWithdrawalTicket is not null;
            removedComposition = _materializedComposition;
            if (pendingWithdrawal)
            {
                _dpoPolicyBaseLifecycleState =
                    ContinuousLearningDpoPolicyBaseLifecycleState.Stopped;
            }
            else
            {
                MoveToRetirementLocked(removedComposition);
                InvalidateDpoPolicyBaseLifecycleLocked(
                    ContinuousLearningDpoPolicyBaseLifecycleState.Stopped);
            }
            _readiness = stoppedReadiness;
        }
        StartRetirement();
    }

    private void HandleRetainedObjectTrustLost()
    {
        ContinuousLearningMaterializedTp1Composition? removedComposition;
        ContinuousLearningDpoPolicyBaseBindingToken? bindingToken = null;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    is ContinuousLearningDpoPolicyBaseLifecycleState.Stopped
                    or ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost)
                return;
            ContinuousLearningProductionReadinessStatus? failedReadiness = null;
            if (!_retainedObjectTrustLost)
            {
                // Если создание артефактов уже не удалось, последующая потеря
                // удержанного пути важнее: оператор должен видеть, что теперь
                // нельзя доверять самим файлам, а не только сборке модели.
                BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
                failedReadiness = _readiness with
                {
                    ConfigurationStatus =
                        "retained_object_identity_lost_inactive",
                    ConfigurationErrorCode = RetainedObjectIdentityLostErrorCode,
                    PathIdentityVerified = false,
                    RetainedObjectIdentityBound = false,
                    LegalEvidenceVerified = false,
                    CompositionMaterialized = false,
                    StableDpoExecutorConfigured = false,
                    TrainingEnabled = false,
                    CycleExecutionAvailable = false,
                    PerformsTraining = false,
                    ProductReady = false,
                };
                bindingToken = _activeDpoPolicyBaseBindingToken;
            }
            removedComposition = _materializedComposition;
            MoveToRetirementLocked(removedComposition);
            if (failedReadiness is not null)
            {
                _retainedObjectTrustLost = true;
                InvalidateDpoPolicyBaseLifecycleLocked(
                    ContinuousLearningDpoPolicyBaseLifecycleState.TrustLost);
                _readiness = failedReadiness;
            }
        }
        StartRetirement();
        bindingToken?.RevokeAfterTrustLoss();
    }

    private void MarkMaterializationFailed()
    {
        ContinuousLearningMaterializedTp1Composition? removedComposition;
        lock (_readinessGate)
        {
            if (!_readiness.ConfigurationAdmitted
                || _retainedObjectTrustLost
                || _dpoPolicyBaseLifecycleState
                    == ContinuousLearningDpoPolicyBaseLifecycleState.Stopped)
                return;
            BeforeReadinessSnapshotAllocationObserver.Value?.Invoke();
            var failedReadiness = _readiness with
            {
                ConfigurationStatus = "materialization_failed_inactive",
                ConfigurationErrorCode = MaterializationFailedErrorCode,
                LegalEvidenceVerified = false,
                CompositionMaterialized = false,
                PathIdentityVerified = false,
                RetainedObjectIdentityBound = false,
                StableDpoExecutorConfigured = false,
                TrainingEnabled = false,
                CycleExecutionAvailable = false,
                PerformsTraining = false,
                ProductReady = false,
            };
            removedComposition = _materializedComposition;
            MoveToRetirementLocked(removedComposition);
            InvalidateDpoPolicyBaseLifecycleLocked(
                ContinuousLearningDpoPolicyBaseLifecycleState.Failed);
            _readiness = failedReadiness;
        }
        StartRetirement();
    }

    internal Task AwaitRetirementAsync()
    {
        lock (_readinessGate)
            return _retirementTask;
    }

    internal bool TryRegisterRetirementDependency(Func<Task> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        lock (_readinessGate)
        {
            if (_retirementStarted
                || _pendingRetirementStart is not null
                || _retirementDependencies.Count >= 16)
                return false;
            _retirementDependencies.Add(dependency);
            return true;
        }
    }

    private void MoveToRetirementLocked(
        ContinuousLearningMaterializedTp1Composition? composition)
    {
        if (composition is null || _retiringComposition is not null)
            return;
        var start = PrepareRetirementStartLocked(composition);
        _pendingRetirementCompletion = start.Pending;
        _retirementTask = start.Pending.Task;
        _pendingRetirementStart = start;
        _materializedComposition = null;
        _retiringComposition = composition;
    }

    private RetirementStartPlan PrepareRetirementStartLocked(
        ContinuousLearningMaterializedTp1Composition? composition)
    {
        var pending = _pendingRetirementCompletion;
        if (pending is null)
        {
            BeforeRetirementPendingCompletionAllocationObserver.Value?.Invoke();
            pending = new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }
        BeforeRetirementDependencySnapshotObserver.Value?.Invoke();
        var dependencies = _retirementDependencies.ToArray();
        BeforeRetirementFailureStateAllocationObserver.Value?.Invoke();
        var failures = new ContinuousLearningRetirementFailureArbiter();
        var dependencyDrains = new Task?[dependencies.Length];
        var dependencyFailures = new Exception?[dependencies.Length];
        var startGate = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new RetirementStartPlan(
            pending,
            composition,
            dependencies,
            dependencyDrains,
            dependencyFailures,
            failures,
            startGate);
        BeforeRetirementCompletionLaunchObserver.Value?.Invoke();
        _ = CompleteRetirementAsync(start);
        return start;
    }

    private void StartRetirement()
    {
        RetirementStartPlan start;
        lock (_readinessGate)
        {
            if (_retirementStarted
                || _pendingDpoPolicyBaseWithdrawalTicket is not null)
                return;
            start = _pendingRetirementStart
                ?? PrepareRetirementStartLocked(_retiringComposition);
            _retirementStarted = true;
            _pendingRetirementStart = null;
            _pendingRetirementCompletion = null;
            _retirementTask = start.Pending.Task;
        }
        try
        {
            RetirementTaskPublishedObserver.Value?.Invoke(start.Pending.Task);
        }
        catch (Exception exception)
        {
            start.Failures.CaptureOriginal(exception);
        }
        try
        {
            start.LegalTeacherDrain =
                _legalTeacherAdmissionAuthority.BeginRevocation() ??
                throw new InvalidOperationException(
                    "Legal teacher revocation returned no drain task.");
        }
        catch (Exception exception)
        {
            start.Failures.CaptureCleanup(exception);
        }
        for (var index = 0; index < start.Dependencies.Length; index++)
        {
            try
            {
                start.DependencyDrains[index] = start.Dependencies[index]() ??
                    throw new InvalidOperationException(
                        "A retirement dependency returned no drain task.");
            }
            catch (Exception exception)
            {
                start.DependencyFailures[index] = exception;
            }
        }
        start.StartGate.TrySetResult(null);
    }

    private async Task CompleteRetirementAsync(RetirementStartPlan start)
    {
        try
        {
            await start.StartGate.Task.ConfigureAwait(false);
            if (start.LegalTeacherDrain is not null)
            {
                try { await start.LegalTeacherDrain.ConfigureAwait(false); }
                catch (Exception exception)
                {
                    start.Failures.CaptureCleanup(exception);
                }
            }
            for (var index = 0; index < start.DependencyDrains.Length; index++)
            {
                if (start.DependencyFailures[index] is { } synchronousFailure)
                {
                    start.Failures.CaptureCleanup(synchronousFailure);
                    continue;
                }
                if (start.DependencyDrains[index] is not { } dependencyDrain)
                    continue;
                try { await dependencyDrain.ConfigureAwait(false); }
                catch (Exception exception)
                {
                    start.Failures.CaptureCleanup(exception);
                }
            }
            try { start.Composition?.Dispose(); }
            catch (Exception exception)
            {
                start.Failures.CaptureCleanup(exception);
            }
        }
        finally
        {
            lock (_readinessGate)
            {
                if (start.Composition is not null &&
                    ReferenceEquals(_retiringComposition, start.Composition))
                    _retiringComposition = null;
            }
            var failure = start.Failures.Resolve();
            if (failure is null)
                start.Pending.TrySetResult(null);
            else
                start.Pending.TrySetException(failure);
        }
    }

    private sealed class RetirementStartPlan(
        TaskCompletionSource<object?> pending,
        ContinuousLearningMaterializedTp1Composition? composition,
        Func<Task>[] dependencies,
        Task?[] dependencyDrains,
        Exception?[] dependencyFailures,
        ContinuousLearningRetirementFailureArbiter failures,
        TaskCompletionSource<object?> startGate)
    {
        internal TaskCompletionSource<object?> Pending { get; } = pending;
        internal ContinuousLearningMaterializedTp1Composition? Composition
        {
            get;
        } = composition;
        internal Func<Task>[] Dependencies { get; } = dependencies;
        internal Task?[] DependencyDrains { get; } = dependencyDrains;
        internal Exception?[] DependencyFailures { get; } = dependencyFailures;
        internal ContinuousLearningRetirementFailureArbiter Failures
        {
            get;
        } = failures;
        internal TaskCompletionSource<object?> StartGate { get; } = startGate;
        internal Task? LegalTeacherDrain { get; set; }
    }

    private void EnsureDpoPolicyBaseBindingStateLocked(
        ServerOwnedContinuousLearningFileAccess fileAccess)
    {
        if (!_readiness.ConfigurationAdmitted
            || _retainedObjectTrustLost
            || !ReferenceEquals(_fileAccess, fileAccess)
            || _deferredPlan is null
            || _materializedComposition is null
            || !_readiness.CompositionMaterialized
            || !_readiness.PathIdentityVerified
            || !_readiness.RetainedObjectIdentityBound)
        {
            throw new ContinuousLearningPathIdentityException();
        }
    }

    private bool IsDpoPolicyBaseProviderBoundLocked(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
        => !_retainedObjectTrustLost
           && ReferenceEquals(_fileAccess, fileAccess)
           && _materializedComposition is not null
           && _dpoPolicyBaseLifecycleState
               == ContinuousLearningDpoPolicyBaseLifecycleState.ProviderBound
           && ReferenceEquals(_activeDpoPolicyBaseBindingToken, bindingToken)
           && bindingToken.Generation == _dpoPolicyBaseLifecycleGeneration
           && _readiness.CompositionMaterialized
           && _readiness.PathIdentityVerified
           && _readiness.RetainedObjectIdentityBound
           && _readiness.StableDpoExecutorConfigured;

    private void InvalidateDpoPolicyBaseLifecycleLocked(
        ContinuousLearningDpoPolicyBaseLifecycleState state)
    {
        _designConversionTeacherAssociationCapability?
            .RevokeWithoutOwnerCallback();
        _designConversionRouteMappingCapability?
            .RevokeWithoutOwnerCallback();
        _designConversionTeacherAssociationCapability = null;
        _designConversionTeacherAssociationIssuer = null;
        _designConversionRouteMappingCapability = null;
        _designConversionRouteMappingIssuer = null;
        _pendingDpoPolicyBaseBindingToken = null;
        _currentDpoPolicyBaseBindingToken = null;
        Volatile.Write(ref _activeDpoPolicyBaseBindingToken, null);
        _dpoPolicyBaseLifecycleGeneration = unchecked(
            _dpoPolicyBaseLifecycleGeneration + 1);
        _dpoPolicyBaseLifecycleState = state;
    }

    private static bool IsExactBoundaryCapability(
        object capability,
        string assemblyName,
        string fullTypeName)
    {
        var capabilityType = capability.GetType();
        return string.Equals(
                   capabilityType.Assembly.GetName().Name,
                   assemblyName,
                   StringComparison.Ordinal)
               && string.Equals(
                   capabilityType.FullName,
                   fullTypeName,
                   StringComparison.Ordinal);
    }

    internal static ContinuousLearningModelServerStartup Create(
        Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);

        var oneLessonGrantCatalog =
            CodeDocumentationOneLessonGrantCatalog.Load(readEnvironment);

        var resolution = ContinuousLearningModelServerConfigurationLoader.Resolve(
            readEnvironment);
        return resolution.State switch
        {
            ContinuousLearningConfigurationState.Disabled => new(
                deferredPlan: null,
                ContinuousLearningProductionReadinessStatus.Disabled(),
                oneLessonGrantCatalog),
            ContinuousLearningConfigurationState.Enabled
                when resolution.DeferredPlan is not null => new(
                    resolution.DeferredPlan,
                    ContinuousLearningProductionReadinessStatus.ConfiguredUnverifiedInactive(
                        resolution.DeferredPlan.TeacherConfiguration.LocalRetentionPolicyRef,
                        resolution.DeferredPlan.TeacherConfiguration.OutputLicenseRef),
                    oneLessonGrantCatalog),
            _ => new(
                deferredPlan: null,
                ContinuousLearningProductionReadinessStatus.Invalid(
                    resolution.ErrorCode),
                oneLessonGrantCatalog),
        };
    }
}

internal enum ContinuousLearningConfigurationState
{
    Disabled,
    Invalid,
    Enabled,
}

internal sealed record ContinuousLearningConfigurationResolution(
    ContinuousLearningConfigurationState State,
    ContinuousLearningDeferredCompositionPlan? DeferredPlan,
    string ErrorCode);

/// <summary>
/// Server-owned immutable configuration admission. Physical path identities,
/// registry contents, checkpoint formats, and executable dependencies remain
/// deliberately unbound until a separate authorized materialization slice.
/// </summary>
internal sealed class ContinuousLearningDeferredCompositionPlan
{
    private ContinuousLearningConfiguredPathBundle? _configuredPaths;
    private readonly ModelConfig _modelConfig;
    private readonly TrainingConfig _trainingConfig;

    internal ContinuousLearningDeferredCompositionPlan(
        ContinuousLearningTp1DpoConfiguration teacherConfiguration,
        ContinuousLearningBaseCheckpointModelIdentity
            baseCheckpointModelIdentity,
        string baseCheckpointPath,
        string tokenizerPath,
        string adapterRoot,
        string cacheRoot,
        string registryRoot,
        RepositoryOwnedDpoArtifactRoot dpoArtifactRoot,
        string dpoPairRoot,
        RepositoryOwnedIncrementalTrainingSessionStartRequest
            repositoryStartRequest,
        ModelConfig modelConfig,
        TrainingConfig trainingConfig,
        bool trainingActivationRequested,
        bool localIndependentPreferenceEvaluatorRequested, // Preserve the existing evaluator opt-in independently of DPO storage authority.
        ContinuousLearningDpoPairRootAccess dpoPairRootAccess = ContinuousLearningDpoPairRootAccess.MutablePairStore) // Direct legacy plans retain their mutable-store default.
    {
        TeacherConfiguration = teacherConfiguration;
        BaseCheckpointModelIdentity = baseCheckpointModelIdentity
            ?? throw new ArgumentNullException(nameof(baseCheckpointModelIdentity));
        _configuredPaths = new ContinuousLearningConfiguredPathBundle(
            baseCheckpointPath,
            tokenizerPath,
            adapterRoot,
            cacheRoot,
            registryRoot,
            dpoPairRoot, // Preserve the canonical dataset selector rather than redirecting it to an empty writable root.
            dpoPairRootAccess); // Carry source/store authority with the configured path to its retained owner.
        RepositoryDpoArtifactRoot = dpoArtifactRoot
            ?? throw new ArgumentNullException(nameof(dpoArtifactRoot));
        RepositoryStartRequest = repositoryStartRequest
            ?? throw new ArgumentNullException(nameof(repositoryStartRequest));
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(trainingConfig);
        _modelConfig = CopyModelConfig(modelConfig);
        _trainingConfig = CopyTrainingConfig(trainingConfig);
        RootInitializationSeed = _trainingConfig.RootInitializationSeed;
        TrainingConfigurationFingerprint =
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                _trainingConfig);
        TrainingActivationRequested = trainingActivationRequested;
        LocalIndependentPreferenceEvaluatorRequested =
            localIndependentPreferenceEvaluatorRequested;
    }

    internal ContinuousLearningTp1DpoConfiguration TeacherConfiguration { get; }
    internal ContinuousLearningBaseCheckpointModelIdentity
        BaseCheckpointModelIdentity { get; }
    internal ModelConfig ModelConfig => CopyModelConfig(_modelConfig);
    internal TrainingConfig TrainingConfig => CopyTrainingConfig(_trainingConfig);
    internal RepositoryOwnedDpoArtifactRoot RepositoryDpoArtifactRoot { get; }
    internal RepositoryOwnedIncrementalTrainingSessionStartRequest
        RepositoryStartRequest { get; }
    internal ulong? RootInitializationSeed { get; }
    internal string TrainingConfigurationFingerprint { get; }
    internal bool TrainingActivationRequested { get; }
    internal bool LocalIndependentPreferenceEvaluatorRequested { get; }

    internal OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>
        ResolveRepositoryStartRequest(string domain)
    {
        if (!StringComparer.Ordinal.Equals(domain, RepositoryStartRequest.DomainSlug))
        {
            return OperationResult<
                RepositoryOwnedIncrementalTrainingSessionStartRequest>.Fail(
                    "No repository-owned training start is configured for this domain.",
                    RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode);
        }

        return OperationResult<
            RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                RepositoryStartRequest);
    }

    internal ServerOwnedContinuousLearningFileAccess AcquireFileAccess(
        ServerOwnedContinuousLearningFileAccessOptions? options,
        Action onRetainedObjectTrustLost)
    {
        ArgumentNullException.ThrowIfNull(onRetainedObjectTrustLost);
        var paths = Interlocked.Exchange(ref _configuredPaths, null)
            ?? throw new ContinuousLearningPathIdentityException();
        return ServerOwnedContinuousLearningFileAccess.Acquire(
            paths.BaseCheckpoint,
            paths.Tokenizer,
            paths.AdapterRoot,
            paths.CacheRoot,
            paths.RegistryRoot,
            paths.DpoPairRoot,
            options,
            onRetainedObjectTrustLost, // Preserve the existing fail-closed startup trust callback.
            paths.DpoPairRootAccess); // Testing options cannot upgrade a sealed source to a mutable pair-store role.
    }

    private sealed class ContinuousLearningConfiguredPathBundle(
        string baseCheckpoint,
        string tokenizer,
        string adapterRoot,
        string cacheRoot,
        string registryRoot,
        string dpoPairRoot, // Keep the canonical source/store path with its typed access role.
        ContinuousLearningDpoPairRootAccess dpoPairRootAccess) // Consume the typed role together with the one-shot configured path bundle.
    {
        internal readonly string BaseCheckpoint = baseCheckpoint;
        internal readonly string Tokenizer = tokenizer;
        internal readonly string AdapterRoot = adapterRoot;
        internal readonly string CacheRoot = cacheRoot;
        internal readonly string RegistryRoot = registryRoot;
        internal readonly string DpoPairRoot = dpoPairRoot;
        internal readonly ContinuousLearningDpoPairRootAccess DpoPairRootAccess = dpoPairRootAccess; // Keep the admitted source role immutable until acquisition.
    }

    private static ModelConfig CopyModelConfig(ModelConfig modelConfig)
        => new()
        {
            VocabSize = modelConfig.VocabSize,
            ContextLen = modelConfig.ContextLen,
            HiddenDim = modelConfig.HiddenDim,
            NumLayers = modelConfig.NumLayers,
            NumHeads = modelConfig.NumHeads,
            NumKvHeads = modelConfig.NumKvHeads,
            FfnHiddenDim = modelConfig.FfnHiddenDim,
            RopeTheta = modelConfig.RopeTheta,
            RmsNormEps = modelConfig.RmsNormEps,
        };

    private static TrainingConfig CopyTrainingConfig(TrainingConfig trainingConfig)
        => new()
        {
            BatchSize = trainingConfig.BatchSize,
            SeqLen = trainingConfig.SeqLen,
            MaxLr = trainingConfig.MaxLr,
            MinLr = trainingConfig.MinLr,
            WarmupSteps = trainingConfig.WarmupSteps,
            TotalSteps = trainingConfig.TotalSteps,
            GradAccumSteps = trainingConfig.GradAccumSteps,
            GradClipNorm = trainingConfig.GradClipNorm,
            WeightDecay = trainingConfig.WeightDecay,
            CheckpointEvery = trainingConfig.CheckpointEvery,
            CheckpointDir = trainingConfig.CheckpointDir,
            LogEvery = trainingConfig.LogEvery,
            RootInitializationSeed = trainingConfig.RootInitializationSeed,
        };
}

/// <summary>
/// Bounded server-owned environment loader. It performs configuration parsing
/// and current metadata checks only. It never creates a filesystem entry,
/// opens artifact content, loads a registry, or materializes a graph.
/// </summary>
internal static class ContinuousLearningModelServerConfigurationLoader
{
    internal const string EnabledEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED";
    internal const string TrainingActivationEnabledEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_ACTIVATION_ENABLED";
    internal const string TrainingRootInitializationSeedEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_ROOT_INITIALIZATION_SEED";
    internal const string LocalIndependentPreferenceEvaluatorEnabledEnvironmentName =
        "XIIGEN_DESIGN_CONVERSION_LOCAL_INDEPENDENT_PREFERENCE_EVALUATOR_ENABLED";
    internal const string LocalIndependentPreferenceEvaluatorRequiresCompositionErrorCode =
        "DESIGN_CONVERSION_LOCAL_INDEPENDENT_PREFERENCE_EVALUATOR_REQUIRES_CONTINUOUS_LEARNING_CONFIGURATION";
    internal const string ModelIdEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_ID";
    internal const string AuthoringProfileIdEnvironmentName = "XIIGEN_CONTINUOUS_LEARNING_TP1_AUTHORING_PROFILE_ID"; // Select the separate authoring contract only through process-owned startup input.
    internal const string AuthoringProfileSha256EnvironmentName = "XIIGEN_CONTINUOUS_LEARNING_TP1_AUTHORING_PROFILE_SHA256"; // Require the exact independently hashed profile beside its ID.
    internal const string EvaluationProfileIdEnvironmentName = "XIIGEN_CONTINUOUS_LEARNING_TP1_EVALUATION_PROFILE_ID";
    internal const string EvaluationProfileSha256EnvironmentName = "XIIGEN_CONTINUOUS_LEARNING_TP1_EVALUATION_PROFILE_SHA256";
    internal const string OllamaManifestDigestEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MANIFEST_DIGEST";
    internal const string OllamaModelLayerDigestEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MODEL_LAYER_DIGEST";
    internal const string BaseModelIdEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_BASE_MODEL_ID";
    internal const string BaseModelRevisionEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_BASE_MODEL_REVISION";
    internal const string MaxTeacherCallsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS";
    internal const string DpoMaxStepCountEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_DPO_MAX_STEPS";
    internal const string AuditSaltEnvironmentName =
        ServerOwnedLogicalCapabilityReplayVerificationComposition
            .HmacKeyEnvironmentName;
    internal const string LocalRetentionPolicyRefEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TP1_LOCAL_RETENTION_POLICY_REF";
    internal const string OutputLicenseRefEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TP1_OUTPUT_LICENSE_REF";
    internal const string BaseCheckpointEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_BASE_CHECKPOINT";
    internal const string TokenizerEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TOKENIZER";
    internal const string AdapterRootEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_ADAPTER_ROOT";
    internal const string CacheRootEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_CACHE_ROOT";
    internal const string RegistryRootEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_REGISTRY_ROOT";
    internal const string DpoPairRootEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_DPO_PAIR_ROOT";
    internal const string RepositoryRootEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_ROOT";
    internal const string RepositoryModelIdEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_MODEL_ID";
    internal const string RepositoryModelVersionEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_REPOSITORY_MODEL_VERSION";
    internal const string RuntimeConfigSha256EnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_CONFIG_SHA256";
    internal const string RuntimeTopologySha256EnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_TOPOLOGY_SHA256";
    internal const string RuntimeTokenizerSha256EnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_TOKENIZER_SHA256";
    internal const string RuntimeTrainerSha256EnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_TRAINER_SHA256";
    internal const string RuntimeSchemaSha256EnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_SCHEMA_SHA256";
    internal const string RuntimeCompatibilityKeyEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_RUNTIME_COMPATIBILITY_KEY";
    internal const string ModelVocabSizeEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_VOCAB_SIZE";
    internal const string ModelContextLenEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_CONTEXT_LEN";
    internal const string ModelHiddenDimEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_HIDDEN_DIM";
    internal const string ModelNumLayersEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_LAYERS";
    internal const string ModelNumHeadsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_HEADS";
    internal const string ModelNumKvHeadsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_NUM_KV_HEADS";
    internal const string ModelFfnHiddenDimEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_FFN_HIDDEN_DIM";
    internal const string ModelRopeThetaEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_ROPE_THETA";
    internal const string ModelRmsNormEpsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_MODEL_RMS_NORM_EPS";
    internal const string TrainingBatchSizeEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_BATCH_SIZE";
    internal const string TrainingSeqLenEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_SEQ_LEN";
    internal const string TrainingMaxLrEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_MAX_LR";
    internal const string TrainingMinLrEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_MIN_LR";
    internal const string TrainingWarmupStepsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_WARMUP_STEPS";
    internal const string TrainingTotalStepsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_TOTAL_STEPS";
    internal const string TrainingGradAccumStepsEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_GRAD_ACCUM_STEPS";
    internal const string TrainingGradClipNormEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_GRAD_CLIP_NORM";
    internal const string TrainingWeightDecayEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_WEIGHT_DECAY";
    internal const string TrainingCheckpointEveryEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_CHECKPOINT_EVERY";
    internal const string TrainingLogEveryEnvironmentName =
        "XIIGEN_CONTINUOUS_LEARNING_TRAINING_LOG_EVERY";

    private const int AuditSaltBytes = 32;
    private const int MaximumEnvironmentValueLength = 32_768;
    internal const long MaximumBaseCheckpointBytes = 16L * 1024 * 1024 * 1024;
    internal const long MaximumTokenizerBytes = Tokenizer.BpeTokenizer.MaximumCanonicalArtifactBytes;

    public static ContinuousLearningConfigurationResolution Resolve(
        Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);

        string? enabled;
        string? trainingActivationEnabled;
        string? localIndependentPreferenceEvaluatorEnabled;
        try
        {
            enabled = ReadBounded(readEnvironment, EnabledEnvironmentName);
            trainingActivationEnabled = ReadBounded(
                readEnvironment,
                TrainingActivationEnabledEnvironmentName);
            localIndependentPreferenceEvaluatorEnabled = ReadBounded(
                readEnvironment,
                LocalIndependentPreferenceEvaluatorEnabledEnvironmentName);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
        }

        var trainingActivationRequested = false;
        if (trainingActivationEnabled is not null)
        {
            if (string.Equals(
                    trainingActivationEnabled,
                    bool.TrueString,
                    StringComparison.OrdinalIgnoreCase))
            {
                trainingActivationRequested = true;
            }
            else if (!string.Equals(
                         trainingActivationEnabled,
                         bool.FalseString,
                         StringComparison.OrdinalIgnoreCase))
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }
        }

        var localIndependentPreferenceEvaluatorRequested = false;
        if (localIndependentPreferenceEvaluatorEnabled is not null)
        {
            if (string.Equals(
                    localIndependentPreferenceEvaluatorEnabled,
                    bool.TrueString,
                    StringComparison.OrdinalIgnoreCase))
            {
                localIndependentPreferenceEvaluatorRequested = true;
            }
            else if (!string.Equals(
                         localIndependentPreferenceEvaluatorEnabled,
                         bool.FalseString,
                         StringComparison.OrdinalIgnoreCase))
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }
        }

        if (enabled is null
            || string.Equals(enabled, bool.FalseString, StringComparison.OrdinalIgnoreCase))
        {
            if (trainingActivationRequested)
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            if (localIndependentPreferenceEvaluatorRequested)
            {
                return Invalid(
                    LocalIndependentPreferenceEvaluatorRequiresCompositionErrorCode);
            }

            return new ContinuousLearningConfigurationResolution(
                ContinuousLearningConfigurationState.Disabled,
                DeferredPlan: null,
                ErrorCode: "CONTINUOUS_LEARNING_DISABLED");
        }

        if (!string.Equals(enabled, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");

        try
        {
            var localRetentionPolicyRef = ReadRequired(
                readEnvironment,
                LocalRetentionPolicyRefEnvironmentName);
            var outputLicenseRef = ReadRequired(
                readEnvironment,
                OutputLicenseRefEnvironmentName);
            if (!LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(
                    localRetentionPolicyRef)
                || !LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(
                    outputLicenseRef))
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }

            var modelId = ReadRequired(readEnvironment, ModelIdEnvironmentName);
            var authoringProfile = ReadAuthoringProfile(readEnvironment); // Capture and validate both profile fields once before resolving production artifacts.
            if (!authoringProfile.IsSuccess) return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID"); // Unknown, incomplete or forged selectors never create a teacher client.
            var evaluationProfile = ReadEvaluationProfile(readEnvironment);
            if (!evaluationProfile.IsSuccess) return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            var expectedOllamaManifestDigest = ReadRequired(
                readEnvironment,
                OllamaManifestDigestEnvironmentName);
            var expectedOllamaModelLayerDigest = ReadRequired(
                readEnvironment,
                OllamaModelLayerDigestEnvironmentName);
            var baseModelId = ReadRequired(
                readEnvironment,
                BaseModelIdEnvironmentName);
            var baseModelRevision = ReadRequired(
                readEnvironment,
                BaseModelRevisionEnvironmentName);
            if (!IsLogicalModelIdentity(
                    baseModelId,
                    maximumLength: 256,
                    allowSlash: true)
                || !IsLogicalModelIdentity(
                    baseModelRevision,
                    maximumLength: 160,
                    allowSlash: false))
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }
            var maxCallsText = ReadRequired(
                readEnvironment,
                MaxTeacherCallsEnvironmentName);
            var dpoMaxStepCount = ReadOptionalDpoMaxStepCount(readEnvironment);
            if (!dpoMaxStepCount.IsSuccess)
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");

            var saltText = ReadRequired(readEnvironment, AuditSaltEnvironmentName);
            var baseCheckpoint = RequireCanonicalRegularFileMetadata(
                ReadRequired(readEnvironment, BaseCheckpointEnvironmentName),
                MaximumBaseCheckpointBytes);
            var tokenizer = RequireCanonicalRegularFileMetadata(
                ReadRequired(readEnvironment, TokenizerEnvironmentName),
                MaximumTokenizerBytes);
            var adapterRoot = RequireCanonicalDirectoryRootMetadata(
                ReadRequired(readEnvironment, AdapterRootEnvironmentName));
            var cacheRoot = RequireCanonicalDirectoryRootMetadata(
                ReadRequired(readEnvironment, CacheRootEnvironmentName));
            var registryRoot = RequireCanonicalDirectoryRootMetadata(
                ReadRequired(readEnvironment, RegistryRootEnvironmentName));
            var repositoryDpoRoot =
                RepositoryOwnedTrainingArtifactLayout.ResolveDpoArtifactRoot(
                    ReadRequired(readEnvironment, RepositoryRootEnvironmentName));
            if (!repositoryDpoRoot.IsSuccess || repositoryDpoRoot.Value is null)
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            var pairRoot = RequireCanonicalDirectoryRootMetadata(
                ReadRequired(readEnvironment, DpoPairRootEnvironmentName));
            var dpoDatasetAddress =
                RepositoryOwnedTrainingArtifactLayout
                    .ResolveDpoDatasetAddressFromPairStoreRoot(
                        repositoryDpoRoot.Value,
                        pairRoot);
            if (!dpoDatasetAddress.IsSuccess
                || dpoDatasetAddress.Value is null)
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }
            var currentDpoDatasetManifest =
                TryReadCurrentRepositoryOwnedDpoDatasetManifestIdentity(
                    dpoDatasetAddress.Value);
            var repositoryModelId = ReadRequired(
                readEnvironment,
                RepositoryModelIdEnvironmentName);
            var repositoryModelVersion = ReadRequired(
                readEnvironment,
                RepositoryModelVersionEnvironmentName);
            var runtimeCompatibility =
                ReadRepositoryRuntimeCompatibility(readEnvironment);
            if (runtimeCompatibility is null)
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            var repositoryStartRequest = CreateRepositoryStartRequest(
                dpoDatasetAddress.Value,
                currentDpoDatasetManifest,
                repositoryModelId,
                repositoryModelVersion,
                runtimeCompatibility);
            if (!repositoryStartRequest.IsSuccess
                || repositoryStartRequest.Value is null)
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }
            var modelConfig = ReadExplicitModelConfig(readEnvironment);
            var trainingConfig = ReadExplicitTrainingConfig(
                readEnvironment,
                modelConfig,
                trainingActivationRequested);

            if (!int.TryParse(
                    maxCallsText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var maxTeacherCalls)
                || maxTeacherCalls < 0
                || maxTeacherCalls
                    > LegalTrainingTeacherAdmissionAuthority.HardMaximumTeacherCalls)
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }

            if (!TryDecodeCanonicalAuditSalt(saltText, out var auditSalt))
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");

            var paths = new[]
            {
                baseCheckpoint,
                tokenizer,
                adapterRoot,
                cacheRoot,
                registryRoot,
                pairRoot,
            };
            if (AnyLexicalPathsOverlap(paths))
                return Invalid("CONTINUOUS_LEARNING_LEXICAL_PATH_COLLISION");

            var teacherConfiguration = ContinuousLearningTp1DpoConfiguration.Create(
                modelId,
                expectedOllamaManifestDigest,
                expectedOllamaModelLayerDigest,
                maxTeacherCalls,
                localRetentionPolicyRef,
                outputLicenseRef,
                auditSalt,
                dpoMaxStepCount.Value, // Keep optimizer limits independent of the selected CPU inference budget.
                authoringProfile.Value,
                evaluationProfile.Value);
            if (!teacherConfiguration.IsSuccess
                || teacherConfiguration.Value is null)
            {
                return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
            }

            return new ContinuousLearningConfigurationResolution(
                ContinuousLearningConfigurationState.Enabled,
                new ContinuousLearningDeferredCompositionPlan(
                    teacherConfiguration.Value,
                    new ContinuousLearningBaseCheckpointModelIdentity(
                        baseModelId,
                        baseModelRevision),
                    baseCheckpoint,
                    tokenizer,
                    adapterRoot,
                    cacheRoot,
                    registryRoot,
                    repositoryDpoRoot.Value,
                    pairRoot,
                    repositoryStartRequest.Value,
                    modelConfig,
                    trainingConfig,
                    trainingActivationRequested,
                    localIndependentPreferenceEvaluatorRequested, // Keep the existing trainer and evaluator activation authority unchanged.
                    currentDpoDatasetManifest is not null // Only the successful repository-owned sealed-manifest admission selects read-only input.
                        ? ContinuousLearningDpoPairRootAccess.SealedRepositorySource // The canonical sealed dataset is an input, not a mutable pair store.
                        : ContinuousLearningDpoPairRootAccess.MutablePairStore), // Missing or rejected seals preserve the guarded legacy mutation contract.
                ErrorCode: "none");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid("CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
        }
    }

    private static string? ReadBounded(
        Func<string, string?> readEnvironment,
        string name)
    {
        var value = readEnvironment(name);
        if (value is not null && value.Length > MaximumEnvironmentValueLength)
            throw new InvalidDataException("Configuration value is too long.");
        return value;
    }

    internal static OperationResult<TrainingTeacherInferenceProfileEvidence> ReadAuthoringProfile(Func<string, string?> readEnvironment) // Parse only immutable allowlisted profiles; this helper issues no effect or source rights.
    { // Read each process value once so selection cannot drift during this startup operation.
        var id = ReadBounded(readEnvironment, AuthoringProfileIdEnvironmentName); // A missing pair of values retains the legacy default.
        var sha = ReadBounded(readEnvironment, AuthoringProfileSha256EnvironmentName); // A single missing value becomes an invalid passport rather than fallback.
        var selected = LocalQwenOllamaInferenceProfileAuthority.ResolveAuthoringDescriptor(id is null && sha is null ? null : new(id ?? "", sha ?? "")); // Enforce exact ID/hash equality without arbitrary options.
        return selected.IsSuccess && selected.Value is not null ? OperationResult<TrainingTeacherInferenceProfileEvidence>.Ok(selected.Value.Evidence) : selected.MapError<TrainingTeacherInferenceProfileEvidence>(); // Return the canonical evidence that the complete configuration will retain.
    } // No environment mutation or transport occurs during profile parsing.

    internal static OperationResult<TrainingTeacherInferenceProfileEvidence> ReadEvaluationProfile(Func<string, string?> readEnvironment)
    {
        var id = ReadBounded(readEnvironment, EvaluationProfileIdEnvironmentName);
        var sha = ReadBounded(readEnvironment, EvaluationProfileSha256EnvironmentName);
        var selected = LocalQwenOllamaInferenceProfileAuthority.ResolveEvaluationDescriptor(
            id is null && sha is null ? null : new(id ?? "", sha ?? ""));
        return selected.IsSuccess && selected.Value is not null
            ? OperationResult<TrainingTeacherInferenceProfileEvidence>.Ok(selected.Value.Evidence)
            : selected.MapError<TrainingTeacherInferenceProfileEvidence>();
    }

    private static OperationResult<int> ReadOptionalDpoMaxStepCount(
        Func<string, string?> readEnvironment)
    {
        var value = ReadBounded(readEnvironment, DpoMaxStepCountEnvironmentName);
        if (value is null)
            return OperationResult<int>.Ok(StableDpoTrainingExecutor.DefaultDpoStepCount);

        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl)
            || !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            return OperationResult<int>.Fail(
                "ContinuousLearning DPO max step count is invalid.",
                "CONTINUOUS_LEARNING_CONFIGURATION_INVALID");
        }

        return StableDpoTrainingExecutor.ValidateDpoStepCount(parsed);
    }

    private static string ReadRequired(
        Func<string, string?> readEnvironment,
        string name)
    {
        var value = ReadBounded(readEnvironment, name);
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl))
        {
            throw new InvalidDataException("Required configuration is missing.");
        }
        return value;
    }

    private static OperationResult<
        RepositoryOwnedIncrementalTrainingSessionStartRequest>
        CreateRepositoryStartRequest(
            RepositoryOwnedDpoDatasetAddress dpoDatasetAddress,
            RepositoryOwnedDpoDatasetManifestIdentity?
                currentDpoDatasetManifest,
            string modelIdSlug,
            string modelVersion,
            RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility)
    {
        var checkpointAddress =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                dpoDatasetAddress.RepositoryRoot,
                dpoDatasetAddress.DomainSlug,
                dpoDatasetAddress.DesignPatternSlug,
                modelIdSlug,
                modelVersion,
                step: 1);
        if (!checkpointAddress.IsSuccess || checkpointAddress.Value is null)
        {
            return checkpointAddress.MapError<
                RepositoryOwnedIncrementalTrainingSessionStartRequest>();
        }

        // WHY: production startup must name the exact repository-owned model
        // lane before any trainer can run. Cold start is denied here by
        // default, so a missing saved model fails closed instead of retraining
        // from the legacy base every process start.
        return OperationResult<
            RepositoryOwnedIncrementalTrainingSessionStartRequest>.Ok(
                new RepositoryOwnedIncrementalTrainingSessionStartRequest(
                    dpoDatasetAddress.RepositoryRoot,
                    dpoDatasetAddress.DomainSlug,
                    dpoDatasetAddress.DesignPatternSlug,
                    modelIdSlug,
                    modelVersion,
                    runtimeCompatibility,
                    RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                    CurrentDpoDatasetManifest: currentDpoDatasetManifest));
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity?
        TryReadCurrentRepositoryOwnedDpoDatasetManifestIdentity(
            RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            var opened = RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(address);
            return opened.IsSuccess && opened.Value is not null
                ? new RepositoryOwnedDpoDatasetManifestIdentity(
                    address.ManifestRelativePath,
                    opened.Value.Manifest.ManifestSha256)
                : null;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }
    }

    private static RepositoryOwnedModelRuntimeCompatibility?
        ReadRepositoryRuntimeCompatibility(
            Func<string, string?> readEnvironment)
    {
        var runtimeCompatibility = new RepositoryOwnedModelRuntimeCompatibility(
            ReadRequired(readEnvironment, RuntimeConfigSha256EnvironmentName),
            ReadRequired(readEnvironment, RuntimeTopologySha256EnvironmentName),
            ReadRequired(readEnvironment, RuntimeTokenizerSha256EnvironmentName),
            ReadRequired(readEnvironment, RuntimeTrainerSha256EnvironmentName),
            ReadRequired(readEnvironment, RuntimeSchemaSha256EnvironmentName),
            ReadRequired(readEnvironment, RuntimeCompatibilityKeyEnvironmentName));
        return IsRepositoryRuntimeCompatibilityValid(runtimeCompatibility)
            ? runtimeCompatibility
            : null;
    }

    private static bool IsRepositoryRuntimeCompatibilityValid(
        RepositoryOwnedModelRuntimeCompatibility compatibility)
        => IsCanonicalSha256Hex(compatibility.ConfigSha256)
           && IsCanonicalSha256Hex(compatibility.TopologySha256)
           && IsCanonicalSha256Hex(compatibility.TokenizerSha256)
           && IsCanonicalSha256Hex(compatibility.TrainerSha256)
           && IsCanonicalSha256Hex(compatibility.SchemaSha256)
           && IsSafeCompatibilityKey(compatibility.CompatibilityKey);

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: 64 }
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool IsSafeCompatibilityKey(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= 512
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    private static bool IsLogicalModelIdentity(
        string value,
        int maximumLength,
        bool allowSlash)
    {
        if (value.Length > maximumLength
            || Path.IsPathRooted(value)
            || value.Contains('\\')
            || value.Contains("//", StringComparison.Ordinal)
            || value.StartsWith("./", StringComparison.Ordinal)
            || value.StartsWith("../", StringComparison.Ordinal)
            || value.EndsWith('/'))
        {
            return false;
        }

        var segments = value.Split('/');
        if (!allowSlash && segments.Length != 1)
            return false;
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
            return false;

        return value.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character is '_' or '-' or '.' or ':' or '@'
            || allowSlash && character == '/');
    }

    private static bool TryDecodeCanonicalAuditSalt(
        string encoded,
        out byte[] auditSalt)
    {
        auditSalt = Array.Empty<byte>();
        if (encoded.Length != 44)
            return false;

        Span<byte> decoded = stackalloc byte[AuditSaltBytes];
        if (!Convert.TryFromBase64String(encoded, decoded, out var written)
            || written != AuditSaltBytes)
        {
            return false;
        }

        auditSalt = decoded.ToArray();
        return StringComparer.Ordinal.Equals(
            Convert.ToBase64String(auditSalt),
            encoded);
    }

    private static ModelConfig ReadExplicitModelConfig(
        Func<string, string?> readEnvironment)
    {
        var config = new ModelConfig
        {
            VocabSize = ReadBoundedInt(readEnvironment, ModelVocabSizeEnvironmentName, 256, 262_144),
            ContextLen = ReadBoundedInt(readEnvironment, ModelContextLenEnvironmentName, 2, 65_536),
            HiddenDim = ReadBoundedInt(readEnvironment, ModelHiddenDimEnvironmentName, 1, 32_768),
            NumLayers = ReadBoundedInt(readEnvironment, ModelNumLayersEnvironmentName, 1, 256),
            NumHeads = ReadBoundedInt(readEnvironment, ModelNumHeadsEnvironmentName, 1, 256),
            NumKvHeads = ReadBoundedInt(readEnvironment, ModelNumKvHeadsEnvironmentName, 1, 256),
            FfnHiddenDim = ReadBoundedInt(readEnvironment, ModelFfnHiddenDimEnvironmentName, 1, 131_072),
            RopeTheta = ReadBoundedFloat(readEnvironment, ModelRopeThetaEnvironmentName, 1f, 10_000_000f),
            RmsNormEps = ReadBoundedFloat(readEnvironment, ModelRmsNormEpsEnvironmentName, 1e-12f, 1f),
        };

        if (config.HiddenDim % config.NumHeads != 0
            || config.NumHeads % config.NumKvHeads != 0)
        {
            throw new InvalidDataException("Model attention dimensions are incompatible.");
        }
        return config;
    }

    private static TrainingConfig ReadExplicitTrainingConfig(
        Func<string, string?> readEnvironment,
        ModelConfig modelConfig,
        bool trainingActivationRequested)
    {
        var maxLr = ReadBoundedFloat(readEnvironment, TrainingMaxLrEnvironmentName, float.Epsilon, 1f);
        var minLr = ReadBoundedFloat(readEnvironment, TrainingMinLrEnvironmentName, 0f, maxLr);
        var totalSteps = ReadBoundedInt(
            readEnvironment,
            TrainingTotalStepsEnvironmentName,
            1,
            10_000_000);
        var config = new TrainingConfig
        {
            BatchSize = ReadBoundedInt(readEnvironment, TrainingBatchSizeEnvironmentName, 1, 1_024),
            SeqLen = ReadBoundedInt(readEnvironment, TrainingSeqLenEnvironmentName, 1, modelConfig.ContextLen),
            MaxLr = maxLr,
            MinLr = minLr,
            WarmupSteps = ReadBoundedInt(readEnvironment, TrainingWarmupStepsEnvironmentName, 0, totalSteps),
            TotalSteps = totalSteps,
            GradAccumSteps = ReadBoundedInt(readEnvironment, TrainingGradAccumStepsEnvironmentName, 1, 1_024),
            GradClipNorm = ReadBoundedFloat(readEnvironment, TrainingGradClipNormEnvironmentName, float.Epsilon, 1_000_000f),
            WeightDecay = ReadBoundedFloat(readEnvironment, TrainingWeightDecayEnvironmentName, 0f, 1_000_000f),
            CheckpointEvery = ReadBoundedInt(readEnvironment, TrainingCheckpointEveryEnvironmentName, 1, totalSteps),
            // Pretraining is not part of this materialization. The explicit
            // invalid value makes accidental pretraining fail before any write.
            CheckpointDir = string.Empty,
            LogEvery = ReadBoundedInt(readEnvironment, TrainingLogEveryEnvironmentName, 1, totalSteps),
            RootInitializationSeed = ReadRootInitializationSeed(
                readEnvironment,
                trainingActivationRequested),
        };
        return config;
    }

    internal static ulong? ReadRootInitializationSeed(
        Func<string, string?> readEnvironment,
        bool required)
    {
        var text = ReadBounded(
            readEnvironment,
            TrainingRootInitializationSeedEnvironmentName);
        if (text is null)
        {
            if (required)
                throw new InvalidDataException(
                    "Root initialization seed is required for training activation.");
            return null;
        }

        if (text.Length == 0
            || text.Any(static character => character is < '0' or > '9')
            || text.Length > 1 && text[0] == '0'
            || !ulong.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var value)
            || !StringComparer.Ordinal.Equals(
                value.ToString(CultureInfo.InvariantCulture),
                text))
        {
            throw new InvalidDataException(
                "Root initialization seed is not canonical unsigned decimal.");
        }

        return value;
    }

    private static int ReadBoundedInt(
        Func<string, string?> readEnvironment,
        string name,
        int minimum,
        int maximum)
    {
        var text = ReadRequired(readEnvironment, name);
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            || value < minimum
            || value > maximum
            || !StringComparer.Ordinal.Equals(value.ToString(CultureInfo.InvariantCulture), text))
        {
            throw new InvalidDataException("Numeric configuration is invalid.");
        }
        return value;
    }

    private static float ReadBoundedFloat(
        Func<string, string?> readEnvironment,
        string name,
        float minimum,
        float maximum)
    {
        var text = ReadRequired(readEnvironment, name);
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            || !float.IsFinite(value)
            || value < minimum
            || value > maximum
            || !StringComparer.Ordinal.Equals(value.ToString("R", CultureInfo.InvariantCulture), text))
        {
            throw new InvalidDataException("Numeric configuration is invalid.");
        }
        return value;
    }

    private static string RequireCanonicalRegularFileMetadata(
        string path,
        long maximumBytes)
    {
        var fullPath = RequireCanonicalPath(path, directoryRoot: false);
        var attributes = File.GetAttributes(fullPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw new InvalidDataException("Configured artifact is not a regular file.");
        RejectCurrentlyObservedReparseSegments(fullPath);

        var length = new FileInfo(fullPath).Length;
        if (length <= 0 || length > maximumBytes)
            throw new InvalidDataException("Configured artifact size is invalid.");
        return fullPath;
    }

    private static string RequireCanonicalDirectoryRootMetadata(string path)
    {
        var fullPath = RequireCanonicalPath(path, directoryRoot: true);
        if (File.Exists(fullPath))
            throw new InvalidDataException("Configured directory root is a file.");
        if (Directory.Exists(fullPath)
            && (File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException("Configured directory root is a reparse point.");
        }
        RejectCurrentlyObservedReparseSegments(fullPath);
        return fullPath;
    }

    private static string RequireCanonicalPath(string path, bool directoryRoot)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new InvalidDataException("Configured path must be fully qualified.");

        var fullPath = Path.GetFullPath(path);
        if (directoryRoot)
            fullPath = Path.TrimEndingDirectorySeparator(fullPath);
        if (!PathComparer.Equals(path, fullPath))
            throw new InvalidDataException("Configured path must be canonical.");
        return fullPath;
    }

    private static void RejectCurrentlyObservedReparseSegments(string fullPath)
    {
        string? current = fullPath;
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.Exists(current) || Directory.Exists(current))
                && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    "Configured path currently contains a reparse point.");
            }
            current = Path.GetDirectoryName(current);
        }
    }

    private static bool AnyLexicalPathsOverlap(IReadOnlyList<string> paths)
    {
        for (var left = 0; left < paths.Count; left++)
        {
            for (var right = left + 1; right < paths.Count; right++)
            {
                if (PathComparer.Equals(paths[left], paths[right])
                    || IsLexicallyContainedBy(paths[left], paths[right])
                    || IsLexicallyContainedBy(paths[right], paths[left]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsLexicallyContainedBy(string candidate, string root)
    {
        var prefix = Path.EndsInDirectorySeparator(root)
            ? root
            : root + Path.DirectorySeparatorChar;
        return candidate.StartsWith(
            prefix,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static ContinuousLearningConfigurationResolution Invalid(string code)
        => new(
            ContinuousLearningConfigurationState.Invalid,
            DeferredPlan: null,
            ErrorCode: code);

    private static StringComparer PathComparer { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
