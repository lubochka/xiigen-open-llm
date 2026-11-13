# Промпт исполнителю VT-APOSTROPHE-01

**Текущая работа — только документирование.** Этот промпт описывает будущую изолированную попытку; наличие промпта не является командой запустить обучение. Когда текущим заданием будет разрешено исполнить этот опыт, выполняй ровно [карточку VT-APOSTROPHE-01](../cards/VT-APOSTROPHE-01.md). Статус сейчас `NOT_MATERIALIZED / NOT_RUN`.

**Немедленный STOP/NEXT.** После «стоп», «закончить немедленно» прекрати действия, включая инструменты, запись отчёта, тесты, review, коммит и cleanup. Ответь кратко уже известным состоянием. После «следующий эксперимент» перестань дорабатывать прошлый опыт и перейди только к указанному следующему; «ещё закончить отчёт» не исключение. Не удаляй старые эксперименты, outputs и failed attempts, не перезаписывай их. Не повторяй запуск и не меняй данные ради красивого результата.

**Критерий успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост общей точности не обязателен. Рядом покажи все остальные показатели, включая ухудшения. VT-APOSTROPHE-01 может уменьшить false accepts и одновременно увеличить false rejects при неизменной accuracy; это смешанный результат, а не основание скрыть одну сторону.

## Задание и предел

Тема обучения — Перевод; направление RU→EN; получатель — настоящий `QualityVerifierUnit`. Проверь один фактор: сохранение полного target token `brother's` при native Train, вместо обрезания до `brother`. Метод изменения — **только** экспериментальная вставка в `TryParseRequiredTerm` из §3 карточки. Native builder, tokenizer, marker selector, Verify, `Contains`, gold и corpus одинаковы в A/B. Не исполняй parser-only benchmark, mock словарь или генератор ответов; такая подмена не проверяет обучение.

Используй текущую модель и режим сессии по умолчанию. Никакого внешнего teacher/judge, новой LLM модели или поиска checkpoint. Не путай модель исполнителя с обучаемой native Unit. Neural updates0, full Flow calls0.

## Что прочитать

Core pin — `66947785d1318f7cbbc88118f3156cdc75001e16`. Корень — `C:/Projects/llm_mvp_core`.

В `docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Прямые инструкции Любы о малом опыте, any-metric success, protected folder, STOP/NEXT выше старых thresholds, требований полного CI и правил production. Не расширяй опыт ради них.

Далее полную карточку и ровно семь source файлов таблицы §5.1. Native causal map: `TranslationStructuredBatchBuilder.cs` 94–141,258–266,566–567 → `TranslationTrainedUnits.cs` 1095–1145,1249–1257 → actual state → обычный `Verify` 1052–1089. `TranslationSemanticTokenizer.cs` 89–99 гарантирует два разные ADV marker; `TranslationTrainedUnits.cs` 1175–1194 экспортирует state. Два marker не должны оказаться одним OR bucket. Не повторяй общий аудит других branches и 799 документов: квитанция новизны уже приложена.

## Exact implementation contract

Пути: source/fixtures `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/Qva01`; host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01`; output `C:/xexp/QVA01/a01`, arms A/B. Короткие пути обязательны. Защищённую папку `docs/plans/replanning-possible-diffs` не изменять. Original production files и tests не менять.

Скопируй семь **полных native** files в пространства имён Qva01.A/B; SHA guard из карточки, namespace change одинаков, single parser patch только B. Metadata содержит лишь DomainTag/FlowId и тип для unused DTO; не добавляй алгоритм. Runner.csproj net9.0, AssemblyName Qva01,19 explicit Compile items, no ProjectReference/no packages. Host пока не создан и не собран; не утверждай обратного.

TRAIN — literal два JSONL ряда карточки, в одном порядке. BuildGeneralBatch Structural/heldOut:false производит 6 rows; выбери только 2 verifier rows. Первое требование `brother's` связано с `нежно`, второе `hello` — с `горестно`. До первого Train guard проверяет все 6 generated/2selected IDs, HeldOut=false, exact issue и chosen Pass=false, firstADV нежно/горестно без collision, Q1/Q2/Q3/Q4/Q7/Q8→нежно и Q5/Q6→горестно, immutable source/corpus/gold, и **все три initial checkpoint коллекции пусты у каждой Unit**. Всего 2 Unit instances, по 1 на arm. Actual completed Train counter увеличивай после native return; entered/failed отдельно. Если предпосылка не выполняется — `PRECONDITION_FAILED`, остановись без подбора новой фразы.

EXAM — literal8 строк карточки, включая **допустимые** Q7 paraphrase и Q8 U+2019. Gold отдельно: до Train разрешён только SHA256 сырых bytes без parse; labels десериализовать только после сохранения outputs. На arm1native Unit,2 TrainOnExample,8 Verify до+8 после,2 state exports; всего 4 updates/32Verify. После Train сохранить фактическую мутацию. Никаких warmup, дополнительного seed, расширенной панели или измерительных repeats. A_before/B_before должны быть одинаковы; если нет — invalid сравнение без последующего «починить» outputs. При timeout отсутствующие durable counters — `null / NOT_MEASURED`, не 0 и не плановые 2; никаких rerun для восстановления счётчика.

Единственный запуск при отдельном разрешении исполнения: `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01/launch.ps1` по §5.3 карточки — create-new issued guard,1`dotnet build` deadline120s,1 A process deadline30s,1 B process deadline30s. `a01` уже существует → BLOCKED_EXISTING_ATTEMPT, не удалять и не менять runId. Перед материализацией existing source/host-root→BLOCKED_EXISTING_HOST_OR_SOURCE без перезаписи. Missing fixed checkout→BLOCKED_FIXED_CHECKOUT_MISSING, missing PowerShell7/SDK9→BLOCKED_FIXED_RUNTIME_MISSING; не устанавливать и не искать другой runtime. Timeout→kill exact process tree/record TIMED_OUT, stop без retry; build/A fail→B не запускать. Запиши error/log, останови попытку; никакого полного solution build или tests. Это исключения техники isolated experiment, не разрешение таких исключений в production/submitting.

## Измерение и return packet

Train timer/allocations охватывают только два actual TrainOnExample, отдельно от builder/Verify/export. Сравнение по 8 matching ID, term fidelity/2, falseaccept/3, falsereject/5, correct/8, все 32 rawoutputs. Прогноз `6/8 A_after` и `6/8 B_after` из карточки **не результат**: не копируй его в measured поля и не подгоняй labels. Если результаты иные, сохрани и объясни; новых запусков не добавляй.

Отчёт **по-русски** в `C:/xexp/QVA01/a01/report.md`. Порядок: статус/домен; source+manifest+one-factor diff; фактические data/calls; before/after state; A_before/A_after/B_before/B_after таблица; полезные delta отдельно; все regressions и 32 выхода; costs+failures; limitations. `candidate.md` указывает exact original method и предлагает перенос только после решения Любы. Измеренный выигрыш разрешает назвать кандидат успешным по конкретной оси, не вносить исправление в production.

Порядок текущего реестра: **VT-APOSTROPHE-01 → RMS-ROW-REDUCE-01 → IDENTITY-M1-01 → KN-router → END**. При NEXT не возвращайся к прошлому BPE/VT ради оформления. Сейчас только документы; автоматических переходов к runtime нет.
