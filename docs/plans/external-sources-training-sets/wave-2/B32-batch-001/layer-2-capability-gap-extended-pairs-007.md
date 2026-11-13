# Layer 2 — Capability Gap Extended Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-007-001 — "exploratory coding": IMPLEMENT fires (coding→code); exploratory is gap

**Request:** "Exploratory coding session — hack together the notification service module."

**Layer 0-A trace:**
```
Token: "exploratory" → gap-vibe-coding-exploratory-001
Token: "coding" → root match on "code" → IMPLEMENT fires ✓
Token: "session" → not an action predicate
Token: "hack together" → gap-vibe-coding-hack-001
Token: "notification service module" → domain nouns

Root match rule: "coding" → "code" root → IMPLEMENT

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-exploratory-001, gap-vibe-coding-hack-001]
```

**Layer 0-B trace:**
```
Predicates: ["IMPLEMENT"] → BackendServiceFlow41
DomainId: "software-development"
GapMetadata passed through
```

**Layer 1 trace:**
```
TransitionType: feature_addition
GapMetadata: [exploratory-001, hack-001]
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present
GapWarningTypes: [exploratory_framing, informal_assembly]
KeyInsight: "coding" fires IMPLEMENT via root match despite "exploratory" framing
```

**Final result:** [IMPLEMENT] → BackendServiceFlow41

---

## B32-L2-007-002 — Root match rule: "coding" → "code"

**The root match rule for gerunds:**

When a gerund (verb ending in -ing) appears in a request, the system checks if its root form is in the predicate lexicon:
- "coding" → root "code" → IMPLEMENT ✓
- "building" → root "build" → IMPLEMENT ✓ (if applicable)
- "implementing" → root "implement" → IMPLEMENT ✓

**What does NOT match via root:**
- "exploring" → root "explore" → NOT in IMPLEMENT lexicon → no predicate
- "hacking" → root "hack" → NOT in predicate lexicon → gap
- "vibing" → root "vibe" → NOT in predicate lexicon → gap

---

## B32-L2-007-003 — "exploratory coding" vs "exploratory session"

| Request | "coding" present? | Predicates | Route |
|---|---|---|---|
| "Exploratory coding session" | yes | [IMPLEMENT] | BackendServiceFlow41 |
| "Exploratory session" | no | [] | knowledge_edge |
| "Exploratory coding and design" | yes (coding) + design | [IMPLEMENT, ARCHITECTURE] | BackendServiceFlow41 + ArchitectureFlow31 |

The presence of "coding" is the determining factor for whether IMPLEMENT fires.

---
