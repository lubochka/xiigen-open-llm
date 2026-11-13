# Layer 0-A — Compiler Pairs (031)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-031-001 — HotfixTag with full predicate set

Input: "Review the auth service hotfix, audit the security controls applied, document the patch criteria, and approve the release."

Vocabulary analysis:
- "Review" → QA
- "auth service" → IMPLEMENT (rule 2 — absolute)
- "hotfix" → context
- "audit" → QA (consolidates) + SECURITY co-fires (rule 19: "security" co-occurs)
- "security controls" → SECURITY
- "document" → DOCUMENT
- "patch criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "approve" → QA (consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 4

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT","SECURITY"] AND DecisionCount == 4 AND TransitionType == "feature_addition"

---

## B38-0A-031-002 — NightlyBuild: document scenario

Input: "Document the nightly build criteria and quality thresholds for artifact promotion."

Vocabulary analysis:
- "Document" → DOCUMENT
- "nightly build criteria" → DOCUMENT (rule 3 — absolute; consolidates; "criteria" fires independently of "build" here)
- "quality thresholds" → context
- "artifact promotion" → context

PredicateSet: ["DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["DOCUMENT"] AND TransitionType == "no_contribution"

---

## B38-0A-031-003 — WeeklyRelease with gap vocabulary

Input: "Validate the weekly release gate and approve the rollout to production."

Vocabulary analysis:
- "Validate" → QA
- "weekly release gate" → context
- "approve" → QA (consolidates)
- "rollout" → GAP (gap-project-delivery-rollout-golive-001)
- "production" → context

PredicateSet: ["QA"]
CapabilityGapWarnings: ["rollout"]
TransitionType: no_contribution
DecisionCount: 1

Gate B: PredicateSet == ["QA"] AND "rollout" in CapabilityGapWarnings AND TransitionType == "no_contribution"

---

## B38-0A-031-004 — RCCandidate with full QA lexicon

Input: "Inspect the RC build quality, verify acceptance criteria, and collect compliance sign-offs."

Vocabulary analysis:
- "Inspect" → QA
- "RC build" → IMPLEMENT (rule 6 — "build" fires IMPLEMENT; RC is context)
- "quality" → QA (consolidates)
- "verify" → QA (consolidates)
- "acceptance criteria" → DOCUMENT (rule 3 — absolute)
- "compliance sign-offs" → QA (sign-off fires QA — rule 23; compliance fires QA; consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT"] AND TransitionType == "feature_addition" AND DecisionCount == 3
