# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-014-001 — "rough cut" vs "rough cut implementation": gap vs IMPLEMENT

**Knowledge edge:** "rough cut" alone → zero predicates. "rough cut" + action verb → predicate fires.

**Examples:**

| Request | Predicates | Reason |
|---|---|---|
| "A rough cut" | [] | No action verb |
| "Rough cut pipeline" | [] | "pipeline" is a domain noun, not an action verb |
| "Build a rough cut" | [IMPLEMENT] | "build" fires IMPLEMENT |
| "Rough cut implementation" | [IMPLEMENT] | "implementation" root fires IMPLEMENT |
| "Rough-cut design" | [ARCHITECTURE] | "design" fires ARCHITECTURE |
| "Rough cut of the API" | [] | No action verb; "API" is a noun here |

---

## B32-0C-014-002 — Hyphenated vs space-separated: same gap ID

**Rule:** "rough-cut" and "rough cut" are treated identically by the compiler.

Both map to gap-vibe-coding-rough-cut-001.

Neither form fires any predicate independently. Both produce the same gap warning.

---

## B32-0C-014-003 — "rough cut" vs "rough" alone: different gap status

**Distinction:**

- "rough cut" (two words or hyphenated) → gap-vibe-coding-rough-cut-001 (registered gap ID)
- "rough" (alone as an adjective) → general style qualifier; no registered gap ID in B32

**Examples:**

| Request | Gap ID | Reason |
|---|---|---|
| "Rough cut architecture" | gap-vibe-coding-rough-cut-001 + ARCHITECTURE fires | "rough cut" is registered gap |
| "Rough architecture sketch" | no gap ID | "rough" alone is not registered gap |
| "Rough-cut UI" | gap-vibe-coding-rough-cut-001 | hyphenated form matches |

**Teaching:** "rough" (standalone adjective) is understood as stylistic, but it does not emit a registered gap ID. Only the compound "rough cut" / "rough-cut" is a registered B32 gap term.

---
