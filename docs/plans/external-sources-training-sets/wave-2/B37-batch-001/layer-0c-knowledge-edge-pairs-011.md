# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-011-001 — Mixed-signal handling: predicates dominate; managing vocab → warnings only

**Knowledge edge: Mixed-signal B37 request handling**

When a B37 request contains BOTH predicate-firing tokens AND gap vocabulary:

1. Predicate fires → routing to sub-phase (normal)
2. Gap vocabulary → CapabilityGapWarnings emitted (not routing)
3. DomainId = "software-development" (predicate fired)
4. SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicate fired)

The predicate always dominates. Gap warnings are advisory. Gap warnings do NOT suppress routing and do NOT change DomainId.

---

## B37-0C-011-002 — Mixed-signal examples: predicate dominates

**Example 1:** "Manage the delivery of the service and track progress."
- "Manage" → gap (Rule 10)
- "delivery" → gap
- "service" → IMPLEMENT fires (Rule 9)
- "track" → gap (Rule 2)
- "progress" → gap (Rule 12)

Result: ["IMPLEMENT"], DomainId "software-development"
Gap warnings: gap-project-managing-manage-delivery-001, gap-project-managing-track-progress-001
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Example 2:** "Coordinate escalations and notify stakeholders."
- "Coordinate" → gap (Rule 11)
- "escalations" → gap (Rule 13)
- "stakeholders" → DOCUMENT fires (Rule 6)

Result: ["DOCUMENT"], DomainId "software-development"
Gap warnings: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

## B37-0C-011-003 — Pure gap: no mixed signal

**When SCHEDULE_CAPABILITY_LEARNING IS emitted:**
- Zero predicates fired
- All vocabulary is gap vocabulary
- DomainId "project-management"
- Route: knowledge_edge

**Examples of pure gap requests:**
1. "Track all delivery blockers and coordinate with team leads."
2. "Manage escalations and report on RAG-status."
3. "Follow up on action items from the RAID log."
4. "Maintain on-track status across all workstreams."
5. "Coordinate the delivery programme dependencies."

All five: [], DomainId "project-management", knowledge_edge, SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B37-0C-011-004 — Gap warning does not consume the gap

**Gap warnings are informational signals, not routing actions.**

A CapabilityGapWarning:
- Records the gap for learning/improvement
- Does NOT route to any flow
- Does NOT prevent the predicate from routing
- Does NOT modify DomainId
- Does NOT modify the predicate result

The gap vocabulary in a mixed-signal request is "acknowledged but unaddressed" — the predicate fires, routing proceeds, and the gap is noted for future capability development.

---

## B37-0C-011-005 — Mixed-signal handling summary table

| Request type | Predicates | DomainId | Route | Gap warnings | SCL |
|-------------|-----------|---------|-------|-------------|-----|
| Pure gap | [] | project-management | knowledge_edge | Yes | Yes |
| Mixed (1 predicate) | [X] | software-development | X's route | Yes (for gap vocab) | No |
| Mixed (2 predicates) | [X, Y] | software-development | X + Y routes | Yes (for gap vocab) | No |
| Pure predicate (no gap vocab) | [X] | software-development | X's route | No | No |

SCL = SCHEDULE_CAPABILITY_LEARNING
