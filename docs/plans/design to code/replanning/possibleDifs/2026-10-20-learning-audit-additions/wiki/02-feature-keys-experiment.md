# WIKI-FEATURE-KEYS-01 — раздельные unigram/bigram признаки wiki learner

**SOURCE_CONFIRMED_COLLISION / CONDITIONAL_LEARNING_HYPOTHESIS / DESIGNED_NOT_RUN.** Подтверждено исходное смешивание разных признаков; потеря полезного качества не измерена. Буквальный fixture и задание подготовки есть, новый host не реализован и не собран. Сейчас только документируем. Production и защищённая папка `docs/plans/replanning-possible-diffs` не меняются.

## 1. Учебная гипотеза и её границы

Core pin `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. [RegexLikeWords L359–378][features] оставляет `_` внутри токена; [Featurize L341–356][features] записывает unigram под самим токеном и bigram под `left + _ + right`. Поэтому identifier `load_customer` и соседние слова `load customer` обновляют одну координату; bigrams `[a_b,c]` и `[a,b_c]` тоже имеют общий ключ `a_b_c`.

Полная source-derived коллизия: `trace trace_trace` и `trace_trace trace` имеют одно отображение `{trace:1/√3, trace_trace:1/√3, trace_trace_trace:1/√3}`. Это иллюстрация из кода, не выполненное обучение. **Этим искусственным строкам не назначать разные route labels:** два различных настоящих wiki-намерения для них не установлены. Не запускать на них отдельный toy classifier; это подменило бы задачу придуманной семантикой.

Основная гипотеза: раздельный codec может снизить перенос обновлений между различными wiki-намерениями, если identifier и естественная фраза несут полезный различный сигнал. Остальные слова могут разрешить задачу уже в A, поэтому честный `NO_GAIN` допустим. Отсутствие collision после изменения само по себе не доказывает полезного качества.

Реальная цепочка: [TrainingRunner L43–53][runner] → [Train L58–83][train] → `Featurize(Prompt)` → `PredictIndex/Score` → запись `_tokenWeights/_biases` → [RouteForEvaluation L95–99, L155–220][reader] → та же Featurize → route/target/status. В experiment действительно обновляются веса, а EXAM проверяет их выход. Это не внешний validator, не участвующий в обучении.

Модель — native lexical perceptron, Labels15,18epochs,LR0.14,CPU. DTO называется DPO pair, но этот `Train` использует только Prompt/ExpectedRouteKind, без chosen/rejected likelihood. Neural DPO/LoRA/layers/VRAM/Teacher — N/A; участие этого learner в конкретном failed RP01 не установлено.

## 2. Один фактор A/B

**A:** исходный native source-copy. **B:** та же копия A, меняется только кодирование feature keys в `Featurize`, одновременно на стороне Train и reader:

```csharp
// Спецификация ключей для будущей лабораторной копии, не новый runner.
static string TokenLength(string token)
    => token.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
static string UniKey(string token)
    => "u:" + TokenLength(token) + ":" + token;
static string BiKey(string left, string right)
    => "b:" + TokenLength(left) + ":" + left + ":" + TokenLength(right) + ":" + right;
```

В unigram loop создать `key=UniKey(token)` и использовать его в обоих обращениях к dictionary; в bigram loop заменить только создание key на `BiKey(tokens[index], tokens[index+1])`. Пример: `u:13:load_customer` и `b:4:load:8:customer`. Частоты, dictionary comparer, порядок циклов, lowercasing, допуск `_`/`-`, L2-normalizer и вся арифметика остаются прежними. Tokenization не меняется.

Не совмещать с FEATURES-ONCE cache, новым margin update, averaging, label taxonomy, shuffle, другим reader или threshold. B создаётся только из A, не из чужого CandidateModel. Обе модели свежие; старый snapshot не загружается новым codec. Если появится полезный gain, original fix-кандидат — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:341–356`. Versioning/migration накопленных весов — отдельное условие до production, не второй фактор этого опыта. Изменение codec не объявляет совместимость со старым snapshot version.

Текущие [Labels L25–42][labels] содержат CodeTraceTraining (index5) и WikiQuestionAnswer (index13), оба не первый класс. Missing WIKI-TAXONOMY classes не используются. Условие `predicted == expected` из WIKI-MARGIN сохраняется. KN-ROUTER-LEARNED-KEY-CANONICALIZATION относится к другому learner и разным C#/.NET normalize на writer/reader; здесь оба конца уже используют одну функцию, проблема внутри feature namespaces. [Дедуп/protected gap](01-findings.md) выполнен отдельно.

## 3. Буквальный доменный fixture, labels не подгоняются

Использовать [fixtures/WIKI-FEATURE-KEYS-01.json](fixtures/WIKI-FEATURE-KEYS-01.json) без генерации и изменения: **4 TRAIN / 6 EXAM**, `origin=simulation`. Это авторские осмысленные runtime requests, не production logs.

| TRAIN | Точный prompt | Expected route / target |
|---|---|---|
| K-T1 | `Train the code trace builder on calls inside load_customer.` | CodeTraceTraining / code-trace-builder-unit |
| K-T2 | `Answer the project wiki question about how services load customer records.` | WikiQuestionAnswer / project-local-wiki-orchestrator |
| K-T3 | `Train the code trace builder on calls inside save_order.` | CodeTraceTraining / code-trace-builder-unit |
| K-T4 | `Answer the project wiki question about how services save order records.` | WikiQuestionAnswer / project-local-wiki-orchestrator |

Gold следует явному intent — обучить code trace либо ответить по wiki; identifier является аргументом, а не произвольно придуманной label rule. Native targets подтверждены [L211/L219][targets]. Опыт учит выбирать route, не запускает downstream CodeTraceBuilder training. Содержательные возражения к gold разрешаются до исполнения новой версией карточки, не после A/B.

EXAM K-Q1…K-Q4 — новые буквальные формулировки этих двух намерений с `load_customer`/`load customer` и `save_order`/`save order`; K-Q5/K-Q6 — plain controls без identifiers. Все шесть полных prompts находятся в JSON. Не переводить их на русский: это изменит фактор. Русским должен быть отчёт. Reader получает только prompt, без ID/group/gold. EXAM не входит в Train. Это small panel на знакомых identifiers, не unseen-repository generalization.

Применимость: lexical wiki/code intents с пересечением underscore vocabulary и соседних естественных слов. Для corpus без таких пересечений возможен нейтральный эффект. E25 tuple POS bigrams и AST-kNN сюда не относятся; результат на них не переносить.

## 4. Бюджет и измерения

| Действие | На arm | Всего A+B |
|---|---:|---:|
| Fresh `Train([])` для нулевого состояния | 1 | 2 |
| Nonempty native `Train(4 rows)` | 1 | 2 |
| Raw Train calls с учётом пустых | 2 | 4 |
| 18 эпох ×4 строки | 72 presentations | 144 |
| EXAM before+after | 6+6 | 24 reader calls |
| Teacher/LLM/neural backward/GPU | 0 | 0 |

Пустая Train исключена из nonempty Train cost. Перед и после непустой Train сохранить snapshot и EXAM decisions. Native reader не должен менять state. Actual updates/nonzero parameters нужны для подтверждения записи весов; не являются useful gain сами по себе.

Полезные показатели: `correct_selected/6` (Selected + правильные route **и** target), `wrong_selected/6`, `deferred/6`; отдельно группы 4 identifier-vs-words и 2 plain controls. Не выдавать defer за правильный отрицательный route по одному совпадению enum. Также Train wall/CPU/allocated bytes, heap-before/after, whole-process peak и persisted snapshot bytes. Длинные ключи могут ухудшить стоимость — показать рядом с выигрышем качества.

Обязательная таблица `A до / A после / B до / B после / ΔA / ΔB / ΔB−ΔA`. Gain B относительно нулевого состояния может быть обычным эффектом обучения, доступным A; причинный эффект codec оценивается **между** arm. Cost A/B измеряется только вокруг непустой Train, включая создание ключей. EXAM, DTO creation, serialization, build и file I/O вне cost-window. Разные keys закономерно меняют snapshot hash A/B: post-state equality здесь не требуется; отсутствие mutation во время EXAM требуется.

Confidence/margin, unique feature keys и disappearance of collision — диагностика. **Любое фактически измеренное улучшение полезной метрики — успех по этой метрике и домену; accuracy↑ не требуется. Все регрессии рядом.** Нет полезного улучшения — `NO_GAIN`; недостаточно разрешения измерения — `INCONCLUSIVE`. Не менять gold/EXAM после результата и не запускать дополнительные batches ради positive verdict. Один короткий process per arm содержит JIT/noise; process peak не равен пику Train. Область вывода остаётся малой.

## 5. Подготовка и точные будущие команды

По [04-source-copy-work-order.md §§2–5](04-source-copy-work-order.md) подготовить **`C:/xg/wk1`**, assembly `WikiProbe`, literal ID `WIKI-FEATURE-KEYS-01`, fixture counts4/6. B — из pinned BaselineModel, не margin/taxonomy B. Результат **`C:/xg/wk1-r/attempt-001`**. Готовый executable здесь не заявлен: команды — после будущего поручения подготовить и исполнить этот опыт.

```powershell
$ErrorActionPreference = 'Stop'
$wikiAttempt = 'C:/xg/wk1-r/attempt-001'
if (Test-Path -LiteralPath $wikiAttempt) { throw 'ATTEMPT_EXISTS; preserve it; no retry' }
[System.IO.Directory]::CreateDirectory($wikiAttempt) | Out-Null
$wikiIntent = [System.IO.File]::Open("$wikiAttempt/intent.json", [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$wikiWriter = [System.IO.StreamWriter]::new($wikiIntent, [System.Text.UTF8Encoding]::new($false))
try { $wikiWriter.Write('{"experimentId":"WIKI-FEATURE-KEYS-01","attempt":1,"noRetry":true}') } finally { $wikiWriter.Dispose() }
dotnet --list-sdks 1> "$wikiAttempt/sdk.stdout" 2> "$wikiAttempt/sdk.stderr"
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed; no retry' }
dotnet restore C:/xg/wk1/WikiProbe.csproj --configfile C:/xg/wk1/NuGet.Config 1> "$wikiAttempt/restore.stdout" 2> "$wikiAttempt/restore.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; training NOT_RUN; no retry' }
dotnet build C:/xg/wk1/WikiProbe.csproj -c Release --no-restore -o C:/xg/wk1/bin 1> "$wikiAttempt/build.stdout" 2> "$wikiAttempt/build.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; training NOT_RUN; no retry' }
'{"phase":"A","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/A-issued.json" -Encoding utf8
dotnet C:/xg/wk1/bin/WikiProbe.dll --arm A --output C:/xg/wk1-r/attempt-001/A 1> "$wikiAttempt/A.stdout" 2> "$wikiAttempt/A.stderr"
$wikiExitA = $LASTEXITCODE
@{ phase='A'; state='returned'; exitCode=$wikiExitA } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/A-returned.json" -Encoding utf8
if ($wikiExitA -ne 0) { throw 'Arm A failed; no B and no retry' }
'{"phase":"B","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/B-issued.json" -Encoding utf8
dotnet C:/xg/wk1/bin/WikiProbe.dll --arm B --output C:/xg/wk1-r/attempt-001/B 1> "$wikiAttempt/B.stdout" 2> "$wikiAttempt/B.stderr"
$wikiExitB = $LASTEXITCODE
@{ phase='B'; state='returned'; exitCode=$wikiExitB } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/B-returned.json" -Encoding utf8
if ($wikiExitB -ne 0) { throw 'Arm B failed; no retry' }
```

Команды последовательно; после failure не выполнять следующую. .NET9 SDK/reference packs missing → `BLOCKED_ENVIRONMENT`. Собственный подготовленный host ожидаемо существует; неизвестный/чужой host до подготовки или любой существующий attempt/output → `BLOCKED_PATH_EXISTS`, ничего не удалять. Новый attempt только после разрешения повторить, следующий номер. Не чинить Windows settings, production tests, full solution или соседние опыты. В каждом arm сохраняются fixture/provenance, before/after snapshot, report.json. Общий русский отчёт — **`C:/xg/wk1-r/attempt-001/report.ru.md`**. После crash старые build.stdout/status.json остаются; отсутствие начавшегося обучения не называется нулевой точностью модели.

## 6. Фиксированный русский отчёт

1. Статус, source pin, simulated domain, fixture hash, actual counts, модель исполнителя по умолчанию.
2. Таблица correct/wrong/defer с A/B before/after и дельтами; per-row outcomes всех шести EXAM, включая неудобные случаи.
3. Затраты непустой Train A/B и snapshot bytes; все ухудшения рядом; не измеренные значения не заполнять предположениями.
4. Evidence actual weights update и native consumer; ограничения маленькой панели/JIT/gates.
5. Verdict по каждой useful metric. Source collision отдельно от learning gain. При успехе — candidate изменить original Featurize; compatibility/versioning не решены. Production решение за пользователем.

## 7. Готовый промпт исполнителю

> ID WIKI-FEATURE-KEYS-01; сейчас документация/NOT_RUN до отдельного поручения исполнить. При исполнении используй модель по умолчанию текущей сессии, отчёт по-русски. Навыки core docs/skills architect-behavior-classifier, planning-session-startup, plan-review, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit — точные пути в work order. Подготовь только C:/xg/wk1 из pinned native BaselineModel по 04-source-copy-work-order.md. A native; B меняет только unigram/bigram codec §2 в Train и reader. Labels15,18epochs,LR0.14,margin/defer,порядок данных прежние. Не добавляй feature cache и чужой CandidateModel. Frozen fixture4TRAIN/6EXAM — явные CodeTraceTraining/WikiQuestionAnswer requests, не разные искусственные labels для trace trace_trace. Ровно A/B по командам карточки, output C:/xg/wk1-r/attempt-001; четыре raw Train calls, два непустых. Измеряй actual training+native EXAM до/после, обе дельты и их разницу. Любое measured improvement useful metric — успех по ней и домену, accuracy↑ не требуется; все regressions рядом. Collision removal само по себе не quality gain; NO_GAIN допустим. Gold/fixture не менять ради результата. Если нет среды/host/build — BLOCKED/NOT_RUN, не чинить production. Protected не менять, старые experiments/results/failed attempts не удалять, production не продвигать. На «следующий эксперимент» немедленно прекращай старую задачу, без ещё одного commit/hash/build/ремонта отчёта. На «стоп/закончить немедленно» прекрати действия без ещё одного tool call; исключение — только необходимое прекращение своего уже работающего процесса этого опыта. Запрещено продолжать прежнюю задачу под видом завершения. Один отчёт, затем пользователь решает, какие исправления внедрять.

[features]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L341-L378
[runner]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53
[train]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L83
[reader]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L95-L220
[labels]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L25-L42
[targets]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L211-L219
