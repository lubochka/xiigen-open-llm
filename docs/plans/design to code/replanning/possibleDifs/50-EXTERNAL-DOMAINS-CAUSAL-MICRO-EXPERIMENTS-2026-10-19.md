# 50 · Короткие причинные опыты: восстановить успешный `external_domains`, затем проверить новые локальные гипотезы

**19.10.2026 · STATUS = DESIGNED / NOT_RUN**. Исходный успешный donor: `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639`, current: `codex/dpo-implementation@649b9343cfda5b81cc54396a3840ad5db5e0b179`. Целевые документы: [главная очередь P0](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md), [точные N50](50-EXTERNAL-DOMAINS-E25-NOVEL-POS-SEAMS-2026-10-19.md). Использовать действующие правила architect/plan-review/generated-code-review/training-data audit и human communication; не подменять эти опыты обязательным Windows GitHub cycle.

## 1. Контракт каждого опыта — проверяем обучение, а не тестовое покрытие

**До любого запуска** зафиксировать `donor_sha, current_sha, corpus_hash, train_ids, heldout_ids, subject_family_id, seed, learner_type, feature_schema, label_schema, checkpoint_sha, unit_id, flow_id, consumer_id, direction, topic_domain`. Не называть EN→RU и RU→EN «двумя доменами»: это направления в теме «перевод». В пределах одного causal A/B фиксировать все параметры, кроме **одной** проверяемой переменной. Критерий качества задаёт независимый judge по эталонам до просмотра ответов; для free answers отмечать соблюдение смысла, отрицания, структуры и отсутствие мусорного текста. Результаты по памяти, pattern classifier и neuron Student — раздельные.

**Для каждого плеча:** `before` -> `Train` (или `no-train` control) -> `after` на **новых** примерах -> `cold process restore` -> **тот же штатный consumer** -> old/new grouped accuracy и per-case diff -> `cost: cpuMs, peakRAM, wallMs`. Слепой кодовый тест/изменение весов — диагностика, не качество. Сначала короткий источник данных; расширять корпус только если есть signal/falsifier. Сохранять raw answers и neighbor IDs, а не только проценты.

При незапущенном эксперименте отчёт должен писать `NOT_RUN`; `predicted_outcome` никогда не переименовывать в `observed_outcome`.

## 2. Первая очередь — именно возврат прежнего успешного поведения

| ID | Домен / получатель | A / B (одно изменение) | Панель/ограничения | Главная метрика; опровержение |
| --- | --- | --- | --- | --- |
| **EXD-R0 (первым)** | Любая одна доказанная donor-способность; исходный consumer и новый эквивалентный consumer отдельно | **A** exact pinned donor + архивная сборка/данные/state; **B** current с *той же* функцией/выходным контрактом, текущий штатный Unit/Flow. Не приписывать различие одной строке, пока не разложили маршрут. | Предварительно сверить архивный run JSON с самим запущенным bin; 8–12 новых heldout задач и negative controls, затем cold restore | `useful_correct/total`, `route_to_trained_recipient`, равенство checkpoint/memory identity. Если donor не воспроизводится, дальнейший attribution STOP: проверить fixture/build. |
| **EXD-R1** | Перевод RU→EN: **E25**, learner `PatternClassifier` и реальный tokenizer/reorder/ExecutorUnit | **A** frozen rule-only, **B** old E25 kNN `Train` над теми же POS labels. Затем *отдельным* опытом consumer with vs without E25-candidate (иначе два фактора). | Реплика архивной 160/32 synthetic, после — 12–20 train/heldout **семейно-разделённых** новых POS и raw текстов, включая VSO/императив/вопрос/сложный порядок; вручную проверять теги | Pattern top1 и downstream `meaning + order accuracy` **отдельно**. Знаки улучшения могут разойтись. Отсутствие downstream gain при росте pattern top1 опровергает тезис о causal consumer benefit. |
| **EXD-R2** | RU→EN технические неоднозначные слова: E26 dictionary (не нейронные веса) | **A** highest priority/previous-concept, **B** left/right POS-context ranking, другие условия одинаковы | 15 archival + 12 новых contextual pairs + unknown-context negatives, split лексических семейств; сравнить отдельно raw tokenizer tags и oracle tags | Word sense accuracy, unsupported-specific-answer-rate, sentence meaning. 15/15 archival не доказывает generalization; отсутствие выигрыша с реальными tags означает loss in feature path. |
| **EXD-R3** | C# next-action/retrieval: E09 AST kNN | **A** same candidate pool без AST feature, **B** тот же kNN с AST feature; одинаковый `k=5`, ranked candidates, labels, one heldout | 8–12 C# файлов из новых семейств `if/try/async/loop`, полностью source/file-family-disjoint; одинаковое число eval cases в обоих плечах | top1 на **одном denominator**, + downstream action correctness. Если эффект только с train-family leakage — восстановление не принято. |
| **EXD-R4** | C# continuation / память переводов: E43 replay | **A** new lessons only, **B** new lessons + фиксированный набор old по выбранному бюджету; model/memory type указан явно | Исходная old/new corpus разнесены по семействам; одинаковый вычислительный бюджет, отдельно bigram, kNN, LoRA; после cold restore | Old retention и new acquisition отдельно, затем free consumer. E43 0.3226→0.3978 на counts не основание применять 30% replay в LoRA. |
| **EXD-R5** | Neuron Student + DPO/LoRA против learned Unit/memory, не смешивать метрики | 4 frozen плеча: **A** RP01 student alone, **B** donor Unit/memory alone, **C** student + donor Unit, **D** DPO student + donor Unit; сравнивать pairwise изоляцию факторов | 8–12 C#/translation задач в **двух отдельных** наборах, раздельные blind judges; прежние B0/C50, tokenizer, reference фиксируются | Полезные ответы из ordinary consumer, answer route trace, teacher-forced scores *дополнительно*. Рост preference margin без правильных ответов = неуспех восстановления. |

**Архивная опора:** [E25 JSON](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) 0.3438→0.875/32; [E26](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) 0.4→1.0/15 fixed; [E09](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) 0.2413→0.3507 с **неодинаковой evaluation panel**; [E43](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) 0.3226→0.3978 bigram counts. Старые числа — HISTORY, не результаты предлагаемых опытов.

## 3. Новые адресные N50, один фактор на опыт

### EXD-N50-01A — не спутать VSO и императив

**Входы:** минимально 8 raw-кейсов с `VERB+PRON+NOUN` (предварительно сверить фактическую POS-разметку!), 8 `VERB+NOUN` control, ещё 8 SOV/SVO/imperative variants; отдельная таблица исходного языка и направления. Никаких заранее подставленных oracle tags в качестве конечного показателя.

**Плеча:** A current `TranslationSemanticTokenizer.ClassifyPattern`, B текущий же разбор с одной явной differentiator веткой для verb-first+subject; C (отдельная диагностика) old E25 trained POS-kNN со state. Нельзя сравнение A→C называть one-factor causal — здесь **две** модификации и нужен промежуточный B.

**Primary:** class accuracy на independent gold, downstream reorder errors. **Negative control:** `VERB+NOUN` должен остаться IMPERATIVE_V. **Falsifier:** в raw-input нет таких POS, либо new branch не исправляет ошибку consumer. **Статус:** source-only candidate, не выполнен.

### EXD-N50-01B — несовместимость 8-label donor / 7-label new Unit

**Входы:** 8 синтетических меток old `SVO,SOV,VSO,IMPERATIVE_V,IMPERATIVE_VS,QUESTION_SV,QUESTION_VS,TOPIC_OV` и serialized `int` для каждой; current label set `SVO,SOV,VSO,IMPERATIVE_V,IMPERATIVE_VS,SVO_INF,UNKNOWN`. Это test только контракта миграции; не имитировать фактическое существование E25 checkpoint, которого не доказали.

**A:** ordinal cast `int` → enum; **B:** explicit symbolic mapping + reject/partial scope for unsupported `QUESTION_*/TOPIC_OV`. **Outcome:** сохранено ли смысловое значение; count lossy aliases; отсутствие silent reinterpretation. **Secondary:** если планируетcя реальное подключение, 8 new-pattern cases after import into Unit/Flow with strict rejection/abstain. **Falsifier:** если реальный production migration никогда не использует эту схему, источник несовместимости пометить `NOT_APPLICABLE`, но сохранить предупреждение перед переносом.

### EXD-N50-02 — пустые биграммы и одинаковые множества при разной кратности

**A:** исходный Jaccard на bigram **set**. **B1:** при `length<2` использовать unigram/POS identity fallback; **B2 (отдельно):** bag/ordered ngrams для повторов. `k=3`, те же memory samples/tie rules/labels и cost budget.

**Fixtures:** 8 коротких `[VERB]` vs `[NOUN]` и 8 однословных/двухсловных контролей; для multiplicity 8 составных `[N,V,N]` vs `[N,V,N,V,N]` плюс уникальные последовательности. Для каждой пары до обучения измерить `distance`, затем `neighbor ids`, после обучения на разнесённых шаблонах `top1, tie_rate, false_nearest_rate`; закончить raw utterance и обычным consumer только если tokenizer доносит такие признаки. **Source-equivalent expected:** `dist([VERB],[NOUN])=0`, `dist([N,V,N],[N,V,N,V,N])=0`; actual native measurements **NOT_RUN**. **Falsifier:** качество не меняется на релевантных задачах или это намеренная equivalence policy (тогда `NOT_APPLICABLE` в профиле).

### EXD-N50-03 — batch-refit вместо continual training

**Recipient:** исторический `SemanticCompiler.PatternClassifier` только при планируемом повторном использовании в разных lessons; не приписывать это `TranslationMachine.TrainStructuredBatchAsync`.

**A:** `Train(oldA)` → `Train(newB)`; **B:** `Train(oldA + newB)` при одинаковом корпусе и k; **C:** separate exact memory store с incremental add (если intended contract допускает). Сначала простая проверка `Train` source: `_train.Clear()`; native outcome должен показать `storedExampleCount`, old/new neighbors и old/new accuracies. `oldA` и `newB` различаются по patterns/source families, без class collapse. **Positive control:** single-batch original E25; **negative control:** `Train(empty)` (должен перевести learner в baseline). **Falsifier:** нормальный интеграционный caller всегда передаёт merged corpus или сам не использует этот learner; тогда регрессии текущего consumer нет.

## 4. Доменная дисциплина / исключённые обобщения

- **C#**: structural KNN/AST, order-of-actions и graph dependences. POS E25 не заменяет AST и не применяется просто потому, что это kNN.
- **RU→EN / EN→RU технический перевод**: POS/context, morphology, word order, actual language IDs; два направления — один тематический домен с двумя сценариями.
- **HE / ES**: включать только если доступны настоящие POS tags + gold, не переносить Russian E26 dictionary и POS ID искусственно.
- **Wiki / DTC / IDE**: проверять собственные trained recipients, graph/Unit contract, source-law veto; не объявлять успешность E25 обучением Wiki.
- **Нейронное DPO**: run отдельно, фиксировать `base/reference`, tokenization, chosen/rejected spans, LoRA topology, per-token gradient, checkpoint and free answer. Ни один перечисленный E25/E26/E43 аргумент не доказывает broken q/v backprop.

## 5. Готовый исполнительский контракт и ревью

**ROLE / SKILLS:** `architect-behavior-classifier-SKILL`, `trainable-architecture-decision-SKILL`, `phase-planning-SKILL`, `plan-review-SKILL`, `plan-self-validation-SKILL`, `generated-code-review-SKILL`, `claim-verification-SKILL`, `planning-dpo-authoring-SKILL`, `human-communication-GLOSSARY`. Загрузить точную версию документа для задания перед изменениями; не делать вид, что старый PASS применим к новому SHA.

**CURRENT TASK:** после явной авторизации провести **один** указанный microexperiment и сохранить подробный receipt с A/B, source SHA, cases, attached raw answers и on/off flag в `replanning/possibleDifs` или другом разрешённом evidence path; после этого независимый reviewer оценивает причинность.

**NOT THE TASK:** переписывание всей архитектуры, производство 500 новых DPO rows, запуск полного Windows GitHub suite, изменение core protected studies, изменение gold после просмотра результатов.

**INPUTS:** закреплённые файлы/JSON из разделов 1–3; ограничения домена и получателя; канонический `chosen/rejected` сохранять, не производить synthetic labels из oracle в скрытом режиме.

**OUTPUTS:** `experiment_id, source_sha, artifact_hashes, corpus_manifest, split, trained_recipient, checkpoint_id, actual_route_trace, before/after/cold answers, metrics_per_case, negative_control, null_falsifier, cost, reviewer_verdict`.

**GOOD:** «E25 после тренировки на новом семейно-разделённом корпусе поднял pattern top1 с X/Y до Z/Y, обычный RU→EN consumer исправил M/N при том же tokenizer и checkpoint SHA; cold-process сохранил результат; доступен trace маршрута». **BAD:** «Стало 16 изменённых LoRA tensors, значит E25 восстановлен» / «в тестах нового enum нет ошибок, значит вопросы и TOPIC_OV переносятся».

**ACCEPT** допускается только когда изменение предметного качества воспроизведено на том же независимом наборе, с честной baseline, указанием получателя и actual consumer, и прочитанным результатом независимого review. `NEEDS_FIX/NOT_RUN` сохраняется, если соответствующее доказательство отсутствует.

**Работа настоящего документа:** только формирование сравнительного плана. Никаких native результатов не создавалось, не разрешалось менять core code и не заявлялась фактическая успешность после ремонта.