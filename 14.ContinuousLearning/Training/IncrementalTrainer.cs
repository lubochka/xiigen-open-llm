// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TensorCore.Layers;
using TransformerModel;

/// <summary>
/// Runs isolated, supervised LoRA updates and publishes an adapter only after
/// proving that base weights stayed byte-identical and adapter weights changed.
/// Non-empty DPO input is admitted by immutable server-owned legal policy and
/// routed through the stable 11.DPO capability before publication.
/// </summary>
public sealed class IncrementalTrainer : BaseService, IIncrementalTrainer,
    ITransactionalIncrementalTrainer
{
    private const int MaximumTransactionalTrainingExampleCount = 100_000;
    private const long MaximumTransactionalTrainingExampleUtf8Bytes =
        64L * 1024L * 1024L;
    private const string DpoTransactionRequiredErrorCode =
        "DPO_TRANSACTION_REQUIRED";
    public static LoraConfig DefaultConfig => CreateDefaultConfig();

    private readonly IIncrementalTrainingSessionFactory? _sessionFactory;
    private readonly string _adapterDir;
    private readonly LoraConfig _loraConfig;
    private readonly ServerOwnedLegalTrainingTeacherAdmissionAuthority?
        _legalTeacherAdmissionAuthority;
    private readonly IDpoTrainingExecutor? _dpoTrainingExecutor;
    private readonly int _dpoMaxStepCount;
    private readonly ISupervisedTrainingStepExecutor _supervisedTrainingStepExecutor;
    private readonly SemaphoreSlim _trainingGate = new(1, 1);

    // Compatibility constructor retained for callers that only exercise
    // pre-session validation. A real training call requires the factory overload.
    public IncrementalTrainer(ISessionStorage storage, string adapterDir = "adapters")
        : base(storage)
    {
        _adapterDir = adapterDir;
        _loraConfig = LoraApplicator.ValidateAndCanonicalize(CreateDefaultConfig());
        _dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount;
        _supervisedTrainingStepExecutor = new ProductionSupervisedTrainingStepExecutor();
    }

    public IncrementalTrainer(
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        string adapterDir = "adapters",
        LoraConfig? loraConfig = null)
        : base(storage)
    {
        _loraConfig = LoraApplicator.ValidateAndCanonicalize(
            loraConfig ?? CreateDefaultConfig());
        _sessionFactory = sessionFactory
            ?? throw new ArgumentNullException(nameof(sessionFactory));
        _adapterDir = adapterDir
            ?? throw new ArgumentNullException(nameof(adapterDir));
        _dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount;
        _supervisedTrainingStepExecutor = new ProductionSupervisedTrainingStepExecutor();
    }

    internal IncrementalTrainer(
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority,
        string adapterDir = "adapters",
        LoraConfig? loraConfig = null,
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount)
        : this(
            storage,
            sessionFactory,
            legalTeacherAdmissionAuthority,
            new StableDpoTrainingExecutor(),
            adapterDir,
            loraConfig,
            dpoMaxStepCount)
    {
    }

    internal IncrementalTrainer(
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority,
        IDpoTrainingExecutor dpoTrainingExecutor,
        string adapterDir = "adapters",
        LoraConfig? loraConfig = null,
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount)
        : this(
            storage,
            sessionFactory,
            legalTeacherAdmissionAuthority,
            dpoTrainingExecutor,
            new ProductionSupervisedTrainingStepExecutor(),
            adapterDir,
            loraConfig,
            dpoMaxStepCount)
    {
    }

    internal IncrementalTrainer(
        ISessionStorage storage,
        IIncrementalTrainingSessionFactory sessionFactory,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority,
        IDpoTrainingExecutor dpoTrainingExecutor,
        ISupervisedTrainingStepExecutor supervisedTrainingStepExecutor,
        string adapterDir = "adapters",
        LoraConfig? loraConfig = null,
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount)
        : base(storage)
    {
        _loraConfig = LoraApplicator.ValidateAndCanonicalize(
            loraConfig ?? CreateDefaultConfig());
        _sessionFactory = sessionFactory
            ?? throw new ArgumentNullException(nameof(sessionFactory));
        _legalTeacherAdmissionAuthority = legalTeacherAdmissionAuthority;
        _dpoTrainingExecutor = dpoTrainingExecutor
            ?? throw new ArgumentNullException(nameof(dpoTrainingExecutor));
        var validatedDpoStepCount =
            StableDpoTrainingExecutor.ValidateDpoStepCount(dpoMaxStepCount);
        if (!validatedDpoStepCount.IsSuccess)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dpoMaxStepCount),
                validatedDpoStepCount.ErrorMessage);
        }

        _dpoMaxStepCount = validatedDpoStepCount.Value;
        _supervisedTrainingStepExecutor = supervisedTrainingStepExecutor
            ?? throw new ArgumentNullException(nameof(supervisedTrainingStepExecutor));
        _adapterDir = adapterDir
            ?? throw new ArgumentNullException(nameof(adapterDir));
    }

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => TrainCoreAsync(
            domain,
            examples,
            dpoPairs,
            transaction: null,
            cancellationToken);

    Task<OperationResult<TrainedAdapterArtifact>>
        ITransactionalIncrementalTrainer.TrainTransactionAsync(
            string domain,
            IReadOnlyList<TrainingExample> examples,
            IReadOnlyList<DpoPair> dpoPairs,
            FileDpoPairStore.TransactionHandle transaction,
            CancellationToken cancellationToken)
        => TrainCoreAsync(
            domain,
            examples,
            dpoPairs,
            transaction ?? throw new ArgumentNullException(nameof(transaction)),
            cancellationToken);

    private async Task<OperationResult<TrainedAdapterArtifact>> TrainCoreAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        FileDpoPairStore.TransactionHandle? transaction,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var exampleSnapshotResult = CaptureTrainingExamples(examples);
        if (!exampleSnapshotResult.IsSuccess
            || exampleSnapshotResult.Value is null)
        {
            return exampleSnapshotResult.MapError<TrainedAdapterArtifact>();
        }
        examples = exampleSnapshotResult.Value;
        var validationFailure = ValidateRequest(domain, examples);
        if (validationFailure is not null)
            return validationFailure;

        var dpoSnapshotResult = DpoPairBatchSnapshot.Capture(dpoPairs);
        if (!dpoSnapshotResult.IsSuccess || dpoSnapshotResult.Value is null)
            return dpoSnapshotResult.MapError<TrainedAdapterArtifact>();
        var dpoSnapshot = dpoSnapshotResult.Value;

        if (dpoSnapshot.Any(pair => pair is null
                                    || string.IsNullOrWhiteSpace(pair.Prompt)
                                    || string.IsNullOrWhiteSpace(pair.Chosen)
                                    || string.IsNullOrWhiteSpace(pair.Rejected)
                                    || !StringComparer.Ordinal.Equals(pair.Domain, domain)))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "Every DPO pair must be complete and belong to the training domain.",
                "INVALID_DPO_PAIR");
        }
        ServerOwnedLegalTrainingTeacherAdmissionLease?
            legalTeacherAdmissionLease = null;
        // Проекция исправления устаревшего урока. Пока она пуста, набор уроков
        // и его отпечаток остаются прежними до байта; она заполняется только
        // тогда, когда допущенная пара предпочтений действительно исправляет
        // ответ одного из уроков этого домена.
        ProjectedSupervisedBatch? supervisedCorrection = null;
        if (dpoSnapshot.Count > 0)
        {
            var trainingContent = DpoPairTrainingContentAuthority
                .Project(dpoSnapshot);
            if (!trainingContent.IsSuccess || trainingContent.Value is null)
                return trainingContent.MapError<TrainedAdapterArtifact>();

            var governanceResult = TrainingDataGovernanceAdmission.Validate(dpoSnapshot);
            if (!governanceResult.IsSuccess)
                return governanceResult.MapError<TrainedAdapterArtifact>();

            if (dpoSnapshot.Any(static pair => pair.TeacherProvenance is null))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Typed teacher provenance is required before DPO training.",
                    LegalTrainingTeacherAdmissionAuthority
                        .ProvenanceRequiredErrorCode);
            }

            if (_legalTeacherAdmissionAuthority is null
                || !_legalTeacherAdmissionAuthority.IsAvailable)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Server-owned legal teacher policy is required for DPO training.",
                    LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
            }

            var provenanceAdmission = _legalTeacherAdmissionAuthority
                .ValidateBatch(dpoSnapshot.Select(static pair =>
                    pair.TeacherProvenance));
            if (!provenanceAdmission.IsSuccess)
                return provenanceAdmission.MapError<TrainedAdapterArtifact>();

            if (transaction is null)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Durable DPO transaction authority is required before training.",
                    DpoTransactionRequiredErrorCode);
            }

            if (_dpoTrainingExecutor is null)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Server-owned legal teacher policy is required for DPO training.",
                    LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
            }

            var admissionResult = _legalTeacherAdmissionAuthority
                .AcquireTrainingLease(
                    trainingContent.Value,
                    dpoSnapshot
                        .Select(static pair => pair.TeacherProvenance)
                        .ToArray(),
                    transaction,
                    domain);
            if (!admissionResult.IsSuccess || admissionResult.Value is null)
                return admissionResult.MapError<TrainedAdapterArtifact>();
            legalTeacherAdmissionLease = admissionResult.Value;

            // ЗАЧЕМ: порядок обучения такой — сначала один шаг прямого
            // сравнения предпочтений (DPO — Direct Preference Optimization)
            // уводит модель от отвергнутого текста, затем уроки обучения с
            // учителем (SFT — supervised fine-tuning) закрепляют ответы. Если
            // ответом урока всё ещё стоит тот самый отвергнутый текст, учебный
            // шаг гасит только что полученное исправление. Поэтому замена
            // делается здесь: допуск пары уже состоялся, а происхождение,
            // знаки учебной строки и отпечаток кандидата ещё не построены.
            // Пока пара не допущена, власти менять учебный урок у неё нет.
            if (DpoSupervisedCorrectionProjection.HasCorrectionCandidate(
                    domain,
                    examples,
                    dpoSnapshot))
            {
                // Отпечаток набора до замены вычисляется прежним владельцем
                // отпечатка, поэтому исправленный отпечаток ссылается ровно на
                // то значение, которое получил бы набор без исправления.
                var originalExampleBatch =
                    BuildTrainingExampleBatchSha256(examples);
                if (!originalExampleBatch.IsSuccess
                    || originalExampleBatch.Value is null)
                {
                    return originalExampleBatch
                        .MapError<TrainedAdapterArtifact>();
                }

                // Номера пар берутся из долговечной транзакции: сама пара
                // своего номера не хранит, а происхождение исправленного урока
                // обязано называть именно ту пару, которая его разрешила.
                var identifiedPairs = IdentifyAdmittedTransactionPairs(
                    domain,
                    dpoSnapshot,
                    transaction!);
                if (!identifiedPairs.IsSuccess || identifiedPairs.Value is null)
                    return identifiedPairs.MapError<TrainedAdapterArtifact>();

                // Проекция сравнивает дословно и либо заменяет ровно один
                // ответ, либо отказывается при неоднозначности — до любой
                // мутации модели и до начала мутации транзакции.
                var projection = DpoSupervisedCorrectionProjection.Project(
                    domain,
                    examples,
                    identifiedPairs.Value,
                    originalExampleBatch.Value);
                if (!projection.IsSuccess || projection.Value is null)
                    return projection.MapError<TrainedAdapterArtifact>();

                // Исправленный набор становится единственным учебным входом:
                // дальше и знаки, и отпечаток, и обучение берут только его.
                supervisedCorrection = projection.Value;
                examples = supervisedCorrection.Examples;
            }
        }
        using var admissionLeaseScope = legalTeacherAdmissionLease;

        if (_sessionFactory is null)
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "An incremental training session factory is required.",
                "TRAINING_SESSION_CREATION_FAILED");
        }

        var gateAcquired = false;
        try
        {
            await _trainingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            gateAcquired = true;

            var sessionPermit = AcquireMutationPermit(
                legalTeacherAdmissionLease);
            if (!sessionPermit.IsSuccess || sessionPermit.Value is null)
                return sessionPermit.MapError<TrainedAdapterArtifact>();
            OperationResult<IncrementalTrainingSession> sessionResult;
            using (sessionPermit.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sessionResult = _sessionFactory.Create(domain, cancellationToken);
            }

            var postSessionPermit = AcquireMutationPermit(
                legalTeacherAdmissionLease);
            if (!postSessionPermit.IsSuccess
                || postSessionPermit.Value is null)
            {
                return postSessionPermit.MapError<TrainedAdapterArtifact>();
            }

            IncrementalTrainingSession session;
            using (postSessionPermit.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!sessionResult.IsSuccess || sessionResult.Value is null)
                {
                    return OperationResult<TrainedAdapterArtifact>.Fail(
                        sessionResult.ErrorMessage,
                        sessionResult.ErrorCode);
                }

                session = sessionResult.Value;
            }

            if (RequiresRepositoryOwnedCheckpointPublication(session)
                && transaction is null)
            {
                // WHY: a repository-owned session, including the first
                // authorized cold start, must publish its full-model
                // checkpoint only after transaction.CompleteCandidate accepts
                // the adapter candidate. Otherwise an adapter-only success
                // could leave no step 000001 for the next process to load.
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "A repository-owned training session requires a durable DPO transaction before training.",
                    DpoTransactionRequiredErrorCode);
            }

            RepositoryOwnedDpoDatasetManifestIdentity?
                currentRepositoryOwnedDpoDatasetManifest = null;
            if (RequiresRepositoryOwnedCheckpointPublication(session))
            {
                var currentDpoDatasetManifest =
                    ResolveCurrentRepositoryOwnedDpoDatasetManifest(
                        session,
                        dpoSnapshot);
                if (!currentDpoDatasetManifest.IsSuccess
                    || currentDpoDatasetManifest.Value is null)
                {
                    return currentDpoDatasetManifest
                        .MapError<TrainedAdapterArtifact>();
                }

                currentRepositoryOwnedDpoDatasetManifest =
                    currentDpoDatasetManifest.Value;
            }

            // Без исправления происхождение строится прежним способом по
            // личностям источника самих уроков. С исправлением берётся уже
            // проверенная родословная проекции: в неё входят и прежняя, и
            // производная личность источника каждого заменённого урока, чтобы
            // по происхождению было видно и что учили раньше, и чем заменили.
            var currentManifestResult = supervisedCorrection is null
                ? TrainingSourceProvenance.BuildManifest(
                    examples.Select(static example => example.SourceId))
                : OperationResult<TrainingSourceManifest>.Ok(
                    supervisedCorrection.TrainingSources);
            if (!currentManifestResult.IsSuccess || currentManifestResult.Value is null)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    currentManifestResult.ErrorMessage,
                    currentManifestResult.ErrorCode);
            }

            var parentAdapterCheckpoint = session.ParentAdapterCheckpoint;
            if (parentAdapterCheckpoint?.Provenance is { } parentProvenance)
            {
                var expectedLora = TrainingSourceProvenance.Identify(_loraConfig);
                if (!StringComparer.Ordinal.Equals(
                        parentProvenance.BaseCheckpointSha256,
                        session.BaseCheckpointSha256)
                    || !StringComparer.Ordinal.Equals(
                        parentProvenance.TokenizerSha256,
                        session.TokenizerSha256)
                    || parentProvenance.Model != session.ModelIdentity
                    || !LoraIdentityEquals(parentProvenance.Lora, expectedLora))
                {
                    return OperationResult<TrainedAdapterArtifact>.Fail(
                        "The parent adapter provenance is incompatible with the training session.",
                        "ADAPTER_PROVENANCE_MISMATCH");
                }
            }

            var combinedManifestResult = TrainingSourceProvenance.Combine(
                parentAdapterCheckpoint?.Provenance.TrainingSources,
                currentManifestResult.Value);
            if (!combinedManifestResult.IsSuccess || combinedManifestResult.Value is null)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    combinedManifestResult.ErrorMessage,
                    combinedManifestResult.ErrorCode);
            }
            var combinedManifest = combinedManifestResult.Value;
            var tokenSequences = new List<IReadOnlyList<int>>(examples.Count);
            foreach (var example in examples)
            {
                var tokenizationContinuationPermit = AcquireMutationPermit(
                    legalTeacherAdmissionLease);
                if (!tokenizationContinuationPermit.IsSuccess
                    || tokenizationContinuationPermit.Value is null)
                {
                    return tokenizationContinuationPermit
                        .MapError<TrainedAdapterArtifact>();
                }

                using (tokenizationContinuationPermit.Value)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Формат учебной строки берётся из единственного владельца
                // SupervisedTextSequence: запрос, один перевод строки, ответ,
                // один вызов словаря. Знаки остаются теми же, что и раньше, но
                // теперь тот же формат читают проверка ответа из сохранённого
                // состояния и диагностика остановки.
                var tokenIds = SupervisedTextSequence.EncodeJointLf(
                    session.Tokenizer,
                    example.Prompt,
                    example.Response);
                if (tokenIds.Count < 2)
                {
                    return OperationResult<TrainedAdapterArtifact>.Fail(
                        "A training example must encode to at least two tokens.",
                        "INVALID_TRAINING_EXAMPLE");
                }

                if (tokenIds.Count > session.Model.Config.ContextLen + 1)
                {
                    return OperationResult<TrainedAdapterArtifact>.Fail(
                        "A training example exceeds the model context.",
                        "TRAINING_EXAMPLE_CONTEXT_EXCEEDED");
                }

                tokenSequences.Add(tokenIds);
            }

            FileDpoPairStore.CandidateReservation? transactionCandidate = null;
            if (transaction is not null)
            {
                // Без исправления отпечаток набора остаётся прежним первой
                // версии, поэтому прежние записи кандидатов и их восстановление
                // не меняются. С исправлением считается вторая версия: она
                // связывает те же поля уроков, отпечаток набора до замены и
                // упорядоченные записи замен, и вычисляется после проекции.
                var exactExampleBatch = supervisedCorrection is null
                    ? BuildTrainingExampleBatchSha256(examples)
                    : BuildCorrectedTrainingExampleBatchSha256(
                        examples,
                        supervisedCorrection);
                if (!exactExampleBatch.IsSuccess
                    || exactExampleBatch.Value is null)
                {
                    return exactExampleBatch.MapError<TrainedAdapterArtifact>();
                }
                var preparedCandidate = transaction.PrepareCandidate(
                    domain,
                    new FileDpoPairStore.CandidateProvenanceSeed(
                        combinedManifest,
                        parentAdapterCheckpoint is null
                            ? FileDpoPairStore.NoParentAdapterIdentity
                            : parentAdapterCheckpoint.Provenance.AdapterSha256,
                        session.BaseCheckpointSha256,
                        session.TokenizerSha256,
                        session.ModelIdentity,
                        TrainingSourceProvenance.Identify(_loraConfig),
                        examples.Count,
                        exactExampleBatch.Value));
                if (!preparedCandidate.IsSuccess
                    || preparedCandidate.Value is null)
                {
                    return preparedCandidate.MapError<TrainedAdapterArtifact>();
                }
                transactionCandidate = preparedCandidate.Value;
                if (transactionCandidate.ExistingArtifact is not null)
                {
                    // WHY: an existing durable adapter candidate is a valid
                    // idempotent adapter result, but a repository-owned run
                    // also owes the sealed full-model checkpoint. The retry
                    // may therefore reuse only these already accepted bytes to
                    // finish publication, never a second DPO/supervised pass.
                    if (RequiresRepositoryOwnedCheckpointPublication(session))
                    {
                        var recoveredCheckpoint =
                            PublishRepositoryOwnedCheckpointFromExistingCandidate(
                                session,
                                transactionCandidate,
                                currentRepositoryOwnedDpoDatasetManifest,
                                legalTeacherAdmissionLease);
                        if (!recoveredCheckpoint.IsSuccess)
                            return recoveredCheckpoint
                                .MapError<TrainedAdapterArtifact>();
                    }

                    return OperationResult<TrainedAdapterArtifact>.Ok(
                        transactionCandidate.ExistingArtifact);
                }
            }
            using var transactionCandidateScope = transactionCandidate;

            var trainingCancellationToken = cancellationToken;
            if (transaction is not null)
            {
                // This is the last cancellable boundary. The durable state is
                // changed before the first model mutation; every later step
                // must finish or leave an exact recoverable candidate stage.
                cancellationToken.ThrowIfCancellationRequested();
                var mutation = transaction.BeginMutation(domain);
                if (!mutation.IsSuccess)
                    return mutation.MapError<TrainedAdapterArtifact>();
                trainingCancellationToken = CancellationToken.None;
            }

            IReadOnlyList<Parameter> adapterParameters;
            Dictionary<Parameter, int[]> baseSnapshot;
            Dictionary<Parameter, int[]> adapterSnapshot;
            DpoReferenceSnapshot? dpoReference = null;
            try
            {
                var preparationPermit = AcquireMutationPermit(
                    legalTeacherAdmissionLease);
                if (!preparationPermit.IsSuccess
                    || preparationPermit.Value is null)
                {
                    return preparationPermit.MapError<TrainedAdapterArtifact>();
                }

                using (preparationPermit.Value)
                {
                    trainingCancellationToken.ThrowIfCancellationRequested();
                    var adapters = LoraApplicator.Apply(
                        session.Model,
                        _loraConfig);
                    if (parentAdapterCheckpoint is not null)
                    {
                        try
                        {
                            using var parentStream = parentAdapterCheckpoint.OpenRead();
                            session.Model.LoadAdapterCheckpoint(parentStream);
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                        {
                            return OperationResult<TrainedAdapterArtifact>.Fail(
                                "The parent adapter checkpoint is invalid.",
                                "ADAPTER_CHECKPOINT_INVALID");
                        }
                    }
                    trainingCancellationToken.ThrowIfCancellationRequested();

                    var baseParameters = session.Model.BaseParameters;
                    adapterParameters = adapters
                        .SelectMany(adapter => adapter.AdapterParameters)
                        .Select(entry => entry.Parameter)
                        .Distinct<Parameter>(ReferenceEqualityComparer.Instance)
                        .ToArray();

                    if (adapterParameters.Count == 0)
                    {
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "LoRA application produced no adapter parameters.",
                            "NO_ADAPTER_UPDATE");
                    }

                    baseSnapshot = SnapshotBits(baseParameters);
                    adapterSnapshot = SnapshotBits(adapterParameters);

                    if (legalTeacherAdmissionLease is not null)
                    {
                        var reference = CreateDpoReferenceSnapshot(
                            session.Model,
                            _loraConfig);
                        if (!reference.IsSuccess || reference.Value is null)
                            return reference.MapError<TrainedAdapterArtifact>();
                        dpoReference = reference.Value;
                    }
                }

                if (legalTeacherAdmissionLease is not null)
                {
                    string dpoCheckpointDirectory;
                    var checkpointDirectoryPermit = AcquireMutationPermit(
                        legalTeacherAdmissionLease);
                    if (!checkpointDirectoryPermit.IsSuccess
                        || checkpointDirectoryPermit.Value is null)
                    {
                        return checkpointDirectoryPermit
                            .MapError<TrainedAdapterArtifact>();
                    }
                    try
                    {
                        using (checkpointDirectoryPermit.Value)
                        {
                            dpoCheckpointDirectory = BuildDpoCheckpointDirectory(
                                _adapterDir,
                                domain);
                        }
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                    {
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "The DPO checkpoint path is invalid.",
                            "DPO_EXECUTION_ARTIFACT_INCOMPLETE");
                    }

                    OperationResult<DpoTrainingExecutionArtifact> dpoResult;
                    try
                    {
                        dpoResult = await _dpoTrainingExecutor!.ExecuteAsync(
                            new DpoTrainingExecutionRequest(
                                domain,
                                dpoSnapshot,
                                session.Model,
                                dpoReference!.Model,
                                session.Tokenizer,
                                _loraConfig,
                                dpoCheckpointDirectory,
                                legalTeacherAdmissionLease,
                                _dpoMaxStepCount),
                            trainingCancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                    {
                        if (!DpoReferenceIsUnchanged(dpoReference!))
                        {
                            return OperationResult<TrainedAdapterArtifact>.Fail(
                                "The frozen DPO reference model changed during execution.",
                                "DPO_REFERENCE_MODEL_MUTATION_DETECTED");
                        }
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "DPO execution failed.",
                            "DPO_EXECUTION_FAILED");
                    }

                    if (!DpoReferenceIsUnchanged(dpoReference!))
                    {
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "The frozen DPO reference model changed during execution.",
                            "DPO_REFERENCE_MODEL_MUTATION_DETECTED");
                    }

                    var postDpoPermit = AcquireMutationPermit(
                        legalTeacherAdmissionLease);
                    if (!postDpoPermit.IsSuccess
                        || postDpoPermit.Value is null)
                    {
                        return postDpoPermit.MapError<TrainedAdapterArtifact>();
                    }

                    using (postDpoPermit.Value)
                    {
                        if (!dpoResult.IsSuccess || dpoResult.Value is null)
                            return dpoResult.MapError<TrainedAdapterArtifact>();
                        trainingCancellationToken.ThrowIfCancellationRequested();
                        if (!IsCompleteDpoArtifact(
                                dpoResult.Value,
                                dpoSnapshot.Count,
                                legalTeacherAdmissionLease.Admission.PolicyIdentityRef,
                                _dpoMaxStepCount,
                                dpoCheckpointDirectory))
                        {
                            return OperationResult<TrainedAdapterArtifact>.Fail(
                                "DPO execution did not publish a complete durable artifact.",
                                "DPO_EXECUTION_ARTIFACT_INCOMPLETE");
                        }
                    }
                }

                for (var index = 0; index < examples.Count; index++)
                {
                    var supervisedPermit = AcquireMutationPermit(
                        legalTeacherAdmissionLease);
                    if (!supervisedPermit.IsSuccess
                        || supervisedPermit.Value is null)
                    {
                        return supervisedPermit.MapError<TrainedAdapterArtifact>();
                    }

                    using (supervisedPermit.Value)
                    {
                        trainingCancellationToken.ThrowIfCancellationRequested();
                        var loss = _supervisedTrainingStepExecutor.Execute(
                            session,
                            tokenSequences[index],
                            checked((float)examples[index].Weight),
                            adapterParameters);
                        if (!float.IsFinite(loss))
                        {
                            return OperationResult<TrainedAdapterArtifact>.Fail(
                                "Supervised adapter training produced a non-finite loss.",
                                "SUPERVISED_ADAPTER_TRAINING_FAILED");
                        }
                        trainingCancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Supervised adapter training failed.",
                    "SUPERVISED_ADAPTER_TRAINING_FAILED");
            }

            if (!MatchesSnapshot(session.Model.BaseParameters, baseSnapshot))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "A frozen base parameter changed during adapter training.",
                    "BASE_PARAMETER_MUTATION_DETECTED");
            }

            if (!AllFinite(adapterParameters))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Adapter training produced a non-finite parameter.",
                    "SUPERVISED_ADAPTER_TRAINING_FAILED");
            }

            if (MatchesSnapshot(adapterParameters, adapterSnapshot))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "Adapter parameters did not change during training.",
                    "NO_ADAPTER_UPDATE");
            }

            var finalAdapterPermit = AcquireMutationPermit(
                legalTeacherAdmissionLease);
            if (!finalAdapterPermit.IsSuccess
                || finalAdapterPermit.Value is null)
            {
                return finalAdapterPermit.MapError<TrainedAdapterArtifact>();
            }
            using var finalAdapterPermitScope = finalAdapterPermit.Value;

            string adapterPath;
            try
            {
                trainingCancellationToken.ThrowIfCancellationRequested();
                if (transactionCandidate is null)
                {
                    var fullDirectory = Path.TrimEndingDirectorySeparator(
                        Path.GetFullPath(_adapterDir));
                    Directory.CreateDirectory(fullDirectory);
                    adapterPath = Path.GetFullPath(
                        Path.Combine(fullDirectory, $"{domain}.lora"));
                    if (!StringComparer.OrdinalIgnoreCase.Equals(
                            Path.GetDirectoryName(adapterPath),
                            fullDirectory))
                    {
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "The adapter path is not contained by the adapter directory.",
                            "ADAPTER_CHECKPOINT_SAVE_FAILED");
                    }
                }
                else
                {
                    adapterPath = Path.GetFullPath(
                        transactionCandidate.StagingPath);
                }

                trainingCancellationToken.ThrowIfCancellationRequested();
                if (transactionCandidate is null)
                {
                    session.Model.SaveAdapterCheckpoint(adapterPath);
                    if (!File.Exists(adapterPath)
                        || new FileInfo(adapterPath).Length <= 0)
                    {
                        return OperationResult<TrainedAdapterArtifact>.Fail(
                            "The adapter checkpoint was not completed.",
                            "ADAPTER_CHECKPOINT_INCOMPLETE");
                    }
                }
                else
                {
                    // В транзакции промежуточный файл кандидата обученных
                    // весов принадлежит защищённому хранилищу FileDpoPairStore.
                    // Тренер IncrementalTrainer передаёт только модель в
                    // CandidateReservation.SaveAdapterCheckpoint; создание
                    // файла, доступ только владельцу, проверка, что открытый
                    // файловый дескриптор ведёт к ожидаемому файлу, и
                    // принудительная запись на диск остаются за арендой права
                    // мутировать этот домен.
                    var savedCandidate =
                        transactionCandidate.SaveAdapterCheckpoint(session.Model);
                    if (!savedCandidate.IsSuccess)
                        return savedCandidate.MapError<TrainedAdapterArtifact>();
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "The adapter checkpoint could not be saved.",
                    "ADAPTER_CHECKPOINT_SAVE_FAILED");
            }

            if (transaction is not null)
            {
                var completed = transaction.CompleteCandidate(domain);
                if (!completed.IsSuccess || completed.Value is null)
                    return completed.MapError<TrainedAdapterArtifact>();

                // WHY: the durable adapter candidate is the existing acceptance
                // boundary for transactional training. The repository-owned
                // full-model checkpoint is published only after that boundary
                // succeeds, so rejected/no-op/failing updates cannot create a
                // child checkpoint.
                var repositoryCheckpoint =
                    PublishRepositoryOwnedCheckpointAfterAcceptedUpdate(
                        session,
                        currentRepositoryOwnedDpoDatasetManifest,
                        completed.Value.Provenance.TrainingSources);
                if (!repositoryCheckpoint.IsSuccess)
                    return repositoryCheckpoint.MapError<TrainedAdapterArtifact>();

                return completed;
            }

            using var adapterStream = OpenAdapterRead(adapterPath);
            var adapterSha256 = await TrainingSourceProvenance
                .Sha256Async(adapterStream, trainingCancellationToken)
                .ConfigureAwait(false);
            adapterStream.Position = 0;
            LlmModel.ValidateAdapterCheckpointArtifact(adapterStream);
            var artifact = new TrainedAdapterArtifact(
                adapterPath,
                new ModelAdapterProvenance(
                    combinedManifest,
                    adapterSha256,
                    session.BaseCheckpointSha256,
                    session.TokenizerSha256,
                    session.ModelIdentity,
                    TrainingSourceProvenance.Identify(_loraConfig)));

            return OperationResult<TrainedAdapterArtifact>.Ok(
                artifact);
        }
        finally
        {
            if (gateAcquired)
                _trainingGate.Release();
        }
    }

    private static LoraConfig CreateDefaultConfig()
        => new()
        {
            Rank = 8,
            Alpha = 16f,
            Dropout = 0.05f,
            TargetModules = ["q_proj", "v_proj"],
        };

    private static OperationResult<IDisposable> AcquireMutationPermit(
        ServerOwnedLegalTrainingTeacherAdmissionLease? admissionLease)
        => admissionLease is null
            ? OperationResult<IDisposable>.Ok(NoopMutationPermit.Instance)
            : admissionLease.AcquireMutationPermit();

    private static bool RequiresRepositoryOwnedCheckpointPublication(
        IncrementalTrainingSession session)
        => session.CheckpointStart?.RepositoryStartRequest is not null;

    private OperationResult<bool>
        PublishRepositoryOwnedCheckpointFromExistingCandidate(
            IncrementalTrainingSession session,
            FileDpoPairStore.CandidateReservation transactionCandidate,
            RepositoryOwnedDpoDatasetManifestIdentity?
                currentDpoDatasetManifest,
            ServerOwnedLegalTrainingTeacherAdmissionLease? admissionLease)
    {
        // WHY: crash recovery must materialize the exact already accepted
        // adapter into the compatible session before the repository-owned
        // publisher serializes a missing full checkpoint.
        var existingArtifact = transactionCandidate.ExistingArtifact;
        if (existingArtifact is null)
            return OperationResult<bool>.Fail(
                "A repository-owned retry needs an existing durable adapter candidate before checkpoint publication.",
                RepositoryOwnedIncrementalTrainingCheckpointPublisher
                    .InvalidErrorCode);

        var mutationPermit = AcquireMutationPermit(admissionLease);
        if (!mutationPermit.IsSuccess || mutationPermit.Value is null)
            return mutationPermit.MapError<bool>();

        try
        {
            using (mutationPermit.Value)
            {
                // WHY: the full checkpoint format serializes attached LoRA
                // slots, so the retry first recreates the same topology and
                // then overwrites it from the validated durable candidate.
                _ = LoraApplicator.Apply(session.Model, _loraConfig);
                var loaded = transactionCandidate.LoadExistingAdapterCheckpoint(
                    session.Model);
                if (!loaded.IsSuccess)
                    return loaded.MapError<bool>();
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "The recovered adapter candidate could not be restored into the repository-owned training session.",
                RepositoryOwnedIncrementalTrainingCheckpointPublisher
                    .InvalidErrorCode);
        }

        return PublishRepositoryOwnedCheckpointAfterAcceptedUpdate(
            session,
            currentDpoDatasetManifest,
            existingArtifact.Provenance.TrainingSources);
    }

    private static OperationResult<bool>
        PublishRepositoryOwnedCheckpointAfterAcceptedUpdate(
            IncrementalTrainingSession session,
            RepositoryOwnedDpoDatasetManifestIdentity?
                currentDpoDatasetManifest,
            TrainingSourceManifest trainingSources)
    {
        if (!RequiresRepositoryOwnedCheckpointPublication(session))
        {
            // WHY: legacy retained-base sessions still have only the historical
            // adapter publication contract; the repository-owned child
            // checkpoint obligation begins when a typed repository-owned start
            // request is present, including the first authorized cold start.
            return OperationResult<bool>.Ok(true);
        }

        if (currentDpoDatasetManifest is null)
        {
            return CurrentRepositoryOwnedDpoDatasetFailure()
                .MapError<bool>();
        }

        var parentCheckpoint = session.ResumeParentCheckpoint;
        if (session.CheckpointStart?.Source
                == IncrementalTrainingCheckpointStartSource
                    .RepositoryOwnedModelCheckpoint
            && parentCheckpoint is null)
        {
            return OperationResult<bool>.Fail(
                "A repository-owned resumed training session is required before publishing the next checkpoint.",
                RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode);
        }

        var published =
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.PublishNext(
                session,
                currentDpoDatasetManifest,
                DateTimeOffset.UtcNow,
                BuildRepositoryOwnedCheckpointProvenanceRefs(trainingSources));

        // WHY: once a session resumed from the repository-owned checkpoint
        // tree, adapter success without the sealed child full-model checkpoint
        // would be a false success. Publication errors therefore block the
        // training result instead of being logged and swallowed.
        if (!published.IsSuccess || published.Value is null)
        {
            return !published.IsSuccess
                ? published.MapError<bool>()
                : OperationResult<bool>.Fail(
                    "The repository-owned child checkpoint was not published.",
                    RepositoryOwnedIncrementalTrainingCheckpointPublisher
                        .InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>
        ResolveCurrentRepositoryOwnedDpoDatasetManifest(
            IncrementalTrainingSession session,
            IReadOnlyList<DpoPair> currentAcceptedDpoPairs)
    {
        var startRequest = session.CheckpointStart?.RepositoryStartRequest;
        var currentDpoDatasetManifest = startRequest?.CurrentDpoDatasetManifest;
        if (startRequest is null || currentDpoDatasetManifest is null)
            return CurrentRepositoryOwnedDpoDatasetFailure();

        var currentAddress = ResolveCurrentRepositoryOwnedDpoDatasetAddress(
            startRequest,
            currentDpoDatasetManifest);
        if (!currentAddress.IsSuccess || currentAddress.Value is null)
            return currentAddress.MapError<RepositoryOwnedDpoDatasetManifestIdentity>();

        if (!RepositoryOwnedDpoDatasetAddressMatchesStartRequest(
                startRequest,
                currentAddress.Value))
        {
            return CurrentRepositoryOwnedDpoDatasetMismatchFailure();
        }

        // WHY: public DPO store reads hide transaction-held rows, but the
        // accepted update that is currently being trained may still hold those
        // rows. The manifest-integrity opener reopens the canonical sealed
        // files and includes held rows, so provenance is bound to the exact
        // accepted in-memory DPO batch before any training or child publication.
        var reopened = RepositoryOwnedDpoDatasetManifestSealer
            .OpenSealedForManifestIntegrity(currentAddress.Value);
        if (!reopened.IsSuccess || reopened.Value is null)
            return reopened.MapError<RepositoryOwnedDpoDatasetManifestIdentity>();

        var reopenedManifest = reopened.Value.Manifest;
        var reopenedIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            currentAddress.Value.ManifestRelativePath,
            reopenedManifest.ManifestSha256);
        if (reopenedIdentity != currentDpoDatasetManifest
            || !StringComparer.Ordinal.Equals(
                reopenedManifest.Domain,
                currentAddress.Value.DomainSlug)
            || !StringComparer.Ordinal.Equals(
                reopenedManifest.DesignPattern,
                currentAddress.Value.DesignPatternSlug)
            || !StringComparer.Ordinal.Equals(
                reopenedManifest.DatasetVersion,
                currentAddress.Value.DatasetVersion)
            || !DpoPairsMatchInOrder(
                reopened.Value.Pairs,
                currentAcceptedDpoPairs))
        {
            return CurrentRepositoryOwnedDpoDatasetMismatchFailure();
        }

        return OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>.Ok(
            reopenedIdentity);
    }

    private static OperationResult<RepositoryOwnedDpoDatasetAddress>
        ResolveCurrentRepositoryOwnedDpoDatasetAddress(
            RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
            RepositoryOwnedDpoDatasetManifestIdentity currentDpoDatasetManifest)
    {
        try
        {
            var dpoRoot = RepositoryOwnedTrainingArtifactLayout
                .ResolveDpoArtifactRoot(startRequest.RepositoryRoot);
            if (!dpoRoot.IsSuccess || dpoRoot.Value is null)
                return dpoRoot.MapError<RepositoryOwnedDpoDatasetAddress>();

            var manifestPath = Path.GetFullPath(Path.Combine(
                startRequest.RepositoryRoot,
                currentDpoDatasetManifest.RepositoryRelativeManifestPath));
            var datasetRoot = Path.GetDirectoryName(manifestPath);
            if (string.IsNullOrWhiteSpace(datasetRoot))
                return CurrentRepositoryOwnedDpoDatasetAddressFailure();

            var pairStoreRoot = Path.Combine(
                datasetRoot,
                RepositoryOwnedTrainingArtifactLayout.PairStoreDirectoryName);
            var address = RepositoryOwnedTrainingArtifactLayout
                .ResolveDpoDatasetAddressFromPairStoreRoot(
                    dpoRoot.Value,
                    pairStoreRoot);
            if (!address.IsSuccess || address.Value is null)
                return address.MapError<RepositoryOwnedDpoDatasetAddress>();

            return StringComparer.Ordinal.Equals(
                    address.Value.ManifestRelativePath,
                    currentDpoDatasetManifest.RepositoryRelativeManifestPath)
                ? OperationResult<RepositoryOwnedDpoDatasetAddress>.Ok(
                    address.Value)
                : CurrentRepositoryOwnedDpoDatasetAddressFailure();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<RepositoryOwnedDpoDatasetAddress>.Fail(
                "The sealed repository-owned DPO dataset identity for the current update could not be reopened.",
                RepositoryOwnedDpoDatasetManifestSealer.UnavailableErrorCode);
        }
    }

    private static bool RepositoryOwnedDpoDatasetAddressMatchesStartRequest(
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
        RepositoryOwnedDpoDatasetAddress currentAddress)
        => PathsEqual(startRequest.RepositoryRoot, currentAddress.RepositoryRoot)
           && StringComparer.Ordinal.Equals(
               startRequest.DomainSlug,
               currentAddress.DomainSlug)
           && StringComparer.Ordinal.Equals(
               startRequest.DesignPatternSlug,
               currentAddress.DesignPatternSlug);

    private static bool DpoPairsMatchInOrder(
        IReadOnlyList<DpoPair>? sealedRows,
        IReadOnlyList<DpoPair>? currentRows)
    {
        if (sealedRows is null
            || currentRows is null
            || sealedRows.Count == 0
            || sealedRows.Count != currentRows.Count)
        {
            return false;
        }

        for (var index = 0; index < sealedRows.Count; index++)
        {
            if (!DpoPairRowsEqual(sealedRows[index], currentRows[index]))
                return false;
        }

        return true;
    }

    private static bool DpoPairRowsEqual(DpoPair left, DpoPair right)
        => left is not null
           && right is not null
           && StringComparer.Ordinal.Equals(left.Prompt, right.Prompt)
           && StringComparer.Ordinal.Equals(left.Chosen, right.Chosen)
           && StringComparer.Ordinal.Equals(left.Rejected, right.Rejected)
           && StringComparer.Ordinal.Equals(left.Domain, right.Domain)
           && left.CapturedAt.Equals(right.CapturedAt)
           && EqualityComparer<TrainingTeacherProvenance?>.Default.Equals(
               left.TeacherProvenance,
               right.TeacherProvenance)
           && EqualityComparer<TrainingDataGovernanceDecision?>.Default.Equals(
               left.GovernanceDecision,
               right.GovernanceDecision)
           && EqualityComparer<DpoPairSideAuthorship?>.Default.Equals(
               left.ChosenAuthorship,
               right.ChosenAuthorship)
           && EqualityComparer<DpoPairSideAuthorship?>.Default.Equals(
               left.RejectedAuthorship,
               right.RejectedAuthorship);

    private static OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>
        CurrentRepositoryOwnedDpoDatasetFailure()
        // WHY: a repository-owned child checkpoint is a full-model lineage
        // claim. It may name only the sealed DPO manifest supplied for the
        // current update; falling back to the parent checkpoint's dataset would
        // publish false provenance for the new model bytes.
        => OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>.Fail(
            "The sealed repository-owned DPO dataset for the current update is required before publishing a child checkpoint.",
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode);

    private static OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>
        CurrentRepositoryOwnedDpoDatasetMismatchFailure()
        // WHY: a child checkpoint manifest must not bind stale sealed DPO rows
        // to a different accepted update. Any domain/address/hash/pair mismatch
        // is therefore a training-blocking provenance failure, not a warning.
        => OperationResult<RepositoryOwnedDpoDatasetManifestIdentity>.Fail(
            "The sealed repository-owned DPO dataset for the current update does not match the accepted DPO pairs.",
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode);

    private static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CurrentRepositoryOwnedDpoDatasetAddressFailure()
        => OperationResult<RepositoryOwnedDpoDatasetAddress>.Fail(
            "The sealed repository-owned DPO dataset identity for the current update is invalid.",
            RepositoryOwnedIncrementalTrainingCheckpointPublisher.InvalidErrorCode);

    private static IReadOnlyList<string> BuildRepositoryOwnedCheckpointProvenanceRefs(
        TrainingSourceManifest trainingSources)
        =>
        [
            "incremental-trainer-accepted-update",
            $"training-source-manifest-sha256:{trainingSources.Sha256}",
        ];

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

    private static OperationResult<TrainedAdapterArtifact>? ValidateRequest(
        string domain,
        IReadOnlyList<TrainingExample>? examples)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return OperationResult<TrainedAdapterArtifact>.Fail("Domain cannot be empty.", "EMPTY_DOMAIN");
        if (!IsSafeFileNameComponent(domain))
        {
            return OperationResult<TrainedAdapterArtifact>.Fail(
                "Domain must be a safe single filename component.",
                "INVALID_TRAINING_EXAMPLE");
        }

        if (examples is null || examples.Count == 0)
            return OperationResult<TrainedAdapterArtifact>.Fail("No training examples.", "EMPTY_EXAMPLES");

        foreach (var example in examples)
        {
            if (example is null
                || string.IsNullOrWhiteSpace(example.Prompt)
                || string.IsNullOrWhiteSpace(example.Response)
                || !StringComparer.Ordinal.Equals(example.Domain, domain)
                || !double.IsFinite(example.Weight)
                || example.Weight <= 0d
                || example.Weight > float.MaxValue)
            {
                return OperationResult<TrainedAdapterArtifact>.Fail(
                    "A training example is invalid.",
                    "INVALID_TRAINING_EXAMPLE");
            }
        }

        return null;
    }

    private static OperationResult<IReadOnlyList<TrainingExample>>
        CaptureTrainingExamples(
            IEnumerable<TrainingExample>? source)
    {
        if (source is null)
        {
            return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                "Training examples are required.",
                "EMPTY_EXAMPLES");
        }
        try
        {
            var snapshot = new List<TrainingExample>();
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (snapshot.Count == MaximumTransactionalTrainingExampleCount)
                {
                    return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                        "The supervised training batch exceeds the safe snapshot bound.",
                        "TRAINING_EXAMPLE_BATCH_LIMIT_EXCEEDED");
                }
                var value = enumerator.Current;
                snapshot.Add(value is null
                    ? null!
                    : new TrainingExample(
                        value.Prompt,
                        value.Response,
                        value.Domain,
                        value.Weight,
                        value.CapturedAt,
                        value.SourceId));
            }
            return OperationResult<IReadOnlyList<TrainingExample>>.Ok(
                Array.AsReadOnly(snapshot.ToArray()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<TrainingExample>>.Fail(
                "Training examples could not be snapshotted safely.",
                "TRAINING_EXAMPLE_ENUMERATION_FAILED");
        }
    }

    private static string BuildDpoCheckpointDirectory(
        string adapterDirectory,
        string domain)
    {
        var fullAdapterDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(adapterDirectory));
        Directory.CreateDirectory(fullAdapterDirectory);
        EnsureExistingDpoPathSegmentsAreNotReparsePoints(
            fullAdapterDirectory,
            fullAdapterDirectory);
        var domainDpoDirectory = Path.GetFullPath(Path.Combine(
            fullAdapterDirectory,
            $"{domain}.dpo"));
        if (!StringComparer.OrdinalIgnoreCase.Equals(
                Path.GetDirectoryName(domainDpoDirectory),
                fullAdapterDirectory))
        {
            throw new InvalidDataException("DPO_CHECKPOINT_PATH_INVALID");
        }
        EnsureExistingDpoPathSegmentsAreNotReparsePoints(
            fullAdapterDirectory,
            domainDpoDirectory);
        Directory.CreateDirectory(domainDpoDirectory);
        EnsureExistingDpoPathSegmentsAreNotReparsePoints(
            fullAdapterDirectory,
            domainDpoDirectory);

        var runDirectory = Path.GetFullPath(Path.Combine(
            domainDpoDirectory,
            $"run-{Guid.NewGuid():N}"));
        if (!StringComparer.OrdinalIgnoreCase.Equals(
                Path.GetDirectoryName(runDirectory),
                domainDpoDirectory))
        {
            throw new InvalidDataException("DPO_CHECKPOINT_PATH_INVALID");
        }
        EnsureExistingDpoPathSegmentsAreNotReparsePoints(
            fullAdapterDirectory,
            runDirectory);
        Directory.CreateDirectory(runDirectory);
        EnsureExistingDpoPathSegmentsAreNotReparsePoints(
            fullAdapterDirectory,
            runDirectory);

        return runDirectory;
    }

    private static void EnsureExistingDpoPathSegmentsAreNotReparsePoints(
        string fullAdapterDirectory,
        string fullPath)
    {
        var normalizedAdapterDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(fullAdapterDirectory));
        var current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(fullPath));
        while (true)
        {
            try
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("DPO_CHECKPOINT_PATH_REPARSE_POINT");
            }
            catch (Exception exception)
                when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                // A missing suffix is allowed; every existing segment is rechecked after creation.
            }

            if (PathsEqual(current, normalizedAdapterDirectory))
                return;

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent)
                || PathsEqual(parent, current))
            {
                throw new InvalidDataException("DPO_CHECKPOINT_PATH_INVALID");
            }

            current = Path.TrimEndingDirectorySeparator(parent);
        }
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool IsCompleteDpoArtifact(
        DpoTrainingExecutionArtifact artifact,
        int expectedPairCount,
        string expectedPolicyIdentityRef,
        int maximumDpoStepCount,
        string expectedDirectory)
    {
        if (artifact.ProcessedPairCount != expectedPairCount
            || artifact.DpoStepCount < LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount
            || artifact.DpoStepCount > maximumDpoStepCount
            || !StringComparer.Ordinal.Equals(
                artifact.LegalTeacherPolicyIdentityRef,
                expectedPolicyIdentityRef))
        {
            return false;
        }

        try
        {
            var expected = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(expectedDirectory));
            var policy = TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                artifact.PolicyCheckpointPath);
            var metrics = TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                artifact.MetricsPath);
            return new FileInfo(policy).Length > 0
                   && new FileInfo(metrics).Length > 0
                   && StringComparer.OrdinalIgnoreCase.Equals(
                       Path.GetDirectoryName(policy),
                       expected)
                   && StringComparer.OrdinalIgnoreCase.Equals(
                       Path.GetDirectoryName(metrics),
                       expected);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static FileStream OpenAdapterRead(string path)
    {
        const long maximumAdapterBytes = 2L * 1024 * 1024 * 1024;
        var stream = new FileStream(
            TrainingSourceProvenance.RequireCanonicalRegularFilePath(path),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);
        if (stream.Length <= 0 || stream.Length > maximumAdapterBytes)
        {
            stream.Dispose();
            throw new InvalidDataException("ADAPTER_CHECKPOINT_SIZE_INVALID");
        }
        return stream;
    }

    private static bool IsSafeFileNameComponent(string value)
        => StringComparer.Ordinal.Equals(value, value.Trim())
           && value is not "." and not ".."
           && value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
           && !value.Contains(Path.DirectorySeparatorChar)
           && !value.Contains(Path.AltDirectorySeparatorChar)
           && !value.EndsWith(".", StringComparison.Ordinal)
           && !value.EndsWith(' ')
           && !IsWindowsDeviceName(value);

    private static bool IsWindowsDeviceName(string value)
    {
        var stem = value.Split('.', 2)[0];
        if (stem.Equals("CON", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("AUX", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (stem.Length != 4)
            return false;

        return (stem.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                || stem.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
               && stem[3] is >= '1' and <= '9';
    }

    private sealed class NoopMutationPermit : IDisposable
    {
        internal static readonly NoopMutationPermit Instance = new();

        public void Dispose()
        {
        }
    }

    private static Dictionary<Parameter, int[]> SnapshotBits(
        IEnumerable<Parameter> parameters)
    {
        var snapshot = new Dictionary<Parameter, int[]>(
            ReferenceEqualityComparer.Instance);
        foreach (var parameter in parameters)
        {
            snapshot.Add(
                parameter,
                parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }

        return snapshot;
    }

    private static OperationResult<string> BuildTrainingExampleBatchSha256(
        IReadOnlyList<TrainingExample> examples)
    {
        if (examples.Count is <= 0 or > MaximumTransactionalTrainingExampleCount)
        {
            return OperationResult<string>.Fail(
                "The exact supervised training batch exceeds the durable transaction bound.",
                "DPO_TRANSACTION_SUPERVISED_BATCH_LIMIT_EXCEEDED");
        }
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long utf8Bytes = 0;
            AppendInt32(hash, examples.Count);
            if (!AppendBoundedString(
                    hash,
                    "xiigen.dpo-supervised-example-batch.v1",
                    ref utf8Bytes))
            {
                return InvalidTransactionalExampleBatch();
            }
            foreach (var example in examples)
            {
                if (example is null
                    || !AppendBoundedString(
                        hash,
                        example.Prompt,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.Response,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.Domain,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.SourceId,
                        ref utf8Bytes))
                {
                    return InvalidTransactionalExampleBatch();
                }
                AppendInt64(
                    hash,
                    BitConverter.DoubleToInt64Bits(example.Weight));
                AppendInt64(hash, example.CapturedAt.Ticks);
                AppendInt64(hash, example.CapturedAt.Offset.Ticks);
            }
            return OperationResult<string>.Ok(
                Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidTransactionalExampleBatch();
        }
    }

    // Отпечаток исправленного учебного набора, вторая версия. Он связывает те
    // же поля уроков, что и первая версия, плюс отпечаток набора до замены и
    // упорядоченные записи замен, поэтому исправленный набор нельзя принять за
    // неисправленный, а повторная попытка с теми же данными даёт то же
    // значение и попадает в того же записанного кандидата.
    private static OperationResult<string>
        BuildCorrectedTrainingExampleBatchSha256(
            IReadOnlyList<TrainingExample> examples,
            ProjectedSupervisedBatch correction)
    {
        // Границы набора те же, что у первой версии отпечатка.
        if (examples.Count is <= 0 or > MaximumTransactionalTrainingExampleCount)
        {
            return OperationResult<string>.Fail(
                "The exact supervised training batch exceeds the durable transaction bound.",
                "DPO_TRANSACTION_SUPERVISED_BATCH_LIMIT_EXCEEDED");
        }
        try
        {
            // Отпечаток считается потоком по тем же правилам, что и первый.
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            // Общий счёт байтов текста держит ту же границу размера.
            long utf8Bytes = 0;
            // Число уроков отделяет короткий набор от начала длинного.
            AppendInt32(hash, examples.Count);
            // Имя второй версии отделяет исправленный набор от прежнего.
            if (!AppendBoundedString(
                    hash,
                    "xiigen.dpo-supervised-example-batch.v2",
                    ref utf8Bytes))
            {
                return InvalidTransactionalExampleBatch();
            }
            foreach (var example in examples)
            {
                // Поля урока дописываются в том же порядке, что в первой версии.
                if (example is null
                    || !AppendBoundedString(
                        hash,
                        example.Prompt,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.Response,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.Domain,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        example.SourceId,
                        ref utf8Bytes))
                {
                    return InvalidTransactionalExampleBatch();
                }
                // Вес урока.
                AppendInt64(
                    hash,
                    BitConverter.DoubleToInt64Bits(example.Weight));
                // Время снятия урока и его смещение от всемирного времени.
                AppendInt64(hash, example.CapturedAt.Ticks);
                AppendInt64(hash, example.CapturedAt.Offset.Ticks);
            }

            // Отпечаток набора до замены: по нему восстановление сверяет, что
            // исправление применено к тому же исходному набору уроков.
            if (!AppendBoundedString(
                    hash,
                    correction.OriginalBatchSha256,
                    ref utf8Bytes))
            {
                return InvalidTransactionalExampleBatch();
            }
            // Число замен отделяет одну запись от начала нескольких.
            AppendInt32(hash, correction.Corrections.Count);
            foreach (var binding in correction.Corrections)
            {
                // Позиция заменённого урока в исходном порядке.
                AppendInt32(hash, binding.ExampleOrdinal);
                // Обе личности источника, номер пары, отпечаток набора пар и
                // отпечатки прежнего и нового текста ответа.
                if (!AppendBoundedString(
                        hash,
                        binding.OriginalSourceId,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        binding.CorrectedSourceId,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        binding.TransactionPairId,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        binding.PairBatchSha256,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        binding.OriginalResponseSha256,
                        ref utf8Bytes)
                    || !AppendBoundedString(
                        hash,
                        binding.CorrectedResponseSha256,
                        ref utf8Bytes))
                {
                    return InvalidTransactionalExampleBatch();
                }
            }

            // Отпечаток приводится к строчным шестнадцатеричным знакам.
            return OperationResult<string>.Ok(
                Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidTransactionalExampleBatch();
        }
    }

    // Номера допущенных пар читаются из долговечной транзакции и сверяются с
    // допущенным набором в исходном порядке. Выбор «по времени» или «по порядку
    // случайной коллекции» запрещён, поэтому любое расхождение числа пар,
    // порядка или содержимого — отказ, а не догадка.
    private static OperationResult<IReadOnlyList<DpoSupervisedCorrectionCandidatePair>>
        IdentifyAdmittedTransactionPairs(
            string domain,
            IReadOnlyList<DpoPair> admittedPairs,
            FileDpoPairStore.TransactionHandle transaction)
    {
        // Снимок транзакции — единственный источник номеров пар.
        var snapshot = transaction.Read();
        if (!snapshot.IsSuccess || snapshot.Value is null)
        {
            return snapshot
                .MapError<IReadOnlyList<DpoSupervisedCorrectionCandidatePair>>();
        }

        // Берутся только пары этого домена и строго в исходном порядке.
        var domainRows = snapshot.Value.Pairs
            .Where(row => row.Pair is not null
                          && StringComparer.Ordinal.Equals(
                              row.Pair.Domain,
                              domain))
            .ToArray();
        // Число пар домена обязано совпасть с числом допущенных пар.
        if (domainRows.Length != admittedPairs.Count)
            return AdmittedTransactionPairMismatch();

        // Результат собирается позиция к позиции.
        var identified =
            new List<DpoSupervisedCorrectionCandidatePair>(domainRows.Length);
        for (var index = 0; index < domainRows.Length; index++)
        {
            // Номер пары обязан быть непустым: без него происхождение пусто.
            if (string.IsNullOrWhiteSpace(domainRows[index].PairId))
                return AdmittedTransactionPairMismatch();
            // Содержимое записанной пары обязано дословно совпасть с допущенной.
            if (!DpoPairRowsEqual(domainRows[index].Pair, admittedPairs[index]))
                return AdmittedTransactionPairMismatch();
            // Пара вместе со своим номером идёт в проекцию.
            identified.Add(new DpoSupervisedCorrectionCandidatePair(
                domainRows[index].PairId,
                domainRows[index].Pair));
        }

        // Список закрывается только для чтения: подменить его позже нельзя.
        return OperationResult<IReadOnlyList<DpoSupervisedCorrectionCandidatePair>>
            .Ok(Array.AsReadOnly(identified.ToArray()));
    }

    // Отказ сверки допущенных пар с записью транзакции.
    private static OperationResult<IReadOnlyList<DpoSupervisedCorrectionCandidatePair>>
        AdmittedTransactionPairMismatch()
        => OperationResult<IReadOnlyList<DpoSupervisedCorrectionCandidatePair>>
            .Fail(
                "Допущенные пары предпочтений не совпадают с записью долговечной транзакции в исходном порядке.",
                DpoSupervisedCorrectionProjection.PairIdentityRequiredErrorCode);

    private static bool AppendBoundedString(
        IncrementalHash hash,
        string? value,
        ref long aggregateUtf8Bytes)
    {
        if (value is null)
        {
            AppendInt32(hash, -1);
            return true;
        }
        var length = Encoding.UTF8.GetByteCount(value);
        if (length > MaximumTransactionalTrainingExampleUtf8Bytes
            - aggregateUtf8Bytes)
        {
            return false;
        }
        aggregateUtf8Bytes += length;
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
        return true;
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static void AppendInt64(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static OperationResult<string> InvalidTransactionalExampleBatch()
        => OperationResult<string>.Fail(
            "The exact supervised training batch cannot be bound to the durable DPO transaction.",
            "DPO_TRANSACTION_SUPERVISED_BATCH_INVALID");

    private static OperationResult<DpoReferenceSnapshot>
        CreateDpoReferenceSnapshot(
            LlmModel policy,
            LoraConfig loraConfig)
    {
        try
        {
            var reference = new LlmModel(policy.Config);
            _ = LoraApplicator.Apply(reference, loraConfig);
            var policyEntries = policy.GetCheckpointParameterEntries();
            var referenceEntries = reference.GetCheckpointParameterEntries();
            if (policyEntries.Count != referenceEntries.Count)
                return InvalidDpoReference();

            for (var index = 0; index < policyEntries.Count; index++)
            {
                var source = policyEntries[index];
                var destination = referenceEntries[index];
                if (ReferenceEquals(source.Parameter, destination.Parameter)
                    || ReferenceEquals(
                        source.Parameter.Value,
                        destination.Parameter.Value)
                    || ReferenceEquals(
                        source.Parameter.Value.Data,
                        destination.Parameter.Value.Data)
                    || !StringComparer.Ordinal.Equals(source.Key, destination.Key)
                    || !source.Parameter.Value.Shape.Dimensions.SequenceEqual(
                        destination.Parameter.Value.Shape.Dimensions)
                    || source.Parameter.Value.Data.Length
                        != destination.Parameter.Value.Data.Length)
                {
                    return InvalidDpoReference();
                }

                source.Parameter.Value.Data.CopyTo(
                    destination.Parameter.Value.Data,
                    0);
                destination.Parameter.Value.RequiresGrad = false;
                for (var valueIndex = 0;
                     valueIndex < source.Parameter.Value.Data.Length;
                     valueIndex++)
                {
                    if (BitConverter.SingleToInt32Bits(
                            source.Parameter.Value.Data[valueIndex])
                        != BitConverter.SingleToInt32Bits(
                            destination.Parameter.Value.Data[valueIndex]))
                    {
                        return InvalidDpoReference();
                    }
                }
            }

            if (referenceEntries.Any(static entry =>
                    entry.Parameter.Value.RequiresGrad))
            {
                return InvalidDpoReference();
            }
            return OperationResult<DpoReferenceSnapshot>.Ok(
                new DpoReferenceSnapshot(
                    reference,
                    FingerprintNamedParameters(referenceEntries)));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidDpoReference();
        }
    }

    private static bool DpoReferenceIsUnchanged(
        DpoReferenceSnapshot reference)
    {
        try
        {
            var entries = reference.Model.GetCheckpointParameterEntries();
            return entries.All(static entry =>
                       !entry.Parameter.Value.RequiresGrad)
                   && StringComparer.Ordinal.Equals(
                       reference.ParameterFingerprint,
                       FingerprintNamedParameters(entries));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static string FingerprintNamedParameters(
        IReadOnlyList<LlmCheckpointParameterEntry> entries)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> integer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(integer, entries.Count);
        hash.AppendData(integer);
        foreach (var entry in entries)
        {
            var key = Encoding.UTF8.GetBytes(entry.Key);
            BinaryPrimitives.WriteInt32LittleEndian(integer, key.Length);
            hash.AppendData(integer);
            hash.AppendData(key);
            var dimensions = entry.Parameter.Value.Shape.Dimensions;
            BinaryPrimitives.WriteInt32LittleEndian(integer, dimensions.Length);
            hash.AppendData(integer);
            foreach (var dimension in dimensions)
            {
                BinaryPrimitives.WriteInt32LittleEndian(integer, dimension);
                hash.AppendData(integer);
            }
            var data = entry.Parameter.Value.Data;
            BinaryPrimitives.WriteInt32LittleEndian(integer, data.Length);
            hash.AppendData(integer);
            foreach (var value in data)
            {
                BinaryPrimitives.WriteInt32LittleEndian(
                    integer,
                    BitConverter.SingleToInt32Bits(value));
                hash.AppendData(integer);
            }
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static OperationResult<DpoReferenceSnapshot> InvalidDpoReference()
        => OperationResult<DpoReferenceSnapshot>.Fail(
            "An exact independent frozen DPO reference model could not be created.",
            "DPO_REFERENCE_MODEL_REQUIRED");

    private sealed record DpoReferenceSnapshot(
        LlmModel Model,
        string ParameterFingerprint);

    private static bool MatchesSnapshot(
        IEnumerable<Parameter> parameters,
        IReadOnlyDictionary<Parameter, int[]> snapshot)
    {
        var current = parameters.ToArray();
        if (current.Length != snapshot.Count)
            return false;

        foreach (var parameter in current)
        {
            if (!snapshot.TryGetValue(parameter, out var expected)
                || expected.Length != parameter.Value.Data.Length)
            {
                return false;
            }

            for (var index = 0; index < expected.Length; index++)
            {
                if (BitConverter.SingleToInt32Bits(parameter.Value.Data[index])
                    != expected[index])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AllFinite(IEnumerable<Parameter> parameters)
        => parameters.All(parameter => parameter.Value.Data.All(float.IsFinite));
}
