# Layer 0-A — Compiler Pairs (052)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-052-001 — AttestationPolicy: document vs. knowledge_edge

Input A: "Document the attestation policy for the release supply chain."
- "Document" → DOCUMENT
- "attestation policy" → DOCUMENT (policy is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Issue the attestation for the release artifact."
- "Issue" → not in lexicon
- "attestation" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-052-002 — AttestationPolicy: review and sign-off

Input: "Review the attestation policy and sign off the release."
- "Review" → QA
- "sign off" → QA (consolidates)
- "attestation policy" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-052-003 — AttestationPolicy: criteria and security

Input: "Document the attestation acceptance criteria, audit the policy security, and approve."
- "Document" → DOCUMENT
- "attestation acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "audit" → QA + SECURITY (rule 19)
- "policy security" → SECURITY (consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-052-004 — AttestationPolicy: build attestation service

Input: "Build the attestation service, document the attestation criteria, and validate the policy."
- "Build" → IMPLEMENT (rule 6)
- "attestation service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "attestation criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-052-005 — AttestationPolicy: full four-predicate

Input: "Review the attestation service, audit the attestation security controls, document the policy criteria, and approve the release."
- "Review" → QA
- "attestation service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "attestation security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "policy criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
