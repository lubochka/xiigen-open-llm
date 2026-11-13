# Layer 0-D — Flow Factory Pairs (043)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-043 — OAuthScopeAPI factory selection

```
source_id: xiigen-b12-0d-043
pair_kind: factory_selection
```

### Input predicates: [API_CONTRACT, DOCUMENT, SECURITY]
Request: "Define the OAuth scope API contract for the customer data access endpoints."

### Factory selection

Step 1 (IMPLEMENT): not present → documentation.
Step 2 (ARCHITECTURE): not present → NOT included.
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): present → DocumentationFlow21.
Step 5 (SECURITY): present → SecurityAuditFlow34 (parallel with DOCUMENT).

SelectedFlows: [APIContractFlow33-learned-v1, DocumentationFlow21, SecurityAuditFlow34]
FlowOrder: APIContractFlow33-learned-v1 → parallel(DocumentationFlow21, SecurityAuditFlow34) → QAFlow
TransitionType: documentation
