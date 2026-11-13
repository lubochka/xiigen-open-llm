# Layer 0-A — Compiler Pairs (019)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-019-001 — Release criteria documentation: "criteria" fires DOCUMENT

Input:
"Document the release criteria for the v4.0 release and have the product manager approve."

Predicate analysis:
- "Document" → DOCUMENT
- "release criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "have the product manager approve" → QA ("approve" fires QA)

Multiple DOCUMENT triggers → one DOCUMENT.
Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "Document" and "release criteria" both trigger DOCUMENT. "approve" fires QA. No IMPLEMENT → no_contribution.

---

## B38-0A-019-002 — Criteria with service: all three predicates

Input:
"Document the release criteria for the inventory service and review the acceptance criteria."

Predicate analysis:
- "Document" → DOCUMENT
- "release criteria" → DOCUMENT (criteria — rule 3)
- "inventory service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "review" → QA
- "acceptance criteria" → DOCUMENT (criteria fires DOCUMENT)

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "service" fires IMPLEMENT. Multiple DOCUMENT triggers consolidate. "review" fires QA. feature_addition because IMPLEMENT is present.

---

## B38-0A-019-003 — Criteria documentation only: DOCUMENT fires without QA

Input:
"Document all exit criteria and readiness criteria for the production release."

Predicate analysis:
- "Document" → DOCUMENT
- "exit criteria" → DOCUMENT (criteria — rule 3)
- "readiness criteria" → DOCUMENT (criteria — rule 3, again consolidates)
- "production release" → context phrase (no predicate)

Predicates fired: ["DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 2.1 (DOCUMENT)
DomainId: software-development

Reasoning: Three DOCUMENT triggers consolidate into one DOCUMENT predicate. No QA terms ("document" alone does not fire QA). No IMPLEMENT → no_contribution.

---

## B38-0A-019-004 — Criteria + security audit: QA+DOCUMENT+SECURITY

Input:
"Audit the security criteria before documenting the final release criteria for sign-off."

Predicate analysis:
- "Audit" → QA (release context) + SECURITY co-fires when "security" is present (rule 19)
- "security" → SECURITY
- "criteria" (in "security criteria") → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "documenting" → DOCUMENT
- "release criteria" → DOCUMENT (consolidates)
- "sign-off" → QA (consolidates with audit into one QA predicate)

Predicates fired: ["QA", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 3
Phase routing: 2.1 (DOCUMENT) + 3.4 (SECURITY) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "audit" + "security" co-fire both QA and SECURITY (rule 19). "criteria" fires DOCUMENT three times; consolidates. "sign-off" consolidates into QA. No IMPLEMENT → no_contribution.

---

## B38-0A-019-005 — Criteria gap: "roll out" without predicates

Input:
"Roll out the criteria document and schedule the go-live checklist review."

Predicate analysis:
- "Roll out" → GAP (rollout variant is B38 gap vocabulary)
- "criteria document" → DOCUMENT fires! ("criteria" fires DOCUMENT — rule 3; "document" also fires DOCUMENT)
- "schedule" → not in any predicate lexicon (scheduling context, not code)
- "go-live" → GAP (B38 gap vocabulary)
- "checklist" → GAP (B38 gap vocabulary)
- "review" → QA (review fires QA even in gap context!)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["rollout", "go-live", "checklist"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "criteria" and "document" fire DOCUMENT. "review" fires QA despite surrounding gap vocabulary (QA is never suppressed by gap terms — v6-01 corruption lesson). Three gap warnings. No IMPLEMENT → no_contribution.
