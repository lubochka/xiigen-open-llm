# Layer 0-A — Compiler Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-023-001 — Sign-off collection: canonical QA-only

Input:
"Collect all release sign-offs from team leads and confirm the release is authorized."

Predicate analysis:
- "sign-offs" → QA (sign-off fires QA)
- "confirm" → not in predicate lexicon
- "release" → context word (not a predicate trigger on its own)
- "authorized" → not in predicate lexicon

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "sign-offs" fires QA. No other predicate vocabulary. No IMPLEMENT → no_contribution.

---

## B38-0A-023-002 — Sign-off with service in context: IMPLEMENT fires

Input:
"Get sign-off on the analytics service before the release date."

Predicate analysis:
- "sign-off" → QA
- "analytics service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "release date" → context phrase

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "analytics service" fires IMPLEMENT (rule 2 — always, regardless of sign-off context). "sign-off" fires QA. feature_addition because IMPLEMENT is present.

---

## B38-0A-023-003 — Sign-off checklist: gap warning but QA fires

Input:
"Complete the sign-off checklist and get approval from the release manager."

Predicate analysis:
- "sign-off" → QA
- "checklist" → GAP (B38 gap vocabulary)
- "approval" → QA (consolidates with sign-off)
- "release manager" → context phrase

Predicates fired: ["QA"]
CapabilityGapWarnings: ["checklist"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "checklist" is gap vocabulary and gets a warning, but it does NOT suppress QA (v6-01 corruption lesson). "sign-off" and "approval" both fire QA. No IMPLEMENT → no_contribution.

---

## B38-0A-023-004 — Sign-off documentation: QA+DOCUMENT

Input:
"Document the sign-off process and verify all approval criteria are met before release."

Predicate analysis:
- "Document" → DOCUMENT
- "sign-off" → QA
- "verify" → QA (consolidates)
- "approval criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3; consolidates)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "Document" and "approval criteria" fire DOCUMENT. "sign-off" and "verify" fire QA. No IMPLEMENT → no_contribution.

---

## B38-0A-023-005 — Sign-off go-live gap: QA fires despite gap context

Input:
"Sign off on the go-live checklist and confirm launch readiness."

Predicate analysis:
- "Sign off" → QA (sign-off fires QA regardless of surrounding gap vocabulary)
- "go-live" → GAP (B38 gap vocabulary)
- "checklist" → GAP (B38 gap vocabulary)
- "launch" → GAP (B38 gap vocabulary)
- "readiness" → context word (not a predicate trigger; "readiness criteria" would fire DOCUMENT, but "readiness" alone does not)

Predicates fired: ["QA"]
CapabilityGapWarnings: ["go-live", "checklist", "launch"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "sign off" fires QA. Three gap terms get warnings. This demonstrates v6-01 anti-corruption rule: QA is never suppressed by co-present gap vocabulary. No IMPLEMENT → no_contribution.
