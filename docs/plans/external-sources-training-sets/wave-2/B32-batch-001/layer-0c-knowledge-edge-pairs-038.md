# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-038-001 — "iterative" alone: knowledge edge

**Knowledge edge: iterative qualifier with no action verb**

"Let's be iterative."

Analysis:
- "iterative" → gap-vibe-coding-style-001
- "be" → existential; no predicate

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-038-002 — gap + SECURITY: fires, not knowledge edge

**Knowledge edge: gap with security verb fires SECURITY**

"Quick-win security — secure the endpoints."

Analysis:
- "quick-win" → gap-vibe-coding-style-001
- "secure" → SECURITY fires ✓

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Not knowledge_edge. SECURITY fires from the verb.

---

## B32-0C-038-003 — "security" as noun only: no SECURITY predicate

**Knowledge edge: "security" as noun does not fire SECURITY**

"We need good security."

Analysis:
- "security" → noun object; not an action verb
- No "secure" imperative
- No action verbs present

Predicates: []
GapWarnings: []
Route: (no gap vocabulary either → normal empty output, no SCL)

---

## B32-0C-038-004 — "iterative" + IMPLEMENT + TEST: two predicates, one gap

**Knowledge edge boundary: multi-predicate with iterative**

"Iterative approach: implement the module and test the output."

Analysis:
- "iterative" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "test" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

---

## B32-0C-038-005 — dashboard + IMPLEMENT: dashboard not UI

**Knowledge edge: dashboard gap does not become UI**

"Iterative sprint — implement the dashboard."

Analysis:
- "iterative" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "dashboard" → gap-vibe-coding-dashboard-001 (NOT UI predicate)

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
CapabilityGapWarning: style_qualifier_noted

---
