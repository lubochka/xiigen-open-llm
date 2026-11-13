# Layer 0-A — Compiler Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-029-001 — "tracer bullet" alone → zero predicates

**Input:** "Tracer bullet for the end-to-end pipeline."

```
Token scan:
"tracer bullet" → gap-vibe-coding-spike-001 (tracer-bullet is investigation/minimal-path framing; same gap family as spike)
"pipeline" → domain noun; no action verb

Action verb scan: none present
Gap vocabulary scan: "tracer bullet" → gap-vibe-coding-spike-001

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-029-002 — "tracer-bullet" (hyphenated) → same gap

**Input:** "Tracer-bullet through the payment flow."

```
Token scan:
"tracer-bullet" → gap-vibe-coding-spike-001 (hyphenated form = same term)
"through" → preposition; not action verb

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: knowledge_edge
```

---

## B32-0A-029-003 — "tracer bullet" + implement → IMPLEMENT fires

**Input:** "Tracer bullet through the pipeline — implement each stage."

```
Token scan:
"tracer bullet" → gap-vibe-coding-spike-001
"implement" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---

## B32-0A-029-004 — "tracer bullet" + ARCHITECTURE

**Input:** "Tracer-bullet approach — design the system architecture first."

```
Token scan:
"tracer-bullet" → gap-vibe-coding-spike-001
"design" → ARCHITECTURE fires ✓
"architecture" → ARCHITECTURE reinforces

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ArchitectureFlow31
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-029-005 — "tracer bullet" + "spike": co-occurrence

**Input:** "Run a tracer bullet spike through the service."

```
Token scan:
"tracer bullet" → gap-vibe-coding-spike-001
"spike" → gap-vibe-coding-spike-001

Both map to same gap ID → one warning emitted
No action verb present

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
SCHEDULE_CAPABILITY_LEARNING: emitted once
```

---
