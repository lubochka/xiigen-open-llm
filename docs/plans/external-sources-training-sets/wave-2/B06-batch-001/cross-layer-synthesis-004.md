# Cross-Layer Synthesis (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 004 traces the full layer-by-layer execution for all four predicates:
"Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow."
This is the full FullRequirementsCycle scenario.

---

## Synthesis Trace — Four predicates: DOCUMENT + ARCHITECTURE + IMPLEMENT + UI

```
source_id: xiigen-b06-synthesis-004
pair_kind: cross_layer_synthesis
scenario: "FullRequirementsCycle — four predicates, four flows"
```

### Input
UserRequest: "Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow for the complete checkout redesign."
TransitionId: "transition-B06-synthesis-004"

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: the full request above

Scan results:
  "design" → ARCHITECTURE
  "architecture" → ARCHITECTURE
  "write" → IMPLEMENT
  "requirements" → DOCUMENT
  "spec" → DOCUMENT
  "implement" → IMPLEMENT
  "document" → DOCUMENT
  "UI" → UI
  "flow" → not in any predicate lexicon (UI already fired from "UI")
  "checkout" → not in any predicate lexicon
  "redesign" → not in any predicate lexicon

DOCUMENT signals: ["requirements", "spec", "document"]
ARCHITECTURE signals: ["design", "architecture"]
IMPLEMENT signals: ["write", "implement"]
UI signals: ["UI"]

Output:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT", "UI"]
  Confidence: 0.92
  KnowledgeEdge: false

v6-01 risk: DOCUMENT missed in the presence of three other strong predicates. Must not happen.

Reference: layer-0a-compiler-pairs-009.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: Four predicates

Routing logic:
  DOCUMENT → 2.1
  ARCHITECTURE → 3.1
  IMPLEMENT → 4.1
  UI → 1.1
  No "stakeholder" → 2.4 NOT triggered
  DecisionCount: 4

Output:
  RoutingDecisions:
    - SubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - SubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
    - SubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]
    - SubPhaseId: "1.1"; EvidencePredicates: ["UI"]

CanonicalOrdering: 2.1 → 3.1 → 4.1 → 1.1 (when 2.4 absent)

Reference: layer-0b-router-pairs-009.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Output:
  Flow_2_1: RequirementsAuthoringFlow21 (trusted_learned)
  Flow_3_1: ArchitectureFlow31 (trusted_learned)
  Flow_4_1: ImplementationFlow41 (trusted_learned)
  Flow_1_1: ScreenInventoryFlow11 (trusted_learned)

Reference: layer-0d-flow-factory-pairs-009.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

TransitionType determination:
  IMPLEMENT present → feature_addition (unconditional)
  UI present → feature_addition (unconditional)
  Both present → feature_addition

Output (all four transitions):
  Transition_2_1:
    TransitionType: "feature_addition"; DomainId: "software-development"; SubPhaseId: "2.1"
  Transition_3_1:
    TransitionType: "feature_addition"; DomainId: "software-development"; SubPhaseId: "3.1"
  Transition_4_1:
    TransitionType: "feature_addition"; DomainId: "software-development"; SubPhaseId: "4.1"
  Transition_1_1:
    TransitionType: "feature_addition"; DomainId: "software-development"; SubPhaseId: "1.1"

TransitionType propagation: feature_addition applied to ALL four transitions because IMPLEMENT is present.

Reference: layer-1-transition-orchestrator-pairs-006.md

---

### Layer 2 — Four Concurrent Nine-Node Flows

Flow_2_1 (RequirementsAuthoringFlow21):
  Produces: 5 core checkout use cases (UC-001 through UC-005), 15+ acceptance criteria, 10 business rules
  N09 Verdict: PASS
  Reference: layer-2-nine-node-2.1-pairs-012.md

Flow_3_1 (ArchitectureFlow31):
  Produces: Checkout service architecture design, component diagram, ADR (Architecture Decision Record) for payment integration
  N09 Verdict: PASS

Flow_4_1 (ImplementationFlow41):
  Produces: Checkout service implementation (code artifacts, service layer, payment processing module)
  N09 Verdict: PASS

Flow_1_1 (ScreenInventoryFlow11):
  Produces: Checkout UI screen inventory (cart → address → payment → review → confirmation)
  N09 Verdict: PASS

All four flows run concurrently. Each flow's N08 Arbiter runs independently.

---

### Synthesis Summary

Predicates: DOCUMENT + ARCHITECTURE + IMPLEMENT + UI (all four)
Routing decisions: 4 (→ 2.1 + 3.1 + 4.1 + 1.1)
TransitionType: feature_addition (IMPLEMENT + UI both present)
DomainId: software-development (Rule 17)
Flows: 4 concurrent flows
N09 verdicts: PASS × 4
Canonical ordering: 2.1 → 3.1 → 4.1 → 1.1

Key teaching:
  All four predicates fire independently from independent lexicon signals
  Four routing decisions → four concurrent flows
  feature_addition propagates to ALL transitions when IMPLEMENT or UI present
  2.1 still runs even when dominated by strong construction signals
  DomainId: software-development on all four transitions (Rule 17)
  This is the maximum predicate scenario for B06

---

### Cross-Synthesis Comparison Table

```
source_id: xiigen-b06-synthesis-comparison
pair_kind: cross_layer_synthesis_summary
```

Scenario                 | Predicates                    | Routing Decisions | TransitionType   | Flows
-------------------------|-------------------------------|-------------------|------------------|-------
DOCUMENT alone           | DOCUMENT                      | 1 (→ 2.1)         | documentation    | 1
DOCUMENT + IMPLEMENT     | DOCUMENT, IMPLEMENT           | 2 (→ 2.1, 4.1)    | feature_addition | 2
DOCUMENT → 2.1 + 2.4     | DOCUMENT                      | 2 (→ 2.1, 2.4)    | documentation    | 2
Four predicates          | DOCUMENT, ARCH, IMPL, UI      | 4 (→ 2.1, 3.1, 4.1, 1.1) | feature_addition | 4

Key observation: Routing decision count is not equal to predicate count. It equals the number of active sub-phases. DOCUMENT can produce 1 or 2 routing decisions depending on stakeholder signal. All four predicates produce exactly 4 routing decisions (no stakeholder in this example).
