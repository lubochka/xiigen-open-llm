# Layer 0-A — Compiler Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-015-001 — Hotfix approval: QA fires on "approve"

Input:
"Approve the hotfix for the login issue and verify it passes the emergency release gate."

Predicate analysis:
- "Approve" → QA
- "verify" → QA
- "hotfix" → not IMPLEMENT (hotfix is a delivery process term, not code authoring in this context)
- "release gate" → context phrase (gate itself is not a predicate trigger)
- "login issue" → not a predicate

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "approve" and "verify" fire QA. "hotfix" as a noun phrase describing the artifact being approved is not a code-authoring trigger. No IMPLEMENT → no_contribution.

---

## B38-0A-015-002 — Hotfix with service: IMPLEMENT fires

Input:
"Review the hotfix to the authentication service and approve the emergency release."

Predicate analysis:
- "Review" → QA
- "hotfix" → not IMPLEMENT (process noun)
- "authentication service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "approve" → QA
- "emergency release" → context phrase

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "authentication service" contains "service" which fires IMPLEMENT (rule 2). Two QA terms consolidate. feature_addition because IMPLEMENT is present.

---

## B38-0A-015-003 — Hotfix criteria: QA+DOCUMENT

Input:
"Validate the hotfix exit criteria and document the release decision rationale."

Predicate analysis:
- "Validate" → QA
- "hotfix exit criteria" → DOCUMENT ("criteria" fires DOCUMENT — rule 3)
- "document" → DOCUMENT (document fires DOCUMENT)
- "release decision" → context phrase

Multiple DOCUMENT triggers → one DOCUMENT predicate.
Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "exit criteria" fires DOCUMENT via "criteria" (rule 3). "document" fires DOCUMENT independently. Both consolidate into one DOCUMENT predicate. No IMPLEMENT → no_contribution.

---

## B38-0A-015-004 — Hotfix rollback gap: knowledge_edge

Input:
"Execute the hotfix rollout and monitor for regressions before cutover."

Predicate analysis:
- "Execute" → GAP (rule 8)
- "hotfix rollout" → GAP (rollout is B38 gap vocabulary)
- "monitor" → GAP (rule 7)
- "regressions" → not in any predicate lexicon
- "cutover" → GAP (B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["execute", "rollout", "monitor", "cutover"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All action terms are gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-015-005 — Hotfix build review: QA+IMPLEMENT

Input:
"Inspect the hotfix build provenance and approve the patch release."

Predicate analysis:
- "Inspect" → QA (inspect is in QA lexicon)
- "hotfix build" → IMPLEMENT ("build" fires IMPLEMENT — rule 6, even in "build provenance")
- "provenance" → not an independent predicate (subsumed by "build")
- "approve" → QA
- "patch release" → context phrase

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "build" fires IMPLEMENT (rule 6). "inspect" and "approve" both fire QA and consolidate. feature_addition because IMPLEMENT is present.
