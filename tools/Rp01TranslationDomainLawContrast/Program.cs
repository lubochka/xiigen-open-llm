// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

// This finite fixture runner observes the real memory consumer; it calls no generative model.
if (args.Length is not (3 or 4) || (args.Length == 4 && args[3] != "--initialize"))
{
    Console.Error.WriteLine("Usage: <fixture.json> <frozen-state-directory> <result.json> [--initialize]");
    return 2;
}

var fixturePath = Path.GetFullPath(args[0]);
var stateDirectory = Path.GetFullPath(args[1]);
var resultPath = Path.GetFullPath(args[2]);
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};
var fixtureBytes = await File.ReadAllBytesAsync(fixturePath);
var fixtures = JsonSerializer.Deserialize<Fixture>(fixtureBytes, options)!;
var unitOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
Directory.CreateDirectory(stateDirectory);
if (args.Length == 4)
{
    // Initialization has no measured serving requests and never overwrites existing state.
    foreach (var item in fixtures.Cases)
        await InitializeAsync(item);
    Console.WriteLine($"Initialized {fixtures.Cases.Count} public fixture checkpoints; no serving measured.");
    return 0;
}
var process = Process.GetCurrentProcess();
var cpuBefore = process.TotalProcessorTime;
var allocatedBefore = GC.GetTotalAllocatedBytes();
var total = Stopwatch.StartNew();
var observations = new List<object>();

foreach (var item in fixtures.Cases)
{
    var checkpointPath = Path.Combine(stateDirectory, item.Id + ".json");
    if (!File.Exists(checkpointPath))
        throw new InvalidOperationException("Initialize frozen fixture checkpoints before measurement.");

    // Every arm restores the same saved state before using the ordinary orchestrator.
    var restore = Stopwatch.StartNew();
    var loaded = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
    if (!loaded.IsSuccess) throw new InvalidOperationException(loaded.ErrorMessage);
    var machine = loaded.Value!.Machine;
    restore.Stop();
    var checkpoint = machine.ExportCheckpoint().Value!;
    var tokens = new TranslationSemanticTokenizer().Tokenize(item.Query, "en");
    var store = new TranslationKnnStore();
    foreach (var entry in checkpoint.KnnStore.Entries)
        store.AddPair(entry.Signature, entry.Source, entry.Target);
    var cascade = store.FindNearestCascade(tokens.Signature, tokens.PrefixSignature,
        tokens.StructureSignature, 5, item.Query);
    var executorTrace = new ExecutorUnit(TranslationDirection.EN_RU, store).Execute(tokens, "general");
    var orchestrator = (await machine.GetOrchestratorAsync()).Value!;
    var request = new LogicalOrchestratorRequest(item.Id, TranslationRoutingFlow.ScenarioId,
        item.Query, new LogicalExecutionFlags(), new Dictionary<string, object>
        {
            ["source_language"] = "en", ["target_language"] = "ru", ["topic_domain"] = "general"
        });
    var allocatedAtRequest = GC.GetTotalAllocatedBytes();
    var serving = Stopwatch.StartNew();
    var result = await orchestrator.RunAsync(request);
    serving.Stop();
    var servingAllocatedBytes = GC.GetTotalAllocatedBytes() - allocatedAtRequest;
    if (!result.IsSuccess) throw new InvalidOperationException(result.ErrorMessage);
    var flow = result.Value!.FlowResult!;
    var adapterId = flow.Metrics["adapter_id"];
    var answered = !adapterId.StartsWith("bootstrap-", StringComparison.Ordinal);
    var correctAnswer = answered && item.Expected is not null && result.Value.FinalHumanResult == item.Expected;
    var badAccepted = answered && (item.Expected is null || !correctAnswer);
    observations.Add(new
    {
        item.Id, item.Query, item.Law, item.Expected,
        poolSha256 = Hash(JsonSerializer.SerializeToUtf8Bytes(item.Pool, options)),
        checkpointSha256 = Hash(await File.ReadAllBytesAsync(checkpointPath)),
        checkpointSchemaVersion = checkpoint.SchemaVersion,
        checkpointModelVersion = checkpoint.Version,
        checkpointEntries = checkpoint.KnnStore.Entries,
        tokens,
        cascade = cascade.Select(pair => new { pair.Source, pair.Target }),
        executorTrace,
        actual = result.Value.FinalHumanResult, adapterId,
        verdict = result.Value.OverallVerdict, flow.Summary, flow.Warnings,
        answered, correctAnswer, badAccepted,
        safeOutcome = item.Expected is null ? !answered : correctAnswer,
        restoreMilliseconds = restore.Elapsed.TotalMilliseconds,
        servingMilliseconds = serving.Elapsed.TotalMilliseconds,
        servingAllocatedBytes
    });
}

total.Stop();
process.Refresh();
var resultDocument = new
{
    fixtures.Protocol, fixtures.Provenance,
    fixtureSha256 = Hash(fixtureBytes),
    binarySha256 = Hash(await File.ReadAllBytesAsync(typeof(TranslationMachine).Assembly.Location)),
    runnerSha256 = Hash(await File.ReadAllBytesAsync(typeof(Fixture).Assembly.Location)),
    stateDirectory, observations,
    cost = new
    {
        totalMilliseconds = total.Elapsed.TotalMilliseconds,
        cpuMilliseconds = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
        allocatedBytes = GC.GetTotalAllocatedBytes() - allocatedBefore,
        processWorkingSetBytes = process.WorkingSet64,
        processPeakWorkingSetBytes = process.PeakWorkingSet64,
        managedHeapBytes = GC.GetTotalMemory(false),
        scope = "one finite runner process: restore, traces, and ordinary requests; initialization/build/process startup/whole-PC excluded",
        modelCalls = 0,
        thetaEffect = "NOT_MEASURED",
        interaction = "NOT_MEASURED"
    }
};
await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(resultDocument, options));
Console.WriteLine($"Observed {fixtures.Cases.Count} public fixture requests; result: {resultPath}");
return 0;

async Task InitializeAsync(FixtureCase item)
{
    var checkpointPath = Path.Combine(stateDirectory, item.Id + ".json");
    if (File.Exists(checkpointPath))
        throw new InvalidOperationException("Initialization refuses to overwrite frozen state.");
    // Only public fixed memory examples are materialized, with genuine tokenizer signatures.
    var fresh = new TranslationMachine();
    var examples = item.Pool.Select((pair, index) => new TranslationLearningExample(
        $"{item.Id}-{index}", TranslationUnitIds.ExecutorEnRu, TranslationRoutingFlow.FlowId,
        JsonSerializer.Serialize(new ExecutorInput(pair.Source, "en", "general"), unitOptions),
        JsonSerializer.Serialize(new ExecutorOutput(pair.Target, 0.9, []), unitOptions),
        JsonSerializer.Serialize(new ExecutorOutput("[WRONG: polarity contrast]", 0.1, []), unitOptions),
        "public-memory-fixture", new TranslationSemanticTokenizer().Tokenize(pair.Source, "en").Signature,
        "simulation", false)).ToArray();
    var trained = await fresh.TrainStructuredBatchAsync(new TranslationLearningBatch(
        item.Id, TranslationMachine.DomainTag, examples, ["simulation"]));
    if (!trained.IsSuccess) throw new InvalidOperationException(trained.ErrorMessage);
    var saved = await fresh.SaveCheckpointAsync(checkpointPath);
    if (!saved.IsSuccess) throw new InvalidOperationException(saved.ErrorMessage);
}

static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

internal sealed record Fixture(string Protocol, string Provenance, IReadOnlyList<FixtureCase> Cases);
internal sealed record FixtureCase(string Id, string Query, string? Expected, string Law, IReadOnlyList<MemoryPair> Pool);
internal sealed record MemoryPair(string Source, string Target);
