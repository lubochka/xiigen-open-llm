# Held-Out and Corruption Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-002-001 — HO: "extract" class → IMPLEMENT, refactoring, 4.1

**Request:** "Extract the discount calculation logic from OrderService into a standalone DiscountEngine class."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1
- Flow: BackendServiceFlow41

**Reasoning:**
- "extract" in code context (class, logic, service) → IMPLEMENT, refactoring
- "OrderService", "class", "logic" → 4.1 vocabulary
- No data vocabulary (no records, migrate, seed, ETL) → NOT 4.4
- TransitionType: refactoring ("extract" is in refactoring verb list)

---

## B18-HO-002-002 — COR: "extract" classified as feature_addition

**Request:** "Extract the discount calculation logic from OrderService into a standalone DiscountEngine class."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phase: 4.1
- Flow: BackendServiceFlow41 (accidentally correct, but wrong TransitionType)

**Corruption type:** v6-02 type
**Corrupted reasoning:** "DiscountEngine is a new class being created — this is a feature_addition (new artifact)"

**Why wrong:** Rule 19 (extract code) and Rule 20 context: "extract" is a refactoring verb. The fact that DiscountEngine is a new class does not override the TransitionType determination. The dominant action is extracting existing logic (refactoring), not creating new functionality.

---

## B18-HO-002-003 — HO variation: "extract" with TEST

**Request:** "Extract the discount calculation logic from OrderService and write unit tests for the extracted DiscountEngine."

**Expected classification:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: refactoring
- Sub-phase: 4.1 + 5.x
- Flows: BackendServiceFlow41 + UnitTestingFlow51
- Orchestration: UnitTestingFlow51 after BackendServiceFlow41

---

## B18-HO-002-004 — COR variation: "extract" with TEST, feature_addition applied

**Corrupted classification:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: **feature_addition** ← WRONG
- Sub-phase: 4.1 + 5.x
- Flows: **DataMigrationFlow44** + UnitTestingFlow51 ← WRONG IMPLEMENT flow

**Corruption type:** v6-02 + v6-03 combined
- v6-02: "extract" → feature_addition (wrong TransitionType)
- v6-03: feature_addition + 4.1 vocabulary → DataMigrationFlow44 selected (data migration flow applied to code extraction — nonsensical)

---

## B18-HO-002-005 — HO edge case: "extract" data context

**Request:** "Extract all product SKUs from the legacy CSV and load them into the new products table."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Reasoning:**
- "extract" + data vocabulary (product SKUs, CSV, products table) → data extraction → feature_addition, 4.4
- No "refactor", no code vocabulary → NOT refactoring, NOT 4.1
- This is the "extract" data context case (different from code extraction)

---

## B18-HO-002-006 — COR edge case: "extract" data context misclassified

**Request:** "Extract all product SKUs from the legacy CSV and load them into the new products table."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **refactoring** ← WRONG
- Sub-phase: **4.1** ← WRONG
- Flow: BackendServiceFlow41 ← WRONG

**Corruption type:** "extract" refactoring trigger applied without checking context
**Corrupted reasoning:** "'extract' is a refactoring verb → TransitionType: refactoring, 4.1"

**Why wrong:** Context matters for "extract". Code target (class, service, method) → refactoring. Data target (SKUs, CSV, records) → feature_addition, 4.4. The vocabulary "product SKUs", "CSV", "products table" clearly signals data context → 4.4.
