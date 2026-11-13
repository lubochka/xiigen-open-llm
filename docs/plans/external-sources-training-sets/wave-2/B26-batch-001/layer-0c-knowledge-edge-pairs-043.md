# Layer 0-C — Knowledge Edge Pairs (043) — TimingAttackAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E043-001 — Timing attack vocabulary

"timing attack" → SECURITY (attack vector vocabulary)
"constant-time comparison" in audit context → SECURITY (security-specific algorithm requirement)
"optimize comparison speed" → gap (performance context)
"early-exit string comparison" in audit context → SECURITY (vulnerability pattern)

---

## E043-002 — User enumeration via timing boundary

"user enumeration via differential response latency" → SECURITY (enumeration = vulnerability)
"response time monitoring" → gap (observability/performance context)
"audit login timing for user enumeration" → SECURITY (audit + enumeration vulnerability)
Teaching: Enumeration as a vulnerability fires SECURITY; monitoring response time does not.

---

## E043-003 — HMAC comparison specificity

"HMAC verification with constant-time comparison" in audit context → SECURITY (HMAC = crypto group)
"implement HMAC" → IMPLEMENT (not SECURITY alone)
"audit HMAC comparison for timing leakage" → SECURITY (audit + HMAC + timing)

---

## E043-004 — Password comparison boundary

"password comparison" in timing-attack audit → SECURITY (credential vocabulary)
"compare password hashes" in feature context → gap (functional task without audit vocabulary)
"audit password comparison for timing attack vulnerability" → SECURITY (audit + vulnerability)

---

## E043-005 — Trust state calibration

Timing attack + audit → deterministic_generic
HMAC + constant-time + audit → deterministic_generic (high, two crypto triggers)
"optimize latency" alone → gap (zero predicates)
