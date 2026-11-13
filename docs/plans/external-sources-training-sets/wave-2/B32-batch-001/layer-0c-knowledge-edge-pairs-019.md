# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-019-001 — "fast and loose" with ARCHITECTURE: style qualifier + architecture predicate

**Knowledge edge:** "fast and loose" is gap vocabulary. ARCHITECTURE fires independently when "design" or "architecture" is present.

**Request:** "Fast and loose design of the microservices architecture."

- "fast and loose" → gap-vibe-coding-style-001
- "design" → ARCHITECTURE fires
- "architecture" → ARCHITECTURE fires (confirmed)

**Result:** `["ARCHITECTURE"]` + gap-vibe-coding-style-001

**Teaching:** ARCHITECTURE fires on "design" and "architecture" regardless of the "fast and loose" framing. The style qualifier is a warning, not a suppressant.

---

## B32-0C-019-002 — "fast and loose" + "code" root: IMPLEMENT fires

**Request:** "Fast and loose coding — code the user-facing service."

- "fast and loose" → gap-vibe-coding-style-001
- "coding" → IMPLEMENT fires via "code" root
- "code" → IMPLEMENT fires (second match; deduplicates to [IMPLEMENT])

**Result:** `["IMPLEMENT"]` + gap-vibe-coding-style-001

---

## B32-0C-019-003 — "fast and loose approach" alone: zero predicates

**Request:** "Take a fast and loose approach here."

- "fast and loose" → gap-vibe-coding-style-001
- "approach" → domain noun; not an action verb
- "take" → not in IMPLEMENT lexicon

**Result:** `[]` + gap-vibe-coding-style-001 → knowledge_edge

**Teaching:** Neither "fast and loose" nor "approach" nor "take" fires any predicate. The system must see a registered action verb (implement, build, create, write, code, design, architect, integrate, secure, test, document, QA) to fire a predicate.

---
