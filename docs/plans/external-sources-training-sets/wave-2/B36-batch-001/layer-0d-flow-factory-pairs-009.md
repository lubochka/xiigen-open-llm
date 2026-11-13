# Layer 0-D — Flow Factory Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-009-001 — v6 factory corruption: PROJECT_PLANNING flow invented

**Corrupt v6-01 behavior (DO NOT REPRODUCE):**

```
Compiler emits: ["PROJECT_PLANNING"] (invented predicate)
Factory receives: ["PROJECT_PLANNING"]
v6-01 factory error: Factory invents ProjectPlanningFlow to handle the predicate.
```

**Why this is doubly wrong:**
1. The compiler should never emit ["PROJECT_PLANNING"] — that predicate does not exist
2. Even if received, the factory should not invent a new flow to handle it
3. The correct factory response to an unknown predicate: throw CapabilityGapException / return null

**Cascade of v6-01:**
Compiler invents predicate → Router passes it to factory → Factory invents flow → Orchestrator runs invented flow → Output is undefined behavior

**Correct behavior at each stage:**
- Compiler: emit []
- Router: route to KNOWLEDGE_EDGE (never reaches factory)
- Factory: not called

---

## B36-0D-009-002 — factory robustness: unknown predicate handling

**Factory contract:**
The factory maps predicates to flows. The mapping is closed — only registered predicates have registered flows. If an unregistered predicate arrives:

```
FlowFactory.Select("PROJECT_PLANNING")
→ No entry in mapping table
→ Return FlowResult.Null or throw CapabilityGapException
→ Do NOT invent ProjectPlanningFlow
→ Do NOT fall through to a default "general" flow
```

---

## B36-0D-009-003 — registered factory mappings (complete for B36)

| Predicate | Flow selected | Phase |
|-----------|-------------|-------|
| IMPLEMENT | BackendServiceFlow41 | 4.1 |
| DOCUMENT | DocumentationFlow | 2.1 |
| ARCHITECTURE | ArchitectureFlow31 | 3.1 |
| QA | QAGatingFlow | QA |
| [] (empty) | null (KNOWLEDGE_EDGE) | null |
| PROJECT_PLANNING (non-existent) | CapabilityGapException | N/A |

---

## B36-0D-009-004 — factory: gap vocabulary never enters factory selection

CapabilityGapWarnings are attached to factory results as metadata. They are NOT inputs to the factory selection algorithm. The factory selects a flow based solely on the predicate list. Gap vocabulary (sprint, backlog, epic, etc.) is invisible to the factory's selection logic.

**Factory inputs:** predicate list
**Factory outputs:** flow selection + phase + TransitionType
**Gap warnings:** metadata attached after factory selection, not inputs to it
