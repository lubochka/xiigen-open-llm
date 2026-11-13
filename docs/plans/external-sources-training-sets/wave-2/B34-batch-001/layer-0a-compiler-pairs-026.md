# Layer 0-A — Compiler Pairs (026)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-026-001 — "demonstrate" (standalone) → gap

A: "Demonstrate how our distributed tracing system works to the platform engineers."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "demonstrate" = training/teaching vocabulary → gap. "distributed tracing" = topic context, no code-action predicate fires.

---

## B34-compiler-026-002 — "introduce" → gap

A: "Introduce the team to our API gateway patterns and routing conventions."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "introduce" = teaching/familiarisation vocabulary → gap. "API gateway patterns" = knowledge topic, not a direct code action.

---

## B34-compiler-026-003 — "introduce" vs "IMPLEMENT" noun disambiguation

A: "Introduce the new rate-limiting feature into the gateway."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "introduce X into Y" with software component noun = IMPLEMENT. "introduce" as verb of insertion (not teaching/familiarisation) → IMPLEMENT fires. Context: adding software feature, not teaching a person.

---

## B34-compiler-026-004 — "introduce" disambiguation rule

Rule: "introduce [person/team] to [topic]" = teaching vocabulary → gap.
Rule: "introduce [feature/component] into [system]" = software insertion verb → IMPLEMENT fires.
Key distinction: object of "introduce" determines routing (person/team = gap; software artifact = IMPLEMENT).

---

## B34-compiler-026-005 — v6 corruption: "introduce" always → IMPLEMENT

v6 corrupt: "Introduce the engineers to our microservices patterns" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02 (gap suppression). Engineers = people noun → teaching sense → gap. "Introduce to [person] [topic]" always gap.
Fix: Check object of "introduce" — person/team = gap; software artifact = IMPLEMENT.
