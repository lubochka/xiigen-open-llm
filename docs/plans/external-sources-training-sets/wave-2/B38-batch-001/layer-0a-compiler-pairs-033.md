# Layer 0-A — Compiler Pairs (033)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-033-001 — MonthlyRelease: architecture review

Input: "Review the monthly release architecture design and approve the versioning strategy."

Vocabulary analysis:
- "Review" → QA
- "monthly release architecture design" → ARCHITECTURE (architecture fires ARCHITECTURE)
- "approve" → QA (consolidates)
- "versioning strategy" → context

PredicateSet: ["QA","ARCHITECTURE"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2

Gate B: PredicateSet == ["QA","ARCHITECTURE"] AND TransitionType == "no_contribution"

---

## B38-0A-033-002 — PatchRelease: full five predicates

Input: "Review the payment service patch, audit security vulnerabilities, document release criteria, evaluate the versioning architecture, and approve."

Vocabulary analysis:
- "Review" → QA
- "payment service" → IMPLEMENT (rule 2 — absolute)
- "patch" → context
- "audit" → QA (consolidates) + SECURITY co-fires (rule 19)
- "security vulnerabilities" → SECURITY (both "security" and "vulnerability" fire SECURITY)
- "document" → DOCUMENT
- "release criteria" → DOCUMENT (rule 3 — absolute; consolidates)
- "versioning architecture" → ARCHITECTURE
- "approve" → QA (consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 5

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] AND DecisionCount == 5 AND TransitionType == "feature_addition"

---

## B38-0A-033-003 — NightlyBuild knowledge edge with multiple gaps

Input: "Configure the nightly rollout schedule and plan the cutover window."

Vocabulary analysis:
- "Configure" → not in predicate lexicon
- "nightly rollout schedule" → GAP: "rollout" fires gap-project-delivery-rollout-golive-001
- "plan" → not in predicate lexicon
- "cutover window" → GAP: "cutover" fires gap-project-delivery-cutover-001

PredicateSet: []
CapabilityGapWarnings: ["rollout","cutover"]
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND "rollout" in CapabilityGapWarnings AND "cutover" in CapabilityGapWarnings

---

## B38-0A-033-004 — WeeklyRelease: service + document

Input: "Review the notification service weekly release and document the readiness criteria."

Vocabulary analysis:
- "Review" → QA
- "notification service" → IMPLEMENT (rule 2 — absolute)
- "weekly release" → context
- "document" → DOCUMENT
- "readiness criteria" → DOCUMENT (rule 3 — absolute; consolidates)

PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3

Gate B: PredicateSet == ["QA","IMPLEMENT","DOCUMENT"] AND TransitionType == "feature_addition"
