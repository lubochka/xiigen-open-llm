# Held-Out and Corruption Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-018-001 — HELD-OUT: "throwaway + interface build" → IMPLEMENT + UI; throwaway is warning

**Request:** "Build a throwaway interface for the login flow."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-throwaway-001]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-018-002 — CORRUPTION: "throwaway" suppresses UI (WRONG)

**Corrupted output:**
```
Request: "Build a throwaway interface for the login flow."
Predicates: ["IMPLEMENT"]  ← UI missing
Reason: "throwaway" = disposable = no formal UI needed
```

**Why this is wrong:**
- "interface" is in UI lexicon → UI must fire
- "throwaway" is gap vocabulary → warning only; never suppresses UI
- v6-02 corruption

**Correct predicates:** ["IMPLEMENT", "UI"]

---

## B32-HO-018-003 — HELD-OUT: "rewrite later" does NOT fire IMPLEMENT for current request

**Request:** "Throwaway code — just throwaway, we'll rewrite later."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-throwaway-001]
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

**Reasoning:** "rewrite later" is future-tense intent; the current request has no imperative action verb.

---
