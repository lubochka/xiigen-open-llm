// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompiler;
using Domain = SemanticCompiler29.Domain;

public sealed class DefaultLogicalCapabilityTrainerTests
{
    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RegistersProbationaryLearnedUnitFlowAndRetryUsesIt()
    {
        // Build a topology that can route implementation work but intentionally has no implementation flow.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation, provider connectors, integration code, and application service changes.",
            ["backend-services"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        // Keep checkpoint artifacts outside the repository and remove them after the test.
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-trainer-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();

        try
        {
            // Build the adaptive orchestrator with the default trainer.
            var orchestrator = new AdaptiveLogicalOrchestrator(
                topology,
                new PromptRoutingInputCompilerUnit(),
                new SubPhaseRouterUnit(topology),
                flows,
                units,
                capabilityTrainer: new DefaultLogicalCapabilityTrainer(checkpointRoot));

            // The first pass should hit a knowledge edge, train, register, and then retry through the generated flow.
            var result = await orchestrator.RunWithCapabilityGenerationAsync(new LogicalOrchestratorRequest(
                "default-capability-training-run",
                AdaptiveLogicalOrchestrator.ScenarioId,
                "Implement the backend service and provider connector for the integration layer.",
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            // The adaptive run must succeed after retry.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // The retry must use the generated learned capability flow rather than returning the missing-flow knowledge edge.
            Assert.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, result.Value!.SelectedFlowId);
            Assert.Equal("PASS", result.Value.OverallVerdict);
            Assert.Equal("True", result.Value.FlowResult.Metrics["generated_capability_flow"]);
            Assert.Equal("True", result.Value.FlowResult.Metrics["learned_unit_selected"]);

            // The trained unit must be registered as learned and probationary.
            var learnedDescriptor = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == Domain.PLAN.ToString());
            Assert.True(learnedDescriptor.IsLearned);
            Assert.Equal(LogicalUnitTrustState.Probationary, learnedDescriptor.TrustState);
            Assert.Equal(typeof(string), learnedDescriptor.InputType);
            Assert.Equal(typeof(string), learnedDescriptor.OutputType);

            // The flow registry must now contain a retryable generated flow for the missing sub-phase.
            Assert.Contains(flows.ListDescriptors(), descriptor =>
                descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal) &&
                descriptor.SubPhaseId == "4.1");

            // The DPO teaching path must have written at least one metric file to the configured checkpoint root.
            Assert.True(Directory.Exists(checkpointRoot));
            Assert.NotEmpty(Directory.GetFiles(checkpointRoot, "dpo_step_*.json", SearchOption.AllDirectories));
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_RepeatedSubPhaseTeachingContinuesFromActiveCheckpoint()
    {
        // Build a topology with one repeat-taught implementation sub-phase.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-continuation-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

        try
        {
            // Teach the first row for 4.4.
            var first = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-continuation-row-0001",
                "Fix a runtime code quality issue in the Figma export path.",
                topology,
                flows,
                units));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(first.Value!.CapabilityRegistered);

            var firstSnapshot = TeachingRegistryPersistence.Capture("continuation-test", 1, units, flows, checkpointRoot);
            Assert.True(firstSnapshot.IsSuccess, firstSnapshot.ErrorMessage);
            var firstManifest = Assert.Single(firstSnapshot.Value!.GeneratedCapabilities, manifest => manifest.SubPhaseId == "4.4");
            Assert.Null(firstManifest.ParentUnitId);
            Assert.Null(firstManifest.ParentPolicyCheckpointPath);
            Assert.Equal(1, firstManifest.TrainingStepCount);

            // Remove parent artifacts to prove the live trainer uses its warm in-memory stack
            // instead of reading the same parent checkpoint back from disk during this bundle session.
            File.Delete(firstManifest.PolicyCheckpointPath);
            File.Delete(firstManifest.TokenizerPath);

            // Teach a second row for the same sub-phase with the same live registry and trainer.
            var second = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-continuation-row-0002",
                "Fix another Figma export error and preserve the previous runtime cleanup behavior.",
                topology,
                flows,
                units));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(second.Value!.CapabilityRegistered);
            Assert.Contains("semantic_compression=enabled", second.Value.Notes);
            Assert.Contains("replay_count=", second.Value.Notes);
            Assert.Contains("d09_max_weight=", second.Value.Notes);

            var secondSnapshot = TeachingRegistryPersistence.Capture("continuation-test", 2, units, flows, checkpointRoot);
            Assert.True(secondSnapshot.IsSuccess, secondSnapshot.ErrorMessage);
            var manifests = secondSnapshot.Value!.GeneratedCapabilities
                .Where(manifest => manifest.SubPhaseId == "4.4")
                .OrderBy(manifest => manifest.CreatedAtUtc)
                .ToArray();
            Assert.Equal(2, manifests.Length);

            var continuedManifest = manifests[^1];
            Assert.Equal(firstManifest.UnitId, continuedManifest.ParentUnitId);
            Assert.Equal(firstManifest.PolicyCheckpointPath, continuedManifest.ParentPolicyCheckpointPath);
            Assert.Equal(firstManifest.TokenizerPath, continuedManifest.ParentTokenizerPath);
            Assert.Equal(2, continuedManifest.TrainingStepCount);
            Assert.Equal("1.0.1", continuedManifest.Version);
            Assert.Contains("semantic_compressed:", continuedManifest.TrainingText);
            Assert.NotEqual(firstManifest.PolicyCheckpointPath, continuedManifest.PolicyCheckpointPath);
            Assert.True(File.Exists(continuedManifest.PolicyCheckpointPath));
            Assert.True(File.Exists(continuedManifest.TokenizerPath));

            // Runtime selection still uses the newest non-rejected version.
            var latest = units.ListDescriptors()
                .Where(descriptor => descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole && descriptor.DomainTag == Domain.BUG.ToString())
                .OrderByDescending(descriptor => descriptor.RegistrationOrder)
                .First();
            Assert.Equal(continuedManifest.UnitId, latest.UnitId);
            Assert.Equal("1.0.1", latest.Version);
        }
        finally
        {
            // Clean test artifacts without touching repository files.
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_SameDomainBranchesShareAdapterFamily()
    {
        // Build two sub-phases that should teach the same BUG domain adapter while keeping separate flow routes.
        var topology = new InMemoryLogicalTopologyRegistry();
        var codeQuality = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(codeQuality.IsSuccess, codeQuality.ErrorMessage);
        var unitTests = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "5.1",
            "5",
            "Unit Test Design",
            "Owns unit test design for runtime failures.",
            ["quality-assurance"],
            ["TEST"],
            ["default-capability-trainer-test"]));
        Assert.True(unitTests.IsSuccess, unitTests.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-domain-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

        try
        {
            var request = new LogicalCapabilityTrainingRequest(
                "capability-domain-row-0001",
                new LogicalOrchestratorRequest(
                    "capability-domain-row-0001",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "Fix the Figma export error and add a regression test.",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [BuildKnowledgeEdgeBranch("4.4"), BuildKnowledgeEdgeBranch("5.1")],
                topology,
                flows,
                units)
            {
                Domain = Domain.BUG
            };

            var result = await trainer.TrainAndRegisterAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.CapabilityRegistered);
            Assert.Equal(Domain.BUG.ToString(), result.Value.Domain);
            Assert.Equal(2, result.Value.BatchedBranchCount);
            Assert.True(result.Value.BatchedDpoStepCount >= 1);
            Assert.Equal([1, 1], result.Value.BranchTimings.Select(timing => timing.TrainingStepCount).ToArray());
            Assert.All(result.Value.BranchTimings, timing => Assert.True(timing.DpoMs > 0));
            Assert.All(
                units.ListDescriptors().Where(descriptor => descriptor.UnitRole == DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole),
                descriptor => Assert.Equal(Domain.BUG.ToString(), descriptor.DomainTag));
            var domainClassifierDescriptor = Assert.Single(units.ListDescriptors(), descriptor =>
                descriptor.UnitRole == SemanticCompilerAdapter.DomainClassifierUnit.Role &&
                descriptor.DomainTag == SemanticCompilerAdapter.DomainClassifierUnit.DomainTag);
            Assert.True(domainClassifierDescriptor.IsLearned);
            Assert.Equal(LogicalUnitTrustState.Probationary, domainClassifierDescriptor.TrustState);
            Assert.Contains(flows.ListDescriptors(), descriptor => descriptor.SubPhaseId == "4.4");
            Assert.Contains(flows.ListDescriptors(), descriptor => descriptor.SubPhaseId == "5.1");

            var snapshot = TeachingRegistryPersistence.Capture("domain-family-test", 1, units, flows, checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var manifests = snapshot.Value!.GeneratedCapabilities.OrderBy(manifest => manifest.CreatedAtUtc).ToArray();
            Assert.Equal(2, manifests.Length);
            Assert.All(manifests, manifest => Assert.Equal(Domain.BUG.ToString(), manifest.DomainTag));
            Assert.All(manifests, manifest => Assert.Equal("generated-capability-bug", manifest.UnitFamilyId));
            Assert.All(manifests, manifest => Assert.Null(manifest.ParentUnitId));
            Assert.All(manifests, manifest => Assert.Equal(1, manifest.TrainingStepCount));
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_IdleGateDefersTrainingWhenMachineIsBusy()
    {
        // Build a topology with one missing capability.
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);

        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-idle-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot, new BusyIdleGate());

        try
        {
            // Run training while the injected D-14 idle gate says the machine is busy.
            var result = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "capability-idle-row-0001",
                "Fix a runtime code quality issue.",
                topology,
                flows,
                units));

            // The teaching pass should defer cleanly, not train synchronously.
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(result.Value!.CapabilityRegistered);
            Assert.Empty(result.Value.RegisteredUnitIds);
            Assert.Empty(result.Value.RegisteredFlowIds);
            Assert.Contains("idle-learning gate", result.Value.Notes);
            Assert.Empty(units.ListDescriptors());
            Assert.Empty(flows.ListDescriptors());
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_ManifestCacheAvoidsRepeatedDiskReads()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-manifest-cache-{Guid.NewGuid():N}");
        var manifestDirectory = Path.Combine(checkpointRoot, "run-0001", "BUG", "4.4");
        Directory.CreateDirectory(manifestDirectory);
        var manifestPath = Path.Combine(manifestDirectory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);
        var manifest = BuildManifest("unit-cache-0001", Domain.BUG.ToString(), "4.4", DateTimeOffset.UtcNow);
        File.WriteAllText(manifestPath, System.Text.Json.JsonSerializer.Serialize(manifest));
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

        try
        {
            var first = InvokeLoadGeneratedCapabilityManifests(trainer);
            Assert.Single(first);

            File.Delete(manifestPath);
            var second = InvokeLoadGeneratedCapabilityManifests(trainer);

            Assert.Single(second);
            Assert.Equal("unit-cache-0001", second[0].UnitId);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest(
        string runId,
        string userRequest,
        InMemoryLogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{runId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            "missing_sub_phase=4.4",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        var branch = new LogicalFlowResult(
            $"missing-flow-{runId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = "4.4" },
            [],
            []);

        return new LogicalCapabilityTrainingRequest(
            runId,
            new LogicalOrchestratorRequest(
                runId,
                AdaptiveLogicalOrchestrator.ScenarioId,
                userRequest,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [branch],
            topology,
            flows,
            units)
        {
            Domain = PromptDomainClassifier.Classify(userRequest).Domain
        };
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

    private static GeneratedCapabilityCheckpointManifest[] InvokeLoadGeneratedCapabilityManifests(DefaultLogicalCapabilityTrainer trainer)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "LoadGeneratedCapabilityManifests",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return (GeneratedCapabilityCheckpointManifest[])method.Invoke(trainer, null)!;
    }

    private static GeneratedCapabilityCheckpointManifest BuildManifest(
        string unitId,
        string domainTag,
        string subPhaseId,
        DateTimeOffset createdAtUtc)
        => new(
            unitId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            domainTag,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary.ToString(),
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix}-{subPhaseId}",
            $"{DefaultLogicalCapabilityTrainer.GeneratedCapabilityScenarioIdPrefix}-{subPhaseId}",
            subPhaseId.Split('.')[0],
            subPhaseId,
            "test-task",
            "training text",
            "output text",
            "example",
            ["IMPLEMENT"],
            ["test-domain"],
            "policy.bin",
            "tokenizer.json",
            512,
            128,
            16,
            1,
            1,
            1,
            32,
            2,
            4f,
            0.05f,
            ["q_proj", "v_proj"],
            createdAtUtc,
            $"generated-capability-{domainTag.ToLowerInvariant()}");

    private sealed class BusyIdleGate : ILogicalTeachingIdleGate
    {
        public AgentInfrastructure.OperationResult<bool> CanTrainNow(LogicalCapabilityTrainingRequest request)
        {
            return AgentInfrastructure.OperationResult<bool>.Ok(false);
        }
    }
}
