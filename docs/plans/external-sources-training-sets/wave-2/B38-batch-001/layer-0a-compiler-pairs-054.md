# Layer 0-A — Compiler Pairs (054)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-054-001 — SupplyChainRelease: document vs. knowledge_edge

Input A: "Document the supply chain release process and integrity requirements."
- "Document" → DOCUMENT
- "supply chain release process" → DOCUMENT (process document; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Coordinate the supply chain release across all vendor teams."
- "Coordinate" → not in lexicon
- "supply chain release" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-054-002 — SupplyChainRelease: review and approve

Input: "Review the supply chain release integrity and approve the release."
- "Review" → QA
- "approve" → QA (consolidates)
- "supply chain release integrity" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-054-003 — SupplyChainRelease: audit security

Input: "Audit the security of the supply chain release components and sign off."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "supply chain release components" → context
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-054-004 — SupplyChainRelease: build and document

Input: "Build the supply chain integrity service, document the supply chain criteria, and validate the release."
- "Build" → IMPLEMENT (rule 6)
- "supply chain integrity service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "supply chain criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-054-005 — SupplyChainRelease: four-predicate

Input: "Review the supply chain service, audit the supply chain security, document the supply chain criteria, and approve."
- "Review" → QA
- "supply chain service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "supply chain security" → SECURITY (consolidates)
- "document" → DOCUMENT
- "supply chain criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
