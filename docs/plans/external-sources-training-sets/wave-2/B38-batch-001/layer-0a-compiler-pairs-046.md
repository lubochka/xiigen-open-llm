# Layer 0-A — Compiler Pairs (046)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-046-001 — SecurityAdvisory: document vs. knowledge_edge

Input A: "Document the security advisory for the v7 release patch."
- "Document" → DOCUMENT
- "security advisory" → DOCUMENT (advisory is a document artifact) + SECURITY (security fires SECURITY)
- PredicateSet: ["DOCUMENT","SECURITY"]
- TransitionType: no_contribution

Input B: "Publish the security advisory to the customer portal."
- "Publish" → not in lexicon (gap)
- "security advisory" → SECURITY (security fires)
- PredicateSet: ["SECURITY"]
- CapabilityGapWarnings: ["publish"]
- TransitionType: no_contribution

---

## B38-0A-046-002 — SecurityAdvisory: review and approve

Input: "Review the security advisory and approve the patch release."
- "Review" → QA
- "approve" → QA (consolidates)
- "security advisory" → SECURITY (security fires)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-046-003 — SecurityAdvisory: audit and criteria

Input: "Audit the security advisory content, document the patch release criteria, and sign off."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "document" → DOCUMENT
- "patch release criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-046-004 — SecurityAdvisory: build advisory service

Input: "Build the security advisory notification service, audit the advisory content, and approve the patch."
- "Build" → IMPLEMENT (rule 6)
- "security advisory notification service" → IMPLEMENT (service — rule 2) + SECURITY (security fires)
- "audit" → QA + SECURITY (rule 19; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- TransitionType: feature_addition

---

## B38-0A-046-005 — SecurityAdvisory: full four-predicate

Input: "Review the security advisory for the payment service, document the patch criteria, audit security controls, and approve."
- "Review" → QA
- "security advisory" → SECURITY
- "payment service" → IMPLEMENT (rule 2)
- "document" → DOCUMENT
- "patch criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "audit" → QA (consolidates) + SECURITY (consolidates)
- "security controls" → SECURITY (consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
