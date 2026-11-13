# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: DocuSign e-signature connector edge cases

---

## B17-0C-028-001 — DocuSign: "Connect webhook" is a product feature, not a verb phrase

DocuSign "Connect" is a product name (the DocuSign webhook notification service). It is a proper noun.
- "Build the DocuSign Connect webhook connector" → "connector" and "webhook" are integration vocabulary; "Connect" (product name) is NOT
- IMPLEMENT → 4.3 because of "connector" and "webhook", not because of the product name "Connect"

---

## B17-0C-028-002 — DocuSign: "e-signature" is a domain noun

"e-signature", "envelope", "recipient", "document workflow" — domain nouns for DocuSign.
None of these fire any predicate. They are transparent to the compiler.
Only "connector", "webhook", "API", "endpoint", "OAuth", "token" etc. fire predicates.

---

## B17-0C-028-003 — DocuSign: refactor preserves 4.3 routing

Input: "Refactor the DocuSign connector to use the v2.1 eSignature REST API endpoint."
- "Refactor" → IMPLEMENT with TransitionType=refactoring
- "connector" → API_CONTRACT (integration vocabulary → IMPLEMENT routes to 4.3)
- "endpoint" → API_CONTRACT (reinforces; fires once)

The refactoring flag changes TransitionType but does NOT change sub-phase routing.
4.3 routing comes from integration vocabulary — refactoring does not remove it.

---

## B17-0C-028-004 — DocuSign: "access token refresh" — token unconditional

"access token refresh" contains "token" → SECURITY fires unconditionally.
No context phrase ("access", "refresh") suppresses SECURITY.
This is the v6-02 prevention rule: Rule 6 has zero exceptions.
