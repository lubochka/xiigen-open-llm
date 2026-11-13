// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionLearnedWeightFeatureAuthorityTests
{
    private const string SourceHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private static readonly string[] AllowedFeatureKeys =
    [
        "correction_category:contrast",
        "correction_category:generic-visual-defect",
        "correction_category:overflow",
        "correction_category:spacing",
        "correction_category:typography",
        "internal_vector:dimension",
        "internal_vector:mean",
        "source_kind:canva",
        "source_kind:figma",
        "source_kind:other"
    ];

    private static readonly string[] UnknownFeatureAliases =
    [
        "verdict_passed",
        "source_kind:accepted_v2",
        "correction_category:passed_v2"
    ];

    [Fact]
    public async Task TrainBatch_UnknownFeatureAliasesCannotMintCheckpointWeightsOrGradientSignals()
    {
        var bridge = new RecordingGradientBridge(request =>
        {
            var allowedControl = Assert.Single(
                request.SanitizedSignals,
                signal => string.Equals(
                    signal.AllowedFeatureKey,
                    "source_kind:figma",
                    StringComparison.Ordinal));
            return ValidBridgeResult(request,
            [
                new DesignConversionMachineGradientBridgeUpdate(
                    allowedControl.CanonicalUnitId,
                    allowedControl.AllowedFeatureKey,
                    allowedControl.SignalValue)
            ]);
        });
        var machine = new DesignConversionMachine(new DesignConversionRoutingFlow(), bridge);

        var train = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            machine,
            BuildUnknownAliasBatch());

        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.Equal(1, bridge.CallCount);
        var request = Assert.IsType<DesignConversionMachineGradientBridgeRequest>(bridge.LastRequest);
        Assert.Contains(
            request.SanitizedSignals,
            signal => string.Equals(signal.AllowedFeatureKey, "source_kind:figma", StringComparison.Ordinal));
        Assert.DoesNotContain(
            request.SanitizedSignals,
            signal => UnknownFeatureAliases.Contains(signal.AllowedFeatureKey, StringComparer.Ordinal));

        var checkpoint = Export(machine);
        var persistedKeys = AllFeatureKeys(checkpoint);
        Assert.Equal(["source_kind:figma"], persistedKeys);
        Assert.DoesNotContain(persistedKeys, key => UnknownFeatureAliases.Contains(key, StringComparer.Ordinal));

        var localMachine = new DesignConversionMachine();
        var localTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            localMachine,
            BuildUnknownAliasBatch());

        Assert.True(localTrain.IsSuccess, localTrain.ErrorMessage);
        var localPersistedKeys = AllFeatureKeys(Export(localMachine));
        Assert.Equal(["source_kind:figma"], localPersistedKeys);
        Assert.DoesNotContain(
            localPersistedKeys,
            key => UnknownFeatureAliases.Contains(key, StringComparer.Ordinal));
        Assert.Equal(["source_kind:figma"], AccumulatedFeatureKeys(localMachine));
    }

    [Theory]
    [InlineData("verdict_passed")]
    [InlineData("source_kind:accepted_v2")]
    [InlineData("correction_category:passed_v2")]
    public async Task ImportCheckpoint_UnknownFeatureKeyFailsAtomicallyWithoutRawKeyLeak(
        string unknownFeatureKey)
    {
        var root = CreateTempRoot("feature-authority-import");
        var checkpointPath = Path.Combine(root, "bound-checkpoint.json");
        try
        {
            var machine = await CreateMachineWithAllowedControlAsync();
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var before = await SnapshotBoundStateAsync(machine);
            var malformed = AddUnknownLearnedWeight(Export(machine), unknownFeatureKey);

            var import = machine.ImportCheckpoint(malformed);

            Assert.False(import.IsSuccess);
            Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
            Assert.DoesNotContain(unknownFeatureKey, import.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(before, await SnapshotBoundStateAsync(machine));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [InlineData("verdict_passed")]
    [InlineData("source_kind:accepted_v2")]
    [InlineData("correction_category:passed_v2")]
    public async Task LoadCheckpoint_UnknownFeatureKeyFailsWithoutReplacingCurrentState(
        string unknownFeatureKey)
    {
        var root = CreateTempRoot("feature-authority-load");
        var currentPath = Path.Combine(root, "current-checkpoint.json");
        var malformedPath = Path.Combine(root, "malformed-checkpoint.json");
        try
        {
            var current = await CreateMachineWithAllowedControlAsync();
            var currentSave = await current.SaveCheckpointAsync(currentPath);
            Assert.True(currentSave.IsSuccess, currentSave.ErrorMessage);
            var before = await SnapshotBoundStateAsync(current);
            var malformed = AddUnknownLearnedWeight(Export(current), unknownFeatureKey);
            await File.WriteAllTextAsync(
                malformedPath,
                JsonSerializer.Serialize(malformed, DesignConversionCheckpointJsonOptions.Standard));

            var load = await DesignConversionMachine.LoadCheckpointAsync(malformedPath);

            Assert.False(load.IsSuccess);
            Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", load.ErrorCode);
            Assert.DoesNotContain(unknownFeatureKey, load.ErrorMessage, StringComparison.Ordinal);
            Assert.Equal(before, await SnapshotBoundStateAsync(current));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task CheckpointRoundTrip_AllCurrentAllowlistedFeatureKeysRemainAccepted()
    {
        var root = CreateTempRoot("feature-authority-roundtrip");
        var checkpointPath = Path.Combine(root, "checkpoint.json");
        try
        {
            var bridge = new RecordingGradientBridge(request => ValidBridgeResult(
                request,
                request.SanitizedSignals
                    .Select(signal => new DesignConversionMachineGradientBridgeUpdate(
                        signal.CanonicalUnitId,
                        signal.AllowedFeatureKey,
                        signal.SignalValue))
                    .ToImmutableArray()));
            var machine = new DesignConversionMachine(new DesignConversionRoutingFlow(), bridge);

            var train = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
                machine,
                BuildAllowedBridgeControlBatch());

            Assert.True(train.IsSuccess, train.ErrorMessage);
            Assert.Equal(1, bridge.CallCount);
            var request = Assert.IsType<DesignConversionMachineGradientBridgeRequest>(bridge.LastRequest);
            Assert.Contains(
                request.SanitizedSignals,
                signal => string.Equals(signal.AllowedFeatureKey, "source_kind:figma", StringComparison.Ordinal));
            Assert.Contains("source_kind:figma", AllFeatureKeys(Export(machine)));

            var allAllowedCheckpoint = WithAllAllowedLearnedWeights(Export(machine));
            var import = machine.ImportCheckpoint(allAllowedCheckpoint);
            Assert.True(import.IsSuccess, import.ErrorMessage);
            Assert.Equal(AllowedFeatureKeys, AllFeatureKeys(Export(machine)));

            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.Equal(AllowedFeatureKeys, AllFeatureKeys(load.Value!.Checkpoint));
            Assert.Equal(AllowedFeatureKeys, AllFeatureKeys(Export(load.Value.Machine)));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildUnknownAliasBatch()
        => new(
            "feature-authority-unknown-aliases",
            DesignConversionMachine.DomainTag,
            [
                new LearningTrainingItem(
                    "allowed-control",
                    $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"figma\"}}",
                    "{\"html\":\"<main><section>chosen</section></main>\",\"css\":\"main{display:grid}section{padding:1rem}\"}",
                    "{\"html\":\"<main><section>rejected</section></main>\",\"css\":\"main{display:block}section{padding:0}\"}"),
                new LearningTrainingItem(
                    "unknown-source-alias",
                    $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"accepted_v2\"}}",
                    "{\"html\":\"<main><article>chosen-alias</article></main>\",\"css\":\"main{display:grid}article{padding:2rem}\"}",
                    "{\"html\":\"<main><article>rejected-alias</article></main>\",\"css\":\"main{display:block}article{padding:0}\"}")
            ],
            [
                "feature-authority-source:allowed-control",
                "feature-authority-source:unknown-source"
            ]);

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildAllowedBridgeControlBatch()
        => new(
            "feature-authority-all-allowed",
            DesignConversionMachine.DomainTag,
            [
                BuildCriticFeatureRow("figma", includeVector: true)
            ],
            ["feature-authority-source:figma"]);

    private static LearningTrainingItem BuildCriticFeatureRow(string sourceKind, bool includeVector)
        => new(
            $"allowed-source-{sourceKind}",
            includeVector
                ? $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"{sourceKind}\",\"internal_vector\":[0.25,0.75]}}"
                : $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"{sourceKind}\"}}",
            $"{{\"html\":\"<main><section>chosen-{sourceKind}</section></main>\",\"css\":\"main{{display:grid}}section{{padding:1rem}}\"}}",
            $"{{\"html\":\"<main><section>rejected-{sourceKind}</section></main>\",\"css\":\"main{{display:block}}section{{padding:0}}\"}}");

    private static DesignConversionMachineGradientBridgeResult ValidBridgeResult(
        DesignConversionMachineGradientBridgeRequest request,
        ImmutableArray<DesignConversionMachineGradientBridgeUpdate> updates)
        => new(
            request.BatchIdentityHash,
            request.DomainTag,
            request.InputMachineVersion,
            AverageRewardMargin: 0.5,
            updates);

    private static async Task<DesignConversionMachine> CreateMachineWithAllowedControlAsync()
    {
        var machine = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            machine,
            new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
                "feature-authority-current-state",
                DesignConversionMachine.DomainTag,
                [
                    new LearningTrainingItem(
                        "allowed-current-state",
                        $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"figma\"}}",
                        "{\"html\":\"<main><section>chosen</section></main>\",\"css\":\"main{display:grid}section{padding:1rem}\"}",
                        "{\"html\":\"<main><section>rejected</section></main>\",\"css\":\"main{display:block}section{padding:0}\"}")
                ],
                ["feature-authority-current-source"]));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        return machine;
    }

    private static DesignConversionMachineCheckpoint AddUnknownLearnedWeight(
        DesignConversionMachineCheckpoint checkpoint,
        string unknownFeatureKey)
    {
        var learnedWeights = (checkpoint.CriticUnit.LearnedWeights ?? [])
            .Append(new DesignConversionLearnedWeightCheckpoint(
                unknownFeatureKey,
                Weight: 0.25,
                ObservationCount: 1,
                LastBatchId: "forged-unknown-feature",
                Derivation: "forged-import",
                SourceIdentityHashes: [SourceHash],
                EvidenceRefs:
                [
                    DesignConversionMachine.LearnedWeightsPersistedEvidence,
                    DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
                ]))
            .ToArray();
        return checkpoint with
        {
            CriticUnit = checkpoint.CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights = learnedWeights
            }
        };
    }

    private static DesignConversionMachineCheckpoint WithAllAllowedLearnedWeights(
        DesignConversionMachineCheckpoint checkpoint)
        => checkpoint with
        {
            CriticUnit = checkpoint.CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights = AllowedFeatureKeys
                    .Where(key => !key.StartsWith("correction_category:", StringComparison.Ordinal))
                    .Select(ValidLearnedWeight)
                    .ToArray()
            },
            ImproverUnit = checkpoint.ImproverUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights = AllowedFeatureKeys
                    .Where(key => key.StartsWith("correction_category:", StringComparison.Ordinal))
                    .Select(ValidLearnedWeight)
                    .ToArray()
            }
        };

    private static DesignConversionLearnedWeightCheckpoint ValidLearnedWeight(string featureKey)
        => new(
            featureKey,
            Weight: 0.25,
            ObservationCount: 1,
            LastBatchId: "allowlisted-feature-roundtrip",
            Derivation: "allowlisted-checkpoint-control",
            SourceIdentityHashes: [SourceHash],
            EvidenceRefs:
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
            ]);

    private static string[] AllFeatureKeys(DesignConversionMachineCheckpoint checkpoint)
        => (checkpoint.CriticUnit.LearnedWeights ?? [])
            .Concat(checkpoint.ImproverUnit.LearnedWeights ?? [])
            .Select(weight => weight.FeatureKey)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string[] AccumulatedFeatureKeys(DesignConversionMachine machine)
    {
        var field = typeof(DesignConversionMachine).GetField(
            "_learnedWeightsByUnit",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var byUnit = Assert.IsAssignableFrom<IDictionary>(field?.GetValue(machine));
        return byUnit.Values
            .Cast<IDictionary>()
            .SelectMany(byFeature => byFeature.Keys.Cast<string>())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static DesignConversionMachineCheckpoint Export(DesignConversionMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!;
    }

    private static async Task<string> SnapshotBoundStateAsync(DesignConversionMachine machine)
    {
        var checkpoint = Export(machine) with { ExportedAtUtc = DateTimeOffset.UnixEpoch };
        var boundExport = await machine.ExportAsync();
        Assert.True(boundExport.IsSuccess, boundExport.ErrorMessage);
        var stableBoundExport = boundExport.Value! with { ExportedAtUtc = DateTimeOffset.UnixEpoch };
        return JsonSerializer.Serialize(
            new { checkpoint, boundExport = stableBoundExport },
            DesignConversionCheckpointJsonOptions.Standard);
    }

    private static string CreateTempRoot(string name)
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private sealed class RecordingGradientBridge(
        Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult> handler)
        : IDesignConversionMachineGradientBridge
    {
        public int CallCount { get; private set; }

        public DesignConversionMachineGradientBridgeRequest? LastRequest { get; private set; }

        public Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(handler(request)));
        }
    }
}
