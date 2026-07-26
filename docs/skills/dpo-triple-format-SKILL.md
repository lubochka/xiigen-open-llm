# DPO Triple Format

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

A DPO (Direct Preference Optimization) triple that is missing its context
package cannot be used for fine-tuning — the model trains on code pairs
without knowing what prompt produced which code. A triple without a context
package is not a training record; it is two code snippets with a preference
label. This skill specifies the complete 6-field DPO triple format, the source
node that produces each field, the minimum triple count per capability class,
and the promotion gate that verifies count before ACTIVE status.

---

## When to Invoke

- When implementing or reviewing feedback.handler in any capability topology
- When a capability reaches the promotion threshold but DPO count is below minimum
- When `n8.DpoTripleId == null` after a score improvement (broken capture)
- When learning-path-audit-SKILL.md reports DPO_TRIPLE FIRES=NO or field gaps

---

## Section 1 — The 6 Mandatory Fields

A DPO triple has exactly 6 mandatory field groups. All 6 must be present
before the record is stored to the training data index.

```csharp
public record DpoTriple
{
    // Field group 1: Identity
    public string TripleId { get; init; }    // "dpo-{CapabilityId}-{CapabilityClass}-{RunId}"
    public string CapabilityId { get; init; }
    public string CapabilityClass { get; init; }
    public string RunId { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
    public string ContextPackageVersion { get; init; }

    // Field group 2: Context (what the AI was given)
    public ContextForTriple Context { get; init; }

    // Field group 3: Rejected output (what scored below 0.85)
    public DpoOutput Rejected { get; init; }

    // Field group 4: Chosen output (what scored >= 0.85)
    public DpoOutput Chosen { get; init; }

    // Field group 5: Teaching point (why chosen > rejected)
    public string TeachingPoint { get; init; }

    // Field group 6: Curriculum tier (training sequence position)
    public int CurriculumTier { get; init; }  // 1-5; never null
}

public record ContextForTriple
{
    public string PromptSystem { get; init; }     // REQUIRED: full context package text
    public List<string> PatternsRetrieved { get; init; }  // RAG pattern IDs used
    public List<string> PlanSteps { get; init; }  // decompose.handler output
    public string CapabilityContract { get; init; }       // iron rules + quality gates
}

public record DpoOutput
{
    public string Code { get; init; }          // FULL generated code (not summary)
    public string Model { get; init; }         // e.g., "claude-opus-4-7"
    public double Score { get; init; }
    public List<string> Violations { get; init; }  // empty for chosen; failing checks for rejected
}
```

---

## Section 2 — Where Each Field Comes From

Each field is produced by a specific pipeline stage. The feedback.handler
(last node in the topology) assembles the triple from the upstream node outputs.

| Field | Source node | What to read |
|---|---|---|
| `Context.PromptSystem` | rag-retrieve.handler (genesis context) | Full context package assembled for this run |
| `Context.PatternsRetrieved` | rag-retrieve.handler output | Pattern IDs from the retrieval result |
| `Context.PlanSteps` | decompose.handler output | Method names / step names from the plan |
| `Context.CapabilityContract` | topology contract (static input) | Iron rules + quality gates for this capability |
| `Rejected.Code` | ai-generate.handler output from the failing cycle | Full generated output |
| `Rejected.Score` | score.handler output from the failing cycle | Score value |
| `Rejected.Violations` | validate.handler + score.handler output | All failing check names |
| `Chosen.Code` | ai-generate.handler output from the passing cycle | Full generated output |
| `Chosen.Score` | score.handler output from the passing cycle | Score value ≥ 0.85 |
| `Chosen.Violations` | validate.handler output | Empty list (all checks passed) |
| `TeachingPoint` | Derived from Rejected.Violations | One sentence: the specific error that distinguished rejected from chosen |
| `CurriculumTier` | Topology contract capability class | From learning-signal-capture-SKILL.md curriculum tier table |

**Critical:** If `Context.PromptSystem` is missing, the triple is unusable for
IncrementalTrainer fine-tuning. The model needs to train on full context, not
just code pairs. An empty PromptSystem must cause the triple to be rejected
at store time with status `PENDING_COMPARISON`, not stored to the main index.

---

## Section 3 — When a Triple Is Generated

A triple is captured when BOTH conditions are true:

```
Condition 1: A previous run for this capability class scored < 0.85
Condition 2: The current run scored >= 0.85
```

The feedback.handler is responsible for detecting this transition and assembling
the triple automatically. If `DpoTripleId == null` after a score improvement:
the triple capture is broken (Class D pipeline bug from pipeline-bug-taxonomy-SKILL.md).

When fewer than 2 model tiers are available (single-tier run): set status
`PENDING_COMPARISON` and log: "DPO triple pending — single-tier run." Do not
store to main training index until a cross-model comparison is available.

When all generators produce the same score (tie): set status `UNDECIDED` and
store separately for human labeling. Never store to main training index.

---

## Section 4 — Minimum Triple Count per Capability Class

Every capability class must produce a minimum number of triples before Phase E
(ACTIVE promotion). Minimum is 1 triple per distinct archetype within the class.

| Capability class | Minimum DPO triples | Rationale |
|---|---|---|
| ROUTING (CurriculumTier=1) | 1 | Single archetype, simpler patterns |
| DATA_PIPELINE (CurriculumTier=2) | 2 | Two typical patterns: transform + validate |
| VALIDATION (CurriculumTier=2) | 2 | Positive and negative validation cases |
| TRANSACTION (CurriculumTier=3) | 3 | Compensate, saga, basic transaction |
| ORCHESTRATION (CurriculumTier=4) | 3 | Multi-step, event-driven, gate events |
| SCHEDULED (CurriculumTier=5) | 3 | SLA window, virtual clock, state machine |

---

## Section 5 — Phase E Promotion Gate

Before marking any capability as ACTIVE (promoted), verify minimum triple count:

```csharp
public static class DpoPromotionGate
{
    // DPO-GATE-001: Verify minimum triple count before ACTIVE status
    public static OperationResult<Unit> VerifyMinimumTriples(
        string capabilityId,
        string capabilityClass,
        int actualTripleCount)
    {
        var minimum = GetMinimumTripleCount(capabilityClass);

        if (actualTripleCount < minimum)
            return OperationResult<Unit>.Fail(
                $"Capability {capabilityId} has {actualTripleCount} valid DPO triples; " +
                $"minimum for {capabilityClass} is {minimum}. " +
                $"Run more improvement cycles before promotion.",
                "DPO_COUNT_BELOW_MINIMUM");

        return OperationResult<Unit>.Ok(Unit.Value);
    }
}
```

A capability with 0 valid DPO triples cannot be promoted regardless of score.
The score measures output quality. The triple count measures learning signal
completeness. Both must be satisfied.

---

## Anti-Patterns

1. **"Rejected code alone is enough for training"** — the model trains on the
   full execution context. Without `Context.PromptSystem`, the model cannot
   learn which prompt constraints the winning code satisfied.

2. **"I'll store the triple now and add PromptSystem later"** — the DPO
   validity gate rejects triples with null PromptSystem at store time.
   There is no "add later" — assemble the complete triple before storing.

3. **"CurriculumTier can be null for now"** — a triple without CurriculumTier
   cannot be sequenced by IncrementalTrainer. The training pipeline assigns
   curriculum tiers at run time. Null means the triple is outside the curriculum.

4. **"The score improved, so the triple is captured"** — score improvement
   triggers triple capture only if feedback.handler is wired correctly. Use
   learning-path-audit-SKILL.md to verify capture is actually happening.

---

## Integration

- learning-signal-capture-SKILL.md Section 3 (DPO validity gate) defines the 3 cross-model conditions
- learning-signal-capture-SKILL.md Section 4 (curriculum tier table) provides CurriculumTier values
- learning-path-audit-SKILL.md Signal 1 audits whether this skill's requirements are met in live code
- IncrementalTrainer consumes DPO triples from the training index
- negative-example-patching-SKILL.md automatically captures a triple when a cycle first reaches ≥0.85

## Trainable Architecture and Set Quarantine

Before DPO/set review for a learned orchestration sphere, load
`trainable-architecture-decision-SKILL.md` and classify the intended capability
against the allowed trainable architecture levels.

For PromptToTask, OrchestrationReasoning, SkillSelection, ResultGap, or any
other learned orchestration sphere, DPO rows must train the intended universal
capability rather than a shortcut.

Reject or quarantine rows when the chosen/rejected difference is mostly:

- source row id;
- pair id;
- filename or path;
- candidate order;
- exact prompt memory;
- project-specific execution detail when the target model is universal;
- keyword-to-skill lookup instead of semantic task understanding;
- deterministic repair instruction instead of a learned behavior contrast.

Required set audit table:

```text
set or row | intended machine | allowed architecture level |
non-level classification | chosen signal | rejected signal |
universal vs project-specific boundary | leakage risk |
project-specific contamination risk | verdict | rebuild action
```

For rows that train a replacement `ILogicalUnit`, add the unit-learning fields:

```text
target ILogicalUnit:
typed trained state field updated:
KNN/nearest memory updated:
KNN/nearest role:
source root:
source license:
chosen teaches:
rejected teaches:
pair-building rule:
forbidden shortcut signals:
load/continue-training expectation:
```

Those fields must be filled with concrete row examples before the row can be
called train-ready. A schema-only DPO section is not training material.

For every replacement `ILogicalUnit`, include at least:

```text
positive example row:
  source_ref:
  license_or_private_leg:
  input prompt/task/available skills:
  chosen:
  rejected:
  exact state fields updated:
  KNN/nearest memory entry added:
  checkpoint written:

rejected example row:
  wrong learned signal:
  why rejected is plausible:
  why chosen wins:
  exact state mutation that must NOT happen:

quarantine example row:
  leakage signal:
  quarantine reason:
  rebuild action:
  train_ready=false
```

If the target unit replaces a dictionary/counter, the DPO example must say how
the old shortcut would have learned the wrong thing and how the new typed state
learns the intended boundary instead. Do not accept `row schema`,
`chosen/rejected`, or `quarantine` as a completed example without concrete
values.

Common-model DPO may use external sources only when the source and license are
recorded. Prefer MIT or Apache-2.0 sources for common training material. Unknown
license, GPL-family, or private user material must be rejected from common
training unless Luba explicitly approves a separate path. Private user prompts,
AI chats, local project notes, and local model corrections are valid for the
user-adaptive leg when redacted according to the active plan.

External common-training rows must name concrete candidate sources, not only a
license family. Candidate examples include Kubernetes docs repositories, Apache
Airflow docs/tutorials, VS Code extension samples, and similar repositories
only after the row records an upstream URL or local archive, license file/ref,
allowed fields, and the universal learning boundary. If the license file/ref is
missing, set `training_allowed=false` for common and route the row to
evidence-only or user-adaptive/private handling.

For every row with `rebuild action`, include the concrete rebuild rule, not
only a verdict. If a skill-selection row could be solved by candidate order, the
row is not train-ready until it has candidate-list permutation evidence:

```text
candidate_order_invariant=true/false
permutation_count=<number>
chosen_skill_stable_across_permutations=true/false
candidate_position_used_as_signal=false
```

If `chosen_skill_stable_across_permutations=false`, the row must be
`QUARANTINE_DO_NOT_TRAIN` or rewritten into a new DPO pair where the same chosen
skill is correct under at least two different candidate orders.

Do not build a DPO row that teaches a raw dictionary shortcut. If the intended
target is `_promptUnderstandingSignals`, `_taskUnderstandingSignals`,
`_promptContextSignals`, or `_intermediateChecksSignals`, first route the row to
the named replacement `ILogicalUnit` and its typed state. The row must say which
feature vector, nearest-memory entry, calibration value, or guard evidence it
updates.

Verdicts:

- `KEEP_FOR_TRAINING`;
- `KEEP_FOR_BASELINE_ONLY`;
- `KEEP_FOR_EVAL_ONLY`;
- `REWRITE_TO_DPO`;
- `QUARANTINE_DO_NOT_TRAIN`;
- `WRONG_DOMAIN_REMOVE_FROM_THIS_MACHINE`.
