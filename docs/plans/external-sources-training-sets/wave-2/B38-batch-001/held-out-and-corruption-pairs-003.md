# Held-Out and Corruption Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-003-001 — HELD-OUT: "deployment criteria" → QA + DOCUMENT

Input: "Review the deployment criteria and validate they are complete."

Correct result:
- "Review" → QA
- "deployment" → GAP (deployment alone is gap even adjacent to "criteria")
- "criteria" → DOCUMENT (criteria fires DOCUMENT; the qualifier "deployment" is gap but criteria still fires)
- "validate" → QA
- PredicateSet: ["QA", "DOCUMENT"]
- CapabilityGapWarnings: ["deployment"]
- TransitionType: no_contribution
- Phase routing: 2.1 + 7.1

Gate B (held-out assertion): "criteria" in DOCUMENT predicate set AND "deployment" in CapabilityGapWarnings

---

## B38-HO-003-002 — CORRUPTION: DOCUMENT suppressed ("criteria" not caught)

Corrupt input processing:
- "Review" → QA ✓
- "deployment" → GAP ✓
- "criteria" → context word only (WRONG — criteria fires DOCUMENT)
- "validate" → QA ✓

Corrupted result:
- PredicateSet: ["QA"] (WRONG — should include DOCUMENT)
- DecisionCount: 1 (WRONG — should be 2)
- Phase routing: 7.1 only (WRONG — should include 2.1)

Why this is wrong:
Rule 3 is absolute: "criteria" → DOCUMENT ALWAYS. The compiler must fire DOCUMENT whenever "criteria" appears regardless of qualifier. "Deployment criteria" does not suppress the criteria → DOCUMENT rule.

Diagnostic: If input contains "criteria" and DOCUMENT is not in PredicateSet, the criteria detection logic is faulty.

---

## B38-HO-003-003 — HELD-OUT variant: "deployment criteria" vs "acceptance criteria"

"Review the deployment criteria" → ["QA","DOCUMENT"] + gap warning on deployment
"Review the acceptance criteria" → ["QA","DOCUMENT"] (no gap warning)
"Review the deployment readiness criteria" → ["QA","DOCUMENT"] + gap warning on deployment

In all three cases, DOCUMENT fires from "criteria". The difference:
- "deployment criteria" has a gap-producing qualifier → gap warning added
- "acceptance criteria" has a QA-adjacent qualifier (no gap vocabulary) → no gap warning
- "deployment readiness criteria" has both → gap warning on "deployment" only

The DOCUMENT predicate fires in all three. Only the gap warning differs.
