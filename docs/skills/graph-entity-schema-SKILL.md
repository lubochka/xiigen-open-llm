# Graph Entity Schema Design

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

A knowledge graph designed as a flat key-value store loses its relational
learning capability. A decision modeled as a node when it should be an edge
cannot accumulate evidence. An edge without a confidence field can never improve.
This skill defines how to classify every element in a learnable decision graph
before writing any DomainModelRegistry entry or ContinuousLearningPipeline rule.

---

## When to Invoke

- After four-tier-decision-classification-SKILL.md classifies a decision as LEARNABLE
- Before writing any DomainModelRegistry capability entry
- Before writing any ContinuousLearningPipeline confidence rule
- When designing the graph structure for a new capability class

---

## Section 1 — Entity Type Classification

For each LEARNABLE decision, classify every element:

**NODE:** An entity that exists independently and accumulates observations.
Test: can this be the subject of its own learning?
Examples:
- `CapabilityClass:WeightedScoring` (a class of capabilities)
- `ComponentRole:Consumer` (a role a component plays)
- `ScoreBracket:70-84` (a scoring tier)

**EDGE:** A claim about a relationship between two nodes — learnable, weighted.
Can be added or removed without changing the nodes it connects.
Test: can this be proven right or wrong by observing run outcomes?
Examples:
- `CapabilityClass:WeightedScoring --[REQUIRES]--> ConfigKey:MatchWeight`
- `Capability:Bm25Scorer --[IMPROVES]--> Metric:RetrievalPrecision`

**PROPERTY:** A fact about a node that is not itself a learning target.
Test: does this value change based on evidence? If no → property, not edge.
Examples:
- `Capability:Bm25Scorer.algorithmsClass = "BM25"` (always true by definition)

**IMMUTABLE EDGE:** A boundary condition that cannot be learned away.
Test: is this a definition rather than a hypothesis?
Examples:
- `DnaRule:Rule1 --[FORCES]--> OperationResult<T>` (DNA invariant — not learnable)

---

## Section 2 — Entity Derivation Table

| Decision type | Node types | Edge type |
|---|---|---|
| "which config keys for capability class" | CapabilityClass, ConfigKey | REQUIRES / OPTIONAL |
| "what score means promotion-ready" | ScoreBracket, PromotionAction | ROUTES_TO |
| "does capability X improve metric Y" | Capability, Metric | IMPROVES |
| "which capabilities are in same domain" | Domain, Capability | CONTAINS |
| "which test category for capability class" | CapabilityClass, TestCategory | REQUIRES_TEST |
| "which DNA rule governs capability class" | DnaRule, CapabilityClass | GOVERNS |

---

## Section 3 — Seed Values

Every edge MUST have these fields when created in DomainModelRegistry:

```csharp
new CapabilityEdge
{
    FromEntity = "CapabilityClass:WeightedScoring",
    Relationship = "REQUIRES",
    ToEntity = "ConfigKey:MatchWeight",
    Confidence = 0.5,          // neutral prior — equal evidence for yes/no
    ObservationCount = 0,      // no runtime observations yet
    Immutable = false,         // can be learned away
    Source = "bootstrap",      // "bootstrap" | "discovered" | "manual"
    Reasoning = "Seeded from algorithm-as-service-SKILL.md weighted scoring class"
}
```

---

## Section 4 — Five-Field Edge Schema (minimum)

Every edge in DomainModelRegistry must have these five fields populated:

```csharp
public record CapabilityEdge
{
    public string FromEntity { get; init; }     // e.g. "Capability:Bm25Scorer"
    public string Relationship { get; init; }   // e.g. "IMPROVES"
    public string ToEntity { get; init; }       // e.g. "Metric:RetrievalPrecision"
    public double Confidence { get; init; }     // 0.0–1.0
    public int ObservationCount { get; init; }  // increments per observed outcome
    public bool Immutable { get; init; }        // true = DNA invariant only
    public string Source { get; init; }         // bootstrap / discovered / manual
    public string Reasoning { get; init; }      // why this edge was created
}
```

---

## Section 5 — Promotion Path Design

For edges that can upgrade, define the path at design time:

```
OPTIONAL → ACTIVE       when confidence ≥ 0.70 AND observationCount ≥ 5
ACTIVE → PROVEN         when confidence ≥ 0.85 AND observationCount ≥ 10
PROVEN → GRADUATED      when confidence ≥ risk-tier threshold (per capability class)
                         (see confidence-lifecycle-SKILL.md for risk-tier values)

IMPROVES edges: no promotion — they accumulate delta evidence continuously
GOVERNS edges: immutable — DNA rules do not promote or demote
CONTAINS edges: informational — track membership, not confidence
```

---

## Section 6 — Entity Schema Document Format

Produce this for every LEARNABLE decision before writing any implementation:

```markdown
## Graph Entity Schema — [DecisionName]

Nodes:
- CapabilityClass:WeightedScoring (type: CapabilityClass)
- ConfigKey:MatchWeight (type: ConfigKey)
- ScoreBracket:70-84 (type: ScoreBracket)

Edges:
- CapabilityClass:WeightedScoring --[REQUIRES]--> ConfigKey:MatchWeight
  seed: confidence=0.5, observationCount=0, immutable=false
  promotion: OPTIONAL → ACTIVE at 5 obs, ACTIVE → PROVEN at 10 obs
  reasoning: "Weighted scoring requires configurable weights per algorithm-as-service-SKILL.md"

Immutable edges:
- DnaRule:Rule10 --[GOVERNS]--> CapabilityClass:WeightedScoring
  confidence: 1.0, immutable: true
  reasoning: "DNA Rule 10 (no external ML) governs all algorithm implementations"
```

---

## Anti-Patterns

1. **"Cycle budget = 3 → graph entity"** — a scalar value with no relationships is
   FREEDOM config (IConfiguration key), not a graph entity. Entities need edges to
   be useful.

2. **"All edges immutable to be safe"** — only DNA invariants are immutable. Everything
   else must be learnable. Immutable edges that are not DNA invariants block the learning loop.

3. **"Edge without a relationship type"** — `Capability:Bm25 → Metric:Precision` means
   nothing without the relationship type. IMPROVES? REQUIRES? MEASURES? The relationship IS
   the knowledge.

4. **"Same relationship type for different semantics"** — REQUIRES (mandatory) ≠ OPTIONAL
   (can be promoted to mandatory). Using REQUIRES for both hides which edges are learnable.

---

## Integration

- confidence-lifecycle-SKILL.md governs promotion thresholds referenced in Section 5
- four-tier-decision-classification-SKILL.md identifies which decisions are LEARNABLE (input to this skill)
- DomainModelRegistry is the concrete implementation that stores entities and edges
- learning-loop-closure-SKILL.md defines the 4-event loop that populates observationCount and confidence
- capability-routing-gate-SKILL.md Q3 (should it appear in DomainModelRegistry?) determines when this skill applies
