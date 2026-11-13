# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-024-001 — Complete TransitionType reference for B32 predicate combinations

**TransitionType derivation table:**

| Predicates | TransitionType | Notes |
|---|---|---|
| [] | unknown | Always unknown for knowledge_edge |
| [IMPLEMENT] | feature_addition | — |
| [UI] | feature_addition | — |
| [IMPLEMENT, UI] | feature_addition | — |
| [ARCHITECTURE] | documentation | — |
| [ARCHITECTURE, API_CONTRACT] | documentation | ARCHITECTURE sets type |
| [ARCHITECTURE, IMPLEMENT] | feature_addition | IMPLEMENT takes precedence |
| [API_CONTRACT] | documentation | — |
| [SECURITY] | no_contribution | — |
| [TEST] | no_contribution | — |
| [DOCUMENT] | documentation | — |
| [QA] | no_contribution | — |

---

## B32-L1-024-002 — ContributionFlag rules for B32

**ContributionFlag = true when:** predicates are non-empty (active flow exists)

**ContributionFlag = false when:** predicates = [] (knowledge_edge, no flow active)

**CapabilityGapWarning** appears in output when: predicates are non-empty AND gap vocabulary was present.

**SessionState:**
- "active" when predicates non-empty
- "knowledge_edge" when predicates empty

**DomainId:** Always "software-development" for all B32 orchestrator outputs.

---
