# Layer 0-C — Knowledge Edge Pairs (047) — PrivilegeEscalationAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E047-001 — Dual-predicate trigger vocabulary (complete list)

Fires BOTH SECURITY and AUTHORIZATION:
  "privilege escalation" (always dual)
  "IDOR" (Insecure Direct Object Reference) (always dual)
  "wildcard permission" (always dual)
  "RBAC bypass" (always dual)
  "role claim forgery" (always dual)
  "unauthorized resource access" (always dual)

---

## E047-002 — "assign admin role" is gap

"assign admin role to user" → gap (administrative action without audit vocabulary)
"grant superuser privilege" → gap (role management action without audit vocabulary)
"audit whether admin role grants are audited" → SECURITY (audit verb present)
Teaching: Performing role assignments is gap. Auditing them fires SECURITY.

---

## E047-003 — Vertical vs horizontal escalation

Vertical: "gaining higher privilege tier" → SECURITY + AUTHORIZATION
Horizontal: "accessing another user's resources" = IDOR pattern → SECURITY + AUTHORIZATION
Both directions fire the same dual-predicate set.

---

## E047-004 — RBAC vs AUTHORIZATION header disambiguation

"RBAC role hierarchy" → AUTHORIZATION (access control vocabulary)
"Authorization: Bearer token" HTTP header → API_CONTRACT (not AUTHORIZATION predicate)
"audit RBAC for privilege escalation" → SECURITY + AUTHORIZATION (dual)
Teaching: HTTP Authorization header is API_CONTRACT. RBAC/IAM/permissions = AUTHORIZATION predicate.

---

## E047-005 — Trust state for dual predicates

Both SECURITY and AUTHORIZATION route to 6.3 in audit context → two concurrent flows, same sub-phase
privilege escalation alone + audit → deterministic_generic
IDOR + wildcard permission + RBAC bypass + audit → deterministic_generic (high)
