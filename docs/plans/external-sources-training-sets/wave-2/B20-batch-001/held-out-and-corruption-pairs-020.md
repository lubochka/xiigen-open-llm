# Held-Out and Corruption Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-020-001 — "write security tests" fires TEST+IMPLEMENT+SECURITY

A: "Write security tests for the PasswordValidator covering timing attack resistance and hash logging prevention."
Expected: IMPLEMENT fires on "write". TEST fires on "security tests" (via "tests"). SECURITY fires on "security tests" (via "security") + "hash" + "timing attack".
CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"]. Three routing decisions.

---

## HO-020-002 — "spec" password history rule fires TEST only

A: "Spec the password history enforcement: reject any of the user's last 5 previously used passwords."
Expected: TEST fires on "spec". No IMPLEMENT verb. CompiledPredicateIds: ["TEST"]. TransitionType: documentation.

---

## COR-020-001 — v6-01: "validate" treated as business-rule validation (not TEST)

CORRUPT: "Validate that the PasswordValidator rejects passwords shorter than 8 characters." → CompiledPredicateIds: [].
Why corrupt: The corrupt model interpreted "validate" as a business-rule verification term, not TEST vocabulary.
Correct: "validate" fires TEST universally — business-rule validation, data validation, and test assertion are the same predicate.
CompiledPredicateIds: ["TEST"]. Routing: 5.1.

---

## COR-020-002 — v6-02: TEST invented for "ensure password is testable"

CORRUPT: "Ensure the PasswordValidator is testable by extracting complexity logic." → CompiledPredicateIds: ["TEST", "IMPLEMENT"].
Why corrupt: "testable" (adjective) was promoted to TEST. "ensure" was treated as TEST vocabulary.
Correct: "testable" (adjective) ≠ TEST. "ensure" is gap. IMPLEMENT fires on "extracting".
CompiledPredicateIds: ["IMPLEMENT"]. TransitionType: refactoring.
