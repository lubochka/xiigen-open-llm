# MICRO-BPE-RETRAIN-01 — проверка, забывает ли KNN старые уроки при пересборке BPE

**16.10.2026 · DRAFT / NOT_RUN / INDEPENDENT_CODEX_REVIEW_PENDING.** Первичный исходник и гипотеза: [12 — BPE + KNN source audit](12-bpe-retrain-identity-and-consumer-causal-gap.md), [21 — отдельный подробный документ planning](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться").

**Вопрос человеку:** после добавления одного правильного примера KNN перестал правильно отвечать на ранее выученные запросы — это само переобучение словаря BPE, конкуренция соседей или случайность графа HNSW? Не принимать изменение hash за изменение качества.

## Поручение и жёсткие границы

- `CURRENT TASK`: один small Tier1 paired experiment на `KnnLogicalUnit<string,string>`: порядковый дрейф и дрейф при росте корпуса, exact cosine до native HNSW shadow. Реальные нативные ответы и семантическая оценка обязательны для утверждения о качестве.
- `NOT THE TASK`: не исправлять DPO/LoRA, не перезаписывать frozen C#500/ENRU500, не проводить 500 DPO, Windows CI, не менять исходный код Student/ProductHost, не получать новые baseline из несовпадающих validation subsets.
- `EVIDENCE-ONLY`: исторические E07/E09/E34/E40V2, tool micro-KNN-order, уже имеющиеся источники и [proxy BPE first merge](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-MICRO-BPE-FIRST-MERGE-PROXY.mjs> "Исторический ref llm_plan3; локальный файл может отличаться"). Никакая старая метрика не считается свежим испытанием.
- `required_skills_and_gates`: `architect-behavior-classifier-SKILL.md`; `human-readable-architecture-audit-SKILL.md`; `trainable-architecture-decision-SKILL.md`; `phase-planning-SKILL.md`; `reference-inventory-and-source-map-SKILL.md`; `plan-review-SKILL.md`; `generated-code-review-SKILL.md`; `experiment-design-SKILL.md`; `claim-as-hypothesis-SKILL.md`; `prompt-patch-authoring-SKILL.md`; `conversation-intent-and-self-reflection-SKILL.md`. Все из `docs/skills/`. Один bottleneck — один patch.
- `context_files_to_read`: `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, `01.Tokenizer/BpeTokenizer.cs`, `01.Tokenizer/RegexSplitter.cs`, `06.VectorSearch/HnswIndex.cs`, документы [12](12-bpe-retrain-identity-and-consumer-causal-gap.md), [N21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться") и [11A](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md> "Исторический ref llm_plan3; локальный файл может отличаться").
- `implementation_or_artifact_details`: новый изолированный console harness, не production-class patch. Результаты записывать в уникальный run folder `.kiro/xiigen-slices/work/results/MICRO-BPE-RETRAIN-01/<run-id>/`. Пин source SHA, fixture SHA, recipe SHA, source-test runner SHA до начала пробы.
- `original_task_requirements_to_cover`: BPE/embedding, actual KNN learned output, re-train memory, code vs historical learning, cold identity and true consumer, tiny causal A/B, CODEX judge. НЕ выдавать verdict на все 44 DIF.
- `mental_simulation_required`: проверить, нет ли heldout leakage в tokenizer training, не изменилось ли label mapping, не мог ли один фактор быть графом ANN, и действительно ли изменился финальный ответ Unit.
- `good_example`: замороженный train/heldout и labels, same law, exact cosine, разные только corpus order/BPE, independent law grader после ответа.
- `bad_example`: A/B оба пересобирают random HNSW, правят одновременно словарь и scoring, затем объявляют causality; или подают ожидаемые predicates в metadata запроса.
- `allowed_files`: изолированный tools/harness, новые recipe/fixture/results, отдельное post-run evidence-addendum. `forbidden_files`: production model/loRA/DPO classes, historical results, frozen datasets/checkpoints, файлы других агентов.
- `allowed_commands`: read-only inspection Git, `dotnet --info`, `dotnet build` и `dotnet run` для названного изолированного проекта, проверка hash/exit/status. `forbidden_commands`: merge/rebase/force-push и любые правки production source, повторные Windows GitHub Actions, подстройка экзамена после открытия результатов.
- `verification_required`: CODEX/Astra 6 UltraCode отдельно утверждает pre-registered rubric; native outputs и ошибки должны иметь точные командные/assembly SHA receipts; независимый review по exact candidate before ACCEPT.
- `return_packet_schema`: `claim_scope, evidence_scope, count_claimed, independent_evidence_count, subagent_id_or_NONE, downgrade_needed, source_sha, fixture_sha, commands, exit_codes, per-case outputs, correct_count, false_safe, paired_diffs, verdict`. Отдельно перечислить исполнения NOT_RUN и недоступные модели.

## Frozen recipe

**ExperimentId:** `MICRO-BPE-RETRAIN-01`, **Tier:** 1, **Metric:** предметная правильность на 16 source-disjoint heldout и число критических ошибок, дополнительно recall@1/cosine rank и drift token IDs. Выбор 8–16 train examples и независимого закона осуществляет CODEX/Astra 6 UltraCode **до** результатов. Ни один label или source text heldout не используется в tokenizer training, векторе или поиске. `k=1` для exact-части, `vectorDimensions=64`, `textSelector=x=>x`, пустые oracle predicates. Для actual Unit shadow оставить `k=3` отдельно.

| Arm | Tokenizer / data | Меняется относительно базы | Зачем |
| --- | --- | --- | --- |
| A baseline | train T01..T08 порядок P0; собственный BPE320 | — | исходная learned memory |
| B reorder | ровно T01..T08 с теми же IDs/Outputs в заранее выбранном обратном порядке P1; BPE320 re-fit | только порядок учебных записей | N21-ORDER |
| C grow | T01..T08 + один законный unrelated T09, BPE320 re-fit | добавление урока и tokenizer | наблюдаемое забывание |
| D grow/frozen | T01..T09, но **BPE с A**, новый пример индексируется в прежнем пространстве | тот же дополнительный урок без tokenizer re-fit | отделить конкуренцию соседей от re-fit |
| E label shuffle | exact hit IDs/ranks как C, outputs переставлены в resolution layer | только метки | проверить обучаемую, а не чисто deterministic пользу |

Во всех A/B/C/D пересчитать именно **exact cosine**, фиксировать общий id tie-break, исключая случайный HNSW. После causal оценки провести shadow через реальный `KnnLogicalUnit.Train → ExecuteAsync`, записав фактический другой ANN hit pool; не подменять exact и ANN.

## L0 — подтвердить или опровергнуть изменение признаков

1. Зафиксировать corpus SHA, order, stable IDs и labels; проверить train/heldout source-disjoint.
2. Для A/B/C/D вызвать **production** `BpeTokenizer.Train(vocabSize:320)`, сохранить `ComputeStateSha256`, хотя бы первые 64 merges, token IDs всех прежних train/heldout строк.
3. Построить реальные 64D token-tier + predicate-tier vectors на одинаковом `KnnLogicalUnit.Vectorize` (через безопасный test-only access/reflection либо узкий extracted test helper, не новый формульный surrogate). Для C/D сравнить координаты старых уроков до cosine.
4. Статус `BPE_DRIFT_CONFIRMED` только если совпадающие старые тексты дают разные токены или 64D coords; если только artifacts SHA разные, но relevant feature не изменился — `NO_FEATURE_DRIFT_ON_PANEL`.

Дополнительный **отдельный** L0-OS: один и тот же corpus и порядок с join(LF) против join(CRLF); не смешивать с A/B/C/D.

## L1 — отличить механизм от вреда для обучения

Запустить одни 16 blinded queries для A/B/C/D exact. Зафиксировать: `query_id, source_family, user_request_sha, tokenizer_sha, old/new token_ids_sha, topK hit IDs/scores, selected training_output, semantic correct, false_safe, latency, actual recipient`. Экзаменатор получает выбранный текст и rubric только после inference.

Порог **pilot-сигнала**, не доказательства generalization: минимум 4/16 formerly-correct queries стали wrong в B либо C, при этом D возвращает существенную долю этих случаев в корректное состояние, без label leakage. Если token IDs изменились, а accuracy нет — `REPRESENTATION_DRIFT_ONLY`. Если все изменения top1 связаны только со случайным HNSW graph — `ANN_CONFOUNDED`. Если нет причинного изменения — `NOT_SUPPORTED_ON_PANEL`. Для слабого/малого эффекта `INCONCLUSIVE`, возможен Tier2 со стратифицированными доменами; нельзя навсегда закрыть полезный алгоритм как FAILED из-за маленького Tier1.

## L2 — проверить получателя, применение и восстановление

Узким readback сверить tokenizer identity, vector/index identity и stored output map между native memory и новым process. Если generic KNN не имеет доступного cold restore API, честный статус `RESTORE_NOT_SUPPORTED`, не `RESTORE_FAILED`. Прямую генерацию `TextGenerator` не объявлять использованием KNN: требуется trace `unit_id → actual_execute → chosen_hit → selected_answer → final_user_output`.

## Роли и финальный verdict

**CODEX с Astra 6 UltraCode исполняет человеческую роль выбора учебной задачи и оценки качества DPO; внешняя разрешённая открытая китайская модель создаёт ОБЕ стороны chosen и rejected каждой новой DPO-пары.** В данном KNN Tier1 DPO-пары не создаются, поэтому внешняя модель не вызывается. CODEX/Astra также утверждает law-gold и независимо оценивает качество финального ответа Unit/Student. Нельзя выдавать написание документа за фактический CODEX запуск. Разрешённые выводы: `MECHANISM_CONFIRMED / REPRESENTATION_DRIFT_ONLY / QUALITY_PILOT_SIGNAL / ANN_CONFOUNDED / INCONCLUSIVE / DATA / TOOL`, с точно указанными native-versus-proxy и readback evidence.
