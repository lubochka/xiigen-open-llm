# Layer 0-A — Compiler Pairs (051)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-051-001 — SignedArtifact: document vs. knowledge_edge

Input A: "Document the signed artifact policy for the release pipeline."
- "Document" → DOCUMENT
- "signed artifact policy" → DOCUMENT (policy is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Sign the release artifact with the code signing certificate."
- "Sign" → not in lexicon (signing action is not a predicate)
- "release artifact" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-051-002 — SignedArtifact: review and approve

Input: "Verify the artifact signature and approve the release."
- "Verify" → QA
- "approve" → QA (consolidates)
- "artifact signature" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-051-003 — SignedArtifact: audit security of signing

Input: "Audit the security of the artifact signing process and sign off the release."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "artifact signing process" → context (process is not a service)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-051-004 — SignedArtifact: build the signing service

Input: "Build the artifact signing service, document the signing policy criteria, and validate signature integrity."
- "Build" → IMPLEMENT (rule 6)
- "artifact signing service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "signing policy criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-051-005 — SignedArtifact: four-predicate with security audit

Input: "Review the signing service, audit the security of the signing keys, document the signing criteria, and approve the release."
- "Review" → QA
- "signing service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security of the signing keys" → SECURITY (consolidates)
- "document" → DOCUMENT
- "signing criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
