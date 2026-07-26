# Plan Restructure

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## Core Problem

A capability plan written before new infrastructure (new handlers, new index types,
new phase gates) was completed can have correct phase content but still be wrong:
it does not reference the new infrastructure, its phases are too large for a single
session, it does not include new gates, and it does not produce the learning handoff
outputs that the new infrastructure expects. Rewriting the entire plan introduces
errors into things that were correct. This skill defines the delta analysis approach
that preserves correct content while adding only what changed.

**Do NOT use for a capability that has no existing plan.** For new capabilities,
use subproject-examination-SKILL.md.

---

## When to Invoke

- When an existing capability plan was written before new shared infrastructure
  (new handlers in ContinuousLearningPipeline, new ISessionStorage patterns,
  new validate.handler named checks) became available
- When a plan has phases too large for a single session (context loss risk)
- When a plan's gate checks do not include new Phase A gates that now apply

---

## Section 1 — Read Before Writing

Read the existing plan completely before writing anything. Read it as if executing it.
While reading, annotate:

```
For each section:
  □ Is this still correct given the new infrastructure?
  □ Does this assume something that no longer exists or has a new interface?
  □ Is there a new gate/check that should be here?
  □ Does this produce output that new downstream capabilities expect?
```

**Do not start writing until you have read the complete plan.** A section that looks
correct in Phase A may have downstream assumptions that break in Phase E. The most
common restructure error is writing Phase A changes before reading what Phase E expects.

---

## Section 2 — The Delta Analysis

Write two explicit lists before touching any session file.

### List 1: What does NOT change

Be explicit. Name each thing:

```
UNCHANGED FROM EXISTING PLAN:
- All N arbiter fixture files and their exact JSON content
- The score bracket decision tree (4 brackets with specific score ranges)
- PromptPatch formula (one targeted block per failing check, WRONG/CORRECT)
- Phase E DPO triple format (PromptSystem required — specific named gap)
- Phase F cross-capability dependency registration
- The M-check infrastructure gate (all checks still required)
- dotnet test zero-regression rule at every gate
- STOP after every phase
```

This list matters: a session executing the file must not re-examine these things.
Without the explicit "unchanged" list, correct content gets "improved" into incorrect
content.

### List 2: What changes and exactly where

For each change:
1. Which phase it belongs to
2. Whether it is a new step, a modified gate, or a new output
3. What new infrastructure drives it
4. Whether it is additive (new step) or replacement (replaces an existing step)

```
PHASE A — New step A0: Capability gap pre-scan (additive)
  Driven by: new ICapabilityGapScanner in phase 5 infrastructure
  Position: before existing A1 (before fixture seeding)
  Action: call ICapabilityGapScanner.Scan for this archetype; read allPresent flag
  On MISSING: read remediation plan from gap scanner output

PHASE B — Modified B1: Add post-run distillation check (additive to existing loop)
  Driven by: new IncrementalTrainer integration
  Position: after each capability run, before next
  Action: check DPO triple count for this capability class
  Expected: ≥1 new triple per passing cycle

PHASE B — Modified B2: Add validation gate 9 (additive to gate)
  Driven by: new validate.handler named checks (phase 6 skills)
  Position: at end of Phase B gate, after existing checks
  Action: run named check for temporal_threshold_from_config if TEMPORAL archetype
  On FAIL: phase completion BLOCKED until named check passes

PHASE E — New field: curriculumTier on DPO triples (additive)
  Driven by: dpo-triple-format-SKILL.md requirement
  Position: when recording DPO triples
  Action: add CurriculumTier field from archetype table; never null
```

---

## Section 3 — Session Split Criteria

This is the most important structural decision. Apply these criteria to each phase:

### Criterion 1: Context loss risk

A phase needs its own session file if losing context mid-phase would make correct
resumption impossible.

```
Phase B of any generation capability: multiple cycles, each producing a trace
  that feeds the next decision
  Losing context mid-B: lose which cycles scored what, what was patched
  VERDICT: dedicated session file

Phase A fixture seeding: N deterministic file-seedings, all independent
  Losing context mid-A: re-read STATE.json, resume from last successful seed
  VERDICT: one session file is sufficient
```

### Criterion 2: Decision tree depth

A phase needs its own session file if it contains branching that produces
significantly different execution paths.

```
Phase B: score brackets × capability classes × cycles = complex branching
  Session file must contain all branches concretely
  VERDICT: dedicated, detailed session file

Phase C (conditional): runs only if patches occurred in Phase B
  Can share a file with Phase D if clearly marked
  VERDICT: can be combined
```

### Criterion 3: Cognitive load

A phase needs its own session file if executing it requires more information in
working memory than a fresh session can reliably hold.

```
Phase D (integrate): writes test files using patterns from multiple skill sets
  Requires mock-factory patterns, handler-testing patterns, module wiring
  VERDICT: dedicated session file

Phases C + E + F: individually manageable
  Can share one file with clear section markers
  VERDICT: one combined file
```

### Criterion 4: Companion document requirement

Every session file must end with:
1. STATE.json update
2. Phase completion package (EXECUTION-LOG + PHASE-COMPLETE + SESSION-BRIEF)
3. STOP gate

A phase that produces a companion document for the next session needs a clean
termination point that is easy to locate in the session file.

---

## Section 4 — Document Architecture

Before writing any session file, write a short document architecture block:

```
DOCUMENT ARCHITECTURE:

CAPABILITY-ID-INFRASTRUCTURE-GATE.md
  Purpose: All N prerequisite checks. Initializes STATE.json.
  New vs existing: M original checks + K new checks for new infrastructure.
  Ends with: STATE.json initialized. STOP.

CAPABILITY-ID-SESSION-A.md
  Purpose: Phase A — INJECT
  New vs existing: N new steps prepended (gap scan, validation baseline)
  Existing content: all M fixture seedings unchanged
  Ends with: Phase completion package. STOP.

CAPABILITY-ID-SESSION-B.md
  Purpose: Phase B — GENERATE (dedicated — context loss risk, deep branching)
  New vs existing: pre-cycle gap scan, post-cycle distillation verify, validation gate
  Existing content: cycle loop, score brackets, phase gate unchanged
  Ends with: Phase completion package. STOP.

CAPABILITY-ID-SESSIONS-C-E-F.md
  Phases C, E, F combined (each ends with STOP)
```

---

## Section 5 — Unchanged List Hard Rule

Without the explicit UNCHANGED list, a session executing the restructured plan will
re-examine and potentially modify correct content. Write the list. Name each thing.
Do not write "most things are unchanged" — that is not a list.

The UNCHANGED list transforms intent into a verifiable record. When a future session
reviews the plan and sees "PromptPatch formula (unchanged)" it knows not to touch
the formula. Without that explicit record, the formula is indistinguishable from
content that needs updating.

---

## Anti-Patterns

1. **Rewriting the entire plan** — the delta list identifies what changed. If something
   is unchanged, preserve it verbatim. Rewrites introduce errors into correct content.

2. **Session files that don't cold-start** — every session file begins with:
   read STATE.json, verify previous phase complete, load governance chain.
   A file that assumes context from a prior session's memory will fail when loaded fresh.

3. **SESSION-BRIEF that says "see previous session"** — the brief must contain the key
   facts, not a pointer to them. Cross-session context does not flow automatically.

4. **Combining phases with context loss risk** — Phase B of any generation capability
   must be its own session file. The branching (score brackets × capability classes ×
   cycles) cannot be safely compressed into a shared file.

5. **Not writing the unchanged list** — without it, the executor will re-examine and
   potentially modify correct content. Write it. Name each thing.

---

## Integration

- subproject-examination-SKILL.md: creates plans from scratch (new capabilities);
  this skill restructures existing plans (adding new infrastructure)
- capability-improvement-cycle-SKILL.md: cycle structure unchanged by restructure —
  what changes is the gates around each cycle
- session-execution-log-SKILL.md: log format applies to all session files produced
- phase-completion-packager-SKILL.md: completion package format applies unchanged
- session-handoff-SKILL.md: SESSION-BRIEF format applies unchanged; content must
  be self-contained
