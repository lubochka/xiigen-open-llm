# Held-Out and Corruption Pairs (019)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-019-001 — HELD-OUT: "fast and loose + implementation" → IMPLEMENT fires; style is warning

**Request:** "Play it fast and loose with the implementation details for now."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: ImplementationFlow
TransitionType: feature_addition
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:** "implementation" contains root "implement" → IMPLEMENT fires.

---

## B32-HO-019-002 — CORRUPTION: "fast and loose" zero predicates despite "implementation" (WRONG)

**Corrupted output:**
```
Request: "Fast and loose with the implementation details."
Predicates: []
Reason: "fast and loose" framing = informal = no formal action
```

**Why this is wrong:**
- "implementation" contains root "implement" → IMPLEMENT must fire
- "fast and loose" is gap → warning only
- Root matching applies regardless of surrounding informal framing
- v6-02 corruption

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-019-003 — HELD-OUT: "fast and loose approach alone" → zero predicates confirmed

**Request:** "Take a fast and loose approach here."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

**Reasoning:** "approach" is a domain noun; "take" is not in IMPLEMENT lexicon. No action verb present.

---
