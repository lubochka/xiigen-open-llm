# Layer 0-D — Flow Factory Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-021-001 — Guidance + API_CONTRACT embedded: factory selection examples

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Walk through integrating the payment API" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Guide through integrating the SSO provider" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Show how to integrate the webhook handler" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Facilitate integration of CRM with ticketing" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Walk through the API" (no integrate) | [] | GapHandler | none |
| "Teach to integrate" (infinitive exception) | [] | GapHandler | none |
| "Walk through integrating + implement the flow" | [API_CONTRACT, IMPLEMENT] | APIContractFlow33 → ImplementationFlow41 | feature_addition |

---

## FF-021-002 — API_CONTRACT + guidance: orchestration note

When API_CONTRACT fires alongside guidance verb:
- APIContractFlow33 executes normally
- No GapHandler invoked
- CapabilityGapWarning appended: gap-skills-guidance-mentor-001 (or appropriate gap ID)
- TransitionType: documentation (API_CONTRACT alone)
- TransitionType: feature_addition if IMPLEMENT also present

---

## FF-021-003 — "Walk through integrating" — three-step factory process

Step 1: "walk through" detected → mark as guidance vocabulary candidate gap warning
Step 2: "integrating" detected → API_CONTRACT fires (integrate always fires in any frame)
Step 3: Predicate set = [API_CONTRACT] → route to APIContractFlow33; "walk through" becomes CapabilityGapWarning
Result: APIContractFlow33 executes; gap warning emitted; GapHandler suppressed.
