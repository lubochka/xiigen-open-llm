// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningTrainerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task OrchestrationReasoningTrainer_MaterializesLearnedPatternsFromPayloadContracts()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();
        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.RowCount);
        Assert.Equal(1, result.Value.LearnedPatternCount);
        Assert.Single(result.Value.LearnedState.LearnedPatterns!);
        Assert.False(result.Value.TrainingStarted);
        Assert.False(result.Value.ModelAssetCreated);
        Assert.False(result.Value.CheckpointCreated);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.ExcludedTechnicalRefsTrainable);
        Assert.Contains("fixture-pair-001", result.Value.AcceptedPairIds);

        var trainableSignals = result.Value.LearnedState.LearnedPatterns!
            .SelectMany(pattern => pattern.RequiredTaskTypes
                .Concat(pattern.RequiredConstraintSignals)
                .Concat(pattern.RequiredRequirementSignals)
                .Concat(pattern.RequiredDeliverableSignals)
                .Concat(pattern.RequiredEvidenceSignals));
        Assert.DoesNotContain("exact_repo_paths", trainableSignals);
        Assert.DoesNotContain("host_repo_operations", trainableSignals);
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_PersistsBundleLocalLearnedStateManifestRef()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();
        const string bundleLocalManifestRef = "source-admission/pattern-01/manifest.json";

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath,
            learnedStateManifestRef: bundleLocalManifestRef);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bundleLocalManifestRef, result.Value!.LearnedState.ManifestId); // The learned state stores the portable path that survives moving the checkpoint bundle.
        Assert.Equal(bundleLocalManifestRef, result.Value.AdmittedState.State.ManifestId);
    }

    [Theory]
    [InlineData("/abs")]
    [InlineData("C:/x")]
    [InlineData(@"C:\x")]
    [InlineData("C:foo")]
    [InlineData("file:manifest")]
    [InlineData("../x")]
    [InlineData("source/../x")]
    [InlineData("x;delta:y")]
    public async Task OrchestrationReasoningTrainer_RejectsUnsafeLearnedStateManifestRefs(string unsafeManifestRef)
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();

        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath,
            learnedStateManifestRef: unsafeManifestRef);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_TRAINER_PORTABLE_MANIFEST_REF_INVALID", result.ErrorCode); // Unsafe refs fail before they can turn into host-specific proof paths.
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_ContinueTrainingPersistsOrderedBundleLocalManifestRefs()
    {
        using var firstCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            pairId: "fixture-pair-merge-001",
            sourceRowId: "fixture-row-merge-001",
            sourceId: "fixture-source-merge-001");
        using var secondCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
            pairId: "fixture-pair-merge-002",
            sourceRowId: "fixture-row-merge-002",
            sourceId: "fixture-source-merge-002");

        var first = await new OrchestrationReasoningTrainer().MaterializeAsync(
            firstCorpus.PayloadPath,
            firstCorpus.ManifestPath,
            firstCorpus.LedgerPath,
            firstCorpus.EvidencePath,
            learnedStateManifestRef: "source-admission/pattern-01/manifest.json");
        var second = await new OrchestrationReasoningTrainer().MaterializeAsync(
            secondCorpus.PayloadPath,
            secondCorpus.ManifestPath,
            secondCorpus.LedgerPath,
            secondCorpus.EvidencePath,
            learnedStateManifestRef: "source-admission/pattern-02/manifest.json");
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);

        var machine = new OrchestrationReasoningMachine();
        Assert.True(machine.ImportAdmittedState(first.Value!.AdmittedState).IsSuccess);
        var continued = machine.ContinueTrainingFromLearnedState(
            second.Value!.AdmittedState,
            second.Value.AdmittedState.Receipt.PairIds);

        Assert.True(continued.IsSuccess, continued.ErrorMessage);
        Assert.Equal(2, continued.Value!.MergedLearnedState.LearnedPatterns!.Count);
        Assert.Equal(
            "source-admission/pattern-01/manifest.json;delta:source-admission/pattern-02/manifest.json",
            continued.Value.MergedLearnedState.ManifestId);
        var manifestRefs = continued.Value.MergedLearnedState.ManifestId.Split(";delta:", StringSplitOptions.None);
        Assert.Equal(
            ["source-admission/pattern-01/manifest.json", "source-admission/pattern-02/manifest.json"],
            manifestRefs);
        Assert.All(manifestRefs, manifestRef =>
        {
            Assert.False(Path.IsPathRooted(manifestRef), manifestRef);
            Assert.DoesNotContain('\\', manifestRef);
            Assert.DoesNotContain("..", manifestRef, StringComparison.Ordinal);
        }); // Each merged part remains a plain checkpoint-bundle ref, so load can re-open only the proof-bound source files.
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_RejectsExcludedTechnicalRefsInTrainableFeatures()
    {
        var root = NewTempRoot();
        try
        {
            using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(root);
            var row = JsonNode.Parse(await File.ReadAllTextAsync(corpus.PayloadPath))!;
            var intentSignals = row["chosen_scenario_plan_features"]!["intent_signals"]!.AsArray();
            intentSignals.Add("exact_repo_paths");
            row["excluded_technical_execution_refs"]!.AsArray().Add("exact_repo_paths");
            await File.WriteAllTextAsync(corpus.PayloadPath, row.ToJsonString(JsonLineOptions));
            OrchestrationReasoningSourceAdmissionTestFixture.RefreshBindings(corpus);

            var result = await new OrchestrationReasoningTrainer().MaterializeAsync(
                corpus.PayloadPath,
                corpus.ManifestPath,
                corpus.LedgerPath,
                corpus.EvidencePath);

            Assert.False(result.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_EXCLUDED_REF_LEAK", result.ErrorCode);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_RejectsEmptyOrMissingPayloadRows()
    {
        var root = NewTempRoot();
        try
        {
            using var emptyCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                Path.Combine(root, "empty"));
            await File.WriteAllTextAsync(emptyCorpus.PayloadPath, string.Empty);
            OrchestrationReasoningSourceAdmissionTestFixture.RefreshBindings(emptyCorpus);

            var emptyResult = await new OrchestrationReasoningTrainer().MaterializeAsync(
                emptyCorpus.PayloadPath,
                emptyCorpus.ManifestPath,
                emptyCorpus.LedgerPath,
                emptyCorpus.EvidencePath);

            Assert.False(emptyResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED", emptyResult.ErrorCode);

            using var missingCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                Path.Combine(root, "missing"));
            var row = JsonNode.Parse(await File.ReadAllTextAsync(missingCorpus.PayloadPath))!;
            row.AsObject().Remove("chosen_scenario_plan_features");
            await File.WriteAllTextAsync(missingCorpus.PayloadPath, row.ToJsonString(JsonLineOptions));
            OrchestrationReasoningSourceAdmissionTestFixture.RefreshBindings(missingCorpus);

            var missingResult = await new OrchestrationReasoningTrainer().MaterializeAsync(
                missingCorpus.PayloadPath,
                missingCorpus.ManifestPath,
                missingCorpus.LedgerPath,
                missingCorpus.EvidencePath);

            Assert.False(missingResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_JSON_INVALID", missingResult.ErrorCode);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_RequiresAuthoredPayloadAndRejectsTrainReadyRows()
    {
        var root = NewTempRoot();
        try
        {
            using var notAuthoredCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                Path.Combine(root, "not-authored"));
            var notAuthored = JsonNode.Parse(await File.ReadAllTextAsync(notAuthoredCorpus.PayloadPath))!;
            notAuthored["dpo_payload_authored"] = false;
            await File.WriteAllTextAsync(notAuthoredCorpus.PayloadPath, notAuthored.ToJsonString(JsonLineOptions));
            OrchestrationReasoningSourceAdmissionTestFixture.RefreshBindings(notAuthoredCorpus);

            var notAuthoredResult = await new OrchestrationReasoningTrainer().MaterializeAsync(
                notAuthoredCorpus.PayloadPath,
                notAuthoredCorpus.ManifestPath,
                notAuthoredCorpus.LedgerPath,
                notAuthoredCorpus.EvidencePath);

            Assert.False(notAuthoredResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_PAYLOAD_NOT_AUTHORED", notAuthoredResult.ErrorCode);

            using var trainReadyCorpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                Path.Combine(root, "train-ready"));
            var trainReady = JsonNode.Parse(await File.ReadAllTextAsync(trainReadyCorpus.PayloadPath))!;
            trainReady["dpo_train_ready"] = true;
            await File.WriteAllTextAsync(trainReadyCorpus.PayloadPath, trainReady.ToJsonString(JsonLineOptions));
            OrchestrationReasoningSourceAdmissionTestFixture.RefreshBindings(trainReadyCorpus);

            var trainReadyResult = await new OrchestrationReasoningTrainer().MaterializeAsync(
                trainReadyCorpus.PayloadPath,
                trainReadyCorpus.ManifestPath,
                trainReadyCorpus.LedgerPath,
                trainReadyCorpus.EvidencePath);

            Assert.False(trainReadyResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_TRAIN_READY_REJECTED", trainReadyResult.ErrorCode);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningTrainer_LearnedStateCanDriveRuntimePlanWithoutRawPromptMemory()
    {
        using var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus();
        var materialized = await new OrchestrationReasoningTrainer().MaterializeAsync(
            corpus.PayloadPath,
            corpus.ManifestPath,
            corpus.LedgerPath,
            corpus.EvidencePath);
        Assert.True(materialized.IsSuccess, materialized.ErrorMessage);

        var learnedState = materialized.Value!.LearnedState;
        var pattern = learnedState.LearnedPatterns![0];
        var machine = new OrchestrationReasoningMachine(
            new StaticLoader(materialized.Value.AdmittedState));

        var first = machine.Plan(CreateRuntimeRequest(pattern, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        var second = machine.Plan(CreateRuntimeRequest(pattern, "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, first.Value!.ContractStatus);
        Assert.Equal(pattern.ScenarioRef, first.Value.ScenarioPlan.ScenarioId);
        Assert.Equal([pattern.PatternId], first.Value.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Equal(first.Value.ScenarioPlan.ScenarioId, second.Value!.ScenarioPlan.ScenarioId);
        Assert.Equal(first.Value.ScenarioPlan.SelectedLearnedPatterns, second.Value.ScenarioPlan.SelectedLearnedPatterns);
    }

    private static string PayloadPath => RepoPath("training-data", "orchestration-reasoning", "orchestration-reasoning-dpo-trainer-payloads.jsonl");

    private static string ManifestPath => RepoPath("training-data", "orchestration-reasoning", "orchestration-reasoning-dpo-trainer-payloads-manifest.json");

    private static OrchestrationReasoningRequest CreateRuntimeRequest(
        OrchestrationReasoningLearnedPattern pattern,
        string rawPromptSha256)
        => new(
            RequestId: $"or-trainer-runtime-{rawPromptSha256[..8]}",
            Task: new StructuredTaskContract(
                PromptId: "or-trainer-runtime-fixture",
                RawPromptSha256: rawPromptSha256,
                UserLanguage: "en",
                TaskType: pattern.RequiredTaskTypes[0],
                ExplicitUserRequirements: pattern.RequiredRequirementSignals,
                Constraints: pattern.RequiredConstraintSignals,
                Deliverables: pattern.RequiredDeliverableSignals,
                ForbiddenActions: ["raw_prompt_memory", "checkpoint_asset_creation"],
                RequiredHosts: ["core-only"],
                EvidenceNeeds: pattern.RequiredEvidenceSignals,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["source_ref"] = $"runtime-fixture:{rawPromptSha256[..8]}",
                    ["payload_contract"] = "ref-only"
                },
                SplitId: "or-trainer-runtime-split"),
            GuideDerivedStateRefs: [pattern.PatternId],
            CorpusBundleIds: ["corpus:orchestration-reasoning:dpo-payloads"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static JsonNode ReadFirstPayloadRow()
    {
        var line = File.ReadLines(PayloadPath).First(line => !string.IsNullOrWhiteSpace(line));
        return JsonNode.Parse(line)!;
    }

    private static async Task<(string PayloadPath, string ManifestPath)> WritePayloadFixtureAsync(
        string root,
        IReadOnlyList<JsonNode> rows,
        string payloadFileName = "payloads.jsonl")
    {
        var payloadPath = Path.Combine(root, payloadFileName);
        await File.WriteAllTextAsync(payloadPath, string.Join(Environment.NewLine, rows.Select(row => row.ToJsonString(JsonLineOptions))));
        var manifestPath = await WriteManifestForPayloadAsync(root, payloadPath, rows.Count);
        return (payloadPath, manifestPath);
    }

    private static async Task<string> WriteManifestForPayloadAsync(
        string root,
        string payloadPath,
        int rowCount)
    {
        var manifest = JsonNode.Parse(await File.ReadAllTextAsync(ManifestPath))!.AsObject();
        manifest["payloads_file_ref"] = payloadPath.Replace('\\', '/');
        manifest["payloads_sha256"] = await HashFileAsync(payloadPath);
        manifest["row_count"] = rowCount;
        var manifestPath = Path.Combine(root, $"{Path.GetFileNameWithoutExtension(payloadPath)}-manifest.json");
        await File.WriteAllTextAsync(manifestPath, manifest.ToJsonString(JsonOptions));
        return manifestPath;
    }

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }

    private static string NewTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-trainer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static async Task<string> HashFileAsync(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private sealed class StaticLoader(OrchestrationReasoningAdmittedLearnedState admittedState)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningAdmittedLearnedState> Load()
            => OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(admittedState);
    }
}
