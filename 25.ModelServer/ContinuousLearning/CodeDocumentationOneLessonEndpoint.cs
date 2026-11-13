// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using AgentInfrastructure.Logical;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TrainableRouter.Training;

/// <summary>
/// Resolves one opaque selector against the startup-owned grant catalog and
/// atomically claims or reopens its durable objective before the CLI accesses
/// any input path or creates a run directory.
/// </summary>
internal static class CodeDocumentationOneLessonEndpoint
{
    internal static IEndpointConventionBuilder MapCodeDocumentationOneLessonPreflight(
        this IEndpointRouteBuilder endpoints,
        string trustedRepositoryRoot)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(trustedRepositoryRoot);

        return endpoints.MapPost(
            CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
            (
                CodeDocumentationOneLessonPreflightRequestV1? request,
                IServiceProvider services,
                CancellationToken cancellationToken) =>
                ExecuteAsync(
                    request,
                    services.GetRequiredService<
                        ContinuousLearningModelServerStartup>()
                        .OneLessonGrantCatalog,
                    trustedRepositoryRoot,
                    services,
                    cancellationToken));
    }

    internal static IResult Execute(
        CodeDocumentationOneLessonPreflightRequestV1? request,
        CodeDocumentationOneLessonGrantCatalog catalog,
        string trustedRepositoryRoot)
    {
        if (request?.Selector is null)
        {
            return Failure(
                "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_REQUEST_INVALID",
                "An objective ID and opaque grant selector are required.",
                StatusCodes.Status400BadRequest);
        }

        var grantResult = catalog.Resolve(request.Selector);
        if (!grantResult.IsSuccess || grantResult.Value is null)
        {
            var status = StringComparer.Ordinal.Equals(
                    grantResult.ErrorCode,
                    CodeDocumentationOneLessonGrantCatalog.GrantUnavailableErrorCode)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status503ServiceUnavailable;
            return Failure(
                grantResult.ErrorCode,
                grantResult.ErrorMessage,
                status);
        }

        var modelRoot = RepositoryOwnedTrainingArtifactLayout
            .ResolveModelArtifactRoot(trustedRepositoryRoot);
        if (!modelRoot.IsSuccess || modelRoot.Value is null)
        {
            return Failure(
                "CODE_DOCUMENTATION_ONE_LESSON_PREFLIGHT_UNAVAILABLE",
                "The trusted repository model-artifact root is unavailable.",
                StatusCodes.Status503ServiceUnavailable);
        }

        var grant = grantResult.Value;
        var binding = new CodeDocumentationOneLessonSessionBinding(
            grant.ObjectiveId,
            grant.GrantId,
            grant.GrantVersion,
            grant.TrainingRunId,
            grant.CandidateSha256,
            grant.ParentManifestSha256,
            grant.ParentWeightsSha256,
            grant.ConfigurationSha256,
            grant.ExamConditionSha256)
        {
            FormatVersion = grant.FormatVersion,
            DescriptorsV2 = grant.DescriptorsV2,
        };
        var opened = CodeDocumentationOneLessonSession.OpenOrCreate(
            modelRoot.Value,
            binding,
            catalog.MaxTeacherCalls);
        if (!opened.IsSuccess || opened.Value is null)
        {
            var conflict = StringComparer.Ordinal.Equals(
                opened.ErrorCode,
                CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode);
            return Failure(
                conflict
                    ? "OBJECTIVE_BINDING_CONFLICT"
                    : opened.ErrorCode,
                opened.ErrorMessage,
                conflict
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status503ServiceUnavailable);
        }

        using var objective = opened.Value;
        var snapshot = objective.Session.Snapshot;

        if (CodeDocumentationOneLessonSession.HasUnresolvedEffects(snapshot.Effects) ||
            (snapshot.TerminalOutcome is null && snapshot.Revision != 0))
        {
            return Failure(
                "OBJECTIVE_RECOVERY_REQUIRED",
                "The owned objective has incomplete or uncertain effects and cannot be replayed.",
                StatusCodes.Status409Conflict);
        }

        if (snapshot.TerminalOutcome is not null)
        {
            var bindingSha256 = CodeDocumentationOneLessonSession
                .ComputeBindingSha256(snapshot.Binding);
            var readback = objective.Session.ReadCanonicalOutcome(
                request.Selector,
                bindingSha256,
                snapshot.TerminalOutcome.OutcomeId,
                snapshot.TerminalOutcomeSha256);
            if (!readback.IsSuccess || readback.Value is null)
            {
                var bindingConflict = StringComparer.Ordinal.Equals(
                    readback.ErrorCode,
                    CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode);
                var terminalConflict = StringComparer.Ordinal.Equals(
                    readback.ErrorCode,
                    CodeDocumentationOneLessonSession.TerminalConflictErrorCode);
                var publicCode = bindingConflict
                    ? "OBJECTIVE_BINDING_CONFLICT"
                    : terminalConflict
                        ? "OBJECTIVE_TERMINAL_CONFLICT"
                        : "OBJECTIVE_RECOVERY_REQUIRED";
                return Failure(
                    publicCode,
                    bindingConflict
                        ? "The durable objective binding differs from the authorized selector."
                        : terminalConflict
                            ? "A different terminal outcome is already durable for this objective."
                            : "The durable terminal outcome is incomplete or inconsistent; recovery is required.",
                    StatusCodes.Status409Conflict);
            }

            return Results.Json(
                new CodeDocumentationOneLessonPreflightResponseV1(
                    true,
                    string.Empty,
                    string.Empty,
                    "TerminalReplay",
                    snapshot.Binding.ObjectiveId,
                    snapshot.Binding.TrainingRunId,
                    bindingSha256,
                    snapshot.SessionId,
                    readback.Value.Revision,
                    readback.Value.CanonicalOutcome,
                    readback.Value.OutcomeSha256),
                statusCode: StatusCodes.Status200OK);
        }

        var response = new CodeDocumentationOneLessonPreflightResponseV1(
            true,
            string.Empty,
            string.Empty,
            objective.Created ? "Fresh" : "Reopened",
            snapshot.Binding.ObjectiveId,
            snapshot.Binding.TrainingRunId,
            CodeDocumentationOneLessonSession.ComputeBindingSha256(
                snapshot.Binding),
            snapshot.SessionId,
            snapshot.Revision);
        return Results.Json(response, statusCode: StatusCodes.Status200OK);
    }

    private static async Task<IResult> ExecuteAsync(
        CodeDocumentationOneLessonPreflightRequestV1? request,
        CodeDocumentationOneLessonGrantCatalog catalog,
        string trustedRepositoryRoot,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        if (request?.Selector is null) return Execute(request, catalog, trustedRepositoryRoot);
        var resolved = catalog.Resolve(request.Selector);
        if (!resolved.IsSuccess || resolved.Value is null || resolved.Value.FormatVersion != 2)
            return Execute(request, catalog, trustedRepositoryRoot);
        var grant = resolved.Value;
        var binding = new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId, grant.GrantId,
            grant.GrantVersion, grant.TrainingRunId, grant.CandidateSha256, grant.ParentManifestSha256,
            grant.ParentWeightsSha256, grant.ConfigurationSha256, grant.ExamConditionSha256)
            { FormatVersion = 2, DescriptorsV2 = grant.DescriptorsV2 };
        if (grant.ProfileSelection is null || grant.CheckpointSelection is null || grant.SourceSelection is null ||
            grant.CorpusSelection is null || grant.EvaluationPlanSelection is null || grant.EvaluatorSelection is null)
            return Execute(request, catalog, trustedRepositoryRoot);
        cancellationToken.ThrowIfCancellationRequested();
        var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(trustedRepositoryRoot);
        if (!root.IsSuccess || root.Value is null)
            return Failure(root.ErrorCode, root.ErrorMessage, StatusCodes.Status503ServiceUnavailable);
        var opened = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root.Value, binding,
            catalog.MaxTeacherCalls, grant);
        if (!opened.IsSuccess || opened.Value is null)
            return Failure(opened.ErrorCode == CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode
                    ? "OBJECTIVE_BINDING_CONFLICT" : opened.ErrorCode, opened.ErrorMessage, StatusCodes.Status409Conflict);
        using var objective = opened.Value;
        var session = objective.Session;
        var bindingHash = CodeDocumentationOneLessonSession.ComputeBindingSha256(binding);
        IResult Terminal(string disposition, System.Text.Json.JsonElement outcome, string sha, long revision)
        {
            var response = new CodeDocumentationOneLessonPreflightResponseV1(true, "", "", disposition,
                grant.ObjectiveId, grant.TrainingRunId, bindingHash, session.Snapshot.SessionId,
                revision, outcome, sha);
            // Preserve the validated V2 terminal bytes inside the existing camelCase envelope:
            // Results.Json would re-escape legal Unicode and invalidate the canonical outcome.
            var envelope = System.Text.Json.JsonSerializer.SerializeToElement(response,
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            return Results.Bytes(CodeDocumentationOneLessonV2Wire.CanonicalBytes(envelope),
                "application/json; charset=utf-8");
        }
        // Trusted history is checked before any retained artifact, evaluator client, corpus or parent open.
        if (session.EarlyOutcome is { } replay)
            return Terminal("TerminalReplay", replay.CanonicalOutcome, replay.OutcomeSha256, replay.Revision);

        // Saved ORIGINAL identity is checked before constructing any evaluator client.
        if (session.HasSavedSelectionBinding) // Fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
        { // Begin fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
            var originalIdentity=await session.InspectExistingOrdinarySelectionOriginalAsync( // Fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
                catalog,request.Selector,trustedRepositoryRoot,cancellationToken).ConfigureAwait(false); // Fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
            if(!originalIdentity.IsSuccess) // Fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
                return Failure(originalIdentity.ErrorCode,originalIdentity.ErrorMessage,StatusCodes.Status409Conflict); // Fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.
        } // Finish fail saved ORIGINAL replay before creating an evaluator client or opening another baseline.

        var startup = services.GetRequiredService<ContinuousLearningModelServerStartup>();
        var fileAccess = services.GetService<ServerOwnedContinuousLearningFileAccess>();
        if (fileAccess is null)
            return Failure(CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode,
                "The retained server-owned training context is unavailable.", StatusCodes.Status503ServiceUnavailable);
        var parent = grant.CheckpointSelection.Parent;
        var prepared = startup.PrepareRepositoryOwnedTrainingExecutionContext(parent.DomainSlug, fileAccess, cancellationToken);
        if (!prepared.IsSuccess || prepared.Value is null)
            return Failure(prepared.ErrorCode, prepared.ErrorMessage, StatusCodes.Status503ServiceUnavailable);
        var start = prepared.Value.RepositoryStartRequest;
        if (!StringComparer.OrdinalIgnoreCase.Equals(System.IO.Path.GetFullPath(start.RepositoryRoot),
                System.IO.Path.GetFullPath(trustedRepositoryRoot)) || start.DesignPatternSlug != parent.DesignPatternSlug ||
            start.DomainSlug != parent.DomainSlug || start.ModelIdSlug != parent.ModelIdSlug || start.ModelVersion != parent.ModelVersion)
            return Failure("CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_START_MISMATCH",
                "The protected selected parent and current server start differ.", StatusCodes.Status409Conflict);
        var seeds = CodeDocumentationDpoSeedAuthorityResolver.Resolve(start.DomainSlug, prepared.Value.RootInitializationSeed);
        if (!seeds.IsSuccess || seeds.Value is null)
            return Failure(seeds.ErrorCode, seeds.ErrorMessage, StatusCodes.Status409Conflict);
        var profiles = CodeDocumentationSavedModelAssessmentProfileResolver.ResolveFromEnvironment();
        if (!profiles.IsSuccess || profiles.Value is null || !profiles.Value.TryGetValue(parent.DomainSlug, out var assessment) ||
            assessment.DesignPatternSlug != parent.DesignPatternSlug)
            return Failure(CodeDocumentationSavedModelAssessmentEndpoint.ProfileMissingErrorCode,
                "The current server-owned assessment profile is unavailable.", StatusCodes.Status503ServiceUnavailable);
        var corpus = CodeDocumentationSavedModelAssessmentEndpoint.OpenConfiguredCorpus(start.RepositoryRoot, assessment);
        if (!corpus.IsSuccess || corpus.Value is null)
            return Failure(corpus.ErrorCode, corpus.ErrorMessage, StatusCodes.Status409Conflict);
        var selectedNoise = grant.EvaluatorSelection.Evaluator.NoisePolicy;
        var noise = new MeasurementNoisePolicyConfig(selectedNoise.RepeatCount,
            double.Parse(selectedNoise.StandardDeviationMultiplier, System.Globalization.CultureInfo.InvariantCulture));
        var selectedEvaluator = grant.EvaluatorSelection.OpenFresh(start.RepositoryRoot, assessment.GenerationSettings,
            startup.TeacherConfiguration, noise, grant.ConfigurationSha256, grant.ExamConditionSha256);
        if (!selectedEvaluator.IsSuccess)
            return Failure(selectedEvaluator.ErrorCode, selectedEvaluator.ErrorMessage, StatusCodes.Status409Conflict);
        var selectedParent = grant.CheckpointSelection.OpenFresh(start.RepositoryRoot, start.RuntimeCompatibility);
        if (!selectedParent.IsSuccess)
            return Failure(selectedParent.ErrorCode, selectedParent.ErrorMessage, StatusCodes.Status409Conflict);
        var selectedSource = grant.SourceSelection.OpenFresh(start.RepositoryRoot);
        if (!selectedSource.IsSuccess)
            return Failure(selectedSource.ErrorCode, selectedSource.ErrorMessage, StatusCodes.Status409Conflict);
        var batch = services.GetRequiredService<CodeDocumentationBatchFileRunner>();
        var material = batch.OpenProfileInputsFresh(catalog, request.Selector, root.Value);
        if (!material.IsSuccess || material.Value is null)
            return Failure(material.ErrorCode, material.ErrorMessage, StatusCodes.Status409Conflict);
        var factory = services.GetService<ICodeDocumentationSavedModelAssessmentEvaluationClientFactory>()
            ?? new DefaultCodeDocumentationSavedModelAssessmentEvaluationClientFactory();
        var client = factory.Create(startup.TeacherConfiguration);
        if (!client.IsSuccess || client.Value is null)
            return Failure(client.ErrorCode, client.ErrorMessage, StatusCodes.Status503ServiceUnavailable);
        var successPrepared = false;
        try
        {
            var parentRequest = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(start.RepositoryRoot,
                start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, prepared.Value.ModelConfig,
                start.RuntimeCompatibility, start.ColdStartPolicy, LoraConfig: prepared.Value.LoraConfig, AdapterSeedAuthority: seeds.Value);
            var quality = new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext(corpus.Value.Corpus,
                corpus.Value.Manifest, prepared.Value.Tokenizer, client.Value, startup.TeacherConfiguration,
                assessment.GenerationSettings, noise,
                CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration, DateTimeOffset.UtcNow,
                prepared.Value.EvidenceRefs.ToArray());
            var original = await session.ConsumeOrdinaryBaselineAsync(catalog, request.Selector,
                parentRequest, quality, cancellationToken).ConfigureAwait(false);
            if (!original.IsSuccess || original.Value is null)
                return Failure(original.ErrorCode, original.ErrorMessage, StatusCodes.Status409Conflict);
            var decision = session.ClassifyOrdinaryBaseline(original.Value, catalog, request.Selector, cancellationToken);
            if (decision.IsSuccess)
            { // Inspect the same held ORIGINAL before any future ordinary lesson continuation.
                var exclusion = session.InspectOrdinaryOriginalTeachingExclusion( // Reuse the receiving owner; this evidence grants no material or effect authority.
                    original.Value, catalog, request.Selector, cancellationToken); // Retain the same genuine baseline, catalog, selector and cancellation contract.
                if (!exclusion.IsSuccess || exclusion.Value is null) // Never turn absent, stale or overlapping material into successful preflight.
                    return Failure(exclusion.ErrorCode, exclusion.ErrorMessage, // Preserve the existing bounded owner error without disclosing the witness or graph.
                        exclusion.ErrorCode == "ONE_LESSON_TEACHING_IDENTITIES_UNAVAILABLE" // A missing declaration is an unavailable prerequisite.
                            ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status409Conflict); // Other genuine inspection failures remain bounded conflicts.
                if (!session.HasSavedSelectionBinding && startup.UsesProtectedTeachingDisclosureContractV3) // Run protected disclosure only for fresh V3 selection; saved replay keeps its existing path.
                { // Prepare the complete immutable request before resolving any decision provider.
                    var inspected = await session.CaptureStartupApprovedNativeSelectionAsync(original.Value, startup, request.Selector, cancellationToken).ConfigureAwait(false); // Retain genuine protected owners and the existing CAS through durable preparation and every final.
                    if (!inspected.IsSuccess || inspected.Value is null) return Failure(inspected.ErrorCode, inspected.ErrorMessage, // Return only the bounded inspection refusal; no raw context leaves the HTTP boundary.
                        inspected.ErrorCode == CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status409Conflict); // Map missing protected approval to503 and genuine owner/graph refusal to409.
                    return Failure("CODE_DOCUMENTATION_ONE_LESSON_NATIVE_HOST_READ_BOUNDARY_UNAVAILABLE", "The teaching-selection request is prepared; the native host read boundary is unavailable.", StatusCodes.Status503ServiceUnavailable); // Only accepted full prepare/replay reaches this next boundary.
                } // End the V3 branch before the ordinary Synthetic provider lookup.
                var provider=session.HasSavedSelectionBinding ? null : services.GetService<CodeDocumentationOneLessonSelectionDecisionProvider>(); // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                if(!session.HasSavedSelectionBinding && provider is null) // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                    return Failure("CODE_DOCUMENTATION_ONE_LESSON_SELECTION_OPERATOR_UNAVAILABLE", // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                        "The server-owned selection operator is unavailable.",StatusCodes.Status503ServiceUnavailable); // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                if (!session.HasSavedSelectionBinding) // Ordinary absent-history entry consumes the full eligible set through the shared Session owner.
                { // Provider-unavailable refusal above still precedes unsupported lesson continuation.
                    var issuedSet = services.GetService<IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>>(); // Exactly one collection lookup; no single-admission lookup in fresh branch.
                    var detachedSet = issuedSet is null ? null : Array.AsReadOnly(issuedSet.ToArray()); // Host list mutation cannot change the consumed collection.
                    var full = await session.BindFreshOrdinaryFullMaterialSelectionAsync(original.Value, catalog, request.Selector, detachedSet, provider, cancellationToken).ConfigureAwait(false); // Continuous CAS extends through staging, binder and all final checks.
                    if (!full.IsSuccess || full.Value is null) return Failure(full.ErrorCode, full.ErrorMessage, StatusCodes.Status409Conflict); // Matching Synthetic and incomplete/foreign sets stop at the exact lower full-material guard.
                    return Failure("CODE_DOCUMENTATION_ONE_LESSON_MATERIAL_CONTEXT_BOUND_CONTINUATION_UNAVAILABLE", // Storage consumption does not make the ordinary full lesson available.
                        "Selected teaching material context is bound; full lesson continuation is unavailable.", StatusCodes.Status503ServiceUnavailable); // Preserve honest continuation boundary.
                } // Saved metadata continues through its original single-admission owner below.
                var selected=session.HasSavedSelectionBinding // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                    ? session.TryReadOrdinarySelectionBinding(original.Value,catalog,request.Selector,cancellationToken) // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                    : await session.BindOrdinarySelectionDecisionAsync(original.Value,catalog,request.Selector, // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                        exclusion.Value,provider!,cancellationToken).ConfigureAwait(false); // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                if(!selected.IsSuccess || selected.Value is null) // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                    return Failure(selected.ErrorCode,selected.ErrorMessage,StatusCodes.Status409Conflict); // Resolve the host issuer only for fresh selection; saved replay skips resolution and retains truthful unavailable continuation.
                var materialAdmission = services.GetService<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>(); // Optional concrete eligibility is absent from default Program composition.
                if (materialAdmission is null) return Failure("CODE_DOCUMENTATION_ONE_LESSON_SELECTION_BOUND_CONTINUATION_UNAVAILABLE", // Preserve the existing truthful missing-admission continuation.
                    "The teaching selection is durably bound; lesson continuation is unavailable.",StatusCodes.Status503ServiceUnavailable); // Saved descriptive evidence never grants material rights.
                var partial = session.PrepareOrdinarySelectedTeachingMaterialContext(original.Value, catalog, request.Selector, materialAdmission, cancellationToken); // Reach the actual authoring payload recipient under the same held Session.
                if (!partial.IsSuccess || partial.Value is null) return Failure(partial.ErrorCode, partial.ErrorMessage, StatusCodes.Status409Conflict); // Preserve exact opaque-admission and real owner refusals without raw material egress.
                return Failure("CODE_DOCUMENTATION_ONE_LESSON_MATERIAL_CONTEXT_BOUND_CONTINUATION_UNAVAILABLE", // A real partial task/reference recipient still cannot claim full lesson readiness.
                    "Selected teaching material context is bound; full lesson continuation is unavailable.", StatusCodes.Status503ServiceUnavailable); // No task/reference, receipt internals or synthetic admission appears in HTTP.
            } // A clean finite exclusion still does not admit rights, curriculum, teacher context or a lesson.
            if (decision.ErrorCode != CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineAlreadySufficientErrorCode)
                return Failure(decision.ErrorCode, decision.ErrorMessage,
                    decision.ErrorCode == CodeDocumentationOneLessonSession.UsableDiagnosisMissingErrorCode
                        ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status409Conflict);
            var completed = session.CompleteEarlyBaselineSufficient(original.Value, batch, material.Value,
                catalog, request.Selector, root.Value, start.RuntimeCompatibility, cancellationToken);
            if (!completed.IsSuccess || completed.Value is null)
                return Failure(completed.ErrorCode, completed.ErrorMessage, StatusCodes.Status409Conflict);
            successPrepared = true;
            return Terminal("Completed", completed.Value.CanonicalOutcome, completed.Value.OutcomeSha256, completed.Value.Revision);
        }
        finally
        {
            try { if (client.Value is IDisposable disposable) disposable.Dispose(); }
            catch (Exception exception) when (!successPrepared && SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Same endpoint cleanup contract: preserve the primary refusal
                // or exception. Disposal after prepared success remains observable.
            }
        }
    }

    private static IResult Failure(
        string errorCode,
        string errorMessage,
        int statusCode)
        => Results.Json(
            CodeDocumentationOneLessonPreflightResponseV1.Failure(
                errorCode,
                errorMessage),
            statusCode: statusCode);
}
