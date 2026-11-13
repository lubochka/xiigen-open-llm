# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-003-001 — API_CONTRACT alone: documentation TransitionType, routes to 3.3

**Scenario:** "Integrate the Slack messaging API for sending channel notifications." — no build verb.

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
DecisionCount: 1
```

**Orchestration plan:**
```
Phase 1: APIContractFlow33 (3.3)
  — Generate Slack API contract specification
  — No other flows active
```

---

## B17-L1-003-002 — Single-flow orchestration: simplest case

**Single-flow orchestration is the simplest possible orchestration:**
- One flow, no dependencies, no parallel execution, no QA gate
- TransitionType: documentation (API_CONTRACT alone)
- No implementation work product generated

**Orchestration output:**
```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["APIContractFlow33"], parallel: false }
  ],
  totalSteps: 1,
  transitionType: "documentation",
  qaGating: false
}
```

---

## B17-L1-003-003 — Comparison: API_CONTRACT alone vs IMPLEMENT + API_CONTRACT

**API_CONTRACT alone:**
- Orchestration: 1 step, 1 flow
- Work product: API specification document
- TransitionType: documentation
- No code produced

**IMPLEMENT + API_CONTRACT:**
- Orchestration: 2 steps (4.3 then 3.3 or parallel)
- Work product: connector code + API specification
- TransitionType: feature_addition
- Code produced

**The orchestrator's workload is dramatically different** even though both scenarios involve API_CONTRACT.

---

## B17-L1-003-004 — Documentation TransitionType orchestration properties

**When TransitionType is documentation:**
- No nine-node IntegrationConnectorFlow43 execution (no connector build)
- APIContractFlow33 runs as a standalone documentation generator
- Output: specification artifacts, not code files
- Commit message template: "docs: add Slack API contract specification"
- No Gate B on implementation quality (no implementation to gate)

**Gate B still applies to documentation:** The API specification must be complete and accurate. But the Gate B criteria are documentation-specific, not implementation-specific.
