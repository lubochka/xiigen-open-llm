# Held-Out and Corruption Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-010-001 — HO: pure managing vocabulary (follow-up, action items, blockers) → knowledge_edge

**Held-out request:** "Follow up with all assignees on their action items and resolve outstanding blockers."

**Expected output:**
- "Follow up" → gap (gap-project-managing-follow-up-001)
- "assignees" → not in any predicate lexicon
- "action items" → gap (gap-project-managing-action-items-001)
- "resolve" → not in any predicate lexicon
- "outstanding" → not in any predicate lexicon
- "blockers" → gap (gap-project-managing-blockers-001)
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-follow-up-001, gap-project-managing-action-items-001, gap-project-managing-blockers-001

---

## B37-HO-010-002 — HO: follow-up + escalation + accountability

**Request:** "Follow up on escalations and confirm accountability for all unresolved action items."

**Expected output:**
- "Follow up" → gap
- "escalations" → gap (Rule 13)
- "accountability" → gap
- "action items" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-follow-up-001, gap-project-managing-escalation-001, gap-project-managing-accountability-001, gap-project-managing-action-items-001

---

## B37-HO-010-003 — HO: action items + review → QA fires

**Request:** "Review all action items and confirm accountability ownership."

**Expected output:**
- "Review" → QA fires (Rule 19)
- "action items" → gap
- "accountability" → gap
- "ownership" → not in any predicate lexicon
- Predicates: ["QA"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-action-items-001, gap-project-managing-accountability-001

---

## B37-HO-010-004 — HO: follow-up + build → IMPLEMENT fires

**Request:** "Build the action item tracker module and follow up with team leads weekly."

**Expected output:**
- "Build" → IMPLEMENT fires
- "action item" → gap
- "tracker" → not in predicate lexicon directly
- "module" → IMPLEMENT fires
- "follow up" → gap
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-action-items-001, gap-project-managing-follow-up-001

---

## B37-HO-010-005 — HO: five pure managing vocabulary patterns → all knowledge_edge

1. "Send follow-up reminders for overdue action items." → [] knowledge_edge
2. "Escalate blockers to the PMO and track resolution." → [] knowledge_edge
3. "Maintain the RAID log and coordinate with programme leads." → [] knowledge_edge
4. "Report on-track/off-track status for each workstream." → [] knowledge_edge
5. "Manage accountability for all delivery workstreams." → [] knowledge_edge

All five: DomainId "project-management", knowledge_edge, SCL emitted.

No predicate fires because none of the tokens (follow-up, action items, escalate, blockers, RAID, coordinate, on-track, off-track, manage, accountability, workstream) are in any predicate lexicon.
