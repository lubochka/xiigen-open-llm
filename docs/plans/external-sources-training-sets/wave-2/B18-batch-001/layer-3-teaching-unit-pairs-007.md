# Layer 3 — Teaching Unit Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-007-001 — Teaching: TransitionType refactoring word list vs feature_addition word list

**What the model must learn:** TransitionType is determined by the primary action verb. Two word lists govern this classification.

**REFACTORING word list (→ TransitionType: refactoring):**
refactor, extract (code), rename, consolidate, restructure, reorganize, move (code), clean up, split (code), decompose, decouple, simplify (code), inline, flatten (code)

**FEATURE_ADDITION word list (→ TransitionType: feature_addition):**
write migration, create migration, build migration, seed, fixture, migrate (data), transform (data), ETL, load (data), extract (data), implement (new artifact), build (new pipeline)

---

## B18-TU-007-002 — Teaching: the hierarchy rule

**TransitionType hierarchy (Rule 8):** refactoring > feature_addition > documentation > no contribution > none

**When both lists have matches:**

1. "Refactor AND write migration" → refactoring wins (Rule 9 + Rule 8 hierarchy)
2. "Consolidate AND seed" → refactoring wins (consolidate = refactoring; seed = feature_addition; hierarchy: refactoring wins)
3. "Rename AND document" → refactoring wins (rename = refactoring; document = documentation; hierarchy: refactoring wins)

**Exception:** "Write migration to consolidate" → feature_addition may win (Rule 2 — migration is the explicit artifact being created)

---

## B18-TU-007-003 — Teaching: Rule 9 — "refactor" always overrides

**Rule 9 states:** "refactor" verb → TransitionType: refactoring (overrides feature_addition even if IMPLEMENT fires for multiple reasons).

**This is an explicit override rule.** Even if feature_addition signals are strong, Rule 9 ensures "refactor" always produces TransitionType: refactoring.

**Examples:**
- "Refactor and write the migration" → refactoring (Rule 9)
- "Refactor the service, add seed data, and build the ETL" → refactoring (Rule 9)
- "Refactor 5 things and write 3 migrations" → refactoring (Rule 9)

**The only time feature_addition wins over refactoring:** when "write/create/build migration" is the PRIMARY TASK (Rule 2) and "refactor" is secondary/incidental. The test: "Is the primary deliverable a migration script?" If yes → feature_addition. If the primary deliverable is refactored code → refactoring.

---

## B18-TU-007-004 — Teaching: feature_addition word list detail

**When each feature_addition word fires:**

| Word | Fires when | Example |
|---|---|---|
| write migration | "write the migration" is primary task | "Write the customer migration" |
| create migration | Same as above | "Create migration for payment table" |
| seed | "seed" standalone or in compound | "Seed the product database" |
| fixture | "fixture" standalone or in compound | "Create category fixtures" |
| migrate (data) | "migrate" + data vocabulary | "Migrate customer records" |
| ETL | ETL abbreviation present | "Build an ETL pipeline" |
| transform (data) | "transform" + data vocabulary | "Transform CSV data" |
| load (data) | "load" + data vocabulary | "Load data into warehouse" |

---

## B18-TU-007-005 — Teaching: documentation is NOT refactoring

**Important: DOCUMENT predicate does NOT affect TransitionType in B18**

When DOCUMENT fires (on "document", "annotate", "spec out"):
- It adds DOCUMENT to the predicate list
- It does NOT change TransitionType to "documentation"
- TransitionType remains refactoring or feature_addition (based on IMPLEMENT verb)

**TransitionType: documentation** is only assigned when the ONLY predicate is DOCUMENT (no IMPLEMENT).

**In B18:** DOCUMENT always co-occurs with IMPLEMENT. Therefore TransitionType in B18 is always refactoring or feature_addition (never documentation alone).

---

## B18-TU-007-006 — Teaching: TransitionType summary quiz

**Quick quiz — what is the TransitionType?**

1. "Refactor the OrderService" → refactoring
2. "Write the OrderHistoryMigration" → feature_addition
3. "Seed 1,000 products" → feature_addition
4. "Consolidate three repositories" → refactoring
5. "Build an ETL pipeline" → feature_addition
6. "Rename processPayment() to executeTransaction()" → refactoring
7. "Refactor the service AND write the migration" → refactoring (Rule 9 wins)
8. "Write migration to consolidate tables" → feature_addition (Rule 2 — migration is the artifact)
9. "Migrate customer records" → feature_addition
10. "Extract PaymentProcessor from OrderService" → refactoring

**Score:** 10/10 is the expected output for a correctly trained classifier.
