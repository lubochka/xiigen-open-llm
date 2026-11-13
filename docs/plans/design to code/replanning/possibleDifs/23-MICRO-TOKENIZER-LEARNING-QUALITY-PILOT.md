# 23. Экспресс-эксперимент: меняет ли регистр языка полезность выученного Translation Unit?

**Карта:** к [новым source seams 23](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md). **Статус:** эксперимент полностью спроектирован; .NET runner/learned consumer в этой сессии **не запускались**. Независимый CODEX/Astra review — **не выполнен**. **Первый шаг — ≤ один малый локальный test invocation**; без Windows GitHub и без полного CI.

## Идея простыми словами

Машине показывают несколько правильных RU→EN соответствий. После обучения ей дают **тот же** незнакомый русский запрос дважды, меняя только один символ в метаданных: код языка `ru` против `RU`. Ни корпус, ни разбиение на задачи, ни веса, ни LoRA, ни checkpoint между запросами не меняются. Если с `ru` машина корректно применяет урок, а с `RU` возвращает неправильный ответ или fallback — мы установили настоящую причинную потерю полезной памяти на границе языка.

**Важно:** изменение одной внутренней подписи KNN ещё не означает ухудшения качества. Нужно увидеть правильность или ошибку конечного ответа в обычном `TranslationRoutingFlow`. Сравнивать свойство памяти (Unit) и свободную генерацию RP01 Student отдельно.

## Требуемый результат: два уровня и строгий verdict

**Уровень 1 — контракт, ≤30 строк кода:** один `TranslationSemanticTokenizer` получает тот же `rawText` с `ru`, `RU`, `Ru`, `ru `. Записать `Pattern`, массив `(Word,Pos,Concept)`, `Signature`, `PrefixSignature`, `StructureSignature`, `RuleTags`. Отдельно проверить, что обычный `TranslationRoutingFlow.ResolveDirection` принимает все четыре формы. Достаточно одного демонстрационного слова «запустить», для которого при русском входе выделяется `VERB`, а при «RU» — `NOUN`; следствие — различающиеся подписи. Результат такого уровня: **CODE_CONTRACT_DIVERGENCE**, не «learning degraded».

**Уровень 2 — качество за один мини-запуск:** train 4–6 одобренных примеров; eval 12 **новых source-disjoint** запросов с тем же конечным законом/смыслом плюс 2 exact-known smoke-контроля. На каждом eval item обычный consumer получает A (`source_language=RU`) и B (`source_language=ru`) **без повторного обучения и переинициализации**, порядок вызова чередовать. По каждому запрашиваемому тексту записать ответ, real `adapter_id`, verification, evidence source (если доступен), gold rubric и вердикт correct/wrong/fallback. Ответы подсчитывать попарно; **не** считать чекпоинт-хеш или PASS юнит-теста метрикой обучения.

### Подготовленный небольшой корпус (адаптировать золотые переводы до запуска, не после)

| Case | TRAIN source / chosen or HELDOUT source | Назначение |
|---|---|---|
| T1 | `добавить проверку` → `add a check` | Базовый закон, RU_EN |
| T2 | `запустить поток` → `start the thread` | Независимая функция |
| T3 | `обработать ошибку` → `handle the error` | Другой semantic context |
| T4 | `вернуть данные` → `return data` | Другая сигнатура |
| H1 | `добавить проверку пожалуйста` → `add a check` | Неизвестная точная строка, тот же закон |
| H2 | `пожалуйста добавить проверку` → `add a check` | Перестановка вежливости |
| H3 | `запустить поток пожалуйста` → `start the thread` | Новый текст, близкое понятие |
| H4 | `пожалуйста запустить поток` → `start the thread` | Новый текст, близкое понятие |
| H5 | `обработать ошибку пожалуйста` → `handle the error` | Новый текст, близкое понятие |
| H6 | `вернуть данные пожалуйста` → `return data` | Новый текст, близкое понятие |
| H7–H12 | 6 других заранее одобренных неповторяющихся парафраз | Дополнительный слепой heldout |
| C1–C2 | точные T1/T2 после обучения | Проверка, что exact-source память скрывает или не скрывает баг |

Это **предложенные автором fixture-строки, а не взятые из RAG gold data**. Исполнитель обязан проверить качество и законность переводов до запуска в отдельном approval sheet; исключить из heldout буквальные нормализованные дубликаты. Если нет хороших H7–H12, провести H1–H6 как *pilot* и не строить статистическую значимость на 6 случаях.

**Подготовленный машинно-читаемый набор:** [23-LANGUAGE-CASING-PAIRED-PILOT-FIXTURE.json](23-LANGUAGE-CASING-PAIRED-PILOT-FIXTURE.json) содержит 4 training rows, 12 heldout формулировок, 4 варианта регистра и явный статус `NOT_RUN`. Предложенные gold-ответы предварительные: обязательна независимая проверка до запуска. Все 12 heldout — вежливые варианты команд; они служат узкому тесту причинной границы, а не доказательству переноса на много доменов.

## Компактный native .NET шаблон для фазы 1 + двух exploratory consumer requests

В `Tests/TranslationDomainTests.cs` уже есть `RunTranslationAsync(machine, prompt, sourceLanguage, targetLanguage)` на базе `machine.GetOrchestratorAsync()` и `LogicalOrchestratorRequest`, а также `BuildDirectionalExecutorExampleWithMappings` — использовать эти реальные helpers и не создавать новый fake consumer. Пример для вставки в ту же test class **НЕ запускался здесь, поведение consumer предположительно и должно логироваться, не подгоняться**:

```csharp
[Fact]
public async Task N23_RuCasePair_ExploratoryOnly()
{
    var tokenizer = new TranslationSemanticTokenizer();
    const string trainText = "добавить проверку";
    const string heldOutText = "добавить проверку пожалуйста";
    var ru = tokenizer.Tokenize(heldOutText, "ru");
    var upper = tokenizer.Tokenize(heldOutText, "RU");
    var mixed = tokenizer.Tokenize(heldOutText, "Ru");
    var spaced = tokenizer.Tokenize(heldOutText, "ru ");

    Assert.NotEqual(ru.Signature, upper.Signature);
    Assert.Equal(upper.Signature, mixed.Signature);
    Assert.NotEqual(ru.Signature, spaced.Signature); // trailing-space variant is a distinct invalid tokenizer code

    var machine = new TranslationMachine();
    var trainedSignature = tokenizer.Tokenize(trainText, "ru").Signature;
    var batch = new TranslationLearningBatch(
        BatchId: "n23-ru-case-pilot-v1",
        DomainTag: TranslationMachine.DomainTag,
        Examples: [
            BuildDirectionalExecutorExampleWithMappings(
                "n23-train-01", TranslationDirection.RU_EN,
                "ru", trainText, trainedSignature,
                "add a check", "negative_dpo_example", [])
        ],
        SourceIds: ["n23-preapproved-train"]);
    var train = await machine.TrainStructuredBatchAsync(batch);
    Assert.True(train.IsSuccess, train.ErrorMessage);

    // Freeze the SAME trained memory; this test records two consumer outputs.
    var aUpper = await RunTranslationAsync(machine, heldOutText, "RU", "en");
    var bLower = await RunTranslationAsync(machine, heldOutText, "ru", "en");
    Assert.False(string.IsNullOrWhiteSpace(aUpper));
    Assert.False(string.IsNullOrWhiteSpace(bLower));

    // Manually log both results, real adapter path, and independent gold.
    // DO NOT assert 'A != B' as proof of quality. Evaluate semantic correctness.
}
```

Команда исполнения **только этого опыта** из корня core (если локальная среда поддерживает .NET 9):

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~N23_RuCasePair_ExploratoryOnly" --no-restore
```

При отсутствующих пакетах/SDK не запускать 60 Windows workflow; диагностировать локальную среду или запускать на заранее рабочей машине. Фазы 1 и 2 разделить: не совмещать исправление и измерение. Уровень 2 требует отдельного прохода по 4 train + H6/H12 и ручной/внешней preapproved semantic rubric.

## Причинная матрица

| Arm | Model/Unit state | Разница | Ожидаемое значение |
|---|---|---|---|
| A: текущий вход | один frozen trained Unit | `RU` в metadata | Контроль возможной потери |
| B: нормализованный вход | **тот же экземпляр** | `ru` вместо `RU` | Единственный causally changed factor |
| C: точно известный source | тот же экземпляр | T1/T2 exact query | Контроль, что exact lookup может маскировать различие |
| D: no-learning | новый Unit без train | `ru`, идентичные heldout | Контроль, что улучшение требует обученной памяти |
| E: shuffled labels | другая память с теми же T inputs, выбранные outputs переставлены циклически | метки не соответствуют урокам | Проверка зависимости правильного результата от правильных labels |

Сравнение A/B — **эффект форматирования входа на обученный consumer**, D/E — доказательство, что полезный ответ действительно обусловлен обучением и не является одним детерминистическим scaffold. Повторить A/B в обоих порядках и после SaveCheckpointAsync→LoadCheckpointAsync; **restore как отдельная ось**, не смешивать с первичным A/B.

## Метрики и заранее установленные границы

- `correct_accepted`: ответ соответствует утверждённому gold по смыслу; `wrong_accepted`: ответ видим пользователю, но неверен (особенно отрицания/термины); `fallback_abstain`: честный запрос дополнительного контекста или Bootstrap.
- Первичная метрика: paired gain правильных окончательных ответов **B − A** на H (в шт. и долях). Вторая метрика: уменьшение `wrong_accepted`. Третья — покрытие (сколько система реально ответила, не убежала в fallback).
- `contract_diff_rate`: доля запросов, у которых разные POS/signature при одном языке в разных регистрах. Это **только промежуточный признак**.
- Каждая запись: `source_id`, `train/heldout`, `language code`, `semantic signature`, `adapter_id`, `output`, `gold`, `verdict`, `checkpoint hash`, `code SHA`. Заранее запечатать gold и имена групп; не изменять rubrics вслед за ответами.
- **Строгое подтверждение влияния:** хотя бы **два** заранее утверждённых heldout случая с `A wrong/fallback, B correct`, **ноль** обратных `A correct, B wrong` и D/E не воспроизводят такой же correct gain. При 6–12 примерах это убедительный *локальный причинный контрпример*, не статистическое доказательство улучшения на генеральной совокупности.
- **Опровержение выбранной гипотезы на этой панели:** POS/signatures отличаются, но парная правильность и route одинаковы на всех H, включая действительно KNN-consuming cases. Не надо объявлять отсутствие багов в других доменах.
- **Невалидная проверка:** сравнивать новый B с другим model checkpoint, смешать test/train, переписать chosen/rejected, менять source text вместе с языком, считать само изменение vector margin или тест PASS улучшением.

## Два независимых очень дешёвых follow-up (только после регистрации результата N23.01)

**N23.02 (EN POS/negative):** frozen EN→RU corpus; A — нынешние generic POS/signature, B — shadow tokenizer с минимумом `VERB/PRON/NEGATION`; остальной `TranslationKnnStore`, source filter и gold неизменны. 8–12 заранее утверждённых unseen полярностных случаев `always/never`, `must/must not`, `active/passive`. Критерий — paired law-correct ответы, не lexicographic neighbor score. Запрет на подгонку B по blind cases.

**N23.03 (clause state):** одна trained RU_EN память, 6 пар `без префикса` / `с неотносящимся предыдущим предложением`; A — current `prevConcept`, B — сброс после границы предложения, C — тот же префикс без границы. Считать точную классификацию нужного action/concept и paired correct/wrong/fallback у ordinary consumer. Если несущественно для quality, не делать дорогостоящую архитектурную замену.

## Поручение CODEX (исполнитель) и независимая проверка

**Цель:** доказать либо опровергнуть, что устойчивая learned memory перестаёт применяться из-за формата языка. Только микропилот. До исполнения прочитать `AGENTS.md`, применимые правила, `docs/skills/architect-behavior-classifier-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`; документ 23, `Tests/TranslationDomainTests.cs`, текущий Flow, Adapter, tokenizer и Unit. Архитектор читает и проверяет, но не пишет production fix до причинного receipt. CODEX/Astra выполняет проверки/независимый review по принятым в проекте полномочиям; их verdicts не изобретать.

1. Зафиксировать ветку, HEAD, source file SHA, доступный рабочий .NET runtime. Выбрать четыре законные train rows и H6 (или H12), preapprove chosen и gold независимо от candidate.
2. Сохранить read-only parity trace. Никаких новых тестов ради квоты и никакого CI Windows.
3. Провести A/B на frozen model/Unit state, D/E негативные контроли. Результат записывать до любого изменения кода.
4. Если B действительно лучше — предложить **одну** минимальную нормализацию на boundary плюс тест миграции/идентичности старых signatures. Новый эксперимент с изменённой реализацией только после сохранённого A/B.
5. Передать независимому reviewer точные ссылки на исходники, fixture и отчёт; reviewer обязан отклонить `learning improved` без native paired quality, подмену H на train, и вывод `это корень RP01 DPO 0/4` без вызова этого Unit из Student.

**Пакет ответа:** `SOURCE-TRACE.md`, `FROZEN-FIXTURE.json`, `CASE-BY-CASE-RESULTS.json`, `QUALITY-VERDICT.md`, `REVIEW-VERDICT.md`. Каждый завершается `RAN/NOT_RUN`, фактическим SHA и указанием, подтверждён ли *именно эффект качества*, а не только различие сигнатур.

**Точки остановки:** как только обнаружен отдельный доказанный случай `same user source + same trained memory + RU vs ru → quality drop`, не масштабировать на 500 DPO пар. Если .NET SDK недоступен — честно `BLOCKED_NATIVE_RUNTIME`; математический/JS proxy не переименовывать в native proof. Рабочие репозитории исходников и RP01 checkpoint не менять без отдельной авторизации на реализацию.
