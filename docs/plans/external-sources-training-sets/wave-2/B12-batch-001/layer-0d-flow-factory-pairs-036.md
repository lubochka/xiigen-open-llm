# Layer 0-D — Flow Factory Pairs (036)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B12-0D-036 — Complete API_CONTRACT reference (Round 2 factory map)

```
source_id: xiigen-b12-0d-036
pair_kind: factory_reference
```

### Round 2 factory selection map (files 025–036)

| File | Input predicates | SelectedFlows | TransitionType |
|------|-----------------|---------------|----------------|
| 025 | [API_CONTRACT, DOCUMENT] | APIContractFlow33-learned-v1 → DocumentationFlow21 | documentation |
| 026 | [IMPLEMENT, API_CONTRACT] | parallel(ImplementationFlow43, APIContractFlow33-learned-v1) | feature_addition |
| 027 | [API_CONTRACT, DOCUMENT] | APIContractFlow33-learned-v1 → DocumentationFlow21 | documentation |
| 028 | [IMPLEMENT, API_CONTRACT, DOCUMENT] | parallel(Impl, API) → Documentation | feature_addition |
| 029 | [API_CONTRACT, DOCUMENT] | APIContractFlow33-learned-v1 → DocumentationFlow21 | documentation |
| 030 | [IMPLEMENT, API_CONTRACT] | parallel(ImplementationFlow43, APIContractFlow33-learned-v1) | feature_addition |
| 031 | [API_CONTRACT, DOCUMENT] | APIContractFlow33-learned-v1 → DocumentationFlow21 | documentation |
| 032 | [API_CONTRACT, DOCUMENT] | APIContractFlow33-learned-v1 → DocumentationFlow21 | documentation |
| 033 | [IMPLEMENT, API_CONTRACT, DOCUMENT] | parallel(Impl, API) → Documentation | feature_addition |
| 034 | [API_CONTRACT, DOCUMENT, SECURITY] | API → parallel(Documentation, Security) | documentation |
| 035 | [IMPLEMENT, API_CONTRACT] | parallel(ImplementationFlow43, APIContractFlow33-learned-v1) | feature_addition |
| 036 | reference | — | — |
