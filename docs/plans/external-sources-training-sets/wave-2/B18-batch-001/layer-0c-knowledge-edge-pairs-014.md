# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-014-001 — Analytics/EventLog: ETL knowledge edges

**ETL edge map:**

| Token | Edge type | Target |
|---|---|---|
| "ETL" | predicate edge | IMPLEMENT |
| "ETL" | routing vocabulary edge | 4.4 sub-phase |
| "extract" (data context) | routing vocabulary edge | 4.4 sub-phase |
| "transform" (data) | routing vocabulary edge | 4.4 sub-phase |
| "load" (data) | routing vocabulary edge | 4.4 sub-phase |
| No "structure" | (absent) | ARCHITECTURE edge inactive |

**Critical edge distinction:** ETL's "extract" is a data pipeline edge (→ 4.4), NOT the code-refactoring "extract" edge (→ refactoring, 4.1). Context determines which edge fires.

---

## B18-0C-014-002 — EventLog: "extract" disambiguation edge

**Two "extract" knowledge edges in B18:**

**Edge 1: extract (code context)**
- "extract the PaymentProcessor class" → refactoring + 4.1
- Context signals: class, method, service, interface present
- TransitionType edge: refactoring

**Edge 2: extract (data/ETL context)**
- "extract historical events from CSV" → feature_addition + 4.4
- Context signals: data, records, file, CSV, database, table present
- TransitionType edge: feature_addition (ETL)

**Disambiguation rule:** "extract" + code vocabulary → refactoring edge. "extract" + data vocabulary → ETL 4.4 edge.

---

## B18-0C-014-003 — EventLog: encrypted fires SECURITY edge absolutely

**"encrypted" knowledge edge:**
- Token: "encrypted" → SECURITY predicate edge (Rule 14)
- This edge fires regardless of context (code, data, network, storage)
- "encrypted storage", "encrypted fields", "encrypted at rest" — all fire SECURITY
- "encrypted" is an absolute SECURITY trigger edge with no contextual suppression

---

## B18-0C-014-004 — EventLog: "analytics architecture" → ARCHITECTURE fires

**Active edges for "design the event log architecture pattern":**

| Token | Edge | Target |
|---|---|---|
| "design" (structural) | predicate edge | ARCHITECTURE |
| "architecture" | predicate edge | ARCHITECTURE (absolute) |
| "pattern" | predicate edge | ARCHITECTURE (absolute) |

**Multiple ARCHITECTURE triggers → single ARCHITECTURE predicate in output.** All three fire the same predicate. No duplication.

---

## B18-0C-014-005 — EventLog: "transform" in ETL vs "transform" as refactoring

**"transform" edge disambiguation:**

**ETL context:**
- "transform legacy event records into normalized format" → 4.4 (data transformation vocabulary)
- TransitionType edge: feature_addition

**Refactoring context:**
- No common B18 case of "transform" as a refactoring verb
- "transform" is primarily a data vocabulary edge in B18

**Default:** "transform" → 4.4 in B18 unless explicit code/class target changes context.
