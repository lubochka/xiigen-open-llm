// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalArtifactDbUtf8Tests
{
    private const string TrainingIntakeInvalidUtf8ErrorMessage =
        "DesignConversion training intake requests could not be read because the training intake requests file is not valid UTF-8.";

    private const string TrainingIntakeRunInvalidUtf8ErrorMessage =
        "DesignConversion training intake runs could not be read because the training intake runs file is not valid UTF-8.";

    private const string TrainingIntakeLocalPromotionInvalidUtf8ErrorMessage =
        "DesignConversion training intake local promotions could not be read because the training intake local promotions file is not valid UTF-8.";

    [Fact]
    public async Task ListRecordsAsync_InvalidUtf8InConversionResults_ReturnsStableSafeFailureAndPreservesBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-invalid-utf8");
        var recordsPath = Path.Combine(root, "conversion-results.jsonl");
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/conversion-results.jsonl";
        var originalBytes = new List<byte>();
        originalBytes.AddRange(Encoding.ASCII.GetBytes("{\"marker\":\"" + privateMarker + "\"}"));
        originalBytes.AddRange(new byte[] { 0x0A, 0xC3, 0x28, 0x0A });
        originalBytes.AddRange(Encoding.ASCII.GetBytes(privateMarker));
        var expectedBytes = originalBytes.ToArray();

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(recordsPath, expectedBytes);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode, result.ErrorCode);
            Assert.Equal(
                "DesignConversion local artifact DB records could not be read because the records file is not valid UTF-8.",
                result.ErrorMessage);
            Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(expectedBytes, afterBytes);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TrainingIntakeReads_InvalidUtf8InsideJsonString_ReturnStableSafeFailuresAndPreserveBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-training-intake-invalid-utf8");
        var trainingIntakePath = Path.Combine(root, "training-intake-requests.jsonl");
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/training-intake-requests.jsonl#corrupt";
        var intake = BuildTrainingIntakeRecord("utf8-invalid-training-intake", privateMarker);
        var expectedBytes = WithInvalidUtf8ByteInsideJsonString(
            JsonSerializer.Serialize(intake) + "\n",
            privateMarker);
        var replacementDecoded = Encoding.UTF8.GetString(expectedBytes);
        var replacementDecodedRecord = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRecord>(
            replacementDecoded);

        Assert.NotNull(replacementDecodedRecord);
        Assert.Contains("\uFFFD", replacementDecodedRecord!.RequestReason, StringComparison.Ordinal);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(trainingIntakePath, expectedBytes);

            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var listResult = await db.ListTrainingIntakeRequestsAsync();
            var afterListBytes = await File.ReadAllBytesAsync(trainingIntakePath);
            var preflightResult = await db.RunTrainingIntakePreflightAsync(intake.IntakeId);
            var afterPreflightBytes = await File.ReadAllBytesAsync(trainingIntakePath);

            AssertTrainingIntakeInvalidUtf8Failure(listResult, root, trainingIntakePath);
            AssertTrainingIntakeInvalidUtf8Failure(preflightResult, root, trainingIntakePath);
            Assert.Equal(expectedBytes, afterListBytes);
            Assert.Equal(expectedBytes, afterPreflightBytes);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAsync_InvalidUtf8InsideReplayRunJsonString_ReturnsStableSafeFailureAndPreservesBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-training-intake-run-invalid-utf8");
        var trainingIntakePath = Path.Combine(root, "training-intake-requests.jsonl");
        var trainingIntakeRunPath = Path.Combine(root, "training-intake-runs.jsonl");
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/training-intake-runs.jsonl#corrupt";
        var intake = BuildTrainingIntakeRecord(
            "utf8-invalid-training-intake-run-replay",
            "valid zero-candidate training intake run replay");
        var expectedIntakeBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(intake) + Environment.NewLine);
        var expectedRunBytes = WithInvalidUtf8ByteInsideJsonString(
            BuildMinimalTrainingIntakeRunJson(intake.IntakeId, privateMarker) + "\n",
            privateMarker);
        var replacementDecoded = Encoding.UTF8.GetString(expectedRunBytes);
        var replacementDecodedRecord = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRunResult>(
            replacementDecoded);

        Assert.NotNull(replacementDecodedRecord);
        Assert.Contains("\uFFFD", Assert.Single(replacementDecodedRecord!.Warnings), StringComparison.Ordinal);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(trainingIntakePath, expectedIntakeBytes);
            await File.WriteAllBytesAsync(trainingIntakeRunPath, expectedRunBytes);

            var result = await new JsonlDesignConversionLocalArtifactDb(root).RunTrainingIntakeAsync(
                intake.IntakeId,
                admissionBoundaryMetadata: new Dictionary<string, string>
                {
                    ["training_intake_tenant_id_ref"] = "tenant://utf8-test",
                    ["training_intake_idempotency_key_ref"] = "idempotency://utf8-test/run-corrupt"
                });
            var afterIntakeBytes = await File.ReadAllBytesAsync(trainingIntakePath);
            var afterRunBytes = await File.ReadAllBytesAsync(trainingIntakeRunPath);
            var rootFiles = Directory.GetFiles(root);
            var rootDirectories = Directory.GetDirectories(root);
            var runMutationLockPath = trainingIntakeRunPath + ".mutation.lock";

            AssertTrainingIntakeRunInvalidUtf8Failure(result, root, trainingIntakeRunPath);
            Assert.Null(result.Value);
            Assert.Equal(expectedIntakeBytes, afterIntakeBytes);
            Assert.Equal(expectedRunBytes, afterRunBytes);
            Assert.Equal(3, rootFiles.Length);
            Assert.Contains(trainingIntakePath, rootFiles);
            Assert.Contains(trainingIntakeRunPath, rootFiles);
            Assert.Contains(runMutationLockPath, rootFiles);
            Assert.Equal(0, new FileInfo(runMutationLockPath).Length);
            Assert.Empty(rootDirectories);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_InvalidUtf8InsideJsonString_ReturnsStableSafeFailureAndPreservesBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-local-promotion-invalid-utf8");
        var promotionPath = Path.Combine(root, "training-intake-local-promotions.jsonl");
        const string privateMarker = "PRIVATE_MARKER_C:/Users/private-user/private/training-intake-local-promotions.jsonl#corrupt";
        var jsonLine = BuildMinimalTrainingIntakeLocalPromotionJson(privateMarker);
        var expectedBytes = WithInvalidUtf8ByteInsideJsonString(jsonLine + "\n", privateMarker);
        var replacementDecoded = Encoding.UTF8.GetString(expectedBytes);
        var replacementDecodedRecord = JsonSerializer.Deserialize<DesignConversionTrainingIntakeLocalPromotionResult>(
            replacementDecoded);

        Assert.NotNull(replacementDecodedRecord);
        Assert.Contains("\uFFFD", Assert.Single(replacementDecodedRecord!.Warnings), StringComparison.Ordinal);

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(promotionPath, expectedBytes);

            var result = await new JsonlDesignConversionLocalArtifactDb(root)
                .RollbackTrainingIntakeLocalPromotionAsync(
                    "artifact://utf8-test/promoted-local-model",
                    "user://utf8-test",
                    "rollback with valid arguments proves promotion reader UTF-8 failure");
            var afterBytes = await File.ReadAllBytesAsync(promotionPath);
            var afterLines = await File.ReadAllLinesAsync(promotionPath);
            var rootFiles = Directory.GetFiles(root);
            var promotionMutationLockPath = promotionPath + ".mutation.lock";

            AssertTrainingIntakeLocalPromotionInvalidUtf8Failure(result, root, promotionPath);
            Assert.Equal(expectedBytes, afterBytes);
            Assert.Single(afterLines);
            Assert.Equal(2, rootFiles.Length);
            Assert.Contains(promotionPath, rootFiles);
            Assert.Contains(promotionMutationLockPath, rootFiles);
            Assert.Equal(0, new FileInfo(promotionMutationLockPath).Length);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ListRecordsAsync_ValidUtf8ConversionResults_ReturnsStoredRecords()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local-artifact-db-valid-utf8");

        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(root);
            var append = await db.AppendRecordAsync(BuildRecord());
            var listed = await db.ListRecordsAsync();

            Assert.True(append.IsSuccess, append.ErrorMessage);
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            var record = Assert.Single(listed.Value!);
            Assert.Equal("utf8-valid-record", record.RecordId);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, record.DatabaseKind);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static DesignConversionLocalArtifactDbRecord BuildRecord()
        => new(
            RecordId: "utf8-valid-record",
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: "utf8-valid-job",
            RequestId: "utf8-valid-request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "artifact://utf8-valid/source-manifest"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: "utf8-valid-result",
                HtmlRef: "artifact://utf8-valid/html",
                HtmlSha256: string.Empty,
                CssRef: "artifact://utf8-valid/css",
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: "utf8-valid-render",
                    Kind: "browser_screenshot",
                    UriRef: "artifact://utf8-valid/render.png",
                    Sha256: "sha256:utf8-valid-render",
                    ByteLength: 10,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0m,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker],
                Metadata: new Dictionary<string, string>()),
            DpoCandidates: [],
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                JsonlDesignConversionLocalArtifactDb.ConversionResultMarker
            ],
            Metadata: new Dictionary<string, string>());

    private static DesignConversionTrainingIntakeRecord BuildTrainingIntakeRecord(
        string intakeId,
        string requestReason)
        => new(
            IntakeId: intakeId,
            IntakeRef: $"artifact://utf8-test/{intakeId}",
            SourceHandoffArtifactId: "utf8-test-handoff",
            SourceHandoffArtifactRef: "artifact://utf8-test/handoff",
            ApprovedCandidateCount: 0,
            AcceptedCandidateCount: 0,
            CandidateRefs: [],
            RequestedAtUtc: DateTimeOffset.UnixEpoch,
            RequestedByRef: "user://utf8-test",
            RequestReason: requestReason,
            TrainingIntakeOnly: true,
            TrainingNotStarted: true,
            NotTrained: true,
            WeightsNotProduced: true,
            CheckpointNotMutated: true,
            ModelUploadNotReady: true,
            ProductNotReady: true,
            HeldOutNotProven: true,
            SourceDisjointNotProven: true,
            Warnings: [],
            BoundaryMetadata: new Dictionary<string, string>
            {
                ["training_intake_only"] = bool.TrueString,
                ["not_trained"] = bool.TrueString,
                ["weights_not_produced"] = bool.TrueString,
                ["model_upload_not_ready"] = bool.TrueString,
                ["product_not_ready"] = bool.TrueString
            });

    private static string BuildMinimalTrainingIntakeLocalPromotionJson(string warning)
        => JsonSerializer.Serialize(new
        {
            TrainingRun = new { },
            LocalPromotionReady = false,
            ExternalCloudUploadAttempted = false,
            PromotionOutputDirectory = "artifact://utf8-test/local-promotion",
            PromotedCheckpointPath = "artifact://utf8-test/checkpoint",
            Publication = (object?)null,
            PromotedLocalModel = (object?)null,
            EvidenceRefs = Array.Empty<string>(),
            Warnings = new[] { warning },
            BlockReasons = Array.Empty<string>(),
            BoundaryMetadata = new Dictionary<string, string>()
        });

    private static string BuildMinimalTrainingIntakeRunJson(string intakeId, string warning)
        => JsonSerializer.Serialize(new
        {
            RunId = "training-intake-run-utf8-test",
            TrainingIntakeId = intakeId,
            PreflightId = "training-intake-preflight-utf8-test",
            AcceptedCandidateCount = 0,
            ResolvedTrainingRecordCount = 0,
            MayStartTraining = false,
            SourceManifestStatus = "missing",
            SourceDisjointStatus = "missing",
            TrainingRunnerInvoked = false,
            LocalLifecycleHookAttempted = false,
            TrainingStarted = false,
            CheckpointMutated = false,
            CheckpointCandidateCreated = false,
            WeightsProduced = false,
            ModelUploadReady = false,
            TrainingExecutionBoundary = "blocked_before_runner_by_training_intake_preflight",
            TrainingExecutionStatus = "not_started",
            RunnerBlockedReason = "no_accepted_candidates",
            CheckpointStatus = "not_created",
            CheckpointMutationStatus = "not_mutated",
            WeightProofStatus = "weights_not_produced",
            MeasurementGateStatus = "not_evaluated",
            PromotionGateStatus = "not_evaluated",
            UploadReadinessStatus = "not_ready",
            PromotionBlockedReasons = Array.Empty<string>(),
            RunnerEvidence = (object?)null,
            Warnings = new[] { warning },
            BlockReasons = new[] { "no_accepted_candidates" },
            BoundaryMetadata = new Dictionary<string, string>()
        });

    private static byte[] WithInvalidUtf8ByteInsideJsonString(
        string jsonLine,
        string marker)
    {
        var bytes = Encoding.UTF8.GetBytes(jsonLine);
        var markerBytes = Encoding.UTF8.GetBytes(marker);
        var markerIndex = bytes.AsSpan().IndexOf(markerBytes);
        Assert.True(markerIndex >= 0, "The test JSON must contain the marker before corruption.");

        bytes[markerIndex + markerBytes.Length - 1] = 0xC3;
        return bytes;
    }

    private static void AssertTrainingIntakeInvalidUtf8Failure<T>(
        OperationResult<T> result,
        string root,
        string trainingIntakePath)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode, result.ErrorCode);
        Assert.Equal(TrainingIntakeInvalidUtf8ErrorMessage, result.ErrorMessage);
        Assert.DoesNotContain("PRIVATE_MARKER", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(trainingIntakePath, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("training-intake-requests.jsonl", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\uFFFD", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static void AssertTrainingIntakeRunInvalidUtf8Failure<T>(
        OperationResult<T> result,
        string root,
        string runPath)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode, result.ErrorCode);
        Assert.Equal(TrainingIntakeRunInvalidUtf8ErrorMessage, result.ErrorMessage);
        Assert.DoesNotContain("PRIVATE_MARKER", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(runPath, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("training-intake-runs.jsonl", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\uFFFD", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static void AssertTrainingIntakeLocalPromotionInvalidUtf8Failure<T>(
        OperationResult<T> result,
        string root,
        string promotionPath)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.InvalidUtf8ErrorCode, result.ErrorCode);
        Assert.Equal(TrainingIntakeLocalPromotionInvalidUtf8ErrorMessage, result.ErrorMessage);
        Assert.DoesNotContain("PRIVATE_MARKER", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(promotionPath, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("training-intake-local-promotions.jsonl", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\uFFFD", result.ErrorMessage, StringComparison.Ordinal);
    }
}
