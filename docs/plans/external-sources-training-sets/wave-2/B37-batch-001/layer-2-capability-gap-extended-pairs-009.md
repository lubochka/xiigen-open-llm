# Layer 2 — Capability-Gap-Extended Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-009-001 — Follow-up + action items: full knowledge_edge trace

**Input:** "Follow up on all open action items from yesterday's project standup."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Follow up" → not in any predicate lexicon → gap
- "open" → not in any predicate lexicon
- "action items" → not in any predicate lexicon → gap
- "project" → not in any predicate lexicon
- "standup" → not in any predicate lexicon

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "follow up" → gap-project-managing-follow-up-001
- "action items" → gap-project-managing-action-items-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING emitted

**Gap: gap-project-managing-follow-up-001, gap-project-managing-action-items-001**

---

## B37-L2-009-002 — Action items with "review": QA fires

**Input:** "Review the outstanding action items and assign owners for each unresolved item."

**Trace:**
- "Review" → QA fires (Rule 19)
- "action items" → gap
- "assign" → not in any predicate lexicon
- "owners" → not in any predicate lexicon (accountability)
- "unresolved" → not in any predicate lexicon

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-action-items-001, gap-project-managing-accountability-001 (owners = accountability)

---

## B37-L2-009-003 — Follow-up with "document": DOCUMENT fires

**Input:** "Document the follow-up actions agreed in the steering committee meeting."

**Trace:**
- "Document" → DOCUMENT fires (Rule 8)
- "follow-up" → gap
- "actions" → not in any predicate lexicon
- "agreed" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-follow-up-001, gap-project-managing-action-items-001

---

## B37-L2-009-004 — Action items + escalation: full gap

**Input:** "Escalate overdue action items to the delivery manager and follow up by end of week."

**Trace:**
- "Escalate" → gap (Rule 13)
- "action items" → gap
- "delivery" → gap
- "manager" → not in any predicate lexicon
- "follow up" → gap

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-escalation-001, gap-project-managing-action-items-001, gap-project-managing-follow-up-001, gap-project-managing-manage-delivery-001

SCL emitted

---

## B37-L2-009-005 — Five follow-up/action items scenarios

1. "Send follow-up to all action item owners." → [] (follow-up + action items = gap)
2. "Review follow-up status for all action items." → ["QA"] (review fires)
3. "Define action item criteria and escalation thresholds." → ["DOCUMENT"] (define + criteria fire)
4. "Build the action item tracking module." → ["IMPLEMENT"] (build + module fire)
5. "Update stakeholders on follow-up progress." → ["DOCUMENT"] (stakeholders fires)

In cases 2-5: managing intent captured in gap warnings (follow-up-001, action-items-001); predicate routing proceeds to non-managing flows.
