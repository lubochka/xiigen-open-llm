# Held-Out and Corruption Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-011-001 — Three v6 corruptions: v6-01, v6-02, v6-03 side by side

**Request used for all three:** "Vibe code a dashboard prototype — just throw it together."

**Correct output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001,
              gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "software-development"
```

**Reasoning for correct output:**
- "vibe code" → gap + IMPLEMENT (code root)
- "dashboard" → gap (not in UI lexicon)
- "prototype" → gap (alone with action verb = gap warning)
- "throw together" → gap (hack-001)
- "build" action is "code" root from "vibe code"

---

## B32-HO-011-002 — v6-01 Corrupted output: VIBE_CODING invented

```
CORRUPTED v6-01:
Predicates: ["VIBE_CODING"]
RouteTarget: VibeCodingFlow
GapWarnings: none
DomainId: "software-development"
```

**Error:** VIBE_CODING is not a registered predicate. VibeCodingFlow does not exist. The correct predicate is IMPLEMENT (from "code" root).

---

## B32-HO-011-003 — v6-02 Corrupted output: IMPLEMENT suppressed

```
CORRUPTED v6-02:
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-hack-001]
RouteTarget: knowledge_edge
DomainId: "software-development"
Reason: "vibe code" + "throw together" signals informal intent; IMPLEMENT suppressed
```

**Error:** "code" root fires IMPLEMENT unconditionally. "Vibe" and "throw together" are gap warnings, not suppressors. IMPLEMENT must be in the output.

---

## B32-HO-011-004 — v6-03 Corrupted output: DomainId wrong

```
CORRUPTED v6-03:
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001,
              gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "project-management"  ← WRONG
```

**Error:** DomainId must be "software-development" for B32. "project-management" is B36/B37.

---

## B32-HO-011-005 — Corruption comparison table

| Property | Correct | v6-01 | v6-02 | v6-03 |
|---|---|---|---|---|
| Predicates | [IMPLEMENT] | [VIBE_CODING] | [] | [IMPLEMENT] |
| RouteTarget | BackendServiceFlow41 | VibeCodingFlow | knowledge_edge | BackendServiceFlow41 |
| DomainId | software-development | software-development | software-development | project-management |
| GapWarnings | 4 warnings | none | 2 warnings | 4 warnings |
| Primary error | — | invented predicate | suppression | wrong DomainId |

---
