# Layer 0-A — Compiler Pairs (038)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-038-001 — "barebones" alone: zero predicates

**Request:** "Keep it barebones."

```
Token scan:
"barebones" → gap-vibe-coding-style-001 (minimal-feature framing)
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-038-002 — "barebones" + IMPLEMENT

**Request:** "Barebones implementation — build the upload handler."

```
Token scan:
"barebones" → gap-vibe-coding-style-001
"build" → IMPLEMENT fires ✓
"upload handler" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-038-003 — "stripped-down" alone: zero predicates

**Request:** "Stripped-down version is fine."

```
Token scan:
"stripped-down" → gap-vibe-coding-style-001 (reduced-feature framing)
"version" → noun; not an action verb
"fine" → approval; no predicate

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-038-004 — "stripped-down" + UI

**Request:** "Stripped-down UI — build the settings screen."

```
Token scan:
"stripped-down" → gap-vibe-coding-style-001
"UI" → present as context but not a standalone predicate trigger here
"build" → IMPLEMENT fires ✓
"settings screen" → scope note: "screen" context → UI fires ✓

Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-038-005 — "barebones" + "stripped-down" combined: one gap ID

**Request:** "Barebones stripped-down prototype."

```
Token scan:
"barebones" → gap-vibe-coding-style-001
"stripped-down" → gap-vibe-coding-style-001 (same ID)
"prototype" → gap-vibe-coding-prototype-001
No action verbs

Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (emitted once)
DomainId: "software-development"
```

Multiple gap IDs → SCL emitted exactly once.

---
