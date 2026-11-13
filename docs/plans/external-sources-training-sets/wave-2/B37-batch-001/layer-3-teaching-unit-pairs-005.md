# Layer 3 — Teaching-Unit Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-005-001 — Teaching: "test" fires TEST even in estimation/planning context

**Core concept:** "test" → TEST fires ALWAYS (Rule 21). Even "test the estimates", "test the assumptions", "test the timeline".

**The rule is absolute:** The compiler fires TEST on the lexical token "test" without regard to:
- Whether a software artifact is being tested
- Whether "test" means "challenge" or "evaluate" in context
- Whether the sentence is about project management or software development

**Teaching pair:**
Q: "Test the delivery estimates with the team." — does TEST fire?
A: Yes. "test" → TEST. Always.

Q: "We should test whether the timeline is realistic." — does TEST fire?
A: Yes. "test" → TEST. Always.

---

## B37-L3-005-002 — Teaching: "test" vs related estimation vocabulary

| Token | Fires predicate? | Notes |
|-------|-----------------|-------|
| test | TEST | Always |
| validate | QA | Different predicate than test |
| verify | TEST | |
| check | NONE | Does not fire QA or TEST |
| assess | NONE | Does not fire any predicate |
| evaluate | NONE | Does not fire any predicate |
| review | QA | Different predicate |
| confirm | NONE | Does not fire any predicate |

**Common B37 managing vocabulary:**
- "Check the delivery status" → [] (check does not fire)
- "Assess the blockers" → [] (assess does not fire)
- "Evaluate escalation options" → [] (evaluate does not fire)
- "Test the assumptions" → [TEST] (test fires)

---

## B37-L3-005-003 — Teaching: TEST + DOCUMENT + QA from a managing sentence

**Input:** "Test the delivery estimates with the stakeholders and validate the timeline assumptions."

This sentence has managing intent (checking estimates, reviewing timeline). Yet three predicates fire:
- "test" → TEST
- "stakeholders" → DOCUMENT
- "validate" → QA

**Teaching insight:** A sentence with zero software development intent can fire three predicates if it contains three incidental predicate-triggering tokens. The predicates fire based on tokens, not intent.

**The managing intent is not lost** — it is captured in CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 (estimates context)
- gap-project-managing-track-status-001 (timeline context)
- gap-project-managing-stakeholder-update-001 (stakeholder notification intent)

---

## B37-L3-005-004 — Teaching: "test" firing in five B37 contexts

1. "Test the delivery estimates." → ["TEST"] (estimates = gap; test fires)
2. "Test the assumptions in the RAID log." → ["TEST"] (RAID = gap; test fires)
3. "Test whether the escalation process works." → ["TEST"] (escalation = gap; test fires)
4. "Test and validate the governance criteria." → ["TEST", "QA", "DOCUMENT"] (test + validate + criteria)
5. "Test the status with stakeholders." → ["TEST", "DOCUMENT"] (test + stakeholders)

In all five cases: TEST fires on "test". Managing vocabulary → gap warnings.

---

## B37-L3-005-005 — Teaching: "test" is the most unexpected B37 predicate trigger

**Why "test" surprises in B37:**
Project managers commonly say "test the waters", "test the assumptions", "test the timeline". In common language, "test" = "evaluate/check". But in the predicate lexicon, "test" = TEST predicate (software testing). This creates false positives in B37.

**Mitigation:** The CapabilityGapWarnings capture the managing intent even when TEST fires. The system does not lose the managing context — it just also routes to TestFlow5x, which is a false positive route.

**This false positive pattern is documented in B37-batch-001 to train the model to:**
1. Fire TEST on "test" in managing context (correct lexical behaviour)
2. Emit gap warnings for the managing intent (correct gap handling)
3. NOT suppress TEST because the context is managing (no suppression)
