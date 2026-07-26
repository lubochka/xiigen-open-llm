// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionCheckpointLifecycleTests
{
    [Fact]
    public async Task ExportImport_RoundTripsMachineMetadataAndCheckpointScaffold()
    {
        var source = new DesignConversionMachine();
        var train = await source.TrainBatchAsync(CreateBatch("batch-export-import"));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var export = await source.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        var target = new DesignConversionMachine();
        var import = await target.ImportAsync(export.Value!);

        Assert.True(import.IsSuccess, import.ErrorMessage);
        var targetExport = await target.ExportAsync();
        Assert.True(targetExport.IsSuccess, targetExport.ErrorMessage);
        Assert.Equal(export.Value!.MachineId, targetExport.Value!.MachineId);
        Assert.Equal(export.Value.DomainTag, targetExport.Value.DomainTag);
        Assert.Equal(export.Value.Version, targetExport.Value.Version);
        Assert.Equal(export.Value.AccuracyAtExport, targetExport.Value.AccuracyAtExport);
    }

    [Fact]
    public async Task ExportImportCheckpoint_RoundTripsTrainingHistoryAndRuntimeUnitBoundaries()
    {
        var source = new DesignConversionMachine();
        var train = await source.TrainBatchAsync(CreateBatch("batch-checkpoint"));
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-save-load"));
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-conflict-no-destination"));
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
        var originalTrain = await original.TrainBatchAsync(CreateBatch("batch-conflict-original"));
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
            var contenderTrain = await contender.TrainBatchAsync(CreateBatch("batch-conflict-contender"));
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

    [Fact]
    public async Task LocalFreshLoadProbe_VerifiesSavedTempCheckpointMetadataOnly()
    {
        var source = new DesignConversionMachine();
        var train = await source.TrainBatchAsync(CreateBatch("batch-fresh-load-probe"));
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-fresh-load-metadata-mismatch"));
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
            var missing = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
                FreshLoadReportFor(missingPath),
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);
            Assert.False(missing.IsSuccess);
            Assert.Equal("CHECKPOINT_NOT_FOUND", missing.ErrorCode);

            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(corruptPath, "{ not valid checkpoint json", Encoding.UTF8);

            var corrupt = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-continue-requires-fresh-load"));
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

            var result = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-continue-held-out"));
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

            var result = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-continue-source"));
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

            var result = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
        var train = await source.TrainBatchAsync(CreateBatch("batch-replay-source"));
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

            var result = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
        var exportTarget = new DesignConversionMachine();
        var exportImport = await exportTarget.ImportAsync(CreateExport(version, accuracy: 0.25));
        Assert.True(exportImport.IsSuccess, exportImport.ErrorMessage);
        var exported = await exportTarget.ExportAsync();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        Assert.Equal(version, exported.Value!.Version);
        Assert.Equal(0.25, exported.Value.AccuracyAtExport);

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
        var baselineImport = await target.ImportAsync(CreateExport("0.1.116", accuracy: 0.25));
        Assert.True(baselineImport.IsSuccess, baselineImport.ErrorMessage);
        var before = await target.ExportAsync();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var import = await target.ImportAsync(CreateExport(version, accuracy: 0.99));

        Assert.False(import.IsSuccess);
        Assert.Equal("VERSION_UNSUPPORTED", import.ErrorCode);
        var after = await target.ExportAsync();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(before.Value!.Version, after.Value!.Version);
        Assert.Equal(before.Value.AccuracyAtExport, after.Value.AccuracyAtExport);
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
        var targetTrain = await target.TrainBatchAsync(CreateBatch("checkpoint-target-baseline"));
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

    [Fact]
    public async Task TrainBatch_WhenPatchCannotIncrement_FailsClosedWithoutMutatingState()
    {
        var machine = new DesignConversionMachine();
        var import = await machine.ImportAsync(CreateExport("1.0.2147483647", accuracy: 0.42));
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);

        var train = await machine.TrainBatchAsync(CreateBatch("batch-patch-overflow"));

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
        var import = await machine.ImportAsync(CreateExport("0.1.116"));
        Assert.True(import.IsSuccess, import.ErrorMessage);

        var train = await machine.TrainBatchAsync(CreateBatch("batch-version-increment"));

        Assert.True(train.IsSuccess, train.ErrorMessage);
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal("0.1.117", export.Value!.Version);
        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        var entry = Assert.Single(checkpoint.Value!.TrainingHistory);
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

    private static void AssertCheckpointStateEquals(
        DesignConversionMachineCheckpoint expected,
        DesignConversionMachineCheckpoint actual)
    {
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.AccuracyAfterLastBatch, actual.AccuracyAfterLastBatch);
        Assert.Equal(expected.HasTrainingScaffoldHistory, actual.HasTrainingScaffoldHistory);
        Assert.Equal(
            expected.TrainingHistory.Select(history => history.BatchId).ToArray(),
            actual.TrainingHistory.Select(history => history.BatchId).ToArray());
        Assert.Equal(
            DesignConversionMachine.CountRealLearnedWeightArtifacts(expected),
            DesignConversionMachine.CountRealLearnedWeightArtifacts(actual));
        Assert.Equal(
            DesignConversionMachine.CountRealLearnedWeightObservations(expected),
            DesignConversionMachine.CountRealLearnedWeightObservations(actual));
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> CreateBatch(string batchId)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}-item-001",
                    Prompt: "figma frame",
                    ChosenResponse: "candidate with closer spacing",
                    RejectedResponse: "candidate with broken spacing")
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
                    ChosenResponse: record.Chosen.OutputId,
                    RejectedResponse: record.Rejected.OutputId)
            ],
            SourceIds: [record.Provenance.SourceManifestRef]);

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

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static FileStream HoldCheckpointWriterLock(string checkpointPath)
    {
        var lockPath = DesignConversionMachine.GetCheckpointWriterLockPath(checkpointPath);
        var lockDir = Path.GetDirectoryName(lockPath);
        if (!string.IsNullOrWhiteSpace(lockDir))
            Directory.CreateDirectory(lockDir);

        return new FileStream(
            lockPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 1,
            FileOptions.Asynchronous);
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
