# 27.TranslationDomain.Tools - implementation documentation

## 1. Map for Luba

`27.TranslationDomain.Tools` is the console CLI surface for packaging, checking, publishing, loading, and continuing the training of `27.TranslationDomain` artifacts. It is not the translation model itself: the real training, checkpoint save/load, and inference live in `27.TranslationDomain`, while `Program.cs` assembles arguments, reads/writes JSON/GZip artifacts, calls model APIs, and returns an exit code.

The single `Program.cs` also hosts orchestration and skill-selection commands. For D27-B the translation commands matter: `export-translation-model`, `regenerate-translation-model-from-training-data`, `import-translation-model`, `publish-translation-model`, `load-translation-model`, `continue-translation-training`. There are currently no separate `audit-translation-corpus`, `build-translation-corpus`, or `export-translation-training-data` commands in the `Main` switch; translation training data is expected as an already-prepared `training-manifest.json` + `corpus.jsonl.gz`.

The main honest boundary: a successful CLI exit, a JSON report, a manifest, or copying a checkpoint are not model readiness by themselves. The machine-checkable phrasing for review: **CLI is not the model**; **JSON/report/registry/copy is not model readiness**. Readiness requires source-backed checkpoint load/use/continue evidence from the `TranslationMachine` path and an honest separation of exact memory from held-out/generalization. For training data, the tool reads manifest fields as JSON; the compatible contract lives in `TranslationTrainingDataContract`, but `Program.cs` in these translation paths does not call this class directly.

## 2. UML for draw.io

```mermaid
flowchart TD
    Args["CLI args<br/>command + --key value"]
    Main["Program.Main<br/>dispatch + exit codes"]
    Parser["ParseKeyValueArgs / Required<br/>argument validation"]
    Export["ExportTranslationModelAsync<br/>copy checkpoint/report + manifest"]
    Regen["RegenerateTranslationModelFromTrainingDataAsync<br/>train one committed row"]
    Import["ImportTranslationModelAsync / ProbeTranslationManifestAsync<br/>manifest + checkpoint + fixture probe"]
    Publish["PublishTranslationModelAsync<br/>models/index.json entry"]
    Load["LoadTranslationModelAsync<br/>registry -> manifest -> probe"]
    Continue["ContinueTranslationTrainingAsync<br/>load checkpoint + train new rows"]
    Domain["27.TranslationDomain<br/>TranslationMachine + contracts"]
    Files["Artifacts<br/>manifest/checkpoint/report/corpus.jsonl.gz"]

    Args --> Main
    Main --> Parser
    Main --> Export
    Main --> Regen
    Main --> Import
    Main --> Publish
    Main --> Load
    Main --> Continue
    Export --> Files
    Regen --> Domain
    Regen --> Files
    Import --> Domain
    Publish --> Files
    Load --> Files
    Continue --> Domain
    Continue --> Files
```

### Legend

| Node | What it does | Status | Where it lives |
|---|---|---|---|
| `Program.Main` | Selects a command by the first argument and returns `0`, `2`, or `3` | ✅ Complete CLI dispatch logic | `27.TranslationDomain.Tools/Program.cs` |
| `ParseKeyValueArgs` / `Required` | Parses `--key value`, throws an error on a missing/unknown shape | ✅ Complete validation helper | `27.TranslationDomain.Tools/Program.cs` |
| `ExportTranslationModelAsync` | Copies checkpoint/report to output and creates a model manifest | ✅ Complete packaging path | `27.TranslationDomain.Tools/Program.cs` |
| `RegenerateTranslationModelFromTrainingDataAsync` | Reads the training manifest/corpus, trains one committed row, writes checkpoint/report/manifest | ✅ Complete narrow regeneration path | `27.TranslationDomain.Tools/Program.cs` |
| `ImportTranslationModelAsync` / `ProbeTranslationManifestAsync` | Loads the manifest/checkpoint and runs a fixture route probe | ✅ Complete probe path | `27.TranslationDomain.Tools/Program.cs` |
| `PublishTranslationModelAsync` | Adds an entry to `models/index.json` | ✅ Complete registry write path | `27.TranslationDomain.Tools/Program.cs` |
| `LoadTranslationModelAsync` | Finds the manifest via the registry and runs import/probe | ✅ Complete registry load probe path | `27.TranslationDomain.Tools/Program.cs` |
| `ContinueTranslationTrainingAsync` | Loads the checkpoint, reads the corpus, excludes old pair ids, trains new rows | ✅ Complete continue path with an anti-replay gate | `27.TranslationDomain.Tools/Program.cs` |
| `TranslationMachine` | The real model logic, train/checkpoint/load/orchestrator | ✅ Implemented in the model project | `27.TranslationDomain/TranslationMachine.cs` |

### Connection simulations

| Connection | Example input | Branch simulation | Expected result | What must not be taken as the result |
|---|---|---|---|---|
| `Args -> Main` | `export-translation-model --checkpoint c.json --report r.json --output out` | `Main` selects `ExportTranslationModelAsync` | The command writes JSON to stdout and exit `0` on success | A single exit `0` must not be taken as proof of model quality |
| `Main -> Parser` | `--manifest` without a value | `Required`/parser throws an `ArgumentException` | `Main` prints an error to stderr and returns `3` | This is not a runtime model failure, only bad CLI input |
| `Export -> Files` | checkpoint/report exist | Files copied to `translation-v1-checkpoint.json`, `runner-report.json`, `manifest.json` | Artifact package is created | Copying does not prove fresh-load |
| `Regen -> Domain` | training manifest + corpus hash + `initial-pair-id` | A `TranslationMachine` is created, a `TranslationLearningBatch` is built, `TrainStructuredBatchAsync` is called; the manifest shape matches `TranslationTrainingDataContract`, but is read as JSON | New checkpoint/report/manifest | Training one row does not prove generalization |
| `Import -> Domain` | manifest points to a checkpoint | `ProbeTranslationManifestAsync` calls `TranslationMachine.LoadCheckpointAsync` and fixture probes | A probe report with `passed` true/false | A fixture does not replace a full product held-out |
| `Publish -> Files` | `--manifest out/manifest.json` | `PublishModelAsync` updates `models/index.json` | Registry entry for `translation-v1` | A registry entry is not model lifecycle proof |
| `Load -> Files` | `--model-id translation-v1` | Registry resolves the manifest, then import/probe | Load probe output | A registry resolve does not prove continue-training |
| `Continue -> Domain` | manifest + training-manifest + output | Load checkpoint, exclude old pair ids, train new rows, save continued checkpoint/report | `continue-report.json` and `translation-v1-continued-checkpoint.json` | A continue report does not prove quality without route/held-out evidence |

## 3. No-code algorithms

### Algorithm A: command dispatch `Program.Main(string[] args)`

- File: `27.TranslationDomain.Tools/Program.cs`.
- Signature: `Main(string[] args)`.
- Input: an array of CLI arguments; example: `["continue-translation-training", "--manifest", "models/translation-v1/manifest.json", "--training-manifest", "training-data/translation/translation-v1/training-manifest.json", "--output", "out"]`.
- Output: an integer exit code: `0` success, `2` usage error, `3` runtime error.
- Steps: if there are no arguments or there is `--help`, it prints usage; otherwise it takes `args[0].ToLowerInvariant()` and selects a handler; an unknown command prints to stderr and returns a usage error; an exception is printed to stderr, and for `continue-translation-training` and `regenerate-translation-model-from-training-data` the full exception is printed.
- What it does not do: it does not run the model itself; it only selects a handler.

### Algorithm B: argument validation `ParseKeyValueArgs` / `Required`

- File: `27.TranslationDomain.Tools/Program.cs`.
- Signatures: `ParseKeyValueArgs(IReadOnlyList<string> args, IReadOnlyCollection<string> flagNames)`, `Required(IReadOnlyDictionary<string,string> values, string key)`.
- Input: `--name value` pairs, optional flag names.
- Output: `ParsedArgs` or an exception.
- Steps: checks that each key starts with `--`; puts flag names into `Flags`; a normal key requires the next value; `Required` requires a non-empty value.
- What it does not do: it does not validate the semantics of the checkpoint/manifest; the handlers and contracts do that.

### Algorithm C: export artifact package `ExportTranslationModelAsync`

- Signature: `ExportTranslationModelAsync(IReadOnlyList<string> args)`.
- Parameters: `--checkpoint`, `--report`, `--output`.
- Input/output: input checkpoint/report files -> output directory with `translation-v1-checkpoint.json`, `manifest.json`, `runner-report.json`.
- Steps: copies checkpoint/report; reads the metric from the report (`best_runtime_product_accuracy`, `accuracy_at_export`, `runtime_product_accuracy`, `target_accuracy`); creates a `TranslationModelManifestContract`; checks `IsValidSharedAssetManifest`; writes the manifest; stdout contains paths and model id.
- What it does not do: it does not check fresh-load or route behavior; it packages already-existing artifacts.

### Algorithm D: regenerate from committed training data

- Signature: `RegenerateTranslationModelFromTrainingDataAsync(IReadOnlyList<string> args)`.
- Parameters: `--training-manifest`, `--output`, `--initial-pair-id`.
- Input/output: `training-manifest.json` + `corpus.jsonl.gz` -> checkpoint/report/manifest.
- Steps: reads the manifest, verifies the corpus sha256, finds exactly one `initial-pair-id` row, creates a `TranslationMachine`, builds a `TranslationLearningBatch`, calls `TrainStructuredBatchAsync`, saves the checkpoint via `SaveCheckpointAsync`, verifies the persisted structured identity, writes the report.
- What it does not do: it does not train the whole corpus; this is a narrow regeneration path for one committed row.

### Algorithm E: import/load probe `ImportTranslationModelAsync`, `LoadTranslationModelAsync`, `ProbeTranslationManifestAsync`

- Signatures: `ImportTranslationModelAsync(IReadOnlyList<string> args)`, `LoadTranslationModelAsync(IReadOnlyList<string> args)`, `ProbeTranslationManifestAsync(string manifestPath)`.
- Parameters: import takes `--manifest`; load takes `--model-id`.
- Input/output: manifest/registry -> probe JSON to stdout.
- Steps: the manifest path resolves the checkpoint; the checkpoint JSON is deserialized; `TranslationMachine.LoadCheckpointAsync` does a fresh load; the orchestrator runs an exact fixture and a non-exact fixture; passed requires manifest/model id, structured training, a non-empty KNN store, a non-bootstrap exact fixture, and a non-exact generalization probe.
- What it does not do: it does not publish the model and does not train it.

### Algorithm F: publish registry `PublishTranslationModelAsync`

- Signature: `PublishTranslationModelAsync(IReadOnlyList<string> args)`.
- Parameters: `--manifest`.
- Input/output: manifest -> `models/index.json`.
- Steps: calls `PublishModelAsync` with `TranslationModelManifestContract.ModelId`, `Domain`, `Version`; the registry entry gets the manifest path, version, accuracy.
- What it does not do: it does not load a checkpoint and does not check inference; a registry row is not equal to a ready model.

### Algorithm G: continue training `ContinueTranslationTrainingAsync`

- Signature: `ContinueTranslationTrainingAsync(IReadOnlyList<string> args)`.
- Parameters: `--manifest`, `--training-manifest`, `--output`, optional `--max-cycles`.
- Input/output: manifest + checkpoint + training manifest/corpus -> `translation-v1-continued-checkpoint.json` and `continue-report.json`.
- Steps: resolves checkpoint from manifest; reads training manifest and corpus path; verifies corpus sha256; loads checkpoint through `TranslationMachine.LoadCheckpointAsync`; exports before state; reads old structured identities; selects new rows excluding old pair ids; if replay proof is unavailable or overlaps, writes failed `continue-report.json`; otherwise builds `TranslationLearningBatch`, calls `TrainStructuredBatchAsync`, saves continued checkpoint, exports after state, builds route evidence, and returns success only when rows trained, no replay proven, and exact-memory leakage check passes.
- What it does not do: does not claim product quality by report alone; route evidence is a probe, not broad held-out evaluation.

### Algorithm G2: route evidence helpers for the continue probe

- File: `27.TranslationDomain.Tools/Program.cs`.
- Signatures: `BuildTranslationRouteEvidenceAsync(TranslationMachine machine)`, `RunTranslationRouteProbeAsync(ILogicalOrchestrator orchestrator, string source)`.
- Input: an already-loaded/continued `TranslationMachine`; inside, the helper requests the orchestrator and runs two source prompts: exact `add null check ValidateToken` and non-exact `add a null check before ValidateToken`.
- Output: `Dictionary<string, object?>` with `route_probe_available`, `route_ids`, exact/non-exact adapter ids, output, and `exact_memory_leakage_proven`; a separate helper returns `TranslationRouteProbeResult`.
- Steps: if the orchestrator is unavailable, returns route evidence with `route_probe_available=false`; otherwise runs the exact and non-exact probes, reads `adapter_id`, counts exact-source hits, checks that the non-exact probe is successful, not bootstrap, and not exact-source. These are support helpers for the `continue-translation-training` report, not separate CLI commands.
- What it does not do: it does not download data, does not train the model, and does not prove full held-out/product quality; it is a narrow route-evidence check so that exact-memory replay is not hidden under the guise of generalization.

### Algorithm H: translation training corpus commands

- Current status: there is no separate `audit-translation-corpus`, `build-translation-corpus`, or `export-translation-training-data` command in the `Main` switch.
- How the data still passes through: `regenerate-translation-model-from-training-data` and `continue-translation-training` require a ready `training-manifest.json`, read `corpus_filename`, verify `corpus_sha256`, then read `corpus.jsonl.gz`; this must be a manifest compatible with `TranslationTrainingDataContract`, although the current tool path accesses the fields through a dictionary.
- What it does not do: the CLI does not build a translation corpus from private pairs in a separate callable command path; if this is needed, it is a separate work order.

## 4. Exact quotations from the code

### The dispatcher explicitly contains translation commands

`27.TranslationDomain.Tools/Program.cs:22-79`

```csharp

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            WriteUsage();
            return args.Length == 0 ? UsageError : Success;
        }

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "audit-orchestration-corpus" => await AuditOrchestrationCorpusAsync(args.Skip(1).ToArray()),
                "build-orchestration-corpus" => await BuildOrchestrationCorpusAsync(args.Skip(1).ToArray()),
                "build-skill-selection-corpus" => await BuildSkillSelectionCorpusAsync(args.Skip(1).ToArray()),
                "run-skill-selection-loop" => await RunSkillSelectionLoopAsync(args.Skip(1).ToArray()),
                "run-codex-smart-skills-trace" => await RunCodexSmartSkillsTraceAsync(args.Skip(1).ToArray()),
                "regenerate-skill-selection-model-from-training-data" => await RegenerateSkillSelectionModelFromTrainingDataAsync(args.Skip(1).ToArray()),
                "export-skill-selection-model" => await ExportSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "import-skill-selection-model" => await ImportSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "publish-skill-selection-model" => await PublishSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "load-skill-selection-model" => await LoadSkillSelectionModelAsync(args.Skip(1).ToArray()),
                "continue-skill-selection-training" => await ContinueSkillSelectionTrainingAsync(args.Skip(1).ToArray()),
                "run-orchestration-loop" => await RunOrchestrationLoopAsync(args.Skip(1).ToArray()),
                "export-translation-model" => await ExportTranslationModelAsync(args.Skip(1).ToArray()),
                "regenerate-translation-model-from-training-data" => await RegenerateTranslationModelFromTrainingDataAsync(args.Skip(1).ToArray()),
                "import-translation-model" => await ImportTranslationModelAsync(args.Skip(1).ToArray()),
                "export-orchestration-model" => await ExportOrchestrationModelAsync(args.Skip(1).ToArray()),
                "export-orchestration-training-data" => await ExportOrchestrationTrainingDataAsync(args.Skip(1).ToArray()),
                "import-orchestration-model" => await ImportOrchestrationModelAsync(args.Skip(1).ToArray()),
                "publish-translation-model" => await PublishTranslationModelAsync(args.Skip(1).ToArray()),
                "publish-orchestration-model" => await PublishOrchestrationModelAsync(args.Skip(1).ToArray()),
                "load-translation-model" => await LoadTranslationModelAsync(args.Skip(1).ToArray()),
                "load-orchestration-model" => await LoadOrchestrationModelAsync(args.Skip(1).ToArray()),
                "continue-translation-training" => await ContinueTranslationTrainingAsync(args.Skip(1).ToArray()),
                "continue-orchestration-training" => await ContinueOrchestrationTrainingAsync(args.Skip(1).ToArray()),
                "materialize-orchestration-reasoning-state" => await MaterializeOrchestrationReasoningStateAsync(args.Skip(1).ToArray()),
                "export-orchestration-reasoning-model" => await ExportOrchestrationReasoningModelAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-fresh-load" => await ProbeOrchestrationReasoningFreshLoadAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-continued-fresh-load" => await ProbeOrchestrationReasoningContinuedFreshLoadAsync(args.Skip(1).ToArray()),
                "continue-orchestration-reasoning-training" => await ContinueOrchestrationReasoningTrainingAsync(args.Skip(1).ToArray()),
                "export-orchestration-reasoning-continued-model" => await ExportOrchestrationReasoningContinuedModelAsync(args.Skip(1).ToArray()),
                "probe-orchestration-reasoning-held-out-guide-patterns" => await ProbeOrchestrationReasoningHeldOutGuidePatternsAsync(args.Skip(1).ToArray()),
                _ => UnknownCommand(args[0])
            };
        }
        catch (Exception ex)
        {
            if (args.Length > 0 &&
                (string.Equals(args[0], "continue-translation-training", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(args[0], "regenerate-translation-model-from-training-data", StringComparison.OrdinalIgnoreCase)))
                Console.Error.WriteLine(ex);
            else
                Console.Error.WriteLine(ex.Message);
            return RuntimeError;
        }
    }
```

Why it matters: this is the list of actually callable commands. There is no translation-specific corpus audit/build command here, but there are export/import/publish/load/continue paths.

### Export copies checkpoint/report and creates a manifest

`27.TranslationDomain.Tools/Program.cs:1942-1979`

```csharp
    private static async Task<int> ExportTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var checkpointPath = Required(values, "checkpoint");
        var outputDir = Required(values, "output");
        var reportPath = Required(values, "report");
        Directory.CreateDirectory(outputDir);

        var checkpointFile = Path.Combine(outputDir, TranslationModelManifestContract.CheckpointFilename);
        var manifestFile = Path.Combine(outputDir, "manifest.json");
        var runnerReportFile = Path.Combine(outputDir, TranslationModelManifestContract.RunnerReportRef);
        File.Copy(checkpointPath, checkpointFile, overwrite: true);
        File.Copy(reportPath, runnerReportFile, overwrite: true);

        var accuracy = ReadFirstReportMetric(
            reportPath,
            "best_runtime_product_accuracy",
            "accuracy_at_export",
            "runtime_product_accuracy",
            "target_accuracy");
        var manifest = TranslationModelManifestContract.Create(
            accuracyAtExport: accuracy,
            sourceBaseCommit: ReadGitHead(),
            createdAtUtc: DateTimeOffset.UtcNow);
        if (!TranslationModelManifestContract.IsValidSharedAssetManifest(manifest))
            throw new InvalidOperationException("Translation model manifest contract validation failed.");
        await WriteJsonFileAsync(manifestFile, manifest);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "export-translation-model",
            manifest = manifestFile,
            checkpoint = checkpointFile,
            runner_report = runnerReportFile,
            model_id = TranslationModelManifestContract.ModelId
        }, ToolJsonOptions));
        return Success;
    }
```

Why it matters: the export command is packaging. It copies existing files and validates manifest shape; it does not train or fresh-load.

### Regenerate verifies the training manifest hash and trains one committed row

`27.TranslationDomain.Tools/Program.cs:1981-2020`

```csharp
    private static async Task<int> RegenerateTranslationModelFromTrainingDataAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        var initialPairId = Required(values, "initial-pair-id");
        Directory.CreateDirectory(outputDir);

        var trainingManifest = await ReadJsonDictionaryAsync(trainingManifestPath);
        var corpusFilename = Convert.ToString(trainingManifest["corpus_filename"], CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Translation training manifest is missing corpus_filename.");
        var expectedCorpusSha = Convert.ToString(trainingManifest["corpus_sha256"], CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Translation training manifest is missing corpus_sha256.");
        var corpusPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))!, corpusFilename);
        var actualCorpusSha = Sha256File(corpusPath);
        if (!string.Equals(expectedCorpusSha, actualCorpusSha, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Translation training corpus hash mismatch.");

        var corpusRows = await ReadTranslationCorpusRowsAsync(corpusPath);
        var initialRows = corpusRows
            .Where(row => string.Equals(row.PairId, initialPairId, StringComparison.Ordinal))
            .ToArray();
        if (initialRows.Length != 1)
            throw new InvalidOperationException($"Expected exactly one committed corpus row for '{initialPairId}', found {initialRows.Length}.");

        var machine = new TranslationMachine();
        var examples = initialRows.Select(BuildTranslationContinueExample).ToArray();
        var batch = new TranslationLearningBatch(
            BatchId: "translation-v1-regenerate-001",
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: initialRows.Select(row => row.PairId).ToArray());
        var train = await machine.TrainStructuredBatchAsync(batch);
        if (!train.IsSuccess || train.Value is null)
            throw new InvalidOperationException(train.ErrorMessage);

        var checkpointFile = Path.Combine(outputDir, TranslationModelManifestContract.CheckpointFilename);
        var save = await machine.SaveCheckpointAsync(checkpointFile);
        if (!save.IsSuccess)
            throw new InvalidOperationException(save.ErrorMessage);
```

Why it matters: here the CLI actually calls `TranslationMachine.TrainStructuredBatchAsync` and `SaveCheckpointAsync`; readiness is still limited to one selected row.

### Publish only writes a registry entry

`27.TranslationDomain.Tools/Program.cs:2197-2213`

```csharp
    private static async Task<int> PublishTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifest = Required(values, "manifest");
        await PublishModelAsync(
            TranslationModelManifestContract.ModelId,
            manifest,
            TranslationModelManifestContract.Domain,
            TranslationModelManifestContract.Version);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            command = "publish-translation-model",
            model_id = TranslationModelManifestContract.ModelId,
            manifest = ToRepoRelativePath(manifest)
        }, ToolJsonOptions));
        return Success;
    }
```

Why it matters: publish writes model registry metadata. It does not load a checkpoint and does not prove translation quality.

### PublishModelAsync writes the registry file, not a model

`27.TranslationDomain.Tools/Program.cs:3478-3492`

```csharp
    private static async Task PublishModelAsync(string modelId, string manifestPath, string domain, string registryVersion)
    {
        var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "models", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
        var entries = File.Exists(indexPath)
            ? JsonSerializer.Deserialize<ModelIndex>(await File.ReadAllTextAsync(indexPath), ToolJsonOptions)?.Models.ToList() ?? []
            : [];
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var accuracy = manifest.TryGetValue("accuracy_at_export", out var value) && value is JsonElement element && element.ValueKind == JsonValueKind.Number
            ? element.GetDouble()
            : 0.0;
        entries.RemoveAll(entry => string.Equals(entry.ModelId, modelId, StringComparison.Ordinal));
        entries.Add(new ModelIndexEntry(modelId, domain, ToRepoRelativePath(manifestPath), registryVersion, accuracy, DateTimeOffset.UtcNow));
        await WriteJsonFileAsync(indexPath, new ModelIndex(entries.OrderBy(entry => entry.ModelId, StringComparer.Ordinal).ToArray()));
    }
```

Why it matters: the helper updates `models/index.json` and takes accuracy from the manifest, but does not call `TranslationMachine.LoadCheckpointAsync`; registry/copy is not model readiness.

### ResolveModelManifestAsync resolves registry metadata only

`27.TranslationDomain.Tools/Program.cs:3494-3505`

```csharp
    private static async Task<string> ResolveModelManifestAsync(string modelId)
    {
        var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "models", "index.json");
        if (!File.Exists(indexPath))
            throw new FileNotFoundException("Model index not found.", indexPath);
        var index = JsonSerializer.Deserialize<ModelIndex>(await File.ReadAllTextAsync(indexPath), ToolJsonOptions)
                    ?? new ModelIndex([]);
        var entry = index.Models.FirstOrDefault(item => string.Equals(item.ModelId, modelId, StringComparison.Ordinal));
        if (entry is null)
            throw new InvalidOperationException($"Model id '{modelId}' not found.");
        return Path.Combine(Directory.GetCurrentDirectory(), entry.ManifestPath);
    }
```

Why it matters: `load-translation-model` gets the manifest path from the registry; this is only a lookup, not model readiness.

### ParseKeyValueArgs enforces CLI shape

`27.TranslationDomain.Tools/Program.cs:3644-3668`

```csharp
    private static ParsedArgs ParseKeyValueArgs(IReadOnlyList<string> args, IReadOnlyCollection<string> flagNames)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Count; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Unexpected argument '{key}'.");

            var name = key[2..];
            if (flagNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                flags.Add(name);
                continue;
            }

            if (i + 1 >= args.Count)
                throw new ArgumentException($"Missing value for '{key}'.");

            values[name] = args[++i];
        }

        return new ParsedArgs(values, flags);
    }
```

Why it matters: the parser proves only the shape of the CLI arguments; it does not check the quality of the checkpoint, registry, or model.

### Load resolves the registry then runs a manifest probe

`27.TranslationDomain.Tools/Program.cs:2353-2363`

```csharp
    private static async Task<int> LoadTranslationModelAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var modelId = Required(values, "model-id");
        var manifest = await ResolveModelManifestAsync(modelId);
        var probe = await ProbeTranslationManifestAsync(manifest);
        probe["command"] = "load-translation-model";
        probe["registry_resolved_manifest"] = ToRepoRelativePath(manifest);
        Console.WriteLine(JsonSerializer.Serialize(probe, ToolJsonOptions));
        return (bool)probe["passed"]! ? Success : RuntimeError;
    }
```

Why it matters: load is registry + probe, not training.

### Continue training verifies corpus hash and anti-replay before training

`27.TranslationDomain.Tools/Program.cs:2776-2830`

```csharp
    private static async Task<int> ContinueTranslationTrainingAsync(IReadOnlyList<string> args)
    {
        var values = ParseKeyValueArgs(args, []).Values;
        var manifestPath = Required(values, "manifest");
        var trainingManifestPath = Required(values, "training-manifest");
        var outputDir = Required(values, "output");
        Directory.CreateDirectory(outputDir);

        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var modelManifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointPath = Path.Combine(manifestDir, Convert.ToString(modelManifest["checkpoint_filename"], CultureInfo.InvariantCulture)!);
        var trainingManifest = await ReadJsonDictionaryAsync(trainingManifestPath);
        var corpusPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(trainingManifestPath))!, Convert.ToString(trainingManifest["corpus_filename"], CultureInfo.InvariantCulture)!);
        var expectedHash = Convert.ToString(trainingManifest["corpus_sha256"], CultureInfo.InvariantCulture);
        var actualHash = Sha256File(corpusPath);
        if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Translation training corpus hash mismatch.");

        var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);

        var loaded = load.Value ?? throw new InvalidOperationException("Translation checkpoint load returned success with null value.");
        var machine = loaded.Machine ?? throw new InvalidOperationException("Translation checkpoint load returned null machine.");
        var beforeExport = machine.ExportCheckpoint();
        if (!beforeExport.IsSuccess || beforeExport.Value is null)
            throw new InvalidOperationException($"Translation checkpoint export before continue failed: {beforeExport.ErrorMessage}");
        var before = beforeExport.Value;
        IReadOnlyList<TranslationTrainingHistoryEntryCheckpoint> beforeHistory = before.TrainingHistory ?? Array.Empty<TranslationTrainingHistoryEntryCheckpoint>();
        IReadOnlyList<TranslationKnnStoreEntryCheckpoint> beforeKnnEntries = before.KnnStore?.Entries ?? Array.Empty<TranslationKnnStoreEntryCheckpoint>();
        IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint> beforeIdentities =
            before.StructuredTrainingIdentities ?? Array.Empty<TranslationStructuredTrainingIdentityCheckpoint>();
        var oldPairIds = beforeIdentities
            .Select(identity => identity.ExampleId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var oldPairIdSet = oldPairIds.ToHashSet(StringComparer.Ordinal);
        var pairReplayProofAvailable = oldPairIds.Length > 0;
        var rows = await ReadTranslationCorpusRowsAsync(corpusPath);
        if (rows.Count == 0)
            throw new InvalidOperationException("Translation training corpus contains no rows.");
        var maxRows = Math.Max(1, IntValue(values, "max-cycles", 1)) * 25;
        var selectedRows = rows
            .Where(row => !pairReplayProofAvailable || !oldPairIdSet.Contains(row.PairId))
            .Take(maxRows)
            .ToArray();
        if (selectedRows.Length == 0)
            throw new InvalidOperationException("Translation continue-training selected no new rows.");
        var newPairIds = selectedRows.Select(row => row.PairId).ToArray();
        var noReplayProven = pairReplayProofAvailable &&
                             newPairIds.All(pairId => !oldPairIdSet.Contains(pairId));
        if (!noReplayProven)
        {
```

Why it matters: the continue path refuses to claim no-replay without structured identities; this is an anti-masquerade guard.

### Route evidence helpers separate exact memory from a non-exact probe

`27.TranslationDomain.Tools/Program.cs:3405-3450`

```csharp
    private static async Task<Dictionary<string, object?>> BuildTranslationRouteEvidenceAsync(TranslationMachine machine)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        if (!orchestrator.IsSuccess || orchestrator.Value is null)
            return new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["route_probe_available"] = false,
                ["route_probe_error"] = orchestrator.ErrorMessage,
                ["exact_memory_leakage_proven"] = false
            };

        var exactProbe = await RunTranslationRouteProbeAsync(orchestrator.Value, "add null check ValidateToken");
        var nonExactProbe = await RunTranslationRouteProbeAsync(orchestrator.Value, "add a null check before ValidateToken");
        var routeIds = new[] { exactProbe.AdapterId, nonExactProbe.AdapterId }
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var exactSourceHitCount = new[] { exactProbe, nonExactProbe }.Count(probe => probe.ExactSourceHit);
        var nonExactProbeProvesGeneralization =
            nonExactProbe.Success &&
            !nonExactProbe.BootstrapOutput &&
            !nonExactProbe.ExactSourceHit;
        var exactMemoryLeakageProven = exactProbe.Success && nonExactProbeProvesGeneralization;
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["route_probe_available"] = true,
            ["route_ids"] = routeIds,
            ["exact_source_hit_count"] = exactSourceHitCount,
            ["exact_probe_source"] = exactProbe.Source,
            ["exact_probe_adapter_id"] = exactProbe.AdapterId,
            ["exact_probe_exact_source_hit"] = exactProbe.ExactSourceHit,
            ["exact_probe_bootstrap_output"] = exactProbe.BootstrapOutput,
            ["exact_probe_output"] = exactProbe.Output,
            ["non_exact_probe_source"] = nonExactProbe.Source,
            ["non_exact_probe_adapter_id"] = nonExactProbe.AdapterId,
            ["non_exact_probe_exact_source_hit"] = nonExactProbe.ExactSourceHit,
            ["non_exact_probe_bootstrap_output"] = nonExactProbe.BootstrapOutput,
            ["non_exact_probe_output"] = nonExactProbe.Output,
            ["non_exact_probe_proves_generalization"] = nonExactProbeProvesGeneralization,
            ["exact_memory_leakage_status"] = exactMemoryLeakageProven
                ? "CONTROLLED: exact-source is labeled separately and non-exact probe did not use exact-source."
                : "FAILED: route evidence does not prove non-exact generalization separate from exact-source memory.",
            ["exact_memory_leakage_proven"] = exactMemoryLeakageProven
        };
    }
```

`27.TranslationDomain.Tools/Program.cs:3452-3476`

```csharp
    private static async Task<TranslationRouteProbeResult> RunTranslationRouteProbeAsync(
        ILogicalOrchestrator orchestrator,
        string source)
    {
        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-continue-route-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));
        var output = result.Value?.FinalHumanResult ?? string.Empty;
        var adapterId = ReadFlowMetric(result.Value?.FlowResult, "adapter_id");
        return new TranslationRouteProbeResult(
            Source: source,
            Success: result.IsSuccess,
            Output: output,
            AdapterId: adapterId,
            ExactSourceHit: adapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase),
            BootstrapOutput: output.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase));
    }
```

Why it matters: these helpers add evidence to the continue report and separate the exact-source route from the non-exact probe. They are not a separate command and do not prove broad model readiness.

### Import/load probe checks the checkpoint, KNN, and non-exact route

`27.TranslationDomain.Tools/Program.cs:2941-3022`

```csharp
    private static async Task<Dictionary<string, object?>> ProbeTranslationManifestAsync(string manifestPath)
    {
        var manifestDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        var manifest = await ReadJsonDictionaryAsync(manifestPath);
        var checkpointFilename = Convert.ToString(manifest["checkpoint_filename"], CultureInfo.InvariantCulture)!;
        var checkpointPath = Path.Combine(manifestDir, checkpointFilename);
        var checkpointJson = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8);
        var checkpoint = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
            checkpointJson,
            TranslationCheckpointJsonOptions.Standard)
            ?? throw new InvalidOperationException("Translation checkpoint deserialized null.");

        var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
        if (!load.IsSuccess)
            throw new InvalidOperationException(load.ErrorMessage);

        var orchestrator = await load.Value!.Machine.GetOrchestratorAsync();
        if (!orchestrator.IsSuccess)
            throw new InvalidOperationException(orchestrator.ErrorMessage);

        var fixtureSource = "add null check ValidateToken";
        var fixture = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-import-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: fixtureSource,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));

        var fixtureOutput = fixture.Value?.FinalHumanResult ?? string.Empty;
        var expectedFixtureOutput = "Add a null check to ValidateToken.";
        var bootstrapOutput = fixtureOutput.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
        var fixtureAdapterId = ReadFlowMetric(fixture.Value?.FlowResult, "adapter_id");
        var fixtureExactSourceHit = fixtureAdapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase);

        var nonExactFixtureSource = "add a null check before ValidateToken";
        var nonExactFixture = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"translation-import-non-exact-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: nonExactFixtureSource,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "technical"
            }));
        var nonExactFixtureOutput = nonExactFixture.Value?.FinalHumanResult ?? string.Empty;
        var nonExactBootstrapOutput = nonExactFixtureOutput.Contains(":BOOTSTRAP]", StringComparison.OrdinalIgnoreCase);
        var nonExactFixtureAdapterId = ReadFlowMetric(nonExactFixture.Value?.FlowResult, "adapter_id");
        var nonExactFixtureExactSourceHit = nonExactFixtureAdapterId.Contains("exact-source", StringComparison.OrdinalIgnoreCase);
        var nonExactFixtureProvesGeneralization =
            nonExactFixture.IsSuccess &&
            !nonExactBootstrapOutput &&
            !nonExactFixtureExactSourceHit;
        var manifestLoaded = string.Equals(
            Convert.ToString(manifest["model_id"], CultureInfo.InvariantCulture),
            TranslationModelManifestContract.ModelId,
            StringComparison.Ordinal) &&
            string.Equals(
                Convert.ToString(manifest["loader_type"], CultureInfo.InvariantCulture),
                TranslationModelManifestContract.LoaderType,
                StringComparison.Ordinal);
        var passed = manifestLoaded &&
                     load.IsSuccess &&
                     checkpoint.HasStructuredTraining &&
                     checkpoint.KnnStore.Entries.Count > 0 &&
                     fixture.IsSuccess &&
                     !bootstrapOutput &&
                     string.Equals(fixtureOutput, expectedFixtureOutput, StringComparison.Ordinal) &&
                     nonExactFixtureProvesGeneralization;

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["command"] = "import-translation-model",
            ["model_id"] = manifest["model_id"],
            ["manifest_loaded"] = manifestLoaded,
            ["checkpoint_loaded"] = load.IsSuccess,
```

Why it matters: this is the strongest CLI readiness probe in the tool. It still remains a fixture probe, not full product held-out coverage.

### csproj wiring confirms this is an executable tooling project

`27.TranslationDomain.Tools/27.TranslationDomain.Tools.csproj:3-13`

```xml
  <ItemGroup>
    <ProjectReference Include="..\27.TranslationDomain\27.TranslationDomain.csproj" />
    <ProjectReference Include="..\42.ProjectWikiDomain\42.ProjectWikiDomain.csproj" />
    <ProjectReference Include="..\43.SmartSkillsDomain\43.SmartSkillsDomain.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
```

Why it matters: the tool project directly references the model/domain projects and builds as an executable, not as a library model.

## 5. Files and data flow

| Command | Input files | Output files | Real boundary |
|---|---|---|---|
| `export-translation-model` | `--checkpoint`, `--report` | `manifest.json`, `translation-v1-checkpoint.json`, `runner-report.json` | Packaging only |
| `regenerate-translation-model-from-training-data` | `training-manifest.json`, `corpus.jsonl.gz`, `--initial-pair-id` | checkpoint, manifest, runner report | Calls `TranslationMachine.TrainStructuredBatchAsync` |
| `import-translation-model` | `--manifest` + manifest-relative checkpoint | stdout probe JSON | Calls `TranslationMachine.LoadCheckpointAsync` and orchestrator fixtures |
| `publish-translation-model` | `--manifest` | `models/index.json` | Registry update only |
| `load-translation-model` | `--model-id` | stdout probe JSON | Registry resolve + manifest probe |
| `continue-translation-training` | manifest, checkpoint, training manifest, corpus | `translation-v1-continued-checkpoint.json`, `continue-report.json` | Fresh load + new-row training + anti-replay report |
| route evidence helpers | loaded `TranslationMachine` inside continue path | route fields inside `continue-report.json` | Support probe only: exact/non-exact route evidence, not standalone CLI/model readiness |
| translation corpus audit/build | No separate callable command | None | Known documentation/CLI gap |

## 6. QA / test mapping

The tests were not run in D5B. A focused search over `export-translation-model`, `continue-translation-training`, `load-translation-model`, `publish-translation-model`, `regenerate-translation-model-from-training-data` in `Tests` found no direct tests for `Program.cs` CLI commands. Existing D5A tests for `27.TranslationDomain` cover model-side checkpoint/load/continue concepts, but do not prove CLI stdout/stderr/exit-code behavior.

| Behavior | Evidence now | Status |
|---|---|---|
| Command dispatch | Source quote `Program.Main` | Source-covered, not runtime-tested in D5B |
| Manifest/checkpoint packaging | Source quote `ExportTranslationModelAsync` | Source-covered, no CLI test run |
| Regenerate from training data | Source quote and model API dependency | Source-covered, no CLI test run |
| Import/load fixture probe | Source quote `ProbeTranslationManifestAsync` | Source-covered, fixture scope only |
| Continue training anti-replay | Source quote `ContinueTranslationTrainingAsync` | Source-covered, no CLI test run |
| Translation corpus audit/build command | Absent in `Main` switch | Gap, needs future CLI work order if required |

## 7. Anti-false-done checks

- CLI exit `0` is not product quality by itself.
- CLI is not the model.
- JSON/report/registry/copy is not model readiness.
- A JSON report, `manifest.json`, `models/index.json`, a PowerShell wrapper, or a copied checkpoint is evidence only, not a substitute for model readiness.
- `publish-translation-model` writes registry metadata; it does not load or evaluate the model.
- `export-translation-model` packages existing artifacts; it does not prove the checkpoint can fresh-load.
- `import-translation-model` / `load-translation-model` run fixture probes; fixture probes are useful but narrower than full held-out/product evaluation.
- `continue-translation-training` writes `continue-report.json`; the report must be read with its anti-replay fields and route evidence, not treated as blanket quality.
- Missing translation-specific corpus audit/build commands must not be hidden by orchestration commands with similar names.

## 8. Current status

| Component | File | Status | What actually works now |
|---|---|---|---|
| `Program.Main` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete | Dispatches translation/orchestration/skill commands; unknown command returns usage error |
| `ExportTranslationModelAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete | Copies checkpoint/report, writes manifest, prints paths |
| `RegenerateTranslationModelFromTrainingDataAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete narrow path | Trains one committed row from corpus and writes checkpoint/report/manifest |
| `ImportTranslationModelAsync` / `ProbeTranslationManifestAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete fixture probe | Loads checkpoint and checks exact/non-exact fixture probes |
| `PublishTranslationModelAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete registry path | Updates model index through `PublishModelAsync` |
| `LoadTranslationModelAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete registry probe | Resolves model id to manifest and reuses import probe |
| `ContinueTranslationTrainingAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Complete guarded continue path | Loads checkpoint, verifies corpus hash, excludes old pair ids, trains new rows, writes continued checkpoint/report |
| `BuildTranslationRouteEvidenceAsync` / `RunTranslationRouteProbeAsync` | `27.TranslationDomain.Tools/Program.cs` | ✅ Support helpers for the continue report | Runs exact/non-exact route probes and records exact-memory leakage fields; not standalone CLI/model readiness |
| Translation corpus audit/build CLI | `27.TranslationDomain.Tools/Program.cs` | ⚠️ Absent | No `audit-translation-corpus` / `build-translation-corpus` / `export-translation-training-data` command in `Main` switch |

Documentation status: D5B documented - parent review passed  
Last verified against source: 2026-06-08  
Known gaps: direct CLI tests not run/found; translation-specific corpus audit/build command absent.
