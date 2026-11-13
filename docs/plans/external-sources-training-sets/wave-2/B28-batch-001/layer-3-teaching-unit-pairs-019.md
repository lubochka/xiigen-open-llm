# Layer 3 — Teaching Unit Pairs (019)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU019-001 — QA+IMPLEMENT+DOCUMENT: multi-service coordinator

A: "Build the release coordinator service, document the inter-service dependency map, and QA sign-off."
B: ["IMPLEMENT", "DOCUMENT", "QA"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "document" → DOCUMENT; "QA sign-off" → QA.
Reason: Three predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]

---

## TU019-002 — Gap: deployment sequencing instruction

A: "Deploy the UserService first, then the OrderService, then the PaymentService."
B: [] — "deploy" → gap; service sequencing → gap.
Reason: Deployment sequencing instructions → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU019-003 — QA+DOCUMENT: multi-service criteria

A: "Document the multi-service release criteria and QA checklist."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA checklist" → QA.
Reason: "QA checklist" fires QA (compound noun). "criteria" fires DOCUMENT. TransitionType: documentation.
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU019-004 — QA+IMPLEMENT+TEST: orchestration + integration tests

A: "Implement the release orchestration, write integration tests for service handshakes, and QA approve."
B: ["IMPLEMENT", "TEST", "QA"] — "implement" → IMPLEMENT; "write integration tests" → TEST; "QA approve" → QA.
Reason: Three predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## TU019-005 — "service handshakes" does not suppress IMPLEMENT or TEST

A: "Implement the service handshake protocol and write integration tests."
B: ["IMPLEMENT", "TEST"] — "implement" → IMPLEMENT; "service" → IMPLEMENT (dedup); "write integration tests" → TEST.
Reason: "service handshakes" is a domain term. "implement" and "write tests" fire their predicates regardless of domain object.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]
