# Layer 0-A — Compiler Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-002-001 — "sign-off" fires QA; "launch" is gap vocabulary

Input:
"Collect the sign-offs for the release and confirm the launch is approved."

Predicate analysis:
- "sign-offs" → QA (sign-off is in QA lexicon)
- "approved" → QA (approve is in QA lexicon)
- "launch" → GAP (launch is B38 gap vocabulary, not in any predicate)
- "release" → no predicate on its own (context word)

Predicates fired: ["QA"]
CapabilityGapWarnings: ["launch"]
TransitionType: no_contribution (QA alone, no IMPLEMENT)
DecisionCount: 1
Phase routing: 7.1 only
DomainId: software-development

Reasoning: "sign-offs" and "approved" both belong to the QA lexicon and fire QA. "launch" is listed as B38 gap vocabulary and does not map to any predicate. Because IMPLEMENT is absent, TransitionType is no_contribution.

---

## B38-0A-002-002 — "approved" alone fires QA (no gap penalty)

Input:
"The release has been approved by all stakeholders."

Predicate analysis:
- "approved" → QA
- "release" → context word only (not a predicate trigger by itself)
- "stakeholders" → no predicate

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "approved" is unambiguously in the QA lexicon. No gap vocabulary present. Clean single-predicate result.

---

## B38-0A-002-003 — "sign-off" in contract/compliance context still fires QA

Input:
"All team leads must provide sign-off before the new component goes live."

Predicate analysis:
- "sign-off" → QA
- "goes live" (go-live variant) → GAP
- "component" → not IMPLEMENT here (no code-authoring context; this is a process sentence)

Predicates fired: ["QA"]
CapabilityGapWarnings: ["go-live"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: "sign-off" fires QA regardless of surrounding context. "goes live" is a go-live variant and is gap vocabulary. "component" does not fire IMPLEMENT because the sentence describes a process action (providing sign-off), not code authoring.

---

## B38-0A-002-004 — "launch" never fires IMPLEMENT (gap boundary demonstration)

Input:
"Schedule the production launch for Friday and get final sign-off."

Predicate analysis:
- "launch" → GAP (not IMPLEMENT — launch is operational, not code-authoring)
- "sign-off" → QA
- "production" → context word

Predicates fired: ["QA"]
CapabilityGapWarnings: ["launch"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: This pair demonstrates the boundary: "launch" looks like an action word but is B38 gap vocabulary. It does not fire IMPLEMENT. Only "sign-off" fires, giving a QA-only result with a gap warning on "launch".

---

## B38-0A-002-005 — Both "sign-off" and "launch" present; only QA fires

Input:
"Please obtain sign-off on the release and prepare for launch day activities."

Predicate analysis:
- "sign-off" → QA
- "launch" → GAP
- "release" → context word
- "activities" → no predicate

Predicates fired: ["QA"]
CapabilityGapWarnings: ["launch"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: Confirms rule: when gap vocabulary ("launch") appears alongside a firing predicate ("sign-off" → QA), the result is CapabilityGapWarning on the gap term, not suppression of the predicate. QA fires, gap is warned.
