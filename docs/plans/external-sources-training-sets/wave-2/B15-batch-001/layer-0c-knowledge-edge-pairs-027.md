# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-027-001 — "invoice" domain noun: no predicate fires

"invoice", "PDF", "invoice generation" are domain nouns and descriptions.
"implement InvoiceService" → IMPLEMENT fires from "implement" verb only.

---

## B15-kedge-027-002 — "document the workflow" fires DOCUMENT

"document the invoice generation workflow" → DOCUMENT fires on "document" verb.
"document" as a noun (e.g., "the invoice document") does NOT fire DOCUMENT.
Teaching: DOCUMENT fires from verb usage of "document" / "spec out" / "define spec".

---

## B15-kedge-027-003 — "data contracts" fires API_CONTRACT

"define the invoice data contracts" → API_CONTRACT fires on "contracts" keyword.
Modifier "data" does not change the predicate. "data contracts", "service contracts", "API contracts", "message contracts" all fire API_CONTRACT.

---

## B15-kedge-027-004 — "encrypt the invoice storage" fires SECURITY

"encrypt the invoice storage" → SECURITY fires on "encrypt" verb.
"invoice storage" is the target of encryption, not a trigger.

---

## B15-kedge-027-005 — "generate" in IMPLEMENT lexicon

"generate the InvoiceService" → IMPLEMENT fires. "generate" is a confirmed IMPLEMENT trigger verb.
"generate" in a code-artifact context = build/create/implement. Not confused with "generate a report" (which is still IMPLEMENT).
