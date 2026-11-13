# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Segment analytics connector edges

---

## B17-0C-017-001 — Edge: analytics domain connector vocabulary

**Assertion:** Analytics domain connectors use the same integration vocabulary tokens as other connector domains. The word "analytics" itself is not in the predicate vocabulary.

**Analytics connector vocabulary that fires predicates:**
| Token | Predicate |
|---|---|
| connector | API_CONTRACT (Rule 4) |
| API | API_CONTRACT (reinforcer) |
| endpoint | API_CONTRACT (reinforcer) |
| wire up | API_CONTRACT (Rule 22) |
| webhook (if analytics uses webhooks) | API_CONTRACT (Rule 5) |
| build, implement, write, create | IMPLEMENT |

**Terms that do NOT fire predicates:**
- analytics, event, tracking, page view, identify, track, group, alias — domain vocabulary, no predicate

---

## B17-0C-017-002 — Edge: "event sink API" → API_CONTRACT via "API" token

**Input:** "Create the Segment analytics connector, wire up the event sink API endpoints."

**Edge assertions:**
- "Create" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "wire up" → API_CONTRACT (Rule 22; reinforces once)
- "API" → API_CONTRACT (reinforces once)
- "endpoints" → API_CONTRACT (reinforces)
- All four API_CONTRACT triggers → predicate fires once

**Result:** ["IMPLEMENT", "API_CONTRACT"]
**DecisionCount:** 2

---

## B17-0C-017-003 — Edge: "data pipeline" without connector vocabulary → 4.1 routing

**Input:** "Implement the analytics data pipeline with event batching and retry logic."

**Edge assertions:**
- "Implement" → IMPLEMENT
- "data pipeline" — data pipeline is not integration vocabulary in this corpus (no connector, webhook, API, external service)
- "batching" — internal operation, not integration vocabulary
- "logic" — internal term

**Result:** ["IMPLEMENT"] → 4.1 (no integration vocabulary present)

**Teaching point:** "data pipeline" sounds like integration work but does not contain the integration vocabulary tokens. Without connector, webhook, API, external service, third-party, gateway, SDK, or endpoint, IMPLEMENT routes to 4.1.

---

## B17-0C-017-004 — Edge: refactoring path changes TransitionType, not routing

**Input:** "Refactor the Segment connector to use the Segment HTTP API v1.3 batch endpoint."

**Edge assertions:**
- "Refactor" → IMPLEMENT (Rule 14: refactor fires IMPLEMENT + sets TransitionType: refactoring)
- "connector" → API_CONTRACT (Rule 4)
- "API" → API_CONTRACT (reinforces)
- "endpoint" → API_CONTRACT (reinforces)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3 (same as feature_addition)
**TransitionType:** refactoring

**Edge confirmed:** Refactoring changes only TransitionType. The sub-phase routing (IMPLEMENT → 4.3) is identical whether the verb is "build" (feature_addition) or "refactor" (refactoring).
