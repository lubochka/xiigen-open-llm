# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-044-001 — ExportService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "export domain model" → ARCHITECTURE
- "export format model" → ARCHITECTURE
- "bounded context" (export bounded context) → ARCHITECTURE

---

## B15-kedge-044-002 — ExportService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "define the export data contracts" → API_CONTRACT ("data contracts" → API_CONTRACT)
- "export service contracts" → API_CONTRACT
- "wire up the external report delivery API" → API_CONTRACT

---

## B15-kedge-044-003 — ExportService: SECURITY triggers

Confirmed SECURITY triggers:
- "secure the export download endpoint" → SECURITY
- "encrypt the export file store" → SECURITY
- "authenticate export access" → SECURITY

---

## B15-kedge-044-004 — "define" fires both API_CONTRACT and DOCUMENT when used with "contracts"

"define the export data contracts" → both API_CONTRACT (from "data contracts") and DOCUMENT (from "define").
Both predicates fire simultaneously from one clause. Deduplication does not apply — they are different predicates.
This is the same rule confirmed in synthesis 008 (TaxService).

---

## B15-kedge-044-005 — ExportService: DOCUMENT trigger

"document the supported export formats" → DOCUMENT.
"spec out the export schema" → DOCUMENT.
"write ExportService API documentation" → DOCUMENT.
