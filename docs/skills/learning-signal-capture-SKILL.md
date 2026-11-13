# Learning Signal Capture

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

## Core Problem

A pipeline topology that executes correctly but emits no learning signals
is indistinguishable from a plain C# service — it runs and produces output,
but the engine does not improve from it. The feedback node in every topology
must capture at minimum one OUTCOME signal. Without explicit signal
selection, topologies accumulate without contributing to DomainModelRegistry
edge updates, IncrementalTrainer calibration, or the capability promotion
cycle. This skill defines the nine learning signal types, the selection
guide, the DPO validity gate, and the curriculum tier table.

---

## When to Invoke

- When writing any feedback node in an ai-pipeline-topology-SKILL.md contract
- During plan-review: verify every topology declares at least one learning signal
- When a capability's confidence never improves (learning-loop-closure-SKILL.md)
- When writing a DESIGN_REASONING signal after an architectural decision

---

## Section 1 — Nine Learning Signal Types

| Type | What it captures | Stored to | Used by |
|------|-----------------|-----------|---------|
| `OUTCOME` | Did this run pass? Score? Which RAG patterns were retrieved? | learning-outcomes | quality-improvement-cycle-SKILL.md |
| `DPO_TRIPLE` | Rejected vs chosen output with full context | training-data | IncrementalTrainer (fine-tuning) |
| `CALIBRATION` | Archetype + detail level + score | calibration-memory | confidence-lifecycle-SKILL.md |
| `GAP_SIGNAL` | Missing capability with proposed skill path | capability-gaps | graph-extension-protocol-SKILL.md |
| `DESIGN_FLAW` | Topology structure was wrong — classification | design-flaws | ai-pipeline-topology-SKILL.md improvement |
| `PROMPT_PATCH` | Context package needed change — what and why | prompt-evolution | prompt-patch-authoring-SKILL.md history |
| `MODEL_COMPARISON` | Which model tier won per capability class per pipeline stage | model-preference | ai-model-selection-SKILL.md calibration |
| `SHADOW_RUN` | Score gap between Tier 1 model and candidate replacement | shadow-runs | model-tier-readiness tracking |
| `ARBITER_VERDICT` | Each arbiter's verdict per candidate output per run | arbiter-verdicts | challenger-round-design-SKILL.md calibration |

**OUTCOME is required on every topology.** It is the minimum learning signal.
A topology without OUTCOME contributes nothing to the quality improvement loop.

---

## Section 2 — Signal Selection Guide

Choose signals based on what the topology does:

```
Topology validates capability presence
  → GAP_SIGNAL if capability is missing
  → OUTCOME always

Topology generates content via AI (pattern, context package, plan)
  → DPO_TRIPLE if multi-generate is used (chosen vs rejected)
  → PROMPT_PATCH if the context package was changed during the run
  → OUTCOME always

Topology updates quality scores
  → OUTCOME (records what was updated and the result)

Topology synthesizes guidance from accumulated records
  → CALIBRATION (records archetype + recommendation quality)
  → OUTCOME always

Topology analyzes why a run failed
  → DESIGN_FLAW if the topology structure was wrong
  → OUTCOME always

Topology designs a topology for another capability
  → DPO_TRIPLE (bad design vs good design)
  → DESIGN_FLAW if self-questioning found a structural problem
  → OUTCOME always

Topology uses multi-generate node (any generation stage)
  → MODEL_COMPARISON always (records model preference per archetype)
  → SHADOW_RUN always — even as placeholder if alternative tier not active
    (paidScore still recorded; shadowStatus = 'PENDING_ALTERNATIVE')

Topology uses arbiter-panel node (any evaluation stage)
  → ARBITER_VERDICT (one entry per arbiter role per candidate output label)
    resolvedModel stays null until after DPO triple is finalized
```

---

## Section 3 — DPO Validity Gate

A DPO_TRIPLE is stored to the training data index ONLY when all three
conditions are met:

```
□ chosen.Model ≠ rejected.Model   (different model tiers — cross-implementation)
□ ShuffleWasApplied = true        (model attribution stripped before judging)
□ Prompt.System is non-null       (genesis context present)
```

When fewer than 2 model tiers are available:
- Set `Status = "PENDING_COMPARISON"` — not stored to main training index
- Log: "DPO triple pending — single-tier run, cannot produce cross-model chosen/rejected"

When all generators score equally (tie):
- Set `Status = "UNDECIDED"` — stored separately for human labeling
- Never stored to main training index

A triple stored without meeting these conditions teaches the model to
compare outputs from the same source — noise, not signal.

```csharp
public record DpoValidityResult
{
    public bool IsValid { get; init; }
    public string Status { get; init; }  // "READY" | "PENDING_COMPARISON" | "UNDECIDED"
    public List<string> FailedConditions { get; init; }
}
```

---

## Section 4 — Curriculum Tier Table

Every DPO_TRIPLE must declare a curriculum tier. Tiers control training
order — the model learns simple patterns before complex ones:

| Archetype | Tier | Rationale |
|-----------|------|-----------|
| ROUTING | 1 | Simplest — idempotency, rate limiting, store-before-enqueue |
| DATA_PIPELINE | 2 | Requires Tier 1 fabric foundation |
| VALIDATION | 2 | Requires Tier 1 validate.handler patterns |
| TRANSACTION | 3 | Complex state — compensation, saga ordering |
| ORCHESTRATION | 4 | Multi-step, event contracts, gate events |
| SCHEDULED | 5 | Most complex — SLA windows, virtual clock, state machine |

Assign the tier from this table at store time. Never store a DPO_TRIPLE
with CurriculumTier = null — the triple cannot be sequenced by the
training pipeline.

---

## Section 5 — LEARNING-001 Mandatory Check

Every topology contract must declare this quality gate:

```csharp
public static class LearningQualityGates
{
    // LEARNING-001: Topology must have at least one learning signal
    public const string HasLearningSignal =
        "feedback.handler has at least one LearningSignal of type OUTCOME";

    // LEARNING-003: DPO triple must have complete fields if present
    public const string DpoTripleComplete =
        "DPO_TRIPLE signal includes: Chosen, Rejected, TeachingPoint, CurriculumTier";

    // LEARNING-004: Cross-model DPO provenance
    public const string DpoCrossModel =
        "DPO_TRIPLE chosen.Model != rejected.Model";
}
```

The validate.handler node enforces LEARNING-001 at topology execution
time. A topology without OUTCOME signal scores zero on the learning
quality gate regardless of other scores.

---

## Section 6 — DESIGN_REASONING Signal

Captured when an architectural decision is made during a planning session.
Stored as a RAG pattern so future sessions can retrieve the reasoning.

```csharp
public record DesignReasoningSignal
{
    public string SignalType { get; init; }  // "DESIGN_REASONING"
    public string PatternType { get; init; } // "ARCHITECTURE_DECISION"
    public string FlowId { get; init; }
    public string DecisionType { get; init; }
    // CAPABILITY_CLASSIFICATION | WAVE_ASSIGNMENT | IRON_RULE_DERIVATION |
    // FABRIC_INTERFACE_INTRO | DEPENDENCY_DIRECTION | ARCHETYPE_SELECTION

    public string Context { get; init; }               // the planning situation
    public string ProposedRepresentation { get; init; } // what was initially proposed
    public List<DesignChallenge> Challenges { get; init; }
    public string FinalRepresentation { get; init; }    // what was decided
    public string TeachingPoint { get; init; }          // one sentence: generalized lesson
    public string PrincipleApplied { get; init; }       // CF-N rule or CLAUDE.md Rule N
    public double QualityScore { get; init; }
}
```

When to capture: at the end of every ARCHITECT session before closing,
when a decision-reopening-SKILL.md challenge is resolved, when a
CONTEXT_INSUFFICIENT signal from pipeline-position-check-SKILL.md
is resolved with a human judgment.

---

## Anti-Patterns

1. **"Feedback node with empty learning_signals list"** — the topology runs
   but contributes nothing to the improvement loop. Add OUTCOME at minimum.

2. **"DPO_TRIPLE without Prompt.System"** — the triple is unusable for
   IncrementalTrainer without the genesis context. The model trains on
   full context, not just outcomes.

3. **"OUTCOME without PatternsRetrieved"** — DomainModelRegistry cannot
   update edge quality scores without knowing which patterns were retrieved.
   Store an empty array rather than omitting the field.

4. **"DPO_TRIPLE with CurriculumTier = null"** — the training pipeline
   cannot sequence curriculum training. Assign from the tier table at
   store time.

5. **"Emitting events before storing documents"** — DNA Rule 1 requires
   OperationResult return; DNA Rule equivalent here: store action must
   precede emit action in the feedback node. Store first, emit second.

---

## Integration

- ai-pipeline-topology-SKILL.md feedback node format requires this skill for signal selection
- planning-dpo-authoring-SKILL.md Section 2 (GENERATED/REINFORCEMENT) and this skill's DPO validity gate together define what a valid DPO triple requires
- IncrementalTrainer consumes DPO_TRIPLE and OUTCOME signals
- DomainModelRegistry uses GAP_SIGNAL to trigger graph-extension-protocol-SKILL.md
- decision-capture-SKILL.md produces DESIGN_REASONING signals for architectural decisions
- confidence-lifecycle-SKILL.md uses CALIBRATION signals for archetype-tier confidence seeds
