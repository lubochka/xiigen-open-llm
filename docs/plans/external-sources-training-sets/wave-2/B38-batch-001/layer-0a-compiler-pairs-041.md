# Layer 0-A — Compiler Pairs (041)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-041-001 — DeprecationNotice: document vs. knowledge_edge

Input A: "Document the deprecation notice for the v1 API (Application Programming Interface) endpoint."
- "Document" → DOCUMENT
- "deprecation notice" → DOCUMENT (notice is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Send the deprecation notice to all subscribers."
- "Send" → not in lexicon
- "deprecation notice" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-041-002 — DeprecationNotice: approve the deprecation

Input: "Review the deprecation notice and approve the API sunset timeline."
- "Review" → QA
- "approve" → QA (consolidates)
- "deprecation notice" → context
- "API sunset timeline" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-041-003 — DeprecationNotice: criteria and sign-off

Input: "Document the deprecation acceptance criteria and sign off the notice for release."
- "Document" → DOCUMENT
- "deprecation acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-041-004 — DeprecationNotice: build deprecation service

Input: "Build the deprecation notification service, document the deprecation criteria, and verify the notice is compliant."
- "Build" → IMPLEMENT (rule 6)
- "deprecation notification service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "deprecation criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "verify" → QA
- "compliant" → QA (compliance fires QA)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-041-005 — DeprecationNotice: security audit context

Input: "Audit the security of the deprecation notification service and approve the deprecation plan."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "deprecation notification service" → IMPLEMENT (service — rule 2)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- TransitionType: feature_addition
