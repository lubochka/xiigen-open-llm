// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectAnswerVerifierTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;

    private readonly ProjectAnswerVerifierDpoGenerator _generator;
    private readonly ProjectAnswerVerifierTrainingRunner _runner;
    private readonly ProjectAnswerVerifierManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner? _dpoMetricRunner;

    public ProjectAnswerVerifierTrainingPipeline(
        ProjectAnswerVerifierDpoGenerator? generator = null,
        ProjectAnswerVerifierTrainingRunner? runner = null,
        ProjectAnswerVerifierManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new ProjectAnswerVerifierDpoGenerator();
        _runner = runner ?? new ProjectAnswerVerifierTrainingRunner();
        _publisher = publisher ?? new ProjectAnswerVerifierManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner(deterministicFreshInitialization: true);
    }

    public async Task<OperationResult<ProjectAnswerVerifierTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "project-answer-verifier-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectAnswerVerifierTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectAnswerVerifierTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<ProjectAnswerVerifierTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<ProjectAnswerVerifierTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<ProjectAnswerVerifierTrainingPipelineResult>();
        ProjectWikiDpoMetricRunResult? dpoMetrics = null;
        if (_dpoMetricRunner is not null)
        {
            var metricTrainingRows = InterleaveByRejectedType(trainBatch.Pairs).Select(ToDpoMetricPair).ToArray();
            var metricHeldOutRows = heldOutBatch.Pairs.Select(ToDpoMetricPair).ToArray();
            var metricRun = await _dpoMetricRunner.TrainAndEvaluateAsync(
                ProjectAnswerVerifier.UnitId,
                batchId,
                metricTrainingRows,
                metricHeldOutRows,
                artifactRoot,
                ct);
            if (!metricRun.IsSuccess)
                return metricRun.MapError<ProjectAnswerVerifierTrainingPipelineResult>();
            dpoMetrics = metricRun.Value!;
        }

        if (!string.Equals(run.Value!.Verdict, "PROJECT_ANSWER_VERIFIER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<ProjectAnswerVerifierTrainingPipelineResult>.Ok(new ProjectAnswerVerifierTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for ProjectAnswerVerifier and rerun.",
                ["answer_verifier_gate_failed_publication_skipped"],
                dpoMetrics));
        }
        if (dpoMetrics is not null && dpoMetrics.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
        {
            return OperationResult<ProjectAnswerVerifierTrainingPipelineResult>.Ok(new ProjectAnswerVerifierTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "DPO held-out accuracy is below 0.75; add 50-row remediation batch and rerun.",
                ["answer_verifier_dpo_accuracy_below_0_75_publication_skipped"],
                dpoMetrics));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<ProjectAnswerVerifierTrainingPipelineResult>();

        return OperationResult<ProjectAnswerVerifierTrainingPipelineResult>.Ok(new ProjectAnswerVerifierTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics));
    }

    private static ProjectAnswerVerifierDpoPair[] InterleaveByRejectedType(IReadOnlyList<ProjectAnswerVerifierDpoPair> pairs)
    {
        var groups = pairs
            .Select((pair, index) => new { pair, index })
            .GroupBy(item => item.pair.RejectedType, StringComparer.Ordinal)
            .OrderBy(group => group.Min(item => item.index))
            .Select(group => group.Select(item => item.pair).ToArray())
            .ToArray();
        var maxCount = groups.Length == 0 ? 0 : groups.Max(group => group.Length);
        var interleaved = new List<ProjectAnswerVerifierDpoPair>(pairs.Count);
        for (var index = 0; index < maxCount; index++)
        {
            foreach (var group in groups)
            {
                if (index < group.Length)
                    interleaved.Add(group[index]);
            }
        }

        return interleaved.ToArray();
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(ProjectAnswerVerifierDpoPair pair)
    {
        var warningText = pair.ExpectedWarnings.Length == 0
            ? "WARNINGS_NONE"
            : string.Join(" ", pair.ExpectedWarnings.Select(item => $"WARNING_{TokenSafe(item)}"));
        var citationState = pair.ExpectedWarnings.Contains("invalid_evidence_citation", StringComparer.Ordinal)
            ? "CITATION_INVALID_REJECTED INVALID_EVIDENCE_ID_DETECTED"
            : "CITATION_STATE_ACCEPTABLE";
        var boundaryState = pair.ExpectedWarnings.Contains("project_boundary_mismatch", StringComparer.Ordinal)
            ? "PROJECT_BOUNDARY_MISMATCH_DETECTED PROJECT_BOUNDARY_REJECTED WRONG_PROJECT_EVIDENCE_REJECTED ANSWER_PROJECT_DIFFERS_FROM_EXPECTED_PROJECT CROSS_PROJECT_CITATION_DENIED PASS_FALSE_REQUIRED_FOR_WRONG_PROJECT"
            : "PROJECT_BOUNDARY_OK";
        var abstentionState = pair.Answer.EvidenceIds.Length == 0 && !string.IsNullOrWhiteSpace(pair.Answer.MissingEvidence)
            ? $"PASS_TRUE_DECLARED_ABSTENTION MISSING_EVIDENCE_ABSTENTION_ACCEPTED DECLARED_MISSING_EVIDENCE_REASON_PRESENT MISSING_EVIDENCE_REASON_{TokenSafe(pair.Answer.MissingEvidence)} ABSTAIN_WITHOUT_CITATION_ALLOWED"
            : pair.Answer.EvidenceIds.Length == 0
                ? "MISSING_EVIDENCE_UNDECLARED_REJECTED DECLARED_MISSING_EVIDENCE_REASON_ABSENT UNCITED_ANSWER_MUST_FAIL"
                : "EVIDENCE_PRESENT DECLARED_MISSING_EVIDENCE_REASON_NOT_NEEDED";
        var correctionState = pair.RejectedType switch
        {
            "rejects_valid_answer" => "CORRECTS_FALSE_REJECTION VALID_LOCAL_EVIDENCE_ACCEPTED USEFUL_ANSWER_ALLOWED CITED_ANSWER_MUST_PASS",
            "accepts_wrong_project" => "CORRECTS_WRONG_PROJECT_ACCEPTANCE WRONG_PROJECT_EVIDENCE_DENIED CROSS_PROJECT_CITATION_BLOCKED ANSWER_PROJECT_MUST_MATCH_EXPECTED_PROJECT",
            "accepts_invalid_citation" => "CORRECTS_INVALID_CITATION_ACCEPTANCE INVALID_CITATION_DENIED CITATION_MUST_EXIST_IN_INDEX",
            "accepts_missing_evidence" => "CORRECTS_UNDECLARED_MISSING_EVIDENCE_ACCEPTANCE UNCITED_NON_ABSTENTION_MUST_FAIL",
            _ => "CORRECTS_INCOMPLETE_VERIFIER_RESULT COMPLETE_WARNING_SET_PRESENT"
        };
        var defectType = $"DEFECT_TYPE_{TokenSafe(pair.RejectedType)}";
        var expectedProject = $"EXPECTED_PROJECT_{TokenSafe(pair.ExpectedProjectId)}";
        var answerProject = $"ANSWER_PROJECT_{TokenSafe(pair.Answer.ProjectId)}";
        var evidenceState = pair.Answer.EvidenceIds.Length == 0 ? "ANSWER_EVIDENCE_NONE" : "ANSWER_EVIDENCE_PRESENT";
        var indexState = pair.EvidenceRecords.Length == 0 ? "INDEX_EVIDENCE_NONE" : "INDEX_EVIDENCE_PRESENT";
        var missingEvidenceState = string.IsNullOrWhiteSpace(pair.Answer.MissingEvidence)
            ? "MISSING_EVIDENCE_REASON_NONE"
            : "MISSING_EVIDENCE_REASON_PRESENT";
        var prompt = string.Join(" ", [
            "PROJECT_ANSWER_VERIFIER_DPO",
            defectType,
            expectedProject,
            answerProject,
            pair.ExpectedPassed ? "EXPECTED_PASS_TRUE" : "EXPECTED_PASS_FALSE",
            evidenceState,
            indexState,
            missingEvidenceState,
            warningText,
            citationState,
            boundaryState,
            abstentionState
        ]);
        var preferredToken = PreferenceTokens("PREFERRED_VERIFIER_OUTPUT", 12);
        var rejectedToken = PreferenceTokens("REJECTED_VERIFIER_OUTPUT", 12);
        var chosen = string.Join(" ", [
            preferredToken,
            defectType,
            "CHOSEN_IS_CORRECT",
            "REJECTED_IS_WRONG",
            "GOOD_PROJECT_ANSWER_VERIFIER_RESULT",
            correctionState,
            expectedProject,
            pair.ExpectedPassed ? "PASS_TRUE" : "PASS_FALSE",
            warningText,
            citationState,
            boundaryState,
            abstentionState,
            "RESULT_MATCHES_EVIDENCE_INDEX",
            "VERIFIER_RESULT_GATE_PASSED",
            "FINAL_PASS",
            preferredToken
        ]);
        var rejectedBody = pair.RejectedType switch
        {
            "accepts_invalid_citation" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE INVALID_CITATION_ACCEPTED RESULT_CONTRADICTS_EVIDENCE_INDEX\n[WRONG: accepts_invalid_citation allows an answer to cite evidence that is not present in the project wiki index.]",
            "accepts_wrong_project" =>
                $"BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE EXPECTED_PROJECT_{TokenSafe(pair.ExpectedProjectId)} ANSWER_PROJECT_{TokenSafe(pair.Answer.ProjectId)} PROJECT_BOUNDARY_MISMATCH_IGNORED\n[WRONG: accepts_wrong_project ignores project boundary and can answer from another repository.]",
            "rejects_valid_answer" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_FALSE WARNING_FALSE_FAILURE VALID_LOCAL_EVIDENCE_REJECTED USEFUL_ANSWER_BLOCKED\n[WRONG: rejects_valid_answer blocks a correctly cited project-local answer and prevents useful local wiki responses.]",
            "accepts_missing_evidence" =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_TRUE WARNINGS_NONE UNCITED_ANSWER_ACCEPTED MISSING_EVIDENCE_UNDECLARED\n[WRONG: accepts_missing_evidence treats an uncited answer as grounded even though no missing-evidence abstention was declared.]",
            _ =>
                "BAD_PROJECT_ANSWER_VERIFIER_RESULT PASS_FALSE WARNING_FALSE_MISSING_EVIDENCE_FAILURE MISSING_EVIDENCE_ABSTENTION_REJECTED WARNING_SET_INCOMPLETE TRACE_REMEDIATION_UNUSABLE\n[WRONG: incomplete_verifier_result omits required warning details needed for WikiQueryTrace remediation.]"
        };
        var rejected = string.Join(" ", [
            rejectedToken,
            defectType,
            "REJECTED_IS_WRONG",
            "CHOSEN_IS_CORRECT",
            rejectedBody,
            "VERIFIER_RESULT_GATE_FAILED",
            "FINAL_FAIL",
            rejectedToken
        ]);

        return new ProjectWikiDpoMetricPair(
            pair.PairId,
            prompt,
            chosen,
            rejected);
    }

    private static string PreferenceTokens(string token, int count)
        => string.Join(" ", Enumerable.Repeat(token, count));

    private static string TokenSafe(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars).Trim('_');
    }
}
