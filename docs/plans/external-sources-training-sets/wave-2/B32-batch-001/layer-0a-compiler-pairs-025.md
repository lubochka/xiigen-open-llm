# Layer 0-A — Compiler Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-025-001 — "good enough" qualifier alone → zero predicates

**Input:** "Good enough auth module."

```
Token scan:
"good enough" → gap-vibe-coding-style-001 (quality-bar framing)
"auth" → domain noun; no registered action verb
"module" → structural noun; no action verb

Action verb scan: none present
Gap vocabulary scan: "good enough" → gap-vibe-coding-style-001

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
```

**Output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-025-002 — "good enough" + action verb → predicate fires

**Input:** "Build a good-enough auth module."

```
Token scan:
"build" → IMPLEMENT fires ✓
"good-enough" → gap-vibe-coding-style-001 (style qualifier)
"auth module" → domain context

Action verb scan:
- build: PRESENT → IMPLEMENT fires ✓

Gap vocabulary scan:
- "good-enough" → gap-vibe-coding-style-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
```

**Output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---

## B32-0A-025-003 — "good enough for now" phrasing → still gap-vibe-coding-style-001

**Input:** "Good enough for now — just implement the login flow."

```
Token scan:
"good enough for now" → extended form of "good enough" → gap-vibe-coding-style-001
"just" → intensifier; no action verb root
"implement" → IMPLEMENT fires ✓
"login flow" → domain context

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0A-025-004 — "good enough" vs "make it good": contrast

**Input A:** "Good enough API."
**Input B:** "Make the API good."

```
Input A:
"good enough" → gap-vibe-coding-style-001; no action verb → Predicates: []

Input B:
"make" → IMPLEMENT fires (make = action verb in IMPLEMENT lexicon)
"good" → style adjective; no gap ID

Input A Predicates: []   → knowledge_edge
Input B Predicates: ["IMPLEMENT"]  → ImplementationFlow
```

---

## B32-0A-025-005 — "good enough" with DOCUMENT

**Input:** "Good enough docs — just document the endpoints."

```
Token scan:
"good enough" → gap-vibe-coding-style-001
"document" → DOCUMENT fires ✓
"endpoints" → scope noun

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: DocumentFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---
