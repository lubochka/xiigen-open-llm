# Layer 0-B — Router Pairs (060)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-060-001 — Complete routing decision table for Round 4 (files 049–060)

```
source_id: xiigen-b12-0b-060-001
pair_kind: router_reference
```

### Routing patterns confirmed in files 049–060

| Topic | Predicates | Routes | TransitionType |
|-------|-----------|--------|----------------|
| OpenAPISpec (define) | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| OpenAPISpec (write YAML) | IMPLEMENT+API_CONTRACT+DOCUMENT | 4.3+3.3+2.1 | feature_addition |
| OpenAPISpec (design+auth) | ARCHITECTURE+API_CONTRACT+DOCUMENT+SECURITY | 3.1+3.3+2.1+3.4 | documentation |
| APIGatewayContract (define) | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| APIGatewayContract (design+auth) | ARCHITECTURE+API_CONTRACT+DOCUMENT+SECURITY | 3.1+3.3+2.1+3.4 | documentation |
| ServiceMeshAPI (TLS only) | API_CONTRACT | 3.3 | documentation |
| ServiceMeshAPI (auth) | ARCHITECTURE+API_CONTRACT+DOCUMENT+SECURITY | 3.1+3.3+2.1+3.4 | documentation |
| CircuitBreakerAPI | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| RetryPolicyAPI (idempotent) | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| TimeoutAPI | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| APIMonitoring | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| APIAnalytics | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| SDKGeneration (publish) | API_CONTRACT | 3.3 | documentation |
| MockServerAPI (build+spec) | IMPLEMENT+API_CONTRACT+DOCUMENT | 4.3+3.3+2.1 | feature_addition |
| ContractTestAPI | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |

---

## B12-0B-060-002 — Complete B12 routing decision table (all rounds consolidated)

```
source_id: xiigen-b12-0b-060-002
pair_kind: router_reference
```

### Master routing decision table for B12

| Predicates | Routes | DecisionCount | TransitionType |
|-----------|--------|--------------|----------------|
| [API_CONTRACT] | [3.3] | 1 | documentation |
| [API_CONTRACT, DOCUMENT] | [3.3, 2.1] | 2 | documentation |
| [API_CONTRACT, SECURITY] | [3.3, 3.4] | 2 | documentation |
| [API_CONTRACT, DOCUMENT, SECURITY] | [3.3, 2.1, 3.4] | 3 | documentation |
| [ARCHITECTURE, API_CONTRACT] | [3.1, 3.3] | 2 | documentation |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | [3.1, 3.3, 2.1] | 3 | documentation |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] | [3.1, 3.3, 2.1, 3.4] | 4 | documentation |
| [IMPLEMENT, API_CONTRACT] | [4.3, 3.3] | 2 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | [4.3, 3.3, 2.1] | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, SECURITY] | [4.3, 3.3, 3.4] | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | [4.3, 3.3, 2.1, 3.4] | 4 | feature_addition |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY, IMPLEMENT] | [3.1, 3.3, 2.1, 3.4, 4.3] | 5 | feature_addition |

FlowOrder rules:
  ARCHITECTURE (3.1) always executes first when present
  IMPLEMENT (4.3) and API_CONTRACT (3.3) execute in parallel
  DOCUMENT (2.1) and SECURITY (3.4) execute in parallel (after API_CONTRACT completes)
  QAFlow always executes last
