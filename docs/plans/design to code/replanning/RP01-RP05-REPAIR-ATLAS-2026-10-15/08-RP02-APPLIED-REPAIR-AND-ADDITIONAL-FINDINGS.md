# 08 · RP02: применённый ремонт и дополнительные предметные находки

**15.10.2026 · AUTHOR_SOURCE_AND_DOCUMENT_REVIEW.** Это продолжение [промежуточной публикации](05-RP02-REVIEW-IN-PROGRESS.md), а не ещё один цикл R01–R25. Исправлены сами действующие RP02 packets и маршруты. Ни изменение документов, ни создание commit не означает исполненное обучение или независимый Astra PASS.

## 1. Что опубликовано

[PR №14](https://github.com/lubochka/xiigen-planning/pull/14) объединён в `llm_plan3`: **`34c5c7bcde02c8221b7aaa2a72928e1c3f3e7ee8`**. Проверенный PR head — `2e6b0459c6b41b0dd0791bcd0ddb2500e76b91ba`. До этого по просьбе пользователя опубликованы промежуточные findings: **`f8789d751be6d450039f7aaa2b413e461848f0c1`**. Продолжение после промежуточной публикации действительно выполнено: это не обещание фоновой работы.

Изменения PR: **35 файлов** — 33 внутри существующего RP02 пакета, `replanning/AGENTS.md` и побайтовая архивная копия предыдущего AGENTS. В RP02 **18 основных task IDs и 3 самостоятельных child packets**, не 21 обязательный последовательный запуск. `tasks-v7/` сохраняет совместимые пути; содержимое имеет единственный effective marker `RP02_EFFECTIVE_7_3`. Не создаётся ещё одна competing V8 directory.

Текущие входы:

- [RP02 README](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/README.md), [START одного задания](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7-START-PROMPT.md), [board](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/EXECUTION-BOARD.md).
- [Общий effective context](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-EFFECTIVE-CONTEXT.md), [runbook 7.3 в сохранённом filename V7.2](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7.2-LEARN-FIRST-RUNBOOK.md), [научный протокол и функциональная карта](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7.2-FUNCTIONAL-EXAMS-AND-TRACEABILITY.md).
- [PACKAGE-MANIFEST](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/PACKAGE-MANIFEST.json) и [compatibility manifest](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7-PACKAGE-MANIFEST.json): после merge оба возвращают один blob **`863797be89d62900709b8c6f7e6110fe593eb59d`**.

Сравнение PR head → merge commit не показало изменений RP02 или отремонтированного AGENTS: содержимое перенесено без потери; параллельные RP01/RP03/RP04/RP05/Atlas изменения сохранены. Эти чужие изменения не приписываются данной ревизии и не объявляются независимо проверенными.

## 2. Что подтверждено из исходного атласа

| Пункт исходного 01 | Результат проверки и применённая дельта RP02 |
|---|---|
| 1–2: полный scope, источники и исторические process rules | Прочитаны initial original/task/goal, WorkLeft MASTER/usage/MECH-01/RESEARCH-03, previous rounds opening и JOIN; сохранены четыре уровня, DTC/IDE/прочие owners. Исторические 30/50-pass/DPO_LAST не создают новый обязательный этап. Полная библиотека/все RAG не объявляются прочитанными. |
| 3: один dispatcher | Подтверждён root AGENTS V6A/17 против local V7.2/18. Root AGENTS консолидирован; old blob `9348868c3bb7f062cc5fc4053369666869f3bf99` сохранён точно в `AGENTS-REFERENCE-9348868.md`. Local README/start/board/оба manifests/AGENTS и compatibility forwarders теперь ведут на current tasks-v7. |
| 4–5: reuse и first learning | 00A различает неизвестный caller и доказанную missing edge. 00C только conditional; 02B1 имеет отдельные CODE/UPDATE children; first pilot использует один готовый домен и сразу 03A/E, не все карточки. |
| 6–8: роли, objective, state | Requested selector и judge = CODEX **astra 6 ultracode** в двух вызовах. Chinese open external author пишет ОБЕ стороны. Ranking loss не называется response-DPO; trial save/update/cold use/quality/promotion разделены. |
| 9–10: смысл вектора и domain-law kNN | Полностью прочитан KnnLogicalUnit на K97e4, отдельно HnswIndex Add/Search/Save/Load/RandomLevel. Подтверждены локальные top1/predicate-bag механизмы, но не все ordinary callers. Сохранены AST, negation, referent, schema/dialect/UNKNOWN, явная learned θ и θk. |
| 11–13: Unit/Flow/Orchestrator/Graph/continual | Сохранено функциональное владение RP03/RP04/RP05 и RP18 projection. RP02 не строит четыре новых trainers и не выдаёт своё ранжирование за весь Graph. Полный аудит их code не заявляется. |
| 14–15: причинное измерение/PC/minimum tests | C0/T по одному объявленному изменяемому фактору; source-family heldout, отдельная confirmation, coverage/abstention, полный PC и bulk retention. 0 новых tests для документов/S/E/review/no-change; changed hard risks — ближайшие existing checks. |
| 16: конкретная карточка вместо шапки | Все 18 current карточек заменены единым текстом; у каждой цель/контекст/skills/source/фазы/GOOD/BAD/EDGE/WHY/falsifier/recovery/return. Добавлены три effect children, без потери их контекста. |

Исходный атлас содержал правильные принципы ещё до этой ревизии. Их повторение здесь не объявляется новым открытием. В частности, запрет Windows coverage как доказательства обучения существовал в V7.2; исправлялись противоречащие детали и пробелы исполнения.

## 3. RP02-specific findings F17–F25: что теперь исправлено

Полные source-bound описания до ремонта находятся в [05-RP02-REVIEW-IN-PROGRESS](05-RP02-REVIEW-IN-PROGRESS.md). Номера здесь принадлежат namespace **RP02-Fxx**, не general extension 17–36 и не RP04 F17.

| Finding | Проверенное основание | Применённый механизм ремонта | Как опровергнуть исправление |
|---|---|---|---|
| RP02-F17: лишняя RU dependency для C# | Старый 01D/header/board требовал 00B2 либо сразу два паспорта | 01D/board/manifest: один selected domain; другие обязательны для их scope claims | C# executor всё ещё ждёт завершения RU/SQL перед своим первым θ step |
| RP02-F18: раскрытые arms в blinded schema | Старый workbook `blinded_arm: P|C0|T` | 03A/workbook: opaque labels, private curator map, grades lock перед unblinding | Judge payload содержит checkpoint role/название treatment или подсказку победителя |
| RP02-F19: update/argmax/gain смешаны | Old runbook/02B2 требовали changed answer как условие эффекта | Отдельные update, consumption, output, quality, cost, promotion; efficiency при заданной non-inferiority | θ честно обновлена, но та же правильная строка объявлена «обучения не было»; либо hash назван gain |
| RP02-F20: circular dynamic-k cost | 02B2 использовал margin/density без стоимости получения; HNSW topK ≠ ef | Двухфазная pre-query→paid probe→expansion policy; effort/recall/consumed k отдельно | Policy использует будущий margin или скрывает цену probe/feature extraction |
| RP02-F21: точный normalized-ID alias | Оба call sites используют trimmedId; два collision nodes и последний dictionary payload | 00A/CODE уточняют реальный defect и conditional guard до частичной insertion | Утверждается, что один Trim даёт разные index/dictionary keys; либо два vectors под одним ID всё ещё смешивают provenance |
| RP02-F22: unverified caller ≠ missing caller | Old 00A отдавал NOT_WIRED по неполному trace | SOURCE_TRACED/CALLER_UNVERIFIED/NOT_WIRED_CONFIRMED; 00C только confirmed | Новый bridge создаётся по отсутствию результата одного поиска |
| RP02-F23: negative memory не только diagnostics | F primary разрешает negative «what not to produce» | Authorised avoidance/contrastive signal, отдельный positive serving pool, final arbiter | Negative record либо выброшен полностью, либо принят как positive output/чужой tenant |
| RP02-F24: requested versus runtime model | Manifest choice отличался от пользовательского requested label | Exact `astra 6 ultracode` для selector/judge; actual ID/receipt отдельно | Runtime alias выдуман или отдельный call не состоялся, но записан PASS |
| RP02-F25: similarity confidence ≠ probability | KnnLogicalUnit clamp/round score и warning внутри Ok | 00A/02B2/reviewer проверяют actual caller; calibration/coverage только по наблюдениям | 0.95 similarity называется 95% accuracy либо warning назван reject |

F23/F25 и ряд методических принципов также описаны параллельным general extension §§20/30/32. Это **совпадающие выводы с точным применением к RP02**, не независимые новые ошибки сверх каталога. Новая конкретика F20/F21 опирается на прочитанные HnswIndex methods, а не только имя класса.

## 4. Дополнения после первой промежуточной публикации

### RP02-F26 · Экзаменатор не должен применять урок

**DOCUMENT_PHASE_CONFLICT_VERIFIED.** В прежнем `tasks-v7/RP02.03B.md`, E prompt: «Simulate legitimate new feedback ... apply exactly once, then restart». Общие правила при этом называли E независимым exam. Это смешивает изменение системы и оценивание. Исходник доступен [на P78cc](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.03B.md).

**Исправлено:** 03B передаёт новый законный урок 02B1/UPDATE; отдельный U меняет θ; 03A/E только читает immutable arms и не меняет optimizer, rubric или accepted pointer. **GOOD:** учитель/optimizer не видят SEALED, examiner не обучает. **BAD:** examiner исправляет θ до удовлетворительного результата — сравнение уже не независимо. **EDGE:** нет реального U receipt → WAITING_FOR_ARTIFACTS, не попытка обучить внутри E. **Recovery/owner:** 03B/03A; при нарушении evidence invalid, parent сохраняется.

### RP02-F27 · Слово OFF обозначало разные эксперименты

**DOCUMENT_DESIGN_AMBIGUITY_VERIFIED.** Прежний 03C задавал Student OFF/ON × kNN OFF/ON, не фиксируя, OFF — это parent parameters или удаление компонента. Его S prompt также требовал существующие checkpoints до подготовки схемы. [Старый 03C](https://github.com/lubochka/xiigen-planning/blob/78ccad8763adc9fd6bf6499f27260c2e9c4c8bab/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/RP02.03C.md).

**Исправлено:** C08 = parent/updated Student adapter × θ0/θ1, при одной lawful retrieval architecture. Удаление deterministic/learned компонента — другой отдельный ablation. S готовит протокол сейчас; E ждёт настоящие arms. **GOOD:** `Y11−Y10−Y01+Y00` имеет определённые factors. **BAD:** в одном OFF нет генератора, в другом сняты guards. **EDGE:** LoRA меняет encoder space → fixed compatible external space либо честно coupled intervention; не смешивать indexes. **Recovery/owner:** 03C/01C/01B; несопоставимые клетки не становятся доказанной synergy.

### RP02-F28 · Ненаблюдавшийся search action не является отрицательным уроком

**DESIGN_COMPLETION, NOT A CLAIM OF OBSERVED RUNTIME BUG.** Старый 02B2 требовал назвать θk, но не давал ограниченной модели достаточно конкретный источник action labels/objective. После ремонта [LEARN child](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/tasks-v7/children/RP02.02B2-LEARN.md) задаёт `Qw(h,a)=w·χ(h,a)`, наблюдённые lawful TRAIN/OP_DEV action outcomes, явно фиксированный quality/cost utility и pairwise preference loss с одним step.

**GOOD:** две допустимые training-ветви действительно измерены и оценены; predecision χ не содержит будущего результата. **BAD:** неисполненная ветвь названа плохой или «быстрая» без измерения. **EDGE:** tie/noisy time/no legal action → NO_UPDATE/UNKNOWN. **Recovery:** новый разрешённый TRAIN observation, не SEALED tune; owner 02B2. Heuristic policy остаётся отдельным честным baseline, а не ложным learned θk.

### RP02-F29 · Fit данных и итоговая оценка разделяются до токенизации

**PROTOCOL_STRENGTHENING.** Family split теряет смысл, если vocab, normalizer, codebook или calibration обучены на SEALED. KnnLogicalUnit.Train действительно fit-ит BPE по переданному corpus; это **не доказательство**, что hidden data уже утекли. General extension §§31–32 описывает тот же риск.

**Исправлено:** 00B1–B3, 01B и 03A требуют split до любого preprocessing fit и не допускают hidden reference answers/embeddings в datastore/teacher. **GOOD:** один fixed encoder для memory-only claim. **BAD:** BPE rebuilt на enlarged mixed corpus, весь gain приписан θ. **EDGE:** input intentionally supplies a document as evidence → отделить допустимый question context от скрытого solution. **Recovery/owner:** 01B/03A: загрязнённый panel заменяется; нет ретроспективного «маленького valid gain».

### RP02-F30 · Локальный referent ID не является числовой семантикой

**DESIGN_RISK, NOT VERIFIED PRODUCT DEFECT.** Требование хранить referent_id полезно, но raw произвольный integer/hash может выучить случайные названия вместо отношений. **Исправлено:** 01D/01B/00B2 включают relational semantics и α-renaming counterexample.

**GOOD:** тот же объект имеет те же связи после переименования ID. **BAD:** object=7 даёт male, object=9 female без смыслового основания. **EDGE:** сменился настоящий antecedent, а не только ID — prediction может обоснованно измениться. **Recovery:** исправить feature meaning, не запретить смену референта. Owner 01D/01B; оценка на независимых discourse families, не сотни snapshots.

### RP02-F31 · Нельзя считать kNN score произвольной HNSW метрикой

**DESIGN_COMPATIBILITY_CHECK.** Contextual grammar/POS/referent score может зависеть от запроса и не совпадать с геометрией индексированных vectors. **Исправлено:** fixed compatible recall representation + domain-conditioned reranker для первого опыта; отдельный backend/coordinate contract при изменении distance. HNSW construction randomness фиксируется сохранённым index artifact, а не выдуманным public seed API.

**GOOD:** одинаковый serialized index у C0/T θ эксперимента. **BAD:** rebuild с другой геометрией/случайным графом, весь эффект приписан θ. **EDGE:** очень маленький lawful corpus — exact scan может быть лучшим; это измеряется, не запрещается. **Recovery/owner:** 01B/02C; disclose coupled changes, exact/ANN C05 отдельно. Нет обещания, что ANN всегда быстрее или accuracy улучшится.

### RP02-F32 · Качество и стоимость нельзя улучшить простым отказом на всех задачах

**EXAM_COMPLETENESS, overlapping general extension §§28/30.** Accuracy только среди отвеченных задач позволяет скрыть массовое abstention. **Исправлено:** 03A/workbook требуют answered coverage, justified/unjustified abstention и общий eligible denominator; fixed finite measured path set для regret, иначе PATH_REGRET_NOT_IDENTIFIED.

**GOOD:** одинаковая задача/семья, известны correct/error/abstain и все затраты. **BAD:** 1 правильный ответ и 99 отказов объявлены 100% полезной точностью; один observed path назван глобально лучшим. **EDGE:** нет независимого best-path oracle → не выдумывать regret. **Recovery:** восстановить полные denominators/compare set и пересчитать; owner 03A, никакого нового production test suite.

## 5. Предметная проверка новой организации работы

Первый ready-domain путь: 00A + selected 00B + minimum 01D + 02B1/S + 03A/S → reuse/CODE → ровно один UPDATE → немедленный independent C0/T exam. Действительные source/type/rights/parent/trial integrity остаются prerequisites своего эффекта. RP02 domain-kNN не ждёт RP01, а успех RP01 не доказывает RP02.

Остальные функции имеют конкретные эксперименты: C01 domain axes; C02 representation; C03 θ; C04 contextual/learned effort; C05 exact/ANN; C06 typed fusion при подходящем type; C07 next-role/token/AST/action; C08 actual Student×θ interaction. Блок B0/B1/B2 проверяет новые законные уроки и удержание старых навыков на сопоставимых family panels. C#/EN→RU/SQL examined отдельно для multi-domain claim. Не нужно сначала сделать все эти опыты, чтобы измерить первый правильный урок.

Минимальный software proof — только изменённые hard invariants: rights/type/schema, direction/finite update, once/journal/recovery/cold. Source/evaluator/prompt/review/no-change = **0 новых tests**. Это не уничтожает existing safeguards и не превращает фактический security failure в PASS. Windows delivery и learning science имеют разные статусы. Эффективность оценивается по всему PC, parser/features/probe/search/Student/optimizer/index/parent/trial/failed calls, а не одному adapter size.

**Осознанная scientific граница:** nondeterministic поведение не закрывается coverage; при этом модель может иметь детерминированный inference и всё равно обучаться. Нужны независимые новые задачи, причинный контроль и неопределённость, а не искусственная случайность или подгонка ожидаемых строк.

## 6. Источники и границы доказательства

Baseline P: `78ccad8763adc9fd6bf6499f27260c2e9c4c8bab`; K: `97e4af3eadd3981320f698d104134a01984a2542`. Прочитаны substantive sections всех 18 прежних RP02 cards; 00A/00C/01D/02B1/03A/03B/03C/04A получены полностью либо последовательными relevant sections; остальные — выделенные substantive phase/exam/example/failure sections, а не FULL_READ всех архивных вводных. Все новые task bodies составлены и авторски проверены; это не independent external review.

Полностью прочитан `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`; `06.VectorSearch/HnswIndex.cs` — Add/Search/Save/Load/RandomLevel и начальные search helpers. Не выполнен полноценный runtime trace всех ordinary consumers. No runtime defect reproduction. Учебные данные не создавались, модели не вызывались, weights не менялись.

Шесть запрошенных skill-файлов открыты по действительным начальным relevant sections: trainable-architecture-decision, phase-planning, plan-review, generated-code-review, prompt-patch-authoring, conversation-intent-and-self-reflection. Большие WorkLeft/previous rounds/RAG прочитаны выборочно, с указанным первичным V/RAG→F переходом; полный MD↔NDJSON reconcile не выполнен. Missing same-name V/RAG-06 NDJSON не подменяется придуманным файлом; current context указывает на реальное дерево и обязательную honest read-depth запись.

После merge прочитаны оба manifest на exact SHA: один blob. PR filename list содержит 35 ожидаемых scoped paths. Сравнение final merge с проверенным PR head показывает отсутствие новых RP02/AGENTS изменений и сохранение concurrent sibling/Atlas work. Это проверка целостности публикации, не доказательство обучения или полного семантического аудита всех cross-repo источников.

В ходе продолжения проверены официальные abstract records первичных статей [LoRA](https://arxiv.org/abs/2106.09685), [Adaptive Nearest Neighbor Machine Translation](https://arxiv.org/abs/2105.13022) и [DPO](https://arxiv.org/abs/2305.18290). Это подтверждение ссылок и различия методов, не полный новый research review и не перенос benchmark gains на XIIGEN.

Parent `replanning/README.md` на момент readback ещё имел label CURRENT V7.2, хотя его ссылки ведут на обновлённые canonical RP02 paths. Управляющий root AGENTS и local pack уже согласованы на 7.3. Такой label нельзя называть полностью обновлённой root-навигацией; дальнейшая адресная правка должна сохранить прочие ссылки/параллельную работу, не добавить новую conflicting banner. Это не runtime blocker и не повод возвращаться к R01–R25.

**Итог доказательств:** `RP02_DOCUMENT_REPAIR_PUBLISHED; AUTHOR_AUDIT_ONLY; PRODUCT_CODE_UNCHANGED; PRODUCT_TESTS_ADDED=0; DATA_WEIGHTS_UNCHANGED; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`. Отдельные перечни рисков не считаются исполненными экспериментами и не повышают статусы соседних слайсов.
