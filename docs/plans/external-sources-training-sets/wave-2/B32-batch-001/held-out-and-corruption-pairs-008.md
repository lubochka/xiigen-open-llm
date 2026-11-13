# Held-Out and Corruption Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-008-001 — HELD-OUT: "quick and dirty unit tests" → TEST fires on "unit tests"

**Request:** "Write quick and dirty unit tests for the payment module."

**Analysis:**
- "write" → IMPLEMENT fires ✓
- "quick and dirty" → gap-vibe-coding-quick-dirty-001
- "unit tests" → TEST fires ✓
- "payment module" → domain noun

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-quick-dirty-001]
RouteTarget: [BackendServiceFlow41, TestFlow]
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-HO-008-002 — CORRUPTION: TEST suppressed by "quick and dirty" (WRONG)

**Corrupted output:**
```
Predicates: ["IMPLEMENT"]
Reason: "quick and dirty" signals informal work; TEST suppressed (no formal tests requested)
```

**Why this is wrong:**
- "unit tests" fires TEST unconditionally
- "quick and dirty" is gap vocabulary; it cannot suppress TEST
- v6-02 corruption applied to TEST predicate

**Correct output:** [IMPLEMENT, TEST] — both predicates fire; gap warning emitted for "quick and dirty"

---

## B32-HO-008-003 — "write unit tests" without "quick and dirty": comparison

**Clean request:** "Write unit tests for the payment module."
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: none
```

**Vibe request:** "Write quick and dirty unit tests for the payment module."
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-quick-dirty-001]
```

Same predicates. Only the gap warnings differ. The "quick and dirty" framing adds metadata but does not change the routing.

---

## B32-HO-008-004 — Teaching point: TEST fires on "unit test" / "tests" / "testing"

| Token | TEST fires? |
|---|---|
| "unit tests" | Yes |
| "test" | Yes |
| "testing" | Yes (via "test" root) |
| "write tests" | Yes (IMPLEMENT + TEST) |
| "quick and dirty unit tests" | Yes (TEST fires; "quick and dirty" is gap) |

---
