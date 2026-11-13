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

/// <summary>One sealed pair file entry in a repository-owned DPO dataset manifest.</summary>
public sealed record RepositoryOwnedDpoDatasetManifestPair(
    string PairId,
    string RepositoryRelativePairFilePath,
    string PairFileSha256);

/// <summary>
/// Hash-only view of paid reference answers. The paid answers themselves must
/// never be persisted in this repository-owned DPO identity.
/// </summary>
public sealed record RepositoryOwnedDpoPaidReferenceAnswerSet(
    int AnswerCount,
    IReadOnlyList<string> OrderedAnswerSha256,
    string CanonicalListSha256);

/// <summary>Stable identity of the open teacher model that produced DPO sides.</summary>
public sealed record RepositoryOwnedDpoOpenTeacherIdentity(
    string Family,
    string ModelId,
    string ModelVersion,
    string ModelArtifactDigestKind,
    string ModelArtifactSha256);

/// <summary>Stable identity of the pair generation policy.</summary>
public sealed record RepositoryOwnedDpoGenerationPolicyIdentity(
    string StableName,
    string Version,
    string ConfigSha256);

/// <summary>Stable identity of the component that created the pair rows.</summary>
public sealed record RepositoryOwnedDpoCreatorIdentity(
    string StableName,
    string Version,
    string EffectiveConfigSha256);

/// <summary>
/// Caller-supplied identity of the request that produced one sealed DPO dataset.
/// Domain, design pattern and dataset version remain the address, not identity
/// fields; this object binds the expensive teacher/reference/config inputs.
/// </summary>
public sealed record RepositoryOwnedDpoDatasetRequestIdentity(
    string Schema,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] RepositoryOwnedDpoPaidReferenceAnswerSet? PaidReferenceAnswers,
    RepositoryOwnedDpoOpenTeacherIdentity OpenTeacher,
    RepositoryOwnedDpoGenerationPolicyIdentity GenerationPolicy,
    RepositoryOwnedDpoCreatorIdentity Creator,
    string RequestIdentitySha256,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] RepositoryOwnedDpoRawSourceIdentity? RawSource = null);

/// <summary>
/// Immutable manifest for one repository-owned DPO dataset address.
/// </summary>
public sealed record RepositoryOwnedDpoDatasetManifest(
    string Schema,
    string Context,
    string Domain,
    string DesignPattern,
    string DatasetVersion,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    int PairCount,
    IReadOnlyList<RepositoryOwnedDpoDatasetManifestPair> OrderedPairs,
    string ManifestSha256);

/// <summary>Freshly reopened sealed repository-owned DPO dataset.</summary>
public sealed record RepositoryOwnedDpoDatasetOpenResult(
    RepositoryOwnedDpoDatasetManifest Manifest,
    IReadOnlyList<DpoPair> Pairs);

/// <summary>
/// Reopened manifest identity without proving the referenced pair-file bytes.
/// </summary>
public sealed record RepositoryOwnedDpoDatasetManifestIdentityOpenResult(
    RepositoryOwnedDpoDatasetManifest Manifest,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity);

/// <summary>
/// Describes the validated manifest snapshot and exact file bytes read by its
/// owner. Manifest-only readers do not prove pair bytes. The reservation-only
/// reader separately attributes raw pair file hashes, without training authority
/// or a retained file lease after return.
/// </summary>
internal sealed record RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult(
    RepositoryOwnedDpoDatasetManifest Manifest,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    string ManifestFileSha256,
    long ManifestFileByteLength);

/// <summary>Full fresh owner proof and raw identity from its same parsed buffer.</summary>
internal sealed record RepositoryOwnedDpoDatasetFileIdentityOpenResult(
    RepositoryOwnedDpoDatasetManifest Manifest,
    IReadOnlyList<DpoPair> Pairs,
    string ManifestFileSha256,
    long ManifestFileByteLength);

/// <summary>
/// Persists already-admitted rich DPO pairs into their canonical repository-owned
/// pair store and seals the exact files with a sibling manifest.
/// </summary>
public static partial class RepositoryOwnedDpoDatasetManifestSealer
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_MANIFEST_INVALID";
    public const string ConflictErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_MANIFEST_CONFLICT";
    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_MANIFEST_UNAVAILABLE";
    public const string RequestIdentityMismatchErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_REQUEST_IDENTITY_MISMATCH";

    public const string CurrentSchema =
        "repository-owned-dpo-dataset-manifest-v2";
    public const string CurrentContext =
        "continuous-learning-dpo-rich-pairs-v1";
    public const string CurrentRequestIdentitySchema =
        "repository-owned-dpo-dataset-request-identity-v1";

    private const string PairFileSuffix = ".dpo-pair.json";
    private const string ManifestTempPrefix = ".manifest.";
    private const string ManifestTempSuffix = ".tmp";
    private const string DatasetSealLockFileName = ".dpo-dataset-seal.lock";
    private const int RichPairRecordSchemaVersion = 5;
    private const int ManifestJsonMaxDepth = 10;
    private const int HashHexLength = 64;
    private const int MaximumIdentityTextLength = 256;
    private const int MaximumManifestPairCount =
        FileDpoPairStore.DefaultMaximumRecordCount;
    private const int FileBufferSize = 4096;
    private const long MaximumManifestBytes = 8L * 1024L * 1024L;
    private const long MaximumPairFileBytes = 16L * 1024L * 1024L;
    private const long DatasetSealLockOffset = 0;
    private const long DatasetSealLockLength = 1;
    private static readonly TimeSpan DefaultDatasetSealLockTimeout =
        TimeSpan.FromSeconds(10);
    private static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly object DatasetProcessGateRegistrySync = new();
    private static readonly Dictionary<string, DatasetProcessGateEntry>
        DatasetProcessGateRegistry =
            new(OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);
    private static readonly AsyncLocal<Action?>
        AfterDatasetLeaseAcquiredOverride = new();

    private const string SchemaProperty = "schema";
    private const string ContextProperty = "context";
    private const string DomainProperty = "domain";
    private const string DesignPatternProperty = "designPattern";
    private const string DatasetVersionProperty = "datasetVersion";
    private const string RequestIdentityProperty = "requestIdentity";
    private const string PairCountProperty = "pairCount";
    private const string OrderedPairsProperty = "orderedPairs";
    private const string ManifestSha256Property = "manifestSha256";
    private const string PairIdProperty = "pairId";
    private const string RepositoryRelativePairFilePathProperty =
        "repositoryRelativePairFilePath";
    private const string PairFileSha256Property = "pairFileSha256";
    private const string PaidReferenceAnswersProperty = "paidReferenceAnswers";
    private const string AnswerCountProperty = "answerCount";
    private const string OrderedAnswerSha256Property = "orderedAnswerSha256";
    private const string CanonicalListSha256Property = "canonicalListSha256";
    private const string OpenTeacherProperty = "openTeacher";
    private const string FamilyProperty = "family";
    private const string ModelIdProperty = "modelId";
    private const string ModelVersionProperty = "modelVersion";
    private const string ModelArtifactDigestKindProperty =
        "modelArtifactDigestKind";
    private const string ModelArtifactSha256Property = "modelArtifactSha256";
    private const string GenerationPolicyProperty = "generationPolicy";
    private const string CreatorProperty = "creator";
    private const string StableNameProperty = "stableName";
    private const string VersionProperty = "version";
    private const string ConfigSha256Property = "configSha256";
    private const string EffectiveConfigSha256Property =
        "effectiveConfigSha256";
    private const string RequestIdentitySha256Property =
        "requestIdentitySha256";

    public const string BusyErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_MANIFEST_BUSY";

    internal static Action? AfterDatasetLeaseAcquiredForTesting
    {
        get => AfterDatasetLeaseAcquiredOverride.Value;
        set => AfterDatasetLeaseAcquiredOverride.Value = value;
    }

    public static OperationResult<RepositoryOwnedDpoDatasetRequestIdentity>
        CreateRequestIdentity(
            IReadOnlyList<string> orderedPaidReferenceAnswerSha256,
            RepositoryOwnedDpoOpenTeacherIdentity openTeacher,
            RepositoryOwnedDpoGenerationPolicyIdentity generationPolicy,
            RepositoryOwnedDpoCreatorIdentity creator)
    {
        try
        {
            var paidReferences = CreatePaidReferenceAnswerSet(
                orderedPaidReferenceAnswerSha256);
            if (!paidReferences.IsSuccess || paidReferences.Value is null)
                return paidReferences.MapError<
                    RepositoryOwnedDpoDatasetRequestIdentity>();

            var unsigned = new RepositoryOwnedDpoDatasetRequestIdentity(
                CurrentRequestIdentitySchema,
                paidReferences.Value,
                openTeacher,
                generationPolicy,
                creator,
                string.Empty);
            var identity = unsigned with
            {
                RequestIdentitySha256 = ComputeSha256(
                    WriteCanonicalRequestIdentityBytes(unsigned)),
            };
            var validation = ValidateRequestIdentity(identity);
            return validation.IsSuccess
                ? OperationResult<RepositoryOwnedDpoDatasetRequestIdentity>.Ok(
                    identity)
                : validation.MapError<RepositoryOwnedDpoDatasetRequestIdentity>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedDpoDatasetRequestIdentity>();
        }
    }

    public static bool IsValidRequestIdentity(
        RepositoryOwnedDpoDatasetRequestIdentity? identity)
        => ValidateRequestIdentity(identity).IsSuccess;

    public static OperationResult<RepositoryOwnedDpoDatasetManifest>
        PersistAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
            IReadOnlyList<DpoPair> suppliedPairs)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetManifest>();

        var identityValidation = ValidateRequestIdentity(requestIdentity);
        if (!identityValidation.IsSuccess)
            return identityValidation.MapError<
                RepositoryOwnedDpoDatasetManifest>();

        var pairs = ValidateSuppliedPairs(address, requestIdentity, suppliedPairs);
        if (!pairs.IsSuccess || pairs.Value is null)
            return pairs.MapError<RepositoryOwnedDpoDatasetManifest>();

        try
        {
            var canonicalDatasetRoot = CanonicalRoot(address.DatasetRoot);
            var timeoutBudget = Stopwatch.StartNew();
            var processLeaseResult = AcquireProcessDatasetSealLease(
                canonicalDatasetRoot,
                timeoutBudget);
            if (!processLeaseResult.IsSuccess || processLeaseResult.Value is null)
                return processLeaseResult.MapError<RepositoryOwnedDpoDatasetManifest>();

            using var processLease = processLeaseResult.Value;
            var datasetRootAlreadyExists = Directory.Exists(address.DatasetRoot);
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext;
            // WHY: существующий dataset root может приехать из clone как 0755,
            // поэтому сначала доказываем DPO custody, а новый root нужно сперва
            // создать, чтобы preparer сузил все свежие промежуточные сегменты.
            // File lease и pairs не пишутся до успешного opaque PreparedContext.
            if (datasetRootAlreadyExists)
            {
                var prepared = PrepareDpoCustodyContextForManifestOperation(
                    address);
                if (!prepared.IsSuccess || prepared.Value is null)
                    return prepared.MapError<RepositoryOwnedDpoDatasetManifest>();
                custodyContext = prepared.Value;

                var lockRootReady = EnsureWritableDatasetRoot(address);
                if (!lockRootReady.IsSuccess)
                    return lockRootReady.MapError<RepositoryOwnedDpoDatasetManifest>();
            }
            else
            {
                var lockRootReady = EnsureWritableDatasetRoot(address);
                if (!lockRootReady.IsSuccess)
                    return lockRootReady.MapError<RepositoryOwnedDpoDatasetManifest>();

                var prepared = PrepareDpoCustodyContextForManifestOperation(
                    address);
                if (!prepared.IsSuccess || prepared.Value is null)
                    return prepared.MapError<RepositoryOwnedDpoDatasetManifest>();
                custodyContext = prepared.Value;
            }

            var leaseResult = AcquireDatasetSealLease(
                address,
                canonicalDatasetRoot,
                timeoutBudget);
            if (!leaseResult.IsSuccess || leaseResult.Value is null)
                return leaseResult.MapError<RepositoryOwnedDpoDatasetManifest>();

            using var lease = leaseResult.Value;
            var revalidation = ValidateAddress(address);
            var rootReady = EnsureWritableDatasetRoot(address);
            if (!revalidation.IsSuccess
                || !rootReady.IsSuccess
                || !PathsEqual(address.DatasetRoot, canonicalDatasetRoot)
                || !lease.MatchesRequestedPath(
                    DatasetSealLockPath(canonicalDatasetRoot))
                || !lease.HasExpectedLength()
                || AnyExistingPathSegmentIsReparsePoint(address.DatasetRoot)
                || AnyExistingPathSegmentIsReparsePoint(
                    DatasetSealLockPath(canonicalDatasetRoot))
                || !HasSafeUnixDirectoryMode(address.DatasetRoot)
                || !HasSafeUnixFileMode(DatasetSealLockPath(canonicalDatasetRoot)))
            {
                if (!revalidation.IsSuccess)
                    return revalidation.MapError<RepositoryOwnedDpoDatasetManifest>();
                if (!rootReady.IsSuccess)
                    return rootReady.MapError<RepositoryOwnedDpoDatasetManifest>();
                return Unavailable<RepositoryOwnedDpoDatasetManifest>();
            }

            AfterDatasetLeaseAcquiredForTesting?.Invoke();
            return PersistAndSealUnderLease(
                address,
                requestIdentity,
                pairs.Value,
                custodyContext);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedDpoDatasetManifest>();
        }
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifest>
        PersistAndSealUnderLease(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
            IReadOnlyList<DpoPair> pairs,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext)
    {
        if (File.Exists(address.ManifestPath))
        {
            var existing = ResolveExistingManifestForSuppliedPairs(
                address,
                requestIdentity,
                pairs,
                custodyContext);
            return existing;
        }

        // WHY: публичный PersistAndSeal уже сузил canonical DPO custody один
        // раз. Запись pair-store должна использовать этот opaque context, а не
        // rewalk/chmod того же `training-data/dpo` на каждом внутреннем шаге.
        var store = ContinuousLearningDpoPairStores
            .CreateFileStoreWithPreparedRepositoryOwnedDpoCustody(
                address,
                custodyContext);
        var ids = new List<string>(pairs.Count);
        var idsSeen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in pairs)
        {
            var added = store.Add(pair);
            if (!added.IsSuccess || string.IsNullOrWhiteSpace(added.Value))
                return added.MapError<RepositoryOwnedDpoDatasetManifest>();
            if (!IsCanonicalSha256Hex(added.Value)
                || !idsSeen.Add(added.Value))
            {
                return Invalid<RepositoryOwnedDpoDatasetManifest>();
            }

            ids.Add(added.Value);
        }

        var reopened = ReadFreshStorePairs(address, custodyContext);
        if (!reopened.IsSuccess || reopened.Value is null)
            return reopened.MapError<RepositoryOwnedDpoDatasetManifest>();
        if (!FreshReaderMatchesSuppliedPairs(
                ids,
                pairs,
                reopened.Value))
        {
            return Invalid<RepositoryOwnedDpoDatasetManifest>();
        }

        var fileSet = InspectPairFiles(address, ids);
        if (!fileSet.IsSuccess || fileSet.Value is null)
            return fileSet.MapError<RepositoryOwnedDpoDatasetManifest>();

        var manifest = BuildManifest(address, requestIdentity, fileSet.Value);
        var canonicalBytes = WriteCanonicalManifestBytes(manifest);
        var temporaryPath = Path.Combine(
            address.DatasetRoot,
            ManifestTempPrefix + Guid.NewGuid().ToString("N") +
            ManifestTempSuffix);
        var published = false;
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
                stream.Write(canonicalBytes);
                stream.Flush(flushToDisk: true);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporaryPath))
                {
                    return Unavailable<RepositoryOwnedDpoDatasetManifest>();
                }
            }

            File.Move(temporaryPath, address.ManifestPath);
            published = true;
        }
        catch (IOException) when (File.Exists(address.ManifestPath))
        {
            DeleteOwnManifestTemp(address, temporaryPath);
            var equivalent = ExistingManifestIsEquivalent(
                address.ManifestPath,
                canonicalBytes,
                manifest);
            if (!equivalent.IsSuccess || !equivalent.Value)
                return equivalent.IsSuccess
                    ? Conflict<RepositoryOwnedDpoDatasetManifest>()
                    : equivalent.MapError<RepositoryOwnedDpoDatasetManifest>();
        }
        finally
        {
            if (!published)
                DeleteOwnManifestTemp(address, temporaryPath);
        }

        var opened = OpenSealedCore(
            address,
            requestIdentity,
            requireExpectedIdentity: true,
            readForManifestIntegrity: false,
            custodyContext);
        return opened.IsSuccess && opened.Value is not null
            ? OperationResult<RepositoryOwnedDpoDatasetManifest>.Ok(
                opened.Value.Manifest)
            : opened.MapError<RepositoryOwnedDpoDatasetManifest>();
    }

    public static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealed(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetOpenResult>();

        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
        return custodyContext.IsSuccess && custodyContext.Value is not null
            ? OpenSealedCore(
                address,
                expectedIdentity,
                requireExpectedIdentity: true,
                readForManifestIntegrity: false,
                custodyContext.Value)
            : Unavailable<RepositoryOwnedDpoDatasetOpenResult>();
    }

    /// <summary>
    /// Reopens the full saved dataset through its existing custody owner, then
    /// validates the persisted licensing/quarantine/evidence decision. This
    /// does not issue a training lease or certify independent source rights.
    /// </summary>
    public static OperationResult<RepositoryOwnedDpoDatasetOpenResult> // Возвращаем открытый набор или типизированную ошибку существующего владельца данных.
        OpenSealedTrainingEligible( // Дополняем полное открытие проверкой сохранённого решения о допуске материала.
            RepositoryOwnedDpoDatasetAddress address, // Используем закреплённый адрес сохранённого набора.
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity) // Требуем совпадения с выбранной идентичностью запроса.
    { // Сохраняем порядок: полное открытие, затем проверка допуска.
        var opened = OpenSealed(address, expectedIdentity); // Повторно читаем набор через существующие проверки целостности и custody.
        if (!opened.IsSuccess || opened.Value is null) // Останавливаемся, если набор не удалось корректно открыть.
            return opened; // Сохраняем исходную ошибку открытия без обходного допуска.
        var eligible = TrainingDataGovernanceAdmission.Validate(opened.Value.Pairs); // Проверяем сохранённые решения о лицензии, карантине и evidence для пар.
        return eligible.IsSuccess // Возвращаем данные только после успешной проверки сохранённого допуска.
            ? opened // Сохраняем полностью проверенный результат открытия.
            : eligible.MapError<RepositoryOwnedDpoDatasetOpenResult>(); // Передаём отказ допуска в типе результата открытия.
    } // Проверка материала не выдаёт разрешение на запуск обучения.

    private static OperationResult<
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext>
        PrepareDpoCustodyContextForManifestOperation(
            RepositoryOwnedDpoDatasetAddress address)
    {
        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
        if (!custodyContext.IsSuccess
            || custodyContext.Value is null
            || !custodyContext.Value.CoversDpoRoot(address.RepositoryRoot))
        {
            return Unavailable<
                RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                    .PreparedContext>();
        }

        return OperationResult<
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext>
            .Ok(custodyContext.Value);
    }

    internal static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealedForManifestIntegrity(
            RepositoryOwnedDpoDatasetAddress address)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetOpenResult>();

        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
        return custodyContext.IsSuccess && custodyContext.Value is not null
            ? OpenSealedForManifestIntegrity(address, custodyContext.Value)
            : Unavailable<RepositoryOwnedDpoDatasetOpenResult>();
    }

    internal static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealedForManifestIntegrity(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext)
        => OpenSealedCore(
            address,
            expectedIdentity: null,
            requireExpectedIdentity: false,
            readForManifestIntegrity: true,
            custodyContext);

    /// <summary>Read-only raw reservation attribution; never re-admits or hydrates training pairs.</summary>
    internal static OperationResult<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult> OpenReservationManifestReadOnly( // Reuse the manifest owner rather than the mutating pair-store integrity route.
        RepositoryOwnedDpoDatasetAddress address, CancellationToken cancellationToken = default) // Only a canonical layout address selects storage.
    { // No clone preparation, chmod, lock creation, pair-store mutation or model authority enters this projection.
        try // Missing, changed or unsafe immutable evidence refuses attribution.
        { // Validate canonical scope and existing owner modes before opening its manifest.
            cancellationToken.ThrowIfCancellationRequested(); // Cancellation never admits an unchecked reservation.
            if (!ValidateAddress(address).IsSuccess || !ReservationPathsHaveOwnerModes(address, [])) // Reject noncanonical/reparse/unsafe existing scope without repairing it.
                return Unavailable<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // Preserve a failed observation rather than inventing fresh state.
            var opened = ReadStrictManifestFileIdentity(address.ManifestPath); // Use the existing strict parser, canonical selfhash and exact stable byte identity.
            if (!opened.IsSuccess || opened.Value is null) return opened; // Preserve the actual manifest refusal.
            var manifest = opened.Value.Manifest; // This detached snapshot is expectation metadata, not training admission.
            if (!ValidateManifestForAddress(address, manifest).IsSuccess) // Bind every row/path and manifest domain/pattern/version to the canonical address.
                return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // A valid selfhash alone does not establish attribution.
            var ids = manifest.OrderedPairs.Select(static pair => pair.PairId).ToArray(); // Derive file names only from canonical validated row identities.
            cancellationToken.ThrowIfCancellationRequested(); // Keep each bounded observation under the current cancellation.
            if (!ReservationPathsHaveOwnerModes(address, ids)) return Unavailable<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // Check all pair-file permissions before their byte reads.
            var files = InspectPairFiles(address, ids); // Reuse exact bounded file-set/canonical-path/stable-raw-hash checks without reading a DpoPair store.
            if (!files.IsSuccess || files.Value is null) return files.MapError<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // A missing or changed file cannot attribute a reservation.
            foreach (var row in manifest.OrderedPairs) // Compare every validated immutable manifest row to its observed raw file.
            { // PairId is the manifest's reservation identity; this API does not recompute semantic pair identity or grant training.
                if (!files.Value.TryGetValue(row.PairId, out var file) || file.RepositoryRelativePairFilePath != row.RepositoryRelativePairFilePath || // Require the exact canonical path observed by the existing owner.
                    file.PairFileSha256 != row.PairFileSha256) return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // Require actual raw bytes to match the sealed row hash.
            } // The projection intentionally contains no hydrated DpoPair or optimizer authority.
            cancellationToken.ThrowIfCancellationRequested(); // Refuse cancellation after bounded pair observation too.
            var after = ReadStrictManifestFileIdentity(address.ManifestPath); // Reopen the manifest through the same stable reader after pair observation.
            if (!after.IsSuccess || after.Value is null || after.Value.ManifestFileSha256 != opened.Value.ManifestFileSha256 || // The complete raw manifest must remain the same snapshot.
                after.Value.ManifestFileByteLength != opened.Value.ManifestFileByteLength || !ReservationPathsHaveOwnerModes(address, ids) || // Recheck every existing scope/file mode after observation.
                !ValidateAddress(address).IsSuccess) return Unavailable<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); // Refuse changed identity/custody rather than normalize it.
            cancellationToken.ThrowIfCancellationRequested(); // Return only a completed uncancelled observation.
            return opened; // Reuse the existing immutable manifest-file/OrderedPairs projection with narrowly documented raw attribution.
        } // Preserve cancellation while classifying nonfatal parser/storage failures.
        catch (OperationCanceledException) { throw; } // Cancellation is not a reservation proof.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Unavailable<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>(); } // Unsafe or partial evidence remains unknown to the caller.
    } // Ordinary fresh/full pair APIs and their custody preparation remain unchanged.

    private static bool ReservationPathsHaveOwnerModes(RepositoryOwnedDpoDatasetAddress address, IReadOnlyList<string> pairIds) // Verify existing scope without changing directories, modes or files.
    { // On Windows these established owner-mode predicates intentionally preserve the stable-path/handle/reparse contract, not an added ACL claim.
        if (!SecureLocalFileBoundary.IsSupportedPlatform()) return false; // Unsupported host evidence cannot be attributed.
        var root = address.DpoArtifactRoot; // Canonical layout validation determines the DPO subtree boundary.
        for (var directory = address.PairsRoot; ; directory = Path.GetDirectoryName(directory)!) // Inspect every directory segment from pair storage through the DPO root.
        { // Canonical addresses make this a bounded parent walk within the existing DPO subtree.
            if (string.IsNullOrEmpty(directory) || (!PathsEqual(directory, root) && !directory.StartsWith(root + Path.DirectorySeparatorChar, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) || // Refuse any unexpected escape from the canonical subtree using the existing platform path comparison.
                !Directory.Exists(directory) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory) || // Existing unsafe/missing directories are never created or repaired here.
                !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(directory)) return false; // Linux group/other access refuses read-only attribution.
            if (PathsEqual(directory, root)) break; // The complete canonical DPO directory chain has been checked.
        } // No path from untrusted saved reservation JSON is traversed.
        foreach (var path in pairIds.Select(id => Path.Combine(address.PairsRoot, id + PairFileSuffix)).Prepend(address.ManifestPath)) // Include the manifest and every validated canonical pair file.
        { // The caller checks this complete set before and after observation.
            if (!File.Exists(path) || Directory.Exists(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path) || // Reject redirected, missing or mistyped files.
                !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path)) return false; // Preserve existing owner modes without chmod or normalization.
        } // The bounded readers independently verify each opened handle and stable raw bytes.
        return true; // This is a read-only permission/path check, never an issuer or training grant.
    } // Strict Windows custody remains a separate existing contract on the new reservation lock.

    public static OperationResult<RepositoryOwnedDpoDatasetManifestIdentityOpenResult>
        OpenManifestIdentityOnly(
            RepositoryOwnedDpoDatasetAddress address)
    {
        // WHY: retain the public projection while both entry points share the
        // same custody, strict parser and address validation.
        var readback = OpenManifestFileIdentityOnly(address);
        return readback.IsSuccess && readback.Value is not null
            ? OperationResult<RepositoryOwnedDpoDatasetManifestIdentityOpenResult>
                .Ok(new RepositoryOwnedDpoDatasetManifestIdentityOpenResult(
                    readback.Value.Manifest,
                    readback.Value.RequestIdentity))
            : readback.MapError<RepositoryOwnedDpoDatasetManifestIdentityOpenResult>();
    }

    internal static OperationResult<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>
        OpenManifestFileIdentityOnly(
            RepositoryOwnedDpoDatasetAddress address)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
        {
            return validation
                .MapError<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
        }

        try
        {
            // WHY: identity-only DPO reads do not need model checkpoints, so
            // their public safety boundary prepares only the DPO subtree. Model
            // routes prepare both roots once before calling the strict bypasses.
            var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
            if (!custodyContext.IsSuccess
                || custodyContext.Value is null
                || !custodyContext.Value.CoversDpoRoot(address.RepositoryRoot))
            {
                return Unavailable<
                    RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
            }

            if (!File.Exists(address.ManifestPath))
            {
                return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
            }

            var manifest = ReadStrictManifestFileIdentity(address.ManifestPath);
            if (!manifest.IsSuccess || manifest.Value is null)
            {
                return manifest
                    .MapError<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
            }

            var manifestValidation = ValidateManifestForAddress(
                address,
                manifest.Value.Manifest);
            if (!manifestValidation.IsSuccess)
            {
                return manifestValidation
                    .MapError<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
            }

            // WHY: callers that only need the immutable request identity must
            // not go through the pair store, because pair-store readers take a
            // mutation lease and may create lock files. Full pair-byte
            // integrity remains the responsibility of OpenSealed/resolver.
            return manifest;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
        }
    }

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenSealedCore(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity? expectedIdentity,
            bool requireExpectedIdentity,
            bool readForManifestIntegrity,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext)
    {
        var opened = OpenSealedFileIdentityCore(address, expectedIdentity,
            requireExpectedIdentity, readForManifestIntegrity, custodyContext);
        return opened.IsSuccess && opened.Value is not null
            ? OperationResult<RepositoryOwnedDpoDatasetOpenResult>.Ok(
                new(opened.Value.Manifest, opened.Value.Pairs))
            : opened.MapError<RepositoryOwnedDpoDatasetOpenResult>();
    }

    internal static OperationResult<RepositoryOwnedDpoDatasetFileIdentityOpenResult>
        OpenSealedFileIdentity(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
        var custody = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
        return custody.IsSuccess && custody.Value is not null
            ? OpenSealedFileIdentityCore(address, expectedIdentity,
                requireExpectedIdentity: true, readForManifestIntegrity: false, custody.Value)
            : Unavailable<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
    }

    private static OperationResult<RepositoryOwnedDpoDatasetFileIdentityOpenResult>
        OpenSealedFileIdentityCore(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity? expectedIdentity,
            bool requireExpectedIdentity,
            bool readForManifestIntegrity,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext custodyContext)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

        try
        {
            // WHY: this core is used by recursive model lineage checks. It
            // accepts only an opaque context that was prepared for the same
            // repository root, so nested opens can reuse custody proof without
            // rewalking the clone tree.
            if (!custodyContext.CoversDpoRoot(address.RepositoryRoot))
                return Unavailable<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            if (!File.Exists(address.ManifestPath))
                return Invalid<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            var fileIdentity = ReadStrictManifestFileIdentity(address.ManifestPath);
            if (!fileIdentity.IsSuccess || fileIdentity.Value is null)
                return fileIdentity.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
            var manifest = fileIdentity.Value.Manifest;

            var manifestValidation = ValidateManifestForAddress(
                address,
                manifest);
            if (!manifestValidation.IsSuccess)
                return manifestValidation.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            if (requireExpectedIdentity)
            {
                var identityValidation = ValidateRequestIdentity(expectedIdentity);
                if (!identityValidation.IsSuccess)
                    return identityValidation.MapError<
                        RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

                if (!RequestIdentitiesEqual(
                        manifest.RequestIdentity,
                        expectedIdentity!))
                {
                    return IdentityMismatch<
                        RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
                }
            }

            var expectedIds = manifest.OrderedPairs
                .Select(static pair => pair.PairId)
                .ToArray();
            var fileSet = InspectPairFiles(address, expectedIds);
            if (!fileSet.IsSuccess || fileSet.Value is null)
                return fileSet.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            foreach (var entry in manifest.OrderedPairs)
            {
                if (!fileSet.Value.TryGetValue(entry.PairId, out var observed)
                    || !StringComparer.Ordinal.Equals(
                        observed.RepositoryRelativePairFilePath,
                        entry.RepositoryRelativePairFilePath)
                    || !StringComparer.Ordinal.Equals(
                        observed.PairFileSha256,
                        entry.PairFileSha256))
                {
                    return Invalid<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
                }
            }

            // WHY: public store reads answer "which pairs are available to a
            // new consumer?" and intentionally hide transaction/replay-held
            // pairs. Checkpoint-manifest validation asks a different question:
            // "does this immutable sealed manifest still name the exact stored
            // rows and file hashes?", so it must include held current pairs.
            var reopened = readForManifestIntegrity
                ? ReadManifestIntegrityStorePairs(address, expectedIds)
                : ReadFreshStorePairs(address, custodyContext);
            if (!reopened.IsSuccess || reopened.Value is null)
                return reopened.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            var orderedPairs = MapReopenedPairsByManifestOrder(
                manifest,
                reopened.Value);
            if (!orderedPairs.IsSuccess || orderedPairs.Value is null)
                return orderedPairs.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            var pairIdentity = ValidatePairsAgainstRequestIdentity(
                manifest.RequestIdentity,
                orderedPairs.Value);
            if (!pairIdentity.IsSuccess)
                return pairIdentity.MapError<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();

            // WHY: raw metadata is projected only after every full owner check,
            // from the identical buffer already parsed above, without another open.
            return OperationResult<RepositoryOwnedDpoDatasetFileIdentityOpenResult>.Ok(
                new RepositoryOwnedDpoDatasetFileIdentityOpenResult(
                    manifest,
                    orderedPairs.Value,
                    fileIdentity.Value.ManifestFileSha256,
                    fileIdentity.Value.ManifestFileByteLength));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<RepositoryOwnedDpoDatasetFileIdentityOpenResult>();
        }
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifest>
        ResolveExistingManifestForSuppliedPairs(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
            IReadOnlyList<DpoPair> suppliedPairs,
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
                custodyContext)
    {
        // WHY: existing-manifest внутри PersistAndSeal остаётся частью одной
        // публичной операции. Повторный public OpenSealed заново прошёл бы DPO
        // custody, хотя lease-holder уже держит проверенный PreparedContext.
        var opened = OpenSealedCore(
            address,
            requestIdentity,
            requireExpectedIdentity: true,
            readForManifestIntegrity: false,
            custodyContext);
        if (!opened.IsSuccess || opened.Value is null)
            return opened.MapError<RepositoryOwnedDpoDatasetManifest>();

        return OpenedDatasetMatchesSuppliedPairs(opened.Value, suppliedPairs)
            ? OperationResult<RepositoryOwnedDpoDatasetManifest>.Ok(
                opened.Value.Manifest)
            : Conflict<RepositoryOwnedDpoDatasetManifest>();
    }

    private static bool OpenedDatasetMatchesSuppliedPairs(
        RepositoryOwnedDpoDatasetOpenResult opened,
        IReadOnlyList<DpoPair> suppliedPairs)
    {
        if (suppliedPairs.Count != opened.Manifest.PairCount
            || opened.Pairs.Count != opened.Manifest.PairCount
            || opened.Manifest.OrderedPairs.Count != opened.Manifest.PairCount)
        {
            return false;
        }

        var suppliedById = new Dictionary<string, DpoPair>(StringComparer.Ordinal);
        foreach (var pair in suppliedPairs)
        {
            var pairId = FileDpoPairStore.ComputeIdentity(
                pair,
                RichPairRecordSchemaVersion);
            if (!IsCanonicalSha256Hex(pairId)
                || !suppliedById.TryAdd(pairId, pair))
            {
                return false;
            }
        }

        for (var index = 0; index < opened.Manifest.OrderedPairs.Count; index++)
        {
            var openedPair = opened.Pairs[index];
            var openedPairId = FileDpoPairStore.ComputeIdentity(
                openedPair,
                RichPairRecordSchemaVersion);
            if (!StringComparer.Ordinal.Equals(
                    openedPairId,
                    opened.Manifest.OrderedPairs[index].PairId)
                || !suppliedById.TryGetValue(openedPairId, out var suppliedPair)
                || suppliedPair != openedPair)
            {
                return false;
            }
        }

        return true;
    }

    private static OperationResult<bool> ValidateAddress(
        RepositoryOwnedDpoDatasetAddress? address)
    {
        if (address is null)
            return Invalid<bool>();
        try
        {
            return RepositoryOwnedTrainingArtifactLayout
                .IsValidDpoDatasetAddress(address)
                ? OperationResult<bool>.Ok(true)
                : Invalid<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<bool>();
        }
    }

    private static OperationResult<RepositoryOwnedDpoPaidReferenceAnswerSet>
        CreatePaidReferenceAnswerSet(
            IReadOnlyList<string>? orderedPaidReferenceAnswerSha256)
    {
        if (orderedPaidReferenceAnswerSha256 is null
            || orderedPaidReferenceAnswerSha256.Count == 0
            || orderedPaidReferenceAnswerSha256.Count > MaximumManifestPairCount
            || orderedPaidReferenceAnswerSha256
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedPaidReferenceAnswerSha256.Count
            || orderedPaidReferenceAnswerSha256.Any(hash =>
                !IsCanonicalSha256Hex(hash)))
        {
            return Invalid<RepositoryOwnedDpoPaidReferenceAnswerSet>();
        }

        var snapshot = Array.AsReadOnly(
            orderedPaidReferenceAnswerSha256.ToArray());
        return OperationResult<RepositoryOwnedDpoPaidReferenceAnswerSet>.Ok(
            new RepositoryOwnedDpoPaidReferenceAnswerSet(
                snapshot.Count,
                snapshot,
                ComputeSha256(WriteCanonicalPaidReferenceAnswerListBytes(
                    snapshot))));
    }

    private static OperationResult<bool> ValidateRequestIdentity(
        RepositoryOwnedDpoDatasetRequestIdentity? identity)
    {
        if (identity is null
            || !ValidateRequestSource(identity)
            || !ValidateOpenTeacherIdentity(identity.OpenTeacher)
            || !ValidateGenerationPolicy(identity.GenerationPolicy)
            || !ValidateCreatorIdentity(identity.Creator)
            || !IsCanonicalSha256Hex(identity.RequestIdentitySha256))
        {
            return Invalid<bool>();
        }

        var expected = ComputeSha256(WriteCanonicalRequestIdentityBytes(
            identity with
            {
                RequestIdentitySha256 = string.Empty,
            }));
        return StringComparer.Ordinal.Equals(
                expected,
                identity.RequestIdentitySha256)
            ? OperationResult<bool>.Ok(true)
            : Invalid<bool>();
    }

    private static bool ValidatePaidReferenceAnswerSet(
        RepositoryOwnedDpoPaidReferenceAnswerSet? paidReference)
        => paidReference is not null
           && paidReference.AnswerCount > 0
           && paidReference.AnswerCount <= MaximumManifestPairCount
           && paidReference.OrderedAnswerSha256 is not null
           && paidReference.OrderedAnswerSha256.Count == paidReference.AnswerCount
           && paidReference.OrderedAnswerSha256
               .Distinct(StringComparer.Ordinal)
               .Count() == paidReference.OrderedAnswerSha256.Count
           && paidReference.OrderedAnswerSha256.All(IsCanonicalSha256Hex)
           && IsCanonicalSha256Hex(paidReference.CanonicalListSha256)
           && StringComparer.Ordinal.Equals(
               ComputeSha256(WriteCanonicalPaidReferenceAnswerListBytes(
                   paidReference.OrderedAnswerSha256)),
               paidReference.CanonicalListSha256);

    private static bool ValidateOpenTeacherIdentity(
        RepositoryOwnedDpoOpenTeacherIdentity? teacher)
        => teacher is not null
           && IsSafeIdentityText(teacher.Family)
           && Enum.TryParse<OpenWeightTeacherFamily>(
               teacher.Family,
               ignoreCase: false,
               out _)
           && IsSafeIdentityText(teacher.ModelId)
           && IsSafeIdentityText(teacher.ModelVersion)
           && IsSafeIdentityText(teacher.ModelArtifactDigestKind)
           && IsCanonicalSha256Hex(teacher.ModelArtifactSha256);

    private static bool ValidateGenerationPolicy(
        RepositoryOwnedDpoGenerationPolicyIdentity? policy)
        => policy is not null
           && IsSafeIdentityText(policy.StableName)
           && IsSafeIdentityText(policy.Version)
           && IsCanonicalSha256Hex(policy.ConfigSha256);

    private static bool ValidateCreatorIdentity(
        RepositoryOwnedDpoCreatorIdentity? creator)
        => creator is not null
           && IsSafeIdentityText(creator.StableName)
           && IsSafeIdentityText(creator.Version)
           && IsCanonicalSha256Hex(creator.EffectiveConfigSha256);

    private static OperationResult<bool> ValidatePairsAgainstRequestIdentity(
        RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
        IReadOnlyList<DpoPair> pairs)
    {
        var requestValidation = ValidateRequestIdentity(requestIdentity);
        if (!requestValidation.IsSuccess)
            return requestValidation;

        var paidReferenceAnswerHashes = requestIdentity.PaidReferenceAnswers?
            .OrderedAnswerSha256.ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        // Raw lessons bind the ordinary consumer's entire input, without routing wrappers.
        if (requestIdentity.RawSource is { } raw &&
            (pairs.Count != 1 || pairs[0].Domain != "csharp-code-documentation" ||
             ComputeTextSha256(pairs[0].Prompt) != raw.OwnerInputSha256 ||
             pairs[0].ChosenAuthorship?.AuthorKind != DPO.Models.DpoPairSideAuthorKind.OpenTeacher ||
             pairs[0].RejectedAuthorship?.AuthorKind != DPO.Models.DpoPairSideAuthorKind.OpenTeacher))
            return Invalid<bool>();
        foreach (var pair in pairs)
        {
            if (pair.TeacherProvenance is null
                || pair.ChosenAuthorship is null
                || pair.RejectedAuthorship is null
                || !TeacherIdentityMatchesProvenance(
                    requestIdentity.OpenTeacher,
                    pair.TeacherProvenance)
                || !SideAuthorshipMatchesTeacherIdentity(
                    pair.ChosenAuthorship,
                    requestIdentity.OpenTeacher)
                || !SideAuthorshipMatchesTeacherIdentity(
                    pair.RejectedAuthorship,
                    requestIdentity.OpenTeacher))
            {
                return Invalid<bool>();
            }

            // WHY: paid model answers are allowed here only as exact-byte
            // reference fingerprints. This is not a semantic detector; it only
            // blocks a paid reference answer being copied byte-for-byte into
            // either trainable side of the DPO pair.
            if (paidReferenceAnswerHashes.Contains(ComputeTextSha256(pair.Chosen))
                || paidReferenceAnswerHashes.Contains(
                    ComputeTextSha256(pair.Rejected)))
            {
                return Invalid<bool>();
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool TeacherIdentityMatchesProvenance(
        RepositoryOwnedDpoOpenTeacherIdentity teacher,
        TrainingTeacherProvenance provenance)
        => provenance is
           {
               SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: true,
               Family: not null,
           }
           && StringComparer.Ordinal.Equals(
               provenance.Family.Value.ToString(),
               teacher.Family)
           && StringComparer.Ordinal.Equals(provenance.ModelId, teacher.ModelId)
           && StringComparer.Ordinal.Equals(
               provenance.ModelVersion,
               teacher.ModelVersion)
           && StringComparer.Ordinal.Equals(
               provenance.ModelArtifactDigestKind,
               teacher.ModelArtifactDigestKind)
           && StringComparer.Ordinal.Equals(
               provenance.ModelArtifactDigest,
               teacher.ModelArtifactSha256);

    private static bool SideAuthorshipMatchesTeacherIdentity(
        DpoPairSideAuthorship authorship,
        RepositoryOwnedDpoOpenTeacherIdentity teacher)
        => authorship.AuthorKind is not DPO.Models.DpoPairSideAuthorKind
               .PaidExternalModel
           && (authorship.AuthorKind is not DPO.Models.DpoPairSideAuthorKind
                   .OpenTeacher
               || authorship.OpenTeacherPassport is not null
               && TeacherIdentityMatchesProvenance(
                   teacher,
                   authorship.OpenTeacherPassport));

    private static bool RequestIdentitiesEqual(
        RepositoryOwnedDpoDatasetRequestIdentity? left,
        RepositoryOwnedDpoDatasetRequestIdentity? right)
        => left is not null
           && right is not null
           && CryptographicOperations.FixedTimeEquals(
               WriteCanonicalRequestIdentityBytes(left),
               WriteCanonicalRequestIdentityBytes(right));

    private static bool IsSafeIdentityText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumIdentityTextLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl)
           && !value.Contains('\\')
           && !value.Contains('/')
           && !value.Contains("..", StringComparison.Ordinal)
           && !Path.IsPathRooted(value);

    private static OperationResult<IReadOnlyList<DpoPair>> ValidateSuppliedPairs(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
        IReadOnlyList<DpoPair>? suppliedPairs)
    {
        if (requestIdentity.RawSource is not null &&
            (address.DomainSlug != "csharp-code-documentation" || address.DesignPatternSlug != "ilearning-unit"))
            return Invalid<IReadOnlyList<DpoPair>>();
        if (suppliedPairs is null
            || suppliedPairs.Count == 0
            || suppliedPairs.Count > MaximumManifestPairCount)
        {
            return Invalid<IReadOnlyList<DpoPair>>();
        }

        var snapshot = new DpoPair[suppliedPairs.Count];
        for (var index = 0; index < suppliedPairs.Count; index++)
        {
            var pair = suppliedPairs[index];
            if (pair is null
                || !StringComparer.Ordinal.Equals(pair.Domain, address.DomainSlug)
                || pair.TeacherProvenance is null
                || pair.GovernanceDecision is null
                || pair.ChosenAuthorship is null
                || pair.RejectedAuthorship is null)
            {
                return Invalid<IReadOnlyList<DpoPair>>();
            }

            snapshot[index] = pair;
        }

        var identityValidation = ValidatePairsAgainstRequestIdentity(
            requestIdentity,
            snapshot);
        return identityValidation.IsSuccess
            ? OperationResult<IReadOnlyList<DpoPair>>.Ok(
                Array.AsReadOnly(snapshot))
            : identityValidation.MapError<IReadOnlyList<DpoPair>>();
    }

    private static OperationResult<IReadOnlyList<DpoPair>> ReadFreshStorePairs(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
            custodyContext)
    {
        try
        {
            return ContinuousLearningDpoPairStores
                .CreateFileStoreWithPreparedRepositoryOwnedDpoCustody(
                    address,
                    custodyContext)
                .GetAll();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<IReadOnlyList<DpoPair>>();
        }
    }

    private static OperationResult<IReadOnlyList<DpoPair>>
        ReadManifestIntegrityStorePairs(
            RepositoryOwnedDpoDatasetAddress address,
            IReadOnlyList<string> expectedPairIds)
    {
        try
        {
            return new FileDpoPairStore(address.PairStoreRoot)
                .ReadManifestIntegrityPairs(expectedPairIds);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<IReadOnlyList<DpoPair>>();
        }
    }

    private static bool FreshReaderMatchesSuppliedPairs(
        IReadOnlyList<string> suppliedIds,
        IReadOnlyList<DpoPair> suppliedPairs,
        IReadOnlyList<DpoPair> reopenedPairs)
    {
        if (suppliedIds.Count != suppliedPairs.Count
            || reopenedPairs.Count != suppliedPairs.Count)
        {
            return false;
        }

        var suppliedById = new Dictionary<string, DpoPair>(StringComparer.Ordinal);
        for (var index = 0; index < suppliedPairs.Count; index++)
        {
            if (!suppliedById.TryAdd(suppliedIds[index], suppliedPairs[index]))
                return false;
        }

        foreach (var reopened in reopenedPairs)
        {
            var reopenedId = FileDpoPairStore.ComputeIdentity(
                reopened,
                RichPairRecordSchemaVersion);
            if (!suppliedById.TryGetValue(reopenedId, out var supplied)
                || supplied != reopened)
            {
                return false;
            }
        }

        return true;
    }

    private static OperationResult<IReadOnlyDictionary<string, PairFileObservation>>
        InspectPairFiles(
            RepositoryOwnedDpoDatasetAddress address,
            IReadOnlyList<string> expectedPairIds)
    {
        if (expectedPairIds.Count == 0
            || expectedPairIds.Count > MaximumManifestPairCount
            || expectedPairIds.Distinct(StringComparer.Ordinal).Count()
                != expectedPairIds.Count)
        {
            return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();
        }

        try
        {
            if (!Directory.Exists(address.PairsRoot)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    address.PairsRoot))
            {
                return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();
            }

            var expected = expectedPairIds.ToHashSet(StringComparer.Ordinal);
            var observed = new Dictionary<string, PairFileObservation>(
                StringComparer.Ordinal);
            var paths = Directory
                .EnumerateFiles(
                    address.PairsRoot,
                    "*" + PairFileSuffix,
                    SearchOption.TopDirectoryOnly)
                .Take(MaximumManifestPairCount + 1)
                .ToArray();
            if (paths.Length != expected.Count)
                return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();

            foreach (var path in paths)
            {
                var fileName = Path.GetFileName(path);
                if (!fileName.EndsWith(PairFileSuffix, StringComparison.Ordinal))
                    return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();
                var pairId = fileName[..^PairFileSuffix.Length];
                if (!IsCanonicalSha256Hex(pairId)
                    || !expected.Contains(pairId)
                    || !observed.TryAdd(
                        pairId,
                        new PairFileObservation(
                            pairId,
                            RepositoryPairPath(address, pairId),
                            string.Empty)))
                {
                    return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();
                }

                var expectedPath = Path.Combine(
                    address.PairsRoot,
                    pairId + PairFileSuffix);
                if (!PathsEqual(path, expectedPath)
                    || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                        expectedPath))
                {
                    return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();
                }

                var hash = ComputeFileSha256(expectedPath, MaximumPairFileBytes);
                if (!hash.IsSuccess || hash.Value is null)
                    return hash.MapError<IReadOnlyDictionary<string, PairFileObservation>>();
                observed[pairId] = observed[pairId] with
                {
                    PairFileSha256 = hash.Value,
                };
            }

            if (!expected.SetEquals(observed.Keys))
                return Invalid<IReadOnlyDictionary<string, PairFileObservation>>();

            return OperationResult<IReadOnlyDictionary<string, PairFileObservation>>
                .Ok(observed);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<IReadOnlyDictionary<string, PairFileObservation>>();
        }
    }

    private static RepositoryOwnedDpoDatasetManifest BuildManifest(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
        IReadOnlyDictionary<string, PairFileObservation> fileSet)
    {
        var entries = fileSet.Values
            .OrderBy(static entry => entry.PairId, StringComparer.Ordinal)
            .Select(static entry => new RepositoryOwnedDpoDatasetManifestPair(
                entry.PairId,
                entry.RepositoryRelativePairFilePath,
                entry.PairFileSha256))
            .ToArray();
        var unsigned = new RepositoryOwnedDpoDatasetManifest(
            CurrentSchema,
            CurrentContext,
            address.DomainSlug,
            address.DesignPatternSlug,
            address.DatasetVersion,
            requestIdentity,
            entries.Length,
            Array.AsReadOnly(entries),
            string.Empty);
        var manifestSha = ComputeSha256(WriteCanonicalManifestBytes(unsigned));
        return unsigned with
        {
            ManifestSha256 = manifestSha,
        };
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifest>
        ReadStrictManifest(string manifestPath)
    {
        // WHY: sealed-open readers keep their typed return and existing errors.
        var readback = ReadStrictManifestFileIdentity(manifestPath);
        return readback.IsSuccess && readback.Value is not null
            ? OperationResult<RepositoryOwnedDpoDatasetManifest>.Ok(
                readback.Value.Manifest)
            : readback.MapError<RepositoryOwnedDpoDatasetManifest>();
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>
        ReadStrictManifestFileIdentity(string manifestPath)
    {
        var bytes = ReadBoundedStableFileBytes(
            manifestPath,
            MaximumManifestBytes);
        if (!bytes.IsSuccess || bytes.Value is null)
            return bytes.MapError<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();

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
            if (!TryReadManifestElement(document.RootElement, out var manifest))
                return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();

            var selfHash = ComputeSha256(WriteCanonicalManifestBytes(
                manifest with
                {
                    ManifestSha256 = string.Empty,
                }));
            if (!StringComparer.Ordinal.Equals(
                    selfHash,
                    manifest.ManifestSha256))
            {
                return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
            }

            // WHY: parser, digest and length consume the identical stable byte
            // buffer; a path hash would reopen, and canonical JSON loses layout.
            return OperationResult<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>
                .Ok(new RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult(
                    manifest,
                    manifest.RequestIdentity,
                    ComputeSha256(bytes.Value),
                    bytes.Value.LongLength));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedDpoDatasetManifestFileIdentityOpenResult>();
        }
    }

    private static OperationResult<bool> ValidateManifestForAddress(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetManifest manifest)
    {
        if (!StringComparer.Ordinal.Equals(manifest.Schema, CurrentSchema)
            || !StringComparer.Ordinal.Equals(manifest.Context, CurrentContext)
            || !StringComparer.Ordinal.Equals(manifest.Domain, address.DomainSlug)
            || !StringComparer.Ordinal.Equals(
                manifest.DesignPattern,
                address.DesignPatternSlug)
            || !StringComparer.Ordinal.Equals(
                manifest.DatasetVersion,
                address.DatasetVersion)
            || !ValidateRequestIdentity(manifest.RequestIdentity).IsSuccess
            || (manifest.RequestIdentity.RawSource is not null &&
                (manifest.Domain != "csharp-code-documentation" || manifest.DesignPattern != "ilearning-unit" || manifest.PairCount != 1))
            || manifest.PairCount <= 0
            || manifest.PairCount > MaximumManifestPairCount
            || manifest.OrderedPairs.Count != manifest.PairCount
            || !IsCanonicalSha256Hex(manifest.ManifestSha256))
        {
            return Invalid<bool>();
        }

        var previousPairId = string.Empty;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in manifest.OrderedPairs)
        {
            if (!IsCanonicalSha256Hex(pair.PairId)
                || !IsCanonicalSha256Hex(pair.PairFileSha256)
                || !StringComparer.Ordinal.Equals(
                    pair.RepositoryRelativePairFilePath,
                    RepositoryPairPath(address, pair.PairId))
                || !seen.Add(pair.PairId))
            {
                return Invalid<bool>();
            }

            if (previousPairId.Length != 0
                && StringComparer.Ordinal.Compare(previousPairId, pair.PairId) >= 0)
            {
                return Invalid<bool>();
            }

            previousPairId = pair.PairId;
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<IReadOnlyList<DpoPair>>
        MapReopenedPairsByManifestOrder(
            RepositoryOwnedDpoDatasetManifest manifest,
            IReadOnlyList<DpoPair> reopenedPairs)
    {
        if (reopenedPairs.Count != manifest.PairCount)
            return Invalid<IReadOnlyList<DpoPair>>();

        var byId = new Dictionary<string, DpoPair>(StringComparer.Ordinal);
        foreach (var pair in reopenedPairs)
        {
            if (pair is null
                || !StringComparer.Ordinal.Equals(pair.Domain, manifest.Domain)
                || pair.TeacherProvenance is null
                || pair.GovernanceDecision is null
                || pair.ChosenAuthorship is null
                || pair.RejectedAuthorship is null)
            {
                return Invalid<IReadOnlyList<DpoPair>>();
            }

            var pairId = FileDpoPairStore.ComputeIdentity(
                pair,
                RichPairRecordSchemaVersion);
            if (!byId.TryAdd(pairId, pair))
                return Invalid<IReadOnlyList<DpoPair>>();
        }

        var ordered = new DpoPair[manifest.PairCount];
        for (var index = 0; index < manifest.OrderedPairs.Count; index++)
        {
            if (!byId.TryGetValue(manifest.OrderedPairs[index].PairId, out var pair))
                return Invalid<IReadOnlyList<DpoPair>>();
            ordered[index] = pair;
        }

        return OperationResult<IReadOnlyList<DpoPair>>.Ok(
            Array.AsReadOnly(ordered));
    }

    private static OperationResult<bool> ExistingManifestIsEquivalent(
        string manifestPath,
        byte[] expectedCanonicalBytes,
        RepositoryOwnedDpoDatasetManifest expectedManifest)
    {
        var existingBytes = ReadBoundedStableFileBytes(
            manifestPath,
            MaximumManifestBytes);
        if (!existingBytes.IsSuccess || existingBytes.Value is null)
            return existingBytes.MapError<bool>();

        if (CryptographicOperations.FixedTimeEquals(
                existingBytes.Value,
                expectedCanonicalBytes))
        {
            return OperationResult<bool>.Ok(true);
        }

        var existing = ReadStrictManifest(manifestPath);
        if (!existing.IsSuccess || existing.Value is null)
            return existing.MapError<bool>();
        return OperationResult<bool>.Ok(
            ManifestsEqual(existing.Value, expectedManifest));
    }

    private static bool ManifestsEqual(
        RepositoryOwnedDpoDatasetManifest left,
        RepositoryOwnedDpoDatasetManifest right)
    {
        if (!StringComparer.Ordinal.Equals(left.Schema, right.Schema)
            || !StringComparer.Ordinal.Equals(left.Context, right.Context)
            || !StringComparer.Ordinal.Equals(left.Domain, right.Domain)
            || !StringComparer.Ordinal.Equals(left.DesignPattern, right.DesignPattern)
            || !StringComparer.Ordinal.Equals(left.DatasetVersion, right.DatasetVersion)
            || !RequestIdentitiesEqual(left.RequestIdentity, right.RequestIdentity)
            || left.PairCount != right.PairCount
            || !StringComparer.Ordinal.Equals(
                left.ManifestSha256,
                right.ManifestSha256)
            || left.OrderedPairs.Count != right.OrderedPairs.Count)
        {
            return false;
        }

        for (var index = 0; index < left.OrderedPairs.Count; index++)
        {
            if (left.OrderedPairs[index] != right.OrderedPairs[index])
                return false;
        }

        return true;
    }

    private static bool TryReadManifestElement(
        JsonElement element,
        out RepositoryOwnedDpoDatasetManifest manifest)
    {
        manifest = EmptyManifest();
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var schemaSeen = false;
        var contextSeen = false;
        var domainSeen = false;
        var designPatternSeen = false;
        var datasetVersionSeen = false;
        var requestIdentitySeen = false;
        var pairCountSeen = false;
        var orderedPairsSeen = false;
        var manifestShaSeen = false;
        var schema = string.Empty;
        var context = string.Empty;
        var domain = string.Empty;
        var designPattern = string.Empty;
        var datasetVersion = string.Empty;
        RepositoryOwnedDpoDatasetRequestIdentity? requestIdentity = null;
        var pairCount = 0;
        IReadOnlyList<RepositoryOwnedDpoDatasetManifestPair>? orderedPairs = null;
        var manifestSha = string.Empty;

        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case SchemaProperty:
                    if (!TryReadString(property.Value, ref schemaSeen, out schema))
                        return false;
                    break;
                case ContextProperty:
                    if (!TryReadString(property.Value, ref contextSeen, out context))
                        return false;
                    break;
                case DomainProperty:
                    if (!TryReadString(property.Value, ref domainSeen, out domain))
                        return false;
                    break;
                case DesignPatternProperty:
                    if (!TryReadString(
                            property.Value,
                            ref designPatternSeen,
                            out designPattern))
                    {
                        return false;
                    }
                    break;
                case DatasetVersionProperty:
                    if (!TryReadString(
                            property.Value,
                            ref datasetVersionSeen,
                            out datasetVersion))
                    {
                        return false;
                    }
                    break;
                case RequestIdentityProperty:
                    if (requestIdentitySeen
                        || !TryReadRequestIdentity(
                            property.Value,
                            out requestIdentity))
                    {
                        return false;
                    }

                    requestIdentitySeen = true;
                    break;
                case PairCountProperty:
                    if (!TryReadPairCount(
                            property.Value,
                            ref pairCountSeen,
                            out pairCount))
                    {
                        return false;
                    }
                    break;
                case OrderedPairsProperty:
                    if (orderedPairsSeen
                        || !TryReadManifestPairs(property.Value, out orderedPairs))
                    {
                        return false;
                    }

                    orderedPairsSeen = true;
                    break;
                case ManifestSha256Property:
                    if (!TryReadString(
                            property.Value,
                            ref manifestShaSeen,
                            out manifestSha))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
        }

        if (!schemaSeen
            || !contextSeen
            || !domainSeen
            || !designPatternSeen
            || !datasetVersionSeen
            || !requestIdentitySeen
            || !pairCountSeen
            || !orderedPairsSeen
            || !manifestShaSeen
            || requestIdentity is null
            || orderedPairs is null)
        {
            return false;
        }

        manifest = new RepositoryOwnedDpoDatasetManifest(
            schema,
            context,
            domain,
            designPattern,
            datasetVersion,
            requestIdentity,
            pairCount,
            orderedPairs,
            manifestSha);
        return true;
    }

    private static bool TryReadRequestIdentity(
        JsonElement element,
        out RepositoryOwnedDpoDatasetRequestIdentity? identity)
    {
        identity = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var schemaSeen = false;
        var paidSeen = false;
        var rawSeen = false;
        var teacherSeen = false;
        var policySeen = false;
        var creatorSeen = false;
        var shaSeen = false;
        var schema = string.Empty;
        RepositoryOwnedDpoPaidReferenceAnswerSet? paid = null;
        RepositoryOwnedDpoRawSourceIdentity? raw = null;
        RepositoryOwnedDpoOpenTeacherIdentity? teacher = null;
        RepositoryOwnedDpoGenerationPolicyIdentity? policy = null;
        RepositoryOwnedDpoCreatorIdentity? creator = null;
        var sha = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case SchemaProperty:
                    if (!TryReadString(property.Value, ref schemaSeen, out schema))
                        return false;
                    break;
                case PaidReferenceAnswersProperty:
                    if (paidSeen
                        || !TryReadPaidReferenceAnswerSet(
                            property.Value,
                            out paid))
                    {
                        return false;
                    }

                    paidSeen = true;
                    break;
                case "rawSource":
                    if (rawSeen || !TryReadRawSource(property.Value, out raw))
                        return false;
                    rawSeen = true;
                    break;
                case OpenTeacherProperty:
                    if (teacherSeen
                        || !TryReadOpenTeacherIdentity(
                            property.Value,
                            out teacher))
                    {
                        return false;
                    }

                    teacherSeen = true;
                    break;
                case GenerationPolicyProperty:
                    if (policySeen
                        || !TryReadGenerationPolicy(
                            property.Value,
                            out policy))
                    {
                        return false;
                    }

                    policySeen = true;
                    break;
                case CreatorProperty:
                    if (creatorSeen
                        || !TryReadCreatorIdentity(
                            property.Value,
                            out creator))
                    {
                        return false;
                    }

                    creatorSeen = true;
                    break;
                case RequestIdentitySha256Property:
                    if (!TryReadString(property.Value, ref shaSeen, out sha))
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (!schemaSeen
            || (schema == CurrentRequestIdentitySchema ? (!paidSeen || rawSeen) :
                schema == RawRequestIdentitySchema ? (!rawSeen || paidSeen) : true)
            || !teacherSeen
            || !policySeen
            || !creatorSeen
            || !shaSeen
            || teacher is null
            || policy is null
            || creator is null)
        {
            return false;
        }

        identity = new RepositoryOwnedDpoDatasetRequestIdentity(
            schema,
            paid,
            teacher,
            policy,
            creator,
            sha,
            raw);
        return true;
    }

    private static bool TryReadPaidReferenceAnswerSet(
        JsonElement element,
        out RepositoryOwnedDpoPaidReferenceAnswerSet? paid)
    {
        paid = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var countSeen = false;
        var orderedSeen = false;
        var listShaSeen = false;
        var count = 0;
        IReadOnlyList<string>? ordered = null;
        var listSha = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case AnswerCountProperty:
                    if (!TryReadPairCount(
                            property.Value,
                            ref countSeen,
                            out count))
                    {
                        return false;
                    }
                    break;
                case OrderedAnswerSha256Property:
                    if (orderedSeen
                        || !TryReadStringArray(property.Value, out ordered))
                    {
                        return false;
                    }

                    orderedSeen = true;
                    break;
                case CanonicalListSha256Property:
                    if (!TryReadString(
                            property.Value,
                            ref listShaSeen,
                            out listSha))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
        }

        if (!countSeen || !orderedSeen || !listShaSeen || ordered is null)
            return false;

        paid = new RepositoryOwnedDpoPaidReferenceAnswerSet(
            count,
            ordered,
            listSha);
        return true;
    }

    private static bool TryReadOpenTeacherIdentity(
        JsonElement element,
        out RepositoryOwnedDpoOpenTeacherIdentity? teacher)
    {
        teacher = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var familySeen = false;
        var modelIdSeen = false;
        var modelVersionSeen = false;
        var digestKindSeen = false;
        var digestSeen = false;
        var family = string.Empty;
        var modelId = string.Empty;
        var modelVersion = string.Empty;
        var digestKind = string.Empty;
        var digest = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case FamilyProperty:
                    if (!TryReadString(property.Value, ref familySeen, out family))
                        return false;
                    break;
                case ModelIdProperty:
                    if (!TryReadString(property.Value, ref modelIdSeen, out modelId))
                        return false;
                    break;
                case ModelVersionProperty:
                    if (!TryReadString(
                            property.Value,
                            ref modelVersionSeen,
                            out modelVersion))
                    {
                        return false;
                    }
                    break;
                case ModelArtifactDigestKindProperty:
                    if (!TryReadString(
                            property.Value,
                            ref digestKindSeen,
                            out digestKind))
                    {
                        return false;
                    }
                    break;
                case ModelArtifactSha256Property:
                    if (!TryReadString(property.Value, ref digestSeen, out digest))
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (!familySeen
            || !modelIdSeen
            || !modelVersionSeen
            || !digestKindSeen
            || !digestSeen)
        {
            return false;
        }

        teacher = new RepositoryOwnedDpoOpenTeacherIdentity(
            family,
            modelId,
            modelVersion,
            digestKind,
            digest);
        return true;
    }

    private static bool TryReadGenerationPolicy(
        JsonElement element,
        out RepositoryOwnedDpoGenerationPolicyIdentity? policy)
    {
        policy = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var nameSeen = false;
        var versionSeen = false;
        var configSeen = false;
        var name = string.Empty;
        var version = string.Empty;
        var config = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case StableNameProperty:
                    if (!TryReadString(property.Value, ref nameSeen, out name))
                        return false;
                    break;
                case VersionProperty:
                    if (!TryReadString(
                            property.Value,
                            ref versionSeen,
                            out version))
                    {
                        return false;
                    }
                    break;
                case ConfigSha256Property:
                    if (!TryReadString(
                            property.Value,
                            ref configSeen,
                            out config))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
        }

        if (!nameSeen || !versionSeen || !configSeen)
            return false;

        policy = new RepositoryOwnedDpoGenerationPolicyIdentity(
            name,
            version,
            config);
        return true;
    }

    private static bool TryReadCreatorIdentity(
        JsonElement element,
        out RepositoryOwnedDpoCreatorIdentity? creator)
    {
        creator = null;
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var nameSeen = false;
        var versionSeen = false;
        var configSeen = false;
        var name = string.Empty;
        var version = string.Empty;
        var config = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case StableNameProperty:
                    if (!TryReadString(property.Value, ref nameSeen, out name))
                        return false;
                    break;
                case VersionProperty:
                    if (!TryReadString(
                            property.Value,
                            ref versionSeen,
                            out version))
                    {
                        return false;
                    }
                    break;
                case EffectiveConfigSha256Property:
                    if (!TryReadString(
                            property.Value,
                            ref configSeen,
                            out config))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
        }

        if (!nameSeen || !versionSeen || !configSeen)
            return false;

        creator = new RepositoryOwnedDpoCreatorIdentity(
            name,
            version,
            config);
        return true;
    }

    private static bool TryReadStringArray(
        JsonElement element,
        out IReadOnlyList<string>? values)
    {
        values = null;
        if (element.ValueKind != JsonValueKind.Array)
            return false;

        var rows = new List<string>();
        foreach (var row in element.EnumerateArray())
        {
            if (rows.Count == MaximumManifestPairCount
                || row.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var parsed = row.GetString();
            if (parsed is null)
                return false;
            rows.Add(parsed);
        }

        values = Array.AsReadOnly(rows.ToArray());
        return true;
    }

    private static bool TryReadManifestPairs(
        JsonElement element,
        out IReadOnlyList<RepositoryOwnedDpoDatasetManifestPair> pairs)
    {
        pairs = Array.Empty<RepositoryOwnedDpoDatasetManifestPair>();
        if (element.ValueKind != JsonValueKind.Array)
            return false;

        var rows = new List<RepositoryOwnedDpoDatasetManifestPair>();
        foreach (var row in element.EnumerateArray())
        {
            if (rows.Count == MaximumManifestPairCount
                || !TryReadManifestPair(row, out var pair))
            {
                return false;
            }

            rows.Add(pair);
        }

        pairs = Array.AsReadOnly(rows.ToArray());
        return true;
    }

    private static bool TryReadManifestPair(
        JsonElement element,
        out RepositoryOwnedDpoDatasetManifestPair pair)
    {
        pair = new RepositoryOwnedDpoDatasetManifestPair(
            string.Empty,
            string.Empty,
            string.Empty);
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var pairIdSeen = false;
        var pathSeen = false;
        var shaSeen = false;
        var pairId = string.Empty;
        var path = string.Empty;
        var sha = string.Empty;
        foreach (var property in element.EnumerateObject())
        {
            switch (property.Name)
            {
                case PairIdProperty:
                    if (!TryReadString(property.Value, ref pairIdSeen, out pairId))
                        return false;
                    break;
                case RepositoryRelativePairFilePathProperty:
                    if (!TryReadString(property.Value, ref pathSeen, out path))
                        return false;
                    break;
                case PairFileSha256Property:
                    if (!TryReadString(property.Value, ref shaSeen, out sha))
                        return false;
                    break;
                default:
                    return false;
            }
        }

        if (!pairIdSeen || !pathSeen || !shaSeen)
            return false;

        pair = new RepositoryOwnedDpoDatasetManifestPair(pairId, path, sha);
        return true;
    }

    private static bool TryReadString(
        JsonElement element,
        ref bool seen,
        out string value)
    {
        value = string.Empty;
        if (seen || element.ValueKind != JsonValueKind.String)
            return false;
        var parsed = element.GetString();
        if (parsed is null)
            return false;
        value = parsed;
        seen = true;
        return true;
    }

    private static bool TryReadPairCount(
        JsonElement element,
        ref bool seen,
        out int value)
    {
        value = 0;
        if (seen
            || element.ValueKind != JsonValueKind.Number
            || !element.TryGetInt32(out value))
        {
            return false;
        }

        seen = true;
        return true;
    }

    private static byte[] WriteCanonicalManifestBytes(
        RepositoryOwnedDpoDatasetManifest manifest)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString(SchemaProperty, manifest.Schema);
            writer.WriteString(ContextProperty, manifest.Context);
            writer.WriteString(DomainProperty, manifest.Domain);
            writer.WriteString(DesignPatternProperty, manifest.DesignPattern);
            writer.WriteString(DatasetVersionProperty, manifest.DatasetVersion);
            writer.WritePropertyName(RequestIdentityProperty);
            WriteRequestIdentity(writer, manifest.RequestIdentity);
            writer.WriteNumber(PairCountProperty, manifest.PairCount);
            writer.WriteStartArray(OrderedPairsProperty);
            foreach (var pair in manifest.OrderedPairs)
            {
                writer.WriteStartObject();
                writer.WriteString(PairIdProperty, pair.PairId);
                writer.WriteString(
                    RepositoryRelativePairFilePathProperty,
                    pair.RepositoryRelativePairFilePath);
                writer.WriteString(PairFileSha256Property, pair.PairFileSha256);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteString(ManifestSha256Property, manifest.ManifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static byte[] WriteCanonicalRequestIdentityBytes(
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            WriteRequestIdentity(writer, identity);
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static byte[] WriteCanonicalPaidReferenceAnswerListBytes(
        IReadOnlyList<string> orderedAnswerSha256)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            foreach (var answerSha256 in orderedAnswerSha256)
                writer.WriteStringValue(answerSha256);
            writer.WriteEndArray();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static void WriteRequestIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        writer.WriteStartObject();
        writer.WriteString(SchemaProperty, identity.Schema);
        // The legacy branch writes precisely the original bytes and property order.
        if (identity.Schema == RawRequestIdentitySchema)
        {
            writer.WritePropertyName("rawSource");
            WriteRawSource(writer, identity.RawSource!);
        }
        else
        {
            writer.WritePropertyName(PaidReferenceAnswersProperty);
            WritePaidReferenceAnswerSet(writer, identity.PaidReferenceAnswers!);
        }
        writer.WritePropertyName(OpenTeacherProperty);
        WriteOpenTeacherIdentity(writer, identity.OpenTeacher);
        writer.WritePropertyName(GenerationPolicyProperty);
        WriteGenerationPolicyIdentity(writer, identity.GenerationPolicy);
        writer.WritePropertyName(CreatorProperty);
        WriteCreatorIdentity(writer, identity.Creator);
        writer.WriteString(
            RequestIdentitySha256Property,
            identity.RequestIdentitySha256);
        writer.WriteEndObject();
    }

    private static void WritePaidReferenceAnswerSet(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoPaidReferenceAnswerSet paidReference)
    {
        writer.WriteStartObject();
        writer.WriteNumber(AnswerCountProperty, paidReference.AnswerCount);
        writer.WriteStartArray(OrderedAnswerSha256Property);
        foreach (var answerSha256 in paidReference.OrderedAnswerSha256)
            writer.WriteStringValue(answerSha256);
        writer.WriteEndArray();
        writer.WriteString(
            CanonicalListSha256Property,
            paidReference.CanonicalListSha256);
        writer.WriteEndObject();
    }

    private static void WriteOpenTeacherIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoOpenTeacherIdentity teacher)
    {
        writer.WriteStartObject();
        writer.WriteString(FamilyProperty, teacher.Family);
        writer.WriteString(ModelIdProperty, teacher.ModelId);
        writer.WriteString(ModelVersionProperty, teacher.ModelVersion);
        writer.WriteString(
            ModelArtifactDigestKindProperty,
            teacher.ModelArtifactDigestKind);
        writer.WriteString(ModelArtifactSha256Property, teacher.ModelArtifactSha256);
        writer.WriteEndObject();
    }

    private static void WriteGenerationPolicyIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoGenerationPolicyIdentity policy)
    {
        writer.WriteStartObject();
        writer.WriteString(StableNameProperty, policy.StableName);
        writer.WriteString(VersionProperty, policy.Version);
        writer.WriteString(ConfigSha256Property, policy.ConfigSha256);
        writer.WriteEndObject();
    }

    private static void WriteCreatorIdentity(
        Utf8JsonWriter writer,
        RepositoryOwnedDpoCreatorIdentity creator)
    {
        writer.WriteStartObject();
        writer.WriteString(StableNameProperty, creator.StableName);
        writer.WriteString(VersionProperty, creator.Version);
        writer.WriteString(
            EffectiveConfigSha256Property,
            creator.EffectiveConfigSha256);
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

    private static OperationResult<string> ComputeFileSha256(
        string path,
        long maximumBytes)
    {
        var bytes = ReadBoundedStableFileBytes(path, maximumBytes);
        return bytes.IsSuccess && bytes.Value is not null
            ? OperationResult<string>.Ok(ComputeSha256(bytes.Value))
            : bytes.MapError<string>();
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string ComputeTextSha256(string value)
        => ComputeSha256(Encoding.UTF8.GetBytes(value));

    private static OperationResult<DatasetProcessLease>
        AcquireProcessDatasetSealLease(
            string canonicalDatasetRoot,
            Stopwatch timeoutBudget)
    {
        if (!SecureLocalFileBoundary.IsSupportedPlatform())
            return Unavailable<DatasetProcessLease>();

        DatasetProcessGateEntry entry;
        lock (DatasetProcessGateRegistrySync)
        {
            if (!DatasetProcessGateRegistry.TryGetValue(
                    canonicalDatasetRoot,
                    out entry!))
            {
                entry = new DatasetProcessGateEntry();
                DatasetProcessGateRegistry.Add(canonicalDatasetRoot, entry);
            }

            checked
            {
                entry.ReferenceCount++;
            }
        }

        try
        {
            var remaining = DefaultDatasetSealLockTimeout - timeoutBudget.Elapsed;
            if (remaining <= TimeSpan.Zero || !entry.Semaphore.Wait(remaining))
            {
                ReleaseDatasetProcessGateReference(canonicalDatasetRoot, entry);
                return Busy<DatasetProcessLease>();
            }

            return OperationResult<DatasetProcessLease>.Ok(
                new DatasetProcessLease(canonicalDatasetRoot, entry));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ReleaseDatasetProcessGateReference(canonicalDatasetRoot, entry);
            return Unavailable<DatasetProcessLease>();
        }
    }

    private static OperationResult<DatasetSealLease> AcquireDatasetSealLease(
        RepositoryOwnedDpoDatasetAddress address,
        string canonicalDatasetRoot,
        Stopwatch timeoutBudget)
    {
        if (!SecureLocalFileBoundary.IsSupportedPlatform()
            || !PathsEqual(address.DatasetRoot, canonicalDatasetRoot))
        {
            return Unavailable<DatasetSealLease>();
        }

        var lockPath = DatasetSealLockPath(canonicalDatasetRoot);
        while (true)
        {
            if (DefaultDatasetSealLockTimeout - timeoutBudget.Elapsed <= TimeSpan.Zero)
                return Busy<DatasetSealLease>();

            FileStream? stream = null;
            DatasetSealLease? lease = null;
            try
            {
                if (AnyExistingPathSegmentIsReparsePoint(canonicalDatasetRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || (File.Exists(lockPath)
                        && !HasSafeUnixFileMode(lockPath)))
                {
                    return Unavailable<DatasetSealLease>();
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
                    || AnyExistingPathSegmentIsReparsePoint(canonicalDatasetRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || !HasSafeUnixFileMode(lockPath))
                {
                    stream.Dispose();
                    return Unavailable<DatasetSealLease>();
                }

                if (stream.Length == 0)
                {
                    stream.SetLength(DatasetSealLockLength);
                    stream.Flush(flushToDisk: true);
                }

                if (stream.Length != DatasetSealLockLength)
                {
                    stream.Dispose();
                    return Unavailable<DatasetSealLease>();
                }

                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    stream.Lock(
                        DatasetSealLockOffset,
                        DatasetSealLockLength);
                }
                else
                {
                    stream.Dispose();
                    return Unavailable<DatasetSealLease>();
                }

                lease = new DatasetSealLease(stream);
                if (!lease.MatchesRequestedPath(lockPath)
                    || !lease.HasExpectedLength()
                    || AnyExistingPathSegmentIsReparsePoint(canonicalDatasetRoot)
                    || AnyExistingPathSegmentIsReparsePoint(lockPath)
                    || !HasSafeUnixFileMode(lockPath))
                {
                    lease.Dispose();
                    return Unavailable<DatasetSealLease>();
                }

                return OperationResult<DatasetSealLease>.Ok(lease);
            }
            catch (IOException exception)
                when (FileDpoPairStore.IsLockContention(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                lease?.Dispose();
                stream?.Dispose();
                var remaining = DefaultDatasetSealLockTimeout - timeoutBudget.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    return Busy<DatasetSealLease>();

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
                return Unavailable<DatasetSealLease>();
            }
        }
    }

    private static OperationResult<bool> EnsureWritableDatasetRoot(
        RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || AnyExistingPathSegmentIsReparsePoint(address.DatasetRoot))
            {
                return Unavailable<bool>();
            }

            if (Directory.Exists(address.DatasetRoot))
            {
                if (!HasSafeUnixDirectoryMode(address.DatasetRoot))
                    return Unavailable<bool>();
            }
            else if (File.Exists(address.DatasetRoot))
            {
                return Unavailable<bool>();
            }
            else if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(address.DatasetRoot);
            }
            else
            {
                Directory.CreateDirectory(address.DatasetRoot, OwnerDirectoryMode);
            }

            if (AnyExistingPathSegmentIsReparsePoint(address.DatasetRoot)
                || !HasSafeUnixDirectoryMode(address.DatasetRoot))
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

    private static string DatasetSealLockPath(string canonicalDatasetRoot)
        => Path.Combine(canonicalDatasetRoot, DatasetSealLockFileName);

    private static bool HasSafeUnixDirectoryMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(path);

    private static bool HasSafeUnixFileMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyFileMode(path);

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(fullPath);

    private static void ReleaseDatasetProcessGateReference(
        string canonicalDatasetRoot,
        DatasetProcessGateEntry entry)
    {
        SemaphoreSlim? semaphoreToDispose = null;
        lock (DatasetProcessGateRegistrySync)
        {
            entry.ReferenceCount--;
            if (entry.ReferenceCount != 0)
                return;
            if (!DatasetProcessGateRegistry.TryGetValue(
                    canonicalDatasetRoot,
                    out var registered)
                || !ReferenceEquals(registered, entry))
            {
                throw new InvalidOperationException(
                    "DPO dataset seal process-gate registry identity was lost.");
            }

            DatasetProcessGateRegistry.Remove(canonicalDatasetRoot);
            semaphoreToDispose = entry.Semaphore;
        }

        semaphoreToDispose.Dispose();
    }

    private static void ReleaseOwnedDatasetProcessGate(
        string canonicalDatasetRoot,
        DatasetProcessGateEntry entry)
    {
        try
        {
            entry.Semaphore.Release();
        }
        finally
        {
            ReleaseDatasetProcessGateReference(canonicalDatasetRoot, entry);
        }
    }

    private static string RepositoryPairPath(
        RepositoryOwnedDpoDatasetAddress address,
        string pairId)
        => address.PairsRootRelativePath + "/" + pairId + PairFileSuffix;

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: HashHexLength }
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static void DeleteOwnManifestTemp(
        RepositoryOwnedDpoDatasetAddress address,
        string temporaryPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(temporaryPath);
            var fileName = Path.GetFileName(fullPath);
            if (PathsEqual(Path.GetDirectoryName(fullPath)!, address.DatasetRoot)
                && fileName.StartsWith(ManifestTempPrefix, StringComparison.Ordinal)
                && fileName.EndsWith(ManifestTempSuffix, StringComparison.Ordinal)
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
            // Cleanup is best-effort and never broadens into user-owned files.
        }
    }

    private static RepositoryOwnedDpoDatasetManifest EmptyManifest()
        => new(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            EmptyRequestIdentity(),
            PairCount: 0,
            Array.Empty<RepositoryOwnedDpoDatasetManifestPair>(),
            string.Empty);

    private static RepositoryOwnedDpoDatasetRequestIdentity EmptyRequestIdentity()
        => new(
            string.Empty,
            new RepositoryOwnedDpoPaidReferenceAnswerSet(
                AnswerCount: 0,
                Array.Empty<string>(),
                string.Empty),
            new RepositoryOwnedDpoOpenTeacherIdentity(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                string.Empty,
                string.Empty,
                string.Empty),
            new RepositoryOwnedDpoCreatorIdentity(
                string.Empty,
                string.Empty,
                string.Empty),
            string.Empty);

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset manifest is invalid.",
            InvalidErrorCode);

    private static OperationResult<T> Conflict<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset manifest already seals different content.",
            ConflictErrorCode);

    private static OperationResult<T> IdentityMismatch<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset manifest seals a different request identity.",
            RequestIdentityMismatchErrorCode);

    private static OperationResult<T> Busy<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset manifest storage is busy.",
            BusyErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset manifest storage is unavailable.",
            UnavailableErrorCode);

    private sealed record PairFileObservation(
        string PairId,
        string RepositoryRelativePairFilePath,
        string PairFileSha256);

    private sealed class DatasetProcessGateEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount { get; set; }
    }

    private sealed class DatasetProcessLease(
        string canonicalDatasetRoot,
        DatasetProcessGateEntry entry) : IDisposable
    {
        private DatasetProcessGateEntry? _entry = entry;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _entry, null);
            if (owned is not null)
                ReleaseOwnedDatasetProcessGate(canonicalDatasetRoot, owned);
        }
    }

    private sealed class DatasetSealLease(FileStream stream) : IDisposable
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
                && owned.Length == DatasetSealLockLength;
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
                        DatasetSealLockOffset,
                        DatasetSealLockLength);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Closing the handle below unconditionally releases any OS
                // byte-range lock if an explicit unlock cannot complete.
            }
            finally
            {
                owned.Dispose();
            }
        }
    }
}
