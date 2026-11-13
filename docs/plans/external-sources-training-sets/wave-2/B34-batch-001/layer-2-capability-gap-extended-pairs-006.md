# Layer 2 — Capability Gap Extended Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-006 — GapHandler nine-node: "Onboard new engineers"

Request: "Onboard the three new engineers joining the payments team next week."

### Flow: GapHandler Nine-Node

**Node 1**: Parse. "onboard" → onboarding vocabulary → gap.
**Node 2**: Predicate scan: "new engineers", "payments team" = contextual nouns; no code-action verb. Predicate set = [].
**Node 3**: Gap classification: onboarding/orientation. Gap category: onboard.
**Node 4**: Gap ID: gap-skills-guidance-onboard-001
**Node 5**: DomainId: software-development
**Node 6**: Emit SCHEDULE_CAPABILITY_LEARNING
**Node 7**: Log: {id: gap-skills-guidance-onboard-001, request: "onboard 3 new engineers", domainId: software-development}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED

---

**Contrast**: "Build the automated onboarding checklist tool" → IMPLEMENT (code verb + software tool noun).
