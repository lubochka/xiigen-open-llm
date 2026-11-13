# Layer 3 — Teaching-Unit Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-006-001 — Teaching: B36 vs B37 domain comparison

**Two related capability gap scenarios:**

**B36 — Project Planning:**
Scenario: Sprint planning, roadmap, milestone setting (PLANNING = future-oriented)
Gap domain: "project-planning"
Gap vocabulary: sprint, roadmap, milestone, estimation, backlog, velocity, story points, capacity
Gap ID format: gap-project-planning-{term}-{sequence}
DomainId (zero predicates): "project-management"

**B37 — Project Managing:**
Scenario: Progress tracking, coordination, status reporting (MANAGING = in-execution)
Gap domain: "project-managing"
Gap vocabulary: track, coordinate, blockers, escalation, accountability, follow-up, action-items, RAG-status, RAID-log, on-track/off-track, progress, status-report
Gap ID format: gap-project-managing-{term}-{sequence}
DomainId (zero predicates): "project-management"

---

## B37-L3-006-002 — Teaching: shared DomainId, different gap domains

**Both B36 and B37 share DomainId "project-management" for zero-predicate requests.**

The DomainId "project-management" signals: no software predicate matched; this is a project management domain request. This applies to both planning (B36) and managing (B37).

**The distinction is in the gap_id subdomain:**
- B36 gap → gap-project-planning-*
- B37 gap → gap-project-managing-*

Never use "project-planning" for B37 vocabulary. Never use "project-managing" for B36 vocabulary.

---

## B37-L3-006-003 — Teaching: mixed B36+B37 sentence

**Input:** "Plan the sprint milestones and track escalations for the current delivery."

**Token analysis:**
- "Plan" → B36 territory (planning); not a predicate
- "sprint" → B36 gap vocabulary
- "milestones" → B36 gap vocabulary
- "track" → B37 gap vocabulary (Rule 2: track ≠ TEST)
- "escalations" → B37 gap vocabulary (Rule 13)
- "delivery" → B37 gap vocabulary

**Correct gap ID assignment:**
- B36: gap-project-planning-sprint-001, gap-project-planning-milestone-001
- B37: gap-project-managing-track-001, gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001

Result: [] — zero predicates
DomainId: "project-management"

**Key: Mixed B36+B37 sentence still produces zero predicates and DomainId "project-management". Gap IDs are assigned per their respective batch domains.**

---

## B37-L3-006-004 — Teaching: why the domain split matters

**Practical importance:** The gap domain determines which capability learning cluster receives the signal.

- B36 gap signals → project-planning learning cluster
- B37 gap signals → project-managing learning cluster

If B37 vocabulary is mis-tagged as B36, the learning signal goes to the wrong cluster. The system would incorrectly learn that sprint/roadmap vocabulary needs improvement when actually the managing (tracking/escalation) vocabulary needs improvement.

**Cross-contamination is a training error.** B37-batch-001 explicitly trains the correct gap domain assignment for managing vocabulary.

---

## B37-L3-006-005 — Teaching: B36 vs B37 boundary vocabulary

**Boundary words that could be confused:**

| Token | Domain | Notes |
|-------|--------|-------|
| sprint | B36 (planning) | Sprint planning, not sprint tracking |
| milestone | B36 (planning) | Setting milestones, not tracking them |
| track milestones | B37 (managing) | Tracking activity = managing |
| roadmap | B36 (planning) | Creating roadmap = planning |
| blockers (vs risks) | B37 (managing) | Blockers = in-execution impediment |
| risks | Both (RAID context) | B37 via RAID log |
| capacity | B36 (planning) | Sprint capacity planning |
| progress | B37 (managing) | In-execution progress tracking |
| delivery | B37 (managing) | In-execution delivery management |
