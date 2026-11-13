# Layer 0-B — Router Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 004 covers [ARCHITECTURE, DOCUMENT, IMPLEMENT] → 3.1 + 2.1 + 4.1 three decisions.

---

## B06-0B-004-001 — [ARCHITECTURE, DOCUMENT, IMPLEMENT] → 3.1 + 2.1 + 4.1

```
source_id: xiigen-b06-0b-004-001
source_title: XIIGen B06 router — three predicates three routing decisions
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-004-001]
  DocumentId: "B06-0a-004-001-prompt"
  CompiledText: "Design the technical architecture for the checkout service and write the functional specification."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-004-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-004-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-004-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale:
  ARCHITECTURE ("design", "architecture") → 3.1 (Technical Architecture).
  DOCUMENT ("functional", "specification") → 2.1 (Business Requirements).
  IMPLEMENT ("write") → 4.1 (Backend Services).
  Three independent decisions, each citing its own evidence predicate.

---

## B06-0B-004-002 — [ARCHITECTURE, DOCUMENT] without IMPLEMENT → 3.1 + 2.1 only

```
source_id: xiigen-b06-0b-004-002
source_title: XIIGen B06 router — ARCHITECTURE+DOCUMENT without IMPLEMENT
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-004-002]
  DocumentId: "B06-0a-004-002-prompt"
  CompiledText: "Design the architecture for the checkout service and document the functional requirements."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-004-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-004-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  No IMPLEMENT → no 4.1 decision.
  ARCHITECTURE → 3.1; DOCUMENT → 2.1.
  DecisionCount: 2.

---

## B06-0B-004-003 — Sub-phase 3.1 routing: ARCHITECTURE predicate

```
source_id: xiigen-b06-0b-004-003
source_title: XIIGen B06 router — ARCHITECTURE predicate to 3.1
source_license_state: first_party
pair_kind: router
```

Teaching note — ARCHITECTURE predicate routing:

  ARCHITECTURE predicate routes to sub-phase 3.1
  (Technical Architecture and System Design).

  Predicate → Sub-phase routing map (complete for B06):
    ARCHITECTURE → 3.1 (Technical Architecture and System Design)
    DOCUMENT     → 2.1 (Business Requirements and Use Cases)
                   [also 2.4 when stakeholder signals present]
    IMPLEMENT    → 4.1 (Backend Services and Business Logic)
    UI           → 1.1 (Screen Inventory and UX Requirements)

  This map is the core routing table that MultiSubPhaseRouterUnit applies.
  Each predicate has exactly one primary sub-phase target (with the exception
  of DOCUMENT which can target 2.1 and/or 2.4 based on sub-phase signals).
