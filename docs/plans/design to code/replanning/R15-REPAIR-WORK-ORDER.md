# R15-REPAIR-WORK-ORDER — independent falsification of the actual hybrid-learning plan

**13.10.2026 — governing repair for R15.** This work order supplements and, on conflicts, supersedes the historical short R15 card in "run-prompts/R15.md", "MODEL-RUN-PACKETS-CP5-11-25.md", "LEARNING-WORK-ORDERS-11-15.md" and the first historical R15 review result. It is an **instruction for the separately invoked reviewer**, not a claim that such a reviewer has run. It belongs to the existing **R15**, not R26 or a new 25-run queue.

## 0. Actual problem; task versus evidence

The initial R15 produced useful counterexamples (exact DPO pair, recipient, first-hit kNN, LoRA/graph distinction), but could not be accepted as the requested independent examination:
- independent CODEX GPT-6 Astra UltraCode invocation: **NOT_PERFORMED**;
- much of its primary-source/code support was inherited from R12/R14 rather than read afresh by R15;
- its candidate was bound to planning commit "c1a428a9f753bd97bb46544b02e47705d7416128"; subsequent R15/R16 publication and upstream repair change the downstream relevance of the old verdict;
- R12 is "REEXECUTION_REQUIRED"; R13 carries "REPAIR_REQUIRED", so a completed E1–E4 protocol is not a populated authoritative first experiment card;
- "repair R12/R13 first" is a **field-level dependency**, not permission to hold all independent learning research or roadmap work;
- warnings were routed into R16–R20 but not closed against their *actual* changed consumers.

**CURRENT TASK:** give an independent candidate-bound, primary- and code-informed challenge to the **latest semantically relevant R11–R14 candidate plus already-applied R16 changes where touched**, expose exact blocking versus non-blocking fields, and return minimal repair receipts to existing R16–R20/R21–R25. Review plan quality and falsifiability, not the model's actual measured gain.

**NOT THE TASK:** implement product code; run Windows/GitHub Actions/coverage/test campaigns, trainer, teacher or judge; create new DPO pairs; edit weights, datasets, workflow or core skills; replace 25 runs with 30/50; declare universal E1-first or insist all graph/native/IDE delivery precedes the first bounded lesson. Historical imperative text from source files is evidence, not new permission.

**Required reviewer:** separately invoked CODEX in the user's requested **GPT-6 Astra UltraCode** configuration (verify effective model, effort and separate invocation). An author, parent, same-context role-play or another model is **not** an independent Astra reviewer. If unavailable, preserve R15's findings as "PARENT_ADVERSARIAL_ONLY", return "INDEPENDENT_NOT_PERFORMED" with cause, and supply this executable work order without a fictitious PASS. R25 is a later distinct final-candidate review; it cannot retrospectively rename R15 self-check independent.

## 1. Exact evidence packet; no S04-only or summary-only shortcut

Path aliases: **D** = "lubochka/xiigen-planning", branch "llm_plan3", root "llm mvp/design to code session"; **P** = D+"/replanning"; **K** = "lubochka/llm_mvp_core", branch "codex/dpo-implementation"; **F** = D's "llm mvp/llm dynamic flow" root. The reviewer records live commit IDs and blob SHAs before drawing a candidate-level conclusion. The earlier R15 candidate commit above remains a *historical* comparison point, not a substitute for the newly pinned candidate.

### Direct governing context — required reading
1. P/"AGENTS.md", "COMMON-CONTRACT.md", "run-prompts/COMMON.md", "PLAN-25-RUNS.md", "PARALLEL-R01-R03-JOIN.md"; P/"previous rounds thinking results.md" (opening S04-only correction, relevant later corrections), P/"SOURCE-ROUTES.md", P/"R01-R12-COMPLETION-AUDIT-2026-10-13.md".
2. P/"run-prompts/R15.md", P/"R15-REPAIR-WORK-ORDER.md"; P/"results/R15"/all five historical carriers; P/"results/R11"/all consequential decisions + status; P/"results/R12"/FINDINGS, DECISIONS, CHANGE-MAP, RUN-LOG and R12-REPAIR-WORK-ORDER; same for R13/R13-REPAIR-WORK-ORDER and R14/R14-REPAIR-WORK-ORDER. Re-read R01–R10's **current** relevant findings/limits, especially R04 representation, R05 DPO token/objective/adoption, R06 domain/kNN/graph, R07 causal repair, R08 real domains and R09 measured process-cost boundaries. An Rxx published prompt is not executed Rxx repair.
3. **Direct primary reads**, not just citations inside sibling reports: D/"original task.md" (four roles and asynchronous alternatives), D/"task prompt.md", "goal of the round.md", "Алгоритм создания ДПО.md" (the *current R16 top contract* takes precedence over conflicting historical claims), D/"planning of testing.md"; D/"09.09.26WorkLeft-02-teaching-chinese-model.md", "09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md" (seven domains and 95%-to-reference goal), "09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md" (current R16 preamble), "09.09.26WorkLeft-MECH-01-mechanism-validation.md" (live-cycle gaps), "09.09.26WorkLeft-PLAN-HUMAN-01.md", "09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md". Follow **consequential** current RAG MASTER/DELTA and DPO/DTC/IDE MD+NDJSON routes from R01/R03 to one primary per disputed obligation. If a route is unresolved, preserve its exact locator, owner and dependent claim; do not infer absence.
4. F/"DECISIONS-LOCKED.md" and "ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md" for semantic compression, StackDetector-before-normalization, shared base/project LoRA, BM25+kNN, positive/negative/warning memory, domain/metric authority, meaningful pattern order, final arbiter, older replay rule versus present no-hidden-replay. Source authority must be reconciled before importing an old target.
5. Scope inventory: S00–S10 and variants, non-slice obligations, all WorkLeft/LeftToWork/RAG, DPO/DynamicGraph, Design-to-Code, Visual Studio/VS Code/Continue/clients. Use R01/R02/R03 inventory for *navigation*; spot-check directly the critical original-only and deferred obligations. Preserve owner+trigger for other lines without a fresh 1.5MB re-audit.

### Exact code seams for adversarial source verification (READ ONLY)
The below were read during R15 repair on K/"codex/dpo-implementation"; the **same** code blobs were reported in R05/R06. A future independent reviewer must recheck affected spans on its pinned K ref and caller/consumer, not assume the blobs never changed.

| Mechanism | Current concrete locator | Question to try to falsify |
|---|---|---|
| DPO preparation | K/"18.CapabilityTraining/LlmIngredientLogicalUnits.cs", around "BuildBatch"/"Compact", lines 2269–2343, observed blob "cb8744fcbbcff99821852e789dc92163976c0ad7" | Raw admitted pair is compacted (whitespace and 480 characters/part) before Encode; is exact *assessed semantic distinction* actually optimized? Mask covers prompt too. |
| DPO objective/gradient | K/"11.DPO/DpoTrainer.cs", "StepVector", "ComputeLogProbs", "BackwardLogProbs" around lines 535–765, observed blob "9c358978075db8c22fe0125eaea9e89e831d5635" | Current score uses masked **mean** over included positions; is a response-conditioned preference objective explicitly chosen and consistently differentiated? Do not silently change public average inference scorer. |
| Ordinary learned consumer | K/"25.ModelServer/ContinuousLearning/CodeDocumentationRepositoryOwnedDpoTrainingEndpoint.cs"; K/"26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs"; K/"25.ModelServer/Local/LocalModelRepository.cs" | Does *ordinary generation*, not only trial evaluator/loader, read accepted Student checkpoint/adapter? Exact semantic recipient vs winning solver ID? Untraced is untraced. |
| kNN learner | K/"21.AgentInfrastructure/Logical/KnnLogicalUnit.cs", lines 102–178 and 201–256, observed blob "2957816a88aa9cdcd3020bb81be56b5dfff165e0" | Train fixes k; Execute searches k but returns "bestExample.Output" from first hit; other hits supply evidence. Where can dynamic-k or learned aggregation affect output? |
| Representation/search | K/"06.VectorSearch/HnswIndex.cs"; existing exact search, BM25 and domain routing paths from R06 source locators | Exact vs approximate/index family, lawful pool and feature/vector version; does chosen metric preserve domain distinctions? |
| LoRA | K/"10.LoRA/LoraLinear.cs", "10.LoRA/LoraApplicator.cs" and actual model consumer | Separate low-rank parameter update from tokenizer/codebook or semantically correct source→target projection. |
| Four roles | K/"00.Core/LogicalContracts.cs", "00.Core/LearningMachineContracts.cs" plus current L2/L3/graph runtime from R04/R06/R07 | Typed many→many result associations, actual trainer owner, Flow correction, Orchestrator async branch choice and Graph learned higher-order authority—not four renamed trainers. |

A code locator in a prior report supports **ATTRIBUTED_CODE_TRACE** only until re-read. Record "CODE_TRACED" only for actually inspected current code/consumer. No performance improvement is observed from reading code.

### Skills — actual K paths; load applicable sections, not names only
- Architect: K/"docs/skills/trainable-architecture-decision-SKILL.md", "human-readable-architecture-audit-SKILL.md", "authority-requirement-binding-SKILL.md".
- Planning/review: "phase-planning-SKILL.md", "plan-review-SKILL.md", "reference-inventory-and-source-map-SKILL.md", "claim-verification-SKILL.md", "output-skepticism-SKILL.md".
- Code review: "code-examination-SKILL.md", "generated-code-review-SKILL.md".
- Prompt engineering: "prompt-patch-authoring-SKILL.md", "context-package-design-SKILL.md".
- Experiment/communication: "experiment-design-SKILL.md", "improvement-measurement-SKILL.md", "output-readability-gate-SKILL.md".
Use current task boundaries in P/AGENTS+COMMON to avoid importing old 30/50-pass or test-count-first directives from large cross-project skills. Record actual opened sections, K ref, and applied rule.

## 2. Review A — whole-program authority and old→new preservation

**CURRENT TASK:** independently reconstruct the user's desired result and challenge whether the R11–R14 capability DAG and R16 applied requirements retain it. **NOT:** read every archived byte or replace R01–R10.

**Inputs:** §1 P/COMMON/JOIN/R01–R10 provenance; D/original/task/goal/WorkLeft RESEARCH/MECH; P/results/R02 requirement IDs, P/results/R14 old→new maps, actual current entrypoints "README.md"/"START-NEXT-RUN.md" and changed R16 consumers.

**Method / prompt:** For each essential original obligation build one row:
"source/ref/span → current authority → requirement ID → current plan/capability node → deterministic guard → learned role/state if applicable → domain & actual consumer → owner/trigger → functional examination → preserved/deferred/missing/conflict". Cross-check the **seven** WorkLeft domains (EN↔RU, orchestration, Figma→HTML+CSS, C#/Angular/SQL documentation, DPO Builder), additional source-supported languages/intents, mixed/unknown domain, the **three product lines**, S00–S10 and extras. Distinguish domain, project, stack, task intent and graph level. Show at least one *original-only* obligation and one legitimately deferred host-native obligation.

**Good:** "IDE/Windows delivery remains owned with trigger on crossing host boundary, but a server-only domain experiment does not wait for Windows." **Bad:** "all of S04 is covered, therefore whole-program PASS", or "deferred" with no owner/trigger. **Edge:** an original requirement lives only in historical archive; same requirement appears in two slices; an active RAG route is incomplete; R16 changed its consumer after R15 candidate pin.

**Verification:** every representative source family has source→expectation→consumer or named gap; unresolved routes block the *dependent* claim only. **Output:** "A-AUTHORITY-RECEIPTS" with strong counterexamples and R16/R19/R20 owners; no empty coverage checkbox.

## 3. Review B — four-role deterministic+learned causal architecture

**CURRENT TASK:** falsify the claimed connection among Unit, Flow, Orchestrator, DynamicGraph, LearningMachine, memory/index and arbiter. **NOT:** create a duplicate trainer or pre-require an unused graph stage for a unit-local lesson.

**Inputs:** §1 original task, F decisions, R04/R06/R07/R12/R14; K contracts and relevant code seams. **Prompt:** For each of four roles specify real inputs/outputs (typed many→many associations for Unit), domain/vector/version, lawful deterministic constraints, actual learned authority and trainable state, signal/objective, mutation owner (LearningMachine where actual), version/recovery, ordinary caller and falsifier. Draw two genuinely separate consumer chains: (1) unit-local CodeDocumentation DPO→ordinary generation; (2) domain/Flow/Orchestrator/Graph learned routing and higher-order pattern transfer. Name adapters/bridges rather than pretending one checkpoint automatically teaches the other route. Flow learns corrections/relations; Orchestrator learns asynchronous ordering/cancellation/budgeted alternatives; Graph higher-order transfer must be examined on **unseen lawful compositions** with lower-level behavior held fixed.

**Good:** "Freeze Units, change one Flow relation state and see whether the ordinary Flow consumer selects a better admissible transition." **Bad:** "one Student answer improved, hence DynamicGraph learned"; "randomly reorder branches = training". **Edge:** same solver serves multiple domains; selector ID differs from winner solver; arbitrary completion order; edge relation stored but not adopted; cyclic correction; hidden alternate route; incomplete IQueryable association.

**Verification:** for every role there is a possible disconfirming observation; absent state/consumer is a **design blocker** for that role, not evidence it cannot work. **Output:** "B-MECHANISM-MATRIX" and separately scoped blockers (first selected experiment vs full program).

## 4. Review C — exact DPO lesson, preference, recipient and adoption

**CURRENT TASK:** adversarially walk the proposed **E1** end to end while preserving existing authority and minimizing the patch. **NOT:** call teacher/CODEX judge/train, or treat pair validity as measured Student utility.

**Inputs:** current S04 RUN-CONTRACT §§2.1–2.3, current R16 DPO top contract, WorkLeft-02/RESEARCH-03/MECH, R05 code evidence and §1 direct code; R13 E1 methodology.

**Prompt:** Verify **CODEX GPT-6 Astra UltraCode chooses the case in the human role**; the **approved external open Chinese model authors BOTH final chosen and final rejected**; CODEX/Astra separately judges the same final projected pair; Student output is optional weakness context, not forced rejected; frozen reference policy is not a judge; post-update evaluation is separate. Record domain, rights, context, pair IDs, author IDs, chosen/rejected exact meaningful bytes, projection/tokenizer versions, response boundaries/masks, preference labels (including TIE/BOTH_BAD/UNJUDGEABLE), optimizer target, solver vs selector ownership, update state, no-repeat, snapshot/rollback and ordinary consumer version. Compare claimed DPO formula "−log σ(β[(logπ(y+|x)−logπref(y+|x))−(logπ(y−|x)−logπref(y−|x))])" with actual objective and symmetric forward/backward masks. A different explicitly justified length-normalized objective is an *alternative*, not silently standard DPO. A zero model margin when policy=reference cannot by itself reject a semantically useful pair.

**Good:** a 600-character near-miss pair differs only in a required cancellation clause; raw pair is judged but current Compact discards the differentiator → INVALID_EVIDENCE until lossless preservation/re-assessment; assign smallest exact R18 seam. **Bad:** "both strings signed, loss dropped, therefore teacher did DPO properly"; "CODEX authors rejected"; "old pair repeated as a new bulk item." **Edge:** tokenization boundary changes across prompt/answer join, meaningful whitespace, unjudgeable pair, crash after update before verdict, unseen ordinary consumer still on parent, wrong target level.

**Verification:** pair-origin, semantic preservation, response-conditioned policy/reference objective, exact state recipient and ordinary consumption are separately proven or named missing. **Output:** "C-DPO-TRACE", verdict per link and minimal code/protocol correction into R18/R20/R21/R22. All performance claims remain NOT_MEASURED.

## 5. Review D — domain laws, token/vector mapping, dynamic kNN and LoRA

**CURRENT TASK:** find where deterministic guards and learned decisions connect correctly, and where metrics/representation assumptions make gains invalid. **NOT:** claim a universally best k or tokenizer, build an encoder first by habit, or call LoRA semantic tokenization.

**Inputs:** WorkLeft RESEARCH-01 domain sections (translation, orchestration, visual layout, C#/Angular/SQL docs, Builder), R04 representation, R06 kNN, R08 domain passports, R12-D/E, R13 E2, F authority and K code.

**Prompt:** Select at least two *structurally distinct* legal domains and one mixed/unknown case. For each make a mini-passport: law/abstention, semantic features that must survive input→canonical→tokens→vector→index→output, source-family split, raw/semantic hard-negatives and multi-correct cases. Distinguish predicted vs oracle-domain routing while freezing solver/context; first-hit/fixed-k/current representation vs one learned rerank/aggregation/budget policy while fixing other factors; exact-search reference vs ANN indexing; candidate eligibility from score/metric/features/k bounds; multiple-neighbor actual consumer and final arbiter; past negative/warning memory and false-positive guard. A dynamic function should depend on query/domain features and a learned validated signal with indexed-backend compatibility, **not** just a hand-written k lookup. A small k can win; an unchanged baseline can win. LoRA requires source/target representation, learned parameter group, frozen base, update signal, projection consumer, version and rollback—an adapter size/loss is not semantic correctness.

**Good:** same representation and candidate pool, Unit frozen, compare fixed first hit against a learned lawful multi-neighbor reranker on SQL NULL/negation cases and record lawful solution path+time; external law label independent of training. **Bad:** k=8 beats k=3 after also changing tokenizer/index/solver; count eight hits while reading only hit[0]; equate low-rank matrix with semantically correct tokens. **Edge:** no lawful neighbor; domain classifier confidently wrong; index stale after projection; meaningful JSON/code whitespace lost; identical token vector for different laws; high recall but illegal retrieved example; warning+answer mistaken for abstention.

**Verification:** name eligible pool, vector dimensions/identity, metric/normalization, learned authority, nearest-consumer and independent falsifier per candidate; domain-law quality versus cost separately. **Output:** "D-DOMAIN-ALGORITHM-CONTRASTS" + R21/R22 precise receiving rows.

## 6. Review E — fastest real PC continual learning and honest measurement

**CURRENT TASK:** challenge whether selected E1/E2/E3/E4 is *actually the most informative next executable experiment*, whether it fits PC and whether bulk learning remains causal. **NOT:** make total system implementation or all seven domains prerequisites to one limited experiment.

**Inputs:** R11 first-lesson vs delivery DAG, R12 unresolved recipients, R13 E1–E4 protocol/current status, R14 candidate queue, WorkLeft RESEARCH-01/MECH; R09 process, R16 applied requirements, current S04/S05/S06 learning state/runs.

**Prompt:** Freeze a **four-row E1–E4 comparison** before looking at any future outcomes: main uncertainty, source law/panel, already reusable code, smallest missing seam, state owner/ordinary consumer, one variable that changes, frozen factors/no-update/memory-only controls, latency-quality and full PC (base/reference/adapters/optimizer/activations/encoder/index/graph/cache/snapshots/reindex/teacher/assessor/evaluator/failed branches) costs, effort/prerequisite count, failure-risk, information value, one winner and one fallback with rationale. If R12/R13 are incomplete, show *provisional* comparison and missing exact fields—not a fictional selected executable seam. Allow independent E2 diagnostic work to proceed while E1-specific recipient remains unresolved. Track time-to-first-lawful-learning-observation and time-to-first-accepted-improvement **separately**.

Paired within-panel baseline/challenger/no-update (and memory-only where confounded), held-out *independent source families*, frozen rubric+threshold before outcomes, worst-domain/regression/retention, domain mapping/retrieval/path/output separately, accuracy with denominator/uncertainty and speed by bulk (new lessons only). Operational panel used for adoption is not untouched final certification. Preserve WorkLeft 95% goal as goal, **not** current measured achievement or router confidence. Include feedback provenance/rights/poison/noise/conflict, exactly-once update, full-state recovery, disk continuation and accepted-state ordinary adoption. Flag as **INVALID_EVIDENCE** leakage, unequal memory/context/compute, changed objective + index simultaneously, assessor leakage, wrong consumer or duplicate update. Report **INCONCLUSIVE** when power/independent families insufficient; do not force positive from a judge score.

**Good:** E2 can be an early frozen-candidate diagnostic while DPO pair/objective remains unbound; one bounded hypothesis, no multi-week test campaign. **Bad:** choose E1 because StepVector exists; conclude best k/PC compactness from a single synthetic source family or adapter-only memory; force all incomplete R01–R13 DONE before preparatory work. **Edge:** unavailable approved author/judge, evaluation contamination, OOM, repeated pair, mixed-domain feedback, equal quality at higher cost, stale graph/index with restored weights.

**Verification:** ranking freeze or exact missing fields; experiments can falsify both hybrid and simple baselines; zero gain is allowed. **Output:** "E-CANDIDATE-RANKING", "E-CONTROLS", time/resource observation spec, exact R18 selection prerequisite and R23 owner.

## 7. Review F — independent opposition, disposition and consumer handoff

**CURRENT TASK:** produce a review of the actual pinned candidate, not a stylistic endorsement. **NOT:** silently edit candidate during review, invent a reviewer, self-approve revised text or certify unmeasured algorithm superiority.

**Inputs:** A–E returned evidence, direct primary source excerpts, exact reviewed candidate SHA and important R16 changed sections. **Prompt:** Start with the strongest coherent defense of the approach, then write at least one serious counterexample per critical **claim family** (coverage/authority, four roles, DPO, domain/vector, dynamic kNN, LoRA, effect attribution, PC feedback). For each object:
"issue ID → actual source/ref/blob/span and exact candidate claim → contrary source/code/example → observable falsifier → severity first experiment / full program → smallest bounded change → target live file/section and owner R16–R20 (or R21–R25) → outcome needed to close → accepted/rejected/open". Include objections that survive unchanged, plus justified accepted choices. Reviewer must explain which R15-O01…O10 historical issues were fixed, remain live, superseded by primary or cannot be decided, **not** only add a new list. Validate R16 actual application where relevant (historical "R15 not observed" in R16 log is time-local and is not proof of later R15 absence).

### Outcome matrix and reviewer honesty
- **ACCEPTED_FOR_BOUNDED_NEXT_ACTION:** exact selected subgraph/source/consumer/controls/budget and old-ID trace all have no unresolved design-blocking field; this is **not** measured gain or entire program approval.
- **NEEDS_FIX:** named design/actionable semantic defect; assign one exact receiver and distinguishing acceptance evidence.
- **INSUFFICIENT_EVIDENCE:** missing source/invocation/panel/consumer/independence, with scope-limited verdict and the needed evidence.
- **INDEPENDENT_NOT_PERFORMED:** the requested separate Astra invocation did not actually occur; parent research/repair stays **PARENT_ADVERSARIAL_ONLY**.
- Actual future empirical efficacy: **NOT_MEASURED**, until real training/functional examination.
At all times distinguish historical candidate review, current document repair, product readiness, whole-program completion, first-experiment readiness and measured gain.

**Good:** CODE_TRACED loss of pair suffix → one R18 seam with source, consumer and acceptance observation, R16 wording owner separately; actual reviewer identity recorded or not performed. **Bad:** "R15 complete because 5 CSVs exist"; "Astra PASS" without invocation; R25 declared complete; 25 extra reviewer cycles added. **Edge:** candidate head advances while reviewing → compare semantic spans and restrict old verdict or re-review affected bytes; only one lawful expert source family; two valid DPO objectives; untraced consumer; model alias unavailable.

**Verification now (documentary):** candidate+SHA and independent invocation evidence or explicit absence; citations with depth and counterexample for every material finding; A–E coverage plus old issues; blockers scoped to dependent field; unambiguous R16–R25 receivers; no learning efficacy claim and no product execution. **Future verification:** distinct R18/R21–R23 intervention on actual ordinary output, governed by R13 causal contract, not by coverage percentages.

## 8. Child work orders; no missing-context delegation

A–E are *subtasks within the one R15 invocation*, not five independent reviewers and not five extra runs. They may be sequential bounded analyses by the same actual independent reviewer; F integrates. Every delegated child receives an **expanded effective prompt** containing all of:

~~~text
TASK_ID: R15-A | R15-B | R15-C | R15-D | R15-E | R15-F
CURRENT TASK: exact matching section 2..7 above
NOT THE TASK: product code/training/CI or sister Rxx
REVIEWER_IDENTITY: requested and actual separate model, effort, invocation ref
CANDIDATE: exact pinned planning/core refs and reviewed source blobs
REQUIRED SKILLS: actual K paths+read spans from section 1, section-specific additions
CONTEXT: exact primary and current Rxx results for the section; quotes/snippets supplied, not only remote C: paths
ORIGINAL REQUIREMENTS: the section's named source-bound obligations and R02 IDs
RAG ROUTE: query -> MD/NDJSON index -> successor -> primary span -> affected requirement
WORK: numbered questions/actions and mechanism/experiment constraints in its matching section
GOOD / BAD / EDGE: matching detailed section's concrete examples
MENTAL SIMULATION: one lawful example and one falsifier, owner-to-ordinary-consumer
ALLOWED ACTIONS: source/code read, compare/diff, return-only reviewed evidence
FORBIDDEN ACTIONS: candidate write, core code, tests/Windows/Actions, teacher/judge/train, weights/datasets, force push
OUTPUT: matching A/B/C/D/E/F named packet, findings with source/issue IDs, limitations and receiver
VERIFICATION: documentary acceptance per matching section; future functional examination separate
HANDOFF: named dependent Rxx, minimum repair and exact test of that claim
CLAIM: DIRECT_READ / ATTRIBUTED / CODE_TRACED / NOT_MEASURED and reviewer independence
~~~

If one section exceeds context, split by **causal source/consumer** (e.g., C: pair/context provenance vs objective/masks vs ordinary adoption; D: domain law/vector vs kNN/consumer vs LoRA). Each split must copy the **complete** fields above, bounded actual sources, a good, a bad and an edge case and distinct output. Merely listing the split titles fails. The parent integrates within one R15 and may not fake reviewer separation.

## 9. Own five carriers and exact publication/readback contract

Only the genuinely invoked R15 reviewer writes or returns material for:
- P/"results/R15/SOURCES.csv": source/ref/blob/path/span/read depth; current/historical; reviewer actual vs attributed.
- P/"results/R15/FINDINGS.md": strongest approach, A–E counterexamples, first-experiment/whole-program distinctions, reviewed SHA and limitation.
- P/"results/R15/DECISIONS.md": each R15-O01…O10 disposition, additional only material issues, independent reviewer evidence and bounded severity.
- P/"results/R15/CHANGE-MAP.csv": issue → source/candidate span → smallest change → exact receiving document/owner → closure observation; incomplete not silently DONE.
- P/"results/R15/RUN-LOG.md": requested vs actual model/config/effort/invocation ID, snapshot, source depth, review work performed, no product commands, publication commit/readback and current verdict.

R15 reviewer returns a signed/identifiable *review packet* without editing the candidate it judged. A planning editor can later publish accepted bounded repairs to the allowed documents; changed candidate does not inherit an older PASS. R16–R20 edits need their own exact content readback, especially when they were already applied on an earlier branch snapshot. Do not write to sibling Rxx results. Preserve remote concurrent work using fresh blob SHA, no force push. The user has approved GitHub document writes; a tool failure does **not** mean "GitHub is read-only".

**Current repair success** means this model-ready work order and accurate R15 status/readback exist. **Independent R15 success** requires a real separately invoked requested reviewer. **Learning-system success** requires later actual experiment and observed ordinary behavior. Never merge these statuses.
