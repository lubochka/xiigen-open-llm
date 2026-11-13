# JSON-OPTIONS-REUSE-01 — точные исходники экспериментального host

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->


Статус: авторские исходники, native сборка и обучение не выполнялись. Python-файлы только разбирались через ast.parse; это не проверка runtime. Прочитать полную карточку JSON-OPTIONS-REUSE-01.md. Сохранить четыре блока как названные файлы в C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/. Единственная команда запуска — C:/Python314/python.exe <полный host>/run.py; literal path находится в карточке. Не запускать при документном поручении. При «переходи» сразу NEXT без доделки прежнего опыта; при «остановить всё» прекратить действия, допустимо лишь завершить собственный запущенный дочерний процесс.

## prepare.py

```python
"""Prepare only the isolated JSON-OPTIONS-REUSE-01 experiment; never train."""
import argparse, difflib, hashlib, json, pathlib, shutil, subprocess
PIN = "66947785d1318f7cbbc88118f3156cdc75001e16"
SOURCE = {
"TranslationMachine.cs":"fd0e736de3739236b4dde2d6b9d000f5ebae8416",
"TranslationTrainedUnits.cs":"1742abfc9fb617dfc66e4d618e0c169da01a6ccc",
"TranslationUnitContracts.cs":"0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8",
"TranslationContracts.cs":"9ccff62783661fab24131e206b42280fe4cdc063",
"TranslationCheckpointContracts.cs":"3879805b0504df7718adb2760e86aa6b4c3b13ab",
"TranslationDirectionAdapters.cs":"ce139d39cfab36b7e919c31398cacde225b952d5",
"TranslationRoutingFlow.cs":"ae9da139a68b9e6d6cfd655425b0327053e06f0d",
"TranslationSemanticTokenizer.cs":"ee6d81c7933fa33a9c57a2111990019ec55e5dab",
"TranslationMemorySourceLaw.cs":"ba841f4f1839d504444d214feb73c80a4cc24f32",
}
DATA_ROOT = "docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/"
a = argparse.ArgumentParser()
a.add_argument("--repo", required=True)
a.add_argument("--out", required=True)
args = a.parse_args()
repo, out = pathlib.Path(args.repo), pathlib.Path(args.out)
assets = pathlib.Path(__file__).resolve().parent
if out.exists():
    raise SystemExit("BLOCKED_EXISTING_OUTPUT: preserve it; no delete, overwrite or automatic retry.")
native = repo / "27.TranslationDomain/bin/Release/net9.0"
for name in ("27.TranslationDomain.dll", "27.TranslationDomain.pdb"):
    if not (native / name).is_file():
        raise SystemExit("BLOCKED_ENVIRONMENT: missing existing native " + str(native / name))
for name in ("Program.cs", "compare.py", "run.py"):
    if not (assets / name).is_file():
        raise SystemExit("BLOCKED_PREPARATION: exact authored asset missing: " + name)
def git(*items):
    return subprocess.check_output(["git", "-C", str(repo), *items])
def sha(b):
    return hashlib.sha256(b).hexdigest()
def blob(b):
    return hashlib.sha1(b"blob " + str(len(b)).encode() + b"\0" + b).hexdigest()
source = {}
for name, expected in SOURCE.items():
    data = git("show", PIN + ":27.TranslationDomain/" + name)
    if blob(data) != expected:
        raise SystemExit("BLOCKED_SOURCE_IDENTITY: " + name)
    source[name] = data
inputs = {}
for name in ("training-batch-B.json", "eval-inputs.json", "panel-gold.json", "score.cjs"):
    data = git("show", PIN + ":" + DATA_ROOT + name)
    expected = git("rev-parse", PIN + ":" + DATA_ROOT + name).decode().strip()
    if blob(data) != expected:
        raise SystemExit("BLOCKED_SOURCE_IDENTITY: " + name)
    inputs[name] = data
batch = json.loads(inputs["training-batch-B.json"].decode("utf-8-sig"))
panel = json.loads(inputs["eval-inputs.json"].decode("utf-8-sig"))
if len(batch["examples"]) != 8 or any(e["heldOut"] or e["unitId"] != "executor-en_ru" for e in batch["examples"]):
    raise SystemExit("BLOCKED_FROZEN_FIXTURE")
if len(panel["cases"]) != 24:
    raise SystemExit("BLOCKED_FROZEN_PANEL")
# This exact declaration is modified only within ExecutorUnit.
text = source["TranslationTrainedUnits.cs"].decode("utf-8-sig")
begin, end = text.index("public sealed class ExecutorUnit"), text.index("public sealed class QualityVerifierUnit")
segment = text[begin:end]
needle = "var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };"
method_start = segment.index("public void TrainOnExample(TranslationLearningExample example)")
method_end = segment.index("public double MeasureAccuracy", method_start)
method = segment[method_start:method_end]
if method.count(needle) != 1:
    raise SystemExit("BLOCKED_PATCH_CONTRAST")
helper = """    // Experiment: fixed training JSON policy, first access inside real training.
    private static class TrainingJsonPolicy
    {
        internal static readonly JsonSerializerOptions Options = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };
        // Explicit cctor prevents beforefieldinit from moving allocation into setup.
        static TrainingJsonPolicy() { }
    }

"""
modified = segment[:method_start] + helper + method.replace(needle, "var opts = TrainingJsonPolicy.Options;") + segment[method_end:]
candidate = (text[:begin] + modified + text[end:]).encode("utf-8")
out.mkdir(parents=True)
(out / "deps").mkdir()
(out / "data").mkdir()
(out / "result").mkdir()
manifest = {"id":"JSON-OPTIONS-REUSE-01","status":"PREPARED_NOT_RUN","sourceCommit":PIN,
            "sourceFiles":[], "dataFiles":[], "nativeFiles":[], "excludedReference":"27.TranslationDomain.dll",
            "pdbCheckScope":"all 9 copied Translation source files; not all dependency source files",
            "executorModel":"session default; no override", "neuralUpdates":0}
for name, data in source.items():
    manifest["sourceFiles"].append({"path":"27.TranslationDomain/"+name,"name":name,"blob":SOURCE[name],"sha256":sha(data)})
for path in sorted(native.iterdir()):
    if path.suffix not in (".dll", ".pdb") or not path.is_file():
        continue
    data = path.read_bytes()
    (out / "deps" / path.name).write_bytes(data)
    manifest["nativeFiles"].append({"name":path.name,"originalPath":str(path),"sha256":sha(data)})
for name, data in inputs.items():
    (out / "data" / name).write_bytes(data)
    manifest["dataFiles"].append({"path":DATA_ROOT+name,"name":name,"blob":blob(data),"sha256":sha(data)})
for arm in ("A", "B"):
    root = out / arm
    (root / "src").mkdir(parents=True)
    for name, data in source.items():
        (root / "src" / name).write_bytes(candidate if arm=="B" and name=="TranslationTrainedUnits.cs" else data)
    (root / "Probe.csproj").write_text(f"""<Project>
  <PropertyGroup><ImportDirectoryBuildProps>false</ImportDirectoryBuildProps><ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets></PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk"/>
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>JsonOptions{arm}</AssemblyName><Deterministic>true</Deterministic>
  </PropertyGroup>
  <ItemGroup><Compile Include="../Program.cs"/><Compile Include="src/*.cs"/></ItemGroup>
  <ItemGroup><Reference Include="../deps/*.dll" Exclude="../deps/27.TranslationDomain.dll"/></ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk"/>
</Project>
""", encoding="utf-8")
for name in ("Program.cs", "compare.py", "prepare.py", "run.py"):
    shutil.copyfile(assets / name, out / name)
scorer = inputs["score.cjs"].decode("utf-8-sig")
if scorer.count("['A','B','C']") != 2:
    raise SystemExit("BLOCKED_SCORER_SHAPE; prepared source preserved")
# The frozen assess() and rubric are unchanged; only drop absent third arm.
scorer = scorer.replace("['A','B','C']", "['A','B']")
(out / "result" / "score.cjs").write_text(scorer, encoding="utf-8")
for name in ("eval-inputs.json", "panel-gold.json"):
    (out / "result" / name).write_bytes(inputs[name])
manifest["candidateSourceSha256"] = sha(candidate)
manifest["scorerChanges"] = "Only two loop lists ['A','B','C'] -> ['A','B']; assess/rubric unchanged."
(out / "one-factor.diff").write_text("".join(difflib.unified_diff(
    text.splitlines(keepends=True), candidate.decode("utf-8").splitlines(keepends=True),
    fromfile="A/TranslationTrainedUnits.cs", tofile="B/TranslationTrainedUnits.cs")), encoding="utf-8")
manifest["hostFiles"] = [{"path":name,"sha256":sha((out/name).read_bytes())} for name in (
    "Program.cs","compare.py","prepare.py","run.py","A/Probe.csproj","B/Probe.csproj",
    "result/score.cjs","one-factor.diff")]
(out / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"status":"PREPARED_NOT_RUN","out":str(out),"next":"Build A then B; do not run before manifest/PDB checks."}))


```

## Program.cs

```csharp
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TranslationDomain;

// Isolated source-copy executable. No gold, external model, product writer or cold load.
internal static class Program
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    static string Root = "", Arm = "", Out = "";
    static readonly List<object> Raw = new();
    static readonly List<object> States = new();

    static async Task<int> Main(string[] args)
    {
        if (args.Length != 2 || (args[1] != "A" && args[1] != "B")) return 64;
        Root = Path.GetFullPath(args[0]); Arm = args[1];
        Out = Path.Combine(Root, "result", Arm);
        // Existing result directory is preserved, even after failed preflight.
        if (Directory.Exists(Out)) { Console.Error.WriteLine("BLOCKED_EXISTING_OUTPUT"); return 73; }
        Directory.CreateDirectory(Out);
        using var armClaim = new FileStream(Path.Combine(Out, "arm.claim"), FileMode.CreateNew, FileAccess.Write, FileShare.None);
        var processClock = Stopwatch.StartNew();
        try
        {
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Root, "manifest.json")))!;
            Save("provenance.json", CheckProvenance(manifest));
            var batch = JsonSerializer.Deserialize<TranslationLearningBatch>(
                File.ReadAllText(Path.Combine(Root, "data", "training-batch-B.json")), Json)!;
            var panel = JsonSerializer.Deserialize<Panel>(
                File.ReadAllText(Path.Combine(Root, "data", "eval-inputs.json")), Json)!;
            if (batch.Examples.Count != 8 || batch.Examples.Any(e => e.HeldOut || e.UnitId != "executor-en_ru") ||
                panel.Cases.Count != 24)
                throw new InvalidOperationException("Frozen data contract mismatch");
            Save("environment.json", new {
                Arm, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                pid = Environment.ProcessId, executorModel = "session default; no override",
                runtimeModelAttestation = "NOT_AVAILABLE", externalModelCalls = 0,
                sourceCommit = manifest["sourceCommit"]!.GetValue<string>()
            });
            var machine = new TranslationMachine();
            await Snapshot(machine, "initial");
            await Evaluate(machine, "before", panel);
            await Snapshot(machine, "before");

            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var cpu0 = process.TotalProcessorTime;
            int gc0 = GC.CollectionCount(0), gc1 = GC.CollectionCount(1), gc2 = GC.CollectionCount(2);
            int thread0 = Environment.CurrentManagedThreadId;
            long threadBytes0 = GC.GetAllocatedBytesForCurrentThread();
            long processBytes0 = GC.GetTotalAllocatedBytes(true);
            long started = Stopwatch.GetTimestamp();
            var pending = machine.TrainStructuredBatchAsync(batch);
            if (!pending.IsCompleted)
                throw new InvalidOperationException("Pinned synchronous training contract changed");
            var result = pending.GetAwaiter().GetResult();
            long ended = Stopwatch.GetTimestamp();
            long threadBytes1 = GC.GetAllocatedBytesForCurrentThread();
            long processBytes1 = GC.GetTotalAllocatedBytes(true);
            int thread1 = Environment.CurrentManagedThreadId;
            process.Refresh();
            var cpu1 = process.TotalProcessorTime;
            Save("training.json", new {
                arm = Arm, result, memoryTrainingCalls = 1, rowsSubmitted = 8, neuralUpdates = 0,
                trainingThreadAllocatedBytes = threadBytes1 - threadBytes0,
                trainingProcessAllocatedBytes = processBytes1 - processBytes0,
                trainingWallMs = (ended - started) * 1000.0 / Stopwatch.Frequency,
                trainingCpuMs = (cpu1 - cpu0).TotalMilliseconds,
                gc0 = GC.CollectionCount(0) - gc0, gc1 = GC.CollectionCount(1) - gc1,
                gc2 = GC.CollectionCount(2) - gc2,
                sameManagedThread = thread0 == thread1, thread0, thread1,
                optionsFirstUse = "Included in this measured training call; nested B cctor is not warmed.",
                scope = "Exactly one actual TrainStructuredBatchAsync; data read, construction, outputs, snapshots outside."
            });
            if (!result.IsSuccess || result.Value!.RowsTrained != 8 ||
                !result.Value.PromotedUnitIds.SequenceEqual(new[] { "executor-en_ru" }) || thread0 != thread1)
                throw new InvalidOperationException("Training receipt invalid for the bounded comparison");

            Save("runtime-bindings-after-training.json", VerifyRuntimeBindings(manifest));

            await Snapshot(machine, "trained");
            await Evaluate(machine, "after", panel);
            await Snapshot(machine, "after");
            Save("runtime-bindings-after-evaluation.json", VerifyRuntimeBindings(manifest));
            process.Refresh();
            Save("completion.json", new {
                status = "EXECUTED_SOURCE_COPY_NATIVE_MEMORY_AND_ORDINARY_FLOW",
                arm = Arm, ordinaryFlowCalls = Raw.Count, memoryTrainingCalls = 1, lessons = 8,
                neuralUpdates = 0, coldLoads = 0, harnessWallMs = processClock.Elapsed.TotalMilliseconds,
                processLifetimeAllocatedBytes = GC.GetTotalAllocatedBytes(true),
                processPeakWorkingSetBytes = process.PeakWorkingSet64,
                costLimits = "Process counters only; no whole-computer RAM/GPU/AI tokens; no causal wall-time attribution."
            });
            return 0;
        }
        catch (Exception ex)
        {
            Save("failure.json", new {
                status = "BLOCKED_OR_FAILED_PARTIAL", error = ex.ToString(), savedOrdinaryOutputs = Raw.Count,
                trainingReceiptExists = File.Exists(Path.Combine(Out, "training.json")),
                retry = "None; preserve all artifacts and follow registry NEXT."
            });
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
    }

    static async Task Evaluate(TranslationMachine machine, string phase, Panel panel)
    {
        var or = await machine.GetOrchestratorAsync();
        if (!or.IsSuccess) throw new InvalidOperationException(or.ErrorMessage);
        foreach (var item in panel.Cases)
        {
            var request = new LogicalOrchestratorRequest(
                $"JSON-OPTIONS-REUSE-01-{Arm}-{phase}-{item.Id}", TranslationRoutingFlow.ScenarioId,
                item.Source, new LogicalExecutionFlags(), new Dictionary<string, object> {
                    ["source_language"] = panel.SourceLanguage, ["target_language"] = panel.TargetLanguage,
                    ["topic_domain"] = panel.TopicDomain
                });
            var t = Stopwatch.StartNew();
            var result = await or.Value!.RunAsync(request);
            t.Stop();
            var flow = result.Value?.FlowResult;
            Raw.Add(new {
                arm = Arm, phase, item.Id, item.Bucket, item.Source,
                success = result.IsSuccess, error = result.ErrorMessage, errorCode = result.ErrorCode,
                actual = result.Value?.FinalHumanResult, verdict = result.Value?.OverallVerdict,
                selectedFlow = result.Value?.SelectedFlowId, adapterId = flow?.Metrics.GetValueOrDefault("adapter_id"),
                metrics = flow?.Metrics, warnings = flow?.Warnings, elapsedMs = t.Elapsed.TotalMilliseconds,
                selectedTrainingIds = "NOT_EXPOSED_BY_ORDINARY_FLOW"
            });
        }
        Save("raw-outputs.json", Raw);
    }

    static async Task Snapshot(TranslationMachine machine, string label)
    {
        var exported = machine.ExportCheckpoint();
        if (!exported.IsSuccess) throw new InvalidOperationException(exported.ErrorMessage);
        var state = exported.Value!;
        var path = Path.Combine(Out, label + ".checkpoint.json");
        var saved = await machine.SaveCheckpointAsync(path);
        if (!saved.IsSuccess) throw new InvalidOperationException(saved.ErrorMessage);
        var canonical = state with {
            ExportedAtUtc = DateTimeOffset.UnixEpoch,
            TrainingHistory = state.TrainingHistory.Select(h => h with { CpuMs = 0, PeakRamMb = 0, ElapsedMs = 0 }).ToArray()
        };
        var executor = state.Executors.Single(e => e.Direction == TranslationDirection.EN_RU);
        var frozen = new {
            state.SemanticTokenizer, state.QualityVerifier,
            otherExecutors = state.Executors.Where(e => e.Direction != TranslationDirection.EN_RU).ToArray()
        };
        States.Add(new {
            label, canonicalSha256 = Hash(JsonSerializer.SerializeToUtf8Bytes(canonical, Json)),
            memorySha256 = Hash(JsonSerializer.SerializeToUtf8Bytes(new { executor, state.KnnStore }, Json)),
            frozenNonExecutorSha256 = Hash(JsonSerializer.SerializeToUtf8Bytes(frozen, Json)),
            state.Version, state.HasStructuredTraining, exact = executor.ExactSourceMap.Count,
            phrases = executor.PhraseMap.Count, words = executor.WordMap.Count, surfaceWords = executor.SurfaceWordMap.Count,
            knnTriples = state.KnnStore.Entries.Count, identities = state.StructuredTrainingIdentities?.Count ?? 0,
            history = state.TrainingHistory.Count, checkpointSha256 = Hash(File.ReadAllBytes(path)),
            checkpointBytes = new FileInfo(path).Length,
            canonicalExclusions = "ExportedAtUtc; TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs only"
        });
        Save("state-receipts.json", States);
    }

    static object CheckProvenance(JsonNode manifest)
    {
        if (typeof(TranslationMachine).Assembly.GetName().Name != "JsonOptions" + Arm)
            throw new InvalidOperationException("Wrong executable/arm identity");
        foreach (var entry in manifest["hostFiles"]!.AsArray())
        {
            var p = Path.Combine(Root, entry!["path"]!.GetValue<string>());
            if (Hash(File.ReadAllBytes(p)) != entry["sha256"]!.GetValue<string>())
                throw new InvalidOperationException("Authored host file changed: " + p);
        }
        foreach (var entry in manifest["nativeFiles"]!.AsArray())
        {
            var p = Path.Combine(Root, "deps", entry!["name"]!.GetValue<string>());
            if (Hash(File.ReadAllBytes(p)) != entry["sha256"]!.GetValue<string>())
                throw new InvalidOperationException("BLOCKED_ENVIRONMENT: native dependency changed: " + p);
        }
        foreach (var entry in manifest["dataFiles"]!.AsArray())
        {
            var name = entry!["name"]!.GetValue<string>();
            // Gold/rubric and scorer are validated by compare.py after native outputs.
            if (name != "training-batch-B.json" && name != "eval-inputs.json") continue;
            var p = Path.Combine(Root, "data", name);
            if (Hash(File.ReadAllBytes(p)) != entry["sha256"]!.GetValue<string>())
                throw new InvalidOperationException("Frozen input changed: " + p);
        }
        var dll = Path.Combine(Root, "deps", "27.TranslationDomain.dll");
        var pdb = Path.Combine(Root, "deps", "27.TranslationDomain.pdb");
        using var pe = new PEReader(File.OpenRead(dll));
        var debug = pe.ReadDebugDirectory().Single(e => e.Type == DebugDirectoryEntryType.CodeView);
        var cv = pe.ReadCodeViewDebugDirectoryData(debug);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdb));
        var reader = provider.GetMetadataReader();
        var id = new BlobContentId(reader.DebugMetadataHeader!.Id);
        if (cv.Guid != id.Guid || debug.Stamp != id.Stamp || cv.Age != 1)
            throw new InvalidOperationException("BLOCKED_ENVIRONMENT: existing native DLL/PDB identity mismatch");
        foreach (var entry in manifest["sourceFiles"]!.AsArray())
        {
            var relative = entry!["path"]!.GetValue<string>();
            var name = entry["name"]!.GetValue<string>();
            var bytes = File.ReadAllBytes(Path.Combine(Root, "A", "src", name));
            if (Hash(bytes) != entry["sha256"]!.GetValue<string>())
                throw new InvalidOperationException("Baseline source copy changed: " + name);
            var doc = reader.Documents.Select(h => reader.GetDocument(h)).Single(d =>
                reader.GetString(d.Name).Replace('\\', '/').EndsWith(relative, StringComparison.OrdinalIgnoreCase));
            var algorithm = reader.GetGuid(doc.HashAlgorithm);
            var actual = algorithm == new Guid("8829d00f-11b8-4213-878b-770e8597ac16") ? SHA256.HashData(bytes) :
                algorithm == new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460") ? SHA1.HashData(bytes) :
                throw new InvalidOperationException("Unsupported PDB checksum");
            if (!actual.SequenceEqual(reader.GetBlobBytes(doc.Hash)))
                throw new InvalidOperationException("BLOCKED_ENVIRONMENT: no source-verified native DLL: " + relative);
            var ownBytes = File.ReadAllBytes(Path.Combine(Root, Arm, "src", name));
            var expected = Arm == "B" && name == "TranslationTrainedUnits.cs"
                ? manifest["candidateSourceSha256"]!.GetValue<string>() : entry["sha256"]!.GetValue<string>();
            if (Hash(ownBytes) != expected)
                throw new InvalidOperationException("Experimental source changed: " + name);
        }
        var experimentalDll = typeof(TranslationMachine).Assembly.Location;
        var experimentalPdb = Path.ChangeExtension(experimentalDll, ".pdb");
        using var ownPe = new PEReader(File.OpenRead(experimentalDll));
        var ownDebug = ownPe.ReadDebugDirectory().Single(e => e.Type == DebugDirectoryEntryType.CodeView);
        var ownCv = ownPe.ReadCodeViewDebugDirectoryData(ownDebug);
        using var ownProvider = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(experimentalPdb));
        var ownReader = ownProvider.GetMetadataReader();
        var ownId = new BlobContentId(ownReader.DebugMetadataHeader!.Id);
        if (ownCv.Guid != ownId.Guid || ownDebug.Stamp != ownId.Stamp || ownCv.Age != 1)
            throw new InvalidOperationException("Experimental DLL/PDB identity mismatch");
        var ownSources = manifest["sourceFiles"]!.AsArray().Select(e =>
            Path.Combine(Root, Arm, "src", e!["name"]!.GetValue<string>())).Append(Path.Combine(Root, "Program.cs"));
        foreach (var source in ownSources)
        {
            var normalized = Path.GetFullPath(source).Replace('\\', '/');
            var doc = ownReader.Documents.Select(h => ownReader.GetDocument(h)).Single(d =>
                ownReader.GetString(d.Name).Replace('\\', '/').Equals(normalized, StringComparison.OrdinalIgnoreCase));
            var algorithm = ownReader.GetGuid(doc.HashAlgorithm);
            var bytes = File.ReadAllBytes(source);
            var actual = algorithm == new Guid("8829d00f-11b8-4213-878b-770e8597ac16") ? SHA256.HashData(bytes) :
                algorithm == new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460") ? SHA1.HashData(bytes) :
                throw new InvalidOperationException("Unsupported experimental PDB checksum");
            if (!actual.SequenceEqual(ownReader.GetBlobBytes(doc.Hash)))
                throw new InvalidOperationException("Experimental source/PDB mismatch: " + source);
        }
        return new {
            sourceCommit = manifest["sourceCommit"]!.GetValue<string>(),
            baselineDllSha256 = Hash(File.ReadAllBytes(dll)), baselinePdbSha256 = Hash(File.ReadAllBytes(pdb)),
            sourcePdbFilesVerified = 9, dependencySourceCoverage = "NOT_FULLY_VERIFIED; exact copied DLL hashes recorded",
            experimentalAssembly = experimentalDll,
            experimentalAssemblySha256 = Hash(File.ReadAllBytes(experimentalDll)),
            experimentalPdbSha256 = Hash(File.ReadAllBytes(experimentalPdb)),
            experimentalSourcePdbFilesVerified = 10,
            runtimeBindings = VerifyRuntimeBindings(manifest),
            loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && a.Location.Length > 0)
                .Select(a => new { a.FullName, a.Location, sha256 = Hash(File.ReadAllBytes(a.Location)) }).ToArray()
        };
    }
    static object VerifyRuntimeBindings(JsonNode manifest)
    {
        var expected = manifest["nativeFiles"]!.AsArray()
            .Where(e => e!["name"]!.GetValue<string>().EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(e => e!["name"]!.GetValue<string>(), e => e!["sha256"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase);
        var copies = new List<object>();
        foreach (var (name, digest) in expected)
        {
            if (name == "27.TranslationDomain.dll") continue;
            var path = Path.Combine(AppContext.BaseDirectory, name);
            if (!File.Exists(path) || Hash(File.ReadAllBytes(path)) != digest)
                throw new InvalidOperationException("Runtime bin dependency differs/missing: " + path);
            copies.Add(new { name, path, sha256 = digest });
        }
        var loaded = new List<object>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
        {
            var name = assembly.GetName().Name + ".dll";
            if (name == "27.TranslationDomain.dll")
                throw new InvalidOperationException("Excluded production Translation assembly was loaded");
            if (!expected.TryGetValue(name, out var digest)) continue;
            if (assembly.Location.Length == 0 || Hash(File.ReadAllBytes(assembly.Location)) != digest)
                throw new InvalidOperationException("Actual loaded dependency differs: " + name);
            loaded.Add(new { name, path = assembly.Location, sha256 = digest });
        }
        return new { copiedRuntimeFiles = copies, verifiedLoadedAssemblies = loaded,
            scope = "Before TRAIN and after TRAIN/evaluation; all checks outside measured training window" };
    }
    static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    static void Save(string name, object value) =>
        File.WriteAllText(Path.Combine(Out, name), JsonSerializer.Serialize(value, Json));
    sealed record Panel(string SourceLanguage, string TargetLanguage, string TopicDomain, IReadOnlyList<Case> Cases);
    sealed record Case(string Id, string Bucket, string Source);
}


```

## compare.py

```python
"""Aggregate only existing A/B receipts, then apply the frozen P125 scorer."""
import argparse, hashlib, json, pathlib, subprocess
p = argparse.ArgumentParser()
p.add_argument("--root", required=True)
args = p.parse_args()
root = pathlib.Path(args.root)
out = root / "result"
def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))
def write(name, data):
    (out / name).write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
if (out / "comparison.json").exists():
    raise SystemExit("BLOCKED_EXISTING_COMPARISON; preserve it; no automatic retry.")
with (out / "comparison.claim").open("xb") as claim:
    claim.write(b"one comparison; no repeat")
arms = {}
for arm in ("A", "B"):
    d = out / arm
    if not (d / "completion.json").is_file():
        raise SystemExit("PARTIAL_NO_COMPLETE_COMPARISON: missing " + str(d / "completion.json"))
    arms[arm] = {
        "raw":read(d / "raw-outputs.json"), "states":read(d / "state-receipts.json"),
        "train":read(d / "training.json"), "completion":read(d / "completion.json")
    }
    if len(arms[arm]["raw"]) != 48 or arms[arm]["completion"]["memoryTrainingCalls"] != 1:
        raise SystemExit("BLOCKED_COUNT_MISMATCH")
raw = arms["A"]["raw"] + arms["B"]["raw"]
manifest = read(root / "manifest.json")
for entry in manifest["dataFiles"]:
    source = root / "data" / entry["name"]
    if hashlib.sha256(source.read_bytes()).hexdigest() != entry["sha256"]:
        raise SystemExit("BLOCKED_FROZEN_DATA: " + str(source))
for entry in manifest["hostFiles"]:
    if hashlib.sha256((root / entry["path"]).read_bytes()).hexdigest() != entry["sha256"]:
        raise SystemExit("BLOCKED_HOST_IDENTITY: " + entry["path"])
# The scorer reads these result-root copies; verify their actual bytes too.
for name in ("panel-gold.json", "eval-inputs.json"):
    expected = next(e["sha256"] for e in manifest["dataFiles"] if e["name"] == name)
    if hashlib.sha256((out / name).read_bytes()).hexdigest() != expected:
        raise SystemExit("BLOCKED_SCORER_INPUT: " + name)
write("raw-outputs.json", raw)
write("completion.json", {"status":"TWO_NATIVE_ARMS_COMPLETED","ordinaryFlowCalls":96,
      "memoryTrainingCalls":2,"lessons":16,"neuralUpdates":0,"coldLoads":0})
# The exact frozen assess()/gold is called only after all native outputs exist.
subprocess.run(["node", str(out / "score.cjs")], check=True)
semantic_fields = ("success","errorCode","actual","verdict","selectedFlow","adapterId","metrics","warnings")
def state(arm, label):
    return next(s for s in arms[arm]["states"] if s["label"] == label)
def pair_row(arm, phase, key):
    return next(r for r in arms[arm]["raw"] if r["phase"]==phase and r["id"]==key)
panels = read(root / "data" / "eval-inputs.json")["cases"]
different = []
for phase in ("before","after"):
    for case in panels:
        a, b = pair_row("A",phase,case["id"]), pair_row("B",phase,case["id"])
        changed = [f for f in semantic_fields if a.get(f) != b.get(f)]
        if changed:
            different.append({"phase":phase,"id":case["id"],"fields":changed,"A":a,"B":b})
fields = ("trainingThreadAllocatedBytes","trainingProcessAllocatedBytes","trainingWallMs",
          "trainingCpuMs","gc0","gc1","gc2")
deltas = []
for metric in fields:
    a, b = arms["A"]["train"][metric], arms["B"]["train"][metric]
    deltas.append({"metric":metric,"A":a,"B":b,"delta_B_minus_A":b-a,
                   "relativeReduction":(a-b)/a if a else None,
                   "observedUsefulDirection": b < a,
                   "attribution":"PENDING_VALIDITY_CHECK"})
summary = read(out / "score-summary.json")
after = {a:next(s for s in summary["summary"] if s["arm"]==a and s["phase"]=="after" and s["bucket"]=="all")
         for a in ("A","B")}
score_fields = ("correct","answered","acceptedWrong","actionErrors","objectErrors","polarityErrors",
                "quantityErrors","secondObjectErrors","questionErrors","safeEdges")
score_deltas = [{"metric":k,"A":after["A"][k],"B":after["B"][k],
                 "delta_B_minus_A":after["B"][k]-after["A"][k]} for k in score_fields]
def delta(after_value, before_value):
    return None if after_value is None or before_value is None else after_value - before_value
four_state_quality = []
for bucket in ("all","exact","near","composition","edge"):
    for metric in (*score_fields,"correctOverAll","correctOverAnswered","coverage"):
        values = {}
        for arm in ("A","B"):
            for phase, suffix in (("before","0"),("after","1")):
                row = next(s for s in summary["summary"] if s["arm"]==arm and s["phase"]==phase and s["bucket"]==bucket)
                values[arm+suffix] = row[metric]
        da, db = delta(values["A1"],values["A0"]), delta(values["B1"],values["B0"])
        four_state_quality.append({"bucket":bucket,"metric":metric,**values,"deltaA":da,"deltaB":db,
                                   "differenceInDeltas":delta(db,da)})
comparisons = {
    "initialStateEqual":state("A","initial")["canonicalSha256"]==state("B","initial")["canonicalSha256"],
    "trainedStateEqual":state("A","trained")["canonicalSha256"]==state("B","trained")["canonicalSha256"],
    "beforeOutputsChangedState":{a:state(a,"initial")["canonicalSha256"]!=state(a,"before")["canonicalSha256"] for a in arms},
    "afterOutputsChangedState":{a:state(a,"trained")["canonicalSha256"]!=state(a,"after")["canonicalSha256"] for a in arms},
    "beforeOutputsChangedMemory":{a:state(a,"initial")["memorySha256"]!=state(a,"before")["memorySha256"] for a in arms},
    "afterOutputsChangedMemory":{a:state(a,"trained")["memorySha256"]!=state(a,"after")["memorySha256"] for a in arms},
    "realMemoryChanged":{a:state(a,"before")["memorySha256"]!=state(a,"trained")["memorySha256"] for a in arms},
    "frozenOtherOwnersChanged":{a:state(a,"before")["frozenNonExecutorSha256"]!=state(a,"trained")["frozenNonExecutorSha256"] for a in arms},
    "outputDifferences":different
}
valid = (comparisons["initialStateEqual"] and comparisons["trainedStateEqual"] and
         all(comparisons["realMemoryChanged"].values()) and not different and
         not any(comparisons["beforeOutputsChangedState"].values()) and
         not any(comparisons["afterOutputsChangedState"].values()) and
         not any(comparisons["frozenOtherOwnersChanged"].values()) and
         all(arms[a]["train"]["sameManagedThread"] for a in arms))
for row in deltas:
    row["attribution"] = ("INVALID_COMPARISON" if not valid else
        "SUPPORTED_IN_THIS_PAIRED_RUN" if row["metric"] in ("trainingThreadAllocatedBytes","trainingProcessAllocatedBytes")
        else "NOT_ESTABLISHED")
write("comparison.json", {"status":"MEASURED_LOCAL_COMPARISON","experiment":"JSON-OPTIONS-REUSE-01",
      "domain":"Translation EN_RU technical synthetic P125 fixture",
      "comparisons":comparisons,"exactOptimizationSupported":valid,
      "costDeltas":deltas,"afterOnlyQualityDeltas":score_deltas,"fourStateQuality":four_state_quality,
      "successRule":"Every measured beneficial metric delta is a success on that metric/domain; all regressions alongside.",
      "promotion":"CANDIDATE_ONLY_USER_DECISION","wallTimeAttribution":"NOT_ESTABLISHED" if valid else "INVALID_COMPARISON",
      "limits":"Two cold processes, A then B, one batch8 each; no warmup, repeat, reverse order or confidence interval."})
print("Recorded comparison.json; no further native calls; write one Russian report, then registry NEXT.")


```

## run.py

```python
"""One fixed attempt: prepare, two small builds, A, B, compare. No retry."""
import datetime, json, os, pathlib, shutil, subprocess, sys, time
HOST = pathlib.Path("C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host")
REPO = pathlib.Path("C:/Projects/llm_mvp_core")
OUT = pathlib.Path("C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JO1/a001")
LEGACY = pathlib.Path("C:/xi/e/jo1")
PYTHON = pathlib.Path("C:/Python314/python.exe")
STAGING = HOST / "attempt-jo1"
if os.name != "nt" or not PYTHON.is_file():
    raise SystemExit("BLOCKED_ENVIRONMENT: assigned Windows Python C:/Python314/python.exe unavailable.")
if pathlib.Path(sys.executable).resolve() != PYTHON.resolve():
    raise SystemExit("BLOCKED_ENVIRONMENT: invoke only the assigned C:/Python314/python.exe.")
if OUT.exists() or STAGING.exists() or LEGACY.exists():
    raise SystemExit("BLOCKED_EXISTING_OUTPUT: preserve execution root/staging; no overwrite or retry.")
if not (HOST / "prepare.py").is_file() or not (HOST / "Program.cs").is_file() or not (HOST / "compare.py").is_file():
    raise SystemExit("BLOCKED_PREPARATION: authored files missing in the exact HOST.")
STAGING.mkdir(parents=True)
with (STAGING/"execution.claim").open("xb") as claim:
    claim.write(b"JSON-OPTIONS-REUSE-01; one attempt; no retry")
receipts = []
native_starts = 0
def utc():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()
def save_receipts(status):
    payload = {"experiment":"JSON-OPTIONS-REUSE-01","status":status,"noAutoRetry":True,
               "nativeProcessesStarted":native_starts,"receipts":receipts}
    text = json.dumps(payload,ensure_ascii=False,indent=2)
    (STAGING/"command-receipts.json").write_text(text,encoding="utf-8")
    if (OUT/"result").is_dir():
        (OUT/"result"/"command-receipts.json").write_text(text,encoding="utf-8")
def kill_owned_tree(process):
    # Cancels only the descendant tree created by this runner.
    if process.poll() is None:
        subprocess.run(["taskkill","/PID",str(process.pid),"/T","/F"],
                       stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,check=False)
        try: process.wait(timeout=5)
        except subprocess.TimeoutExpired: process.kill()
steps = [
("prepare",[str(PYTHON),str(HOST/"prepare.py"),"--repo",str(REPO),"--out",str(OUT)],30),
("build-A",["dotnet","build",str(OUT/"A"/"Probe.csproj"),"-c","Release","--nologo"],120),
("build-B",["dotnet","build",str(OUT/"B"/"Probe.csproj"),"-c","Release","--nologo"],120),
("A",["dotnet",str(OUT/"A"/"bin"/"Release"/"net9.0"/"JsonOptionsA.dll"),str(OUT),"A"],60),
("B",["dotnet",str(OUT/"B"/"bin"/"Release"/"net9.0"/"JsonOptionsB.dll"),str(OUT),"B"],60),
("compare",[str(PYTHON),str(OUT/"compare.py"),"--root",str(OUT)],30),
]
for name,argv,timeout in steps:
    stdout,stderr = STAGING/(name+".stdout"),STAGING/(name+".stderr")
    start,started = utc(),time.perf_counter()
    status,exit_code,error,process = "RUNNING",None,None,None
    try:
        with stdout.open("xb") as so, stderr.open("xb") as se:
            process = subprocess.Popen(argv,cwd=REPO,stdout=so,stderr=se,
                                       creationflags=subprocess.CREATE_NEW_PROCESS_GROUP)
            if name in ("A","B"): native_starts += 1
            exit_code = process.wait(timeout=timeout)
            status = "COMPLETED" if exit_code == 0 else "FAILED"
    except subprocess.TimeoutExpired:
        status,error = "TIMEOUT",f"Timeout after {timeout}s"
        if process is not None: kill_owned_tree(process)
    except KeyboardInterrupt:
        # A user stop cancels our running child and immediately ends; no report/receipt work follows.
        if process is not None: kill_owned_tree(process)
        raise
    except Exception as exc:
        status,error = "FAILED_TO_START",str(exc)
    receipt = {"step":name,"argv":argv,"cwd":str(REPO),"startUtc":start,"finishUtc":utc(),
               "wallMs":(time.perf_counter()-started)*1000,"exit":exit_code,"status":status,
               "timeoutSeconds":timeout,"stdoutPath":str(stdout),"stderrPath":str(stderr),"error":error}
    receipts.append(receipt)
    if (OUT/"result").is_dir():
        for path in (stdout,stderr):
            if path.is_file(): shutil.copyfile(path,OUT/"result"/path.name)
    save_receipts(status)
    if status != "COMPLETED":
        print(json.dumps({"status":status,"failedStep":name,"nativeProcessesStarted":native_starts,
              "trainingStarted":False if name in ("prepare","build-A","build-B") else "See preserved native receipt; unknown if incomplete",
              "next":"Registry NEXT; no B after failed A; no retry"},ensure_ascii=False))
        raise SystemExit(1)
save_receipts("COMPLETED_ONE_BOUNDED_COMPARISON")
print("One comparison finished. Write one Russian report and candidate if beneficial, then registry NEXT.")


```


**Применение в этой сессии:** только Markdown/JSON; приведённые source blocks остаются будущим заданием, реальные host/source/data/weights сейчас не создаются. Исполнитель — модель его сессии по умолчанию без override. Student указан отдельно. При «остановить всё» остановиться немедленно; при «следующий» немедленно перейти по назначенному NEXT, без report/hash/review/Git/retry прежнего опыта. При отдельном поручении одного ID — END.

При обычном завершении измеренного полезного опыта: русский report по карточке и одна запись до120слов в [required-changes.md](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md) со ссылкой на report, метрикой, доменом и ухудшениями. Отдельный candidate рядом с raw служит содержимым этой записи; дополнительных запусков нет. STOP/NEXT выше оформления. Все экспериментальные послабления (минимальный host, throw/try-catch/void) действуют; полный product lifecycle не требуется.

**Защита старой попытки:** C:/xi/e/jo1 — только проверка legacy существования. При наличии этого пути либо JO1/a001 либо host/attempt-jo1 сохранить всё и закончить BLOCKED_EXISTING_OUTPUT. Материализатор по четырём блокам не запускает их; единственный runtime owner — host/run.py. Он владеет staging/execution.claim до prepare/build, prepare создаёт JO1/a001/result, Program резервирует arm.claim до модели/Train, compare резервирует comparison.claim до scorer. Ручные внутренние команды запрещены: это повтор бюджета. Baseline DLL/PDB на этой машине существуют, но их runtime/PDB совпадение ещё не проверено; несовпадение даёт BLOCKED_ENVIRONMENT, без product rebuild.

**Точные навыки:** [trainable-architecture-decision-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md), [intent-to-plan-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/intent-to-plan-SKILL.md), [plan-self-validation-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-self-validation-SKILL.md), [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md), [experiment-design-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md), [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md), [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md), [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md). Нового стека/модели не выбирать.
