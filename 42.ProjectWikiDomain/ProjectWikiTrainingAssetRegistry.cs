// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed record ProjectWikiTrainingAssetRegistry(
    [property: JsonPropertyName("schema_version")] string SchemaVersion,
    [property: JsonPropertyName("generated_at_utc")] string GeneratedAtUtc,
    [property: JsonPropertyName("source_root")] string SourceRoot,
    [property: JsonPropertyName("training_continuation_blocked_until_registry_validated")] bool TrainingContinuationBlockedUntilRegistryValidated,
    [property: JsonPropertyName("status_counts")] Dictionary<string, int> StatusCounts,
    [property: JsonPropertyName("entries")] ProjectWikiTrainingAssetEntry[] Entries)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static OperationResult<ProjectWikiTrainingAssetRegistry> LoadFromDataRoot(string learningDataRoot)
    {
        if (string.IsNullOrWhiteSpace(learningDataRoot))
            return OperationResult<ProjectWikiTrainingAssetRegistry>.Fail("Learning data root is required.", "LEARNING_DATA_ROOT_REQUIRED");

        var registryPath = Path.Combine(learningDataRoot, "registry", "PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json");
        if (!File.Exists(registryPath))
            return OperationResult<ProjectWikiTrainingAssetRegistry>.Fail($"Project wiki training asset registry not found: {registryPath}.", "PROJECT_WIKI_REGISTRY_NOT_FOUND");

        try
        {
            var json = File.ReadAllText(registryPath);
            var registry = JsonSerializer.Deserialize<ProjectWikiTrainingAssetRegistry>(json, JsonOptions);
            return registry is null
                ? OperationResult<ProjectWikiTrainingAssetRegistry>.Fail("Project wiki training asset registry is empty.", "PROJECT_WIKI_REGISTRY_EMPTY")
                : OperationResult<ProjectWikiTrainingAssetRegistry>.Ok(registry);
        }
        catch (Exception ex)
        {
            return OperationResult<ProjectWikiTrainingAssetRegistry>.Fail($"Failed to load project wiki training asset registry: {ex.Message}", "PROJECT_WIKI_REGISTRY_LOAD_FAILED");
        }
    }

    public OperationResult<bool> ValidateTrainingMayContinue()
    {
        if (TrainingContinuationBlockedUntilRegistryValidated)
            return OperationResult<bool>.Fail("Project wiki training is blocked until the learning asset registry is validated.", "PROJECT_WIKI_TRAINING_BLOCKED_UNTIL_REGISTRY_VALIDATED");

        var invalidStatus = Entries.FirstOrDefault(entry =>
            !string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            !string.Equals(entry.Status, "rejected", StringComparison.Ordinal) &&
            !string.Equals(entry.Status, "experimental_no_authority", StringComparison.Ordinal) &&
            !string.Equals(entry.Status, "blocked", StringComparison.Ordinal));
        if (invalidStatus is not null)
            return OperationResult<bool>.Fail($"Registry entry has invalid status: {invalidStatus.BatchId}.", "PROJECT_WIKI_REGISTRY_INVALID_STATUS");

        var rejectedContinuation = RejectedContinuationCandidates().FirstOrDefault();
        if (rejectedContinuation is not null)
            return OperationResult<bool>.Fail($"Rejected checkpoint cannot be used as continuation source: {rejectedContinuation.BatchId}.", "REJECTED_CHECKPOINT_CONTINUATION_SOURCE");

        var acceptedMissingReport = Entries.FirstOrDefault(entry =>
            string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(entry.RunnerReportPath));
        if (acceptedMissingReport is not null)
            return OperationResult<bool>.Fail($"Accepted registry entry has no runner report: {acceptedMissingReport.BatchId}.", "ACCEPTED_RUNNER_REPORT_MISSING");

        var acceptedCheckpointWithoutReference = Entries.FirstOrDefault(entry =>
            string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            entry.CheckpointAllowedAsContinuation &&
            string.IsNullOrWhiteSpace(entry.CheckpointRef));
        if (acceptedCheckpointWithoutReference is not null)
            return OperationResult<bool>.Fail($"Accepted continuation entry has no checkpoint reference: {acceptedCheckpointWithoutReference.BatchId}.", "ACCEPTED_CHECKPOINT_REFERENCE_MISSING");

        var acceptedUnknownLocator = Entries.FirstOrDefault(entry =>
            string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            string.Equals(entry.LocatorStatus, "UNKNOWN", StringComparison.Ordinal));
        if (acceptedUnknownLocator is not null)
            return OperationResult<bool>.Fail($"Accepted registry entry has unknown locator status: {acceptedUnknownLocator.BatchId}.", "ACCEPTED_LOCATOR_STATUS_UNKNOWN");

        var publishableMissingLocatorKey = Entries.FirstOrDefault(entry =>
            string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            !string.Equals(entry.ImportExportStatus, "not_publishable_baseline_only", StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(entry.LocatorKey));
        if (publishableMissingLocatorKey is not null)
            return OperationResult<bool>.Fail($"Accepted publishable registry entry has no locator key: {publishableMissingLocatorKey.BatchId}.", "ACCEPTED_LOCATOR_KEY_MISSING");

        var publishableMissingRemoteUnit = Entries.FirstOrDefault(entry =>
            string.Equals(entry.Status, "accepted", StringComparison.Ordinal) &&
            !string.Equals(entry.ImportExportStatus, "not_publishable_baseline_only", StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(entry.RemoteUnitId));
        if (publishableMissingRemoteUnit is not null)
            return OperationResult<bool>.Fail($"Accepted publishable registry entry has no remote unit id: {publishableMissingRemoteUnit.BatchId}.", "ACCEPTED_REMOTE_UNIT_ID_MISSING");

        return OperationResult<bool>.Ok(true);
    }

    public ProjectWikiTrainingAssetEntry[] EntriesForUnit(string unitName)
        => Entries
            .Where(entry => string.Equals(entry.UnitName, unitName, StringComparison.Ordinal))
            .ToArray();

    public ProjectWikiTrainingAssetEntry[] RejectedContinuationCandidates()
        => Entries
            .Where(entry => string.Equals(entry.Status, "rejected", StringComparison.Ordinal) && entry.CheckpointAllowedAsContinuation)
            .ToArray();
}

public sealed record ProjectWikiTrainingAssetEntry(
    [property: JsonPropertyName("batch_id")] string BatchId,
    [property: JsonPropertyName("unit_name")] string UnitName,
    [property: JsonPropertyName("unit_version")] string UnitVersion,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("status_reason")] string StatusReason,
    [property: JsonPropertyName("train_rows")] JsonElement TrainRows,
    [property: JsonPropertyName("heldout_rows")] JsonElement HeldOutRows,
    [property: JsonPropertyName("train_path")] string TrainPath,
    [property: JsonPropertyName("heldout_path")] string HeldOutPath,
    [property: JsonPropertyName("runner_report_path")] string RunnerReportPath,
    [property: JsonPropertyName("metrics")] ProjectWikiTrainingAssetMetrics Metrics,
    [property: JsonPropertyName("checkpoint_ref")] string CheckpointRef,
    [property: JsonPropertyName("manifest_ref")] string ManifestRef,
    [property: JsonPropertyName("locator_key")] string LocatorKey,
    [property: JsonPropertyName("remote_unit_id")] string RemoteUnitId,
    [property: JsonPropertyName("checkpoint_allowed_as_continuation")] bool CheckpointAllowedAsContinuation,
    [property: JsonPropertyName("allowed_next_action")] string AllowedNextAction,
    [property: JsonPropertyName("source_authority")] string SourceAuthority,
    [property: JsonPropertyName("import_export_status")] string ImportExportStatus,
    [property: JsonPropertyName("locator_status")] string LocatorStatus,
    [property: JsonPropertyName("artifact_integrity")] string ArtifactIntegrity,
    [property: JsonPropertyName("artifact_checksum")] string ArtifactChecksum,
    [property: JsonPropertyName("lfs_oid")] string LfsOid,
    [property: JsonPropertyName("source_run_id")] string SourceRunId,
    [property: JsonPropertyName("discovered_from")] string DiscoveredFrom,
    [property: JsonPropertyName("review_status")] string ReviewStatus,
    [property: JsonPropertyName("warnings")] string[] Warnings);

public sealed record ProjectWikiTrainingAssetMetrics(
    [property: JsonPropertyName("accuracy")] JsonElement Accuracy,
    [property: JsonPropertyName("reward_margin")] JsonElement RewardMargin,
    [property: JsonPropertyName("loss")] JsonElement Loss,
    [property: JsonPropertyName("cpu_ms")] JsonElement CpuMs,
    [property: JsonPropertyName("ram_mb")] JsonElement RamMb,
    [property: JsonPropertyName("elapsed_ms")] JsonElement ElapsedMs);

public sealed record ProjectWikiNextTrainingAction(
    string TargetUnitName,
    string Reason,
    string[] EvidenceBatchIds,
    bool StopBlindTraining);

public sealed record ProjectWikiTrainingLoopPlanResult(
    string LearningDataRoot,
    int RegistryEntryCount,
    Dictionary<string, int> StatusCounts,
    ProjectWikiNextTrainingAction NextTrainingAction,
    string RecommendedBatchStrategy,
    bool NewTrainingBatchAllowed,
    string[] Warnings);
