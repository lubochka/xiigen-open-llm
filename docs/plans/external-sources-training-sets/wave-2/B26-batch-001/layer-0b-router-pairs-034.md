# Layer 0-B — Router Pairs (034) — SessionFixationAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R034-001 — Session fixation audit → SECURITY → 6.3

R034-001 test: "Audit the login flow for session fixation vulnerabilities."
  SECURITY → 6.3 (audit + session fixation + vulnerabilities) | CompiledPredicateIds: ["SECURITY"]

---

## R034-002 — Session management review → SECURITY → 6.3

R034-002 test: "Review session management for predictable session IDs and fixation attack vectors."
  SECURITY → 6.3 (review + session + attack vectors) | CompiledPredicateIds: ["SECURITY"]

---

## R034-003 — Session token lifecycle → SECURITY → 6.3

R034-003 test: "Audit session token lifecycle: creation, rotation on elevation, invalidation on logout."
  SECURITY → 6.3 (audit + session token) | CompiledPredicateIds: ["SECURITY"]

---

## R034-004 — "set session timeout" → GapHandler

R034-004 test: "Set the session timeout to 30 minutes for inactive users."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "set session timeout" is a configuration task — gap without audit/vulnerability vocabulary.

---

## R034-005 — Authentication bypass via session → SECURITY → 6.3

R034-005 test: "Audit session creation for authentication bypass via pre-authenticated tokens."
  SECURITY → 6.3 (audit + authentication bypass + session) | CompiledPredicateIds: ["SECURITY"]
