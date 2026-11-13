# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-006-001 — DomainId determination: "project-management" vs "software-development"

**The rule (B36 Rule 7):**

- DomainId: "project-management" ONLY when predicates: []
- DomainId: "software-development" when ANY predicate fires, regardless of gap vocabulary proportion

**This is not a vote.** There is no scoring of planning vocabulary vs development vocabulary. The rule is binary: predicate fired? → "software-development." No predicate? → "project-management."

---

## B36-0C-006-002 — DomainId test matrix

| Predicates | Gap vocab count | DomainId | Note |
|-----------|-----------------|----------|------|
| [] | 0 (no gap vocab) | "project-management" | Pure no-vocab request |
| [] | 5 (sprint, backlog, velocity, standup, retrospective) | "project-management" | Many gap terms — still project-management |
| ["QA"] | 4 (sprint, burndown, velocity, epic) | "software-development" | One predicate overrides all gap vocab |
| ["IMPLEMENT"] | 1 (sprint) | "software-development" | One predicate |
| ["IMPLEMENT", "DOCUMENT", "QA"] | 3 | "software-development" | Three predicates |

---

## B36-0C-006-003 — why "project-management" is inferred from gap vocabulary

When predicates: [], the orchestrator has no fired predicate to assign a domain. The gap vocabulary in the request is the only signal. All current B36 gap vocabulary is project management vocabulary. Therefore, when zero predicates fire and gap vocabulary is present, "project-management" is inferred.

**This inference is secondary to predicate firing.** If a predicate fires, the inference is bypassed and "software-development" is set unconditionally.

---

## B36-0C-006-004 — DomainId "project-management" edge case: no gap vocab either

**Input:** "Please help me think through this."

**Token analysis:**
- No gap vocabulary
- No predicate vocabulary
- predicates: []

**DomainId:** "project-management" is the fallback even with no gap vocabulary present, as long as predicates: []. This is the default domain for KNOWLEDGE_EDGE state.

However, for B36 batches, all knowledge_edge cases include gap vocabulary. The fallback to "project-management" with no gap vocab is documented here for completeness but is not the primary B36 use case.
