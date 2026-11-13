# Layer 0-A — Compiler Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-031-001 — "thin slice" alone → zero predicates

**Input:** "Thin slice of the checkout flow."

```
Token scan:
"thin slice" → gap-vibe-coding-spike-001 (thin-slice = minimal vertical path; spike/investigation family)
"checkout flow" → domain noun; no action verb

Action verb scan: none present

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-031-002 — "thin-slice" (hyphenated) + implement

**Input:** "Implement a thin-slice end-to-end feature."

```
Token scan:
"implement" → IMPLEMENT fires ✓
"thin-slice" → gap-vibe-coding-spike-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-031-003 — "thin slice" + "walking skeleton": co-occurrence

**Input:** "Thin slice walking skeleton for the new API."

```
Token scan:
"thin slice" → gap-vibe-coding-spike-001
"walking skeleton" → gap-vibe-coding-spike-001

Both → same gap ID → one warning
No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
SCHEDULE_CAPABILITY_LEARNING: emitted once
```

---

## B32-0A-031-004 — "thin slice" + DOCUMENT

**Input:** "Write a thin-slice spec — document just the happy path."

```
Token scan:
"write" → IMPLEMENT fires ✓
"thin-slice" → gap-vibe-coding-spike-001
"document" → DOCUMENT fires ✓
"spec" → domain noun (specification)

Predicates: ["IMPLEMENT", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-031-005 — "vertical slice" → related framing, same gap family

**Input:** "Vertical slice through the user registration feature."

```
Token scan:
"vertical slice" → gap-vibe-coding-spike-001 (vertical slice = thin-slice family; minimal end-to-end path)
"feature" → noun; no action verb

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
```

---
