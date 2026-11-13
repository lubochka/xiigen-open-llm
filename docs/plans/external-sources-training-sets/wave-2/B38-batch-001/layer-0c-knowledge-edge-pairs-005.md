# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-005-001 — Complete B38 gap vocabulary list

The following terms are B38 gap vocabulary (B38 Core Rule 5 and extensions):

| Term | Gap ID | Rule Reference |
|---|---|---|
| go-live | gap-project-delivery-go-live-001 | Rule 5 |
| checklist | gap-project-delivery-checklist-001 | Rule 5 |
| rollout | gap-project-delivery-rollout-golive-001 | Rule 5 |
| cutover | gap-project-delivery-cutover-001 | Rule 5 |
| runbook | gap-project-delivery-runbook-001 | Rule 5 |
| launch | gap-project-delivery-launch-001 | Rule 5 |
| on-call | (operational vocabulary) | Rule 5 |
| monitor | (not in any predicate) | Rule 7 |
| execute (in execution context) | (not IMPLEMENT in execution context) | Rule 8 |
| deployment (operational context) | (operational vocabulary) | Rule 4 |
| release-gate (compound) | gap-project-delivery-checklist-001 | Rule 5 |

---

## B38-0C-005-002 — Gap vocabulary behavior

When gap vocabulary appears in input:

Case A: Other predicates also fire
→ CapabilityGapWarning issued for the gap term
→ Other predicates still fire and route normally
→ ResultState = routed (not knowledge_edge)
→ DecisionCount = count of non-gap predicates

Case B: No other predicates fire
→ CapabilityGapWarnings issued for all gap terms
→ PredicateSet = []
→ ResultState = knowledge_edge
→ No routing decisions

---

## B38-0C-005-003 — "monitor" does not fire any predicate (Rule 7)

"monitor" → not QA, not IMPLEMENT, not DOCUMENT, not SECURITY
"monitor" → GAP always

This surprises learners who expect "monitor" to be close to QA (quality monitoring) or IMPLEMENT (monitoring code).
The rule is explicit: rule 7 states monitor is not in any predicate.

---

## B38-0C-005-004 — "execute" in execution context does not fire IMPLEMENT (Rule 8)

IMPLEMENT = code authoring, not running.
"Execute the pipeline" → not IMPLEMENT (executing is not authoring)
"Execute the rollout" → not IMPLEMENT (executing is not authoring)
"Execute the smoke tests" → not TEST or IMPLEMENT

Exception: if "execute" appears in a code authoring context (very rare in B38 scenarios), the surrounding context would need to clearly indicate code authoring. In typical B38 release-gate vocabulary, "execute" always means running an operation, not writing code.

---

## B38-0C-005-005 — "launch" vs "build": a critical gap boundary

"launch" → GAP (gap-project-delivery-launch-001)
"build" → IMPLEMENT (IMPLEMENT lexicon)

"launch the service" → IMPLEMENT (from "service") only; "launch" contributes GAP warning
"build the service" → IMPLEMENT (from both "build" and "service")

A sentence with "launch" alone produces gap warning; no IMPLEMENT from "launch" itself.
A sentence with "build" alone produces IMPLEMENT.

This is non-obvious to learners who expect "launch" (as in launching a product) to be similar to "build" (building a product). In B38 vocabulary, launch = go-live-adjacent (operational), build = implementation (code).
