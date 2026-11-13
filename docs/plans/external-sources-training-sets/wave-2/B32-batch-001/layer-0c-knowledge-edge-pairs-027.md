# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-027-001 — gap + TEST: knowledge edge for co-occurrence pattern

**Knowledge edge: when gap vocabulary co-occurs with TEST trigger**

Rule: TEST fires when "test", "unit test", "write tests", "add tests", "testing" appear as positive action imperatives.

| Pattern | TEST fires? | Gap warning? |
|---|---|---|
| "good enough — write tests" | YES | YES (gap-vibe-coding-style-001) |
| "scrappy but test the paths" | YES | YES (gap-vibe-coding-style-001) |
| "PoC + test coverage" | YES | YES (gap-vibe-coding-prototype-001) |
| "spike — run tests after" | YES (run tests = positive) | YES (gap-vibe-coding-spike-001) |
| "no tests needed" | NO (negated) | — |

---

## B32-0C-027-002 — gap + DOCUMENT: knowledge edge for co-occurrence

**Knowledge edge: DOCUMENT trigger vocabulary**

DOCUMENT fires when: "document", "docs", "write documentation", "add docs", "spec" (as document object) appear as positive action targets.

| Pattern | DOCUMENT fires? | Gap warning? |
|---|---|---|
| "rough draft — document endpoints" | YES | YES (gap-vibe-coding-rough-cut-001) |
| "good enough docs" | YES ("docs" as target) | YES (gap-vibe-coding-style-001) |
| "minimum viable — write spec" | YES | YES (gap-vibe-coding-style-001) |
| "thin slice — document happy path" | YES | YES (gap-vibe-coding-spike-001) |

---

## B32-0C-027-003 — gap + QA: knowledge edge for co-occurrence

**Knowledge edge: QA trigger vocabulary**

QA fires when: "QA", "quality assurance", "qa the", "do QA on" appear as positive directives.

| Pattern | QA fires? | Gap warning? |
|---|---|---|
| "good enough — QA before release" | YES | YES (gap-vibe-coding-style-001) |
| "scrappy but QA the form" | YES | YES (gap-vibe-coding-style-001) |
| "minimum viable + QA" | YES | YES (gap-vibe-coding-style-001) |
| "PoC, implement, QA" | YES | YES (gap-vibe-coding-prototype-001) |

---

## B32-0C-027-004 — Negation does NOT fire predicates

**Knowledge edge: negation patterns that do NOT fire predicates**

| Phrase | Predicate NOT fired | Reason |
|---|---|---|
| "no tests needed" | TEST | Negated |
| "don't bother testing" | TEST | Negated |
| "skip the tests" | TEST | Negated |
| "no documentation required" | DOCUMENT | Negated |
| "no QA needed" | QA | Negated |
| "no security concerns" | SECURITY | Negated |

**Rule:** Negated mentions of action vocabulary do NOT fire predicates. Only positive imperatives fire.

---

## B32-0C-027-005 — DomainId invariant note

**For all B32 knowledge-edge outputs:**

DomainId = "software-development" regardless of:
- Number of gap IDs
- Which predicates fired
- Whether SCHEDULE_CAPABILITY_LEARNING was emitted
- Style of the request (formal vs informal)

DomainId is set once at request classification and never changes mid-processing.

---
