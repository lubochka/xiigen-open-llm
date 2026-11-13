// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningR17CommandBoundaryTests
{
    [Fact]
    public async Task MaterializeCli_AdmissionFailurePreservesCallerOutputAndUsesStructuredStderr()
    {
        var root = NewRoot();
        try
        {
            var repoRoot = RepoRoot();
            var output = Path.Combine(root, "materialized.json");
            await File.WriteAllTextAsync(output, "caller-sentinel");
            var result = await RunDotnetAsync(
                Path.Combine(
                    repoRoot,
                    "27.TranslationDomain.Tools",
                    "bin",
                    TestBuildConfiguration,
                    "net9.0",
                    "27.TranslationDomain.Tools.dll"),
                "materialize-orchestration-reasoning-state",
                "--payloads",
                Path.Combine(
                    repoRoot,
                    "training-data",
                    "orchestration-reasoning",
                    "orchestration-reasoning-dpo-trainer-payloads.jsonl"),
                "--manifest",
                Path.Combine(
                    repoRoot,
                    "training-data",
                    "orchestration-reasoning",
                    "orchestration-reasoning-dpo-trainer-payloads-manifest.json"),
                "--output",
                output);

            Assert.Equal(3, result.ExitCode);
            Assert.Equal("caller-sentinel", await File.ReadAllTextAsync(output));
            Assert.True(string.IsNullOrWhiteSpace(result.StandardOutput));
            using var error = JsonDocument.Parse(result.StandardError);
            Assert.Equal(
                "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED",
                error.RootElement.GetProperty("error_code").GetString());
            Assert.False(error.RootElement.GetProperty("passed").GetBoolean());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task PersistencePackageCliLifecycle_MaterializesExportsLoadsContinuesAndReloads()
    {
        var root = NewRoot();
        try
        {
            var tool = TranslationToolAssembly();
            using var baseCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "base"),
                    pairId: "cli-base-pair-001",
                    sourceRowId: "cli-base-row-001",
                    sourceId: "cli-base-source-001",
                    sourceRef: "fixture://cli/base",
                    learnedPattern: CliPattern("base"));
            using var deltaCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "delta"),
                    pairId: "cli-delta-pair-001",
                    sourceRowId: "cli-delta-row-001",
                    sourceId: "cli-delta-source-001",
                    sourceRef: "fixture://cli/delta",
                    learnedPattern: CliPattern("delta"));
            var baseReport = Path.Combine(root, "base-materialized.json");
            var deltaReport = Path.Combine(root, "delta-materialized.json");

            AssertCommandPassed(await RunMaterializeCliAsync(tool, baseCorpus, baseReport));
            AssertCommandPassed(await RunMaterializeCliAsync(tool, deltaCorpus, deltaReport));
            var baseReportText = await File.ReadAllTextAsync(baseReport);
            Assert.Contains("\"admitted_state_package\"", baseReportText, StringComparison.Ordinal);
            Assert.DoesNotContain("\"admitted_state_envelope\"", baseReportText, StringComparison.Ordinal);

            var checkpoint = Path.Combine(root, "base.checkpoint.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            AssertCommandPassed(await RunDotnetAsync(
                tool,
                "export-orchestration-reasoning-model",
                "--learned-state-report",
                baseReport,
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest));
            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpoint, manifest);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Single(loaded.Value!.Checkpoint.LearnedState.LearnedPatterns);
            Assert.Equal(
                ["cli-base-pair-001"],
                loaded.Value.Checkpoint.AcceptedRefIds.Order(StringComparer.Ordinal).ToArray());

            var continueReport = Path.Combine(root, "continue-report.json");
            AssertCommandPassed(await RunDotnetAsync(
                tool,
                "continue-orchestration-reasoning-training",
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest,
                "--delta-learned-state-report",
                deltaReport,
                "--output",
                continueReport));
            var continueReportText = await File.ReadAllTextAsync(continueReport);
            Assert.Contains("\"admitted_state_package\"", continueReportText, StringComparison.Ordinal);
            Assert.DoesNotContain("\"admitted_state_envelope\"", continueReportText, StringComparison.Ordinal);

            var continuedCheckpoint = Path.Combine(root, "continued.checkpoint.json");
            var continuedManifest = Path.Combine(root, "continued.manifest.json");
            var continuedExportReport = Path.Combine(root, "continued-export-report.json");
            AssertCommandPassed(await RunDotnetAsync(
                tool,
                "export-orchestration-reasoning-continued-model",
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest,
                "--delta-learned-state-report",
                deltaReport,
                "--continued-checkpoint",
                continuedCheckpoint,
                "--continued-manifest",
                continuedManifest,
                "--output",
                continuedExportReport));

            var continuedLoaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                continuedCheckpoint,
                continuedManifest);
            Assert.True(continuedLoaded.IsSuccess, continuedLoaded.ErrorMessage);
            Assert.Equal(2, continuedLoaded.Value!.Checkpoint.LearnedState.LearnedPatterns.Count);
            Assert.Equal(
                ["cli-base-pair-001", "cli-delta-pair-001"],
                continuedLoaded.Value.Checkpoint.AcceptedRefIds.Order(StringComparer.Ordinal).ToArray());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TamperedPersistencePackage_FailsEveryCliBoundaryWithoutWrites()
    {
        var root = NewRoot();
        try
        {
            var tool = TranslationToolAssembly();
            using var baseCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "base"),
                    pairId: "cli-tamper-base-pair",
                    sourceRowId: "cli-tamper-base-row",
                    sourceId: "cli-tamper-base-source",
                    sourceRef: "fixture://cli/tamper/base");
            using var deltaCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "delta"),
                    pairId: "cli-tamper-delta-pair",
                    sourceRowId: "cli-tamper-delta-row",
                    sourceId: "cli-tamper-delta-source",
                    sourceRef: "fixture://cli/tamper/delta");
            var baseReport = Path.Combine(root, "base-materialized.json");
            var deltaReport = Path.Combine(root, "delta-materialized.json");
            AssertCommandPassed(await RunMaterializeCliAsync(tool, baseCorpus, baseReport));
            AssertCommandPassed(await RunMaterializeCliAsync(tool, deltaCorpus, deltaReport));
            var tamperedBaseReport = await WriteTamperedPackageAsync(
                baseReport,
                Path.Combine(root, "base-tampered.json"));
            var tamperedDeltaReport = await WriteTamperedPackageAsync(
                deltaReport,
                Path.Combine(root, "delta-tampered.json"));

            var rejectedCheckpoint = Path.Combine(root, "rejected.checkpoint.json");
            var rejectedManifest = Path.Combine(root, "rejected.manifest.json");
            await File.WriteAllTextAsync(rejectedCheckpoint, "checkpoint-sentinel");
            await File.WriteAllTextAsync(rejectedManifest, "manifest-sentinel");
            var rejectedExport = await RunDotnetAsync(
                tool,
                "export-orchestration-reasoning-model",
                "--learned-state-report",
                tamperedBaseReport,
                "--checkpoint",
                rejectedCheckpoint,
                "--manifest",
                rejectedManifest);
            AssertAdmissionFailure(rejectedExport);
            Assert.Equal("checkpoint-sentinel", await File.ReadAllTextAsync(rejectedCheckpoint));
            Assert.Equal("manifest-sentinel", await File.ReadAllTextAsync(rejectedManifest));
            Assert.False(File.Exists(OrchestrationReasoningSourceAdmission.GetProofPath(rejectedCheckpoint)));

            var checkpoint = Path.Combine(root, "base.checkpoint.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            AssertCommandPassed(await RunDotnetAsync(
                tool,
                "export-orchestration-reasoning-model",
                "--learned-state-report",
                baseReport,
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest));
            var checkpointSha = OrchestrationReasoningSourceAdmissionTestFixture.HashFile(checkpoint);
            var manifestSha = OrchestrationReasoningSourceAdmissionTestFixture.HashFile(manifest);

            var continueOutput = Path.Combine(root, "continue-output.json");
            await File.WriteAllTextAsync(continueOutput, "continue-sentinel");
            var rejectedContinue = await RunDotnetAsync(
                tool,
                "continue-orchestration-reasoning-training",
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest,
                "--delta-learned-state-report",
                tamperedDeltaReport,
                "--output",
                continueOutput);
            AssertAdmissionFailure(rejectedContinue);
            Assert.Equal("continue-sentinel", await File.ReadAllTextAsync(continueOutput));
            Assert.Equal(checkpointSha, OrchestrationReasoningSourceAdmissionTestFixture.HashFile(checkpoint));
            Assert.Equal(manifestSha, OrchestrationReasoningSourceAdmissionTestFixture.HashFile(manifest));

            var continuedCheckpoint = Path.Combine(root, "rejected-continued.checkpoint.json");
            var continuedManifest = Path.Combine(root, "rejected-continued.manifest.json");
            var continuedOutput = Path.Combine(root, "continued-output.json");
            await File.WriteAllTextAsync(continuedCheckpoint, "continued-checkpoint-sentinel");
            await File.WriteAllTextAsync(continuedManifest, "continued-manifest-sentinel");
            await File.WriteAllTextAsync(continuedOutput, "continued-output-sentinel");
            var rejectedContinuedExport = await RunDotnetAsync(
                tool,
                "export-orchestration-reasoning-continued-model",
                "--checkpoint",
                checkpoint,
                "--manifest",
                manifest,
                "--delta-learned-state-report",
                tamperedDeltaReport,
                "--continued-checkpoint",
                continuedCheckpoint,
                "--continued-manifest",
                continuedManifest,
                "--output",
                continuedOutput);
            AssertAdmissionFailure(rejectedContinuedExport);
            Assert.Equal(
                "continued-checkpoint-sentinel",
                await File.ReadAllTextAsync(continuedCheckpoint));
            Assert.Equal(
                "continued-manifest-sentinel",
                await File.ReadAllTextAsync(continuedManifest));
            Assert.Equal("continued-output-sentinel", await File.ReadAllTextAsync(continuedOutput));
            Assert.False(File.Exists(
                OrchestrationReasoningSourceAdmission.GetProofPath(continuedCheckpoint)));
            Assert.Equal(checkpointSha, OrchestrationReasoningSourceAdmissionTestFixture.HashFile(checkpoint));
            Assert.Equal(manifestSha, OrchestrationReasoningSourceAdmissionTestFixture.HashFile(manifest));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Phase05Runner_ContinuedExportUsesMergedAdmissionProvenance()
    {
        var root = NewRoot();
        try
        {
            using var baseCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "base"),
                    pairId: "phase05-base-pair",
                    sourceRowId: "phase05-base-source-row",
                    sourceId: "phase05-base-source",
                    sourceRef: "fixture://phase05/base-source",
                    learnedPattern: CliPattern("phase05_base"));
            using var deltaCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "delta"),
                    pairId: "phase05-delta-pair",
                    sourceRowId: "phase05-delta-source-row",
                    sourceId: "phase05-delta-source",
                    sourceRef: "fixture://phase05/delta-source",
                    learnedPattern: CliPattern("phase05_delta"));
            var heldout = await WritePhase05HeldoutFixtureAsync(Path.Combine(root, "heldout"));
            var output = Path.Combine(root, "phase05-output");

            var result = await RunDotnetAsync(
                Phase05RunnerAssembly(),
                "--output",
                output,
                "--base-payload",
                baseCorpus.PayloadPath,
                "--base-manifest",
                baseCorpus.ManifestPath,
                "--base-source-admission-ledger",
                baseCorpus.LedgerPath,
                "--base-source-admission-evidence",
                baseCorpus.EvidencePath,
                "--delta-payload",
                deltaCorpus.PayloadPath,
                "--delta-manifest",
                deltaCorpus.ManifestPath,
                "--delta-source-admission-ledger",
                deltaCorpus.LedgerPath,
                "--delta-source-admission-evidence",
                deltaCorpus.EvidencePath,
                "--heldout",
                heldout.PayloadPath,
                "--heldout-manifest",
                heldout.ManifestPath,
                "--focused-test-result",
                "PASS");

            AssertCommandPassed(result);
            var baseCheckpoint = Path.Combine(output, "orchestration-reasoning-checkpoint.json");
            var baseManifest = Path.Combine(output, "checkpoint-manifest.json");
            var baseProof = OrchestrationReasoningSourceAdmission.GetProofPath(baseCheckpoint);
            var continuedCheckpoint = Path.Combine(output, "orchestration-reasoning-continued-checkpoint.json");
            var continuedManifest = Path.Combine(output, "orchestration-reasoning-continued-manifest.json");
            var continuedProof = OrchestrationReasoningSourceAdmission.GetProofPath(continuedCheckpoint);
            var lifecyclePaths = new[]
            {
                baseCheckpoint,
                baseManifest,
                baseProof,
                continuedCheckpoint,
                continuedManifest,
                continuedProof
            };
            Assert.Equal(
                lifecyclePaths.Length,
                lifecyclePaths.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.All(lifecyclePaths, path => Assert.True(File.Exists(path), $"Expected lifecycle artifact at {path}."));
            Assert.NotEqual(
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(baseCheckpoint),
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(continuedCheckpoint));
            Assert.NotEqual(
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(baseManifest),
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(continuedManifest));
            Assert.NotEqual(
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(baseProof),
                OrchestrationReasoningSourceAdmissionTestFixture.HashFile(continuedProof));

            var baseLoaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                baseCheckpoint,
                baseManifest);
            var continuedLoaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                continuedCheckpoint,
                continuedManifest);

            Assert.True(baseLoaded.IsSuccess, baseLoaded.ErrorMessage);
            Assert.True(continuedLoaded.IsSuccess, continuedLoaded.ErrorMessage);
            Assert.Equal(
                ["phase05-base-pair"],
                baseLoaded.Value!.Checkpoint.AcceptedRefIds.Order(StringComparer.Ordinal).ToArray());
            var expectedContinuedPairIds = new[] { "phase05-base-pair", "phase05-delta-pair" };
            Assert.Equal(
                expectedContinuedPairIds,
                continuedLoaded.Value!.Checkpoint.AcceptedRefIds.Order(StringComparer.Ordinal).ToArray());
            Assert.NotNull(continuedLoaded.Value.Checkpoint.AdmissionReceipt);
            Assert.Equal(
                expectedContinuedPairIds,
                continuedLoaded.Value.Checkpoint.AdmissionReceipt!.PairIds.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(
                continuedLoaded.Value.Checkpoint.LearnedState.ManifestId,
                continuedLoaded.Value.Checkpoint.SourceManifestRef);
            Assert.Equal(
                ["cli-phase05_base-pattern", "cli-phase05_delta-pattern"],
                continuedLoaded.Value.Checkpoint.LearnedState.LearnedPatterns!
                    .Select(pattern => pattern.PatternId)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

            var rejectedCheckpoint = Path.Combine(root, "delta-only.checkpoint.json");
            var rejectedManifest = Path.Combine(root, "delta-only.manifest.json");
            var rejected = await continuedLoaded.Value.Machine.SaveCheckpointAsync(
                rejectedCheckpoint,
                rejectedManifest,
                deltaCorpus.ManifestPath.Replace('\\', '/'),
                ["phase05-delta-pair"]);

            Assert.False(rejected.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH", rejected.ErrorCode);
            Assert.False(File.Exists(rejectedCheckpoint));
            Assert.False(File.Exists(rejectedManifest));
            Assert.False(File.Exists(
                OrchestrationReasoningSourceAdmission.GetProofPath(rejectedCheckpoint)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Phase05DeltaAdmissionFailure_ExitsSevenBeforeCreatingOutputDirectory()
    {
        var root = NewRoot();
        try
        {
            var runner = Phase05RunnerAssembly();
            var missingBaseOutput = Path.Combine(root, "phase05-missing-base-output");
            var missingBase = await RunDotnetAsync(
                runner,
                "--output",
                missingBaseOutput);
            Assert.Equal(2, missingBase.ExitCode);
            Assert.False(Directory.Exists(missingBaseOutput));
            Assert.True(string.IsNullOrWhiteSpace(missingBase.StandardOutput));
            Assert.Contains(
                "source-admission evidence",
                missingBase.StandardError,
                StringComparison.OrdinalIgnoreCase);

            using var baseCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "base"));
            using var deltaCorpus =
                OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                    Path.Combine(root, "delta"));
            var output = Path.Combine(root, "phase05-output");
            var result = await RunDotnetAsync(
                runner,
                "--output",
                output,
                "--base-payload",
                baseCorpus.PayloadPath,
                "--base-manifest",
                baseCorpus.ManifestPath,
                "--base-source-admission-ledger",
                baseCorpus.LedgerPath,
                "--base-source-admission-evidence",
                baseCorpus.EvidencePath,
                "--delta-payload",
                deltaCorpus.PayloadPath,
                "--delta-manifest",
                deltaCorpus.ManifestPath);

            Assert.Equal(7, result.ExitCode);
            Assert.False(Directory.Exists(output));
            Assert.True(string.IsNullOrWhiteSpace(result.StandardOutput));
            Assert.Contains(
                "source-admission evidence",
                result.StandardError,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExplicitSaveWithoutAdmittedState_FailsClosedWithoutWrites()
    {
        var root = NewRoot();
        try
        {
            var checkpoint = Path.Combine(root, "empty", "base.json");
            var manifest = Path.Combine(root, "empty", "base.manifest.json");
            var proof = OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint);
            var sourceMaterials = Path.Combine(root, "empty", "source-materials");
            var machine = new OrchestrationReasoningMachine();

            var result = await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                "missing-runtime-admission-manifest",
                ["missing-runtime-admission-pair"]);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode,
                result.ErrorCode);
            Assert.Equal(
                "Checkpoint save requires verified runtime admission authority.",
                result.ErrorMessage);
            Assert.False(File.Exists(checkpoint));
            Assert.False(File.Exists(manifest));
            Assert.False(File.Exists(proof));
            Assert.False(Directory.Exists(sourceMaterials));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SchemaV2CheckpointManifestProofTriple_IsPortable()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("portable");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));
            var source = Path.Combine(root, "source");
            var checkpoint = Path.Combine(source, "base.json");
            var manifest = Path.Combine(source, "base.manifest.json");

            var saved = await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                admitted.State.ManifestId,
                admitted.Receipt.PairIds);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            var proof = Path.Combine(source, "base.source-admission-proof.json");
            Assert.True(File.Exists(proof));

            var moved = Path.Combine(root, "moved");
            CopyDirectory(source, moved);

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                Path.Combine(moved, "base.json"),
                Path.Combine(moved, "base.manifest.json"));

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.Equal(2, loaded.Value!.Checkpoint.SchemaVersion);
            Assert.Equal(
                loaded.Value.Checkpoint.LearnedState.ManifestId,
                loaded.Value.Checkpoint.SourceManifestRef);
            Assert.Contains("source-materials/01/manifest.json", loaded.Value.Checkpoint.SourceManifestRef, StringComparison.Ordinal);
            Assert.True(
                loaded.Value.Checkpoint.AcceptedRefIds
                    .ToHashSet(StringComparer.Ordinal)
                    .SetEquals(admitted.Receipt.PairIds));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CopyingCheckpointAndManifestWithoutProof_FailsNotFound()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("missing-proof");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));
            var checkpoint = Path.Combine(root, "source", "base.json");
            var manifest = Path.Combine(root, "source", "base.manifest.json");
            Assert.True((await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                admitted.State.ManifestId,
                admitted.Receipt.PairIds)).IsSuccess);

            var copy = Path.Combine(root, "copy");
            Directory.CreateDirectory(copy);
            File.Copy(checkpoint, Path.Combine(copy, "base.json"));
            File.Copy(manifest, Path.Combine(copy, "base.manifest.json"));

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                Path.Combine(copy, "base.json"),
                Path.Combine(copy, "base.manifest.json"));

            Assert.False(loaded.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_ADMISSION_PROOF_NOT_FOUND", loaded.ErrorCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TamperedProof_FailsHashMismatch()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("tamper");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));
            var checkpoint = Path.Combine(root, "base.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            Assert.True((await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                admitted.State.ManifestId,
                admitted.Receipt.PairIds)).IsSuccess);
            var proof = OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint);
            await File.AppendAllTextAsync(proof, " ");

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpoint, manifest);

            Assert.False(loaded.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_ADMISSION_PROOF_HASH_MISMATCH", loaded.ErrorCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ConflictingProofOverwrite_IsRejectedBeforeCheckpointOrManifestWrite()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("immutable");
            var checkpoint = Path.Combine(root, "base.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            var proof = OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint);
            await File.WriteAllTextAsync(checkpoint, "checkpoint-sentinel");
            await File.WriteAllTextAsync(manifest, "manifest-sentinel");
            await File.WriteAllTextAsync(proof, "conflicting-proof");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));

            var result = await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                admitted.State.ManifestId,
                admitted.Receipt.PairIds);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "ORCHESTRATION_REASONING_ADMISSION_PROOF_IMMUTABLE_CONFLICT",
                result.ErrorCode);
            Assert.Equal("checkpoint-sentinel", await File.ReadAllTextAsync(checkpoint));
            Assert.Equal("manifest-sentinel", await File.ReadAllTextAsync(manifest));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task StagingWriteFailure_RollsBackAllDestinationsAndCleansTemps(int failingOrdinal)
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted($"stage-failure-{failingOrdinal}");
            var machine = new OrchestrationReasoningMachine(
                new FaultingStagingWriter(failingOrdinal));
            Assert.True(machine.ImportAdmittedState(admitted).IsSuccess);
            var checkpoint = Path.Combine(root, "base.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            await File.WriteAllTextAsync(checkpoint, "checkpoint-before");
            await File.WriteAllTextAsync(manifest, "manifest-before");

            var result = await machine.SaveCheckpointAsync(checkpoint, manifest);

            Assert.False(result.IsSuccess);
            Assert.Equal("checkpoint-before", await File.ReadAllTextAsync(checkpoint));
            Assert.Equal("manifest-before", await File.ReadAllTextAsync(manifest));
            Assert.False(File.Exists(
                OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint)));
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp.*"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CheckpointTriplePathAliasesAndSeparateDirectories_FailBeforeWrites()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("path-preflight");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));
            var checkpoint = Path.Combine(root, "base.json");
            var proof = OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint);

            var same = await machine.SaveCheckpointAsync(checkpoint, checkpoint);
            var proofAlias = await machine.SaveCheckpointAsync(checkpoint, proof);
            var separate = await machine.SaveCheckpointAsync(
                checkpoint,
                Path.Combine(root, "other", "manifest.json"));

            Assert.Equal("ORCHESTRATION_REASONING_CHECKPOINT_TRIPLE_PATH_ALIAS", same.ErrorCode);
            Assert.Equal("ORCHESTRATION_REASONING_CHECKPOINT_TRIPLE_PATH_ALIAS", proofAlias.ErrorCode);
            Assert.Equal(
                "ORCHESTRATION_REASONING_CHECKPOINT_MANIFEST_DIRECTORY_MISMATCH",
                separate.ErrorCode);
            Assert.False(File.Exists(checkpoint));
            Assert.False(File.Exists(proof));
            Assert.False(Directory.Exists(Path.Combine(root, "other")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task FalseCallerProvenance_IsRejectedBeforeAnyWrite()
    {
        var root = NewRoot();
        try
        {
            var admitted = Admitted("provenance-mismatch");
            var machine = new OrchestrationReasoningMachine(new StaticLoader(admitted));
            var checkpoint = Path.Combine(root, "base.json");
            var manifest = Path.Combine(root, "base.manifest.json");

            var result = await machine.SaveCheckpointAsync(
                checkpoint,
                manifest,
                "caller://false-manifest",
                ["caller:false-ref"]);

            Assert.False(result.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH", result.ErrorCode);
            Assert.False(File.Exists(checkpoint));
            Assert.False(File.Exists(manifest));
            Assert.False(File.Exists(
                OrchestrationReasoningSourceAdmission.GetProofPath(checkpoint)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LoadReopensSourceMaterialsAndRejectsPostSaveTampering()
    {
        var root = NewRoot();
        try
        {
            using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();
            var materialized = await new OrchestrationReasoningTrainer().MaterializeAsync(
                corpus.PayloadPath,
                corpus.ManifestPath,
                corpus.LedgerPath,
                corpus.EvidencePath);
            Assert.True(materialized.IsSuccess, materialized.ErrorMessage);
            var machine = new OrchestrationReasoningMachine(
                new StaticLoader(materialized.Value!.AdmittedState));
            var checkpoint = Path.Combine(root, "base.json");
            var manifest = Path.Combine(root, "base.manifest.json");
            Assert.True((await machine.SaveCheckpointAsync(checkpoint, manifest)).IsSuccess);
            var bundledLedger = ResolveBundledSourceMaterialPath(
                root,
                checkpoint,
                "ledger_ref");
            await File.AppendAllTextAsync(bundledLedger, " ");

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                checkpoint,
                manifest);

            Assert.False(loaded.IsSuccess);
            Assert.Equal(
                OrchestrationReasoningSourceAdmission.AdmissionRejectedCode,
                loaded.ErrorCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CommonSchemaV1AtArbitraryPath_RequiresProvenanceRebuild()
    {
        var root = NewRoot();
        try
        {
            var checkpoint = Path.Combine(root, "anything.json");
            var manifest = Path.Combine(root, "anything.manifest.json");
            await File.WriteAllTextAsync(
                checkpoint,
                """{"schemaVersion":1,"learnedState":{},"acceptedRefIds":[],"sourceManifestRef":"legacy"}""");
            await File.WriteAllTextAsync(manifest, """{"schemaVersion":1}""");

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpoint, manifest);

            Assert.False(loaded.IsSuccess);
            Assert.Equal(
                "ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED",
                loaded.ErrorCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RuntimeAdmissionCapability_HasNoPublicConstructBindOrJsonPath()
    {
        var admitted = Admitted("mutation");
        var mutatedState = admitted.State with
        {
            LearnedPatterns =
            [
                admitted.State.LearnedPatterns![0] with
                {
                    ScenarioRef = "scenario:mutated-after-receipt"
                }
            ]
        };
        var checkpoint = OrchestrationReasoningSourceAdmissionTestFixture.Checkpoint(admitted) with
        {
            LearnedState = mutatedState
        };
        var machine = new OrchestrationReasoningMachine();

        var imported = machine.ImportLearnedState(checkpoint);

        Assert.False(imported.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_STATE_SOURCE_ADMISSION_INVALID", imported.ErrorCode);
        Assert.Empty(typeof(OrchestrationReasoningAdmittedLearnedState).GetConstructors());
        Assert.DoesNotContain(
            typeof(OrchestrationReasoningSourceAdmission).GetMethods(),
            method => method.Name == "Bind");
        Assert.ThrowsAny<NotSupportedException>(() =>
            JsonSerializer.Deserialize<OrchestrationReasoningAdmittedLearnedState>(
                JsonSerializer.Serialize(new
                {
                    state = mutatedState,
                    receipt = admitted.Receipt
                })));
        var handcraftedPackage =
            new OrchestrationReasoningAdmittedLearnedStatePersistencePackage(
                SchemaVersion: 1,
                State: mutatedState,
                Receipt: admitted.Receipt,
                SourceMaterials: []);
        var handcraftedRehydration =
            await OrchestrationReasoningSourceAdmission.RehydrateAsync(handcraftedPackage);
        Assert.False(handcraftedRehydration.IsSuccess);
        Assert.DoesNotContain(
            typeof(OrchestrationReasoningScenarioUnit).GetConstructors(),
            constructor => constructor.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(OrchestrationReasoningLearnedGuideStateDescriptor)
                || parameter.ParameterType == typeof(OrchestrationReasoningSourceAdmissionReceipt)));
    }

    private static OrchestrationReasoningAdmittedLearnedState Admitted(string seed)
        => OrchestrationReasoningSourceAdmissionTestFixture.Admit(
        [
            new OrchestrationReasoningLearnedPattern(
                $"pattern:{seed}",
                $"scenario:{seed}",
                ["universal_reasoning"],
                ["constraint"],
                ["requirement"],
                ["deliverable"],
                ["evidence"],
                ["phase"],
                ["gate"],
                ["asset"],
                ["host"],
                ["risk"],
                ["next"],
                0.8)
        ],
        $"state:{seed}",
        seed);

    private static OrchestrationReasoningLearnedPattern CliPattern(string seed)
        => new(
            PatternId: $"cli-{seed}-pattern",
            ScenarioRef: $"fixture://cli/{seed}/scenario",
            RequiredTaskTypes: [$"cli_{seed}_task"],
            RequiredConstraintSignals: [$"cli_{seed}_constraint"],
            RequiredRequirementSignals: [$"cli_{seed}_requirement"],
            RequiredDeliverableSignals: [$"cli_{seed}_deliverable"],
            RequiredEvidenceSignals: [$"cli_{seed}_evidence"],
            PhaseFamilies: ["cli-lifecycle"],
            GateFamilies: ["source-admission"],
            AssetFamilies: ["temp-corpus"],
            HostCheckSets: ["core-only"],
            RiskFlags: ["no-runtime-overclaim"],
            NextGranularizationActions: ["verify"],
            Confidence: 0.9);

    private static OrchestrationReasoningRequest Request()
        => new(
            "request",
            new StructuredTaskContract(
                "prompt",
                new string('a', 64),
                "en",
                "universal_reasoning",
                ["requirement"],
                ["constraint"],
                ["deliverable"],
                [],
                ["host"],
                ["evidence"],
                new Dictionary<string, string>(),
                "split"),
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-r17-boundary-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static string ResolveBundledSourceMaterialPath(
        string checkpointDirectory,
        string checkpointPath,
        string propertyName)
    {
        var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(checkpointPath);
        using var proofDocument = JsonDocument.Parse(File.ReadAllText(proofPath));
        var portableRef = proofDocument.RootElement
            .GetProperty("source_materials")[0]
            .GetProperty(propertyName)
            .GetString();
        Assert.False(string.IsNullOrWhiteSpace(portableRef));

        // The saved checkpoint is a self-contained bundle. The test corrupts
        // the bundle copy, not the temporary corpus that happened to train the
        // machine, so the assertion proves fresh proof re-reading after export.
        return Path.Combine(
            Path.GetDirectoryName(checkpointPath)!,
            portableRef!.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (Directory.Exists(destinationDir))
            Directory.Delete(destinationDir, recursive: true);
        Directory.CreateDirectory(destinationDir);
        foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, directory)));
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, file)), overwrite: true);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private static string TestBuildConfiguration
    {
        get
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }

    private static string TranslationToolAssembly()
        => Path.Combine(
            RepoRoot(),
            "27.TranslationDomain.Tools",
            "bin",
            TestBuildConfiguration,
            "net9.0",
            "27.TranslationDomain.Tools.dll");

    private static string Phase05RunnerAssembly()
        => Path.Combine(
            RepoRoot(),
            "tools",
            "OrchestrationReasoningPhase05Runner",
            "bin",
            TestBuildConfiguration,
            "net9.0",
            "OrchestrationReasoningPhase05Runner.dll");

    private static async Task<(string PayloadPath, string ManifestPath)> WritePhase05HeldoutFixtureAsync(
        string root)
    {
        Directory.CreateDirectory(root);
        var payloadPath = Path.Combine(root, "heldout.jsonl");
        var manifestPath = Path.Combine(root, "heldout-manifest.json");
        var rows = new[]
        {
            Phase05HeldoutRow("phase05_base", "base"),
            Phase05HeldoutRow("phase05_delta", "delta")
        };
        await File.WriteAllTextAsync(
            payloadPath,
            string.Join('\n', rows.Select(row => JsonSerializer.Serialize(row))) + "\n");
        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(new
            {
                row_count = rows.Length,
                payloads_sha256 = OrchestrationReasoningSourceAdmissionTestFixture.HashFile(payloadPath),
                source_pairs_sha256 =
                    OrchestrationReasoningSourceAdmissionTestFixture.Hash("phase05-heldout-pairs"),
                source_disjoint_from_accepted_refs = true,
                raw_private_text_copied = false,
                technical_execution_leakage = false,
                external_ai_used = false,
                training_started = false
            }));
        return (payloadPath, manifestPath);
    }

    private static Dictionary<string, object?> Phase05HeldoutRow(string patternSeed, string rowSeed)
        => new()
        {
            ["row_id"] = $"phase05-heldout-{rowSeed}",
            ["pair_id"] = $"phase05-heldout-pair-{rowSeed}",
            ["source_row_id"] = $"phase05-heldout-source-row-{rowSeed}",
            ["source_ref"] = $"fixture://phase05/heldout/{rowSeed}",
            ["source_sha256"] =
                OrchestrationReasoningSourceAdmissionTestFixture.Hash($"phase05-heldout-source:{rowSeed}"),
            ["raw_prompt_sha256"] =
                OrchestrationReasoningSourceAdmissionTestFixture.Hash($"phase05-heldout-prompt:{rowSeed}"),
            ["task_type"] = $"cli_{patternSeed}_task",
            ["requirement_signals"] = new[] { $"cli_{patternSeed}_requirement" },
            ["constraint_signals"] = new[] { $"cli_{patternSeed}_constraint" },
            ["deliverable_signals"] = new[] { $"cli_{patternSeed}_deliverable" },
            ["host_signals"] = new[] { "core-only" },
            ["evidence_signals"] = new[] { $"cli_{patternSeed}_evidence" },
            ["expected_pattern_id"] = $"cli-{patternSeed}-pattern",
            ["expected_scenario_id"] = $"fixture://cli/{patternSeed}/scenario",
            ["row_novelty_class"] = "source-disjoint-signal-preserving",
            ["raw_private_text_copied"] = false,
            ["technical_execution_leakage"] = false,
            ["external_ai_used"] = false
        };

    private static Task<CommandResult> RunMaterializeCliAsync(
        string toolAssembly,
        OrchestrationReasoningSourceAdmissionTestFixture.SyntheticCorpus corpus,
        string outputPath)
        => RunDotnetAsync(
            toolAssembly,
            "materialize-orchestration-reasoning-state",
            "--payloads",
            corpus.PayloadPath,
            "--manifest",
            corpus.ManifestPath,
            "--source-admission-ledger",
            corpus.LedgerPath,
            "--source-admission-evidence",
            corpus.EvidencePath,
            "--output",
            outputPath);

    private static async Task<string> WriteTamperedPackageAsync(string source, string destination)
    {
        var root = JsonNode.Parse(await File.ReadAllTextAsync(source))!.AsObject();
        root["admitted_state_package"]!["receipt"]!["receipt_sha256"] =
            new string('0', 64);
        await File.WriteAllTextAsync(
            destination,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return destination;
    }

    private static void AssertCommandPassed(CommandResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"Command failed with {result.ExitCode}.{Environment.NewLine}stdout: {result.StandardOutput}{Environment.NewLine}stderr: {result.StandardError}");
    }

    private static void AssertAdmissionFailure(CommandResult result)
    {
        Assert.Equal(3, result.ExitCode);
        Assert.True(string.IsNullOrWhiteSpace(result.StandardOutput));
        using var error = JsonDocument.Parse(result.StandardError);
        var errorCode = error.RootElement.GetProperty("error_code").GetString();
        Assert.True(
            errorCode?.Contains("SOURCE_ADMISSION", StringComparison.Ordinal) == true
            || errorCode?.Contains("ADMISSION_PROOF", StringComparison.Ordinal) == true,
            $"Unexpected admission error: {errorCode}");
    }

    private static async Task<CommandResult> RunDotnetAsync(
        string assemblyPath,
        params string[] arguments)
    {
        Assert.True(File.Exists(assemblyPath), $"Expected built executable at {assemblyPath}.");
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (string.IsNullOrWhiteSpace(dotnet))
        {
            var userDotnet = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".dotnet",
                OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
            dotnet = File.Exists(userDotnet) ? userDotnet : "dotnet";
        }

        var start = new ProcessStartInfo
        {
            FileName = dotnet,
            WorkingDirectory = RepoRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        start.ArgumentList.Add(assemblyPath);
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start);
        Assert.NotNull(process);
        var standardOutput = process!.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new CommandResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private sealed record CommandResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);

    private sealed class StaticLoader(OrchestrationReasoningAdmittedLearnedState admitted)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningAdmittedLearnedState> Load()
            => OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(admitted);
    }

    private sealed class FaultingStagingWriter(int failingOrdinal)
        : IOrchestrationReasoningCheckpointStagingWriter
    {
        public Task WriteAsync(
            int ordinal,
            string stagingPath,
            byte[] bytes,
            CancellationToken ct)
            => ordinal == failingOrdinal
                ? throw new IOException($"forced-stage-write-{ordinal}")
                : File.WriteAllBytesAsync(stagingPath, bytes, ct);
    }
}
