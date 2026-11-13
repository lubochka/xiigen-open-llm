# Layer 3 — Teaching Unit Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-009-001 — "coding" fires IMPLEMENT via "code" root match

**Teaching unit:** The gerund "coding" is the present participle of the verb "code". Since "code" is in the IMPLEMENT lexicon, "coding" fires IMPLEMENT through root matching.

**Core rule:**
```
"coding" → root: "code" → IMPLEMENT fires
This applies even when "coding" is preceded by gap vocabulary
"exploratory coding" → IMPLEMENT fires (on "coding") + gap-exploratory-001 (on "exploratory")
```

---

## B32-L3-009-002 — Root matching rule for gerunds

**Registered action verbs and their gerund forms that also trigger IMPLEMENT:**

| Base form (registered) | Gerund form | Also fires IMPLEMENT? |
|---|---|---|
| code | coding | Yes (root match) |
| build | building | Yes (root match, if applicable) |
| implement | implementing | Yes (root match) |
| create | creating | Yes (root match) |
| write | writing | Yes (root match) |

**Gerunds that do NOT fire IMPLEMENT (their roots are not in the lexicon):**

| Gerund | Root | In IMPLEMENT lexicon? |
|---|---|---|
| exploring | explore | No |
| hacking | hack | No |
| vibing | vibe | No |
| prototyping | prototype | No (prototype is not an action verb) |
| spiking | spike | No |

---

## B32-L3-009-003 — "exploratory coding": canonical B32 gerund example

**Request:** "Exploratory coding on the auth module."

**Parse:**
- "exploratory" → gap-vibe-coding-exploratory-001
- "coding" → "code" root → IMPLEMENT fires ✓

**Result:** [IMPLEMENT] + gap-exploratory-001

**Teaching point:** Despite the "exploratory" qualifier, IMPLEMENT fires because "coding" root-matches "code". This is the expected and correct behavior.

---

## B32-L3-009-004 — Context: when "coding" appears in B32 requests

**B32 requests with "coding" always fire IMPLEMENT (unless only "vibe coding" is present where "vibe" is the primary frame):**

Actually: "vibe coding" → "coding" still root-matches "code" → IMPLEMENT fires even in "vibe coding".

**Verification:**
- "exploratory coding" → IMPLEMENT ✓
- "vibe coding" → IMPLEMENT ✓ (via "coding" root match)
- "quick coding" → IMPLEMENT ✓
- "rough coding session" → IMPLEMENT ✓

All gerund uses of "coding" fire IMPLEMENT through root match.

---
