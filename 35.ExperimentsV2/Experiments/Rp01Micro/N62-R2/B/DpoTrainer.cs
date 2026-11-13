// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using TensorCore.Core;
using DPO.Models;
using LoRA;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace DPO;

public enum DpoTrainerArtifactKind
{
    ReservationLock,
    PolicySerialization,
    PolicyStaging,
    MetricsStaging,
    PolicyFinal,
    MetricsFinal
}

internal enum DpoReservationPlatform
{
    Windows,
    Unix
}

internal enum DpoReservationFailurePhase
{
    Create,
    Open,
    Lock
}

public enum DpoTrainerArtifactIdentityPlatform
{
    Windows = 1,
    Linux = 2
}

public readonly record struct DpoTrainerArtifactIdentity(
    DpoTrainerArtifactIdentityPlatform Platform,
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

public sealed record DpoTrainerArtifactOwnershipProof(
    int Step,
    DpoTrainerArtifactKind Kind,
    string Path,
    long Length,
    string Sha256,
    uint LinkCount,
    DpoTrainerArtifactIdentity Identity,
    DpoTrainerArtifactKind? TransitionFromKind = null,
    string? TransitionFromPath = null,
    DpoTrainerArtifactIdentity? TransitionFromIdentity = null);

// Прямая оптимизация предпочтений получает уже проверенные и упакованные пары:
// предпочтительный и отклонённый ответы представлены тензорами одного контекста.
// Проверка лицензии, происхождения данных и разделения обучения и оценки начинается
// выше по потоку, но низкоуровневый тренер держит последнюю дверь перед весами.
// Замороженная эталонная модель задаёт исходную точку, а градиенты получает только
// обучаемая модель. Изменять её веса и каталог результатов можно лишь при наличии
// действующего разрешения, выданного серверной границей.
// Direct Preference Optimisation тренер.
// Loss = -E[log σ(β · (log π(y_w|x) - log π(y_l|x) - log π_ref(y_w|x) + log π_ref(y_l|x)))]
// DNA rule 8: метрики сохраняются ДО инкремента счётчика шага.
public sealed record DpoTrainingOptimizerSettings(
    float LearningRate,
    float Beta1,
    float Beta2,
    float WeightDecay,
    float Epsilon)
{
    public bool IsWellFormed
        => float.IsFinite(LearningRate) && LearningRate > 0f
           && float.IsFinite(Beta1) && Beta1 >= 0f && Beta1 < 1f
           && float.IsFinite(Beta2) && Beta2 >= 0f && Beta2 < 1f
           && float.IsFinite(WeightDecay) && WeightDecay >= 0f
           && float.IsFinite(Epsilon) && Epsilon > 0f;
}

public sealed record DpoTrainingPublicationBundle(
    AdamWPortableState OptimizerState,
    LlmModelRandomStateSnapshot? RandomState,
    int OptimizerStateStep,
    int DpoStep,
    string TrialWeightsSha256 = "",
    long TrialWeightsByteLength = 0,
    DpoExecutionProfile? ExecutionProfile = null)
{
    public int Step => OptimizerStateStep;
}

public sealed record DpoRandomStateColdStartAuthorization(
    string AuthorityId,
    string Reason,
    ulong RootSeed)
{
    public bool IsWellFormed
        => !string.IsNullOrWhiteSpace(AuthorityId) &&
           !string.IsNullOrWhiteSpace(Reason) &&
           AuthorityId.Length <= 256 && Reason.Length <= 512 &&
           AuthorityId.IndexOfAny(['\r', '\n']) < 0 &&
           Reason.IndexOfAny(['\r', '\n']) < 0;
}

public sealed record DpoAllowedParameterIdentity(string Key, int ElementCount);

/// <summary>Closed identity of the validated execution, not caller-supplied labels.</summary>
public sealed class DpoExecutionProfile
{
    public const string MismatchErrorCode = "DPO_CONTINUATION_PROFILE_MISMATCH";
    private DpoExecutionProfile(string material, string framing, string objective,
        IEnumerable<DpoAllowedParameterIdentity> parameters)
    {
        MaterialVersion = material; FramingVersion = framing; ObjectiveVersion = objective;
        AllowedParameterMap = Array.AsReadOnly(parameters.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray());
        RequireCurrent();
    }
    public string MaterialVersion { get; }
    public string FramingVersion { get; }
    public string ObjectiveVersion { get; }
    public IReadOnlyList<DpoAllowedParameterIdentity> AllowedParameterMap { get; }
    internal static DpoExecutionProfile Capture(DpoBatch validatedBatch, IReadOnlyList<AdamWParameterBinding> bindings)
    {
        if (validatedBatch.MaterialVersion != DpoExactPairMaterial.Version || validatedBatch.ExactMaterial is null) Mismatch();
        return new(validatedBatch.MaterialVersion, DpoExactPairMaterial.FramingVersion,
            DpoExactPairMaterial.ObjectiveVersion, bindings.Select(x => new DpoAllowedParameterIdentity(x.Key, x.Parameter.Value.Data.Length)));
    }
    internal static DpoExecutionProfile FromVerifiedManifest(string material, string framing, string objective,
        IEnumerable<DpoAllowedParameterIdentity> parameters) => new(material, framing, objective, parameters);
    internal void RequireCurrent()
    {
        if (MaterialVersion != DpoExactPairMaterial.Version || FramingVersion != DpoExactPairMaterial.FramingVersion ||
            ObjectiveVersion != DpoExactPairMaterial.ObjectiveVersion || AllowedParameterMap.Count == 0 ||
            AllowedParameterMap.Any(x => x is null || string.IsNullOrWhiteSpace(x.Key) || x.ElementCount <= 0) ||
            AllowedParameterMap.Select(x => x.Key).Distinct(StringComparer.Ordinal).Count() != AllowedParameterMap.Count) Mismatch();
    }
    internal bool Matches(DpoExecutionProfile other) => MaterialVersion == other.MaterialVersion &&
        FramingVersion == other.FramingVersion && ObjectiveVersion == other.ObjectiveVersion && AllowedParameterMap.SequenceEqual(other.AllowedParameterMap);
    internal static void Mismatch() => throw new DpoMutationAdmissionException("The exact DPO continuation profile/source proof does not match execution.", MismatchErrorCode);
}

/// <summary>Issued only after canonical manifest/address/hash/readback validation.</summary>
public sealed class DpoContinuationProof
{
    private readonly byte[] _optimizerBytes;
    private readonly byte[]? _randomBytes;
    private readonly Func<bool> _sourceStillMatches;
    private DpoContinuationProof(DpoExecutionProfile profile, AdamWPortableState optimizer,
        LlmModelRandomStateSnapshot? random, string sourceIdentity, Func<bool> sourceStillMatches)
    {
        Profile = profile; SourceIdentity = sourceIdentity; _optimizerBytes = optimizer.ToBytes();
        _randomBytes = RandomBytes(random); _sourceStillMatches = sourceStillMatches;
    }
    public DpoExecutionProfile Profile { get; }
    public string SourceIdentity { get; }
    internal static DpoContinuationProof IssueVerified(DpoExecutionProfile profile, AdamWPortableState optimizer,
        LlmModelRandomStateSnapshot? random, string sourceIdentity, Func<bool> sourceStillMatches)
    {
        profile.RequireCurrent();
        if (string.IsNullOrWhiteSpace(sourceIdentity) || !sourceStillMatches()) DpoExecutionProfile.Mismatch();
        return new(profile, optimizer, random, sourceIdentity, sourceStillMatches);
    }
    public void RequireMatches(AdamWPortableState? optimizer, LlmModelRandomStateSnapshot? random)
    {
        Profile.RequireCurrent();
        var bytes = RandomBytes(random);
        if (optimizer is null || !CryptographicOperations.FixedTimeEquals(_optimizerBytes, optimizer.ToBytes()) ||
            (_randomBytes is null ? bytes is not null : bytes is null || !CryptographicOperations.FixedTimeEquals(_randomBytes, bytes)) ||
            !_sourceStillMatches()) DpoExecutionProfile.Mismatch();
    }
    internal void RequireProfile(DpoExecutionProfile profile)
    { if (!Profile.Matches(profile)) DpoExecutionProfile.Mismatch(); }
    private static byte[]? RandomBytes(LlmModelRandomStateSnapshot? state)
        => state is null ? null : System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(state);
}

/// <summary>Actual installed project-LoRA objects and their immutable canonical bindings.</summary>
internal sealed class DpoProjectLoraRecipient
{
    internal const string InvalidErrorCode = "DPO_PROJECT_LORA_RECIPIENT_INVALID";
    private readonly LlmModel _policy, _reference;
    private readonly LoraConfig _config;
    private readonly LoraLinear[] _adapters;
    private readonly byte[] _referenceHash;
    internal IReadOnlyList<AdamWParameterBinding> Bindings { get; }
    private DpoProjectLoraRecipient(LlmModel policy, LlmModel reference, IReadOnlyList<LoraLinear> adapters, LoraConfig config)
    {
        _policy = policy; _reference = reference; _adapters = adapters.ToArray();
        _config = LoraApplicator.ValidateAndCanonicalize(config);
        var allowed = _adapters.SelectMany(x => x.AdapterParameters).Select(x => x.Parameter).ToHashSet(ReferenceEqualityComparer.Instance);
        Bindings = Array.AsReadOnly(policy.GetCheckpointParameterEntries().Where(x => allowed.Contains(x.Parameter))
            .Select(x => new AdamWParameterBinding(x.Key, x.Parameter)).OrderBy(x => x.Key, StringComparer.Ordinal).ToArray());
        _referenceHash = HashReference(); RequireValid();
    }
    internal static DpoProjectLoraRecipient Capture(LlmModel policy, LlmModel reference, IReadOnlyList<LoraLinear> adapters, LoraConfig config)
        => new(policy, reference, adapters, config);
    internal static void RequirePreparedPolicy(LlmModel policy, LoraConfig config)
    {
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        if (canonical.TargetModules.Length == 0) Invalid();
        var slots = Enumerable.Range(0, policy.Blocks.Count).SelectMany(block => canonical.TargetModules
            .Select(target => policy.GetAttachedProjectionAdapter(block, target))).ToArray();
        if (slots.All(x => x is null))
        {
            if (policy.Parameters().Count() != policy.BaseParameters.Count) Invalid();
            return; // Explicit first preparation owns installing the absent topology.
        }
        if (slots.Any(x => x is not LoraLinear)) Invalid();
        var allowed = slots.Cast<LoraLinear>().SelectMany(x => x.AdapterParameters).Select(x => x.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance);
        var all = policy.GetCheckpointParameterEntries();
        if (allowed.Count != slots.Length * 2 || all.Count != policy.BaseParameters.Count + allowed.Count ||
            all.Any(x => x.Parameter.Value.RequiresGrad != allowed.Contains(x.Parameter)) ||
            policy.Parameters().Count() != all.Count) Invalid();
    }
    internal void RequireValid()
    {
        if (ReferenceEquals(_policy, _reference) || _policy.Config != _reference.Config || _adapters.Length == 0 ||
            _adapters.Distinct(ReferenceEqualityComparer.Instance).Count() != _adapters.Length ||
            _adapters.Length != _policy.Blocks.Count * _config.TargetModules.Length) Invalid();
        var installed = new HashSet<LoraLinear>(ReferenceEqualityComparer.Instance);
        for (var block = 0; block < _policy.Blocks.Count; block++)
            foreach (var target in _config.TargetModules)
            {
                if (_policy.GetAttachedProjectionAdapter(block, target) is not LoraLinear adapter || !_adapters.Contains(adapter) ||
                    !installed.Add(adapter) || adapter.Rank != _config.Rank || adapter.Alpha != _config.Alpha ||
                    adapter.DropoutProbability != _config.Dropout || adapter.AdapterParameters.Count != 2 ||
                    !adapter.AdapterParameters.Select(x => x.Suffix).SequenceEqual(new[] { "lora_a", "lora_b" })) Invalid();
            }
        var allowed = _adapters.SelectMany(x => x.AdapterParameters).Select(x => x.Parameter).ToHashSet(ReferenceEqualityComparer.Instance);
        var all = _policy.GetCheckpointParameterEntries();
        var bases = _policy.BaseParameters.ToHashSet(ReferenceEqualityComparer.Instance);
        if (allowed.Count != _adapters.Length * 2 || allowed.Overlaps(bases) || Bindings.Count != allowed.Count ||
            Bindings.Select(x => x.Key).Distinct(StringComparer.Ordinal).Count() != Bindings.Count ||
            all.Count != bases.Count + allowed.Count || _policy.Parameters().Count() != all.Count ||
            all.Select(x => x.Parameter).Distinct(ReferenceEqualityComparer.Instance).Count() != all.Count ||
            all.Any(x => !bases.Contains(x.Parameter) && !allowed.Contains(x.Parameter)) ||
            all.Any(x => x.Parameter.Value.RequiresGrad != allowed.Contains(x.Parameter)) ||
            Bindings.Any(x => !all.Any(entry => entry.Key == x.Key && ReferenceEquals(entry.Parameter, x.Parameter))) ||
            _reference.Parameters().Any(x => x.Value.RequiresGrad) || !CryptographicOperations.FixedTimeEquals(_referenceHash, HashReference())) Invalid();
        var reference = _reference.GetCheckpointParameterEntries();
        if (reference.Count != all.Count || _reference.Parameters().Count() != reference.Count) Invalid();
        foreach (var entry in all)
        {
            var other = reference.SingleOrDefault(x => x.Key == entry.Key);
            if (other is null || ReferenceEquals(entry.Parameter.Value.Data, other.Parameter.Value.Data) ||
                !entry.Parameter.Value.Shape.Dimensions.SequenceEqual(other.Parameter.Value.Shape.Dimensions) ||
                (bases.Contains(entry.Parameter) && !entry.Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits)
                    .SequenceEqual(other.Parameter.Value.Data.Select(BitConverter.SingleToInt32Bits)))) Invalid();
        }
    }
    internal void RequireAllowedState(AdamWPortableState state, bool validateRecipient = true)
    {
        if (validateRecipient) RequireValid();
        foreach (var entry in state.Entries)
        {
            var binding = Bindings.SingleOrDefault(x => x.Key == entry.Key);
            if (binding is null || !entry.Dimensions.SequenceEqual(binding.Parameter.Value.Shape.Dimensions) ||
                entry.FirstMoment.Count != binding.Parameter.Value.Data.Length || entry.SecondMoment.Count != entry.FirstMoment.Count) Invalid();
        }
    }
    private byte[] HashReference()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> buffer = stackalloc byte[1024];
        var encoding = System.Text.Encoding.UTF8;
        var encoder = encoding.GetEncoder();
        // Preserve BinaryWriter's private encoding: canonical entry order,
        // 7-bit UTF8 byte length + key, then little-endian shapes/float bits.
        foreach (var entry in _reference.GetCheckpointParameterEntries())
        {
            var remaining = (uint)encoding.GetByteCount(entry.Key);
            var prefixLength = 0;
            while (remaining >= 0x80)
            {
                buffer[prefixLength++] = (byte)((remaining & 0x7f) | 0x80);
                remaining >>= 7;
            }
            buffer[prefixLength++] = (byte)remaining;
            hash.AppendData(buffer[..prefixLength]);

            encoder.Reset();
            var characters = entry.Key.AsSpan();
            bool completed;
            do
            {
                encoder.Convert(characters, buffer, flush: true,
                    out var charactersUsed, out var bytesUsed, out completed);
                hash.AppendData(buffer[..bytesUsed]);
                characters = characters[charactersUsed..];
            } while (!completed);

            foreach (var dimension in entry.Parameter.Value.Shape.Dimensions)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(buffer, dimension);
                hash.AppendData(buffer[..sizeof(int)]);
            }
            var values = entry.Parameter.Value.Data;
            for (var offset = 0; offset < values.Length;)
            {
                var count = Math.Min(buffer.Length / sizeof(float), values.Length - offset);
                for (var i = 0; i < count; i++)
                    System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(
                        buffer.Slice(i * sizeof(float), sizeof(float)), values[offset + i]);
                hash.AppendData(buffer[..(count * sizeof(float))]);
                offset += count;
            }
        }
        return hash.GetHashAndReset();
    }
    private static void Invalid() => throw new DpoMutationAdmissionException("The actual project-LoRA recipient, active objects or frozen base/reference is invalid.", InvalidErrorCode);
}

public class DpoTrainer
{
    private const float Beta = 0.1f;  // коэффициент KL-штрафа
    public const string PolicyCheckpointFilePrefix = "dpo_policy_step_";
    public const string PolicyCheckpointFileExtension = ".bin";
    public const string MetricsFilePrefix = "dpo_step_";
    public const string MetricsFileExtension = ".json";
    public const string CheckpointCollisionErrorCode = "DPO_CHECKPOINT_COLLISION";
    public const string CheckpointIncompleteErrorCode = "DPO_CHECKPOINT_INCOMPLETE";
    public const string CheckpointReservationActiveErrorCode = "DPO_CHECKPOINT_RESERVATION_ACTIVE";
    public const string CheckpointReservationCreateFailedErrorCode = "DPO_RESERVATION_CREATE_FAILED";
    public const string CheckpointReservationLockFailedErrorCode = "DPO_RESERVATION_LOCK_FAILED";
    public const string CheckpointReservationReleaseFailedErrorCode = "DPO_RESERVATION_RELEASE_FAILED";
    public const string CheckpointPublicationCleanupFailedErrorCode = "DPO_PUBLICATION_CLEANUP_FAILED";
    public const string StepRollbackFailedErrorCode = "DPO_STEP_ROLLBACK_FAILED";
    public const string ContinuationRestoreRollbackFailedErrorCode =
        "DPO_CONTINUATION_RESTORE_ROLLBACK_FAILED";
    public const string ContinuationRestoreOptimizerRollbackFailedErrorCode =
        "DPO_CONTINUATION_RESTORE_OPTIMIZER_ROLLBACK_FAILED";
    public const string ContinuationRestoreRandomRollbackFailedErrorCode =
        "DPO_CONTINUATION_RESTORE_RANDOM_ROLLBACK_FAILED";
    public const string PolicyMutationActiveErrorCode = "DPO_POLICY_MUTATION_ACTIVE";

    private readonly LlmModel _policy;
    private readonly LlmModel _reference;
    private readonly AdamW    _optimizer;
    private readonly string   _checkpointDir;
    private readonly float    _learningRate;
    private readonly DpoTrainerPublicationObserver? _publicationObserver;
    private readonly Func<int, LlmModelRandomStateSnapshot>? _captureRandomState;
    private readonly Action<LlmModelRandomStateSnapshot>? _restoreRandomState;
    private readonly DpoMutationAdmissionCapability? _mutationAdmission;
    private readonly bool _deferCheckpointDirectory;
    private readonly DpoProjectLoraRecipient? _projectRecipient;
    private readonly DpoExecutionProfile? _executionProfile;
    private readonly DpoContinuationProof? _continuationProof;

    public const float DefaultLearningRate = 1e-5f;
    public const float DefaultWeightDecay = 0.01f;

    public static DpoTrainingOptimizerSettings ProductionOptimizerSettings
    {
        get
        {
            var optimizer = new AdamW(weightDecay: DefaultWeightDecay);
            return new DpoTrainingOptimizerSettings(
                DefaultLearningRate,
                optimizer.Beta1,
                optimizer.Beta2,
                optimizer.WeightDecay,
                optimizer.Epsilon);
        }
    }

    public DpoTrainingOptimizerSettings OptimizerSettings
        => new(
            _learningRate,
            _optimizer.Beta1,
            _optimizer.Beta2,
            _optimizer.WeightDecay,
            _optimizer.Epsilon);
    private static readonly ConcurrentDictionary<string, byte> ActiveReservationGates = new(
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<LlmModel, object> ActivePolicyMutationGates = new(
        ReferenceEqualityComparer.Instance);

    // Конструкторы без серверного разрешения оставляют тренер пригодным для чтения
    // и оценки, но любая попытка изменить веса завершится до первой мутации.
    // Сам тренер не может выдать себе разрешение и тем самым обойти внешний допуск.
    public DpoTrainer(LlmModel policy, LlmModel reference, string checkpointDir, float learningRate = 1e-5f)
        : this(policy, reference, checkpointDir, learningRate, publicationObserver: null)
    {
    }

    public DpoTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        float learningRate,
        DpoTrainerPublicationObserver? publicationObserver)
    {
        _policy        = policy;
        _reference     = reference;
        _optimizer     = new AdamW(weightDecay: DefaultWeightDecay);
        _checkpointDir = checkpointDir;
        _learningRate  = learningRate;
        _publicationObserver = publicationObserver;
        _mutationAdmission = null;
        _captureRandomState = null;
        _restoreRandomState = null;
    }

    public DpoTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        DpoMutationAdmissionCapability mutationAdmission,
        float learningRate = 1e-5f,
        DpoTrainerPublicationObserver? publicationObserver = null,
        Func<int, LlmModelRandomStateSnapshot>? captureRandomState = null,
        Action<LlmModelRandomStateSnapshot>? restoreRandomState = null)
        : this(policy, reference, checkpointDir, mutationAdmission, learningRate,
            publicationObserver, captureRandomState, restoreRandomState,
            deferCheckpointDirectory: false)
    {
    }

    // Prepared continuation owns no output until preparation hands off to an
    // admitted first step. Ordinary admitted construction remains eager.
    internal static DpoTrainer CreateForPreparedContinuation(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        DpoMutationAdmissionCapability mutationAdmission,
        float learningRate,
        DpoTrainerPublicationObserver? publicationObserver,
        Func<int, LlmModelRandomStateSnapshot>? captureRandomState,
        Action<LlmModelRandomStateSnapshot>? restoreRandomState)
        => new(policy, reference, checkpointDir, mutationAdmission, learningRate,
            publicationObserver, captureRandomState, restoreRandomState,
            deferCheckpointDirectory: true);

    internal static DpoTrainer CreateForProjectLora(LlmModel policy, LlmModel reference, string checkpointDir,
        DpoMutationAdmissionCapability mutationAdmission, DpoProjectLoraRecipient recipient, DpoExecutionProfile profile,
        DpoContinuationProof? proof, DpoTrainerPublicationObserver? observer,
        Func<int, LlmModelRandomStateSnapshot>? captureRandomState, Action<LlmModelRandomStateSnapshot>? restoreRandomState)
        => new(policy, reference, checkpointDir, mutationAdmission, DefaultLearningRate, observer,
            captureRandomState, restoreRandomState, true, recipient, profile, proof);

    private DpoTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        DpoMutationAdmissionCapability mutationAdmission,
        float learningRate,
        DpoTrainerPublicationObserver? publicationObserver,
        Func<int, LlmModelRandomStateSnapshot>? captureRandomState,
        Action<LlmModelRandomStateSnapshot>? restoreRandomState,
        bool deferCheckpointDirectory,
        DpoProjectLoraRecipient? recipient = null,
        DpoExecutionProfile? profile = null,
        DpoContinuationProof? proof = null)
    {
        ArgumentNullException.ThrowIfNull(mutationAdmission);
        _policy        = policy;
        _reference     = reference;
        _optimizer     = new AdamW(weightDecay: DefaultWeightDecay);
        _checkpointDir = checkpointDir;
        _learningRate  = learningRate;
        _publicationObserver = publicationObserver;
        _mutationAdmission = mutationAdmission;
        _captureRandomState = captureRandomState;
        _restoreRandomState = restoreRandomState;
        _deferCheckpointDirectory = deferCheckpointDirectory;
        _projectRecipient = recipient; _executionProfile = profile; _continuationProof = proof;
        recipient?.RequireValid();
        profile?.RequireCurrent();
        if (proof is not null && profile is not null) proof.RequireProfile(profile);

        // Проверяем выданное сервером разрешение до создания каталога результатов:
        // отсутствующее или отозванное разрешение останавливает путь раньше любого
        // изменения файловой системы или весов модели.
        using var permit = AcquireMutationPermit();
        if (!deferCheckpointDirectory)
            Directory.CreateDirectory(checkpointDir);
    }

    /// <summary>
    /// Отдаёт память оптимизатора этого тренера в переносимом виде, привязанном к
    /// именам параметров контрольной точки обучаемой модели.
    ///
    /// Зачем это нужно продукту: файл весов сохраняет только сами веса, поэтому
    /// после перезапуска процесса оптимизатор начинался бы с нуля и урок
    /// продолжался бы по другой траектории обучения. Экспорт позволяет положить
    /// накопленные оценки градиентов и счётчик шага рядом с пробными весами.
    ///
    /// Это не запись на диск и не доказательство качества обучения: вызывающая
    /// сторона сама решает, куда и по каким правилам сохранять эти байты.
    /// </summary>
    public AdamWPortableState ExportOptimizerState()
    {
        if (_projectRecipient is not null)
        {
            var state = _optimizer.ExportPortableState(_projectRecipient.Bindings);
            _projectRecipient.RequireAllowedState(state);
            return state;
        }
        var checkpointEntries = _policy.GetCheckpointParameterEntries();
        var bindings = new AdamWParameterBinding[checkpointEntries.Count];
        for (var index = 0; index < checkpointEntries.Count; index++)
        {
            var entry = checkpointEntries[index];
            bindings[index] = new AdamWParameterBinding(entry.Key, entry.Parameter);
        }

        return _optimizer.ExportPortableState(bindings);
    }

    /// <summary>
    /// Переносит ранее сохранённую память оптимизатора в этот тренер. Возвращает
    /// false и ничего не меняет, если имена параметров не разрешаются в текущей
    /// модели или формы не совпадают: продолжать урок с чужой или неполной
    /// памятью оптимизатора нельзя, и молчаливый возврат к свежему оптимизатору
    /// запрещён.
    /// </summary>
    public bool TryRestoreOptimizerState(AdamWPortableState? portableState)
    {
        if (_projectRecipient is not null && portableState is not null)
        {
            if (_continuationProof is null) DpoExecutionProfile.Mismatch();
            _continuationProof!.RequireMatches(portableState, null);
        }
        return TryImportOptimizerState(portableState);
    }

    // Existing preparation rollback restores a locally captured optimizer, not
    // an external continuation. It must also work while restoring caller flags.
    internal bool TryRestorePreparationOptimizerState(AdamWPortableState? snapshot)
    {
        if (_projectRecipient is null) return TryImportOptimizerState(snapshot);
        if (snapshot is null) return false;
        _projectRecipient.RequireAllowedState(snapshot, validateRecipient: false);
        return _optimizer.TryImportPortableState(_projectRecipient.Bindings, snapshot);
    }

    private bool TryImportOptimizerState(AdamWPortableState? portableState)
    {
        if (portableState is null)
            return false;
        if (_projectRecipient is not null)
        {
            _projectRecipient.RequireAllowedState(portableState);
            return _optimizer.TryImportPortableState(_projectRecipient.Bindings, portableState);
        }

        var checkpointEntries = _policy.GetCheckpointParameterEntries();
        var bindings = new AdamWParameterBinding[checkpointEntries.Count];
        for (var index = 0; index < checkpointEntries.Count; index++)
        {
            var entry = checkpointEntries[index];
            bindings[index] = new AdamWParameterBinding(entry.Key, entry.Parameter);
        }

        return _optimizer.TryImportPortableState(bindings, portableState);
    }

    public bool TryRestoreContinuationState(
        LlmModelRandomStateSnapshot? randomState,
        AdamWPortableState? optimizerState)
    {
        if (_projectRecipient is not null && optimizerState is null &&
            (randomState is not null || _continuationProof is not null))
            DpoExecutionProfile.Mismatch();
        if (_projectRecipient is not null && optimizerState is not null)
        {
            _projectRecipient.RequireAllowedState(optimizerState);
            if (_continuationProof is null) DpoExecutionProfile.Mismatch();
            _continuationProof!.RequireMatches(optimizerState, randomState);
        }
        var optimizerStateBeforeRestore = ExportOptimizerState();
        LlmModelRandomStateSnapshot? randomStateBeforeRestore = null;
        if (randomState is not null)
        {
            if (_captureRandomState is null || _restoreRandomState is null)
                throw new InvalidOperationException("DPO_RANDOM_STATE_RESTORE_SEAM_MISSING");

            randomStateBeforeRestore =
                _captureRandomState(optimizerStateBeforeRestore.Step);
        }

        try
        {
            if (randomState is not null)
                _restoreRandomState!(randomState);

            if (optimizerState is null || TryImportOptimizerState(optimizerState))
                return true;
        }
        catch (Exception exception)
        {
            try
            {
                RestoreContinuationState(
                    optimizerStateBeforeRestore,
                    randomStateBeforeRestore);
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException(
                    ContinuationRestoreRollbackFailedErrorCode,
                    exception,
                    rollbackException);
            }

            throw;
        }

        RestoreContinuationState(
            optimizerStateBeforeRestore,
            randomStateBeforeRestore);
        return false;
    }

    private void RestoreContinuationState(
        AdamWPortableState optimizerState,
        LlmModelRandomStateSnapshot? randomState)
    {
        if (!TryImportOptimizerState(optimizerState))
        {
            throw new InvalidOperationException(
                ContinuationRestoreOptimizerRollbackFailedErrorCode);
        }

        if (randomState is not null)
        {
            if (_restoreRandomState is null)
                throw new InvalidOperationException(
                    ContinuationRestoreRandomRollbackFailedErrorCode);
            _restoreRandomState(randomState);
        }
    }

    public DpoMetrics Step(DpoBatch batch, int step)
    {
        batch = CaptureBatch(batch);
        // Admission пары проверяется до server mutation permit, lock-а, forward-а
        // и публикации: плохая пара не должна даже просить право менять веса.
        RequirePairAdmission(batch);
        using var mutationPermit = AcquireMutationPermit();
        return ExecuteWithPublication(step, publication =>
        {

        // Reference log-probs: модель заморожена, градиент не нужен
        float refChosenLogP   = BatchLogProbs(_reference, batch, chosen: true);
        float refRejectedLogP = BatchLogProbs(_reference, batch, chosen: false);

        // Policy log-probs: обучаемая модель, нужен backward
        var polChosen = ForwardPolicyBatchLogProbs(_policy, batch, chosen: true);
        var polRejected = ForwardPolicyBatchLogProbs(_policy, batch, chosen: false);

        // DPO margin: насколько политика предпочитает chosen над rejected относительно reference
        float chosenReward   = Beta * (polChosen.LogProbability   - refChosenLogP);
        float rejectedReward = Beta * (polRejected.LogProbability - refRejectedLogP);
        float margin         = chosenReward - rejectedReward;

        var objective = ObjectiveTerms(batch, margin);
        float loss = objective.Loss;

        // Ручной backward: d(-log σ(m))/dm = -(1 - σ(m)) = σ(m) - 1
        float gradMargin = objective.Derivative;
        BackwardLogProbs(polChosen,  gradMargin * Beta);
        BackwardLogProbs(polRejected, -gradMargin * Beta);

        ApplyOptimizer();
        _policy.ZeroGrad();

        var metrics = new DpoMetrics
        {
            Step           = step,
            Loss           = loss,
            ChosenReward   = chosenReward,
            RejectedReward = rejectedReward,
            RewardMargin   = margin,
            Accuracy       = margin > 0 ? 1f : 0f,
            ObjectiveVersion = IsExact(batch) ? DpoExactPairMaterial.ObjectiveVersion : null
        };

        // Metrics are the commit marker and are published only after the policy file.
        publication.Publish(_policy, metrics, BuildPublicationBundle(step));
        return metrics;
        });
    }

    public DpoMetrics StepMany(IReadOnlyList<DpoBatch> batches, int step)
    {
        if (batches.Count == 0)
            throw new ArgumentException("At least one DPO batch is required.", nameof(batches));
        // Весь список проверяется до первой мутации: валидная первая пара не
        // получает частичный optimizer step, если более поздняя пара без паспорта.
        batches = batches.Select(CaptureBatch).ToArray();
        RequirePairAdmissions(batches);
        if (batches.Count == 1)
            return Step(batches[0], step);

        using var mutationPermit = AcquireMutationPermit();
        return ExecuteWithPublication(step, publication =>
        {

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;
        var gradientScale = 1f / batches.Count;

        foreach (var batch in batches)
        {
            float refChosenLogP   = BatchLogProbs(_reference, batch, chosen: true);
            float refRejectedLogP = BatchLogProbs(_reference, batch, chosen: false);
            var polChosen = ForwardPolicyBatchLogProbs(_policy, batch, chosen: true);
            var polRejected = ForwardPolicyBatchLogProbs(_policy, batch, chosen: false);

            float chosenReward   = Beta * (polChosen.LogProbability   - refChosenLogP);
            float rejectedReward = Beta * (polRejected.LogProbability - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            var objective        = ObjectiveTerms(batch, margin);
            float loss           = objective.Loss;
            float gradMargin     = objective.Derivative * gradientScale;

            BackwardLogProbs(polChosen, gradMargin * Beta);
            BackwardLogProbs(polRejected, -gradMargin * Beta);

            lossSum += loss;
            chosenRewardSum += chosenReward;
            rejectedRewardSum += rejectedReward;
            marginSum += margin;
            accuracySum += margin > 0 ? 1f : 0f;
        }

        ApplyOptimizer();
        _policy.ZeroGrad();

        var count = batches.Count;
        var metrics = new DpoMetrics
        {
            Step = step,
            Loss = lossSum / count,
            ChosenReward = chosenRewardSum / count,
            RejectedReward = rejectedRewardSum / count,
            RewardMargin = marginSum / count,
            Accuracy = accuracySum / count,
            ObjectiveVersion = HomogeneousObjectiveVersion(batches)
        };

        publication.Publish(_policy, metrics, BuildPublicationBundle(step));
        return metrics;
        });
    }

    public DpoMetrics EvaluateMany(IReadOnlyList<DpoBatch> batches, int step)
    {
        if (batches.Count == 0)
            throw new ArgumentException("At least one DPO batch is required.", nameof(batches));
        batches = batches.Select(CaptureBatch).ToArray();

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;

        foreach (var batch in batches)
        {
            float refChosenLogP   = BatchLogProbs(_reference, batch, chosen: true);
            float refRejectedLogP = BatchLogProbs(_reference, batch, chosen: false);
            float polChosenLogP   = BatchLogProbs(_policy, batch, chosen: true);
            float polRejectedLogP = BatchLogProbs(_policy, batch, chosen: false);

            float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
            float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            float loss           = ObjectiveTerms(batch, margin).Loss;

            lossSum += loss;
            chosenRewardSum += chosenReward;
            rejectedRewardSum += rejectedReward;
            marginSum += margin;
            accuracySum += margin > 0 ? 1f : 0f;
        }

        var count = batches.Count;
        return new DpoMetrics
        {
            Step = step,
            Loss = lossSum / count,
            ChosenReward = chosenRewardSum / count,
            RejectedReward = rejectedRewardSum / count,
            RewardMargin = marginSum / count,
            Accuracy = accuracySum / count,
            ObjectiveVersion = HomogeneousObjectiveVersion(batches)
        };
    }

    public DpoMetrics StepVector(IReadOnlyList<DpoVectorBatch> batches, int step)
        => StepVectorCore(batches, step, timing: null);

    /// <summary>Same transactional step with optional in-process phase observation, separate from DpoMetrics.</summary>
    public DpoMetrics StepVector(IReadOnlyList<DpoVectorBatch> batches, int step, DpoStepVectorTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);
        timing.Begin(step);
        var returned = false;
        try
        {
            var metrics = StepVectorCore(batches, step, timing);
            returned = true;
            return metrics;
        }
        finally
        {
            timing.End(returned);
        }
    }

    private DpoMetrics StepVectorCore(IReadOnlyList<DpoVectorBatch> batches, int step, DpoStepVectorTiming? timing)
    {
        // Пустой вход не имеет права публиковать шаг, который не увидел ни одной
        // пары предпочтений и потому не мог чему-либо обучить модель.
        if (batches.Count == 0)
            throw new ArgumentException("At least one vector DPO batch is required.", nameof(batches));

        // Векторный путь несет admission внутри DpoVectorBatch.Batch; проверяем
        // все вложенные batch-и до permit, веса teaching-vector и публикации.
        batches = batches.Select(vectorBatch =>
        {
            ArgumentNullException.ThrowIfNull(vectorBatch);
            ArgumentNullException.ThrowIfNull(vectorBatch.TeachingVector);
            return new DpoVectorBatch(CaptureBatch(vectorBatch.Batch),
                vectorBatch.TeachingVector.ToArray(), vectorBatch.Weight);
        }).ToArray();
        RequireVectorPairAdmissions(batches);

        // Живое серверное разрешение получаем до резервирования имени, прямого
        // прохода, градиента, изменения весов и записи файлов результата.
        using var mutationPermit = AcquireMutationPermit();
        return ExecuteWithPublication(step, publication =>
        {

        float lossSum = 0f;
        float chosenRewardSum = 0f;
        float rejectedRewardSum = 0f;
        float marginSum = 0f;
        float accuracySum = 0f;
        // Вес каждой уже допущенной пары ограничивается снизу и нормализуется по
        // набору. Это относительная сила вклада, а не доказательство качества,
        // лицензии или происхождения самой пары.
        var totalWeight = Math.Max(1e-6f, batches.Sum(batch => Math.Max(0.01f, batch.Weight)));

        foreach (var vectorBatch in batches)
        {
            var batch = vectorBatch.Batch;
            var vectorWeight = Math.Max(0.01f, vectorBatch.Weight) / totalWeight;
            // Обучающий вектор только масштабирует вклад пары, которую уже приняли
            // выше по потоку; он не заменяет решение о допуске данных к обучению.
            var vectorSignal = VectorSignal(vectorBatch.TeachingVector);
            var scaledWeight = vectorWeight * vectorSignal;

            // Эталонная модель оценивает оба ответа без графа градиентов. Обучаемая
            // модель строит отдельные графы для предпочтительного и отклонённого
            // ответов, чтобы менять только собственные параметры.
            var phaseStart = timing?.ReferenceForward.Begin() ?? 0;
            float refChosenLogP   = BatchLogProbs(_reference, batch, chosen: true);
            timing?.ReferenceForward.Complete(phaseStart);
            phaseStart = timing?.ReferenceForward.Begin() ?? 0;
            float refRejectedLogP = BatchLogProbs(_reference, batch, chosen: false);
            timing?.ReferenceForward.Complete(phaseStart);
            phaseStart = timing?.PolicyForward.Begin() ?? 0;
            var polChosen = ForwardPolicyBatchLogProbs(_policy, batch, chosen: true);
            timing?.PolicyForward.Complete(phaseStart);
            phaseStart = timing?.PolicyForward.Begin() ?? 0;
            var polRejected = ForwardPolicyBatchLogProbs(_policy, batch, chosen: false);
            timing?.PolicyForward.Complete(phaseStart);

            float chosenReward   = Beta * (polChosen.LogProbability   - refChosenLogP);
            float rejectedReward = Beta * (polRejected.LogProbability - refRejectedLogP);
            float margin         = chosenReward - rejectedReward;
            var objective        = ObjectiveTerms(batch, margin);
            float loss           = objective.Loss;
            float gradMargin     = objective.Derivative * scaledWeight;

            // Противоположные знаки градиента усиливают предпочтительный ответ и
            // ослабляют отклонённый относительно неизменной эталонной модели.
            phaseStart = timing?.Backward.Begin() ?? 0;
            BackwardLogProbs(polChosen, gradMargin * Beta);
            timing?.Backward.Complete(phaseStart);
            phaseStart = timing?.Backward.Begin() ?? 0;
            BackwardLogProbs(polRejected, -gradMargin * Beta);
            timing?.Backward.Complete(phaseStart);

            // Все пары накапливают вклад в один общий шаг оптимизатора; ни одна
            // пара не публикует промежуточную версию модели самостоятельно.
            lossSum += loss * (IsExact(batch) ? scaledWeight : vectorWeight);
            chosenRewardSum += chosenReward * vectorWeight;
            rejectedRewardSum += rejectedReward * vectorWeight;
            marginSum += margin * vectorWeight;
            accuracySum += (margin > 0 ? 1f : 0f) * vectorWeight;
            timing?.CompleteVectorBatch();
        }

        // Оптимизатор вызывается один раз для всего набора и видит только параметры,
        // которым явно разрешён градиент; этим замороженные параметры защищены от
        // случайного обновления вместе с обучаемой моделью.
        var optimizerStart = timing?.ApplyOptimizer.Begin() ?? 0;
        ApplyOptimizer();
        timing?.ApplyOptimizer.Complete(optimizerStart);
        _policy.ZeroGrad();

        // Эти метрики описывают только текущий обучающий набор. Они не доказывают
        // качество на отложенных примерах и не заменяют последующую оценку.
        var metrics = new DpoMetrics
        {
            Step = step,
            Loss = lossSum,
            ChosenReward = chosenRewardSum,
            RejectedReward = rejectedRewardSum,
            RewardMargin = marginSum,
            Accuracy = accuracySum,
            ObjectiveVersion = HomogeneousObjectiveVersion(batches.Select(vectorBatch => vectorBatch.Batch))
        };

        // Веса и метрики публикуются как связанная пара; финальный файл метрик
        // появляется последним и тем самым отмечает полностью завершённый шаг.
        publication.Publish(_policy, metrics, BuildPublicationBundle(step));
        return metrics;
        });
    }

    public static string BuildPolicyCheckpointFileName(int step)
        => $"{PolicyCheckpointFilePrefix}{step:D6}{PolicyCheckpointFileExtension}";

    public static string BuildMetricsFileName(int step)
        => $"{MetricsFilePrefix}{step:D6}{MetricsFileExtension}";

    internal static bool TryDeleteFinalArtifactIfOwned(
        DpoTrainerArtifactOwnershipProof proof)
        => CheckpointPublication.TryDeleteProofMatchedForTransaction(proof);

    // Средний log-prob модели по невыровненным позициям
    public static float EvaluateAverageNextTokenLogProbability(
        LlmModel model,
        Tensor ids,
        Tensor mask,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(mask);

        if (ids.Shape.Rank != 2 ||
            mask.Shape.Rank != 2 ||
            ids.Shape.Dimensions[0] != 1 ||
            mask.Shape.Dimensions[0] != 1 ||
            ids.Shape.Dimensions[1] != mask.Shape.Dimensions[1] ||
            ids.Data.Length != mask.Data.Length)
        {
            throw new ArgumentException("DPO inference ids and mask must be [1, sequence] tensors with matching lengths.");
        }

        var logits = model.Forward(ids, training: false, cancellationToken);
        if (logits.Shape.Rank != 3 ||
            logits.Shape.Dimensions[0] != 1 ||
            logits.Shape.Dimensions[1] != ids.Shape.Dimensions[1] ||
            logits.Shape.Dimensions[2] <= 0)
        {
            throw new ArgumentException("DPO inference logits must be [1, sequence, vocab].");
        }

        return ComputeLogProbs(logits, ids, mask);
    }

    private static float LogProbs(LlmModel model, Tensor ids, Tensor mask)
        => ComputeLogProbs(model.Forward(ids, training: false), ids, mask);

    private DpoBatch CaptureBatch(DpoBatch batch)
    {
        var captured = DpoExactPairMaterial.CaptureAndValidate(batch,
            Math.Min(_policy.Config.VocabSize, _reference.Config.VocabSize),
            Math.Min(_policy.Config.ContextLen, _reference.Config.ContextLen));
        if (_executionProfile is not null &&
            !_executionProfile.Matches(DpoExecutionProfile.Capture(captured, _projectRecipient!.Bindings)))
            DpoExecutionProfile.Mismatch();
        return captured;
    }

    private static Tensor TargetMask(DpoBatch batch, bool chosen)
        => batch.MaterialVersion == DpoExactPairMaterial.Version
            ? (chosen ? batch.ChosenResponseMask! : batch.RejectedResponseMask!)
            : (chosen ? batch.ChosenMask : batch.RejectedMask);

    private static float BatchLogProbs(LlmModel model, DpoBatch batch, bool chosen)
    {
        var ids = chosen ? batch.ChosenIds : batch.RejectedIds;
        return ComputeTargetLogProbs(model.Forward(ids, training: false), ids,
            TargetMask(batch, chosen), batch.MaterialVersion == DpoExactPairMaterial.Version);
    }

    private static PolicyLogProbGraph ForwardPolicyBatchLogProbs(LlmModel model, DpoBatch batch, bool chosen)
        => ForwardPolicyLogProbs(model, chosen ? batch.ChosenIds : batch.RejectedIds,
            TargetMask(batch, chosen), batch.MaterialVersion == DpoExactPairMaterial.Version);

    private static PolicyLogProbGraph ForwardPolicyLogProbs(
        LlmModel model,
        Tensor ids,
        Tensor mask,
        bool sumTargets = false)
    {
        var logits = model.Forward(ids, training: true);
        return new PolicyLogProbGraph(
            logits,
            ids,
            mask,
            ComputeTargetLogProbs(logits, ids, mask, sumTargets),
            sumTargets);
    }

    private static float ComputeLogProbs(Tensor logits, Tensor ids, Tensor mask)
        => ComputeTargetLogProbs(logits, ids, mask, sumTargets: false);

    private static float ComputeTargetLogProbs(Tensor logits, Tensor ids, Tensor mask, bool sumTargets)
    {
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        float sum  = 0f;
        float cnt  = 0f;

        // Сдвиг на 1: предсказываем ids[t+1] по контексту до t
        for (int i = 0; i < t - 1; i++)
        {
            float maskVal = mask.Data[i + 1];
            if (maskVal == 0f) continue;

            int targetId = (int)ids.Data[i + 1];

            // LogSoftmax строки i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal)
                    maxVal = logits.Data[i * v + j];

            float logSumExp = 0f;
            for (int j = 0; j < v; j++)
                logSumExp += MathF.Exp(logits.Data[i * v + j] - maxVal);
            logSumExp = MathF.Log(logSumExp);

            float logP = logits.Data[i * v + targetId] - maxVal - logSumExp;
            sum += logP * maskVal;
            cnt += maskVal;
        }

        return sumTargets ? sum : cnt > 0f ? sum / cnt : 0f;
    }

    // Упрощённый backward для log-probs (аккумулируем градиент в logits)
    private static void BackwardLogProbs(PolicyLogProbGraph graph, float upstream)
    {
        var logits = graph.Logits;
        var ids = graph.Ids;
        var mask = graph.Mask;
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        float cnt = 0f;
        for (int i = 0; i < t - 1; i++)
            cnt += mask.Data[i + 1];
        if (cnt <= 0f) return;

        float scale = graph.SumTargets ? upstream : upstream / cnt;

        // Опыт N62-R2, единственный фактор: один локальный массив softmax на вызов
        // backward вместо нового массива на каждой активной целевой позиции. Массив
        // локален для этого вызова, поэтому у chosen и у rejected он свой: общего
        // массива на две стороны нет. Все v элементов ниже перезаписываются целиком
        // на каждой позиции, поэтому значения между позициями не переносятся. При
        // cnt == 0 метод уже вернулся выше, то есть массив по-прежнему не создаётся.
        var expVals = new float[v];

        for (int i = 0; i < t - 1; i++)
        {
            if (mask.Data[i + 1] == 0f) continue;
            float targetScale = scale * mask.Data[i + 1];

            int targetId = (int)ids.Data[i + 1];

            // Softmax строки i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal) maxVal = logits.Data[i * v + j];

            float expSum = 0f;
            for (int j = 0; j < v; j++)
            {
                expVals[j] = MathF.Exp(logits.Data[i * v + j] - maxVal);
                expSum    += expVals[j];
            }

            // The supplied upstream value is dLoss/d(log-probability), so chain it with
            // d(log-softmax(target))/d(logit_j) = oneHot(target)_j - softmax_j.
            for (int j = 0; j < v; j++)
            {
                float sm = expVals[j] / expSum;
                float oh = j == targetId ? 1f : 0f;
                logits.Grad.Data[i * v + j] += (oh - sm) * targetScale;
            }
        }

        logits.Backward(logits.Grad);
    }

    private static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-x));

    private static bool IsExact(DpoBatch batch)
        => batch.MaterialVersion == DpoExactPairMaterial.Version;

    private static string? HomogeneousObjectiveVersion(IEnumerable<DpoBatch> batches)
        => batches.All(IsExact) ? DpoExactPairMaterial.ObjectiveVersion : null;

    private static (float Loss, float Derivative) ObjectiveTerms(DpoBatch batch, float margin)
        => IsExact(batch) ? ExactObjective(margin)
            : (-MathF.Log(Sigmoid(margin) + 1e-8f), Sigmoid(margin) - 1f);

    private static (float Loss, float Derivative) ExactObjective(float margin)
    {
        var tail = MathF.Exp(-MathF.Abs(margin));
        var loss = MathF.Max(0f, -margin) + LogOnePlusPositive(tail);
        // Avoid subtracting one from a sigmoid rounded to one at positive margins.
        var derivative = margin >= 0f ? -tail / (1f + tail) : -1f / (1f + tail);
        return (loss, derivative);
    }

    // z lies in [0,1]. Compensate rounding in 1+z using double arithmetic;
    // when even double loses z, log1p(z) rounds to z in float. This keeps
    // representable tails (including margin +100) instead of rounding to zero.
    private static float LogOnePlusPositive(float z)
    {
        var sum = 1d + z;
        return sum == 1d ? z : (float)(Math.Log(sum) * (z / (sum - 1d)));
    }

    private sealed record PolicyLogProbGraph(
        Tensor Logits,
        Tensor Ids,
        Tensor Mask,
        float LogProbability,
        bool SumTargets = false);

    private static float VectorSignal(IReadOnlyList<float> vector)
    {
        if (vector.Count == 0)
            return 1f;

        var activeSlots = vector.Count(value => MathF.Abs(value) > 1e-6f);
        return Math.Clamp(1f + (activeSlots / 60f), 1f, 2f);
    }

    private static void RequirePairAdmissions(IReadOnlyList<DpoBatch> batches)
    {
        for (var index = 0; index < batches.Count; index++)
            RequirePairAdmission(batches[index]);
    }

    private static void RequireVectorPairAdmissions(
        IReadOnlyList<DpoVectorBatch> batches)
    {
        for (var index = 0; index < batches.Count; index++)
        {
            var vectorBatch = batches[index];
            ArgumentNullException.ThrowIfNull(vectorBatch);
            RequirePairAdmission(vectorBatch.Batch);
        }
    }

    private static void RequirePairAdmission(DpoBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        var admission = batch.Admission;
        if (admission is null)
        {
            throw new DpoMutationAdmissionException(
                "A DPO pair admission passport is required before trainer mutation.",
                DpoTrainerPairAdmission.MissingErrorCode);
        }

        if (admission.HasPaidExternalModelSide)
        {
            throw new DpoMutationAdmissionException(
                "A paid external model output cannot be a DPO trainer pair side.",
                DpoTrainerPairAdmission.PaidExternalModelSideErrorCode);
        }
    }

    // Низкоуровневый тренер принимает только уже выданное сервером разрешение и
    // никогда не создаёт его сам. Поэтому конструктор без разрешения остаётся
    // безопасным для оценки, а путь изменения завершается до первой мутации.
    private IDisposable AcquireMutationPermit()
    {
        _projectRecipient?.RequireValid();
        var permit = _mutationAdmission?.AcquireMutationPermit()
           ?? throw new DpoMutationAdmissionException(
               "A server-issued DPO trainer mutation capability is required.",
               DpoMutationAdmissionCapability.RequiredErrorCode);
        try { _projectRecipient?.RequireValid(); return permit; }
        catch { permit.Dispose(); throw; }
    }

    private void ApplyOptimizer()
    {
        _projectRecipient?.RequireValid();
        _optimizer.Step(_projectRecipient is null
            ? _policy.Parameters().Where(parameter => parameter.Value.RequiresGrad)
            : _projectRecipient.Bindings.Select(x => x.Parameter), lr: _learningRate);
        _projectRecipient?.RequireValid();
    }

    private T ExecuteWithPublication<T>(
        int step,
        Func<CheckpointPublication, T> operation)
    {
        using var policyMutation = AcquirePolicyMutationGate();
        _projectRecipient?.RequireValid();
        var publication = ReservePublication(step);
        DpoTrainerStepSnapshot? rollback = null;
        Exception? primaryFailure = null;
        try
        {
            rollback = DpoTrainerStepSnapshot.Capture(
                _policy,
                _optimizer,
                _captureRandomState?.Invoke(ExportOptimizerState().Step),
                _restoreRandomState);
            _projectRecipient?.RequireValid();
            return operation(publication);
        }
        catch (Exception exception)
        {
            primaryFailure = exception;
            if (rollback is not null && !publication.PublicationCompleted)
                RestoreBeforeRetry(rollback, exception);
            if (!publication.PublicationCompleted)
            {
                try
                {
                    _publicationObserver?.PublicationFailed?.Invoke();
                }
                catch (Exception cleanupFailure)
                {
                    AttachCleanupFailure(exception, cleanupFailure);
                }
            }
            throw;
        }
        finally
        {
            // Резервирование освобождается и после успеха, и после ошибки. Сбой
            // очистки присоединяется к исходной ошибке и не скрывает её причину.
            publication.Release(primaryFailure);
        }
    }

    private DpoTrainingPublicationBundle BuildPublicationBundle(int dpoStep)
    {
        var optimizerState = ExportOptimizerState();
        var randomState = _captureRandomState?.Invoke(optimizerState.Step);
        return new DpoTrainingPublicationBundle(
            optimizerState,
            randomState,
            optimizerState.Step,
            dpoStep,
            ExecutionProfile: _executionProfile);
    }

    private PolicyMutationLease AcquirePolicyMutationGate()
    {
        var token = new object();
        if (!ActivePolicyMutationGates.TryAdd(_policy, token))
        {
            throw new InvalidOperationException(
                $"{PolicyMutationActiveErrorCode}: policy model is already being mutated by another DPO trainer step.");
        }

        return new PolicyMutationLease(_policy, token);
    }

    private static void RestoreBeforeRetry(
        DpoTrainerStepSnapshot rollback,
        Exception primaryFailure)
    {
        try
        {
            rollback.Restore();
        }
        catch (Exception rollbackFailure)
        {
            AttachStepRollbackFailure(primaryFailure, rollbackFailure);
        }
    }

    private CheckpointPublication ReservePublication(int step)
    {
        // Reached only after step admission. Preparation never creates, adopts
        // or removes a directory; publication keeps its existing collision rules.
        if (_deferCheckpointDirectory)
            Directory.CreateDirectory(_checkpointDir);
        var policyPath = Path.Combine(_checkpointDir, BuildPolicyCheckpointFileName(step));
        var metricsPath = Path.Combine(_checkpointDir, BuildMetricsFileName(step));
        var lockPath = Path.Combine(
            _checkpointDir,
            $".{MetricsFilePrefix}{step:D6}.lock");
        // Сначала резервируем имена шага: занятая или неполная пара файлов должна
        // остановить повтор раньше изменения весов и создания нового результата.
        var reservation = AcquireReservation(lockPath, step, _publicationObserver);

        try
        {
            _publicationObserver?.AfterReservationAcquired?.Invoke();
            EnsureFinalArtifactsAreAbsent(policyPath, metricsPath, step);
            return new CheckpointPublication(
                reservation,
                step,
                policyPath,
                metricsPath,
                _publicationObserver,
                _projectRecipient is null ? null : _projectRecipient.RequireValid);
        }
        catch (Exception exception)
        {
            try
            {
                var releaseFailure = reservation.Release();
                if (releaseFailure is not null)
                    AttachCleanupFailure(exception, releaseFailure);
            }
            catch (Exception cleanupFailure)
            {
                if (IsNonFatalExceptionGraph(exception) &&
                    !IsNonFatalExceptionGraph(cleanupFailure))
                {
                    AttachCleanupFailure(cleanupFailure, exception);
                    ExceptionDispatchInfo.Capture(cleanupFailure).Throw();
                }
                AttachCleanupFailure(exception, cleanupFailure);
            }
            throw;
        }
    }

    private static StepReservation AcquireReservation(
        string lockPath,
        int step,
        DpoTrainerPublicationObserver? publicationObserver)
    {
        var normalizedLockPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(lockPath));
        if (!ActiveReservationGates.TryAdd(normalizedLockPath, 0))
        {
            throw new InvalidOperationException(
                $"{CheckpointReservationActiveErrorCode}: step {step:D6} is already reserved.");
        }

        FileStream? lockStream = null;
        var createdByTrainer = false;
        var byteRangeLocked = false;
        DpoTrainerArtifactOwnershipProof? creationProof = null;
        try
        {
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            {
                throw new PlatformNotSupportedException(
                    "DPO checkpoint reservations are supported only on Windows and Linux x64.");
            }
            var reservationPlatform = OperatingSystem.IsWindows()
                ? DpoReservationPlatform.Windows
                : DpoReservationPlatform.Unix;

            publicationObserver?.AfterProcessLocalGateAcquired?.Invoke();
            try
            {
                publicationObserver?.ReservationCreateFailureForTesting?.Invoke();
                lockStream = OpenReservationStream(
                    normalizedLockPath,
                    out createdByTrainer,
                    publicationObserver?.ReservationOpenFailureForTesting);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                var phase = exception is ReservationPhaseException phaseFailure
                    ? phaseFailure.Phase
                    : DpoReservationFailurePhase.Create;
                var cause = exception is ReservationPhaseException { InnerException: not null } wrapped
                    ? wrapped.InnerException!
                    : exception;
                if (IsReservationContentionForTesting(cause, phase, reservationPlatform))
                {
                    var active = ReservationActive(step, cause);
                    ReleaseFailedReservation(
                        lockStream,
                        normalizedLockPath,
                        step,
                        createdByTrainer,
                        creationProof,
                        byteRangeLocked,
                        active);
                    lockStream = null;
                    throw active;
                }

                var failure = ReservationOperationFailure(
                    CheckpointReservationCreateFailedErrorCode,
                    "Reservation file could not be created or opened.",
                    cause);
                ReleaseFailedReservation(
                    lockStream,
                    normalizedLockPath,
                    step,
                    createdByTrainer,
                    creationProof,
                    byteRangeLocked,
                    failure);
                lockStream = null;
                throw failure;
            }

            if (createdByTrainer || OperatingSystem.IsLinux())
            {
                try
                {
                    creationProof = CaptureOwnershipProof(
                        lockStream,
                        normalizedLockPath,
                        step,
                        DpoTrainerArtifactKind.ReservationLock);
                }
                catch (Exception exception)
                {
                    ReleaseFailedReservation(
                        lockStream,
                        normalizedLockPath,
                        step,
                        createdByTrainer,
                        creationProof,
                        byteRangeLocked,
                        exception);
                    lockStream = null;
                    throw;
                }
            }

            try
            {
                publicationObserver?.ReservationLockFailureForTesting?.Invoke();
                LockReservationStream(lockStream, normalizedLockPath);
                byteRangeLocked = true;
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                if (IsReservationContentionForTesting(
                        exception,
                        DpoReservationFailurePhase.Lock,
                        reservationPlatform))
                {
                    var active = ReservationActive(step, exception);
                    ReleaseFailedReservation(
                        lockStream,
                        normalizedLockPath,
                        step,
                        createdByTrainer,
                        creationProof,
                        byteRangeLocked,
                        active);
                    lockStream = null;
                    throw active;
                }

                var failure = ReservationOperationFailure(
                    CheckpointReservationLockFailedErrorCode,
                    "Reservation byte-range lock could not be acquired.",
                    exception);
                ReleaseFailedReservation(
                    lockStream,
                    normalizedLockPath,
                    step,
                    createdByTrainer,
                    creationProof,
                    byteRangeLocked,
                    failure);
                lockStream = null;
                throw failure;
            }

            if (creationProof is not null)
            {
                try
                {
                    ReportOwnershipProof(publicationObserver, creationProof);
                }
                catch (Exception exception)
                {
                    ReleaseFailedReservation(
                        lockStream,
                        normalizedLockPath,
                        step,
                        createdByTrainer,
                        creationProof,
                        byteRangeLocked,
                        exception);
                    lockStream = null;
                    throw;
                }
            }

            var reservation = new StepReservation(
                lockStream,
                normalizedLockPath,
                step,
                createdByTrainer,
                creationProof,
                publicationObserver?.BeforeReservationUnlockForTesting,
                publicationObserver?.BeforeReservationStreamDisposeForTesting);
            lockStream = null;
            return reservation;
        }
        catch (Exception acquisitionFailure)
        {
            Exception? disposeFailure = null;
            try
            {
                try
                {
                    lockStream?.Dispose();
                }
                catch (Exception exception)
                {
                    disposeFailure = exception;
                }
            }
            finally
            {
                ActiveReservationGates.TryRemove(normalizedLockPath, out _);
            }
            if (disposeFailure is not null)
            {
                if (IsNonFatalExceptionGraph(acquisitionFailure) &&
                    !IsNonFatalExceptionGraph(disposeFailure))
                {
                    AttachCleanupFailure(
                        disposeFailure,
                        acquisitionFailure);
                    ExceptionDispatchInfo.Capture(disposeFailure).Throw();
                }
                AttachCleanupFailure(acquisitionFailure, disposeFailure);
            }
            return RethrowPreserving<StepReservation>(acquisitionFailure);
        }
    }

    private static InvalidOperationException ReservationActive(int step, Exception inner)
        => new(
            $"{CheckpointReservationActiveErrorCode}: step {step:D6} is already reserved.",
            inner);

    private static IOException ReservationOperationFailure(
        string code,
        string message,
        Exception inner)
        => new($"{code}: {message}", inner);

    internal static bool IsReservationContentionForTesting(
        Exception exception,
        DpoReservationFailurePhase phase,
        DpoReservationPlatform platform)
    {
        return NativeErrorCodes(exception).Any(nativeError => platform switch
        {
            DpoReservationPlatform.Windows => phase switch
            {
                DpoReservationFailurePhase.Create =>
                    nativeError is ErrorFileExists or ErrorAlreadyExists,
                DpoReservationFailurePhase.Open => nativeError == ErrorSharingViolation,
                DpoReservationFailurePhase.Lock => nativeError == ErrorLockViolation,
                _ => false
            },
            DpoReservationPlatform.Unix => phase switch
            {
                DpoReservationFailurePhase.Create => nativeError == UnixErrorFileExists,
                DpoReservationFailurePhase.Lock =>
                    nativeError is UnixErrorAgain or UnixErrorAccessDenied,
                _ => false
            },
            _ => false
        });
    }

    private static IEnumerable<int> NativeErrorCodes(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is System.ComponentModel.Win32Exception win32)
            {
                yield return win32.NativeErrorCode;
            }
            else if (current is IOException or UnauthorizedAccessException)
            {
                yield return current.HResult & 0xffff;
            }

            if (current is AggregateException aggregate)
            {
                for (var index = aggregate.InnerExceptions.Count - 1; index >= 0; index--)
                    pending.Push(aggregate.InnerExceptions[index]);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }
    }

    private static FileStream OpenReservationStream(
        string normalizedLockPath,
        out bool createdByTrainer,
        Action? beforeOpenExistingForTesting)
    {
        if (OperatingSystem.IsLinux())
        {
            createdByTrainer = false;
            return OpenLinuxReservationStream(
                normalizedLockPath,
                beforeOpenExistingForTesting);
        }

        if (!OperatingSystem.IsWindows())
        {
            createdByTrainer = false;
            throw new PlatformNotSupportedException(
                "DPO checkpoint reservations are supported only on Windows and Linux x64.");
        }

        var handle = CreateFileWindows(
            ToWindowsExtendedPath(normalizedLockPath),
            GenericReadAccess | GenericWriteAccess | DeleteAccess,
            (uint)(FileShare.Read | FileShare.Write),
            IntPtr.Zero,
            CreateNewDisposition,
            FileAttributeNormal,
            IntPtr.Zero);
        if (!handle.IsInvalid)
        {
            createdByTrainer = true;
            return new FileStream(handle, FileAccess.ReadWrite);
        }

        var error = Marshal.GetLastWin32Error();
        handle.Dispose();
        if (error is not ErrorFileExists and not ErrorAlreadyExists)
        {
            createdByTrainer = false;
            throw new ReservationPhaseException(
                DpoReservationFailurePhase.Create,
                new System.ComponentModel.Win32Exception(error));
        }

        createdByTrainer = false;
        return OpenExistingReservationStream(
            normalizedLockPath,
            beforeOpenExistingForTesting);
    }

    private static string ToWindowsExtendedPath(string path)
    {
        if (!OperatingSystem.IsWindows())
            return path;

        var fullPath = Path.GetFullPath(path);
        if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal))
            return fullPath;
        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return @"\\?\UNC\" + fullPath[2..];
        return @"\\?\" + fullPath;
    }

    private static FileStream OpenLinuxReservationStream(
        string normalizedLockPath,
        Action? beforeOpenForTesting)
    {
        EnsureLinuxX64StatLayout();
        var parentPath = Path.GetDirectoryName(normalizedLockPath);
        var leafName = Path.GetFileName(normalizedLockPath);
        if (string.IsNullOrWhiteSpace(parentPath) ||
            string.IsNullOrWhiteSpace(leafName) ||
            leafName is "." or "..")
        {
            throw new ReservationPhaseException(
                DpoReservationFailurePhase.Create,
                new IOException("DPO_RESERVATION_PATH_UNSAFE"));
        }

        using var parentHandle = OpenLinuxDirectoryForReservation(parentPath);
        try
        {
            beforeOpenForTesting?.Invoke();
            var descriptor = OpenAtLinux(
                ToFileDescriptor(parentHandle),
                leafName,
                LinuxOpenReadWrite | LinuxOpenCreate | LinuxOpenNoFollow |
                LinuxOpenCloseOnExec,
                LinuxReservationFileMode);
            if (descriptor < 0)
            {
                throw LinuxIOExceptionWithNativeCause(
                    "open persistent DPO reservation sidecar",
                    normalizedLockPath,
                    Marshal.GetLastPInvokeError());
            }

            var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
            try
            {
                ValidateLinuxReservationHandle(handle, normalizedLockPath);
                // Служебный файл блокировки живёт столько же, сколько каталог
                // контрольной точки. После открытия с `O_CREAT` принудительно
                // сохраняем запись каталога, чтобы имя пережило аварийный сбой.
                if (FsyncLinux(ToFileDescriptor(parentHandle)) != 0)
                {
                    throw LinuxIOExceptionWithNativeCause(
                        "flush DPO reservation parent directory",
                        parentPath,
                        Marshal.GetLastPInvokeError());
                }
                return new FileStream(handle, FileAccess.ReadWrite, 4096, isAsync: false);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }
        catch (ReservationPhaseException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new ReservationPhaseException(
                DpoReservationFailurePhase.Open,
                exception);
        }
    }

    private static SafeFileHandle OpenLinuxDirectoryForReservation(string path)
    {
        var descriptor = OpenLinux(
            path,
            LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw new ReservationPhaseException(
                DpoReservationFailurePhase.Open,
                LinuxIOExceptionWithNativeCause(
                    "open DPO reservation parent without following links",
                    path,
                    Marshal.GetLastPInvokeError()));
        }
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static void ValidateLinuxReservationHandle(
        SafeFileHandle handle,
        string path)
    {
        EnsureLinuxX64StatLayout();
        if (FStatLinux(ToFileDescriptor(handle), out var stat) != 0)
        {
            throw LinuxIOExceptionWithNativeCause(
                "inspect DPO reservation sidecar",
                path,
                Marshal.GetLastPInvokeError());
        }
        if ((stat.Mode & LinuxFileTypeMask) != LinuxRegularFileType)
            throw new IOException("DPO_RESERVATION_NON_REGULAR_FILE");
        if (stat.LinkCount != 1)
            throw new IOException("DPO_RESERVATION_LINK_COUNT_UNSAFE");
        if (stat.Size < 0 || stat.Size > LinuxReservationMaximumBytes)
            throw new IOException("DPO_RESERVATION_SIZE_UNSAFE");
    }

    private static void LockReservationStream(FileStream stream, string path)
    {
        if (OperatingSystem.IsWindows())
        {
            stream.Lock(0, 1);
            return;
        }
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("DPO reservation locking is unavailable.");

        ValidateLinuxReservationHandle(stream.SafeFileHandle, path);
        if (FlockLinux(
                ToFileDescriptor(stream.SafeFileHandle),
                LinuxFlockExclusive | LinuxFlockNonBlocking) != 0)
        {
            throw LinuxIOExceptionWithNativeCause(
                "acquire DPO reservation flock",
                path,
                Marshal.GetLastPInvokeError());
        }
        ValidateLinuxReservationHandle(stream.SafeFileHandle, path);
    }

    private static void UnlockReservationStream(FileStream stream, string path)
    {
        if (OperatingSystem.IsWindows())
        {
            stream.Unlock(0, 1);
            return;
        }
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("DPO reservation unlocking is unavailable.");
        if (FlockLinux(ToFileDescriptor(stream.SafeFileHandle), LinuxFlockUnlock) != 0)
        {
            throw LinuxIOExceptionWithNativeCause(
                "release DPO reservation flock",
                path,
                Marshal.GetLastPInvokeError());
        }
    }

    private static FileStream OpenExistingReservationStream(
        string normalizedLockPath,
        Action? beforeOpenExistingForTesting)
    {
        try
        {
            beforeOpenExistingForTesting?.Invoke();
            return new FileStream(
                normalizedLockPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new ReservationPhaseException(
                DpoReservationFailurePhase.Open,
                exception);
        }
    }

    private static void ReleaseFailedReservation(
        FileStream? stream,
        string normalizedLockPath,
        int step,
        bool createdByTrainer,
        DpoTrainerArtifactOwnershipProof? creationProof,
        bool byteRangeLocked,
        Exception primaryException)
    {
        Exception? unlockFailure = null;
        Exception? deleteFailure = null;
        Exception? disposeFailure = null;
        static void Capture(Action action, ref Exception? failure)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        }

        try
        {
            if (stream is not null)
            try
            {
                if (byteRangeLocked)
                {
                    Capture(
                        () => UnlockReservationStream(stream, normalizedLockPath),
                        ref unlockFailure);
                }
            }
            finally
            {
                try
                {
                    if (OperatingSystem.IsWindows() &&
                        createdByTrainer && creationProof is not null)
                    {
                        Capture(() =>
                        {
                            var observed = CaptureOwnershipProof(
                                stream,
                                normalizedLockPath,
                                step,
                                DpoTrainerArtifactKind.ReservationLock);
                            if (!ReservationProofMatches(
                                    creationProof,
                                    observed))
                            {
                                throw new IOException(
                                    "DPO_RESERVATION_OWNERSHIP_REVALIDATION_FAILED");
                            }
                            MarkPinnedStreamForDeletion(stream, normalizedLockPath);
                        }, ref deleteFailure);
                    }
                }
                finally
                {
                    Capture(stream.Dispose, ref disposeFailure);
                }
            }
        }
        finally
        {
            ActiveReservationGates.TryRemove(normalizedLockPath, out _);
        }
        var fatalCleanup = FirstFatal(
            unlockFailure,
            deleteFailure,
            disposeFailure);
        if (!IsNonFatalExceptionGraph(primaryException))
        {
            AttachCleanupFailures(
                primaryException,
                unlockFailure,
                deleteFailure,
                disposeFailure);
            return;
        }
        if (fatalCleanup is not null)
        {
            AttachCleanupFailures(
                fatalCleanup,
                primaryException,
                unlockFailure,
                deleteFailure,
                disposeFailure);
            ExceptionDispatchInfo.Capture(fatalCleanup).Throw();
        }
        AttachCleanupFailures(
            primaryException,
            unlockFailure,
            deleteFailure,
            disposeFailure);
    }

    private static void AttachCleanupFailure(
        Exception primaryException,
        Exception cleanupFailure)
        => AttachCleanupFailures(primaryException, cleanupFailure, null);

    private static void AttachStepRollbackFailure(
        Exception primaryException,
        Exception rollbackFailure)
    {
        try
        {
            primaryException.Data["DPO_STEP_ROLLBACK_FAILURE"] =
                new IOException(
                    $"{StepRollbackFailedErrorCode}: failed to restore pre-step trainer state.",
                    rollbackFailure);
        }
        catch
        {
            // Сведения об ошибке отката не должны заменять уже установленную
            // основную причину сбоя.
        }
    }

    private static void AttachCleanupFailures(
        Exception primaryException,
        Exception? first,
        Exception? second,
        Exception? third = null,
        Exception? fourth = null,
        Exception? fifth = null)
    {
        try
        {
            var cleanupFailures = new[]
            {
                first,
                second,
                third,
                fourth,
                fifth
            }.Where(failure =>
                    failure is not null &&
                    !ReferenceEquals(failure, primaryException))
             .Select(static failure => failure!)
             .ToArray();
            if (cleanupFailures.Length == 0)
                return;
            primaryException.Data["DPO_CLEANUP_FAILURE"] =
                cleanupFailures.Length == 1
                    ? cleanupFailures[0]
                    : new AggregateException(cleanupFailures);
        }
        catch
        {
            // Cleanup diagnostics must never replace the established primary failure.
        }
    }

    private static bool IsNonFatalExceptionGraph(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(
            ReferenceEqualityComparer.Instance);
        pending.Push(exception);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;
            if (current is OperationCanceledException
                or OutOfMemoryException
                or StackOverflowException
                or AccessViolationException
                or AppDomainUnloadedException
                or BadImageFormatException ||
                StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }
            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }
        return true;
    }

    private static Exception? FirstFatal(
        Exception? first,
        Exception? second,
        Exception? third = null,
        Exception? fourth = null,
        Exception? fifth = null)
    {
        if (first is not null && !IsNonFatalExceptionGraph(first))
            return first;
        if (second is not null && !IsNonFatalExceptionGraph(second))
            return second;
        if (third is not null && !IsNonFatalExceptionGraph(third))
            return third;
        if (fourth is not null && !IsNonFatalExceptionGraph(fourth))
            return fourth;
        return fifth is not null && !IsNonFatalExceptionGraph(fifth)
            ? fifth
            : null;
    }

    private static T RethrowPreserving<T>(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        return default!;
    }

    private static bool ReservationProofMatches(
        DpoTrainerArtifactOwnershipProof expected,
        DpoTrainerArtifactOwnershipProof observed)
        => expected.Identity == observed.Identity &&
           expected.LinkCount == observed.LinkCount &&
           expected.Length == observed.Length &&
           string.Equals(expected.Sha256, observed.Sha256, StringComparison.Ordinal);

    private static void EnsureFinalArtifactsAreAbsent(
        string policyPath,
        string metricsPath,
        int step)
    {
        var policyExists = PathExists(policyPath);
        var metricsExists = PathExists(metricsPath);

        if (policyExists && metricsExists)
        {
            throw new InvalidOperationException(
                $"{CheckpointCollisionErrorCode}: step {step:D6} already has a published policy and metrics pair.");
        }

        if (policyExists || metricsExists)
        {
            throw new InvalidOperationException(
                $"{CheckpointIncompleteErrorCode}: step {step:D6} has a lone policy or metrics artifact.");
        }
    }

    private static bool PathExists(string path)
        => File.Exists(path) || Directory.Exists(path);

    private sealed class ReservationPhaseException(
        DpoReservationFailurePhase phase,
        Exception inner) : IOException("DPO_RESERVATION_PHASE_FAILED", inner)
    {
        internal DpoReservationFailurePhase Phase { get; } = phase;
    }

    private sealed class PolicyMutationLease(LlmModel policy, object token) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            var entry = new KeyValuePair<LlmModel, object>(policy, token);
            ((ICollection<KeyValuePair<LlmModel, object>>)ActivePolicyMutationGates)
                .Remove(entry);
        }
    }

    private sealed class StepReservation
    {
        private readonly FileStream _lockStream;
        private readonly string _normalizedLockPath;
        private readonly int _step;
        private readonly bool _createdByTrainer;
        private readonly DpoTrainerArtifactOwnershipProof? _creationProof;
        private readonly Action? _beforeUnlockForTesting;
        private readonly Action? _beforeStreamDisposeForTesting;
        private int _disposed;

        public StepReservation(
            FileStream lockStream,
            string normalizedLockPath,
            int step,
            bool createdByTrainer,
            DpoTrainerArtifactOwnershipProof? creationProof,
            Action? beforeUnlockForTesting,
            Action? beforeStreamDisposeForTesting)
        {
            _lockStream = lockStream;
            _normalizedLockPath = normalizedLockPath;
            _step = step;
            _createdByTrainer = createdByTrainer;
            _creationProof = creationProof;
            _beforeUnlockForTesting = beforeUnlockForTesting;
            _beforeStreamDisposeForTesting = beforeStreamDisposeForTesting;
        }

        public IOException? Release()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return null;

            Exception? beforeUnlockFailure = null;
            Exception? unlockFailure = null;
            Exception? deleteFailure = null;
            Exception? beforeDisposeFailure = null;
            Exception? disposeFailure = null;
            static void Capture(Action action, ref Exception? failure)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            }
            try
            {
                Capture(
                    () => _beforeUnlockForTesting?.Invoke(),
                    ref beforeUnlockFailure);
                try
                {
                    Capture(
                        () => UnlockReservationStream(
                            _lockStream,
                            _normalizedLockPath),
                        ref unlockFailure);
                }
                finally
                {
                    try
                    {
                        if (OperatingSystem.IsWindows() &&
                            _createdByTrainer && _creationProof is not null)
                        {
                            Capture(() =>
                            {
                                var observed = CaptureOwnershipProof(
                                    _lockStream,
                                    _normalizedLockPath,
                                    _step,
                                    DpoTrainerArtifactKind.ReservationLock);
                                if (!ReservationProofMatches(
                                        _creationProof,
                                        observed))
                                {
                                    throw new IOException(
                                        "DPO_RESERVATION_OWNERSHIP_REVALIDATION_FAILED");
                                }
                                MarkPinnedStreamForDeletion(
                                    _lockStream,
                                    _normalizedLockPath);
                            }, ref deleteFailure);
                        }
                    }
                    finally
                    {
                        try
                        {
                            Capture(
                                () => _beforeStreamDisposeForTesting?.Invoke(),
                                ref beforeDisposeFailure);
                        }
                        finally
                        {
                            Capture(_lockStream.Dispose, ref disposeFailure);
                        }
                    }
                }
            }
            finally
            {
                ActiveReservationGates.TryRemove(_normalizedLockPath, out _);
            }

            if (beforeUnlockFailure is null && unlockFailure is null &&
                deleteFailure is null && beforeDisposeFailure is null &&
                disposeFailure is null)
            {
                return null;
            }
            var fatalFailure = FirstFatal(
                beforeUnlockFailure,
                unlockFailure,
                deleteFailure,
                beforeDisposeFailure,
                disposeFailure);
            if (fatalFailure is not null)
            {
                AttachCleanupFailures(
                    fatalFailure,
                    beforeUnlockFailure,
                    unlockFailure,
                    deleteFailure,
                    beforeDisposeFailure,
                    disposeFailure);
                ExceptionDispatchInfo.Capture(fatalFailure).Throw();
            }
            var failures = new[]
            {
                beforeUnlockFailure,
                unlockFailure,
                deleteFailure,
                beforeDisposeFailure,
                disposeFailure
            }.Where(static failure => failure is not null)
             .Select(static failure => failure!)
             .ToArray();
            var failure = new IOException(
                $"{CheckpointReservationReleaseFailedErrorCode}: reservation handle release failed.",
                failures[0]);
            if (failures.Length > 1)
            {
                failure.Data["DPO_ADDITIONAL_RELEASE_FAILURES"] =
                    failures.Skip(1).ToArray();
            }
            return failure;
        }
    }

    private sealed class CheckpointPublication
    {
        private readonly StepReservation _reservation;
        private readonly int _step;
        private readonly string _policyPath;
        private readonly string _metricsPath;
        private readonly string _policyStagingPath;
        private readonly string _metricsStagingPath;
        private readonly DpoTrainerPublicationObserver? _publicationObserver;
        private readonly Action? _validateRecipient;
        private readonly Dictionary<DpoTrainerArtifactIdentity, ArtifactCustody> _artifactCustody = [];
        private readonly List<UnprovenArtifact> _unprovenArtifacts = [];
        private bool _publicationCompleted;

        public CheckpointPublication(
            StepReservation reservation,
            int step,
            string policyPath,
            string metricsPath,
            DpoTrainerPublicationObserver? publicationObserver,
            Action? validateRecipient)
        {
            _reservation = reservation;
            _step = step;
            _policyPath = policyPath;
            _metricsPath = metricsPath;
            _publicationObserver = publicationObserver;
            _validateRecipient = validateRecipient;
            var attemptId = Guid.NewGuid().ToString("N");
            var checkpointDirectory = Path.GetDirectoryName(policyPath)
                ?? throw new InvalidDataException("DPO_CHECKPOINT_STAGING_PATH_INVALID");
            _policyStagingPath = Path.Combine(
                checkpointDirectory,
                $".{Path.GetFileName(policyPath)}.{attemptId}.staging");
            _metricsStagingPath = Path.Combine(
                checkpointDirectory,
                $".{Path.GetFileName(metricsPath)}.{attemptId}.staging");
        }

        public bool PublicationCompleted => _publicationCompleted;

        public void Publish(
            LlmModel policy,
            DpoMetrics metrics,
            DpoTrainingPublicationBundle publicationBundle)
        {
            _validateRecipient?.Invoke();
            // Оба временных файла полностью записываются и сбрасываются на диск до
            // появления хотя бы одного финального имени.
            var policyStagingProof = WritePolicyStaging(policy);
            var metricsStagingProof = WriteMetricsStaging(metrics);
            var expectedPolicyFinalProof = BuildExpectedFinalTransitionProof(
                _policyPath,
                DpoTrainerArtifactKind.PolicyFinal,
                policyStagingProof);
            var expectedMetricsFinalProof = BuildExpectedFinalTransitionProof(
                _metricsPath,
                DpoTrainerArtifactKind.MetricsFinal,
                metricsStagingProof);
            var policyFinalCustody = OwnershipObservationEnabled
                ? new ArtifactCustody(
                    expectedPolicyFinalProof,
                    policyStagingProof)
                : null;
            var metricsFinalCustody = OwnershipObservationEnabled
                ? new ArtifactCustody(
                    expectedMetricsFinalProof,
                    metricsStagingProof)
                : null;
            // Доказательство владения связывает последующую очистку с тем же
            // файловым объектом, чтобы не удалить чужой файл с совпавшим именем.
            if (policyFinalCustody is not null)
                PreinstallFinalCustody(policyStagingProof, policyFinalCustody);
            if (metricsFinalCustody is not null)
                PreinstallFinalCustody(metricsStagingProof, metricsFinalCustody);

            // The optimizer state is prepared before either final policy or metrics
            // name becomes visible. If this callback fails, ExecuteWithPublication
            // restores the in-memory step and the final metrics commit marker never
            // exists.
            _publicationObserver?.BeforePolicyFinalMove?.Invoke(
                publicationBundle with
                {
                    // The staging proof is the immutable identity that will be
                    // transferred to the final trial path below. The executor
                    // persists this identity in the same manifest as optimizer
                    // and RNG state before evaluation can observe the trial.
                    TrialWeightsSha256 = policyStagingProof.Sha256,
                    TrialWeightsByteLength = policyStagingProof.Length
                });
            _validateRecipient?.Invoke();

            // Сначала становится видимым файл весов. Файл метрик перемещается
            // последним и служит признаком того, что опубликованы обе части шага.
            MoveArtifact(
                _policyStagingPath,
                _policyPath,
                policyStagingProof,
                policyFinalCustody);
            _publicationObserver?
                .AfterPolicyFinalMoveBeforeProofForTesting?.Invoke();
            PublishMovedOwnershipProof(
                _policyPath,
                expectedPolicyFinalProof,
                policyFinalCustody);
            _publicationObserver?.AfterPolicyFinalMove?.Invoke();
            MoveArtifact(
                _metricsStagingPath,
                _metricsPath,
                metricsStagingProof,
                metricsFinalCustody);
            _publicationObserver?
                .AfterMetricsFinalMoveBeforeProofForTesting?.Invoke();
            PublishMovedOwnershipProof(
                _metricsPath,
                expectedMetricsFinalProof,
                metricsFinalCustody);
            _publicationObserver?.AfterMetricsFinalMove?.Invoke();
            _publicationCompleted = true;
        }

        private void MoveArtifact(
            string sourcePath,
            string destinationPath,
            DpoTrainerArtifactOwnershipProof sourceProof,
            ArtifactCustody? custody)
        {
            if (_publicationObserver?.MoveArtifactForTesting is not null)
            {
                _publicationObserver.MoveArtifactForTesting(
                    sourcePath,
                    destinationPath);
                return;
            }
            if (OperatingSystem.IsLinux())
            {
                MoveLinuxArtifactNoReplace(
                    sourcePath,
                    destinationPath,
                    sourceProof,
                    custody?.RetainedStream);
                return;
            }
            File.Move(sourcePath, destinationPath);
        }

        private static void MoveLinuxArtifactNoReplace(
            string sourcePath,
            string destinationPath,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream? retainedSourceStream)
        {
            EnsureLinuxX64StatLayout();
            var source = SplitLinuxPublicationPath(sourcePath);
            var destination = SplitLinuxPublicationPath(destinationPath);
            if (!string.Equals(
                    source.ParentPath,
                    destination.ParentPath,
                    StringComparison.Ordinal))
            {
                throw new IOException(
                    "DPO_ARTIFACT_PUBLICATION_PARENT_PATH_CHANGED");
            }

            // Staging и final должны жить в одном checkpoint-каталоге. Один
            // удержанный дескриптор родителя закрывает окно между двумя open()
            // каталога и не даёт проверить source в одном каталоге, а
            // публиковать destination уже в другом.
            using var parentHandle = OpenLinuxDirectoryForDeletion(
                source.ParentPath);
            ValidateLinuxMoveSource(
                parentHandle,
                source,
                sourceProof,
                retainedSourceStream,
                expectedLinkCount: 1);

            var renameError = TryRenameLinuxNoReplace(
                parentHandle,
                source.LeafName,
                parentHandle,
                destination.LeafName);
            if (renameError is null)
            {
                ValidateLinuxMovedArtifact(
                    parentHandle,
                    destination,
                    sourceProof,
                    retainedSourceStream,
                    expectedLinkCount: 1);
                FlushLinuxDirectoryHandle(parentHandle, source.ParentPath);
                return;
            }
            if (renameError == UnixErrorFileExists)
                throw new IOException("DPO_ARTIFACT_PUBLICATION_DESTINATION_EXISTS");
            if (!IsLinuxNoReplaceRenameUnsupported(renameError.Value))
            {
                throw LinuxIOException(
                    "publish DPO artifact with renameat2 no-replace",
                    destination.FullPath,
                    renameError.Value);
            }

            LinkThenUnlinkLinuxNoReplace(
                parentHandle,
                source,
                destination,
                sourceProof,
                retainedSourceStream);
        }

        private static int? TryRenameLinuxNoReplace(
            SafeFileHandle sourceParentHandle,
            string sourceLeafName,
            SafeFileHandle destinationParentHandle,
            string destinationLeafName)
        {
            try
            {
                if (RenameAt2Linux(
                        ToFileDescriptor(sourceParentHandle),
                        sourceLeafName,
                        ToFileDescriptor(destinationParentHandle),
                        destinationLeafName,
                        LinuxRenameNoReplace) == 0)
                {
                    return null;
                }

                return Marshal.GetLastPInvokeError();
            }
            catch (EntryPointNotFoundException)
            {
                return UnixErrorFunctionNotImplemented;
            }
        }

        private static void LinkThenUnlinkLinuxNoReplace(
            SafeFileHandle parentHandle,
            LinuxPublicationPath source,
            LinuxPublicationPath destination,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream? retainedSourceStream)
        {
            if (LinkAtLinux(
                    ToFileDescriptor(parentHandle),
                    source.LeafName,
                    ToFileDescriptor(parentHandle),
                    destination.LeafName,
                    flags: 0) != 0)
            {
                var linkError = Marshal.GetLastPInvokeError();
                if (linkError == UnixErrorFileExists)
                    throw new IOException("DPO_ARTIFACT_PUBLICATION_DESTINATION_EXISTS");
                throw LinuxIOException(
                    "link DPO artifact final name without replacing destination",
                    destination.FullPath,
                    linkError);
            }

            // На /mnt/c атомарный renameat2(RENAME_NOREPLACE) недоступен, а
            // File.Move скрывает финальное имя, пока открыт proof-дескриптор
            // staging. Fallback сначала создаёт вторую ссылку без перезаписи и
            // проверяет identity/hash у обоих имён, прежде чем убрать staging.
            ValidateLinuxMovedArtifact(
                parentHandle,
                destination,
                sourceProof,
                retainedSourceStream,
                expectedLinkCount: 2);
            ValidateLinuxSourceNameAfterLink(
                parentHandle,
                source,
                sourceProof,
                retainedSourceStream);
            FlushLinuxDirectoryHandle(parentHandle, source.ParentPath);
            if (UnlinkAtLinux(
                    ToFileDescriptor(parentHandle),
                    source.LeafName,
                    flags: 0) != 0)
            {
                throw LinuxIOException(
                    "unlink DPO artifact staging name after no-replace link",
                    source.FullPath,
                    Marshal.GetLastPInvokeError());
            }

            ValidateLinuxMovedArtifact(
                parentHandle,
                destination,
                sourceProof,
                retainedSourceStream,
                expectedLinkCount: 1);
            FlushLinuxDirectoryHandle(parentHandle, source.ParentPath);
        }

        private static void ValidateLinuxSourceNameAfterLink(
            SafeFileHandle sourceParentHandle,
            LinuxPublicationPath source,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream? retainedSourceStream)
        {
            using var sourceNameStream = OpenLinuxLeafReadStream(
                sourceParentHandle,
                source);
            var sourceNameProof = CaptureLinuxOwnershipProofAllowingLinkCount(
                sourceNameStream,
                source.FullPath,
                sourceProof.Step,
                sourceProof.Kind);
            ValidateSameLinuxArtifactProof(
                sourceProof,
                sourceNameProof,
                expectedLinkCount: 2,
                "DPO_ARTIFACT_PUBLICATION_SOURCE_CHANGED_AFTER_LINK");
            if (retainedSourceStream is not null)
            {
                ValidateRetainedLinuxArtifactProof(
                    source.FullPath,
                    sourceProof,
                    retainedSourceStream,
                    expectedLinkCount: 2);
            }
        }

        private static void ValidateLinuxMoveSource(
            SafeFileHandle sourceParentHandle,
            LinuxPublicationPath source,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream? retainedSourceStream,
            uint expectedLinkCount)
        {
            using var sourceNameStream = OpenLinuxLeafReadStream(
                sourceParentHandle,
                source);
            var sourceNameProof = CaptureLinuxOwnershipProofAllowingLinkCount(
                sourceNameStream,
                source.FullPath,
                sourceProof.Step,
                sourceProof.Kind);
            ValidateSameLinuxArtifactProof(
                sourceProof,
                sourceNameProof,
                expectedLinkCount,
                "DPO_ARTIFACT_PUBLICATION_SOURCE_CHANGED_BEFORE_MOVE");
            if (retainedSourceStream is not null)
            {
                ValidateRetainedLinuxArtifactProof(
                    source.FullPath,
                    sourceProof,
                    retainedSourceStream,
                    expectedLinkCount);
            }
        }

        private static void ValidateLinuxMovedArtifact(
            SafeFileHandle destinationParentHandle,
            LinuxPublicationPath destination,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream? retainedSourceStream,
            uint expectedLinkCount)
        {
            using var destinationStream = OpenLinuxLeafReadStream(
                destinationParentHandle,
                destination);
            var destinationProof = CaptureLinuxOwnershipProofAllowingLinkCount(
                destinationStream,
                destination.FullPath,
                sourceProof.Step,
                sourceProof.Kind);
            ValidateSameLinuxArtifactProof(
                sourceProof,
                destinationProof,
                expectedLinkCount,
                "DPO_ARTIFACT_PUBLICATION_DESTINATION_IDENTITY_CHANGED");
            if (retainedSourceStream is not null)
            {
                ValidateRetainedLinuxArtifactProof(
                    destination.FullPath,
                    sourceProof,
                    retainedSourceStream,
                    expectedLinkCount);
            }
        }

        private static void ValidateRetainedLinuxArtifactProof(
            string path,
            DpoTrainerArtifactOwnershipProof sourceProof,
            FileStream retainedSourceStream,
            uint expectedLinkCount)
        {
            var retainedProof = CaptureLinuxOwnershipProofAllowingLinkCount(
                retainedSourceStream,
                path,
                sourceProof.Step,
                sourceProof.Kind);
            ValidateSameLinuxArtifactProof(
                sourceProof,
                retainedProof,
                expectedLinkCount,
                "DPO_ARTIFACT_PUBLICATION_RETAINED_IDENTITY_CHANGED");
        }

        private static void ValidateSameLinuxArtifactProof(
            DpoTrainerArtifactOwnershipProof expected,
            DpoTrainerArtifactOwnershipProof observed,
            uint expectedLinkCount,
            string errorCode)
        {
            if (observed.Identity != expected.Identity ||
                observed.Length != expected.Length ||
                observed.LinkCount != expectedLinkCount ||
                !string.Equals(observed.Sha256, expected.Sha256, StringComparison.Ordinal))
            {
                throw new IOException(errorCode);
            }
        }

        private static DpoTrainerArtifactOwnershipProof
            CaptureLinuxOwnershipProofAllowingLinkCount(
                FileStream stream,
                string path,
                int step,
                DpoTrainerArtifactKind kind)
        {
            var fullPath = Path.GetFullPath(path);
            var fileShape = CaptureLinuxArtifactFileShapeAllowingLinkCount(
                stream.SafeFileHandle,
                fullPath);
            var originalPosition = stream.Position;
            stream.Position = 0;
            var hash = Convert.ToHexString(SHA256.HashData(stream))
                .ToLowerInvariant();
            stream.Position = originalPosition;
            var length = stream.Length;
            if (fileShape.Length != length)
            {
                throw new IOException(
                    "DPO_ARTIFACT_PUBLICATION_LENGTH_CHANGED_DURING_PROOF");
            }

            return new DpoTrainerArtifactOwnershipProof(
                step,
                kind,
                fullPath,
                length,
                hash,
                fileShape.LinkCount,
                fileShape.Identity);
        }

        private static DpoTrainerArtifactFileShape
            CaptureLinuxArtifactFileShapeAllowingLinkCount(
                SafeFileHandle handle,
                string fullPath)
        {
            EnsureLinuxX64StatLayout();
            if (handle.IsInvalid || handle.IsClosed)
                throw new IOException("DPO_ARTIFACT_OWNERSHIP_PROOF_UNAVAILABLE");

            var descriptor = checked((int)handle.DangerousGetHandle().ToInt64());
            if (FStatLinux(descriptor, out var stat) != 0)
            {
                throw LinuxIOException(
                    "inspect DPO artifact handle",
                    fullPath,
                    Marshal.GetLastPInvokeError());
            }
            if ((stat.Mode & LinuxFileTypeMask) != LinuxRegularFileType)
                throw new IOException("DPO_ARTIFACT_OWNERSHIP_NON_REGULAR_FILE");
            if (stat.LinkCount > uint.MaxValue)
                throw new IOException("DPO_ARTIFACT_OWNERSHIP_LINK_COUNT_UNSAFE");

            return new DpoTrainerArtifactFileShape(
                checked((long)stat.Size),
                (uint)stat.LinkCount,
                new DpoTrainerArtifactIdentity(
                    DpoTrainerArtifactIdentityPlatform.Linux,
                    stat.Device,
                    stat.Inode,
                    0UL));
        }

        private static bool IsLinuxNoReplaceRenameUnsupported(int error)
            => error is UnixErrorInvalidArgument or
                UnixErrorFunctionNotImplemented or
                UnixErrorOperationNotSupported;

        private static LinuxPublicationPath SplitLinuxPublicationPath(
            string path)
        {
            var fullPath = Path.GetFullPath(path);
            var parentPath = Path.GetDirectoryName(fullPath);
            var leafName = Path.GetFileName(fullPath);
            if (string.IsNullOrWhiteSpace(parentPath) ||
                string.IsNullOrWhiteSpace(leafName) ||
                leafName is "." or "..")
            {
                throw new IOException("DPO_ARTIFACT_PUBLICATION_PARENT_PATH_UNSAFE");
            }

            return new LinuxPublicationPath(fullPath, parentPath, leafName);
        }

        private static FileStream OpenLinuxLeafReadStream(
            SafeFileHandle parentHandle,
            LinuxPublicationPath path)
        {
            var handle = OpenLinuxLeafForDeletion(
                parentHandle,
                path.LeafName,
                path.FullPath);
            try
            {
                return new FileStream(handle, FileAccess.Read);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        private static void FlushLinuxDirectoryHandle(
            SafeFileHandle directoryHandle,
            string directoryPath)
        {
            if (FsyncLinux(ToFileDescriptor(directoryHandle)) != 0)
            {
                throw LinuxIOException(
                    "flush DPO artifact publication parent directory",
                    directoryPath,
                    Marshal.GetLastPInvokeError());
            }
        }

        private readonly record struct LinuxPublicationPath(
            string FullPath,
            string ParentPath,
            string LeafName);

        public void Release(Exception? primaryFailure)
        {
            try
            {
                // При незавершённой публикации удаляем только принадлежащие этой
                // попытке временные объекты, затем всегда освобождаем резервирование.
                // Ошибка очистки дополняет исходную ошибку, но не подменяет её.
                Exception? publicationCleanupFailure = null;
                try
                {
                    if (!_publicationCompleted && OwnershipObservationEnabled)
                    {
                        foreach (var custody in _artifactCustody.Values.Where(static value => !value.Transferred))
                            DeleteCustodyMatchedBestEffort(custody);
                        foreach (var artifact in _unprovenArtifacts)
                            DeleteUnprovenIdentityMatchedBestEffort(artifact);
                    }
                }
                catch (Exception exception)
                {
                    publicationCleanupFailure = IsNonFatalExceptionGraph(exception)
                        ? new IOException(
                            $"{CheckpointPublicationCleanupFailedErrorCode}: publication cleanup failed.",
                            exception)
                        : exception;
                }

                Exception? reservationCleanupFailure = null;
                try
                {
                    var reservationFailure = _reservation.Release();
                    if (reservationFailure is not null)
                        reservationCleanupFailure = reservationFailure;
                }
                catch (Exception exception)
                {
                    reservationCleanupFailure = exception;
                }

                if (publicationCleanupFailure is null &&
                    reservationCleanupFailure is null)
                {
                    return;
                }
                if (primaryFailure is not null)
                {
                    var fatalCleanup = FirstFatal(
                        publicationCleanupFailure,
                        reservationCleanupFailure);
                    if (IsNonFatalExceptionGraph(primaryFailure) &&
                        fatalCleanup is not null)
                    {
                        AttachCleanupFailures(
                            fatalCleanup,
                            primaryFailure,
                            publicationCleanupFailure,
                            reservationCleanupFailure);
                        ExceptionDispatchInfo.Capture(fatalCleanup).Throw();
                    }
                    AttachCleanupFailures(
                        primaryFailure,
                        publicationCleanupFailure,
                        reservationCleanupFailure);
                    return;
                }
                var fatalFailure = FirstFatal(
                    publicationCleanupFailure,
                    reservationCleanupFailure);
                if (fatalFailure is not null)
                {
                    AttachCleanupFailures(
                        fatalFailure,
                        publicationCleanupFailure,
                        reservationCleanupFailure);
                    ExceptionDispatchInfo.Capture(fatalFailure).Throw();
                }
                var cleanupFailures = new[]
                {
                    publicationCleanupFailure,
                    reservationCleanupFailure
                }.Where(static failure => failure is not null)
                 .Select(static failure => failure!)
                 .ToArray();
                if (cleanupFailures.Length == 1)
                    throw cleanupFailures[0];
                var combined = new IOException(
                    $"{CheckpointPublicationCleanupFailedErrorCode}: multiple cleanup operations failed.",
                    cleanupFailures[0]);
                AttachCleanupFailures(
                    combined,
                    cleanupFailures.Length > 1 ? cleanupFailures[1] : null,
                    null);
                throw combined;
            }
            finally
            {
                DisposeRetainedProducerStreams();
            }
        }

        private DpoTrainerArtifactOwnershipProof WritePolicyStaging(LlmModel policy)
        {
            FileStream? stream = new FileStream(
                _policyStagingPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.WriteThrough);
            try
            {
                TrackUnprovenArtifact(stream, _policyStagingPath);
                _publicationObserver?.BeforePolicySerialization?.Invoke(stream);
                policy.SaveCheckpointToStream(stream);
                stream.Flush(flushToDisk: true);
                _publicationObserver?.AfterPolicySerializationBeforeProof?.Invoke(stream);
                if (stream.Length <= 0)
                    throw new InvalidDataException(
                        "DPO_CHECKPOINT_STAGING_INCOMPLETE: staged policy artifact is empty.");
                return PublishOwnedArtifact(
                    ref stream,
                    _policyStagingPath,
                    DpoTrainerArtifactKind.PolicyStaging);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private DpoTrainerArtifactOwnershipProof WriteMetricsStaging(DpoMetrics metrics)
        {
            FileStream? stream = new FileStream(
                _metricsStagingPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.WriteThrough);
            try
            {
                TrackUnprovenArtifact(stream, _metricsStagingPath);
                System.Text.Json.JsonSerializer.Serialize(stream, metrics);
                stream.Flush(flushToDisk: true);
                if (stream.Length <= 0)
                    throw new InvalidDataException(
                        "DPO_CHECKPOINT_STAGING_INCOMPLETE: staged metrics artifact is empty.");
                return PublishOwnedArtifact(
                    ref stream,
                    _metricsStagingPath,
                    DpoTrainerArtifactKind.MetricsStaging);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private DpoTrainerArtifactOwnershipProof PublishOwnedArtifact(
            ref FileStream? stream,
            string path,
            DpoTrainerArtifactKind kind)
        {
            var activeStream = stream ??
                throw new ObjectDisposedException(nameof(stream));
            var proof = CaptureOwnershipProof(activeStream, path, _step, kind);
            if (RegisterProducerCustody(proof, activeStream))
                stream = null;
            _unprovenArtifacts.RemoveAll(value =>
                value.Identity == proof.Identity &&
                string.Equals(value.Path, proof.Path, StringComparison.Ordinal));
            // Staging is observable evidence, not an ownership-transfer
            // boundary. The producer retains exact custody until the same
            // identity has moved to and been reproved at its final path.
            _publicationObserver?.ArtifactOwnershipProven?.Invoke(proof);
            return proof;
        }

        private void TrackUnprovenArtifact(FileStream stream, string path)
        {
            if (!OwnershipObservationEnabled)
                return;
            var proof = CaptureOwnershipProof(
                stream,
                path,
                _step,
                DpoTrainerArtifactKind.PolicyStaging);
            _unprovenArtifacts.Add(new UnprovenArtifact(proof.Path, proof.Identity));
        }

        private static DpoTrainerArtifactOwnershipProof
            BuildExpectedFinalTransitionProof(
                string path,
                DpoTrainerArtifactKind kind,
                DpoTrainerArtifactOwnershipProof previousProof)
            => previousProof with
            {
                Kind = kind,
                Path = Path.GetFullPath(path),
                TransitionFromKind = previousProof.Kind,
                TransitionFromPath = previousProof.Path,
                TransitionFromIdentity = previousProof.Identity
            };

        private void PreinstallFinalCustody(
            DpoTrainerArtifactOwnershipProof previousProof,
            ArtifactCustody finalCustody)
        {
            if (!_artifactCustody.TryGetValue(
                    previousProof.Identity,
                    out var currentCustody) ||
                currentCustody.Proof != previousProof ||
                currentCustody.Transferred)
            {
                throw new IOException(
                    "DPO_ARTIFACT_OWNERSHIP_TRANSITION_SOURCE_MISSING");
            }

            // Install final-path custody before File.Move. The fallback proof
            // binds the same identity at the staging path, so cleanup remains
            // exact whether the move fails before changing the namespace,
            // succeeds, or succeeds and a foreign staging entry is recreated.
            finalCustody.ReplaceRetainedStream(
                currentCustody.DetachRetainedStream());
            _artifactCustody[previousProof.Identity] = finalCustody;
        }

        private DpoTrainerArtifactOwnershipProof PublishMovedOwnershipProof(
            string path,
            DpoTrainerArtifactOwnershipProof expectedProof,
            ArtifactCustody? finalCustody)
        {
            if (_publicationObserver?.TryAcceptArtifactOwnership is null &&
                _publicationObserver?.ArtifactOwnershipProven is null)
            {
                return expectedProof;
            }

            using var stream = OpenArtifactProofReadStream(path);
            var captured = CaptureOwnershipProof(
                stream,
                path,
                _step,
                expectedProof.Kind);
            if (captured.Identity != expectedProof.Identity ||
                captured.LinkCount != expectedProof.LinkCount ||
                captured.Length != expectedProof.Length ||
                !string.Equals(
                    captured.Path,
                    expectedProof.Path,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    captured.Sha256,
                    expectedProof.Sha256,
                    StringComparison.Ordinal))
            {
                throw new IOException("DPO_ARTIFACT_OWNERSHIP_IDENTITY_CHANGED_DURING_PUBLICATION");
            }

            if (finalCustody is null ||
                !_artifactCustody.TryGetValue(
                    expectedProof.Identity,
                    out var currentCustody) ||
                !ReferenceEquals(currentCustody, finalCustody) ||
                currentCustody.Proof != expectedProof ||
                currentCustody.Transferred)
            {
                throw new IOException(
                    "DPO_ARTIFACT_OWNERSHIP_TRANSITION_INVALID");
            }

            var actualTransitionProof = captured with
            {
                TransitionFromKind = expectedProof.TransitionFromKind,
                TransitionFromPath = expectedProof.TransitionFromPath,
                TransitionFromIdentity = expectedProof.TransitionFromIdentity
            };
            _publicationObserver?.ArtifactOwnershipProven?.Invoke(
                actualTransitionProof);
            if ((actualTransitionProof.Kind is
                    DpoTrainerArtifactKind.PolicyFinal or
                    DpoTrainerArtifactKind.MetricsFinal) &&
                _publicationObserver?.TryAcceptArtifactOwnership is not null)
            {
                if (!_publicationObserver.TryAcceptArtifactOwnership(
                        actualTransitionProof))
                {
                    throw new IOException(
                        "DPO_ARTIFACT_OWNERSHIP_TRANSFER_REJECTED");
                }
                finalCustody.Transferred = true;
                finalCustody.DisposeRetainedStream();
            }
            return actualTransitionProof;
        }

        private bool RegisterProducerCustody(
            DpoTrainerArtifactOwnershipProof proof,
            FileStream stream)
        {
            if (!OwnershipObservationEnabled)
                return false;
            var retainsProducerStream =
                OperatingSystem.IsLinux() &&
                proof.Kind is DpoTrainerArtifactKind.PolicyStaging
                    or DpoTrainerArtifactKind.MetricsStaging;
            if (!_artifactCustody.TryAdd(
                    proof.Identity,
                    new ArtifactCustody(
                        proof,
                        retainedStream: retainsProducerStream ? stream : null)))
            {
                throw new IOException(
                    "DPO_ARTIFACT_OWNERSHIP_TRANSITION_INVALID");
            }
            return retainsProducerStream;
        }

        private bool OwnershipObservationEnabled =>
            _publicationObserver?.ArtifactOwnershipProven is not null ||
            _publicationObserver?.TryAcceptArtifactOwnership is not null;

        private void DisposeRetainedProducerStreams()
        {
            foreach (var custody in _artifactCustody.Values)
                custody.DisposeRetainedStream();
        }

        private void DeleteCustodyMatchedBestEffort(ArtifactCustody custody)
        {
            if (TryDeleteProofMatchedBestEffort(custody.Proof))
                return;
            if (custody.FallbackProof is not null)
                TryDeleteProofMatchedBestEffort(custody.FallbackProof);
        }

        private bool TryDeleteProofMatchedBestEffort(
            DpoTrainerArtifactOwnershipProof proof)
        {
            try
            {
                using var stream = OpenPinnedDeleteStream(proof.Path);
                if (stream is null)
                    return false;
                var observed = CaptureOwnershipProof(
                    stream,
                    proof.Path,
                    proof.Step,
                    proof.Kind);
                if (observed.Identity != proof.Identity ||
                    observed.LinkCount != proof.LinkCount ||
                    observed.LinkCount != 1 ||
                    observed.Length != proof.Length ||
                    !string.Equals(
                        observed.Sha256,
                        proof.Sha256,
                        StringComparison.Ordinal))
                {
                    return false;
                }
                _publicationObserver?
                    .AfterFallbackProofValidationBeforeDeleteForTesting?
                    .Invoke(proof);
                MarkPinnedStreamForDeletion(stream, proof.Path);
                return true;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }

        internal static bool TryDeleteProofMatchedForTransaction(
            DpoTrainerArtifactOwnershipProof proof)
        {
            if (proof.Kind is not
                    (DpoTrainerArtifactKind.PolicyFinal or
                     DpoTrainerArtifactKind.MetricsFinal))
            {
                return false;
            }

            try
            {
                using var stream = OpenPinnedDeleteStream(proof.Path);
                if (stream is null)
                    return false;

                var observed = CaptureOwnershipProof(
                    stream,
                    proof.Path,
                    proof.Step,
                    proof.Kind);
                if (observed.Identity != proof.Identity ||
                    observed.LinkCount != proof.LinkCount ||
                    observed.LinkCount != 1 ||
                    observed.Length != proof.Length ||
                    !string.Equals(
                        observed.Sha256,
                        proof.Sha256,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                MarkPinnedStreamForDeletion(stream, proof.Path);
                return true;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void DeleteUnprovenIdentityMatchedBestEffort(UnprovenArtifact artifact)
        {
            try
            {
                using var stream = OpenPinnedDeleteStream(artifact.Path);
                if (stream is null)
                    return;
                var observed = CaptureOwnershipProof(
                    stream,
                    artifact.Path,
                    step: 1,
                    DpoTrainerArtifactKind.PolicyStaging);
                if (observed.Identity != artifact.Identity || observed.LinkCount != 1)
                    return;
                MarkPinnedStreamForDeletion(stream, artifact.Path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
            }
        }

        private static FileStream? OpenPinnedDeleteStream(string path)
        {
            if (OperatingSystem.IsLinux())
                return OpenArtifactProofReadStream(path);
            if (!OperatingSystem.IsWindows())
                return null;
            var handle = CreateFileWindows(
                ToWindowsExtendedPath(path),
                GenericReadAccess | DeleteAccess,
                (uint)FileShare.Read,
                IntPtr.Zero,
                OpenExisting,
                FileAttributeNormal,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                handle.Dispose();
                return null;
            }
            return new FileStream(handle, FileAccess.Read);
        }

        private sealed class ArtifactCustody(
            DpoTrainerArtifactOwnershipProof proof,
            DpoTrainerArtifactOwnershipProof? fallbackProof = null,
            FileStream? retainedStream = null)
        {
            internal DpoTrainerArtifactOwnershipProof Proof { get; set; } = proof;
            internal DpoTrainerArtifactOwnershipProof? FallbackProof { get; } =
                fallbackProof;
            internal FileStream? RetainedStream { get; private set; } =
                retainedStream;
            internal bool Transferred { get; set; }

            internal FileStream? DetachRetainedStream()
            {
                var retainedStream = RetainedStream;
                RetainedStream = null;
                return retainedStream;
            }

            internal void ReplaceRetainedStream(FileStream? retainedStream)
            {
                DisposeRetainedStream();
                RetainedStream = retainedStream;
            }

            internal void DisposeRetainedStream()
            {
                var retainedStream = RetainedStream;
                RetainedStream = null;
                retainedStream?.Dispose();
            }
        }

        private sealed record UnprovenArtifact(
            string Path,
            DpoTrainerArtifactIdentity Identity);

    }

    private static void ReportOwnershipProof(
        DpoTrainerPublicationObserver? observer,
        DpoTrainerArtifactOwnershipProof proof)
    {
        observer?.ArtifactOwnershipProven?.Invoke(proof);
    }

    private static void MarkPinnedStreamForDeletion(FileStream stream, string path)
    {
        if (OperatingSystem.IsWindows())
        {
            MarkPinnedWindowsStreamForDeletion(stream);
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            MarkPinnedLinuxStreamForDeletion(stream, path);
            return;
        }

        throw new IOException("DPO_ARTIFACT_DELETE_DISPOSITION_UNAVAILABLE");
    }

    private static void MarkPinnedWindowsStreamForDeletion(FileStream stream)
    {
        var disposition = new FileDispositionInformation { DeleteFile = 1 };
        if (!SetFileInformationByHandleWindows(
                stream.SafeFileHandle,
                FileDispositionInfoClass,
                ref disposition,
                (uint)Marshal.SizeOf<FileDispositionInformation>()))
        {
            throw new IOException("DPO_ARTIFACT_DELETE_DISPOSITION_REJECTED");
        }
    }

    private static void MarkPinnedLinuxStreamForDeletion(FileStream stream, string path)
    {
        var fullPath = Path.GetFullPath(path);
        var parentPath = Path.GetDirectoryName(fullPath);
        var leafName = Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(leafName) ||
            leafName is "." or "..")
        {
            throw new IOException("DPO_ARTIFACT_DELETE_PATH_UNSAFE");
        }

        var originalShape = CaptureLinuxArtifactFileShape(
            stream.SafeFileHandle,
            fullPath);
        using var parentHandle = OpenLinuxDirectoryForDeletion(parentPath);
        using var reopenedHandle = OpenLinuxLeafForDeletion(parentHandle, leafName, fullPath);
        var reopenedShape = CaptureLinuxArtifactFileShape(reopenedHandle, fullPath);
        if (reopenedShape.Identity != originalShape.Identity ||
            reopenedShape.Length != originalShape.Length ||
            reopenedShape.LinkCount != originalShape.LinkCount)
        {
            throw new IOException("DPO_ARTIFACT_DELETE_IDENTITY_CHANGED");
        }

        // Linux удаляет имя через открытый дескриптор родительского каталога.
        // Перед `unlinkat` мы доказали, что имя указывает на тот же открытый
        // обычный файл, поэтому чужой файл по этому пути не будет удалён.
        if (UnlinkAtLinux(ToFileDescriptor(parentHandle), leafName, 0) != 0)
        {
            throw LinuxIOException(
                "unlink retained DPO artifact",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        if (CaptureLinuxArtifactIdentity(stream.SafeFileHandle, fullPath) !=
            originalShape.Identity)
        {
            throw new IOException("DPO_ARTIFACT_DELETE_HANDLE_CHANGED");
        }

        if (FsyncLinux(ToFileDescriptor(parentHandle)) != 0)
        {
            throw LinuxIOException(
                "flush DPO artifact parent directory",
                parentPath,
                Marshal.GetLastPInvokeError());
        }
    }

    private static SafeFileHandle OpenLinuxDirectoryForDeletion(string path)
    {
        var descriptor = OpenLinux(
            path,
            LinuxOpenReadOnly | LinuxOpenDirectory | LinuxOpenNoFollow |
            LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "open DPO artifact parent without following links",
                path,
                Marshal.GetLastPInvokeError());
        }

        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static SafeFileHandle OpenLinuxLeafForDeletion(
        SafeFileHandle parentHandle,
        string leafName,
        string fullPath)
    {
        var descriptor = OpenAtLinux(
            ToFileDescriptor(parentHandle),
            leafName,
            LinuxOpenReadOnly | LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "open DPO artifact leaf without following links",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static DpoTrainerArtifactOwnershipProof CaptureOwnershipProof(
        FileStream stream,
        string path,
        int step,
        DpoTrainerArtifactKind kind)
    {
        var fullPath = Path.GetFullPath(path);
        // Доверяем не строковому имени, а самому открытому файлу: один и тот же
        // дескриптор сообщает тип, число жёстких ссылок, размер и физическую
        // личность файла; из него же вычисляется контрольная сумма содержимого.
        var fileShape = CaptureArtifactFileShape(stream.SafeFileHandle, fullPath);

        var originalPosition = stream.Position;
        stream.Position = 0;
        var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        stream.Position = originalPosition;
        var length = stream.Length;
        if (fileShape.Length != length)
        {
            throw new IOException(
                "DPO_ARTIFACT_OWNERSHIP_LENGTH_CHANGED_DURING_PROOF");
        }

        return new DpoTrainerArtifactOwnershipProof(
            step,
            kind,
            fullPath,
            length,
            hash,
            fileShape.LinkCount,
            fileShape.Identity);
    }

    private const int FileIdInfoClass = 18;
    private const int FileDispositionInfoClass = 4;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint GenericReadAccess = 0x80000000;
    private const uint GenericWriteAccess = 0x40000000;
    private const uint DeleteAccess = 0x00010000;
    private const uint CreateNewDisposition = 1;
    private const uint OpenExisting = 3;
    private const uint FileAttributeNormal = 0x00000080;
    private const int ErrorSharingViolation = 32;
    private const int ErrorLockViolation = 33;
    private const int ErrorFileExists = 80;
    private const int ErrorAlreadyExists = 183;
    private const int UnixErrorAgain = 11;
    private const int UnixErrorAccessDenied = 13;
    private const int UnixErrorFileExists = 17;
    private const int UnixErrorInvalidArgument = 22;
    private const int UnixErrorFunctionNotImplemented = 38;
    private const int UnixErrorOperationNotSupported = 95;
    private const int LinuxCurrentWorkingDirectory = -100;
    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenReadWrite = 2;
    private const int LinuxOpenCreate = 0x40;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const int LinuxFlockExclusive = 0x02;
    private const int LinuxFlockNonBlocking = 0x04;
    private const int LinuxFlockUnlock = 0x08;
    private const int LinuxFileTypeMask = 0xF000;
    private const int LinuxRegularFileType = 0x8000;
    private const uint LinuxRenameNoReplace = 1;
    private const uint LinuxReservationFileMode = 0x180; // 0600: только владелец.
    private const long LinuxReservationMaximumBytes = 4L * 1024;

    private static FileStream OpenArtifactProofReadStream(string path)
    {
        if (!OperatingSystem.IsLinux())
        {
            return new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read | FileShare.Delete);
        }

        // На Linux имя открывается с запретом перехода по символической ссылке.
        // Все последующие признаки доверия читаются из удержанного дескриптора.
        var descriptor = OpenLinux(
            path,
            LinuxOpenReadOnly | LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        if (descriptor < 0)
        {
            throw LinuxIOException(
                "open DPO artifact without following links",
                path,
                Marshal.GetLastPInvokeError());
        }

        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            return new FileStream(handle, FileAccess.Read);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static DpoTrainerArtifactFileShape CaptureArtifactFileShape(
        SafeFileHandle handle,
        string fullPath)
    {
        if (OperatingSystem.IsWindows())
            return CaptureWindowsArtifactFileShape(handle);
        if (OperatingSystem.IsLinux())
            return CaptureLinuxArtifactFileShape(handle, fullPath);

        throw new IOException("DPO_ARTIFACT_OWNERSHIP_PROOF_UNAVAILABLE");
    }

    private static DpoTrainerArtifactFileShape CaptureWindowsArtifactFileShape(
        SafeFileHandle handle)
    {
        if (!GetFileInformationByHandleWindows(handle, out var fileInfo) ||
            !GetFileInformationByHandleExWindows(
                handle,
                FileIdInfoClass,
                out var identityInfo,
                (uint)Marshal.SizeOf<FileIdInfo>()))
        {
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_PROOF_UNAVAILABLE");
        }

        if ((fileInfo.FileAttributes & FileAttributeDirectory) != 0)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_NON_REGULAR_FILE");
        if (fileInfo.NumberOfLinks != 1)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_LINK_COUNT_UNSAFE");

        var length = checked(
            ((long)fileInfo.FileSizeHigh << 32) | fileInfo.FileSizeLow);
        return new DpoTrainerArtifactFileShape(
            length,
            fileInfo.NumberOfLinks,
            new DpoTrainerArtifactIdentity(
                DpoTrainerArtifactIdentityPlatform.Windows,
                identityInfo.VolumeSerialNumber,
                identityInfo.FileIdLow,
                identityInfo.FileIdHigh));
    }

    private static DpoTrainerArtifactFileShape CaptureLinuxArtifactFileShape(
        SafeFileHandle handle,
        string fullPath)
    {
        EnsureLinuxX64StatLayout();
        if (handle.IsInvalid || handle.IsClosed)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_PROOF_UNAVAILABLE");

        var descriptor = checked((int)handle.DangerousGetHandle().ToInt64());
        if (FStatLinux(descriptor, out var stat) != 0)
        {
            throw LinuxIOException(
                "inspect DPO artifact handle",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        if ((stat.Mode & LinuxFileTypeMask) != LinuxRegularFileType)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_NON_REGULAR_FILE");
        if (stat.LinkCount > uint.MaxValue)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_LINK_COUNT_UNSAFE");
        if (stat.LinkCount != 1)
            throw new IOException("DPO_ARTIFACT_OWNERSHIP_LINK_COUNT_UNSAFE");

        // Тренер и защищённый каталог должны одинаково определять физический
        // файл: устройство и номер объекта берутся через `fstat` одного и того же
        // дескриптора. При несовпадении мост останавливает очистку как опасную.
        return new DpoTrainerArtifactFileShape(
            checked((long)stat.Size),
            (uint)stat.LinkCount,
            new DpoTrainerArtifactIdentity(
                DpoTrainerArtifactIdentityPlatform.Linux,
                stat.Device,
                stat.Inode,
                0UL));
    }

    private static DpoTrainerArtifactIdentity CaptureLinuxArtifactIdentity(
        SafeFileHandle handle,
        string fullPath)
    {
        EnsureLinuxX64StatLayout();
        var descriptor = ToFileDescriptor(handle);
        if (FStatLinux(descriptor, out var stat) != 0)
        {
            throw LinuxIOException(
                "inspect DPO artifact handle",
                fullPath,
                Marshal.GetLastPInvokeError());
        }

        return new DpoTrainerArtifactIdentity(
            DpoTrainerArtifactIdentityPlatform.Linux,
            stat.Device,
            stat.Inode,
            0UL);
    }

    private static IOException LinuxIOException(string operation, string path, int error)
        => new($"DPO artifact custody failed to {operation} '{path}' (errno {error}).");

    private static IOException LinuxIOExceptionWithNativeCause(
        string operation,
        string path,
        int error)
        => new(
            $"DPO artifact custody failed to {operation} '{path}' (errno {error}).",
            new System.ComponentModel.Win32Exception(error));

    private static void EnsureLinuxX64StatLayout()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException(
                "DPO Linux file identity is verified only for the x64 stat layout.");
        }
    }

    private readonly record struct DpoTrainerArtifactFileShape(
        long Length,
        uint LinkCount,
        DpoTrainerArtifactIdentity Identity);

    private static int ToFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new IOException("DPO_ARTIFACT_HANDLE_UNAVAILABLE");
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileDispositionInformation
    {
        [MarshalAs(UnmanagedType.U1)]
        internal byte DeleteFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdInfo
    {
        internal ulong VolumeSerialNumber;
        internal ulong FileIdLow;
        internal ulong FileIdHigh;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        internal uint FileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        internal uint VolumeSerialNumber;
        internal uint FileSizeHigh;
        internal uint FileSizeLow;
        internal uint NumberOfLinks;
        internal uint FileIndexHigh;
        internal uint FileIndexLow;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxStat
    {
        internal ulong Device;
        internal ulong Inode;
        internal ulong LinkCount;
        internal uint Mode;
        internal uint Uid;
        internal uint Gid;
        internal int Padding0;
        internal ulong DeviceId;
        internal long Size;
        internal long BlockSize;
        internal long Blocks;
        internal long AccessTimeSeconds;
        internal long AccessTimeNanoseconds;
        internal long ModificationTimeSeconds;
        internal long ModificationTimeNanoseconds;
        internal long ChangeTimeSeconds;
        internal long ChangeTimeNanoseconds;
        internal long Unused0;
        internal long Unused1;
        internal long Unused2;
    }

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleWindows(
        SafeFileHandle fileHandle,
        out ByHandleFileInformation fileInformation);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        out FileIdInfo fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        ref FileDispositionInformation fileInformation,
        uint bufferSize);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinux(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "unlinkat", SetLastError = true)]
    private static extern int UnlinkAtLinux(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport("libc", EntryPoint = "renameat2", SetLastError = true)]
    private static extern int RenameAt2Linux(
        int oldDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        int newDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath,
        uint flags);

    [DllImport("libc", EntryPoint = "linkat", SetLastError = true)]
    private static extern int LinkAtLinux(
        int oldDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string oldPath,
        int newDirectoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath,
        int flags);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinux(
        int fileDescriptor,
        out LinuxStat status);

    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int FsyncLinux(int fileDescriptor);

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockLinux(int fileDescriptor, int operation);
}

public sealed class DpoTrainerPublicationObserver
{
    internal Action? AfterProcessLocalGateAcquired { get; init; }
    internal Action? AfterReservationAcquired { get; init; }
    internal Action? AfterPolicyFinalMove { get; init; }
    internal Action? AfterMetricsFinalMove { get; init; }
    internal Action? AfterPolicyFinalMoveBeforeProofForTesting { get; init; }
    internal Action? AfterMetricsFinalMoveBeforeProofForTesting { get; init; }
    internal Action<string, string>? MoveArtifactForTesting { get; init; }
    internal Action<DpoTrainerArtifactOwnershipProof>?
        AfterFallbackProofValidationBeforeDeleteForTesting { get; init; }
    internal Action<FileStream>? BeforePolicySerialization { get; init; }
    internal Action<FileStream>? AfterPolicySerializationBeforeProof { get; init; }
    internal Action? ReservationCreateFailureForTesting { get; init; }
    internal Action? ReservationOpenFailureForTesting { get; init; }
    internal Action? ReservationLockFailureForTesting { get; init; }
    internal Action? BeforeReservationUnlockForTesting { get; init; }
    internal Action? BeforeReservationStreamDisposeForTesting { get; init; }
    public Action<DpoTrainerArtifactOwnershipProof>? ArtifactOwnershipProven { get; init; }
    public Func<DpoTrainerArtifactOwnershipProof, bool>? TryAcceptArtifactOwnership { get; init; }
    public Action<DpoTrainingPublicationBundle>? BeforePolicyFinalMove { get; init; }
    public Action? PublicationFailed { get; init; }
}

public sealed record DpoVectorBatch(DpoBatch Batch, float[] TeachingVector, float Weight = 1f);
