# Layer 3 — Teaching Unit Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-027-001 — Teaching: "lean" as quality-bar adjective

**Teaching unit:** "lean" → gap-vibe-coding-style-001

"Lean" as a quality-bar adjective (lowercase, not Lean methodology) triggers gap-vibe-coding-style-001.

**Distinction:**
- "lean" (adjective): "keep it lean" → gap warning
- "Lean" (methodology): "Lean startup principle" → not a B32 gap trigger

**Rule:** When "lean" appears with an action verb, IMPLEMENT (or other predicate) fires and gap warning is added.

---

## B32-L3-027-002 — Teaching: "agile" as informal qualifier

**Teaching unit:** "agile" (informal) → gap-vibe-coding-style-001

Informal "agile" signals flexibility preference, not a software methodology invocation.

**Boundary:** "agile" + "sprint" or "agile" + "Scrum" is methodology context → B32 gap rules do not apply.

**Rule:** "agile" in isolation or as adjective ("agile approach", "stay agile") → gap-vibe-coding-style-001.

---

## B32-L3-027-003 — Teaching: QA predicate fires independently

**Teaching unit:** QA predicate rules

QA fires when "QA" or "quality assurance" appears as a positive imperative ("QA the release", "run QA").

QA does NOT fire when:
- "QA" appears as a noun only ("the QA team")
- "QA" appears negated ("no QA needed")

Gap qualifiers do not suppress QA when QA fires.

---

## B32-L3-027-004 — Teaching: TransitionType = no_contribution for QA and TEST

| Predicate | TransitionType |
|---|---|
| QA | no_contribution |
| TEST | no_contribution |
| SECURITY | no_contribution |
| DOCUMENT | documentation |
| API_CONTRACT | documentation |
| ARCHITECTURE | documentation |
| IMPLEMENT | feature_addition |
| UI | feature_addition |

When IMPLEMENT or UI is present alongside others, feature_addition takes precedence.

---
