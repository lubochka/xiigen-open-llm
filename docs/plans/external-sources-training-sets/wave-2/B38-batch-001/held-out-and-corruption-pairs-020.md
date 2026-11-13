# Held-Out and Corruption Pairs (020)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-020-001 — HELD-OUT: post-release validation with deployment gap

Input: "Validate the production environment after release and verify the deployment is stable."

Correct result:
- "Validate" → QA
- "verify" → QA (consolidates)
- "production environment" → context
- "deployment" → GAP warning (rule 4: deployment is operational context)
- "stable" → context
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["deployment"]
- TransitionType: no_contribution
- DecisionCount: 1

Gate B assertion: PredicateSet == ["QA"] AND "deployment" in CapabilityGapWarnings

---

## B38-HO-020-002 — CORRUPTION: "deployment" wrongly fires IMPLEMENT

Corrupt input processing:
- "deployment" → IMPLEMENT (WRONG — rule 4: deployment is operational context)

Corrupted result:
- PredicateSet: ["QA","IMPLEMENT"] (WRONG)
- TransitionType: feature_addition (WRONG)

Why this is wrong:
"deployment" refers to the act of running code in a target environment.
Running code is operational; writing code is IMPLEMENT.
Rule 4 explicitly states: "deployment" alone in operational context does not fire IMPLEMENT.

The only exception: if "deployment pipeline" or "deployment service" appears, "pipeline" or "service" fires IMPLEMENT — not "deployment".

---

## B38-HO-020-003 — HELD-OUT: monitor rollout is knowledge_edge

Input: "Monitor the production rollout and check for errors after go-live."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["monitor","rollout","go-live"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "monitor" in CapabilityGapWarnings
