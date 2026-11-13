# Plan Self-Validation

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

## Fresh Execution / Method / Humility Self-Validation Hook

Self-validation must downgrade plans or packets that miss these gates:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: specified method/model/reasoning/count/folders/target/sub-agent route is binding; old matching content without the method remains NEEDS_FIX.
- `FINAL_ANSWER_FRESHNESS_GATE`: final/status must answer the latest command and state freshly executed this turn vs only verified.

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

## Full-Task Readiness Supersession Gate

PLAN_READY for planning/execution requires BOTH zero unresolved design/dependency blockers and full original-task coverage proven by minimum 30 sequential mental implementation passes, one at a time, not parallel. A later stricter full-task simulation, reviewer packet, or attachment that reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers supersedes any earlier narrow ready verdict.

When this happens, downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim, and propagate the downgrade into active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim. A plan-only wording patch is incomplete. Readiness-repair work orders must name skills, source attachments, active files, original-task requirements, good/bad examples, pass-count/evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

## Core Problem

A plan that references a skill that does not exist yet, or a plan that omits the
phase that creates a skill it later invokes, will fail at execution time with no
useful diagnostic. This skill installs a Phase 0 gate that catches those failures
before Phase 1 executes.

Phase 0 also validates authority-chain requirements. Requirements from the
current human operator / product owner, architect work order, approved active
plan, or accepted reviewer gate are binding unless that authority explicitly
marks them optional, exploratory, or preference-only. They are not wishlist
items.

Before Phase 1, build an Authority Requirements Ledger, Conversation Delta
Ledger, Replacement Mapping Register, and Contradiction Sweep. Every binding
requirement must map to a plan section, gate/test, and artifact. Every rejected
or superseded mechanism must map to the required replacement capability.
Deletion-only coverage is not valid.

For trainable replacements, the plan must name the trainable unit, trained
state, checkpoint/manifest/export/import path, locator/marketplace path,
fresh-load probe, continue-training probe, and ablation or leakage gate. For
prompt/flow grounding, `PromptFlowIndex` removal, `FlowReferenceDetector`,
explicit `FlowIndex`, direct dictionary retrieval, exact-prompt lookup, or
regex-only flow selection in a production route is not coverage for
`PromptFlowGroundingModel` unless the active authority explicitly permits
production deterministic grounding.

For orchestration and prompt-to-task extension plans, Phase 0 must also reject
any hidden deterministic shortcut that replaces a trainable unit. The plan must
cover the common and adaptive model and dataset lifecycle: checkpoint,
manifest, training manifest, locator, export, import, upgrade, load, and
continue-training evidence.

Dual-Path Host Verification Rule / Двухконтурная проверка host-сценариев:
for extension/runtime/product readiness, Phase 0 must reject a single fragile
`user in host -> extension -> core -> answer in host UI` verification chain.
Each scenario needs an Automated boundary path (adapter/API/core request-
response, model logic, DPO/training, token measurement, quality scoring,
backup/export/import/share/continue-training/rollback, checkpoint/locator gates)
and a Local desktop path (agent on the local computer in the real installed
host with visible marker, screenshot/video/log evidence, exact blocker if UI
control is impossible). JSON-only, adapter/core-only, test-only hook,
process/window-list, or synthetic helper UI cannot replace Local desktop proof;
screenshot-only cannot replace Automated boundary proof. A blocked Local
desktop path downgrades the claim and continues Automated boundary/other-host
work; it is not a stop gate. Phase 0 output must include `phase.step`,
Automated boundary status, Local desktop status, evidence, remaining work, and
downgraded claims.

Phase 0 must also reject requested-artifact substitution. A plan is invalid if
its literal execution would create validators, reports, JSON bundles,
PowerShell scripts, or static proofs where Luba asked for runtime product code,
plugin code, trainable-unit code, model assets, or host integration.

When a prior implementation already reached wrong code shape, Phase 0 starts
with skill/guide gap repair, not plan repair:
1. Identify the missing architect/planning/review rule.
2. Patch the skill or guide.
3. Mentally replay the plan under the patched rule.
4. Repeat until the replay produces the requested artifact class.
5. Only then approve plan repair or executor work.

Dishonest claim prevention is part of Phase 0. Before Phase 1, the plan must
prove that every count/status/completion/sub-agent claim has evidence matching
its exact scope. A ledger row, checklist row, generated table, or summary cannot
be counted as a review/fix cycle. Parent/self review cannot be counted as
sub-agent review. If the plan requires N reviews, N cycles, or N passes, it must
define the N independent evidence records, ids, work orders, reviewed scopes,
findings or no-fix justifications, and recheck evidence that will count.

---

## When to Invoke

- Before any multi-phase plan begins execution
- At the end of plan authoring, before handing a plan to an EXECUTOR session
- Whenever a plan references skills, interfaces, or artifacts that may not exist yet

One Phase 0 gate before Phase 1 = no phantom-skill failures mid-execution.

---

## Section 1 — The Phase 0 Requirement

Every multi-phase plan (≥ 2 phases) must contain a **Phase 0: Plan Self-Validation**
that runs all 5 checks below before Phase 1 executes.

Phase 0 is non-negotiable. If Luba's plan does not include Phase 0,
add it before execution begins.

Phase 0 gate: all 5 checks pass → Phase 1 may proceed.
Any check fails → execution stops; Luba resolves the gap.

---

## Section 2 — The 7 Checks

### Check -1 — Requested Artifact Shape Simulation

Before checking phase coverage, simulate the files and entrypoints the plan
would create.

```
Target repository:
Requested artifact class:
Expected implementation files:
Forbidden substitute artifacts:
Evidence artifacts location:
Replay result: PASS | FAIL
```

For Claude, Cursor, and Codex extensions, PASS requires real host entrypoint
code, manifest/package wiring, and thin adapter code that invokes xiigen exe or
the approved llm_mvp_core API. JSON, PowerShell, logs, and reports may support
verification, but they do not satisfy the plugin-code requirement.

If replay result is FAIL, repair the governing skills/guides before editing the
plan.

### Check 0 — Authority Requirements Ledger Completeness

Before validating phase structure, extract authority-chain requirements from
the current human operator / product owner, architect work order, approved
active plan, and accepted reviewer gate and put them into an Authority
Requirements Ledger.

Protocol:
1. Extract binding authority-chain requirements.
2. Build a Conversation Delta Ledger for requirements added or strengthened
   since the previous review.
3. Map every requirement to plan section + test/gate + artifact.
4. Map every rejected or superseded mechanism to the required replacement.
5. Any unmapped, deletion-only, or contradicted requirement blocks execution.
6. Any plan sentence contradicting a binding requirement is invalid.

```
Requirement ID: R-[short-name]
Conversation source: [message / paraphrase]
Plan section: [section or UNMAPPED]
Gate/test: [gate/test/probe or UNMAPPED]
Artifact: [artifact or UNMAPPED]
Status: PASS | UNMAPPED | CONTRADICTED
Next action: [repair action]
```

Trainable model plans must include rows for model export, model import,
checkpoint asset, training-data asset or hashed pointer, manifest,
locator/marketplace entry, fresh-load probe, and resume-training probe whenever
the active task authority asks for a usable, persisted, improvable, or
marketplace-loaded model.

If any ledger row is unmapped or contradicted → BLOCKED.

### Check 0.5 — Trainable Material Quality Readiness

For every trainable unit, adaptive model, DPO pipeline, semantic selector, or
orchestration learner, Phase 0 must prove the plan can tell an executor what is
good training material and what must be rejected before DPO rows are created.

Required evidence:

```text
Machine:
Good common material rubric:
Bad/rejected material rubric:
Adaptive-only material rubric:
Evidence-only material rubric:
Material-quality audit row:
Real-reference seed table:
DPO binding contract:
Rejected taxonomy:
Held-out construction:
Metric fields:
Deterministic controls:
Quality-claim false reason codes:
Status: PASS | BLOCKED
```

BLOCKED if:
- the plan says "train model" before source material is classified;
- examples are abstract families without real source refs and hashes;
- raw private text, exact paths, filenames, or code-bound execution recipes can
  enter common training;
- quality can be claimed from JSON/static/contract-only evidence;
- no runner-emitted metrics, trained-vs-control deltas, or failure examples are
  required.

### Check 1 — Goal Delivery Completeness

Every element of the active authority's stated goal must be assigned to exactly
one phase with a Gate B condition.

Protocol:
1. List every element of the stated goal
2. For each element: find the phase that delivers it
3. For each phase: confirm the Gate B is a `dotnet test` command with an exact
   expected value (e.g., `dotnet test → 47 passed, 0 failed`)
4. If any goal element has no assigned phase → BLOCKED

```
Goal element: [element text]
Assigned phase: Phase [N] — [phase name]
Gate B: dotnet test → [count] passed, 0 failed
Status: PASS | BLOCKED
```

### Check 2 — Phantom Skill Detection

Every skill referenced in any phase must either:
- Already exist in `docs/skills/` with a matching filename, OR
- Be created by a phase that runs before the referencing phase

Protocol:
```bash
# For each skill referenced in the plan:
ls docs/skills/[skill-name]-SKILL.md
# → exists: PASS
# → not found: check if a prior phase creates it
```

Phantom skill record:
```
Skill referenced: [skill-name]-SKILL.md
Referenced in: Phase [N]
Exists now: YES / NO
Created by: Phase [M] (where M < N) / NOT CREATED → BLOCKED
```

### Check 3 — Skill Presence Matrix

Every skill created by the plan must be registered in `docs/HOW-TO-USE-SKILLS.md`
by a phase that runs before it is first invoked.

Blast radius check: for each new skill, list which downstream phases invoke it
and confirm those phases run after the registration phase.

```
New skill: [skill-name]-SKILL.md
Created by: Phase [M]
Registered in HOW-TO-USE by: Phase [R] (R ≤ M)
First invoked by: Phase [N] (N > R)
Status: PASS | BLOCKED
```

### Check 4 — Execution Order Consistency

Rules verified:
1. HOW-TO-USE-SKILLS.md update phase runs AFTER the skill-creating phase
2. No phase depends on an artifact produced by a later phase
3. Gate B conditions are testable with `dotnet test` or `dotnet build` — not
   "visually inspect" or "check manually"

```
Phase [N] dependency: [artifact]
Produced by: Phase [M]
Order: M < N → PASS | M > N → BLOCKED
```

---

## Section 3 — Execution Phase Template

Every CREATE phase in a plan follows this template:

```markdown
## Phase [N]: [Phase Name]

**Pre-condition:**
- [specific file or test count that must be true before this phase starts]
- Verified by: `dotnet test --filter "[filter]" → [expected count] passed`

**Output files:**
- `[path/to/file.cs]` — [one-line description of what the file does]
- `[path/to/FileTests.cs]` — [what is tested]

**Content specification:**
- [specific methods, interfaces, or behaviors the output files must contain]
- DNA compliance: Rule 1 (OperationResult<T>), Rule 2 (fabric-first), Rule 3 (comments)

**Blast radius:**
- Files read: [list]
- Files written: [list]
- Files that reference this output: [list]

**Gate B:**
`dotnet test --filter "[filter]" → [exact count] passed, 0 failed`

**Post-condition:**
- `[specific assertion that the output is complete and correct]`
```

---

## Section 4 — Phase 0 Output Format

Phase 0 produces a self-validation report:

```markdown
## Phase 0: Plan Self-Validation (plan-self-validation-SKILL)

Check -1 - Requested Artifact Shape Simulation:
  [target repository shape replay table]
  Result: PASS | [N] wrong-shape risks

Check 0 - Authority Requirements Ledger Completeness:
  [requirements ledger with plan section, gate/test, artifact, status]
  Result: PASS | [N] unmapped/contradicted requirements

Check 0.5 - Trainable Material Quality Readiness:
  [per-machine good/bad/adaptive/evidence-only material rubric, seed refs,
   DPO contracts, held-out construction, metric fields, deterministic controls]
  Result: PASS | [N] material-quality gaps

Check 1 — Goal Delivery Completeness:
  [goal element table with phase assignments and Gate B conditions]
  Result: PASS | [N] BLOCKED items

Check 2 — Phantom Skill Detection:
  [skill reference table]
  Result: PASS | [N] phantom skills found

Check 3 — Skill Presence Matrix:
  [matrix table]
  Result: PASS | [N] registration gaps found

Check 4 — Execution Order Consistency:
  [dependency order table]
  Result: PASS | [N] order violations found

PHASE 0 VERDICT: PASS → Phase 1 may proceed
               | FAIL → [list blocking items] → Luba resolves before Phase 1
```

---

## Anti-Patterns

1. **"Skip Phase 0 for small plans"** — phantom skills in 2-phase plans fail
   just as hard as in 10-phase plans. Phase 0 is a 5-minute check.

2. **"I'll fix phantom skills as I go"** — a mid-execution discovery that a
   required skill doesn't exist stops the EXECUTOR session cold. Phase 0 prevents this.

3. **"Gate B is 'tests pass'"** — "tests pass" is not a Gate B. A Gate B is
   `dotnet test → 47 passed, 0 failed`. Exact count, exact command.

4. **"HOW-TO-USE update doesn't need its own phase"** — it does, and it must
   run after the skill-creating phase, not before or during.

---

## Integration

- intent-to-plan-SKILL.md produces the plan structure that this skill validates
- phase-completion-packager-SKILL.md requires all 5 Phase 0 checks to be in the
  packager's evidence list before it marks a phase complete
- prerequisite-chain-SKILL.md handles Check 2 (phantom skills) for cases where
  the skill prerequisite is an interface, not a skill file
- session-execution-log-SKILL.md records Phase 0 results in EXECUTION-LOG-Pn.json
