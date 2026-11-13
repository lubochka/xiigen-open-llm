# Layer 3 — Teaching Unit Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-003 — Embedded verb rule: guidance frames do not suppress code verbs

**Rule**: When a guidance frame wraps a code verb, the code verb fires its predicate. The guidance wrapper becomes a gap warning, not a full gap.

**Pattern**: "[guidance verb] + [code verb + artifact]"

**Examples**:

| Request | Gap component | Code verb | Predicate |
|---------|--------------|-----------|-----------|
| "Mentor the team and build the skill tracker" | mentor → gap warning | build | IMPLEMENT |
| "Coach the team and write tests for the service" | coach → gap warning | write tests | TEST |
| "Guide the team through documenting the API" | guide through → gap warning | documenting | DOCUMENT |
| "Walk them through architecting the system" | walk through → gap warning | architecting | ARCHITECTURE |

**Exception**: "Teach to [verb]" → gap. The infinitive is what others will learn, not what the requester does.
- "Teach the team to write tests" → gap (teach = requester's action; write tests = others' future action)
- "Write tests for the service" → TEST (direct requester action)
