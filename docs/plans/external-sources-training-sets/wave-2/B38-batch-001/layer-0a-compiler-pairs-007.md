# Layer 0-A — Compiler Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-007-001 — "acceptance criteria" fires DOCUMENT; verify+sign-off fire QA

Input:
"Verify the acceptance criteria are met and sign off on the feature release."

Predicate analysis:
- "Verify" → QA (rule 20: verify → QA)
- "acceptance criteria" → DOCUMENT (rule 3: criteria fires DOCUMENT ALWAYS)
- "sign off" → QA (rule 23: sign-off → QA)
- "feature" → context word
- "release" → context word

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution (IMPLEMENT absent; DOCUMENT routes to Phase 2 documentation)
DecisionCount: 2
Phase routing: 7.1 (QA) + 2.1 (DOCUMENT)
DomainId: software-development

Reasoning: "acceptance criteria" is the canonical DOCUMENT trigger. "Verify" and "sign off" both fire QA. No gap vocabulary present. No IMPLEMENT means no feature_addition TransitionType.

---

## B38-0A-007-002 — "criteria" qualifier does not change DOCUMENT outcome

Input:
"Check that all completion criteria are fulfilled and obtain manager sign-off."

Predicate analysis:
- "completion criteria" → DOCUMENT (criteria fires DOCUMENT regardless of qualifier)
- "sign-off" → QA
- "Check" → QA (check in quality context; verify-adjacent)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: "completion criteria" — the qualifier "completion" does not change the DOCUMENT fire. The rule is criteria → DOCUMENT ALWAYS. "Check" in quality-gate context functions as a QA synonym.

---

## B38-0A-007-003 — "release criteria" fires DOCUMENT (note: the exact qualifier from rule)

Input:
"Review and document the release criteria so all stakeholders can verify compliance."

Predicate analysis:
- "Review" → QA
- "document" → DOCUMENT (explicit document action also fires DOCUMENT)
- "release criteria" → DOCUMENT (criteria fires DOCUMENT)
- "verify" → QA
- "compliance" → QA (compliance is in QA lexicon)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: Multiple signals for both predicates. "Review", "verify", "compliance" all fire QA. "document" action and "release criteria" both fire DOCUMENT. Multiple triggers consolidate into one instance of each predicate.

---

## B38-0A-007-004 — "criteria" in definition sentence still fires DOCUMENT

Input:
"Define the performance criteria and sign off once all tests have passed."

Predicate analysis:
- "performance criteria" → DOCUMENT (criteria triggers DOCUMENT even in define context)
- "sign off" → QA
- "Define" → DOCUMENT (defining = documentation action)
- "tests" → context word (not TEST predicate; no test authoring described)

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: "Define" as a documentation action fires DOCUMENT. "performance criteria" reinforces DOCUMENT. "sign off" fires QA. TransitionType remains no_contribution because IMPLEMENT is absent.

---

## B38-0A-007-005 — "acceptance criteria" + "deployment" boundary

Input:
"Verify that the acceptance criteria pass and approve the deployment readiness."

Predicate analysis:
- "Verify" → QA
- "acceptance criteria" → DOCUMENT
- "approve" → QA
- "deployment" → GAP (deployment alone in operational context)
- "readiness" → context word

Predicates fired: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 2.1
DomainId: software-development

Reasoning: "acceptance criteria" and "deployment" appear together. "criteria" fires DOCUMENT; "deployment" is gap. They are evaluated independently. The gap warning on "deployment" does not suppress any predicate.
