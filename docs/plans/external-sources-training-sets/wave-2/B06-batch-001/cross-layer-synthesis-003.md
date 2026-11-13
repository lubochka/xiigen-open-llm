# Cross-Layer Synthesis (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 003 traces the full layer-by-layer execution for DOCUMENT → 2.1 + 2.4 dual sub-phase routing:
"Define the checkout use cases and document the stakeholder requirements for external payment partners."
This is the canonical DOCUMENT + stakeholder signal scenario.

---

## Synthesis Trace — DOCUMENT → 2.1 + 2.4 dual routing

```
source_id: xiigen-b06-synthesis-003
pair_kind: cross_layer_synthesis
scenario: "DOCUMENT → 2.1 + 2.4 — stakeholder signal triggers dual routing"
```

### Input
UserRequest: "Define the checkout use cases and document the stakeholder requirements for external payment partners and marketplace vendors."
TransitionId: "transition-B06-synthesis-003"

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: "Define the checkout use cases and document the stakeholder requirements for external payment partners and marketplace vendors."

Scan results:
  "define" → DOCUMENT (Rule 6)
  "use cases" (use-case) → DOCUMENT
  "document" → DOCUMENT
  "stakeholder" → DOCUMENT (Rule 8; also triggers 2.4 routing)
  "requirements" → DOCUMENT
  "payment" → not in any predicate lexicon (payment partner is an external entity reference, not a predicate term)
  "marketplace" → not in any predicate lexicon

DOCUMENT signals: ["define", "use-case", "document", "stakeholder", "requirements"]
IMPLEMENT signals: none

Output:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["define", "use-case", "document", "stakeholder", "requirements"]
  SignalCount: 5
  Confidence: 0.92
  KnowledgeEdge: false
  StakeholderSignalDetected: true

Note: Five DOCUMENT signals → one predicate; confidence ceiling 0.92.

Reference: layer-0a-compiler-pairs-005.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT"]; StakeholderSignalDetected: true

Routing logic:
  DOCUMENT → 2.1 (always)
  "stakeholder" signal → also 2.4 (StakeholderRequirementsFlow24)
  DecisionCount: 2

Output:
  RoutingDecisions:
    - DocumentId: "transition-B06-synthesis-003"
      PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]
      SubPhaseSignal: "use-case, define, requirements"
      KnowledgeEdge: false

    - DocumentId: "transition-B06-synthesis-003"
      PredictedSubPhaseId: "2.4"
      EvidencePredicates: ["DOCUMENT"]
      SubPhaseSignal: "stakeholder"
      KnowledgeEdge: false

v6-03 risk: emitting only 2.1 and dropping 2.4. Must emit both.

Reference: layer-0b-router-pairs-005.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Output:
  Flow_2_1:
    SelectedFlowId: "requirements-authoring-flow-2.1"
    TrustState: "trusted_learned"
  Flow_2_4:
    SelectedFlowId: "stakeholder-requirements-flow-2.4"
    TrustState: "trusted_learned"

Reference: layer-0d-flow-factory-pairs-002.md (2.1 side) + layer-0d-flow-factory-pairs-009.md (2.4 side)

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

Output:
  Transition_2_1:
    TransitionId: "transition-B06-synthesis-003-2.1"
    TransitionType: "documentation"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "requirements-authoring-flow-2.1"

  Transition_2_4:
    TransitionId: "transition-B06-synthesis-003-2.4"
    TransitionType: "documentation"
    DomainId: "software-development"
    SubPhaseId: "2.4"
    SelectedFlowId: "stakeholder-requirements-flow-2.4"

TransitionType_reason: "DOCUMENT only (no IMPLEMENT, no UI) → documentation"
ParallelFlows: ["2.1", "2.4"]
CanonicalOrdering: "2.1 runs before 2.4 when both are present"

Reference: layer-1-transition-orchestrator-pairs-007.md

---

### Layer 2 — Nine-Node Flow (2.1 side)

Flow: requirements-authoring-flow-2.1

N01: DomainPacket DOCUMENT → "Checkout use cases for external partners (payment, vendor, shipping, tax)"
N02: TechnicalStatement: "UC-EXT-001 through UC-EXT-004: payment partner auth, vendor fulfillment, shipping tracking, tax service"
N03: WorkBranches: [payment-partner-uc, vendor-uc, shipping-uc, tax-uc, business-rules]
N09: PASS — 4 partner use cases, 3 business rules

Reference: layer-2-nine-node-2.1-pairs-009.md

### Layer 2 — Nine-Node Flow (2.4 side)

Flow: stakeholder-requirements-flow-2.4

N01: DomainPacket DOCUMENT → "Formal stakeholder requirements for external payment partners and marketplace vendors"
N02: TechnicalStatement: "Partner SLAs (Service-Level Agreements), integration contracts, external-facing requirements document"
N09: PASS — formal stakeholder requirements document

---

### Synthesis Summary

Predicate: DOCUMENT (5 signals, including "stakeholder")
Routing decisions: 2 (→ 2.1 AND → 2.4)
TransitionType: documentation (no IMPLEMENT, no UI)
DomainId: software-development
Flows: 2 (RequirementsAuthoringFlow21 + StakeholderRequirementsFlow24), running concurrently
Canonical ordering: 2.1 runs before 2.4

Key teaching:
  One predicate (DOCUMENT) can trigger two routing decisions
  "stakeholder" is the trigger for 2.4 — no other term triggers it
  Both flows receive the same UserRequest
  documentation TransitionType even with two flows
  v6-03 risk: must always emit both 2.1 and 2.4 when "stakeholder" compiled
