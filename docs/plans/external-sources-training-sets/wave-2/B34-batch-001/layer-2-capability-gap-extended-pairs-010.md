# Layer 2 — Capability Gap Extended Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-010 — GapHandler nine-node: "Pair programming sessions"

Request: "Set up regular pair programming sessions to improve code quality across the team."

### Flow: GapHandler Nine-Node

**Node 1**: Parse. "set up pair programming sessions" → guidance/mentoring vocabulary → gap.
**Node 2**: Predicate scan: no code-action verb. "code quality" = quality concept, not predicate. Predicate set = [].
**Node 3**: Gap classification: mentoring/collaborative learning. Gap category: mentor.
**Node 4**: Gap ID: gap-skills-guidance-mentor-001
**Node 5**: DomainId: software-development
**Node 6**: Emit SCHEDULE_CAPABILITY_LEARNING
**Node 7**: Log: {id: gap-skills-guidance-mentor-001, request: "pair programming sessions for code quality"}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED

---

## B34-gap-010b — Gap ID summary for B34

| Gap ID | Category | Example triggers |
|--------|----------|-----------------|
| gap-skills-guidance-mentor-001 | Mentoring | mentor, coach, advise, pair program, shadow, review (mentoring), give feedback |
| gap-skills-guidance-onboard-001 | Onboarding | onboard, orientation, buddy system, walkthrough (facilitation), ramp-up |
| gap-skills-guidance-train-001 | Training | teach, train (human), explain, upskill, workshop, TDD (methodology), bootcamp |

All three: DomainId = software-development | SCHEDULE_CAPABILITY_LEARNING emitted.
