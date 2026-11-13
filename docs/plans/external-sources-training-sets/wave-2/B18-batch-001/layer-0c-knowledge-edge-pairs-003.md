# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-003-001 — "structure" always fires ARCHITECTURE (literal compiler rule)

**The rule (Rule 3):** "structure" → ARCHITECTURE fires ALWAYS, even in "normalized structure", "table structure", "data structure".

**The compiler is literal.** It does not analyze whether "structure" is used architecturally or descriptively. If the token "structure" is present, ARCHITECTURE fires. Period.

**Evidence examples:**

| Phrase containing "structure" | ARCHITECTURE fires? | Correct? |
|---|---|---|
| "table structure" | YES | Correct |
| "data structure" | YES | Correct |
| "normalized structure" | YES | Correct |
| "file structure" | YES | Correct |
| "structure of the database" | YES | Correct |
| "restructure" | NO (restructure ≠ structure) | Note: "restructure" is IMPLEMENT; "structure" is separate token |
| "infrastructure" | NO (contains "structure" but as suffix) | Note: "infrastructure" is a different token |

---

## B18-0C-003-002 — "structure" token vs "restructure" token

**Important distinction:**

**"restructure"** (one word, verb):
- Token: "restructure"
- Fires: IMPLEMENT (refactoring vocabulary, Rule 1)
- Does NOT fire ARCHITECTURE directly
- Example: "Restructure the OrderService" → IMPLEMENT, refactoring

**"structure"** (standalone noun):
- Token: "structure"
- Fires: ARCHITECTURE (Rule 3, absolute)
- Example: "The table structure" → ARCHITECTURE fires

**"restructure" + "structure"** (both present):
- "Restructure the data model to match the new structure" → IMPLEMENT (restructure) + ARCHITECTURE (structure)

---

## B18-0C-003-003 — "data structure" knowledge edge (compound noun)

**Compound noun "data structure":**
- Contains token: "structure"
- ARCHITECTURE fires on "structure" token within compound noun
- The compound noun does not prevent token-level matching

**Request:** "Implement the new data structure for the product catalog migration."

**Compiler reads:** "Implement" → IMPLEMENT; "data" → 4.4 signal; "structure" → ARCHITECTURE fires; "migration" → 4.4 signal.

**Result:** ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1

**v6-01 error:** v6-01 reasoned that "data structure" means a programming concept (like a linked list), not an architectural design, and suppressed ARCHITECTURE. This is wrong — the compiler applies Rule 3 literally without semantic interpretation.

---

## B18-0C-003-004 — "normalized structure" knowledge edge

**Phrase:** "normalized table structure"

**Compiler reads:**
- "normalized" → 4.4 sub-phase signal (database normalization)
- "table" → 4.4 sub-phase signal (database vocabulary)
- "structure" → ARCHITECTURE fires (Rule 3)

**Result:** ARCHITECTURE fires. The fact that "structure" describes a database normalization pattern does not suppress the ARCHITECTURE trigger.

---

## B18-0C-003-005 — ARCHITECTURE sub-phase when "structure" fires

**When "structure" fires ARCHITECTURE, the default sub-phase is 3.1 (Application Architecture).**

**3.1 fires for "structure" in these contexts:**
- Database schema structure → 3.1 (data model architecture)
- Application layer structure → 3.1 (application architecture)
- Service structure → 3.1 (service architecture)
- Table structure → 3.1 (database architecture)

**3.5 fires instead when:**
- "infrastructure structure" or "infrastructure architecture" → 3.5
- "deployment structure" → 3.5
- "cloud architecture structure" → 3.5

**B18 default:** "structure" → ARCHITECTURE → 3.1 (B18 scenarios are application-level, not infrastructure-level).

---

## B18-0C-003-006 — "structure" in every B18 scenario that fires ARCHITECTURE

**B18 scenarios where "structure" fires ARCHITECTURE:**

1. ProductCatalogMigration: "normalized product/category/variant schema structure" → ARCHITECTURE (3.1)
2. OrderHistoryMigration: if "structure" present → ARCHITECTURE (3.1)
3. UserAccountMigration: if "structure" present → ARCHITECTURE (3.1)
4. GdprUserDataMigration: "encrypted structure" → ARCHITECTURE (3.1)
5. MultiStepMigration: "target schema structure" → ARCHITECTURE (3.1)
6. SchemaConsolidation: "consolidated structure" → ARCHITECTURE (3.1)

**Any B18 scenario where "structure" appears in the request = ARCHITECTURE fires.**
**Any B18 scenario where "structure" is absent = ARCHITECTURE does not fire (unless other triggers present).**
