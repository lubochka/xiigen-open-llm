# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-032-001 — "store credit" domain phrase: no predicate fires

"store credit", "credit balance", "credit ledger" are domain phrases — no predicates.
"implement CreditService that issues store credit" → "implement" fires IMPLEMENT only.

---

## B15-kedge-032-002 — "credit ledger model" fires ARCHITECTURE

"architect the credit ledger model" → ARCHITECTURE fires from both "architect" (verb) and "credit ledger model" (domain model pattern).
Deduplicated to one ARCHITECTURE predicate.

---

## B15-kedge-032-003 — "API contracts" fires API_CONTRACT

"define the credit issuance API contracts" → API_CONTRACT fires on "contracts" keyword.
"credit issuance" is a domain qualifier — "API contracts" is the trigger phrase.

---

## B15-kedge-032-004 — "secure the credit balance endpoints" fires SECURITY

"secure the credit balance endpoints" → SECURITY fires on "secure".
"credit balance endpoints" are the targets — no other predicate fires from this phrase.

---

## B15-kedge-032-005 — "code" fires IMPLEMENT

"code the CreditService with balance cap enforcement" → IMPLEMENT fires from "code".
"balance cap enforcement" is a feature requirement description — no predicate fires from it.
Complete IMPLEMENT lexicon confirmation: build / implement / create / scaffold / bootstrap / add / write (code) / extend / develop / code / generate / refactor / process.
