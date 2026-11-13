# Held-Out and Corruption Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-006-001 — HELD-OUT: "rough prototype — nothing fancy, build it" → IMPLEMENT fires on "build"

**Request:** "Rough prototype — nothing fancy, build it for the cart feature."

**Analysis:**
- "rough prototype" → gap-vibe-coding-rough-cut-001 + gap-vibe-coding-prototype-001
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001
- "build" → IMPLEMENT fires ✓
- "cart feature" → domain noun

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-rough-cut-001, gap-vibe-coding-prototype-001, gap-vibe-coding-nothing-fancy-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-HO-006-002 — CORRUPTION: "prototype" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Predicates: ["IMPLEMENT"]
Reason: "prototype" signals an implementation task → IMPLEMENT fires on "prototype"
```

**Why this is partially wrong:**

The result (IMPLEMENT fires) happens to be correct because "build" is present. However, the REASONING is wrong:
- "prototype" does NOT fire IMPLEMENT
- "build" fires IMPLEMENT
- The model must correctly attribute the predicate to "build", not to "prototype"

**If "build" were absent:**
- "Rough prototype — nothing fancy, for the cart feature." → predicates = []
- This case demonstrates why correct attribution matters

---

## B32-HO-006-003 — Attribution test: which token fired the predicate?

**Purpose:** The system must correctly identify WHICH token fired each predicate.

**Request:** "Rough prototype — nothing fancy, build it."

**Correct attribution:**
```
"build" → IMPLEMENT
"prototype" → gap-vibe-coding-prototype-001 (gap warning only)
"rough" → gap-vibe-coding-rough-cut-001 (gap warning only)
"nothing fancy" → gap-vibe-coding-nothing-fancy-001 (gap warning only)
```

**Wrong attribution (corruption):**
```
"prototype" → IMPLEMENT  ← WRONG
"build" → not mentioned  ← WRONG
```

---
