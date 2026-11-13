# Layer 0-A — Compiler Pairs (059)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-059-001 — FinalReleaseReport: document vs. knowledge_edge

Input A: "Document the final release report for the v8 platform release."
- "Document" → DOCUMENT
- "final release report" → DOCUMENT (report is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Archive the final release report for the project records."
- "Archive" → not in lexicon
- "final release report" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-059-002 — FinalReleaseReport: review and approve

Input: "Review the final release report and approve the project closure."
- "Review" → QA
- "approve" → QA (consolidates)
- "final release report" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-059-003 — FinalReleaseReport: criteria and sign-off

Input: "Document the final release acceptance criteria and sign off the release report."
- "Document" → DOCUMENT
- "final release acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-059-004 — FinalReleaseReport: build reporting service

Input: "Build the release reporting service, document the report criteria, and validate the final report."
- "Build" → IMPLEMENT (rule 6)
- "release reporting service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "report criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-059-005 — FinalReleaseReport: four-predicate

Input: "Review the reporting service, audit the security of the release records, document the report criteria, and approve."
- "Review" → QA
- "reporting service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security of the release records" → SECURITY (consolidates)
- "document" → DOCUMENT
- "report criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
