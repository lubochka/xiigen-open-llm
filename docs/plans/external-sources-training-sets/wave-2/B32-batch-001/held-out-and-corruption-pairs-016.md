# Held-Out and Corruption Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-016-001 — HELD-OUT: "hack together + build" → IMPLEMENT fires; gap warning

**Request:** "Hack together a solution — build the cache module."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: ImplementationFlow
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-016-002 — CORRUPTION: "hack together" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Hack together a solution."
Predicates: ["IMPLEMENT"]
Reason: "hack" treated as a synonym for "build" or "create"
```

**Why this is wrong:**
- "hack together" is gap vocabulary → gap-vibe-coding-hack-001
- "hack" is NOT in IMPLEMENT lexicon
- When "hack together" is the only verb in the request, predicates = []
- IMPLEMENT fires only from: implement, build, create, write, code

**Correct output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: knowledge_edge
```

---

## B32-HO-016-003 — CORRUPTION: "cobble together" produces different gap ID from "hack together" (WRONG)

**Corrupted output:**
```
Request: "Cobble together a solution."
GapWarnings: [gap-vibe-coding-cobble-001]  ← WRONG gap ID
```

**Why this is wrong:**
- "cobble together" maps to gap-vibe-coding-hack-001 (same as "hack together")
- There is no gap-vibe-coding-cobble-001; this ID does not exist
- The three synonyms (hack together, cobble together, throw together) all map to gap-vibe-coding-hack-001

**Correct gap ID:** gap-vibe-coding-hack-001

---
