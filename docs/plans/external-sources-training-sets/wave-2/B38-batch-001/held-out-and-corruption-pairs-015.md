# Held-Out and Corruption Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-015-001 — HELD-OUT: hotfix approval with service

Input: "Review the hotfix to the authentication service and approve the emergency release."

Correct result:
- "Review" → QA
- "authentication service" → IMPLEMENT (service fires IMPLEMENT — rule 2)
- "approve" → QA (consolidates)
- "hotfix" → no predicate (delivery context noun)
- "emergency release" → context phrase
- PredicateSet: ["QA","IMPLEMENT"]
- TransitionType: feature_addition
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-HO-015-002 — CORRUPTION: "hotfix" wrongly fires IMPLEMENT

Corrupt input processing:
- "hotfix" → IMPLEMENT (WRONG — hotfix is a delivery noun, not a code-authoring term)

Corrupted result (assuming no "service" present):
- PredicateSet: ["QA","IMPLEMENT"] where IMPLEMENT came from "hotfix"
- This produces a false positive: IMPLEMENT fires when it shouldn't

Why this is wrong:
"hotfix" describes the type of change being released (a small urgent fix).
IMPLEMENT requires code-authoring vocabulary: service, build, pipeline.
"hotfix" is not in the IMPLEMENT lexicon.

Diagnostic: If IMPLEMENT fires in a sentence without "service", "build", or "pipeline", check for predicate map error.

---

## B38-HO-015-003 — HELD-OUT: hotfix rollback is knowledge_edge

Input: "Execute the hotfix rollout and monitor the go-live rollout after remediation."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["execute","rollout","monitor","go-live"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND len(CapabilityGapWarnings) >= 3
