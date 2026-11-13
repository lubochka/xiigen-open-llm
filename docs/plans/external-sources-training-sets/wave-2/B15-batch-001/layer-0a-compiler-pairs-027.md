# Layer 0-A — Compiler Pairs (027)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-027-001 — InvoiceService: IMPLEMENT alone

A: "Implement an InvoiceService that generates PDF invoices for completed orders."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"implement" → IMPLEMENT. "PDF invoices" and "completed orders" are domain context, not predicate triggers.

---

## B15-compiler-027-002 — InvoiceService: IMPLEMENT+DOCUMENT

A: "Implement an InvoiceService and document the invoice generation workflow."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "document" → DOCUMENT. DOCUMENT is parallel-eligible with IMPLEMENT.

---

## B15-compiler-027-003 — InvoiceService: IMPLEMENT+API_CONTRACT

A: "Implement an InvoiceService and define the invoice data contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "data contracts" → API_CONTRACT ("contracts" keyword fires API_CONTRACT regardless of modifier).

---

## B15-compiler-027-004 — InvoiceService: IMPLEMENT+SECURITY

A: "Implement an InvoiceService and encrypt the invoice storage."
B: Predicates: ["IMPLEMENT", "SECURITY"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "encrypt" → SECURITY.

---

## B15-compiler-027-005 — "generate" fires IMPLEMENT

A: "Generate the InvoiceService with PDF export capability."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"generate" → IMPLEMENT (generate is in IMPLEMENT lexicon). "PDF export" is a feature description, not a predicate trigger.
