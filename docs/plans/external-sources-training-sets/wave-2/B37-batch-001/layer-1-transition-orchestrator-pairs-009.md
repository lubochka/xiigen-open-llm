# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-009-001 — One SCHEDULE_CAPABILITY_LEARNING per request (B37 gap)

**Rule:** One SCHEDULE_CAPABILITY_LEARNING event per zero-predicate request — regardless of how many gap IDs are present.

**Example 1:** "Track blockers and coordinate escalations."
- Gap IDs: gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-escalation-001
- SCHEDULE_CAPABILITY_LEARNING events emitted: 1 (not 4)

**Example 2:** "Manage delivery, track progress, coordinate workstreams, escalate blockers, update stakeholders on RAG-status."
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-track-progress-001, gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001, gap-project-managing-rag-status-001
- Wait: "stakeholders" → DOCUMENT fires
- Corrected: ["DOCUMENT"], DomainId "software-development"
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicate fired)

**Corrected Example 2 without "stakeholders":**
"Manage delivery, track progress, coordinate workstreams, escalate blockers."
- Gap IDs: 4 gap IDs
- SCHEDULE_CAPABILITY_LEARNING: 1 event

---

## B37-L1-009-002 — SCHEDULE_CAPABILITY_LEARNING payload structure

**SCHEDULE_CAPABILITY_LEARNING event payload:**

```
{
  event_type: "SCHEDULE_CAPABILITY_LEARNING",
  scenario: "B37",
  domain: "project-management",
  batch: "B37-batch-001",
  gap_ids: ["gap-project-managing-track-001", ...],
  request_text: "[original request]",
  timestamp: "[ISO timestamp]",
  request_hash: "[hash for deduplication]"
}
```

One event per request, not one per gap ID.

---

## B37-L1-009-003 — SCHEDULE_CAPABILITY_LEARNING not emitted when predicate fires

**Confirming the rule:**

Request A: "Track blockers." → [] → SCL emitted
Request B: "Track blockers and review the RAID log." → ["QA"] → SCL NOT emitted
Request C: "Track blockers and notify stakeholders." → ["DOCUMENT"] → SCL NOT emitted
Request D: "Track blockers and build the tracking module." → ["IMPLEMENT"] → SCL NOT emitted

Adding ANY predicate-firing token suppresses SCL emission.

---

## B37-L1-009-004 — SCL and TransitionType interaction

**When SCL is emitted:**
- TransitionType: unknown
- SCL: emitted
- Next state: knowledge_edge

**When SCL is NOT emitted:**
- TransitionType: set by predicate(s)
- SCL: not emitted
- Next state: appropriate flow(s)

These are mutually exclusive. SCL emission ↔ TransitionType "unknown" ↔ knowledge_edge route.

---

## B37-L1-009-005 — SCHEDULE_CAPABILITY_LEARNING batch accumulation

**Batch purpose:** Multiple SCHEDULE_CAPABILITY_LEARNING events from B37 requests accumulate in the learning pipeline. The batch B37-batch-001 training set provides the model with examples of how to correctly handle these requests.

After batch training:
- Model correctly emits SCL for zero-predicate B37 requests
- Model correctly forwards gap IDs
- Model correctly sets DomainId "project-management"
- Model correctly routes to knowledge_edge
- Model correctly does NOT invent managing predicates (v6-01 corrected)
- Model correctly does NOT suppress DOCUMENT for stakeholder (v6-02 corrected)
- Model correctly sets DomainId "software-development" when predicate fires (v6-03 corrected)
