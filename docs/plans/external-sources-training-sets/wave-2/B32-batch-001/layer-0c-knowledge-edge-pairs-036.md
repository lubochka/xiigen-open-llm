# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-036-001 — "barebones" alone: knowledge edge

**Knowledge edge: barebones with no action verb**

"Just barebones for now."

Analysis:
- "barebones" → gap-vibe-coding-style-001
- "for now" → temporal qualifier; no predicate
- No action verb present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-036-002 — "stripped-down" alone: knowledge edge

**Knowledge edge: stripped-down with no action verb**

"Stripped-down is the goal."

Analysis:
- "stripped-down" → gap-vibe-coding-style-001
- "goal" → noun; not an action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-036-003 — "barebones" + DOCUMENT: knowledge edge boundary

**Knowledge edge: barebones with DOCUMENT fires predicate**

"Barebones docs — document just the endpoints."

Analysis:
- "barebones" → gap-vibe-coding-style-001
- "document" → DOCUMENT fires ✓
- "endpoints" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Not knowledge_edge; DOCUMENT fires. Gap warning forwarded.

---

## B32-0C-036-004 — "stripped-down" + ARCHITECTURE: knowledge edge boundary

**Knowledge edge: stripped-down with ARCHITECTURE fires predicate**

"Stripped-down design — architect the data model."

Analysis:
- "stripped-down" → gap-vibe-coding-style-001
- "architect" → ARCHITECTURE fires ✓
- "data model" → scope

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

---

## B32-0C-036-005 — "barebones" + "stripped-down" multi-gap: still one SCL

**Knowledge edge: two gap terms, still one SCL emission**

"Barebones stripped-down version — no action yet."

Analysis:
- "barebones" → gap-vibe-coding-style-001
- "stripped-down" → gap-vibe-coding-style-001 (same ID; not double-counted)
- "version" → noun
- "no action" → negation

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL (once)

---
