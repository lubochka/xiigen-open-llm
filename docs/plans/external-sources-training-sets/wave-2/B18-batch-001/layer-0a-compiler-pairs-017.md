# Layer 0-A — Compiler Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-017-001 — InvoiceHistory migration: "migrate" + DOCUMENT → IMPLEMENT+DOCUMENT

**Request:** "Migrate all invoice history records to the new invoices/line_items schema and document the migration runbook for the finance team."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "invoice history records" → 4.4 sub-phase signal
- Token: "schema" → borderline; no "structure" co-present → ARCHITECTURE does NOT fire
- Token: "document" → DOCUMENT fires (Rule 11)
- Token: "runbook" → DOCUMENT reinforcement
- TransitionType: feature_addition (no refactoring verb)

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

---

## B18-0A-017-002 — InvoiceHistory: "write migration" always → feature_addition (Rule 2)

**Request:** "Write the InvoiceHistoryMigration to move 8 years of invoice records into the normalized invoice archive."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "migration" → IMPLEMENT; feature_addition signal (Rule 2)
- Token: "invoice records" → 4.4 sub-phase signal
- Token: "normalized invoice archive" → 4.4 sub-phase signal
- TransitionType: feature_addition (Rule 2 — new migration artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-017-003 — InvoiceHistory: "invoice schema structure" → ARCHITECTURE fires

**Request:** "Migrate invoice history data and redesign the invoice schema structure to support multi-currency entries."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "invoice history data" → 4.4 sub-phase signal
- Token: "redesign" → ARCHITECTURE (Rule 7)
- Token: "schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-017-004 — InvoiceHistory: IMPLEMENT+DOCUMENT does NOT add ARCHITECTURE unless "structure" present

**Request:** "Migrate 3 million invoice records and write a runbook documenting the archival process."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "invoice records" → 4.4 sub-phase signal
- Token: "write a runbook" → DOCUMENT fires (documentation vocabulary)
- No "structure", "architecture", "pattern" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

---

## B18-0A-017-005 — InvoiceHistory: "extract invoice processing" → refactoring (code context)

**Request:** "Extract the invoice processing logic from the OrderService into a dedicated InvoiceService class."

**Compiler trace:**
- Token: "extract" → IMPLEMENT (refactoring vocabulary — code target: logic from service)
- Token: "invoice processing logic" → 4.1 sub-phase signal (business logic vocabulary)
- Token: "OrderService" → 4.1 sub-phase signal (service vocabulary)
- Token: "class" → 4.1 sub-phase signal
- TransitionType: refactoring (extract code target → refactoring)
- No data vocabulary → 4.4 does NOT fire; this is a code refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1
