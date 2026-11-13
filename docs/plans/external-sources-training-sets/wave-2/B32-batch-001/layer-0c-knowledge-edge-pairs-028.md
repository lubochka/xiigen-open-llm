# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-028-001 — Spike family: knowledge edge taxonomy

**Extended Category 4 — Technical investigation framing**

Gap ID: gap-vibe-coding-spike-001

**Subcategories within spike family:**

1. **Point-investigation framing:** spike, technical spike, run a spike
   - Meaning: investigate feasibility of one technical approach
2. **Thin-path framing:** tracer bullet, tracer-bullet
   - Meaning: minimal end-to-end path through all system layers
3. **Minimal-skeleton framing:** walking skeleton
   - Meaning: thin end-to-end with all architectural layers but no flesh
4. **Vertical-slice framing:** thin slice, thin-slice, vertical slice
   - Meaning: minimal functional feature cutting all layers

All subcategories: same gap ID, same zero-predicate rule, same routing.

---

## B32-0C-028-002 — "rough draft" edge: rough-cut vs prototype family

**Boundary case: rough draft**

"rough draft" maps primarily to gap-vibe-coding-rough-cut-001 (document-artifact family).
"rough draft" also overlaps with artifact-type framing (prototype family) in context.

**Resolution rule:**
- "rough draft" for a document → gap-vibe-coding-rough-cut-001
- "rough draft" for code → gap-vibe-coding-rough-cut-001 (same; rough-cut family takes precedence)
- Both produce zero predicates alone

---

## B32-0C-028-003 — "MVP" disambiguation

**Edge case: MVP (Minimum Viable Product)**

MVP in software context = quality-bar framing → gap-vibe-coding-style-001
MVP in business context = not handled by B32 (B32 is software-development only)

**DomainId confirms context:** DomainId = "software-development" for all B32 requests.
MVP alone in software context → gap-vibe-coding-style-001 → zero predicates.

---

## B32-0C-028-004 — "good enough" co-occurrence edge cases

**Good enough with various action verbs:**

| Request | Predicate fired | Gap ID |
|---|---|---|
| "Good enough — build it" | IMPLEMENT | gap-vibe-coding-style-001 |
| "Good enough — design it" | ARCHITECTURE | gap-vibe-coding-style-001 |
| "Good enough — document it" | DOCUMENT | gap-vibe-coding-style-001 |
| "Good enough — test it" | TEST | gap-vibe-coding-style-001 |
| "Good enough — QA it" | QA | gap-vibe-coding-style-001 |
| "Good enough auth module" | NONE | gap-vibe-coding-style-001 |

Key rule: "it" as pronoun target does not fire a predicate; the action verb is what matters.

---

## B32-0C-028-005 — "paper prototype" + "screen" co-occurrence

**Edge case: artifact qualifier + UI trigger noun**

"Paper prototype for the signup screen":
- "paper prototype" → gap-vibe-coding-prototype-001
- "screen" → UI fires ✓

Result: ["UI"] + gap-vibe-coding-prototype-001

"Paper prototype for the signup flow" (no screen/page/interface noun):
- "paper prototype" → gap-vibe-coding-prototype-001
- "flow" → domain noun; no predicate

Result: [] + gap-vibe-coding-prototype-001 → knowledge_edge

---
