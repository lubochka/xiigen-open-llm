# Layer 0-A — Compiler Pairs (055)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-055-001 — ComplianceRelease: document vs. knowledge_edge

Input A: "Document the compliance release requirements for the financial platform."
- "Document" → DOCUMENT
- "compliance release requirements" → DOCUMENT (requirements document; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Ensure the release meets all compliance requirements."
- "Ensure" → not in lexicon
- "compliance requirements" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-055-002 — ComplianceRelease: review and approve

Input: "Review the compliance release checklist and approve the deployment."
- "Review" → QA
- "approve" → QA (consolidates)
- "compliance release checklist" → context (checklist is gap vocabulary)
- Wait — "compliance" alone doesn't fire QA; it fires QA only from compliance review context.
- "Review" fires QA; "compliance" is context here.
- CapabilityGapWarnings: ["checklist"]
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-055-003 — ComplianceRelease: compliance fires QA

Input: "Verify compliance with the release standards and sign off."
- "Verify" → QA
- "compliance" → QA (compliance fires QA — rule: compliance in verification context)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-055-004 — ComplianceRelease: audit and document

Input: "Audit the security compliance controls, document the compliance criteria, and approve the release."
- "Audit" → QA + SECURITY (rule 19)
- "security compliance controls" → SECURITY (security fires; consolidates)
- "compliance" → QA (consolidates)
- "document" → DOCUMENT
- "compliance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-055-005 — ComplianceRelease: full four-predicate

Input: "Review the compliance service, audit the security controls, document the compliance criteria, and approve the release."
- "Review" → QA
- "compliance service" → IMPLEMENT (service — rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "compliance criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
