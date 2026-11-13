// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningMachineAssetLifecycleTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_ExportImportRoundTripsLearnedGuideState()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-lifecycle-{Guid.NewGuid():N}");
        try
        {
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(
                CreateDescriptor([CreatePattern()]),
                Path.Combine(root, "source-admission")));

            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath);

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            Assert.True(File.Exists(exportResult.Value!.CheckpointRef));
            Assert.True(File.Exists(exportResult.Value.ManifestRef));
            Assert.False(string.IsNullOrWhiteSpace(exportResult.Value.CheckpointSha256));

            var imported = new OrchestrationReasoningMachine();
            var importResult = await imported.ImportAsync(exportResult.Value);
            var planned = imported.Plan(CreateLearnedRequest("sha256:or-life-import-a"));

            Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
            Assert.True(planned.IsSuccess, planned.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, planned.Value!.ContractStatus);
            Assert.Equal("scenario:bounded-implementation-review-loop", planned.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(["or-pattern-bounded-implementation-review"], planned.Value.ScenarioPlan.SelectedLearnedPatterns);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_PersistedManifestUsesPortableRefs()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-portable-{Guid.NewGuid():N}");
        try
        {
            var checkpointPath = Path.Combine(root, "nested", "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "nested", "orchestration-reasoning-manifest.json");
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(
                CreateDescriptor([CreatePattern()]),
                Path.Combine(root, "nested", "source-admission")));

            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath);
            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            var manifestJson = await File.ReadAllTextAsync(exportResult.Value!.ManifestRef);
            var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(manifestJson, JsonOptions)!;

            Assert.Equal("orchestration-reasoning-checkpoint.json", manifest.CheckpointRef);
            Assert.Equal("orchestration-reasoning-manifest.json", manifest.ManifestRef);
            Assert.Equal("orchestration-reasoning-checkpoint.source-admission-proof.json", manifest.AdmissionProofRef);
            Assert.True(Path.IsPathRooted(exportResult.Value.CheckpointRef));
            Assert.True(Path.IsPathRooted(exportResult.Value.ManifestRef));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_SourceAdmissionProofRefsArePortableAndRelocatable()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-proof-portable-{Guid.NewGuid():N}");
        var relocatedRoot = Path.Combine(Path.GetTempPath(), $"or-proof-portable-relocated-{Guid.NewGuid():N}");
        var assetRoot = Path.Combine(root, "asset-root");
        var relocatedAssetRoot = Path.Combine(relocatedRoot, "asset-root");
        try
        {
            var checkpointDirectory = Path.Combine(assetRoot, "checkpoints", "orchestration-reasoning-v1");
            var sourceRoot = Path.Combine(checkpointDirectory, "source-admission");
            var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                root: sourceRoot,
                learnedPattern: CreatePattern());
            var materialized = await new OrchestrationReasoningTrainer()
                .MaterializeAsync(
                    corpus.PayloadPath,
                    corpus.ManifestPath,
                    corpus.LedgerPath,
                    corpus.EvidencePath,
                    learnedStateManifestRef: "source-admission/manifest.json");
            Assert.True(materialized.IsSuccess, materialized.ErrorMessage);

            var source = new OrchestrationReasoningMachine();
            var imported = source.ImportAdmittedState(materialized.Value!.AdmittedState);
            Assert.True(imported.IsSuccess, imported.ErrorMessage);

            var checkpointPath = Path.Combine(checkpointDirectory, "checkpoint.json");
            var manifestPath = Path.Combine(checkpointDirectory, "manifest.json");
            var exportResult = await source.SaveCheckpointAsync(checkpointPath, manifestPath);
            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);

            var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(exportResult.Value!.CheckpointRef);
            var proofJson = await File.ReadAllTextAsync(proofPath);
            AssertProofOmitsAbsolutePath(proofJson, assetRoot);
            Assert.Contains("source-admission/payload.jsonl", proofJson, StringComparison.Ordinal);
            Assert.Contains("source-admission/manifest.json", proofJson, StringComparison.Ordinal);
            Assert.Contains("source-admission/source-admission-ledger.json", proofJson, StringComparison.Ordinal);
            Assert.Contains("source-admission/source-admission-evidence.json", proofJson, StringComparison.Ordinal);
            Assert.DoesNotContain("../", proofJson, StringComparison.Ordinal);
            AssertProofSourceMaterialRefsAreRelativeForwardSlash(proofJson);

            Directory.CreateDirectory(relocatedRoot);
            Directory.Move(assetRoot, relocatedAssetRoot);

            var relocatedCheckpointPath = Path.Combine(relocatedAssetRoot, "checkpoints", "orchestration-reasoning-v1", "checkpoint.json");
            var relocatedManifestPath = Path.Combine(relocatedAssetRoot, "checkpoints", "orchestration-reasoning-v1", "manifest.json");
            var relocatedLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(relocatedCheckpointPath, relocatedManifestPath);
            Assert.True(relocatedLoad.IsSuccess, relocatedLoad.ErrorMessage);

            var relocatedPayloadPath = Path.Combine(relocatedAssetRoot, "checkpoints", "orchestration-reasoning-v1", "source-admission", "payload.jsonl");
            await File.AppendAllTextAsync(relocatedPayloadPath, " \n");
            var tamperedLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(relocatedCheckpointPath, relocatedManifestPath);
            Assert.False(tamperedLoad.IsSuccess);
            Assert.Equal(OrchestrationReasoningSourceAdmission.AdmissionRejectedCode, tamperedLoad.ErrorCode);

            File.Delete(relocatedPayloadPath);
            var missingLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(relocatedCheckpointPath, relocatedManifestPath);
            Assert.False(missingLoad.IsSuccess);
            Assert.Equal(OrchestrationReasoningSourceAdmission.AdmissionRejectedCode, missingLoad.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(relocatedRoot))
                Directory.Delete(relocatedRoot, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_SaveCopiesExternalAdmittedSourceMaterialsIntoCheckpointBundle()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-proof-save-contained-{Guid.NewGuid():N}");
        try
        {
            var externalSourceRoot = Path.Combine(root, "external-source-admission");
            var checkpointDirectory = Path.Combine(root, "checkpoint-bundle");
            var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                root: externalSourceRoot,
                learnedPattern: CreatePattern());
            var materialized = await new OrchestrationReasoningTrainer()
                .MaterializeAsync(corpus.PayloadPath, corpus.ManifestPath, corpus.LedgerPath, corpus.EvidencePath);
            Assert.True(materialized.IsSuccess, materialized.ErrorMessage);

            var source = new OrchestrationReasoningMachine();
            var imported = source.ImportAdmittedState(materialized.Value!.AdmittedState);
            Assert.True(imported.IsSuccess, imported.ErrorMessage);

            var checkpointPath = Path.Combine(checkpointDirectory, "checkpoint.json");
            var manifestPath = Path.Combine(checkpointDirectory, "manifest.json");

            // Внешние исходники допустимы только как уже проверенный снимок.
            // После сохранения bundle обязан жить без этой исходной папки.
            var sourceFiles = new Dictionary<string, (string SourcePath, string ExpectedRef)>
            {
                ["payload_ref"] = (corpus.PayloadPath, "source-materials/01/payload.jsonl"),
                ["manifest_ref"] = (corpus.ManifestPath, "source-materials/01/manifest.json"),
                ["ledger_ref"] = (corpus.LedgerPath, "source-materials/01/source-admission-ledger.json"),
                ["evidence_ref"] = (corpus.EvidencePath, "source-materials/01/source-admission-evidence.json")
            };
            var sourceHashes = sourceFiles.ToDictionary(
                item => item.Key,
                item => OrchestrationReasoningSourceAdmissionTestFixture.HashFile(item.Value.SourcePath));

            var exportResult = await source.SaveCheckpointAsync(checkpointPath, manifestPath);

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);

            var checkpoint = await ReadCheckpointAsync(exportResult.Value!.CheckpointRef);
            var manifest = await ReadManifestAsync(exportResult.Value.ManifestRef);
            var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(exportResult.Value.CheckpointRef);
            var proofJson = await File.ReadAllTextAsync(proofPath);
            using var proofDocument = JsonDocument.Parse(proofJson);
            var material = proofDocument.RootElement.GetProperty("source_materials").EnumerateArray().Single();

            // Checkpoint, manifest и proof не должны ссылаться на абсолютный
            // временный corpus. Они описывают только переносимый каталог bundle.
            Assert.Equal("checkpoint.json", manifest.CheckpointRef);
            Assert.Equal("manifest.json", manifest.ManifestRef);
            Assert.Equal("checkpoint.source-admission-proof.json", checkpoint.AdmissionProofRef);
            Assert.Equal(checkpoint.AdmissionProofRef, manifest.AdmissionProofRef);
            Assert.Equal("source-materials/01/manifest.json", checkpoint.SourceManifestRef);
            Assert.Equal(checkpoint.SourceManifestRef, manifest.SourceManifestRef);
            AssertProofOmitsAbsolutePath(proofJson, externalSourceRoot);

            foreach (var (propertyName, sourceFile) in sourceFiles)
            {
                var reference = material.GetProperty(propertyName).GetString()!;
                AssertPortableBundleSourceRef(reference, sourceFile.ExpectedRef);

                // Каждая ссылка proof должна вести к реальной копии внутри
                // checkpoint bundle, а не к исходной папке admission corpus.
                var bundledPath = Path.Combine(
                    checkpointDirectory,
                    reference.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(bundledPath), bundledPath);
                Assert.Equal(
                    sourceHashes[propertyName],
                    OrchestrationReasoningSourceAdmissionTestFixture.HashFile(bundledPath));
            }

            Directory.Delete(externalSourceRoot, recursive: true);

            // Fresh machine/load после удаления исходников доказывает, что в
            // bundle нет скрытого fallback к прежнему внешнему каталогу.
            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                exportResult.Value.CheckpointRef,
                exportResult.Value.ManifestRef);
            var planned = loaded.Value?.Machine.Plan(CreateLearnedRequest("sha256:or-life-contained-save"));

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.True(planned!.IsSuccess, planned.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, planned.Value!.ContractStatus);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("payload_ref", @"C:\outside\payload.jsonl")]
    [InlineData("manifest_ref", @"\\server\share\manifest.json")]
    [InlineData("ledger_ref", "/var/tmp/source-admission-ledger.json")]
    [InlineData("evidence_ref", "../outside/source-admission-evidence.json")]
    public async Task OrchestrationReasoningMachineAssetLifecycle_LoadRejectsProofSourceRefsOutsideCheckpointBundleBeforeSourceRead(
        string propertyName,
        string replacementRef)
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-proof-load-contained-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            await RewriteProofSourceMaterialRefAndRebindAsync(paths.CheckpointPath, paths.ManifestPath, propertyName, replacementRef);

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(paths.CheckpointPath, paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal(OrchestrationReasoningSourceAdmission.ProofSourceMaterialOutsideBundleCode, loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_LoadRejectsMergedManifestRefThatDisagreesWithProofManifestRef()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-proof-logical-ref-mismatch-{Guid.NewGuid():N}");
        try
        {
            var checkpointDirectory = Path.Combine(root, "checkpoint-bundle");
            var firstCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                root: Path.Combine(checkpointDirectory, "source-admission", "pattern-01"),
                pairId: "fixture-pair-logical-ref-001",
                sourceRowId: "fixture-row-logical-ref-001",
                sourceId: "fixture-source-logical-ref-001",
                learnedPattern: CreatePattern());
            var secondCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                root: Path.Combine(checkpointDirectory, "source-admission", "pattern-02"),
                pairId: "fixture-pair-logical-ref-002",
                sourceRowId: "fixture-row-logical-ref-002",
                sourceId: "fixture-source-logical-ref-002",
                learnedPattern: CreatePattern() with
                {
                    PatternId = "or-pattern-logical-ref-delta",
                    ScenarioRef = "scenario:logical-ref-delta"
                });
            var first = await new OrchestrationReasoningTrainer().MaterializeAsync(
                firstCorpus.PayloadPath,
                firstCorpus.ManifestPath,
                firstCorpus.LedgerPath,
                firstCorpus.EvidencePath,
                learnedStateManifestRef: "source-admission/pattern-01/manifest.json");
            var second = await new OrchestrationReasoningTrainer().MaterializeAsync(
                secondCorpus.PayloadPath,
                secondCorpus.ManifestPath,
                secondCorpus.LedgerPath,
                secondCorpus.EvidencePath,
                learnedStateManifestRef: "source-admission/pattern-02/manifest.json");
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);

            var source = new OrchestrationReasoningMachine();
            Assert.True(source.ImportAdmittedState(first.Value!.AdmittedState).IsSuccess);
            var continued = source.ContinueTrainingFromLearnedState(
                second.Value!.AdmittedState,
                second.Value.AdmittedState.Receipt.PairIds);
            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            var checkpointPath = Path.Combine(checkpointDirectory, "checkpoint.json");
            var manifestPath = Path.Combine(checkpointDirectory, "manifest.json");
            var export = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                continued.Value!.MergedLearnedState.ManifestId,
                continued.Value.MergedAcceptedRefIds);
            Assert.True(export.IsSuccess, export.ErrorMessage);

            await RewriteProofSourceMaterialRefAndRebindAsync(
                export.Value!.CheckpointRef,
                export.Value.ManifestRef,
                "manifest_ref",
                "source-admission/pattern-99/manifest.json",
                materialIndex: 1);

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                export.Value.CheckpointRef,
                export.Value.ManifestRef);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode,
                loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_LoadRejectsLinuxProofSourceMaterialSymlinkEscapeWithMatchingBytes()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(Path.GetTempPath(), $"or-proof-linux-symlink-escape-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            var checkpointDirectory = Path.GetDirectoryName(paths.CheckpointPath)!;
            var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(paths.CheckpointPath);
            var proof = JsonNode.Parse(await File.ReadAllTextAsync(proofPath))!.AsObject();
            var payloadRef = proof["source_materials"]!.AsArray()[0]!["payload_ref"]!.GetValue<string>();
            var bundledPayloadPath = Path.Combine(
                checkpointDirectory,
                payloadRef.Replace('/', Path.DirectorySeparatorChar));
            var originalBundledPayloadBytes = await File.ReadAllBytesAsync(bundledPayloadPath);

            var outsideDirectory = Path.Combine(root, "outside-source-material");
            Directory.CreateDirectory(outsideDirectory);
            var outsidePayloadPath = Path.Combine(outsideDirectory, "payload.jsonl");
            await File.WriteAllBytesAsync(outsidePayloadPath, originalBundledPayloadBytes);
            Assert.Equal(
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(bundledPayloadPath),
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(outsidePayloadPath));

            File.Delete(bundledPayloadPath);
            File.CreateSymbolicLink(bundledPayloadPath, outsidePayloadPath);

            Assert.True(File.Exists(bundledPayloadPath));
            Assert.Equal(originalBundledPayloadBytes, await File.ReadAllBytesAsync(bundledPayloadPath));

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                paths.CheckpointPath,
                paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.ProofSourceMaterialOutsideBundleCode,
                loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_MismatchedManifestSourceRefFailsLoad()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-source-mismatch-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            var manifest = await ReadManifestAsync(paths.ManifestPath);
            await WriteManifestAsync(paths.ManifestPath, manifest with
            {
                SourceManifestRef = "training-data/orchestration-reasoning/different-source-manifest.json"
            });

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(paths.CheckpointPath, paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_MANIFEST_SOURCE_REF_MISMATCH", loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_MismatchedManifestPatternCountFailsLoad()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-pattern-count-mismatch-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            var manifest = await ReadManifestAsync(paths.ManifestPath);
            await WriteManifestAsync(paths.ManifestPath, manifest with
            {
                LearnedPatternCount = manifest.LearnedPatternCount + 1
            });

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(paths.CheckpointPath, paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_MANIFEST_PATTERN_COUNT_MISMATCH", loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_FileBackedLoaderFreshLoadsCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-fresh-load-{Guid.NewGuid():N}");
        try
        {
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(
                CreateDescriptor([CreatePattern()]),
                Path.Combine(root, "source-admission")));
            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath);

            var original = source.Plan(CreateLearnedRequest("sha256:or-life-original"));
            var fresh = new OrchestrationReasoningMachine(new FileBackedOrchestrationReasoningLearnedGuideStateLoader(
                exportResult.Value!.CheckpointRef,
                exportResult.Value.ManifestRef));
            var freshResult = fresh.Plan(CreateLearnedRequest("sha256:or-life-fresh"));

            Assert.True(original.IsSuccess, original.ErrorMessage);
            Assert.True(freshResult.IsSuccess, freshResult.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, freshResult.Value!.ContractStatus);
            Assert.Equal(original.Value!.ScenarioPlan.ScenarioId, freshResult.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(original.Value.ScenarioPlan.SelectedLearnedPatterns, freshResult.Value.ScenarioPlan.SelectedLearnedPatterns);
            Assert.Contains("INFERENCE_SOURCE:LOADED_LEARNED_GUIDE_STATE", freshResult.Value.ContractEvidence);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_NoStateAndNoPatternAblationsRemainNoSemanticPlan()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-ablation-{Guid.NewGuid():N}");
        try
        {
            var noState = new OrchestrationReasoningMachine().Plan(CreateLearnedRequest("sha256:or-life-no-state"));
            var noPatterns = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([])))
                .Plan(CreateLearnedRequest("sha256:or-life-no-patterns"));
            var sourceWithoutPatterns = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([])));
            var exportResult = await sourceWithoutPatterns.SaveCheckpointAsync(
                Path.Combine(root, "checkpoint.json"),
                Path.Combine(root, "manifest.json"));

            Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, noState.Value!.ContractStatus);
            Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, noPatterns.Value!.ContractStatus);
            Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, noState.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, noPatterns.Value.ScenarioPlan.ScenarioId);
            Assert.False(exportResult.IsSuccess);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode,
                exportResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_CheckpointAndManifestDoNotClaimOutOfScopeReadiness()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-claims-{Guid.NewGuid():N}");
        try
        {
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(
                CreateDescriptor([CreatePattern()]),
                Path.Combine(root, "source-admission")));
            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath);

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                exportResult.Value!.CheckpointRef,
                exportResult.Value.ManifestRef);
            var checkpointJson = await File.ReadAllTextAsync(exportResult.Value.CheckpointRef);
            var manifestJson = await File.ReadAllTextAsync(exportResult.Value.ManifestRef);

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.False(loaded.Value!.Checkpoint.SemanticQualityClaimed);
            Assert.False(loaded.Value.Checkpoint.ContinueTrainingClaimed);
            Assert.False(loaded.Value.Checkpoint.HostRuntimeClaimed);
            Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);
            Assert.False(loaded.Value.Manifest.ContinueTrainingClaimed);
            Assert.False(loaded.Value.Manifest.HostRuntimeClaimed);
            Assert.DoesNotContain("sensitive-local-wording", checkpointJson);
            Assert.DoesNotContain("sensitive-local-wording", manifestJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static OrchestrationReasoningLearnedGuideStateDescriptor CreateDescriptor(
        IReadOnlyList<OrchestrationReasoningLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "orchestration-reasoning-state:v1:lifecycle-test",
            GuideStateRef: "guide-state:orchestration-reasoning:common:v1",
            ManifestId: "models/orchestration-reasoning/orchestration-reasoning-v1/manifest",
            ManifestSha256: "sha256:or-life-manifest-test",
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: "commit:or-life-test",
            StateSha256: "sha256:or-life-state-test",
            LearnedPatterns: learnedPatterns);

    private static OrchestrationReasoningLearnedPattern CreatePattern()
        => new(
            PatternId: "or-pattern-bounded-implementation-review",
            ScenarioRef: "scenario:bounded-implementation-review-loop",
            RequiredTaskTypes: ["bounded_implementation_slice"],
            RequiredConstraintSignals: ["no host repos", "no external AI"],
            RequiredRequirementSignals: ["parent review required"],
            RequiredDeliverableSignals: ["focused test evidence"],
            RequiredEvidenceSignals: ["ablation evidence"],
            PhaseFamilies: ["implementation", "review"],
            GateFamilies: ["parent-review", "completion-matrix", "no-hidden-determinism"],
            AssetFamilies: ["llm_mvp_core"],
            HostCheckSets: ["codex"],
            RiskFlags: ["no_hidden_determinism_review_required", "no_training_claim"],
            NextGranularizationActions: ["write completion matrix before parent review", "prepare commit boundary only after acceptance"],
            Confidence: 0.93);

    private static OrchestrationReasoningRequest CreateLearnedRequest(string rawPromptSha256)
        => new(
            RequestId: $"orch-life-request-{rawPromptSha256}",
            Task: new StructuredTaskContract(
                PromptId: "prompt-or-life-001",
                RawPromptSha256: rawPromptSha256,
                UserLanguage: "en",
                TaskType: "bounded_implementation_slice",
                ExplicitUserRequirements: ["parent review required"],
                Constraints: ["no host repos", "no external AI"],
                Deliverables: ["focused test evidence"],
                ForbiddenActions: ["training runs", "commit/push"],
                RequiredHosts: ["claude", "cursor", "codex"],
                EvidenceNeeds: ["ablation evidence"],
                SourceProvenance: new Dictionary<string, string>
                {
                    ["source_root_id"] = "or-04a-temp-fixture",
                    ["fixture_text_sha256"] = "sha256:fixture-only"
                },
                SplitId: "split-or-life-01"),
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static async Task<(string CheckpointPath, string ManifestPath)> SaveValidCheckpointAsync(string root)
    {
        var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
        var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
        var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(
            CreateDescriptor([CreatePattern()]),
            Path.Combine(root, "source-admission")));
        var exportResult = await source.SaveCheckpointAsync(
            checkpointPath,
            manifestPath);

        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
        return (exportResult.Value!.CheckpointRef, exportResult.Value.ManifestRef);
    }

    private static async Task<OrchestrationReasoningCheckpointManifest> ReadManifestAsync(string manifestPath)
        => JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(
            await File.ReadAllTextAsync(manifestPath),
            JsonOptions)!;

    private static async Task<OrchestrationReasoningCheckpoint> ReadCheckpointAsync(string checkpointPath)
        => JsonSerializer.Deserialize<OrchestrationReasoningCheckpoint>(
            await File.ReadAllTextAsync(checkpointPath),
            JsonOptions)!;

    private static Task WriteManifestAsync(
        string manifestPath,
        OrchestrationReasoningCheckpointManifest manifest)
        => File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions));

    private static Task WriteCheckpointAsync(
        string checkpointPath,
        OrchestrationReasoningCheckpoint checkpoint)
        => File.WriteAllTextAsync(checkpointPath, JsonSerializer.Serialize(checkpoint, JsonOptions));

    private static async Task RewriteProofSourceMaterialRefAndRebindAsync(
        string checkpointPath,
        string manifestPath,
        string propertyName,
        string replacementRef,
        int materialIndex = 0)
    {
        var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(checkpointPath);
        var proof = JsonNode.Parse(await File.ReadAllTextAsync(proofPath))!.AsObject();
        proof["source_materials"]!.AsArray()[materialIndex]![propertyName] = replacementRef;
        await File.WriteAllTextAsync(proofPath, proof.ToJsonString(JsonOptions));

        var proofSha = OrchestrationReasoningCanonicalState.HashBytes(await File.ReadAllBytesAsync(proofPath));
        var checkpoint = await ReadCheckpointAsync(checkpointPath);
        await WriteCheckpointAsync(checkpointPath, checkpoint with
        {
            AdmissionProofSha256 = proofSha
        });

        var checkpointSha = OrchestrationReasoningCanonicalState.HashBytes(await File.ReadAllBytesAsync(checkpointPath));
        var manifest = await ReadManifestAsync(manifestPath);
        await WriteManifestAsync(manifestPath, manifest with
        {
            AdmissionProofSha256 = proofSha,
            CheckpointSha256 = checkpointSha
        });
    }

    private static void AssertProofOmitsAbsolutePath(string proofJson, string originalRoot)
    {
        var fullRoot = Path.GetFullPath(originalRoot);
        Assert.DoesNotContain(fullRoot, proofJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fullRoot.Replace('\\', '/'), proofJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fullRoot.Replace("\\", "\\\\"), proofJson, StringComparison.OrdinalIgnoreCase);

        var driveRoot = Path.GetPathRoot(fullRoot);
        if (!string.IsNullOrWhiteSpace(driveRoot) && driveRoot != Path.DirectorySeparatorChar.ToString())
        {
            Assert.DoesNotContain(driveRoot, proofJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(driveRoot.Replace("\\", "\\\\"), proofJson, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertProofSourceMaterialRefsAreRelativeForwardSlash(string proofJson)
    {
        using var document = JsonDocument.Parse(proofJson);
        var materials = document.RootElement.GetProperty("source_materials")
            .EnumerateArray()
            .ToArray();
        Assert.NotEmpty(materials);

        foreach (var material in materials)
        {
            foreach (var propertyName in new[] { "payload_ref", "manifest_ref", "ledger_ref", "evidence_ref" })
            {
                var reference = material.GetProperty(propertyName).GetString();
                Assert.False(string.IsNullOrWhiteSpace(reference));
                Assert.False(Path.IsPathRooted(reference), reference);
                Assert.DoesNotContain('\\', reference);
                Assert.DoesNotContain("../", reference, StringComparison.Ordinal);
                Assert.Contains('/', reference);
            }
        }
    }

    private static void AssertPortableBundleSourceRef(string reference, string expectedReference)
    {
        // Читаемый контракт для proof refs: относительный путь с прямыми
        // слешами, без Windows/Unix root и без выхода через родительский каталог.
        Assert.Equal(expectedReference, reference);
        Assert.False(Path.IsPathRooted(reference), reference);
        Assert.DoesNotContain('\\', reference);
        Assert.DoesNotContain("../", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("..\\", reference, StringComparison.Ordinal);
        Assert.StartsWith("source-materials/01/", reference, StringComparison.Ordinal);
    }

    private sealed class StaticOrchestrationReasoningGuideStateLoader(
        OrchestrationReasoningLearnedGuideStateDescriptor descriptor,
        string? sourceRoot = null)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public AgentInfrastructure.OperationResult<OrchestrationReasoningAdmittedLearnedState> Load()
            => descriptor.LearnedPatterns is null || descriptor.LearnedPatterns.Count == 0
                ? AgentInfrastructure.OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                    "A verified file-backed state requires at least one learned pattern.",
                    OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode)
                : AgentInfrastructure.OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(
                    OrchestrationReasoningSourceAdmissionTestFixture.Admit(descriptor, descriptor.StateId, sourceRoot));
    }
}
