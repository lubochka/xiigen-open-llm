# Layer 0-C — Knowledge Edge Pairs (045) — SensitiveDataExposure Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E045-001 — Sensitive data vocabulary

"PII (Personally Identifiable Information) in logs" → SECURITY (sensitive data exposure)
"hardcoded secrets" → SECURITY (credential vocabulary)
"API keys in source code" → SECURITY (credential vocabulary)
"mask credit card digits" → gap (UI display formatting task)

---

## E045-002 — TLS enforcement boundary

"TLS encryption enforced" in audit context → SECURITY (TLS = crypto group + audit)
"use HTTPS" → gap (configuration instruction without audit vocabulary)
"audit TLS certificate configuration" → SECURITY (audit + TLS = crypto group)
Teaching: "use HTTPS" is gap. "audit TLS" fires SECURITY.

---

## E045-003 — Data-at-rest vs data-in-transit

"encrypt data at rest" → SECURITY (encrypt = crypto action)
"audit encryption at rest configuration" → SECURITY (audit + encryption)
"store data in the database" → gap (functional task)
Both "at-rest" and "in-transit" encryption audit fire SECURITY when paired with audit/review.

---

## E045-004 — "sensitive" keyword boundary

"sensitive data" in audit context → SECURITY (when paired with exposure/leakage/audit)
"sensitive data" alone → probationary_accepted (weak, needs action verb)
"log sensitive user data" → gap (logging feature request without security vocabulary)
"audit sensitive data exposure" → SECURITY (audit verb makes it deterministic)

---

## E045-005 — Trust state calibration

PII leakage + audit → deterministic_generic
TLS + encryption + audit → deterministic_generic (high, two crypto triggers)
"mask credit card" alone → gap (zero predicates)
