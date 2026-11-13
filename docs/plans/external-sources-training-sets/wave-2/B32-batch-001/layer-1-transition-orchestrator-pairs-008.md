# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-008-001 — One SCHEDULE_CAPABILITY_LEARNING per request

**Rule:** SCHEDULE_CAPABILITY_LEARNING is emitted exactly once per request, never multiple times.

**Example with multiple gap IDs:**

**Request:** "Spike, vibe code, hack together, nothing fancy, throwaway prototype."

**Gap IDs detected:** 6 separate gap IDs

**SCHEDULE_CAPABILITY_LEARNING emissions:** 1 (not 6)

**Output:**
```
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING  ← emitted once
GapIds: [
  "gap-vibe-coding-spike-001",
  "gap-vibe-coding-style-001",
  "gap-vibe-coding-hack-001",
  "gap-vibe-coding-nothing-fancy-001",
  "gap-vibe-coding-throwaway-001",
  "gap-vibe-coding-prototype-001"
]
DomainId: "software-development"
```

All gap IDs are bundled into a single SCHEDULE_CAPABILITY_LEARNING event.

---

## B32-L1-008-002 — SCHEDULE_CAPABILITY_LEARNING: when it fires and when it doesn't

| Condition | SCHEDULE_CAPABILITY_LEARNING? |
|---|---|
| predicates = [] | Yes — once |
| predicates non-empty, gap warnings present | No — predicates route normally; gap warnings are metadata |
| predicates non-empty, no gap warnings | No |
| predicates = [], zero gap warnings | Yes — once (even without specific gap IDs) |

**Critical:** SCHEDULE_CAPABILITY_LEARNING only fires when predicates = []. When predicates are non-empty (even with gap warnings), the request routes to flows normally. SCHEDULE_CAPABILITY_LEARNING is NOT emitted alongside successful routing.

---

## B32-L1-008-003 — SCHEDULE_CAPABILITY_LEARNING payload

**Complete payload structure:**

```
{
  action: "SCHEDULE_CAPABILITY_LEARNING",
  domainId: "software-development",
  sourceScenario: "B32",
  requestText: "[original request]",
  gapIds: [...],
  sessionId: "[session identifier]",
  timestamp: "[ISO 8601 timestamp]"
}
```

This payload is dispatched to the learning pipeline. The pipeline uses it to:
1. Log the unrecognized vocabulary
2. Schedule training data generation for the identified gap IDs
3. Track frequency of gap encounters for prioritization

---

## B32-L1-008-004 — Per-request vs per-session SCHEDULE_CAPABILITY_LEARNING

**Per-request:** One emission per request with empty predicates.

**Per-session:** If the same session has multiple requests, each request with empty predicates generates its own SCHEDULE_CAPABILITY_LEARNING event.

**Deduplication:** Within a single request, SCHEDULE_CAPABILITY_LEARNING is emitted once regardless of how many gap IDs are detected.

---
