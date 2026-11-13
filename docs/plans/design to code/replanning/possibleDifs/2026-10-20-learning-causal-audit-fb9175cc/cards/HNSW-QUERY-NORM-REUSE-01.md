# HNSW-QUERY-NORM-REUSE-01: меньше повторной арифметики при настоящем обучении KNN Unit

**Статус 20.10.2026: `AUTHOR_SETUP_REQUIRED / NOT_RUN`.** Источник механизма прочитан; новый host и экспериментальные копии ещё не создавались, не собирались и не запускались. Ниже задан проект отдельного малого опыта. Его реализация и запуск сейчас не назначены. Никакого измеренного ускорения эта карточка пока не утверждает.

## 1. Что проверяем и где это участвует в обучении

Получатель — `AgentInfrastructure.Logical.KnnLogicalUnit<string,string>`. В его настоящем `Train` сначала обучается BPE, затем для каждого урока создаётся вектор и вызывается `HnswIndex.Add`. Add многократно сравнивает один query со stored vectors, а cosine каждый раз повторяет сумму квадратов query. Гипотеза: вычислять эту сумму один раз на вызов `GreedySearch`, `SearchLayer`, `SelectNeighbours` дешевле при сохранении выученного состояния и ответов.

| Участок и значение | Pinned source |
|---|---|
| Native Train: BPE320 → HNSW `m8/efConstruction64/efSearch32` → foreach `index.Add(Vectorize(...))` | [KnnLogicalUnit.cs:132–154](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L132-L154) |
| Add → GreedySearch/SearchLayer/SelectNeighbours, в том числе reverse pruning | [HnswIndex.cs:49–97](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/06.VectorSearch/HnswIndex.cs#L49-L97) |
| Повторяемые сравнения: 2+2+1 call sites | [HnswIndex.cs:164–230](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/06.VectorSearch/HnswIndex.cs#L164-L230) |
| Каждый cosine заново считает `na += a[i]*a[i]` | [Distances.cs:10–20](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/06.VectorSearch/Distances.cs#L10-L20) |
| Native readout кодирует input обученным tokenizer, ищет index, возвращает `bestExample.Output` | [KnnLogicalUnit.cs:216–256](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L256) |

Это эксперимент полного процесса обучения маленькой non-neural памяти, с дополнительной детализацией стоимости Add. Изолированный цикл cosine, чтение готового JSON или сравнение констант его не заменяют. Домен первого опыта — **EN→RU перевод коротких технических инструкций через generic learned Unit**. Здесь не изучаются TranslationKnnStore, синтаксический StructuralKnn, DPO loss/LoRA или полный RP01. Адаптация на другие домены потребует отдельно выбранного малого материала; универсального выигрыша не предполагается.

## 2. Дедупликация и приоритет

Проверен corpus planning `fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c` (929 Markdown +23 таблицы) и core `fb9175cc752d41e1cb525e0609b7c96140940217` (372 защищённых Markdown). Поиск норм, cosine, cache/reuse, HNSW и адресных владельцев не обнаружил такого one-factor опыта. Ближайшие известные семейства: P18.1 shape/norm completeness; P21 aliases/nonfinite/atomic Add; P22 RNG; P20 reachability; P107 neighbor aggregation. Новая карточка их не переименовывает и не чинит одновременно. KH-COST-01 и AST-SIB-CACHE-01 относятся к подготовке AST; IDENTITY-TRAIN-INDEX-01 — к ledger в другом learner.

Строка нового registry: `id=HNSW-QUERY-NORM-REUSE-01; class=TRAINING_COST; status=AUTHOR_SETUP_REQUIRED; domain=EN_RU_GENERIC_KNN; depends_on=active_learning_unblock_and_card_conflicts; promotion=USER_DECIDES`. Она помещается после действующих блокировок и конфликтов обучения, в добавочную очередь новых учебных cost-гипотез; **не меняет текущий NEXT и не назначает повтор уже завершённого опыта**. Полный порядковый номер и следующий ID задаёт единый registry публикуемого пакета. Пока эта строка явно не назначена следующей, исполнять её нельзя. При отдельном поручении только этого ID NEXT=END. В явно разрешённой общей авторской очереди A5→№46 IDENTITY-M1-01; это переход подготовки в той же разрешённой фазе, не автоматическая команда запуска.

## 3. Обязательные пользовательские правила для этой карточки

**Любое фактически измеренное улучшение полезного показателя считается успехом по этому показателю и данному домену; рост точности для этого не обязателен. Рядом показываются все другие изменения, включая ухудшения.** Снижение времени при нарушенной эквивалентности — наблюдение своей оси, но не доказательство обещания «та же выученная модель дешевле». Одна пара tiny timings — однократное наблюдение, не доказанное устойчивое ускорение и не основание для обещаний production.

**STOP/NEXT:** при прямом «стоп», «закончить немедленно» прекратить действия немедленно: без дополнительного чтения, hash, review, оформления отчёта, commit, cleanup или нового запуска. Допустимо только прекратить уже работающий собственный процесс, если иначе он продолжит обучение. При «следующий эксперимент» прекратить прежний опыт и перейти только к указанному следующему: не дописывать прежний candidate, не чинить его tests/CI, не делать «последнюю» проверку и не запускать снова. Если следующий ID не назван и registry его не определяет, сообщить известное состояние и запросить только ID; предыдущий опыт не возобновлять. Ни старая карточка, ни шаблон не разрешают игнорировать прямую команду пользователя. Процитированное пользователем поведение, когда агент четыре раза продолжил прежнюю работу вместо NEXT, **запрещено в этой карточке**.

Модель исполнителя — default модель текущей сессии; не менять model/provider/reasoning. Для выбора и человеческой оценки учебных DPO роль закреплена за **CODEX Astra 6 ultracode**. Открытая китайская внешняя модель может только генерировать новые DPO в отдельно назначенной задаче. Здесь повторно используются уже сохранённые `sourceId=simulation` lab fixtures; внешних обращений и новой генерации DPO нет. Не приписывать этим fixtures внешнее происхождение или допуск в production.

Автор/исполнитель применяет указанные repo skills: `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md` и `human-communication-GLOSSARY.md`. Здесь их роли фиксированы: архитектурная причинность → план малого host → review факторов/бюджета → review собственного source-copy host → русский человеческий отчёт. User limits выше общих требований полного production pipeline.

Старые experiments и их raw результаты не удаляются. Собственная лабораторная source-copy адаптация, fixed seed, reflection для снимка выученного индекса и отдельный маленький проект допустимы только в этом опыте; автоматически переносить их в production, submission или общий framework нельзя. Сейчас разрешено документирование; никакой новый runtime не выполнен.

## 4. A0/B0 и A/B: настоящий учебный переход

Модель: `KnnLogicalUnit<string,string>`, `unitId="hnq01-enru"`, `unitRole="translation-executor"`, `domainTag="TranslationDomain"`, dim64, k3, confidenceFloor0.70, `textSelector: s => s`, пустые PredicateIds. Общий seed HNSW —7331 в обоих arm, только в экспериментальной копии. Для всех native запросов `LogicalExecutionFlags` с обычными значениями по умолчанию; Metadata пустой. Флаг confidence floor оставляет предупреждения, не подменяет output.

| Состояние | Что реально выполнено | Измерение |
|---|---|---|
| A0 | Original distance, native `Train(tr01..tr04)` | Warm learned state, 24 обычных readout; manifest и HNSW snapshot |
| B0 | Proposed local norm reuse, native `Train(tr01..tr04)` | Тот же warm материал и readout |
| A | Original distance, новый native `Train(tr01..tr08)` | Полное измеренное расширенное обучение и 24 readout |
| B | Proposed local norm reuse, новый native `Train(tr01..tr08)` | Те же восемь уроков, измерение и readout |

Это **переобучение на расширенной памяти 4→8**, потому что существующий static Train заново обучает BPE и перестраивает HNSW. Public API продолжения полного generic Unit из checkpoint здесь не существует; выдумывать `LoadUnit/TrainOnExample` запрещено. Warm C0 создаётся четырёхстрочным настоящим Train. Это также естественно прогревает часть JIT без отдельного warmup; всё его обучение включено в бюджет. В каждой новой arm процесс начинается свежим; RNG далее расходуется Train4→Train8 одинаково, запросы RNG не используют.

### Замороженный материал

Core SHA всех следующих файлов: `fb9175cc752d41e1cb525e0609b7c96140940217`.

Базовый путь `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/` **только читается**:

| Файл | Git blob SHA | Как использовать |
|---|---|---|
| `training-batch-B.json` | `0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c` | Восемь examples в сохранённом порядке. `InputJson.SourceText` → Input; `ChosenOutputJson.Text` → Output. Полный original JSON сохранить для provenance. |
| `eval-inputs.json` | `585bf30cad84b57b2e1f3fe8c00f171f2348c313` | Все 24 source строки, без подмены/фильтра по результату; case IDs и buckets служат только отчёту. |
| `panel-gold.json` | `af73d89cfd5b42f954380437bc5596b46dae9498` | Accepted variants читаются только scorer после сохранения соответствующих raw predictions. Никогда не передавать Train, textSelector или query metadata. |

Фиксированные уроки:

| ID | Учебный input | Учебный output |
|---|---|---|
| tr01 | validate the file | проверь файл |
| tr02 | delete the cache | удали кэш |
| tr03 | copy the report | скопируй отчёт |
| tr04 | move the config | перемести конфигурацию |
| tr05 | upload the image | загрузи изображение |
| tr06 | download the package | скачай пакет |
| tr07 | save the draft | сохрани черновик |
| tr08 | archive the log | архивируй журнал |

Создание rows: `new LogicalKnnTrainingExample<string,string>(id, source, target, [], ["translation"], ["P125-enru-memory-01:"+id])`. Восемь строк — существующие supervised lessons из simulation fixture; новый опыт **не реализует neural DPO objective**, не использует rejected как дополнительный фактор и не повторяет историческую P125 quality-проверку. Источник сохранён, но тип получателя отличается и это явно видно в отчёте.

Панель 24 разделить по сохранённым buckets: exact8 (учебный readback), near3, composition8, edge5. Exact x01..x04 — старые уроки, x05..x08 — новые. Их точность нельзя называть held-out generalization. Остальные 16 — отличные input строки, но принадлежат очень малой общей теме; они не доказывают качество произвольного перевода.

## 5. Ровно одно изменение и контролируемые адаптации

Original `Distances.cs` сохраняется без изменений. В B-копии `HnswIndex.cs` добавить только локальные helpers ниже и провести пять original call sites через `CosineWithQueryNorm`. В каждом из GreedySearch/SearchLayer/SelectNeighbours один раз в начале вычислить `float querySquaredNorm = QuerySquaredNorm(query);`. Порядок кандидатов, сортировки, сравнения, early termination, maxM, ef, k и сохранение state остаются прежними.

```csharp
private static float QuerySquaredNorm(float[] query)
{
    float na = 0f;
    for (int i = 0; i < query.Length; i++)
        na += query[i] * query[i];
    return na;
}

private static float CosineWithQueryNorm(float[] a, float[] b, float na)
{
    float dot = 0f, nb = 0f;
    for (int i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        nb += b[i] * b[i];
    }
    float denom = MathF.Sqrt(na) * MathF.Sqrt(nb);
    return denom < 1e-9f ? 0f : dot / denom;
}
```

Пять replacements точно: `query,_vectors[entry]` ×1; `query,_vectors[nb]` ×2 (GreedySearch и SearchLayer); `query,_vectors[e]` ×1; `query,_vectors[c]` ×1. Count-проверка преобразования — документная защита source shape, без вызова learner. Не заменять cosine одним dot, не переупорядочивать FP32 накопление dot/nb, не использовать double/SIMD, другой epsilon или новую нормализацию векторов. Равенство runtime результатов не предполагать: проверить по уже собранным артефактам.

**Scope mutable aliases.** Норма не сохраняется в поле HNSW, dictionary или checkpoint; она исчезает после одного вызова метода. Native Unit синхронно создаёт новый vector для Add и не публикует mutable alias наружу. Опыт не выполняет конкурентную мутацию векторов. Это ограничение не является исправлением существующего публичного ownership-контракта. На восемь vectors reverse-pruning ветка с maxM16 не активируется; её поведение не объявляется экспериментально покрытым.

Общие адаптации A/B: заменить `private static readonly Random _rng = new();` на `new(7331)`; одинаковая telemetry Add; изолированный project/source closure; одинаковые fixtures. Ничего другого не менять в native Unit/BPE/index. Общие адаптации перечислить отдельно от независимого фактора.

## 6. Точный source-copy closure малого host

Repo root будущего автора: `C:/Projects/llm_mvp_core`. Canonical новый эксперимент: `experiments-v2/RP01-MICRO/HNSW-QUERY-NORM-REUSE-01/`. Только его `harness` содержит новые Python scripts и text templates. Исполняемые C# copies размещаются в **`C:/xgi/qn01/r1`**, а не под текущим 35.csproj: [существующий ExperimentsV2.csproj](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/35.ExperimentsV2/ExperimentsV2.csproj) имеет default Compile glob и не исключает будущие вложенные Program.cs.

Автору создать только перечисленные новые шаблоны/инструменты: `harness/prepare.py`, `harness/run.py`, `harness/report.py`, `harness/Program.cs.txt`, `harness/LabProbe.cs.txt`, `harness/Hnq.csproj.txt`, `harness/manifest.json`. Они **планируются**, сейчас их нет. Никакого поиска альтернативного runner или запуска существующего CLI вместо них.

`prepare.py` обязан извлечь перечисленные bytes командой `git -C C:/Projects/llm_mvp_core show <pinned-commit>:<path>` через argv-array/subprocess, проверить Git blob hash исходных bytes и сохранить exact raw рядом с transformed copies. UTF8 BOM и CRLF допустимо нормализовать только **после** проверки original hash; факт преобразования записать. SHA256 card/templates/copied inputs фиксируется до первой arm. Git HEAD может измениться чужими документами; исходный материал всё равно берётся с указанного pin. Недоступный pinned blob — `PINNED_SOURCE_MISSING`, без замены snapshot.

| Original path | Git blob SHA | Локальная копия в каждом `<arm>/src` |
|---|---|---|
| 00.Core/OperationResult.cs | `2bfde76ca7d99f63eb657e736330da177838c644` | `OperationResult.cs`, полный файл |
| 00.Core/LogicalContracts.cs | `fbba6f97f226d96d0d36ed08d6befa8338e86434` | `LogicalContracts.cs`, точный prefix до единственного marker `// This record carries a request to execute one logical flow.`; это original строки 1–111 с flags/request/result/PhaseTransitionSignal/ILogicalUnit |
| 29.SemanticCompiler/Core/Core.cs | `0dc09344f15816c11f23e3166bfe8e24ff9fada9` | `SemanticCore.cs`, полный самодостаточный файл с Domain enum |
| 01.Tokenizer/BpeTokenizer.cs | `d7c5e69308a32bf832ec4b087097a35b26f51d64` | `BpeTokenizer.cs`, полный файл |
| 01.Tokenizer/RegexSplitter.cs | `ad3eed613fb90777af20ed07b6f8340b8c16bfd9` | `RegexSplitter.cs`, полный файл |
| 01.Tokenizer/VocabEntry.cs | `396257edb67bf0c409362f487c3d0b6d10b4225d` | `VocabEntry.cs`, полный файл |
| 06.VectorSearch/IVectorIndex.cs | `e952a8679d098b9e511f2a656b98f7c55b044504` | `IVectorIndex.cs`, полный файл |
| 06.VectorSearch/Distances.cs | `3bef456735d33aa23228c6aee72dde308a09bcac` | `Distances.cs`, полный файл, одинаковый A/B |
| 06.VectorSearch/HnswIndex.cs | `03287c05bcd6c4422d679a3b2e37a03f418918d6` | `HnswIndex.cs`, полная copy с описанными адаптациями/фактором |
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | `2957816a88aa9cdcd3020bb81be56b5dfff165e0` | `KnnLogicalUnit.cs`, полный файл без замены learner |

`Program.cs` и `LabProbe.cs` автора размещаются рядом с `Hnq.csproj` в `C:/xgi/qn01/r1/A` и `/B`. В `input` лежат три frozen JSON. В `results` лежат только новые raw файлы этого run. Максимальная длина всех output/source/bin/obj путей при подготовке —200 символов; при превышении использовать уже фиксированный короткий root, не менять настройки ОС.

Шаблон `Hnq.csproj.txt`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>Hnq</AssemblyName>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="LabProbe.cs" />
    <Compile Include="src/*.cs" />
  </ItemGroup>
</Project>
```

Нет ProjectReference, PackageReference, solution/dispatcher edits или сборки общей цепочки. SDK .NET9 и Python3 уже должны быть доступны у исполнителя; в противном случае автор фиксирует конкретный setup failure, не меняет модели/окружение/систему автоматически.

## 7. Однозначный контракт Program и probes для автора host

Реализовать этот контракт в указанных новых файлах; новых задач по поиску интерфейсов исполнителю не оставлять.

1. `Program.cs` принимает ровно четыре аргумента: `A|B`, абсолютный input directory, новый output directory arm, `run_id` (строго `run01`). Отвергает existing arm result directory с raw/issued.marker; ничего не удаляет. Проверяет ровно 8 unique training IDs, 24 unique eval IDs, ожидаемые source/target первых восьми, `sourceId=simulation`, одинаковые input blobs.
2. Конструирует supervised rows описанной проекцией. `Train` вызывается из одного синхронного потока, без Tasks и benchmark loop: `KnnLogicalUnit<string,string>.Train("hnq01-enru","translation-executor","TranslationDomain",rows, s=>s,64,3,0.70)`. При `!IsSuccess` сохраняет error code/message; native результат не заменяется fake Unit.
3. Перед warm Train сбрасывает LabProbe. Записывает actual count4, внешний time/CPU/alloc и native Manifest. Для измерения полного Train: Stopwatch.GetTimestamp/Stopwatch.GetElapsedTime; `GC.GetAllocatedBytesForCurrentThread`; process TotalProcessorTime. CPU получить отдельно от synchronous allocated/time region, сохранить границы и объяснить погрешность. Без forced GC.
4. После warm Train выполняет все 24 `ExecuteAsync(new LogicalUnitRequest<string>(runId,"hnq01-enru","translation-executor","TranslationDomain",source,new LogicalExecutionFlags(),new Dictionary<string,object>())).GetAwaiter().GetResult()`. До/после точно 24 вызова, без дополнительных scorer-generated запросов. Сохраняет raw output/confidence/evidence/warnings/error для каждого case, а также `BitConverter.DoubleToInt64Bits(confidence)`.
5. Сохраняет raw predictions **до чтения gold scorer**. Scorer оценивает accepted exact после normalization из раздела 8. Если на warm x01..x04 нет хотя бы одного настоящего правильного ответа, статус `INVALID_LEARNING_PANEL`; main Train не выполняется, result сохраняется, самовольного выбора новой панели/увеличения бюджета/повтора нет. Такой исход не выдаётся за учебный эксперимент с 0→0. Никакой guard не рисует правильный answer из gold.
6. Сохраняет warm HNSW через reflection поля `_index` native Unit и его настоящий `Save`. Reflection только читает ссылку; не редактирует private данные. Это **snapshot индекса**, не обещание полного restore Unit. Hash input rows и actual manifest сохраняются. Всё сохранение вне train intervals.
7. Сбрасывает probe и делает один измеренный native Train на всех 8 строках. Записывает те же metrics/manifest; повторяет 24 native readout и snapshot. Если после обучения нет хотя бы одного actual correct exact ответа, `INVALID_LEARNING_PANEL/REGRESSION`; не писать «успешное обучение 0→0». Не менять данные или запуски для улучшения отчёта.
8. Записывает `A0.json/A.json` либо `B0.json/B.json`, `before-predictions.json`, `after-predictions.json`, `index-before.json`, `index-after.json`, `status.json`; `issued.marker` создан до первого native Train. При exception сохраняет доступное `status.json`/raw, original stdout/stderr. Учебная программа после записи завершается.

### Общая telemetry Add

В обеих source копиях HNSW ввести одинаковый `using var scope = LabProbe.MeasureAdd();` в начале `Add`. Scope — struct IDisposable, измеряет elapsed ticks и thread allocations этого единственного Add, считает AddCalls. Ничего не выделяет внутри tight loop для каждой cosine; список per-Add записей допустим только при завершении Add, его allocation учитывать вне самого Add interval и одинаково в обоих arm. На каждый scope store {ordinal,elapsedTicks,allocatedBytes,cosineCalls,queryNormBuilds,queryNormElements}. Полный внешний Train остаётся главным интервалом, включающим неизбежную telemetry overhead.

A: пять HNSW call sites проходят через тонкий `OriginalCosineWithProbe(a,b)` — увеличить только active-Add CosineCalls на 1, затем вызвать **неизменённый** `Distances.CosineSimilarity`. B: симметричный `PatchedCosineWithProbe(a,b,na)` увеличивает тот же CosineCalls на 1 и вызывает `CosineWithQueryNorm`. В обеих копиях в начале каждого GreedySearch/SearchLayer/SelectNeighbours выполняется одинаковый `LabProbe.QueryMethod(query.Length)`; он считает число таких вызовов и сумму их длин. Вне активного Add counters не изменяются. Не делать дополнительных отдельных counter increments только для A на каждой дистанции: это создало бы искусственную разницу telemetry overhead. В report **вычислить** norm-builds A=CosineCalls, B=QueryMethodCalls, norm-elements A=CosineCalls×64, B=QueryMethodLengthSum, поскольку dim64 фиксирован и original/variant loops прочитаны. Сохранить эти derived diagnostics отдельно от измеренных wall/CPU/bytes. Helper return values соответствуют native API; probe не участвует в выборе кандидатов или labels.

Число посещённых элементов нормы — диагностический расчёт по счётчикам фактически пройденного учебного пути и фиксированным loop bounds, **не измерение времени процессора**. Он объясняет фактор и может показать сокращение учебной арифметики; один этот proxy не означает ускорения полного Train. Instrumentation overhead записывается как ограничение timing. Первый Add в каждом native Train естественно имеет 0 cosine и служит отрицательным контролем без отдельного эксперимента. Число сравнений/графы в A/B должны совпасть для атрибуции; при расхождении не скрывать его.

## 8. Scorer, метрики и бюджет

Фиксированный scorer этого опыта: Normalize(string) = lowercase invariant → ё→е → trim Unicode punctuation/symbols только по границам → collapse всех Unicode whitespace в один пробел. Exact-correct iff normalized native output равен одному normalized `accepted` из panel-gold. Сохранить version=`HNQ_ACCEPTED_EXACT_V1`, нормализованный output и совпавший accepted ID. Это явно выбранный строгий scorer; он **не равен** прошлому семантическому P125 scorer, поэтому P12519/24 с ним не сравнивать. Не использовать semantic action/object regex как второй незаявленный критерий.

| Полезный показатель | Как показать before/after и A/B |
|---|---|
| Full native Train elapsed / process CPU / managed allocated bytes | A0,B0 отдельно; главное сравнение A↔B на одинаковых 8 rows; signed delta и unit ms/bytes |
| Сумма Add elapsed / Add allocations | Внутри каждого Train, raw per-Add и сумма; не выдавать за всё обучение |
| Query norm elements/builds, cosine calls, Add calls | Cosine/Add/query-method calls — actual telemetry; norm elements/builds — derived arithmetic diagnostics, не физическое ускорение |
| Learned correctness | correct/total отдельно old exact4, new exact4, near3, composition8, edge5; A0→A и B0→B; difference-in-differences для изменения качества, без обещания causality между разными learners |
| Все errors/warnings/confidence | Полные raw строки; coverage = успешные native ответы/24; confidence не accuracy |
| Сохранённая выученная память | index Count4→8, ordered vectors/IDs/graph; A0↔B0 и A↔B byte/hash parity индекса и содержательных outputs/confidence/evidence. У manifest сравнивать только identity/config/count/tokenizer/index-kind/ingredients. TrainingTimeMs и ManagedAllocationBytes исключить из semantic parity и показать как расходы. Отдельно нет полного checkpoint restore |

Любое measured полезное улучшение записать в candidate с конкретной метрикой и доменом; не требовать accuracy uplift. Для утверждения «эквивалентное обучение дешевле» нужна parity индексов и actual before/after outputs/confidence/evidence; иначе статус `INVALID_EQUIVALENCE` рядом с любыми полезными наблюдениями. Отрицательный/нулевой результат тоже сохраняется. **Одна A→B пара не доказывает устойчивость маленького time delta**; повтор ради статистики/красивого результата в эту карточку не входит.

| Операция | На arm | Общий потолок |
|---|---:|---:|
| Native Train4 + Train8 | 2 | 4 |
| Учебные presentations / HNSW Add | 12 | 24 |
| Native ExecuteAsync до/после | 48 | 96 |
| Результатные HNSW Save | 2 | 4 |
| Дальнейшее optimizer/LoRA/DPO обучение, генерации, extra parity-training | 0 | 0 |
| Дополнительный warmup, повтор timing, reverse arm order, новый corpus | 0 | 0 |

A→B в двух свежих процессах. Максимум 120 секунд на каждый процесс и 300 секунд весь run после build; лимит — аварийная граница, не причина урезать 8 фиксированных уроков. Timeout означает `INCOMPLETE_BUDGET` с actual выполненным количеством, не PASS и не разрешение повторить. Минимальный набор 8 уроков покрывается заранее выбранной программой; если не успел, это setup/runtime проблема для следующего решения пользователя. Build собственной копии допускается починить до first issued без изменения фактора/данных/метрик, но после первого native Train не добавлять повтор arm. Никакой полный test suite не требуется для гипотезы, успех которой ещё не доказан.

## 9. Точные будущие команды и конфликтные состояния

**Это команды проектируемого author-owned host, а не существующего репозиторного CLI.** Они станут исполнимы только после создания файлов из разделов 6–8. Статус до этого — `AUTHOR_SETUP_REQUIRED`; исполнитель не угадывает путь или аргументы.

```powershell
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNSW-QUERY-NORM-REUSE-01/harness/prepare.py --repo C:/Projects/llm_mvp_core --work C:/xgi/qn01/r1 --source fb9175cc752d41e1cb525e0609b7c96140940217
dotnet build C:/xgi/qn01/r1/A/Hnq.csproj -c Release --nologo
dotnet build C:/xgi/qn01/r1/B/Hnq.csproj -c Release --nologo
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNSW-QUERY-NORM-REUSE-01/harness/run.py --work C:/xgi/qn01/r1 --run-id run01
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNSW-QUERY-NORM-REUSE-01/harness/report.py --work C:/xgi/qn01/r1 --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNSW-QUERY-NORM-REUSE-01/results/run01
```

`run.py` запускать через subprocess argv с timeout120, shell=false: сначала `dotnet C:/xgi/qn01/r1/A/bin/Release/net9.0/Hnq.dll A C:/xgi/qn01/r1/input C:/xgi/qn01/r1/results/A run01`, затем такую же команду B. При invalid warm A или failed A следующий B автоматически не выдавать за полную пару: зафиксировать partial и завершить. No rerun. Stdout/stderr пишутся в новый run path; build outputs сохраняются отдельно как setup и не считаются learning results.

`report.py` только читает уже сохранённые raw и выдаёт `report.md` на русском, `metrics.json`, `candidate.md`, `provenance.json`. Он не вызывает Train/Execute/Load, не извлекает новые ответы из модели. Один финальный readback для фактических опечаток/ссылок, затем NEXT. При явном NEXT/STOP этот readback не выполняется.

Конфликты заранее разрешены так: existing identical unissued own templates/copies можно продолжить готовить без переименования ID; неидентичные чужие файлы → `PATH_CONFLICT`, сохранить точные paths и не перезаписывать. Existing `issued.marker` или raw этой arm → сохранить и оформить known partial, не удалять каталог, не создавать тайный run02. Source blob/shape mismatch → `SOURCE_MISMATCH`, не смешивать версии. Missing .NET/Python/source → точный setup status и одна названная недостающая dependency. Никаких правок protected, production, Tests/CI/solution, восстановления чужих веток или поиска другого эксперимента. Повторный вопрос о GitHub permission для чтения source не нужен: авторизация пользователя уже дана.

## 10. Отчёт и кандидат для решения пользователя

Русский `report.md` содержит: что меняли и почему это часть Train; domain/model/Unit identity; source+fixture+host SHA; actual run status и budget; A0/B0/A/B table с числителями/знаменателями; все ms/bytes/count deltas и regressions; raw links; parity; ограничения single observation, 8-node graph и simulation. Human summary открывается конкретным полезным показателем, если он изменился; ID вторичен. При нулевом или невалидном результате написать это прямо.

`candidate.md`: status measured-success-by-metric / no-useful-effect / invalid; точный полезный эффект и домен; ссылка на исходное место `06.VectorSearch/HnswIndex.cs` methods GreedySearch/SearchLayer/SelectNeighbours; proposed production change только scoped query norm helper; сохраняемые numerical/ownership contracts; какие измерения ещё не покрыты. **Решение о переносе принимает пользователь.** Пока успех не измерен и перенос не выбран, production не меняется, fullchain tests не чинятся, PR с кодом не создаётся.

Если пользователь позже выберет перенос: отдельная задача переносит только локальный helper в original HNSW и проверяет затронутый native learner с реальными incoming workloads; public aliases остаются основанием не вводить постоянный norm cache. Seed7331, source slicing, reflection и probes — экспериментальные адаптации, они не входят в предлагаемую production-правку. Это будущая карта переноса, не назначение выполнять её после результата автоматически.

## 11. Полный промпт исполнителю

> Работай только по HNSW-QUERY-NORM-REUSE-01 и её назначенной строке общего registry. При отдельном поручении этого ID NEXT=END; в явно разрешённой общей авторской очереди следующий№46 IDENTITY-M1-01. Manifest costs TrainingTimeMs/ManagedAllocationBytes сравнивай как расходы, не требуй их byte parity; parity индекса/содержательных outputs и семантических manifest полей сохраняется. Сейчас карточка `AUTHOR_SETUP_REQUIRED / NOT_RUN`: текущая задача — документация, запуск не назначен. Когда автору явно назначено создать host, создай ровно новые templates/scripts из раздела 6, следуя source pins, native API, данным и контракту разделов 7–8; не ищи другой проект или runner. Если назначено выполнение после author setup, используй только команды раздела 9, A→B, default модель сессии без override. Применяй перечисленные repo skills архитектора, планирования/review, code review, experiment design, prompt authoring и человеческой коммуникации. Отчёт и candidate — по назначенным путям, на русском. CODEX Astra 6 ultracode отвечает за выбор/человеческую оценку DPO; здесь уже сохранённые simulation fixtures и внешних моделей нет.
>
> Единственный фактор — локальная норма query на пять cosine call sites HNSW; участвует в настоящем KnnLogicalUnit.Train → Add. Original Unit и BPE используются source copies, без переписывания learner. A0/B0 реально обучаются 4 урокам; A/B новым native Train на 8 уроках; до/после обычные ExecuteAsync. Не подставляй gold как model output. Gold scorer читает после raw, exact TRAIN readback не называй held-out. Warm и after обязаны иметь хотя бы один настоящий correct exact; 0→0 не выдавай за валидный учебный результат. Бюджет максимум 4 Train/24presentations/96 Execute; никаких повторов, extra warmup, новых fixtures, brute-force microbench вместо обучения или полного CI. Любое измеренное улучшение полезного показателя — успех своей оси и EN_RU generic-KNN домена; accuracy uplift не обязателен. Все регрессии рядом. Tiny timing — single observation, а не stable speedup. При parity failure сохрани полезные observations, но не заявляй «та же модель дешевле».
>
> Pinned source/protected читаются; production/Tests/CI/solution/старые experiments не меняются и не удаляются. Короткий root C:/xgi/qn01/r1 обязателен; existing issued/raw не удалять ради rerun, run02 не создавать. Лабораторные исключения не распространяются на production. Ошибка setup исправляется только в собственном невыданном host в рамках той же карточки; отсутствующую source/dependency называй точно, не заменяй опыт и не ищи модель/метрики заново. После измерения только русский report и кандидат; пользователь сам решает production перенос.
>
> **Абсолютный запрет повторить поведение из пользовательской цитаты:** по «следующий эксперимент» немедленно прекрати прежний и перейди к назначенному NEXT, без ещё одного report/hash/review/commit/теста или исправления прошлой карточки. По «стоп/закончить немедленно» немедленно остановись; допустимо только прекратить свой уже работающий процесс, если иначе он продолжит обучение. Не совершай других tool calls, cleanup или обновлений памяти. Сообщи уже известное состояние. Старое поручение не даёт права продолжить прежний опыт после прямого указания пользователя.
