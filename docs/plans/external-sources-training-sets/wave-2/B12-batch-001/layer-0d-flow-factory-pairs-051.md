# Layer 0-D — Flow Factory Pairs (051)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-051 — ServiceMeshAPI factory selection

```
source_id: xiigen-b12-0d-051
pair_kind: factory_selection
```

### Input predicates: [API_CONTRACT, SECURITY]
Request: "Define the service mesh API contract with mutual authentication between internal services."

### Factory selection

Step 1 (IMPLEMENT check): IMPLEMENT not present → TransitionType = documentation.
Step 2 (ARCHITECTURE check): ARCHITECTURE not present → NOT included.
Step 3 (API_CONTRACT check): API_CONTRACT present → trusted_learned → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT check): DOCUMENT not present → NOT included.
Step 5 (SECURITY check): SECURITY present → SecurityAuditFlow34 included (no parallel target — runs after API).

SelectedFlows: [APIContractFlow33-learned-v1, SecurityAuditFlow34]
FlowOrder: APIContractFlow33-learned-v1 → SecurityAuditFlow34 → QAFlow
TransitionType: documentation
