// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TrainableRouter.Training;

/// <summary>
/// Локальная серверная дверь полного обучения CodeDocumentation.
/// Запрос приносит routing-run либо raw sealed DPO с явным NOT_PERFORMED и версию
/// сохранённого DPO. Форму модели, tokenizer, LoRA, корень репозитория,
/// идентичность модели, число DPO-шагов и право менять веса сервер берёт из
/// своей уже принятой конфигурации, а не из клиентского JSON.
/// </summary>
internal static class CodeDocumentationRepositoryOwnedDpoTrainingEndpoint
{
    // Human reason: the server consumes the same wire route and limit as the
    // CLI client, while the training authority itself stays in ModelServer.
    internal const string EndpointPath =
        CodeDocumentationRepositoryOwnedDpoTrainingWireContract.EndpointPath;
    internal const int MaximumProvenanceRefCount =
        CodeDocumentationRepositoryOwnedDpoTrainingWireContract
            .MaximumProvenanceRefCount;
    internal const string InvalidRequestErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_REQUEST_INVALID";
    internal const string ParentCheckpointRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_PARENT_REQUIRED";

    internal static IEndpointConventionBuilder
        MapCodeDocumentationRepositoryOwnedDpoTraining(
            this IEndpointRouteBuilder endpoints)
        => MapCodeDocumentationRepositoryOwnedDpoTrainingCore(
            endpoints,
            Program.RepositoryOwnedTrainingTrustedRepositoryRoot);

    internal static IEndpointConventionBuilder
        MapCodeDocumentationRepositoryOwnedDpoTrainingCore(
            this IEndpointRouteBuilder endpoints,
            string trustedRepositoryRoot)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(trustedRepositoryRoot);

        foreach (var operation in new[] { "train-and-hold", "resume-baseline-and-hold", "publish-held" }) // Three explicit operations share the existing admitted server context and guards.
            endpoints.MapPost(CodeDocumentationScientificTrainingModelServerClient.EndpointPrefix + operation, (CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest? request, IServiceProvider services, CancellationToken token) => ExecuteScientificCoreAsync(request, services, trustedRepositoryRoot, operation, token)); // Request cancellation is linked to host lifetime below, without an arbitrary deadline.

        return endpoints.MapPost(
            EndpointPath,
            (
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest?
                        request,
                    IServiceProvider services,
                    CancellationToken cancellationToken) =>
                ExecuteCoreAsync(
                    request,
                    services,
                    DateTimeOffset.UtcNow,
                    trustedRepositoryRoot,
                    cancellationToken));
    }

    internal static async Task<IResult> ExecuteAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest? request,
        IServiceProvider services,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
        => await ExecuteCoreAsync(
                request,
                services,
                createdAtUtc,
                Program.RepositoryOwnedTrainingTrustedRepositoryRoot,
                cancellationToken)
            .ConfigureAwait(false);

    internal static async Task<IResult> ExecuteCoreAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest? request,
        IServiceProvider services,
        DateTimeOffset createdAtUtc,
        string trustedRepositoryRoot,
        CancellationToken cancellationToken = default,
        string? scientificOperation = null) // Only these server route delegates select the explicit lifecycle, never request JSON.
    {
        ArgumentNullException.ThrowIfNull(trustedRepositoryRoot);
        var validation = ValidateRequest(request, createdAtUtc);
        if (!validation.IsSuccess)
            return Error(StatusCodes.Status400BadRequest, validation);

        CodeDocumentationScientificTrainingPolicy? scientificPolicy = null; // Default routing retains its original publication path.
        var scientificConfiguration = services.GetService<CodeDocumentationScientificTrainingConfiguration>(); // Configuration was captured before host start.
        if (scientificOperation is not null) // New scientific operations require explicit raw input and configured frozen trust before model/context work.
        {
            if (request!.RoutingDisposition != CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed || scientificOperation is not ("train-and-hold" or "resume-baseline-and-hold" or "publish-held")) return Error(StatusCodes.Status400BadRequest, InvalidRequest()); // An unknown/legacy marker cannot mint scientific authority.
            var policy = scientificConfiguration?.Open(trustedRepositoryRoot, scientificOperation == "publish-held", cancellationToken) ?? OperationResult<CodeDocumentationScientificTrainingPolicy>.Fail("Scientific startup is disabled.", CodeDocumentationScientificTrainingPolicy.Invalid); // Missing choice refuses before C0.
            if (!policy.IsSuccess || policy.Value is null) return Error(StatusCodes.Status503ServiceUnavailable, policy); // No baseline or optimizer has run.
            scientificPolicy = policy.Value; // The request cannot replace any pre-T1 policy or trust source.
        }
        else if (request!.RoutingDisposition is not null) return Error(StatusCodes.Status409Conflict, OperationResult<bool>.Fail("Raw first-RP01 training requires train-and-hold; publish-held requires the real pinned independent verdict.", CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificHoldRequired)); // Old raw train-existing cannot auto-publish the first Trial.

        var initialConfiguration = request!.RoutingDisposition is null ? null : services.GetService<CodeDocumentationInitialTrainingConfiguration>(); // The raw route requires a separate server-owned startup choice, never permission from its marker.
        if (request.RoutingDisposition is not null) // Refuse absent or malformed initial choice before opening assessment profiles or preparing a model.
        { // Legacy routing calls keep their previous startup contract.
            var choice = initialConfiguration?.RequireChoice() ?? OperationResult<bool>.Fail("Explicit first-root training startup choice is missing.", CodeDocumentationInitialTrainingConfiguration.Required); // A B0 or raw marker alone cannot authorize optimizer creation.
            if (!choice.IsSuccess) return Error(StatusCodes.Status503ServiceUnavailable, choice); // No baseline/evaluator/model work occurs on this refusal.
        } // The accepted choice is still bound to actual admitted context and B0 below.

        var domainProfile =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveSupportedDomainProfile(request!.DomainSlug);
        if (!domainProfile.IsSuccess || domainProfile.Value is null)
            return Error(StatusCodes.Status400BadRequest, domainProfile);
        if (!StringComparer.Ordinal.Equals(
                request.DesignPatternSlug,
                domainProfile.Value.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "CodeDocumentation repository-owned DPO training can use only the design pattern admitted for the requested documentation domain.",
                    "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_PATTERN_MISMATCH"));
        }

        var assessmentProfiles =
            CodeDocumentationSavedModelAssessmentProfileResolver
                .ResolveFromEnvironment();
        if (!assessmentProfiles.IsSuccess || assessmentProfiles.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, assessmentProfiles);
        if (!assessmentProfiles.Value.TryGetValue(
                request.DomainSlug,
                out var assessmentProfile) ||
            !StringComparer.Ordinal.Equals(
                assessmentProfile.DesignPatternSlug,
                request.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "CodeDocumentation repository-owned DPO training requires the server-owned saved-model assessment profile before any training attempt.",
                    CodeDocumentationSavedModelAssessmentEndpoint
                        .ProfileMissingErrorCode));
        }

        // Важно: право менять веса не создаётся здесь заново.
        // Берём ровно тот broker, который уже принял ModelServer при старте.
        var startup = services.GetService<ContinuousLearningModelServerStartup>();
        var fileAccess = services.GetService<ServerOwnedContinuousLearningFileAccess>();
        var leaseBroker = services.GetService<IServerOwnedDpoTrainingLeaseBroker>();
        if (startup is null || fileAccess is null || leaseBroker is null)
        {
            return Error(
                StatusCodes.Status503ServiceUnavailable,
                OperationResult<bool>.Fail(
                    "CodeDocumentation repository-owned DPO training is unavailable: ModelServer has not materialized the server-owned ContinuousLearning context.",
                    "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_CONTEXT_UNAVAILABLE"));
        }

        var context = startup.PrepareRepositoryOwnedTrainingExecutionContext(
            request!.DomainSlug,
            fileAccess,
            cancellationToken);
        if (!context.IsSuccess || context.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, context);

        CodeDocumentationInitialTrainingInputs? initialInputs = null; // Preserve the legacy context when the explicit raw first-update route is not selected.
        if (initialConfiguration is not null) // Only the separately captured startup choice can supply the initial optimizer permission.
        { // Verify context, zero-dropout configuration and exact saved root before any C0 call.
            var bound = initialConfiguration.Bind(context.Value); // Existing generation options provide model/LoRA/seed; strict loader proves actual root B0.
            if (!bound.IsSuccess || bound.Value is null) return Error(StatusCodes.Status409Conflict, bound); // Missing/mismatched/continuation sources refuse before expensive assessment.
            initialInputs = bound.Value; // Retain the checked configuration and exact auditable optimizer reason.
            context = OperationResult<ContinuousLearningRepositoryOwnedTrainingExecutionContext>.Ok(context.Value with { LoraConfig = initialInputs.StartRequest.LoraConfig! }); // Opt-in first training uses its actual B0 LoRA, without changing generic composition defaults.
        } // All existing legal/current-manifest/lease checks still follow.

        var start = context.Value.RepositoryStartRequest;
        if (!RepositoryRootIsCanonical(
                start.RepositoryRoot,
                trustedRepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                request.DesignPatternSlug,
                start.DesignPatternSlug))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "CodeDocumentation repository-owned DPO training must run only for the canonical checkout and the design pattern admitted by ModelServer startup.",
                    "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_START_MISMATCH"));
        }

        var adapterSeedAuthority = initialInputs is not null // First-B0 training keeps exact seed streams from admitted generation configuration.
            ? OperationResult<LoRA.LoraPairRandomSeedAuthority>.Ok(initialInputs.StartRequest.AdapterSeedAuthority!) // The binding already proved root seed and exact topology.
            : CodeDocumentationDpoSeedAuthorityResolver.Resolve(start.DomainSlug, context.Value.RootInitializationSeed); // Preserve the legacy domain resolver unchanged.
        if (!adapterSeedAuthority.IsSuccess || adapterSeedAuthority.Value is null)
            return Error(StatusCodes.Status409Conflict, adapterSeedAuthority);

        var datasetAddress =
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                start.RepositoryRoot,
                start.DomainSlug,
                start.DesignPatternSlug,
                request.DatasetVersion);
        if (!datasetAddress.IsSuccess || datasetAddress.Value is null)
            return Error(StatusCodes.Status400BadRequest, datasetAddress);
        if (request.RoutingDisposition is not null) // The explicit raw route must be bound to an actual raw-v2 source on disk.
        { // Do not accept a legacy paid-reference dataset relabelled by a wire marker.
            var identity = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(datasetAddress.Value); // Read through the existing strict manifest owner.
            if (!identity.IsSuccess || identity.Value is null) return Error(StatusCodes.Status409Conflict, identity); // Preserve corrupt/missing manifest refusal.
            if (identity.Value.RequestIdentity.Schema != RepositoryOwnedDpoDatasetManifestSealer.RawRequestIdentitySchema || // Require the actual raw identity format.
                identity.Value.RequestIdentity.RawSource is null || // A marker cannot replace the sealed source preimage.
                identity.Value.RequestIdentity.RequestIdentitySha256 != request.RoutingRun.RepositoryOwnedDpoReceipt!.RequestIdentitySha256) // Bind the receipt to that exact raw source identity.
                return Error(StatusCodes.Status400BadRequest, InvalidRequest()); // Refuse mismatched source before model preparation or quality calls.
        } // The existing current-manifest and full pair reopen checks still follow.
        // Клиент может назвать только версию сохранённого DPO, но не выбрать
        // произвольный набор. Тренируемся только на manifest, который сервер
        // уже удерживает как текущий для принятого плана обучения.
        if (!DatasetMatchesServerCurrentManifest(
                datasetAddress.Value,
                request.RoutingRun.RepositoryOwnedDpoReceipt,
                start.CurrentDpoDatasetManifest))
        {
            return Error(
                StatusCodes.Status409Conflict,
                OperationResult<bool>.Fail(
                    "CodeDocumentation repository-owned DPO training must use the exact saved DPO manifest already admitted by ModelServer startup.",
                    "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_DPO_MANIFEST_MISMATCH"));
        }

        var corpus = CodeDocumentationSavedModelAssessmentEndpoint
            .OpenConfiguredCorpus(start.RepositoryRoot, assessmentProfile);
        if (!corpus.IsSuccess || corpus.Value is null)
            return Error(StatusCodes.Status409Conflict, corpus);

        var qualityPlan =
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .PlanQualityGate(
                    corpus.Value.Corpus,
                    assessmentProfile.GenerationSettings,
                    startup.TeacherConfiguration,
                    MeasurementNoisePolicyConfig.Canonical);
        if (!qualityPlan.IsSuccess || qualityPlan.Value is null)
            return Error(StatusCodes.Status409Conflict, qualityPlan);

        var evaluationFactory =
            services.GetService<ICodeDocumentationSavedModelAssessmentEvaluationClientFactory>()
            ?? new DefaultCodeDocumentationSavedModelAssessmentEvaluationClientFactory();
        var evaluationClient = evaluationFactory.Create(startup.TeacherConfiguration);
        if (!evaluationClient.IsSuccess || evaluationClient.Value is null)
            return Error(StatusCodes.Status503ServiceUnavailable, evaluationClient);

        var ownedEvaluationClient = evaluationClient.Value;
        var successPrepared = false;
        try
        {
            var startRequest = initialInputs?.StartRequest ?? // Exact saved B0 options keep zero dropout, original seed streams and denied model cold start.
                new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
                    start.RepositoryRoot,
                    start.DomainSlug,
                    start.DesignPatternSlug,
                    start.ModelIdSlug,
                    start.ModelVersion,
                    context.Value.ModelConfig,
                    start.RuntimeCompatibility,
                    start.ColdStartPolicy,
                    LoraConfig: context.Value.LoraConfig,
                    AdapterSeedAuthority: adapterSeedAuthority.Value);

            var provenanceRefs = BuildProvenanceRefs(
                request,
                context.Value.EvidenceRefs);
            var qualityContext =
                new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(
                    corpus.Value.Corpus,
                    corpus.Value.Manifest,
                    context.Value.Tokenizer,
                    ownedEvaluationClient,
                    startup.TeacherConfiguration,
                    assessmentProfile.GenerationSettings,
                    MeasurementNoisePolicyConfig.Canonical,
                    startup.TeacherConfiguration.EvaluationProfile == LocalQwenOllamaInferenceProfileAuthority.ManualEvaluation
                        ? Timeout.InfiniteTimeSpan
                        : CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration,
                    createdAtUtc,
                    BuildQualityEvidenceRefs(
                        provenanceRefs,
                        corpus.Value.Manifest));
            var publicationRequest = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
                            request.RoutingRun,
                            datasetAddress.Value,
                            startRequest,
                            context.Value.Tokenizer,
                            context.Value.LoraConfig,
                            leaseBroker,
                            request.TrainingRunId,
                            context.Value.DpoStepCount,
                            createdAtUtc,
                            provenanceRefs,
                            qualityContext,
                            AdapterSeedAuthority: adapterSeedAuthority.Value,
                            OptimizerStateColdStartAuthorization: initialInputs?.OptimizerAuthorization) { RoutingDisposition = request.RoutingDisposition }; // Forward truthful source semantics and existing first optimizer reason without granting science from a marker.
            if (scientificPolicy is not null) // The explicit lifecycle returns HELD or actual PUBLISHED with a distinct safe wire contract.
            {
                var scientific = scientificOperation == "publish-held" ? await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.PublishHeldForScienceAsync(publicationRequest, scientificPolicy, scientificConfiguration!.HeldSha256, scientificConfiguration.VerdictSha256, cancellationToken).ConfigureAwait(false) : scientificOperation == "resume-baseline-and-hold" ? await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ResumeBaselineAndHoldForScienceAsync(publicationRequest, scientificPolicy, cancellationToken).ConfigureAwait(false) : await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndHoldForScienceAsync(publicationRequest, scientificPolicy, cancellationToken).ConfigureAwait(false); // Publish-only reopens the same immutable Trial with zero optimizer/assessment calls.
                if (!scientific.IsSuccess || scientific.Value is null) return Error(StatusCodes.Status409Conflict, scientific); // Unknown or partial effects remain reconciliation-required.
                var body = scientific.Value; // No raw closed answers enter the HTTP response.
                var published = body.Publication is null ? null : CreateSuccessResponse(body.Publication); // Reuse existing accepted-publication wire validation.
                if (published is not null && (!published.IsSuccess || published.Value is null)) return Error(StatusCodes.Status409Conflict, published); // Never report a fake publication receipt.
                successPrepared = true; // Dispose the same admitted evaluation client below.
                return Results.Json(new CodeDocumentationScientificTrainingHttpResponse(body.Status, body.TrainingRunId, body.HeldTrial, body.NativeQualityStatus, published?.Value) { EvaluationAccounting = body.EvaluationAccounting }, statusCode: StatusCodes.Status200OK); // Only private references/native status and ordinary publication evidence are exposed.
            }
            var trained = await CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.TrainAndPublishAsync(publicationRequest, cancellationToken).ConfigureAwait(false); // Preserve legacy routing behavior and bounded transport.

            if (!trained.IsSuccess || trained.Value is null)
            {
                return Error(
                    StatusCodes.Status409Conflict,
                    trained);
            }

            var successResponse = CreateSuccessResponse(trained.Value);
            if (!successResponse.IsSuccess || successResponse.Value is null)
            {
                return Error(
                    StatusCodes.Status409Conflict,
                    successResponse);
            }

            var successResult = Results.Json(
                successResponse.Value,
                statusCode: StatusCodes.Status200OK);
            successPrepared = true;
            return successResult;
        }
        finally
        {
            try
            {
                ownedEvaluationClient.Dispose();
            }
            catch (Exception exception)
                when (!successPrepared &&
                    SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Preserve the primary failure or exception during cleanup.
            }
        }
    }

    private static async Task<IResult> ExecuteScientificCoreAsync(CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest? request, IServiceProvider services, string root, string operation, CancellationToken token) // Host shutdown and caller cancellation are the only new-path lifetime authorities.
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token, services.GetRequiredService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>().ApplicationStopping); // No CancelAfter or arbitrary local timer.
        return await ExecuteCoreAsync(request, services, DateTimeOffset.UtcNow, root, lifetime.Token, operation).ConfigureAwait(false); // Retain all existing URI/server-context/source/lease checks.
    } // No background server work is detached from caller/host lifetime.

    internal static OperationResult<
        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
        CreateSuccessResponse(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence?
                evidence)
    {
        if (evidence is null ||
            evidence.ParentCheckpointStep is null ||
            evidence.ParentCheckpointStep < 1 ||
            evidence.PublishedCheckpointStep !=
            evidence.ParentCheckpointStep + 1 ||
            evidence.QualityDecision is null ||
            !evidence.QualityDecision.Accepted)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Fail(
                "CodeDocumentation repository-owned DPO endpoint cannot publish a success wire response without a positive resumed parent checkpoint, exact next child step, and accepted quality decision.",
                ParentCheckpointRequiredErrorCode);
        }

        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Ok(
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Success(
                evidence));
    }

    private static OperationResult<bool> ValidateRequest(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest? request,
        DateTimeOffset createdAtUtc)
    {
        if (request is null ||
            !IsBoundedRequiredText(
                request.DomainSlug,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.DesignPatternSlug,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.DatasetVersion,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.TrainingRunId,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumTrainingRunIdLength) ||
            request.RoutingRun is null ||
            !AreSafeEvidenceRefs(
                request.ProvenanceRefs,
                MaximumProvenanceRefCount) ||
            createdAtUtc.Offset != TimeSpan.Zero)
        {
            return InvalidRequest();
        }

        var disposition = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition( // Recheck the raw marker after server JSON deserialization.
            request.RoutingRun, request.RoutingDisposition, request.DomainSlug); // Keep legacy null-marker receipt semantics intact.
        if (!disposition.IsSuccess) return disposition; // Client and server reject contradictions with the same business code.
        if (!RoutingRunReceiptIsSafe(request.RoutingRun))
        {
            return InvalidRequest();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyList<string> BuildProvenanceRefs(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest request,
        IReadOnlyList<string> serverEvidenceRefs)
    {
        var refs = new List<string>(request.ProvenanceRefs.Count + serverEvidenceRefs.Count + 2)
        {
            "modelserver-route:" + EndpointPath.TrimStart('/'),
            "modelserver-training-run:" + request.TrainingRunId,
        };
        refs.AddRange(request.ProvenanceRefs);
        refs.AddRange(serverEvidenceRefs);
        return Array.AsReadOnly(refs.ToArray());
    }

    private static IReadOnlyList<string> BuildQualityEvidenceRefs(
        IReadOnlyList<string> publicationProvenanceRefs,
        RepositoryOwnedReferenceCorpusManifest corpusManifest)
    {
        var refs = new List<string>(publicationProvenanceRefs.Count + 3)
        {
            "modelserver-quality-gate:server-owned-context",
            "reference_corpus_manifest:" +
            corpusManifest.RepositoryRelativeManifestPath,
        };
        refs.AddRange(publicationProvenanceRefs);
        return Array.AsReadOnly(refs.ToArray());
    }

    private static bool RepositoryRootIsCanonical(
        string repositoryRoot,
        string trustedRepositoryRoot)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(repositoryRoot).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                Path.GetFullPath(trustedRepositoryRoot)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool IsTrimmedVisible(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static bool IsBoundedRequiredText(string? value, int maximumLength)
        => IsTrimmedVisible(value) &&
           value!.Length <= maximumLength;

    private static bool AreSafeEvidenceRefs(
        IReadOnlyList<string>? evidenceRefs,
        int maximumCount)
        => evidenceRefs is not null &&
           evidenceRefs.Count > 0 &&
           evidenceRefs.Count <= maximumCount &&
           evidenceRefs.All(IsSafeEvidenceRef);

    private static bool IsSafeEvidenceRef(string? value)
    {
        if (!IsBoundedRequiredText(
                value,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumEvidenceRefLength) ||
            value!.StartsWith("/", StringComparison.Ordinal) ||
            value.StartsWith("\\", StringComparison.Ordinal) ||
            value.Contains('\\', StringComparison.Ordinal) ||
            value.Contains("/mnt/", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
            ContainsDriveRoot(value))
        {
            return false;
        }

        return true;
    }

    private static bool RoutingRunReceiptIsSafe(
        CodeDocumentationTrainingRunResult routingRun)
    {
        var receipt = routingRun.RepositoryOwnedDpoReceipt;
        if (receipt is null ||
            !IsSafeRepositoryRelativePath(
                receipt.RepositoryRelativeManifestPath) ||
            !IsHexSha256(receipt.ManifestSha256) ||
            !IsHexSha256(receipt.RequestIdentitySha256) ||
            receipt.PairCount <= 0 ||
            receipt.PairCount >
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .MaximumRepositoryOwnedDpoReceiptPairCount ||
            receipt.OrderedPairs is null ||
            receipt.OrderedPairs.Length != receipt.PairCount)
        {
            return false;
        }

        return receipt.OrderedPairs.All(static pair =>
            IsBoundedRequiredText(
                pair.PairId,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) &&
            IsSafeRepositoryRelativePath(
                pair.RepositoryRelativePairFilePath) &&
            IsHexSha256(pair.PairFileSha256));
    }

    private static bool DatasetMatchesServerCurrentManifest(
        RepositoryOwnedDpoDatasetAddress datasetAddress,
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt? routingReceipt,
        RepositoryOwnedDpoDatasetManifestIdentity? serverManifest)
        => routingReceipt is not null &&
           serverManifest is not null &&
           StringComparer.Ordinal.Equals(
               datasetAddress.ManifestRelativePath,
               serverManifest.RepositoryRelativeManifestPath) &&
           StringComparer.Ordinal.Equals(
               routingReceipt.RepositoryRelativeManifestPath,
               serverManifest.RepositoryRelativeManifestPath) &&
           StringComparer.Ordinal.Equals(
               routingReceipt.ManifestSha256,
               serverManifest.ManifestSha256);

    private static OperationResult<bool> InvalidRequest()
        => OperationResult<bool>.Fail(
            "CodeDocumentation repository-owned DPO training request is invalid.",
            InvalidRequestErrorCode);

    private static bool IsHexSha256(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f' or
                   >= 'A' and <= 'F');

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (!IsTrimmedVisible(path) ||
            path!.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("\\", StringComparison.Ordinal) ||
            path.Contains('\\', StringComparison.Ordinal) ||
            path.Contains(':', StringComparison.Ordinal) ||
            path.Contains("//", StringComparison.Ordinal) ||
            Path.IsPathFullyQualified(path))
        {
            return false;
        }

        var segments = path.Split('/');
        return segments.Length > 0 &&
               segments.All(static segment =>
                   segment.Length > 0 &&
                   segment is not "." and not "..");
    }

    private static bool ContainsDriveRoot(string value)
    {
        for (var index = 0; index + 2 < value.Length; index++)
        {
            if (char.IsAsciiLetter(value[index]) &&
                value[index + 1] == ':' &&
                value[index + 2] is '\\' or '/')
            {
                return true;
            }
        }

        return false;
    }

    private static IResult Error<T>(
        int statusCode,
        OperationResult<T> result)
        => Results.Json(
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse.Failure(
                result.ErrorCode,
                result.ErrorMessage),
            statusCode: statusCode);
}
