> **Согласование9d311:** S04 — единственный новый distinct factor этого пакета. Предшественник NG01, NEXT исходный16 IDENTITY-M1-01, затем исходный32 KN-ROUTER. SKNN — reference alias существующего AST, не ещё один запуск. Все данные/Train budgets/host/result-root этого BPE рецепта сохранены. Текущая фаза документная.

# BPE-REALIZED-MERGE-COUNT-01: выбирать слияние по числу исполнимых замен

**Статус: DESIGNED_NOT_RUN.** Карточка описывает будущий малый опыт. Сейчас разрешено только документирование: исходный код продукта, защищённая папка `docs/plans/replanning-possible-diffs`, сохранённые результаты и старые эксперименты не изменяются. Новый runner ещё не создан, команд обучения не выполняли, измеренного успеха этой гипотезы нет.

**Место в единой очереди — S04**, после NG01. Следующий — исходный №16 IDENTITY-M1-01, затем исходный №32 KN-ROUTER и END. ROPE и AST из пакета63 сохранены; SKNN не отдельный запуск. Текущий NEXT всей очереди — №05 N62-R2. Исполнение только после отдельного назначения этого ID; наличие карточки не разрешает запуск.

## 1. Вопрос, тема и получатель обучения

Тема — язык программирования и документация кода; малый материал — символы C# lambda-выражений и строк комментариев. Получатель изменения — настоящий `Tokenizer.BpeTokenizer`: в `Train` меняются vocabulary и таблица merges, затем `Encode` использует эту выученную таблицу. Два профиля EXAM — lambda и комментарии — не объявляются двумя новыми доменами. Нейтральный профиль нужен для контроля изменений на строке без выбранной пары.

В исходном алгоритме `CountPairs` учитывает все соседние позиции, включая перекрывающиеся одинаковые пары. `MergeChunk` заменяет найденную пару слева направо с шагом два. Для серии из L одинаковых token IDs число пар равно L−1, а исполнимых замен — floor(L/2). При конкуренции разных пар эти критерии могут выбрать разное первое слияние.

**Гипотеза:** выбор по числу реально исполнимых замен может эффективнее расходовать ограниченный бюджет merges для части учебных материалов. Возможна обратная сторона: ухудшение токенизации повторяющихся символов и иной предметной темы. Это альтернативная цель обучения BPE, а не установленная ошибка стандартного BPE. Ухудшение RP01, улучшение нейросетевой точности или KNN-ответов этой гипотезой пока не доказаны.

## 2. Код, запись состояния и настоящий потребитель

Все core-ссылки закреплены на `883496bca18113bf37e0afe683c75b05a0075048`, ветка `codex/dpo-implementation`.

| Звено | Первичный источник | Что следует из кода |
|---|---|---|
| Учебный выбор и запись | [BpeTokenizer.Train,42–68](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L42-L68) | CountPairs → MaxBy → запись `_merges` и `_vocab` → MergeAll. Фактор меняет решение непосредственно внутри обучения. |
| Исходная частота | [CountPairs,526–538](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L526-L538) | Каждая соседняя позиция увеличивает счётчик. |
| Исполнение слияния | [MergeChunk,548–565](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L548-L565) | После совпадения `i += 2`, поэтому два соседних экземпляра одной self-pair нельзя исполнить одновременно. |
| Использование выученного состояния | [Encode,73–82](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L73-L82); [ApplyMerges,568–574](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L568-L574) | Encode применяет записанные merges в выученном порядке. |
| Применение в обучении единицы | [KnnLogicalUnit.Train,136–153](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153); [Vectorize,266–282](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L266-L282) | Caller обучает BPE с vocab320; затем Encode участвует в векторах, добавляемых в HNSW. Это реальный downstream consumer, а не только счётчик отчёта. |

В малом опыте используется vocab257, чтобы изолировать **один выбор** и не смешивать его с накоплением последующих расхождений. Это диагностическая граница. Она не доказывает пользу при vocab320, не оценивает всю KNN-цепочку и не подменяет её результат компрессией. После отдельного решения Любы потенциальное место изменения продукта — только `01.Tokenizer/BpeTokenizer.cs::CountPairs`; способ изменения задан ниже. Автоматического переноса нет.

## 3. Дедуп: новый фактор внутри известного семейства

На planning `6bc4b6fba8f14870f5dc4966cf5a344754962d66` проверены поиском 747 Markdown документов possibleDifs; на core `883496b` — 172 доступных Markdown файла защищённого корпуса, включая 153 карточки. Поиск включал `CountPairs`, realized/realisable merges, фактическое сокращение, исполнимые слияния, overlap/non-overlap и семантически ближайшие BPE-карточки. Проверка не означает ручного прочтения каждой строки всех файлов.

Факт перекрывающегося счёта **уже известен**. [BPE-WEIGHTED-CHUNKS-01 и source-copy specification](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/bpe-source-copy-design.md#L25-L35) требуют его сохранять и называют non-overlap CountPairs другим алгоритмом. [Защищённая карточка BPE-WEIGHTED-CHUNKS-01](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/BPE-WEIGHTED-CHUNKS-01.md) повторяет это ограничение. BPE-NO-MATCH-COPY меняет лишние копии и сохраняет выбор merges.

Новая часть здесь — отдельный причинный A/B **другой цели выбора**, с конкурирующими парами, сохранением остального алгоритма и заранее показанной ценой для разных профилей. Такой отдельный опыт в проверенном корпусе не найден. Не считать карточку повторным «открытием overlap bug» и не складывать её с weighted/no-match как независимые доказанные причины неудачного обучения. Неизвестные незагруженные ветки этим выводом не покрываются.

## 4. Ровно одно различие A/B

**A:** неизменённые `BpeTokenizer.cs` и `RegexSplitter.cs` указанного commit.

**B:** те же файлы; заменить только тело `CountPairs` приведённым алгоритмом. Self-pair означает пару с одинаковыми левым и правым token IDs. Для иных пар перекрывающиеся вхождения невозможны, поэтому исходный счёт сохраняется.

```csharp
private static Dictionary<(int, int), int> CountPairs(List<List<int>> chunks)
{
    var counts = new Dictionary<(int, int), int>();
    foreach (var chunk in chunks)
    {
        int selfEdges = 0;
        for (int i = 0; i < chunk.Count - 1; i++)
        {
            var pair = (chunk[i], chunk[i + 1]);
            if (pair.Item1 == pair.Item2)
            {
                selfEdges++;
                if ((selfEdges & 1) == 0) continue;
            }
            else
            {
                selfEdges = 0;
            }
            counts[pair] = counts.GetValueOrDefault(pair) + 1;
        }
    }
    return counts;
}
```

Первое появление каждого ключа и tie policy MaxBy сохраняются. Regex, UTF-8, порядок чанков, корпус, target vocabulary, `_merges`, MergeChunk, Save/Load, Encode и остановка Train не меняются. Не добавлять weighted chunks, no-match fast path, сортировку пар, нормализацию текста, EOS-токен, dropout, другую tokenizer-библиотеку, дополнительный KNN/DPO/SFT запуск. Host вызывает native методы; он не реализует второй tokenizer на Python/JS или через формулу.

## 5. Полностью фиксированные данные

Будущий `C:/xg/br01/in/fixture.json` содержит ровно этот JSON, UTF-8 без BOM. TRAIN собирается соединением `train_lines` через LF (`\n`), без заключительного LF. Данные EXAM фиксированы сейчас и не подбираются после результата. Совпадение общих символов с TRAIN необходимо для проверки переноса merges; точного совпадения EXAM-строк с TRAIN-строками нет.

```json
{
  "id": "BPE-REALIZED-MERGE-COUNT-01",
  "domain": "code-documentation",
  "target_vocab": 257,
  "train_lines": ["///////", "x=>x", "y=>y", "z=>z", "w=>w"],
  "exam": [
    {"id": "E1", "profile": "lambda", "text": "a=>a"},
    {"id": "E2", "profile": "lambda", "text": "b=>b"},
    {"id": "E3", "profile": "lambda", "text": "c=>c"},
    {"id": "E4", "profile": "comment", "text": "/////////"},
    {"id": "E5", "profile": "neutral", "text": "q+r"}
  ]
}
```

Заранее выведенный из кода диагностический результат ниже — **не измерение**. Если native результат отличается, сохранить расхождение и завершить опыт; не менять fixture до совпадения с прогнозом.

| Объект | Before, byte IDs | Ожидаемый A после | Ожидаемый B после | Ожидаемая цена B относительно A |
|---|---:|---:|---:|---|
| Выбранная единственная merge | 0 merges | `//` (байты47,47) | `=>` (байты61,62) | Семантика намеренно различается |
| Весь TRAIN, token_count | 27 | 24 | 23 | На1 токен меньше |
| E1–E3, сумма lambda | 12 | 12 | 9 | На3 токена меньше |
| E4, comment | 9 | 5 | 9 | **На4 токена больше** |
| E5, neutral | 3 | 3 | 3 | Нет изменения |
| Все пять EXAM | 24 | 20 | 21 | **На1 токен больше** |

У A в TRAIN `//` имеет6 соседних вхождений, но только3 исполнимые замены; `=>` имеет4 и4. У B эти оценки3 и4. Это выбор ограниченного merge-budget, а не доказательство «B всегда лучше». В частности, заранее ожидается ухудшение общего EXAM-token_count при выигрыше lambda-профиля.

## 6. Реальное обучение, before/after и no-update контроль

Два независимых child-процесса, порядок A → B, без warmup и повторов. В каждом процессе:

1. Создать новый native BpeTokenizer и вызвать `Train("",256)` один раз для одинакового byte vocabulary. Сохранить canonical state и hash. Это initialization, отдельно от содержательного обучения.
2. Получить no-update контроль `C` через native сохранение и загрузку того же начального состояния: `using var stream = new MemoryStream(); tokenizer.Save(stream); stream.Position = 0; var control = BpeTokenizer.Load(stream);`. Явный rewind обязателен: native Save оставляет Position в конце, Load сам его не перематывает. У `C` не вызывать Train вообще.
3. Выполнить один before-readout пяти EXAM на обучаемом tokenizer и на `C`: actual IDs, token_count, Decode, round-trip equality. Дополнительно один Encode всего TRAIN на обучаемом tokenizer, только для baseline token_count.
4. Снять TRAIN counters, вызвать ровно один `Train(corpus,257)`, немедленно снять counters. Никакие Encode, Save, JSON parsing, report writing, hash/readout не входят в timed Train interval.
5. Сохранить состояние после, actual merge `(A,B,Result)`, vocab и hash. Повторить те же пять EXAM на обучаемом tokenizer и неизменённом `C`, плюс один Encode TRAIN на обучаемом tokenizer. Контрольные hashes и IDs должны оставаться прежними.
6. Записать raw JSON и завершить child. Controller только читает два уже полученных JSON и формирует сравнение; он не обучает, не вызывает Encode и не запускает детей повторно.

**Жёсткий бюджет:** 2 содержательных `Train(corpus,257)` +2 byte-vocabulary initialization, **всего4 вызова native Train API**; максимум2 learned merges суммарно; 40 пар EXAM Encode/Decode (2 процесса ×2 момента ×2 состояния ×5 строк), плюс4 TRAIN Encode; 0 NN forward/backward, 0 DPO/SFT/KNN fit, 0 LLM API calls. Один содержательный Train на плечо, без выбора лучшего seed/запуска. На один child — максимум30 секунд, на controller с двумя children —90 секунд. При timeout остановить собственный child, сохранить уже записанное, отметить неполные значения `UNKNOWN`, не повторять.

До сравнения подтвердить равенство corpus bytes/hash, fixture bytes/hash, A0/B0 canonical state, target vocabulary и исходных source hashes. A1/B1 **должны иметь разные merges при данном прогнозе**: различие не является ошибкой оптимизации, поскольку меняется учебный objective. Идентичные A1/B1 при выбранных данных означают `FACTOR_NOT_DISTINGUISHED`: гипотеза другого выбора merge не подтверждена. Это не отменяет отдельно наблюдаемых полезных ресурсных изменений; их и все регрессии сохранить с границей причинности. Изменение no-update контроля или разные входы дают `INVALID_COMPARISON`: raw сохраняется, но несопоставимую разницу не приписывать фактору. Успешный разбор JSON без native Train не является экспериментом.

## 7. Что измерить и как назвать успех

| Метрика | Как фиксируется | Граница интерпретации |
|---|---|---|
| Выученное состояние | A0/A1/B0/B1: canonical hash, vocab, actual merges | Доказывает наличие или отсутствие учебного перехода, само по себе не доказывает полезность. |
| Компрессия | Для каждого EXAM и TRAIN: фактические token counts до/после; суммы по profile и всей панели | Меньше tokens при точном Decode — полезно по этой оси. Не является точностью программы, перевода или KNN. |
| Сохранение текста | Actual IDs, Decode, exact ordinal equality исходной строки; no-update readouts | Потерю текста нельзя скрыть под выигрышем компрессии. |
| Выделенные байты Train | ΔGC.GetAllocatedBytesForCurrentThread вокруг синхронного Train | Это allocation, не живая RAM. |
| CPU и wall time Train | Process.Refresh + TotalProcessorTime до/после; Stopwatch | Одна маленькая A→B пара не отделяет общий шум/JIT/порядок процессов. Это наблюдение, не устойчивый speedup. |
| GC | ΔGC.CollectionCount для Gen0, Gen1 и Gen2 | Все три счётчика показываются, включая ухудшения. |

Для численных before/after показателей показать ΔA=A1−A0, ΔB=B1−B0 и ΔB−ΔA, с направлением «меньше/больше лучше». Для стоимости законченного Train показать абсолютные A/B и B−A; выдуманного «до-стоимости обучения» нет. Не сводить разные оси в произвольный суммарный score.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и этому профилю/домену; рост точности не обязателен.** Рядом обязательно показываются остальные изменения, в том числе comment и общий EXAM regressions. Никаких требований «все показатели должны улучшиться». Например, measured lambda-compression gain сохраняется как кандидат даже при худшей суммарной компрессии; карточка не превращает это в общий выигрыш для всего домена. Если ни одна полезная ось не улучшилась — `NO_MEASURED_GAIN`, без подбора нового corpus или дополнительного запуска.

## 8. Фиксированная площадка, сборка и конфликты

Источник только для чтения: `C:/Projects/llm_mvp_core`, объект commit `883496bca18113bf37e0afe683c75b05a0075048`. Единственный будущий пакет опыта: **`C:/xg/br01`**. Host: **`C:/xg/br01/Runner.csproj`**. Эти короткие пути заранее выбраны, чтобы не воспроизводить известный Windows path-limit failure. Ничего не создавать в старых attempt folders и не удалять их.

| Файл будущего пакета | Точное назначение |
|---|---|
| `A/BpeTokenizer.cs` | Неизменённый файл указанного commit; сохранить copyright/license header. |
| `B/BpeTokenizer.cs` | Та же копия с единственной заменой CountPairs из §4. |
| `common/RegexSplitter.cs` | Неизменённый source указанного commit. |
| `Program.cs` | Native Train/Encode/Save/Load, finite CLI, контроль времени, raw measurements; другого BPE нет. |
| `Runner.csproj` | .NET9 console, AssemblyName `BpeRealizedRunner`, ImplicitUsings/Nullable enabled; Disable default Compile items. Include Program.cs, common/RegexSplitter.cs и только `$(Variant)/BpeTokenizer.cs`. NuGet/ProjectReference на продукт не требуются. |
| `in/fixture.json` | Literal fixture §5. |
| `source-copy-manifest.json` | Repo/commit и Git blob IDs, SHA256 originals и A/B копий, точный diff, fixture hash, .NET version. |
| `r/0001/` | Append-only intent/status, A.json, B.json, stdout/stderr, initial/final canonical states, comparison.json, report.md, candidate.md при полезном выигрыше. |

Материализация после назначения этого опыта извлекает только два файла через `git show <fixed SHA>:<fixed path>` из существующего checkout. Содержимое писать как UTF-8 без BOM, без PowerShell default-encoding преобразования; например, читать stdout в .NET Process и записывать `new UTF8Encoding(false)`. Не использовать текущий HEAD вместо указанного объекта. Отсутствие объекта, .NET9 или исходного checkout — `PREPARATION_FAILED` с конкретной ошибкой, без смены ветки, скачивания SDK или поиска иной модели/проекта.

Экспериментальная оболочка может использовать `throw`, `try/catch`, `void`, простые DTO и собственный JSON-журнал. Не переносить на неё продуктовый запрет исключений или полный code-slice workflow. Исключение сохраняется с этапом и ненулевым exit code, не превращается в нулевую точность. Это послабление не разрешает менять данные, фактор или бюджет.

`ImportDirectoryBuildProps=false` и `ImportDirectoryBuildTargets=false` передаются обеими build-командами как global MSBuild properties, чтобы действовать до SDK imports; одно объявление внутри тела implicit-SDK csproj недостаточно. До SDK imports задать `BaseIntermediateOutputPath` и `MSBuildProjectExtensionsPath` как `C:/xg/br01/obj/$(Variant)/$(MSBuildProjectName)/`. OutputPath — `C:/xg/br01/bin/$(Variant)/Release/net9.0/`, `AppendTargetFrameworkToOutputPath=false`, `AppendRuntimeIdentifierToOutputPath=false`. Только Variant A или B, Release, пустой RID. Это фиксирует соответствие команд путям и раздельные generated files.

`run-pair --peer ... --fixture ... --attempt 0001` эксклюзивно создаёт intent в новом `r/0001`, один раз запускает A и один раз B через `run-arm --arm A|B --fixture ... --output ...`; Arm A не включает B через runtime toggle. Существующий пакет `C:/xg/br01` допустим только как та же уже подготовленная source-copy с совпадающим manifest; иной пакет — `PREPARATION_CONFLICT`. Существующий `r/0001` **всегда** означает `ATTEMPT_EXISTS_NO_REPLAY`, даже при совпадающем manifest и отсутствии completed записи. Проверить это до обеих build-команд; controller повторяет тот же guard перед эксклюзивным созданием attempt. Записать состояние в ответе, не перезаписывать чужие файлы, не выбирать `0002`, не удалять каталоги. В пределах отдельного поручения материализации допускается одна сборка каждого варианта. Сбой этой сборки завершает попытку подготовки; не запускать full solution, не чинить продуктовые тесты и не создавать цикл ретраев.

Точные команды **будущего**, ещё не реализованного host:

```powershell
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath C:/xg/br01/r/0001) { throw 'ATTEMPT_EXISTS_NO_REPLAY: no build; no training' }
dotnet build C:/xg/br01/Runner.csproj -c Release -p:Variant=A -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/br01/Runner.csproj -c Release -p:Variant=B -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/br01/bin/A/Release/net9.0/BpeRealizedRunner.dll run-pair --peer C:/xg/br01/bin/B/Release/net9.0/BpeRealizedRunner.dll --fixture C:/xg/br01/in/fixture.json --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

После единственной попытки разрешено прочесть raw для одного отчёта; это не повод повторять runtime. Build commands ограничить60 секундами каждую; при превышении остановить собственный процесс и зафиксировать ошибку. Никакого commit/push, CI, product promotion или правки защищённых документов эта карточка не назначает.

## 9. Исполнитель, отчёт и правило немедленного перехода

Исполнитель и последовательный проверяющий используют **модель текущей сессии по умолчанию**, без поиска другой модели и без самостоятельной смены режима. Обучаемая модель отдельно фиксирована: native BpeTokenizer с byte-vocabulary initialization, не LLM. Применить навыки из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md` и `human-communication-GLOSSARY.md`. Уже прочитанные неизменившиеся файлы не перечитывать ради формальности. Требования навыков к полной продуктовой цепочке не расширяют документационное поручение или ограниченный эксперимент.

Отчёт **на русском языке**, Markdown, `C:/xg/br01/r/0001/report.md`: до350 слов пояснений плюс таблица всех raw метрик/профилей и ссылки на JSON. Разделы: «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Числа из §5 пометить как прогноз и не копировать в measured columns. Отсутствующее измерение — «не измерено», не0. При любом полезном measured improvement добавить рядом `candidate.md` до120 слов: ось/профиль выигрыша, все регрессии, исходный метод CountPairs, точное изменение, ограничение vocab257 и решение Любы о дальнейшем переносе. Сейчас candidate success не создаётся: результатов ещё нет.

**STOP/NEXT — обязательный запрет на продолжение прежней задачи.** При прямом «стоп/закончить немедленно» сразу прекратить действия: не делать новый tool call, запись памяти, checksum, исправление отчёта или commit. Единственное допустимое завершение — остановить уже работающий собственный процесс, если иначе он продолжит обучение. Затем кратко сообщить только известное состояние. При «следующий эксперимент» немедленно прекратить прежний опыт и перейти к уже назначенному следующему ID/единому реестру; запрещено дописывать прежний report, candidate, review, исправлять CI, проверять hashes или перезапускать его. Если следующего назначения нет — сообщить известный статус, не возобновлять прежний опыт. При обычном завершении допустим один readback фактических ошибок и ссылок, затем карточка закончена. Существующие данные не удалять. Повторно запрашивать уже предоставленные GitHub permissions не требуется.
