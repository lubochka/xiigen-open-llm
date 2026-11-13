# XIIGEN — предметная программа проверки гибридных алгоритмов (PARALLEL25)

**Design and examination contract, 2026-10-13.** Источники: `results/R03/R04/R05/R06/R07/R12/R13/R18/R21/R22/R23`, `R12 A–F`, `R13-REPAIR-WORK-ORDER`, `R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER`, `R22-CAUSAL-EXPERIMENT-CATALOG`, D/WorkLeft-02, WorkLeft-RESEARCH-01, WorkLeft-RESEARCH-03 и оригинальные D/G/F requirements. Ниже предложены критерии будущего исследования; **не** measured values и **не** новые готовые APIs. Очередь не обнуляется.

## Четыре различимых learning interventions (каждый с законной детерминированной частью)

| Role | Детерминированное (оставить) | Что может обучаться | Mutable state → ordinary consumer | Проверяемая гипотеза |
|---|---|---|---|---|
| **ILearningUnit** domain/vector/output typed many→many | Типизация, законный фильтр, связь input↔outputs, stable IDs, serializer and pure transforms | Веса Student/LoRA, предпочтение полезных typed outputs, domain representation | Student/adapter/known tokenizer version → typed result payload; не путать materialized refs с payload | Новый допустимый case улучшает новые typed answers без потери association |
| **IlearningFlow** коррекция Unit→Unit | Input/output compatibility, permissions, branch invariants | Обучаемое edge weighting, choice of correction, recovery from mismatch | Flow relation snapshot → runtime actual correction of downstream input/output | Меньше неверных переходов при замороженных Unit weights |
| **IlearningOrchestrator** asynchronous many→many | Dependency DAG, cancellation, idempotency, deadlines, bounded resources, lawful constraints | Policy ранжирования допустимых paths, parallel vs sequential tradeoffs | Versioned policy → actual async execution, trace and branch outcome | Лучший допустимый path/time/cost без нарушения ordering |
| **ILearningDynamicGraph** higher-order patterns | Identity/provenance of graph nodes, constraint arbiter, graph snapshot/version | Learned higher-order motif/relationship transfer and Builder case recommendation | Accepted graph/motif memory → ordinary unseen composite route | Перенос на невстречавшиеся composition families, не запоминание соседнего графа |

Смешение четырех частей в один `IsLearned` флаг/один DPO loss недопустимо. ILearningMachine может обслуживать разные targets; **конкретный recipient и loaded selected state** проверяются отдельно. Не приписывать готовность Graph training по одному интерфейсу: в нынешнем R12 evidence DynamicGraphMachine имеет режим `honest_untrained / TRAINING_NOT_ACTIVATED`.

## Domain-aware tokens, vectorization, dynamic kNN (R04/R06/R08/R22)

В каждом домене паспорту нужны: `DomainId, version, schema, legal_hard_rules, inputIR, outputIR, tokenizer/encoder version, feature semantics, candidate_pool, abstain policy, learned-state target, ordinary consumer` и examples. Контраст отделяет семантику от формата.

| Domain | Закон и значимое представление | Hard negative / OOD | Независимый экзамен |
|---|---|---|---|
| EN↔RU | negation, tense, placeholders, named entities, preservation of facts; bilingual spans | fluent but omitted «не», changed ID or role | accuracy of meaning/constraints, not only fluency |
| Orchestration | dependency DAG, retry semantics, irreversible side effects, time budgets | deploy before validated build, duplicate mutation | lawful executed path, latency/abort success |
| Figma→HTML/CSS | visual hierarchy, responsive breakpoints, interactive DOM semantics, assets license | pretty screenshot with missing semantics or wrong viewport | same task/viewport visual+functional comparison |
| C# documentation | namespaces, types, generics, async effects, signatures and code references | plausible invented method or missing exception | source-grounded doc fidelity and navigable refs |
| Angular documentation | component/template/signal and DI binding across versions | binding to nonexistent property, wrong framework version | referenced bindings exist and user task completed |
| SQL documentation | schema/keys/joins/dialects and constraints | cross-tenant join or incorrect NULL/aggregation | executable meaning under authorised fixture/semantic oracle |
| DPO Builder | lawful provenance, meaningful chosen≻rejected, separate assessor and utility | rejected is equally right; chosen contains licensed/restricted target | candidate admitted AND later independent Student benefit, separately |

Extras, new languages and IDE/DTC flows keep original owner+acceptance even if no examples in this compact table. Same word token in SQL/C#/English need not map to identical coordinates; use domain-marked features and versioned projector. Existing input-local 60D TeachingVector and persisted BPE/HNSW vector space are **not interchangeable** unless a measured map demonstrates it. Never infer vector meaning from model dimension.

Dynamic `k_d(x)` is an **experimental policy**, not a magic constant: candidate features include neighborhood density, top-score margin, index uncertainty, domain shift, reliability/noise, cost/RAM budget and hard constraints; restrict by admissible k_min..k_max and abstain on unknown. Compare fixed k, dynamic k, top-1-only consumption, multi-neighbor aggregate/rerank and learned selector under **same domain pool/index snapshot**. Measure lawful answer quality, recall/nDCG for genuine relevant references, calibrated abstention, path quality, p50/p95, RAM and false authority. If unchanged first-hit output, changing k without consumer change is *not* a proven improvement.

**LoRA is not vectorization**: optimizer produces adapter-weight update; adapter produces response/hidden representation; a distinct versioned projector/tokenizer may encode that response into a typed domain vector. Bind each transformation to source bytes, dimensional semantic schema, collisions, loss, and downstream consumer. Compare LoRA-only vs representation-only vs both; no transfer of weight bytes across incompatible vocabulary.

## Correct DPO and causal path to actual teaching (R05/R13/R18)

`CODEX Astra UltraCode selects authorised lesson` → independent eligible Chinese model generates **both** final `chosen` and `rejected` → separately `CODEX Astra` assesses exact authorised context & both projected texts → semantic and byte/material preservation → authorised one-new-`pair_id` update of **correct Student/adapter** → trial checkpoint → independent evaluation → accept/reject/rollback → cold ordinary user consumption; older accepted pair reused as history/context **not** hidden optimizer replay. Pair selector, pair author, pair judge, runtime safety arbiter, Student and post-update evaluator have different obligations.

A conventional response-conditional DPO objective compares `log πθ(chosen|prompt)-log πθ(rejected|prompt)` **relative to frozen πref difference** through a sigmoid/preference loss. Gradient/loss/positive pair quality alone cannot show useful behavior. Check what actual code optimizes; when it is an approximation, label it exactly. Before optimizer assert **judged semantic material still present**: formerly found whitespace collapse / 480-char trimming must not erase distinguishing tokens. Keep raw and projected hashes *and meaningful differencing*. `TIE/BOTH_BAD/REJECTED_BETTER/UNJUDGEABLE` are not forced into a positive preference; a correct deterministic answer is never rejected merely because it came from a rule.

## Prospective E1–E4, independent starts, same panel and rollback

- **E1 Student/DPO**: N = frozen parent served with corrected exact code; U = same parent + one authorised pair, identical tokenizer/objective/inference and heldout panel. Baseline can be captured before training; no need to complete all seven domains first. Verify cold selected-state reloading, not assessment-only TextGenerator.
- **E2 Domain/vector/kNN**: fixed representation+fixed-k vs domain-feature-only vs dynamic-k-only vs combined on identical lawful queries/index and same frozen Student; evaluate changed actual choice, not neighbor count.
- **E3 Flow/Orchestrator**: Units frozen, compare fixed legal route vs learned correction/order over unseen DAG scenarios with same hard invariants; trace cancellations and time-to-solution.
- **E4 DynamicGraph**: lower role parameters frozen, compare graph off vs motif transfer on family-disjoint unseen combinations and retention. Training currently not established, so preserve as targeted future capability, not release prerequisite for E1.

**Experiment passport before seeing outcomes:** `experiment_id, owner, commit and model/base/adapter/checkpoint, consent/license, source/pair hash, domain law+panel version, train/validation/heldout split by families, N/U state, knobs held fixed, seed/repeats, predeclared primary metric and minimum benefit, uncertainty/CI rule, robustness and OOD negative cases, no-leak rules, p50/p95 and full PC accounting, rollback trigger, ordinary consumer IDs`. Predefine numeric effect thresholds rather than retroactively selecting pass; if missing `DESIGN_BINDING_REQUIRED` not approval. User's 95% aspiration is a requirement to plan for, **not** achieved accuracy or automatic decision threshold. Report `IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE` by domain/family; ties and failures are useful findings.

**Bulk b:** one new admitted pair per permitted update, with `pair_id` dedup across crash/restarts. At b0 evaluate all frozen domain panels, apply new lesson at b1, evaluate new domain + previous panels, continue b2.. only after verdict; report new-task performance, retained-task drop, total wall time, time-per-valid-gain, tokens/teacher/judge cost, CPU/GPU/RAM, disk, index, optimizer, rejected attempts, storage and model version. No heldout example becomes future training data. Protect regulated/sensitive user feedback against unauthorized use/poisoning; generic FeedbackRouter preferred responses are **not automatically Chinese-authored pairs**.

## Release boundaries and falsification

Structural integrity checks remain for types, hashes, consent, tenant/auth, idempotency, exactly-once, state recovery, compatibility; they are *not* quality verdicts. Stochastic learning uses protected holdout panels, independent judge, repeated or paired comparisons and uncertainty. Windows/Actions only when an actual host/runtime interface requires them; do not require Windows to choose a semantic DPO pair. Value is ordinary DTC/IDE/translation/documentation delivered to user, not a score in a trainer-only evaluator. One contradictory domain law violation, unbound selected state, or data leak invalidates the specific claim even when average score rises.

**Owner map:** R04/R06/R08 mechanisms and passports; R05 pair objective; R07/R12 role connection; R13/R22 prospective experiments; R18 implementation fast lane; R21 curriculum; R23 PC/feedback; R24 ordinary DTC/IDE; R15 and R25 external independent review. This file is an execution criterion, not an extra R26.
