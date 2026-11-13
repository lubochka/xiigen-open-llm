# N34 — новая отдельная гипотеза: одно корректно принятое занятие занимает два места в KNN top-k из-за alias full/prefix signature

**16.10.2026 · SOURCE-CONFIRMED CONDITION / QUALITY IMPACT HYPOTHESIS / NATIVE DOTNET NOT_RUN.** Независимое дополнение к уже опубликованным N16 FIFO, N14 duplicate ID, N23 semantic key, N30 six-direction; **не переписывает** эти исследования.

## 1. Новое различие

При обучении `ExecutorUnit.TrainOnExample` один chosen сохраняется под тремя ключами: `example.SemanticSignature`, заново рассчитанными `sourceTokens.PrefixSignature` и `sourceTokens.StructureSignature`. При коротком тексте, где всего один или два различных meaningful semantic concept и не более трёх rule tags, **полная сигнатура может совпасть с префиксной**. Для такого источника код вызывает `AddPair` дважды с **буквально одинаковым** `(signature, source, target)` в одном bucket. `TranslationKnnStore.AddPair` безусловно дописывает запись, не проверяя существующий source+target+lesson identity. Позднее `FindExactWithSourceGuard` делает `Take(k)` **без unique lesson dedup**, а `ExecutorUnit.TrySelectKnnCandidate` видит только первые `k=5` и последовательно пытается выбрать подходящего соседа.

**Подтверждённая проблема состояния:** один training lesson может создать два slot в том же bucket и раздуть `KnnStore.Count`. **Гипотеза качества:** при полном раннем bucket (или несовместимых ранних переводах) удвоение старых записей выталкивает поздний правильный кандидат за `top-5`, даже если по source-law он подходит. Пока нет native доказательства, что такие истории и accept conditions реально происходят в RP01: это **TranslationMachine learner**, не direct Student LoRA.

## 2. Точная цепочка

- [TranslationSemanticTokenizer.Tokenize L124–140](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationSemanticTokenizer.cs#L124-L140): `Signature = pattern + all meaningful concepts + rule tags`, `PrefixSignature = pattern + first two meaningful concepts + first three rule tags`. Для коротких примеров при этих границах сигнатуры **могут быть равными**, что проверить реальным tokenizer dump, не предполагать всеобщего равенства.
- [TranslationStructuredBatchBuilder L90–111](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L90-L111): `example.SemanticSignature` строится из `sequence.Signature` и потенциальных `AppendRuleTags`; условие `example.SemanticSignature == sourceTokens.PrefixSignature` должно быть проверено после admitted rejected tags.
- [ExecutorUnit.TrainOnExample L404–426](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L426): `AddPair(example.SemanticSignature, source, chosen)`, затем `AddPair(sourceTokens.PrefixSignature,...)` и `AddPair(sourceTokens.StructureSignature,...)` без проверки равенства строк.
- [TranslationKnnStore.AddPair L206–217](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L206-L217): `bucket.Add((sourceText, targetText))` — unconditional append.
- [TranslationKnnStore.FindNearestCascade L246–292](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L246-L292): полная bucket имеет приоритет и обрезается `Take(k)` после source-guard, но **без dedup**.
- [ExecutorUnit.TrySelectKnnCandidate L335–365](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L335-L365): `k:5`, перебор лишь выданных neighbor и downstream `IsTargetCompatibleForDirection`; поздний, но правильный, может не попасть в список для этой проверки.
- [ExportCheckpoint L328–351](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L328-L351): все записи экспортируются и заново `AddPair` на cold restore, то есть дубли сохраняются как learned state.

Это **отличается** от прежнего N16-07 (повторный TrainOnExample для **одного ExampleId**): здесь достаточно **единственного** вызова `TrainOnExample`; дубли появляются внутри самого Unit из-за коллизии двух сигнатур. И отличается от N16 FIFO: FIFO вытесняет новые записи вообще, а **alias-дубли удваивают давление на fixed top-k даже без повторного обучения**.

## 3. Минимальный source-logic proxy и область честного результата

[Исполняемый JS proxy](34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY.mjs) и [receipt](34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY-RESULT.json). Четыре условных урока под одним full/prefix signature, `k=5`: первые три имеют outputs, несовместимые с RU→EN target script, четвёртый — корректный English. В модели текущего двойного AddPair bucket из 8 элементов, `top5=[A,A,B,B,C]` и допустимый `D` невидим. При дедупликации **только идентичного lesson под тем же ключом** bucket из 4 элементов, `top5=[A,B,C,D]`, `D` доступен. **Не реальный TranslationMachine output.** Source guard, фактическая admissibility, prior phrase/word/exact memory, направления и final product path здесь не моделировались; сценарий quality-loss условный.

## 4. Один маленький native .NET эксперимент, без Windows CI

**Вход:** четыре законных `TranslationLearningExample` с `UnitId=executor-ru_en`, разными source IDs, одним реально вычисленным full/prefix key (проверить ***до*** training), уникальными chosen, без exact-source heldout, плюс `k:5`. Первые candidates должны быть *допустимы при AddPair*, но отвергаться только **поздним** target-law guard; если нельзя честно получить такой корпус, hypothesis NOT_TRIGGERED и не делать искусственно неприемлемые пары для приписывания улучшения обучения.

**A (первичное состояние):** один вызов `TrainOnExample` для каждого lesson; снимки ключей, `bucket` (прямой checkpoint inspection), `Count`, `FindNearestCascade` и candidate choices; следовать `TrainedTranslationAdapter` до ordinary consumer и verifier. Отметить duplicate source ID per key и режим, по которому новый кандидат стал невидим.

**B (одна переменная):** в `TrainOnExample` dedup только `(signature, lesson identity)` **в рамках этого вызова**, не globally dedup повторные валидные independent lessons; остальные score, k, source law, UnitState, order, WMap, Verifier, checkpoint и source panel равны. Лучший minimally invasive control: формировать `Distinct()` только на списке трёх **signature keys** перед `AddPair`; сохранять один evidence occurrence на различном key. Отдельно тестировать key collision вместе с различными sourceIds, чтобы не схлопывать разные legitimate lessons.

**Exam:** 4 простых A/B плюс 8 sealed source-family-disjoint query cases на одном learnable law, 2 no-alias controls (более двух concepts/rule tags), один inconsistent language candidate и один known-good KNN without phrase preemption. Gold — независимый domain law: `correct_final/wrong_accepted/abstained/critical_regression`; candidate-recall@5/uniqueLessons@5 только вспомогательные. Повторить после cold restore. Если нет действительного duplicate same-key, результат `NOT_TRIGGERED`. Если есть duplicate, но ни одна real final output не меняется, статус `NO_QUALITY_GAIN_OBSERVED`, а не успешное обучение. Полный Windows CI / main500 / переписывание `results/Rxx` не требуется для экспериментального вывода.

**Фальсификаторы:** (i) в native admission `example.SemanticSignature` никогда не равна prefix для relevant dataset; (ii) источник отфильтрован source law раньше top-k; (iii) все допустимые target уже входят в baseline top5; (iv) exact/phrase/wordmap всегда доминирует; (v) after one-factor patch native heldout law score не изменился. Во всех случаях causal quality effect не подтверждён, хотя кодовое double insertion остаётся наблюдаемым при выполнении его условия.

## 5. Приоритет, безопасность и владельцы

**Приоритет:** P1 для Translation KNN, **после** RP01 REC02 и проверки, что product consumer вообще читает этот KNN. Применимость: короткие контекстные EN/RU/HE/ES предложения, а не все домены автоматически; `C#/SQL` require independent parser and distinct meaning preservation. Архитектор описывает ownership и условия alias, исполнитель проводит один fixed-index native probe, независимый reviewer требует diff/source line и semantic gold, human report разделяет actual code double insert / JS proxy / actual .NET quality. Protected core docs **не изменять**.


## Уточнение достижимости и учёта уроков — 12 ноября 2026

Это профилактическое усиление постановки по [аудиту переносов](https://github.com/lubochka/llm_mvp_core/blob/5afb9d265fb62db8c9d4ea00f61e5f2ed9e9fc3a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/codex-product-review.md). Описанное выше условие относится к историческому source; настоящий аудит не исполнял N34 native и не заявляет, что двойная вставка всё ещё присутствует после всех поздних правок продукта.

Перед изменением pin актуальный source и проверить одну настоящую принятую строку от builder/tokenizer до bucket. Если текущий writer уже устраняет равные ключи, сохранить это как выполненный перенос и проверить его существующую приёмку; не назначать второй одинаковый ремонт. NOT_TRIGGERED и отсутствие эффекта в consumer — допустимые исходы.

Оракул разделяет: logical lesson ID, admission occurrence, alias signature, physical bucket slot, candidate rank и final output. Нельзя подменять «одна копия урока под тем же ключом» глобальным distinct по source/target: два независимых законных урока с одинаковым текстом сохраняют отдельное происхождение. Дедупликация alias внутри одного Train и idempotency повторной доставки одного lesson — разные контракты.

Будущая проверка содержит два обязательных отрицательных контроля: перестановка alias-ключей не меняет состав логических кандидатов; добавление независимого урока не исчезает из памяти из-за равенства текста. При равных scores смотреть состав списка **до** Take(k): сортировка оставшихся пяти не доказывает, что правильный шестой не был потерян. Gold — законный final answer, не число записей и не заранее выбранный candidate ID.

Обычный запрос обязан действительно дойти до KNN: сохранить adapter ID и причины отказа более ранних exact/phrase/word путей. Искусственное выключение этих путей допустимо только как отдельная механическая диагностика, не как доказательство потери качества обычного продукта. После restore снова проверяются реальные индексы, logical multiplicity и final result; tags/счётчики из TRAIN не подставляются вместо чтения состояния.

До исполнения явно пронумеровать panel из раздела 4 и убрать неоднозначность «4 простых A/B»: 4 основных + 8 sealed + 2 no-alias + 1 incompatible + 1 known-good = 16 уникальных RowId при таком прочтении. Для двух arms до/после restore это 64 final readouts; cold baseline, intermediate lookup и training имеют отдельные счётчики. Это предлагаемый точный ledger прежнего перечня, а не уже израсходованный бюджет или разрешение скрытых дополнительных запросов.
