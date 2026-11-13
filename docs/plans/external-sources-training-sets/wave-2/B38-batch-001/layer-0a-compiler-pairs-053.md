# Layer 0-A — Compiler Pairs (053)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-053-001 — ProvenanceVerification: review vs. knowledge_edge

Input A: "Verify the provenance of all release artifacts before approval."
- "Verify" → QA
- "approval" → QA (consolidates)
- "provenance" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

Input B: "Check the provenance chain for the release bundle."
- "Check" → not in lexicon
- "provenance chain" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-053-002 — ProvenanceVerification: document the policy

Input: "Document the provenance verification policy and criteria for all release artifacts."
- "Document" → DOCUMENT
- "provenance verification policy" → DOCUMENT (policy is a document artifact; consolidates)
- "criteria" → DOCUMENT (criteria — rule 3; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-053-003 — ProvenanceVerification: audit and sign-off

Input: "Audit the provenance security chain and sign off the release."
- "Audit" → QA + SECURITY (rule 19)
- "provenance security chain" → SECURITY (security fires; consolidates)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-053-004 — ProvenanceVerification: build verification service

Input: "Build the provenance verification service, document the verification criteria, and approve the release."
- "Build" → IMPLEMENT (rule 6)
- "provenance verification service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "verification criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "approve" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition
