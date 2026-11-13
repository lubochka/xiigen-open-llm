# MANIFEST — B32-batch-001
# Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Batch type: CAPABILITY GAP (style vocabulary; DomainId: "software-development")
# Production date: 2026-05-05 (files 001–012), 2026-05-06 (files 013–024, synthesis 005–008)
# Layer 2 type: capability-gap-extended-pairs (NOT nine-node pairs)

---

## Summary

This batch contains training data for the XIIGen AdaptiveLogicalOrchestrator covering Scenario B32: vibe coding style vocabulary. The batch teaches the system how to handle requests framed with informal, exploratory, or low-quality-bar vocabulary while correctly routing on registered action predicates.

**Total files:** 201 (200 content files + 1 MANIFEST)
- Layer 0-A compiler pairs: 23 files (002-024)
- Layer 0-B router pairs: 23 files (002-024)
- Layer 0-C knowledge-edge pairs: 23 files (002-024)
- Layer 0-D flow factory pairs: 23 files (002-024)
- Layer 1 transition orchestrator pairs: 23 files (002-024)
- Layer 2 capability-gap-extended pairs: 23 files (002-024)
- Layer 3 teaching-unit pairs: 23 files (002-024)
- Held-out and corruption pairs: 23 files (002-024)
- Cross-layer synthesis: 8 files (001-008)
- MANIFEST.md: 1 file

---

## File Count Table

| File type | Pattern | Files | Count |
|---|---|---|---|
| Layer 0-A compiler pairs | layer-0a-compiler-pairs-NNN.md | 002-024 | 23 |
| Layer 0-B router pairs | layer-0b-router-pairs-NNN.md | 002-024 | 23 |
| Layer 0-C knowledge-edge pairs | layer-0c-knowledge-edge-pairs-NNN.md | 002-024 | 23 |
| Layer 0-D flow factory pairs | layer-0d-flow-factory-pairs-NNN.md | 002-024 | 23 |
| Layer 1 transition orchestrator | layer-1-transition-orchestrator-pairs-NNN.md | 002-024 | 23 |
| Layer 2 capability-gap-extended | layer-2-capability-gap-extended-pairs-NNN.md | 002-024 | 23 |
| Layer 3 teaching-unit pairs | layer-3-teaching-unit-pairs-NNN.md | 002-024 | 23 |
| Held-out and corruption pairs | held-out-and-corruption-pairs-NNN.md | 002-024 | 23 |
| Cross-layer synthesis | cross-layer-synthesis-NNN.md | 001-008 | 8 |
| MANIFEST | MANIFEST.md | - | 1 |
| **TOTAL** | | | **201** |

---

## Layer 2 Extended Pairs Table

Layer 2 uses capability-gap-extended-pairs format (NOT nine-node pairs).

| File | Topic |
|---|---|
| layer-2-capability-gap-extended-pairs-002.md | "spike" request: full knowledge_edge trace (DomainId: software-development) |
| layer-2-capability-gap-extended-pairs-003.md | "prototype alone" vs "build prototype": side-by-side comparison |
| layer-2-capability-gap-extended-pairs-004.md | "dashboard" vs "screen": full boundary trace |
| layer-2-capability-gap-extended-pairs-005.md | "hack together" / "throwaway": full knowledge_edge trace |
| layer-2-capability-gap-extended-pairs-006.md | "quick and dirty build": IMPLEMENT fires; style qualifiers are warnings only |
| layer-2-capability-gap-extended-pairs-007.md | "exploratory coding": IMPLEMENT fires via "code" root; exploratory is gap |
| layer-2-capability-gap-extended-pairs-008.md | "vibe code a screen": UI fires; vibe code is gap warning |
| layer-2-capability-gap-extended-pairs-009.md | "rough-cut architecture": ARCHITECTURE fires; rough-cut is gap warning |
| layer-2-capability-gap-extended-pairs-010.md | "just make it work + implement": IMPLEMENT fires; "just make it work" is gap |
| layer-2-capability-gap-extended-pairs-011.md | Seven-predicate vibe session: all predicates fire; only "vibe session" is gap |
| layer-2-capability-gap-extended-pairs-012.md | Complete B32 gap vocabulary map + gap-ID naming reference |
| layer-2-capability-gap-extended-pairs-013.md | "scrappy" gap analysis cases |
| layer-2-capability-gap-extended-pairs-014.md | "rough cut" gap analysis cases |
| layer-2-capability-gap-extended-pairs-015.md | "quick win" mixed cases — IMPLEMENT fires, gap warning |
| layer-2-capability-gap-extended-pairs-016.md | "hack together" gap cases |
| layer-2-capability-gap-extended-pairs-017.md | "low-fi" gap analysis cases |
| layer-2-capability-gap-extended-pairs-018.md | "throwaway" gap with UI mixed |
| layer-2-capability-gap-extended-pairs-019.md | "fast and loose" with ARCHITECTURE mixed |
| layer-2-capability-gap-extended-pairs-020.md | Multiple style qualifiers in one request |
| layer-2-capability-gap-extended-pairs-021.md | Gap + code predicate disambiguation (vibe code vs vibe out) |
| layer-2-capability-gap-extended-pairs-022.md | Dashboard vs screen UI boundary analysis |
| layer-2-capability-gap-extended-pairs-023.md | Comprehensive style qualifier taxonomy |
| layer-2-capability-gap-extended-pairs-024.md | Complete capability-gap-extended summary |

---

## Cross-Layer Synthesis Table

| File | Request summary | Key teaching |
|---|---|---|
| cross-layer-synthesis-001.md | "Vibe out a dashboard prototype." | Zero predicates; vibe+dashboard+prototype all gaps; knowledge_edge + SCHEDULE_CAPABILITY_LEARNING |
| cross-layer-synthesis-002.md | "Build something quick and dirty for the shopping cart — just make it work." | IMPLEMENT fires on "build" despite vibe framing; style becomes warnings only |
| cross-layer-synthesis-003.md | "Vibe code the user profile screen and make it feel right." | UI fires on "screen"; IMPLEMENT fires on "code" root; parallel flow execution |
| cross-layer-synthesis-004.md | "Quick vibe session: architect, integrate, build, secure, test, document, QA." | All 7 predicates fire; "vibe session" = 1 gap warning; 4-phase ordered execution |
| cross-layer-synthesis-005.md | "Let's just vibe this." | Pure gap: "vibe" alone (not "vibe code") → knowledge_edge |
| cross-layer-synthesis-006.md | "Vibe code a screen for the user profile." | IMPLEMENT fires (code root) + UI fires (screen) + gap warning; vibe code is NOT pure gap |
| cross-layer-synthesis-007.md | "Build something quick and dirty for the shopping cart." | IMPLEMENT fires on "build"; "quick and dirty" = gap warning only; no suppression |
| cross-layer-synthesis-008.md | "Rough architecture, build it scrappy, no tests needed." | ARCHITECTURE + IMPLEMENT fire; "scrappy" = gap warning; negated "no tests" does NOT fire TEST |

---

## Registered Gap IDs

All B32 gap IDs use the prefix gap-vibe-coding-.

| Gap ID | Primary triggers |
|---|---|
| gap-vibe-coding-style-001 | vibe, vibe code, vibe coding, creative mode, good enough, scrappy, low-fi, lo-fi, quick win, fast and loose, scaffold (alone) |
| gap-vibe-coding-exploratory-001 | exploratory, explore |
| gap-vibe-coding-prototype-001 | prototype (alone, no action verb) |
| gap-vibe-coding-dashboard-001 | dashboard |
| gap-vibe-coding-spike-001 | spike, run a spike, technical spike |
| gap-vibe-coding-hack-001 | hack together, cobble together, throw together, hack (dev verb) |
| gap-vibe-coding-demo-001 | demo (standalone) |
| gap-vibe-coding-rough-cut-001 | rough-cut, rough cut, rough draft |
| gap-vibe-coding-intuition-001 | intuition-driven, feel right, feels right |
| gap-vibe-coding-throwaway-001 | throwaway, throwaway code |
| gap-vibe-coding-quick-dirty-001 | quick and dirty |
| gap-vibe-coding-just-works-001 | just make it work, just get it working |
| gap-vibe-coding-nothing-fancy-001 | nothing fancy |

---

## 12 Key Teaching Points for B32

1. **"prototype" requires action verb**: "prototype" alone produces zero predicates and routes to knowledge_edge. "build/create/write/implement/code + prototype" fires IMPLEMENT on the action verb; "prototype" becomes a gap warning.

2. **"dashboard" is NOT in UI lexicon**: UI fires on: screen, page, interface, component, form, layout, button, input, widget, modal. "dashboard" emits gap-vibe-coding-dashboard-001 but never fires UI.

3. **Style qualifiers never suppress action predicates**: All B32 vibe vocabulary produces gap warnings only. Action verbs fire predicates unconditionally regardless of surrounding style framing. Applies to all 7 predicates.

4. **B32 DomainId is always "software-development"**: Never "project-management" (B36/B37) or any invented value. Every B32 output uses DomainId "software-development".

5. **"spike" is pure gap vocabulary**: spike, run a spike, technical spike produce gap-vibe-coding-spike-001 and never fire any predicate. Zero predicates when spike is the only verb.

6. **All "vibe" forms are gap**: vibe, vibe out produce gap-vibe-coding-style-001. "vibe code" / "vibe coding" additionally fire IMPLEMENT via the "code" root.

7. **"coding" fires IMPLEMENT via "code" root match**: The gerund "coding" root-matches "code" and fires IMPLEMENT. Applies in "exploratory coding", "vibe coding", "quick coding".

8. **SCHEDULE_CAPABILITY_LEARNING fires exactly once when predicates are empty**: Multiple gap IDs bundle into one event. Never emitted when predicates are non-empty.

9. **v6-01 corruption: VIBE_CODING predicate does not exist**: "VIBE_CODING" is not registered. Correct behavior: route on "code" root or emit gap warning.

10. **v6-02 corruption: style framing never suppresses predicates**: "just make it work" + "implement" fires IMPLEMENT. Any output with action verb present but predicates empty is v6-02 corrupt.

11. **v6-03 corruption: DomainId is never "project-management" for B32**: Always "software-development". Wrong DomainId misroutes SCHEDULE_CAPABILITY_LEARNING.

12. **QA always executes last in multi-predicate flows**: QAFlow is the final phase. Vibe framing and ordering requests do not change this constraint.

---

## New Teaching Points from Files 013–024

13. **"scrappy" is a style qualifier, not an action verb**: "scrappy" alone → gap-vibe-coding-style-001 → zero predicates. With action verb: predicate fires + gap warning.

14. **"rough cut" vs "rough" alone**: "rough cut" / "rough-cut" → gap-vibe-coding-rough-cut-001. "rough" alone is a general style adjective with no registered gap ID.

15. **"quick win" is scope vocabulary**: "quick win" alone → gap. "quick win" + action verb → predicate fires + gap warning.

16. **"hack together" synonym family**: hack together, cobble together, throw together all map to gap-vibe-coding-hack-001. None fire IMPLEMENT.

17. **"low-fi" / "lo-fi" are quality-level framing**: Both map to gap-vibe-coding-style-001. Neither fires any predicate alone.

18. **Future-tense intent does not fire predicates**: "we'll rewrite later" is future-tense; does not fire IMPLEMENT for the current request.

19. **Negated TEST does not fire TEST**: "no tests needed" negates TEST. Only positive imperative mentions of test/testing fire TEST.

20. **"vibe code" vs "vibe out" distinction**: "vibe code" contains "code" root → IMPLEMENT fires. "vibe out" does not contain "code" root → IMPLEMENT does not fire.

---

## Production Notes

- Files 001–012 (all layers) produced 2026-05-05
- Files 013–024 (all layers) + cross-layer synthesis 005–008 produced 2026-05-06
- Layer 2 uses capability-gap-extended-pairs format per batch specification
- DomainId "software-development" used throughout (B32 invariant)
- Layer files numbered 002-024 (001 reserved for batch-level reference files)
- Cross-layer synthesis files numbered 001-008
- Total content files produced: 200
- MANIFEST.md is 1 additional file (total: 201)
