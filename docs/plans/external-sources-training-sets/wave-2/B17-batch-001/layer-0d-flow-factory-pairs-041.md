# Layer 0-D — Flow Factory Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Splunk connector

---

## B17-0D-041-001 — Splunk: token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-041-002 — Splunk: six-predicate factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)
- TestingFlow51(5.1)
- DocumentationFlow21(2.1)

SelectedFlows: 6 | UniqueSubPhases: 5

---

## B17-0D-041-003 — Splunk: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-041-004 — Splunk anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "HEC endpoint" alone | HEC (HTTP Event Collector) acronym is not a SECURITY trigger |
| Inventing LogIngestionFlow | No domain-specific flow; always deterministic_generic |
| Selecting IntegrationConnectorFlow43 for "integrate Splunk HEC API" | "integrate" → APIContractFlow33 only |
