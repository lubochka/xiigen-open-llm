# Layer 1 — Transition Orchestrator Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Plaid banking connector orchestration

---

## B17-L1-027-001 — PlaidBankingConnector orchestration: [IMPL, API, SEC, AUTH]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build PlaidBankingConnector [parallel with Step 2]
Step 2: DocumentationFlow21 (2.1) — absent (no DOCUMENT predicate)
Step 2: APIContractFlow33 (3.3) — finalize Plaid API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — OAuth token exchange security review [after Step 1, parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth authorization flow review [after Step 1, parallel with Step 3]

Steps 3 and 4 can run in parallel after Step 1.
```

---

## B17-L1-027-002 — Plaid orchestration: [IMPL, SEC] (4.1 path)

```
Step 1: BackendServiceFlow41 (4.1) — implement Plaid link token creation service
Step 2: SecurityAuditFlow34 (3.4) — token authentication security review [after Step 1]
TransitionType: feature_addition
```

BackendServiceFlow41 is used when no integration vocabulary is present. Sequential orchestration.

---

## B17-L1-027-003 — Plaid: TestingFlow51 dependency if TEST added

If TEST predicate present for Plaid:
```
Step 1: IntegrationConnectorFlow43 (4.3) — build PlaidBankingConnector
Step 2: ... other flows parallel or sequential after Step 1 ...
Step N: TestingFlow51 (5.1) — MUST be after IntegrationConnectorFlow43 [hard sequential dependency]
```

TestingFlow51 cannot start before IntegrationConnectorFlow43 completes. This rule is universal — applies to all B17 connectors.
