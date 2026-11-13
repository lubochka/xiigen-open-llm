// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;
using ContinuousLearningTp1EvaluationRunner;

namespace LlmFromScratch.Tests;

public sealed class ContinuousLearningTp1EvaluationRunnerTests
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
    private static readonly JsonSerializerOptions ArtifactJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };

    [Fact]
    public async Task SuccessfulRun_PublishesOnlyObservationAndManifestWithNegativeInvariants()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0), ValidRow("case-2", 1));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient();
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", maxCalls: 2),
                client,
                publications);

            Assert.Equal(0, result.ExitCode);
            Assert.Empty(result.Stderr);
            Assert.Equal(2, client.CallCount);
            var destination = Path.Combine(output, "run-001");
            Assert.True(Directory.Exists(destination));
            Assert.Equal(
                new[] { "tp1-evaluation-observations.jsonl", "tp1-evaluation-run.json" },
                Directory.EnumerateFiles(destination)
                    .Select(Path.GetFileName)
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .ToArray());
            Assert.Empty(Directory.EnumerateDirectories(output, "*tp1-evaluation-tmp*"));
            var publication = Assert.Single(publications.Boundaries);
            Assert.Equal(1, publication.PublishCalls);
            Assert.True(publication.RenamePerformed);
            Assert.False(Directory.Exists(publication.StagingDirectory));

            var observations = File.ReadAllLines(Path.Combine(
                destination, "tp1-evaluation-observations.jsonl"));
            Assert.Equal(2, observations.Length);
            for (var index = 0; index < observations.Length; index++)
            {
                using var document = JsonDocument.Parse(observations[index]);
                var row = document.RootElement;
                Assert.Equal($"case-{index + 1}", row.GetProperty("case_id").GetString());
                Assert.Equal(index, row.GetProperty("generation_index").GetInt32());
                Assert.Equal($"output-{index + 1}", row.GetProperty("output").GetString());
                Assert.False(row.GetProperty("dpo_train_ready").GetBoolean());
                Assert.False(row.GetProperty("teacher_output_used_for_training").GetBoolean());
                Assert.False(row.GetProperty("training_started").GetBoolean());
                Assert.Equal(0, row.GetProperty("dpo_steps_executed").GetInt32());
                Assert.False(row.GetProperty("checkpoint_promoted").GetBoolean());
                Assert.False(row.GetProperty("scoring_executed").GetBoolean());
                Assert.Equal("set-1", row.GetProperty("evaluation_set_id").GetString());
                Assert.Equal("domain-1", row.GetProperty("domain_id").GetString());
                Assert.Equal("scorer-explicit", row.GetProperty("scorer_id").GetString());
                Assert.Equal(EvidenceRef, row.GetProperty("source_ref").GetString());
                Assert.Equal("evaluation", row.GetProperty("split").GetString());
                Assert.False(row.TryGetProperty("prompt", out _));
                Assert.False(row.TryGetProperty("chosen", out _));
                Assert.False(row.TryGetProperty("rejected", out _));
                Assert.False(row.TryGetProperty("graph_dpo_pair", out _));
                Assert.False(row.TryGetProperty("audit_salt", out _));
                Assert.False(row.GetProperty("provenance")
                    .GetProperty("teacher_output_used_for_training").GetBoolean());
                Assert.Equal(
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                    row.GetProperty("provenance")
                        .GetProperty("model_artifact_digest_kind").GetString());
                Assert.Equal(
                    ModelLayerDigest,
                    row.GetProperty("provenance")
                        .GetProperty("model_artifact_digest").GetString());

                var serializedProvenance = JsonSerializer.Deserialize<TrainingTeacherProvenance>(
                    row.GetProperty("provenance").GetRawText(),
                    ArtifactJsonOptions);
                Assert.NotNull(serializedProvenance);
                var trainingAuthority = CreateAuthority(maxTeacherCalls: 2);
                Assert.False(trainingAuthority.ValidateBatch([serializedProvenance]).IsSuccess);
            }

            using (var manifestDocument = JsonDocument.Parse(File.ReadAllText(
                       Path.Combine(destination, "tp1-evaluation-run.json"))))
            {
                var manifest = manifestDocument.RootElement;
                Assert.Equal("run-001", manifest.GetProperty("run_id").GetString());
                Assert.Equal(2, manifest.GetProperty("input_row_count").GetInt32());
                Assert.Equal(2, manifest.GetProperty("teacher_calls_executed").GetInt32());
                Assert.Equal(ModelId, manifest.GetProperty("model_id").GetString());
                Assert.Equal(ManifestDigest, manifest.GetProperty("ollama_manifest_digest").GetString());
                Assert.Equal(ModelLayerDigest, manifest.GetProperty("ollama_model_layer_digest").GetString());
                Assert.Equal(EvidenceRef, manifest.GetProperty("local_retention_policy_ref").GetString());
                Assert.Equal(EvidenceRef, manifest.GetProperty("output_license_ref").GetString());
                Assert.Equal(EvidenceRef, manifest.GetProperty("teacher_license_evidence_ref").GetString());
                Assert.Equal(EvidenceRef, manifest.GetProperty("deployment_evidence_ref").GetString());
                Assert.False(manifest.GetProperty("dpo_train_ready").GetBoolean());
                Assert.False(manifest.GetProperty("teacher_output_used_for_training").GetBoolean());
                Assert.False(manifest.GetProperty("training_started").GetBoolean());
                Assert.Equal(0, manifest.GetProperty("dpo_steps_executed").GetInt32());
                Assert.False(manifest.GetProperty("checkpoint_promoted").GetBoolean());
                Assert.False(manifest.GetProperty("scoring_executed").GetBoolean());
            }

            var allArtifacts = string.Join('\n', observations)
                + File.ReadAllText(Path.Combine(destination, "tp1-evaluation-run.json"));
            Assert.DoesNotContain("\"chosen\"", allArtifacts, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"rejected\"", allArtifacts, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GraphDpoPair", allArtifacts, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Salt, allArtifacts, StringComparison.Ordinal);
            Assert.DoesNotContain("prompt one", allArtifacts, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task MissingArmFlag_FailsBeforeClientCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var calls = 0;
            var dependencies = CreatePortableFakeBoundaryDependencies(configuration =>
            {
                calls++;
                return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                    new FakeEvaluationClient());
            });
            var args = BuildArgs(input, output, "run-001", 1)
                .Where(static value => value != "--execute-evaluation")
                .ToArray();
            var result = await RunAsync(args, dependencies);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_NOT_ARMED");
            Assert.Equal(0, calls);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task ZeroOrMismatchedBudget_FailsBeforeClientCreation(int maxCalls)
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var calls = 0;
            var dependencies = CreatePortableFakeBoundaryDependencies(configuration =>
            {
                calls++;
                return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                    new FakeEvaluationClient());
            });
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", maxCalls), dependencies);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, calls);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("{not-json")]
    [InlineData("{\"evaluationSetId\":\"set-1\",\"caseId\":\"case-1\",\"domainId\":\"domain-1\",\"generationIndex\":0,\"prompt\":\"prompt one\",\"scorerId\":\"scorer-explicit\",\"sourceRef\":\"sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef\",\"split\":\"training\"}")]
    public async Task MalformedOrTrainingRow_FailsWithoutClientCreation(string row)
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, row);
            var output = Path.Combine(root, "published");
            var calls = 0;
            var dependencies = CreatePortableFakeBoundaryDependencies(configuration =>
            {
                calls++;
                return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                    new FakeEvaluationClient());
            });
            var result = await RunAsync(BuildArgs(input, output, "run-001", 1), dependencies);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, calls);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnknownOrDuplicateJsonProperty_FailsWithoutClientCreation(bool unknown)
    {
        var root = CreateTempRoot();
        try
        {
            var valid = ValidRow("case-1", 0);
            var row = unknown
                ? valid.Replace("\"split\"", "\"unexpected\":\"value\",\"split\"", StringComparison.Ordinal)
                : valid.Insert(1, "\"caseId\":\"duplicate\",");
            var input = WriteRows(root, row);
            var output = Path.Combine(root, "published");
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    created++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, created);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExistingDestination_FailsBeforeClientCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            Directory.CreateDirectory(Path.Combine(output, "run-001"));
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    created++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_DESTINATION_EXISTS");
            Assert.Equal(0, created);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task LexicalInputUnderOutput_FailsBeforeClientCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, root, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    created++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, created);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task DuplicateCorrelationRows_FailWithoutClientCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var row = ValidRow("case-1", 0);
            var input = WriteRows(root, row, row);
            var output = Path.Combine(root, "published");
            var calls = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 2),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    calls++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, calls);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task InvalidProvenance_AbortsWithoutFinalDirectory()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(trainingUse: true);
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                client,
                publications);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_PROVENANCE_INVALID");
            Assert.Equal(1, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ProvenanceClaimingMultipleInvocations_AbortsWithoutPublication()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(
                root,
                ValidRow("case-1", 0),
                ValidRow("case-2", 1));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(invocationCount: 2);
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 2),
                client,
                publications);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_PROVENANCE_INVALID");
            Assert.Equal(1, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task OutputRootUnderProjects_FailsBeforeClientCreation()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.GetFullPath(@"C:\Projects\tp1-evaluation-forbidden");
            var calls = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    calls++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, calls);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PhysicalProjectsAlias_FailsBeforeClientCreation()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var projects = Path.GetFullPath(@"C:\Projects");
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                    {
                        created++;
                        return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                            new FakeEvaluationClient());
                    },
                    (path, allowMissing) => OperationResult<string>.Ok(
                        StringComparer.OrdinalIgnoreCase.Equals(path, output)
                            ? Path.Combine(projects, "aliased-output")
                            : path)));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, created);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PhysicalInputOutputOverlap_FailsBeforeClientCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var physicalOutput = Path.Combine(root, "physical-output");
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                    {
                        created++;
                        return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                            new FakeEvaluationClient());
                    },
                    (path, allowMissing) => OperationResult<string>.Ok(
                        StringComparer.OrdinalIgnoreCase.Equals(path, input)
                            ? Path.Combine(physicalOutput, "input.jsonl")
                            : StringComparer.OrdinalIgnoreCase.Equals(path, output)
                                ? physicalOutput
                                : path)));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, created);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con.txt")]
    [InlineData("PRN.log")]
    [InlineData("AUX")]
    [InlineData("NUL.data")]
    [InlineData("COM1")]
    [InlineData("LPT9.txt")]
    [InlineData("trailing.")]
    public async Task UnsafeWindowsRunId_FailsBeforeClientCreation(string runId)
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var created = 0;
            var result = await RunAsync(
                BuildArgs(input, output, runId, 1),
                CreatePortableFakeBoundaryDependencies(configuration =>
                {
                    created++;
                    return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                        new FakeEvaluationClient());
                }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_INVALID_ARGUMENT");
            Assert.Equal(0, created);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task DualEvaluationAndTrainingClient_IsRejectedBeforeEvaluation()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var dual = new DualCapabilityClient();
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration => OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(dual),
                    publications: publications));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_CAPABILITY_INVALID");
            Assert.Equal(0, dual.EvaluationCalls);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task MissingOrDoublePermitIssue_AbortsWithoutPublication(
        bool issuePermit,
        bool doubleIssuePermit)
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(
                issuePermit: issuePermit,
                doubleIssuePermit: doubleIssuePermit);
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                client,
                publications);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_PERMIT_UNUSED");
            Assert.Equal(1, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task BlankOutput_AbortsWithoutFinalDirectory()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(output: "   ");
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                client,
                publications);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_OUTPUT_INVALID");
            Assert.Equal(1, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task MidRunFailure_CleansStagingAndDoesNotPublish()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0), ValidRow("case-2", 1));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(failAtCall: 2);
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 2),
                client,
                publications);

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_TEACHER_FAILED");
            Assert.Equal(2, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task CleanupFailure_IsExplicitAndCannotBecomeSuccess()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient(failAtCall: 1);
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                        OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                    createBoundary: (outputRoot, runId) =>
                        new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            cleanupFails: true)));

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("TP1_EVALUATION_CLEANUP_FAILED", result.Stderr, StringComparison.Ordinal);
            using var error = JsonDocument.Parse(result.Stderr);
            Assert.Contains(
                output,
                error.RootElement.GetProperty("error").GetString(),
                StringComparison.OrdinalIgnoreCase);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            Assert.Single(Directory.EnumerateDirectories(output, "*tp1-evaluation-tmp*"));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task SubstitutedStagingIdentity_CannotReachClientOrPublicationSuccess()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var clientCreations = 0;
            var boundary = default(FakePublicationBoundary);
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                    {
                        clientCreations++;
                        return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                            new FakeEvaluationClient());
                    },
                    createBoundary: (outputRoot, runId) =>
                    {
                        boundary = new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            stagingIdentityMismatch: true);
                        return boundary;
                    }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_FAILED");
            Assert.Equal(0, clientCreations);
            Assert.NotNull(boundary);
            Assert.Equal(0, boundary!.PublishCalls);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoStaging(output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task FinalIdentityMismatchAfterRename_CleansFinalAndCannotReturnSuccess()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient();
            FakePublicationBoundary? boundary = null;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                        OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                    createBoundary: (outputRoot, runId) =>
                    {
                        boundary = new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            finalIdentityMismatch: true);
                        return boundary;
                    }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_FAILED");
            Assert.Equal(1, client.CallCount);
            Assert.NotNull(boundary);
            Assert.Equal(1, boundary!.PublishCalls);
            Assert.True(boundary.RenamePerformed);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoStaging(output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ReplacedArtifactIdentityAfterRename_CleansFinalAndCannotReturnSuccess()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient();
            FakePublicationBoundary? boundary = null;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                        OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                    createBoundary: (outputRoot, runId) =>
                    {
                        boundary = new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            artifactIdentityMismatch: true);
                        return boundary;
                    }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_FAILED");
            Assert.Equal(1, client.CallCount);
            Assert.NotNull(boundary);
            Assert.Equal(1, boundary!.PublishCalls);
            Assert.True(boundary.RenamePerformed);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoStaging(output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExtraArtifactAfterRename_CleansFinalAndCannotReturnSuccess()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            var client = new FakeEvaluationClient();
            FakePublicationBoundary? boundary = null;
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration =>
                        OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                    createBoundary: (outputRoot, runId) =>
                    {
                        boundary = new FakePublicationBoundary(
                            outputRoot,
                            runId,
                            injectExtraChild: true);
                        return boundary;
                    }));

            Assert.Equal(1, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_FAILED");
            Assert.Equal(1, client.CallCount);
            Assert.NotNull(boundary);
            Assert.Equal(1, boundary!.PublishCalls);
            Assert.True(boundary.RenamePerformed);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoStaging(output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public void ProductionDependency_UsesWindowsHandleRelativePublicationFactory()
    {
        Assert.True(EvaluationRunnerDependencies.Production.RequiresWindowsHandlePublication);
        Assert.Equal(
            typeof(WindowsEvaluationPublicationBoundary),
            EvaluationRunnerDependencies.Production.CreatePublication.Method.DeclaringType);
        Assert.Equal(
            nameof(WindowsEvaluationPublicationBoundary.Create),
            EvaluationRunnerDependencies.Production.CreatePublication.Method.Name);
    }

    [Fact]
    public void FinalVerificationShare_IsCompatibleWithRetainedDeleteAccess()
    {
        Assert.Equal(
            FileShare.Read | FileShare.Write,
            WindowsEvaluationPublicationBoundary.RetainedDirectoryShareMode);
        Assert.False(WindowsEvaluationPublicationBoundary
            .RetainedDirectoryShareMode.HasFlag(FileShare.Delete));
        Assert.True(WindowsEvaluationPublicationBoundary
            .FinalVerificationShareMode.HasFlag(FileShare.Delete));
        Assert.Equal(
            FileShare.Read | FileShare.Write | FileShare.Delete,
            WindowsEvaluationPublicationBoundary.FinalVerificationShareMode);

        var layout = WindowsEvaluationPublicationBoundary
            .RenameInformationLayoutForTesting();
        Assert.Equal(IntPtr.Size == 8 ? 8 : 4, layout.RootOffset);
        Assert.Equal(layout.RootOffset + IntPtr.Size, layout.LengthOffset);
        Assert.Equal(layout.LengthOffset + sizeof(uint), layout.NameOffset);
    }

    [Fact]
    public void ArtifactGuardContract_DeniesMutationAndUsesExactFixedSet()
    {
        Assert.Equal(
            FileShare.Read,
            WindowsEvaluationPublicationBoundary.ArtifactWriterShareMode);
        Assert.False(WindowsEvaluationPublicationBoundary
            .ArtifactWriterShareMode.HasFlag(FileShare.Write));
        Assert.False(WindowsEvaluationPublicationBoundary
            .ArtifactWriterShareMode.HasFlag(FileShare.Delete));
        Assert.Equal(
            FileShare.Read | FileShare.Write,
            WindowsEvaluationPublicationBoundary.FinalArtifactVerificationShareMode);
        Assert.True(WindowsEvaluationPublicationBoundary
            .FinalArtifactVerificationShareMode.HasFlag(FileShare.Write));
        Assert.False(WindowsEvaluationPublicationBoundary
            .FinalArtifactVerificationShareMode.HasFlag(FileShare.Delete));
        Assert.Equal(
            new[]
            {
                "tp1-evaluation-observations.jsonl",
                "tp1-evaluation-run.json",
            },
            WindowsEvaluationPublicationBoundary.ExpectedArtifactLeafNames);
        Assert.Equal(
            WindowsEvaluationPublicationBoundary.ExpectedArtifactLeafNames.Count,
            WindowsEvaluationPublicationBoundary.ExpectedArtifactLeafNames
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void FileNamesInformationParser_AcceptsLongAlignedFourModEightOffset()
    {
        const int headerSize = sizeof(uint) * 3;
        const int nextEntryOffset = 60;
        var firstName = "tp1-evaluation-run.json";
        var secondName = "tp1-evaluation-observations.jsonl";
        var firstNameBytes = System.Text.Encoding.Unicode.GetBytes(firstName);
        var secondNameBytes = System.Text.Encoding.Unicode.GetBytes(secondName);
        Assert.True(headerSize + firstNameBytes.Length <= nextEntryOffset);
        Assert.Equal(0, nextEntryOffset % sizeof(uint));
        Assert.Equal(4, nextEntryOffset % sizeof(long));

        var buffer = new byte[
            nextEntryOffset + headerSize + secondNameBytes.Length];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(
            buffer.AsSpan(0, sizeof(uint)),
            (uint)nextEntryOffset);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(
            buffer.AsSpan(sizeof(uint) * 2, sizeof(uint)),
            (uint)firstNameBytes.Length);
        firstNameBytes.CopyTo(buffer, headerSize);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(
            buffer.AsSpan(nextEntryOffset + sizeof(uint) * 2, sizeof(uint)),
            (uint)secondNameBytes.Length);
        secondNameBytes.CopyTo(buffer, nextEntryOffset + headerSize);

        Assert.Equal(
            new[] { firstName, secondName },
            WindowsEvaluationPublicationBoundary.ParseDirectoryNamesForTesting(buffer));

        var malformed = (byte[])buffer.Clone();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(
            malformed.AsSpan(0, sizeof(uint)),
            (uint)(nextEntryOffset + 2));
        Assert.Throws<IOException>(() =>
            WindowsEvaluationPublicationBoundary.ParseDirectoryNamesForTesting(malformed));
    }

    [Fact]
    public void WindowsProductionBoundary_GuardsArtifactsUntilExactPublication()
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
            var created = WindowsEvaluationPublicationBoundary.Create(output, "run-001");
            Assert.True(created.IsSuccess);
            publication = Assert.IsAssignableFrom<IEvaluationPublicationBoundary>(created.Value);
            publication.VerifyStagingIdentity();
            WriteProductionArtifact(
                publication,
                "tp1-evaluation-observations.jsonl",
                "{\"observation\":true}\n");
            WriteProductionArtifact(
                publication,
                "tp1-evaluation-run.json",
                "{\"manifest\":true}\n");

            var observationPath = Path.Combine(
                publication.StagingDirectory,
                "tp1-evaluation-observations.jsonl");
            Assert.Throws<IOException>(() =>
            {
                using var denied = new FileStream(
                    observationPath,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete);
            });

            var replacementPath = Path.Combine(output, "replacement.tmp");
            File.WriteAllText(replacementPath, "replacement");
            AssertWindowsPathMutationDenied(() =>
                File.Move(replacementPath, observationPath, overwrite: true));
            Assert.True(File.Exists(replacementPath));
            Assert.True(File.Exists(observationPath));
            AssertWindowsPathMutationDenied(() => File.Delete(observationPath));
            Assert.True(File.Exists(observationPath));

            publication.PublishAndVerify();
            published = true;
            Assert.True(File.Exists(Path.Combine(
                publication.FinalDirectory,
                "tp1-evaluation-observations.jsonl")));
            Assert.True(File.Exists(Path.Combine(
                publication.FinalDirectory,
                "tp1-evaluation-run.json")));
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

    [Fact]
    public void WindowsProductionBoundary_ExtraRegularChildFailsBeforeRenameAndCleansStaging()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateTempRoot();
        IEvaluationPublicationBoundary? publication = null;
        var cleanupCompleted = false;
        try
        {
            var output = Path.Combine(root, "published");
            Directory.CreateDirectory(output);
            var created = WindowsEvaluationPublicationBoundary.Create(output, "run-001");
            Assert.True(created.IsSuccess);
            publication = Assert.IsAssignableFrom<IEvaluationPublicationBoundary>(created.Value);
            publication.VerifyStagingIdentity();
            WriteProductionArtifact(
                publication,
                "tp1-evaluation-observations.jsonl",
                "{\"observation\":true}\n");
            WriteProductionArtifact(
                publication,
                "tp1-evaluation-run.json",
                "{\"manifest\":true}\n");
            File.WriteAllText(
                Path.Combine(publication.StagingDirectory, "unexpected-child.txt"),
                "unexpected");

            Assert.Throws<IOException>(() => publication.PublishAndVerify());
            Assert.True(Directory.Exists(publication.StagingDirectory));
            Assert.False(Directory.Exists(publication.FinalDirectory));

            publication.CleanupFailedPublication();
            cleanupCompleted = true;
            Assert.False(Directory.Exists(publication.StagingDirectory));
            Assert.False(Directory.Exists(publication.FinalDirectory));
        }
        finally
        {
            if (!cleanupCompleted && publication is not null)
            {
                try { publication.CleanupFailedPublication(); }
                catch (Exception) { }
            }
            publication?.Dispose();
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task Cancellation_CleansStagingAndDoesNotPublish()
    {
        var root = CreateTempRoot();
        try
        {
            var input = WriteRows(root, ValidRow("case-1", 0));
            var output = Path.Combine(root, "published");
            using var cancellation = new CancellationTokenSource();
            var client = new FakeEvaluationClient(cancel: cancellation);
            var publications = new FakePublicationRecorder();
            var result = await RunAsync(
                BuildArgs(input, output, "run-001", 1),
                CreatePortableFakeBoundaryDependencies(
                    configuration => OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                    publications: publications),
                cancellation.Token);

            Assert.Equal(130, result.ExitCode);
            AssertErrorCode(result, "TP1_EVALUATION_CANCELLED");
            Assert.Equal(1, client.CallCount);
            Assert.False(Directory.Exists(Path.Combine(output, "run-001")));
            AssertNoUnintendedPublish(publications, output);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public void CompositionWrapper_IsEvaluationOnlyAndDisposesOwnedInner()
    {
        var inner = new FakeEvaluationClient();
        var wrapper = ContinuousLearningTp1EvaluationComposition.OwnForTesting(inner);

        Assert.IsAssignableFrom<ILegalTeacherEvaluationInferenceClient>(wrapper);
        Assert.False(wrapper is ITrainingTeacherInferenceClient);
        wrapper.Dispose();
        wrapper.Dispose();
        Assert.Equal(1, inner.DisposeCount);
    }

    [Fact]
    public void ProductionComposition_ReturnsOnlyEvaluationCapability()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            ModelId,
            ManifestDigest,
            ModelLayerDigest,
            maxTeacherCalls: 1,
            EvidenceRef,
            EvidenceRef,
            Convert.FromBase64String(Salt));
        Assert.True(configuration.IsSuccess);
        Assert.NotNull(configuration.Value);

        var created = ContinuousLearningTp1EvaluationComposition.CreateClient(
            configuration.Value!);

        Assert.True(created.IsSuccess);
        Assert.NotNull(created.Value);
        Assert.False(created.Value is ITrainingTeacherInferenceClient);
        created.Value!.Dispose();
    }

    [Fact]
    public void CompositionWrapper_IsAuthoringOnlyAndDisposesOwnedInner()
    {
        var inner = new DualCapabilityClient();
        var wrapper = ContinuousLearningTp1AuthoringComposition
            .OwnForTesting(inner);

        Assert.IsAssignableFrom<ITrainingTeacherInferenceClient>(wrapper);
        Assert.False(wrapper is ILegalTeacherEvaluationInferenceClient);
        wrapper.Dispose();
        wrapper.Dispose();
        Assert.Equal(1, inner.DisposeCount);
    }

    [Fact]
    public void ProductionAuthoringComposition_ReturnsOnlyTrainingCapability()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            ModelId,
            ManifestDigest,
            ModelLayerDigest,
            maxTeacherCalls: 1,
            EvidenceRef,
            EvidenceRef,
            Convert.FromBase64String(Salt));
        Assert.True(configuration.IsSuccess);

        var created = ContinuousLearningTp1AuthoringComposition.CreateClient(
            configuration.Value!);

        Assert.True(created.IsSuccess);
        Assert.NotNull(created.Value);
        Assert.False(created.Value is ILegalTeacherEvaluationInferenceClient);
        created.Value!.Dispose();
    }

    [Fact]
    public void CanonicalLegalEvidenceReference_RequiresExactLowercaseSha256Form()
    {
        Assert.True(ContinuousLearningTp1DpoConfiguration
            .IsCanonicalLegalEvidenceRef(EvidenceRef));
        Assert.False(ContinuousLearningTp1DpoConfiguration
            .IsCanonicalLegalEvidenceRef(null));
        Assert.False(ContinuousLearningTp1DpoConfiguration
            .IsCanonicalLegalEvidenceRef("sha256:abc"));
        Assert.False(ContinuousLearningTp1DpoConfiguration
            .IsCanonicalLegalEvidenceRef(EvidenceRef.ToUpperInvariant()));
    }

    private static string ValidRow(string caseId, int generationIndex)
        => JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["evaluationSetId"] = "set-1",
            ["caseId"] = caseId,
            ["domainId"] = "domain-1",
            ["generationIndex"] = generationIndex,
            ["prompt"] = "prompt one",
            ["scorerId"] = "scorer-explicit",
            ["sourceRef"] = EvidenceRef,
            ["split"] = "evaluation",
        });

    private static string WriteRows(string root, params string[] rows)
    {
        var path = Path.Combine(root, "input.jsonl");
        File.WriteAllLines(path, rows, new System.Text.UTF8Encoding(false));
        return path;
    }

    private static void WriteProductionArtifact(
        IEvaluationPublicationBoundary publication,
        string leafName,
        string content)
    {
        using var stream = publication.CreateArtifactFile(leafName);
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private static string[] BuildArgs(
        string input,
        string output,
        string runId,
        int maxCalls)
        =>
        [
            "--execute-evaluation",
            "--input-jsonl", input,
            "--output-root", output,
            "--run-id", runId,
            "--model-id", ModelId,
            "--ollama-manifest-digest", ManifestDigest,
            "--ollama-model-layer-digest", ModelLayerDigest,
            "--max-teacher-calls", maxCalls.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--audit-salt-base64", Salt,
            "--local-retention-policy-ref", EvidenceRef,
            "--output-license-ref", EvidenceRef,
            "--teacher-license-evidence-ref", EvidenceRef,
            "--deployment-evidence-ref", EvidenceRef,
        ];

    private static Task<CliResult> RunAsync(
        string[] args,
        FakeEvaluationClient client,
        FakePublicationRecorder? publications = null)
        => RunAsync(
            args,
            CreatePortableFakeBoundaryDependencies(
                configuration =>
                    OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client),
                publications: publications));

    private static EvaluationRunnerDependencies CreatePortableFakeBoundaryDependencies(
        Func<ContinuousLearningTp1DpoConfiguration,
            OperationResult<ILegalTeacherEvaluationInferenceClient>> createClient,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null,
        Func<string, string, FakePublicationBoundary>? createBoundary = null,
        FakePublicationRecorder? publications = null)
        => new(
            createClient,
            resolvePhysicalPath,
            createPublication: (outputRoot, runId) =>
            {
                // Переносимый fake-client тест явно получает fake publication boundary.
                // Так Linux/WSL проверяет бизнес-ошибку, а не production Windows gate.
                var boundary = createBoundary?.Invoke(outputRoot, runId)
                    ?? new FakePublicationBoundary(outputRoot, runId);
                publications?.Record(boundary);
                return OperationResult<IEvaluationPublicationBoundary>.Ok(boundary);
            });

    private static async Task<CliResult> RunAsync(
        string[] args,
        EvaluationRunnerDependencies dependencies,
        CancellationToken cancellationToken = default)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exit = await EvaluationCli.RunAsync(
            args, stdout, stderr, cancellationToken, dependencies);
        return new CliResult(exit, stdout.ToString(), stderr.ToString());
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"xiigen-tp1-eval-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void AssertNoStaging(string output)
    {
        if (Directory.Exists(output))
            Assert.Empty(Directory.EnumerateDirectories(output, "*tp1-evaluation-tmp*"));
    }

    private static void AssertErrorCode(CliResult result, string expectedErrorCode)
    {
        using var error = JsonDocument.Parse(result.Stderr);
        Assert.False(error.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(
            expectedErrorCode,
            error.RootElement.GetProperty("error_code").GetString());
    }

    private static void AssertNoUnintendedPublish(
        FakePublicationRecorder publications,
        string output,
        string runId = "run-001")
    {
        var publication = Assert.Single(publications.Boundaries);
        Assert.Equal(0, publication.PublishCalls);
        Assert.False(publication.RenamePerformed);
        Assert.False(Directory.Exists(Path.Combine(output, runId)));
        AssertNoStaging(output);
    }

    private static void AssertWindowsPathMutationDenied(Action mutation)
    {
        try
        {
            mutation();
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        Assert.Fail("Expected Windows to deny the guarded path mutation.");
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static LegalTrainingTeacherAdmissionAuthority CreateAuthority(int maxTeacherCalls)
    {
        var created = LegalTrainingTeacherAdmissionAuthority.Create(
            [new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ModelId,
                ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                EvidenceRef,
                EvidenceRef)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ModelLayerDigest
            }],
            maxTeacherCalls,
            Convert.FromBase64String(Salt));
        Assert.True(created.IsSuccess);
        Assert.NotNull(created.Value);
        return created.Value!;
    }

    private sealed class FakePublicationRecorder
    {
        private readonly List<FakePublicationBoundary> _boundaries = [];

        internal IReadOnlyList<FakePublicationBoundary> Boundaries => _boundaries;

        internal void Record(FakePublicationBoundary boundary)
        {
            // Рекордер хранит каждую fake-boundary, чтобы тест отдельно доказал:
            // публикации не было, staging убран, production gate при этом не менялся.
            _boundaries.Add(boundary);
        }
    }

    private sealed class FakeEvaluationClient(
        bool trainingUse = false,
        int failAtCall = 0,
        CancellationTokenSource? cancel = null,
        string? output = null,
        int invocationCount = 1,
        bool issuePermit = true,
        bool doubleIssuePermit = false)
        : ILegalTeacherEvaluationInferenceClient
    {
        public int CallCount { get; private set; }
        public int DisposeCount { get; private set; }

        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
            string prompt,
            ILegalTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (issuePermit)
            {
                var issued = callPermit.TryIssue();
                if (!issued.IsSuccess)
                    return Task.FromResult(issued.MapError<TeacherEvaluationInferenceResult>());
                if (doubleIssuePermit)
                    _ = callPermit.TryIssue();
            }
            cancel?.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            if (CallCount == failAtCall)
            {
                return Task.FromResult(
                    OperationResult<TeacherEvaluationInferenceResult>.Fail(
                        "Scripted failure.", "SCRIPTED_FAILURE"));
            }

            return Task.FromResult(OperationResult<TeacherEvaluationInferenceResult>.Ok(
                new TeacherEvaluationInferenceResult(
                    output ?? $"output-{CallCount}",
                    new TrainingTeacherProvenance(
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
                        TeacherOutputUsedForTraining: trainingUse,
                        RequestId: $"request-{CallCount}",
                        InvocationCount: invocationCount)
                    {
                        ModelArtifactDigestKind =
                            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                        ModelArtifactDigest = ModelLayerDigest
                    })));
        }

        public void Dispose() => DisposeCount++;
    }

    private sealed class FakePublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly bool _stagingIdentityMismatch;
        private readonly bool _finalIdentityMismatch;
        private readonly bool _artifactIdentityMismatch;
        private readonly bool _injectExtraChild;
        private readonly bool _cleanupFails;
        private bool _renamed;

        internal FakePublicationBoundary(
            string outputRoot,
            string runId,
            bool stagingIdentityMismatch = false,
            bool finalIdentityMismatch = false,
            bool artifactIdentityMismatch = false,
            bool injectExtraChild = false,
            bool cleanupFails = false)
        {
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{runId}.tp1-evaluation-tmp-fake");
            FinalDirectory = Path.Combine(outputRoot, runId);
            _stagingIdentityMismatch = stagingIdentityMismatch;
            _finalIdentityMismatch = finalIdentityMismatch;
            _artifactIdentityMismatch = artifactIdentityMismatch;
            _injectExtraChild = injectExtraChild;
            _cleanupFails = cleanupFails;
            Directory.CreateDirectory(StagingDirectory);
        }

        public string StagingDirectory { get; }
        public string FinalDirectory { get; }
        public string CurrentDirectory => _renamed ? FinalDirectory : StagingDirectory;
        internal int PublishCalls { get; private set; }
        internal bool RenamePerformed => _renamed;

        public void VerifyOutputRootIdentity(string expectedPhysicalPath)
        {
        }

        public void VerifyStagingIdentity()
        {
            if (_stagingIdentityMismatch)
                throw new IOException("Scripted staging junction or identity mismatch.");
        }

        public FileStream CreateArtifactFile(string leafName)
            => new(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 16_384,
                FileOptions.WriteThrough);

        public void PublishAndVerify()
        {
            PublishCalls++;
            Directory.Move(StagingDirectory, FinalDirectory);
            _renamed = true;
            if (_finalIdentityMismatch)
                throw new IOException("Scripted post-rename final identity mismatch.");
            if (_artifactIdentityMismatch)
            {
                var artifact = Path.Combine(
                    FinalDirectory,
                    "tp1-evaluation-observations.jsonl");
                File.Delete(artifact);
                File.WriteAllText(artifact, "scripted replacement");
                throw new IOException("Scripted post-rename artifact identity mismatch.");
            }
            if (_injectExtraChild)
            {
                File.WriteAllText(
                    Path.Combine(FinalDirectory, "unexpected-child.txt"),
                    "scripted extra child");
                throw new IOException("Scripted post-rename extra artifact.");
            }
        }

        public void CleanupFailedPublication()
        {
            if (_cleanupFails)
                throw new IOException("Scripted cleanup failure.");
            if (Directory.Exists(CurrentDirectory))
                Directory.Delete(CurrentDirectory, recursive: true);
        }

        public void Dispose()
        {
        }
    }

    private sealed class DualCapabilityClient
        : ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient
    {
        internal int EvaluationCalls { get; private set; }
        internal int DisposeCount { get; private set; }

        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
            string prompt,
            ILegalTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            EvaluationCalls++;
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Fail(
                    "Must not be called.", "UNEXPECTED_CALL"));
        }

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<TrainingTeacherInferenceResult>.Fail(
                    "Must not be called.", "UNEXPECTED_CALL"));

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);
}
