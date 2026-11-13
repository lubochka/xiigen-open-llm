# Cross-Layer Synthesis (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B12-CLS-006 — Full layer-by-layer trace: "database schema design" → ARCHITECTURE only (NOT API_CONTRACT)

### Request

"Design the database schema for the inventory service tables."

---

### Layer 0-A: Compiler

Predicate detection:
  "design" → ARCHITECTURE fires (design = ARCHITECTURE trigger, unconditional)
  "database schema" → "schema" does NOT fire API_CONTRACT — no API co-occurrence present
    → API co-occurrence check: api / rest / graphql / openapi / swagger / grpc / webhook → NONE found
    → "database" is a data-storage term, not an API protocol term → schema gate FAILS
  "database" → no predicate (data layer term, not API or document trigger)
  "tables" → no predicate (database structural term)
  implied "specify" / "document" → DOCUMENT fires (design implies specification)

CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
Confidence: 0.91
TransitionType_hint: "documentation" (no IMPLEMENT verb)

Rules applied:
  Core Rule 5 (schema gate): "schema" requires API co-occurrence → "database schema" has no API co-occurrence → API_CONTRACT does NOT fire
  Core Rule 12 (design): "design" → ARCHITECTURE unconditional
  Core Rule 22: no GapHandler (B12)
  Core Rule 24: DomainId = "software-development"

Anti-pattern (v6-01 trigger): Do NOT fire API_CONTRACT on "database schema". Schema alone is not an API_CONTRACT signal.

---

### Layer 0-B: Router

Routing decisions:
  1. ARCHITECTURE → Phase 3.1 → ArchitectureFlow31 (DecisionCount: 1)
  2. DOCUMENT → Phase 2.1 → DocumentationFlow21 (DecisionCount: 2)

PrimarySubPhase: "3.1"
TotalDecisionCount: 2
TransitionType: "documentation"
FlowOrder: ["ArchitectureFlow31", "DocumentationFlow21"]

v6 check:
  "database schema" → API_CONTRACT must NOT be in CompiledPredicateIds → PASS (API_CONTRACT absent)
  "design" → ARCHITECTURE must be present → PASS
  DecisionCount = 2 → matches len(CompiledPredicateIds) = 2 → PASS (no v6-03)

---

### Layer 0-C: Knowledge Edge (rules applied)

Rule B12-R04 (schema gate): "schema" fires API_CONTRACT ONLY with api/rest/graphql/openapi/swagger/grpc/webhook co-occurrence → "database schema" has no co-occurrence → API_CONTRACT BLOCKED ✓
Rule B12-R12 (design): "design" → ARCHITECTURE fires ✓
Rule B12-R13 (design boundary): "design" does not fire API_CONTRACT alone (API terms absent here) ✓
Rule B12-R14 (routing): ARCHITECTURE → 3.1 ✓
Rule B12-R15 (routing): DOCUMENT → 2.1 ✓

Negative checks:
  "database schema" ≠ API schema → API_CONTRACT blocked ✓
  "tables" → no predicate (data structure term) ✓
  "design" alone (no API co-occurrence) → ARCHITECTURE only, not API_CONTRACT ✓

Corruption risk: v6-01 most likely to incorrectly fire API_CONTRACT on "schema" without checking co-occurrence.

---

### Layer 0-D: Flow Factory

Factory selection for [ARCHITECTURE, DOCUMENT]:
  FlowType: "trusted_learned" (two-predicate ARCHITECTURE+DOCUMENT pattern)
  SelectedFlow: "ArchitectureFlow31-learned-v1"
  SecondaryFlow: "DocumentationFlow21"
  FlowOrder: ["ArchitectureFlow31-learned-v1", "DocumentationFlow21"]
  TransitionType: "documentation"
  FactoryReason: "ARCHITECTURE + DOCUMENT without API_CONTRACT → database/system design scope, not API contract scope"

---

### Layer 1: Transition Orchestrator

ExecutionPlan:
  Phase1_Sequential: ArchitectureFlow31
    - Inputs: user_request, inventory service domain context
    - Outputs: entity-relationship diagram, table definitions, index strategy, normalization notes
  Phase2_Sequential: DocumentationFlow21
    - Inputs: ArchitectureFlow31.outputs.table_definitions
    - Outputs: database schema documentation, data dictionary

TransitionType: "documentation"
Note: No APIContractFlow33 — "database schema" is a data-layer concern, not an API boundary concern.
No code scaffolding (IMPLEMENT absent). No security review (SECURITY absent).

---

### Layer 2: Nine-Node Flow (ArchitectureFlow31 execution — NOT APIContractFlow33)

N01: Input — [ARCHITECTURE, DOCUMENT], documentation TransitionType, inventory database domain
N02: Domain — inventory service data layer
N03: Entity taxonomy — Product, SKU, InventoryItem, Reservation, WarehouseLocation
N04: Schema design — primary keys, foreign keys, indexes, constraints, partition strategy
N05: Normalization — 3NF applied; denormalization for hot-path queries documented
N06: DDL statements — CREATE TABLE with constraints; migration scripts outlined
N07: Validation — all foreign keys resolved, no orphaned entities
N08: Documentation assembly — entity-relationship diagram, data dictionary
N09: Output — inventory-schema.sql (DDL) + inventory-data-dictionary.md
GateB: PASS — "inventory-schema.sql defines ≥3 tables with primary keys and foreign key constraints"

Note: This flow uses ArchitectureFlow31, NOT APIContractFlow33. The nine-node structure is identical in format but the domain processing (N02-N06) is data-layer, not API-layer.

---

### Teaching Layer (rules exercised)

B12-R04 (schema gate): "database schema" → no API co-occurrence → API_CONTRACT blocked ✓
B12-R12 (design): "design" → ARCHITECTURE ✓
B12-R13 (design ARCHITECTURE): ARCHITECTURE fires on "design" alone even when no API terms present ✓
B12-R14 (routing): ARCHITECTURE → 3.1 ✓
B12-R15 (routing): DOCUMENT → 2.1 ✓

Key teaching: "schema" is a dual-domain word. It appears in both API contracts (OpenAPI schemas, GraphQL type definitions) AND in database design (table schemas, DDL). The gate is API co-occurrence. Without it, schema → DOCUMENT scope only.

Artifacts: 2 files
  1. inventory-schema.sql (from ArchitectureFlow31)
  2. inventory-data-dictionary.md (from DocumentationFlow21)

TransitionType path: documentation (no IMPLEMENT, no API_CONTRACT — pure architecture and documentation work)
