# GRADB-LOCALITY-ENRU-01 — уменьшить стоимость реального LoRA backward перестановкой циклов

## Статус и проверяемое утверждение

**Новая проверяемая гипотеза стоимости обучения, НЕ измеренный успех и НЕ установленная причина плохих переводов.** В текущем `TensorOps.MatMul` градиент правого операнда `gradB=AᵀG` вычисляется скалярно с самым внутренним циклом по строкам. Он читает два массива с шагами `aCols` и `bCols`. Перестановка циклов может уменьшить стоимость настоящего DPO update, сохраняя математическую функцию, FP32 тип и порядок сложения для каждой координаты. Численных результатов этого нового опыта нет; в текущей сессии ничего не запускается и продукт не меняется.

Предметная область — **перевод**; профиль первой проверки — **EN→RU**, native DPO/LoRA на том же C50 и восьми archived live8-парах. Возможный перенос на C#-документацию относится к отдельному будущему опыту: общность kernel не доказывает одинаковую экономию в другом наборе длин, hidden/rank или recipient.

## Код и обязательная связь с обучением

[Текущий `TensorOps.MatMul:679–698`][matmul] выполняет:

```text
batch → inner (aCols) → column (bCols) → row (aRows)
gradB[bOffset + inner*bCols + column] +=
  A[aOffset + row*aCols + inner] * G[resultOffset + row*bCols + column]
```

[Native `LoraLinear.Forward:118–125`][lora] использует два таких умножения — `xDrop @ Aᵀ`, затем `… @ Bᵀ`. Transposed A/B — правые trainable операнды. [`DpoTrainer.Step:665–681`][step] вызывает native backward chosen/rejected и настоящий optimizer. Это конкретный reached учебный получатель, а не произвольный benchmark матрицы.

**A:** текущие скалярные циклы gradB. **B:** только порядок `batch → row → inner → column`; до внутреннего цикла прочитать `aScalar = A[aOffset + row*aCols + inner]`, затем умножать его на contiguous row G и прибавлять к contiguous row gradB. Матрица gradB, offsets, batch modulo и вызов `AccumulateGrad` сохраняются.

Для каждой фиксированной координаты gradB последовательность прибавлений остаётся batch по возрастанию, затем row по возрастанию. Поэтому этот вариант намеренно **не меняет reduction tree и не добавляет SIMD/FMA/FP64, transpose scratch, threading, caching или новые буферы**. Нельзя молча объединить его с SIMD оптимизацией, vectorized forward или gradA: это будут другие факторы. Равенство результатов надо читать из реальных updates; оно не объявляется заранее измеренным.

## Новизна относительно всех загруженных документов

Проверен локальный корпус **662 Markdown possibleDifs** на planning SHA `de270758087502a350c4fcf460acdf1638bdd318` и **296 Markdown защищённого core-каталога** на SHA `9622b575a6aabb79939b12cc9f5d96313f1bf370`. Полнота 296 protected Markdown сверена с remote inventory (`protected-corpus-inventory.json`). Это полный поиск по Markdown-корпусу, а не полный ручной смысловой review всех документов. Поиск:

```text
rg -n -i '\bgradB\b|right.operand|AᵀG|gradB.*SIMD|SIMD.*gradB|right.*adjoint' <possibleDifs> <protected> --glob '*.md'
```

Найдено **3 строки в 3 файлах**, каждая прочитана в контексте:

| Ближайшая прежняя работа | Разграничение |
|---|---|
| Protected [N-ADJOINT-01][adjoint] | Прямо ограничивает фактор `gradA`; говорит «не forward и не gradB». Там scalar/SIMD numerical reduction, здесь скалярный порядок обхода gradB при сохранении coordinate reduction order |
| Planning [N-ADJOINT recipe][prior-plan] | Явно сохраняет gradB неизменным |
| Planning [N-SHAPE-01/02][shape] | Корректность shapes/broadcast и достижимость; не локальность действительного gradB |
| Protected [P110 numeric matrix cost][p110] | Общая тема скорости численной матрицы/SIMD/sparse. Не конкретный правый adjoint в настоящих LoRA updates и не данный loop-interchange factor |

По точным словам отсутствие прежнего названия не доказывает абсолютную мировую новизну. В прочитанных ближайших механизмах **точного дубля перестановки gradB-циклов внутри actual DPO не найдено**. Все прежние карточки сохраняются, новая карточка их не заменяет.

## Конкретное размещение: только добавленные экспериментальные копии

Паспорт задаёт пути будущей реализации. Файлы и runner ниже **ещё не созданы этим аудитом**; команды — контракт реализации карточки, не свидетельство выполненного запуска.

| Назначение | Точный путь относительно `C:/Projects/llm_mvp_core` |
|---|---|
| Копия TensorCore A | `35.ExperimentsV2/Experiments/Rp01Micro/GRADB-LOCALITY-ENRU-01/before/00.TensorCore/` |
| Копия TensorCore B | `35.ExperimentsV2/Experiments/Rp01Micro/GRADB-LOCALITY-ENRU-01/after/00.TensorCore/` |
| Меняемый файл обеих копий | `Core/TensorOps.cs`; A — common telemetry, B — та же telemetry плюс только loop order |
| Common telemetry file обеих копий | `Core/GradBExperimentTelemetry.cs`, namespace `TensorCore.Core`, class `GradBExperimentTelemetry` |
| Host project | `experiments-v2/RP01-MICRO/GRADB-LOCALITY-ENRU-01/DpoLive.csproj` |
| Host entry point | Соседний `Program.cs`, global namespace, `internal static class Program`, `static int Main(string[] args)` |
| Подготовка единственного опыта | Соседний `prepare.ps1` — только копирование, manifest и сборка названных копий |
| Runtime/inputs/results root | `training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/` |
| Выход A/B | `…/before/`, `…/after/`; runtimes `…/runtime/before/`, `…/runtime/after/` |
| Frozen answer scorer | `training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/score.cjs` — byte-copy archived live8 wrapper |
| Сводка learning/cost | Соседний `compare-gradb.cjs`, точный контракт ниже; новый documentation-specified script |
| Итоговый additive отчёт | `…/report.md` и `…/candidate-fix.md`; затем документный перенос в planning possibleDifs |
| Защищённый архив | Только источник чтения `docs/plans/replanning-possible-diffs/`; запись туда запрещена |

**Исходный host — ровно [архивный DpoLive.csproj][host-project] и [Program.cs live8][host-program].** Сохраняются `AssemblyName=Rp01SimulationDpo`, target net9.0 и существующая native ownership/admission цепь. Это reuse существующего экспериментального host, не переименование произвольной сборки ради обхода допуска. Не создавать третий dispatcher или новый продуктовый entrypoint.

### Явный контракт подготовки и зависимостей

1. Использовать core checkout ровно SHA `9622b575a6aabb79939b12cc9f5d96313f1bf370`. Скопировать **все** исходные файлы и `TensorCore.csproj` каталога `00.TensorCore` в два указанных source-каталога; не копировать старые bin/obj и не удалять их. Этот csproj имеет net9.0 и не содержит ProjectReference, поэтому сборка TensorCore не требует запуска общего solution.
2. Read-only источник native assemblies — сохранённый полный `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after-runtime/`, то есть исправленное DPO-плечо, а не его старый baseline. Скопировать его DLL/PDB/deps/runtimeconfig в оба назначенных `runtime/<arm>`. Все non-TensorCore DLL/PDB между плечами должны иметь одинаковые hashes. Проверить PDB/source provenance по тем же owners, что archived host. Если источник не сохранён или source/DLL provenance не совпадает, записать конкретный отсутствующий путь/несовпадение и завершить подготовку без Train. Не выбирать другой runtime, checkpoint или новый общий build самостоятельно.
3. C50 — существующий `training-artifacts/experiments/code-documentation-simulation/simulation-csharp-main500-20261015-01/checkpoints/dpo_policy_step_000050.bin`, SHA256 `6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b`. ConfigHash `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`, Topology `f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb`, LoRA identity `4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35` сохраняются из archived host. Checkpoint и большие binaries не опубликованы в GitHub-архиве; их доступность на машине будущего запуска — конкретное входное условие, не право создать другой B0.
4. Скопировать весь `inputs/` live8 в `<result-root>/inputs/` byte-for-byte: восемь `train8.jsonl` пар, frozen eval inputs/gold. Не составлять новую панель. Hash manifest включает каждый вход и checkpoint. В `<result-root>/score.cjs` byte-for-byte скопировать [archived live8 scorer][scorer-source]. Ему нужны уже скопированные `inputs/score.cjs` и `inputs/panel-gold.json`; внешних package dependencies нет, только Node builtins `fs`, `path`, `vm`. Gold/scorer не передаются learner или generator. **Оба** плеча используют уже исправленную DPO derivative и одну frozen fractional legacy mask-панель; mask не становится вторым фактором.
5. В copied host заменить только plumbing, одинаково в A/B: `native=Path.Combine(root,"runtime",arm)`; входы/выходы идут в новый root; TensorCore provenance сравнивается с соответствующей **экспериментальной копией** source, включая добавленный `Core/GradBExperimentTelemetry.cs`; остальные owners — с pinned original. Не ослаблять guards и не скрывать источник экспериментального TensorCore. `args[0]=result-root`, `args[1]=before|after`, `args[2]=полный путь DPO.dll текущего runtime` — исходный трёхаргументный launch contract сохраняется.
6. Common host не вызывает старый `ComponentGuard()` и отдельный reachability-only forward: для новой карточки они не являются измерением обучения. Реальную достижимость подтверждают 8 native Steps, gradB counters, finite nonzero optimizer moments и итоговые policy deltas. Старые guard-код/результаты в защищённом архиве не удалять; это назначение новой host-копии.
7. `GradBExperimentTelemetry` — одинаковая в A/B небольшая экспериментальная **public static class** с public `Reset()` и `Snapshot()`; host находится в другой сборке. `Snapshot()` возвращает public record `GradBSnapshot(long Calls, long ElapsedTicks, long MultiplyAddCount, long StopwatchFrequency)`. Counters и произведение размеров считают через `long`, начиная с первого множителя, чтобы не переполнить int на настоящем backward. Счётчики обрамляют **существующий** `if(b.RequiresGrad)` body во время actual native backward. Обновление counters/Stopwatch одинаковое для A/B. Не сохранять logits/giant operands и не вызывать synthetic MatMul. В каждой строке `training-steps.json` сохранить snapshot как поле `gradB` (JSON: `calls`, `elapsedTicks`, `multiplyAddCount`, `stopwatchFrequency`), а затем обнулить counters. Там же записать `trainCpuMs` и `trainAllocatedBytes`: разности process CPU и `GC.GetTotalAllocatedBytes(true)` непосредственно до/после того же Step; сохранить прежний `elapsedMs`. Формула gradB milliseconds: `1000.0 * elapsedTicks / stopwatchFrequency`. Счётчик >0 показывает reached factor; нулевой счётчик — `NOT_REACHED`, не успешное ускорение.
8. Строгий порядок зависимостей: **собрать TensorCore A и B → заменить TensorCore.dll/PDB в runtime/before и runtime/after соответствующими instrumented сборками → собрать copied host со ссылкой на runtime/before, где уже есть public telemetry → скопировать только четыре host artifacts в оба runtime**. Host references указывают на `<result-root>/runtime/before/*.dll`, исключая `Rp01SimulationDpo.dll` самого старого host. Allowlist копирования host: `Rp01SimulationDpo.dll`, `Rp01SimulationDpo.pdb`, `Rp01SimulationDpo.deps.json`, `Rp01SimulationDpo.runtimeconfig.json`. **Не копировать всю host bin-directory:** туда MSBuild может скопировать TensorCore A; её перенос в B уничтожит experimental treatment. После копирования проверить hash TensorCore каждого runtime против его собственной сборки и записать manifest. Ошибки компиляции experimental files можно исправлять и пересобирать до первого обучения при тех же factor/data/metrics; число сборок не является барьером обычной подготовки. Product source, tests, checkpoint и архивы не менять.

`prepare.ps1` обязан выполнить именно эти действия и сохранить `preparation.json` с источниками, hashes, A/B diff, build receipts и фактическими zero-Train counters; он не запускает learner. Если target output содержит `started.json`, подготовка не затирает попытку и не назначает альтернативное имя: сохранить существующие результаты и завершиться. Ошибка preparation не считается нулём точности и не вызывает повторный Train.

## Точные команды после реализации указанного host

Рабочий каталог — `C:/Projects/llm_mvp_core`. Все команды **предписаны для будущей отдельно порученной реализации/проверки**, сейчас не исполняются:

```powershell
pwsh -NoProfile -File experiments-v2/RP01-MICRO/GRADB-LOCALITY-ENRU-01/prepare.ps1
dotnet training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/runtime/before/Rp01SimulationDpo.dll C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01 before C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/runtime/before/DPO.dll
dotnet training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/runtime/after/Rp01SimulationDpo.dll C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01 after C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/runtime/after/DPO.dll
node C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/score.cjs
node C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01/compare-gradb.cjs
```

Внутри preparation назначен следующий порядок сборки и копирования; это тело будущего script, не команды, выполненные этим аудитом:

```powershell
$gradBResultRoot = "C:/Projects/llm_mvp_core/training-artifacts/experiments/replanning-candidates/gradb-locality-enru-01"
$gradBCopyRoot = "C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/GRADB-LOCALITY-ENRU-01"
$gradBHostRoot = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/GRADB-LOCALITY-ENRU-01"
dotnet build "$gradBCopyRoot/before/00.TensorCore/TensorCore.csproj" -c Release --nologo
dotnet build "$gradBCopyRoot/after/00.TensorCore/TensorCore.csproj" -c Release --nologo
foreach ($gradBArm in @("before", "after")) {
    foreach ($gradBFile in @("TensorCore.dll", "TensorCore.pdb")) {
        Copy-Item -LiteralPath "$gradBCopyRoot/$gradBArm/00.TensorCore/bin/Release/net9.0/$gradBFile" -Destination "$gradBResultRoot/runtime/$gradBArm/$gradBFile"
    }
}
dotnet build "$gradBHostRoot/DpoLive.csproj" -c Release --nologo
foreach ($gradBArm in @("before", "after")) {
    foreach ($gradBFile in @("Rp01SimulationDpo.dll", "Rp01SimulationDpo.pdb", "Rp01SimulationDpo.deps.json", "Rp01SimulationDpo.runtimeconfig.json")) {
        Copy-Item -LiteralPath "$gradBHostRoot/bin/Release/net9.0/$gradBFile" -Destination "$gradBResultRoot/runtime/$gradBArm/$gradBFile"
    }
}
```

Script проверяет exit code каждого build и наличие каждого allowlisted file перед следующим шагом; при ошибке сохраняет receipt и не запускает learner. Скрипт также выполняет предписанные выше initial copy/hash действия и final manifest, опущенные в этом build-only фрагменте. Публичный telemetry API появляется в reference **до** компиляции host. Runtime B сохраняет собственную TensorCore B.

### Оценка 48 ответов и сводка того же обучения

Обе Node-команды выполняются **после двух completion.json с 8 completed steps и 24 raw answers**, с одинаковыми 24 уникальными eval ID. Они читают результаты тех же 16 updates/48 generations; новых Train/Generate/MatMul/teacher calls нет. При неполном плече сохранить ошибку, не дополнять ответы новым запуском.

`node <result-root>/score.cjs` — exact archived wrapper, которому `root=__dirname`. Он использует frozen `inputs/score.cjs` и `inputs/panel-gold.json`, читает `before|after/raw-outputs.json` и receipts, затем пишет **`before|after/scores.json`, `before|after/score-summary.json` и общий `comparison.json`**. Отдельный quality compare executable не требуется: последнее сравнение уже входит в эту команду. В `comparison.json` фиксируются `correctBefore`, `correctAfter`, `gained`, `lost`, `identicalAnswers` и все 24 paired rows. Старые поля `userNonRegressionCriterion` и `qualityConclusion` являются **только quality-specific diagnostics**, не gate успеха стоимости или всей карточки. Scorer compatibility fields `TRANSLATED` и adapter ID служат входом прежней rubric и не доказывают прохождение настоящего Flow/verifier. [Реальный контракт scorer:2–15][scorer-source]

`compare-gradb.cjs` — новый малый documentation-specified reader, который надо реализовать в назначенном result root до запуска; его полный функциональный контракт фиксирован:

- Читает `preparation.json`, обе `completion.json`, `initial-state.json`, `training-material.json`, `training-steps.json`, `parameter-updates.json`, `trained-state.json`, `cost.json`, `environment.json`, `raw-outputs.json`, `scores.json` и общий `comparison.json`. Не вызывает native DLL, не импортирует старый finalize и не генерирует ответы.
- Проверяет ровно 8 сохранённых steps/optimizerStep 8 и 24 score/raw ID каждого плеча; одинаковые initial weights/reference/base/RNG/tokenizer/config/optimizer settings и byte-identical training material; unchanged base/reference и finite trainable state; gradB `calls > 0`. Несовпадение выполненной работы и multiply-add counts показывается как ограничение сопоставимости, а не speedup.
- Пишет `gradb-validation.json`: перечисленные проверки, реальные счётчики, hashes runtime/source, равенство или различие final policy/moment hashes. Numerical equality — отдельное наблюдение, не выдуманный PASS. Несовпадающие weights/moments сохранить как возможную численную цену, не стирать их и не запускать новую диагностику.
- Пишет `gradb-metrics.json`: по каждому плечу сумму 8 `elapsedMs`, `trainCpuMs`, `trainAllocatedBytes`, gradB elapsed milliseconds/calls/multiply-adds; mean из 8 pre-update `metrics.loss`, chosen/rejected rewards, margin, preference accuracy; из `comparison.json` — generated-answer counts/gained/lost; из `cost.json` — harness wall/CPU, peak working set, cumulative allocated bytes; checkpoint bytes из trained-state. Для каждой сопоставимой величины вычисляет B−A. Scope и знаменатель сохраняются, absent field → null/«не измерено», не0.
- Пишет `answers.md` с 24 paired raw texts, frozen rubric verdicts и gained/lost, а также `report.md` по таблице результатов этой карточки. В отчёте любые улучшения полезных показателей признаются успехами своих осей при показанных регрессиях; рост accuracy не является условием. `candidate-fix.md` ссылается только на loop-order factor, сохраняет статус решения о переносе за пользователем.

**Архивный `dpo-mask-live8-01/finalize.cjs` сюда не переносить и не запускать:** он ожидает removed component-guard/process-group файлы, DPO-only assembly change, изменения DPO backward IL и старые baseline paths, затем пишет отчёт именно прежнего mask-эксперимента. Его использование подменило бы фактор и вывод новой карточки. Сводка gradB не запускает finite differences или отдельный oracle. [Архивный finalize][old-finalize]

Копируемый host, script и instrumentation ещё необходимо реализовать по этому контракту. **Не выдавать команду существующего общего Rp01 CLI за реализованный micro runner.** До подтверждения доступности точных архивных binary inputs статус `ARCHIVED_INPUTS_TO_VERIFY`, а не «готово к запуску на любой машине».

## Учебный бюджет и измерение

- **A = 8 native DPO updates, B = 8**, по одной одинаковой паре на шаг, тот же порядок live8. Итого 16 updates; каждый новый arm стартует с C50, свежих m/v/step0 и одинакового RNG.
- Policy/base/reference/topology/tokenizer/material/β/LR/weight decay/dropout одинаковы. Base39/reference frozen, trainable16; current исправленная mask derivative одинакова.
- **После обучения 24 собственных ответа на плечо, всего 48** по существующей frozen live8 EXAM-панели; нет дополнительных уроков, warmup updates, before-model generation, синтетических contractions или повторных запусков.
- Всё, что входит в `DpoTrainer.Step`, включая публикацию, измеряется внешним Stopwatch. Внутренний gradB time показывает часть той же учебной работы. Отдельно записать фактический optimizer step, 16 m/v keys, base/reference hashes, finite state, token lengths и weighted mask sums.
- До/после **каждого Step** снять GC allocated bytes и CPU-process milliseconds. Если используется общепроцессная метрика, пометить её scope. Process peak RAM сохранять отдельно от cumulative allocations; не называть накопленные bytes RAM.
- Раздельно показать loading, 8 Steps, 24 generations, saving/audit output, total wall/CPU и state bytes. Дополнительная telemetry overhead одинакова и входит в обе стороны.
- Loss — pre-update native online mean по восьми шагам, не heldout. Pair preference accuracy — отдельна от точности 24 generated answers. Не включать время подготовительной сборки в native training speed без отдельной подписи.

| Обязательная строка результатов | A before implementation | B loop order | Δ и интерпретация |
|---|---|---|---|
| Native completed optimizer updates / expected | NOT_RUN / 8 | NOT_RUN / 8 | — |
| Actual gradB calls, multiply-add count | NOT_RUN | NOT_RUN | Должна совпадать выполненная работа |
| Время gradB внутри реальных Steps | NOT_RUN | NOT_RUN | Полезная метрика |
| Сумма native Step wall/CPU/allocations | NOT_RUN | NOT_RUN | Полезная метрика |
| Policy/m/v deltas; base/reference invariants | NOT_RUN | NOT_RUN | Связь с обучением и цена численной разницы |
| Online loss, chosen/rejected reward, margin | NOT_RUN | NOT_RUN | Не heldout |
| Собственные правильные ответы /24, gained/lost | NOT_RUN | NOT_RUN | Subject metric |
| Генерация, total harness, peak RAM, bytes | NOT_RUN | NOT_RUN | Остальные изменения, включая ухудшения |

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и профилю; рост accuracy не обязателен.** Снижение gradB/Step времени при прежних ответах — положительный результат обучения по стоимости. Изменение hash без полезной метрики — только изменение состояния. Один paired run даёт локальный результат: не вводить обязательные 20+200 повторов, не заявлять устойчивый speedup без данных и не отвергать наблюдённый выигрыш одним предположением о шуме.

## Конфликты, остановка и будущий перенос

User documentation-only scope сильнее старых production workflow gates: сейчас только эта карточка, без кода/запуска. При будущем прямом поручении выполнить опыт experimental host может использовать простой DTO, try/catch, void/throw; это не изменение правил продукта. Не менять `docs/plans/replanning-possible-diffs`, не запускать полный CI/Main500, не исправлять продуктовые tests, не делать commit/push. Существующие файлы/опыты не удалять и не перезаписывать.

Недостающий C50/runtime, несовпадение source provenance, admission failure, нулевой фактический reached gradB, nonfinite state или неоконченный предыдущий запуск фиксируются конкретными receipts; никакого нового learner/branch/checkpoint/обхода прав не выбирается. После успешного результата кандидат переноса — **только** `00.TensorCore/Core/TensorOps.cs`, gradB loop order. Люба отдельно решает, переносить ли его в оригинал; telemetry остаётся экспериментальной.

**Полный промпт исполнителю будущего опыта:** «Реализуй и исполни только GRADB-LOCALITY-ENRU-01 по точным путям/entrypoint/commands этого паспорта после отдельного поручения на запуск. Меняется только порядок скалярных циклов gradB внутри native LoRA backward; forward, gradA, DPO derivative и все данные одинаковы. Скопируй существующий live8 host, сохрани его настоящие допуски и две реальные восьмишаговые учебные траектории. Не делай benchmark MatMul вместо обучения. Измерь раздельно время/CPU/allocations настоящих updates и gradB внутри них, сохранённые m/v/weights, онлайн objective, 48 собственных ответов и все ухудшения. Любой измеренный выигрыш полезной метрики признай успехом по этой метрике и профилю независимо от accuracy. Не запускай дополнительные уроки/повторы, не меняй protected folder/product, не удаляй старые результаты. После отчёта оформи кандидат и передай решение о переносе пользователю».

[matmul]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/00.TensorCore/Core/TensorOps.cs#L679-L698
[lora]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/10.LoRA/LoraLinear.cs#L112-L125
[step]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L650-L697
[adjoint]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/16-additions-typed-state-and-restore.md#L116-L126
[p110]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/cards/P110-numeric-matrix-cost.md
[prior-plan]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/05-SMALL-EXPERIMENTS-AND-PROMPTS.md#L18
[shape]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/00-LOCAL-WIKI-RECOVERY-FIRST.md#L52
[host-project]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/harness/DpoLive.csproj
[host-program]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/harness/Program.cs


[scorer-source]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/score.cjs#L2-L15
[old-finalize]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/finalize.cjs
