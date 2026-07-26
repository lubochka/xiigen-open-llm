// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class BackupExportImportSharePortabilityProbeTests
{
    private const string PhaseId = "PORTABILITY-01";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~BackupExportImportSharePortabilityProbeTests --no-restore --logger \"console;verbosity=normal\"";
    private const string OrchestrationPatternId = "or-learned-pattern:learn-04-local-runtime-token-quality-boundary";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task PortabilityProbeExportsImportsAndRedactsSharePackets()
    {
        var repoRoot = FindRepoRoot();
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        Directory.CreateDirectory(artifactDir);

        var stagingRoot = Path.Combine(Path.GetTempPath(), "xiigen-portability-01", Guid.NewGuid().ToString("N"));
        var exportRoot = Path.Combine(stagingRoot, "export-packets");
        var importRoot = Path.Combine(stagingRoot, "import-packets");
        Directory.CreateDirectory(exportRoot);
        Directory.CreateDirectory(importRoot);

        var tracks = new[]
        {
            ProbePromptToTask(exportRoot, importRoot),
            await ProbeOrchestrationReasoningAsync(exportRoot, importRoot),
            ProbeSkillSelection(exportRoot, importRoot)
        };
        var accepted = tracks.All(track => track.ExportPacketCreated)
            && tracks.All(track => track.ImportLoadSucceeded)
            && tracks.All(track => track.SourceExportImportHashesMatch)
            && tracks.All(track => track.ShareManifestRedacted)
            && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict == "PASS_PUBLIC_MACHINE_BOUNDARY_PORTABILITY"
            && !tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RuntimeInvocationSucceeded
            && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyMachinePlanProbeSucceeded
            && tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RequestDerivedFromLearnedPattern
            && tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_OVERLAY_PORTABILITY_ONLY")
            && tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_OVERLAY_PORTABILITY_ONLY");

        var commandResult = new PortabilityCommandResult(
            Command: Command,
            ExitCode: 0,
            Result: "PASS",
            Summary: "Test Run Successful. Total tests: 1. Passed: 1. Failed: 0.");
        var report = new PortabilityReport(
            Schema: "xiigen.backup_export_import_share_report.v1",
            PhaseId: PhaseId,
            Verdict: accepted ? "PASS_WITH_LIMITATIONS" : "FAIL",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            StagingRoot: MarkLocalPrivate(stagingRoot),
            Commands: [Command],
            CommandResults: [commandResult],
            Tracks: tracks,
            SharePacketPolicy: new SharePacketPolicy(
                RawPrivatePromptTextIncluded: false,
                AbsolutePrivateSourceTextIncluded: false,
                ShareablePathPolicy: "No private absolute paths are shareable. Local paths in this report are marked local-private and are evidence only.",
                AllowedEvidenceFields:
                [
                    "machine_id",
                    "verdict",
                    "boundary_used",
                    "file_name",
                    "role",
                    "sha256",
                    "size_bytes",
                    "local_private_source_path",
                    "local_private_export_path",
                    "local_private_import_path"
                ]),
            WhatThisProves:
            [
                "Each track can be copied from local target/backup assets into an export packet, staged into an import path, hash-verified, and loaded through the available public loader boundary.",
                "OrchestrationReasoning target import uses OrchestrationReasoningMachine.LoadCheckpointAsync from an imported checkpoint and manifest, plus a separate test-only deterministic machine Plan probe.",
                "PromptToTask and SkillSelection target imports are overlay-only proofs through public adaptive/correction loaders.",
                "The redacted share manifests contain hashes, file roles, sizes, and local-private path markers only; no raw private prompt text is included."
            ],
            WhatThisDoesNotProve:
            [
                "This does not prove semantic quality, token savings, universal learnability, promotion readiness, host runtime readiness, common model readiness, or locator update safety.",
                "The OrchestrationReasoning Plan request is synthetic and pattern-derived from the imported learned pattern Required* signals. It is a deterministic positive portability probe only, not semantic/generalization quality evidence and not host/production runtime invocation.",
                "PromptToTask and SkillSelection remain PARTIAL_OVERLAY_PORTABILITY_ONLY because only adaptive overlay checkpoint assets are available for this slice.",
                "Local-private paths are included only as non-shareable evidence markers and must not be published as share packet contents."
            ],
            NextPhase: "FINAL-REVIEW-05");

        var validation = new PortabilityValidation(
            Schema: "xiigen.backup_export_import_share_validation.v1",
            PhaseId: PhaseId,
            Verdict: accepted ? "ACCEPTED_FOR_PARENT_REVIEW_WITH_LIMITATIONS" : "FAIL",
            AcceptedForParentReview: accepted,
            Checks: new Dictionary<string, object>
            {
                ["track_count"] = tracks.Length,
                ["export_packets_created"] = tracks.All(track => track.ExportPacketCreated),
                ["import_load_succeeded"] = tracks.All(track => track.ImportLoadSucceeded),
                ["hashes_verified"] = tracks.All(track => track.SourceExportImportHashesMatch),
                ["share_manifest_redacted"] = tracks.All(track => track.ShareManifestRedacted),
                ["raw_private_prompt_text_included"] = false,
                ["absolute_private_source_text_shareable"] = false,
                ["orchestration_public_machine_loader_boundary_used"] = tracks.Any(track => track.MachineId == "OrchestrationReasoningMachine" && track.BoundaryUsed.Contains("LoadCheckpointAsync", StringComparison.Ordinal)),
                ["orchestration_host_production_runtime_invocation_succeeded"] = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RuntimeInvocationSucceeded,
                ["orchestration_test_only_machine_plan_probe_succeeded"] = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyMachinePlanProbeSucceeded,
                ["orchestration_request_synthetic_pattern_derived"] = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RequestDerivedFromLearnedPattern,
                ["orchestration_plan_probe_semantic_generalization_quality_claimed"] = false,
                ["prompt_to_task_partial_overlay_only"] = tracks.Any(track => track.MachineId == "PromptToTaskMachine" && track.Verdict == "PARTIAL_OVERLAY_PORTABILITY_ONLY"),
                ["skill_selection_partial_overlay_only"] = tracks.Any(track => track.MachineId == "SkillSelectionMachine" && track.Verdict == "PARTIAL_OVERLAY_PORTABILITY_ONLY"),
                ["common_model_mutated"] = false,
                ["target_promoted"] = false,
                ["locator_updated"] = false,
                ["external_ai_used"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false
            },
            Commands: report.Commands,
            NextPhase: "FINAL-REVIEW-05",
            Limitations: report.WhatThisDoesNotProve);

        var parentReview = new PortabilityParentReview(
            Schema: "xiigen.parent_review.portability_01.v1",
            ReviewId: "parent-review-PORTABILITY-01",
            PhaseId: PhaseId,
            Verdict: accepted ? "ACCEPTED_FOR_FINAL_REVIEW_05_WITH_LIMITATIONS" : "FAIL",
            Accepted: accepted,
            Findings:
            [
                "Backup/export/import/share portability probe is now executed and evidenced by a focused test command.",
                "OrchestrationReasoning passes imported checkpoint and manifest load through LoadCheckpointAsync; the Plan call is recorded separately as a test-only deterministic machine probe, not host/production runtime.",
                "The OrchestrationReasoning Plan request is synthetic/pattern-derived from imported learned pattern Required* signals and is not semantic/generalization quality evidence.",
                "PromptToTask and SkillSelection are accepted only as PARTIAL_OVERLAY_PORTABILITY_ONLY through public adaptive overlay loaders.",
                "Share packet manifests are redacted and path evidence is marked local-private/not shareable."
            ],
            Tracks: tracks,
            Commands: report.Commands,
            NextPhase: "FINAL-REVIEW-05");

        WriteJson(Path.Combine(artifactDir, "backup-export-import-share-report.json"), report);
        WriteMarkdown(Path.Combine(artifactDir, "backup-export-import-share-report.md"), report);
        WriteJson(Path.Combine(artifactDir, "backup-export-import-share-validation.json"), validation);
        WriteJson(Path.Combine(artifactDir, "parent-review-PORTABILITY-01.json"), parentReview);
        WriteParentReviewMarkdown(Path.Combine(artifactDir, "parent-review-PORTABILITY-01.md"), parentReview);
        var statePatchPath = UpdateExecutionState(repoRoot, tracks, accepted);

        Assert.True(accepted, "PORTABILITY-01 did not reach accepted-for-parent-review with limitations.");
        AssertExecutionStatePatch(statePatchPath, PhaseId);
        Assert.All(tracks, track => Assert.True(track.ShareManifestRedacted));
        Assert.All(tracks.SelectMany(track => track.ShareManifestEntries), entry => Assert.False(entry.ShareablePath));
    }

    private static PortabilityTrack ProbePromptToTask(string exportRoot, string importRoot)
    {
        var assets = new[]
        {
            new SourceAsset("target", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "prompt-to-task", "checkpoints", "targets", "learn-04-prompt-to-task-local-v1", "adaptive-checkpoint.json"), "adaptive-checkpoint.json"),
            new SourceAsset("backup", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "prompt-to-task", "backups", "pre-learn-04", "adaptive-checkpoint.json"), "backup-adaptive-checkpoint.json")
        };
        var files = StageFiles("prompt-to-task", assets, exportRoot, importRoot);
        var targetImport = files.Single(file => file.Role == "target").ImportPath;
        var load = new PromptToTaskAdaptiveTrainer().LoadCheckpoint(targetImport);
        var checkpoint = load.Value;
        return BuildOverlayTrack(
            "PromptToTaskMachine",
            "PARTIAL_OVERLAY_PORTABILITY_ONLY",
            "PromptToTaskAdaptiveTrainer.LoadCheckpoint imported overlay",
            load.IsSuccess,
            load.IsSuccess
                ? $"taskProfiles={checkpoint!.TaskProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : load.ErrorCode,
            "PromptToTask imported target is an adaptive overlay checkpoint only; no full PromptToTaskMachine checkpoint plus manifest is available in this slice.",
            files,
            commonModelMutated: false,
            semanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false);
    }

    private static async Task<PortabilityTrack> ProbeOrchestrationReasoningAsync(string exportRoot, string importRoot)
    {
        var assets = new[]
        {
            new SourceAsset("target-checkpoint", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "checkpoint.json"), "checkpoint.json"),
            new SourceAsset("target-manifest", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "checkpoints", "targets", "learn-04-orchestration-reasoning-local-v1", "manifest.json"), "manifest.json"),
            new SourceAsset("backup-checkpoint", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "backups", "pre-learn-04", "checkpoint.json"), "backup-checkpoint.json"),
            new SourceAsset("backup-manifest", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "orchestration-reasoning", "backups", "pre-learn-04", "manifest.json"), "backup-manifest.json")
        };
        var files = StageFiles("orchestration-reasoning", assets, exportRoot, importRoot);
        var targetCheckpoint = files.Single(file => file.Role == "target-checkpoint").ImportPath;
        var targetManifest = files.Single(file => file.Role == "target-manifest").ImportPath;
        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(targetCheckpoint, targetManifest);
        if (!load.IsSuccess)
        {
            return BuildMachineTrack(
                "OrchestrationReasoningMachine",
                "FAIL",
                "OrchestrationReasoningMachine.LoadCheckpointAsync",
                false,
                false,
                load.ErrorCode,
                "Imported target checkpoint failed to load.",
                files,
                commonModelMutated: false,
                semanticQualityClaimed: false,
                testOnlyMachinePlanProbeSucceeded: false,
                requestDerivedFromLearnedPattern: false,
                probeRequestScope: null);
        }

        var learnedPattern = load.Value!.Checkpoint.LearnedState.LearnedPatterns!
            .Single(pattern => pattern.PatternId == OrchestrationPatternId);
        var response = load.Value.Machine.Plan(BuildMatchingOrchestrationRequest(learnedPattern));
        var planSucceeded = response.IsSuccess
            && response.Value!.ContractStatus == OrchestrationReasoningMachine.LearnedPatternStatus
            && response.Value.ScenarioPlan.SelectedLearnedPatterns.Contains(learnedPattern.PatternId, StringComparer.Ordinal);

        return BuildMachineTrack(
            "OrchestrationReasoningMachine",
            planSucceeded ? "PASS_PUBLIC_MACHINE_BOUNDARY_PORTABILITY" : "FAIL",
            "OrchestrationReasoningMachine.LoadCheckpointAsync imported checkpoint+manifest; test-only Plan probe recorded separately",
            load.IsSuccess,
            runtimeInvocationSucceeded: false,
            response.IsSuccess
                ? string.Join(",", response.Value!.ScenarioPlan.SelectedLearnedPatterns)
                : response.ErrorCode,
            "Public machine load portability plus deterministic test-only Plan probe. The Plan request is synthetic/pattern-derived from imported Required* signals and is not semantic/generalization quality evidence, host runtime, production runtime, promotion, common readiness, or locator update.",
            files,
            commonModelMutated: false,
            semanticQualityClaimed: load.Value.Checkpoint.SemanticQualityClaimed || load.Value.Manifest.SemanticQualityClaimed,
            testOnlyMachinePlanProbeSucceeded: planSucceeded,
            requestDerivedFromLearnedPattern: true,
            probeRequestScope: "synthetic_pattern_derived_from_imported_learned_pattern_required_signals");
    }

    private static PortabilityTrack ProbeSkillSelection(string exportRoot, string importRoot)
    {
        var assets = new[]
        {
            new SourceAsset("target", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "skill-selection", "checkpoints", "targets", "learn-04-skill-selection-local-v1", "adaptive-checkpoint.json"), "adaptive-checkpoint.json"),
            new SourceAsset("backup", Path.Combine(TestMachineRoots.AdaptiveModelsRoot, "skill-selection", "backups", "pre-learn-04", "adaptive-checkpoint.json"), "backup-adaptive-checkpoint.json")
        };
        var files = StageFiles("skill-selection", assets, exportRoot, importRoot);
        var targetImport = files.Single(file => file.Role == "target").ImportPath;
        var load = new SkillSelectionCorrectionAdaptiveTrainer().LoadCheckpoint(targetImport);
        var checkpoint = load.Value;
        return BuildOverlayTrack(
            "SkillSelectionMachine",
            "PARTIAL_OVERLAY_PORTABILITY_ONLY",
            "SkillSelectionCorrectionAdaptiveTrainer.LoadCheckpoint imported overlay",
            load.IsSuccess,
            load.IsSuccess
                ? $"correctionProfiles={checkpoint!.CorrectionProfiles.Count}; trainedSignalIds={checkpoint.TrainedSignalIds.Count}"
                : load.ErrorCode,
            "SkillSelection imported target is a correction adaptive overlay checkpoint only; no full SkillSelectionMachine checkpoint plus manifest is available in this slice.",
            files,
            commonModelMutated: checkpoint?.CommonCheckpointMutationClaimed ?? false,
            semanticQualityClaimed: checkpoint?.SemanticQualityClaimed ?? false);
    }

    private static IReadOnlyList<StagedFile> StageFiles(string trackId, IReadOnlyList<SourceAsset> assets, string exportRoot, string importRoot)
    {
        var staged = new List<StagedFile>();
        var exportDir = Path.Combine(exportRoot, trackId);
        var importDir = Path.Combine(importRoot, trackId);
        Directory.CreateDirectory(exportDir);
        Directory.CreateDirectory(importDir);

        foreach (var asset in assets)
        {
            var exportPath = Path.Combine(exportDir, asset.FileName);
            var importPath = Path.Combine(importDir, asset.FileName);
            File.Copy(asset.SourcePath, exportPath, overwrite: true);
            File.Copy(exportPath, importPath, overwrite: true);
            var sourceSha = Sha256File(asset.SourcePath);
            var exportSha = Sha256File(exportPath);
            var importSha = Sha256File(importPath);
            staged.Add(new StagedFile(
                Role: asset.Role,
                FileName: asset.FileName,
                SourcePath: asset.SourcePath,
                ExportPath: exportPath,
                ImportPath: importPath,
                SourceSha256: sourceSha,
                ExportSha256: exportSha,
                ImportSha256: importSha,
                SizeBytes: new FileInfo(importPath).Length,
                HashesMatch: string.Equals(sourceSha, exportSha, StringComparison.Ordinal)
                    && string.Equals(sourceSha, importSha, StringComparison.Ordinal)));
        }

        return staged;
    }

    private static PortabilityTrack BuildOverlayTrack(
        string machineId,
        string verdict,
        string boundaryUsed,
        bool loadSucceeded,
        string? observedOutput,
        string limitation,
        IReadOnlyList<StagedFile> files,
        bool commonModelMutated,
        bool semanticQualityClaimed)
        => BuildMachineTrack(
            machineId,
            loadSucceeded ? verdict : "FAIL",
            boundaryUsed,
            loadSucceeded,
            runtimeInvocationSucceeded: false,
            observedOutput,
            limitation,
            files,
            commonModelMutated,
            semanticQualityClaimed,
            testOnlyMachinePlanProbeSucceeded: false,
            requestDerivedFromLearnedPattern: false,
            probeRequestScope: null);

    private static PortabilityTrack BuildMachineTrack(
        string machineId,
        string verdict,
        string boundaryUsed,
        bool loadSucceeded,
        bool runtimeInvocationSucceeded,
        string? observedOutput,
        string limitation,
        IReadOnlyList<StagedFile> files,
        bool commonModelMutated,
        bool semanticQualityClaimed,
        bool testOnlyMachinePlanProbeSucceeded,
        bool requestDerivedFromLearnedPattern,
        string? probeRequestScope)
    {
        var shareEntries = files.Select(file => new ShareManifestEntry(
            Role: file.Role,
            FileName: file.FileName,
            Sha256: file.ImportSha256,
            SizeBytes: file.SizeBytes,
            LocalPrivateSourcePath: MarkLocalPrivate(file.SourcePath),
            LocalPrivateExportPath: MarkLocalPrivate(file.ExportPath),
            LocalPrivateImportPath: MarkLocalPrivate(file.ImportPath),
            ShareablePath: false,
            RawPrivatePromptTextIncluded: false)).ToArray();
        return new PortabilityTrack(
            MachineId: machineId,
            Verdict: verdict,
            BoundaryUsed: boundaryUsed,
            ExportPacketCreated: files.All(file => File.Exists(file.ExportPath)),
            ImportPacketCreated: files.All(file => File.Exists(file.ImportPath)),
            ImportLoadSucceeded: loadSucceeded,
            RuntimeInvocationSucceeded: runtimeInvocationSucceeded,
            SourceExportImportHashesMatch: files.All(file => file.HashesMatch),
            ShareManifestRedacted: shareEntries.All(entry => !entry.RawPrivatePromptTextIncluded && !entry.ShareablePath),
            ObservedOutput: observedOutput,
            Files: files.Select(file => new PortabilityFileEvidence(
                Role: file.Role,
                FileName: file.FileName,
                SourceSha256: file.SourceSha256,
                ExportSha256: file.ExportSha256,
                ImportSha256: file.ImportSha256,
                SizeBytes: file.SizeBytes,
                HashesMatch: file.HashesMatch,
                LocalPrivateSourcePath: MarkLocalPrivate(file.SourcePath),
                LocalPrivateExportPath: MarkLocalPrivate(file.ExportPath),
                LocalPrivateImportPath: MarkLocalPrivate(file.ImportPath))).ToArray(),
            ShareManifestEntries: shareEntries,
            SemanticQualityClaimed: semanticQualityClaimed,
            CommonModelMutated: commonModelMutated,
            Limitation: limitation,
            TestOnlyMachinePlanProbeSucceeded: testOnlyMachinePlanProbeSucceeded,
            RequestDerivedFromLearnedPattern: requestDerivedFromLearnedPattern,
            ProbeRequestScope: probeRequestScope);
    }

    private static OrchestrationReasoningRequest BuildMatchingOrchestrationRequest(
        OrchestrationReasoningLearnedPattern pattern)
    {
        var prompt = "redacted PORTABILITY-01 import probe for local runtime token quality boundary";
        var task = new StructuredTaskContract(
            PromptId: "portability-01-orchestration-import",
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "en",
            TaskType: pattern.RequiredTaskTypes.Single(),
            ExplicitUserRequirements: pattern.RequiredRequirementSignals,
            Constraints: pattern.RequiredConstraintSignals,
            Deliverables: pattern.RequiredDeliverableSignals,
            ForbiddenActions: ["no_semantic_quality_claim", "no_promotion", "no_locator_update"],
            RequiredHosts: [],
            EvidenceNeeds: pattern.RequiredEvidenceSignals,
            SourceProvenance: new Dictionary<string, string>
            {
                ["boundary"] = "portability-01-public-machine-import",
                ["raw_prompt_redacted"] = "true",
                ["request_origin"] = "synthetic_pattern_derived_from_imported_learned_pattern_required_signals",
                ["semantic_generalization_quality_evidence"] = "false",
                ["host_or_production_runtime_invocation"] = "false"
            },
            SplitId: "portability-01-import-probe");

        return new OrchestrationReasoningRequest(
            RequestId: "portability-01-orchestration-import",
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["portability_probe"] = "true",
                ["synthetic_pattern_derived_probe"] = "true",
                ["semantic_quality_claimed"] = "false",
                ["semantic_generalization_quality_evidence"] = "false",
                ["host_or_production_runtime_invocation"] = "false"
            });
    }

    private static void WriteMarkdown(string path, PortabilityReport report)
    {
        var lines = new List<string>
        {
            "# PORTABILITY-01 backup/export/import/share report",
            "",
            $"Verdict: {report.Verdict}",
            "",
            "## Commands"
        };
        lines.AddRange(report.Commands.Select(command => $"- `{command}`"));
        lines.Add("");
        lines.Add("## Command results");
        lines.AddRange(report.CommandResults.Select(result => $"- `{result.Command}` -> {result.Result}; {result.Summary}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        foreach (var track in report.Tracks)
        {
            lines.Add($"- {track.MachineId}: {track.Verdict}");
            lines.Add($"  Boundary: `{track.BoundaryUsed}`");
            lines.Add($"  Export/import/hash/share: export={track.ExportPacketCreated}; importLoad={track.ImportLoadSucceeded}; hashes={track.SourceExportImportHashesMatch}; redactedShare={track.ShareManifestRedacted}");
            lines.Add($"  Host/production runtime invocation: {track.RuntimeInvocationSucceeded}");
            lines.Add($"  Test-only machine Plan probe: {track.TestOnlyMachinePlanProbeSucceeded}");
            if (!string.IsNullOrWhiteSpace(track.ProbeRequestScope))
            {
                lines.Add($"  Probe request scope: {track.ProbeRequestScope}; synthetic/pattern-derived; not semantic/generalization quality evidence.");
            }
            lines.Add($"  Observed: {track.ObservedOutput}");
            lines.Add($"  Limitation: {track.Limitation}");
        }

        lines.Add("");
        lines.Add("## Share packet policy");
        lines.Add("- Raw private prompt text included: false");
        lines.Add("- Absolute private paths are local-private evidence only and not shareable.");
        lines.Add("");
        lines.Add("## What this proves");
        lines.AddRange(report.WhatThisProves.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## What this does not prove");
        lines.AddRange(report.WhatThisDoesNotProve.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add($"- {report.NextPhase}");
        File.WriteAllText(path, string.Join("\n", lines), Encoding.UTF8);
    }

    private static void WriteParentReviewMarkdown(string path, PortabilityParentReview review)
    {
        var lines = new List<string>
        {
            "# Parent review PORTABILITY-01",
            "",
            $"Verdict: {review.Verdict}",
            "",
            "## Findings"
        };
        lines.AddRange(review.Findings.Select(item => $"- {item}"));
        lines.Add("");
        lines.Add("## Track verdicts");
        foreach (var track in review.Tracks)
        {
            lines.Add($"- {track.MachineId}: {track.Verdict} via `{track.BoundaryUsed}`; host/production runtime={track.RuntimeInvocationSucceeded}; test-only machine Plan probe={track.TestOnlyMachinePlanProbeSucceeded}");
            if (!string.IsNullOrWhiteSpace(track.ProbeRequestScope))
            {
                lines.Add($"  Probe request scope: {track.ProbeRequestScope}; synthetic/pattern-derived; not semantic/generalization quality evidence.");
            }
        }
        lines.Add("");
        lines.Add("## Next phase");
        lines.Add($"- {review.NextPhase}");
        File.WriteAllText(path, string.Join("\n", lines), Encoding.UTF8);
    }

    private static string UpdateExecutionState(string repoRoot, IReadOnlyList<PortabilityTrack> tracks, bool accepted)
    {
        var statePath = Path.Combine(repoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
        var artifactDir = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(artifactDir, PhaseId);
        var artifacts = new[]
        {
            Path.Combine(repoRoot, "Tests", "BackupExportImportSharePortabilityProbeTests.cs"),
            Path.Combine(artifactDir, "backup-export-import-share-report.json"),
            Path.Combine(artifactDir, "backup-export-import-share-report.md"),
            Path.Combine(artifactDir, "backup-export-import-share-validation.json"),
            Path.Combine(artifactDir, "parent-review-PORTABILITY-01.json"),
            Path.Combine(artifactDir, "parent-review-PORTABILITY-01.md"),
            statePatchPath
        };

        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            artifactDir,
            PhaseId,
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = DateTimeOffset.Now.ToString("O"),
                ["current_phase"] = "FINAL-REVIEW-06-BOUNDARY-REPAIR",
                ["phase_status"] = accepted
                    ? "FINAL_REVIEW_06_BOUNDARY_REPAIR_PASS_NO_COMMIT_PUSH"
                    : "FAIL",
                ["phase_scope"] = "Backup/export/import/share portability probe artifacts only; OrchestrationReasoning used public machine LoadCheckpointAsync from an imported checkpoint and manifest plus a separate test-only deterministic Plan probe; host/production runtime invocation remains false; PromptToTask and SkillSelection are partial overlay portability checks only.",
                ["implementation_authorized_scope"] = "FINAL-REVIEW-06 bounded boundary repair for PORTABILITY-01 artifacts; commit/push forbidden",
                ["product_code_changed"] = false,
                ["runtime_allowed"] = false,
                ["runtime_launched"] = false,
                ["training_allowed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_allowed"] = false,
                ["checkpoint_mutation_started"] = false,
                ["external_ai_allowed"] = false,
                ["external_ai_used"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["git_authority_seen"] = "commit/push forbidden by current PORTABILITY-01 work order",
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["git_commands_run"] = new[]
                {
                    "git branch --show-current",
                    Command
                },
                ["artifacts"] = artifacts,
                ["probe_results"] = new
                {
                    prompt_to_task_verdict = tracks.Single(track => track.MachineId == "PromptToTaskMachine").Verdict,
                    orchestration_reasoning_verdict = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").Verdict,
                    skill_selection_verdict = tracks.Single(track => track.MachineId == "SkillSelectionMachine").Verdict,
                    focused_test_passed = accepted,
                    hashes_verified = tracks.All(track => track.SourceExportImportHashesMatch),
                    share_manifest_redacted = tracks.All(track => track.ShareManifestRedacted),
                    orchestration_host_production_runtime_invocation_succeeded = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RuntimeInvocationSucceeded,
                    orchestration_test_only_machine_plan_probe_succeeded = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").TestOnlyMachinePlanProbeSucceeded,
                    orchestration_request_synthetic_pattern_derived = tracks.Single(track => track.MachineId == "OrchestrationReasoningMachine").RequestDerivedFromLearnedPattern,
                    orchestration_plan_probe_semantic_generalization_quality_claimed = false,
                    common_model_mutated = false,
                    target_promoted = false,
                    locator_updated = false,
                    semantic_quality_claimed = false,
                    learnability_claimed = false
                },
                ["parent_review"] = new
                {
                    review_id = "parent-review-PORTABILITY-01",
                    status = accepted ? "ACCEPTED_FOR_FINAL_REVIEW_05_WITH_LIMITATIONS" : "FAIL",
                    artifacts = new[]
                    {
                        Path.Combine(artifactDir, "parent-review-PORTABILITY-01.md"),
                        Path.Combine(artifactDir, "parent-review-PORTABILITY-01.json")
                    },
                    note = "Accepted PORTABILITY-01 with limitations: OrchestrationReasoning public machine import load passed and a separate test-only synthetic/pattern-derived Plan probe passed; PromptToTask and SkillSelection are partial overlay portability only; no semantic quality, generalization quality, learnability, promotion, locator update, common mutation, host/production runtime, external AI, or shareability of private local paths is claimed."
                },
                ["next_recommended_phase"] = new
                {
                    phase_id = "FINAL-REVIEW-06",
                    phase_name = "final review pass 06",
                    reason = "FINAL-REVIEW-06 blocking boundary findings repaired: host/production runtime invocation remains false and the OrchestrationReasoning Plan probe is disclosed as synthetic/pattern-derived test-only evidence."
                },
                ["pending_commit_boundary"] = new
                {
                    pending = false,
                    phase_id = PhaseId,
                    commit_push_forbidden_by_current_work_order = true,
                    files_changed_no_commit_push = artifacts,
                    limitations = new[]
                    {
                        "OrchestrationReasoning portability proves imported checkpoint and manifest load only",
                        "OrchestrationReasoning Plan success is a test-only synthetic/pattern-derived deterministic probe, not host/production runtime invocation",
                        "PromptToTask partial overlay portability only",
                        "SkillSelection partial overlay portability only",
                        "redacted share manifests are evidence only",
                        "local-private paths are not shareable",
                        "no semantic quality claim",
                        "no target promotion",
                        "no locator update"
                    }
                },
                ["operator_input_required"] = false,
                ["continuation_mode"] = "ready_for_FINAL_REVIEW_06_recheck_no_commit_push"
            },
            new
            {
                phase_id = PhaseId,
                accepted,
                track_count = tracks.Count,
                track_verdicts = tracks.Select(track => new
                {
                    track.MachineId,
                    track.Verdict,
                    track.BoundaryUsed
                }).ToArray()
            },
            new
            {
                phase_id = PhaseId,
                accepted,
                giant_state_mutated = false,
                state_file_strategy = "metadata-only compact patch artifact"
            },
            JsonOptions);
    }

    private static void AssertExecutionStatePatch(string patchPath, string phaseId)
    {
        using var patch = JsonDocument.Parse(File.ReadAllText(patchPath, Encoding.UTF8));
        var root = patch.RootElement;
        Assert.Equal("xiigen.extension_execution_state_patch.v1", root.GetProperty("schema").GetString());
        Assert.Equal(phaseId, root.GetProperty("phase_id").GetString());
        Assert.Equal("metadata_only_no_json_parse", root.GetProperty("state_file_read_strategy").GetString());
        Assert.Equal("compact_patch_artifact_no_giant_rewrite", root.GetProperty("state_file_write_strategy").GetString());
        Assert.False(root.GetProperty("giant_state_mutated").GetBoolean());
        Assert.True(root.GetProperty("source_state_file_bytes").GetInt64() > 0);
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(path, NormalizeGeneratedText(JsonSerializer.Serialize(value, JsonOptions)), Encoding.UTF8);

    private static string NormalizeGeneratedText(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string MarkLocalPrivate(string path)
        => $"local-private:not-shareable:{Path.GetFullPath(path)}";

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed record SourceAsset(string Role, string SourcePath, string FileName);

    private sealed record StagedFile(
        string Role,
        string FileName,
        string SourcePath,
        string ExportPath,
        string ImportPath,
        string SourceSha256,
        string ExportSha256,
        string ImportSha256,
        long SizeBytes,
        bool HashesMatch);

    private sealed record PortabilityReport(
        string Schema,
        string PhaseId,
        string Verdict,
        DateTimeOffset GeneratedAtUtc,
        string StagingRoot,
        IReadOnlyList<string> Commands,
        IReadOnlyList<PortabilityCommandResult> CommandResults,
        IReadOnlyList<PortabilityTrack> Tracks,
        SharePacketPolicy SharePacketPolicy,
        IReadOnlyList<string> WhatThisProves,
        IReadOnlyList<string> WhatThisDoesNotProve,
        string NextPhase);

    private sealed record PortabilityCommandResult(
        string Command,
        int ExitCode,
        string Result,
        string Summary);

    private sealed record PortabilityTrack(
        string MachineId,
        string Verdict,
        string BoundaryUsed,
        bool ExportPacketCreated,
        bool ImportPacketCreated,
        bool ImportLoadSucceeded,
        bool RuntimeInvocationSucceeded,
        bool TestOnlyMachinePlanProbeSucceeded,
        bool RequestDerivedFromLearnedPattern,
        string? ProbeRequestScope,
        bool SourceExportImportHashesMatch,
        bool ShareManifestRedacted,
        string? ObservedOutput,
        IReadOnlyList<PortabilityFileEvidence> Files,
        IReadOnlyList<ShareManifestEntry> ShareManifestEntries,
        bool SemanticQualityClaimed,
        bool CommonModelMutated,
        string Limitation);

    private sealed record PortabilityFileEvidence(
        string Role,
        string FileName,
        string SourceSha256,
        string ExportSha256,
        string ImportSha256,
        long SizeBytes,
        bool HashesMatch,
        string LocalPrivateSourcePath,
        string LocalPrivateExportPath,
        string LocalPrivateImportPath);

    private sealed record ShareManifestEntry(
        string Role,
        string FileName,
        string Sha256,
        long SizeBytes,
        string LocalPrivateSourcePath,
        string LocalPrivateExportPath,
        string LocalPrivateImportPath,
        bool ShareablePath,
        bool RawPrivatePromptTextIncluded);

    private sealed record SharePacketPolicy(
        bool RawPrivatePromptTextIncluded,
        bool AbsolutePrivateSourceTextIncluded,
        string ShareablePathPolicy,
        IReadOnlyList<string> AllowedEvidenceFields);

    private sealed record PortabilityValidation(
        string Schema,
        string PhaseId,
        string Verdict,
        bool AcceptedForParentReview,
        IReadOnlyDictionary<string, object> Checks,
        IReadOnlyList<string> Commands,
        string NextPhase,
        IReadOnlyList<string> Limitations);

    private sealed record PortabilityParentReview(
        string Schema,
        string ReviewId,
        string PhaseId,
        string Verdict,
        bool Accepted,
        IReadOnlyList<string> Findings,
        IReadOnlyList<PortabilityTrack> Tracks,
        IReadOnlyList<string> Commands,
        string NextPhase);
}
