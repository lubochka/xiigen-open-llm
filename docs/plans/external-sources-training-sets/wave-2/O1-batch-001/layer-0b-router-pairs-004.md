# Layer 0-B — Router Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-004-001 — TEST alone → 5.1

Predicates: ["TEST"] | Routes to: 5.1
Routing decisions: 1
Evidence: TEST maps to sub-phase 5.1 (UnitTestDesign51FlowAdapter — probationary_rejected, generic fallback)
NearestWrongSubPhase: 5.2 (also carries TEST; discriminator is API_CONTRACT for 5.2)
NearestWrongScore: 0.70
FactoryState: probationary_rejected (5.1 specific flow rejected; generic nine-node used)

---

## O1-router-004-002 — SECURITY+TEST → 3.4+5.1 (two routing decisions)

Predicates: ["SECURITY", "TEST"] | Routes to: 3.4, 5.1
Routing decisions: 2
Decision 1: SECURITY → 3.4 | Confidence: 0.92 | EvidencePredicates: ["SECURITY"]
Decision 2: TEST → 5.1 | Confidence: 0.88 | EvidencePredicates: ["TEST"]
Source: "Write tests for the JWT token validation middleware."
Fan-out rule: SECURITY and TEST route to different sub-phases → 2 separate decisions
Ordering: SECURITY (3.4) appears before TEST (5.1) per canonical predicate ordering

---

## O1-router-004-003 — AUTHORIZATION+TEST → 3.4+5.1 (two routing decisions)

Predicates: ["AUTHORIZATION", "TEST"] | Routes to: 3.4, 5.1
Routing decisions: 2
Decision 1: AUTHORIZATION → 3.4 | Confidence: 0.88 | EvidencePredicates: ["AUTHORIZATION"]
Decision 2: TEST → 5.1 | Confidence: 0.88 | EvidencePredicates: ["TEST"]
Source: "Assert that each tenant's data is unreachable by other tenants in integration tests."

---

## O1-router-004-004 — SECURITY+AUTHORIZATION+TEST → 3.4+5.1 (two routing decisions, not three)

Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] | Routes to: 3.4, 5.1
Routing decisions: 2 (not 3 — SECURITY+AUTHORIZATION fold into one 3.4 decision)
Decision 1: SECURITY+AUTHORIZATION → 3.4 | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Decision 2: TEST → 5.1 | EvidencePredicates: ["TEST"]
Key rule: SECURITY and AUTHORIZATION are co-predicate for 3.4; they are not split into
  two separate 3.4 routing decisions. The result is exactly 2 decisions: one for 3.4, one for 5.1.

---

## O1-router-004-005 — TEST alone with "verify isolation" (TEST not SECURITY)

Predicates: ["TEST"] | Routes to: 5.1
Routing decisions: 1
Source: "Verify isolation between tenant auth contexts in the test suite."
SECURITY also fires on "auth" | AUTHORIZATION also fires on "tenant"
Revised: Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] → 2 routing decisions (3.4, 5.1)
Critical boundary: "verify isolation" fires TEST — the isolation concept is a test concern,
  not a security implementation concern. v6-03 error is routing "verify isolation" to 3.4.
