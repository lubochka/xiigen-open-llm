# Layer 0-A — Compiler Pairs (051)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-051-001 — informal + UI: "throw together a screen"

**Request:** "Throw together a screen for the user profile."

```
Token scan:
"throw together" → gap-vibe-coding-hack-001
"screen" → UI fires ✓
"user profile" → scope

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-051-002 — informal + UI: "whip up an interface"

**Request:** "Whip up a quick interface for the settings."

```
Token scan:
"whip up" → gap-vibe-coding-hack-001
"interface" → UI fires ✓ (interface → UI predicate)
"settings" → scope

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-051-003 — informal + UI + IMPLEMENT: screen + build

**Request:** "Hack together a screen and implement the backend handler."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"screen" → UI fires ✓
"implement" → IMPLEMENT fires ✓
"backend handler" → scope

Predicates: ["UI", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-051-004 — style qualifier + UI: "lean UI"

**Request:** "Lean UI — build the settings screen."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"build" → IMPLEMENT fires ✓
"settings screen" → UI fires ✓ (screen → UI predicate; "build" is the verb trigger)

Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Note: "build [screen]" fires both IMPLEMENT (from "build") and UI (from "screen"). Both predicates are independent.

---

## B32-0A-051-005 — "UI" alone without action verb: no predicate

**Request:** "Lean UI."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"UI" → noun in context; no action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

"UI" as noun alone does not fire UI predicate without an action verb.

---
