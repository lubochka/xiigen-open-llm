# Layer 0-D — Flow Factory Pairs (048)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-048 — API_CONTRACT+SECURITY combo factory selection and Round 3 summary

```
source_id: xiigen-b12-0d-048
pair_kind: factory_reference
```

### Input predicates: [IMPLEMENT, API_CONTRACT, SECURITY]
Request: "Implement the secure REST API endpoint with OAuth2 bearer token authentication for the admin panel."

### Factory selection

Step 1 (IMPLEMENT): present → ImplementationFlow43 included. TransitionType = feature_addition.
Step 2 (ARCHITECTURE): not present → NOT included.
Step 3 (API_CONTRACT): present → APIContractFlow33-learned-v1.
Step 4 (DOCUMENT): not present → NOT included.
Step 5 (SECURITY): present → SecurityAuditFlow34 (parallel with IMPLEMENT and API_CONTRACT).

SelectedFlows: [ImplementationFlow43, APIContractFlow33-learned-v1, SecurityAuditFlow34]
FlowOrder: parallel(ImplementationFlow43, APIContractFlow33-learned-v1, SecurityAuditFlow34) → QAFlow
TransitionType: feature_addition

### Round 3 factory map (files 037–048)

| File | Input predicates | Key factory decision |
|------|-----------------|---------------------|
| 037 | [API_CONTRACT, DOCUMENT, SECURITY] | SECURITY → SecurityAuditFlow34 parallel with DOC |
| 038 | [ARCHITECTURE, API_CONTRACT, DOCUMENT] | ARCHITECTURE → tier 0 |
| 039 | [IMPLEMENT, API_CONTRACT] | IMPLEMENT → parallel with API_CONTRACT |
| 040 | [ARCHITECTURE, API_CONTRACT] | ARCHITECTURE → tier 0, no DOCUMENT |
| 041 | [API_CONTRACT, DOCUMENT] | documentation pattern |
| 042 | [IMPLEMENT, API_CONTRACT, SECURITY] | 3-way parallel (Impl + API + Security) |
| 043 | [API_CONTRACT, DOCUMENT, SECURITY] | Security parallel with DOCUMENT |
| 044 | [API_CONTRACT, DOCUMENT, SECURITY] | Security parallel with DOCUMENT |
| 045 | [IMPLEMENT, API_CONTRACT, DOCUMENT] | Impl+API parallel → DOC sequential |
| 046 | [API_CONTRACT, DOCUMENT] | documentation pattern |
| 047 | [IMPLEMENT, API_CONTRACT, DOCUMENT] | Impl+API parallel → DOC sequential |
| 048 | [IMPLEMENT, API_CONTRACT, SECURITY] | 3-way parallel (Impl + API + Security) |
