# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-007-001 — IMPLEMENT + API_CONTRACT + SECURITY: three flows

**Scenario:** GitHubWebhookConnector — build with signature validation.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4)
]
```

**Orchestration plan:**
```
Phase 1: IntegrationConnectorFlow43 (4.3) — build webhook connector
Phase 2 (parallel):
  ‖ APIContractFlow33 (3.3) — document webhook endpoint contract
  ‖ SecurityAuditFlow34 (3.4) — security review for signature validation
```

---

## B17-L1-007-002 — Why SecurityAuditFlow34 can run in parallel with APIContractFlow33

**SecurityAuditFlow34 and APIContractFlow33 are independent:**
- SecurityAuditFlow34 reviews the security implementation (token storage, HMAC validation)
- APIContractFlow33 documents the API endpoint surface (webhook URL, payload schema)
- Neither depends on the other's output

**Both can begin after IntegrationConnectorFlow43 completes.** The connector must exist before either security review or API documentation can be meaningful.

---

## B17-L1-007-003 — Three-flow orchestration output

```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["IntegrationConnectorFlow43"], parallel: false },
    { step: 2, flows: ["APIContractFlow33", "SecurityAuditFlow34"], parallel: true }
  ],
  dependencies: [
    { flow: "SecurityAuditFlow34", dependsOn: "IntegrationConnectorFlow43" },
    { flow: "APIContractFlow33", dependsOn: null }  // can start any time
  ],
  transitionType: "feature_addition"
}
```

---

## B17-L1-007-004 — SecurityAuditFlow34 in webhook context

**What SecurityAuditFlow34 produces for webhook connector:**
- HMAC (Hash-based Message Authentication Code) signature verification checklist
- Webhook secret storage guidance (environment variable, secrets manager)
- Replay attack (sending the same request twice) prevention recommendations
- Rate limiting guidance for webhook endpoint

**These outputs are reviewed alongside the connector code produced by IntegrationConnectorFlow43.** The security audit flow reads the implementation output and produces a security assessment.
