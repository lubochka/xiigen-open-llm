# Layer 0-D — Flow Factory Pairs (040)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Datadog connector

---

## B17-0D-040-001 — Datadog: API key factory (no SECURITY)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

SecurityAuditFlow34 NOT selected — "API key" does not compile SECURITY.

---

## B17-0D-040-002 — Datadog: agent token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-040-003 — Datadog: refactor factory output

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

TransitionType: refactoring (factory output is same; TransitionType is metadata, not a flow selector).

---

## B17-0D-040-004 — Datadog anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "Datadog API key" | API key (no token) does not compile SECURITY; factory cannot select what was not compiled |
| Inventing MonitoringFlow for Datadog | No domain-specific flows; always deterministic_generic |
| Selecting BackendServiceFlow41 for "build Datadog connector" | "connector" vocabulary routes IMPLEMENT to 4.3, not 4.1 |
