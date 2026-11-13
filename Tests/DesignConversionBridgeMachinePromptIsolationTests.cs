// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;

public sealed class DesignConversionBridgeMachinePromptIsolationTests
{
    [Fact]
    public void BridgeRuntimeBatch_UsesCanonicalSemanticPromptAndKeepsCustodyInSourceIds()
    {
        var record = CreateRecord();
        var firstSha = new string('a', 64);
        var secondSha = new string('b', 64);
        var first = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyRuntimeBridgeBatchPreviewForTests(
            [record],
            "C:/private/checkpoints/first-policy.json",
            firstSha);
        var identityMutation = record with
        {
            RecordId = "different-record-id",
            InternalUnitStatePayload = "{\"checkpoint\":\"different\"}",
            Chosen = record.Chosen with
            {
                OutputId = "different-chosen-output",
                CandidateId = "different-chosen-candidate",
                Verdict = record.Chosen.Verdict with { VerdictId = "different-chosen-verdict" }
            },
            Rejected = record.Rejected with
            {
                OutputId = "different-rejected-output",
                CandidateId = "different-rejected-candidate",
                Verdict = record.Rejected.Verdict with { VerdictId = "different-rejected-verdict" }
            },
            Provenance = record.Provenance with
            {
                SourceRequestId = "different-request",
                SourceManifestRef = "manifest://different/source",
                ReviewerRef = "different-reviewer"
            }
        };
        var second = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyRuntimeBridgeBatchPreviewForTests(
            [identityMutation],
            "D:/other/checkpoints/second-policy.json",
            secondSha);

        var firstItem = Assert.Single(first.TrainingItems);
        var secondItem = Assert.Single(second.TrainingItems);
        Assert.Equal(
            DesignConversionDpoDatasetBuilder.BuildOptimizerPromptPayload(
                record,
                DesignConversionMachine.DomainTag),
            firstItem.Prompt);
        Assert.Equal(firstItem.Prompt, secondItem.Prompt);
        Assert.NotEqual(first.SourceIds.Order(StringComparer.Ordinal), second.SourceIds.Order(StringComparer.Ordinal));
        Assert.Contains(first.SourceIds, sourceId => sourceId.Contains(firstSha, StringComparison.Ordinal));
        Assert.Contains(second.SourceIds, sourceId => sourceId.Contains(secondSha, StringComparison.Ordinal));
        Assert.Contains(first.SourceIds, sourceId => sourceId.EndsWith("first-policy.json", StringComparison.Ordinal));
        Assert.Contains(second.SourceIds, sourceId => sourceId.EndsWith("second-policy.json", StringComparison.Ordinal));

        using var prompt = JsonDocument.Parse(firstItem.Prompt);
        Assert.Equal(
            ["domain_tag", "unit_id", "source_kind", "internal_vector", "target", "source", "source_encoder"],
            prompt.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Contains("FRAME", prompt.RootElement.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.Equal([0.25, -0.5, 0.75, 1.0], prompt.RootElement.GetProperty("internal_vector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
        foreach (var forbidden in new[]
                 {
                     "source_manifest_hash", "source_identity_hash", "source_structure_hash",
                     "source_encoder_vector_hash", "source_encoder_feature_map_hash",
                     "source_vector_payload_hash", "dpo_policy_checkpoint", "pair_family",
                     firstSha, "first-policy.json", "C:/private"
                 })
        {
            Assert.DoesNotContain(forbidden, firstItem.Prompt, StringComparison.OrdinalIgnoreCase);
        }

        AssertCanonicalResponse(firstItem.ChosenResponse, record.Chosen.Html, record.Chosen.Css);
        AssertCanonicalResponse(firstItem.RejectedResponse, record.Rejected.Html, record.Rejected.Css);

        var semanticMutation = record with
        {
            InternalVectorPayload = [0.9, 0.8],
            SourceStructurePayload = "{\"type\":\"COMPONENT\",\"name\":\"semantic-change\"}",
            Provenance = record.Provenance with { SourceKind = "Canva" }
        };
        var semanticPrompt = DesignConversionDpoPolicyCheckpointBridge.BuildPolicyRuntimeBridgePromptPreviewForTests(
            semanticMutation,
            "C:/private/checkpoints/first-policy.json",
            firstSha);
        Assert.NotEqual(firstItem.Prompt, semanticPrompt);
    }

    [Fact]
    public async Task Machine_SourceIdentityChangesOnlyCustodyHashesWhileSemanticSignalsAndRuntimeStayInvariant()
    {
        var first = new DesignConversionMachine();
        var second = new DesignConversionMachine();
        var firstTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            first,
            CreateBatch("identity-invariant", "figma", [0.2, 0.4], "manifest://first", new string('1', 64)));
        var secondTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            second,
            CreateBatch("identity-invariant", "figma", [0.2, 0.4], "manifest://second", new string('2', 64), includeExtraCustodyId: true));
        Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
        Assert.True(secondTrain.IsSuccess, secondTrain.ErrorMessage);

        var firstCheckpoint = Export(first);
        var secondCheckpoint = Export(second);
        Assert.Equal(SemanticWeights(firstCheckpoint), SemanticWeights(secondCheckpoint));
        Assert.NotEqual(
            SourceHashes(firstCheckpoint).Order(StringComparer.Ordinal),
            SourceHashes(secondCheckpoint).Order(StringComparer.Ordinal));
        Assert.DoesNotContain(SemanticWeights(firstCheckpoint), weight => weight.FeatureKey.StartsWith("pair_family:", StringComparison.Ordinal));
        Assert.DoesNotContain(SemanticWeights(firstCheckpoint), weight => string.Equals(weight.FeatureKey, "source_manifest:hash_diversity", StringComparison.Ordinal));

        var runtimeInput = DesignConversionTestFixtures.CreateInput();
        Assert.Equal(
            await RuntimeBehavior(first, runtimeInput, "identity-first"),
            await RuntimeBehavior(second, runtimeInput, "identity-second"));

        var semanticMutation = new DesignConversionMachine();
        var semanticTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            semanticMutation,
            CreateBatch("identity-invariant", "canva", [0.8, 0.9], "manifest://first", new string('1', 64)));
        Assert.True(semanticTrain.IsSuccess, semanticTrain.ErrorMessage);
        Assert.NotEqual(SemanticWeights(firstCheckpoint), SemanticWeights(Export(semanticMutation)));
    }

    [Fact]
    public async Task ConfiguredGradientBridge_CannotObserveSourceIdentityOrCardinality()
    {
        var firstBridge = new CapturingSemanticGradientBridge();
        var secondBridge = new CapturingSemanticGradientBridge();
        var first = new DesignConversionMachine(new DesignConversionRoutingFlow(), firstBridge);
        var second = new DesignConversionMachine(new DesignConversionRoutingFlow(), secondBridge);

        var firstTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            first,
            CreateBatch("configured-identity-invariant", "figma", [0.2, 0.4], "manifest://first", new string('1', 64)));
        var secondTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            second,
            CreateBatch(
                "configured-identity-invariant",
                "figma",
                [0.2, 0.4],
                "manifest://second",
                new string('2', 64),
                includeExtraCustodyId: true));

        Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
        Assert.True(secondTrain.IsSuccess, secondTrain.ErrorMessage);
        Assert.Equal(firstBridge.SerializedRequest, secondBridge.SerializedRequest);
        Assert.Equal(SemanticWeights(Export(first)), SemanticWeights(Export(second)));
        Assert.NotEqual(
            SourceHashes(Export(first)).Order(StringComparer.Ordinal),
            SourceHashes(Export(second)).Order(StringComparer.Ordinal));

        var runtimeInput = DesignConversionTestFixtures.CreateInput();
        Assert.Equal(
            await RuntimeBehavior(first, runtimeInput, "configured-identity-first"),
            await RuntimeBehavior(second, runtimeInput, "configured-identity-second"));

        Assert.DoesNotContain(
            typeof(DesignConversionMachineGradientBridgeSanitizedSignal).GetProperties(),
            property => property.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("SourceId", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(DesignConversionMachineGradientBridgeUpdate).GetProperties(),
            property => property.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("SourceId", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("source_manifest:hash_diversity")]
    [InlineData("pair_family:legacy_route")]
    public async Task Machine_LegacyIdentityFeatureCheckpointFailsClosed(string legacyFeatureKey)
    {
        var source = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            source,
            CreateBatch("legacy-source", "figma", [0.2, 0.4], "manifest://source", new string('3', 64)));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var checkpoint = Export(source);
        var seed = Assert.Single(
            checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals(weight.FeatureKey, "source_kind:figma", StringComparison.Ordinal));
        var forged = checkpoint with
        {
            CriticUnit = checkpoint.CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights = [seed with { FeatureKey = legacyFeatureKey }]
            }
        };

        var target = new DesignConversionMachine();
        var import = target.ImportCheckpoint(forged);

        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_LEARNED_WEIGHTS_INVALID", import.ErrorCode);
        var targetCheckpoint = Export(target);
        Assert.False(targetCheckpoint.CriticUnit.LearnedWeightsPresent);
        Assert.Empty(targetCheckpoint.CriticUnit.LearnedWeights ?? []);
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> CreateBatch(
        string batchId,
        string sourceKind,
        IReadOnlyList<double> vector,
        string manifestRef,
        string identityHash,
        bool includeExtraCustodyId = false)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "semantic-item",
                    Prompt: JsonSerializer.Serialize(new
                    {
                        unit_id = DesignConversionDpoUnits.CriticUnit,
                        source_kind = sourceKind,
                        internal_vector = vector,
                        source_manifest_hash = identityHash,
                        source_identity_hash = identityHash,
                        source_structure_hash = identityHash,
                        pair_family = "legacy_route"
                    }),
                    ChosenResponse: "{\"html\":\"<main>chosen</main>\",\"css\":\"main{display:grid}\"}",
                    RejectedResponse: "{\"html\":\"<main>rejected</main>\",\"css\":\"main{display:block}\"}")
            ],
            SourceIds: includeExtraCustodyId
                ? [manifestRef, $"checkpoint:{identityHash}", $"record:{identityHash}"]
                : [manifestRef, $"checkpoint:{identityHash}"]);

    private static DesignConversionMachineCheckpoint Export(DesignConversionMachine machine)
    {
        var result = machine.ExportCheckpoint();
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static IReadOnlyList<SemanticWeight> SemanticWeights(DesignConversionMachineCheckpoint checkpoint)
        => (checkpoint.CriticUnit.LearnedWeights ?? [])
            .OrderBy(weight => weight.FeatureKey, StringComparer.Ordinal)
            .Select(weight => new SemanticWeight(
                weight.FeatureKey,
                weight.Weight,
                weight.ObservationCount,
                weight.Derivation))
            .ToArray();

    private static IReadOnlyList<string> SourceHashes(DesignConversionMachineCheckpoint checkpoint)
        => (checkpoint.CriticUnit.LearnedWeights ?? [])
            .SelectMany(weight => weight.SourceIdentityHashes)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static async Task<RuntimeSnapshot> RuntimeBehavior(
        DesignConversionMachine machine,
        DesignConversionRuntimeFlowInput input,
        string runId)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);
        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: runId,
            ScenarioId: $"{runId}-scenario",
            UserRequest: "convert this design",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object> { ["runtime_flow_input"] = input }));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return new RuntimeSnapshot(
            result.Value!.FlowResult.Summary.EnhancedPrimary,
            result.Value.FlowResult.Summary.Passed,
            result.Value.OverallVerdict,
            result.Value.FlowResult.Summary.NextAction);
    }

    private static void AssertCanonicalResponse(string json, string html, string css)
    {
        using var document = JsonDocument.Parse(json);
        Assert.Equal(["html", "css"], document.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(html, document.RootElement.GetProperty("html").GetString());
        Assert.Equal(css, document.RootElement.GetProperty("css").GetString());
    }

    private static DesignConversionDpoTrainingRecord CreateRecord()
    {
        var chosen = new DesignConversionImprovementOutput(
            "chosen-output", "chosen-candidate", "<main>chosen</main>", "main{display:grid}",
            new VisualVerdict("chosen-verdict", 0.95, [], ["chosen-evidence"], true), [], ["chosen-evidence"], true);
        var rejected = new DesignConversionImprovementOutput(
            "rejected-output", "rejected-candidate", "<main>rejected</main>", "main{display:block}",
            new VisualVerdict("rejected-verdict", 0.2, [], ["rejected-evidence"], false), [], ["rejected-evidence"], false);
        return new DesignConversionDpoTrainingRecord(
            "record-id", DesignConversionDpoUnits.CriticUnit, "{\"state\":\"private\"}", [0.25, -0.5, 0.75, 1.0],
            "{\"type\":\"FRAME\",\"name\":\"dashboard\",\"children\":[{\"type\":\"TEXT\",\"characters\":\"Title\"}]}",
            chosen, rejected, null, 0.9, "negative", "visual_hierarchy",
            new DesignConversionDpoProvenance(
                "request-id", "Figma", "manifest://source/one", "reviewer-one", DateTimeOffset.UnixEpoch,
                new Dictionary<string, string>
                {
                    ["source_encoder_feature_names"] = "source_json_node_count,source_layout_hint_count",
                    ["source_structure_signal_summary"] = "nodes=3;depth=2;properties=7",
                    ["source_encoder_vector_hash"] = new string('c', 64)
                }),
            new DesignConversionLicenseScope("license", "training", true, false, new Dictionary<string, string>()),
            new DesignConversionQuarantineState(false, string.Empty, []),
            TrainingTeacherProvenance.Deterministic("semantic-test"));
    }

    private sealed record SemanticWeight(string FeatureKey, double Weight, int ObservationCount, string Derivation);
    private sealed record RuntimeSnapshot(double Score, bool Passed, string OverallVerdict, string NextAction);

    private sealed class CapturingSemanticGradientBridge : IDesignConversionMachineGradientBridge
    {
        public string? SerializedRequest { get; private set; }

        public Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            SerializedRequest = JsonSerializer.Serialize(request);
            var sourceKindSignal = request.SanitizedSignals.Single(
                signal => string.Equals(signal.AllowedFeatureKey, "source_kind:figma", StringComparison.Ordinal));
            return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                request.BatchIdentityHash,
                request.DomainTag,
                request.InputMachineVersion,
                AverageRewardMargin: 0.25,
                Updates:
                [
                    new DesignConversionMachineGradientBridgeUpdate(
                        sourceKindSignal.CanonicalUnitId,
                        sourceKindSignal.AllowedFeatureKey,
                        Contribution: 0.125)
                ])));
        }
    }
}
