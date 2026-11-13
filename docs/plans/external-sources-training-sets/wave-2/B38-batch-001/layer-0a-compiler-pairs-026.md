# Layer 0-A — Compiler Pairs (026)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-026-001 — PreReleaseTag: QA scenario

Input: "Verify the pre-release tag format and approve publishing to the staging registry."

Vocabulary analysis:
- "Verify" → QA
- "pre-release tag" → context (release labeling noun)
- "approve" → QA (consolidates)
- "publishing" → context (distribution verb; no predicate)
- "staging registry" → context

PredicateSet: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND TransitionType == "no_contribution"

---

## B38-0A-026-002 — PreReleaseTag with build

Input: "Build the pre-release tagging pipeline and validate each tagged artifact."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "pre-release tagging" → context
- "pipeline" → IMPLEMENT (code artifact context; consolidates)
- "validate" → QA
- "tagged artifact" → context

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-026-003 — RCCandidate: review scenario

Input: "Review the release candidate and collect sign-offs from QA leads."

Vocabulary analysis:
- "Review" → QA
- "release candidate" → context (release artifact noun)
- "collect sign-offs" → QA (sign-off fires QA — rule 23; consolidates)
- "QA leads" → context (people)

PredicateSet: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND DecisionCount == 1

---

## B38-0A-026-004 — RCCandidate with criteria document

Input: "Document the acceptance criteria for the release candidate and sign off on readiness."

Vocabulary analysis:
- "Document" → DOCUMENT
- "acceptance criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "release candidate" → context
- "sign off" → QA (rule 23)
- "readiness" → context

PredicateSet: ["QA","DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","DOCUMENT"] AND TransitionType == "no_contribution"
