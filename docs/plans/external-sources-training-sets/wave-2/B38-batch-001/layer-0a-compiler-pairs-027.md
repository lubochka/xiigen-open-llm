# Layer 0-A — Compiler Pairs (027)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-027-001 — NightlyBuild: implement scenario

Input: "Build the nightly release pipeline and verify the artifact integrity check passes."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "nightly release pipeline" → IMPLEMENT (pipeline in code artifact context; consolidates)
- "verify" → QA
- "artifact integrity check" → context

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-027-002 — NightlyBuild knowledge edge

Input: "Schedule the nightly build job for midnight UTC."

Vocabulary analysis:
- "Schedule" → not in predicate lexicon
- "nightly build job" → context (scheduling noun; "build" is noun here, not the code-authoring "build" — no predicate fires when it is a pure schedule noun)
- "midnight UTC" → context

PredicateSet: []
CapabilityGapWarnings: []
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-0A-027-003 — WeeklyRelease: QA sign-off

Input: "Collect weekly release sign-offs and validate the build quality report."

Vocabulary analysis:
- "Collect" → not in predicate lexicon alone
- "weekly release sign-offs" → QA (sign-off fires QA — rule 23)
- "validate" → QA (consolidates)
- "build quality report" → QA (quality fires QA; build fires IMPLEMENT — rule 6)
- "build" → IMPLEMENT (rule 6 — absolute)

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-027-004 — WeeklyRelease document

Input: "Document the weekly release process criteria and approval requirements."

Vocabulary analysis:
- "Document" → DOCUMENT
- "weekly release process" → context
- "criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "approval requirements" → context (approval alone could fire QA; "requirements" as noun here is not a QA trigger)

PredicateSet: ["DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["DOCUMENT"] AND TransitionType == "no_contribution"
