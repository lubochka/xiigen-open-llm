# Layer 0-A — Compiler Pairs (040)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-040-001 — UpgradeGuide: document the upgrade

Input A: "Document the upgrade guide for the platform v3 release."
- "Document" → DOCUMENT
- "upgrade guide" → DOCUMENT (guide artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Distribute the upgrade guide to all customer teams."
- "Distribute" → not in lexicon (gap)
- "upgrade guide" → context
- PredicateSet: []
- CapabilityGapWarnings: ["distribute"]
- ResultState: knowledge_edge

---

## B38-0A-040-002 — UpgradeGuide: review and approve

Input: "Review the upgrade guide for completeness and approve it for distribution."
- "Review" → QA
- "approve" → QA (consolidates)
- "upgrade guide" → context
- "completeness" → context
- "distribution" → gap
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- TransitionType: no_contribution

---

## B38-0A-040-003 — UpgradeGuide: criteria and sign-off

Input: "Document the upgrade readiness criteria and sign off the upgrade guide."
- "Document" → DOCUMENT
- "upgrade readiness criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-040-004 — UpgradeGuide: build the upgrade service

Input: "Build the upgrade service, document the upgrade acceptance criteria, and validate the process."
- "Build" → IMPLEMENT (rule 6)
- "upgrade service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "upgrade acceptance criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition
