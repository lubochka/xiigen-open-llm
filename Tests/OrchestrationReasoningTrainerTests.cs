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
        var result = await new OrchestrationReasoningTrainer().MaterializeAsync(PayloadPath, ManifestPath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.RowCount);
        Assert.Equal(3, result.Value.LearnedPatternCount);
        Assert.Equal(3, result.Value.LearnedState.LearnedPatterns!.Count);
        Assert.False(result.Value.TrainingStarted);
        Assert.False(result.Value.ModelAssetCreated);
        Assert.False(result.Value.CheckpointCreated);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.ExcludedTechnicalRefsTrainable);
        Assert.Contains("or-dpo-pair:or01u-sonnet-clinerules-001", result.Value.AcceptedPairIds);

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
    public async Task OrchestrationReasoningTrainer_RejectsExcludedTechnicalRefsInTrainableFeatures()
    {
        var root = NewTempRoot();
        try
        {
            var row = ReadFirstPayloadRow();
            var intentSignals = row["chosen_scenario_plan_features"]!["intent_signals"]!.AsArray();
            intentSignals.Add("exact_repo_paths");
            var (payloadPath, manifestPath) = await WritePayloadFixtureAsync(root, [row]);

            var result = await new OrchestrationReasoningTrainer().MaterializeAsync(payloadPath, manifestPath);

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
            var emptyPayloadPath = Path.Combine(root, "empty-payloads.jsonl");
            await File.WriteAllTextAsync(emptyPayloadPath, string.Empty);
            var emptyManifestPath = await WriteManifestForPayloadAsync(root, emptyPayloadPath, rowCount: 0);

            var emptyResult = await new OrchestrationReasoningTrainer().MaterializeAsync(emptyPayloadPath, emptyManifestPath);

            Assert.False(emptyResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_PAYLOAD_ROWS_REQUIRED", emptyResult.ErrorCode);

            var row = ReadFirstPayloadRow();
            row.AsObject().Remove("chosen_scenario_plan_features");
            var (missingPayloadPath, missingManifestPath) = await WritePayloadFixtureAsync(root, [row], "missing-field-payloads.jsonl");

            var missingResult = await new OrchestrationReasoningTrainer().MaterializeAsync(missingPayloadPath, missingManifestPath);

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
            var notAuthored = ReadFirstPayloadRow();
            notAuthored["dpo_payload_authored"] = false;
            var (notAuthoredPayloadPath, notAuthoredManifestPath) = await WritePayloadFixtureAsync(root, [notAuthored], "not-authored-payloads.jsonl");

            var notAuthoredResult = await new OrchestrationReasoningTrainer().MaterializeAsync(notAuthoredPayloadPath, notAuthoredManifestPath);

            Assert.False(notAuthoredResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_TRAINER_PAYLOAD_NOT_AUTHORED", notAuthoredResult.ErrorCode);

            var trainReady = ReadFirstPayloadRow();
            trainReady["dpo_train_ready"] = true;
            var (trainReadyPayloadPath, trainReadyManifestPath) = await WritePayloadFixtureAsync(root, [trainReady], "train-ready-payloads.jsonl");

            var trainReadyResult = await new OrchestrationReasoningTrainer().MaterializeAsync(trainReadyPayloadPath, trainReadyManifestPath);

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
        var materialized = await new OrchestrationReasoningTrainer().MaterializeAsync(PayloadPath, ManifestPath);
        Assert.True(materialized.IsSuccess, materialized.ErrorMessage);

        var learnedState = materialized.Value!.LearnedState;
        var pattern = learnedState.LearnedPatterns![0];
        var machine = new OrchestrationReasoningMachine(new StaticLoader(learnedState));

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

    private sealed class StaticLoader(OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(descriptor);
    }
}
