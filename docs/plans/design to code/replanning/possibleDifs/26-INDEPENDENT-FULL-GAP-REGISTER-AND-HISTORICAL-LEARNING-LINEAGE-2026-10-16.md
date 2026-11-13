# 26 — Сводка пробелов RP01 по коду, RAG и позитивным экспериментам: что действительно могло потеряться

**16 октября 2026 | EVIDENCE-LEDGER / SOURCE-CONFIRMED VS HYPOTHESIS SEPARATED / NEW MECHANISMS M26 / NOT A NATIVE LEARNING RUN.**  
Planning: lubochka/xiigen-planning@llm_plan3; прочитанный planning HEAD до наших файлов ee20a022676895e89b42a9ea4079d79ae5b59ab0. Core: lubochka/llm_mvp_core@codex/dpo-implementation, прямое чтение C# и исторических run JSON. Дополнение к уже существующим 117 файлам possibleDifs, **не переписывает** находки или выводы других агентов. Core docs/plans/replanning-possible-diffs — READ ONLY.

## Короткий вывод

Снижение качества нельзя объяснить одним «плохим DPO». В сравнении под одним названием «обучение» оказываются как минимум:

1. обучение параметров Student через DPO/LoRA;
2. обучение дискретных отношений и предпочтений Unit / KNN / словарной памяти;
3. domain-specific детерминистический или гибридный алгоритм, который выбирает, какое выученное правило применить;
4. learning orchestration / re-index / replay / восстановление и передача состояния;
5. научная оценка ответа **обычного потребителя**, а не наличие файла/градиента или PASS.

В [текущем RP01 README](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) для B0 размер vocabulary=629, Student случайно инициализирован, LoRA действительно меняется; при этом published 500-парные C# / EN→RU semantic free-response probes дали соответственно 0/3→0/3 и 0/4→0/4. Отсюда **не следует**, что вся XIIGEN потеряла способность обучаться: испытывался другой learner/consumer, чем многие исторические успешные retrieval-Units. Но не следует и обратное — нельзя считать старый успешный KNN report доказательством обобщения текущего Student.

## Основные первичные свидетельства успешных learning механизмов, а не только планов

| Первичный архив | Сохранённый результат | Чему именно он свидетельствует и ограничение |
| --- | --- | --- |
| [E14 Tier2, 30.04 JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json) | trainFiles16 / valFiles4; top1 baseline 0.75, KNN 1.00; PASSED | Малый положительный routing-matching опыт, 4 проверки; не доказательство широкой generalization |
| [E14 Tier3, 30.04 JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-002.json) | trainFiles220 / valFiles56; 0.8393→0.875; PASSED=false; label balance .0179 | При расширении состав классов стал чрезвычайно неравновесным. Иное Tier/население, не идентичный matched transfer |
| [E26, 01.05 JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) | disambiguation 0.40→1.00, testCases15, hard6, PASSED | Полезна предметная контекстная функция выбора значения на данном наборе; current Executor word-map использует другой key, описано [E26 crosswalk](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) |
| [E43 replay, 01.05 JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | retention baseline 0.3226, replay 0.3978, oldPairs465/newPairs414/replay177 | Реальная отчетная прибавка *ретеншна* в прежнем bigram/replay эксперименте, не доказательство DPO или междоменных Unit transfer |
| [E09 Tier2, 01.05 JSON](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | baseline .2413, enhanced .3507, PASSED | Исторический positive retrieval report, **но** [код E09](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L78) использует несовпадающие baseline/treatment validation subsets — causal gain без matched rerun не доказан |
| [E13B heldout archive, 07.05](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/heldout-2026-05-07-075333/e13b-heldout-validation.md) | train correct100%, heldout matched0%, abstained100%, false positives0% | **Успех памяти известных формул и безопасного отказа**; НЕ 100% правильных completions новых выражений |

Из этого следует не «все ранние эксперименты ошибочны», а «сохранён положительный экспериментальный сигнал разных, часто узких функций». Его можно использовать как hypothesis donor только с подтверждённой схожей входной схемой, same data/split и настоящим маршрутом потребителя.

## Генеалогия матрица → KNN: не заменить воспоминание догадкой

**Проверенные линии из соседних агентов:**

- [12 — transfer lineage](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) разделяет E09 structural vectors, E14 context-action KNN, E12 numerical matrix optimizations и нынешние HNSW/hashed-BPE/transformer embeddings. Отрицательный ранний matrix experiment НЕ исключает успех полезной функции как KNN scorer/routing rule.
- [Core 10 — historical function transfer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md) документирует E40V2 learned trust feature vs fixed weights, E38V2 skill scorer existence vs adoption и E35 dependency logic; сохранение формулы не равно подключению обучаемого consumer.
- [Core 09 — sibling branches](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/09-domain-functions-and-sibling-branches.md): в language_translate имеется неперенесённый morphology index; E13B превратился из generic KNN в specialized expression function с сохранённым известным/неизвестным поведением; старый vs новый recall corpus loading разный.
- Ветви, которые следует удержать в исследовании: DesignToCode, external_domains, language_translate, orchestrator-extensions, codex/dpo-implementation. Ни один найденный E13B/E09/E14 commit **пока не доказан точным тем самым donor**, о котором сообщила Люба: source-lineage status = OPEN.

**Следующая дешёвая evidence-задача donor search:** фиксировать для каждого кандидата (а) старую матричную формулу/веса/признаки, (б) новый KNN helper/score/caller с diff/commit, (в) одинаковые исходные train/validation families и метрики до/после, (г) какой production caller реально потребляет код. Без четырёх связей не публиковать «мы нашли тот самый эксперимент».

## Сквозной реестр направлений потери качества, с владельцами существующих документов

Сокращения: **S** — конкретный RP01 Student, **U** — отдельный обучаемый Unit, **H** — история experiments, **C** — обычный consumer. P0/P1 здесь — приоритет проверки, НЕ установленная тяжесть product incident.

| Контур | Evidence/current difference | Открытый causal вопрос | Самое дешёвое различающее действие |
| --- | --- | --- | --- |
| S P0: неверный learner | Случайная B0, vocabulary629, 50 updates и реальное изменение q/v LoRA, free response не вырос | Может ли DPO без базовой компетенции породить навык? | existing [REC02 tiny gradient](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md) затем [REC03 warm base](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC03-CAPABILITY.md), не ещё 500 |
| S P0: DPO objective | Exact-v2 response masks/summed log-prob; выбранное/отклонённое может потерять контраст на handoff | Правильный ли gradient действительно действует на семантически различающие token IDs? | [02-DPO-DATA](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [16 raw-to-visible pair](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md): 4 пары, модельные ID, finite-difference |
| S P1: LoRA design | Обучены q/v, rank8, alpha16 и frozen base; update ≠ meaningful decode | Ограничение capacity/target, не incorrect optimizer? | [03-LORA](03-LORA-TOKENIZATION-VECTOR-SPACE.md) matched 8–32 tiny overfit; разные targets отдельным arm |
| S P1: generation | Decoder greedy/stop/cache и historical EOS ID overlap | Изменились log-probs, но не выбранный первый токен или stop | [10 decoder](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md); chosen token position, stop reason on same learned checkpoint |
| S+U P1: tokenizer/vector identity | Learned BPE vocabulary/coordinate drift; domain semantic vectors иногда scalar weight вместо semantic geometry | Несопоставимы train/infer coordinates или feature slots | [03 LoRA+vector](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [BPE restore](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), exact fingerprints |
| U+C P0: lost actual consumer | Исторический KNN/exact/phrase/Unit consumer ≠ RP01 TextGenerator alone | Потерян именно полезный предметный получатель, а не градиент? | [REC04 HYBRID](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC04-HYBRID.md): same-input Student / Unit / combined ablation |
| U P1: KNN candidate set | first-hit generic KNN; hashed order-insensitive vectors; HNSW identity/ANN recall | Правильный сосед отсутствует или выбран неверный? | [04 KNN hybrid](04-KNN-HYBRID-RETRIEVAL.md), [14 duplicate+scope](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md): exact vs ANN, top1 vs law-aware rank |
| U P1: scoring from historical functions | E34 adaptive blend/E40V2 feature scorer в новых callers отличаются от original learned logic | Сохранили алгоритм, но потеряли learned coefficients? | [12 matrix KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [core function transfer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md) |
| U P1: lost real learning | DPO row → 3 typed unit lessons; SemanticTokenizer self-label; rejected parts могут не использоваться; UnitId can silently skip | Получатель реально изменился или только RowsTrained/version? | [N21 structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N17 dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), actual caller/learned digest |
| **U NEW N26-LANG** | _signatureCorrections key = normalizedRaw, SourceLang ignored; 6 language adapters share tokenizer | Межъязыковой last-write signature влияет на проверку/ответ? | [N26](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) M26-A/B/C |
| **U NEW N26-TAGS** | Base tokenizer includes RU RuleTags in prefix/structure, learned correction builders drop them | Потерян KNN address, или exact-source mask makes it irrelevant? | [MICRO-26](26-MICRO-LANGUAGE-KEY-AND-RULETAG-CAUSAL-EXPERIMENT-2026-10-16.md) real trace + conditional A/B |
| U P1: domain/semantic law | SourceLang=RU vs ru; EN POS coarse, prevConcept crosses clauses, contextual verifier ignores context | False ACCEPT/REJECT, law leaks across examples | [23 language](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [N21](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) |
| U P1: training admission | Novelty filter misses recipient/negation/symbol changes; malformed rejected chosen collisions | Lawful corrected lesson never reaches trainer | [N21](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [21 symbol-blind](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md) |
| C P1: KNN/veto/output arbitration | Novel candidate can reorder max-normalized BM25, weak patterns boost, shared verifier accumulates law | Новая память ухудшает старое решение | [20 nonlocal fusion](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md), [23 verifier conflict](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) |
| C P1: restore | ExportAsync metadata-only vs ExportCheckpoint actual Unit state; manifest-linked memory vs history; rebuilt ANN | Cold ordinary consumer sees same learned keys/vectors/candidates? | [06 restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [core 09 history](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/09-domain-functions-and-sibling-branches.md) |
| S+U P1: continual/replay | Old E43 retained some bigram skill; current RP01 has zero proven baseline C# free competence | Old skill forgotten or never existed? | [07 replay](07-CONTINUAL-REPLAY.md): no-replay vs bounded lawful replay ONLY after first measured skill |
| H P0: inaccurate proof | E09/E34 unmatched denominators; E13B absent unseen completions; false train counters | «Раньше работало» относится к тому же input/goal/consumer? | [01 historical reconciliation](01-EXPERIMENT-BASELINE-RECONCILIATION.md), [11 positive branches](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md); same-input blinded panel |

**Не считать все 17 направлений установленными багами.** Сюда входят source facts, исторические несопоставимости и проверяемые предположения. 117 уже существовавших документов могут содержать другие специфические подварианты; эта карта не заменяет exhaustive raw-file audit каждого из них.

## RAG / source hierarchy: никаких ложных гарантий

- [WorkLeft Master RAG](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) и [RAG Usage Guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) — **карта первоисточников**, а не подтверждение уже успешного обучения.
- [Original Task](../../original%20task.md) формулирует четыре исходные роли: trainable typed ILearningUnit many→many, ILearningFlow, асинхронный ILearningOrchestrator, higher-order ILearningDynamicGraph. Появление interface/class со знакомым именем не подтверждает семантическое исполнение.
- [Стратегия R01–R25→code slices](../R01-R25-TO-CODE-SLICES-STRATEGY.md) запрещает считать 25 research runs 25 реализациями и различает SOURCE_TRACED, CODE_TRACED, PRODUCT_IMPLEMENTED, SCIENTIFICALLY_EXAMINED, ADOPTED_AND_REVIEWED. Наш новый M26 на уровне CODE_TRACED/experiment proposed.
- Разрешение на DPO-корпус, independent chosen/rejected judgment и real external teacher provenance **не подменяются** synthetic fixtures M26.

## Одна следующая работа, чтобы не повторить потерю месяцев

**Приоритет 1 (дёшево):** M26-A/B local native source-level checks и подтверждение, что critical consumer действительно доходит до исправляемого участка. Если нет — остановиться на gap report, не запускать quality A/B, не объявлять correction.

**Приоритет 2 (quality):** на той же малой переводческой панели M26-C с замороженными обученными Unit, source-language-scoped key отдельно от RuleTag parity. Измерять actual outputs: correct / false accepted / false rejected / abstain и exact source/kNN/verifier branch. 12 независимых cases на первом smoke-signal уровне — не доказательство устойчивого generalization.

**Приоритет 3 (независимый RP01):** REC02 tiny Student overfit и REC03 baseline ability, чтобы решить принципиальную проблему randomly initialized B0. После положительного meaningful free response — REC04 same-input Unit/Student ablation и только потом broad replay/multi-domain budget.

**Отдельная evidence очередь (без блокировки RP01):** donor commit lookup matrix→KNN по старым веткам и saved run files; не переписывать чужие auditor документы, не заменять точное свидетельство красивой схемой.

## Review verdict и выходные статусы

- Architect: различены S/U/H/C; реальный consumer адрес нужен до решения о патче.
- Code reviewer: новые N26-LANG и N26-TAGS привязаны к конкретным C# чтениям, но полная независимая проверка отсутствует.
- Planning reviewer: fixture/control arms меняют ОДНО условие за раз, не требуют Windows Actions и новой дорогой neural model.
- Prompt engineer: synthetic diagnostics помечены не-DPO; blinded gold и source-family separation обязательны до качества.
- Human communication: кодовые seam доказаны, качество и причинность RP01 НЕ доказаны.

**Overall: DRAFT / NEW_SOURCE_SEAMS_FOUND / NATIVE_MICRO_PROBE_PENDING / INDEPENDENT_CODEX_ASTRA_REVIEW_PENDING / NO_PROTECTED_CORE_DOC_EDITS.**
