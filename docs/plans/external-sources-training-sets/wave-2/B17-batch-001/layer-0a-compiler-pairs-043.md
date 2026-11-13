# Layer 0-A — Compiler Pairs (043)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Redis connector

---

## B17-0A-043-001 — Redis: predicate firing

**Input:** "Build the Redis cache connector for session storage and pub/sub messaging."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: pub/sub, session, messaging are domain nouns — they fire no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-043-002 — Redis: "integrate" alone

**Input:** "Integrate the Redis client SDK."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — "integrate" never fires IMPLEMENT regardless of SDK or domain.

---

## B17-0A-043-003 — Redis: auth token fires SECURITY

**Input:** "Implement the Redis connector with ACL (Access Control List) authentication token."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

Note: ACL (Access Control List) is a domain acronym — it fires no predicate alone.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-043-004 — Redis: sync/cache verbs are not IMPLEMENT triggers

**Input:** "Sync application state through Redis and cache query results."

Triggers: none ("sync" and "cache" are not in the IMPLEMENT trigger list)
CompiledPredicateIds: []
TransitionType: none
Teaching: "sync", "cache", "store" do not fire IMPLEMENT.

---

## B17-0A-043-005 — Redis: refactor + connector

**Input:** "Refactor the Redis connector to use connection pooling and TLS (Transport Layer Security)."

| Trigger | Predicate |
|---|---|
| Refactor | IMPLEMENT |
| connector | API_CONTRACT |

Note: TLS (Transport Layer Security) as an acronym or "TLS" alone does not fire SECURITY (no "token" or authenticate/HMAC phrase).
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
DecisionCount: 2 | UniqueSubPhases: 2
