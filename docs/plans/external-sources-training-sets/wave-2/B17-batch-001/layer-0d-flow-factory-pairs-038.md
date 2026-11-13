# Layer 0-D — Flow Factory Pairs (038)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Slack connector

---

## B17-0D-038-001 — Slack: factory selection for [IMPL, API, SEC]

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-038-002 — Slack: signing secret — factory selection unchanged

**Predicates:** ["IMPLEMENT", "API_CONTRACT"] (signing secret does not fire SECURITY)

Factory output: IntegrationConnectorFlow43(4.3) + APIContractFlow33(3.3) only.
SecurityAuditFlow34 NOT selected.

---

## B17-0D-038-003 — Slack: HMAC phrase — factory adds SECURITY

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"] (HMAC-SHA256 validate phrase fires SECURITY)

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-038-004 — Slack anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "signing secret" | "signing secret" is not a SECURITY trigger |
| Selecting IntegrationConnectorFlow43 for "integrate Slack" | "integrate" → APIContractFlow33 only |
| Inventing SlackEventFlow | No connector-specific flows; always use deterministic_generic flows |
