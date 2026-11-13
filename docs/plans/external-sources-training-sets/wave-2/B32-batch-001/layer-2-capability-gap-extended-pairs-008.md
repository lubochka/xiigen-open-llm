# Layer 2 — Capability Gap Extended Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-008-001 — "vibe code a screen": UI fires; vibe code is gap warning

**Request:** "Vibe code a quick screen for the product listing — make it feel intuitive."

**Layer 0-A trace:**
```
Token: "vibe code" → gap-vibe-coding-style-001 (vibe framing)
Token: "code" (root from "vibe code") → IMPLEMENT fires ✓
Token: "screen" → UI lexicon match → UI fires ✓
Token: "feel intuitive" → gap-vibe-coding-intuition-001
Token: "product listing" → domain noun

Predicates: ["UI", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-intuition-001]
```

**Layer 0-B trace:**
```
Predicates: ["UI", "IMPLEMENT"] → UIFlow + BackendServiceFlow41
DomainId: "software-development"
ParallelEligible: true
```

**Layer 1 trace:**
```
TransitionType: feature_addition
FlowOrder: [UIFlow || BackendServiceFlow41]  ← parallel
GapMetadata: [style-001, intuition-001]
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present
GapWarningTypes: [vibe_style_framing, intuition_driven_framing]
KeyInsight: "vibe code" → "code" root fires IMPLEMENT; "screen" fires UI; vibe framing = gap warning only
```

**Final result:** [UI, IMPLEMENT] → UIFlow + BackendServiceFlow41 (parallel) + gap warnings

---

## B32-L2-008-002 — Dual gap warning in vibe+intuition request

**Two distinct gap IDs are emitted in this request:**

1. gap-vibe-coding-style-001: triggered by "vibe code"
   - Category: vibe_style_framing
   - Signal: developer is using informal working style

2. gap-vibe-coding-intuition-001: triggered by "feel intuitive"
   - Category: intuition_driven_design
   - Signal: developer prioritizes subjective "feel" over specification

Both are emitted in the same SCHEDULE_CAPABILITY_LEARNING context? No — because predicates fired. Gap warnings are metadata only. No SCHEDULE_CAPABILITY_LEARNING is emitted when predicates are non-empty.

---

## B32-L2-008-003 — "vibe code" token parsing: both effects simultaneously

**Parse of "vibe code a quick screen":**

```
"vibe" → gap token → gap-vibe-coding-style-001
"code" → action predicate token → IMPLEMENT fires
"screen" → UI token → UI fires
```

Three effects from four words:
1. Gap warning emitted (vibe)
2. IMPLEMENT fires (code root)
3. UI fires (screen)

This is the correct and expected behavior. "Vibe" and "code" are parsed independently. The compound "vibe code" is recognized as a compound expression where "vibe" is the modifier and "code" retains its predicate semantics.

---
