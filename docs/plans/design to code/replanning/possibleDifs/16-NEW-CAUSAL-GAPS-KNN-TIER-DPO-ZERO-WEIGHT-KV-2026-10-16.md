# 16 — Независимые дополнительные гипотезы: энергия признаков KNN, нулевой вес DPO и parity KV-cache

<!-- POST117-CURRENT-2026-11-03:START -->
## После P117: нулевой вес уже исключается, граница приёмки уточнена

По [текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) и core 879a07b основной перенос P114 уже есть: нулевые пары исключены из потери, градиента и знаменателя, all-zero партия возвращается до получения разрешения на шаг. Историческую source-причину N16-02 ниже нельзя повторно назначать как отсутствующий фикс. Свежий native запуск проверки здесь не выполнен; равенство сохранённых B/C ограничено реально доступными артефактами, не заявляется новая проверка бинарных весов.

**Что обязан проверить следующий назначенный исполнитель:** одна положительная пара и та же партия с нулевой парой дают одинаковые итоговые веса, optimizer и loss-denominator; all-zero не запрашивает permit, не изменяет step/state и возвращает определённый результат. В каждой проверке отдельно учитываются фактический consumer и тип результата. Плохой контроль сравнивает только total loss, пока optimizer step всё равно двигается. Хороший отрицательный контроль изменяет один из этих эффектов и заставляет итоговую приёмку отказать.

R08 исторического опыта остаётся самостоятельным: finite/blind/finalizer,16 дополнительных наблюдений и timeout нельзя закрыть наличием product filter. Обязательный false/missing контроль должен менять общий verdict и exit. N16-01 и N16-03 этим переносом не закрываются. Далее сохранена исходная редакция 16.10; запуск её probe не назначается автоматически.
<!-- POST117-CURRENT-2026-11-03:END -->

**Дата:** 2026-10-16. **Статус:** SOURCE-CONFIRMED / MATHEMATICAL-COUNTEREXAMPLE / RP01-CAUSALITY-NOT-PROVED.  
**Изученный core:** `lubochka/llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` (ветка `codex/dpo-implementation`).  
**Цель:** дополнить существующие [44 DIF](01-PRIORITIZED-DIFFERENCES.md) и [новые KNN-gaps 14](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) только проверяемыми *новыми* механизмами, не подменяя их успешными прошлыми экспериментами и не приписывая ошибки изолированному RP01 Student.

## Короткий вывод для архитектора

| ID | Новый, отдельно проверенный механизм | Факт исходника | Причинность относительно RP01 | Приоритет/проверка |
| --- | --- | --- | --- | --- |
| **N16-01** | **Вектор KNN нормализует текст и предикаты одним общим L2, поэтому текстовая длина подавляет закон** | **C**: count BPE `+1` и predicate `+2` до единственного L2 | **H/NOT_INVOKED_IN_RP01_ISOLATED_STUDENT**; риск для реального KNN consumer | **P0 для RP02**: фиксированный KNN corpus A/B, короткий probe |
| **N16-02** | **Нулевой/отрицательный вес допущенной DPO-пары превращается в положительный 0.01** | **C**: `Math.Max(0.01f, Weight)` до gradient | **NOT_DIRECT_RP01_CAUSE**: RP01 materializer явно выставляет Weight=1 | **P1**: отдельный 2-pair zero-weight gradient probe |
| **N16-03** | **DPO оценивает full-sequence Forward, генерация использует независимый KV-cache ForwardWithCache** | **C**: два пути есть; численное несовпадение **НЕ установлено** | **H**: если есть расхождение, preference gain не переносится в генерацию | **P0 cheap check**: logits parity без обучения |

Обозначения: **C** — поведение данного исходного метода подтверждено, **M** — измеренный результат указанного прогона, **H** — гипотеза, **U** — неизвестно. Точность native .NET, источники обучающих файлов и реальные Student-training эффекты *не измерялись в этом проходе*. **Нет независимого CODEX/Astra-review**; это не ACCEPT.

## N16-01 — подавление предметных признаков длиной текста, даже когда metadata правильна

**Факт исходника, не дубль «потери порядка» или «predicate_ids должен быть string[]»:**

- [`KnnLogicalUnit.Train` — локальный BPE320 и HNSW](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L121-L155) обучает локальный индекс.
- [`Vectorize` — counts +2 и один общий L2](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L300): первые `d/2` координат — **ненормализованные частоты** BPE токенов; вторые `d/2` — веса 2 для активных `predicate_ids`; затем *весь* вектор делится на одну норму. Нет отдельной tier-wise нормализации, IDF/length-normalization или законодательно калиброванной доли предикатов **в данном Unit**.
- [`ExecuteAsync`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L220-L258) использует kNN по косинусу, выбирает `hits[0]`; факт top1 уже известен, новое отличие — **ещё до поиска** закон может стать почти невидимым.

Для одного предиката без коллизий доля квадратов нормы предикатного tier:
`E_pred = 4 / (Σ_j count_j² + 4)`. Увеличение длины входа без изменения предиката уменьшает `E_pred`. Если восемь BPE-бакетов имеют по 40 токенов, `Σ count² = 12800`, тогда `E_pred=4/12804≈0.0003124` (0.03124%). Два *противоположных* закона при одинаковых токенах получают cosine `12800/12804≈0.999688`: очень уверенная близость, хотя правильные действия противоположны.

**Сконструированный численный контрпример:** training-candidate WRONG имеет точную текстовую гистограмму query, но другой predicate; candidate CORRECT имеет тот же predicate, но четыре токена перенесены между двумя текстовыми слотами. При global-L2 `cos(WRONG)≈0.999688`, `cos(CORRECT)≈0.998753` — выбран неверный ответ. При двух отдельных нормализациях с фиксированным весом tier 1/2 — `cos(WRONG)=0.5`, `cos(CORRECT)≈0.999376`. Здесь обучена **память примеров KNN, не LoRA**; это сконструированные числа, не реальные C#/ENRU оценки.

**Независимость от прежних работ:** [DIF-13](01-PRIORITIZED-DIFFERENCES.md) изучал *неупорядоченность/хеш*, [DIF-14.02](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) — потерю metadata из-за типа, [DIF-07](01-PRIORITIZED-DIFFERENCES.md) — k>1 без голосования. N16-01 остаётся даже при корректных metadata, без хеш-коллизий и с точным exhaustive cosine. Не приписывать отсутствию tier balancing **всех** моделей `TranslationKnnStore`, `TransformerEmbeddingKnnLogicalUnit` или production route.

**Историческая сверка:** E09 работал с [Roslyn SyntaxKind и иной train/validation выборкой](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md); E51A/E51B демонстрируют функциональную обучаемую память, но [E51A repeated CodeContext](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md) ограничивает claim о unseen переносе. Успех старого structural KNN не доказывает, что именно нормализация вызывала современное ухудшение. Назначить `TRANSFORMATION_DIFF` только после одинакового корпуса и consumer trace, не по названию legacy algorithm.

**Falsifier / действие:** [микроэксперимент N16-01](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md) и [исполняемый synthetic runner](16-KNN-TIER-BALANCE-PROBE.mjs). A=current 64D global-L2; B=same token/predicate counts, но separate-L2 по tier, вес каждого nonempty tier `1/√2`. Один tokenizer, одна frozen TRAIN-память, один exact cosine (HNSW только отдельная shadow-фаза), одни семантические heldouts. Не менять структуру, labels, k и source IDs одновременно. Если нет выигрыша semantic quality на source-disjoint примерах — не интегрировать tier-правку. Если выигрыша нет даже в численном counterexample — гипотеза о его наличии опровергнута.

## N16-02 — zero-weight DPO не означает zero-gradient

[`DpoTrainer.StepVectorCore` 859–913](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L859-L913) нормализует пары по `Math.Max(0.01f,batch.Weight)`; даже `Weight=0` или отрицательный ввод получает *положительную* долю градиента `objective.Derivative*vectorWeight*VectorSignal`. При одном `Weight=1` и восьми `Weight=0` суммарный вес нулевых по исходному полю пар становится `0.08/1.08≈7.4%` до `VectorSignal`. Это доказательство формулы, не доказательство, что в реальные 500 вошли нулевые веса. Значения `Weight=0` по контракту могут сознательно означать минимальное принятие, поэтому **нет оснований объявлять код багом без проверки семантики Weight/Admission**.

[`DpoVectorBatch` record](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L3864) допускает float Weight (default 1). [В RP01 `PrepareExecution` конструктор передаёт `1f`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1736-L1760). Значит **N16-02 НЕ объясняет уже опубликованные RP01 C#500/ENRU500**: текущие пары не были скопированы с Weight0. Он важен при будущем curriculum/hard-negative weighting и переносе DPO в разные под-юниты: попытка исключить недостоверный пример только весом не исключает его; quarantine/admission должны блокировать до trainer.

**Отдельный telemetry seam:** `lossSum` взвешен `scaledWeight`, а `RewardMargin/Accuracy` — только `vectorWeight`; числа оптимизируемой эффективности и публикуемой margin могут разойтись при разных `VectorSignal`. Это *разная семантика метрик*, не автоматически ошибочный градиент; для научного сравнения публиковать raw + optimization-weighted отдельно.

**Микропроверка:** freeze B0/ref/optimizer seed, legal admitted pair P(weight1) и контрастная Q(weight0), затем A=только P, B=P+Q(weight0), C=P+Q(weight1); тот же tokenizer/exact-v2/step budget; сравнить keyed parameter deltas и logits на одинаковой панели. При B≠A фактически проверен нулевой вес. Не запускать без допущенных новых пар; не мешать этому опыту measured Student gain. Принудительное `minWeight` может быть желаемым, но тогда назвать `Weight` не «exclusion mask», запретить применять для quarantine и не обещать weight0=excluded.

## N16-03 — численная parity full-sequence vs autoregressive KV cache неизвестна

**Кодовые якоря:**
- [DPO forward через `Forward(ids,training:true/false)`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1018-L1040).
- [Модель с `ForwardWithCache` и `Forward`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/03.TransformerModel/LlmModel.cs#L153-L221).
- [Attention: full mask vs cache mask + RoPE positionOffset](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L249); [TensorOps causal mask with past](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/00.TensorCore/Core/TensorOps.cs#L1367-L1453).
- [`TextGenerator.GenerateCore` prefill/decode по cache](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/09.Inference/TextGenerator.cs#L181-L244).

Формулы стараются быть эквивалентными; **исходный код не содержит здесь доказанной ошибки**. Но итог DPO objective измеряет путь full Forward, тогда как Student free-generation использует cache. Поэтому прежде чем увеличивать 500 DPO/LoRA budget надо на *одном* замороженном checkpoint сравнить: (A) full `Forward(all_ids,false)`; (B) cache prefill`(all_prefix_ids)`; (C) `ForwardWithCache` чанками 1 и 7; для каждой позиции сравнить per-vocab logits или log-softmax после тех же q/v-LoRA. Порог эквивалентности зафиксировать до результатов, например `atol=1e-4, rtol=1e-4` для CPU FP32 (допускается корректировка только *до* blinded run под известную математику). Отдельно LoRA adapter on/off, совпадение входного BPE/позиции, и детерминированная генерация с `temperature=0`. **Если эквивалентность выдержана** — не записывать `KV` как причину падения; дальше проверять B0, данные, SFT prerequisite и loss→decoded utility.

Это отличается от [DIF-06/39](01-PRIORITIZED-DIFFERENCES.md) о выборе argmax и форматировании prompt: проверяет конкретную **внутримодельную численную семантику** двух вычислительных путей при *одинаковых* токенах и состоянии, а не качество ответов.

## Что НЕ является новой найденной причиной

| Уже исследованный участок | Вердикт в этом проходе |
| --- | --- |
| Cold-start random B0, rank8 q/v, 629 BPE, 50×batch10 | Реальный приоритет для **isolated RP01 Student**; уже рассмотрено в [RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). |
| DPO exact-v2 vs legacy mask/Compact480 | RP01 использует exact-v2; перенос legacy defect без invocation запрещён. |
| KNN ExampleId collision, predicate_ids type-only, domainTag не scope, HNSW RNG | Уже подтверждены и имеют отдельные фальсификаторы в [14](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md). |
| Исторический E09/E34 «прошёл», E51A «100%» | Не считать paired, source-disjoint доказательством neural generalization: [09](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [11](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md), [12](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md). |
| Полный checkpoint restore | Опубликованные результаты restore положительны; ordinary consumer adoption отдельно не доказан. |

## Контракт авторства и оценки при исполнении

1. **Роль человека в выборе нового учебного случая выполняет CODEX с моделью Astra 6 UltraCode**: выбирает ошибку Student и учебный recipient на основании доменного закона/контекста.
2. **Обе стороны DPO (chosen И rejected) генерирует разрешённая внешняя открытая китайская модель**, с записанными model/version/provider/license/output hashes. Student не является автоматически автором rejected. Это не исторический `DeterministicGenerator` simulation importer.
3. **Независимую от генератора оценку качества точной DPO-пары выполняет CODEX с моделью Astra 6 UltraCode** (VALID/TIE/BOTH_BAD/UNJUDGEABLE, exact prompt/bytes/доменный закон). Отдельно оценивается **обученный Student** на blind heldout, не той же оценкой пары.
4. Здесь **ни китайская модель, ни CODEX/Astra не вызывались**. KNN synthetic fixture не является DPO-парой и этих вызовов не требует. Не выдавать архитектурный план за fresh научный PASS.

## Порядок без новой Windows CI-эпопеи

- **T0 (дёшево, read-only):** математический counterexample + exact vector counts; затем cache/logits parity на имеющемся frozen Student.
- **T1 (короткий KNN эксперимент):** [N16-01 microprobe](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md), source-disjoint real ENRU/C# holdout и negative-control labels; **никаких новых 500 пар**.
- **T2 (только если T1 полезен):** native `KnnLogicalUnit` shadow-variant с тем же BPE320, одним индексом на arm, фиксированными запросами; померить true end-user output и latency. Parent frozen, rollback при law regression.
- **T3 (если T0 показывает проблему Student):** одна исправленная семантическая ось, крошечный Student/LoRA experiment; без изменения исходных datasets, scorecards, checkpoints или DPO producer contracts.
- **T4:** убедиться, что production/ordinary consumer действительно вызывает улучшенный Unit/Student и cold restores proper state; только после этого говорить об RP01 causal recovery.

**Применённые скилы (фактически прочитаны из `docs/skills`):** `human-readable-architecture-audit`, `architectural-decision-testing`, `phase-planning`, `plan-review`, `generated-code-review`, `prompt-patch-authoring`, `experiment-design`, `claim-as-hypothesis`, `learning-path-audit`. Проверки: exact source pin, consumer attribution, one factor per arm, historical metric honesty, dynamic-graph four roles, explicit failure/unknown status. Self-review этого документа не приравнивается к независимому review.

**Исторический статус редакции16.10:** N16-01 mathematical counterexample CONFIRMED (не native quality gain), N16-02 code semantics CONFIRMED (not RP01 cause on known batches), N16-03 UNVERIFIED. Native experiment/Student learning/pair review/ordinary product gain **NOT_RUN** на том срезе. Последующие P114 и product zero filter имеют отдельный актуальный статус в начале карточки.
