# Layer 0-A — Compiler Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-005-001 — All gap vocabulary; zero predicates → knowledge_edge

Input:
"Execute the production rollout and monitor the go-live deployment for any issues."

Predicate analysis:
- "Execute" → GAP (rule 8: execute is not IMPLEMENT in execution context; IMPLEMENT = code authoring, not running)
- "production" → context word
- "rollout" → GAP (rule 5: rollout is B38 gap vocabulary)
- "monitor" → GAP (rule 7: monitor is not in any predicate)
- "go-live" → GAP (rule 5: go-live is B38 gap vocabulary)
- "deployment" → GAP (rule 4: deployment alone is gap)
- "issues" → no predicate

Predicates fired: []
CapabilityGapWarnings: ["execute", "rollout", "monitor", "go-live", "deployment"]
TransitionType: N/A (no predicates)
DecisionCount: 0
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap
DomainId: software-development

Reasoning: Every operative word is B38 gap vocabulary. "Execute" does not fire IMPLEMENT because B38 rule 8 specifies IMPLEMENT = code authoring, not execution. With zero predicates, the path is knowledge_edge (not GapHandler — B38 rule 18 clarification: project delivery gap vocabulary routes to knowledge_edge, not SCHEDULE_CAPABILITY_LEARNING).

---

## B38-0A-005-002 — "monitor" alone → gap, no predicates

Input:
"Monitor the production environment and track deployment health metrics."

Predicate analysis:
- "Monitor" → GAP (rule 7)
- "production" → context word
- "deployment" → GAP (rule 4)
- "health" → context word
- "metrics" → no predicate

Predicates fired: []
CapabilityGapWarnings: ["monitor", "deployment"]
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap
DomainId: software-development

Reasoning: Both operative words are gap. "Monitor" is explicitly not in any predicate per rule 7. knowledge_edge is the correct result state.

---

## B38-0A-005-003 — "rollout" + "go-live" + "cutover" → knowledge_edge

Input:
"Plan the rollout schedule and coordinate the go-live cutover with the operations team."

Predicate analysis:
- "rollout" → GAP
- "go-live" → GAP
- "cutover" → GAP (rule 5: cutover is B38 gap vocabulary)
- "Plan" → not a registered predicate trigger (planning is not QA/IMPLEMENT/DOCUMENT/SECURITY)
- "schedule" → context word

Predicates fired: []
CapabilityGapWarnings: ["rollout", "go-live", "cutover"]
Gap IDs: gap-project-delivery-rollout-golive-001, gap-project-delivery-cutover-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
DomainId: software-development

Reasoning: Three gap vocabulary terms. "Plan" is not in any predicate lexicon. knowledge_edge returned. Gap IDs issued for go-live compound and cutover.

---

## B38-0A-005-004 — "execute" does not fire IMPLEMENT (rule 8 precise boundary)

Input:
"Execute the smoke tests against the production service after deployment."

Predicate analysis:
- "Execute" → GAP (rule 8: IMPLEMENT = code authoring, not running; execute in execution context is gap)
- "smoke tests" → context phrase (executing tests is not TEST predicate; TEST fires on authoring/creating tests)
- "production" → context word
- "service" → IMPLEMENT (rule 2: service fires IMPLEMENT ALWAYS)
- "deployment" → GAP

Predicates fired: ["IMPLEMENT"]
CapabilityGapWarnings: ["execute", "deployment"]
TransitionType: feature_addition (IMPLEMENT present, but no QA — unusual single-predicate result)
DecisionCount: 1
Phase routing: 4.1
DomainId: software-development

Reasoning: "service" still fires IMPLEMENT per rule 2 absolute. "Execute" does not fire IMPLEMENT per rule 8. This is a boundary demonstration: even though "execute" looks like it could mean code action, rule 8 is explicit. No QA terms present, so QA does not fire.

---

## B38-0A-005-005 — Pure operational language: rollout + checklist + on-call → knowledge_edge

Input:
"Complete the on-call handover and run through the go-live checklist before rollout."

Predicate analysis:
- "on-call" → GAP (rule 5)
- "go-live" → GAP
- "checklist" → GAP
- "rollout" → GAP
- "Complete" → context word (not QA in this non-review context)
- "run through" → not a predicate

Predicates fired: []
CapabilityGapWarnings: ["on-call", "go-live", "checklist", "rollout"]
Gap IDs: gap-project-delivery-checklist-001, gap-project-delivery-rollout-golive-001
TransitionType: N/A
DecisionCount: 0
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap
DomainId: software-development
