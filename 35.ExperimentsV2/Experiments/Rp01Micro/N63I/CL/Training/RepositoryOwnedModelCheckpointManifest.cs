// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Identifies the previous checkpoint in the same repository-owned model tree.
/// </summary>
public sealed record RepositoryOwnedModelCheckpointParentIdentity(
    [property: JsonPropertyName("repositoryRelativeManifestPath")]
    string RepositoryRelativeManifestPath,
    [property: JsonPropertyName("step")]
    int Step,
    [property: JsonPropertyName("checkpointSha256")]
    string CheckpointSha256,
    [property: JsonPropertyName("checkpointByteLength")]
    long CheckpointByteLength);

/// <summary>
/// Binds a model checkpoint to the repository-owned DPO dataset manifest that
/// supplied its preference rows.
/// </summary>
public sealed record RepositoryOwnedDpoDatasetManifestIdentity(
    [property: JsonPropertyName("repositoryRelativeManifestPath")]
    string RepositoryRelativeManifestPath,
    [property: JsonPropertyName("manifestSha256")]
    string ManifestSha256);

/// <summary>
/// Captures the compatibility fingerprints a later loader must match before it
/// can resume from this checkpoint.
/// </summary>
public sealed record RepositoryOwnedModelRuntimeCompatibility(
    [property: JsonPropertyName("configSha256")]
    string ConfigSha256,
    [property: JsonPropertyName("topologySha256")]
    string TopologySha256,
    [property: JsonPropertyName("tokenizerSha256")]
    string TokenizerSha256,
    [property: JsonPropertyName("trainerSha256")]
    string TrainerSha256,
    [property: JsonPropertyName("schemaSha256")]
    string SchemaSha256,
    [property: JsonPropertyName("compatibilityKey")]
    string CompatibilityKey);

/// <summary>
/// Immutable caller-supplied identity for sealing one checkpoint.
/// </summary>
public sealed record RepositoryOwnedModelCheckpointManifestRequest(
    RepositoryOwnedModelCheckpointParentIdentity? ParentCheckpoint,
    string ColdStartReason,
    RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifest,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<string> ProvenanceRefs) // Сохраняем прежний публичный запрос публикации и его аргументы.
{ // Отдельное разрешение исходного состояния доступно только доверенным владельцам сборок.
    internal bool InitialBaselineNoUpdate { get; init; } // Обычный caller не может пометить переданные веса как необученный baseline.
} // Закрываем запрос, не добавляя публичного флага обучения.

public enum RepositoryOwnedModelCheckpointDpoApplication // Отличаем происхождение исходного состояния от уже применённых учебных пар.
{ // Роль становится частью запечатанной идентичности, а не текстом provenance.
    InitialBaselineNoUpdate = 1 // Владелец сам создал свежую модель и сохранил её без optimizer update.
} // Другие числовые роли запрещаются проверкой manifest.

/// <summary>
/// Immutable manifest for one sealed repository-owned common model checkpoint.
/// </summary>
public sealed record RepositoryOwnedModelCheckpointManifest(
    [property: JsonPropertyName("schema")]
    string Schema,
    [property: JsonPropertyName("context")]
    string Context,
    [property: JsonPropertyName("domain")]
    string Domain,
    [property: JsonPropertyName("designPattern")]
    string DesignPattern,
    [property: JsonPropertyName("modelId")]
    string ModelId,
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("step")]
    int Step,
    [property: JsonPropertyName("repositoryRelativeCheckpointDirectory")]
    string RepositoryRelativeCheckpointDirectory,
    [property: JsonPropertyName("repositoryRelativeWeightsPath")]
    string RepositoryRelativeWeightsPath,
    [property: JsonPropertyName("repositoryRelativeManifestPath")]
    string RepositoryRelativeManifestPath,
    [property: JsonPropertyName("weightsSha256")]
    string WeightsSha256,
    [property: JsonPropertyName("weightsByteLength")]
    long WeightsByteLength,
    [property: JsonPropertyName("parentCheckpoint")]
    RepositoryOwnedModelCheckpointParentIdentity? ParentCheckpoint,
    [property: JsonPropertyName("coldStartReason")]
    string ColdStartReason,
    [property: JsonPropertyName("dpoDatasetManifest")]
    RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifest,
    [property: JsonPropertyName("runtimeCompatibility")]
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    [property: JsonPropertyName("createdAtUtc")]
    string CreatedAtUtc,
    [property: JsonPropertyName("provenanceRefs")]
    IReadOnlyList<string> ProvenanceRefs,
    [property: JsonPropertyName("manifestSha256")]
    string ManifestSha256) // Сохраняем конструкцию старых manifest без изменения обязательных полей.
{ // Новая роль отсутствует у старой истории и не меняет её canonical bytes.
    [JsonPropertyName("dpoApplication")] // Закрепляем точное имя типизированной роли на диске.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Старый manifest не получает даже null-поле при сериализации.
    public RepositoryOwnedModelCheckpointDpoApplication? DpoApplication { get; init; } // Только новая исходная схема разрешает эту роль.
} // Завершаем совместимое расширение запечатанного manifest.

/// <summary>
/// Freshly reopened and verified repository-owned model checkpoint.
/// </summary>
public sealed record RepositoryOwnedModelCheckpointOpenResult(
    RepositoryOwnedModelCheckpointManifest Manifest,
    string WeightsSha256,
    long WeightsByteLength);

/// <summary>
/// Persists exact model-weight bytes into the canonical repository-owned tree
/// and seals them with a sibling manifest.
/// </summary>
public static class RepositoryOwnedModelCheckpointManifestSealer
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_INVALID";
    public const string ConflictErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_CONFLICT";
    public const string BusyErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_BUSY";
    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_UNAVAILABLE";

    public const string CurrentSchema =
        "repository-owned-model-checkpoint-manifest-v1";
    public const string InitialBaselineSchema = // Отделяем явно необученный корень от старой истории применённых пар.
        "repository-owned-model-checkpoint-manifest-v2-initial-baseline"; // Новая схема требует роль и строго первый parentless шаг.
    public const string CurrentContext =
        "continuous-learning-common-model-checkpoint-v1";

    private const string WeightsTempPrefix = ".weights.";
    private const string ManifestTempPrefix = ".manifest.";
    private const string TempSuffix = ".tmp";
    private const string CheckpointSealLockFileName =
        ".model-checkpoint-seal.lock";
    private const int FileBufferSize = 81920;
    private const int HashHexLength = 64;
    private const int MaximumSlugLength = 96;
    private const int MaximumTextLength = 256;
    private const int MaximumProvenanceRefCount = 32;
    private const int ManifestJsonMaxDepth = 8;
    private const long MaximumManifestBytes = 2L * 1024L * 1024L;
    internal const long MaximumWeightBytes = 16L * 1024L * 1024L * 1024L;
    private const long CheckpointSealLockOffset = 0;
    private const long CheckpointSealLockLength = 1;
    private static readonly TimeSpan DefaultCheckpointSealLockTimeout =
        TimeSpan.FromSeconds(10);
    private static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly object CheckpointProcessGateRegistrySync = new();
    private static readonly Dictionary<string, CheckpointProcessGateEntry>
        CheckpointProcessGateRegistry =
            new(OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

    private const string SchemaProperty = "schema";
    private const string ContextProperty = "context";
    private const string DomainProperty = "domain";
    private const string DesignPatternProperty = "designPattern";
    private const string ModelIdProperty = "modelId";
    private const string VersionProperty = "version";
    private const string StepProperty = "step";
    private const string RepositoryRelativeCheckpointDirectoryProperty =
        "repositoryRelativeCheckpointDirectory";
    private const string RepositoryRelativeWeightsPathProperty =
        "repositoryRelativeWeightsPath";
    private const string RepositoryRelativeManifestPathProperty =
        "repositoryRelativeManifestPath";
    private const string WeightsSha256Property = "weightsSha256";
    private const string WeightsByteLengthProperty = "weightsByteLength";
    private const string ParentCheckpointProperty = "parentCheckpoint";
    private const string ColdStartReasonProperty = "coldStartReason";
    private const string DpoDatasetManifestProperty = "dpoDatasetManifest";
    private const string DpoApplicationProperty = "dpoApplication"; // Имя роли участвует в canonical bytes и self hash.
    private const string RuntimeCompatibilityProperty = "runtimeCompatibility";
    private const string CreatedAtUtcProperty = "createdAtUtc";
    private const string ProvenanceRefsProperty = "provenanceRefs";
    private const string ManifestSha256Property = "manifestSha256";
    private const string CheckpointSha256Property = "checkpointSha256";
    private const string CheckpointByteLengthProperty = "checkpointByteLength";
    private const string DpoManifestSha256Property = "manifestSha256";
    private const string ConfigSha256Property = "configSha256";
    private const string TopologySha256Property = "topologySha256";
    private const string TokenizerSha256Property = "tokenizerSha256";
    private const string TrainerSha256Property = "trainerSha256";
    private const string SchemaSha256Property = "schemaSha256";
    private const string CompatibilityKeyProperty = "compatibilityKey";

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        AllowTrailingCommas = false,
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = ManifestJsonMaxDepth,
    };

    public static OperationResult<RepositoryOwnedModelCheckpointManifest>
        PersistAndSeal(
            RepositoryOwnedModelCheckpointAddress address,
            Stream weightBytes,
            RepositoryOwnedModelCheckpointManifestRequest request)
        => PersistAndSealCore(
            address,
            request,
            () => WriteWeightsStaging(address, weightBytes));

    public static OperationResult<RepositoryOwnedModelCheckpointManifest>
        PersistAndSeal(
            RepositoryOwnedModelCheckpointAddress address,
            Action<Stream> writeWeightBytes,
            RepositoryOwnedModelCheckpointManifestRequest request)
        => PersistAndSealCore(
            address,
            request,
            () => WriteWeightsStaging(address, writeWeightBytes));

    private static OperationResult<RepositoryOwnedModelCheckpointManifest>
        PersistAndSealCore(
            RepositoryOwnedModelCheckpointAddress address,
            RepositoryOwnedModelCheckpointManifestRequest request,
            Func<OperationResult<WeightStagingObservation>> stageWeights)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedModelCheckpointManifest>();

        var requestValidation = ValidateRequest(address, request);
        if (!requestValidation.IsSuccess)
            return requestValidation.MapError<RepositoryOwnedModelCheckpointManifest>();

        try
        {
            var canonicalCheckpointRoot = CanonicalRoot(address.CheckpointRoot);
            var timeoutBudget = Stopwatch.StartNew();
            var processLeaseResult = AcquireProcessCheckpointSealLease(
                canonicalCheckpointRoot,
                timeoutBudget);
            if (!processLeaseResult.IsSuccess
                || processLeaseResult.Value is null)
            {
                return processLeaseResult
                    .MapError<RepositoryOwnedModelCheckpointManifest>();
            }

            using var processLease = processLeaseResult.Value;
            var rootReady = EnsureWritableCheckpointRoot(address);
            if (!rootReady.IsSuccess)
                return rootReady.MapError<RepositoryOwnedModelCheckpointManifest>();

            var fileLeaseResult = AcquireCheckpointSealLease(
                address,
                canonicalCheckpointRoot,
                timeoutBudget);
            if (!fileLeaseResult.IsSuccess || fileLeaseResult.Value is null)
                return fileLeaseResult.MapError<RepositoryOwnedModelCheckpointManifest>();

            using var fileLease = fileLeaseResult.Value;
            var revalidation = ValidateAddress(address);
            rootReady = EnsureWritableCheckpointRoot(address);
            if (!revalidation.IsSuccess
                || !rootReady.IsSuccess
                || !PathsEqual(address.CheckpointRoot, canonicalCheckpointRoot)
                || !fileLease.MatchesRequestedPath(
                    CheckpointSealLockPath(canonicalCheckpointRoot))
                || !fileLease.HasExpectedLength()
                || AnyExistingPathSegmentIsReparsePoint(address.CheckpointRoot)
                || AnyExistingPathSegmentIsReparsePoint(
                    CheckpointSealLockPath(canonicalCheckpointRoot))
                || !HasSafeUnixDirectoryMode(address.CheckpointRoot)
                || !HasSafeUnixFileMode(
                    CheckpointSealLockPath(canonicalCheckpointRoot)))
            {
                if (!revalidation.IsSuccess)
                    return revalidation.MapError<RepositoryOwnedModelCheckpointManifest>();
                if (!rootReady.IsSuccess)
                    return rootReady.MapError<RepositoryOwnedModelCheckpointManifest>();
                return Unavailable<RepositoryOwnedModelCheckpointManifest>();
            }

            return PersistAndSealUnderLease(
                address,
                stageWeights,
                request);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedModelCheckpointManifest>();
        }
    }

    public static OperationResult<RepositoryOwnedModelCheckpointOpenResult>
        OpenSealed(RepositoryOwnedModelCheckpointAddress address)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedModelCheckpointOpenResult>();

        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareModelAndDpoContextForStrictReaders(address.RepositoryRoot);
        return custodyContext.IsSuccess && custodyContext.Value is not null
            ? OpenSealedWithPreparedCloneCustody(address, custodyContext.Value)
            : Unavailable<RepositoryOwnedModelCheckpointOpenResult>();
    }

    internal static OperationResult<RepositoryOwnedModelCheckpointOpenResult>
        OpenSealedWithPreparedCloneCustody(
            RepositoryOwnedModelCheckpointAddress address,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedModelCheckpointOpenResult>();

        try
        {
            // WHY: candidate scans and parent recursion share one stack-local
            // custody context. A mismatched context fails closed rather than
            // becoming an internal bypass around clone preparation.
            if (!custodyContext.CoversModelAndDpoRoots(address.RepositoryRoot))
                return Unavailable<RepositoryOwnedModelCheckpointOpenResult>();

            if (!File.Exists(address.ManifestPath))
                return Invalid<RepositoryOwnedModelCheckpointOpenResult>();

            var manifest = ReadStrictManifest(address.ManifestPath);
            if (!manifest.IsSuccess || manifest.Value is null)
                return manifest.MapError<RepositoryOwnedModelCheckpointOpenResult>();

            var manifestValidation = ValidateManifestForAddress(
                address,
                manifest.Value,
                custodyContext);
            if (!manifestValidation.IsSuccess)
                return manifestValidation
                    .MapError<RepositoryOwnedModelCheckpointOpenResult>();

            var dpoManifestValidation = ValidateSealedDpoDatasetManifest(
                address,
                manifest.Value.DpoDatasetManifest,
                custodyContext);
            if (!dpoManifestValidation.IsSuccess)
                return dpoManifestValidation
                    .MapError<RepositoryOwnedModelCheckpointOpenResult>();

            var weights = ReadStableFileHashAndSize(address.WeightsPath);
            if (!weights.IsSuccess || weights.Value is null)
                return weights.MapError<RepositoryOwnedModelCheckpointOpenResult>();
            if (weights.Value.ByteLength != manifest.Value.WeightsByteLength
                || !StringComparer.Ordinal.Equals(
                    weights.Value.Sha256,
                    manifest.Value.WeightsSha256))
            {
                return Invalid<RepositoryOwnedModelCheckpointOpenResult>();
            }

            return OperationResult<RepositoryOwnedModelCheckpointOpenResult>.Ok(
                new RepositoryOwnedModelCheckpointOpenResult(
                    manifest.Value,
                    weights.Value.Sha256,
                    weights.Value.ByteLength));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedModelCheckpointOpenResult>();
        }
    }

    private static OperationResult<RepositoryOwnedModelCheckpointManifest>
        PersistAndSealUnderLease(
            RepositoryOwnedModelCheckpointAddress address,
            Func<OperationResult<WeightStagingObservation>> stageWeights,
            RepositoryOwnedModelCheckpointManifestRequest request)
    {
        var stagedWeights = stageWeights();
        if (!stagedWeights.IsSuccess || stagedWeights.Value is null)
            return stagedWeights.MapError<RepositoryOwnedModelCheckpointManifest>();

        var manifest = BuildManifest(address, stagedWeights.Value, request);
        var canonicalManifestBytes = WriteCanonicalManifestBytes(manifest);
        var manifestStagingPath = Path.Combine(
            address.CheckpointRoot,
            ManifestTempPrefix + Guid.NewGuid().ToString("N") + TempSuffix);
        var weightsPublished = false;
        var manifestPublished = false;
        try
        {
            if (File.Exists(address.ManifestPath))
            {
                var existing = OpenSealed(address);
                if (!existing.IsSuccess || existing.Value is null)
                    return existing.MapError<RepositoryOwnedModelCheckpointManifest>();
                return ManifestBytesEqual(
                        existing.Value.Manifest,
                        canonicalManifestBytes)
                    ? OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                        existing.Value.Manifest)
                    : Conflict<RepositoryOwnedModelCheckpointManifest>();
            }

            if (File.Exists(address.WeightsPath))
            {
                var orphanWeights = ReadStableFileHashAndSize(address.WeightsPath);
                if (!orphanWeights.IsSuccess || orphanWeights.Value is null)
                    return orphanWeights.MapError<RepositoryOwnedModelCheckpointManifest>();
                if (!StringComparer.Ordinal.Equals(
                        orphanWeights.Value.Sha256,
                        stagedWeights.Value.Sha256)
                    || orphanWeights.Value.ByteLength
                    != stagedWeights.Value.ByteLength)
                {
                    return Conflict<RepositoryOwnedModelCheckpointManifest>();
                }

                // Если процесс умер после публикации weights.bin, но до
                // manifest.json, повтор с теми же байтами может допечатать
                // только паспорт. Чужие/отличающиеся orphan-байты остаются
                // нетронутыми и не превращаются в sealed checkpoint.
                WriteManifestStaging(
                    manifestStagingPath,
                    canonicalManifestBytes);

                if (File.Exists(address.ManifestPath))
                    return Conflict<RepositoryOwnedModelCheckpointManifest>();

                File.Move(manifestStagingPath, address.ManifestPath);
                manifestPublished = true;

                var recovered = OpenSealed(address);
                return recovered.IsSuccess && recovered.Value is not null
                    ? OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                        recovered.Value.Manifest)
                    : recovered.MapError<RepositoryOwnedModelCheckpointManifest>();
            }

            WriteManifestStaging(
                manifestStagingPath,
                canonicalManifestBytes);

            File.Move(stagedWeights.Value.TemporaryPath, address.WeightsPath);
            weightsPublished = true;

            if (File.Exists(address.ManifestPath))
                return Conflict<RepositoryOwnedModelCheckpointManifest>();

            // Сначала становятся видны байты, и только потом паспорт. Читатель
            // признаёт checkpoint sealed только по финальному manifest.json, так
            // что смерть между двумя rename не превращает половину записи в канон.
            File.Move(manifestStagingPath, address.ManifestPath);
            manifestPublished = true;

            var opened = OpenSealed(address);
            return opened.IsSuccess && opened.Value is not null
                ? OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                    opened.Value.Manifest)
                : opened.MapError<RepositoryOwnedModelCheckpointManifest>();
        }
        catch (IOException) when (File.Exists(address.ManifestPath))
        {
            var existing = OpenSealed(address);
            if (!existing.IsSuccess || existing.Value is null)
                return existing.MapError<RepositoryOwnedModelCheckpointManifest>();
            return ManifestBytesEqual(
                    existing.Value.Manifest,
                    canonicalManifestBytes)
                ? OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                    existing.Value.Manifest)
                : Conflict<RepositoryOwnedModelCheckpointManifest>();
        }
        catch (IOException) when (File.Exists(address.WeightsPath))
        {
            return Conflict<RepositoryOwnedModelCheckpointManifest>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedModelCheckpointManifest>();
        }
        finally
        {
            if (!weightsPublished)
                DeleteOwnStaging(address, stagedWeights.Value.TemporaryPath);
            if (!manifestPublished)
                DeleteOwnStaging(address, manifestStagingPath);
        }
    }

    private static OperationResult<WeightStagingObservation> WriteWeightsStaging(
        RepositoryOwnedModelCheckpointAddress address,
        Stream weightBytes)
    {
        if (weightBytes is null || !weightBytes.CanRead)
            return Invalid<WeightStagingObservation>();

        var temporaryPath = Path.Combine(
            address.CheckpointRoot,
            WeightsTempPrefix + Guid.NewGuid().ToString("N") + TempSuffix);
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[FileBufferSize];
            long total = 0;
            var exceededMaximumBytes = false;
            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough,
                       FileBufferSize))
            {
                while (true)
                {
                    var read = weightBytes.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;

                    if (read > MaximumWeightBytes - total)
                    {
                        exceededMaximumBytes = true;
                        break;
                    }

                    total += read;
                    hash.AppendData(buffer.AsSpan(0, read));
                    stream.Write(buffer.AsSpan(0, read));
                }

                stream.Flush(flushToDisk: true);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporaryPath))
                {
                    return Unavailable<WeightStagingObservation>();
                }
            }

            if (exceededMaximumBytes
                || total <= 0
                || AnyExistingPathSegmentIsReparsePoint(temporaryPath))
            {
                DeleteTemporaryPath(temporaryPath);
                return Invalid<WeightStagingObservation>();
            }

            return OperationResult<WeightStagingObservation>.Ok(
                new WeightStagingObservation(
                    temporaryPath,
                    Convert.ToHexString(hash.GetHashAndReset())
                        .ToLowerInvariant(),
                    total));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryPath(temporaryPath);
            return Unavailable<WeightStagingObservation>();
        }
    }

    private static OperationResult<WeightStagingObservation> WriteWeightsStaging(
        RepositoryOwnedModelCheckpointAddress address,
        Action<Stream> writeWeightBytes)
    {
        if (writeWeightBytes is null)
            return Invalid<WeightStagingObservation>();

        var temporaryPath = Path.Combine(
            address.CheckpointRoot,
            WeightsTempPrefix + Guid.NewGuid().ToString("N") + TempSuffix);
        try
        {
            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough,
                       FileBufferSize))
            {
                // WHY: trained model weights can be huge. The model writes
                // directly into the repository-owned staging file, then this
                // sealer reopens that same file to measure hash and length
                // before publication. No cache, quarantine, TestResults, or
                // alternate worktree becomes the hidden authority for weights.
                writeWeightBytes(stream);
                stream.Flush(flushToDisk: true);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporaryPath))
                {
                    return Unavailable<WeightStagingObservation>();
                }
            }

            if (AnyExistingPathSegmentIsReparsePoint(temporaryPath))
            {
                DeleteTemporaryPath(temporaryPath);
                return Invalid<WeightStagingObservation>();
            }

            var observation = ReadStableFileHashAndSize(temporaryPath);
            if (!observation.IsSuccess || observation.Value is null)
            {
                DeleteTemporaryPath(temporaryPath);
                return observation.MapError<WeightStagingObservation>();
            }

            return OperationResult<WeightStagingObservation>.Ok(
                new WeightStagingObservation(
                    temporaryPath,
                    observation.Value.Sha256,
                    observation.Value.ByteLength));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DeleteTemporaryPath(temporaryPath);
            return Unavailable<WeightStagingObservation>();
        }
    }

    private static void WriteManifestStaging(
        string temporaryPath,
        byte[] canonicalManifestBytes)
    {
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
            temporaryPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            FileOptions.WriteThrough,
            FileBufferSize);
        stream.Write(canonicalManifestBytes);
        stream.Flush(flushToDisk: true);
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                stream.SafeFileHandle,
                temporaryPath))
        {
            throw new IOException("Manifest staging handle did not match path.");
        }
    }

    private static RepositoryOwnedModelCheckpointManifest BuildManifest(
        RepositoryOwnedModelCheckpointAddress address,
        WeightStagingObservation weights,
        RepositoryOwnedModelCheckpointManifestRequest request)
    {
        var unsigned = new RepositoryOwnedModelCheckpointManifest(
            request.InitialBaselineNoUpdate ? InitialBaselineSchema : CurrentSchema, // Обычная публикация сохраняет прежнюю схему и applied semantics.
            CurrentContext,
            address.DomainSlug,
            address.DesignPatternSlug,
            address.ModelIdSlug,
            address.ModelVersion,
            address.Step,
            address.CheckpointRootRelativePath,
            address.WeightsRelativePath,
            address.ManifestRelativePath,
            weights.Sha256,
            weights.ByteLength,
            request.ParentCheckpoint,
            request.ParentCheckpoint is null
                ? request.ColdStartReason
                : string.Empty,
            request.DpoDatasetManifest,
            request.RuntimeCompatibility,
            request.CreatedAtUtc.UtcDateTime.ToString(
                "O",
                CultureInfo.InvariantCulture),
            Array.AsReadOnly(request.ProvenanceRefs.ToArray()),
            string.Empty) // Сначала строим manifest без подписи для прежнего canonical self hash.
        { // Роль выдаётся только внутренним запросом доверенного initial owner.
            DpoApplication = request.InitialBaselineNoUpdate // Публичная публикация не может включить новую роль.
                ? RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate : null // Старые canonical bytes остаются побайтно прежними.
        }; // Роль включена до вычисления manifestSha256.
        var manifestSha = ComputeSha256(WriteCanonicalManifestBytes(unsigned));
        return unsigned with
        {
            ManifestSha256 = manifestSha,
        };
    }

    private static OperationResult<RepositoryOwnedModelCheckpointManifest>
        ReadStrictManifest(string manifestPath)
    {
        var bytes = ReadBoundedStableFileBytes(manifestPath, MaximumManifestBytes);
        if (!bytes.IsSuccess || bytes.Value is null)
            return bytes.MapError<RepositoryOwnedModelCheckpointManifest>();

        try
        {
            using var document = JsonDocument.Parse(
                bytes.Value,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = ManifestJsonMaxDepth,
                });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return Invalid<RepositoryOwnedModelCheckpointManifest>();

            var manifest =
                JsonSerializer.Deserialize<RepositoryOwnedModelCheckpointManifest>(
                    bytes.Value,
                    ManifestJsonOptions);
            if (manifest is null)
                return Invalid<RepositoryOwnedModelCheckpointManifest>();

            var selfHash = ComputeSha256(WriteCanonicalManifestBytes(
                manifest with
                {
                    ManifestSha256 = string.Empty,
                }));
            if (!StringComparer.Ordinal.Equals(selfHash, manifest.ManifestSha256))
                return Invalid<RepositoryOwnedModelCheckpointManifest>();

            var canonical = WriteCanonicalManifestBytes(manifest);
            if (!bytes.Value.SequenceEqual(canonical))
                return Invalid<RepositoryOwnedModelCheckpointManifest>();

            return OperationResult<RepositoryOwnedModelCheckpointManifest>.Ok(
                manifest);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedModelCheckpointManifest>();
        }
    }

    private static OperationResult<bool> ValidateManifestForAddress(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointManifest manifest,
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
            custodyContext)
    {
        if (!ValidDpoApplication(manifest) // Проверяем согласованность схемы, роли и корня до допуска checkpoint.
            || !StringComparer.Ordinal.Equals(manifest.Context, CurrentContext)
            || !StringComparer.Ordinal.Equals(manifest.Domain, address.DomainSlug)
            || !StringComparer.Ordinal.Equals(
                manifest.DesignPattern,
                address.DesignPatternSlug)
            || !StringComparer.Ordinal.Equals(manifest.ModelId, address.ModelIdSlug)
            || !StringComparer.Ordinal.Equals(manifest.Version, address.ModelVersion)
            || manifest.Step != address.Step
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeCheckpointDirectory,
                address.CheckpointRootRelativePath)
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeWeightsPath,
                address.WeightsRelativePath)
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeManifestPath,
                address.ManifestRelativePath)
            || manifest.WeightsByteLength <= 0
            || !IsCanonicalSha256Hex(manifest.WeightsSha256)
            || !IsCanonicalSha256Hex(manifest.ManifestSha256)
            || !ValidateParent(
                address,
                manifest.ParentCheckpoint,
                manifest.ColdStartReason,
                custodyContext)
            || !ValidateDpoDatasetIdentity(address, manifest.DpoDatasetManifest)
            || !ValidateRuntimeCompatibility(manifest.RuntimeCompatibility)
            || !ValidateCreatedAtUtc(manifest.CreatedAtUtc)
            || !ValidateProvenanceRefs(manifest.ProvenanceRefs))
        {
            return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateAddress(
        RepositoryOwnedModelCheckpointAddress? address)
    {
        if (address is null)
            return Invalid<bool>();
        try
        {
            return RepositoryOwnedTrainingArtifactLayout
                .IsValidModelCheckpointAddress(address)
                ? OperationResult<bool>.Ok(true)
                : Invalid<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<bool>();
        }
    }

    private static OperationResult<bool> ValidateRequest(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointManifestRequest? request)
    {
        if (request is null // Сохраняем прежний отказ отсутствующего запроса.
            || (request.InitialBaselineNoUpdate && (address.Step != 1 || request.ParentCheckpoint is not null)) // Initial owner может запечатать только исходный parentless шаг.
            || !ValidateParent(
                address,
                request.ParentCheckpoint,
                request.ColdStartReason)
            || !ValidateDpoDatasetIdentity(address, request.DpoDatasetManifest)
            || !ValidateRuntimeCompatibility(request.RuntimeCompatibility)
            || !ValidateProvenanceRefs(request.ProvenanceRefs))
        {
            return Invalid<bool>();
        }

        var dpoManifest = ValidateSealedDpoDatasetManifest(
            address,
            request.DpoDatasetManifest);
        if (!dpoManifest.IsSuccess)
            return dpoManifest;

        return OperationResult<bool>.Ok(true);
    }

    private static bool ValidDpoApplication(RepositoryOwnedModelCheckpointManifest manifest) // Fail closed для неизвестной, удалённой или противоречивой роли.
        => StringComparer.Ordinal.Equals(manifest.Schema, CurrentSchema) // История v1 остаётся историей применённых пар, включая первый cold-start шаг.
            ? manifest.DpoApplication is null // Добавление роли к старой схеме не превращает обученные веса в baseline.
            : StringComparer.Ordinal.Equals(manifest.Schema, InitialBaselineSchema) // Только новая схема может объявлять проверенный initial baseline.
                && manifest.DpoApplication == RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate // Принимаем ровно одну известную типизированную роль.
                && manifest.Step == 1 && manifest.ParentCheckpoint is null // Дочерний или поздний checkpoint никогда не освобождает применённую пару.
                && IsBoundedHumanText(manifest.ColdStartReason); // Сохраняем обязательную причину разрешённого холодного старта.

    private static bool ValidateParent(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointParentIdentity? parent,
        string? coldStartReason)
        => ValidateParent(
            address,
            parent,
            coldStartReason,
            custodyContext: null);

    private static bool ValidateParent(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointParentIdentity? parent,
        string? coldStartReason,
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext?
            custodyContext)
    {
        if (parent is null)
        {
            return address.Step == 1
                && IsBoundedHumanText(coldStartReason);
        }

        if (!string.IsNullOrEmpty(coldStartReason)
            || parent.Step <= 0
            || parent.Step >= address.Step
            || parent.CheckpointByteLength <= 0
            || !IsCanonicalSha256Hex(parent.CheckpointSha256))
        {
            return false;
        }

        var expectedParentPath = ParentManifestPath(address, parent.Step);
        if (!StringComparer.Ordinal.Equals(
                parent.RepositoryRelativeManifestPath,
                expectedParentPath))
        {
            return false;
        }

        var parentAddress =
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                address.RepositoryRoot,
                address.DomainSlug,
                address.DesignPatternSlug,
                address.ModelIdSlug,
                address.ModelVersion,
                parent.Step);
        if (!parentAddress.IsSuccess || parentAddress.Value is null)
            return false;

        // Родитель не принимается как строка из запроса: мы заново строим его
        // canonical address в той же ветке дерева и открываем sealed manifest.
        // Так child-checkpoint не может сослаться на другой домен, чужую модель
        // или на отсутствующие/повреждённые веса.
        var openedParent = custodyContext is null
            ? OpenSealed(parentAddress.Value)
            : OpenSealedWithPreparedCloneCustody(parentAddress.Value, custodyContext);
        return openedParent.IsSuccess
               && openedParent.Value is not null
               && openedParent.Value.Manifest.Step == parent.Step
               && StringComparer.Ordinal.Equals(
                   openedParent.Value.Manifest.RepositoryRelativeManifestPath,
                   parent.RepositoryRelativeManifestPath)
               && StringComparer.Ordinal.Equals(
                   openedParent.Value.Manifest.WeightsSha256,
                   parent.CheckpointSha256)
               && openedParent.Value.Manifest.WeightsByteLength
               == parent.CheckpointByteLength
               && StringComparer.Ordinal.Equals(
                   openedParent.Value.WeightsSha256,
                   parent.CheckpointSha256)
               && openedParent.Value.WeightsByteLength
               == parent.CheckpointByteLength;
    }

    private static bool ValidateDpoDatasetIdentity(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedDpoDatasetManifestIdentity? identity)
    {
        if (identity is null
            || !IsCanonicalSha256Hex(identity.ManifestSha256)
            || !TryParseDpoDatasetManifestPath(
                identity.RepositoryRelativeManifestPath,
                out var domainSlug,
                out var designPatternSlug,
                out _))
        {
            return false;
        }

        return StringComparer.Ordinal.Equals(domainSlug, address.DomainSlug)
               && StringComparer.Ordinal.Equals(
                   designPatternSlug,
                   address.DesignPatternSlug);
    }

    private static OperationResult<bool> ValidateSealedDpoDatasetManifest(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedDpoDatasetManifestIdentity identity)
        => ValidateSealedDpoDatasetManifest(
            address,
            identity,
            custodyContext: null);

    private static OperationResult<bool> ValidateSealedDpoDatasetManifest(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedDpoDatasetManifestIdentity identity,
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext?
            custodyContext)
    {
        if (!TryParseDpoDatasetManifestPath(
                identity.RepositoryRelativeManifestPath,
                out var domainSlug,
                out var designPatternSlug,
                out var datasetVersion)
            || !StringComparer.Ordinal.Equals(domainSlug, address.DomainSlug)
            || !StringComparer.Ordinal.Equals(
                designPatternSlug,
                address.DesignPatternSlug))
        {
            return Invalid<bool>();
        }

        var dpoAddress = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                address.RepositoryRoot,
                domainSlug!,
                designPatternSlug!,
                datasetVersion!);
        if (!dpoAddress.IsSuccess || dpoAddress.Value is null)
            return dpoAddress.MapError<bool>();

        if (!StringComparer.Ordinal.Equals(
                dpoAddress.Value.ManifestRelativePath,
                identity.RepositoryRelativeManifestPath))
        {
            return Invalid<bool>();
        }

        // WHY: JSON с совпавшим hash ещё не доказывает, что обучающие пары
        // существуют. Checkpoint можно продолжать только после того, как DPO
        // sealer заново открыл набор, проверил каждый pair-файл и подтвердил,
        // что identity manifest указывает именно на этот sealed dataset.
        var openedDpoDataset = custodyContext is null
            ? RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(
                dpoAddress.Value)
            : RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(
                dpoAddress.Value,
                custodyContext);
        if (!openedDpoDataset.IsSuccess || openedDpoDataset.Value is null)
        {
            return StringComparer.Ordinal.Equals(
                    openedDpoDataset.ErrorCode,
                    RepositoryOwnedDpoDatasetManifestSealer
                        .UnavailableErrorCode)
                ? Unavailable<bool>()
                : Invalid<bool>();
        }

        return StringComparer.Ordinal.Equals(
                   openedDpoDataset.Value.Manifest.ManifestSha256,
                   identity.ManifestSha256)
               && StringComparer.Ordinal.Equals(
                   openedDpoDataset.Value.Manifest.Domain,
                   address.DomainSlug)
               && StringComparer.Ordinal.Equals(
                   openedDpoDataset.Value.Manifest.DesignPattern,
                   address.DesignPatternSlug)
               && StringComparer.Ordinal.Equals(
                   openedDpoDataset.Value.Manifest.DatasetVersion,
                   datasetVersion)
            ? OperationResult<bool>.Ok(true)
            : Invalid<bool>();
    }

    private static bool TryParseDpoDatasetManifestPath(
        string? value,
        out string? domainSlug,
        out string? designPatternSlug,
        out string? datasetVersion)
    {
        domainSlug = null;
        designPatternSlug = null;
        datasetVersion = null;
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumTextLength
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl)
            || value.Contains('\\')
            || value.Contains("//", StringComparison.Ordinal)
            || value.Contains("/./", StringComparison.Ordinal)
            || value.Contains("/../", StringComparison.Ordinal)
            || value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("../", StringComparison.Ordinal)
            || Path.IsPathRooted(value))
        {
            return false;
        }

        var segments = value.Split('/');
        if (segments.Length != 6
            || !StringComparer.Ordinal.Equals(segments[0], "training-data")
            || !StringComparer.Ordinal.Equals(segments[1], "dpo")
            || !StringComparer.Ordinal.Equals(
                segments[5],
                RepositoryOwnedTrainingArtifactLayout.ManifestFileName)
            || !IsSafeSlugSegment(segments[2])
            || !IsSafeSlugSegment(segments[3])
            || !IsSafeSlugSegment(segments[4]))
        {
            return false;
        }

        domainSlug = segments[2];
        designPatternSlug = segments[3];
        datasetVersion = segments[4];
        return true;
    }

    private static bool ValidateRuntimeCompatibility(
        RepositoryOwnedModelRuntimeCompatibility? compatibility)
        => compatibility is not null
           && IsCanonicalSha256Hex(compatibility.ConfigSha256)
           && IsCanonicalSha256Hex(compatibility.TopologySha256)
           && IsCanonicalSha256Hex(compatibility.TokenizerSha256)
           && IsCanonicalSha256Hex(compatibility.TrainerSha256)
           && IsCanonicalSha256Hex(compatibility.SchemaSha256)
           && IsSafeToken(compatibility.CompatibilityKey);

    private static bool ValidateCreatedAtUtc(string? value)
        => IsBoundedHumanText(value)
           && DateTimeOffset.TryParse(
               value,
               CultureInfo.InvariantCulture,
               DateTimeStyles.RoundtripKind,
               out var parsed)
           && parsed.Offset == TimeSpan.Zero;

    private static bool ValidateProvenanceRefs(IReadOnlyList<string>? refs)
    {
        if (refs is null
            || refs.Count == 0
            || refs.Count > MaximumProvenanceRefCount
            || refs.Distinct(StringComparer.Ordinal).Count() != refs.Count)
        {
            return false;
        }

        return refs.All(IsBoundedHumanText);
    }

    private static bool IsSafeRepositoryRelativeManifestPath(
        string? value,
        string requiredPrefix,
        string requiredSuffix)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumTextLength
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.Any(char.IsControl)
            || value.Contains('\\')
            || value.Contains("//", StringComparison.Ordinal)
            || value.Contains("/./", StringComparison.Ordinal)
            || value.Contains("/../", StringComparison.Ordinal)
            || value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("../", StringComparison.Ordinal)
            || Path.IsPathRooted(value)
            || !value.StartsWith(requiredPrefix, StringComparison.Ordinal)
            || !value.EndsWith(requiredSuffix, StringComparison.Ordinal))
        {
            return false;
        }

        return value.Split('/').All(segment =>
            !string.IsNullOrWhiteSpace(segment)
            && segment is not "." and not "..");
    }

    private static bool IsSafeToken(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumTextLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    private static bool IsSafeSlugSegment(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumSlugLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value is not "." and not ".."
           && !Path.IsPathRooted(value)
           && !value.Contains('/')
           && !value.Contains('\\')
           && !value.Contains('.')
           && value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character == '-');

    private static bool IsBoundedHumanText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumTextLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !Path.IsPathRooted(value)
           && !value.Contains('\\')
           && !value.Contains("..", StringComparison.Ordinal)
           && value.All(static character => !char.IsControl(character));

    private static string ParentManifestPath(
        RepositoryOwnedModelCheckpointAddress address,
        int step)
        => string.Join(
            '/',
            RepositoryOwnedTrainingArtifactLayout.ModelArtifactRootRelativePath,
            address.DomainSlug,
            address.DesignPatternSlug,
            address.ModelIdSlug,
            address.ModelVersion,
            RepositoryOwnedTrainingArtifactLayout.CheckpointsDirectoryName,
            step.ToString("D6", CultureInfo.InvariantCulture),
            RepositoryOwnedTrainingArtifactLayout.ManifestFileName);

    private static bool ManifestBytesEqual(
        RepositoryOwnedModelCheckpointManifest manifest,
        byte[] expectedCanonicalBytes)
        => WriteCanonicalManifestBytes(manifest)
            .SequenceEqual(expectedCanonicalBytes);

    private static byte[] WriteCanonicalManifestBytes(
        RepositoryOwnedModelCheckpointManifest manifest)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString(SchemaProperty, manifest.Schema);
            writer.WriteString(ContextProperty, manifest.Context);
            writer.WriteString(DomainProperty, manifest.Domain);
            writer.WriteString(DesignPatternProperty, manifest.DesignPattern);
            writer.WriteString(ModelIdProperty, manifest.ModelId);
            writer.WriteString(VersionProperty, manifest.Version);
            writer.WriteNumber(StepProperty, manifest.Step);
            writer.WriteString(
                RepositoryRelativeCheckpointDirectoryProperty,
                manifest.RepositoryRelativeCheckpointDirectory);
            writer.WriteString(
                RepositoryRelativeWeightsPathProperty,
                manifest.RepositoryRelativeWeightsPath);
            writer.WriteString(
                RepositoryRelativeManifestPathProperty,
                manifest.RepositoryRelativeManifestPath);
            writer.WriteString(WeightsSha256Property, manifest.WeightsSha256);
            writer.WriteNumber(
                WeightsByteLengthProperty,
                manifest.WeightsByteLength);
            writer.WritePropertyName(ParentCheckpointProperty);
            WriteParentCheckpoint(writer, manifest.ParentCheckpoint);
            writer.WriteString(ColdStartReasonProperty, manifest.ColdStartReason);
            writer.WritePropertyName(DpoDatasetManifestProperty);
            WriteDpoDatasetIdentity(writer, manifest.DpoDatasetManifest);
            if (manifest.DpoApplication is { } application) // Не добавляем новое поле в старые canonical manifests.
                writer.WriteNumber(DpoApplicationProperty, (int)application); // Роль запечатывается тем же self hash вместе с весами и происхождением.
            writer.WritePropertyName(RuntimeCompatibilityProperty);
            WriteRuntimeCompatibility(writer, manifest.RuntimeCompatibility);
            writer.WriteString(CreatedAtUtcProperty, manifest.CreatedAtUtc);
            writer.WriteStartArray(ProvenanceRefsProperty);
            foreach (var provenanceRef in manifest.ProvenanceRefs)
                writer.WriteStringValue(provenanceRef);
            writer.WriteEndArray();
            writer.WriteString(ManifestSha256Property, manifest.ManifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static void WriteParentCheckpoint(
        Utf8JsonWriter writer,
        RepositoryOwnedModelCheckpointParentIdentity? parent)
    {
        if (parent is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString(
            RepositoryRelativeManifestPathProperty,
            parent.RepositoryRelativeManifestPath);
        writer.WriteNumber(StepProperty, parent.Step);
        writer.WriteString(CheckpointSha256Property, parent.CheckpointSha256);
        writer.WriteNumber(
            CheckpointByteLengthProperty,
            parent.CheckpointByteLength);
        writer.WriteEndObject();
    }

    private static void WriteDpoDatasetIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoDatasetManifestIdentity identity)
    {
        writer.WriteStartObject();
        writer.WriteString(
            RepositoryRelativeManifestPathProperty,
            identity.RepositoryRelativeManifestPath);
        writer.WriteString(DpoManifestSha256Property, identity.ManifestSha256);
        writer.WriteEndObject();
    }

    private static void WriteRuntimeCompatibility(
        Utf8JsonWriter writer,
        RepositoryOwnedModelRuntimeCompatibility compatibility)
    {
        writer.WriteStartObject();
        writer.WriteString(ConfigSha256Property, compatibility.ConfigSha256);
        writer.WriteString(TopologySha256Property, compatibility.TopologySha256);
        writer.WriteString(TokenizerSha256Property, compatibility.TokenizerSha256);
        writer.WriteString(TrainerSha256Property, compatibility.TrainerSha256);
        writer.WriteString(SchemaSha256Property, compatibility.SchemaSha256);
        writer.WriteString(CompatibilityKeyProperty, compatibility.CompatibilityKey);
        writer.WriteEndObject();
    }

    private static OperationResult<byte[]> ReadBoundedStableFileBytes(
        string path,
        long maximumBytes)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath)
                || !File.Exists(fullPath))
            {
                return Invalid<byte[]>();
            }

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath))
            {
                return Invalid<byte[]>();
            }

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                maximumBytes);
            if (bytes is null
                || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath))
            {
                return Invalid<byte[]>();
            }

            return OperationResult<byte[]>.Ok(bytes);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<byte[]>();
        }
    }

    private static OperationResult<WeightFileObservation>
        ReadStableFileHashAndSize(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath)
                || !File.Exists(fullPath))
            {
                return Invalid<WeightFileObservation>();
            }

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath))
            {
                return Invalid<WeightFileObservation>();
            }

            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[FileBufferSize];
            long total = 0;
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                checked
                {
                    total += read;
                }

                if (total > MaximumWeightBytes)
                    return Invalid<WeightFileObservation>();

                hash.AppendData(buffer.AsSpan(0, read));
            }

            if (total <= 0
                || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath))
            {
                return Invalid<WeightFileObservation>();
            }

            return OperationResult<WeightFileObservation>.Ok(
                new WeightFileObservation(
                    Convert.ToHexString(hash.GetHashAndReset())
                        .ToLowerInvariant(),
                    total));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<WeightFileObservation>();
        }
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static OperationResult<CheckpointProcessLease>
        AcquireProcessCheckpointSealLease(
            string canonicalCheckpointRoot,
            Stopwatch timeoutBudget)
    {
        if (!SecureLocalFileBoundary.IsSupportedPlatform())
            return Unavailable<CheckpointProcessLease>();

        CheckpointProcessGateEntry entry;
        lock (CheckpointProcessGateRegistrySync)
        {
            if (!CheckpointProcessGateRegistry.TryGetValue(
                    canonicalCheckpointRoot,
                    out entry!))
            {
                entry = new CheckpointProcessGateEntry();
                CheckpointProcessGateRegistry.Add(canonicalCheckpointRoot, entry);
            }

            checked
            {
                entry.ReferenceCount++;
            }
        }

        try
        {
            var remaining =
                DefaultCheckpointSealLockTimeout - timeoutBudget.Elapsed;
            if (remaining <= TimeSpan.Zero || !entry.Semaphore.Wait(remaining))
            {
                ReleaseCheckpointProcessGateReference(
                    canonicalCheckpointRoot,
                    entry);
                return Busy<CheckpointProcessLease>();
            }

            return OperationResult<CheckpointProcessLease>.Ok(
                new CheckpointProcessLease(canonicalCheckpointRoot, entry));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ReleaseCheckpointProcessGateReference(canonicalCheckpointRoot, entry);
            return Unavailable<CheckpointProcessLease>();
        }
    }

    private static OperationResult<CheckpointSealLease> AcquireCheckpointSealLease(
        RepositoryOwnedModelCheckpointAddress address,
        string canonicalCheckpointRoot,
        Stopwatch timeoutBudget)
    {
        if (!SecureLocalFileBoundary.IsSupportedPlatform()
            || !PathsEqual(address.CheckpointRoot, canonicalCheckpointRoot))
        {
            return Unavailable<CheckpointSealLease>();
        }

        var lockPath = CheckpointSealLockPath(canonicalCheckpointRoot);
        while (true)
        {
            if (DefaultCheckpointSealLockTimeout - timeoutBudget.Elapsed
                <= TimeSpan.Zero)
            {
                return Busy<CheckpointSealLease>();
            }

            FileStream? stream = null;
            CheckpointSealLease? lease = null;
            try
            {
                if (AnyExistingPathSegmentIsReparsePoint(canonicalCheckpointRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || (File.Exists(lockPath)
                        && !HasSafeUnixFileMode(lockPath)))
                {
                    return Unavailable<CheckpointSealLease>();
                }

                stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                    lockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileOptions.WriteThrough,
                    FileBufferSize);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        lockPath)
                    || AnyExistingPathSegmentIsReparsePoint(canonicalCheckpointRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || !HasSafeUnixFileMode(lockPath))
                {
                    stream.Dispose();
                    return Unavailable<CheckpointSealLease>();
                }

                if (stream.Length == 0)
                {
                    stream.SetLength(CheckpointSealLockLength);
                    stream.Flush(flushToDisk: true);
                }

                if (stream.Length != CheckpointSealLockLength)
                {
                    stream.Dispose();
                    return Unavailable<CheckpointSealLease>();
                }

                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    stream.Lock(
                        CheckpointSealLockOffset,
                        CheckpointSealLockLength);
                }
                else
                {
                    stream.Dispose();
                    return Unavailable<CheckpointSealLease>();
                }

                lease = new CheckpointSealLease(stream);
                if (!lease.MatchesRequestedPath(lockPath)
                    || !lease.HasExpectedLength()
                    || AnyExistingPathSegmentIsReparsePoint(canonicalCheckpointRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || !HasSafeUnixFileMode(lockPath))
                {
                    lease.Dispose();
                    return Unavailable<CheckpointSealLease>();
                }

                return OperationResult<CheckpointSealLease>.Ok(lease);
            }
            catch (IOException exception)
                when (FileDpoPairStore.IsLockContention(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                lease?.Dispose();
                stream?.Dispose();
                var remaining =
                    DefaultCheckpointSealLockTimeout - timeoutBudget.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    return Busy<CheckpointSealLease>();

                Thread.Sleep(
                    remaining < TimeSpan.FromMilliseconds(25)
                        ? remaining
                        : TimeSpan.FromMilliseconds(25));
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                lease?.Dispose();
                stream?.Dispose();
                return Unavailable<CheckpointSealLease>();
            }
        }
    }

    private static OperationResult<bool> EnsureWritableCheckpointRoot(
        RepositoryOwnedModelCheckpointAddress address)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || AnyExistingPathSegmentIsReparsePoint(address.CheckpointRoot))
            {
                return Unavailable<bool>();
            }

            if (Directory.Exists(address.CheckpointRoot))
            {
                if (!HasSafeUnixDirectoryMode(address.CheckpointRoot))
                    return Unavailable<bool>();
            }
            else if (File.Exists(address.CheckpointRoot))
            {
                return Unavailable<bool>();
            }
            else if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(address.CheckpointRoot);
            }
            else
            {
                Directory.CreateDirectory(address.CheckpointRoot, OwnerDirectoryMode);
            }

            if (AnyExistingPathSegmentIsReparsePoint(address.CheckpointRoot)
                || !HasSafeUnixDirectoryMode(address.CheckpointRoot))
            {
                return Unavailable<bool>();
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<bool>();
        }
    }

    private static string CanonicalRoot(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static string CheckpointSealLockPath(string canonicalCheckpointRoot)
        => Path.Combine(canonicalCheckpointRoot, CheckpointSealLockFileName);

    private static bool HasSafeUnixDirectoryMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(path);

    private static bool HasSafeUnixFileMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyFileMode(path);

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(fullPath);

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: HashHexLength }
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static void DeleteOwnStaging(
        RepositoryOwnedModelCheckpointAddress address,
        string temporaryPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(temporaryPath);
            var fileName = Path.GetFileName(fullPath);
            if (PathsEqual(Path.GetDirectoryName(fullPath)!, address.CheckpointRoot)
                && (fileName.StartsWith(WeightsTempPrefix, StringComparison.Ordinal)
                    || fileName.StartsWith(ManifestTempPrefix, StringComparison.Ordinal))
                && fileName.EndsWith(TempSuffix, StringComparison.Ordinal)
                && !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    fullPath)
                && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Очистка только best-effort: она не имеет права расшириться за
            // пределы нашего checkpoint directory или удалить чужие файлы.
        }
    }

    private static void DeleteTemporaryPath(string temporaryPath)
    {
        try
        {
            if (!SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    temporaryPath)
                && File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Failed temp cleanup leaves an invalid temp leaf, not a sealed file.
        }
    }

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "Repository-owned model checkpoint manifest is invalid.",
            InvalidErrorCode);

    private static OperationResult<T> Conflict<T>()
        => OperationResult<T>.Fail(
            "Repository-owned model checkpoint already seals different content.",
            ConflictErrorCode);

    private static OperationResult<T> Busy<T>()
        => OperationResult<T>.Fail(
            "Repository-owned model checkpoint storage is busy.",
            BusyErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "Repository-owned model checkpoint storage is unavailable.",
            UnavailableErrorCode);

    private sealed record WeightStagingObservation(
        string TemporaryPath,
        string Sha256,
        long ByteLength);

    private sealed record WeightFileObservation(
        string Sha256,
        long ByteLength);

    private sealed class CheckpointProcessGateEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount { get; set; }
    }

    private sealed class CheckpointProcessLease(
        string canonicalCheckpointRoot,
        CheckpointProcessGateEntry entry) : IDisposable
    {
        private CheckpointProcessGateEntry? _entry = entry;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _entry, null);
            if (owned is not null)
                ReleaseOwnedCheckpointProcessGate(canonicalCheckpointRoot, owned);
        }
    }

    private sealed class CheckpointSealLease(FileStream stream) : IDisposable
    {
        private FileStream? _stream = stream;

        public bool MatchesRequestedPath(string requestedPath)
        {
            var owned = Volatile.Read(ref _stream);
            return owned is not null
                && SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    owned.SafeFileHandle,
                    requestedPath);
        }

        public bool HasExpectedLength()
        {
            var owned = Volatile.Read(ref _stream);
            return owned is not null
                && owned.Length == CheckpointSealLockLength;
        }

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _stream, null);
            if (owned is null)
                return;
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    owned.Unlock(
                        CheckpointSealLockOffset,
                        CheckpointSealLockLength);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Closing the handle releases the byte-range lock if explicit
                // unlock cannot complete on this platform.
            }
            finally
            {
                owned.Dispose();
            }
        }
    }

    private static void ReleaseCheckpointProcessGateReference(
        string canonicalCheckpointRoot,
        CheckpointProcessGateEntry entry)
    {
        SemaphoreSlim? semaphoreToDispose = null;
        lock (CheckpointProcessGateRegistrySync)
        {
            entry.ReferenceCount--;
            if (entry.ReferenceCount != 0)
                return;
            if (!CheckpointProcessGateRegistry.TryGetValue(
                    canonicalCheckpointRoot,
                    out var registered)
                || !ReferenceEquals(registered, entry))
            {
                throw new InvalidOperationException(
                    "Model checkpoint process-gate registry identity was lost.");
            }

            CheckpointProcessGateRegistry.Remove(canonicalCheckpointRoot);
            semaphoreToDispose = entry.Semaphore;
        }

        semaphoreToDispose.Dispose();
    }

    private static void ReleaseOwnedCheckpointProcessGate(
        string canonicalCheckpointRoot,
        CheckpointProcessGateEntry entry)
    {
        try
        {
            entry.Semaphore.Release();
        }
        finally
        {
            ReleaseCheckpointProcessGateReference(
                canonicalCheckpointRoot,
                entry);
        }
    }
}
