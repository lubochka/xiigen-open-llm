# Layer 0-C — Knowledge Edge Pairs (028)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-028-001 — "run through" vs "run [tests]" disambiguation

"run through [topic]" → gap (walk-through/explanation sense).
"run the tests" → TEST fires (execute tests = code action).
"run through the test strategy" → gap ("run through" + "strategy" = explanation).
"run through the test strategy and run the tests" → TEST fires; "run through" = gap warning.

Critical distinction: "run through" = teaching vocabulary; "run [noun]" = execution vocabulary.

---

## KE-028-002 — "discuss" vs "review" overlap

"discuss [topic]" = dialogue/knowledge-sharing → gap.
"review [topic] with [person]" = collaborative examination → gap.
"discuss and review" (both present, no code action) → gap (both guidance).
"review [code] for [issues]" = examination with action intent → predicate fires based on issue type.

When both appear: gap unless a code action follows.

---

## KE-028-003 — "present" boundary with IMPLEMENT topics

"present the refactored service" → gap (present = communication; service = presentation topic).
"present and deploy the refactored service" → IMPLEMENT fires; "present" = gap warning.
"present the feature plan and implement the feature" → IMPLEMENT fires; "present" = gap warning.
"present the implementation for review" → gap (present for review = communication, not implementation action).

---

## KE-028-004 — "introduce" (software sense) edge cases

"introduce [feature] into [system]" → IMPLEMENT (software insertion).
"introduce a new endpoint" → IMPLEMENT (new code artifact).
"introduce breaking changes" → IMPLEMENT (code change action).
"introduce [topic] to [meeting/team]" → gap (teaching sense).

Distinguishing: does "introduce" have a software artifact as object? → IMPLEMENT. Person/team/concept as object? → gap.

---

## KE-028-005 — "show how to" boundary with integrate

"show how to integrate third-party services" → gap (show how to = teaching frame; "to integrate" = infinitive learning target).
"show the integration approach and integrate" → API_CONTRACT fires; "show" = gap warning.
"show the team integrating [service]" = observing → gap (third-party action: team integrates; requester shows/observes).
"show how to integrate" ≠ direct integrate action → gap; same rule as "teach to integrate."
