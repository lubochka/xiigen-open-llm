# Layer 3 — Teaching Unit Pairs (013)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-013-001 — Teaching: environment promotion does not require IMPLEMENT

LESSON: "Environment promotion" is an operational process. The word "promotion" does not fire IMPLEMENT.

When IMPLEMENT fires in environment promotion:
Only when "service", "build", or "pipeline" appears in the input.

"Verify the staging environment meets promotion criteria" → ["QA","DOCUMENT"] (no IMPLEMENT)
"Approve the auth service promotion" → ["QA","IMPLEMENT"] (service forces IMPLEMENT)

RULE: IMPLEMENT comes from code vocabulary (service/build/pipeline), not from deployment-process vocabulary (promotion/cutover/migration).

---

## B38-TU-013-002 — Teaching: gap vocabulary co-present with predicates

LESSON: When gap vocabulary appears alongside predicate-firing vocabulary, the result is predicates + CapabilityGapWarning. The gap vocabulary does NOT suppress the predicate.

"Run the environment cutover" → knowledge_edge (all gap)
"Run the environment cutover and approve the release" → ["QA"] (approve fires QA; cutover gets gap warning)

The presence of gap vocabulary NEVER reduces the predicate set.
The presence of gap vocabulary ALWAYS adds a CapabilityGapWarning.

---

## B38-TU-013-003 — Teaching: why criteria always fires DOCUMENT

LESSON: "Criteria" is Rule 3 — absolute. Every qualifier produces the same result.

"environment promotion criteria" → DOCUMENT (promotion is the qualifier)
"staging environment criteria" → DOCUMENT (staging environment is the qualifier)
"production readiness criteria" → DOCUMENT (production readiness is the qualifier)
"exit criteria" → DOCUMENT (exit is the qualifier)

No qualifier can make "criteria" fire anything other than DOCUMENT.
No qualifier can make "criteria" become gap vocabulary.
No qualifier can suppress the DOCUMENT predicate from "criteria".
