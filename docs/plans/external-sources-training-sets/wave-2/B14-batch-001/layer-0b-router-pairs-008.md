# Layer 0-B — Router Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-008-001 — ARCHITECTURE+SECURITY+IMPLEMENT: network enforcement

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"] — design zero-trust layer, enforce TLS, implement network policies
B: → 3.5 + 6.3 (or 5.2 if implement-security context) + 4.1
  Teaching: When all three verbs coexist (design/architect, audit/enforce, implement), three flows created. SECURITY sub-phase depends on whether "audit" or "enforce/implement" dominates.

---

## B14-router-008-002 — ARCHITECTURE+AUTHORIZATION: RBAC design

A: ["ARCHITECTURE", "AUTHORIZATION"]
B: ARCHITECTURE → 3.5 | AUTHORIZATION → AUTHZ sub-phase
  Teaching: RBAC architecture design fires both ARCHITECTURE (structural layer) and AUTHORIZATION (permission model). Independent predicates, independent routing.

---

## B14-router-008-003 — Five-predicate: ARCHITECTURE+IMPLEMENT+TEST+SECURITY+DOCUMENT

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"]
B: → 3.5 + 4.1 + 5.1 + 6.3 + 6.2 | TransitionType: feature_addition
  Teaching: Five concurrent flows. No cap on predicate count. Each routes independently.

---

## B14-router-008-004 — ARCHITECTURE alone: "design" verb in non-infra context → 3.1 not 3.5

A: ["ARCHITECTURE"] | vocab: "design the domain model and service patterns for the OrderService"
B: → 3.1 (not 3.5)
  Teaching: Application/domain vocabulary (domain model, service patterns, module) routes ARCHITECTURE to 3.1. Infra vocabulary (topology, cluster, networking, infrastructure) routes to 3.5.

---

## B14-router-008-005 — gap: platform operational requests

A: []
B: → GapHandler | SCHEDULE_CAPABILITY_LEARNING emitted | DomainId: "software-development"
  Teaching: "deploy to production", "rollback the release", "restart the pod" → gap. No architecture design verb present.

