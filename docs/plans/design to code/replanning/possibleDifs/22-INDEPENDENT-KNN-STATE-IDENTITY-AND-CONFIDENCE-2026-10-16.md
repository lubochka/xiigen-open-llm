# 22 — Ещё два независимых рисковых шва: identity обученной KNN-памяти и неподтверждённая confidence authority

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Полный объект памяти вместо counts

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

P128 добавил конкретные свидетели: пустая TopicSourceMap, заменённые KNN target/lineage и переставленный order могут пройти проверку keys/counts. Сравнивать actual state с независимым producer snapshot, а не ожидаемый журнал с собой. Честный набор принимается; каждую структурно допустимую порчу довести через native Load до того же comparer. Полный hash полезен, только если сравнивается и предикат входит в finalizer. Обычный consumer проверяется отдельно от helper. Confidence не становится доверенным только от сохранения; без одновременно верных и неверных ответов его разделяющая способность остаётся NOT_MEASURABLE.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P117-translation-direction-memory.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

**16.10.2026 | DRAFT / CODE-VERIFIED SEAMS / QUALITY NOT RUN / CODEX-ASTRA REVIEW NOT RUN.** Core pin: [83590a462b2f8aedc9d3ab4cb775193eb9c25ce5](https://github.com/lubochka/llm_mvp_core/tree/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5). Подробный контрфакт переобучения BPE и протокол — [21](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md). Не объявлять эти соседние Unit-пути причиной RP01 Student DPO 0/3 и 0/4.

## N22-A · P1 · Generic KNN manifest удостоверяет форму обучения, но НЕ идентичность выученного словаря и output-map

**Прямые улики:**
- [`LogicalKnnModelManifest`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L17-L29) содержит UnitId, роль, домен, TrainingExampleCount, vectorDimensions, k, confidenceFloor, время, allocation, имена классов tokenizer/index и ingredient IDs. В этом контракте нет SHA полного BPE vocab/merges, SHA indexed vectors, SHA labelled examples или versioned output-map.
- [`Train` 118–153](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L118-L153) создаёт BPE320 и HNSW8 заново и помещает примеры в dictionary. [Manifest construction, 158–180](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L158-L180) вычисляет количество и метаописание, **не криптографическую идентичность learned state**.
- У [`BpeTokenizer.ComputeStateSha256`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/BpeTokenizer.cs#L112-L124) уже есть нужный механизм fingerprint, но этот generic KNN manifest им не пользуется.

**Вывод C:** два manifest с одинаковыми логическими полями идентичности и количеством строк могут соответствовать различным BPE merges, индексам и меткам. Время/allocations остаются диагностическими числами, не доказывают равенства состояния. Это **недостающий инвариант provenance/restore**, а не прямое доказательство неверного ответа. Отдельный [N16 encoder/index SHA](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) касается другого `TransformerEmbeddingKnnLogicalUnit` с динамическим embedding. Здесь fingerprint нужен **generic static KNN при самом Train/retrain**.

**MICRO-IDENTITY-01:** построить A/B с теми же `UnitId/role/domain`, 8 уникальными train rows с одинаковым набором, но другим порядком. Проверить все поля манифеста, кроме nondeterministic времени/alloc, сравнить отдельно `tokenizer SHA`, `labelled examples SHA`, `index vectors SHA`, `ordered train fingerprint` и 8 blind query outputs. Получили идентичные логические manifest + различные state hashes → `METADATA_NOT_STATE_IDENTITY` (даже если ответы совпали). Улучшение качества не заявлять без paired heldout. Для реального restore дополнительно нужны frozen tokenizer+index+mapping и проверка соответствия, а не только manifest.

**Минимальный design guard (предложение, НЕ кодовое изменение):** версионированный `KnnLearnedStateIdentity` из хешей канонического tokenizer, training identities+labels, vectorizer spec/dimensions и index+output map; сериализовать и проверять при serving adoption. Не путать этот feature-design с доказанным дефектом сохранения checkpoint RP01 (для RP01 LoRA reopen уже был положительным).

## N22-B · P1 conditional · Transformer KNN выдаёт confidence даже при заведомо плохом соответствии

Это другая, самостоятельная реализация, не generic KNN. [`TransformerEmbeddingKnnLogicalUnit.ExecuteAsync` 166–210](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L166-L210) возвращает победителя и задаёт `confidence = Round(Clamp(0.50 + fusedScore*0.49, 0, 0.99),2)`; `Warnings=[]`. [`ScoreCandidates` 215–244](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L215-L244) получает `fused` как сумму неотрицательных terms: normalized cosine после `(sim+1)/2`, нормализованного BM25 и совпадения паттерна. Значит для непустого candidate set `fused≥0`, следовательно итоговая confidence **не меньше `0.50` (после округления)**, даже если ни один сосед не отвечает предметному закону запроса.

Это не calibration probability: score является монотонным преобразованием взаимного ранга ближайших примеров и score fusion, а не `P(law_correct|request)`. Нет потребительского доменного veto/abstain в показанном method. Ранний [DIF-404 confidence != P(correct)](04-KNN-HYBRID-RETRIEVAL.md) описывает **generic** KnnUnit cosine + warning; здесь дополнительный source-специфический случай: **TransformerKNN fusion всегда создаёт baseline confidence и не добавляет warnings**. Исследовать actual outer caller и его trust threshold, прежде чем называть опасным user-facing дефектом.

**MICRO-CONFIDENCE-01:** зафиксировать 12 blind queries: 6 in-domain и 6 out-of-domain/negation/order/scope (семантический закон известен scorer'у **только после** выбора, не в индексных metadata). Заморозить корпус, encoder, candidates и `fusedScore`, замерить `confidence`, law correctness, ошибочные ответы при confidence ≥0.50/0.70/0.90, отдельную coverage. Treatment: shadow-selective abstain threshold/empirical calibration, настроенный ТОЛЬКО на dev panel; на слепой панели сравнить selective risk при равной coverage. Если все wrong имеют confidence ниже действительного threshold и caller законно abstains, причинный риск не установлен. Нельзя объявлять простое снижение coverage улучшением генеративного знания.

## Приоритет в отношении RP01

1. Сначала различающий Student-exam: warm language base vs random B0, exact DPO quality, training/free generation, что уже отражено DIF-01/09/11.
2. Параллельно минимальный [MICRO-BPE-RETRAIN-01](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) для восстановленного generic KNN: *могли ли менять старые ответы при добавлении урока?*
3. Затем N22-A fingerprint/adoption и N22-B confidence **только в реально вызывающем этот Unit consumer**. Сначала trace, потом изменение алгоритма.
4. Матрица→функция KNN: сохраняем открытый `USER-REPORTED / EXACT_COMMIT_UNCONFIRMED`; ни один из N21/N22 не устанавливает точную историческую миграцию. См. [12-MATRIX](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) и [core provenance E40](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md).

## Права, роли, доказательства, отзыв

Человеческий выбор учебной задачи, получателя и приемлемости урока выполняет **CODEX / Astra 6 UltraCode**. Если для дальнейшего опыта нужны DPO, оба `chosen` и `rejected` генерирует разрешённая внешняя открытая китайская модель, а их качество независимо оценивает **CODEX / Astra 6 UltraCode**. CODEX отдельно оценивает смысловое качество Student/Unit на hidden/source-disjoint задачах. Выше — **документарная проверка кода**, а не выполненный native learning-exam или независимая приёмка. Рекомендации DRAFT до такого исполнения и review.
