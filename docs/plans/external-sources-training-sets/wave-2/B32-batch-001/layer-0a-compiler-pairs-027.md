# Layer 0-A — Compiler Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-027-001 — "proof-of-concept" alone → zero predicates

**Input:** "Proof-of-concept for the recommendation engine."

```
Token scan:
"proof-of-concept" → gap-vibe-coding-prototype-001 (PoC (Proof-of-Concept) is artifact-type framing; no action verb)
"recommendation engine" → domain noun; no action verb

Action verb scan: none present
Gap vocabulary scan: "proof-of-concept" → gap-vibe-coding-prototype-001

Predicates: []
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-027-002 — "PoC" abbreviation → same as proof-of-concept

**Input:** "Need a PoC for the recommendation engine."

```
Token scan:
"PoC" → Proof-of-Concept abbreviation → gap-vibe-coding-prototype-001
"need" → desire verb; not in IMPLEMENT action lexicon
"recommendation engine" → domain noun

Predicates: []
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
```

---

## B32-0A-027-003 — "proof-of-concept" + build → IMPLEMENT fires

**Input:** "Build a proof-of-concept for the recommendation engine."

```
Token scan:
"build" → IMPLEMENT fires ✓
"proof-of-concept" → gap-vibe-coding-prototype-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---

## B32-0A-027-004 — "prototype" vs "proof-of-concept": same gap family

**Input A:** "Prototype recommendation engine."
**Input B:** "Proof-of-concept recommendation engine."

```
Input A: "prototype" alone → gap-vibe-coding-prototype-001 → Predicates: []
Input B: "proof-of-concept" alone → gap-vibe-coding-prototype-001 → Predicates: []

Both map to same gap ID. Both route to knowledge_edge.
Both emit SCHEDULE_CAPABILITY_LEARNING once.
```

---

## B32-0A-027-005 — "proof-of-concept" + ARCHITECTURE

**Input:** "Design a proof-of-concept architecture for the recommendation engine."

```
Token scan:
"design" → ARCHITECTURE fires ✓ (design fires ARCHITECTURE, not IMPLEMENT)
"proof-of-concept" → gap-vibe-coding-prototype-001
"architecture" → ARCHITECTURE reinforces

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: ArchitectureFlow31
CapabilityGapWarning: style_qualifier_noted
```

---
