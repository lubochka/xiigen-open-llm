# Layer 0-B — Router Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-012-001 — Complete B32 routing decision table (reference)

**Purpose:** Complete reference for all B32 routing patterns.

### Zero-Predicate Routing

| Condition | Route | DomainId | Action |
|---|---|---|---|
| [] + any gap warnings | knowledge_edge | software-development | SCHEDULE_CAPABILITY_LEARNING |
| [] + gap-vibe-coding-spike-001 | knowledge_edge | software-development | SCHEDULE_CAPABILITY_LEARNING |
| [] + gap-vibe-coding-prototype-001 | knowledge_edge | software-development | SCHEDULE_CAPABILITY_LEARNING |
| [] + gap-vibe-coding-dashboard-001 | knowledge_edge | software-development | SCHEDULE_CAPABILITY_LEARNING |

### Single-Predicate Routing

| Predicate | Flow | TransitionType |
|---|---|---|
| [IMPLEMENT] | BackendServiceFlow41 | feature_addition |
| [UI] | UIFlow | feature_addition |
| [ARCHITECTURE] | ArchitectureFlow31 | documentation |
| [API_CONTRACT] | APIContractFlow | feature_addition |
| [SECURITY] | SecurityFlow | feature_addition |
| [TEST] | TestFlow | no contribution |
| [DOCUMENT] | DocumentFlow | documentation |
| [QA] | QAFlow | no contribution |

### Multi-Predicate Routing (with vibe framing examples)

| Predicates | Flows | Notes |
|---|---|---|
| [UI, IMPLEMENT] | UIFlow + BackendServiceFlow41 | parallel-eligible |
| [ARCHITECTURE, IMPLEMENT] | ArchitectureFlow31 + BackendServiceFlow41 | IMPLEMENT dominates TransitionType |
| [ARCHITECTURE, API_CONTRACT] | ArchitectureFlow31 + APIContractFlow | parallel-eligible |
| [IMPLEMENT, TEST] | BackendServiceFlow41 + TestFlow | TEST after IMPLEMENT |
| [IMPLEMENT, DOCUMENT] | BackendServiceFlow41 + DocumentFlow | parallel with TEST |
| [all 7] | all flows | QA last |

---

## B32-0B-012-002 — Router invariants for B32

**These invariants hold for every B32 routing decision:**

1. Gap warnings never affect which flow is selected
2. DomainId is always "software-development" (never "project-management" or "vibe-coding")
3. SCHEDULE_CAPABILITY_LEARNING fires if and only if predicates = []
4. TransitionType = feature_addition when IMPLEMENT is present
5. TransitionType = documentation when ARCHITECTURE or DOCUMENT is present without IMPLEMENT
6. TransitionType = no contribution when only QA or TEST (without IMPLEMENT)
7. TransitionType = unknown when predicates = []
8. QA always has the last ordering position in multi-predicate flows

---
