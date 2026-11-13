# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-032-001 — "rough draft" knowledge edge

**Term:** rough draft
**Category:** Category 8 — Low-fidelity cut framing
**Gap ID:** gap-vibe-coding-rough-cut-001

**Knowledge edge summary:**
- "rough draft" alone → zero predicates → knowledge_edge
- "document the rough draft" → DOCUMENT fires (document = action verb)
- "write a rough draft spec" → IMPLEMENT fires (write = action verb) + potentially DOCUMENT

**Boundary:** "rough" alone (without "cut" or "draft") → general style adjective; no registered gap ID

---

## B32-0C-032-002 — "wireframe" co-occurrence with UI triggers

**Wireframe + screen/page/interface/component:**

"Wireframe for the checkout page":
- "wireframe" → gap-vibe-coding-prototype-001
- "page" → UI fires ✓

"Wireframe only — no implementation":
- "wireframe" → gap-vibe-coding-prototype-001
- "only" → intensifier; no predicate
- "no implementation" → negated IMPLEMENT; does NOT fire

Predicates: [] for second case; ["UI"] for first case.

---

## B32-0C-032-003 — "MVP" boundary: software vs business context

**MVP in B32 always treated as software-development context:**

The B32 DomainId invariant ("software-development") means MVP is always interpreted as quality-bar framing for software development, not as a business or product strategy term.

MVP → gap-vibe-coding-style-001 (same as "minimum viable" and "good enough")

---

## B32-0C-032-004 — "just enough" knowledge edge

**Term:** just enough
**Category:** Category 1 — Quality-bar framing
**Gap ID:** gap-vibe-coding-style-001

"Just enough auth service" → gap-vibe-coding-style-001 → zero predicates
"Just enough — build the auth service" → IMPLEMENT fires + gap warning

Note: "just enough" ≠ "just make it work" (Category 12, gap-vibe-coding-just-works-001)

---

## B32-0C-032-005 — Complete Round 2 knowledge-edge summary

**All new terms introduced in Round 2 (files 025-036):**

| Term | Gap ID | Category | Zero-predicate alone |
|---|---|---|---|
| good-enough (hyphenated) | gap-vibe-coding-style-001 | 1 | YES |
| good enough for now | gap-vibe-coding-style-001 | 1 | YES |
| minimum viable | gap-vibe-coding-style-001 | 1 | YES |
| MVP | gap-vibe-coding-style-001 | 1 | YES |
| proof-of-concept | gap-vibe-coding-prototype-001 | 3 | YES |
| PoC | gap-vibe-coding-prototype-001 | 3 | YES |
| tracer bullet | gap-vibe-coding-spike-001 | 4 | YES |
| walking skeleton | gap-vibe-coding-spike-001 | 4 | YES |
| thin slice | gap-vibe-coding-spike-001 | 4 | YES |
| vertical slice | gap-vibe-coding-spike-001 | 4 | YES |
| paper prototype | gap-vibe-coding-prototype-001 | 3 | YES |
| wireframe | gap-vibe-coding-prototype-001 | 3 | YES |
| rough draft | gap-vibe-coding-rough-cut-001 | 8 | YES |

DomainId for all: "software-development"

---
