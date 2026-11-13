# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-021-001 — "vibe code" splits into two independent signals

**Knowledge edge:** "vibe code" as a phrase produces two independent compiler signals:

1. "vibe" → gap-vibe-coding-style-001 (gap warning)
2. "code" → IMPLEMENT fires via root match (predicate)

These are independent. Neither signal cancels the other.

**Contrast table:**

| Phrase | Gap? | Predicate fires? | Predicate |
|---|---|---|---|
| vibe | Yes | No | — |
| code | No | Yes | IMPLEMENT |
| vibe code | Yes + Yes | Yes (on "code") | IMPLEMENT + gap warning |
| vibe out | Yes | No | — |
| vibe session | Yes | No | — |

---

## B32-0C-021-002 — "vibe coding a screen" full disambiguation

**Request:** "Vibe coding a screen for the user profile."

Token scan:
- "vibe" → gap-vibe-coding-style-001
- "coding" → root matches "code" → IMPLEMENT fires
- "screen" → UI fires

**Result:** `["IMPLEMENT", "UI"]` + gap-vibe-coding-style-001

**The three signals are independent:**
1. "vibe" produces a gap warning (but no predicate)
2. "coding" produces IMPLEMENT (via code root)
3. "screen" produces UI

Gap warning does not suppress either predicate. The two predicates do not affect the gap warning.

---

## B32-0C-021-003 — "vibe code" vs "vibe out" at the screen boundary

**Critical distinction for UI routing:**

| Request | IMPLEMENT? | UI? | Gap? |
|---|---|---|---|
| "Vibe code a screen" | Yes (code→IMPLEMENT) | Yes (screen→UI) | Yes (vibe→style-001) |
| "Vibe out a screen" | No (no code root) | Yes (screen→UI) | Yes (vibe→style-001) |
| "Vibe code a dashboard" | Yes (code→IMPLEMENT) | No (dashboard≠UI) | Yes (vibe→style-001 + dashboard-001) |
| "Vibe out a dashboard" | No | No | Yes (vibe→style-001 + dashboard-001) |

The presence of "code" in "vibe code" is the single determinant of whether IMPLEMENT fires.

---
