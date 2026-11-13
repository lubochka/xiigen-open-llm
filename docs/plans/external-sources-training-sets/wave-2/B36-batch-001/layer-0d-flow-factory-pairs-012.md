# Layer 0-D — Flow Factory Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-012-001 — Complete B36 factory map

**Gap vocab → knowledge_edge; predicate + gap → normal factory**

| Request type | Predicates | Factory | Phase | Gap warnings |
|-------------|-----------|---------|-------|-------------|
| Pure gap (sprint, backlog only) | [] | NONE | null | All gap terms |
| QA + gap (review in planning) | [QA] | QAGatingFlow | QA | All gap terms |
| IMPLEMENT + gap (build in sprint) | [IMPLEMENT] | BackendServiceFlow41 | 4.1 | All gap terms |
| DOCUMENT + gap (document milestone) | [DOCUMENT] | DocumentationFlow | 2.1 | All gap terms |
| ARCHITECTURE + gap (design in sprint) | [ARCHITECTURE] | ArchitectureFlow31 | 3.1 | All gap terms |
| IMPLEMENT+DOCUMENT + gap | [IMPLEMENT, DOCUMENT] | Both factories | 4.1 + 2.1 | All gap terms |
| IMPLEMENT+DOCUMENT+QA + gap | [IMPLEMENT, DOCUMENT, QA] | All three | 4.1 + 2.1 + QA | All gap terms |

---

## B36-0D-012-002 — Factory invariants for B36

1. Factory is never called when predicates: [] (KNOWLEDGE_EDGE bypasses factory)
2. Each predicate independently selects its own factory (no compound factories)
3. QAGatingFlow is always scheduled last when QA co-occurs with other predicates
4. Gap vocabulary never changes which factory is selected
5. CapabilityGapWarnings are metadata attached to factory results, not factory inputs
6. PROJECT_PLANNING is not a registered predicate; no factory handles it

---

## B36-0D-012-003 — factory selection is deterministic

Given the same predicate list, the factory always selects the same flows. There is no ambiguity or context-sensitivity in factory selection. The factory is a pure mapping: predicate → flow.

This means: the same sentence appearing in different contexts always produces the same factory selection (assuming the same predicate output from the compiler). Context may vary the gap warnings but never the factory selection.

---

## B36-0D-012-004 — factory stage position in the pipeline

```
Request
  → Compiler (predicates + gap IDs)
  → Router (path + DomainId)
  → Factory (flow selection)         ← this stage
  → Orchestrator (execution + SCHEDULE_CAPABILITY_LEARNING)
  → Phase execution (4.1 / 2.1 / 3.1 / QA)
  → FinalHumanResult
```

The factory is between the router and the orchestrator. It receives the predicate list from the router and returns flow selections to the orchestrator. Gap warnings pass through all stages as metadata.
