// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class Phase08CommonUserTwoLegLifecycleTests
{
    private static readonly Phase08CommonUserTwoLegLifecycleService Service = new();
    private static readonly Regex WindowsAbsoluteLocalPathPattern = new(@"(?<![A-Za-z0-9])[A-Za-z]:[\\/]", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public void Phase08LifecycleIntegrationWritesEvidenceLeafAndRejectsNegativePaths()
    {
        var repoRoot = FindRepoRoot();
        var evidenceDir = Path.Combine(
            repoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "phase-08-common-user-two-leg-lifecycle");
        var workDir = Path.Combine(evidenceDir, "test-work");
        ResetDirectory(workDir);

        var commonStateRoot = Path.Combine(workDir, "common-leg-root");
        var userA = Path.Combine(workDir, "user-profile-a");
        var userB = Path.Combine(workDir, "user-profile-b");
        Directory.CreateDirectory(commonStateRoot);
        Directory.CreateDirectory(userA);
        Directory.CreateDirectory(userB);

        var rows = Service.BuildTwoLegLifecycleRows(commonStateRoot, userA);
        Assert.True(rows.IsSuccess, rows.ErrorMessage);
        Assert.Equal(4, rows.Value!.Count);

        var skillSelection = rows.Value.Single(row => row.SphereId == "skill-selection");
        var commonManifestPath = Path.Combine(commonStateRoot, "common-upgrade-sandbox", "skill-selection-common-manifest.json");
        WriteText(commonManifestPath, """{"schemaVersion":1,"commonVersion":"v1","owner":"llm_mvp_core"}""");
        var commonManifestSha = Sha256File(commonManifestPath);
        var personalCheckpointPath = Path.Combine(
            userA,
            skillSelection.PersonalCheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var seeded = Service.SeedPersonalCheckpoint(
            personalCheckpointPath,
            skillSelection.SphereId,
            skillSelection.CommonModelManifestRef,
            commonManifestSha,
            ["phase08-signal-1", "phase08-signal-2"]);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage);

        var storeManifestPath = Path.Combine(workDir, "user-local-store-manifest.json");
        var storeManifest = Service.WriteUserLocalStoreManifest(
            commonStateRoot,
            userA,
            "phase08-user-a",
            rows.Value,
            storeManifestPath);
        Assert.True(storeManifest.IsSuccess, storeManifest.ErrorMessage);
        Assert.True(storeManifest.Value!.PersonalStateUnderUserRoot);
        Assert.True(storeManifest.Value.PersonalStateOutsideCommonRepoRoot);
        Assert.False(storeManifest.Value.SilentSyncEnabled);

        var export = Service.ExportUserModel(new Phase08BackupExportRequest(
            SphereId: skillSelection.SphereId,
            UserProfileId: "phase08-user-a",
            PersonalCheckpointPath: personalCheckpointPath,
            UserStoreManifestPath: storeManifestPath,
            ExportDirectory: Path.Combine(evidenceDir, "backup-export-package")));
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.True(export.Value!.Package.BackupCreated);
        Assert.True(export.Value.Package.ExportCreated);
        Assert.True(export.Value.SourceHashesVerified);
        Assert.All(export.Value.Package.Files, file => Assert.False(file.ContainsAbsoluteUserPath));
        AssertPortableArtifact(export.Value.PackageManifestPath, commonStateRoot, userA);
        AssertPortableArtifact(Path.Combine(Path.GetDirectoryName(export.Value.PackageManifestPath)!, "personal-adaptive-checkpoint.json"), commonStateRoot, userA);
        AssertPortableArtifact(Path.Combine(Path.GetDirectoryName(export.Value.PackageManifestPath)!, "user-local-store-manifest.json"), commonStateRoot, userA);

        var restore = Service.RestoreUserModel(new Phase08RestoreImportRequest(
            PackageManifestPath: export.Value.PackageManifestPath,
            FreshUserLocalRoot: userB,
            FreshUserProfileId: "phase08-fresh-user-b"));
        Assert.True(restore.IsSuccess, restore.ErrorMessage);
        Assert.True(restore.Value!.ImportedWithoutRetrain);
        Assert.True(restore.Value.FreshProfileUsed);
        Assert.True(restore.Value.SourceImportHashesMatch);
        AssertPortableArtifact(restore.Value.ImportManifestPath, commonStateRoot, userA);
        AssertPortableArtifact(restore.Value.ImportedCheckpointPath, commonStateRoot, userA);

        var unapprovedShare = Service.CreateApprovedShareCopy(new Phase08ShareCopyRequest(
            PackageManifestPath: export.Value.PackageManifestPath,
            ShareDirectory: Path.Combine(evidenceDir, "share-unapproved"),
            UserApprovalRecorded: false,
            Anonymize: true,
            SilentSyncRequested: false));
        Assert.False(unapprovedShare.IsSuccess);
        Assert.Equal("PHASE08_SHARE_APPROVAL_REQUIRED", unapprovedShare.ErrorCode);

        var silentShare = Service.CreateApprovedShareCopy(new Phase08ShareCopyRequest(
            PackageManifestPath: export.Value.PackageManifestPath,
            ShareDirectory: Path.Combine(evidenceDir, "share-silent-sync"),
            UserApprovalRecorded: true,
            Anonymize: true,
            SilentSyncRequested: true));
        Assert.False(silentShare.IsSuccess);
        Assert.Equal("PHASE08_SILENT_SYNC_FORBIDDEN", silentShare.ErrorCode);

        var approvedShare = Service.CreateApprovedShareCopy(new Phase08ShareCopyRequest(
            PackageManifestPath: export.Value.PackageManifestPath,
            ShareDirectory: Path.Combine(evidenceDir, "approved-anonymized-share"),
            UserApprovalRecorded: true,
            Anonymize: true,
            SilentSyncRequested: false));
        Assert.True(approvedShare.IsSuccess, approvedShare.ErrorMessage);
        Assert.True(approvedShare.Value!.Anonymized);
        Assert.False(approvedShare.Value.SilentSyncEnabled);
        Assert.False(approvedShare.Value.ContainsRawPrivateText);
        Assert.False(approvedShare.Value.ContainsAbsolutePrivatePath);
        Assert.All(approvedShare.Value.Entries, entry => Assert.False(entry.AbsolutePrivatePathIncluded));

        var missingCheckpointContinue = Service.ContinueTrainingFromCheckpoint(new Phase08ContinueTrainingRequest(
            SphereId: skillSelection.SphereId,
            ExistingCheckpointPath: Path.Combine(workDir, "missing-checkpoint.json"),
            NewTrainingSignalIds: ["phase08-signal-3"],
            RequireExistingCheckpoint: true,
            ReportPath: Path.Combine(workDir, "missing-continue-report.json")));
        Assert.False(missingCheckpointContinue.IsSuccess);
        Assert.Equal("PHASE08_CONTINUE_REQUIRES_CHECKPOINT", missingCheckpointContinue.ErrorCode);

        var commonMutationContinue = Service.ContinueTrainingFromCheckpoint(new Phase08ContinueTrainingRequest(
            SphereId: skillSelection.SphereId,
            ExistingCheckpointPath: restore.Value.ImportedCheckpointPath,
            NewTrainingSignalIds: ["phase08-signal-3"],
            RequireExistingCheckpoint: true,
            ReportPath: Path.Combine(workDir, "common-mutation-continue-report.json"),
            AttemptCommonCheckpointMutation: true));
        Assert.False(commonMutationContinue.IsSuccess);
        Assert.Equal("PHASE08_COMMON_MUTATION_REJECTED", commonMutationContinue.ErrorCode);

        var continueTraining = Service.ContinueTrainingFromCheckpoint(new Phase08ContinueTrainingRequest(
            SphereId: skillSelection.SphereId,
            ExistingCheckpointPath: restore.Value.ImportedCheckpointPath,
            NewTrainingSignalIds: ["phase08-signal-3"],
            RequireExistingCheckpoint: true,
            ReportPath: Path.Combine(evidenceDir, "phase-08-continue-training-report.json")));
        Assert.True(continueTraining.IsSuccess, continueTraining.ErrorMessage);
        Assert.True(continueTraining.Value!.ContinuedFromExistingCheckpoint);
        Assert.False(continueTraining.Value.RetrainedFromScratch);
        Assert.True(continueTraining.Value.MetricImproved);
        Assert.Equal(2, continueTraining.Value.MetricBefore);
        Assert.Equal(3, continueTraining.Value.MetricAfter);
        Assert.False(continueTraining.Value.CommonCheckpointMutated);

        var overwriteAttempt = Service.ApplyCommonUpgrade(new Phase08CommonUpgradeRequest(
            CommonManifestPath: commonManifestPath,
            NewCommonManifestContent: """{"schemaVersion":1,"commonVersion":"v2","owner":"llm_mvp_core"}""",
            PersonalCheckpointPath: personalCheckpointPath,
            RollbackRecordPath: Path.Combine(workDir, "rejected-rollback.json"),
            AttemptPersonalOverwrite: true));
        Assert.False(overwriteAttempt.IsSuccess);
        Assert.Equal("PHASE08_PERSONAL_OVERWRITE_REJECTED", overwriteAttempt.ErrorCode);

        var upgrade = Service.ApplyCommonUpgrade(new Phase08CommonUpgradeRequest(
            CommonManifestPath: commonManifestPath,
            NewCommonManifestContent: """{"schemaVersion":1,"commonVersion":"v2","owner":"llm_mvp_core"}""",
            PersonalCheckpointPath: personalCheckpointPath,
            RollbackRecordPath: Path.Combine(evidenceDir, "phase-08-common-upgrade-rollback-record.json"),
            AttemptPersonalOverwrite: false));
        Assert.True(upgrade.IsSuccess, upgrade.ErrorMessage);
        Assert.True(upgrade.Value!.CommonManifestUpdated);
        Assert.True(upgrade.Value.PersonalCheckpointPreserved);
        Assert.True(upgrade.Value.RollbackRecordCreated);
        AssertPortableArtifact(upgrade.Value.RollbackRecordPath, repoRoot, workDir, commonStateRoot, userA, userB);

        using var rollbackRecordDocument = JsonDocument.Parse(File.ReadAllText(upgrade.Value.RollbackRecordPath, Encoding.UTF8));
        Assert.False(rollbackRecordDocument.RootElement.TryGetProperty("commonManifestPath", out _));
        Assert.StartsWith(
            "portable://runtime/",
            rollbackRecordDocument.RootElement.GetProperty("commonManifestPortableRef").GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            "skill-selection-common-manifest",
            rollbackRecordDocument.RootElement.GetProperty("commonManifestId").GetString());
        Assert.True(rollbackRecordDocument.RootElement.GetProperty("rootRemapTable").GetArrayLength() >= 2);

        var rollback = Service.RollbackCommonUpgrade(
            upgrade.Value.RollbackRecordPath,
            personalCheckpointPath);
        Assert.True(rollback.IsSuccess, rollback.ErrorMessage);
        Assert.True(rollback.Value!.RestoredPreviousCommonManifest);
        Assert.True(rollback.Value.PersonalCheckpointUntouched);

        var matrix = Service.BuildCoverageMatrix(
            rows.Value,
            new HashSet<string>(StringComparer.Ordinal) { skillSelection.SphereId });
        Assert.True(matrix.IsSuccess, matrix.ErrorMessage);
        Assert.False(matrix.Value!.Passed);
        Assert.Equal(
            "covered_by_executable_probe",
            matrix.Value.Rows.Single(row => row.SphereName == "skill-selection").Portability.ImplementationStatus);
        Assert.All(
            matrix.Value.Rows.Where(row => row.SphereName != "skill-selection"),
            row => Assert.Equal("declared_required_but_not_yet_executed", row.Portability.ImplementationStatus));

        var report = BuildReport(
            matrix.Value,
            storeManifest.Value,
            export.Value.Package,
            restore.Value,
            approvedShare.Value,
            continueTraining.Value,
            upgrade.Value,
            rollback.Value);
        var evidenceLeaf = Service.WriteEvidenceLeaf(evidenceDir, report);
        Assert.True(evidenceLeaf.IsSuccess, evidenceLeaf.ErrorMessage);
        Assert.True(File.Exists(evidenceLeaf.Value!.JsonPath));
        Assert.True(File.Exists(evidenceLeaf.Value.JsonlPath));
        Assert.True(File.Exists(evidenceLeaf.Value.MarkdownPath));

        using var reportDocument = JsonDocument.Parse(File.ReadAllText(evidenceLeaf.Value.JsonPath, Encoding.UTF8));
        Assert.Equal("Common/user two-leg learning lifecycle", reportDocument.RootElement.GetProperty("phaseName").GetString());
        Assert.False(reportDocument.RootElement.GetProperty("twoLegMatrix").GetProperty("passed").GetBoolean());
        Assert.Contains("declared-not-yet-executed", reportDocument.RootElement.GetProperty("whatItSolvesRu").GetString(), StringComparison.Ordinal);
        Assert.Equal(7, File.ReadLines(evidenceLeaf.Value.JsonlPath, Encoding.UTF8).Count(line => !string.IsNullOrWhiteSpace(line)));
        AssertPortableArtifactTree(evidenceDir, repoRoot, workDir, commonStateRoot, userA, userB);
        AssertNoMojibakeOrEscapedCyrillic(evidenceDir);
    }

    [Fact]
    public void Phase08UserLocalManifestRejectsPersonalRootInsideCommonRepo()
    {
        var repoRoot = FindRepoRoot();
        var workDir = Path.Combine(
            repoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "phase-08-common-user-two-leg-lifecycle",
            "test-work-negative");
        ResetDirectory(workDir);

        var commonRepo = Path.Combine(workDir, "common-leg-root");
        var userInsideCommon = Path.Combine(commonRepo, "user-profile");
        Directory.CreateDirectory(userInsideCommon);

        var rows = Service.BuildTwoLegLifecycleRows(commonRepo, userInsideCommon);
        Assert.True(rows.IsSuccess, rows.ErrorMessage);

        var manifest = Service.WriteUserLocalStoreManifest(
            commonRepo,
            userInsideCommon,
            "bad-user",
            rows.Value!,
            Path.Combine(commonRepo, "manifest.json"));

        Assert.False(manifest.IsSuccess);
        Assert.Equal("PHASE08_USER_ROOT_INSIDE_COMMON_REPO", manifest.ErrorCode);
    }

    [Fact]
    public void Phase08CoverageMatrixSeparatesExecutableProofFromDeclaredRequiredRows()
    {
        var repoRoot = FindRepoRoot();
        var userRoot = Path.Combine(Path.GetTempPath(), "xiigen-phase08-user", Guid.NewGuid().ToString("N"));
        var rows = Service.BuildTwoLegLifecycleRows(repoRoot, userRoot);
        Assert.True(rows.IsSuccess, rows.ErrorMessage);

        var declaredOnlyMatrix = Service.BuildCoverageMatrix(rows.Value!);

        Assert.True(declaredOnlyMatrix.IsSuccess, declaredOnlyMatrix.ErrorMessage);
        Assert.False(declaredOnlyMatrix.Value!.Passed);
        Assert.All(declaredOnlyMatrix.Value.Rows, row =>
        {
            Assert.Equal("present", row.CommonGlobalLeg.ImplementationStatus);
            Assert.Equal("present", row.PersonalAdaptiveLeg.ImplementationStatus);
            Assert.Equal("declared_required_but_not_yet_executed", row.Portability.ImplementationStatus);
            Assert.False(row.DeterministicShortcutSubstituted);
            Assert.Equal("executable lifecycle probe not yet run for this exact sphere", row.Gap);
        });

        var skillSelectionOnlyMatrix = Service.BuildCoverageMatrix(
            rows.Value!,
            new HashSet<string>(StringComparer.Ordinal) { "skill-selection" });
        Assert.True(skillSelectionOnlyMatrix.IsSuccess, skillSelectionOnlyMatrix.ErrorMessage);
        Assert.False(skillSelectionOnlyMatrix.Value!.Passed);
        Assert.Equal(
            "covered_by_executable_probe",
            skillSelectionOnlyMatrix.Value.Rows.Single(row => row.SphereName == "skill-selection").Portability.ImplementationStatus);
        Assert.All(
            skillSelectionOnlyMatrix.Value.Rows.Where(row => row.SphereName != "skill-selection"),
            row => Assert.Equal("declared_required_but_not_yet_executed", row.Portability.ImplementationStatus));

        var allSphereMatrix = Service.BuildCoverageMatrix(
            rows.Value!,
            rows.Value!.Select(row => row.SphereId).ToHashSet(StringComparer.Ordinal));
        Assert.True(allSphereMatrix.IsSuccess, allSphereMatrix.ErrorMessage);
        Assert.True(allSphereMatrix.Value!.Passed);
        Assert.All(allSphereMatrix.Value.Rows, row =>
        {
            Assert.Equal("covered_by_executable_probe", row.Portability.ImplementationStatus);
            Assert.Equal("none", row.Gap);
        });
    }

    private static Phase08LifecycleEvidenceReport BuildReport(
        LearningMachinePairCoverageMatrix matrix,
        Phase08UserLocalStoreManifest storeManifest,
        Phase08BackupExportPackage backupExport,
        Phase08RestoreImportResult restore,
        Phase08ShareCopyResult share,
        Phase08ContinueTrainingResult continueTraining,
        Phase08CommonUpgradeResult upgrade,
        Phase08RollbackResult rollback)
    {
        var checks = new[]
        {
            new Phase08LifecycleCheckEvidence(
                Substep: "08.02",
                CheckName: "common/user two-leg matrix",
                Passed: !matrix.Passed
                    && matrix.Rows.Count == 4
                    && matrix.Rows.Single(row => row.SphereName == "skill-selection").Portability.ImplementationStatus == "covered_by_executable_probe"
                    && matrix.Rows.Where(row => row.SphereName != "skill-selection").All(row => row.Portability.ImplementationStatus == "declared_required_but_not_yet_executed"),
                HumanEvidence: "The matrix contains PromptToTask, OrchestrationReasoning, SkillSelection and ResultGap/correction, but the executable lifecycle proof was performed only for SkillSelection; the remaining rows are explicitly left declared-required-but-not-yet-executed.",
                PositiveExample: "SkillSelection row has covered_by_executable_probe after user-local manifest, portable export/import, approved share, continue-training and rollback/hash isolation.",
                NegativeExample: "A matrix that marks PromptToTask, OrchestrationReasoning or ResultGap/correction as fully portable without an executable probe would fail this check."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.03",
                CheckName: "user-local store manifest",
                Passed: storeManifest.PersonalStateUnderUserRoot
                    && storeManifest.PersonalStateOutsideCommonRepoRoot
                    && !storeManifest.SilentSyncEnabled,
                HumanEvidence: "Store manifest keeps personal checkpoint refs under the user root and outside the common repo root.",
                PositiveExample: "User profile A has user-local checkpoint entries with Shareable=false.",
                NegativeExample: "A user root inside common repo is rejected with PHASE08_USER_ROOT_INSIDE_COMMON_REPO."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.04",
                CheckName: "backup/export package",
                Passed: backupExport.BackupCreated
                    && backupExport.ExportCreated
                    && !backupExport.ContainsRawPrivateText,
                HumanEvidence: "Backup/export package copies the personal checkpoint and store manifest with hashes.",
                PositiveExample: "Export package contains personal-adaptive-checkpoint.json and user-local-store-manifest.json.",
                NegativeExample: "A missing checkpoint is rejected with PHASE08_EXPORT_CHECKPOINT_REQUIRED."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.05",
                CheckName: "restore/import fresh profile",
                Passed: restore.ImportedWithoutRetrain
                    && restore.FreshProfileUsed
                    && restore.SourceImportHashesMatch,
                HumanEvidence: "Fresh profile import copies the exported checkpoint and verifies the same hash without retraining.",
                PositiveExample: "User profile B imports the checkpoint and keeps the exported hash.",
                NegativeExample: "A package without personal checkpoint would fail PHASE08_IMPORT_CHECKPOINT_MISSING."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.06",
                CheckName: "approved anonymized share/copy",
                Passed: share.UserApprovalRecorded
                    && share.Anonymized
                    && !share.SilentSyncEnabled
                    && !share.ContainsAbsolutePrivatePath,
                HumanEvidence: "Share/copy creates only an anonymized manifest after explicit approval and disables silent sync.",
                PositiveExample: "Approved share manifest contains role, file name, hash and size only.",
                NegativeExample: "Unapproved share and silent-sync requests are rejected before any share manifest is accepted."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.07",
                CheckName: "continue-training from checkpoint",
                Passed: continueTraining.ContinuedFromExistingCheckpoint
                    && !continueTraining.RetrainedFromScratch
                    && continueTraining.MetricAfter > continueTraining.MetricBefore
                    && !continueTraining.CommonCheckpointMutated,
                HumanEvidence: "Continue-training loads the imported checkpoint, appends a new signal id, and does not mutate common state.",
                PositiveExample: "MetricBefore=2 and MetricAfter=3 after adding phase08-signal-3.",
                NegativeExample: "Missing checkpoint and attempted common mutation are rejected."),
            new Phase08LifecycleCheckEvidence(
                Substep: "08.08",
                CheckName: "rollback/hash isolation",
                Passed: upgrade.CommonManifestUpdated
                    && upgrade.PersonalCheckpointPreserved
                    && upgrade.RollbackRecordCreated
                    && rollback.RestoredPreviousCommonManifest
                    && rollback.PersonalCheckpointUntouched,
                HumanEvidence: "Common upgrade changes only the common manifest, preserves personal checkpoint hash, and rollback restores the previous common manifest.",
                PositiveExample: "Personal checkpoint hash before and after common upgrade is identical.",
                NegativeExample: "A common upgrade that attempts personal overwrite is rejected with PHASE08_PERSONAL_OVERWRITE_REJECTED.")
        };

        return new Phase08LifecycleEvidenceReport(
            PhaseName: "Common/user two-leg learning lifecycle",
            WhatItSolvesRu: "The phase proves the executable common/user lifecycle only for SkillSelection. PromptToTask, OrchestrationReasoning and ResultGap/correction remain declared-not-yet-executed: their common/user legs are required and described, but a full lifecycle probe for them is not claimed.",
            CoveredSubsteps: ["08.02", "08.03", "08.04", "08.05", "08.06", "08.07", "08.08"],
            TwoLegMatrix: matrix,
            UserLocalStoreManifest: storeManifest,
            BackupExportPackage: backupExport,
            RestoreImportResult: restore,
            ShareCopyResult: share,
            ContinueTrainingResult: continueTraining,
            CommonUpgradeResult: upgrade,
            RollbackResult: rollback,
            Checks: checks,
            ChangedFiles:
            [
                "43.SmartSkillsDomain/Phase08CommonUserTwoLegLifecycle.cs",
                "Tests/Phase08CommonUserTwoLegLifecycleTests.cs",
                "artifacts/extension-runtime-token-quality-learnability/phase-08-common-user-two-leg-lifecycle/phase-08-lifecycle-evidence.json",
                "artifacts/extension-runtime-token-quality-learnability/phase-08-common-user-two-leg-lifecycle/phase-08-lifecycle-checks.jsonl",
                "artifacts/extension-runtime-token-quality-learnability/phase-08-common-user-two-leg-lifecycle/phase-08-lifecycle-report.ru.md"
            ],
            ExplicitNonClaims:
            [
                "Full semantic quality is not claimed.",
                "Full extension readiness for Claude, Cursor or Codex is not claimed.",
                "Token savings are not claimed.",
                "Final project readiness is not claimed.",
                "An executable lifecycle proof for PromptToTask, OrchestrationReasoning and ResultGap/correction is not claimed.",
                "No host live proof was run and phase 09 was not started."
            ],
            DashboardApiQaBoundary: "In Phase 08 the dashboard/API project was not changed, because the allowed write scope does not include 43.SmartSkillsDomain.Dashboard. The proof remains domain-service + xUnit + evidence leaf; a separate dashboard scenario must later show the same lifecycle rows through the UI/API without claiming host runtime proof.");
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void AssertPortableArtifact(string path, params string[] staleRoots)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        AssertNoAbsoluteLocalPaths(path, text, staleRoots);
    }

    private static void AssertPortableArtifactTree(string directory, params string[] staleRoots)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            AssertPortableArtifact(file, staleRoots);
        }
    }

    private static void AssertNoAbsoluteLocalPaths(string path, string text, params string[] staleRoots)
    {
        Assert.False(WindowsAbsoluteLocalPathPattern.IsMatch(text), $"{path} contains a Windows absolute path.");
        Assert.DoesNotContain("/Users/", text.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
        foreach (var staleRoot in staleRoots)
        {
            var fullRoot = Path.GetFullPath(staleRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            Assert.DoesNotContain(fullRoot.Replace('\\', '/'), text.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(fullRoot, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private static void AssertNoMojibakeOrEscapedCyrillic(string directory)
    {
        var forbidden = new[]
        {
            ((char)0x00D0).ToString(),
            ((char)0x00D1).ToString(),
            ((char)0xFFFD).ToString(),
            ((char)0x00E2).ToString(),
            ((char)0x00C3).ToString(),
            string.Concat("??", "??"),
            string.Concat("\\u", "04"),
            string.Concat("\\u", "05")
        };
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            foreach (var marker in forbidden)
            {
                Assert.DoesNotContain(marker, text, StringComparison.Ordinal);
            }
        }
    }
}
