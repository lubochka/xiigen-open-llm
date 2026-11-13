# Layer 0-A — Compiler Pairs (044)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-044-001 — ClientCommunication: document vs. knowledge_edge

Input A: "Document the client communication plan for the v5 release."
- "Document" → DOCUMENT
- "client communication plan" → DOCUMENT (plan document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Send the client communication email about the v5 release."
- "Send" → not in lexicon
- "client communication email" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-044-002 — ClientCommunication: review and approve

Input: "Review the client communication template and approve it for release distribution."
- "Review" → QA
- "approve" → QA (consolidates)
- "client communication template" → context
- "distribution" → gap
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- TransitionType: no_contribution

---

## B38-0A-044-003 — ClientCommunication: criteria and sign-off

Input: "Document the communication release criteria and sign off the client notice."
- "Document" → DOCUMENT
- "communication release criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-044-004 — ClientCommunication: build notification service

Input: "Build the client notification service, document the communication criteria, and validate the messaging."
- "Build" → IMPLEMENT (rule 6)
- "client notification service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "communication criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-044-005 — ClientCommunication: security audit of notification service

Input: "Audit the security of the client notification service and approve the communication release."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "client notification service" → IMPLEMENT (service — rule 2)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- TransitionType: feature_addition
