# Layer 0-A — Compiler Pairs (057)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-057-001 — GatedRelease: document vs. knowledge_edge

Input A: "Document the gated release process and exit criteria."
- "Document" → DOCUMENT
- "gated release process" → DOCUMENT (process document; consolidates)
- "exit criteria" → DOCUMENT (criteria — rule 3; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Configure the gated release pipeline stages."
- "Configure" → not in lexicon
- "gated release pipeline stages" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-057-002 — GatedRelease: review and approve

Input: "Review the gated release conditions and approve the production release."
- "Review" → QA
- "approve" → QA (consolidates)
- "gated release conditions" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-057-003 — GatedRelease: criteria and sign-off

Input: "Document the gated release acceptance criteria and sign off the gate."
- "Document" → DOCUMENT
- "gated release acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-057-004 — GatedRelease: build gate service

Input: "Build the release gate service, document the gate exit criteria, and validate the gate conditions."
- "Build" → IMPLEMENT (rule 6)
- "release gate service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "gate exit criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-057-005 — GatedRelease: four-predicate gate

Input: "Review the gate service, audit the security gate controls, document the gate criteria, and approve the release."
- "Review" → QA
- "gate service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security gate controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "gate criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
