# Текст проекта будущего изолированного host

20.10.2026. **Не реализация runner и не запуск.** Ни `.csproj`, ни DLL, ни результат обучения в этом пакете не создаются. Статус `NEEDS_NATIVE_HOST / NOT_RUN` сохраняется до отдельного поручения реализовать опыт. Ниже фиксируются место, последовательность и заготовка prepare, чтобы исполнитель не выбирал другой механизм или материал.

## Имена и фиксированная последовательность

Authored root: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/IDENTITY-TRAIN-INDEX-01/host/`.
Рабочий root: `C:/xgi/idx01/run01`; занятый root не удалять и не обходить новым.
Файлы для будущей реализации: `prepare.py`, `Program.cs`, `compare.py`, `run.py`.

| Шаг | Точная будущая команда | Граница времени |
|---|---|---:|
| prepare | `C:/Python314/python.exe <authored-root>/prepare.py --repo C:/Projects/llm_mvp_core --out C:/xgi/idx01/run01` | 30 с |
| build A | `dotnet build C:/xgi/idx01/run01/A/Probe.csproj -c Release --nologo` | 120 с |
| build B | `dotnet build C:/xgi/idx01/run01/B/Probe.csproj -c Release --nologo` | 120 с |
| W | `dotnet C:/xgi/idx01/run01/A/bin/Release/net9.0/IdentityTrainIndexA.dll C:/xgi/idx01/run01 W` | 120 с |
| A | `dotnet C:/xgi/idx01/run01/A/bin/Release/net9.0/IdentityTrainIndexA.dll C:/xgi/idx01/run01 A` | 60 с |
| B | `dotnet C:/xgi/idx01/run01/B/bin/Release/net9.0/IdentityTrainIndexB.dll C:/xgi/idx01/run01 B` | 60 с |
| compare | `C:/Python314/python.exe C:/xgi/idx01/run01/compare.py --root C:/xgi/idx01/run01` | 30 с |

`<authored-root>` — ровно указанный выше путь, не параметр выбора другого каталога. Будущий run.py последовательно выполняет только эти команды с captured stdout/stderr, PID, exit code и временем. При неуспехе следующие шаги не запускать; сохранить partial. При пользовательском stop только остановить своё дочернее дерево; не продолжать отчёт. Одновременная запись и запуск host запрещены. При таймауте сначала наблюдать/остановить тот же owned process; автоматического retry нет.

## Точное поведение ещё не реализованного Program.cs

Опора для authoring — pinned настоящий native [P125 Program.cs](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/harness/Program.cs), не поиск нового harness. Его flow вызов и сохранение raw использовать без замены обучаемого компонента Python/C# surrogate. Программа компилирует девять реальных исходников из prepare; `27.TranslationDomain.dll` не загружается как второй тип того же получателя.

- Аргументы — root и W/A/B. W работает A assembly. Для каждого режима свой новый `result/<mode>`; отказ на существующий каталог до training. Источник фиксируется manifest и SHA, A/B используют одни и те же DLL dependencies, их фактические loaded paths/hashes записываются. Не требуются полный rebuild продукта и строгий product approval workflow.
- W создаёт native TranslationMachine, исполняет точный генератор FIXTURE, сохраняет один native checkpoint и реальные счётчики; gold не читает. Записывает фактические 128 calls/512 row updates. Warm setup измеряется и показывается отдельно, не включается только в B.
- A/B измеряют настоящий `TranslationMachine.LoadCheckpointAsync(warmPath)`; includes parse, constructor, ImportCheckpoint и B set rebuild. После успешного restore получают `.Value.Machine`. Сохраняют canonical state before; выполняют 24 ordinary Flow calls before; сохраняют state ещё раз; один native TrainStructuredBatchAsync нового пакета 4; state; 24 after; state. Panel source/target/topic metadata — из frozen JSON. Каждая фаза хранит `arm,phase,id,bucket,source,success,error,errorCode,actual,verdict,selectedFlow,adapterId,metrics,warnings,elapsedMs`.
- Canonical state исключает только ExportedAtUtc и TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs. Memory digest — EN_RU executor плюс KnnStore. Frozen-other digest — SemanticTokenizer, QualityVerifier и остальные executors. Это проверяет state effect и неизменность посторонних owners, не доказывает качество других языков.
- Restore/Train receipt отдельно: wall Stopwatch, process CPU, GC collection counts и GC.GetTotalAllocatedBytes; Train дополнительно thread allocation и same-managed-thread. Асинхронный restore не объявлять same-thread. Замеры обрамляют реальные native calls, чтение fixtures, scorer и snapshots вне этих окон. Сумма restore+Train считается явно. Full process peak хранить отдельно, не складывать пики.
- В training receipt проверить success, RowsTrained=4, PromotedUnitIds содержит только executor-en_ru. Не считать встроенную AccuracyAfter=MeasureUnitCoverage точностью EN_RU панели. Проверить реальные приросты exact/phrase/KNN/identity, не подставлять ожидания вместо измерений.

## Точное поведение ещё не реализованного compare.py

После завершения обоих native процессов собрать ровно 96 raw строк, проверить pinned data/host hashes по сохранённому manifest, вызвать frozen scorer с единственным изменением списков arms ABC→AB. Gold не доступен learner. Сохранить четырёхсостоянийную таблицу качества по all/exact/near/composition/edge и все raw/gold пары. Сравнить initial/final canonical state A/B, изменения memory от обучения, отсутствие изменения state от evaluation. Для старых x01–x04 до должно быть 4 правильных; после обучения x05–x08 дают реальную проверку нового знания. 0→0 невалидно.

Cost comparison хранит A,B,B−A и direction для restore, Train, суммы, all allocations отдельно от time. Если B лучше по любой полезной оси, отметить успех этой оси с остальными ухудшениями; не навязывать рост accuracy. Один cold process на плечо, fixed order: causal speedup не установлен. Никаких повторов/дополнительных запросов из compare. Результат успеха не предопределён. Итоговый `report.md` и один candidate — только в разрешённом additive output, не в protected документах.

## Текстовая заготовка prepare.py

Это проверенная на форму исходных замен заготовка для будущего authoring, а не утверждение о компиляции или исполнении host. Полный host ещё требует Program/compare/run в отдельной фазе; запрет на текущий запуск действует.

```python
"""Prepare IDENTITY-TRAIN-INDEX-01 only. Never train or touch product/protected files."""
import argparse
import difflib
import hashlib
import json
import pathlib
import shutil
import subprocess

PIN = "dd150aa5216e42b8a8bba14955337398ec5f8397"
SOURCE = {
    "TranslationMachine.cs": "fd0e736de3739236b4dde2d6b9d000f5ebae8416",
    "TranslationTrainedUnits.cs": "1742abfc9fb617dfc66e4d618e0c169da01a6ccc",
    "TranslationUnitContracts.cs": "0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8",
    "TranslationContracts.cs": "9ccff62783661fab24131e206b42280fe4cdc063",
    "TranslationCheckpointContracts.cs": "3879805b0504df7718adb2760e86aa6b4c3b13ab",
    "TranslationDirectionAdapters.cs": "ce139d39cfab36b7e919c31398cacde225b952d5",
    "TranslationRoutingFlow.cs": "ae9da139a68b9e6d6cfd655425b0327053e06f0d",
    "TranslationSemanticTokenizer.cs": "ee6d81c7933fa33a9c57a2111990019ec55e5dab",
    "TranslationMemorySourceLaw.cs": "ba841f4f1839d504444d214feb73c80a4cc24f32",
}
DATA_ROOT = "docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/"
DATA = {
    "training-batch-B.json": "0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c",
    "eval-inputs.json": "585bf30cad84b57b2e1f3fe8c00f171f2348c313",
    "panel-gold.json": "af73d89cfd5b42f954380437bc5596b46dae9498",
    "score.cjs": "e8b20d471667da397e8d5c59ac63b4e7f947540b",
}


def patch_candidate(source):
    """Three replacements, one factor: an auxiliary identity index, never admission."""
    field = "    private readonly List<TranslationStructuredTrainingIdentityCheckpoint> _structuredTrainingIdentities = [];"
    scan = """            if (!_structuredTrainingIdentities.Any(identity =>
                    string.Equals(identity.ExampleId, example.ExampleId, StringComparison.Ordinal) &&
                    string.Equals(identity.UnitId, example.UnitId, StringComparison.Ordinal)))"""
    restore = """        _structuredTrainingIdentities.Clear();
        foreach (var identity in checkpoint.StructuredTrainingIdentities ?? [])
            _structuredTrainingIdentities.Add(identity);"""
    replacements = (
        (field, field + "\n    private readonly HashSet<(string ExampleId, string UnitId)> _structuredTrainingIdentityIndex = [];"),
        (scan, "            if (_structuredTrainingIdentityIndex.Add((example.ExampleId, example.UnitId)))"),
        (restore, """        _structuredTrainingIdentities.Clear();
        _structuredTrainingIdentityIndex.Clear();
        foreach (var identity in checkpoint.StructuredTrainingIdentities ?? [])
        {
            _structuredTrainingIdentities.Add(identity);
            _structuredTrainingIdentityIndex.Add((identity.ExampleId, identity.UnitId));
        }"""),
    )
    # Normalize line endings for the experimental C# copy only; baseline bytes stay exact.
    result = source.decode("utf-8-sig").replace("\r\n", "\n")
    for old, new in replacements:
        if result.count(old) != 1:
            raise ValueError("BLOCKED_PATCH_SHAPE: pinned source differs from authored patch")
        result = result.replace(old, new, 1)
    return result.encode("utf-8")


def sha(data):
    return hashlib.sha256(data).hexdigest()


def blob(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args()
    repo, out = pathlib.Path(args.repo), pathlib.Path(args.out)
    assets = pathlib.Path(__file__).resolve().parent
    if out.exists():
        raise SystemExit("BLOCKED_EXISTING_OUTPUT: preserve it; no overwrite/delete/retry")
    native = repo / "27.TranslationDomain/bin/Release/net9.0"
    dependencies = sorted(p for p in native.glob("*.dll") if p.name != "27.TranslationDomain.dll")
    if not dependencies:
        raise SystemExit("BLOCKED_ENVIRONMENT: existing native dependency DLLs unavailable")
    for name in ("Program.cs", "compare.py", "run.py", "prepare.py"):
        if not (assets / name).is_file():
            raise SystemExit("BLOCKED_PREPARATION: missing exact authored asset " + name)

    def git_bytes(path):
        return subprocess.check_output(["git", "-C", str(repo), "show", PIN + ":" + path])

    source, inputs = {}, {}
    for name, expected in SOURCE.items():
        data = git_bytes("27.TranslationDomain/" + name)
        if blob(data) != expected:
            raise SystemExit("BLOCKED_SOURCE_IDENTITY: " + name)
        source[name] = data
    for name, expected in DATA.items():
        data = git_bytes(DATA_ROOT + name)
        if blob(data) != expected:
            raise SystemExit("BLOCKED_FIXTURE_IDENTITY: " + name)
        inputs[name] = data
    batch = json.loads(inputs["training-batch-B.json"].decode("utf-8-sig"))
    panel = json.loads(inputs["eval-inputs.json"].decode("utf-8-sig"))
    ids = ["tr01", "tr02", "tr03", "tr04", "tr05", "tr06", "tr07", "tr08"]
    if [e["exampleId"] for e in batch["examples"]] != ids or any(
        e["heldOut"] or e["unitId"] != "executor-en_ru" for e in batch["examples"]
    ) or len(panel["cases"]) != 24:
        raise SystemExit("BLOCKED_FROZEN_FIXTURE_SHAPE")
    candidate = patch_candidate(source["TranslationMachine.cs"])
    out.mkdir(parents=True)
    for name in ("deps", "data", "result"):
        (out / name).mkdir()
    manifest = {
        "id": "IDENTITY-TRAIN-INDEX-01", "status": "PREPARED_NOT_RUN", "sourceCommit": PIN,
        "sourceFiles": [], "dataFiles": [], "nativeFiles": [], "hostFiles": [],
        "executorModel": "session default; no override", "neuralUpdates": 0,
        "dependencySourceCoverage": "Exact copied DLL hashes, not a source rebuild of dependencies",
        "excludedReference": "27.TranslationDomain.dll",
        "fixture": {"warmBatches": 128, "warmRowsPerBatch": 4, "warmDistinctIdentities": 512,
                    "warmIndependentLessons": 4, "mainRowsPerArm": 4, "mainArms": 2,
                    "nativeTrainingCallsTotal": 130, "submittedRowsTotal": 520,
                    "independentLessonsTotal": 8, "ordinaryFlowCallsTotal": 96},
    }
    for name, data in source.items():
        manifest["sourceFiles"].append({"path": "27.TranslationDomain/" + name, "name": name,
                                       "blob": SOURCE[name], "sha256": sha(data)})
    for path in dependencies:
        data = path.read_bytes()
        (out / "deps" / path.name).write_bytes(data)
        manifest["nativeFiles"].append({"name": path.name, "originalPath": str(path), "sha256": sha(data)})
    for name, data in inputs.items():
        (out / "data" / name).write_bytes(data)
        manifest["dataFiles"].append({"path": DATA_ROOT + name, "name": name,
                                     "blob": DATA[name], "sha256": sha(data)})
    for arm in ("A", "B"):
        root = out / arm
        (root / "src").mkdir(parents=True)
        for name, data in source.items():
            (root / "src" / name).write_bytes(candidate if arm == "B" and name == "TranslationMachine.cs" else data)
        (root / "Probe.csproj").write_text(f"""<Project>
  <PropertyGroup><ImportDirectoryBuildProps>false</ImportDirectoryBuildProps><ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets></PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk"/>
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>IdentityTrainIndex{arm}</AssemblyName><Deterministic>true</Deterministic>
  </PropertyGroup>
  <ItemGroup><Compile Include="../Program.cs"/><Compile Include="src/*.cs"/></ItemGroup>
  <ItemGroup><Reference Include="../deps/*.dll"/></ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk"/>
</Project>
""", encoding="utf-8")
    for name in ("Program.cs", "compare.py", "prepare.py", "run.py"):
        shutil.copyfile(assets / name, out / name)
    scorer = inputs["score.cjs"].decode("utf-8-sig")
    if scorer.count("['A','B','C']") != 2:
        raise SystemExit("BLOCKED_SCORER_SHAPE; prepared directory preserved")
    (out / "result" / "score.cjs").write_text(scorer.replace("['A','B','C']", "['A','B']"), encoding="utf-8")
    for name in ("eval-inputs.json", "panel-gold.json"):
        (out / "result" / name).write_bytes(inputs[name])
    manifest["candidateSourceSha256"] = sha(candidate)
    manifest["scorerChanges"] = "Only two arm lists ABC to AB; assess() and gold unchanged"
    (out / "one-factor.diff").write_text("".join(difflib.unified_diff(
        source["TranslationMachine.cs"].decode("utf-8-sig").replace("\r\n", "\n").splitlines(keepends=True),
        candidate.decode("utf-8").splitlines(keepends=True),
        fromfile="A/TranslationMachine.cs", tofile="B/TranslationMachine.cs")), encoding="utf-8")
    for name in ("Program.cs", "compare.py", "prepare.py", "run.py", "A/Probe.csproj", "B/Probe.csproj",
                 "result/score.cjs", "one-factor.diff"):
        manifest["hostFiles"].append({"path": name, "sha256": sha((out / name).read_bytes())})
    (out / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print("PREPARED_NOT_RUN: pinned source copies and frozen input only")


if __name__ == "__main__":
    main()

```
