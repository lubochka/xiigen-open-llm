# Held-Out and Corruption Pairs (019)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-019-001 — HELD-OUT: document alone fires DOCUMENT (no QA)

Input: "Document all exit criteria and readiness criteria for the production release."

Correct result:
- "Document" → DOCUMENT
- "exit criteria" → DOCUMENT (criteria — rule 3)
- "readiness criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "production release" → context
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution
- DecisionCount: 1

Gate B assertion: PredicateSet == ["DOCUMENT"] AND QA NOT in PredicateSet

---

## B38-HO-019-002 — CORRUPTION: "document" wrongly fires QA

Corrupt input processing:
- "Document" → QA (WRONG — "document" is a DOCUMENT trigger, not a QA trigger)

Corrupted result:
- PredicateSet: ["QA","DOCUMENT"] (WRONG — QA should not fire from "document")
- DecisionCount: 2 (WRONG)
- TransitionType: no_contribution (coincidentally correct but for wrong reasons)

Why this is wrong:
"Document" fires DOCUMENT. It does not fire QA.
QA fires from: review, approve, verify, validate, sign-off, audit, compliance, inspect, acceptance, quality.
"Document" as a verb is not in the QA lexicon.

---

## B38-HO-019-003 — HELD-OUT: security criteria audit — three predicates

Input: "Audit the security criteria before documenting the final release criteria for sign-off."

Correct result:
- "Audit" → QA (release context) + SECURITY co-fires (rule 19: security+audit)
- "security" → SECURITY
- "criteria" (in "security criteria") → DOCUMENT
- "documenting" → DOCUMENT (consolidates)
- "release criteria" → DOCUMENT (consolidates)
- "sign-off" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution (no IMPLEMENT)
- DecisionCount: 3

Gate B assertion: PredicateSet == ["QA","DOCUMENT","SECURITY"] AND TransitionType == "no_contribution"
