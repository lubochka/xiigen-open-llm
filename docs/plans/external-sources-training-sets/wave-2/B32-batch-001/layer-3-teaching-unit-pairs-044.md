# Layer 3 — Teaching Unit Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-044-001 — Teaching: "quick" and "just enough" are gap qualifiers

**Teaching unit:** Additional style-001 vocabulary

"Quick" as informal speed/quality modifier → gap-vibe-coding-style-001.
"Just enough" as sufficiency framing → gap-vibe-coding-style-001.

Both follow the same rules as all style-001 vocabulary:
- Alone → knowledge_edge + SCL
- With action verb → predicate fires + gap warning

---

## B32-L3-044-002 — Teaching: six gap ID families (complete)

**Teaching unit:** All six gap ID families and their key vocabulary

1. gap-vibe-coding-style-001: quality-bar adjectives and vibe phrases (largest; 20+ terms)
2. gap-vibe-coding-hack-001: informal rushed-execution phrases
3. gap-vibe-coding-prototype-001: PoC/prototype/wireframe vocabulary
4. gap-vibe-coding-spike-001: exploration/skeleton vocabulary
5. gap-vibe-coding-rough-cut-001: rough draft/rough cut vocabulary
6. gap-vibe-coding-dashboard-001: dashboard specifically

Each family is distinct. Vocabulary from one family does not use another family's gap ID.

---

## B32-L3-044-003 — Teaching: pattern recognition for unknown vocabulary

**Teaching unit:** How to assign gap IDs to unknown vocabulary

When encountering new informal vocabulary:
1. Is it an action phrase implying rushed execution? → hack-001
2. Is it PoC/prototype/experimental scope? → prototype-001
3. Is it an exploration/skeleton/incremental scope? → spike-001
4. Is it a quality-bar adjective or disposition phrase? → style-001
5. Is it "dashboard" specifically? → dashboard-001
6. Is it a rough/draft qualifier? → rough-cut-001

When in doubt between hack-001 and style-001: action phrases → hack-001; adjectives → style-001.

---

## B32-L3-044-004 — Teaching: B32 training signal completeness

**Teaching unit:** What B32 has trained

B32 training covers:
- All gap vocabulary (Rounds 1-4; 30+ terms; 6 gap IDs)
- All predicate types (8 predicates; all action verbs)
- All co-occurrence patterns (gap alone, gap+verb, gap+negation, multi-gap, multi-predicate)
- All TransitionType combinations
- All corruption types (9 types)
- DomainId invariant
- SCL emission rules
- ContributionFlag rules
- GapMetadata structure

A model trained on this set should correctly handle any informal/vibe vocabulary gap detection scenario.

---
