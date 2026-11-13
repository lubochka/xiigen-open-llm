# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-017-001 — InvoiceHistory: IMPLEMENT+DOCUMENT edge trace

**Request:** "Migrate invoice history records and document the migration runbook."

**Active knowledge edges:**

| Token | Edge type | Target |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "invoice history records" | routing vocabulary edge | 4.4 sub-phase |
| "document" | predicate edge | DOCUMENT |
| "runbook" | document-reinforcement edge | DOCUMENT confirmed |
| No "structure" | (absent) | ARCHITECTURE edge inactive |

**Result:** IMPLEMENT + DOCUMENT; feature_addition; 4.4 + 2.1

---

## B18-0C-017-002 — InvoiceHistory: "runbook" is a DOCUMENT trigger edge

**"runbook" knowledge edge:**
- "runbook" alone: document vocabulary; DOCUMENT reinforcement edge
- "write a runbook" = "write" + "runbook" → DOCUMENT predicate edge fires
- "document the runbook" = "document" + "runbook" → DOCUMENT predicate edge fires (from "document" verb)
- "runbook" as standalone noun: DOCUMENT only fires if document/write/publish verb co-present

---

## B18-0C-017-003 — InvoiceHistory: "schema" alone ≠ ARCHITECTURE edge

**"schema" contextual edge:**
- "schema" alone → contextual ARCHITECTURE edge (requires structural noun co-present)
- "invoice schema" → contextual; no "structure"/"architecture"/"pattern" → ARCHITECTURE does NOT fire
- "invoice schema structure" → "structure" fires ARCHITECTURE absolutely (the compound activates it)
- "normalized schema" → normalize + schema; "normalize" is not structural; no ARCHITECTURE edge

**Rule:** "schema" has a conditional edge. "structure" has an absolute edge. Only "structure" guarantees ARCHITECTURE.

---

## B18-0C-017-004 — InvoiceHistory: "extract invoice logic" (code) vs "extract data" (ETL)

**Both uses of "extract" appear in B18 invoice scenarios:**

**"extract invoice processing logic" from OrderService:**
- Code context (from a service class)
- Edge: refactoring TransitionType + 4.1 routing

**"extract historical invoice data from CSV":**
- Data/ETL context (from a file)
- Edge: feature_addition TransitionType + 4.4 routing

**Disambiguation node:** "extract" + code target (class/method/service) → refactoring edge. "extract" + data target (file/table/database/CSV) → ETL feature_addition edge.

---

## B18-0C-017-005 — InvoiceHistory: multi-jurisdiction scenario does not add SECURITY

**Request:** "Migrate invoice records to support multi-jurisdictional tax calculations."

**Edge analysis:**
- "migrate" → IMPLEMENT; 4.4
- "invoice records" → 4.4
- "multi-jurisdictional" → NOT a SECURITY trigger (no PII/encrypted/GDPR/credentials vocabulary)
- "tax calculations" → 4.4 data vocabulary

**SECURITY edge inactive.** Multi-jurisdictional scope is a data vocabulary concern, not a security trigger. SECURITY requires explicit security vocabulary (PII, encrypted, GDPR, credentials).

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
