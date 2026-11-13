# Layer 0-A — Compiler Pairs (013)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-013-001 — Environment promotion: "verify" fires QA; "environment" is context

Input:
"Verify that the staging environment meets all promotion criteria before approving the move to production."

Predicate analysis:
- "Verify" → QA (verify is in QA lexicon)
- "approved" → QA (approve fires QA)
- "promotion criteria" → DOCUMENT (criteria fires DOCUMENT always — rule 3)
- "staging environment" → not IMPLEMENT (environment is a deployment context word, not code)
- "production" → context word

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "verify" and "approving" fire QA. "promotion criteria" fires DOCUMENT via "criteria" (rule 3). "environment" is infrastructure context, not code authoring. No IMPLEMENT → no_contribution.

---

## B38-0A-013-002 — Environment promotion with service: IMPLEMENT fires

Input:
"Approve the auth service promotion from staging to production after verifying the gate criteria."

Predicate analysis:
- "Approve" → QA
- "auth service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "verifying" → QA
- "gate criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "promotion" → context word
- "staging" / "production" → context words

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition (IMPLEMENT present)
DecisionCount: 3
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: Three predicates from one input. "service" absolutely fires IMPLEMENT. "criteria" fires DOCUMENT. Both QA terms fire and consolidate. TransitionType = feature_addition because IMPLEMENT is present.

---

## B38-0A-013-003 — Environment promotion gap: "deploy to prod" is all gap

Input:
"Execute the environment promotion runbook to deploy the build to prod."

Predicate analysis:
- "Execute" → GAP (rule 8 — execute in execution context is not IMPLEMENT)
- "environment promotion" → context phrase (no predicate)
- "runbook" → GAP (B38 gap vocabulary)
- "deploy" → GAP (rule 4 — deployment operational context)
- "build" → IMPLEMENT (rule 6: build fires IMPLEMENT always!)

Correction: "build" fires IMPLEMENT.
Predicates fired: ["IMPLEMENT"]
CapabilityGapWarnings: ["execute", "runbook", "deploy"]
TransitionType: feature_addition
DecisionCount: 1
Phase routing: 4.1 (IMPLEMENT)
DomainId: software-development

Reasoning: "build" is the only predicate trigger. It fires IMPLEMENT from rule 6 regardless of the surrounding operational vocabulary. Execute, runbook, and deploy are all gap vocabulary.

---

## B38-0A-013-004 — Promotion criteria review: QA+DOCUMENT only

Input:
"Review the environment promotion criteria and validate all acceptance criteria are documented."

Predicate analysis:
- "Review" → QA
- "validate" → QA
- "promotion criteria" → DOCUMENT (criteria fires DOCUMENT)
- "acceptance criteria" → DOCUMENT (acceptance criteria: "criteria" fires DOCUMENT)
- "documented" → DOCUMENT (document/documentation fires DOCUMENT)

Multiple DOCUMENT triggers → single DOCUMENT predicate.
Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: Two QA triggers consolidate into one QA predicate. Three DOCUMENT triggers consolidate into one DOCUMENT predicate. No IMPLEMENT → no_contribution.

---

## B38-0A-013-005 — Pure promotion gap: no predicates

Input:
"Run the environment cutover and monitor the promotion rollout."

Predicate analysis:
- "Run" → not IMPLEMENT (execution context, not code authoring)
- "environment cutover" → GAP (cutover is gap vocabulary)
- "monitor" → GAP (rule 7)
- "promotion rollout" → GAP (rollout is B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["cutover", "monitor", "rollout"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All terms are gap vocabulary. Zero predicates → knowledge_edge (not GapHandler; not SCHEDULE_CAPABILITY_LEARNING — rule 18).
