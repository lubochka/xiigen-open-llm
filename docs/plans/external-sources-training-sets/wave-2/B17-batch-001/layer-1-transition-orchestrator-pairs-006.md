# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-006-001 — Refactoring: IMPLEMENT + API_CONTRACT, refactoring TransitionType

**Scenario:** ShopifyConnector — legacy to new REST API refactoring.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3)
]
TransitionType: refactoring
```

**Orchestration plan:**
```
Phase 1: IntegrationConnectorFlow43 (4.3) — refactor connector to new API
Phase 2: APIContractFlow33 (3.3) — update API contract to reflect new API
```

**Ordering note:** For refactoring, the implementation refactor (4.3) runs first, then the API contract is updated to reflect the refactored interface (3.3). This is the same ordering as feature_addition, but the work content differs.

---

## B17-L1-006-002 — Refactoring orchestration vs feature_addition orchestration

**Same flows, same ordering. Different work content:**

| Property | feature_addition | refactoring |
|---|---|---|
| Flows selected | IntegrationConnectorFlow43, APIContractFlow33 | IntegrationConnectorFlow43, APIContractFlow33 |
| Sub-phases | 4.3, 3.3 | 4.3, 3.3 |
| DecisionCount | 2 | 2 |
| TransitionType | feature_addition | refactoring |
| Work in 4.3 | Build new connector | Migrate existing connector |
| Work in 3.3 | Define new API contract | Update existing API contract |

---

## B17-L1-006-003 — Refactoring orchestration output

```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["IntegrationConnectorFlow43"], parallel: false,
      workType: "refactoring" },
    { step: 2, flows: ["APIContractFlow33"], parallel: false,
      workType: "contract-update" }
  ],
  transitionType: "refactoring",
  migrationScope: "6 files migrated"
}
```

---

## B17-L1-006-004 — Refactoring in nine-node flow: N01 input difference

**For refactoring, N01 (input node) receives additional context:**
- Original connector: ShopifyConnector-v1 (legacy)
- Target connector: ShopifyConnector-v2 (new REST API)
- Migration scope: 6 files

**N02 (domain statement) for refactoring:**
"Refactor ShopifyConnector from legacy Shopify API v1 to Shopify REST Admin API 2024-01. 6 existing files will be migrated. No new files expected."

This is different from a feature_addition N02 which would describe building new functionality from scratch.
