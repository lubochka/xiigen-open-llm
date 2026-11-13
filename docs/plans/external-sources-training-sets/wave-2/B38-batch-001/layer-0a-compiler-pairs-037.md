# Layer 0-A — Compiler Pairs (037)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-037-001 — ChangelogAutomation: generate vs. review

Input A: "Generate the changelog automatically from commit messages."
- "Generate" → not in lexicon (automation verb)
- "changelog" → context noun
- "commit messages" → context
- PredicateSet: []
- ResultState: knowledge_edge

Input B: "Review the auto-generated changelog and approve it for release."
- "Review" → QA
- "auto-generated changelog" → context (changelog is context)
- "approve" → QA (consolidates)
- PredicateSet: ["QA"]
- TransitionType: no_contribution

---

## B38-0A-037-002 — ChangelogAutomation: build the changelog pipeline

Input: "Build the changelog automation pipeline for the release service."
- "Build" → IMPLEMENT (rule 6: build always fires IMPLEMENT)
- "changelog automation pipeline" → context (pipeline noun here is the changelog tool, not code artifact — BUT "service" fires IMPLEMENT)
- "release service" → IMPLEMENT (rule 2: service always fires IMPLEMENT; consolidates)
- PredicateSet: ["IMPLEMENT"]
- TransitionType: feature_addition

---

## B38-0A-037-003 — ChangelogAutomation: document the changelog criteria

Input: "Document the changelog entry criteria and validate the output format."
- "Document" → DOCUMENT (rule 3 extension: document verb fires DOCUMENT)
- "changelog entry criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3; consolidates)
- "validate" → QA
- PredicateSet: ["DOCUMENT","QA"] → canonical order: ["QA","DOCUMENT"] after QA-last rule
- Wait — QA-last applies to routing order, but predicate detection yields both.
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-037-004 — ChangelogAutomation: security audit of changelog service

Input: "Audit the security of the changelog generation service and approve the release."
- "Audit" → QA + SECURITY co-fires (rule 19)
- "security" → SECURITY (consolidates)
- "changelog generation service" → IMPLEMENT (service fires — rule 2)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- TransitionType: feature_addition

---

## B38-0A-037-005 — ChangelogAutomation: gap vocabulary check

Input: "Automate the changelog generation and publish the release notes."
- "Automate" → not in lexicon
- "changelog generation" → context
- "publish" → not in lexicon (gap)
- "release notes" → context
- PredicateSet: []
- CapabilityGapWarnings: ["publish"]
- ResultState: knowledge_edge
