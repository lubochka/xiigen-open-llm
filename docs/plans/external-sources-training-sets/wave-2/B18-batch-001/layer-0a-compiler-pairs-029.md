# Layer 0-A — Compiler Pairs (029)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-029-001 — StoredProcedure: predicate firing

**Request:** "Write a stored procedure to batch-process order fulfillment status updates."

**Token scan:**
- "write a stored procedure" → IMPLEMENT fires (Rule 2: new artifact — stored procedure is a new database object)
- "batch-process" → 4.4 data processing vocabulary
- "order fulfillment status updates" → 4.4 data vocabulary
- No "structure", "architecture" → ARCHITECTURE does not fire
- No security tokens, no TEST verb

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-029-002 — StoredProcedure: "refactor stored procedure" disambiguation

**Request:** "Refactor the legacy payment processing stored procedure to use set-based operations instead of cursors."

**Token scan:**
- "refactor" → IMPLEMENT fires; refactor is a refactoring verb → TransitionType: refactoring (Rule 9, absolute)
- "stored procedure" → 4.4 database object vocabulary
- "set-based operations" → 4.4 SQL vocabulary
- "cursors" → 4.4 database vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-029-003 — StoredProcedure with ARCHITECTURE

**Request:** "Design the stored procedure architecture for the data pipeline before implementing the migration procedures."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "stored procedure architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "implementing the migration procedures" → IMPLEMENT fires (Rule 2: new procedures = new artifacts)
- "data pipeline" → 4.4 vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-029-004 — StoredProcedure: "migrate to stored procedure" disambiguation

**Request:** "Migrate the inline SQL queries in the reporting module to stored procedures."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5: migrate verb with data/database target)
- "inline SQL queries" → 4.4 vocabulary
- "stored procedures" → 4.4 database object vocabulary
- "reporting module" → could suggest 4.1; but "migrate" + SQL/procedure vocabulary → 4.4 wins
- No ARCHITECTURE trigger (no "structure", "pattern", "architecture")

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-029-005 — StoredProcedure with TEST

**Request:** "Create stored procedures for all data transformation steps and write integration tests to verify each procedure."

**Token scan:**
- "create stored procedures" → IMPLEMENT fires (Rule 2)
- "data transformation steps" → 4.4 vocabulary (ETL-adjacent)
- "write integration tests" → TEST fires (Rule 12: verb form)
- "verify each procedure" → TEST reinforcement; but "write" is the TEST trigger
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development
