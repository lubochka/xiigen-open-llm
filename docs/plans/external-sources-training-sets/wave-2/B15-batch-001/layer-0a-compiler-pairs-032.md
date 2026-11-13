# Layer 0-A — Compiler Pairs (032)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-032-001 — CreditService: IMPLEMENT alone

A: "Implement a CreditService that issues store credit to customers."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"implement" → IMPLEMENT. "store credit" is domain context.

---

## B15-compiler-032-002 — CreditService: IMPLEMENT+API_CONTRACT

A: "Implement a CreditService and define the credit issuance API contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "API contracts" → API_CONTRACT.

---

## B15-compiler-032-003 — CreditService: IMPLEMENT+ARCHITECTURE

A: "Implement a CreditService and architect the credit ledger model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "architect" verb → ARCHITECTURE. "credit ledger model" → ARCHITECTURE (domain + model phrase; deduplicated).

---

## B15-compiler-032-004 — CreditService: IMPLEMENT+SECURITY+DOCUMENT

A: "Implement a CreditService, secure the credit balance endpoints, and document the credit policy."
B: Predicates: ["IMPLEMENT", "SECURITY", "DOCUMENT"] | feature_addition | DecisionCount: 3
"implement" → IMPLEMENT. "secure" → SECURITY. "document" → DOCUMENT. Three predicates, three decisions.

---

## B15-compiler-032-005 — "code" fires IMPLEMENT

A: "Code the CreditService with balance cap enforcement."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"code" → IMPLEMENT (code is in IMPLEMENT lexicon). "balance cap enforcement" is a feature specification.
