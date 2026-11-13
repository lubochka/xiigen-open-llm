# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-049-001 — EventPublisherService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "event domain model" → ARCHITECTURE
- "event envelope model" → ARCHITECTURE
- "bounded context" (event publishing bounded context) → ARCHITECTURE

---

## B15-kedge-049-002 — EventPublisherService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "integrate with the RabbitMQ exchange" → API_CONTRACT
- "integrate with the Kafka (distributed event streaming platform) topic" → API_CONTRACT
- "wire up the message broker API" → API_CONTRACT
- "event publisher service contracts" → API_CONTRACT

---

## B15-kedge-049-003 — EventPublisherService: TEST trigger

"write unit tests for event serialization" → TEST.
"test event routing and delivery confirmation" → TEST.

---

## B15-kedge-049-004 — "publish" and "emit" are IMPLEMENT trigger verbs (confirmed)

"publish" → IMPLEMENT. "emit" → IMPLEMENT.
Both fire feature_addition TransitionType when "refactor" absent.
"Publish user.created events to the order topic" → IMPLEMENT.

---

## B15-kedge-049-005 — "event" and "message" are domain nouns — do NOT fire predicates

"event", "message", "topic", "exchange", "queue" are domain nouns.
None fire predicates. IMPLEMENT fires only from action verbs.
v6-01 risk: "event" suppressing IMPLEMENT or triggering unknown predicate is incorrect.
