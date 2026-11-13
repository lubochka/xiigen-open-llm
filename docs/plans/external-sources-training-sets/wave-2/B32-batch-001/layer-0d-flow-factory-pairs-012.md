# Layer 0-D — Flow Factory Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-012-001 — Complete B32 factory map

**Purpose:** Complete reference for all B32 factory selection patterns.

### Factory Registry (registered factories only)

| Predicate | Factory | TransitionType |
|---|---|---|
| IMPLEMENT | BackendServiceFlow41 | feature_addition |
| UI | UIFlow | feature_addition |
| ARCHITECTURE | ArchitectureFlow31 | documentation |
| API_CONTRACT | APIContractFlow | feature_addition |
| SECURITY | SecurityFlow | feature_addition |
| TEST | TestFlow | no contribution |
| DOCUMENT | DocumentFlow | documentation |
| QA | QAFlow | no contribution |

**Note on TransitionType override:** When IMPLEMENT is present alongside other predicates, TransitionType = feature_addition (IMPLEMENT dominates).

### Zero-Predicate Cases → No Factory

| Condition | Factory | Route |
|---|---|---|
| [] (any B32 gap vocabulary) | none | knowledge_edge |

---

## B32-0D-012-002 — B32 factory selection invariants

1. **No VibeCodingFlow**: This factory does not exist. Never select it.
2. **No ExploratoryFlow**: This factory does not exist. Never select it.
3. **Gap metadata is always informational**: It never changes which factory is selected.
4. **DomainId "software-development"**: Always in B32. Never "vibe-coding" or "project-management".
5. **Factory creation does not depend on style framing**: Same predicates → same factories, always.
6. **SCHEDULE_CAPABILITY_LEARNING only when predicates = []**: Not emitted when factories are selected.

---

## B32-0D-012-003 — B32 factory map: complete example table

| Request | Predicates | Factories |
|---|---|---|
| "Run a spike on the recommendation engine" | [] | none (knowledge_edge) |
| "Build something quick and dirty" | [IMPLEMENT] | BackendServiceFlow41 |
| "Vibe code a quick screen" | [UI, IMPLEMENT] | UIFlow, BackendServiceFlow41 |
| "Throw together a dashboard — nothing fancy" | [] | none (knowledge_edge) |
| "Build a rough prototype" | [IMPLEMENT] | BackendServiceFlow41 |
| "Exploratory coding — hack the module" | [IMPLEMENT] | BackendServiceFlow41 |
| "Vibe code screen + implement upload" | [UI, IMPLEMENT] | UIFlow, BackendServiceFlow41 |
| "Design rough-cut architecture" | [ARCHITECTURE] | ArchitectureFlow31 |
| "Full vibe session — all 7 verbs" | [all 7] | all 7 factories |

---
