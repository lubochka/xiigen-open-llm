# Layer 0-B — Router Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-005-001 — DOCUMENT alone → 2.1

Predicates: ["DOCUMENT"] | Routes to: 2.1
Routing decisions: 1
Evidence: DOCUMENT maps to sub-phase 2.1 (RequirementsAuthoringFlow21 — deterministic_generic)
NearestWrongSubPhase: 2.4 (edge-cases; also carries DOCUMENT)
NearestWrongScore: 0.71
FactoryState: deterministic_generic

---

## O1-router-005-002 — API_CONTRACT+DOCUMENT → 3.3+2.1 (two routing decisions)

Predicates: ["API_CONTRACT", "DOCUMENT"] | Routes to: 3.3, 2.1
Routing decisions: 2
Decision 1: API_CONTRACT → 3.3 | Confidence: 0.92 | EvidencePredicates: ["API_CONTRACT"]
Decision 2: DOCUMENT → 2.1 | Confidence: 0.90 | EvidencePredicates: ["DOCUMENT"]
Source: "Document the REST endpoints for the RBAC management API."
Ordering: API_CONTRACT (3.3) before DOCUMENT (2.1) per canonical predicate ordering

---

## O1-router-005-003 — SECURITY+DOCUMENT → 3.4+2.1 (two routing decisions)

Predicates: ["SECURITY", "DOCUMENT"] | Routes to: 3.4, 2.1
Routing decisions: 2
Decision 1: SECURITY → 3.4 | Confidence: 0.92
Decision 2: DOCUMENT → 2.1 | Confidence: 0.90
Source: "Write the security specification covering JWT issuance policies."
Note: "write" + "specification" fires DOCUMENT; "jwt", "security" fire SECURITY

---

## O1-router-005-004 — AUTHORIZATION+DOCUMENT → 3.4+2.1 (two routing decisions)

Predicates: ["AUTHORIZATION", "DOCUMENT"] | Routes to: 3.4, 2.1
Routing decisions: 2
Decision 1: AUTHORIZATION → 3.4 | Confidence: 0.88
Decision 2: DOCUMENT → 2.1 | Confidence: 0.90
Source: "Write the specification for tenant onboarding and permission provisioning."
Note: "write" + "specification" fires DOCUMENT; "tenant", "permission" fire AUTHORIZATION

---

## O1-router-005-005 — SECURITY+API_CONTRACT+DOCUMENT → 3.4+3.3+2.1 (three routing decisions)

Predicates: ["SECURITY", "API_CONTRACT", "DOCUMENT"] | Routes to: 3.4, 3.3, 2.1
Routing decisions: 3
Decision 1: SECURITY → 3.4 | Confidence: 0.92
Decision 2: API_CONTRACT → 3.3 | Confidence: 0.90
Decision 3: DOCUMENT → 2.1 | Confidence: 0.90
Source: "Document the authentication API contract for external developers."
Note: Each predicate maps to a different sub-phase → three independent routing decisions
