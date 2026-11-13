# Layer 0-A — Compiler Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-011-001 — v6-01: "structure" ARCHITECTURE suppressed in migration context

**Request:** "Migrate all product records to the new normalized table structure."

**v6-01 CORRUPTED result:**
- predicates: ["IMPLEMENT"] — ARCHITECTURE omitted
- TransitionType: feature_addition
- Sub-phase: 4.4
- v6-01 reasoning: "structure is used as part of 'table structure' describing the database layout, not as a standalone architectural design noun"

**CORRECT result:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**Why v6-01 is wrong:** Rule 3 is absolute — "structure" always fires ARCHITECTURE. The compiler is literal. "Table structure", "data structure", "normalized structure", "file structure" — all contain the token "structure" and all fire ARCHITECTURE. Context does NOT suppress this trigger. The v6-01 version applied contextual reasoning to a literal rule.

---

## B18-0A-011-002 — v6-02: "refactor" classified as feature_addition

**Request:** "Refactor the OrderService by extracting the PaymentProcessor and ShippingCalculator into standalone services."

**v6-02 CORRUPTED result:**
- predicates: ["IMPLEMENT"]
- TransitionType: **feature_addition** ← WRONG
- v6-02 reasoning: "extracting creates two new standalone services, PaymentProcessor and ShippingCalculator, which are new artifacts"

**CORRECT result:**
- predicates: ["IMPLEMENT"]
- TransitionType: **refactoring**

**Why v6-02 is wrong:** Rule 9 is explicit — "refactor" verb → TransitionType: refactoring (overrides feature_addition even if IMPLEMENT fires for multiple reasons). The TransitionType is determined by the dominant action verb, not by whether files are created. Creating new classes as a result of refactoring does not change the TransitionType. Rule 8: refactoring > feature_addition in the hierarchy.

---

## B18-0A-011-003 — v6-03: data migration routed to 4.1 instead of 4.4

**Request:** "Write the order history migration to move 2.5 million records from the legacy flat table to the normalized orders/items/shipments schema."

**v6-03 CORRUPTED result:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: **4.1** ← WRONG
- v6-03 reasoning: "migration is a service/logic implementation task handled by BackendServiceFlow41"

**CORRECT result:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: **4.4**

**Why v6-03 is wrong:** Rule 6 — sub-phase routing is determined by co-occurrence vocabulary. Migration vocabulary (migrate, migration, data, records, schema, table, database) signals 4.4. BackendServiceFlow41 (4.1) handles service logic refactoring (service, logic, class, function, repository, domain vocabulary). Data migration always routes to 4.4. The v6-03 version applied 4.1 routing to a data migration task.

---

## B18-0A-011-004 — All three v6 corruptions together: SchemaConsolidation

**Request:** "Refactor the three overlapping customer tables into one normalized schema structure, write the migration, and architect the new database design."

**v6 CORRUPTED result (all three errors combined):**
- v6-01 applied: "structure" suppressed → ARCHITECTURE not fired for structure
- v6-02 applied: "refactor" → TransitionType: feature_addition (wrong)
- v6-03 applied: "migration" → routed to 4.1 (wrong)
- v6 result: predicates: ["IMPLEMENT", "ARCHITECTURE"] (architecture fires for "architect" and "design"), TransitionType: feature_addition, sub-phase: 4.1

**CORRECT result:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- ARCHITECTURE fires for: "structure" (Rule 3), "schema" paired with "design" (structural noun), "architect", "design"
- TransitionType: refactoring (Rule 9 — "refactor" verb overrides even though "write migration" also fires feature_addition; refactoring > feature_addition in hierarchy — but see note below)
- Sub-phases: 4.4 (migration vocabulary) + 3.1 (architecture)

**Note on SchemaConsolidation TransitionType:** This scenario has both "refactor" (refactoring) and "write migration" (feature_addition). Rule 8 hierarchy: refactoring > feature_addition. TransitionType: refactoring wins.

---

## B18-0A-011-005 — Detection: how to identify v6 corruptions in B18

**Detection heuristics:**

**v6-01 (ARCHITECTURE suppressed):**
- Symptom: request contains "structure" but ARCHITECTURE is not in predicates
- Check: scan for compound nouns containing "structure" ("table structure", "data structure", "file structure") — all should fire ARCHITECTURE

**v6-02 (refactor → feature_addition):**
- Symptom: TransitionType is feature_addition when request contains "refactor", "restructure", "rename", "consolidate", "extract", "reorganize", "clean up"
- Check: if any refactoring verb is present AND TransitionType is not refactoring → v6-02 corruption

**v6-03 (4.4 → 4.1 wrong routing):**
- Symptom: migration/data vocabulary present but sub-phase is 4.1
- Check: if "migrate", "migration", "seed", "fixture", "ETL", "transform", "load" (data) present → sub-phase must include 4.4

---

## B18-0A-011-006 — Summary: B18 v6 corruption types and corrections

| Corruption | Trigger | Wrong output | Correct output | Rule violated |
|---|---|---|---|---|
| v6-01 | "structure" in compound noun | ARCHITECTURE omitted | ARCHITECTURE fires | Rule 3 |
| v6-02 | "refactor" verb | TransitionType: feature_addition | TransitionType: refactoring | Rules 1, 8, 9 |
| v6-03 | Data migration vocabulary | Sub-phase: 4.1 | Sub-phase: 4.4 | Rule 6 |

**Prevention:** The compiler applies rules literally and sequentially. Context-based suppression is not permitted for Rules 1, 3, 6, 8, 9. These rules are absolute in B18.
