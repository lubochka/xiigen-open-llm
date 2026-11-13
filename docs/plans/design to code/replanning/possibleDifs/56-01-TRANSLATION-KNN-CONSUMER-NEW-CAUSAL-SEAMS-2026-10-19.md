# N56-01 — новые стыки причин: именно переводческий Executor → kNN → Verifier → Flow

**2026-10-19 · SOURCE-CONFIRMED CALLER MAP + HYPOTHESES NOT_RUN.** Здесь **новые вопросы о взаимодействии уже известных механизмов**, а не заявление, будто впервые обнаружены \`TopicDomain\`, first-hit, WordMap, negation или отсутствие калибровки. Первоисточники прочитаны напрямую в \`llm_mvp_core@eb05219c27663835ce927c7bf97cf1274c07cddb\`; все proposed outcome numbers ниже — **фикстуры**, не реальные measured gains. Пишем только в planning/possibleDifs; защищённый экспериментальный каталог не менять.

## 1. Прямо подтверждённая последовательность потребителей

[\`ExecutorUnit.Execute\`, L264–330](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L330):

\`exactSourceMap (confidence .93) → phraseMap (.86) → wordMap (.50) → conditional kNN (.88) → fallback (.49)\`.

[\`TrySelectKnnCandidate\`, L333–364](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L364) вызывает \`FindNearestCascade(signature,prefixSignature,structureSignature,k:5,querySourceText)\` и **возвращает первый прошедший фильтры Target**, с **постоянным** \`Confidence = 0.88\` (не проверкой качества перевода). [\`TrainedTranslationAdapter.TranslateAsync\`, L117–139](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L139) пропускает candidate с confidence≥.5 через \`QualityVerifier.Verify\`, кроме особого phrase-map override, и экспортирует \`max(candidate confidence, verifier score)\` (cap .99). [\`TranslationRoutingFlow\`, L84–126](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationRoutingFlow.cs#L84-L126) запоминает adapter_id, а не source neighbour ID/score внутри общего результата.

[\`TryComposeFromWordMappings\`, L366–400](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L366-L400) проходит при **не менее двух mapped слов** и **mappedRatio ≥ 0.50**; unknown слова пропускаются, выполняется E27 reorder при полном покрытии. Поэтому kNN не будет вызван, если эта ветка уже вернула текст; доля покрытия не равна сохранению полярности или ролей. [\`ExecutorUnit.TrainOnExample\`, L404–459](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L459) заносит exact/semantic/prefix/structure/word/phrase данные. \`topicDomain\` пришёл в \`Execute\`, но в показанном методе не применяется; в \`TrainOnExample\` exact key тоже без TopicDomain.

Существующий [\`TranslationMemorySourceLaw\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMemorySourceLaw.cs#L11-L43) распознаёт узкие простые английские copular clauses и polarity; вне диапазона **UNKNOWN, не automatic reject**. Этот guard сам не обеспечивает универсальную POS/syntax/semantic корректность.

## 2. N56-NEW-C1: kNN улучшили, но пользователь не видит изменений, потому что более ранняя память перехватывает запрос

**Новизна аудита:** единый **каузальный контракт между P54/P51/P107 и переводческим ordinary consumer**: общая метрика kNN точности может улучшиться, но итоговый текст не меняется из-за exact/phrase/word precedence. Сама очередность и возможный shadowing уже обсуждались в N55; здесь нов только конкретный *factorized translation-specific observable* и критерий несостоятельности kNN-only repair. **Не записывать как подтверждённый root cause RP01**: RP01 neural Student может обходить этот consumer полностью.

- **Профиль:** перевод EN→RU; повтор RU→EN лишь после подтверждения supported adapter. 2 урока с phrase/word mappings и 4 EXAM: negation, role swap, order, near-match; \`TRAIN != EXAM\` по whole-source; обязательный контраст — хотя бы 1 вход, где \`wordMap\` срабатывает раньше kNN.
- **A:** frozen exact/phrase/word policy и исходный kNN; **B:** тот же exact/phrase/word, **изменяется только kNN candidate selector** (без изменения training/index/scorer). Кандидаты и их законность фиксируются одной записью/снимком; если B не достигается через обычный Flow, два исхода *NOT_REACHABLE*, не «невидимый успех обучения».
- **Diagnostic C:** read-only trace с отключением раннего word branch **только в изолированной тестовой обвязке**, не product change; показывает скрытый потенциал kNN, но НЕ является production B и не измеряет полезную архитектурную замену.
- **Нужно сохранить:** selected channel, neighbour signature/id/target/order/score/law, \`mappedTokens\`, \`mappedRatio\`, verifier verdict, final adapterId, raw translation, semantic verdict (отрицание, субъект, объект), timing.
- **Фальсификатор:** первый \`exact/phrase/word\` ветвь не срабатывает ни в одном исследуемом запросе, оба A/B реально заходят в kNN; тогда именно shadowing на этой панели неприменим.

## 3. N56-NEW-C2: «первый допустимый из пяти» как отдельный owner, НЕ generic KnnLogicalUnit

В protected [P107](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P107-lawful-neighbor-aggregation.md) выбран **Generic KnnLogicalUnit** и k3 для категориальных \`TOutput\`. Это полезная методика, но **не proof**, что проверены \`TranslationKnnStore\` и first-eligible k5 внутри \`ExecutorUnit\`. Само first-hit не новое; **новый пробел — owner/caller coverage**.

- **A/B на одном frozen snapshot:** A=current first eligible; B=иной **строго законный** способ выбора среди *тех же* пяти источников, при неизменных фильтрах и \`k\`. Для текста нельзя голосовать raw targets как классами. B, если нет trusted semantic equivalence codec, только deterministic chooser по заранее объявленной admissibility/calibration (например, source-role/polarity compatibility), с abstain при недоказанной совместимости; это **отдельная гипотеза**.
- **Мини-случаи:** negated vs positive, verb/object swap, grammatical word order, topic ambiguity, all-safe same output. Явно записать, почему каждый кандидат допустим; не использовать ожидаемый ответ TEST внутри selector. Для C# AST/SQL нужна другая структурная ветка; этот B нельзя объявлять универсальным алгоритмом.
- **Измерения:** correct/answered, correct/all, harmful accepts/all, abstain/all, selected neighbour ranks, latency, memory. Сохранить прежние правильные запросы — не подгонять scorer/lookup под 4 examples.
- **Фальсификатор:** 5 retrieved items отсутствуют; первый hit всегда единственный admissible или независимая предметная оценка считает A правильным. Тогда k5 vs first не объясняет потерю на данной панели.

## 4. N56-NEW-C3: фиксированный .88 и внешний Verifier дают confidence, не связанный с качеством соседа

**C:** непосредственный \`TrySelectKnnCandidate\` возвращает \`.88\` каждому принятому соседу; downstream может поднять эту уверенность за счёт verifier. **H:** близкие по форме, но неверные по роли/negation KNN кандидаты могут достигать обычного пользователя с высоким confidence. Важная разница: \`Confidence\` — pipeline signal, не calibrated P(correct).

- **Сравнивать отдельно от C2:** A frozen chosen hit и нынешний confidence propagation; B **только** calibrated acceptance/abstention policy, без изменения выбора, расстояний, памяти и verifier. Требуется заранее подписанная truth rubric с semantic obligations. Контроль правильной candidate при низкой confidence; контроль неверной candidate с высокой.
- **Primary:** число **неверных принятых** ответов и предметная корректность/coverage; reliability binned по actual correctness; abstention cost. **Secondary:** confidence score. Изменение только отчётного confidence без route effect — не «улучшение обучения».
- **Известное пересечение:** [P93 generic confidence](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P93-confidence-abstention.md) и прежние N35/N55 negation/consumer. **Новый audit target** — exact translation \`ExecutorUnit k5 → Verifier → Flow\` и receipt каждого отказа.

## 5. Наблюдения, которые не должны превращаться в ложный regression

- \`TopicDomain\` не включён в exact key: уже **P51.1**, не новая находка. Дополнительный тест нужен, если одна source form имеет разные значения в finance/river. Нельзя смешивать query domain with linguistic direction; проверять оба состояния/consumer и old-domain retention.
- \`0.49 fallback → wrong trained PASS\` **не подтвердилось**: trained adapter отвергает fallback и вызывает Bootstrap, а ordinary Flow различает Bootstrap.
- \`k5\` здесь — запрос **пяти** соседей, не доказательство majority aggregation. Сходство по токенам исключает некоторые стоп-слова; source-law отсекает только явно доказанное contradiction, не весь негатив. Нельзя объявить source-law глобально сломанным.
- Версия policy \`en-simple-copular-polarity-v1\` не тождественна версии сохранённого kNN индекса; прежний N55 уже отметил contract gap. Холодный restore и actual law обязаны записываться раздельно.

## 6. Crosswalk защищённых экспериментов (read-only)

| Новая проверка | Ближайший защищённый материал | Gap |
|---|---|---|
| C1 kNN effect hidden by earlier candidate | P51.1 TopicDomain; P54 phrase/word composition; P125 actual Executor learning | Нужен один A/B на **фактическом translation caller** с channel trace и native before/after |
| C2 translation k5 first-eligible | P107 generic k3; P16 order; P28/P43 morphology/context | Generic candidate aggregation **не** доказывает translation k5 correctness |
| C3 constant .88 × verifier × Flow | P93 generic confidence, P57 semantic scoring, P56 verifier | Измерить конкретную долю wrong accepted на обычном TrainedTranslationAdapter |

Для всех опытов план и expected находятся в [N56-03](56-03-FAST-DOMAIN-AB-EXPERIMENTS-AND-EXECUTOR-REVIEW-2026-10-19.md), фактических результатов сегодня нет.
