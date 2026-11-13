# Layer 0-A — Compiler Pairs (038)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-038-001 — ReleaseNoteTemplate: create vs. approve

Input A: "Create a release note template for all future releases."
- "Create" → DOCUMENT (create in document-production context fires DOCUMENT)
- "release note template" → DOCUMENT (template document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Approve the release note template before distribution."
- "Approve" → QA
- "release note template" → context
- "distribution" → context (not a predicate)
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-038-002 — ReleaseNoteTemplate: build template service

Input: "Build the release note template service for the documentation pipeline."
- "Build" → IMPLEMENT (rule 6)
- "release note template service" → IMPLEMENT (service fires — rule 2; consolidates)
- "documentation pipeline" → context (pipeline = documentation tool here)
- PredicateSet: ["IMPLEMENT"]
- TransitionType: feature_addition

---

## B38-0A-038-003 — ReleaseNoteTemplate: document the criteria

Input: "Document the release note entry criteria and sign off the template."
- "Document" → DOCUMENT
- "release note entry criteria" → DOCUMENT (criteria fires — rule 3; consolidates)
- "sign off" → QA (sign-off fires QA — rule 23)
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-038-004 — ReleaseNoteTemplate: full validation

Input: "Review the release note template against acceptance criteria, build the template service, and approve for release."
- "Review" → QA
- "acceptance criteria" → DOCUMENT (criteria — rule 3)
- "build" → IMPLEMENT (rule 6)
- "template service" → IMPLEMENT (service — rule 2; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition
