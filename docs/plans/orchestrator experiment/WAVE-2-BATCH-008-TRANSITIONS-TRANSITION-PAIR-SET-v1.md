# Wave 2 Batch 008 - Transition Pair Set v1
# Date: 2026-05-05

---

## Phase Context Compiler Pairs

| pair_id | source_document_id | B.phase_context_summary | B.output_type | B.confidence |
|---|---|---|---|---:|
| PC-T01 | W2B008-T01 | Phase context summary carries design constraints, accessibility markers, interaction decisions from phase-1-ui-ux-definition to phase-2-functional-specification, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |
| PC-T02 | W2B008-T02 | Phase context summary carries business rules, integration boundaries, data boundaries from phase-2-functional-specification to phase-3-technical-design, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |
| PC-T03 | W2B008-T03 | Phase context summary carries architecture decisions, API contracts, security constraints from phase-3-technical-design to phase-4-implementation, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |
| PC-T04 | W2B008-T04 | Phase context summary carries implemented behaviors, rejected branches, source anchors from phase-4-implementation to phase-5-testing, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |
| PC-T05 | W2B008-T05 | Phase context summary carries test results, coverage gaps, performance findings from phase-5-testing to phase-6-qa, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |
| PC-T06 | W2B008-T06 | Phase context summary carries regression risks, compliance gaps, automation candidates from phase-6-qa to phase-7-qa-automation, with rejected and missing items preserved as predicate-level context. | phase_context_summary | 0.84 |

---

## Transition Gate Pairs

| pair_id | phase_context_id | B.transition_verdict_rule | B.output_type | B.confidence |
|---|---|---|---|---:|
| TG-T01 | PC-T01 | PASS_WITH_GAPS when downstream acknowledges design constraints, accessibility markers, interaction decisions and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |
| TG-T02 | PC-T02 | PASS_WITH_GAPS when downstream acknowledges business rules, integration boundaries, data boundaries and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |
| TG-T03 | PC-T03 | PASS_WITH_GAPS when downstream acknowledges architecture decisions, API contracts, security constraints and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |
| TG-T04 | PC-T04 | PASS_WITH_GAPS when downstream acknowledges implemented behaviors, rejected branches, source anchors and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |
| TG-T05 | PC-T05 | PASS_WITH_GAPS when downstream acknowledges test results, coverage gaps, performance findings and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |
| TG-T06 | PC-T06 | PASS_WITH_GAPS when downstream acknowledges regression risks, compliance gaps, automation candidates and names any missing source. BLOCK when downstream contradicts a carried constraint. | transition_gate_verdict | 0.84 |

---

## Transition Formatter Pairs

| pair_id | gate_verdict_id | B.human_transition_result | B.sections_present | B.confidence |
|---|---|---|---|---:|
| TF-T01 | TG-T01 | UI/UX to functional spec: carry forward design constraints, accessibility markers, interaction decisions. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |
| TF-T02 | TG-T02 | functional spec to technical design: carry forward business rules, integration boundaries, data boundaries. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |
| TF-T03 | TG-T03 | technical design to implementation: carry forward architecture decisions, API contracts, security constraints. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |
| TF-T04 | TG-T04 | implementation to testing: carry forward implemented behaviors, rejected branches, source anchors. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |
| TF-T05 | TG-T05 | testing to QA: carry forward test results, coverage gaps, performance findings. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |
| TF-T06 | TG-T06 | QA to QA automation: carry forward regression risks, compliance gaps, automation candidates. If downstream lacks authority, name the source gap instead of reopening or inventing the decision. | carry_forward, source_gap_visible, next_action | 0.84 |

---

## Phrasing B Rule

Every transition pair has a Phrasing B companion that changes surface wording while preserving the same carry-forward result.
