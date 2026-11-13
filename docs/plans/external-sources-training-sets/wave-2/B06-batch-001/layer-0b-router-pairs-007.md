# Layer 0-B — Router Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 007 covers [DOCUMENT, UI] → 2.1 + 1.1 for checkout form requirements.

---

## B06-0B-007-001 — [DOCUMENT, UI] → 2.1 + 1.1 checkout form requirements

```
source_id: xiigen-b06-0b-007-001
source_title: XIIGen B06 router — DOCUMENT+UI to 2.1+1.1 form requirements
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-007-001]
  DocumentId: "B06-0a-007-001-prompt"
  CompiledText: "Document the checkout form requirements and describe the user interface behavior for each step."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-007-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-007-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.34
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  DOCUMENT ("document", "requirements", "describe", "behavior") → 2.1.
  UI ("form", "interface") → 1.1 (Screen Inventory and UX Requirements).
  DecisionCount: 2.

---

## B06-0B-007-002 — [DOCUMENT, UI] → 2.1 + 1.1 screen requirements

```
source_id: xiigen-b06-0b-007-002
source_title: XIIGen B06 router — DOCUMENT+UI to 2.1+1.1 screen requirements
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-007-002]
  DocumentId: "B06-0a-007-002-prompt"
  CompiledText: "Specify the requirements for the payment screen and describe the UX flow for each payment method selection."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-007-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-007-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.33
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  DOCUMENT ("specify", "requirements", "describe") → 2.1.
  UI ("screen", "ux") → 1.1.
  DecisionCount: 2.

---

## B06-0B-007-003 — UI predicate routes to 1.1: ScreenInventoryFlow11

```
source_id: xiigen-b06-0b-007-003
source_title: XIIGen B06 router — UI predicate to 1.1 ScreenInventoryFlow11
source_license_state: first_party
pair_kind: router
```

Teaching note — Sub-phase 1.1 routing:

  UI predicate routes to sub-phase 1.1 (Screen Inventory and UX Requirements).
  The flow factory instantiates ScreenInventoryFlow11 for 1.1 routing decisions.

  The 1.1 flow is outside the primary scope of B06 (which covers 2.1), but
  the routing decision for 1.1 must still be produced when UI fires.

  DOCUMENT + UI → both 2.1 and 1.1 routing decisions must appear.
  Neither suppresses the other.

  Flow factory map for B06-touched sub-phases:
    1.1 → ScreenInventoryFlow11
    2.1 → RequirementsAuthoringFlow21
    2.4 → StakeholderRequirementsFlow24
    3.1 → ArchitectureFlow31
    4.1 → ImplementationFlow41
