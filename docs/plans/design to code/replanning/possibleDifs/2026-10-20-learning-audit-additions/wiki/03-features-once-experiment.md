# WIKI-FEATURES-ONCE-01 — стоимость повторного построения признаков во время обучения

**Статус: SOURCE_CONFIRMED / NEW_CANDIDATE / DESIGNED_NOT_RUN.** Дизайн и буквальные данные подготовлены; новый host ещё не реализован, не скомпилирован и не запускался. Сейчас только документируем. Карточка не разрешает изменить production или защищённый `docs/plans/replanning-possible-diffs`.

## 1. Проверяемая гипотеза и место в настоящем Train

При неизменных строках повторное построение лексических признаков в каждой эпохе может расходовать лишние выделенные байты и время. Проверяется один фактор: вычислить `Featurize` каждой строки один раз на вызов `Train`, использовать результат в тех же 18 эпохах. Полезный результат может быть уменьшением allocations/CPU/time при той же точности. Дополнительная живая память cache во время эпох — возможная цена, которую нельзя скрывать.

Core pin: `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. [TrainingRunner L43–53][runner] вызывает native `Train`, затем native `RouteForEvaluation`. В [Train L58–83][train] признаки прямо входят в `PredictIndex`, затем в запись `_tokenWeights[token][expected/predicted]` и `_biases`. [Featurize L341–378][features] детерминирован и не читает обучаемые параметры. Это измерение процесса настоящего обучения, не внешнего отчётного валидатора.

Native модель — лексический perceptron с 15 классами. Тип входа называется DPO pair, но данный метод учится только по `Prompt/ExpectedRouteKind`. Нейронный DPO, LoRA, число neural layers и VRAM здесь `N/A`. Участие этого learner в конкретном неудачном RP01 не установлено; этот опыт не объявляет такую причинность.

## 2. Зафиксированный A/B фактор

**A:** source-copy текущего native learner; `Featurize(row.Prompt)` остаётся внутри каждой эпохи.

**B:** копия A, отличающаяся только локальным cache признаков в `Train`. После создания `model` и перед циклом эпох сформировать `prepared = rows.Select(row => (Row: row, Features: Featurize(row.Prompt))).ToArray()`. Внутри epoch заменить `foreach (var row in rows)` на `foreach (var item in prepared)`, восстановить `var row = item.Row`, а строку вычисления features заменить на `var features = item.Features`. Сохранить порядок перечисления prepared, словарей признаков и все остальные операции.

`LabelIndex(row.ExpectedRouteKind)` остаётся внутри эпохи. `PredictIndex/Score` никогда не кешируются: они зависят от новых весов. `EpochCount=18`, `LearningRate=0.14`, Labels15, biases, `if (predicted == expected) continue`, размер обновления, confidence/margin/defer reader остаются прежними. Не добавлять averaging, shuffle, cache между разными Train вызовами, margin training либо WIKI-TAXONOMY.

Cache — только локальная переменная Train; не добавлять его в snapshot или поля модели. Существующие presentation/update counters одинаковы в A/B. По source при восьми строках ожидается 144 построения TRAIN features в A и 8 в B; это механизм, не уже измеренный результат и не полезная метрика успеха сам по себе.

Если после опыта полезный показатель улучшился, кандидат в original fix — только перенос `Featurize` в `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:58–83` по этой схеме. Сначала отчёт и решение пользователя; полная production-цепочка здесь не запускается.

## 3. Домен и буквальный учебный материал

Использовать без генерации и редактирования [fixtures/WIKI-FEATURES-ONCE-01.json](fixtures/WIKI-FEATURES-ONCE-01.json): **8 TRAIN и 4 EXAM**. `origin=simulation`: это авторские правдоподобные runtime wiki prompts, а не добытый production корпус. TRAIN достаточно длинный, чтобы в измерение вошло реальное построение нескольких десятков признаков; увеличение batch после просмотра результата запрещено.

Четыре строки явно просят обучить code trace, четыре — ответить на вопрос по wiki. Ожидаемые labels уже есть в текущем [Labels L25–42][labels]: `CodeTraceTraining` (index5) и `WikiQuestionAnswer` (index13). Targets native [L211, L219][targets]: `code-trace-builder-unit` и `project-local-wiki-orchestrator`. Missing taxonomy classes не используются. Два EXAM на каждый intent — отдельные замороженные формулировки; это small side panel, не оценка широкой репрезентативности.

TRAIN действительно проходит native запись весов; нельзя заменить обучение чтением готового snapshot. EXAM ни прямо, ни через expected labels не подаётся в `Train`. Reader получает только prompt; gold/target хранятся в host. Нельзя вместо router обучения запускать нижележащий CodeTraceBuilder: task здесь — научиться выбирать его по запросу.

Применимость: данный lexical wiki learner и другие точно установленные callers того же Train. Для BPE, AST-kNN, морфологического tokenizer или изменяющихся online-признаков пригодность cache не следует из этого опыта.

## 4. Бюджет и измерения до/после

| Величина | Один arm | Всего A+B |
|---|---:|---:|
| Fresh `Train([])` для нулевого состояния | 1 | 2 |
| Непустой native `Train(8 rows)` | 1 | 2 |
| Raw Train calls с учётом пустого | 2 | 4 |
| Эпох | 18 | 18 на каждый независимый arm |
| Row presentations | 144 | 288 |
| Native EXAM reader до/после | 4+4 | 16 |
| LLM/Teacher calls, neural backward, GPU calls | 0 | 0 |

Оба arm начинают с нулевых весов и biases. До и после одной непустой Train сохранить snapshot и ответы на один и тот же замороженный EXAM. EXAM не меняет state hash. Actual update count и nonzero parameters фиксируются, не подгоняются под ожидаемое значение. Если state не изменился, честно указать это, не засчитывать построенные fixtures как обучение.

Primary useful metric: `nonemptyTrainAllocatedBytesCurrentThread` (меньше лучше). Дополнительно `nonemptyTrainWallMilliseconds`, CPU milliseconds, whole-process peak working set, managed heap before/after, persisted snapshot bytes. Показать все, включая ухудшения. Таймер и allocations окружают только **непустую Train**, включая создание cache B; построение DTO, EXAM, snapshot serialization, build и запись отчёта вне этого окна. Нельзя исключить подготовку cache из измерения B.

Для качества: `correct_selected` требует native `Selected` и совпадение route+target; `wrong_selected`, `deferred` показывать отдельно. Confidence/margin — диагностика, не самостоятельная полезная метрика. Уменьшение defer полезно только вместе с числом правильных и неправильных выбранных решений. Обязательны `A до / A после / B до / B после`, `ΔA`, `ΔB`, `ΔB−ΔA`. Cost сравнивается A vs B внутри Train, не с пустой инициализацией.

Поскольку B должен сохранять вычисляемую модель, сравнить canonical содержимое весов/biases/labels и native EXAM outcomes A после vs B после. При различии не объявлять успех эквивалентной оптимизации: записать `SEMANTIC_DRIFT`, сохранить всё наблюдённое и закончить этот attempt. Не добавлять второй алгоритмический фактор ради совпадения.

Один короткий process per arm не доказывает стабильную пропускную способность. JIT/startup и resolution CPU timer могут доминировать; report сохраняет raw ticks/frequency и ограничения. Whole-process peak включает host. Cache держит все feature dictionaries живыми в Train; heap-after может этого не показать, поэтому отсутствие его роста не доказывает нулевую цену live RAM.

**Критерий пользователя:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и wiki-domain; рост accuracy не обязателен. Честно назвать уровень доказательства: наблюдение одного micro-run, а не подтверждённый performance SLO. Все регрессии рядом. При отсутствии улучшений — `NO_GAIN`; при недостаточном разрешении измерения — `INCONCLUSIVE`, без автоматического расширения опыта.

## 5. Где готовить и как запускать после поручения на опыт

Подготовка строго по [04-source-copy-work-order.md](04-source-copy-work-order.md), sections 2–5. Исходный scaffold выбран там; B строится из BaselineModel, не из CandidateModel другого опыта. Host: `C:/xg/wc1`; literal ID: `WIKI-FEATURES-ONCE-01`; fixture counts `8/4`. Assembly: `WikiProbe`. Результат: `C:/xg/wc1-r/attempt-001`.

Команды ниже выполняются **только после будущей подготовки host и поручения выполнить этот опыт**. В текущей сессии они не выполнялись; готового executable не заявлено.

```powershell
$ErrorActionPreference = 'Stop'
$wikiAttempt = 'C:/xg/wc1-r/attempt-001'
if (Test-Path -LiteralPath $wikiAttempt) { throw 'ATTEMPT_EXISTS; preserve it; no retry' }
[System.IO.Directory]::CreateDirectory($wikiAttempt) | Out-Null
$wikiIntent = [System.IO.File]::Open("$wikiAttempt/intent.json", [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$wikiWriter = [System.IO.StreamWriter]::new($wikiIntent, [System.Text.UTF8Encoding]::new($false))
try { $wikiWriter.Write('{"experimentId":"WIKI-FEATURES-ONCE-01","attempt":1,"noRetry":true}') } finally { $wikiWriter.Dispose() }
dotnet --list-sdks 1> "$wikiAttempt/sdk.stdout" 2> "$wikiAttempt/sdk.stderr"
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed; no retry' }
dotnet restore C:/xg/wc1/WikiProbe.csproj --configfile C:/xg/wc1/NuGet.Config 1> "$wikiAttempt/restore.stdout" 2> "$wikiAttempt/restore.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; training NOT_RUN; no retry' }
dotnet build C:/xg/wc1/WikiProbe.csproj -c Release --no-restore -o C:/xg/wc1/bin 1> "$wikiAttempt/build.stdout" 2> "$wikiAttempt/build.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; training NOT_RUN; no retry' }
'{"phase":"A","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/A-issued.json" -Encoding utf8
dotnet C:/xg/wc1/bin/WikiProbe.dll --arm A --output C:/xg/wc1-r/attempt-001/A 1> "$wikiAttempt/A.stdout" 2> "$wikiAttempt/A.stderr"
$wikiExitA = $LASTEXITCODE
@{ phase='A'; state='returned'; exitCode=$wikiExitA } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/A-returned.json" -Encoding utf8
if ($wikiExitA -ne 0) { throw 'Arm A failed; no B and no retry' }
'{"phase":"B","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/B-issued.json" -Encoding utf8
dotnet C:/xg/wc1/bin/WikiProbe.dll --arm B --output C:/xg/wc1-r/attempt-001/B 1> "$wikiAttempt/B.stdout" 2> "$wikiAttempt/B.stderr"
$wikiExitB = $LASTEXITCODE
@{ phase='B'; state='returned'; exitCode=$wikiExitB } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/B-returned.json" -Encoding utf8
if ($wikiExitB -ne 0) { throw 'Arm B failed; no retry' }
```

Исполнять команды последовательно, проверять exit status; после failure не переходить к следующей команде. Нужен установленный .NET9 SDK/reference packs. Если его нет — `BLOCKED_ENVIRONMENT`. Собственный подготовленный host ожидаемо существует; неизвестный/чужой host до подготовки или любой существующий attempt/output — конфликт по work order, никакого удаления. Никакого full solution build, production тестов, download другой модели или правки protected.

Выход: в каждом arm `fixture.json`, `source-provenance.json`, `before-snapshot.json`, `after-snapshot.json`, `report.json`; общий **`C:/xg/wc1-r/attempt-001/report.ru.md`** по шаблону ниже. Сохранённые ошибки/логи не затирать. Успех build не заменяет execution evidence.

## 6. Русский отчёт — фиксированная структура

1. `Статус и область`: фактический run/not-run, wiki lexical router; source pin, fixture hash, default executor model, counts из runtime.
2. `Полезное изменение`: allocation/CPU/time A→B и цена RAM; рядом все положительные, нулевые и отрицательные изменения, без требования accuracy↑.
3. Таблица: `показатель | A до | A после | B до | B после | ΔA | ΔB | ΔB−ΔA`. Для costs отдельная таблица `Train A | Train B | B−A`, поскольку пустая модель не является cost baseline.
4. `Действительно обучалось`: native writer/reader, changed params/update count, post-state equality, отдельные correct/wrong/defer outcomes.
5. `Пределы`: 8/4 simulated rows, один process, JIT, cache live RAM, no production/RP01 attribution.
6. `Кандидат`: точное original Train и только перенос Featurize, если есть полезное измерение. Иначе `NO_GAIN`/`INCONCLUSIVE` с сохранением receipt. Решение о production оставлено пользователю.

## 7. Готовый промпт исполнителю

> Текущий ID — WIKI-FEATURES-ONCE-01. До отдельного поручения исполнить опыт — только документация, NOT_RUN. Когда поручено исполнение: используй модель по умолчанию своей текущей сессии, отчёт по-русски; навыки core docs/skills architect-behavior-classifier, planning-session-startup, plan-review, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit из work order. Подготовь только C:/xg/wc1 по 04-source-copy-work-order.md: A — native source-copy ab583032, B — такой же с единственным локальным cache Featurize до 18 эпох. LabelIndex, Score, margin/taxonomy, обновления и reader не меняй. Возьми frozen fixture 8 TRAIN/4 EXAM без редактирования. Выполни ровно A/B по командам этой карточки, результат C:/xg/wc1-r/attempt-001; четыре raw Train calls, только два непустых. Измеряй настоящую Train вместе с созданием cache; сохрани до/после и все показатели. Любое измеренное улучшение полезной метрики — успех по ней и этому домену, accuracy↑ не требуется; ухудшения показывай рядом. Число feature calls — только диагностика. Если нет среды/runner/build — честно BLOCKED/NOT_RUN, не чинить production. Старые опыты/attempts не удалять, protected не менять, production не продвигать. На «следующий эксперимент» немедленно переходи к указанному следующему без доработки старого отчёта/commit/hash/build; на «стоп/закончить немедленно» прекрати действия, без последнего tool call; исключение — только необходимое прекращение своего уже работающего процесса этого опыта. Нельзя продолжать предыдущую задачу под предлогом завершения. После одного отчёта пользователь решает, какие исправления вносить.

[runner]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53
[train]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L83
[features]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L341-L378
[labels]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L25-L42
[targets]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L211-L219
