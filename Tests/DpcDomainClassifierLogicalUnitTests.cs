// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

public sealed class DpcDomainClassifierLogicalUnitTests
{
    [Fact]
    public async Task CompiledDomainClassifierUnit_ResolvesFromRegistry()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var classifier = new DomainClassifierUnit();
        classifier.Train([([0, 1, 2], Domain.BUG)]);
        var registered = units.Register<DomainClassifierRequest, DomainClassification>(
            "compiled-domain-classifier-test",
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            new CompiledDomainClassifierUnit(classifier));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var resolved = units.ResolveLatest<DomainClassifierRequest, DomainClassification>(
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var result = await resolved.Value!.ExecuteAsync(new LogicalUnitRequest<DomainClassifierRequest>(
            "compiled-domain-classifier-run",
            CompiledDomainClassifierUnit.Id,
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag,
            new DomainClassifierRequest("compiled predicate profile", [0, 1, 2]),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(Domain.BUG, result.Value!.Output.Label);
        Assert.Contains("trained_predicate_knn", result.Value.Output.Evidence);
    }

    [Fact]
    public void DomainClassifierUnit_ImplementsLogicalUnitDirectlyFromProject23()
    {
        var unit = new DomainClassifierUnit();

        Assert.IsAssignableFrom<ILogicalUnit<DomainClassifierRequest, DomainClassification>>(unit);
    }

    [Fact]
    public void DomainClassifierUnit_TrainedPredicatePathOverridesKeywordFallback()
    {
        const string prompt = "Fix the auth crash in the authorization endpoint";
        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        var profile = DomainClassifierUnit.EncodePredicateProfile(
            compiled.Value!.Compilation.DevelopmentPredicateIds.Concat(compiled.Value.Compilation.PipelinePredicates));
        var classifier = new DomainClassifierUnit();
        classifier.Train([(profile, Domain.ARCH)]);

        var classified = classifier.Classify(new DomainClassifierRequest(compiled.Value.CanonicalText, profile));

        Assert.Equal(Domain.ARCH, classified.Label);
        Assert.Contains("compiled_predicate_profile", classified.Evidence);
    }

    [Fact]
    public void PromptDomainClassifier_CompatibilityWrapperUsesCompiledMetadataWhenPresent()
    {
        var metadata = PromptDomainClassifier.AddCompiledPromptToMetadata(
            new Dictionary<string, object>(),
            new SubPhaseRouterSourceDocument(
                "compiled-domain-doc",
                "memory://compiled",
                "runtime_prompt",
                "user_request",
                "prompt",
                "Compiled prompt",
                "SECURITY AUTHORIZATION semantic_domain:ARCH semantic_stack:typescript",
                ["SECURITY", "AUTHORIZATION"],
                "ready",
                "compiled",
                Domain.ARCH.ToString()));

        var classified = PromptDomainClassifier.Classify("fix crash", metadata);

        Assert.Equal(Domain.ARCH, classified.Domain);
        Assert.Contains("trained_predicate_knn", classified.EvidenceTerms);
    }

    [Fact]
    public async Task AttributedSegmentRouterUnit_UsesCompiledSegmentsOnly()
    {
        var compiled = new SemanticCompiler29.DevelopmentPromptCompilation(
            "fix auth, update docs",
            Domain.BUG,
            "generic",
            [],
            ["IMPLEMENT", "DOCUMENT"],
            "fix auth update docs",
            1.0,
            [
                new SemanticCompiler29.DevelopmentPromptSegment("fix auth", Domain.ARCH, [], ["SECURITY", "AUTHORIZATION"], 1.0),
                new SemanticCompiler29.DevelopmentPromptSegment("update docs", Domain.PLAN, [], ["DOCUMENT"], 1.0)
            ]);
        var unit = new AttributedSegmentRouterUnit();

        var result = await unit.ExecuteAsync(new LogicalUnitRequest<SemanticCompiler29.DevelopmentPromptCompilation>(
            "attributed-router-run",
            AttributedSegmentRouterUnit.Id,
            AttributedSegmentRouterUnit.Role,
            AttributedSegmentRouterUnit.DomainTag,
            compiled,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.Output.Length);
        Assert.All(result.Value.Output, segment => Assert.DoesNotContain("fix auth, update docs", segment.Text));
        Assert.Equal(Domain.ARCH, result.Value.Output[0].Domain);
        Assert.Equal(Domain.PLAN, result.Value.Output[1].Domain);
    }

    [Fact]
    public void InternalLlmGateway_UsesCompiledPredicateClassifierBeforeKeywordFallback()
    {
        const string prompt = "Fix the auth crash in the authorization endpoint";
        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        var profile = DomainClassifierUnit.EncodePredicateProfile(
            compiled.Value!.Compilation.DevelopmentPredicateIds.Concat(compiled.Value.Compilation.PipelinePredicates));
        var classifier = new DomainClassifierUnit();
        classifier.Train([(profile, Domain.ARCH)]);
        var gateway = new InternalLlmGateway(new FixedLlmClient(), new FixedDomainModelRegistry(), classifier);

        var routing = gateway.InspectRouting(prompt);

        Assert.True(routing.IsSuccess, routing.ErrorMessage);
        Assert.Equal(InternalModelDomain.Arch, routing.Value!.Domain);
        Assert.Equal("arch.lora", routing.Value.AdapterPath);
    }

    [Fact]
    public void InternalLlmGateway_UsesCompiledDomainClassifierFromRegistry()
    {
        const string prompt = "Fix the auth crash in the authorization endpoint";
        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        var profile = DomainClassifierUnit.EncodePredicateProfile(
            compiled.Value!.Compilation.DevelopmentPredicateIds.Concat(compiled.Value.Compilation.PipelinePredicates));
        var classifier = new DomainClassifierUnit();
        classifier.Train([(profile, Domain.PLAN)]);
        var units = new InMemoryLogicalUnitRegistry();
        var registered = units.Register<DomainClassifierRequest, DomainClassification>(
            "gateway-registry-domain-classifier",
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            new CompiledDomainClassifierUnit(classifier));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var gateway = new InternalLlmGateway(new FixedLlmClient(), new FixedDomainModelRegistry(), logicalUnits: units);

        var routing = gateway.InspectRouting(prompt);

        Assert.True(routing.IsSuccess, routing.ErrorMessage);
        Assert.Equal(InternalModelDomain.Plan, routing.Value!.Domain);
        Assert.Equal("plan.lora", routing.Value.AdapterPath);
    }

    [Fact]
    public void DomainClassifierTraining_PersistsExamplesAcrossTrainerInstances()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"domain-classifier-training-{Guid.NewGuid():N}");
        try
        {
            var firstUnits = new InMemoryLogicalUnitRegistry();
            var firstTrainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
            var firstCompiled = new SemanticTeachingPromptCompiler().Compile("fix the null auth crash");
            Assert.True(firstCompiled.IsSuccess, firstCompiled.ErrorMessage);
            var first = InvokeRegisterTrainedDomainClassifier(
                firstTrainer,
                BuildTrainingRequest("domain-classifier-persist-1", "fix the null auth crash", firstUnits, Domain.BUG),
                firstCompiled.Value!);
            Assert.True(first.IsSuccess, first.ErrorMessage);

            var secondUnits = new InMemoryLogicalUnitRegistry();
            var secondTrainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
            var secondCompiled = new SemanticTeachingPromptCompiler().Compile("design authorization architecture");
            Assert.True(secondCompiled.IsSuccess, secondCompiled.ErrorMessage);
            var second = InvokeRegisterTrainedDomainClassifier(
                secondTrainer,
                BuildTrainingRequest("domain-classifier-persist-2", "design authorization architecture", secondUnits, Domain.ARCH),
                secondCompiled.Value!);
            Assert.True(second.IsSuccess, second.ErrorMessage);

            var resolved = secondUnits.ResolveLatest<DomainClassifierRequest, DomainClassification>(
                CompiledDomainClassifierUnit.Role,
                CompiledDomainClassifierUnit.DomainTag);
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            var firstProfile = DomainClassifierUnit.EncodePredicateProfile(
                firstCompiled.Value.Compilation.DevelopmentPredicateIds.Concat(firstCompiled.Value.Compilation.PipelinePredicates));
            var classified = Assert.IsType<CompiledDomainClassifierUnit>(resolved.Value).Classify(
                new DomainClassifierRequest(firstCompiled.Value.CanonicalText, firstProfile));
            Assert.Equal(Domain.BUG, classified.Label);
            Assert.Contains("trained_predicate_knn", classified.Evidence);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void TransformerKnnManifest_RecordsSemanticRootsAndPatternFusion()
    {
        var model = new TransformerModel.LlmModel(new TransformerModel.Config.ModelConfig
        {
            VocabSize = 512,
            ContextLen = 32,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        });
        var tokenizer = new Tokenizer.BpeTokenizer();
        tokenizer.Train("fix auth crash implement backend service", 300);

        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "knn-semantic-fusion-test",
            "generated-capability",
            Domain.BUG.ToString(),
            model,
            tokenizer,
            [
                new TransformerKnnTrainingExample<string>(
                    "example-1",
                    "Fix the auth crash in backend service",
                    "bug output",
                    ["FAIL", "AUTHORIZATION"],
                    [Domain.BUG.ToString()])
            ]);

        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        Assert.Contains("E15:SEMANTIC_ROOT_TOKENIZATION", trained.Value!.Manifest.IngredientIds);
        Assert.Contains("E25:PATTERN_CLASSIFIER_FUSION", trained.Value.Manifest.IngredientIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", trained.Value.Manifest.IngredientIds);
    }

    private sealed class FixedDomainModelRegistry : IDomainModelRegistry
    {
        public OperationResult<string> ResolveAdapter(InternalModelDomain domain)
            => OperationResult<string>.Ok(domain switch
            {
                InternalModelDomain.Arch => "arch.lora",
                InternalModelDomain.Generic => "generic.lora",
                _ => $"{domain.ToString().ToLowerInvariant()}.lora"
            });
    }

    private sealed class FixedLlmClient : ILlmClient
    {
        public Task<OperationResult<LlmResponse>> SendAsync(
            IReadOnlyList<ConversationMessage> messageHistory,
            IReadOnlyList<ToolDefinition>? availableTools,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(OperationResult<LlmResponse>.Ok(new LlmResponse("ok", null, null)));
        }
    }

    private static OperationResult<LogicalUnitDescriptor> InvokeRegisterTrainedDomainClassifier(
        DefaultLogicalCapabilityTrainer trainer,
        LogicalCapabilityTrainingRequest request,
        SemanticTeachingPromptRepresentation compiledPrompt)
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod(
            "RegisterTrainedDomainClassifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        return Assert.IsType<OperationResult<LogicalUnitDescriptor>>(method.Invoke(trainer, [request, compiledPrompt, compiledPrompt.CanonicalText]));
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest(
        string runId,
        string userRequest,
        InMemoryLogicalUnitRegistry units,
        Domain domain)
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        return new LogicalCapabilityTrainingRequest(
            runId,
            new LogicalOrchestratorRequest(
                runId,
                AdaptiveLogicalOrchestrator.ScenarioId,
                userRequest,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [],
            topology,
            flows,
            units)
        {
            Domain = domain
        };
    }
}
