// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

public sealed record DesignConversionDpoPolicyBaseIdentity(
    string ProviderKind,
    string ConfigurationKind,
    string BaseModelId,
    string BaseRevision,
    string BaseCheckpointSha256);

public sealed record DesignConversionDpoPolicyBaseProviderOptions(
    ulong? RootInitializationSeed,
    string TrainingConfigurationFingerprint) // Carry the training-configuration fingerprint for the provider's random-state and reproducibility contract.
{ // Provider options carry checkpoint metadata without granting execution rights.
    // Only the existing trusted initial-root owner can construct this declaration.
    // It carries serialized topology and bytes, never continuation authority.
    public ContinuousLearning.RepositoryOwnedFullLoraCheckpointDescriptor? InitialFullCheckpoint { get; init; } // Carry the same immutable full-LoRA declaration through startup and later policy loading.
} // Provider options carry checkpoint metadata without granting execution rights.

public static class DesignConversionDpoPolicyRandomContextContract
{
    public const int CurrentSchemaVersion = 1;
    public const string PolicyDropoutStreamDomain =
        "design-conversion.policy";
    public const string ReferenceDropoutStreamDomain =
        "design-conversion.reference";
    public const string FreshLoadDropoutStreamDomain =
        "design-conversion.fresh-load";
    public const string ReproducibilityScope =
        "initialization-only-no-mid-step-dropout-resume";
    public const string TypedEvidence =
        "DESIGN_CONVERSION_DPO_POLICY_RANDOM_CONTEXT_TYPED";
    public const string InitializationReproducibilityOnlyEvidence =
        "DESIGN_CONVERSION_DPO_POLICY_INITIALIZATION_REPRODUCIBILITY_ONLY";
    public const string RootInitializationSeedEvidencePrefix =
        "dpo_policy_root_initialization_seed:";
    public const string TrainingConfigurationFingerprintEvidencePrefix =
        "dpo_policy_training_configuration_fingerprint:";
    public const string RandomDerivationSchemeEvidencePrefix =
        "dpo_policy_random_derivation_scheme:";

    public static DesignConversionDpoPolicyRandomContext CreateCurrent(
        ulong rootInitializationSeed,
        string trainingConfigurationFingerprint)
    {
        if (!IsCanonicalFingerprint(trainingConfigurationFingerprint))
        {
            throw new ArgumentException(
                "Training configuration fingerprint must be 64 lowercase hexadecimal characters.",
                nameof(trainingConfigurationFingerprint));
        }

        return new DesignConversionDpoPolicyRandomContext(
            CurrentSchemaVersion,
            rootInitializationSeed,
            trainingConfigurationFingerprint,
            LoraAdapterRandomSeedDerivation.DerivationScheme,
            LoraAdapterRandomSeedDerivation.InitializationDomain,
            LoraAdapterRandomSeedDerivation.DropoutDomain,
            PolicyDropoutStreamDomain,
            ReferenceDropoutStreamDomain,
            FreshLoadDropoutStreamDomain,
            ReproducibilityScope);
    }

    public static bool IsValidCurrent(
        DesignConversionDpoPolicyRandomContext? context)
        => context is not null
           && context.SchemaVersion == CurrentSchemaVersion
           && IsCanonicalFingerprint(context.TrainingConfigurationFingerprint)
           && string.Equals(
               context.DerivationScheme,
               LoraAdapterRandomSeedDerivation.DerivationScheme,
               StringComparison.Ordinal)
           && string.Equals(
               context.InitializationDomain,
               LoraAdapterRandomSeedDerivation.InitializationDomain,
               StringComparison.Ordinal)
           && string.Equals(
               context.DropoutDomain,
               LoraAdapterRandomSeedDerivation.DropoutDomain,
               StringComparison.Ordinal)
           && string.Equals(
               context.PolicyDropoutStreamDomain,
               PolicyDropoutStreamDomain,
               StringComparison.Ordinal)
           && string.Equals(
               context.ReferenceDropoutStreamDomain,
               ReferenceDropoutStreamDomain,
               StringComparison.Ordinal)
           && string.Equals(
               context.FreshLoadDropoutStreamDomain,
               FreshLoadDropoutStreamDomain,
               StringComparison.Ordinal)
           && string.Equals(
               context.ReproducibilityScope,
               ReproducibilityScope,
               StringComparison.Ordinal);

    public static bool MatchesCurrentAuthority(
        DesignConversionDpoPolicyRandomContext? context,
        ulong rootInitializationSeed,
        string trainingConfigurationFingerprint)
        => IsValidCurrent(context)
           && context!.RootInitializationSeed == rootInitializationSeed
           && string.Equals(
               context.TrainingConfigurationFingerprint,
               trainingConfigurationFingerprint,
               StringComparison.Ordinal);

    public static string[] BuildRequiredEvidenceRefs(
        DesignConversionDpoPolicyRandomContext context)
    {
        if (!IsValidCurrent(context))
        {
            throw new ArgumentException(
                "DesignConversion DPO policy random context is invalid.",
                nameof(context));
        }

        return
        [
            TypedEvidence,
            InitializationReproducibilityOnlyEvidence,
            $"{RootInitializationSeedEvidencePrefix}{context.RootInitializationSeed.ToString(CultureInfo.InvariantCulture)}",
            $"{TrainingConfigurationFingerprintEvidencePrefix}{context.TrainingConfigurationFingerprint}",
            $"{RandomDerivationSchemeEvidencePrefix}{context.DerivationScheme}"
        ];
    }

    public static bool HasRequiredEvidenceRefs(
        DesignConversionDpoPolicyRandomContext? context,
        IReadOnlyList<string>? evidenceRefs)
    {
        if (!IsValidCurrent(context) || evidenceRefs is null)
            return false;

        var requiredEvidenceRefs = BuildRequiredEvidenceRefs(context!);
        if (!requiredEvidenceRefs.All(required =>
                evidenceRefs.Contains(required, StringComparer.Ordinal)))
        {
            return false;
        }

        return HasExactlyOneCanonicalReservedEvidenceRef(
                   evidenceRefs,
                   RootInitializationSeedEvidencePrefix,
                   $"{RootInitializationSeedEvidencePrefix}" +
                   context!.RootInitializationSeed.ToString(
                       CultureInfo.InvariantCulture)) &&
               HasExactlyOneCanonicalReservedEvidenceRef(
                   evidenceRefs,
                   TrainingConfigurationFingerprintEvidencePrefix,
                   $"{TrainingConfigurationFingerprintEvidencePrefix}" +
                   context.TrainingConfigurationFingerprint) &&
               HasExactlyOneCanonicalReservedEvidenceRef(
                   evidenceRefs,
                   RandomDerivationSchemeEvidencePrefix,
                   $"{RandomDerivationSchemeEvidencePrefix}" +
                   context.DerivationScheme);
    }

    private static bool HasExactlyOneCanonicalReservedEvidenceRef(
        IReadOnlyList<string> evidenceRefs,
        string prefix,
        string expected)
    {
        var matchCount = 0;
        foreach (var evidenceRef in evidenceRefs)
        {
            if (evidenceRef is null ||
                !evidenceRef.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            matchCount++;
            if (matchCount > 1 ||
                !string.Equals(evidenceRef, expected, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return matchCount == 1;
    }

    internal static bool IsCanonicalFingerprint(string? value)
    {
        if (value is not { Length: 64 })
            return false;
        foreach (var character in value)
        {
            if (!((character >= '0' && character <= '9')
                  || (character >= 'a' && character <= 'f')))
            {
                return false;
            }
        }
        return true;
    }
}

public sealed record DesignConversionDpoPolicyBaseModelSet(
    DesignConversionDpoPolicyBaseIdentity Identity,
    LlmModel Policy,
    LlmModel Reference,
    LlmModel FreshLoadTarget,
    DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding = null)
{
    public DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
        CanonicalBpeTokenizerSnapshot { get; init; }

    public DesignConversionDpoPolicyRandomContext? RandomContext { get; init; }

}

internal enum DesignConversionDpoPolicyModelRole
{
    Policy,
    Reference,
    FreshLoad
}

/// <summary>
/// Immutable, path-free description of the identity and model configuration a
/// provider will use. Capturing this descriptor must not open checkpoints,
/// tokenizers, streams, or construct models.
/// </summary>
public sealed class DesignConversionDpoPolicyBasePreparationDescriptor
{
    public const string CurrentSchema =
        "design-conversion-dpo-policy-base-preparation-v1";

    internal DesignConversionDpoPolicyBasePreparationDescriptor(
        DesignConversionDpoPolicyBaseIdentity identity,
        ModelConfig config,
        string providerBindingRef = "",
        long providerBindingGeneration = 0)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(config);
        Schema = CurrentSchema;
        ProviderKind = identity.ProviderKind;
        ConfigurationKind = identity.ConfigurationKind;
        BaseModelId = identity.BaseModelId;
        BaseRevision = identity.BaseRevision;
        BaseCheckpointSha256 = identity.BaseCheckpointSha256;
        VocabSize = config.VocabSize;
        ContextLength = config.ContextLen;
        HiddenDim = config.HiddenDim;
        NumLayers = config.NumLayers;
        NumHeads = config.NumHeads;
        NumKvHeads = config.NumKvHeads;
        FfnHiddenDim = config.FfnHiddenDim;
        RopeTheta = config.RopeTheta;
        RmsNormEps = config.RmsNormEps;
        ConfigSha256 = DesignConversionDpoPolicyBaseMetadata
            .ComputeConfigSha256(config);
        ProviderBindingRef = providerBindingRef ?? string.Empty;
        ProviderBindingGeneration = providerBindingGeneration;
    }

    public string Schema { get; }
    public string ProviderKind { get; }
    public string ConfigurationKind { get; }
    public string BaseModelId { get; }
    public string BaseRevision { get; }
    public string BaseCheckpointSha256 { get; }
    public int VocabSize { get; }
    public int ContextLength { get; }
    public int HiddenDim { get; }
    public int NumLayers { get; }
    public int NumHeads { get; }
    public int NumKvHeads { get; }
    public int FfnHiddenDim { get; }
    public float RopeTheta { get; }
    public float RmsNormEps { get; }
    public string ConfigSha256 { get; }
    public string ProviderBindingRef { get; }
    public long ProviderBindingGeneration { get; }

    internal DesignConversionDpoPolicyBaseIdentity ToIdentity()
        => new(
            ProviderKind,
            ConfigurationKind,
            BaseModelId,
            BaseRevision,
            BaseCheckpointSha256);

    internal ModelConfig ToConfig()
        => new()
        {
            VocabSize = VocabSize,
            ContextLen = ContextLength,
            HiddenDim = HiddenDim,
            NumLayers = NumLayers,
            NumHeads = NumHeads,
            NumKvHeads = NumKvHeads,
            FfnHiddenDim = FfnHiddenDim,
            RopeTheta = RopeTheta,
            RmsNormEps = RmsNormEps
        };
}

/// <summary>
/// Transfers ownership of one fresh canonical BPE tokenizer artifact stream to
/// the caller for every successful call. The stream must be readable,
/// seekable, positioned at zero, and must not be shared with another consumer.
/// </summary>
public interface IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
{
    Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Path-free, reopenable handle for the canonical BPE tokenizer snapshot that
/// was validated with a checkpoint-backed model set.
/// </summary>
public sealed class DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
{
    public const string CancelledErrorCode = "CANCELLED";
    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_DPO_CANONICAL_BPE_TOKENIZER_SNAPSHOT_INVALID";
    public const string SourceFailedErrorCode =
        "DESIGN_CONVERSION_DPO_CANONICAL_BPE_TOKENIZER_SNAPSHOT_SOURCE_FAILED";

    private const string CancelledErrorMessage =
        "DesignConversion DPO canonical BPE tokenizer snapshot opening was cancelled.";
    private const string InvalidErrorMessage =
        "The DesignConversion DPO canonical BPE tokenizer snapshot is invalid.";
    private const string SourceFailedErrorMessage =
        "The DesignConversion DPO canonical BPE tokenizer snapshot source failed.";

    private readonly ModelConfig _ownedConfig;
    private readonly IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource _source;

    internal DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor descriptor,
        ModelConfig modelConfig,
        IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource source)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(source);
        Descriptor = DesignConversionDpoPolicyBaseMetadata
            .CopyCanonicalBpeTokenizerArtifactDescriptor(descriptor);
        _ownedConfig = DesignConversionDpoPolicyBaseMetadata.CopyConfig(modelConfig);
        _source = source;
    }

    public DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor Descriptor { get; }

    public async Task<OperationResult<Stream>> OpenSnapshotAsync(
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<Stream>? sourceResult;
        try
        {
            sourceResult = await _source.OpenCanonicalBpeTokenizerSnapshotAsync(
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();
            return SourceFailed();
        }

        if (sourceResult is null || !sourceResult.IsSuccess || sourceResult.Value is null)
        {
            if (sourceResult?.Value is not null)
                DisposeOwnedStream(sourceResult.Value);
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();
            return SourceFailed();
        }

        var stream = sourceResult.Value;
        var transferOwnership = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revalidated = DesignConversionDpoPromotionArtifactContractCodec
                .RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                    stream,
                    _ownedConfig,
                    Descriptor,
                    cancellationToken);
            if (!revalidated.IsSuccess || revalidated.Value is null)
            {
                if (cancellationToken.IsCancellationRequested ||
                    string.Equals(
                        revalidated.ErrorCode,
                        CancelledErrorCode,
                        StringComparison.Ordinal))
                {
                    return Cancelled();
                }

                return Invalid();
            }

            cancellationToken.ThrowIfCancellationRequested();
            transferOwnership = true;
            return OperationResult<Stream>.Ok(stream);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();
            return Invalid();
        }
        finally
        {
            if (!transferOwnership)
                DisposeOwnedStream(stream);
        }
    }

    private static OperationResult<Stream> Cancelled()
        => OperationResult<Stream>.Fail(
            CancelledErrorMessage,
            CancelledErrorCode);

    private static OperationResult<Stream> SourceFailed()
        => OperationResult<Stream>.Fail(
            SourceFailedErrorMessage,
            SourceFailedErrorCode);

    private static OperationResult<Stream> Invalid()
        => OperationResult<Stream>.Fail(
            InvalidErrorMessage,
            InvalidErrorCode);

    private static void DisposeOwnedStream(Stream stream)
    {
        try
        {
            stream.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // A sanitized open result must not be replaced with source details.
        }
    }
}

public interface IDesignConversionDpoPolicyBaseProvider
{
    public const string PreparationDescriptorUnavailableErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_PREPARATION_DESCRIPTOR_UNAVAILABLE";

    OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
        DescribePreparation()
        => OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>.Fail(
            "The DesignConversion DPO policy base preparation descriptor is unavailable.",
            PreparationDescriptorUnavailableErrorCode);

    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>> CreateModelSetAsync(
        CancellationToken cancellationToken);

    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            CancellationToken cancellationToken)
        => Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            "The DesignConversion DPO policy base preparation descriptor is unavailable.",
            PreparationDescriptorUnavailableErrorCode));
}

public sealed record DesignConversionDpoPolicyContinuationRequest(
    IDesignConversionDpoPolicyCheckpointSource CheckpointSource,
    DesignConversionDpoSourceEncoderContract SourceEncoderContract,
    DesignConversionDpoPolicyCheckpointContract PolicyCheckpointContract,
    DesignConversionDpoUtf8ByteTokenizerArtifact? TokenizerArtifact = null,
    DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding = null)
{
    public DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
        ParentCanonicalBpeTokenizerSnapshot { get; init; }

    internal DesignConversionDpoCheckpointMutationAdmission?
        CheckpointMutationAdmission { get; init; }

    internal IDesignConversionDpoPolicyCheckpointSource?
        BridgeProofCheckpointSource { get; init; }
}

internal sealed class DesignConversionDpoInMemoryCanonicalBpeTokenizerSnapshotSource(
    byte[] bytes)
    : IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
{
    private readonly byte[] _bytes = bytes?.ToArray() ??
        throw new ArgumentNullException(nameof(bytes));

    public Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
        CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Task.FromResult(OperationResult<Stream>.Fail(
                "DesignConversion DPO canonical BPE tokenizer snapshot opening was cancelled.",
                DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
                    .CancelledErrorCode))
            : Task.FromResult(OperationResult<Stream>.Ok(
                new MemoryStream(_bytes, writable: false)));
}

public interface IDesignConversionDpoPolicyContinuationProvider
    : IDesignConversionDpoPolicyBaseProvider
{
    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken);

    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        => Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            "The DesignConversion DPO policy base preparation descriptor is unavailable.",
            PreparationDescriptorUnavailableErrorCode));

    // Код отказа для сверочного набора моделей: отдельный от кода про
    // описатель подготовки, чтобы отказ на пути продолжения был опознаваем
    // именно как отсутствие сверочного входа у поставщика.
    public const string VerificationModelSetUnavailableErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_VERIFICATION_MODEL_SET_UNAVAILABLE";

    // Сверочный набор для доказательства неизменности продолжения: три
    // независимые модели по ролям, в которые мост затем загрузит допущенную
    // контрольную точку. Обучаемые модели этим входом не отдаются.
    // Отпечаток остаётся за мостом; поставщик отдаёт только модели.
    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuationVerificationModelSetAsync(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
        // Отказ по умолчанию закрыт: поставщик, который сверочный набор не
        // умеет отдавать, не может молча пройти путь продолжения.
        => Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            "The DesignConversion DPO continuation verification model set is unavailable.",
            VerificationModelSetUnavailableErrorCode));
}

internal static class DesignConversionDpoPolicyBaseMetadata
{
    public const string LegacyTinyPolicyConfigurationKind =
        "DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig";
    public const string LegacyFreshPolicyConfigurationKind =
        "DesignConversionDpoTrainingHandoff.ConvertPolicyCheckpointToRuntimeCheckpointAsync:freshPolicyModel.Config";

    internal const int CanonicalLoraRank = 2;
    internal const float CanonicalLoraAlpha = 4f;
    internal const float CanonicalLoraDropout = 0f;

    private static readonly string[] CanonicalLoraTargets = ["q_proj", "v_proj"];

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool IsValidIdentity(DesignConversionDpoPolicyBaseIdentity? identity)
        => identity is not null &&
           IsLogicalIdentifier(identity.ProviderKind, 64, allowSlash: false) &&
           IsLogicalIdentifier(identity.ConfigurationKind, 160, allowSlash: false) &&
           !IsLegacyConfigurationKind(identity.ConfigurationKind) &&
           IsLogicalIdentifier(identity.BaseModelId, 256, allowSlash: true) &&
           IsLogicalIdentifier(identity.BaseRevision, 160, allowSlash: false) &&
           IsCanonicalSha256(identity.BaseCheckpointSha256);

    public static bool IsLegacyConfigurationKind(string? configurationKind)
        => string.Equals(
               configurationKind,
               LegacyTinyPolicyConfigurationKind,
               StringComparison.Ordinal) ||
           string.Equals(
               configurationKind,
               LegacyFreshPolicyConfigurationKind,
               StringComparison.Ordinal);

    public static bool IsLogicalIdentifier(string? value, int maximumLength, bool allowSlash)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > maximumLength ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            Path.IsPathRooted(value) ||
            HasWindowsDrivePrefix(value) ||
            value.Contains('\\') ||
            value.Contains("//", StringComparison.Ordinal) ||
            value.StartsWith("./", StringComparison.Ordinal) ||
            value.StartsWith("../", StringComparison.Ordinal) ||
            value.EndsWith('/'))
        {
            return false;
        }

        var segments = value.Split('/');
        if (!allowSlash && segments.Length != 1)
            return false;
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
            return false;

        return value.All(character =>
            char.IsAsciiLetterOrDigit(character) ||
            character is '_' or '-' or '.' or ':' or '@' ||
            allowSlash && character == '/');
    }

    private static bool HasWindowsDrivePrefix(string value)
    {
        // В Linux Path.IsPathRooted не распознаёт Windows-формы C:/model и C:model,
        // поэтому отклоняем букву диска с двоеточием одинаково на всех системах.
        return value.Length >= 2 &&
               char.IsAsciiLetter(value[0]) &&
               value[1] == ':';
    }

    public static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(character =>
               character is >= '0' and <= '9' ||
               character is >= 'a' and <= 'f');

    public static bool IsValidConfig(ModelConfig? config)
        => config is not null &&
           config.VocabSize >= 256 &&
           config.ContextLen >= 2 &&
           config.HiddenDim > 0 &&
           config.NumLayers > 0 &&
           config.NumHeads > 0 &&
           config.NumKvHeads > 0 &&
           config.FfnHiddenDim > 0 &&
           config.HiddenDim % config.NumHeads == 0 &&
           config.NumHeads % config.NumKvHeads == 0 &&
           float.IsFinite(config.RopeTheta) &&
           config.RopeTheta > 0 &&
           float.IsFinite(config.RmsNormEps) &&
           config.RmsNormEps > 0;

    public static bool ConfigsEqual(ModelConfig? left, ModelConfig? right)
        => left is not null &&
           right is not null &&
           left.VocabSize == right.VocabSize &&
           left.ContextLen == right.ContextLen &&
           left.HiddenDim == right.HiddenDim &&
           left.NumLayers == right.NumLayers &&
           left.NumHeads == right.NumHeads &&
           left.NumKvHeads == right.NumKvHeads &&
           left.FfnHiddenDim == right.FfnHiddenDim &&
           BitConverter.SingleToInt32Bits(left.RopeTheta) == BitConverter.SingleToInt32Bits(right.RopeTheta) &&
           BitConverter.SingleToInt32Bits(left.RmsNormEps) == BitConverter.SingleToInt32Bits(right.RmsNormEps);

    internal static ModelConfig CopyConfig(ModelConfig config)
        => new()
        {
            VocabSize = config.VocabSize,
            ContextLen = config.ContextLen,
            HiddenDim = config.HiddenDim,
            NumLayers = config.NumLayers,
            NumHeads = config.NumHeads,
            NumKvHeads = config.NumKvHeads,
            FfnHiddenDim = config.FfnHiddenDim,
            RopeTheta = config.RopeTheta,
            RmsNormEps = config.RmsNormEps
        };

    internal static DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor
        CopyCanonicalBpeTokenizerArtifactDescriptor(
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor descriptor)
        => descriptor with { };

    internal static bool TrainingContextMatchesConfig(
        DesignConversionDpoPolicyCheckpointTrainingContext? context,
        ModelConfig? config)
    {
        if (context is null || !IsValidConfig(config))
            return false;

        var contextConfig = new ModelConfig
        {
            VocabSize = context.VocabSize,
            ContextLen = context.ContextLength,
            HiddenDim = context.HiddenDim,
            NumLayers = context.NumLayers,
            NumHeads = context.NumHeads,
            NumKvHeads = context.NumKvHeads,
            FfnHiddenDim = context.FfnHiddenDim,
            RopeTheta = context.RopeTheta,
            RmsNormEps = context.RmsNormEps
        };
        return IsValidConfig(contextConfig) &&
               ConfigsEqual(contextConfig, config) &&
               IsCanonicalSha256(context.ConfigSha256) &&
               string.Equals(
                   context.ConfigSha256,
                   ComputeConfigSha256(contextConfig),
                   StringComparison.Ordinal);
    }

    internal static LoraConfig CreateCanonicalLoraConfig()
        => new()
        {
            Rank = CanonicalLoraRank,
            Alpha = CanonicalLoraAlpha,
            Dropout = CanonicalLoraDropout,
            TargetModules = CanonicalLoraTargets.ToArray()
        };

    internal static bool IsCanonicalLoraContext(
        DesignConversionDpoPolicyCheckpointLoraContext? context,
        ModelConfig? config)
    {
        if (context is null || !IsValidConfig(config))
            return false;

        int expectedAdapterCount;
        try
        {
            expectedAdapterCount = checked(config!.NumLayers * CanonicalLoraTargets.Length);
        }
        catch (OverflowException)
        {
            return false;
        }

        return context.Rank == CanonicalLoraRank &&
               BitConverter.SingleToInt32Bits(context.Alpha) ==
               BitConverter.SingleToInt32Bits(CanonicalLoraAlpha) &&
               BitConverter.SingleToInt32Bits(context.Dropout) ==
               BitConverter.SingleToInt32Bits(CanonicalLoraDropout) &&
               context.TargetModules is not null &&
               context.TargetModules.SequenceEqual(CanonicalLoraTargets, StringComparer.Ordinal) &&
               context.AdapterCount == expectedAdapterCount;
    }

    // Legacy/test-fixture compatibility only. Active DPO paths use the seeded helper below.
    internal static IReadOnlyList<LoraLinear> AttachCanonicalLoraTopology(LlmModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var config = CreateCanonicalLoraConfig();
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        if (canonical.Rank != CanonicalLoraRank ||
            BitConverter.SingleToInt32Bits(canonical.Alpha) !=
            BitConverter.SingleToInt32Bits(CanonicalLoraAlpha) ||
            BitConverter.SingleToInt32Bits(canonical.Dropout) !=
            BitConverter.SingleToInt32Bits(CanonicalLoraDropout) ||
            !canonical.TargetModules.SequenceEqual(CanonicalLoraTargets, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA configuration changed.");
        }

        var adapters = LoraApplicator.Apply(model, canonical);
        var expectedAdapterCount = checked(model.Config.NumLayers * CanonicalLoraTargets.Length);
        if (adapters.Count != expectedAdapterCount)
        {
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA topology is incomplete.");
        }

        return adapters;
    }

    internal static IReadOnlyList<LoraLinear> AttachSeededCanonicalLoraTopology(
        LlmModel model,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role)
    {
        var application = ValidateSeededCanonicalLoraApplication(
            model,
            randomContext,
            role);
        LoraApplicator.ValidateFreshApplication(model, application.Canonical);

        var adapters = LoraApplicator.Apply(
            model,
            application.Canonical,
            (blockIndex, projection) =>
                LoraAdapterRandomSeedDerivation.Derive(
                    randomContext.RootInitializationSeed,
                    blockIndex,
                    projection,
                    application.RoleDomain));
        if (adapters.Count != application.ExpectedAdapterCount)
        {
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA topology is incomplete.");
        }

        return adapters;
    }

    internal static void ValidateSeededCanonicalLoraFreshApplication(
        LlmModel model,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role)
    {
        var application = ValidateSeededCanonicalLoraApplication(
            model,
            randomContext,
            role);
        LoraApplicator.ValidateFreshApplication(model, application.Canonical);
    }

    internal static IReadOnlyList<LoraLinear> ValidateSeededCanonicalLoraTopology(
        LlmModel model,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role)
    {
        var application = ValidateSeededCanonicalLoraApplication(
            model,
            randomContext,
            role);
        var adapters = LoraApplicator.ValidateSeededApplicationProvenance(
            model,
            application.Canonical,
            (blockIndex, projection) =>
                LoraAdapterRandomSeedDerivation.Derive(
                    randomContext.RootInitializationSeed,
                    blockIndex,
                    projection,
                    application.RoleDomain));
        if (adapters.Count != application.ExpectedAdapterCount)
        {
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA topology is incomplete.");
        }

        return adapters;
    }

    private static SeededCanonicalLoraApplication
        ValidateSeededCanonicalLoraApplication(
            LlmModel model,
            DesignConversionDpoPolicyRandomContext randomContext,
            DesignConversionDpoPolicyModelRole role)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (!DesignConversionDpoPolicyRandomContextContract.IsValidCurrent(
                randomContext))
        {
            throw new ArgumentException(
                "DesignConversion DPO policy random context is invalid.",
                nameof(randomContext));
        }
        if (!IsValidConfig(model.Config))
        {
            throw new ArgumentException(
                "DesignConversion DPO policy model configuration is invalid.",
                nameof(model));
        }

        var roleDomain = role switch
        {
            DesignConversionDpoPolicyModelRole.Policy =>
                randomContext.PolicyDropoutStreamDomain,
            DesignConversionDpoPolicyModelRole.Reference =>
                randomContext.ReferenceDropoutStreamDomain,
            DesignConversionDpoPolicyModelRole.FreshLoad =>
                randomContext.FreshLoadDropoutStreamDomain,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                "DesignConversion DPO policy model role is invalid.")
        };
        var config = CreateCanonicalLoraConfig();
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        if (canonical.Rank != CanonicalLoraRank ||
            BitConverter.SingleToInt32Bits(canonical.Alpha) !=
            BitConverter.SingleToInt32Bits(CanonicalLoraAlpha) ||
            BitConverter.SingleToInt32Bits(canonical.Dropout) !=
            BitConverter.SingleToInt32Bits(CanonicalLoraDropout) ||
            !canonical.TargetModules.SequenceEqual(
                CanonicalLoraTargets,
                StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA configuration changed.");
        }
        var expectedAdapterCount = checked(
            model.Config.NumLayers * CanonicalLoraTargets.Length);

        return new SeededCanonicalLoraApplication(
            canonical,
            roleDomain,
            expectedAdapterCount);
    }

    private sealed record SeededCanonicalLoraApplication(
        LoraConfig Canonical,
        string RoleDomain,
        int ExpectedAdapterCount);

    internal static DesignConversionDpoPolicyCheckpointLoraContext
        CreateCanonicalLoraContext(ModelConfig config, int adapterCount)
    {
        var context = new DesignConversionDpoPolicyCheckpointLoraContext(
            CanonicalLoraRank,
            CanonicalLoraAlpha,
            CanonicalLoraDropout,
            CanonicalLoraTargets.ToArray(),
            adapterCount);
        if (!IsCanonicalLoraContext(context, config))
            throw new InvalidOperationException(
                "DesignConversion DPO canonical LoRA context is invalid.");
        return context;
    }

    public static string ComputeConfigSha256(ModelConfig config)
        => ComputeConfigSha256(
            config.VocabSize,
            config.ContextLen,
            config.HiddenDim,
            config.NumLayers,
            config.NumHeads,
            config.NumKvHeads,
            config.FfnHiddenDim,
            config.RopeTheta,
            config.RmsNormEps);

    public static string ComputeConfigSha256(DesignConversionDpoPolicyCheckpointTrainingContext context)
        => ComputeConfigSha256(
            context.VocabSize,
            context.ContextLength,
            context.HiddenDim,
            context.NumLayers,
            context.NumHeads,
            context.NumKvHeads,
            context.FfnHiddenDim,
            context.RopeTheta,
            context.RmsNormEps);

    public static string ComputeInitialWeightSha256(LlmModel model)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, "design-conversion-dpo-initial-weights-v1");

        var entries = model.GetCheckpointParameterEntries();
        AppendInt32(hash, entries.Count);
        foreach (var entry in entries)
        {
            AppendString(hash, entry.Key);
            AppendInt32(hash, entry.Parameter.Value.Shape.Rank);
            foreach (var dimension in entry.Parameter.Value.Shape.Dimensions)
                AppendInt32(hash, dimension);
            AppendInt32(hash, entry.Parameter.Value.Data.Length);
            foreach (var value in entry.Parameter.Value.Data)
                AppendInt32(hash, BitConverter.SingleToInt32Bits(value));
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string ComputeConfigSha256(
        int vocabSize,
        int contextLength,
        int hiddenDim,
        int numLayers,
        int numHeads,
        int numKvHeads,
        int ffnHiddenDim,
        float ropeTheta,
        float rmsNormEps)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, "design-conversion-dpo-policy-config-v1");
        AppendInt32(hash, vocabSize);
        AppendInt32(hash, contextLength);
        AppendInt32(hash, hiddenDim);
        AppendInt32(hash, numLayers);
        AppendInt32(hash, numHeads);
        AppendInt32(hash, numKvHeads);
        AppendInt32(hash, ffnHiddenDim);
        AppendInt32(hash, BitConverter.SingleToInt32Bits(ropeTheta));
        AppendInt32(hash, BitConverter.SingleToInt32Bits(rmsNormEps));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendString(IncrementalHash hash, string value)
    {
        var bytes = StrictUtf8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
