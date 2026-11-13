// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using ContinuousLearning;
using DPO;
using TrainingLoop.Optimizers;
using TranslationDomain;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

// Deliberately no LlmModel construction, Student forward/generation, optimizer call or training lease.
public sealed class SimulationEnRuContinuationTests
{
    private const string Root = "C:/Projects/llm_mvp_core";
    private const string Input = Root + "/training-data/dpo/translation-en-ru/simulation-enru-500-v1/train500.jsonl";

    [Fact]
    public void ExactEnRu500ImportPreservesAllRowsAndRejectsWrongDomainHashAndCount()
    {
        var actual = CodeDocumentationSimulationDpoExperiment.Inspect(Input, SimulationEnRuContinuation.DatasetSha256, 500, SimulationDpoDomain.EnRu);
        Assert.True(actual.IsSuccess, actual.ErrorCode);
        Assert.Equal(500, actual.Value!.Count);
        Assert.All(actual.Value, row => Assert.Equal(row.Prompt, SimulationEnRuPrompt.Build(SimulationEnRuPrompt.Extract(row.Prompt))));
        Assert.False(CodeDocumentationSimulationDpoExperiment.Inspect(Input, SimulationEnRuContinuation.DatasetSha256, 500).IsSuccess);
        Assert.False(CodeDocumentationSimulationDpoExperiment.Inspect(Input, new string('0', 64), 500, SimulationDpoDomain.EnRu).IsSuccess);
        Assert.False(CodeDocumentationSimulationDpoExperiment.Inspect(Input, SimulationEnRuContinuation.DatasetSha256, 499, SimulationDpoDomain.EnRu).IsSuccess);
    }

    [Fact]
    public void RawEnglishHasOneExactPrefixAndPreservesWhitespaceAndUnicode()
    {
        const string raw = "  Do not remove café or имена.\n";
        var prompt = SimulationEnRuPrompt.Build(raw);
        Assert.Equal("Translate English to Russian:\n" + raw, prompt);
        Assert.Equal(raw, SimulationEnRuPrompt.Extract(prompt));
        Assert.Throws<ArgumentException>(() => SimulationEnRuPrompt.Build(prompt));
        Assert.Throws<ArgumentException>(() => SimulationEnRuPrompt.Extract(raw));
        Assert.Throws<ArgumentException>(() => SimulationEnRuPrompt.Build(" \r\n"));
        Assert.Throws<System.Text.EncoderFallbackException>(() => SimulationEnRuPrompt.Build("\ud800"));
    }

    [Fact]
    public void AlternateEnRuScheduleRefusesBeforeLoadingOrCreatingAttempt()
    {
        var loaded = false;
        var result = CodeDocumentationSimulationDpoExperiment.Run(new(Root, "never-issued-invalid-enru-schedule", "main",
            Input, SimulationEnRuContinuation.DatasetSha256, 500, 5, 100, null,
            _ => { loaded = true; throw new InvalidOperationException("MODEL_ALLOCATION_FORBIDDEN"); }, SimulationDpoDomain.EnRu));
        Assert.False(result.IsSuccess);
        Assert.Equal("SIMULATION_DPO_ARGUMENTS_INVALID", result.ErrorCode);
        Assert.False(loaded);
        Assert.False(Directory.Exists(Root + "/training-artifacts/experiments/code-documentation-simulation/never-issued-invalid-enru-schedule"));
    }

    [Fact]
    public void ActualC50PortableStateBindsMomentsRngAndProfileRatherThanCounterAlone()
    {
        var saved = SimulationEnRuContinuation.ReadState(Root);
        Assert.Equal(50, saved.Optimizer.Step);
        Assert.Equal(50, saved.Random.RandomStateStep);
        Assert.Equal(16, saved.Optimizer.Entries.Count);
        Assert.Equal(53248, saved.Optimizer.Entries.Sum(x => x.FirstMoment.Count));
        Assert.Contains(saved.Optimizer.Entries, e => e.FirstMoment.Any(v => v != 0));
        saved.Proof.RequireMatches(saved.Optimizer, saved.Random);
        var wrongCounter = new AdamWPortableState(AdamWPortableState.CurrentFormatVersion, 51, saved.Optimizer.Entries);
        Assert.Throws<DpoMutationAdmissionException>(() => saved.Proof.RequireMatches(wrongCounter, saved.Random));
        Assert.Throws<DpoMutationAdmissionException>(() => saved.Proof.RequireMatches(saved.Optimizer, null));
        var missing = new AdamWPortableState(AdamWPortableState.CurrentFormatVersion, 50, saved.Optimizer.Entries.Skip(1).ToArray());
        Assert.Throws<DpoMutationAdmissionException>(() => saved.Proof.RequireMatches(missing, saved.Random));
        var first = saved.Optimizer.Entries[0]; var moments = first.FirstMoment.ToArray(); moments[0] += 1;
        var changed = new AdamWPortableParameterState(first.Key, first.Dimensions.ToArray(), moments, first.SecondMoment.ToArray());
        var altered = new AdamWPortableState(AdamWPortableState.CurrentFormatVersion, 50, new[] { changed }.Concat(saved.Optimizer.Entries.Skip(1)).ToArray());
        Assert.Throws<DpoMutationAdmissionException>(() => saved.Proof.RequireMatches(altered, saved.Random));
        var wrongProfile = DpoExecutionProfile.FromVerifiedManifest(saved.Proof.Profile.MaterialVersion, saved.Proof.Profile.FramingVersion,
            saved.Proof.Profile.ObjectiveVersion, saved.Proof.Profile.AllowedParameterMap.Skip(1));
        Assert.Throws<InvalidDataException>(() => SimulationEnRuContinuation.ReadState(Root, recipientProfile: wrongProfile));
        Assert.Throws<InvalidDataException>(() => SimulationEnRuContinuation.ReadState(Root, topology: new string('0', 64)));
        Assert.Throws<InvalidDataException>(() => SimulationEnRuContinuation.ReadState(Path.GetTempPath()));
    }

    [Fact]
    public void EnRuEvaluationCannotSilentlySelectOldB0OrAllocateStudent()
    {
        var loaded = false;
        var result = CodeDocumentationSimulationDpoExperiment.Evaluate(new(Root, "never-issued-null-enru-control", "not-read.jsonl", new string('0', 64),
            null, null, new(0, 0, 0, 1, 20261014, 4096), _ => { loaded = true; throw new InvalidOperationException(); }, SimulationDpoDomain.EnRu));
        Assert.False(result.IsSuccess);
        Assert.Equal("SIMULATION_DPO_EVALUATION_INVALID", result.ErrorCode);
        Assert.False(loaded);
    }

    [Fact]
    public void ReachedAdapterPreparationNormalizesLanguageCodesAndReturnsTypedInputRefusals()
    {
        const string raw = "  Keep this exact whitespace.\n";
        var good = ResearchCheckpointEnRuAdapter.PreparePrompt(new(" EN ", " RU ", "general", raw));
        Assert.True(good.IsSuccess);
        Assert.Equal(SimulationEnRuPrompt.Prefix + raw, good.Value);
        Assert.Equal("RESEARCH_ENRU_DIRECTION_INVALID", ResearchCheckpointEnRuAdapter.PreparePrompt(new("ru", "en", "general", raw)).ErrorCode);
        foreach (var invalid in new[] { "", "  ", "\ud800", SimulationEnRuPrompt.Prefix + raw })
            Assert.Equal("RESEARCH_ENRU_INPUT_INVALID", ResearchCheckpointEnRuAdapter.PreparePrompt(new("en", "ru", "general", invalid)).ErrorCode);
    }

    [Fact]
    public async Task OrdinaryTypedFlowPreservesRawOutputWithoutClaimingSemanticPass()
    {
        var machine = new TranslationMachine(new TranslationRoutingFlow(new Dictionary<TranslationDirection, ITranslationDirectionAdapter>
            { [TranslationDirection.EN_RU] = new UnknownQualityAdapter() }));
        var selected = await machine.GetOrchestratorAsync();
        Assert.True(selected.IsSuccess);
        var routed = await selected.Value!.RunAsync(new("no-student-typed-route", TranslationRoutingFlow.ScenarioId, "raw English", new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source_language"] = " EN ", ["target_language"] = " RU " }));
        Assert.True(routed.IsSuccess);
        Assert.Equal("raw-invalid-output", routed.Value!.FinalHumanResult);
        Assert.Equal("TRANSLATION_LOW_CONFIDENCE", routed.Value.OverallVerdict);
        Assert.False(routed.Value.FlowResult.Summary.Passed);
        Assert.DoesNotContain("Bootstrap", string.Join(" ", routed.Value.FlowResult.Warnings));
    }

    private sealed class UnknownQualityAdapter : ITranslationDirectionAdapter
    {
        public TranslationDirection Direction => TranslationDirection.EN_RU;
        public string AdapterId => "pure-contract-test-no-student";
        public Task<OperationResult<TranslationRoutingResult>> TranslateAsync(TranslationRoutingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<TranslationRoutingResult>.Ok(new(Direction, "raw-invalid-output", 0, AdapterId)));
    }
}
