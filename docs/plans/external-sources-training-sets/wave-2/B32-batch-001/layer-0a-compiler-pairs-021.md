# Layer 0-A — Compiler Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-021-001 — "vibe code a screen" → UI fires; "vibe code" is gap warning

**Request:** "Vibe code a screen for the user profile."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001
- "code" → IMPLEMENT fires via root match
- "screen" → UI fires (screen is in UI lexicon)

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**Rule confirmed:** "vibe code" produces IMPLEMENT (from "code") + gap warning (from "vibe"). "screen" independently fires UI.

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---

## B32-0A-021-002 — "vibe out a screen" → UI fires; no IMPLEMENT (no "code" root)

**Request:** "Vibe out a screen for the settings page."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001
- "out" → not a predicate trigger; "vibe out" does NOT contain "code" root
- "screen" → UI fires
- "page" → UI fires (second match)

**Result:** `["UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**Key distinction:** "vibe code" fires IMPLEMENT (contains "code" root). "vibe out" does NOT fire IMPLEMENT. UI fires on "screen" and "page" regardless.

**OutputDecision:** route to UI flow

**DomainId:** "software-development"

---

## B32-0A-021-003 — "vibe coding a modal" → IMPLEMENT + UI; modal is UI lexicon

**Request:** "Vibe coding a modal for the payment form."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001
- "coding" → root matches "code" → IMPLEMENT fires
- "modal" → UI fires (modal is in UI lexicon)
- "form" → UI fires (form is in UI lexicon)

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---

## B32-0A-021-004 — "vibe code a dashboard" → IMPLEMENT fires; UI does NOT fire (dashboard not in UI lexicon)

**Request:** "Vibe code a dashboard for the analytics."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001
- "code" → IMPLEMENT fires
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001

**Note:** "dashboard" does NOT fire UI. IMPLEMENT fires from "code". Both "vibe" and "dashboard" produce gap warnings.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-021-005 — "vibe" alone + "screen" → UI fires; no IMPLEMENT (no action verb beyond "vibe")

**Request:** "Just vibe the user registration screen."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001 (gap only; "vibe" alone is not an action verb)
- "screen" → UI fires
- "just" → not an action verb

**Result:** `["UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to UI flow

**DomainId:** "software-development"

---
