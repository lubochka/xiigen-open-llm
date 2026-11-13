# Layer 0-A — Compiler Pairs (025)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-025-001 — IndexCreation: predicate firing

**Request:** "Create an index on the email column of the users table to improve query performance."

**Token scan:**
- "create an index" → IMPLEMENT fires (Rule 2: new artifact — index is a new database object)
- "email column" → 4.4 sub-phase signal (database table vocabulary)
- "users table" → 4.4 reinforcement
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire
- No "write/add/create tests" → TEST does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development
- GapHandler: none

---

## B18-0A-025-002 — IndexCreation with ARCHITECTURE: "index structure"

**Request:** "Design the index structure for the products table to support full-text search and range queries."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11: structural verb)
- "index structure" → "structure" → ARCHITECTURE fires (Rule 3: absolute)
- No explicit "write migration" or "create migration" → IMPLEMENT does NOT fire independently
- "products table" → 4.4 vocabulary present but no IMPLEMENT verb
- ARCHITECTURE fires twice → one predicate

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-025-003 — IndexCreation with IMPLEMENT + ARCHITECTURE

**Request:** "Create a composite index on (user_id, created_at) and design the index structure for the orders table."

**Token scan:**
- "create a composite index" → IMPLEMENT fires (Rule 2: new artifact)
- "design" → ARCHITECTURE fires (Rule 11)
- "index structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "orders table" → 4.4 vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-025-004 — IndexCreation: "rebuild index" disambiguation

**Request:** "Rebuild the product search index after the bulk data load completes."

**Token scan:**
- "rebuild" (data/index target) → IMPLEMENT fires; data rebuild = feature_addition + 4.4 (Rule 6)
- "product search index" → 4.4 data vocabulary
- "bulk data load" → 4.4 reinforcement (load = data vocabulary)
- "rebuild" does NOT fire refactoring when the target is a data index (not code)

**Key disambiguation:** "rebuild" on code structures → refactoring. "rebuild" on data/index → feature_addition + 4.4.

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-025-005 — IndexCreation with TEST

**Request:** "Create an index on the transactions table and write tests to verify query performance improvement."

**Token scan:**
- "create an index" → IMPLEMENT fires (Rule 2)
- "transactions table" → 4.4 data vocabulary
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "verify" alone does NOT fire TEST; but "write tests" does
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development
