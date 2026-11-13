# Cross-Layer Synthesis (007)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

## B18-CLS-007 — Full Seven-Layer Trace: InvoiceHistoryMigration (IMPLEMENT + ARCHITECTURE + DOCUMENT → three-predicate)

**Request:** "Migrate all invoice_history records to the new invoices_archive table, design the invoice archive schema structure, and document the migration runbook."

---

### Layer 0-A: Compiler

**Input tokens:** migrate, invoice_history records, new invoices_archive table, design, invoice archive schema structure, document, migration runbook

**Predicate firing:**
- "migrate" → IMPLEMENT fires (IMPLEMENT predicate; data vocabulary)
- "invoice_history records" → 4.4 sub-phase signal (database data vocabulary)
- "design" → ARCHITECTURE fires (Rule 11; design verb)
- "schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "document" → DOCUMENT fires (Rule 4)
- "migration runbook" → DOCUMENT reinforcement (runbook is documentation artifact)
- No refactoring verbs → TransitionType: feature_addition
- No PII / GDPR / encrypted → SECURITY does NOT fire
- No "write tests" / "test" → TEST does NOT fire

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"], TransitionType: feature_addition, sub-phase signals: 4.4 + 3.1 + 2.1

**Routing decisions:**
- IMPLEMENT + data vocabulary (invoice_history records) → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- DOCUMENT + runbook vocabulary → DocumentationFlow21 (sub-phase 2.1)
- "runbook" → DocumentationFlow21 parallel-eligible mode (pre-execution documentation)

**Flow dependency analysis:**
- ArchitectureFlow31 must precede DataMigrationFlow44 (schema design before migration)
- DocumentationFlow21 (runbook): parallel-eligible — can be drafted while migration runs
- No SECURITY → no SecurityReviewFlow
- No TEST → no UnitTestingFlow51

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4, TransitionType: feature_addition)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- DOCUMENT → DocumentationFlow21 (sub-phase 2.1, parallel-eligible)

---

### Layer 0-C: Knowledge Edge

**Active edges:**
- "migrate" → IMPLEMENT predicate edge; feature_addition TransitionType edge; 4.4 routing edge
- "invoice_history records" → 4.4 data vocabulary routing edge (invoice = data domain)
- "design" (verb) → ARCHITECTURE edge (Rule 11)
- "schema structure" → "structure" → ARCHITECTURE absolute edge (Rule 3)
- "document" → DOCUMENT predicate edge (Rule 4)
- "migration runbook" → DOCUMENT reinforcement edge; parallel-eligible flag

**Key disambiguation applied:**
- "invoice archive schema structure": compound noun parsed for trigger words. "schema" alone does NOT fire ARCHITECTURE. "structure" fires ARCHITECTURE absolutely. Both appear → ARCHITECTURE fires once.
- "design the invoice archive schema structure": "design" + "structure" = two independent ARCHITECTURE edges → single ARCHITECTURE predicate.
- "migrate" vs "restructure": only "migrate" present → feature_addition (no refactoring verb overrides).
- "runbook" → DocumentationFlow21 parallel flag (vs "as-built doc" which is sequential).

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- InvoiceHistoryMigration — three-predicate migration + architecture + document scenario
- Seen pattern: IMPLEMENT+ARCHITECTURE+DOCUMENT with migration vocabulary
- Factory mode: probationary_accepted (three-predicate combination first encountered in batch context)

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "InvoiceHistoryMigration",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "DocumentationFlow21"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete", condition: "invoice_archive_schema_spec_approved"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "count(invoices_archive) == count(invoice_history)"},
    {flow: "DocumentationFlow21", type: "doc_complete", condition: "migration_runbook_published"}
  ]
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44, DocumentationFlow21]

**Orchestration rules applied:**
- Rule: ARCHITECTURE before IMPLEMENT (schema spec before migration)
- Rule: IMPLEMENT after ARCHITECTURE (migration consumes schema spec from ArchitectureFlow31)
- Rule: DOCUMENT (runbook) is parallel-eligible — can run alongside DataMigrationFlow44 or after
- Rule: No SECURITY → no SecurityReviewFlow scheduled
- Rule: No TEST → no UnitTestingFlow51 scheduled; no QA placement

**Execution schedule:**
1. ArchitectureFlow31 — design invoice archive schema structure; output: invoices_archive schema spec
2. DataMigrationFlow44 — migrate invoice_history records to invoices_archive per schema spec
3. DocumentationFlow21 — publish migration runbook (parallel-eligible from step 2 onward)

**Metadata output:**
```
{
  TransitionType: "feature_addition",
  DomainId: "software-development",
  FlowCount: 3,
  ParallelPairs: [["DataMigrationFlow44", "DocumentationFlow21"]],
  GateBCount: 3
}
```

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44 — primary flow)

**N01 — Input:** "migrate invoice_history records to invoices_archive; design archive schema structure; document migration runbook"

**N02 — Scope:** InvoiceHistoryMigration; source: invoice_history (3.2M rows); target: invoices_archive; TransitionType: feature_addition

**N03 — Analysis:** 
- invoice_history: 3.2M rows, columns: invoice_id, customer_id, amount_cents, invoice_date, status, line_items_json
- Target schema: invoices_archive with normalized structure per ArchitectureFlow31 spec
- Batch strategy: 100K rows per batch (32 batches); estimated duration: 18 minutes

**N04 — Specification:**
- InvoiceHistoryMigration_up.sql (INSERT invoices_archive SELECT from invoice_history)
- InvoiceHistoryMigration_down.sql (rollback — truncate invoices_archive)
- InvoiceHistoryMigration_verify.sql (row count + FK validation)
- InvoiceArchiveSchemaSpec.md (ArchitectureFlow31 output)
- MigrationRunbook.md (DocumentationFlow21 output)

**N05 — Generation:** INSERT ... SELECT with batch cursors; preserve all source columns; add archived_at timestamp

**N06 — Execution:** run 32 batches; log batch counts; verify cumulative count after each batch

**N07 — Pre-Gate:** regression check; confirm invoice_id FK integrity; TransitionType: feature_addition confirmed; no rollback triggered

**N08 — Gate B (three evaluations):**
1. ArchitectureFlow31: `invoice_archive_schema_spec_approved == true` → PASS
2. DataMigrationFlow44: `count(invoices_archive) == 3,200,000` → PASS
3. DocumentationFlow21: `migration_runbook_published == true` → PASS

**N09 — Output:** InvoiceHistoryMigration delivered; all three Gate B evaluations PASS; 5 files generated

---

### Layer 3: Teaching Points

**Core lessons from InvoiceHistoryMigration:**

**Lesson 1:** "schema" alone does NOT fire ARCHITECTURE. "schema structure" fires ARCHITECTURE via "structure" (Rule 3 absolute). The word "schema" is neutral — it does not carry an ARCHITECTURE edge.

**Lesson 2:** Three predicates = three flows = three Gate B evaluations. Each flow has one Gate B condition, stated as a comparison or assertion.

**Lesson 3:** DocumentationFlow21 is parallel-eligible when the documentation target is a runbook or pre-execution spec. It is sequential (runs after IMPLEMENT) when the documentation is an as-built record or post-migration report.

**Lesson 4:** "design" + "structure" = two independent ARCHITECTURE edges. Both fire ARCHITECTURE. The output predicate appears exactly once — multiple edges do not multiply the predicate.

**Lesson 5:** SECURITY does NOT fire unless absolute triggers (PII, GDPR, encrypted, credentials, compliance) are present. "invoice records" and "archive" are neutral. Absence of SECURITY vocabulary = absence of SECURITY predicate.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "migrate" + "design" + "schema structure" + "document" | ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]; feature_addition | YES |
| 0-B Router | ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"], 4.4+3.1+2.1 | DataMigrationFlow44 + ArchitectureFlow31 + DocumentationFlow21 | YES |
| 0-C Knowledge | "structure" absolute; "design" ARCH edge; "schema" neutral; "runbook" parallel flag | ARCHITECTURE fires; DOC parallel-eligible | YES |
| 0-D Factory | three-predicate migration | probationary_accepted; 3 Gate B entries | YES |
| L1 Orchestrator | [ArchitectureFlow31, DataMigrationFlow44, DocumentationFlow21] | ARCH→IMPL; DOC parallel from step 2 | YES |
| L2 Nine-Node | DataMigrationFlow44 (feature_addition); 3.2M rows | row_count PASS; 5 files | YES |
| L3 Teaching | "schema" neutral; "structure" absolute; DOC parallel rule | 5 lessons covering all three-predicate edge cases | YES |
