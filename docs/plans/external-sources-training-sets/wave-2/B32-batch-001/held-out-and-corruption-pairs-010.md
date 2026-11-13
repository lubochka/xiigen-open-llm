# Held-Out and Corruption Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-010-001 — HELD-OUT: "just vibe code it" → zero predicates or IMPLEMENT?

**Request:** "Just vibe code it."

**Analysis:**
- "vibe code" → gap-vibe-coding-style-001 + "code" root → IMPLEMENT fires ✓
- "just" → general qualifier (not a predicate)
- "it" → pronoun; no predicate

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "software-development"
```

**IMPLEMENT fires because "code" root is present in "vibe code".**

---

## B32-HO-010-002 — CORRUPTION: "code" fires IMPLEMENT → this is CORRECT (not a corruption)

**Statement to evaluate:** "In the request 'just vibe code it', should 'code' fire IMPLEMENT?"

**Answer:** YES. This is correct behavior, not a corruption.

The "code" root match fires IMPLEMENT. The word "just" does not suppress predicates. The word "vibe" is gap but does not suppress "code".

**The corruption would be NOT firing IMPLEMENT when "code" is present.**

---

## B32-HO-010-003 — Contrast: "just vibe it" vs "just vibe code it"

**"just vibe it":**
- "vibe" → gap
- "just" → qualifier
- "it" → pronoun
- No "code" root present → IMPLEMENT does not fire
- Result: [] → knowledge_edge

**"just vibe code it":**
- "vibe" → gap
- "code" → IMPLEMENT fires ✓
- Result: ["IMPLEMENT"]

The word "code" is the critical difference. With "code": IMPLEMENT fires. Without "code": predicates = [].

---

## B32-HO-010-004 — Teaching: "just" as a vibe qualifier that cannot suppress predicates

**"just" vocabulary in B32:**
- "just make it work" → gap-vibe-coding-just-works-001 (PHRASE)
- "just" alone → not a registered gap ID; general informality marker
- "just" alone cannot suppress predicates

"Just build it" → [IMPLEMENT] (not knowledge_edge)
"Just implement it" → [IMPLEMENT]
"Just code it" → [IMPLEMENT]
"Just vibe code it" → [IMPLEMENT] + gap warning for vibe

---
