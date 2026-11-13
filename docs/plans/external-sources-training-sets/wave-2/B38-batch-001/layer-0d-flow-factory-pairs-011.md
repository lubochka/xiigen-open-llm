# Layer 0-D — Flow Factory Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-011-001 — knowledge_edge factory: no flow selected

PredicateSet: []
Scenario: Pure rollout/cutover vocabulary

Factory behavior when predicate set is empty:
1. No flow candidates evaluated
2. No flows created
3. knowledge_edge state set
4. CapabilityGapWarnings populated from input gap terms
5. Factory returns empty flow list + knowledge_edge metadata

Factory output:
{
  "flows": [],
  "decision_count": 0,
  "result_state": "knowledge_edge",
  "capability_gap_warnings": ["execute", "rollout", "go-live", "deployment"],
  "gap_ids": ["gap-project-delivery-rollout-golive-001", "gap-project-delivery-go-live-001"],
  "transition_type": null
}

---

## B38-0D-011-002 — Factory does not select a default flow for knowledge_edge

Important: When predicates = [], the factory does NOT:
- Select ReleaseGateFlow71 as a default
- Select any flow based on scenario context
- Infer QA from the scenario label "B38"

The factory is strictly predicate-driven. No predicates = no flows. knowledge_edge is not a fallback execution; it is a non-execution state.

---

## B38-0D-011-003 — knowledge_edge factory vs probationary_rejected factory

probationary_rejected:
- Predicates fired (e.g., ["QA", "IMPLEMENT"])
- Flow selected (probationary)
- Gate B evaluated → fails
- Fallback flow executes
- Flows list: [fallback_flow]
- result_state: probationary_rejected (but a flow still ran)

knowledge_edge:
- No predicates fired
- No flow selected
- No Gate B evaluation
- No flow executes
- Flows list: []
- result_state: knowledge_edge (no flow ran at all)

Both end in no-new-code result, but probationary_rejected ran a fallback flow; knowledge_edge ran nothing.

---

## B38-0D-011-004 — Factory result state codes for B38

| result_state | Predicates | Gate B | Flows executed | TransitionType |
|---|---|---|---|---|
| routed_deterministic | ["QA"] etc. | N/A | deterministic_generic | varies |
| probationary_accepted | has predicates | PASS | probationary | varies |
| probationary_rejected | has predicates | FAIL | fallback (deterministic) | varies |
| trusted_learned | has predicates | N/A (promoted) | trusted | varies |
| knowledge_edge | [] | N/A | none | null |
