# Layer 3 — Teaching Unit Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-036-001 — Teaching: Round 3 gap vocabulary complete rules

**Teaching unit: all Round 3 rules**

**Rule A:** Style adjectives (expedient, pragmatic, lean, no-frills, barebones, stripped-down, agile, iterative) → gap-vibe-coding-style-001.

**Rule B:** Vibe phrases (just get it working, clean up later, wing it) → gap-vibe-coding-style-001.

**Rule C:** Informal action phrases (whip up, bang out, knock out, throw together) → gap-vibe-coding-hack-001.

**Rule D:** All three groups: alone → predicates = [] → knowledge_edge → SCL.

**Rule E:** Action verb alongside any qualifier → predicate fires + gap warning.

---

## B32-L3-036-002 — Teaching: negation patterns complete rules

**Teaching unit: all negation patterns**

- "no X needed", "skip the X", "don't bother with X" → X's predicate does NOT fire
- Negation patterns add no gap IDs
- Gap vocabulary still fires SCL if present
- Positive predicates nearby still fire

---

## B32-L3-036-003 — Teaching: the nine absolute B32 corruption types

**v6-01:** VIBE_CODING predicate used (not registered)
**v6-02:** Gap qualifier suppresses action predicate (predicate absent when verb present)
**v6-03:** DomainId = "project-management" (must be "software-development")
**v6-04:** SCL emitted when predicates non-empty (SCL only for empty predicates)
**v6-05:** SCL emitted multiple times (once per request)
**v6-06:** Negated vocabulary fires its predicate
**v6-07:** Wrong gap ID used (hack-001 vs style-001 vs spike-001)
**v6-08:** "dashboard" treated as UI predicate (always gap only)
**v6-09:** DomainId null or omitted

---

## B32-L3-036-004 — Teaching: complete action verb to predicate mapping

| Action verb | Predicate fired |
|---|---|
| implement, build, create, write (code), code | IMPLEMENT |
| design UI, build screen, create interface | UI |
| architect, design (arch context), structure | ARCHITECTURE |
| define contract, specify API, document contract | API_CONTRACT |
| secure, audit (security) | SECURITY |
| test, unit test, write tests, bang out tests | TEST |
| document, write docs, create docs | DOCUMENT |
| QA, quality assurance, run QA | QA |

---
