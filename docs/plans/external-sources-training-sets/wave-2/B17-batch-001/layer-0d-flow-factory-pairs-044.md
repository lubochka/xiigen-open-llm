# Layer 0-D — Flow Factory Pairs (044)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Kafka connector

---

## B17-0D-044-001 — Kafka: SASL token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-044-002 — Kafka: OAuth + token + test factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)
- TestingFlow51(5.1)

SelectedFlows: 5 | UniqueSubPhases: 4

---

## B17-0D-044-003 — Kafka: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-044-004 — Kafka anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "orchestrate Kafka topics" | "orchestrate" fires no IMPLEMENT; no flows selected |
| Inventing EventStreamingFlow | No domain-specific flow; always deterministic_generic |
| Selecting SecurityAuditFlow34 for "SASL mechanism" alone | "SASL mechanism" without "authentication" or "token" does not fire SECURITY |
