# Layer 0-D — Flow Factory Pairs (049)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-049 — OpenAPISpec factory selection

```
source_id: xiigen-b12-0d-049
pair_kind: factory_selection
```

### Input predicates: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY]
Request: "Design and specify the OpenAPI contract for the billing service including authentication scheme definitions."

### Factory selection

Step 1 (IMPLEMENT): not present → documentation.
Step 2 (ARCHITECTURE): present → ArchitectureFlow31 included (tier 0, executes first).
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): present → DocumentationFlow21.
Step 5 (SECURITY): present → SecurityAuditFlow34 (parallel with DOCUMENT).

SelectedFlows: [ArchitectureFlow31, APIContractFlow33-learned-v1, DocumentationFlow21, SecurityAuditFlow34]
FlowOrder: ArchitectureFlow31 → APIContractFlow33-learned-v1 → parallel(DocumentationFlow21, SecurityAuditFlow34) → QAFlow
TransitionType: documentation
