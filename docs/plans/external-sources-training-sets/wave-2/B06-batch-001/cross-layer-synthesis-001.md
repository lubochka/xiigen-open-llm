# Cross-Layer Synthesis (001)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 001 traces the full layer-by-layer execution for a pure DOCUMENT request:
"Define checkout flow business requirements."
This is the canonical DOCUMENT-alone scenario with documentation TransitionType.

---

## Synthesis Trace — DOCUMENT alone

```
source_id: xiigen-b06-synthesis-001
pair_kind: cross_layer_synthesis
scenario: "DOCUMENT alone — pure business requirements request"
```

### Input
UserRequest: "Define checkout flow business requirements."
TransitionId: "transition-B06-synthesis-001"

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: "Define checkout flow business requirements."

Scan results:
  "define" → DOCUMENT (Rule 6: define fires DOCUMENT)
  "business" → DOCUMENT (Rule 7: business fires DOCUMENT)
  "requirements" → DOCUMENT (requirements in DOCUMENT lexicon)
  "checkout" → not in any predicate lexicon
  "flow" → not in any predicate lexicon

Output:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["define", "business", "requirements"]
  SignalCount: 3
  Confidence: 0.92
  KnowledgeEdge: false

Reference: layer-0a-compiler-pairs-002.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT"]; CompiledTerms: ["define", "business", "requirements"]

Routing logic:
  DOCUMENT → default routing → sub-phase 2.1
  No "stakeholder" in compiled terms → 2.4 NOT triggered
  DecisionCount: 1

Output:
  RoutingDecisions:
    - DocumentId: "transition-B06-synthesis-001"
      PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      NearestWrongSubPhaseScore: 0.31

Reference: layer-0b-router-pairs-002.md

---

### Layer 0-C — Knowledge Edge

Layer: 0-C
Unit: KnowledgeEdgeDetectorUnit
Input: same request

Verdict: FLOW_EXECUTING
Rationale: High-signal development terms present (define, business, requirements → DOCUMENT). Confidence 0.92 exceeds 0.70 threshold. Knowledge edge NOT triggered.

Reference: layer-0c-knowledge-edge-pairs-002.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory
Input: SubPhaseId: "2.1"; TrustState context

Output:
  SelectedFlowId: "requirements-authoring-flow-2.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  SelectionOutcome: "TRUSTED_FLOW_SELECTED"
  Warnings: []

Reference: layer-0d-flow-factory-pairs-002.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit
Input: RoutingDecisions + SelectedFlowId

Output:
  TransitionId: "transition-B06-synthesis-001"
  TransitionType: "documentation"
  DomainId: "software-development"
  SubPhaseId: "2.1"
  SelectedFlowId: "requirements-authoring-flow-2.1"
  TransitionType_reason: "DOCUMENT only; no IMPLEMENT or UI present"
  MetaData:
    ParallelFlows: ["2.1"]

Reference: layer-1-transition-orchestrator-pairs-002.md

---

### Layer 2 — Nine-Node Flow (2.1)

Layer: 2
Flow: requirements-authoring-flow-2.1
SubPhase: 2.1 — Business Requirements and Use Cases

N01 DomainSplitter:
  DomainPacket: DOCUMENT → Focus: "Checkout flow business requirements"

N02 DomainTranslator:
  TechnicalStatement: "Define requirements for checkout flow: core use cases (cart, payment, confirmation), business rules for payment retry and session timeout, acceptance criteria."

N03 SchemePlanner:
  WorkBranches: [core-use-cases, business-rules, acceptance-criteria, success-gates]

N09 FormatterUnit:
  OverallVerdict: "PASS"
  FinalHumanResult: "Checkout flow business requirements: 3 core use cases, 5 business rules, 10 acceptance criteria."
  Warnings: []

Reference: layer-2-nine-node-2.1-pairs-002.md

---

### Synthesis Summary

Predicate: DOCUMENT (3 signals)
Routing decisions: 1 (→ 2.1)
TransitionType: documentation
DomainId: software-development
Flows: 1 (RequirementsAuthoringFlow21)
N09 verdict: PASS

Key teaching:
  "define" + "business" + "requirements" → pure DOCUMENT, no IMPLEMENT
  One routing decision, one flow, documentation TransitionType
  Rule 6 + Rule 7 both fire from a 5-word request
