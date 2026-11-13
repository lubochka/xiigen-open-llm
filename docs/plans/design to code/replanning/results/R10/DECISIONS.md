# R10/J03 — integration decisions and model-ready handoff to R11–R15

**13.10.2026.** These decisions apply only to the replanning program. They do not authorize product implementation, teacher/judge model calls, training, Windows runs or edits outside R10 carriers.

## D10-01 — Accept a partial nine-way join, not a false whole-research completion

R01–R09 are all observable. R10 may therefore integrate them, but must preserve their actual states. R07 remains REPAIR_REQUIRED; R08 remains IN_PROGRESS; R01/R03 traversal is incomplete; R05 remains OPEN. The correct product is a useful bounded join plus exact unresolved owners.

## D10-02 — Reuse the existing DPO/training path

Do not create another trainer. The first implementation candidate must start from the existing pair→training→trial/publication chain identified by R05 and repair only the necessary semantic projection/objective/adoption seams.

## D10-03 — Choose one first semantic recipient before implementation

R12 must select exactly one first recipient/state family and ordinary consumer. It must not mix document-model LlmModel, routing profile, kNN example/BPE state and graph edge-authority state unless a concrete producer→state→consumer bridge and matching objective are demonstrated.

Winning solver ID, logical unit ID and learning recipient are not synonyms.

## D10-04 — First-lesson critical path excludes unrelated Windows/native qualification

Keep deterministic/state/provenance checks that directly protect the selected lesson. Defer Windows/native qualification to the actual native delivery boundary unless R11 finds a causal native dependency in the selected path.

## D10-05 — Pair semantics must survive to the optimizer

A pair that is valid in raw bytes but loses the decisive distinction in projection is not a valid learning signal. R18 later must either preserve the distinction within the supported budget or reject/re-author/re-assess the lesson; silent semantic truncation is not allowed.

## D10-06 — Objective and evaluation are separate

The optimizer objective must be explicitly response-conditional if that is the chosen DPO contract. CODEX/Astra judgment decides whether the pair is a valid pedagogical preference; it is not the optimizer objective and does not prove Student improvement.

## D10-07 — Hybrid architecture is mandatory

For every mechanism, later plans must identify deterministic admissibility/safety, learned semantic authority, mutable state, ordinary consumer, fallback and falsifier. Correct deterministic outputs remain eligible positive answers.

## D10-08 — Dynamic kNN/vectorization/LoRA are experimental factors, not slogans

No universal k, metric, tokenizer or "LoRA transpose" claim is accepted. Each domain experiment must define the law-sensitive feature/token/coordinate hypothesis, state owner, consumer and matched comparison. Exact search is a preferred diagnostic baseline for small bounded pools.

## D10-09 — Learning acceptance uses controlled behavior, not coverage

Learning verdicts require matched parent/challenger/no-update panels, held-out families, mapping/solver/path separation, retention and resource/time measures. Coverage and CI remain mechanism/delivery evidence only.

## D10-10 — Bulk means distinct lessons

One-pair/no-repeat/full-state rollback remains. A bulk is a reporting window of sequential distinct admitted lessons, not replay of the same pair under new IDs and not permission to train multiple ablation branches on one lesson unless policy explicitly changes.

# Model-ready handoff packets

The next five runs are analysis, not implementation. Each packet below is bounded and may be expanded from the existing detailed plan books, but the following fields are mandatory.

## R11 — causal DAG and first-lesson cut-set

**CURRENT TASK:** Build the smallest dependency DAG from one permitted raw lesson through pair authoring/admission, projection, objective, update, publication, ordinary inference and independent examination. Identify the exact minimum cut-set that must be solved before one scientifically valid lesson.

**NOT THE TASK:** Do not implement the seam, run training/CI, optimize all domains, or repeat R01–R10 inventory.

**Required skills:** phase-planning, prerequisite-chain, data-flow-integrity, authority-requirement-binding, trainable-architecture-decision, plan-review, human-readable-architecture-audit, output-readability-gate.

**Context:** R10 FINDINGS/DECISIONS; R05 CP2; R07 bounded adoption findings plus current repair status; R09 process-cost findings; current R05/R06 code locators; original four-role and DPO role contracts.

**RAG/source rule:** Follow primary only when a specific DAG edge is disputed. Do not reopen the whole RAG tree.

**Output:** DAG with hard prerequisite / optional optimization / delivery-only / evidence-only edges; exact first-lesson cut-set; retained deferred owners/triggers; one human simulation; failure branches for unknown update, failed assessment and ordinary consumer still on parent.

**Good example:** "Response-objective repair is a hard prerequisite to this DPO lesson; DynamicGraph preload is not on the path for a document-model lesson; Windows PsecPhysical is delivery-only unless this consumer invokes that native boundary."

**Bad example:** "Finish S04–S10 and all Windows checks, then train."

**Edge cases:** pending applied pair; same bytes with changed context; child published but consumer uses parent; source unavailable; recipient changes during analysis.

**Acceptance:** every prerequisite has a causal consumer. No task is prerequisite solely because it occurs earlier in the historical queue.

## R12 — select recipient, state family and connection architecture

**CURRENT TASK:** Choose the concrete first semantic recipient and map lesson intent → state/parameters → objective → publication/registration → ordinary consumer. Resolve helper-vs-learned authority and any representation/index compatibility required by that path.

**NOT THE TASK:** Do not implement, train, or create a universal architecture for all domains.

**Required skills:** trainable-architecture-decision, service-boundary-design, data-flow-integrity, graph-backed-routing, code-examination, authority-requirement-binding, plan-review, no-product-decisions, output-readability-gate.

**Context:** R04 representation map; R05 CP2 trainer/state; R06 kNN/graph state; R07 adoption gaps; R10 decisions; exact current code callers as needed.

**Required decision table per candidate recipient:** state owner; trainable parameter group; objective; input representation; publication owner; ordinary consumer; rollback unit; domain fit; PC cost class; unresolved bridge.

**Good example:** choose repository-owned document LlmModel only if a real ordinary generation caller is found/defined and the DPO objective updates that exact state.

**Bad example:** "Train winner.UnitId and load weights into kNN because dimensions match."

**Edge cases:** selector and solver separate; same physical model hosts multiple semantic states; compatible tensor shape but incompatible coordinates; graph preload trusts stale version; LoRA adapter published but not selected.

**Acceptance:** exactly one recommended first recipient plus explicit rejected/deferred alternatives and evidence. No semantic bridge is inferred from naming or shape.

## R13 — first matched learning experiment

**CURRENT TASK:** Design one experiment that can causally decide whether the selected lesson improves the Student through the real ordinary consumer.

**NOT THE TASK:** Do not run the experiment, generate real DPO, or expand to the full curriculum.

**Required skills:** experiment-design, capability-measurement, improvement-measurement, score-interpretation, learning-loop-closure, planning-dpo-authoring, trainable-architecture-decision, output-skepticism.

**Context:** selected R12 recipient; R05 projection/objective seam; R08 law-sensitive experiment rules; R09 cost fields; R10 acceptance contract.

**Required arms:** frozen parent; admitted-update challenger; same-memory no-update control. Add oracle-domain diagnostic where mapping is relevant. Use independent held-out families.

**Metrics:** mapping accuracy, solver/path quality, domain-law rubric, retention, time-to-quality, total update/eval wall-clock, p50/p95 ordinary inference when meaningful, peak RAM/VRAM, persistent bytes, reindex/reload cost, uncertainty.

**DPO role invariant:** CODEX/Astra 6 ultracode selects lesson; external open Chinese model authors both final sides; CODEX/Astra separately judges exact pair; Student alone is updated.

**Good example:** same SQL family split, same consumer and memory, one admitted law-sensitive pair, held-out LEFT JOIN near-misses, compare parent/no-update/update and retention on a prior family.

**Bad example:** train on 100 old pairs, rerun unit tests and call green "better learning."

**Edge cases:** both sides bad; tie; distinction lost in projection; update changes state but not ordinary consumer; apparent gain from memory rather than update; regression in prior domain.

**Acceptance:** predeclared IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE branches and falsifier.

## R14 — reorder whole program around learning capability

**CURRENT TASK:** Convert the current slice/task queue into a capability/dependency sequence that reaches the first valid lesson early while preserving all later domain, graph, DTC, IDE and native-delivery obligations.

**NOT THE TASK:** Do not delete requirements because they are deferred; do not implement or run CI.

**Required skills:** plan-restructure, feature-prioritization, wave-assignment, phase-planning, prerequisite-chain, plan-review, goal-delivery-completeness, human-readable-architecture-audit.

**Context:** all R01–R10 receipts, R11 cut-set, R12 recipient decision, R13 experiment recipe, current S00–S10 and WorkLeft task inventory.

**For every old task/slice row:** classify REUSE / CHANGE / DEFER / SPLIT / SUPERSEDE; give new owner, prerequisite reason, retained trigger and learning evidence it supports.

**Good example:** move unrelated Windows full qualification to the native-delivery milestone while keeping pair-integrity/state checks before training and preserving the Windows obligation.

**Bad example:** remove S08/S09 because the first lesson uses documentation.

**Edge cases:** task supports two delivery lines; historical task contains both deterministic guard and learning-evidence proxy; a later source supersedes only half the task.

**Acceptance:** no known requirement disappears. Critical path is causal, not historical-number order.

## R15 — independent Astra 6 ultracode challenge of the chosen direction

**CURRENT TASK:** Have the requested independent CODEX/Astra 6 ultracode reviewer try to falsify the concrete R11–R14 candidate: recipient, first seam, experiment and reordered critical path.

**NOT THE TASK:** Do not pretend self-review is independent and do not replace empirical learning measurements with expert opinion.

**Required skills:** challenger-round-design, claim-verification, output-skepticism, plan-review, architectural-decision-testing, human-readable-architecture-audit.

**Input:** exact candidate artifacts and primary evidence; no giant corpus dump unless reviewer identifies one bounded missing source.

**Questions:** Is the recipient actually the one updated? Can pair distinction survive? Is objective correct? Does ordinary consumer read the child? Is a deterministic shortcut producing apparent gain? Are controls sufficient? Was a necessary domain/native obligation incorrectly deferred? Is PC cost measurably bounded?

**Good result:** a concrete falsifying counterexample with exact affected edge/task, or no blocker found with residual uncertainties listed.

**Bad result:** "Astra says looks good" without evidence, counterexample or scope.

**Acceptance:** independent review identity recorded, issues mapped to R16–R20 or explicit NOT_PERFORMED. Reviewer confidence is not learning gain.


# R10 repair decisions — 2026-10-13

## D10-11 — Every downstream task must be a full model packet

R11–R15 and their bounded subtasks must include exact prompt, context, applicable skills, primary/RAG/WorkLeft references, good/bad examples, edge cases, allowed scope, output schema, functional examination, falsifier, unresolved-owner handoff and stop condition. A topic label or broad folder link is insufficient.

## D10-12 — Functional learning examination outranks coverage for learned claims

Deterministic tests/checks remain valid for deterministic laws and integrity. Learned capability must be examined with matched behavior: parent/no-update/update, held-out families, domain-law-sensitive cases, mapping separated from solver/path, bulk-to-bulk time+accuracy+retention+resource curves. No coverage quota or Windows green state may be used as the learning verdict.

## D10-13 — Four hybrid roles require different interventions

Unit, Flow, Orchestrator and DynamicGraph must each have a distinct freeze/change/observe/falsify experiment. Do not flatten them into one learned classifier, one graph score or one trainer. Training remains owned by the actual LearningMachine path.

## D10-14 — DPO role assignment is immutable unless the user changes it

CODEX/Astra 6 ultracode selects the lesson and separately judges the exact final pair. The external permitted open Chinese model authors both final sides. XIIGEN is Student. Downstream work must not reopen this as an architectural choice.

## D10-15 — Domain-law chain is mandatory

Every domain experiment must bind law/constraint → discriminating token/feature → plausible near-miss → representation → kNN policy → mutable state/recipient → ordinary consumer → held-out evaluation. A domain name alone is not a curriculum.

## D10-16 — Earliest real learning is the optimization target

R11–R14 must minimize the causal cut-set before the first valid real update and move non-causal delivery/coverage work later while retaining owners and triggers. “Historical task came earlier” is not a prerequisite argument.

## D10-17 — R07 stale COMPLETE text is not closure evidence

Use the current correction: R07 is REPAIR_REQUIRED for full hybrid scope. Its bounded consumer traces remain usable; any broad COMPLETE wording is historical/overclaimed until the repair work order closes direct primary scope.

## D10-18 — Historical plan corpus is fully consumed only as evidence

The repaired R10 consumes the current historical thinking file as planning evidence, including CP5/CP6 and R01/R02 repair narratives, but does not convert those narratives into runtime, training or completion evidence.
