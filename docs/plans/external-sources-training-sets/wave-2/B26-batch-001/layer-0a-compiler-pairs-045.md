# Layer 0-A — Compiler Pairs (045) — SensitiveDataExposure
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C045-001 — PII (Personally Identifiable Information) in logs → SECURITY

A: "Audit application logs for sensitive PII (Personally Identifiable Information) leakage including email addresses and phone numbers."
  "audit" → SECURITY | "sensitive" in log-leakage context → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C045-002 — Secret in environment variable scan → SECURITY

A: "Scan the codebase for hardcoded secrets, API (Application Programming Interface) keys, and database credentials."
  "scan" → SECURITY | "hardcoded secrets" → SECURITY | "credentials" → SECURITY (credential vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C045-003 — TLS (Transport Layer Security) data-in-transit → SECURITY

A: "Audit all external API calls to verify TLS encryption is enforced for data in transit."
  "audit" → SECURITY | "TLS" → SECURITY (cryptographic group) | "encryption" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C045-004 — "mask credit card numbers in UI" → gap (no audit vocabulary)

A: "Mask the last four digits of credit card numbers in the account dashboard UI (User Interface)."
  "mask" → gap | "credit card" → gap (display formatting task, no audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C045-005 — Sensitive data + DOCUMENT → SECURITY+DOCUMENT

A: "Audit sensitive data exposure across all API (Application Programming Interface) response bodies and document the findings in a data classification report."
  "audit" → SECURITY | "sensitive data exposure" → SECURITY | "document findings" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
