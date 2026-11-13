# Layer 3 — Teaching Unit Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-010 — "integrate" always fires API_CONTRACT in B34

**Rule**: "integrate" fires API_CONTRACT in all B34 contexts — LMS, HR systems, SSO, training platforms, etc.

**B34 examples**:

| Request | Predicate |
|---------|-----------|
| "Integrate the LMS with Slack" | API_CONTRACT |
| "Integrate the training portal with SSO" | API_CONTRACT |
| "Integrate the skill tracker with the HR system" | API_CONTRACT |
| "Integrate the mentorship platform with Zoom" | API_CONTRACT |
| "Connect the certification system to the payroll API" | API_CONTRACT |

**What does NOT fire API_CONTRACT in B34**:
- "Use the LMS API" → gap (use = consumption vocabulary, not integration verb)
- "Add LMS support" → could be IMPLEMENT depending on artifact noun
- "Connect mentors with mentees" (human, not system) → gap (interpersonal vocabulary)

**Rule clarification**: "connect" fires API_CONTRACT only when connecting systems/services. "connect" with human subjects → gap.
