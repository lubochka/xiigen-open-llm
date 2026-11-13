# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-023-001 — Sign-off: "sign-off" vs "handoff" — critical vocabulary boundary

"sign-off" → QA (always — rule 23)
"handoff" → no predicate (operational transfer noun)

These two words look similar but have completely different predicate mappings:
- "sign-off" contains the approval action "sign" → QA fires
- "handoff" is a process transfer term with no approval semantics in the predicate model

Never confuse these two. "handoff" does NOT fire QA. Only "sign-off" (and sign off, sign-offs) fires QA.

---

## B38-0C-023-002 — Sign-off with gap vocabulary: QA always fires (v6-01 rule)

Input: "Sign off on the go-live checklist and confirm launch readiness."
- "Sign off" → QA (fires regardless of surrounding gap vocabulary)
- "go-live" → GAP (warning only; does NOT suppress QA)
- "checklist" → GAP (warning only; does NOT suppress QA)
- "launch" → GAP (warning only; does NOT suppress QA)
- "confirm" → not in predicate lexicon
- "readiness" → not a predicate trigger alone

Result: ["QA"] with three gap warnings.

v6-01 corruption would produce: CompiledPredicateIds: [] (incorrectly suppressing QA due to gap context).
Correct result: QA fires. Gap vocabulary produces warnings, never predicate suppression.

---

## B38-0C-023-003 — Sign-off without service: confirm QA-only result

"Collect all release sign-offs from team leads and confirm the release is authorized."
- "sign-offs" → QA
- "confirm" → not a predicate trigger
- "authorized" → not a predicate trigger
- "release" → context word
- "team leads" → context phrase

Result: ["QA"] only.
No IMPLEMENT, no DOCUMENT, no SECURITY.
TransitionType: no_contribution.

Lesson: When no service/build/pipeline/criteria vocabulary appears in a sign-off sentence, the result is strictly QA alone. This is the canonical QA-only path in B38.
