# Layer 0-A — Compiler Pairs (025)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-025-001 — SemanticVersioning: predicate analysis

Input: "Review the semantic versioning scheme and approve the version bump strategy."

Vocabulary analysis:
- "Review" → QA
- "semantic versioning" → context (versioning concept; no predicate)
- "approve" → QA (consolidates)
- "version bump" → context (version management noun; no predicate)
- "strategy" → context

PredicateSet: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND TransitionType == "no_contribution"

---

## B38-0A-025-002 — SemanticVersioning with implementation

Input: "Build the automated semantic version bump service and validate the output."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "automated" → context
- "semantic version bump" → context
- "service" → IMPLEMENT (rule 2 — absolute; consolidates with build)
- "validate" → QA

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-025-003 — CalendarVersioning: no-predicate scenario

Input: "Plan the calendar versioning scheme for Q2 releases."

Vocabulary analysis:
- "Plan" → not in predicate lexicon
- "calendar versioning" → context (versioning model noun; no predicate)
- "Q2 releases" → context (time reference)

PredicateSet: []
CapabilityGapWarnings: []
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-0A-025-004 — SemVerBump with document

Input: "Document the SemVer bump criteria for patch, minor, and major releases."

Vocabulary analysis:
- "Document" → DOCUMENT
- "SemVer bump" → context (versioning noun)
- "criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "patch / minor / major" → context

PredicateSet: ["DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["DOCUMENT"] AND TransitionType == "no_contribution"
