# Layer 0-D — Flow Factory Pairs (039)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: PagerDuty connector

---

## B17-0D-039-001 — PagerDuty: factory selection for [IMPL, API, SEC]

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-039-002 — PagerDuty: factory for [IMPL, API, TEST, DOC]

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- TestingFlow51(5.1)
- DocumentationFlow21(2.1)

SelectedFlows: 4 flows | UniqueSubPhases: 4

---

## B17-0D-039-003 — PagerDuty: "integrate" factory output

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-039-004 — PagerDuty anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "integrate PagerDuty" | Rule 1: "integrate" → APIContractFlow33 only |
| Selecting SecurityAuditFlow34 without "token" or HMAC phrase | SECURITY must be compiled first; factory reflects predicates |
| Placing TestingFlow51 before IntegrationConnectorFlow43 | TEST hard depends on IMPLEMENT completing; TestingFlow51 runs after 4.3 |
