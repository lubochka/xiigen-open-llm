# Layer 3 — Teaching Unit Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-009 — TransitionType in B34: same hierarchy, no exceptions

**Rule**: TransitionType determination is identical in B34 as in all other batches.

| Condition | TransitionType |
|-----------|----------------|
| "refactor" verb + IMPLEMENT | refactoring (highest priority) |
| IMPLEMENT present, no refactor | feature_addition |
| TEST and/or DOCUMENT, no IMPLEMENT | documentation |
| ARCHITECTURE alone | documentation |
| QA alone | no contribution |
| Zero predicates (gap) | none |

**B34-specific edge cases**:
- "Build a learning portal" → feature_addition (IMPLEMENT)
- "Write the onboarding guide" → documentation (DOCUMENT)
- "QA the certification process" → no contribution (QA)
- "Mentor the team" → none (gap)
- "Refactor the onboarding module while mentoring" → refactoring (refactor verb + IMPLEMENT; mentor→gap suppressed)

**Note**: No "education_addition" or "curriculum_addition" TransitionType exists. B34 uses the same five TransitionType values as all batches.
