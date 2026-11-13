# Held-Out and Corruption Pairs (021)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-021-001 — HELD-OUT: incident service fix fires IMPLEMENT

Input: "Verify the incident-related fix to the auth service and sign off on the emergency release."

Correct result:
- "Verify" → QA
- "auth service" → IMPLEMENT (service fires IMPLEMENT — rule 2)
- "sign off" → QA (consolidates)
- "incident-related fix" → context ("fix" is not IMPLEMENT)
- "emergency release" → context
- PredicateSet: ["QA","IMPLEMENT"]
- TransitionType: feature_addition
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-HO-021-002 — CORRUPTION: "fix" wrongly fires IMPLEMENT

Corrupt input processing:
- "fix" → IMPLEMENT (WRONG — "fix" in B38 release vocabulary is a delivery noun, not code authoring)

In the sentence: "Review the incident response for the release failure and approve the remediation plan."
Corrupt: PredicateSet: ["QA","IMPLEMENT"] — "fix" or "remediation" → IMPLEMENT
Correct: PredicateSet: ["QA"] — no IMPLEMENT vocabulary present

Why this is wrong:
"fix" and "remediation" describe the type of change but are not code-authoring vocabulary.
IMPLEMENT fires from: service, build, pipeline, implementation, code.
Neither "fix" nor "remediation" is in the IMPLEMENT lexicon.

---

## B38-HO-021-003 — HELD-OUT: incident rollback knowledge_edge

Input: "Execute the incident rollback and monitor the go-live rollout after remediation."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["execute","rollback","monitor","go-live","rollout"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND len(CapabilityGapWarnings) >= 4
