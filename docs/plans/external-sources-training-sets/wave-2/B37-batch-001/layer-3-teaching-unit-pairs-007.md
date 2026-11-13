# Layer 3 — Teaching-Unit Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-007-001 — Teaching: DomainId "project-management" for both B36 and B37 zero-predicate requests

**Shared DomainId rule:**

Both B36 and B37 produce DomainId "project-management" for zero-predicate requests.

**Why the same DomainId?**
The DomainId represents the CAPABILITY DOMAIN of the request, not the gap sub-domain. Both B36 planning and B37 managing are within the "project management" capability domain. Neither is software development. Neither fires software development predicates.

The DomainId "project-management" means: "This request is in the project management domain. The system does not have a registered predicate for this domain."

---

## B37-L3-007-002 — Teaching: when DomainId "project-management" is set

**Set to "project-management":**
- Zero predicates fired
- DomainId = "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted
- Route: knowledge_edge

**Set to "software-development":**
- One or more predicates fired
- DomainId = "software-development"
- SCHEDULE_CAPABILITY_LEARNING NOT emitted
- Route: appropriate flow(s)

**The switch is binary.** No gradation. Even one predicate fires → "software-development".

---

## B37-L3-007-003 — Teaching: DomainId does not reflect request intent

**Example of DomainId not reflecting intent:**

Request: "Notify stakeholders about the escalation status."
Intent: Project managing (stakeholder notification)
Predicate: DOCUMENT (fires on "stakeholders")
DomainId: "software-development"

The DomainId says "software-development" but the intent is project managing. DomainId reflects PREDICATE FIRING, not request intent.

The managing intent is captured in:
- CapabilityGapWarning: gap-project-managing-stakeholder-update-001
- CapabilityGapWarning: gap-project-managing-escalation-001

---

## B37-L3-007-004 — Teaching: five DomainId determination examples

1. "Track blockers." → [] → DomainId "project-management"
2. "Review blockers." → ["QA"] → DomainId "software-development" (QA fired)
3. "Document blockers." → ["DOCUMENT"] → DomainId "software-development" (DOCUMENT fired)
4. "Build the blocker tracker module." → ["IMPLEMENT"] → DomainId "software-development" (IMPLEMENT fired)
5. "Track, review, and document blockers." → ["QA", "DOCUMENT"] → DomainId "software-development" (predicates fired)

Case 1: zero predicates → "project-management". Cases 2-5: predicates → "software-development".

---

## B37-L3-007-005 — Teaching: DomainId is computed, not assumed

**Anti-pattern:** Model assumes DomainId from request topic before scanning.

"This request is about project management, so DomainId = 'project-management'." — WRONG.

**Correct pattern:** Scan tokens → check predicates → compute DomainId from predicate result.

The request topic (project managing) does not determine DomainId. Only the predicate scan result determines DomainId. This prevents v6-03 corruption where managing framing causes wrong DomainId even when a predicate has fired.
