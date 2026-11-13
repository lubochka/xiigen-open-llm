// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearningTp1EvaluationRunner;

namespace LlmFromScratch.Tests;

public sealed class ContinuousLearningTp1ExitDecisionRequestRunnerTests
{
    private const string EvaluationId = "tp1-ceiling-evaluation-001";
    private const string DecisionRequestId = "tp1-exit-decision-request-001";
    private const string ReviewPacketId = "tp1-review-packet-001";
    private const string ModelId = "qwen3.5:9b";
    private const string ManifestDigest =
        "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7";
    private const string ModelLayerDigest =
        "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c";
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string OtherEvidenceRef =
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
    public async Task CandidateTrue_PublishesExactSafePacketAndFullHashDag()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(
                root,
                goals: [0.60D, 0.61D, 0.615D],
                wins: [0, 0, 0]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr);
            Assert.Equal(
                Tp1ExitDecisionRequestCli.ArtifactLeafNames.Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(fixture.Destination)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));

            var requestPath = Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.DecisionRequestLeaf);
            var markdownPath = Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.MarkdownLeaf);
            var schemaPath = Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.SubmissionSchemaLeaf);
            var runPath = Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.DecisionRequestRunLeaf);
            using var request = JsonDocument.Parse(File.ReadAllBytes(requestPath));
            using var schema = JsonDocument.Parse(File.ReadAllBytes(schemaPath));
            using var run = JsonDocument.Parse(File.ReadAllBytes(runPath));
            var requestRoot = request.RootElement;
            var runRoot = run.RootElement;

            Assert.Equal(
                Tp1ExitDecisionRequestCli.FixedStatus,
                requestRoot.GetProperty("status").GetString());
            Assert.True(requestRoot.GetProperty(
                "source_local_teacher_ceiling_evaluated").GetBoolean());
            Assert.True(requestRoot.GetProperty(
                "source_local_teacher_ceiling_candidate").GetBoolean());
            Assert.False(requestRoot.GetProperty("human_decision_present").GetBoolean());
            Assert.False(requestRoot.GetProperty(
                "source_run_ordering_verified").GetBoolean());
            Assert.False(requestRoot.GetProperty(
                "source_authenticity_verified").GetBoolean());
            Assert.False(requestRoot.GetProperty(
                "numeric_evidence_verified").GetBoolean());
            Assert.False(requestRoot.GetProperty("independent_acceptance").GetBoolean());
            Assert.False(requestRoot.GetProperty(
                "local_teacher_ceiling_accepted").GetBoolean());
            Assert.False(requestRoot.GetProperty("passed").GetBoolean());
            Assert.False(requestRoot.GetProperty("tp1_exit_gate_evaluated").GetBoolean());
            Assert.False(requestRoot.GetProperty("tp2_authorized").GetBoolean());
            Assert.True(requestRoot.GetProperty("eval_only").GetBoolean());
            Assert.False(requestRoot.GetProperty("dpo_train_ready").GetBoolean());
            Assert.False(requestRoot.GetProperty(
                "teacher_output_used_for_training").GetBoolean());
            Assert.False(requestRoot.GetProperty("training_started").GetBoolean());
            Assert.Equal(0, requestRoot.GetProperty("dpo_steps_executed").GetInt32());
            Assert.False(requestRoot.GetProperty("checkpoint_promoted").GetBoolean());
            Assert.False(requestRoot.GetProperty("promotion_started").GetBoolean());
            Assert.False(requestRoot.GetProperty("activation_started").GetBoolean());
            Assert.True(requestRoot.GetProperty("human_decision_required").GetBoolean());
            Assert.Equal(
                ["TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT"],
                requestRoot.GetProperty("luba_tp1_decision_prerequisite")
                    .GetProperty("allowed_decisions")
                    .EnumerateArray().Select(static item => item.GetString()));
            Assert.Equal(
                4,
                requestRoot.GetProperty("independent_reviewer_prerequisite")
                    .GetProperty("required_receipt_fields").GetArrayLength());
            Assert.False(requestRoot.GetProperty("independent_reviewer_prerequisite")
                .GetProperty("authority_evidence_present").GetBoolean());
            Assert.False(requestRoot.GetProperty("luba_tp1_decision_prerequisite")
                .GetProperty("decision_evidence_present").GetBoolean());
            Assert.True(requestRoot.GetProperty("future_tp2_start_requirements")
                .GetProperty("separate_request_required").GetBoolean());

            var sourceHashes = requestRoot.GetProperty("source_packet_hashes");
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.EvaluationPath)),
                sourceHashes.GetProperty("ceiling_evaluation_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.MarkdownPath)),
                sourceHashes.GetProperty("ceiling_markdown_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.EvaluationRunPath)),
                sourceHashes.GetProperty("ceiling_run_sha256").GetString());

            Assert.Equal(Hash(File.ReadAllBytes(requestPath)),
                runRoot.GetProperty("decision_request_sha256").GetString());
            Assert.Equal(Hash(File.ReadAllBytes(markdownPath)),
                runRoot.GetProperty("markdown_sha256").GetString());
            Assert.Equal(Hash(File.ReadAllBytes(schemaPath)),
                runRoot.GetProperty(
                    "human_decision_submission_schema_sha256").GetString());
            Assert.Equal(Hash(File.ReadAllBytes(fixture.EvaluationPath)),
                runRoot.GetProperty("ceiling_evaluation_sha256").GetString());
            Assert.Equal(Hash(File.ReadAllBytes(fixture.MarkdownPath)),
                runRoot.GetProperty("ceiling_markdown_sha256").GetString());
            Assert.Equal(Hash(File.ReadAllBytes(fixture.EvaluationRunPath)),
                runRoot.GetProperty("ceiling_run_sha256").GetString());
            Assert.Equal(
                Tp1ExitDecisionRequestCli.ArtifactLeafNames,
                runRoot.GetProperty("published_artifact_leaf_names")
                    .EnumerateArray().Select(static item => item.GetString()));
            Assert.Equal(
                Hash(JsonSerializer.SerializeToUtf8Bytes(
                    Tp1ExitDecisionRequestCli.ArtifactLeafNames,
                    SnakeJson)),
                runRoot.GetProperty("published_artifact_roster_sha256").GetString());
            Assert.False(runRoot.TryGetProperty(
                "decision_request_run_sha256",
                out _));
            Assert.False(runRoot.GetProperty("human_decision_present").GetBoolean());
            Assert.False(runRoot.GetProperty(
                "source_run_ordering_verified").GetBoolean());
            Assert.False(runRoot.GetProperty(
                "source_authenticity_verified").GetBoolean());
            Assert.False(runRoot.GetProperty(
                "numeric_evidence_verified").GetBoolean());
            Assert.False(runRoot.GetProperty("independent_acceptance").GetBoolean());
            Assert.False(runRoot.GetProperty(
                "local_teacher_ceiling_accepted").GetBoolean());
            Assert.False(runRoot.GetProperty("passed").GetBoolean());
            Assert.False(runRoot.GetProperty("tp1_exit_gate_evaluated").GetBoolean());
            Assert.False(runRoot.GetProperty("tp2_authorized").GetBoolean());
            Assert.True(runRoot.GetProperty("eval_only").GetBoolean());
            Assert.False(runRoot.GetProperty("dpo_train_ready").GetBoolean());
            Assert.False(runRoot.GetProperty(
                "teacher_output_used_for_training").GetBoolean());
            Assert.False(runRoot.GetProperty("training_started").GetBoolean());
            Assert.Equal(0, runRoot.GetProperty("dpo_steps_executed").GetInt32());
            Assert.False(runRoot.GetProperty("checkpoint_promoted").GetBoolean());
            Assert.False(runRoot.GetProperty("promotion_started").GetBoolean());
            Assert.False(runRoot.GetProperty("activation_started").GetBoolean());
            Assert.True(runRoot.GetProperty("human_decision_required").GetBoolean());

            var schemaRoot = schema.RootElement;
            Assert.True(schemaRoot.GetProperty("x_template_only").GetBoolean());
            Assert.True(schemaRoot.GetProperty("x_not_authority").GetBoolean());
            Assert.True(schemaRoot.GetProperty(
                "x_never_parse_as_receipt").GetBoolean());
            Assert.False(schemaRoot.GetProperty("additionalProperties").GetBoolean());
            var properties = schemaRoot.GetProperty("properties");
            Assert.Equal(
                DecisionRequestId,
                properties.GetProperty("decision_request_id")
                    .GetProperty("const").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.EvaluationPath)),
                properties.GetProperty("source_ceiling_evaluation_sha256")
                    .GetProperty("const").GetString());
            Assert.False(properties.TryGetProperty("tp2_decision", out _));
            Assert.False(properties.TryGetProperty("tp2_authorization", out _));
            Assert.DoesNotContain(
                properties.EnumerateObject(),
                static property => property.Name.Contains(
                    "tp2",
                    StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                "ProviderTermsProof",
                File.ReadAllText(markdownPath),
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("plateau-equality")]
    [InlineData("beat-equality")]
    public async Task EqualityBoundaries_AreStrictAndCandidateFalseAllowsOnlyReject(
        string boundary)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = boundary == "plateau-equality"
                ? CreateFixture(root, [0D, 0.02D, 0.01D], [0, 0, 0])
                : CreateFixture(root, [0.50D, 0.51D, 0.515D], [1, 1, 1]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            using var request = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.DecisionRequestLeaf)));
            Assert.False(request.RootElement.GetProperty(
                "source_local_teacher_ceiling_candidate").GetBoolean());
            Assert.Equal(
                ["TP1_EXIT_REJECT"],
                request.RootElement.GetProperty("luba_tp1_decision_prerequisite")
                    .GetProperty("allowed_decisions")
                    .EnumerateArray().Select(static item => item.GetString()));
            using var schema = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.SubmissionSchemaLeaf)));
            var tuples = ReadSchemaTuples(schema.RootElement);
            Assert.NotEmpty(tuples);
            Assert.All(tuples, static tuple =>
            {
                Assert.False(tuple.LocalTeacherCeilingAccepted);
                Assert.False(tuple.IndependentAcceptance);
                Assert.Equal("TP1_EXIT_REJECT", tuple.Decision);
            });
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SubmissionSchema_ExactlyRepresentsPureAllowedCombinations(
        bool sourceCandidate)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = sourceCandidate
                ? CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0])
                : CreateFixture(root, [0D, 0.02D, 0.01D], [0, 0, 0]);
            var result = await RunAsync(fixture);
            Assert.Equal(0, result.ExitCode);
            using var schema = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.SubmissionSchemaLeaf)));
            Assert.Equal(
                sourceCandidate,
                schema.RootElement.GetProperty("properties")
                    .GetProperty("source_local_teacher_ceiling_candidate")
                    .GetProperty("const").GetBoolean());
            var represented = ReadSchemaTuples(schema.RootElement);

            for (var mask = 0; mask < 1 << 4; mask++)
            foreach (var independentAcceptance in new[] { false, true })
            foreach (var decision in new[]
            {
                "TP1_EXIT_ACCEPT",
                "TP1_EXIT_REJECT",
                "TP2_START",
            })
            {
                var tuple = new SubmissionTuple(
                    SourceRunOrderingVerified: (mask & 1) != 0,
                    SourceAuthenticityVerified: (mask & 2) != 0,
                    NumericEvidenceVerified: (mask & 4) != 0,
                    LocalTeacherCeilingAccepted: (mask & 8) != 0,
                    IndependentAcceptance: independentAcceptance,
                    Decision: decision);
                Assert.Equal(
                    Tp1ExitDecisionRequestCli.IsSubmissionCombinationAllowed(
                        sourceCandidate,
                        tuple.SourceRunOrderingVerified,
                        tuple.SourceAuthenticityVerified,
                        tuple.NumericEvidenceVerified,
                        tuple.LocalTeacherCeilingAccepted,
                        tuple.IndependentAcceptance,
                        tuple.Decision),
                    represented.Contains(tuple));
            }
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task NegativeBestImprovement_IsRegressionReasonWithoutSelfAcceptance()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.60D, 0.59D, 0.58D], [0, 0, 0]);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            using var request = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionRequestCli.DecisionRequestLeaf)));
            Assert.True(request.RootElement.GetProperty("best_improvement").GetDouble() < 0D);
            Assert.Contains(
                "NEGATIVE_BEST_IMPROVEMENT_IS_REGRESSION",
                request.RootElement.GetProperty("source_reason_codes")
                    .EnumerateArray().Select(static item => item.GetString()));
            Assert.True(request.RootElement.GetProperty(
                "source_local_teacher_ceiling_candidate").GetBoolean());
            Assert.False(request.RootElement.GetProperty("passed").GetBoolean());
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("status")]
    [InlineData("policy")]
    [InlineData("arithmetic")]
    [InlineData("evaluation-hash")]
    [InlineData("roster")]
    [InlineData("selected-runs")]
    [InlineData("reasons")]
    [InlineData("run-candidate")]
    public async Task TamperedCeilingEvidence_FailsClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            if (mutation == "evaluation-hash")
            {
                RewriteRun(fixture, run => run with { EvaluationSha256 = OtherEvidenceRef });
            }
            else if (mutation == "roster")
            {
                RewriteRun(fixture, run => run with
                {
                    PublishedArtifactLeafNames = run.PublishedArtifactLeafNames.Reverse().ToArray(),
                });
            }
            else if (mutation == "run-candidate")
            {
                RewriteRun(fixture, run => run with
                {
                    LocalTeacherCeilingCandidate = !run.LocalTeacherCeilingCandidate,
                });
            }
            else
            {
                RewriteEvaluation(fixture, evaluation => mutation switch
                {
                    "status" => evaluation with { Status = "CEILING_ACCEPTED" },
                    "policy" => evaluation with
                    {
                        Policy = evaluation.Policy with { WindowSize = 4 },
                    },
                    "arithmetic" => evaluation with
                    {
                        SelectedCaseCount = evaluation.SelectedCaseCount + 1,
                    },
                    "selected-runs" => evaluation with
                    {
                        SelectedRuns = evaluation.SelectedRuns.Select((run, index) =>
                            index == 1 ? run with { Ordinal = run.Ordinal + 1 } : run)
                            .ToArray(),
                    },
                    "reasons" => evaluation with
                    {
                        ReasonCodes = ["FORGED_REASON"],
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
                });
            }

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("extra", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("duplicate", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("missing", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("noncanonical", "TP1_EXIT_DECISION_REQUEST_EVIDENCE_MISMATCH")]
    public async Task SourceJsonSchemaAndCanonicalBytes_AreStrict(
        string mutation,
        string expectedCode)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var bytes = File.ReadAllBytes(fixture.EvaluationPath);
            if (mutation == "noncanonical")
            {
                File.WriteAllBytes(fixture.EvaluationPath, bytes.Append((byte)'\n').ToArray());
            }
            else if (mutation == "missing")
            {
                var node = JsonNode.Parse(bytes)!.AsObject();
                Assert.True(node.Remove("status"));
                File.WriteAllBytes(
                    fixture.EvaluationPath,
                    JsonSerializer.SerializeToUtf8Bytes(node, SnakeJson));
            }
            else
            {
                var text = Encoding.UTF8.GetString(bytes);
                var insertion = mutation == "extra"
                    ? "{\"unexpected\":true,"
                    : $"{{\"status\":\"{Tp1LocalTeacherCeilingCli.FixedStatus}\",";
                File.WriteAllText(
                    fixture.EvaluationPath,
                    insertion + text[1..],
                    new UTF8Encoding(false));
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(expectedCode, result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("policy", "unknown")]
    [InlineData("policy", "missing")]
    [InlineData("policy", "duplicate")]
    [InlineData("source_packet_hashes", "unknown")]
    [InlineData("source_packet_hashes", "missing")]
    [InlineData("source_packet_hashes", "duplicate")]
    [InlineData("invariant_identity", "unknown")]
    [InlineData("invariant_identity", "missing")]
    [InlineData("invariant_identity", "duplicate")]
    [InlineData("selected_runs", "unknown")]
    [InlineData("selected_runs", "missing")]
    [InlineData("selected_runs", "duplicate")]
    public async Task EvaluationNestedObjects_RejectUnknownMissingAndDuplicateFields(
        string target,
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            MutateNestedEvaluationJson(fixture.EvaluationPath, target, mutation);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("unknown", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("missing", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("duplicate", "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID")]
    [InlineData("noncanonical", "TP1_EXIT_DECISION_REQUEST_EVIDENCE_MISMATCH")]
    public async Task EvaluationRunSchemaAndCanonicalBytes_AreStrict(
        string mutation,
        string expectedCode)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var bytes = File.ReadAllBytes(fixture.EvaluationRunPath);
            if (mutation == "noncanonical")
            {
                File.WriteAllBytes(
                    fixture.EvaluationRunPath,
                    bytes.Append((byte)'\n').ToArray());
            }
            else if (mutation == "duplicate")
            {
                var text = Encoding.UTF8.GetString(bytes);
                File.WriteAllText(
                    fixture.EvaluationRunPath,
                    $"{{\"status\":\"{Tp1LocalTeacherCeilingCli.FixedStatus}\"," + text[1..],
                    new UTF8Encoding(false));
            }
            else
            {
                var node = JsonNode.Parse(bytes)!.AsObject();
                if (mutation == "unknown")
                    node["unexpected"] = true;
                else
                    Assert.True(node.Remove("status"));
                WriteNode(fixture.EvaluationRunPath, node);
            }

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(expectedCode, result.Stderr, StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("evaluation-passed")]
    [InlineData("evaluation-human-not-required")]
    [InlineData("run-passed")]
    [InlineData("run-training")]
    [InlineData("run-steps")]
    public async Task SourceEvaluationAndRunPostureMutations_FailClosed(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            if (mutation.StartsWith("evaluation-", StringComparison.Ordinal))
            {
                RewriteEvaluation(fixture, evaluation => mutation switch
                {
                    "evaluation-passed" => evaluation with { Passed = true },
                    "evaluation-human-not-required" => evaluation with
                    {
                        HumanDecisionRequired = false,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
                });
            }
            else
            {
                RewriteRun(fixture, run => mutation switch
                {
                    "run-passed" => run with { Passed = true },
                    "run-training" => run with { TrainingStarted = true },
                    "run-steps" => run with { DpoStepsExecuted = 1 },
                    _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
                });
            }

            var result = await RunAsync(fixture);

            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("extra-file")]
    [InlineData("extra-directory")]
    [InlineData("missing-file")]
    public async Task SourcePacketRosterMustBeExact(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            if (mutation == "extra-file")
                File.WriteAllText(Path.Combine(fixture.CeilingDirectory, "extra.json"), "{}", new UTF8Encoding(false));
            else if (mutation == "extra-directory")
                Directory.CreateDirectory(Path.Combine(fixture.CeilingDirectory, "extra"));
            else
                File.Delete(fixture.MarkdownPath);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_DECISION_REQUEST_INPUT_INVALID",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task LogicalPathOverlap_IsRejected()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);

            var result = await RunAsync(fixture, outputRoot: fixture.Root);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", result.Stderr);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PhysicalAlias_IsRejected()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            OperationResult<string> AliasResolver(string _, bool __)
                => OperationResult<string>.Ok(Path.Combine(root, "same-physical"));

            var result = await RunAsync(fixture, resolvePhysicalPath: AliasResolver);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", result.Stderr);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task OutputUnderProjects_IsRejectedBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var forbidden = Path.Combine(@"C:\Projects", $"tp1-exit-{Guid.NewGuid():N}");

            var result = await RunAsync(
                fixture,
                outputRoot: forbidden,
                rejectOutputUnderProjects: true);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", result.Stderr);
            Assert.False(Directory.Exists(forbidden));
        }
        finally { DeleteIfExists(root); }
    }

    [Tp1ExitDecisionWindowsFileSymlinkFact]
    public async Task SourceLeafSymlink_IsRejected()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var target = Path.Combine(root, "external-markdown.md");
            File.Copy(fixture.MarkdownPath, target);
            File.Delete(fixture.MarkdownPath);
            File.CreateSymbolicLink(fixture.MarkdownPath, target);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PostWriteSourceIdentityDrift_RemainsEvidenceMismatchAndCleansStaging()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var sourceCalls = 0;
            OperationResult<string> ResolveWithDrift(string path, bool _)
            {
                var physical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                if (StringComparer.OrdinalIgnoreCase.Equals(
                    physical,
                    fixture.CeilingDirectory))
                {
                    sourceCalls++;
                    if (sourceCalls == 3)
                        return OperationResult<string>.Ok(physical + "-drift");
                }
                return OperationResult<string>.Ok(physical);
            }

            var result = await RunAsync(
                fixture,
                resolvePhysicalPath: ResolveWithDrift);

            Assert.Equal(3, sourceCalls);
            AssertEvidenceMismatch(result, fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PostCreateOutputPhysicalDrift_IsPublicationFailureAndCleansStaging()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var outputCalls = 0;
            OperationResult<string> ResolveWithOutputDrift(string path, bool _)
            {
                var physical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                if (StringComparer.OrdinalIgnoreCase.Equals(physical, fixture.OutputRoot))
                {
                    outputCalls++;
                    if (outputCalls == 2)
                        return OperationResult<string>.Ok(physical + "-drift");
                }
                return OperationResult<string>.Ok(physical);
            }

            var result = await RunAsync(
                fixture,
                resolvePhysicalPath: ResolveWithOutputDrift);

            Assert.Equal(2, outputCalls);
            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_DECISION_REQUEST_PUBLICATION_FAILED",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(PublicationFailure.Create)]
    [InlineData(PublicationFailure.Write)]
    [InlineData(PublicationFailure.Publish)]
    [InlineData(PublicationFailure.OutputIdentity)]
    [InlineData(PublicationFailure.Cleanup)]
    public async Task PublicationBoundaryFailures_ArePublicationFailedAndNeverPublish(
        PublicationFailure failure)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            OperationResult<IEvaluationPublicationBoundary> Create(
                string outputRoot,
                string id,
                IReadOnlyCollection<string> leaves)
                => failure == PublicationFailure.Create
                    ? OperationResult<IEvaluationPublicationBoundary>.Fail(
                        "Injected create failure.",
                        "CREATE_FAILED")
                    : OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(outputRoot, id, leaves, failure));

            var result = await RunAsync(fixture, createPublication: Create);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_DECISION_REQUEST_PUBLICATION_FAILED",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.False(Directory.Exists(fixture.Destination));
            if (failure != PublicationFailure.Cleanup)
                AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    [Tp1ExitDecisionWindowsDirectorySymlinkFact]
    public async Task SourceDirectoryReparsePoint_IsRejected()
    {
        var root = CreateTempRoot();
        string? link = null;
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var target = Path.Combine(root, "real-ceiling");
            Directory.Move(fixture.CeilingDirectory, target);
            link = fixture.CeilingDirectory;
            Directory.CreateSymbolicLink(link, target);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", result.Stderr);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally
        {
            DeleteDirectoryLink(link);
            DeleteIfExists(root);
        }
    }

    [Tp1ExitDecisionWindowsDirectorySymlinkFact]
    public async Task OutputRootReparsePoint_IsRejected()
    {
        var root = CreateTempRoot();
        string? link = null;
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            var target = Path.Combine(root, "real-output");
            Directory.CreateDirectory(target);
            link = fixture.OutputRoot;
            Directory.CreateSymbolicLink(link, target);

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_INPUT_INVALID", result.Stderr);
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally
        {
            DeleteDirectoryLink(link);
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task DestinationCollision_IsPublicationFailureWithoutOverwrite()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            Directory.CreateDirectory(fixture.Destination);
            var sentinel = Path.Combine(fixture.Destination, "sentinel.txt");
            File.WriteAllText(sentinel, "keep", new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_DECISION_REQUEST_PUBLICATION_FAILED",
                result.Stderr,
                StringComparison.Ordinal);
            Assert.Equal("keep", File.ReadAllText(sentinel));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task CancellationDuringPublication_Returns130AndCleansStaging()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, [0.50D, 0.51D, 0.515D], [0, 0, 0]);
            using var cancellation = new CancellationTokenSource();
            OperationResult<IEvaluationPublicationBoundary> CreateAndCancel(
                string outputRoot,
                string id,
                IReadOnlyCollection<string> leaves)
            {
                var publication = new FakePublicationBoundary(
                    outputRoot,
                    id,
                    leaves,
                    PublicationFailure.None);
                cancellation.Cancel();
                return OperationResult<IEvaluationPublicationBoundary>.Ok(publication);
            }

            var result = await RunAsync(
                fixture,
                cancellationToken: cancellation.Token,
                createPublication: CreateAndCancel);

            Assert.Equal(130, result.ExitCode);
            Assert.Contains("TP1_EXIT_DECISION_REQUEST_CANCELLED", result.Stderr);
            Assert.False(Directory.Exists(fixture.Destination));
            AssertNoStaging(fixture.OutputRoot);
        }
        finally { DeleteIfExists(root); }
    }

    private static Fixture CreateFixture(
        string root,
        IReadOnlyList<double> goals,
        IReadOnlyList<int> wins)
    {
        Assert.Equal(3, goals.Count);
        Assert.Equal(goals.Count, wins.Count);
        var ceilingDirectory = Path.Combine(root, EvaluationId);
        var outputRoot = Path.Combine(root, "output");
        Directory.CreateDirectory(ceilingDirectory);
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
        var selected = goals.Select((goal, index) =>
            new Tp1LocalTeacherCeilingSelectedRun(
                Ordinal: index + 4,
                RunId: $"scoring-run-{index + 4}",
                WindowRef: RefFor(index + 4),
                CaseCount: 10,
                StrictTeacherWins: wins[index],
                HeldOutAccuracyYgoal: goal)).ToArray();
        var cases = selected.Sum(static run => run.CaseCount);
        var strictWins = selected.Sum(static run => run.StrictTeacherWins);
        var first = selected[0].HeldOutAccuracyYgoal;
        var best = Math.Max(
            selected[1].HeldOutAccuracyYgoal,
            selected[2].HeldOutAccuracyYgoal);
        var improvement = best - first;
        var plateau = improvement < 0.02D;
        var lowBeat = checked(strictWins * 10) < cases;
        var candidate = plateau && lowBeat;
        var sourceHashes = new Tp1LocalTeacherCeilingSourcePacketHashes(
            EvidenceRef,
            RefFor(20),
            RefFor(21));
        var evaluation = new Tp1LocalTeacherCeilingEvaluationArtifact(
            Tp1LocalTeacherCeilingCli.FixedStatus,
            EvaluationId,
            new Tp1LocalTeacherCeilingPolicyArtifact(
                Tp1LocalTeacherCeilingCli.FixedPolicyId,
                WindowSize: 3,
                MinimumBestImprovement: 0.02D,
                BeatNumeratorMultiplier: 10,
                BeatDenominatorThreshold: 1,
                PlateauOperator: "strict_less_than",
                LowBeatAuthorityOperator:
                    "strict_less_than_checked_integer_cross_product"),
            ReviewPacketId,
            "REVIEW_REQUEST_CREATED_NOT_ACCEPTED",
            sourceHashes,
            invariants,
            selected,
            cases,
            strictWins,
            first,
            best,
            improvement,
            (double)strictWins / cases,
            plateau,
            lowBeat,
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
            ReasonCodes: BuildReasons(plateau, lowBeat, improvement),
            Limitations: ["Source evaluation remains eval-only."]);
        var evaluationPath = Path.Combine(
            ceilingDirectory,
            Tp1LocalTeacherCeilingCli.EvaluationLeaf);
        WriteJson(evaluationPath, evaluation);
        var markdownPath = Path.Combine(
            ceilingDirectory,
            Tp1LocalTeacherCeilingCli.MarkdownLeaf);
        File.WriteAllText(
            markdownPath,
            "# Source TP1 ceiling evaluation\n\nNot accepted.\n",
            new UTF8Encoding(false));
        var evaluationRun = new Tp1LocalTeacherCeilingEvaluationRunArtifact(
            Tp1LocalTeacherCeilingCli.FixedStatus,
            EvaluationId,
            Tp1LocalTeacherCeilingCli.FixedPolicyId,
            ReviewPacketId,
            Hash(File.ReadAllBytes(evaluationPath)),
            Hash(File.ReadAllBytes(markdownPath)),
            sourceHashes.ReviewRequestSha256,
            sourceHashes.ReviewMarkdownSha256,
            sourceHashes.ReviewRunSha256,
            Tp1LocalTeacherCeilingCli.ArtifactLeafNames,
            Hash(JsonSerializer.SerializeToUtf8Bytes(
                Tp1LocalTeacherCeilingCli.ArtifactLeafNames,
                SnakeJson)),
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
        var evaluationRunPath = Path.Combine(
            ceilingDirectory,
            Tp1LocalTeacherCeilingCli.EvaluationRunLeaf);
        WriteJson(evaluationRunPath, evaluationRun);
        return new Fixture(
            Path.GetFullPath(root),
            Path.GetFullPath(ceilingDirectory),
            evaluationPath,
            markdownPath,
            evaluationRunPath,
            Path.GetFullPath(outputRoot),
            Path.Combine(outputRoot, DecisionRequestId));
    }

    private static IReadOnlyList<string> BuildReasons(
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

    private static HashSet<SubmissionTuple> ReadSchemaTuples(JsonElement schema)
    {
        var tuples = new HashSet<SubmissionTuple>();
        foreach (var alternative in schema.GetProperty("oneOf").EnumerateArray())
        {
            var properties = alternative.GetProperty("properties");
            var tuple = new SubmissionTuple(
                properties.GetProperty("source_run_ordering_verified")
                    .GetProperty("const").GetBoolean(),
                properties.GetProperty("source_authenticity_verified")
                    .GetProperty("const").GetBoolean(),
                properties.GetProperty("numeric_evidence_verified")
                    .GetProperty("const").GetBoolean(),
                properties.GetProperty("local_teacher_ceiling_accepted")
                    .GetProperty("const").GetBoolean(),
                properties.GetProperty("independent_acceptance")
                    .GetProperty("const").GetBoolean(),
                properties.GetProperty("tp1_exit_decision")
                    .GetProperty("const").GetString()!);
            Assert.True(tuples.Add(tuple), "Schema contains a duplicate authority tuple.");
        }
        return tuples;
    }

    private static void MutateNestedEvaluationJson(
        string path,
        string target,
        string mutation)
    {
        var bytes = File.ReadAllBytes(path);
        var root = JsonNode.Parse(bytes)!.AsObject();
        JsonObject targetObject;
        string firstProperty;
        string duplicatePrefix;
        switch (target)
        {
            case "policy":
                targetObject = root["policy"]!.AsObject();
                firstProperty = "policy_id";
                duplicatePrefix = "\"policy\":{\"policy_id\":\"duplicate\",";
                break;
            case "source_packet_hashes":
                targetObject = root["source_packet_hashes"]!.AsObject();
                firstProperty = "review_request_sha256";
                duplicatePrefix = "\"source_packet_hashes\":{\"review_request_sha256\":\""
                    + EvidenceRef + "\",";
                break;
            case "invariant_identity":
                targetObject = root["invariant_identity"]!.AsObject();
                firstProperty = "stage";
                duplicatePrefix = "\"invariant_identity\":{\"stage\":\"TP1_LOCAL\",";
                break;
            case "selected_runs":
                targetObject = root["selected_runs"]!.AsArray()[0]!.AsObject();
                firstProperty = "ordinal";
                duplicatePrefix = "\"selected_runs\":[{\"ordinal\":4,";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target));
        }
        if (mutation == "unknown")
        {
            targetObject["unexpected"] = true;
            WriteNode(path, root);
        }
        else if (mutation == "missing")
        {
            Assert.True(targetObject.Remove(firstProperty));
            WriteNode(path, root);
        }
        else
        {
            var text = Encoding.UTF8.GetString(bytes);
            var marker = target == "selected_runs"
                ? "\"selected_runs\":[{"
                : $"\"{target}\":{{";
            var index = text.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(index >= 0);
            File.WriteAllText(
                path,
                text[..index] + duplicatePrefix + text[(index + marker.Length)..],
                new UTF8Encoding(false));
        }
    }

    private static void WriteNode(string path, JsonNode node)
        => File.WriteAllBytes(
            path,
            JsonSerializer.SerializeToUtf8Bytes(node, SnakeJson));

    private static void RewriteEvaluation(
        Fixture fixture,
        Func<Tp1LocalTeacherCeilingEvaluationArtifact,
            Tp1LocalTeacherCeilingEvaluationArtifact> mutation)
    {
        var evaluation = JsonSerializer.Deserialize<
            Tp1LocalTeacherCeilingEvaluationArtifact>(
                File.ReadAllBytes(fixture.EvaluationPath),
                SnakeJson)!;
        WriteJson(fixture.EvaluationPath, mutation(evaluation));
        RewriteRun(fixture, run => run with
        {
            EvaluationSha256 = Hash(File.ReadAllBytes(fixture.EvaluationPath)),
        });
    }

    private static void RewriteRun(
        Fixture fixture,
        Func<Tp1LocalTeacherCeilingEvaluationRunArtifact,
            Tp1LocalTeacherCeilingEvaluationRunArtifact> mutation)
    {
        var run = JsonSerializer.Deserialize<Tp1LocalTeacherCeilingEvaluationRunArtifact>(
            File.ReadAllBytes(fixture.EvaluationRunPath),
            SnakeJson)!;
        WriteJson(fixture.EvaluationRunPath, mutation(run));
    }

    private static async Task<CliResult> RunAsync(
        Fixture fixture,
        string? outputRoot = null,
        bool rejectOutputUnderProjects = false,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null,
        CancellationToken cancellationToken = default,
        Func<string, string, IReadOnlyCollection<string>,
            OperationResult<IEvaluationPublicationBoundary>>? createPublication = null)
    {
        outputRoot ??= fixture.OutputRoot;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var dependencies = new Tp1ExitDecisionRequestDependencies(
            createPublication: createPublication ?? ((root, id, leaves) =>
                OperationResult<IEvaluationPublicationBoundary>.Ok(
                    new FakePublicationBoundary(
                        root,
                        id,
                        leaves,
                        PublicationFailure.None))),
            rejectOutputUnderProjects: rejectOutputUnderProjects,
            resolvePhysicalPath: resolvePhysicalPath ?? ResolvePhysicalPath);
        var exit = await Tp1ExitDecisionRequestCli.RunAsync(
            [
                Tp1ExitDecisionRequestCli.ArmFlag,
                "--ceiling-evaluation-directory", fixture.CeilingDirectory,
                "--output-root", outputRoot,
                "--decision-request-id", DecisionRequestId,
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

    private static void AssertEvidenceMismatch(CliResult result, Fixture fixture)
    {
        Assert.Equal(1, result.ExitCode);
        Assert.Contains(
            "TP1_EXIT_DECISION_REQUEST_EVIDENCE_MISMATCH",
            result.Stderr,
            StringComparison.Ordinal);
        Assert.False(Directory.Exists(fixture.Destination));
        AssertNoStaging(fixture.OutputRoot);
    }

    private static void AssertNoStaging(string outputRoot)
    {
        if (Directory.Exists(outputRoot))
        {
            Assert.Empty(Directory.EnumerateDirectories(
                outputRoot,
                ".*.tp1-exit-request-tmp-*"));
        }
    }

    private static string RefFor(int value)
        => "sha256:" + checked(value + 1).ToString("x64");

    private static string Hash(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllBytes(
            path,
            JsonSerializer.SerializeToUtf8Bytes(value, SnakeJson));

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-tp1-exit-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static void DeleteDirectoryLink(string? path)
    {
        if (path is not null && Directory.Exists(path))
            Directory.Delete(path);
    }

    private sealed record Fixture(
        string Root,
        string CeilingDirectory,
        string EvaluationPath,
        string MarkdownPath,
        string EvaluationRunPath,
        string OutputRoot,
        string Destination);

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private readonly record struct SubmissionTuple(
        bool SourceRunOrderingVerified,
        bool SourceAuthenticityVerified,
        bool NumericEvidenceVerified,
        bool LocalTeacherCeilingAccepted,
        bool IndependentAcceptance,
        string Decision);

    public enum PublicationFailure
    {
        None,
        Create,
        Write,
        Publish,
        OutputIdentity,
        Cleanup,
    }

    private sealed class FakePublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly HashSet<string> _allowed;
        private readonly HashSet<string> _created = new(StringComparer.Ordinal);
        private readonly string _expectedOutputRoot;
        private readonly PublicationFailure _failure;
        private bool _published;

        internal FakePublicationBoundary(
            string outputRoot,
            string decisionRequestId,
            IReadOnlyCollection<string> leaves,
            PublicationFailure failure)
        {
            _allowed = new HashSet<string>(leaves, StringComparer.Ordinal);
            _expectedOutputRoot = Path.GetFullPath(outputRoot);
            _failure = failure;
            Directory.CreateDirectory(outputRoot);
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{decisionRequestId}.tp1-exit-request-tmp-{Guid.NewGuid():N}");
            FinalDirectory = Path.Combine(outputRoot, decisionRequestId);
            Directory.CreateDirectory(StagingDirectory);
        }

        public string StagingDirectory { get; }
        public string FinalDirectory { get; }
        public string CurrentDirectory => _published ? FinalDirectory : StagingDirectory;

        public void VerifyOutputRootIdentity(string expectedPhysicalPath)
        {
            if (_failure == PublicationFailure.OutputIdentity)
                throw new IOException("Injected output identity failure.");
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
            if (_failure is PublicationFailure.Write or PublicationFailure.Cleanup)
                throw new IOException("Injected artifact write failure.");
            if (!_allowed.Contains(leafName) || !_created.Add(leafName))
                throw new IOException("Unexpected exit-request artifact.");
            return new FileStream(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
        }

        public void PublishAndVerify()
        {
            if (_failure == PublicationFailure.Publish)
                throw new IOException("Injected publication failure.");
            if (!_created.SetEquals(_allowed))
                throw new IOException("Exit-request artifact set incomplete.");
            Directory.Move(StagingDirectory, FinalDirectory);
            _published = true;
            if (!_allowed.SetEquals(Directory.EnumerateFiles(FinalDirectory)
                    .Select(Path.GetFileName)!))
                throw new IOException("Published exit-request artifact set changed.");
        }

        public void CleanupFailedPublication()
        {
            if (_failure == PublicationFailure.Cleanup)
                throw new IOException("Injected cleanup failure.");
            if (Directory.Exists(CurrentDirectory))
                Directory.Delete(CurrentDirectory, recursive: true);
        }

        public void Dispose() { }
    }
}

file sealed class Tp1ExitDecisionWindowsFileSymlinkFactAttribute : FactAttribute
{
    public Tp1ExitDecisionWindowsFileSymlinkFactAttribute()
        => Skip = Tp1ExitDecisionFileSymlinkSupport.GetCapabilitySkipReason();
}

file static class Tp1ExitDecisionFileSymlinkSupport
{
    internal static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: Windows no-follow handle proof is required.";
        var root = Path.Combine(
            Path.GetTempPath(),
            $"tp1-exit-symlink-capability-{Guid.NewGuid():N}");
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

file sealed class Tp1ExitDecisionWindowsDirectorySymlinkFactAttribute : FactAttribute
{
    public Tp1ExitDecisionWindowsDirectorySymlinkFactAttribute()
        => Skip = Tp1ExitDecisionDirectorySymlinkSupport.GetCapabilitySkipReason();
}

file static class Tp1ExitDecisionDirectorySymlinkSupport
{
    internal static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: Windows directory reparse proof is required.";
        var root = Path.Combine(
            Path.GetTempPath(),
            $"tp1-exit-directory-symlink-capability-{Guid.NewGuid():N}");
        var target = Path.Combine(root, "target");
        var link = Path.Combine(root, "link");
        try
        {
            Directory.CreateDirectory(target);
            try
            {
                Directory.CreateSymbolicLink(link, target);
                return (File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0
                    ? null
                    : "REPARSE_TEST_PLATFORM_LIMITATION: directory link was not a reparse point.";
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
            if (Directory.Exists(link))
                Directory.Delete(link);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
