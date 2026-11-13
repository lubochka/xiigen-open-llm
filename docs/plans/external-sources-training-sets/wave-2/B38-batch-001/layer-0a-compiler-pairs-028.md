# Layer 0-A — Compiler Pairs (028)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-028-001 — MonthlyRelease: full gate scenario

Input: "Review the monthly release package, validate the exit criteria, and approve deployment to production."

Vocabulary analysis:
- "Review" → QA
- "monthly release package" → context
- "validate" → QA (consolidates)
- "exit criteria" → DOCUMENT (rule 3 — absolute)
- "approve" → QA (consolidates)
- "deployment to production" → GAP (rule 4 — operational deployment)

PredicateSet: ["QA","DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","DOCUMENT"] AND "deployment" in CapabilityGapWarnings AND TransitionType == "no_contribution"

---

## B38-0A-028-002 — MonthlyRelease with service

Input: "Review the payment service monthly release and sign off on the release gate."

Vocabulary analysis:
- "Review" → QA
- "payment service" → IMPLEMENT (rule 2 — absolute)
- "monthly release" → context
- "sign off" → QA (rule 23; consolidates)
- "release gate" → context

PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2

Gate B: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-0A-028-003 — QuarterlyRelease: full predicate set

Input: "Review the auth service quarterly release, audit the security posture, document the release criteria, and approve."

Vocabulary analysis:
- "Review" → QA
- "auth service" → IMPLEMENT (rule 2 — absolute)
- "quarterly release" → context
- "audit" → QA (consolidates) + SECURITY co-fires (rule 19: security co-occurs)
- "security posture" → SECURITY ("security" fires SECURITY)
- "document" → DOCUMENT
- "release criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "approve" → QA (consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 4

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT","SECURITY"] AND DecisionCount == 4 AND TransitionType == "feature_addition"

---

## B38-0A-028-004 — QuarterlyRelease knowledge edge

Input: "Plan the quarterly release timeline and cutover window."

Vocabulary analysis:
- "Plan" → not in predicate lexicon
- "quarterly release timeline" → context
- "cutover window" → GAP (gap-project-delivery-cutover-001)

PredicateSet: []
CapabilityGapWarnings: ["cutover"]
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND "cutover" in CapabilityGapWarnings AND ResultState == "knowledge_edge"
