# Layer 0-A — Compiler Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-009-001 — Full four-predicate: QA+IMPLEMENT+DOCUMENT+SECURITY

Input:
"Review the payment service deployment, validate the release criteria, audit the security controls, and approve the go-live."

Predicate analysis:
- "Review" → QA
- "payment service" → IMPLEMENT (service fires IMPLEMENT always)
- "deployment" → GAP
- "validate" → QA
- "release criteria" → DOCUMENT (criteria fires DOCUMENT)
- "audit" → QA (release context) + SECURITY co-fires because "security controls" follows
- "security" → SECURITY
- "controls" → SECURITY domain context
- "approve" → QA
- "go-live" → GAP

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: ["deployment", "go-live"]
TransitionType: feature_addition (IMPLEMENT present)
DecisionCount: 4
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 3.4 (SECURITY) + 7.1 (QA — last)
DomainId: software-development

Reasoning: This is the full four-predicate scenario. Each predicate fires from a distinct set of terms. QA ordering rule (absolute): QA is last regardless of order terms appear in input. "deployment" and "go-live" both produce gap warnings. IMPLEMENT presence gives feature_addition TransitionType.

---

## B38-0A-009-002 — Ordering enforcement: IMPLEMENT first, DOCUMENT second, SECURITY third, QA last

Input:
"Build the release service module, document the deployment criteria, audit the security scan, and review for approval."

Predicate analysis:
- "Build" → IMPLEMENT
- "service" → IMPLEMENT (consolidates)
- "module" → context
- "document" → DOCUMENT
- "deployment" → GAP
- "criteria" → DOCUMENT (consolidates)
- "audit" → QA + SECURITY context
- "security" → SECURITY
- "review" → QA
- "approval" → QA

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: ["deployment"]
TransitionType: feature_addition
DecisionCount: 4
Phase routing: 4.1 + 2.1 + 3.4 + 7.1 (QA last, absolute)
DomainId: software-development

Reasoning: Even though "review for approval" appears last in the input, QA is still last in the predicate pipeline per rule 12 (QA ordering rule absolute). Build/service fire IMPLEMENT; document/criteria fire DOCUMENT; audit/security fire SECURITY; review/approval/audit fire QA.

---

## B38-0A-009-003 — Four predicates with zero gap (all terms covered)

Input:
"Verify the checkout service implementation, validate the acceptance criteria, inspect security vulnerability coverage, and sign off."

Predicate analysis:
- "Verify" → QA
- "checkout service" → IMPLEMENT
- "implementation" → IMPLEMENT context
- "validate" → QA
- "acceptance criteria" → DOCUMENT
- "inspect" → QA
- "security" → SECURITY
- "vulnerability" → SECURITY
- "sign off" → QA

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 4
Phase routing: 4.1 + 2.1 + 3.4 + 7.1
DomainId: software-development

Reasoning: Clean four-predicate result with no gap warnings. All vocabulary resolves to named predicates.

---

## B38-0A-009-004 — Four predicates; two gap warnings; QA last confirmed

Input:
"Approve the go-live for the order service after reviewing deployment readiness criteria and completing the security audit."

Predicate analysis:
- "Approve" → QA
- "go-live" → GAP
- "order service" → IMPLEMENT
- "reviewing" → QA
- "deployment" → GAP
- "readiness criteria" → DOCUMENT (criteria)
- "security" → SECURITY
- "audit" → QA + SECURITY context

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: ["go-live", "deployment"]
TransitionType: feature_addition
DecisionCount: 4
Phase routing: 4.1 + 2.1 + 3.4 + 7.1
DomainId: software-development

Reasoning: "go-live" and "deployment" produce gap warnings. All four predicates still fire. QA is last in routing order. This is the canonical full-predicate B38 scenario with gap co-occurrence.

---

## B38-0A-009-005 — Distinguishing four-predicate from three-predicate (SECURITY absent without security term)

Input:
"Review the billing service build, validate the release criteria, and approve the rollout."

Predicate analysis:
- "Review" → QA
- "billing service" → IMPLEMENT
- "build" → IMPLEMENT (consolidates)
- "validate" → QA
- "release criteria" → DOCUMENT
- "approve" → QA
- "rollout" → GAP (not SECURITY, not IMPLEMENT)

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: ["rollout"]
TransitionType: feature_addition
DecisionCount: 3
Phase routing: 4.1 + 2.1 + 7.1
DomainId: software-development

Reasoning: SECURITY does not fire because no security-domain terms are present. Three predicates only. "rollout" is gap. This pair teaches the boundary between three-predicate and four-predicate outcomes in B38.
