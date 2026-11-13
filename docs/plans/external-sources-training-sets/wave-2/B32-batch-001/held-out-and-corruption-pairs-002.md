# Held-Out and Corruption Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-002-001 — HELD-OUT: "spike the API design" → ARCHITECTURE fires; "spike" is gap

**Request:** "Spike the API design for the notification system."

**Expected (correct) output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ArchitectureFlow31
TransitionType: documentation
DomainId: "software-development"
```

**Reasoning:**
- "spike" → gap-vibe-coding-spike-001 (gap, not predicate)
- "design" → ARCHITECTURE fires ✓
- "API" → API_CONTRACT fires ✓ ("API" is in API_CONTRACT lexicon)

**Corrected expected output:**
```
Predicates: ["ARCHITECTURE", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: [ArchitectureFlow31, APIContractFlow]
TransitionType: documentation
DomainId: "software-development"
```

---

## B32-HO-002-002 — CORRUPTION: spike fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Predicates: ["IMPLEMENT"]
Reason: "spike" treated as an action verb → IMPLEMENT fires
```

**Why this is wrong:**
- "spike" is gap vocabulary; it is NOT in the IMPLEMENT lexicon
- "spike" never fires any predicate
- The predicates that fire are from "design" (ARCHITECTURE) and "API" (API_CONTRACT)
- IMPLEMENT fires on: implement, build, create, write, code — NOT on "spike"

**Detection:** Any output mapping "spike" to IMPLEMENT is a corruption.

---

## B32-HO-002-003 — CORRUPTION variant: "spike" fires API_CONTRACT (WRONG)

**Corrupted output:**
```
Predicates: ["API_CONTRACT"]
Reason: "spike the API" → "API" fires API_CONTRACT; "spike" maps to "design" by semantic similarity
```

**Partial correct, partial wrong:**
- API_CONTRACT firing on "API" is correct
- ARCHITECTURE firing on "design" is also correct and must be present
- "spike" contributing to any predicate is wrong

**Correct predicates:** ["ARCHITECTURE", "API_CONTRACT"] — not just ["API_CONTRACT"]

---
