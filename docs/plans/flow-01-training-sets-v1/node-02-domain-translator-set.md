# Node 02 - Domain Translator Set

## Purpose

This file teaches the Flow-01 domain translator array.

Node 02 receives one domain packet at a time from node 01. It does not receive the whole split array as one large input. Its output is one formal domain statement that the scheme planner can later combine with other translated statements.

The output is the actual translated statement, not a lesson about translation quality.

## Coverage Types Included

This first node-02 set includes:

- covered packets;
- boundary packets;
- knowledge-edge packets;
- conflict packets.

---

## Pair 001 - Covered provider identity entry

A:

domain_label: provider_identity_entry

prompt_fragment: The person wants to begin registration through Google as an external identity provider.

coverage_status: covered

reason_for_inclusion: Google sign-up depends on trusting the provider identity result.

B:

translated_domain_statement:

The registration entry point is an external identity-provider flow. The provider is Google. The system must treat the provider response as an identity assertion that is not trusted until verified. The translated work statement is: receive the provider result, validate that the result belongs to the expected provider flow, extract the provider identity, normalize the returned email or stable provider subject, and expose the verified identity result to the account-resolution step. This statement does not decide whether to create or link an account; it only prepares the trusted provider identity for the planner.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

Reason: Node 02 turns one provider-entry packet into formal identity-provider language while avoiding account creation planning.

## Pair 001 - Phrasing B

A:

domain_label: provider_identity_entry

prompt_fragment: The person wants provider-based entry instead of password-first registration.

coverage_status: covered

reason_for_inclusion: The request names Google profile as the entry authority.

B:

translated_domain_statement:

The requested registration entry path uses a third-party identity provider instead of local password-first credentials. The provider identity must be verified before it can be used as local account evidence. The formal statement is: accept the provider callback or provider profile only as an untrusted identity claim at first, validate that it belongs to the expected sign-up attempt, derive the stable provider identity, and pass the verified identity claim forward for account matching. No local account should be created by this translation step.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

---

## Pair 002 - Covered email verification

A:

domain_label: email_verification

prompt_fragment: The person needs a verification link sent to the registered email address.

coverage_status: covered

reason_for_inclusion: Sending and later validating the verification link is the central request.

B:

translated_domain_statement:

The email registration path requires mailbox-ownership verification. The formal statement is: identify the pending registration account, create a time-limited verification action bound to that pending account and mailbox, send the verification message to the registered email address, and treat the account as pending until the verification action returns and is validated. Sending the message is not the same as completing verification.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

Reason: Node 02 must preserve the wait between message delivery and verified account access.

## Pair 002 - Phrasing B

A:

domain_label: email_verification

prompt_fragment: The person expects an email confirmation link after account creation.

coverage_status: covered

reason_for_inclusion: Confirmation link delivery and later verification are part of the email path.

B:

translated_domain_statement:

The account may be created in a pending state, but mailbox trust is not complete until the confirmation link is used and accepted. The formal statement is: produce a confirmation action for the pending account, deliver it to the registered mailbox, keep the account in pending-verification state, and later validate the returned confirmation action before granting normal account access. The translator must not collapse delivery and verification into one step.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

---

## Pair 003 - Covered token session safety

A:

domain_label: token_session_safety

prompt_fragment: The registration token expires before signup is complete.

coverage_status: covered

reason_for_inclusion: The central issue is whether expired trust material can complete registration.

B:

translated_domain_statement:

The registration continuation depends on temporary trust material that has expired. The formal statement is: validate the token or session proof before using it, reject the continuation when the token is expired or invalid, do not complete the account from that token, do not start a registered session from that token, and request a fresh trusted sign-in or verification action. Expiration is a stop condition, not a warning.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

Reason: Token safety must be translated as a hard continuation rule.

## Pair 003 - Phrasing B

A:

domain_label: token_session_safety

prompt_fragment: The signup session token is invalid before account completion.

coverage_status: covered

reason_for_inclusion: Account completion must not rely on invalid session proof.

B:

translated_domain_statement:

The account-completion step cannot rely on invalid session proof. The formal statement is: check the session or token before completing registration, stop the completion path if the proof is invalid, preserve the account as incomplete or pending, avoid issuing a completed-session result, and ask the person to repeat a trusted entry action. The invalid proof must not be repaired by assumption.

coverage_status: covered

handoff_marker: ready_for_scheme_planner

---

## Pair 004 - Boundary credential recovery

A:

domain_label: credential_recovery_boundary

prompt_fragment: The person forgot the password before registration completed.

coverage_status: boundary

reason_for_inclusion: Password recovery during pending registration is not fully taught by the first Flow-01 set.

B:

translated_domain_statement:

The packet describes recovery during an unfinished registration, not ordinary recovery for a completed account. The formal statement is: preserve the pending registration state, mark password recovery as a boundary that needs account-recovery source rules, do not grant normal account access from the recovery request, and require mailbox ownership or another trusted proof before allowing continuation. The translator may express the boundary, but it must not invent the full recovery design.

coverage_status: boundary

boundary_marker: needs_account_recovery_rules

handoff_marker: send_to_scheme_planner_with_boundary_flag

Reason: Boundary packets are translated with their missing rule attached, not treated as clean covered work.

## Pair 004 - Phrasing B

A:

domain_label: credential_recovery_boundary

prompt_fragment: The person needs help because the selected password is no longer available to them.

coverage_status: boundary

reason_for_inclusion: This is recovery during pending registration, not ordinary completed-account recovery.

B:

translated_domain_statement:

The request is a pending-registration credential recovery case. The formal statement is: keep the registration incomplete, represent the password problem as a recovery boundary, require a trusted ownership step before any continuation, and prevent the recovery request from becoming account activation by itself. The missing rule is the exact recovery path for a user whose account has not yet been verified.

coverage_status: boundary

boundary_marker: needs_account_recovery_rules

handoff_marker: send_to_scheme_planner_with_boundary_flag

---

## Pair 005 - Boundary policy acceptance

A:

domain_label: policy_acceptance_boundary

prompt_fragment: The person says registration completion depends on accepting terms and privacy policy.

coverage_status: boundary

reason_for_inclusion: Flow-01 registration is covered, but the first local set does not fully teach consent capture and policy evidence.

B:

translated_domain_statement:

The registration flow has a completion condition based on user acceptance of terms and privacy policy. The formal statement is: account completion is blocked until the acceptance event is recorded, the acceptance record must identify the user or pending account, the policy versions accepted, and the time of acceptance, and the planner must treat this as a policy-evidence boundary because the current local set does not fully teach consent capture rules.

coverage_status: boundary

boundary_marker: needs_policy_acceptance_rules

handoff_marker: send_to_scheme_planner_with_boundary_flag

Reason: Policy acceptance is part of registration completion but needs separate source rules.

## Pair 005 - Phrasing B

A:

domain_label: policy_acceptance_boundary

prompt_fragment: The person blocks account completion until terms and privacy notice agreement is recorded.

coverage_status: boundary

reason_for_inclusion: The splitter must preserve this as a completion condition, not hide it inside generic registration.

B:

translated_domain_statement:

The account cannot move to completed-registration state until agreement evidence exists. The formal statement is: require a recorded acceptance action before completion, bind that action to the pending account, include the accepted terms and privacy notice versions, and keep the flow marked as boundary-covered until the policy-evidence rules are supplied. The translator must not silently convert agreement into a generic checkbox.

coverage_status: boundary

boundary_marker: needs_policy_acceptance_rules

handoff_marker: send_to_scheme_planner_with_boundary_flag

---

## Pair 006 - Knowledge-edge identity proofing

A:

domain_label: identity_proofing_knowledge_edge

prompt_fragment: The person asks to add biometric passport verification to signup.

coverage_status: knowledge_edge

reason_for_inclusion: Passport and biometric proofing are not covered by the local registration set.

B:

translated_domain_statement:

The packet asks for identity proofing that is outside the covered Flow-01 registration set. The formal statement is: biometric passport verification is a missing competence; the local system knows provider sign-in, email verification, duplicate prevention, and token safety, but it does not know the rules for passport scanning, face matching, identity-proof assurance, privacy handling, or sensitive biometric data retention. The translated output must request a knowledge-edge path, not create a local implementation plan.

coverage_status: knowledge_edge

knowledge_edge_marker: missing_biometric_passport_verification

handoff_marker: route_to_knowledge_edge_context_pack

Reason: Knowledge-edge packets must be translated into explicit missing competence.

## Pair 006 - Phrasing B

A:

domain_label: identity_proofing_knowledge_edge

prompt_fragment: The registration path adds passport scan and face matching.

coverage_status: knowledge_edge

reason_for_inclusion: This is identity proofing beyond provider sign-in and email verification.

B:

translated_domain_statement:

The requested registration step introduces passport document proof and biometric face matching. The formal statement is: this is not ordinary provider login or mailbox verification; it requires identity-proofing authority, sensitive-data handling, and review rules that are not present in the local Flow-01 set. The correct translation is a named knowledge edge for biometric passport verification, with the downstream action set to build or request a context pack rather than invent the answer.

coverage_status: knowledge_edge

knowledge_edge_marker: missing_biometric_passport_verification

handoff_marker: route_to_knowledge_edge_context_pack

---

## Pair 007 - Conflict duplicate prevention

A:

domain_label: duplicate_prevention

prompt_fragment: The prompt asks to create a second account for the same repeated provider callback.

coverage_status: conflict

reason_for_inclusion: This directly contradicts the duplicate-prevention rule.

B:

translated_domain_statement:

The packet contains a conflict with identity consistency. The formal statement is: a repeated provider callback for the same provider identity must resolve to the existing account or pending registration, not create a second local account. The user's instruction asks for the forbidden result. Translate the conflict explicitly so the planner and arbiters see that the request is not an accepted duplicate-creation requirement.

coverage_status: conflict

conflict_marker: duplicate_account_creation_requested

handoff_marker: send_to_scheme_planner_with_rejection_flag

Reason: The translator must translate the conflict, not obey the unsafe instruction.

## Pair 007 - Phrasing B

A:

domain_label: duplicate_prevention

prompt_fragment: The prompt asks to make a new user rather than reuse the existing account.

coverage_status: conflict

reason_for_inclusion: A repeated provider identity should not create another account.

B:

translated_domain_statement:

The packet asks the flow to violate duplicate prevention. The formal statement is: when the same external identity appears again, the flow must search for and reuse the existing account or pending registration. Creating a new user from the repeated identity is the rejected branch. Preserve this as a conflict marker for later arbiter review; do not translate it as a valid creation task.

coverage_status: conflict

conflict_marker: duplicate_account_creation_requested

handoff_marker: send_to_scheme_planner_with_rejection_flag

---

## Pair 008 - Clarification required

A:

domain_label: user_clarification

prompt_fragment: The input contains punctuation but no usable registration request.

coverage_status: clarification_required

reason_for_inclusion: Blankness means absence of semantic content, not only absence of characters.

B:

translated_domain_statement:

The packet contains no actionable registration intent. The formal statement is: the system has no subject, no trusted user goal, and no registration path to route. The next step is clarification from the user. No downstream registration plan should be created from this packet.

coverage_status: clarification_required

clarification_marker: no_semantic_request

handoff_marker: ask_user_for_registration_request

Reason: The translator must keep non-semantic input from becoming invented work.

## Pair 008 - Phrasing B

A:

domain_label: user_clarification

prompt_fragment: No meaningful user request was provided.

coverage_status: clarification_required

reason_for_inclusion: The flow must not invent a registration subject from blank input.

B:

translated_domain_statement:

The input has no meaningful task for the registration flow. The formal statement is: no domain work can be translated because the packet does not contain a user intention, registration subject, or requested action. The only safe output is a clarification requirement asking the user to provide a real request.

coverage_status: clarification_required

clarification_marker: no_semantic_request

handoff_marker: ask_user_for_registration_request
