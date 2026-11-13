# Layer 0-C — Knowledge Edge Pairs (042) — RaceConditionAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E042-001 — Race condition vocabulary boundary

"race condition" in security audit context → SECURITY (vulnerability pattern)
"race condition" in performance context → gap ("optimize locking for throughput" = gap)
"TOCTOU" (Time-of-Check to Time-of-Use) → SECURITY (security-specific attack pattern abbreviation)
"concurrency bug" → gap (engineering term without security-action vocabulary)

---

## E042-002 — Token double-spend pattern

"double-spend of one-time tokens" → SECURITY (token + vulnerability context)
"OTP (One-Time Password) race condition audit" → SECURITY (audit + token vocabulary)
"use token once" → gap (implementation instruction without audit vocabulary)

---

## E042-003 — Permission check race (TOCTOU) boundary

"file permission check race condition" → SECURITY (permission boundary + vulnerability)
"check file permissions before accessing" → gap (no audit/vulnerability vocabulary)
Teaching: TOCTOU fires SECURITY when paired with audit/review. Permission checks alone are gap.

---

## E042-004 — Session creation race

"race condition in session creation allowing privilege escalation" → SECURITY + AUTHORIZATION
"create session on login" → gap (feature implementation, no vulnerability vocabulary)
Teaching: "privilege escalation" always fires both SECURITY and AUTHORIZATION regardless of context.

---

## E042-005 — Trust state calibration

Race condition + audit + token vocabulary → deterministic_generic
TOCTOU alone + review → probationary_accepted
Race condition + privilege escalation + audit → deterministic_generic (high, dual predicate)
