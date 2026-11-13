// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;
using ContinuousLearningTp1EvaluationRunner;
using TranslationDomain;

namespace LlmFromScratch.Tests;

public sealed class ContinuousLearningTp1EvaluationReviewRunnerTests
{
    private const string ModelId = "qwen3.5:9b";
    private const string ManifestDigest =
        "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7";
    private const string ModelLayerDigest =
        "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c";
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string EvidenceRef2 =
        "sha256:123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0";
    private const string OrderingRef =
        "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string GoldHash =
        "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string GoldHash2 =
        "abababababababababababababababababababababababababababababababab";
    private const string TeacherHash =
        "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private const string TeacherHash2 =
        "cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd";
    private const string LearnerHash =
        "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
    private const string RequestHash =
        "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
    private const string RequestHash2 =
        "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";

    private static readonly JsonSerializerOptions SnakeJson = Options(
        JsonNamingPolicy.SnakeCaseLower);
    private static readonly JsonSerializerOptions CamelJson = Options(
        JsonNamingPolicy.CamelCase);

    [Fact]
    public async Task ValidOrderedRuns_PublishExactThreeArtifactEvalOnlyReviewRequest()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 2);
            var indexJson = File.ReadAllText(fixture.IndexPath, Encoding.UTF8);
            Assert.Contains("\"sequenceOrdinal\":0", indexJson, StringComparison.Ordinal);
            Assert.DoesNotContain("\"ordinal\"", indexJson, StringComparison.Ordinal);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr);
            Assert.Equal(
                Tp1EvaluationReviewCli.ArtifactLeafNames.Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(fixture.Destination)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));
            using var request = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1EvaluationReviewCli.ReviewRequestLeaf)));
            var rootElement = request.RootElement;
            Assert.Equal(
                "REVIEW_REQUEST_CREATED_NOT_ACCEPTED",
                rootElement.GetProperty("status").GetString());
            Assert.False(rootElement.GetProperty("ordering_verified").GetBoolean());
            Assert.True(rootElement.GetProperty("eval_only").GetBoolean());
            Assert.False(rootElement.GetProperty("independent_acceptance").GetBoolean());
            Assert.False(rootElement.GetProperty("tp1_exit_gate_evaluated").GetBoolean());
            Assert.False(rootElement.GetProperty("local_teacher_ceiling_evaluated").GetBoolean());
            Assert.False(rootElement.GetProperty("dpo_train_ready").GetBoolean());
            Assert.False(rootElement.GetProperty("teacher_output_used_for_training").GetBoolean());
            Assert.False(rootElement.GetProperty("training_started").GetBoolean());
            Assert.Equal(0, rootElement.GetProperty("dpo_steps_executed").GetInt32());
            Assert.False(rootElement.GetProperty("checkpoint_promoted").GetBoolean());
            Assert.True(rootElement.GetProperty("human_decision_required").GetBoolean());
            var aggregate = rootElement.GetProperty("aggregate");
            Assert.Equal(2, aggregate.GetProperty("run_count").GetInt32());
            Assert.Equal(2, aggregate.GetProperty("total_case_observations").GetInt32());
            Assert.Equal(1D, aggregate.GetProperty("mean_held_out_accuracy_ygoal").GetDouble());
            Assert.Equal(0.4D, aggregate.GetProperty("mean_held_out_accuracy_ybest").GetDouble(), 12);
            Assert.Equal(1D, aggregate.GetProperty("pooled_teacher_beat_rate").GetDouble());
            Assert.Single(rootElement.GetProperty("adjacent_deltas").EnumerateArray());
            foreach (var run in rootElement.GetProperty("runs").EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Null, run
                    .GetProperty("measurement_noise_floor").ValueKind);
                Assert.Equal(
                    MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                    run.GetProperty("measurement_noise_status").GetString());
            }

            var markdown = File.ReadAllText(Path.Combine(
                fixture.Destination,
                Tp1EvaluationReviewCli.ReviewMarkdownLeaf),
                Encoding.UTF8);
            foreach (var heading in new[]
                     {
                         "Запрос на независимую проверку TP1",
                         "Что собрано",
                         "Что арифметически проверено",
                         "Сравнение последовательных прогонов",
                         "Что этот пакет не доказывает",
                         "Решение остаётся за Любой",
                     })
                Assert.Contains(heading, markdown, StringComparison.Ordinal);
            Assert.Contains("не независимая приёмка", markdown, StringComparison.Ordinal);
            Assert.Contains("не подписаны", markdown, StringComparison.Ordinal);
            Assert.Contains("не подлинность автора", markdown, StringComparison.Ordinal);
            Assert.Contains("повторно не выполнялся", markdown, StringComparison.Ordinal);
            Assert.Contains("не разрешает TP2, DPO, обучение", markdown, StringComparison.Ordinal);
            Assert.Contains("разность помечена как недоказанная", markdown, StringComparison.Ordinal);

            var allPublished = string.Join('\n', Directory.EnumerateFiles(
                    fixture.Destination).Select(File.ReadAllText));
            Assert.DoesNotContain(fixture.Root, allPublished, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("raw prompt", allPublished, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("chosen", allPublished, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rejected", allPublished, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(RequestHash, allPublished, StringComparison.Ordinal);

            using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1EvaluationReviewCli.ReviewRunLeaf)));
            var publication = manifest.RootElement;
            Assert.False(publication.TryGetProperty("review_run_sha256", out _));
            Assert.Matches("^sha256:[0-9a-f]{64}$",
                publication.GetProperty("review_index_sha256").GetString());
            Assert.Matches("^sha256:[0-9a-f]{64}$",
                publication.GetProperty("review_request_sha256").GetString());
            Assert.Matches("^sha256:[0-9a-f]{64}$",
                publication.GetProperty("review_markdown_sha256").GetString());
            Assert.Equal(2, publication.GetProperty("source_artifacts").GetArrayLength());
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("bom")]
    [InlineData("sequence-ordinal")]
    [InlineData("legacy-ordinal")]
    [InlineData("relative")]
    [InlineData("ordering")]
    public async Task StrictReviewIndex_RejectsMalformedInputs(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            var json = File.ReadAllText(fixture.IndexPath, Encoding.UTF8);
            switch (mutation)
            {
                case "unknown":
                    json = json.Replace("{", "{\"unknown\":true,", StringComparison.Ordinal);
                    break;
                case "duplicate":
                    json = json.Replace(
                        "{",
                        "{\"reviewPacketId\":\"duplicate\",",
                        StringComparison.Ordinal);
                    break;
                case "sequence-ordinal":
                    json = json.Replace(
                        "\"sequenceOrdinal\":0",
                        "\"sequenceOrdinal\":1",
                        StringComparison.Ordinal);
                    break;
                case "legacy-ordinal":
                    json = json.Replace(
                        "\"sequenceOrdinal\":0",
                        "\"ordinal\":0",
                        StringComparison.Ordinal);
                    break;
                case "relative":
                    json = json.Replace(
                        fixture.RunDirectories[0].Replace("\\", "\\\\", StringComparison.Ordinal),
                        "relative-run",
                        StringComparison.Ordinal);
                    break;
                case "ordering":
                    json = json.Replace(OrderingRef, OrderingRef.ToUpperInvariant(), StringComparison.Ordinal);
                    break;
            }
            var bytes = Encoding.UTF8.GetBytes(json);
            if (mutation == "bom")
                bytes = Encoding.UTF8.Preamble.ToArray().Concat(bytes).ToArray();
            File.WriteAllBytes(fixture.IndexPath, bytes);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_INPUT_INVALID", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("candidate-score")]
    [InlineData("exact-match")]
    [InlineData("selected-link")]
    [InlineData("learner-range")]
    [InlineData("teacher-win")]
    [InlineData("report")]
    [InlineData("noise-status")]
    [InlineData("noise-floor")]
    [InlineData("noise-floor-status")]
    [InlineData("roster")]
    [InlineData("training-flag")]
    [InlineData("manifest-digest")]
    [InlineData("model-layer-digest")]
    public async Task ArithmeticIdentityAndLegalMutations_FailClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            var run = fixture.RunDirectories[0];
            if (mutation is "candidate-score" or "exact-match")
            {
                var candidate = Read<CandidateScoreArtifact>(run,
                    OfflineScoringCli.CandidateScoresLeaf);
                candidate = mutation == "candidate-score"
                    ? candidate with { CandidateScore = 0.9D }
                    : candidate with { ExactMatch = false };
                WriteJsonLines(Path.Combine(run, OfflineScoringCli.CandidateScoresLeaf),
                    [candidate]);
            }
            else if (mutation is "selected-link" or "learner-range" or "teacher-win")
            {
                var comparison = Read<CaseComparisonArtifact>(run,
                    OfflineScoringCli.CaseComparisonsLeaf);
                comparison = mutation switch
                {
                    "selected-link" => comparison with { SelectedTeacherOutputSha256 = LearnerHash },
                    "learner-range" => comparison with { LearnerScore = 1.1D },
                    _ => comparison with { TeacherBeatLearner = false },
                };
                WriteJsonLines(Path.Combine(run, OfflineScoringCli.CaseComparisonsLeaf),
                    [comparison]);
            }
            else if (mutation == "report")
            {
                var report = Read<TeacherStageGapReport>(run,
                    OfflineScoringCli.StageGapReportLeaf);
                WriteJson(Path.Combine(run, OfflineScoringCli.StageGapReportLeaf),
                    report with { HeldOutAccuracyYgoal = 0.9D });
            }
            else if (mutation is "noise-status" or "noise-floor" or "noise-floor-status")
            {
                var report = Read<TeacherStageGapReport>(run,
                    OfflineScoringCli.StageGapReportLeaf);
                WriteJson(Path.Combine(run, OfflineScoringCli.StageGapReportLeaf),
                    mutation switch
                    {
                        "noise-status" => report with
                        {
                            // Без паспорта нельзя заявлять, что разница доказана.
                            MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
                        },
                        "noise-floor" => report with { MeasurementNoiseFloor = 0.01D },
                        _ => report with
                        {
                            MeasurementNoiseFloor = 0.01D,
                            MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus
                        }
                    });
            }
            else
            {
                var manifest = Read<OfflineScoringManifest>(run,
                    OfflineScoringCli.ScoringManifestLeaf);
                manifest = mutation switch
                {
                    "roster" => manifest with { EvaluationRosterSha256 = OrderingRef },
                    "training-flag" => manifest with { TrainingStarted = true },
                    "manifest-digest" => manifest with { OllamaManifestDigest = new string('F', 64) },
                    _ => manifest with { OllamaModelLayerDigest = new string('F', 64) },
                };
                WriteJson(Path.Combine(run, OfflineScoringCli.ScoringManifestLeaf), manifest);
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_EVIDENCE_MISMATCH", result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("run-id")]
    [InlineData("source-run-id")]
    [InlineData("window")]
    [InlineData("tuple")]
    public async Task ReplayOrCopiedEvidence_CannotIncreaseRunCount(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 2);
            var first = fixture.RunDirectories[0];
            var second = fixture.RunDirectories[1];
            var firstManifest = Read<OfflineScoringManifest>(first,
                OfflineScoringCli.ScoringManifestLeaf);
            var secondManifest = Read<OfflineScoringManifest>(second,
                OfflineScoringCli.ScoringManifestLeaf);
            secondManifest = mutation switch
            {
                "run-id" => secondManifest with { RunId = firstManifest.RunId },
                "source-run-id" => secondManifest with { SourceRunId = firstManifest.SourceRunId },
                "window" => secondManifest with { WindowRef = firstManifest.WindowRef },
                _ => secondManifest,
            };
            if (mutation == "tuple")
            {
                foreach (var leaf in new[]
                         {
                             OfflineScoringCli.CandidateScoresLeaf,
                             OfflineScoringCli.CaseComparisonsLeaf,
                             OfflineScoringCli.StageGapReportLeaf,
                             OfflineScoringCli.ScoringManifestLeaf,
                         })
                    File.Copy(Path.Combine(first, leaf), Path.Combine(second, leaf), overwrite: true);
                var copied = Read<OfflineScoringManifest>(second,
                    OfflineScoringCli.ScoringManifestLeaf);
                WriteJson(Path.Combine(second, OfflineScoringCli.ScoringManifestLeaf),
                    copied with { RunId = Path.GetFileName(second) });
            }
            else
            {
                WriteJson(Path.Combine(second, OfflineScoringCli.ScoringManifestLeaf),
                    secondManifest);
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_EVIDENCE_MISMATCH", result.Stderr,
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("scorer")]
    [InlineData("roster")]
    [InlineData("split-proof")]
    [InlineData("model")]
    [InlineData("retention")]
    public async Task CrossRunInvariantDrift_FailsClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 2);
            var run = fixture.RunDirectories[1];
            var manifest = Read<OfflineScoringManifest>(run,
                OfflineScoringCli.ScoringManifestLeaf);
            manifest = mutation switch
            {
                "scorer" => manifest with { ScorerVersion = "v2" },
                "roster" => manifest with { EvaluationRosterSha256 = OrderingRef },
                "split-proof" => manifest with { SplitProofSha256 = OrderingRef },
                "model" => manifest with { ModelId = "qwen3.5:27b" },
                _ => manifest with { LocalRetentionPolicyRef = OrderingRef },
            };
            WriteJson(Path.Combine(run, OfflineScoringCli.ScoringManifestLeaf), manifest);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_EVIDENCE_MISMATCH", result.Stderr,
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExactSourceSetAndRunLeafBinding_AreFailClosed()
    {
        foreach (var mutation in new[] { "extra-file", "subdirectory", "leaf-mismatch" })
        {
            var root = CreateTempRoot();
            try
            {
                var fixture = CreateFixture(root, runCount: 1);
                var run = fixture.RunDirectories[0];
                if (mutation == "extra-file")
                    File.WriteAllText(Path.Combine(run, "extra.json"), "{}", Encoding.UTF8);
                else if (mutation == "subdirectory")
                    Directory.CreateDirectory(Path.Combine(run, "nested"));
                else
                {
                    var manifest = Read<OfflineScoringManifest>(run,
                        OfflineScoringCli.ScoringManifestLeaf);
                    WriteJson(Path.Combine(run, OfflineScoringCli.ScoringManifestLeaf),
                        manifest with { RunId = "other-run" });
                }

                var result = await RunAsync(fixture);

                Assert.Equal(1, result.ExitCode);
                Assert.False(Directory.Exists(fixture.Destination));
            }
            finally { DeleteIfExists(root); }
        }
    }

    [Fact]
    public async Task ProducerByteMutationWithoutSemanticChange_IsRejected()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            var path = Path.Combine(
                fixture.RunDirectories[0],
                OfflineScoringCli.CandidateScoresLeaf);
            var text = File.ReadAllText(path, Encoding.UTF8)
                .Replace("{", "{ ", StringComparison.Ordinal);
            File.WriteAllText(path, text, new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_EVIDENCE_MISMATCH", result.Stderr,
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("index")]
    [InlineData("line")]
    public async Task ResourceBounds_FailBeforePublication(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            if (mutation == "index")
            {
                var valid = File.ReadAllBytes(fixture.IndexPath);
                File.WriteAllBytes(
                    fixture.IndexPath,
                    valid.Concat(new byte[1024 * 1024]).ToArray());
            }
            else
            {
                File.WriteAllText(
                    Path.Combine(
                        fixture.RunDirectories[0],
                        OfflineScoringCli.CandidateScoresLeaf),
                    new string(' ', 196_609),
                    new UTF8Encoding(false));
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_INPUT_INVALID", result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(4_095, 1, true)]
    [InlineData(4_096, 1, false)]
    public void PerRunCombinedRowLimit_HasExact4096Boundary(
        int candidateCount,
        int caseCount,
        bool expected)
        => Assert.Equal(
            expected,
            Tp1EvaluationReviewCli.IsWithinPerRunRowLimit(
                candidateCount,
                caseCount));

    [Fact]
    public async Task RequestHashes_AreUniqueAcrossEntireRunNotOnlyWithinCase()
    {
        var acceptedRoot = CreateTempRoot();
        var rejectedRoot = CreateTempRoot();
        try
        {
            var accepted = CreateFixture(acceptedRoot, runCount: 1);
            RewriteAsTwoCaseRun(accepted.RunDirectories[0], reuseRequestHash: false);
            var rejected = CreateFixture(rejectedRoot, runCount: 1);
            RewriteAsTwoCaseRun(rejected.RunDirectories[0], reuseRequestHash: true);

            var acceptedResult = await RunAsync(accepted);
            var rejectedResult = await RunAsync(rejected);

            Assert.Equal(0, acceptedResult.ExitCode);
            Assert.Equal(1, rejectedResult.ExitCode);
            Assert.Contains(
                "TP1_REVIEW_EVIDENCE_MISMATCH",
                rejectedResult.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(rejected.Destination));
        }
        finally
        {
            DeleteIfExists(acceptedRoot);
            DeleteIfExists(rejectedRoot);
        }
    }

    [Tp1ReviewWindowsFileSymlinkFact]
    public async Task CorrectlyNamedExternalArtifactFileSymlink_IsRejectedBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            var run = fixture.RunDirectories[0];
            var candidatePath = Path.Combine(
                run,
                OfflineScoringCli.CandidateScoresLeaf);
            var externalDirectory = Path.Combine(root, "external-evidence");
            Directory.CreateDirectory(externalDirectory);
            var externalPath = Path.Combine(externalDirectory, "candidate.jsonl");
            File.Copy(candidatePath, externalPath);
            File.Delete(candidatePath);
            File.CreateSymbolicLink(candidatePath, externalPath);
            Assert.NotEqual(
                FileAttributes.None,
                File.GetAttributes(candidatePath) & FileAttributes.ReparsePoint);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_INPUT_INVALID", result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExistingDestinationAndPublicationFailure_LeaveNoPartialReview()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            Directory.CreateDirectory(fixture.Destination);

            var collision = await RunAsync(fixture);

            Assert.Equal(1, collision.ExitCode);
            Assert.Contains("TP1_REVIEW_PUBLICATION_FAILED", collision.Stderr,
                StringComparison.Ordinal);
            Assert.Empty(Directory.EnumerateFiles(fixture.Destination));
            Directory.Delete(fixture.Destination);

            var failed = await RunAsync(fixture, failOnPublish: true);

            Assert.Equal(1, failed.ExitCode);
            Assert.Contains("TP1_REVIEW_PUBLICATION_FAILED", failed.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task CancelledAssembly_PublishesNothingAndReturnsStableCancellation()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = await RunAsync(fixture, cancellationToken: cancellation.Token);

            Assert.Equal(130, result.ExitCode);
            Assert.Contains("TP1_REVIEW_CANCELLED", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task SameEvidenceAndIndex_ProduceByteIdenticalArtifacts()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 2);

            Assert.Equal(0, (await RunAsync(fixture)).ExitCode);
            var first = Tp1EvaluationReviewCli.ArtifactLeafNames.ToDictionary(
                static leaf => leaf,
                leaf => File.ReadAllBytes(Path.Combine(fixture.Destination, leaf)),
                StringComparer.Ordinal);
            Directory.Delete(fixture.Destination, recursive: true);
            Assert.Equal(0, (await RunAsync(fixture)).ExitCode);
            foreach (var leaf in Tp1EvaluationReviewCli.ArtifactLeafNames)
                Assert.Equal(
                    first[leaf],
                    File.ReadAllBytes(Path.Combine(fixture.Destination, leaf)));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ProductionPolicy_RejectsLexicalProjectsOutputBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, runCount: 1);
            var args = BuildArgs(fixture);
            args[Array.IndexOf(args, "--output-root") + 1] =
                Path.Combine(@"C:\Projects", $"tp1-review-prohibited-{Guid.NewGuid():N}");
            var createCalls = 0;
            var dependencies = new Tp1EvaluationReviewDependencies(
                (output, packet, leaves) =>
                {
                    createCalls++;
                    return OperationResult<IEvaluationPublicationBoundary>.Fail("no", "no");
                },
                rejectOutputUnderProjects: true);

            var result = await RunDirectAsync(args, dependencies);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_REVIEW_INPUT_INVALID", result.Stderr,
                StringComparison.Ordinal);
            Assert.Equal(0, createCalls);
        }
        finally { DeleteIfExists(root); }
    }

    private static ReviewFixture CreateFixture(string root, int runCount)
    {
        var indexDirectory = Path.Combine(root, "index");
        var runsRoot = Path.Combine(root, "runs");
        var outputRoot = Path.Combine(root, "published");
        Directory.CreateDirectory(indexDirectory);
        Directory.CreateDirectory(runsRoot);
        var runs = new List<string>();
        var indexRuns = new List<Tp1ReviewRunIndexInput>();
        for (var ordinal = 0; ordinal < runCount; ordinal++)
        {
            var runId = $"score-run-{ordinal + 1:D3}";
            var directory = Path.Combine(runsRoot, runId);
            Directory.CreateDirectory(directory);
            WriteValidRun(directory, runId, ordinal);
            runs.Add(directory);
            indexRuns.Add(new Tp1ReviewRunIndexInput(ordinal, directory));
        }
        var indexPath = Path.Combine(indexDirectory, "review-index.json");
        WriteJson(indexPath, new Tp1ReviewIndexInput(
            "review-packet-001",
            OrderingRef,
            indexRuns), CamelJson);
        return new ReviewFixture(
            root,
            indexPath,
            outputRoot,
            Path.Combine(outputRoot, "review-packet-001"),
            runs);
    }

    private static void WriteValidRun(string directory, string runId, int ordinal)
    {
        var candidate = new CandidateScoreArtifact(
            "evaluation-set-1",
            "case-1",
            LanguageTranslateHeldOutScorer.DomainTag,
            "language-held-out-scorer",
            "v1",
            "human-owned-threshold-set",
            EvidenceRef,
            TeacherHash,
            ExactMatch: true,
            CharacterAccuracy: 1D,
            CandidateScore: 1D,
            GenerationIndices: [0],
            TeacherRequestRefSha256s: [RequestHash],
            EvalOnly: true);
        var comparison = new CaseComparisonArtifact(
            "evaluation-set-1",
            "case-1",
            LanguageTranslateHeldOutScorer.DomainTag,
            "language-held-out-scorer",
            "v1",
            "human-owned-threshold-set",
            EvidenceRef,
            GoldHash,
            EvidenceRef,
            TeacherHash,
            SelectedTeacherScore: 1D,
            SelectedTeacherGenerationIndices: [0],
            SelectedTeacherRequestRefSha256s: [RequestHash],
            LearnerHash,
            EvidenceRef,
            LearnerScore: 0.4D,
            TeacherBeatLearner: true,
            EvalOnly: true);
        var window = "sha256:" + ordinal.ToString("x64");
        var roster = OfflineScoringCli.ComputeEvaluationRosterSha256(
        [
            new EvaluationRosterEntry(
                "evaluation-set-1",
                "case-1",
                LanguageTranslateHeldOutScorer.DomainTag,
                EvidenceRef,
                LanguageTranslateHeldOutScorer.HeldOutSplit,
                GoldHash,
                EvidenceRef),
        ]);
        var report = new TeacherStageGapReport(
            "TP1_LOCAL",
            "language-held-out-scorer",
            "v1",
            "human-owned-threshold-set",
            HeldOutAccuracyYgoal: 1D,
            HeldOutAccuracyYbest: 0.4D,
            TeacherGapToGold: 0D,
            TeacherBeatRate: 1D,
            StageGap9bVs235b: null,
            EvalOnly: true,
            window);
        var manifest = new OfflineScoringManifest(
            runId,
            "TP1_LOCAL",
            "evaluation-set-1",
            LanguageTranslateHeldOutScorer.DomainTag,
            "language-held-out-scorer",
            "v1",
            "human-owned-threshold-set",
            window,
            InferredK: 1,
            CaseCount: 1,
            SemanticCandidateCount: 1,
            EvidenceRef,
            $"source-run-{ordinal + 1:D3}",
            ModelId,
            "ollama_manifest_sha256",
            ManifestDigest,
            ModelLayerDigest,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            roster,
            "sha256:" + (ordinal + 16).ToString("x64"),
            "sha256:" + (ordinal + 32).ToString("x64"),
            "sha256:" + (ordinal + 48).ToString("x64"),
            EvidenceRef,
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
        WriteJsonLines(Path.Combine(directory, OfflineScoringCli.CandidateScoresLeaf),
            [candidate]);
        WriteJsonLines(Path.Combine(directory, OfflineScoringCli.CaseComparisonsLeaf),
            [comparison]);
        WriteJson(Path.Combine(directory, OfflineScoringCli.StageGapReportLeaf), report);
        WriteJson(Path.Combine(directory, OfflineScoringCli.ScoringManifestLeaf), manifest);
    }

    private static void RewriteAsTwoCaseRun(
        string directory,
        bool reuseRequestHash)
    {
        var firstCandidate = Read<CandidateScoreArtifact>(
            directory,
            OfflineScoringCli.CandidateScoresLeaf);
        var firstComparison = Read<CaseComparisonArtifact>(
            directory,
            OfflineScoringCli.CaseComparisonsLeaf);
        var secondRequestHash = reuseRequestHash ? RequestHash : RequestHash2;
        var secondCandidate = firstCandidate with
        {
            CaseId = "case-2",
            SourceRef = EvidenceRef2,
            OutputSha256 = TeacherHash2,
            TeacherRequestRefSha256s = [secondRequestHash],
        };
        var secondComparison = firstComparison with
        {
            CaseId = "case-2",
            SourceRef = EvidenceRef2,
            GoldLabelSha256 = GoldHash2,
            SelectedTeacherOutputSha256 = TeacherHash2,
            SelectedTeacherRequestRefSha256s = [secondRequestHash],
        };
        var roster = OfflineScoringCli.ComputeEvaluationRosterSha256(
        [
            new EvaluationRosterEntry(
                firstComparison.EvaluationSetId,
                firstComparison.CaseId,
                firstComparison.DomainId,
                firstComparison.SourceRef,
                LanguageTranslateHeldOutScorer.HeldOutSplit,
                firstComparison.GoldLabelSha256,
                firstComparison.GoldAuthorityRef),
            new EvaluationRosterEntry(
                secondComparison.EvaluationSetId,
                secondComparison.CaseId,
                secondComparison.DomainId,
                secondComparison.SourceRef,
                LanguageTranslateHeldOutScorer.HeldOutSplit,
                secondComparison.GoldLabelSha256,
                secondComparison.GoldAuthorityRef),
        ]);
        var manifest = Read<OfflineScoringManifest>(
            directory,
            OfflineScoringCli.ScoringManifestLeaf) with
        {
            CaseCount = 2,
            SemanticCandidateCount = 2,
            EvaluationRosterSha256 = roster,
        };
        WriteJsonLines(
            Path.Combine(directory, OfflineScoringCli.CandidateScoresLeaf),
            [firstCandidate, secondCandidate]);
        WriteJsonLines(
            Path.Combine(directory, OfflineScoringCli.CaseComparisonsLeaf),
            [firstComparison, secondComparison]);
        WriteJson(
            Path.Combine(directory, OfflineScoringCli.ScoringManifestLeaf),
            manifest);
    }

    private static async Task<CliResult> RunAsync(
        ReviewFixture fixture,
        bool failOnPublish = false,
        CancellationToken cancellationToken = default)
    {
        var dependencies = new Tp1EvaluationReviewDependencies(
            (outputRoot, packetId, leaves) =>
                OperationResult<IEvaluationPublicationBoundary>.Ok(
                    new FakeReviewPublicationBoundary(
                        outputRoot,
                        packetId,
                        leaves,
                        failOnPublish)),
            rejectOutputUnderProjects: false);
        return await RunDirectAsync(BuildArgs(fixture), dependencies, cancellationToken);
    }

    private static async Task<CliResult> RunDirectAsync(
        string[] args,
        Tp1EvaluationReviewDependencies dependencies,
        CancellationToken cancellationToken = default)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exit = await Tp1EvaluationReviewCli.RunAsync(
            args,
            stdout,
            stderr,
            cancellationToken,
            dependencies);
        return new CliResult(exit, stdout.ToString(), stderr.ToString());
    }

    private static string[] BuildArgs(ReviewFixture fixture)
        =>
        [
            Tp1EvaluationReviewCli.ArmFlag,
            "--review-index-json", fixture.IndexPath,
            "--output-root", fixture.OutputRoot,
        ];

    private static T Read<T>(string directory, string leaf)
        => JsonSerializer.Deserialize<T>(
            File.ReadLines(Path.Combine(directory, leaf)).Single(),
            SnakeJson)!;

    private static void WriteJson<T>(string path, T value)
        => WriteJson(path, value, SnakeJson);

    private static void WriteJson<T>(
        string path,
        T value,
        JsonSerializerOptions options)
        => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(value, options));

    private static void WriteJsonLines<T>(string path, IEnumerable<T> rows)
        => File.WriteAllLines(
            path,
            rows.Select(row => JsonSerializer.Serialize(row, SnakeJson)),
            new UTF8Encoding(false));

    private static JsonSerializerOptions Options(JsonNamingPolicy namingPolicy)
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

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-tp1-review-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void AssertNoStaging(string outputRoot)
    {
        if (Directory.Exists(outputRoot))
            Assert.Empty(Directory.EnumerateDirectories(outputRoot, ".*.tp1-review-tmp-*"));
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record ReviewFixture(
        string Root,
        string IndexPath,
        string OutputRoot,
        string Destination,
        IReadOnlyList<string> RunDirectories);

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed class FakeReviewPublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly HashSet<string> _allowed;
        private readonly HashSet<string> _created = new(StringComparer.Ordinal);
        private readonly string _expectedOutputRoot;
        private readonly bool _failOnPublish;
        private bool _published;

        internal FakeReviewPublicationBoundary(
            string outputRoot,
            string packetId,
            IReadOnlyCollection<string> leaves,
            bool failOnPublish)
        {
            _allowed = new HashSet<string>(leaves, StringComparer.Ordinal);
            _expectedOutputRoot = Path.GetFullPath(outputRoot);
            _failOnPublish = failOnPublish;
            Directory.CreateDirectory(outputRoot);
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{packetId}.tp1-review-tmp-{Guid.NewGuid():N}");
            FinalDirectory = Path.Combine(outputRoot, packetId);
            Directory.CreateDirectory(StagingDirectory);
        }

        public string StagingDirectory { get; }
        public string FinalDirectory { get; }
        public string CurrentDirectory => _published ? FinalDirectory : StagingDirectory;

        public void VerifyOutputRootIdentity(string expectedPhysicalPath)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(
                    Path.TrimEndingDirectorySeparator(expectedPhysicalPath),
                    Path.TrimEndingDirectorySeparator(_expectedOutputRoot)))
                throw new IOException("Unexpected output identity.");
        }

        public void VerifyStagingIdentity()
        {
            if (_published || !Directory.Exists(StagingDirectory))
                throw new IOException("Staging identity changed.");
        }

        public FileStream CreateArtifactFile(string leafName)
        {
            if (!_allowed.Contains(leafName) || !_created.Add(leafName))
                throw new IOException("Unexpected review leaf.");
            return new FileStream(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
        }

        public void PublishAndVerify()
        {
            if (_failOnPublish)
                throw new IOException("Injected publish failure.");
            if (!_created.SetEquals(_allowed))
                throw new IOException("Review set incomplete.");
            Directory.Move(StagingDirectory, FinalDirectory);
            _published = true;
        }

        public void CleanupFailedPublication()
        {
            if (Directory.Exists(CurrentDirectory))
                Directory.Delete(CurrentDirectory, recursive: true);
        }

        public void Dispose() { }
    }
}

file sealed class Tp1ReviewWindowsFileSymlinkFactAttribute : FactAttribute
{
    public Tp1ReviewWindowsFileSymlinkFactAttribute()
        => Skip = Tp1ReviewFileSymlinkTestSupport.GetCapabilitySkipReason();
}

file static class Tp1ReviewFileSymlinkTestSupport
{
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);
    private const int HResultOperationNotSupported = unchecked((int)0x80070032);
    private const int HResultInvalidFunction = unchecked((int)0x80070001);
    private const int HResultAccessDenied = unchecked((int)0x80070005);

    internal static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: Windows no-follow handle proof is required.";

        var root = Path.Combine(
            Path.GetTempPath(),
            $"tp1-review-file-symlink-capability-{Guid.NewGuid():N}");
        var target = Path.Combine(root, "target.json");
        var link = Path.Combine(root, "link.json");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(target, "{}", new UTF8Encoding(false));
            try
            {
                File.CreateSymbolicLink(link, target);
                return (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0
                    ? null
                    : "REPARSE_TEST_PLATFORM_LIMITATION: created link was not a reparse point.";
            }
            catch (Exception exception) when (IsCapabilityLimitation(exception))
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: "
                    + $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}.";
            }
        }
        finally
        {
            if (File.Exists(link))
                File.Delete(link);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static bool IsCapabilityLimitation(Exception exception)
        => exception is NotSupportedException
            || exception is UnauthorizedAccessException
                && exception.HResult == HResultAccessDenied
            || exception is IOException
                && exception.HResult is HResultPrivilegeNotHeld
                    or HResultOperationNotSupported
                    or HResultInvalidFunction;
}
