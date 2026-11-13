# Layer 3 — Teaching Unit Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-011-001 — False-positive taxonomy: which project planning terms accidentally fire predicates

**Teaching objective:** Model learns the complete list of B36 planning terms that accidentally fire predicates.

**False-positive table:**

| Planning term | Predicate fires | Why | False positive? |
|--------------|----------------|-----|----------------|
| story (in "story points") | DOCUMENT | "story" is in DOCUMENT lexicon | YES — user meant estimation |
| stories (in "user stories") | DOCUMENT | "story" substring | YES — user meant work items |
| review (in "sprint review") | QA | "review" is in QA lexicon | DEBATED — sprint review may legitimately need QA |
| feature (in "sprint feature") | IMPLEMENT | "feature" is in IMPLEMENT lexicon | PARTIAL — user might mean backlog item |
| write (in "write user stories") | IMPLEMENT | "write" is in IMPLEMENT lexicon | YES — user meant authoring, not coding |
| build (in "build the roadmap") | IMPLEMENT | "build" is in IMPLEMENT lexicon | YES — user meant create the roadmap document |
| design (in "design the sprint structure") | ARCHITECTURE | "design" is in ARCHITECTURE lexicon | PARTIAL — structural design may be appropriate |
| define (in "define sprint goals") | DOCUMENT | "define" is in DOCUMENT lexicon | PARTIAL — defining goals does produce documentation |
| criteria (in "sprint exit criteria") | DOCUMENT | "criteria" is in DOCUMENT lexicon | NO — exit criteria are documentation |
| requirements (in "sprint requirements") | DOCUMENT | "requirements" is in DOCUMENT lexicon | NO — sprint requirements are documentation |

---

## B36-L3-011-002 — True false positives (should not fire but do)

The clearest false positives in B36 context:
1. "story" fires DOCUMENT when user means estimation (story points)
2. "write" fires IMPLEMENT when user means write user stories (authoring)
3. "build" fires IMPLEMENT when user means create the roadmap (non-code)

For these, the predicate fires but the routing serves the user poorly. CapabilityGapWarnings note the mismatch.

---

## B36-L3-011-003 — Accurate fires (looks like false positive but is correct)

Some fires that might seem like false positives are actually correct:
1. "define the acceptance criteria" → DOCUMENT fires on "define" + "criteria" → correct: acceptance criteria are documented requirements
2. "specify the sprint requirements" → DOCUMENT fires → correct: sprint requirements produce documentation
3. "review the sprint" → QA fires → debatable but consistent: sprint review is a QA gate
4. "implement the sprint feature" → IMPLEMENT fires → correct: the user does want to implement the feature

---

## B36-L3-011-004 — Model behavior rule for false positives

**The rule is simple:** The model does NOT classify fires as "true" or "false" and suppress based on that classification. All lexicon matches fire. The model produces the fired predicate in all cases. The classification of "true" vs "false" positive is a documentation concern (for batch notes and CapabilityGapWarnings), not a model behavior concern.

The model's job: fire on lexicon match → output predicates → emit gap warnings. Never suppress.
