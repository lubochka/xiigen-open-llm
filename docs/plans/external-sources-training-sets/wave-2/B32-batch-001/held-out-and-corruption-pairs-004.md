# Held-Out and Corruption Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-004-001 — HELD-OUT: "exploratory screen design" → UI + ARCHITECTURE

**Request:** "Exploratory screen design for the user profile."

**Analysis:**
- "exploratory" → gap-vibe-coding-exploratory-001
- "screen" → UI fires ✓
- "design" → ARCHITECTURE fires ✓
- "user profile" → domain noun

**Expected output:**
```
Predicates: ["UI", "ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-exploratory-001]
RouteTarget: [UIFlow, ArchitectureFlow31]
TransitionType: documentation (ARCHITECTURE present, no IMPLEMENT)
DomainId: "software-development"
```

**Note:** Both UI and ARCHITECTURE fire. "Exploratory" is gap. The design is for a screen (UI) and involves architectural decisions (design = ARCHITECTURE).

---

## B32-HO-004-002 — CORRUPTION: UI/ARCHITECTURE suppressed in exploratory context (WRONG)

**Corrupted output:**
```
Predicates: []
Reason: "exploratory" signals that this is a preliminary inquiry; no concrete action is requested
```

**Why this is wrong:**
- "screen" fires UI unconditionally
- "design" fires ARCHITECTURE unconditionally
- "exploratory" is gap vocabulary; it cannot suppress UI or ARCHITECTURE
- v6-02 corruption pattern applied to UI and ARCHITECTURE (not just IMPLEMENT)

**Rule:** The suppression rule applies to ALL predicates, not just IMPLEMENT. "Exploratory" cannot suppress UI. "Rough-cut" cannot suppress ARCHITECTURE. Style qualifiers never suppress any predicate.

---

## B32-HO-004-003 — CORRUPTION variant: only UI fires (ARCHITECTURE missed)

**Corrupted output:**
```
Predicates: ["UI"]
Reason: "screen" fires UI; "design" is modified by "exploratory" so ARCHITECTURE does not fire
```

**Why this is wrong:**
- "design" fires ARCHITECTURE regardless of "exploratory" prefix
- ARCHITECTURE must be present in the output
- "Exploratory design" is DESIGN with exploratory framing; the ARCHITECTURE trigger ("design") is still present

**Correct output:** ["UI", "ARCHITECTURE"] + gap warning for "exploratory"

---
