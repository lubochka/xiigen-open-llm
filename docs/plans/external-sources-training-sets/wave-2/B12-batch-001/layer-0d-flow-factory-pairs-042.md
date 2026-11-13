# Layer 0-D — Flow Factory Pairs (042)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-042 — APIKeyManagement factory selection

```
source_id: xiigen-b12-0d-042
pair_kind: factory_selection
```

### Input predicates: [IMPLEMENT, API_CONTRACT, SECURITY]
Request: "Implement the API key generation endpoint with authentication checks and write the key issuance contract."

### Factory selection

Step 1 (IMPLEMENT): present → ImplementationFlow43 included. TransitionType = feature_addition.
Step 2 (ARCHITECTURE): not present → NOT included.
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): not present → NOT included.
Step 5 (SECURITY): present → SecurityAuditFlow34 included (parallel with IMPLEMENT and API_CONTRACT).

SelectedFlows: [ImplementationFlow43, APIContractFlow33-learned-v1, SecurityAuditFlow34]
FlowOrder: parallel(ImplementationFlow43, APIContractFlow33-learned-v1, SecurityAuditFlow34) → QAFlow
TransitionType: feature_addition
