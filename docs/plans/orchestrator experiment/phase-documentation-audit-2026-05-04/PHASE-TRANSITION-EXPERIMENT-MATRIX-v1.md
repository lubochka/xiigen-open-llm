# Phase Transition Experiment Matrix
# Date: 2026-05-04

## Adjacent Phase Transitions

All six adjacent phase transitions have enough candidate source material to
start transition-gate experiments after manual source binding.

| Transition | Automated evidence verdict | Human judgment |
|---|---|---|
| Phase 1 -> Phase 2 | transition_experiment_candidate | Ready after selecting one design authority and one functional-spec authority. |
| Phase 2 -> Phase 3 | transition_experiment_candidate | Ready after selecting one requirement authority and one technical-design authority. |
| Phase 3 -> Phase 4 | transition_experiment_candidate | Highest-value next experiment. This is the transition recommended by the multi-phase plan. |
| Phase 4 -> Phase 5 | transition_experiment_candidate | Ready, but should avoid the missing Phase 5.2 integration-test gap unless that gap is the scenario. |
| Phase 5 -> Phase 6 | transition_experiment_candidate | Ready for functional and regression review; security/compliance must be a named boundary. |
| Phase 6 -> Phase 7 | transition_experiment_candidate | Ready only for CI/CD gate and reporting paths; scenario automation and regression-suite maintenance are knowledge edges. |

## Adjacent Sub-Phase Transitions

The scan tested 24 adjacent sub-phase transitions inside the seven phases.
Eighteen are candidate transitions and six are knowledge-edge transitions.

Knowledge-edge transitions:

| Transition | Reason |
|---|---|
| 5.1 -> 5.2 | Integration-test source authority missing. |
| 5.2 -> 5.3 | Integration-test source authority missing upstream. |
| 6.2 -> 6.3 | Security/compliance review source authority missing. |
| 6.3 -> 6.4 | Security/compliance review source authority missing upstream; accessibility audit is thin downstream. |
| 7.1 -> 7.2 | Test scenario automation source authority missing; regression maintenance source authority missing. |
| 7.2 -> 7.3 | Regression-suite maintenance source authority missing upstream. |

## First Transition Experiment To Run

Run Phase 3 -> Phase 4 first.

Why:

- The multi-phase plan already identifies this as the strongest first
  transition.
- Phase 3 and Phase 4 both have strong document coverage.
- This transition tests the hardest relationship: technical design constraints
  must be acknowledged by implementation plans.
- It has known failure modes: stated-but-unenforced constraints, missing
  out-of-scope acknowledgment, and false positives where build-time sequence
  is confused with runtime topology.

Required Round A oracle documents:

- `T34-TRANSITION-DOMAIN-MAP-v1.md`
- `T34-TRANSITION-FAILURE-TAXONOMY-v1.md`
- `T34-TRANSITION-EXPECTED-TRACE-v1.md`
- `T34-TRANSITION-HUMAN-RESULT-STANDARDS-v1.md`
- `T34-TRANSITION-EXECUTOR-CONTRACT-v1.md`

The oracle must be written before any code or learning pairs. The expected
trace is the senior artifact; learning pairs come after the experiment reveals
which nodes need them.

## Second Transition Experiment

Run Phase 1 -> Phase 2 second.

Why:

- It introduces the most novel vocabulary relative to the existing E53 corpus:
  design tokens, interaction states, layout, accessibility, and motion.
- It tests whether design decisions become functional-spec constraints rather
  than background text.
- It will reveal whether the local corpus is ready to handle product and UX
  orchestration, not just technical orchestration.

## Third Transition Experiment

Run Phase 6 -> Phase 7 third, but intentionally as a boundary experiment.

Why:

- The scan shows Phase 7 is partial.
- QA automation has strong CI/CD and reporting material but missing scenario
  automation and regression-suite maintenance authority.
- A correct orchestrator should produce a PASS WITH GAPS or knowledge-edge
  result, not a complete automation plan.

