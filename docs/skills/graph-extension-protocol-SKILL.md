# Graph Extension Protocol

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

DomainModelRegistry starts with a designed graph of capability edges. As the system
encounters new capability classes and new relationships, the graph needs to grow.
But uncontrolled graph mutation — adding edges from a single observation, modifying
immutable architectural edges, proposing schema changes without cross-model validation
— corrupts the registry. This skill defines the three legitimate mutation types,
the three-phase approval process (automated → cross-model → Luba), and the
REJECTION_REASON edge pattern that prevents the same proposal from being made twice.

---

## When to Invoke

- When DomainModelRegistry.GetEdges() returns no matching edges for a new capability class
- When all matching edges have confidence below the risk-tier minimum threshold
- When ContinuousLearningPipeline emits a CONTEXT_INSUFFICIENT signal for an unknown relationship
- When designing the update cycle for DomainModelRegistry after a new subproject completes

---

## Section 1 — Three Escalation Triggers

The graph extension protocol activates when:

```
Trigger 1: Registry returns no matching edges for a query
  (Novel capability class — no prior art in the graph)

Trigger 2: All matching edges have confidence < LOW_RISK threshold (0.75)
  (Existing edges insufficient to route above the threshold)

Trigger 3: IncrementalTrainer or ContinuousLearningPipeline emits CONTEXT_INSUFFICIENT
  (The training or learning pipeline hit a gap it cannot self-resolve)
```

---

## Section 2 — Three Legitimate Mutation Types

Only these three mutations are accepted. Proposals outside these types are auto-rejected.

### ADD_EDGE — New relationship between existing entities

```csharp
public record GraphMutationProposal
{
    public string MutationType { get; init; }  // "ADD_EDGE"
    public string FromEntity { get; init; }    // existing capability class or concept
    public string ToEntity { get; init; }      // existing capability class or concept
    public string Relationship { get; init; }  // relationship type (GOVERNS/PRECEDES/BLOCKS/etc.)
    public double InitialConfidence { get; init; }  // seed from confidence-seeding-SKILL.md
    public List<string> TriggeringSessionIds { get; init; }  // sessions where gap was observed
    public int ObservationCount { get; init; }  // must be ≥ 3 for ADD_EDGE
}
```

**Evidence bar:** ≥ 3 sessions where the missing edge was observed as a gap.

### PROMOTE_EDGE_TYPE — Raise edge from OPTIONAL to PROMOTED to REQUIRED

```csharp
public record PromoteEdgeProposal
{
    public string MutationType { get; init; }  // "PROMOTE_EDGE_TYPE"
    public string EdgeId { get; init; }
    public string CurrentType { get; init; }   // "OPTIONAL"
    public string TargetType { get; init; }    // "PROMOTED" or "REQUIRED"
    public int ObservationCount { get; init; } // must cross REQUIRED threshold
    public double AverageScore { get; init; }  // average score across observations
}
```

**Evidence bar:** ObservationCount crosses the REQUIRED threshold from confidence-seeding-SKILL.md
(typically observationCount ≥ risk-tier threshold observations).

### PROPOSE_NEW_NODE — New entity type not yet in the graph

```csharp
public record NewNodeProposal
{
    public string MutationType { get; init; }  // "PROPOSE_NEW_NODE"
    public string ProposedEntityType { get; init; }
    public string Definition { get; init; }    // what this entity type represents
    public int ObservationCount { get; init; } // must be ≥ 10
    public List<string> ProposedEdges { get; init; }  // edges connecting to existing nodes
    public string WhyNotAnEdge { get; init; }  // why this cannot be an ADD_EDGE instead
}
```

**Evidence bar:** ≥ 10 sessions showing a consistent pattern for this entity type.
**This bar is the highest** because new node types change the schema and affect all consumers.

**Note:** PROPOSE_NEW_NODE requires justification for why the concept cannot be represented
as an ADD_EDGE on existing nodes. New nodes are expensive; prefer ADD_EDGE first.

---

## Section 3 — Two Prohibited Actions

1. **Modify an `Immutable = true` edge** — immutable edges are architectural invariants
   encoding CLAUDE.md rules or locked decisions. The extension protocol cannot override
   them regardless of evidence. Any proposal targeting an immutable edge is auto-rejected
   in Phase 1 screening.

2. **Add an edge that contradicts a locked decision (D-NN)** — graph mutations must be
   consistent with DECISIONS-LOCKED.md. A proposed edge that contradicts D-07 (for example)
   requires decision-reopening-SKILL.md before the edge can be proposed.

---

## Section 4 — Three-Phase Approval

### Phase 1 — Automated Screening

```
□ Does the proposal target an Immutable=true edge? → REJECT immediately
□ Does the edge/node name already exist? → MERGE_SUGGESTION (is this a duplicate?)
□ ObservationCount < minimum for this mutation type? → INSUFFICIENT_EVIDENCE
  Requeue after reaching the minimum observation count

If Phase 1 passes → Phase 2
```

### Phase 2 — Cross-Model Simulation

```
Constraint: the model that simulates the proposal must be DIFFERENT from the model
that produced the proposal. (Same model self-simulating = confirmation bias.)

Simulator question: "If this edge had existed for the last [N] training sessions,
would IncrementalTrainer produce different (better) confidence updates?"

Output: SIMULATION_REPORT
  - Before: average convergence with current graph
  - After: projected convergence if the edge existed
  - CONTESTED: if simulator disagrees with proposer (show both views to Luba)

If only one model is available in the session → skip Phase 2, route directly to Luba
with a note that Phase 2 simulation was unavailable.
```

### Phase 3 — Luba Review

```
Present to Luba:
  1. Mutation in plain language (not graph terminology)
  2. Evidence: which sessions triggered this proposal, ObservationCount
  3. Simulation report (or note that simulation was unavailable)
  4. Scope level: CONVENTION / EXTENSION / NEW_CAPABILITY

Luba's options:
  APPROVED → mutation applied immediately to DomainModelRegistry
  REJECTED → REJECTION_REASON edge stored (see Section 5)
  MODIFY   → Luba's guidance becomes the approval criterion for future similar proposals
```

---

## Section 5 — REJECTION_REASON Edges

When Luba rejects a mutation, a REJECTION_REASON edge is stored in DomainModelRegistry:

```csharp
new GraphEdge
{
    FromEntity = "PROPOSAL:add-WeightedScoring-AVOIDS-IVectorIndex-direct",
    Relationship = "REJECTION_REASON",
    ToEntity = "REASON:interface-already-covered-by-fabric-first-rule",
    Confidence = 1.0,
    Immutable = false,
    Reasoning = "Luba rejected: fabric-first (Rule 2) already prevents direct IVectorIndex injection. The edge would duplicate an existing rule."
}
```

**Before proposing any mutation:** query REJECTION_REASON edges for similar proposals.
If a similar mutation was previously rejected:
- The new proposal must either cite new evidence that addresses the rejection reason
- Or not be proposed at all (if the rejection reason still applies)

Rejected proposals become training data: "mutations Luba doesn't want."
This is the mechanism by which the Top Manager's judgment improves over sessions.

---

## Anti-Patterns

1. **"Proposing a mutation based on 1 observation"** — 1 observation is not a pattern.
   ADD_EDGE requires ≥ 3. PROPOSE_NEW_NODE requires ≥ 10. Queue and wait.

2. **"Phase 2 simulation by the same model that proposed the mutation"** —
   self-evaluation is confirmation bias. Simulation must use a different model.

3. **"Proposing a new node when an edge would work"** — new nodes change the schema.
   Always check: can this be an ADD_EDGE on existing nodes? Only escalate to PROPOSE_NEW_NODE
   if the concept genuinely cannot be represented as a relationship between existing entities.

4. **"Proposing a mutation without checking REJECTION_REASON history"** — a proposal
   rejected in session 7 will be rejected again in session 21 if the rejection reason
   still applies. Check rejection history before proposing.

5. **"Applying an APPROVED mutation to a different subproject without re-approval"** —
   a graph mutation approved for one capability class may not be appropriate for another.
   Each use requires its own approval chain.

---

## Integration

- DomainModelRegistry stores all edges, immutable flags, and REJECTION_REASON edges
- confidence-seeding-SKILL.md provides the InitialConfidence for new ADD_EDGE proposals
- IncrementalTrainer triggers the extension protocol when it encounters CONTEXT_INSUFFICIENT
- decision-reopening-SKILL.md handles proposals that contradict locked decisions (D-NN)
- graph-entity-schema-SKILL.md defines the NODE/EDGE/PROPERTY/IMMUTABLE classification used here
