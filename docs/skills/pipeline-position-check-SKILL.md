# Pipeline Position Check

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

A phase that implements a component without knowing what its downstream consumer
needs from it produces locally correct output that is globally useless. The
component compiles, passes tests, and still fails to serve its purpose because
the wrong shape was produced. This skill defines the four pre-phase questions
that must be answered before any file is read or any code is written.

---

## When to Invoke

- Before Q1 of any implementation session
- Before Phase A of any phase plan that produces output consumed by a downstream phase
- When session scope is "fix the interface between Phase N and Phase N+1"
- When a component passes Gate B but the downstream phase still fails

---

## Section 1 — The 4-Part Pipeline Position Declaration

Answer all four parts explicitly before any file is read or any code is written.
If Q0d cannot be answered with specificity, emit a CONTEXT_INSUFFICIENT signal
(Section 2) instead of proceeding.

### Q0a — What does this phase RECEIVE as input?

State: which upstream phase or component produces the input, field names, required shape.

```
Example: "IncrementalTrainer.Train() receives a TrainingBatch from
          ContinuousLearningPipeline after outcome observation.
          Required fields: SessionId, DpoTriples[], CapabilityClass"
```

### Q0b — What does this phase PRODUCE as output?

State: field names, required shape, persistence destination.

```
Example: "IncrementalTrainer.Train() produces an UpdatedModelWeights record
          stored in ISessionStorage keyed by SessionId + CapabilityClass.
          Required fields: Weights[], LoraAdapterPath, ObservationCount"
```

### Q0c — Which component CONSUMES this output?

State: which downstream component reads this output and what it does with it.

```
Example: "DomainModelRegistry reads UpdatedModelWeights after IncrementalTrainer
          completes, to update the confidence of CapabilityClass edges.
          It uses ObservationCount to apply decay protection."
```

### Q0d — What does the consumer NEED from this output?

Answer this from the consumer's perspective, not the producer's.

```
BAD:  "DomainModelRegistry needs the UpdatedModelWeights"  ← restatement
GOOD: "DomainModelRegistry needs ObservationCount to be the CUMULATIVE count
      (not the delta for this training run) because decay protection is calculated
      from total evidence, not incremental evidence. If ObservationCount is a delta,
      confidence will be under-protected and 5 outliers can override 30 good runs."
```

**The gate rule:** If Q0d produces "I'm not sure" or a generic restatement of the
output fields, the session is not ready to start. Emit CONTEXT_INSUFFICIENT.

---

## Section 2 — CONTEXT_INSUFFICIENT Signal

When Q0d cannot be answered with specificity, emit this signal. This is correct behavior,
not a failure. The session halts here and waits for resolution.

**Signal format — all fields required:**

```
CONTEXT_INSUFFICIENT

phase:    [which phase this session is implementing]
gap:      [what specific information is missing — not vague, not "more context"]
consumer: [which downstream phase or component is affected]
impact:   [what quality failure will occur if this proceeds without resolution — be specific]
request:  [3-6 concrete questions Luba can answer in one sentence each]
```

**Example — consumer requirement unknown:**

```
CONTEXT_INSUFFICIENT

phase:    IncrementalTrainer.Train() implementation in 22.AgentAlgorithms Phase 5
gap:      Unknown whether ObservationCount field should be cumulative or delta.
          CLAUDE.md and COMPONENT-INTERFACES.md are silent on this field's semantics.
consumer: DomainModelRegistry.UpdateConfidence() in Phase 6
impact:   If delta is returned instead of cumulative, DomainModelRegistry will under-
          protect confidence (decay window = max(5, count/4)). 5 outliers could then
          override 30 good observations. Learning loop will be fragile.
request:
  1. Should ObservationCount in UpdatedModelWeights be the total count since
     capability was first registered, or the count added in this training run?
  2. Does DomainModelRegistry expect to add this count to its own stored count,
     or replace its stored count with this value?
  3. Is there a DECISIONS-LOCKED.md entry I should read before implementing this?
```

---

## Section 3 — Resolution Protocol

**Step 1 — Session halts**

After emitting CONTEXT_INSUFFICIENT, the session halts completely.
No files are read. No code is written.
State explicitly: "Session halted. Awaiting context resolution."

**Step 2 — Luba provides resolution**

Luba responds to the concrete questions. One sentence per question is sufficient.

**Step 3 — Resumption gate**

```
Before resuming, verify:
□ Each question in `request` has a specific answer (not "use your judgment")
□ The answers together close the `gap` stated in the signal
□ Q0d can now be answered with specificity using the answers
```

If any item fails → re-emit CONTEXT_INSUFFICIENT with the remaining gap.

**Step 4 — Update design artifacts**

Before writing any code, update the relevant design artifacts (iron rules, interface
contracts, COMPONENT-INTERFACES.md) with the resolved information. The resolution
prevents the same gap from recurring in future sessions.

---

## Section 4 — Pipeline Contract Check (Before Gate B)

After implementation, before declaring Gate B passed:

```
□ Q0a verified: input shape received matches what was declared
□ Q0b verified: output shape produced matches what was declared
□ Q0c verified: consumer can receive this output (no interface mismatch)
□ Q0d verified: consumer requirement is met by what was produced
               If not → surface gap, resolve, then proceed to Gate B
```

Passing test count is necessary but not sufficient. A phase that produces locally
correct output failing Q0d has produced noise, not a deliverable.

---

## Section 5 — Pipeline Data Contracts Reference

Consumer requirement for each common phase boundary in this project:

```
PHASE BOUNDARY     CONSUMER REQUIREMENT
─────────────────────────────────────────────────────────────────────────────
ContinuousLearning → IncrementalTrainer:
  Trainer needs DPO triples with genuine chosen/rejected pairs.
  If both come from the same source, training produces overfit corrections.
  Triples must have different Sources for chosen vs rejected.

IncrementalTrainer → DomainModelRegistry:
  Registry needs cumulative ObservationCount, not delta.
  Decay protection depends on total evidence accumulated, not per-run evidence.

DomainModelRegistry → ai-pipeline-design-SKILL.md:
  AI pipeline needs the confidence BEFORE the current run to determine
  whether AI is needed at all. Use the pre-update confidence for threshold check.

HybridRetriever → ContinuousLearningPipeline:
  Learning pipeline needs both the score AND the query context that produced it.
  Score alone cannot be attributed to a specific retrieval strategy.
```

---

## Anti-Patterns

1. **"Implementing a phase without stating Q0d first"** — if Q0d is unknown, emit
   CONTEXT_INSUFFICIENT before touching any file.

2. **"Q0d answered as a restatement: 'DomainModelRegistry needs the output'"** — this
   is not an answer. State what specific property of the output the consumer depends on.

3. **"'Looks correct' means done"** — output that looks well-formed but fails Q0d's
   consumer requirement is incorrect. Verify Q0d before claiming Gate B.

4. **"Vague CONTEXT_INSUFFICIENT requests"** — "Please provide more context" shifts
   work back to Luba without narrowing the gap. Name the specific field and why it matters.

5. **"Resuming without updating design artifacts"** — the resolution closes a gap.
   That gap should be recorded so the same question doesn't recur.

---

## Integration

- planning-session-startup-SKILL.md Q4 (blockers) produces the prerequisite context for Q0a
- COMPONENT-INTERFACES.md is the canonical source for interface shapes needed in Q0b-Q0c
- implementation-integrity-SKILL.md runs Q0c verification as its Check 3 (no new interface gaps)
- api-shape-verification-SKILL.md verifies Q0c's interface mismatch check
- context-package-design-SKILL.md structure maps to Q0a/Q0b/Q0c/Q0d fields
