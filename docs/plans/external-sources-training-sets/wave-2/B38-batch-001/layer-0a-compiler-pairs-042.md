# Layer 0-A — Compiler Pairs (042)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-042-001 — APIBreakingChange: document vs. knowledge_edge

Input A: "Document the API (Application Programming Interface) breaking change in the release notes."
- "Document" → DOCUMENT
- "API breaking change" → context (breaking change is a context noun)
- "release notes" → context
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Introduce the API breaking change in v3."
- "Introduce" → not in lexicon
- "API breaking change" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-042-002 — APIBreakingChange: review and approve

Input: "Review the API breaking change impact and approve the v3 release."
- "Review" → QA
- "approve" → QA (consolidates)
- "API breaking change impact" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-042-003 — APIBreakingChange: criteria document and sign-off

Input: "Document the breaking change acceptance criteria and sign off on the API release."
- "Document" → DOCUMENT
- "breaking change acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-042-004 — APIBreakingChange: build the updated service

Input: "Build the updated API gateway service, document the breaking change criteria, and validate compatibility."
- "Build" → IMPLEMENT (rule 6)
- "API gateway service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "breaking change criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- "compatibility" → context
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-042-005 — APIBreakingChange: security audit

Input: "Audit the security of the updated API service and approve the breaking change for release."
- "Audit" → QA + SECURITY (rule 19)
- "security" → SECURITY (consolidates)
- "API service" → IMPLEMENT (service — rule 2)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- TransitionType: feature_addition
