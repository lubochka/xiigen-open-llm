# Capability Simulation

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

A capability plan that has never been traced end-to-end will fail at the
handler boundary the plan did not anticipate. Reviewing the plan in prose
is insufficient — prose descriptions do not expose the moment where an
input shape does not match what the next handler expects, or where the
learning path silently skips a stage. Simulation forces every plan step
through a structured table that exposes gaps before implementation begins,
not after the first test run.

---

## When to Invoke

- Before writing any new capability class implementation
- When a topology contract (ai-pipeline-topology-SKILL.md) is finalized and before Phase B begins
- When a capability change modifies an interface that feeds a downstream handler
- When a SILENT_FAILURE verdict has been identified in any capability run
- During plan review as part of verification-before-completion-SKILL.md

---

## Section 1 — Step Table Format

Every simulation uses a step table. One row per handler in the execution path:

```
| Step | What must happen | Handler | Input | Output expected | Actual | Gap |
|------|-----------------|---------|-------|-----------------|--------|-----|
```

Column definitions:

```
Step:             Sequential number (1, 2, 3...)
What must happen: One sentence — the business action at this step
Handler:          The .NET class or method that executes this step
Input:            ✅ shape is correct  |  ⚠️ shape is uncertain  |  ❌ shape is wrong
Output expected:  The exact OperationResult<T> type and payload this step should produce
Actual:           What the handler actually returns (filled during simulation run)
Gap:              GAP-ID if a discrepancy is found; empty if none
```

Example row:

```
| 3 | IncrementalTrainer updates confidence for the capability | IncrementalTrainer.Update() | ✅ DomainModelRegistry.GetEdges() result | OperationResult<TrainingResult> with updated Confidence | OperationResult<TrainingResult> with Confidence = 0.0 | GAP-003 |
```

---

## Section 2 — Five Simulation Verdicts

Each step receives one verdict after the simulation run:

| Verdict | Meaning |
|---------|---------|
| `WORKS` | Step produces the expected output shape and value |
| `PARTIAL` | Step produces a result, but the shape or a field value is wrong |
| `BREAKS` | Step returns an error OperationResult — execution cannot continue |
| `WRONG` | Step produces OperationResult.Ok() but with incorrect payload |
| `SILENT_FAILURE` | Step produces OperationResult.Ok(), correct shape, but a field holds the wrong value — no visible error |

**`SILENT_FAILURE` is the highest priority verdict.** The engine continues
running. Downstream handlers consume wrong data. The learning pipeline
stores wrong training signals. No test fails at this step — failures appear
later and are misattributed to downstream code.

---

## Section 3 — SILENT_FAILURE Patterns in .NET

The three most common SILENT_FAILURE patterns in this project:

```
Pattern 1: OperationResult.Ok() with wrong payload field
  Example: IncrementalTrainer.Update() returns Ok(result) where result.Confidence
           was read from Context.ConfidenceScore (which is null) instead of
           Edges.First().Confidence (which is populated).
           Symptom: Confidence = 0.0, engine continues, all downstream uses 0.0.

Pattern 2: machine_constant hardcoded but compiles
  Example: ScoreResult.MachineConstant is set to 1.0 unconditionally instead of
           reading from DomainModelRegistry.
           Symptom: quality gate never triggers hard stop; capability appears to pass.

Pattern 3: IncrementalTrainer reading wrong DomainModelRegistry path
  Example: trainer reads domainResult.Context.PatternScore instead of
           domainResult.Edges[0].PatternScore.
           Symptom: PatternScore = default(double), training data corrupted.
```

When a SILENT_FAILURE is found: file a Class D bug using pipeline-bug-taxonomy-SKILL.md
before writing any fix. A SILENT_FAILURE that is fixed without a bug record
has no test coverage and will reappear.

---

## Section 4 — Three Depth Levels

Choose depth based on what needs verification:

```
L1 — Decomposition
  Purpose: Verify that the capability class breaks down into the correct handler steps
  What to trace: input shape → step sequence → output shape
  What to skip: learning path, DPO (Direct Preference Optimization) signal emission
  When to use: early plan review before topology contract is written

L2 — Full Handler Chain
  Purpose: Verify every handler's OperationResult<T> contract end-to-end
  What to trace: all steps including validate.handler, score.handler, route.handler
  What to skip: DPO triple contents, curriculum tier assignment
  When to use: after topology contract is finalized, before Phase B begins

L3 — Full with Learning Path
  Purpose: Verify the complete execution including feedback node and learning signals
  What to trace: all steps + feedback.handler signal emission + DomainModelRegistry writes
  What to skip: nothing — full path
  When to use: when capability runs in production or when IncrementalTrainer calibration is wrong
```

---

## Section 5 — Gap Format

Every gap found during simulation uses this record:

```
GAP-ID:       Sequential per simulation (GAP-001, GAP-002...)
NAME:         One noun phrase describing what is missing or wrong
ROOT CAUSE:   The mechanism — what the code does wrong, not symptoms
SYMPTOM:      What the user or test observes
SYMPTOM_TEST: The exact assertion that will fail when this gap exists
FIX CLASS:    A | B | C | D | E | F (from pipeline-bug-taxonomy-SKILL.md)
PRIORITY:     SILENT_FAILURE | BREAKS | PARTIAL | WRONG | COSMETIC
```

Example:

```
GAP-ID:       GAP-003
NAME:         IncrementalTrainer reads confidence from wrong field path
ROOT CAUSE:   Update() reads domainResult.Context?.ConfidenceScore which is always null;
              confidence is written to domainResult.Edges[0].Confidence by DomainModelRegistry
SYMPTOM:      All capability confidence values remain at 0.0 after training runs
SYMPTOM_TEST: Assert.NotEqual(0.0, result.Confidence) — fails because Confidence is always 0.0
FIX CLASS:    D (Pipeline Stage — wrong data from upstream stage)
PRIORITY:     SILENT_FAILURE
```

---

## Section 6 — Simulation Run Protocol

```
Phase 1: Select depth level (L1 / L2 / L3)
Phase 2: Build the step table — one row per handler in the selected depth
Phase 3: For each row: determine Input status (✅/⚠️/❌) based on the upstream row's Output expected
Phase 4: For each row: run or trace the handler — record Actual
Phase 5: Compare Output expected vs Actual — assign verdict (WORKS/PARTIAL/BREAKS/WRONG/SILENT_FAILURE)
Phase 6: For every non-WORKS verdict: create a GAP record
Phase 7: Order gaps by PRIORITY (SILENT_FAILURE first, then BREAKS, then others)
Phase 8: File each gap as a bug using pipeline-bug-taxonomy-SKILL.md before writing fixes
```

---

## Anti-Patterns

1. **"I reviewed the plan in prose — simulation is redundant"** — prose review
   does not expose shape mismatches at handler boundaries. Simulation forces
   each step to name its exact OperationResult<T> type. Prose cannot do this.

2. **"SILENT_FAILURE is low priority — no test fails"** — SILENT_FAILURE is
   HIGHEST priority because it corrupts training data silently. A BREAKS verdict
   stops the engine; a SILENT_FAILURE trains the engine on wrong data.

3. **"I'll use L1 depth for a production capability change"** — L1 skips the
   learning path. Any change that could affect IncrementalTrainer or
   DomainModelRegistry requires L3 depth.

4. **"I found a gap and fixed it without a bug record"** — a gap fixed without
   a bug record has no test coverage. File the pipeline-bug-taxonomy-SKILL.md
   record first; write the bug-to-tests-SKILL.md failing test second; fix third.

5. **"Input ✅ means no gap is possible at this step"** — Input ✅ means the
   shape is correct. It does not mean the value is correct. SILENT_FAILURE
   occurs on ✅ input when the field path is wrong inside the handler.

---

## Integration

- ai-pipeline-topology-SKILL.md topology contract is the input to L2/L3 simulation
- pipeline-bug-taxonomy-SKILL.md receives gap records classified by Fix Class
- bug-to-tests-SKILL.md writes failing tests from the SYMPTOM_TEST field of each gap
- learning-signal-capture-SKILL.md Section 3 (DPO validity gate) is verified in L3 simulation
- verification-before-completion-SKILL.md requires L2 simulation before claiming a capability complete
