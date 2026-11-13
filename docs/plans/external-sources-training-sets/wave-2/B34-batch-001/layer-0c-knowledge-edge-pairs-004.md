# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-004-001 — Gap ID priority for B34

When multiple guidance gap categories appear in one request:
1. Mentoring vocabulary present → gap-skills-guidance-mentor-001 (highest priority)
2. Onboarding vocabulary present (no mentoring) → gap-skills-guidance-onboard-001
3. Training/learning vocabulary only → gap-skills-guidance-train-001

One SCHEDULE_CAPABILITY_LEARNING emitted per request regardless of how many gap verbs present.

---

## KE-004-002 — QA abbreviation rule applies in B34

"QA the training program rollout" → QA fires (QA abbreviation present)
"Quality assure the training program" → gap (quality assurance spelled out → gap)
"Ensure quality of the training content" → gap (no QA abbreviation)
"Do a QA pass on the onboarding docs" → QA fires

Same rule as all batches: QA abbreviation fires; "quality assurance" spelled out → gap.

---

## KE-004-003 — "TDD" abbreviation does not fire TEST

"Introduce TDD (Test-Driven Development) to the team" → gap
"Practice TDD in the new feature" → gap (practice = methodology vocabulary, not test-writing verb)
"Write tests first (TDD style)" → TEST fires ("write tests" = test-writing verb; TDD label is adjective)

Rule: TDD/BDD/ATDD abbreviations alone do not fire TEST. Test-writing verbs (write, create, assert) fire TEST.

---

## KE-004-004 — "learning path" noun disambiguation

"Build a learning path" → DOCUMENT (content artifact default)
"Build a learning path management system" → IMPLEMENT (software system noun)
"Design a learning path" → gap (curriculum design, not structural system design)
"Create a learning path document" → DOCUMENT

Rule: When "learning path" is the primary artifact and no software-system noun follows, default to DOCUMENT. System/platform/tracker suffix → IMPLEMENT.
