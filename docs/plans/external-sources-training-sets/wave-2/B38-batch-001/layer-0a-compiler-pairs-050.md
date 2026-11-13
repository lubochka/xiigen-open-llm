# Layer 0-A — Compiler Pairs (050)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-050-001 — SBOMGeneration: document vs. knowledge_edge

Input A: "Document the SBOM (Software Bill of Materials) generation process for the release."
- "Document" → DOCUMENT
- "SBOM generation process" → DOCUMENT (process document; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Generate the SBOM for the release artifact."
- "Generate" → not in lexicon
- "SBOM" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-050-002 — SBOMGeneration: review and approve

Input: "Review the SBOM for completeness and approve the release."
- "Review" → QA
- "approve" → QA (consolidates)
- "SBOM" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-050-003 — SBOMGeneration: audit and security

Input: "Audit the SBOM security controls and sign off the release."
- "Audit" → QA + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "SBOM" → context
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-050-004 — SBOMGeneration: build the SBOM service

Input: "Build the SBOM generation service, document the SBOM criteria, and validate the output."
- "Build" → IMPLEMENT (rule 6)
- "SBOM generation service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "SBOM criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-050-005 — SBOMGeneration: full four-predicate

Input: "Review the SBOM generation service, audit security of all components, document the SBOM criteria, and approve the release."
- "Review" → QA
- "SBOM generation service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security of all components" → SECURITY (consolidates)
- "document" → DOCUMENT
- "SBOM criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
