# Agent Output Format

## Human Publication-State Wording Gate

For S04 publication/status, translate repository state into one explicit human meaning before any SHA:

- `DRAFT` — work is durably committed/pushed but still awaiting required review/acceptance; never say the task is done.
- `ACCEPT` — required independent review and exact-final-SHA validation are complete; task closure may be reported.
- `GOVERNANCE` — rule/compiler/checker repair only; active product task remains open.
- `RECONCILE` — Git/merge reconciliation only; exact resulting HEAD still needs validation before acceptance.

Generic labels such as `s04 artifacts`, `slice 4 artifacts`, or `work artifacts` are not sufficient human status and are forbidden for new publication.

A user-facing report must say whether the publication is durable-only or accepted, and whether the execution pointer may advance.

`DRAFT_IS_DURABLE_NOT_ACCEPTED`
`GOVERNANCE_OR_RECONCILE_COMMIT_DOES_NOT_CLOSE_PRODUCT_TASK`


## S04 product-progress wording gate

For S04 status, report product progress separately from support work. Governance, skills, agent-file, packet-compiler, merge, receipt and planning changes are support-only unless they carry actual product code/test/runtime evidence for a named S04 predicate.

Every S04 status must say whether the first real safe lesson outcome remains open and name the next product predicate. Do not let clean Git/governance wording imply product completion.

`GOVERNANCE_IS_SUPPORT_NOT_PRODUCT_PROGRESS`
`PACK_VALIDATION_IS_NOT_S04_PRODUCT_COMPLETION`

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Active Subagent Final-Output Ban

Any final/status/hold response is invalid while a current-task subagent is still
running, waiting, cancel-pending, unknown, or lacks supported terminal evidence.

A progress update may be shown, but it must be followed by a wait/status/recovery
tool action in the same turn. A wait timeout, no heartbeat, or "still working"
never authorizes turn termination.

A returned NEEDS_FIX/PARTIAL/FAIL packet must be translated into the next
repair/review action and continued under OP-65. It is not a final response unless
the task is terminally blocked by proven external evidence or Luba explicitly
stopped it.

Exact literals:
`ACTIVE_SUBAGENT_FINAL_FORBIDDEN_GATE`
`progress update must be followed by wait/status/recovery in the same turn`
`no final while current-task subagent is active`

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include explicit skill-load guidance and the complete schema below. A generic prompt that only names a target file and broad instruction is invalid. If a sub-agent receives a generic prompt, the parent must interrupt and replace it before accepting work.

Required skill load guidance for DPO/DynamicGraph plan repair:

```text
SK-SESSION: planning-session-startup-SKILL.md + SESSION-START-PROMPT.md + AGENTS.md
SK-INTENT: architect-behavior-classifier-SKILL.md
SK-PLAN: authority-requirement-binding-SKILL.md + reference-inventory-and-source-map-SKILL.md + phase-planning-SKILL.md
SK-REVIEW: plan-review-SKILL.md + goal-delivery-completeness-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md + specificity-calibration-SKILL.md + output-skepticism-SKILL.md + reconnaissance-gate-SKILL.md
SK-ARCH: trainable-architecture-decision-SKILL.md + no-product-decisions-SKILL.md + output-readability-gate-SKILL.md
SK-DOC/PARTA when plan touches existing code: implementation-doc-uml-SKILL.md + implementation-doc-algorithms-SKILL.md + implementation-doc-code-refs-SKILL.md + implementation-doc-status-SKILL.md + implementation-doc-hierarchy-SKILL.md
```

If a work order lacks this explicit skill block or an equivalent current-task-specific skill block, it is invalid and must be interrupted/replaced.

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
original_task_requirements_to_cover:
mental_simulation_required:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

Good sub-agent prompt: names exact skills, exact attachments, exact active files, exact requirements, good/bad examples, pass count, nonparallel rule, verification commands, return schema, and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix active-codex plan readiness` or `Search NOT_READY and fix wording` without skills, task attachments, examples, coverage matrix, and simulation gates.

## Current-Ledger Pass Evidence Gate

For the ILearningDynamicGraph / `dpo legal build` 50-pass planning-readiness
chain, the canonical current ledger for Pass31+ is
`C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`
(в DTC-версии тот же путь записан как `<planning-workspace>\llm mvp\dpo legal build\active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`; `<planning-workspace>` = `C:\Users\lubao\Documents\xiigen`)
unless Luba explicitly changes it in a current unquoted instruction.

Every pass work order must name the exact current ledger path and pass number.
The worker must append exactly one `### Pass N/50` section to that exact file
before claiming pass completion. The worker must run/read back post-append
verification: heading count is 1, exact `subagent_id_or_NONE` count in that
section is 1, `requested_model: gpt-5.5` and
`requested_reasoning_effort: xhigh` are present, line-anchored conflict-marker
count is 0, and configured mojibake marker count in the section is 0.

A final packet alone is not pass evidence. The parent must verify the exact
current-ledger section before accepting the pass. If the worker appended to the
wrong ledger or omitted the append, the append claim is false/unsupported for
the current ledger; correction must record the same pass with
`count_claimed: 0 new passes`, not create a new pass. A current-ledger section
is necessary but not sufficient: the packet must also include
`skill_load_status` and a task-specific coverage table.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Spawned Worker Final Packet Identity Gate

Every spawned-worker final packet and evidence packet must include:

```text
subagent_id_or_NONE: <platform id if visible, otherwise UNKNOWN_IN_SUBAGENT>
identity_instruction: Do not use NONE; hidden platform id becomes UNKNOWN_IN_SUBAGENT.
subagent_identity_self_check: spawned_worker=true; used_NONE=false; final_packet_self_scan=clean; parent_must_reconcile=true
```

Before return, the worker must self-scan the final packet and any evidence
artifact it wrote. A spawned packet fails before return if the scan finds
`subagent_id_or_NONE: NONE`, `subagent_id_or_NONE=NONE`, `NONE_PARENT`,
`current-thread worker`, `id not visible`, `I did not use a sub-agent`, `I did
not use a subagent`, `no sub-agent was used`, or `no subagent was used`; correct
it before returning. A spawned worker may say `No nested sub-agent was used by
this worker` only when true. The parent must route missing, false, placeholder,
unscanned identity, or spawned-worker self-denial to NEEDS_FIX/recovery and
count it as 0 passes/reviews/cycles.

## SILENT_SUBAGENT_NO_PACKET_RECOVERY_GATE

Spawned workers must not disappear into a long silent run. A work order must ask
for early micro-packets after initial reads, first compile/test/runtime error,
filesystem denial, or scope uncertainty; the worker returns a human-readable
`NEEDS_FIX` packet instead of silently continuing.

For parent output, a wait timeout, heartbeat nonresponse,
running/shutdown/interrupted/not_found notification, or no-output wait is not a
completed packet. If no packet arrives after the parent heartbeat/status request
and bounded wait, report `LOST_WORKER_UNACCEPTED`, `count_claimed=0`, and
`independent_evidence_count=0`, then name the smaller recovery work order or
allowed orchestration/cleanup. Without a returned packet and parent-visible
allowed-file diff, accepted work is zero. Do not ask Luba to debug agents and do
not present the silent/lost worker as a product blocker. Human-gate-first status
remains mandatory before the machine fields.

## The Three-File Rule

Every plan produces exactly three files:

| File | Audience | Purpose |
|------|---------|---------|
| `PLAN-[PHASE]-REFERENCE.md` | Humans (Luba) | What we're building and why — architecture, decisions, context |
| `docs/STATE-P[N].json` | Navigation — resuming sessions | Phase state, test baseline, completed deliverables, next step |
| `PLAN-[PHASE]-EXECUTION.md` | Claude Code — execution guide | Step-by-step, file paths, code blocks, Gate B conditions |

Never merge these. Each file has one audience and one purpose.

Every plan file set must also carry authority-chain requirement evidence:
Authority Requirements Ledger, Conversation Delta Ledger, Replacement Mapping
Register, and Contradiction Sweep. Requirements from the current human operator
/ product owner, architect work order, approved active plan, or accepted
reviewer gate are binding unless that authority explicitly marks them optional,
exploratory, or preference-only.

Removing an old mechanism is not coverage for a required replacement. If the
authority requires a trainable replacement, the plan must name the trainable
unit, trained state, checkpoint/manifest/export/import path,
locator/marketplace path, fresh-load probe, continue-training probe, and
ablation or leakage gate. For prompt/flow grounding, `PromptFlowIndex` removal,
`FlowReferenceDetector`, explicit `FlowIndex`, direct dictionary retrieval,
exact-prompt lookup, or regex-only flow selection in production is not coverage
for `PromptFlowGroundingModel` unless the active authority explicitly allows
production deterministic grounding.

## Bounded Sub-Agent Execution Output

Spawned bounded sub-agent is already the sub-agent executor. If the work order
gives allowed write scope, the receiving sub-agent must directly create or edit
those scoped artifacts in its workspace and then report evidence in the packet.
It must not attempt to invoke `codex.exe`, CLI Codex, another AI, or another
sub-agent to satisfy "through sub-agents" unless the work order explicitly asks
it to delegate. `codex.exe Access is denied is not a blocker` for a spawned
sub-agent artifact task; it is evidence of a forbidden nested-delegation path,
not a reason to demand parent direct execution. If the sub-agent cannot write an
allowed file for filesystem reasons, it reports the exact path, attempted
operation, and filesystem error. Parent direct execution remains forbidden, and
sub-agent execution is not parent direct execution.

## Phase Propagation Output Gates

Exact output-gate literals for verification: `boundary/API evidence != host readiness`; `current-state invariants + evidence packet`.

Machine-only statuses are invalid. Any phase, review, fix, propagation, or
bounded sub-agent packet shown to Luba must include phase/step meaning, counts,
done/in-progress/remaining, review findings, fixes, and next action before any
machine label, JSON path, packet id, STATE code, or validator code.
`STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED`: every visible progress/status/
review/final/worker packet must pass the canonical human gate in
`docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md`; otherwise reject it as
`STATUS_FORMAT_HUMAN_GATE_FAILED`. A raw `claim_scope` block, bare `NEEDS_FIX`,
`packet accepted`, `QSG`, `boundary pass`, STATE/JSON path, or informal partial
board is not valid user-visible status. A returned sub-agent packet must be
translated by the parent before it is shown to Luba as status.
`HUMAN_BOARD_BEFORE_CLAIM_SCOPE_SUPPORT_BLOCK`: the claim evidence block
(`claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed`)
is mandatory support, but it must be internal or follow the human board. Any
Luba-facing packet that starts with raw `claim_scope`, JSON/STATE/path/packet
id, ALL_CAPS verdict, compact machine table, or raw sub-agent excerpt before
the Russian phase/step/status board is `STATUS_FORMAT_HUMAN_GATE_FAILED`.
`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`: ordinary commentary heartbeat/progress
updates must be a short human paragraph or compact board first and may omit the
full claim/evidence schema. If evidence fields are visible in routine progress,
cap them at one short `Evidence:` sentence or at most two short support lines.
Full claim/evidence schema belongs to worker return packets, acceptance/
rejection reviews, final summaries, and direct audit/evidence answers. A report
that formally starts with `Фаза...` but then dumps a long raw evidence block is
still `STATUS_FORMAT_HUMAN_GATE_FAILED` when the visible message is not
skimmable.
`HYBRID_STATUS_PACKET_DUMP_FORBIDDEN`: reject a formal human board plus a long
raw claim/evidence packet as unreadable hybrid status. This includes
`Фаза/Шаг/Статус` text followed by `claim_scope`, `evidence_scope`,
`count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, or
`downgrade_needed` as the main body; phase/status prose followed by a raw worker
packet or machine inventory; and technical packet fields used as the main
status. Parent output must summarize worker packets in human language first.
Routine progress gets at most one short `Evidence:` sentence or 1-2 compact
support lines; full schemas are only for worker return packets, acceptance/
rejection reviews, final summaries, or explicit audit/evidence answers.
`PARENT_TRANSLATES_WORKER_MACHINE_FIELDS`: parent status must translate returned
worker machine fields into human meaning before showing them to Luba. Do not use
a raw `claim_scope/evidence_scope` block or raw sub-agent excerpt as the main
status.
`POST_GOVERNANCE_PATCH_RESUME_PRODUCT_EXECUTION`: after a governance human-gate
patch, active product execution must resume through the next authorized product
worker or recovery route; governance repair is not completion.
`PRODUCT_CODE_CONTINUATION_ONLY`: after governance/skill/rule/agent-file repair
for a live product/code/QA task, visible output must not present another
governance worker, read-only scan, status package, `NO_PATCH`, rule readback, or
packet-formatting repair as product/code continuation. It must name the
product/code/QA execution worker or narrowed recovery worker for the concrete
`audit_union_row_id` or `next_product_code_slice`, or state the true external
blocker certificate.
`PRODUCT_WORKER_NO_PACKET_ZERO_ACCEPTED_REQUIRES_RECOVERY`: `shutdown`,
`not_found`, interrupted, no packet, or 0 accepted product evidence from the
product/code worker is zero accepted product work and must be reported as a
recovery queue, not as final/status or a true blocker.

Review output must keep evidence boundaries explicit: boundary/API evidence !=
host readiness; marker-only != full local desktop pass; trace != savings;
schema != training; checkpoint candidate != promotion. If the packet only has
one side of proof, write the narrow claim and `claim=false` for the broader
claim.

Downgrade, blocker, not proven, not ready, needs verification, no claim, or
status-only artifact is not completion. Output must turn it into the next
concrete work item unless all product requirements are already proven or Luba
explicitly stops. Separate `evidence_accounting_done` from
`product_requirement_proven`.

For scenario coverage output, use scenario counters instead of phase-artifact
counts: total scenarios, covered by Path A, covered by Path B, blocked Path B
with reason, remaining steps, and exact scenario moved. Every scenario should
attempt dual-path proof: Path A automated adapter/API/core evidence for the same
prompt/scenario, plus Path B local-agent UI path in the real host program on
this computer as far as the local UI permits. If Path B cannot reach an
extension/plugin route because of host limitations, auth, storage, missing UI
route, or inaccessible app surface, output a human-readable host-path blocker
with the exact screen/state and next repair; do not report deprecated
`product readiness` or `product-ready count` as the primary completion term.

Russian examples:

```text
Сценарии: всего 12; полностью доказано 3; частично доказано 5; не сдвинуто 2; заблокировано с понятным следующим действием 2.
Сдвинулся сценарий Claude visible-marker: local proof частичный, automated boundary ещё нужен; продуктовая готовность не доказана.
```

Parent Architect Subagent-Only Firewall output gate: packets produced under
architect-only or sub-agent-only authority must show the parent did not become
executor. The output must preserve the exact phrases `parent local sidecar work
ban`, `read-only verification after a sub-agent packet returns`, `sub-agent
waiting is not a stop`, and `no local non-overlapping work escape`, and must
explain any parent action as orchestration, review, bounded-fix request, or an
explicitly assigned commit/push boundary.

Exact literals: `Parent Architect Subagent-Only Firewall`; `parent local sidecar work ban`; `read-only verification after a sub-agent packet returns`; `sub-agent waiting is not a stop`; `no local non-overlapping work escape`.

For untracked new artifacts, a review may use current-state invariants +
evidence packet instead of an impossible git baseline. The packet must still
list exact paths, explicit counts, exact search terms, and `claim=false` for
any readiness/product/full-pass claim not proven by both required proof paths.

No stop gates from protocol repair: after a bounded protocol/governance/skill
repair is reviewed, the output names the next bounded phase and continues
unless GOLD GATE, missing authority, unsafe action, or a real blocker remains.

Repeated self-block drift must be recorded as governance drift. If a spawned
bounded sub-agent already has allowed write scope, looking for nested
sub-agents, local codex CLI, or parent direct execution is not a blocker; it is
a failure pattern to repair or avoid in later work orders.

---

## REFERENCE File Format

```markdown
# [Phase Name] — Reference

## Goal
One paragraph: what this phase accomplishes and why it matters.

## Architecture Decisions
- DR-01: [decision and rationale]
- DR-02: [decision and rationale]

## Component Map
| Component | Location | Interface |
|-----------|---------|-----------|
| BruteForceIndex | Src/09.VectorSearch/ | IVectorIndex |

## Dependencies
What must exist before this phase starts.

## What Success Looks Like
How Luba will know this phase is complete. Measurable outcomes.
```

---

## STATE-Pn.json Format

```json
{
  "phase": "P1",
  "status": "IN_PROGRESS",
  "startedAt": "2026-05-01T10:00:00Z",
  "testBaselineAtStart": 916,
  "testCountNow": 924,
  "completedDeliverables": [
    "Src/09.VectorSearch/BruteForceIndex.cs",
    "Tests/VectorSearch/BruteForceIndexTests.cs"
  ],
  "pendingDeliverables": [
    "Src/09.VectorSearch/HnswIndex.cs"
  ],
  "nextStep": "STEP 3 — Write HnswIndex implementation",
  "gateResults": {
    "gate0_discovery": "PASS",
    "gate1_dna": "PASS",
    "gate2_fabric": "PASS",
    "gate3_flow": "N/A",
    "gate4_decisions": "PASS",
    "gate5_tests": "PASS",
    "gate6_docs": "PASS",
    "gate7_principles": "PASS"
  },
  "phaseGatesPassed": true
}
```

---

## EXECUTION File Format

```markdown
# [Phase Name] — Execution

## SESSION GATE
- Build baseline: dotnet build LlmFromScratch.sln → 0 errors
- Test baseline: dotnet test → 916 passed, 0 failed
- Branch: main

---

## STEP 1 — [Description]

**File:** `Src/09.VectorSearch/BruteForceIndex.cs`

**Gate B:** `dotnet test --filter "BruteForceIndex" shows Failed: 0, Passed: ≥ 5`

**Code:**
[exact code block]

**Test:**
[exact test block]

---

## STEP 2 — [Description]

**File:** `...`
**Gate B:** `...`

[...]

---

## SESSION GATE (End)

```
dotnet build LlmFromScratch.sln → 0 errors
dotnet test --no-build → Failed: 0, Total ≥ [baseline + N new tests]
```

## STATE UPDATE
[What to write to STATE-Pn.json after this step]

## STOP — Await Luba Approval Before Next Phase
```

---

## SESSION-0 Plan Review Gate

Before any execution plan is approved, run three gates:

### Gate A — Format Checks (FC)

```
FC-0: Does the file include an Authority Requirements Ledger mapping every
      binding requirement from the active human/product/architect/plan/reviewer
      authority to plan section + test/gate + artifact, with no unmapped,
      deletion-only, or contradicted rows?
FC-0a: Does the file include a Conversation Delta Ledger, Replacement Mapping
       Register, and Contradiction Sweep, and do required replacements map to
       actual replacement capabilities rather than deletion-only changes?
FC-0b: Does every status/count/sub-agent claim have evidence matching its exact
       scope: ledger rows are not cycles, parent/self review is not sub-agent
       review, and N cycles/reviews/passes require N independent evidence
       records?
FC-1: Does every STEP have a Gate B that is a command + expected output?
FC-2: Does every STEP reference a real file path (not a placeholder)?
FC-3: Does every new interface appear in COMPONENT-INTERFACES.md update plan?
FC-4: Does the plan violate any of the 10 DNA rules?
FC-5: Does every Step that creates a class include a test step?
FC-6: Does the plan include a final dotnet test count check?
FC-7: Are all abbreviations expanded on first use?
FC-8: Does every code block have Rule 3 comments on every line?
FC-9: Does the plan list every file it will create or modify?
FC-10: Does the SESSION GATE section list the exact dotnet test command?
FC-11: Is the STOP marker present before the phase transition?
FC-12: Is the STATE UPDATE section present after every session gate?
FC-30: If the plan touches learned, semantic, adaptive, DPO, scorer,
       selector, classifier, router, KNN, dictionary, counter, regex, or
       pattern-matcher logic, does it map every capability to exactly one
       allowed trainable architecture level and classify helpers only as
       implementation details inside that level?
FC-31: If the output is architecture, trainable, code-review, documentation, or
       audit material, does it pass the Human-Readable Architecture Artifact
       Gate and Concrete Repair Blueprint Gate: Russian summary, PlantUML or
       Mermaid UML importable to draw.io, simulations, no-code algorithms,
       exact code quotes, adjacent concrete solutions, and no schema-only or
       prompt/spec substitute?
FC-32: If an architecture/trainable/code audit claims sufficiency, readiness,
       completion, or closure, does it pass the Audit Sufficiency Boundary Gate:
       exact bounded scope, green breadth challenge, no over-broad
       "Architecture audit sufficient/complete", parent review question
       answered, and Delta-Severity Feedback Gate recorded for later
       same-domain BLOCKING/HIGH misses?
FC-33: Does every bounded sub-agent work order include the Model/Reasoning
       Configuration Gate: `requested_model=gpt-5.5`,
       `requested_reasoning_effort=xhigh`, runtime/request metadata or
       `mode_verified_from_work_order=true` when unavailable, downgrade
       authority only by explicit current Luba quote, warning only for concrete
       mismatch, generic "GPT-5" identity is not downgrade evidence, and
       state/log record?
FC-34: If execution continues after governance repair, does the plan require the
       10 Review Cycle Gate before runtime/code/training/host execution?
FC-35: Does the plan require the Execution Plan Visibility Gate: parent presents
       the plan, current stage, current plan step, 10-cycle outcome, opened/
       closed claims, remaining gaps, and execution admission reason in
       human-readable Russian chat?
FC-36: Does the plan define the Full Extension Definition Gate for Claude,
       Cursor, and Codex separately?
FC-37: Does final completion require the Final Execution Review Gate with 10
       real independent sub-agent execution reviews?
FC-38: Does every code slice review require the Deterministic-Boundary Code
       Review Gate?
FC-39: Does every long execution status require the Human-Readable Execution
       Board Gate with total task count, completed count and names, current
       task and owner/sub-agent, remaining count and names, current result
       type, accepted evidence, closed claims with reasons, open gaps, next
       action, and execution pause state?
FC-39a: Does every parent/sub-agent execution report and status board row use
        the Russian-first contract before any opaque codes:
        `Фаза X из Y: <name>. Смысл фазы: <product purpose>.`,
        `Шаг X.Y из X.N: <name>. Смысл шага: <what it checks/creates>.`,
        rule-file exact verification literals
        `Фаза N из M: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.`
        and `Шаг N.K из N.T: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.`,
        while actual reports replace N, M, N.K, and N.T with concrete
        phase/step numbers,
        `Статус: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON`
        with a plain Russian explanation, total project position, `Сделано`,
        `В процессе`, numbered `Осталось`,
        `Review нашёл`, `Что исправлено`, `Что осталось`,
        `Следующее действие`, and whether commit/continuation is allowed?
FC-39b: Does every status reject bare `QSG-28`, `07.06/07.07 NEEDS_FIX`,
        `packet accepted`, `boundary pass`, JSON/state labels, or other
        machine-only codes unless they are translated into human meaning and
        done/in-progress/remaining counts in the same update?
FC-40: Does the plan include the Plan Presentation Gate so artifact-only plans,
       JSON logs, STATE rows, and QSG labels cannot substitute for presenting
       the plan in chat when Luba says she cannot see it?
FC-41: If 10 reviews are required, does the plan require visible review records
       1..10 with reviewer role/id/model/effort, verdict, findings, repair
       action, affected files/sections, and whether plan/skills changed?
FC-42: Does sub-agent evidence require a Sub-Agent Visibility Ledger with agent
       id/nickname, requested model/effort, task, status, key result, exact
       review scope, exact changed/reviewed files or sections, progress counts,
       and parent review disposition?
FC-43: Does the completion format include the False Completion / Stop Repair
       Rule so "ready/done" without full requested-goal proof becomes
       plan/skills/gate repair plus continued review/fix work?
FC-44: Does the Full Extensions Readiness Review Gate list Claude, Cursor, and
       Codex separately and reject smoke/API/dashboard/helper/static evidence as
       proof of full extension readiness?
FC-45: Does every code slice review require the Execution/Code Review
       Completeness Gate: exact review scope, project standards/tests,
       `llm_mvp_core` core boundary, thin host adapters, hidden deterministic
       shortcut sweep, deterministic helper labels, and docs/QA/dashboard
       impact?
FC-46: Does every chat log/status/final-or-near-final report pass the human
       gate: Luba can tell what is done, what is running, what remains, current
       result type, closed claims and why, and next bounded action without
       opening machine logs?
```

### Gate B — AI Review

Get independent review from a second model for plans that:
- Span more than one subproject
- Add or change a public interface
- Introduce a new algorithm

### Gate C — Luba Approval

Present the plan in plan mode. Wait for explicit written approval. Do not begin until it is received.

---

## Anti-Patterns

1. **Merging REFERENCE and EXECUTION into one file** — audiences and purposes conflict
2. **EXECUTION without Gate B per step** — steps without gates have no completion criteria
3. **STATE without test count** — resuming sessions cannot establish regression baseline
4. **STOP marker missing** — Claude Code continues past phase boundary without approval
5. **Placeholder file paths** — `Src/TODO/` is not a file path; do not put it in EXECUTION
6. **Machine-readable audit as human plan** — table-only, shortcode-heavy,
   English-only, schema-only, or prompt/spec output не является решением when
   Luba needs a human-readable architecture artifact.
7. **Narrow audit as complete architecture audit** — "v1 scope completed" may be
   used only with exact bounded scope; "Architecture audit sufficient/complete"
   is forbidden without a green breadth challenge and Delta-Severity Feedback
   Gate coverage.
8. **Plan hides new governance gates in prose** — model/reasoning, 10 review
   cycles, execution-plan visibility, full-extension definition, final
   execution review, and deterministic-boundary review must appear as explicit
   plan gates/work-order fields, not as vague narrative.
9. **Machine log replaces user status** - a QSG label, STATE row, JSON report,
   hidden review transcript, or opaque row such as `Phase 01.01 Scenario And
   Claim Inventory` without `Фаза X из Y`, `Фаза N из M`, `Смысл фазы`, `Шаг X.Y`,
   `Шаг N.K из N.T`,
   `Смысл шага`, status vocabulary `DONE | IN_PROGRESS | NEEDS_FIX |
   REVIEWING | BLOCKED_WITH_REASON`, total project position, `Сделано`,
   `В процессе`, numbered `Осталось`, `Review нашёл`, `Что исправлено`,
   `Следующее действие`, and commit/continuation permission is not a valid
   user-facing status. Bare `QSG-28`, `07.06/07.07 NEEDS_FIX`,
   `packet accepted`, and `boundary pass` are invalid without human
   translation and counts. Opaque codes are secondary only.
10. **Code review scope hidden by a green label** - tests, QSG, or a generated
    review row do not accept a code slice unless the review names standards,
    tests, core boundary, thin host adapters, deterministic labels, and
    docs/QA/dashboard impact.
11. **Ledger rows called cycles** - a generated table with N rows is not N
    review/fix cycles. Count claims require N independent evidence records.
12. **Parent work called sub-agent work** - parent/self review cannot be
    described as sub-agent review without a named sub-agent packet.
