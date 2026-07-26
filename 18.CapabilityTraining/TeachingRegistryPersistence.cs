// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Text.Json;
using AgentInfrastructure;
using LoRA;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// This class persists and restores cumulative teaching registry state between rows.
public static class TeachingRegistryPersistence
{
    public const string GeneratedCapabilityManifestFileName = "generated_capability_manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Capture registry descriptors and generated capability manifests into one snapshot.
    public static OperationResult<TeachingRegistrySnapshot> Capture(
        string bundleId,
        int rowIndex,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        string checkpointRoot,
        string? snapshotId = null)
    {
        if (units is null)
            return OperationResult<TeachingRegistrySnapshot>.Fail("Unit registry is required.", "UNIT_REGISTRY_REQUIRED");
        if (flows is null)
            return OperationResult<TeachingRegistrySnapshot>.Fail("Flow registry is required.", "FLOW_REGISTRY_REQUIRED");
        if (string.IsNullOrWhiteSpace(bundleId))
            return OperationResult<TeachingRegistrySnapshot>.Fail("Bundle id is required.", "BUNDLE_ID_REQUIRED");

        try
        {
            var manifests = LoadGeneratedCapabilityManifests(checkpointRoot);
            var checkpointPaths = manifests
                .SelectMany(manifest => new[] { manifest.PolicyCheckpointPath, manifest.TokenizerPath })
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            var snapshot = new TeachingRegistrySnapshot(
                string.IsNullOrWhiteSpace(snapshotId) ? $"teaching-snapshot-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}" : snapshotId,
                bundleId.Trim(),
                rowIndex,
                DateTimeOffset.UtcNow,
                units.ListDescriptors().Select(ToSnapshotEntry).ToArray(),
                flows.ListDescriptors().Select(ToSnapshotEntry).ToArray(),
                manifests,
                checkpointPaths);

            return OperationResult<TeachingRegistrySnapshot>.Ok(snapshot);
        }
        catch (Exception ex)
        {
            return OperationResult<TeachingRegistrySnapshot>.Fail(ex.Message, "SNAPSHOT_CAPTURE_FAILED");
        }
    }

    // Save one generated capability manifest beside the checkpoint files that rebuild it.
    public static OperationResult<string> SaveGeneratedCapabilityManifest(
        GeneratedCapabilityCheckpointManifest manifest,
        string manifestPath)
    {
        if (manifest is null)
            return OperationResult<string>.Fail("Generated capability manifest is required.", "MANIFEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<string>.Fail("Manifest path is required.", "MANIFEST_PATH_REQUIRED");

        try
        {
            var directory = Path.GetDirectoryName(manifestPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions));
            return OperationResult<string>.Ok(manifestPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "MANIFEST_WRITE_FAILED");
        }
    }

    // Save the cumulative teaching snapshot after a row completes.
    public static OperationResult<string> Save(TeachingRegistrySnapshot snapshot, string snapshotPath)
    {
        if (snapshot is null)
            return OperationResult<string>.Fail("Snapshot is required.", "SNAPSHOT_REQUIRED");
        if (string.IsNullOrWhiteSpace(snapshotPath))
            return OperationResult<string>.Fail("Snapshot path is required.", "SNAPSHOT_PATH_REQUIRED");

        try
        {
            var directory = Path.GetDirectoryName(snapshotPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(snapshotPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            return OperationResult<string>.Ok(snapshotPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail(ex.Message, "SNAPSHOT_WRITE_FAILED");
        }
    }

    // Load the cumulative teaching snapshot. Null means row one has no prior state.
    public static OperationResult<TeachingRegistrySnapshot?> Load(string snapshotPath)
    {
        if (string.IsNullOrWhiteSpace(snapshotPath))
            return OperationResult<TeachingRegistrySnapshot?>.Fail("Snapshot path is required.", "SNAPSHOT_PATH_REQUIRED");
        if (!File.Exists(snapshotPath))
            return OperationResult<TeachingRegistrySnapshot?>.Ok(null);

        try
        {
            var json = File.ReadAllText(snapshotPath);
            var snapshot = JsonSerializer.Deserialize<TeachingRegistrySnapshot>(json, JsonOptions);
            return OperationResult<TeachingRegistrySnapshot?>.Ok(snapshot);
        }
        catch (Exception ex)
        {
            return OperationResult<TeachingRegistrySnapshot?>.Fail(ex.Message, "SNAPSHOT_READ_FAILED");
        }
    }

    // Load generated learned units and generated capability flows into registries without re-registering them.
    public static OperationResult<int> LoadIntoRegistries(
        TeachingRegistrySnapshot snapshot,
        ILogicalTopologyRegistry topology,
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows)
    {
        if (snapshot is null)
            return OperationResult<int>.Fail("Snapshot is required.", "SNAPSHOT_REQUIRED");
        if (topology is null)
            return OperationResult<int>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED");
        if (units is null)
            return OperationResult<int>.Fail("Unit registry is required.", "UNIT_REGISTRY_REQUIRED");
        if (flows is null)
            return OperationResult<int>.Fail("Flow registry is required.", "FLOW_REGISTRY_REQUIRED");

        var unitEntries = snapshot.Units.ToDictionary(entry => entry.UnitId, StringComparer.Ordinal);
        var loadedUnits = new List<LogicalUnitRegistryLoadedEntry>();
        var loadedFlows = new List<LogicalFlowRegistryLoadedEntry>();

        foreach (var manifest in snapshot.GeneratedCapabilities
            .Where(manifest => unitEntries.ContainsKey(manifest.UnitId))
            .OrderBy(manifest => manifest.CreatedAtUtc))
        {
            var unitEntry = unitEntries[manifest.UnitId];

            var unitTrustState = ParseTrustState(unitEntry.TrustState);
            if (!unitTrustState.IsSuccess)
                return OperationResult<int>.Fail(
                    "Teaching unit trust state is invalid.",
                    "TEACHING_UNIT_TRUST_STATE_INVALID");

            var manifestTrustState = ParseTrustState(manifest.TrustState);
            if (!manifestTrustState.IsSuccess)
                return OperationResult<int>.Fail(
                    "Generated capability trust state is invalid.",
                    "GENERATED_CAPABILITY_TRUST_STATE_INVALID");

            if (unitTrustState.Value != manifestTrustState.Value)
                return OperationResult<int>.Fail(
                    "Generated capability trust state does not match its teaching unit.",
                    "GENERATED_CAPABILITY_TRUST_STATE_MISMATCH");

            var unit = LoadGeneratedCapabilityUnit(manifest, snapshot.GeneratedCapabilities);
            if (!unit.IsSuccess)
                return OperationResult<int>.Fail(unit.ErrorMessage, unit.ErrorCode);

            var version = string.IsNullOrWhiteSpace(unitEntry.Version) ? manifest.Version : unitEntry.Version;
            var registrationOrder = unitEntry.RegistrationOrder;
            loadedUnits.Add(new LogicalUnitRegistryLoadedEntry(
                new LogicalUnitDescriptor(
                manifest.UnitId,
                manifest.UnitRole,
                manifest.DomainTag,
                version,
                unitTrustState.Value,
                true,
                typeof(string),
                typeof(string),
                registrationOrder),
                unit.Value!));
        }

        foreach (var flowEntry in snapshot.Flows
            .Where(entry => entry.FlowKind == TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow)
            .OrderBy(entry => entry.RegistrationOrder))
        {
            var subPhase = topology.ResolveSubPhase(flowEntry.SubPhaseId);
            if (!subPhase.IsSuccess)
                return OperationResult<int>.Fail(subPhase.ErrorMessage, subPhase.ErrorCode);

            var manifest = snapshot.GeneratedCapabilities
                .LastOrDefault(candidate => string.Equals(candidate.SubPhaseId, flowEntry.SubPhaseId, StringComparison.Ordinal));
            var predicateIds = manifest is not null && manifest.PredicateIds.Length > 0
                ? manifest.PredicateIds
                : subPhase.Value!.SubPhase.PredicateIds;

            loadedFlows.Add(new LogicalFlowRegistryLoadedEntry(
                new LogicalFlowDescriptor(
                    flowEntry.FlowId,
                    flowEntry.ScenarioId,
                    flowEntry.Version,
                    typeof(string),
                    flowEntry.PhaseId,
                    flowEntry.SubPhaseId,
                    flowEntry.TaskType,
                    flowEntry.DomainTags,
                    flowEntry.RegistrationOrder),
                new GeneratedCapabilityFlowFactory(
                    flowEntry.PhaseId,
                    flowEntry.SubPhaseId,
                    flowEntry.TaskType,
                    flowEntry.DomainTags,
                    predicateIds,
                    manifest?.DomainTag)));
        }

        var unitLoad = units.LoadEntries(loadedUnits, replaceExisting: false);
        if (!unitLoad.IsSuccess)
            return OperationResult<int>.Fail(unitLoad.ErrorMessage, unitLoad.ErrorCode);

        var flowLoad = flows.LoadEntries(loadedFlows, replaceExisting: false);
        if (!flowLoad.IsSuccess)
            return OperationResult<int>.Fail(flowLoad.ErrorMessage, flowLoad.ErrorCode);

        return OperationResult<int>.Ok(unitLoad.Value + flowLoad.Value);
    }

    private static TeachingUnitRegistryEntry ToSnapshotEntry(LogicalUnitDescriptor descriptor)
    {
        return new TeachingUnitRegistryEntry(
            descriptor.UnitId,
            descriptor.UnitRole,
            descriptor.DomainTag,
            descriptor.Version,
            descriptor.TrustState.ToString(),
            descriptor.IsLearned,
            descriptor.InputType.AssemblyQualifiedName ?? descriptor.InputType.FullName ?? descriptor.InputType.Name,
            descriptor.OutputType.AssemblyQualifiedName ?? descriptor.OutputType.FullName ?? descriptor.OutputType.Name,
            descriptor.RegistrationOrder,
            DetectUnitKind(descriptor));
    }

    private static TeachingFlowRegistryEntry ToSnapshotEntry(LogicalFlowDescriptor descriptor)
    {
        return new TeachingFlowRegistryEntry(
            descriptor.FlowId,
            descriptor.ScenarioId,
            descriptor.Version,
            descriptor.InputType.AssemblyQualifiedName ?? descriptor.InputType.FullName ?? descriptor.InputType.Name,
            descriptor.PhaseId,
            descriptor.SubPhaseId,
            descriptor.TaskType,
            descriptor.DomainTags,
            descriptor.RegistrationOrder,
            DetectFlowKind(descriptor));
    }

    private static GeneratedCapabilityCheckpointManifest[] LoadGeneratedCapabilityManifests(string checkpointRoot)
    {
        if (string.IsNullOrWhiteSpace(checkpointRoot) || !Directory.Exists(checkpointRoot))
            return [];

        var manifests = new List<GeneratedCapabilityCheckpointManifest>();
        foreach (var manifestPath in Directory.GetFiles(checkpointRoot, GeneratedCapabilityManifestFileName, SearchOption.AllDirectories))
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(json, JsonOptions);
            if (manifest is not null)
                manifests.Add(manifest);
        }

        return manifests
            .OrderBy(manifest => manifest.CreatedAtUtc)
            .ThenBy(manifest => manifest.UnitId, StringComparer.Ordinal)
            .ToArray();
    }

    private static OperationResult<TransformerEmbeddingKnnLogicalUnit<string>> LoadGeneratedCapabilityUnit(
        GeneratedCapabilityCheckpointManifest manifest,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> allManifests)
    {
        if (!File.Exists(manifest.PolicyCheckpointPath))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                $"Policy checkpoint not found: {manifest.PolicyCheckpointPath}",
                "POLICY_CHECKPOINT_NOT_FOUND");
        if (!File.Exists(manifest.TokenizerPath))
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(
                $"Tokenizer checkpoint not found: {manifest.TokenizerPath}",
                "TOKENIZER_CHECKPOINT_NOT_FOUND");

        try
        {
            var tokenizer = BpeTokenizer.Load(manifest.TokenizerPath);
            var model = new LlmModel(new ModelConfig
            {
                VocabSize = manifest.VocabSize,
                ContextLen = manifest.ContextLen,
                HiddenDim = manifest.HiddenDim,
                NumLayers = manifest.NumLayers,
                NumHeads = manifest.NumHeads,
                NumKvHeads = manifest.NumKvHeads,
                FfnHiddenDim = manifest.FfnHiddenDim
            });

            LoraApplicator.Apply(model, new LoraConfig
            {
                Rank = manifest.LoraRank,
                Alpha = manifest.LoraAlpha,
                Dropout = manifest.LoraDropout,
                TargetModules = manifest.LoraTargetModules
            });
            model.LoadCheckpoint(manifest.PolicyCheckpointPath);

            var examples = allManifests
                .Where(candidate =>
                    string.Equals(candidate.DomainTag, manifest.DomainTag, StringComparison.Ordinal) &&
                    candidate.CreatedAtUtc <= manifest.CreatedAtUtc)
                .OrderBy(candidate => candidate.CreatedAtUtc)
                .ThenBy(candidate => candidate.UnitId, StringComparer.Ordinal)
                .Select(candidate => new TransformerKnnTrainingExample<string>(
                    candidate.ExampleId,
                    candidate.TrainingText,
                    candidate.OutputText,
                    candidate.PredicateIds,
                    candidate.DomainTags))
                .ToArray();

            return TransformerEmbeddingKnnLogicalUnit<string>.Train(
                manifest.UnitId,
                manifest.UnitRole,
                manifest.DomainTag,
                model,
                tokenizer,
                examples.Length == 0
                    ? [
                        new TransformerKnnTrainingExample<string>(
                            manifest.ExampleId,
                            manifest.TrainingText,
                            manifest.OutputText,
                            manifest.PredicateIds,
                            manifest.DomainTags)
                    ]
                    : examples);
        }
        catch (Exception ex)
        {
            return OperationResult<TransformerEmbeddingKnnLogicalUnit<string>>.Fail(ex.Message, "GENERATED_CAPABILITY_LOAD_FAILED");
        }
    }

    private static OperationResult<LogicalUnitTrustState> ParseTrustState(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return OperationResult<LogicalUnitTrustState>.Fail(
                "Trust state is invalid.",
                "TRUST_STATE_INVALID");
        }

        foreach (var name in Enum.GetNames<LogicalUnitTrustState>())
        {
            if (string.Equals(value, name, StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<LogicalUnitTrustState>(name, out var state) &&
                Enum.IsDefined(state))
            {
                return OperationResult<LogicalUnitTrustState>.Ok(state);
            }
        }

        return OperationResult<LogicalUnitTrustState>.Fail(
            "Trust state is invalid.",
            "TRUST_STATE_INVALID");
    }

    private static string DetectUnitKind(LogicalUnitDescriptor descriptor)
    {
        return descriptor.IsLearned &&
               string.Equals(descriptor.UnitRole, DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, StringComparison.Ordinal)
            ? TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit
            : TeachingRegistrySnapshotKinds.DeterministicUnit;
    }

    private static string DetectFlowKind(LogicalFlowDescriptor descriptor)
    {
        if (descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal))
            return TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow;
        if (descriptor.FlowId.StartsWith(UniversalNineNodeDevelopmentFlow.FlowIdPrefix, StringComparison.Ordinal))
            return TeachingRegistrySnapshotKinds.UniversalNineNodeFlow;

        return TeachingRegistrySnapshotKinds.RegisteredFlow;
    }
}
