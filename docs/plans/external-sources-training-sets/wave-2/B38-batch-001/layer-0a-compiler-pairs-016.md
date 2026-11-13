# Layer 0-A — Compiler Pairs (016)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-016-001 — Release notes: "document" fires DOCUMENT

Input:
"Document the release notes for v3.2 and have the product owner sign off."

Predicate analysis:
- "Document" → DOCUMENT (document fires DOCUMENT predicate)
- "release notes" → context phrase (not an independent predicate; notes are what's being documented)
- "sign off" → QA (sign-off is in QA lexicon)
- "product owner" → context phrase

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "document" fires DOCUMENT. "sign off" fires QA. No service/build/pipeline vocabulary → no IMPLEMENT. TransitionType = no_contribution.

---

## B38-0A-016-002 — Release notes with service: IMPLEMENT co-fires

Input:
"Document the release notes for the notification service and review them with the team."

Predicate analysis:
- "Document" → DOCUMENT
- "release notes" → context phrase
- "notification service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "review" → QA

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "notification service" fires IMPLEMENT. "document" fires DOCUMENT. "review" fires QA. Three predicates, three routing decisions. feature_addition because IMPLEMENT is present.

---

## B38-0A-016-003 — Release notes criteria: QA+DOCUMENT

Input:
"Validate the release notes against the release criteria and document the acceptance decision."

Predicate analysis:
- "Validate" → QA
- "release criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "document" → DOCUMENT (consolidates with criteria DOCUMENT)
- "acceptance decision" → context phrase ("acceptance" alone without "criteria" does not necessarily fire in this context; "decision" is context word)

Note: "acceptance criteria" would fire DOCUMENT via "criteria". Here "acceptance decision" does not contain "criteria" — only "acceptance decision". "acceptance" alone in quality context can fire QA.

Revised:
- "acceptance" → QA (acceptance in quality/gate context fires QA)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: Two DOCUMENT triggers (criteria + document) consolidate. Two QA triggers (validate + acceptance) consolidate. No IMPLEMENT → no_contribution.

---

## B38-0A-016-004 — Documentation gap: "publish" has no predicate

Input:
"Publish the release notes on the go-live day and distribute to all stakeholders."

Predicate analysis:
- "Publish" → not in any predicate lexicon
- "release notes" → context phrase
- "go-live" → GAP (B38 gap vocabulary)
- "distribute" → not in any predicate lexicon
- "stakeholders" → context word

Predicates fired: []
CapabilityGapWarnings: ["go-live"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: "publish" and "distribute" are not predicate triggers. "go-live" is gap vocabulary. Zero predicates → knowledge_edge.

---

## B38-0A-016-005 — Full release notes scenario: QA+IMPLEMENT+DOCUMENT

Input:
"Review the release notes for the order service, verify they meet release criteria, and approve publication."

Predicate analysis:
- "Review" → QA
- "release notes" → context phrase
- "order service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "verify" → QA
- "release criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "approve" → QA

Predicates fired: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 3
Phase routing: 4.1 (IMPLEMENT) + 2.1 (DOCUMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: Three predicates. "service" fires IMPLEMENT. "criteria" fires DOCUMENT. Three QA terms consolidate. feature_addition because IMPLEMENT is present.
