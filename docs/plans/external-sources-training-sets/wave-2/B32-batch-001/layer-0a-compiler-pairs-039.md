# Layer 0-A — Compiler Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-039-001 — "lean" qualifier alone: zero predicates

**Request:** "Keep the solution lean."

```
Token scan:
"lean" → gap-vibe-coding-style-001 (minimal-overhead framing; not the Lean methodology proper noun)
"solution" → noun; not an action verb
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

Note: "lean" as a quality-bar adjective triggers the gap. "Lean" as a proper methodology noun (with capital L) does not.

---

## B32-0A-039-002 — "lean" + IMPLEMENT

**Request:** "Lean implementation — build the notification service."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"build" → IMPLEMENT fires ✓
"notification service" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-039-003 — "agile qualifier" alone: zero predicates

**Request:** "Stay agile about it."

```
Token scan:
"agile" → gap-vibe-coding-style-001 (informal agile-qualifier usage; not Agile methodology proper noun)
"stay" → linking verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

Note: lowercase "agile" in quality-bar framing triggers the gap. "Agile sprint" or "Agile process" as methodology references are out-of-scope for B32.

---

## B32-0A-039-004 — "agile" + ARCHITECTURE

**Request:** "Agile design — architect the pipeline."

```
Token scan:
"agile" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"pipeline" → scope

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-039-005 — "lean" + "agile": two qualifiers, same gap ID

**Request:** "Keep it lean and agile."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"agile" → gap-vibe-coding-style-001 (same ID)
No action verbs

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (once)
DomainId: "software-development"
```

---
