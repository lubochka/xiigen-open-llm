# Layer 0-A — Compiler Pairs (020)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-020-001 — Post-release validation: QA fires

Input:
"Validate the production environment after release and verify the deployment is stable."

Predicate analysis:
- "Validate" → QA (validate fires QA)
- "verify" → QA (consolidates)
- "production environment" → context phrase (no predicate)
- "deployment" → GAP (rule 4: deployment in operational context)
- "stable" → not in any predicate lexicon

Predicates fired: ["QA"]
CapabilityGapWarnings: ["deployment"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "validate" and "verify" fire QA. "deployment" generates a gap warning (not IMPLEMENT — operational context rule 4). No IMPLEMENT → no_contribution.

---

## B38-0A-020-002 — Post-release with service: QA+IMPLEMENT

Input:
"Verify the payment service is functioning correctly after release and approve the post-release sign-off."

Predicate analysis:
- "Verify" → QA
- "payment service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "approve" → QA
- "post-release sign-off" → QA ("sign-off" fires QA; all consolidate)
- "functioning" → context word

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "payment service" fires IMPLEMENT. Three QA terms consolidate. feature_addition because IMPLEMENT is present.

---

## B38-0A-020-003 — Post-release gap: monitor and check only

Input:
"Monitor the production rollout and check for errors after go-live."

Predicate analysis:
- "Monitor" → GAP (rule 7)
- "production rollout" → GAP (rollout is B38 gap vocabulary)
- "check" → not in any predicate lexicon
- "errors" → not in any predicate lexicon
- "go-live" → GAP (B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["monitor", "rollout", "go-live"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All action terms are gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-020-004 — Post-release criteria check: QA+DOCUMENT

Input:
"Validate the post-release acceptance criteria and document the validation results."

Predicate analysis:
- "Validate" → QA
- "post-release acceptance criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "document" → DOCUMENT (consolidates)
- "validation results" → context phrase ("validate" already captured as QA)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "criteria" fires DOCUMENT. "document" also fires DOCUMENT; consolidates. "validate" fires QA. No IMPLEMENT → no_contribution.

---

## B38-0A-020-005 — Post-release audit: QA fires on "audit" (no security)

Input:
"Audit the post-release status and confirm the release gate is officially closed."

Predicate analysis:
- "Audit" → QA (audit in release context; no "security" present → only QA fires)
- "confirm" → not in predicate lexicon
- "release gate" → context phrase
- "officially closed" → context phrase

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "audit" fires QA in release context. No "security" co-occurrence, so SECURITY does not fire. No IMPLEMENT → no_contribution.
