# Layer 3 — Teaching Unit Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-011-001 — False-positive taxonomy for B32: which vibe terms accidentally fire predicates

**Teaching unit:** Some vibe vocabulary contains embedded roots that match registered predicate lexicons. These are "accidental" predicate fires — the vibe term accidentally contains a predicate-triggering root.

**Key cases:**

| Vibe term | Embedded root | Fires predicate? | Correct behavior |
|---|---|---|---|
| vibe code | "code" | Yes — IMPLEMENT fires | Expected; "code" root fires IMPLEMENT |
| vibe coding | "coding" → "code" | Yes — IMPLEMENT fires | Expected; gerund root match |
| exploratory coding | "coding" → "code" | Yes — IMPLEMENT fires | Expected; vibe framing is gap |
| hack together | "hack" | No — not in lexicon | "hack" is gap only |
| throwaway | none | No | Pure gap vocabulary |
| prototype | none | No | Pure gap vocabulary |
| spike | none | No | Pure gap vocabulary |
| design spike | "design" | Yes — ARCHITECTURE fires | "design" fires ARCHITECTURE; "spike" is gap |

---

## B32-L3-011-002 — "accidental" IMPLEMENT fires from vibe vocabulary

**Pattern:** Vibe vocabulary that contains "code" as a root will always fire IMPLEMENT.

**Inventory of vibe terms that contain "code" root:**
- "vibe code" → "code" root → IMPLEMENT fires
- "vibe coding" → "coding" → "code" root → IMPLEMENT fires
- "exploratory coding" → "coding" → "code" root → IMPLEMENT fires
- "rough coding" → "coding" → "code" root → IMPLEMENT fires
- "quick coding session" → "coding" → IMPLEMENT fires

**These are NOT false positives.** They are CORRECT. The user IS asking to code something. The "vibe" qualifier is the framing; "code" is the action.

---

## B32-L3-011-003 — True zero-predicate vibe terms (no embedded predicate roots)

**Vibe terms that genuinely produce zero predicates when used alone:**

| Term | Contains predicate root? | Result |
|---|---|---|
| spike | No | zero predicates → knowledge_edge |
| vibe (alone) | No | zero predicates → knowledge_edge |
| prototype (alone) | No | zero predicates → knowledge_edge |
| hack together | No | zero predicates → knowledge_edge |
| throwaway | No | zero predicates → knowledge_edge |
| demo (alone) | No | zero predicates → knowledge_edge |
| nothing fancy (alone) | No | zero predicates if no other predicates |

---

## B32-L3-011-004 — Summary: the "false positive" is actually correct behavior

**Key teaching point:**

"False positive" is a misnomer in this context. When "vibe code" fires IMPLEMENT, this is CORRECT. The user wants to code. The vibe framing is style metadata. IMPLEMENT should fire.

The real risk is false NEGATIVES: "implement" NOT firing when it should (v6-02 corruption). Teaching this taxonomy helps the model understand that vibe vocabulary containing action roots will always fire those predicates — and that this is intended behavior.

---
