# XIIGEN — обязательный learning-first контракт для RP01–RP04: минимальные engineering checks, реальные экзамены обучения

> **SOURCE-OF-TRUTH CLARIFICATION (2026-10-14):** The canonical first-slice **per-task test-budget/dispatch policy** is [LEARNING-FIRST-MINIMAL-VERIFICATION-POLICY-2026-10-14.md](LEARNING-FIRST-MINIMAL-VERIFICATION-POLICY-2026-10-14.md), which is linked from all 57 RP01–RP04 task cards. **This document is a companion algorithm/causal-examination specification**, not a second approval stage, duplicate test quota, or competing task dispatcher. Where wording overlaps, honor the narrower canonical per-task policy plus applicable unchanged safety/native-release obligations. Neither document is evidence of executed model training or Astra review.

**2026-10-14 · ACTIVE PLANNING GOVERNANCE / NOT EXECUTED / NOT A GRANT TO TRAIN.** Этот документ исправляет причинную ошибку, когда `Windows GitHub Actions green`, число тестов или закрытый перечень подготовительных задач заменяет **изменение работающего обучаемого состояния и измеримый результат**. Применять к стартовым RP01–RP04 task cards и всем последующим слайсам через [32-slice audit](LEARNING-POINT-32-SLICE-AUDIT-2026-10-14.md), [execution board](LEARNING-FIRST-EXECUTION-BOARD-2026-10-14.md) и [52-requirement crosswalk](R02-52-REQUIREMENTS-32-SLICE-CROSSWALK-2026-10-14.md). Статус любых code/Student/teacher/experiments остаётся **NOT_EXECUTED / NOT_MEASURED**, пока нет отдельных настоящих доказательств. Исходные required branch checks/native-release обязанности НЕ удалены и НЕ считаются пройденными.

## 0. Priority and supersession — READ BEFORE TASK

1. **First learning point E1 is not gated by 32 slices, R01–R25 completion, S04 Windows runs, seven completed domain passports, RP02, RP03, RP04, RP06 or IDE release.** RP01 already contains a narrow critical path. E2/E3/E4 and RP09 scientific preparation begin independently when their own inputs exist. No task may silently reinstall a sequential 590-card prerequisite chain.
2. A generic skill, historical original S04 Windows validation task, old report, shared template or broad "full/impacted tests" wording **does not create a new test requirement for a distinct RP01–RP04 scientific learning task**. Existing externally enforced branch protection, applicable security/integrity tests and truly required Windows/native delivery checks remain valid **for their actual code/release boundary** and must be reported honestly. If such a real requirement blocks promotion, mark **RELEASE_BLOCKED**, not LEARNING_NOT_PERFORMED. Never falsify PASS or edit a test merely to turn it green.
3. **No extra engineering test for read-only, planning-only, already-proved `REUSE_NO_CHANGE` or domain-exam-design tasks.** Source-readback/link/manifest checks do not count as product tests.
4. For a changed **deterministic** boundary, select the *smallest risk-based set* of focused local assertions/probes that witnesses the exact changed invariant, commonly 1–3 targeted checks rather than an invented numerical coverage goal. More only if independently necessary to protect distinct real user/security/safety invariants; record why each cannot be subsumed. Include any already applicable existing focused regression for changed code; do not weaken it. No new blanket impacted/full Windows suite or coverage percentage gate.
5. For **learned/nondeterministic** performance, engineering unit tests, snapshots, assertion-count, green Actions, lowered loss, hash change, larger k, changed edge count and training-sample scores are **NOT** proof of utility. Use source-family-disjoint, precommitted law-grounded bulk-to-bulk experiments on ordinary user outputs. Independent graders, uncertainty, positive/negative cases, abstentions, regression and PC cost must be preserved, not adjusted after results.
6. Do not expand "validation" into repeated planning/approval loops. One producer→algorithm→consumer trace, one minimal mechanism intervention OR source-proved NO_CHANGE, one exact independent candidate review at the applicable acceptance boundary, one scientific comparison when an actual update is authorized. A separate CODEX/Astra review cannot be claimed without a real invocation and exact candidate identity.

**Actual authority:** GitHub editing of planning packets in `xiigen-planning@llm_plan3` is authorized by the current instruction. This document grants NO product changes to `llm_mvp_core`, rights to call external models, alter weights/datasets, spend money, execute CI or promote a model. Each effect needs its own actual stage/rights/budget, as existing task contracts say.

## 1. Hard evidence taxonomy: don't conflate machine operation with learning

| Evidence level | Real observable requirement | Verdict if missing |
| --- | --- | --- |
| L0 SOURCE_BOUND | D original + WorkLeft + relevant RAG→**primary G/F/V** + live core blob/callers + ordinary consumer + legal owner mapped; index alone ≠ read | SOURCE_UNRESOLVED |
| L1 MECHANISM_USED | Real ordinary input exercises changed deterministic/learned code path, output identity + trace | NOT_EXERCISED |
| L2 STATE_CHANGED | Specific authorized lesson/update modifies approved trainable weights/policy/memory (named keys, optimizer membership, before/after, step/journal) or honest NO_UPDATE | STATE_NOT_CHANGED |
| L3 CAUSAL_EXAM | Immutable heldout domain-law P / C0 / T or matched per-factor ablation, same corrected inference path and sources; blind rubric, N, wall/PC cost | NOT_MEASURED / INVALID_EVIDENCE |
| L4 COLD_ADOPTION | Accepted **complete** state survives new process/restart and produces a normal user answer with exact checkpoint/encoder/index/law provenance | NOT_ADOPTED |
| L5 INDEPENDENT_REVIEW | Separate real CODEX GPT-6 Astra UltraCode examination on **exact reviewed candidate** and actual evidence; author/selector/judge are not self-proclaimed reviewer | NOT_PERFORMED |

A valid experiment can yield **IMPROVE, REGRESS, INCONCLUSIVE, INVALID_EVIDENCE, or NO_TRAINING_NEEDED**. Do not relabel a negative or non-improving run as PASS to satisfy a test. An unsuccessful but informative first experiment can satisfy "first real scientific observation", **not** promotion to the user. The user has asked to reach the learning point quickly; no "must improve" threshold can be invented after the data.

## 2. RP01 first-lesson path: preserve model roles and avoid rebuilding current exact DPO

1. **CODEX using GPT-6 Astra UltraCode performs the human teaching-task selection**. Save lawful domain, target weakness, source/version/rights, frozen input x and budget.
2. **A specifically licensed/admitted external open Chinese model writes BOTH final `chosen` and final `rejected` for exactly the same frozen x/context**. The Student's failed output is contextual evidence, not automatically the rejected side. Do not label an actually correct deterministic answer as negative just because it was produced by a deterministic mechanism.
3. **A separate CODEX GPT-6 Astra UltraCode invocation judges the exact unchanged final DPO pair**: correctness, real contrast, same source law, rights, meaning, dangerous negative patterns, and TIE/BOTH_BAD/UNJUDGEABLE. An unauthorized pair is not trained. The independent *post-update evaluator* and independent *architecture/code reviewer* are different roles; never collapse the checks into one claimed call.
4. Trace actual producer → `DpoTrainingExecutionRequest` / `LogicalDpoTrainingRequest` MaterialVersion → `DpoExactPairMaterial` → response-only masks/objective → actual project LoRA optimizer keys → journal and accepted full state → shared ordinary text-generation caller. **Current exact segmented-v2 material and response-only objective exist in core**, while some request defaults still select legacy. Do not recreate exact tokenizer/trainer, swap global inference metric or change encoded bytes just because the old task says "fix DPO". Patch only the live selected lossy caller/contract if proved reached; otherwise **REUSE_NO_CHANGE**.
5. **Experiment P/C0/T**: P frozen parent/native baseline; C0 same corrected pipeline with *no* train update; T same corrected pipeline with **one lawful NEW** pair update. Freeze legal C# CodeDocumentation source families/splits and blind grading before mutation. Different source family train vs sealed exam; no sealed answers in teacher context. Measure lawful functional success, wrong-law failures, answer usefulness, latency p50/p95 where sample suffices, peak RAM/VRAM/CPU, disk/state/index, rejected runs, total time & cost per additional useful bulk. If T ≈ C0 or worse: keep parent, record failure/falsifier, do not test-fit training to green.
6. **Engineering probes only if relevant touched seam:** judged-byte/response-target preservation and no truncation; legal admission/LoRA key whitelist; idempotent one-pair journal + full-state hash/cold reload. These are mechanism-integrity guards, not a coverage target. Failure to load accepted state in an ordinary request is a real product gap even when scientific T gain was observed.

## 3. RP02 domain mapping → law-sensitive token/vector → multi-neighbor adaptive kNN

Use independent law source/version passports with train/dev/sealed source-family splits. C# nullable/signature and EN→RU negation/entity/number are first two **structurally different** families; additional Angular, SQL, Figma/Canva, Orchestration, Builder, Hebrew→English and eight original task types are retained in the 52-ID crosswalk with their own owner/trigger, **not gates to the first C# lesson**.

**Disentangle intervention order:** (a) mapper domain vs intent vs stack with UNKNOWN/MIXED and law-source checks; (b) semantic domain token/codebook vs fixed representation; (c) retrieval current `hits[0]` vs real lawful **multi-hit influence on ordinary typed output** with same pool/k; (d) fixed-k rerank with named learnable recipient/signal; (e) domain-query adaptive k/radius with feature/cost attribution; (f) exact/ANN comparison and rebuild cost. Never claim "multi-neighbor" merely because evidence IDs contain many hits or claim learned adaptive k when it is a static heuristic. Vector-similarity score, LoRA output and token/codebook coordinates are different entities. Law-incompatible, warning/negative, stale/tenant mismatched or unknown candidates are gated deterministically **before** learned preference score. 1–3 focused probes for changed index/schema/rights boundaries; *scientific* law accuracy, mapping confusion, retrieval recall, best lawful path, time and PC cost are separate evidence.

## 4. RP03/RP04: four distinct interfaces and a causal hybrid path

- `ILearningUnit`: domain/semantic vector/output type + typed **many→many** association `OutputId ↔ SourceInputIds`, 0..N candidates; deterministic eligibility and a named existing ILearningMachine training owner if actually found.
- `ILearningFlow`: resolve actual typed payload/OutputRefs/lifetimes, input→output corrections/edge preferences, measure downstream next Unit with Units frozen. A positional Zip or IQueryable candidate list is **not** proof of valid many→many correction.
- `ILearningOrchestrator`: asynchronous lawful fan-out/fan-in, cancellation, per-branch cost and typed dependency semantics; learned path selection if there is an actual mutable route-policy recipient. A faster deterministic scheduler is useful engineering, NOT demonstrated learning.
- `ILearningDynamicGraph`: lawful higher-order pattern composition, real learner owner/state/change/reload, heldout motif/path difference vs frozen lower-order policy; `DynamicGraphMachine` currently advertises NOT_ACTIVATED on examined source, so do not infer learned Graph from an interface alone.

For each intervention: freeze unchanged owners, present at least one **lawful good** path, a **plausible but invalid** path, and EDGE (unknown law / stale OutputRef / cancellation / same-shape wrong vector / TIE). Trace actual user-facing output. Compare fixed policy vs learned policy under identical eligible paths and cost budget; include all branches, retries, aborted runs. Keep existing mandatory effects/permission/tenant/safety/arbiter guards. Never create another ILearningMachine or DPO trainer to make a test pass.

## 5. Who may advance first, and what constitutes a stop

| Parallel work order | Start now on actual direct source | Smallest accepted result; forbidden closure |
| --- | --- | --- |
| RP01.00A/00B + RP01.01A–01C source trace | Existing primary/K selected caller; no second full library inventory | Exact-live-path REUSE_NO_CHANGE or minimal verified seam. No mandatory 00C-style sequential paperwork loop before source investigation |
| RP01.02A and RP09 scientific precommit | Lawful C# source families and usable ordinary evaluator | Frozen P/C0/T rubric, rights, split, budget; no training required to *design* evaluation |
| RP01.02B → RP01.03A | Separate real teacher/CODEX selection + exact judgment + staged permissions | One approved new lesson, true state delta and controlled outcome; honest authorization blocker otherwise |
| RP02.00A/00B1/00B2 and 02A | Law sources, typed consumer and 2 eligible examples; independent of RP01 | First ordinary multi-hit contrast even if adaptive k/encoder follow |
| RP03.00B/.00C/.02A | Actual typed producer/OutputRef/consumer with frozen RP02 representation | Verified many→many/lawful Flow baseline, independent of Student training |
| RP04.00A/.00B and E3/E4 owner trace | Actual existing orchestrator/Graph source and deterministic permissions | Named effect owner/consumer or evidence-backed missing seam; E3 and E4 independent |
| RP05/RP06/RP10/S23/S24/S25 | Read-only host/adoption tracing may start; actual promotion when accepted complete state exists | Keep ordinary DTC/IDE, restart and full-PC obligations named; don't block first E1 |

No required Windows "green" checkpoint at any of the first-learning gates. If a pre-existing branch protection requires an applicable check for a later **code merge**, identify and satisfy that concrete merge rule when needed, with focused applicable code validation. Do not dispatch more Windows jobs just to manufacture training evidence.

## 6. Task card mandatory MODEL-READY verification delta

Each RP01–RP04 task inherits this contract *at task start*, including when old body/skill template mentions wide preflight. Effective executor prompt must attach the contract's relevant section plus existing full task card. The executor's return packet MUST contain:

```text
task_id + exact full pack path; CURRENT / NOT / EVIDENCE_ONLY
P and K current HEAD; changed blob/line SHA; actually read original/WorkLeft/RAG->PRIMARY and source->method->caller->ordinary consumer; loaded skills
scope: READ_ONLY | DETERMINISTIC_CODE | STUDENT/LEARNED_UPDATE | SCIENTIFIC_EXAM | NATIVE_RELEASE
existing_impl: REUSE_NO_CHANGE | MINIMAL_DELTA + one concrete falsifier
test_minimum: [0 for docs/read-only; touched deterministic risk -> named focused property probe + why; native release required? yes/no, exact rule]
learning_experiment: preregistered law/families/train-dev-sealed; P/C0/T or one-factor frozen alternative; pair provenance; actual N; accuracy/error/retention/latency/full PC cost or NOT_MEASURED
STATE_CHANGED, SCIENCE_EXAMINED, COLD_ADOPTED, INDEPENDENT_REVIEWED as separate observed booleans/status
GOOD/BAD/EDGE with source-law version; failed paths/negative outcome; exact next owner/trigger and rights
```

**GOOD:** `RP01.01A` reads current exact-v2 code, discovers real caller already opts in and records `REUSE_NO_CHANGE`, zero new tests, proceeds to optimizer/cold-consumer evidence. **BAD:** rewrites already-correct DPO, adds 100 Windows tests, then claims learned Student by PASS. **EDGE:** caller still legacy, but exact core exists: change only caller/selected MaterialVersion and one boundary probe; C0 isolates impact from training.

**GOOD:** `RP02.02A` reports typed ordinary answer changed because two lawful neighbors influence it, while wrong-law candidate was excluded. **BAD:** result remains `hits[0]` and evidence IDs grow; increased k is falsely certified. **EDGE:** negative and positive neighbors conflict or domain UNKNOWN; preserve deterministic abstention and label INCONCLUSIVE.

**GOOD:** `RP04.01A` proves lawful bounded async execution with cancellation and all-branch accounting, clearly labels `ENGINEERING_GAIN_ONLY`. **BAD:** green test says DynamicGraph trained despite `NOT_ACTIVATED`. **EDGE:** timed-out branch returns after cancellation; cannot mutate accepted state or escape cost accounting.

## 7. Reviewer checklist and acceptance scope

Plan-review and code-review experts from K `docs/skills` continue to check architecture integrity, original requirement continuity, law/data leakage, permissions, exact final SHA and real consumer. **Skill guidance about phase-wide test coverage is interpreted as a risk inventory, not a new full-suite request for this learning-first slice.** Independent CODEX GPT-6 Astra UltraCode approval is only real if separately invoked with candidate evidence. Stop if a deterministic security invariant fails; otherwise negative performance evidence becomes the next causal hypothesis, not a campaign to tune test expectations.

**Publication scope:** planning artifacts on `llm_plan3` only. Inspect live remote after commit; no force update and no workflow dispatch. Do not claim training, measured efficiency, Astra examination or changed core from this document.
