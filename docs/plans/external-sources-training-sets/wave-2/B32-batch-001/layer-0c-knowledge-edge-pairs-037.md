# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-037-001 — "lean" alone: knowledge edge

**Knowledge edge: lean qualifier with no action verb**

"Keep the approach lean."

Analysis:
- "lean" → gap-vibe-coding-style-001 (quality-bar adjective)
- "approach" → noun; not an action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-037-002 — "agile" alone: knowledge edge

**Knowledge edge: agile qualifier with no action verb**

"Be agile with it."

Analysis:
- "agile" → gap-vibe-coding-style-001 (informal quality-bar usage)
- "be" → existential verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-037-003 — "lean" + QA: knowledge edge boundary

**Knowledge edge: lean with QA fires predicate**

"Lean QA pass — QA the release."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "QA" (second use, imperative) → QA fires ✓

Predicates: ["QA"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Not knowledge_edge. QA fires on the imperative use.

---

## B32-0C-037-004 — "agile" + API_CONTRACT: knowledge edge boundary

**Knowledge edge: agile with API_CONTRACT fires predicate**

"Agile sprint — define the API contract."

Analysis:
- "agile" → gap-vibe-coding-style-001
- "define the API contract" → API_CONTRACT fires ✓

Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

---

## B32-0C-037-005 — "lean and agile": combined, still no predicates

**Knowledge edge: two qualifiers both silent**

"We should be lean and agile in this."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "agile" → gap-vibe-coding-style-001 (same ID)
- "should be" → modal + existential; no predicate

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL (once)

---
