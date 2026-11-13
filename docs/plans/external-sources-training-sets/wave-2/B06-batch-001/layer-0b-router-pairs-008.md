# Layer 0-B — Router Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 008 covers [DOCUMENT] → 2.1 for behavior/use-case requests.
Single predicate, single routing decision.

---

## B06-0B-008-001 — [DOCUMENT] → 2.1 behavior and use cases

```
source_id: xiigen-b06-0b-008-001
source_title: XIIGen B06 router — DOCUMENT to 2.1 behavior use cases
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-008-001]
  DocumentId: "B06-0a-008-001-prompt"
  CompiledText: "Specify the behavior of the order confirmation system and describe the notification use cases."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-008-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.30
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  Pure 2.1 signals: "specify"→spec, "behavior", "describe", "use cases".
  No stakeholder, technical, or non-functional signals.
  DecisionCount: 1.

---

## B06-0B-008-002 — [DOCUMENT] → 2.1 functional requirements

```
source_id: xiigen-b06-0b-008-002
source_title: XIIGen B06 router — DOCUMENT to 2.1 functional requirements
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-008-003]
  DocumentId: "B06-0a-008-003-prompt"
  CompiledText: "Specify the functional requirements for the address validation step in the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-008-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.31
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  "specify"→specification, "functional", "requirements" → strong 2.1 signals.
  No competing sub-phase signals.
  DecisionCount: 1.

---

## B06-0B-008-003 — NearestWrongSubPhaseScore interpretation

```
source_id: xiigen-b06-0b-008-003
source_title: XIIGen B06 router — NearestWrongSubPhaseScore interpretation
source_license_state: first_party
pair_kind: router
```

Teaching note — NearestWrongSubPhaseScore interpretation:

  NearestWrongSubPhaseScore indicates the confidence of the closest competing
  sub-phase routing that was NOT selected.

  Score range: 0.0 to 1.0
  Low score (< 0.40):  weak competing signal; routing decision is clear
  High score (> 0.50): strong competing signal; dual routing may be warranted

  B06 examples:
    Score 0.30: "specify the behavior" → no competing sub-phase signals
    Score 0.39: "acceptance criteria and business rules" → 2.4 weakly present
    Score 0.44: "stakeholder rules" → 2.4 moderately present
    Score 0.55: "requirements and stakeholder expectations" → 2.4 strong enough
                for dual routing (both 2.1 and 2.4 are produced)

  Threshold rule: if NearestWrongSubPhaseScore >= 0.50 for a competing sub-phase,
  produce a routing decision for that sub-phase as well (dual routing).
