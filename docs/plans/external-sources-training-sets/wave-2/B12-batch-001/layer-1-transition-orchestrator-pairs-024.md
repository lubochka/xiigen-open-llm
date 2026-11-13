# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-024 — Complete orchestration reference for B12

```
source_id: xiigen-b12-l1-024
pair_kind: transition_orchestrator_reference
```

### FlowOrder rules (B12)

Priority ordering (highest to lowest):
1. IMPLEMENT → ImplementationFlow43 (4.3) — executes first or parallel with API_CONTRACT
2. ARCHITECTURE → ArchitectureFlow31 (3.1) — executes before API_CONTRACT when present
3. API_CONTRACT → APIContractFlow33 (3.3) — primary B12 flow
4. DOCUMENT → DocumentationFlow21 (2.1) — executes after API_CONTRACT
5. SECURITY → SecurityAuditFlow34 (3.4) — parallel with DOCUMENT
6. AUTHORIZATION → AuthorizationFlow34 (3.4) — parallel with DOCUMENT and SECURITY
7. QA — always LAST

### Parallel eligibility rules

- IMPLEMENT and API_CONTRACT: parallel when both present (neither depends on the other)
- ARCHITECTURE and API_CONTRACT: sequential (ARCHITECTURE first, API_CONTRACT uses architecture decisions)
- DOCUMENT and SECURITY: parallel (both depend on contract flows, neither depends on each other)
- QA: never parallel (depends on ALL preceding flows)

### TransitionType effect on orchestration

| TransitionType | ImplementationFlow43? | Parallel in Phase1? |
|---------------|----------------------|---------------------|
| documentation | NO | NO (all sequential) |
| feature_addition | YES | YES (IMPLEMENT parallel with API_CONTRACT) |

### Complete B12 orchestration patterns

| Predicates | Pattern | Total flows |
|-----------|---------|------------|
| [API_CONTRACT] | API_CONTRACT → QA | 1 |
| [API_CONTRACT, DOCUMENT] | API_CONTRACT → DOCUMENT → QA | 2 |
| [API_CONTRACT, DOCUMENT, SECURITY] | API_CONTRACT → parallel(DOCUMENT, SECURITY) → QA | 3 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | ARCH → API_CONTRACT → DOCUMENT → QA | 3 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | parallel(IMPL, API_CONTRACT) → DOCUMENT → QA | 3 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | parallel(IMPL, API_CONTRACT) → parallel(DOC, SEC) → QA | 4 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] | ARCH → API_CONTRACT → parallel(DOC, SEC) → QA | 4 |

### v6 ordering violations (B12 context)

1. QA not last → always a corruption
2. DOCUMENT before API_CONTRACT → always a corruption (documentation depends on contract)
3. SECURITY before DOCUMENT (as sequential dependency) → ordering violation
4. ARCHITECTURE after API_CONTRACT → ordering violation (architecture should be first)
