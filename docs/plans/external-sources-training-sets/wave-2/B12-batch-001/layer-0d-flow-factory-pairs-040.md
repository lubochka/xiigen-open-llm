# Layer 0-D — Flow Factory Pairs (040)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-040 — VersionedEndpoint factory selection

```
source_id: xiigen-b12-0d-040
pair_kind: factory_selection
```

### Input predicates: [ARCHITECTURE, API_CONTRACT]
Request: "Design the versioned API strategy with header-based and URI-based version negotiation."

### Factory selection

Step 1 (IMPLEMENT): not present → documentation.
Step 2 (ARCHITECTURE): present → ArchitectureFlow31 included (tier 0, executes first).
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): not present → NOT included.
Step 5 (SECURITY): not present → NOT included.

SelectedFlows: [ArchitectureFlow31, APIContractFlow33-learned-v1]
FlowOrder: ArchitectureFlow31 → APIContractFlow33-learned-v1 → QAFlow
TransitionType: documentation
