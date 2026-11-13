# RP05 V4 — реальный путь к B1, предметные законы, минимальные проверки

**2026-10-14 · ACTIVE documentary execution repair, NOT_IMPLEMENTED_BY_THIS_DOCUMENT, STUDENT_NOT_TRAINED_BY_THIS_DOCUMENT, ASTRA_INDEPENDENT_REVIEW_NOT_PERFORMED.** This repair is additive to the 14 full original task cards and supersedes V2/V3 execution shortcuts if they conflict. RP01–RP04/RP06 keep their algorithms and ownership; historical S05 is remote Git/LFS, not RP05 continual learning. R01–R25 are reusable research inputs, not 25 gates.

## 1. Подтверждённые расхождения (не предположения об улучшении)

Code source: `lubochka/llm_mvp_core@codex/dpo-implementation`, inspected HEAD `979256ad682e76925286bd6a7365f9fa773959cc`. Re-pin to live HEAD for implementation, and read the *entire changed file*, interfaces, callers, consumer before editing.

| ID | Actual evidence | Why previous instruction unsafe | Task-level correction |
|---|---|---|---|
| V4-F01 | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` blob `22e9397`, method `ExecuteCapturedPreparedAsync` near line 769: `request.DpoStepCount != 1` refuses prepared request. Generic `14.ContinuousLearning/Training/DpoTrainingExecution.cs` blob `987abd2` validates configurable `DpoStepCount`. | V3 says one DPO pair may have 2+ steps, but that is not a proof that this **particular prepared production path** can execute them. V2/original also says “precisely one Step” without separating transaction vs optimizer step. | RP05.00B explicitly classify actual entrypoint; RP05.03A default to the supported, authorised prepared single-step route if it can produce a lawful first B1; multi-step requires separate caller/contract/risk review, no silent guard removal. One lesson ≠ one atomic row ≠ one training transaction ≠ one optimizer step. |
| V4-F02 | `14.ContinuousLearning/Feedback/FeedbackRouter.cs` blob `a676058`: EDITED stores user correction as chosen and model response as rejected; REJECTED uses no-preferred sentinel. | Generic feedback pair is not final externally OPEN Chinese BOTH-sides DPO material for governed S04/RP05; receipt names/identity can otherwise disguise the author. | RP05.01A/01B treat feedback as observation/weakness; RP05.02A requires distinct CODEX human-case selector, approved external OPEN Chinese model author **BOTH** final chosen and rejected, distinct CODEX Astra 6 UltraCode exact immutable pair judge, then separate post-update evaluator. |
| V4-F03 | `14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs` blob `a46a003`, `SelectAtomicTrainingUnits` near lines 1582–1620: group with `CriticUnit`+`ImproverUnit` must be atomic with **two** stored rows. | A blanket “exactly one pair row” could split an atomic logical lesson or violate journal/provenance. | RP05.02B/03A must bind actual lesson/group ID, constituent pair IDs, request ID, transaction and replay ledger; one authorised lesson attempt means one bounded transaction for that admitted unit/group, not forcing row_count=1. |
| V4-F04 | `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` blob `2957816`: `Search(query, _k)`, but selects `hits[0]` for the learned output near lines 223–230. | Merely obtaining many neighbors is NOT learned law-conditioned fusion or adaptive decision. | RP05.04B measures lawful typed output; RP02 owns actual change. Distinguish retrieval eligibility from action admissibility; retain lawful negative/warning memory as contraindication, never execute illegal neighbor because it is close. |
| V4-F05 | R22 §3.5 proposes PILOT-P12: 12 independent C# source families×2=24 cases, 72 base P/C0/T outputs +24 extra boundary-seed outputs = **ceiling 96**. | This is an **unapproved prospective scientific profile**; calling it a required gate or a measured 95% result fabricates authority. | Freeze/owner-bind prospective panel before seeing candidate. C# first B1; EN↔RU and SQL pilots start in parallel on **their own actual RP02 recipients**, never pooled as Student gain. |
| V4-F06 | `00.Core/LearningMachineContracts.cs` blob `7f605d4` has actual `ILearningMachine`/orchestrator hierarchy; original design names ILearningUnit/Flow/Orchestrator/Graph specify **roles**, not automatically four new trainable interfaces. | Document-only architecture may look installed; static routing may be reported as learned. | RP05.04C traces actual contracts, trainable parameter owner, law gates, intervention, consumer and independently measurable path delta. RP03 Unit/Flow; RP04 async Orchestrator/Graph; RP05 scientific joint cost. |

**Source depth limitation:** We directly inspected the cited code fragments, root RP05 files, 14 task cards, R22/R23/R21, WorkLeft-02/MECH/RESEARCH opening, original task and a scoped RAG map. We did **not** audit every archived RAG primary or all 5,030 lines of the pair store; executors must read full code when editing. No actual model call/training/independent CODEX review occurred here.

## 2. Сначала реальная способность, потом расширение

**REUSE-FIRST FAST B1:** If an already lawful, consented, durable user weakness is present with full source-family and domain provenance, `01A` may return `REUSE_NO_CHANGE` and `01B` may admit the exact novel lesson without building a new generic feedback UI or database. Where source/provenance insufficient, implement only one narrow safe typed observation. Domain tokenizer/kNN and Unit/Flow/Graph from RP02–RP04 are parallel and not prerequisites to the first supported B1 update. A prospective early study may use a smaller independent panel than R22 PILOT-P12 **only with a pre-T owner-approved power/noise/cost and claim-scope decision**; report early result as exploratory if low precision. Never choose scoring thresholds after T.


```text
F0 [parallel read/science] 00A(partial) || 00B(actual CodeDoc route) ||
   03B(seal pre-result panel) || 04A(PC preflight) || 02B(parent/journal read)
F1 01A(feedback as observation) -> 01B(law/novelty/consent)
F2 02A(CODEX case selection -> authorised Chinese BOTH final sides ->
   distinct CODEX exact pair judgment)   [only after effect-specific grants]
F3 03A(accepted real B0 + supported step-count path + ONE new lesson/group
   + rollback reserve -> one bounded exclusive B1 transaction)
F4 03B(independent matched P/C0/T + old-family retention + all-in PC) ->
   cold real CodeDocumentation selected digest/output -> 06A(exact reviewer)
Parallel: 04B -> RP02 law/vectors/kNN; 04C -> RP03/RP04 Unit/Flow/Orch/Graph;
          05A/05B -> RP06 DTC/IDE real cold user, no shared gate.
```

**Critical principle:** preparation can run without B0 or paid/model grants, but claim `STATE_CHANGED` requires observable exact effect; B1 mutation needs accepted B0 (or independently approved lawful initial parent path) and separate granted effects. No auto-replay after UNKNOWN optimizer effect. F1–F4 do not wait for seven domains, all R01–R25, all 14 RP05 cards, Windows Actions, or 40+ family confirmation. If first legal lesson yields negative/inconclusive quality, that is scientific evidence, not an instruction to alter sealed expectations.

## 3. Четыре разные роли и комбинированный алгоритм

- **Unit / ILearningUnit design role ↔ actual ILogicalUnit contracts:** `domain + input/output type + semantic vector/codebook -> many inputs -> many typed and linked output associations`. Require proof of actual typed output consumer and mutable owner if claiming learned adaptation.
- **Flow / ILearningFlow design role:** corrects association between Unit input/output refs (including many-to-many/consistency) with versioned trained state. A hard-law/type guard is deterministic; learned correction is a different factor.
- **Orchestrator / ILearningOrchestrator design role:** bounded asynchronous fanout/order/cancel/early-stop, legally permissible branches and real total cost of unsuccessful paths; no illegal faster route counts as success.
- **DynamicGraph / ILearningDynamicGraph design role:** learned higher-order associations among patterns, including unseen composed families; static graph edges are not learning.
- **ILearningMachine/Student** owns actual DPO/LoRA mutable trainable state. **LoRA weight update does not itself define a lawful semantic token**. Semantic encoding/codebook and output projection must be tested with their own versioned invariants; neural/LoRA projection is evaluated as a *separate factor*. Never invent extra incompatible interfaces just from historical naming.

## 4. Законы доменов — индивидуальные экзамены, не одна смешанная accuracy

| Domain, actual owner | GOOD example and law | BAD/negative and WHY invalid | EDGE; actual scored result |
|---|---|---|---|
| **C# CodeDocumentation / RP01+RP05** | Explain `s?.Length ?? 0` from actual C# AST/version: ?. optional member; ?? fallback | “?. returns zero” confuses operators; prohibited false source claim | Nullable overload, differing compiler versions; source-family essential-claims accuracy, new/old retention, cold served output |
| **EN↔RU / RP02** | `no more than five` → `не более пяти`; feminine antecedent retains correct linked adjective/pronoun agreement with role/syntax | `не менее пяти` changes inequality; nearest masculine form violates referent constraints | Ambiguous antecedent/animacy; abstain or clarify; predicted vs oracle domain, agreement/negation error by independent family |
| **SQL / RP02** | Correct NULL/UNKNOWN and dialect-aware `NOT IN` vs `NOT EXISTS` on actual schema/cardinality | Treat as universally equal even if NULL appears; false 3-valued logic | No dialect/empty subquery/duplicates; law-conditioned retrieval/action veto, execution-backed truth |
| **Angular documentation / ordinary serviced version** | Distinguish `(click)` event from `[value]` property binding under actual Angular version | `[click]` automatically event handler; conflates syntax | Template aliases, Angular version transitions; independent versioned component families |
| **Orchestration / RP03/04** | DAG A→B executes B only after legal A success; cancellation accounts effects and losing branches | Fast illegal branch or irrecoverable side effect reported as best path | Cycle/timeout/permission; lawful route validity, bounded regret vs best available lawful route, total latency |
| **Figma/Canva→HTML/CSS / RP06** | Source semantics/order/RTL, accessible DOM and 320/768/1440 responsive render in installed user client | Only 1440 screenshot matches while keyboard focus or 320 layout broken | Missing font/private asset; separate actual cold host utility and law rubric |
| **DPO Builder / RP07** | CODEX Astra UltraCode chooses case; Chinese BOTH exact final sides; second CODEX Astra judges same x/context with meaningful law contrast | Cosmetic rejected, user-authored rejected labelled Chinese, or pair judge secretly edits both | TIE/BOTH_BAD/license unknown; exact-pair admissibility and downstream Student gain are distinct measures |

For kNN all domains use **two gates**: `retrieval_eligible = rights/tenant/split/schema` and `action_admissible = hard domain law/precondition`. Eligible negative/warning retrieval must be available as *counterevidence*; only lawful final actions execute. The function `k = f(domain law, density, score margin, uncertainty, eligible-pool count, PC budget)` is a *hypothesis*, not a magic fixed function. Separately ablate (i) top1 vs truly aggregated fixed-k, (ii) law-weighted reranking, (iii) adaptive-k, (iv) exact vs ANN (with recall), (v) vector tokenization/version and (vi) LoRA vs semantic projection. Match the lawful pool, domain oracle/predicted classifier mode, latency and recipient.

## 5. Минимум engineering tests — не метрика learning

| Change kind | New product tests, default | Evidence that DOES matter |
|---|---:|---|
| 00A/00B source and code trace, 03B prereg, 04A telemetry, 04B/04C RP05 science, 05A/05B read-only, 06A documentary review | **0** | Exact source/consumer link, preregistration, lawful blinded independent family outputs, counters/falsifiers |
| Feedback/rights/novelty/material admission changes | **0 unless current changed-risk is uncovered** | Focused consent/tenant, no fabricated Chinese authorship, same x/context, pair/hash/mask/recipient refusal |
| Trainer request/step guard, optimizer, journal, rollback, cold selected pointer | **0 unless current changed-risk is uncovered** | One named supported transaction; crash-before/after unknown, group atomicity, no replay, full restore and cold selected digest |
| RP02 representation/kNN or RP03/04 hybrid code | **0 unless changed deterministic contract is uncovered** | Focused type/law-rights/version/cancel checks; **real** domain-source-family evaluation and full resource ledger |
| Actual installed host native/release | Host's existing *mandatory* release/integrity checks still apply | Real cold user and installed version; Windows checks are not a universal scientific quality gate |

No test-per-task quota, no % coverage, no “rerun GitHub Windows until green” as RP05 first learning prerequisite. Engineering checks may protect deterministic safety; learning quality is nondeterministic/causal and MUST use the independent domain examination. Do not rewrite expected laws after seeing T. Count actual time/cost of learner, examiner, Chinese author and CODEX roles, failed branches, RAM/VRAM and rollback storage. If unavailable: `NOT_MEASURED` and exact blocker, not fabricated numbers.

## 6. Freeze sheet before seeing T (per actual recipient)

```yaml
experiment_id: required
parent_sha: required_before_T
corrected_no_update_sha: required_before_T
treatment_training_input_digest: hidden_from_blind_examiner
real_ordinary_caller_and_recipient: required
domain_law_version_and_sources: required
train_family_ids: required
dev_family_ids: required
sealed_family_ids: required_different_source_families
chosen_rejected_author_receipts: governed_CODEX_chinese_CODEX_or_NOT_INVOKED
predeclared_one_factor: required
decoder_seed_and_budget: required
critical_law_error_policy: required_before_T
old_family_retention_policy: required_before_T
minimum_useful_effect_and_uncertainty: owner_approved_or_DESIGN_BINDING_REQUIRED
full_PC_cost_limit: required_before_effect
raw_family_outcomes: required_after_exam_or_NOT_MEASURED
verdict: IMPROVE_or_REGRESS_or_INCONCLUSIVE_or_INVALID_EVIDENCE_or_NOT_MEASURED
```

R22 PILOT-P12 is a *proposed* early C# profile, **not an absolute quota**; use approved noise/resource profile if present. Pair family scores (wins/ties/losses, weighted equally by independent source family), abstentions in denominator under preregistered rubric, preserved critical-law and retention constraints. `P→C0` is engineering change; `C0→T` is learnable effect. For hybrid compare `D` (deterministic law/hard guards), `L` (learned policy with always-required legal guards), `H` (hybrid) with same candidate/data/budget. Report errors of best available lawful path and all wasted branches; unimplemented arm = NOT_MEASURED, not failed test.

## 7. Reviewer acceptance and finish line

For every one of 14 `tasks/RP05.*.md`, reviewer needs exact prompt, context, skills actually read, RAG→primary, actual caller/consumer, GOOD/BAD/EDGE+WHY, recovery, bounded children, smallest changed-risk verification and scientific result/falsifier. Check task V4 direct opening against original lower body: *no active contradictory order* for single-step/multi-step, group row cardinality, Chinese DPO authorship, blind evaluation or Windows gate.

Four independent certificates: (1) **IMPLEMENTED** method+real consumer (2) **STATE_CHANGED** exact update owner/checkpoint/journal (3) **LEARNING_MEASURED** lawful frozen P/C0/T + old retention/full PC (4) **COLD_ADOPTED** ordinary user reads selected state. None can inherit a PASS from another. CODEX Astra UltraCode **independent code/architecture review** requires a real separate invocation and final exact candidate SHA; this document's author review never claims independent PASS.

Publication review: count task files and manifest, refetch changed files at final Git commit, report SHA, diff scope and truthful `STUDENT_NOT_TRAINED`. Planning-only Git write authority is granted; model training/paid author/judge/host effects are separate actions with separate grants.