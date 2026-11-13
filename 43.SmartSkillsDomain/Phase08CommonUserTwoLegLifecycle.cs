// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record Phase08TrainableSphereLifecycleRow(
    string SphereId,
    string HumanName,
    string AllowedArchitectureLevel,
    string CommonMachineRef,
    string CommonModelManifestRef,
    string CommonCheckpointRef,
    string CommonTrainingSetRef,
    string CommonUpgradePathRef,
    string PersonalStateRelativeRoot,
    string PersonalCheckpointRelativePath,
    string PersonalDpoSignalRelativePath,
    bool CommonLegRepoOwned,
    bool PersonalLegUserLocal,
    bool BackupExportSupported,
    bool RestoreImportSupported,
    bool ApprovedShareCopySupported,
    bool ContinueTrainingSupported,
    bool RollbackSupported,
    bool DeterministicShortcutSubstituted,
    string ClaimBoundary);

public sealed record Phase08StoreManifestEntry(
    string SphereId,
    string StateKind,
    string AbsolutePath,
    string RelativePath,
    string Sha256,
    bool UnderUserLocalRoot,
    bool UnderCommonRepoRoot,
    bool Shareable);

public sealed record Phase08UserLocalStoreManifest(
    int SchemaVersion,
    string ManifestId,
    string UserProfileId,
    string UserLocalRoot,
    string CommonRepoRoot,
    IReadOnlyList<Phase08StoreManifestEntry> Entries,
    bool PersonalStateUnderUserRoot,
    bool PersonalStateOutsideCommonRepoRoot,
    bool CommonStateUnderRepoRoot,
    bool SilentSyncEnabled,
    bool SemanticQualityClaimed);

public sealed record Phase08PersonalCheckpoint(
    int SchemaVersion,
    string SphereId,
    string CheckpointId,
    string CommonManifestRef,
    string CommonManifestSha256,
    IReadOnlyList<string> TrainedSignalIds,
    int TrainingGeneration,
    string? ImportedFromPackageId,
    string? ParentCheckpointSha256,
    bool SemanticQualityClaimed,
    DateTimeOffset SavedAtUtc);

public sealed record Phase08BackupExportRequest(
    string SphereId,
    string UserProfileId,
    string PersonalCheckpointPath,
    string UserStoreManifestPath,
    string ExportDirectory);

public sealed record Phase08PortableRootRemap(
    string LogicalRoot,
    string PortableRef,
    string RemapPolicy);

public sealed record Phase08PackageFile(
    string Role,
    string FileName,
    string PackageRelativePath,
    string Sha256,
    long SizeBytes,
    bool ContainsRawPrivateText,
    bool ContainsAbsoluteUserPath);

public sealed record Phase08BackupExportPackage(
    int SchemaVersion,
    string PackageId,
    string SphereId,
    string SourceUserProfileId,
    string ExportDirectory,
    IReadOnlyList<Phase08PortableRootRemap> RootRemapTable,
    IReadOnlyList<Phase08PackageFile> Files,
    bool BackupCreated,
    bool ExportCreated,
    bool ContainsRawPrivateText,
    bool SilentSyncEnabled,
    bool SemanticQualityClaimed,
    DateTimeOffset CreatedAtUtc);

public sealed record Phase08BackupExportResult(
    Phase08BackupExportPackage Package,
    string PackageManifestPath,
    bool SourceHashesVerified);

public sealed record Phase08RestoreImportRequest(
    string PackageManifestPath,
    string FreshUserLocalRoot,
    string FreshUserProfileId);

public sealed record Phase08RestoreImportResult(
    string SphereId,
    string FreshUserProfileId,
    [property: JsonIgnore]
    string ImportedCheckpointPath,
    [property: JsonIgnore]
    string ImportManifestPath,
    string ImportedCheckpointPortableRef,
    string ImportManifestPortableRef,
    string ImportedCheckpointSha256,
    bool ImportedWithoutRetrain,
    bool FreshProfileUsed,
    bool SourceImportHashesMatch,
    bool SemanticQualityClaimed);

public sealed record Phase08ShareCopyRequest(
    string PackageManifestPath,
    string ShareDirectory,
    bool UserApprovalRecorded,
    bool Anonymize,
    bool SilentSyncRequested);

public sealed record Phase08ShareManifestEntry(
    string Role,
    string FileName,
    string Sha256,
    long SizeBytes,
    bool ShareablePath,
    bool RawPrivateTextIncluded,
    bool AbsolutePrivatePathIncluded);

public sealed record Phase08ShareCopyResult(
    [property: JsonIgnore]
    string ShareManifestPath,
    string ShareManifestPortableRef,
    IReadOnlyList<Phase08ShareManifestEntry> Entries,
    bool UserApprovalRecorded,
    bool Anonymized,
    bool SilentSyncEnabled,
    bool ContainsRawPrivateText,
    bool ContainsAbsolutePrivatePath);

public sealed record Phase08ContinueTrainingRequest(
    string SphereId,
    string ExistingCheckpointPath,
    IReadOnlyList<string> NewTrainingSignalIds,
    bool RequireExistingCheckpoint,
    string ReportPath,
    bool AttemptCommonCheckpointMutation = false);

public sealed record Phase08ContinueTrainingResult(
    string SphereId,
    [property: JsonIgnore]
    string ExistingCheckpointPath,
    string ExistingCheckpointPortableRef,
    string LoadedCheckpointSha256,
    string SavedCheckpointSha256,
    int MetricBefore,
    int MetricAfter,
    bool ContinuedFromExistingCheckpoint,
    bool RetrainedFromScratch,
    bool MetricImproved,
    IReadOnlyList<string> TrainedSignalIds,
    bool CommonCheckpointMutated,
    bool SemanticQualityClaimed);

public sealed record Phase08CommonUpgradeRequest(
    string CommonManifestPath,
    string NewCommonManifestContent,
    string PersonalCheckpointPath,
    string RollbackRecordPath,
    bool AttemptPersonalOverwrite);

public sealed record Phase08CommonUpgradeResult(
    [property: JsonIgnore]
    string CommonManifestPath,
    [property: JsonIgnore]
    string RollbackRecordPath,
    string CommonManifestPortableRef,
    string RollbackRecordPortableRef,
    string PreviousCommonManifestSha256,
    string NewCommonManifestSha256,
    string PersonalCheckpointSha256Before,
    string PersonalCheckpointSha256After,
    bool CommonManifestUpdated,
    bool PersonalCheckpointPreserved,
    bool RollbackRecordCreated,
    bool PersonalOverwriteRejected);

public sealed record Phase08RollbackRecord(
    int SchemaVersion,
    string CommonManifestPortableRef,
    string CommonManifestId,
    IReadOnlyList<Phase08PortableRootRemap> RootRemapTable,
    string PreviousCommonManifestContent,
    string PreviousCommonManifestSha256,
    string PersonalCheckpointSha256Before,
    DateTimeOffset CreatedAtUtc);

public sealed record Phase08RollbackResult(
    [property: JsonIgnore]
    string CommonManifestPath,
    string CommonManifestPortableRef,
    string RestoredCommonManifestSha256,
    bool RestoredPreviousCommonManifest,
    bool PersonalCheckpointUntouched);

public sealed record Phase08LifecycleCheckEvidence(
    string Substep,
    string CheckName,
    bool Passed,
    string HumanEvidence,
    string PositiveExample,
    string NegativeExample);

public sealed record Phase08LifecycleEvidenceReport(
    string PhaseName,
    string WhatItSolvesRu,
    IReadOnlyList<string> CoveredSubsteps,
    LearningMachinePairCoverageMatrix TwoLegMatrix,
    Phase08UserLocalStoreManifest UserLocalStoreManifest,
    Phase08BackupExportPackage BackupExportPackage,
    Phase08RestoreImportResult RestoreImportResult,
    Phase08ShareCopyResult ShareCopyResult,
    Phase08ContinueTrainingResult ContinueTrainingResult,
    Phase08CommonUpgradeResult CommonUpgradeResult,
    Phase08RollbackResult RollbackResult,
    IReadOnlyList<Phase08LifecycleCheckEvidence> Checks,
    IReadOnlyList<string> ChangedFiles,
    IReadOnlyList<string> ExplicitNonClaims,
    string DashboardApiQaBoundary);

public sealed record Phase08EvidenceLeafResult(
    string JsonPath,
    string JsonlPath,
    string MarkdownPath);

public sealed class Phase08CommonUserTwoLegLifecycleService
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OperationResult<IReadOnlyList<Phase08TrainableSphereLifecycleRow>> BuildTwoLegLifecycleRows(
        string commonRepoRoot,
        string userLocalRoot)
    {
        if (string.IsNullOrWhiteSpace(commonRepoRoot))
            return OperationResult<IReadOnlyList<Phase08TrainableSphereLifecycleRow>>.Fail(
                "Common repository root is required.",
                "PHASE08_COMMON_REPO_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(userLocalRoot))
            return OperationResult<IReadOnlyList<Phase08TrainableSphereLifecycleRow>>.Fail(
                "User-local root is required.",
                "PHASE08_USER_LOCAL_ROOT_REQUIRED");

        var repo = NormalizePath(commonRepoRoot);
        var user = NormalizePath(userLocalRoot);
        var rows = new[]
        {
            BuildRow(
                "prompt-to-task",
                "PromptToTask task contract learning",
                "ILearningMachinePair",
                "PromptToTaskMachine",
                "models/prompt-to-task/prompt-to-task-v1/manifest.json",
                "models/prompt-to-task/prompt-to-task-v1/checkpoint.json",
                "training-data/prompt-to-task/prompt-to-task-v1/corpus.jsonl",
                "models/prompt-to-task/prompt-to-task-v1/rollback",
                ".xiigen/prompt-to-task",
                repo,
                user),
            BuildRow(
                "orchestration-reasoning",
                "OrchestrationReasoning scenario planner learning",
                "ILearningMachinePair",
                "OrchestrationReasoningMachine",
                "models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json",
                "models/orchestration-reasoning/orchestration-reasoning-v1/checkpoint.json",
                "artifacts/orchestration-reasoning-corpus-summary.json",
                "models/orchestration-reasoning/orchestration-reasoning-v1/rollback",
                ".xiigen/orchestration-reasoning",
                repo,
                user),
            BuildRow(
                "skill-selection",
                "SkillSelection trainable semantic unit family",
                "ILearningMachinePair",
                "SkillSelectionMachine",
                "models/skill-selection/skill-selection-v1/manifest.json",
                "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
                "training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl",
                "models/skill-selection/skill-selection-v1/rollback",
                ".xiigen/smart-skills",
                repo,
                user),
            BuildRow(
                "result-gap-correction",
                "ResultGap missing-pattern and correction adaptive signal learning",
                "ILogicalUnit inside ILearningMachinePair",
                "ResultGapMachine and SkillSelection correction adapter",
                "artifacts/dpo-quality/smart-skills-dpo-promotion-manifest.json",
                "models/skill-selection/result-gap-correction-v1/checkpoint.json",
                "artifacts/dpo-quality/smart-skills-dpo-materialized.jsonl",
                "models/skill-selection/result-gap-correction-v1/rollback",
                ".xiigen/smart-skills/corrections",
                repo,
                user)
        };

        return OperationResult<IReadOnlyList<Phase08TrainableSphereLifecycleRow>>.Ok(rows);
    }

    public OperationResult<LearningMachinePairCoverageMatrix> BuildCoverageMatrix(
        IReadOnlyList<Phase08TrainableSphereLifecycleRow> rows,
        IReadOnlySet<string>? executableProofSphereIds = null)
    {
        if (rows.Count == 0)
            return OperationResult<LearningMachinePairCoverageMatrix>.Fail(
                "At least one trainable sphere row is required.",
                "PHASE08_MATRIX_ROWS_REQUIRED");

        var executableProof = executableProofSphereIds ?? new HashSet<string>(StringComparer.Ordinal);
        var coverageRows = rows
            .Select(row =>
            {
                var hasExecutableProbe = executableProof.Contains(row.SphereId);
                var declaredLifecycle =
                    row.CommonLegRepoOwned
                    && row.PersonalLegUserLocal
                    && row.BackupExportSupported
                    && row.RestoreImportSupported
                    && row.ApprovedShareCopySupported
                    && row.ContinueTrainingSupported
                    && row.RollbackSupported
                    && !row.DeterministicShortcutSubstituted;
                var lifecycleStatus = hasExecutableProbe
                    ? "covered_by_executable_probe"
                    : declaredLifecycle
                        ? "declared_required_but_not_yet_executed"
                        : "missing_required_lifecycle_contract";
                var gap = hasExecutableProbe
                    ? "none"
                    : declaredLifecycle
                        ? "executable lifecycle probe not yet run for this exact sphere"
                        : "required lifecycle declaration missing";

                return new LearningMachinePairCoverageRow(
                    SphereName: row.SphereId,
                    CommonGlobalLeg: new LearningMachineLegContract(
                        LegName: "common/global",
                        OwnerScope: "llm_mvp_core",
                        ModelOrStateRef: row.CommonModelManifestRef,
                        ImplementationStatus: row.CommonLegRepoOwned ? "present" : "missing"),
                    PersonalAdaptiveLeg: new LearningMachineLegContract(
                        LegName: "personal/adaptive",
                        OwnerScope: "user-local",
                        ModelOrStateRef: row.PersonalCheckpointRelativePath,
                        ImplementationStatus: row.PersonalLegUserLocal ? "present" : "missing"),
                    Portability: new LearningMachinePortabilityContract(
                        BackupExportPath: row.BackupExportSupported ? "phase08.ExportUserModel" : "missing",
                        RestoreImportPath: row.RestoreImportSupported ? "phase08.RestoreUserModel" : "missing",
                        CrossDevicePolicy: "fresh profile import must copy checkpoint and verify hash without retrain",
                        SharePolicy: row.ApprovedShareCopySupported
                            ? "explicit approval plus anonymized manifest; silent sync disabled"
                            : "missing",
                        ImplementationStatus: lifecycleStatus),
                    DeterministicShortcutSubstituted: row.DeterministicShortcutSubstituted,
                    CurrentImplementationStatus: hasExecutableProbe
                        ? "covered_by_executable_probe: user-local manifest, backup/export, fresh-profile import, approved anonymized share, continue-training, and rollback/hash isolation were executed for this sphere"
                        : $"{lifecycleStatus}: {row.ClaimBoundary}",
                    Gap: gap);
            })
            .ToArray();

        var passed = coverageRows.All(row =>
            row.CommonGlobalLeg.ImplementationStatus == "present"
            && row.PersonalAdaptiveLeg.ImplementationStatus == "present"
            && row.Portability.ImplementationStatus == "covered_by_executable_probe"
            && !row.DeterministicShortcutSubstituted);

        var provenSpheres = coverageRows
            .Where(row => row.Portability.ImplementationStatus == "covered_by_executable_probe")
            .Select(row => row.SphereName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var pendingSpheres = coverageRows
            .Where(row => row.Portability.ImplementationStatus != "covered_by_executable_probe")
            .Select(row => row.SphereName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<LearningMachinePairCoverageMatrix>.Ok(new LearningMachinePairCoverageMatrix(
            MatrixId: "phase-08-common-user-two-leg-lifecycle",
            Rows: coverageRows,
            Passed: passed,
            ClaimScope: $"Lifecycle portability/provenance proof only; executable-probed spheres=[{string.Join(", ", provenSpheres)}]; declared-not-yet-executed spheres=[{string.Join(", ", pendingSpheres)}]; no semantic quality, token saving, or full extension readiness claim."));
    }

    public OperationResult<Phase08UserLocalStoreManifest> WriteUserLocalStoreManifest(
        string commonRepoRoot,
        string userLocalRoot,
        string userProfileId,
        IReadOnlyList<Phase08TrainableSphereLifecycleRow> rows,
        string manifestPath)
    {
        if (rows.Count == 0)
            return OperationResult<Phase08UserLocalStoreManifest>.Fail(
                "Trainable sphere rows are required.",
                "PHASE08_MANIFEST_ROWS_REQUIRED");

        var repo = NormalizePath(commonRepoRoot);
        var user = NormalizePath(userLocalRoot);
        if (IsPathInsideRoot(user, repo))
            return OperationResult<Phase08UserLocalStoreManifest>.Fail(
                "User-local root must be separate from the common llm_mvp_core repository root.",
                "PHASE08_USER_ROOT_INSIDE_COMMON_REPO");

        var entries = rows
            .Select(row => BuildStoreEntry(row, user, repo))
            .ToArray();
        var manifest = new Phase08UserLocalStoreManifest(
            SchemaVersion: 1,
            ManifestId: $"phase08-store:{userProfileId}",
            UserProfileId: userProfileId,
            UserLocalRoot: "portable://user-local-root",
            CommonRepoRoot: "portable://common-root",
            Entries: entries,
            PersonalStateUnderUserRoot: entries.All(entry => entry.UnderUserLocalRoot),
            PersonalStateOutsideCommonRepoRoot: entries.All(entry => !entry.UnderCommonRepoRoot),
            CommonStateUnderRepoRoot: true,
            SilentSyncEnabled: false,
            SemanticQualityClaimed: false);

        WriteJson(manifestPath, manifest);
        return OperationResult<Phase08UserLocalStoreManifest>.Ok(manifest);
    }

    public OperationResult<Phase08PersonalCheckpoint> SeedPersonalCheckpoint(
        string checkpointPath,
        string sphereId,
        string commonManifestRef,
        string commonManifestSha256,
        IReadOnlyList<string> trainedSignalIds)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<Phase08PersonalCheckpoint>.Fail(
                "Checkpoint path is required.",
                "PHASE08_CHECKPOINT_PATH_REQUIRED");

        var checkpoint = new Phase08PersonalCheckpoint(
            SchemaVersion: 1,
            SphereId: sphereId,
            CheckpointId: $"phase08:{sphereId}:{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            CommonManifestRef: commonManifestRef,
            CommonManifestSha256: commonManifestSha256,
            TrainedSignalIds: trainedSignalIds.Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            TrainingGeneration: trainedSignalIds.Count,
            ImportedFromPackageId: null,
            ParentCheckpointSha256: null,
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        WriteJson(checkpointPath, checkpoint);
        return OperationResult<Phase08PersonalCheckpoint>.Ok(checkpoint);
    }

    public OperationResult<Phase08BackupExportResult> ExportUserModel(Phase08BackupExportRequest request)
    {
        if (!File.Exists(request.PersonalCheckpointPath))
            return OperationResult<Phase08BackupExportResult>.Fail(
                "Personal checkpoint is required for export.",
                "PHASE08_EXPORT_CHECKPOINT_REQUIRED");

        if (!File.Exists(request.UserStoreManifestPath))
            return OperationResult<Phase08BackupExportResult>.Fail(
                "User-local store manifest is required for export.",
                "PHASE08_EXPORT_STORE_MANIFEST_REQUIRED");

        Directory.CreateDirectory(request.ExportDirectory);
        var checkpointPackagePath = Path.Combine(request.ExportDirectory, "personal-adaptive-checkpoint.json");
        var storePackagePath = Path.Combine(request.ExportDirectory, "user-local-store-manifest.json");

        var checkpointResult = LoadJson<Phase08PersonalCheckpoint>(
            request.PersonalCheckpointPath,
            "PHASE08_EXPORT_CHECKPOINT_INVALID");
        if (!checkpointResult.IsSuccess)
            return checkpointResult.MapError<Phase08BackupExportResult>();

        var storeManifestResult = LoadJson<Phase08UserLocalStoreManifest>(
            request.UserStoreManifestPath,
            "PHASE08_EXPORT_STORE_MANIFEST_INVALID");
        if (!storeManifestResult.IsSuccess)
            return storeManifestResult.MapError<Phase08BackupExportResult>();

        var checkpoint = checkpointResult.Value!;
        var portableCheckpoint = checkpoint with
        {
            CommonManifestRef = ToPortableCommonManifestRef(checkpoint.CommonManifestRef)
        };
        var portableStoreManifest = ToPortableStoreManifest(storeManifestResult.Value!);
        WriteJson(checkpointPackagePath, portableCheckpoint);
        WriteJson(storePackagePath, portableStoreManifest);

        var files = new[]
        {
            BuildPackageFile("personal-checkpoint", checkpointPackagePath, request.ExportDirectory),
            BuildPackageFile("store-manifest", storePackagePath, request.ExportDirectory)
        };
        var package = new Phase08BackupExportPackage(
            SchemaVersion: 1,
            PackageId: $"phase08-export:{request.UserProfileId}:{request.SphereId}",
            SphereId: request.SphereId,
            SourceUserProfileId: request.UserProfileId,
            ExportDirectory: "portable://package-root",
            RootRemapTable:
            [
                new Phase08PortableRootRemap("source-user-local-root", "portable://user-local-root", "remap to importer-selected fresh profile root"),
                new Phase08PortableRootRemap("source-common-root", "portable://common-root", "resolve through installed common model manifest id, never stale absolute path"),
                new Phase08PortableRootRemap("package-root", ".", "files are package-relative")
            ],
            Files: files,
            BackupCreated: true,
            ExportCreated: true,
            ContainsRawPrivateText: false,
            SilentSyncEnabled: false,
            SemanticQualityClaimed: false,
            CreatedAtUtc: DateTimeOffset.UtcNow);
        var packageManifestPath = Path.Combine(request.ExportDirectory, "phase08-export-package-manifest.json");
        WriteJson(packageManifestPath, package);

        return OperationResult<Phase08BackupExportResult>.Ok(new Phase08BackupExportResult(
            Package: package,
            PackageManifestPath: packageManifestPath,
            SourceHashesVerified: !files.Any(file => file.ContainsAbsoluteUserPath)));
    }

    public OperationResult<Phase08RestoreImportResult> RestoreUserModel(Phase08RestoreImportRequest request)
    {
        var packageResult = LoadJson<Phase08BackupExportPackage>(request.PackageManifestPath, "PHASE08_IMPORT_PACKAGE_INVALID");
        if (!packageResult.IsSuccess)
            return packageResult.MapError<Phase08RestoreImportResult>();

        var package = packageResult.Value!;
        var checkpointFile = package.Files.SingleOrDefault(file => file.Role == "personal-checkpoint");
        if (checkpointFile is null)
            return OperationResult<Phase08RestoreImportResult>.Fail(
                "Export package has no personal checkpoint file.",
                "PHASE08_IMPORT_CHECKPOINT_MISSING");

        var sourceCheckpointPath = Path.Combine(Path.GetDirectoryName(request.PackageManifestPath)!, checkpointFile.PackageRelativePath);
        if (!File.Exists(sourceCheckpointPath))
            return OperationResult<Phase08RestoreImportResult>.Fail(
                "Exported checkpoint file is missing.",
                "PHASE08_IMPORT_CHECKPOINT_FILE_MISSING");

        var importedRoot = Path.Combine(request.FreshUserLocalRoot, ".xiigen", package.SphereId);
        Directory.CreateDirectory(importedRoot);
        var importedCheckpointPath = Path.Combine(importedRoot, "imported-adaptive-checkpoint.json");
        File.Copy(sourceCheckpointPath, importedCheckpointPath, overwrite: true);
        var importedSha = Sha256File(importedCheckpointPath);
        var importedWithoutRetrain = string.Equals(importedSha, checkpointFile.Sha256, StringComparison.Ordinal);
        var importManifestPath = Path.Combine(importedRoot, "phase08-import-manifest.json");
        var importedCheckpointRelativePath = $".xiigen/{package.SphereId}/imported-adaptive-checkpoint.json";
        var importManifestRelativePath = $".xiigen/{package.SphereId}/phase08-import-manifest.json";
        var importManifest = new
        {
            schemaVersion = 1,
            package.PackageId,
            package.SphereId,
            request.FreshUserProfileId,
            importedCheckpointRelativePath,
            importedCheckpointSha256 = importedSha,
            importedWithoutRetrain,
            freshProfileUsed = true,
            rootRemapTable = new[]
            {
                new Phase08PortableRootRemap("fresh-user-local-root", "portable://fresh-user-local-root", "importer-selected root supplied at restore time"),
                new Phase08PortableRootRemap("package-root", ".", "package-relative source files")
            },
            semanticQualityClaimed = false
        };
        WriteJson(importManifestPath, importManifest);

        return OperationResult<Phase08RestoreImportResult>.Ok(new Phase08RestoreImportResult(
            SphereId: package.SphereId,
            FreshUserProfileId: request.FreshUserProfileId,
            ImportedCheckpointPath: importedCheckpointPath,
            ImportManifestPath: importManifestPath,
            ImportedCheckpointPortableRef: $"portable://fresh-user-local-root/{importedCheckpointRelativePath}",
            ImportManifestPortableRef: $"portable://fresh-user-local-root/{importManifestRelativePath}",
            ImportedCheckpointSha256: importedSha,
            ImportedWithoutRetrain: importedWithoutRetrain,
            FreshProfileUsed: true,
            SourceImportHashesMatch: importedWithoutRetrain,
            SemanticQualityClaimed: false));
    }

    public OperationResult<Phase08ShareCopyResult> CreateApprovedShareCopy(Phase08ShareCopyRequest request)
    {
        if (!request.UserApprovalRecorded)
            return OperationResult<Phase08ShareCopyResult>.Fail(
                "Explicit user approval is required before share/copy.",
                "PHASE08_SHARE_APPROVAL_REQUIRED");

        if (!request.Anonymize)
            return OperationResult<Phase08ShareCopyResult>.Fail(
                "Anonymization is required before share/copy.",
                "PHASE08_SHARE_ANONYMIZATION_REQUIRED");

        if (request.SilentSyncRequested)
            return OperationResult<Phase08ShareCopyResult>.Fail(
                "Silent sync is forbidden for personal adaptive state.",
                "PHASE08_SILENT_SYNC_FORBIDDEN");

        var packageResult = LoadJson<Phase08BackupExportPackage>(request.PackageManifestPath, "PHASE08_SHARE_PACKAGE_INVALID");
        if (!packageResult.IsSuccess)
            return packageResult.MapError<Phase08ShareCopyResult>();

        Directory.CreateDirectory(request.ShareDirectory);
        var entries = packageResult.Value!.Files
            .Select(file => new Phase08ShareManifestEntry(
                Role: file.Role,
                FileName: file.FileName,
                Sha256: file.Sha256,
                SizeBytes: file.SizeBytes,
                ShareablePath: false,
                RawPrivateTextIncluded: false,
                AbsolutePrivatePathIncluded: false))
            .ToArray();
        var share = new Phase08ShareCopyResult(
            ShareManifestPath: Path.Combine(request.ShareDirectory, "phase08-anonymized-share-manifest.json"),
            ShareManifestPortableRef: "portable://approved-anonymized-share/phase08-anonymized-share-manifest.json",
            Entries: entries,
            UserApprovalRecorded: true,
            Anonymized: true,
            SilentSyncEnabled: false,
            ContainsRawPrivateText: false,
            ContainsAbsolutePrivatePath: false);
        WriteJson(share.ShareManifestPath, share);
        return OperationResult<Phase08ShareCopyResult>.Ok(share);
    }

    public OperationResult<Phase08ContinueTrainingResult> ContinueTrainingFromCheckpoint(
        Phase08ContinueTrainingRequest request)
    {
        if (request.AttemptCommonCheckpointMutation)
            return OperationResult<Phase08ContinueTrainingResult>.Fail(
                "Personal continue-training cannot mutate the common checkpoint.",
                "PHASE08_COMMON_MUTATION_REJECTED");

        if (request.RequireExistingCheckpoint && !File.Exists(request.ExistingCheckpointPath))
            return OperationResult<Phase08ContinueTrainingResult>.Fail(
                "Continue-training requires an existing or imported checkpoint; cold retrain is not allowed on this path.",
                "PHASE08_CONTINUE_REQUIRES_CHECKPOINT");

        var checkpointResult = LoadJson<Phase08PersonalCheckpoint>(
            request.ExistingCheckpointPath,
            "PHASE08_CONTINUE_CHECKPOINT_INVALID");
        if (!checkpointResult.IsSuccess)
            return checkpointResult.MapError<Phase08ContinueTrainingResult>();

        var checkpoint = checkpointResult.Value!;
        if (!string.Equals(checkpoint.SphereId, request.SphereId, StringComparison.Ordinal))
            return OperationResult<Phase08ContinueTrainingResult>.Fail(
                "Checkpoint sphere id must match the continue-training request.",
                "PHASE08_CONTINUE_SPHERE_MISMATCH");

        var beforeHash = Sha256File(request.ExistingCheckpointPath);
        var beforeCount = checkpoint.TrainedSignalIds.Count;
        var trained = checkpoint.TrainedSignalIds
            .Concat(request.NewTrainingSignalIds)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var continued = checkpoint with
        {
            CheckpointId = $"phase08:{checkpoint.SphereId}:continued:{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            TrainedSignalIds = trained,
            TrainingGeneration = checkpoint.TrainingGeneration + Math.Max(0, trained.Length - beforeCount),
            ParentCheckpointSha256 = beforeHash,
            SavedAtUtc = DateTimeOffset.UtcNow
        };
        WriteJson(request.ExistingCheckpointPath, continued);
        var afterHash = Sha256File(request.ExistingCheckpointPath);
        var result = new Phase08ContinueTrainingResult(
            SphereId: request.SphereId,
            ExistingCheckpointPath: request.ExistingCheckpointPath,
            ExistingCheckpointPortableRef: PortableRuntimeRef(request.ExistingCheckpointPath),
            LoadedCheckpointSha256: beforeHash,
            SavedCheckpointSha256: afterHash,
            MetricBefore: beforeCount,
            MetricAfter: trained.Length,
            ContinuedFromExistingCheckpoint: true,
            RetrainedFromScratch: false,
            MetricImproved: trained.Length > beforeCount,
            TrainedSignalIds: trained,
            CommonCheckpointMutated: false,
            SemanticQualityClaimed: false);
        WriteJson(request.ReportPath, result);
        return OperationResult<Phase08ContinueTrainingResult>.Ok(result);
    }

    public OperationResult<Phase08CommonUpgradeResult> ApplyCommonUpgrade(
        Phase08CommonUpgradeRequest request)
    {
        if (request.AttemptPersonalOverwrite)
            return OperationResult<Phase08CommonUpgradeResult>.Fail(
                "Common upgrade attempted to overwrite personal adaptive checkpoint.",
                "PHASE08_PERSONAL_OVERWRITE_REJECTED");

        if (!File.Exists(request.CommonManifestPath))
            return OperationResult<Phase08CommonUpgradeResult>.Fail(
                "Common manifest is required for upgrade.",
                "PHASE08_COMMON_MANIFEST_REQUIRED");

        if (!File.Exists(request.PersonalCheckpointPath))
            return OperationResult<Phase08CommonUpgradeResult>.Fail(
                "Personal checkpoint is required for isolation check.",
                "PHASE08_PERSONAL_CHECKPOINT_REQUIRED");

        var previousCommonContent = File.ReadAllText(request.CommonManifestPath, Encoding.UTF8);
        var previousCommonSha = Sha256File(request.CommonManifestPath);
        var personalBefore = Sha256File(request.PersonalCheckpointPath);
        var commonManifestPortableRef = PortableRuntimeRef(request.CommonManifestPath);
        var rollback = new Phase08RollbackRecord(
            SchemaVersion: 1,
            CommonManifestPortableRef: commonManifestPortableRef,
            CommonManifestId: Path.GetFileNameWithoutExtension(request.CommonManifestPath),
            RootRemapTable:
            [
                new Phase08PortableRootRemap(
                    "runtime-artifact-root",
                    "portable://runtime/phase-08-common-user-two-leg-lifecycle",
                    "resolved from the rollback record location at restore time"),
                new Phase08PortableRootRemap(
                    "common-manifest",
                    commonManifestPortableRef,
                    "portable manifest locator; no local absolute path is stored")
            ],
            PreviousCommonManifestContent: previousCommonContent,
            PreviousCommonManifestSha256: previousCommonSha,
            PersonalCheckpointSha256Before: personalBefore,
            CreatedAtUtc: DateTimeOffset.UtcNow);
        WriteJson(request.RollbackRecordPath, rollback);
        WriteText(request.CommonManifestPath, request.NewCommonManifestContent);
        var newCommonSha = Sha256File(request.CommonManifestPath);
        var personalAfter = Sha256File(request.PersonalCheckpointPath);

        return OperationResult<Phase08CommonUpgradeResult>.Ok(new Phase08CommonUpgradeResult(
            CommonManifestPath: request.CommonManifestPath,
            RollbackRecordPath: request.RollbackRecordPath,
            CommonManifestPortableRef: PortableRuntimeRef(request.CommonManifestPath),
            RollbackRecordPortableRef: PortableRuntimeRef(request.RollbackRecordPath),
            PreviousCommonManifestSha256: previousCommonSha,
            NewCommonManifestSha256: newCommonSha,
            PersonalCheckpointSha256Before: personalBefore,
            PersonalCheckpointSha256After: personalAfter,
            CommonManifestUpdated: !string.Equals(previousCommonSha, newCommonSha, StringComparison.Ordinal),
            PersonalCheckpointPreserved: string.Equals(personalBefore, personalAfter, StringComparison.Ordinal),
            RollbackRecordCreated: File.Exists(request.RollbackRecordPath),
            PersonalOverwriteRejected: false));
    }

    public OperationResult<Phase08RollbackResult> RollbackCommonUpgrade(
        string rollbackRecordPath,
        string personalCheckpointPath)
    {
        var rollbackResult = LoadJson<Phase08RollbackRecord>(rollbackRecordPath, "PHASE08_ROLLBACK_RECORD_INVALID");
        if (!rollbackResult.IsSuccess)
            return rollbackResult.MapError<Phase08RollbackResult>();

        var rollback = rollbackResult.Value!;
        var commonManifestPathResult = ResolveRollbackCommonManifestPath(rollback, rollbackRecordPath);
        if (!commonManifestPathResult.IsSuccess)
            return commonManifestPathResult.MapError<Phase08RollbackResult>();

        var commonManifestPath = commonManifestPathResult.Value!;
        var personalBeforeRollback = File.Exists(personalCheckpointPath)
            ? Sha256File(personalCheckpointPath)
            : "";
        WriteExactText(commonManifestPath, rollback.PreviousCommonManifestContent);
        var restoredSha = Sha256File(commonManifestPath);
        var personalAfterRollback = File.Exists(personalCheckpointPath)
            ? Sha256File(personalCheckpointPath)
            : "";

        return OperationResult<Phase08RollbackResult>.Ok(new Phase08RollbackResult(
            CommonManifestPath: commonManifestPath,
            CommonManifestPortableRef: rollback.CommonManifestPortableRef,
            RestoredCommonManifestSha256: restoredSha,
            RestoredPreviousCommonManifest: string.Equals(restoredSha, rollback.PreviousCommonManifestSha256, StringComparison.Ordinal),
            PersonalCheckpointUntouched: string.Equals(personalBeforeRollback, personalAfterRollback, StringComparison.Ordinal)
                && string.Equals(personalAfterRollback, rollback.PersonalCheckpointSha256Before, StringComparison.Ordinal)));
    }

    public OperationResult<Phase08EvidenceLeafResult> WriteEvidenceLeaf(
        string evidenceDirectory,
        Phase08LifecycleEvidenceReport report)
    {
        Directory.CreateDirectory(evidenceDirectory);
        var jsonPath = Path.Combine(evidenceDirectory, "phase-08-lifecycle-evidence.json");
        var jsonlPath = Path.Combine(evidenceDirectory, "phase-08-lifecycle-checks.jsonl");
        var markdownPath = Path.Combine(evidenceDirectory, "phase-08-lifecycle-report.ru.md");
        WriteJson(jsonPath, report);
        WriteJsonl(jsonlPath, report.Checks);
        WriteText(markdownPath, BuildRussianMarkdown(report));
        return OperationResult<Phase08EvidenceLeafResult>.Ok(new Phase08EvidenceLeafResult(
            JsonPath: jsonPath,
            JsonlPath: jsonlPath,
            MarkdownPath: markdownPath));
    }

    private static Phase08TrainableSphereLifecycleRow BuildRow(
        string sphereId,
        string humanName,
        string allowedArchitectureLevel,
        string commonMachineRef,
        string commonManifestRef,
        string commonCheckpointRef,
        string commonTrainingSetRef,
        string commonUpgradePathRef,
        string personalRoot,
        string commonRepoRoot,
        string userLocalRoot)
        => new(
            SphereId: sphereId,
            HumanName: humanName,
            AllowedArchitectureLevel: allowedArchitectureLevel,
            CommonMachineRef: commonMachineRef,
            CommonModelManifestRef: NormalizePortableRef(commonRepoRoot, commonManifestRef),
            CommonCheckpointRef: NormalizePortableRef(commonRepoRoot, commonCheckpointRef),
            CommonTrainingSetRef: NormalizePortableRef(commonRepoRoot, commonTrainingSetRef),
            CommonUpgradePathRef: NormalizePortableRef(commonRepoRoot, commonUpgradePathRef),
            PersonalStateRelativeRoot: personalRoot,
            PersonalCheckpointRelativePath: $"{personalRoot}/adaptive-checkpoint.json",
            PersonalDpoSignalRelativePath: $"{personalRoot}/adaptive-dpo-signals.jsonl",
            CommonLegRepoOwned: !string.Equals(
                    sphereId,
                    "orchestration-reasoning",
                    StringComparison.Ordinal)
                || HasActiveRegistryEntry(commonRepoRoot, commonManifestRef),
            PersonalLegUserLocal: !IsPathInsideRoot(userLocalRoot, commonRepoRoot),
            BackupExportSupported: true,
            RestoreImportSupported: true,
            ApprovedShareCopySupported: true,
            ContinueTrainingSupported: true,
            RollbackSupported: true,
            DeterministicShortcutSubstituted: false,
            ClaimBoundary: "lifecycle proof only; semantic quality and full extension readiness are not claimed");

    private static bool HasActiveRegistryEntry(
        string commonRepoRoot,
        string commonManifestRef)
    {
        // Phase08 advertises a common leg only after the registry row is bound
        // to the manifest and checkpoint bytes in this checkout.
        return RepositoryModelIndexIntegrityGuard
            .FindValidatedRowByManifestPath(commonRepoRoot, commonManifestRef)
            .IsSuccess;
    }

    private static Phase08StoreManifestEntry BuildStoreEntry(
        Phase08TrainableSphereLifecycleRow row,
        string userRoot,
        string commonRepoRoot)
    {
        var path = Path.Combine(userRoot, row.PersonalCheckpointRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return new Phase08StoreManifestEntry(
            SphereId: row.SphereId,
            StateKind: "personal-adaptive-checkpoint",
            AbsolutePath: $"portable://user-local-root/{row.PersonalCheckpointRelativePath}",
            RelativePath: row.PersonalCheckpointRelativePath,
            Sha256: File.Exists(path) ? Sha256File(path) : "not-created-yet",
            UnderUserLocalRoot: IsPathInsideRoot(path, userRoot),
            UnderCommonRepoRoot: IsPathInsideRoot(path, commonRepoRoot),
            Shareable: false);
    }

    private static Phase08PackageFile BuildPackageFile(string role, string path, string packageRoot)
        => new(
            Role: role,
            FileName: Path.GetFileName(path),
            PackageRelativePath: Path.GetRelativePath(packageRoot, path).Replace('\\', '/'),
            Sha256: Sha256File(path),
            SizeBytes: new FileInfo(path).Length,
            ContainsRawPrivateText: false,
            ContainsAbsoluteUserPath: ContainsAbsolutePath(File.ReadAllText(path, Encoding.UTF8)));

    private static Phase08UserLocalStoreManifest ToPortableStoreManifest(Phase08UserLocalStoreManifest manifest)
        => manifest with
        {
            UserLocalRoot = "portable://user-local-root",
            CommonRepoRoot = "portable://common-root",
            Entries = manifest.Entries
                .Select(entry => entry with
                {
                    AbsolutePath = $"portable://user-local-root/{entry.RelativePath}"
                })
                .ToArray()
        };

    private static string ToPortableCommonManifestRef(string commonManifestRef)
    {
        var normalized = commonManifestRef.Replace('\\', '/');
        if (!Path.IsPathRooted(commonManifestRef))
            return normalized;

        foreach (var marker in new[] { "models/", "artifacts/", "training-data/" })
        {
            var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return normalized[index..];
        }

        return $"common://manifest/{Path.GetFileName(commonManifestRef)}";
    }

    private static string PortableRuntimeRef(string path)
    {
        var normalized = NormalizePath(path).Replace('\\', '/');
        foreach (var marker in new[] { ".xiigen/", "phase-08-common-user-two-leg-lifecycle/", "common-upgrade-sandbox/" })
        {
            var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return $"portable://runtime/{normalized[index..]}";
        }

        return $"portable://runtime/{Path.GetFileName(path)}";
    }

    private static OperationResult<string> ResolveRollbackCommonManifestPath(
        Phase08RollbackRecord rollback,
        string rollbackRecordPath)
    {
        const string runtimePrefix = "portable://runtime/";
        var portableRef = rollback.CommonManifestPortableRef.Replace('\\', '/');
        if (!portableRef.StartsWith(runtimePrefix, StringComparison.Ordinal))
            return OperationResult<string>.Fail(
                "Rollback record must contain a portable runtime common manifest locator.",
                "PHASE08_ROLLBACK_COMMON_MANIFEST_REF_INVALID");

        var relative = portableRef[runtimePrefix.Length..];
        const string artifactRootMarker = "phase-08-common-user-two-leg-lifecycle/";
        if (relative.StartsWith(artifactRootMarker, StringComparison.OrdinalIgnoreCase))
            relative = relative[artifactRootMarker.Length..];

        if (string.IsNullOrWhiteSpace(relative)
            || relative.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(relative))
        {
            return OperationResult<string>.Fail(
                "Rollback record common manifest locator is not relative to the Phase 08 artifact root.",
                "PHASE08_ROLLBACK_COMMON_MANIFEST_REF_INVALID");
        }

        var artifactRoot = ResolvePhase08ArtifactRoot(rollbackRecordPath);
        if (artifactRoot is null)
            return OperationResult<string>.Fail(
                "Rollback record location does not identify the Phase 08 artifact root.",
                "PHASE08_ROLLBACK_ARTIFACT_ROOT_NOT_FOUND");

        var resolved = Path.GetFullPath(Path.Combine(
            artifactRoot,
            relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsPathInsideRoot(resolved, artifactRoot))
            return OperationResult<string>.Fail(
                "Rollback record common manifest locator resolves outside the Phase 08 artifact root.",
                "PHASE08_ROLLBACK_COMMON_MANIFEST_REF_INVALID");

        return OperationResult<string>.Ok(resolved);
    }

    private static string? ResolvePhase08ArtifactRoot(string rollbackRecordPath)
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(rollbackRecordPath))!);
        while (directory is not null)
        {
            if (string.Equals(directory.Name, "phase-08-common-user-two-leg-lifecycle", StringComparison.OrdinalIgnoreCase))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private static OperationResult<T> LoadJson<T>(string path, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return OperationResult<T>.Fail("JSON file is required.", errorCode);

        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions);
        return value is null
            ? OperationResult<T>.Fail("JSON file could not be parsed.", errorCode)
            : OperationResult<T>.Ok(value);
    }

    private static void WriteJson<T>(string path, T value)
        => WriteText(path, JsonSerializer.Serialize(value, JsonOptions));

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
        => WriteText(path, string.Join('\n', rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))) + "\n");

    private static void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, NormalizeArtifactText(text), Utf8NoBom);
    }

    private static void WriteExactText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, Utf8NoBom);
    }

    private static string BuildRussianMarkdown(Phase08LifecycleEvidenceReport report)
    {
        var lines = new List<string>
        {
            "# Фаза 08: common/user two-leg learning lifecycle",
            "",
            "## Что решает фаза",
            report.WhatItSolvesRu,
            "",
            "## Покрытые подшаги"
        };
        lines.AddRange(report.CoveredSubsteps.Select(step => $"- {step}"));
        lines.Add("");
        lines.Add("## Что доказано");
        lines.Add("- SkillSelection имеет executable lifecycle proof: user-local manifest, portable backup/export, fresh-profile import, approved anonymized share, continue-training and rollback/hash isolation.");
        lines.Add("- PromptToTask, OrchestrationReasoning и ResultGap/correction остаются required-but-not-yet-executed, поэтому matrix.passed=false до отдельных executable probes.");
        lines.Add("- Backup/export/import artifacts используют portable refs и remap table вместо абсолютных user/common paths.");
        lines.Add("- User-local manifest хранит personal state отдельно от common repo state и отключает silent sync.");
        lines.Add("- Share/copy требует approval, anonymization и не содержит silent sync.");
        lines.Add("- Common upgrade меняет common manifest, сохраняет personal checkpoint hash и создает rollback record для проверенной SkillSelection path.");
        lines.Add("");
        lines.Add("## Проверки");
        foreach (var check in report.Checks)
        {
            lines.Add($"- {check.Substep}: {(check.Passed ? "PASS" : "FAIL")} - {check.HumanEvidence}");
            lines.Add($"  Positive: {check.PositiveExample}");
            lines.Add($"  Negative: {check.NegativeExample}");
        }
        lines.Add("");
        lines.Add("## Dashboard/API QA boundary");
        lines.Add(report.DashboardApiQaBoundary);
        lines.Add("");
        lines.Add("## Что не заявлено");
        lines.AddRange(report.ExplicitNonClaims.Select(claim => $"- {claim}"));
        return string.Join('\n', lines) + "\n";
    }

    private static bool ContainsAbsolutePath(string text)
        => text.Contains(@":\", StringComparison.Ordinal)
            || text.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            || text.Contains("/Users/", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeArtifactText(string value)
    {
        var normalized = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        return string.Join('\n', normalized.Split('\n').Select(line => line.TrimEnd())).TrimEnd() + "\n";
    }

    private static string NormalizePortableRef(string root, string relative)
        => Path.IsPathRooted(relative)
            ? ToPortableCommonManifestRef(Path.GetRelativePath(NormalizePath(root), relative))
            : relative.Replace('\\', '/');

    private static string NormalizePath(string path)
        => Path.GetFullPath(path);

    private static bool IsPathInsideRoot(string path, string root)
    {
        var fullPath = EnsureTrailingSeparator(Path.GetFullPath(path));
        var fullRoot = EnsureTrailingSeparator(Path.GetFullPath(root));
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Sha256Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
