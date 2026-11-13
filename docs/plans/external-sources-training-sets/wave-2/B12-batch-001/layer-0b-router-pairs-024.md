# Layer 0-B — Router Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-024 — Complete routing decision table for B12

```
source_id: xiigen-b12-0b-024
pair_kind: router_reference
```

### Sub-phase routing map

| Predicate | Routes to | Flow | Notes |
|-----------|-----------|------|-------|
| API_CONTRACT | 3.3 | APIContractFlow33 | Primary B12 sub-phase |
| IMPLEMENT | 4.3 | ImplementationFlow43 | Code creation sub-phase |
| ARCHITECTURE | 3.1 | ArchitectureFlow31 | System design sub-phase |
| DOCUMENT | 2.1 | DocumentationFlow21 | Documentation sub-phase |
| SECURITY | 3.4 | SecurityAuditFlow34 | Security and compliance |
| AUTHORIZATION | 3.4 | AuthorizationFlow34 | Separate 3.4 decision |

### FlowOrder rules

1. ARCHITECTURE (3.1) executes FIRST when present
2. IMPLEMENT (4.3) executes after ARCHITECTURE, before API_CONTRACT
3. API_CONTRACT (3.3) executes after IMPLEMENT (if present)
4. DOCUMENT (2.1), SECURITY (3.4), AUTHORIZATION (3.4) execute LAST (parallel-eligible)
5. QA checkpoint is ALWAYS last

### Full routing decision table

| CompiledPredicateIds | Routes | DecisionCount | TransitionType |
|---------------------|--------|---------------|----------------|
| [API_CONTRACT] | 3.3 | 1 | documentation |
| [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | 2 | documentation |
| [API_CONTRACT, DOCUMENT, SECURITY] | 3.3 + 2.1 + 3.4 | 3 | documentation |
| [API_CONTRACT, DOCUMENT, AUTHORIZATION] | 3.3 + 2.1 + 3.4 | 3 | documentation |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | 3.1 + 3.3 + 2.1 | 3 | documentation |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3 + 3.3 + 2.1 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | 4.3 + 3.3 + 2.1 + 3.4 | 4 | feature_addition |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] | 3.1 + 3.3 + 2.1 + 3.4 | 4 | documentation |
| [IMPLEMENT, ARCHITECTURE, API_CONTRACT, DOCUMENT] | 4.3 + 3.1 + 3.3 + 2.1 | 4 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | 4.3 + 3.3 + 2.1 + 3.4 + 3.4 | 5 | feature_addition |

### v6 corruption patterns for routing

v6-01: API_CONTRACT present in request but missing from CompiledPredicateIds → 3.3 missing from routes
v6-02: SECURITY present in request ("authentication") but missing → 3.4 missing from routes
v6-03: Multiple predicates → DecisionCount collapsed to 1 (only primary sub-phase) → CORRUPT

### Three B12 routing anti-patterns

1. Treating "database schema" as API_CONTRACT → wrongly adds 3.3 to routes
2. Treating "authentication" in API context as not-SECURITY → misses 3.4 routing
3. Adding only 3.3 when [API_CONTRACT, DOCUMENT] → misses 2.1 routing (DecisionCount=1 wrong)
