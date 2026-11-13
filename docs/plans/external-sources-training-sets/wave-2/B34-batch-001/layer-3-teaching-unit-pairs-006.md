# Layer 3 — Teaching Unit Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-006 — QA abbreviation rule in B34 context

**Rule**: QA fires ONLY on "QA" abbreviation. "quality assurance," "quality," "quality control," "quality of training," "ensure quality" all → gap.

**B34 examples**:

| Request | Predicate |
|---------|-----------|
| "QA the onboarding portal" | QA |
| "Do a QA pass on the training docs" | QA |
| "Quality assure the certification process" | [] gap |
| "Ensure the quality of our training program" | [] gap |
| "Improve the quality of the onboarding" | [] gap |
| "QA sign-off on the mentorship features" | QA |

**Mnemonic**: The two-letter abbreviation "QA" is the trigger. Anything else spelled out → gap. This rule is absolute — no exceptions in B34 or any other batch.

**Note**: Even "QA process" as a noun phrase does not fire QA. The abbreviation must appear as a direct action verb or subject of the action (e.g., "QA the X" or "do QA on X").
