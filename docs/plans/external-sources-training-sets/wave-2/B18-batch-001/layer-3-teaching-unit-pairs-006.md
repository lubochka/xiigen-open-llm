# Layer 3 — Teaching Unit Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-006-001 — Teaching: "migrate" disambiguation

**What the model must learn:** "migrate" has two distinct meanings in B18 — code migration and data migration. They route differently.

**Data migration (→ IMPLEMENT, feature_addition, 4.4):**
- Moving data records from one table/schema/database to another
- Creating a new data state
- Example: "Migrate customer records to the new schema"

**Code migration (→ IMPLEMENT, refactoring or feature_addition, 4.1):**
- Moving code from one framework/platform/structure to another
- Example: "Migrate the application from .NET Framework to .NET 9"
- Example: "Migrate the stored procedures to C# service methods"

---

## B18-TU-006-002 — Teaching: "migrate" + data vocabulary → 4.4

**Reliable data migration signals:**

| Phrase | Route |
|---|---|
| "migrate records" | 4.4 |
| "migrate data" | 4.4 |
| "migrate rows" | 4.4 |
| "migrate from one table/database to another" | 4.4 |
| "migrate customer/order/product [noun]" | 4.4 (data entity) |
| "migrate to normalized schema" | 4.4 |
| "migrate legacy [data noun]" | 4.4 |

**The presence of data nouns (records, rows, data, table, schema context, database) confirms 4.4.**

---

## B18-TU-006-003 — Teaching: "migrate" + code vocabulary → 4.1

**Reliable code migration signals:**

| Phrase | Route |
|---|---|
| "migrate the application" | 4.1 |
| "migrate the codebase" | 4.1 |
| "migrate from framework X to Y" | 4.1 |
| "migrate stored procedures to services" | 4.1 |
| "migrate jQuery to React" | 4.1 |

**The presence of code nouns (application, codebase, framework, language, technology, stored procedures → services) confirms 4.1.**

---

## B18-TU-006-004 — Teaching: "migrate" TransitionType in data context

**Data migration TransitionType:** ALWAYS feature_addition (Rule 17)

**Why feature_addition:**
- Data migration creates a new data state
- The migrated data in the target tables/schema is a new artifact
- Nothing was "refactored" — the data was moved

**Even if "write migration" has a verb "write" that could suggest feature_addition:**
- "Write migration" → feature_addition (Rule 2 — creating a new migration script)
- "Migrate data" → feature_addition (Rule 17 — creating a new data state)

**Data migration is NEVER refactoring in B18.** The TransitionType for data migration scenarios is always feature_addition.

---

## B18-TU-006-005 — Teaching: "migrate" + refactoring verb = disambiguation required

**When "migrate" appears with a refactoring verb:**

Request: "Refactor the data migration service to consolidate duplicate query logic."

**Analysis:**
- "refactor" → IMPLEMENT, refactoring
- "consolidate" → IMPLEMENT, refactoring
- "migration service" → what is being acted on? The service (code class), not data records
- "query logic" → code, not data
- Context: code service class is being refactored
- Route: 4.1 (code vocabulary: service, query logic)
- TransitionType: refactoring (refactor verb wins)

**This is NOT a data migration.** The word "migration" describes the SERVICE NAME, not the action. The action is refactoring the service. Route: 4.1.

---

## B18-TU-006-006 — Teaching: "migrate" vs "write migration" — subtle difference

**"migrate [data]" vs "write [the] migration":**

Both route to 4.4 and both are feature_addition. The difference is subtle:

**"migrate data":**
- Action: the act of moving data records
- TransitionType: feature_addition (new data state)
- May or may not involve a formal migration script

**"write [the] migration":**
- Action: creating a migration script (code artifact)
- TransitionType: feature_addition (Rule 2 — new artifact)
- Explicitly creates a SQL or code migration file

**In B18:** both phrases route to DataMigrationFlow44 (4.4). The difference in phrasing does not change the routing.
