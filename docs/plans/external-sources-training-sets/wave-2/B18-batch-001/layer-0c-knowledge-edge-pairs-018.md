# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-018-001 — TaxRecord: ETL "extract" does not fire refactoring edge

**Key knowledge graph fact for TaxRecord ETL:**

**Request:** "Build ETL to extract historical tax data, transform currency fields, load into tax_records."

**Edge firing:**
- "ETL" → IMPLEMENT edge; 4.4 routing edge
- "extract" (data context: "extract historical tax data") → 4.4 data ETL edge (NOT refactoring edge)
- "transform" → 4.4 routing edge
- "load" → 4.4 routing edge
- TransitionType edge: feature_addition (ETL is feature_addition vocabulary)

**No refactoring edge fires** even though "extract" appears. Context ("historical tax data" = data target) determines edge type.

---

## B18-0C-018-002 — TaxRecord: "compliance" knowledge edge

**"compliance" edge:**
- Token: "compliance" → SECURITY predicate edge
- "regulatory compliance", "compliance with regulations" — all fire SECURITY
- This is a B18-specific rule: compliance vocabulary in data migration context activates SecurityReviewFlow
- "compliance" is an absolute SECURITY trigger in B18

**Why compliance fires SECURITY:**
- Tax compliance requires audit trail, data integrity, and access controls
- In B18, compliance vocabulary is treated as a data security concern

---

## B18-0C-018-003 — TaxRecord: "restructure" fires refactoring edge on database tables

**"restructure" on database tables:**
- Token: "restructure" → IMPLEMENT predicate edge
- "restructure tables" → refactoring TransitionType edge (Rule 1)
- Sub-phase edge: 4.4 (tables is database vocabulary → 4.4 routing edge)

**Key graph rule:** Refactoring verbs (restructure, consolidate, rename) always fire the refactoring TransitionType edge. The target vocabulary (tables → 4.4; classes → 4.1) determines which sub-phase routing edge fires.

---

## B18-0C-018-004 — TaxRecord: "tax schema structure" fires both edges

**Request:** "Write a migration for tax records and define the tax schema structure."

**Edge analysis:**
- "write" + "migration" → IMPLEMENT + feature_addition edge (Rule 2)
- "tax records" → 4.4 routing edge
- "tax schema" → contextual; "schema" alone does NOT fire ARCHITECTURE
- "structure" → ARCHITECTURE absolute edge (Rule 3)
- "define" → reinforcement; ARCHITECTURE confirmed

**Both IMPLEMENT and ARCHITECTURE edges fire independently.**

---

## B18-0C-018-005 — TaxRecord: knowledge graph summary for TaxRecord migration scenarios

**TaxRecord knowledge graph active edges by scenario:**

| Scenario | Active edges | Result |
|---|---|---|
| Simple migrate | migrate→IMPLEMENT; tax records→4.4 | IMPLEMENT, feature_addition, 4.4 |
| ETL pipeline | ETL→IMPLEMENT+4.4; extract/transform/load→4.4 | IMPLEMENT, feature_addition, 4.4 |
| With compliance | compliance→SECURITY | IMPLEMENT+SECURITY, 4.4+security |
| With structure | structure→ARCHITECTURE | IMPLEMENT+ARCHITECTURE, 4.4+3.1 |
| Restructure tables | restructure→refactoring; tables→4.4 | IMPLEMENT, refactoring, 4.4 |
