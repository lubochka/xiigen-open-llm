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

internal static class Tp1LocalTeacherCeilingCli
{
    internal const string ArmFlag = "--evaluate-tp1-local-teacher-ceiling";
    internal const string EvaluationLeaf =
        "tp1-local-teacher-ceiling-evaluation.json";
    internal const string MarkdownLeaf =
        "tp1-local-teacher-ceiling-evaluation.ru.md";
    internal const string EvaluationRunLeaf =
        "tp1-local-teacher-ceiling-evaluation-run.json";

    internal const string FixedStatus =
        "LOCAL_TEACHER_CEILING_CANDIDATE_EVALUATED_NOT_ACCEPTED";
    internal const string FixedPolicyId = "Q-TP-NUMS/plan-11-gate-6b-v1";

    private const string ReviewStatus = "REVIEW_REQUEST_CREATED_NOT_ACCEPTED";
    private const string ProjectsRoot = @"C:\Projects";
    private const int WindowSize = 3;
    private const double MinimumBestImprovement = 0.02D;
    private const int BeatNumeratorMultiplier = 10;
    private const int BeatDenominatorThreshold = 1;
    private const long MaximumArtifactBytes = 16L * 1024 * 1024;
    private const int MaximumRuns = 256;
    private const int MaximumRowsPerRun = 4_096;

    private static readonly JsonSerializerOptions SnakeJson = CreateJsonOptions();
    private static readonly StringComparer PathComparer =
        Tp1EvaluationEvidencePacketIo.PathComparer;

    internal static IReadOnlyList<string> SourceLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            Tp1EvaluationReviewCli.ReviewRequestLeaf,
            Tp1EvaluationReviewCli.ReviewMarkdownLeaf,
            Tp1EvaluationReviewCli.ReviewRunLeaf,
        });

    internal static IReadOnlyList<string> ArtifactLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            EvaluationLeaf,
            MarkdownLeaf,
            EvaluationRunLeaf,
        });

    private static readonly HashSet<string> ReviewRequestProperties = new(
    [
        "status", "review_packet_id", "ordering_authority_ref",
        "ordering_verified", "invariants", "runs", "adjacent_deltas",
        "aggregate", "eval_only", "independent_acceptance",
        "tp1_exit_gate_evaluated", "local_teacher_ceiling_evaluated",
        "dpo_train_ready", "teacher_output_used_for_training",
        "training_started", "dpo_steps_executed", "checkpoint_promoted",
        "human_decision_required",
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

    private static readonly HashSet<string> ReviewRunSummaryProperties = new(
    [
        "ordinal", "run_id", "source_run_id", "window_ref", "case_count",
        "semantic_candidate_count", "held_out_accuracy_ygoal",
        "held_out_accuracy_ybest", "teacher_gap_to_gold",
        "teacher_beat_rate", "strict_teacher_wins",
        "evaluation_roster_sha256", "source_artifact_hashes",
        "measurement_noise_floor", "measurement_noise_status",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SourceArtifactHashProperties = new(
    [
        "ordinal", "candidate_scores_sha256", "case_comparisons_sha256",
        "teacher_stage_gap_report_sha256", "scoring_run_manifest_sha256",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> AdjacentDeltaProperties = new(
    [
        "from_ordinal", "to_ordinal", "delta_held_out_accuracy_ygoal",
        "delta_held_out_accuracy_ybest", "delta_teacher_gap_to_gold",
        "delta_teacher_beat_rate",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> AggregateProperties = new(
    [
        "run_count", "total_case_observations", "total_semantic_candidates",
        "pooled_strict_teacher_wins", "mean_held_out_accuracy_ygoal",
        "mean_held_out_accuracy_ybest", "aggregate_teacher_gap_to_gold",
        "pooled_teacher_beat_rate",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> ReviewRunProperties = new(
    [
        "status", "review_packet_id", "ordering_authority_ref",
        "ordering_verified", "review_index_sha256", "review_request_sha256",
        "review_markdown_sha256", "published_artifact_leaf_names",
        "source_artifacts", "eval_only", "independent_acceptance",
        "tp1_exit_gate_evaluated", "local_teacher_ceiling_evaluated",
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
            Tp1LocalTeacherCeilingDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        Tp1LocalTeacherCeilingDependencies dependencies)
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
            await WriteErrorAsync(stderr, "TP1_CEILING_CANCELLED")
                .ConfigureAwait(false);
            return 130;
        }
        catch (Tp1CeilingException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode).ConfigureAwait(false);
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(stderr, "TP1_CEILING_PUBLICATION_FAILED")
                .ConfigureAwait(false);
            return 1;
        }
    }

    private static async Task<Tp1LocalTeacherCeilingSuccess> ExecuteAsync(
        string[] args,
        Tp1LocalTeacherCeilingDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
            throw InputInvalid();

        var parsed = ParseArguments(args);
        var reviewDirectory = RequireAbsoluteDirectory(parsed.ReviewPacketDirectory);
        var outputRoot = RequireAbsoluteDirectory(parsed.OutputRoot);
        if (File.Exists(outputRoot)
            || PathComparer.Equals(outputRoot, Path.GetPathRoot(outputRoot)))
            throw InputInvalid();
        if (!Tp1EvaluationEvidencePacketIo.IsSafeIdentity(parsed.EvaluationId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(
                parsed.EvaluationId))
            throw InputInvalid();
        RejectExistingReparseSegments(reviewDirectory);
        RejectExistingReparseSegments(outputRoot);
        ValidateOutputOutsideProjects(outputRoot, ProjectsRoot, dependencies);
        ValidateDisjoint(reviewDirectory, outputRoot);

        var physicalReview = RequirePhysical(
            dependencies,
            reviewDirectory,
            allowMissing: false);
        var physicalOutput = RequirePhysical(
            dependencies,
            outputRoot,
            allowMissing: true);
        ValidateDisjoint(physicalReview, physicalOutput);
        var physicalProjects = dependencies.RejectOutputUnderProjects
            ? RequirePhysical(dependencies, ProjectsRoot, allowMissing: false)
            : ProjectsRoot;
        ValidateOutputOutsideProjects(
            physicalOutput,
            physicalProjects,
            dependencies);

        var destination = Path.Combine(outputRoot, parsed.EvaluationId);
        if (File.Exists(destination) || Directory.Exists(destination))
            throw PublicationFailed();

        using var packet = OpenPacket(reviewDirectory);
        if (!PathComparer.Equals(packet.PhysicalDirectory, physicalReview))
            throw EvidenceMismatch();
        var requestBytes = ReadPacketBytes(packet,
            Tp1EvaluationReviewCli.ReviewRequestLeaf);
        var markdownBytes = ReadPacketBytes(packet,
            Tp1EvaluationReviewCli.ReviewMarkdownLeaf);
        var reviewRunBytes = ReadPacketBytes(packet,
            Tp1EvaluationReviewCli.ReviewRunLeaf);
        ValidateMarkdown(markdownBytes);
        var request = ParseReviewRequest(requestBytes);
        var reviewRun = ParseReviewRun(reviewRunBytes);
        ValidateReviewPacket(
            reviewDirectory,
            request,
            reviewRun,
            requestBytes,
            markdownBytes);
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Runs.Count < WindowSize)
            throw WindowIncomplete();

        var selected = request.Runs.TakeLast(WindowSize).ToArray();
        long selectedCases = 0;
        long selectedWins = 0;
        foreach (var run in selected)
        {
            selectedCases = checked(selectedCases + run.CaseCount);
            selectedWins = checked(selectedWins + run.StrictTeacherWins);
        }
        var firstGoal = selected[0].HeldOutAccuracyYgoal;
        var bestSubsequentGoal = Math.Max(
            selected[1].HeldOutAccuracyYgoal,
            selected[2].HeldOutAccuracyYgoal);
        var bestImprovement = bestSubsequentGoal - firstGoal;
        var plateau = IsStrictPlateau(
            firstGoal,
            selected[1].HeldOutAccuracyYgoal,
            selected[2].HeldOutAccuracyYgoal);
        var lowBeatAuthority = IsLowBeatAuthority(selectedWins, selectedCases);
        var candidate = plateau && lowBeatAuthority;
        var descriptiveBeatRate = (double)selectedWins / selectedCases;

        var sourceHashes = new Tp1LocalTeacherCeilingSourcePacketHashes(
            HashRef(requestBytes),
            HashRef(markdownBytes),
            HashRef(reviewRunBytes));
        var selectedArtifacts = selected.Select(static run =>
                new Tp1LocalTeacherCeilingSelectedRun(
                    run.Ordinal,
                    run.RunId,
                    run.WindowRef,
                    run.CaseCount,
                    run.StrictTeacherWins,
                    run.HeldOutAccuracyYgoal))
            .ToArray();
        var evaluation = new Tp1LocalTeacherCeilingEvaluationArtifact(
            FixedStatus,
            parsed.EvaluationId,
            Policy,
            request.ReviewPacketId,
            request.Status,
            sourceHashes,
            request.Invariants,
            selectedArtifacts,
            checked((int)selectedCases),
            selectedWins,
            firstGoal,
            bestSubsequentGoal,
            bestImprovement,
            descriptiveBeatRate,
            plateau,
            lowBeatAuthority,
            LocalTeacherCeilingEvaluated: true,
            LocalTeacherCeilingCandidate: candidate,
            SourceOrderingVerified: false,
            IndependentAcceptance: false,
            Passed: false,
            Tp1ExitGateEvaluated: false,
            Tp2Authorized: false,
            EvalOnly: true,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            HumanDecisionRequired: true,
            BuildReasonCodes(plateau, lowBeatAuthority, bestImprovement),
            Limitations);
        var evaluationBytes = SerializeJson(evaluation);
        var reportBytes = Tp1EvaluationEvidencePacketIo.StrictUtf8.GetBytes(
            BuildRussianMarkdown(evaluation));
        var runArtifact = new Tp1LocalTeacherCeilingEvaluationRunArtifact(
            FixedStatus,
            parsed.EvaluationId,
            FixedPolicyId,
            request.ReviewPacketId,
            HashRef(evaluationBytes),
            HashRef(reportBytes),
            sourceHashes.ReviewRequestSha256,
            sourceHashes.ReviewMarkdownSha256,
            sourceHashes.ReviewRunSha256,
            ArtifactLeafNames,
            HashRef(SerializeJson(ArtifactLeafNames)),
            LocalTeacherCeilingEvaluated: true,
            LocalTeacherCeilingCandidate: candidate,
            SourceOrderingVerified: false,
            IndependentAcceptance: false,
            Passed: false,
            Tp1ExitGateEvaluated: false,
            Tp2Authorized: false,
            EvalOnly: true,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            HumanDecisionRequired: true);
        var runBytes = SerializeJson(runArtifact);

        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(outputRoot);
        RejectExistingReparseSegments(outputRoot);
        var currentPhysicalOutput = RequirePhysical(
            dependencies,
            outputRoot,
            allowMissing: false);
        if (!PathComparer.Equals(currentPhysicalOutput, physicalOutput))
            throw PublicationFailed();
        ValidateOutputOutsideProjects(
            currentPhysicalOutput,
            physicalProjects,
            dependencies);
        VerifyPacketUnchanged(packet);
        VerifySourcePhysicalIdentity(
            reviewDirectory,
            physicalReview,
            dependencies);
        await PublishAsync(
                outputRoot,
                currentPhysicalOutput,
                parsed.EvaluationId,
                evaluationBytes,
                reportBytes,
                runBytes,
                packet,
                reviewDirectory,
                physicalReview,
                dependencies,
                cancellationToken)
            .ConfigureAwait(false);

        return new Tp1LocalTeacherCeilingSuccess(
            Ok: true,
            FixedStatus,
            parsed.EvaluationId,
            candidate,
            WindowSize,
            checked((int)selectedCases));
    }

    private static Tp1LocalTeacherCeilingPolicyArtifact Policy { get; } = new(
        FixedPolicyId,
        WindowSize,
        MinimumBestImprovement,
        BeatNumeratorMultiplier,
        BeatDenominatorThreshold,
        PlateauOperator: "strict_less_than",
        LowBeatAuthorityOperator: "strict_less_than_checked_integer_cross_product");

    private static IReadOnlyList<string> Limitations { get; } = Array.AsReadOnly(
        new[]
        {
            "Порядок прогонов заявлен неподписанным исходным пакетом и не проверен независимо.",
            "Хеши подтверждают совпадение байтов, но не авторство исходных артефактов.",
            "Кандидат на потолок — числовой диагностический результат, а не PASS, приёмка или TP1 exit.",
            "Результат не разрешает TP2, DPO, обучение или promotion checkpoint.",
            "Десятичный pooled beat rate приведён только описательно; решающим является целочисленное сравнение.",
        });

    private static IReadOnlyList<string> BuildReasonCodes(
        bool plateau,
        bool lowBeatAuthority,
        double bestImprovement)
    {
        var reasons = new List<string>
        {
            plateau
                ? "BEST_IMPROVEMENT_STRICTLY_BELOW_0_02"
                : "BEST_IMPROVEMENT_NOT_STRICTLY_BELOW_0_02",
            lowBeatAuthority
                ? "POOLED_WINS_TIMES_10_STRICTLY_BELOW_CASES"
                : "POOLED_WINS_TIMES_10_NOT_STRICTLY_BELOW_CASES",
        };
        if (bestImprovement < 0D)
            reasons.Add("NEGATIVE_BEST_IMPROVEMENT_IS_REGRESSION");
        return reasons.AsReadOnly();
    }

    private static void ValidateReviewPacket(
        string reviewDirectory,
        Tp1EvaluationReviewRequestArtifact request,
        Tp1EvaluationReviewRunArtifact reviewRun,
        byte[] requestBytes,
        byte[] markdownBytes)
    {
        if (!StringComparer.Ordinal.Equals(request.Status, ReviewStatus)
            || !StringComparer.Ordinal.Equals(reviewRun.Status, ReviewStatus)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(request.ReviewPacketId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(
                request.ReviewPacketId)
            || !StringComparer.Ordinal.Equals(
                Path.GetFileName(reviewDirectory),
                request.ReviewPacketId)
            || !StringComparer.Ordinal.Equals(
                request.ReviewPacketId,
                reviewRun.ReviewPacketId)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                request.OrderingAuthorityRef)
            || !StringComparer.Ordinal.Equals(
                request.OrderingAuthorityRef,
                reviewRun.OrderingAuthorityRef)
            || request.OrderingVerified
            || reviewRun.OrderingVerified
            || !ValidReviewPosture(request)
            || !ValidReviewRunPosture(reviewRun)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                reviewRun.ReviewIndexSha256)
            || !StringComparer.Ordinal.Equals(
                reviewRun.ReviewRequestSha256,
                HashRef(requestBytes))
            || !StringComparer.Ordinal.Equals(
                reviewRun.ReviewMarkdownSha256,
                HashRef(markdownBytes))
            || reviewRun.PublishedArtifactLeafNames is null
            || !reviewRun.PublishedArtifactLeafNames.SequenceEqual(
                Tp1EvaluationReviewCli.ArtifactLeafNames,
                StringComparer.Ordinal)
            || request.Runs is null
            || request.AdjacentDeltas is null
            || request.Runs.Count is < 1 or > MaximumRuns
            || reviewRun.SourceArtifacts is null
            || reviewRun.SourceArtifacts.Count != request.Runs.Count)
            throw EvidenceMismatch();

        ValidateInvariants(request.Invariants);
        var runIds = new HashSet<string>(StringComparer.Ordinal);
        var sourceRunIds = new HashSet<string>(StringComparer.Ordinal);
        var windows = new HashSet<string>(StringComparer.Ordinal);
        var artifactTuples = new HashSet<string>(StringComparer.Ordinal);
        var totalCases = 0;
        var totalCandidates = 0;
        var totalWins = 0;
        var totalGoal = 0D;
        var totalBest = 0D;
        for (var ordinal = 0; ordinal < request.Runs.Count; ordinal++)
        {
            var run = request.Runs[ordinal];
            var manifestHashes = reviewRun.SourceArtifacts[ordinal];
            if (run is null
                || run.Ordinal != ordinal
                || manifestHashes is null
                || manifestHashes.Ordinal != ordinal
                || run.SourceArtifactHashes is null
                || run.SourceArtifactHashes.Ordinal != ordinal
                || run.SourceArtifactHashes != manifestHashes
                || !ValidRun(run, request.Invariants))
                throw EvidenceMismatch();
            var hashes = run.SourceArtifactHashes;
            var tuple = string.Join('|',
                hashes.CandidateScoresSha256,
                hashes.CaseComparisonsSha256,
                hashes.TeacherStageGapReportSha256,
                hashes.ScoringRunManifestSha256);
            if (!runIds.Add(run.RunId)
                || !sourceRunIds.Add(run.SourceRunId)
                || !windows.Add(run.WindowRef)
                || !artifactTuples.Add(tuple))
                throw EvidenceMismatch();
            totalCases = checked(totalCases + run.CaseCount);
            totalCandidates = checked(totalCandidates + run.SemanticCandidateCount);
            totalWins = checked(totalWins + run.StrictTeacherWins);
            totalGoal += run.HeldOutAccuracyYgoal;
            totalBest += run.HeldOutAccuracyYbest;
        }

        if (request.AdjacentDeltas.Count != request.Runs.Count - 1)
            throw EvidenceMismatch();
        for (var index = 0; index < request.AdjacentDeltas.Count; index++)
        {
            var delta = request.AdjacentDeltas[index];
            var from = request.Runs[index];
            var to = request.Runs[index + 1];
            if (delta is null
                || delta.FromOrdinal != index
                || delta.ToOrdinal != index + 1
                || delta.DeltaHeldOutAccuracyYgoal
                    != to.HeldOutAccuracyYgoal - from.HeldOutAccuracyYgoal
                || delta.DeltaHeldOutAccuracyYbest
                    != to.HeldOutAccuracyYbest - from.HeldOutAccuracyYbest
                || delta.DeltaTeacherGapToGold
                    != to.TeacherGapToGold - from.TeacherGapToGold
                || delta.DeltaTeacherBeatRate
                    != to.TeacherBeatRate - from.TeacherBeatRate)
                throw EvidenceMismatch();
        }

        var aggregate = request.Aggregate;
        var meanGoal = totalGoal / request.Runs.Count;
        var meanBest = totalBest / request.Runs.Count;
        if (aggregate is null
            || aggregate.RunCount != request.Runs.Count
            || aggregate.TotalCaseObservations != totalCases
            || aggregate.TotalSemanticCandidates != totalCandidates
            || aggregate.PooledStrictTeacherWins != totalWins
            || aggregate.MeanHeldOutAccuracyYgoal != meanGoal
            || aggregate.MeanHeldOutAccuracyYbest != meanBest
            || aggregate.AggregateTeacherGapToGold != 1D - meanGoal
            || aggregate.PooledTeacherBeatRate != (double)totalWins / totalCases)
            throw EvidenceMismatch();
    }

    private static bool ValidReviewPosture(
        Tp1EvaluationReviewRequestArtifact request)
        => request.EvalOnly
            && !request.IndependentAcceptance
            && !request.Tp1ExitGateEvaluated
            && !request.LocalTeacherCeilingEvaluated
            && !request.DpoTrainReady
            && !request.TeacherOutputUsedForTraining
            && !request.TrainingStarted
            && request.DpoStepsExecuted == 0
            && !request.CheckpointPromoted
            && request.HumanDecisionRequired;

    private static bool ValidReviewRunPosture(
        Tp1EvaluationReviewRunArtifact run)
        => run.EvalOnly
            && !run.IndependentAcceptance
            && !run.Tp1ExitGateEvaluated
            && !run.LocalTeacherCeilingEvaluated
            && !run.DpoTrainReady
            && !run.TeacherOutputUsedForTraining
            && !run.TrainingStarted
            && run.DpoStepsExecuted == 0
            && !run.CheckpointPromoted
            && run.HumanDecisionRequired;

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

    private static bool ValidRun(
        Tp1ReviewRunSummary run,
        Tp1ReviewInvariantSet invariants)
    {
        if (!Tp1EvaluationEvidencePacketIo.IsSafeIdentity(run.RunId)
            || !Tp1EvaluationEvidencePacketIo.IsSafeIdentity(run.SourceRunId)
            || !Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(run.WindowRef)
            || run.CaseCount != invariants.CaseCount
            || run.CaseCount <= 0
            || run.SemanticCandidateCount <= 0
            || (long)run.CaseCount + run.SemanticCandidateCount > MaximumRowsPerRun
            || !FiniteUnit(run.HeldOutAccuracyYgoal)
            || !FiniteUnit(run.HeldOutAccuracyYbest)
            || !FiniteUnit(run.TeacherGapToGold)
            || !FiniteUnit(run.TeacherBeatRate)
            || run.TeacherGapToGold != 1D - run.HeldOutAccuracyYgoal
            || run.StrictTeacherWins is < 0
            || run.StrictTeacherWins > run.CaseCount
            || run.TeacherBeatRate != (double)run.StrictTeacherWins / run.CaseCount
            || !StringComparer.Ordinal.Equals(
                run.EvaluationRosterSha256,
                invariants.EvaluationRosterSha256)
            || !MeasurementNoiseReportIsValid(run)
            || run.SourceArtifactHashes is null)
            return false;
        var hashes = run.SourceArtifactHashes;
        return Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.CandidateScoresSha256)
            && Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.CaseComparisonsSha256)
            && Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.TeacherStageGapReportSha256)
            && Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(
                hashes.ScoringRunManifestSha256);
    }

    private static bool MeasurementNoiseReportIsValid(Tp1ReviewRunSummary run)
        // Пока TP1 ceiling не получает настоящий passport-bound calibration,
        // любой пол шума или заявленный outcome был бы неподписанным повышением доверия.
        => run.MeasurementNoiseFloor is null
            && string.Equals(
                run.MeasurementNoiseStatus,
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                StringComparison.Ordinal);

    private static Tp1EvaluationReviewRequestArtifact ParseReviewRequest(byte[] bytes)
    {
        var request = ParseCanonical<Tp1EvaluationReviewRequestArtifact>(
            bytes,
            ReviewRequestProperties,
            static root =>
            {
                ValidateExactObject(root.GetProperty("invariants"), InvariantProperties);
                ValidateArrayObjects(
                    root.GetProperty("runs"),
                    ReviewRunSummaryProperties,
                    static run => ValidateExactObject(
                        run.GetProperty("source_artifact_hashes"),
                        SourceArtifactHashProperties));
                ValidateArrayObjects(
                    root.GetProperty("adjacent_deltas"),
                    AdjacentDeltaProperties);
                ValidateExactObject(root.GetProperty("aggregate"), AggregateProperties);
            });
        return request;
    }

    private static Tp1EvaluationReviewRunArtifact ParseReviewRun(byte[] bytes)
        => ParseCanonical<Tp1EvaluationReviewRunArtifact>(
            bytes,
            ReviewRunProperties,
            static root => ValidateArrayObjects(
                root.GetProperty("source_artifacts"),
                SourceArtifactHashProperties));

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
        catch (Tp1CeilingException)
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
        HashSet<string> properties,
        Action<JsonElement>? validateNested = null)
    {
        if (array.ValueKind != JsonValueKind.Array)
            throw InputInvalid();
        foreach (var item in array.EnumerateArray())
        {
            ValidateExactObject(item, properties);
            validateNested?.Invoke(item);
        }
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

    private static ParsedArguments ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["--review-packet-directory"] = null,
            ["--output-root"] = null,
            ["--evaluation-id"] = null,
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
            values["--review-packet-directory"]!,
            values["--output-root"]!,
            values["--evaluation-id"]!);
    }

    private static async Task PublishAsync(
        string outputRoot,
        string physicalOutputRoot,
        string evaluationId,
        byte[] evaluationBytes,
        byte[] markdownBytes,
        byte[] runBytes,
        Tp1RetainedEvidencePacket packet,
        string reviewDirectory,
        string physicalReviewDirectory,
        Tp1LocalTeacherCeilingDependencies dependencies,
        CancellationToken cancellationToken)
    {
        IEvaluationPublicationBoundary? publication = null;
        var published = false;
        try
        {
            var created = dependencies.CreatePublication(
                outputRoot,
                evaluationId,
                ArtifactLeafNames);
            if (!created.IsSuccess || created.Value is null)
                throw PublicationFailed();
            publication = created.Value;
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            await WriteBytesAsync(
                    publication,
                    EvaluationLeaf,
                    evaluationBytes,
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
                    EvaluationRunLeaf,
                    runBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            VerifyPublicationSourceAuthority(
                packet,
                reviewDirectory,
                physicalReviewDirectory,
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
        catch (Tp1CeilingException)
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
        Tp1LocalTeacherCeilingEvaluationArtifact evaluation)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Числовая проверка кандидата на потолок локального учителя TP1");
        builder.AppendLine();
        builder.AppendLine($"Статус: `{FixedStatus}`.");
        builder.AppendLine($"Политика: `{FixedPolicyId}`; окно — последние {WindowSize} прогона.");
        builder.AppendLine();
        builder.AppendLine("## Формулы и строгие границы");
        builder.AppendLine();
        builder.AppendLine($"Плато: `max(yGoal[1], yGoal[2]) - yGoal[0] < {Format(MinimumBestImprovement)}`. Равенство порогу не проходит: оператор строго `<`.");
        builder.AppendLine($"Низкая доля побед учителя: `checked(wins × {BeatNumeratorMultiplier}) < cases × {BeatDenominatorThreshold}`. Равенство одной десятой не проходит.");
        builder.AppendLine("Десятичная pooled beat rate показана описательно; решение принято целочисленным сравнением без ошибки округления.");
        builder.AppendLine();
        builder.AppendLine("## Измеренный результат");
        builder.AppendLine();
        builder.AppendLine($"Первый yGoal окна: {Format(evaluation.FirstHeldOutAccuracyYgoal)}.");
        builder.AppendLine($"Лучший последующий yGoal: {Format(evaluation.BestSubsequentHeldOutAccuracyYgoal)}.");
        builder.AppendLine($"Лучшее улучшение: {Format(evaluation.BestImprovement)}; plateau={evaluation.PlateauPredicate.ToString().ToLowerInvariant()}.");
        builder.AppendLine($"Строгие победы/случаи: {evaluation.SelectedStrictTeacherWins}/{evaluation.SelectedCaseCount}; описательная доля={Format(evaluation.PooledTeacherBeatRateDescriptive)}; lowBeatAuthority={evaluation.LowBeatAuthorityPredicate.ToString().ToLowerInvariant()}.");
        builder.AppendLine($"localTeacherCeilingCandidate={evaluation.LocalTeacherCeilingCandidate.ToString().ToLowerInvariant()}.");
        builder.AppendLine("Отрицательное улучшение означает регрессию, а не дополнительное доказательство качества; формально оно лишь удовлетворяет числовому предикату плато.");
        builder.AppendLine();
        builder.AppendLine("## Ограничения и запреты");
        builder.AppendLine();
        builder.AppendLine("Порядок последних трёх прогонов взят из неподписанного исходного review-пакета: sourceOrderingVerified=false. Хеши подтверждают байты, но не авторство.");
        builder.AppendLine("Кандидат не является независимой приёмкой, PASS или TP1 exit: independentAcceptance=false; passed=false; tp1ExitGateEvaluated=false.");
        builder.AppendLine("TP2 не разрешён. DPO-ready=false; обучение не начиналось; DPO-шагов 0; checkpoint не продвигался.");
        builder.AppendLine("localTeacherCeilingEvaluated=true означает только то, что числовые предикаты действительно вычислены.");
        builder.AppendLine();
        builder.AppendLine("## Решение остаётся за Любой");
        builder.AppendLine();
        builder.AppendLine("humanDecisionRequired=true. Этот eval-only пакет не заменяет человеческое решение.");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
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
        Tp1LocalTeacherCeilingDependencies dependencies)
    {
        if (!dependencies.RejectOutputUnderProjects)
            return;
        var projects = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectsRoot));
        if (PathComparer.Equals(outputRoot, projects)
            || Tp1EvaluationEvidencePacketIo.IsContainedBy(outputRoot, projects))
            throw InputInvalid();
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

    private static void VerifyPublicationSourceAuthority(
        Tp1RetainedEvidencePacket packet,
        string reviewDirectory,
        string expectedPhysical,
        Tp1LocalTeacherCeilingDependencies dependencies)
    {
        try
        {
            VerifyPacketUnchanged(packet);
            VerifySourcePhysicalIdentity(
                reviewDirectory,
                expectedPhysical,
                dependencies);
        }
        catch (Tp1CeilingException exception)
        {
            throw PublicationFailed(exception);
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

    private static string RequirePhysical(
        Tp1LocalTeacherCeilingDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var result = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw InputInvalid();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static void VerifySourcePhysicalIdentity(
        string reviewDirectory,
        string expectedPhysical,
        Tp1LocalTeacherCeilingDependencies dependencies)
    {
        var physical = RequirePhysical(
            dependencies,
            reviewDirectory,
            allowMissing: false);
        if (!PathComparer.Equals(physical, expectedPhysical))
            throw EvidenceMismatch();
    }

    private static bool FiniteUnit(double value)
        => double.IsFinite(value) && value is >= 0D and <= 1D;

    internal static bool IsStrictPlateau(
        double firstHeldOutAccuracyYgoal,
        double secondHeldOutAccuracyYgoal,
        double thirdHeldOutAccuracyYgoal)
        => Math.Max(
                secondHeldOutAccuracyYgoal,
                thirdHeldOutAccuracyYgoal)
            - firstHeldOutAccuracyYgoal < MinimumBestImprovement;

    internal static bool IsLowBeatAuthority(long strictWins, long caseCount)
        => checked(strictWins * BeatNumeratorMultiplier)
            < checked(caseCount * BeatDenominatorThreshold);

    private static string Format(double value)
        => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

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
            "TP1_CEILING_INPUT_INVALID" => "The TP1 ceiling input is invalid.",
            "TP1_CEILING_EVIDENCE_MISMATCH" => "The TP1 ceiling evidence is inconsistent.",
            "TP1_CEILING_WINDOW_INCOMPLETE" => "The TP1 ceiling window requires at least three runs.",
            "TP1_CEILING_CANCELLED" => "The TP1 ceiling evaluation was cancelled.",
            _ => "The TP1 ceiling publication failed.",
        };
        await stderr.WriteLineAsync(JsonSerializer.Serialize(
                new { ok = false, error_code = code, message }))
            .ConfigureAwait(false);
    }

    private static Tp1CeilingException InputInvalid(Exception? inner = null)
        => new("TP1_CEILING_INPUT_INVALID", inner);

    private static Tp1CeilingException EvidenceMismatch(Exception? inner = null)
        => new("TP1_CEILING_EVIDENCE_MISMATCH", inner);

    private static Tp1CeilingException WindowIncomplete(Exception? inner = null)
        => new("TP1_CEILING_WINDOW_INCOMPLETE", inner);

    private static Tp1CeilingException PublicationFailed(Exception? inner = null)
        => new("TP1_CEILING_PUBLICATION_FAILED", inner);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private sealed record ParsedArguments(
        string ReviewPacketDirectory,
        string OutputRoot,
        string EvaluationId);

    private sealed class Tp1CeilingException : Exception
    {
        internal Tp1CeilingException(string errorCode, Exception? inner)
            : base(errorCode, inner) => ErrorCode = errorCode;

        internal string ErrorCode { get; }
    }
}

internal sealed class Tp1LocalTeacherCeilingDependencies
{
    internal Tp1LocalTeacherCeilingDependencies(
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

    internal static Tp1LocalTeacherCeilingDependencies Production { get; } = new();
}
