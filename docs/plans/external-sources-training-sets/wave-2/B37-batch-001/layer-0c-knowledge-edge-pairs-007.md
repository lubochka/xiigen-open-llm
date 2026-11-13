# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-007-001 — B36 vs B37 gap domain distinction: project-planning vs project-managing

**Knowledge edge: Gap domain IDs for B36 and B37**

**B36 gap domain = "project-planning"**
B36 scenario: Sprint planning, roadmap, milestone setting
B36 gap vocabulary: sprint, roadmap, milestone, planning, estimation, backlog, capacity, velocity, story points, release plan
B36 gap ID format: gap-project-planning-{term}-{sequence}

**B37 gap domain = "project-managing"**
B37 scenario: Progress tracking, coordination, status reporting
B37 gap vocabulary: track, coordinate, blockers, dependencies (no API), stakeholder-update, escalation, accountability, follow-up, action-items, on-track/off-track, RAG-status, RAID-log, progress, status-report, manage, delivery-status
B37 gap ID format: gap-project-managing-{term(s)}-{sequence}

---

## B37-0C-007-002 — Why the gap domains differ

**B36 is about PLANNING:** Sprint planning, roadmap definition, milestone setting are future-oriented planning activities. They occur before execution.

**B37 is about MANAGING:** Progress tracking, blocker escalation, status reporting are in-execution management activities. They occur during and after work begins.

**Both have DomainId "project-management"** for zero-predicate requests (Rule 18). The DomainId is the same. Only the gap_id sub-domain differs.

---

## B37-0C-007-003 — Gap ID boundary: which vocabulary belongs to which batch

**B36-only vocabulary (project-planning):**
- sprint → gap-project-planning-sprint-001
- roadmap → gap-project-planning-roadmap-001
- milestone → gap-project-planning-milestone-001
- velocity → gap-project-planning-velocity-001
- backlog → gap-project-planning-backlog-001
- story points → gap-project-planning-story-points-001

**B37-only vocabulary (project-managing):**
- track progress → gap-project-managing-track-progress-001
- coordinate → gap-project-managing-coordinate-001
- blockers → gap-project-managing-blockers-001
- escalation → gap-project-managing-escalation-001
- RAID log → gap-project-managing-raid-log-001
- RAG-status → gap-project-managing-rag-status-001
- action items → gap-project-managing-action-items-001
- follow-up → gap-project-managing-follow-up-001

**Mixed sentences containing both B36 and B37 vocabulary:**
Use the appropriate gap ID for each token — do NOT use "project-planning" for B37 tokens or vice versa.

---

## B37-0C-007-004 — Mixed B36+B37 sentence: correct gap ID assignment

**Input:** "Plan the sprint milestones and track the blockers affecting delivery."

**Analysis:**
- "Plan" → gap — B36 context → gap-project-planning-{term}
- "sprint" → gap-project-planning-sprint-001 (B36)
- "milestones" → gap-project-planning-milestone-001 (B36)
- "track" → gap-project-managing-track-001 (B37)
- "blockers" → gap-project-managing-blockers-001 (B37)
- "delivery" → gap-project-managing-manage-delivery-001 (B37)

Result: []

DomainId: "project-management"

CapabilityGapWarnings (correctly separated by domain):
- B36: gap-project-planning-sprint-001, gap-project-planning-milestone-001
- B37: gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001

---

## B37-0C-007-005 — Error: using wrong gap domain

**Incorrect (v6 anti-pattern):**
Request: "Coordinate escalations and track delivery blockers."
v6 error: gap-project-planning-coordinate-001 (wrong domain — "project-planning" used for B37 vocabulary)

**Correct:**
gap-project-managing-coordinate-001 (B37 domain)
gap-project-managing-escalation-001 (B37 domain)
gap-project-managing-track-001 (B37 domain)
gap-project-managing-blockers-001 (B37 domain)

B37 gap vocabulary always uses "project-managing" sub-domain, never "project-planning".
