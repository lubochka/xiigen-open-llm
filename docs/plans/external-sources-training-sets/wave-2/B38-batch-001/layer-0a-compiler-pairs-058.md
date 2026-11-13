# Layer 0-A — Compiler Pairs (058)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-058-001 — RegulatorySubmission: document vs. knowledge_edge

Input A: "Document the regulatory submission for the FDA (Food and Drug Administration) software release."
- "Document" → DOCUMENT
- "regulatory submission" → DOCUMENT (submission is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Submit the regulatory package to the FDA."
- "Submit" → not in lexicon
- "regulatory package" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-058-002 — RegulatorySubmission: review and approve

Input: "Review the regulatory submission and approve the release for submission."
- "Review" → QA
- "approve" → QA (consolidates)
- "regulatory submission" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-058-003 — RegulatorySubmission: compliance audit and criteria

Input: "Audit the security compliance of the regulatory submission, document the submission criteria, and sign off."
- "Audit" → QA + SECURITY (rule 19)
- "security compliance" → SECURITY (security fires; consolidates)
- "document" → DOCUMENT
- "submission criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-058-004 — RegulatorySubmission: build submission service

Input: "Build the regulatory submission service, document the submission criteria, and validate regulatory compliance."
- "Build" → IMPLEMENT (rule 6)
- "regulatory submission service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "submission criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- "regulatory compliance" → QA (compliance fires QA; consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-058-005 — RegulatorySubmission: four-predicate

Input: "Review the regulatory service, audit the security controls, document the regulatory criteria, and approve the release."
- "Review" → QA
- "regulatory service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "regulatory criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
