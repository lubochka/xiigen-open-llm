# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-016-001 — "hack together" vs "hack the service": both are gap in B32

**Knowledge edge:** In B32, both "hack together" (idiomatic: casual assembly) and "hack [a system]" (verb: modify/patch) are classified as gap vocabulary.

**Rule:**
- "hack together" → gap-vibe-coding-hack-001 (canonical form)
- "hack" as a standalone verb → gap-vibe-coding-hack-001 (same ID)
- Neither form fires IMPLEMENT

**Reason:** "Hack" is not in the IMPLEMENT lexicon (implement, build, create, write, code). While "hack" can colloquially mean "implement", this batch conservatively assigns it to gap vocabulary to teach the system the distinction.

---

## B32-0C-016-002 — "hack together" synonyms: cobble together, throw together

**All three map to gap-vibe-coding-hack-001:**

| Phrase | Gap ID |
|---|---|
| hack together | gap-vibe-coding-hack-001 |
| cobble together | gap-vibe-coding-hack-001 |
| throw together | gap-vibe-coding-hack-001 |

When combined with an action verb, the predicate fires and the gap warning is emitted:
- "hack together and build" → [IMPLEMENT] + gap-hack-001
- "cobble together a screen" → [UI] from "screen" (if "screen" present) + gap-hack-001 (if no action verb, UI fires from "screen")

---

## B32-0C-016-003 — "hack" in a non-gap context: SECURITY vocabulary

**Edge case:** "hack" in a security context may trigger SECURITY, not just gap vocabulary.

**Request:** "Prevent hackers from breaking into the system — secure the authentication."

- "secure" → SECURITY fires
- "hackers" → security context noun; not a gap trigger in this form
- "hack" (noun form "hackers") does not trigger gap-vibe-coding-hack-001

**Rule:** "hack together" / "hack [system]" as a development methodology verb → gap. "hacker" / "hack" in a security-threat context → SECURITY vocabulary, not gap.

---
