# Layer 0-A — Compiler Pairs (039)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-039-001 — MigrationGuide: document vs. knowledge_edge

Input A: "Document the migration guide for upgrading from v1 to v2."
- "Document" → DOCUMENT
- "migration guide" → DOCUMENT (guide is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Prepare the migration guide for the v2 release."
- "Prepare" → not in lexicon
- "migration guide" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-039-002 — MigrationGuide: review and sign-off

Input: "Review the migration guide, verify it covers all exit criteria, and sign off."
- "Review" → QA
- "verify" → QA (consolidates)
- "exit criteria" → DOCUMENT (criteria — rule 3)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-039-003 — MigrationGuide: security review

Input: "Audit the migration guide for security implications and approve the migration plan."
- "Audit" → QA + SECURITY co-fires (rule 19)
- "security" → SECURITY (consolidates)
- "approve" → QA (consolidates)
- "migration plan" → context
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-039-004 — MigrationGuide: build migration service

Input: "Build the migration service and document the migration guide criteria for the release."
- "Build" → IMPLEMENT (rule 6)
- "migration service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "migration guide criteria" → DOCUMENT (criteria — rule 3; consolidates)
- PredicateSet: ["IMPLEMENT","DOCUMENT"] → no QA trigger here
- TransitionType: feature_addition

---

## B38-0A-039-005 — MigrationGuide: full four-predicate

Input: "Review the migration service, audit security of the migration path, document exit criteria, and approve the release."
- "Review" → QA
- "migration service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "document" → DOCUMENT
- "exit criteria" → DOCUMENT (consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
