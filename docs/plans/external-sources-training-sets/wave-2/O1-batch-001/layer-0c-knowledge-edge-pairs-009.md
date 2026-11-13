# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-009-001 — Unroutable transition: predicate fires but sub-phase not registered

A: "Implement UI/UX improvements to the authentication flow dashboard."
B: KnowledgeEdge: unroutable_transition | Predicates: ["IMPLEMENT", "SECURITY", "UI"] | Action: route known sub-phases; UI sub-phase unregistered in O1

Explanation:
  IMPLEMENT fires on "implement"
  SECURITY fires on "authentication"
  UI fires on "ui/ux", "dashboard"
  But: the UI sub-phase is not registered in the O1 scenario topology
  Result: IMPLEMENT → 4.1 and SECURITY → 3.4 produce normal routing decisions
    UI predicate fires but no sub-phase is registered → produces one knowledge_edge record
    for the UI predicate; the other decisions proceed normally
  FailureClassification: "unroutable_transition" (for the UI portion only)

---

## O1-kedge-009-002 — Unroutable transition: ARCHITECTURE predicate fires, no arch sub-phase for O1

A: "Redesign the system architecture to support JWT-based multi-tenant authentication."
B: KnowledgeEdge: partial_unroutable | Predicates: ["ARCHITECTURE", "SECURITY", "AUTHORIZATION"] | Action: route SECURITY+AUTHORIZATION to 3.4; ARCHITECTURE unroutable for O1

Explanation:
  ARCHITECTURE fires on "architecture"
  SECURITY fires on "jwt"
  AUTHORIZATION fires on "tenant"
  SECURITY+AUTHORIZATION → 3.4 (normal routing)
  ARCHITECTURE → no specific sub-phase registered in O1 scope → unroutable_transition
  Note: In the broader topology ARCHITECTURE has a sub-phase; in O1-specific scope it may not
    be registered. The routing decision for ARCHITECTURE produces KnowledgeEdge: true

---

## O1-kedge-009-003 — Multi-predicate with partial knowledge edge

A: "Build the CI/CD (Continuous Integration / Continuous Delivery) pipeline security scanning and auth integration."
B: Predicates: ["IMPLEMENT", "SECURITY"] | KnowledgeEdge: false for SECURITY; CI/CD terms produce no additional predicate

Explanation:
  IMPLEMENT fires on "build"
  SECURITY fires on "security", "auth"
  "CI/CD", "pipeline", "scanning" — no predicate match; these are devops terms
  Result: IMPLEMENT → 4.1 and SECURITY → 3.4 route normally
  No knowledge_edge for known predicates; unrecognized terms simply produce no signal
  CI/CD expansion: CI/CD = Continuous Integration / Continuous Delivery

---

## O1-kedge-009-004 — Partial unroutable: known + unknown predicates

A: "Implement tenant RBAC (Role-Based Access Control) with blockchain audit logging."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | KnowledgeEdge: false for IMPLEMENT+AUTHORIZATION; blockchain not routable

Explanation:
  IMPLEMENT fires on "implement"
  AUTHORIZATION fires on "tenant", "rbac", "role"
  "blockchain", "audit logging" — no predicate match
  IMPLEMENT → 4.1 and AUTHORIZATION → 3.4 route normally
  The blockchain/audit terms do not produce a predicate or routing decision
  They also do not cause a knowledge_edge — they simply produce no signal
  RBAC expansion: RBAC = Role-Based Access Control
