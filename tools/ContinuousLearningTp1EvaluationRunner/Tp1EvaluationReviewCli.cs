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
using TranslationDomain;

internal static class Tp1EvaluationReviewCli
{
    internal const string ArmFlag = "--assemble-tp1-review-request";
    internal const string ReviewRequestLeaf = "tp1-evaluation-review-request.json";
    internal const string ReviewMarkdownLeaf = "tp1-evaluation-review.ru.md";
    internal const string ReviewRunLeaf = "tp1-evaluation-review-run.json";

    private const string Status = "REVIEW_REQUEST_CREATED_NOT_ACCEPTED";
    private const string ProjectsRoot = @"C:\Projects";
    private const string ExpectedStage = "TP1_LOCAL";
    private const string DigestKind = "ollama_manifest_sha256";
    private const long MaximumIndexBytes = 1024L * 1024;
    private const long MaximumArtifactBytes = 16L * 1024 * 1024;
    private const int MaximumJsonLineBytes = 196_608;
    private const int MaximumRowsPerArtifact = 4_096;
    private const int MaximumRuns = 256;
    private const long MaximumAggregateSourceBytes = 512L * 1024 * 1024;
    private const long MaximumAggregateRows = 1_048_576;

    private static readonly UTF8Encoding StrictUtf8 =
        Tp1EvaluationEvidencePacketIo.StrictUtf8;
    private static readonly JsonSerializerOptions CamelJson = CreateJsonOptions(
        JsonNamingPolicy.CamelCase);
    private static readonly JsonSerializerOptions SnakeJson = CreateJsonOptions(
        JsonNamingPolicy.SnakeCaseLower);
    private static readonly StringComparer PathComparer =
        Tp1EvaluationEvidencePacketIo.PathComparer;

    internal static IReadOnlyList<string> ArtifactLeafNames { get; } =
        Array.AsReadOnly(new[]
        {
            ReviewRequestLeaf,
            ReviewMarkdownLeaf,
            ReviewRunLeaf,
        });

    private static readonly string[] SourceLeafNames =
    [
        OfflineScoringCli.CandidateScoresLeaf,
        OfflineScoringCli.CaseComparisonsLeaf,
        OfflineScoringCli.StageGapReportLeaf,
        OfflineScoringCli.ScoringManifestLeaf,
    ];

    private static readonly HashSet<string> IndexProperties = new(
        ["reviewPacketId", "orderingAuthorityRef", "runs"],
        StringComparer.Ordinal);
    private static readonly HashSet<string> IndexRunProperties = new(
        ["sequenceOrdinal", "scoringRunDirectory"],
        StringComparer.Ordinal);
    private static readonly HashSet<string> CandidateProperties = new(
    [
        "evaluation_set_id", "case_id", "domain_id", "scorer_id",
        "scorer_version", "threshold_set_id", "source_ref", "output_sha256",
        "exact_match", "character_accuracy", "candidate_score",
        "generation_indices", "teacher_request_ref_sha256s", "eval_only",
    ], StringComparer.Ordinal);
    private static readonly HashSet<string> ComparisonProperties = new(
    [
        "evaluation_set_id", "case_id", "domain_id", "scorer_id",
        "scorer_version", "threshold_set_id", "source_ref",
        "gold_label_sha256", "gold_authority_ref",
        "selected_teacher_output_sha256", "selected_teacher_score",
        "selected_teacher_generation_indices",
        "selected_teacher_request_ref_sha256s", "learner_output_sha256",
        "learner_artifact_ref", "learner_score", "teacher_beat_learner",
        "eval_only",
    ], StringComparer.Ordinal);
    private static readonly HashSet<string> ReportProperties = new(
    [
        "stage", "scorer_id", "scorer_version", "threshold_set_id",
        "held_out_accuracy_ygoal", "held_out_accuracy_ybest",
        "teacher_gap_to_gold", "teacher_beat_rate", "stage_gap9b_vs235b",
        "eval_only", "window_ref", "measurement_noise_floor",
        "measurement_noise_status",
    ], StringComparer.Ordinal);
    private static readonly HashSet<string> ManifestProperties = new(
    [
        "run_id", "stage", "evaluation_set_id", "domain_id", "scorer_id",
        "scorer_version", "threshold_set_id", "window_ref", "inferred_k",
        "case_count", "semantic_candidate_count", "split_proof_artifact_ref",
        "source_run_id", "model_id", "teacher_artifact_digest_kind",
        "ollama_manifest_digest", "ollama_model_layer_digest",
        "local_retention_policy_ref", "output_license_ref",
        "teacher_license_evidence_ref", "deployment_evidence_ref",
        "evaluation_roster_sha256", "source_manifest_sha256",
        "source_observations_sha256", "comparison_input_sha256",
        "split_proof_sha256", "eval_only", "scoring_executed",
        "teacher_stage_gap_report_created", "dpo_train_ready",
        "teacher_output_used_for_training", "training_started",
        "dpo_steps_executed", "checkpoint_promoted", "tp1_exit_gate_evaluated",
        "local_teacher_ceiling_evaluated", "human_decision_required",
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
            Tp1EvaluationReviewDependencies.Production);

    internal static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken,
        Tp1EvaluationReviewDependencies dependencies)
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
            await WriteErrorAsync(stderr, "TP1_REVIEW_CANCELLED").ConfigureAwait(false);
            return 130;
        }
        catch (Tp1ReviewException exception)
        {
            await WriteErrorAsync(stderr, exception.ErrorCode).ConfigureAwait(false);
            return 1;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            await WriteErrorAsync(stderr, "TP1_REVIEW_PUBLICATION_FAILED")
                .ConfigureAwait(false);
            return 1;
        }
    }

    private static async Task<Tp1EvaluationReviewSuccess> ExecuteAsync(
        string[] args,
        Tp1EvaluationReviewDependencies dependencies,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (dependencies.RequiresWindowsHandlePublication
            && !OperatingSystem.IsWindows())
            throw InputInvalid();

        var parsed = ParseArguments(args);
        var indexPath = RequireAbsoluteFile(parsed.ReviewIndexJson);
        var outputRoot = RequireAbsoluteDirectory(parsed.OutputRoot);
        ValidateOutputOutsideProjects(outputRoot, ProjectsRoot, dependencies);
        RejectExistingReparseSegments(indexPath);
        RejectExistingReparseSegments(outputRoot);

        var indexParent = Path.GetDirectoryName(indexPath);
        if (string.IsNullOrEmpty(indexParent))
            throw InputInvalid();
        var physicalIndexParent = RequirePhysical(
            dependencies,
            indexParent,
            allowMissing: false);
        using var indexLease = OpenRetainedFile(
            indexPath,
            MaximumIndexBytes,
            physicalIndexParent);
        var indexBytes = ReadRetainedFile(indexLease);
        var index = ParseReviewIndex(indexBytes);
        ValidateIndex(index);
        var destination = Path.Combine(outputRoot, index.ReviewPacketId);
        if (File.Exists(destination) || Directory.Exists(destination))
            throw PublicationFailed();

        var physicalIndex = RequirePhysical(dependencies, indexPath, allowMissing: false);
        var physicalOutput = RequirePhysical(dependencies, outputRoot, allowMissing: true);
        var physicalProjects = dependencies.RejectOutputUnderProjects
            ? RequirePhysical(dependencies, ProjectsRoot, allowMissing: false)
            : ProjectsRoot;
        var lexicalRunDirectories = index.Runs
            .Select(static run => RequireAbsoluteDirectory(run.ScoringRunDirectory))
            .ToArray();
        ValidateLexicalDisjointness(indexPath, outputRoot, lexicalRunDirectories);

        var physicalRuns = lexicalRunDirectories
            .Select(path => RequirePhysical(dependencies, path, allowMissing: false))
            .ToArray();
        ValidatePhysicalDisjointness(physicalIndex, physicalOutput, physicalRuns);
        ValidateOutputOutsideProjects(physicalOutput, physicalProjects, dependencies);

        var leases = new List<RetainedReviewRun>(index.Runs.Count);
        try
        {
            long aggregateBytes = indexBytes.LongLength;
            long aggregateRows = 0;
            foreach (var runInput in index.Runs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directory = lexicalRunDirectories[runInput.SequenceOrdinal];
                RejectExistingReparseSegments(directory);
                var lease = RetainedReviewRun.Open(
                    runInput.SequenceOrdinal,
                    directory);
                leases.Add(lease);
                aggregateBytes = checked(aggregateBytes + lease.TotalLength);
                if (aggregateBytes > MaximumAggregateSourceBytes)
                    throw InputInvalid();
                var validated = ParseAndValidateRun(
                    lease,
                    physicalRuns[runInput.SequenceOrdinal],
                    dependencies,
                    cancellationToken);
                lease.SetValidated(validated);
                aggregateRows = checked(aggregateRows
                    + validated.CandidateRowCount
                    + validated.CaseRowCount);
                if (aggregateRows > MaximumAggregateRows)
                    throw InputInvalid();
            }

            ValidateReplayResistance(leases);
            var validatedRuns = leases.Select(static lease => lease.Validated).ToArray();
            var invariants = ValidateCrossRunInvariants(validatedRuns);
            var summaries = validatedRuns.Select(static run => run.Summary).ToArray();
            var deltas = BuildAdjacentDeltas(summaries);
            var aggregate = BuildAggregate(summaries);
            var request = new Tp1EvaluationReviewRequestArtifact(
                Status,
                index.ReviewPacketId,
                index.OrderingAuthorityRef,
                OrderingVerified: false,
                invariants,
                summaries,
                deltas,
                aggregate,
                EvalOnly: true,
                IndependentAcceptance: false,
                Tp1ExitGateEvaluated: false,
                LocalTeacherCeilingEvaluated: false,
                DpoTrainReady: false,
                TeacherOutputUsedForTraining: false,
                TrainingStarted: false,
                DpoStepsExecuted: 0,
                CheckpointPromoted: false,
                HumanDecisionRequired: true);
            var requestBytes = SerializeJson(request);
            var requestHash = HashRef(requestBytes);
            var markdownBytes = StrictUtf8.GetBytes(BuildRussianMarkdown(
                index,
                summaries,
                deltas,
                aggregate,
                requestHash));
            var sourceHashes = summaries
                .Select(static summary => summary.SourceArtifactHashes)
                .ToArray();
            var publicationManifest = new Tp1EvaluationReviewRunArtifact(
                Status,
                index.ReviewPacketId,
                index.OrderingAuthorityRef,
                OrderingVerified: false,
                ReviewIndexSha256: HashRef(indexBytes),
                ReviewRequestSha256: requestHash,
                ReviewMarkdownSha256: HashRef(markdownBytes),
                PublishedArtifactLeafNames: ArtifactLeafNames,
                SourceArtifacts: sourceHashes,
                EvalOnly: true,
                IndependentAcceptance: false,
                Tp1ExitGateEvaluated: false,
                LocalTeacherCeilingEvaluated: false,
                DpoTrainReady: false,
                TeacherOutputUsedForTraining: false,
                TrainingStarted: false,
                DpoStepsExecuted: 0,
                CheckpointPromoted: false,
                HumanDecisionRequired: true);
            var manifestBytes = SerializeJson(publicationManifest);

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
            VerifyRetainedFile(indexLease);
            VerifyRunsUnchanged(leases, physicalRuns, dependencies);

            await PublishAsync(
                    outputRoot,
                    currentPhysicalOutput,
                    index.ReviewPacketId,
                    requestBytes,
                    markdownBytes,
                    manifestBytes,
                    leases,
                    physicalRuns,
                    dependencies,
                    cancellationToken)
                .ConfigureAwait(false);
            return new Tp1EvaluationReviewSuccess(
                Ok: true,
                Status,
                index.ReviewPacketId,
                summaries.Length,
                aggregate.TotalCaseObservations);
        }
        finally
        {
            foreach (var lease in leases)
                lease.Dispose();
        }
    }

    private static async Task PublishAsync(
        string outputRoot,
        string physicalOutputRoot,
        string reviewPacketId,
        byte[] requestBytes,
        byte[] markdownBytes,
        byte[] manifestBytes,
        IReadOnlyList<RetainedReviewRun> leases,
        IReadOnlyList<string> physicalRuns,
        Tp1EvaluationReviewDependencies dependencies,
        CancellationToken cancellationToken)
    {
        IEvaluationPublicationBoundary? publication = null;
        var published = false;
        try
        {
            var created = dependencies.CreatePublication(
                outputRoot,
                reviewPacketId,
                ArtifactLeafNames);
            if (!created.IsSuccess || created.Value is null)
                throw PublicationFailed();
            publication = created.Value;
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            await WriteBytesAsync(
                    publication,
                    ReviewRequestLeaf,
                    requestBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(
                    publication,
                    ReviewMarkdownLeaf,
                    markdownBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteBytesAsync(
                    publication,
                    ReviewRunLeaf,
                    manifestBytes,
                    cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            VerifyRunsUnchanged(leases, physicalRuns, dependencies);
            publication.VerifyOutputRootIdentity(physicalOutputRoot);
            publication.VerifyStagingIdentity();
            publication.PublishAndVerify();
            published = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Tp1ReviewException)
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
        string leafName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        await using var stream = publication.CreateArtifactFile(leafName);
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static ValidatedReviewRun ParseAndValidateRun(
        RetainedReviewRun lease,
        string expectedPhysicalDirectory,
        Tp1EvaluationReviewDependencies dependencies,
        CancellationToken cancellationToken)
    {
        lease.VerifyUnchanged();
        var actualPhysical = RequirePhysical(
            dependencies,
            lease.Directory,
            allowMissing: false);
        if (!PathComparer.Equals(actualPhysical, expectedPhysicalDirectory))
            throw EvidenceMismatch();

        var manifestBytes = lease.ReadAllBytes(OfflineScoringCli.ScoringManifestLeaf);
        var reportBytes = lease.ReadAllBytes(OfflineScoringCli.StageGapReportLeaf);
        var candidateBytes = lease.ReadAllBytes(OfflineScoringCli.CandidateScoresLeaf);
        var comparisonBytes = lease.ReadAllBytes(OfflineScoringCli.CaseComparisonsLeaf);
        var manifest = ParseObject<OfflineScoringManifest>(
            manifestBytes,
            ManifestProperties,
            SnakeJson);
        var report = ParseObject<TeacherStageGapReport>(
            reportBytes,
            ReportProperties,
            SnakeJson);
        var candidates = ParseJsonLines<CandidateScoreArtifact>(
            candidateBytes,
            CandidateProperties,
            SnakeJson,
            cancellationToken);
        var comparisons = ParseJsonLines<CaseComparisonArtifact>(
            comparisonBytes,
            ComparisonProperties,
            SnakeJson,
            cancellationToken);
        if (!IsWithinPerRunRowLimit(candidates.Count, comparisons.Count))
            throw InputInvalid();

        ValidateManifest(manifest, lease.Directory);
        ValidateProducerOrder(candidates, comparisons);
        var arithmetic = ValidateRunArithmetic(
            manifest,
            report,
            candidates,
            comparisons);
        var hashes = new Tp1ReviewSourceArtifactHashes(
            lease.Ordinal,
            HashRef(candidateBytes),
            HashRef(comparisonBytes),
            HashRef(reportBytes),
            HashRef(manifestBytes));
        var summary = new Tp1ReviewRunSummary(
            lease.Ordinal,
            manifest.RunId,
            manifest.SourceRunId,
            manifest.WindowRef,
            manifest.CaseCount,
            manifest.SemanticCandidateCount,
            report.HeldOutAccuracyYgoal,
            report.HeldOutAccuracyYbest,
            report.TeacherGapToGold,
            report.TeacherBeatRate,
            arithmetic.StrictTeacherWins,
            manifest.EvaluationRosterSha256,
            hashes)
        {
            MeasurementNoiseFloor = report.MeasurementNoiseFloor,
            MeasurementNoiseStatus = report.MeasurementNoiseStatus
        };
        return new ValidatedReviewRun(
            manifest,
            report,
            summary,
            candidates.Count,
            comparisons.Count);
    }

    private static void ValidateManifest(
        OfflineScoringManifest manifest,
        string scoringDirectory)
    {
        if (!IsSafeIdentity(manifest.RunId)
            || !IsSafeWindowsLeafName(manifest.RunId)
            || !StringComparer.Ordinal.Equals(
                Path.GetFileName(Path.TrimEndingDirectorySeparator(scoringDirectory)),
                manifest.RunId)
            || !IsSafeIdentity(manifest.SourceRunId)
            || !StringComparer.Ordinal.Equals(manifest.Stage, ExpectedStage)
            || manifest.CaseCount is <= 0 or > MaximumRowsPerArtifact
            || manifest.SemanticCandidateCount is <= 0 or > MaximumRowsPerArtifact
            || manifest.InferredK is <= 0 or > MaximumRowsPerArtifact
            || !manifest.EvalOnly
            || !manifest.ScoringExecuted
            || !manifest.TeacherStageGapReportCreated
            || manifest.DpoTrainReady
            || manifest.TeacherOutputUsedForTraining
            || manifest.TrainingStarted
            || manifest.DpoStepsExecuted != 0
            || manifest.CheckpointPromoted
            || manifest.Tp1ExitGateEvaluated
            || manifest.LocalTeacherCeilingEvaluated
            || !manifest.HumanDecisionRequired
            || !StringComparer.Ordinal.Equals(
                manifest.TeacherArtifactDigestKind,
                DigestKind)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                manifest.ModelId)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(
                manifest.OllamaManifestDigest)
            || !ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelLayerDigest(
                manifest.OllamaModelLayerDigest)
            || !IsCanonicalEvidenceRef(manifest.WindowRef)
            || !IsCanonicalEvidenceRef(manifest.SplitProofArtifactRef)
            || !IsCanonicalEvidenceRef(manifest.EvaluationRosterSha256)
            || !IsCanonicalEvidenceRef(manifest.SourceManifestSha256)
            || !IsCanonicalEvidenceRef(manifest.SourceObservationsSha256)
            || !IsCanonicalEvidenceRef(manifest.ComparisonInputSha256)
            || !IsCanonicalEvidenceRef(manifest.SplitProofSha256)
            || !IsCanonicalEvidenceRef(manifest.LocalRetentionPolicyRef)
            || !IsCanonicalEvidenceRef(manifest.OutputLicenseRef)
            || !IsCanonicalEvidenceRef(manifest.TeacherLicenseEvidenceRef)
            || !IsCanonicalEvidenceRef(manifest.DeploymentEvidenceRef)
            || !IsSafeIdentity(manifest.EvaluationSetId)
            || !IsSafeIdentity(manifest.DomainId)
            || !IsSafeIdentity(manifest.ScorerId)
            || !IsSafeIdentity(manifest.ScorerVersion)
            || !IsSafeIdentity(manifest.ThresholdSetId))
            throw EvidenceMismatch();
    }

    private static RunArithmetic ValidateRunArithmetic(
        OfflineScoringManifest manifest,
        TeacherStageGapReport report,
        IReadOnlyList<CandidateScoreArtifact> candidates,
        IReadOnlyList<CaseComparisonArtifact> comparisons)
    {
        if (candidates.Count != manifest.SemanticCandidateCount
            || comparisons.Count != manifest.CaseCount
            || comparisons.Count == 0
            || !StringComparer.Ordinal.Equals(report.Stage, manifest.Stage)
            || !StringComparer.Ordinal.Equals(report.ScorerId, manifest.ScorerId)
            || !StringComparer.Ordinal.Equals(report.ScorerVersion, manifest.ScorerVersion)
            || !StringComparer.Ordinal.Equals(report.ThresholdSetId, manifest.ThresholdSetId)
            || !StringComparer.Ordinal.Equals(report.WindowRef, manifest.WindowRef)
            || !report.EvalOnly
            || report.StageGap9bVs235b is not null
            || !MeasurementNoiseReportIsValid(report))
            throw EvidenceMismatch();

        var candidatesByCase = candidates.GroupBy(static row => (
                row.EvaluationSetId,
                row.CaseId),
            EqualityComparer<(string EvaluationSetId, string CaseId)>.Default)
            .ToDictionary(static group => group.Key, static group => group.ToArray());
        if (candidatesByCase.Count != comparisons.Count)
            throw EvidenceMismatch();

        var roster = new List<EvaluationRosterEntry>(comparisons.Count);
        var selectedTotal = 0D;
        var learnerTotal = 0D;
        var strictWins = 0;
        var runRequestHashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var comparison in comparisons)
        {
            if (!comparison.EvalOnly
                || !FiniteUnit(comparison.SelectedTeacherScore)
                || !FiniteUnit(comparison.LearnerScore)
                || comparison.TeacherBeatLearner
                    != (comparison.SelectedTeacherScore > comparison.LearnerScore)
                || !IsLowerHex(comparison.GoldLabelSha256, 64)
                || !IsCanonicalEvidenceRef(comparison.GoldAuthorityRef)
                || !IsLowerHex(comparison.LearnerOutputSha256, 64)
                || !IsCanonicalEvidenceRef(comparison.LearnerArtifactRef)
                || !IsSafeIdentity(comparison.CaseId)
                || !IdentityMatches(manifest, comparison))
                throw EvidenceMismatch();
            if (!candidatesByCase.TryGetValue(
                    (comparison.EvaluationSetId, comparison.CaseId),
                    out var caseCandidates)
                || caseCandidates.Length == 0)
                throw EvidenceMismatch();

            var allIndices = new List<int>();
            var allRequestHashes = new List<string>();
            foreach (var candidate in caseCandidates)
            {
                if (!candidate.EvalOnly
                    || !IdentityMatches(manifest, comparison, candidate)
                    || !IsLowerHex(candidate.OutputSha256, 64)
                    || !FiniteUnit(candidate.CharacterAccuracy)
                    || !FiniteUnit(candidate.CandidateScore)
                    || candidate.ExactMatch != (candidate.CharacterAccuracy == 1D)
                    || candidate.CandidateScore
                        != 0.5D * (candidate.ExactMatch ? 1D : 0D)
                            + 0.5D * candidate.CharacterAccuracy
                    || candidate.GenerationIndices is null
                    || candidate.TeacherRequestRefSha256s is null
                    || candidate.GenerationIndices.Count == 0
                    || candidate.GenerationIndices.Count
                        != candidate.TeacherRequestRefSha256s.Count
                    || !StrictlyIncreasing(candidate.GenerationIndices)
                    || candidate.TeacherRequestRefSha256s
                        .Any(static value => !IsLowerHex(value, 64))
                    || candidate.TeacherRequestRefSha256s
                        .Distinct(StringComparer.Ordinal).Count()
                        != candidate.TeacherRequestRefSha256s.Count
                    || !candidate.TeacherRequestRefSha256s.SequenceEqual(
                        candidate.TeacherRequestRefSha256s.Order(StringComparer.Ordinal),
                        StringComparer.Ordinal))
                    throw EvidenceMismatch();
                allIndices.AddRange(candidate.GenerationIndices);
                allRequestHashes.AddRange(candidate.TeacherRequestRefSha256s);
            }
            if (caseCandidates.Select(static candidate => candidate.OutputSha256)
                    .Distinct(StringComparer.Ordinal).Count() != caseCandidates.Length
                || comparison.SelectedTeacherGenerationIndices is null
                || comparison.SelectedTeacherRequestRefSha256s is null)
                throw EvidenceMismatch();
            if (!allIndices.Order().SequenceEqual(Enumerable.Range(0, manifest.InferredK))
                || allIndices.Distinct().Count() != allIndices.Count
                || allRequestHashes.Count != manifest.InferredK
                || allRequestHashes.Distinct(StringComparer.Ordinal).Count()
                    != allRequestHashes.Count)
                throw EvidenceMismatch();
            foreach (var requestHash in allRequestHashes)
            {
                if (!runRequestHashes.Add(requestHash))
                    throw EvidenceMismatch();
            }

            var selected = caseCandidates
                .Where(candidate => StringComparer.Ordinal.Equals(
                    candidate.OutputSha256,
                    comparison.SelectedTeacherOutputSha256))
                .ToArray();
            if (selected.Length != 1)
                throw EvidenceMismatch();
            var selectedCandidate = selected[0];
            var expectedSelected = caseCandidates
                .OrderByDescending(static candidate => candidate.CandidateScore)
                .ThenBy(static candidate => candidate.OutputSha256, StringComparer.Ordinal)
                .First();
            if (!ReferenceEquals(selectedCandidate, expectedSelected)
                || comparison.SelectedTeacherScore != selectedCandidate.CandidateScore
                || !comparison.SelectedTeacherGenerationIndices.SequenceEqual(
                    selectedCandidate.GenerationIndices)
                || !comparison.SelectedTeacherRequestRefSha256s.SequenceEqual(
                    selectedCandidate.TeacherRequestRefSha256s))
                throw EvidenceMismatch();
            selectedTotal += comparison.SelectedTeacherScore;
            learnerTotal += comparison.LearnerScore;
            if (comparison.TeacherBeatLearner)
                strictWins++;
            roster.Add(new EvaluationRosterEntry(
                comparison.EvaluationSetId,
                comparison.CaseId,
                comparison.DomainId,
                comparison.SourceRef,
                LanguageTranslateHeldOutScorer.HeldOutSplit,
                comparison.GoldLabelSha256,
                comparison.GoldAuthorityRef));
        }

        var yGoal = selectedTotal / comparisons.Count;
        var yBest = learnerTotal / comparisons.Count;
        var gap = 1D - yGoal;
        var beatRate = (double)strictWins / comparisons.Count;
        if (report.HeldOutAccuracyYgoal != yGoal
            || report.HeldOutAccuracyYbest != yBest
            || report.TeacherGapToGold != gap
            || report.TeacherBeatRate != beatRate
            || !FiniteUnit(yGoal)
            || !FiniteUnit(yBest)
            || !FiniteUnit(gap)
            || !FiniteUnit(beatRate)
            || !StringComparer.Ordinal.Equals(
                manifest.EvaluationRosterSha256,
                OfflineScoringCli.ComputeEvaluationRosterSha256(roster)))
            throw EvidenceMismatch();
        return new RunArithmetic(strictWins);
    }

    private static bool MeasurementNoiseReportIsValid(
        TeacherStageGapReport report)
        // TP1 пока не несёт passport-bound calibration. Ненулевой floor был бы
        // неподписанной имитацией прибора, поэтому review принимает только честное отсутствие калибровки.
        => report.MeasurementNoiseFloor is null &&
           string.Equals(
               report.MeasurementNoiseStatus,
               MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
               StringComparison.Ordinal);

    private static bool IdentityMatches(
        OfflineScoringManifest manifest,
        CaseComparisonArtifact comparison)
        => StringComparer.Ordinal.Equals(comparison.EvaluationSetId, manifest.EvaluationSetId)
            && StringComparer.Ordinal.Equals(comparison.DomainId, manifest.DomainId)
            && StringComparer.Ordinal.Equals(comparison.ScorerId, manifest.ScorerId)
            && StringComparer.Ordinal.Equals(comparison.ScorerVersion, manifest.ScorerVersion)
            && StringComparer.Ordinal.Equals(
                comparison.ThresholdSetId,
                manifest.ThresholdSetId)
            && IsCanonicalEvidenceRef(comparison.SourceRef);

    private static bool IdentityMatches(
        OfflineScoringManifest manifest,
        CaseComparisonArtifact comparison,
        CandidateScoreArtifact candidate)
        => IdentityMatches(manifest, comparison)
            && StringComparer.Ordinal.Equals(
                candidate.EvaluationSetId,
                comparison.EvaluationSetId)
            && StringComparer.Ordinal.Equals(candidate.CaseId, comparison.CaseId)
            && StringComparer.Ordinal.Equals(candidate.DomainId, comparison.DomainId)
            && StringComparer.Ordinal.Equals(candidate.ScorerId, comparison.ScorerId)
            && StringComparer.Ordinal.Equals(
                candidate.ScorerVersion,
                comparison.ScorerVersion)
            && StringComparer.Ordinal.Equals(
                candidate.ThresholdSetId,
                comparison.ThresholdSetId)
            && StringComparer.Ordinal.Equals(candidate.SourceRef, comparison.SourceRef);

    private static void ValidateProducerOrder(
        IReadOnlyList<CandidateScoreArtifact> candidates,
        IReadOnlyList<CaseComparisonArtifact> comparisons)
    {
        if (!candidates.SequenceEqual(candidates
                .OrderBy(static row => row.EvaluationSetId, StringComparer.Ordinal)
                .ThenBy(static row => row.CaseId, StringComparer.Ordinal)
                .ThenBy(static row => row.OutputSha256, StringComparer.Ordinal))
            || !comparisons.SequenceEqual(comparisons
                .OrderBy(static row => row.EvaluationSetId, StringComparer.Ordinal)
                .ThenBy(static row => row.CaseId, StringComparer.Ordinal)))
            throw EvidenceMismatch();
    }

    private static void ValidateReplayResistance(IReadOnlyList<RetainedReviewRun> leases)
    {
        var runIds = new HashSet<string>(StringComparer.Ordinal);
        var sourceRunIds = new HashSet<string>(StringComparer.Ordinal);
        var windows = new HashSet<string>(StringComparer.Ordinal);
        var tuples = new HashSet<string>(StringComparer.Ordinal);
        foreach (var lease in leases)
        {
            var run = lease.Validated;
            var hashes = run.Summary.SourceArtifactHashes;
            var tuple = string.Join('|',
                hashes.CandidateScoresSha256,
                hashes.CaseComparisonsSha256,
                hashes.TeacherStageGapReportSha256,
                hashes.ScoringRunManifestSha256);
            if (!runIds.Add(run.Manifest.RunId)
                || !sourceRunIds.Add(run.Manifest.SourceRunId)
                || !windows.Add(run.Manifest.WindowRef)
                || !tuples.Add(tuple))
                throw EvidenceMismatch();
        }
    }

    private static Tp1ReviewInvariantSet ValidateCrossRunInvariants(
        IReadOnlyList<ValidatedReviewRun> runs)
    {
        var first = runs[0].Manifest;
        foreach (var run in runs.Skip(1).Select(static value => value.Manifest))
        {
            if (!StringComparer.Ordinal.Equals(run.Stage, first.Stage)
                || !StringComparer.Ordinal.Equals(run.EvaluationSetId, first.EvaluationSetId)
                || !StringComparer.Ordinal.Equals(run.DomainId, first.DomainId)
                || !StringComparer.Ordinal.Equals(run.ScorerId, first.ScorerId)
                || !StringComparer.Ordinal.Equals(run.ScorerVersion, first.ScorerVersion)
                || !StringComparer.Ordinal.Equals(run.ThresholdSetId, first.ThresholdSetId)
                || run.InferredK != first.InferredK
                || run.CaseCount != first.CaseCount
                || !StringComparer.Ordinal.Equals(
                    run.EvaluationRosterSha256,
                    first.EvaluationRosterSha256)
                || !StringComparer.Ordinal.Equals(
                    run.SplitProofArtifactRef,
                    first.SplitProofArtifactRef)
                || !StringComparer.Ordinal.Equals(run.SplitProofSha256, first.SplitProofSha256)
                || !StringComparer.Ordinal.Equals(run.ModelId, first.ModelId)
                || !StringComparer.Ordinal.Equals(
                    run.TeacherArtifactDigestKind,
                    first.TeacherArtifactDigestKind)
                || !StringComparer.Ordinal.Equals(
                    run.OllamaManifestDigest,
                    first.OllamaManifestDigest)
                || !StringComparer.Ordinal.Equals(
                    run.OllamaModelLayerDigest,
                    first.OllamaModelLayerDigest)
                || !StringComparer.Ordinal.Equals(
                    run.LocalRetentionPolicyRef,
                    first.LocalRetentionPolicyRef)
                || !StringComparer.Ordinal.Equals(run.OutputLicenseRef, first.OutputLicenseRef)
                || !StringComparer.Ordinal.Equals(
                    run.TeacherLicenseEvidenceRef,
                    first.TeacherLicenseEvidenceRef)
                || !StringComparer.Ordinal.Equals(
                    run.DeploymentEvidenceRef,
                    first.DeploymentEvidenceRef))
                throw EvidenceMismatch();
        }
        return new Tp1ReviewInvariantSet(
            first.Stage,
            first.EvaluationSetId,
            first.DomainId,
            first.ScorerId,
            first.ScorerVersion,
            first.ThresholdSetId,
            first.InferredK,
            first.CaseCount,
            first.EvaluationRosterSha256,
            first.SplitProofArtifactRef,
            first.SplitProofSha256,
            first.ModelId,
            first.TeacherArtifactDigestKind,
            first.OllamaManifestDigest,
            first.OllamaModelLayerDigest,
            first.LocalRetentionPolicyRef,
            first.OutputLicenseRef,
            first.TeacherLicenseEvidenceRef,
            first.DeploymentEvidenceRef);
    }

    private static IReadOnlyList<Tp1ReviewAdjacentDelta> BuildAdjacentDeltas(
        IReadOnlyList<Tp1ReviewRunSummary> runs)
        => runs.Zip(runs.Skip(1), static (from, to) => new Tp1ReviewAdjacentDelta(
                from.Ordinal,
                to.Ordinal,
                to.HeldOutAccuracyYgoal - from.HeldOutAccuracyYgoal,
                to.HeldOutAccuracyYbest - from.HeldOutAccuracyYbest,
                to.TeacherGapToGold - from.TeacherGapToGold,
                to.TeacherBeatRate - from.TeacherBeatRate))
            .ToArray();

    private static Tp1ReviewAggregate BuildAggregate(
        IReadOnlyList<Tp1ReviewRunSummary> runs)
    {
        var totalCases = runs.Sum(static run => run.CaseCount);
        var wins = runs.Sum(static run => run.StrictTeacherWins);
        var meanGoal = runs.Average(static run => run.HeldOutAccuracyYgoal);
        return new Tp1ReviewAggregate(
            runs.Count,
            totalCases,
            runs.Sum(static run => run.SemanticCandidateCount),
            wins,
            meanGoal,
            runs.Average(static run => run.HeldOutAccuracyYbest),
            1D - meanGoal,
            (double)wins / totalCases);
    }

    private static string BuildRussianMarkdown(
        Tp1ReviewIndexInput index,
        IReadOnlyList<Tp1ReviewRunSummary> runs,
        IReadOnlyList<Tp1ReviewAdjacentDelta> deltas,
        Tp1ReviewAggregate aggregate,
        string requestHash)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Запрос на независимую проверку TP1");
        builder.AppendLine();
        builder.AppendLine($"Статус: `{Status}`.");
        builder.AppendLine($"Идентификатор пакета: `{index.ReviewPacketId}`.");
        builder.AppendLine($"SHA-256 JSON-запроса: `{requestHash}`.");
        builder.AppendLine($"Источник порядка: `{index.OrderingAuthorityRef}`; порядок не верифицирован этим сборщиком.");
        builder.AppendLine();
        builder.AppendLine("## Что собрано");
        builder.AppendLine();
        builder.AppendLine($"Собрано прогонов: {runs.Count}; наблюдений случаев: {aggregate.TotalCaseObservations}; строгих побед учителя: {aggregate.PooledStrictTeacherWins}.");
        builder.AppendLine("Пакет содержит только хеши, числовые метрики и идентификаторы технических артефактов; сырой текст и материал пар предпочтений не включены.");
        builder.AppendLine();
        builder.AppendLine("## Что арифметически проверено");
        builder.AppendLine();
        builder.AppendLine("Проверены формула candidate_score, связи выбранного кандидата, строгие победы, средние yGoal/yBest, teacher gap, beat rate и отпечаток held-out roster.");
        builder.AppendLine("Скоринг по сырому тексту повторно не выполнялся; этот пакет не доказывает семантическую корректность скорера.");
        builder.AppendLine("Пол шума прибора перенесён из scoring-отчёта как отдельный статус; при отсутствии калибровки разность помечена как недоказанная.");
        builder.AppendLine();
        builder.AppendLine("## Сравнение последовательных прогонов");
        builder.AppendLine();
        if (deltas.Count == 0)
        {
            builder.AppendLine("Последовательных пар нет: представлен один прогон.");
        }
        else
        {
            foreach (var delta in deltas)
            {
                builder.AppendLine($"- {delta.FromOrdinal} → {delta.ToOrdinal}: ΔyGoal={Format(delta.DeltaHeldOutAccuracyYgoal)}, ΔyBest={Format(delta.DeltaHeldOutAccuracyYbest)}, ΔteacherGap={Format(delta.DeltaTeacherGapToGold)}, ΔbeatRate={Format(delta.DeltaTeacherBeatRate)}.");
            }
        }
        builder.AppendLine();
        builder.AppendLine("## Что этот пакет не доказывает");
        builder.AppendLine();
        builder.AppendLine("Это запрос на проверку, а не независимая приёмка. Исходные scoring-артефакты не подписаны.");
        builder.AppendLine("Хеши точных байтов подтверждают совпадение байтов, но не подлинность автора.");
        builder.AppendLine("Порог, плато, TP1 exit gate и потолок локального учителя не оценивались.");
        builder.AppendLine("Пакет не разрешает TP2, DPO, обучение или promotion checkpoint.");
        builder.AppendLine();
        builder.AppendLine("## Решение остаётся за Любой");
        builder.AppendLine();
        builder.AppendLine("Требуется независимое человеческое решение. evalOnly=true; independentAcceptance=false; humanDecisionRequired=true.");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string Format(double value)
        => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

    private static ParsedReviewArguments ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["--review-index-json"] = null,
            ["--output-root"] = null,
        };
        var armSeen = false;
        for (var index = 0; index < args.Length; index++)
        {
            var current = args[index];
            if (StringComparer.Ordinal.Equals(current, ArmFlag))
            {
                if (armSeen)
                    throw InputInvalid();
                armSeen = true;
                continue;
            }
            if (!values.ContainsKey(current)
                || values[current] is not null
                || index + 1 >= args.Length)
                throw InputInvalid();
            values[current] = args[++index];
        }
        if (!armSeen || values.Values.Any(static value => string.IsNullOrWhiteSpace(value)))
            throw InputInvalid();
        return new ParsedReviewArguments(
            values["--review-index-json"]!,
            values["--output-root"]!);
    }

    private static Tp1ReviewIndexInput ParseReviewIndex(byte[] bytes)
    {
        RejectBom(bytes);
        try
        {
            using var document = JsonDocument.Parse(bytes, DocumentOptions());
            ValidateExactObject(document.RootElement, IndexProperties);
            var runs = document.RootElement.GetProperty("runs");
            if (runs.ValueKind != JsonValueKind.Array)
                throw InputInvalid();
            foreach (var run in runs.EnumerateArray())
                ValidateExactObject(run, IndexRunProperties);
            return JsonSerializer.Deserialize<Tp1ReviewIndexInput>(bytes, CamelJson)
                ?? throw InputInvalid();
        }
        catch (Tp1ReviewException exception)
        {
            throw InputInvalid(exception);
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw InputInvalid(exception);
        }
    }

    private static void ValidateIndex(Tp1ReviewIndexInput index)
    {
        if (!IsSafeIdentity(index.ReviewPacketId)
            || !IsSafeWindowsLeafName(index.ReviewPacketId)
            || !IsCanonicalEvidenceRef(index.OrderingAuthorityRef)
            || index.Runs is null
            || index.Runs.Count is < 1 or > MaximumRuns)
            throw InputInvalid();
        for (var ordinal = 0; ordinal < index.Runs.Count; ordinal++)
        {
            var run = index.Runs[ordinal];
            if (run is null
                || run.SequenceOrdinal != ordinal
                || string.IsNullOrWhiteSpace(run.ScoringRunDirectory)
                || !Path.IsPathFullyQualified(run.ScoringRunDirectory))
                throw InputInvalid();
        }
    }

    private static T ParseObject<T>(
        byte[] bytes,
        HashSet<string> properties,
        JsonSerializerOptions options)
    {
        RejectBom(bytes);
        try
        {
            using var document = JsonDocument.Parse(bytes, DocumentOptions());
            ValidateExactObject(document.RootElement, properties);
            var value = JsonSerializer.Deserialize<T>(bytes, options)
                ?? throw EvidenceMismatch();
            if (!bytes.AsSpan().SequenceEqual(
                    JsonSerializer.SerializeToUtf8Bytes(value, options)))
                throw EvidenceMismatch();
            return value;
        }
        catch (Tp1ReviewException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static IReadOnlyList<T> ParseJsonLines<T>(
        byte[] bytes,
        HashSet<string> properties,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        RejectBom(bytes);
        try
        {
            RejectBom(bytes);
            var rows = new List<T>();
            var start = 0;
            for (var index = 0; index <= bytes.Length; index++)
            {
                if (index != bytes.Length && bytes[index] != (byte)'\n')
                    continue;
                if (index == bytes.Length && start == bytes.Length)
                    break;
                cancellationToken.ThrowIfCancellationRequested();
                var length = index - start;
                if (length > 0 && bytes[index - 1] == (byte)'\r')
                    length--;
                if (length <= 0 || length > MaximumJsonLineBytes
                    || rows.Count >= MaximumRowsPerArtifact)
                    throw InputInvalid();
                var line = bytes.AsSpan(start, length).ToArray();
                rows.Add(ParseObject<T>(line, properties, options));
                start = index + 1;
            }
            if (rows.Count == 0)
                throw InputInvalid();
            return rows;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Tp1ReviewException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw EvidenceMismatch(exception);
        }
    }

    private static void ValidateExactObject(
        JsonElement element,
        HashSet<string> expected)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw EvidenceMismatch();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                throw EvidenceMismatch();
        }
        if (!seen.SetEquals(expected))
            throw EvidenceMismatch();
    }

    private static void ValidateLexicalDisjointness(
        string indexPath,
        string outputRoot,
        IReadOnlyList<string> runs)
        => ValidateDisjointSet([indexPath, outputRoot, .. runs]);

    private static void ValidatePhysicalDisjointness(
        string indexPath,
        string outputRoot,
        IReadOnlyList<string> runs)
        => ValidateDisjointSet([indexPath, outputRoot, .. runs]);

    private static void ValidateDisjointSet(IReadOnlyList<string> paths)
    {
        if (paths.Distinct(PathComparer).Count() != paths.Count)
            throw InputInvalid();
        for (var left = 0; left < paths.Count; left++)
        for (var right = left + 1; right < paths.Count; right++)
        {
            if (IsContainedBy(paths[left], paths[right])
                || IsContainedBy(paths[right], paths[left]))
                throw InputInvalid();
        }
    }

    private static void ValidateOutputOutsideProjects(
        string outputRoot,
        string projectsRoot,
        Tp1EvaluationReviewDependencies dependencies)
    {
        if (!dependencies.RejectOutputUnderProjects)
            return;
        var projects = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectsRoot));
        if (PathComparer.Equals(outputRoot, projects)
            || IsContainedBy(outputRoot, projects))
            throw InputInvalid();
    }

    private static string RequirePhysical(
        Tp1EvaluationReviewDependencies dependencies,
        string path,
        bool allowMissing)
    {
        var result = dependencies.ResolvePhysicalPath(path, allowMissing);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw InputInvalid();
        return Path.TrimEndingDirectorySeparator(result.Value);
    }

    private static Tp1RetainedEvidenceFile OpenRetainedFile(
        string path,
        long maximumLength,
        string expectedPhysicalParent)
    {
        try
        {
            return Tp1RetainedEvidenceFile.Open(
                path,
                maximumLength,
                expectedPhysicalParent);
        }
        catch (Tp1EvidencePacketMismatchException exception)
        {
            throw EvidenceMismatch(exception);
        }
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static byte[] ReadRetainedFile(Tp1RetainedEvidenceFile file)
    {
        try
        {
            return file.ReadAllBytes();
        }
        catch (Tp1EvidencePacketMismatchException exception)
        {
            throw EvidenceMismatch(exception);
        }
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static void VerifyRetainedFile(Tp1RetainedEvidenceFile file)
    {
        try
        {
            file.VerifyUnchanged();
        }
        catch (Tp1EvidencePacketMismatchException exception)
        {
            throw EvidenceMismatch(exception);
        }
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static string RequireAbsoluteFile(string path)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.RequireAbsoluteFile(path);
        }
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static string RequireAbsoluteDirectory(string path)
    {
        try
        {
            return Tp1EvaluationEvidencePacketIo.RequireAbsoluteDirectory(path);
        }
        catch (Tp1EvidencePacketInputException exception)
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
        catch (Tp1EvidencePacketInputException exception)
        {
            throw InputInvalid(exception);
        }
    }

    private static void VerifyRunsUnchanged(
        IReadOnlyList<RetainedReviewRun> leases,
        IReadOnlyList<string> physicalRuns,
        Tp1EvaluationReviewDependencies dependencies)
    {
        for (var index = 0; index < leases.Count; index++)
        {
            leases[index].VerifyUnchanged();
            var physical = RequirePhysical(
                dependencies,
                leases[index].Directory,
                allowMissing: false);
            if (!PathComparer.Equals(physical, physicalRuns[index]))
                throw EvidenceMismatch();
        }
    }

    private static bool IsContainedBy(string candidate, string root)
        => Tp1EvaluationEvidencePacketIo.IsContainedBy(candidate, root);

    private static bool IsSafeIdentity(string value)
        => Tp1EvaluationEvidencePacketIo.IsSafeIdentity(value);

    private static bool IsSafeWindowsLeafName(string value)
        => Tp1EvaluationEvidencePacketIo.IsSafeWindowsLeafName(value);

    private static bool IsLowerHex(string value, int length)
        => Tp1EvaluationEvidencePacketIo.IsLowerHex(value, length);

    private static bool IsCanonicalEvidenceRef(string value)
        => Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(value);

    private static bool FiniteUnit(double value)
        => double.IsFinite(value) && value is >= 0D and <= 1D;

    internal static bool IsWithinPerRunRowLimit(
        int candidateCount,
        int caseCount)
        => candidateCount > 0
            && caseCount > 0
            && (long)candidateCount + caseCount <= MaximumRowsPerArtifact;

    private static bool StrictlyIncreasing(IReadOnlyList<int> values)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (values[index] < 0 || index > 0 && values[index] <= values[index - 1])
                return false;
        }
        return true;
    }

    private static byte[] SerializeJson<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, SnakeJson);

    private static string HashRef(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static void RejectBom(byte[] bytes)
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

    private static JsonDocumentOptions DocumentOptions()
        => new()
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 32,
        };

    private static JsonSerializerOptions CreateJsonOptions(JsonNamingPolicy namingPolicy)
        => new()
        {
            PropertyNamingPolicy = namingPolicy,
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
            "TP1_REVIEW_INPUT_INVALID" => "The TP1 review input is invalid.",
            "TP1_REVIEW_EVIDENCE_MISMATCH" => "The TP1 review evidence is inconsistent.",
            "TP1_REVIEW_CANCELLED" => "The TP1 review request assembly was cancelled.",
            _ => "The TP1 review request publication failed.",
        };
        await stderr.WriteLineAsync(JsonSerializer.Serialize(
                new { ok = false, error_code = code, message }))
            .ConfigureAwait(false);
    }

    private static Tp1ReviewException InputInvalid(Exception? inner = null)
        => new("TP1_REVIEW_INPUT_INVALID", inner);

    private static Tp1ReviewException EvidenceMismatch(Exception? inner = null)
        => new("TP1_REVIEW_EVIDENCE_MISMATCH", inner);

    private static Tp1ReviewException PublicationFailed(Exception? inner = null)
        => new("TP1_REVIEW_PUBLICATION_FAILED", inner);

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    private sealed record ParsedReviewArguments(
        string ReviewIndexJson,
        string OutputRoot);

    private sealed record RunArithmetic(int StrictTeacherWins);

    private sealed record ValidatedReviewRun(
        OfflineScoringManifest Manifest,
        TeacherStageGapReport Report,
        Tp1ReviewRunSummary Summary,
        int CandidateRowCount,
        int CaseRowCount);

    private sealed class Tp1ReviewException : Exception
    {
        internal Tp1ReviewException(string errorCode, Exception? inner)
            : base(errorCode, inner) => ErrorCode = errorCode;

        internal string ErrorCode { get; }
    }

    private sealed class RetainedReviewRun : IDisposable
    {
        private readonly Tp1RetainedEvidencePacket _packet;
        private ValidatedReviewRun? _validated;

        private RetainedReviewRun(
            int ordinal,
            string directory,
            Tp1RetainedEvidencePacket packet)
        {
            Ordinal = ordinal;
            Directory = directory;
            _packet = packet;
        }

        internal int Ordinal { get; }
        internal string Directory { get; }
        internal long TotalLength => _packet.TotalLength;
        internal ValidatedReviewRun Validated
            => _validated ?? throw EvidenceMismatch();

        internal static RetainedReviewRun Open(int ordinal, string directory)
        {
            try
            {
                var packet = Tp1EvaluationEvidencePacketIo.OpenExactPacket(
                    directory,
                    SourceLeafNames,
                    MaximumArtifactBytes);
                return new RetainedReviewRun(
                    ordinal,
                    directory,
                    packet);
            }
            catch (Tp1EvidencePacketMismatchException exception)
            {
                throw EvidenceMismatch(exception);
            }
            catch (Tp1EvidencePacketInputException exception)
            {
                throw InputInvalid(exception);
            }
        }

        internal byte[] ReadAllBytes(string leaf)
        {
            try
            {
                return _packet.ReadAllBytes(leaf);
            }
            catch (Tp1EvidencePacketMismatchException exception)
            {
                throw EvidenceMismatch(exception);
            }
            catch (Tp1EvidencePacketInputException exception)
            {
                throw InputInvalid(exception);
            }
        }

        internal void SetValidated(ValidatedReviewRun validated)
            => _validated = validated;

        internal void VerifyUnchanged()
        {
            try
            {
                _packet.VerifyUnchanged();
            }
            catch (Tp1EvidencePacketMismatchException exception)
            {
                throw EvidenceMismatch(exception);
            }
            catch (Tp1EvidencePacketInputException exception)
            {
                throw InputInvalid(exception);
            }
        }

        public void Dispose() => _packet.Dispose();
    }

}

internal sealed class Tp1EvaluationReviewDependencies
{
    internal Tp1EvaluationReviewDependencies(
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

    internal static Tp1EvaluationReviewDependencies Production { get; } = new();
}
