# Layer 0-D — Flow Factory Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-011-001 — QA factory in B17 context: QAGatingFlow73

**Scenario:** DocuSignConnector — IMPLEMENT + API_CONTRACT + DOCUMENT + QA.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "QA"]
ConnectorName: DocuSignConnector
```

**Factory output for QA predicate:**
```
{ subPhase: "7.x", flow: "QAGatingFlow73", position: "last" }
```

**Full factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { subPhase: "3.3", flow: "APIContractFlow33" },
  { subPhase: "2.1", flow: "DocumentationFlow21" },
  { subPhase: "7.x", flow: "QAGatingFlow73", position: "last" }
]
```

---

## B17-0D-011-002 — QAGatingFlow73 characteristics

**QAGatingFlow73:**
- Handles: end-to-end QA (Quality Assurance) gating, acceptance criteria validation
- Sub-phase 7.x name: QA Gating and Validation
- Position: always last (cannot run before other flows complete)
- Status: deterministic_generic

**QA gating in connector context:**
- Verify connector file count meets specification
- Run end-to-end connector test against sandbox API
- Validate API contract matches implementation
- Confirm documentation completeness

---

## B17-0D-011-003 — QAGatingFlow73 position enforcement

**Rule 21 at factory level:** The factory always emits QAGatingFlow73 as the last entry in SelectedFlows.

**Position metadata:**
```
{ subPhase: "7.x", flow: "QAGatingFlow73", position: "last", executionOrder: "after-all" }
```

**The orchestrator (Layer 1) reads the "position: last" metadata and enforces QA execution after all other flows complete.** The factory's job is to emit the correct position metadata; enforcement is the orchestrator's job.

---

## B17-0D-011-004 — Factory for QA alone: minimal factory

**Input:** ["QA"] only
**Factory output:**
```
SelectedFlows: [
  { subPhase: "7.x", flow: "QAGatingFlow73", position: "last" }
]
DecisionCount: 1
TransitionType: no contribution
```

**QA alone produces the smallest possible factory output.** One flow selected, one sub-phase, DecisionCount 1, no contribution to the codebase (QA is a validation activity, not a production activity).
