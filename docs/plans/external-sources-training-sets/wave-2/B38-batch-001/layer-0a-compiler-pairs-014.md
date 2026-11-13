# Layer 0-A — Compiler Pairs (014)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-014-001 — Release train approval: "approve" fires QA

Input:
"Approve the Q2 release train and confirm all feature teams have signed off."

Predicate analysis:
- "Approve" → QA
- "signed off" → QA (sign-off is in QA lexicon)
- "release train" → context phrase (no predicate)
- "Q2" → context word
- "feature teams" → not IMPLEMENT (teams are people, not code)

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "approve" and "signed off" both fire QA and consolidate into one QA predicate. "release train" is a delivery planning concept, not a predicate trigger. No IMPLEMENT → no_contribution.

---

## B38-0A-014-002 — Release train with service: IMPLEMENT fires

Input:
"Review all service deployments in the release train and approve each one."

Predicate analysis:
- "Review" → QA
- "service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "approve" → QA
- "deployments" → GAP (rule 4: deployment operational context)
- "release train" → context phrase

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["deployments"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "service" fires IMPLEMENT regardless that the sentence is about reviewing deployments. "deployments" generates a gap warning. QA fires from "review" and "approve".

---

## B38-0A-014-003 — Release train criteria: QA+DOCUMENT

Input:
"Validate the release train criteria and verify all completion criteria are met."

Predicate analysis:
- "Validate" → QA
- "verify" → QA
- "release train criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "completion criteria" → DOCUMENT (completion criteria: "criteria" fires DOCUMENT)

Multiple QA → one QA. Multiple DOCUMENT → one DOCUMENT.
Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "criteria" fires DOCUMENT regardless of qualifier ("release train", "completion"). Two QA terms and two DOCUMENT terms consolidate into one each.

---

## B38-0A-014-004 — Release train launch: gap only

Input:
"Launch the Q2 release train on the scheduled go-live date."

Predicate analysis:
- "Launch" → GAP (B38 gap vocabulary)
- "release train" → context phrase
- "go-live" → GAP (B38 gap vocabulary)
- "scheduled" → context word

Predicates fired: []
CapabilityGapWarnings: ["launch", "go-live"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: "launch" and "go-live" are both B38 gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-014-005 — Release train build approval: QA+IMPLEMENT

Input:
"Inspect the build artifacts in each release train slot and approve those that pass quality gates."

Predicate analysis:
- "Inspect" → QA (inspect is in QA lexicon)
- "build" → IMPLEMENT (rule 6: build fires IMPLEMENT — even for artifacts)
- "approve" → QA
- "quality" → QA
- "release train" → context phrase
- "artifacts" → not an independent predicate (subsumed by "build")

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "build" fires IMPLEMENT from rule 6. Multiple QA terms (inspect, approve, quality) consolidate into one QA predicate. feature_addition because IMPLEMENT is present.
