# Layer 0-A — Compiler Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-026-001 — "minimum viable" qualifier alone → zero predicates

**Input:** "Minimum viable payment integration."

```
Token scan:
"minimum viable" → gap-vibe-coding-style-001 (quality-bar framing; MV- prefix family)
"payment integration" → domain noun compound; no action verb

Action verb scan: none present
Gap vocabulary scan: "minimum viable" → gap-vibe-coding-style-001

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-026-002 — "minimum viable" + action verb → IMPLEMENT fires

**Input:** "Build the minimum viable payment integration."

```
Token scan:
"build" → IMPLEMENT fires ✓
"minimum viable" → gap-vibe-coding-style-001

Action verb scan:
- build: PRESENT → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-026-003 — "MVP" as abbreviation → same gap family

**Input:** "Just an MVP build, implement the core checkout."

```
Token scan:
"MVP" → minimum viable product abbreviation → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"checkout" → domain context

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
Note: "MVP" in software context maps to same quality-bar family as "minimum viable"
```

---

## B32-0A-026-004 — "minimum viable" + TEST → both predicates independent

**Input:** "Minimum viable test coverage for the auth service — write the tests."

```
Token scan:
"minimum viable" → gap-vibe-coding-style-001
"write" → IMPLEMENT fires ✓ (write fires IMPLEMENT as action verb)
"tests" → TEST fires ✓ (positive mention as noun object of "write")

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, TestFlow]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-026-005 — "minimal" vs "minimum viable": contrast

**Input A:** "Minimum viable service."
**Input B:** "Minimal service."

```
Input A: "minimum viable" → gap-vibe-coding-style-001 → Predicates: []
Input B: "minimal" → general adjective; no registered gap ID → Predicates: []

Both route to knowledge_edge (no action verb in either).
Only Input A emits gap-vibe-coding-style-001.
```

---
