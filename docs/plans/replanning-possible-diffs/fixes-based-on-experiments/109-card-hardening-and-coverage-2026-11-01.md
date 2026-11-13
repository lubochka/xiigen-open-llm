# 109. Карта усиления карточек и проверка покрытия — 1 ноября 2026

**Поздняя сверка текущего дерева:** ниже описан отдельный срез core790228c. Число89 из него нельзя переносить на HEAD2004e81030c на дату этой исторической сверки как новое доказательство применения. На прочитанных P65/P66/P69/P74 отсутствовали названные верхние адресные блоки/RF-ссылки. В текущем документном дополнении непосредственно записаны и должны читаться обратно шесть конкретных блоков P65/P66/P68/P69/P70/P74; полный повторный аудит89 не заявлен. Первичные уточнения — [review после P65](reviews/2026-11-01-post-p65-evidence-review.md), новый разрыв переноса P56 — [113](113-p56-commit-closure-and-learning-acceptance-audit-2026-11-01.md).
## Что изменено

В89 карточек добавлены адресные верхние блоки приёмки:45 связанных P04–P81 и все44 P82–P125. Старое тело каждой карточки сохраняется побайтово; status/NEXT/budget внутри него не переписаны. Верхние блоки актуализируют границу доказанного на core790228c и задают случаи, которыми можно опровергнуть ложное подтверждение. P125 остаётся RECORDED_RESULT_ALIAS. Это не89 завершённых переносов или89 новых опытов.

[107](107-transfer-and-experiment-audit-2026-11-01.md) содержит источники и выводы; [108](108-addressed-repair-plan-2026-11-01.md) — будущие repairs. [Машинная карта](../../../../experiments-v2/_audits/2026-11-01-transfer-audit/card-hardening.json) содержит original/new SHA256 каждой карточки, проверки и границу изменения. Общий договор39 дополнен правилами observed_count/denominator, actual consumer и честного вывода. README/102/103/104/105/106, required-changes и execution index ведут к одному действующему остатку.

## Ошибка → распространённая приёмка

| Ошибка | Что теперь нужно показать |
|---|---|
| JSON parse принят за atomic transaction | Late typed/train/eval/publication failure и valid batch, состояние сразу после refusal |
| Исправлен соседний owner, обычный reader обходится | Owner/overload/caller matrix, controlled public result и отдельный no-evidence |
| E28 improved branch имеет0 hits | Runtime project identity, per-file pairs, source roles, actual treatment hit и disable-factor control |
| Constant tie назван crossing | Назначенные alpha/observable; positive crossing и negative permanent tie/same alpha |
| Пустой контроль объявлен PASS | observed_count>0; иначе UNEXERCISED, zero denominator→UNMEASURED |
| Proxy coverage или большой contrast объявлены correctness | Отдельные law/каждый знак/actual generation/acquisition и прежний baseline |
| Зелёный адресный log объявлен full gate | Actual before/after summary, known failures, terminal outcome |
| Manifest/архив status принят за исполнение/restore | Actual payload reader и legacy terminal guard; report-only reconstruction Train0 |
| Combined vector uniqueness принята за input distinction | Prompt-only IDs/mask/consumed spans отдельно от target, post-encoding overlap |

## Полный список89 карточек

| Карточка | Причина / адресная проверка |
|---|---|
| [P04](../experimets/cards/P04-preference-contrast.md) | P81/RF10: Знак каждой preference пары |
| [P18](../experimets/cards/P18-state-identity.md) | A09/RF09: Байты состояния и разрешитель версии |
| [P21](../experimets/cards/P21-hnsw-ingress-atomicity.md) | A09/RF09: Топология загруженного индекса |
| [P25](../experimets/cards/P25-cold-restore.md) | A09/RF09: Новый reader без повторного обучения |
| [P29](../experimets/cards/P29-formula-memory.md) | formula-transfer/RF08: Отказ отдельно от неверного ответа |
| [P30](../experimets/cards/P30-learned-fusion-alpha.md) | A03/RF03: Постоянная ничья не пересечение |
| [P33](../experimets/cards/P33-domain-classifier.md) | A05/RF05: Проверяется actual router owner |
| [P34](../experimets/cards/P34-router-labels.md) | A05/RF05: Пустой запрос и законная метка |
| [P36](../experimets/cards/P36-incremental-router.md) | A01+A05/RF01+RF05: Атомарность памяти и чтение — разные gates |
| [P37](../experimets/cards/P37-empty-router-query.md) | A05/RF05: Не переносить empty guard между классами |
| [P46](../experimets/cards/P46-pair-value.md) | P70/A04/RF04: Вход learner отдельно от ответов |
| [P47](../experimets/cards/P47-file-vector-context.md) | E30+A04/RF04: Адрес перехода и границы файла |
| [P49](../experimets/cards/P49-probe-warm-start.md) | ProbeNN/RF09: Исправленный backward и actual warm state |
| [P50](../experimets/cards/P50-graph-dependencies.md) | A02+A09/RF02+RF09: Старый terminal не разрешает полный повтор |
| [P51](../experimets/cards/P51-translation-context.md) | exact-key+A01/RF01+RF11: Точный контекст исправлен; revision отдельно |
| [P52](../experimets/cards/P52-semantic-annotation.md) | A02/RF02: Continuation и подпись rejected-side |
| [P53](../experimets/cards/P53-batch-atomicity.md) | A01/RF01: Синтаксис не равен транзакции |
| [P54](../experimets/cards/P54-phrase-composition.md) | A06/RF06: Композиция должна сохранить закон |
| [P55](../experimets/cards/P55-translation-laws.md) | polarity+A06/RF06: Узкий перенос can’t и все достигнутые tiers |
| [P56](../experimets/cards/P56-verifier-context.md) | P56+A08/RF07: Целое слово и реальная signature |
| [P57](../experimets/cards/P57-semantic-measurement.md) | P57/RF08: Semantic scorer на actual выдачах |
| [P58](../experimets/cards/P58-feedback-learning.md) | P58/RF05: Feedback должен изменить читаемое состояние |
| [P59](../experimets/cards/P59-correction-visibility.md) | P59/RF09: Видимость correction, цена и смысловая сохранность |
| [P60](../experimets/cards/P60-signature-duplicate.md) | A06/RF06: Membership до top-k |
| [P61](../experimets/cards/P61-unknown-tail.md) | P61/RF06: Хвост должен достигать native writer и reader |
| [P62](../experimets/cards/P62-signature-monotonicity.md) | P62/RF06: Дополнительный tag не ослабляет admission |
| [P63](../experimets/cards/P63-source-containment.md) | P63+A06/RF06: Source guard проверяется до конца cascade |
| [P64](../experimets/cards/P64-tier-postfilter.md) | A06/RF06: Пригодность до завершения tier |
| [P65](../experimets/cards/P65-history-admission.md) | P65/111-P65: ручной contrast admission; RF08 не его владелец |
| [P66](../experimets/cards/P66-heldout-contrast.md) | P66: раздельные blind/controlled панели; адресного native report work order в RF08 нет |
| [P67](../experimets/cards/P67-correction-identity.md) | P67/RF11: Correction authority до порядка |
| [P68](../experimets/cards/P68-pretrain-pipeline.md) | P68/DataLoader: Позиции, край shard и цена отдельно |
| [P69](../experimets/cards/P69-graphrag-context.md) | P69: encoding boundary; RF04/E28 не задаёт новый GraphRAG encoder |
| [P70](../experimets/cards/P70-wikitrace-context.md) | P70/WikiTrace: Prompt-only view после переноса полей |
| [P71](../experimets/cards/P71-perplexity-alphabet.md) | P71/RF03: Алфавит должен различать вход и кодировать кандидатов |
| [P72](../experimets/cards/P72-project-conditioning.md) | A04/RF04: Project key не закрывает E28 protocol |
| [P73](../experimets/cards/P73-domain-conditioning.md) | P73+A04/RF04: Сигнал проекта и достижимое улучшенное плечо |
| [P74](../experimets/cards/P74-memory-eviction.md) | P74: coverage не гарантирует удержание; E42 repair в RF08 не назначен |
| [P75](../experimets/cards/P75-sampler-residual.md) | P75/sampler: Исправленный sampler и граница law |
| [P76](../experimets/cards/P76-utf8-boundary.md) | P76/UTF8: Хвост отделён от внутреннего повреждения |
| [P77](../experimets/cards/P77-rare-class-support.md) | A07/RF08: Редкий класс и действительные source hashes |
| [P78](../experimets/cards/P78-loss-plateau.md) | P78/RF10: Cadence не moving window |
| [P79](../experimets/cards/P79-grammar-leakage.md) | A07/RF08: Утечка не проявилась; пустой control не PASS |
| [P80](../experimets/cards/P80-marker-denominator.md) | A07/RF08: Retention относится к присутствовавшим маркерам |
| [P81](../experimets/cards/P81-input-conditioning.md) | P81/RF10: Контраст, правильность знаков и приобретение |
| [P82](../experimets/cards/P82-file-split.md) | PT-04, P79: TRAIN/EXAM leakage: P82-SPLIT |
| [P83](../experimets/cards/P83-expression-body.md) | PT-04, P71: indistinguishable input: P83-EXPRESSION |
| [P84](../experimets/cards/P84-lambda-family.md) | P71: indistinguishable input, P77: rare-class blindness: P84-LAMBDA |
| [P85](../experimets/cards/P85-composite-obligations.md) | P80: wrong denominator, P77: rare-class blindness, PT-05: P85-OBLIGATIONS |
| [P86](../experimets/cards/P86-fractional-response-mask.md) | P81: wrong preference sign, gradient source audit: P86-MASK |
| [P87](../experimets/cards/P87-mixed-objectives.md) | PT-01, PT-08: P87-ATOMIC |
| [P88](../experimets/cards/P88-unit-context-consumption.md) | PT-02, PT-03, mutable KNN ownership: P88-CONTEXT |
| [P89](../experimets/cards/P89-density-stop.md) | P78: monitor window, P81: wrong preference sign: P89-STOP |
| [P90](../experimets/cards/P90-tagged-search.md) | PT-04, PT-05, P77: rare-class blindness: P90-SELECTION |
| [P91](../experimets/cards/P91-method-pair-split.md) | PT-04, P79: TRAIN/EXAM leakage: P91-PAIRSET |
| [P92](../experimets/cards/P92-dpo-memory-flow-bridge.md) | PT-04, PT-05, P69: truncation collisions, P70: lost owner: P92-REACH |
| [P93](../experimets/cards/P93-confidence-abstention.md) | P74: confidence not correctness, P77: rare-class blindness, P80: wrong denominator: P93-CALIBRATION |
| [P94](../experimets/cards/P94-typed-many-results.md) | PT-03, mutable KNN ownership, P80: wrong denominator: P94-CARDINALITY |
| [P95](../experimets/cards/P95-learned-flow-edge.md) | PT-02, PT-04, E30: current-action key: P95-EDGE |
| [P96](../experimets/cards/P96-async-orchestration.md) | PT-03, PT-08, execution-state honesty: P96-ASYNC |
| [P97](../experimets/cards/P97-learned-graph-order.md) | PT-02, PT-04, PT-05: P97-ORDER |
| [P98](../experimets/cards/P98-tokenizer-capacity.md) | P80: wrong denominator, P76: UTF8 decoding: P98-DENOMINATOR |
| [P99](../experimets/cards/P99-unknown-token-id.md) | P76: UTF8 decoding, P75: sampler support, PT-05: P99-DECODE |
| [P100](../experimets/cards/P100-topk-ties.md) | P75: sampler support, R7: equal/tie control: P100-SUPPORT |
| [P101](../experimets/cards/P101-learning-budget.md) | P81: wrong preference sign, P77: rare-class blindness, execution budget: P101-TRAJECTORY |
| [P102](../experimets/cards/P102-teacher-forced-decoding.md) | P81: verdict correctness, P80: wrong denominator, P61: substring verifier: P102-PREFIX |
| [P103](../experimets/cards/P103-stack-before-normalization.md) | PT-02, PT-03, P77: rare-class blindness: P103-DISPATCH |
| [P104](../experimets/cards/P104-law-eligibility.md) | PT-06, E28: project-qualified key: P104-ELIGIBILITY |
| [P105](../experimets/cards/P105-frozen-reference.md) | mutable KNN ownership, PT-01: P105-OWNERSHIP |
| [P106](../experimets/cards/P106-corrected-semantic-target.md) | P70: lost fields, PT-01, P79: post-encoding leakage: P106-TARGET |
| [P107](../experimets/cards/P107-lawful-neighbor-aggregation.md) | PT-04, PT-07, R7: tie control: P107-POOL |
| [P108](../experimets/cards/P108-metadata-carrier.md) | P70: lost fields, PT-03, mutable ownership: P108-CARRIER |
| [P109](../experimets/cards/P109-gold-validity.md) | P81: wrong sign, P61: substring verifier, P77: class coverage: P109-GOLD |
| [P110](../experimets/cards/P110-numeric-matrix-cost.md) | ProbeNeuralNetwork gradient fix, PT-04, PT-05: P110-MATVEC |
| [P111](../experimets/cards/P111-reward-credit.md) | PT-01, P70: owner loss, PT-02: P111-CREDIT |
| [P112](../experimets/cards/P112-family-disjoint-evaluation.md) | P79: TRAIN/EXAM leakage, P71: indistinguishable input, P77: rare-class blindness: P112-FAMILY |
| [P113](../experimets/cards/P113-feature-energy.md) | P69: truncation collisions, P70: target leakage, PT-03: P113-ENERGY |
| [P114](../experimets/cards/P114-zero-weight-pair.md) | PT-01, P80: wrong denominator, execution budget: P114-ZERO |
| [P115](../experimets/cards/P115-bm25-statistics-cache.md) | P67: state version ambiguity, PT-04, PT-01: P115-CACHE |
| [P116](../experimets/cards/P116-e43-file-boundary.md) | PT-04, P80: denominator, E28: cross-file transitions: P116-BOUNDARY |
| [P117](../experimets/cards/P117-translation-direction-memory.md) | PT-06, PT-05, source polarity scope: P117-DIRECTION |
| [P118](../experimets/cards/P118-projectwiki-content.md) | P70: lost fields, PT-04, PT-05: P118-CONTENT |
| [P119](../experimets/cards/P119-learning-dynamics.md) | P81: wrong preference sign, gradient source audit, execution budget: P119-SIGN |
| [P120](../experimets/cards/P120-semantic-compression-laws.md) | P80: wrong denominator, P71: input/candidate encoding, source polarity scope: P120-MARKERS |
| [P121](../experimets/cards/P121-project-adapter-selection.md) | E28: project-qualified key, mutable ownership, PT-01, PT-05: P121-OWNER |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | P70: lost fields, P74: coverage not law, P80: wrong denominator: P122-CONSUMED |
| [P123](../experimets/cards/P123-design-conversion-critic-evidence.md) | PT-02, PT-04, P77: rare-class blindness: P123-GEOMETRY |
| [P124](../experimets/cards/P124-p3-fixed-discriminator-contract.md) | PT-02, PT-04, P70: owner/input loss: P124-CONTRACT |
| [P125](../experimets/cards/P125-executor-memory-learning.md) | PT-01, PT-06, PT-07, exact topic-key migration: P125-HISTORY |

## Границы распространения

Карточки P01, P02, P03, P05, P06, P07, P08, P09, P10, P11, P12, P13, P14, P15, P16, P17, P19, P20, P22, P23, P24, P26, P27, P28, P31, P32, P35, P38, P39, P40, P41, P42, P43, P44, P45, P48 сохраняют прежние условия106/39; они не объявлены дополнительно глубоко проверенными этим slice. Точный список89 изменённых файлов выше является мерой текущего покрытия. Common contract не создаёт для каждой карточки обязанность заново выполнять все89 проверок.

Планирование: прочитаны и актуализированы8 связанных первичных документов possibleDifs, новый bridge и README. Полный каталог561 entries не объявляется проверенным. Historical scope restrictions относятся к прежним заданиям; текущий запрос прямо разрешает укрепить core/planning descriptions.

Нативные runtime tests и новое обучение этим пакетом не выполнялись. Python readback и source analogues выполнялись отдельно, с их explicit scope. Финальный независимый review и проверка точного publish diff записываются в evidence README/review receipt. Продуктовые defects исправляются будущим108, а не наличием этого coverage map.
