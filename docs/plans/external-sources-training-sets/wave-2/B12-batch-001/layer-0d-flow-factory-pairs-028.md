# Layer 0-D — Flow Factory Pairs (028)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-028 — EventStreamingAPI factory selection

```
source_id: xiigen-b12-0d-028
pair_kind: factory_selection
```

### Input predicates: [IMPLEMENT, API_CONTRACT, DOCUMENT]
Request: "Build the event streaming API contract and document the event schema for order lifecycle events."

### Factory selection

Step 1 (IMPLEMENT check): IMPLEMENT present → ImplementationFlow43 included. TransitionType = feature_addition.
Step 2 (ARCHITECTURE check): ARCHITECTURE not present → NOT included.
Step 3 (API_CONTRACT check): API_CONTRACT present → trusted_learned → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT check): DOCUMENT present → DocumentationFlow21 included (after contract).
Step 5 (SECURITY check): SECURITY not present → NOT included.

SelectedFlows: [ImplementationFlow43, APIContractFlow33-learned-v1, DocumentationFlow21]
FlowOrder: parallel(ImplementationFlow43, APIContractFlow33-learned-v1) → DocumentationFlow21 → QAFlow
TransitionType: feature_addition
