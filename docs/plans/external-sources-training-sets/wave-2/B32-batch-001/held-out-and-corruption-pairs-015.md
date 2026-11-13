# Held-Out and Corruption Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-015-001 — HELD-OUT: "quick win + implement" → IMPLEMENT fires; gap warning present

**Request:** "Implement a quick win for the notification service."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: ImplementationFlow
TransitionType: feature_addition
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-015-002 — CORRUPTION: "quick win" treated as zero predicates (WRONG)

**Corrupted output:**
```
Request: "Implement a quick win for the notification service."
Predicates: []
Reason: "quick win" framing = low-formality = no formal predicate
```

**Why this is wrong:**
- "implement" is in IMPLEMENT lexicon → IMPLEMENT must fire
- "quick win" is gap vocabulary → warning only; never suppresses IMPLEMENT
- v6-02 corruption

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-015-003 — HELD-OUT: "quick win alone" → zero predicates; confirmed gap

**Request:** "We just need a quick win for the sprint."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

**Reasoning:** No action verb. "Quick win" is gap. "Need" is not in IMPLEMENT lexicon.

---
