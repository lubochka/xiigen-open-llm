# Layer 0-A — Compiler Pairs (029)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-029-001 — HotfixTag: QA scenario

Input: "Review the hotfix tag and approve the emergency release."

Vocabulary analysis:
- "Review" → QA
- "hotfix tag" → context (delivery labeling noun)
- "approve" → QA (consolidates)
- "emergency release" → context

PredicateSet: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND TransitionType == "no_contribution"

---

## B38-0A-029-002 — HotfixTag with build and service

Input: "Build the hotfix for the payment service, tag the release, and verify the patch."

Vocabulary analysis:
- "Build" → IMPLEMENT (rule 6 — absolute)
- "hotfix" → context
- "payment service" → IMPLEMENT (rule 2 — absolute; consolidates)
- "tag the release" → context (tagging action; no predicate)
- "verify" → QA

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-029-003 — PatchRelease: QA+DOCUMENT scenario

Input: "Document the patch release notes and sign off on the maintenance window."

Vocabulary analysis:
- "Document" → DOCUMENT
- "patch release notes" → context
- "sign off" → QA (rule 23)
- "maintenance window" → context (operational noun; no predicate)

PredicateSet: ["QA","DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","DOCUMENT"] AND TransitionType == "no_contribution"

---

## B38-0A-029-004 — PatchRelease with security

Input: "Audit the security implications of the patch release and validate the vulnerability remediation."

Vocabulary analysis:
- "Audit" → QA + SECURITY co-fires (rule 19: "security" co-occurs)
- "security implications" → SECURITY ("security" fires SECURITY)
- "patch release" → context
- "validate" → QA (consolidates)
- "vulnerability remediation" → SECURITY (vulnerability fires SECURITY; consolidates); "remediation" → context

PredicateSet: ["QA","SECURITY"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","SECURITY"] AND TransitionType == "no_contribution"
