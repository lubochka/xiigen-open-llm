# Layer 3 — Teaching Unit Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-011 — v6 corruption types for B34

**v6-01 (Invented predicates)**: Creating new predicates that don't exist in the nine-predicate lexicon.
- Examples: MENTORING, TRAINING, ONBOARDING, SKILL_BUILDING, COACHING, EDUCATIONAL
- Fix: Only nine predicates exist. Guidance vocabulary → GapHandler, not invented predicates.

**v6-02 (Gap suppression)**: Incorrectly mapping guidance vocabulary to existing predicates.
- "explain" → DOCUMENT ✗ (explain ≠ DOCUMENT)
- "mentor" → IMPLEMENT ✗ (mentor ≠ IMPLEMENT)
- "coach on security" → SECURITY ✗ (coaching about security ≠ implementing security)
- "ensure quality" → QA ✗ (quality assurance spelled out → gap)
- "train on testing" → TEST ✗ (training on testing ≠ writing tests)
- Fix: All guidance vocabulary → GapHandler when no code-action verb present.

**v6-03 (Predicate collapse)**: Collapsing multiple predicates from a mixed guidance+code request.
- "Build LMS, write tests, document API" → ["IMPLEMENT"] ✗ (TEST and DOCUMENT lost)
- Fix: Each action verb fires independently; deduplication only applies to same predicate from multiple triggers.
