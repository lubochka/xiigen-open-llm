# Held-Out and Corruption Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-007-001 — HELD-OUT: "hack together a component" → UI fires on "component"

**Request:** "Hack together a component for the notification bar."

**Analysis:**
- "hack together" → gap-vibe-coding-hack-001 (gap, not predicate)
- "component" → UI fires ✓ (component is in UI lexicon)
- "notification bar" → domain noun

**Expected output:**
```
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: UIFlow
TransitionType: feature_addition
DomainId: "software-development"
```

**Key insight:** "hack together" cannot suppress UI. "component" fires UI unconditionally.

---

## B32-HO-007-002 — CORRUPTION: "hack together" suppresses UI (WRONG)

**Corrupted output:**
```
Predicates: []
Reason: "hack together" signals informal assembly; formal UI routing suppressed
```

**Why this is wrong:**
- "component" is in the UI lexicon; UI fires unconditionally
- "hack together" is gap vocabulary; it cannot suppress any predicate
- This is v6-02 corruption applied to the UI predicate

**Correct output:**
```
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: UIFlow
```

---

## B32-HO-007-003 — Proof: all UI lexicon tokens fire UI even with "hack together"

**Verification that "hack together" cannot suppress any UI token:**

| Request | Predicates | Gap |
|---|---|---|
| "Hack together a screen" | [UI] | hack-001 |
| "Hack together a page" | [UI] | hack-001 |
| "Hack together an interface" | [UI] | hack-001 |
| "Hack together a component" | [UI] | hack-001 |
| "Hack together a form" | [UI] | hack-001 |
| "Hack together a button" | [UI] | hack-001 |
| "Hack together an input" | [UI] | hack-001 |
| "Hack together a modal" | [UI] | hack-001 |

All 10 UI lexicon tokens fire UI regardless of "hack together" prefix. This confirms the suppression rule universally.

---
