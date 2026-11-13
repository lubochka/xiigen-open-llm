// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;
using ContinuousLearningTp1EvaluationRunner;

namespace LlmFromScratch.Tests;

public sealed class ContinuousLearningTp1LocalTeacherCeilingRunnerTests
{
    private const string ReviewPacketId = "tp1-review-packet-001";
    private const string EvaluationId = "tp1-ceiling-evaluation-001";
    private const string ModelId = "qwen3.5:9b";
    private const string ManifestDigest =
        "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7";
    private const string ModelLayerDigest =
        "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c";
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string OrderingRef =
        "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private static readonly JsonSerializerOptions SnakeJson = new()
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

    [Fact]
    public async Task ValidFiveRunPacket_UsesLastThreeAndPublishesCandidateNotAcceptance()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(
                root,
                goals: [0.10D, 0.90D, 0.60D, 0.61D, 0.615D],
                wins: [9, 9, 0, 0, 0]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr);
            Assert.Equal(
                Tp1LocalTeacherCeilingCli.ArtifactLeafNames.Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(fixture.Destination)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));

            var evaluationPath = Path.Combine(
                fixture.Destination,
                Tp1LocalTeacherCeilingCli.EvaluationLeaf);
            var markdownPath = Path.Combine(
                fixture.Destination,
                Tp1LocalTeacherCeilingCli.MarkdownLeaf);
            var runPath = Path.Combine(
                fixture.Destination,
                Tp1LocalTeacherCeilingCli.EvaluationRunLeaf);
            using var evaluation = JsonDocument.Parse(File.ReadAllBytes(evaluationPath));
            var value = evaluation.RootElement;
            Assert.Equal(
                Tp1LocalTeacherCeilingCli.FixedStatus,
                value.GetProperty("status").GetString());
            Assert.Equal(
                Tp1LocalTeacherCeilingCli.FixedPolicyId,
                value.GetProperty("policy").GetProperty("policy_id").GetString());
            Assert.Equal(
                new[] { 2, 3, 4 },
                value.GetProperty("selected_runs")
                    .EnumerateArray()
                    .Select(static run => run.GetProperty("ordinal").GetInt32()));
            Assert.Equal(0.60D, value.GetProperty("first_held_out_accuracy_ygoal").GetDouble());
            Assert.Equal(0.615D, value.GetProperty("best_subsequent_held_out_accuracy_ygoal").GetDouble());
            Assert.True(value.GetProperty("plateau_predicate").GetBoolean());
            Assert.True(value.GetProperty("low_beat_authority_predicate").GetBoolean());
            Assert.True(value.GetProperty("local_teacher_ceiling_evaluated").GetBoolean());
            Assert.True(value.GetProperty("local_teacher_ceiling_candidate").GetBoolean());
            AssertAllNegativeGates(value);

            var evaluationText = File.ReadAllText(evaluationPath, Encoding.UTF8);
            Assert.DoesNotContain(fixture.Root, evaluationText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("teacher_request", evaluationText, StringComparison.Ordinal);
            var markdown = File.ReadAllText(markdownPath, Encoding.UTF8);
            Assert.Contains("оператор строго `<`", markdown, StringComparison.Ordinal);
            Assert.Contains("Отрицательное улучшение означает регрессию", markdown, StringComparison.Ordinal);
            Assert.Contains("не является независимой приёмкой, PASS или TP1 exit", markdown, StringComparison.Ordinal);
            Assert.Contains("Решение остаётся за Любой", markdown, StringComparison.Ordinal);
            Assert.DoesNotContain(fixture.Root, markdown, StringComparison.OrdinalIgnoreCase);

            using var run = JsonDocument.Parse(File.ReadAllBytes(runPath));
            var runValue = run.RootElement;
            Assert.Equal(
                Tp1LocalTeacherCeilingCli.FixedStatus,
                runValue.GetProperty("status").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(evaluationPath)),
                runValue.GetProperty("evaluation_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(markdownPath)),
                runValue.GetProperty("markdown_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.ReviewRequestPath)),
                runValue.GetProperty("review_request_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.ReviewMarkdownPath)),
                runValue.GetProperty("review_markdown_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.ReviewRunPath)),
                runValue.GetProperty("review_run_sha256").GetString());
            Assert.Equal(
                Tp1LocalTeacherCeilingCli.ArtifactLeafNames.ToArray(),
                runValue.GetProperty("published_artifact_leaf_names")
                    .EnumerateArray()
                    .Select(static item => item.GetString()!)
                    .ToArray());
            Assert.Equal(
                Hash(JsonSerializer.SerializeToUtf8Bytes(
                    Tp1LocalTeacherCeilingCli.ArtifactLeafNames,
                    SnakeJson)),
                runValue.GetProperty("published_artifact_roster_sha256").GetString());
            Assert.False(runValue.TryGetProperty("evaluation_run_sha256", out _));
            AssertAllNegativeGates(runValue);
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(0.0D, 0.019999999999D, 0.0D, true)]
    [InlineData(0.0D, 0.02D, 0.0D, false)]
    [InlineData(0.5D, 0.49D, 0.48D, true)]
    public void PlateauPredicate_UsesStrictBestOfLastTwoBoundary(
        double first,
        double second,
        double third,
        bool expected)
        => Assert.Equal(
            expected,
            Tp1LocalTeacherCeilingCli.IsStrictPlateau(first, second, third));

    [Theory]
    [InlineData(0L, 10L, true)]
    [InlineData(1L, 10L, false)]
    [InlineData(1L, 11L, true)]
    [InlineData(2L, 20L, false)]
    public void BeatAuthority_UsesCheckedRationalStrictBoundary(
        long wins,
        long cases,
        bool expected)
        => Assert.Equal(
            expected,
            Tp1LocalTeacherCeilingCli.IsLowBeatAuthority(wins, cases));

    [Fact]
    public async Task ValidWindowAtExactBeatBoundary_IsEvaluatedButNotCandidate()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(
                root,
                goals: [0.50D, 0.50D, 0.50D],
                wins: [1, 1, 1]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            using var evaluation = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1LocalTeacherCeilingCli.EvaluationLeaf)));
            Assert.True(evaluation.RootElement
                .GetProperty("local_teacher_ceiling_evaluated").GetBoolean());
            Assert.False(evaluation.RootElement
                .GetProperty("low_beat_authority_predicate").GetBoolean());
            Assert.False(evaluation.RootElement
                .GetProperty("local_teacher_ceiling_candidate").GetBoolean());
            Assert.False(evaluation.RootElement.GetProperty("passed").GetBoolean());
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task TwoRunPacket_FailsWithStableWindowIncompleteError()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D], [0, 0]);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_CEILING_WINDOW_INCOMPLETE", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ForgedRequestHash_FailsEvidenceMismatchBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var manifest = ReadReviewRun(fixture);
            WriteJson(
                fixture.ReviewRunPath,
                manifest with { ReviewRequestSha256 = EvidenceRef });

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ForgedAggregateWithReboundHash_FailsArithmeticRevalidation()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            RewriteRequest(
                fixture,
                request => request with
                {
                    Aggregate = request.Aggregate with
                    {
                        PooledStrictTeacherWins = 1,
                    },
                });

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ForgedAcceptedPostureWithReboundHash_FailsEvidenceMismatch()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            RewriteRequest(
                fixture,
                request => request with { IndependentAcceptance = true });

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("bom")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    public async Task NonCanonicalOrMalformedRequest_FailsInputInvalid(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var bytes = File.ReadAllBytes(fixture.ReviewRequestPath);
            if (mutation == "bom")
            {
                File.WriteAllBytes(
                    fixture.ReviewRequestPath,
                    Encoding.UTF8.Preamble.ToArray().Concat(bytes).ToArray());
            }
            else
            {
                var text = Encoding.UTF8.GetString(bytes);
                text = mutation == "unknown"
                    ? text.Replace("{", "{\"unexpected\":true,", StringComparison.Ordinal)
                    : text.Replace(
                        "{",
                        "{\"status\":\"duplicate\",",
                        StringComparison.Ordinal);
                File.WriteAllText(
                    fixture.ReviewRequestPath,
                    text,
                    new UTF8Encoding(false));
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_CEILING_INPUT_INVALID", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExtraLeafOrNestedDirectory_FailsExactPacketIntake()
    {
        foreach (var nested in new[] { false, true })
        {
            var root = CreateTempRoot();
            try
            {
                var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
                if (nested)
                    Directory.CreateDirectory(Path.Combine(fixture.ReviewDirectory, "extra"));
                else
                    File.WriteAllText(
                        Path.Combine(fixture.ReviewDirectory, "extra.json"),
                        "{}",
                        new UTF8Encoding(false));

                var result = await RunAsync(fixture);

                Assert.Equal(1, result.ExitCode);
                Assert.Contains("TP1_CEILING_INPUT_INVALID", result.Stderr, StringComparison.Ordinal);
                Assert.False(Directory.Exists(fixture.Destination));
            }
            finally { DeleteIfExists(root); }
        }
    }

    [Fact]
    public async Task InputOutputContainment_FailsBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);

            var result = await RunAsync(fixture, outputRoot: root);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_CEILING_INPUT_INVALID", result.Stderr, StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PublicationFailure_CleansStagingAndDoesNotPublishDestination()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);

            var result = await RunAsync(fixture, failOnPublish: true);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_CEILING_PUBLICATION_FAILED", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task Cancellation_Returns130AndPublishesNothing()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = await RunAsync(
                fixture,
                cancellationToken: cancellation.Token);

            Assert.Equal(130, result.ExitCode);
            Assert.Contains("TP1_CEILING_CANCELLED", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Tp1CeilingWindowsFileSymlinkFact]
    public async Task CorrectlyNamedExternalReviewLeafSymlink_IsRejected()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var external = Path.Combine(root, "external-review-request.json");
            File.Move(fixture.ReviewRequestPath, external);
            File.CreateSymbolicLink(fixture.ReviewRequestPath, external);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_CEILING_INPUT_INVALID", result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PostWriteSourceIdentityDrift_IsPublicationFailureAndCleansStaging()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var reviewResolveCount = 0;
            OperationResult<string> ResolveWithPostWriteDrift(string path, bool _)
            {
                var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                if (StringComparer.OrdinalIgnoreCase.Equals(
                        full,
                        fixture.ReviewDirectory))
                {
                    reviewResolveCount++;
                    if (reviewResolveCount >= 3)
                        return OperationResult<string>.Ok(Path.Combine(root, "drifted-review"));
                }
                return OperationResult<string>.Ok(full);
            }

            var result = await RunAsync(
                fixture,
                resolvePhysicalPath: ResolveWithPostWriteDrift);

            Assert.Equal(3, reviewResolveCount);
            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_CEILING_PUBLICATION_FAILED",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "TP1_CEILING_EVIDENCE_MISMATCH",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task NegativeBestImprovement_IsRegressionReasonButMayRemainCandidate()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.60D, 0.59D, 0.58D], [0, 0, 0]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            using var evaluation = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1LocalTeacherCeilingCli.EvaluationLeaf)));
            var value = evaluation.RootElement;
            Assert.True(value.GetProperty("local_teacher_ceiling_candidate").GetBoolean());
            Assert.False(value.GetProperty("passed").GetBoolean());
            Assert.True(value.GetProperty("best_improvement").GetDouble() < 0D);
            Assert.Contains(
                "NEGATIVE_BEST_IMPROVEMENT_IS_REGRESSION",
                value.GetProperty("reason_codes")
                    .EnumerateArray()
                    .Select(static reason => reason.GetString()!));
            var markdown = File.ReadAllText(
                Path.Combine(fixture.Destination, Tp1LocalTeacherCeilingCli.MarkdownLeaf),
                Encoding.UTF8);
            Assert.Contains(
                "Отрицательное улучшение означает регрессию",
                markdown,
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("noncontiguous_ordinal")]
    [InlineData("duplicate_ordinal")]
    [InlineData("duplicate_run_id")]
    [InlineData("duplicate_source_run_id")]
    [InlineData("duplicate_window_ref")]
    public async Task ForgedRunSequenceOrIdentity_FailsEvidenceMismatch(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            RewriteRequest(fixture, request =>
            {
                var runs = request.Runs.ToArray();
                runs[1] = mutation switch
                {
                    "noncontiguous_ordinal" => runs[1] with { Ordinal = 3 },
                    "duplicate_ordinal" => runs[1] with { Ordinal = 0 },
                    "duplicate_run_id" => runs[1] with { RunId = runs[0].RunId },
                    "duplicate_source_run_id" => runs[1] with
                    {
                        SourceRunId = runs[0].SourceRunId,
                    },
                    "duplicate_window_ref" => runs[1] with
                    {
                        WindowRef = runs[0].WindowRef,
                    },
                    _ => throw new InvalidOperationException(),
                };
                return request with { Runs = runs };
            });

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("source_array_mismatch")]
    [InlineData("duplicate_source_tuple")]
    public async Task ForgedSourceArtifactBinding_FailsEvidenceMismatch(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            if (mutation == "source_array_mismatch")
            {
                var reviewRun = ReadReviewRun(fixture);
                var sources = reviewRun.SourceArtifacts.ToArray();
                sources[1] = sources[0] with { Ordinal = 1 };
                WriteJson(
                    fixture.ReviewRunPath,
                    reviewRun with { SourceArtifacts = sources });
            }
            else
            {
                var request = ReadRequest(fixture);
                var runs = request.Runs.ToArray();
                runs[1] = runs[1] with
                {
                    SourceArtifactHashes = runs[0].SourceArtifactHashes with
                    {
                        Ordinal = 1,
                    },
                };
                var mutatedRequest = request with { Runs = runs };
                WriteJson(fixture.ReviewRequestPath, mutatedRequest);
                var reviewRun = ReadReviewRun(fixture);
                WriteJson(
                    fixture.ReviewRunPath,
                    reviewRun with
                    {
                        ReviewRequestSha256 = Hash(
                            File.ReadAllBytes(fixture.ReviewRequestPath)),
                        SourceArtifacts = runs
                            .Select(static run => run.SourceArtifactHashes)
                            .ToArray(),
                    });
            }

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("status_improved")]
    [InlineData("status_regressed")]
    [InlineData("status_indistinguishable")]
    [InlineData("status_invalid")]
    [InlineData("status_unknown")]
    [InlineData("floor")]
    [InlineData("floor_and_status")]
    public async Task ForgedMeasurementNoisePassport_FailsEvidenceMismatch(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            RewriteRequest(fixture, request =>
            {
                var runs = request.Runs.ToArray();
                runs[0] = mutation switch
                {
                    "status_improved" => runs[0] with
                    {
                        MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus,
                    },
                    "status_regressed" => runs[0] with
                    {
                        MeasurementNoiseStatus = MeasurementNoisePolicy.RegressedStatus,
                    },
                    "status_indistinguishable" => runs[0] with
                    {
                        MeasurementNoiseStatus = MeasurementNoisePolicy.IndistinguishableStatus,
                    },
                    "status_invalid" => runs[0] with
                    {
                        MeasurementNoiseStatus = MeasurementNoisePolicy.InvalidPassportStatus,
                    },
                    "status_unknown" => runs[0] with
                    {
                        MeasurementNoiseStatus = "unknown_measurement_noise_status",
                    },
                    "floor" => runs[0] with { MeasurementNoiseFloor = 0.01D },
                    _ => runs[0] with
                    {
                        MeasurementNoiseFloor = 0.01D,
                        MeasurementNoiseStatus = MeasurementNoisePolicy.ImprovedStatus,
                    },
                };
                return request with { Runs = runs };
            });

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ReviewRunSummaryUnknownProperty_StillFailsInputInvalid()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var text = File.ReadAllText(fixture.ReviewRequestPath, Encoding.UTF8)
                .Replace(
                    "\"measurement_noise_status\":",
                    "\"unexpected_noise\":true,\"measurement_noise_status\":",
                    StringComparison.Ordinal);
            File.WriteAllText(
                fixture.ReviewRequestPath,
                text,
                new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_CEILING_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("nested_unknown", "TP1_CEILING_INPUT_INVALID")]
    [InlineData("nested_duplicate", "TP1_CEILING_INPUT_INVALID")]
    [InlineData("noncanonical_bytes", "TP1_CEILING_EVIDENCE_MISMATCH")]
    public async Task ReviewRunNestedSchemaAndCanonicalBytes_AreStrict(
        string mutation,
        string expectedError)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.5D, 0.51D, 0.515D], [0, 0, 0]);
            var text = File.ReadAllText(fixture.ReviewRunPath, Encoding.UTF8);
            text = mutation switch
            {
                "nested_unknown" => text.Replace(
                    "\"source_artifacts\":[{",
                    "\"source_artifacts\":[{\"unexpected\":true,",
                    StringComparison.Ordinal),
                "nested_duplicate" => text.Replace(
                    "\"source_artifacts\":[{",
                    "\"source_artifacts\":[{\"ordinal\":0,",
                    StringComparison.Ordinal),
                "noncanonical_bytes" => text + "\n",
                _ => throw new InvalidOperationException(),
            };
            File.WriteAllText(
                fixture.ReviewRunPath,
                text,
                new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(expectedError, result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    private static void AssertAllNegativeGates(JsonElement value)
    {
        Assert.False(value.GetProperty("source_ordering_verified").GetBoolean());
        Assert.False(value.GetProperty("independent_acceptance").GetBoolean());
        Assert.False(value.GetProperty("passed").GetBoolean());
        Assert.False(value.GetProperty("tp1_exit_gate_evaluated").GetBoolean());
        Assert.False(value.GetProperty("tp2_authorized").GetBoolean());
        Assert.True(value.GetProperty("eval_only").GetBoolean());
        Assert.False(value.GetProperty("dpo_train_ready").GetBoolean());
        Assert.False(value.GetProperty("teacher_output_used_for_training").GetBoolean());
        Assert.False(value.GetProperty("training_started").GetBoolean());
        Assert.Equal(0, value.GetProperty("dpo_steps_executed").GetInt32());
        Assert.False(value.GetProperty("checkpoint_promoted").GetBoolean());
        Assert.True(value.GetProperty("human_decision_required").GetBoolean());
    }

    private static void AssertEvidenceMismatch(CliResult result, Fixture fixture)
    {
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("TP1_CEILING_EVIDENCE_MISMATCH", result.Stderr, StringComparison.Ordinal);
        Assert.False(Directory.Exists(fixture.Destination));
        AssertNoStaging(fixture.OutputRoot);
    }

    private static Fixture CreateFixture(
        string root,
        IReadOnlyList<double> goals,
        IReadOnlyList<int> wins)
    {
        Assert.Equal(goals.Count, wins.Count);
        var reviewDirectory = Path.Combine(root, ReviewPacketId);
        var outputRoot = Path.Combine(root, "output");
        Directory.CreateDirectory(reviewDirectory);
        var invariants = new Tp1ReviewInvariantSet(
            "TP1_LOCAL",
            "evaluation-set",
            "translation",
            "held-out-scorer",
            "v1",
            "threshold-v1",
            InferredK: 1,
            CaseCount: 10,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            ModelId,
            "ollama_manifest_sha256",
            ManifestDigest,
            ModelLayerDigest,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef,
            EvidenceRef);
        var runs = goals.Select((goal, ordinal) =>
        {
            var hashes = new Tp1ReviewSourceArtifactHashes(
                ordinal,
                RefFor(ordinal, 1),
                RefFor(ordinal, 2),
                RefFor(ordinal, 3),
                RefFor(ordinal, 4));
            return new Tp1ReviewRunSummary(
                ordinal,
                $"scoring-run-{ordinal}",
                $"source-run-{ordinal}",
                RefFor(ordinal, 5),
                CaseCount: 10,
                SemanticCandidateCount: 10,
                goal,
                HeldOutAccuracyYbest: 0.4D,
                TeacherGapToGold: 1D - goal,
                TeacherBeatRate: (double)wins[ordinal] / 10D,
                StrictTeacherWins: wins[ordinal],
                EvidenceRef,
                hashes);
        }).ToArray();
        var deltas = runs.Zip(runs.Skip(1), static (from, to) =>
            new Tp1ReviewAdjacentDelta(
                from.Ordinal,
                to.Ordinal,
                to.HeldOutAccuracyYgoal - from.HeldOutAccuracyYgoal,
                to.HeldOutAccuracyYbest - from.HeldOutAccuracyYbest,
                to.TeacherGapToGold - from.TeacherGapToGold,
                to.TeacherBeatRate - from.TeacherBeatRate)).ToArray();
        var totalCases = runs.Sum(static run => run.CaseCount);
        var totalWins = runs.Sum(static run => run.StrictTeacherWins);
        var meanGoal = runs.Average(static run => run.HeldOutAccuracyYgoal);
        var request = new Tp1EvaluationReviewRequestArtifact(
            "REVIEW_REQUEST_CREATED_NOT_ACCEPTED",
            ReviewPacketId,
            OrderingRef,
            OrderingVerified: false,
            invariants,
            runs,
            deltas,
            new Tp1ReviewAggregate(
                runs.Length,
                totalCases,
                runs.Sum(static run => run.SemanticCandidateCount),
                totalWins,
                meanGoal,
                runs.Average(static run => run.HeldOutAccuracyYbest),
                1D - meanGoal,
                (double)totalWins / totalCases),
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
        var requestPath = Path.Combine(
            reviewDirectory,
            Tp1EvaluationReviewCli.ReviewRequestLeaf);
        WriteJson(requestPath, request);
        var markdownPath = Path.Combine(
            reviewDirectory,
            Tp1EvaluationReviewCli.ReviewMarkdownLeaf);
        File.WriteAllText(
            markdownPath,
            "# Исходный review-пакет\n\nПриёмка не выполнена.\n",
            new UTF8Encoding(false));
        var reviewRun = new Tp1EvaluationReviewRunArtifact(
            "REVIEW_REQUEST_CREATED_NOT_ACCEPTED",
            ReviewPacketId,
            OrderingRef,
            OrderingVerified: false,
            EvidenceRef,
            Hash(File.ReadAllBytes(requestPath)),
            Hash(File.ReadAllBytes(markdownPath)),
            Tp1EvaluationReviewCli.ArtifactLeafNames,
            runs.Select(static run => run.SourceArtifactHashes).ToArray(),
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
        var reviewRunPath = Path.Combine(
            reviewDirectory,
            Tp1EvaluationReviewCli.ReviewRunLeaf);
        WriteJson(reviewRunPath, reviewRun);
        return new Fixture(
            root,
            reviewDirectory,
            requestPath,
            markdownPath,
            reviewRunPath,
            outputRoot,
            Path.Combine(outputRoot, EvaluationId));
    }

    private static void RewriteRequest(
        Fixture fixture,
        Func<Tp1EvaluationReviewRequestArtifact,
            Tp1EvaluationReviewRequestArtifact> mutation)
    {
        var mutated = mutation(ReadRequest(fixture));
        WriteJson(fixture.ReviewRequestPath, mutated);
        var reviewRun = ReadReviewRun(fixture);
        WriteJson(
            fixture.ReviewRunPath,
            reviewRun with
            {
                ReviewRequestSha256 = Hash(
                    File.ReadAllBytes(fixture.ReviewRequestPath)),
            });
    }

    private static Tp1EvaluationReviewRequestArtifact ReadRequest(Fixture fixture)
        => JsonSerializer.Deserialize<Tp1EvaluationReviewRequestArtifact>(
            File.ReadAllBytes(fixture.ReviewRequestPath),
            SnakeJson)!;

    private static Tp1EvaluationReviewRunArtifact ReadReviewRun(Fixture fixture)
        => JsonSerializer.Deserialize<Tp1EvaluationReviewRunArtifact>(
            File.ReadAllBytes(fixture.ReviewRunPath),
            SnakeJson)!;

    private static async Task<CliResult> RunAsync(
        Fixture fixture,
        bool failOnPublish = false,
        string? outputRoot = null,
        CancellationToken cancellationToken = default,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null)
    {
        outputRoot ??= fixture.OutputRoot;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var dependencies = new Tp1LocalTeacherCeilingDependencies(
            createPublication: (root, id, leaves) =>
                OperationResult<IEvaluationPublicationBoundary>.Ok(
                    new FakePublicationBoundary(
                        root,
                        id,
                        leaves,
                        failOnPublish)),
            rejectOutputUnderProjects: false,
            resolvePhysicalPath: resolvePhysicalPath ?? ResolvePhysicalPath);
        var exit = await Tp1LocalTeacherCeilingCli.RunAsync(
            [
                Tp1LocalTeacherCeilingCli.ArmFlag,
                "--review-packet-directory", fixture.ReviewDirectory,
                "--output-root", outputRoot,
                "--evaluation-id", EvaluationId,
            ],
            stdout,
            stderr,
            cancellationToken,
            dependencies);
        return new CliResult(exit, stdout.ToString(), stderr.ToString());
    }

    private static OperationResult<string> ResolvePhysicalPath(
        string path,
        bool allowMissing)
    {
        _ = allowMissing;
        return OperationResult<string>.Ok(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)));
    }

    private static string RefFor(int ordinal, int component)
    {
        var value = checked(ordinal * 16 + component + 1);
        return "sha256:" + value.ToString("x64");
    }

    private static string Hash(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(value, SnakeJson));

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-tp1-ceiling-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void AssertNoStaging(string outputRoot)
    {
        if (Directory.Exists(outputRoot))
        {
            Assert.Empty(Directory.EnumerateDirectories(
                outputRoot,
                ".*.tp1-ceiling-tmp-*"));
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record Fixture(
        string Root,
        string ReviewDirectory,
        string ReviewRequestPath,
        string ReviewMarkdownPath,
        string ReviewRunPath,
        string OutputRoot,
        string Destination);

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed class FakePublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly HashSet<string> _allowed;
        private readonly HashSet<string> _created = new(StringComparer.Ordinal);
        private readonly string _expectedOutputRoot;
        private readonly bool _failOnPublish;
        private bool _published;

        internal FakePublicationBoundary(
            string outputRoot,
            string evaluationId,
            IReadOnlyCollection<string> leaves,
            bool failOnPublish)
        {
            _allowed = new HashSet<string>(leaves, StringComparer.Ordinal);
            _expectedOutputRoot = Path.GetFullPath(outputRoot);
            _failOnPublish = failOnPublish;
            Directory.CreateDirectory(outputRoot);
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{evaluationId}.tp1-ceiling-tmp-{Guid.NewGuid():N}");
            FinalDirectory = Path.Combine(outputRoot, evaluationId);
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
                throw new IOException("Unexpected output-root identity.");
        }

        public void VerifyStagingIdentity()
        {
            if (_published || !Directory.Exists(StagingDirectory))
                throw new IOException("Staging identity changed.");
        }

        public FileStream CreateArtifactFile(string leafName)
        {
            if (!_allowed.Contains(leafName) || !_created.Add(leafName))
                throw new IOException("Unexpected ceiling artifact.");
            return new FileStream(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
        }

        public void PublishAndVerify()
        {
            if (_failOnPublish)
                throw new IOException("Injected publication failure.");
            if (!_created.SetEquals(_allowed))
                throw new IOException("Ceiling artifact set incomplete.");
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

file sealed class Tp1CeilingWindowsFileSymlinkFactAttribute : FactAttribute
{
    public Tp1CeilingWindowsFileSymlinkFactAttribute()
        => Skip = Tp1CeilingFileSymlinkSupport.GetCapabilitySkipReason();
}

file static class Tp1CeilingFileSymlinkSupport
{
    internal static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: Windows no-follow handle proof is required.";
        var root = Path.Combine(
            Path.GetTempPath(),
            $"tp1-ceiling-symlink-capability-{Guid.NewGuid():N}");
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
                    : "REPARSE_TEST_PLATFORM_LIMITATION: link was not a reparse point.";
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException or NotSupportedException)
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
}
