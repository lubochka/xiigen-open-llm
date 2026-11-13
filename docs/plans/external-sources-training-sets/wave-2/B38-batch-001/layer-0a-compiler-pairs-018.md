# Layer 0-A — Compiler Pairs (018)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-018-001 — Production handoff: "sign-off" fires QA

Input:
"Collect sign-off from the operations team for the production handoff."

Predicate analysis:
- "sign-off" → QA (sign-off is in QA lexicon)
- "operations team" → context phrase
- "production handoff" → context phrase (handoff is not a predicate trigger)

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "sign-off" fires QA. "handoff" is an operational delivery term without predicate mapping. No IMPLEMENT → no_contribution.

---

## B38-0A-018-002 — Production handoff with service: QA+IMPLEMENT

Input:
"Verify the payment service handoff to the operations team and approve the go-live confirmation."

Predicate analysis:
- "Verify" → QA
- "payment service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "approve" → QA
- "go-live" → GAP (B38 gap vocabulary)
- "handoff" → context word (not a predicate trigger)

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["go-live"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "payment service" fires IMPLEMENT. QA fires from "verify" and "approve". "go-live" gets a gap warning. feature_addition because IMPLEMENT is present.

---

## B38-0A-018-003 — Handoff documentation: QA+DOCUMENT

Input:
"Document the production handoff criteria and validate that all acceptance criteria are satisfied."

Predicate analysis:
- "Document" → DOCUMENT
- "handoff criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "validate" → QA
- "acceptance criteria" → DOCUMENT (criteria again; all consolidate)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: Three DOCUMENT triggers consolidate into one DOCUMENT predicate. One QA trigger. No IMPLEMENT → no_contribution.

---

## B38-0A-018-004 — Handoff runbook: gap only

Input:
"Execute the handoff runbook and monitor the production environment after cutover."

Predicate analysis:
- "Execute" → GAP (rule 8)
- "handoff runbook" → GAP (runbook is B38 gap vocabulary)
- "monitor" → GAP (rule 7)
- "production environment" → context phrase (no predicate)
- "cutover" → GAP (B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["execute", "runbook", "monitor", "cutover"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All action and noun terms are gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-018-005 — Handoff compliance check: QA fires on "compliance"

Input:
"Confirm compliance with the handoff checklist and get approval from all stakeholders."

Predicate analysis:
- "compliance" → QA (compliance fires QA in release context)
- "handoff checklist" → GAP (checklist is B38 gap vocabulary)
- "approval" → QA (approve fires QA)
- "stakeholders" → context word

Predicates fired: ["QA"]
CapabilityGapWarnings: ["checklist"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "compliance" and "approval" both fire QA and consolidate. "checklist" is gap vocabulary (gets warning). No IMPLEMENT → no_contribution.
