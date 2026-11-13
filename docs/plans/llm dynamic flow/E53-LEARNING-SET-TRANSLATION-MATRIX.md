# E53 Learning-Set Translation Matrix

Scenario id: `E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION`

Experiment: `E53-HIGH-LEVEL-ORCHESTRATION-PROOF`

Date: 2026-05-04

## Purpose

This matrix shows how the E53 source scenario can become small learning examples after the orchestration proof is reviewed.

This file does not replace the expected trace.

This file does not say that all examples should be trained immediately.

It tells the dataset curator which station should learn from each source fact, what shape the example should have, and whether the material is positive, negative, warning, or boundary material.

## Use Rule

Do not extract a learning pair from this matrix until the expected trace is accepted.

The trace decides what the system should do. The matrix decides where that behavior should be taught later.

## Matrix

| Source fact id | Source wording | Logical structure | Target node role | Target node input shape | Target node expected output shape | Example status | Arbiter status | Learning timing |
|---|---|---|---|---|---|---|---|---|
| F01 | We are changing our customer onboarding flow. | human complex orchestration request | node-00 logical structure compiler | full human problem | detected subject: onboarding flow change; local subject: complex registration and onboarding orchestration | positive | none | immediate after E53 passes |
| F02 | Existing users started with email and password. | existing account-state branch | node-01 domain splitter | logical structure with email-started branch | domain packet: email_registration_state | positive | identity_consistency watches later use | immediate after E53 passes |
| F03 | Some users want Google sign-in. | provider identity entry branch | node-01 domain splitter | logical structure with provider sign-in branch | domain packet: provider_identity_entry | positive | token_safety watches proof | immediate after E53 passes |
| F04 | Users stopped halfway through questionnaire and want to continue later. | questionnaire progress plus resume boundary | node-01 domain splitter | logical structure with onboarding progress and return request | packets: questionnaire_onboarding and resume_state_boundary | boundary | resume_state_coverage | immediate boundary set after E53 passes |
| F05 | Repeated provider callbacks must not create duplicate accounts. | hard identity consistency rule | node-02 domain translator array | duplicate-prevention packet | formal statement: repeated callback resolves to existing account or pending registration, not new account | positive safety rule | identity_consistency | immediate after E53 passes |
| F06 | Terms and privacy acceptance before active user. | activation gate with missing evidence detail | node-02 domain translator array | policy-acceptance packet | formal statement: activation blocked until acceptance evidence rules are supplied or confirmed | boundary | policy_evidence | backlog until policy source rules exist |
| F07 | Expired token or session proof stops the flow. | trust proof stop condition | node-03 scheme planner | translated token-safety statement in full path | plan item: check current trust before continuation; wait for fresh trusted action if invalid | positive safety rule | token_safety | immediate after E53 passes |
| F08 | High-risk users may need biometric passport verification. | knowledge edge | knowledge edge detector | logical structure with biometric passport request | named edge: missing_biometric_passport_verification | knowledge_edge | knowledge_edge_non_invention | idle-learning backlog until source authority exists |
| F09 | Stakeholder asks to create a second account for repeated callback. | explicit conflict | node-03 scheme planner | full translated set with duplicate conflict and audit reason | plan item: reject duplicate creation and preserve audit need separately | negative | identity_consistency | immediate negative set after E53 passes |
| F10 | Audit trail should be easier to inspect. | valid goal with forbidden mechanism | node-05 applier array | processed duplicate-rejection item plus audit traceability goal | applied result: audit clarity uses traceability on existing account path | warning | audit_traceability | immediate warning set after E53 passes |
| F11 | Produce an orchestration plan and human-readable decision. | final human decision requirement | node-09 formatter | human-readable fragments with statuses | final answer with can proceed, blocked, rejected, unknown, next actions | positive | stack_independence_result_check | immediate after E53 passes |
| F12 | Do not bind to a specific stack. | stack independence constraint | node-06 result translator array and node-09 formatter | formal result fragments plus no-stack constraint | human-readable fragments and final result with no stack binding | positive and negative | stack_independence_result_check | immediate after E53 passes |
| C01 | Duplicate account request conflicts with duplicate prevention. | conflict marker | node-08 arbiter units | formal result plus identity_consistency arbiter | verdict rejects any second-account branch | negative | identity_consistency mandatory | immediate after E53 passes |
| C02 | Biometric passport request exceeds local coverage. | missing competence marker | external help planner and context pack builder | knowledge-edge result | approval-ready context pack; no live external call | knowledge_edge | knowledge_edge_non_invention | idle-learning backlog |
| C03 | Resume request needs saved-state rules. | missing source rule boundary | node-04 domain processor array | plan item b07_flag_resume_state_boundary | processed outcome with wait_for_saved_state_source_rules | boundary | resume_state_coverage | backlog until source rules exist |
| U01 | How questionnaire progress is stored is unknown. | missing saved-state source | dataset curator | learning gap packet | target: resume_state_boundary unit | boundary | resume_state_coverage | idle-learning backlog |
| U02 | What qualifies a user as high-risk is unknown. | missing business rule | knowledge edge detector | biometric proofing edge | ask user or outside source for high-risk definition before proofing rule | knowledge_edge | knowledge_edge_non_invention | backlog |
| U03 | Whether biometric proofing is allowed is unknown. | missing outside authority | external context pack builder | biometric context pack | context question asks what source rules are required before using biometric proofing | knowledge_edge | privacy_and_identity_review | backlog |
| R01 | Provider trust before account resolution. | dependency edge | node-03 scheme planner | full translated provider and duplicate statements | ordered plan: validate provider before account resolution | positive dependency | token_safety and identity_consistency | immediate after E53 passes |
| R02 | Account resolution before questionnaire continuation. | dependency edge | node-03 scheme planner | translated account and questionnaire statements | ordered plan: resolve account before reading questionnaire progress | positive dependency | edge_preservation | immediate after E53 passes |
| R03 | Policy acceptance before activation. | activation dependency | node-03 scheme planner | translated policy boundary and activation plan | ordered plan: block full activation until acceptance evidence exists | boundary dependency | policy_evidence | backlog until source rules exist |
| R04 | Fresh trusted action after expired proof. | wait-state rule | node-04 domain processor array | trust-check plan item | processed outcome: wait_for_fresh_trusted_action | positive safety and wait-state | token_safety | immediate after E53 passes |
| R05 | Context pack is not an answer. | authority boundary | formatter and knowledge-edge detector | knowledge-edge context pack and final fragments | final answer says outside help is needed before proofing is accepted | warning | knowledge_edge_non_invention | immediate after E53 passes |
| R06 | Arbiters decide acceptance. | authority rule | arbiter selector and arbiter units | formal result set with risk markers | mandatory arbiters and binding verdicts | positive authority | all selected arbiters | immediate after E53 passes |

## Immediate Learning Candidates

These examples can become local learning-set pairs immediately after the expected trace is accepted:

- F01 for node-00 complex orchestration recognition.
- F02 and F03 for node-01 domain splitting.
- F05 and F07 for safety-preserving translation and planning.
- F09 and C01 as negative duplicate-account material.
- F10 as warning material for audit-traceability without duplicate creation.
- F11 and F12 for final human result and stack independence.
- R01, R02, R04, R05, and R06 for dependency, wait, knowledge-edge, and authority behavior.

## Backlog Learning Candidates

These should not become full positive examples yet because source authority is missing:

- F06 policy acceptance evidence details.
- F08 biometric passport verification details.
- C02 biometric passport context-pack answer.
- C03 saved-state restoration behavior.
- U01 saved questionnaire progress storage.
- U02 high-risk user definition.
- U03 whether biometric proofing is allowed.
- R03 exact policy-before-activation implementation rule.

## Negative And Warning Examples

Negative examples:

- create a second account for a repeated provider callback;
- allow activation without policy acceptance evidence;
- continue onboarding from expired proof;
- invent biometric passport verification rules;
- promise exact resume behavior without saved-state rules;
- final answer hides rejected or unknown work.

Warning examples:

- audit clarity is valid but cannot override identity consistency;
- biometric passport proofing may be useful but requires outside authority;
- policy acceptance is required but exact evidence rules are missing;
- resume intent is valid but saved-state restoration is not supplied.

## Dataset-Curation Rule

The E53 proof should produce learning gap packets first.

Only accepted, rejected, and warning statuses from the proof should become training material.

Do not convert this matrix directly into training pairs before the proof has produced evidence.
