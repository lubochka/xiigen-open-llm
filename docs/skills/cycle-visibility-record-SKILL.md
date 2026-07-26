# Cycle Visibility Record

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

A capability pipeline cycle that produces output but no visibility record
cannot be audited, debugged, or learned from. Its output stands without evidence.
When a later cycle produces wrong output, you cannot trace which earlier decision
caused it — the only option is to re-run and hope. This skill defines the 4-field
visibility record required for every AI pipeline execution cycle and the exact
content each field must contain.

---

## When to Invoke

- When implementing any ai-pipeline-topology-SKILL.md feedback node
- When a cycle produces wrong output and the cause is unclear
- When verifying that a Phase E promotion is backed by auditable evidence
- During capability-simulation-SKILL.md L3 depth simulation

---

## Section 1 — The Governing Rule

```
A cycle with no visibility record is a cycle that never ran.
Its output cannot be trusted and its learning cannot be captured.
```

Every topology execution cycle must produce one visibility record.
All four fields are required. An empty field is a failed record.

Count honesty rule: a row in a ledger, checklist, generated table, or summary
is not a cycle. A claim such as "50 cycles completed" is valid only when there
are 50 cycle visibility records or equivalent independently inspectable
before/after review/fix evidence records. If the records do not exist, the
honest claim is "50 ledger rows exist; 50 cycles are not proven."

---

## Section 2 — The 4-Field Structure

### SENT — the full context package given to the AI

```
Definition:   The complete context package for this cycle.
Required:     All 5 fields from context-package-design-SKILL.md
              (INTENT / DOMAIN / CONSTRAINTS / PRIOR_ART / SUCCESS)
              with their actual values for this execution, not a reference.
Complete means: Every field present with the values actually used.
Empty fails because: Without SENT, you cannot verify the AI operated
              within correct constraints. If the score fails, you cannot
              determine whether the context package or the AI judgment
              was wrong.
```

### RECEIVED — each model's output, labelled and unmodified

```
Definition:   The raw output from each model called.
Required:     One entry per model, labelled A / B / C (shuffled —
              model identity not revealed in the label until after judging).
Complete means: N entries where N = number of models called.
              Each entry: label, full output text, token count.
              "Full output" = complete text, not a summary.
Empty fails because: Without RECEIVED, you cannot verify multi-model
              generation ran correctly or that the arbiter saw all candidates.
```

### DECIDED — the winner and the arbiter's reasoning

```
Definition:   The winning candidate label plus the reasoning.
Required:     Winner label AND reasoning that references at least one
              criterion from the SENT context package.
Complete means:
  "Winner: [label] — [reasoning citing the specific SENT criterion
   that distinguished winner from runner-up]"
  PLUS each arbiter's verdict (APPROVED / CONCERN / BLOCK) with
  the specific criterion each arbiter evaluated.

Failing examples:
  ❌ "Winner: B"                     (no reasoning)
  ❌ "Winner: B — better quality"    (no criterion reference)
  ✅ "Winner: B — only candidate where CONSTRAINTS Rule 1 (OperationResult<T>
      everywhere) is respected. Candidates A and C return void."

Empty fails because: The reasoning is what makes the decision auditable.
              Without it, arbiter reliability cannot be evaluated over time.
```

### CHANGED — what was updated in the learning index as a result

```
Definition:   What in DomainModelRegistry, the training index, or the RAG
              pattern store was updated as a result of this cycle.
Required:     At minimum one entry naming the store updated and the key.
Complete means:
  "[store name] key [key value] updated with [what changed]"
  Example: "DomainModelRegistry edge SCORING:DETAIL_GAP→ROUTES_TO updated
            — confidence 0.62 → 0.71"
  "Nothing changed" is valid ONLY when explicitly stated with reasoning:
  "CHANGED: nothing — this is a re-run of an existing step with the same
            score, no new learning signal generated."

Empty without statement fails because: Silence cannot distinguish
              "nothing changed" from "the CHANGED field was forgotten."
```

---

## Section 3 — C# Record

```csharp
public record CycleVisibilityRecord
{
    public string CycleId { get; init; }
    public string TopologyId { get; init; }
    public string CapabilityId { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    public ContextPackage Sent { get; init; }          // 5-field context package

    public List<ModelOutput> Received { get; init; }  // one per model, shuffled labels

    public DecidedRecord Decided { get; init; }        // winner + reasoning + arbiter verdicts

    public ChangedRecord Changed { get; init; }        // what was updated; NothingChanged flag
}

public record ModelOutput
{
    public string Label { get; init; }       // "A" | "B" | "C" (shuffled)
    public string FullText { get; init; }    // complete output, not summary
    public int TokenCount { get; init; }
}

public record DecidedRecord
{
    public string WinnerLabel { get; init; }
    public string ReasoningWithCriterionCite { get; init; }
    public List<ArbiterVerdict> ArbiterVerdicts { get; init; }
}

public record ChangedRecord
{
    public bool NothingChanged { get; init; }
    public string NothingChangedReason { get; init; }  // required when NothingChanged = true
    public List<LearningUpdate> Updates { get; init; }
}

public record LearningUpdate
{
    public string StoreName { get; init; }
    public string Key { get; init; }
    public string Description { get; init; }
}
```

---

## Section 4 — Validation

Before writing the visibility record to ISessionStorage, validate all four fields:

```csharp
public static class VisibilityRecordValidator
{
    public static OperationResult<Unit> Validate(CycleVisibilityRecord record)
    {
        if (record.Sent is null || string.IsNullOrEmpty(record.Sent.Intent))
            return OperationResult<Unit>.Fail("SENT.Intent is required", "VISIBILITY_SENT_MISSING");

        if (record.Received is null || record.Received.Count == 0)
            return OperationResult<Unit>.Fail("RECEIVED must have at least one model output", "VISIBILITY_RECEIVED_EMPTY");

        if (string.IsNullOrEmpty(record.Decided?.ReasoningWithCriterionCite))
            return OperationResult<Unit>.Fail("DECIDED requires reasoning with criterion cite", "VISIBILITY_DECIDED_NO_REASONING");

        if (record.Changed is null)
            return OperationResult<Unit>.Fail("CHANGED field is required", "VISIBILITY_CHANGED_MISSING");

        if (record.Changed.NothingChanged && string.IsNullOrEmpty(record.Changed.NothingChangedReason))
            return OperationResult<Unit>.Fail("NothingChanged requires explicit reason", "VISIBILITY_NOTHING_CHANGED_NO_REASON");

        return OperationResult<Unit>.Ok(Unit.Value);
    }
}
```

---

## Anti-Patterns

1. **"DECIDED field = winner only"** — a winner without reasoning cannot be audited.
   "Winner: B" with no criterion reference is an empty DECIDED field.

2. **"CHANGED field empty when nothing changed"** — empty is different from
   explicitly stating nothing changed. The field must contain one or the other.

3. **"SENT field references another document instead of containing the values"** —
   "see context package v2.3" is not a SENT field. The field must contain the
   actual values used in this execution, not a pointer.

4. **"Summary in RECEIVED instead of full output"** — the arbiter made its
   decision on the full output. A summary cannot reproduce the decision if
   re-verification is needed.

---

## Integration

- ai-pipeline-topology-SKILL.md feedback node is responsible for writing the visibility record
- learning-signal-capture-SKILL.md OUTCOME signal is derived from the CHANGED field
- capability-simulation-SKILL.md L3 depth verification checks that visibility records exist after simulation
- learning-path-audit-SKILL.md checks whether visibility records are being written (backward-looking)
- context-package-design-SKILL.md defines the 5-field structure used in SENT
