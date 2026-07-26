// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using OrchestratorModel.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using Domain = SemanticCompiler29.Domain;

public sealed class TeachingRegistryPersistenceTests
{
    [Fact]
    public void UnitRegistry_LoadEntries_PreservesRegistrationOrderAndResolveLatest()
    {
        var registry = new InMemoryLogicalUnitRegistry();
        var loadedUnit = new StaticStringUnit("loaded-unit", "loaded output");
        var descriptor = new LogicalUnitDescriptor(
            "loaded-unit",
            "generated-capability",
            "4.1",
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            true,
            typeof(string),
            typeof(string),
            7);

        var load = registry.LoadEntries([new LogicalUnitRegistryLoadedEntry(descriptor, loadedUnit)]);

        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(7, Assert.Single(registry.ListDescriptors()).RegistrationOrder);
        Assert.True(registry.ResolveLatest<string, string>("generated-capability", "4.1").IsSuccess);

        var next = registry.Register<string, string>(
            "next-unit",
            "generated-capability",
            "4.1",
            "1.0.1",
            LogicalUnitTrustState.Probationary,
            true,
            new StaticStringUnit("next-unit", "next output"));

        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.True(next.Value!.RegistrationOrder > 7);
    }

    [Fact]
    public void FlowRegistry_LoadEntries_PreservesRegistrationOrderAndResolveBinding()
    {
        var registry = new InMemoryLogicalFlowRegistry();
        var descriptor = new LogicalFlowDescriptor(
            "loaded-flow-4-1",
            "loaded-scenario-4-1",
            "1.0.0",
            typeof(string),
            "4",
            "4.1",
            "backend-services",
            ["software-development"],
            9);

        var load = registry.LoadEntries([
            new LogicalFlowRegistryLoadedEntry(
                descriptor,
                new UniversalNineNodeDevelopmentFlow("4", "4.1", "backend-services", ["software-development"], ["IMPLEMENT"]))
        ]);

        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(9, Assert.Single(registry.ListDescriptors()).RegistrationOrder);
        Assert.True(registry.ResolveBindingBySubPhase("4.1").IsSuccess);

        var next = registry.Register(
            "next-flow",
            "next-scenario",
            "1.0.1",
            new UniversalNineNodeDevelopmentFlow("4", "4.2", "frontend", ["software-development"], ["IMPLEMENT"]));

        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.True(next.Value!.RegistrationOrder > 9);
    }

    [Fact]
    public void LoadRegistry_WithMatchingProbationaryTrustState_LoadsGeneratedCapability()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var snapshot = BuildValidTrustStateSnapshot(artifacts);
        var units = new InMemoryLogicalUnitRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            units,
            flows);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value);
        var descriptor = Assert.Single(units.ListDescriptors());
        Assert.Equal("trust-boundary-generated-unit", descriptor.UnitId);
        Assert.Equal(DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole, descriptor.UnitRole);
        Assert.Equal("PLAN", descriptor.DomainTag);
        Assert.Equal(LogicalUnitTrustState.Probationary, descriptor.TrustState);
        Assert.True(descriptor.IsLearned);
        Assert.Empty(flows.ListDescriptors());
        Assert.True(units.ResolveLatest<string, string>(descriptor.UnitRole, descriptor.DomainTag).IsSuccess);
    }

    [Fact]
    public void LoadRegistry_WithBlankUnitTrustState_FailsWithTrustStateInvalid()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            Units = [Assert.Single(validSnapshot.Units) with { TrustState = "" }]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.GeneratedCapabilities, snapshot.GeneratedCapabilities);

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "TEACHING_UNIT_TRUST_STATE_INVALID",
            "Teaching unit trust state is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_WithUnknownManifestTrustState_FailsWithCapabilityTrustStateInvalid()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            GeneratedCapabilities =
            [
                Assert.Single(validSnapshot.GeneratedCapabilities) with { TrustState = "Unknown" }
            ]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.Units, snapshot.Units);

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_TRUST_STATE_INVALID",
            "Generated capability trust state is invalid.",
            registries);
    }

    [Fact]
    public void LoadRegistry_WithUnitAndManifestTrustMismatch_FailsWithMismatchCode()
    {
        using var artifacts = CreateLoadableTrustStateArtifacts();
        var validSnapshot = BuildValidTrustStateSnapshot(artifacts);
        var snapshot = validSnapshot with
        {
            GeneratedCapabilities =
            [
                Assert.Single(validSnapshot.GeneratedCapabilities) with
                {
                    TrustState = nameof(LogicalUnitTrustState.Trusted)
                }
            ]
        };
        var registries = BuildRegistriesWithSentinels();

        Assert.Same(validSnapshot.Units, snapshot.Units);

        var result = TeachingRegistryPersistence.LoadIntoRegistries(
            snapshot,
            BuildTopologyWithSubPhase41(),
            registries.Units,
            registries.Flows);

        AssertFailedLoadWithoutRegistryMutation(
            result,
            "GENERATED_CAPABILITY_TRUST_STATE_MISMATCH",
            "Generated capability trust state does not match its teaching unit.",
            registries);
    }

    [Fact]
    public async Task CapabilityTrainer_WritesLoadableGeneratedCapabilityArtifacts()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"capability-artifacts-{Guid.NewGuid():N}");
        try
        {
            var topology = BuildTopologyWithSubPhase41();
            var flows = new InMemoryLogicalFlowRegistry();
            var units = new InMemoryLogicalUnitRegistry();
            var branch = BuildKnowledgeEdgeBranch("4.1");
            var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

            var result = await trainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "artifact-run",
                BuildRequest("artifact-run"),
                [branch],
                topology,
                flows,
                units)
            {
                Domain = Domain.PLAN
            });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);

            var manifestPath = Assert.Single(Directory.GetFiles(
                checkpointRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories));
            var snapshot = TeachingRegistryPersistence.Capture("bundle-artifact", 1, units, flows, checkpointRoot);

            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var manifest = Assert.Single(snapshot.Value!.GeneratedCapabilities);
            var generatedUnit = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole);
            Assert.Equal(generatedUnit.UnitId, manifest.UnitId);
            Assert.Equal(Assert.Single(flows.ListDescriptors()).FlowId, manifest.FlowId);
            Assert.True(File.Exists(manifest.TokenizerPath));
            Assert.True(File.Exists(manifest.PolicyCheckpointPath));
            Assert.Equal(Path.GetFullPath(manifestPath), Path.GetFullPath(Directory.GetFiles(
                checkpointRoot,
                TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
                SearchOption.AllDirectories).Single()));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SnapshotLoad_MakesPriorRowVisibleWithoutCapabilityGeneration()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"snapshot-load-{Guid.NewGuid():N}");
        try
        {
            var topology = BuildTopologyWithSubPhase41();
            var rowOneFlows = new InMemoryLogicalFlowRegistry();
            var rowOneUnits = new InMemoryLogicalUnitRegistry();
            var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
            var training = await trainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "row-1-run",
                BuildRequest("row-1-run"),
                [BuildKnowledgeEdgeBranch("4.1")],
                topology,
                rowOneFlows,
                rowOneUnits)
            {
                Domain = Domain.PLAN
            });
            Assert.True(training.IsSuccess, training.ErrorMessage);

            var snapshot = TeachingRegistryPersistence.Capture("bundle-snapshot", 1, rowOneUnits, rowOneFlows, checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);

            var loadedFlows = new InMemoryLogicalFlowRegistry();
            var loadedUnits = new InMemoryLogicalUnitRegistry();
            var load = TeachingRegistryPersistence.LoadIntoRegistries(snapshot.Value!, topology, loadedUnits, loadedFlows);
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var binding = loadedFlows.ResolveBindingBySubPhase(
                "4.1",
                new LogicalFlowFactoryRequest(
                    "row-2-run",
                    new LogicalExecutionFlags(),
                    loadedUnits,
                    new Dictionary<string, object>()));
            Assert.True(binding.IsSuccess, binding.ErrorMessage);

            var result = await binding.Value!.Flow.ExecuteAsync(new LogicalFlowRequest<string>(
                "row-2-run",
                binding.Value.FlowId,
                binding.Value.ScenarioId,
                "Implement the backend service and provider connector.",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("PASS", result.Value!.OverallVerdict);
            Assert.Equal("True", result.Value.Metrics["learned_unit_selected"]);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TeachingSessionBuilder_LoadsPriorSnapshotBeforeNextRow()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"teaching-session-{Guid.NewGuid():N}");
        var snapshotPath = Path.Combine(checkpointRoot, "bundle-registry-snapshot.json");
        try
        {
            var rowOneSession = TeachingSessionBuilder.Open(
                new TeachingNullSessionStorage(),
                "bundle-teaching-session",
                checkpointRoot,
                snapshotPath);
            Assert.True(rowOneSession.IsSuccess, rowOneSession.ErrorMessage);

            var rowOne = await new DefaultLogicalCapabilityTrainer(checkpointRoot).TrainAndRegisterAsync(
                new LogicalCapabilityTrainingRequest(
                    "row-one-teaching-session",
                    BuildRequest("row-one-teaching-session"),
                    [BuildKnowledgeEdgeBranch("4.1")],
                    rowOneSession.Value!.Topology,
                    rowOneSession.Value.Flows,
                    rowOneSession.Value.Units)
                {
                    Domain = Domain.PLAN
                });
            Assert.True(rowOne.IsSuccess, rowOne.ErrorMessage);
            Assert.True(rowOne.Value!.CapabilityRegistered);

            var saved = rowOneSession.Value.SaveSnapshot(1);
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            Assert.True(File.Exists(snapshotPath));

            var rowTwoSession = TeachingSessionBuilder.Open(
                new TeachingNullSessionStorage(),
                "bundle-teaching-session",
                checkpointRoot,
                snapshotPath);
            Assert.True(rowTwoSession.IsSuccess, rowTwoSession.ErrorMessage);

            Assert.Contains(rowTwoSession.Value!.Units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == Domain.PLAN.ToString());
            Assert.Contains(rowTwoSession.Value.Flows.ListDescriptors(), descriptor =>
                descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal) &&
                descriptor.SubPhaseId == "4.1");

            var rowTwo = await rowTwoSession.Value.Assistant.RunAsync(
                "Implement the backend service and provider connector for the integration layer.");

            Assert.True(rowTwo.IsSuccess, rowTwo.ErrorMessage);
            Assert.Equal("PASS", rowTwo.Value!.OverallVerdict);
            Assert.Equal("True", rowTwo.Value.Metrics["learned_unit_selected"]);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static InMemoryLogicalTopologyRegistry BuildTopologyWithSubPhase41()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["teaching-registry-persistence-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);
        return topology;
    }

    private static TeachingRegistrySnapshot BuildValidTrustStateSnapshot(
        LoadableTrustStateArtifacts artifacts)
    {
        const string unitId = "trust-boundary-generated-unit";
        const string unitRole = DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole;
        const string domainTag = "PLAN";
        const string version = "1.0.0";
        var createdAtUtc = DateTimeOffset.UnixEpoch;

        return new TeachingRegistrySnapshot(
            SnapshotId: "trust-boundary-snapshot",
            BundleId: "trust-boundary-bundle",
            RowIndex: 1,
            CreatedAtUtc: createdAtUtc,
            Units:
            [
                new TeachingUnitRegistryEntry(
                    UnitId: unitId,
                    UnitRole: unitRole,
                    DomainTag: domainTag,
                    Version: version,
                    TrustState: nameof(LogicalUnitTrustState.Probationary),
                    IsLearned: true,
                    InputTypeName: typeof(string).AssemblyQualifiedName!,
                    OutputTypeName: typeof(string).AssemblyQualifiedName!,
                    RegistrationOrder: 2,
                    UnitKind: TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit)
            ],
            Flows: [],
            GeneratedCapabilities:
            [
                new GeneratedCapabilityCheckpointManifest(
                    UnitId: unitId,
                    UnitRole: unitRole,
                    DomainTag: domainTag,
                    Version: version,
                    TrustState: nameof(LogicalUnitTrustState.Probationary),
                    FlowId: "trust-boundary-generated-flow",
                    ScenarioId: "trust-boundary-scenario",
                    PhaseId: "4",
                    SubPhaseId: "4.1",
                    TaskType: "backend-services",
                    TrainingText: "Implement the backend service.",
                    OutputText: "PASS",
                    ExampleId: "trust-boundary-example",
                    PredicateIds: ["IMPLEMENT"],
                    DomainTags: ["software-development"],
                    PolicyCheckpointPath: artifacts.PolicyCheckpointPath,
                    TokenizerPath: artifacts.TokenizerPath,
                    VocabSize: artifacts.ModelConfig.VocabSize,
                    ContextLen: artifacts.ModelConfig.ContextLen,
                    HiddenDim: artifacts.ModelConfig.HiddenDim,
                    NumLayers: artifacts.ModelConfig.NumLayers,
                    NumHeads: artifacts.ModelConfig.NumHeads,
                    NumKvHeads: artifacts.ModelConfig.NumKvHeads,
                    FfnHiddenDim: artifacts.ModelConfig.FfnHiddenDim,
                    LoraRank: artifacts.LoraConfig.Rank,
                    LoraAlpha: artifacts.LoraConfig.Alpha,
                    LoraDropout: artifacts.LoraConfig.Dropout,
                    LoraTargetModules: artifacts.LoraConfig.TargetModules,
                    CreatedAtUtc: createdAtUtc)
            ],
            CheckpointPaths: []);
    }

    private static LoadableTrustStateArtifacts CreateLoadableTrustStateArtifacts()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"teaching-registry-trust-boundary-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(
                "Implement the backend service and provider connector. PASS",
                vocabSize: 260);
            var tokenizerPath = Path.Combine(root, "tokenizer.json");
            tokenizer.Save(tokenizerPath);

            var modelConfig = new ModelConfig
            {
                VocabSize = tokenizer.VocabSize,
                ContextLen = 32,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 16
            };
            var loraConfig = new LoraConfig
            {
                Rank = 2,
                Alpha = 4.0f,
                Dropout = 0.0f,
                TargetModules = ["q_proj", "v_proj"]
            };
            var policyModel = new LlmModel(modelConfig);
            LoraApplicator.Apply(policyModel, loraConfig);
            var policyCheckpointPath = Path.Combine(root, "policy.bin");
            policyModel.SaveCheckpoint(policyCheckpointPath);

            Assert.True(File.Exists(tokenizerPath));
            Assert.True(File.Exists(policyCheckpointPath));
            return new LoadableTrustStateArtifacts(
                root,
                policyCheckpointPath,
                tokenizerPath,
                modelConfig,
                loraConfig);
        }
        catch
        {
            Directory.Delete(root, recursive: true);
            throw;
        }
    }

    private static (
        InMemoryLogicalUnitRegistry Units,
        InMemoryLogicalFlowRegistry Flows,
        LogicalUnitDescriptor[] UnitDescriptors,
        LogicalFlowDescriptor[] FlowDescriptors) BuildRegistriesWithSentinels()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var unitRegistration = units.Register<string, string>(
            "trust-boundary-sentinel-unit",
            "sentinel-role",
            "sentinel-domain",
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            false,
            new StaticStringUnit("trust-boundary-sentinel-unit", "sentinel output"));
        Assert.True(unitRegistration.IsSuccess, unitRegistration.ErrorMessage);

        var flows = new InMemoryLogicalFlowRegistry();
        var flowRegistration = flows.Register(
            "trust-boundary-sentinel-flow",
            "trust-boundary-sentinel-scenario",
            "1.0.0",
            new UniversalNineNodeDevelopmentFlow(
                "4",
                "4.1",
                "backend-services",
                ["software-development"],
                ["IMPLEMENT"]));
        Assert.True(flowRegistration.IsSuccess, flowRegistration.ErrorMessage);

        return (
            units,
            flows,
            units.ListDescriptors().ToArray(),
            flows.ListDescriptors().ToArray());
    }

    private static void AssertFailedLoadWithoutRegistryMutation(
        OperationResult<int> result,
        string expectedErrorCode,
        string expectedErrorMessage,
        (
            InMemoryLogicalUnitRegistry Units,
            InMemoryLogicalFlowRegistry Flows,
            LogicalUnitDescriptor[] UnitDescriptors,
            LogicalFlowDescriptor[] FlowDescriptors) registries)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(expectedErrorMessage, result.ErrorMessage);
        Assert.Equal(registries.UnitDescriptors, registries.Units.ListDescriptors());
        Assert.Equal(registries.FlowDescriptors, registries.Flows.ListDescriptors());
    }

    private static LogicalOrchestratorRequest BuildRequest(string runId)
    {
        return new LogicalOrchestratorRequest(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement the backend service and provider connector for the integration layer.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(string subPhaseId)
    {
        return new LogicalFlowResult(
            AdaptiveLogicalOrchestrator.KnowledgeEdgeFlowId,
            $"generated-capability-{subPhaseId}",
            "KNOWLEDGE_EDGE",
            $"Missing capability for {subPhaseId}.",
            new LogicalFlowSummary(
                "knowledge-edge",
                1,
                0,
                0,
                "knowledge_edge",
                false,
                false,
                false,
                "TRAIN_CAPABILITY",
                1,
                subPhaseId,
                "Missing capability.",
                0,
                0),
            new Dictionary<string, string>
            {
                ["missing_sub_phase"] = subPhaseId,
                ["knowledge_edge"] = true.ToString()
            },
            [],
            []);
    }

    private sealed class StaticStringUnit : ILogicalUnit<string, string>
    {
        private readonly string _unitId;
        private readonly string _output;

        public StaticStringUnit(string unitId, string output)
        {
            _unitId = unitId;
            _output = output;
        }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(new LogicalUnitResult<string>(
                _unitId,
                request.UnitRole,
                request.DomainTag,
                _output,
                0.95,
                ["IMPLEMENT"],
                [request.DomainTag],
                [_unitId],
                [])));
        }
    }

    private sealed record LoadableTrustStateArtifacts(
        string Root,
        string PolicyCheckpointPath,
        string TokenizerPath,
        ModelConfig ModelConfig,
        LoraConfig LoraConfig) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class TeachingNullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        {
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        {
            return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));
        }

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        {
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
        }
    }
}
