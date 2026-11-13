# Layer 0-A — Compiler Pairs (021)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-021-001 — Incident response release: QA fires on "review"

Input:
"Review the incident response for the release failure and approve the remediation plan."

Predicate analysis:
- "Review" → QA
- "approve" → QA (consolidates)
- "incident response" → context phrase (not a predicate trigger)
- "release failure" → context phrase
- "remediation plan" → context phrase

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "review" and "approve" fire QA. No service/build/pipeline vocabulary → no IMPLEMENT. No "criteria"/"document" → no DOCUMENT. TransitionType = no_contribution.

---

## B38-0A-021-002 — Incident release with service: QA+IMPLEMENT

Input:
"Verify the incident-related fix to the auth service and sign off on the emergency release."

Predicate analysis:
- "Verify" → QA
- "auth service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "sign off" → QA (consolidates)
- "emergency release" → context phrase
- "incident-related fix" → context phrase (fix is not in IMPLEMENT lexicon)

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "auth service" fires IMPLEMENT. Two QA terms consolidate. feature_addition because IMPLEMENT is present.

---

## B38-0A-021-003 — Incident criteria documentation: QA+DOCUMENT

Input:
"Document the incident resolution criteria and validate that all acceptance criteria are closed."

Predicate analysis:
- "Document" → DOCUMENT
- "incident resolution criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "validate" → QA
- "acceptance criteria" → DOCUMENT (criteria fires DOCUMENT, consolidates)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: Multiple DOCUMENT triggers (document + two criteria occurrences) consolidate. "validate" fires QA. No IMPLEMENT → no_contribution.

---

## B38-0A-021-004 — Incident rollback gap: knowledge_edge

Input:
"Execute the incident rollback and monitor the go-live rollout after remediation."

Predicate analysis:
- "Execute" → GAP (rule 8)
- "incident rollback" → GAP (rollback has no predicate)
- "monitor" → GAP (rule 7)
- "go-live" → GAP (B38 gap vocabulary)
- "rollout" → GAP (B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["execute", "rollback", "monitor", "go-live", "rollout"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All terms are gap vocabulary or non-predicate terms. Zero predicates → knowledge_edge.

---

## B38-0A-021-005 — Incident full: QA+IMPLEMENT+DOCUMENT

Input:
"Review the incident service fix, document the release criteria for the re-release, and approve the hotfix deployment."

Predicate analysis:
- "Review" → QA
- "incident service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "document" → DOCUMENT
- "release criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3; consolidates)
- "approve" → QA (consolidates)
- "hotfix deployment" → GAP for deployment (rule 4)

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: feature_addition
DecisionCount: 3
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "service" fires IMPLEMENT. "document" + "criteria" fire DOCUMENT. Multiple QA terms consolidate. "deployment" gets gap warning. feature_addition because IMPLEMENT is present.
