# Layer 0-A — Compiler Pairs (049)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-049-001 — EventPublisherService: IMPLEMENT from "build"

A: "Build an EventPublisherService to publish domain events to the message broker."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT.

---

## B15-compiler-049-002 — EventPublisherService: IMPLEMENT+API_CONTRACT on "integrate"

A: "Build EventPublisherService and integrate with the RabbitMQ (message broker) exchange."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "integrate" → API_CONTRACT.

---

## B15-compiler-049-003 — EventPublisherService: IMPLEMENT+ARCHITECTURE on "event model"

A: "Build EventPublisherService with an event domain model defining event envelope structure."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "event domain model" → ARCHITECTURE.

---

## B15-compiler-049-004 — EventPublisherService: IMPLEMENT+TEST on "write tests"

A: "Build EventPublisherService and write unit tests for event serialization."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"build" → IMPLEMENT. "write unit tests" → TEST.

---

## B15-compiler-049-005 — "publish" and "emit" are IMPLEMENT trigger verbs

"publish" and "emit" as code artifact creation verbs fire IMPLEMENT.
"Publish domain events to the broker" → IMPLEMENT. "Emit user.created events" → IMPLEMENT.
Both are feature_addition TransitionType when no "refactor" present.
