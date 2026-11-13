# Layer 0-B — Router Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-005-001 — [ARCHITECTURE] → Phase 3.1

**Input predicate list:** `["ARCHITECTURE"]`
**Request origin:** "Design the system architecture for the sprint deliverables and plan the milestone dependencies."

**Router decision tree:**

```
predicates contains ARCHITECTURE? YES → route to Phase 3.1 (ArchitectureFlow31)
predicates contains IMPLEMENT? NO
predicates contains DOCUMENT? NO
predicates contains QA? NO
→ Single path: ARCHITECTURE → Phase 3.1
→ TransitionType: documentation (ARCHITECTURE maps to documentation transition type)
→ DomainId: "software-development"
→ CapabilityGapWarnings: sprint-001, milestone-001
```

**Router output:**
- Path selected: Phase 3.1 — ArchitectureFlow31
- TransitionType: documentation
- CapabilityGapWarnings forwarded: sprint-001, milestone-001

---

## B36-0B-005-002 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1

**Input predicate list:** `["ARCHITECTURE", "DOCUMENT"]`
**Request origin:** "Architect the data flow for the upcoming sprint and document the milestone delivery plan."

**Router decision:**
- ARCHITECTURE → Phase 3.1
- DOCUMENT → Phase 2.1
- Both fire → both phases selected
- TransitionType: documentation (both ARCHITECTURE and DOCUMENT map to documentation)
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, milestone-001

---

## B36-0B-005-003 — [ARCHITECTURE] in planning sentence: routing unchanged

**Key point for 005:**
When "design" or "architect" appears in a sentence about sprint planning, the router still routes to Phase 3.1. The sprint/milestone/planning context does not redirect to KNOWLEDGE_EDGE. The router follows the predicate, not the context.

**Contrast:**
- "Design the sprint structure" → [ARCHITECTURE] → Phase 3.1 (ARCHITECTURE fired)
- "Structure the sprint tasks" → [] if "structure" is not the same as the verb "structure" triggering... wait: "structure" fires ARCHITECTURE per B36 Rule 4.
- "Organize the sprint tasks" → [] (no predicate on "organize") → KNOWLEDGE_EDGE

---

## B36-0B-005-004 — [ARCHITECTURE] TransitionType is documentation, not feature_addition

**Common mistake:** Treating ARCHITECTURE routing as feature_addition because it "builds" something.

**Correct mapping:**

| Predicate | TransitionType |
|-----------|---------------|
| IMPLEMENT | feature_addition |
| DOCUMENT | documentation |
| ARCHITECTURE | documentation |
| QA | no_contribution |

ARCHITECTURE maps to documentation because the output of an architecture phase is architectural documentation, not deployed code. The TransitionType reflects the output artifact, not the subject matter.
