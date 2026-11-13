# Layer 0-B — Router Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-003-001 — API_CONTRACT alone → 3.3

Predicates: ["API_CONTRACT"] | Routes to: 3.3
Routing decisions: 1
Evidence: API_CONTRACT maps to sub-phase 3.3 (ApiContract33FlowAdapter)
NearestWrongSubPhase: 2.1 (DOCUMENT sometimes co-fires with API_CONTRACT)
NearestWrongScore: 0.64
FactoryState: deterministic_generic (3.3 not yet trusted_learned for O1 scenario)

---

## O1-router-003-002 — SECURITY+API_CONTRACT → 3.4+3.3 (two routing decisions, fan-out)

Predicates: ["SECURITY", "API_CONTRACT"] | Routes to: 3.4, 3.3
Routing decisions: 2 (fan-out: each predicate maps to a different sub-phase)
Decision 1: SECURITY → 3.4 | Confidence: 0.92
Decision 2: API_CONTRACT → 3.3 | Confidence: 0.90
Fan-out rule: When predicates map to different sub-phases, each gets its own routing decision.
  Contrast with 002-003: SECURITY+AUTHORIZATION both map to 3.4 = 1 decision.
  Here SECURITY → 3.4 and API_CONTRACT → 3.3 = 2 decisions (different destinations).

---

## O1-router-003-003 — API_CONTRACT alone from "endpoint schema"

Predicates: ["API_CONTRACT"] | Routes to: 3.3
Routing decisions: 1
Source: "Specify the request and response schema for the token exchange endpoint."
SECURITY also fires on "token" → revised: Predicates: ["SECURITY", "API_CONTRACT"]
Revised routing decisions: 2 (SECURITY → 3.4, API_CONTRACT → 3.3)
Confidence: 0.92 each
Note: "token" fires SECURITY; always check all lexicons when composing routing decisions

---

## O1-router-003-004 — API_CONTRACT+AUTHORIZATION → 3.3+3.4 (fan-out)

Predicates: ["API_CONTRACT", "AUTHORIZATION"] | Routes to: 3.3, 3.4
Routing decisions: 2
Decision 1: API_CONTRACT → 3.3 | Confidence: 0.90
Decision 2: AUTHORIZATION → 3.4 | Confidence: 0.88
Source: "Generate the OpenAPI specification for the tenant management REST endpoints."
Note: Ordering follows canonical predicate order: API_CONTRACT before AUTHORIZATION

---

## O1-router-003-005 — "login" → no routing decision (API_CONTRACT does not fire)

Predicates: [] | Routes to: none
Routing decisions: 0
Source: "The login page keeps redirecting users after they submit the form."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Note: "login" fires no predicate; "page" fires no predicate; no API_CONTRACT, no SECURITY
  Result: single knowledge_edge decision record with KnowledgeEdge: true
