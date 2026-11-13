# Layer 0-D — Flow Factory Pairs (045)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: RabbitMQ connector

---

## B17-0D-045-001 — RabbitMQ: basic factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-045-002 — RabbitMQ: vhost token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-045-003 — RabbitMQ: OAuth + token + document factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "DOCUMENT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)
- DocumentationFlow21(2.1)

SelectedFlows: 5 | UniqueSubPhases: 4

---

## B17-0D-045-004 — RabbitMQ anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "coordinate RabbitMQ delivery" | "coordinate" fires no IMPLEMENT |
| Inventing MessageBrokerFlow | No domain-specific flows in B17 |
| Selecting AuthorizationFlow34 for "vhost access" without "OAuth" | No "OAuth" → no AUTHORIZATION |
