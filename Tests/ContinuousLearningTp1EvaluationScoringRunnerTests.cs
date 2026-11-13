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

public sealed class ContinuousLearningTp1EvaluationScoringRunnerTests
{
    private const string ModelId = "qwen3.5:9b";
    private const string ManifestDigest =
        "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7";
    private const string ModelLayerDigest =
        "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c";
    private const string Salt =
        "YXVkaXQtc2FsdC1zZWNyZXQtMDEyMzQ1Njc4OUFCQ0Q=";
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string TrainingRef =
        "sha256:1111111111111111111111111111111111111111111111111111111111111111";
    private const string GoldRef =
        "sha256:2222222222222222222222222222222222222222222222222222222222222222";
    private const string LearnerRef =
        "sha256:3333333333333333333333333333333333333333333333333333333333333333";
    private const string ProofRef =
        "sha256:4444444444444444444444444444444444444444444444444444444444444444";
    private const string WindowRef =
        "sha256:5555555555555555555555555555555555555555555555555555555555555555";
    private const string EvidenceRef2 =
        "sha256:6666666666666666666666666666666666666666666666666666666666666666";
    private const string ScorerId = "language-held-out-scorer";
    private const string ScorerVersion = "v1";
    private const string ThresholdSetId = "human-owned-threshold-set";

    private static readonly JsonSerializerOptions SnakeJson = Options(
        JsonNamingPolicy.SnakeCaseLower);
    private static readonly JsonSerializerOptions CamelJson = Options(
        JsonNamingPolicy.CamelCase);

    [Fact]
    public async Task SuccessfulOfflineScoring_PublishesExactEvalOnlyEvidenceWithoutRawOutputs()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec(
                    "case-1",
                    EvidenceRef,
                    "hello",
                    "hallo",
                    [
                        new OutputSpec(0, "hxllo", "request-b"),
                        new OutputSpec(1, "hello", "request-a"),
                    ]),
            ]);
            var sourceHashes = SourceHashes(fixture);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr);
            var destination = fixture.Destination;
            Assert.Equal(
                OfflineScoringCli.ArtifactLeafNames.Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(destination)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));
            Assert.Equal(sourceHashes, SourceHashes(fixture));

            using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                destination,
                OfflineScoringCli.StageGapReportLeaf)));
            Assert.Equal("TP1_LOCAL", report.RootElement.GetProperty("stage").GetString());
            Assert.Equal(1D, report.RootElement
                .GetProperty("held_out_accuracy_ygoal").GetDouble());
            Assert.Equal(0.4D, report.RootElement
                .GetProperty("held_out_accuracy_ybest").GetDouble(), 12);
            Assert.Equal(0D, report.RootElement
                .GetProperty("teacher_gap_to_gold").GetDouble());
            Assert.Equal(1D, report.RootElement
                .GetProperty("teacher_beat_rate").GetDouble());
            Assert.Equal(JsonValueKind.Null, report.RootElement
                .GetProperty("stage_gap9b_vs235b").ValueKind);
            Assert.True(report.RootElement.GetProperty("eval_only").GetBoolean());
            Assert.Equal(JsonValueKind.Null, report.RootElement
                .GetProperty("measurement_noise_floor").ValueKind);
            Assert.Equal(
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
                report.RootElement.GetProperty("measurement_noise_status").GetString());

            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                destination,
                OfflineScoringCli.ScoringManifestLeaf)));
            var run = manifest.RootElement;
            Assert.Equal(2, run.GetProperty("inferred_k").GetInt32());
            Assert.Equal(1, run.GetProperty("case_count").GetInt32());
            Assert.Equal("source-run-1", run.GetProperty("source_run_id").GetString());
            Assert.Equal(ModelId, run.GetProperty("model_id").GetString());
            Assert.Equal(
                "ollama_manifest_sha256",
                run.GetProperty("teacher_artifact_digest_kind").GetString());
            Assert.Equal(
                ManifestDigest,
                run.GetProperty("ollama_manifest_digest").GetString());
            Assert.Equal(
                ModelLayerDigest,
                run.GetProperty("ollama_model_layer_digest").GetString());
            Assert.Equal(
                EvidenceRef,
                run.GetProperty("local_retention_policy_ref").GetString());
            Assert.Equal(
                EvidenceRef,
                run.GetProperty("output_license_ref").GetString());
            Assert.Equal(
                EvidenceRef,
                run.GetProperty("teacher_license_evidence_ref").GetString());
            Assert.Equal(
                EvidenceRef,
                run.GetProperty("deployment_evidence_ref").GetString());
            Assert.Matches(
                "^sha256:[0-9a-f]{64}$",
                run.GetProperty("evaluation_roster_sha256").GetString());
            Assert.True(run.GetProperty("eval_only").GetBoolean());
            Assert.True(run.GetProperty("scoring_executed").GetBoolean());
            Assert.True(run.GetProperty("teacher_stage_gap_report_created").GetBoolean());
            Assert.False(run.GetProperty("dpo_train_ready").GetBoolean());
            Assert.False(run.GetProperty("teacher_output_used_for_training").GetBoolean());
            Assert.False(run.GetProperty("training_started").GetBoolean());
            Assert.Equal(0, run.GetProperty("dpo_steps_executed").GetInt32());
            Assert.False(run.GetProperty("checkpoint_promoted").GetBoolean());
            Assert.False(run.GetProperty("tp1_exit_gate_evaluated").GetBoolean());
            Assert.False(run.GetProperty("local_teacher_ceiling_evaluated").GetBoolean());
            Assert.True(run.GetProperty("human_decision_required").GetBoolean());

            var published = string.Join(
                '\n',
                Directory.EnumerateFiles(destination)
                    .Select(File.ReadAllText));
            Assert.DoesNotContain("hello", published, StringComparison.Ordinal);
            Assert.DoesNotContain("hallo", published, StringComparison.Ordinal);
            Assert.DoesNotContain("hxllo", published, StringComparison.Ordinal);
            Assert.DoesNotContain(
                LocalRequestId("request-a"),
                published,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                LocalRequestId("request-b"),
                published,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                fixture.SourceRunDirectory,
                published,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                fixture.ComparisonPath,
                published,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                fixture.SplitProofPath,
                published,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"chosen\"", published, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"rejected\"", published, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GraphDpoPair", published, StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task BestOfK_IsPermutationInvariantForAllSemanticArtifacts()
    {
        var firstRoot = CreateTempRoot();
        var secondRoot = CreateTempRoot();
        try
        {
            var outputs = new[]
            {
                new OutputSpec(0, "abce", "request-0"),
                new OutputSpec(1, "abcd", "request-1"),
                new OutputSpec(2, "abcf", "request-2"),
            };
            var first = CreateFixture(firstRoot,
            [
                new CaseSpec("case-1", EvidenceRef, "abcd", "abcz", outputs),
            ]);
            var second = CreateFixture(secondRoot,
            [
                new CaseSpec(
                    "case-1",
                    EvidenceRef,
                    "abcd",
                    "abcz",
                    outputs.Reverse().ToArray()),
            ]);

            Assert.Equal(0, (await RunAsync(first)).ExitCode);
            Assert.Equal(0, (await RunAsync(second)).ExitCode);
            foreach (var leaf in new[]
                     {
                         OfflineScoringCli.CandidateScoresLeaf,
                         OfflineScoringCli.CaseComparisonsLeaf,
                         OfflineScoringCli.StageGapReportLeaf,
                     })
            {
                Assert.Equal(
                    File.ReadAllBytes(Path.Combine(first.Destination, leaf)),
                    File.ReadAllBytes(Path.Combine(second.Destination, leaf)));
            }
        }
        finally
        {
            DeleteIfExists(firstRoot);
            DeleteIfExists(secondRoot);
        }
    }

    [Fact]
    public async Task EvaluationRosterFingerprint_IsOrderInvariant()
    {
        var firstRoot = CreateTempRoot();
        var secondRoot = CreateTempRoot();
        try
        {
            var cases = new[]
            {
                new CaseSpec("case-a", EvidenceRef, "gold-a", "learner-a",
                    [new OutputSpec(0, "teacher-a", "request-a")]),
                new CaseSpec("case-b", EvidenceRef2, "gold-b", "learner-b",
                    [new OutputSpec(0, "teacher-b", "request-b")]),
            };
            var first = CreateFixture(firstRoot, cases);
            var second = CreateFixture(secondRoot, cases.Reverse().ToArray());

            Assert.Equal(0, (await RunAsync(first)).ExitCode);
            Assert.Equal(0, (await RunAsync(second)).ExitCode);
            Assert.Equal(
                ReadScoringManifestString(first, "evaluation_roster_sha256"),
                ReadScoringManifestString(second, "evaluation_roster_sha256"));
        }
        finally
        {
            DeleteIfExists(firstRoot);
            DeleteIfExists(secondRoot);
        }
    }

    [Theory]
    [InlineData("case")]
    [InlineData("source")]
    [InlineData("gold")]
    public async Task EvaluationRosterFingerprint_ChangesOnAuthorityTupleChange(
        string mutation)
    {
        var baselineRoot = CreateTempRoot();
        var changedRoot = CreateTempRoot();
        try
        {
            var baseline = CreateFixture(baselineRoot,
            [
                new CaseSpec("case-1", EvidenceRef, "gold", "learner",
                    [new OutputSpec(0, "teacher", "request-1")]),
            ]);
            var changed = CreateFixture(changedRoot,
            [
                new CaseSpec(
                    mutation == "case" ? "case-2" : "case-1",
                    mutation == "source" ? EvidenceRef2 : EvidenceRef,
                    mutation == "gold" ? "changed-gold" : "gold",
                    "learner",
                    [new OutputSpec(0, "teacher", "request-2")]),
            ]);

            Assert.Equal(0, (await RunAsync(baseline)).ExitCode);
            Assert.Equal(0, (await RunAsync(changed)).ExitCode);
            Assert.NotEqual(
                ReadScoringManifestString(baseline, "evaluation_roster_sha256"),
                ReadScoringManifestString(changed, "evaluation_roster_sha256"));
        }
        finally
        {
            DeleteIfExists(baselineRoot);
            DeleteIfExists(changedRoot);
        }
    }

    [Fact]
    public async Task EqualTeacherScores_UseJsonStringShaTieBreakAndLearnerTieIsNotTeacherWin()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec(
                    "case-1",
                    EvidenceRef,
                    "abcd",
                    "abce",
                    [
                        new OutputSpec(0, "abce", "request-0"),
                        new OutputSpec(1, "abcf", "request-1"),
                    ]),
            ]);

            Assert.Equal(0, (await RunAsync(fixture)).ExitCode);

            using var comparison = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                fixture.Destination,
                OfflineScoringCli.CaseComparisonsLeaf)));
            var row = comparison.RootElement;
            var expectedWinner = new[] { HashJsonString("abce"), HashJsonString("abcf") }
                .Order(StringComparer.Ordinal)
                .First();
            Assert.Equal(
                expectedWinner,
                row.GetProperty("selected_teacher_output_sha256").GetString());
            Assert.False(row.GetProperty("teacher_beat_learner").GetBoolean());
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task IdenticalOutputs_AreOneSemanticCandidateWithAllSortedReferences()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec(
                    "case-1",
                    EvidenceRef,
                    "hello",
                    "other",
                    [
                        new OutputSpec(0, "hello", "request-z"),
                        new OutputSpec(1, "hello", "request-a"),
                    ]),
            ]);

            Assert.Equal(0, (await RunAsync(fixture)).ExitCode);

            var line = Assert.Single(File.ReadAllLines(Path.Combine(
                fixture.Destination,
                OfflineScoringCli.CandidateScoresLeaf)));
            using var candidate = JsonDocument.Parse(line);
            Assert.Equal(
                new[] { 0, 1 },
                candidate.RootElement.GetProperty("generation_indices")
                    .EnumerateArray().Select(static value => value.GetInt32()).ToArray());
            Assert.Equal(
                new[]
                {
                    HashJsonString(LocalRequestId("request-a")),
                    HashJsonString(LocalRequestId("request-z")),
                }.Order(StringComparer.Ordinal),
                candidate.RootElement.GetProperty("teacher_request_ref_sha256s")
                    .EnumerateArray().Select(static value => value.GetString()).ToArray());
            var published = File.ReadAllText(Path.Combine(
                fixture.Destination,
                OfflineScoringCli.CandidateScoresLeaf));
            Assert.DoesNotContain(LocalRequestId("request-a"), published, StringComparison.Ordinal);
            Assert.DoesNotContain(LocalRequestId("request-z"), published, StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task MissingDuplicateGappedAndMixedKInputs_FailClosedWithoutPublication()
    {
        foreach (var failure in new[] { "missing", "duplicate", "gapped", "mixed-k" })
        {
            var root = CreateTempRoot();
            try
            {
                var fixture = failure is "mixed-k" or "missing"
                    ? CreateFixture(root,
                    [
                        new CaseSpec("case-1", EvidenceRef, "a", "x",
                        [
                            new OutputSpec(0, "a", "request-0"),
                            new OutputSpec(1, "b", "request-1"),
                        ]),
                        new CaseSpec("case-2", EvidenceRef2, "c", "y",
                        [
                            new OutputSpec(0, "c", "request-2"),
                            .. (failure == "missing"
                                ? [new OutputSpec(1, "d", "request-3")]
                                : Array.Empty<OutputSpec>()),
                        ]),
                    ])
                    : CreateFixture(root,
                    [
                        new CaseSpec("case-1", EvidenceRef, "a", "x",
                        [
                            new OutputSpec(0, "a", "request-0"),
                            new OutputSpec(failure == "gapped" ? 2 : 1, "b", "request-1"),
                        ]),
                    ]);
                if (failure == "missing")
                {
                    var first = JsonSerializer.Deserialize<ComparisonInputRow>(
                        File.ReadLines(fixture.ComparisonPath).First(),
                        CamelJson)!;
                    WriteJsonLines(fixture.ComparisonPath, new[] { first }, CamelJson);
                    WriteJson(
                        fixture.SplitProofPath,
                        ReadProof(fixture) with { EvaluationSourceRefs = [EvidenceRef] },
                        CamelJson);
                }
                else if (failure == "duplicate")
                {
                    var row = ReadComparison(fixture);
                    WriteJsonLines(fixture.ComparisonPath, new[] { row, row }, CamelJson);
                }

                var result = await RunAsync(fixture);

                Assert.Equal(1, result.ExitCode);
                Assert.False(Directory.Exists(fixture.Destination));
                AssertNoStaging(fixture.OutputRoot);
            }
            finally { DeleteIfExists(root); }
        }
    }

    [Fact]
    public async Task UnsafeSourceProvenanceManifestAndSplitProof_AreRejected()
    {
        foreach (var failure in new[] { "manifest", "provenance", "split" })
        {
            var root = CreateTempRoot();
            try
            {
                var fixture = CreateFixture(root,
                [
                    new CaseSpec("case-1", EvidenceRef, "a", "x",
                    [
                        new OutputSpec(0, "a", "request-0"),
                    ]),
                ]);
                if (failure == "manifest")
                {
                    var manifest = ReadManifest(fixture) with { TrainingStarted = true };
                    WriteJson(fixture.SourceManifestPath, manifest, SnakeJson);
                }
                else if (failure == "provenance")
                {
                    var observation = ReadObservation(fixture);
                    observation = observation with
                    {
                        Provenance = observation.Provenance with
                        {
                            TeacherOutputUsedForTraining = true,
                        },
                    };
                    WriteJsonLines(
                        fixture.SourceObservationPath,
                        new[] { observation },
                        SnakeJson);
                }
                else
                {
                    var proof = ReadProof(fixture) with
                    {
                        CrossSplitNearDuplicateCount = 1,
                    };
                    WriteJson(fixture.SplitProofPath, proof, CamelJson);
                }

                var result = await RunAsync(fixture);

                Assert.Equal(1, result.ExitCode);
                Assert.False(Directory.Exists(fixture.Destination));
            }
            finally { DeleteIfExists(root); }
        }
    }

    [Theory]
    [InlineData("request-id")]
    [InlineData("invocation-count")]
    [InlineData("artifact-kind")]
    [InlineData("artifact-digest")]
    public async Task SourceProvenance_RequiresExactLocalClientRequestShape(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var observation = ReadObservation(fixture);
            observation = observation with
            {
                Provenance = mutation switch
                {
                    "request-id" => observation.Provenance with
                    {
                        RequestId = "raw teacher output alias",
                    },
                    "invocation-count" => observation.Provenance with
                    {
                        InvocationCount = 2
                    },
                    "artifact-kind" => observation.Provenance with
                    {
                        ModelArtifactDigestKind = "ollama_manifest_sha256"
                    },
                    "artifact-digest" => observation.Provenance with
                    {
                        ModelArtifactDigest = new string('e', 64)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(mutation))
                },
            };
            WriteJsonLines(
                fixture.SourceObservationPath,
                new[] { observation },
                SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("non-empty")]
    public async Task SourceProvenance_RequiresPresentEmptyTrainingContentBinding(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            const string emptyProperty = ",\"training_content_sha256\":\"\"";
            var sourceJson = File.ReadAllText(fixture.SourceObservationPath);
            Assert.Contains(emptyProperty, sourceJson, StringComparison.Ordinal);
            var mutatedJson = mutation switch
            {
                "missing" => sourceJson.Replace(
                    emptyProperty,
                    string.Empty,
                    StringComparison.Ordinal),
                "non-empty" => sourceJson.Replace(
                    emptyProperty,
                    $",\"training_content_sha256\":\"{new string('a', 64)}\"",
                    StringComparison.Ordinal),
                _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
            };
            File.WriteAllText(
                fixture.SourceObservationPath,
                mutatedJson,
                new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_OFFLINE_SCORING_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.Contains(
                mutation == "missing"
                    ? "JSON properties do not match the scoring schema."
                    : "A source evaluation observation is not exact local eval-only provenance.",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ReplayedSourceRequestIdAcrossContiguousGenerations_FailsClosed()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "replayed-request"),
                    new OutputSpec(1, "b", "replayed-request"),
                ]),
            ]);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_OFFLINE_SCORING_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("qwen3.5")]
    [InlineData("llama3.2:3b")]
    public async Task ForgedUntaggedOrWrongFamilyModelIdentity_FailsClosed(
        string forgedModelId)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            WriteJson(
                fixture.SourceManifestPath,
                ReadManifest(fixture) with { ModelId = forgedModelId },
                SnakeJson);
            var observation = ReadObservation(fixture);
            WriteJsonLines(
                fixture.SourceObservationPath,
                new[]
                {
                    observation with
                    {
                        Provenance = observation.Provenance with
                        {
                            ModelId = forgedModelId,
                        },
                    },
                },
                SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_OFFLINE_SCORING_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("whitespace")]
    [InlineData("nul")]
    public async Task InvalidSourceTeacherOutput_FailsClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var observation = ReadObservation(fixture) with
            {
                Output = mutation == "whitespace" ? " \t\r\n " : "answer\0tail",
            };
            WriteJsonLines(
                fixture.SourceObservationPath,
                new[] { observation },
                SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("source-run.")]
    public async Task UnsafeSourceManifestWindowsRunId_FailsClosed(string runId)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            WriteJson(
                fixture.SourceManifestPath,
                ReadManifest(fixture) with { RunId = runId },
                SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task FreeFormIdentityAlias_IsRejectedBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            WriteJsonLines(
                fixture.ComparisonPath,
                new[]
                {
                    ReadComparison(fixture) with
                    {
                        EvaluationSetId = "raw output hidden in identity",
                    },
                },
                CamelJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("domain")]
    [InlineData("license")]
    [InlineData("node")]
    [InlineData("scorer-version")]
    [InlineData("learner-hash")]
    [InlineData("semantic-claim")]
    [InlineData("gold-mismatch")]
    public async Task ComparisonAuthorityAndIdentityMismatches_FailClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var row = ReadComparison(fixture);
            row = mutation switch
            {
                "domain" => row with { DomainId = "other-domain" },
                "license" => row with { GoldLicense = "external" },
                "node" => row with { GoldFromNodeUnderTest = true },
                "scorer-version" => row with { ScorerVersion = "v2" },
                "learner-hash" => row with
                {
                    LearnerBestOutputSha256 = new string('0', 64),
                },
                "semantic-claim" => row with { SemanticQualityClaimed = true },
                "gold-mismatch" => row with { ChosenTranslation = "b" },
                _ => throw new InvalidOperationException(),
            };
            WriteJsonLines(fixture.ComparisonPath, new[] { row }, CamelJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task AggregateScoringWorkLimit_FailsBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var expected = new string('a', 2_500);
            var fixture = CreateFixture(root,
            [
                new CaseSpec(
                    "case-1",
                    EvidenceRef,
                    expected,
                    new string('c', 2_500),
                    Enumerable.Range(0, 6)
                        .Select(index => new OutputSpec(
                            index,
                            new string((char)('b' + index), 2_500),
                            $"request-{index}"))
                        .ToArray()),
            ]);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PublicationFailure_CleansStagingAndDoesNotLeaveFinalDirectory()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();
            var exit = await OfflineScoringCli.RunAsync(
                BuildArgs(fixture),
                stdout,
                stderr,
                CancellationToken.None,
                new OfflineScoringDependencies((outputRoot, runId, leaves) =>
                    OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            leaves,
                            failOnPublish: true))));

            Assert.Equal(1, exit);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("bom")]
    public async Task StrictComparisonJson_RejectsUnknownDuplicateAndBom(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var bytes = File.ReadAllBytes(fixture.ComparisonPath);
            if (mutation == "bom")
            {
                File.WriteAllBytes(
                    fixture.ComparisonPath,
                    Encoding.UTF8.Preamble.ToArray().Concat(bytes).ToArray());
            }
            else
            {
                var text = Encoding.UTF8.GetString(bytes);
                text = mutation == "unknown"
                    ? text.Replace("{", "{\"unexpected\":true,", StringComparison.Ordinal)
                    : text.Replace(
                        "{",
                        "{\"caseId\":\"duplicate\",",
                        StringComparison.Ordinal);
                File.WriteAllText(fixture.ComparisonPath, text, new UTF8Encoding(false));
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task RunnerMode_ExhaustivelyRequiresExactlyOneOfSixArms()
    {
        var flags = new[]
        {
            "--execute-evaluation",
            OfflineScoringCli.ArmFlag,
            Tp1EvaluationReviewCli.ArmFlag,
            Tp1LocalTeacherCeilingCli.ArmFlag,
            Tp1ExitDecisionRequestCli.ArmFlag,
            Tp1ExitDecisionSubmissionValidationCli.ArmFlag,
        };
        for (var mask = 0; mask < 1 << flags.Length; mask++)
        {
            var args = flags.Where((_, index) => (mask & 1 << index) != 0).ToArray();
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            var exit = await RunnerModeCli.RunAsync(
                args,
                stdout,
                stderr,
                CancellationToken.None);

            Assert.Equal(1, exit);
            var selectedArmCount = Enumerable.Range(0, flags.Length)
                .Count(index => (mask & 1 << index) != 0);
            if (selectedArmCount == 1)
            {
                Assert.DoesNotContain(
                    "TP1_RUNNER_ARM_INVALID",
                    stderr.ToString(),
                    StringComparison.Ordinal);
            }
            else
            {
                Assert.Contains(
                    "TP1_RUNNER_ARM_INVALID",
                    stderr.ToString(),
                    StringComparison.Ordinal);
                Assert.Contains(
                    "Exactly one TP1 runner arm flag is required.",
                    stderr.ToString(),
                    StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task ForgedSourceAndProvenanceWithConsistentValidManifestDigest_FailsExpectedPin()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                    [new OutputSpec(0, "a", "request-0")]),
            ]);
            var forgedDigest = new string('a', 64);
            WriteJson(
                fixture.SourceManifestPath,
                ReadManifest(fixture) with { OllamaManifestDigest = forgedDigest },
                SnakeJson);
            var observation = ReadObservation(fixture);
            WriteJsonLines(
                fixture.SourceObservationPath,
                new[]
                {
                    observation with
                    {
                        Provenance = observation.Provenance with
                        {
                            ModelVersion = forgedDigest,
                        },
                    },
                },
                SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("retention")]
    [InlineData("output-license")]
    [InlineData("teacher-license")]
    [InlineData("deployment")]
    [InlineData("model-layer")]
    public async Task SourceManifestLegalAndLayerPins_CannotSelfAuthorize(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                    [new OutputSpec(0, "a", "request-0")]),
            ]);
            var otherRef = EvidenceRef2;
            var manifest = ReadManifest(fixture);
            manifest = mutation switch
            {
                "retention" => manifest with { LocalRetentionPolicyRef = otherRef },
                "output-license" => manifest with { OutputLicenseRef = otherRef },
                "teacher-license" => manifest with { TeacherLicenseEvidenceRef = otherRef },
                "deployment" => manifest with { DeploymentEvidenceRef = otherRef },
                "model-layer" => manifest with
                {
                    OllamaModelLayerDigest = new string('a', 64),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
            };
            WriteJson(fixture.SourceManifestPath, manifest, SnakeJson);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("--expected-model-id")]
    [InlineData("--expected-ollama-manifest-digest")]
    [InlineData("--expected-ollama-model-layer-digest")]
    [InlineData("--expected-local-retention-policy-ref")]
    [InlineData("--expected-output-license-ref")]
    [InlineData("--expected-teacher-license-evidence-ref")]
    [InlineData("--expected-deployment-evidence-ref")]
    public async Task IndependentAuthorityPins_AreAllMandatory(string option)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                    [new OutputSpec(0, "a", "request-0")]),
            ]);
            var values = BuildArgs(fixture).ToList();
            var index = values.IndexOf(option);
            Assert.True(index >= 0);
            values.RemoveRange(index, 2);
            var createCalls = 0;
            var exit = await OfflineScoringCli.RunAsync(
                values.ToArray(),
                new StringWriter(),
                new StringWriter(),
                CancellationToken.None,
                new OfflineScoringDependencies((outputRoot, runId, leaves) =>
                {
                    createCalls++;
                    return OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, runId, leaves));
                }));

            Assert.Equal(1, exit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("bad/name")]
    [InlineData("CON")]
    [InlineData("run.")]
    public async Task UnsafeWindowsRunId_IsRejectedBeforePublication(string runId)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var args = BuildArgs(fixture);
            args[Array.IndexOf(args, "--run-id") + 1] = runId;
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();
            var createCalls = 0;
            var exit = await OfflineScoringCli.RunAsync(
                args,
                stdout,
                stderr,
                CancellationToken.None,
                new OfflineScoringDependencies((outputRoot, requestedRunId, leaves) =>
                {
                    createCalls++;
                    return OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, requestedRunId, leaves));
                }));

            Assert.Equal(1, exit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(Path.GetFullPath(
                Path.Combine(fixture.OutputRoot, runId))));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("comparison")]
    [InlineData("output")]
    public async Task PhysicalIdentityChange_IsRejectedBeforePublication(string changedPath)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var comparisonResolutions = 0;
            var outputResolutions = 0;
            var createCalls = 0;
            OperationResult<string> Resolve(string path, bool allowMissing)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(path, fixture.ComparisonPath)
                    && ++comparisonResolutions > 1
                    && changedPath == "comparison")
                {
                    return OperationResult<string>.Ok(Path.Combine(root, "swapped.jsonl"));
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(path, fixture.OutputRoot)
                    && ++outputResolutions > 1
                    && changedPath == "output")
                {
                    return OperationResult<string>.Ok(Path.Combine(root, "swapped-output"));
                }
                return PhysicalPathResolver.Resolve(path, allowMissing);
            }
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            var exit = await OfflineScoringCli.RunAsync(
                BuildArgs(fixture),
                stdout,
                stderr,
                CancellationToken.None,
                new OfflineScoringDependencies(
                    (outputRoot, runId, leaves) =>
                    {
                        createCalls++;
                        return OperationResult<IEvaluationPublicationBoundary>.Ok(
                            new FakePublicationBoundary(outputRoot, runId, leaves));
                    },
                    resolvePhysicalPath: Resolve));

            Assert.Equal(1, exit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task SourceSetChangeDuringRetainedValidation_IsRejectedWithoutPublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var splitResolutions = 0;
            var createCalls = 0;
            OperationResult<string> Resolve(string path, bool allowMissing)
            {
                var resolved = PhysicalPathResolver.Resolve(path, allowMissing);
                if (StringComparer.OrdinalIgnoreCase.Equals(path, fixture.SplitProofPath)
                    && ++splitResolutions == 2)
                {
                    File.WriteAllText(
                        Path.Combine(fixture.SourceRunDirectory, "unexpected.txt"),
                        "unexpected",
                        new UTF8Encoding(false));
                }
                return resolved;
            }
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            var exit = await OfflineScoringCli.RunAsync(
                BuildArgs(fixture),
                stdout,
                stderr,
                CancellationToken.None,
                new OfflineScoringDependencies(
                    (outputRoot, runId, leaves) =>
                    {
                        createCalls++;
                        return OperationResult<IEvaluationPublicationBoundary>.Ok(
                            new FakePublicationBoundary(outputRoot, runId, leaves));
                    },
                    resolvePhysicalPath: Resolve));

            Assert.Equal(1, exit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ProductionPathPolicy_AllowsExternalSourceAndRejectsProjectsOutput()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var allowedRoot = CreateTempRoot();
        var rejectedRoot = CreateTempRoot();
        try
        {
            var allowed = CreateFixture(allowedRoot,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            Assert.False(allowed.SourceRunDirectory.StartsWith(
                @"C:\Projects\",
                StringComparison.OrdinalIgnoreCase));
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();
            var productionPolicy = new OfflineScoringDependencies(
                (outputRoot, runId, leaves) =>
                    OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, runId, leaves)),
                rejectOutputUnderProjects: true);

            var allowedExit = await OfflineScoringCli.RunAsync(
                BuildArgs(allowed),
                stdout,
                stderr,
                CancellationToken.None,
                productionPolicy);

            Assert.Equal(0, allowedExit);
            Assert.True(Directory.Exists(allowed.Destination));

            var rejected = CreateFixture(rejectedRoot,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-1"),
                ]),
            ]);
            var rejectedArgs = BuildArgs(rejected);
            rejectedArgs[Array.IndexOf(rejectedArgs, "--output-root") + 1] =
                Path.Combine(@"C:\Projects", $"tp1-prohibited-{Guid.NewGuid():N}");
            var createCalls = 0;
            var rejectedPolicy = new OfflineScoringDependencies(
                (outputRoot, runId, leaves) =>
                {
                    createCalls++;
                    return OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, runId, leaves));
                },
                rejectOutputUnderProjects: true);

            var rejectedExit = await OfflineScoringCli.RunAsync(
                rejectedArgs,
                new StringWriter(),
                new StringWriter(),
                CancellationToken.None,
                rejectedPolicy);

            Assert.Equal(1, rejectedExit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(rejected.Destination));
        }
        finally
        {
            DeleteIfExists(allowedRoot);
            DeleteIfExists(rejectedRoot);
        }
    }

    [Fact]
    public async Task ProductionPathPolicy_RejectsOutputThatPhysicallyResolvesUnderProjects()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root,
            [
                new CaseSpec("case-1", EvidenceRef, "a", "x",
                [
                    new OutputSpec(0, "a", "request-0"),
                ]),
            ]);
            var createCalls = 0;
            OperationResult<string> Resolve(string path, bool allowMissing)
                => StringComparer.OrdinalIgnoreCase.Equals(path, fixture.OutputRoot)
                    ? OperationResult<string>.Ok(
                        Path.Combine(@"C:\Projects", "physical-output-alias"))
                    : PhysicalPathResolver.Resolve(path, allowMissing);
            var dependencies = new OfflineScoringDependencies(
                (outputRoot, runId, leaves) =>
                {
                    createCalls++;
                    return OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, runId, leaves));
                },
                rejectOutputUnderProjects: true,
                resolvePhysicalPath: Resolve);

            var exit = await OfflineScoringCli.RunAsync(
                BuildArgs(fixture),
                new StringWriter(),
                new StringWriter(),
                CancellationToken.None,
                dependencies);

            Assert.Equal(1, exit);
            Assert.Equal(0, createCalls);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public void WindowsPublicationBoundary_UsesTheSeparateFourArtifactAllowlist()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateTempRoot();
        IEvaluationPublicationBoundary? publication = null;
        var published = false;
        try
        {
            var output = Path.Combine(root, "published");
            Directory.CreateDirectory(output);
            var created = WindowsEvaluationPublicationBoundary.Create(
                output,
                "run-001",
                OfflineScoringCli.ArtifactLeafNames);
            Assert.True(created.IsSuccess, created.ErrorMessage);
            publication = Assert.IsAssignableFrom<IEvaluationPublicationBoundary>(created.Value);
            publication.VerifyStagingIdentity();
            Assert.Throws<IOException>(() => publication.CreateArtifactFile(
                "tp1-evaluation-observations.jsonl"));
            foreach (var leaf in OfflineScoringCli.ArtifactLeafNames)
            {
                using var file = publication.CreateArtifactFile(leaf);
                file.WriteByte((byte)'\n');
                file.Flush(flushToDisk: true);
            }
            publication.PublishAndVerify();
            published = true;
            Assert.Equal(
                OfflineScoringCli.ArtifactLeafNames.Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(publication.FinalDirectory)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));
        }
        finally
        {
            if (!published && publication is not null)
            {
                try { publication.CleanupFailedPublication(); }
                catch (Exception) { }
            }
            publication?.Dispose();
            DeleteIfExists(root);
        }
    }

    private static Fixture CreateFixture(string root, IReadOnlyList<CaseSpec> cases)
    {
        var sourceRun = Path.Combine(root, "source-run-1");
        var input = Path.Combine(root, "input");
        var output = Path.Combine(root, "published");
        Directory.CreateDirectory(sourceRun);
        Directory.CreateDirectory(input);
        var observationPath = Path.Combine(
            sourceRun,
            "tp1-evaluation-observations.jsonl");
        var manifestPath = Path.Combine(sourceRun, "tp1-evaluation-run.json");
        var comparisonPath = Path.Combine(input, "comparison.jsonl");
        var proofPath = Path.Combine(input, "split-proof.json");
        var observations = cases.SelectMany(item => item.Outputs.Select(outputRow =>
            new EvaluationObservation(
                "evaluation-set-1",
                item.CaseId,
                LanguageTranslateHeldOutScorer.DomainTag,
                outputRow.GenerationIndex,
                ScorerId,
                item.SourceRef,
                LanguageTranslateHeldOutScorer.HeldOutSplit,
                outputRow.Output,
                Provenance(outputRow.RequestId),
                DpoTrainReady: false,
                TeacherOutputUsedForTraining: false,
                TrainingStarted: false,
                DpoStepsExecuted: 0,
                CheckpointPromoted: false,
                ScoringExecuted: false))).ToArray();
        var manifest = new EvaluationManifest(
            "source-run-1",
            observations.Length,
            observations.Length,
            ModelId,
            ManifestDigest,
            ModelLayerDigest,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            DpoTrainReady: false,
            TeacherOutputUsedForTraining: false,
            TrainingStarted: false,
            DpoStepsExecuted: 0,
            CheckpointPromoted: false,
            ScoringExecuted: false);
        var comparisons = cases.Select(item => new ComparisonInputRow(
            "evaluation-set-1",
            item.CaseId,
            LanguageTranslateHeldOutScorer.DomainTag,
            ScorerId,
            ScorerVersion,
            ThresholdSetId,
            item.SourceRef,
            LanguageTranslateHeldOutScorer.HeldOutSplit,
            item.ExpectedLabel,
            item.ExpectedLabel,
            LanguageTranslateHeldOutScorer.SelfAuthoredOrigin,
            "project_internal_self_authored",
            GoldRef,
            GoldFromNodeUnderTest: false,
            SemanticQualityClaimed: false,
            item.LearnerOutput,
            HashJsonString(item.LearnerOutput),
            LearnerRef)).ToArray();
        var evaluationRefs = cases
            .Select(static item => item.SourceRef)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var proof = new SplitProofInput(
            "evaluation-set-1",
            LanguageTranslateHeldOutScorer.HeldOutSplit,
            [TrainingRef],
            evaluationRefs,
            CrossSplitNearDuplicateCount: 0,
            NearDuplicateAlgorithm: "normalized_levenshtein_v1",
            NearDuplicateThreshold: 0.85D,
            ProofRef);
        WriteJsonLines(observationPath, observations, SnakeJson);
        WriteJson(manifestPath, manifest, SnakeJson);
        WriteJsonLines(comparisonPath, comparisons, CamelJson);
        WriteJson(proofPath, proof, CamelJson);
        return new Fixture(
            sourceRun,
            observationPath,
            manifestPath,
            comparisonPath,
            proofPath,
            output,
            Path.Combine(output, "run-001"));
    }

    private static async Task<CliResult> RunAsync(Fixture fixture)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var dependencies = new OfflineScoringDependencies(
            (outputRoot, runId, leaves) =>
                OperationResult<IEvaluationPublicationBoundary>.Ok(
                    new FakePublicationBoundary(outputRoot, runId, leaves)));
        var exit = await OfflineScoringCli.RunAsync(
            BuildArgs(fixture),
            stdout,
            stderr,
            CancellationToken.None,
            dependencies);
        return new CliResult(exit, stdout.ToString(), stderr.ToString());
    }

    private static string[] BuildArgs(Fixture fixture)
        =>
        [
            OfflineScoringCli.ArmFlag,
            "--source-run-directory", fixture.SourceRunDirectory,
            "--comparison-jsonl", fixture.ComparisonPath,
            "--split-proof-json", fixture.SplitProofPath,
            "--output-root", fixture.OutputRoot,
            "--run-id", "run-001",
            "--audit-salt-base64", Salt,
            "--scorer-id", ScorerId,
            "--scorer-version", ScorerVersion,
            "--threshold-set-id", ThresholdSetId,
            "--window-ref", WindowRef,
            "--expected-model-id", ModelId,
            "--expected-ollama-manifest-digest", ManifestDigest,
            "--expected-ollama-model-layer-digest", ModelLayerDigest,
            "--expected-local-retention-policy-ref", EvidenceRef,
            "--expected-output-license-ref", EvidenceRef,
            "--expected-teacher-license-evidence-ref", EvidenceRef,
            "--expected-deployment-evidence-ref", EvidenceRef,
        ];

    private static TrainingTeacherProvenance Provenance(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            ModelId,
            ManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: false,
            LocalRequestId(requestId),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = ModelLayerDigest
        };

    private static string LocalRequestId(string alias)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(alias)))
            .ToLowerInvariant()[..32];

    private static string HashJsonString(string value)
        => Convert.ToHexString(SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(value)))
            .ToLowerInvariant();

    private static string[] SourceHashes(Fixture fixture)
        =>
        [
            Convert.ToHexString(SHA256.HashData(
                File.ReadAllBytes(fixture.SourceManifestPath))),
            Convert.ToHexString(SHA256.HashData(
                File.ReadAllBytes(fixture.SourceObservationPath))),
        ];

    private static EvaluationManifest ReadManifest(Fixture fixture)
        => JsonSerializer.Deserialize<EvaluationManifest>(
            File.ReadAllBytes(fixture.SourceManifestPath),
            SnakeJson)!;

    private static string ReadScoringManifestString(
        Fixture fixture,
        string propertyName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            fixture.Destination,
            OfflineScoringCli.ScoringManifestLeaf)));
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static EvaluationObservation ReadObservation(Fixture fixture)
        => JsonSerializer.Deserialize<EvaluationObservation>(
            File.ReadLines(fixture.SourceObservationPath).Single(),
            SnakeJson)!;

    private static ComparisonInputRow ReadComparison(Fixture fixture)
        => JsonSerializer.Deserialize<ComparisonInputRow>(
            File.ReadLines(fixture.ComparisonPath).Single(),
            CamelJson)!;

    private static SplitProofInput ReadProof(Fixture fixture)
        => JsonSerializer.Deserialize<SplitProofInput>(
            File.ReadAllBytes(fixture.SplitProofPath),
            CamelJson)!;

    private static void WriteJson<T>(
        string path,
        T value,
        JsonSerializerOptions options)
        => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(value, options));

    private static void WriteJsonLines<T>(
        string path,
        IEnumerable<T> rows,
        JsonSerializerOptions options)
        => File.WriteAllLines(
            path,
            rows.Select(row => JsonSerializer.Serialize(row, options)),
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
            $"xiigen-tp1-score-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void AssertNoStaging(string outputRoot)
    {
        if (Directory.Exists(outputRoot))
            Assert.Empty(Directory.EnumerateDirectories(outputRoot, "*.tp1-score-tmp-*"));
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record OutputSpec(
        int GenerationIndex,
        string Output,
        string RequestId);

    private sealed record CaseSpec(
        string CaseId,
        string SourceRef,
        string ExpectedLabel,
        string LearnerOutput,
        IReadOnlyList<OutputSpec> Outputs);

    private sealed record Fixture(
        string SourceRunDirectory,
        string SourceObservationPath,
        string SourceManifestPath,
        string ComparisonPath,
        string SplitProofPath,
        string OutputRoot,
        string Destination);

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed class FakePublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly HashSet<string> _allowed;
        private readonly HashSet<string> _created = new(StringComparer.Ordinal);
        private readonly string _expectedOutputRoot;
        private bool _published;

        internal FakePublicationBoundary(
            string outputRoot,
            string runId,
            IReadOnlyCollection<string> allowed,
            bool failOnPublish = false)
        {
            _allowed = new HashSet<string>(allowed, StringComparer.Ordinal);
            _expectedOutputRoot = Path.GetFullPath(outputRoot);
            FailOnPublish = failOnPublish;
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{runId}.tp1-score-tmp-{Guid.NewGuid():N}");
            FinalDirectory = Path.Combine(outputRoot, runId);
            Directory.CreateDirectory(StagingDirectory);
        }

        public string StagingDirectory { get; }
        public string FinalDirectory { get; }
        public string CurrentDirectory => _published ? FinalDirectory : StagingDirectory;
        private bool FailOnPublish { get; }

        public void VerifyOutputRootIdentity(string expectedPhysicalPath)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(
                    Path.TrimEndingDirectorySeparator(expectedPhysicalPath),
                    Path.TrimEndingDirectorySeparator(_expectedOutputRoot)))
                throw new IOException("Unexpected output-root physical identity.");
        }

        public void VerifyStagingIdentity()
        {
            if (!Directory.Exists(StagingDirectory) || _published)
                throw new IOException("Fake staging identity changed.");
        }

        public FileStream CreateArtifactFile(string leafName)
        {
            if (!_allowed.Contains(leafName) || !_created.Add(leafName))
                throw new IOException("Artifact is not in the scoring allowlist.");
            return new FileStream(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 16_384,
                FileOptions.WriteThrough);
        }

        public void PublishAndVerify()
        {
            if (!_allowed.SetEquals(_created))
                throw new IOException("Scoring artifact set is incomplete.");
            if (FailOnPublish)
                throw new IOException("Scripted publication failure.");
            Directory.Move(StagingDirectory, FinalDirectory);
            _published = true;
            if (!_allowed.SetEquals(Directory.EnumerateFiles(FinalDirectory)
                    .Select(Path.GetFileName)!))
                throw new IOException("Published scoring artifact set changed.");
        }

        public void CleanupFailedPublication()
        {
            if (Directory.Exists(StagingDirectory))
                Directory.Delete(StagingDirectory, recursive: true);
            if (!_published && Directory.Exists(FinalDirectory))
                Directory.Delete(FinalDirectory, recursive: true);
        }

        public void Dispose()
        {
        }
    }
}
