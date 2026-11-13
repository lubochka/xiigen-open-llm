# XIIGEN — перевод R01–R25 в code slices: evidence → механизм → продукт → обучение

**13.10.2026 · STATUS: DOCUMENTARY PLANNING STRATEGY / NOT IMPLEMENTED / NOT TRAINED / NOT INDEPENDENTLY REVIEWED.**

## 0. Главная договорённость

25 Rxx = **10 исследований R01–R10 + 5 анализов R11–R15 + 5 изменений R16–R20 + 5 дополнений R21–R25**, уже существующие assigned runs, допускающие независимые параллельные старты согласно [PARALLEL-R01-R25-CONTRACT](PARALLEL-R01-R25-CONTRACT.md). Они НЕ являются 25 реализациями, 25 GPU lessons, 25 approvals или сертификатами. R10 интегрирует **девять R01–R09 inputs**; partial input даёт partial join, не остановку иных работ. Продолжающиеся Rxx владеют собственными results/Rxx и checkpoint. Slice integrator не меняет их статусы и не переписывает результаты.

**Главная цель:** за минимальную полную стоимость получить одну действительную learnable state mutation и измеримое изменение качества обычного решения, сохраняя все четыре архитектурные роли и возможность локального дальнейшего обучения. Уже существующие WorkLeft research, DPO/LoRA, kNN/Graph и 25 Rxx использовать как baseline; не создавать второй trainer, второй полный roadmap или ещё один Windows-run gate.

## 1. Не 25 «DONE», а 5 разных уровней evidence

1. SOURCE_TRACED: actual primary blob, path/span, root/ref, author/authority; RAG MD/NDJSON index **не** подтверждает чтение primary.
2. CODE_TRACED: current K blob, конкретный producer→transform→actual ordinary consumer, original callers/downstream, full file before editing. Имя класса не доказывает активное обучение.
3. PRODUCT_IMPLEMENTED: named code diff, focused deterministic property guards, real invocation, load/reopen; не training gain.
4. SCIENTIFICALLY_EXAMINED: frozen domain-family panel; baseline/no-update/isolated treatment under same pipeline; validity, N, uncertainty, laws, quality, time, whole-PC/retention.
5. ADOPTED_AND_REVIEWED: accepted state observed by **cold ordinary consumer**, exact-SHA separately invoked reviewer receipt; mere published pack = DRAFT.

Нельзя повысить один уровень по доказательству нижнего. R25 independent CODEX/Astra UltraCode invocation = NOT_PERFORMED до настоящего receipt. Эти документы не генерируют Chinese pairs, Student updates или benchmark numbers.

## 2. Решение, куда идут результаты

| Rxx | Source contribution; do not re-execute | Primary product slice / specific consumer |
|---|---|---|
| R01 | Whole-program P/K/G/F/V/WorkLeft/RAG/slices/extras inventory; IN_PROGRESS | every slice source-route ownership, not a prerequisite to all implementation |
| R02 | 52 requirements, four roles, authority/ID | RP01/RP02/03/04 original→mechanism→consumer matrix |
| R03 | RAG→primary context, still SOURCE_TRAVERSAL_NOT_COMPLETE | per-slice visited primary receipts; NOT global re-audit |
| R04 | Domain, representation, typed many→many, coordinate meaning | RP02 mapper/encoder; RP03 association |
| R05 | exact DPO pair lost in Compact 480 and response likelihood mismatch | RP01 lossless material + objective + recipient |
| R06 | dynamic domain/kNN, KnnLogicalUnit hit[0], graph routes | RP02 real multi-neighbor consumer/rerank/k; RP04 Graph |
| R07 | full hybrid A–J REPAIR_REQUIRED | RP01–RP04 separately affected causal seams, no premature global closure |
| R08 | seven subject laws, GOOD/BAD/TIE/OOD | RP01 lawful C#; RP02 C#/EN→RU then staged subjects |
| R09 | process/Windows expense evidence incomplete (RAW_SAMPLE_NOT_RECONSTRUCTED) | all slices wall-clock/total cost ledger, no invented savings |
| R10 | nine-way partial research join | slice dependency map with source/conflict/limitations |
| R11 | minimal first-lesson DAG versus complete-delivery DAG | RP01 critical path, RP02 independent early start |
| R12 | A–F integrated architecture design candidate; runtime OPEN | four role ownership/contracts RP01–RP04 |
| R13 | experiment compiler REPAIR_REQUIRED, actual launch bindings OPEN | RP01 frozen P/C0/T; RP02 C01–C06 causal cards |
| R14 | all capabilities S00–S10/extras, no mandatory serial chain | RP01–RP06 full owner/trigger preservation |
| R15 | skeptic challenge, actual independent Astra NOT_PERFORMED | separate exact-code/diff review per slice |
| R16 | requirements repair in live WorkLeft | per-slice acceptance clauses and source authority |
| R17 | learning != coverage != Windows/native release | per-slice separated acceptance |
| R18 | A–D first CodeDocumentation learner work order | RP01 implementation; reuse, don't rewrite |
| R19 | dispatch / effective prompts, 25-start clarification | named cold-start prompt and per-task returns |
| R20 | packet manifests/child packets with documentary limits | model-ready individual tasks, not count/manifest-only |
| R21 | seven domains/curriculum, C# first, EN→RU second | RP01 E1; RP02 E2 subjects; RP05 Builder/feedback |
| R22 | C00–C11 causal contrast and interactions | RP02 C01–C06, RP03 C07, RP04 C08–C09, RP05 C00/C10–C11 |
| R23 | whole-PC state, feedback, no-repeat, restore | RP01 one update; RP02 optional isolated state; RP05 full continual loop |
| R24 | ordinary DTC + IDE user journeys | RP01/02 real consumer receipts; RP06 product delivery |
| R25 | independent examination packet + incomplete current closure | exact-candidate review of each completed slice; no fictional approvals |

**Important unresolved statuses observed 13.10:** R01 IN_PROGRESS, R03 SOURCE_TRAVERSAL_NOT_COMPLETE, R05 OPEN, R07 REPAIR_REQUIRED, R08 IN_PROGRESS, R10 NINE_WAY_RESEARCH_INCOMPLETE, R12 design candidate, R13 REPAIR_REQUIRED, R15 independent NOT_PERFORMED, R18 E1 runtime OPEN. Future runs must refresh against live source, not freeze this snapshot.

## 3. Slice pipeline (not rigid waterfall)

| Slice | Distinct objective; minimum demonstrable outcome | Inputs | Does NOT block |
|---|---|---|---|
| [RP01](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/) | E1: exact Chinese-both-sides DPO → CODEX separate judge → response-only LoRA update → P/C0/T → ordinary Student; or explicit lawful no-update | R05/R11/R12/R13/R18/R21–R25, WorkLeft-02 | RP02 domain research and first baseline |
| [RP02](Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/) | E2: lawful domain/intent/stack identification, versioned domain vector/IR, real multi-neighbor fusion, isolated rerank/k policy, ordinary measured utility | R04/R06/R08/R12/R13/R21/R22/R23; F domain authority and K code | RP01 first training |
| [RP03](Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/README.md) — published planning pack, NOT_IMPLEMENTED; [five task-bound first Flow lesson passes](Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/RP03-FIVE-EXECUTOR-PASSES-TO-FIRST-FLOW-LESSON-2026-10-14.md) | E3 Unit many→many actual typed consumer + one trainable Flow correction under frozen Units, independent C#/EN→RU heldout (SQL staged) | RP01.04C, R04/R07/R12/R22 C07, K LogicalContracts & actual OutputRef consumer | E1/E2 scores; only required compatible port schema |
| [RP04](Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/) — published DRAFT | E3/E4 async Orchestrator lawful routing and DynamicGraph higher-order motif transfer; frozen Units/Flow | RP01.04D, R06/R07/R12/R22 C08–C09 | full S00–S10 release |
| [RP05](Kiro-Xiigen-RP05-Continual-Learning-PC-Feedback-Slice-Pack/) — published DRAFT | PC-footprint/continual feedback/Builder/retention, real source-consented new lessons, isolated LoRA vs token projection | R21–R23/C00/C10/C11, RP01.03B, RP01.04E | breadth preparatory work |
| [RP07](Kiro-Xiigen-RP07-Builder-MetaLearning-Slice-Pack/) — **PUBLISHED DRAFT, 17 tasks** | Original S07 Builder meta-learning: lawful graph teaching candidates → CODEX case choice → authorized open Chinese BOTH exact final DPO sides → separate CODEX pair-judge → distinct Student utility → lawful B0→B1 Builder meta update → cold round2 better teaching or negative result; original 5 arms × 2 rounds × 6 subjects | historical S07/44 tasks; WorkLeft R07/R12/R13/R21–R25; RP01 Student, RP02 kNN, RP04 Graph, RP05 PC/continual; full current K ordinary consumer | RP01 first E1 lesson, RP06 DTC/IDE transport, RP08 seven-domain cross-transfer |
| [RP09](Kiro-Xiigen-RP09-Independent-Learning-Examination-Lab-Pack/) — **PUBLISHED DRAFT (14 model-ready tasks)** | Independent actual-consumer scientific examiner: domain-family lawful panels, frozen rubric, exact-pair/LoRA provenance, C00–C11 ablations, true local runner, blinded P/C0/T, full PC/bulk retention, cold adoption and independent exact-candidate review; reuse and do not duplicate RP01–RP08/S09 product owners | R13/R17/R22/R23/R24/R25; seven WorkLeft domain laws; actual K caller/probes; RP01/02/04/05/06/08 source | first DPO lesson, whole native release and all 25 Rxx closure; no new teacher grant |
| [RP08](Kiro-Xiigen-RP08-Hybrid-Domain-Transfer-and-DTC-Adoption-Slice-Pack/) — **PUBLISHED DRAFT** | Cross-domain hybrid integration: domain law→semantic token/vector→adaptive kNN→typed Unit/Flow→async Orchestrator/Graph→actual Student/LoRA selected state→ordinary DTC/IDE; causal seven-domain family and whole-PC examination, without replacing RP01–RP06/S08 owners | R04/R06/R08/R21–R25; original WorkLeft/RAG; actual K caller and legacy S08/RP06 consumer | RP01 first lesson; existing S08 transport product release | 
| [RP06](Kiro-Xiigen-RP06-DTC-IDE-Learning-Delivery-Slice-Pack/) — **PUBLISHED DRAFT, 19 tasks** | DTC: Figma/assets→HTML/CSS multi-viewport→API/storage→teaching→delivery; IDE: indexing→task/plan→execution→weakness→lesson→ordinary use; native/remote S00–S10 closure | R14/R16/R24; original slices and P/V/RAG | first scientific observations |
| [RP09](Kiro-Xiigen-RP09-Independent-Learning-Examination-Lab-Pack/) — **PUBLISHED DRAFT** | Local independent examiner/real runner, sealed domain-family exams, blind P/C0/T and full-PC cost; **no new trainer or learner** | R13/R17/R22–R25, RP01–RP08 actual owners | scientific baseline before Student update |
| [RP10](Kiro-Xiigen-RP10-PC-First-Learning-Pilot-and-Promotion-Slice-Pack/) — **PUBLISHED DRAFT** | One ordinary PC-first hybrid learning pilot + authorized feedback→new DPO, science-gated full-state local selected pointer, cold reload/rollback and historical S10 FINAL owner handoff | R01–R25, WorkLeft, RP01–RP09, G/F/V, historic S10 | RP01 first lesson, RP09 baseline, all native host delivery |
| [RP11](Kiro-Xiigen-RP11-Causal-Hybrid-Credit-and-PC-Policy-Slice-Pack/) — **PUBLISHED DRAFT (14 tasks)** | New small causal-credit/provenance and whole-branch-PC observation; matched factor/interaction attribution; compact truly learned domain-conditioned credit state, cold rollback and safe score hint to RP04 lawful Orchestrator/Graph; CODEX lesson-choice / approved Chinese both DPO sides / separate exact-pair CODEX judge only via existing RP01/RP05; RP09 independent examiner and RP10 pilot/promotion reused, not duplicated | R04/R06/R08/R11/R12/R13/R21–R25, WorkLeft+RAG→primary, P/G/F/V, actual K caller, RP04/RP05/RP09/RP10 | first RP01 DPO lesson, all seven-domain certification, native S10/IDE release; no new model-call permissions |

RP08 is now a **published model-ready documentary pack**; RP03 remains an allocation here; RP04, RP05, RP06 and RP07 have their own published **DRAFT** work packs, NOT proof of product execution. RP06.06D owns DTC/IDE full-product hybrid/PC efficacy; RP08.04B is only isolated cross-domain **algorithm-factor** attribution and consumes RP06 host receipts without duplicating them. Earlier strategy snapshots do not prove product implementation. Discover actual owner/consumer before materialization. Existing original S00–S10 tasks retain their obligations and separate native/publication gates.

## 4. Dispatch decision per Rxx finding

For each unique finding ID, build an integration row:
`finding_id | source_file/blob/line + evidence_depth | original_requirement_id | affected role/state | producer→consumer | existing capability/reuse | exact minimal missing seam | proposed slice/task | experiment/falsifier | permissions/grant | deadline_priority | status + owner/trigger`.

- **REUSE_NO_CHANGE** if current behavior is proven and already used by real consumer; never build duplicate implementation.
- **IMPLEMENT_MINIMAL** if bounded diff makes a tested (not coverage-scored) mechanism observable; priority: E1 exact pair/loss/ordinary checkpoint and RP02 neighbor consumption.
- **EXAMINE_CAUSALLY** if intervention and comparable sources/controls are available: preregister metric/panel/failure threshold, then execute under separate grants.
- **NEEDS_SOURCE** if a meaningful primary/consumer link is missing. Assign to existing Rxx or scoped 00A; do NOT block unrelated code.
- **DEFER_OWNER_TRIGGER** for S00–S10/DTC/IDE/extras where needed later; never silently drop.
- **REJECT_OR_REOPEN** only with cited conflict, owner decision and exact competing hypotheses.

Prioritize by `expected information gain / effort / resource burden` with explicit legal/consumer feasibility, not by file count or number of passed checks. No automatic R26 and no extra 30/50-pass self-review loop. One owner serializes overlapping source writes; research may run concurrently, but one mutable Student pair/update at a time.

## 5. Scientific and engineering checks are different

**For learnable behaviors:** freeze lawful source-family splits TRAIN/OP_DEV/CERT, law versions and heldout, compare corrected no-update to one update; for E2 isolate mapper/representation/neighbor-use/metric/k/ANN independently with same candidate universe and solver. Judge subject accuracy, law errors, nontrivial negative families, domain confusion, routing/path regret vs *enumerated lawful candidates*, abstention, bulk-to-bulk transfer/retention, quality vs total time/PC/teacher/reindex cost. Record N and source-family uncertainty. NEVER use CI percentage as proof.

**For deterministic invariants:** narrow local checks for eligibility/permissions, type/schema associations, loss masks, order/cancel, journal/no-repeat, rollback/reopen and original branch-native requirements when relevant. Do not disable existing required branch rules, but don't initiate unrelated Windows runs to answer «научился ли Student?».

Verdicts scoped: `NOT_MEASURED | INVALID_EVIDENCE | INCONCLUSIVE | REGRESS | IMPROVE`; delivery separately `NOT_IMPLEMENTED | IMPLEMENTED_NOT_OBSERVED | OBSERVED_NOT_ADOPTED | ADOPTED`; exact independent review separately `NOT_PERFORMED | NEEDS_FIX | PASS`.

## 6. Locked DPO human/teacher roles

CODEX GPT-6 Astra UltraCode chooses lawful weakness/task (human decision); **admitted external open Chinese model authors BOTH final chosen/rejected for same x**; separate CODEX Astra UltraCode invocation judges EXACT pair; XIIGEN Student/its authorized mutable state learns; independent blind evaluator measures Student utility; separate Astra architecture review assesses exact final diff/SHA. No implicit model calls, no Student-origin rejected relabelled as Chinese, no closed exam leakage or provider/license guessing. DPO reference = frozen policy, not judge. No replay consumed pair even after rollback.

## 7. Source entrypoints and final integration

- [Primary original task](../original%20task.md), [WorkLeft-02](../09.09.26WorkLeft-02-teaching-chinese-model.md), [all WorkLeft/RAG](../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [original code slices](../Kiro-Xiigen-Original-Slice-Queue.md).
- [previous rounds thinking](<previous rounds thinking results.md>), [results/R01–R25](results), [join](PARALLEL-R01-R03-JOIN.md), [PLAN-25](PLAN-25-RUNS.md), [R18](R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md), [R22](R22-CAUSAL-EXPERIMENT-CATALOG.md), [R23](R23-PC-STATE-FEEDBACK-LIFECYCLE.md), [R24](R24-CAUSAL-REPAIR-2026-10-13.md).
- Other planning roots **G**=`llm mvp/dpo legal build`, **F**=`llm mvp/llm dynamic flow`, **V**=`llm mvp/visual studio & visual studio code continue`; these are NOT inside D.
- Core `lubochka/llm_mvp_core@codex/dpo-implementation` and actual `docs/skills/`. RAG link→actual primary→actual current code→ordinary consumer receipt. Directly read changed file fully and callers before any code patch.

**Next actionable work:** start RP01 E1 small code path, and in parallel RP02.00A (actual domain/memory/consumer bind) + RP02.00B1/00B2 law panels. Do not wait for all 25 result verdicts to become DONE. No actual training/PC result is claimed by this strategy.


## RP07 ownership — Builder learns to teach, original S07 preserved

[RP07 workpack](Kiro-Xiigen-RP07-Builder-MetaLearning-Slice-Pack/README.md) is **not** a reissued old S07 and not a sixth Student trainer. Unlike RP04's learned graph route, RP07 examines the distinct **Builder B0→B1** parameter change caused by lawful downstream Student utility and the second-round ability to select/build a more useful **new** lesson. Its five arms, two rounds and original six Student subjects are retained from historical S07, with frozen B0 control, source-family-disjoint evaluation and full PC accounting. Published RP06 owns actual DTC/IDE transport and RP08 owns seven-law hybrid transfer attribution; RP07 reuses those artifacts without a second implementation. Requirements and S07 historical 44 tasks retain their owners. Planning receipt does not establish Student or Builder gain, E1/E4 state mutation or independent Astra acceptance.

## RP13 — отдельный селектор причин и targeted learning authority (published DRAFT)

[RP13 code-slice workpack](Kiro-Xiigen-RP13-Causal-Hybrid-Policy-Control-Slice-Pack/README.md) содержит 16 ограниченных prompt/task packets. Он продолжает R04/R06/R07/R12/R13/R21–R25, но **не** заменяет результаты R01–R25 или уже выделенных RP01–RP09: RP13 определяет каузальный источник ошибки по frozen lawful route choices, какой state owner (RP01/02/03/04/07 или существующая отдельная policy) действительно нуждается в допустимом update, а когда требуется abstain/NO_UPDATE. При прочих одинаковых условиях сравнивает детерминированные ограничения, learned policy и hybrid, доказуемый ordinary consumer и full-PC цену. Кодовых изменений, GPU run, Student/selector training или independent Astra review этим пунктом не произведено.


## RP12 — source-bound causal credit assignment and minimum legal corrective algorithm

**[Published 15-task RP12 implementation-plan pack](Kiro-Xiigen-RP12-Causal-Repair-Selection-Slice-Pack/README.md)** ([execution board](Kiro-Xiigen-RP12-Causal-Repair-Selection-Slice-Pack/EXECUTION-BOARD.md), [manifest](Kiro-Xiigen-RP12-Causal-Repair-Selection-Slice-Pack/PACKAGE-MANIFEST.json)) is a new, independent diagnostic/repair-selection slice, NOT historical S12, RP09's evaluator or an RP10/RP11 completion claim. It takes RP09's lawful independent error/bulk observations and actual ordinary user call traces; separately attributes failure to domain multi-axis mapping, semantic tokenizer/vector/projection, kNN real multi-neighbor usage/dynamic k, L2 typed Unit, L3 correcting Flow, L4 lawful async Orchestrator, L5 DynamicGraph motifs, Student exact DPO/response-only LoRA or accepted-state serving. Existing RP01–RP09 product owners retain their actual code and effects responsibilities; RP12 does not create a second trainer/runner/router/Builder. R01–R25 statuses/owner shards remain unchanged.

**Launch without a hard waterfall:** RP12.00A source+RAG→primary and 01A one lawful C# or EN↔RU passport are independent read-only starts; 00B binds actual consumer; 01B preregisters one factor; 02B or 02A diagnoses first-hit or semantic loss; 04A selects minimal lawful fix; 05A is the first real frozen Student causal trial (no teacher/GPU DPO prerequisite). RP12.03A and 03B examine exact pair and feedback separately. RP12.05B is an optional separately authorized real Student unique-pair bulk. RP12.06A is cold normal consumer adoption and 06B is separate actually invoked exact-SHA Astra UltraCode examination. No full Windows GitHub CI or code coverage metric is a proxy for domain-law learning gain; focused deterministic rights/masks/state/type/rollback checks continue to protect correctness.

**Evidence boundary at publication:** DOCUMENTS_PUBLISHED / K_CODE_UNCHANGED_BY_THIS_PACK / DPO_UPDATE_NOT_PERFORMED / ACTUAL_GAIN_NOT_MEASURED / INDEPENDENT_ASTRA_NOT_PERFORMED. A valid NO_CHANGE/REGRESS/INCONCLUSIVE experiment is not converted into a fake successful training step.
