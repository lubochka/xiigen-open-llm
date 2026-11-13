# Layer 0-A — Compiler Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Sub-phase: 4.4 (Data Migration and Seeding) / 4.1 (refactoring context)
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

Refactoring vocabulary: refactor, restructure, extract, rename, migrate, consolidate,
clean up, move, reorganize — these fire IMPLEMENT because IMPLEMENT is the predicate
for all code-modification actions. There is no separate REFACTOR predicate.
"Migrate" and "seed" additionally route to 4.4 (Data Migration) when data-context
co-occurrence is present (database, schema, table, data, seed, fixture).

---

## Pair B18B1-WDA-compiler-001 — data migration: IMPLEMENT fires

```
source_id: xiigen-b18-compiler-001
transition_type: feature_addition
phase: 4
sub_phase: 4.4
pair_kind: compiler
```

A:
  user_request: "Write the database migration to move customer data from the legacy schema to the new normalized structure."

B:
  DocumentId: "B18-compiler-001-prompt"
  DocumentTitle: "Customer data migration — legacy to normalized schema"
  CompiledText: "Write the database migration to move customer data from the legacy schema to the new normalized structure."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

Predicate evidence:
  "write" → IMPLEMENT
  "migration" — "migrate" is in IMPLEMENT lexicon → IMPLEMENT fires on "migration"
  "database" — not in IMPLEMENT directly
  "schema" — not in IMPLEMENT (schema without API co-occurrence = no API_CONTRACT; without database DDL context it's just vocabulary)
  "structure" → ARCHITECTURE fires on "structure"!

  Correction: ARCHITECTURE fires on "structure".
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

Corrected B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  SourceStatus: "ready"

Teaching point: "structure" fires ARCHITECTURE even in "normalized structure" (a database design term).
The compiler fires literally. "normalized structure" = ARCHITECTURE term.

---

## Pair B18B1-WDA-compiler-002 — code refactoring: IMPLEMENT

```
source_id: xiigen-b18-compiler-002
transition_type: refactoring
phase: 4
sub_phase: 4.1
pair_kind: compiler
```

A:
  user_request: "Refactor the OrderService class to extract the payment logic into a separate PaymentProcessor module."

B:
  DocumentId: "B18-compiler-002-prompt"
  DocumentTitle: "OrderService refactoring — payment logic extraction to PaymentProcessor"
  CompiledText: "Refactor the OrderService class to extract the payment logic into a separate PaymentProcessor module."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

Predicate evidence:
  "refactor" — IMPLEMENT fires on "refactor" (refactoring is a code-modification action in IMPLEMENT lexicon)
  "class" → IMPLEMENT fires on "class"
  "extract" → IMPLEMENT fires on "extract"
  "logic" → IMPLEMENT fires on "logic"
  "module" → IMPLEMENT fires on "module"
  "payment" — not in any predicate
  "processor" — not in any predicate
  Result: ["IMPLEMENT"] high-density, Confidence: 0.95

---

## Pair B18B1-WDA-compiler-003 — data seeding: IMPLEMENT

```
source_id: xiigen-b18-compiler-003
transition_type: feature_addition
phase: 4
sub_phase: 4.4
pair_kind: compiler
```

A:
  user_request: "Create database seed data for the product catalog with 500 sample products and category fixtures."

B:
  DocumentId: "B18-compiler-003-prompt"
  DocumentTitle: "Product catalog database seed — 500 products with category fixtures"
  CompiledText: "Create database seed data for the product catalog with 500 sample products and category fixtures."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

Predicate evidence:
  "create" → IMPLEMENT
  "seed" — IMPLEMENT fires on "seed" (in IMPLEMENT lexicon as code action: seeding data)
  "data" — not in IMPLEMENT directly
  "fixtures" — IMPLEMENT fires on "fixture" (test fixture = code artifact)
  "catalog", "products", "category", "sample" — none in predicate lexicons
  Result: ["IMPLEMENT"], Confidence: 0.92
