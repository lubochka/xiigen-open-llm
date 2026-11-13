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

public sealed class ContinuousLearningTp1ExitDecisionSubmissionValidationRunnerTests
{
    private const string DecisionRequestId = "tp1-exit-decision-request-001";
    private const string ValidationId = "tp1-exit-submission-validation-001";
    private const string EvaluationId = "tp1-ceiling-evaluation-001";
    private const string ModelId = "qwen3.5:9b";
    private const string ManifestDigest =
        "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7";
    private const string ModelLayerDigest =
        "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c";
    private const string EvidenceRef =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string AuthorityRef =
        "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string LubaRef =
        "sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string ReasonRef =
        "sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

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

    [Theory]
    [InlineData(true, true, "TP1_EXIT_ACCEPT")]
    [InlineData(true, false, "TP1_EXIT_REJECT")]
    [InlineData(false, false, "TP1_EXIT_REJECT")]
    public async Task ShapeConformantSubmission_RemainsUntrustedAndNeverAcceptsTp1(
        bool candidate,
        bool allChecks,
        string decision)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate);
            var submission = Submission(
                candidate,
                allChecks,
                allChecks,
                allChecks,
                allChecks && candidate,
                allChecks && candidate,
                decision);
            WriteSubmission(fixture, submission);

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Stderr);
            Assert.Equal(
                Tp1ExitDecisionSubmissionValidationCli.ArtifactLeafNames
                    .Order(StringComparer.Ordinal),
                Directory.EnumerateFiles(fixture.Destination)
                    .Select(Path.GetFileName)
                    .Order(StringComparer.Ordinal));
            using var validation = ReadJson(fixture,
                Tp1ExitDecisionSubmissionValidationCli.ValidationLeaf);
            using var run = ReadJson(fixture,
                Tp1ExitDecisionSubmissionValidationCli.ValidationRunLeaf);
            var value = validation.RootElement;
            Assert.Equal(
                Tp1ExitDecisionSubmissionValidationCli.FixedStatus,
                value.GetProperty("status").GetString());
            Assert.True(value.GetProperty("submission_present").GetBoolean());
            Assert.True(value.GetProperty("shape_validated").GetBoolean());
            Assert.True(value.GetProperty("schema_template_only").GetBoolean());
            Assert.True(value.GetProperty("schema_not_authority").GetBoolean());
            Assert.True(value.GetProperty("schema_never_parse_as_receipt").GetBoolean());
            AssertFalseTrustAndExecution(value);
            var claims = value.GetProperty("untrusted_declared_claims");
            Assert.Equal(decision, claims.GetProperty("tp1_exit_decision").GetString());
            Assert.Equal(
                allChecks && candidate,
                claims.GetProperty("independent_acceptance").GetBoolean());
            Assert.False(value.TryGetProperty("tp1_exit_decision", out _));
            Assert.Equal(
                Hash(File.ReadAllBytes(fixture.SubmissionPath)),
                value.GetProperty("original_submission_sha256").GetString());
            Assert.Equal(
                Hash(File.ReadAllBytes(Path.Combine(
                    fixture.Destination,
                    Tp1ExitDecisionSubmissionValidationCli.ValidationLeaf))),
                run.RootElement.GetProperty("validation_sha256").GetString());
            Assert.Equal(
                Tp1ExitDecisionSubmissionValidationCli.ArtifactLeafNames,
                run.RootElement.GetProperty("published_artifact_leaf_names")
                    .EnumerateArray().Select(static item => item.GetString()));
            Assert.False(run.RootElement.TryGetProperty("validation_run_sha256", out _));
            AssertFalseTrustAndExecution(run.RootElement);
            AssertCompletePublishedHashDag(
                fixture,
                submission,
                validation.RootElement,
                run.RootElement);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public void SharedSchemaBuilder_PreservesIndependentGoldenBytes()
    {
        var actual = Tp1ExitDecisionSubmissionContract.BuildSchema(
            DecisionRequestId,
            EvidenceRef,
            candidate: true);
        var expected = BuildIndependentGoldenSchema(
            DecisionRequestId,
            EvidenceRef,
            candidate: true);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EveryAuthorityTuple_IsAcceptedExactlyByIndependentPredicate(bool candidate)
    {
        for (var mask = 0; mask < 1 << 5; mask++)
        foreach (var decision in new[]
        {
            "TP1_EXIT_ACCEPT",
            "TP1_EXIT_REJECT",
            "TP2_START",
        })
        {
            var ordering = (mask & 1) != 0;
            var authenticity = (mask & 2) != 0;
            var numeric = (mask & 4) != 0;
            var ceiling = (mask & 8) != 0;
            var independent = (mask & 16) != 0;
            var expected = ExpectedAllowed(
                candidate,
                ordering,
                authenticity,
                numeric,
                ceiling,
                independent,
                decision);

            Assert.Equal(
                expected,
                Tp1ExitDecisionSubmissionContract.IsCombinationAllowed(
                    candidate,
                    ordering,
                    authenticity,
                    numeric,
                    ceiling,
                    independent,
                    decision));
        }
    }

    [Fact]
    public async Task WhitespaceAndPropertyOrder_AreAcceptedButOriginalAndCanonicalHashesDiffer()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            var submission = Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT");
            var node = JsonSerializer.SerializeToNode(submission, SnakeJson)!.AsObject();
            var reversed = new JsonObject();
            foreach (var property in node.Reverse())
                reversed[property.Key] = property.Value?.DeepClone();
            File.WriteAllText(
                fixture.SubmissionPath,
                reversed.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
                new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            using var validation = ReadJson(
                fixture,
                Tp1ExitDecisionSubmissionValidationCli.ValidationLeaf);
            using var run = ReadJson(
                fixture,
                Tp1ExitDecisionSubmissionValidationCli.ValidationRunLeaf);
            Assert.NotEqual(
                validation.RootElement.GetProperty("original_submission_sha256").GetString(),
                validation.RootElement.GetProperty(
                    "canonical_submission_semantic_record_sha256").GetString());
            AssertCompletePublishedHashDag(
                fixture,
                submission,
                validation.RootElement,
                run.RootElement);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("wrong-type")]
    [InlineData("request-const")]
    [InlineData("candidate-const")]
    [InlineData("bad-ref")]
    [InlineData("bad-utc")]
    [InlineData("empty-reasons")]
    [InlineData("duplicate-reasons")]
    [InlineData("forbidden-tuple")]
    [InlineData("tp2-field")]
    public async Task InvalidHumanSubmission_FailsAsShapeInvalid(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            var submission = Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT");
            WriteSubmission(fixture, submission);
            MutateSubmission(fixture.SubmissionPath, mutation);

            var result = await RunAsync(fixture);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_SHAPE_INVALID");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("request")]
    [InlineData("request-roster")]
    [InlineData("markdown")]
    [InlineData("schema")]
    [InlineData("run")]
    [InlineData("run-hash")]
    [InlineData("roster")]
    [InlineData("posture")]
    public async Task TamperedRequestPacket_FailsAsEvidenceMismatch(string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            TamperRequestPacket(fixture, mutation);

            var result = await RunAsync(fixture);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_EVIDENCE_MISMATCH");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("selected-run")]
    [InlineData("arithmetic")]
    [InlineData("reason")]
    public async Task CanonicalForgedRequestWithRecomputedRunHash_IsEvidenceMismatch(
        string mutation)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            RewriteRequestAndRehashRun(fixture, request => mutation switch
            {
                "selected-run" => request with
                {
                    SelectedRuns =
                    [
                        request.SelectedRuns[0],
                        request.SelectedRuns[1] with
                        {
                            RunId = request.SelectedRuns[0].RunId,
                        },
                        request.SelectedRuns[2],
                    ],
                },
                "arithmetic" => request with
                {
                    SelectedCaseCount = checked(request.SelectedCaseCount + 1),
                },
                "reason" => request with
                {
                    SourceReasonCodes = ["FORGED_REASON"],
                },
                _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
            });

            var result = await RunAsync(fixture);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_EVIDENCE_MISMATCH");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("request")]
    [InlineData("submission")]
    public async Task SourceRaceDuringPublication_IsEvidenceMismatchAndCleansStaging(
        string source)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            var drifted = false;
            var sourceDirectory = source == "request"
                ? fixture.RequestDirectory
                : fixture.SubmissionDirectory;
            OperationResult<string> Resolve(string path, bool allowMissing)
            {
                _ = allowMissing;
                var physical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                if (drifted && StringComparer.OrdinalIgnoreCase.Equals(
                    physical,
                    sourceDirectory))
                    physical += "-replacement";
                return OperationResult<string>.Ok(physical);
            }

            var result = await RunAsync(
                fixture,
                resolvePhysicalPath: Resolve,
                boundaryFactory: (output, id, leaves) => new FakePublicationBoundary(
                    output,
                    id,
                    leaves,
                    onFirstCreate: () => drifted = true));

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_EVIDENCE_MISMATCH");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task LogicalAndPhysicalPathAliases_AreRejectedBeforePublication()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var logical = await RunAsync(fixture, outputRoot: fixture.SubmissionDirectory);
            AssertError(logical, "TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID");

            OperationResult<string> Alias(string path, bool allowMissing)
            {
                _ = allowMissing;
                var resolved = StringComparer.OrdinalIgnoreCase.Equals(
                    Path.GetFullPath(path),
                    fixture.SubmissionDirectory)
                    ? fixture.RequestDirectory
                    : Path.GetFullPath(path);
                return OperationResult<string>.Ok(resolved);
            }
            var physical = await RunAsync(fixture, resolvePhysicalPath: Alias);
            AssertError(physical, "TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID");
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task SubmissionLeafReparsePoint_IsRejectedWithoutFollowingIt()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            var target = Path.Combine(root, "untrusted-target.json");
            WriteJson(target, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            File.Delete(fixture.SubmissionPath);
            try
            {
                File.CreateSymbolicLink(fixture.SubmissionPath, target);
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException or NotSupportedException)
            {
                return;
            }

            var result = await RunAsync(fixture);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_SHAPE_INVALID");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("request")]
    [InlineData("submission")]
    [InlineData("output")]
    public async Task DirectoryReparsePoints_AreInputInvalidWithoutPublication(string source)
    {
        if (!OperatingSystem.IsWindows())
            return;
        var root = CreateTempRoot();
        string? alias = null;
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            var target = source switch
            {
                "request" => fixture.RequestDirectory,
                "submission" => fixture.SubmissionDirectory,
                "output" => fixture.OutputRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };
            if (source == "output")
                Directory.CreateDirectory(target);
            alias = Path.Combine(root, $"{source}-directory-alias");
            try
            {
                Directory.CreateSymbolicLink(alias, target);
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException or NotSupportedException)
            {
                return;
            }

            var result = await RunAsync(
                fixture,
                requestDirectory: source == "request" ? alias : null,
                submissionDirectory: source == "submission" ? alias : null,
                outputRoot: source == "output" ? alias : null);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID");
            AssertNoPublication(fixture);
        }
        finally
        {
            if (alias is not null && Directory.Exists(alias))
                Directory.Delete(alias);
            DeleteIfExists(root);
        }
    }

    [Fact]
    public async Task ProductionPolicy_RejectsOutputUnderProjects()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            var output = Path.Combine(@"C:\Projects", $"tp1-validation-{Guid.NewGuid():N}");

            var result = await RunAsync(
                fixture,
                outputRoot: output,
                rejectOutputUnderProjects: true);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_INPUT_INVALID");
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task ExistingDestination_IsOnlyPublicationFailedAndIsNotOverwritten()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            Directory.CreateDirectory(fixture.Destination);
            var sentinel = Path.Combine(fixture.Destination, "sentinel.txt");
            File.WriteAllText(sentinel, "keep", new UTF8Encoding(false));

            var result = await RunAsync(fixture);

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED");
            Assert.Equal("keep", File.ReadAllText(sentinel));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PostCreateOutputPhysicalIdentityDrift_IsPublicationFailedBeforeBoundaryCreation()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));
            var outputObservations = 0;
            var publicationCreations = 0;
            OperationResult<string> Resolve(string path, bool allowMissing)
            {
                _ = allowMissing;
                var physical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                if (StringComparer.OrdinalIgnoreCase.Equals(
                    physical,
                    fixture.OutputRoot))
                {
                    outputObservations++;
                    if (outputObservations > 1)
                        physical += "-replacement";
                }
                return OperationResult<string>.Ok(physical);
            }

            var result = await RunAsync(
                fixture,
                resolvePhysicalPath: Resolve,
                createPublication: (output, id, leaves) =>
                {
                    publicationCreations++;
                    return OperationResult<IEvaluationPublicationBoundary>.Ok(
                        new FakePublicationBoundary(output, id, leaves));
                });

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED");
            Assert.True(outputObservations >= 2);
            Assert.Equal(0, publicationCreations);
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task CreatePublicationFailure_IsPublicationFailedWithoutArtifacts()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var result = await RunAsync(
                fixture,
                createPublication: static (_, _, _) =>
                    OperationResult<IEvaluationPublicationBoundary>.Fail("no", "no"));

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task VerifyOutputRootIdentityFailure_IsPublicationFailedAndCleansStaging()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var result = await RunAsync(
                fixture,
                boundaryFactory: (output, id, leaves) => new FakePublicationBoundary(
                    output,
                    id,
                    leaves,
                    failOutputIdentity: true));

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED");
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Theory]
    [InlineData("write")]
    [InlineData("publish")]
    [InlineData("cleanup")]
    public async Task PublicationAndCleanupFailures_MapOnlyToPublicationFailed(string failure)
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var result = await RunAsync(
                fixture,
                boundaryFactory: (output, id, leaves) => new FakePublicationBoundary(
                    output,
                    id,
                    leaves,
                    failWrite: failure is "write" or "cleanup",
                    failPublish: failure == "publish",
                    failCleanup: failure == "cleanup"));

            AssertError(result, "TP1_EXIT_SUBMISSION_VALIDATION_PUBLICATION_FAILED");
            Assert.False(Directory.Exists(fixture.Destination));
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task CancellationDuringPublication_Returns130AndCleansStaging()
    {
        var root = CreateTempRoot();
        using var cancellation = new CancellationTokenSource();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var result = await RunAsync(
                fixture,
                cancellationToken: cancellation.Token,
                boundaryFactory: (output, id, leaves) => new FakePublicationBoundary(
                    output,
                    id,
                    leaves,
                    onFirstCreate: cancellation.Cancel));

            Assert.Equal(130, result.ExitCode);
            Assert.Contains(
                "TP1_EXIT_SUBMISSION_VALIDATION_CANCELLED",
                result.Stderr,
                StringComparison.Ordinal);
            AssertNoPublication(fixture);
        }
        finally { DeleteIfExists(root); }
    }

    [Fact]
    public async Task PublishedPacket_DoesNotLeakPathsAuthorityRefsReasonsOrRawSubmission()
    {
        var root = CreateTempRoot();
        try
        {
            var fixture = CreateFixture(root, candidate: true);
            WriteSubmission(fixture, Submission(
                true, true, true, true, true, true, "TP1_EXIT_ACCEPT"));

            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            var aggregate = string.Join("\n", Directory.EnumerateFiles(fixture.Destination)
                .Select(File.ReadAllText));
            Assert.DoesNotContain(fixture.Root, aggregate, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(AuthorityRef, aggregate, StringComparison.Ordinal);
            Assert.DoesNotContain(LubaRef, aggregate, StringComparison.Ordinal);
            Assert.DoesNotContain(ReasonRef, aggregate, StringComparison.Ordinal);
            Assert.DoesNotContain("independent_reviewed_at_utc", aggregate,
                StringComparison.Ordinal);
            Assert.DoesNotContain("luba_decided_at_utc", aggregate,
                StringComparison.Ordinal);
        }
        finally { DeleteIfExists(root); }
    }

    private static Fixture CreateFixture(string root, bool candidate)
    {
        var requestDirectory = Path.Combine(root, DecisionRequestId);
        var submissionDirectory = Path.Combine(root, "submission");
        var outputRoot = Path.Combine(root, "output");
        Directory.CreateDirectory(requestDirectory);
        Directory.CreateDirectory(submissionDirectory);
        var requestPath = Path.Combine(
            requestDirectory,
            Tp1ExitDecisionRequestCli.DecisionRequestLeaf);
        var markdownPath = Path.Combine(
            requestDirectory,
            Tp1ExitDecisionRequestCli.MarkdownLeaf);
        var schemaPath = Path.Combine(
            requestDirectory,
            Tp1ExitDecisionRequestCli.SubmissionSchemaLeaf);
        var runPath = Path.Combine(
            requestDirectory,
            Tp1ExitDecisionRequestCli.DecisionRequestRunLeaf);
        var submissionPath = Path.Combine(
            submissionDirectory,
            Tp1ExitDecisionSubmissionValidationCli.SubmissionLeaf);
        var hashes = new Tp1ExitDecisionSourceHashes(
            EvidenceRef,
            RefFor(2),
            RefFor(3));
        var goals = candidate
            ? new[] { 0.60D, 0.61D, 0.615D }
            : new[] { 0.60D, 0.62D, 0.61D };
        var wins = candidate ? new[] { 0, 0, 0 } : new[] { 1, 1, 1 };
        var selected = goals.Select((goal, index) =>
            new Tp1LocalTeacherCeilingSelectedRun(
                index + 4,
                $"scoring-run-{index + 4}",
                RefFor(index + 10),
                10,
                wins[index],
                goal)).ToArray();
        var totalWins = selected.Sum(static value => value.StrictTeacherWins);
        var improvement = Math.Max(goals[1], goals[2]) - goals[0];
        var plateau = improvement < 0.02D;
        var lowBeat = totalWins * 10 < 30;
        var computedCandidate = plateau && lowBeat;
        Assert.Equal(candidate, computedCandidate);
        var request = new Tp1ExitDecisionRequestArtifact(
            Tp1ExitDecisionRequestCli.FixedStatus,
            DecisionRequestId,
            EvaluationId,
            Tp1LocalTeacherCeilingCli.FixedStatus,
            Tp1LocalTeacherCeilingCli.FixedPolicyId,
            hashes,
            new Tp1ReviewInvariantSet(
                "TP1_LOCAL", "evaluation-set", "translation", "held-out-scorer",
                "v1", "threshold-v1", 1, 10, RefFor(20), RefFor(21), RefFor(22),
                ModelId, "ollama_manifest_sha256", ManifestDigest, ModelLayerDigest,
                RefFor(23), RefFor(24), RefFor(25), RefFor(26)),
            selected,
            30,
            totalWins,
            goals[0],
            Math.Max(goals[1], goals[2]),
            improvement,
            (double)totalWins / 30,
            plateau,
            lowBeat,
            SourceLocalTeacherCeilingEvaluated: true,
            SourceLocalTeacherCeilingCandidate: candidate,
            SourceReasonCodes: Reasons(plateau, lowBeat, improvement),
            new Tp1ExitIndependentReviewPrerequisite(
                "independent_tp1_evidence_reviewer",
                Required: true,
                [
                    "source_run_ordering_verified",
                    "source_authenticity_verified",
                    "numeric_evidence_verified",
                    "local_teacher_ceiling_accepted",
                ],
                AuthorityEvidencePresent: false),
            new Tp1ExitLubaDecisionPrerequisite(
                "luba_tp1_decision_authority",
                Required: true,
                candidate
                    ? ["TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT"]
                    : ["TP1_EXIT_REJECT"],
                DecisionEvidencePresent: false),
            new Tp1ExitFutureTp2StartRequirements(
                SeparateRequestRequired: true,
                "A separate future TP2-start request must include a current ProviderTermsProof.",
                "That request must bind the exact hosted provider, model, and version.",
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
            Limitations: ["Request packet is not an acceptance receipt."]);
        WriteJson(requestPath, request);
        File.WriteAllText(
            markdownPath,
            "# TP1 exit decision request\n\nNot accepted.\n",
            new UTF8Encoding(false));
        File.WriteAllBytes(
            schemaPath,
            Tp1ExitDecisionSubmissionContract.BuildSchema(
                DecisionRequestId,
                hashes.CeilingEvaluationSha256,
                candidate));
        var run = new Tp1ExitDecisionRequestRunArtifact(
            Tp1ExitDecisionRequestCli.FixedStatus,
            DecisionRequestId,
            EvaluationId,
            Tp1LocalTeacherCeilingCli.FixedPolicyId,
            Hash(File.ReadAllBytes(requestPath)),
            Hash(File.ReadAllBytes(markdownPath)),
            Hash(File.ReadAllBytes(schemaPath)),
            hashes.CeilingEvaluationSha256,
            hashes.CeilingMarkdownSha256,
            hashes.CeilingRunSha256,
            Tp1ExitDecisionRequestCli.ArtifactLeafNames,
            Hash(JsonSerializer.SerializeToUtf8Bytes(
                Tp1ExitDecisionRequestCli.ArtifactLeafNames,
                SnakeJson)),
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
        WriteJson(runPath, run);
        return new Fixture(
            Path.GetFullPath(root),
            Path.GetFullPath(requestDirectory),
            requestPath,
            markdownPath,
            schemaPath,
            runPath,
            Path.GetFullPath(submissionDirectory),
            submissionPath,
            Path.GetFullPath(outputRoot),
            Path.Combine(outputRoot, ValidationId));
    }

    private static Tp1ExitHumanDecisionSubmissionArtifact Submission(
        bool candidate,
        bool ordering,
        bool authenticity,
        bool numeric,
        bool ceiling,
        bool independent,
        string decision)
        => new(
            DecisionRequestId,
            EvidenceRef,
            candidate,
            AuthorityRef,
            "2026-08-07T10:11:12Z",
            ordering,
            authenticity,
            numeric,
            ceiling,
            independent,
            LubaRef,
            "2026-08-07T10:12:13.123Z",
            decision,
            [ReasonRef]);

    private static void MutateSubmission(string path, string mutation)
    {
        var bytes = File.ReadAllBytes(path);
        var node = JsonNode.Parse(bytes)!.AsObject();
        switch (mutation)
        {
            case "unknown": node["unexpected"] = true; break;
            case "missing": node.Remove("reason_refs"); break;
            case "wrong-type": node["numeric_evidence_verified"] = "true"; break;
            case "request-const": node["decision_request_id"] = "wrong-request"; break;
            case "candidate-const": node["source_local_teacher_ceiling_candidate"] = false; break;
            case "bad-ref": node["luba_authority_ref"] = "sha256:ABC"; break;
            case "bad-utc": node["luba_decided_at_utc"] = "2026-08-07T10:12:13+00:00"; break;
            case "empty-reasons": node["reason_refs"] = new JsonArray(); break;
            case "duplicate-reasons": node["reason_refs"] = new JsonArray(ReasonRef, ReasonRef); break;
            case "forbidden-tuple": node["independent_acceptance"] = false; break;
            case "tp2-field": node["tp2_authorized"] = true; break;
            case "duplicate":
                var text = Encoding.UTF8.GetString(bytes);
                File.WriteAllText(
                    path,
                    "{\"decision_request_id\":\"duplicate\"," + text[1..],
                    new UTF8Encoding(false));
                return;
            default: throw new ArgumentOutOfRangeException(nameof(mutation));
        }
        File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(node, SnakeJson));
    }

    private static void TamperRequestPacket(Fixture fixture, string mutation)
    {
        if (mutation == "request")
        {
            File.AppendAllText(fixture.RequestPath, " ", new UTF8Encoding(false));
            return;
        }
        if (mutation == "request-roster")
        {
            File.WriteAllText(
                Path.Combine(fixture.RequestDirectory, "unexpected.json"),
                "{}",
                new UTF8Encoding(false));
            return;
        }
        if (mutation == "markdown")
        {
            File.AppendAllText(fixture.RequestMarkdownPath, "changed", new UTF8Encoding(false));
            return;
        }
        if (mutation == "schema")
        {
            File.AppendAllText(fixture.SchemaPath, " ", new UTF8Encoding(false));
            return;
        }
        if (mutation == "run")
        {
            File.AppendAllText(fixture.RequestRunPath, " ", new UTF8Encoding(false));
            return;
        }
        RewriteRequestRun(fixture, run => mutation switch
        {
            "run-hash" => run with { DecisionRequestSha256 = RefFor(50) },
            "roster" => run with
            {
                PublishedArtifactLeafNames = [Tp1ExitDecisionRequestCli.DecisionRequestLeaf],
            },
            "posture" => run with { Passed = true },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        });
    }

    private static void RewriteRequestRun(
        Fixture fixture,
        Func<Tp1ExitDecisionRequestRunArtifact, Tp1ExitDecisionRequestRunArtifact> mutate)
    {
        var run = JsonSerializer.Deserialize<Tp1ExitDecisionRequestRunArtifact>(
            File.ReadAllBytes(fixture.RequestRunPath),
            SnakeJson)!;
        WriteJson(fixture.RequestRunPath, mutate(run));
    }

    private static void RewriteRequestAndRehashRun(
        Fixture fixture,
        Func<Tp1ExitDecisionRequestArtifact, Tp1ExitDecisionRequestArtifact> mutate)
    {
        var request = JsonSerializer.Deserialize<Tp1ExitDecisionRequestArtifact>(
            File.ReadAllBytes(fixture.RequestPath),
            SnakeJson)!;
        WriteJson(fixture.RequestPath, mutate(request));
        RewriteRequestRun(
            fixture,
            run => run with
            {
                DecisionRequestSha256 = Hash(File.ReadAllBytes(fixture.RequestPath)),
            });
    }

    private static async Task<CliResult> RunAsync(
        Fixture fixture,
        string? outputRoot = null,
        bool rejectOutputUnderProjects = false,
        Func<string, bool, OperationResult<string>>? resolvePhysicalPath = null,
        CancellationToken cancellationToken = default,
        Func<string, string, IReadOnlyCollection<string>,
            IEvaluationPublicationBoundary>? boundaryFactory = null,
        string? requestDirectory = null,
        string? submissionDirectory = null,
        Func<string, string, IReadOnlyCollection<string>,
            OperationResult<IEvaluationPublicationBoundary>>? createPublication = null)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var dependencies = new Tp1ExitDecisionSubmissionValidationDependencies(
            createPublication: createPublication ?? ((root, id, leaves) =>
                OperationResult<IEvaluationPublicationBoundary>.Ok(
                    boundaryFactory?.Invoke(root, id, leaves)
                    ?? new FakePublicationBoundary(root, id, leaves))),
            rejectOutputUnderProjects: rejectOutputUnderProjects,
            resolvePhysicalPath: resolvePhysicalPath ?? ResolvePhysicalPath);
        var exit = await Tp1ExitDecisionSubmissionValidationCli.RunAsync(
            [
                Tp1ExitDecisionSubmissionValidationCli.ArmFlag,
                "--decision-request-directory", requestDirectory ?? fixture.RequestDirectory,
                "--submission-directory", submissionDirectory ?? fixture.SubmissionDirectory,
                "--output-root", outputRoot ?? fixture.OutputRoot,
                "--validation-id", ValidationId,
            ],
            stdout,
            stderr,
            cancellationToken,
            dependencies);
        return new CliResult(exit, stdout.ToString(), stderr.ToString());
    }

    private static OperationResult<string> ResolvePhysicalPath(string path, bool allowMissing)
    {
        _ = allowMissing;
        return OperationResult<string>.Ok(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)));
    }

    private static void AssertFalseTrustAndExecution(JsonElement value)
    {
        foreach (var property in new[]
        {
            "trusted_human_decision_present", "authority_evidence_verified",
            "independent_reviewer_identity_verified", "luba_identity_verified",
            "submission_signature_verified", "replay_protection_verified",
            "receipt_accepted", "independent_acceptance",
            "local_teacher_ceiling_accepted", "passed", "tp1_exit_gate_evaluated",
            "tp2_authorized", "dpo_train_ready", "teacher_output_used_for_training",
            "training_started", "checkpoint_promoted", "promotion_started",
            "activation_started",
        })
            Assert.False(value.GetProperty(property).GetBoolean());
        Assert.Equal(0, value.GetProperty("dpo_steps_executed").GetInt32());
    }

    private static void AssertError(CliResult result, string code)
    {
        Assert.Equal(1, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        using var error = JsonDocument.Parse(result.Stderr);
        Assert.False(error.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(code, error.RootElement.GetProperty("error_code").GetString());
    }

    private static void AssertNoPublication(Fixture fixture)
    {
        Assert.False(Directory.Exists(fixture.Destination));
        if (Directory.Exists(fixture.OutputRoot))
            Assert.Empty(Directory.EnumerateDirectories(
                fixture.OutputRoot,
                ".*.tp1-exit-submission-validation-tmp-*"));
    }

    private static void AssertCompletePublishedHashDag(
        Fixture fixture,
        Tp1ExitHumanDecisionSubmissionArtifact submission,
        JsonElement validation,
        JsonElement run)
    {
        var expectedCanonicalSubmission = submission with
        {
            IndependentReviewedAtUtc = "2026-08-07T10:11:12.0000000Z",
            LubaDecidedAtUtc = "2026-08-07T10:12:13.1230000Z",
        };
        var expectedCanonicalBytes = JsonSerializer.SerializeToUtf8Bytes(
            expectedCanonicalSubmission,
            SnakeJson);
        var expectedOriginalHash = Hash(File.ReadAllBytes(fixture.SubmissionPath));
        var expectedCanonicalHash = Hash(expectedCanonicalBytes);
        var expectedRequestHashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["decision_request_sha256"] = Hash(File.ReadAllBytes(fixture.RequestPath)),
            ["decision_request_markdown_sha256"] = Hash(
                File.ReadAllBytes(fixture.RequestMarkdownPath)),
            ["human_decision_submission_schema_sha256"] = Hash(
                File.ReadAllBytes(fixture.SchemaPath)),
            ["decision_request_run_sha256"] = Hash(
                File.ReadAllBytes(fixture.RequestRunPath)),
        };

        foreach (var artifact in new[] { validation, run })
        {
            var actualRequestHashes = artifact.GetProperty("source_request_packet_hashes");
            Assert.Equal(
                expectedRequestHashes.Keys.Order(StringComparer.Ordinal),
                actualRequestHashes.EnumerateObject()
                    .Select(static property => property.Name)
                    .Order(StringComparer.Ordinal));
            foreach (var expected in expectedRequestHashes)
                Assert.Equal(
                    expected.Value,
                    actualRequestHashes.GetProperty(expected.Key).GetString());
            Assert.Equal(
                expectedOriginalHash,
                artifact.GetProperty("original_submission_sha256").GetString());
            Assert.Equal(
                expectedCanonicalHash,
                artifact.GetProperty(
                    "canonical_submission_semantic_record_sha256").GetString());
            AssertNoRawCanonicalSubmissionBytes(artifact);
        }

        Assert.Equal(
            validation.GetProperty("original_submission_sha256").GetString(),
            run.GetProperty("original_submission_sha256").GetString());
        Assert.Equal(
            validation.GetProperty(
                "canonical_submission_semantic_record_sha256").GetString(),
            run.GetProperty(
                "canonical_submission_semantic_record_sha256").GetString());
        Assert.Equal(
            Hash(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionSubmissionValidationCli.ValidationLeaf))),
            run.GetProperty("validation_sha256").GetString());
        Assert.Equal(
            Hash(File.ReadAllBytes(Path.Combine(
                fixture.Destination,
                Tp1ExitDecisionSubmissionValidationCli.MarkdownLeaf))),
            run.GetProperty("markdown_sha256").GetString());
        Assert.Equal(
            Tp1ExitDecisionSubmissionValidationCli.ArtifactLeafNames,
            run.GetProperty("published_artifact_leaf_names")
                .EnumerateArray()
                .Select(static item => item.GetString()));
        Assert.Equal(
            Hash(JsonSerializer.SerializeToUtf8Bytes(
                Tp1ExitDecisionSubmissionValidationCli.ArtifactLeafNames,
                SnakeJson)),
            run.GetProperty("published_artifact_roster_sha256").GetString());
        Assert.False(run.TryGetProperty("validation_run_sha256", out _));
    }

    private static void AssertNoRawCanonicalSubmissionBytes(JsonElement artifact)
    {
        foreach (var forbidden in new[]
        {
            "canonical_submission_semantic_record",
            "canonical_submission_semantic_record_bytes",
            "canonical_submission_bytes",
            "raw_canonical_submission",
        })
            Assert.False(artifact.TryGetProperty(forbidden, out _));
        Assert.DoesNotContain(
            artifact.EnumerateObject(),
            static property => property.Name.EndsWith("_bytes", StringComparison.Ordinal));
    }

    private static JsonDocument ReadJson(Fixture fixture, string leaf)
        => JsonDocument.Parse(File.ReadAllBytes(Path.Combine(fixture.Destination, leaf)));

    private static bool ExpectedAllowed(
        bool candidate,
        bool ordering,
        bool authenticity,
        bool numeric,
        bool ceiling,
        bool independent,
        string decision)
    {
        if (decision is not ("TP1_EXIT_ACCEPT" or "TP1_EXIT_REJECT"))
            return false;
        if (!candidate)
            return !ceiling && !independent && decision == "TP1_EXIT_REJECT";
        var all = ordering && authenticity && numeric && ceiling;
        return independent == all && (all || decision == "TP1_EXIT_REJECT");
    }

    private static byte[] BuildIndependentGoldenSchema(
        string requestId,
        string evaluationHash,
        bool candidate)
    {
        static JsonObject Ref() => new()
        {
            ["type"] = "string",
            ["pattern"] = "^sha256:[0-9a-f]{64}$",
        };
        static JsonObject Utc() => new()
        {
            ["type"] = "string",
            ["format"] = "date-time",
            ["pattern"] = "Z$",
        };
        static JsonObject Bool() => new() { ["type"] = "boolean" };
        var names = new[]
        {
            "decision_request_id", "source_ceiling_evaluation_sha256",
            "source_local_teacher_ceiling_candidate",
            "independent_reviewer_authority_ref", "independent_reviewed_at_utc",
            "source_run_ordering_verified", "source_authenticity_verified",
            "numeric_evidence_verified", "local_teacher_ceiling_accepted",
            "independent_acceptance", "luba_authority_ref", "luba_decided_at_utc",
            "tp1_exit_decision", "reason_refs",
        };
        var properties = new JsonObject
        {
            [names[0]] = new JsonObject { ["const"] = requestId },
            [names[1]] = new JsonObject { ["const"] = evaluationHash },
            [names[2]] = new JsonObject { ["const"] = candidate },
            [names[3]] = Ref(), [names[4]] = Utc(), [names[5]] = Bool(),
            [names[6]] = Bool(), [names[7]] = Bool(), [names[8]] = Bool(),
            [names[9]] = Bool(), [names[10]] = Ref(), [names[11]] = Utc(),
            [names[12]] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT"),
            },
            [names[13]] = new JsonObject
            {
                ["type"] = "array", ["minItems"] = 1, ["uniqueItems"] = true,
                ["items"] = Ref(),
            },
        };
        var tuples = new JsonArray();
        for (var mask = 0; mask < 16; mask++)
        foreach (var independent in new[] { false, true })
        foreach (var decision in new[] { "TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT" })
        {
            var ordering = (mask & 1) != 0;
            var authenticity = (mask & 2) != 0;
            var numeric = (mask & 4) != 0;
            var ceiling = (mask & 8) != 0;
            if (!ExpectedAllowed(candidate, ordering, authenticity, numeric,
                ceiling, independent, decision))
                continue;
            tuples.Add(new JsonObject
            {
                ["properties"] = new JsonObject
                {
                    [names[5]] = new JsonObject { ["const"] = ordering },
                    [names[6]] = new JsonObject { ["const"] = authenticity },
                    [names[7]] = new JsonObject { ["const"] = numeric },
                    [names[8]] = new JsonObject { ["const"] = ceiling },
                    [names[9]] = new JsonObject { ["const"] = independent },
                    [names[12]] = new JsonObject { ["const"] = decision },
                },
                ["required"] = new JsonArray(
                    names[5], names[6], names[7], names[8], names[9], names[12]),
            });
        }
        return JsonSerializer.SerializeToUtf8Bytes(new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$id"] = $"urn:xiigen:tp1-exit-decision:{requestId}",
            ["title"] = "TP1 exit human decision submission template",
            ["type"] = "object", ["additionalProperties"] = false,
            ["x_template_only"] = true, ["x_not_authority"] = true,
            ["x_never_parse_as_receipt"] = true,
            ["properties"] = properties,
            ["required"] = new JsonArray(names.Select(static name => (JsonNode?)name).ToArray()),
            ["oneOf"] = tuples,
        }, SnakeJson);
    }

    private static IReadOnlyList<string> Reasons(bool plateau, bool lowBeat, double improvement)
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
        return reasons;
    }

    private static void WriteSubmission(
        Fixture fixture,
        Tp1ExitHumanDecisionSubmissionArtifact submission)
        => WriteJson(fixture.SubmissionPath, submission);

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(value, SnakeJson));

    private static string Hash(byte[] bytes)
        => Tp1EvaluationEvidencePacketIo.HashRef(bytes);

    private static string RefFor(int value)
        => "sha256:" + checked(value + 1).ToString("x64");

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-tp1-submission-validation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record Fixture(
        string Root,
        string RequestDirectory,
        string RequestPath,
        string RequestMarkdownPath,
        string SchemaPath,
        string RequestRunPath,
        string SubmissionDirectory,
        string SubmissionPath,
        string OutputRoot,
        string Destination);

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed class FakePublicationBoundary : IEvaluationPublicationBoundary
    {
        private readonly HashSet<string> _allowed;
        private readonly HashSet<string> _created = new(StringComparer.Ordinal);
        private readonly string _expectedOutputRoot;
        private readonly Action? _onFirstCreate;
        private readonly bool _failWrite;
        private readonly bool _failPublish;
        private readonly bool _failCleanup;
        private readonly bool _failOutputIdentity;
        private bool _published;

        internal FakePublicationBoundary(
            string outputRoot,
            string validationId,
            IReadOnlyCollection<string> leaves,
            Action? onFirstCreate = null,
            bool failWrite = false,
            bool failPublish = false,
            bool failCleanup = false,
            bool failOutputIdentity = false)
        {
            _allowed = new HashSet<string>(leaves, StringComparer.Ordinal);
            _expectedOutputRoot = Path.GetFullPath(outputRoot);
            _onFirstCreate = onFirstCreate;
            _failWrite = failWrite;
            _failPublish = failPublish;
            _failCleanup = failCleanup;
            _failOutputIdentity = failOutputIdentity;
            Directory.CreateDirectory(outputRoot);
            StagingDirectory = Path.Combine(
                outputRoot,
                $".{validationId}.tp1-exit-submission-validation-tmp-{Guid.NewGuid():N}");
            FinalDirectory = Path.Combine(outputRoot, validationId);
            Directory.CreateDirectory(StagingDirectory);
        }

        public string StagingDirectory { get; }
        public string FinalDirectory { get; }
        public string CurrentDirectory => _published ? FinalDirectory : StagingDirectory;

        public void VerifyOutputRootIdentity(string expectedPhysicalPath)
        {
            if (_failOutputIdentity)
                throw new IOException("Injected output identity failure.");
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
            if (_failWrite)
                throw new IOException("Injected write failure.");
            if (!_allowed.Contains(leafName) || !_created.Add(leafName))
                throw new IOException("Unexpected validation artifact.");
            if (_created.Count == 1)
                _onFirstCreate?.Invoke();
            return new FileStream(
                Path.Combine(StagingDirectory, leafName),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
        }

        public void PublishAndVerify()
        {
            if (_failPublish)
                throw new IOException("Injected publish failure.");
            if (!_created.SetEquals(_allowed))
                throw new IOException("Validation artifact roster incomplete.");
            Directory.Move(StagingDirectory, FinalDirectory);
            _published = true;
        }

        public void CleanupFailedPublication()
        {
            if (_failCleanup)
                throw new IOException("Injected cleanup failure.");
            if (Directory.Exists(CurrentDirectory))
                Directory.Delete(CurrentDirectory, recursive: true);
        }

        public void Dispose() { }
    }
}
