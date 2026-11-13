# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-012-001 — v6 ordering violations: complete catalog for B17

| v6 violation | Error | Correct |
|---|---|---|
| QA not last | QAGatingFlow73 before any other flow | QAGatingFlow73 always last |
| Test before build | TestingFlow51 before IntegrationConnectorFlow43 | 4.3 → 5.1 (sequential) |
| Build after document | DocumentationFlow21 before IntegrationConnectorFlow43 when doc is spec-driven | 4.3 ‖ 2.1 (parallel) is valid |
| Security before build | SecurityAuditFlow34 before IntegrationConnectorFlow43 | 4.3 → 3.4 (security reviews implementation) |
| API contract before build (strict) | APIContractFlow33 before IntegrationConnectorFlow43 when contract depends on implementation | Flexible — can be parallel if spec-driven |

---

## B17-L1-012-002 — Correct canonical ordering table for B17

| Flows present | Canonical step sequence |
|---|---|
| [4.3] | Step 1: 4.3 |
| [3.3] | Step 1: 3.3 |
| [4.3, 3.3] | Step 1: 4.3 → Step 2: 3.3 |
| [4.3, 3.3, 3.4] | Step 1: 4.3 → Step 2: 3.3 ‖ 3.4 |
| [4.3, 3.3, 5.1] | Step 1: 4.3 → Step 2: 3.3 ‖ 5.1 |
| [4.3, 2.1, 3.3] | Step 1: 4.3 ‖ 2.1 → Step 2: 3.3 |
| [4.3, 3.3, 3.4, 5.1] | Step 1: 4.3 → Step 2: 3.3 ‖ 3.4 → Step 3: 5.1 |
| [4.3, 3.3, 2.1, 7.x] | Step 1: 4.3 ‖ 2.1 → Step 2: 3.3 → Step 3: 7.x (last) |
| [4.3, 3.3, 3.4, 3.4, 5.1] | Step 1: 4.3 → Step 2: 3.3 ‖ 3.4 ‖ 3.4 → Step 3: 5.1 |

---

## B17-L1-012-003 — Orchestration dependency rules (summary)

**Hard dependencies (must be sequential):**
- 5.1 depends on 4.3 completing (tests need implementation)
- 7.x (QA) depends on ALL other flows completing (absolute last)

**Soft parallels (can be concurrent):**
- 2.1 (documentation) can be concurrent with 4.3 (implementation)
- 3.3 (API contract) can be concurrent with 4.3 in spec-driven scenarios
- 3.4 SecurityAuditFlow34 and AuthorizationFlow34 are always parallel-eligible with each other

**No dependencies:**
- 3.3 and 2.1 have no dependency on each other
- 3.4 and 3.3 have no dependency on each other

---

## B17-L1-012-004 — Orchestrator output format summary

```
OrchestratorOutput: {
  scenario: "B17",
  connectorName: "<ConnectorName>",
  sequence: [
    { step: N, flows: ["FlowId1", ...], parallel: bool }
  ],
  transitionType: "<type>",
  decisionCount: N,
  filesGenerated: N,
  qaGating: bool,
  commitMetadata: { prefix: "<prefix>:", subject: "<description>" }
}
```
