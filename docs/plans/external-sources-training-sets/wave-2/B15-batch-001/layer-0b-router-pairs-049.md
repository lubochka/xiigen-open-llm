# Layer 0-B — Router Pairs (049)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-049-001 — EventPublisherService: IMPLEMENT routing

A: "Build an EventPublisherService to publish domain events to the message broker."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-049-002 — EventPublisherService: IMPLEMENT+API_CONTRACT routing

A: "Build EventPublisherService and integrate with the RabbitMQ exchange."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-049-003 — EventPublisherService: IMPLEMENT+ARCHITECTURE routing

A: "Build EventPublisherService with an event domain model."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-049-004 — EventPublisherService: IMPLEMENT+TEST routing

A: "Build EventPublisherService and write unit tests for event serialization."
B: Flow: ImplementationFlow41 → TestingFlow51 | DecisionCount: 2

---

## B15-router-049-005 — Routing order confirmed for 049–060 range

All 049–060 services follow the same routing order as prior ranges:
ARCHITECTURE (3.1) → API_CONTRACT (3.3) → SECURITY → IMPLEMENT (4.1) → TEST (5.1) → DOCUMENT (2.1) → QA (7.x)
QA (7.1 or 7.3) always last — absolute ordering constraint.
