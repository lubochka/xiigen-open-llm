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

internal static class Tp1ExitDecisionRequestCli
{
    internal const string ArmFlag = "--assemble-tp1-exit-decision-request";
    internal const string DecisionRequestLeaf = "tp1-exit-decision-request.json";
    internal const string MarkdownLeaf = "tp1-exit-decision-request.ru.md";
    internal const string SubmissionSchemaLeaf =
        "tp1-exit-human-decision-submission-schema.json";
    internal const string DecisionRequestRunLeaf =
        "tp1-exit-decision-request-run.json";
    internal const string FixedStatus =
        "TP1_EXIT_DECISION_REQUEST_CREATED_NOT_ACCEPTED";

    private const string ProjectsRoot = @"C:\Projects";
    private const string ReviewStatus = "REVIEW_REQUEST_CREATED_NOT_ACCEPTED";
    private const int WindowSize = 3;
    private const double MinimumBestImprovement = 0.02D;
    private const int BeatNumeratorMultiplier = 10;
    private const int BeatDenominatorThreshold = 1;
    private const int MaximumRowsPerRun = 4_096;
    private const long MaximumArtifactBytes = 16L * 1024 * 1024;

    private static readonly JsonSerializerOptions SnakeJson = CreateJsonOptions();
    private static readonly StringComparer PathComparer =
        Tp1EvaluationEvidencePacketIo.PathComparer;

    internal static IReadOnlyList<string> SourceLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            Tp1LocalTeacherCeilingCli.EvaluationLeaf,
            Tp1LocalTeacherCeilingCli.MarkdownLeaf,
            Tp1LocalTeacherCeilingCli.EvaluationRunLeaf,
        });

    internal static IReadOnlyList<string> ArtifactLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            DecisionRequestLeaf,
            MarkdownLeaf,
            SubmissionSchemaLeaf,
            DecisionRequestRunLeaf,
        });

    private static IReadOnlyList<string> IndependentReceiptFields { get; } =
        Array.AsReadOnly(new[]
        {
            "source_run_ordering_verified",
            "source_authenticity_verified",
            "numeric_evidence_verified",
            "local_teacher_ceiling_accepted",
        });

    private static readonly HashSet<string> EvaluationProperties = new(
    [
        "status", "evaluation_id", "policy", "source_review_packet_id",
        "source_review_status", "source_packet_hashes", "invariant_identity",
        "selected_runs", "selected_case_count", "selected_strict_teacher_wins",
        "first_held_out_accuracy_ygoal",
        "best_subsequent_held_out_accuracy_ygoal", "best_improvement",
        "pooled_teacher_beat_rate_descriptive", "plateau_predicate",
        "low_beat_authority_predicate", "local_teacher_ceiling_evaluated",
        "local_teacher_ceiling_candidate", "source_ordering_verified",
        "independent_acceptance", "passed", "tp1_exit_gate_evaluated",
        "tp2_authorized", "eval_only", "dpo_train_ready",
        "teacher_output_used_for_training", "training_started",
        "dpo_steps_executed", "checkpoint_promoted", "human_decision_required",
        "reason_codes", "limitations",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> PolicyProperties = new(
    [
        "policy_id", "window_size", "minimum_best_improvement",
        "beat_numerator_multiplier", "beat_denominator_threshold",
        "plateau_operator", "low_beat_authority_operator",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SourceHashProperties = new(
    [
        "review_request_sha256", "review_markdown_sha256", "review_run_sha256",
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

    private static readonly HashSet<string> EvaluationRunProperties = new(
    [
        "status", "evaluation_id", "policy_id", "source_review_packet_id",
        "evaluation_sha256", "markdown_sha256", "review_request_sha256",
        "review_markdown_sha256", "review_run_sha256",
        "published_artifact_leaf_names", "published_artifact_roster_sha256",
        "local_teacher_ceiling_evaluated", "local_teacher_ceiling_candidate",
        "source_ordering_verified", "independent_acceptance", "passed",
        "tp1_exit_gate_evaluated", "tp2_authorized", "eval_only",
        "dpo_train_ready", "teacher_output_used_for_training",
        "training_started", "dpo_steps_executed", "checkpoint_promoted",
        "human_decision_required",
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
            Tp1ExitDecisionRequestDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        Tp1ExitDecisionRequestDependencies dependencies)
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
            await WriteErrorAsync(stderr, "TP1_EXIT_DECISION_REQUEST_CANCELLED")
                .ConfigureAwait(false);
            return 130;
        }
        catch (Tp1ExitDecisionRequestException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode).ConfigureAwait(false);
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(
                    stderr,
                    "TP1_EXIT_DECISION_REQUEST_PUBLICATION_FAILED")
                .ConfigureAwait(false);
            return 1;
        }
    }

    private static async Task<Tp1ExitDecisionRequestSuccess> ExecuteAsync(
        string[] args,
        Tp1ExitDecisionRequestDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
            throw InputInvalid();

        var parsed = ParseArguments(args);
        var ceilingDirectory = RequireAbsoluteDirectory(
            parsed.CeilingEvaluationDirectory);
        var outputRoot = RequireAbsoluteDirectory(parsed.OutputRoot);
        if (File.Exists(outputRoot)
            || PathComparer.Equals(outputRoot, Path.GetPathRoot(outputRoot)))
            throw InputInvalid();
        if (!Tp1EvaluationEvidencePacketIo.IsSafeIdentity(parsed.DecisionRequestId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(
                parsed.DecisionRequestId))
            throw InputInvalid();
        RejectExistingReparseSegments(ceilingDirectory);
        RejectExistingReparseSegments(outputRoot);
        ValidateOutputOutsideProjects(outputRoot, ProjectsRoot, dependencies);
        ValidateDisjoint(ceilingDirectory, outputRoot);

        var physicalCeiling = RequirePhysical(
            dependencies,
            ceilingDirectory,
            allowMissing: false);
        var physicalOutput = RequirePhysical(
            dependencies,
            outputRoot,
            allowMissing: true);
        ValidateDisjoint(physicalCeiling, physicalOutput);
        var physicalProjects = dependencies.RejectOutputUnderProjects
            ? RequirePhysical(dependencies, ProjectsRoot, allowMissing: false)
            : ProjectsRoot;
        ValidateOutputOutsideProjects(
            physicalOutput,
            physicalProjects,
            dependencies);

        var destination = Path.Combine(outputRoot, parsed.DecisionRequestId);
        if (File.Exists(destination) || Directory.Exists(destination))
            throw PublicationFailed();

        using var packet = OpenPacket(ceilingDirectory);
        if (!PathComparer.Equals(packet.PhysicalDirectory, physicalCeiling))
            throw EvidenceMismatch();
        var evaluationBytes = ReadPacketBytes(
            packet,
            Tp1LocalTeacherCeilingCli.EvaluationLeaf);
        var markdownBytes = ReadPacketBytes(
            packet,
            Tp1LocalTeacherCeilingCli.MarkdownLeaf);
        var evaluationRunBytes = ReadPacketBytes(
            packet,
            Tp1LocalTeacherCeilingCli.EvaluationRunLeaf);
        ValidateMarkdown(markdownBytes);
        var evaluation = ParseEvaluation(evaluationBytes);
        var evaluationRun = ParseEvaluationRun(evaluationRunBytes);
        ValidateSourcePacket(
            ceilingDirectory,
            evaluation,
            evaluationRun,
            evaluationBytes,
            markdownBytes);
        cancellationToken.ThrowIfCancellationRequested();

        var sourceHashes = new Tp1ExitDecisionSourceHashes(
            HashRef(evaluationBytes),
            HashRef(markdownBytes),
            HashRef(evaluationRunBytes));
        var request = BuildDecisionRequest(
            parsed.DecisionRequestId,
            evaluation,
            sourceHashes);
        var requestBytes = SerializeJson(request);
        var requestMarkdownBytes = Tp1EvaluationEvidencePacketIo.StrictUtf8.GetBytes(
            BuildRussianMarkdown(request));
        var schemaBytes = BuildSubmissionSchema(
            parsed.DecisionRequestId,
            sourceHashes.CeilingEvaluationSha256,
            evaluation.LocalTeacherCeilingCandidate);
        var run = new Tp1ExitDecisionRequestRunArtifact(
            FixedStatus,
            parsed.DecisionRequestId,
            evaluation.EvaluationId,
            evaluation.Policy.PolicyId,
            HashRef(requestBytes),
            HashRef(requestMarkdownBytes),
            HashRef(schemaBytes),
            sourceHashes.CeilingEvaluationSha256,
            sourceHashes.CeilingMarkdownSha256,
            sourceHashes.CeilingRunSha256,
            ArtifactLeafNames,
            HashRef(SerializeJson(ArtifactLeafNames)),
            HumanDecisionPresent: false,
            SourceRunOrderingVerified: false,
            SourceAuthenticityVerified: false,
            NumericEvidenceVerified: false,
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

        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(outputRoot);
        RejectPublicationReparseSegments(outputRoot);
        var currentPhysicalOutput = RequirePublicationPhysical(
            dependencies,
            outputRoot);
        if (!PathComparer.Equals(currentPhysicalOutput, physicalOutput))
            throw PublicationFailed();
        ValidatePublicationOutputOutsideProjects(
            currentPhysicalOutput,
            physicalProjects,
            dependencies);
        VerifyPacketUnchanged(packet);
        VerifySourcePhysicalIdentity(
            ceilingDirectory,
            physicalCeiling,
            dependencies);
        await PublishAsync(
                outputRoot,
                currentPhysicalOutput,
                parsed.DecisionRequestId,
                requestBytes,
                requestMarkdownBytes,
                schemaBytes,
                runBytes,
                packet,
                ceilingDirectory,
                physicalCeiling,
                dependencies,
                cancellationToken)
            .ConfigureAwait(false);

        return new Tp1ExitDecisionRequestSuccess(
            Ok: true,
            FixedStatus,
            parsed.DecisionRequestId,
            evaluation.EvaluationId,
            evaluation.LocalTeacherCeilingCandidate);
    }

    private static Tp1ExitDecisionRequestArtifact BuildDecisionRequest(
        string decisionRequestId,
        Tp1LocalTeacherCeilingEvaluationArtifact evaluation,
        Tp1ExitDecisionSourceHashes sourceHashes)
    {
        var allowedDecisions = evaluation.LocalTeacherCeilingCandidate
            ? Array.AsReadOnly(new[] { "TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT" })
            : Array.AsReadOnly(new[] { "TP1_EXIT_REJECT" });
        return new Tp1ExitDecisionRequestArtifact(
            FixedStatus,
            decisionRequestId,
            evaluation.EvaluationId,
            evaluation.Status,
            evaluation.Policy.PolicyId,
            sourceHashes,
            evaluation.InvariantIdentity,
            evaluation.SelectedRuns,
            evaluation.SelectedCaseCount,
            evaluation.SelectedStrictTeacherWins,
            evaluation.FirstHeldOutAccuracyYgoal,
            evaluation.BestSubsequentHeldOutAccuracyYgoal,
            evaluation.BestImprovement,
            evaluation.PooledTeacherBeatRateDescriptive,
            evaluation.PlateauPredicate,
            evaluation.LowBeatAuthorityPredicate,
            SourceLocalTeacherCeilingEvaluated: true,
            SourceLocalTeacherCeilingCandidate:
                evaluation.LocalTeacherCeilingCandidate,
            evaluation.ReasonCodes,
            new Tp1ExitIndependentReviewPrerequisite(
                AuthorityRole: "independent_tp1_evidence_reviewer",
                Required: true,
                IndependentReceiptFields,
                AuthorityEvidencePresent: false),
            new Tp1ExitLubaDecisionPrerequisite(
                AuthorityRole: "luba_tp1_decision_authority",
                Required: true,
                allowedDecisions,
                DecisionEvidencePresent: false),
            new Tp1ExitFutureTp2StartRequirements(
                SeparateRequestRequired: true,
                ProviderTermsProofRequirement:
                    "A separate future TP2-start request must include a current ProviderTermsProof.",
                HostedTeacherIdentityRequirement:
                    "That request must bind the exact hosted provider, model, and version.",
                HumanApprovalEvidenceRequirement:
                    "That request must include matching human approval evidence for that exact provider/model/version."),
            HumanDecisionPresent: false,
            SourceRunOrderingVerified: false,
            SourceAuthenticityVerified: false,
            NumericEvidenceVerified: false,
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
            HumanDecisionRequired: true,
            Limitations);
    }

    private static IReadOnlyList<string> Limitations { get; } =
        Array.AsReadOnly(new[]
        {
            "Пакет является только запросом решения и не является решением, приёмкой или квитанцией полномочий.",
            "Порядок, подлинность источника, числовые доказательства и вывод о локальном потолке ещё должен проверить независимый reviewer.",
            "Отдельное решение Любы по TP1 ещё отсутствует; candidate не означает ACCEPT.",
            "При candidate=false допустимо запросить только TP1_EXIT_REJECT.",
            "TP2, DPO, обучение, promotion и activation не разрешены и не начинались.",
            "Начало TP2 потребует нового отдельного запроса с текущим ProviderTermsProof, точной hosted provider/model/version идентичностью и совпадающим человеческим approval evidence.",
        });

    private static void ValidateSourcePacket(
        string ceilingDirectory,
        Tp1LocalTeacherCeilingEvaluationArtifact evaluation,
        Tp1LocalTeacherCeilingEvaluationRunArtifact run,
        byte[] evaluationBytes,
        byte[] markdownBytes)
    {
        if (!StringComparer.Ordinal.Equals(
                evaluation.Status,
                Tp1LocalTeacherCeilingCli.FixedStatus)
            || !StringComparer.Ordinal.Equals(run.Status, evaluation.Status)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(evaluation.EvaluationId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(
                evaluation.EvaluationId)
            || !StringComparer.Ordinal.Equals(
                Path.GetFileName(ceilingDirectory),
                evaluation.EvaluationId)
            || !StringComparer.Ordinal.Equals(
                run.EvaluationId,
                evaluation.EvaluationId)
            || evaluation.Policy is null
            || !ValidPolicy(evaluation.Policy)
            || !StringComparer.Ordinal.Equals(
                run.PolicyId,
                evaluation.Policy.PolicyId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(
                evaluation.SourceReviewPacketId)
            || !StringComparer.Ordinal.Equals(
                evaluation.SourceReviewPacketId,
                run.SourceReviewPacketId)
            || !StringComparer.Ordinal.Equals(
                evaluation.SourceReviewStatus,
                ReviewStatus)
            || evaluation.SourcePacketHashes is null
            || !ValidSourceHashes(evaluation.SourcePacketHashes)
            || !StringComparer.Ordinal.Equals(
                run.ReviewRequestSha256,
                evaluation.SourcePacketHashes.ReviewRequestSha256)
            || !StringComparer.Ordinal.Equals(
                run.ReviewMarkdownSha256,
                evaluation.SourcePacketHashes.ReviewMarkdownSha256)
            || !StringComparer.Ordinal.Equals(
                run.ReviewRunSha256,
                evaluation.SourcePacketHashes.ReviewRunSha256)
            || !StringComparer.Ordinal.Equals(
                run.EvaluationSha256,
                HashRef(evaluationBytes))
            || !StringComparer.Ordinal.Equals(
                run.MarkdownSha256,
                HashRef(markdownBytes))
            || run.PublishedArtifactLeafNames is null
            || !run.PublishedArtifactLeafNames.SequenceEqual(
                Tp1LocalTeacherCeilingCli.ArtifactLeafNames,
                StringComparer.Ordinal)
            || !StringComparer.Ordinal.Equals(
                run.PublishedArtifactRosterSha256,
                HashRef(SerializeJson(Tp1LocalTeacherCeilingCli.ArtifactLeafNames)))
            || run.LocalTeacherCeilingCandidate
                != evaluation.LocalTeacherCeilingCandidate
            || !ValidEvaluationPosture(evaluation)
            || !ValidRunPosture(run))
            throw EvidenceMismatch();

        ValidateInvariants(evaluation.InvariantIdentity);
        try
        {
            ValidateSelectedRunsAndArithmetic(evaluation);
        }
        catch (OverflowException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static bool ValidPolicy(Tp1LocalTeacherCeilingPolicyArtifact policy)
        => StringComparer.Ordinal.Equals(
                policy.PolicyId,
                Tp1LocalTeacherCeilingCli.FixedPolicyId)
            && policy.WindowSize == WindowSize
            && policy.MinimumBestImprovement == MinimumBestImprovement
            && policy.BeatNumeratorMultiplier == BeatNumeratorMultiplier
            && policy.BeatDenominatorThreshold == BeatDenominatorThreshold
            && StringComparer.Ordinal.Equals(
                policy.PlateauOperator,
                "strict_less_than")
            && StringComparer.Ordinal.Equals(
                policy.LowBeatAuthorityOperator,
                "strict_less_than_checked_integer_cross_product");

    private static bool ValidSourceHashes(
        Tp1LocalTeacherCeilingSourcePacketHashes hashes)
        => Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.ReviewRequestSha256)
            && Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.ReviewMarkdownSha256)
            && Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.ReviewRunSha256);

    private static bool ValidEvaluationPosture(
        Tp1LocalTeacherCeilingEvaluationArtifact evaluation)
        => evaluation.LocalTeacherCeilingEvaluated
            && !evaluation.SourceOrderingVerified
            && !evaluation.IndependentAcceptance
            && !evaluation.Passed
            && !evaluation.Tp1ExitGateEvaluated
            && !evaluation.Tp2Authorized
            && evaluation.EvalOnly
            && !evaluation.DpoTrainReady
            && !evaluation.TeacherOutputUsedForTraining
            && !evaluation.TrainingStarted
            && evaluation.DpoStepsExecuted == 0
            && !evaluation.CheckpointPromoted
            && evaluation.HumanDecisionRequired;

    private static bool ValidRunPosture(
        Tp1LocalTeacherCeilingEvaluationRunArtifact run)
        => run.LocalTeacherCeilingEvaluated
            && !run.SourceOrderingVerified
            && !run.IndependentAcceptance
            && !run.Passed
            && !run.Tp1ExitGateEvaluated
            && !run.Tp2Authorized
            && run.EvalOnly
            && !run.DpoTrainReady
            && !run.TeacherOutputUsedForTraining
            && !run.TrainingStarted
            && run.DpoStepsExecuted == 0
            && !run.CheckpointPromoted
            && run.HumanDecisionRequired;

    private static void ValidateSelectedRunsAndArithmetic(
        Tp1LocalTeacherCeilingEvaluationArtifact evaluation)
    {
        if (evaluation.SelectedRuns is null
            || evaluation.SelectedRuns.Count != WindowSize
            || evaluation.SelectedRuns.Any(static run => run is null)
            || evaluation.ReasonCodes is null
            || evaluation.Limitations is null
            || evaluation.Limitations.Count == 0
            || evaluation.Limitations.Any(string.IsNullOrWhiteSpace))
            throw EvidenceMismatch();

        long cases = 0;
        long wins = 0;
        var runIds = new HashSet<string>(StringComparer.Ordinal);
        var windows = new HashSet<string>(StringComparer.Ordinal);
        var firstOrdinal = evaluation.SelectedRuns[0].Ordinal;
        for (var index = 0; index < evaluation.SelectedRuns.Count; index++)
        {
            var selected = evaluation.SelectedRuns[index];
            if (firstOrdinal < 0
                || selected.Ordinal != checked(firstOrdinal + index)
                || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(selected.RunId)
                || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                    selected.WindowRef)
                || selected.CaseCount != evaluation.InvariantIdentity.CaseCount
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

        var firstGoal = evaluation.SelectedRuns[0].HeldOutAccuracyYgoal;
        var bestSubsequent = Math.Max(
            evaluation.SelectedRuns[1].HeldOutAccuracyYgoal,
            evaluation.SelectedRuns[2].HeldOutAccuracyYgoal);
        var improvement = bestSubsequent - firstGoal;
        var plateau = improvement < MinimumBestImprovement;
        var lowBeat = checked(wins * BeatNumeratorMultiplier)
            < checked(cases * BeatDenominatorThreshold);
        var candidate = plateau && lowBeat;
        var expectedReasons = BuildReasonCodes(plateau, lowBeat, improvement);
        if (cases > int.MaxValue
            || evaluation.SelectedCaseCount != cases
            || evaluation.SelectedStrictTeacherWins != wins
            || evaluation.FirstHeldOutAccuracyYgoal != firstGoal
            || evaluation.BestSubsequentHeldOutAccuracyYgoal != bestSubsequent
            || evaluation.BestImprovement != improvement
            || evaluation.PooledTeacherBeatRateDescriptive != (double)wins / cases
            || evaluation.PlateauPredicate != plateau
            || evaluation.LowBeatAuthorityPredicate != lowBeat
            || evaluation.LocalTeacherCeilingCandidate != candidate
            || !evaluation.ReasonCodes.SequenceEqual(
                expectedReasons,
                StringComparer.Ordinal))
            throw EvidenceMismatch();
    }

    private static IReadOnlyList<string> BuildReasonCodes(
        bool plateau,
        bool lowBeat,
        double bestImprovement)
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
        if (bestImprovement < 0D)
            reasons.Add("NEGATIVE_BEST_IMPROVEMENT_IS_REGRESSION");
        return reasons.AsReadOnly();
    }

    private static void ValidateInvariants(Tp1ReviewInvariantSet invariants)
    {
        if (invariants is null
            || !StringComparer.Ordinal.Equals(invariants.Stage, "TP1_LOCAL")
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(
                invariants.EvaluationSetId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.DomainId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(invariants.ScorerId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(
                invariants.ScorerVersion)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(
                invariants.ThresholdSetId)
            || invariants.InferredK is <= 0 or > MaximumRowsPerRun
            || invariants.CaseCount is <= 0 or > MaximumRowsPerRun
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.EvaluationRosterSha256)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.SplitProofArtifactRef)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.SplitProofSha256)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                invariants.ModelId)
            || !StringComparer.Ordinal.Equals(
                invariants.TeacherArtifactDigestKind,
                "ollama_manifest_sha256")
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaManifestDigest(invariants.OllamaManifestDigest)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(
                    invariants.OllamaModelLayerDigest)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.LocalRetentionPolicyRef)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.OutputLicenseRef)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.TeacherLicenseEvidenceRef)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                invariants.DeploymentEvidenceRef))
            throw EvidenceMismatch();
    }

    private static Tp1LocalTeacherCeilingEvaluationArtifact ParseEvaluation(
        byte[] bytes)
        => ParseCanonical<Tp1LocalTeacherCeilingEvaluationArtifact>(
            bytes,
            EvaluationProperties,
            static root =>
            {
                ValidateExactObject(root.GetProperty("policy"), PolicyProperties);
                ValidateExactObject(
                    root.GetProperty("source_packet_hashes"),
                    SourceHashProperties);
                ValidateExactObject(
                    root.GetProperty("invariant_identity"),
                    InvariantProperties);
                ValidateArrayObjects(
                    root.GetProperty("selected_runs"),
                    SelectedRunProperties);
            });

    private static Tp1LocalTeacherCeilingEvaluationRunArtifact ParseEvaluationRun(
        byte[] bytes)
        => ParseCanonical<Tp1LocalTeacherCeilingEvaluationRunArtifact>(
            bytes,
            EvaluationRunProperties);

    private static T ParseCanonical<T>(
        byte[] bytes,
        HashSet<string> rootProperties,
        Action<JsonElement>? validateNested = null)
    {
        ValidateStrictUtf8(bytes);
        try
        {
            using var document = JsonDocument.Parse(bytes, DocumentOptions());
            ValidateExactObject(document.RootElement, rootProperties);
            validateNested?.Invoke(document.RootElement);
            var value = JsonSerializer.Deserialize<T>(bytes, SnakeJson)
                ?? throw InputInvalid();
            if (!bytes.AsSpan().SequenceEqual(SerializeJson(value)))
                throw EvidenceMismatch();
            return value;
        }
        catch (Tp1ExitDecisionRequestException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw InputInvalid(exception);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static void ValidateArrayObjects(
        JsonElement array,
        HashSet<string> properties)
    {
        if (array.ValueKind != JsonValueKind.Array)
            throw InputInvalid();
        foreach (var item in array.EnumerateArray())
            ValidateExactObject(item, properties);
    }

    private static void ValidateExactObject(
        JsonElement element,
        HashSet<string> expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw InputInvalid();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                throw InputInvalid();
        }
        if (!seen.SetEquals(expected))
            throw InputInvalid();
    }

    private static byte[] BuildSubmissionSchema(
        string decisionRequestId,
        string sourceEvaluationSha256,
        bool candidate)
        => Tp1ExitDecisionSubmissionContract.BuildSchema(
            decisionRequestId,
            sourceEvaluationSha256,
            candidate);

    internal static bool IsSubmissionCombinationAllowed(
        bool sourceCandidate,
        bool sourceRunOrderingVerified,
        bool sourceAuthenticityVerified,
        bool numericEvidenceVerified,
        bool localTeacherCeilingAccepted,
        bool independentAcceptance,
        string tp1ExitDecision)
        => Tp1ExitDecisionSubmissionContract.IsCombinationAllowed(
            sourceCandidate,
            sourceRunOrderingVerified,
            sourceAuthenticityVerified,
            numericEvidenceVerified,
            localTeacherCeilingAccepted,
            independentAcceptance,
            tp1ExitDecision);

    private static string BuildRussianMarkdown(Tp1ExitDecisionRequestArtifact request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Запрос человеческого решения о выходе из TP1");
        builder.AppendLine();
        builder.AppendLine($"Статус: `{FixedStatus}`.");
        builder.AppendLine($"Идентификатор запроса: `{request.DecisionRequestId}`.");
        builder.AppendLine($"Источник: `{request.SourceCeilingEvaluationId}`, политика `{request.SourcePolicyId}`.");
        builder.AppendLine();
        builder.AppendLine("## Воспроизведённая числовая сводка");
        builder.AppendLine();
        builder.AppendLine($"Лучшее улучшение: {Format(request.BestImprovement)}; строгий plateau (< 0.02): {Lower(request.PlateauPredicate)}.");
        builder.AppendLine($"Победы/случаи: {request.SelectedStrictTeacherWins}/{request.SelectedCaseCount}; checked(wins × 10) < cases: {Lower(request.LowBeatAuthorityPredicate)}.");
        builder.AppendLine($"sourceLocalTeacherCeilingCandidate={Lower(request.SourceLocalTeacherCeilingCandidate)}. Отрицательное улучшение является регрессией, а не дополнительным доказательством качества.");
        builder.AppendLine();
        builder.AppendLine("## Два раздельных будущих условия");
        builder.AppendLine();
        builder.AppendLine("1. Независимый reviewer должен отдельно проверить порядок прогонов, подлинность источника, числовые доказательства и вывод о локальном потолке.");
        builder.AppendLine("2. После этого требуется отдельное решение Любы только по TP1: `TP1_EXIT_ACCEPT` или `TP1_EXIT_REJECT`.");
        if (!request.SourceLocalTeacherCeilingCandidate)
            builder.AppendLine("Так как candidate=false, этот запрос допускает только будущее `TP1_EXIT_REJECT`.");
        builder.AppendLine();
        builder.AppendLine("## Границы полномочий");
        builder.AppendLine();
        builder.AppendLine("Схема submission является неизменяемым шаблоном, не authority и никогда не читается этой командой как receipt.");
        builder.AppendLine("humanDecisionPresent=false; sourceRunOrderingVerified=false; sourceAuthenticityVerified=false; numericEvidenceVerified=false; independentAcceptance=false; localTeacherCeilingAccepted=false; passed=false; tp1ExitGateEvaluated=false.");
        builder.AppendLine("tp2Authorized=false; DPO-ready=false; обучение, promotion и activation не начинались.");
        builder.AppendLine("Отдельный будущий TP2-start request должен содержать текущий ProviderTermsProof, точные hosted provider/model/version и совпадающее human approval evidence.");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static ParsedArguments ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["--ceiling-evaluation-directory"] = null,
            ["--output-root"] = null,
            ["--decision-request-id"] = null,
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
        if (!armed || values.Values.Any(static value => string.IsNullOrWhiteSpace(value)))
            throw InputInvalid();
        return new ParsedArguments(
            values["--ceiling-evaluation-directory"]!,
            values["--output-root"]!,
            values["--decision-request-id"]!);
    }

    private static async Task PublishAsync(
        string outputRoot,
        string physicalOutputRoot,
        string decisionRequestId,
        byte[] requestBytes,
        byte[] markdownBytes,
        byte[] schemaBytes,
        byte[] runBytes,
        Tp1RetainedEvidencePacket packet,
        string ceilingDirectory,
        string physicalCeilingDirectory,
        Tp1ExitDecisionRequestDependencies dependencies,
        CancellationToken cancellationToken)
    {
        IEvaluationPublicationBoundary? publication = null;
        var published = false;
        try
        {
            var created = dependencies.CreatePublication(
                outputRoot,
                decisionRequestId,
                ArtifactLeafNames);
            if (!created.IsSuccess || created.Value is null)
                throw PublicationFailed();
            publication = created.Value;
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            await WriteBytesAsync(
                    publication,
                    DecisionRequestLeaf,
                    requestBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(
                    publication,
                    MarkdownLeaf,
                    markdownBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(
                    publication,
                    SubmissionSchemaLeaf,
                    schemaBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(
                    publication,
                    DecisionRequestRunLeaf,
                    runBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            VerifyPacketUnchanged(packet);
            VerifySourcePhysicalIdentity(
                ceilingDirectory,
                physicalCeilingDirectory,
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
        catch (Tp1ExitDecisionRequestException)
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

    private static void ValidateMarkdown(byte[] bytes)
    {
        ValidateStrictUtf8(bytes);
        if (bytes.Length == 0)
            throw InputInvalid();
    }

    private static void ValidateStrictUtf8(byte[] bytes)
    {
        try
        {
            Tp1EvaluationEvidencePacketIo.ValidateStrictUtf8WithoutBom(bytes);
        }
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static void ValidateDisjoint(string left, string right)
    {
        if (PathComparer.Equals(left, right)
            || Tp1EvaluationEvidencePacketIo.IsContainedBy(left, right)
            || Tp1EvaluationEvidencePacketIo.IsContainedBy(right, left))
            throw InputInvalid();
    }

    private static void ValidateOutputOutsideProjects(
        string outputRoot,
        string projectsRoot,
        Tp1ExitDecisionRequestDependencies dependencies)
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
        Tp1ExitDecisionRequestDependencies dependencies)
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

    private static Tp1RetainedEvidencePacket OpenPacket(string directory)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.OpenExactPacket(
                directory,
                SourceLeafNames,
                MaximumArtifactBytes);
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static byte[] ReadPacketBytes(
        Tp1RetainedEvidencePacket packet,
        string leaf)
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

    private static void VerifyPacketUnchanged(Tp1RetainedEvidencePacket packet)
    {
        try
        {
            packet.VerifyUnchanged();
        }
        catch (Tp1EvidencePacketException exception)
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static string RequirePhysical(
        Tp1ExitDecisionRequestDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var result = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw InputInvalid();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static string RequirePublicationPhysical(
        Tp1ExitDecisionRequestDependencies dependencies,
        string path)
    {
        var result = dependencies.ResolvePhysicalPath(path, false);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw PublicationFailed();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static void VerifySourcePhysicalIdentity(
        string ceilingDirectory,
        string expectedPhysical,
        Tp1ExitDecisionRequestDependencies dependencies)
    {
        var result = dependencies.ResolvePhysicalPath(
            ceilingDirectory,
            false);
        if (!result.IsSuccess
            || string.IsNullOrWhiteSpace(result.Value)
            || !PathComparer.Equals(
                Path.TrimEndingDirectorySeparator(result.Value),
                expectedPhysical))
            throw EvidenceMismatch();
    }

    private static bool FiniteUnit(double value)
        => double.IsFinite(value) && value is >= 0D and <= 1D;

    private static string HashRef(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static string Format(double value)
        => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

    private static string Lower(bool value)
        => value.ToString().ToLowerInvariant();

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
            "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID" =>
                "The TP1 exit decision request input is invalid.",
            "TP1_EXIT_DECISION_REQUEST_EVIDENCE_MISMATCH" =>
                "The TP1 exit decision request evidence is inconsistent.",
            "TP1_EXIT_DECISION_REQUEST_CANCELLED" =>
                "The TP1 exit decision request was cancelled.",
            _ => "The TP1 exit decision request publication failed.",
        };
        await stderr.WriteLineAsync(JsonSerializer.Serialize(
                new { ok = false, error_code = code, message }))
            .ConfigureAwait(false);
    }

    private static Tp1ExitDecisionRequestException InputInvalid(
        Exception? inner = null)
        => new("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", inner);

    private static Tp1ExitDecisionRequestException EvidenceMismatch(
        Exception? inner = null)
        => new("TP1_EXIT_DECISION_REQUEST_EVIDENCE_MISMATCH", inner);

    private static Tp1ExitDecisionRequestException PublicationFailed(
        Exception? inner = null)
        => new("TP1_EXIT_DECISION_REQUEST_PUBLICATION_FAILED", inner);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private sealed record ParsedArguments(
        string CeilingEvaluationDirectory,
        string OutputRoot,
        string DecisionRequestId);

    private sealed class Tp1ExitDecisionRequestException : Exception
    {
        internal Tp1ExitDecisionRequestException(
            string errorCode,
            Exception? inner)
            : base(errorCode, inner) => ErrorCode = errorCode;

        internal string ErrorCode { get; }
    }
}

internal sealed class Tp1ExitDecisionRequestDependencies
{
    internal Tp1ExitDecisionRequestDependencies(
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

    internal static Tp1ExitDecisionRequestDependencies Production { get; } = new();
}
