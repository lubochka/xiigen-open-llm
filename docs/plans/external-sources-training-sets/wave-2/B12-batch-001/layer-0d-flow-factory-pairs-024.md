# Layer 0-D — Flow Factory Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-024 — Complete flow factory selection map for B12

```
source_id: xiigen-b12-0d-024
pair_kind: factory_reference
```

### Flow registry for B12 (sub-phase 3.3)

| Flow | Trust state | Status | Gate B |
|------|------------|--------|--------|
| APIContractFlow33 | deterministic_generic | always available | N/A (rules-based) |
| APIContractFlow33-learned-v1 | trusted_learned | active | PASSED |
| ImplementationFlow43 | deterministic_generic | always available | N/A |
| ArchitectureFlow31 | deterministic_generic | always available | N/A |
| DocumentationFlow21 | deterministic_generic | always available | N/A |
| SecurityAuditFlow34 | deterministic_generic | always available | N/A |
| AuthorizationFlow34 | deterministic_generic | always available | N/A |

### Factory selection algorithm

1. Check predicate list for IMPLEMENT:
   YES → include ImplementationFlow43 at front of FlowOrder; TransitionType = feature_addition
   NO  → do not include; TransitionType = documentation

2. Check for ARCHITECTURE:
   YES → include ArchitectureFlow31 (before APIContractFlow33 in FlowOrder)
   NO  → do not include

3. Check for API_CONTRACT:
   YES → check flow registry for trusted_learned or probationary_accepted
         FOUND → use learned flow
         NOT FOUND → use APIContractFlow33 (deterministic_generic)
   NO  → do not include APIContractFlow33

4. Check for DOCUMENT:
   YES → include DocumentationFlow21 (last, after contract flows)
   NO  → do not include

5. Check for SECURITY:
   YES → include SecurityAuditFlow34 (parallel with DOCUMENT)
   NO  → do not include

6. Check for AUTHORIZATION:
   YES → include AuthorizationFlow34 (separate decision from SECURITY, parallel)
   NO  → do not include

### Complete factory output table

| Predicates | SelectedFlows | TransitionType |
|-----------|---------------|----------------|
| [API_CONTRACT] | [APIContractFlow33] | documentation |
| [API_CONTRACT, DOCUMENT] | [APIContractFlow33, DocumentationFlow21] | documentation |
| [API_CONTRACT, DOCUMENT, SECURITY] | [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | documentation |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21] | documentation |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21] | feature_addition |
| [IMPLEMENT, ARCHITECTURE, API_CONTRACT, DOCUMENT] | [ImplementationFlow43, ArchitectureFlow31, APIContractFlow33, DocumentationFlow21] | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | feature_addition |
| [DOCUMENT] | [DocumentationFlow21] | documentation |
