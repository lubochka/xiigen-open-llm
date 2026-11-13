# Layer 0-A — Compiler Pairs (049)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-049-001 — ArtifactProvenanceTracking: document vs. knowledge_edge

Input A: "Document the artifact provenance tracking policy for the release pipeline."
- "Document" → DOCUMENT
- "artifact provenance tracking policy" → DOCUMENT (policy is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Track artifact provenance throughout the build pipeline."
- "Track" → not in lexicon
- "artifact provenance" → context
- "build pipeline" → context (pipeline here is the build tool, not a code artifact)
- Wait — "build" fires IMPLEMENT (rule 6: build always fires)
- PredicateSet: ["IMPLEMENT"]
- TransitionType: feature_addition

---

## B38-0A-049-002 — ArtifactProvenanceTracking: review and approve

Input: "Review the artifact provenance records and approve the release."
- "Review" → QA
- "approve" → QA (consolidates)
- "artifact provenance records" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-049-003 — ArtifactProvenanceTracking: audit and security

Input: "Audit the security of the artifact provenance tracking system and approve the release."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "artifact provenance tracking system" → context (system is not "service")
- "approve" → QA (consolidates)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-049-004 — ArtifactProvenanceTracking: build and document

Input: "Build the provenance tracking service, document the provenance criteria, and validate the artifact chain."
- "Build" → IMPLEMENT (rule 6)
- "provenance tracking service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "provenance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-049-005 — ArtifactProvenanceTracking: full four-predicate

Input: "Review the provenance tracking service, audit the security controls, document the provenance criteria, and approve the release."
- "Review" → QA
- "provenance tracking service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "provenance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
