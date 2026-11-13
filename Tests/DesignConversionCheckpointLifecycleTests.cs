// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionCheckpointLifecycleTests
{
    private const string CheckpointShapeInvalidCode = "CHECKPOINT_SHAPE_INVALID";
    private const string CheckpointRuntimeUnitIdentityMismatchCode = "CHECKPOINT_RUNTIME_UNIT_IDENTITY_MISMATCH";
    private const string CheckpointNumericValuesInvalidCode = "CHECKPOINT_NUMERIC_VALUES_INVALID";
    private const string PrivateBatchIdSentinel = "PRIVATE_CHECKPOINT_BATCH_ID_MUST_NOT_LEAK_7F3A9C";
    private const string PrivateRuntimeUnitIdentitySentinel = "PRIVATE_RUNTIME_UNIT_IDENTITY_MUST_NOT_LEAK_A58D20";
    private const string PrivateInvalidSourceIdentityHashSentinel = "PRIVATE_SOURCE_IDENTITY_HASH_MUST_NOT_LEAK_5C83D1";
    private const string PrivateInvalidNumericValueSentinelText = "42.424";
    private const double PrivateInvalidNumericValueSentinel = 42.4242;

    [Fact]
    public async Task ExportAsync_UnsavedMachineFailsClosed()
    {
        var machine = new DesignConversionMachine();

        var export = await machine.ExportAsync();

        Assert.False(export.IsSuccess);
        Assert.Equal(DesignConversionMachine.ExportCheckpointNotBoundCode, export.ErrorCode);
    }

    [Fact]
    public async Task ImportAsync_WithMetadataOnlyExportFailsClosedWithoutMutatingState()
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var import = await target.ImportAsync(CreateExport("0.1.140", accuracy: 0.99));

        Assert.False(import.IsSuccess);
        Assert.Equal("IMPORT_CHECKPOINT_REF_NOT_ABSOLUTE", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ImportAsync_WithBlankCheckpointRefFailsBeforeLoadingWithoutMutatingState(string? checkpointRef)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.NotEmpty(before.Value!.TrainingHistory);
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(before.Value) > 0);
        var export = CreateExport("0.1.140", accuracy: 0.5) with
        {
            CheckpointRef = checkpointRef!,
            ManifestRef = "manifest://design-conversion-machine/0.1.140/sha256/validation"
        };

        var import = await target.ImportAsync(export);

        Assert.False(import.IsSuccess);
        Assert.Equal("IMPORT_CHECKPOINT_REF_REQUIRED", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value, after.Value!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ImportAsync_WithBlankManifestRefFailsBeforeLoadingWithoutMutatingState(string? manifestRef)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.NotEmpty(before.Value!.TrainingHistory);
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(before.Value) > 0);
        var missingCheckpointPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"design-conversion-import-manifest-validation-{Guid.NewGuid():N}",
            "missing-checkpoint.json"));
        Assert.False(File.Exists(missingCheckpointPath));
        var export = CreateExport("0.1.140", accuracy: 0.5) with
        {
            CheckpointRef = missingCheckpointPath,
            ManifestRef = manifestRef!
        };

        var import = await target.ImportAsync(export);

        Assert.False(import.IsSuccess);
        Assert.Equal("IMPORT_MANIFEST_REF_REQUIRED", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value, after.Value!);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public async Task ImportAsync_WithInvalidAccuracyFailsBeforeLoadingWithoutMutatingState(double accuracy)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.NotEmpty(before.Value!.TrainingHistory);
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(before.Value) > 0);
        var missingCheckpointPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"design-conversion-import-accuracy-validation-{Guid.NewGuid():N}",
            "missing-checkpoint.json"));
        Assert.False(File.Exists(missingCheckpointPath));
        var export = CreateExport("0.1.140", accuracy) with
        {
            CheckpointRef = missingCheckpointPath,
            ManifestRef = "manifest://design-conversion-machine/0.1.140/sha256/validation"
        };

        var import = await target.ImportAsync(export);

        Assert.False(import.IsSuccess);
        Assert.Equal("IMPORT_ACCURACY_INVALID", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value, after.Value!);
    }

    [Fact]
    public async Task SaveExportImport_RoundTripsRealCheckpointStateAndLearnedWeights()
    {
        var source = new DesignConversionMachine();
        var sourceCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.140",
            accuracy: 0.64,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.31)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.69)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("batch-export-import-real-state", accuracyAfter: 0.64)
            ]);
        var sourceImport = source.ImportCheckpoint(sourceCheckpoint);
        Assert.True(sourceImport.IsSuccess, sourceImport.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-export-import-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            Assert.Equal(Path.GetFullPath(path), save.Value!.CheckpointRef);
            var savedSha256 = await ReadFileSha256Async(path);
            Assert.Equal(
                $"manifest://{DesignConversionMachine.MachineId}/{sourceCheckpoint.Version}/sha256/{savedSha256}",
                save.Value.ManifestRef);

            var export = await source.ExportAsync();
            Assert.True(export.IsSuccess, export.ErrorMessage);
            Assert.Equal(Path.GetFullPath(path), export.Value!.CheckpointRef);
            Assert.Equal(save.Value.ManifestRef, export.Value.ManifestRef);

            var target = new DesignConversionMachine();
            var import = await target.ImportAsync(export.Value!);

            Assert.True(import.IsSuccess, import.ErrorMessage);
            var importedCheckpoint = target.ExportCheckpoint();
            Assert.True(importedCheckpoint.IsSuccess, importedCheckpoint.ErrorMessage);
            AssertCheckpointStateEquals(sourceCheckpoint, importedCheckpoint.Value!);
            Assert.Equal(2, DesignConversionMachine.CountRealLearnedWeightArtifacts(importedCheckpoint.Value));
            Assert.Equal(8, DesignConversionMachine.CountRealLearnedWeightObservations(importedCheckpoint.Value));

            var targetExport = await target.ExportAsync();
            Assert.True(targetExport.IsSuccess, targetExport.ErrorMessage);
            Assert.Equal(export.Value!.MachineId, targetExport.Value!.MachineId);
            Assert.Equal(export.Value.DomainTag, targetExport.Value.DomainTag);
            Assert.Equal(export.Value.Version, targetExport.Value.Version);
            Assert.Equal(export.Value.AccuracyAtExport, targetExport.Value.AccuracyAtExport);
            Assert.Equal(export.Value.CheckpointRef, targetExport.Value.CheckpointRef);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExportImportCheckpoint_RoundTripsTrainingHistoryAndRuntimeUnitBoundaries()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-checkpoint"));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var checkpoint = source.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);

        var target = new DesignConversionMachine();
        var import = target.ImportCheckpoint(checkpoint.Value!);

        Assert.True(import.IsSuccess, import.ErrorMessage);
        var roundTrip = target.ExportCheckpoint();
        Assert.True(roundTrip.IsSuccess, roundTrip.ErrorMessage);
        Assert.Equal(checkpoint.Value!.Version, roundTrip.Value!.Version);
        Assert.True(roundTrip.Value.HasTrainingScaffoldHistory);
        Assert.Single(roundTrip.Value.TrainingHistory);
        Assert.Equal("batch-checkpoint", roundTrip.Value.TrainingHistory[0].BatchId);
        Assert.False(roundTrip.Value.CriticUnit.LearnedWeightsPresent);
        Assert.False(roundTrip.Value.ImproverUnit.LearnedWeightsPresent);
        Assert.Contains(DesignConversionMachine.CheckpointLifecycleEvidence, roundTrip.Value.EvidenceRefs);
    }

    [Fact]
    public async Task SaveLoadCheckpoint_RoundTripsThroughJsonFile()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-save-load"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-save-load-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            Assert.True(File.Exists(path));
            Assert.Equal(Path.GetFullPath(path), save.Value!.CheckpointRef);
            AssertNoCheckpointTempFiles(path);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(DesignConversionMachine.MachineId, load.Value!.Checkpoint.MachineId);
            Assert.Equal(DesignConversionMachine.DomainTag, load.Value.Checkpoint.DomainTag);
            Assert.Single(load.Value.Checkpoint.TrainingHistory);

            var loadedExport = await load.Value.Machine.ExportAsync();
            Assert.True(loadedExport.IsSuccess, loadedExport.ErrorMessage);
            Assert.Equal(save.Value.Version, loadedExport.Value!.Version);
            Assert.Equal(Path.GetFullPath(path), loadedExport.Value.CheckpointRef);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_LegacyJsonWithoutDpoPolicyCheckpointRefStillLoads()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-legacy-without-dpo-policy-ref"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-legacy-dpo-ref-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var legacyJson = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            legacyJson.Remove("dpoPolicyCheckpointRef");
            await File.WriteAllTextAsync(
                path,
                legacyJson.ToJsonString(DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Null(load.Value!.Checkpoint.DpoPolicyCheckpointRef);
            Assert.Single(load.Value.Checkpoint.TrainingHistory);
            Assert.Contains(DesignConversionMachine.CheckpointLifecycleEvidence, load.Value.Checkpoint.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_InjectedV2DpoPolicyRefRoundTripsAndLegacyV1StillLoads()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"design-conversion-dpo-policy-ref-versions-{Guid.NewGuid():N}");
        var v2Path = Path.Combine(root, "v2.json");
        var v1Path = Path.Combine(root, "v1.json");
        Directory.CreateDirectory(root);

        try
        {
            var v2Checkpoint = CreateCheckpointWithDpoPolicyRef(CreateValidV2DpoPolicyCheckpointRef());
            await File.WriteAllTextAsync(
                v2Path,
                JsonSerializer.Serialize(v2Checkpoint, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var v2Load = await DesignConversionMachine.LoadCheckpointAsync(v2Path);

            Assert.True(v2Load.IsSuccess, v2Load.ErrorMessage);
            var v2Context = Assert.IsType<DesignConversionDpoPolicyCheckpointTrainingContext>(
                v2Load.Value!.Checkpoint.DpoPolicyCheckpointRef?.TrainingContext);
            Assert.Equal(2, v2Context.SchemaVersion);
            Assert.Equal("test_in_memory", v2Context.ProviderKind);
            Assert.Equal("test-org/non-default-policy", v2Context.BaseModelId);
            Assert.NotEqual(v2Context.BaseCheckpointSha256, v2Context.InitialWeightSha256);

            var legacyCheckpoint = CreateCheckpointWithDpoPolicyRef(CreateValidDpoPolicyCheckpointRef());
            var legacyJson = JsonNode.Parse(
                JsonSerializer.Serialize(legacyCheckpoint, DesignConversionCheckpointJsonOptions.Standard))!.AsObject();
            var legacyContext = legacyJson["dpoPolicyCheckpointRef"]!["trainingContext"]!.AsObject();
            foreach (var property in new[]
                     {
                         "schemaVersion", "ropeTheta", "rmsNormEps", "providerKind", "baseModelId",
                         "baseRevision", "baseCheckpointSha256", "configSha256", "initialWeightSha256"
                     })
            {
                legacyContext.Remove(property);
            }

            await File.WriteAllTextAsync(
                v1Path,
                legacyJson.ToJsonString(DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var v1Load = await DesignConversionMachine.LoadCheckpointAsync(v1Path);

            Assert.True(v1Load.IsSuccess, v1Load.ErrorMessage);
            Assert.Contains(
                v1Load.Value!.Checkpoint.DpoPolicyCheckpointRef!.TrainingContext.SchemaVersion,
                new[] { 0, 1 });
            Assert.Equal(
                "DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig",
                v1Load.Value.Checkpoint.DpoPolicyCheckpointRef.TrainingContext.ConfigKind);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("provider-kind")]
    [InlineData("configuration-kind")]
    [InlineData("base-model-id")]
    [InlineData("base-revision")]
    [InlineData("base-checkpoint-sha")]
    [InlineData("config-sha")]
    [InlineData("initial-weight-sha")]
    [InlineData("vocab")]
    [InlineData("context")]
    [InlineData("hidden")]
    [InlineData("layers")]
    [InlineData("heads")]
    [InlineData("kv-heads")]
    [InlineData("ffn")]
    [InlineData("rope")]
    [InlineData("rms-eps")]
    [InlineData("rope-nan")]
    [InlineData("rope-infinity")]
    [InlineData("rope-zero")]
    [InlineData("rms-eps-nan")]
    [InlineData("rms-eps-infinity")]
    [InlineData("rms-eps-zero")]
    [InlineData("hidden-head-invalid-relation")]
    [InlineData("head-kv-invalid-relation")]
    public void ImportCheckpoint_InjectedV2IdentityHashArchitectureAndSchemaTamperingFails(
        string tamperCase)
    {
        var valid = CreateValidV2DpoPolicyCheckpointRef();
        var context = valid.TrainingContext;
        var tamperedRef = tamperCase switch
        {
            "schema" => valid with { TrainingContext = context with { SchemaVersion = 3 } },
            "provider-kind" => valid with { TrainingContext = context with { ProviderKind = "other_provider" } },
            "configuration-kind" => valid with { TrainingContext = context with { ConfigKind = "other.configuration.v2" } },
            "base-model-id" => valid with { TrainingContext = context with { BaseModelId = "other-org/non-default-policy" } },
            "base-revision" => valid with { TrainingContext = context with { BaseRevision = "revision-v2" } },
            "base-checkpoint-sha" => valid with { TrainingContext = context with { BaseCheckpointSha256 = new string('d', 64) } },
            "config-sha" => valid with { TrainingContext = context with { ConfigSha256 = new string('d', 64) } },
            "initial-weight-sha" => valid with { TrainingContext = context with { InitialWeightSha256 = new string('d', 64) } },
            "vocab" => valid with { TrainingContext = context with { VocabSize = context.VocabSize + 1 } },
            "context" => valid with { TrainingContext = context with { ContextLength = context.ContextLength + 1 } },
            "hidden" => valid with { TrainingContext = context with { HiddenDim = 15, NumHeads = 3 } },
            "layers" => valid with { TrainingContext = context with { NumLayers = context.NumLayers + 1 } },
            "heads" => valid with { TrainingContext = context with { NumHeads = 2 } },
            "kv-heads" => valid with { TrainingContext = context with { NumKvHeads = 3 } },
            "ffn" => valid with { TrainingContext = context with { FfnHiddenDim = context.FfnHiddenDim + 1 } },
            "rope" => valid with { TrainingContext = context with { RopeTheta = context.RopeTheta + 1 } },
            "rms-eps" => valid with { TrainingContext = context with { RmsNormEps = context.RmsNormEps * 2 } },
            "rope-nan" => WithRecomputedV2ConfigEvidence(valid, context with { RopeTheta = float.NaN }),
            "rope-infinity" => WithRecomputedV2ConfigEvidence(valid, context with { RopeTheta = float.PositiveInfinity }),
            "rope-zero" => WithRecomputedV2ConfigEvidence(valid, context with { RopeTheta = 0f }),
            "rms-eps-nan" => WithRecomputedV2ConfigEvidence(valid, context with { RmsNormEps = float.NaN }),
            "rms-eps-infinity" => WithRecomputedV2ConfigEvidence(valid, context with { RmsNormEps = float.PositiveInfinity }),
            "rms-eps-zero" => WithRecomputedV2ConfigEvidence(valid, context with { RmsNormEps = 0f }),
            "hidden-head-invalid-relation" => WithRecomputedV2ConfigEvidence(
                valid,
                context with { HiddenDim = 14, NumHeads = 3 }),
            "head-kv-invalid-relation" => WithRecomputedV2ConfigEvidence(
                valid,
                context with { NumHeads = 3, NumKvHeads = 2 }),
            _ => throw new ArgumentOutOfRangeException(nameof(tamperCase), tamperCase, null)
        };
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var import = target.ImportCheckpoint(CreateCheckpointWithDpoPolicyRef(
            tamperedRef));

        Assert.False(import.IsSuccess);
        Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, import.ErrorCode);
        Assert.Equal("Checkpoint DPO policy reference is invalid.", import.ErrorMessage);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
        Assert.Null(after.Value!.DpoPolicyCheckpointRef);
    }

    [Theory]
    [InlineData("training-context-null")]
    [InlineData("evidence-refs-null")]
    [InlineData("artifact-kind-blank")]
    [InlineData("source-system-wrong")]
    [InlineData("source-mechanism-wrong")]
    [InlineData("runtime-bridge-kind-wrong")]
    [InlineData("config-kind-wrong")]
    [InlineData("filename-path")]
    [InlineData("filename-format")]
    [InlineData("filename-step-mismatch")]
    [InlineData("sha-short")]
    [InlineData("sha-non-hex")]
    [InlineData("fresh-load-false")]
    [InlineData("converted-false")]
    [InlineData("source-count-zero")]
    [InlineData("bridge-count-zero")]
    [InlineData("bridge-count-exceeds-source")]
    [InlineData("dpo-step-zero")]
    [InlineData("legacy-commitment-present")]
    [InlineData("vocab-size-zero")]
    [InlineData("context-length-zero")]
    [InlineData("hidden-dim-zero")]
    [InlineData("num-layers-zero")]
    [InlineData("num-heads-zero")]
    [InlineData("num-kv-heads-zero")]
    [InlineData("ffn-hidden-dim-zero")]
    [InlineData("ffn-hidden-dim-smaller-than-hidden")]
    [InlineData("hidden-head-relation")]
    [InlineData("head-kv-relation")]
    [InlineData("raw-payload-persisted")]
    [InlineData("absolute-path-persisted")]
    [InlineData("evidence-missing-typed")]
    [InlineData("evidence-missing-no-path")]
    [InlineData("evidence-missing-no-raw")]
    [InlineData("evidence-sha-mismatch")]
    [InlineData("evidence-file-mismatch")]
    [InlineData("evidence-source-count-mismatch")]
    [InlineData("evidence-preference-count-mismatch")]
    [InlineData("evidence-step-count-mismatch")]
    public void ImportCheckpoint_InvalidDpoPolicyCheckpointRefFailsBeforeStateMutation(
        string invalidCase)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var invalidCheckpoint = CreateCheckpointWithDpoPolicyRef(
            CreateInvalidDpoPolicyCheckpointRef(invalidCase));

        var import = target.ImportCheckpoint(invalidCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, import.ErrorCode);
        Assert.Equal("Checkpoint DPO policy reference is invalid.", import.ErrorMessage);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
        Assert.Null(after.Value!.DpoPolicyCheckpointRef);
    }

    [Theory]
    [InlineData("training-context-null")]
    [InlineData("sha-short")]
    [InlineData("filename-path")]
    [InlineData("bridge-count-exceeds-source")]
    [InlineData("fresh-load-false")]
    [InlineData("converted-false")]
    public async Task LoadCheckpointAsync_SerializedInvalidDpoPolicyCheckpointRefUsesStableCode(
        string invalidCase)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"design-conversion-invalid-dpo-policy-ref-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        Directory.CreateDirectory(root);

        try
        {
            var checkpoint = CreateCheckpointWithDpoPolicyRef(
                CreateInvalidDpoPolicyCheckpointRef(invalidCase));
            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, load.ErrorCode);
            Assert.Equal("Checkpoint DPO policy reference is invalid.", load.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DpoPolicyPayloadValidSiblingRoundTripsOnlyWhenBytesShaAndLengthMatch()
    {
        var payload = Enumerable.Range(1, 64).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-valid-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var payloadFileName = "dpo_policy_step_000001.bin";

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(Path.Combine(root, payloadFileName), payload);
            var checkpoint = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: payloadFileName,
                    sha256: validSha,
                    rawPayloadPersisted: true,
                    byteLength: payload.Length));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.True(load.IsSuccess, load.ErrorMessage);
            var loadedRef = load.Value!.Checkpoint.DpoPolicyCheckpointRef;
            Assert.NotNull(loadedRef);
            Assert.Equal(2, loadedRef.TrainingContext.SchemaVersion);
            Assert.True(loadedRef.TrainingContext.RawPayloadPersisted);
            Assert.Equal(payloadFileName, loadedRef.PolicyCheckpointFileName);
            Assert.Equal(validSha, loadedRef.Sha256);
            Assert.Equal(payload.Length, loadedRef.PolicyCheckpointByteLength);
            var machineExport = load.Value.Machine.ExportCheckpoint();
            Assert.True(machineExport.IsSuccess, machineExport.ErrorMessage);
            Assert.Equal(validSha, machineExport.Value!.DpoPolicyCheckpointRef!.Sha256);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportCheckpoint_SnapshotsDpoPolicyCheckpointEvidenceRefsAtImportAndExportBoundaries()
    {
        var payload = Enumerable.Range(7, 32).Select(value => (byte)value).ToArray();
        var sha256 = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        const string payloadFileName = "dpo_policy_step_000001.bin";
        var baseRef = CreateDpoPolicyCheckpointRef(
            fileName: payloadFileName,
            sha256: sha256,
            rawPayloadPersisted: true,
            byteLength: payload.Length);
        var importedEvidenceRefs = baseRef.EvidenceRefs.ToList();
        var expectedEvidenceRefs = importedEvidenceRefs.ToArray();
        var checkpoint = CreateCheckpointWithDpoPolicyRef(baseRef with
        {
            EvidenceRefs = importedEvidenceRefs
        });
        var target = new DesignConversionMachine();

        var import = target.ImportCheckpoint(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        importedEvidenceRefs.Add("caller-mutated-after-import");
        var firstExport = target.ExportCheckpoint();
        Assert.True(firstExport.IsSuccess, firstExport.ErrorMessage);
        var firstExportRef = firstExport.Value!.DpoPolicyCheckpointRef;
        Assert.NotNull(firstExportRef);
        Assert.Equal(expectedEvidenceRefs, firstExportRef.EvidenceRefs.ToArray());

        var mutableExportedEvidenceRefs = Assert.IsAssignableFrom<IList<string>>(firstExportRef.EvidenceRefs);
        mutableExportedEvidenceRefs[0] = "caller-mutated-after-export";

        var secondExport = target.ExportCheckpoint();
        Assert.True(secondExport.IsSuccess, secondExport.ErrorMessage);
        Assert.Equal(
            expectedEvidenceRefs,
            secondExport.Value!.DpoPolicyCheckpointRef!.EvidenceRefs.ToArray());
        Assert.Equal("caller-mutated-after-export", firstExportRef.EvidenceRefs[0]);
    }

    [Fact]
    public async Task LoadCheckpointAsync_DpoPolicyRefWithNonPersistedPayloadFailsClosed()
    {
        var payload = Enumerable.Range(2, 48).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-non-persisted-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var tampered = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: "dpo_policy_step_000001.bin",
                    sha256: validSha,
                    rawPayloadPersisted: false,
                    byteLength: payload.Length));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, load.ErrorCode);
            Assert.Equal("Checkpoint DPO policy reference is invalid.", load.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("absolute")]
    [InlineData("rooted")]
    [InlineData("traversal")]
    [InlineData("invalid-format")]
    public async Task LoadCheckpointAsync_DpoPolicyRefWithUnsafePayloadFileNameFailsClosed(string unsafeFileNameKind)
    {
        var payload = Enumerable.Range(3, 40).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-unsafe-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var outsidePayloadPath = Path.GetFullPath(Path.Combine(root, "..", $"dpo_policy_step_{Guid.NewGuid():N}.bin"));

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(outsidePayloadPath, payload);
            var unsafeFileName = unsafeFileNameKind switch
            {
                "absolute" => outsidePayloadPath,
                "rooted" => $"{Path.DirectorySeparatorChar}dpo_policy_step_000001.bin",
                "traversal" => Path.Combine("..", Path.GetFileName(outsidePayloadPath)),
                "invalid-format" => "policy-checkpoint-0001.json",
                _ => throw new ArgumentOutOfRangeException(nameof(unsafeFileNameKind), unsafeFileNameKind, "Unknown unsafe file name kind.")
            };
            var tampered = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: unsafeFileName,
                    sha256: validSha,
                    rawPayloadPersisted: true,
                    byteLength: payload.Length));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, load.ErrorCode);
            Assert.Equal("Checkpoint DPO policy reference is invalid.", load.ErrorMessage);
        }
        finally
        {
            if (File.Exists(outsidePayloadPath))
                File.Delete(outsidePayloadPath);

            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("provider-kind")]
    [InlineData("config-sha")]
    [InlineData("byte-length-zero")]
    public async Task LoadCheckpointAsync_DpoPolicyPayloadInvalidV2MetadataFailsBeforePayloadLookup(string invalidCase)
    {
        var payload = Enumerable.Range(4, 52).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var validRef = CreateDpoPolicyCheckpointRef(
            fileName: "dpo_policy_step_000001.bin",
            sha256: validSha,
            rawPayloadPersisted: true,
            byteLength: payload.Length);
        var tamperedRef = invalidCase switch
        {
            "schema" => validRef with { TrainingContext = validRef.TrainingContext with { SchemaVersion = 3 } },
            "provider-kind" => validRef with { TrainingContext = validRef.TrainingContext with { ProviderKind = "other_provider" } },
            "config-sha" => validRef with { TrainingContext = validRef.TrainingContext with { ConfigSha256 = new string('d', 64) } },
            "byte-length-zero" => validRef with { PolicyCheckpointByteLength = 0 },
            _ => throw new ArgumentOutOfRangeException(nameof(invalidCase), invalidCase, "Unknown invalid payload metadata case.")
        };
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-invalid-metadata-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var tampered = CreateCheckpointWithDpoPolicyRef(tamperedRef);

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointDpoPolicyRefInvalidCode, load.ErrorCode);
            Assert.Equal("Checkpoint DPO policy reference is invalid.", load.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DpoPolicyPayloadMissingSiblingFileFailsClosed()
    {
        var payload = Enumerable.Range(5, 56).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-missing-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var tampered = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: "dpo_policy_step_000001.bin",
                    sha256: validSha,
                    rawPayloadPersisted: true,
                    byteLength: payload.Length));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.DpoPolicyCheckpointPayloadNotFoundCode, load.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DpoPolicyPayloadShaMismatchFailsClosed()
    {
        var payload = Enumerable.Range(6, 60).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-sha-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var payloadFileName = "dpo_policy_step_000001.bin";

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(Path.Combine(root, payloadFileName), payload);
            var tampered = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: payloadFileName,
                    sha256: new string('0', validSha.Length),
                    rawPayloadPersisted: true,
                    byteLength: payload.Length));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.DpoPolicyCheckpointPayloadShaMismatchCode, load.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadCheckpointAsync_DpoPolicyPayloadByteLengthMismatchFailsClosed()
    {
        var payload = Enumerable.Range(7, 44).Select(value => (byte)value).ToArray();
        var validSha = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-policy-payload-length-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var payloadFileName = "dpo_policy_step_000001.bin";

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllBytesAsync(Path.Combine(root, payloadFileName), payload);
            var tampered = CreateCheckpointWithDpoPolicyRef(
                CreateDpoPolicyCheckpointRef(
                    fileName: payloadFileName,
                    sha256: validSha,
                    rawPayloadPersisted: true,
                    byteLength: payload.Length + 1));

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, DesignConversionCheckpointJsonOptions.Standard),
                Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(DesignConversionMachine.DpoPolicyCheckpointPayloadLengthMismatchCode, load.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_WhenBoundCheckpointFileIsDeletedFailsWithStableCode()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-delete-bound-file"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-deleted-export-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            File.Delete(path);

            var export = await source.ExportAsync();

            Assert.False(export.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointFileMissingCode, export.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_WhenCheckpointFileIsMissingPreservesTargetState()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-missing-import-source"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-missing-import-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var export = await source.ExportAsync();
            Assert.True(export.IsSuccess, export.ErrorMessage);
            File.Delete(path);

            var import = await target.ImportAsync(export.Value!);

            Assert.False(import.IsSuccess);
            Assert.Equal("CHECKPOINT_NOT_FOUND", import.ErrorCode);
            var after = target.ExportCheckpoint();
            Assert.True(after.IsSuccess, after.ErrorMessage);
            AssertCheckpointStateEquals(before.Value!, after.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAndImport_WhenCheckpointIsReplacedByValidSameMetadataSnapshotFailWithoutMutatingTarget()
    {
        var source = new DesignConversionMachine();
        var sourceCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.141",
            accuracy: 0.61,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.41)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("batch-tamper-source", accuracyAfter: 0.61)
            ]);
        var sourceImport = source.ImportCheckpoint(sourceCheckpoint);
        Assert.True(sourceImport.IsSuccess, sourceImport.ErrorMessage);
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-tampered-import-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var export = await source.ExportAsync();
            Assert.True(export.IsSuccess, export.ErrorMessage);

            var replacementCheckpoint = CreateCheckpointWithLearnedWeights(
                version: sourceCheckpoint.Version,
                accuracy: sourceCheckpoint.AccuracyAfterLastBatch,
                hasTrainingScaffoldHistory: sourceCheckpoint.HasTrainingScaffoldHistory,
                criticLearnedWeightsPresent: true,
                criticLearnedWeights:
                [
                    ValidLearnedWeight("source_kind:canva", weight: 0.91)
                ],
                trainingHistory: sourceCheckpoint.TrainingHistory);
            var replacementSource = new DesignConversionMachine();
            var replacementImport = replacementSource.ImportCheckpoint(replacementCheckpoint);
            Assert.True(replacementImport.IsSuccess, replacementImport.ErrorMessage);
            var replacementSave = await replacementSource.SaveCheckpointAsync(path);
            Assert.True(replacementSave.IsSuccess, replacementSave.ErrorMessage);
            Assert.Equal(export.Value!.MachineId, replacementSave.Value!.MachineId);
            Assert.Equal(export.Value.DomainTag, replacementSave.Value.DomainTag);
            Assert.Equal(export.Value.Version, replacementSave.Value.Version);
            Assert.Equal(export.Value.AccuracyAtExport, replacementSave.Value.AccuracyAtExport);
            Assert.NotEqual(export.Value.ManifestRef, replacementSave.Value.ManifestRef);

            var postTamperExport = await source.ExportAsync();
            var import = await target.ImportAsync(export.Value!);

            Assert.False(postTamperExport.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointHashMismatchCode, postTamperExport.ErrorCode);
            Assert.False(import.IsSuccess);
            Assert.Equal(DesignConversionMachine.ImportCheckpointMetadataMismatchCode, import.ErrorCode);
            var after = target.ExportCheckpoint();
            Assert.True(after.IsSuccess, after.ErrorMessage);
            AssertCheckpointStateEquals(before.Value!, after.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_AfterFurtherTrainingFailsAsStaleWithoutMutatingState()
    {
        var source = new DesignConversionMachine();
        var firstTrain = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-stale-before-save"));
        Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-stale-export-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var beforeStaleExport = source.ExportCheckpoint();
            Assert.True(beforeStaleExport.IsSuccess, beforeStaleExport.ErrorMessage);

            var secondTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(source, CreateBatch("batch-stale-after-save"));
            Assert.True(secondTrain.IsSuccess, secondTrain.ErrorMessage);

            var export = await source.ExportAsync();

            Assert.False(export.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointVersionStaleCode, export.ErrorCode);
            var afterStaleExport = source.ExportCheckpoint();
            Assert.True(afterStaleExport.IsSuccess, afterStaleExport.ErrorMessage);
            Assert.Equal("0.1.2", afterStaleExport.Value!.Version);
            Assert.Equal(2, afterStaleExport.Value.TrainingHistory.Count);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveCheckpoint_WhenWriterLockIsHeld_FailsClosedWithoutWritingCheckpointOrTemp()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-conflict-no-destination"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-conflict-empty-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            await using var writerLock = HoldCheckpointWriterLock(path);

            var save = await source.SaveCheckpointAsync(path);

            Assert.False(save.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointWriteConflictCode, save.ErrorCode);
            Assert.False(File.Exists(path));
            AssertNoCheckpointTempFiles(path);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveCheckpoint_WhenWriterLockIsHeld_PreservesExistingCheckpointBytes()
    {
        var original = new DesignConversionMachine();
        var originalTrain = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(original, CreateBatch("batch-conflict-original"));
        Assert.True(originalTrain.IsSuccess, originalTrain.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-conflict-existing-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var originalSave = await original.SaveCheckpointAsync(path);
            Assert.True(originalSave.IsSuccess, originalSave.ErrorMessage);
            var originalSha = await ReadFileSha256Async(path);
            var originalBytes = await File.ReadAllBytesAsync(path);

            await using var writerLock = HoldCheckpointWriterLock(path);
            var contender = new DesignConversionMachine();
            var contenderTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(contender, CreateBatch("batch-conflict-contender"));
            Assert.True(contenderTrain.IsSuccess, contenderTrain.ErrorMessage);

            var save = await contender.SaveCheckpointAsync(path);

            Assert.False(save.IsSuccess);
            Assert.Equal(DesignConversionMachine.CheckpointWriteConflictCode, save.ErrorCode);
            Assert.Equal(originalSha, await ReadFileSha256Async(path));
            Assert.Equal(originalBytes, await File.ReadAllBytesAsync(path));
            AssertNoCheckpointTempFiles(path);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            var entry = Assert.Single(load.Value!.Checkpoint.TrainingHistory);
            Assert.Equal("batch-conflict-original", entry.BatchId);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(DesignConversionDeterministicMutationLock.WindowsSharingViolationNativeCode)]
    [InlineData(DesignConversionDeterministicMutationLock.WindowsLockViolationNativeCode)]
    public void DeterministicMutationLock_ContentionClassifierAcceptsWindowsShareAndLockCodes(
        int nativeErrorCode)
    {
        var exception = new IOException(
            "synthetic Windows mutation lock contention",
            nativeErrorCode);

        Assert.True(DesignConversionDeterministicMutationLock.IsContentionForTesting(exception));
    }

    [Fact]
    public void DeterministicMutationLock_LinuxErrno11IsContentionOnlyForFlockAcquirePath()
    {
        var rawIOException = new IOException(
            "raw errno 11 outside mutation-lock acquire",
            DesignConversionDeterministicMutationLock.LinuxWouldBlockNativeCode);
        var flockIOException =
            DesignConversionDeterministicMutationLock.CreateLinuxFlockContentionForTesting();

        Assert.False(DesignConversionDeterministicMutationLock.IsContentionForTesting(rawIOException));
        Assert.True(DesignConversionDeterministicMutationLock.IsContentionForTesting(flockIOException));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(13)]
    [InlineData(999)]
    public void DeterministicMutationLock_ContentionClassifierRejectsUnknownIoFailures(
        int nativeErrorCode)
    {
        var exception = new IOException(
            "synthetic unknown mutation lock failure",
            nativeErrorCode);

        Assert.False(DesignConversionDeterministicMutationLock.IsContentionForTesting(exception));
    }

    [Fact]
    public async Task DeterministicMutationLock_StalePayloadWithoutHeldLeaseIsClearedAndDoesNotBlock()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            $"design-conversion-mutation-lock-stale-{Guid.NewGuid():N}");
        var lockPath = Path.Combine(root, "checkpoint.json.mutation.lock");

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(lockPath, "stale-payload-must-not-own-the-lease");

            await using (var lease = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath))
            {
                Assert.Equal(0, new FileInfo(lockPath).Length);
                var contender = DesignConversionDeterministicMutationLock.TryAcquire(
                    lockPath,
                    "held",
                    "HELD",
                    "FAILED");

                Assert.False(contender.IsSuccess);
                Assert.Equal("HELD", contender.ErrorCode);
            }

            await using var retry = DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath);
            Assert.Equal(0, new FileInfo(lockPath).Length);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalFreshLoadProbe_VerifiesSavedTempCheckpointMetadataOnly()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-fresh-load-probe"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-probe-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var probe = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(source, path);

            Assert.True(probe.IsSuccess, probe.ErrorMessage);
            Assert.True(File.Exists(path));
            Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, probe.Value!.Status);
            Assert.Equal(Path.GetFullPath(path), probe.Value.CheckpointRef);
            Assert.Equal(DesignConversionMachine.MachineId, probe.Value.MachineId);
            Assert.Equal(DesignConversionMachine.DomainTag, probe.Value.DomainTag);
            Assert.Equal(probe.Value.ManifestRef, probe.Value.LoadedManifestRef);
            Assert.Equal(1, probe.Value.TrainingHistoryCount);
            Assert.True(probe.Value.MetadataContinuityVerified);
            Assert.False(probe.Value.CheckpointContainsRealTrainedWeights);
            Assert.False(probe.Value.ContinueTrainingProven);
            Assert.False(probe.Value.MeasurementGateRun);
            Assert.False(probe.Value.PromotionUploadAllowed);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_VERIFIED",
                probe.Value.EvidenceRefs);
            Assert.DoesNotContain("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED",
                probe.Value.EvidenceRefs);
            Assert.Empty(probe.Value.FailureReasons);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalFreshLoadProbe_FailedMetadataContinuityDoesNotEmitVerifiedEvidence()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-fresh-load-metadata-mismatch"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-probe-mismatch-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var probe = await DesignConversionLocalFreshLoadProbe.VerifyCheckpointFileAsync(
                path,
                expectedManifestRef: "design-conversion-machine/intentional-mismatch/manifest.json");

            Assert.True(probe.IsSuccess, probe.ErrorMessage);
            Assert.Equal("failed", probe.Value!.Status);
            Assert.False(probe.Value.MetadataContinuityVerified);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED",
                probe.Value.FailureReasons);
            Assert.DoesNotContain("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_VERIFIED",
                probe.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_FAILED",
                probe.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_PROVE_TRAINED_WEIGHTS",
                probe.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_RUN_MEASUREMENT_GATE",
                probe.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_DOES_NOT_ALLOW_PROMOTION_UPLOAD",
                probe.Value.EvidenceRefs);
            Assert.False(probe.Value.CheckpointContainsRealTrainedWeights);
            Assert.False(probe.Value.PromotionUploadAllowed);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalFreshLoadProbe_FailsHonestlyWhenCheckpointIsMissingOrCorrupt()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-probe-fail-{Guid.NewGuid():N}");
        var missingPath = Path.Combine(root, "missing.json");
        var corruptPath = Path.Combine(root, "corrupt.json");

        try
        {
            var missing = await DesignConversionLocalFreshLoadProbe.VerifyCheckpointFileAsync(missingPath);
            Assert.False(missing.IsSuccess);
            Assert.Equal("CHECKPOINT_NOT_FOUND", missing.ErrorCode);

            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(corruptPath, "{ not valid checkpoint json", Encoding.UTF8);

            var corrupt = await DesignConversionLocalFreshLoadProbe.VerifyCheckpointFileAsync(corruptPath);
            Assert.False(corrupt.IsSuccess);
            Assert.Equal("CHECKPOINT_READ_FAILED", corrupt.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_FailsWhenCheckpointCandidateIsMissingOrCorrupt()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-continue-fail-{Guid.NewGuid():N}");
        var missingPath = Path.Combine(root, "missing.json");
        var corruptPath = Path.Combine(root, "corrupt.json");
        var continuedPath = Path.Combine(root, "continued.json");
        var continuationBatch = CreateContinuationBatch("continue-missing-corrupt", CreateTrainingRecord("continue-record-001"));

        try
        {
            var missing = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                FreshLoadReportFor(missingPath),
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);
            Assert.False(missing.IsSuccess);
            Assert.Equal("CHECKPOINT_NOT_FOUND", missing.ErrorCode);

            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(corruptPath, "{ not valid checkpoint json", Encoding.UTF8);

            var corrupt = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                FreshLoadReportFor(corruptPath),
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);
            Assert.False(corrupt.IsSuccess);
            Assert.Equal("CHECKPOINT_READ_FAILED", corrupt.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_RequiresFreshLoadMetadataContinuityBeforeTraining()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-continue-requires-fresh-load"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-continue-requires-fresh-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var continuedPath = Path.Combine(root, "continued.json");
        var record = CreateTrainingRecord("continue-fresh-required");
        var continuationBatch = CreateContinuationBatch("continue-fresh-required", record);

        try
        {
            var save = await source.SaveCheckpointAsync(path);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var result = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                FreshLoadReportFor(path, metadataContinuityVerified: false),
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_REQUIRED", result.ErrorCode);
            Assert.False(File.Exists(continuedPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_RejectsHeldOutRecordIdentities()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-continue-held-out"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-continue-held-out-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var continuedPath = Path.Combine(root, "continued.json");
        var record = CreateTrainingRecord("continue-held-out-record");
        var continuationBatch = CreateContinuationBatch("continue-held-out", record);

        try
        {
            var fresh = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(source, path);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);

            var result = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                fresh.Value!,
                continuationBatch,
                heldOutRecordIdentityHashes: [ToRecordIdentityHash(record)],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_CONTINUE_TRAINING_HELD_OUT_RECORD_USAGE_DETECTED", result.ErrorCode);
            Assert.False(File.Exists(continuedPath));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_AppendsBatchAndPreservesSourceCheckpoint()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-continue-source"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-continue-append-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var continuedPath = Path.Combine(root, "continued.json");
        var record = CreateTrainingRecord("continue-append-record");
        var continuationBatch = CreateContinuationBatch("continue-append-batch", record);

        try
        {
            var fresh = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(source, path);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            var sourceShaBefore = await ReadFileSha256Async(path);

            var result = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                fresh.Value!,
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, result.Value!.Status);
            Assert.Equal(Path.GetFullPath(path), result.Value.SourceCheckpointRef);
            Assert.Equal(Path.GetFullPath(continuedPath), result.Value.ContinuedCheckpointRef);
            Assert.Equal(sourceShaBefore, result.Value.SourceCheckpointSha256Before);
            Assert.Equal(sourceShaBefore, result.Value.SourceCheckpointSha256After);
            Assert.True(result.Value.SourceCheckpointUnchanged);
            Assert.True(result.Value.ContinuationBatchAppended);
            Assert.True(result.Value.NoReplayBatchIdVerified);
            Assert.Equal(1, result.Value.StartingTrainingHistoryCount);
            Assert.Equal(2, result.Value.ContinuedTrainingHistoryCount);
            Assert.Equal("continue-append-batch", result.Value.ContinueBatchId);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_SOURCE_CHECKPOINT_UNCHANGED",
                result.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_APPENDED_ONE_BATCH",
                result.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_NO_REPLAY_BATCH_ID_VERIFIED",
                result.Value.EvidenceRefs);

            Assert.Equal(sourceShaBefore, await ReadFileSha256Async(path));
            var sourceReload = await DesignConversionMachine.LoadCheckpointAsync(path);
            Assert.True(sourceReload.IsSuccess, sourceReload.ErrorMessage);
            var sourceEntry = Assert.Single(sourceReload.Value!.Checkpoint.TrainingHistory);
            Assert.Equal("batch-continue-source", sourceEntry.BatchId);

            var continuedReload = await DesignConversionMachine.LoadCheckpointAsync(continuedPath);
            Assert.True(continuedReload.IsSuccess, continuedReload.ErrorMessage);
            Assert.Equal(
                ["batch-continue-source", "continue-append-batch"],
                continuedReload.Value!.Checkpoint.TrainingHistory.Select(history => history.BatchId).ToArray());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_RejectsReplayBatchIdBeforeWritingContinuedCheckpoint()
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(source, CreateBatch("batch-replay-source"));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-continue-replay-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");
        var continuedPath = Path.Combine(root, "continued.json");
        var record = CreateTrainingRecord("continue-replay-record");
        var continuationBatch = CreateContinuationBatch("batch-replay-source", record);

        try
        {
            var fresh = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(source, path);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            var sourceShaBefore = await ReadFileSha256Async(path);

            var result = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                fresh.Value!,
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_CONTINUE_TRAINING_REPLAY_BATCH_DETECTED", result.ErrorCode);
            Assert.False(File.Exists(continuedPath));
            Assert.Equal(sourceShaBefore, await ReadFileSha256Async(path));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportCheckpoint_RejectsWrongDomainAndUnsupportedVersion()
    {
        var source = new DesignConversionMachine();
        var checkpoint = source.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);

        var wrongDomain = checkpoint.Value! with { DomainTag = "OtherDomain" };
        var unsupportedVersion = checkpoint.Value! with { Version = "99.0.0" };
        var target = new DesignConversionMachine();

        var wrongDomainResult = target.ImportCheckpoint(wrongDomain);
        var unsupportedVersionResult = target.ImportCheckpoint(unsupportedVersion);

        Assert.False(wrongDomainResult.IsSuccess);
        Assert.Equal("DOMAIN_TAG_MISMATCH", wrongDomainResult.ErrorCode);
        Assert.False(unsupportedVersionResult.IsSuccess);
        Assert.Equal("VERSION_UNSUPPORTED", unsupportedVersionResult.ErrorCode);
    }

    [Theory]
    [InlineData("0.1.0")]
    [InlineData("0.1.116")]
    [InlineData("1.0.0")]
    public async Task ImportAsyncAndImportCheckpoint_AcceptSupportedExactNumericVersions(string version)
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-supported-import-{Guid.NewGuid():N}");
        try
        {
            var checkpointPath = Path.Combine(root, $"checkpoint-{version.Replace('.', '-')}.json");
            var savedExport = await CreateSavedExportAsync(
                checkpointPath,
                version,
                accuracy: 0.25);
            var exportTarget = new DesignConversionMachine();
            var exportImport = await exportTarget.ImportAsync(savedExport);
            Assert.True(exportImport.IsSuccess, exportImport.ErrorMessage);
            var exported = await exportTarget.ExportAsync();
            Assert.True(exported.IsSuccess, exported.ErrorMessage);
            Assert.Equal(version, exported.Value!.Version);
            Assert.Equal(0.25, exported.Value.AccuracyAtExport);
            Assert.Equal(Path.GetFullPath(checkpointPath), exported.Value.CheckpointRef);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        var checkpointTarget = new DesignConversionMachine();
        var checkpoint = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);

        var checkpointImport = checkpointTarget.ImportCheckpoint(checkpoint.Value! with { Version = version });

        Assert.True(checkpointImport.IsSuccess, checkpointImport.ErrorMessage);
        var importedCheckpoint = checkpointTarget.ExportCheckpoint();
        Assert.True(importedCheckpoint.IsSuccess, importedCheckpoint.ErrorMessage);
        Assert.Equal(version, importedCheckpoint.Value!.Version);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1.zzz.qqq")]
    [InlineData("1.0")]
    [InlineData("1.0.0.0")]
    [InlineData("1.-1.0")]
    [InlineData("1.+1.0")]
    [InlineData("2.0.0")]
    [InlineData("1.0.2147483648")]
    public async Task ImportAsync_RejectsMalformedVersionsWithVersionUnsupportedAndPreservesState(string? version)
    {
        var target = new DesignConversionMachine();
        var baselineImport = target.ImportCheckpoint(
            CreateCheckpointWithLearnedWeights(
                version: "0.1.116",
                accuracy: 0.25,
                hasTrainingScaffoldHistory: true,
                criticLearnedWeightsPresent: true,
                criticLearnedWeights:
                [
                    ValidLearnedWeight("source_kind:figma", weight: 0.25)
                ],
                trainingHistory:
                [
                    CreateTrainingHistory("checkpoint-import-version-baseline", accuracyAfter: 0.25)
                ]));
        Assert.True(baselineImport.IsSuccess, baselineImport.ErrorMessage);
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var import = await target.ImportAsync(CreateExport(version, accuracy: 0.99));

        Assert.False(import.IsSuccess);
        Assert.Equal("VERSION_UNSUPPORTED", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1.zzz.qqq")]
    [InlineData("1.0")]
    [InlineData("1.0.0.0")]
    [InlineData("1.-1.0")]
    [InlineData("1.+1.0")]
    [InlineData("2.0.0")]
    [InlineData("1.0.2147483648")]
    public async Task ImportCheckpoint_RejectsMalformedVersionsWithVersionUnsupportedAndPreservesState(string? version)
    {
        var target = new DesignConversionMachine();
        var targetTrain = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(target, CreateBatch("checkpoint-target-baseline"));
        Assert.True(targetTrain.IsSuccess, targetTrain.ErrorMessage);
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var sourceCheckpoint = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(sourceCheckpoint.IsSuccess, sourceCheckpoint.ErrorMessage);
        var malformedCheckpoint = sourceCheckpoint.Value! with
        {
            Version = version!,
            AccuracyAfterLastBatch = 0.99,
            HasTrainingScaffoldHistory = false,
            TrainingHistory = []
        };

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("VERSION_UNSUPPORTED", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-0.01)]
    [InlineData(PrivateInvalidNumericValueSentinel)]
    public void ImportCheckpoint_RejectsInvalidRootAccuracyWithoutMutatingState(double invalidAccuracy)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithInvalidRootAccuracy(invalidAccuracy);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        AssertNumericCheckpointFailure(import, "AccuracyAfterLastBatch", expectHistoryIndex: false);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("rows-zero", "RowsTrained")]
    [InlineData("rows-negative", "RowsTrained")]
    [InlineData("accuracy-before-nan", "AccuracyBefore")]
    [InlineData("accuracy-before-out-of-range", "AccuracyBefore")]
    [InlineData("accuracy-after-infinity", "AccuracyAfter")]
    [InlineData("accuracy-after-out-of-range", "AccuracyAfter")]
    [InlineData("reward-margin-nan", "RewardMargin")]
    [InlineData("reward-margin-infinity", "RewardMargin")]
    [InlineData("cpu-negative", "CpuMs")]
    [InlineData("peak-ram-negative", "PeakRamMb")]
    [InlineData("elapsed-negative", "ElapsedMs")]
    public void ImportCheckpoint_RejectsInvalidTrainingHistoryNumericValuesWithoutMutatingState(
        string invalidCase,
        string expectedSafeField)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithInvalidTrainingHistoryNumericValue(invalidCase);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        AssertNumericCheckpointFailure(import, expectedSafeField, expectHistoryIndex: true);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public async Task LoadCheckpointAsync_RejectsSerializedFiniteInvalidNumericCheckpointValuesWithoutLoading()
    {
        var checkpoint = CreateCheckpointWithInvalidRootAccuracy(PrivateInvalidNumericValueSentinel);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-invalid-numeric-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var json = JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard);
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            AssertNumericCheckpointFailure(load, "AccuracyAfterLastBatch", expectHistoryIndex: false);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportCheckpoint_RejectsMixedValidAndInvalidLearnedWeightsWithoutPartialAcceptance()
    {
        var target = new DesignConversionMachine();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.118",
            accuracy: 0.91,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.4),
                InvalidLearnedWeight("source_kind:canva")
            ]);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public void ImportCheckpoint_RejectsDuplicateLearnedWeightFeatureKeysWithoutCollapsingState()
    {
        var target = new DesignConversionMachine();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.119",
            accuracy: 0.87,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.4),
                ValidLearnedWeight("source_kind:figma", weight: 0.7)
            ]);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public void ImportCheckpoint_RejectsLearnedWeightEntriesWhenPresenceFlagIsFalse()
    {
        var target = new DesignConversionMachine();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.119",
            accuracy: 0.88,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: false,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.5)
            ]);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public void ImportCheckpoint_WhenLearnedWeightValidationFailsAfterHeaderValidation_PreservesFullExistingCheckpointState()
    {
        var target = new DesignConversionMachine();
        var baselineCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.120",
            accuracy: 0.31,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.25)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.75)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-existing-history", accuracyAfter: 0.31)
            ]);
        var baselineImport = target.ImportCheckpoint(baselineCheckpoint);
        Assert.True(baselineImport.IsSuccess, baselineImport.ErrorMessage);
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var malformedCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.121",
            accuracy: 0.99,
            hasTrainingScaffoldHistory: false,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.9)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                InvalidLearnedWeight("correction_category:layout")
            ],
            trainingHistory: []);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("critic")]
    [InlineData("improver")]
    public void ImportCheckpoint_RejectsNullSourceIdentityHashElementWithoutMutatingObservableState(string slot)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithNullSourceIdentityHash(slot);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var message = import.ErrorMessage ?? string.Empty;
        Assert.DoesNotContain(PrivateInvalidSourceIdentityHashSentinel, message, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateBatchIdSentinel, message, StringComparison.Ordinal);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("critic")]
    [InlineData("improver")]
    public async Task LoadCheckpointAsync_RejectsSerializedNullSourceIdentityHashElementWithoutThrowing(string slot)
    {
        var malformedCheckpoint = CreateCheckpointWithNullSourceIdentityHash(slot);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-null-source-hash-{slot}-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var json = JsonSerializer.Serialize(malformedCheckpoint, DesignConversionCheckpointJsonOptions.Standard);
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", load.ErrorCode);
            var message = load.ErrorMessage ?? string.Empty;
            Assert.DoesNotContain(PrivateInvalidSourceIdentityHashSentinel, message, StringComparison.Ordinal);
            Assert.DoesNotContain(PrivateBatchIdSentinel, message, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("critic")]
    [InlineData("improver")]
    public void LearnedWeightInspectionHelpers_TreatNullSourceIdentityHashElementAsNoRealWeights(string slot)
    {
        var malformedCheckpoint = CreateCheckpointWithNullSourceIdentityHash(slot, includeOtherValidWeight: false);

        Assert.False(DesignConversionMachine.CheckpointHasRealLearnedWeights(malformedCheckpoint));
        Assert.Equal(0, DesignConversionMachine.CountRealLearnedWeightArtifacts(malformedCheckpoint));
        Assert.Equal(0, DesignConversionMachine.CountRealLearnedWeightObservations(malformedCheckpoint));
    }

    [Theory]
    [InlineData("critic-unit")]
    [InlineData("improver-unit")]
    public void ImportCheckpoint_RejectsNullRuntimeUnitNodesWithoutMutatingState(string nullNode)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithNullNode(nullNode);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal(CheckpointShapeInvalidCode, import.ErrorCode);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("training-history")]
    [InlineData("training-history-entry")]
    [InlineData("promoted-unit-ids")]
    public void ImportCheckpoint_RejectsNullTrainingHistoryShapeWithoutMutatingState(string nullNode)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithNullNode(nullNode);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal(CheckpointShapeInvalidCode, import.ErrorCode);
        if (nullNode == "promoted-unit-ids")
        {
            Assert.DoesNotContain(PrivateBatchIdSentinel, import.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal("Checkpoint training history entry at index 0 promoted unit ids are required.", import.ErrorMessage);
        }

        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("critic-unit")]
    [InlineData("improver-unit")]
    [InlineData("training-history")]
    [InlineData("training-history-entry")]
    [InlineData("promoted-unit-ids")]
    public async Task LoadCheckpointAsync_RejectsSerializedNullCheckpointShapeWithoutThrowing(string nullNode)
    {
        var checkpoint = CreateCheckpointWithNullNode(nullNode);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-null-shape-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var json = JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard);
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(CheckpointShapeInvalidCode, load.ErrorCode);
            if (nullNode == "promoted-unit-ids")
            {
                Assert.DoesNotContain(PrivateBatchIdSentinel, load.ErrorMessage, StringComparison.Ordinal);
                Assert.Equal("Checkpoint training history entry at index 0 promoted unit ids are required.", load.ErrorMessage);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("critic", "UnitId")]
    [InlineData("critic", "UnitRole")]
    [InlineData("critic", "DomainTag")]
    [InlineData("improver", "UnitId")]
    [InlineData("improver", "UnitRole")]
    [InlineData("improver", "DomainTag")]
    public void ImportCheckpoint_RejectsRuntimeUnitIdentityMismatchWithoutMutatingState(
        string slot,
        string field)
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithRuntimeUnitIdentityMismatch(slot, field);

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal(CheckpointRuntimeUnitIdentityMismatchCode, import.ErrorCode);
        Assert.Contains(slot, import.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains(field, import.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateRuntimeUnitIdentitySentinel, import.ErrorMessage, StringComparison.Ordinal);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public void ImportCheckpoint_RejectsWrongRuntimeUnitIdWithValidLearnedWeightsBeforeWeightsCanDrop()
    {
        var target = CreateMachineWithObservableCheckpointState();
        var before = target.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var malformedCheckpoint = CreateCheckpointWithRuntimeUnitIdentityMismatch("critic", "UnitId");
        Assert.Equal(2, DesignConversionMachine.CountRealLearnedWeightArtifacts(malformedCheckpoint));

        var import = target.ImportCheckpoint(malformedCheckpoint);

        Assert.False(import.IsSuccess);
        Assert.Equal(CheckpointRuntimeUnitIdentityMismatchCode, import.ErrorCode);
        Assert.Contains("critic", import.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("UnitId", import.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateRuntimeUnitIdentitySentinel, import.ErrorMessage, StringComparison.Ordinal);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Theory]
    [InlineData("critic", "UnitId")]
    [InlineData("critic", "UnitRole")]
    [InlineData("critic", "DomainTag")]
    [InlineData("improver", "UnitId")]
    [InlineData("improver", "UnitRole")]
    [InlineData("improver", "DomainTag")]
    public async Task LoadCheckpointAsync_RejectsSerializedRuntimeUnitIdentityMismatchWithoutThrowing(
        string slot,
        string field)
    {
        var checkpoint = CreateCheckpointWithRuntimeUnitIdentityMismatch(slot, field);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-runtime-unit-identity-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "checkpoint.json");

        try
        {
            Directory.CreateDirectory(root);
            var json = JsonSerializer.Serialize(checkpoint, DesignConversionCheckpointJsonOptions.Standard);
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);

            var load = await DesignConversionMachine.LoadCheckpointAsync(path);

            Assert.False(load.IsSuccess);
            Assert.Equal(CheckpointRuntimeUnitIdentityMismatchCode, load.ErrorCode);
            Assert.Contains(slot, load.ErrorMessage, StringComparison.Ordinal);
            Assert.Contains(field, load.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(PrivateRuntimeUnitIdentitySentinel, load.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportCheckpoint_AcceptsConsistentLearnedWeightsAndRoundTripsWeights()
    {
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.122",
            accuracy: 0.42,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.2),
                ValidLearnedWeight("internal_vector:mean", weight: 0.6)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.8)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-valid-learned", accuracyAfter: 0.42)
            ]);
        var target = new DesignConversionMachine();

        var import = target.ImportCheckpoint(checkpoint);

        Assert.True(import.IsSuccess, import.ErrorMessage);
        var after = target.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(3, DesignConversionMachine.CountRealLearnedWeightArtifacts(after.Value!));
        Assert.Equal(12, DesignConversionMachine.CountRealLearnedWeightObservations(after.Value!));
        AssertCheckpointStateEquals(checkpoint, after.Value!);
    }

    [Fact]
    public void ImportCheckpoint_SnapshotsPromotedUnitIdsAtImportAndExportBoundaries()
    {
        var importedPromotedUnitIds = new List<string>
        {
            DesignConversionRuntimeScaffoldCriticUnit.UnitId
        };
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.143",
            accuracy: 0.43,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: false,
            criticLearnedWeights: [],
            improverLearnedWeightsPresent: false,
            improverLearnedWeights: [],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-promoted-unit-alias", accuracyAfter: 0.43) with
                {
                    PromotedUnitIds = importedPromotedUnitIds
                }
            ]);
        var target = new DesignConversionMachine();
        var expectedPromotedUnitIds = new[] { DesignConversionRuntimeScaffoldCriticUnit.UnitId };

        var import = target.ImportCheckpoint(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        importedPromotedUnitIds.Add("caller-mutated-after-import");
        var firstExport = target.ExportCheckpoint();
        Assert.True(firstExport.IsSuccess, firstExport.ErrorMessage);
        var firstExportPromotedUnitIds = Assert.Single(firstExport.Value!.TrainingHistory).PromotedUnitIds;
        Assert.Equal(expectedPromotedUnitIds, firstExportPromotedUnitIds.ToArray());

        var mutableExportedPromotedUnitIds = Assert.IsAssignableFrom<IList<string>>(firstExportPromotedUnitIds);
        mutableExportedPromotedUnitIds[0] = "caller-mutated-after-export";

        var secondExport = target.ExportCheckpoint();
        Assert.True(secondExport.IsSuccess, secondExport.ErrorMessage);
        Assert.Equal(
            expectedPromotedUnitIds,
            Assert.Single(secondExport.Value!.TrainingHistory).PromotedUnitIds.ToArray());
        Assert.Equal("caller-mutated-after-export", firstExportPromotedUnitIds[0]);
    }

    [Fact]
    public async Task TrainBatch_WhenPatchCannotIncrement_FailsClosedWithoutMutatingState()
    {
        var machine = new DesignConversionMachine();
        var checkpoint = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        var import = machine.ImportCheckpoint(checkpoint.Value! with
        {
            Version = "1.0.2147483647",
            AccuracyAfterLastBatch = 0.42
        });
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, CreateBatch("batch-patch-overflow"));

        Assert.False(train.IsSuccess);
        Assert.Equal("VERSION_UNSUPPORTED", train.ErrorCode);
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        AssertCheckpointStateEquals(before.Value!, after.Value!);
    }

    [Fact]
    public async Task TrainBatch_IncrementsPatchExactlyOnceWithInvariantFormatting()
    {
        var machine = new DesignConversionMachine();
        var templateCheckpoint = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(templateCheckpoint.IsSuccess, templateCheckpoint.ErrorMessage);
        var import = machine.ImportCheckpoint(templateCheckpoint.Value! with { Version = "0.1.116" });
        Assert.True(import.IsSuccess, import.ErrorMessage);

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, CreateBatch("batch-version-increment"));

        Assert.True(train.IsSuccess, train.ErrorMessage);
        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.Equal("0.1.117", checkpoint.Value!.Version);
        var entry = Assert.Single(checkpoint.Value.TrainingHistory);
        Assert.Equal("batch-version-increment", entry.BatchId);
    }

    private static LearningMachineExport CreateExport(string? version, double accuracy = 0.0)
        => new(
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: version!,
            CheckpointRef: $"test/{version ?? "null"}/checkpoint",
            ManifestRef: $"test/{version ?? "null"}/manifest.json",
            AccuracyAtExport: accuracy,
            ExportedAtUtc: DateTimeOffset.UtcNow);

    private static async Task<LearningMachineExport> CreateSavedExportAsync(
        string checkpointPath,
        string version,
        double accuracy)
    {
        var source = new DesignConversionMachine();
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: version,
            accuracy: accuracy,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: false,
            criticLearnedWeights: [],
            trainingHistory:
            [
                CreateTrainingHistory($"checkpoint-backed-import-{version}", accuracyAfter: accuracy)
            ]);
        var import = source.ImportCheckpoint(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var save = await source.SaveCheckpointAsync(checkpointPath);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var export = await source.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!;
    }

    private static void AssertCheckpointStateEquals(
        DesignConversionMachineCheckpoint expected,
        DesignConversionMachineCheckpoint actual)
    {
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.AccuracyAfterLastBatch, actual.AccuracyAfterLastBatch);
        Assert.Equal(expected.HasTrainingScaffoldHistory, actual.HasTrainingScaffoldHistory);
        AssertTrainingHistoryEquals(expected.TrainingHistory, actual.TrainingHistory);
        AssertRuntimeUnitStateEquals(expected.CriticUnit, actual.CriticUnit);
        AssertRuntimeUnitStateEquals(expected.ImproverUnit, actual.ImproverUnit);
        Assert.Equal(
            DesignConversionMachine.CountRealLearnedWeightArtifacts(expected),
            DesignConversionMachine.CountRealLearnedWeightArtifacts(actual));
        Assert.Equal(
            DesignConversionMachine.CountRealLearnedWeightObservations(expected),
            DesignConversionMachine.CountRealLearnedWeightObservations(actual));
    }

    private static void AssertTrainingHistoryEquals(
        IReadOnlyList<DesignConversionTrainingHistoryEntryCheckpoint> expected,
        IReadOnlyList<DesignConversionTrainingHistoryEntryCheckpoint> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].BatchId, actual[i].BatchId);
            Assert.Equal(expected[i].RowsTrained, actual[i].RowsTrained);
            Assert.Equal(expected[i].AccuracyBefore, actual[i].AccuracyBefore);
            Assert.Equal(expected[i].AccuracyAfter, actual[i].AccuracyAfter);
            Assert.Equal(expected[i].RewardMargin, actual[i].RewardMargin);
            Assert.Equal(expected[i].CpuMs, actual[i].CpuMs);
            Assert.Equal(expected[i].PeakRamMb, actual[i].PeakRamMb);
            Assert.Equal(expected[i].ElapsedMs, actual[i].ElapsedMs);
            Assert.Equal(
                expected[i].PromotedUnitIds.ToArray(),
                actual[i].PromotedUnitIds.ToArray());
        }
    }

    private static void AssertRuntimeUnitStateEquals(
        DesignConversionRuntimeUnitCheckpoint expected,
        DesignConversionRuntimeUnitCheckpoint actual)
    {
        Assert.Equal(expected.UnitId, actual.UnitId);
        Assert.Equal(expected.UnitRole, actual.UnitRole);
        Assert.Equal(expected.DomainTag, actual.DomainTag);
        Assert.Equal(expected.ImplementationStatus, actual.ImplementationStatus);
        Assert.Equal(expected.LearnedWeightsPresent, actual.LearnedWeightsPresent);
        Assert.Equal(
            expected.EvidenceRefs.Order(StringComparer.Ordinal).ToArray(),
            actual.EvidenceRefs.Order(StringComparer.Ordinal).ToArray());
        AssertLearnedWeightsEqual(
            (expected.LearnedWeights ?? []).OrderBy(weight => weight.FeatureKey, StringComparer.Ordinal).ToArray(),
            (actual.LearnedWeights ?? []).OrderBy(weight => weight.FeatureKey, StringComparer.Ordinal).ToArray());
    }

    private static void AssertLearnedWeightsEqual(
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> expected,
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].FeatureKey, actual[i].FeatureKey);
            Assert.Equal(expected[i].Weight, actual[i].Weight);
            Assert.Equal(expected[i].ObservationCount, actual[i].ObservationCount);
            Assert.Equal(expected[i].LastBatchId, actual[i].LastBatchId);
            Assert.Equal(expected[i].Derivation, actual[i].Derivation);
            Assert.Equal(
                expected[i].SourceIdentityHashes.Order(StringComparer.Ordinal).ToArray(),
                actual[i].SourceIdentityHashes.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(
                expected[i].EvidenceRefs.Order(StringComparer.Ordinal).ToArray(),
                actual[i].EvidenceRefs.Order(StringComparer.Ordinal).ToArray());
        }
    }

    private static DesignConversionMachineCheckpoint CreateCheckpointWithLearnedWeights(
        string version,
        double accuracy,
        bool hasTrainingScaffoldHistory,
        bool criticLearnedWeightsPresent,
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint>? criticLearnedWeights,
        bool improverLearnedWeightsPresent = false,
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint>? improverLearnedWeights = null,
        IReadOnlyList<DesignConversionTrainingHistoryEntryCheckpoint>? trainingHistory = null)
    {
        var template = new DesignConversionMachine().ExportCheckpoint();
        Assert.True(template.IsSuccess, template.ErrorMessage);
        return template.Value! with
        {
            Version = version,
            AccuracyAfterLastBatch = accuracy,
            HasTrainingScaffoldHistory = hasTrainingScaffoldHistory,
            CriticUnit = RuntimeUnitWithLearnedWeights(
                template.Value.CriticUnit,
                criticLearnedWeightsPresent,
                criticLearnedWeights),
            ImproverUnit = RuntimeUnitWithLearnedWeights(
                template.Value.ImproverUnit,
                improverLearnedWeightsPresent,
                improverLearnedWeights),
            TrainingHistory = trainingHistory ?? []
        };
    }

    private static DesignConversionMachineCheckpoint CreateCheckpointWithDpoPolicyRef(
        DesignConversionDpoPolicyCheckpointRef checkpointRef)
    {
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.142",
            accuracy: 0.72,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.44)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.56)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("dpo-policy-ref-validation", accuracyAfter: 0.72)
            ]);

        return checkpoint with { DpoPolicyCheckpointRef = checkpointRef };
    }

    private static DesignConversionDpoPolicyCheckpointRef CreateValidDpoPolicyCheckpointRef()
    {
        const string fileName = "dpo_policy_step_000001.bin";
        var sha256 = new string('a', 64);
        return new DesignConversionDpoPolicyCheckpointRef(
            ArtifactKind: "11.DPO:DpoTrainer.policy_checkpoint",
            PolicyCheckpointFileName: fileName,
            Sha256: sha256,
            SourceSystem: "11.DPO:DpoTrainer",
            SourceMechanism: "11.DPO:DpoTrainer.StepVector:fresh_loaded",
            RuntimeBridgeKind: "dpo_policy_checkpoint_identity_metadata_not_direct_policy_inference",
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: 2,
            BridgePreferenceCount: 1,
            DpoStepCount: 1,
            TrainingContext: new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: "DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig",
                VocabSize: 512,
                ContextLength: 64,
                HiddenDim: 16,
                NumLayers: 1,
                NumHeads: 4,
                NumKvHeads: 2,
                FfnHiddenDim: 32,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false),
            EvidenceRefs:
            [
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_NO_RAW_PAYLOAD",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                $"dpo_policy_checkpoint_sha256:{sha256}",
                $"dpo_policy_checkpoint_file:{fileName}",
                "dpo_policy_bridge_source_record_count:2",
                "dpo_policy_bridge_preference_count:1",
                "dpo_policy_step_count:1",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_RAW_PAYLOAD"
            ]);
    }

    private static DesignConversionDpoPolicyCheckpointRef CreateValidV2DpoPolicyCheckpointRef()
    {
        var legacy = CreateValidDpoPolicyCheckpointRef();
        var context = legacy.TrainingContext with
        {
            ConfigKind = "test.non_default.policy.v2",
            VocabSize = 300,
            ContextLength = 17,
            HiddenDim = 12,
            NumLayers = 1,
            NumHeads = 3,
            NumKvHeads = 1,
            FfnHiddenDim = 20,
            SchemaVersion = 2,
            RopeTheta = 7_777f,
            RmsNormEps = 2e-5f,
            ProviderKind = "test_in_memory",
            BaseModelId = "test-org/non-default-policy",
            BaseRevision = "revision-v1",
            BaseCheckpointSha256 = new string('b', 64),
            InitialWeightSha256 = new string('c', 64)
        };
        context = context with { ConfigSha256 = ComputeExpectedV2ConfigSha256(context) };

        return legacy with
        {
            TrainingContext = context,
            EvidenceRefs = legacy.EvidenceRefs
                .Concat(
                [
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
                    "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
                    "DESIGN_CONVERSION_DPO_POLICY_REFERENCE_FROZEN_AND_UNCHANGED",
                    "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
                    $"dpo_policy_base_provider_kind:{context.ProviderKind}",
                    $"dpo_policy_base_configuration_kind:{context.ConfigKind}",
                    $"dpo_policy_base_model_id:{context.BaseModelId}",
                    $"dpo_policy_base_revision:{context.BaseRevision}",
                    $"dpo_policy_base_checkpoint_sha256:{context.BaseCheckpointSha256}",
                    $"dpo_policy_config_sha256:{context.ConfigSha256}",
                    $"dpo_policy_initial_weight_sha256:{context.InitialWeightSha256}"
                ])
                .ToArray()
        };
    }

    private static string ComputeExpectedV2ConfigSha256(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExpectedV2String(hash, "design-conversion-dpo-policy-config-v1");
        AppendExpectedV2Int32(hash, context.VocabSize);
        AppendExpectedV2Int32(hash, context.ContextLength);
        AppendExpectedV2Int32(hash, context.HiddenDim);
        AppendExpectedV2Int32(hash, context.NumLayers);
        AppendExpectedV2Int32(hash, context.NumHeads);
        AppendExpectedV2Int32(hash, context.NumKvHeads);
        AppendExpectedV2Int32(hash, context.FfnHiddenDim);
        AppendExpectedV2Int32(hash, BitConverter.SingleToInt32Bits(context.RopeTheta));
        AppendExpectedV2Int32(hash, BitConverter.SingleToInt32Bits(context.RmsNormEps));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static DesignConversionDpoPolicyCheckpointRef WithRecomputedV2ConfigEvidence(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        DesignConversionDpoPolicyCheckpointTrainingContext context)
    {
        var previousConfigSha = checkpointRef.TrainingContext.ConfigSha256;
        var recomputedContext = context with { ConfigSha256 = ComputeExpectedV2ConfigSha256(context) };
        return ReplaceDpoPolicyEvidence(
            checkpointRef with { TrainingContext = recomputedContext },
            $"dpo_policy_config_sha256:{previousConfigSha}",
            $"dpo_policy_config_sha256:{recomputedContext.ConfigSha256}");
    }

    private static void AppendExpectedV2String(IncrementalHash hash, string value)
    {
        var bytes = new UTF8Encoding(false, true).GetBytes(value);
        AppendExpectedV2Int32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendExpectedV2Int32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static DesignConversionDpoPolicyCheckpointRef CreateInvalidDpoPolicyCheckpointRef(
        string invalidCase)
    {
        var valid = CreateValidDpoPolicyCheckpointRef();
        var context = valid.TrainingContext;
        return invalidCase switch
        {
            "training-context-null" => valid with { TrainingContext = null! },
            "legacy-commitment-present" => valid with
            {
                TrainingContext = context with
                {
                    LegalTeacherAdmissionCommitment =
                        DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(
                            valid.EvidenceRefs)
                }
            },
            "evidence-refs-null" => valid with { EvidenceRefs = null! },
            "artifact-kind-blank" => valid with { ArtifactKind = " " },
            "source-system-wrong" => valid with { SourceSystem = "11.DPO:DpoTrainer.other" },
            "source-mechanism-wrong" => valid with { SourceMechanism = "11.DPO:DpoTrainer.StepVector" },
            "runtime-bridge-kind-wrong" => valid with { RuntimeBridgeKind = "direct_policy_inference" },
            "config-kind-wrong" => valid with
            {
                TrainingContext = context with
                {
                    ConfigKind = "DesignConversionDpoPolicyCheckpointBridge.CreateTinyPolicyConfig.other"
                }
            },
            "filename-path" => valid with { PolicyCheckpointFileName = @"C:\private\dpo_policy_step_000001.bin" },
            "filename-format" => valid with { PolicyCheckpointFileName = "policy-checkpoint-0001.json" },
            "filename-step-mismatch" => valid with { PolicyCheckpointFileName = "dpo_policy_step_000002.bin" },
            "sha-short" => valid with { Sha256 = new string('a', 63) },
            "sha-non-hex" => valid with { Sha256 = $"{new string('a', 63)}g" },
            "fresh-load-false" => valid with { FreshLoadVerified = false },
            "converted-false" => valid with { ConvertedToRuntimeWeights = false },
            "source-count-zero" => valid with { SourceRecordCount = 0 },
            "bridge-count-zero" => valid with { BridgePreferenceCount = 0 },
            "bridge-count-exceeds-source" => valid with { SourceRecordCount = 1, BridgePreferenceCount = 2 },
            "dpo-step-zero" => valid with { DpoStepCount = 0 },
            "vocab-size-zero" => valid with { TrainingContext = context with { VocabSize = 0 } },
            "context-length-zero" => valid with { TrainingContext = context with { ContextLength = 0 } },
            "hidden-dim-zero" => valid with { TrainingContext = context with { HiddenDim = 0 } },
            "num-layers-zero" => valid with { TrainingContext = context with { NumLayers = 0 } },
            "num-heads-zero" => valid with { TrainingContext = context with { NumHeads = 0 } },
            "num-kv-heads-zero" => valid with { TrainingContext = context with { NumKvHeads = 0 } },
            "ffn-hidden-dim-zero" => valid with { TrainingContext = context with { FfnHiddenDim = 0 } },
            "ffn-hidden-dim-smaller-than-hidden" => valid with { TrainingContext = context with { FfnHiddenDim = 8 } },
            "hidden-head-relation" => valid with { TrainingContext = context with { HiddenDim = 15 } },
            "head-kv-relation" => valid with { TrainingContext = context with { NumKvHeads = 3 } },
            "raw-payload-persisted" => valid with { TrainingContext = context with { RawPayloadPersisted = true } },
            "absolute-path-persisted" => valid with { TrainingContext = context with { AbsolutePathPersisted = true } },
            "evidence-missing-typed" => RemoveDpoPolicyEvidence(
                valid,
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED"),
            "evidence-missing-no-path" => RemoveDpoPolicyEvidence(
                valid,
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH"),
            "evidence-missing-no-raw" => RemoveDpoPolicyEvidence(
                valid,
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_RAW_PAYLOAD"),
            "evidence-sha-mismatch" => ReplaceDpoPolicyEvidence(
                valid,
                $"dpo_policy_checkpoint_sha256:{valid.Sha256}",
                $"dpo_policy_checkpoint_sha256:{new string('b', 64)}"),
            "evidence-file-mismatch" => ReplaceDpoPolicyEvidence(
                valid,
                $"dpo_policy_checkpoint_file:{valid.PolicyCheckpointFileName}",
                "dpo_policy_checkpoint_file:dpo_policy_step_000002.bin"),
            "evidence-source-count-mismatch" => ReplaceDpoPolicyEvidence(
                valid,
                "dpo_policy_bridge_source_record_count:2",
                "dpo_policy_bridge_source_record_count:3"),
            "evidence-preference-count-mismatch" => ReplaceDpoPolicyEvidence(
                valid,
                "dpo_policy_bridge_preference_count:1",
                "dpo_policy_bridge_preference_count:2"),
            "evidence-step-count-mismatch" => ReplaceDpoPolicyEvidence(
                valid,
                "dpo_policy_step_count:1",
                "dpo_policy_step_count:2"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(invalidCase),
                invalidCase,
                "Unknown invalid DPO policy checkpoint reference case.")
        };
    }

    private static DesignConversionDpoPolicyCheckpointRef RemoveDpoPolicyEvidence(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        string evidenceRef)
        => checkpointRef with
        {
            EvidenceRefs = checkpointRef.EvidenceRefs
                .Where(current => !string.Equals(current, evidenceRef, StringComparison.Ordinal))
                .ToArray()
        };

    private static DesignConversionDpoPolicyCheckpointRef ReplaceDpoPolicyEvidence(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        string oldEvidenceRef,
        string newEvidenceRef)
        => checkpointRef with
        {
            EvidenceRefs = checkpointRef.EvidenceRefs
                .Select(current => string.Equals(current, oldEvidenceRef, StringComparison.Ordinal)
                    ? newEvidenceRef
                    : current)
                .ToArray()
        };

    private static DesignConversionRuntimeUnitCheckpoint RuntimeUnitWithLearnedWeights(
        DesignConversionRuntimeUnitCheckpoint template,
        bool learnedWeightsPresent,
        IReadOnlyList<DesignConversionLearnedWeightCheckpoint>? learnedWeights)
        => template with
        {
            ImplementationStatus = learnedWeightsPresent
                ? $"{template.ImplementationStatus}_with_local_learned_weight_artifact"
                : template.ImplementationStatus,
            EvidenceRefs = learnedWeightsPresent
                ? template.EvidenceRefs
                    .Concat(
                    [
                        DesignConversionMachine.LearnedWeightsPersistedEvidence,
                        DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
                    ])
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
                : template.EvidenceRefs,
            LearnedWeightsPresent = learnedWeightsPresent,
            LearnedWeights = learnedWeights
        };

    private static DesignConversionLearnedWeightCheckpoint ValidLearnedWeight(
        string featureKey,
        double weight = 0.5)
        => new(
            FeatureKey: featureKey,
            Weight: weight,
            ObservationCount: 4,
            LastBatchId: "checkpoint-learned-weight-import-test",
            Derivation: "sanitized_runtime_feature_import_test",
            SourceIdentityHashes:
            [
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            ],
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence,
                "DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT"
            ]);

    private static DesignConversionLearnedWeightCheckpoint InvalidLearnedWeight(string featureKey)
        => ValidLearnedWeight(featureKey) with
        {
            EvidenceRefs = []
        };

    private static DesignConversionLearnedWeightCheckpoint LearnedWeightWithNullSourceIdentityHash(string featureKey)
        => ValidLearnedWeight(featureKey) with
        {
            LastBatchId = PrivateBatchIdSentinel,
            SourceIdentityHashes =
            [
                null!,
                PrivateInvalidSourceIdentityHashSentinel
            ]
        };

    private static DesignConversionTrainingHistoryEntryCheckpoint CreateTrainingHistory(
        string batchId,
        double accuracyAfter)
        => new(
            BatchId: batchId,
            RowsTrained: 1,
            AccuracyBefore: 0.1,
            AccuracyAfter: accuracyAfter,
            RewardMargin: 0.2,
            CpuMs: 1,
            PeakRamMb: 1,
            ElapsedMs: 1,
            PromotedUnitIds:
            [
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.UnitId
            ]);

    private static DesignConversionMachineCheckpoint CreateCheckpointWithInvalidRootAccuracy(double invalidAccuracy)
        => CreateCheckpointWithLearnedWeights(
            version: "0.1.133",
            accuracy: invalidAccuracy,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.47)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:layout", weight: 0.53)
            ],
            trainingHistory:
            [
                CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.52)
            ]);

    private static DesignConversionMachineCheckpoint CreateCheckpointWithInvalidTrainingHistoryNumericValue(string invalidCase)
    {
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.134",
            accuracy: 0.52,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.48)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:layout", weight: 0.52)
            ],
            trainingHistory:
            [
                CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.52)
            ]);
        var history = checkpoint.TrainingHistory[0];
        var malformedHistory = invalidCase switch
        {
            "rows-zero" => history with { RowsTrained = 0 },
            "rows-negative" => history with { RowsTrained = -1 },
            "accuracy-before-nan" => history with { AccuracyBefore = double.NaN },
            "accuracy-before-out-of-range" => history with { AccuracyBefore = PrivateInvalidNumericValueSentinel },
            "accuracy-after-infinity" => history with { AccuracyAfter = double.PositiveInfinity },
            "accuracy-after-out-of-range" => history with { AccuracyAfter = -0.01 },
            "reward-margin-nan" => history with { RewardMargin = double.NaN },
            "reward-margin-infinity" => history with { RewardMargin = double.PositiveInfinity },
            "cpu-negative" => history with { CpuMs = -1 },
            "peak-ram-negative" => history with { PeakRamMb = -1 },
            "elapsed-negative" => history with { ElapsedMs = -1 },
            _ => throw new ArgumentOutOfRangeException(nameof(invalidCase), invalidCase, "Unknown numeric checkpoint case.")
        };

        return checkpoint with { TrainingHistory = [malformedHistory] };
    }

    private static DesignConversionMachineCheckpoint CreateCheckpointWithNullSourceIdentityHash(
        string slot,
        bool includeOtherValidWeight = true)
        => slot switch
        {
            "critic" when includeOtherValidWeight => CreateCheckpointWithLearnedWeights(
                version: "0.1.135",
                accuracy: 0.93,
                hasTrainingScaffoldHistory: true,
                criticLearnedWeightsPresent: true,
                criticLearnedWeights:
                [
                    LearnedWeightWithNullSourceIdentityHash("source_kind:canva")
                ],
                improverLearnedWeightsPresent: true,
                improverLearnedWeights:
                [
                    ValidLearnedWeight("correction_category:layout", weight: 0.54)
                ],
                trainingHistory:
                [
                    CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.93)
                ]),
            "critic" => CreateCheckpointWithLearnedWeights(
                version: "0.1.135",
                accuracy: 0.93,
                hasTrainingScaffoldHistory: true,
                criticLearnedWeightsPresent: true,
                criticLearnedWeights:
                [
                    LearnedWeightWithNullSourceIdentityHash("source_kind:canva")
                ],
                improverLearnedWeightsPresent: false,
                improverLearnedWeights: [],
                trainingHistory:
                [
                    CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.93)
                ]),
            "improver" when includeOtherValidWeight => CreateCheckpointWithLearnedWeights(
                version: "0.1.135",
                accuracy: 0.93,
                hasTrainingScaffoldHistory: true,
                criticLearnedWeightsPresent: true,
                criticLearnedWeights:
                [
                    ValidLearnedWeight("source_kind:canva", weight: 0.46)
                ],
                improverLearnedWeightsPresent: true,
                improverLearnedWeights:
                [
                    LearnedWeightWithNullSourceIdentityHash("correction_category:layout")
                ],
                trainingHistory:
                [
                    CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.93)
                ]),
            "improver" => CreateCheckpointWithLearnedWeights(
                version: "0.1.135",
                accuracy: 0.93,
                hasTrainingScaffoldHistory: true,
                criticLearnedWeightsPresent: false,
                criticLearnedWeights: [],
                improverLearnedWeightsPresent: true,
                improverLearnedWeights:
                [
                    LearnedWeightWithNullSourceIdentityHash("correction_category:layout")
                ],
                trainingHistory:
                [
                    CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.93)
                ]),
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown learned-weight slot.")
        };

    private static void AssertNumericCheckpointFailure<T>(
        OperationResult<T> result,
        string expectedSafeField,
        bool expectHistoryIndex)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(CheckpointNumericValuesInvalidCode, result.ErrorCode);
        var message = result.ErrorMessage ?? string.Empty;
        Assert.Contains(expectedSafeField, message, StringComparison.Ordinal);
        if (expectHistoryIndex)
            Assert.Contains("index 0", message, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateBatchIdSentinel, message, StringComparison.Ordinal);
        Assert.DoesNotContain(PrivateInvalidNumericValueSentinelText, message, StringComparison.Ordinal);
        Assert.DoesNotContain("NaN", message, StringComparison.Ordinal);
        Assert.DoesNotContain("Infinity", message, StringComparison.Ordinal);
    }

    private static DesignConversionMachine CreateMachineWithObservableCheckpointState()
    {
        var target = new DesignConversionMachine();
        var baselineCheckpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.130",
            accuracy: 0.56,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:figma", weight: 0.35)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.65)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-null-node-existing-history", accuracyAfter: 0.56)
            ]);
        var baselineImport = target.ImportCheckpoint(baselineCheckpoint);
        Assert.True(baselineImport.IsSuccess, baselineImport.ErrorMessage);
        return target;
    }

    private static DesignConversionMachineCheckpoint CreateCheckpointWithNullNode(string nullNode)
    {
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.131",
            accuracy: 0.91,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.45)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:layout", weight: 0.55)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-null-node-import-history", accuracyAfter: 0.91)
            ]);

        return nullNode switch
        {
            "critic-unit" => checkpoint with { CriticUnit = null! },
            "improver-unit" => checkpoint with { ImproverUnit = null! },
            "training-history" => checkpoint with { TrainingHistory = null! },
            "training-history-entry" => checkpoint with { TrainingHistory = [null!] },
            "promoted-unit-ids" => checkpoint with
            {
                TrainingHistory =
                [
                    CreateTrainingHistory(PrivateBatchIdSentinel, accuracyAfter: 0.91) with
                    {
                        PromotedUnitIds = null!
                    }
                ]
            },
            _ => throw new ArgumentOutOfRangeException(nameof(nullNode), nullNode, "Unknown null-node checkpoint case.")
        };
    }

    private static DesignConversionMachineCheckpoint CreateCheckpointWithRuntimeUnitIdentityMismatch(
        string slot,
        string field)
    {
        var checkpoint = CreateCheckpointWithLearnedWeights(
            version: "0.1.132",
            accuracy: 0.92,
            hasTrainingScaffoldHistory: true,
            criticLearnedWeightsPresent: true,
            criticLearnedWeights:
            [
                ValidLearnedWeight("source_kind:canva", weight: 0.46)
            ],
            improverLearnedWeightsPresent: true,
            improverLearnedWeights:
            [
                ValidLearnedWeight("correction_category:spacing", weight: 0.54)
            ],
            trainingHistory:
            [
                CreateTrainingHistory("checkpoint-runtime-unit-identity-import-history", accuracyAfter: 0.92)
            ]);

        return slot switch
        {
            "critic" => checkpoint with
            {
                CriticUnit = RuntimeUnitWithIdentityMismatch(checkpoint.CriticUnit, field)
            },
            "improver" => checkpoint with
            {
                ImproverUnit = RuntimeUnitWithIdentityMismatch(checkpoint.ImproverUnit, field)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown runtime-unit slot.")
        };
    }

    private static DesignConversionRuntimeUnitCheckpoint RuntimeUnitWithIdentityMismatch(
        DesignConversionRuntimeUnitCheckpoint unit,
        string field)
        => field switch
        {
            "UnitId" => unit with { UnitId = PrivateRuntimeUnitIdentitySentinel },
            "UnitRole" => unit with { UnitRole = PrivateRuntimeUnitIdentitySentinel },
            "DomainTag" => unit with { DomainTag = PrivateRuntimeUnitIdentitySentinel },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown runtime-unit identity field.")
        };

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> CreateBatch(string batchId)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}-item-001",
                    Prompt: "figma frame",
                    ChosenResponse: SerializeOptimizerSide(
                        "<main class=\"screen\"><section>closer spacing</section></main>",
                        ".screen{display:grid;gap:16px;}"),
                    RejectedResponse: SerializeOptimizerSide(
                        "<main class=\"screen\"><section>broken spacing</section></main>",
                        ".screen{display:block;margin:0;}"))
            ],
            SourceIds: ["slice-3-test"]);

    private static DesignConversionDpoTrainingRecord CreateTrainingRecord(string recordId)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            [DesignConversionDpoGeneratorTests.CreateExample(recordId, DesignConversionDpoUnits.CriticUnit)]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return Assert.Single(build.Value!.AcceptedRecords);
    }

    private static DesignConversionTrainingBatch CreateContinuationBatch(
        string batchId,
        DesignConversionDpoTrainingRecord record)
        => new(
            BatchId: batchId,
            UnitId: record.UnitId,
            Records: [record],
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: record.RecordId,
                    Prompt: record.SourceStructurePayload,
                    ChosenResponse: SerializeOptimizerSide(
                        record.Chosen.Html,
                        record.Chosen.Css),
                    RejectedResponse: SerializeOptimizerSide(
                        record.Rejected.Html,
                        record.Rejected.Css))
            ],
            SourceIds: [record.Provenance.SourceManifestRef]);

    private static string SerializeOptimizerSide(string html, string css)
        => JsonSerializer.Serialize(new { html, css });

    private static DesignConversionLocalFreshLoadProbeReport FreshLoadReportFor(
        string checkpointPath,
        bool metadataContinuityVerified = true)
        => new(
            Status: metadataContinuityVerified ? DesignConversionLocalFreshLoadProbe.VerifiedStatus : "failed",
            CheckpointRef: Path.GetFullPath(checkpointPath),
            ManifestRef: "design-conversion-machine/0.1.1/manifest.json",
            LoadedManifestRef: "design-conversion-machine/0.1.1/manifest.json",
            MachineId: DesignConversionMachine.MachineId,
            DomainTag: DesignConversionMachine.DomainTag,
            Version: "0.1.1",
            TrainingHistoryCount: 1,
            MetadataContinuityVerified: metadataContinuityVerified,
            CheckpointContainsRealTrainedWeights: false,
            ContinueTrainingProven: false,
            MeasurementGateRun: false,
            PromotionUploadAllowed: false,
            EvidenceRefs: ["TEST_FRESH_LOAD_REPORT"],
            FailureReasons: metadataContinuityVerified ? [] : ["TEST_METADATA_CONTINUITY_FAILED"]);

    private static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static DesignConversionDpoPolicyCheckpointRef CreateDpoPolicyCheckpointRef(
        string fileName,
        string sha256,
        bool rawPayloadPersisted,
        int byteLength)
    {
        var baseRef = CreateValidV2DpoPolicyCheckpointRef();
        var context = baseRef.TrainingContext with
        {
            ConfigKind = "test.sibling.policy.v2",
            RawPayloadPersisted = rawPayloadPersisted
        };
        context = context with { ConfigSha256 = ComputeExpectedV2ConfigSha256(context) };

        return baseRef with
        {
            PolicyCheckpointFileName = fileName,
            Sha256 = sha256,
            RuntimeBridgeKind = "dpo_policy_checkpoint_payload_file_sibling_validated_not_direct_policy_inference",
            SourceRecordCount = 1,
            BridgePreferenceCount = 1,
            DpoStepCount = 1,
            TrainingContext = context,
            EvidenceRefs =
            [
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                "DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PRODUCT_READY_FALSE",
                "11.DPO:DpoTrainer",
                "11.DPO:DpoTrainer.StepVector",
                $"dpo_policy_checkpoint_sha256:{sha256}",
                $"dpo_policy_checkpoint_file:{fileName}",
                "dpo_policy_bridge_source_record_count:1",
                "dpo_policy_bridge_preference_count:1",
                "dpo_policy_step_count:1",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_NO_ABSOLUTE_PATH",
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_PAYLOAD_FILE_PERSISTED",
                "DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_MODELS_DISTINCT",
                "DESIGN_CONVERSION_DPO_POLICY_INITIAL_POLICY_REFERENCE_BITWISE_EQUAL",
                "DESIGN_CONVERSION_DPO_POLICY_BASE_AND_INITIAL_WEIGHT_HASHES_DISTINCT",
                $"dpo_policy_base_provider_kind:{context.ProviderKind}",
                $"dpo_policy_base_configuration_kind:{context.ConfigKind}",
                $"dpo_policy_base_model_id:{context.BaseModelId}",
                $"dpo_policy_base_revision:{context.BaseRevision}",
                $"dpo_policy_base_checkpoint_sha256:{context.BaseCheckpointSha256}",
                $"dpo_policy_config_sha256:{context.ConfigSha256}",
                $"dpo_policy_initial_weight_sha256:{context.InitialWeightSha256}"
            ],
            PolicyCheckpointByteLength = byteLength
        };
    }

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static DesignConversionDeterministicMutationLease HoldCheckpointWriterLock(string checkpointPath)
    {
        var lockPath = DesignConversionMachine.GetCheckpointWriterLockPath(checkpointPath);
        return DesignConversionDeterministicMutationLock.AcquireForTesting(lockPath);
    }

    private static void AssertNoCheckpointTempFiles(string checkpointPath)
    {
        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        Assert.False(File.Exists(fullCheckpointPath + ".tmp"));

        var dir = Path.GetDirectoryName(fullCheckpointPath);
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return;

        var fileName = Path.GetFileName(fullCheckpointPath);
        var leakedTempFiles = Directory.EnumerateFiles(dir, $".{fileName}.*.tmp").ToArray();
        Assert.Empty(leakedTempFiles);
    }
}
