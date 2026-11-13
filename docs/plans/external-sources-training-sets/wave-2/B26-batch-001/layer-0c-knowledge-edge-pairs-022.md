# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E022-001 — "RBAC" fires SECURITY; "roles" alone is gap

A-fires: "Audit the RBAC (Role-Based Access Control) configuration — confirm all roles follow the principle of least privilege."
  "audit" → SECURITY; "RBAC" → SECURITY (rbac trigger); "least privilege" → SECURITY (access control context)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The admin can assign roles to team members from the user management page."
  "assign roles" → gap (product feature); "user management" → gap
  CompiledPredicateIds: []

---

## E022-002 — "IDOR" fires BOTH SECURITY and AUTHORIZATION; disambiguation from "object ID"

A-fires: "Review the OrderService for IDOR (Insecure Direct Object Reference) — can user A access user B's orders by changing the order ID in the URL?"
  "review" (security context) → SECURITY; "IDOR" → SECURITY + AUTHORIZATION; "access user B's orders" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]

A-gap: "Pass the object ID as a URL parameter to the order detail endpoint."
  "object ID", "URL parameter" → gap (API design, not security audit)
  CompiledPredicateIds: []
  Teaching: "IDOR" fires both SECURITY and AUTHORIZATION. "object ID" alone is gap.

---

## E022-003 — "default-deny" fires SECURITY; "default" alone is gap

A-fires: "Audit the middleware stack for default-deny posture — are all routes protected until explicitly opened?"
  "audit" → SECURITY; "default-deny" → SECURITY; "routes protected" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Set the default pagination to 20 items per page."
  "default" → gap (UI configuration, not security)
  CompiledPredicateIds: []

---

## E022-004 — "wildcard permission" fires SECURITY+AUTHORIZATION; "asterisk" alone is gap

A-fires: "Audit the AdminRole for wildcard permissions — any role with '*' access violates least-privilege."
  "audit" → SECURITY; "wildcard permissions" → SECURITY+AUTHORIZATION; "least-privilege" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]

A-gap: "Use the asterisk (*) wildcard in the routing pattern to match all subpaths."
  "asterisk" / "wildcard in routing pattern" → gap (URL routing, not access control)
  CompiledPredicateIds: []

---

## E022-005 — "unauthenticated" fires SECURITY; "anonymous" endpoint is context-dependent

A-fires: "Audit all unauthenticated API endpoints — verify that public exposure is intentional and documented."
  "audit" → SECURITY; "unauthenticated API endpoints" → SECURITY (authentication + audit context)
  CompiledPredicateIds: ["SECURITY"]

A-borderline: "The /health endpoint is intentionally anonymous."
  "anonymous" → gap in isolation; "anonymous" without audit/vulnerability vocabulary → no SECURITY
  CompiledPredicateIds: []
