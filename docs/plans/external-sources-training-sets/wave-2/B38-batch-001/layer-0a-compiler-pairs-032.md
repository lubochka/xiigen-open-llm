# Layer 0-A — Compiler Pairs (032)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-032-001 — SemVerBump: build and validate

Input: "Build the SemVer bump automation script and validate it increments versions correctly."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "SemVer bump automation script" → context (script noun; "build" already fires IMPLEMENT)
- "validate" → QA
- "increments versions correctly" → context

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-032-002 — PreReleaseTag with security

Input: "Audit the security of the pre-release tag signing process and verify the artifact signatures."

Vocabulary analysis:
- "Audit" → QA + SECURITY co-fires (rule 19: "security" co-occurs)
- "security of the pre-release tag signing" → SECURITY
- "pre-release tag" → context
- "verify" → QA (consolidates)
- "artifact signatures" → context

PredicateSet: ["QA","SECURITY"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","SECURITY"] AND TransitionType == "no_contribution"

---

## B38-0A-032-003 — CalendarVersioning: implement scenario

Input: "Build the CalVer version generator service and review the output format."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "CalVer version generator" → context
- "service" → IMPLEMENT (rule 2 — absolute; consolidates)
- "review" → QA

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-032-004 — QuarterlyRelease sign-off only

Input: "Collect quarterly release sign-offs from all stakeholders and confirm approval."

Vocabulary analysis:
- "Collect" → not in predicate lexicon
- "quarterly release sign-offs" → QA (sign-off fires QA — rule 23)
- "stakeholders" → context (people)
- "confirm approval" → QA ("approval" fires QA; consolidates)

PredicateSet: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND TransitionType == "no_contribution"
