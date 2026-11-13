# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-008-001 — TransitionType in B34 context

TransitionType hierarchy (same as all batches; no B34-specific overrides):
1. "refactor" verb → refactoring
2. IMPLEMENT present (no refactor) → feature_addition
3. TEST and/or DOCUMENT (no IMPLEMENT) → documentation
4. QA alone → no contribution
5. Zero predicates (gap) → none

B34 examples:
- "Build the skill tracker" → feature_addition
- "Write the onboarding guide" → documentation
- "Refactor the training module" → refactoring (IMPLEMENT fires; refactor verb → refactoring TransitionType)
- "QA the onboarding process" → no contribution
- "Mentor the team" → none (gap)

---

## KE-008-002 — "certify" disambiguation

"Certify the developers as cloud architects" → gap (HR/certification process → gap)
"Build the certification management system" → IMPLEMENT (build + software system)
"Write the certification criteria document" → DOCUMENT (write + document artifact)

---

## KE-008-003 — "performance review" vs QA

"Conduct a performance review for the junior developers" → gap (HR vocabulary → gap)
"QA the developer performance review system" → QA (QA abbreviation + software system)
"Document the performance review process" → DOCUMENT

Rule: "performance review" in HR context → gap. QA fires only on "QA" abbreviation in software-gating context.

---

## KE-008-004 — v6 corruption types for B34

v6-01 (invented predicates): MENTORING, TRAINING, ONBOARDING, SKILL_BUILDING — none of these exist in the nine-predicate lexicon
v6-02 (gap suppression): "explain" → DOCUMENT; "mentor" → IMPLEMENT; "coach" → ARCHITECTURE; "teach" → TEST
v6-03 (collapse): guidance + code → only guidance logged; code predicates lost
All three are errors. Correct behavior: guidance vocabulary → gap; code verbs → their predicates.
