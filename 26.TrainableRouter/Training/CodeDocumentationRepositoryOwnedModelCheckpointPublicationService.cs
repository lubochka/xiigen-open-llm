// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Evidence returned only after a CodeDocumentation checkpoint was written into
/// the canonical repository layout and reopened from disk.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence(
    int Step,
    string CheckpointRoot,
    string WeightsPath,
    string ManifestPath,
    string RepositoryRelativeCheckpointDirectory,
    string RepositoryRelativeWeightsPath,
    string RepositoryRelativeManifestPath,
    string ManifestSha256,
    string WeightsSha256,
    long WeightsByteLength,
    RepositoryOwnedModelCheckpointParentIdentity? ParentCheckpoint,
    string? ColdStartReason,
    RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifest,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    IReadOnlyList<string> ProvenanceRefs,
    IReadOnlyList<string> EvidenceRefs);

internal abstract record CodeDocumentationInitialBaselinePublicationOutcome // Передаём команде типизированную границу эффекта без модели или caller-флагов.
{ // Все результаты создаёт тот же владелец initial публикации.
    internal sealed record Created(CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence Publication) : CodeDocumentationInitialBaselinePublicationOutcome; // Запись и reopen уже подтверждены существующим владельцем.
    internal sealed record AlreadyPresent : CodeDocumentationInitialBaselinePublicationOutcome; // Совместимый parent найден единственным START; новую публикацию не вызываем.
    internal sealed record ReconciliationRequired(string ErrorCode) : CodeDocumentationInitialBaselinePublicationOutcome; // После возможной записи ошибка требует сверки, а не повторного bootstrap.
} // Ни один outcome не переносит Model либо право caller перемаркировать веса.

/// <summary>
/// Publishes the already-started CodeDocumentation model as a sealed
/// repository-owned checkpoint. This is a narrow save boundary: it does not
/// train, call Qwen, create DPO rows, or wire a runner.
/// </summary>
public static class CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
{
    // These evidence labels are intentionally plain: a report can say exactly
    // which disk boundaries ran without pretending that training happened here.
    public const string FullCheckpointWrittenEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_FULL_BYTES_WRITTEN";
    public const string ManifestSealedEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_SEALED";
    public const string ReopenedAfterPublishEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_REOPENED_AFTER_PUBLISH";
    public const string PublicationOnlyEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLICATION_ONLY_NO_TRAINING";

    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLICATION_INVALID";
    public const string PublishFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLICATION_FAILED";
    public const string VerifyFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLICATION_VERIFY_FAILED";
    public const string CancelledErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLICATION_CANCELLED";

    internal static OperationResult<CodeDocumentationInitialBaselinePublicationOutcome> PublishInitialBaseline( // Единственный owner START возвращает созданное состояние, найденный parent или неизвестный итог записи.
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest, // Используем прежний проверенный START и его явное разрешение холодного старта.
        RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest, // Пара остаётся подлинным источником происхождения, но ещё не applied history.
        DateTimeOffset createdAtUtc, IReadOnlyList<string> provenanceRefs, // Сохраняем прежние время и доказательства происхождения публикации.
        CancellationToken cancellationToken = default) // Отмена проходит существующие START и publication gates.
    { // Не принимаем Model или публичный START result, которые caller мог обучить либо подменить.
        var publicationAttempted = false; // Только внутренняя seal-граница меняет отказ до записи на reconciliation.
        try // Сохраняем границу возможной записи также при отмене и некритическом исключении.
        { // Caller не получает ссылку на созданную либо восстановленную модель.
            var started = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(startRequest, cancellationToken); // Только этот START выбирает existing parent либо clean missing и создаёт fresh веса.
            if (!started.IsSuccess || started.Value is null) // Corrupt, incompatible и cancelled START не допускаются к sealer.
                return started.MapError<CodeDocumentationInitialBaselinePublicationOutcome>(); // Отказ до checkpoint записи остаётся обычным отказом, без fallback.
            if (started.Value.ParentManifest is not null) // Найденная модель не становится initial baseline и не порождает Step 2.
                return OperationResult<CodeDocumentationInitialBaselinePublicationOutcome>.Ok(new CodeDocumentationInitialBaselinePublicationOutcome.AlreadyPresent()); // Типизированный результат не удерживает parent Model в команде.
            var published = PublishCore(startRequest, started.Value, dpoDatasetManifest, createdAtUtc, provenanceRefs, cancellationToken, initialBaselineNoUpdate: true, // Переиспользуем прежнюю transaction только для owner-created fresh модели.
                beforeCheckpointWrite: () => publicationAttempted = true); // Закрытый callback отмечает точный вход в sealer и не передаётся caller.
            if (!published.IsSuccess || published.Value is null) // Различаем раннюю validation failure и неизвестный итог возможной записи.
                return RefuseOrReconcile(published.ErrorMessage, string.IsNullOrEmpty(published.ErrorCode) ? PublishFailedErrorCode : published.ErrorCode); // Сохраняем owner error code и не повторяем запись.
            return OperationResult<CodeDocumentationInitialBaselinePublicationOutcome>.Ok(new CodeDocumentationInitialBaselinePublicationOutcome.Created(published.Value)); // Возвращаем только фактическое sealed/reopened evidence без Model.
        } // Завершаем единственный initial START и его публикацию.
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) // Отмена до sealer и после возможной записи имеет разные результаты.
        { // Не начинаем второй START и не восстанавливаем unknown write повтором.
            return RefuseOrReconcile("Initial baseline publication was cancelled.", CancelledErrorCode); // До seal это отказ, после seal требуется сверка с диском.
        } // Завершаем безопасный cancellation outcome владельца.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Сохраняем тот же fail-closed контракт для некритических исключений.
        { // Исключение не даёт права пометить переданные веса как fresh baseline.
            return RefuseOrReconcile("Initial baseline publication is unavailable.", PublishFailedErrorCode); // После возможной записи оставляем только reconciliation.
        } // Завершаем обработку недоступности без скрытого fallback.
        OperationResult<CodeDocumentationInitialBaselinePublicationOutcome> RefuseOrReconcile(string message, string code) // Внутренняя граница не зависит от bool или строк caller.
            => publicationAttempted // Признак устанавливает только private seal boundary этого вызова.
                ? OperationResult<CodeDocumentationInitialBaselinePublicationOutcome>.Ok(new CodeDocumentationInitialBaselinePublicationOutcome.ReconciliationRequired(code)) // Возможная запись никогда не становится безопасным повтором.
                : OperationResult<CodeDocumentationInitialBaselinePublicationOutcome>.Fail(message, code); // До sealer возвращаем прежний типизированный отказ.
    } // Завершаем owner-created baseline до первого настоящего урока.

    /// <summary>
    /// Saves the model returned by the START service as the next sealed
    /// repository-owned checkpoint and proves the saved bytes by reopening them.
    /// </summary>
    public static OperationResult<
        CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>
        Publish(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest,
            CodeDocumentationRepositoryOwnedModelCheckpointStart start,
            RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest,
            DateTimeOffset createdAtUtc,
            IReadOnlyList<string> provenanceRefs,
            CancellationToken cancellationToken = default)
        => PublishCore(startRequest, start, dpoDatasetManifest, createdAtUtc, provenanceRefs, cancellationToken, initialBaselineNoUpdate: false); // Обычный путь сохраняет applied semantics для любых переданных моделей.

    private static OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence> PublishCore( // Оба пути используют одну существующую transaction и один sealer.
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest, // Закрепляем прежнюю идентичность модели и репозитория.
        CodeDocumentationRepositoryOwnedModelCheckpointStart start, // Исходный путь передаёт сюда только локально созданную owner модель.
        RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest, // Сохраняем полную привязку к проверенному DPO набору.
        DateTimeOffset createdAtUtc, IReadOnlyList<string> provenanceRefs, // Не меняем правила lineage и evidence.
        CancellationToken cancellationToken, bool initialBaselineNoUpdate, // Initial роль остаётся закрытым флагом владельца, отсутствующим в публичном запросе.
        Action? beforeCheckpointWrite = null) // Только initial owner отмечает свою границу эффекта; ordinary Publish не меняется.
    { // Сохраняем все существующие validation, cancellation, seal и reopen gates.
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>.Fail(
                "CodeDocumentation repository-owned model checkpoint publication was cancelled before writing bytes.",
                CancelledErrorCode);
        }

        // WHY: the publication boundary starts by reusing START validation so a
        // caller cannot publish weights into a different repository, domain,
        // design pattern, model id, or runtime fingerprint than the model was
        // started for.
        var startRequestValidation =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .ValidateRequest(startRequest);
        if (!startRequestValidation.IsSuccess)
        {
            return startRequestValidation.MapError<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>();
        }

        var lineage = BuildLineage(
            startRequest,
            start,
            createdAtUtc,
            provenanceRefs);
        if (!lineage.IsSuccess || lineage.Value is null)
            return lineage.MapError<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>();

        // WHY: this address is derived from the checked lineage step, not from a
        // free-form path. Continuation can only write parent.Step + 1, and an
        // explicit first cold start can only write 000001.
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                startRequest.RepositoryRoot,
                startRequest.DomainSlug,
                startRequest.DesignPatternSlug,
                startRequest.ModelIdSlug,
                startRequest.ModelVersion,
                lineage.Value.Step);
        if (!address.IsSuccess || address.Value is null)
        {
            return address.MapError<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>();
        }

        var manifestRequest = new RepositoryOwnedModelCheckpointManifestRequest(
            lineage.Value.ParentCheckpoint,
            lineage.Value.ColdStartReason ?? string.Empty,
            dpoDatasetManifest,
            startRequest.RuntimeCompatibility,
            createdAtUtc,
            lineage.Value.ProvenanceRefs) // Сохраняем публичный manifest request без нового caller authority.
        { // Внутренний контракт уже доступен TrainableRouter по существующему InternalsVisibleTo.
            InitialBaselineNoUpdate = initialBaselineNoUpdate // Только owner-created entry может запечатать initial schema.
        }; // Обычная публикация всегда оставляет этот признак false.

        OperationResult<RepositoryOwnedModelCheckpointManifest> sealedManifest;
        try
        {
            // The LlmModel writes its complete checkpoint stream directly into
            // the shared sealer. There is no side cache, quarantine, temp repo,
            // or manifest-only shortcut: the repository tree receives the full
            // model bytes that the next START will load.
            beforeCheckpointWrite?.Invoke(); // Отмечаем возможную checkpoint запись после validation и сразу перед прежним sealer.
            sealedManifest =
                RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                    address.Value,
                    start.Model.SaveCheckpointToStream,
                    manifestRequest);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>.Fail(
                "CodeDocumentation model checkpoint publication could not write the full model checkpoint stream.",
                PublishFailedErrorCode);
        }

        if (!sealedManifest.IsSuccess || sealedManifest.Value is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>.Fail(
                sealedManifest.ErrorMessage,
                string.IsNullOrWhiteSpace(sealedManifest.ErrorCode)
                    ? PublishFailedErrorCode
                    : sealedManifest.ErrorCode);
        }

        var reopened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(address.Value);
        if (!PublicationReopenedSameBytes(
                sealedManifest.Value,
                reopened,
                address.Value))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>.Fail(
                "CodeDocumentation repository-owned model checkpoint publication was not freshly reopened with the same manifest hash, weights hash, and byte length.",
                VerifyFailedErrorCode);
        }

        var manifest = reopened.Value!.Manifest;
        return OperationResult<
            CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence>.Ok(
            new CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence(
                manifest.Step,
                address.Value.CheckpointRoot,
                address.Value.WeightsPath,
                address.Value.ManifestPath,
                manifest.RepositoryRelativeCheckpointDirectory,
                manifest.RepositoryRelativeWeightsPath,
                manifest.RepositoryRelativeManifestPath,
                manifest.ManifestSha256,
                reopened.Value.WeightsSha256,
                reopened.Value.WeightsByteLength,
                manifest.ParentCheckpoint,
                string.IsNullOrEmpty(manifest.ColdStartReason)
                    ? null
                    : manifest.ColdStartReason,
                manifest.DpoDatasetManifest,
                manifest.RuntimeCompatibility,
                manifest.ProvenanceRefs,
                [
                    FullCheckpointWrittenEvidence,
                    ManifestSealedEvidence,
                    ReopenedAfterPublishEvidence,
                    PublicationOnlyEvidence,
                    "repository_manifest:" +
                    manifest.RepositoryRelativeManifestPath,
                    "repository_weights:" +
                    manifest.RepositoryRelativeWeightsPath
                ]));
    }

    private static OperationResult<PublicationLineage> BuildLineage(
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest,
        CodeDocumentationRepositoryOwnedModelCheckpointStart? start,
        DateTimeOffset createdAtUtc,
        IReadOnlyList<string>? provenanceRefs)
    {
        if (start is null ||
            start.Model is null ||
            startRequest.RuntimeCompatibility is null ||
            createdAtUtc.Offset != TimeSpan.Zero ||
            provenanceRefs is null ||
            provenanceRefs.Count == 0)
        {
            return InvalidLineage();
        }

        // WHY: START returned a concrete model object. Before saving it, this
        // service proves the model still has the ModelConfig fingerprint named
        // by the repository-owned request, so another model shape cannot be
        // published under a compatible-looking manifest.
        var modelConfigSha256 =
            TrainingSourceProvenance.Identify(start.Model.Config).Sha256;
        if (!StringComparer.Ordinal.Equals(
                modelConfigSha256,
                startRequest.RuntimeCompatibility.ConfigSha256))
        {
            return InvalidLineage();
        }

        var hasParent = start.ParentManifest is not null;
        var hasColdReason = !string.IsNullOrWhiteSpace(start.ColdStartReason);
        if (hasParent == hasColdReason)
            return InvalidLineage();

        var refsSnapshot = Array.AsReadOnly(provenanceRefs.ToArray());
        if (hasParent)
        {
            var parent = start.ParentManifest!;
            if (!ParentManifestMatchesRequest(parent, startRequest))
                return InvalidLineage();

            int nextStep;
            try
            {
                // WHY: a repository-owned lineage cannot continue past the
                // largest representable checkpoint step. Overflow must be an
                // auditable fail-closed result, not an escaped exception that
                // leaves the caller unsure whether bytes were written.
                nextStep = checked(parent.Step + 1);
            }
            catch (OverflowException)
            {
                return InvalidLineage();
            }

            return OperationResult<PublicationLineage>.Ok(
                new PublicationLineage(
                    nextStep,
                    new RepositoryOwnedModelCheckpointParentIdentity(
                        parent.RepositoryRelativeManifestPath,
                        parent.Step,
                        parent.WeightsSha256,
                        parent.WeightsByteLength),
                    ColdStartReason: null,
                    refsSnapshot));
        }

        // WHY: a first checkpoint without parent is allowed only when it is the
        // exact cold-start decision returned by START and the original request
        // still carries that same human reason. This prevents a quiet empty
        // lineage from being published later as if it were trained continuation.
        if (!startRequest.ColdStartPolicy.IsAuthorized ||
            !StringComparer.Ordinal.Equals(
                startRequest.ColdStartPolicy.HumanReason,
                start.ColdStartReason))
        {
            return InvalidLineage();
        }

        return OperationResult<PublicationLineage>.Ok(
            new PublicationLineage(
                Step: 1,
                ParentCheckpoint: null,
                ColdStartReason: start.ColdStartReason,
                refsSnapshot));
    }

    private static bool ParentManifestMatchesRequest(
        RepositoryOwnedModelCheckpointManifest parent,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request)
        => parent.Step > 0 &&
           StringComparer.Ordinal.Equals(parent.Domain, request.DomainSlug) &&
           StringComparer.Ordinal.Equals(
               parent.DesignPattern,
               request.DesignPatternSlug) &&
           StringComparer.Ordinal.Equals(parent.ModelId, request.ModelIdSlug) &&
           StringComparer.Ordinal.Equals(parent.Version, request.ModelVersion) &&
           parent.WeightsByteLength > 0 &&
           parent.RuntimeCompatibility == request.RuntimeCompatibility;

    private static bool PublicationReopenedSameBytes(
        RepositoryOwnedModelCheckpointManifest sealedManifest,
        OperationResult<RepositoryOwnedModelCheckpointOpenResult> reopened,
        RepositoryOwnedModelCheckpointAddress address)
        => reopened.IsSuccess &&
           reopened.Value is not null &&
           StringComparer.Ordinal.Equals(
               reopened.Value.Manifest.ManifestSha256,
               sealedManifest.ManifestSha256) &&
           StringComparer.Ordinal.Equals(
               reopened.Value.Manifest.RepositoryRelativeManifestPath,
               address.ManifestRelativePath) &&
           StringComparer.Ordinal.Equals(
               reopened.Value.Manifest.RepositoryRelativeWeightsPath,
               address.WeightsRelativePath) &&
           StringComparer.Ordinal.Equals(
               reopened.Value.WeightsSha256,
               sealedManifest.WeightsSha256) &&
           reopened.Value.WeightsByteLength == sealedManifest.WeightsByteLength;

    private static OperationResult<PublicationLineage> InvalidLineage()
        => OperationResult<PublicationLineage>.Fail(
            "CodeDocumentation repository-owned model checkpoint publication needs one exact START lineage: either a sealed parent manifest for continuation or the exact authorized cold-start reason for step 000001.",
            InvalidErrorCode);

    private sealed record PublicationLineage(
        int Step,
        RepositoryOwnedModelCheckpointParentIdentity? ParentCheckpoint,
        string? ColdStartReason,
        IReadOnlyList<string> ProvenanceRefs);
}
