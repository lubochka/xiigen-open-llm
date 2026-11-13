# Graph-Backed Routing

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

A route.handler with hardcoded score thresholds (>=85 → PASS, >=65 → PATCH,
default → FAIL) never learns which routing decisions were correct. The
threshold is fixed at design time and does not improve as more runs complete.
After 30 capability executions, the routing is no better than it was at run 1.
This skill defines the transformation pattern that replaces hardcoded branches
with graph-backed decisions that accumulate evidence and improve over time,
and the two-step feedback protocol that distinguishes "decision taken" from
"outcome known."

---

## When to Invoke

- When reviewing a topology that has a route.handler with more than 2 branches
- When designing a new topology whose routing threshold should be learnable
- When the same routing threshold appears in more than one topology
- When four-tier-decision-classification-SKILL.md classifies a decision as LEARNABLE

---

## Section 1 — The Transformation Pattern

**Before (static — hardcoded thresholds):**

```csharp
// route.handler with static branches
var routeDecision = score switch
{
    >= 0.85 => "promote",
    >= 0.65 => "prompt-patch",
    _       => "escalate"
};
```

**After (graph-backed):**

```csharp
// Step n3a: query DomainModelRegistry for learned routing decision
var graphQuery = new GraphQuery
{
    FromEntity = $"{archetype}:{scoreBracket}",
    Relationship = "ROUTES_TO"
};
var graphResult = await _domainModelRegistry.QueryEdge(graphQuery);

// Step n3b: route.handler still exists — reads from graph result, not hardcoded branches
// If graph confidence is below threshold, fallback to static thresholds
var routeDecision = graphResult.Confidence >= _config.RoutingThreshold
    ? graphResult.Decision
    : FallbackRoute(score);

// Step n3c: feedback records DECISION only — outcome is unknown at this point
var routingDecision = new RoutingDecisionSignal
{
    DecisionId = Guid.NewGuid().ToString(),
    Decision = routeDecision,
    Score = score,
    CyclesUsed = cycleCount,
    CycleBudget = _config.CycleBudget,
    OutcomeClass = RoutingOutcome.Pending   // resolved at Phase F
};
await _learningBus.Emit(routingDecision);
```

---

## Section 2 — Five Refactoring Rules

```
Rule 1: Every static route.handler with more than 2 branches is a candidate
Rule 2: Every hardcoded threshold referenced by more than 1 topology is a candidate
Rule 3: The graph query runs BEFORE the route.handler, not instead of it
Rule 4: The route.handler STILL EXISTS — it reads from graph result with a fallback
Rule 5: Feedback is TWO STEPS:
         Step 1 (inline): record decision with OutcomeClass = Pending
         Step 2 (Phase F retrospective): resolve outcome and update graph edge
```

---

## Section 3 — Two-Step Feedback

A routing decision at score 0.68 that chooses PROMPT_PATCH does not know if
PROMPT_PATCH was the right action until after the capability run completes.
Recording `OutcomeClass = Success` at decision time is a lie — the outcome
is unknown.

```
Step 1 — At decision time (inline feedback node):
  Record:  what action was taken, at what score, in which cycle
  Set:     OutcomeClass = Pending
  Set:     TrainingDataQuality = OutcomePending

Step 2 — After capability run completes (Phase F retrospective):
  Determine: SuccessWithinBudget | WastedCycle | EscalationRequired
  Update:    DomainModelRegistry edge confidence via UpdateEdge()
  Update:    DPO triple TrainingDataQuality → Validated
  Check:     PromoteEdgeIfThresholdMet() for edges ready to graduate
```

```csharp
// Phase F retrospective — resolves all PENDING routing decisions
public record RoutingOutcomeResolution
{
    public string DecisionId { get; init; }
    public RoutingOutcome OutcomeClass { get; init; }
    // SuccessWithinBudget | WastedCycle | EscalationRequired
    public double UpdatedEdgeConfidence { get; init; }
}

public enum RoutingOutcome
{
    Pending,
    SuccessWithinBudget,
    WastedCycle,
    EscalationRequired
}
```

---

## Section 4 — What NOT to Refactor

Not every route.handler needs graph backing. Apply four-tier-decision-classification-SKILL.md
first. Only refactor decisions classified as LEARNABLE.

```
Binary pass/fail gates (validate.handler result → pass or stop)
  → STATIC — leave as-is

DNA invariant routing (BLOCK → remove from pool)
  → IMMUTABLE — never learnable

Routing to structurally different handler types (generate vs convergence)
  → STATIC structure — leave as-is

route.handler with exactly 2 branches and clear domain rule
  → Usually FREEDOM CONFIG — move threshold to appsettings.json, not graph
```

---

## Section 5 — DomainModelRegistry Edge Format

A graph-backed routing edge stores:

```csharp
public record RoutingEdge
{
    public string FromEntity { get; init; }   // e.g., "SCORING:DETAIL_GAP"
    public string Relationship { get; init; } // "ROUTES_TO"
    public string ToDecision { get; init; }   // e.g., "PROMPT_PATCH"
    public double Confidence { get; init; }   // 0.0–1.0; starts at seed value
    public int EvidenceCount { get; init; }   // increments after each Phase F resolution
    public string ArchetypeClass { get; init; }
}
```

Edge confidence seeds:
- New edge (no data): start at 0.30 (confidence-seeding-SKILL.md default for LEARNABLE)
- After 3 SuccessWithinBudget resolutions: promote confidence toward 0.70
- After 3 consecutive WastedCycle or EscalationRequired: demote confidence

---

## Anti-Patterns

1. **"Remove route.handler entirely, replace with graph query"** — the route.handler
   is the fallback when the graph is unavailable or confidence is below threshold.
   Remove it and graph failure stops generation.

2. **"Record OutcomeClass = SuccessWithinBudget at decision time"** — the outcome
   is unknown at decision time. The Phase F retrospective is what determines outcome.
   An outcome recorded inline is a guess stored as fact.

3. **"Add graph query without a feedback node after the route"** — the graph provides
   decisions based on past evidence. Without the feedback node recording new decisions,
   the graph provides answers but never accumulates new evidence.

4. **"Refactor a binary pass/fail gate"** — binary gates are structural invariants.
   There is no "correct" direction to learn — PASS and FAIL are defined by the DNA rule
   or iron rule being checked. Refactoring these corrupts the gate.

---

## Integration

- four-tier-decision-classification-SKILL.md routes LEARNABLE decisions to this skill
- DomainModelRegistry edge updates follow confidence-seeding-SKILL.md and confidence-lifecycle-SKILL.md
- learning-signal-capture-SKILL.md: the inline feedback node emits OUTCOME with OutcomeClass=Pending; Phase F emits CALIBRATION after resolution
- graph-extension-protocol-SKILL.md governs the creation of new RoutingEdge types
- learning-loop-closure-SKILL.md diagnoses why Phase F resolutions never arrive (broken 4-event loop)
