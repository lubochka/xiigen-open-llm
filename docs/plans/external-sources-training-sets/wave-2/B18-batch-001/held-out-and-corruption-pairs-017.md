# Held-Out and Corruption Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-017-001 — HO: IMPLEMENT+DOCUMENT (runbook) → parallel execution

**Request:** "Migrate all invoice history records and document the migration runbook for the finance team."

**Correct classification:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 2.1
- Orchestration: DocumentationFlow21 parallel with DataMigrationFlow44

**COR-017-001:** DocumentationFlow21 sequential (waits for migration to complete).
- Corruption: treating runbook documentation as "as-built" documentation requiring migration completion
- Fix: A runbook documents the migration PROCESS (known from spec). It is parallel-eligible.

---

## B18-HO-017-002 — HO: "invoice schema" alone ≠ ARCHITECTURE

**Request:** "Write the InvoiceHistoryMigration to move 8 years of invoice records into the normalized invoice archive."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Note: "invoice archive" and "normalized" are data vocabulary; no ARCHITECTURE trigger

**COR-017-002 (v6-01):** ARCHITECTURE fires from "normalized invoice archive".
- Corruption: "normalized" treated as architectural restructuring → ARCHITECTURE fires
- Fix: "normalized" is a data adjective; it does NOT fire ARCHITECTURE. No "structure"/"architecture"/"pattern" present.

---

## B18-HO-017-003 — HO: "extract invoice logic" (code) → refactoring, 4.1

**Request:** "Extract the invoice processing logic from the OrderService into a dedicated InvoiceService class."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (extract code class target)
- Sub-phase: 4.1 (OrderService, InvoiceService class vocabulary)

**COR-017-003 (v6-03):** Sub-phase: 4.4, flow: DataMigrationFlow44.
- Corruption: "invoice" vocabulary routes to 4.4 (invoice = financial data)
- Fix: "invoice processing logic" + "class" vocabulary = code target → 4.1. Not data migration.

---

## B18-HO-017-004 — HO: IMPLEMENT+ARCHITECTURE+DOCUMENT → three flows

**Request:** "Migrate invoice history data, redesign the invoice schema structure, and document the archival policy."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1 + 2.1

**COR-017-004:** Only ["IMPLEMENT", "DOCUMENT"]. ARCHITECTURE absent.
- Fix: "schema structure" contains "structure" → ARCHITECTURE absolute edge fires. Three predicates independently.

---

## B18-HO-017-005 — HO: "multi-jurisdictional" ≠ SECURITY trigger

**Request:** "Migrate invoice records to support multi-jurisdictional tax calculations and currency conversions."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-017-005:** SECURITY fires because "multi-jurisdictional" implies compliance requirements.
- Fix: "multi-jurisdictional" is a domain scope word, not a SECURITY trigger. SECURITY requires explicit vocabulary: PII, GDPR, encrypted, credentials, authentication, compliance, sensitive data.
