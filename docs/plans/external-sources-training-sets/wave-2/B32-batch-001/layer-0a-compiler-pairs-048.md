# Layer 0-A — Compiler Pairs (048)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-048-001 — Round 3 complete qualifier reference (037-048)

**Reference: all Round 3 qualifiers and their gap IDs**

| Term / phrase | Gap ID | Zero predicates alone? |
|---|---|---|
| expedient | gap-vibe-coding-style-001 | YES |
| pragmatic | gap-vibe-coding-style-001 | YES |
| no-frills | gap-vibe-coding-style-001 | YES |
| barebones | gap-vibe-coding-style-001 | YES |
| stripped-down | gap-vibe-coding-style-001 | YES |
| lean | gap-vibe-coding-style-001 | YES |
| agile (informal) | gap-vibe-coding-style-001 | YES |
| iterative | gap-vibe-coding-style-001 | YES |
| just get it working | gap-vibe-coding-style-001 | YES |
| clean up later | gap-vibe-coding-style-001 | YES |
| wing it | gap-vibe-coding-style-001 | YES |
| whip up | gap-vibe-coding-hack-001 | NO (fires with object) |
| throw together | gap-vibe-coding-hack-001 | NO (fires with object) |
| bang out | gap-vibe-coding-hack-001 | NO (fires with object) |
| knock out | gap-vibe-coding-hack-001 | NO (fires with object) |

---

## B32-0A-048-002 — Round 3 co-occurrence rule summary

**Rule: action verbs fire regardless of any Round 3 qualifier above**

| Qualifier type | Action verb present | Result |
|---|---|---|
| Style adjective (lean/pragmatic/etc.) | build / implement / code | IMPLEMENT fires + gap-vibe-coding-style-001 |
| Hack phrase (whip up/bang out/etc.) | "implementation" noun | IMPLEMENT fires + gap-vibe-coding-hack-001 |
| Style adjective | test / unit test | TEST fires + gap-vibe-coding-style-001 |
| Hack phrase | "tests" object | TEST fires + gap-vibe-coding-hack-001 |
| Any qualifier | None | Predicates = [] → knowledge_edge |

**DomainId for all B32 outputs:** "software-development"

---

## B32-0A-048-003 — Negation pattern reference for Round 3

**Negations that suppress predicates (do NOT fire them):**

| Negation expression | Predicate suppressed |
|---|---|
| "no tests needed" | TEST |
| "skip the tests" | TEST |
| "don't bother with tests" | TEST |
| "no docs needed" | DOCUMENT |
| "skip documentation" | DOCUMENT |
| "skip the QA" | QA |
| "no QA needed" | QA |
| "don't secure it" | SECURITY |
| "skip the architecture" | ARCHITECTURE |

Negation patterns add no gap IDs to GapWarnings.

---

## B32-0A-048-004 — DomainId invariant: always "software-development"

**Invariant rule (all B32 outputs):**

DomainId = "software-development" in every B32 output.

Never: "project-management"
Never: "vibe-coding"
Never: null
Never: omitted

This applies to: compiler output, router output, factory output, orchestrator output, gap analysis output, teaching unit output, held-out and corruption pairs.

---
