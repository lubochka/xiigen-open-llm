# Cross-Layer Synthesis (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## B36-CLS-006-001 — "story" false positive: full layer trace

**Request:** "Assign story points to backlog items and prioritize for the next sprint."

**Layer 0-A — Compiler:**

Token scan:
- assign: no predicate
- story: DOCUMENT fires (Rule 19: "story" fires DOCUMENT unconditionally — false positive accepted)
- points: no predicate (compound "story points" does not block DOCUMENT from "story")
- backlog: GAP → backlog-001
- prioritize: GAP → no predicate (Rule 20: prioritize is gap vocabulary)
- sprint: GAP → sprint-001

Predicate output: `["DOCUMENT"]`
CapabilityGapWarnings: backlog-001, sprint-001
Note: DOCUMENT fired via false positive on "story" in "story points" context.

---

**Layer 0-B — Router:**

predicates: ["DOCUMENT"]
→ Phase 2.1 (DOCUMENT)
DomainId: "software-development" (one predicate fired — regardless of intent)
KNOWLEDGE_EDGE: false
TransitionType: documentation
PhaseId: 2.1

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (DOCUMENT fired)
Gap warnings forwarded as metadata only
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

**Layer 0-D — Factory:**

FlowFactory.Select(DOCUMENT) → DocumentationFlow (Phase 2.1)

---

**Layer 1 — Transition Orchestrator:**

transitionType: documentation
domainId: "software-development"
contextCarryForward: true
phaseId: 2.1
finalHumanResult: populated (from DocumentationFlow)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicate fired)
capabilityGapWarnings: backlog-001, sprint-001 (forwarded as metadata)

---

**Layer 2 — Gap Extended:**

DOCUMENT fired; gap warnings present but do not affect routing:
- backlog-001: backlog context present in documentation request
- sprint-001: sprint context present in prioritization request

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 19: "story" fires DOCUMENT even inside "story points" — known false positive, never suppress
- Rule 20: "prioritize" is gap vocabulary — does not fire any predicate
- Rule 1: backlog, sprint are gap vocabulary
- Rule 7: DomainId "software-development" — one predicate fired (even false positive counts)
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted because DOCUMENT fired
- ContextCarryForward: true (predicate fired)

---

## B36-CLS-006-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["DOCUMENT"] via "story" false positive |
| Router | Phase 2.1; DomainId: software-development |
| Factory | DocumentationFlow selected |
| Orchestrator | documentation transition; FinalHumanResult: populated; no SCHEDULE_CAPABILITY_LEARNING |
| Gap extended | backlog-001, sprint-001 (warnings — routing not affected) |
| Teaching | Rules 19, 20, 1, 7 applied; false positive demonstrated |

---

## B36-CLS-006-003 — False positive synthesis: the "story" boundary case

This synthesis demonstrates the "story" false positive boundary:
1. "story points" contains "story" → DOCUMENT fires unconditionally (false positive)
2. The false positive is accepted: never suppress DOCUMENT for "story"
3. DomainId becomes "software-development" even though intent is sprint estimation
4. SCHEDULE_CAPABILITY_LEARNING is NOT emitted because DOCUMENT fired
5. The routing proceeds on the false positive: DocumentationFlow executes

The v6-02 corruption suppresses DOCUMENT here claiming "story points is estimation, not documentation."
The correct behavior accepts the false positive and routes through DOCUMENT unconditionally.
