# Layer 0-A — Compiler Pairs (035)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-035-001 — HotfixTag: architecture review scenario

Input: "Review the hotfix tagging architecture and approve the emergency release process design."

Vocabulary analysis:
- "Review" → QA
- "hotfix tagging architecture" → ARCHITECTURE (architecture fires ARCHITECTURE)
- "approve" → QA (consolidates)
- "emergency release process design" → ARCHITECTURE (design in architecture context; consolidates)

PredicateSet: ["QA","ARCHITECTURE"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","ARCHITECTURE"] AND TransitionType == "no_contribution"

---

## B38-0A-035-002 — PatchRelease: build + security

Input: "Build the patch release service and audit the security of the patch before sign-off."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "patch release service" → IMPLEMENT (rule 2 — absolute; consolidates)
- "audit" → QA + SECURITY co-fires (rule 19: "security" co-occurs)
- "security of the patch" → SECURITY
- "sign-off" → QA (rule 23; consolidates)

PredicateSet: ["QA","IMPLEMENT","SECURITY"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3

Gate B: PredicateSet == ["QA","IMPLEMENT","SECURITY"] AND TransitionType == "feature_addition"

---

## B38-0A-035-003 — SemanticVersioning: no predicate scenario

Input: "Finalize the semantic version number for Q3 release."

Vocabulary analysis:
- "Finalize" → not in predicate lexicon (operational completion verb)
- "semantic version number" → context
- "Q3 release" → context

PredicateSet: []
CapabilityGapWarnings: []
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-0A-035-004 — RCCandidate: build + document + security + QA

Input: "Build the RC validation service, audit security compliance, document the acceptance criteria, and approve the candidate."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "RC validation service" → IMPLEMENT (rule 2 — absolute; consolidates)
- "audit" → QA + SECURITY co-fires (rule 19: "security" co-occurs)
- "security compliance" → SECURITY; "compliance" also fires QA (consolidates)
- "document" → DOCUMENT
- "acceptance criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "approve" → QA (consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 4

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT","SECURITY"] AND DecisionCount == 4 AND TransitionType == "feature_addition"
