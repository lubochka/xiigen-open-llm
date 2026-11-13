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
    public void CapabilityResultTranslator_PreservesLearnedOutputAndSubPhaseMetadata()
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
        Assert.Contains("Generated capability for sub-phase 3.4: Security and Authorization Model.", translated);
        Assert.DoesNotContain($"{Environment.NewLine}{Environment.NewLine}", translated);
    }

    [Fact]
    public void CapabilityResultTranslator_PreservesPromptMetadataAndModelFormattingWithoutAddingPrompt()
    {
        var prompt = "fix the auth check in the tenant service";
        var promptLine = $"For your request: \"{prompt}\"";
        var authoritativeOutput =
            $"{promptLine}{Environment.NewLine}{Environment.NewLine}Generated capability for sub-phase 3.4: Security and Authorization Model.";
        var subPhase = BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model.", ["SECURITY"]);

        var translated = CapabilityResultTranslator.Translate(
            authoritativeOutput,
            subPhase,
            prompt);

        Assert.StartsWith(authoritativeOutput, translated, StringComparison.Ordinal);
        Assert.Contains(promptLine, translated);
        Assert.Equal(
            translated.IndexOf(promptLine, StringComparison.Ordinal),
            translated.LastIndexOf(promptLine, StringComparison.Ordinal));
        Assert.Contains("Generated capability for sub-phase 3.4: Security and Authorization Model.", translated);
        Assert.Contains("Sub-phase 3.4 (Security and Authorization Model)", translated);
        Assert.Contains("Scope: Owns security model.", translated);
        Assert.Contains("Focus areas: SECURITY", translated);
    }

    [Theory]
    [InlineData("Prompt: a different tenant request")]
    [InlineData("Request: a different tenant request")]
    public void CapabilityResultTranslator_DoesNotAddCallerPromptWhenOutputContainsPromptMetadata(
        string learnedPromptLine)
    {
        const string callerPrompt = "fix the auth check in the tenant service";
        const string canonicalCallerPromptLine = "For your request: \"fix the auth check in the tenant service\"";
        var subPhase = BuildSubPhase(
            "3.4",
            "Security and Authorization Model",
            "Owns security model.",
            ["SECURITY"]);

        var translated = CapabilityResultTranslator.Translate(
            $"{learnedPromptLine}{Environment.NewLine}Generated capability for sub-phase 3.4.",
            subPhase,
            callerPrompt);

        Assert.Contains(learnedPromptLine, translated, StringComparison.Ordinal);
        Assert.Equal(0, CountOccurrences(translated, canonicalCallerPromptLine));
        Assert.Contains("Generated capability for sub-phase 3.4.", translated, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Prompt: \"  FIX   the AUTH check in the tenant service  \"")]
    [InlineData("Request: 'fix the auth check in the tenant service'")]
    [InlineData("For your request: fix the auth check in the tenant service")]
    public void CapabilityResultTranslator_DeduplicatesEquivalentPromptMetadataAfterSafeNormalization(
        string learnedPromptLine)
    {
        const string callerPrompt = "fix the auth check in the tenant service";
        const string canonicalCallerPromptLine = "For your request: \"fix the auth check in the tenant service\"";
        var subPhase = BuildSubPhase(
            "3.4",
            "Security and Authorization Model",
            "Owns security model.",
            ["SECURITY"]);

        var translated = CapabilityResultTranslator.Translate(
            $"{learnedPromptLine}{Environment.NewLine}Generated capability for sub-phase 3.4.",
            subPhase,
            callerPrompt);

        Assert.Contains(learnedPromptLine, translated, StringComparison.Ordinal);
        Assert.Equal(0, CountOccurrences(translated, canonicalCallerPromptLine));
        Assert.Contains("Generated capability for sub-phase 3.4.", translated, StringComparison.Ordinal);
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CapabilityResultTranslator_DirectEmptyOutputKeepsDeterministicFallback(
        string? emptyOutput)
    {
        var subPhase = BuildSubPhase(
            "4.1",
            "Backend Services",
            "Owns backend service implementation.",
            ["IMPLEMENT"]);

        var translated = CapabilityResultTranslator.Translate(
            emptyOutput!,
            subPhase,
            "implement the backend service");
        var fallback = CapabilityResultTranslator.TranslateFromKnnOutput(
            emptyOutput!,
            "implement the backend service");

        Assert.Contains("For your request: \"implement the backend service\"", translated, StringComparison.Ordinal);
        Assert.Contains("Sub-phase 4.1 (Backend Services)", translated, StringComparison.Ordinal);
        Assert.Equal("Capability identified for: \"implement the backend service\"", fallback);
    }

    [Theory]
    [InlineData("Prompt: a different request")]
    [InlineData("Request: a different request")]
    public void CapabilityResultTranslator_FallbackPreservesRecognizedPromptMetadataWithoutAddingCallerPrompt(
        string authoritativeOutput)
    {
        var translated = CapabilityResultTranslator.TranslateFromKnnOutput(
            authoritativeOutput,
            "implement the backend service");

        Assert.Equal(authoritativeOutput, translated);
        Assert.DoesNotContain("For your request:", translated, StringComparison.Ordinal);
    }

    [Fact]
    public void CapabilityResultTranslator_RawPromptSuppressionUsesTokenBoundaries()
    {
        const string authoritativeOutput = "concatenate the values";
        const string originalPrompt = "cat";
        var subPhase = BuildSubPhase(
            "4.1",
            "Backend Services",
            "Owns backend service implementation.",
            ["IMPLEMENT"]);

        var translated = CapabilityResultTranslator.Translate(
            authoritativeOutput,
            subPhase,
            originalPrompt);
        var fallback = CapabilityResultTranslator.TranslateFromKnnOutput(
            authoritativeOutput,
            originalPrompt);

        Assert.Contains("For your request: \"cat\"", translated, StringComparison.Ordinal);
        Assert.Contains("For your request: \"cat\"", fallback, StringComparison.Ordinal);
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
        Assert.Contains("Generated capability for sub-phase 4.1: Backend Services.", result.Value.FinalHumanResult);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_PreservesAuthoritativeUnitOutputBeforeDeterministicMetadata()
    {
        const string authoritativeOutput = "MODEL_OUTPUT: use the tenant-scoped authorization guard.";
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flow = CreateGeneratedCapabilityFlow(new FixedLogicalUnit(authoritativeOutput), topology);

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var humanResult = result.Value!.FinalHumanResult;
        var learnedOutputIndex = humanResult.IndexOf(authoritativeOutput, StringComparison.Ordinal);
        var deterministicMetadataIndex = humanResult.IndexOf(
            "Sub-phase 4.1 (Backend Services)",
            StringComparison.Ordinal);
        Assert.True(learnedOutputIndex >= 0);
        Assert.True(deterministicMetadataIndex > learnedOutputIndex);
        Assert.Equal(1, CountOccurrences(humanResult, authoritativeOutput));
    }

    [Fact]
    public void CapabilityResultTranslator_DoesNotDuplicateAuthoritativeSubPhaseLine()
    {
        const string authoritativeOutput = "Sub-phase 4.1 (Backend Services)";
        var subPhase = BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]);

        var translated = CapabilityResultTranslator.Translate(
            authoritativeOutput,
            subPhase,
            "implement the backend service");

        Assert.StartsWith(authoritativeOutput, translated, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(translated, authoritativeOutput));
        Assert.Contains("Scope: Deterministic topology metadata.", translated, StringComparison.Ordinal);
    }

    [Fact]
    public void CapabilityResultTranslator_PreservesMultilineOutputVerbatimBeforeAnnotations()
    {
        const string authoritativeOutput = "\r\n  first model line  \r\n\r\nsecond model line\r\n";
        var subPhase = BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]);

        var translated = CapabilityResultTranslator.Translate(
            authoritativeOutput,
            subPhase,
            "implement the backend service");

        Assert.StartsWith(authoritativeOutput, translated, StringComparison.Ordinal);
        Assert.Equal(
            authoritativeOutput.Length,
            translated.IndexOf("For your request:", StringComparison.Ordinal));
        Assert.Contains("Sub-phase 4.1 (Backend Services)", translated, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_OutputEqualToCallerPromptAppearsOnceWithTopology()
    {
        const string prompt = "implement the backend service";
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flow = CreateGeneratedCapabilityFlow(new FixedLogicalUnit(prompt), topology);

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith(prompt, result.Value!.FinalHumanResult, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(result.Value.FinalHumanResult, prompt));
        Assert.DoesNotContain("For your request:", result.Value.FinalHumanResult, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_OutputEqualToCallerPromptAppearsOnceWithoutTopology()
    {
        const string prompt = "implement the backend service";
        var flow = CreateGeneratedCapabilityFlow(new FixedLogicalUnit(prompt));

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith(
            prompt,
            result.Value!.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Contains(
            "Sub-phase 4.1 (Backend Services)",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Contains(
            "Focus areas: IMPLEMENT",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task GeneratedCapabilityFlow_MultilineOutputEqualToCallerPromptAppearsOnceWithTopology(
        string lineEnding)
    {
        var prompt = $"implement the{lineEnding}backend service";
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flow = CreateGeneratedCapabilityFlow(
            new FixedLogicalUnit(prompt),
            topology,
            prompt);

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest(prompt));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith(prompt, result.Value!.FinalHumanResult, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(result.Value.FinalHumanResult, prompt));
        Assert.DoesNotContain("For your request:", result.Value.FinalHumanResult, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task GeneratedCapabilityFlow_MultilineOutputEqualToCallerPromptAppearsOnceWithoutTopology(
        string lineEnding)
    {
        var prompt = $"implement the{lineEnding}backend service";
        var flow = CreateGeneratedCapabilityFlow(
            new FixedLogicalUnit(prompt),
            topology: null,
            originalPrompt: prompt);

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest(prompt));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith(
            prompt,
            result.Value!.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(result.Value.FinalHumanResult, prompt));
        Assert.DoesNotContain("For your request:", result.Value.FinalHumanResult, StringComparison.Ordinal);
        Assert.Contains(
            "Sub-phase 4.1 (Backend Services)",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Contains(
            "Focus areas: IMPLEMENT",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_PropagatesUnitFailureWithoutFabricatingHumanResult()
    {
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit(
            (_, _) => Task.FromResult(
                OperationResult<LogicalUnitResult<string>>.Fail(
                    "Model endpoint unavailable.",
                    "MODEL_ENDPOINT_UNAVAILABLE"))));

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("MODEL_ENDPOINT_UNAVAILABLE", result.ErrorCode);
        Assert.Equal("Model endpoint unavailable.", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_MapsCooperativeCancellationToCancelledFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit((_, token) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<OperationResult<LogicalUnitResult<string>>>(token);
        }));

        var result = await flow.ExecuteAsync(
            CreateGeneratedCapabilityRequest(),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal("Cancelled.", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_CallerCancellationWinsOverSuccessfulUnitEnvelope()
    {
        using var cancellation = new CancellationTokenSource();
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit((request, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(
                BuildLogicalUnitResult(request, "model output after cancellation")));
        }));

        var result = await flow.ExecuteAsync(
            CreateGeneratedCapabilityRequest(),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal("Cancelled.", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_CallerCancellationWinsOverFailedUnitEnvelope()
    {
        using var cancellation = new CancellationTokenSource();
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(
                OperationResult<LogicalUnitResult<string>>.Fail(
                    "Unit failure after cancellation.",
                    "UNIT_FAILURE_AFTER_CANCELLATION"));
        }));

        var result = await flow.ExecuteAsync(
            CreateGeneratedCapabilityRequest(),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal("Cancelled.", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_DoesNotMapUnrelatedOperationCanceledException()
    {
        var unrelatedCancellation = new OperationCanceledException("Unit cancelled unrelated work.");
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit(
            (_, _) => Task.FromException<OperationResult<LogicalUnitResult<string>>>(
                unrelatedCancellation)));

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(
            () => flow.ExecuteAsync(CreateGeneratedCapabilityRequest()));

        Assert.Same(unrelatedCancellation, thrown);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_RejectsMissingUnitResultEnvelope()
    {
        var flow = CreateGeneratedCapabilityFlow(new DelegateLogicalUnit(
            (_, _) => Task.FromResult(
                OperationResult<LogicalUnitResult<string>>.Ok(null!))));

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("GENERATED_CAPABILITY_RESULT_REQUIRED", result.ErrorCode);
        Assert.Equal("Generated capability unit returned no result.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t")]
    [InlineData(null)]
    public async Task GeneratedCapabilityFlow_RejectsEmptyUnitOutputInsteadOfReturningDeterministicTemplate(
        string? emptyOutput)
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var add = topology.AddOrUpdateSubPhase(BuildSubPhase(
            "4.1",
            "Backend Services",
            "Deterministic topology metadata.",
            ["IMPLEMENT"]));
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flow = CreateGeneratedCapabilityFlow(new FixedLogicalUnit(emptyOutput!), topology);

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("EMPTY_GENERATED_CAPABILITY_OUTPUT", result.ErrorCode);
        Assert.Equal("Generated capability unit returned an empty output.", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratedCapabilityFlow_TopologyAbsenceCannotSuppressSignedAnnotations()
    {
        const string authoritativeOutput = "MODEL_OUTPUT: preserve this answer when topology lookup fails.";
        var flow = CreateGeneratedCapabilityFlow(
            new FixedLogicalUnit(authoritativeOutput),
            new InMemoryLogicalTopologyRegistry());

        var result = await flow.ExecuteAsync(CreateGeneratedCapabilityRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(authoritativeOutput, result.Value!.FinalHumanResult, StringComparison.Ordinal);
        Assert.Contains(
            "Sub-phase 4.1 (Backend Services)",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Contains(
            "Focus areas: IMPLEMENT",
            result.Value.FinalHumanResult,
            StringComparison.Ordinal);
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
            var trainer = new DpoTrainer(
                new LlmModel(InvokeBuildTinyConfig()),
                new LlmModel(InvokeBuildTinyConfig()),
                checkpointDir,
                IssueDpoMutationAdmission()
                    .DpoTrainerMutationAdmissionCapability);

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
        var baseParameters = model.Parameters().Select(parameter => parameter.Value).ToArray();
        var config = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };

        var adapters = LoraApplicator.Apply(model, config);
        var parametersAfterFirstApply = model.Parameters().Select(parameter => parameter.Value).ToArray();
        var reapplied = LoraApplicator.Apply(model, new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["v_proj", "q_proj", "q_proj"] });

        Assert.Equal(8, adapters.Count);
        Assert.Equal(baseParameters.Length + 16, parametersAfterFirstApply.Length);
        Assert.Equal(parametersAfterFirstApply.Length, model.Parameters().Count());
        Assert.All(baseParameters, parameter => Assert.False(parameter.RequiresGrad));
        Assert.All(parametersAfterFirstApply.Skip(baseParameters.Length), parameter => Assert.True(parameter.RequiresGrad));
        Assert.Equal(adapters.Count, reapplied.Count);
        Assert.True(adapters.Zip(reapplied).All(pair => ReferenceEquals(pair.First, pair.Second)));
        Assert.All(adapters.SelectMany(adapter => adapter.Parameters()), parameter =>
            Assert.All(parameter.Value.Data, value => Assert.False(float.IsNaN(value))));
    }

    [Fact]
    public async Task LogicalDpoTrainingUnit_UsesSameLivePolicyAndReapplyIsIdempotent()
    {
        var checkpointDir = Path.Combine(Path.GetTempPath(), $"logical-dpo-live-policy-{Guid.NewGuid():N}");
        try
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train("prompt chosen rejected live policy adapter", 300);
            var policy = new LlmModel(InvokeBuildTinyConfig());
            var reference = new LlmModel(InvokeBuildTinyConfig());
            var config = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
            var preferences = new[]
            {
                new LogicalDpoPreferenceExample(
                    "prompt", "chosen", "rejected", [1f])
                {
                    ChosenAuthorKind =
                        DpoPairSideAuthorKind.DeterministicGenerator,
                    RejectedAuthorKind =
                        DpoPairSideAuthorKind.DeterministicGenerator
                }
            };
            var prepared = LogicalDpoTrainingUnit.PrepareExecution(
                preferences,
                tokenizer);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var request = new LogicalDpoTrainingRequest(
                policy,
                reference,
                tokenizer,
                checkpointDir,
                preferences,
                config,
                AdmissionCapabilities: IssueDpoMutationAdmission())
            {
                PreparedExecution = prepared.Value
            };

            var trained = await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
                "logical-dpo-live-policy",
                LogicalDpoTrainingUnit.Id,
                LogicalDpoTrainingUnit.Role,
                LogicalDpoTrainingUnit.DomainTag,
                request,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()));

            Assert.True(trained.IsSuccess, trained.ErrorMessage);
            Assert.Equal(8, trained.Value!.Output.LoraAdapterCount);
            var parametersAfterTraining = policy.Parameters().Select(parameter => parameter.Value).ToArray();
            var reapplied = LoraApplicator.Apply(
                policy,
                new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["v_proj", "q_proj"] });
            Assert.Equal(parametersAfterTraining.Length, policy.Parameters().Count());
            Assert.Equal(8, reapplied.Count);

            var liveAdapter = reapplied[1];
            foreach (var parameter in liveAdapter.Parameters())
                Array.Fill(parameter.Value.Data, 0f);
            var tokens = new Tensor([1f, 2f, 3f], new TensorShape(1, 3));
            var before = policy.Forward(tokens, training: false).Data.ToArray();
            foreach (var parameter in liveAdapter.Parameters())
                Array.Fill(parameter.Value.Data, 0.05f);
            var after = policy.Forward(tokens, training: false).Data;

            Assert.False(before.SequenceEqual(after));
        }
        finally
        {
            if (Directory.Exists(checkpointDir))
                Directory.Delete(checkpointDir, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_StoresOriginalPromptInManifest()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model.", ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]);
        var add = topology.AddOrUpdateSubPhase(subPhase);
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"original-prompt-manifest-{Guid.NewGuid():N}");

        try
        {
            var trained = await new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "original-prompt-manifest-run",
                new LogicalOrchestratorRequest(
                    "original-prompt-manifest-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "add auth guard",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [Attest(BuildKnowledgeEdgeBranch("3.4"), "add auth guard",
                    Domain.ARCH, subPhase)],
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
        var subPhase = BuildSubPhase("3.4", "Security and Authorization Model", "Owns security model, authentication, authorization, and tenant isolation.", ["SECURITY", "AUTHORIZATION", "IMPLEMENT"]);
        var add = topology.AddOrUpdateSubPhase(subPhase);
        Assert.True(add.IsSuccess, add.ErrorMessage);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"full-roundtrip-deeper-{Guid.NewGuid():N}");

        try
        {
            var trained = await new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: new RecordingLogicalCapabilityTrainingAdmissionAuthority()).TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
                "full-roundtrip-deeper-run",
                new LogicalOrchestratorRequest(
                    "full-roundtrip-deeper-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "implement the authorization guard for the tenant controller",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                [Attest(BuildKnowledgeEdgeBranch("3.4"),
                    "implement the authorization guard for the tenant controller",
                    Domain.ARCH, subPhase)],
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
            Assert.Contains("Generated capability for sub-phase 3.4", result.Value.FinalHumanResult);
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

    private sealed class DelegateLogicalUnit : ILogicalUnit<string, string>
    {
        private readonly Func<
            LogicalUnitRequest<string>,
            CancellationToken,
            Task<OperationResult<LogicalUnitResult<string>>>> _execute;

        public DelegateLogicalUnit(
            Func<
                LogicalUnitRequest<string>,
                CancellationToken,
                Task<OperationResult<LogicalUnitResult<string>>>> execute)
        {
            _execute = execute;
        }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            return _execute(request, cancellationToken);
        }
    }

    private static LogicalCapabilityTrainingAdmissionCapabilities
        IssueDpoMutationAdmission()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var logicalMutationAdmission =
            LogicalDpoMutationAdmissionCapability.Issue(lease);
        Assert.True(
            logicalMutationAdmission.IsSuccess,
            logicalMutationAdmission.ErrorMessage);
        var executableManifestAttestation =
            LogicalCapabilityExecutableManifestAttestationCapability.Issue(
                lease);
        Assert.True(
            executableManifestAttestation.IsSuccess,
            executableManifestAttestation.ErrorMessage);
        return new LogicalCapabilityTrainingAdmissionCapabilities(
            logicalMutationAdmission.Value!,
            DpoMutationAdmissionCapability.Issue(lease),
            executableManifestAttestation.Value!);
    }

    private sealed class TestDpoMutationAdmissionLease
        : ILogicalCapabilityTrainingAdmissionLease,
          ILogicalDpoMutationAdmissionIssuer,
          IDpoMutationAdmissionIssuer,
          ILogicalCapabilityExecutableManifestAttestationIssuer
    {
        public OperationResult<IDisposable> AcquireMutationPermit()
            => OperationResult<IDisposable>.Ok(
                new CancellationTokenSource());

        OperationResult<IDisposable>
            ILogicalDpoMutationAdmissionIssuer.AcquireDpoMutationPermit()
            => AcquireMutationPermit();

        IDisposable IDpoMutationAdmissionIssuer
            .AcquireDpoTrainerMutationPermit()
        {
            var permit = AcquireMutationPermit();
            if (!permit.IsSuccess || permit.Value is null)
            {
                throw new DpoMutationAdmissionException(
                    permit.ErrorMessage,
                    permit.ErrorCode);
            }

            return permit.Value;
        }

        OperationResult<LogicalCapabilityExecutableManifestAttestation>
            ILogicalCapabilityExecutableManifestAttestationIssuer
                .SealExecutableManifest(
                    string executableManifestSha256,
                    LogicalCapabilityTrainingReplayAttestation
                        replayAttestation)
            => OperationResult<
                LogicalCapabilityExecutableManifestAttestation>.Fail(
                "Executable manifest sealing is unavailable in this test.",
                LogicalCapabilityExecutableManifestAttestationCapability
                    .RequiredErrorCode);

        public void Dispose()
        {
        }
    }

    private static GeneratedCapabilityFlow CreateGeneratedCapabilityFlow(
        ILogicalUnit<string, string> unit,
        ILogicalTopologyRegistry? topology = null,
        string originalPrompt = "implement the backend service")
    {
        return new GeneratedCapabilityFlow(
            "4",
            "4.1",
            "Backend Services",
            ["software-development"],
            ["IMPLEMENT"],
            Domain.CODE_Q.ToString(),
            unit,
            topology,
            originalPrompt);
    }

    private static LogicalFlowRequest<string> CreateGeneratedCapabilityRequest(
        string input = "implement the backend service")
    {
        return new LogicalFlowRequest<string>(
            "generated-flow-output-test",
            "generated-capability-flow-4-1",
            "generated-capability-4-1",
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LogicalUnitResult<string> BuildLogicalUnitResult(
        LogicalUnitRequest<string> request,
        string output)
    {
        return new LogicalUnitResult<string>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            output,
            0.99,
            ["IMPLEMENT"],
            [request.DomainTag],
            ["delegate-unit"],
            []);
    }

    private static LogicalFlowResult Attest(LogicalFlowResult branch,
        string userRequest, Domain domain, LogicalSubPhase subPhase)
        => branch with
        {
            TrainingContentProvenance = LogicalCapabilityTrainingAuthorityTestFactory
                .IssueSingle(branch,
                    new LogicalTrainingContentDeterministicInputs(userRequest,
                        domain.ToString(), subPhase.PhaseId, subPhase.SubPhaseId,
                        subPhase.Name, subPhase.Purpose, subPhase.PredicateIds,
                        subPhase.DomainIds))
        };

    private static int CountOccurrences(string value, string expected)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(expected, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += expected.Length;
        }

        return count;
    }
}
