# Held-Out and Corruption Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-009-001 — HELD-OUT: pure vibe vocabulary → knowledge_edge, DomainId: software-development

**Request:** "Just vibe and spike the whole thing — throwaway prototype, nothing fancy."

**Analysis:**
- "vibe" → gap-vibe-coding-style-001
- "spike" → gap-vibe-coding-spike-001
- "throwaway" → gap-vibe-coding-throwaway-001
- "prototype" → gap-vibe-coding-prototype-001
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001
- No action verbs: implement/build/create/write/code all absent
- No UI vocabulary: screen/page/interface/etc. all absent

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-throwaway-001,
              gap-vibe-coding-prototype-001, gap-vibe-coding-nothing-fancy-001]
RouteTarget: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
TransitionType: unknown
```

---

## B32-HO-009-002 — CORRUPTION: DomainId "project-management" (WRONG)

**Corrupted output:**
```
Predicates: []
RouteTarget: knowledge_edge
DomainId: "project-management"  ← WRONG
```

**Why this is wrong:**
- B32 vibe coding → DomainId must be "software-development"
- "project-management" belongs to B36/B37
- The vibe vocabulary (spike, prototype, throwaway) is software development vocabulary, not project management vocabulary
- v6-03 corruption pattern

**Correct output:**
```
DomainId: "software-development"  ← always for B32
```

---

## B32-HO-009-003 — Multiple gap warnings: SCHEDULE_CAPABILITY_LEARNING emitted once

**For the request in 009-001:**

5 gap IDs are detected. SCHEDULE_CAPABILITY_LEARNING is emitted ONCE, bundling all 5 gap IDs:

```json
{
  "action": "SCHEDULE_CAPABILITY_LEARNING",
  "domainId": "software-development",
  "gapIds": [
    "gap-vibe-coding-style-001",
    "gap-vibe-coding-spike-001",
    "gap-vibe-coding-throwaway-001",
    "gap-vibe-coding-prototype-001",
    "gap-vibe-coding-nothing-fancy-001"
  ]
}
```

Not 5 separate SCHEDULE_CAPABILITY_LEARNING events.

---
