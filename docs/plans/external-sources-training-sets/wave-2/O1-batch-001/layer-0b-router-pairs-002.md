# Layer 0-B — Router Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-002-001 — Single SECURITY → 3.4

Predicates: ["SECURITY"] | Routes to: 3.4
Routing decisions: 1
Evidence: SECURITY alone maps to sub-phase 3.4 (SecurityVerification34Flow)
NearestWrongSubPhase: 3.3 (shares phase 3 but lacks SECURITY as primary signal)
NearestWrongScore: 0.58

---

## O1-router-002-002 — Single AUTHORIZATION → 3.4

Predicates: ["AUTHORIZATION"] | Routes to: 3.4
Routing decisions: 1
Evidence: AUTHORIZATION alone maps to sub-phase 3.4 (SecurityVerification34Flow)
Note: AUTHORIZATION shares 3.4 with SECURITY — both route to the security and authorization
  model sub-phase. A single AUTHORIZATION predicate is sufficient to trigger 3.4.
NearestWrongSubPhase: 4.5 (IMPLEMENT+AUTHORIZATION overlap)
NearestWrongScore: 0.55

---

## O1-router-002-003 — SECURITY+AUTHORIZATION → 3.4 (one routing decision, two predicates)

Predicates: ["SECURITY", "AUTHORIZATION"] | Routes to: 3.4
Routing decisions: 1 (SECURITY+AUTHORIZATION both route to 3.4 — single sub-phase, two predicates)
Evidence: Both SECURITY and AUTHORIZATION map to the same destination (3.4)
Key rule: Two predicates, one destination = one routing decision. The decision carries
  both EvidencePredicates. This is NOT two separate decisions.
NearestWrongSubPhase: 3.3 (SECURITY signal has partial overlap)
NearestWrongScore: 0.62

---

## O1-router-002-004 — SECURITY alone with high confidence

Predicates: ["SECURITY"] | Routes to: 3.4
Routing decisions: 1
Source: "Harden the authentication flow to prevent replay attacks."
Confidence: 0.92 (strong SECURITY signal from "harden", "authentication")
NearestWrongSubPhase: 4.1 (IMPLEMENT+SECURITY overlap; "harden" is action verb)
NearestWrongScore: 0.61
Note: Even when IMPLEMENT-adjacent verbs appear, SECURITY primary signal wins for 3.4

---

## O1-router-002-005 — AUTHORIZATION alone with role signal

Predicates: ["AUTHORIZATION"] | Routes to: 3.4
Routing decisions: 1
Source: "Build the role assignment service for tenant administrators."
Confidence: 0.88
NearestWrongSubPhase: 4.1 (IMPLEMENT fires on "build" but AUTHORIZATION is routing signal)
NearestWrongScore: 0.60
Note: IMPLEMENT fires for TransitionType selection but does NOT create a second routing
  decision unless IMPLEMENT alone appears without other security/auth predicates
