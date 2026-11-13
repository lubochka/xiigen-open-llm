# Layer 3 — Teaching Unit Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-008-001 — Zero predicates in B32: SCHEDULE_CAPABILITY_LEARNING with DomainId: software-development

**Teaching unit:** When a B32 request produces zero predicates, the system must:
1. Route to knowledge_edge
2. Emit SCHEDULE_CAPABILITY_LEARNING exactly once
3. Set DomainId to "software-development"
4. Bundle all gap IDs into the single SCHEDULE_CAPABILITY_LEARNING event

**Complete rule:**
```
IF predicates = []
THEN:
  RouteTarget = knowledge_edge
  SCHEDULE_CAPABILITY_LEARNING emitted (once)
  DomainId = "software-development"  ← B32 invariant
  GapIds = [all detected gap IDs]
  SCHEDULE_CAPABILITY_LEARNING payload = {domainId, gapIds, requestText}
```

---

## B32-L3-008-002 — SCHEDULE_CAPABILITY_LEARNING: full specification

**When it fires:** predicates = []
**When it does NOT fire:** predicates is non-empty (even with gap warnings present)
**Emission count:** exactly once per request
**DomainId:** always "software-development" for B32
**Payload:** all detected gap IDs bundled

**Complete payload example:**
```json
{
  "action": "SCHEDULE_CAPABILITY_LEARNING",
  "domainId": "software-development",
  "sourceScenario": "B32",
  "requestText": "Run a spike to explore the best approach.",
  "gapIds": [
    "gap-vibe-coding-spike-001",
    "gap-vibe-coding-exploratory-001"
  ],
  "sessionId": "session-abc123",
  "timestamp": "2026-05-05T10:00:00Z"
}
```

---

## B32-L3-008-003 — Common B32 zero-predicate triggers

**Requests that always produce zero predicates in B32:**

| Request pattern | Gap IDs emitted |
|---|---|
| "Run a spike on X" | spike-001 |
| "A prototype of X" | prototype-001 |
| "Dashboard for X" | dashboard-001 |
| "Hack together a solution" | hack-001 |
| "Throwaway demo" | throwaway-001, demo-001 |
| "Vibe out X" | style-001 |
| "Exploratory session on X" | exploratory-001 |
| "Just a quick spike" | spike-001, style-001 |

All produce: predicates = [] → knowledge_edge + SCHEDULE_CAPABILITY_LEARNING (DomainId: software-development)

---

## B32-L3-008-004 — Teaching contrast: zero predicates vs gap warnings with predicates

| Scenario | Predicates | SCHEDULE_CAPABILITY_LEARNING? | Route |
|---|---|---|---|
| Only gap vocabulary | [] | Yes (once) | knowledge_edge |
| Action verb + gap vocabulary | non-empty | No | Flow(s) |
| Action verb, no gap vocabulary | non-empty | No | Flow(s) |
| Zero predicates, zero gaps (edge) | [] | Yes | knowledge_edge |

**Key insight:** SCHEDULE_CAPABILITY_LEARNING fires when the system cannot route (predicates empty). It does NOT fire when routing succeeds, even if gaps are present.

---
