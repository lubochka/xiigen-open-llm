# Layer 0-D — Flow Factory Pairs (047)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-047 — APIDocGenerator factory selection

```
source_id: xiigen-b12-0d-047
pair_kind: factory_selection
```

### Input predicates: [IMPLEMENT, API_CONTRACT, DOCUMENT]
Request: "Build the API documentation generator that reads OpenAPI specs and writes HTML reference pages."

### Factory selection

Step 1 (IMPLEMENT): present → ImplementationFlow43 included. TransitionType = feature_addition.
Step 2 (ARCHITECTURE): not present → NOT included.
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): present → DocumentationFlow21.
Step 5 (SECURITY): not present → NOT included.

SelectedFlows: [ImplementationFlow43, APIContractFlow33-learned-v1, DocumentationFlow21]
FlowOrder: parallel(ImplementationFlow43, APIContractFlow33-learned-v1) → DocumentationFlow21 → QAFlow
TransitionType: feature_addition
