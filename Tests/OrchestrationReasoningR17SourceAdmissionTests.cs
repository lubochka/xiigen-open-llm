// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningR17SourceAdmissionTests
{
    [Fact]
    public async Task ProductionCorpusWithPendingEmptyLedger_FailsClosedWithExactRejectedCode()
    {
        var payload = RepoPath(
            "training-data",
            "orchestration-reasoning",
            "orchestration-reasoning-dpo-trainer-payloads.jsonl");
        var manifest = RepoPath(
            "training-data",
            "orchestration-reasoning",
            "orchestration-reasoning-dpo-trainer-payloads-manifest.json");
        var canonicalLedger = RepoPath(
            "training-data",
            "orchestration-reasoning",
            "source-admission-ledger.json");
        var productionEvidence = RepoPath(
            "training-data",
            "orchestration-reasoning",
            "source-admission-evidence.json");

        Assert.True(File.Exists(canonicalLedger));
        using var ledgerDocument = JsonDocument.Parse(
            await File.ReadAllTextAsync(canonicalLedger));
        Assert.Equal(
            "REBUILD_PENDING_NO_ADMITTED_ROWS",
            ledgerDocument.RootElement.GetProperty("status").GetString());
        Assert.False(
            ledgerDocument.RootElement.GetProperty("computed_training_allowed").GetBoolean());
        Assert.Empty(ledgerDocument.RootElement.GetProperty("entries").EnumerateArray());
        Assert.False(File.Exists(productionEvidence));
        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            payload,
            manifest,
            canonicalLedger,
            sourceAdmissionEvidencePath: null);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED",
            result.ErrorCode);
    }

    [Theory]
    [InlineData("project_internal_self_authored")]
    public async Task OnlyAllowedEvidenceBoundSourceKinds_AreAdmitted(string sourceKind)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            sourceKind: sourceKind);

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(sourceKind, result.Value!.AdmittedState.Receipt.AllowedSourceKind);
        Assert.True(
            OrchestrationReasoningSourceAdmission.Validate(result.Value.AdmittedState).IsSuccess);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("hosted")]
    [InlineData("closed_model")]
    [InlineData("sonnet")]
    [InlineData("project_internal")]
    public async Task AllOtherSourceKinds_AreRejected(string sourceKind)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            sourceKind: sourceKind);

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED", result.ErrorCode);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("chatgpt")]
    [InlineData("gpt-5")]
    [InlineData("o1")]
    [InlineData("o3")]
    [InlineData("o4")]
    [InlineData("anthropic")]
    [InlineData("claude")]
    [InlineData("sonnet")]
    [InlineData("haiku")]
    [InlineData("opus")]
    [InlineData("google")]
    [InlineData("gemini")]
    [InlineData("xai")]
    [InlineData("grok")]
    [InlineData("copilot")]
    [InlineData("jsonnet/SONNET-model")]
    public async Task CanonicalForbiddenIdentityAliases_AreRejectedAtAdmissionBoundary(
        string alias)
    {
        var safeAlias = alias.Replace("/", "-", StringComparison.Ordinal);
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            pairId: $"fixture-{safeAlias}-pair",
            sourceRowId: $"fixture-{safeAlias}-row",
            sourceId: $"fixture-{safeAlias}-source",
            sourceRef: $"fixture://self-authored/{alias}/source");

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.AdmissionRejectedCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData("jsonnet")]
    [InlineData("sonnetry")]
    public async Task NonIdentitySuperstrings_AreAdmittedAtTokenBoundaries(
        string nonIdentityModelFragment)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            pairId: $"fixture-qwen-{nonIdentityModelFragment}-9b-pair",
            sourceRowId: $"fixture-qwen-{nonIdentityModelFragment}-9b-row",
            sourceId: $"fixture-qwen-{nonIdentityModelFragment}-9b-source",
            sourceRef: $"fixture://self-authored/qwen-{nonIdentityModelFragment}-9b/source");

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(
            OrchestrationReasoningSourceAdmission.Validate(result.Value!.AdmittedState).IsSuccess);
    }

    [Fact]
    public async Task LearnedPatternHostCheckSetsWithKnownHostLabels_AreAdmittedAsHosts()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            sourceRef: "fixture://self-authored/internal/source",
            learnedPattern: CreateLearnedPatternWithHostChecks(["claude", "cursor", "codex"]));

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["claude", "codex", "cursor"], result.Value!.AdmittedState.State.LearnedPatterns![0].HostCheckSets);
        Assert.True(
            OrchestrationReasoningSourceAdmission.Validate(result.Value.AdmittedState).IsSuccess);
    }

    [Theory]
    [InlineData("anthropic/claude")]
    [InlineData("claude-3-5-sonnet")]
    [InlineData("gpt-5")]
    [InlineData("openai")]
    [InlineData("fixture://host/codex")]
    [InlineData("repo\\codex")]
    [InlineData("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task LearnedPatternHostCheckSetsRejectModelOrReferenceShapedLabels(
        string unsafeHostLabel)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            sourceRef: "fixture://self-authored/internal/source",
            learnedPattern: CreateLearnedPatternWithHostChecks([unsafeHostLabel]));

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.AdmissionRejectedCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task ScenarioRefStillRejectsForbiddenModelIdentityWhenHostLabelsAreSafe()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            sourceRef: "fixture://self-authored/internal/source",
            learnedPattern: CreateLearnedPatternWithHostChecks(
                ["codex"],
                scenarioRef: "fixture://scenario/anthropic/claude/source"));

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.AdmissionRejectedCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData("or01u-sonnet-clinerules-001", "fixture://safe", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("safe-pair", "sonnet-local-analyzes:.clinerules-analysis.md", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("safe-pair", "fixture://safe", "af174a555ca99292c0c87210665536dec6d520f2bd00532397ccf9318383fb05")]
    [InlineData("or-expanded-v1-pair-001", "fixture://safe", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("or-expanded-v1-pair-002", "fixture://safe", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("or-expanded-v1-pair-003", "fixture://safe", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task HardDeniedIdsRefsHashesAndDerivedPairs_RejectRegardlessOfBooleans(
        string pairId,
        string sourceRef,
        string sourceHash)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            pairId: pairId,
            sourceRef: sourceRef,
            sourceSha256: sourceHash);

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED", result.ErrorCode);
    }

    [Fact]
    public async Task TrainingAllowedInputFlags_DoNotGrantOrDenyAuthority()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(corpus.ManifestPath));
        Assert.True(manifest.RootElement.GetProperty("training_allowed").GetBoolean());
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(corpus.LedgerPath));
        Assert.False(ledger.RootElement.GetProperty("entries")[0].GetProperty("training_allowed").GetBoolean());

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public async Task DerivedPairPattern_IsRejectedBeforeRuntimeAuthorityCanBeCreated()
    {
        var descriptor = OrchestrationReasoningCanonicalState.CreateDescriptor(
            "state-r17-hard-denied-pattern",
            "fixture://guide",
            "fixture-manifest",
            new string('a', 64),
            "ca440dc",
            [
                new OrchestrationReasoningLearnedPattern(
                    "or-learned-pattern:or-expanded-v1-pair-001",
                    "fixture://scenario",
                    ["translation"],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    0.5)
            ]);
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            learnedPattern: descriptor.LearnedPatterns![0]);
        var validation = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);
        Assert.False(validation.IsSuccess);
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.AdmissionRejectedCode,
            validation.ErrorCode);
    }

    private static OrchestrationReasoningLearnedPattern CreateLearnedPatternWithHostChecks(
        IReadOnlyList<string> hostCheckSets,
        string scenarioRef = "fixture://scenario/host-check")
        => new(
            "or-learned-pattern:fixture-host-check",
            scenarioRef,
            ["bounded-work-order"],
            ["one-agent"],
            ["human-status"],
            ["code-plus-test"],
            ["focused-admission-test"],
            ["implementation"],
            ["source-admission"],
            ["orchestration-reasoning"],
            hostCheckSets,
            ["no-model-identity-in-host-label"],
            ["return-reviewed-slice"],
            0.91);

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }
}
