# Layer 3 — Teaching Unit Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-023-001 — Teaching: sign-off collection is the canonical QA-only path

LESSON: Pure sign-off collection (no service/build/criteria) = QA alone = no_contribution.

Canonical QA-only input: "Collect all release sign-offs from team leads and confirm the release is authorized."
Result: ["QA"] → no_contribution → 2 files → release_authorized

This is the simplest and most common B38 path. It demonstrates:
- QA fires from "sign-offs" (and would also fire from "confirm" if it were in lexicon)
- No other predicate fires
- TransitionType = no_contribution (governance action only)
- FileCount = 2 (sign_off_register + release_authorization)

---

## B38-TU-023-002 — Teaching: gap vocabulary never suppresses QA (absolute v6-01 rule)

LESSON: The most important anti-corruption rule for sign-off scenarios.

"Sign off on the go-live checklist and confirm launch readiness."
- "sign off" → QA (fires)
- "go-live" → GAP (warning; does NOT suppress QA)
- "checklist" → GAP (warning; does NOT suppress QA)
- "launch" → GAP (warning; does NOT suppress QA)

Three gap terms → three gap warnings. QA still fires. One routing decision (7.1).

WRONG (v6-01 corruption): "go-live, checklist, launch present → all vocabulary is gap → knowledge_edge"
CORRECT: "sign off" is in the QA lexicon → QA fires regardless of surrounding gap vocabulary.

---

## B38-TU-023-003 — Teaching: sign-off with service forces IMPLEMENT (rule 2)

LESSON: "Get sign-off on the analytics service" → QA + IMPLEMENT.

The user wants a sign-off (QA). But "analytics service" contains "service" → IMPLEMENT fires (rule 2).

Wrong inference: "This is a sign-off request → QA only"
Correct inference: "service is present → IMPLEMENT fires → QA+IMPLEMENT"

The compiler is literal. It does not consider intent. It evaluates each word independently.
Even when the purpose is a governance action (sign-off), the presence of "service" forces IMPLEMENT.
