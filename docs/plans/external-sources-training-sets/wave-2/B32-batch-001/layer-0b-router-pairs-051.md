# Layer 0-B — Router Pairs (051)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-051-001 — "throw together a screen": UI routing

**Request:** "Throw together a screen for the user profile."

```
Input: Predicates = ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
UI → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-051-002 — "hack together a screen" + IMPLEMENT: two flows

**Request:** "Hack together a screen and implement the backend handler."

```
Input: Predicates = ["UI", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
UI → UIFlow
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-051-003 — "lean UI — build screen": IMPLEMENT + UI routing

**Request:** "Lean UI — build the settings screen."

```
Input: Predicates = ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
UI → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-051-004 — "lean UI" alone (no verb): knowledge_edge

**Request:** "Lean UI."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---
