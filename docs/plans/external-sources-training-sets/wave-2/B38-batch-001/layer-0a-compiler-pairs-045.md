# Layer 0-A — Compiler Pairs (045)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-045-001 — InternalAnnouncement: document vs. knowledge_edge

Input A: "Document the internal announcement for the v6 release."
- "Document" → DOCUMENT
- "internal announcement" → DOCUMENT (announcement is a document artifact; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Post the internal announcement on the company wiki."
- "Post" → not in lexicon
- "internal announcement" → context
- "company wiki" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-045-002 — InternalAnnouncement: review and approve

Input: "Review the internal release announcement and approve it before distribution."
- "Review" → QA
- "approve" → QA (consolidates)
- "internal release announcement" → context
- "distribution" → gap
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- TransitionType: no_contribution

---

## B38-0A-045-003 — InternalAnnouncement: criteria and sign-off

Input: "Document the announcement release criteria and sign off the internal notice."
- "Document" → DOCUMENT
- "announcement release criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-045-004 — InternalAnnouncement: build announcement service

Input: "Build the internal announcement service and document the announcement template criteria."
- "Build" → IMPLEMENT (rule 6)
- "internal announcement service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "announcement template criteria" → DOCUMENT (criteria — rule 3; consolidates)
- PredicateSet: ["IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition
