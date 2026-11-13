# Layer 1 — Transition Orchestrator Pairs (054)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Netlify connector

---

## B17-L1-054-001 — Netlify: orchestration for [IMPL, API, SEC]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build NetlifyConnector
Step 2: APIContractFlow33 (3.3) — Netlify Build API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — personal access token audit [after Step 1]
```

---

## B17-L1-054-002 — Netlify: OAuth + token orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build NetlifyConnector
Step 2: APIContractFlow33 (3.3)
Step 3: SecurityAuditFlow34 (3.4) [parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) [parallel with Step 3]
```

---

## B17-L1-054-003 — Netlify: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Netlify Build API contract
```

TransitionType: documentation

---

## B17-L1-054-004 — Netlify: webhook only orchestration (no SECURITY)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build NetlifyConnector with webhook handlers
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
```

Note: SecurityAuditFlow34 absent when only webhook is present without token or HMAC phrase.
