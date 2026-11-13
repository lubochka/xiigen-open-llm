# Layer 3 — Teaching Unit Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-031-001 — Teaching: gap qualifiers never suppress any predicate

**Teaching unit:** Rule 3 — absolute non-suppression

No style qualifier in B32 suppresses any registered action predicate.

| Request pattern | Predicate fires? |
|---|---|
| "expedient — implement X" | IMPLEMENT: YES |
| "lean — secure X" | SECURITY: YES |
| "pragmatic — test X" | TEST: YES |
| "no-frills — architect X" | ARCHITECTURE: YES |
| "barebones — define contract" | API_CONTRACT: YES |
| "stripped-down — document X" | DOCUMENT: YES |
| "iterative — QA X" | QA: YES |

Any output that suppresses a predicate in these patterns is v6-02 corruption.

---

## B32-L3-031-002 — Teaching: IMPLEMENT takes precedence regardless of how many predicates

**Teaching unit:** TransitionType = feature_addition when IMPLEMENT or UI is present

Regardless of how many other predicates are present:
- IMPLEMENT in Predicates → TransitionType = feature_addition (always)
- UI in Predicates → TransitionType = feature_addition (always)
- ARCHITECTURE, API_CONTRACT, DOCUMENT, TEST, QA, SECURITY alone → documentation or no_contribution

---

## B32-L3-031-003 — Teaching: SCL (SCHEDULE_CAPABILITY_LEARNING) rules

**Teaching unit:** SCL emission rules

SCL emits when:
- Predicates = [] AND gap vocabulary present

SCL does NOT emit when:
- Predicates are non-empty (even if gap vocabulary present)

SCL emits exactly once per request:
- Multiple gap IDs → still one SCL event
- Multiple qualifiers of same ID → still one SCL event

---

## B32-L3-031-004 — Teaching: all Round 3 qualifiers share gap-vibe-coding-style-001

Eight new qualifiers in Round 3 (expedient, pragmatic, no-frills, barebones, stripped-down, lean, agile, iterative) all share gap-vibe-coding-style-001. No new unique gap IDs were introduced for this group.

When any combination of these appears: GapWarnings = [gap-vibe-coding-style-001] (one entry, never doubled).

---
