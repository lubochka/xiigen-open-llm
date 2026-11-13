# Layer 0-C — Knowledge Edge Pairs (034) — SessionFixationAudit Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E034-001 — Session fixation vocabulary that fires SECURITY

"session fixation" → SECURITY (session attack vector)
"session ID regeneration" → SECURITY (session security)
"predictable session IDs" → SECURITY (session vulnerability)
"pre-authenticated session tokens" → SECURITY (session + token)
"session hijacking" → SECURITY (session attack vector)

---

## E034-002 — Session timeout vocabulary: gap vs SECURITY

"session timeout" alone (in configuration context) → gap
"session timeout security gap" → SECURITY ("security gap" is actionable)
"audit session expiration for fixation risk" → SECURITY (audit + session + risk in security context)
Teaching: "session timeout" is a setting; "session fixation" is an attack vector. The latter fires SECURITY.

---

## E034-003 — Authentication bypass via session

"authentication bypass" → SECURITY (strong solo trigger)
"pre-authenticated token reuse" → SECURITY (token + session)
"session not invalidated after logout" → SECURITY (session security invariant)

---

## E034-004 — Session group membership

Full session group vocabulary (all fire SECURITY independently):
  session, session token, session ID, session fixation, session hijacking,
  session invalidation, stateless token, session management

---

## E034-005 — Trust state for session audits

"Audit login flow for session fixation and authentication bypass vulnerabilities" → deterministic_generic
"Review session management implementation" → probationary_accepted (review + session)
"Set session timeout to 30 minutes" → knowledge_edge (configuration, zero predicates)
