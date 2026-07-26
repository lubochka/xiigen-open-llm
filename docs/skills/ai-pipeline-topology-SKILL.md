# AI Pipeline Topology

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

An AI pipeline assembled without a defined topology contract degrades
into an ad-hoc sequence of model calls that cannot be tested, cannot
produce DPO (Direct Preference Optimization) training data reliably, and
cannot be routed by the escalation orchestrator. This skill defines the
nine node handler types, the six wiring patterns, the mandatory elements
every topology must declare, and the DPO-required pattern (Pattern F)
that the ContinuousLearningPipeline uses when training data generation
is a first-class goal.

---

## When to Invoke

- When designing the execution structure for a new capability class
- When ContinuousLearningPipeline requests a topology for a new phase
- When self-questioning-SKILL.md T1-T4 questions require a topology to be specified
- Before any AI pipeline phase that needs to produce DPO training data

---

## Section 1 — Nine Node Handler Types

Each node in a topology performs one of these nine functions. Nodes that
do not map to one of these nine types are not valid topology nodes.

```
rag-retrieve:    Load context from DomainModelRegistry, COMPONENT-INTERFACES.md,
                 HybridRetriever, or ISessionStorage.
                 .NET: call HybridRetriever.Search() or DomainModelRegistry.GetEdges()

decompose:       Break a complex capability class into sub-tasks.
                 .NET: produce a List<SubTask> with explicit task boundaries

ai-generate:     Single model call producing one candidate output.
                 .NET: call the AI model API once; capture the raw output

multi-generate:  Two or more ai-generate calls producing independent candidates.
                 .NET: run generator A and generator B with the same prompt;
                 sources must be different (no self-comparison for DPO)

arbiter-panel:   One or more arbiter calls evaluating candidates from multi-generate.
                 .NET: call each arbiter independently; combine verdicts into
                 ArbiterPanelResult. Must include principles-arbiter (isolated=true).

validate:        Structural validation — verifies mandatory elements, format, contracts.
                 .NET: check iron rule compliance, self-questioning protocol (SELF-Q-001/002/003),
                 Gate B conditions

score:           Numeric scoring of a candidate against the current context package.
                 .NET: produce CompositeScore with five sub-scores from score-interpretation-SKILL.md

route:           Conditional branch based on score bracket or verdict.
                 .NET: switch on score bracket (PASS/DETAIL_GAP/PATTERN_MISSING/STRUCTURAL)
                 or arbiter verdict (PASS/BLOCK/WARN)

feedback:        Captures learning signal and stores it for IncrementalTrainer.
                 .NET: produce DpoTriple and write to training data store;
                 must be the FINAL node in every topology
```

---

## Section 2 — Mandatory Topology Elements

Every topology contract must declare all of the following. A topology
missing any element fails the validate.handler check:

```csharp
public record TopologyContract
{
    public string TopologyId { get; init; }      // unique identifier
    public string CapabilityClass { get; init; } // capability class this topology serves
    public string Archetype { get; init; }       // SCORING / ROUTING / RETRIEVAL / LEARNING / etc.
    public string Trigger { get; init; }         // what event starts this topology
    public List<TopologyNode> Nodes { get; init; }    // minimum 2 nodes
    public List<TopologyEdge> Edges { get; init; }    // explicit connections between nodes
    public List<string> IronRules { get; init; }      // minimum 2 CF-N identifiers
    public List<string> QualityGates { get; init; }   // minimum 2 (Gate A = entry, Gate B = exit)
    // feedback.handler must be the last node in Nodes
}
```

Minimum node count: 2. Minimum iron rules: 2. Minimum quality gates: 2.
The feedback.handler node must always appear last in the node list —
it is the learning capture that makes the pipeline self-improving.

---

## Section 3 — Six Wiring Patterns

### Pattern A — Linear Validation

```
rag-retrieve → ai-generate → validate → score → feedback
```

Use when: single-pass generation with structural validation. No branching,
no multi-generate. Produces one DPO triple if score ≥ PASS.

### Pattern B — Branch on Validation

```
rag-retrieve → ai-generate → validate → [PASS] score → feedback
                                       → [FAIL] route → feedback (rejected)
```

Use when: validation failure should produce a separate learning signal
from validation success. Both branches end with feedback.

### Pattern C — Enrich and Store

```
rag-retrieve → decompose → ai-generate → validate → score → rag-retrieve(store) → feedback
```

Use when: the output needs to be stored back into DomainModelRegistry or
ISessionStorage after passing validation. The second rag-retrieve node
performs the write.

### Pattern D — Score-Gated Promotion

```
rag-retrieve → ai-generate → score → route → [PASS] feedback(promoted)
                                            → [DETAIL_GAP] prompt-patch → ai-generate (retry)
                                            → [STRUCTURAL] feedback(escalate)
```

Use when: score-based routing determines the continuation path. Implements
the PATCH / REWRITE / ESCALATE decision from score-interpretation-SKILL.md.

### Pattern E — Design with Self-Questioning

```
rag-retrieve → ai-generate → validate(SELF-Q-001/002/003) → score → feedback
```

Use when: the artifact must pass the self-questioning protocol before scoring.
The validate node enforces SELF-Q-001, SELF-Q-002, and SELF-Q-003 from
self-questioning-SKILL.md.

### Pattern F — Multi-Arbiter DPO Pipeline

```
rag-retrieve → multi-generate → arbiter-panel → validate → score → feedback(DPO_TRIPLE)
```

Use when: training data generation is the primary goal. This is the pattern
required whenever ContinuousLearningPipeline is instructed to produce DPO
triples. Requirements:
- multi-generate must call at least 2 different generators (different sources)
- arbiter-panel must include principles-arbiter with isolated=true
- feedback node must produce a DpoTriple with TrainingCategory=GENERATED
- Chosen.Source ≠ Rejected.Source (cross-implementation requirement from
  planning-dpo-authoring-SKILL.md Section 2)
```

Pattern F is mandatory whenever the phase plan requires DPO training data.
Using Pattern A or B when Pattern F is required produces no training signal.

---

## Section 4 — Topology Verification Checklist

Before submitting any topology for execution:

```
□ Does the topology have a unique TopologyId?
□ Does the topology declare a CapabilityClass?
□ Are there ≥ 2 nodes?
□ Is feedback.handler the last node?
□ Are there ≥ 2 iron rules (CF-N identifiers) declared?
□ Are there ≥ 2 quality gates declared?
□ If multi-generate is present: are both generators using different sources?
□ If DPO training data is required: is Pattern F used?
□ Has self-questioning-SKILL.md been applied? (T1-T4 questions)
```

---

## Anti-Patterns

1. **"No feedback node"** — a topology without a feedback node produces
   no learning signal. The pipeline runs but does not improve. feedback
   must be the final node, always.

2. **"Using if/else branching inside node descriptions"** — nodes have
   a single function. Conditional logic belongs in a route node, not
   inside an ai-generate or validate node description. A node that does
   two things depending on a condition is two nodes with a route between them.

3. **"Choosing Pattern A when DPO training data is required"** — Pattern A
   produces at most one DPO triple, from a single generator. DPO training
   requires cross-implementation comparison (planning-dpo-authoring-SKILL.md
   GENERATED requirement). Use Pattern F.

4. **"Arbiter-panel with only one arbiter"** — a single arbiter evaluating
   its own generator's output is self-evaluation (confirmation bias). A panel
   requires at minimum: one domain arbiter + principles-arbiter (isolated=true).

5. **"Declaring iron rules but not wiring them to validate or arbiter nodes"** —
   iron rules declared in the topology contract but not enforced by any node
   are decorative. Each CF-N rule must have a corresponding check in a validate
   or arbiter-panel node.

---

## Integration

- ContinuousLearningPipeline executes topologies defined by this skill
- self-questioning-SKILL.md T1-T4 questions verify topology mandatory elements
- principles-arbiter-SKILL.md is the arbiter-panel node required in Pattern F
- score-interpretation-SKILL.md provides the five sub-scores that the score node produces
- planning-dpo-authoring-SKILL.md Section 2 (GENERATED vs REINFORCEMENT) applies to
  the feedback node in Pattern F
- IncrementalTrainer consumes the DpoTriple produced by the feedback node
