# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-011-001 — Canonical B17 orchestration patterns (one-verb through four-verb)

**One-verb (API_CONTRACT only):**
"Integrate the Slack API."
```
Step 1: APIContractFlow33 (3.3)
TransitionType: documentation
```

**Two-verb (IMPLEMENT + API_CONTRACT):**
"Build the PayPal connector."
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2: APIContractFlow33 (3.3)
TransitionType: feature_addition
```

**Three-verb A (IMPLEMENT + API_CONTRACT + SECURITY):**
"Build the GitHub webhook connector with signature validation."
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2: APIContractFlow33 (3.3) ‖ SecurityAuditFlow34 (3.4)
TransitionType: feature_addition
```

**Four-verb (IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION):**
"Build the Auth0 OAuth connector with token security."
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2: APIContractFlow33 (3.3) ‖ SecurityAuditFlow34 (3.4) ‖ AuthorizationFlow34 (3.4)
TransitionType: feature_addition
```

---

## B17-L1-011-002 — Three-verb B (IMPLEMENT + API_CONTRACT + TEST)

**"Build the SendGrid connector and write integration tests."**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build
Step 2: APIContractFlow33 (3.3) ‖ TestingFlow51 (5.1) — parallel
  Note: tests written concurrently; test execution after Step 1
TransitionType: feature_addition
```

**Three-verb C (IMPLEMENT + API_CONTRACT + DOCUMENT):**
**"Build the Plaid connector and document the interface."**
```
Step 1: IntegrationConnectorFlow43 (4.3) ‖ DocumentationFlow21 (2.1) — parallel
Step 2: APIContractFlow33 (3.3) — after build
TransitionType: feature_addition
```

---

## B17-L1-011-003 — v6 ordering violations in B17

**v6 ordering violation 1: QA not last**
```
WRONG: Step 1: QAGatingFlow73, Step 2: IntegrationConnectorFlow43
CORRECT: Step 1: IntegrationConnectorFlow43, Step 2: QAGatingFlow73 (last-absolute)
```

**v6 ordering violation 2: TestingFlow51 before IntegrationConnectorFlow43**
```
WRONG: Step 1: TestingFlow51 (5.1), Step 2: IntegrationConnectorFlow43 (4.3)
CORRECT: Step 1: IntegrationConnectorFlow43 (4.3), Step 2: TestingFlow51 (5.1)
```

**v6 ordering violation 3: parallel flows run sequentially when parallel is optimal**
```
NOT WRONG (just suboptimal): Step 1: SecurityAuditFlow34, Step 2: AuthorizationFlow34
PREFERRED: Step 1: SecurityAuditFlow34 ‖ AuthorizationFlow34 (parallel)
```
Note: Sequential execution of parallel-eligible flows is not a corruption, just inefficient.

---

## B17-L1-011-004 — Orchestration invariants for B17

1. IntegrationConnectorFlow43 (4.3) always runs before TestingFlow51 (5.1)
2. QAGatingFlow73 (7.x) always runs last when present
3. SecurityAuditFlow34 and AuthorizationFlow34 can always run in parallel
4. DocumentationFlow21 can always run parallel to IntegrationConnectorFlow43
5. APIContractFlow33 has no strict dependency (can run at any step)
