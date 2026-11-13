# Layer 0-A — Compiler Pairs (041)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-041-001 — CompositeIndex: predicate firing

**Request:** "Create a composite index on (tenant_id, created_at, status) for the orders table."

**Token scan:**
- "create a composite index" → IMPLEMENT fires (Rule 2: new index artifact)
- "tenant_id, created_at, status" → 4.4 column vocabulary
- "orders table" → 4.4 database table vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-041-002 — CompositeIndex with ARCHITECTURE

**Request:** "Design the composite index pattern for multi-tenant query performance across orders and payments tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "composite index pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "multi-tenant" → 4.4 architecture vocabulary
- No explicit "write migration" or "create index" → IMPLEMENT does not fire
- No security or TEST tokens

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-041-003 — CoveringIndex: predicate firing

**Request:** "Add a covering index on orders(customer_id) INCLUDE (total_amount, created_at) to eliminate table lookups."

**Token scan:**
- "add a covering index" → IMPLEMENT fires (Rule 2: new index artifact)
- "INCLUDE" clause → 4.4 DDL vocabulary (covering index syntax)
- "orders(customer_id)" → 4.4 table/column vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-041-004 — PartialIndex: predicate firing

**Request:** "Create a partial index on orders WHERE status = 'active' to optimize queries for active orders only."

**Token scan:**
- "create a partial index" → IMPLEMENT fires (Rule 2: new index artifact)
- "WHERE status = 'active'" → 4.4 DDL predicate vocabulary
- "orders" → 4.4 database table vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-041-005 — PartialIndex with ARCHITECTURE + IMPLEMENT combo

**Request:** "Design the partial index structure for the transactions table and implement the indexes for pending and failed states."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "partial index structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "implement the indexes" → IMPLEMENT fires (Rule 2: new artifacts)
- "pending and failed states" → 4.4 vocabulary (query predicate conditions)
- No security or TEST tokens; no refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development
