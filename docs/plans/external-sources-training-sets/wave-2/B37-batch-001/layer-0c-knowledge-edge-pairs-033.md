# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-033-001 — Gap ID assignment for Round 3a vocabulary (037-048)

| Vocabulary | Gap ID(s) |
|-----------|---------|
| escalation process | gap-project-managing-escalation-001 |
| communication plan | gap-project-managing-manage-delivery-001 |
| project kickoff | gap-project-managing-manage-delivery-001 |
| steering committee | gap-project-managing-manage-delivery-001 |
| change control board | gap-project-managing-manage-delivery-001 + coordinate-001 |
| scope creep | gap-project-managing-manage-delivery-001 |
| risk mitigation | gap-project-managing-raid-log-001 |
| quality gate | gap-project-managing-manage-delivery-001 |
| sign-off process | gap-project-managing-manage-delivery-001 |
| post-mortem | gap-project-managing-manage-delivery-001 |

---

## B37-0C-033-002 — Gap ID assignment for Round 4 vocabulary (049-060)

| Vocabulary | Gap ID(s) |
|-----------|---------|
| waterfall | gap-project-managing-manage-delivery-001 |
| agile transformation | gap-project-managing-manage-delivery-001 |
| SAFe | gap-project-managing-manage-delivery-001 + coordinate-001 |
| program increment | gap-project-managing-manage-delivery-001 |
| portfolio management | gap-project-managing-manage-delivery-001 + coordinate-001 |
| value stream | gap-project-managing-manage-delivery-001 |
| flow state | gap-project-managing-manage-delivery-001 |
| project closure | gap-project-managing-manage-delivery-001 |
| benefits realization | gap-project-managing-manage-delivery-001 + track-001 |
| KPI monitoring | gap-project-managing-manage-delivery-001 |

---

## B37-0C-033-003 — Gap domain "project-managing" vs "project-management" DomainId

**Critical distinction:**

Gap ID sub-domain: "project-managing" (e.g., gap-project-managing-escalation-001)
Output DomainId: "project-management" (for zero-predicate requests)

These are DIFFERENT strings. Gap ID uses "project-managing" (hyphen, no 't'). DomainId uses "project-management" (hyphen, 't').

---

## B37-0C-033-004 — SCL + gap forwarding rule for Round 3+4

**For all Round 3+4 zero-predicate requests:**
1. SCHEDULE_CAPABILITY_LEARNING: one event emitted
2. CapabilityGapWarnings: list of all gap IDs identified in the request
3. DomainId: "project-management"
4. Route: knowledge_edge

**For all Round 3+4 predicate-firing requests:**
1. SCHEDULE_CAPABILITY_LEARNING: NOT emitted
2. CapabilityGapWarnings: STILL forwarded (gap IDs identified even when predicate fires)
3. DomainId: "software-development"
4. Route: appropriate flow

---

## B37-0C-033-005 — Knowledge-edge handling: all rounds combined

**B37 knowledge-edge applies to ALL rounds (002-060):**

Any PM managing vocabulary from any round, used alone without predicate triggers → knowledge_edge.
One SCL per request.
DomainId "project-management".
All identified gap IDs forwarded.

The only exceptions (terms that self-trigger predicate):
- "work breakdown structure" → ARCHITECTURE (via "structure")
- "handover document" → DOCUMENT (via "document")
- Any compound with "service", "module", "build", "create", "implement" → IMPLEMENT
- Any compound with "stakeholder" → DOCUMENT
- Any compound with "structure", "design", "architecture" → ARCHITECTURE
