# Layer 0-B — Router Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-004-001 — [IMPLEMENT, DOCUMENT] → 4.1 + 2.1

**Input predicate list:** `["IMPLEMENT", "DOCUMENT"]`
**Request origin:** "Write the user stories for the checkout feature and estimate the story points."

**Router decision tree:**

```
predicates contains IMPLEMENT? YES → route to Phase 4.1 (BackendServiceFlow41)
predicates contains DOCUMENT? YES → route to Phase 2.1 (DocumentationFlow)
predicates contains QA? NO
→ Parallel eligible: IMPLEMENT (4.1) + DOCUMENT (2.1) can run in parallel
→ CapabilityGapWarnings forwarded: story-point-001
→ DomainId: "software-development"
→ TransitionType: feature_addition (IMPLEMENT is primary)
```

**Router output:**
- Primary path: IMPLEMENT → Phase 4.1
- Secondary path: DOCUMENT → Phase 2.1
- Parallel eligible: yes
- CapabilityGapWarnings: story-point-001
- DomainId: "software-development"

---

## B36-0B-004-002 — [IMPLEMENT, DOCUMENT] ordering

**When IMPLEMENT and DOCUMENT both fire:**
- IMPLEMENT is always primary
- TransitionType is feature_addition (driven by IMPLEMENT)
- DOCUMENT runs in parallel with IMPLEMENT, not sequentially after
- QA (if present) runs last — after both IMPLEMENT and DOCUMENT complete

**This ordering is fixed regardless of the sentence structure.** Even if the user said "document the requirements then implement the feature," the router places IMPLEMENT as primary.

---

## B36-0B-004-003 — [IMPLEMENT, DOCUMENT] with gap warnings: warnings don't change ordering

**Input predicate list:** `["IMPLEMENT", "DOCUMENT"]`
**Request origin:** "Build the sprint tracking service, document the backlog requirements."

**Gap vocabulary present:** sprint, backlog
**CapabilityGapWarnings emitted:** sprint-001, backlog-001

**Router decision:** Same as B36-0B-004-001 — IMPLEMENT primary, DOCUMENT parallel.
Gap warnings are attached to the RouterResult but do NOT change which phases execute or their ordering.

---

## B36-0B-004-004 — [DOCUMENT] only → Phase 2.1

**Input predicate list:** `["DOCUMENT"]`
**Request origin:** "Document the milestone deliverables and define the acceptance criteria."

**Router decision:**
- IMPLEMENT: not present
- DOCUMENT: present → Phase 2.1
- QA: not present
- Path: DOCUMENT → DocumentationFlow, Phase 2.1
- TransitionType: documentation
- DomainId: "software-development"
- CapabilityGapWarnings: milestone-001
