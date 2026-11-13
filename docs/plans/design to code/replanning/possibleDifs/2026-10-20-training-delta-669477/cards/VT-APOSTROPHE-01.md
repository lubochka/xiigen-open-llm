# VT-APOSTROPHE-01 — апостроф в слове теряется при записи обучаемого правила verifier

Дата: 20.10.2026. Core: `66947785d1318f7cbbc88118f3156cdc75001e16`. Статус: **SOURCE_CONFIRMED / EXPERIMENT_DESIGNED / NOT_MATERIALIZED / NOT_RUN**. Этот документ не является отчётом об исполненном эксперименте. Ни обучение, ни сборка host в этой сессии не выполнялись.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом обязательно показать изменения остальных показателей, включая ухудшения.** В этом опыте полезные оси — точность сохранения учебного термина и снижение ложного принятия; отклонение допустимых перефразировок — отдельная возможная регрессия.

**Немедленный STOP/NEXT:** после «стоп» / «закончить немедленно» прекратить действия, включая инструменты, отчёт, проверки, оформление и коммит; ответить кратко уже известным состоянием. После «следующий эксперимент» перестать дорабатывать прошлый опыт и перейти только к указанному следующему. «Нужно лишь закончить отчёт / тест / review / cleanup» не является исключением. Старые эксперименты, результаты и незавершённые попытки не удалять, не перезаписывать. Не повторять запуск ради более красивого результата и не менять панель после наблюдения ответа.

## 1. Что действительно связано с обучением

В переводческой единице есть конкретная потеря учебного сигнала: native builder сохраняет английское `brother's` в текстовом issue, но native `QualityVerifierUnit.TrainOnExample` разбирает issue через `Split('\'')` и заносит в память только `brother`. Обычный `Verify` потом читает именно эту память и принимает строку без притяжательного окончания. Это гипотеза о поведении **обучаемого verifier**, а не форматирование отчёта и не проверка parser отдельно от обучения.

| Звено | Закреплённый source |
|---|---|
| Реальный structured builder получает две TRAIN пары; из `[PHRASE: … => brother's]` извлекает requiredTerm | [`BuildGeneralBatch`, 94–104][builder]; [`ExtractRequiredTerm`, 258–266][required] |
| WordRegex допускает апостроф внутри target token | [`WordRegex`, 566–567][regex] |
| Builder создаёт chosen verifier issue `expected 'brother's' for translated quality marker` и `Pass=false` | [128–141][issue] |
| Trainer читает chosen issue и изменяет `_requiredTermsBySourceMarker` | [`TrainOnExample`, 1095–1145][train] |
| Деление на `'` записывает `parts[1]`, то есть `brother` | [`TryParseRequiredTerm`, 1249–1257][parser] |
| Source marker берётся из первого ADV; `нежно` и `горестно` находятся в native allowlist | [`ExtractSourceMarker`, 1265–1279][marker]; [allowlist, 89–99][allow] |
| Тот же обычный verifier читает обученные terms через `Contains`, без нормализации апострофов | [`Verify`, 1052–1089][verify] |
| Можно наблюдать реальную память без reflection и без её изменения | [`ExportCheckpoint`, 1175–1194][export] |

`TryParseIssue` перед fallback не распознаёт этот issue как concept-rule: хвост после первого фрагмента `'` равен `s`, среди [SemanticConcept][concepts] такого имени нет. Для plain `hello` хвост `for translated quality marker` также не содержит concept enum. В fallback `qualityMarkerIssue=true` пропускает signature-store, поэтому два явных ADV source marker необходимы для достижимой записи. Менять selector marker или искусственно записывать dictionary вместо native Train запрещено.

Домен — **Перевод**, направление RU→EN, узкая тема притяжательных форм в коротких бытовых инструкциях. Тип результата — quality-verifier unit learning. Не доказываются улучшение генератора перевода, эффект на RP01 neural DPO, обычный полный Flow, обобщение на другие языки или все формы английского. Внешний переводчик не генерирует кандидаты: фиксированные кандидаты являются штатным входом проверяемой единицы `Verify`.

## 2. Новизна и отличие от известных карточек

Проверены все 799 Markdown `possibleDifs` planning `53cbcabd3926cda714bab428d294eb666301e9e4` и 155 Markdown каталога protected cards (154 карточки и индекс) на закреплённом core. Поиск включал `apostroph`, `апостроф`, `brother.s`, `TryParseRequiredTerm`, `issue.Split`, `single.quote`, `кавыч`. [Квитанция поиска](../evidence/verifier-apostrophe-novelty.json).

- P63.3 / N30 — апостроф в **исходном** contraction `can't`, потеря отрицания при source-token normalization. Здесь повреждается **target term в writer обучаемого verifier**, до consumer.
- N28 / P56 — разбор ролей enum/concept; здесь literal delimiter в `TryParseRequiredTerm`, concept-ветвь остаётся исходной.
- N57 и `2026-10-20-learning-signal-and-cost-audit` — недостижимая запись при пустом source marker. Здесь обе TRAIN строки имеют явные native ADV marker и проходят существующий путь; selector не меняется.
- Прежние polarity/conflict/signature карточки не заменяют этот фактор. Их изменения не совмещать с этим A/B.

Поиск покрывает указанные документы и карточки; неизвестные неопубликованные работы не объявляются проверенными.

## 3. A/B: один фактор и адрес возможного исправления

**A:** неизменённый `TryParseRequiredTerm`.

**B:** в экспериментальной копии только этого метода перед прежним `Split` добавить точное распознавание рамки, которую выпускает native builder:

```csharp
const string prefix = "expected '";
const string suffix = "' for translated quality marker";
if (issue.Length >= prefix.Length + suffix.Length &&
    issue.StartsWith(prefix, StringComparison.Ordinal) &&
    issue.EndsWith(suffix, StringComparison.Ordinal))
{
    required = issue.Substring(prefix.Length,
        issue.Length - prefix.Length - suffix.Length);
    return !string.IsNullOrWhiteSpace(required);
}
```

Остальной исходный метод, включая `required = string.Empty;`, старый fallback `Split` и return, сохранить. Вставка следует **после** `required = string.Empty;`. Проверка длины не позволяет общей единственной кавычке malformed issue одновременно оказаться открывающим и закрывающим delimiter; такие строки остаются на прежнем fallback. На заданных native-generated TRAIN issues это не отдельный фактор. Ветки `TryParseIssue`, marker selection, OR нескольких terms, `Contains`, case comparison, BuildGeneralBatch, checkpoint, tokenizer, gold и кандидаты в обоих плечах одинаковы. Это не комплексное исправление verifier. Если понадобится перенос после решения Любы, точка изменения — `27.TranslationDomain/TranslationTrainedUnits.cs::QualityVerifierUnit.TryParseRequiredTerm`. Original source и product tests сейчас не менять.

## 4. Модель, corpus и фиксированный бюджет

Исполнитель использует ту же модель и режим, что уже выбраны в назначенной сессии; выбирать иной LLM, teacher или judge не нужно. Обучаемая модель — fresh native `QualityVerifierUnit`, без весов Transformer. Seed N/A: этот путь детерминированный. Внешние model calls0, neural updates0, DPO optimizer updates0, full Flow calls0, cold-load calls0, retries0.

Две TRAIN пары ниже передать native `ParseJsonl`, затем `BuildGeneralBatch("qva01-train", pairs, heldOut:false, signatureMode:Structural)`. Оба плеча читают побайтно один и тот же UTF-8 `train.jsonl` в указанном порядке. SourceIds одинаковы; native builder обязан создать 6 rows. Обучать **только две** rows с `UnitId==TranslationUnitIds.QualityVerifier`, в порядке T1,T2. Остальные четыре generated rows сохранить как evidence преобразования, не Train. Нельзя называть эти две Unit updates полным обучением TranslationMachine.

```jsonl
{"id":"qva01-possessive","source":"qva01-train","direction":"RU_EN","pair_family":"possessive-marker","prompt":"Translate Russian to English.","a_side":"Нежно положи книгу брата","chosen":"Gently put down your brother's book.","rejected":"Gently put down your brother book. [WRONG: possessive_omission - the possessive marker is missing.] [PHRASE: брата => brother's]","validation_outcome":"train_ready","validation_errors":[]}
{"id":"qva01-control","source":"qva01-train","direction":"RU_EN","pair_family":"plain-marker","prompt":"Translate Russian to English.","a_side":"Горестно скажи привет","chosen":"Say hello sadly.","rejected":"Say sadly. [WRONG: lexical_omission - hello is missing.] [PHRASE: привет => hello]","validation_outcome":"train_ready","validation_errors":[]}
```

Markers T1=`нежно`, T2=`горестно` независимы; новый опыт не смешивает два требования в одном OR bucket. Проверить это по токенам и exported checkpoint; если native source на этих exact fixtures не даёт указанных marker/issue, закончить `PRECONDITION_FAILED`, не искать другую фразу под желаемый исход.

EXAM — следующие восемь literal записей. Кандидат не генерировать заново. `expectedPass` хранить отдельно в `gold.json`, читать только после сохранения before/after outputs. Каждую строку один раз токенизировать штатным `TranslationSemanticTokenizer.Tokenize(source,"ru")`, сохранить токены, использовать ту же sequence до и после обучения. Gold не передавать builder или Unit.

| ID | Source | Candidate | expectedPass | Назначение |
|---|---|---|---|---|
| Q1 | Нежно положи книгу брата | Gently put down your brother's book. | true | Exact допустимый TRAIN target |
| Q2 | Нежно положи книгу брата | Gently put down your brother book. | false | Исходная ошибка притяжательной формы |
| Q3 | Нежно подними книгу брата | Gently pick up your brother's book. | true | Новый глагол, тот же нужный term |
| Q4 | Нежно подними книгу брата | Gently pick up your brother book. | false | Та же ошибка на другом source |
| Q5 | Горестно повтори привет | Repeat hello sadly. | true | Plain-term контроль |
| Q6 | Горестно повтори привет | Repeat sadly. | false | Plain-term omission контроль |
| Q7 | Нежно положи книгу брата | Gently put down the book of your brother. | true | Допустимая перефразировка без literal possessive token |
| Q8 | Нежно положи книгу брата | Gently put down your brother’s book. | true | Допустимый U+2019 апостроф |

Качество здесь означает соответствие восьми заранее заданным минимальным переводческим различиям. Q7/Q8 намеренно считаются допустимыми, даже если узкое learned literal правило их отвергает. Не менять gold на false для улучшения результата B. Q1/Q2 — recall учебных текстов; Q3/Q4 — ограниченный перенос на новый глагол; Q5/Q6 — plain control; Q7/Q8 — проверки ширины требования. Это не восемь независимых языковых семейств.

На плечо: 1 fresh Unit, 1 native BuildGeneralBatch на 2 исходные пары, 6 generated rows, 2 actual TrainOnExample calls, 8 Verify до и 8 после, 2 ExportCheckpoint. Суммарно A+B: **2 Unit instances, 4 Unit updates, 32 Verify calls, 4 исходные пары прочитаны двумя плечами; уникальных TRAIN pairs2, уникальных EXAM rows8**. Не добавлять warmup, отдельные scorer-прогоны native Unit или повторные seed. После двух Train память обязана отличаться от initial; отсутствие мутации делает эксперимент невалидным как измерение обучения.

## 5. Проект, пути и построение без поиска

Это blueprint, ещё не созданный host. Следующему исполнителю разрешено материализовать **только** опыт по этому blueprint, когда в задаче будет разрешён его запуск. Не выбирать иной experiment framework и не строить весь solution.

| Объект | Единственный путь |
|---|---|
| Core checkout | `C:/Projects/llm_mvp_core` |
| Копии native source A/B, runner исходники, fixtures | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/Qva01` |
| Изолированный host project и bin/obj | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01` |
| Результат единственной попытки | `C:/xexp/QVA01/a01` |
| Arm outputs | `C:/xexp/QVA01/a01/A` и `C:/xexp/QVA01/a01/B` |
| Русский отчёт, команда, failures, source manifest | `C:/xexp/QVA01/a01/report.md`, `commands.json`, `status.json`, `source-manifest.json` |
| Кандидат для исправления | `C:/xexp/QVA01/a01/candidate.md` |

Короткие пути выбраны заранее из-за исторического инцидента длинного пути, приведённого пользователем, и отдельно подтверждённого LFT scaffold failure; это не утверждение, что текущий BPE опыт всё ещё заблокирован. `docs/plans/replanning-possible-diffs` защищён пользователем: в него ничего не записывать. Не удалять старые output folders и не подбирать другой runId автоматически. Если `a01` существует, результат — `BLOCKED_EXISTING_ATTEMPT`, без обучения и изменения этого каталога. Перед материализацией проверить также source-root и host-root: любой уже существующий — `BLOCKED_EXISTING_HOST_OR_SOURCE`, без перезаписи и обучения; ранее оставленный собственный partial host тоже сохранить. Если путь checkout отсутствует — `BLOCKED_FIXED_CHECKOUT_MISSING`; не искать другой диск.

### 5.1. Source closure

В A/ и B/ скопировать полностью ровно семь файлов из `27.TranslationDomain`, сохранив license header; заменить только один file-scoped `namespace TranslationDomain;` на `namespace Qva01.A;` / `namespace Qva01.B;`. B дополнительно получает единственную вставку §3. Никаких product ProjectReference, внешних NuGet пакетов, reflection/friend assemblies и подмены native Unit упрощённой моделью.

| Файл | Git blob SHA на закреплённом core |
|---|---|
| TranslationContracts.cs | `9ccff62783661fab24131e206b42280fe4cdc063` |
| TranslationUnitContracts.cs | `0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8` |
| TranslationSemanticTokenizer.cs | `ee6d81c7933fa33a9c57a2111990019ec55e5dab` |
| TranslationStructuredBatchBuilder.cs | `7ceed82476edae9173acc44e2298885c561ca4d5` |
| TranslationTrainedUnits.cs | `1742abfc9fb617dfc66e4d618e0c169da01a6ccc` |
| TranslationMemorySourceLaw.cs | `ba841f4f1839d504444d214feb73c80a4cc24f32` |
| TranslationCheckpointContracts.cs | `3879805b0504df7718adb2760e86aa6b4c3b13ab` |

Записать SHA256 фактических input bytes и patches. Проверить Git blob hash до namespace change; если source drift только в иных файлах, это не повод широкого повторного аудита. Если изменился один из этих семи source — остановить перед Train и записать конкретное несовпадение; нельзя отключать проверку.

Зависимости этих полных файлов — BCL и два metadata имени, без исполнения TranslationMachine/Flow. В каждом namespace добавить `Metadata.cs` с **только**:

```csharp
public sealed class TranslationMachine
{
    public const string DomainTag = "TranslationDomain";
}
public static class TranslationRoutingFlow
{
    public const string FlowId = "translation-routing-flow";
}
```

У metadata класса Machine нет constructor logic, Train, Load, Run или результата. Имя класса также нужно DTO `TranslationMachineLoadResult.Machine`; этот DTO не используется. Metadata constants обслуживают native builder rows и не заменяют обучаемый код. Их значения сверены с [Machine.DomainTag,15][machine-meta] и [Flow.FlowId,26][flow-meta]. Нельзя по наличию имени Machine заявлять полный pipeline. Полная native зависимость closure проверена чтением исходников, **компиляцией ещё не подтверждена**.

`Runner.csproj`: SDK Microsoft.NET.Sdk, OutputType Exe, TargetFramework net9.0, AssemblyName Qva01, ImplicitUsings enable, Nullable enable, LangVersion latest, EnableDefaultCompileItems false. Явные Compile items: семь файлов A + A/Metadata.cs, семь B + B/Metadata.cs, Program.cs, AArm.cs, BArm.cs — 19 items. Два runner файла идентичны кроме namespace arm, пути и поля arm; все literal fixtures общие. SDK9 должен уже быть установлен; его установка или переход на иной runtime вне этой карточки. Изолированная компиляция по образцу существующего [N60-e1 host][host], но без заимствования его данных, порогов, числа запусков или product references.

### 5.2. Контракт runner

`Program.Main` принимает ровно `--arm A|B --output <absolute arm path>`, иначе exit2. Перед learner создавать files через FileMode.CreateNew. Один процесс A и один новый процесс B. В каждом: source/fixture guard → fresh Unit → exact batch и 2 отфильтрованные native rows → 8 before Verify и checkpoint → **ровно два native TrainOnExample** → 8 after Verify и checkpoint → immutable raw JSON → post-output scorer.

До **первого** Train проверить: generated ExampleId ровно `qva01-possessive:semantic`, `qva01-possessive:executor`, `qva01-possessive:verifier`, `qva01-control:semantic`, `qva01-control:executor`, `qva01-control:verifier`; selected IDs ровно два `:verifier` в T1,T2 порядке; обе HeldOut=false; chosen Pass=false; issues ровно `expected 'brother's' for translated quality marker` и `expected 'hello' for translated quality marker`; first ADV на TRAIN соответственно нежно/горестно, разные; Q1,Q2,Q3,Q4,Q7,Q8 читают marker нежно, Q5,Q6 — горестно; все три initial checkpoint коллекции ConceptRules/RequiredTermsBySignature/RequiredTermsBySourceMarker пусты. Это одна fresh Unit на arm, две Unit на весь опыт. Проверить SHA исходников и одинаковые input/gold bytes; несовпадение — остановка до Train. Счётчик actual completed Train увеличивать **после** успешного возврата native call, entered/failed отдельно.

`AArm` / `BArm` сохраняют: `generated-rows.json`, `train-selected.json`, `exam-inputs.json`, `exam-tokens.json`, `before-state.json`, `after-state.json`, `before-outputs.json`, `after-outputs.json`, `train-cost.json`, `score.json`. До Train guard может читать только сырые gold bytes для SHA256, без parse/deserialize labels. Score десериализует gold labels только после записи outputs. Не передавать expected term в private dictionaries; исходные label входят исключительно в native builder TRAIN. Runner не должен измерять только parser или отдельно mock Verify.

После B post-output comparator читает сохранённые A/B outputs, state и cost; пишет `comparison.json`, русские `report.md` и `candidate.md`, без дополнительных native вызовов. В отчёте обязательно показать generated issue и фактически экспортированные terms, а не только ожидаемое число correct.

### 5.3. Единственный запуск после разрешения на выполнение

Launcher находится ровно `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01/launch.ps1`. Внешняя команда — `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01/launch.ps1`; требуется уже установленный PowerShell7, если его нет — `BLOCKED_FIXED_RUNTIME_MISSING`, не устанавливать и не искать альтернативный launcher. В script фиксировать абсолютные paths выше; сначала `Test-Path C:/xexp/QVA01/a01` и fail при наличии, затем create-new `issued.json` до build/Train. Launch должен сохранить точные commands, UTC, exit codes и complete stderr/stdout. Build1, A1, B1; если build/A падает, B не запускается. Hard deadlines: build120s; A30s; B30s. Старт каждого процесса через System.Diagnostics.Process, redirected streams, WaitForExit с указанным timeout; при истечении Kill(entireProcessTree:true), сохранить `TIMED_OUT` с фактическими entered/completed counters и остановить попытку. Содержимое команд ниже не меняется при wrapping. Не retry, не удалять partial output. Обычная ошибка до завершения фиксируется status, затем завершение. Явный STOP имеет приоритет даже перед этой записью.

```powershell
$QvaHost = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01'
$QvaOut = 'C:/xexp/QVA01/a01'
# Только после create-new issued.json и проверки source/fixture manifest:
dotnet build "$QvaHost/Runner.csproj" -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet "$QvaHost/bin/Release/net9.0/Qva01.dll" --arm A --output "$QvaOut/A"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet "$QvaHost/bin/Release/net9.0/Qva01.dll" --arm B --output "$QvaOut/B"
exit $LASTEXITCODE
```

Лаунчер также отключает автоматический runtime retry. При timeout counters брать только из уже сохранённого durable receipt; если его нет, писать `null / NOT_MEASURED`, не 0 и не плановые 2. Нельзя rerun ради восстановления счётчика. Ошибку closure/build исправлять здесь запуском всей цепочки нельзя: записать `BUILD_FAILED_NO_TRAIN`, точный missing symbol и сохранённые логи. Исправление host и новый запуск — отдельное решение, не скрытое продолжение одного бюджета. Создание изолированных source копий и metadata разрешено как техника эксперимента; это не разрешение аналогичных исключений в production или submitted code.

## 6. Метрики, before/after и правила интерпретации

Показать четыре колонки A_before/A_after/B_before/B_after; рядом learning delta для каждого плеча и B_after−A_after. Denominator всегда указан.

- **term fidelity:** число точно сохранённых required terms / 2, из native checkpoint. Empty initial=0/2. Не считать обрезанный `brother` эквивалентом `brother's`.
- **false accept:** число `Pass=true` на Q2,Q4,Q6 / 3; чем меньше, тем лучше.
- **false reject:** число `Pass=false` на Q1,Q3,Q5,Q7,Q8 / 5; чем меньше, тем лучше.
- **correct verdicts:** совпадения native Pass с frozen gold / 8; отдельно possessive, plain и допустимые варианты. Gold оценивает переводческую уместность кандидата, не соответствие внутреннему substring matcher.
- Вывести все 32 native records с Pass/Score/Issues и source/candidate IDs; считать gains/losses по одинаковым ID. Не выводить токены/латентные scores вместо ответа Unit.
- Время и managed allocations: отдельная граница вокруг двух TrainOnExample; preparation/builder, Verify и export вне train timer. `GC.GetTotalAllocatedBytes` — выделенные байты, не RAM. Сохранить state bytes; peak process working set при наличии указать отдельным process наблюдением. Один A→B run с JIT не доказывает ускорение. Missing measurements пометить NOT_MEASURED, не нулём.

**Статический прогноз, не измеренный результат:** исходный пустой Unit принимает все 8 — correct5/8, falseaccept3/3, falsereject0/5. После A ожидаются terms `[brother, hello]`, correct6/8, falseaccept2/3, falsereject0/5. После B ожидаются `[brother's, hello]`, correct6/8, falseaccept0/3, falsereject2/5. Причина потерь B — Q7/Q8 не содержат literal ASCII term. Это объясняет, почему снижение false accepts может быть полезным успехом при неизменной общей точности и ухудшении false rejects. **Ожидаемые числа запрещено помещать в results как measured или подгонять под них данные.** Если actual отличается, оставить actual и объяснить по source/trace; новых кандидатов и дополнительных прогонов не добавлять.

## 7. Отчёт, решение и порядок

Язык отчёта и общения — русский; source identifiers, commands и exact strings сохранить. Навыки находятся под `C:/Projects/llm_mvp_core/docs/skills/`: `code-examination-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Это проверенные имена на закреплённом core; широкого поиска другого skill pack не требуется. Инструкции пользователя о малом опыте, any-metric success, STOP/NEXT и protected folder имеют приоритет над прежними skill thresholds/CI/approval правилами. Ту же модель исполнителя оставить по умолчанию.

Минимальные разделы `report.md`: статус/домен; source pin и one-factor diff; точные TRAIN/EXAM/call counts; реальная мутация state; четыре before/after колонки; улучшения по каждой оси; все ухудшения; все failures/cost bounds; границы применимости; `candidate.md` с точным original method и предложением переноса **после решения Любы**. Даже если измерено полезное улучшение, никаких production repairs, исправления всего тестового набора, CI/submit или удаления старых опытов. Если успех только по false accepts, так и назвать, не обещать универсально лучший переводчик.

Позиция в актуальном реестре: **VT-APOSTROPHE-01 → RMS-ROW-REDUCE-01 → IDENTITY-M1-01 → KN-router → END**, после документированных завершённых learning unblockers. Не возвращаться к старому BPE или прошлым карточкам при команде NEXT. В текущей задаче выполняется только документирование, автоматический запуск следующего опыта не разрешён.

[builder]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L94-L104
[required]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L258-L266
[regex]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L566-L567
[issue]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141
[train]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1145
[parser]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1249-L1257
[marker]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1279
[allow]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationSemanticTokenizer.cs#L89-L99
[verify]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1089
[export]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1175-L1194
[concepts]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationUnitContracts.cs#L57-L77
[host]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L103-L218
[machine-meta]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationMachine.cs#L15
[flow-meta]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationRoutingFlow.cs#L26
