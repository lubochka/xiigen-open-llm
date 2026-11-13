# Layer 0-A — Compiler Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-010-001 — "cutover runbook" pure gap → knowledge_edge

Input:
"Execute the cutover runbook and initiate rollback if the error rate exceeds threshold."

Predicate analysis:
- "Execute" → GAP (rule 8: execute in execution context is not IMPLEMENT)
- "cutover" → GAP (rule 5)
- "runbook" → GAP (rule 5)
- "initiate" → not a predicate trigger
- "rollback" → not in any predicate lexicon; operational vocabulary
- "error rate" → context phrase
- "threshold" → context word

Predicates fired: []
CapabilityGapWarnings: ["execute", "cutover", "runbook", "rollback"]
Gap IDs: gap-project-delivery-cutover-001, gap-project-delivery-runbook-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap
DomainId: software-development

Reasoning: Every operative word is either gap vocabulary (cutover, runbook, execute) or operational context (rollback). Zero predicates fire. Per B38 rule 18, project delivery gap vocabulary routes to knowledge_edge, not GapHandler.

---

## B38-0A-010-002 — "runbook" alone → knowledge_edge

Input:
"Follow the runbook steps for the production release window."

Predicate analysis:
- "Follow" → not a predicate
- "runbook" → GAP
- "production" → context word
- "release" → context word

Predicates fired: []
CapabilityGapWarnings: ["runbook"]
Gap IDs: gap-project-delivery-runbook-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
DomainId: software-development

Reasoning: Single gap word determines knowledge_edge. Even "production release" context words do not fire predicates. "Follow" is not in any predicate lexicon.

---

## B38-0A-010-003 — "cutover" alone → knowledge_edge

Input:
"Coordinate the cutover with the database team and update the on-call schedule."

Predicate analysis:
- "cutover" → GAP
- "on-call" → GAP (rule 5)
- "Coordinate" → not a predicate
- "database" → context word
- "schedule" → context word

Predicates fired: []
CapabilityGapWarnings: ["cutover", "on-call"]
Gap IDs: gap-project-delivery-cutover-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
DomainId: software-development

Reasoning: Two gap terms, zero predicates. knowledge_edge path confirmed.

---

## B38-0A-010-004 — "runbook" + "checklist" + "rollout" — three gap terms

Input:
"Review the runbook checklist and prepare for the weekend rollout."

Predicate analysis:
- "Review" → QA (review IS in QA lexicon)
- "runbook" → GAP
- "checklist" → GAP
- "rollout" → GAP

Predicates fired: ["QA"]
CapabilityGapWarnings: ["runbook", "checklist", "rollout"]
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: IMPORTANT BOUNDARY: "review" fires QA even in a sentence about reviewing a runbook. The QA predicate fires on "review" regardless of what is being reviewed. Three gap warnings issued. Result is NOT knowledge_edge because QA fires. This demonstrates that one QA word is enough to prevent knowledge_edge.

---

## B38-0A-010-005 — Pure cutover/runbook sentence without review: knowledge_edge confirmed

Input:
"Execute the database cutover using the runbook and initiate the rollback procedure if needed."

Predicate analysis:
- "Execute" → GAP
- "database" → context word
- "cutover" → GAP
- "runbook" → GAP
- "initiate" → not a predicate
- "rollback" → operational gap
- "procedure" → context word

Predicates fired: []
CapabilityGapWarnings: ["execute", "cutover", "runbook", "rollback"]
Gap IDs: gap-project-delivery-cutover-001, gap-project-delivery-runbook-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap
DomainId: software-development

Reasoning: No QA-firing word present (unlike pair 004 above which had "review"). Pure operational execution vocabulary produces knowledge_edge.
