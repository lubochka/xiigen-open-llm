# Layer 3 — Teaching Unit Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-026-001 — Teaching: "barebones" is a style qualifier

**Teaching unit:** "barebones" → gap-vibe-coding-style-001

"Barebones" signals minimal-feature scope acceptance. No action verb is embedded.

**Rule application:**
- "barebones" alone → predicates = [] → knowledge_edge → SCL
- "barebones" + "build" → IMPLEMENT fires + gap warning

**Anti-pattern (v6-02):**
- Assuming "barebones" implies an implement intent is corrupt

---

## B32-L3-026-002 — Teaching: "stripped-down" is a style qualifier

**Teaching unit:** "stripped-down" → gap-vibe-coding-style-001

"Stripped-down" describes a reduced-feature version. Like "barebones", it expresses a quality bar, not a task.

**Boundary note:** "stripped-down UI" contains "UI" but that does not fire the UI predicate alone. UI fires only when a build/create/implement verb is also present.

---

## B32-L3-026-003 — Teaching: multi-term same gap ID = one SCL

**Teaching unit:** multiple qualifiers mapping to the same gap ID

When two or more qualifiers both map to gap-vibe-coding-style-001:
- Both gap IDs are recorded in GapWarnings (deduplicated)
- SCHEDULE_CAPABILITY_LEARNING is emitted exactly once
- No stacking of SCL events

**Example:** "barebones stripped-down" → GapWarnings = [gap-vibe-coding-style-001] → SCL × 1

---

## B32-L3-026-004 — Teaching: "stripped-down UI" boundary

"Stripped-down UI" alone → no UI predicate fire.

UI predicate fires when:
1. "screen" or "interface" is the object AND a build/create/implement verb is present
2. Or "UI" is used with a direct action verb

"Stripped-down UI" without a verb = gap warning only.

---
