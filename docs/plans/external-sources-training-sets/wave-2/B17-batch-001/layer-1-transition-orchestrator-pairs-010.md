# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-010-001 — TransitionType summary table for B17

| Scenario type | Predicates | TransitionType | Orchestration pattern |
|---|---|---|---|
| Build new connector | [IMPLEMENT, API_CONTRACT] | feature_addition | Serial: 4.3 → 3.3 |
| Refactor connector | [IMPLEMENT, API_CONTRACT] + refactor | refactoring | Serial: 4.3 → 3.3 |
| Integrate only | [API_CONTRACT] | documentation | Single: 3.3 |
| Document only | [DOCUMENT] | documentation | Single: 2.1 |
| Build + secure | [IMPLEMENT, API_CONTRACT, SECURITY] | feature_addition | 4.3 → 3.3 ‖ 3.4 |
| Build + OAuth | [IMPLEMENT, API_CONTRACT, AUTHORIZATION] | feature_addition | 4.3 → 3.3 ‖ 3.4 |
| Build + test | [IMPLEMENT, API_CONTRACT, TEST] | feature_addition | 4.3 → 3.3 → 5.1 |
| Build + document | [IMPLEMENT, API_CONTRACT, DOCUMENT] | feature_addition | 4.3 ‖ 2.1 → 3.3 |
| Build + QA | [IMPLEMENT, API_CONTRACT, QA] | feature_addition | 4.3 → 3.3 → 7.x |
| QA only | [QA] | no contribution | Single: 7.x |
| Test only | [TEST] | no contribution | Single: 5.1 |

---

## B17-L1-010-002 — TransitionType determination: orchestrator perspective

**The orchestrator reads TransitionType from the factory output, not from re-analyzing the input.**

**Orchestrator TransitionType handling:**
- feature_addition → standard orchestration, normal commit
- refactoring → orchestration identical, commit tagged as refactoring
- documentation → lightweight orchestration (no 4.3 flow)
- no contribution → QA or test-only orchestration (no code production)

**The orchestrator does not recompute TransitionType.** It trusts the factory's TransitionType output.

---

## B17-L1-010-003 — Commit message templates by TransitionType

| TransitionType | Commit message prefix | Example |
|---|---|---|
| feature_addition | feat: | feat: add PayPal payment connector with capture and refund APIs |
| refactoring | refactor: | refactor: migrate ShopifyConnector to Admin REST API 2024-01 |
| documentation | docs: | docs: add Slack API contract specification |
| no contribution | chore: | chore: QA validation run for DocuSign connector |

---

## B17-L1-010-004 — Orchestrator handles TransitionType in output metadata

```
OrchestratorOutput: {
  completedFlows: ["IntegrationConnectorFlow43", "APIContractFlow33"],
  transitionType: "feature_addition",
  commitMetadata: {
    prefix: "feat:",
    scope: "connector",
    subject: "add PayPal payment connector with capture and refund APIs"
  },
  filesGenerated: 8,
  decisionCount: 2
}
```
