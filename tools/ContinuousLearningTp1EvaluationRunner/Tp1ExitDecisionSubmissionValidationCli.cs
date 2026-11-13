// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;

internal static class Tp1ExitDecisionSubmissionValidationCli
{
    internal const string ArmFlag = "--validate-tp1-exit-decision-submission";
    internal const string SubmissionLeaf = "tp1-exit-human-decision-submission.json";
    internal const string ValidationLeaf =
        "tp1-exit-decision-submission-validation.json";
    internal const string MarkdownLeaf =
        "tp1-exit-decision-submission-validation.ru.md";
    internal const string ValidationRunLeaf =
        "tp1-exit-decision-submission-validation-run.json";
    internal const string FixedStatus =
        "SHAPE_VALIDATED_NOT_TRUSTED_NOT_ACCEPTED";

    private const string ProjectsRoot = @"C:\Projects";
    private const int WindowSize = 3;
    private const double MinimumBestImprovement = 0.02D;
    private const int MaximumRowsPerRun = 4_096;
    private const long MaximumArtifactBytes = 16L * 1024 * 1024;
    private const string FutureProviderTermsProofRequirement =
        "A separate future TP2-start request must include a current ProviderTermsProof.";
    private const string FutureHostedTeacherIdentityRequirement =
        "That request must bind the exact hosted provider, model, and version.";
    private const string FutureHumanApprovalEvidenceRequirement =
        "That request must include matching human approval evidence for that exact provider/model/version.";

    private static readonly JsonSerializerOptions SnakeJson = CreateJsonOptions();
    private static readonly StringComparer PathComparer =
        Tp1EvaluationEvidencePacketIo.PathComparer;

    internal static IReadOnlyList<string> RequestLeafNames { get; } =
        Tp1ExitDecisionRequestCli.ArtifactLeafNames;

    internal static IReadOnlyList<string> SubmissionLeafNames { get; } =
        Array.AsReadOnly(new[] { SubmissionLeaf });

    internal static IReadOnlyList<string> ArtifactLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            ValidationLeaf,
            MarkdownLeaf,
            ValidationRunLeaf,
        });

    private static IReadOnlyList<string> IndependentReceiptFields { get; } =
        Array.AsReadOnly(new[]
        {
            "source_run_ordering_verified",
            "source_authenticity_verified",
            "numeric_evidence_verified",
            "local_teacher_ceiling_accepted",
        });

    private static readonly HashSet<string> RequestProperties = new(
    [
        "status", "decision_request_id", "source_ceiling_evaluation_id",
        "source_ceiling_status", "source_policy_id", "source_packet_hashes",
        "invariant_identity", "selected_runs", "selected_case_count",
        "selected_strict_teacher_wins", "first_held_out_accuracy_ygoal",
        "best_subsequent_held_out_accuracy_ygoal", "best_improvement",
        "pooled_teacher_beat_rate_descriptive", "plateau_predicate",
        "low_beat_authority_predicate", "source_local_teacher_ceiling_evaluated",
        "source_local_teacher_ceiling_candidate", "source_reason_codes",
        "independent_reviewer_prerequisite", "luba_tp1_decision_prerequisite",
        "future_tp2_start_requirements", "human_decision_present",
        "source_run_ordering_verified", "source_authenticity_verified",
        "numeric_evidence_verified", "independent_acceptance",
        "local_teacher_ceiling_accepted", "passed", "tp1_exit_gate_evaluated",
        "tp2_authorized", "eval_only", "dpo_train_ready",
        "teacher_output_used_for_training", "training_started",
        "dpo_steps_executed", "checkpoint_promoted", "promotion_started",
        "activation_started", "human_decision_required", "limitations",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SourceHashProperties = new(
    [
        "ceiling_evaluation_sha256", "ceiling_markdown_sha256",
        "ceiling_run_sha256",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> InvariantProperties = new(
    [
        "stage", "evaluation_set_id", "domain_id", "scorer_id",
        "scorer_version", "threshold_set_id", "inferred_k", "case_count",
        "evaluation_roster_sha256", "split_proof_artifact_ref",
        "split_proof_sha256", "model_id", "teacher_artifact_digest_kind",
        "ollama_manifest_digest", "ollama_model_layer_digest",
        "local_retention_policy_ref", "output_license_ref",
        "teacher_license_evidence_ref", "deployment_evidence_ref",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SelectedRunProperties = new(
    [
        "ordinal", "run_id", "window_ref", "case_count",
        "strict_teacher_wins", "held_out_accuracy_ygoal",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> IndependentPrerequisiteProperties = new(
    [
        "authority_role", "required", "required_receipt_fields",
        "authority_evidence_present",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> LubaPrerequisiteProperties = new(
    [
        "authority_role", "required", "allowed_decisions",
        "decision_evidence_present",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> FutureTp2Properties = new(
    [
        "separate_request_required", "provider_terms_proof_requirement",
        "hosted_teacher_identity_requirement", "human_approval_evidence_requirement",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> RequestRunProperties = new(
    [
        "status", "decision_request_id", "source_ceiling_evaluation_id",
        "source_policy_id", "decision_request_sha256", "markdown_sha256",
        "human_decision_submission_schema_sha256", "ceiling_evaluation_sha256",
        "ceiling_markdown_sha256", "ceiling_run_sha256",
        "published_artifact_leaf_names", "published_artifact_roster_sha256",
        "human_decision_present", "source_run_ordering_verified",
        "source_authenticity_verified", "numeric_evidence_verified",
        "independent_acceptance", "local_teacher_ceiling_accepted", "passed",
        "tp1_exit_gate_evaluated", "tp2_authorized", "eval_only",
        "dpo_train_ready", "teacher_output_used_for_training", "training_started",
        "dpo_steps_executed", "checkpoint_promoted", "promotion_started",
        "activation_started", "human_decision_required",
    ], StringComparer.Ordinal);

    internal static Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
        => RunAsync(
            args,
            stdout,
            stderr,
            cancellationToken,
            Tp1ExitDecisionSubmissionValidationDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies)
    {
        try
        {
            var result = await ExecuteAsync(args, dependencies, cancellationToken)
                .ConfigureAwait(false);
            await stdout.WriteLineAsync(JsonSerializer.Serialize(result, SnakeJson))
                .ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(
                    stderr,
                    "TP1_EXIT_SUBMISSION_VALIDATION_CANCELLED")
                .ConfigureAwait(false);
            return 130;
        }
        catch (Tp1ExitDecisionSubmissionValidationException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode).ConfigureAwait(false);
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(
                    stderr,
                    "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED")
                .ConfigureAwait(false);
            return 1;
        }
    }

    private static async Task<Tp1ExitDecisionSubmissionValidationSuccess> ExecuteAsync(
        string[] args,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
            throw InputInvalid();

        var parsed = ParseArguments(args);
        var requestDirectory = RequireAbsoluteDirectory(parsed.DecisionRequestDirectory);
        var submissionDirectory = RequireAbsoluteDirectory(parsed.SubmissionDirectory);
        var outputRoot = RequireAbsoluteDirectory(parsed.OutputRoot);
        if (File.Exists(outputRoot)
            || PathComparer.Equals(outputRoot, Path.GetPathRoot(outputRoot))
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(parsed.ValidationId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(parsed.ValidationId))
            throw InputInvalid();

        RejectExistingReparseSegments(requestDirectory);
        RejectExistingReparseSegments(submissionDirectory);
        RejectExistingReparseSegments(outputRoot);
        ValidateOutputOutsideProjects(outputRoot, ProjectsRoot, dependencies);
        ValidatePairwiseDisjoint(requestDirectory, submissionDirectory, outputRoot);

        var physicalRequest = RequirePhysical(
            dependencies,
            requestDirectory,
            allowMissing: false);
        var physicalSubmission = RequirePhysical(
            dependencies,
            submissionDirectory,
            allowMissing: false);
        var physicalOutput = RequirePhysical(
            dependencies,
            outputRoot,
            allowMissing: true);
        ValidatePairwiseDisjoint(physicalRequest, physicalSubmission, physicalOutput);
        var physicalProjects = dependencies.RejectOutputUnderProjects
            ? RequirePhysical(dependencies, ProjectsRoot, allowMissing: false)
            : ProjectsRoot;
        ValidateOutputOutsideProjects(physicalOutput, physicalProjects, dependencies);

        var destination = Path.Combine(outputRoot, parsed.ValidationId);
        if (File.Exists(destination) || Directory.Exists(destination))
            throw PublicationFailed();

        using var requestPacket = OpenRequestPacket(requestDirectory);
        using var submissionPacket = OpenSubmissionPacket(submissionDirectory);
        if (!PathComparer.Equals(requestPacket.PhysicalDirectory, physicalRequest)
            || !PathComparer.Equals(submissionPacket.PhysicalDirectory, physicalSubmission))
            throw EvidenceMismatch();

        var requestBytes = ReadPacketBytes(
            requestPacket,
            Tp1ExitDecisionRequestCli.DecisionRequestLeaf);
        var requestMarkdownBytes = ReadPacketBytes(
            requestPacket,
            Tp1ExitDecisionRequestCli.MarkdownLeaf);
        var schemaBytes = ReadPacketBytes(
            requestPacket,
            Tp1ExitDecisionRequestCli.SubmissionSchemaLeaf);
        var requestRunBytes = ReadPacketBytes(
            requestPacket,
            Tp1ExitDecisionRequestCli.DecisionRequestRunLeaf);
        var submissionBytes = ReadPacketBytes(submissionPacket, SubmissionLeaf);
        VerifyPacketsUnchanged(requestPacket, submissionPacket);

        ValidateMarkdown(requestMarkdownBytes);
        var request = ParseCanonicalRequest(requestBytes);
        var requestRun = ParseCanonicalRequestRun(requestRunBytes);
        ValidateRequestPacket(
            requestDirectory,
            request,
            requestRun,
            requestBytes,
            requestMarkdownBytes,
            schemaBytes);
        var expectedSchema = Tp1ExitDecisionSubmissionContract.BuildSchema(
            request.DecisionRequestId,
            request.SourcePacketHashes.CeilingEvaluationSha256,
            request.SourceLocalTeacherCeilingCandidate);
        if (!schemaBytes.AsSpan().SequenceEqual(expectedSchema))
            throw EvidenceMismatch();

        Tp1ExitDecisionSubmissionParseResult parsedSubmission;
        try
        {
            parsedSubmission = Tp1ExitDecisionSubmissionContract.ParseAndValidate(
                submissionBytes,
                request.DecisionRequestId,
                request.SourcePacketHashes.CeilingEvaluationSha256,
                request.SourceLocalTeacherCeilingCandidate);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw ShapeInvalid(exception);
        }
        cancellationToken.ThrowIfCancellationRequested();

        var requestHashes = new Tp1ExitDecisionRequestPacketHashes(
            HashRef(requestBytes),
            HashRef(requestMarkdownBytes),
            HashRef(schemaBytes),
            HashRef(requestRunBytes));
        var originalSubmissionHash = HashRef(submissionBytes);
        var canonicalSubmissionHash = HashRef(parsedSubmission.CanonicalBytes);
        var submission = parsedSubmission.Submission;
        var validation = new Tp1ExitDecisionSubmissionValidationArtifact(
            FixedStatus,
            parsed.ValidationId,
            request.DecisionRequestId,
            request.SourceCeilingEvaluationId,
            request.SourcePolicyId,
            request.SourceLocalTeacherCeilingCandidate,
            requestHashes,
            originalSubmissionHash,
            canonicalSubmissionHash,
            new Tp1ExitUntrustedDeclaredClaims(
                submission.SourceRunOrderingVerified,
                submission.SourceAuthenticityVerified,
                submission.NumericEvidenceVerified,
                submission.LocalTeacherCeilingAccepted,
                submission.IndependentAcceptance,
                submission.Tp1ExitDecision),
            SubmissionPresent: true,
            ShapeValidated: true,
            SchemaTemplateOnly: true,
            SchemaNotAuthority: true,
            SchemaNeverParseAsReceipt: true,
            TrustedHumanDecisionPresent: false,
            AuthorityEvidenceVerified: false,
            IndependentReviewerIdentityVerified: false,
            LubaIdentityVerified: false,
            SubmissionSignatureVerified: false,
            ReplayProtectionVerified: false,
            ReceiptAccepted: false,
            IndependentAcceptance: false,
            LocalTeacherCeilingAccepted: false,
            Passed: false,
            Tp1ExitGateEvaluated: false,
            Tp2Authorized: false,
            EvalOnly: true,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            PromotionStarted: false,
            ActivationStarted: false,
            HumanDecisionRequired: true);
        var validationBytes = SerializeJson(validation);
        var markdownBytes = Tp1EvaluationEvidencePacketIo.StrictUtf8.GetBytes(
            BuildRussianMarkdown(validation));
        var run = new Tp1ExitDecisionSubmissionValidationRunArtifact(
            FixedStatus,
            parsed.ValidationId,
            request.DecisionRequestId,
            HashRef(validationBytes),
            HashRef(markdownBytes),
            requestHashes,
            originalSubmissionHash,
            canonicalSubmissionHash,
            ArtifactLeafNames,
            HashRef(SerializeJson(ArtifactLeafNames)),
            SubmissionPresent: true,
            ShapeValidated: true,
            SchemaTemplateOnly: true,
            SchemaNotAuthority: true,
            SchemaNeverParseAsReceipt: true,
            TrustedHumanDecisionPresent: false,
            AuthorityEvidenceVerified: false,
            IndependentReviewerIdentityVerified: false,
            LubaIdentityVerified: false,
            SubmissionSignatureVerified: false,
            ReplayProtectionVerified: false,
            ReceiptAccepted: false,
            IndependentAcceptance: false,
            LocalTeacherCeilingAccepted: false,
            Passed: false,
            Tp1ExitGateEvaluated: false,
            Tp2Authorized: false,
            EvalOnly: true,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            PromotionStarted: false,
            ActivationStarted: false,
            HumanDecisionRequired: true);
        var runBytes = SerializeJson(run);

        Directory.CreateDirectory(outputRoot);
        RejectPublicationReparseSegments(outputRoot);
        var currentPhysicalOutput = RequirePublicationPhysical(dependencies, outputRoot);
        if (!PathComparer.Equals(currentPhysicalOutput, physicalOutput))
            throw PublicationFailed();
        ValidatePublicationOutputOutsideProjects(
            currentPhysicalOutput,
            physicalProjects,
            dependencies);
        VerifyPacketsUnchanged(requestPacket, submissionPacket);
        VerifySourcePhysicalIdentity(
            requestDirectory,
            physicalRequest,
            submissionDirectory,
            physicalSubmission,
            dependencies);
        await PublishAsync(
                outputRoot,
                currentPhysicalOutput,
                parsed.ValidationId,
                validationBytes,
                markdownBytes,
                runBytes,
                requestPacket,
                submissionPacket,
                requestDirectory,
                physicalRequest,
                submissionDirectory,
                physicalSubmission,
                dependencies,
                cancellationToken)
            .ConfigureAwait(false);

        return new Tp1ExitDecisionSubmissionValidationSuccess(
            Ok: true,
            FixedStatus,
            parsed.ValidationId,
            request.DecisionRequestId,
            ShapeValidated: true,
            TrustedHumanDecisionPresent: false,
            Tp1ExitGateEvaluated: false);
    }

    private static void ValidateRequestPacket(
        string requestDirectory,
        Tp1ExitDecisionRequestArtifact request,
        Tp1ExitDecisionRequestRunArtifact run,
        byte[] requestBytes,
        byte[] markdownBytes,
        byte[] schemaBytes)
    {
        if (!StringComparer.Ordinal.Equals(request.Status, Tp1ExitDecisionRequestCli.FixedStatus)
            || !StringComparer.Ordinal.Equals(run.Status, request.Status)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(request.DecisionRequestId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(request.DecisionRequestId)
            || !StringComparer.Ordinal.Equals(
                Path.GetFileName(requestDirectory),
                request.DecisionRequestId)
            || !StringComparer.Ordinal.Equals(run.DecisionRequestId, request.DecisionRequestId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(
                request.SourceCeilingEvaluationId)
            || !StringComparer.Ordinal.Equals(
                run.SourceCeilingEvaluationId,
                request.SourceCeilingEvaluationId)
            || !StringComparer.Ordinal.Equals(
                request.SourceCeilingStatus,
                Tp1LocalTeacherCeilingCli.FixedStatus)
            || !StringComparer.Ordinal.Equals(
                request.SourcePolicyId,
                Tp1LocalTeacherCeilingCli.FixedPolicyId)
            || !StringComparer.Ordinal.Equals(run.SourcePolicyId, request.SourcePolicyId)
            || request.SourcePacketHashes is null
            || !ValidRequestSourceHashes(request.SourcePacketHashes)
            || !StringComparer.Ordinal.Equals(
                run.CeilingEvaluationSha256,
                request.SourcePacketHashes.CeilingEvaluationSha256)
            || !StringComparer.Ordinal.Equals(
                run.CeilingMarkdownSha256,
                request.SourcePacketHashes.CeilingMarkdownSha256)
            || !StringComparer.Ordinal.Equals(
                run.CeilingRunSha256,
                request.SourcePacketHashes.CeilingRunSha256)
            || !StringComparer.Ordinal.Equals(run.DecisionRequestSha256, HashRef(requestBytes))
            || !StringComparer.Ordinal.Equals(run.MarkdownSha256, HashRef(markdownBytes))
            || !StringComparer.Ordinal.Equals(
                run.HumanDecisionSubmissionSchemaSha256,
                HashRef(schemaBytes))
            || run.PublishedArtifactLeafNames is null
            || !run.PublishedArtifactLeafNames.SequenceEqual(
                Tp1ExitDecisionRequestCli.ArtifactLeafNames,
                StringComparer.Ordinal)
            || !StringComparer.Ordinal.Equals(
                run.PublishedArtifactRosterSha256,
                HashRef(SerializeJson(Tp1ExitDecisionRequestCli.ArtifactLeafNames)))
            || !ValidRequestPosture(request)
            || !ValidRequestRunPosture(run))
            throw EvidenceMismatch();

        ValidateInvariants(request.InvariantIdentity);
        ValidateSelectedRunsAndArithmetic(request);
        ValidatePrerequisites(request);
    }

    private static bool ValidRequestSourceHashes(Tp1ExitDecisionSourceHashes hashes)
        => Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                hashes.CeilingEvaluationSha256)
            && Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                hashes.CeilingMarkdownSha256)
            && Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                hashes.CeilingRunSha256);

    private static bool ValidRequestPosture(Tp1ExitDecisionRequestArtifact request)
        => request.SourceLocalTeacherCeilingEvaluated
            && !request.HumanDecisionPresent
            && !request.SourceRunOrderingVerified
            && !request.SourceAuthenticityVerified
            && !request.NumericEvidenceVerified
            && !request.IndependentAcceptance
            && !request.LocalTeacherCeilingAccepted
            && !request.Passed
            && !request.Tp1ExitGateEvaluated
            && !request.Tp2Authorized
            && request.EvalOnly
            && !request.DpoTrainReady
            && !request.TeacherOutputUsedForTraining
            && !request.TrainingStarted
            && request.DpoStepsExecuted == 0
            && !request.CheckpointPromoted
            && !request.PromotionStarted
            && !request.ActivationStarted
            && request.HumanDecisionRequired;

    private static bool ValidRequestRunPosture(Tp1ExitDecisionRequestRunArtifact run)
        => !run.HumanDecisionPresent
            && !run.SourceRunOrderingVerified
            && !run.SourceAuthenticityVerified
            && !run.NumericEvidenceVerified
            && !run.IndependentAcceptance
            && !run.LocalTeacherCeilingAccepted
            && !run.Passed
            && !run.Tp1ExitGateEvaluated
            && !run.Tp2Authorized
            && run.EvalOnly
            && !run.DpoTrainReady
            && !run.TeacherOutputUsedForTraining
            && !run.TrainingStarted
            && run.DpoStepsExecuted == 0
            && !run.CheckpointPromoted
            && !run.PromotionStarted
            && !run.ActivationStarted
            && run.HumanDecisionRequired;

    private static void ValidatePrerequisites(Tp1ExitDecisionRequestArtifact request)
    {
        var independent = request.IndependentReviewerPrerequisite;
        var luba = request.LubaTp1DecisionPrerequisite;
        var future = request.FutureTp2StartRequirements;
        var expectedDecisions = request.SourceLocalTeacherCeilingCandidate
            ? new[] { "TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT" }
            : new[] { "TP1_EXIT_REJECT" };
        if (independent is null
            || !StringComparer.Ordinal.Equals(
                independent.AuthorityRole,
                "independent_tp1_evidence_reviewer")
            || !independent.Required
            || independent.RequiredReceiptFields is null
            || !independent.RequiredReceiptFields.SequenceEqual(
                IndependentReceiptFields,
                StringComparer.Ordinal)
            || independent.AuthorityEvidencePresent
            || luba is null
            || !StringComparer.Ordinal.Equals(
                luba.AuthorityRole,
                "luba_tp1_decision_authority")
            || !luba.Required
            || luba.AllowedDecisions is null
            || !luba.AllowedDecisions.SequenceEqual(expectedDecisions, StringComparer.Ordinal)
            || luba.DecisionEvidencePresent
            || future is null
            || !future.SeparateRequestRequired
            || !StringComparer.Ordinal.Equals(
                future.ProviderTermsProofRequirement,
                FutureProviderTermsProofRequirement)
            || !StringComparer.Ordinal.Equals(
                future.HostedTeacherIdentityRequirement,
                FutureHostedTeacherIdentityRequirement)
            || !StringComparer.Ordinal.Equals(
                future.HumanApprovalEvidenceRequirement,
                FutureHumanApprovalEvidenceRequirement)
            || request.SourceReasonCodes is null
            || request.Limitations is null
            || request.Limitations.Count == 0
            || request.Limitations.Any(string.IsNullOrWhiteSpace))
            throw EvidenceMismatch();
    }

    private static void ValidateInvariants(Tp1ReviewInvariantSet invariants)
    {
        if (invariants is null
            || !StringComparer.Ordinal.Equals(invariants.Stage, "TP1_LOCAL")
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.EvaluationSetId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.DomainId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.ScorerId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.ScorerVersion)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.ThresholdSetId)
            || invariants.InferredK is <= 0 or > MaximumRowsPerRun
            || invariants.CaseCount is <= 0 or > MaximumRowsPerRun
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.EvaluationRosterSha256)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.SplitProofArtifactRef)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.SplitProofSha256)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                invariants.ModelId)
            || !StringComparer.Ordinal.Equals(
                invariants.TeacherArtifactDigestKind,
                "ollama_manifest_sha256")
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(
                invariants.OllamaManifestDigest)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelLayerDigest(
                invariants.OllamaModelLayerDigest)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.LocalRetentionPolicyRef)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.OutputLicenseRef)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.TeacherLicenseEvidenceRef)
            || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                invariants.DeploymentEvidenceRef))
            throw EvidenceMismatch();
    }

    private static void ValidateSelectedRunsAndArithmetic(
        Tp1ExitDecisionRequestArtifact request)
    {
        if (request.SelectedRuns is null
            || request.SelectedRuns.Count != WindowSize
            || request.SelectedRuns.Any(static run => run is null)
            || request.SourceReasonCodes is null)
            throw EvidenceMismatch();
        try
        {
            long cases = 0;
            long wins = 0;
            var runIds = new HashSet<string>(StringComparer.Ordinal);
            var windows = new HashSet<string>(StringComparer.Ordinal);
            var firstOrdinal = request.SelectedRuns[0].Ordinal;
            for (var index = 0; index < request.SelectedRuns.Count; index++)
            {
                var selected = request.SelectedRuns[index];
                if (firstOrdinal < 0
                    || selected.Ordinal != checked(firstOrdinal + index)
                    || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(selected.RunId)
                    || !Tp1ExitDecisionSubmissionContract.IsCanonicalEvidenceRef(
                        selected.WindowRef)
                    || selected.CaseCount != request.InvariantIdentity.CaseCount
                    || selected.CaseCount <= 0
                    || selected.StrictTeacherWins is < 0
                    || selected.StrictTeacherWins > selected.CaseCount
                    || !FiniteUnit(selected.HeldOutAccuracyYgoal)
                    || !runIds.Add(selected.RunId)
                    || !windows.Add(selected.WindowRef))
                    throw EvidenceMismatch();
                cases = checked(cases + selected.CaseCount);
                wins = checked(wins + selected.StrictTeacherWins);
            }
            var first = request.SelectedRuns[0].HeldOutAccuracyYgoal;
            var best = Math.Max(
                request.SelectedRuns[1].HeldOutAccuracyYgoal,
                request.SelectedRuns[2].HeldOutAccuracyYgoal);
            var improvement = best - first;
            var plateau = improvement < MinimumBestImprovement;
            var lowBeat = checked(wins * 10) < cases;
            var candidate = plateau && lowBeat;
            var expectedReasons = BuildReasonCodes(plateau, lowBeat, improvement);
            if (cases > int.MaxValue
                || request.SelectedCaseCount != cases
                || request.SelectedStrictTeacherWins != wins
                || request.FirstHeldOutAccuracyYgoal != first
                || request.BestSubsequentHeldOutAccuracyYgoal != best
                || request.BestImprovement != improvement
                || request.PooledTeacherBeatRateDescriptive != (double)wins / cases
                || request.PlateauPredicate != plateau
                || request.LowBeatAuthorityPredicate != lowBeat
                || request.SourceLocalTeacherCeilingCandidate != candidate
                || !request.SourceReasonCodes.SequenceEqual(
                    expectedReasons,
                    StringComparer.Ordinal))
                throw EvidenceMismatch();
        }
        catch (OverflowException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static IReadOnlyList<string> BuildReasonCodes(
        bool plateau,
        bool lowBeat,
        double improvement)
    {
        var reasons = new List<string>
        {
            plateau
                ? "BEST_IMPROVEMENT_STRICTLY_BELOW_0_02"
                : "BEST_IMPROVEMENT_NOT_STRICTLY_BELOW_0_02",
            lowBeat
                ? "POOLED_WINS_TIMES_10_STRICTLY_BELOW_CASES"
                : "POOLED_WINS_TIMES_10_NOT_STRICTLY_BELOW_CASES",
        };
        if (improvement < 0D)
            reasons.Add("NEGATIVE_BEST_IMPROVEMENT_IS_REGRESSION");
        return reasons.AsReadOnly();
    }

    private static Tp1ExitDecisionRequestArtifact ParseCanonicalRequest(byte[] bytes)
        => ParseCanonical<Tp1ExitDecisionRequestArtifact>(
            bytes,
            RequestProperties,
            static root =>
            {
                ValidateExactObject(root.GetProperty("source_packet_hashes"),
                    SourceHashProperties);
                ValidateExactObject(root.GetProperty("invariant_identity"),
                    InvariantProperties);
                ValidateArrayObjects(root.GetProperty("selected_runs"),
                    SelectedRunProperties);
                ValidateExactObject(root.GetProperty("independent_reviewer_prerequisite"),
                    IndependentPrerequisiteProperties);
                ValidateExactObject(root.GetProperty("luba_tp1_decision_prerequisite"),
                    LubaPrerequisiteProperties);
                ValidateExactObject(root.GetProperty("future_tp2_start_requirements"),
                    FutureTp2Properties);
            });

    private static Tp1ExitDecisionRequestRunArtifact ParseCanonicalRequestRun(byte[] bytes)
        => ParseCanonical<Tp1ExitDecisionRequestRunArtifact>(bytes, RequestRunProperties);

    private static T ParseCanonical<T>(
        byte[] bytes,
        HashSet<string> properties,
        Action<JsonElement>? validateNested = null)
    {
        ValidateStrictUtf8AsEvidence(bytes);
        try
        {
            using var document = JsonDocument.Parse(bytes, DocumentOptions());
            ValidateExactObject(document.RootElement, properties);
            validateNested?.Invoke(document.RootElement);
            var value = JsonSerializer.Deserialize<T>(bytes, SnakeJson)
                ?? throw EvidenceMismatch();
            if (!bytes.AsSpan().SequenceEqual(SerializeJson(value)))
                throw EvidenceMismatch();
            return value;
        }
        catch (Tp1ExitDecisionSubmissionValidationException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static void ValidateArrayObjects(JsonElement array, HashSet<string> properties)
    {
        if (array.ValueKind != JsonValueKind.Array)
            throw EvidenceMismatch();
        foreach (var item in array.EnumerateArray())
            ValidateExactObject(item, properties);
    }

    private static void ValidateExactObject(JsonElement value, HashSet<string> properties)
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw EvidenceMismatch();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!properties.Contains(property.Name) || !seen.Add(property.Name))
                throw EvidenceMismatch();
        }
        if (!seen.SetEquals(properties))
            throw EvidenceMismatch();
    }

    private static ParsedArguments ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["--decision-request-directory"] = null,
            ["--submission-directory"] = null,
            ["--output-root"] = null,
            ["--validation-id"] = null,
        };
        var armed = false;
        for (var index = 0; index < args.Length; index++)
        {
            var current = args[index];
            if (StringComparer.Ordinal.Equals(current, ArmFlag))
            {
                if (armed)
                    throw InputInvalid();
                armed = true;
                continue;
            }
            if (!values.ContainsKey(current)
                || values[current] is not null
                || index + 1 >= args.Length
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw InputInvalid();
            values[current] = args[++index];
        }
        if (!armed || values.Values.Any(string.IsNullOrWhiteSpace))
            throw InputInvalid();
        return new ParsedArguments(
            values["--decision-request-directory"]!,
            values["--submission-directory"]!,
            values["--output-root"]!,
            values["--validation-id"]!);
    }

    private static async Task PublishAsync(
        string outputRoot,
        string physicalOutputRoot,
        string validationId,
        byte[] validationBytes,
        byte[] markdownBytes,
        byte[] runBytes,
        Tp1RetainedEvidencePacket requestPacket,
        Tp1RetainedEvidencePacket submissionPacket,
        string requestDirectory,
        string physicalRequestDirectory,
        string submissionDirectory,
        string physicalSubmissionDirectory,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies,
        CancellationToken cancellationToken)
    {
        IEvaluationPublicationBoundary? publication = null;
        var published = false;
        try
        {
            var created = dependencies.CreatePublication(
                outputRoot,
                validationId,
                ArtifactLeafNames);
            if (!created.IsSuccess || created.Value is null)
                throw PublicationFailed();
            publication = created.Value;
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            await WriteBytesAsync(
                publication,
                ValidationLeaf,
                validationBytes,
                cancellationToken).ConfigureAwait(false);
            await WriteBytesAsync(
                publication,
                MarkdownLeaf,
                markdownBytes,
                cancellationToken).ConfigureAwait(false);
            await WriteBytesAsync(
                publication,
                ValidationRunLeaf,
                runBytes,
                cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            VerifyPacketsUnchanged(requestPacket, submissionPacket);
            VerifySourcePhysicalIdentity(
                requestDirectory,
                physicalRequestDirectory,
                submissionDirectory,
                physicalSubmissionDirectory,
                dependencies);
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            publication.PublishAndVerify();
            published = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Tp1ExitDecisionSubmissionValidationException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw PublicationFailed(exception);
        }
        finally
        {
            Exception? cleanupFailure = null;
            if (!published && publication is not null)
            {
                try { publication.CleanupFailedPublication(); }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    cleanupFailure = exception;
                }
            }
            publication?.Dispose();
            if (cleanupFailure is not null)
                throw PublicationFailed(cleanupFailure);
        }
    }

    private static async Task WriteBytesAsync(
        IEvaluationPublicationBoundary publication,
        string leaf,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        await using var stream = publication.CreateArtifactFile(leaf);
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static string BuildRussianMarkdown(
        Tp1ExitDecisionSubmissionValidationArtifact validation)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Структурная проверка TP1 exit submission");
        builder.AppendLine();
        builder.Append("Статус: ").AppendLine(validation.Status);
        builder.Append("Validation ID: ").AppendLine(validation.ValidationId);
        builder.Append("Decision request ID: ").AppendLine(validation.DecisionRequestId);
        builder.AppendLine();
        builder.AppendLine("Файл submission присутствует и соответствует форме, но остаётся недоверенным заявлением.");
        builder.AppendLine("JSON Schema является только шаблоном и не является полномочием или receipt.");
        builder.AppendLine("Личности reviewer и Любы, подпись, защита от replay и независимая приёмка не проверены.");
        builder.AppendLine("TP1 exit не оценён; TP2, DPO, обучение, promotion и activation не разрешены и не начинались.");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void ValidateMarkdown(byte[] bytes)
    {
        ValidateStrictUtf8AsEvidence(bytes);
        if (bytes.Length == 0)
            throw EvidenceMismatch();
    }

    private static void ValidateStrictUtf8AsEvidence(byte[] bytes)
    {
        try
        {
            Tp1EvaluationEvidencePacketIo.ValidateStrictUtf8WithoutBom(bytes);
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static void ValidatePairwiseDisjoint(params string[] paths)
    {
        for (var left = 0; left < paths.Length; left++)
        for (var right = left + 1; right < paths.Length; right++)
        {
            if (PathComparer.Equals(paths[left], paths[right])
                || Tp1EvaluationEvidencePacketIo.IsContainedBy(paths[left], paths[right])
                || Tp1EvaluationEvidencePacketIo.IsContainedBy(paths[right], paths[left]))
                throw InputInvalid();
        }
    }

    private static void ValidateOutputOutsideProjects(
        string outputRoot,
        string projectsRoot,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies)
    {
        if (!dependencies.RejectOutputUnderProjects)
            return;
        var projects = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectsRoot));
        if (PathComparer.Equals(outputRoot, projects)
            || Tp1EvaluationEvidencePacketIo.IsContainedBy(outputRoot, projects))
            throw InputInvalid();
    }

    private static void ValidatePublicationOutputOutsideProjects(
        string outputRoot,
        string projectsRoot,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies)
    {
        if (!dependencies.RejectOutputUnderProjects)
            return;
        var projects = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectsRoot));
        if (PathComparer.Equals(outputRoot, projects)
            || Tp1EvaluationEvidencePacketIo.IsContainedBy(outputRoot, projects))
            throw PublicationFailed();
    }

    private static string RequireAbsoluteDirectory(string path)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.RequireAbsoluteDirectory(path);
        }
        catch (Exception exception) when (exception is Tp1EvidencePacketException
            or ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw InputInvalid(exception);
        }
    }

    private static void RejectExistingReparseSegments(string path)
    {
        try
        {
            Tp1EvaluationEvidencePacketIo.RejectExistingReparseSegments(path);
        }
        catch (Exception exception) when (exception is Tp1EvidencePacketException
            or IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            throw InputInvalid(exception);
        }
    }

    private static void RejectPublicationReparseSegments(string path)
    {
        try
        {
            Tp1EvaluationEvidencePacketIo.RejectExistingReparseSegments(path);
        }
        catch (Exception exception) when (exception is Tp1EvidencePacketException
            or IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            throw PublicationFailed(exception);
        }
    }

    private static Tp1RetainedEvidencePacket OpenRequestPacket(string directory)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.OpenExactPacket(
                directory,
                RequestLeafNames,
                MaximumArtifactBytes);
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static Tp1RetainedEvidencePacket OpenSubmissionPacket(string directory)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.OpenExactPacket(
                directory,
                SubmissionLeafNames,
                MaximumArtifactBytes);
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw ShapeInvalid(exception);
        }
    }

    private static byte[] ReadPacketBytes(Tp1RetainedEvidencePacket packet, string leaf)
    {
        try
        {
            return packet.ReadAllBytes(leaf);
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static void VerifyPacketsUnchanged(
        Tp1RetainedEvidencePacket requestPacket,
        Tp1RetainedEvidencePacket submissionPacket)
    {
        try
        {
            requestPacket.VerifyUnchanged();
            submissionPacket.VerifyUnchanged();
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static string RequirePhysical(
        Tp1ExitDecisionSubmissionValidationDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var result = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw InputInvalid();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static string RequirePublicationPhysical(
        Tp1ExitDecisionSubmissionValidationDependencies dependencies,
        string path)
    {
        var result = dependencies.ResolvePhysicalPath(path, false);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw PublicationFailed();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static void VerifySourcePhysicalIdentity(
        string requestDirectory,
        string expectedPhysicalRequest,
        string submissionDirectory,
        string expectedPhysicalSubmission,
        Tp1ExitDecisionSubmissionValidationDependencies dependencies)
    {
        var request = dependencies.ResolvePhysicalPath(requestDirectory, false);
        var submission = dependencies.ResolvePhysicalPath(submissionDirectory, false);
        if (!request.IsSuccess
            || string.IsNullOrWhiteSpace(request.Value)
            || !submission.IsSuccess
            || string.IsNullOrWhiteSpace(submission.Value)
            || !PathComparer.Equals(
                Path.TrimEndingDirectorySeparator(request.Value),
                expectedPhysicalRequest)
            || !PathComparer.Equals(
                Path.TrimEndingDirectorySeparator(submission.Value),
                expectedPhysicalSubmission))
            throw EvidenceMismatch();
    }

    private static bool FiniteUnit(double value)
        => double.IsFinite(value) && value is >= 0D and <= 1D;

    private static string HashRef(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static byte[] SerializeJson<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, SnakeJson);

    private static JsonDocumentOptions DocumentOptions()
        => new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 32,
        };

    private static JsonSerializerOptions CreateJsonOptions()
        => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
            MaxDepth = 32,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            },
        };

    private static async Task WriteErrorAsync(TextWriter stderr, string code)
    {
        var message = code switch
        {
            "TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID" =>
                "The TP1 exit submission validation input is invalid.",
            "TP1_EXIT_SUBMISSION_VALIDATION_EVIDENCE_MISMATCH" =>
                "The TP1 exit submission validation evidence is inconsistent.",
            "TP1_EXIT_SUBMISSION_VALIDATION_SHAPE_INVALID" =>
                "The TP1 exit submission shape is invalid.",
            "TP1_EXIT_SUBMISSION_VALIDATION_CANCELLED" =>
                "The TP1 exit submission validation was cancelled.",
            _ => "The TP1 exit submission validation publication failed.",
        };
        await stderr.WriteLineAsync(JsonSerializer.Serialize(
            new { ok = false, error_code = code, message })).ConfigureAwait(false);
    }

    private static Tp1ExitDecisionSubmissionValidationException InputInvalid(
        Exception? inner = null)
        => new("TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID", inner);

    private static Tp1ExitDecisionSubmissionValidationException EvidenceMismatch(
        Exception? inner = null)
        => new("TP1_EXIT_SUBMISSION_VALIDATION_EVIDENCE_MISMATCH", inner);

    private static Tp1ExitDecisionSubmissionValidationException ShapeInvalid(
        Exception? inner = null)
        => new("TP1_EXIT_SUBMISSION_VALIDATION_SHAPE_INVALID", inner);

    private static Tp1ExitDecisionSubmissionValidationException PublicationFailed(
        Exception? inner = null)
        => new("TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED", inner);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private sealed record ParsedArguments(
        string DecisionRequestDirectory,
        string SubmissionDirectory,
        string OutputRoot,
        string ValidationId);

    private sealed class Tp1ExitDecisionSubmissionValidationException : Exception
    {
        internal Tp1ExitDecisionSubmissionValidationException(
            string errorCode,
            Exception? inner)
            : base(errorCode, inner) => ErrorCode = errorCode;

        internal string ErrorCode { get; }
    }
}

internal sealed class Tp1ExitDecisionSubmissionValidationDependencies
{
    internal Tp1ExitDecisionSubmissionValidationDependencies(
        Func<string, string, IReadOnlyCollection<string>,
            OperationResult<IEvaluationPublicationBoundary>>? createPublication = null,
        bool rejectOutputUnderProjects = true,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null)
    {
        CreatePublication = createPublication
            ?? WindowsEvaluationPublicationBoundary.Create;
        RejectOutputUnderProjects = rejectOutputUnderProjects;
        ResolvePhysicalPath = resolvePhysicalPath ?? PhysicalPathResolver.Resolve;
        RequiresWindowsHandlePublication = createPublication is null;
    }

    internal Func<string, string, IReadOnlyCollection<string>,
        OperationResult<IEvaluationPublicationBoundary>> CreatePublication { get; }
    internal bool RejectOutputUnderProjects { get; }
    internal Func<string, bool, OperationResult<string>> ResolvePhysicalPath { get; }
    internal bool RequiresWindowsHandlePublication { get; }

    internal static Tp1ExitDecisionSubmissionValidationDependencies Production { get; }
        = new();
}
