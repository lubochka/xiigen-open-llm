# Layer 0-B — Router Pairs (035)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## B06-0B-035-001 — DigitalProductDelivery: DOCUMENT → 2.1

```
source_id: xiigen-b06-0b-035-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define the digital product delivery requirements including download link generation."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1

---

## B06-0B-035-002 — DigitalProductDelivery: DOCUMENT + IMPLEMENT → 2.1 + 4.1

```
source_id: xiigen-b06-0b-035-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Specify the digital delivery specification and develop the license key generation service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-035-003 — DigitalProductDelivery: DOCUMENT + ARCHITECTURE + IMPLEMENT → 2.1 + 3.1 + 4.1

```
source_id: xiigen-b06-0b-035-003
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  CompiledText: "Write the digital delivery requirements, design the CDN architecture, and build the download token service."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "3"; PredictedSubPhaseId: "3.1"
      EvidencePredicates: ["ARCHITECTURE"]; Confidence: 0.91; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.91; KnowledgeEdge: false
  DecisionCount: 3
  CanonicalOrdering: 2.1 → 3.1 → 4.1

---

## B06-0B-035-004 — DigitalProductDelivery: knowledge-edge

```
source_id: xiigen-b06-0b-035-004
source_license_state: first_party
pair_kind: router
```

A:
  CompiledPredicateIds: []
  CompiledText: "Email the download link to the customer after payment confirmation."
  KnowledgeEdge: true

B:
  RoutingDecisions: []
  DecisionCount: 0
  KnowledgeEdge: true
