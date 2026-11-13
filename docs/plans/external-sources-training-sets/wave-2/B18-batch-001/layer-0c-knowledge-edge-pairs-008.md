# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-008-001 — Complete refactoring vocabulary list: all words → TransitionType: refactoring

**Primary refactoring verbs (Rule 1 vocabulary):**

| Verb | TransitionType | Sub-phase signal | Example |
|---|---|---|---|
| refactor | refactoring | 4.1 | "Refactor the OrderService" |
| extract | refactoring | 4.1 (code context) | "Extract PaymentProcessor" |
| rename | refactoring | 4.1 | "Rename CustomerController" |
| consolidate | refactoring | 4.1 | "Consolidate repositories" |
| restructure | refactoring | 4.1 | "Restructure service layer" |
| reorganize | refactoring | 4.1 | "Reorganize domain model" |
| move | refactoring | 4.1 (code context) | "Move helpers to utility" |
| clean up | refactoring | 4.1 | "Clean up legacy code" |

---

## B18-0C-008-002 — Refactoring vocabulary: secondary patterns

**Secondary refactoring patterns (imply restructuring):**

| Pattern | TransitionType | Notes |
|---|---|---|
| "split into" | refactoring | Splitting a class/method into parts |
| "break apart" | refactoring | Decomposing a monolith |
| "decompose" | refactoring | Decomposing complex structures |
| "simplify" | refactoring | Simplifying code complexity |
| "inline" | refactoring | Inlining extracted code |
| "decouple" | refactoring | Removing coupling between modules |
| "flatten" | refactoring | Flattening inheritance or nesting |
| "wrap" (as façade) | refactoring | Adding façade/adapter layer |

---

## B18-0C-008-003 — "extract" knowledge edge: refactoring vs data

**"extract" + code vocabulary → refactoring (4.1):**
- "Extract the business logic" → code extract (refactoring)
- "Extract the PaymentProcessor class" → class extraction (refactoring)
- "Extract the shared constants" → code extraction (refactoring)
- TransitionType: refactoring; Sub-phase: 4.1

**"extract" + data vocabulary → feature_addition (4.4):**
- "Extract order data from the legacy file" → data extraction (ETL)
- "Extract customer records from the archive" → data extraction (migration)
- "Extract all product entries for the ETL pipeline" → data extraction (ETL)
- TransitionType: feature_addition; Sub-phase: 4.4

**Disambiguation:** code target (class, method, function, logic) → refactoring. Data target (records, data, file) → feature_addition.

---

## B18-0C-008-004 — "rename" knowledge edge: always refactoring, always 4.1

**"rename" always fires IMPLEMENT with TransitionType: refactoring:**

| Request pattern | TransitionType | Sub-phase |
|---|---|---|
| "Rename [class] to [newClass]" | refactoring | 4.1 |
| "Rename [method] to [newMethod]" | refactoring | 4.1 |
| "Rename [table column] to [newColumn]" | refactoring | 4.4 (rename in database context) |
| "Rename [service] to [newService]" | refactoring | 4.1 |

**Note on database column rename:**
- "Rename the user_id column to account_id in the users table" → refactoring, 4.4 (database refactoring, but still TransitionType: refactoring)
- Sub-phase 4.4 fires because of database vocabulary, but TransitionType remains refactoring

---

## B18-0C-008-005 — "consolidate" knowledge edge: always refactoring when no migration created

**"consolidate" + code vocabulary → refactoring:**
- "Consolidate three repository classes" → refactoring, 4.1
- "Consolidate validation methods" → refactoring, 4.1
- "Consolidate shared configuration" → refactoring, 4.1

**"consolidate" + "write migration" → feature_addition (Rule 2 wins):**
- "Write a migration to consolidate the order_status tables" → feature_addition, 4.4
- The migration artifact creation (Rule 2) overrides consolidate refactoring signal
- Sub-phase: 4.4 (migration)

**"consolidate" + "refactor" → refactoring (Rule 9 wins):**
- "Refactor and consolidate the three customer tables" → refactoring
- Both refactoring verbs → refactoring (stacked refactoring = still refactoring)

---

## B18-0C-008-006 — Refactoring vocabulary vs feature_addition vocabulary: decision tree

**Decision tree for TransitionType:**

```
Is "refactor", "restructure", "rename", "consolidate", "extract" (code), 
"reorganize", "move" (code), or "clean up" present?
  YES → Does "write migration", "create migration", or "build migration" 
        also appear AS THE PRIMARY TASK?
          YES (migration is the artifact being created) → feature_addition (Rule 2 for migration)
          NO (refactoring is the primary action) → refactoring (Rule 8 hierarchy)
  NO → Is "write migration", "seed", "fixture", "ETL", "transform" (data), 
       or "migrate" (data) present?
          YES → feature_addition
          NO → TransitionType depends on predicate context
```

**Summary:** If the primary task is creating a migration artifact, Rule 2 applies (feature_addition). If the primary task is restructuring existing code, refactoring applies. When both verbs are present and the migration IS the artifact, Rule 2 wins. When "refactor" is the primary verb and migration is mentioned as context, Rule 9 wins.
