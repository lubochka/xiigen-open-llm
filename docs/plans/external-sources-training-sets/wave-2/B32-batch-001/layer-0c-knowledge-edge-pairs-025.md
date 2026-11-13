# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-025-001 — "good enough" knowledge edge: category and gap ID

**Knowledge edge entry:**

- **Term:** good enough / good-enough / good enough for now
- **Category:** Category 1 — Quality-bar framing (low-polish intent)
- **Gap ID:** gap-vibe-coding-style-001
- **Zero-predicate alone:** YES
- **Co-occurrence rule:** "good enough" + action verb → predicate fires; gap warning emitted; SCHEDULE_CAPABILITY_LEARNING NOT emitted

---

## B32-0C-025-002 — "minimum viable" knowledge edge

**Knowledge edge entry:**

- **Term:** minimum viable / minimum viable product / MVP
- **Category:** Category 1 — Quality-bar framing (low-polish intent)
- **Gap ID:** gap-vibe-coding-style-001
- **Zero-predicate alone:** YES
- **Note:** Same gap ID as "good enough" and other Category 1 terms. Router treats all Category 1 terms identically.

---

## B32-0C-025-003 — "proof-of-concept" knowledge edge

**Knowledge edge entry:**

- **Term:** proof-of-concept / PoC / proof of concept
- **Category:** Category 3 — Artifact-type without action (noun-only)
- **Gap ID:** gap-vibe-coding-prototype-001
- **Zero-predicate alone:** YES
- **Relation to "prototype":** Both "prototype" and "proof-of-concept" map to gap-vibe-coding-prototype-001

---

## B32-0C-025-004 — "spike" knowledge edge

**Knowledge edge entry:**

- **Term:** spike / technical spike / run a spike
- **Category:** Category 4 — Technical investigation framing
- **Gap ID:** gap-vibe-coding-spike-001
- **Zero-predicate alone:** YES
- **Expanded family:** tracer bullet, walking skeleton, thin slice, vertical slice → all gap-vibe-coding-spike-001

---

## B32-0C-025-005 — "paper prototype" and "wireframe" knowledge edge

**Knowledge edge entry:**

- **Terms:** paper prototype / wireframe / low-fidelity prototype
- **Category:** Category 3 — Artifact-type without action (noun-only)
- **Gap ID:** gap-vibe-coding-prototype-001
- **Zero-predicate alone:** YES (when no action verb present)
- **Exception:** "wireframe" + "screen" → screen fires UI predicate; "wireframe" adds gap warning

---
