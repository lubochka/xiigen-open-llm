# N55 — два точно локализованных разрыва между «что выучили» и «какой ответ измерили»

**19.10.2026 · независимый source review.** Основание — непосредственное чтение Git blobs [GOLD@31346d8](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2) и [CURRENT@6d36492](https://github.com/lubochka/llm_mvp_core/tree/6d36492714ebc1b9ae5f31a5a1da00673f524d83). Это **НЕ** исполненная native .NET проверка ухудшения качества. Новизна ограничена точными стыками N55, которые проверены в этом исследовании; при обнаружении более раннего тождественного отчёта объединить IDs, не дублировать root causes. [Главная GOLD-глава](00-01-PREPARETOGPL-GOLDEN-RESTORE-2026-10-19.md) и [быстрые эксперименты](55-RP01-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md).

**Категории доказательств:** SOURCE_CONFIRMED = код ведёт себя указанным образом при входе; COUNTEREXAMPLE_CONSTRUCTED = можно построить вход, делающий ветвление явным; HISTORICAL_NATIVE = запущенная ранее .NET панель; DOMAIN_CAUSE_PROVEN = одинаковый recipient, before/after, frozen semantic gold и native output. Здесь имеются первые две категории и ссылки на отдельные HISTORICAL_NATIVE результаты, но **нет DOMAIN_CAUSE_PROVEN**.

## N55-01 — внутренняя EvaluateAsync проверяет RU→EN даже для чужого направления

**Точное доказательство, не приговор новой ветке.** В [current TranslationMachine.EvaluateAsync L148–198](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L148-L198) каждый item передаёт только item.Prompt в TranslateForEvaluationAsync и затем сравнивает text с ExpectedResponse по StringComparison.Ordinal. В [helper L686–709](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L686-L709) context всегда фиксирован: source_language="ru"; target_language="en"; topic_domain="general". Ни язык, ни направление, ни recipient каждого item не передаются.

**Отрицательное доказательство новизны:** точно **такой же** helper с RU→EN существует в [GOLD TranslationMachine L527–551](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L527-L551), а GOLD EvaluateAsync использовал его. Поэтому **нельзя** утверждать «это было сломано при переносе из PrepareToGPL». Узкая новая для данного аудита гипотеза состоит в *активации уже существовавшего неправильного контракта* новым multi-direction caller: одна рабочая EN→RU единица может считаться не обучившейся, если оценщик тестирует RU→EN.

**Домен/получатель:** перевод; EN→RU, HE→EN, ES→EN как профили, только если caller использует **этот** EvaluateAsync. Не приписывать его P125/P25: native P125 оценивает ordinary Flow независимо и видит 19/24; RP01 neural scorer другой. Существующие [метрические caveats](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/58-unit-metrics-and-generation-consumer-boundaries.md) отдельно разбирают RU_EN-only MeasureUnitCoverage и собственный heldout path; это смежное, но не тождественное утверждение.

**Наблюдаемые следствия при применимости:** val accuracy относится к неверному маршруту; PassesGate threshold 0.75 и _accuracyAfterLastBatch отражают чужой профиль. ExportCheckpoint сохраняет AccuracyAfterLastBatch, так что неверная оценка может стать persisted scalar, хотя KNN maps и сами ответы не изменились. «Машина не научилась» и «оценивалась не та машина» — разные объяснения.

**Быстрый фальсификатор N55-01:** импортировать один и тот же обученный EN_RU checkpoint P125, не выполнять обучения; на 6 коротких EN→RU заданиях:
A — native ordinary Flow с source=en,target=ru;
B — native generic EvaluateAsync;
C — минимальная временная ветка диагностического harness, которая вызывает тот же Flow с metadata en/ru и тем же frozen gold (без правки production core).
Если B routes RU_EN, а A/C EN_RU и A/C дают верные ответы, гипотеза про неправильно измеряемого получателя подтверждена **для этого caller**, а не доказана причина RP01. Если native caller не использует EvaluateAsync, эффект для него отсутствует. После повторного чтения checkpoint сравнить state (кроме ожидаемого scalar) и outputs.

## N55-02 — новая candidate-conditioned KNN policy изменилась, но не входит в checkpoint identity

**Прямой diff:** в [GOLD TranslationUnitContracts.cs L268–293](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationUnitContracts.cs#L268-L293) FindExactWithSourceGuard начинает с exact bucket и выполняет similarity/topK. В [CURRENT L268–294](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationUnitContracts.cs#L268-L294) добавлен фильтр **до Take(k)**: отбрасываются записи, для которых TranslationMemorySourceLaw.Compare(querySourceText,pair.Source) == Contradiction. Это **реальное различие алгоритма inference**, но само по себе скорее защитное улучшение, не дефект.

**Узкая policy:** [TranslationMemorySourceLaw.cs L7–44](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMemorySourceLaw.cs#L7-L44) задаёт версию **en-simple-copular-polarity-v1** и определяет Contradiction лишь для очень ограниченных английских copular clauses, когда субъект, tense и predicate совпадают, а присутствие not противоположно. Для любого иного предложения возвращается Unknown, что **не равно Compatible** и не даёт основания объявлять fail.

**Стык состояния:** корневой [TranslationMachineCheckpoint schema1](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationCheckpointContracts.cs#L19-L56) сохраняет version, KNN, Units, metrics, history, но **не сохраняет эту версию executable policy**. [ExportCheckpoint L275–302](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L275-L302) не записывает policy; [ImportCheckpoint L306–368](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L306-L368) проверяет schema/machine/domain/structure, но не policy digest.

**Гипотеза качества:** *одна и та же* обученная память KNN, включая тот же SHA JSON и machine Version, может давать другой результат после обновления двоичного consumer без дообучения. Cold-process P25 проверяет 24/24 **с одной и той же DLL**, поэтому этот cross-runtime вопрос не решает. **Не откатывать защитный фильтр из production просто ради совпадения GOLD** — сначала определить, исправляет ли он ложное принятие, не снижая correctness полезных примеров.

**Фальсификатор N55-02:** один immutable checkpoint, один sealed набор и pinned старый/current consumer; сравнить KNN bucket before/after и output/adapter/confidence. Контраст: “the box is empty” против stored “the box is not empty”, matched subject+predicate; контроли другой субъект, другая modal/question construction, correct polarity. Предварительно проследить фактическое заполнение signature bucket и candidate route, чтобы не принять неактивированный фильтр за отрицательный результат. Метрики: false-accepted contradictions / covered, correct/total, abstentions, same-checkpoint state equality. Новое качество может улучшиться за счёт безопасного abstention; семантическая польза ещё не измерялась.

**Предлагаемый архитектурный contract (не implementation claim):** checkpoint provenance хранит отдельно fingerprint обученных записей и fingerprint executable retrieval policy; при проверке «качество после восстановления» обе версии именуются. Возможны совместимый импорт с явно объявленной сменой policy и репорт различий — не требуется отвергать все старые state.

## N55-03 — дополнительная узкая гипотеза: условие Contradiction не устойчиво к внутренним Unicode-пробелам

**Основание:** policy regex допускает **[space или tab]**, а не любой Unicode whitespace, между английскими словами, и распознаёт полярность только в полной регулярной форме. Так, строка с обычными пробелами “the box is not empty” потенциально даёт распознанное not, а та же строка с U+00A0 между is/not даёт Unknown. При этом KNN source tokenizer и другие normalization paths могут обработать эти строки иначе.

**Новизна:** это не новое открытие «отрицание теряется» (см. ранее [ошибки P125](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md)). Узкое предположение — **метаморфная нестабильность нового фильтра при эквивалентных raw formatting variants**. Пока source-level, native consumer route и фактическая способность ученика NOT_RUN. Риск есть лишь там, где кандидат дошёл до candidate-conditioned law. Нельзя расширять regex на произвольные тексты без контроля quotation/negation scope.

**Самая дешёвая проверка:** одна learned copular pair; шесть вариантов одной и той же отрицательной пропозиции — ASCII space, tab, NBSP U+00A0, narrow NBSP U+202F, thin space U+2009, две обычные space. По каждому зафиксировать Compare∈{Compatible,Contradiction,Unknown}, сохранённый target, выбранный Unit, correctness и false accepted. Критерий для политики — **одинаковая семантическая безопасность** на содержательно эквивалентных вариантах; нельзя считать все Unknown автоматически неправильными до вызова продукта.

## 4. Матрица угроз и контрдоказательств

| ID | Source факт | Отличие именно от GOLD? | Уже измеренная деградация? | Класс |
| --- | --- | --- | --- | --- |
| N55-01 | generic EvaluateAsync routes RU→EN на любом item | **Нет**, кодовый seam существовал в GOLD | **Нет**, применимость caller требует trace | inherited evaluator contract, conditional |
| N55-02 | added candidate-conditioned policy, policy digest absent from schema1 | **Да**, новый KNN filter; отсутствие version field в обеих схемах | **Нет**; возможен полезный safety gain | current-vs-GOLD consumer divergence |
| N55-03 | regex narrow internal-whitespace grammar → Unknown | **Да**, относится к новому policy | **Нет** | metamorphic coverage hypothesis |

**Негативный контроль:** сравнивать не только raw list of KNN candidate IDs: это лишь доступность. Для утверждения «ухудшилось обучение» нужны gold answer quality, одинаковые обученные Unit, заданный domain profile и ordinary consumer. При изменении только routing/policy фиксировать CONSUMER_GAIN/LOSS, а не WEIGHT_LEARNING_GAIN/LOSS.

## 5. Минимальный кодовый / экспериментальный handoff

- **Исходники читать полностью перед изменением:** TranslationMachine.cs, TranslationUnitContracts.cs, TranslationMemorySourceLaw.cs, TranslationCheckpointContracts.cs, TranslationTrainedUnits.cs, TranslationRoutingFlow.cs, TranslationDomainTests.cs.
- **Инструментация временная:** emit input language/direction, selected unit/adapter, source policy verdict, checkpoint KNN/state fingerprints, output, gold verdict. Без изменения учебного корпуса и без ослабления existing guardrails.
- **Малые панели сначала:** 6 direction cases, 8 policy polarities, 6 Unicode metamorphic variants; source-disjoint train/validation.
- **Измерения:** correct/total, false accepts, abstentions, route identity, before/after content SHA, ms/call, peak memory; trained-parameter delta = 0 в этих опытах.
- **Reject overclaim:** source-level PASS не native PASS; обычный 24/24 cold restore на одной DLL не cross-version equivalence. Independent code/architecture reviewer и повторная native репликация **NOT_RUN**.
