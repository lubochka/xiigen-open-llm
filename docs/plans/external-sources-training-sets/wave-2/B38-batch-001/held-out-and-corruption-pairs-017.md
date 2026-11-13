# Held-Out and Corruption Pairs (017)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-017-001 — HELD-OUT: build version tag fires IMPLEMENT

Input: "Verify the build version tag matches the release candidate and approve the final tag."

Correct result:
- "Verify" → QA
- "build" → IMPLEMENT (rule 6 — even in "build version tag")
- "approve" → QA (consolidates)
- "version tag" → context (no predicate)
- "release candidate" → context
- PredicateSet: ["QA","IMPLEMENT"]
- TransitionType: feature_addition
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","IMPLEMENT"] AND "build" is identified as IMPLEMENT trigger

---

## B38-HO-017-002 — CORRUPTION: "version" or "tag" wrongly fires IMPLEMENT

Corrupt input processing:
- "version" → IMPLEMENT (WRONG — version numbering is not code authoring)
- OR "tag" → IMPLEMENT (WRONG — git tags describe code artifacts but "tag" alone is not in IMPLEMENT lexicon)

Corrupted result:
- PredicateSet: ["QA","IMPLEMENT"] where IMPLEMENT came from "version" or "tag"
- This produces a false positive when "build" is absent

Why this is wrong:
"version" describes a software release number. Not code authoring.
"tag" describes a label. Not code authoring.
Only "build" (or "service", "pipeline") reliably fires IMPLEMENT.

Test: "Verify the version tag matches the release candidate" (no "build")
Correct: ["QA"] only — IMPLEMENT should NOT fire without "build".

---

## B38-HO-017-003 — HELD-OUT: version tag rollout is knowledge_edge

Input: "Rollout the version tag to all environments on go-live day."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["rollout","go-live"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "rollout" in CapabilityGapWarnings AND "go-live" in CapabilityGapWarnings
