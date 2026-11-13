# Layer 3 — Teaching Unit Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-012 — B34 complete knowledge summary

**Gap trigger**: Any guidance/mentoring/training verb without an embedded code-action verb.

**Three gap IDs** (priority order):
1. gap-skills-guidance-mentor-001: mentor, coach, advise, pair program, shadow, review (mentoring), give feedback, facilitate, sponsor
2. gap-skills-guidance-onboard-001: onboard, orientation, buddy system, walkthrough (facilitation), ramp-up
3. gap-skills-guidance-train-001: teach, train (human), explain, demonstrate, upskill, workshop, TDD (methodology), bootcamp (learning)

**Key disambiguation pairs**:
- "explain X" → gap; "document X" → DOCUMENT
- "teach to implement" → gap; "implement" → IMPLEMENT
- "design curriculum" → gap; "design system" → ARCHITECTURE
- "create course" → DOCUMENT; "create course platform" → IMPLEMENT
- "run tests" → gap; "write tests" → TEST
- "quality assurance" → gap; "QA" → QA
- "coach on security" → gap; "secure the API" → SECURITY
- "connect mentors with mentees" → gap; "integrate systems" → API_CONTRACT

**DomainId**: always software-development.
**SCHEDULE_CAPABILITY_LEARNING**: emitted once per gap request.
**Response**: "Capability not available. Request logged for learning pipeline."
