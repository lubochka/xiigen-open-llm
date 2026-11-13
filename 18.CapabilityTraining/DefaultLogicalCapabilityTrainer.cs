// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DPO;
using DPO.Models;
using LoRA;
using SemanticCompilerAdapter;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using Microsoft.Win32.SafeHandles;

// This trainer turns a visible knowledge edge into a probationary retrieval unit and a retryable flow.
public sealed class DefaultLogicalCapabilityTrainer : ILogicalCapabilityTrainer
{
    // Retrieval units registered by this trainer fill the generated-capability slot.
    public const string GeneratedCapabilityUnitRole = "generated-capability";

    // Generated flows are registered after training so the retry path can execute the new capability.
    public const string GeneratedCapabilityFlowIdPrefix = "generated-capability-flow";

    // Generated scenarios mirror the generated flow id so registry lookup remains traceable.
    public const string GeneratedCapabilityScenarioIdPrefix = "generated-capability";

    // Version applied to first-generation runtime capabilities.
    public const string GeneratedCapabilityVersion = "1.0.0";

    public const string AdmissionAuthorityRequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_ADMISSION_AUTHORITY_REQUIRED";
    public const string WindowsIdentityCustodyRequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_WINDOWS_IDENTITY_CUSTODY_REQUIRED";
    public const string TrainingContentProvenanceRequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_PROVENANCE_REQUIRED";
    public const string TrainingContentProvenanceInvalidErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_PROVENANCE_INVALID";

    private const int DefaultDpoMaxStepCount = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
    private const int DefaultDpoConvergenceWindowSteps = 1;
    private const int MaximumTrainingBranchCount = 256;
    private const int MaximumMetadataCount = 256;
    private const int MaximumTrainingTextLength = 1_048_576;
    private const int MaximumTrainingVectorLength = 65_536;
    internal const int MaximumDomainClassifierStateBytes = 4 * 1024 * 1024;
    internal const int MaximumDomainClassifierExampleCount = 4_096;
    internal const int MaximumDomainClassifierPredicateCount = MaximumMetadataCount;
    private const int MaximumDomainClassifierStringLength = 16_384;
    private const int MaximumDomainClassifierEncodedStringBytes =
        MaximumDomainClassifierStringLength * 6;
    private const int MaximumDomainClassifierJsonDepth = 8;
    private const int MaximumDomainClassifierJsonTokenCount =
        MaximumDomainClassifierExampleCount *
        (MaximumDomainClassifierPredicateCount + 12) + 2;
    public const string ReplayAttestationRequiredErrorCode =
        "LOGICAL_CAPABILITY_TRAINING_REPLAY_ATTESTATION_REQUIRED";
    private static readonly TimeSpan RootPublicationLeaseTimeout =
        TimeSpan.FromSeconds(30);
    private readonly string _checkpointRoot;
    private readonly object _manifestCacheGate = new();
    private readonly Dictionary<string, CachedTeachingStack> _teachingCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, GeneratedCapabilityCheckpointManifest> _manifestCacheByUnitId = new(StringComparer.Ordinal);
    private readonly ILogicalTeachingIdleGate _idleGate;
    private readonly ITrustScorer? _trustScorer;
    private readonly ILogicalCapabilityTrainingAdmissionAuthority? _admissionAuthority;
    private bool _manifestCacheHydrated;

    private static readonly AsyncLocal<Action<string>?> MutationBoundaryObserver = new();
    private static readonly AsyncLocal<Action?> PostDpoObserver = new();
    private static readonly AsyncLocal<Action?> FinalPublicationObserver = new();
    private static readonly AsyncLocal<Action<string, string>?> PublicationStageObserver = new();
    private static readonly AsyncLocal<Action<string>?> RegistryPublicationObserver = new();
    private static readonly AsyncLocal<Func<Exception?>?> CleanupFailureFactory = new();
    private static readonly AsyncLocal<Exception?> LastCleanupFailure = new();
    private static readonly AsyncLocal<Action?> AbandonedRootLeaseObserver = new();
    private static readonly AsyncLocal<Action?> RootLeaseWaitObserver = new();
    private static readonly AsyncLocal<Func<Exception?>?>
        ClassifierBackupDeletionFailureFactory = new();
    private static readonly AsyncLocal<Action?>
        PostClassifierBackupDeletionObserver = new();
    private static readonly AsyncLocal<Func<bool>?>
        WindowsIdentityCustodyAvailability = new();
    private static readonly AsyncLocal<Action<DpoTrainerArtifactOwnershipProof>?>
        DpoArtifactOwnershipAcceptedObserver = new();
    private static readonly AsyncLocal<Action<DpoTrainerArtifactOwnershipProof>?>
        DpoProofOnlyCleanupAfterValidationObserver = new();
    private static readonly AsyncLocal<Action<string, string, string>?>
        ClassifierIdentityRenameObserver = new();
    private static readonly AsyncLocal<Action<string>?>
        CleanupArbiterFaultObserver = new();
    private static readonly AsyncLocal<Func<Exception?>?>
        ClassifierBackupProtectionFailureFactory = new();
    private static readonly AsyncLocal<Action<LogicalDpoTrainingRequest>?>
        DpoRequestObserver = new();

    internal static Action<string>? MutationBoundaryObserverForTesting
    {
        get => MutationBoundaryObserver.Value;
        set => MutationBoundaryObserver.Value = value;
    }

    internal static Action? PostDpoObserverForTesting
    {
        get => PostDpoObserver.Value;
        set => PostDpoObserver.Value = value;
    }

    internal static Action? FinalPublicationObserverForTesting
    {
        get => FinalPublicationObserver.Value;
        set => FinalPublicationObserver.Value = value;
    }

    internal static Action<string, string>? PublicationStageObserverForTesting
    {
        get => PublicationStageObserver.Value;
        set => PublicationStageObserver.Value = value;
    }

    internal static Action<string>? RegistryPublicationObserverForTesting
    {
        get => RegistryPublicationObserver.Value;
        set => RegistryPublicationObserver.Value = value;
    }

    internal static Func<Exception?>? CleanupFailureFactoryForTesting
    {
        get => CleanupFailureFactory.Value;
        set => CleanupFailureFactory.Value = value;
    }

    internal static Exception? LastCleanupFailureForTesting
    {
        get => LastCleanupFailure.Value;
        set => LastCleanupFailure.Value = value;
    }

    internal static Action? AbandonedRootLeaseObserverForTesting
    {
        get => AbandonedRootLeaseObserver.Value;
        set => AbandonedRootLeaseObserver.Value = value;
    }

    internal static Action? RootLeaseWaitObserverForTesting
    {
        get => RootLeaseWaitObserver.Value;
        set => RootLeaseWaitObserver.Value = value;
    }

    internal static Func<Exception?>?
        ClassifierBackupDeletionFailureFactoryForTesting
    {
        get => ClassifierBackupDeletionFailureFactory.Value;
        set => ClassifierBackupDeletionFailureFactory.Value = value;
    }

    internal static Action? PostClassifierBackupDeletionObserverForTesting
    {
        get => PostClassifierBackupDeletionObserver.Value;
        set => PostClassifierBackupDeletionObserver.Value = value;
    }

    internal static Func<bool>?
        WindowsIdentityCustodyAvailabilityForTesting
    {
        get => WindowsIdentityCustodyAvailability.Value;
        set => WindowsIdentityCustodyAvailability.Value = value;
    }

    internal static Action<DpoTrainerArtifactOwnershipProof>?
        DpoArtifactOwnershipAcceptedObserverForTesting
    {
        get => DpoArtifactOwnershipAcceptedObserver.Value;
        set => DpoArtifactOwnershipAcceptedObserver.Value = value;
    }

    internal static Action<DpoTrainerArtifactOwnershipProof>?
        DpoProofOnlyCleanupAfterValidationObserverForTesting
    {
        get => DpoProofOnlyCleanupAfterValidationObserver.Value;
        set => DpoProofOnlyCleanupAfterValidationObserver.Value = value;
    }

    internal static Action<string, string, string>?
        ClassifierIdentityRenameObserverForTesting
    {
        get => ClassifierIdentityRenameObserver.Value;
        set => ClassifierIdentityRenameObserver.Value = value;
    }

    internal static Action<string>? CleanupArbiterFaultObserverForTesting
    {
        get => CleanupArbiterFaultObserver.Value;
        set => CleanupArbiterFaultObserver.Value = value;
    }

    internal static Func<Exception?>?
        ClassifierBackupProtectionFailureFactoryForTesting
    {
        get => ClassifierBackupProtectionFailureFactory.Value;
        set => ClassifierBackupProtectionFailureFactory.Value = value;
    }

    internal static Action<LogicalDpoTrainingRequest>?
        DpoRequestObserverForTesting
    {
        get => DpoRequestObserver.Value;
        set => DpoRequestObserver.Value = value;
    }

    internal static bool TryDeleteDpoProofOnlyArtifactForTesting(
        DpoTrainerArtifactOwnershipProof proof)
        => InvocationPublicationCleanup
            .TryDeleteDpoProofOnlyArtifactForTesting(proof);

    internal static string BuildRootPublicationMutexNameForTesting(string root)
        => BuildRootPublicationMutexName(NormalizeRootIdentity(root));

    // Create the trainer with a checkpoint root outside the repository by default.
    public DefaultLogicalCapabilityTrainer(
        string? checkpointRoot = null,
        ILogicalTeachingIdleGate? idleGate = null,
        ITrustScorer? trustScorer = null,
        ILogicalCapabilityTrainingAdmissionAuthority? admissionAuthority = null,
        // evaluationMaterialProvider — доверенный подготовитель материала оценки. Параметр стоит
        // последним и необязателен, поэтому все прежние вызывающие продолжают работать без правок.
        // Материал оценки приходит ИМЕННО отсюда, а не из пользовательских полей запроса и не из
        // доменных метк: иначе проверку независимости можно было бы пройти простым объявлением
        // нужных значений.
        ICapabilityEvaluationMaterialProvider? evaluationMaterialProvider = null,
        // deliveryLedger — журнал состоявшихся выдач ответа пользователю. Параметр стоит
        // последним и необязателен, поэтому все прежние вызывающие продолжают работать без
        // правок. Журнал нужен ровно для одного: чтобы точка обычной выдачи запомнила, какая
        // единица была выбрана и какой ответ показан, и будущее подтверждённое исправление
        // нашло своего адресата. Без журнала поведение продукта остаётся в точности прежним.
        IAnsweredDeliveryLedger<string>? deliveryLedger = null)
    {
        _checkpointRoot = string.IsNullOrWhiteSpace(checkpointRoot)
            ? Path.Combine(Path.GetTempPath(), "llm-mvp-capability-training")
            : checkpointRoot;
        _idleGate = idleGate ?? (ILogicalTeachingIdleGate?)AutonomousLearningIdleGate.TryCreate() ?? new AlwaysReadyLogicalTeachingIdleGate();
        _trustScorer = trustScorer;
        _admissionAuthority = admissionAuthority;
        // Подготовитель запоминается как есть: его отсутствие — обычный разрешённый случай.
        _evaluationMaterialProvider = evaluationMaterialProvider;
        // Журнал выдач запоминается как есть и дальше только передаётся фабрике потока.
        _deliveryLedger = deliveryLedger;
    }

    // _evaluationMaterialProvider хранит доверенного подготовителя материала оценки или ничего.
    private readonly ICapabilityEvaluationMaterialProvider? _evaluationMaterialProvider;

    // _deliveryLedger хранит журнал состоявшихся выдач или ничего, если его не передали.
    private readonly IAnsweredDeliveryLedger<string>? _deliveryLedger;

    // TraceMaterialAdmission — ЧИТАТЕЛЬ расписки допуска, и читает он её сразу после допуска, до
    // первого разрешения на изменение.
    //
    // Зачем читатель вообще нужен. Расписка, которую никто не читает, — украшение: по ней нельзя
    // ни узнать, что обучали именно проверенное, ни увидеть, что оценка независимости не
    // выдавалась вовсе. Прежде так и было: заметка об отсутствии оценки была объявлена в коде и не
    // выводилась ни одной строкой.
    //
    // ГРАНИЦА ЭТОГО ЧИТАТЕЛЯ, названная прямо. Это диагностический след, а не публикация расписки
    // в наружный результат обучения и не запись её в слепок состояния. Наружная публикация меняет
    // договор результата и версию сохраняемого слепка, и это отдельная работа отдельного
    // владельца; здесь она НЕ сделана и названа остатком.
    private static void TraceMaterialAdmission(
        CapabilityMaterialLineageGuard.AdmittedTrainingMaterial admitted)
    {
        // Материала оценки не было: это разрешённый прежний путь, но заявить по нему независимую
        // оценку нельзя, и молчать об этом нечестно.
        if (admitted.Receipt is null)
        {
            // Один человеческий след на запуск с названной заметкой и числом обучаемых пар.
            System.Diagnostics.Trace.TraceWarning(
                "CapabilityTraining: независимая оценка материала этим запуском НЕ выдаётся " +
                "({0}): доверенного материала оценки для него нет. Обучение продолжается прежним " +
                "разрешённым путём; обучаемых пар предпочтений {1}.",
                admitted.EvaluationNote,
                admitted.PreferenceRowCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return;
        }

        // Материал допущен: след называет профиль оценки, что именно доказано о связи, число
        // связанных пар и отпечаток фактически потребляемых байтов.
        System.Diagnostics.Trace.TraceInformation(
            "CapabilityTraining: материал допущен к обучению ({0}), профиль оценки '{1}', " +
            "доказанная связь с потребляемым '{2}', связанных пар предпочтений {3}, отпечаток " +
            "потребляемого материала {4}.",
            admitted.EvaluationNote,
            admitted.Receipt.ProfileId,
            admitted.Receipt.ConsumedMaterialBinding,
            admitted.Receipt.ConsumedPreferenceRowCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            admitted.Receipt.ConsumedMaterialSha256 ?? "отсутствует");
    }

    // MaterialAdmissionReceiptDirectoryName — имя каталога расписок допуска внутри корня
    // контрольных точек. Каталог отдельный намеренно: он не лежит ни под одним корнем публикации,
    // поэтому запись расписки не касается ни проверки отсутствия назначения, ни сверки описи
    // слепков, ни чужих опубликованных улик.
    private const string MaterialAdmissionReceiptDirectoryName = ".capability-material-receipts";

    // PersistMaterialAdmissionReceipt сохраняет расписку допуска на диск и тут же читает её
    // обратно, сверяя все поля.
    //
    // Почему это нужно. Прежде расписка существовала ровно до выхода из метода обучения: её читал
    // один диагностический след и больше никто. Сказать потом, какой именно материал был допущен
    // к этому обучению, было не по чему, то есть связка материала с обучением доказательством не
    // была. Доказательство обязано переживать перезапуск и читаться обратно.
    //
    // Почему отказ, а не молчание. Если расписку сохранить не удалось, обучение прошло бы без
    // долговечной связки, а наружу об этом никто бы не узнал. Молчаливая потеря улики — это и
    // есть та самая расписка-украшение, от которой здесь уходим.
    //
    // ГРАНИЦА, названная прямо. На прежнем разрешённом пути без материала оценки расписки нет
    // вовсе, и записывать нечего: этот случай назван успехом с пустым путём, а не отказом.
    private OperationResult<string> PersistMaterialAdmissionReceipt(
        CapabilityMaterialLineageGuard.AdmittedTrainingMaterial admitted,
        string runId)
    {
        // Расписки нет — прежний путь без материала оценки; записывать нечего.
        if (admitted.Receipt is null)
            return OperationResult<string>.Ok(string.Empty);

        // Каталог расписок лежит рядом с корнем контрольных точек этого обучателя.
        var receiptRoot = Path.Combine(
            Path.GetFullPath(_checkpointRoot),
            MaterialAdmissionReceiptDirectoryName);
        // Хранилище само пишет, само читает обратно и само сверяет все поля.
        return CapabilityMaterialAdmissionReceiptStore.Save(receiptRoot, runId, admitted.Receipt);
    }

    // Release live teaching stacks and manifest indexes so long-running batch runners
    // can start the next physical batch from disk without retaining model objects.
    public void ClearCache()
    {
        lock (_manifestCacheGate)
        {
            _teachingCache.Clear();
            _manifestCacheByUnitId.Clear();
            _manifestCacheHydrated = false;
        }
    }

    // Thin delegation to DpoConvergenceMonitor with no behaviour of its own:
    // it detects DPO (Direct Preference Optimization) convergence by comparing
    // the loss of the current observation with the loss of the previous one.
    // `windowSteps` is the observation interval - how many optimizer steps the
    // caller runs between two ShouldStop calls - and NOT a rolling-average
    // window: no loss buffer exists here or in the inner monitor.
    public sealed class ConvergenceMonitor
    {
        // The inner monitor owns the comparison; this wrapper only forwards.
        private readonly DpoConvergenceMonitor _inner;

        // Create a monitor using the E11 (experiment 11) loss-delta threshold
        // and the observation interval in optimizer steps.
        public ConvergenceMonitor(float threshold = 0.002f, int windowSteps = 25)
        {
            // Forward both values unchanged; normalization happens inside.
            _inner = new DpoConvergenceMonitor(threshold, windowSteps);
        }

        // Return true once two consecutive comparisons stayed under the
        // threshold. The first call only records the starting loss, so the
        // earliest possible true is the THIRD call; a caller that queries this
        // monitor once every `windowSteps` steps therefore needs a step budget
        // of at least three observation intervals before convergence can ever
        // be detected.
        public bool ShouldStop(float currentLoss) => _inner.ShouldStop(currentLoss);

        // Шагов, о которых сообщил вызывающий: число вызовов ShouldStop,
        // умноженное на интервал наблюдения. Это учёт, а не измерение качества.
        public int StepsTaken => _inner.StepsTaken;
    }

    // Train from the observed knowledge-edge branches and register the generated capability.
    public async Task<OperationResult<LogicalCapabilityTrainingResult>> TrainAndRegisterAsync(
        LogicalCapabilityTrainingRequest request,
        CancellationToken cancellationToken = default)
    {
        var arbiter = new OperationCleanupArbiter();
        Exception? primary = null;
        try
        {
            return await TrainAndRegisterCoreAsync(
                request, cancellationToken, arbiter).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // Retain the exact object before any optional classification or
            // diagnostic work can allocate or fail.
            primary = exception;
            throw;
        }
        finally
        {
            arbiter.ThrowWinner(primary);
        }
    }

    private async Task<OperationResult<LogicalCapabilityTrainingResult>>
        TrainAndRegisterCoreAsync(
            LogicalCapabilityTrainingRequest request,
            CancellationToken cancellationToken,
            OperationCleanupArbiter cleanupArbiter)
    {
        // Публикация обученной способности разрешена только там, где платформа
        // умеет удерживать физическую личность файла. Отказ происходит до
        // допуска, обучения, реестра и создания каталогов; код ошибки оставлен
        // прежним, чтобы старые отчёты не потеряли совместимость.
        if (!(WindowsIdentityCustodyAvailability.Value?.Invoke() ??
              LogicalCapabilityFileCustody.IsCurrentPlatformSupported))
        {
            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                "Logical capability training requires Windows file-identity custody.",
                WindowsIdentityCustodyRequiredErrorCode);
        }

        LastCleanupFailure.Value = null;

        // Return cancellation through the shared envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Cancelled.", "CANCELLED");

        // Validate the request shape.
        if (request is null)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Capability training request is required.", "CAPABILITY_TRAINING_REQUEST_REQUIRED");
        if (request.KnowledgeEdgeBranches is null ||
            request.KnowledgeEdgeBranches.Length == 0)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("At least one knowledge-edge branch is required.", "KNOWLEDGE_EDGE_BRANCH_REQUIRED");
        if (request.KnowledgeEdgeBranches.Length > MaximumTrainingBranchCount)
            return InvalidTrainingSnapshot()
                .MapError<LogicalCapabilityTrainingResult>();
        if (request.Topology is null || request.Flows is null || request.Units is null)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Topology, flow, and unit registries are required.", "CAPABILITY_REGISTRIES_REQUIRED");
        foreach (var branch in request.KnowledgeEdgeBranches)
        {
            if (branch?.TrainingContentProvenance is null)
            {
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    "Typed provenance for every logical training branch is required.",
                    TrainingContentProvenanceRequiredErrorCode);
            }
            if (!ValidateTrainingBranchProvenance(request, branch))
            {
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    "Logical training branch provenance is invalid.",
                    TrainingContentProvenanceInvalidErrorCode);
            }
        }
        var ownedRequestResult = SnapshotTrainingRequest(request);
        if (!ownedRequestResult.IsSuccess)
            return ownedRequestResult.MapError<LogicalCapabilityTrainingResult>();
        request = ownedRequestResult.Value!;

        // Track runtime and registered artifacts for the returned teaching evidence.
        var stopwatch = Stopwatch.StartNew();
        var registeredFlowIds = new List<string>();
        var registeredSubPhaseIds = new List<string>();
        var registeredUnitIds = new List<string>();
        var evidenceIds = new List<string>();
        var notes = new List<string>();
        var branchTimings = new List<LogicalCapabilityTrainingBranchTiming>();
        var domainAdapterTag = request.Domain.ToString();
        var compiledPrompt = CompileForTeaching(
            request.OriginalRequest.UserRequest);
        var compiledText = compiledPrompt?.CanonicalText;
        var batchedDpoStepCount = 0;
        string[] dpoMetricSourcePaths = [];

        // Validate and collect every branch first so one same-domain row can teach in one batched DPO step.
        var branchPlans = new List<TrainingBranchPlan>();
        foreach (var branch in request.KnowledgeEdgeBranches)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalCapabilityTrainingResult>.Fail("Cancelled.", "CANCELLED");

            var trainingContentProvenance =
                branch.TrainingContentProvenance;
            if (trainingContentProvenance is null)
            {
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    "Typed provenance for the exact logical training branch is required.",
                    TrainingContentProvenanceRequiredErrorCode);
            }
            if (!ValidateTrainingBranchProvenance(request, branch))
            {
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    "Logical training branch provenance is invalid.",
                    TrainingContentProvenanceInvalidErrorCode);
            }

            // Extract the routed sub-phase from the branch metrics.
            var subPhaseId = ExtractSubPhaseId(branch);
            if (string.IsNullOrWhiteSpace(subPhaseId))
            {
                notes.Add($"Skipped branch '{branch.FlowId}' because it did not name a sub-phase.");
                continue;
            }

            // The trainer can only install a flow when the topology contains the target sub-phase.
            var subPhase = request.Topology.ResolveSubPhase(subPhaseId);
            if (!subPhase.IsSuccess)
            {
                notes.Add($"Skipped sub-phase '{subPhaseId}' because it is not registered in topology.");
                continue;
            }

            // Build or continue the local teaching stack for this sub-phase.
            LogicalSubPhase ownedSubPhase;
            LogicalCapabilityTrainingCurrentProjection currentProjection;
            try
            {
                ownedSubPhase = CloneSubPhase(subPhase.Value!.SubPhase);
                if (!ValidateTrainingBranchProvenance(
                        request,
                        branch,
                        ownedSubPhase))
                {
                    return OperationResult<
                        LogicalCapabilityTrainingResult>.Fail(
                            "Logical training branch provenance is invalid.",
                        TrainingContentProvenanceInvalidErrorCode);
                }
                var projected =
                    LogicalCapabilityTrainingPreferenceProjector
                        .ProjectCurrent(
                    request.OriginalRequest.UserRequest,
                    branch,
                    ownedSubPhase);
                if (!projected.IsSuccess || projected.Value is null ||
                    !trainingContentProvenance.AuthorizesCurrentPreference(
                        projected.Value.DpoPreferenceSha256))
                {
                    return OperationResult<
                        LogicalCapabilityTrainingResult>.Fail(
                        "Logical training branch provenance is invalid.",
                        TrainingContentProvenanceInvalidErrorCode);
                }
                currentProjection = projected.Value;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return InvalidTrainingSnapshot()
                    .MapError<LogicalCapabilityTrainingResult>();
            }
            if (currentProjection.Prompt.Length > MaximumTrainingTextLength ||
                currentProjection.TeachingVector.Length is 0 or > MaximumTrainingVectorLength ||
                currentProjection.TeachingVector.Any(static value => !float.IsFinite(value)))
            {
                return InvalidTrainingSnapshot()
                    .MapError<LogicalCapabilityTrainingResult>();
            }
            branchPlans.Add(new TrainingBranchPlan(
                branch,
                subPhaseId,
                ownedSubPhase,
                currentProjection.Prompt,
                currentProjection.Chosen,
                currentProjection.Rejected,
                currentProjection.TeachingVector,
                currentProjection,
                AuthorizedReplayDescriptor: null,
                AuthorizedReplayExampleId: null,
                trainingContentProvenance.Snapshot(),
                [trainingContentProvenance.BindingSha256]));
        }

        if (branchPlans.Count > 0)
        {
            var registrationPlansResult = BuildRegistrationPlans(
                branchPlans,
                domainAdapterTag);
            if (!registrationPlansResult.IsSuccess || registrationPlansResult.Value is null)
                return registrationPlansResult.MapError<LogicalCapabilityTrainingResult>();
            var registrationPlans = registrationPlansResult.Value;
            var invocationToken = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            var publicationPlansResult = BuildPublicationPlans(
                registrationPlans,
                request.RunId,
                domainAdapterTag,
                invocationToken);
            if (!publicationPlansResult.IsSuccess)
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    publicationPlansResult.ErrorMessage,
                    publicationPlansResult.ErrorCode);

            var requestFlags = request.OriginalRequest.Flags ?? new LogicalExecutionFlags();
            var inventoryResult = ReadManifestInventorySnapshotResult();
            if (!inventoryResult.IsSuccess)
                return inventoryResult.MapError<LogicalCapabilityTrainingResult>();
            var manifestInventory = inventoryResult.Value!;
            if (_admissionAuthority is null)
            {
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                    "Logical capability training admission authority is required.",
                    AdmissionAuthorityRequiredErrorCode);
            }
            var replayResult = SelectAndValidateReplaySnapshot(
                manifestInventory,
                request.Topology,
                domainAdapterTag,
                compiledText,
                requestFlags.UseReplayFusionRanking);
            if (!replayResult.IsSuccess)
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(replayResult.ErrorMessage, replayResult.ErrorCode);
            var legalTrainingReplayManifests = replayResult.Value!;

            var totalKnownRows = Math.Max(
                1,
                legalTrainingReplayManifests.Length + branchPlans.Count);
            var distinctClasses = Math.Max(1, branchPlans.Select(plan => plan.SubPhaseId).Distinct(StringComparer.Ordinal).Count());
            var rareRows = Math.Max(1, legalTrainingReplayManifests.Length + 1);
            var currentRepeatCount = LogicalTeachingDatasetWeighter.ComputeCurrentRepeatCount(totalKnownRows, distinctClasses, rareRows);
            if (legalTrainingReplayManifests.Length > 0)
                currentRepeatCount = Math.Max(currentRepeatCount, 2);
            var maxDpoStepCount = ReadDpoMaxStepCount(
                request.OriginalRequest.Metadata);
            var convergenceWindowSteps = ReadDpoConvergenceWindowSteps(
                request.OriginalRequest.Metadata);

            var finalPreferences = SnapshotPreferences(
                AutonomousLearningReplayMixer.BuildPreferences(
                    branchPlans.Select(plan => new LogicalDpoPreferenceExample(
                        plan.TrainingText,
                        plan.ChosenText,
                        plan.RejectedText,
                        plan.TeachingVector.ToArray())
                    {
                        // ValidateTrainingBranchProvenance уже доказал
                        // DeterministicSyntheticNoExternalTeacher; поэтому
                        // current pair получает typed авторство без fallback.
                        ChosenAuthorKind =
                            DpoPairSideAuthorKind.DeterministicGenerator,
                        RejectedAuthorKind =
                            DpoPairSideAuthorKind.DeterministicGenerator,
                        TrainingContentProvenance =
                            plan.TrainingContentProvenance.Snapshot()
                    }).ToArray(),
                    legalTrainingReplayManifests,
                    currentRepeatCount));
            var admissionBatchResult = BuildAdmissionBatch(
                request.RunId,
                domainAdapterTag,
                finalPreferences);
            if (!admissionBatchResult.IsSuccess)
                return admissionBatchResult.MapError<LogicalCapabilityTrainingResult>();

            var leaseResult = _admissionAuthority.AcquireTrainingLease(
                admissionBatchResult.Value!);
            if (!leaseResult.IsSuccess || leaseResult.Value is null)
                return leaseResult.MapError<LogicalCapabilityTrainingResult>();
            var admissionLease = leaseResult.Value;
            using var admissionLeaseCleanup = cleanupArbiter.Track(admissionLease);
            var admissionCapabilities =
                admissionLease.IssueAdmissionCapabilities();
            if (!admissionCapabilities.IsSuccess ||
                admissionCapabilities.Value is null)
            {
                return admissionCapabilities
                    .MapError<LogicalCapabilityTrainingResult>();
            }
            var sealedReplayResult = SealCurrentReplayAttestations(
                admissionLease,
                registrationPlans,
                domainAdapterTag);
            if (!sealedReplayResult.IsSuccess || sealedReplayResult.Value is null)
                return sealedReplayResult.MapError<LogicalCapabilityTrainingResult>();
            var sealedReplayBySubPhaseId = sealedReplayResult.Value;

            if (requestFlags.UseIdleLearningScheduling)
            {
                var idle = _idleGate.CanTrainNow(request);
                if (!idle.IsSuccess)
                    return idle.MapError<LogicalCapabilityTrainingResult>();
                if (!idle.Value)
                {
                    return OperationResult<LogicalCapabilityTrainingResult>.Ok(
                        new LogicalCapabilityTrainingResult(
                            false,
                            [],
                            [],
                            [],
                            0,
                            [],
                            "Teaching deferred by D-14 idle-learning gate."));
                }
            }

            // Общий учебный текст собирается ЗДЕСЬ, до первого разрешения на изменение: он
            // участвует в допуске материала, а допуск обязан состояться раньше любого разрешения.
            var combinedTexts = branchPlans.Select(plan => plan.TrainingText).ToArray();
            var combinedTrainingTextResult = BuildCombinedTrainingText(combinedTexts);
            if (!combinedTrainingTextResult.IsSuccess ||
                combinedTrainingTextResult.Value is null)
                return combinedTrainingTextResult
                    .MapError<LogicalCapabilityTrainingResult>();
            var combinedTrainingText = combinedTrainingTextResult.Value;

            // ДОПУСК МАТЕРИАЛА. Единственный вход: носитель сам спрашивает подготовителя оценки,
            // сам проверяет происхождение его строк и сам связывает проверенное с тем, что
            // обучение фактически потребляет — с парами предпочтений и с общим учебным текстом.
            //
            // Почему именно здесь. Ни одного разрешения на изменение к этому моменту ещё не
            // выдано, учебный стек не собран, обучение не начиналось. Отказ материала поэтому
            // ничего не откатывает: откатывать нечего.
            //
            // Почему у обучения больше нет другого входа для материала. Сборка учебного стека и
            // выполнение обучения предпочтениями принимают теперь ТОЛЬКО этот носитель: довода с
            // сырым текстом или сырым набором пар у них нет вовсе. Поэтому подменить содержание
            // между проверкой и обучением нечем — не проверкой вдогонку, а отсутствием возможности.
            //
            // Корпус ближайших соседей тоже входит в допуск, и входит ЗДЕСЬ, а не позже.
            //
            // Что было неправильно. Корпус поиска собирался много позже, уже после обучения
            // предпочтениями, прямо из тех же двух входов — отобранных слепков повтора и планов
            // ветвей. Допуск их не видел вовсе, поэтому проверенный материал и тот корпус,
            // который реально читала обучаемая единица поиска, были связаны только порядком строк
            // в коде. Экзаменационная строка в корпусе не ловилась ничем: отказ смотрел общий
            // текст и стороны пар, а корпус в него не входил.
            //
            // Почему перенос сборки сюда ничего не меняет в байтах. Строки корпуса — чистая
            // функция от тех же двух входов, и оба уже готовы: планы ветвей собраны выше, слепки
            // повтора отобраны выше, ни одного разрешения на изменение ещё не выдано. Тот же
            // вход в том же порядке даёт те же строки, поэтому и отпечаток корпуса, и опись
            // остаются прежними.
            var orderedRetrievalCorpus = BuildOrderedRetrievalCorpusExamples(
                legalTrainingReplayManifests,
                registrationPlans);

            var admittedMaterial = CapabilityMaterialLineageGuard.AdmittedTrainingMaterial.Admit(
                _evaluationMaterialProvider,
                request.RunId,
                domainAdapterTag,
                finalPreferences,
                combinedTrainingText,
                orderedRetrievalCorpus);
            // Отказ материала — обычный результат с человеческой причиной и кодом.
            if (!admittedMaterial.IsSuccess || admittedMaterial.Value is null)
                return admittedMaterial.MapError<LogicalCapabilityTrainingResult>();
            // Допущенный носитель берётся в отдельную величину сразу после проверки на пустоту:
            // дальше по ходу она не переспрашивается, и обучение получает ровно её.
            var admitted = admittedMaterial.Value;
            // Расписка допуска ЧИТАЕТСЯ сразу после допуска и выводится одной человеческой строкой.
            TraceMaterialAdmission(admitted);
            // И сохраняется на диск: расписка, живущая только в памяти одного запуска, связкой
            // материала с обучением не является, потому что после выхода из метода её нет.
            var persistedReceipt = PersistMaterialAdmissionReceipt(admitted, request.RunId);
            // Невозможность сохранить связку — названный отказ, а не молчаливая потеря улики.
            if (!persistedReceipt.IsSuccess)
                return persistedReceipt.MapError<LogicalCapabilityTrainingResult>();

            var batchCheckpointDirectory = Path.Combine(
                Path.GetFullPath(_checkpointRoot),
                $".generated-capability-dpo-staging-{invocationToken}");
            var publicationCleanup = new InvocationPublicationCleanup();
            using var publicationCleanupScope = cleanupArbiter.Track(
                publicationCleanup);
            var destinationPermit = AcquireMutationPermit(
                admissionCapabilities.Value,
                "capability_training:destination_reservation");
            if (!destinationPermit.IsSuccess || destinationPermit.Value is null)
                return destinationPermit.MapError<LogicalCapabilityTrainingResult>();
            using (cleanupArbiter.Track(destinationPermit.Value))
            {
                var preparedDirectories = PrepareInvocationStaging(
                    publicationPlansResult.Value!,
                    batchCheckpointDirectory,
                    invocationToken,
                    publicationCleanup);
                if (!preparedDirectories.IsSuccess)
                    return preparedDirectories.MapError<LogicalCapabilityTrainingResult>();
            }
            cleanupArbiter.ThrowPendingFatal();
            publicationCleanup.PrepareDpoArtifactOwnership(maxDpoStepCount);

            var buildStackStopwatch = Stopwatch.StartNew();
            var parentManifestResult = FindActiveParentManifestFromSnapshot(
                request.Units,
                domainAdapterTag,
                manifestInventory);
            if (!parentManifestResult.IsSuccess)
                return parentManifestResult.MapError<LogicalCapabilityTrainingResult>();

            OperationResult<TeachingStack> teachingStack;
            LlmModel? referenceModel = null;
            var referenceModelStopwatch = Stopwatch.StartNew();
            var stackPermit = AcquireMutationPermit(
                admissionCapabilities.Value,
                "capability_training:build_stack");
            if (!stackPermit.IsSuccess || stackPermit.Value is null)
                return stackPermit.MapError<LogicalCapabilityTrainingResult>();
            using (cleanupArbiter.Track(stackPermit.Value))
            {
                teachingStack = BuildTeachingStack(
                    parentManifestResult.Value,
                    domainAdapterTag,
                    // Учебный текст уходит в сборку стека только носителем допущенного материала.
                    admitted,
                    compiledPrompt,
                    admissionCapabilities.Value);
                if (teachingStack.IsSuccess && teachingStack.Value is not null)
                    referenceModel = new LlmModel(teachingStack.Value.PolicyModel.Config);
            }
            cleanupArbiter.ThrowPendingFatal();
            buildStackStopwatch.Stop();
            referenceModelStopwatch.Stop();
            if (!teachingStack.IsSuccess)
            {
                return teachingStack.MapError<LogicalCapabilityTrainingResult>();
            }
            else
            {
                var tokenizer = teachingStack.Value!.Tokenizer;
                var policyModel = teachingStack.Value.PolicyModel;
                // Execute one DPO teaching unit for every current branch in this domain row.
                var dpoStopwatch = Stopwatch.StartNew();
                var dpoResult = await ExecuteDpoTeachingAsync(
                    request.RunId,
                    policyModel,
                    referenceModel!,
                    tokenizer,
                    batchCheckpointDirectory,
                    // Пары предпочтений уходят в обучение только носителем допущенного материала.
                    admitted,
                    teachingStack.Value.ParentPolicyCheckpointPath,
                    teachingStack.Value.ParentProvenance,
                    maxDpoStepCount,
                    convergenceWindowSteps,
                    admissionCapabilities.Value,
                    publicationCleanup.TryAcceptDpoArtifactOwnership,
                    cancellationToken);
                dpoStopwatch.Stop();
                if (!dpoResult.IsSuccess)
                {
                    // DPO может вернуть отказ уже после того, как финальные
                    // policy/metrics были приняты callback'ом. На Linux рядом с
                    // ними остаётся точный reservation-lock текущего шага; для
                    // rollback мы забираем только этот ожидаемый owned sidecar и
                    // всё ещё оставляем чужую замену нетронутой.
                    publicationCleanup.TrackDpoReservationLocksForRollback(
                        batchCheckpointDirectory,
                        maxDpoStepCount);
                    return dpoResult.MapError<LogicalCapabilityTrainingResult>();
                }
                else
                {
                    var postDpoPermit = AcquireMutationPermit(
                        admissionCapabilities.Value,
                        "capability_training:post_dpo_result");
                    if (!postDpoPermit.IsSuccess || postDpoPermit.Value is null)
                        return postDpoPermit.MapError<LogicalCapabilityTrainingResult>();
                    using (cleanupArbiter.Track(postDpoPermit.Value))
                    {
                        dpoMetricSourcePaths = dpoResult.Value!.Output
                            .MetricFilePaths
                            .ToArray();
                        batchedDpoStepCount = dpoResult.Value.Output.DpoStepCount;
                    }
                    cleanupArbiter.ThrowPendingFatal();

                    var dpoArtifactTracking =
                        publicationCleanup.TrackCompletedDpoArtifacts(
                            batchCheckpointDirectory,
                            invocationToken,
                            batchedDpoStepCount,
                            dpoMetricSourcePaths);
                    if (!dpoArtifactTracking.IsSuccess)
                    {
                        return dpoArtifactTracking
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    PostDpoObserver.Value?.Invoke();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            "Cancelled.",
                            "CANCELLED");
                    }
                    var trustState = AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(
                        request,
                        branchPlans.Select(plan => new TrainingBranchPlanView(plan.SubPhaseId, plan.SubPhase.PredicateIds, plan.SubPhase.DomainIds)).ToArray(),
                        legalTrainingReplayManifests,
                        _trustScorer);
                    notes.Add($"domain={domainAdapterTag}; batch_branches={branchPlans.Count}; registered_sub_phases={registrationPlans.Count}; semantic_compression=enabled; current_repeat={currentRepeatCount}; replay_count={Math.Max(0, dpoResult.Value.Output.PreferenceCount - (branchPlans.Count * currentRepeatCount))}; batched_dpo_steps={dpoResult.Value.Output.DpoStepCount}; d09_max_weight={LogicalTeachingDatasetWeighter.ComputeMaxWeight(totalKnownRows, distinctClasses, rareRows):0.###}");
                    notes.Add($"trust_scorer=project16; trust_state={trustState}");
                    var sharedBuildStackMs = buildStackStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;
                    var sharedReferenceModelMs = referenceModelStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;
                    var sharedDpoMs = dpoStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;

                    var rootPublicationLeaseResult =
                        AcquireRootPublicationLease(
                            publicationPlansResult.Value![0].CanonicalRoot);
                    if (!rootPublicationLeaseResult.IsSuccess ||
                        rootPublicationLeaseResult.Value is null)
                    {
                        return rootPublicationLeaseResult
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    var rootPublicationLease = rootPublicationLeaseResult.Value;
                    using var rootPublicationLeaseCleanup = cleanupArbiter.Track(rootPublicationLease);
                    var rootBoundPublicationCleanup = publicationCleanup.EnterRootLeaseScope();
                    using var rootBoundPublicationCleanupScope = cleanupArbiter.Track(rootBoundPublicationCleanup);
                    // This composite scope is synchronous and contains no
                    // await. It is the sole production lock order: unit-to-flow.
                    var registryPublicationLeaseResult =
                        LogicalRegistryPublicationLease.TryAcquireUnitThenFlow(
                            request.Units,
                            request.Flows,
                            RootPublicationLeaseTimeout);
                    if (!registryPublicationLeaseResult.IsSuccess ||
                        registryPublicationLeaseResult.Value is null)
                    {
                        return registryPublicationLeaseResult
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    var registryPublicationLease =
                        registryPublicationLeaseResult.Value;
                    using var registryPublicationLeaseCleanup =
                        cleanupArbiter.Track(registryPublicationLease);

                    var finalPreflight = ValidateFinalDestinationAbsence(
                        publicationPlansResult.Value!);
                    if (!finalPreflight.IsSuccess)
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            finalPreflight.ErrorMessage,
                            finalPreflight.ErrorCode);

                    var currentInventory = ReadManifestInventorySnapshotResult();
                    if (!currentInventory.IsSuccess)
                        return currentInventory
                            .MapError<LogicalCapabilityTrainingResult>();
                    if (!ManifestInventoriesMatch(
                            manifestInventory,
                            currentInventory.Value!))
                    {
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            "Generated capability publication snapshot changed.",
                            "GENERATED_CAPABILITY_PUBLICATION_SNAPSHOT_CHANGED");
                    }

                    var abandonedClassifierTransaction =
                        ValidateNoAbandonedClassifierTransactions(
                            publicationPlansResult.Value![0].CanonicalRoot);
                    if (!abandonedClassifierTransaction.IsSuccess)
                    {
                        return abandonedClassifierTransaction
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            "Cancelled.",
                            "CANCELLED");
                    }

                    var publicationPermit = AcquireMutationPermit(
                        admissionCapabilities.Value,
                        "capability_training:final_publication");
                    if (!publicationPermit.IsSuccess || publicationPermit.Value is null)
                        return publicationPermit.MapError<LogicalCapabilityTrainingResult>();
                    using var finalPublicationPermit = cleanupArbiter.Track(publicationPermit.Value);

                    PreparedDomainClassifierPublication?
                        preparedDomainClassifier = null;
                    var domainClassifierPreparation =
                        PrepareDomainClassifierPublication(
                            request,
                            compiledPrompt,
                            compiledText,
                            batchCheckpointDirectory,
                            invocationToken,
                            publicationCleanup);
                    if (domainClassifierPreparation.IsSuccess)
                    {
                        preparedDomainClassifier =
                            domainClassifierPreparation.Value;
                    }
                    else if (StringComparer.Ordinal.Equals(
                                 domainClassifierPreparation.ErrorCode,
                                 "DOMAIN_CLASSIFIER_STATE_INVALID"))
                    {
                        return domainClassifierPreparation
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    else
                    {
                        notes.Add(
                            $"domain_classifier_training=failed; error={domainClassifierPreparation.ErrorCode}");
                    }

                    var registryPreflight = ValidateFinalRegistryPublication(
                        request,
                        registrationPlans,
                        domainAdapterTag,
                        preparedDomainClassifier?.UnitId,
                        manifestInventory,
                        admissionCapabilities.Value);
                    if (!registryPreflight.IsSuccess)
                        return registryPreflight.MapError<LogicalCapabilityTrainingResult>();

                    var nextRegistrationOrders = registryPublicationLease!
                        .PeekNextRegistrationOrders();
                    if (!nextRegistrationOrders.IsSuccess)
                    {
                        return nextRegistrationOrders
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    if (nextRegistrationOrders.Value
                            .NextUnitRegistrationOrder >
                            long.MaxValue - registrationPlans.Count ||
                        nextRegistrationOrders.Value
                            .NextFlowRegistrationOrder >
                            long.MaxValue - registrationPlans.Count)
                    {
                        return RegistryPublicationInvalid()
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    var durableDpoMetricsResult = PrepareDurableDpoMetrics(
                        dpoMetricSourcePaths,
                        publicationPlansResult.Value![0],
                        publicationCleanup);
                    if (!durableDpoMetricsResult.IsSuccess)
                    {
                        return durableDpoMetricsResult
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    var durableDpoMetrics = durableDpoMetricsResult.Value!;
                    evidenceIds.AddRange(durableDpoMetrics.Select(
                        static metric => metric.FinalPath));

                    var preparedPublications = new List<PreparedCapabilityPublication>(
                        registrationPlans.Count);
                    foreach (var plan in registrationPlans)
                    {
                        var branchStopwatch = Stopwatch.StartNew();
                        var saveArtifactsMs = 0.0;
                        var wrapUnitMs = 0.0;
                        var saveManifestMs = 0.0;
                        var subPhaseId = plan.SubPhaseId;
                        var subPhase = plan.SubPhase;
                        var publicationPlan = publicationPlansResult.Value!
                            .Single(candidate => string.Equals(candidate.SubPhaseId, subPhaseId, StringComparison.Ordinal));
                        var checkpointDirectory = publicationPlan.CheckpointDirectory;
                        var unitId = publicationPlan.UnitId;
                        var flowId = BuildGeneratedCapabilityFlowId(domainAdapterTag, subPhaseId);
                        var scenarioId = $"{GeneratedCapabilityScenarioIdPrefix}-{Sanitize(subPhaseId)}";
                        var tokenizerPath = publicationPlan.TokenizerPath;
                        var policyCheckpointPath = publicationPlan.PolicyCheckpointPath;
                        var manifestPath = publicationPlan.ManifestPath;

                        // Persist only to invocation-unique staging before any exposed publication.
                        var saveArtifactsStopwatch = Stopwatch.StartNew();
                        var tokenizerArtifactSha256 =
                            publicationCleanup.CreateAndTrackOwnedFile(
                                publicationPlan.StagingTokenizerPath,
                                tokenizer.Save);
                        var policyArtifactSha256 =
                            publicationCleanup.CreateAndTrackOwnedFile(
                                publicationPlan.StagingPolicyCheckpointPath,
                                policyModel.SaveCheckpointToStream);
                        saveArtifactsStopwatch.Stop();
                        saveArtifactsMs = saveArtifactsStopwatch.Elapsed.TotalMilliseconds;
                        string tokenizerSha256;
                        string policyCheckpointSha256;
                        string runtimeProvenanceSha256;
                        try
                        {
                            tokenizerSha256 = tokenizer.ComputeStateSha256();
                            if (!string.Equals(
                                    tokenizerSha256,
                                    tokenizerArtifactSha256,
                                    StringComparison.Ordinal))
                            {
                                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                    "Generated tokenizer provenance could not be verified.",
                                    "GENERATED_TOKENIZER_PROVENANCE_INVALID");
                            }

                            policyCheckpointSha256 = policyArtifactSha256;
                            var runtimeProvenance = RuntimeProvenance.ComputeSha256(
                                policyModel.Config,
                                BuildCapabilityLoraConfig(),
                                tokenizerSha256,
                                policyCheckpointSha256);
                            if (!runtimeProvenance.IsSuccess)
                            {
                                return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                    "Generated capability runtime provenance could not be computed.",
                                    "GENERATED_RUNTIME_PROVENANCE_INVALID");
                            }

                            runtimeProvenanceSha256 = runtimeProvenance.Value!;
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                        {
                            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                "Generated capability artifact provenance could not be computed.",
                                "GENERATED_CAPABILITY_PROVENANCE_FAILED");
                        }

                        // Capture the exact ordered retrieval memory once. The
                        // same owned corpus constructs the live unit and is
                        // persisted into the signed executable manifest.
                        var knnCorpus = BuildKnnTrainingCorpus(admitted);
                        if (!knnCorpus.IsSuccess || knnCorpus.Value is null)
                        {
                            return knnCorpus
                                .MapError<LogicalCapabilityTrainingResult>();
                        }

                        // Wrap the taught model as an executable transformer-embedding retrieval unit.
                        var wrapUnitStopwatch = Stopwatch.StartNew();
                        var retrievalUnit = TransformerEmbeddingKnnLogicalUnit<string>.Train(
                            unitId,
                            GeneratedCapabilityUnitRole,
                            domainAdapterTag,
                            policyModel,
                            tokenizer,
                            knnCorpus.Value.RuntimeExamples);
                        wrapUnitStopwatch.Stop();
                        wrapUnitMs = wrapUnitStopwatch.Elapsed.TotalMilliseconds;
                        if (!retrievalUnit.IsSuccess)
                        {
                            return retrievalUnit.MapError<LogicalCapabilityTrainingResult>();
                        }

                        var manifest = BuildGeneratedCapabilityManifest(
                            unitId,
                            flowId,
                            scenarioId,
                            tokenizerPath,
                            policyCheckpointPath,
                            plan.TrainingText,
                            plan.ChosenText,
                            plan.AuthorizedReplayExampleId!,
                            domainAdapterTag,
                            subPhase,
                            teachingStack.Value.ParentManifest,
                            teachingStack.Value.TrainingStepCount,
                            teachingStack.Value.Version,
                            trustState,
                            request.OriginalRequest.UserRequest,
                            policyModel.Config,
                            tokenizerSha256,
                            policyCheckpointSha256,
                            runtimeProvenanceSha256,
                            sealedReplayBySubPhaseId[subPhaseId],
                            checked(nextRegistrationOrders.Value
                                .NextUnitRegistrationOrder +
                                preparedPublications.Count),
                            checked(nextRegistrationOrders.Value
                                .NextFlowRegistrationOrder +
                                preparedPublications.Count),
                            knnCorpus.Value.ManifestExamples,
                            knnCorpus.Value.Sha256);
                        var executableManifestSha256 =
                            GeneratedCapabilityExecutableManifestProjector
                                .ProjectSha256(
                                    manifest,
                                    tokenizerArtifactSha256,
                                    policyArtifactSha256);
                        if (!executableManifestSha256.IsSuccess ||
                            executableManifestSha256.Value is null)
                        {
                            return executableManifestSha256
                                .MapError<LogicalCapabilityTrainingResult>();
                        }
                        var executableAttestation =
                            admissionCapabilities.Value.SealExecutableManifest(
                                executableManifestSha256.Value,
                                manifest.ReplayAttestation!);
                        if (!executableAttestation.IsSuccess ||
                            executableAttestation.Value is null)
                        {
                            return executableAttestation
                                .MapError<LogicalCapabilityTrainingResult>();
                        }
                        manifest = manifest with
                        {
                            ExecutableManifestAttestation =
                                executableAttestation.Value with { }
                        };
                        var saveManifestStopwatch = Stopwatch.StartNew();
                        string manifestArtifactSha256;
                        try
                        {
                            manifestArtifactSha256 =
                                publicationCleanup.CreateAndTrackOwnedFile(
                                    publicationPlan.StagingManifestPath,
                                    destination => TeachingRegistryPersistence
                                        .WriteGeneratedCapabilityManifest(
                                            manifest,
                                            destination));
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                        {
                            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                "Generated capability manifest could not be persisted.",
                                "GENERATED_CAPABILITY_MANIFEST_SAVE_FAILED");
                        }
                        saveManifestStopwatch.Stop();
                        saveManifestMs = saveManifestStopwatch.Elapsed.TotalMilliseconds;
                        publicationCleanup.TrackStagedArtifactHashes(
                            publicationPlan,
                            tokenizerSha256,
                            policyCheckpointSha256,
                            manifestArtifactSha256);

                        branchStopwatch.Stop();
                        preparedPublications.Add(new PreparedCapabilityPublication(
                            plan,
                            publicationPlan,
                            flowId,
                            scenarioId,
                            retrievalUnit.Value!,
                            new GeneratedCapabilityFlowFactory(
                                subPhase.PhaseId,
                                subPhaseId,
                                BuildTaskType(subPhase),
                                subPhase.DomainIds,
                                subPhase.PredicateIds,
                                domainAdapterTag,
                                request.OriginalRequest.UserRequest,
                                // Журнал выдач доходит до штатно публикуемого потока здесь:
                                // иначе у обычной выдачи не было бы способа запомнить адресата
                                // будущего исправления. Отсутствие журнала оставляет прежнее
                                // поведение потока без единого изменения.
                                _deliveryLedger),
                            manifest,
                            manifestArtifactSha256,
                            StringComparer.Ordinal.Equals(
                                publicationPlan.SubPhaseId,
                                publicationPlansResult.Value![0].SubPhaseId)
                                ? durableDpoMetrics
                                : [],
                            saveArtifactsMs,
                            wrapUnitMs,
                            saveManifestMs,
                            branchStopwatch.Elapsed.TotalMilliseconds));
                    }

                    FinalPublicationObserver.Value?.Invoke();
                    PublicationStageObserver.Value?.Invoke(
                        "before_directory_publication",
                        preparedPublications[0].Publication.CheckpointDirectory);
                    var filePublication = PublishStagedDirectories(
                        preparedPublications,
                        publicationCleanup);
                    if (!filePublication.IsSuccess)
                    {
                        return filePublication
                            .MapError<LogicalCapabilityTrainingResult>();
                    }
                    var publishedArtifactHandles = filePublication.Value!;
                    using var publishedArtifactHandlesCleanup = cleanupArbiter.Track(publishedArtifactHandles);

                    var publishedIdentity = publishedArtifactHandles
                        .VerifyFinalPathIdentity();
                    if (!publishedIdentity.IsSuccess)
                    {
                        return publishedIdentity
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    var registryTransaction =
                        new CapabilityPublicationTransaction(
                            this,
                            request.Units,
                            request.Flows,
                            preparedDomainClassifier,
                            publicationCleanup,
                            preparedPublications.Count +
                            (preparedDomainClassifier is null ? 0 : 1),
                            preparedPublications.Count);
                    using var registryTransactionCleanup = cleanupArbiter.Track(registryTransaction);
                    foreach (var prepared in preparedPublications)
                    {
                        var registerUnitStopwatch = Stopwatch.StartNew();
                        var unitRegistration = request.Units
                            .RegisterCompensatable<string, string>(
                                prepared.Publication.UnitId,
                                GeneratedCapabilityUnitRole,
                                domainAdapterTag,
                                teachingStack.Value.Version,
                                trustState,
                                isLearned: false,
                                prepared.RetrievalUnit);
                        registerUnitStopwatch.Stop();
                        if (!unitRegistration.IsSuccess)
                        {
                            return unitRegistration
                                .MapError<LogicalCapabilityTrainingResult>();
                        }

                        if (!registryTransaction.Track(
                                unitRegistration.Value!.Token))
                        {
                            return PublicationEnlistmentFailed();
                        }
                        if (!MatchesSignedUnitRegistration(
                                unitRegistration.Value.Descriptor,
                                prepared.Manifest))
                        {
                            return RegistryPublicationInvalid()
                                .MapError<LogicalCapabilityTrainingResult>();
                        }
                        try
                        {
                            RegistryPublicationObserver.Value?.Invoke(
                                "after_unit_registration");
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(
                                exception))
                        {
                            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                "Generated capability registry publication failed.",
                                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED");
                        }

                        var subPhase = prepared.Plan.SubPhase;
                        var registerFlowStopwatch = Stopwatch.StartNew();
                        var flowRegistration = request.Flows
                            .RegisterFactoryCompensatable(
                                prepared.FlowId,
                                prepared.ScenarioId,
                                teachingStack.Value.Version,
                                subPhase.PhaseId,
                                prepared.Plan.SubPhaseId,
                                BuildTaskType(subPhase),
                                subPhase.DomainIds,
                                prepared.FlowFactory);
                        registerFlowStopwatch.Stop();
                        if (!flowRegistration.IsSuccess)
                        {
                            return flowRegistration
                                .MapError<LogicalCapabilityTrainingResult>();
                        }

                        if (!registryTransaction.Track(
                                flowRegistration.Value!.Token))
                        {
                            return PublicationEnlistmentFailed();
                        }
                        if (!MatchesSignedFlowRegistration(
                                flowRegistration.Value.Descriptor,
                                prepared.Manifest))
                        {
                            return RegistryPublicationInvalid()
                                .MapError<LogicalCapabilityTrainingResult>();
                        }
                        registeredUnitIds.Add(
                            unitRegistration.Value.Descriptor.UnitId);
                        registeredSubPhaseIds.Add(prepared.Plan.SubPhaseId);
                        registeredFlowIds.Add(
                            flowRegistration.Value.Descriptor.FlowId);
                        evidenceIds.Add(prepared.Publication.TokenizerPath);
                        evidenceIds.Add(prepared.Publication.PolicyCheckpointPath);
                        evidenceIds.Add(prepared.Publication.ManifestPath);
                        evidenceIds.Add(
                            unitRegistration.Value.Descriptor.UnitId);
                        evidenceIds.Add(
                            flowRegistration.Value.Descriptor.FlowId);
                        branchTimings.Add(new LogicalCapabilityTrainingBranchTiming(
                            prepared.Plan.SubPhaseId,
                            prepared.Publication.UnitId,
                            teachingStack.Value.ParentManifest?.UnitId,
                            teachingStack.Value.TrainingStepCount,
                            teachingStack.Value.UsedWarmCache,
                            sharedBuildStackMs,
                            sharedReferenceModelMs,
                            sharedDpoMs,
                            prepared.SaveArtifactsMs,
                            prepared.WrapUnitMs,
                            prepared.SaveManifestMs,
                            registerUnitStopwatch.Elapsed.TotalMilliseconds,
                            registerFlowStopwatch.Elapsed.TotalMilliseconds,
                            prepared.PreparationMs +
                            registerUnitStopwatch.Elapsed.TotalMilliseconds +
                            registerFlowStopwatch.Elapsed.TotalMilliseconds +
                            sharedBuildStackMs +
                            sharedReferenceModelMs +
                            sharedDpoMs));
                    }

                    if (preparedDomainClassifier is not null)
                    {
                        var classifierRegistration = request.Units
                            .RegisterCompensatable<
                                DomainClassifierRequest,
                                DomainClassification>(
                                preparedDomainClassifier.UnitId,
                                CompiledDomainClassifierUnit.Role,
                                CompiledDomainClassifierUnit.DomainTag,
                                GeneratedCapabilityVersion,
                                LogicalUnitTrustState.Probationary,
                                isLearned: true,
                                preparedDomainClassifier.Unit);
                        if (!classifierRegistration.IsSuccess)
                        {
                            return classifierRegistration
                                .MapError<LogicalCapabilityTrainingResult>();
                        }

                        if (!registryTransaction.Track(
                                classifierRegistration.Value!.Token))
                        {
                            return PublicationEnlistmentFailed();
                        }
                        registeredUnitIds.Add(
                            classifierRegistration.Value.Descriptor.UnitId);
                        evidenceIds.Add(
                            $"domain_classifier_training:{classifierRegistration.Value.Descriptor.UnitId}");
                        notes.Add(
                            $"domain_classifier_training=registered; unit_id={classifierRegistration.Value.Descriptor.UnitId}");
                        try
                        {
                            RegistryPublicationObserver.Value?.Invoke(
                                "after_domain_classifier_registration");
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(
                                exception))
                        {
                            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                "Generated capability registry publication failed.",
                                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED");
                        }
                    }

                    var cachePublication = registryTransaction.PublishCache(
                        manifestInventory,
                        preparedPublications,
                        policyModel,
                        tokenizer);
                    if (!cachePublication.IsSuccess)
                    {
                        return cachePublication
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    if (preparedDomainClassifier is not null)
                    {
                        var classifierStatePublication =
                            registryTransaction.PublishClassifierState(
                                preparedDomainClassifier);
                        if (!classifierStatePublication.IsSuccess)
                        {
                            return classifierStatePublication
                                .MapError<LogicalCapabilityTrainingResult>();
                        }


                        try
                        {
                            RegistryPublicationObserver.Value?.Invoke(
                                "after_classifier_state_publication");
                        }
                        catch (Exception exception)
                            when (SnapshotExceptionGraphClassifier.IsNonFatal(
                                exception))
                        {
                            return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                                "Generated capability registry publication failed.",
                                "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED");
                        }
                    }

                    stopwatch.Stop();
                    var publishedResult = BuildTrainingResult(
                        registeredFlowIds,
                        registeredSubPhaseIds,
                        registeredUnitIds,
                        evidenceIds,
                        notes,
                        domainAdapterTag,
                        branchPlans.Count,
                        batchedDpoStepCount,
                        branchTimings,
                        stopwatch.Elapsed.TotalMilliseconds);
                    var publishedEnvelope =
                        OperationResult<LogicalCapabilityTrainingResult>.Ok(
                            publishedResult);
                    try
                    {
                        RegistryPublicationObserver.Value?.Invoke(
                            "after_registry_publication");
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(
                            exception))
                    {
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            "Generated capability registry publication failed.",
                            "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED");
                    }

                    var transientCleanup = publicationCleanup
                        .PrepareTransientCleanupForCommit();
                    if (!transientCleanup.IsSuccess)
                    {
                        return transientCleanup
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    publishedIdentity = publishedArtifactHandles
                        .VerifyFinalPathIdentity();
                    if (!publishedIdentity.IsSuccess)
                    {
                        return publishedIdentity
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    try
                    {
                        RegistryPublicationObserver.Value?.Invoke(
                            "before_publication_commit");
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(
                            exception))
                    {
                        return OperationResult<LogicalCapabilityTrainingResult>.Fail(
                            "Generated capability registry publication failed.",
                            "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_FAILED");
                    }

                    publishedIdentity = publishedArtifactHandles
                        .VerifyFinalPathIdentity();
                    if (!publishedIdentity.IsSuccess)
                    {
                        return publishedIdentity
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    var classifierCommitReadiness = registryTransaction
                        .VerifyClassifierStateReadyToCommit();
                    if (!classifierCommitReadiness.IsSuccess)
                    {
                        return classifierCommitReadiness
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    var classifierBackupFinalization = registryTransaction
                        .FinalizeClassifierBackupForCommit();
                    if (!classifierBackupFinalization.IsSuccess)
                    {
                        return classifierBackupFinalization
                            .MapError<LogicalCapabilityTrainingResult>();
                    }

                    registryTransaction.Commit();
                    publicationCleanup.Commit();
                    return publishedEnvelope;
                }
            }
        }

        stopwatch.Stop();

        // Report whether anything executable was actually registered.
        return OperationResult<LogicalCapabilityTrainingResult>.Ok(
            BuildTrainingResult(
                registeredFlowIds,
                registeredSubPhaseIds,
                registeredUnitIds,
                evidenceIds,
                notes,
                domainAdapterTag,
                branchPlans.Count,
                batchedDpoStepCount,
                branchTimings,
                stopwatch.Elapsed.TotalMilliseconds));
    }

    private static OperationResult<LogicalCapabilityTrainingRequest>
        SnapshotTrainingRequest(LogicalCapabilityTrainingRequest request)
    {
        try
        {
            if (request.OriginalRequest is null ||
                request.OriginalRequest.Metadata is null ||
                string.IsNullOrWhiteSpace(request.RunId) ||
                !StringComparer.Ordinal.Equals(request.RunId, request.RunId.Trim()) ||
                request.RunId.Length > 16_384 ||
                request.OriginalRequest.UserRequest is null ||
                request.OriginalRequest.UserRequest.Length > 16_384 ||
                request.KnowledgeEdgeBranches is null)
            {
                return InvalidTrainingSnapshot();
            }

            var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var pair in request.OriginalRequest.Metadata)
            {
                if (metadata.Count == MaximumMetadataCount ||
                    pair.Key is null || pair.Key.Length > 16_384 ||
                    pair.Value is null ||
                    !metadata.TryAdd(pair.Key, CloneMetadataValue(pair.Value)))
                {
                    return InvalidTrainingSnapshot();
                }
            }

            var original = request.OriginalRequest with
            {
                Flags = (request.OriginalRequest.Flags ?? new LogicalExecutionFlags()) with { },
                Metadata = new ReadOnlyDictionary<string, object>(metadata)
            };
            var branches = request.KnowledgeEdgeBranches
                .Take(MaximumTrainingBranchCount + 1)
                .Select(CloneFlowResult)
                .ToArray();
            if (branches.Length is 0 or > MaximumTrainingBranchCount)
                return InvalidTrainingSnapshot();
            if (branches.Any(static branch => branch is null))
                return InvalidTrainingSnapshot();
            if (branches.Any(branch =>
                    branch.TrainingContentProvenance is null ||
                    !ValidateTrainingBranchProvenance(
                        request with { KnowledgeEdgeBranches = branches! },
                        branch)))
            {
                return OperationResult<LogicalCapabilityTrainingRequest>.Fail(
                    "Logical training branch provenance is invalid.",
                    TrainingContentProvenanceInvalidErrorCode);
            }

            return OperationResult<LogicalCapabilityTrainingRequest>.Ok(
                new LogicalCapabilityTrainingRequest(
                    request.RunId,
                    original,
                    branches!,
                    request.Topology,
                    request.Flows,
                    request.Units)
                {
                    Domain = request.Domain
                });
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidTrainingSnapshot();
        }
    }

    private static bool ValidateTrainingBranchProvenance(
        LogicalCapabilityTrainingRequest request,
        LogicalFlowResult branch)
    {
        try
        {
            var provenance = branch.TrainingContentProvenance;
            var subPhaseId = ExtractSubPhaseId(branch);
            if (provenance is null || !provenance.IsCanonical ||
                !provenance.BindsTrainingInputs ||
                provenance.SourceKind != LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher ||
                string.IsNullOrWhiteSpace(subPhaseId))
            {
                return false;
            }
            var resolved = request.Topology.ResolveSubPhase(subPhaseId);
            if (!resolved.IsSuccess || resolved.Value?.SubPhase is null)
                return false;
            var subPhase = resolved.Value.SubPhase;
            return ValidateTrainingBranchProvenance(
                request,
                branch,
                subPhase);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool ValidateTrainingBranchProvenance(
        LogicalCapabilityTrainingRequest request,
        LogicalFlowResult branch,
        LogicalSubPhase subPhase)
    {
        try
        {
            var provenance = branch.TrainingContentProvenance;
            if (provenance is null || !provenance.IsCanonical ||
                !provenance.BindsTrainingInputs)
            {
                return false;
            }
            return provenance.Attests(
                branch,
                new LogicalTrainingContentDeterministicInputs(
                    request.OriginalRequest.UserRequest,
                    request.Domain.ToString(),
                    subPhase.PhaseId,
                    subPhase.SubPhaseId,
                    subPhase.Name,
                    subPhase.Purpose,
                    subPhase.PredicateIds,
                    subPhase.DomainIds));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static object CloneMetadataValue(object value)
        => value switch
        {
            string text => text,
            string[] values => SnapshotMetadataArray(values),
            int[] values => SnapshotMetadataArray(values),
            float[] values => SnapshotMetadataArray(values),
            double[] values => SnapshotMetadataArray(values),
            byte[] values => SnapshotMetadataArray(values),
            IReadOnlyDictionary<string, string> values =>
                SnapshotMetadataDictionary(values),
            _ => value
        };

    private static IReadOnlyDictionary<string, string>
        SnapshotMetadataDictionary(
            IReadOnlyDictionary<string, string> values)
    {
        var snapshot = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            if (snapshot.Count == MaximumMetadataCount ||
                pair.Key is null || pair.Value is null ||
                pair.Key.Length > 16_384 || pair.Value.Length > 16_384 ||
                !snapshot.TryAdd(pair.Key, pair.Value))
            {
                throw new InvalidOperationException();
            }
        }
        return new ReadOnlyDictionary<string, string>(snapshot);
    }

    private static T[] SnapshotMetadataArray<T>(IEnumerable<T> values)
    {
        var snapshot = values.Take(4_097).ToArray();
        if (snapshot.Length > 4_096 ||
            snapshot.OfType<string>().Any(static value =>
                value is null || value.Length > 16_384) ||
            snapshot.OfType<float>().Any(static value => !float.IsFinite(value)) ||
            snapshot.OfType<double>().Any(static value => !double.IsFinite(value)))
        {
            throw new InvalidOperationException();
        }
        return snapshot;
    }

    private static LogicalFlowResult CloneFlowResult(LogicalFlowResult source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source.Summary);
        ArgumentNullException.ThrowIfNull(source.Metrics);
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source.Metrics)
        {
            if (metrics.Count == MaximumMetadataCount ||
                pair.Key is null || pair.Value is null ||
                pair.Key.Length > 16_384 || pair.Value.Length > 16_384 ||
                !metrics.TryAdd(pair.Key, pair.Value))
                throw new InvalidOperationException("Invalid branch metrics.");
        }

        DynamicGraphEdgeTargetHandoff? handoff = null;
        if (source.EdgeTargetHandoff is not null)
        {
            var candidate = source.EdgeTargetHandoff;
            handoff = new DynamicGraphEdgeTargetHandoff(
                candidate.RunId,
                candidate.EdgeId,
                candidate.FromNodeId,
                candidate.DomainTag,
                candidate.Kind,
                candidate.ParallelEligible,
                candidate.OrderedTargets.Select(static target => target with { }).ToArray(),
                candidate.KnowledgeEdge,
                candidate.AuthoritySource,
                candidate.UpstreamCandidateRefs.ToArray(),
                candidate.UpstreamOutputHash,
                candidate.ProjectionRule);
        }

        return new LogicalFlowResult(
            source.FlowId,
            source.ScenarioId,
            source.OverallVerdict,
            source.FinalHumanResult,
            source.Summary with { },
            new ReadOnlyDictionary<string, string>(metrics),
            source.EvidenceIds?.ToArray() ?? throw new InvalidOperationException(),
            source.Warnings?.ToArray() ?? throw new InvalidOperationException())
        {
            TransitionSignals = source.TransitionSignals?
                .Select(static signal => signal with
                {
                    EvidenceIds = signal.EvidenceIds?.ToArray()
                        ?? throw new InvalidOperationException()
                })
                .ToArray() ?? throw new InvalidOperationException(),
            EdgeTargetHandoff = handoff,
            TrainingContentProvenance =
                source.TrainingContentProvenance?.Snapshot()
        };
    }

    private static LogicalSubPhase CloneSubPhase(LogicalSubPhase source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source with
        {
            DomainIds = SnapshotSubPhaseValues(source.DomainIds),
            PredicateIds = SnapshotSubPhaseValues(source.PredicateIds),
            SourceIds = SnapshotSubPhaseValues(source.SourceIds)
        };
    }

    private static string[] SnapshotSubPhaseValues(
        IReadOnlyList<string>? values)
    {
        if (values is null)
            throw new InvalidOperationException();
        var snapshot = values.Take(MaximumMetadataCount + 1).ToArray();
        if (snapshot.Length > MaximumMetadataCount ||
            snapshot.Any(static value =>
                value is null || value.Length > 16_384))
        {
            throw new InvalidOperationException();
        }
        return snapshot;
    }

    private OperationResult<GeneratedCapabilityCheckpointManifest[]>
        ReadManifestInventorySnapshotResult()
    {
        var inventory = TeachingRegistryPersistence
            .ReadGeneratedCapabilityManifestInventory(_checkpointRoot);
        if (!inventory.IsSuccess || inventory.Value is null)
            return inventory.MapError<GeneratedCapabilityCheckpointManifest[]>();
        try
        {
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(
                inventory.Value.ByUnitId.Values
                    .Select(CloneManifest)
                    .OrderBy(static manifest => manifest.CreatedAtUtc)
                    .ThenBy(static manifest => manifest.UnitId, StringComparer.Ordinal)
                    .ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Fail(
                "Generated capability replay inventory is invalid.",
                "LOGICAL_CAPABILITY_TRAINING_REPLAY_INVALID");
        }
    }

    private static GeneratedCapabilityCheckpointManifest CloneManifest(
        GeneratedCapabilityCheckpointManifest source)
    {
        var knnExamples = SnapshotKnnExamples(source.KnnExamples);
        return source with
        {
            PredicateIds = SnapshotManifestValues(source.PredicateIds),
            DomainTags = SnapshotManifestValues(source.DomainTags),
            LoraTargetModules = SnapshotManifestValues(
                source.LoraTargetModules),
            KnnExamples = knnExamples,
            ExecutableManifestAttestation =
                source.ExecutableManifestAttestation is null
                    ? null
                    : source.ExecutableManifestAttestation with { }
        };
    }

    private static IReadOnlyList<GeneratedCapabilityKnnExample>?
        SnapshotKnnExamples(
            IReadOnlyList<GeneratedCapabilityKnnExample>? examples)
    {
        if (examples is null)
            return null;

        var projected = GeneratedCapabilityKnnCorpusProjector.Project(examples);
        if (!projected.IsSuccess || projected.Value is null)
            throw new InvalidOperationException();
        return projected.Value.Examples;
    }

    private static string[] SnapshotManifestValues(
        IReadOnlyList<string>? values)
    {
        if (values is null)
            throw new InvalidOperationException();
        var snapshot = values.Take(MaximumMetadataCount + 1).ToArray();
        if (snapshot.Length > MaximumMetadataCount ||
            snapshot.Any(static value =>
                value is null || value.Length > 16_384))
        {
            throw new InvalidOperationException();
        }
        return snapshot;
    }

    private static bool ManifestInventoriesMatch(
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> expected,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> current)
    {
        if (expected.Count != current.Count)
            return false;

        var expectedDigests = expected
            .OrderBy(static manifest => manifest.UnitId, StringComparer.Ordinal)
            .Select(static manifest => ComputeBytesSha256(
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(manifest)))
            .ToArray();
        var currentDigests = current
            .OrderBy(static manifest => manifest.UnitId, StringComparer.Ordinal)
            .Select(static manifest => ComputeBytesSha256(
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(manifest)))
            .ToArray();
        return expectedDigests.SequenceEqual(
            currentDigests,
            StringComparer.Ordinal);
    }

    private OperationResult<GeneratedCapabilityCheckpointManifest[]>
        SelectAndValidateReplaySnapshot(
            IReadOnlyList<GeneratedCapabilityCheckpointManifest> inventory,
            ILogicalTopologyRegistry topology,
            string domainAdapterTag,
            string? compiledText,
            bool useReplayFusionRanking)
    {
        try
        {
            if (_admissionAuthority is null)
            {
                return OperationResult<GeneratedCapabilityCheckpointManifest[]>
                    .Fail(
                        "Logical capability training admission authority is required.",
                        AdmissionAuthorityRequiredErrorCode);
            }

            var verifiedCandidates = new List<
                GeneratedCapabilityCheckpointManifest>();
            foreach (var source in inventory)
            {
                if (!StringComparer.Ordinal.Equals(
                        source.DomainTag,
                        domainAdapterTag))
                {
                    continue;
                }
                if (verifiedCandidates.Count == MaximumTrainingBranchCount ||
                    !TrySnapshotReplayManifest(source, out var manifest))
                {
                    return InvalidReplay();
                }

                var resolved = topology.ResolveSubPhase(manifest.SubPhaseId);
                if (!resolved.IsSuccess || resolved.Value?.SubPhase is null)
                    return InvalidReplay();
                var subPhase = CloneSubPhase(resolved.Value.SubPhase);
                if (string.IsNullOrWhiteSpace(manifest.TrainingText) ||
                    !StringComparer.Ordinal.Equals(
                        manifest.TaskType,
                        BuildTaskType(subPhase)) ||
                    !manifest.PredicateIds.SequenceEqual(
                        subPhase.PredicateIds,
                        StringComparer.Ordinal) ||
                    !manifest.DomainTags.SequenceEqual(
                        subPhase.DomainIds,
                        StringComparer.Ordinal))
                {
                    return InvalidReplay();
                }

                var attestation = manifest.ReplayAttestation;
                if (attestation is null)
                {
                    return OperationResult<
                        GeneratedCapabilityCheckpointManifest[]>.Fail(
                            "A server replay attestation is required for every persisted replay row.",
                            ReplayAttestationRequiredErrorCode);
                }

                var payload = LogicalCapabilityTrainingPreferenceProjector
                    .ProjectReplayPayload(
                    attestation.SourceBindings.Values,
                    attestation.ReplaySealGroupSha256,
                    manifest.DomainTag,
                    manifest.SubPhaseId,
                    manifest.ExampleId,
                    manifest.TrainingText,
                    manifest.OutputText,
                    manifest.PredicateIds,
                    manifest.DomainTags);
                if (!payload.IsSuccess || payload.Value is null ||
                    !attestation.IsCanonical ||
                    !attestation.Matches(payload.Value.Descriptor))
                {
                    return InvalidReplay();
                }

                var verified = _admissionAuthority.VerifyReplay(attestation);
                if (!verified.IsSuccess || !verified.Value)
                {
                    return OperationResult<
                        GeneratedCapabilityCheckpointManifest[]>.Fail(
                            verified.ErrorMessage,
                            verified.ErrorCode);
                }
                verifiedCandidates.Add(manifest);
            }

            var candidates = verifiedCandidates
                .OrderByDescending(static manifest => manifest.CreatedAtUtc)
                .ToArray();
            var selected = !useReplayFusionRanking ||
                           string.IsNullOrWhiteSpace(compiledText)
                ? candidates.Take(3).Select(CloneManifest).ToArray()
                : ReplayFusionRanker.Rank(candidates, compiledText, topK: 3)
                    .Select(CloneManifest)
                    .ToArray();
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(
                selected.Select(CloneManifest).ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplay();
        }
    }

    private static OperationResult<GeneratedCapabilityCheckpointManifest[]> InvalidReplay()
        => OperationResult<GeneratedCapabilityCheckpointManifest[]>.Fail(
            "Generated capability replay cannot be reconstructed deterministically.",
            "LOGICAL_CAPABILITY_TRAINING_REPLAY_INVALID");

    private static bool TrySnapshotReplayManifest(
        GeneratedCapabilityCheckpointManifest? source,
        out GeneratedCapabilityCheckpointManifest snapshot)
    {
        snapshot = null!;
        try
        {
            if (source is null ||
                !TeachingRegistryPersistence
                    .HasBoundedExecutablePredicateIds(source.PredicateIds) ||
                source.DomainTags is null ||
                source.LoraTargetModules is null ||
                source.SchemaVersion !=
                    GeneratedCapabilityCheckpointManifest
                        .CurrentSchemaVersion ||
                source.UnitRegistrationOrder is null or <= 0 ||
                source.FlowRegistrationOrder is null or <= 0 ||
                source.ExecutableManifestAttestation is not
                    { IsCanonical: true })
            {
                return false;
            }
            var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
                source.KnnExamples);
            if (!knnCorpus.IsSuccess || knnCorpus.Value is null ||
                !RuntimeProvenance.IsSha256Hex(source.KnnCorpusSha256) ||
                !RuntimeProvenance.FixedTimeSha256Equals(
                    source.KnnCorpusSha256!,
                    knnCorpus.Value.Sha256))
            {
                return false;
            }
            var predicates = source.PredicateIds
                .Take(MaximumMetadataCount + 1)
                .ToArray();
            var domains = source.DomainTags
                .Take(MaximumMetadataCount + 1)
                .ToArray();
            var modules = source.LoraTargetModules
                .Take(MaximumMetadataCount + 1)
                .ToArray();
            if (predicates.Length > MaximumMetadataCount ||
                domains.Length > MaximumMetadataCount ||
                modules.Length > MaximumMetadataCount ||
                predicates.Any(static value =>
                    value is null || value.Length > 16_384) ||
                domains.Any(static value =>
                    value is null || value.Length > 16_384) ||
                modules.Any(static value =>
                    value is null || value.Length > 16_384))
            {
                return false;
            }
            snapshot = source with
            {
                PredicateIds = predicates,
                DomainTags = domains,
                LoraTargetModules = modules,
                KnnExamples = knnCorpus.Value.Examples,
                KnnCorpusSha256 = knnCorpus.Value.Sha256,
                ReplayAttestation = source.ReplayAttestation is null
                    ? null
                    : source.ReplayAttestation with { },
                ExecutableManifestAttestation =
                    source.ExecutableManifestAttestation is null
                        ? null
                        : source.ExecutableManifestAttestation with { }
            };
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            snapshot = null!;
            return false;
        }
    }

    private static OperationResult<
        IReadOnlyDictionary<string,
            LogicalCapabilityTrainingReplayAttestation>>
        SealCurrentReplayAttestations(
            ILogicalCapabilityTrainingAdmissionLease lease,
            IReadOnlyList<TrainingBranchPlan> registrationPlans,
            string domainAdapterTag)
    {
        try
        {
            var attestations = new Dictionary<string,
                LogicalCapabilityTrainingReplayAttestation>(
                StringComparer.Ordinal);
            foreach (var plan in registrationPlans)
            {
                var descriptor = plan.AuthorizedReplayDescriptor;
                if (descriptor is null || !descriptor.IsCanonical)
                {
                    return InvalidReplay().MapError<IReadOnlyDictionary<string,
                        LogicalCapabilityTrainingReplayAttestation>>();
                }

                var sealedReplay = lease.SealReplay(
                    descriptor);
                if (!sealedReplay.IsSuccess || sealedReplay.Value is null)
                {
                    return sealedReplay.MapError<IReadOnlyDictionary<string,
                        LogicalCapabilityTrainingReplayAttestation>>();
                }
                if (!sealedReplay.Value.IsCanonical ||
                    !sealedReplay.Value.Matches(descriptor) ||
                    !attestations.TryAdd(
                        plan.SubPhaseId,
                        sealedReplay.Value with { }))
                {
                    return InvalidReplay().MapError<IReadOnlyDictionary<string,
                        LogicalCapabilityTrainingReplayAttestation>>();
                }
            }

            return OperationResult<IReadOnlyDictionary<string,
                LogicalCapabilityTrainingReplayAttestation>>.Ok(attestations);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplay().MapError<IReadOnlyDictionary<string,
                LogicalCapabilityTrainingReplayAttestation>>();
        }
    }

    private static LogicalDpoPreferenceExample[] SnapshotPreferences(
        IEnumerable<LogicalDpoPreferenceExample> preferences)
    {
        var snapshot = preferences
            .Take(LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount + 1)
            .ToArray();
        if (snapshot.Length is 0 or >
            LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount)
        {
            throw new InvalidOperationException();
        }
        return snapshot.Select(preference =>
        {
            if (preference.Prompt is null || preference.Chosen is null ||
                preference.Rejected is null ||
                Encoding.UTF8.GetByteCount(preference.Prompt) >
                    MaximumTrainingTextLength ||
                Encoding.UTF8.GetByteCount(preference.Chosen) >
                    MaximumTrainingTextLength ||
                Encoding.UTF8.GetByteCount(preference.Rejected) >
                    MaximumTrainingTextLength)
            {
                throw new InvalidOperationException();
            }
            var vector = preference.TeachingVector?
                .Take(MaximumTrainingVectorLength + 1)
                .ToArray();
            if (vector is null ||
                vector.Length is 0 or > MaximumTrainingVectorLength ||
                vector.Any(static value => !float.IsFinite(value)))
            {
                throw new InvalidOperationException();
            }
            return preference with
            {
                TeachingVector = vector,
                TrainingContentProvenance =
                    preference.TrainingContentProvenance?.Snapshot(),
                ReplayDescriptor = preference.ReplayDescriptor is null
                    ? null
                    : preference.ReplayDescriptor with { },
                ReplayAttestation = preference.ReplayAttestation is null
                    ? null
                    : preference.ReplayAttestation with { }
            };
        }).ToArray();
    }

    internal static OperationResult<LogicalCapabilityTrainingAdmissionBatch>
        BuildAdmissionBatch(
            string runId,
            string domainAdapterTag,
            IReadOnlyList<LogicalDpoPreferenceExample> preferences)
        => LogicalCapabilityTrainingAdmissionBatchFactory.CreateAttested(
            runId,
            domainAdapterTag,
            preferences?
                .Select(static preference =>
                    new LogicalCapabilityTrainingAdmissionPreference(
                        preference.Prompt,
                        preference.Chosen,
                        preference.Rejected,
                        preference.TeachingVector,
                        preference.TrainingContentProvenance,
                        preference.ReplayDescriptor,
                        preference.ReplayAttestation))
                .ToArray());

    private static OperationResult<IDisposable> AcquireMutationPermit(
        LogicalCapabilityTrainingAdmissionCapabilities capabilities,
        string phase)
    {
        MutationBoundaryObserver.Value?.Invoke(phase);
        var permit = capabilities.AcquireMutationPermit();
        if (!permit.IsSuccess)
            return permit;
        return permit.Value is null
            ? OperationResult<IDisposable>.Fail(
                "Logical capability training mutation permit is required.",
                "LOGICAL_CAPABILITY_TRAINING_MUTATION_PERMIT_REQUIRED")
            : permit;
    }

    private static OperationResult<IDisposable> AcquireRootPublicationLease(
        string canonicalRoot)
    {
        var normalizedRoot = NormalizeRootIdentity(canonicalRoot);
        Mutex? mutex = null;
        RootPublicationLease? lease = null;
        var mutexHeld = false;
        try
        {
            var mutexName = BuildRootPublicationMutexName(normalizedRoot);
            mutex = new Mutex(initiallyOwned: false, mutexName);
            lease = new RootPublicationLease(mutex);
            var result = OperationResult<IDisposable>.Ok(lease);
            var abandoned = OperationResult<IDisposable>.Fail(
                "Generated capability publication ownership was abandoned.",
                "GENERATED_CAPABILITY_PUBLICATION_LEASE_ABANDONED");
            try
            {
                RootLeaseWaitObserver.Value?.Invoke();
                mutexHeld = mutex.WaitOne(RootPublicationLeaseTimeout);
            }
            catch (AbandonedMutexException abandonedException)
            {
                mutexHeld = true;
                Exception? firstFatal = null;
                var failures = new List<Exception> { abandonedException };
                try
                {
                    AbandonedRootLeaseObserver.Value?.Invoke();
                }
                catch (Exception exception)
                {
                    CaptureCleanupException(
                        exception,
                        ref firstFatal,
                        failures);
                }
                AttemptCleanup(lease.Dispose, ref firstFatal, failures);
                mutexHeld = false;
                mutex = null;
                ThrowFirstFatalWithFailures(firstFatal, failures);
                return abandoned;
            }

            if (!mutexHeld)
            {
                mutex.Dispose();
                return OperationResult<IDisposable>.Fail(
                    "Generated capability publication lease acquisition timed out.",
                    "GENERATED_CAPABILITY_PUBLICATION_LEASE_TIMEOUT");
            }
            return result;
        }
        catch (Exception exception)
        {
            Exception? firstFatal = null;
            CaptureCleanupException(exception, ref firstFatal);
            List<Exception> failures;
            try
            {
                failures = [exception];
            }
            catch
            {
                if (firstFatal is not null)
                    ThrowExactException(firstFatal);
                throw;
            }
            if (lease is not null)
            {
                AttemptCleanup(lease.Dispose, ref firstFatal, failures);
                mutex = null;
            }
            else if (mutex is not null)
            {
                if (mutexHeld)
                    AttemptCleanup(
                        mutex.ReleaseMutex,
                        ref firstFatal,
                        failures);
                AttemptCleanup(mutex.Dispose, ref firstFatal, failures);
            }
            ThrowFirstFatalWithFailures(firstFatal, failures);
            return OperationResult<IDisposable>.Fail(
                "Generated capability publication lease could not be acquired.",
                "GENERATED_CAPABILITY_PUBLICATION_LEASE_FAILED");
        }
    }

    private static string BuildRootPublicationMutexName(string normalizedRoot)
    {
        var identity = ComputeBytesSha256(
            Encoding.UTF8.GetBytes(normalizedRoot));
        return OperatingSystem.IsWindows()
            ? $"Global\\xiigen-capability-publication-{identity}"
            : $"xiigen-capability-publication-{identity}";
    }

    private static string NormalizeRootIdentity(string canonicalRoot)
    {
        var fullPath = Path.GetFullPath(canonicalRoot);
        var root = Path.GetPathRoot(fullPath);
        var normalized = StringComparer.Ordinal.Equals(fullPath, root)
            ? fullPath
            : fullPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        return OperatingSystem.IsWindows()
            ? normalized.ToUpperInvariant()
            : normalized;
    }

    private static LogicalCapabilityTrainingResult BuildTrainingResult(
        IReadOnlyList<string> registeredFlowIds,
        IReadOnlyList<string> registeredSubPhaseIds,
        IReadOnlyList<string> registeredUnitIds,
        IReadOnlyList<string> evidenceIds,
        IReadOnlyList<string> notes,
        string domainAdapterTag,
        int batchedBranchCount,
        int batchedDpoStepCount,
        IReadOnlyList<LogicalCapabilityTrainingBranchTiming> branchTimings,
        double trainingTimeMs)
        => new(
            registeredUnitIds.Count > 0 || registeredFlowIds.Count > 0,
            registeredFlowIds.Distinct(StringComparer.Ordinal).ToArray(),
            registeredSubPhaseIds.Distinct(StringComparer.Ordinal).ToArray(),
            registeredUnitIds.Distinct(StringComparer.Ordinal).ToArray(),
            trainingTimeMs,
            evidenceIds.Distinct(StringComparer.Ordinal).ToArray(),
            notes.Count == 0
                ? "Generated probationary retrieval capability and retryable flow."
                : string.Join(" | ", notes))
        {
            Domain = domainAdapterTag,
            BatchedBranchCount = batchedBranchCount,
            BatchedDpoStepCount = batchedDpoStepCount,
            BranchTimings = branchTimings.ToArray()
        };

    private static OperationResult<LogicalCapabilityTrainingRequest>
        InvalidTrainingSnapshot()
        => OperationResult<LogicalCapabilityTrainingRequest>.Fail(
            "Logical capability training request snapshot is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_SNAPSHOT_INVALID");

    // Execute one DPO preference row through the logical teaching unit.
    private static async Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteDpoTeachingAsync(
        string runId,
        LlmModel policyModel,
        LlmModel referenceModel,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        // Пары предпочтений приходят ТОЛЬКО носителем допущенного материала: довода с сырым
        // набором пар у этого входа больше нет, поэтому подать сюда непроверенный набор нечем.
        CapabilityMaterialLineageGuard.AdmittedTrainingMaterial admittedMaterial,
        string? parentPolicyCheckpointPath,
        LogicalDpoParentProvenance? parentProvenance,
        int maxDpoStepCount,
        int convergenceWindowSteps,
        LogicalCapabilityTrainingAdmissionCapabilities admissionCapabilities,
        Func<DpoTrainerArtifactOwnershipProof, bool>
            tryAcceptArtifactOwnership,
        CancellationToken cancellationToken)
    {
        // BuildAdmissionBatch admitted these final owned rows by exact
        // prompt/chosen/rejected/teaching-vector digest. Token packing is a
        // deterministic derivation under this run's tokenizer; preparing it
        // exactly once here preserves the pre-existing carrier's authority and
        // gives the logical unit the same optimizer objects that were derived
        // from that admitted content.
        //
        // Копия набора пар берётся из носителя ОДИН раз и дальше используется и для подготовки
        // оптимизатора, и для самого запроса обучения. Один раз, а не дважды: прежде в оба места
        // уходил один и тот же объект набора, и опознание точного материала пары опирается именно
        // на это. Две разные копии изменили бы наблюдаемое поведение.
        // Чтение сверяющее: если содержание носителя после допуска изменилось — например кто-то
        // поменял число в учебном векторе уже допущенной пары, — выдачи не будет вовсе.
        var admittedPreferences = admittedMaterial.ReadPreferencesForLearner();
        // Отказ тождества передаётся вызывающему как есть, до любой подготовки оптимизатора.
        if (!admittedPreferences.IsSuccess || admittedPreferences.Value is null)
        {
            return admittedPreferences.MapError<LogicalUnitResult<
                LogicalDpoTrainingResult>>();
        }

        // Дальше используется ровно эта одна выданная копия, как и прежде.
        var preferences = admittedPreferences.Value;
        var preparedExecution = LogicalDpoTrainingUnit.PrepareExecution(
            preferences,
            tokenizer);
        if (!preparedExecution.IsSuccess || preparedExecution.Value is null)
        {
            return preparedExecution.MapError<LogicalUnitResult<
                LogicalDpoTrainingResult>>();
        }

        var request = new LogicalDpoTrainingRequest(
            policyModel,
            referenceModel,
            tokenizer,
            checkpointDirectory,
            preferences,
            BuildCapabilityLoraConfig(),
            parentPolicyCheckpointPath,
            MaxStepCount: maxDpoStepCount,
            ConvergenceWindowSteps: convergenceWindowSteps,
            ParentProvenance: parentProvenance,
            AdmissionCapabilities: admissionCapabilities,
            TryAcceptArtifactOwnership: tryAcceptArtifactOwnership)
        {
            PreparedExecution = preparedExecution.Value
        };

        DpoRequestObserver.Value?.Invoke(request);

        return await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
            runId,
            LogicalDpoTrainingUnit.Id,
            LogicalDpoTrainingUnit.Role,
            LogicalDpoTrainingUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
    }

    // Build the model/tokenizer stack, continuing from the active domain-adapter checkpoint when one exists.
    // BuildTeachingStack собирает учебный стек. Учебный текст приходит ТОЛЬКО носителем
    // допущенного материала: довода с сырой строкой у этого входа больше нет, поэтому подать сюда
    // непроверенный текст нечем.
    private OperationResult<TeachingStack> BuildTeachingStack(
        GeneratedCapabilityCheckpointManifest? parent,
        string domainAdapterTag,
        CapabilityMaterialLineageGuard.AdmittedTrainingMaterial admittedMaterial,
        SemanticTeachingPromptRepresentation? compiledPrompt,
        LogicalCapabilityTrainingAdmissionCapabilities admissionCapabilities)
    {
        try
        {
            // Текст берётся из носителя сверяющим чтением: ровно тот, который прошёл допуск, и
            // только если содержание носителя с тех пор не изменилось. Чтение стоит внутри того
            // же перехвата, что и прежде, поэтому границы обработки ошибок не сдвинулись.
            var admittedTrainingText = admittedMaterial.ReadCombinedTrainingTextForLearner();
            // Отказ тождества передаётся как есть, до сборки учебного стека.
            if (!admittedTrainingText.IsSuccess || admittedTrainingText.Value is null)
                return admittedTrainingText.MapError<TeachingStack>();
            var trainingText = admittedTrainingText.Value;
            var config = BuildTinyConfig();
            if (parent is null)
            {
                return OperationResult<TeachingStack>.Ok(new TeachingStack(
                    new LlmModel(config),
                    BuildTokenizer(compiledPrompt, trainingText),
                    null,
                    GeneratedCapabilityVersion,
                    1,
                    null,
                    null,
                    false));
            }

            var parentProvenanceResult = BuildParentProvenance(parent);
            if (!parentProvenanceResult.IsSuccess)
            {
                return OperationResult<TeachingStack>.Fail(
                    parentProvenanceResult.ErrorMessage,
                    parentProvenanceResult.ErrorCode);
            }
            var parentProvenance = parentProvenanceResult.Value!;

            if (!File.Exists(parent.PolicyCheckpointPath))
                return OperationResult<TeachingStack>.Fail("Parent policy checkpoint was not found.", "PARENT_POLICY_CHECKPOINT_NOT_FOUND");
            if (!File.Exists(parent.TokenizerPath))
                return OperationResult<TeachingStack>.Fail("Parent tokenizer checkpoint was not found.", "PARENT_TOKENIZER_NOT_FOUND");

            var executableBinding =
                ValidateSignedExecutableManifestForTraining(
                    parent,
                    admissionCapabilities);
            if (!executableBinding.IsSuccess)
            {
                return executableBinding.MapError<TeachingStack>();
            }

            var tokenizer = BpeTokenizer.Load(parent.TokenizerPath);
            var policyModel = new LlmModel(parentProvenance.ParentConfig);
            var trainingStepCount = Math.Max(1, parent.TrainingStepCount) + 1;
            return OperationResult<TeachingStack>.Ok(new TeachingStack(
                policyModel,
                tokenizer,
                parent,
                IncrementPatchVersion(parent.Version),
                trainingStepCount,
                parent.PolicyCheckpointPath,
                parentProvenance,
                false));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TeachingStack>.Fail(
                "Teaching stack creation failed.",
                "TEACHING_STACK_CREATION_FAILED");
        }
    }

    // Find the manifest for the newest usable generated capability in this domain adapter.
    private static OperationResult<GeneratedCapabilityCheckpointManifest?>
        FindActiveParentManifestFromSnapshot(
            InMemoryLogicalUnitRegistry units,
            string domainAdapterTag,
            IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifests)
    {
        try
        {
            var activeDescriptor = units.ListDescriptors()
                .Where(descriptor =>
                    descriptor.UnitRole == GeneratedCapabilityUnitRole &&
                    descriptor.DomainTag == domainAdapterTag &&
                    descriptor.InputType == typeof(string) &&
                    descriptor.OutputType == typeof(string) &&
                    descriptor.TrustState >= LogicalUnitTrustState.Probationary &&
                    descriptor.TrustState != LogicalUnitTrustState.Rejected)
                .OrderByDescending(static descriptor => descriptor.RegistrationOrder)
                .FirstOrDefault();
            if (activeDescriptor is not null)
            {
                var activeManifest = manifests.LastOrDefault(manifest =>
                    HasCurrentExecutableBindingShape(manifest) &&
                    StringComparer.Ordinal.Equals(
                        manifest.UnitId,
                        activeDescriptor.UnitId) &&
                    StringComparer.Ordinal.Equals(
                        manifest.DomainTag,
                        domainAdapterTag));
                if (activeManifest is not null)
                {
                    return OperationResult<GeneratedCapabilityCheckpointManifest?>.Ok(
                        CloneManifest(activeManifest));
                }
            }

            var selected = manifests
                .Where(manifest =>
                    HasCurrentExecutableBindingShape(manifest) &&
                    StringComparer.Ordinal.Equals(
                        manifest.UnitRole,
                        GeneratedCapabilityUnitRole) &&
                    StringComparer.Ordinal.Equals(
                        manifest.DomainTag,
                        domainAdapterTag) &&
                    !StringComparer.OrdinalIgnoreCase.Equals(
                        manifest.TrustState,
                        LogicalUnitTrustState.Rejected.ToString()))
                .OrderByDescending(static manifest => manifest.CreatedAtUtc)
                .ThenByDescending(static manifest => manifest.UnitId, StringComparer.Ordinal)
                .FirstOrDefault();
            return OperationResult<GeneratedCapabilityCheckpointManifest?>.Ok(
                selected is null ? null : CloneManifest(selected));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<GeneratedCapabilityCheckpointManifest?>.Fail(
                "Generated capability parent snapshot is invalid.",
                "PARENT_CAPABILITY_PROVENANCE_INVALID");
        }
    }

    private static OperationResult<bool>
        ValidateSignedExecutableManifestForTraining(
            GeneratedCapabilityCheckpointManifest manifest,
            LogicalCapabilityTrainingAdmissionCapabilities
                admissionCapabilities)
    {
        if (manifest.ReplayAttestation is null ||
            manifest.ExecutableManifestAttestation is null)
        {
            return OperationResult<bool>.Fail(
                "Parent capability executable identity is invalid.",
                "PARENT_CAPABILITY_EXECUTABLE_IDENTITY_INVALID");
        }

        try
        {
            var tokenizerSha256 = ComputeFileSha256(
                manifest.TokenizerPath);
            var policySha256 = ComputeFileSha256(
                manifest.PolicyCheckpointPath);
            var executableManifestSha256 =
                GeneratedCapabilityExecutableManifestProjector.ProjectSha256(
                    manifest,
                    tokenizerSha256,
                    policySha256);
            if (!executableManifestSha256.IsSuccess ||
                executableManifestSha256.Value is null)
            {
                return executableManifestSha256.MapError<bool>();
            }

            var resealed = admissionCapabilities.SealExecutableManifest(
                executableManifestSha256.Value,
                manifest.ReplayAttestation);
            return resealed.IsSuccess &&
                   resealed.Value is not null &&
                   Equals(
                       resealed.Value,
                       manifest.ExecutableManifestAttestation)
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "Parent capability executable identity is invalid.",
                    "PARENT_CAPABILITY_EXECUTABLE_IDENTITY_INVALID");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Parent capability executable identity is invalid.",
                "PARENT_CAPABILITY_EXECUTABLE_IDENTITY_INVALID");
        }
    }

    private static bool HasCurrentExecutableBindingShape(
        GeneratedCapabilityCheckpointManifest manifest)
    {
        if (manifest.SchemaVersion !=
                GeneratedCapabilityCheckpointManifest.CurrentSchemaVersion ||
            !TeachingRegistryPersistence
                .HasBoundedExecutablePredicateIds(manifest.PredicateIds) ||
            manifest.UnitRegistrationOrder is not > 0 ||
            manifest.FlowRegistrationOrder is not > 0 ||
            manifest.ReplayAttestation is not { IsCanonical: true } ||
            manifest.ExecutableManifestAttestation is not
                { IsCanonical: true })
        {
            return false;
        }

        var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
            manifest.KnnExamples);
        return knnCorpus.IsSuccess &&
               knnCorpus.Value is not null &&
               RuntimeProvenance.IsSha256Hex(manifest.KnnCorpusSha256) &&
               RuntimeProvenance.FixedTimeSha256Equals(
                   manifest.KnnCorpusSha256!,
                   knnCorpus.Value.Sha256);
    }

    // Find the manifest for the newest usable generated capability in this domain adapter.
    private OperationResult<GeneratedCapabilityCheckpointManifest?> FindActiveParentManifestResult(
        InMemoryLogicalUnitRegistry units,
        string domainAdapterTag)
    {
        var manifestsResult = LoadGeneratedCapabilityManifestsForDomainResult(domainAdapterTag);
        if (!manifestsResult.IsSuccess)
            return OperationResult<GeneratedCapabilityCheckpointManifest?>.Fail(
                manifestsResult.ErrorMessage,
                manifestsResult.ErrorCode);
        var manifests = manifestsResult.Value!;
        var activeDescriptor = units.ListDescriptors()
            .Where(descriptor =>
                descriptor.UnitRole == GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == domainAdapterTag &&
                descriptor.InputType == typeof(string) &&
                descriptor.OutputType == typeof(string) &&
                descriptor.TrustState >= LogicalUnitTrustState.Probationary &&
                descriptor.TrustState != LogicalUnitTrustState.Rejected)
            .OrderByDescending(descriptor => descriptor.RegistrationOrder)
            .FirstOrDefault();
        if (activeDescriptor is not null)
        {
            var activeManifest = manifests
                .LastOrDefault(manifest =>
                    HasCurrentExecutableBindingShape(manifest) &&
                    string.Equals(manifest.UnitId, activeDescriptor.UnitId, StringComparison.Ordinal) &&
                    string.Equals(manifest.DomainTag, domainAdapterTag, StringComparison.Ordinal));
            if (activeManifest is not null)
                return OperationResult<GeneratedCapabilityCheckpointManifest?>.Ok(activeManifest);
        }

        var selected = manifests
            .Where(manifest =>
                HasCurrentExecutableBindingShape(manifest) &&
                string.Equals(manifest.UnitRole, GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
                string.Equals(manifest.DomainTag, domainAdapterTag, StringComparison.Ordinal) &&
                !string.Equals(manifest.TrustState, LogicalUnitTrustState.Rejected.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .ThenByDescending(manifest => manifest.UnitId, StringComparer.Ordinal)
            .FirstOrDefault();
        return OperationResult<GeneratedCapabilityCheckpointManifest?>.Ok(selected);
    }

    // Load generated capability manifests from the configured checkpoint root.
    private GeneratedCapabilityCheckpointManifest[] LoadGeneratedCapabilityManifests()
    {
        var result = LoadGeneratedCapabilityManifestsResult();
        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"{result.ErrorCode}: Generated capability manifest inventory is unavailable.");
        return result.Value!;
    }

    private OperationResult<GeneratedCapabilityCheckpointManifest[]> LoadGeneratedCapabilityManifestsResult()
    {
        lock (_manifestCacheGate)
        {
            var hydration = EnsureManifestCacheHydratedUnderGate();
            if (!hydration.IsSuccess)
                return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Fail(
                    hydration.ErrorMessage,
                    hydration.ErrorCode);

            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(
                _manifestCacheByUnitId.Values
                    .OrderBy(manifest => manifest.CreatedAtUtc)
                    .ThenBy(manifest => manifest.UnitId, StringComparer.Ordinal)
                    .ToArray());
        }
    }

    private OperationResult<GeneratedCapabilityCheckpointManifest[]> LoadGeneratedCapabilityManifestsForDomainResult(
        string domainTag)
    {
        var all = LoadGeneratedCapabilityManifestsResult();
        if (!all.IsSuccess)
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Fail(all.ErrorMessage, all.ErrorCode);

        return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(
            all.Value!
                .Where(manifest => string.Equals(manifest.DomainTag, domainTag, StringComparison.Ordinal))
                .ToArray());
    }

    private OperationResult<int> CountGeneratedCapabilityManifestsResult()
    {
        lock (_manifestCacheGate)
        {
            var hydration = EnsureManifestCacheHydratedUnderGate();
            if (!hydration.IsSuccess)
                return OperationResult<int>.Fail(hydration.ErrorMessage, hydration.ErrorCode);
            return OperationResult<int>.Ok(_manifestCacheByUnitId.Count);
        }
    }

    private OperationResult<bool> EnsureManifestCacheHydratedUnderGate()
    {
        if (_manifestCacheHydrated)
            return OperationResult<bool>.Ok(true);

        var inventory = TeachingRegistryPersistence.ReadGeneratedCapabilityManifestInventory(_checkpointRoot);
        if (!inventory.IsSuccess)
            return OperationResult<bool>.Fail(inventory.ErrorMessage, inventory.ErrorCode);

        var accepted = new Dictionary<string, GeneratedCapabilityCheckpointManifest>(
            inventory.Value!.ByUnitId,
            StringComparer.Ordinal);
        _manifestCacheByUnitId.Clear();
        foreach (var pair in accepted)
            _manifestCacheByUnitId.Add(pair.Key, pair.Value);
        _manifestCacheHydrated = true;
        return OperationResult<bool>.Ok(true);
    }

    // Return previous manifests for the same domain adapter, optionally ranked by E34V2 replay fusion.
    private GeneratedCapabilityCheckpointManifest[] FindReplayManifests(
        string domainAdapterTag,
        string? compiledText,
        bool useReplayFusionRanking)
    {
        var result = FindReplayManifestsResult(domainAdapterTag, compiledText, useReplayFusionRanking);
        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"{result.ErrorCode}: Generated capability replay inventory is unavailable.");
        return result.Value!;
    }

    private OperationResult<GeneratedCapabilityCheckpointManifest[]> FindReplayManifestsResult(
        string domainAdapterTag,
        string? compiledText,
        bool useReplayFusionRanking)
    {
        var domainManifests = LoadGeneratedCapabilityManifestsForDomainResult(domainAdapterTag);
        if (!domainManifests.IsSuccess)
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Fail(
                domainManifests.ErrorMessage,
                domainManifests.ErrorCode);

        var candidates = domainManifests.Value!
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .Take(10)
            .ToArray();

        if (!useReplayFusionRanking || string.IsNullOrWhiteSpace(compiledText))
            return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(candidates.Take(3).ToArray());

        return OperationResult<GeneratedCapabilityCheckpointManifest[]>.Ok(
            ReplayFusionRanker.Rank(candidates, compiledText, topK: 3).ToArray());
    }

    private OperationResult<PublicationPlan[]> BuildPublicationPlans(
        IReadOnlyList<TrainingBranchPlan> registrationPlans,
        string runId,
        string domainAdapterTag,
        string invocationToken)
    {
        var pathComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        try
        {
            var canonicalRoot = Path.GetFullPath(_checkpointRoot);
            if (!HasReparseFreeExistingAncestors(canonicalRoot))
                return PublicationPathInvalid();

            var rootPrefix = canonicalRoot.EndsWith(Path.DirectorySeparatorChar)
                ? canonicalRoot
                : canonicalRoot + Path.DirectorySeparatorChar;
            var acceptedPaths = new HashSet<string>(pathComparer);
            var publications = new List<PublicationPlan>(registrationPlans.Count);
            for (var publicationIndex = 0;
                 publicationIndex < registrationPlans.Count;
                 publicationIndex++)
            {
                var registrationPlan = registrationPlans[publicationIndex];
                var checkpointDirectory = Path.GetFullPath(Path.Combine(
                    canonicalRoot,
                    Sanitize(runId),
                    Sanitize(domainAdapterTag),
                    Sanitize(registrationPlan.SubPhaseId)));
                if (!IsPublicationPathContained(checkpointDirectory, canonicalRoot, rootPrefix, pathComparison))
                    return PublicationPathInvalid();

                if (!HasReparseFreeExistingAncestors(checkpointDirectory))
                    return PublicationPathInvalid();

                var tokenizerPath = Path.GetFullPath(Path.Combine(checkpointDirectory, "tokenizer.json"));
                var policyCheckpointPath = Path.GetFullPath(Path.Combine(checkpointDirectory, "policy.bin"));
                var policyTemporaryPath = Path.GetFullPath(Path.Combine(checkpointDirectory, "policy.bin.tmp"));
                var manifestPath = Path.GetFullPath(Path.Combine(
                    checkpointDirectory,
                    TeachingRegistryPersistence.GeneratedCapabilityManifestFileName));
                var reservationPath = Path.GetFullPath(Path.Combine(
                    checkpointDirectory,
                    ".generated-capability.destination.reservation"));
                var stagingDirectory = Path.GetFullPath(Path.Combine(
                    canonicalRoot,
                    $".generated-capability-publication-staging-{invocationToken}-{publicationIndex:D4}"));
                var stagingMarkerPath = Path.Combine(
                    stagingDirectory,
                    ".generated-capability.publication-owner");
                var stagingTokenizerPath = Path.Combine(
                    stagingDirectory,
                    "tokenizer.json");
                var stagingPolicyCheckpointPath = Path.Combine(
                    stagingDirectory,
                    "policy.bin");
                var stagingManifestPath = Path.Combine(
                    stagingDirectory,
                    TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
                var dpoMetricsDirectory = Path.Combine(
                    checkpointDirectory,
                    "dpo-metrics");
                var stagingDpoMetricsDirectory = Path.Combine(
                    stagingDirectory,
                    "dpo-metrics");
                var protectedPaths = new[]
                {
                    tokenizerPath,
                    policyCheckpointPath,
                    policyTemporaryPath,
                    manifestPath
                };

                foreach (var path in protectedPaths.Append(reservationPath))
                {
                    if (!IsPublicationPathContained(path, canonicalRoot, rootPrefix, pathComparison) ||
                        !acceptedPaths.Add(path))
                    {
                        return OperationResult<PublicationPlan[]>.Fail(
                            "Generated capability publication destinations conflict.",
                            "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS");
                    }
                }

                foreach (var path in protectedPaths)
                {
                    if (File.Exists(path) || Directory.Exists(path))
                        return OperationResult<PublicationPlan[]>.Fail(
                            "Generated capability artifact already exists.",
                            "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS");
                }

                if (File.Exists(reservationPath) || Directory.Exists(reservationPath))
                    return OperationResult<PublicationPlan[]>.Fail(
                        "Generated capability destination is busy.",
                        "GENERATED_CAPABILITY_DESTINATION_BUSY");

                if (Directory.Exists(checkpointDirectory) ||
                    File.Exists(checkpointDirectory) ||
                    Directory.Exists(stagingDirectory) ||
                    File.Exists(stagingDirectory))
                {
                    return OperationResult<PublicationPlan[]>.Fail(
                        "Generated capability destination is busy.",
                        "GENERATED_CAPABILITY_DESTINATION_BUSY");
                }

                publications.Add(new PublicationPlan(
                    registrationPlan.SubPhaseId,
                    checkpointDirectory,
                    $"{GeneratedCapabilityUnitRole}-{Sanitize(domainAdapterTag)}-{Sanitize(registrationPlan.SubPhaseId)}-{Sanitize(runId)}",
                    tokenizerPath,
                    policyCheckpointPath,
                    policyTemporaryPath,
                    manifestPath,
                    stagingDirectory,
                    stagingMarkerPath,
                    stagingTokenizerPath,
                    stagingPolicyCheckpointPath,
                    stagingManifestPath,
                    dpoMetricsDirectory,
                    stagingDpoMetricsDirectory,
                    invocationToken,
                    canonicalRoot,
                    rootPrefix,
                    pathComparison));
            }

            return OperationResult<PublicationPlan[]>.Ok(publications.ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return PublicationPathInvalid();
        }
    }

    private static OperationResult<bool> ValidateFinalRegistryPublication(
        LogicalCapabilityTrainingRequest request,
        IReadOnlyList<TrainingBranchPlan> registrationPlans,
        string domainAdapterTag,
        string? domainClassifierUnitId,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifestInventory,
        LogicalCapabilityTrainingAdmissionCapabilities admissionCapabilities)
    {
        try
        {
            var expectedUnitIds = registrationPlans
                .Select(plan => $"{GeneratedCapabilityUnitRole}-{Sanitize(domainAdapterTag)}-{Sanitize(plan.SubPhaseId)}-{Sanitize(request.RunId)}")
                .Concat(string.IsNullOrEmpty(domainClassifierUnitId)
                    ? []
                    : [domainClassifierUnitId])
                .ToArray();
            var expectedFlowIds = registrationPlans
                .Select(plan => BuildGeneratedCapabilityFlowId(
                    domainAdapterTag,
                    plan.SubPhaseId))
                .ToArray();
            if (expectedUnitIds.Distinct(StringComparer.Ordinal).Count() != expectedUnitIds.Length ||
                expectedFlowIds.Distinct(StringComparer.Ordinal).Count() != expectedFlowIds.Length)
            {
                return RegistryPublicationInvalid();
            }

            var existingUnits = request.Units.ListDescriptors()
                .Select(static descriptor => descriptor.UnitId)
                .ToHashSet(StringComparer.Ordinal);
            var existingFlows = request.Flows.ListDescriptors();
            if (expectedUnitIds.Any(existingUnits.Contains) ||
                existingFlows.Any(descriptor =>
                    expectedFlowIds.Contains(
                        descriptor.FlowId,
                        StringComparer.Ordinal) &&
                    !IsVerifiedCurrentDomainFlowBinding(
                        descriptor,
                        domainAdapterTag,
                        manifestInventory,
                        admissionCapabilities)))
            {
                return RegistryPublicationInvalid();
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RegistryPublicationInvalid();
        }
    }

    private static bool IsVerifiedCurrentDomainFlowBinding(
        LogicalFlowDescriptor descriptor,
        string domainAdapterTag,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifestInventory,
        LogicalCapabilityTrainingAdmissionCapabilities admissionCapabilities)
    {
        foreach (var manifest in manifestInventory)
        {
            if (!StringComparer.Ordinal.Equals(
                    manifest.DomainTag,
                    domainAdapterTag) ||
                !HasCurrentExecutableBindingShape(manifest) ||
                !MatchesSignedFlowRegistration(descriptor, manifest))
            {
                continue;
            }

            var executableBinding = ValidateSignedExecutableManifestForTraining(
                manifest,
                admissionCapabilities);
            if (executableBinding.IsSuccess && executableBinding.Value)
                return true;
        }

        return false;
    }

    private static OperationResult<bool> RegistryPublicationInvalid()
        => OperationResult<bool>.Fail(
            "Generated capability registry publication preflight failed.",
            "GENERATED_CAPABILITY_REGISTRY_PUBLICATION_INVALID");

    private static OperationResult<LogicalCapabilityTrainingResult>
        PublicationEnlistmentFailed()
        => OperationResult<LogicalCapabilityTrainingResult>.Fail(
            "Generated capability registration could not be enlisted for rollback.",
            "GENERATED_CAPABILITY_PUBLICATION_ENLISTMENT_FAILED");

    private static OperationResult<bool> ValidateFinalDestinationAbsence(
        IReadOnlyList<PublicationPlan> publications)
    {
        try
        {
            foreach (var publication in publications)
            {
                if (!Directory.Exists(publication.StagingDirectory) ||
                    !File.Exists(publication.StagingMarkerPath) ||
                    (File.GetAttributes(publication.StagingDirectory) &
                     FileAttributes.ReparsePoint) != 0)
                {
                    return OperationResult<bool>.Fail(
                        "Generated capability publication staging is invalid.",
                        "GENERATED_CAPABILITY_DESTINATION_RESERVATION_FAILED");
                }

                if (Directory.Exists(publication.CheckpointDirectory) ||
                    File.Exists(publication.CheckpointDirectory) ||
                    publication.ProtectedArtifactPaths.Any(path =>
                        File.Exists(path) || Directory.Exists(path)))
                {
                    return OperationResult<bool>.Fail(
                        "Generated capability artifact already exists.",
                        "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS");
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Generated capability publication destination could not be validated.",
                "GENERATED_CAPABILITY_DESTINATION_RESERVATION_FAILED");
        }
    }

    private static OperationResult<DurableDpoMetricPublication[]>
        PrepareDurableDpoMetrics(
            IReadOnlyList<string> sourcePaths,
            PublicationPlan publication,
            InvocationPublicationCleanup cleanup)
    {
        try
        {
            if (sourcePaths.Count == 0)
                return OperationResult<DurableDpoMetricPublication[]>.Ok([]);

            Directory.CreateDirectory(publication.StagingDpoMetricsDirectory);
            cleanup.TrackOwnedDirectory(
                publication.StagingDpoMetricsDirectory);
            var prepared = new DurableDpoMetricPublication[sourcePaths.Count];
            for (var index = 0; index < sourcePaths.Count; index++)
            {
                var sourcePath = Path.GetFullPath(sourcePaths[index]);
                var fileName = Path.GetFileName(sourcePath);
                if (!fileName.StartsWith("dpo_step_", StringComparison.Ordinal) ||
                    !fileName.EndsWith(".json", StringComparison.Ordinal) ||
                    !cleanup.TryGetOwnedFileSha256(
                        sourcePath,
                        out var expectedSourceSha256))
                {
                    return OperationResult<DurableDpoMetricPublication[]>.Fail(
                        "DPO metric evidence identity is invalid.",
                        "DPO_METRIC_EVIDENCE_INVALID");
                }

                var stagingPath = Path.Combine(
                    publication.StagingDpoMetricsDirectory,
                    fileName);
                var finalPath = Path.Combine(
                    publication.DpoMetricsDirectory,
                    fileName);
                var stagedSha256 = cleanup.CreateAndTrackOwnedFile(
                    stagingPath,
                    destination =>
                    {
                        using var source = new FileStream(
                            sourcePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read | FileShare.Delete);
                        source.CopyTo(destination);
                    });

                var sourceSha256 = ComputeFileSha256(
                    sourcePath,
                    FileShare.Read | FileShare.Delete);
                if (!StringComparer.Ordinal.Equals(
                        sourceSha256,
                        stagedSha256) ||
                    !StringComparer.Ordinal.Equals(
                        sourceSha256,
                        expectedSourceSha256))
                {
                    return OperationResult<DurableDpoMetricPublication[]>.Fail(
                        "DPO metric evidence could not be verified.",
                        "DPO_METRIC_EVIDENCE_INVALID");
                }

                prepared[index] = new DurableDpoMetricPublication(
                    sourcePath,
                    stagingPath,
                    finalPath,
                    stagedSha256);
            }

            return OperationResult<DurableDpoMetricPublication[]>.Ok(prepared);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DurableDpoMetricPublication[]>.Fail(
                "DPO metric evidence could not be promoted.",
                "DPO_METRIC_EVIDENCE_PUBLICATION_FAILED");
        }
    }

    private static OperationResult<PublishedArtifactHandleSet> PublishStagedDirectories(
        IReadOnlyList<PreparedCapabilityPublication> prepared,
        InvocationPublicationCleanup cleanup)
    {
        var ownership = cleanup.PreparePublishedDirectoryOwnership(prepared);
        if (!ownership.IsSuccess)
            return ownership.MapError<PublishedArtifactHandleSet>();
        var handles = new PublishedArtifactHandleSet(
            prepared.Count,
            cleanup);
        for (var index = 0; index < prepared.Count; index++)
            handles.Prepare(index, prepared[index]);
        var published = OperationResult<PublishedArtifactHandleSet>.Ok(handles);
        var identityChanged = OperationResult<PublishedArtifactHandleSet>.Fail(
            "Generated capability artifact handles could not be retained.",
            "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
        var transferred = false;
        try
        {
            for (var index = 0; index < prepared.Count; index++)
            {
                var item = prepared[index];
                var publication = item.Publication;
                var parent = Path.GetDirectoryName(
                    publication.CheckpointDirectory)
                    ?? throw new InvalidOperationException();
                Directory.CreateDirectory(parent);
                if (!HasReparseFreeExistingAncestors(parent) ||
                    Directory.Exists(publication.CheckpointDirectory) ||
                    File.Exists(publication.CheckpointDirectory))
                {
                    return OperationResult<PublishedArtifactHandleSet>.Fail(
                        "Generated capability artifact already exists.",
                        "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS");
                }

                PublicationStageObserver.Value?.Invoke(
                    "before_directory_move",
                    publication.CheckpointDirectory);
                if (!cleanup.ReleasePublishedDirectoryHandlesForAtomicMove(item))
                    return identityChanged;
                try
                {
                    Directory.Move(
                        publication.StagingDirectory,
                        publication.CheckpointDirectory);
                }
                catch (IOException exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return Directory.Exists(publication.CheckpointDirectory) ||
                           File.Exists(publication.CheckpointDirectory)
                        ? OperationResult<PublishedArtifactHandleSet>.Fail(
                            "Generated capability artifact already exists.",
                            "GENERATED_CAPABILITY_ARTIFACT_ALREADY_EXISTS")
                        : OperationResult<PublishedArtifactHandleSet>.Fail(
                            "Generated capability artifacts could not be published.",
                            "GENERATED_CAPABILITY_PUBLICATION_FAILED");
                }
                if (!cleanup.PromotePublishedDirectoryOwnership(item))
                    return identityChanged;
                PublicationStageObserver.Value?.Invoke(
                    "after_directory_move_before_handle_acquisition",
                    publication.CheckpointDirectory);
                if (!cleanup.PublishedDirectoryOwnershipStillMatches(item))
                    return identityChanged;
                handles.AcquireTokenizer(index);
                PublicationStageObserver.Value?.Invoke(
                    "after_tokenizer_handle_acquisition",
                    publication.CheckpointDirectory);
                handles.AcquirePolicy(index);
                PublicationStageObserver.Value?.Invoke(
                    "after_policy_handle_acquisition",
                    publication.CheckpointDirectory);
                handles.AcquireManifest(index);
                PublicationStageObserver.Value?.Invoke(
                    "after_manifest_handle_acquisition",
                    publication.CheckpointDirectory);
                for (var metricIndex = 0;
                     metricIndex < item.DpoMetrics.Length;
                     metricIndex++)
                {
                    handles.AcquireMetric(index, metricIndex);
                    PublicationStageObserver.Value?.Invoke(
                        "after_metric_handle_acquisition",
                        publication.CheckpointDirectory);
                }
                PublicationStageObserver.Value?.Invoke(
                    "after_directory_move",
                    publication.CheckpointDirectory);
            }

            transferred = true;
            return published;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return exception is IOException or UnauthorizedAccessException
                ? identityChanged
                : OperationResult<PublishedArtifactHandleSet>.Fail(
                    "Generated capability artifacts could not be published.",
                    "GENERATED_CAPABILITY_PUBLICATION_FAILED");
        }
        finally
        {
            if (!transferred)
                handles.Dispose();
        }
    }

    private static OperationResult<bool> PrepareInvocationStaging(
        IReadOnlyList<PublicationPlan> publications,
        string batchCheckpointDirectory,
        string invocationToken,
        InvocationPublicationCleanup cleanup)
    {
        try
        {
            if (publications.Count == 0)
                return OperationResult<bool>.Ok(true);

            var first = publications[0];
            Directory.CreateDirectory(first.CanonicalRoot);
            if ((File.GetAttributes(first.CanonicalRoot) & FileAttributes.ReparsePoint) != 0)
                return PublicationValidationPathInvalid();

            var canonicalBatch = Path.GetFullPath(batchCheckpointDirectory);
            if (!IsPublicationPathContained(
                    canonicalBatch,
                    first.CanonicalRoot,
                    first.RootPrefix,
                    first.PathComparison) ||
                Directory.Exists(canonicalBatch) ||
                File.Exists(canonicalBatch))
            {
                return OperationResult<bool>.Fail(
                    "Generated capability DPO staging destination is unavailable.",
                    "GENERATED_CAPABILITY_DESTINATION_BUSY");
            }

            Directory.CreateDirectory(canonicalBatch);
            cleanup.TrackOwnedDirectory(canonicalBatch);
            PublicationStageObserver.Value?.Invoke(
                "after_dpo_directory_create_before_marker",
                canonicalBatch);
            if (!IsEmptyRegularDirectory(canonicalBatch))
            {
                return OperationResult<bool>.Fail(
                    "Generated capability DPO staging identity is invalid.",
                    "GENERATED_CAPABILITY_DESTINATION_BUSY");
            }
            var dpoMarker = Path.Combine(
                canonicalBatch,
                ".generated-capability.dpo-owner");
            cleanup.CreateAndTrackOwnedFile(
                dpoMarker,
                destination => destination.Write(
                    Encoding.ASCII.GetBytes(invocationToken)));
            cleanup.TrackInvocationStagingDirectory(
                canonicalBatch,
                dpoMarker,
                invocationToken,
                transientOnSuccess: true);

            foreach (var publication in publications)
            {
                if (!IsPublicationPathContained(
                        publication.StagingDirectory,
                        publication.CanonicalRoot,
                        publication.RootPrefix,
                        publication.PathComparison) ||
                    Directory.Exists(publication.StagingDirectory) ||
                    File.Exists(publication.StagingDirectory))
                {
                    return OperationResult<bool>.Fail(
                        "Generated capability publication staging destination is unavailable.",
                        "GENERATED_CAPABILITY_DESTINATION_BUSY");
                }

                Directory.CreateDirectory(publication.StagingDirectory);
                cleanup.TrackOwnedDirectory(publication.StagingDirectory);
                PublicationStageObserver.Value?.Invoke(
                    "after_publication_directory_create_before_marker",
                    publication.StagingDirectory);
                if (!IsEmptyRegularDirectory(publication.StagingDirectory))
                {
                    return OperationResult<bool>.Fail(
                        "Generated capability publication staging identity is invalid.",
                        "GENERATED_CAPABILITY_DESTINATION_BUSY");
                }
                cleanup.CreateAndTrackOwnedFile(
                    publication.StagingMarkerPath,
                    destination => destination.Write(
                        Encoding.ASCII.GetBytes(
                            publication.InvocationToken)));
                cleanup.TrackInvocationStagingDirectory(
                    publication.StagingDirectory,
                    publication.StagingMarkerPath,
                    publication.InvocationToken,
                    transientOnSuccess: false);
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "Generated capability publication directories could not be prepared.",
                "GENERATED_CAPABILITY_DESTINATION_RESERVATION_FAILED");
        }
    }

    private static bool IsEmptyRegularDirectory(string path)
        => Directory.Exists(path) &&
           (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0 &&
           !Directory.EnumerateFileSystemEntries(path).Any();

    private static bool HasReparseFreeExistingAncestors(string canonicalPath)
    {
        var current = canonicalPath;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(current) || Directory.Exists(current))
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    return false;
            }

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent) ||
                StringComparer.Ordinal.Equals(parent, current))
            {
                return true;
            }

            current = parent;
        }

        return true;
    }

    private static bool IsPublicationPathContained(
        string candidate,
        string canonicalRoot,
        string rootPrefix,
        StringComparison comparison)
    {
        return string.Equals(candidate, canonicalRoot, comparison) ||
               candidate.StartsWith(rootPrefix, comparison);
    }

    private static OperationResult<PublicationPlan[]> PublicationPathInvalid()
    {
        return OperationResult<PublicationPlan[]>.Fail(
            "Generated capability publication path is invalid.",
            "GENERATED_CAPABILITY_DESTINATION_RESERVATION_FAILED");
    }

    private static OperationResult<bool> PublicationValidationPathInvalid()
    {
        return OperationResult<bool>.Fail(
            "Generated capability publication path is invalid.",
            "GENERATED_CAPABILITY_DESTINATION_RESERVATION_FAILED");
    }

    private static int ReadDpoMaxStepCount(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata.TryGetValue("dpo.max_steps", out var value) &&
            int.TryParse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), out var parsed))
            return Math.Max(1, parsed);

        return DefaultDpoMaxStepCount;
    }

    private static int ReadDpoConvergenceWindowSteps(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata.TryGetValue("dpo.convergence_window_steps", out var value) &&
            int.TryParse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), out var parsed))
            return Math.Max(1, parsed);

        return DefaultDpoConvergenceWindowSteps;
    }

    // Extract the routed or missing sub-phase from one branch.
    private static string ExtractSubPhaseId(LogicalFlowResult branch)
    {
        if (branch.Metrics.TryGetValue("missing_sub_phase", out var missingSubPhase) && !string.IsNullOrWhiteSpace(missingSubPhase))
            return missingSubPhase.Trim();
        if (branch.Metrics.TryGetValue("routed_sub_phase_id", out var routedSubPhase) && !string.IsNullOrWhiteSpace(routedSubPhase))
            return routedSubPhase.Trim();
        if (branch.Metrics.TryGetValue("sub_phase_id", out var subPhase) && !string.IsNullOrWhiteSpace(subPhase))
            return subPhase.Trim();

        return string.Empty;
    }

    // Convert a sub-phase name into a stable task type when no explicit flow metadata exists.
    private static string BuildTaskType(LogicalSubPhase subPhase)
    {
        return Sanitize(subPhase.Name);
    }

    // Build the durable manifest used by cumulative teaching sessions to load prior row capability.
    private static GeneratedCapabilityCheckpointManifest BuildGeneratedCapabilityManifest(
        string unitId,
        string flowId,
        string scenarioId,
        string tokenizerPath,
        string policyCheckpointPath,
        string trainingText,
        string outputText,
        string exampleId,
        string domainAdapterTag,
        LogicalSubPhase subPhase,
        GeneratedCapabilityCheckpointManifest? parentManifest,
        int trainingStepCount,
        string version,
        LogicalUnitTrustState trustState,
        string originalPrompt,
        ModelConfig config,
        string tokenizerSha256,
        string policyCheckpointSha256,
        string runtimeProvenanceSha256,
        LogicalCapabilityTrainingReplayAttestation replayAttestation,
        long unitRegistrationOrder,
        long flowRegistrationOrder,
        IReadOnlyList<GeneratedCapabilityKnnExample> knnExamples,
        string knnCorpusSha256)
    {
        var loraConfig = BuildCapabilityLoraConfig();
        return new GeneratedCapabilityCheckpointManifest(
            unitId,
            GeneratedCapabilityUnitRole,
            domainAdapterTag,
            version,
            trustState.ToString(),
            flowId,
            scenarioId,
            subPhase.PhaseId,
            subPhase.SubPhaseId,
            BuildTaskType(subPhase),
            trainingText,
            outputText,
            exampleId,
            subPhase.PredicateIds,
            subPhase.DomainIds,
            Path.GetFullPath(policyCheckpointPath),
            Path.GetFullPath(tokenizerPath),
            config.VocabSize,
            config.ContextLen,
            config.HiddenDim,
            config.NumLayers,
            config.NumHeads,
            config.NumKvHeads,
            config.FfnHiddenDim,
            loraConfig.Rank,
            loraConfig.Alpha,
            loraConfig.Dropout,
            loraConfig.TargetModules,
            DateTimeOffset.UtcNow,
            UnitFamilyId: BuildUnitFamilyId(domainAdapterTag),
            ParentUnitId: parentManifest?.UnitId,
            ParentPolicyCheckpointPath: parentManifest?.PolicyCheckpointPath,
            ParentTokenizerPath: parentManifest?.TokenizerPath,
            TrainingStepCount: trainingStepCount,
            OriginalPrompt: originalPrompt?.Trim() ?? string.Empty,
            RopeTheta: config.RopeTheta,
            RmsNormEps: config.RmsNormEps,
            TokenizerSha256: tokenizerSha256,
            PolicyCheckpointSha256: policyCheckpointSha256,
            RuntimeProvenanceSha256: runtimeProvenanceSha256,
            ReplayAttestation: replayAttestation with { },
            SchemaVersion:
                GeneratedCapabilityCheckpointManifest.CurrentSchemaVersion,
            UnitRegistrationOrder: unitRegistrationOrder,
            FlowRegistrationOrder: flowRegistrationOrder,
            KnnExamples: knnExamples,
            KnnCorpusSha256: knnCorpusSha256);
    }

    private static bool MatchesSignedUnitRegistration(
        LogicalUnitDescriptor descriptor,
        GeneratedCapabilityCheckpointManifest manifest)
        => manifest.UnitRegistrationOrder is not null &&
           descriptor.RegistrationOrder ==
               manifest.UnitRegistrationOrder.Value &&
           StringComparer.Ordinal.Equals(
               descriptor.UnitId,
               manifest.UnitId) &&
           StringComparer.Ordinal.Equals(
               descriptor.UnitRole,
               manifest.UnitRole) &&
           StringComparer.Ordinal.Equals(
               descriptor.DomainTag,
               manifest.DomainTag) &&
           StringComparer.Ordinal.Equals(
               descriptor.Version,
               manifest.Version) &&
           StringComparer.Ordinal.Equals(
               descriptor.TrustState.ToString(),
               manifest.TrustState) &&
           descriptor.InputType == typeof(string) &&
           descriptor.OutputType == typeof(string) &&
           !descriptor.IsLearned;

    private static bool MatchesSignedFlowRegistration(
        LogicalFlowDescriptor descriptor,
        GeneratedCapabilityCheckpointManifest manifest)
        => manifest.FlowRegistrationOrder is not null &&
           descriptor.RegistrationOrder ==
               manifest.FlowRegistrationOrder.Value &&
           StringComparer.Ordinal.Equals(
               descriptor.FlowId,
               manifest.FlowId) &&
           StringComparer.Ordinal.Equals(
               descriptor.ScenarioId,
               manifest.ScenarioId) &&
           StringComparer.Ordinal.Equals(
               descriptor.Version,
               manifest.Version) &&
           StringComparer.Ordinal.Equals(
               descriptor.PhaseId,
               manifest.PhaseId) &&
           StringComparer.Ordinal.Equals(
               descriptor.SubPhaseId,
               manifest.SubPhaseId) &&
           StringComparer.Ordinal.Equals(
               descriptor.TaskType,
               manifest.TaskType) &&
           descriptor.InputType == typeof(string) &&
           descriptor.DomainTags.SequenceEqual(
               manifest.DomainTags,
               StringComparer.Ordinal);

    // Build and own the exact domain-adapter retrieval memory from selected
    // replay rows plus the current teaching rows.
    // BuildOrderedRetrievalCorpusExamples собирает УПОРЯДОЧЕННЫЕ строки корпуса ближайших соседей
    // из тех же двух входов, что и прежде: отобранных слепков повтора и планов ветвей.
    //
    // Порядок строк здесь значим и оставлен прежним дословно: сначала слепки повтора в обратном
    // порядке, затем планы ветвей. Вынесено в отдельного владельца затем, чтобы один и тот же
    // упорядоченный набор можно было подать на допуск ДО первого разрешения на изменение, а потом
    // получить его из допущенного носителя — вместо того, чтобы собирать его второй раз позже и
    // надеяться, что собралось то же самое.
    private static IReadOnlyList<GeneratedCapabilityKnnExample> BuildOrderedRetrievalCorpusExamples(
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        IReadOnlyList<TrainingBranchPlan> branchPlans)
    {
        // Слепки повтора идут первыми и в обратном порядке — ровно как было.
        var examples = replayManifests
            .Reverse()
            .Select(manifest => new GeneratedCapabilityKnnExample(
                manifest.ExampleId,
                manifest.TrainingText,
                manifest.OutputText,
                manifest.PredicateIds,
                manifest.DomainTags))
            .ToList();

        // Планы ветвей добавляются следом, тем же способом и в том же порядке.
        examples.AddRange(branchPlans.Select(plan => new GeneratedCapabilityKnnExample(
            plan.AuthorizedReplayExampleId!,
            plan.TrainingText,
            plan.ChosenText,
            plan.SubPhase.PredicateIds,
            plan.SubPhase.DomainIds)));

        // Готовый упорядоченный набор возвращается вызывающему.
        return examples;
    }

    // BuildKnnTrainingCorpus строит корпус поиска ИЗ ДОПУЩЕННОГО НОСИТЕЛЯ.
    //
    // Что было неправильно. Этот метод принимал слепки повтора и планы ветвей и собирал корпус
    // сам, уже после обучения предпочтениями. Допуск материала эти входы не видел, поэтому
    // корпус, который читала обучаемая единица поиска, и проверенный материал были разными
    // вещами: проверить одну копию и обучать по другой было нечем помешать.
    //
    // Что стало. Корпус приходит только из носителя, и носитель перед выдачей сверяет тождество
    // того, чем владеет, с отпечатком, записанным при допуске. Довода с сырыми входами у этого
    // метода больше нет, поэтому подменить корпус между допуском и обучением нечем.
    private static OperationResult<PreparedKnnCorpus> BuildKnnTrainingCorpus(
        CapabilityMaterialLineageGuard.AdmittedTrainingMaterial admittedMaterial)
    {
        // Упорядоченный корпус читается сверяющим чтением: изменённое после допуска не выдаётся.
        var admittedCorpus = admittedMaterial.ReadRetrievalCorpusForLearner();
        // Отказ тождества передаётся вызывающему как есть, вместе с кодом и причиной.
        if (!admittedCorpus.IsSuccess || admittedCorpus.Value is null)
            return admittedCorpus.MapError<PreparedKnnCorpus>();

        // Опись и отпечаток корпуса считает прежний владелец прежним договором: его семантика
        // этой правкой не меняется, меняется только источник строк.
        var projected = GeneratedCapabilityKnnCorpusProjector.Project(
            admittedCorpus.Value);
        if (!projected.IsSuccess || projected.Value is null)
            return projected.MapError<PreparedKnnCorpus>();

        var manifestExamples = projected.Value.Examples;
        var runtimeExamples = manifestExamples
            .Select(static example =>
                new TransformerKnnTrainingExample<string>(
                    example.ExampleId,
                    example.TrainingText,
                    example.OutputText,
                    example.PredicateIds.ToArray(),
                    example.DomainTags.ToArray()))
            .ToArray();
        return OperationResult<PreparedKnnCorpus>.Ok(
            new PreparedKnnCorpus(
                manifestExamples,
                runtimeExamples,
                projected.Value.Sha256));
    }

    private OperationResult<PreparedDomainClassifierPublication>
        PrepareDomainClassifierPublication(
            LogicalCapabilityTrainingRequest request,
            SemanticTeachingPromptRepresentation? compiledPrompt,
            string? compiledText,
            string stagingDirectory,
            string invocationToken,
            InvocationPublicationCleanup cleanup)
    {
        try
        {
            compiledPrompt ??= !string.IsNullOrWhiteSpace(compiledText)
                ? CompileForTeaching(compiledText!)
                : null;
            if (compiledPrompt is null)
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "No compiled prompt was available for domain classifier training.",
                    "COMPILED_PROMPT_REQUIRED");
            }

            var predicateProfile = DomainClassifierUnit.EncodePredicateProfile(
                    compiledPrompt.Compilation.DevelopmentPredicateIds.Concat(
                        compiledPrompt.Compilation.PipelinePredicates))
                .ToArray();
            if (predicateProfile.Length == 0)
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Compiled prompt did not contain trainable predicate ids.",
                    "DOMAIN_CLASSIFIER_PREDICATES_REQUIRED");
            }
            if (!IsValidDomainClassifierPredicateVector(predicateProfile))
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Compiled domain classifier predicate profile exceeds its bound.",
                    "DOMAIN_CLASSIFIER_STATE_INVALID");
            }

            var statePath = Path.GetFullPath(DomainClassifierTrainingPath());
            if (Directory.Exists(statePath) ||
                (File.Exists(statePath) &&
                 (File.GetAttributes(statePath) & FileAttributes.ReparsePoint) != 0))
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Domain classifier training state is invalid.",
                    "DOMAIN_CLASSIFIER_STATE_INVALID");
            }

            var priorStateExists = File.Exists(statePath);
            ClassifierPriorStateCustody? priorStateCustody = null;
            if (priorStateExists)
            {
                try
                {
                    priorStateCustody = cleanup.CaptureClassifierPriorState(
                        statePath);
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    return OperationResult<PreparedDomainClassifierPublication>.Fail(
                        "Domain classifier training state is invalid.",
                        "DOMAIN_CLASSIFIER_STATE_INVALID");
                }
            }
            var priorStateBytes = priorStateCustody?.Bytes;
            var existingExamplesResult = priorStateBytes is null
                ? OperationResult<DomainClassifierTrainingExample[]>.Ok([])
                : ParseDomainClassifierTrainingExamples(priorStateBytes);
            if (!existingExamplesResult.IsSuccess ||
                existingExamplesResult.Value is null)
            {
                return existingExamplesResult
                    .MapError<PreparedDomainClassifierPublication>();
            }
            var existingExamples = existingExamplesResult.Value;
            var examples = existingExamples
                .Append(new DomainClassifierTrainingExample(
                    request.RunId,
                    request.Domain,
                    predicateProfile,
                    DateTimeOffset.UtcNow))
                .GroupBy(
                    example =>
                        $"{example.Domain}:{string.Join(",", example.Predicates)}",
                    StringComparer.Ordinal)
                .Select(static group => group.Last())
                .ToArray();
            if (examples.Length > MaximumDomainClassifierExampleCount ||
                examples.Any(static example =>
                    !IsValidDomainClassifierTrainingExample(example)))
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Domain classifier training state exceeds its bound.",
                    "DOMAIN_CLASSIFIER_STATE_INVALID");
            }

            var classifier = new DomainClassifierUnit();
            classifier.Train(examples.Select(
                static example => (example.Predicates, example.Domain)).ToArray());
            var wrapper = new CompiledDomainClassifierUnit(classifier);
            var stateBytesResult = SerializeDomainClassifierTrainingExamples(
                examples);
            if (!stateBytesResult.IsSuccess || stateBytesResult.Value is null)
            {
                return stateBytesResult
                    .MapError<PreparedDomainClassifierPublication>();
            }
            var stateBytes = stateBytesResult.Value;
            var stagedStatePath = Path.Combine(
                stagingDirectory,
                $".domain-classifier-training-state-{invocationToken}.json");
            var stateDirectory = Path.GetDirectoryName(statePath)
                ?? throw new InvalidOperationException();
            var backupStatePath = Path.Combine(
                stateDirectory,
                $".domain-classifier-training-backup-{invocationToken}.json");
            var rollbackNewStatePath = Path.Combine(
                stateDirectory,
                $".domain-classifier-training-new-{invocationToken}.json");
            if (File.Exists(backupStatePath) ||
                Directory.Exists(backupStatePath) ||
                File.Exists(rollbackNewStatePath) ||
                Directory.Exists(rollbackNewStatePath))
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Domain classifier training transaction paths are unavailable.",
                    "DOMAIN_CLASSIFIER_STATE_INVALID");
            }
            var stateSha256 = ComputeBytesSha256(stateBytes);
            var stagedStateSha256 = cleanup.CreateAndTrackOwnedFile(
                stagedStatePath,
                stream => stream.Write(stateBytes));
            if (!StringComparer.Ordinal.Equals(
                    stagedStateSha256,
                    stateSha256))
            {
                return OperationResult<PreparedDomainClassifierPublication>.Fail(
                    "Domain classifier training state identity is invalid.",
                    "DOMAIN_CLASSIFIER_STATE_INVALID");
            }

            return OperationResult<PreparedDomainClassifierPublication>.Ok(
                new PreparedDomainClassifierPublication(
                    $"domain-classifier-{Sanitize(request.Domain.ToString())}-{Sanitize(request.RunId)}",
                    wrapper,
                    statePath,
                    stagedStatePath,
                    backupStatePath,
                    priorStateExists,
                    priorStateCustody,
                    priorStateCustody?.Proof.Sha256,
                    stateSha256));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<PreparedDomainClassifierPublication>.Fail(
                "Domain classifier training could not be prepared.",
                "DOMAIN_CLASSIFIER_TRAINING_PREPARATION_FAILED");
        }
    }

    private static OperationResult<DomainClassifierTrainingExample[]>
        ParseDomainClassifierTrainingExamples(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0 ||
            bytes.Length > MaximumDomainClassifierStateBytes)
        {
            return InvalidDomainClassifierTrainingState();
        }

        try
        {
            var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
            {
                MaxDepth = MaximumDomainClassifierJsonDepth,
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });
            var tokenCount = 0;
            if (!ReadDomainClassifierJsonToken(ref reader, ref tokenCount) ||
                reader.TokenType != JsonTokenType.StartArray)
            {
                return InvalidDomainClassifierTrainingState();
            }

            var examples = new List<DomainClassifierTrainingExample>();
            while (ReadDomainClassifierJsonToken(ref reader, ref tokenCount))
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (reader.Read())
                        return InvalidDomainClassifierTrainingState();
                    return OperationResult<DomainClassifierTrainingExample[]>.Ok(
                        examples.ToArray());
                }
                if (reader.TokenType != JsonTokenType.StartObject ||
                    examples.Count == MaximumDomainClassifierExampleCount)
                {
                    return InvalidDomainClassifierTrainingState();
                }

                const int runIdProperty = 1;
                const int domainProperty = 2;
                const int predicatesProperty = 4;
                const int createdAtUtcProperty = 8;
                const int allProperties = runIdProperty | domainProperty |
                    predicatesProperty | createdAtUtcProperty;
                var seenProperties = 0;
                string? runId = null;
                SemanticCompiler29.Domain? domain = null;
                int[]? predicates = null;
                DateTimeOffset? createdAtUtc = null;

                while (ReadDomainClassifierJsonToken(
                           ref reader,
                           ref tokenCount) &&
                       reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        return InvalidDomainClassifierTrainingState();

                    int property;
                    if (reader.ValueTextEquals("RunId"u8))
                        property = runIdProperty;
                    else if (reader.ValueTextEquals("Domain"u8))
                        property = domainProperty;
                    else if (reader.ValueTextEquals("Predicates"u8))
                        property = predicatesProperty;
                    else if (reader.ValueTextEquals("CreatedAtUtc"u8))
                        property = createdAtUtcProperty;
                    else
                        return InvalidDomainClassifierTrainingState();

                    if ((seenProperties & property) != 0 ||
                        !ReadDomainClassifierJsonToken(
                            ref reader,
                            ref tokenCount))
                    {
                        return InvalidDomainClassifierTrainingState();
                    }
                    seenProperties |= property;

                    switch (property)
                    {
                        case runIdProperty:
                            if (reader.TokenType != JsonTokenType.String ||
                                DomainClassifierJsonValueLength(reader) >
                                MaximumDomainClassifierEncodedStringBytes)
                                return InvalidDomainClassifierTrainingState();
                            runId = reader.GetString();
                            if (!IsValidDomainClassifierString(runId))
                                return InvalidDomainClassifierTrainingState();
                            break;

                        case domainProperty:
                            if (reader.TokenType != JsonTokenType.Number ||
                                !reader.TryGetInt32(out var domainValue) ||
                                !Enum.IsDefined(
                                    typeof(SemanticCompiler29.Domain),
                                    domainValue))
                            {
                                return InvalidDomainClassifierTrainingState();
                            }
                            domain = (SemanticCompiler29.Domain)domainValue;
                            break;

                        case predicatesProperty:
                            if (reader.TokenType != JsonTokenType.StartArray)
                                return InvalidDomainClassifierTrainingState();
                            var predicateValues = new List<int>();
                            while (ReadDomainClassifierJsonToken(
                                       ref reader,
                                       ref tokenCount) &&
                                   reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType != JsonTokenType.Number ||
                                    !reader.TryGetInt32(out var predicate) ||
                                    predicate < 0 ||
                                    predicate >= MaximumTrainingVectorLength ||
                                    predicateValues.Count ==
                                    MaximumDomainClassifierPredicateCount ||
                                    predicateValues.Contains(predicate))
                                {
                                    return InvalidDomainClassifierTrainingState();
                                }
                                predicateValues.Add(predicate);
                            }
                            if (reader.TokenType != JsonTokenType.EndArray ||
                                predicateValues.Count == 0)
                            {
                                return InvalidDomainClassifierTrainingState();
                            }
                            predicates = predicateValues.ToArray();
                            break;

                        case createdAtUtcProperty:
                            if (reader.TokenType != JsonTokenType.String ||
                                !reader.TryGetDateTimeOffset(
                                    out var createdAtValue) ||
                                createdAtValue.Offset != TimeSpan.Zero)
                            {
                                return InvalidDomainClassifierTrainingState();
                            }
                            createdAtUtc = createdAtValue;
                            break;
                    }
                }

                if (reader.TokenType != JsonTokenType.EndObject ||
                    seenProperties != allProperties ||
                    runId is null ||
                    domain is null ||
                    predicates is null ||
                    createdAtUtc is null)
                {
                    return InvalidDomainClassifierTrainingState();
                }

                var example = new DomainClassifierTrainingExample(
                    runId,
                    domain.Value,
                    predicates,
                    createdAtUtc.Value);
                if (!IsValidDomainClassifierTrainingExample(example))
                    return InvalidDomainClassifierTrainingState();
                examples.Add(example);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidDomainClassifierTrainingState();
        }

        return InvalidDomainClassifierTrainingState();
    }

    private static OperationResult<byte[]>
        SerializeDomainClassifierTrainingExamples(
            IReadOnlyList<DomainClassifierTrainingExample> examples)
    {
        if (examples.Count > MaximumDomainClassifierExampleCount ||
            examples.Any(static example =>
                !IsValidDomainClassifierTrainingExample(example)))
        {
            return OperationResult<byte[]>.Fail(
                "Domain classifier training state exceeds its bound.",
                "DOMAIN_CLASSIFIER_STATE_INVALID");
        }

        try
        {
            using var stream = new BoundedDomainClassifierStateStream(
                MaximumDomainClassifierStateBytes);
            JsonSerializer.Serialize(stream, examples);
            return OperationResult<byte[]>.Ok(stream.ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<byte[]>.Fail(
                "Domain classifier training state exceeds its bound.",
                "DOMAIN_CLASSIFIER_STATE_INVALID");
        }
    }

    private static bool ReadDomainClassifierJsonToken(
        ref Utf8JsonReader reader,
        ref int tokenCount)
    {
        if (!reader.Read())
            return false;
        tokenCount = checked(tokenCount + 1);
        if (tokenCount > MaximumDomainClassifierJsonTokenCount)
            throw new JsonException(
                "Domain classifier training state has too many JSON tokens.");
        return true;
    }

    private static long DomainClassifierJsonValueLength(
        in Utf8JsonReader reader)
        => reader.HasValueSequence
            ? reader.ValueSequence.Length
            : reader.ValueSpan.Length;

    private static bool IsValidDomainClassifierTrainingExample(
        DomainClassifierTrainingExample example)
        => IsValidDomainClassifierString(example.RunId) &&
           Enum.IsDefined(typeof(SemanticCompiler29.Domain), example.Domain) &&
           example.CreatedAtUtc.Offset == TimeSpan.Zero &&
           IsValidDomainClassifierPredicateVector(example.Predicates);

    private static bool IsValidDomainClassifierString(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumDomainClassifierStringLength &&
           StringComparer.Ordinal.Equals(value, value.Trim());

    private static bool IsValidDomainClassifierPredicateVector(
        int[]? predicates)
    {
        if (predicates is null ||
            predicates.Length is 0 or > MaximumDomainClassifierPredicateCount)
        {
            return false;
        }

        var distinct = new HashSet<int>();
        foreach (var predicate in predicates)
        {
            if (predicate < 0 ||
                predicate >= MaximumTrainingVectorLength ||
                !distinct.Add(predicate))
            {
                return false;
            }
        }
        return true;
    }

    private static OperationResult<DomainClassifierTrainingExample[]>
        InvalidDomainClassifierTrainingState()
        => OperationResult<DomainClassifierTrainingExample[]>.Fail(
            "Domain classifier training state is invalid.",
            "DOMAIN_CLASSIFIER_STATE_INVALID");

    private static OperationResult<bool>
        ValidateNoAbandonedClassifierTransactions(string canonicalRoot)
    {
        try
        {
            if (!Directory.Exists(canonicalRoot))
                return OperationResult<bool>.Ok(true);
            if ((File.GetAttributes(canonicalRoot) &
                 FileAttributes.ReparsePoint) != 0)
            {
                return AbandonedClassifierTransaction();
            }

            foreach (var path in Directory.EnumerateFileSystemEntries(
                         canonicalRoot,
                         ".domain-classifier-training-*",
                         SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(path);
                if (name.StartsWith(
                        ".domain-classifier-training-backup-",
                        StringComparison.Ordinal) ||
                    name.StartsWith(
                        ".domain-classifier-training-new-",
                        StringComparison.Ordinal) ||
                    name.StartsWith(
                        ".domain-classifier-training-transaction",
                        StringComparison.Ordinal))
                {
                    return AbandonedClassifierTransaction();
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AbandonedClassifierTransaction();
        }
    }

    private static OperationResult<bool> AbandonedClassifierTransaction()
        => OperationResult<bool>.Fail(
            "An abandoned domain classifier transaction requires recovery.",
            "DOMAIN_CLASSIFIER_TRANSACTION_RECOVERY_REQUIRED");

    private string DomainClassifierTrainingPath()
    {
        return Path.Combine(_checkpointRoot, "domain-classifier-training.json");
    }

    private static OperationResult<IReadOnlyList<TrainingBranchPlan>>
        BuildRegistrationPlans(
            IReadOnlyList<TrainingBranchPlan> branchPlans,
            string domainAdapterTag)
    {
        var groups = branchPlans
            .GroupBy(plan => plan.SubPhaseId, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal);
        var result = new List<TrainingBranchPlan>();
        foreach (var group in groups)
        {
                var first = group.First();
                var sourceBindings = group
                    .SelectMany(static plan => plan.SourceBindingSha256s)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .Take(LogicalCapabilityTrainingSourceBindingComposite.MaximumBindingCount + 1)
                    .ToArray();
                if (sourceBindings.Length > LogicalCapabilityTrainingSourceBindingComposite.MaximumBindingCount)
                    return OperationResult<IReadOnlyList<TrainingBranchPlan>>.Fail(
                        "Grouped logical training provenance exceeds its bound.",
                        "LOGICAL_CAPABILITY_TRAINING_COMPOSITE_TOO_LARGE");
                var replay = LogicalCapabilityTrainingPreferenceProjector
                    .ProjectAuthorizedReplay(
                        group.Select(static plan => plan.CurrentProjection)
                            .ToArray(),
                        sourceBindings,
                        first.TrainingContentProvenance.ReplaySealGroupSha256,
                        domainAdapterTag,
                        first.SubPhase);
                if (!replay.IsSuccess || replay.Value is null ||
                    group.Any(plan =>
                        !plan.TrainingContentProvenance.AuthorizesReplay(
                            replay.Value.Descriptor.ReplaySealGroupSha256,
                            replay.Value.Descriptor.SourceBindings.Values,
                            replay.Value.Descriptor.ReplayPayloadSha256,
                            replay.Value.Descriptor.DpoPreferenceSha256)))
                {
                    return OperationResult<IReadOnlyList<TrainingBranchPlan>>
                        .Fail(
                            "Grouped logical training provenance is invalid.",
                            TrainingContentProvenanceInvalidErrorCode);
                }
                var plan = first with
                {
                    TrainingText = replay.Value.TrainingText,
                    ChosenText = replay.Value.OutputText,
                    RejectedText = replay.Value.RejectedText,
                    TeachingVector = replay.Value.TeachingVector,
                    SourceBindingSha256s = sourceBindings,
                    AuthorizedReplayDescriptor = replay.Value.Descriptor,
                    AuthorizedReplayExampleId = replay.Value.ExampleId
                };
                result.Add(plan);
        }
        return OperationResult<IReadOnlyList<TrainingBranchPlan>>.Ok(result.ToArray());
    }

    // Keep capability adapters tiny enough for cumulative teaching on ordinary hardware.
    private static LoraConfig BuildCapabilityLoraConfig()
    {
        return new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
    }

    // Train a tiny tokenizer with enough vocabulary headroom for merged prompt tokens.
    private static BpeTokenizer BuildTokenizer(SemanticTeachingPromptRepresentation? compiledPrompt, string text)
    {
        var tokenizer = new BpeTokenizer();
        var seedText = compiledPrompt?.TokenizerSeedText ??
            $"{SemanticTeachingPromptCompiler.CanonicalPredicateSeed} capability generated learned flow selected rejected";
        var stableMergeSeed = string.Join(" ", Enumerable.Range(0, 512).Select(index => $"semantic_seed_{index:000}"));
        tokenizer.Train($"{seedText} {text} {stableMergeSeed}", 512);
        return tokenizer;
    }

    private static SemanticTeachingPromptRepresentation? CompileForTeaching(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return null;

        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        return compiled.IsSuccess ? compiled.Value : null;
    }

    // Keep capability training small enough to run on ordinary developer machines.
    private static ModelConfig BuildTinyConfig()
    {
        return new ModelConfig
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 32,
            NumLayers = 4,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 64
        };
    }

    private static OperationResult<LogicalDpoParentProvenance> BuildParentProvenance(
        GeneratedCapabilityCheckpointManifest parent)
    {
        if (parent.RopeTheta is null
            || parent.RmsNormEps is null
            || !float.IsFinite(parent.RopeTheta.Value)
            || parent.RopeTheta <= 0f
            || !float.IsFinite(parent.RmsNormEps.Value)
            || parent.RmsNormEps <= 0f
            || !RuntimeProvenance.IsSha256Hex(parent.TokenizerSha256)
            || !RuntimeProvenance.IsSha256Hex(parent.PolicyCheckpointSha256)
            || !RuntimeProvenance.IsSha256Hex(parent.RuntimeProvenanceSha256)
            || string.IsNullOrWhiteSpace(parent.TokenizerPath)
            || string.IsNullOrWhiteSpace(parent.PolicyCheckpointPath))
        {
            return OperationResult<LogicalDpoParentProvenance>.Fail(
                "Parent capability provenance is incomplete or invalid.",
                "PARENT_CAPABILITY_PROVENANCE_INVALID");
        }

        var config = new ModelConfig
        {
            VocabSize = parent.VocabSize,
            ContextLen = parent.ContextLen,
            HiddenDim = parent.HiddenDim,
            NumLayers = parent.NumLayers,
            NumHeads = parent.NumHeads,
            NumKvHeads = parent.NumKvHeads,
            FfnHiddenDim = parent.FfnHiddenDim,
            RopeTheta = parent.RopeTheta.Value,
            RmsNormEps = parent.RmsNormEps.Value
        };
        var lora = RuntimeProvenance.CanonicalizeLora(new LoraConfig
        {
            Rank = parent.LoraRank,
            Alpha = parent.LoraAlpha,
            Dropout = parent.LoraDropout,
            TargetModules = parent.LoraTargetModules
        });
        if (!lora.IsSuccess)
        {
            return OperationResult<LogicalDpoParentProvenance>.Fail(
                "Parent capability provenance is incomplete or invalid.",
                "PARENT_CAPABILITY_PROVENANCE_INVALID");
        }

        var recomputedProvenance = RuntimeProvenance.ComputeSha256(
            config,
            lora.Value!,
            parent.TokenizerSha256!,
            parent.PolicyCheckpointSha256!);
        if (!recomputedProvenance.IsSuccess
            || !RuntimeProvenance.FixedTimeSha256Equals(
                parent.RuntimeProvenanceSha256!,
                recomputedProvenance.Value!))
        {
            return OperationResult<LogicalDpoParentProvenance>.Fail(
                "Parent capability provenance is incomplete or invalid.",
                "PARENT_CAPABILITY_PROVENANCE_INVALID");
        }

        return OperationResult<LogicalDpoParentProvenance>.Ok(
            new LogicalDpoParentProvenance(
                config,
                lora.Value!,
                parent.TokenizerPath,
                parent.TokenizerSha256!,
                parent.PolicyCheckpointSha256!,
                parent.RuntimeProvenanceSha256!));
    }

    private static string ComputeFileSha256(
        string path,
        FileShare share = FileShare.Read)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            share);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static OperationResult<string> BuildCombinedTrainingText(
        IReadOnlyList<string>? texts)
    {
        if (texts is null || texts.Count > LogicalCapabilityTrainingAdmissionBatch.MaximumRowCount)
            return OperationResult<string>.Fail("Combined logical training text is invalid.",
                "LOGICAL_CAPABILITY_TRAINING_COMPOSITE_TOO_LARGE");
        long characters = Math.Max(0, texts.Count - 1);
        long utf8Bytes = Math.Max(0, texts.Count - 1);
        foreach (var text in texts)
        {
            if (text is null)
                return OperationResult<string>.Fail("Combined logical training text is invalid.",
                    "LOGICAL_CAPABILITY_TRAINING_COMPOSITE_TOO_LARGE");
            characters += text.Length;
            utf8Bytes += Encoding.UTF8.GetByteCount(text);
            if (characters > MaximumTrainingTextLength ||
                utf8Bytes > MaximumTrainingTextLength)
                return OperationResult<string>.Fail("Combined logical training text exceeds its bound.",
                    "LOGICAL_CAPABILITY_TRAINING_COMPOSITE_TOO_LARGE");
        }
        return OperationResult<string>.Ok(string.Join(" ", texts));
    }

    private static string ComputeBytesSha256(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void CaptureFirstCleanupFailure(Exception exception)
    {
        if (LastCleanupFailure.Value is null)
            LastCleanupFailure.Value = exception;
    }

    private static void CaptureCleanupException(
        Exception exception,
        ref Exception? firstFatal,
        List<Exception>? failures = null)
    {
        var isNonFatal = IsExceptionGraphNonFatalConservatively(exception);
        if (!isNonFatal && firstFatal is null)
            firstFatal = exception;
        try
        {
            failures?.Add(exception);
        }
        catch
        {
            // Diagnostics are optional once the exact fatal reference is held.
        }
        if (isNonFatal)
        {
            try
            {
                CaptureFirstCleanupFailure(exception);
            }
            catch
            {
                // Diagnostics must not replace the observed cleanup failure.
            }
        }
    }

    private static void AttemptCleanup(
        Action cleanup,
        ref Exception? firstFatal,
        List<Exception>? failures = null)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            CaptureCleanupException(exception, ref firstFatal, failures);
        }
    }

    private static void ThrowFirstFatalWithFailures(
        Exception? firstFatal,
        IReadOnlyList<Exception> failures)
    {
        if (firstFatal is null)
            return;
        try
        {
            var attached = failures
                .Where(failure => !ReferenceEquals(failure, firstFatal))
                .ToArray();
            if (attached.Length > 0)
            {
                firstFatal.Data["CAPABILITY_TRAINING_CLEANUP_FAILURE"] =
                    attached.Length == 1
                        ? attached[0]
                        : new AggregateException(attached);
            }
        }
        catch
        {
            // Diagnostics must never replace the established fatal exception.
        }
        ThrowExactException(firstFatal);
    }

    private static bool IsExceptionGraphNonFatalConservatively(
        Exception exception)
    {
        try
        {
            return SnapshotExceptionGraphClassifier.IsNonFatal(exception);
        }
        catch
        {
            // A classifier failure is itself unsafe; preserve and propagate the
            // exact already-observed exception as the conservative winner.
            return false;
        }
    }

    private static void ThrowExactException(Exception exception)
    {
        try
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
        catch (Exception dispatchFailure)
            when (!ReferenceEquals(dispatchFailure, exception))
        {
            // If preserving the dispatch stack itself fails (notably under
            // memory pressure), retain object identity even at the cost of a
            // reset throw site.
            throw exception;
        }

        throw exception;
    }

    private sealed class OperationCleanupArbiter
    {
        private readonly object _gate = new();
        private readonly List<Exception> _failures = [];
        private Exception? _firstFatal;

        internal TrackedDisposable<T> Track<T>(T disposable)
            where T : IDisposable
            => new(disposable, this);

        private void Record(Exception exception)
        {
            var isNonFatal = IsNonFatalConservatively(exception);
            if (!isNonFatal)
                Interlocked.CompareExchange(ref _firstFatal, exception, null);
            try
            {
                CleanupArbiterFaultObserver.Value?.Invoke("record");
                lock (_gate)
                {
                    if (!_failures.Any(existing =>
                            ReferenceEquals(existing, exception)))
                    {
                        _failures.Add(exception);
                    }
                }
            }
            catch
            {
                // Optional ordering/diagnostics cannot replace a retained
                // primary or fatal cleanup reference.
            }
            if (isNonFatal)
            {
                try
                {
                    CaptureFirstCleanupFailure(exception);
                }
                catch
                {
                    // Last-cleanup diagnostics are best effort only.
                }
            }
        }

        internal void ThrowPendingFatal()
        {
            var fatal = Volatile.Read(ref _firstFatal);
            if (fatal is not null)
                ThrowExactException(fatal);
        }

        internal void ThrowWinner(Exception? primary)
        {
            var primaryFatal = primary is not null &&
                !IsNonFatalConservatively(primary);
            var firstFatalCleanup = Volatile.Read(ref _firstFatal);
            var winner = primaryFatal ? primary : firstFatalCleanup is null
                ? primary
                : firstFatalCleanup;
            if (winner is null)
                return;
            TryAttachDiagnostics(winner, primary);
            if (!ReferenceEquals(winner, primary))
                ThrowExactException(winner);
        }

        private void TryAttachDiagnostics(Exception winner, Exception? primary)
        {
            try
            {
                CleanupArbiterFaultObserver.Value?.Invoke("diagnostics");
                Exception[] failures;
                lock (_gate)
                    failures = _failures.ToArray();
                const string cleanupFailureKey =
                    "CAPABILITY_TRAINING_CLEANUP_FAILURE";
                var attached = new List<Exception>();
                var seen = new HashSet<Exception>(
                    ReferenceEqualityComparer.Instance)
                {
                    winner
                };
                void AddFlattened(Exception root)
                {
                    var pending = new Stack<Exception>();
                    pending.Push(root);
                    while (pending.Count > 0)
                    {
                        var exception = pending.Pop();
                        if (exception is AggregateException aggregate)
                        {
                            for (var index = aggregate.InnerExceptions.Count - 1;
                                 index >= 0;
                                 index--)
                            {
                                pending.Push(aggregate.InnerExceptions[index]);
                            }
                            continue;
                        }
                        if (!seen.Add(exception))
                            continue;
                        attached.Add(exception);
                        if (exception.Data[cleanupFailureKey]
                            is Exception nested)
                        {
                            pending.Push(nested);
                        }
                    }
                }
                if (winner.Data[cleanupFailureKey]
                    is Exception existing)
                    AddFlattened(existing);
                if (primary is not null &&
                    !ReferenceEquals(primary, winner))
                    AddFlattened(primary);
                foreach (var failure in failures)
                {
                    if (!ReferenceEquals(failure, winner))
                        AddFlattened(failure);
                }
                if (attached.Count > 0)
                    winner.Data[cleanupFailureKey] =
                        attached.Count == 1 ? attached[0] : new AggregateException(attached);
            }
            catch
            {
                // Diagnostics are strictly best effort after winner selection.
            }
        }

        private static bool IsNonFatalConservatively(Exception exception)
        {
            try
            {
                CleanupArbiterFaultObserver.Value?.Invoke("classification");
                return SnapshotExceptionGraphClassifier.IsNonFatal(exception);
            }
            catch
            {
                return false;
            }
        }

        internal sealed class TrackedDisposable<T> : IDisposable where T : IDisposable
        {
            private readonly OperationCleanupArbiter _owner;
            private int _disposed;
            internal TrackedDisposable(T inner, OperationCleanupArbiter owner)
            { Inner = inner; _owner = owner; }
            internal T Inner { get; }
            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
                try { Inner.Dispose(); }
                catch (Exception exception) { _owner.Record(exception); }
            }
        }
    }

    // Build a stable logical unit family id for the domain adapter capability.
    private static string BuildUnitFamilyId(string domainAdapterTag)
    {
        return $"{GeneratedCapabilityUnitRole}-{Sanitize(domainAdapterTag)}";
    }

    // Increment semantic patch version for the next checkpoint in a unit-family chain.
    private static string IncrementPatchVersion(string version)
    {
        var parts = (version ?? string.Empty).Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var major) &&
            int.TryParse(parts[1], out var minor) &&
            int.TryParse(parts[2], out var patch))
            return $"{major}.{minor}.{patch + 1}";

        return "1.0.1";
    }

    private static string BuildGeneratedCapabilityFlowId(
        string domainAdapterTag,
        string subPhaseId)
    {
        var domainBytes = Encoding.UTF8.GetBytes(domainAdapterTag ?? string.Empty);
        var subPhaseBytes = Encoding.UTF8.GetBytes(subPhaseId ?? string.Empty);
        using var payload = new MemoryStream();
        WriteLengthPrefixed(payload, domainBytes);
        WriteLengthPrefixed(payload, subPhaseBytes);
        var digest = SHA256.HashData(payload.ToArray());
        return $"{GeneratedCapabilityFlowIdPrefix}-v2-{Convert.ToHexString(digest).ToLowerInvariant()}";
    }

    private static void WriteLengthPrefixed(Stream destination, byte[] value)
    {
        var prefix = Encoding.ASCII.GetBytes(
            value.Length.ToString(CultureInfo.InvariantCulture) + ":");
        destination.Write(prefix, 0, prefix.Length);
        destination.Write(value, 0, value.Length);
    }

    // Sanitize ids so generated registry and checkpoint names are stable.
    private static string Sanitize(string value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        var chars = safeValue.Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-').ToArray();
        return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private sealed record TeachingStack(
        LlmModel PolicyModel,
        BpeTokenizer Tokenizer,
        GeneratedCapabilityCheckpointManifest? ParentManifest,
        string Version,
        int TrainingStepCount,
        string? ParentPolicyCheckpointPath,
        LogicalDpoParentProvenance? ParentProvenance,
        bool UsedWarmCache);

    private sealed record CachedTeachingStack(
        LlmModel PolicyModel,
        BpeTokenizer Tokenizer,
        GeneratedCapabilityCheckpointManifest Manifest);

    private sealed record PreparedKnnCorpus(
        IReadOnlyList<GeneratedCapabilityKnnExample> ManifestExamples,
        TransformerKnnTrainingExample<string>[] RuntimeExamples,
        string Sha256);

    private sealed record PublicationPlan(
        string SubPhaseId,
        string CheckpointDirectory,
        string UnitId,
        string TokenizerPath,
        string PolicyCheckpointPath,
        string PolicyTemporaryPath,
        string ManifestPath,
        string StagingDirectory,
        string StagingMarkerPath,
        string StagingTokenizerPath,
        string StagingPolicyCheckpointPath,
        string StagingManifestPath,
        string DpoMetricsDirectory,
        string StagingDpoMetricsDirectory,
        string InvocationToken,
        string CanonicalRoot,
        string RootPrefix,
        StringComparison PathComparison)
    {
        public string[] ProtectedArtifactPaths =>
            [TokenizerPath, PolicyCheckpointPath, PolicyTemporaryPath, ManifestPath];
    }

    private sealed record PreparedCapabilityPublication(
        TrainingBranchPlan Plan,
        PublicationPlan Publication,
        string FlowId,
        string ScenarioId,
        ILogicalUnit<string, string> RetrievalUnit,
        ILogicalFlowFactory<string> FlowFactory,
        GeneratedCapabilityCheckpointManifest Manifest,
        string ManifestArtifactSha256,
        DurableDpoMetricPublication[] DpoMetrics,
        double SaveArtifactsMs,
        double WrapUnitMs,
        double SaveManifestMs,
        double PreparationMs);

    private sealed record DurableDpoMetricPublication(
        string SourcePath,
        string StagingPath,
        string FinalPath,
        string Sha256);

    private sealed record PreparedDomainClassifierPublication(
        string UnitId,
        CompiledDomainClassifierUnit Unit,
        string StatePath,
        string StagedStatePath,
        string BackupStatePath,
        bool PriorStateExists,
        ClassifierPriorStateCustody? PriorStateCustody,
        string? PriorStateSha256,
        string StateSha256);

    private sealed class ClassifierPriorStateCustody : IDisposable
    {
        private FileStream? _retainedIdentity;

        internal ClassifierPriorStateCustody(
            string path,
            byte[] bytes,
            DpoTrainerArtifactOwnershipProof proof,
            FileStream retainedIdentity)
        {
            Path = path;
            Bytes = bytes;
            Proof = proof;
            _retainedIdentity = retainedIdentity;
        }

        internal string Path { get; }
        internal byte[] Bytes { get; }
        internal DpoTrainerArtifactOwnershipProof Proof { get; }

        internal FileStream? TakeRetainedIdentity()
            => Interlocked.Exchange(ref _retainedIdentity, null);

        public void Dispose()
        {
            try
            {
                TakeRetainedIdentity()?.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                CaptureFirstCleanupFailure(exception);
            }
        }
    }

    private sealed class PublishedArtifactHandleSet : IDisposable
    {
        private readonly PublishedArtifactHandles?[] _items;
        private readonly InvocationPublicationCleanup _cleanup;
        private bool _disposed;

        internal PublishedArtifactHandleSet(
            int capacity,
            InvocationPublicationCleanup cleanup)
        {
            _items = new PublishedArtifactHandles?[capacity];
            _cleanup = cleanup;
        }

        internal void Prepare(
            int index,
            PreparedCapabilityPublication prepared)
            => _items[index] = new PublishedArtifactHandles(
                prepared,
                _cleanup);

        internal void AcquireTokenizer(int index)
            => _items[index]!.AcquireTokenizer();

        internal void AcquirePolicy(int index)
            => _items[index]!.AcquirePolicy();

        internal void AcquireManifest(int index)
            => _items[index]!.AcquireManifest();

        internal void AcquireMetric(int index, int metricIndex)
            => _items[index]!.AcquireMetric(metricIndex);

        internal OperationResult<bool> VerifyFinalPathIdentity()
        {
            try
            {
                foreach (var item in _items)
                {
                    if (item is null || !item.StillMatchesFinalPaths())
                    {
                        return OperationResult<bool>.Fail(
                            "Generated capability publication identity changed.",
                            "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
                    }
                }

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(
                    "Generated capability publication identity changed.",
                    "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            for (var index = _items.Length - 1; index >= 0; index--)
                _items[index]?.Dispose();
        }

        private sealed class PublishedArtifactHandles : IDisposable
        {
            private readonly PreparedCapabilityPublication _prepared;
            private readonly InvocationPublicationCleanup _cleanup;

            internal PublishedArtifactHandles(
                PreparedCapabilityPublication prepared,
                InvocationPublicationCleanup cleanup)
            {
                _prepared = prepared;
                _cleanup = cleanup;
            }

            internal void AcquireTokenizer()
                => EnsureMatches(
                    _prepared.Publication.TokenizerPath,
                    _prepared.Manifest.TokenizerSha256!);

            internal void AcquirePolicy()
                => EnsureMatches(
                    _prepared.Publication.PolicyCheckpointPath,
                    _prepared.Manifest.PolicyCheckpointSha256!);

            internal void AcquireManifest()
                => EnsureMatches(
                    _prepared.Publication.ManifestPath,
                    _prepared.ManifestArtifactSha256);

            internal void AcquireMetric(int index)
                => EnsureMatches(
                    _prepared.DpoMetrics[index].FinalPath,
                    _prepared.DpoMetrics[index].Sha256);

            internal bool StillMatchesFinalPaths()
            {
                var prepared = _prepared;
                var publication = prepared.Publication;
                var markerPath = Path.Combine(
                    publication.CheckpointDirectory,
                    Path.GetFileName(publication.StagingMarkerPath));
                var comparer = OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;
                var expectedTopLevel = new HashSet<string>(comparer)
                {
                    Path.GetFullPath(markerPath),
                    Path.GetFullPath(publication.TokenizerPath),
                    Path.GetFullPath(publication.PolicyCheckpointPath),
                    Path.GetFullPath(publication.ManifestPath)
                };
                if (prepared.DpoMetrics.Length > 0)
                {
                    expectedTopLevel.Add(Path.GetFullPath(
                        publication.DpoMetricsDirectory));
                }

                var actualTopLevel = Directory
                    .EnumerateFileSystemEntries(
                        publication.CheckpointDirectory,
                        "*",
                        SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFullPath)
                    .ToHashSet(comparer);
                if (!expectedTopLevel.SetEquals(actualTopLevel) ||
                    !_cleanup.TryGetOwnedFileSha256(
                        markerPath,
                        out var markerSha256) ||
                    !_cleanup.GenericOwnedPathStillMatches(
                        markerPath,
                        markerSha256) ||
                    !_cleanup.GenericOwnedPathStillMatches(
                        publication.TokenizerPath,
                        prepared.Manifest.TokenizerSha256!) ||
                    !_cleanup.GenericOwnedPathStillMatches(
                        publication.PolicyCheckpointPath,
                        prepared.Manifest.PolicyCheckpointSha256!) ||
                    !_cleanup.GenericOwnedPathStillMatches(
                        publication.ManifestPath,
                        prepared.ManifestArtifactSha256))
                {
                    return false;
                }

                if (prepared.DpoMetrics.Length == 0)
                    return true;

                var expectedMetricPaths = prepared.DpoMetrics
                    .Select(static metric => Path.GetFullPath(metric.FinalPath))
                    .ToHashSet(comparer);
                var actualMetricPaths = Directory
                    .EnumerateFileSystemEntries(
                        publication.DpoMetricsDirectory,
                        "*",
                        SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFullPath)
                    .ToHashSet(comparer);
                if (!expectedMetricPaths.SetEquals(actualMetricPaths))
                    return false;
                for (var index = 0;
                     index < prepared.DpoMetrics.Length;
                     index++)
                {
                    var metric = prepared.DpoMetrics[index];
                    if (!_cleanup.GenericOwnedPathStillMatches(
                            metric.FinalPath,
                            metric.Sha256))
                    {
                        return false;
                    }
                }

                return true;
            }

            private void EnsureMatches(string path, string sha256)
            {
                if (!_cleanup.GenericOwnedPathStillMatches(path, sha256))
                    throw new IOException(
                        "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
            }

            public void Dispose()
            {
            }
        }
    }

    private sealed class ClassifierStateHandleSet : IDisposable
    {
        private readonly PreparedDomainClassifierPublication _prepared;
        private readonly InvocationPublicationCleanup _cleanup;
        private FileStream? _priorTransition;
        private FileStream? _backup;
        private DpoTrainerArtifactOwnershipProof? _priorProof;

        internal ClassifierStateHandleSet(
            PreparedDomainClassifierPublication prepared,
            InvocationPublicationCleanup cleanup)
        {
            _prepared = prepared;
            _cleanup = cleanup;
        }

        internal void AcquireActive()
        {
            if (!_cleanup.GenericOwnedPathStillMatches(
                    _prepared.StatePath,
                    _prepared.StateSha256))
            {
                throw new IOException(
                    "DOMAIN_CLASSIFIER_ACTIVE_IDENTITY_CHANGED");
            }
        }

        internal void AcquirePriorTransition()
        {
            var custody = _prepared.PriorStateCustody ??
                throw new InvalidOperationException(
                    "DOMAIN_CLASSIFIER_PRIOR_CUSTODY_MISSING");
            var transition = custody.TakeRetainedIdentity() ??
                throw new InvalidOperationException(
                    "DOMAIN_CLASSIFIER_PRIOR_CUSTODY_ALREADY_TRANSFERRED");
            _priorTransition = transition;
            _priorProof = custody.Proof;
            if (!string.Equals(
                    Path.GetFullPath(custody.Path),
                    Path.GetFullPath(_prepared.StatePath),
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal) ||
                !InvocationPublicationCleanup.DpoProofMatchesStream(
                    transition,
                    custody.Proof) ||
                !InvocationPublicationCleanup.DpoPathMatchesProof(
                    _prepared.StatePath,
                    custody.Proof,
                    FileShare.Read | FileShare.Delete) ||
                custody.Proof.LinkCount != 1 ||
                !StringComparer.Ordinal.Equals(
                    custody.Proof.Sha256,
                    _prepared.PriorStateSha256))
            {
                throw new IOException(
                    "DOMAIN_CLASSIFIER_PRIOR_IDENTITY_CHANGED");
            }
        }

        internal bool RenamePriorToBackupIdentityBound()
        {
            var transition = _priorTransition;
            var proof = _priorProof;
            if (transition is null || proof is null ||
                !HandleAndPathMatch(
                    transition,
                    _prepared.StatePath) ||
                File.Exists(_prepared.BackupStatePath) ||
                Directory.Exists(_prepared.BackupStatePath))
            {
                return false;
            }

            ClassifierIdentityRenameObserver.Value?.Invoke(
                "before_prior_active_to_backup_identity_rename",
                _prepared.StatePath,
                _prepared.BackupStatePath);
            return RenameExactIdentity(
                transition,
                _prepared.StatePath,
                _prepared.BackupStatePath);
        }

        internal void AcquireBackupAfterMove()
        {
            var proof = _priorProof ?? throw new InvalidOperationException();
            var transition = _priorTransition ??
                throw new InvalidOperationException(
                    "DOMAIN_CLASSIFIER_PRIOR_TRANSITION_MISSING");
            var intermediate = OpenClassifierHandle(
                _prepared.BackupStatePath,
                ClassifierGenericReadAccess,
                FileShare.Read | FileShare.Delete);
            if (!InvocationPublicationCleanup.DpoProofMatchesStream(
                    intermediate,
                    proof) ||
                !HandleAndPathMatch(
                    intermediate,
                    _prepared.BackupStatePath))
            {
                intermediate.Dispose();
                throw new IOException(
                    "DOMAIN_CLASSIFIER_BACKUP_IDENTITY_CHANGED");
            }

            _priorTransition = intermediate;
            DisposeNonMasking(transition);
            FileStream? protective = null;
            try
            {
                var injectedFailure =
                    ClassifierBackupProtectionFailureFactory.Value?.Invoke();
                if (injectedFailure is not null)
                    throw injectedFailure;
                protective = OpenIdentityProtectingRead(
                    _prepared.BackupStatePath);
                if (!InvocationPublicationCleanup.DpoProofMatchesStream(
                        protective,
                        proof) ||
                    !HandleAndPathMatch(
                        protective,
                        _prepared.BackupStatePath))
                {
                    throw new IOException(
                        "DOMAIN_CLASSIFIER_BACKUP_IDENTITY_CHANGED");
                }

                _backup = protective;
                protective = null;
                DisposeNonMasking(Interlocked.Exchange(
                    ref _priorTransition,
                    null));
            }
            finally
            {
                DisposeNonMasking(protective);
            }
        }

        internal bool StillMatchesActivePath()
            => _cleanup.GenericOwnedPathStillMatches(
                   _prepared.StatePath,
                   _prepared.StateSha256);

        internal bool StillMatchesBackupPath()
            => !_prepared.PriorStateExists ||
               (_backup is not null &&
                HandleAndPathMatch(
                    _backup,
                    _prepared.BackupStatePath));

        internal void ReleaseBackup()
            => DisposeNonMasking(Interlocked.Exchange(ref _backup, null));

        internal bool DeleteBackupIdentityBound()
        {
            var backup = _backup;
            if (backup is null ||
                !HandleAndPathMatch(
                    backup,
                    _prepared.BackupStatePath))
            {
                return false;
            }

            if (OperatingSystem.IsWindows())
            {
                var disposition = new ClassifierFileDispositionInformation
                {
                    DeleteFile = 1
                };
                if (!SetClassifierFileInformationByHandleWindows(
                        backup.SafeFileHandle,
                        ClassifierFileDispositionInfoClass,
                        ref disposition,
                        (uint)Marshal.SizeOf<ClassifierFileDispositionInformation>()))
                {
                    return false;
                }
            }
            else
            {
                // POSIX не защищает имя открытым handle так, как Windows. Перед
                // unlink мы уже доказали, что backup path всё ещё указывает на
                // удержанную identity; helper повторяет no-follow сверку и не
                // удаляет чужую замену.
                LogicalCapabilityFileCustody.MarkIdentityForDeletion(
                    backup,
                    _prepared.BackupStatePath);
            }

            ReleaseBackup();
            return !File.Exists(_prepared.BackupStatePath) &&
                   !Directory.Exists(_prepared.BackupStatePath);
        }

        internal bool RestoreBackupIdentityBound()
        {
            if (!_prepared.PriorStateExists)
                return true;
            // The exact transition handle remains rollback authority until a
            // stronger delete-protecting backup handle has been installed.
            // This closes the rename-to-protection failure window without ever
            // selecting a replacement by path alone.
            var backup = _backup ?? _priorTransition;
            var proof = _priorProof;
            if (backup is null || proof is null ||
                !HandleAndPathMatch(backup, _prepared.BackupStatePath) ||
                File.Exists(_prepared.StatePath) ||
                Directory.Exists(_prepared.StatePath))
            {
                return false;
            }

            ClassifierIdentityRenameObserver.Value?.Invoke(
                "before_prior_backup_to_active_identity_rename",
                _prepared.BackupStatePath,
                _prepared.StatePath);
            if (_backup is not null)
            {
                backup = AcquireRollbackTransition();
                if (backup is null ||
                    !HandleAndPathMatch(
                        backup,
                        _prepared.BackupStatePath))
                {
                    return false;
                }
            }
            return RenameExactIdentity(
                       backup,
                       _prepared.BackupStatePath,
                       _prepared.StatePath) &&
                   HandleAndPathMatch(
                        backup,
                       _prepared.StatePath);
        }

        private FileStream? AcquireRollbackTransition()
        {
            var proof = _priorProof;
            var protective = _backup;
            if (proof is null || protective is null ||
                !HandleAndPathMatch(protective, _prepared.BackupStatePath))
            {
                return null;
            }

            FileStream? transition = null;
            try
            {
                transition = OpenClassifierHandle(
                    _prepared.BackupStatePath,
                    ClassifierGenericReadAccess,
                    FileShare.Read | FileShare.Delete);
                if (!InvocationPublicationCleanup.DpoProofMatchesStream(
                        transition,
                        proof) ||
                    !HandleAndPathMatch(
                        transition,
                        _prepared.BackupStatePath))
                {
                    return null;
                }

                _priorTransition = transition;
                transition = null;
                ReleaseBackup();
                return _priorTransition;
            }
            finally
            {
                DisposeNonMasking(transition);
            }
        }

        internal void ReleaseForCommit()
        {
            Exception? firstFatal = null;
            var failures = new List<Exception>();
            AttemptCleanup(ReleaseBackup, ref firstFatal, failures);
            var prior = Interlocked.Exchange(ref _priorTransition, null);
            AttemptCleanup(
                () => DisposeNonMasking(prior),
                ref firstFatal,
                failures);
            ThrowFirstFatalWithFailures(firstFatal, failures);
        }

        public void Dispose()
            => ReleaseForCommit();

        private static FileStream OpenIdentityProtectingRead(string path)
            => OpenClassifierHandle(
                path,
                ClassifierGenericReadAccess | ClassifierDeleteAccess,
                FileShare.Read);

        private static FileStream OpenClassifierHandle(
            string path,
            uint desiredAccess,
            FileShare share)
        {
            if (!OperatingSystem.IsWindows())
            {
                var stream = LogicalCapabilityFileCustody.OpenIdentityStream(
                    path,
                    desiredAccess,
                    share);
                if (stream is not null)
                    return stream;
                throw new IOException(
                    "DOMAIN_CLASSIFIER_IDENTITY_HANDLE_OPEN_FAILED");
            }

            var handle = CreateClassifierFileWindows(
                path,
                desiredAccess,
                (uint)share,
                IntPtr.Zero,
                ClassifierOpenExisting,
                ClassifierFileAttributeNormal,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                var nativeError = Marshal.GetLastWin32Error();
                handle.Dispose();
                throw new IOException(
                    $"DOMAIN_CLASSIFIER_IDENTITY_HANDLE_OPEN_FAILED:{nativeError}");
            }
            return new FileStream(handle, FileAccess.Read);
        }

        private bool HandleAndPathMatch(
            FileStream retained,
            string path)
        {
            return _priorProof is not null &&
                   InvocationPublicationCleanup.DpoProofMatchesStream(
                       retained,
                       _priorProof) &&
                   InvocationPublicationCleanup.DpoPathMatchesProof(
                       path,
                       _priorProof,
                       FileShare.Read | FileShare.Delete);
        }

        private bool RenameExactIdentity(
            FileStream retained,
            string expectedSourcePath,
            string destinationPath)
        {
            var proof = _priorProof;
            if (proof is null ||
                !InvocationPublicationCleanup.DpoProofMatchesStream(
                    retained,
                    proof))
            {
                return false;
            }

            var sourcePath = OperatingSystem.IsWindows()
                ? GetClassifierPathFromHandle(retained)
                : Path.GetFullPath(expectedSourcePath);
            if (sourcePath is null ||
                !InvocationPublicationCleanup.DpoPathMatchesProof(
                    sourcePath,
                    proof,
                    FileShare.Read | FileShare.Delete))
            {
                return false;
            }

            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    // Linux-путь переносим только через identity-bound helper:
                    // source уже доказан retained proof'ом, destination создаётся
                    // атомарно без перезаписи чужой замены.
                    return LogicalCapabilityFileCustody.TryRenameIdentityNoReplace(
                               retained,
                               proof,
                               sourcePath,
                               destinationPath,
                               FileShare.Read | FileShare.Delete) &&
                           HandleAndPathMatch(
                               retained,
                               destinationPath);
                }

                if (File.Exists(destinationPath) ||
                    Directory.Exists(destinationPath))
                {
                    return false;
                }
                File.Move(sourcePath, destinationPath, overwrite: false);
                return HandleAndPathMatch(retained, destinationPath);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return false;
            }
        }

        private static string? GetClassifierPathFromHandle(FileStream retained)
        {
            var buffer = new StringBuilder(32_768);
            var length = GetClassifierFinalPathNameByHandleWindows(
                retained.SafeFileHandle,
                buffer,
                (uint)buffer.Capacity,
                0);
            if (length == 0 || length >= buffer.Capacity)
                return null;
            var value = buffer.ToString();
            if (value.StartsWith(
                    @"\\?\UNC\",
                    StringComparison.OrdinalIgnoreCase))
            {
                value = @"\\" + value[8..];
            }
            else if (value.StartsWith(
                         @"\\?\",
                         StringComparison.OrdinalIgnoreCase))
            {
                value = value[4..];
            }
            return Path.GetFullPath(value);
        }

        private static void DisposeNonMasking(IDisposable? disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                CaptureFirstCleanupFailure(exception);
            }
        }

        private const int ClassifierFileDispositionInfoClass = 4;
        private const uint ClassifierGenericReadAccess = 0x80000000;
        private const uint ClassifierDeleteAccess = 0x00010000;
        private const uint ClassifierOpenExisting = 3;
        private const uint ClassifierFileAttributeNormal = 0x00000080;

        [StructLayout(LayoutKind.Sequential)]
        private struct ClassifierFileDispositionInformation
        {
            // FILE_DISPOSITION_INFO.DeleteFile is Win32 BOOLEAN (one byte),
            // not the four-byte BOOL used by UnmanagedType.Bool.
            internal byte DeleteFile;
        }

        [DllImport(
            "kernel32.dll",
            EntryPoint = "CreateFileW",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern SafeFileHandle CreateClassifierFileWindows(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport(
            "kernel32.dll",
            EntryPoint = "SetFileInformationByHandle",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetClassifierFileInformationByHandleWindows(
            SafeFileHandle fileHandle,
            int fileInformationClass,
            ref ClassifierFileDispositionInformation fileInformation,
            uint bufferSize);

        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetFinalPathNameByHandleW",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern uint GetClassifierFinalPathNameByHandleWindows(
            SafeFileHandle fileHandle,
            StringBuilder filePath,
            uint filePathLength,
            uint flags);
    }

    private sealed class CapabilityPublicationTransaction : IDisposable
    {
        private readonly DefaultLogicalCapabilityTrainer _owner;
        private readonly InMemoryLogicalUnitRegistry _units;
        private readonly InMemoryLogicalFlowRegistry _flows;
        private readonly InvocationPublicationCleanup _cleanup;
        private readonly LogicalUnitRegistrationToken?[] _unitTokens;
        private readonly LogicalFlowRegistrationToken?[] _flowTokens;
        private int _unitTokenCount;
        private int _flowTokenCount;
        private Dictionary<string, CachedTeachingStack>? _priorTeachingCache;
        private Dictionary<string, GeneratedCapabilityCheckpointManifest>?
            _priorManifestCache;
        private PreparedDomainClassifierPublication? _classifierState;
        private readonly ClassifierStateHandleSet? _classifierHandles;
        private bool _priorManifestCacheHydrated;
        private bool _cacheMutated;
        private bool _classifierStatePublicationAttempted;
        private bool _committed;
        private bool _disposed;

        internal CapabilityPublicationTransaction(
            DefaultLogicalCapabilityTrainer owner,
            InMemoryLogicalUnitRegistry units,
            InMemoryLogicalFlowRegistry flows,
            PreparedDomainClassifierPublication? classifierState,
            InvocationPublicationCleanup cleanup,
            int unitTokenCapacity,
            int flowTokenCapacity)
        {
            _owner = owner;
            _units = units;
            _flows = flows;
            _cleanup = cleanup;
            _classifierHandles = classifierState is null
                ? null
                : new ClassifierStateHandleSet(classifierState, cleanup);
            _unitTokens = new LogicalUnitRegistrationToken?[unitTokenCapacity];
            _flowTokens = new LogicalFlowRegistrationToken?[flowTokenCapacity];
        }

        internal bool Track(LogicalUnitRegistrationToken token)
        {
            if ((uint)_unitTokenCount >= (uint)_unitTokens.Length)
            {
                _units.Rollback(token);
                return false;
            }

            _unitTokens[_unitTokenCount++] = token;
            return true;
        }

        internal bool Track(LogicalFlowRegistrationToken token)
        {
            if ((uint)_flowTokenCount >= (uint)_flowTokens.Length)
            {
                _flows.Rollback(token);
                return false;
            }

            _flowTokens[_flowTokenCount++] = token;
            return true;
        }

        internal OperationResult<bool> PublishCache(
            IReadOnlyList<GeneratedCapabilityCheckpointManifest> inventory,
            IReadOnlyList<PreparedCapabilityPublication> prepared,
            LlmModel policyModel,
            BpeTokenizer tokenizer)
        {
            try
            {
                lock (_owner._manifestCacheGate)
                {
                    _priorTeachingCache = new Dictionary<string, CachedTeachingStack>(
                        _owner._teachingCache,
                        StringComparer.Ordinal);
                    _priorManifestCache = _owner._manifestCacheByUnitId
                        .ToDictionary(
                            static pair => pair.Key,
                            static pair => CloneManifest(pair.Value),
                            StringComparer.Ordinal);
                    _priorManifestCacheHydrated =
                        _owner._manifestCacheHydrated;
                    _cacheMutated = true;

                    _owner._manifestCacheByUnitId.Clear();
                    foreach (var manifest in inventory)
                    {
                        _owner._manifestCacheByUnitId.Add(
                            manifest.UnitId,
                            CloneManifest(manifest));
                    }

                    foreach (var item in prepared)
                    {
                        if (!_owner._manifestCacheByUnitId.TryAdd(
                                item.Manifest.UnitId,
                                CloneManifest(item.Manifest)))
                        {
                            return OperationResult<bool>.Fail(
                                "Generated capability manifest UnitId is duplicated.",
                                "GENERATED_CAPABILITY_MANIFEST_DUPLICATE_UNIT_ID");
                        }

                        _owner._teachingCache[item.Manifest.UnitFamilyId] =
                            new CachedTeachingStack(
                                policyModel,
                                tokenizer,
                                CloneManifest(item.Manifest));
                    }

                    _owner._manifestCacheHydrated = true;
                    return OperationResult<bool>.Ok(true);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(
                    "Generated capability cache publication failed.",
                "GENERATED_CAPABILITY_CACHE_PUBLICATION_FAILED");
            }
        }

        internal OperationResult<bool> PublishClassifierState(
            PreparedDomainClassifierPublication prepared)
        {
            _classifierState = prepared;
            _classifierStatePublicationAttempted = true;
            try
            {
                if (!_cleanup.GenericOwnedPathStillMatches(
                        prepared.StagedStatePath,
                        prepared.StateSha256))
                {
                    return ClassifierStatePublicationInvalid();
                }

                if (prepared.PriorStateExists)
                {
                    _classifierHandles!.AcquirePriorTransition();
                    if (!_classifierHandles
                            .RenamePriorToBackupIdentityBound())
                    {
                        return ClassifierStatePublicationInvalid();
                    }
                    _classifierHandles.AcquireBackupAfterMove();
                    RegistryPublicationObserver.Value?.Invoke(
                        "after_classifier_backup_handle_acquisition");
                    if (!_classifierHandles.StillMatchesBackupPath())
                        return ClassifierStatePublicationInvalid();
                }
                else if (File.Exists(prepared.StatePath) ||
                         Directory.Exists(prepared.StatePath))
                {
                    return ClassifierStatePublicationInvalid();
                }

                if (!_cleanup.RetargetOwnedFile(
                        prepared.StagedStatePath,
                        prepared.StatePath,
                        prepared.StateSha256))
                {
                    return ClassifierStatePublicationInvalid();
                }
                File.Move(
                    prepared.StagedStatePath,
                    prepared.StatePath,
                    overwrite: false);
                if (!_cleanup.PromoteOwnedFileToFinalProtection(
                        prepared.StatePath,
                        prepared.StateSha256))
                {
                    return ClassifierStatePublicationInvalid();
                }
                _classifierHandles!.AcquireActive();
                RegistryPublicationObserver.Value?.Invoke(
                    "after_classifier_active_handle_acquisition");
                if (!_classifierHandles.StillMatchesActivePath())
                    return ClassifierStatePublicationInvalid();

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ClassifierStatePublicationInvalid();
            }
        }

        private static OperationResult<bool> ClassifierStatePublicationInvalid()
            => OperationResult<bool>.Fail(
                "Domain classifier training state publication failed.",
                "DOMAIN_CLASSIFIER_STATE_PUBLICATION_FAILED");

        internal OperationResult<bool> VerifyClassifierStateReadyToCommit()
        {
            if (_classifierState is null)
                return OperationResult<bool>.Ok(true);

            try
            {
                var prepared = _classifierState;
                if (!_classifierHandles!.StillMatchesActivePath())
                {
                    return ClassifierStatePublicationInvalid();
                }

                if (!prepared.PriorStateExists)
                    return OperationResult<bool>.Ok(true);

                if (!_classifierHandles.StillMatchesBackupPath())
                {
                    return ClassifierStatePublicationInvalid();
                }

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ClassifierStatePublicationInvalid();
            }
        }

        internal OperationResult<bool> FinalizeClassifierBackupForCommit()
        {
            var success = OperationResult<bool>.Ok(true);
            var prepared = _classifierState;
            if (prepared is null || !prepared.PriorStateExists)
                return success;

            try
            {
                if (!_classifierHandles!.StillMatchesActivePath() ||
                    !_classifierHandles.StillMatchesBackupPath())
                    return ClassifierStatePublicationInvalid();
                var injectedFailure =
                    ClassifierBackupDeletionFailureFactory.Value?.Invoke();
                if (injectedFailure is not null)
                    throw injectedFailure;
                if (!_classifierHandles!.StillMatchesActivePath() ||
                    !_classifierHandles.StillMatchesBackupPath())
                {
                    return ClassifierStatePublicationInvalid();
                }
                PostClassifierBackupDeletionObserver.Value?.Invoke();
                if (!_classifierHandles.StillMatchesActivePath() ||
                    !_classifierHandles.StillMatchesBackupPath())
                {
                    return ClassifierStatePublicationInvalid();
                }
                if (!_classifierHandles.DeleteBackupIdentityBound())
                {
                    return ClassifierStatePublicationInvalid();
                }
                return success;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ClassifierStatePublicationInvalid();
            }
        }

        internal void Commit()
            => _committed = true;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Exception? firstFatal = null;
            var failures = new List<Exception>();
            if (_committed)
            {
                if (_classifierHandles is not null)
                {
                    AttemptCleanup(
                        _classifierHandles.Dispose,
                        ref firstFatal,
                        failures);
                }
                ThrowFirstFatalWithFailures(firstFatal, failures);
                return;
            }

            if (_classifierStatePublicationAttempted &&
                _classifierState is not null)
            {
                AttemptCleanup(() =>
                {
                    if (!_cleanup.DeleteOwnedFileForRollback(
                            _classifierState.StatePath))
                    {
                        throw new IOException(
                            "DOMAIN_CLASSIFIER_ACTIVE_ROLLBACK_IDENTITY_CHANGED");
                    }
                }, ref firstFatal, failures);
                AttemptCleanup(() =>
                {
                    if (!_classifierHandles!.RestoreBackupIdentityBound())
                    {
                        throw new IOException(
                            "DOMAIN_CLASSIFIER_BACKUP_ROLLBACK_IDENTITY_CHANGED");
                    }
                }, ref firstFatal, failures);
            }
            if (_classifierHandles is not null)
            {
                AttemptCleanup(
                    _classifierHandles.Dispose,
                    ref firstFatal,
                    failures);
            }

            if (_cacheMutated)
            {
                AttemptCleanup(() =>
                {
                    lock (_owner._manifestCacheGate)
                    {
                        _owner._teachingCache.Clear();
                        foreach (var pair in _priorTeachingCache!)
                            _owner._teachingCache.Add(pair.Key, pair.Value);
                        _owner._manifestCacheByUnitId.Clear();
                        foreach (var pair in _priorManifestCache!)
                        {
                            _owner._manifestCacheByUnitId.Add(
                                pair.Key,
                                CloneManifest(pair.Value));
                        }
                        _owner._manifestCacheHydrated =
                            _priorManifestCacheHydrated;
                    }
                }, ref firstFatal, failures);
            }

            for (var index = _flowTokenCount - 1; index >= 0; index--)
            {
                var token = _flowTokens[index]!;
                AttemptCleanup(
                    () => _flows.Rollback(token),
                    ref firstFatal,
                    failures);
            }
            for (var index = _unitTokenCount - 1; index >= 0; index--)
            {
                var token = _unitTokens[index]!;
                AttemptCleanup(
                    () => _units.Rollback(token),
                    ref firstFatal,
                    failures);
            }
            ThrowFirstFatalWithFailures(firstFatal, failures);
        }

    }

    private sealed class RootPublicationLease(Mutex mutex) : IDisposable
    {
        private Mutex? _mutex = mutex;

        public void Dispose()
        {
            var ownedMutex = Interlocked.Exchange(ref _mutex, null);
            if (ownedMutex is not null)
            {
                Exception? firstFatal = null;
                var failures = new List<Exception>();
                AttemptCleanup(
                    ownedMutex.ReleaseMutex,
                    ref firstFatal,
                    failures);
                AttemptCleanup(
                    ownedMutex.Dispose,
                    ref firstFatal,
                    failures);
                ThrowFirstFatalWithFailures(firstFatal, failures);
            }
        }
    }

    private sealed class InvocationPublicationCleanup : IDisposable
    {
        private readonly List<StagingOwnership> _staging = [];
        private readonly Dictionary<PublicationPlan, ArtifactHashes> _artifactHashes = [];
        private readonly List<OwnedFile> _ownedFiles = [];
        private readonly List<ClassifierPriorStateCustody>
            _classifierPriorStateCustodies = [];
        private readonly List<string> _ownedDirectories = [];
        private bool _committed;
        private bool _disposed;

        internal RootBoundRollbackScope EnterRootLeaseScope()
            => new(this);

        internal void TrackInvocationStagingDirectory(
            string directory,
            string markerPath,
            string token,
            bool transientOnSuccess)
            => _staging.Add(new StagingOwnership(
                Path.GetFullPath(directory),
                Path.GetFullPath(markerPath),
                token,
                transientOnSuccess));

        internal void TrackOwnedDirectory(string path)
            => _ownedDirectories.Add(Path.GetFullPath(path));

        internal ClassifierPriorStateCustody CaptureClassifierPriorState(
            string path)
        {
            var canonicalPath = Path.GetFullPath(path);
            FileStream? retained = null;
            try
            {
                retained = OpenDpoIdentityStream(
                    canonicalPath,
                    DpoGenericReadAccess | DpoDeleteAccess,
                    FileShare.Read | FileShare.Delete);
                if (retained is null ||
                    !File.Exists(canonicalPath) ||
                    Directory.Exists(canonicalPath) ||
                    (File.GetAttributes(canonicalPath) &
                     FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException(
                        "DOMAIN_CLASSIFIER_PRIOR_IDENTITY_CAPTURE_FAILED");
                }

                var proof = CaptureIdentityProof(retained, canonicalPath);
                if (proof.LinkCount != 1 ||
                    proof.Length < 0 ||
                    proof.Length > MaximumDomainClassifierStateBytes ||
                    !DpoPathMatchesProof(
                        canonicalPath,
                        proof,
                        FileShare.Read | FileShare.Delete))
                {
                    throw new IOException(
                        "DOMAIN_CLASSIFIER_PRIOR_IDENTITY_INVALID");
                }

                retained.Position = 0;
                var bytes = new byte[checked((int)proof.Length)];
                retained.ReadExactly(bytes);
                retained.Position = 0;
                if (!StringComparer.Ordinal.Equals(
                        ComputeBytesSha256(bytes),
                        proof.Sha256) ||
                    !DpoProofMatchesStream(retained, proof))
                {
                    throw new IOException(
                        "DOMAIN_CLASSIFIER_PRIOR_IDENTITY_INVALID");
                }

                var custody = new ClassifierPriorStateCustody(
                    canonicalPath,
                    bytes,
                    proof,
                    retained);
                _classifierPriorStateCustodies.Add(custody);
                retained = null;
                return custody;
            }
            finally
            {
                retained?.Dispose();
            }
        }

        internal string CreateAndTrackOwnedFile(
            string path,
            Action<Stream> write)
        {
            var canonicalPath = Path.GetFullPath(path);
            var ownedFile = new OwnedFile(canonicalPath, string.Empty);
            _ownedFiles.Add(ownedFile);
            var creator = OpenDpoIdentityStream(
                canonicalPath,
                DpoGenericReadAccess |
                DpoGenericWriteAccess |
                DpoDeleteAccess,
                FileShare.Read | FileShare.Delete,
                DpoCreateNew);
            if (creator is null)
                throw new IOException("GENERIC_ARTIFACT_IDENTITY_CREATE_FAILED");
            ownedFile.AttachRetainedIdentity(creator, canDelete: true);
            write(creator);
            creator.Flush(flushToDisk: true);
            var proof = CaptureIdentityProof(creator, canonicalPath);
            if (proof.LinkCount != 1 || proof.Length <= 0)
                throw new IOException("GENERIC_ARTIFACT_IDENTITY_INVALID");
            ownedFile.SealGenericIdentity(proof);

            var intermediate = OpenDpoIdentityStream(
                canonicalPath,
                DpoGenericReadAccess,
                FileShare.Read | FileShare.Write | FileShare.Delete);
            if (intermediate is null ||
                !DpoProofMatchesStream(intermediate, proof))
            {
                intermediate?.Dispose();
                throw new IOException("GENERIC_ARTIFACT_IDENTITY_TRANSITION_FAILED");
            }
            var prior = ownedFile.ReplaceRetainedIdentity(
                intermediate,
                canDelete: false);
            DisposeDpoIdentityHandleNonMasking(prior);

            var transition = OpenDpoIdentityStream(
                canonicalPath,
                DpoGenericReadAccess,
                FileShare.Read | FileShare.Delete);
            if (transition is null ||
                !DpoProofMatchesStream(transition, proof))
            {
                transition?.Dispose();
                throw new IOException("GENERIC_ARTIFACT_IDENTITY_TRANSITION_FAILED");
            }
            prior = ownedFile.ReplaceRetainedIdentity(
                transition,
                canDelete: false);
            DisposeDpoIdentityHandleNonMasking(prior);
            return proof.Sha256;
        }

        internal bool RetargetOwnedFile(
            string sourcePath,
            string destinationPath,
            string expectedSha256)
        {
            var source = Path.GetFullPath(sourcePath);
            var destination = Path.GetFullPath(destinationPath);
            var owned = _ownedFiles.LastOrDefault(file =>
                file.GenericProof is not null &&
                PathComparer.Equals(file.Path, source));
            if (owned is null ||
                !StringComparer.Ordinal.Equals(
                    owned.Sha256,
                    expectedSha256) ||
                !GenericOwnedFileStillMatches(owned))
            {
                return false;
            }
            owned.Retarget(destination);
            return true;
        }

        internal bool GenericOwnedPathStillMatches(
            string path,
            string expectedSha256)
        {
            var canonicalPath = Path.GetFullPath(path);
            var owned = _ownedFiles.LastOrDefault(file =>
                file.GenericProof is not null &&
                PathComparer.Equals(file.Path, canonicalPath));
            return owned is not null &&
                   StringComparer.Ordinal.Equals(
                       owned.Sha256,
                       expectedSha256) &&
                   GenericOwnedFileStillMatches(owned);
        }

        internal bool PromoteOwnedFileToFinalProtection(
            string path,
            string expectedSha256)
        {
            var canonicalPath = Path.GetFullPath(path);
            var owned = _ownedFiles.LastOrDefault(file =>
                file.GenericProof is not null &&
                PathComparer.Equals(file.Path, canonicalPath));
            if (owned is null ||
                !StringComparer.Ordinal.Equals(
                    owned.Sha256,
                    expectedSha256) ||
                owned.GenericProof is null ||
                (owned.RetainedIdentity is not null
                    ? !GenericOwnedFileStillMatches(owned)
                    : !DpoPathMatchesProof(
                        owned.Path,
                        owned.GenericProof,
                        FileShare.Read | FileShare.Delete)))
            {
                return false;
            }

            var protective = OpenDpoIdentityStream(
                canonicalPath,
                DpoGenericReadAccess | DpoDeleteAccess,
                FileShare.Read);
            if (protective is null ||
                !DpoProofMatchesStream(protective, owned.GenericProof!))
            {
                protective?.Dispose();
                return false;
            }
            var prior = owned.ReplaceRetainedIdentity(
                protective,
                canDelete: true);
            DisposeDpoIdentityHandleNonMasking(prior);
            return GenericOwnedFileStillMatches(owned);
        }

        internal bool DeleteOwnedFileForRollback(string path)
        {
            var canonicalPath = Path.GetFullPath(path);
            var owned = _ownedFiles.LastOrDefault(file =>
                file.GenericProof is not null &&
                PathComparer.Equals(file.Path, canonicalPath));
            return owned is null || TryDeleteIdentityBoundFile(owned);
        }

        internal bool PromotePublishedDirectoryOwnership(
            PreparedCapabilityPublication prepared)
        {
            var publication = prepared.Publication;
            var markerPath = Path.Combine(
                publication.CheckpointDirectory,
                Path.GetFileName(publication.StagingMarkerPath));
            return TryGetOwnedFileSha256(markerPath, out var markerSha256) &&
                   PromoteOwnedFileToFinalProtection(
                       markerPath,
                       markerSha256) &&
                   PromoteOwnedFileToFinalProtection(
                       publication.TokenizerPath,
                       prepared.Manifest.TokenizerSha256!) &&
                   PromoteOwnedFileToFinalProtection(
                       publication.PolicyCheckpointPath,
                       prepared.Manifest.PolicyCheckpointSha256!) &&
                   PromoteOwnedFileToFinalProtection(
                       publication.ManifestPath,
                       prepared.ManifestArtifactSha256) &&
                   prepared.DpoMetrics.All(metric =>
                       PromoteOwnedFileToFinalProtection(
                           metric.FinalPath,
                           metric.Sha256));
        }

        internal bool ReleasePublishedDirectoryHandlesForAtomicMove(
            PreparedCapabilityPublication prepared)
        {
            var publication = prepared.Publication;
            var markerPath = Path.Combine(
                publication.CheckpointDirectory,
                Path.GetFileName(publication.StagingMarkerPath));
            var expected = new (string Path, string Sha256)[]
            {
                (markerPath, GetTrackedSha256(markerPath)),
                (publication.TokenizerPath, prepared.Manifest.TokenizerSha256!),
                (publication.PolicyCheckpointPath, prepared.Manifest.PolicyCheckpointSha256!),
                (publication.ManifestPath, prepared.ManifestArtifactSha256)
            }.Concat(prepared.DpoMetrics.Select(metric =>
                (Path: metric.FinalPath, Sha256: metric.Sha256)))
                .ToArray();

            foreach (var item in expected)
            {
                var owned = _ownedFiles.LastOrDefault(file =>
                    file.GenericProof is not null &&
                    PathComparer.Equals(file.Path, Path.GetFullPath(item.Path)));
                if (owned is null ||
                    owned.RetainedIdentity is null ||
                    owned.GenericProof is null ||
                    !StringComparer.Ordinal.Equals(owned.Sha256, item.Sha256) ||
                    !DpoProofMatchesStream(
                        owned.RetainedIdentity,
                        owned.GenericProof) ||
                    owned.FallbackPath is null ||
                    !DpoPathMatchesProof(
                        owned.FallbackPath,
                        owned.GenericProof,
                        FileShare.Read | FileShare.Delete))
                {
                    return false;
                }
            }

            foreach (var item in expected)
            {
                var owned = _ownedFiles.Last(file =>
                    file.GenericProof is not null &&
                    PathComparer.Equals(file.Path, Path.GetFullPath(item.Path)));
                DisposeDpoIdentityHandleNonMasking(owned.TakeRetainedIdentity());
            }

            return true;
        }

        private string GetTrackedSha256(string path)
        {
            if (!TryGetOwnedFileSha256(path, out var sha256))
                return string.Empty;
            return sha256;
        }

        internal bool PublishedDirectoryOwnershipStillMatches(
            PreparedCapabilityPublication prepared)
        {
            var publication = prepared.Publication;
            var markerPath = Path.Combine(
                publication.CheckpointDirectory,
                Path.GetFileName(publication.StagingMarkerPath));
            return TryGetOwnedFileSha256(markerPath, out var markerSha256) &&
                   GenericOwnedPathStillMatches(markerPath, markerSha256) &&
                   GenericOwnedPathStillMatches(
                       publication.TokenizerPath,
                       prepared.Manifest.TokenizerSha256!) &&
                   GenericOwnedPathStillMatches(
                       publication.PolicyCheckpointPath,
                       prepared.Manifest.PolicyCheckpointSha256!) &&
                   GenericOwnedPathStillMatches(
                       publication.ManifestPath,
                       prepared.ManifestArtifactSha256) &&
                   prepared.DpoMetrics.All(metric =>
                       GenericOwnedPathStillMatches(
                           metric.FinalPath,
                           metric.Sha256));
        }

        internal void PrepareDpoArtifactOwnership(int maximumStepCount)
            => _ownedFiles.EnsureCapacity(checked(
                _ownedFiles.Count + (maximumStepCount * 5)));

        internal bool TryAcceptDpoArtifactOwnership(
            DpoTrainerArtifactOwnershipProof proof)
        {
            try
            {
                if (proof.Kind is not (
                    DpoTrainerArtifactKind.PolicyFinal or
                    DpoTrainerArtifactKind.MetricsFinal))
                {
                    return false;
                }
                if (proof.Step < LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount ||
                    proof.Step > LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount ||
                    proof.LinkCount != 1 ||
                    proof.Length < 0 ||
                    proof.Sha256.Length != 64 ||
                    !proof.Sha256.All(static character =>
                        character is >= '0' and <= '9' or >= 'a' and <= 'f'))
                {
                    return false;
                }

                var path = Path.GetFullPath(proof.Path);
                var directory = Path.GetDirectoryName(path);
                if (directory is null)
                    return false;
                var ownedDpoDirectory = _staging.FirstOrDefault(staging =>
                    staging.TransientOnSuccess &&
                    PathComparer.Equals(staging.Directory, directory));
                if (ownedDpoDirectory is null ||
                    !MarkerMatches(
                        ownedDpoDirectory.MarkerPath,
                        ownedDpoDirectory.Token) ||
                    !TryGetOwnedFileSha256(
                        ownedDpoDirectory.MarkerPath,
                        out var markerSha256) ||
                    !GenericOwnedPathStillMatches(
                        ownedDpoDirectory.MarkerPath,
                        markerSha256))
                {
                    return false;
                }

                var policyName = DpoTrainer.BuildPolicyCheckpointFileName(
                    proof.Step);
                var metricsName = DpoTrainer.BuildMetricsFileName(proof.Step);
                var fileName = Path.GetFileName(path);
                var validPath = proof.Kind switch
                {
                    DpoTrainerArtifactKind.PolicyFinal =>
                        StringComparer.Ordinal.Equals(fileName, policyName) &&
                        IsValidDpoTransition(
                            proof,
                            DpoTrainerArtifactKind.PolicyStaging,
                            policyName),
                    DpoTrainerArtifactKind.MetricsFinal =>
                        StringComparer.Ordinal.Equals(fileName, metricsName) &&
                        IsValidDpoTransition(
                            proof,
                            DpoTrainerArtifactKind.MetricsStaging,
                            metricsName),
                    _ => false
                };
                if (!validPath || proof.Length == 0)
                {
                    return false;
                }

                FileStream? acceptedIdentity = null;
                try
                {
                    acceptedIdentity = OpenDpoIdentityStream(
                        path,
                        DpoGenericReadAccess | DpoDeleteAccess,
                        FileShare.Read);
                    if (acceptedIdentity is null ||
                        !DpoProofMatchesStream(acceptedIdentity, proof))
                    {
                        return false;
                    }

                    var ownedFile = new OwnedFile(
                        path,
                        proof.Sha256,
                        proof,
                        acceptedIdentity,
                        retainedCanDelete: true);
                    _ownedFiles.Add(ownedFile);
                    acceptedIdentity = null;
                    DpoArtifactOwnershipAcceptedObserver.Value?.Invoke(proof);
                    return true;
                }
                finally
                {
                    acceptedIdentity?.Dispose();
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return false;
            }
        }

        private static bool IsDpoStagingName(
            string fileName,
            string finalName)
            => fileName.StartsWith($".{finalName}.", StringComparison.Ordinal) &&
               fileName.EndsWith(".staging", StringComparison.Ordinal) &&
               fileName.Length > finalName.Length + ".x.staging".Length;

        private static bool IsValidDpoTransition(
            DpoTrainerArtifactOwnershipProof proof,
            DpoTrainerArtifactKind stagingKind,
            string finalName)
            => proof.TransitionFromKind == stagingKind &&
               proof.TransitionFromIdentity == proof.Identity &&
               proof.TransitionFromPath is not null &&
               PathComparer.Equals(
                   Path.GetDirectoryName(Path.GetFullPath(
                       proof.TransitionFromPath)),
                   Path.GetDirectoryName(Path.GetFullPath(proof.Path))) &&
               IsDpoStagingName(
                   Path.GetFileName(proof.TransitionFromPath),
                   finalName);

        internal bool TryGetOwnedFileSha256(
            string path,
            out string sha256)
        {
            var canonicalPath = Path.GetFullPath(path);
            for (var index = _ownedFiles.Count - 1; index >= 0; index--)
            {
                if (!PathComparer.Equals(
                        _ownedFiles[index].Path,
                        canonicalPath))
                    continue;
                sha256 = _ownedFiles[index].Sha256;
                return true;
            }

            sha256 = string.Empty;
            return false;
        }

        internal void TrackStagedArtifactHashes(
            PublicationPlan publication,
            string tokenizerSha256,
            string policySha256,
            string manifestSha256)
        {
            if (!GenericOwnedPathStillMatches(
                    publication.StagingTokenizerPath,
                    tokenizerSha256) ||
                !GenericOwnedPathStillMatches(
                    publication.StagingPolicyCheckpointPath,
                    policySha256) ||
                !GenericOwnedPathStillMatches(
                    publication.StagingManifestPath,
                    manifestSha256))
            {
                throw new IOException(
                    "GENERATED_CAPABILITY_STAGING_IDENTITY_CHANGED");
            }
            _artifactHashes[publication] = new ArtifactHashes(
                tokenizerSha256,
                policySha256,
                manifestSha256);
        }

        internal OperationResult<bool> TrackCompletedDpoArtifacts(
            string directory,
            string token,
            int stepCount,
            IReadOnlyList<string> metricPaths)
        {
            try
            {
                var canonicalDirectory = Path.GetFullPath(directory);
                var markerPath = Path.Combine(
                    canonicalDirectory,
                    ".generated-capability.dpo-owner");
                if (stepCount < 0 ||
                    !MarkerMatches(markerPath, token) ||
                    !TryGetOwnedFileSha256(
                        markerPath,
                        out var markerSha256) ||
                    !GenericOwnedPathStillMatches(
                        markerPath,
                        markerSha256))
                    return DpoArtifactIdentityInvalid();

                var comparer = OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;
                var expectedEntries = new HashSet<string>(comparer)
                {
                    Path.GetFullPath(markerPath)
                };
                var expectedMetrics = new HashSet<string>(comparer);
                var expectedReservationLocks = new HashSet<string>(comparer);
                for (var step = 1; step <= stepCount; step++)
                {
                    expectedEntries.Add(Path.GetFullPath(Path.Combine(
                        canonicalDirectory,
                        DpoTrainer.BuildPolicyCheckpointFileName(step))));
                    var metricPath = Path.GetFullPath(Path.Combine(
                        canonicalDirectory,
                        DpoTrainer.BuildMetricsFileName(step)));
                    expectedEntries.Add(metricPath);
                    expectedMetrics.Add(metricPath);
                    if (OperatingSystem.IsLinux())
                    {
                        // На Linux DPO оставляет reservation-lock как обычный
                        // sidecar. Он допустим только с точным именем текущего
                        // шага; все остальные lock/lookalike остаются чужими.
                        var reservationLockPath = Path.GetFullPath(Path.Combine(
                            canonicalDirectory,
                            BuildDpoReservationLockFileName(step)));
                        expectedEntries.Add(reservationLockPath);
                        expectedReservationLocks.Add(reservationLockPath);
                    }
                }

                var actualEntries = Directory
                    .EnumerateFileSystemEntries(
                        canonicalDirectory,
                        "*",
                        SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFullPath)
                    .ToHashSet(comparer);
                var reportedMetrics = metricPaths
                    .Select(Path.GetFullPath)
                    .ToHashSet(comparer);
                if (!expectedEntries.SetEquals(actualEntries) ||
                    !expectedMetrics.SetEquals(reportedMetrics))
                {
                    return DpoArtifactIdentityInvalid();
                }

                foreach (var path in expectedEntries)
                {
                    if (comparer.Equals(path, markerPath))
                        continue;
                    if (expectedReservationLocks.Contains(path))
                    {
                        if (!TrackDpoReservationLock(path))
                            return DpoArtifactIdentityInvalid();
                        continue;
                    }

                    if (!File.Exists(path) ||
                        (File.GetAttributes(path) &
                         FileAttributes.ReparsePoint) != 0)
                    {
                        return DpoArtifactIdentityInvalid();
                    }
                    var accepted = _ownedFiles.LastOrDefault(file =>
                        file.DpoProof is not null &&
                        PathComparer.Equals(file.Path, path));
                    if (accepted is null ||
                        !StringComparer.Ordinal.Equals(
                            accepted.Sha256,
                            ComputeFileSha256(
                                path,
                                FileShare.Read | FileShare.Delete)) ||
                        !DpoOwnedFileStillMatches(accepted))
                    {
                        return DpoArtifactIdentityInvalid();
                    }
                }

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return DpoArtifactIdentityInvalid();
            }
        }

        internal void TrackDpoReservationLocksForRollback(
            string directory,
            int maximumStepCount)
        {
            if (!OperatingSystem.IsLinux() ||
                maximumStepCount <= 0)
            {
                return;
            }

            var canonicalDirectory = Path.GetFullPath(directory);
            for (var step = 1; step <= maximumStepCount; step++)
            {
                var reservationLockPath = Path.GetFullPath(Path.Combine(
                    canonicalDirectory,
                    BuildDpoReservationLockFileName(step)));
                if (!File.Exists(reservationLockPath) ||
                    Directory.Exists(reservationLockPath))
                {
                    continue;
                }

                // Это не wildcard-cleanup: rollback берёт только lock с точным
                // именем шага и только если helper доказал retained identity,
                // link-count и свободный flock. Подмена просто останется чужой.
                _ = TrackDpoReservationLock(reservationLockPath);
            }
        }

        private bool TrackDpoReservationLock(string path)
        {
            var canonicalPath = Path.GetFullPath(path);
            var existing = _ownedFiles.LastOrDefault(file =>
                file.GenericProof is not null &&
                PathComparer.Equals(file.Path, canonicalPath));
            if (existing is not null)
                return GenericOwnedFileStillMatches(existing);

            FileStream? retained = null;
            try
            {
                if (!File.Exists(canonicalPath) ||
                    Directory.Exists(canonicalPath) ||
                    (File.GetAttributes(canonicalPath) &
                     FileAttributes.ReparsePoint) != 0)
                {
                    return false;
                }

                retained = OpenDpoIdentityStream(
                    canonicalPath,
                    DpoGenericReadAccess | DpoDeleteAccess,
                    FileShare.Read | FileShare.Delete);
                if (retained is null ||
                    !LogicalCapabilityFileCustody.TryAcquireIdentityLock(
                        retained,
                        canonicalPath))
                {
                    return false;
                }

                var step = ParseDpoReservationLockStep(canonicalPath);
                if (step <= 0)
                    return false;

                // Этот sidecar уже не удерживается DpoTrainer после публикации
                // шага, поэтому trainer забирает собственный retained-handle и
                // удаляет только ту же inode/FileId во время transient cleanup.
                var proof = LogicalCapabilityFileCustody.CaptureIdentityProof(
                    retained,
                    canonicalPath,
                    step,
                    DpoTrainerArtifactKind.ReservationLock);
                if (proof.LinkCount != 1 ||
                    proof.Length < 0 ||
                    proof.Length > DpoReservationLockMaximumBytes ||
                    !DpoPathMatchesProof(
                        canonicalPath,
                        proof,
                        FileShare.Read | FileShare.Delete) ||
                    !DpoProofMatchesStream(retained, proof))
                {
                    return false;
                }

                var owned = new OwnedFile(canonicalPath, proof.Sha256);
                owned.AttachRetainedIdentity(retained, canDelete: true);
                owned.SealGenericIdentity(proof);
                _ownedFiles.Add(owned);
                retained = null;
                return true;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return false;
            }
            finally
            {
                retained?.Dispose();
            }
        }

        private static string BuildDpoReservationLockFileName(int step)
            => $".{DpoTrainer.MetricsFilePrefix}{step:D6}.lock";

        private static int ParseDpoReservationLockStep(string path)
        {
            var fileName = Path.GetFileName(path);
            var prefix = "." + DpoTrainer.MetricsFilePrefix;
            const string suffix = ".lock";
            if (!fileName.StartsWith(prefix, StringComparison.Ordinal) ||
                !fileName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return -1;
            }

            var stepText = fileName.Substring(
                prefix.Length,
                fileName.Length - prefix.Length - suffix.Length);
            return int.TryParse(
                stepText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var step)
                    ? step
                    : -1;
        }

        private static OperationResult<bool> DpoArtifactIdentityInvalid()
            => OperationResult<bool>.Fail(
                "DPO staging artifact identity is invalid.",
                "DPO_STAGING_ARTIFACT_IDENTITY_INVALID");

        internal OperationResult<bool> PreparePublishedDirectoryOwnership(
            IReadOnlyList<PreparedCapabilityPublication> prepared)
        {
            try
            {
                var additionalDirectories = prepared.Count;
                foreach (var item in prepared)
                {
                    if (item.DpoMetrics.Length > 0)
                        additionalDirectories++;
                }
                _ownedDirectories.EnsureCapacity(checked(
                    _ownedDirectories.Count + additionalDirectories));
                _staging.EnsureCapacity(checked(
                    _staging.Count + prepared.Count));

                foreach (var item in prepared)
                {
                    var publication = item.Publication;
                    if (!_artifactHashes.TryGetValue(publication, out var hashes))
                    {
                        return OperationResult<bool>.Fail(
                            "Published capability artifacts were not tracked.",
                            "GENERATED_CAPABILITY_PUBLICATION_ENLISTMENT_FAILED");
                    }

                    TrackOwnedDirectory(publication.CheckpointDirectory);
                    if (!RetargetOwnedFile(
                            publication.StagingTokenizerPath,
                            publication.TokenizerPath,
                            hashes.TokenizerSha256) ||
                        !RetargetOwnedFile(
                            publication.StagingPolicyCheckpointPath,
                            publication.PolicyCheckpointPath,
                            hashes.PolicySha256) ||
                        !RetargetOwnedFile(
                            publication.StagingManifestPath,
                            publication.ManifestPath,
                            hashes.ManifestSha256))
                    {
                        return OperationResult<bool>.Fail(
                            "Published capability artifact identity changed.",
                            "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
                    }
                    if (item.DpoMetrics.Length > 0)
                        TrackOwnedDirectory(publication.DpoMetricsDirectory);
                    foreach (var metric in item.DpoMetrics)
                    {
                        if (!RetargetOwnedFile(
                                metric.StagingPath,
                                metric.FinalPath,
                                metric.Sha256))
                        {
                            return OperationResult<bool>.Fail(
                                "Published capability artifact identity changed.",
                                "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
                        }
                    }

                    var markerFinalPath = Path.Combine(
                        publication.CheckpointDirectory,
                        Path.GetFileName(publication.StagingMarkerPath));
                    if (!TryGetOwnedFileSha256(
                            publication.StagingMarkerPath,
                            out var markerSha256) ||
                        !RetargetOwnedFile(
                            publication.StagingMarkerPath,
                            markerFinalPath,
                            markerSha256))
                    {
                        return OperationResult<bool>.Fail(
                            "Published capability marker identity changed.",
                            "GENERATED_CAPABILITY_PUBLICATION_IDENTITY_CHANGED");
                    }

                    _staging.Add(new StagingOwnership(
                        Path.GetFullPath(publication.CheckpointDirectory),
                        markerFinalPath,
                        publication.InvocationToken,
                        TransientOnSuccess: false));
                }

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<bool>.Fail(
                    "Generated capability publication ownership could not be enlisted.",
                    "GENERATED_CAPABILITY_PUBLICATION_ENLISTMENT_FAILED");
            }
        }

        internal OperationResult<bool> PrepareTransientCleanupForCommit()
        {
            try
            {
                foreach (var staging in _staging.Where(
                             static item => item.TransientOnSuccess))
                {
                    foreach (var file in _ownedFiles.Where(
                                 file => IsContainedBy(
                                     file.Path,
                                     staging.Directory)))
                    {
                        if (!TryDeleteIdentityBoundFile(file))
                        {
                            return HasForeignFileIdentity(file)
                                ? TransientCleanupForeignContent()
                                : TransientCleanupFailed();
                        }
                    }

                    if (File.Exists(staging.MarkerPath) ||
                        Directory.Exists(staging.MarkerPath))
                    {
                        return TransientCleanupForeignContent();
                    }

                    foreach (var directory in _ownedDirectories
                                 .Where(path => IsContainedBy(
                                     path,
                                     staging.Directory))
                                 .OrderByDescending(static path => path.Length))
                    {
                        if (!TryDeleteEmptyDirectory(directory))
                        {
                            return Directory.Exists(directory) &&
                                   ((File.GetAttributes(directory) &
                                     FileAttributes.ReparsePoint) != 0 ||
                                    Directory.EnumerateFileSystemEntries(
                                        directory).Any())
                                ? TransientCleanupForeignContent()
                                : TransientCleanupFailed();
                        }
                    }

                    if (Directory.Exists(staging.Directory) ||
                        File.Exists(staging.Directory))
                    {
                        return TransientCleanupFailed();
                    }
                }

                return OperationResult<bool>.Ok(true);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return TransientCleanupFailed();
            }
        }

        private static OperationResult<bool> TransientCleanupFailed()
            => OperationResult<bool>.Fail(
                "Generated capability transient artifacts could not be cleaned.",
                "GENERATED_CAPABILITY_TRANSIENT_CLEANUP_FAILED");

        private static OperationResult<bool>
            TransientCleanupForeignContent()
            => OperationResult<bool>.Fail(
                "Generated capability transient staging contains foreign content.",
                "GENERATED_CAPABILITY_TRANSIENT_FOREIGN_CONTENT");

        private static bool HasForeignFileIdentity(OwnedFile file)
        {
            try
            {
                if (Directory.Exists(file.Path))
                    return true;
                if (!File.Exists(file.Path))
                    return false;
                if ((File.GetAttributes(file.Path) &
                     FileAttributes.ReparsePoint) != 0)
                {
                    return true;
                }

                if (file.DpoProof is not null)
                    return !DpoOwnedFileStillMatches(file);
                if (file.GenericProof is not null)
                    return !GenericOwnedFileStillMatches(file);

                return true;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return true;
            }
        }

        internal void Commit()
        {
            _committed = true;
            Exception? firstFatal = null;
            var failures = new List<Exception>();
            DisposeOwnedFileHandles(ref firstFatal, failures);
            DisposeClassifierPriorStateCustodies(ref firstFatal, failures);
            ThrowFirstFatalWithFailures(firstFatal, failures);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Exception? firstFatal = null;
            var failures = new List<Exception>();
            if (_committed)
            {
                DisposeOwnedFileHandles(ref firstFatal, failures);
                DisposeClassifierPriorStateCustodies(
                    ref firstFatal,
                    failures);
                ThrowFirstFatalWithFailures(firstFatal, failures);
                return;
            }

            for (var index = _ownedFiles.Count - 1; index >= 0; index--)
            {
                var file = _ownedFiles[index];
                AttemptCleanup(
                    () => TryDeleteIdentityBoundFile(file),
                    ref firstFatal,
                    failures);
            }

            foreach (var directory in _ownedDirectories
                         .Distinct(PathComparer)
                         .OrderByDescending(static path => path.Length))
            {
                AttemptCleanup(
                    () =>
                    {
                    TryDeleteEmptyDirectory(directory);
                    },
                    ref firstFatal,
                    failures);
            }

            AttemptCleanup(() =>
            {
                var injectedFailure = CleanupFailureFactory.Value?.Invoke();
                if (injectedFailure is not null)
                    throw injectedFailure;
            }, ref firstFatal, failures);

            DisposeOwnedFileHandles(ref firstFatal, failures);
            DisposeClassifierPriorStateCustodies(ref firstFatal, failures);
            ThrowFirstFatalWithFailures(firstFatal, failures);
        }

        private void DisposeOwnedFileHandles(
            ref Exception? firstFatal,
            List<Exception> failures)
        {
            for (var index = _ownedFiles.Count - 1; index >= 0; index--)
            {
                var file = _ownedFiles[index];
                AttemptCleanup(file.Dispose, ref firstFatal, failures);
            }
        }

        private void DisposeClassifierPriorStateCustodies(
            ref Exception? firstFatal,
            List<Exception> failures)
        {
            for (var index = _classifierPriorStateCustodies.Count - 1;
                 index >= 0;
                 index--)
            {
                var custody = _classifierPriorStateCustodies[index];
                AttemptCleanup(custody.Dispose, ref firstFatal, failures);
            }
        }

        private static StringComparer PathComparer =>
            OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

        private static bool IsContainedBy(string path, string directory)
        {
            var canonicalPath = Path.GetFullPath(path);
            var canonicalDirectory = Path.GetFullPath(directory);
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            return string.Equals(
                       canonicalPath,
                       canonicalDirectory,
                       comparison) ||
                   canonicalPath.StartsWith(
                       Path.EndsInDirectorySeparator(canonicalDirectory)
                           ? canonicalDirectory
                           : canonicalDirectory + Path.DirectorySeparatorChar,
                       comparison);
        }

        private static bool MarkerMatches(string path, string token)
        {
            try
            {
                if (!File.Exists(path) ||
                    (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    return false;
                return StringComparer.Ordinal.Equals(
                    File.ReadAllText(path, Encoding.ASCII),
                    token);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return false;
            }
        }

        private static bool TryDeleteIdentityBoundFile(OwnedFile file)
        {
            try
            {
                var identityProof = file.DpoProof ?? file.GenericProof;
                if (identityProof is not null ||
                    file.RetainedIdentity is not null)
                {
                    var retained = file.RetainedIdentity;
                    if (retained is not null)
                    {
                        if (identityProof is not null &&
                            !DpoProofMatchesStream(
                                retained,
                                identityProof))
                        {
                            return false;
                        }
                        var matchedPath = file.Path;
                        var pathMatches = identityProof is not null &&
                            DpoPathMatchesProof(
                                matchedPath,
                                identityProof,
                                FileShare.Read | FileShare.Delete);
                        if (!pathMatches &&
                            identityProof is not null &&
                            file.FallbackPath is not null &&
                            DpoPathMatchesProof(
                                file.FallbackPath,
                                identityProof,
                                FileShare.Read | FileShare.Delete))
                        {
                            matchedPath = file.FallbackPath;
                            pathMatches = true;
                        }
                        if (!pathMatches)
                        {
                            // Если имя уже смотрит не на удержанный proof, не
                            // зовём delete даже через retained handle: дальше
                            // HasForeignFileIdentity честно классифицирует
                            // замену как чужой transient content.
                            DisposeDpoIdentityHandleNonMasking(
                                file.TakeRetainedIdentity());
                            return false;
                        }

                        if (file.RetainedCanDelete)
                        {
                            MarkDpoIdentityForDeletion(retained, matchedPath);
                            DisposeDpoIdentityHandleNonMasking(
                                file.TakeRetainedIdentity());
                        }
                        else
                        {
                            FileStream? deletion = null;
                            if (pathMatches)
                            {
                                deletion = OpenDpoIdentityStream(
                                    matchedPath,
                                    DpoGenericReadAccess | DpoDeleteAccess,
                                    FileShare.Read);
                            }
                            if (deletion is null ||
                                identityProof is null ||
                                !DpoProofMatchesStream(deletion, identityProof))
                            {
                                deletion?.Dispose();
                                DisposeDpoIdentityHandleNonMasking(
                                    file.TakeRetainedIdentity());
                                return false;
                            }
                            MarkDpoIdentityForDeletion(deletion, matchedPath);
                            deletion.Dispose();
                            DisposeDpoIdentityHandleNonMasking(
                                file.TakeRetainedIdentity());
                        }
                        return pathMatches &&
                               !File.Exists(matchedPath) &&
                               !Directory.Exists(matchedPath);
                    }

                    if (!File.Exists(file.Path))
                        return !Directory.Exists(file.Path);
                    if ((File.GetAttributes(file.Path) &
                         FileAttributes.ReparsePoint) != 0)
                    {
                        return false;
                    }

                    using (var stream = OpenDpoIdentityStream(
                               file.Path,
                               DpoGenericReadAccess | DpoDeleteAccess,
                               FileShare.Read))
                    {
                        if (stream is null ||
                            identityProof is null ||
                            !DpoProofMatchesStream(stream, identityProof))
                        {
                            return false;
                        }
                        if (file.DpoProof is not null)
                        {
                            DpoProofOnlyCleanupAfterValidationObserver.Value?
                                .Invoke(file.DpoProof);
                        }
                        MarkDpoIdentityForDeletion(stream, file.Path);
                    }
                    return !File.Exists(file.Path) &&
                           !Directory.Exists(file.Path);
                }

                return !File.Exists(file.Path) &&
                       !Directory.Exists(file.Path);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                CaptureFirstCleanupFailure(exception);
                return false;
            }
        }

        private static bool DpoOwnedFileStillMatches(OwnedFile file)
        {
            if (file.DpoProof is null)
                return false;
            if (file.RetainedIdentity is not null &&
                !DpoProofMatchesStream(
                    file.RetainedIdentity,
                    file.DpoProof))
            {
                return false;
            }

            return DpoPathMatchesProof(
                file.Path,
                file.DpoProof,
                file.RetainedIdentity is null
                    ? FileShare.Read | FileShare.Write
                   : FileShare.Read | FileShare.Delete);
        }

        private static bool GenericOwnedFileStillMatches(OwnedFile file)
        {
            if (file.GenericProof is null ||
                file.RetainedIdentity is null ||
                !DpoProofMatchesStream(
                    file.RetainedIdentity,
                    file.GenericProof))
            {
                return false;
            }
            return DpoPathMatchesProof(
                file.Path,
                file.GenericProof,
                FileShare.Read | FileShare.Delete);
        }

        private static FileStream? OpenDpoIdentityStream(
            string path,
            uint desiredAccess,
            FileShare share,
            uint creationDisposition = DpoOpenExisting)
            => LogicalCapabilityFileCustody.OpenIdentityStream(
                path,
                desiredAccess,
                share,
                creationDisposition);

        internal static DpoTrainerArtifactOwnershipProof CaptureIdentityProof(
            FileStream stream,
            string path)
            => LogicalCapabilityFileCustody.CaptureIdentityProof(
                stream,
                path,
                step: 0,
                DpoTrainerArtifactKind.PolicySerialization);

        internal static bool DpoPathMatchesProof(
            string path,
            DpoTrainerArtifactOwnershipProof proof,
            FileShare share)
            => LogicalCapabilityFileCustody.PathMatchesProof(
                path,
                proof,
                share);

        internal static bool DpoProofMatchesStream(
            FileStream stream,
            DpoTrainerArtifactOwnershipProof proof)
            => LogicalCapabilityFileCustody.ProofMatchesStream(stream, proof);

        private static void MarkDpoIdentityForDeletion(
            FileStream stream,
            string path)
            => LogicalCapabilityFileCustody.MarkIdentityForDeletion(stream, path);

        internal static bool TryDeleteDpoProofOnlyArtifactForTesting(
            DpoTrainerArtifactOwnershipProof proof)
            => TryDeleteIdentityBoundFile(new OwnedFile(
                Path.GetFullPath(proof.Path),
                proof.Sha256,
                proof));

        private static bool TryDeleteEmptyDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return !File.Exists(path);
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 ||
                    Directory.EnumerateFileSystemEntries(path).Any())
                    return false;
                Directory.Delete(path, recursive: false);
                return !Directory.Exists(path) && !File.Exists(path);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                CaptureFirstCleanupFailure(exception);
                return false;
            }
        }

        private const int DpoFileIdInfoClass = 18;
        private const int DpoFileDispositionInfoClass = 4;
        private const uint DpoGenericReadAccess = 0x80000000;
        private const uint DpoGenericWriteAccess = 0x40000000;
        private const uint DpoDeleteAccess = 0x00010000;
        private const uint DpoCreateNew = 1;
        private const uint DpoOpenExisting = 3;
        private const uint DpoFileAttributeNormal = 0x00000080;
        private const long DpoReservationLockMaximumBytes = 4L * 1024;

        [StructLayout(LayoutKind.Sequential)]
        private struct DpoFileDispositionInformation
        {
            // FILE_DISPOSITION_INFO.DeleteFile is Win32 BOOLEAN (one byte),
            // not the four-byte BOOL used by UnmanagedType.Bool.
            internal byte DeleteFile;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DpoFileIdInfo
        {
            internal ulong VolumeSerialNumber;
            internal ulong FileIdLow;
            internal ulong FileIdHigh;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DpoByHandleFileInformation
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

        [DllImport(
            "kernel32.dll",
            EntryPoint = "CreateFileW",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern SafeFileHandle CreateDpoIdentityFileWindows(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetFileInformationByHandle",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDpoFileInformationByHandleWindows(
            SafeFileHandle fileHandle,
            out DpoByHandleFileInformation fileInformation);

        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetFileInformationByHandleEx",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDpoFileIdentityByHandleWindows(
            SafeFileHandle fileHandle,
            int fileInformationClass,
            out DpoFileIdInfo fileInformation,
            uint bufferSize);

        [DllImport(
            "kernel32.dll",
            EntryPoint = "SetFileInformationByHandle",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDpoFileInformationByHandleWindows(
            SafeFileHandle fileHandle,
            int fileInformationClass,
            ref DpoFileDispositionInformation fileInformation,
            uint bufferSize);

        private static void DisposeDpoIdentityHandleNonMasking(
            IDisposable? disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                CaptureFirstCleanupFailure(exception);
            }
        }

        private sealed record StagingOwnership(
            string Directory,
            string MarkerPath,
            string Token,
            bool TransientOnSuccess);

        private sealed class OwnedFile(
            string path,
            string sha256,
            DpoTrainerArtifactOwnershipProof? dpoProof = null,
            FileStream? retainedIdentity = null,
            bool retainedCanDelete = false) : IDisposable
        {
            internal string Path { get; private set; } = path;
            internal string? FallbackPath { get; private set; }
            internal string Sha256 { get; private set; } = sha256;
            internal DpoTrainerArtifactOwnershipProof? DpoProof { get; } =
                dpoProof;
            internal DpoTrainerArtifactOwnershipProof? GenericProof
            {
                get;
                private set;
            }
            internal FileStream? RetainedIdentity { get; private set; } =
                retainedIdentity;
            internal bool RetainedCanDelete { get; private set; } =
                retainedCanDelete;

            internal void AttachRetainedIdentity(
                FileStream retained,
                bool canDelete)
            {
                if (RetainedIdentity is not null)
                    throw new InvalidOperationException(
                        "GENERIC_ARTIFACT_IDENTITY_ALREADY_ATTACHED");
                RetainedIdentity = retained;
                RetainedCanDelete = canDelete;
            }

            internal FileStream? ReplaceRetainedIdentity(
                FileStream retained,
                bool canDelete)
            {
                var prior = RetainedIdentity;
                RetainedIdentity = retained;
                RetainedCanDelete = canDelete;
                return prior;
            }

            internal void SealGenericIdentity(
                DpoTrainerArtifactOwnershipProof proof)
            {
                GenericProof = proof;
                Sha256 = proof.Sha256;
            }

            internal void Retarget(string destinationPath)
            {
                if (GenericProof is null)
                    throw new InvalidOperationException(
                        "GENERIC_ARTIFACT_IDENTITY_NOT_SEALED");
                FallbackPath = Path;
                Path = destinationPath;
                GenericProof = GenericProof with { Path = destinationPath };
            }

            internal FileStream? TakeRetainedIdentity()
            {
                var retained = RetainedIdentity;
                RetainedIdentity = null;
                RetainedCanDelete = false;
                return retained;
            }

            public void Dispose()
                => DisposeDpoIdentityHandleNonMasking(
                    TakeRetainedIdentity());
        }

        private sealed record ArtifactHashes(
            string TokenizerSha256,
            string PolicySha256,
            string ManifestSha256);

        internal readonly struct RootBoundRollbackScope(
            InvocationPublicationCleanup cleanup) : IDisposable
        {
            public void Dispose()
                => cleanup.Dispose();
        }
    }

    private sealed record TrainingBranchPlan(
        LogicalFlowResult Branch,
        string SubPhaseId,
        LogicalSubPhase SubPhase,
        string TrainingText,
        string ChosenText,
        string RejectedText,
        float[] TeachingVector,
        LogicalCapabilityTrainingCurrentProjection CurrentProjection,
        LogicalCapabilityTrainingReplayDescriptor? AuthorizedReplayDescriptor,
        string? AuthorizedReplayExampleId,
        LogicalTrainingContentProvenance TrainingContentProvenance,
        string[] SourceBindingSha256s);

    private sealed class BoundedDomainClassifierStateStream(
        int maximumLength) : Stream
    {
        private readonly MemoryStream _inner = new();

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        internal byte[] ToArray() => _inner.ToArray();

        public override void Flush() => _inner.Flush();

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureWithinBound(count);
            _inner.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureWithinBound(buffer.Length);
            _inner.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            EnsureWithinBound(1);
            _inner.WriteByte(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }

        private void EnsureWithinBound(int additionalBytes)
        {
            if (additionalBytes < 0 ||
                _inner.Length > maximumLength - additionalBytes)
            {
                throw new InvalidDataException(
                    "Domain classifier training state exceeds its byte bound.");
            }
        }
    }

    private sealed record DomainClassifierTrainingExample(
        string RunId,
        SemanticCompiler29.Domain Domain,
        int[] Predicates,
        DateTimeOffset CreatedAtUtc);
}

// This factory creates a generated capability flow that selects probationary retrieval units for retry validation.
public sealed class GeneratedCapabilityFlowFactory : ILogicalFlowFactory<string>
{
    private readonly string _phaseId;
    private readonly string _subPhaseId;
    private readonly string _unitDomainTag;
    private readonly string _taskType;
    private readonly string[] _domainTags;
    private readonly string[] _predicateIds;
    private readonly string _originalPrompt;

    // Журнал состоявшихся выдач, который фабрика передаёт созданному потоку. Он необязателен:
    // без него собранный поток ведёт себя в точности как прежде.
    private readonly IAnsweredDeliveryLedger<string>? _deliveryLedger;

    public GeneratedCapabilityFlowFactory(
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        string[] predicateIds,
        string? unitDomainTag = null,
        string? originalPrompt = null,
        IAnsweredDeliveryLedger<string>? deliveryLedger = null)
    {
        _phaseId = phaseId;
        _subPhaseId = subPhaseId;
        _unitDomainTag = string.IsNullOrWhiteSpace(unitDomainTag) ? subPhaseId : unitDomainTag;
        _taskType = taskType;
        _domainTags = domainTags?.ToArray() ?? [];
        _predicateIds = CapabilityResultTranslator
            .SnapshotSignedPredicateIds(predicateIds);
        _originalPrompt = originalPrompt ?? string.Empty;
        // Журнал выдач принимается как есть и дальше только передаётся созданному потоку.
        _deliveryLedger = deliveryLedger;
    }

    // Create a flow that uses the newest probationary-or-better generated capability unit.
    public OperationResult<ILogicalFlow<string>> Create(LogicalFlowFactoryRequest request)
    {
        if (request is null)
            return OperationResult<ILogicalFlow<string>>.Fail("Flow factory request is required.", "FLOW_FACTORY_REQUEST_REQUIRED");

        if (!request.Flags.UseLearnedUnitWhenAvailable)
            return OperationResult<ILogicalFlow<string>>.Ok(new UniversalNineNodeDevelopmentFlow(_phaseId, _subPhaseId, _taskType, _domainTags, _predicateIds));

        var retrievalUnit = request.Units.ResolveLatest<string, string>(
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            _unitDomainTag,
            LogicalUnitTrustState.Probationary);
        if (!retrievalUnit.IsSuccess)
            return OperationResult<ILogicalFlow<string>>.Ok(new UniversalNineNodeDevelopmentFlow(_phaseId, _subPhaseId, _taskType, _domainTags, _predicateIds));

        return OperationResult<ILogicalFlow<string>>.Ok(new GeneratedCapabilityFlow(
            _phaseId,
            _subPhaseId,
            _taskType,
            _domainTags,
            _predicateIds,
            _unitDomainTag,
            retrievalUnit.Value!,
            originalPrompt: _originalPrompt,
            // Журнал выдач доходит до точки выдачи через фабрику: собранный поток запомнит, какая
            // единица была выбрана и какой ответ показан.
            deliveryLedger: _deliveryLedger));
    }
}

// This flow proves a generated retrieval unit can become executable on the orchestrator retry path.
public sealed class GeneratedCapabilityFlow : ILogicalFlow<string>
{
    private readonly string _phaseId;
    private readonly string _subPhaseId;
    private readonly string _unitDomainTag;
    private readonly string _taskType;
    private readonly string[] _domainTags;
    private readonly string[] _predicateIds;
    private readonly ILogicalUnit<string, string> _unit;
    private readonly string _originalPrompt;

    // Журнал состоявшихся выдач. Он необязателен: без него поток ведёт себя в точности как
    // прежде, а с ним точка выдачи запоминает выбранную единицу и фактически показанный ответ,
    // чтобы пришедшее позже подтверждённое исправление нашло своего адресата.
    private readonly IAnsweredDeliveryLedger<string>? _deliveryLedger;

    public GeneratedCapabilityFlow(
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        string[] predicateIds,
        string unitDomainTag,
        ILogicalUnit<string, string> unit,
        ILogicalTopologyRegistry? topology = null,
        string? originalPrompt = null,
        IAnsweredDeliveryLedger<string>? deliveryLedger = null)
    {
        _phaseId = phaseId;
        _subPhaseId = subPhaseId;
        _unitDomainTag = unitDomainTag;
        _taskType = taskType;
        _domainTags = domainTags?.ToArray() ?? [];
        _predicateIds = CapabilityResultTranslator
            .SnapshotSignedPredicateIds(predicateIds);
        _unit = unit;
        _originalPrompt = originalPrompt ?? string.Empty;
        // Журнал выдач принимается как есть: отсутствие журнала — законный случай, а не отказ.
        _deliveryLedger = deliveryLedger;
    }

    // Execute the retrieval capability unit and return its selected stored output as a flow result.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(request.Input))
            return OperationResult<LogicalFlowResult>.Fail("Flow input cannot be empty.", "EMPTY_FLOW_INPUT");

        var compiled = new SemanticTeachingPromptCompiler().Compile(request.Input);
        var queryText = compiled.IsSuccess ? compiled.Value!.CanonicalText : request.Input;

        OperationResult<LogicalUnitResult<string>> unitResult;
        try
        {
            unitResult = await _unit.ExecuteAsync(new LogicalUnitRequest<string>(
                request.RunId,
                DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
                DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
                _unitDomainTag,
                queryText,
                request.Flags,
                request.Metadata), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);
        if (unitResult.Value is null)
        {
            return OperationResult<LogicalFlowResult>.Fail(
                "Generated capability unit returned no result.",
                "GENERATED_CAPABILITY_RESULT_REQUIRED");
        }
        if (string.IsNullOrWhiteSpace(unitResult.Value.Output))
        {
            return OperationResult<LogicalFlowResult>.Fail(
                "Generated capability unit returned an empty output.",
                "EMPTY_GENERATED_CAPABILITY_OUTPUT");
        }

        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["phase_id"] = _phaseId,
            ["sub_phase_id"] = _subPhaseId,
            ["task_type"] = _taskType,
            ["domain_adapter_tag"] = _unitDomainTag,
            ["generated_capability_flow"] = true.ToString(),
            ["retrieval_unit_selected"] = true.ToString(),
            ["retrieval_unit_id"] = unitResult.Value.UnitId,
            ["retrieval_unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["learned_unit_selected"] = false.ToString()
        };

        // ЭТО ТОЧКА ОБЫЧНОЙ ВЫДАЧИ. Ответ уже получен от выбранной единицы и сейчас уйдёт
        // пользователю. Здесь и только здесь ещё известны одновременно ДВЕ вещи, которые нужны
        // будущему исправлению: объект выбранной единицы и фактически показанный ответ.
        if (_deliveryLedger is not null)
        {
            // Выдача запоминается тем объектом единицы, который её и выдал: одинаковое имя роли у
            // двух владельцев — обычное дело, и адресом оно быть не может, а объект отличает их всегда.
            var recorded = _deliveryLedger.RecordDelivery(
                request.RunId,
                request.FlowId,
                _unit,
                unitResult.Value.UnitId,
                unitResult.Value.UnitRole,
                unitResult.Value.DomainTag,
                unitResult.Value.Output);

            // Признак «выдача запомнена» виден в показателях прогона, а не только внутри журнала.
            metrics["answered_delivery_recorded"] = recorded.IsSuccess.ToString();

            // Удавшаяся запись называет выбранного получателя: именно ему уйдёт будущий урок.
            if (recorded.IsSuccess && recorded.Value is not null)
                metrics["answered_delivery_owner_id"] = recorded.Value.Owner.UnitId;
            else
                // Неудавшаяся запись НЕ отменяет уже полученный ответ пользователю, но и не
                // прячется: её причина названа кодом отказа, чтобы потерянный адрес был виден.
                metrics["answered_delivery_refusal_code"] = recorded.ErrorCode ?? string.Empty;
        }

        var evidenceIds = unitResult.Value.EvidenceIds
            .Concat([$"generated-capability-flow-{_subPhaseId}"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var summary = new LogicalFlowSummary(
            request.FlowId,
            1,
            0,
            1,
            "generated_capability",
            false,
            true,
            false,
            "USE_PROBATIONARY_RETRIEVAL_CAPABILITY",
            unitResult.Value.Confidence,
            $"sub_phase={_subPhaseId}",
            "Generated retrieval capability selected a stored output from the registry.",
            0,
            0);

        var humanResult = CapabilityResultTranslator
            .TranslateSignedGeneratedCapability(
                unitResult.Value.Output,
                _subPhaseId,
                _taskType,
                _predicateIds,
                _originalPrompt);

        var result = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            "PASS",
            humanResult,
            summary,
            metrics,
            evidenceIds,
            unitResult.Value.Warnings);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

}
