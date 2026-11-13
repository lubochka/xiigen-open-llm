# Layer 0-A — Compiler Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-003-001 — "criteria" fires DOCUMENT; "deployment" is gap; QA from review+validate

Input:
"Review the deployment readiness criteria and validate the production environment is ready."

Predicate analysis:
- "Review" → QA (review is in QA lexicon)
- "deployment" → GAP (deployment alone in operational context does not fire IMPLEMENT)
- "criteria" → DOCUMENT (criteria fires DOCUMENT always, including "deployment readiness criteria")
- "validate" → QA
- "production" → context word

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: no_contribution (no IMPLEMENT present; DOCUMENT routes to Phase 2, not contribution)
DecisionCount: 2
Phase routing: 7.1 (QA) + 2.1 (DOCUMENT)
DomainId: software-development

Reasoning: Two predicates fire independently. "criteria" fires DOCUMENT per B38 rule 3 (DOCUMENT ALWAYS). "Review" and "validate" fire QA per rule 1. "deployment" alone in operational context is gap per rule 4.

---

## B38-0A-003-002 — "exit criteria" fires DOCUMENT (same as release/acceptance criteria)

Input:
"Validate the exit criteria for the sprint and confirm quality gates are passed."

Predicate analysis:
- "Validate" → QA
- "exit criteria" → DOCUMENT (criteria fires DOCUMENT regardless of qualifier — exit, release, acceptance all apply)
- "quality" → QA (quality is in QA lexicon)
- "gates" → context word (does not fire on its own)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: "exit criteria" triggers the DOCUMENT rule because "criteria" is the operative word. The qualifier "exit" does not change the outcome. "Validate" and "quality" both fire QA.

---

## B38-0A-003-003 — "readiness criteria" fires DOCUMENT; "checklist" is gap

Input:
"Complete the readiness criteria review and work through the deployment checklist."

Predicate analysis:
- "readiness criteria" → DOCUMENT (criteria fires DOCUMENT)
- "review" → QA
- "deployment" → GAP
- "checklist" → GAP (checklist is B38 gap vocabulary)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["deployment", "checklist"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: Both gap terms ("deployment" and "checklist") produce CapabilityGapWarnings. Two predicates still fire. "checklist" is explicitly listed as B38 gap vocabulary.

---

## B38-0A-003-004 — "criteria" in quality gate context still fires DOCUMENT

Input:
"The release manager must review and approve if all quality criteria are satisfied."

Predicate analysis:
- "review" → QA
- "approve" → QA
- "quality" → QA
- "criteria" → DOCUMENT

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: Three QA signals ("review", "approve", "quality") consolidate into a single QA predicate. "criteria" fires independently as DOCUMENT. QA ordering rule: QA is last in pipeline even with only two predicates.

---

## B38-0A-003-005 — "deployment" is gap; criteria still fires DOCUMENT

Input:
"Define the acceptance criteria for deployment and validate the test environment."

Predicate analysis:
- "acceptance criteria" → DOCUMENT (criteria fires DOCUMENT)
- "deployment" → GAP
- "validate" → QA
- "test environment" → context phrase (does not fire TEST without explicit test creation action)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: "acceptance criteria" is the canonical example of DOCUMENT firing on criteria. "deployment" in operational context remains gap. "test environment" reference does not trigger TEST predicate because no test authoring action is described.
