# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-006-001 — "spike" / "hack" / "demo" / "throwaway": complete gap vocabulary list

**Knowledge edge:** These terms are ALL gap vocabulary. None of them fire any registered predicate. They always produce gap warnings.

### Spike vocabulary

| Term | Gap ID | Notes |
|---|---|---|
| spike | gap-vibe-coding-spike-001 | technical spike (investigation) |
| "run a spike" | gap-vibe-coding-spike-001 | phrase form |
| "spike on X" | gap-vibe-coding-spike-001 | verb form |

### Hack vocabulary

| Term | Gap ID | Notes |
|---|---|---|
| hack together | gap-vibe-coding-hack-001 | canonical phrase |
| cobble together | gap-vibe-coding-hack-001 | synonym |
| throw together | gap-vibe-coding-hack-001 | synonym |
| put together | gap-vibe-coding-hack-001 | softer synonym |

### Demo vocabulary

| Term | Gap ID | Notes |
|---|---|---|
| demo (standalone) | gap-vibe-coding-demo-001 | "for the demo", "demo it" |

**Note:** "demo" as part of "demo the feature" might suggest DOCUMENT or QA in some contexts. In B32, "demo" alone is gap. If "demo" co-occurs with "build" ("build a demo"), IMPLEMENT fires on "build" and "demo" is gap.

### Throwaway vocabulary

| Term | Gap ID | Notes |
|---|---|---|
| throwaway | gap-vibe-coding-throwaway-001 | noun or adjective |
| throwaway code | gap-vibe-coding-throwaway-001 | phrase form |
| throwaway prototype | gap-vibe-coding-throwaway-001 + gap-vibe-coding-prototype-001 | both warnings emitted |

---

## B32-0C-006-002 — None of these terms fire predicates

**Verification table:**

| Term | Does it fire IMPLEMENT? | Does it fire any predicate? |
|---|---|---|
| spike | NO | NO |
| hack together | NO | NO |
| demo | NO | NO |
| throwaway | NO | NO |
| cobble together | NO | NO |
| throw together | NO | NO |

**These terms will ONLY appear in GapWarnings, never in Predicates.**

---

## B32-0C-006-003 — "spike" + "design": spike is gap but "design" fires ARCHITECTURE

This is the critical co-occurrence pattern. When "spike" appears with a registered predicate trigger:

"Spike the design of the architecture":
- "spike" → gap-vibe-coding-spike-001
- "design" → ARCHITECTURE fires ✓
- "architecture" → ARCHITECTURE fires ✓ (deduplicated)

Result: [ARCHITECTURE] + gap warning for spike

The spike does not suppress ARCHITECTURE. It is a gap warning only.

---
