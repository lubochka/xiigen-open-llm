> **CURRENT R23 EXECUTOR RULE (V2, 2026-10-13):** The earlier A–H entries below are an inventory/history, NOT complete executor prompts. Use the **fully specified V2 packets in §V2 at the end of this file**. No global A–H completion gate before the first lawful R18/E1 lesson. Documentation is not learning evidence.

# R23 repair — executable learning-first work orders

**2026-10-13 · supersedes the claim that old R23-A..D were fully model-ready.**
The old four packets remain historical planning evidence. They were useful, but they did not satisfy the current sub-agent work-order completeness gate and bundled too many independent learning questions.

## Governing rule

Learning quality is **not accepted by test coverage**. Deterministic checks may prove state integrity, replay prevention, type/domain-law enforcement, serialization, rollback and native integration. They do **not** prove that a non-deterministic learner improved. Learning claims require matched bulk-to-bulk examination on independent source families, with before/after quality, time, memory and cost.

Every task below must read the exact files it cites, not only filenames. For WorkLeft/RAG discovery start from:
- `09.09.26WorkLeft-RAG-MASTER-INDEX.md`
- `09.09.26WorkLeft-RAG-00-usage-guide.md`
- relevant RAG among `RAG-01-dpo-legal-build`, `RAG-02-design-to-code`, `RAG-03-merge-and-branches`, `RAG-04-vs-vscode-continue`
- `09.09.26WorkLeft-PROMPTS.md`
- all relevant `09.09.26WorkLeft-TASK-PROMPTS-*.md`
- relevant `09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `RESEARCH-01-training-quality-speed-accuracy-program.md`, `RESEARCH-03-dpo-algorithm-conformance.md`
- `original task.md`, `Алгоритм создания ДПО.md`, `S05-S10-learning-role-audit-2026-10-08.md`
- R01-R20 results plus `previous rounds thinking results.md` and `PARALLEL-R01-R03-JOIN.md`.

Required skill stack for **every** child:
SK-SESSION: planning-session-startup + SESSION-START-PROMPT + AGENTS
SK-INTENT: architect-behavior-classifier
SK-PLAN: authority-requirement-binding + reference-inventory-and-source-map + phase-planning
SK-REVIEW: plan-review + goal-delivery-completeness + claim-verification + audit-protocol + specificity-calibration + output-skepticism + reconnaissance-gate
SK-ARCH: trainable-architecture-decision + human-readable-architecture-audit + no-product-decisions + output-readability-gate
SK-CODE where code is touched: code-examination + implementation-doc-code-refs + data-flow-integrity
SK-LEARNING: experiment-design + learning-loop-closure; add client-adaptive-learning-loop for feedback tasks and prompt-patch-authoring for DPO/prompt tasks.

Each child must return:
`claim_scope; evidence_scope; count_claimed; independent_evidence_count; subagent_id_or_NONE; downgrade_needed`, exact source paths + blob/SHA/read depth, findings, solved architecture block, falsifiers, unknowns, next owner.

---

## R23-A — whole-PC resource envelope

**CURRENT TASK:** measure/derive the resource envelope of the *actual* first learning path, including base/reference/trial, LoRA, optimizer, activations, tokenizer/vector stores, kNN/graph, snapshots, evaluator and failed attempts.

**NOT THE TASK:** no model training, no provider calls, no Windows Actions, no invented hardware numbers, no deletion of rollback state.

**EVIDENCE-ONLY:** historical PC specs and old CI durations are hints only until fresh runtime evidence.

**context_files_to_read:** R18 work order; R22 catalog; R23 lifecycle; R09 findings; optimizer/checkpoint/artifact-layout code; WorkLeft RAG master + RAG-01/RAG-02; WorkLeft RESEARCH-01; relevant S04/S05 prompts.

**implementation_or_artifact_details:** produce P0-P8 simultaneous residency timeline; RAM/VRAM/disk/temp peaks; wall/p50/p95; all remote call/token cost; concurrency/fanout; safe headroom; pre-mutation rejection rule.

**original_task_requirements_to_cover:** learner must fit on ordinary PC, continue learning from user input, keep deterministic+learned path compact.

**mental_simulation_required:** simulate baseline, one accepted pair, update, evaluation, rollback, ordinary reload; simulate OOM/disk full during each phase.

**good_examples:** “LoRA is 2 MB but optimizer+activations+reference make P4 exceed budget → reject before mutate.”
**bad_examples:** “adapter is small therefore PC-fit proven.”
**edge_cases:** CPU offload, shared tensors, evaluator process overlap, reindex double-copy, ordinary inference during training, cancelled branches.

**allowed_files:** planning/replanning and read-only core sources.
**forbidden_files:** product code/weights/datasets/workflows.
**allowed_commands:** read/search/hash/size/profiling commands that do not mutate.
**forbidden_commands:** train, push model state, delete/compact, paid model calls, Windows Actions.

**verification_required:** every numeric claim must be tagged OBSERVED/DERIVED/UNKNOWN and reproducible from a named measurement source.

**return_packet_schema:** footprint.csv + explanation + stop rules + unknown bindings + exact next measurement command list.

---

## R23-B — durable state / UNKNOWN_STATE / ordinary adoption

**CURRENT TASK:** prove the state transition chain from accepted parent through one pair/update to accepted or rolled-back state and cold ordinary consumer.

**NOT THE TASK:** do not invent a second trainer or repeat a pair after uncertain effect.

**EVIDENCE-ONLY:** manifests alone do not prove the ordinary caller uses them.

**context_files_to_read:** R18 A-D/G1-G6; R23 lifecycle; WorkLeft TASK-PROMPTS-TRAINING; RAG-01/RAG-02; FeedbackRouter/ContinuousLearningPipeline/FileDpoPairStore transactions; baseline/optimizer/checkpoint/session/trainer code.

**implementation_or_artifact_details:** exact state IDs/hashes for parent/reference/trial/tokenizer/encoder/index/graph/optimizer/RNG/applied-pairs; transition owner; durable write; readback; recovery; consumer reload.

**original_task_requirements_to_cover:** continual learning, no duplicate learning, correct many-to-many learning state, safe PC operation.

**mental_simulation_required:** crash before step, during step, after step-before-journal, evaluator crash, promotion timeout, stale ordinary cache, incompatible tokenizer/index.

**good_examples:** post-step timeout → UNKNOWN_STATE → reconcile fingerprints → evaluate same trial read-only; never re-step.
**bad_examples:** HTTP 500 → regenerate pair_id and train again.
**edge_cases:** corrupt accepted checkpoint, partial fsync, local S04 accepted while S05 remote blocked.

**allowed_files/commands:** read-only planning/core inspection.
**forbidden_files/commands:** mutation/training/provider calls.

**verification_required:** no transition may have an unowned mutable state; ordinary adopter must be traced or marked ADOPTION_GAP.

**return_packet_schema:** state-machine table + data-flow diagram + failure walkthroughs + missing-owner repair work order.

---

## R23-C — feedback admission and DPO authority

**CURRENT TASK:** map real user feedback into lawful evidence and determine when it may become a training lesson.

**NOT THE TASK:** thumbs-up is not truth; user edit is not automatically final chosen; rejected sentinel is not a teacher-authored chosen.

**EVIDENCE-ONLY:** generic FeedbackRouter behavior is not S04 authorization.

**context_files_to_read:** WorkLeft-02; RAG-01; DPO algorithm; FeedbackRouter; DPO stores; R21 domain laws; S05-S10 role audit; relevant prompts.

**implementation_or_artifact_details:** classify preference/factual correction/changed requirement/poison/OOD/tenant conflict; consent/source rights; novelty; domain/version; exact Unit/Flow/Orchestrator/Graph/Student target; quarantine rules.

**original_task_requirements_to_cover:** learning from user input without corrupting shared knowledge; correct DPO positive/negative construction.

**mental_simulation_required:** edited SQL answer with different dialect; malicious correction; changed legal rule; duplicate lesson; teacher unavailable.

**good_examples:** raw user correction retained as user evidence; independent law check + CODEX lesson selection; Chinese model authors both final sides; CODEX separately judges exact pair.
**bad_examples:** user edit copied directly into accepted S04 DPO chosen.
**edge_cases:** TIE/BOTH_BAD/UNJUDGEABLE, consent revoked, cross-tenant conflict.

**verification_required:** every admitted pair has provenance, same frozen prompt/context, chosen/rejected authorship, target recipient, no leakage from certification panel.

**return_packet_schema:** admission matrix + pair lineage schema + quarantine examples + target-owner map.

---

## R23-D — DPO pair quality and learning effect

**CURRENT TASK:** define and examine whether DPO pairs are *useful*, not merely syntactically valid.

**NOT THE TASK:** do not use loss decrease/checkpoint existence as learning success; do not repeat same pair until score rises.

**context_files_to_read:** WorkLeft RESEARCH-03, RAG-01, original DPO algorithm, R13/R17/R22, current DPO objective/tokenization code and manifests.

**implementation_or_artifact_details:** freeze prompt/context; chosen superior by domain law; rejected plausible and pedagogically specific; response masks/reference policy; pair novelty; before/after on independent same-capability families + protected old domains.

**original_task_requirements_to_cover:** correct positive/negative DPO and measurable improvement from bulk to bulk.

**mental_simulation_required:** both-bad pair, stylistic negative, leaking exam answer, correct chosen with wrong reasoning, one pair helps train set but hurts novel family.

**good_examples:** rejected violates one named constraint that the learner can generalize; held-out families improve without old-domain regression.
**bad_examples:** chosen is longer and rejected shorter, therefore pair accepted.
**edge_cases:** ambiguous preference, identical semantics, source-family duplicates, evaluator disagreement.

**verification_required:** report pair-admission rate separately from downstream gain; outcomes IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE.

**return_packet_schema:** DPO-pair rubric + 3 good/3 bad/3 quarantine examples + matched bulk evaluation recipe.

---

## R23-E — domain mapping and real-world law passport

**CURRENT TASK:** examine whether the system maps inputs to the correct domain(s) and preserves domain-specific laws before learned ranking.

**NOT THE TASK:** do not force every input into one known domain; do not score routing by agreement with current router.

**context_files_to_read:** R21 curriculum/passports; RAG-02; original task; WorkLeft coverage/requirements; relevant code for domain/router/logical units; domain datasets.

**implementation_or_artifact_details:** per-domain law table, allowed labels/multi-label/unknown, source-family split, precision/recall/F1 or appropriate multilabel metrics, unknown rejection, downstream quality with actual route vs expert-allowed route.

**original_task_requirements_to_cover:** broad learning across real domains; ability to map the best path to solution.

**mental_simulation_required:** mixed C#+SQL, unknown domain, same text with different jurisdiction/version, valid deterministic route beating learned route.

**good_examples:** expert-allowed domain improves downstream quality while actual router fails → routing gap isolated.
**bad_examples:** router says C#, therefore C# is ground truth.
**edge_cases:** multi-domain, low confidence, changed law/version, domain with sparse examples.

**verification_required:** macro + worst-domain result, support counts, confusion analysis, no hidden certification leakage.

**return_packet_schema:** domain passport + mapping benchmark + error taxonomy + next algorithm hypothesis.

---

## R23-F — vector tokenization / domain representation / LoRA projection

**CURRENT TASK:** separate tokenizer, structured parser, embedding/features, domain projection, optional codebook/vector tokens and LoRA adaptation; identify what actually should learn.

**NOT THE TASK:** do not call LoRA a tokenizer; do not treat equal vector dimension as compatibility.

**context_files_to_read:** R05/R12/R22; RAG-02; current tokenizer/vector/LoRA code; checkpoint/index manifests; original four-role goal.

**implementation_or_artifact_details:** raw→token/structure→embedding/features→domain projection→retrieval/ranking→output representation. For each edge: shape, semantic meaning, version, producer, consumer, mutable state, migration/reindex cost.

**original_task_requirements_to_cover:** proper vector tokenization per domain, compact learning, LoRA result transposition where actually justified.

**mental_simulation_required:** representation change improves compression but destroys law-sensitive distinction; same dimensionality with incompatible encoder; adapter norm changes but downstream retrieval unchanged.

**good_examples:** compare frozen reference representation vs one domain-aware projection with same downstream panel and full reindex cost.
**bad_examples:** fewer tokens = automatically better.
**edge_cases:** codebook collision, stale vectors, mixed domains, no-adapter baseline incompatible with runtime type.

**verification_required:** downstream semantic/path quality is primary; reconstruction/alignment loss is diagnostic only.

**return_packet_schema:** representation contract + migration matrix + controlled ablation work order.

---

## R23-G — dynamic kNN function per domain

**CURRENT TASK:** design and examine a domain-aware dynamic neighbor function that can outperform fixed-k only when evidence supports it.

**NOT THE TASK:** do not sweep many parameters simultaneously; do not assume ANN remains valid for a new query-conditioned score.

**context_files_to_read:** R06/R22; KnnLogicalUnit and actual index code; RAG-02; domain passports from R23-E.

**implementation_or_artifact_details:** admissible-neighbor hard constraints; normalized feature distances; domain/query/state-dependent weights; k/radius/temperature; coarse retrieval + reranking; multi-neighbor aggregation; refusal k=0.

**original_task_requirements_to_cover:** best dynamic kNN per domain; compact deterministic+learned combination.

**mental_simulation_required:** fixed k=1 baseline; same k with domain distance; adaptive k; exact-search oracle on small subset; first-hit consumer ignores additional neighbors.

**good_examples:** dynamic k improves downstream answer/path and total cost on sparse domain, while dense domain retains simpler fixed k.
**bad_examples:** recall@k improves but ordinary consumer still uses first hit and quality unchanged.
**edge_cases:** no legal neighbors, ANN misses best candidate, density shift, stale index after encoder update.

**verification_required:** one factor per contrast; report neighbor relevance, final quality, p50/p95, index bytes/update/reindex and PC cost.

**return_packet_schema:** function specification + per-domain parameter authority + experiment matrix + fallback rule.

---

## R23-H — four-level hybrid algorithm interaction and bulk-to-bulk learning

**CURRENT TASK:** examine the combined deterministic+learned behavior of ILearningUnit → ILearningFlow → ILearningOrchestrator → ILearningDynamicGraph and determine where each learned decision improves path quality or compactness.

**NOT THE TASK:** do not collapse four roles into one generic scorer; do not let learned ranking bypass deterministic type/law/safety boundaries.

**context_files_to_read:** original task; original DPO algorithm; R12 architecture; R21 curriculum; R22 contrasts; R23-E/F/G outputs; RAG-02; actual Unit/Flow/Orchestrator/Graph code and callers.

**implementation_or_artifact_details:**
- Unit: typed many→many candidate/result association and domain representation.
- Flow: relationship/correction between Unit outputs and next inputs.
- Orchestrator: async feasible path/ordering/branch selection with complete loser cost.
- DynamicGraph: higher-order pattern relation and unseen composition.
For every edge name deterministic law, learned authority, state/checkpoint, training signal, fallback, consumer and cost.

**original_task_requirements_to_cover:** best path to solution, compact algorithm usage, deterministic + nondeterministic combination, continual improvement on PC.

**mental_simulation_required:** deterministic path already optimal; learned Unit helps but Flow hurts; Orchestrator launches costly losers; Graph memorizes seen topology but fails unseen composition.

**good_examples:** promote only the layer whose controlled intervention improves held-out path quality/time without protected-domain regression.
**bad_examples:** one end-to-end score rises after changing all four layers and all four get credit.
**edge_cases:** fan-out/fan-in, multiple valid paths, cyclic/invalid graph, cancelled side effects, no-learning-needed case.

**verification_required:** layer-by-layer ablation plus end-to-end matched bulk; report path validity, solution quality, wall/CPU/GPU/memory, token/vector/index growth, old-domain retention.

**return_packet_schema:** graph reconstruction + edge authority table + bulk B0/B1/B2 comparison + promotion/rollback decision.

## R23 repaired sequencing

Execute in dependency order, not one giant pass:
1. A and B may run in parallel as read-only discovery.
2. C and D after source/authority map is pinned.
3. E before F/G so domain laws are not inferred from current vectors/kNN.
4. F and G may run as separate controlled hypotheses.
5. H consumes E/F/G and performs the combined layer attribution.
6. Only after these documentary/examination tasks are green enough to define one bounded experiment should R18/R22 execute the first real lesson. Do **not** wait for blanket Windows CI or full repository test coverage.

## Acceptance of the repair

R23 is repaired when:
- old A-D packets are explicitly historical/superseded as executor prompts;
- all eight work orders have complete model-facing fields and exact source/RAG obligations;
- learning acceptance is based on independent domain/bulk measurements, not test coverage;
- DPO, domain mapping, vectorization/LoRA, dynamic kNN and Unit/Flow/Orchestrator/DynamicGraph are separately falsifiable;
- deterministic safety tests remain only where they prove deterministic invariants, and are not counted as proof of learning improvement.


<!-- R23-V2-EXECUTABLE-REPAIR-2026-10-13 -->
---
# §V2 — R23: действующий исправленный план для модели

**Scope:** этот документ чинит R23, не исполняет DPO, не создаёт новых numbered Rxx. Выход R23 — source-bound work orders и научная методика проверки; результаты первых опытов принадлежат отдельно уполномоченным R18/R22/R24. Названия A0/A1 и H1–H5 — внутренние дочерние задания R23.

**ID COLLISION GUARD:** R18:G1–G6 обозначает preflight/runtime gaps; R23:G1/G2 — только subtask по kNN. R22:E1 — первый учебный эксперимент CodeDocumentation; R23:E01–E07 — только паспорта семи доменов. Использовать полные namespace R23-A0, R23-G1, R23-E01 в dispatch/receipts; никакой глобальной готовности из совпадения суффикса.

## V2.1. Review finding, архитектурная причина, выбранное решение

Старая редакция A–H полезно назвала восемь тем, но без достаточной детализации E–H (команды, границы файлов, алгоритм, реальные доменные примеры, полная evidence-передача). Большая задача H объединяла четыре learned authority в одну недоказуемую «оценку успеха». Фраза о последовательном завершении A–H до первого R18/E1 создавала именно тот новый исследовательский барьер, от которого нужно уйти. Ресурсный шаг A требовал «измерения», хотя разрешал только read-only и не имел доступа к подтверждённому устройству. Необходима и чёткая граница между Generic FeedbackRouter и разрешённым S04 DPO.

**Решение:** (1) раздельные source-bound work orders A–H и H1–H5, (2) ранний E1 с минимальным контекстом A0/B0/C0/D0/E0, (3) самостоятельные domain/representation/kNN/Graph исследования вне критического пути E1, (4) проверка полезности на независимых семействах задач, а не Windows/coverage, (5) отдельные DOCUMENT_VALID / CODE_TRACED / LEARNING_NOT_MEASURED / PC_NOT_MEASURED / INDEPENDENT_NOT_PERFORMED.

**FAST LANE E1:** если R18 G1–G6 реально разрешены и привязаны (parent или законный cold start, student+LoRA mutable keys, источник/teacher/CODEX права, независимая C# панель, no-repeat full snapshot и budget, ordinary caller), product-executor может начать первый E1 урок **без** завершения A1/C1/D1/E01–E07/F1/F2/G1/G2/H1–H5, полного R01–R20 DONE или GitHub Windows CI. Прежде чем мутировать state, лишь E1-relevant A0/B0/C0/D0/E0 должны дать достаточную безопасность и причинную проверяемость. C0 при curated E1 не требует реализованного пользовательского UI-feedback. R23 не делает вызовы/обучение за R18.

**Parallel:** независимые *документные* A1/B1/C1/D1/E01–E07/F/G/H могут исследоваться параллельно с E1, если не владеют одним изменяемым model/index. Девять research agents НЕ девять одновременных GPU trainers. Зависимости E-domain passport → F/G domain-specific hypothesis, H5 ← H1–H4, но эти зависимости не блокируют первую E1 попытку.

## V2.2. Общий исполнимый envelope — должен передаваться ПОЛНОСТЬЮ каждому child

**CURRENT TASK:** выполнить ОДНО конкретное R23-A0/A1/B0/B1/C0/C1/D0/D1/E0/E-domain/F1/F2/G1/G2/H1/H2/H3/H4/H5 и вернуть source-bound проектное решение.

**NOT THE TASK:** не запускать optimizer, обучающие/оплачиваемые внешние вызовы, Windows Actions, build/test/coverage ради «улучшения обучения»; не изменять продукт, веса, датасеты, workflows; не переписывать чужие results/Rxx; не сносить принятые snapshots, git refs и закрытые evaluation case IDs.

**EVIDENCE-ONLY:** прежние Rxx/FINDINGS, исторический ПК/8GB GPU, отсутствие errors в CI, список моделей, название класса, новый checkpoint hash и этот план — *не* measured learning, fit, invocation или permission. Присваивать каждому утверждению DIRECT_FULL, DIRECT_EXCERPT, ATTRIBUTED, CODE_TRACED, OBSERVED или UNKNOWN.

**SOURCE MAP:** Planning P = lubochka/xiigen-planning branch llm_plan3; D = P/llm mvp/design to code session; R = D/replanning; core K = lubochka/llm_mvp_core branch codex/dpo-implementation. Обязательно читать R/run-prompts/R23.md, R/run-prompts/COMMON.md, R/COMMON-CONTRACT.md, R/R23-PC-STATE-FEEDBACK-LIFECYCLE.md, релевантные results/R01–R22 и R/previous rounds thinking results.md (в частности R18/R22/R23), R/PARALLEL-R01-R03-JOIN.md, R/R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md G1–G6, R/R22-CAUSAL-EXPERIMENT-CATALOG.md C01–C11, R/results/R21 паспорта. До выбора slice: D/original task.md и D/Алгоритм создания ДПО.md; D/09.09.26WorkLeft-02-teaching-chinese-model.md; D/09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md; D/09.09.26WorkLeft-PROMPTS.md и task-relevant TASK-PROMPTS-TRAINING, RESEARCH-03-dpo-algorithm-conformance, MECH-01-mechanism-validation. RAG discovery ОБЯЗАТЕЛЬНО через D/09.09.26WorkLeft-RAG-MASTER-INDEX.md и RAG-00-usage-guide.md; trace relevant RAG-01 legal, RAG-02 DTC, RAG-03 branches, RAG-04 IDE, including NDJSON/actual root outside D. Directory listing ≠ прочитанные первоисточники. Не загружать всё подряд каждому ребёнку: читать только нужные spans и фиксировать непрочитанное.

**SKILLS:** K/docs/skills. Перед предметным выводом загрузить actual AGENTS.md + найденный SESSION-START-PROMPT, planning-session-startup, architect-behavior-classifier, authority-requirement-binding, reference-inventory-and-source-map, phase-planning, plan-review, goal-delivery-completeness, claim-verification, audit-protocol, specificity-calibration, output-skepticism, reconnaissance-gate, trainable-architecture-decision, no-product-decisions, human-readable-architecture-audit, output-readability-gate, experiment-design, learning-loop-closure. При кодовом пути дополнительно code-examination, data-flow-integrity, implementation-doc-code-refs, applicable implementation-doc-uml/algorithms/status/hierarchy. При feedback дополнительно client-adaptive-learning-loop, при pair/prompt — prompt-patch-authoring. Все имена выше — файлы вида NAME-SKILL.md. Skill text обязателен, перечисление не равнозначно исполнению; отдельный specialist текст подгружать по запросу ребёнка, а не копировать всю библиотеку в контекст каждого.

**ORIGINAL REQUIREMENTS:** ILearningUnit (domain + vector/token + typed output, many-to-many), ILearningFlow (correction between units), ILearningOrchestrator (async routes and budgets), ILearningDynamicGraph (higher-order), ILearningMachine (actual training owner), dual-side authorized DPO, lawful domain mapping, compact vector representation, query-dependent domain kNN, user feedback/continued teaching, bulk-to-bulk quality/time/retention/full-PC cost, real product caller; DTC/IDE/S00–S10 remain in programme (R24).

**IMPLEMENTATION DETAILS:** дать сначала человеческий русский рассказ: вход → действие и детерминированный закон → learned choice/state → typed output → consumer → ошибка/rollback. Затем исходный код: реальный caller→method→mutated state→next consumer (paths + branch + SHA + line range). Если не найдено — UNBOUND + куда читать, никакой вымышленной функции. Каждый proposed learned decision требует target trainable owner, signal/objective, fallback, version/migration and a falsifying experiment. Отдельно rejected alternative.

**MENTAL SIMULATION:** один good input, один bad/negative input и один edge/OOD; в каждом указать исходный доменный закон, ожидаемое поведение, чем проверить на реальном ordinary consumer, не использовать эти иллюстрации автоматически как teacher data/закрытый экзамен. Четыре уровня нельзя смешивать в одну логику. При тяжёлом контексте сохранять exact unread_path:span/risk/owner.

**ALLOWED FILES/COMMANDS:** только read/search/list/hash/parse/existing measurements в P/D/R, K source и K/docs/skills; child по умолчанию RETURN ONLY. **FORBIDDEN FILES/COMMANDS:** веса, датасеты, production source/workflows/secrets, чужие results/Rxx, удаление/reindex, paid model calls, optimizer, Actions/Windows retries, force push. GitHub edits собственного разрешённого R23 planning — только parent writer, не автоматическое право дочернего агента.

**VERIFICATION:** документно проверить requirement→primary source→law/owner→code seam→good/bad/edge→falsifier→next owner; научно (ПОСЛЕ отдельного разрешённого execution) frozen parent/no-update/trial, идентичная удержанная independent family panel, version+law consistency, old-domain retention, simultaneous RAM/VRAM/disk/failed calls. Детерминированные схемы, no-repeat, типы и rollback могут иметь узкие проверки; они НЕ свидетельствуют о gain. Не заявлять независимую CODEX/Astra UltraCode review без реального отдельного review точного кандидата.

**RETURN PACKET SCHEMA (все ключи обязаны быть):** child_id; claim_scope; evidence_scope; count_claimed; independent_evidence_count; subagent_id_or_NONE; downgrade_needed; exact_repo_ref_path_spans_sha_read_depth; WorkLeft_RAG_paths_and_unread; skills_loaded_receipt; requirement_ids; current_vs_proposed_algorithm; deterministic_laws_vs_learned_authority; input_output_associations; state_and_ordinary_consumer_owner; example_good_bad_edge; mental_simulation; rejected_alternative; falsifier/metrics; resource_cost; documentary_verdict; learning_verdict_NOT_MEASURED; PC_fit_NOT_MEASURED; unresolved_owner_and_next_step; files_changed_NONE. Вердикт per child: PACKET_VALID/NEEDS_FIX, не преждевременное PLAN_READY или LEARNING_PASS.

## V2.3. A0/A1 — бюджет фактического ПК, а не размер адаптера

**ROLE:** resource architect/code examiner. **CURRENT TASK A0:** зафиксировать минимальный E1 feasibility/stop. **A1:** долгосрочный рост PC. **NOT:** не угадывать числовые hardware/caps.
**CONTEXT:** K/14.ContinuousLearning/Training/IncrementalTrainer.cs, CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs, RepositoryOwnedModelCheckpointManifest.cs, RepositoryOwnedTrainingArtifactLayout.cs; найдённые actual loader/encoder/index/graph callers, WorkLeft RESEARCH-01, R09, R18 G1–G6.
**PROMPT A0:** «Проследи P0 preflight → P1 baseline → P2 selector/teacher/exact assessor → P3 full snapshot → P4 one optimizer update → P5 independent examination → P6 accept/restore → P7 cold ordinary call → P8 feedback. Для каждой фазы выпиши одновременно загруженные parent/reference/trial/LoRA/optimizer/gradients/activations/KV/encoder/index/graph/cache/journal, владельца CPU/GPU, RAM/VRAM/disk/temp double-copy, контекст/batch/fan-out, p50/p95/wall, стоимость CODEX+Chinese+evaluator и все отменённые ветви. Actual/DERIVED/UNKNOWN по каждому числу. Если неизвестно место для snapshot+restore, H0=HOLD до Step, но чтение иных ветвей продолжать».
**PROMPT A1:** «Для новых независимых bulk B0,B1,B2 оцени накопление bytes по accepted/rollback/journal/index/graph, reindex/compaction doubling, user cache, old-domain retention, concurrent inference; различи GPU capacity и платный remote inference; конечный budget/growth/eviction policy с защитой принятого state».
**GOOD:** LoRA 2MB, но optimizer+reference+snapshot не помещаются → не мутировать. **BAD:** «RTX 3050 8GB, значит точно помещается». **EDGE:** CPU offload, OOM во время snapshot, обычный запрос параллельно Step.
**OUTPUT/VERIFICATION:** таблица P0–P8 + источники замеров/команд для owner, finite stop, восстановление, UNKNOWN ledger, без выдуманного PC_FIT. **FILES/COMMANDS:** envelope §V2.2. **RETURN:** A0 или A1 с полными полями.

## V2.4. B0/B1 — state machine, crash, UNKNOWN_STATE и ordinary adoption

**ROLE:** transactional-state architect/code examiner. **CURRENT TASK B0:** one E1 pair/state/adopter trace; **B1:** continuing feedback recovery. **NOT:** никакого второго trainer/повторного optimizer для той же pair.
**CONTEXT:** K/25.ModelServer/ContinuousLearning/CodeDocumentationOneLessonEndpoint.cs; K/14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs; K/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs; K/14.ContinuousLearning/Training/{CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs,CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs,RepositoryOwnedModelCheckpointManifest.cs,IncrementalTrainingSession.cs,IncrementalTrainer.cs}; workLeft training+R18.
**PROMPT B0:** «Свяжи accepted parent/reference→E1 immutable pair ID→exclusive reservation→weights+adapter+optimizer+scheduler+RNG+counters snapshot→one allowed Step→trial→heldout→accepted pointer or complete restore→cold normal CodeDocumentation caller. Укажи для каждого перехода durable identity/readback/version, owner и изменяемые bytes. Реальный call chain вплоть до пользователя; отсутствие consumer = ADOPTION_GAP. Baseline.json не полноценный checkpoint; повреждённый parent не новый cold start».
**PROMPT B1:** «Прогони на бумаге crash before Step, after Step before durable journal, after trial before exam, promotion timeout, stale cache, mismatched tokenizer/index, disk-full, duplicate feedback, local S04 with remote S05 failed. UNKNOWN_STATE → verify process/journal/model+optimizer fingerprints → read-only reconciliation; никогда не повторять Step вслепую».
**GOOD:** HTTP 500 после Step оставляет UNKNOWN_STATE. **BAD:** pair переименовали и обучили второй раз. **EDGE:** expired OutputRef/partially fsynced manifest. **OUTPUT:** state diagram+transition ledger+recovery walkthrough+adopter gap. **VERIFY:** нет orphan mutation/unsafe replay. **FILES/COMMANDS:** envelope.

## V2.5. C0/C1 — законное происхождение DPO и реальный user feedback

**ROLE:** provenance/feedback architect. **CURRENT C0:** E1 lawful one-pair source/role route; **C1:** user-feedback host/admission. **NOT:** user editing или thumbs-up не становятся автоматически ground truth.
**CONTEXT:** WorkLeft-02, RAG-01, R21 law passports, K/14.ContinuousLearning/Feedback/{FeedbackRouter.cs,ContinuousLearningDpoPairStores.cs,FileDpoPairStore.Transactions.cs}, K/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs; actual UI producer search.
**PROMPT C0:** «Для одной source-lawful C# ошибки выдели permitted observation/source rights and privacy, CODEX GPT-6 Astra UltraCode human lesson selection, разрешённый внешний open Chinese model создает ОБЕ финальные DPO стороны, CODEX Astra отдельно проверяет ИМЕННО frozen exact pair; Student/reference/post-update independent evaluator отличны. Проверить legal+provider terms/authorization, лимит денег и передачу данных, не делать реального API вызова. Отсутствие grant = запрет affected mutation, не конец исследования».
**PROMPT C1:** «Для Accepted/Edited/Rejected, factual correction, preference, changed requirement, SQL dialect switch, malicious injection, cross-tenant/project, duplicate, revoked consent, OOD и TIE/BOTH_BAD/UNJUDGEABLE: источник, domain/version/law, novelty, isolation, human/model authority, quarantine, recipient Unit/Flow/Orchestrator/Graph/Student, async-only training. Найди actual host and next consumer or mark UNKNOWN. Generic FeedbackRouter user-authored chosen and rejected-sentinel НЕ считать Chinese-authored S04 финальной парой».
**GOOD:** SQL исправление меняет диалект → не засчитывать как ground truth старого SQL. **BAD:** user edit превращён в Chinese chosen. **EDGE:** законное исправление после revoke consent. **OUTPUT:** admission matrix+pair lineage/rights+recipient map+host gap; **VERIFY:** no unauthorized lesson/tenant leakage. **FILES/COMMANDS:** envelope.

## V2.6. D0/D1 — качество пары + доказанная польза обновления

**ROLE:** DPO objective examiner/prompt engineer. **CURRENT D0:** E1 exact pair admissibility; **D1:** retained independent bulk evaluation. **NOT:** loss decrease или число пар не свидетельствуют о gain.
**CONTEXT:** D/Алгоритм создания ДПО.md; WorkLeft RESEARCH-03 and RAG-01; R05/R18/R22; exact K current tokenization, objective and mutable-LoRA code discovered via caller graph.
**PROMPT D0:** «В замороженных prompt/context и реальном доменном правиле сформулируй выбранный ответ factually superior; отвергнутый plausibly wrong по ОДНОМУ полезному различию; не разрешай length-only/stylistic/hallucinated-author/both-bad/duplicate пары. Проверить response-only conditional log-probs policy and detached reference, masks/padding/EOS/length, beta/sign/chosen-vs-rejected, trainable keys, frozen reference, отсутствие loss по prompt. Не называй arbitrary similarity canonical DPO; отдельно фиксируй дифф с реальным кодом. Пример (не teacher-data): документация C# CancellationToken, rejected скрывает реальное исключение при отмене».
**PROMPT D1:** «Заморозь и заранее опубликуй heldout source-family hashes (не парафразы training), parent/no-update/trial, blind domain rubric, old-domain panel, confidence/uncertainty, cost+failed attempts, stop/look rule и минимальный эффект (от grant owner). Сравни bulk B0/B1/B2 на same matched anchor; separate pair admission, real gradient, normal caller adoption, measured quality. Не подгоняй thresholds и не повторяй pair ради PASS».
**GOOD:** hard negative правдоподобен и нарушает закон конкретного метода. **BAD:** chosen длиннее rejected => хороший. **EDGE:** оба допустимы/неподтверждённые C# facts → UNJUDGEABLE. **OUTPUT:** rubric, schema, 3 illustrative good/bad/quarantine case families, objective audit+matched paired panel. **VERIFY:** IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE/NOT_MEASURED раздельно. **FILES/COMMANDS:** envelope.

## V2.7. E0 / E01–E07 — реальные законы доменов, mapping и OOD

**ROLE:** domain/law architect. **CURRENT E0:** достаточный C# CodeDocumentation passport для E1; **E01–E07:** другие домены независимо. **NOT:** текущий router prediction не oracle.
**CONTEXT:** R21/R08 passports, original task, WorkLeft coverage+RAG-02, actual K domain classifier, StackDetector before normalizer, Unit pipeline and real data provenance.
**PROMPT E0:** «Возьми versioned C# method source и разбей на независимые code-family tasks; укажи input/output, typed domain label, law/overloads/null/cancellation semantics, allowed good answer и plausible false answer, abstention, ordinary consumer; экспертный allowed-domain vs predicted-domain, до обучения».
**PROMPT E01–E07:** «Заполни source/law/version/rights + domain/intent/stack + vector features + train/heldout/retention family groups + many valid paths/unknown для EN↔RU (negation/numbers), orchestration (ordering/side effect), Figma→HTML/CSS (geometry), C# docs, Angular docs (version/lifecycle), SQL docs (dialect/NULL/JOIN), DPO Builder (valid dual-side provenance). Сохрани дополнительные исходные домены, DTC/IDE и S00–S10 в реестре».
**GOOD:** C#+SQL запрос имеет два допустимых домена. **BAD:** router сказал C# => oracle=C#. **EDGE:** законодательство/SQL диалект изменились, unknown/abstain. **OUTPUT:** семь law passports, multi-label mapping precision/recall/F1 (если уместно), confusion/OOD and downstream actual-vs-oracle route quality, family sizes/worst-domain. **VERIFY:** no leakage from final exam to teacher. **FILES/COMMANDS:** envelope.

## V2.8. F1/F2 — domain vector tokens vs LoRA objective и перенос

**ROLE:** vector/adapter architect. **CURRENT F1:** representation graph; **F2:** actual LoRA recipient. **NOT:** считать LoRA tokenizer или равные размеры совместимым encoding.
**CONTEXT:** R05/R12/R22, RAG-02, K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs, actual BPE/HNSW/index/model projector and Student LoRA/checkpoints via code discovery.
**PROMPT F1:** «Для C#/SQL/translation/Figma нарисуй raw→StackDetector-before-normalisation→domain AST/tokens→embedding→optional vector projection/codebook→BM25/exact/ANN→typed many-to-many Unit output. Для каждого шага укажи semantic invariant, shape/type, encoder/version, producer/consumer, trainability, collisions, migration/reindex/cost. Сравни frozen baseline с одним law-aware вариантом, не меняя остальную модель».
**PROMPT F2:** «Trace chosen/rejected token IDs→response mask→policy/reference gradient→actual trainable LoRA keys/checkpoint→ordinary response. Если переносишь LoRA output в vectorized token, укажи source/target semantic spaces, decoder, learned alignment objective, versioned codebook, реального consumer и independent ablation; иначе явно отвергни этот механизм как неподтверждённый».
**GOOD:** same vector dimension/different encoder ID требует reindex. **BAD:** adapter 2MB значит vector token semantics хороши. **EDGE:** negation ‘not’ lost by compression. **OUTPUT:** version compatibility and response-vs-retrieval ablation, cost/retention. **VERIFY:** actual downstream semantic quality primary, reconstruction diagnostic. **FILES/COMMANDS:** envelope.

## V2.9. G1/G2 — формальная dynamic kNN per domain

**ROLE:** retrieval architect/code examiner. **CURRENT G1:** actual candidate/consumer path; **G2:** falsifiable adaptive function. **NOT:** k>1 не гарантия, и ANN-index не следует без проверки использовать с query-dependent metric.
**CONTEXT:** R06, R22 C03–C06, K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs Train/Execute, actual HNSW neighbor consumer, RAG-02 and E-law passports.
**PROMPT G1:** «Trace input→lawful candidate eligibility(domain/version/type/tenant/rights)→normalized distance/index→neighbors→first-hit/aggregation→ordinary answer. Если real code использует только first-hit, это BLOCK на utility of changed k unless downstream policy changes. Exact search oracle on small subset vs ANN shortlist; incompatible encoder => no index reuse».
**PROMPT G2:** «Define eligible_d(q,x)=authorized & version-compatible & lawful-domain/type. Hard violations mean exclusion, not soft score. For eligible candidate distance_d(q,x)=sum_j w[d,j](q,confidence,density,state_version) * normalized_distance[d,j](q,x) + allowed_soft_penalty, w>=0 and sum(w)=1. N_d(q)=eligible candidates under radius_d(q,confidence,density), k_d(q)=min(K_cap, size(N_d), affordable_budget_k); k=0 => lawful refusal. Name weight owner/trainable signal/version; compare in separate arms fixed k=1, fixed k>1, weighted metric same k, adaptive radius/k with frozen metric, multi-neighbor aggregation same candidates. Count exact-vs-ANN recall, true ordinary solution/path gains and full PC cost, not recall@k alone».
**GOOD:** sparse SQL dialect excludes cross-dialect neighbors. **BAD:** recall@8 improves while consumer still first-hit. **EDGE:** no admissible neighbor/ANN missed best one/density shift. **OUTPUT:** formula, learned/fixed authority, fallback and 5-arm scientific comparison. **VERIFY:** one factor per arm plus resource/retention; **FILES/COMMANDS:** envelope.

## V2.10. H1–H5 — four distinct hybrid learning levels

**ROLE:** integration architect with separate level owners. **CURRENT:** H1–H4 independently reconstruct and falsify each level; H5 joins, not before source-bounded receipts. **NOT:** generic global scorer or all-four-changed attribution.
**CONTEXT:** original four-role task, R07/R12, actual K/00.Core/LogicalContracts.cs + K found actual Unit/Flow/Orchestrator/Graph interfaces and callers, R22 C07–C11, E/F/G receipts and source RAG-02. Do not assert conceptual ILearning* is literal code type unless verified in source.

**PROMPT H1 Unit:** «Для input IDs a,b, output types T и three legal result IDs, trace domain+vector schema+typed many→many association source→Unit→ordinary consumer. Verify associations survive reorder, cancellation and fan-in/fan-out, not positional Zip; name deterministic law, learned Unit mapping/ranker state and ILearningMachine owner or UNKNOWN. Freeze Flow/Orchestrator/Graph; compare deterministic Unit vs changed Unit on independent families. GOOD: a has two distinct typed outputs; BAD: a loses mapping on shuffle; EDGE: no lawful output. RETURN: Unit typed contract, learned signal/fallback, measurement».

**PROMPT H2 Flow:** «Fix same Unit output IDs; trace OutputRef payload resolver identity/version/lifetime/tenant, correction to next Unit input. Design one learned relation-state hypothesis and separate deterministic link baseline, not a fabricated trainable API. GOOD: relevant corrected downstream payload; BAD: only AsQueryable hash used; EDGE: stale/cancelled OutputRef. Freeze other layers; RETURN: Flow state/owner, source-to-consumer, one-change ablation and lawful validity».

**PROMPT H3 Orchestrator:** «Trace legal DAG, async branch scheduling, dependency, cancellation/idempotency, fan-out/fan-in and budget; identify learned route/ordering decision owner or UNKNOWN. On same admissible path set compare deterministic scheduling/learned ranking/hybrid, counting ALL losers, errors, wall and side effects; no claim global optimum outside explored graph. GOOD: cheaper correct end result; BAD: fastest wrong path; EDGE: cancelled branch already caused irreversible effect. RETURN: route trace, branch-cost table, falsifier».

**PROMPT H4 DynamicGraph:** «Trace actual higher-order typed graph edges/version, unseen pattern composition, lawful final arbiter, mutable learned relation owner and actual downstream consumer. Compare seen topology and genuinely unseen composition while Unit/Flow/Orchestrator frozen; no “new graph hash” as learning. GOOD: unseen correct lawful composition; BAD: memorized topology only; EDGE: cycle, stale edge/version or forbidden relation. RETURN: higher-order graph UML, learned/fixed authority, transfer/failure metrics».

**PROMPT H5 Integration:** «Consume H1–H4 receipts + E/F/G contracts, and draw user request→domain mapping→Unit typed associations→Flow correction→Orchestrator async feasible path→Graph unseen higher-order resolution→lawful final arbiter→ordinary reply→feedback→Machine/DPO target→adopted state→next ordinary reply. Every edge: producer/consumer/type/version, deterministic law, trainable authority, checkpoint and full PC cost. Paper-run: C# CancellationToken, mixed C#+SQL, EN→RU negation retention, invalid graph route, cancelled expensive async branch. Compare deterministic-only vs learned-only constrained by laws vs hybrid; single-layer ablations and matched source-disjoint B0/B1/B2; report worst-domain regression and honest negative or inconclusive. Return architecture diagram and exact R18/R22/R24 evidence receipt. Missing H_i = UNKNOWN, never invented».

**GOOD:** promote only the independently improving layer. **BAD:** change all four layers and say each learned. **EDGE:** hard law forbids an otherwise high-scoring route. **OUTPUT/VERIFICATION:** five individual full-envelope return packets, real caller/consumer, per-layer change/no-change and cost/accuracy/retention. **FILES/COMMANDS:** envelope; no actual graph mutation.

## V2.11. Scientific examination template and honest stopping

**Frozen before candidate:** experiment_id; law/domain/intent/version; train/heldout/confirmation/retention source-family IDs+hashes; parent/no-update/trial identifiers; tokenizer/encoder/index/graph/adapter versions; selected lesson+CODEX human operator ID; legally permitted Chinese teacher ID for both exact sides; distinct CODEX exact-pair assessor and independent post-update evaluator; sample size rationale; primary domain-law metric, routing/path validity, minimum utility threshold/uncertainty, hard failure tolerance, budgets/early stop, ordinary consumer. Unknown budget is not zero and is not a grant.

**Per matching bulk:** real domain multi-label routing vs expert allowed/OOD, pair quality vs Student gain separately, path correctness (including negative/warning abstention), Unit-many→many/Flow-correction/Orchestrator-all-branches/Graph-novelty, domain tokens/dynamic-k/LoRA attribution, p50/p95+full time, RAM/VRAM simultaneous peak/temp and growth, remote calls and failed attempts, old-domain retention, accepted/rolled-back/consumer version. Different bulk difficulty requires fixed anchor panel and family-level uncertainty. Observed high score on training/pair material is not generalisation.

**Verdicts:** IMPROVE requires predeclared primary gain + no hard-law failure/retention violation + resource cap; REGRESS keeps parent; INCONCLUSIVE keeps parent and proposes a DIFFERENT justified hypothesis or genuinely independent extra data within grant; INVALID_EVIDENCE repairs evaluation without repeating an already attempted optimizer; NOT_MEASURED if no run. Deterministic contract checks (types/replay/recovery/host integration) remain narrow and never called learning quality tests. Unknown outcome never triggers silent retry.

## V2.12. Review and publication boundary

Review these V2 packets for schema completeness and source-honesty with planning/code/prompt and human-communication skills. Current R23 change targets permitted only R/R23-REPAIR-WORK-ORDERS.md, R/run-prompts/R23.md, R/R23-PC-STATE-FEEDBACK-LIFECYCLE.md and own R/results/R23 five carriers. Hand R18/R22/R24/R25 proposed exact changes, do not overwrite their sibling results. Each write requires current SHA and readback of actual GitHub bytes. Separate CODEX/Astra independent exact-candidate review is NOT_PERFORMED until a real authorized invocation.
