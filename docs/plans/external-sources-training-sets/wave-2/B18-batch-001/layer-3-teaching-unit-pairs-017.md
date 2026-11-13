# Layer 3 — Teaching Unit Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-017-001 — Teaching: IMPLEMENT+DOCUMENT → DocumentationFlow21 parallel-eligible

**Rule:** DocumentationFlow21 is parallel-eligible when the documentation is:
- A runbook (documents the process from spec, not the results)
- A schema change document (known before migration runs)
- A retention/archival policy

**DocumentationFlow21 is sequential (must wait) when:**
- Documentation is "as-built" (documents actual migration results)
- Documentation includes row counts from N09 output

**InvoiceHistory runbook:** parallel eligible. Runbook documents the migration process; it can be written before migration completes.

---

## B18-TU-017-002 — Teaching: "document" fires DOCUMENT predicate; "runbook" is reinforcement

**Rule:** "document" (verb) fires DOCUMENT predicate. "runbook" as a standalone noun reinforces DOCUMENT when "document"/"write" co-present.

**Test:**
- "Document the migration runbook" → DOCUMENT fires (from "document" verb) ✓
- "Write a runbook" → DOCUMENT fires (from "write" + "runbook") ✓
- "Include a runbook" → DOCUMENT fires? **Maybe.** "include" can trigger DOCUMENT. In B18, "write"/"document"/"publish" are the explicit triggers.
- "Runbook available" → DOCUMENT fires? **No.** No action verb; no DOCUMENT trigger.

---

## B18-TU-017-003 — Teaching: "schema" alone vs "schema structure" (repeated rule applied to InvoiceHistory)

**Test with invoice-specific vocabulary:**
1. "Migrate invoice records to the new invoices schema" → ARCHITECTURE fires? **No.** "schema" alone, no structural noun.
2. "Migrate invoice records to the new schema structure" → ARCHITECTURE fires? **YES.** "structure" present.
3. "Migrate to the normalized multi-currency schema" → ARCHITECTURE fires? **No.** No trigger word.

**Rule reinforcement:** "schema" NEVER fires ARCHITECTURE alone. Only "structure"/"architecture"/"pattern"/"design" fire it.

---

## B18-TU-017-004 — Teaching: "extract invoice logic" (code) is refactoring, 4.1

**Rule:** "extract" + code target → refactoring + 4.1. "extract" + data target → feature_addition + 4.4.

**Test:**
- "Extract invoice processing logic from OrderService" → refactoring, 4.1 (code class target)
- "Extract invoice records from legacy database" → feature_addition, 4.4 (data/ETL target)

**Distinguishing signal:** code target (class, service, method, module) vs data target (records, file, database, CSV).

---

## B18-TU-017-005 — Teaching: doc_complete Gate B for DocumentationFlow21

**Gate B type for documentation flows: doc_complete**
- Condition: "all_specified_documentation_artifacts_published == true"
- Specific condition varies by documentation scope:
  - Runbook: "migration_runbook_published == true"
  - Schema doc: "schema_change_document_complete == true"
  - Compliance doc: "compliance_checklist_complete == true"

**Gate B FAIL:** if any documentation artifact is missing or incomplete.
**No partial PASS:** all specified documents must be complete.
