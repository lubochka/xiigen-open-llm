# Layer 3 — Teaching Unit Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-025-001 — Teaching: "expedient" is a style qualifier

**Teaching unit:** "expedient" → gap-vibe-coding-style-001

"Expedient" communicates acceptance of a pragmatic shortcut. It does NOT contain an action verb.

**Rule application:**
- "expedient" alone → predicates = [] → knowledge_edge → SCHEDULE_CAPABILITY_LEARNING
- "expedient" + "implement" → IMPLEMENT fires → active flow + gap warning

**Anti-pattern (v6-02 corruption):**
- "expedient" present → predicates = [] even though "expedient" sounds action-like
- "expedient" never fires IMPLEMENT on its own

---

## B32-L3-025-002 — Teaching: "pragmatic" is a style qualifier

**Teaching unit:** "pragmatic" → gap-vibe-coding-style-001

"Pragmatic" describes an attitude toward quality trade-offs. Same gap family as "good enough" and "scrappy".

**Boundary note:** "pragmatic" ≠ "practical" ≠ task. "Be pragmatic" has no action verb.

**Rule application:**
- "pragmatic" alone → predicates = [] → SCL
- "pragmatic" + "test" → TEST fires + gap warning

---

## B32-L3-025-003 — Teaching: "no-frills" is a style qualifier

**Teaching unit:** "no-frills" → gap-vibe-coding-style-001

"No-frills" signals low-complexity, low-quality-bar framing. Common in "no-frills API" or "no-frills UI" patterns.

**Key distinction:**
- "no-frills API" alone → predicates = [] (no "define", "build", or "create" present)
- "no-frills API — define the contract" → API_CONTRACT fires + gap warning

---

## B32-L3-025-004 — Teaching: all three Round 3 openers share gap-vibe-coding-style-001

| Qualifier | Gap ID | Zero predicates alone? |
|---|---|---|
| expedient | gap-vibe-coding-style-001 | YES |
| pragmatic | gap-vibe-coding-style-001 | YES |
| no-frills | gap-vibe-coding-style-001 | YES |

No qualifier in this group has a unique gap ID. All share gap-vibe-coding-style-001.

---
