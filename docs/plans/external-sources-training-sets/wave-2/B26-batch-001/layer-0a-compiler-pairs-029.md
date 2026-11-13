# Layer 0-A — Compiler Pairs (029) — NISTFramework
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C029-001 — NIST (National Institute of Standards and Technology) framework label is gap

A: "Align the security program to the NIST Cybersecurity Framework (CSF) identify and protect functions."
  "NIST CSF" → gap (framework label) | "align" → gap (management action)
  CompiledPredicateIds: [] → knowledge_edge

---

## C029-002 — NIST SP 800-53 + vulnerability audit → SECURITY

A: "Audit the system against NIST SP 800-53 (Security and Privacy Controls) AC-2 controls for privilege escalation."
  "audit" → SECURITY | "privilege escalation" → SECURITY + AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C029-003 — NIST + cryptographic controls review

A: "Review the cryptographic module implementations against NIST FIPS 140-2 (Federal Information Processing Standards) requirements."
  "Review" + "cryptographic module" → SECURITY | "FIPS 140-2" → gap context label
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C029-004 — NIST Identify function: risk vocabulary remains gap

A: "Perform an asset inventory and risk assessment as part of the NIST CSF Identify function."
  "risk assessment" → gap | "asset inventory" → gap (no security action verb)
  "NIST CSF" → gap | "Identify function" → gap
  CompiledPredicateIds: [] → knowledge_edge

---

## C029-005 — NIST Detect function + scan → SECURITY

A: "Scan all endpoints for anomalous authentication events as required by the NIST CSF Detect function."
  "Scan" (security context) → SECURITY | "authentication events" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
