# Layer 0-B — Router Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-020-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (FileUploadAPI)

```
source_id: xiigen-b12-0b-020-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the file upload API endpoint contract including multipart request format."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-020-002 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (write file upload spec)

```
source_id: xiigen-b12-0b-020-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Write the OpenAPI specification for the file upload service."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-020-003 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (signed URL auth)

```
source_id: xiigen-b12-0b-020-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Document the file upload REST API authentication requirements and signed URL contract."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0B-020-004 — FileUploadAPI: create REST contract → feature_addition routing

```
source_id: xiigen-b12-0b-020-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Create the REST API contract and response schema for the document upload service."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

Teaching: "create" = IMPLEMENT verb → feature_addition.
Same predicate set as pair-002, same routing. "create" and "write" both → IMPLEMENT + feature_addition.
