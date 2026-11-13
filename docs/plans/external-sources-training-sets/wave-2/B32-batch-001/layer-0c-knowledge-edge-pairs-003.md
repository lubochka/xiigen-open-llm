# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-003-001 — "dashboard" not in UI lexicon: complete UI lexicon list

**Knowledge edge:** "dashboard" is NOT in the UI predicate lexicon. It does not fire the UI predicate.

**Complete UI predicate lexicon:**

| Token | Fires UI |
|---|---|
| screen | ✓ yes |
| page | ✓ yes |
| interface | ✓ yes |
| component | ✓ yes |
| form | ✓ yes |
| layout | ✓ yes |
| button | ✓ yes |
| input | ✓ yes |
| widget | ✓ yes |
| modal | ✓ yes |
| dashboard | ✗ NO — gap-vibe-coding-dashboard-001 |
| panel | ✗ NO — not in UI lexicon |
| view | ✗ NO — not in UI lexicon |
| window | ✗ NO — not in UI lexicon |
| sidebar | ✗ NO — not in UI lexicon |
| navbar | ✗ NO — not in UI lexicon |
| header | ✗ NO — not in UI lexicon |
| footer | ✗ NO — not in UI lexicon |

---

## B32-0C-003-002 — Why "dashboard" is not in the UI lexicon

**Reasoning:**

"Dashboard" is a high-level concept that encompasses multiple UI components (charts, tables, cards, navigation). It is not a primitive UI element. The UI predicate lexicon contains primitive UI elements that have direct implementation semantics.

A developer building a "dashboard" would build screens, pages, components, and forms. The request needs to specify which primitive elements are involved for the UI predicate to fire.

**Correct handling of "dashboard" requests:**

If a user asks "build a dashboard":
- "build" → IMPLEMENT fires
- "dashboard" → gap-vibe-coding-dashboard-001 (unrecognized UI noun; learning signal)
- Result: [IMPLEMENT] + gap warning

If a user asks "build a dashboard with a main screen":
- "build" → IMPLEMENT fires
- "screen" → UI fires
- "dashboard" → gap warning (still emitted)
- Result: [UI, IMPLEMENT] + gap-vibe-coding-dashboard-001

---

## B32-0C-003-003 — UI lexicon in practice: recognizing boundaries

**B32 context test cases:**

| Request contains | UI fires? | Why |
|---|---|---|
| "screen" | ✓ | screen is in lexicon |
| "dashboard" | ✗ | not in lexicon |
| "admin panel" | ✗ | neither "admin" nor "panel" in lexicon |
| "user interface" | ✓ | "interface" is in lexicon |
| "settings page" | ✓ | "page" is in lexicon |
| "login form" | ✓ | "form" is in lexicon |
| "nav component" | ✓ | "component" is in lexicon |
| "analytics view" | ✗ | "view" is not in lexicon |
| "modal dialog" | ✓ | "modal" is in lexicon |
| "input field" | ✓ | "input" is in lexicon |

---
