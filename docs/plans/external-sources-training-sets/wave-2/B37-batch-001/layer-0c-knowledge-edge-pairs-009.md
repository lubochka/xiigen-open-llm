# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-009-001 — v6 corruption types in B37: complete taxonomy

**Three v6 corruption types specific to B37:**

**v6-01: Inventing MANAGEMENT or COORDINATION predicate**
- Trigger: "coordinate", "manage", "track" vocabulary with managing intent
- v6 behaviour: Invents a non-existent predicate (MANAGEMENT, COORDINATION, PROJECT_MANAGEMENT)
- Correct behaviour: Zero predicates; gap warnings; knowledge_edge route
- Detection: Predicate list contains tokens not in the registered predicate registry

**v6-02: Suppressing DOCUMENT for "stakeholder" in managing context**
- Trigger: "stakeholder" appears in a sentence framed as status reporting or managing communication
- v6 behaviour: Suppresses DOCUMENT because "the context is managing, not documentation"
- Correct behaviour: DOCUMENT fires ALWAYS on "stakeholder" regardless of context
- Detection: Sentence contains "stakeholder" but predicate list is empty or missing DOCUMENT

**v6-03: Setting DomainId "project-management" when predicate fires**
- Trigger: Managing vocabulary surrounding a predicate-firing token
- v6 behaviour: Sets DomainId "project-management" because "the request is about project management"
- Correct behaviour: DomainId = "software-development" whenever ANY predicate fires
- Detection: Predicate list is non-empty but DomainId = "project-management"

---

## B37-0C-009-002 — v6-01 deep analysis: why MANAGEMENT predicate does not exist

The predicate registry contains: TEST, QA, DOCUMENT, ARCHITECTURE, IMPLEMENT, API_CONTRACT.

"MANAGEMENT", "COORDINATION", "PROJECT_MANAGEMENT", "TRACKING" are NOT registered predicates. A model that invents these is hallucinating predicate names.

**Root cause of v6-01:** The model correctly identifies managing vocabulary and incorrectly concludes there must be a corresponding predicate for it. The correct conclusion is: managing vocabulary → capability gap → no predicate registered → gap warning.

---

## B37-0C-009-003 — v6-02 deep analysis: why DOCUMENT cannot be suppressed

The lexicon is the law. "stakeholder" → DOCUMENT is an unconditional lexical rule. The compiler does not evaluate:
- Whether the sentence intent is documentation
- Whether the "stakeholder" reference is genuine documentation work
- Whether the context is managing, testing, or anything else

Any contextual override is a v6-02 corruption. The only correct response when "stakeholder" appears is DOCUMENT fires.

**Implication:** Every B37 request mentioning "stakeholders" will false-positive into DOCUMENT routing. This is the correct behaviour for the current lexicon state.

---

## B37-0C-009-004 — v6-03 deep analysis: why DomainId cannot bleed

DomainId reflects routing destination, not request intent. "project-management" as a DomainId signals: no predicate matched, capability gap, send to knowledge_edge and emit SCHEDULE_CAPABILITY_LEARNING.

"software-development" as a DomainId signals: at least one predicate matched, route to appropriate sub-phase.

When a predicate fires alongside managing vocabulary:
- The predicate determines routing
- The managing vocabulary generates gap warnings
- DomainId = "software-development" (predicate fired)

Allowing "project-management" DomainId when predicates fire would break the routing contract.

---

## B37-0C-009-005 — v6 corruption compound: all three in one request

**Input that triggers all three v6 corruptions:**
"Coordinate the delivery and update stakeholders on current status."

**Correct analysis:**
- "Coordinate" → gap (no predicate — Rule 11)
- "delivery" → gap
- "update" → not a DOCUMENT trigger
- "stakeholders" → DOCUMENT fires (Rule 6)
- "status" → gap (Rule 4)

Correct result: ["DOCUMENT"], DomainId: "software-development"

**v6 corrupt analysis (all three errors):**
- v6-01: Invents COORDINATION predicate for "coordinate"
- v6-02: Suppresses DOCUMENT because "this is a status update, not documentation"
- v6-03: Sets DomainId "project-management" because "this is a managing request"

v6 result: ["COORDINATION"], DomainId: "project-management" — wrong on all three counts.
