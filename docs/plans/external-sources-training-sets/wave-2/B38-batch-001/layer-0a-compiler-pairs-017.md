# Layer 0-A — Compiler Pairs (017)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-017-001 — Version tagging: "build" fires IMPLEMENT

Input:
"Verify the build version tag matches the release candidate and approve the final tag."

Predicate analysis:
- "Verify" → QA (verify is in QA lexicon)
- "build" → IMPLEMENT (rule 6: build fires IMPLEMENT always)
- "version tag" → context phrase (not an independent predicate)
- "release candidate" → context phrase
- "approve" → QA

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "build" fires IMPLEMENT (rule 6). "verify" and "approve" both fire QA and consolidate. feature_addition because IMPLEMENT is present.

---

## B38-0A-017-002 — Version tagging criteria: QA+DOCUMENT

Input:
"Validate the version tagging criteria and confirm all release criteria are correctly applied."

Predicate analysis:
- "Validate" → QA
- "version tagging criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "confirm" → not in any predicate lexicon
- "release criteria" → DOCUMENT (second DOCUMENT trigger, consolidates)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 2.1 (DOCUMENT) + 7.1 (QA)
DomainId: software-development

Reasoning: "criteria" fires DOCUMENT twice (version tagging criteria + release criteria); consolidates to one DOCUMENT. "validate" fires QA. No IMPLEMENT → no_contribution.

---

## B38-0A-017-003 — Version tagging with service and build

Input:
"Sign off on the version tag for the payment service build before production promotion."

Predicate analysis:
- "Sign off" → QA (sign-off is in QA lexicon)
- "version tag" → context phrase
- "payment service" → IMPLEMENT (service fires IMPLEMENT — rule 2)
- "build" → IMPLEMENT (rule 6: consolidates with service into one IMPLEMENT)
- "production promotion" → context phrase
- "promotion" → context word (not a predicate trigger)

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 4.1 (IMPLEMENT) + 7.1 (QA, last)
DomainId: software-development

Reasoning: "service" and "build" both fire IMPLEMENT and consolidate into one predicate. "sign off" fires QA. feature_addition because IMPLEMENT is present.

---

## B38-0A-017-004 — Version tag rollout: gap only

Input:
"Rollout the version tag to all environments on go-live day."

Predicate analysis:
- "Rollout" → GAP (B38 gap vocabulary)
- "version tag" → context phrase (not a predicate trigger)
- "environments" → not in any predicate lexicon
- "go-live" → GAP (B38 gap vocabulary)

Predicates fired: []
CapabilityGapWarnings: ["rollout", "go-live"]
TransitionType: N/A
Coverage status: knowledge_edge
DomainId: software-development

Reasoning: All action vocabulary is gap. Zero predicates → knowledge_edge.

---

## B38-0A-017-005 — Version tag audit: QA fires on "audit"

Input:
"Audit the version tag history to confirm release integrity before approving the final release."

Predicate analysis:
- "Audit" → QA (audit fires QA in release context — rule 19 context note: "security" not present here so only QA fires, not SECURITY)
- "confirm" → not in predicate lexicon
- "release integrity" → context phrase
- "approving" → QA
- "final release" → context phrase

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "audit" fires QA in release context (no security co-occurrence, so SECURITY does not co-fire). "approving" fires QA. Consolidate to one QA predicate. No IMPLEMENT → no_contribution.
