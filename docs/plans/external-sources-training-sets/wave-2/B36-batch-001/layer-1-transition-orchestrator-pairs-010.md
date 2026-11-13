# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-010-001 — TransitionType summary for B36 (all predicate combinations)

**Complete TransitionType table for B36:**

| Predicates | TransitionType | Phases | Notes |
|-----------|---------------|--------|-------|
| [] | unknown | null | KNOWLEDGE_EDGE |
| ["QA"] | no_contribution | QA | Validates, does not create |
| ["IMPLEMENT"] | feature_addition | 4.1 | Creates code |
| ["DOCUMENT"] | documentation | 2.1 | Creates docs |
| ["ARCHITECTURE"] | documentation | 3.1 | Creates arch docs |
| ["IMPLEMENT", "DOCUMENT"] | feature_addition | 4.1, 2.1 | IMPLEMENT is primary |
| ["IMPLEMENT", "QA"] | feature_addition | 4.1, QA | QA last |
| ["DOCUMENT", "QA"] | documentation | 2.1, QA | QA last |
| ["ARCHITECTURE", "DOCUMENT"] | documentation | 3.1, 2.1 | Both documentation |
| ["IMPLEMENT", "DOCUMENT", "QA"] | feature_addition | 4.1, 2.1, QA | IMPLEMENT primary, QA last |

---

## B36-L1-010-002 — TransitionType selection rules

1. If IMPLEMENT is in predicates: TransitionType = feature_addition
2. Else if ARCHITECTURE is in predicates: TransitionType = documentation
3. Else if DOCUMENT is in predicates: TransitionType = documentation
4. Else if QA only: TransitionType = no_contribution
5. Else (predicates: []): TransitionType = unknown

Rule 1 has highest priority. IMPLEMENT always yields feature_addition even when DOCUMENT or QA also fire.

---

## B36-L1-010-003 — Gap vocabulary has no effect on TransitionType

Gap vocabulary (sprint, backlog, epic, etc.) has no entry in the TransitionType selection rules. It never changes the TransitionType. A sentence with ten sprint-vocabulary tokens and one "implement" token produces TransitionType: feature_addition — not "project-management transition" or "planning transition."

---

## B36-L1-010-004 — TransitionType "unknown" is specific to KNOWLEDGE_EDGE

TransitionType "unknown" does not mean "I don't know what type it is." It is a defined value with a specific meaning: the request entered KNOWLEDGE_EDGE state because no predicate fired. "unknown" in this context means "no transition happened" — the request did not transition to any phase.

Using "unknown" for predicates: ["IMPLEMENT"] would be an error. "unknown" is only valid when predicates: [].
