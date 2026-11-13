# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Stripe payment connector

---

## B17-L1-013-001 — StripePaymentConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3)
]
TransitionType: feature_addition
ConnectorName: StripePaymentConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build StripePaymentConnector
Step 2: APIContractFlow33 (3.3) — document Stripe payment API contract

Sequential (Step 2 can start when Step 1 completes, or in parallel — no strict code dependency)
No TEST → no TestingFlow51 dependency
No QA → orchestration ends after Step 2
```

**TransitionId:** `transition-B17-L1-013-4.3-feature_addition`

---

## B17-L1-013-002 — StripePaymentConnector: four-flow orchestration (OAuth)

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
No QA → orchestration ends after Step 2.
```

---

## B17-L1-013-003 — Orchestration ordering rationale for Stripe

**Why 4.3 runs first:**
IntegrationConnectorFlow43 generates the StripePaymentConnector code. The security audit, authorization review, and API contract documentation can reference the connector interface (which is known upfront) but don't strictly depend on the code being complete.

**Why 3.3, 3.4, 3.4 can run in parallel:**
- SecurityAuditFlow34: reviews security posture of the connector design
- AuthorizationFlow34: reviews OAuth protocol implementation
- APIContractFlow33: documents the payment API contract

All three are review and documentation activities that can proceed concurrently after the connector design is finalized (Step 1 provides the design inputs).

---

## B17-L1-013-004 — TransitionType determination for Stripe connector

| Input pattern | TransitionType |
|---|---|
| "Build the Stripe connector" | feature_addition |
| "Refactor the Stripe connector" | refactoring |
| "Integrate the Stripe API" | documentation |
| "Document the Stripe API contract" | documentation |

**TransitionType is set by the dominant verb:**
- Refactor/restructure/rewrite → refactoring (highest priority)
- Build/implement/write/create/develop → feature_addition
- Document/spec → documentation
- API_CONTRACT alone (no IMPLEMENT) → documentation
