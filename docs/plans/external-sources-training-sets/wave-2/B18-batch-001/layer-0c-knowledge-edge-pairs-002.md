# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-002-001 — 4.4 vs 4.1 routing disambiguation: complete vocabulary comparison

**Sub-phase 4.4 (Data Migration and Seeding) routing vocabulary:**
| Word/phrase | Signal strength | Notes |
|---|---|---|
| migrate | Strong | Data context |
| migration | Strong | Always 4.4 when present |
| seed | Strong | Always 4.4 |
| seeds | Strong | Always 4.4 |
| fixture | Strong | Always 4.4 |
| fixtures | Strong | Always 4.4 |
| ETL | Strong | Extract/Transform/Load |
| transform (data) | Strong | Data transformation |
| load (data) | Strong | Data loading |
| extract (data) | Moderate | Data extraction |
| data | Moderate | With migration context |
| records | Moderate | With migration context |
| database | Moderate | With migration context |
| table / tables | Moderate | Database vocabulary |
| schema change | Strong | 4.4 (schema as data object) |
| batch | Moderate | Batch processing of data |
| rollback | Moderate | Migration rollback |
| idempotent | Weak | Migration safety vocabulary |

---

## B18-0C-002-002 — Sub-phase 4.1 (Refactoring) routing vocabulary

**Sub-phase 4.1 (Refactoring and Code Restructuring) vocabulary:**
| Word/phrase | Signal strength | Notes |
|---|---|---|
| service / services | Strong | Service class vocabulary |
| class / classes | Strong | Object-oriented vocabulary |
| method / methods | Strong | Code method vocabulary |
| function / functions | Strong | Function vocabulary |
| module / modules | Strong | Module vocabulary |
| repository / repositories | Strong | Repository pattern vocabulary |
| domain | Strong | Domain-driven design vocabulary |
| rule / rules (business) | Strong | Business rule vocabulary |
| business | Moderate | Business logic context |
| handler / handlers | Strong | Handler vocabulary |
| controller | Strong | MVC (Model/View/Controller) layer vocabulary |
| interface | Moderate | Code interface vocabulary |
| base class | Moderate | OOP (Object-Oriented Programming) vocabulary |
| dependency | Moderate | Dependency injection vocabulary |
| layer | Moderate | Architectural layer vocabulary |
| logic | Moderate | Business/domain logic |

---

## B18-0C-002-003 — 4.4 vs 4.1 disambiguation: word boundary cases

**Words that could signal either — disambiguation required:**

**"repository":**
- "Refactor the OrderRepository" → 4.1 (code class refactoring)
- "Write migration for the order repository table" → 4.4 (database table migration)
- **Rule:** "repository" + refactoring verb → 4.1. "repository" + migration verb + "table" → 4.4.

**"schema":**
- "Migrate data to the new schema" → 4.4 (schema = target data structure for migration)
- "Design the schema architecture" → 3.1 (schema = architectural design noun)
- **Rule:** "schema" + migration vocabulary → 4.4. "schema" + "architecture"/"design" → 3.1.

**"model":**
- "Refactor the domain model" → 4.1 (code domain model)
- "Migrate the data model" → 4.4 (data model migration)
- **Rule:** "model" + code/domain context → 4.1. "model" + data/migrate context → 4.4.

---

## B18-0C-002-004 — When both 4.4 and 4.1 vocabulary present

**Request:** "Refactor the OrderRepository and write the migration for the orders table."

**Vocabulary analysis:**
- "Refactor" → IMPLEMENT (refactoring); "OrderRepository" → 4.1 vocabulary
- "write migration" → IMPLEMENT (feature_addition); "orders table" → 4.4 vocabulary
- Both sub-phases signaled

**TransitionType conflict:**
- "Refactor" → refactoring (Rule 9)
- "write migration" → feature_addition (Rule 2)
- Resolution: refactoring > feature_addition (Rule 8 hierarchy)
- TransitionType: refactoring

**But sub-phase:** both 4.1 and 4.4 fire (both vocabularies present)
**Routing:** BackendServiceFlow41 (4.1) + DataMigrationFlow44 (4.4)

---

## B18-0C-002-005 — SchemaConsolidation: both 4.4 and 4.1 routing

**Request:** "Refactor three overlapping customer tables into one normalized schema and write the consolidation migration."

**Predicate result:** ["IMPLEMENT"] (both refactoring and migration fire; IMPLEMENT is still one predicate)
**TransitionType:** refactoring (Rule 9 — "refactor" wins)
**Sub-phases:** 4.1 (refactor vocabulary) + 4.4 (migration vocabulary)
**Flows:** BackendServiceFlow41 (4.1) + DataMigrationFlow44 (4.4)

**This is the SchemaConsolidation scenario** — has both refactoring and feature_addition signals. The TransitionType is refactoring (wins), but both flows fire because both sub-phase vocabularies are present.

---

## B18-0C-002-006 — 4.4 vs 4.1 summary: one-line disambiguation rules

**Quick disambiguation rules:**
1. Migration artifact being written? → 4.4 (DataMigrationFlow44)
2. Seed/fixture data being created? → 4.4 (DataMigrationFlow44)
3. ETL pipeline being built? → 4.4 (DataMigrationFlow44)
4. Code class/service/method being restructured? → 4.1 (BackendServiceFlow41)
5. Business rule/domain logic being reorganized? → 4.1 (BackendServiceFlow41)
6. Both migration + code refactoring? → Both flows fire
7. TransitionType determination: refactoring verb present → refactoring (wins)
