# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-011-001 — Canonical B12 orchestration patterns: five core patterns

```
source_id: xiigen-b12-l1-011-001
pair_kind: transition_orchestrator_reference
coverage_status: reference
```

### Five Canonical B12 Orchestration Patterns

Pattern 1: Single API contract documentation
  CompiledPredicateIds: [API_CONTRACT, DOCUMENT]
  TransitionType: documentation
  ExecutionModel: 2-flow sequential
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  QA: after DocumentationFlow21
  Typical: gRPC contract, webhook contract, data contracts, versioning docs

Pattern 2: API contract with security documentation
  CompiledPredicateIds: [API_CONTRACT, DOCUMENT, SECURITY]
  TransitionType: documentation
  ExecutionModel: 1+2 parallel
  FlowOrder: [APIContractFlow33, then parallel(DocumentationFlow21, SecurityAuditFlow34)]
  QA: after both parallel flows
  Typical: REST API with authentication, webhook with auth headers

Pattern 3: Architecture-driven API contract
  CompiledPredicateIds: [ARCHITECTURE, API_CONTRACT, DOCUMENT]
  TransitionType: documentation
  ExecutionModel: 3-flow sequential
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  QA: after DocumentationFlow21
  Typical: designing new API from architectural requirements

Pattern 4: New API implementation with spec
  CompiledPredicateIds: [IMPLEMENT, API_CONTRACT, DOCUMENT]
  TransitionType: feature_addition
  ExecutionModel: 2+1 parallel
  FlowOrder: [parallel(ImplementationFlow43, APIContractFlow33), DocumentationFlow21]
  QA: after DocumentationFlow21
  Typical: writing new OpenAPI spec and implementing new endpoint

Pattern 5: Full OAuth API cycle
  CompiledPredicateIds: [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]
  TransitionType: feature_addition
  ExecutionModel: 2+3 parallel
  FlowOrder: [parallel(ImplementationFlow43, APIContractFlow33), parallel(DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34)]
  QA: after all Phase2 flows
  Typical: implementing OAuth service with complete documentation and security review

---

## B12-L1-011-002 — Nine-node flow orchestration: B12 Layer 2 connection

```
source_id: xiigen-b12-l1-011-002
pair_kind: transition_orchestrator_reference
```

Connection between Layer 1 orchestrator and Layer 2 nine-node flows:

Layer 1 orchestrator determines: which flows run, in what order, with what parallelism
Layer 2 nine-node flows: each flow (APIContractFlow33, DocumentationFlow21, etc.) runs
as a nine-node flow internally

Nine-node flow structure (internal to each Layer 2 flow):
  N01: Input classification (predicates, TransitionType received from Layer 1)
  N02-N08: Processing nodes (specific to each flow)
  N09: Output node (produces the files/artifacts)

When Layer 1 selects APIContractFlow33, that flow runs internally as a nine-node flow:
  N01: receives [API_CONTRACT, DOCUMENT] predicates and "documentation" TransitionType
  N02-N08: processes the API contract documentation request
  N09: outputs contract files

Layer 1 orchestrates WHICH nine-node flows run.
Layer 2 defines WHAT each nine-node flow does internally.

---

## B12-L1-011-003 — Canonical pattern selection logic

```
source_id: xiigen-b12-l1-011-003
pair_kind: transition_orchestrator_reference
```

How to select the canonical pattern for a given request:

Step 1: Determine CompiledPredicateIds from Layer 0-A compiler output
Step 2: Determine TransitionType (IMPLEMENT present → feature_addition, else documentation)
Step 3: Match against canonical pattern table:
  - [API_CONTRACT, DOCUMENT] → Pattern 1
  - [API_CONTRACT, DOCUMENT, SECURITY] → Pattern 2
  - [ARCHITECTURE, API_CONTRACT, DOCUMENT] → Pattern 3
  - [IMPLEMENT, API_CONTRACT, DOCUMENT] → Pattern 4
  - [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] → Pattern 5
  - Other combinations → compose from individual predicate flows

Step 4: Execute ExecutionModel from matched pattern
Step 5: Add QA checkpoint at end

For non-matching combinations (e.g., [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY]):
  Compose: ArchitectureFlow31 (first) + APIContractFlow33 (second) + parallel(DocumentationFlow21, SecurityAuditFlow34)
  This is not in the canonical 5, but follows the composition rules:
  - Architecture first
  - API contract second
  - Documentation and security parallel last

---

## B12-L1-011-004 — Canonical patterns: file output counts

```
source_id: xiigen-b12-l1-011-004
pair_kind: transition_orchestrator_reference
```

Expected file outputs per canonical pattern:

Pattern 1 [API_CONTRACT, DOCUMENT]:
  Minimum: 2 files (contract spec + documentation)
  Typical: 3-4 files (spec + guide + supporting notes)

Pattern 2 [API_CONTRACT, DOCUMENT, SECURITY]:
  Minimum: 3 files (contract + documentation + security notes)
  Typical: 4-5 files

Pattern 3 [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  Minimum: 3 files (architecture doc + contract + developer guide)
  Typical: 4-6 files

Pattern 4 [IMPLEMENT, API_CONTRACT, DOCUMENT]:
  Minimum: 3 files (code + spec + documentation)
  Typical: 4-6 files (implementation stubs + spec + reference + guide)

Pattern 5 [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
  Minimum: 5 files (code + spec + documentation + security notes + auth docs)
  Typical: 6-8 files
