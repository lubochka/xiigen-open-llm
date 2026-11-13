# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-006-001 — documentation vs feature_addition distinction for API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-l1-006-001
pair_kind: transition_orchestrator
```

Two requests with same predicate set but different TransitionType:

Case A: documentation (no IMPLEMENT)
  Request: "Define the webhook contract for order status events."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  OrchestratorBehavior:
    - Checks existing webhook documentation repository
    - Produces contract specification (may create new spec file, but for existing system)
    - Does NOT create new webhook handler code
    - Outputs: webhook-contract.yaml, webhook-integration-guide.md

Case B: feature_addition (with IMPLEMENT)
  Request: "Implement the webhook for order status events and write the contract specification."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  OrchestratorBehavior:
    - Creates NEW webhook handler code
    - Creates NEW API contract specification
    - Creates NEW documentation for the new webhook
    - Outputs: webhook-handler.cs, webhook-contract.yaml, webhook-integration-guide.md

Key difference:
  documentation: spec/documentation files only (no new service code)
  feature_addition: spec + code + documentation (all new artifacts)

---

## B12-L1-006-002 — TransitionType: orchestrator scaffolding differences

```
source_id: xiigen-b12-l1-006-002
pair_kind: transition_orchestrator
```

Orchestrator scaffolding for documentation TransitionType:
  - File operations: update or create documentation/specification files
  - No code generation scaffolding triggered
  - No test framework provisioning
  - No CI/CD pipeline configuration
  - Commit message template: "docs: [describe documentation update]"

Orchestrator scaffolding for feature_addition TransitionType:
  - File operations: create code files, spec files, documentation files
  - Code generation scaffolding triggered (service template, controller)
  - Test stubs created for new endpoints
  - CI/CD configuration updates triggered
  - Commit message template: "feat: [describe new feature]"

For B12 API contract work:
  Documentation-only requests → documentation scaffolding (no new code)
  Requests with "write/implement/build" → feature_addition scaffolding (code + docs)

---

## B12-L1-006-003 — TransitionType detection for ambiguous cases

```
source_id: xiigen-b12-l1-006-003
pair_kind: transition_orchestrator
```

Ambiguous cases and their resolution:

Case 1: "Create the API documentation for the cart service"
  "create" = code verb? → check object: "documentation" → documentation object
  → IMPLEMENT does NOT fire (creating documentation, not code)
  → DOCUMENT fires on "documentation"
  → TransitionType: documentation

Case 2: "Create the cart service API endpoint"
  "create" = code verb? → check object: "API endpoint" → code artifact
  → IMPLEMENT fires (creating a code artifact)
  → TransitionType: feature_addition

Case 3: "Build the OpenAPI specification"
  "build" = code verb? → check object: "OpenAPI specification" → specification document
  Ambiguous: "build" usually fires IMPLEMENT
  Resolution: "build" always fires IMPLEMENT (code verb, Core Rule 11)
  → TransitionType: feature_addition

Case 4: "Write the API specification document"
  "write" fires IMPLEMENT in code artifact creation context
  "specification document" = documentation artifact
  Resolution: "write the specification" → "write" fires IMPLEMENT (code verb); spec is new artifact
  → TransitionType: feature_addition (creating new spec file)

---

## B12-L1-006-004 — TransitionType summary table for B12

```
source_id: xiigen-b12-l1-006-004
pair_kind: transition_orchestrator
```

Complete TransitionType determination table for B12:

IMPLEMENT predicate present → feature_addition (always)

| Contains IMPLEMENT? | TransitionType |
|---------------------|----------------|
| YES (any case) | feature_addition |
| NO | documentation |

Predicates that ALWAYS fire IMPLEMENT:
  - "implement", "build", "create [code artifact]", "write [spec/contract]", "develop"

Predicates that NEVER fire IMPLEMENT:
  - "document", "define", "specify", "describe", "annotate", "design", "integrate", "review"
  - "authentication", "oauth", "access control" (fire SECURITY/AUTHORIZATION)

Predicates that are CONTEXT-DEPENDENT for IMPLEMENT:
  - "write" — fires IMPLEMENT when creating a new code/spec artifact; DOCUMENT when describing
  - "create" — fires IMPLEMENT for code artifacts; not for "create documentation"
  - "add" — fires IMPLEMENT for "add endpoint"; DOCUMENT for "add notes to documentation"
