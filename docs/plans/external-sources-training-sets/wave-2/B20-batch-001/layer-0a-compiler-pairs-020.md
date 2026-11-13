# Layer 0-A — Compiler Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P020-001 — TEST+IMPLEMENT: "write unit tests" for PasswordValidator

A: "Write unit tests for the PasswordValidator covering minimum length, special character requirements, and common password rejection."
  "write" → IMPLEMENT; "unit tests" → TEST; "covering" — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P020-002 — TEST: "assert" weak password rejection

A: "Assert that the PasswordValidator rejects passwords shorter than 8 characters and passwords that appear in the common-password blocklist."
  "assert" → TEST; "blocklist" — domain security term; "shorter than 8 characters" — domain rule
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P020-003 — TEST: "verify" complexity rules

A: "Verify that the PasswordValidator accepts a password only when it contains at least one uppercase letter, one digit, and one special character."
  "verify" → TEST; "complexity rules" — domain security policy; "uppercase" — domain rule
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P020-004 — TEST+SECURITY: "write security tests"

A: "Write security tests for the PasswordValidator to assert that hashed passwords are never logged and that validation timing is constant."
  "write" → IMPLEMENT; "security tests" → TEST via "tests" + SECURITY via "security"; "assert" → TEST;
  "hashed passwords" → SECURITY; "timing" — domain
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"] | SubPhases: [5.1, 4.1, 5.2]
  Teaching: "security tests" fires both TEST (via "tests") and SECURITY (via "security"). Three total predicates.

---

## P020-005 — TEST alone: "spec" password history enforcement

A: "Spec the password history rule: the PasswordValidator must reject any of the user's last 5 passwords."
  "spec" → TEST; "password history" — domain policy; no IMPLEMENT vocabulary
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
