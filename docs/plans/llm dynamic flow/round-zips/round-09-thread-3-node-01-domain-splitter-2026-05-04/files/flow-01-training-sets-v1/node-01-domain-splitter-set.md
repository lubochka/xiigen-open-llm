# Node 01 - Domain Splitter Set

## Purpose

This file teaches the Flow-01 domain splitter.

Node 01 receives the checked logical structure from node 00, together with the original user prompt for human traceability. It emits an array of domain packets. Each packet is one element that the domain translator array can process independently.

The output is the actual split result, not a lesson about the splitter's job.

## Domain Labels Used In This First Set

- provider_identity_entry
- email_registration_entry
- email_verification
- credential_safety
- credential_recovery_boundary
- token_session_safety
- duplicate_prevention
- policy_acceptance_boundary
- identity_proofing_knowledge_edge
- resume_state_boundary
- questionnaire_onboarding
- user_clarification
- arbiter_review_required

---

## Pair 001 - Email registration

A:

original_prompt: I want to create an account with my email address and a password.

compiled_subject: flow-01 user registration

local_subject: email_registration_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0]

normalized_roots: [create, account, email, password, registration]

missing_coverage_signal: covered

B:

domain_labels: [email_registration_entry, credential_safety, email_verification, token_session_safety]

domain_packets:

- domain_label: email_registration_entry
  prompt_fragment: The person wants to create a new account using an email address.
  coverage_status: covered
  reason_for_inclusion: The prompt explicitly asks to create an account through the email path.

- domain_label: credential_safety
  prompt_fragment: The person will choose or use a password during registration.
  coverage_status: covered
  reason_for_inclusion: Password handling is part of the registration request and must not be hidden inside account creation.

- domain_label: email_verification
  prompt_fragment: The email address will need ownership verification before the account is fully trusted.
  coverage_status: covered
  reason_for_inclusion: Flow-01 separates email registration from email verification.

- domain_label: token_session_safety
  prompt_fragment: A completed registration should only start a session after the email and credential conditions are safe.
  coverage_status: covered
  reason_for_inclusion: Registration completion leads to a session, so session safety is touched.

split_confidence: 0.91

Reason: The prompt touches account creation, password safety, email verification, and eventual session start.

## Pair 001 - Phrasing B

A:

original_prompt: Let me register by entering my email and choosing a password.

compiled_subject: flow-01 user registration

local_subject: email_registration_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0]

normalized_roots: [register, email, password, account]

missing_coverage_signal: covered

B:

domain_labels: [email_registration_entry, credential_safety, email_verification, token_session_safety]

domain_packets:

- domain_label: email_registration_entry
  prompt_fragment: The person wants to enter the registration flow through email.
  coverage_status: covered
  reason_for_inclusion: Email registration is the chosen entry path.

- domain_label: credential_safety
  prompt_fragment: The person will create password credentials.
  coverage_status: covered
  reason_for_inclusion: Password creation is a safety-sensitive part of the request.

- domain_label: email_verification
  prompt_fragment: The mailbox must later be confirmed before full account trust.
  coverage_status: covered
  reason_for_inclusion: The email path requires verification before normal account access.

- domain_label: token_session_safety
  prompt_fragment: Session start must wait until registration is safely completed.
  coverage_status: covered
  reason_for_inclusion: The registration path eventually produces a logged-in session.

split_confidence: 0.91

---

## Pair 002 - Google provider registration

A:

original_prompt: I want to sign up using my Google account.

compiled_subject: flow-01 user registration

local_subject: provider_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [sign, provider, account, identity, registration]

missing_coverage_signal: covered

B:

domain_labels: [provider_identity_entry, duplicate_prevention, token_session_safety]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The person wants to begin registration through Google as an external identity provider.
  coverage_status: covered
  reason_for_inclusion: Google sign-up depends on trusting the provider identity result.

- domain_label: duplicate_prevention
  prompt_fragment: The provider identity must be checked against existing linked accounts before a new account is created.
  coverage_status: covered
  reason_for_inclusion: Provider entry can collide with an existing account or repeated callback.

- domain_label: token_session_safety
  prompt_fragment: Provider tokens and the resulting session must be safe before registration completes.
  coverage_status: covered
  reason_for_inclusion: Provider sign-up uses token-bearing identity proof.

split_confidence: 0.94

Reason: Provider registration naturally splits into provider trust, duplicate prevention, and token/session safety.

## Pair 002 - Phrasing B

A:

original_prompt: Can I join with my Google profile instead of making a password first?

compiled_subject: flow-01 user registration

local_subject: provider_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [join, provider, account, password, identity]

missing_coverage_signal: covered

B:

domain_labels: [provider_identity_entry, duplicate_prevention, token_session_safety]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The person wants provider-based entry instead of password-first registration.
  coverage_status: covered
  reason_for_inclusion: The request names Google profile as the entry authority.

- domain_label: duplicate_prevention
  prompt_fragment: The Google profile must be matched against existing accounts before creating anything new.
  coverage_status: covered
  reason_for_inclusion: The same external identity should resolve to one local account.

- domain_label: token_session_safety
  prompt_fragment: The provider result must be valid before the system starts a session.
  coverage_status: covered
  reason_for_inclusion: The prompt replaces password-first entry with token-based provider trust.

split_confidence: 0.94

---

## Pair 003 - Email verification link

A:

original_prompt: I registered with email; send me a verification link.

compiled_subject: flow-01 user registration

local_subject: email_verification

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [register, email, send, verification, link, account]

missing_coverage_signal: covered

B:

domain_labels: [email_verification, token_session_safety]

domain_packets:

- domain_label: email_verification
  prompt_fragment: The person needs a verification link sent to the registered email address.
  coverage_status: covered
  reason_for_inclusion: Sending and later validating the verification link is the central request.

- domain_label: token_session_safety
  prompt_fragment: The verification link must remain valid, single-purpose, and safe before it can activate account access.
  coverage_status: covered
  reason_for_inclusion: Verification links behave like temporary trust tokens.

split_confidence: 0.93

Reason: The prompt is mainly email verification, with token safety attached to the link.

## Pair 003 - Phrasing B

A:

original_prompt: After I make an account with email, I need the system to email me the confirmation link.

compiled_subject: flow-01 user registration

local_subject: email_verification

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [account, email, confirmation, link, verify]

missing_coverage_signal: covered

B:

domain_labels: [email_verification, token_session_safety]

domain_packets:

- domain_label: email_verification
  prompt_fragment: The person expects an email confirmation link after account creation.
  coverage_status: covered
  reason_for_inclusion: Confirmation link delivery and later verification are part of the email path.

- domain_label: token_session_safety
  prompt_fragment: The confirmation link must be checked before it grants normal account access.
  coverage_status: covered
  reason_for_inclusion: The link proves mailbox ownership only if it is safe and valid.

split_confidence: 0.93

---

## Pair 004 - Password recovery during unfinished registration

A:

original_prompt: I started registering with email, forgot the password I chose, and still have not verified my email.

compiled_subject: flow-01 user registration

local_subject: registration_recovery_boundary

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [start, register, email, password, forgot, verify, pending]

missing_coverage_signal: boundary_needs_account_recovery_rules

B:

domain_labels: [email_registration_entry, credential_recovery_boundary, email_verification, token_session_safety]

domain_packets:

- domain_label: email_registration_entry
  prompt_fragment: The person has already started an email registration attempt.
  coverage_status: covered
  reason_for_inclusion: The problem belongs to an unfinished email registration path.

- domain_label: credential_recovery_boundary
  prompt_fragment: The person forgot the password before registration completed.
  coverage_status: boundary
  reason_for_inclusion: Password recovery during pending registration is not fully taught by the first Flow-01 set.

- domain_label: email_verification
  prompt_fragment: The email address is still unverified.
  coverage_status: covered
  reason_for_inclusion: The account cannot be treated as fully trusted yet.

- domain_label: token_session_safety
  prompt_fragment: Any recovery or continuation link must not grant access before the mailbox is trusted.
  coverage_status: covered
  reason_for_inclusion: Recovery and verification depend on temporary trust material.

split_confidence: 0.72

Reason: The splitter must separate covered registration facts from the account-recovery boundary.

## Pair 004 - Phrasing B

A:

original_prompt: I was signing up by email, I lost the password I entered, and my email confirmation is not done yet.

compiled_subject: flow-01 user registration

local_subject: registration_recovery_boundary

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [sign, email, lost, password, confirmation, pending]

missing_coverage_signal: boundary_needs_account_recovery_rules

B:

domain_labels: [email_registration_entry, credential_recovery_boundary, email_verification, token_session_safety]

domain_packets:

- domain_label: email_registration_entry
  prompt_fragment: The person is inside an email sign-up flow.
  coverage_status: covered
  reason_for_inclusion: Email sign-up is the active registration path.

- domain_label: credential_recovery_boundary
  prompt_fragment: The person needs help because the selected password is no longer available to them.
  coverage_status: boundary
  reason_for_inclusion: This is recovery during pending registration, not ordinary completed-account recovery.

- domain_label: email_verification
  prompt_fragment: The email confirmation step is still incomplete.
  coverage_status: covered
  reason_for_inclusion: The flow must preserve the unverified mailbox state.

- domain_label: token_session_safety
  prompt_fragment: Any continuation must use a safe verification or recovery action.
  coverage_status: covered
  reason_for_inclusion: The person must not receive normal access from an untrusted recovery shortcut.

split_confidence: 0.72

---

## Pair 005 - Terms and conditions confirmation

A:

original_prompt: Before finishing signup, I need to accept the terms and privacy policy.

compiled_subject: flow-01 user registration

local_subject: registration_completion_condition

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0]

normalized_roots: [finish, signup, accept, terms, privacy, condition]

missing_coverage_signal: boundary_needs_policy_acceptance_rules

B:

domain_labels: [policy_acceptance_boundary]

domain_packets:

- domain_label: policy_acceptance_boundary
  prompt_fragment: The person says registration completion depends on accepting terms and privacy policy.
  coverage_status: boundary
  reason_for_inclusion: Flow-01 registration is covered, but the first local set does not fully teach consent capture and policy evidence.

split_confidence: 0.69

Reason: The prompt is part of registration completion but needs policy-acceptance rules before planning.

## Pair 005 - Phrasing B

A:

original_prompt: Do not complete my account until I agree to the terms and the privacy notice.

compiled_subject: flow-01 user registration

local_subject: registration_completion_condition

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0]

normalized_roots: [complete, account, agree, terms, privacy, condition]

missing_coverage_signal: boundary_needs_policy_acceptance_rules

B:

domain_labels: [policy_acceptance_boundary]

domain_packets:

- domain_label: policy_acceptance_boundary
  prompt_fragment: The person blocks account completion until terms and privacy notice agreement is recorded.
  coverage_status: boundary
  reason_for_inclusion: The splitter must preserve this as a completion condition, not hide it inside generic registration.

split_confidence: 0.69

---

## Pair 006 - Biometric passport verification knowledge edge

A:

original_prompt: I want biometric passport verification during signup.

compiled_subject: flow-01 user registration

local_subject: knowledge_edge_identity_proofing

predicate_counts_22: [0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0]

normalized_roots: [biometric, passport, verification, signup, identity, proofing]

missing_coverage_signal: knowledge_edge_missing_biometric_passport_verification

B:

domain_labels: [identity_proofing_knowledge_edge, token_session_safety, arbiter_review_required]

domain_packets:

- domain_label: identity_proofing_knowledge_edge
  prompt_fragment: The person asks to add biometric passport verification to signup.
  coverage_status: knowledge_edge
  reason_for_inclusion: Passport and biometric proofing are not covered by the local registration set.

- domain_label: token_session_safety
  prompt_fragment: Any identity-proof continuation would affect whether registration can safely continue.
  coverage_status: boundary
  reason_for_inclusion: Stronger identity proofing changes what counts as a trusted continuation.

- domain_label: arbiter_review_required
  prompt_fragment: The request needs identity-proofing, privacy, and sensitive-data review before a local answer is accepted.
  coverage_status: knowledge_edge
  reason_for_inclusion: The local flow must ask for help or source authority rather than invent biometric rules.

split_confidence: 0.44

Reason: This is the knowledge-edge case. The splitter must still emit useful packets, but mark the missing competence clearly.

## Pair 006 - Phrasing B

A:

original_prompt: During account creation, require a passport scan and face match before the user can continue.

compiled_subject: flow-01 user registration

local_subject: knowledge_edge_identity_proofing

predicate_counts_22: [0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0]

normalized_roots: [account, creation, passport, scan, face, match, continue]

missing_coverage_signal: knowledge_edge_missing_biometric_passport_verification

B:

domain_labels: [identity_proofing_knowledge_edge, token_session_safety, arbiter_review_required]

domain_packets:

- domain_label: identity_proofing_knowledge_edge
  prompt_fragment: The registration path adds passport scan and face matching.
  coverage_status: knowledge_edge
  reason_for_inclusion: This is identity proofing beyond provider sign-in and email verification.

- domain_label: token_session_safety
  prompt_fragment: The continuation point depends on a new kind of trusted identity proof.
  coverage_status: boundary
  reason_for_inclusion: The proofing result would decide whether signup may continue.

- domain_label: arbiter_review_required
  prompt_fragment: The system needs review for biometric identity proofing and sensitive personal data before accepting an answer.
  coverage_status: knowledge_edge
  reason_for_inclusion: The local set lacks the required authority for this proofing method.

split_confidence: 0.42

---

## Pair 007 - Duplicate account creation rejection

A:

original_prompt: If the same provider callback arrives twice, create a second account anyway.

compiled_subject: flow-01 user registration

local_subject: duplicate_prevention_conflict

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [provider, callback, duplicate, create, second, account]

missing_coverage_signal: covered_conflict_requires_arbiter_rejection

B:

domain_labels: [provider_identity_entry, duplicate_prevention, arbiter_review_required]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The prompt refers to the provider callback that returns identity information.
  coverage_status: covered
  reason_for_inclusion: Callback handling belongs to provider registration.

- domain_label: duplicate_prevention
  prompt_fragment: The prompt asks to create a second account for the same repeated provider callback.
  coverage_status: conflict
  reason_for_inclusion: This directly contradicts the duplicate-prevention rule.

- domain_label: arbiter_review_required
  prompt_fragment: The duplicate-creation instruction must be rejected or sent back for reprocessing.
  coverage_status: covered_conflict
  reason_for_inclusion: Similar registration examples cannot override identity consistency.

split_confidence: 0.95

Reason: The splitter must preserve both the provider-callback subject and the conflict that requires arbiter review.

## Pair 007 - Phrasing B

A:

original_prompt: When Google sends the same signup response again, make a new user instead of reusing the old one.

compiled_subject: flow-01 user registration

local_subject: duplicate_prevention_conflict

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [provider, signup, response, duplicate, new, user, reuse]

missing_coverage_signal: covered_conflict_requires_arbiter_rejection

B:

domain_labels: [provider_identity_entry, duplicate_prevention, arbiter_review_required]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The prompt describes a repeated Google signup response.
  coverage_status: covered
  reason_for_inclusion: The response is part of the provider registration path.

- domain_label: duplicate_prevention
  prompt_fragment: The prompt asks to make a new user rather than reuse the existing account.
  coverage_status: conflict
  reason_for_inclusion: A repeated provider identity should not create another account.

- domain_label: arbiter_review_required
  prompt_fragment: The instruction conflicts with identity consistency and must not pass downstream as accepted work.
  coverage_status: covered_conflict
  reason_for_inclusion: The arbiter-authority rule must see this conflict explicitly.

split_confidence: 0.95

---

## Pair 008 - Blank input

A:

original_prompt:

compiled_subject: none

local_subject: blank_input

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

normalized_roots: []

missing_coverage_signal: blank_input_requires_user_clarification

B:

domain_labels: [user_clarification]

domain_packets:

- domain_label: user_clarification
  prompt_fragment: No meaningful user request was provided.
  coverage_status: clarification_required
  reason_for_inclusion: The flow must not invent a registration subject from blank input.

split_confidence: 1.00

Reason: A blank input is a real runtime case. It must produce a clarification route, not an empty accidental success.

## Pair 008 - Phrasing B

A:

original_prompt: ?

compiled_subject: none

local_subject: blank_input

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

normalized_roots: []

missing_coverage_signal: blank_input_requires_user_clarification

B:

domain_labels: [user_clarification]

domain_packets:

- domain_label: user_clarification
  prompt_fragment: The input contains punctuation but no usable registration request.
  coverage_status: clarification_required
  reason_for_inclusion: Blankness means absence of semantic content, not only absence of characters.

split_confidence: 1.00

---

## Pair 009 - Interrupted multi-step registration

A:

original_prompt: I started signing up with Google, got sent to a questionnaire, closed the app halfway through, and now I want to continue.

compiled_subject: flow-01 user registration

local_subject: interrupted_registration_continuation

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [start, sign, provider, questionnaire, close, halfway, continue]

missing_coverage_signal: boundary_needs_resume_state_rules

B:

domain_labels: [provider_identity_entry, questionnaire_onboarding, resume_state_boundary, duplicate_prevention, token_session_safety]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The person started signup through Google.
  coverage_status: covered
  reason_for_inclusion: The registration entry path is provider-based.

- domain_label: questionnaire_onboarding
  prompt_fragment: The person reached the onboarding questionnaire stage.
  coverage_status: covered
  reason_for_inclusion: Flow-01 includes questionnaire delivery after account creation.

- domain_label: resume_state_boundary
  prompt_fragment: The person closed the app before finishing and wants to continue from the saved point.
  coverage_status: boundary
  reason_for_inclusion: Resume-state behavior is not fully taught by the first Flow-01 set.

- domain_label: duplicate_prevention
  prompt_fragment: Continuing after interruption must not create a second account for the same provider identity.
  coverage_status: covered
  reason_for_inclusion: Resume must read existing state before account creation.

- domain_label: token_session_safety
  prompt_fragment: Continuation must rely on a still-trusted session or a fresh trusted action.
  coverage_status: covered
  reason_for_inclusion: The user cannot safely resume from stale trust material.

split_confidence: 0.78

Reason: The prompt crosses provider entry, questionnaire onboarding, saved-state recovery, duplicate prevention, and token safety.

## Pair 009 - Phrasing B

A:

original_prompt: My Google signup reached the onboarding questions, then I left the app before finishing. Let me pick up from there.

compiled_subject: flow-01 user registration

local_subject: interrupted_registration_continuation

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

normalized_roots: [provider, signup, onboarding, question, left, finish, resume]

missing_coverage_signal: boundary_needs_resume_state_rules

B:

domain_labels: [provider_identity_entry, questionnaire_onboarding, resume_state_boundary, duplicate_prevention, token_session_safety]

domain_packets:

- domain_label: provider_identity_entry
  prompt_fragment: The person began registration with Google.
  coverage_status: covered
  reason_for_inclusion: Google signup is the provider entry path.

- domain_label: questionnaire_onboarding
  prompt_fragment: The person reached onboarding questions.
  coverage_status: covered
  reason_for_inclusion: Questionnaire onboarding is a distinct Flow-01 stage.

- domain_label: resume_state_boundary
  prompt_fragment: The person wants to resume after leaving before completion.
  coverage_status: boundary
  reason_for_inclusion: The local set does not fully define saved-progress restoration.

- domain_label: duplicate_prevention
  prompt_fragment: Resuming should reuse the known pending account instead of creating another account.
  coverage_status: covered
  reason_for_inclusion: Resume behavior depends on reading existing registration state.

- domain_label: token_session_safety
  prompt_fragment: The system must check whether the continuation is still trusted.
  coverage_status: covered
  reason_for_inclusion: Stale or expired trust should stop unsafe continuation.

split_confidence: 0.78

---

## Pair 010 - Expired token before registration completion

A:

original_prompt: What happens if the login token expires before signup finishes?

compiled_subject: flow-01 user registration

local_subject: token_session_safety

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [login, token, expire, signup, finish, safety]

missing_coverage_signal: covered

B:

domain_labels: [token_session_safety, provider_identity_entry]

domain_packets:

- domain_label: token_session_safety
  prompt_fragment: The registration token expires before signup is complete.
  coverage_status: covered
  reason_for_inclusion: The central issue is whether expired trust material can complete registration.

- domain_label: provider_identity_entry
  prompt_fragment: The token is tied to a login or provider-based registration step.
  coverage_status: covered
  reason_for_inclusion: The token proves a prior sign-in action and must be checked before account completion.

split_confidence: 0.92

Reason: The prompt is mainly token/session safety, with provider-entry context attached.

## Pair 010 - Phrasing B

A:

original_prompt: If my signup session token is no longer valid, should the account still be completed?

compiled_subject: flow-01 user registration

local_subject: token_session_safety

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0]

normalized_roots: [signup, session, token, invalid, account, complete]

missing_coverage_signal: covered

B:

domain_labels: [token_session_safety, provider_identity_entry]

domain_packets:

- domain_label: token_session_safety
  prompt_fragment: The signup session token is invalid before account completion.
  coverage_status: covered
  reason_for_inclusion: Account completion must not rely on invalid session proof.

- domain_label: provider_identity_entry
  prompt_fragment: The invalid token belongs to a trusted sign-in or registration continuation.
  coverage_status: covered
  reason_for_inclusion: The flow needs a fresh trusted entry action before completion.

split_confidence: 0.92
