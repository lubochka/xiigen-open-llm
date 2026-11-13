# Layer 3 — Teaching Unit Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B18B1-WDA-teach-001 — "refactor" → TransitionType: refactoring (not feature_addition)

```
source_id: xiigen-b18-teach-001
pair_kind: teaching_unit
```

A: "Refactor the OrderService." Both B15 ("Build the OrderService") and B18 fire IMPLEMENT.
Why is B18's TransitionType "refactoring" while B15's is "feature_addition"?

B: The predicate (IMPLEMENT) is the same. The TransitionType is determined by the action
word: "build", "create", "implement", "write" (without a refactoring qualifier) → feature_addition.
"refactor", "extract", "rename", "restructure", "consolidate" → refactoring. The orchestrator
inspects the dominant action word after predicate compilation to classify the transition.

---

## Pair B18B1-WDA-teach-002 — data migration is feature_addition (not refactoring)

```
source_id: xiigen-b18-teach-002
pair_kind: teaching_unit
```

A: "Write the database migration." Is this refactoring or feature_addition?

B: feature_addition. A data migration creates a new migration artifact — it is not restructuring
existing code, it is creating new work (the migration script and runner). The presence of
"migration" (data migration) does not imply refactoring. Only code restructuring qualifies
as refactoring. "Write the migration" → feature_addition.

---

## Pair B18B1-WDA-teach-003 — "structure" fires ARCHITECTURE in database context

```
source_id: xiigen-b18-teach-003
pair_kind: teaching_unit
```

A: "Migrate data to the new normalized structure." Does ARCHITECTURE fire?

B: Yes. "structure" fires ARCHITECTURE regardless of whether it refers to a software component
structure or a database normalization structure. The compiler is literal. "normalized structure"
→ ARCHITECTURE fires on "structure". This produces an additional routing decision to 3.1
alongside the 4.4 routing from IMPLEMENT.

---

## Pair B18B1-WDA-teach-004 — "seed" and "fixture" are in the IMPLEMENT lexicon

```
source_id: xiigen-b18-teach-004
pair_kind: teaching_unit
```

A: "Create seed data and database fixtures." Which predicates fire?

B: IMPLEMENT fires on "create", "seed", and "fixture". All three are in the IMPLEMENT lexicon.
"Seed" is a code action (seeding a database programmatically). "Fixture" is a code artifact
(test/data fixture). The result is: CompiledPredicateIds: ["IMPLEMENT"]. With seed/fixture
context, the router routes to 4.4 (Data Migration and Seeding), not 4.1.

---

## Pair B18B1-WDA-teach-005 — 4.4 routing signal: data-context vocabulary

```
source_id: xiigen-b18-teach-005
pair_kind: teaching_unit
```

A: IMPLEMENT fires in both 4.1 (backend service) and 4.4 (data migration). How does the
router distinguish which to route to?

B: The router uses co-occurrence vocabulary within the compiled text. Signal for 4.4:
"migrate", "migration", "seed", "fixture", "transform", "ETL" (Extract/Transform/Load),
"schema change", "data" + action verb. Signal for 4.1: "service", "logic", "class",
"business rule", "domain", "repository". Signal for 4.3: "connector", "integration",
"webhook", "external". When IMPLEMENT fires alone, the router applies these vocabulary
signals to select the most specific Phase 4 sub-phase.
