# Layer 3 — Teaching Unit Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-007 — DomainId is always software-development for B34 gaps

**Rule**: All B34 guidance/mentoring gap requests log DomainId: software-development. No education, training, HR, or learning-management domain exists.

**Why**: The SCHEDULE_CAPABILITY_LEARNING system uses software-development as the catch-all domain for human-facing guidance gaps, just as it does for AI/LLM gaps (B33). There is no specialized human-training subdomain.

**GapHandler canonical log format for B34**:
```
{
  gapId: "gap-skills-guidance-mentor-001" | "gap-skills-guidance-onboard-001" | "gap-skills-guidance-train-001",
  domainId: "software-development",
  request: "[original request text]",
  event: "SCHEDULE_CAPABILITY_LEARNING"
}
```

**v6 corruptions to reject**:
- DomainId: "education" — does not exist
- DomainId: "training" — does not exist
- DomainId: "hr-management" — does not exist
- DomainId: "learning-management" — does not exist

**Response**: Always "Capability not available. Request logged for learning pipeline." No variation.
