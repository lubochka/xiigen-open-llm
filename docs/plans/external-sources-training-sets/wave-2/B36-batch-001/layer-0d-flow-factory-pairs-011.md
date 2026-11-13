# Layer 0-D — Flow Factory Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-011-001 — compound predicate factory in planning context

**Input predicate list:** `["IMPLEMENT", "DOCUMENT", "QA"]`
**Request origin:** "Implement the epic features, specify the sprint documentation, and review readiness."

**Factory selection — compound case:**

Each predicate maps to exactly one factory. For three predicates, three factory selections occur:
1. IMPLEMENT → BackendServiceFlow41
2. DOCUMENT → DocumentationFlow
3. QA → QAGatingFlow (last)

No compound factory exists. There is no "IMPLEMENT+DOCUMENT factory" or "IMPLEMENT+QA factory." Each predicate selects its own factory independently. The execution model (parallel vs sequential) is determined by the orchestrator, not the factory.

---

## B36-0D-011-002 — factory independence: each predicate selects independently

**Why factories are independent:**
- BackendServiceFlow41 does not know about DocumentationFlow
- DocumentationFlow does not know about QAGatingFlow
- The orchestrator coordinates their execution and result merging

**Gap vocabulary in compound case:**
- epic-001 → CapabilityGapWarning
- sprint-001 → CapabilityGapWarning
These are attached to the orchestrator-level result, not to any individual factory result.

---

## B36-0D-011-003 — compound factory result structure

```json
{
  "factoryResults": [
    {
      "predicate": "IMPLEMENT",
      "flow": "BackendServiceFlow41",
      "phase": "4.1",
      "executionOrder": 1,
      "parallel": true
    },
    {
      "predicate": "DOCUMENT",
      "flow": "DocumentationFlow",
      "phase": "2.1",
      "executionOrder": 1,
      "parallel": true
    },
    {
      "predicate": "QA",
      "flow": "QAGatingFlow",
      "phase": "QA",
      "executionOrder": 2,
      "parallel": false,
      "constraint": "must_run_after_all_phase_1_flows"
    }
  ],
  "domainId": "software-development",
  "capabilityGapWarnings": ["gap-project-planning-epic-001", "gap-project-planning-sprint-001"]
}
```

---

## B36-0D-011-004 — summary: when does the factory get called in B36?

| Situation | Factory called? | Factory result |
|-----------|----------------|----------------|
| predicates: [] (pure gap) | NO | null |
| predicates: ["IMPLEMENT"] | YES | BackendServiceFlow41 |
| predicates: ["DOCUMENT"] | YES | DocumentationFlow |
| predicates: ["ARCHITECTURE"] | YES | ArchitectureFlow31 |
| predicates: ["QA"] | YES | QAGatingFlow |
| predicates: ["IMPLEMENT", "DOCUMENT"] | YES (twice) | BackendServiceFlow41 + DocumentationFlow |
| predicates: ["IMPLEMENT", "DOCUMENT", "QA"] | YES (three times) | All three flows |
| Predicate: "PROJECT_PLANNING" (non-existent) | Error / null | CapabilityGapException |
