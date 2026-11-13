# Session Scope Resolution

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

## Authority Requirement Binding Override

Authority-chain requirement binding applies before every session dispatch.
Requirements from the current human operator / product owner, architect work
order, approved active plan, or accepted reviewer gate are binding unless that
authority explicitly marks them optional, exploratory, or preference-only. A
downstream session must not narrow an upstream requirement. Load
authority-requirement-binding-SKILL.md before planning, reviewing, issuing work
orders, or executing a bounded work order.

## Core Problem

A session that loads the wrong documents, runs the wrong governance chain,
or misses a blocking condition before starting will either do the wrong work
or produce invalid output. The session type (ARCHITECT, EXECUTOR, INVESTIGATOR,
MAINTAINER, DEBUG) determines which documents to load, which skills to run first,
and what blocking conditions prevent starting. This skill is the dispatch table
that maps session type → minimum documents + governance chain + blocking conditions.

---

## When to Invoke

- At every session start, after session-mode-declaration-SKILL.md declares the mode
- Before loading any project documents (determine what to load first)
- When planning-session-startup-SKILL.md Q1 (mode + scope) is answered

---

## Section 1 — Dispatch Table

### EXECUTOR SESSION

Work target: implementing a phase in a specific subproject

```
PREREQUISITE CHECKS:
  → dotnet build — must show 0 errors
  → dotnet test — record baseline (must be ≥ prior baseline)
  → If any test fails at session start: STOP. Fix before any implementation.

MINIMUM DOCUMENTS:
  1. CLAUDE.md + AGENTS.md                           (every session)
  2. docs/DECISIONS-LOCKED.md                        (locked decisions D-01..D-NN)
  3. docs/COMPONENT-INTERFACES.md §[relevant section] (interface contracts for this phase)
  4. The phase plan file for the current phase       (what to implement)

GOVERNANCE CHAIN (mandatory, in order):
  1. session-output-contract-SKILL.md                ← define done BEFORE reading any files
  2. phase-preflight-SKILL.md                        ← assumption registry before code
  3. pipeline-position-check-SKILL.md                ← Q0a-Q0d before any code
  4. dna-compliance-guard-SKILL.md                   ← for each new .cs file created
  5. verification-before-completion-SKILL.md         ← before claiming any phase done

BLOCKING CONDITIONS:
  - dotnet test failures > 0 at session start → STOP. Fix before phase work.
  - Locked decision (D-NN) exists that contradicts the phase plan → STOP.
    Route to decision-reopening-SKILL.md before proceeding.
  - Required interface missing from COMPONENT-INTERFACES.md → emit CONTEXT_INSUFFICIENT.
```

---

### ARCHITECT SESSION

Work target: designing phases, writing plans, reviewing architecture

```
PREREQUISITE CHECKS:
  → dotnet build — must show 0 errors (plans cannot assume broken baseline)
  → Architect sessions are rarely blocked; check only if prior session left open issues

MINIMUM DOCUMENTS:
  1. CLAUDE.md + AGENTS.md                           (every session)
  2. docs/DECISIONS-LOCKED.md                        (load first — do not propose what's locked)
  3. docs/SUBPROJECT-MAP.md                          (current subproject statuses)
  4. docs/ARCHITECTURE.md                            (if designing new components)
  5. docs/COMPONENT-INTERFACES.md                    (for interface design)

GOVERNANCE CHAIN (mandatory, in order):
  1. planning-session-startup-SKILL.md               ← 5 pre-plan questions before first output
  2. session-output-contract-SKILL.md                ← output contract before any work
  3. plan-review-SKILL.md                            ← current FC gates, including trainable-architecture gate when triggered, before any plan is final
  4. goal-delivery-completeness-SKILL.md             ← first arbiter check on any plan
  5. documentation-sync-SKILL.md                     ← at session end

BLOCKING CONDITIONS:
  - Any proposal contradicts DECISIONS-LOCKED.md → STOP. Run decision-reopening-SKILL.md.
  - Plan has no Gate B for any phase → BLOCK (plan-review-SKILL.md FC-4).
```

---

### INVESTIGATOR SESSION

Work target: diagnosing gaps, tracing failures, simulation

```
PREREQUISITE CHECKS:
  → None. Investigator sessions can start from any state.
  → Load the specific artifact being investigated first, not reference documents wholesale.

MINIMUM DOCUMENTS:
  1. The specific file being investigated            (targeted, not wholesale)
  2. docs/DECISIONS-LOCKED.md                       (to check if the gap is a known issue)
  DO NOT load ARCHITECTURE.md or COMPONENT-INTERFACES.md upfront —
  load specific sections when needed

GOVERNANCE CHAIN (mandatory):
  1. claim-as-hypothesis-SKILL.md                   ← verify claims before investigation
  2. problem-decomposition-SKILL.md                 ← ask "why" 3× before proposing fixes
  3. root-cause-ladder-SKILL.md                     ← after gap catalog produced
  4. investigation-handoff-SKILL.md                 ← at session end

BLOCKING CONDITIONS:
  - None. Investigation sessions are always safe to start.
  - Any issue found during investigation: ISSUE INVENTORY entry required before
    session ends (FIXED / DEFERRED with authorization / EXCEPTION — not "noted").
```

---

### MAINTAINER SESSION

Work target: fixing files, updating skills, syncing documentation

```
PREREQUISITE CHECKS:
  → Run blast-radius-SKILL.md before any file changes.
  → If the change touches tests: dotnet test must pass after the change.

MINIMUM DOCUMENTS:
  1. docs/DECISIONS-LOCKED.md                       (to know what's locked)
  2. The specific file being updated                 (read before touching — Rule 5)
  DO NOT load reference documents unless the change requires interface verification.

GOVERNANCE CHAIN (mandatory):
  1. blast-radius-SKILL.md                          ← FIRST, before any change
  2. change-propagation-SKILL.md                    ← derived views must update in same commit
  3. verification-before-completion-SKILL.md        ← before declaring any change done
  4. documentation-sync-SKILL.md                    ← at session end

BLOCKING CONDITIONS:
  - Any issue found: FIXED / DEFERRED (authorization + DECISIONS-LOCKED note) / EXCEPTION.
    "Noted" is not a valid status. "Out of scope" is not a valid status.
  - If tests touched: dotnet test must show 0 failures before session ends.
  - One commit per logical change (not one commit per file — Rule 6).
```

---

### DEBUG SESSION

Work target: specific failing test, broken method, broken command

```
PREREQUISITE CHECKS:
  → None. Load the failing test output or error message directly.
  → Do NOT load any document that doesn't contain the failing code.

MINIMUM DOCUMENTS:
  1. The failing test file or error output          (direct and targeted)
  2. The implementation file being debugged         (direct)
  Nothing else unless the root cause trace leads there.

GOVERNANCE CHAIN (mandatory):
  1. bug-to-tests-SKILL.md                          ← discriminating regression case and appropriate controls; no fixed red-test quota
  2. root-cause-tracing-SKILL.md                    ← trace backward through call chain
  3. three-level-verification-SKILL.md              ← after fix: unit→integration→suite
  4. verification-before-completion-SKILL.md        ← before declaring bug fixed

BLOCKING CONDITIONS:
  - Every issue found during debug: ISSUE INVENTORY entry required.
    FIXED / DEFERRED (authorization) / EXCEPTION — not "out of scope."
  - The fix must pass dotnet test with 0 failures before session ends.
```

---

## Section 2 — What This Skill Does NOT Contain

```
✗ Prerequisite verification commands — those are in prerequisite-chain-SKILL.md
✗ Bug fix procedures — those are in bug-to-tests-SKILL.md
✗ Flow-specific decisions — those are in DECISIONS-LOCKED.md
✗ Interface contracts — those are in COMPONENT-INTERFACES.md
```

This skill is a dispatch table. It routes to the right skills and documents
for the session type. The content of those skills and documents is authoritative;
this skill is the lookup, not the source.

---

## Anti-Patterns

1. **"Loading all reference documents at session start regardless of type"** —
   an INVESTIGATOR session that loads ARCHITECTURE.md upfront is loading what
   it intends to investigate. Load targeted documents first; expand as the
   trace requires.

2. **"Starting EXECUTOR work without an output contract"** — session-output-contract-SKILL.md
   is the zeroth step for every EXECUTOR session. Define done before reading files.

3. **"MAINTAINER session ends without ISSUE INVENTORY"** — every issue found
   during maintenance must be classified FIXED/DEFERRED/EXCEPTION. "Noted" means
   the issue will recur in the next session.

4. **"ARCHITECT session proposes something without loading DECISIONS-LOCKED.md"** —
   DECISIONS-LOCKED.md is the first document in the ARCHITECT minimum list for this
   reason. A proposal that contradicts D-NN is wasted design work.

---

## Integration

- session-mode-declaration-SKILL.md declares the mode that feeds this skill's session type input
- planning-session-startup-SKILL.md Q1 (mode + scope) is where this skill's output is produced
- prerequisite-chain-SKILL.md handles prerequisite verification for all session types
- session-output-contract-SKILL.md is the zeroth step for EXECUTOR and ARCHITECT sessions
- verification-before-completion-SKILL.md is the final step for all session types
