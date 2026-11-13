# Held-Out and Corruption Pairs (038)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-038-001 — HELD-OUT: "cobble together the auth module and write tests" → IMPLEMENT + TEST

**Request:** "Cobble together the auth module and write some tests."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-038-002 — HELD-OUT: "hack something together" alone → SCL, hack gap ID

**Request:** "Let's hack something together."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
TransitionType: unknown
```

---

## B32-HO-038-003 — CORRUPTION: "hack together" assigns style-001 (WRONG)

**Corrupted output:**
```
Request: "Hack together the auth module."
GapWarnings: [gap-vibe-coding-style-001]
```

**Why this is wrong:**
- "hack together" → gap-vibe-coding-hack-001 (action phrase, not quality-bar adjective)
- style-001 is for quality-bar adjectives only

**Correct GapWarnings:** [gap-vibe-coding-hack-001]

---

## B32-HO-038-004 — CORRUPTION: ContributionFlag = true for knowledge_edge (WRONG)

**Corrupted output:**
```
Request: "Let's hack something together."
ContributionFlag: true
Reason: "hack" vocabulary signals active work intent
```

**Why this is wrong:**
- ContributionFlag = true only when predicates are non-empty
- "hack something together" → predicates = [] → knowledge_edge → ContributionFlag = false

**Correct ContributionFlag:** false

---
