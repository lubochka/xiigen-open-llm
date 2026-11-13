# Layer 0-D — Flow Factory Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-008-001 — Refactoring factory: same flows as feature_addition, different TransitionType

**Scenario:** ShopifyConnector refactoring — legacy REST API to new REST API.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
ConnectorName: ShopifyConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "probationary_accepted" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
DecisionCount: 2
TransitionType: refactoring
```

**Flows are IDENTICAL to feature_addition.** TransitionType is the only difference. The factory selects the same flows regardless of whether the operation is a new feature or a refactoring.

---

## B17-0D-008-002 — What TransitionType: refactoring changes in the factory

**TransitionType: refactoring changes:**
1. Commit message classification (refactoring tag vs feature tag)
2. Reporting category (refactoring work vs new feature work)
3. Factory metadata tagging (for analytics)

**TransitionType: refactoring does NOT change:**
1. Flow selection (same flows selected)
2. Sub-phase routing (same sub-phases)
3. DecisionCount (same count)
4. Nine-node flow execution (same nine-node process)

---

## B17-0D-008-003 — Refactoring factory metadata

```
{
  "factoryId": "IntegrationConnectorFlow43-Shopify-Refactor",
  "candidateStatus": "probationary_accepted",
  "transitionType": "refactoring",
  "originalFlow": "ShopifyConnector-v1-Legacy",
  "targetFlow": "ShopifyConnector-v2-NewAPI",
  "migrationScope": "6 files",
  "gateB": "PASS"
}
```

---

## B17-0D-008-004 — Refactoring factory decision table

| Input verb(s) | TransitionType | Factory flow selection |
|---|---|---|
| "build" only | feature_addition | IntegrationConnectorFlow43 |
| "refactor" only | refactoring | IntegrationConnectorFlow43 (same) |
| "refactor" + "build" | refactoring | IntegrationConnectorFlow43 (same) |
| "refactor" + "document" | refactoring | IntegrationConnectorFlow43 + DocumentationFlow21 |
| "refactor" + "write tests" | refactoring | IntegrationConnectorFlow43 + TestingFlow51 |

**Consistent pattern:** The flow selections are the same; only TransitionType metadata differs.
