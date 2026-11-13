# Layer 0-A — Compiler Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-004-001 — "build" fires IMPLEMENT; "audit"+"approve" fire QA; "service" fires IMPLEMENT

Input:
"Audit the build artifacts and approve the release for the order service component."

Predicate analysis:
- "Audit" → QA (audit in release context fires QA; not SECURITY because no security-domain context)
- "build" → IMPLEMENT (build is in IMPLEMENT lexicon; fires even in "build artifacts")
- "approve" → QA
- "service" → IMPLEMENT (service fires IMPLEMENT always, including release review context)
- "component" → context word (no predicate)

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition (IMPLEMENT is present)
DecisionCount: 2
Phase routing: 7.1 (QA) + 4.1 (IMPLEMENT)
DomainId: software-development

Reasoning: "build" fires IMPLEMENT per B38 rule 6 (build fires even in "build artifacts" context). "service" fires IMPLEMENT per rule 2 (service = IMPLEMENT ALWAYS). "audit" fires QA per rule 1 (audit in release context). Two distinct predicates, QA ordered last.

---

## B38-0A-004-002 — "build artifacts" specifically fires IMPLEMENT (rule 6 demonstration)

Input:
"Inspect the build artifacts to verify their integrity before release."

Predicate analysis:
- "Inspect" → QA (inspect is in QA lexicon)
- "build" → IMPLEMENT (rule 6: build fires even in "build artifacts")
- "artifacts" → no predicate on its own
- "verify" → QA

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: Rule 6 is explicit: "build" fires IMPLEMENT even in "build provenance" and "build artifacts" phrases. "Inspect" and "verify" both contribute to QA.

---

## B38-0A-004-003 — "build provenance" also fires IMPLEMENT (rule 6 full coverage)

Input:
"Verify the build provenance records and approve the artifact for deployment."

Predicate analysis:
- "Verify" → QA
- "build" → IMPLEMENT (rule 6: build fires in "build provenance")
- "approve" → QA
- "deployment" → GAP

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: "build provenance" triggers IMPLEMENT because "build" is the IMPLEMENT lexicon word. "deployment" in post-action context remains gap. Both "verify" and "approve" contribute to QA.

---

## B38-0A-004-004 — "service" fires IMPLEMENT even in sign-off sentence

Input:
"Sign off on the payment service release after reviewing the audit trail."

Predicate analysis:
- "Sign off" → QA
- "service" → IMPLEMENT (rule 2: service = IMPLEMENT ALWAYS)
- "reviewing" → QA
- "audit" → QA (audit in release context)
- "trail" → context word

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: Rule 2 is absolute: even in a release review sentence where the user means "the service being released", the word "service" fires IMPLEMENT. Three QA signals collapse to a single QA predicate. QA is last.

---

## B38-0A-004-005 — "build" + "service" both fire IMPLEMENT (one predicate, two triggers)

Input:
"Approve the build for the notification service after quality validation."

Predicate analysis:
- "Approve" → QA
- "build" → IMPLEMENT
- "service" → IMPLEMENT (both triggers consolidate into single IMPLEMENT predicate)
- "quality" → QA
- "validation" → QA

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: Multiple IMPLEMENT-firing terms ("build", "service") consolidate into a single IMPLEMENT predicate — not two separate IMPLEMENT decisions. Predicate set is deduplicated. QA last ordering applies.
