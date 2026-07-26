// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using Domain = SemanticCompiler29.Domain;

public sealed class CapabilityResultTranslatorAndDeeperModelTests
{
    [Fact]
    public void CapabilityResultTranslator_UsesSubPhaseMetadataInsteadOfMachineLabel()
    {
        var subPhase = BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model, authentication, authorization, token, OAuth, OIDC, ASVS, session, and tenant-isolation routing.", ["SECURITY", "AUTHORIZATION"]);

        var translated = CapabilityResultTranslator.Translate(
            "Generated capability for sub-phase 3.4: Security and Authorization Model.",
            subPhase,
            "fix the auth check in the tenant service");

        Assert.Contains("3.4", translated);
        Assert.Contains("Security and Authorization Model", translated);
        Assert.Contains("SECURITY", translated);
        Assert.Contains("AUTHORIZATION", translated);
        Assert.Contains("fix the auth check in the tenant service", translated);
        Assert.DoesNotContain("Generated capability", translated);
    }

    [Fact]
    public void CapabilityResultTranslator_FallbackIncludesPromptWhenTopologyUnavailable()
    {
        var translated = CapabilityResultTranslator.TranslateFromKnnOutput(
            "Generated capability for sub-phase 4.1: Backend Services.",
            "add the user service endpoint");

        Assert.Contains("add the user service endpoint", translated);
        Assert.Contains("Generated capability", translated);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_ReturnsTranslatedResultWhenTopologyIsAvailable()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase("4.1", "Backend Services", "Owns backend service implementation.", ["IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flow = new GeneratedCapabilityFlow(
            "4",
            "4.1",
            "Backend Services",
            ["software-development"],
            ["IMPLEMENT"],
            Domain.CODE_Q.ToString(),
            new FixedLogicalUnit("Generated capability for sub-phase 4.1: Backend Services."),
            topology,
            "implement the backend service");

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
            "translated-flow-run",
            "generated-capability-flow-4-1",
            "generated-capability-4-1",
            "implement the backend service",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("PASS", result.Value!.OverallVerdict);
        Assert.Contains("Backend Services", result.Value.FinalHumanResult);
        Assert.Contains("implement the backend service", result.Value.FinalHumanResult);
        Assert.DoesNotContain("Generated capability", result.Value.FinalHumanResult);
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_ModelConfigUsesFourLayersHiddenThirtyTwo()
    {
        var config = InvokeBuildTinyConfig();

        Assert.Equal(512, config.VocabSize);
        Assert.Equal(64, config.ContextLen);
        Assert.Equal(32, config.HiddenDim);
        Assert.Equal(4, config.NumLayers);
        Assert.Equal(4, config.NumHeads);
        Assert.Equal(2, config.NumKvHeads);
        Assert.Equal(64, config.FfnHiddenDim);
    }

    [Fact]
    public void DeeperModel_CompletesOneDpoStep()
    {
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"deeper-dpo-{Guid.NewGuid():N}");
        try
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train("SECURITY AUTHORIZATION IMPLEMENT chosen rejected prompt", 300);
            var chosen = tokenizer.Encode("SECURITY AUTHORIZATION IMPLEMENT chosen");
            var rejected = tokenizer.Encode("SECURITY AUTHORIZATION IMPLEMENT rejected");
            var batch = new DpoBatch
            {
                ChosenIds = ToTensor(chosen, 16),
                ChosenMask = ToMask(chosen.Count, 16),
                RejectedIds = ToTensor(rejected, 16),
                RejectedMask = ToMask(rejected.Count, 16)
            };
            var trainer = new DpoTrainer(new LlmModel(InvokeBuildTinyConfig()), new LlmModel(InvokeBuildTinyConfig()), checkpointDir);

            var metrics = trainer.Step(batch, 1);

            Assert.Equal(1, metrics.Step);
            Assert.False(float.IsNaN(metrics.Loss));
            Assert.False(float.IsInfinity(metrics.Loss));
            Assert.True(metrics.Accuracy is 0f or 1f);
            Assert.True(File.Exists(Path.Combine(checkpointDir, "dpo_step_000001.json")));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public void FourLayerModel_LoraApplicatorCreatesEightAdapters()
    {
        var model = new LlmModel(InvokeBuildTinyConfig());

        var adapters = LoraApplicator.Apply(model, new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] });

        Assert.Equal(8, adapters.Count);
        Assert.All(model.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));
        Assert.All(adapters.SelectMany(adapter => adapter.Parameters()), parameter =>
            Assert.All(parameter.Value.Data, value => Assert.False(float.IsNaN(value))));
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_StoresOriginalPromptInManifest()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model.", ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"original-prompt-manifest-{Guid.NewGuid():N}");

        try
        {
            var trained = await new DefaultLogicalCapabilityTrainer(checkpointRoot).TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "original-prompt-manifest-run",
                new LogicalOrchestratorRequest(
                    "original-prompt-manifest-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "add auth guard",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [BuildKnowledgeEdgeBranch("3.4")],
                topology,
                new InMemoryLogicalFlowRegistry(),
                new InMemoryLogicalUnitRegistry())
            {
                Domain = Domain.ARCH
            });

            Assert.True(trained.IsSuccess, trained.ErrorMessage);
            var manifest = Directory.GetFiles(checkpointRoot, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName, SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .Select(json => System.Text.Json.JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(json))
                .Single(item => item is not null)!;
            Assert.Equal("add auth guard", manifest.OriginalPrompt);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task FullRoundTrip_UsesCompiledInputDeeperModelAndBackTranslation()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model, authentication, authorization, and tenant isolation.", ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"full-roundtrip-deeper-{Guid.NewGuid():N}");

        try
        {
            var trained = await new DefaultLogicalCapabilityTrainer(checkpointRoot).TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "full-roundtrip-deeper-run",
                new LogicalOrchestratorRequest(
                    "full-roundtrip-deeper-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "implement the authorization guard for the tenant controller",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [BuildKnowledgeEdgeBranch("3.4")],
                topology,
                flows,
                units)
            {
                Domain = Domain.ARCH
            });
            Assert.True(trained.IsSuccess, trained.ErrorMessage);

            var binding = flows.ResolveBindingBySubPhase("3.4", new LogicalFlowFactoryRequest(
                "full-roundtrip-deeper-run",
                new LogicalExecutionFlags(),
                units,
                new Dictionary<string, object>(),
                topology));
            Assert.True(binding.IsSuccess, binding.ErrorMessage);

            var result = await binding.Value!.Flow.ExecuteAsync(new LogicalFlowRequest<string>(
                "full-roundtrip-deeper-run",
                binding.Value.FlowId,
                binding.Value.ScenarioId,
                "implement the authorization guard for the tenant controller",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("PASS", result.Value!.OverallVerdict);
            Assert.Equal("3.4", result.Value.Metrics["sub_phase_id"]);
            Assert.Contains("Security and Authorization Model", result.Value.FinalHumanResult);
            Assert.Contains("implement the authorization guard", result.Value.FinalHumanResult);
            Assert.DoesNotContain("Generated capability for sub-phase", result.Value.FinalHumanResult);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static ModelConfig InvokeBuildTinyConfig()
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "BuildTinyConfig",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        return (ModelConfig)method.Invoke(null, null)!;
    }

    private static Tensor ToTensor(IReadOnlyList<int> ids, int length)
    {
        var data = new float[length];
        for (var i = 0; i < length; i++)
            data[i] = i < ids.Count ? ids[i] : 0;
        return new Tensor(data, new TensorShape(1, length));
    }

    private static Tensor ToMask(int count, int length)
    {
        var data = new float[length];
        for (var i = 0; i < length; i++)
            data[i] = i < count ? 1f : 0f;
        return new Tensor(data, new TensorShape(1, length));
    }

    private static LogicalSubPhase BuildSubPhase(string subPhaseId, string name, string purpose, string[] predicates)
    {
        return new LogicalSubPhase(
            subPhaseId,
            subPhaseId.Split('.')[0],
            name,
            purpose,
            ["software-development"],
            predicates,
            ["back-translation-test"]);
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(string subPhaseId)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{subPhaseId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            $"missing_sub_phase={subPhaseId}",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        return new LogicalFlowResult(
            $"missing-flow-{subPhaseId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = subPhaseId },
            [],
            []);
    }

    private sealed class FixedLogicalUnit : ILogicalUnit<string, string>
    {
        private readonly string _output;

        public FixedLogicalUnit(string output)
        {
            _output = output;
        }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(new LogicalUnitResult<string>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                _output,
                0.99,
                ["IMPLEMENT"],
                [request.DomainTag],
                ["fixed-unit"],
                [])));
        }
    }
}
