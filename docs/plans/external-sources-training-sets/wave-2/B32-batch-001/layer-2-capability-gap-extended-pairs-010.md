# Layer 2 — Capability Gap Extended Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-010-001 — "just make it work + implement": IMPLEMENT fires; "just make it work" is gap

**Request:** "Just make it work — implement a basic session handler."

**Layer 0-A trace:**
```
Token: "just make it work" → gap-vibe-coding-just-works-001
Token: "implement" → IMPLEMENT fires ✓ (direct lexicon match)
Token: "basic" → quality qualifier (not a predicate)
Token: "session handler" → domain noun

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-just-works-001]
```

**Layer 0-B trace:**
```
Predicates: ["IMPLEMENT"] → BackendServiceFlow41
DomainId: "software-development"
```

**Layer 1 trace:**
```
TransitionType: feature_addition
GapMetadata: [just-works-001]
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present
GapWarningType: minimum_bar_framing
KeyInsight: "just make it work" is a quality/ambition reducer; it does not affect routing
v6-02 anti-pattern: this request would be corrupted by suppressing IMPLEMENT due to "just make it work"
```

**Final result:** [IMPLEMENT] → BackendServiceFlow41

---

## B32-L2-010-002 — v6-02 corruption context: "just make it work" suppression

**This request is the canonical v6-02 corruption case.**

**WRONG (v6-02):**
```
Predicates: [] (IMPLEMENT suppressed because "just make it work" signals informal intent)
RouteTarget: knowledge_edge
```

**CORRECT:**
```
Predicates: ["IMPLEMENT"] (fires on "implement" regardless of surrounding framing)
RouteTarget: BackendServiceFlow41
```

The v6-02 corruption treats "just make it work" as an override signal. It is not. It is a gap warning. The action verb "implement" determines the predicate. The gap framing determines the gap warning.

---

## B32-L2-010-003 — Extended analysis: "just make it work" vocabulary

**User intent:** The user wants functional code without overengineering. They are signaling impatience with perfection and a bias toward shipping something that works.

**What the system should do:**
1. Route to BackendServiceFlow41 (IMPLEMENT fires on "implement")
2. Emit gap-vibe-coding-just-works-001 warning
3. Pass the "minimum viable quality" signal to the flow as metadata
4. The flow produces working code; the quality floor is noted in metadata

**What the system should NOT do:**
- Route to knowledge_edge (incorrect — "implement" fires IMPLEMENT)
- Suppress IMPLEMENT (v6-02 corruption)
- Invent a new predicate for "just make it work" (v6-01 variant)

---
