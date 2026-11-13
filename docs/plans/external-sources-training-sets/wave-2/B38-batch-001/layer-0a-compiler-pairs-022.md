# Layer 0-A — Compiler Pairs (022)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-022-001 — Staged rollout gate: QA fires on "approve"

Input:
"Approve the staged rollout gate after validating each environment is healthy."

Predicate analysis:
- "Approve" → QA
- "validating" → QA (validate fires QA)
- "staged rollout" → GAP (rollout is B38 gap vocabulary; "staged" is context)
- "environment" → context word
- "healthy" → context word

Predicates fired: ["QA"]
CapabilityGapWarnings: ["rollout"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "approve" and "validating" fire QA. "staged rollout" generates a gap warning on "rollout". No IMPLEMENT → no_contribution.

---

## B38-0A-022-002 — Staged rollout with service: QA+IMPLEMENT

Input:
"Verify the recommendation service is stable in each staged rollout phase and sign off."

Predicate analysis:
- "Verify" → QA
- "recommendation service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "staged rollout" → GAP (rollout is gap vocabulary)
- "sign off" → QA (consolidates)
- "stable" → context word

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["rollout"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "recommendation service" fires IMPLEMENT. Two QA terms consolidate. "rollout" gets gap warning. feature_addition because IMPLEMENT is present.

---

## B38-0A-022-003 — Staged rollout criteria: QA+DOCUMENT

Input:
"Validate the staged rollout success criteria before approving the next environment tier."

Predicate analysis:
- "Validate" → QA
- "staged rollout" → GAP (rollout = gap)
- "success criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "approving" → QA (consolidates)
- "environment tier" → context phrase

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["rollout"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "success criteria" fires DOCUMENT. Two QA terms consolidate. "rollout" gets gap warning. No IMPLEMENT → no_contribution.

---

## B38-0A-022-004 — Staged rollout pure gap: no predicates

Input:
"Launch the staged rollout on Friday and monitor each environment tier for issues."

Predicate analysis:
- "Launch" → GAP (B38 gap vocabulary)
- "staged rollout" → GAP (rollout = B38 gap vocabulary)
- "monitor" → GAP (rule 7)
- "environment tier" → context phrase
- "issues" → not in predicate lexicon

Predicates fired: []
CapabilityGapWarnings: ["launch", "rollout", "monitor"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All action terms are gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-022-005 — Staged rollout architecture: QA+ARCHITECTURE boundary note

Input:
"Review the staged rollout architecture design and approve the phased deployment approach."

Predicate analysis:
- "Review" → QA
- "staged rollout" → GAP (rollout = gap)
- "architecture design" → ARCHITECTURE fires (architecture is in ARCHITECTURE lexicon)
- "approve" → QA (consolidates)
- "phased deployment" → GAP (deployment is gap — rule 4)

Predicates fired: ["QA", "ARCHITECTURE"]
CapabilityGapWarnings: ["rollout", "deployment"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 5.x (ARCHITECTURE) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "architecture" fires ARCHITECTURE predicate. "review" and "approve" fire QA. Two gap warnings. ARCHITECTURE does not elevate to feature_addition (only IMPLEMENT does). No_contribution.
