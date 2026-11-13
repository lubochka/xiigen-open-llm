# Held-Out and Corruption Pairs (016)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-016-001 — HELD-OUT: release notes documentation

Input: "Document the release notes for v3.2 and have the product owner sign off."

Correct result:
- "Document" → DOCUMENT
- "release notes" → context phrase (notes are the content being documented)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","DOCUMENT"] AND TransitionType == "no_contribution"

---

## B38-HO-016-002 — CORRUPTION: "notes" wrongly fires DOCUMENT via "release notes"

Corrupt logic: "release notes" → DOCUMENT (treating "notes" as documentation vocabulary)
Then "document" also fires DOCUMENT.

This is actually correct — DOCUMENT fires from "document". The corrupted model is getting the right answer but for the wrong reason.

Better corruption example:
"Publish the release notes on the go-live day" → CORRUPT produces ["QA"] (treating "publish" as QA)
Correct result: [] → knowledge_edge
"publish" is not in the QA lexicon. Zero predicates → knowledge_edge.

---

## B38-HO-016-003 — HELD-OUT: publish notes on go-live day

Input: "Publish the release notes on the go-live day and distribute to all stakeholders."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["go-live"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "go-live" in CapabilityGapWarnings
