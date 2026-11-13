# Layer 3 — Teaching Unit Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-010-001 — v6 corruption patterns for B36 (three types with examples)

**Teaching objective:** Model recognizes and avoids all three v6 corruption patterns in B36 context.

**Summary:**

| Code | What the corrupt model does | Detection input |
|------|---------------------------|----------------|
| v6-01 | Emits ["PROJECT_PLANNING"] | Any planning-only request |
| v6-02 | Suppresses a valid predicate because "planning context" | Any request with "review" + planning vocab |
| v6-03 | Sets DomainId "project-management" despite predicate fire | Any request with predicate + planning vocab |

---

## B36-L3-010-002 — v6-01 example with correction

**Detection input:** "Plan the sprint standup and grooming session."

**v6-01 (wrong):**
- predicates: ["PROJECT_PLANNING"]
- routing: crash (unknown flow)

**Correct:**
- predicates: []
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**Why the model might make this error:** It recognizes that planning vocabulary is present and invents a category to label it. The fix: the only valid predicate outputs are from the nine registered predicates. When none fire, output [].

---

## B36-L3-010-003 — v6-02 example with correction

**Detection input:** "Review the sprint backlog."

**v6-02 (wrong):**
- predicates: [] (QA suppressed because "sprint backlog is planning")
- DomainId: "project-management" (wrong)
- KNOWLEDGE_EDGE: true (wrong)

**Correct:**
- predicates: ["QA"] (review fires QA)
- DomainId: "software-development"
- KNOWLEDGE_EDGE: false
- CapabilityGapWarnings: sprint-001, backlog-001

**Why the model might make this error:** It semantically evaluates the sentence and concludes "this is a planning task, so no development predicate should fire." The fix: "review" is in the QA lexicon. Lexicon matches fire regardless of semantic context.

---

## B36-L3-010-004 — v6-03 example with correction

**Detection input:** "Implement the sprint backlog feature."

**v6-03 (wrong):**
- predicates: ["IMPLEMENT"] (correct predicate)
- DomainId: "project-management" (WRONG — predicate fired)

**Correct:**
- predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, backlog-001

**Why the model might make this error:** It counts planning vocabulary ("sprint", "backlog") and development vocabulary ("implement", "feature") and decides the sentence is "more planning than development." The fix: DomainId is binary — predicate fired means "software-development," always.
