# Layer 3 — Teaching Unit Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-021-001 — "vibe code" contains "code" root: the IMPLEMENT trigger inside vibe framing

**Teaching unit:** "Vibe code" is a compound phrase where:
- "vibe" → gap-vibe-coding-style-001 (gap warning only)
- "code" → root matches "code" → IMPLEMENT fires

The "code" root inside "vibe code" fires IMPLEMENT independently of the "vibe" gap warning. These are two separate compiler signals.

---

## B32-L3-021-002 — The "vibe code" vs "vibe out" distinction

**This is the single most important "vibe coding" boundary:**

| Phrase | Contains "code" root? | IMPLEMENT fires? |
|---|---|---|
| vibe code | Yes | Yes |
| vibe coding | Yes (coding root = code) | Yes |
| vibe out | No | No |
| vibe session | No | No |
| just vibe | No | No |

**Rule:** IMPLEMENT fires if and only if the "code" root appears in the request. "Vibe" alone never fires IMPLEMENT.

---

## B32-L3-021-003 — "vibe code + screen" vs "vibe code + dashboard"

**vibe code + screen:**
- "code" → IMPLEMENT
- "vibe" → gap-style-001
- "screen" → UI
Result: [IMPLEMENT, UI] + gap warning

**vibe code + dashboard:**
- "code" → IMPLEMENT
- "vibe" → gap-style-001
- "dashboard" → NOT UI → gap-dashboard-001
Result: [IMPLEMENT] + two gap warnings (style-001 + dashboard-001)

**Teaching:** Whether UI fires depends on whether a UI lexicon term is present (screen, page, interface, component, form, layout, button, input, widget, modal). "Dashboard" is not that term.

---
