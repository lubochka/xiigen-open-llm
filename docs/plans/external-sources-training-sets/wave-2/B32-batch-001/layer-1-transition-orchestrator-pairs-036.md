# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-036-001 — Round 3 TransitionType reference (complete)

**TransitionType derivation for all predicate combinations:**

| Predicates | TransitionType |
|---|---|
| [] | unknown |
| [IMPLEMENT] | feature_addition |
| [UI] | feature_addition |
| [IMPLEMENT, UI] | feature_addition |
| [IMPLEMENT, TEST] | feature_addition |
| [IMPLEMENT, SECURITY] | feature_addition |
| [IMPLEMENT, ARCHITECTURE] | feature_addition |
| [IMPLEMENT, API_CONTRACT] | feature_addition |
| [IMPLEMENT, TEST, SECURITY] | feature_addition |
| [ARCHITECTURE] | documentation |
| [API_CONTRACT] | documentation |
| [DOCUMENT] | documentation |
| [ARCHITECTURE, API_CONTRACT] | documentation |
| [ARCHITECTURE, SECURITY] | documentation |
| [DOCUMENT, API_CONTRACT] | documentation |
| [TEST] | no_contribution |
| [QA] | no_contribution |
| [SECURITY] | no_contribution |
| [TEST, QA] | no_contribution |
| [TEST, SECURITY] | no_contribution |

**Rule:** IMPLEMENT or UI present → always feature_addition.

---

## B32-L1-036-002 — ContributionFlag reference

**ContributionFlag = true when:** predicates non-empty (any active flow exists)
**ContributionFlag = false when:** predicates = [] (knowledge_edge)

---

## B32-L1-036-003 — SessionState reference

**SessionState values:**
- "active" → predicates non-empty
- "knowledge_edge" → predicates = []

---

## B32-L1-036-004 — DomainId invariant in orchestrator output

**All orchestrator outputs use DomainId = "software-development"**

Applies to active sessions and knowledge_edge sessions equally.

---
