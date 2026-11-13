# Node 03 - Scheme Planner Set

## Purpose

This file teaches the Flow-01 scheme planner.

Node 03 is not an array node. It receives the translated domain statements for one flow path together. Its output is the actual ordered plan: labelled sub-tasks, dependencies, wait states, boundary handling, rejection handling, and transformational prompts.

The output is the plan itself, not a description of a plan.

## Input Completeness Rule

The planner must see the full translated set for the path. If a path came from node 01 with five domain packets, the node-03 A input must preserve five translated statements or explicitly mark the missing translated statement as a coverage gap. Do not remove a packet just because it makes the plan harder.

Some translated statements below are direct continuations of the accepted node-02 output shape for domain packets that node-02 has not yet expanded into their own pair. They are included here so node-03 is trained on complete paths. Those packets remain useful targets for a later node-02 expansion round.

## Plan Output Shape

Each B output uses this shape:

- plan_status;
- path_name;
- ordered_plan_items;
- dependency_rules;
- parallel_groups;
- wait_states;
- recovery_or_rejection_rules;
- planner_confidence;
- next_node_handoff.

Each plan item has:

- task_id;
- task_statement;
- transformational_prompt;
- depends_on;
- allowed_parallel_with;
- stop_or_wait_condition;
- review_marker.

---

## Pair 001 - Clean email verification path

A:

path_name: email verification after email registration

translated_domain_statements:

- statement_id: t_email_verification
  domain_label: email_verification
  translated_domain_statement: The email registration path requires mailbox-ownership verification. Identify the pending registration account, create a time-limited verification action bound to that pending account and mailbox, send the verification message to the registered email address, and treat the account as pending until the verification action returns and is validated. Sending the message is not the same as completing verification.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_token_session_safety
  domain_label: token_session_safety
  translated_domain_statement: The verification link must remain valid, single-purpose, and safe before it can activate account access. Check the verification proof before completing registration, reject expired or reused proof, and start a normal session only after verification succeeds.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: ready_for_processing

path_name: email verification after email registration

ordered_plan_items:

- task_id: b1_find_pending_registration
  task_statement: Find the pending registration account that owns the email address requesting verification.
  transformational_prompt: Use the pending account as the only account target for this path. Do not verify a mailbox without binding it to one pending registration account.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if no pending registration account exists for the mailbox.
  review_marker: none

- task_id: b2_create_verification_action
  task_statement: Create a time-limited verification action for the pending account and mailbox.
  transformational_prompt: Bind the verification action to the pending account, the mailbox, and a limited validity period. The verification action proves mailbox ownership only after it returns and is accepted.
  depends_on: [b1_find_pending_registration]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the verification action cannot be bound to both account and mailbox.
  review_marker: token_safety

- task_id: b3_send_verification_message
  task_statement: Send the verification message to the registered email address.
  transformational_prompt: Deliver the verification action to the same mailbox recorded on the pending account. Sending the message does not complete registration.
  depends_on: [b2_create_verification_action]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for the user to return the verification action.
  review_marker: none

- task_id: b4_validate_returned_verification
  task_statement: Validate the returned verification action before changing the account state.
  transformational_prompt: Accept only the verification action that is bound to the pending account, bound to the same mailbox, still valid, and not previously used.
  depends_on: [b3_send_verification_message]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the action is expired, reused, mismatched, or absent.
  review_marker: token_safety

- task_id: b5_complete_email_registration
  task_statement: Move the account from pending email verification to completed registration and allow normal session start.
  transformational_prompt: Complete registration only after b4 validates the returned verification action. Do not create a normal session before mailbox ownership is verified.
  depends_on: [b4_validate_returned_verification]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if verification did not pass.
  review_marker: none

dependency_rules:

- b1 must happen before every other item.
- b2 must happen before b3.
- b3 creates a wait state; b4 happens only after the user returns the verification action.
- b5 is blocked until b4 passes.

parallel_groups: []

wait_states:

- wait_for_verification_action_return after b3_send_verification_message.

recovery_or_rejection_rules:

- Expired, reused, mismatched, or absent verification proof stops b4 and routes to a fresh verification action.

planner_confidence: 0.93

next_node_handoff: send_each_plan_item_to_domain_processor_array

Reason: The translated set is clean and covered, so the planner can produce a complete ordered verification path.

## Pair 001 - Phrasing B

A:

path_name: confirmation link for an email account

translated_domain_statements:

- statement_id: t_email_confirmation
  domain_label: email_verification
  translated_domain_statement: The account may be created in a pending state, but mailbox trust is not complete until the confirmation link is used and accepted. Produce a confirmation action for the pending account, deliver it to the registered mailbox, keep the account in pending-verification state, and later validate the returned confirmation action before granting normal account access.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_link_safety
  domain_label: token_session_safety
  translated_domain_statement: The confirmation action acts as temporary trust proof. It must be checked for account match, mailbox match, validity period, and single use before the account can be treated as verified.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: ready_for_processing

path_name: confirmation link for an email account

ordered_plan_items:

- task_id: b1_select_pending_account
  task_statement: Select the pending account whose mailbox needs confirmation.
  transformational_prompt: Work only with the pending account connected to the mailbox in the request. Do not use the confirmation flow to create a separate account.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the mailbox is not tied to one pending account.
  review_marker: none

- task_id: b2_prepare_confirmation_action
  task_statement: Prepare a confirmation action for that pending account and mailbox.
  transformational_prompt: Bind the action to the account, mailbox, validity period, and single-use rule. The action is not proof until it returns and passes validation.
  depends_on: [b1_select_pending_account]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the action cannot be bound safely.
  review_marker: token_safety

- task_id: b3_deliver_confirmation_message
  task_statement: Deliver the confirmation message to the pending account mailbox.
  transformational_prompt: Send the action to the registered mailbox and keep the account pending. Do not treat message delivery as verified ownership.
  depends_on: [b2_prepare_confirmation_action]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for the confirmation action to return.
  review_marker: none

- task_id: b4_check_confirmation_return
  task_statement: Check the returned confirmation action.
  transformational_prompt: Confirm account match, mailbox match, validity, and single use. Reject any returned action that fails one of those checks.
  depends_on: [b3_deliver_confirmation_message]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the action fails any safety check.
  review_marker: token_safety

- task_id: b5_open_verified_account_access
  task_statement: Mark the account email as verified and allow normal account access.
  transformational_prompt: Open account access only after the confirmation action passes b4. Keep the account pending if b4 fails or is still waiting.
  depends_on: [b4_check_confirmation_return]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if confirmation is not accepted.
  review_marker: none

dependency_rules:

- b1, b2, b3, b4, and b5 are sequential.
- b3 creates the wait state.
- b5 cannot run from message delivery alone.

parallel_groups: []

wait_states:

- wait_for_confirmation_return after b3_deliver_confirmation_message.

recovery_or_rejection_rules:

- If b4 rejects the action, keep the account pending and require a new confirmation action.

planner_confidence: 0.93

next_node_handoff: send_each_plan_item_to_domain_processor_array

---

## Pair 002 - Clean provider sign-up path

A:

path_name: Google provider sign-up with duplicate prevention

translated_domain_statements:

- statement_id: t_provider_identity
  domain_label: provider_identity_entry
  translated_domain_statement: The registration entry point is an external identity-provider flow. The provider is Google. The system must treat the provider response as an identity assertion that is not trusted until verified. Receive the provider result, validate that the result belongs to the expected provider flow, extract the provider identity, normalize the returned email or stable provider subject, and expose the verified identity result to the account-resolution step.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_duplicate_prevention
  domain_label: duplicate_prevention
  translated_domain_statement: The verified provider identity must be matched against existing linked accounts and pending registrations before any new account is created. The same provider subject or verified mailbox must resolve to one local account path.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_token_session_safety
  domain_label: token_session_safety
  translated_domain_statement: Provider tokens and the resulting session must be safe before registration completes. The flow must validate provider proof before account completion and must not start a registered session from invalid or stale provider trust material.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: ready_for_processing

path_name: Google provider sign-up with duplicate prevention

ordered_plan_items:

- task_id: b1_validate_provider_result
  task_statement: Validate the Google provider result before using it as local identity evidence.
  transformational_prompt: Treat the provider response as untrusted until it matches the expected provider flow and current sign-up attempt. Extract the stable provider subject and normalized mailbox only after validation passes.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the provider result is absent, invalid, mismatched, or stale.
  review_marker: token_safety

- task_id: b2_match_existing_identity
  task_statement: Search for an existing account or pending registration that already owns the verified provider identity.
  transformational_prompt: Use the stable provider subject first, then the verified mailbox if available. The matching step must finish before any account write.
  depends_on: [b1_validate_provider_result]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop account creation until identity matching is complete.
  review_marker: identity_consistency

- task_id: b3_choose_create_or_link_path
  task_statement: Choose exactly one account path: create a new account or link to an existing account or pending registration.
  transformational_prompt: If no matching account exists, choose creation. If a matching account or pending registration exists, choose linking or continuation. Never choose both paths for the same provider identity.
  depends_on: [b2_match_existing_identity]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the match result is ambiguous.
  review_marker: identity_consistency

- task_id: b4_write_single_account_result
  task_statement: Write the chosen account result.
  transformational_prompt: For creation, write one new account tied to the verified provider identity. For linking, preserve the existing account identifier and add the provider identity. For pending continuation, reuse the pending registration. Do not create a second account for the same provider identity.
  depends_on: [b3_choose_create_or_link_path]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the write would produce two accounts for one provider identity.
  review_marker: identity_consistency

- task_id: b5_start_safe_session
  task_statement: Start a normal session only after a single account result exists and provider proof is still valid.
  transformational_prompt: Use the account result from b4 and the validated provider proof from b1. Do not start a session from stale provider material or from an unresolved duplicate match.
  depends_on: [b1_validate_provider_result, b4_write_single_account_result]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if account resolution or provider proof is not accepted.
  review_marker: token_safety

dependency_rules:

- b1 must complete before b2.
- b2 must complete before b3.
- b3 must complete before b4.
- b5 depends on both b1 and b4.

parallel_groups: []

wait_states: []

recovery_or_rejection_rules:

- Invalid provider proof stops the path and asks for a fresh trusted provider entry.
- Ambiguous identity match stops account writing and routes to identity review.
- Duplicate account creation is rejected.

planner_confidence: 0.94

next_node_handoff: send_each_plan_item_to_domain_processor_array

Reason: Provider sign-up is covered and must be planned as verify, match, choose, write, then session.

## Pair 002 - Phrasing B

A:

path_name: provider profile registration without password-first entry

translated_domain_statements:

- statement_id: t_provider_entry
  domain_label: provider_identity_entry
  translated_domain_statement: The requested registration entry path uses a third-party identity provider instead of local password-first credentials. Accept the provider callback or provider profile only as an untrusted identity claim at first, validate that it belongs to the expected sign-up attempt, derive the stable provider identity, and pass the verified identity claim forward for account matching.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_account_match
  domain_label: duplicate_prevention
  translated_domain_statement: A provider profile can create a local account only after the same provider identity and verified mailbox have been checked against existing accounts and pending registrations. One external identity must not become two local accounts.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_session_safety
  domain_label: token_session_safety
  translated_domain_statement: A session may start only from valid provider proof and a resolved account. Invalid, repeated, or stale proof must not complete registration or open a session.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: ready_for_processing

path_name: provider profile registration without password-first entry

ordered_plan_items:

- task_id: b1_check_provider_claim
  task_statement: Check the provider claim before using it for local registration.
  transformational_prompt: Verify that the provider claim belongs to the expected sign-up attempt and derive the stable provider identity only after that check passes.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the provider claim is invalid or no longer trusted.
  review_marker: token_safety

- task_id: b2_resolve_identity_owner
  task_statement: Resolve whether the verified provider identity already belongs to a local account or pending registration.
  transformational_prompt: Search by stable provider subject and verified mailbox before writing. The identity owner decision must happen before creation.
  depends_on: [b1_check_provider_claim]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if ownership cannot be determined.
  review_marker: identity_consistency

- task_id: b3_select_single_outcome
  task_statement: Select one outcome for the provider identity.
  transformational_prompt: Create when no owner exists. Link when an existing account owns the identity. Continue when a pending registration owns the identity. Do not combine outcomes.
  depends_on: [b2_resolve_identity_owner]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if two outcomes appear possible for the same identity.
  review_marker: identity_consistency

- task_id: b4_commit_account_outcome
  task_statement: Commit the chosen account outcome.
  transformational_prompt: Write exactly one account result for the provider identity. Preserve an existing account identifier when linking. Preserve a pending registration when continuing.
  depends_on: [b3_select_single_outcome]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the write would duplicate the local account.
  review_marker: identity_consistency

- task_id: b5_open_session_after_resolution
  task_statement: Open the registered session only after identity and account resolution pass.
  transformational_prompt: Use the accepted provider proof and the one account result. Do not open a session from unresolved or expired proof.
  depends_on: [b1_check_provider_claim, b4_commit_account_outcome]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if either proof or account resolution is missing.
  review_marker: token_safety

dependency_rules:

- Provider proof comes first.
- Identity matching comes before account writing.
- Session opening comes last.

parallel_groups: []

wait_states: []

recovery_or_rejection_rules:

- Request a fresh provider entry when proof fails.
- Route unresolved owner conflicts to identity review.
- Reject any branch that creates two local accounts for one provider identity.

planner_confidence: 0.94

next_node_handoff: send_each_plan_item_to_domain_processor_array

---

## Pair 003 - Mixed pending password recovery path

A:

path_name: unfinished email registration with forgotten password

translated_domain_statements:

- statement_id: t_email_registration_entry
  domain_label: email_registration_entry
  translated_domain_statement: The person is inside an unfinished email registration path. The account is not fully completed because the mailbox has not yet been verified and credential handling is not complete.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_credential_recovery_boundary
  domain_label: credential_recovery_boundary
  translated_domain_statement: The packet describes recovery during an unfinished registration, not ordinary recovery for a completed account. Preserve the pending registration state, mark password recovery as a boundary that needs account-recovery source rules, do not grant normal account access from the recovery request, and require mailbox ownership or another trusted proof before allowing continuation.
  coverage_status: boundary
  boundary_marker: needs_account_recovery_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_email_verification
  domain_label: email_verification
  translated_domain_statement: The mailbox is still unverified. The pending registration cannot be treated as a normal trusted account until verification returns and is accepted.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_token_session_safety
  domain_label: token_session_safety
  translated_domain_statement: Any recovery or continuation action must use safe temporary proof and must not grant normal account access before mailbox ownership is trusted.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: boundary_plan_requires_source_rules

path_name: unfinished email registration with forgotten password

ordered_plan_items:

- task_id: b1_locate_pending_email_registration
  task_statement: Locate the pending email registration attempt.
  transformational_prompt: Work only with a pending registration state. Do not treat this as a completed-account recovery flow.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if no pending registration exists.
  review_marker: none

- task_id: b2_preserve_unverified_state
  task_statement: Preserve the account as pending and unverified.
  transformational_prompt: Keep normal account access blocked. The forgotten password does not change the mailbox trust state.
  depends_on: [b1_locate_pending_email_registration]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if any branch attempts normal access before verification.
  review_marker: token_safety

- task_id: b3_flag_recovery_rule_gap
  task_statement: Mark the password recovery step as a boundary that needs account-recovery source rules.
  transformational_prompt: Carry boundary_marker needs_account_recovery_rules forward. Do not invent how a pending-registration password recovery should be performed.
  depends_on: [b1_locate_pending_email_registration]
  allowed_parallel_with: [b2_preserve_unverified_state]
  stop_or_wait_condition: Stop normal planning for the recovery mechanism until source rules are supplied.
  review_marker: coverage_review

- task_id: b4_require_trusted_mailbox_or_equivalent_proof
  task_statement: Require mailbox verification or another trusted proof before continuation.
  transformational_prompt: Allow continuation only from trusted proof. If proof is missing, produce a wait or request step instead of completing registration.
  depends_on: [b2_preserve_unverified_state, b3_flag_recovery_rule_gap]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for trusted proof or missing recovery rules.
  review_marker: token_safety

dependency_rules:

- b1 comes first.
- b2 and b3 may be considered in parallel after b1 because one preserves state and the other records the missing rule.
- b4 depends on both b2 and b3.

parallel_groups:

- [b2_preserve_unverified_state, b3_flag_recovery_rule_gap]

wait_states:

- wait_for_account_recovery_source_rules after b3_flag_recovery_rule_gap.
- wait_for_trusted_mailbox_or_equivalent_proof after b4_require_trusted_mailbox_or_equivalent_proof.

recovery_or_rejection_rules:

- Reject any plan that grants normal access from forgotten-password input alone.
- Route the missing recovery mechanism to future account-recovery training material.

planner_confidence: 0.72

next_node_handoff: send_non_boundary_items_to_processor_and_keep_boundary_flag_attached

Reason: The path is partly covered, so the planner must plan covered safety work while preserving the account-recovery boundary.

## Pair 003 - Phrasing B

A:

path_name: pending email sign-up with lost password

translated_domain_statements:

- statement_id: t_pending_email_signup
  domain_label: email_registration_entry
  translated_domain_statement: The user is in an email sign-up attempt that has not reached trusted completion. The mailbox confirmation is unfinished and the credential problem belongs to that pending state.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_lost_password_boundary
  domain_label: credential_recovery_boundary
  translated_domain_statement: The password problem is a recovery case during pending registration. Keep the registration incomplete, require a trusted ownership step before any continuation, and preserve the missing rule for how pending-registration recovery should work.
  coverage_status: boundary
  boundary_marker: needs_account_recovery_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_mailbox_pending
  domain_label: email_verification
  translated_domain_statement: The email confirmation step is not finished, so the mailbox cannot yet serve as fully trusted account evidence.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_recovery_token_safety
  domain_label: token_session_safety
  translated_domain_statement: Any link or recovery action used for continuation must be checked before it can affect the pending account. It must not become normal account access by itself.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: boundary_plan_requires_source_rules

path_name: pending email sign-up with lost password

ordered_plan_items:

- task_id: b1_find_pending_signup
  task_statement: Find the pending email sign-up attempt.
  transformational_prompt: Keep the work inside the pending registration flow. Do not switch to ordinary completed-account recovery.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if there is no pending sign-up attempt.
  review_marker: none

- task_id: b2_keep_mailbox_untrusted
  task_statement: Keep mailbox trust incomplete.
  transformational_prompt: The mailbox remains untrusted until confirmation or equivalent trusted proof is accepted. Do not unlock the account from the lost-password request.
  depends_on: [b1_find_pending_signup]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the flow tries to grant access before trust proof.
  review_marker: token_safety

- task_id: b3_record_pending_recovery_gap
  task_statement: Record that pending-registration password recovery needs source rules.
  transformational_prompt: Attach boundary_marker needs_account_recovery_rules to the plan item. Do not fill in the missing recovery design from assumption.
  depends_on: [b1_find_pending_signup]
  allowed_parallel_with: [b2_keep_mailbox_untrusted]
  stop_or_wait_condition: Stop recovery design until source rules exist.
  review_marker: coverage_review

- task_id: b4_wait_for_trusted_continuation
  task_statement: Wait for a trusted continuation action.
  transformational_prompt: Continue only after trusted proof exists and the missing recovery rule is supplied. Otherwise keep the account pending.
  depends_on: [b2_keep_mailbox_untrusted, b3_record_pending_recovery_gap]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for proof and source rule coverage.
  review_marker: token_safety

dependency_rules:

- b1 starts the path.
- b2 and b3 can be evaluated after b1 without waiting on each other.
- b4 waits on both b2 and b3.

parallel_groups:

- [b2_keep_mailbox_untrusted, b3_record_pending_recovery_gap]

wait_states:

- wait_for_pending_recovery_source_rules after b3_record_pending_recovery_gap.
- wait_for_trusted_continuation after b4_wait_for_trusted_continuation.

recovery_or_rejection_rules:

- Reject account activation from lost-password text alone.
- Keep the boundary visible for account-recovery learning.

planner_confidence: 0.72

next_node_handoff: send_non_boundary_items_to_processor_and_keep_boundary_flag_attached

---

## Pair 004 - Interrupted provider onboarding path

A:

path_name: Google signup interrupted during questionnaire onboarding

translated_domain_statements:

- statement_id: t_provider_identity
  domain_label: provider_identity_entry
  translated_domain_statement: The person started signup through Google. The provider identity must be verified before it can be used as account evidence.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_questionnaire_onboarding
  domain_label: questionnaire_onboarding
  translated_domain_statement: The person reached the onboarding questionnaire stage after provider-based registration began. Questionnaire progress belongs after account identity resolution, not before provider trust.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_resume_state_boundary
  domain_label: resume_state_boundary
  translated_domain_statement: The person closed the app before finishing and wants to continue from the saved point. Saved-state restoration after app closure is a boundary because the first local set does not fully define how onboarding progress is stored and restored.
  coverage_status: boundary
  boundary_marker: needs_resume_state_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_duplicate_prevention
  domain_label: duplicate_prevention
  translated_domain_statement: Continuing after interruption must reuse the known pending account or existing account for the same provider identity. Resume must not create a second account for the same provider identity.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_token_session_safety
  domain_label: token_session_safety
  translated_domain_statement: Continuation must rely on a still-trusted session or a fresh trusted action. Stale or expired trust material must stop unsafe continuation.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: mixed_plan_with_resume_boundary

path_name: Google signup interrupted during questionnaire onboarding

ordered_plan_items:

- task_id: b1_validate_or_refresh_provider_trust
  task_statement: Validate the provider trust material or require a fresh trusted provider action.
  transformational_prompt: Use provider trust only if it is still valid for the same sign-up attempt. If it is stale or missing, stop continuation and request a fresh trusted provider action.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if provider trust is not valid.
  review_marker: token_safety

- task_id: b2_resolve_existing_registration_state
  task_statement: Resolve the existing account or pending registration for the provider identity.
  transformational_prompt: Search for the account or pending registration tied to the verified provider identity. Reuse that state. Do not create a second account during resume.
  depends_on: [b1_validate_or_refresh_provider_trust]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if identity state cannot be resolved.
  review_marker: identity_consistency

- task_id: b3_check_questionnaire_stage
  task_statement: Check whether the resolved registration had reached questionnaire onboarding.
  transformational_prompt: Read the onboarding stage from the resolved registration state. The questionnaire stage can be used only after provider identity and account state are resolved.
  depends_on: [b2_resolve_existing_registration_state]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if no questionnaire stage is recorded.
  review_marker: none

- task_id: b4_flag_resume_state_gap
  task_statement: Preserve the saved-state restoration gap.
  transformational_prompt: Attach boundary_marker needs_resume_state_rules. Do not invent storage, restoration, or progress rules for onboarding after app closure.
  depends_on: [b3_check_questionnaire_stage]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for resume-state source rules before promising exact continuation behavior.
  review_marker: coverage_review

- task_id: b5_resume_only_after_rules_and_trust
  task_statement: Resume onboarding only when trust is valid and saved-state rules are available.
  transformational_prompt: Continue from the questionnaire point only if b1 confirms trust, b2 resolves the same account, b3 confirms questionnaire stage, and b4 has source rules for restoration. Otherwise return a boundary result.
  depends_on: [b1_validate_or_refresh_provider_trust, b2_resolve_existing_registration_state, b3_check_questionnaire_stage, b4_flag_resume_state_gap]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for resume-state rules or fresh trust.
  review_marker: token_safety

dependency_rules:

- b1 must happen before identity and state resolution.
- b2 must happen before questionnaire state can be trusted.
- b4 keeps the resume edge open as a boundary.
- b5 depends on every earlier item.

parallel_groups: []

wait_states:

- wait_for_resume_state_source_rules after b4_flag_resume_state_gap.
- wait_for_fresh_provider_trust if b1 fails.

recovery_or_rejection_rules:

- Reject any resume branch that creates a second account.
- Reject any resume promise that invents saved-state behavior without source rules.
- Ask for more resume-state source material if exact continuation behavior is required.

planner_confidence: 0.78

next_node_handoff: send_non_boundary_items_to_processor_and_keep_boundary_flag_attached

Reason: This path proves the planner can combine covered provider work with a boundary resume gap without cutting the resume edge.

## Pair 004 - Phrasing B

A:

path_name: continue Google onboarding after leaving the app

translated_domain_statements:

- statement_id: t_google_entry
  domain_label: provider_identity_entry
  translated_domain_statement: The registration began with Google provider entry. The Google identity must be verified again or still trusted before continuation.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_onboarding_questions
  domain_label: questionnaire_onboarding
  translated_domain_statement: The user had reached onboarding questions. That stage belongs after account identity was established.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_saved_progress_boundary
  domain_label: resume_state_boundary
  translated_domain_statement: The user left the app before finishing and wants saved progress restored. The local set does not fully teach saved-progress restoration, so this stays boundary-marked.
  coverage_status: boundary
  boundary_marker: needs_resume_state_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_reuse_registration
  domain_label: duplicate_prevention
  translated_domain_statement: Resume must reuse the existing account or pending registration for the same Google identity. It must not create another account.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_continuation_trust
  domain_label: token_session_safety
  translated_domain_statement: Continuation is safe only from valid current trust or from a new trusted action. Expired trust stops continuation.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

B:

plan_status: mixed_plan_with_resume_boundary

path_name: continue Google onboarding after leaving the app

ordered_plan_items:

- task_id: b1_confirm_google_trust
  task_statement: Confirm that Google trust for the sign-up attempt is still valid or refresh it.
  transformational_prompt: Use the provider identity only when trust is current and belongs to the same registration attempt. Request fresh provider entry if trust is stale.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if trust cannot be confirmed.
  review_marker: token_safety

- task_id: b2_reopen_same_registration
  task_statement: Reopen the same account or pending registration.
  transformational_prompt: Match the Google identity to existing state. Reuse that state and reject any branch that creates a new local account.
  depends_on: [b1_confirm_google_trust]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if no same-registration state can be found.
  review_marker: identity_consistency

- task_id: b3_find_questionnaire_progress
  task_statement: Find the questionnaire progress point for the reopened registration.
  transformational_prompt: Use questionnaire progress only from the registration state selected in b2. Do not accept progress from another account.
  depends_on: [b2_reopen_same_registration]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if no questionnaire progress is available.
  review_marker: none

- task_id: b4_keep_saved_progress_as_boundary
  task_statement: Keep saved-progress restoration as a boundary.
  transformational_prompt: Carry boundary_marker needs_resume_state_rules into the next node. Do not invent how progress is restored after app closure.
  depends_on: [b3_find_questionnaire_progress]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for saved-progress source rules.
  review_marker: coverage_review

- task_id: b5_continue_when_rules_exist
  task_statement: Continue onboarding only after same-registration state, valid trust, and saved-progress rules are all present.
  transformational_prompt: Require b1, b2, b3, and b4 before continuation. If b4 is unresolved, return a boundary result instead of a completed resume plan.
  depends_on: [b1_confirm_google_trust, b2_reopen_same_registration, b3_find_questionnaire_progress, b4_keep_saved_progress_as_boundary]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for saved-progress rules.
  review_marker: token_safety

dependency_rules:

- Trust comes before reopening state.
- Reopened state comes before questionnaire progress.
- Resume-state rules gate the final continuation.

parallel_groups: []

wait_states:

- wait_for_saved_progress_rules after b4_keep_saved_progress_as_boundary.

recovery_or_rejection_rules:

- Reject duplicate account creation.
- Request fresh trust if trust is stale.
- Return a boundary result if saved-progress rules are missing.

planner_confidence: 0.78

next_node_handoff: send_non_boundary_items_to_processor_and_keep_boundary_flag_attached

---

## Pair 005 - Knowledge-edge biometric passport path

A:

path_name: biometric passport verification during signup

translated_domain_statements:

- statement_id: t_identity_proofing
  domain_label: identity_proofing_knowledge_edge
  translated_domain_statement: Biometric passport verification is a missing competence. The local system knows provider sign-in, email verification, duplicate prevention, and token safety, but it does not know the rules for passport scanning, face matching, identity-proof assurance, privacy handling, or sensitive biometric data retention.
  coverage_status: knowledge_edge
  knowledge_edge_marker: missing_biometric_passport_verification
  handoff_marker: route_to_knowledge_edge_context_pack

- statement_id: t_continuation_trust
  domain_label: token_session_safety
  translated_domain_statement: Any identity-proof continuation would affect whether registration can safely continue, but the trust rules for biometric passport proof are not available in the local set.
  coverage_status: boundary
  boundary_marker: needs_identity_proofing_trust_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_review_required
  domain_label: arbiter_review_required
  translated_domain_statement: The request needs identity-proofing, privacy, and sensitive-data review before a local answer is accepted.
  coverage_status: knowledge_edge
  knowledge_edge_marker: missing_biometric_passport_verification
  handoff_marker: route_to_knowledge_edge_context_pack

B:

plan_status: blocked_for_knowledge_edge

path_name: biometric passport verification during signup

ordered_plan_items:

- task_id: b1_name_missing_competence
  task_statement: Name the missing competence as biometric passport verification for registration.
  transformational_prompt: State exactly what is missing: passport scanning, face matching, identity-proof assurance, privacy handling, and sensitive biometric data retention. Do not convert the request into a local implementation plan.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop local planning after the missing competence is named.
  review_marker: coverage_review

- task_id: b2_prepare_knowledge_edge_context_pack
  task_statement: Prepare a context pack for external or later learning help.
  transformational_prompt: Include the original request, the known local coverage, the missing biometric passport competence, the required answer shape, and the review constraints. Keep the context pack separate from the final answer path.
  depends_on: [b1_name_missing_competence]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for approved help or new source material.
  review_marker: privacy_and_identity_review

- task_id: b3_hold_registration_plan
  task_statement: Hold the registration implementation plan until the knowledge edge is resolved.
  transformational_prompt: Do not produce account-creation, session, or proofing steps from assumption. The only valid downstream work is the knowledge-edge packet and learning-gap record.
  depends_on: [b1_name_missing_competence, b2_prepare_knowledge_edge_context_pack]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for accepted external answer or future local training.
  review_marker: coverage_review

dependency_rules:

- b1 comes before any context packaging.
- b2 prepares help-seeking context only after b1 names the exact missing competence.
- b3 blocks ordinary planning until accepted knowledge arrives.

parallel_groups: []

wait_states:

- wait_for_user_approved_external_help_or_new_local_training after b2_prepare_knowledge_edge_context_pack.

recovery_or_rejection_rules:

- Reject any ordinary local plan that invents biometric passport verification.
- Preserve the request as a learning gap for idle-time training after an accepted answer exists.

planner_confidence: 0.44

next_node_handoff: route_to_knowledge_edge_context_pack

Reason: Knowledge-edge input must become a context-pack plan, not a fake registration design.

## Pair 005 - Phrasing B

A:

path_name: passport scan and face match before account continuation

translated_domain_statements:

- statement_id: t_passport_face_match
  domain_label: identity_proofing_knowledge_edge
  translated_domain_statement: Passport document proof and biometric face matching are beyond ordinary provider login and mailbox verification. The local Flow-01 set lacks identity-proofing authority, sensitive-data handling, and review rules for this proofing method.
  coverage_status: knowledge_edge
  knowledge_edge_marker: missing_biometric_passport_verification
  handoff_marker: route_to_knowledge_edge_context_pack

- statement_id: t_proof_based_continuation
  domain_label: token_session_safety
  translated_domain_statement: Signup continuation would depend on a new proofing result, but the local set does not define how that result becomes trusted.
  coverage_status: boundary
  boundary_marker: needs_identity_proofing_trust_rules
  handoff_marker: send_to_scheme_planner_with_boundary_flag

- statement_id: t_sensitive_review
  domain_label: arbiter_review_required
  translated_domain_statement: The request requires review for identity proofing, privacy, and sensitive personal data before it can be accepted.
  coverage_status: knowledge_edge
  knowledge_edge_marker: missing_biometric_passport_verification
  handoff_marker: route_to_knowledge_edge_context_pack

B:

plan_status: blocked_for_knowledge_edge

path_name: passport scan and face match before account continuation

ordered_plan_items:

- task_id: b1_state_uncovered_identity_proofing
  task_statement: State that passport scan and face match are uncovered identity-proofing work.
  transformational_prompt: Name passport scanning, face matching, assurance, privacy handling, and sensitive data retention as missing. Do not turn them into local steps.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop local proofing design here.
  review_marker: coverage_review

- task_id: b2_build_help_request_context
  task_statement: Build the context pack for the missing proofing competence.
  transformational_prompt: Include the user request, current Flow-01 coverage, the specific missing proofing work, expected answer shape, and identity/privacy review constraints.
  depends_on: [b1_state_uncovered_identity_proofing]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for approved help or new source authority.
  review_marker: privacy_and_identity_review

- task_id: b3_block_account_continuation_from_uncovered_proof
  task_statement: Block account continuation from the uncovered proofing request.
  transformational_prompt: Do not continue registration from biometric proof until accepted source authority defines how the proof is captured, trusted, reviewed, and retained.
  depends_on: [b1_state_uncovered_identity_proofing, b2_build_help_request_context]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for accepted proofing rules.
  review_marker: coverage_review

dependency_rules:

- Missing competence is named first.
- Context pack creation follows the named missing competence.
- Account continuation is blocked until accepted source authority exists.

parallel_groups: []

wait_states:

- wait_for_identity_proofing_source_authority after b2_build_help_request_context.

recovery_or_rejection_rules:

- Reject invented biometric proofing steps.
- Preserve the case as a learning gap after accepted help arrives.

planner_confidence: 0.42

next_node_handoff: route_to_knowledge_edge_context_pack

---

## Pair 006 - Duplicate account conflict path

A:

path_name: repeated provider callback asks for second account

translated_domain_statements:

- statement_id: t_provider_callback
  domain_label: provider_identity_entry
  translated_domain_statement: The prompt refers to the provider callback that returns identity information. The provider identity must be treated as identity evidence only after validation.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_duplicate_conflict
  domain_label: duplicate_prevention
  translated_domain_statement: A repeated provider callback for the same provider identity must resolve to the existing account or pending registration, not create a second local account. The user's instruction asks for the forbidden result. Translate the conflict explicitly so the planner and arbiters see that the request is not an accepted duplicate-creation requirement.
  coverage_status: conflict
  conflict_marker: duplicate_account_creation_requested
  handoff_marker: send_to_scheme_planner_with_rejection_flag

- statement_id: t_arbiter_review
  domain_label: arbiter_review_required
  translated_domain_statement: The duplicate-creation instruction must be rejected or sent back for reprocessing. Similar registration examples cannot override identity consistency.
  coverage_status: covered_conflict
  conflict_marker: duplicate_account_creation_requested
  handoff_marker: send_to_scheme_planner_with_rejection_flag

B:

plan_status: rejected_conflict_plan

path_name: repeated provider callback asks for second account

ordered_plan_items:

- task_id: b1_validate_callback_identity
  task_statement: Validate the repeated provider callback enough to identify the provider identity involved.
  transformational_prompt: Use validation only to identify the repeated provider identity. Do not use the callback to create an account.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the provider identity cannot be established.
  review_marker: token_safety

- task_id: b2_find_existing_or_pending_account
  task_statement: Find the existing account or pending registration for the provider identity.
  transformational_prompt: Search for the provider identity and any verified mailbox tied to it. The goal is reuse or continuation, not creation.
  depends_on: [b1_validate_callback_identity]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if the identity match is ambiguous.
  review_marker: identity_consistency

- task_id: b3_reject_second_account_instruction
  task_statement: Reject the requested second account creation.
  transformational_prompt: Carry conflict_marker duplicate_account_creation_requested. The planned action is rejection of duplicate creation, not execution of duplicate creation.
  depends_on: [b2_find_existing_or_pending_account]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop all account creation branches for this repeated identity.
  review_marker: identity_consistency

- task_id: b4_route_conflict_to_arbiter
  task_statement: Route the conflict result to identity-consistency arbitration.
  transformational_prompt: Ask the arbiter to verify that no branch creates a second account and that the existing account or pending registration is preserved.
  depends_on: [b3_reject_second_account_instruction]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for arbiter verdict before any final answer.
  review_marker: identity_consistency

dependency_rules:

- b1 and b2 are allowed only to identify the existing state.
- b3 rejects the forbidden instruction.
- b4 makes the rejection visible to arbiter review.

parallel_groups: []

wait_states:

- wait_for_identity_consistency_arbiter after b4_route_conflict_to_arbiter.

recovery_or_rejection_rules:

- Reject any branch that creates a second account from the repeated provider identity.
- Reprocess if a downstream node turns the conflict into a create-account task.

planner_confidence: 0.95

next_node_handoff: send_conflict_plan_to_processor_with_rejection_flag

Reason: The planner must not launder a conflict translation into an accepted creation plan.

## Pair 006 - Phrasing B

A:

path_name: repeated Google response asks to make a new user

translated_domain_statements:

- statement_id: t_google_response
  domain_label: provider_identity_entry
  translated_domain_statement: The prompt describes a repeated Google signup response. The response is part of provider registration and must be validated only as identity evidence.
  coverage_status: covered
  handoff_marker: ready_for_scheme_planner

- statement_id: t_new_user_conflict
  domain_label: duplicate_prevention
  translated_domain_statement: When the same external identity appears again, the flow must search for and reuse the existing account or pending registration. Creating a new user from the repeated identity is the rejected branch.
  coverage_status: conflict
  conflict_marker: duplicate_account_creation_requested
  handoff_marker: send_to_scheme_planner_with_rejection_flag

- statement_id: t_conflict_review
  domain_label: arbiter_review_required
  translated_domain_statement: The instruction conflicts with identity consistency and must not pass downstream as accepted work.
  coverage_status: covered_conflict
  conflict_marker: duplicate_account_creation_requested
  handoff_marker: send_to_scheme_planner_with_rejection_flag

B:

plan_status: rejected_conflict_plan

path_name: repeated Google response asks to make a new user

ordered_plan_items:

- task_id: b1_confirm_repeated_google_identity
  task_statement: Confirm the repeated Google response identifies the same external identity.
  transformational_prompt: Validate the response only to establish identity sameness. Do not treat the repeated response as permission to create another user.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if identity cannot be confirmed.
  review_marker: token_safety

- task_id: b2_locate_existing_registration_owner
  task_statement: Locate the account or pending registration that already owns the external identity.
  transformational_prompt: Reuse ownership evidence from the same Google identity. Do not create a second owner.
  depends_on: [b1_confirm_repeated_google_identity]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop if ownership is unclear.
  review_marker: identity_consistency

- task_id: b3_block_new_user_branch
  task_statement: Block the requested new-user branch.
  transformational_prompt: Carry conflict_marker duplicate_account_creation_requested and convert the unsafe requested action into a rejection item.
  depends_on: [b2_locate_existing_registration_owner]
  allowed_parallel_with: []
  stop_or_wait_condition: Stop all duplicate creation work.
  review_marker: identity_consistency

- task_id: b4_send_to_identity_arbiter
  task_statement: Send the conflict to identity-consistency review.
  transformational_prompt: Require the arbiter to confirm that the same external identity maps to one local account path and that the new-user branch is rejected.
  depends_on: [b3_block_new_user_branch]
  allowed_parallel_with: []
  stop_or_wait_condition: Wait for identity-consistency review.
  review_marker: identity_consistency

dependency_rules:

- Identity confirmation comes first.
- Existing owner search comes before rejection routing.
- The new-user branch is never sent as executable work.

parallel_groups: []

wait_states:

- wait_for_identity_consistency_review after b4_send_to_identity_arbiter.

recovery_or_rejection_rules:

- Reject any downstream result that creates the new user.
- Reprocess if duplicate creation appears after b3.

planner_confidence: 0.95

next_node_handoff: send_conflict_plan_to_processor_with_rejection_flag

---

## Pair 007 - Clarification-only path

A:

path_name: no semantic registration request

translated_domain_statements:

- statement_id: t_clarification
  domain_label: user_clarification
  translated_domain_statement: The packet contains no actionable registration intent. The system has no subject, no trusted user goal, and no registration path to route. The next step is clarification from the user. No downstream registration plan should be created from this packet.
  coverage_status: clarification_required
  clarification_marker: no_semantic_request
  handoff_marker: ask_user_for_registration_request

B:

plan_status: clarification_required

path_name: no semantic registration request

ordered_plan_items:

- task_id: b1_ask_for_registration_request
  task_statement: Ask the user to provide a meaningful registration request.
  transformational_prompt: Do not create registration, verification, session, policy, identity, or onboarding tasks. Return a clarification request because the input has no semantic work.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop all planning until the user provides a real request.
  review_marker: none

dependency_rules:

- b1 is the only valid item.

parallel_groups: []

wait_states:

- wait_for_user_registration_request after b1_ask_for_registration_request.

recovery_or_rejection_rules:

- Reject any downstream plan that invents registration work from punctuation or blank text.

planner_confidence: 1.00

next_node_handoff: ask_user_for_registration_request

Reason: The planner must stop on no-semantic input instead of inventing a flow.

## Pair 007 - Phrasing B

A:

path_name: empty task for registration flow

translated_domain_statements:

- statement_id: t_empty_task
  domain_label: user_clarification
  translated_domain_statement: The input has no meaningful task for the registration flow. No domain work can be translated because the packet does not contain a user intention, registration subject, or requested action. The only safe output is a clarification requirement asking the user to provide a real request.
  coverage_status: clarification_required
  clarification_marker: no_semantic_request
  handoff_marker: ask_user_for_registration_request

B:

plan_status: clarification_required

path_name: empty task for registration flow

ordered_plan_items:

- task_id: b1_request_real_user_goal
  task_statement: Request a real registration goal from the user.
  transformational_prompt: Do not plan any account, provider, verification, token, policy, or onboarding work. The correct next step is to ask the user what they want to do.
  depends_on: []
  allowed_parallel_with: []
  stop_or_wait_condition: Stop until the user supplies a meaningful request.
  review_marker: none

dependency_rules:

- b1 is the only valid item.

parallel_groups: []

wait_states:

- wait_for_meaningful_user_goal after b1_request_real_user_goal.

recovery_or_rejection_rules:

- Reject invented registration tasks from empty input.

planner_confidence: 1.00

next_node_handoff: ask_user_for_registration_request
