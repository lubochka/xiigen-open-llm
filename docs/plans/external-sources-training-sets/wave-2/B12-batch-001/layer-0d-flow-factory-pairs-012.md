# Layer 0-D — Flow Factory Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-012-001 — Complete B12 factory map table (all predicate combos → factory → TransitionType)

```
source_id: xiigen-b12-0d-012-001
pair_kind: factory_reference
coverage_status: reference
```

### Complete B12 Factory Map

| CompiledPredicateIds | Factory Type | FlowOrder | TransitionType | DecisionCount |
|---------------------|--------------|-----------|----------------|---------------|
| [API_CONTRACT] | deterministic_generic | [APIContractFlow33] | documentation | 1 |
| [DOCUMENT] | deterministic_generic | [DocumentationFlow21] | documentation | 1 |
| [ARCHITECTURE] | deterministic_generic | [ArchitectureFlow31] | documentation | 1 |
| [IMPLEMENT] | deterministic_generic | [ImplementationFlow43] | feature_addition | 1 |
| [API_CONTRACT, DOCUMENT] | trusted_learned (when available) / deterministic_generic | [APIContractFlow33, DocumentationFlow21] | documentation | 2 |
| [ARCHITECTURE, DOCUMENT] | deterministic_generic | [ArchitectureFlow31, DocumentationFlow21] | documentation | 2 |
| [IMPLEMENT, API_CONTRACT] | deterministic_generic | [ImplementationFlow43, APIContractFlow33] | feature_addition | 2 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | probationary_accepted / deterministic_generic | [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21] | documentation | 3 |
| [API_CONTRACT, DOCUMENT, SECURITY] | deterministic_generic | [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | documentation | 3 |
| [API_CONTRACT, DOCUMENT, AUTHORIZATION] | deterministic_generic | [APIContractFlow33, DocumentationFlow21, AuthorizationFlow34] | documentation | 3 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | deterministic_generic | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21] | feature_addition | 3 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] | deterministic_generic | [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | documentation | 4 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | deterministic_generic | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] | feature_addition | 4 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, AUTHORIZATION] | deterministic_generic | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, AuthorizationFlow34] | feature_addition | 4 |
| [API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | deterministic_generic | [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34] | documentation | 4 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | deterministic_generic | [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34] | feature_addition | 5 |

---

## B12-0D-012-002 — Factory type selection rules

```
source_id: xiigen-b12-0d-012-002
pair_kind: factory_reference
coverage_status: reference
```

Factory type hierarchy and selection:

trusted_learned → selected when:
  - Predicate pattern has registered learned flow
  - Gate B passed + 10+ production uses without error
  - Available for: [API_CONTRACT, DOCUMENT] (most trained pattern)

probationary_accepted → selected when:
  - Learned flow candidate available
  - Gate B passed but fewer than 10 production uses
  - Available for: [API_CONTRACT, DOCUMENT], [ARCHITECTURE, API_CONTRACT, DOCUMENT]

deterministic_generic → selected when:
  - No learned flow registered for predicate pattern
  - probationary_rejected → fallback
  - Default for all multi-predicate combinations in early training stages

probationary_rejected → never selected:
  - Rejected flows archive only
  - Fallback to deterministic_generic immediately upon rejection

---

## B12-0D-012-003 — Factory selection decision tree

```
source_id: xiigen-b12-0d-012-003
pair_kind: factory_reference
coverage_status: reference
```

Decision tree for factory type selection:

1. Is there a trusted_learned flow for this predicate combination?
   YES → select trusted_learned flow
   NO → continue

2. Is there a probationary_accepted flow for this predicate combination?
   YES → select probationary_accepted flow
   NO → continue

3. Is there a probationary_rejected flow for this predicate combination?
   YES → do NOT select rejected flow → fallback to deterministic_generic
   NO → continue

4. No learned flow available → select deterministic_generic
   Use sub_phase_hint to determine primary flow
   Compose FlowOrder from predicate-to-flow registry

---

## B12-0D-012-004 — Factory: GapHandler absent in B12

```
source_id: xiigen-b12-0d-012-004
pair_kind: factory_reference
coverage_status: reference
```

B12 has no GapHandler (Core Rule 22). Factory implications:

  - No GapHandlerFlow in any B12 FlowOrder
  - No fallback to GapHandler when predicate confidence is low
  - If predicate detection confidence is below threshold:
    B12 behavior: use deterministic_generic with best available sub_phase_hint
    Non-B12 behavior: might trigger GapHandler

  Factory output will never include GapHandlerFlow for B12 requests.
  If a factory result includes GapHandlerFlow for a B12 request → factory error.

DomainId validation:
  All B12 factory outputs must have DomainId: "software-development"
  Any factory output with different DomainId for B12 requests → factory error.
