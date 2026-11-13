// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;
using TranslationDomain;

internal static class RunnerModeCli
{
    internal static Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
    {
        var evaluation = args.Contains("--execute-evaluation", StringComparer.Ordinal);
        var scoring = args.Contains(OfflineScoringCli.ArmFlag, StringComparer.Ordinal);
        var review = args.Contains(Tp1EvaluationReviewCli.ArmFlag, StringComparer.Ordinal);
        var ceiling = args.Contains(
            Tp1LocalTeacherCeilingCli.ArmFlag,
            StringComparer.Ordinal);
        var exitDecisionRequest = args.Contains(
            Tp1ExitDecisionRequestCli.ArmFlag,
            StringComparer.Ordinal);
        var exitDecisionSubmissionValidation = args.Contains(
            Tp1ExitDecisionSubmissionValidationCli.ArmFlag,
            StringComparer.Ordinal);
        if ((evaluation ? 1 : 0)
            + (scoring ? 1 : 0)
            + (review ? 1 : 0)
            + (ceiling ? 1 : 0)
            + (exitDecisionRequest ? 1 : 0)
            + (exitDecisionSubmissionValidation ? 1 : 0) != 1)
            return OfflineScoringCli.WriteModeErrorAsync(stderr);
        if (scoring)
            return OfflineScoringCli.RunAsync(args, stdout, stderr, cancellationToken);
        if (ceiling)
            return Tp1LocalTeacherCeilingCli.RunAsync(
                args,
                stdout,
                stderr,
                cancellationToken);
        if (exitDecisionRequest)
            return Tp1ExitDecisionRequestCli.RunAsync(
                args,
                stdout,
                stderr,
                cancellationToken);
        if (exitDecisionSubmissionValidation)
            return Tp1ExitDecisionSubmissionValidationCli.RunAsync(
                args,
                stdout,
                stderr,
                cancellationToken);
        return review
            ? Tp1EvaluationReviewCli.RunAsync(args, stdout, stderr, cancellationToken)
            : EvaluationCli.RunAsync(args, stdout, stderr, cancellationToken);
    }
}

internal static class OfflineScoringCli
{
    internal const string ArmFlag = "--execute-offline-scoring";
    internal const string CandidateScoresLeaf =
        "tp1-evaluation-candidate-scores.jsonl";
    internal const string CaseComparisonsLeaf =
        "tp1-evaluation-case-comparisons.jsonl";
    internal const string StageGapReportLeaf =
        "tp1-teacher-stage-gap-report.json";
    internal const string ScoringManifestLeaf =
        "tp1-evaluation-scoring-run.json";

    private const string SourceObservationLeaf =
        "tp1-evaluation-observations.jsonl";
    private const string SourceManifestLeaf = "tp1-evaluation-run.json";
    private const string ProjectsRoot = @"C:\Projects";
    private const string Stage = "TP1_LOCAL";
    private const int MaximumRows = 4_096;
    private const int MaximumTeacherOutputUtf8Bytes = 262_144;
    private const long MaximumFileBytes = 16L * 1024 * 1024;
    private const int MaximumLineBytes = 196_608;
    private const long MaximumAggregateScoringCells = 32_000_000;

    internal static IReadOnlyList<string> ArtifactLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            CandidateScoresLeaf,
            CaseComparisonsLeaf,
            StageGapReportLeaf,
            ScoringManifestLeaf,
        });

    private static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions CamelJson = CreateJsonOptions(
        JsonNamingPolicy.CamelCase);
    private static readonly JsonSerializerOptions SnakeJson = CreateJsonOptions(
        JsonNamingPolicy.SnakeCaseLower);
    private static StringComparer PathComparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static readonly string[] RequiredOptions =
    [
        "--source-run-directory",
        "--comparison-jsonl",
        "--split-proof-json",
        "--output-root",
        "--run-id",
        "--audit-salt-base64",
        "--scorer-id",
        "--scorer-version",
        "--threshold-set-id",
        "--window-ref",
        "--expected-model-id",
        "--expected-ollama-manifest-digest",
        "--expected-ollama-model-layer-digest",
        "--expected-local-retention-policy-ref",
        "--expected-output-license-ref",
        "--expected-teacher-license-evidence-ref",
        "--expected-deployment-evidence-ref",
    ];

    private static readonly HashSet<string> ComparisonProperties = new(
    [
        "evaluationSetId", "caseId", "domainId", "scorerId", "scorerVersion",
        "thresholdSetId", "sourceRef", "split", "expectedLabel",
        "chosenTranslation", "goldOrigin", "goldLicense", "goldAuthorityRef",
        "goldFromNodeUnderTest", "semanticQualityClaimed", "learnerBestOutput",
        "learnerBestOutputSha256", "learnerBestArtifactRef",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SplitProofProperties = new(
    [
        "evaluationSetId", "splitId", "trainingSourceRefs",
        "evaluationSourceRefs", "crossSplitNearDuplicateCount",
        "nearDuplicateAlgorithm", "nearDuplicateThreshold", "proofArtifactRef",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SourceManifestProperties = new(
    [
        "run_id", "input_row_count", "teacher_calls_executed", "model_id",
        "ollama_manifest_digest", "ollama_model_layer_digest",
        "local_retention_policy_ref",
        "output_license_ref", "teacher_license_evidence_ref",
        "deployment_evidence_ref", "dpo_train_ready",
        "teacher_output_used_for_training", "training_started",
        "dpo_steps_executed", "checkpoint_promoted", "scoring_executed",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> SourceObservationProperties = new(
    [
        "evaluation_set_id", "case_id", "domain_id", "generation_index",
        "scorer_id", "source_ref", "split", "output", "provenance",
        "dpo_train_ready", "teacher_output_used_for_training",
        "training_started", "dpo_steps_executed", "checkpoint_promoted",
        "scoring_executed",
    ], StringComparer.Ordinal);

    private static readonly HashSet<string> ProvenanceProperties = new(
    [
        "source_kind", "family", "access_mode", "hosting_provider", "model_id",
        "model_version", "model_license", "provider_terms_checked",
        "provider_terms_allow_training", "closed_proprietary_model",
        "teacher_output_used_for_training", "request_id", "invocation_count",
        "model_artifact_digest_kind", "model_artifact_digest",
        "training_content_sha256",
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
            OfflineScoringDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        OfflineScoringDependencies dependencies)
    {
        try
        {
            var success = await ExecuteAsync(args, dependencies, cancellationToken)
                .ConfigureAwait(false);
            await stdout.WriteLineAsync(JsonSerializer.Serialize(success, SnakeJson))
                .ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(stderr, "TP1_OFFLINE_SCORING_CANCELLED",
                    "Offline scoring was cancelled.")
                .ConfigureAwait(false);
            return 130;
        }
        catch (EvaluationRunnerException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode, exception.Message)
                .ConfigureAwait(false);
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(stderr, "TP1_OFFLINE_SCORING_FAILED",
                    "Offline scoring failed before publication completed.")
                .ConfigureAwait(false);
            return 1;
        }
    }

    internal static async Task<int> WriteModeErrorAsync(TextWriter stderr)
    {
        await WriteErrorAsync(
                stderr,
                "TP1_RUNNER_ARM_INVALID",
                "Exactly one TP1 runner arm flag is required.")
            .ConfigureAwait(false);
        return 1;
    }

    private static async Task<OfflineScoringSuccess> ExecuteAsync(
        string[] args,
        OfflineScoringDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
        {
            throw Invalid("Windows handle-relative publication is required.");
        }

        var request = BuildRequest(ParseArguments(args));
        ValidatePaths(request, dependencies.RejectOutputUnderProjects);
        var finalDirectory = Path.Combine(request.OutputRoot, request.RunId);
        if (Directory.Exists(finalDirectory) || File.Exists(finalDirectory))
            throw Invalid("The scoring destination already exists.");

        var sourceObservationPath = Path.Combine(
            request.SourceRunDirectory,
            SourceObservationLeaf);
        var sourceManifestPath = Path.Combine(
            request.SourceRunDirectory,
            SourceManifestLeaf);
        var physicalPaths = ValidatePhysicalPaths(
            request,
            sourceObservationPath,
            sourceManifestPath,
            dependencies);
        using var sourceObservationsFile = OpenRetainedRead(sourceObservationPath);
        using var sourceManifestFile = OpenRetainedRead(sourceManifestPath);
        using var comparisonFile = OpenRetainedRead(request.ComparisonJsonl);
        using var splitProofFile = OpenRetainedRead(request.SplitProofJson);
        RevalidatePhysicalInputs(
            request,
            sourceObservationPath,
            sourceManifestPath,
            physicalPaths,
            dependencies);

        var sourceManifestBytes = ReadBounded(sourceManifestFile);
        var sourceObservationBytes = ReadBounded(sourceObservationsFile);
        var comparisonBytes = ReadBounded(comparisonFile);
        var splitProofBytes = ReadBounded(splitProofFile);
        var sourceManifest = ParseSourceManifest(sourceManifestBytes);
        ValidateSourceManifest(sourceManifest, request);
        var sourceRows = ParseSourceObservations(sourceObservationBytes);
        var comparisons = ParseComparisons(comparisonBytes);
        var splitProof = ParseSplitProof(splitProofBytes);
        var scored = BuildScoringArtifacts(
            request,
            sourceManifest,
            sourceRows,
            comparisons,
            splitProof,
            cancellationToken);

        Directory.CreateDirectory(request.OutputRoot);
        RejectExistingReparseSegments(request.OutputRoot);
        RevalidatePhysicalOutput(request, physicalPaths, dependencies);
        IEvaluationPublicationBoundary? publication = null;
        var accepted = false;
        try
        {
            var created = dependencies.CreatePublication(
                request.OutputRoot,
                request.RunId,
                ArtifactLeafNames);
            if (!created.IsSuccess || created.Value is null)
                throw Invalid("The scoring publication authority is unavailable.");
            publication = created.Value;
            publication.VerifyOutputRootIdentity(physicalPaths.OutputRoot);
            publication.VerifyStagingIdentity();
            await WriteJsonLinesAsync(
                    publication,
                    CandidateScoresLeaf,
                    scored.CandidateScores,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteJsonLinesAsync(
                    publication,
                    CaseComparisonsLeaf,
                    scored.CaseComparisons,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteJsonAsync(
                    publication,
                    StageGapReportLeaf,
                    scored.StageGapReport,
                    cancellationToken)
                .ConfigureAwait(false);
            var manifest = scored.Manifest with
            {
                SourceManifestSha256 = HashRef(sourceManifestBytes),
                SourceObservationsSha256 = HashRef(sourceObservationBytes),
                ComparisonInputSha256 = HashRef(comparisonBytes),
                SplitProofSha256 = HashRef(splitProofBytes),
            };
            await WriteJsonAsync(
                    publication,
                    ScoringManifestLeaf,
                    manifest,
                    cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            RevalidatePhysicalInputs(
                request,
                sourceObservationPath,
                sourceManifestPath,
                physicalPaths,
                dependencies);
            RevalidatePhysicalOutput(request, physicalPaths, dependencies);
            publication.PublishAndVerify();
            accepted = true;
            return new OfflineScoringSuccess(
                true,
                request.RunId,
                scored.CaseComparisons.Count,
                finalDirectory);
        }
        finally
        {
            Exception? cleanupFailure = null;
            if (!accepted && publication is not null)
            {
                try { publication.CleanupFailedPublication(); }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    cleanupFailure = exception;
                }
            }
            publication?.Dispose();
            if (cleanupFailure is not null)
            {
                throw new EvaluationRunnerException(
                    "TP1_OFFLINE_SCORING_CLEANUP_FAILED",
                    "Offline scoring publication cleanup failed.",
                    cleanupFailure);
            }
        }
    }

    private static ScoringArtifacts BuildScoringArtifacts(
        OfflineScoringRequest request,
        EvaluationManifest sourceManifest,
        IReadOnlyList<EvaluationObservation> sourceRows,
        IReadOnlyList<ComparisonInputRow> comparisons,
        SplitProofInput splitProof,
        CancellationToken cancellationToken)
    {
        if (sourceRows.Count == 0 || sourceRows.Count != sourceManifest.InputRowCount
            || sourceManifest.TeacherCallsExecuted != sourceRows.Count)
            throw Invalid("Source evaluation row counts are inconsistent.");

        var authorityResult = LegalTrainingTeacherAdmissionAuthority.Create(
        [
            new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                request.ExpectedModelId,
                request.ExpectedOllamaManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                request.ExpectedLocalRetentionPolicyRef,
                request.ExpectedOutputLicenseRef)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = request.ExpectedOllamaModelLayerDigest
            },
        ], sourceManifest.TeacherCallsExecuted, request.AuditSalt);
        if (!authorityResult.IsSuccess || authorityResult.Value is null)
            throw Invalid("Source teacher authority could not be reconstructed.");
        var authority = authorityResult.Value;

        var sourceRequestIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in sourceRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateSourceObservation(row, sourceManifest, request, authority);
            if (!sourceRequestIds.Add(row.Provenance.RequestId))
            {
                throw Invalid(
                    "A source teacher request id is duplicated across observations.");
            }
        }
        foreach (var comparison in comparisons)
            ValidateComparison(comparison, request);
        ValidateSplitProof(splitProof, comparisons);

        var sourceGroups = sourceRows.GroupBy(SourceKey).ToDictionary(
            static group => group.Key,
            static group => group.OrderBy(row => row.GenerationIndex).ToArray());
        var comparisonGroups = comparisons.GroupBy(ComparisonKey).ToDictionary(
            static group => group.Key,
            static group => group.ToArray());
        if (sourceGroups.Count == 0
            || sourceGroups.Count != comparisonGroups.Count
            || sourceGroups.Keys.Any(key => !comparisonGroups.ContainsKey(key)))
            throw Invalid("Source and comparison case joins are incomplete.");

        var scorer = new LanguageTranslateHeldOutScorer(
            request.ScorerId,
            request.ScorerVersion);
        var candidateArtifacts = new List<CandidateScoreArtifact>();
        var caseArtifacts = new List<CaseComparisonArtifact>();
        var selectedTeacherRows = new List<HeldOutScoringRow>();
        var learnerRows = new List<HeldOutScoringRow>();
        var roster = new List<EvaluationRosterEntry>();
        int? inferredK = null;
        ExpectedLabelSource? expectedLabelSource = null;
        var teacherWins = 0;
        long reservedScoringCells = 0;

        foreach (var pair in sourceGroups.OrderBy(static pair => pair.Key))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = pair.Key;
            var sources = pair.Value;
            var candidates = comparisonGroups[key];
            ValidateCaseCardinality(sources, candidates, ref inferredK);
            var template = candidates[0];
            expectedLabelSource ??= new ExpectedLabelSource(
                template.GoldOrigin,
                template.GoldAuthorityRef,
                template.GoldFromNodeUnderTest,
                template.Split);
            if (expectedLabelSource != new ExpectedLabelSource(
                    template.GoldOrigin,
                    template.GoldAuthorityRef,
                    template.GoldFromNodeUnderTest,
                    template.Split))
                throw Invalid("Expected-label authority is mixed.");

            var sourceByOutput = sources.GroupBy(
                static row => row.Output,
                StringComparer.Ordinal).ToDictionary(
                    static group => group.Key,
                    static group => group.ToArray(),
                    StringComparer.Ordinal);

            var semanticCandidates = new List<ScoredSemanticCandidate>();
            foreach (var outputGroup in sourceByOutput.OrderBy(
                         static pair => pair.Key,
                         StringComparer.Ordinal))
            {
                ReserveScoringWork(
                    template.ExpectedLabel,
                    outputGroup.Key,
                    ref reservedScoringCells,
                    cancellationToken);
                var scoreResult = LanguageTranslateHeldOutScorer.ScoreOne(
                    template.ExpectedLabel,
                    outputGroup.Key,
                    cancellationToken);
                if (!scoreResult.IsSuccess || scoreResult.Value is null)
                    throw Invalid("A teacher candidate could not be scored.");
                var outputSha = HashJsonString(outputGroup.Key);
                var generations = outputGroup.Value
                    .Select(static row => row.GenerationIndex)
                    .Order()
                    .ToArray();
                var requestRefSha256s = outputGroup.Value
                    .Select(static row => HashJsonString(row.Provenance.RequestId))
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                semanticCandidates.Add(new ScoredSemanticCandidate(
                    outputGroup.Key,
                    outputSha,
                    scoreResult.Value,
                    generations,
                    requestRefSha256s));
                candidateArtifacts.Add(new CandidateScoreArtifact(
                    key.EvaluationSetId,
                    key.CaseId,
                    key.DomainId,
                    request.ScorerId,
                    request.ScorerVersion,
                    request.ThresholdSetId,
                    key.SourceRef,
                    outputSha,
                    scoreResult.Value.ExactMatch,
                    scoreResult.Value.CharacterAccuracy,
                    scoreResult.Value.CandidateScore,
                    generations,
                    requestRefSha256s,
                    EvalOnly: true));
            }

            var selected = semanticCandidates
                .OrderByDescending(static candidate => candidate.Score.CandidateScore)
                .ThenBy(static candidate => candidate.OutputSha256, StringComparer.Ordinal)
                .First();
            ReserveScoringWork(
                template.ExpectedLabel,
                template.LearnerBestOutput,
                ref reservedScoringCells,
                cancellationToken);
            var learnerScoreResult = LanguageTranslateHeldOutScorer.ScoreOne(
                template.ExpectedLabel,
                template.LearnerBestOutput,
                cancellationToken);
            if (!learnerScoreResult.IsSuccess || learnerScoreResult.Value is null)
                throw Invalid("The learner output could not be scored.");
            var learnerScore = learnerScoreResult.Value;
            var teacherBeatLearner = selected.Score.CandidateScore
                > learnerScore.CandidateScore;
            if (teacherBeatLearner)
                teacherWins++;
            selectedTeacherRows.Add(new HeldOutScoringRow(
                key.CaseId,
                template.ExpectedLabel,
                selected.RawOutput));
            learnerRows.Add(new HeldOutScoringRow(
                key.CaseId,
                template.ExpectedLabel,
                template.LearnerBestOutput));
            caseArtifacts.Add(new CaseComparisonArtifact(
                key.EvaluationSetId,
                key.CaseId,
                key.DomainId,
                request.ScorerId,
                request.ScorerVersion,
                request.ThresholdSetId,
                key.SourceRef,
                HashJsonString(template.ExpectedLabel),
                template.GoldAuthorityRef,
                selected.OutputSha256,
                selected.Score.CandidateScore,
                selected.GenerationIndices,
                selected.RequestRefSha256s,
                template.LearnerBestOutputSha256,
                template.LearnerBestArtifactRef,
                learnerScore.CandidateScore,
                teacherBeatLearner,
                EvalOnly: true));
            roster.Add(new EvaluationRosterEntry(
                key.EvaluationSetId,
                key.CaseId,
                key.DomainId,
                key.SourceRef,
                template.Split,
                HashJsonString(template.ExpectedLabel),
                template.GoldAuthorityRef));
        }

        if (expectedLabelSource is null || inferredK is null)
            throw Invalid("No complete held-out cases were supplied.");
        var evaluationSetIds = caseArtifacts
            .Select(static row => row.EvaluationSetId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (evaluationSetIds.Length != 1)
            throw Invalid("Exactly one evaluation set is required.");
        var evaluationSetId = evaluationSetIds[0];
        var teacherAggregate = scorer.Score(new HeldOutScoringRequest(
            evaluationSetId,
            LanguageTranslateHeldOutScorer.DomainTag,
            expectedLabelSource,
            selectedTeacherRows), cancellationToken);
        var learnerAggregate = scorer.Score(new HeldOutScoringRequest(
            evaluationSetId,
            LanguageTranslateHeldOutScorer.DomainTag,
            expectedLabelSource,
            learnerRows), cancellationToken);
        if (!teacherAggregate.IsSuccess || teacherAggregate.Value is null
            || !learnerAggregate.IsSuccess || learnerAggregate.Value is null)
            throw Invalid("Aggregate held-out scoring failed.");
        var yGoal = teacherAggregate.Value.RuntimeProductAccuracy;
        var yBest = learnerAggregate.Value.RuntimeProductAccuracy;
        var report = new TeacherStageGapReport(
            Stage,
            request.ScorerId,
            request.ScorerVersion,
            request.ThresholdSetId,
            yGoal,
            yBest,
            TeacherGapToGold: 1D - yGoal,
            TeacherBeatRate: (double)teacherWins / caseArtifacts.Count,
            StageGap9bVs235b: null,
            EvalOnly: true,
            request.WindowRef)
        {
            // Этот CLI не делает повторных калибровочных прогонов; отчёт
            // поэтому обязан явно сказать, что разность не доказана шумомером.
            MeasurementNoiseFloor = null,
            MeasurementNoiseStatus =
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus
        };
        var manifest = new OfflineScoringManifest(
            request.RunId,
            Stage,
            evaluationSetId,
            LanguageTranslateHeldOutScorer.DomainTag,
            request.ScorerId,
            request.ScorerVersion,
            request.ThresholdSetId,
            request.WindowRef,
            inferredK.Value,
            caseArtifacts.Count,
            candidateArtifacts.Count,
            splitProof.ProofArtifactRef,
            sourceManifest.RunId,
            request.ExpectedModelId,
            TeacherArtifactDigestKind: "ollama_manifest_sha256",
            request.ExpectedOllamaManifestDigest,
            request.ExpectedOllamaModelLayerDigest,
            request.ExpectedLocalRetentionPolicyRef,
            request.ExpectedOutputLicenseRef,
            request.ExpectedTeacherLicenseEvidenceRef,
            request.ExpectedDeploymentEvidenceRef,
            EvaluationRosterSha256: ComputeEvaluationRosterSha256(roster),
            SourceManifestSha256: string.Empty,
            SourceObservationsSha256: string.Empty,
            ComparisonInputSha256: string.Empty,
            SplitProofSha256: string.Empty,
            EvalOnly: true,
            ScoringExecuted: true,
            TeacherStageGapReportCreated: true,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            Tp1ExitGateEvaluated: false,
            LocalTeacherCeilingEvaluated: false,
            HumanDecisionRequired: true);
        return new ScoringArtifacts(
            candidateArtifacts
                .OrderBy(static row => row.EvaluationSetId, StringComparer.Ordinal)
                .ThenBy(static row => row.CaseId, StringComparer.Ordinal)
                .ThenBy(static row => row.OutputSha256, StringComparer.Ordinal)
                .ToArray(),
            caseArtifacts
                .OrderBy(static row => row.EvaluationSetId, StringComparer.Ordinal)
                .ThenBy(static row => row.CaseId, StringComparer.Ordinal)
                .ToArray(),
            report,
            manifest);
    }

    private static void ValidateSourceManifest(
        EvaluationManifest manifest,
        OfflineScoringRequest request)
    {
        if (!IsSafeIdentity(manifest.RunId)
            || !IsSafeWindowsLeafName(manifest.RunId)
            || !StringComparer.Ordinal.Equals(
                Path.GetFileName(request.SourceRunDirectory),
                manifest.RunId)
            || manifest.InputRowCount is <= 0 or > MaximumRows
            || manifest.TeacherCallsExecuted != manifest.InputRowCount
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                manifest.ModelId)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaManifestDigest(manifest.OllamaManifestDigest)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(manifest.OllamaModelLayerDigest)
            || !StringComparer.Ordinal.Equals(manifest.ModelId, request.ExpectedModelId)
            || !StringComparer.Ordinal.Equals(
                manifest.OllamaManifestDigest,
                request.ExpectedOllamaManifestDigest)
            || !StringComparer.Ordinal.Equals(
                manifest.OllamaModelLayerDigest,
                request.ExpectedOllamaModelLayerDigest)
            || !StringComparer.Ordinal.Equals(
                manifest.LocalRetentionPolicyRef,
                request.ExpectedLocalRetentionPolicyRef)
            || !StringComparer.Ordinal.Equals(
                manifest.OutputLicenseRef,
                request.ExpectedOutputLicenseRef)
            || !StringComparer.Ordinal.Equals(
                manifest.TeacherLicenseEvidenceRef,
                request.ExpectedTeacherLicenseEvidenceRef)
            || !StringComparer.Ordinal.Equals(
                manifest.DeploymentEvidenceRef,
                request.ExpectedDeploymentEvidenceRef)
            || !IsCanonicalEvidenceRef(manifest.LocalRetentionPolicyRef)
            || !IsCanonicalEvidenceRef(manifest.OutputLicenseRef)
            || !IsCanonicalEvidenceRef(manifest.TeacherLicenseEvidenceRef)
            || !IsCanonicalEvidenceRef(manifest.DeploymentEvidenceRef)
            || manifest.DpoTrainReady
            || manifest.TeacherOutputUsedForTraining
            || manifest.TrainingStarted
            || manifest.DpoStepsExecuted != 0
            || manifest.CheckpointPromoted
            || manifest.ScoringExecuted)
            throw Invalid("The source evaluation manifest is not eval-only authority.");
    }

    private static void ValidateSourceObservation(
        EvaluationObservation row,
        EvaluationManifest manifest,
        OfflineScoringRequest request,
        LegalTrainingTeacherAdmissionAuthority authority)
    {
        if (!IsSafeIdentity(row.EvaluationSetId)
            || !IsSafeIdentity(row.CaseId)
            || row.GenerationIndex < 0
            || !StringComparer.Ordinal.Equals(
                row.DomainId,
                LanguageTranslateHeldOutScorer.DomainTag)
            || !StringComparer.Ordinal.Equals(row.ScorerId, request.ScorerId)
            || !IsCanonicalEvidenceRef(row.SourceRef)
            || !StringComparer.Ordinal.Equals(
                row.Split,
                LanguageTranslateHeldOutScorer.HeldOutSplit)
            || string.IsNullOrWhiteSpace(row.Output)
            || row.Output.Any(static character => character == '\0')
            || StrictUtf8.GetByteCount(row.Output) > MaximumTeacherOutputUtf8Bytes
            || row.DpoTrainReady
            || row.TeacherOutputUsedForTraining
            || row.TrainingStarted
            || row.DpoStepsExecuted != 0
            || row.CheckpointPromoted
            || row.ScoringExecuted
            || row.Provenance is null
            || row.Provenance.SourceKind
                != TrainingTeacherSourceKind.ExternalTeacherOutput
            || row.Provenance.Family != OpenWeightTeacherFamily.Qwen
            || row.Provenance.AccessMode
                != TrainingTeacherAccessMode.LocalHostedOpenWeight
            || row.Provenance.HostingProvider
                != TrainingTeacherHostingProvider.Ollama
            || row.Provenance.ModelLicense != TrainingTeacherModelLicense.Apache20
            || !StringComparer.Ordinal.Equals(row.Provenance.ModelId, manifest.ModelId)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelId,
                request.ExpectedModelId)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelVersion,
                manifest.OllamaManifestDigest)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelVersion,
                request.ExpectedOllamaManifestDigest)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelArtifactDigestKind,
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelArtifactDigest,
                manifest.OllamaModelLayerDigest)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.ModelArtifactDigest,
                request.ExpectedOllamaModelLayerDigest)
            || !StringComparer.Ordinal.Equals(
                row.Provenance.TrainingContentSha256,
                string.Empty)
            || !IsLowerHex(row.Provenance.RequestId, 32)
            || row.Provenance.InvocationCount != 1)
            throw Invalid("A source evaluation observation is not exact local eval-only provenance.");
        var evaluation = authority.ValidateLocalEvaluationOutput(row.Provenance);
        var training = authority.ValidateBatch([row.Provenance]);
        if (!evaluation.IsSuccess
            || training.IsSuccess
            || !StringComparer.Ordinal.Equals(
                training.ErrorCode,
                LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode))
            throw Invalid("Source provenance did not preserve the legal evaluation boundary.");
    }

    private static void ValidateComparison(
        ComparisonInputRow row,
        OfflineScoringRequest request)
    {
        if (!IsSafeIdentity(row.EvaluationSetId)
            || !IsSafeIdentity(row.CaseId)
            || !StringComparer.Ordinal.Equals(
                row.DomainId,
                LanguageTranslateHeldOutScorer.DomainTag)
            || !StringComparer.Ordinal.Equals(row.ScorerId, request.ScorerId)
            || !StringComparer.Ordinal.Equals(row.ScorerVersion, request.ScorerVersion)
            || !StringComparer.Ordinal.Equals(row.ThresholdSetId, request.ThresholdSetId)
            || !IsCanonicalEvidenceRef(row.SourceRef)
            || !StringComparer.Ordinal.Equals(row.Split, "held-out")
            || row.ExpectedLabel is null
            || row.ChosenTranslation is null
            || !StringComparer.Ordinal.Equals(row.ExpectedLabel, row.ChosenTranslation)
            || !StringComparer.Ordinal.Equals(
                row.GoldOrigin,
                LanguageTranslateHeldOutScorer.SelfAuthoredOrigin)
            || !StringComparer.Ordinal.Equals(
                row.GoldLicense,
                "project_internal_self_authored")
            || !IsCanonicalEvidenceRef(row.GoldAuthorityRef)
            || row.GoldFromNodeUnderTest
            || row.SemanticQualityClaimed
            || row.LearnerBestOutput is null
            || !StringComparer.Ordinal.Equals(
                row.LearnerBestOutputSha256,
                HashJsonString(row.LearnerBestOutput))
            || !IsCanonicalEvidenceRef(row.LearnerBestArtifactRef))
            throw Invalid("A comparison row violates the offline scoring contract.");
    }

    private static void ValidateSplitProof(
        SplitProofInput proof,
        IReadOnlyList<ComparisonInputRow> comparisons)
    {
        var evaluationSets = comparisons
            .Select(static row => row.EvaluationSetId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (evaluationSets.Length != 1
            || !StringComparer.Ordinal.Equals(proof.EvaluationSetId, evaluationSets[0])
            || !StringComparer.Ordinal.Equals(proof.SplitId, "held-out")
            || proof.TrainingSourceRefs is null
            || proof.EvaluationSourceRefs is null
            || proof.TrainingSourceRefs.Count == 0
            || proof.EvaluationSourceRefs.Count == 0
            || proof.TrainingSourceRefs.Any(reference => !IsCanonicalEvidenceRef(reference))
            || proof.EvaluationSourceRefs.Any(reference => !IsCanonicalEvidenceRef(reference))
            || proof.TrainingSourceRefs.Distinct(StringComparer.Ordinal).Count()
                != proof.TrainingSourceRefs.Count
            || proof.EvaluationSourceRefs.Distinct(StringComparer.Ordinal).Count()
                != proof.EvaluationSourceRefs.Count
            || proof.TrainingSourceRefs.Intersect(
                proof.EvaluationSourceRefs,
                StringComparer.Ordinal).Any()
            || proof.CrossSplitNearDuplicateCount != 0
            || !IsSafeIdentity(proof.NearDuplicateAlgorithm)
            || proof.NearDuplicateThreshold != 0.85D
            || !IsCanonicalEvidenceRef(proof.ProofArtifactRef)
            || !new HashSet<string>(
                    proof.EvaluationSourceRefs,
                    StringComparer.Ordinal)
                .SetEquals(comparisons.Select(static row => row.SourceRef)))
            throw Invalid("The held-out split proof is incomplete or contaminated.");
    }

    private static void ValidateCaseCardinality(
        EvaluationObservation[] sources,
        ComparisonInputRow[] comparisons,
        ref int? inferredK)
    {
        if (sources.Length == 0 || comparisons.Length != 1)
            throw Invalid("K is zero or the case-level comparison is not unique.");
        var indices = sources.Select(static row => row.GenerationIndex).Order().ToArray();
        if (indices.Distinct().Count() != indices.Length
            || !indices.SequenceEqual(Enumerable.Range(0, indices.Length)))
            throw Invalid("Generation indices are duplicated or noncontiguous.");
        inferredK ??= sources.Length;
        if (inferredK.Value != sources.Length)
            throw Invalid("Mixed K is not permitted.");
    }

    private static CaseJoinKey SourceKey(EvaluationObservation row)
        => new(
            row.EvaluationSetId,
            row.CaseId,
            row.DomainId,
            row.ScorerId,
            row.SourceRef,
            row.Split);

    private static CaseJoinKey ComparisonKey(ComparisonInputRow row)
        => new(
            row.EvaluationSetId,
            row.CaseId,
            row.DomainId,
            row.ScorerId,
            row.SourceRef,
            row.Split);

    private static EvaluationManifest ParseSourceManifest(byte[] bytes)
    {
        using var document = ParseSingleJson(bytes);
        RequireExactProperties(document.RootElement, SourceManifestProperties);
        return JsonSerializer.Deserialize<EvaluationManifest>(bytes, SnakeJson)
            ?? throw Invalid("Source manifest JSON is invalid.");
    }

    private static IReadOnlyList<EvaluationObservation> ParseSourceObservations(
        byte[] bytes)
        => ParseJsonLines(bytes, (element, raw) =>
        {
            RequireExactProperties(element, SourceObservationProperties);
            if (!element.TryGetProperty("provenance", out var provenance))
                throw Invalid("Source provenance is missing.");
            RequireExactProperties(provenance, ProvenanceProperties);
            return JsonSerializer.Deserialize<EvaluationObservation>(raw, SnakeJson)
                ?? throw Invalid("Source observation JSON is invalid.");
        });

    private static IReadOnlyList<ComparisonInputRow> ParseComparisons(byte[] bytes)
        => ParseJsonLines(bytes, (element, raw) =>
        {
            RequireExactProperties(element, ComparisonProperties);
            return JsonSerializer.Deserialize<ComparisonInputRow>(raw, CamelJson)
                ?? throw Invalid("Comparison JSON is invalid.");
        });

    private static SplitProofInput ParseSplitProof(byte[] bytes)
    {
        using var document = ParseSingleJson(bytes);
        RequireExactProperties(document.RootElement, SplitProofProperties);
        return JsonSerializer.Deserialize<SplitProofInput>(bytes, CamelJson)
            ?? throw Invalid("Split-proof JSON is invalid.");
    }

    private static IReadOnlyList<T> ParseJsonLines<T>(
        byte[] bytes,
        Func<JsonElement, string, T> parse)
    {
        RejectBom(bytes);
        var text = StrictUtf8.GetString(bytes);
        var rows = new List<T>();
        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.EndsWith('\r') ? rawLine[..^1] : rawLine;
            if (line.Length == 0)
                continue;
            if (StrictUtf8.GetByteCount(line) > MaximumLineBytes
                || rows.Count >= MaximumRows)
                throw Invalid("A scoring JSONL limit was exceeded.");
            using var document = JsonDocument.Parse(line, StrictDocumentOptions);
            rows.Add(parse(document.RootElement, line));
        }
        if (rows.Count == 0)
            throw Invalid("At least one scoring row is required.");
        return rows;
    }

    private static JsonDocument ParseSingleJson(byte[] bytes)
    {
        RejectBom(bytes);
        return JsonDocument.Parse(bytes, StrictDocumentOptions);
    }

    private static readonly JsonDocumentOptions StrictDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 32,
    };

    private static void RequireExactProperties(
        JsonElement element,
        HashSet<string> expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw Invalid("A strict JSON object is required.");
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!actual.Add(property.Name))
                throw Invalid("Duplicate JSON properties are not permitted.");
        }
        if (!actual.SetEquals(expected))
            throw Invalid("JSON properties do not match the scoring schema.");
    }

    private static Dictionary<string, string?> ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            var name = args[index];
            if (name == ArmFlag)
            {
                if (!values.TryAdd(name, null))
                    throw Invalid("The offline scoring arm is duplicated.");
                continue;
            }
            if (!RequiredOptions.Contains(name, StringComparer.Ordinal)
                || index + 1 >= args.Length
                || args[index + 1].StartsWith("--", StringComparison.Ordinal)
                || !values.TryAdd(name, args[++index]))
                throw Invalid("An unknown, duplicate, or valueless scoring option was supplied.");
        }
        if (!values.ContainsKey(ArmFlag)
            || RequiredOptions.Any(option => !values.ContainsKey(option))
            || values.Count != RequiredOptions.Length + 1)
            throw Invalid("All explicit offline scoring options are required.");
        return values;
    }

    private static OfflineScoringRequest BuildRequest(
        Dictionary<string, string?> values)
    {
        byte[] salt;
        try { salt = Convert.FromBase64String(Required(values, "--audit-salt-base64")); }
        catch (FormatException) { throw Invalid("Audit salt is invalid."); }
        if (salt.Length < 32)
            throw Invalid("Audit salt is invalid.");
        var request = new OfflineScoringRequest(
            Path.GetFullPath(Required(values, "--source-run-directory")),
            Path.GetFullPath(Required(values, "--comparison-jsonl")),
            Path.GetFullPath(Required(values, "--split-proof-json")),
            Path.GetFullPath(Required(values, "--output-root")),
            Required(values, "--run-id"),
            salt,
            Required(values, "--scorer-id"),
            Required(values, "--scorer-version"),
            Required(values, "--threshold-set-id"),
            Required(values, "--window-ref"),
            Required(values, "--expected-model-id"),
            Required(values, "--expected-ollama-manifest-digest"),
            Required(values, "--expected-ollama-model-layer-digest"),
            Required(values, "--expected-local-retention-policy-ref"),
            Required(values, "--expected-output-license-ref"),
            Required(values, "--expected-teacher-license-evidence-ref"),
            Required(values, "--expected-deployment-evidence-ref"));
        if (!IsSafeIdentity(request.RunId)
            || !IsSafeWindowsRunId(request.RunId, request.OutputRoot)
            || !IsSafeIdentity(request.ScorerId)
            || !IsSafeIdentity(request.ScorerVersion)
            || !IsSafeIdentity(request.ThresholdSetId)
            || !IsCanonicalEvidenceRef(request.WindowRef)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                request.ExpectedModelId)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaManifestDigest(
                    request.ExpectedOllamaManifestDigest)
            || !ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(
                    request.ExpectedOllamaModelLayerDigest)
            || !IsCanonicalEvidenceRef(request.ExpectedLocalRetentionPolicyRef)
            || !IsCanonicalEvidenceRef(request.ExpectedOutputLicenseRef)
            || !IsCanonicalEvidenceRef(request.ExpectedTeacherLicenseEvidenceRef)
            || !IsCanonicalEvidenceRef(request.ExpectedDeploymentEvidenceRef))
            throw Invalid("Explicit scoring identities are invalid.");
        return request;
    }

    private static void ValidatePaths(
        OfflineScoringRequest request,
        bool rejectOutputUnderProjects)
    {
        if (!Directory.Exists(request.SourceRunDirectory)
            || !File.Exists(request.ComparisonJsonl)
            || !File.Exists(request.SplitProofJson))
            throw Invalid("Required scoring input paths do not exist.");
        RejectExistingReparseSegments(request.SourceRunDirectory);
        RejectExistingReparseSegments(request.ComparisonJsonl);
        RejectExistingReparseSegments(request.SplitProofJson);
        RejectExistingReparseSegments(
            Directory.Exists(request.OutputRoot)
                ? request.OutputRoot
                : Path.GetDirectoryName(request.OutputRoot)
                    ?? throw Invalid("Output root has no parent."));
        var sourceFiles = Directory.EnumerateFiles(request.SourceRunDirectory)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (!sourceFiles.SequenceEqual(
            new[] { SourceObservationLeaf, SourceManifestLeaf }
                .Order(StringComparer.Ordinal),
            StringComparer.Ordinal)
            || Directory.EnumerateDirectories(request.SourceRunDirectory).Any())
            throw Invalid("Source run directory is not an exact evaluation publication.");
        var allPaths = new[]
        {
            request.SourceRunDirectory,
            request.ComparisonJsonl,
            request.SplitProofJson,
            request.OutputRoot,
        };
        if (allPaths.Select(NormalizePath).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != allPaths.Length)
            throw Invalid("Scoring paths must be physically distinct.");
        var source = NormalizePath(request.SourceRunDirectory) + Path.DirectorySeparatorChar;
        var output = NormalizePath(request.OutputRoot) + Path.DirectorySeparatorChar;
        if (source.StartsWith(output, StringComparison.OrdinalIgnoreCase)
            || output.StartsWith(source, StringComparison.OrdinalIgnoreCase))
            throw Invalid("Source and output directory trees must be disjoint.");
        if (rejectOutputUnderProjects)
        {
            var projects = NormalizePath(ProjectsRoot);
            var outputRoot = NormalizePath(request.OutputRoot);
            if (PathComparer.Equals(outputRoot, projects)
                || IsContainedBy(outputRoot, projects))
            {
                throw Invalid(
                    "Production scoring outputs cannot be written under C:\\Projects.");
            }
        }
    }

    private static OfflineResolvedPhysicalPaths ValidatePhysicalPaths(
        OfflineScoringRequest request,
        string sourceObservationPath,
        string sourceManifestPath,
        OfflineScoringDependencies dependencies)
    {
        var resolved = new OfflineResolvedPhysicalPaths(
            RequirePhysicalPath(
                dependencies,
                request.SourceRunDirectory,
                allowMissing: false),
            RequirePhysicalPath(dependencies, sourceObservationPath, allowMissing: false),
            RequirePhysicalPath(dependencies, sourceManifestPath, allowMissing: false),
            RequirePhysicalPath(dependencies, request.ComparisonJsonl, allowMissing: false),
            RequirePhysicalPath(dependencies, request.SplitProofJson, allowMissing: false),
            RequirePhysicalPath(dependencies, request.OutputRoot, allowMissing: true),
            dependencies.RejectOutputUnderProjects
                ? RequirePhysicalPath(dependencies, ProjectsRoot, allowMissing: false)
                : null);
        ValidatePhysicalRelationships(
            resolved,
            dependencies.RejectOutputUnderProjects);
        return resolved;
    }

    private static void RevalidatePhysicalInputs(
        OfflineScoringRequest request,
        string sourceObservationPath,
        string sourceManifestPath,
        OfflineResolvedPhysicalPaths expected,
        OfflineScoringDependencies dependencies)
    {
        var current = new OfflineResolvedPhysicalPaths(
            RequirePhysicalPath(
                dependencies,
                request.SourceRunDirectory,
                allowMissing: false),
            RequirePhysicalPath(dependencies, sourceObservationPath, allowMissing: false),
            RequirePhysicalPath(dependencies, sourceManifestPath, allowMissing: false),
            RequirePhysicalPath(dependencies, request.ComparisonJsonl, allowMissing: false),
            RequirePhysicalPath(dependencies, request.SplitProofJson, allowMissing: false),
            expected.OutputRoot,
            expected.ProjectsRoot);
        if (!PathComparer.Equals(current.SourceRunDirectory, expected.SourceRunDirectory)
            || !PathComparer.Equals(current.SourceObservations, expected.SourceObservations)
            || !PathComparer.Equals(current.SourceManifest, expected.SourceManifest)
            || !PathComparer.Equals(current.ComparisonJsonl, expected.ComparisonJsonl)
            || !PathComparer.Equals(current.SplitProofJson, expected.SplitProofJson))
            throw Invalid("A physical scoring input identity changed.");
        ValidatePhysicalRelationships(
            current,
            dependencies.RejectOutputUnderProjects);
        ValidateExactSourcePublication(request.SourceRunDirectory);
    }

    private static void RevalidatePhysicalOutput(
        OfflineScoringRequest request,
        OfflineResolvedPhysicalPaths expected,
        OfflineScoringDependencies dependencies)
    {
        var currentOutput = RequirePhysicalPath(
            dependencies,
            request.OutputRoot,
            allowMissing: false);
        if (!PathComparer.Equals(currentOutput, expected.OutputRoot))
            throw Invalid("The physical scoring output identity changed.");
        ValidatePhysicalRelationships(
            expected with { OutputRoot = currentOutput },
            dependencies.RejectOutputUnderProjects);
    }

    private static void ValidatePhysicalRelationships(
        OfflineResolvedPhysicalPaths paths,
        bool rejectOutputUnderProjects)
    {
        if (!PathComparer.Equals(
                Path.GetDirectoryName(paths.SourceObservations),
                paths.SourceRunDirectory)
            || !PathComparer.Equals(
                Path.GetDirectoryName(paths.SourceManifest),
                paths.SourceRunDirectory))
            throw Invalid("Source artifacts escaped the physical source-run directory.");
        var independent = new[]
        {
            paths.SourceRunDirectory,
            paths.ComparisonJsonl,
            paths.SplitProofJson,
            paths.OutputRoot,
        };
        if (independent.Distinct(PathComparer).Count() != independent.Length
            || IsContainedBy(paths.OutputRoot, paths.SourceRunDirectory)
            || IsContainedBy(paths.SourceRunDirectory, paths.OutputRoot)
            || IsContainedBy(paths.ComparisonJsonl, paths.OutputRoot)
            || IsContainedBy(paths.SplitProofJson, paths.OutputRoot))
            throw Invalid("Physical scoring path boundaries overlap.");
        if (rejectOutputUnderProjects)
        {
            if (paths.ProjectsRoot is null
                || PathComparer.Equals(paths.OutputRoot, paths.ProjectsRoot)
                || IsContainedBy(paths.OutputRoot, paths.ProjectsRoot))
            {
                throw Invalid(
                    "Production scoring outputs cannot physically resolve under C:\\Projects.");
            }
        }
    }

    private static string RequirePhysicalPath(
        OfflineScoringDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var resolved = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!resolved.IsSuccess || string.IsNullOrWhiteSpace(resolved.Value))
            throw Invalid("A physical scoring path could not be resolved.");
        return Path.TrimEndingDirectorySeparator(resolved.Value);
    }

    private static bool IsContainedBy(string candidate, string root)
        => candidate.StartsWith(
            Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static void ValidateExactSourcePublication(string sourceRunDirectory)
    {
        var files = Directory.EnumerateFiles(sourceRunDirectory)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (!files.SequenceEqual(
                new[] { SourceObservationLeaf, SourceManifestLeaf }
                    .Order(StringComparer.Ordinal),
                StringComparer.Ordinal)
            || Directory.EnumerateDirectories(sourceRunDirectory).Any())
            throw Invalid("Source run directory changed during scoring.");
    }

    private static string NormalizePath(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool IsSafeWindowsRunId(string runId, string outputRoot)
    {
        if (!IsSafeWindowsLeafName(runId))
            return false;
        var combined = Path.Combine(outputRoot, runId);
        try
        {
            var roundTrip = Path.GetFullPath(combined);
            return StringComparer.Ordinal.Equals(Path.GetFileName(roundTrip), runId)
                && StringComparer.OrdinalIgnoreCase.Equals(combined, roundTrip);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            return false;
        }
    }

    private static bool IsSafeWindowsLeafName(string value)
    {
        if (string.IsNullOrEmpty(value)
            || value.EndsWith('.')
            || value.Any(static character => character < ' '
                || character is '<' or '>' or ':' or '"' or '/' or '\\'
                    or '|' or '?' or '*'))
            return false;
        var upperBaseName = value.Split('.', 2)[0].ToUpperInvariant();
        return upperBaseName is not ("CON" or "PRN" or "AUX" or "NUL")
            && !IsNumberedDeviceName(upperBaseName, "COM")
            && !IsNumberedDeviceName(upperBaseName, "LPT");
    }

    private static bool IsNumberedDeviceName(string value, string prefix)
        => value.Length == 4
            && value.StartsWith(prefix, StringComparison.Ordinal)
            && value[3] is >= '1' and <= '9';

    private static void ReserveScoringWork(
        string expected,
        string predicted,
        ref long reserved,
        CancellationToken cancellationToken)
    {
        var estimated = LanguageTranslateHeldOutScorer.EstimateWorkCells(
            expected,
            predicted,
            cancellationToken);
        if (!estimated.IsSuccess)
            throw Invalid("Scoring work could not be reserved safely.");
        var cells = estimated.Value;
        if (cells > MaximumAggregateScoringCells - reserved)
            throw Invalid("The aggregate scoring work limit was exceeded.");
        reserved += cells;
    }

    private static void RejectExistingReparseSegments(string path)
    {
        var current = Path.GetFullPath(path);
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw Invalid("Reparse-point scoring paths are not permitted.");
            var parent = Path.GetDirectoryName(current);
            if (StringComparer.OrdinalIgnoreCase.Equals(parent, current))
                break;
            current = parent ?? string.Empty;
        }
    }

    private static FileStream OpenRetainedRead(string path)
        => new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16_384,
            FileOptions.SequentialScan);

    private static byte[] ReadBounded(FileStream stream)
    {
        if (stream.Length is < 1 or > MaximumFileBytes)
            throw Invalid("A scoring input file limit was exceeded.");
        var bytes = new byte[checked((int)stream.Length)];
        stream.Position = 0;
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static async Task WriteJsonLinesAsync<T>(
        IEvaluationPublicationBoundary publication,
        string leafName,
        IReadOnlyList<T> rows,
        CancellationToken cancellationToken)
    {
        await using var stream = publication.CreateArtifactFile(leafName);
        await using var writer = new StreamWriter(
            stream,
            StrictUtf8,
            bufferSize: 16_384,
            leaveOpen: true);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(JsonSerializer.Serialize(row, SnakeJson))
                .ConfigureAwait(false);
        }
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static async Task WriteJsonAsync<T>(
        IEvaluationPublicationBoundary publication,
        string leafName,
        T value,
        CancellationToken cancellationToken)
    {
        await using var stream = publication.CreateArtifactFile(leafName);
        await JsonSerializer.SerializeAsync(
                stream,
                value,
                SnakeJson,
                cancellationToken)
            .ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    internal static string ComputeEvaluationRosterSha256(
        IReadOnlyList<EvaluationRosterEntry> roster)
    {
        if (roster.Count == 0
            || roster.Select(static row => (row.EvaluationSetId, row.CaseId))
                .Distinct()
                .Count() != roster.Count)
        {
            throw Invalid("The evaluation roster must contain each case exactly once.");
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(
                   stream,
                   new JsonWriterOptions
                   {
                       Indented = false,
                       MaxDepth = 4,
                       SkipValidation = false,
                   }))
        {
            writer.WriteStartArray();
            foreach (var row in roster
                         .OrderBy(static row => row.EvaluationSetId, StringComparer.Ordinal)
                         .ThenBy(static row => row.CaseId, StringComparer.Ordinal)
                         .ThenBy(static row => row.DomainId, StringComparer.Ordinal)
                         .ThenBy(static row => row.SourceRef, StringComparer.Ordinal)
                         .ThenBy(static row => row.Split, StringComparer.Ordinal)
                         .ThenBy(static row => row.GoldLabelSha256, StringComparer.Ordinal)
                         .ThenBy(static row => row.GoldAuthorityRef, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("evaluation_set_id", row.EvaluationSetId);
                writer.WriteString("case_id", row.CaseId);
                writer.WriteString("domain_id", row.DomainId);
                writer.WriteString("source_ref", row.SourceRef);
                writer.WriteString("split", row.Split);
                writer.WriteString("gold_label_sha256", row.GoldLabelSha256);
                writer.WriteString("gold_authority_ref", row.GoldAuthorityRef);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        return HashRef(stream.ToArray());
    }

    private static string HashJsonString(string value)
        => Convert.ToHexString(SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(value)))
            .ToLowerInvariant();

    private static string HashRef(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static bool IsLowerHex(string value, int length)
        => value is not null
            && value.Length == length
            && value.All(static character => character is >= '0' and <= '9'
                or >= 'a' and <= 'f');

    private static bool IsCanonicalEvidenceRef(string value)
        => value is not null
            && value.StartsWith("sha256:", StringComparison.Ordinal)
            && IsLowerHex(value[7..], 64);

    private static bool IsSafeIdentity(string value)
        => !string.IsNullOrEmpty(value)
            && value.Length <= 128
            && char.IsAsciiLetterOrDigit(value[0])
            && value.All(static character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '_' or '-' or '.' or ':');

    private static void RejectBom(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble))
            throw Invalid("UTF-8 BOM is not permitted in scoring inputs.");
    }

    private static string Required(
        Dictionary<string, string?> values,
        string name)
        => values[name] ?? throw Invalid("A scoring option value is missing.");

    private static EvaluationRunnerException Invalid(string message)
        => new("TP1_OFFLINE_SCORING_INPUT_INVALID", message);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private static Task WriteErrorAsync(
        TextWriter stderr,
        string code,
        string message)
        => stderr.WriteLineAsync(JsonSerializer.Serialize(
            new EvaluationError(false, code, message),
            SnakeJson));

    private static JsonSerializerOptions CreateJsonOptions(
        JsonNamingPolicy namingPolicy)
        => new()
        {
            PropertyNamingPolicy = namingPolicy,
            WriteIndented = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            },
        };
}

internal sealed class OfflineScoringDependencies(
    Func<string, string, IReadOnlyCollection<string>,
        OperationResult<IEvaluationPublicationBoundary>> createPublication,
    bool rejectOutputUnderProjects = false,
    Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null)
{
    internal Func<string, string, IReadOnlyCollection<string>,
        OperationResult<IEvaluationPublicationBoundary>> CreatePublication { get; } =
        createPublication ?? throw new ArgumentNullException(nameof(createPublication));

    internal bool RejectOutputUnderProjects { get; } = rejectOutputUnderProjects;
    internal Func<string, bool, OperationResult<string>> ResolvePhysicalPath { get; } =
        resolvePhysicalPath ?? PhysicalPathResolver.Resolve;
    internal bool RequiresWindowsHandlePublication { get; } =
        createPublication.Method.DeclaringType == typeof(WindowsEvaluationPublicationBoundary);

    internal static OfflineScoringDependencies Production { get; } = new(
        WindowsEvaluationPublicationBoundary.Create,
        rejectOutputUnderProjects: true);
}

internal sealed record OfflineScoringRequest(
    string SourceRunDirectory,
    string ComparisonJsonl,
    string SplitProofJson,
    string OutputRoot,
    string RunId,
    byte[] AuditSalt,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    string WindowRef,
    string ExpectedModelId,
    string ExpectedOllamaManifestDigest,
    string ExpectedOllamaModelLayerDigest,
    string ExpectedLocalRetentionPolicyRef,
    string ExpectedOutputLicenseRef,
    string ExpectedTeacherLicenseEvidenceRef,
    string ExpectedDeploymentEvidenceRef);

internal sealed record OfflineResolvedPhysicalPaths(
    string SourceRunDirectory,
    string SourceObservations,
    string SourceManifest,
    string ComparisonJsonl,
    string SplitProofJson,
    string OutputRoot,
    string? ProjectsRoot);

internal sealed record ComparisonInputRow(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    string SourceRef,
    string Split,
    string ExpectedLabel,
    string ChosenTranslation,
    string GoldOrigin,
    string GoldLicense,
    string GoldAuthorityRef,
    bool GoldFromNodeUnderTest,
    bool SemanticQualityClaimed,
    string LearnerBestOutput,
    string LearnerBestOutputSha256,
    string LearnerBestArtifactRef);

internal sealed record SplitProofInput(
    string EvaluationSetId,
    string SplitId,
    IReadOnlyList<string> TrainingSourceRefs,
    IReadOnlyList<string> EvaluationSourceRefs,
    int CrossSplitNearDuplicateCount,
    string NearDuplicateAlgorithm,
    double NearDuplicateThreshold,
    string ProofArtifactRef);

internal readonly record struct CaseJoinKey(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    string ScorerId,
    string SourceRef,
    string Split) : IComparable<CaseJoinKey>
{
    public int CompareTo(CaseJoinKey other)
    {
        var values = new[]
        {
            (EvaluationSetId, other.EvaluationSetId),
            (CaseId, other.CaseId),
            (DomainId, other.DomainId),
            (ScorerId, other.ScorerId),
            (SourceRef, other.SourceRef),
            (Split, other.Split),
        };
        foreach (var (left, right) in values)
        {
            var compared = StringComparer.Ordinal.Compare(left, right);
            if (compared != 0)
                return compared;
        }
        return 0;
    }
}

internal sealed record ScoredSemanticCandidate(
    string RawOutput,
    string OutputSha256,
    LanguageTranslateCandidateScore Score,
    IReadOnlyList<int> GenerationIndices,
    IReadOnlyList<string> RequestRefSha256s);

internal sealed record CandidateScoreArtifact(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    string SourceRef,
    string OutputSha256,
    bool ExactMatch,
    double CharacterAccuracy,
    double CandidateScore,
    IReadOnlyList<int> GenerationIndices,
    IReadOnlyList<string> TeacherRequestRefSha256s,
    bool EvalOnly);

internal sealed record CaseComparisonArtifact(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    string SourceRef,
    string GoldLabelSha256,
    string GoldAuthorityRef,
    string SelectedTeacherOutputSha256,
    double SelectedTeacherScore,
    IReadOnlyList<int> SelectedTeacherGenerationIndices,
    IReadOnlyList<string> SelectedTeacherRequestRefSha256s,
    string LearnerOutputSha256,
    string LearnerArtifactRef,
    double LearnerScore,
    bool TeacherBeatLearner,
    bool EvalOnly);

internal sealed record EvaluationRosterEntry(
    string EvaluationSetId,
    string CaseId,
    string DomainId,
    string SourceRef,
    string Split,
    string GoldLabelSha256,
    string GoldAuthorityRef);

internal sealed record OfflineScoringManifest(
    string RunId,
    string Stage,
    string EvaluationSetId,
    string DomainId,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    string WindowRef,
    int InferredK,
    int CaseCount,
    int SemanticCandidateCount,
    string SplitProofArtifactRef,
    string SourceRunId,
    string ModelId,
    string TeacherArtifactDigestKind,
    string OllamaManifestDigest,
    string OllamaModelLayerDigest,
    string LocalRetentionPolicyRef,
    string OutputLicenseRef,
    string TeacherLicenseEvidenceRef,
    string DeploymentEvidenceRef,
    string EvaluationRosterSha256,
    string SourceManifestSha256,
    string SourceObservationsSha256,
    string ComparisonInputSha256,
    string SplitProofSha256,
    bool EvalOnly,
    bool ScoringExecuted,
    bool TeacherStageGapReportCreated,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool Tp1ExitGateEvaluated,
    bool LocalTeacherCeilingEvaluated,
    bool HumanDecisionRequired);

internal sealed record ScoringArtifacts(
    IReadOnlyList<CandidateScoreArtifact> CandidateScores,
    IReadOnlyList<CaseComparisonArtifact> CaseComparisons,
    TeacherStageGapReport StageGapReport,
    OfflineScoringManifest Manifest);

internal sealed record OfflineScoringSuccess(
    bool Ok,
    string RunId,
    int CaseCount,
    string Destination);
