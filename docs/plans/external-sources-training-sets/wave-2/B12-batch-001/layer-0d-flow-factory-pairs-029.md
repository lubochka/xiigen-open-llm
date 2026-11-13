# Layer 0-D — Flow Factory Pairs (029)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-029 — BulkImportAPI factory selection

```
source_id: xiigen-b12-0d-029
pair_kind: factory_selection
```

### Input predicates: [API_CONTRACT, DOCUMENT]
Request: "Define the bulk import API contract for loading product catalog data from external systems."

### Factory selection

Step 1 (IMPLEMENT check): IMPLEMENT not present → TransitionType = documentation.
Step 2 (ARCHITECTURE check): ARCHITECTURE not present → NOT included.
Step 3 (API_CONTRACT check): API_CONTRACT present → trusted_learned → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT check): DOCUMENT present → DocumentationFlow21 included.
Step 5 (SECURITY check): SECURITY not present → NOT included.

SelectedFlows: [APIContractFlow33-learned-v1, DocumentationFlow21]
FlowOrder: APIContractFlow33-learned-v1 → DocumentationFlow21 → QAFlow
TransitionType: documentation
