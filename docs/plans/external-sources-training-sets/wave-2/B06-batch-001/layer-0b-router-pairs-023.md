# Layer 0-B — Router Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 023 covers DOCUMENT+IMPLEMENT dual routing — four pairs demonstrating
that IMPLEMENT verbs always produce a 4.1 routing decision alongside 2.1.

---

## B06-0B-023-001 — "write the requirements document" → 2.1+4.1

```
source_id: xiigen-b06-0b-023-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Write the requirements document for the checkout payment error recovery flow."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2
  Anti-pattern: Suppressing 4.1 because "write" appears with documentation nouns.
    The router must honor the IMPLEMENT predicate from the compiler.

---

## B06-0B-023-002 — "create the user story" → 2.1+4.1

```
source_id: xiigen-b06-0b-023-002
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Create the user story for guest checkout address entry."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-023-003 — "build the spec" → 2.1+4.1

```
source_id: xiigen-b06-0b-023-003
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Build the spec for the order-summary checkout module."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2

---

## B06-0B-023-004 — "develop the feature-brief" → 2.1+4.1

```
source_id: xiigen-b06-0b-023-004
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledText: "Develop the feature-brief for address book checkout integration."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
    - PredictedPhaseId: "4"; PredictedSubPhaseId: "4.1"
      EvidencePredicates: ["IMPLEMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 2
  Note: "develop" → IMPLEMENT; "feature-brief" → DOCUMENT. Router honors both decisions.
