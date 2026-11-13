// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training; // One update, durable scientific hold, then publication of the same saved Trial.

using System.Text; // Validate exact retained UTF8 without normalization.
using AgentInfrastructure; // Keep effect refusals in the business result boundary.
using ContinuousLearning; // Reuse existing checkpoint, graph and custody owners.

public static partial class CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator // Extend the existing lifecycle rather than introducing another trainer.
{ // No assessment, optimizer or teacher call exists in the publish-only branch.
    public const string ScientificHoldRequired = "CODE_DOCUMENTATION_SCIENTIFIC_HOLD_REQUIRED"; // Ordinary/direct resume cannot bypass a held attempt.
    private const string HeldSignal = "CODE_DOCUMENTATION_SCIENTIFIC_TRIAL_HELD"; // Internal projection signal is not a publication receipt.
    internal sealed class ScientificInvocation(CodeDocumentationScientificTrainingPolicy policy, CodeDocumentationPrivateEvaluationStore store, string? heldSha, string? verdictSha, bool resumeBaseline) // Constructed only by these explicit server-owned entries.
    { // Keep request wire booleans and fabricated capabilities out of this authority.
        internal readonly CodeDocumentationScientificTrainingPolicy Policy = policy; // Frozen before C0.
        internal readonly CodeDocumentationPrivateEvaluationStore Store = store; // Server-derived private destination.
        internal readonly string? HeldSha = heldSha, VerdictSha = verdictSha; // Only trusted startup can supply publish-only expectations.
        internal readonly bool ResumeBaseline = resumeBaseline; // Only the explicit recovery operation may reopen pre-executor C0.
        internal CodeDocumentationRetainedBaselineRecovery? Recovery; // Populated after exact intent and owner eligibility checks.
        internal bool PublishOnly => HeldSha is not null; // This private branch never authorizes a new optimizer step.
        internal CodeDocumentationScientificTrainingResponse? Response; // Populated only after durable readback or actual publication.
    } // No public factory creates a native training capability.
    private sealed record ScienceIntent(string Schema, string PolicySha256, string SourceBindingSha256); // Durable before any scientific generation or optimizer effect.
    private sealed record HeldScienceTrial(string Schema, string PolicySha256, string SourceBindingSha256, string ParentManifestSha256, string TrialWeightsSha256, CodeDocumentationServerOwnedDpoTrainingEvidence Training, CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult Detailed); // Complete graphs preserve native negative outcomes too.

    public static Task<OperationResult<CodeDocumentationScientificTrainingResponse>> TrainAndHoldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationScientificTrainingPolicy policy, CancellationToken cancellationToken = default) // Explicit first-RP01 route with a distinct pending result.
        => ExecuteScientificAsync(request, policy, null, null, false, cancellationToken); // Existing core remains the sole update/calibration owner.
    public static Task<OperationResult<CodeDocumentationScientificTrainingResponse>> PublishHeldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationScientificTrainingPolicy policy, string heldBundleSha256, string independentVerdictSha256, CancellationToken cancellationToken = default) // Trusted server configuration pins real artifacts, never client acceptance booleans.
        => ExecuteScientificAsync(request, policy, heldBundleSha256, independentVerdictSha256, false, cancellationToken); // Reuse existing sealed-attempt validators without re-evaluation.
    public static Task<OperationResult<CodeDocumentationScientificTrainingResponse>> ResumeBaselineAndHoldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationScientificTrainingPolicy policy, CancellationToken cancellationToken = default) // Continue only the same admitted intent before any canonical attempt exists.
        => ExecuteScientificAsync(request, policy, null, null, true, cancellationToken); // Reuse the sole ordinary calibration/executor/hold spine.
    private static async Task<OperationResult<CodeDocumentationScientificTrainingResponse>> ExecuteScientificAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationScientificTrainingPolicy policy, string? heldSha, string? verdictSha, bool resumeBaseline, CancellationToken token) // Capture a single explicit invocation.
    { // Refuse malformed authority before START or model work.
        if (request?.StartRequest is null || policy is null || (heldSha is null) != (verdictSha is null) || heldSha is not null && (!CodeDocumentationScientificTrainingPolicy.Hex(heldSha) || !CodeDocumentationScientificTrainingPolicy.Hex(verdictSha))) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationScientificTrainingResponse>(); // No missing half-grant can train or promote.
        var address = CreateTrainingAttemptAddress(request.StartRequest, request.TrainingRunId); // Reuse canonical run/root identity validation.
        if (!address.IsSuccess || address.Value is null) return address.MapError<CodeDocumentationScientificTrainingResponse>(); // Preserve actual layout refusal.
        var store = CodeDocumentationPrivateEvaluationStore.Create(address.Value); // Creation has no file/model effects.
        if (!store.IsSuccess || store.Value is null) return store.MapError<CodeDocumentationScientificTrainingResponse>(); // No client output path is accepted.
        var invocation = new ScientificInvocation(policy, store.Value, heldSha, verdictSha, resumeBaseline); // Attach actual server authority only here.
        try // Malformed retained data remains a typed refusal without reflecting protected values.
        {
            var result = await TrainAndPublishCoreAsync(request with { Scientific = invocation }, null, null, token).ConfigureAwait(false); // Exactly one existing core invocation.
            if (invocation.Response is not null && (result.IsSuccess || result.ErrorCode == HeldSignal)) return OperationResult<CodeDocumentationScientificTrainingResponse>.Ok(invocation.Response); // A held response has no fake Published receipt.
            return result.MapError<CodeDocumentationScientificTrainingResponse>(); // Preserve unknown effects/recovery/cancellation failures.
        }
        catch (OperationCanceledException) { throw; } // Cancellation preserves durable intent/Trial and never implies zero effects.
        catch (Exception exception) when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationScientificTrainingResponse>(); } // Fail closed on malformed UTF8/graphs/storage without logging closed text.
    } // No automatic retry is performed.
    private static OperationResult<bool> AdmitScientificRoute(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CancellationToken token) // Common entry guard covers legacy and direct coordinator resume.
    { // Existence causes refusal; only hash-verified evidence can grant publication.
        var marker = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition(request.RoutingRun, request.RoutingDisposition, request.StartRequest.DomainSlug); // Common direct API rejects unknown/contradictory markers as client/server wire already does.
        if (!marker.IsSuccess) return marker; // A marker never grants permission or bypasses source validation.
        var address = CreateTrainingAttemptAddress(request.StartRequest, request.TrainingRunId); // Use exactly the same run identity as the optimizer owner.
        if (!address.IsSuccess || address.Value is null) return address.MapError<bool>(); // Unsafe addresses cannot bypass the marker.
        var store = CodeDocumentationPrivateEvaluationStore.Create(address.Value); // Resolve outside canonical checkpoint discovery.
        if (!store.IsSuccess || store.Value is null) return store.MapError<bool>(); // Fail closed on unknown storage state.
        var intent = store.Value.HasHoldIntent(); // Even a partial or malformed intent blocks the ordinary route.
        if (!intent.IsSuccess) return intent; // Do not guess absence on an I/O/custody error.
        if (request.Scientific is not { } science) // Preserve normal routing behavior except explicitly raw scientific input and held run IDs.
            return intent.Value || request.RoutingDisposition == CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed ? OperationResult<bool>.Fail("This raw or held attempt requires the explicit scientific hold/publish route.", ScientificHoldRequired) : OperationResult<bool>.Ok(true); // A legacy raw endpoint cannot accidentally publish RP01.
        if (request.QualityGate is null || request.DpoStepCount != 1 || request.RoutingDisposition != CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed || !CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition(request.RoutingRun, request.RoutingDisposition, request.StartRequest.DomainSlug).IsSuccess || science.Policy.RepositoryRoot != request.StartRequest.RepositoryRoot || (science.PublishOnly || science.ResumeBaseline) != intent.Value) return CodeDocumentationScientificTrainingPolicy.Refuse<bool>(); // Initial hold requires freshness and honest raw observations; publish-only requires existing intent, never a new step.
        return science.Policy.ValidateCurrent(token); // Frozen policy/supporting sources are required before START.
    } // Every ordinary/direct core entry runs this guard.
    private static string ScienceSourceBinding(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedDpoDatasetManifest dataset, RepositoryOwnedDpoDatasetManifestPair pair) // Hash current admitted source/conditions without copying closed corpus bodies.
        => CodeDocumentationScientificTrainingPolicy.ValueHash(new { request.TrainingRunId, request.DpoStepCount, request.StartRequest.DomainSlug, request.StartRequest.DesignPatternSlug, request.StartRequest.ModelIdSlug, request.StartRequest.ModelVersion, request.StartRequest.ModelConfig, request.StartRequest.RuntimeCompatibility, request.LoraConfig, request.AdapterSeedAuthority, Parent = parent, Dataset = dataset, Pair = pair, request.QualityGate!.CorpusManifest, request.QualityGate.GenerationSettings, request.QualityGate.NoisePolicyConfig, request.QualityGate.EvaluatorConfiguration, HeldOut = request.QualityGate.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).Select(e => new { e.EntryId, Input = CodeDocumentationScientificTrainingPolicy.TextHash(e.OwnerInput) }).ToArray() }); // Observation timestamps are not scientific conditions; restarts retain the actual same sources.
    private static OperationResult<ScienceIntent> CurrentScienceIntent(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedDpoDatasetManifest dataset, RepositoryOwnedDpoDatasetManifestPair pair, CancellationToken token) // Bind the policy to actual admitted data and model before effects or promotion.
    { // Parent, dataset, pair and source families are never inferred from a raw marker.
        var science = request.Scientific!; var policy = science.Policy.Document; // Only explicit server authority reaches this helper.
        var current = science.Policy.ValidateCurrent(token); // Recheck all preregistration/trust inputs.
        var heldOut = request.QualityGate!.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray(); // Same ordering as the existing assessment owner.
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(request.QualityGate.Corpus, request.QualityGate.GenerationSettings, request.QualityGate.EvaluatorConfiguration, request.QualityGate.NoisePolicyConfig); // Reuse existing repeat/count policy.
        if (!current.IsSuccess || !plan.IsSuccess || plan.Value is null || plan.Value.BaselinePlan.RepeatCount != policy.RepeatCount || heldOut.Length != policy.FamilyCount || policy.TrainingRunId != request.TrainingRunId || policy.ParentManifestSha256 != parent.ManifestSha256 || policy.DatasetManifestSha256 != dataset.ManifestSha256 || policy.PairId != pair.PairId || parent.ParentCheckpoint is not null || parent.Step != 1 || heldOut.Any(e => !policy.Cases.Any(c => c.CaseId == e.EntryId && c.OwnerInputSha256 == CodeDocumentationScientificTrainingPolicy.TextHash(e.OwnerInput)))) return CodeDocumentationScientificTrainingPolicy.Refuse<ScienceIntent>(); // First root update only, with exact preregistered family inputs.
        return OperationResult<ScienceIntent>.Ok(new("first-rp01-science-intent-v1", science.Policy.Sha256, ScienceSourceBinding(request, parent, dataset, pair))); // The intent binds complete actual source metadata before C0.
    } // No baseline or optimizer action occurs here.
    private static OperationResult<bool> BeginScientificHold(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedDpoDatasetManifest dataset, RepositoryOwnedDpoDatasetManifestPair pair, CancellationToken token) // Make cancellation/crash safe before expensive stages.
    { // Create-only intent remains even if all subsequent effects stop.
        if (request.Scientific!.PublishOnly) return CodeDocumentationScientificTrainingPolicy.Refuse<bool>(); // A publish-only invocation can never enter fresh calibration/training.
        var intent = CurrentScienceIntent(request, parent, dataset, pair, token); // Bind actual admitted parent and dataset.
        if (!intent.IsSuccess || intent.Value is null) return intent.MapError<bool>(); // Missing policy cannot start C0.
        if (request.Scientific.ResumeBaseline) // Recovery must reopen the same immutable pre-effect intent, never rewrite it.
        { // The current source graph remains unchanged by administrative code upgrades.
            var opened = request.Scientific.Store.OpenReference("intent.json", CodeDocumentationScientificTrainingPolicy.ValueHash(intent.Value), token); // Compare exact original intent bytes/hash to complete recomputed conditions.
            if (!opened.IsSuccess || opened.Value is null) return opened.MapError<bool>(); // Policy or source drift refuses before measurement effects.
            var existing = request.Scientific.Store.Read<ScienceIntent>(opened.Value, token); // Strictly validate the original schema and values.
            if (!existing.IsSuccess || existing.Value != intent.Value) return CodeDocumentationScientificTrainingPolicy.Refuse<bool>(); // Malformed or changed intent is never new work.
            var address = CreateTrainingAttemptAddress(request.StartRequest, request.TrainingRunId); // Reuse the canonical executor address owner.
            if (!address.IsSuccess || address.Value is null) return address.MapError<bool>(); // No caller path can select another attempt.
            var eligible = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.AdmitPartialBaselineRecovery(address.Value, pair.PairId, pair.PairFileSha256, token); // SHORT pair-lock check; its handle is disposed before return.
            if (!eligible.IsSuccess || eligible.Value is null) return eligible.MapError<bool>(); // Any canonical attempt, claim, journal or conflicting sibling refuses.
            var entries = request.QualityGate!.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray(); // Preserve the original case ordering.
            var settings = CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(request.QualityGate.GenerationSettings, entries); // Same existing producer serialization, not a new settings hash.
            var passport = CreateAcceptedPassport(parent, request.StartRequest.MaximumCheckpointBytes); // Reuse exact B0 identity.
            var slots = Enumerable.Range(1, request.Scientific.Policy.Document.RepeatCount).SelectMany(repeat => entries.Select(entry => new CodeDocumentationRawAnswerBinding(CodeDocumentationSavedModelAssessmentCalibrationService.BuildRepeatAssessmentRunId(BuildQualityAssessmentRunId(request.TrainingRunId, "baseline"), repeat), entry.EntryId, CodeDocumentationScientificTrainingPolicy.TextHash(entry.OwnerInput), settings, request.QualityGate.CorpusManifest.ManifestSha256, request.QualityGate.Corpus.CorpusSha256, passport))).ToArray(); // Fix all sixty original run/case/model/input/settings/corpus slots before inspecting any raw.
            var recovery = request.Scientific.Store.AdmitRetainedBaseline(eligible.Value, slots, token); // Validate the ENTIRE inventory before generation/evaluator effects.
            if (!recovery.IsSuccess || recovery.Value is null) return recovery.MapError<bool>(); // Orphan, extra arm, malformed result or changed bytes refuse rather than regenerate.
            request.Scientific.Recovery = recovery.Value; // Typed admission survives request copies through calibration repeats.
            return OperationResult<bool>.Ok(true); // Intent and every retained raw remain unchanged.
        } // No post-update general recovery route is added.
        var saved = request.Scientific.Store.Put("intent.json", intent.Value, token, requireNew: true); // Exclusive durable intent admits one fresh invocation, even under concurrent requests.
        return saved.IsSuccess ? OperationResult<bool>.Ok(true) : saved.MapError<bool>(); // Failed readback forbids generation and optimizer effects.
    } // No deletion or automatic retry clears this reservation.
    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>> CompleteScientificHoldAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedDpoDatasetManifest dataset, RepositoryOwnedDpoDatasetManifestPair pair, CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline, CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial, CodeDocumentationSavedModelAssessmentModelPassport passport, CodeDocumentationServerOwnedDpoTrainingEvidence training, CancellationToken token) // Complete the sole measured Trial and retain both native outcomes.
    { // Existing assessment service remains the only measuring owner.
        var detail = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(request.QualityGate, baseline, trial.Model, passport, BuildQualityAssessmentRunId(request.TrainingRunId, "trial"), pair.PairId, token).ConfigureAwait(false); // Exactly the existing five Trial repeats, no extra C0 or model stage.
        if (!detail.IsSuccess || detail.Value is null) return detail.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Intent/Trial/any raw outputs remain for unknown or partial outcomes.
        var intent = CurrentScienceIntent(request, parent, dataset, pair, token); // Recheck sources after all measurements.
        if (!intent.IsSuccess || intent.Value is null) return intent.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Drift cannot produce a complete held bundle.
        var savedIntent = request.Scientific!.Store.Put("intent.json", intent.Value, token); // Existing intent must still match byte-for-byte.
        if (!savedIntent.IsSuccess) return savedIntent.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // A changed hold never becomes ordinary-resumable.
        var bundle = new HeldScienceTrial("first-rp01-held-trial-v1", request.Scientific.Policy.Sha256, intent.Value.SourceBindingSha256, parent.ManifestSha256, trial.WeightsSha256, training, detail.Value); // Full graphs and exact immutable Trial identity are retained.
        var raw = ValidateScienceAnswers(request, bundle, CreateAcceptedPassport(parent, request.StartRequest.MaximumCheckpointBytes), passport, token); // Prove every repeat/case has exact bound retained UTF8 before reporting held completion.
        if (!raw.IsSuccess) return raw.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Partial retention remains recovery-required, never published.
        var saved = request.Scientific.Store.Put("held-trial.json", bundle, token); // Native negative results are equally durable and never promoted.
        if (!saved.IsSuccess || saved.Value is null) return saved.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Complete means actual disk readback, not in-memory values.
        request.Scientific.Response = new("HELD_FOR_SCIENCE", request.TrainingRunId, saved.Value, detail.Value.Disposition.ToString(), null) { EvaluationAccounting = request.Scientific.Store.Accounting(request.Scientific.ResumeBaseline) }; // Pending result carries no canonical checkpoint claim.
        return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Fail("The complete Trial is held for independent frozen scientific acceptance.", HeldSignal); // Stop before the existing canonical Publish call.
    } // Canonical latest remains the parent.
    private static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> PublishScientificHeld(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationRepositoryOwnedModelCheckpointStart start, RepositoryOwnedDpoDatasetManifest dataset, RepositoryOwnedDpoDatasetManifestPair pair, CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline, CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial, CodeDocumentationSavedModelAssessmentModelPassport passport, CodeDocumentationServerOwnedDpoTrainingEvidence training, CancellationToken token) // Reuse the existing recovery prefix's fully validated saved Trial.
    { // No generation, calibration, evaluator, executor or pair replay is called below.
        var science = request.Scientific!; // Authority came from explicit server startup.
        if (!science.PublishOnly) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Repeating train-and-hold cannot re-evaluate an applied attempt.
        var intent = CurrentScienceIntent(request, start.ParentManifest!, dataset, pair, token); // Revalidate current parent, lesson, policy and source graph.
        if (!intent.IsSuccess || intent.Value is null) return intent.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Changed parent/source never promotes.
        var intentRef = science.Store.OpenReference("intent.json", CodeDocumentationScientificTrainingPolicy.ValueHash(intent.Value), token); // The exact pre-effect intent must remain present and unchanged.
        if (!intentRef.IsSuccess) return intentRef.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // No posthoc policy can replace the original.
        var heldRef = science.Store.OpenReference("held-trial.json", science.HeldSha!, token); // Trusted startup pins the complete immutable bundle.
        if (!heldRef.IsSuccess || heldRef.Value is null) return heldRef.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Missing/partial/changed bundle refuses without new measurement.
        var held = science.Store.Read<HeldScienceTrial>(heldRef.Value, token); // Complete graph reopening uses strict private schema/custody.
        if (!held.IsSuccess || held.Value is null) return held.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Unknown fields or drift cannot be accepted.
        var bundle = held.Value; // Retain exact actual saved preimage for all following comparisons.
        if (bundle.Schema != "first-rp01-held-trial-v1" || bundle.PolicySha256 != science.Policy.Sha256 || bundle.SourceBindingSha256 != intent.Value.SourceBindingSha256 || bundle.ParentManifestSha256 != start.ParentManifest!.ManifestSha256 || bundle.TrialWeightsSha256 != trial.WeightsSha256 || CodeDocumentationScientificTrainingPolicy.ValueHash(bundle.Detailed.Baseline) != CodeDocumentationScientificTrainingPolicy.ValueHash(baseline)) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Bind complete baseline journal and actual reopened weights, not caller claims.
        var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(request.QualityGate!, trial.Model, passport, BuildQualityAssessmentRunId(request.TrainingRunId, "trial")); // Metadata carrier only: does not invoke generation/evaluation.
        var compared = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.CompareCompletedTrialDetailed(exam, request.QualityGate!.NoisePolicyConfig, baseline, bundle.Detailed.Trial, pair.PairId); // Existing pure complete-graph/native-policy admission.
        if (!compared.IsSuccess || compared.Value is null || CodeDocumentationScientificTrainingPolicy.ValueHash(compared.Value) != CodeDocumentationScientificTrainingPolicy.ValueHash(bundle.Detailed) || compared.Value.Disposition != CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Native negative remains held even with a positive external claim.
        var raw = ValidateScienceAnswers(request, bundle, CreateAcceptedPassport(start.ParentManifest, request.StartRequest.MaximumCheckpointBytes), passport, token); // Reopen all exact raw files/bindings and compare every saved repeat hash.
        if (!raw.IsSuccess || raw.Value is null) return raw.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // No lossy preview substitutes for full raw answers.
        var verdictRef = science.Store.OpenReference("scientific-verdict.json", science.VerdictSha!, token); // Operator-pinned real distinct examiner artifact; no client-supplied result.
        if (!verdictRef.IsSuccess || verdictRef.Value is null) return verdictRef.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Missing/rejected verdict never publishes.
        var verdict = science.Store.Read<CodeDocumentationScientificVerdict>(verdictRef.Value, token); // Strict schema and duplicate rejection precede criteria checks.
        if (!verdict.IsSuccess || verdict.Value is null || !ValidScientificVerdict(science.Policy, verdict.Value, heldRef.Value.Sha256, bundle.SourceBindingSha256, trial.WeightsSha256, raw.Value)) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Integer gain and separate critical constraints remain frozen.
        var current = CurrentScienceIntent(request, start.ParentManifest, dataset, pair, token); // Recheck configured sources immediately before canonical publication.
        if (!current.IsSuccess || current.Value != intent.Value) return CodeDocumentationScientificTrainingPolicy.Refuse<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Post-review criteria/source drift cannot pass.
        var publicationStart = CreatePublicationStartFromReloadedTrial(start, trial); // Same exact saved Trial model from existing recovery owner, no optimizer call.
        var published = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.Publish(request.StartRequest, publicationStart, new(request.DpoDatasetAddress.ManifestRelativePath, dataset.ManifestSha256), request.CreatedAtUtc, request.ProvenanceRefs, token); // Existing parent/checkpoint/canonical publication enforcement remains authoritative.
        if (!published.IsSuccess || published.Value is null) return published.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Preserve actual publication failures.
        var evidence = new CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence(request.DpoDatasetAddress.ManifestRelativePath, dataset.ManifestSha256, 1, start.ParentManifest.Step, published.Value.Step, published.Value.RepositoryRelativeWeightsPath, published.Value.RepositoryRelativeManifestPath, training, published.Value, BuildEvidenceRefs(publicationStart, published.Value, compared.Value.Evidence), compared.Value.Evidence); // Report actual existing publication, never a manufactured pending receipt.
        science.Response = new("PUBLISHED", request.TrainingRunId, heldRef.Value, compared.Value.Disposition.ToString(), evidence); // Distinguish successful same-Trial promotion from held completion.
        return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Ok(evidence); // Only actual canonical Publish succeeds here.
    } // No independent-science stage is fabricated by this owner.
    private static OperationResult<Dictionary<string, (string[] Baseline, string[] Trial)>> ValidateScienceAnswers(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, HeldScienceTrial bundle, CodeDocumentationSavedModelAssessmentModelPassport baselinePassport, CodeDocumentationSavedModelAssessmentModelPassport trialPassport, CancellationToken token) // Bind all five raw outputs in both arms to the actual completed graphs.
    { // Protected text is read only for hash/char/UTF8 checks, never logged or trained on.
        var science = request.Scientific!; var policy = science.Policy.Document; // Use preregistered family identities.
        var result = policy.Cases.ToDictionary(c => c.CaseId, _ => (Baseline: new string[policy.RepeatCount], Trial: new string[policy.RepeatCount]), StringComparer.Ordinal); // Five repeats are supporting observations of twelve families.
        foreach (var arm in new[] { (Graph: bundle.Detailed.Baseline.Calibration, Passport: baselinePassport, Baseline: true), (Graph: bundle.Detailed.Trial, Passport: trialPassport, Baseline: false) }) // Validate both complete original and Trial outputs.
        { // Observation metadata is already checked by the existing complete-graph owner during promotion.
            if (arm.Graph.Calibration.Observations.Count != policy.RepeatCount) return CodeDocumentationScientificTrainingPolicy.Refuse<Dictionary<string, (string[], string[])>>(); // No missing repeat is inferred from a representative report.
            for (var repeat = 0; repeat < policy.RepeatCount; repeat++) // Retain deterministic repeat ordering for the examiner.
            { // Every case in each repeat must have one actual private artifact.
                var observation = arm.Graph.Calibration.Observations[repeat]; // Preserve the actual source run ID/settings/hash order.
                if (observation.CaseIds.Count != policy.FamilyCount || observation.StudentAnswerSha256.Count != policy.FamilyCount || observation.PrivateRawAnswers?.Count != policy.FamilyCount || observation.CaseIds.Distinct(StringComparer.Ordinal).Count() != policy.FamilyCount) return CodeDocumentationScientificTrainingPolicy.Refuse<Dictionary<string, (string[], string[])>>(); // Reject partial/duplicated capture.
                for (var index = 0; index < policy.FamilyCount; index++) // Check actual case-to-family binding rather than an unbound hash list.
                { // Raw text never leaves this private verification boundary.
                    var source = policy.Cases.SingleOrDefault(c => c.CaseId == observation.CaseIds[index]); // Exact preregistered case identity.
                    if (source is null) return CodeDocumentationScientificTrainingPolicy.Refuse<Dictionary<string, (string[], string[])>>(); // Unknown cases do not enlarge the family count.
                    var opened = science.Store.Read<CodeDocumentationRawAnswerArtifact>(observation.PrivateRawAnswers![index], token); // Reopen actual metadata under hash/custody.
                    if (!opened.IsSuccess || opened.Value is null) return opened.MapError<Dictionary<string, (string[], string[])>>(); // Changed metadata refuses.
                    var artifact = opened.Value; var binding = artifact.Binding; // Use actual complete retained binding.
                    var bytes = science.Store.ReadBytes(artifact.RawUtf8, token); // Verify full exact raw bytes, not preview.
                    if (!bytes.IsSuccess || bytes.Value is null) return bytes.MapError<Dictionary<string, (string[], string[])>>(); // Missing/changed output refuses.
                    if (binding.AssessmentRunId != observation.AssessmentRunId || binding.CaseId != source.CaseId || binding.OwnerInputSha256 != source.OwnerInputSha256 || binding.SettingsSha256 != observation.GenerationSettingsSha256 || binding.CorpusManifestSha256 != request.QualityGate!.CorpusManifest.ManifestSha256 || binding.CorpusSha256 != request.QualityGate.Corpus.CorpusSha256 || CodeDocumentationScientificTrainingPolicy.ValueHash(binding.Model) != CodeDocumentationScientificTrainingPolicy.ValueHash(arm.Passport) || artifact.RawUtf8.Sha256 != observation.StudentAnswerSha256[index] || new UTF8Encoding(false, true).GetString(bytes.Value).Length != artifact.CharCount) return CodeDocumentationScientificTrainingPolicy.Refuse<Dictionary<string, (string[], string[])>>(); // Bind input/model/settings/run/corpus plus exact SHA and character count.
                    (arm.Baseline ? result[source.CaseId].Baseline : result[source.CaseId].Trial)[repeat] = artifact.RawUtf8.Sha256; // Ordered five-output support for one binary family verdict.
                } // End exact per-case raw validation.
            } // End one arm's five repeats.
        } // No generation/evaluation was performed.
        return OperationResult<Dictionary<string, (string[] Baseline, string[] Trial)>>.Ok(result); // Only private hashes escape, never raw answers.
    } // Full raw output does not grant a training purpose.
    internal static bool ValidScientificVerdict(CodeDocumentationScientificTrainingPolicy policy, CodeDocumentationScientificVerdict verdict, string heldSha, string sourceSha, string trialSha, IReadOnlyDictionary<string, (string[] Baseline, string[] Trial)> raw) // Pure frozen acceptance check, separately testable without model calls.
    { // Operator provenance explicitly trusts the configured examiner artifact; no cryptographic/model attestation is invented.
        var expected = policy.Document; // Criteria were frozen before C0 and optimizer execution.
        if (verdict.Schema != "first-rp01-independent-scientific-verdict-v1" || verdict.GraderRole != "INDEPENDENT_BLIND_EXAMINER" || verdict.ExaminerAuthority != expected.ExaminerAuthority || verdict.PolicySha256 != policy.Sha256 || verdict.ProtocolSha256 != expected.Protocol.Sha256 || verdict.TrainingRunId != expected.TrainingRunId || verdict.HeldBundleSha256 != heldSha || verdict.SourceBindingSha256 != sourceSha || verdict.ParentManifestSha256 != expected.ParentManifestSha256 || verdict.TrialWeightsSha256 != trialSha || verdict.Families is not { Length: 12 } || verdict.Families.Select(f => f?.CaseId).Distinct(StringComparer.Ordinal).Count() != 12) return false; // Exact real run/model/held/source binding precedes scores.
        foreach (var family in verdict.Families) // Twelve binary source families, not 120 independent cases.
        { // Separate critical constraints cannot be hidden in aggregate gain.
            if (family is null || !expected.Cases.Any(c => c.CaseId == family.CaseId && c.FamilySha256 == family.FamilySha256) || !raw.TryGetValue(family.CaseId, out var answers) || family.BaselineRawAnswerSha256 is null || family.TrialRawAnswerSha256 is null || !family.BaselineRawAnswerSha256.SequenceEqual(answers.Baseline, StringComparer.Ordinal) || !family.TrialRawAnswerSha256.SequenceEqual(answers.Trial, StringComparer.Ordinal) || family.TrialCriticalErrors != 0 || family.NewCriticalErrors != 0) return false; // Negative, missing or drifted supporting outputs never promote.
        } // Every family has its five exact baseline/Trial outputs.
        return verdict.Families.Count(f => f.TrialCorrect) - verdict.Families.Count(f => f.BaselineCorrect) >= expected.MinimumCorrectFamilyGain; // Exact integer boundary: 11/12 minus 9/12 means gain two, without floating-point rounding or epsilon.
    } // Native quality remains an additional independently enforced necessary condition.
} // Ordinary default routes retain their existing bounded lifecycle unless a raw/held scientific identity requires this explicit branch.
