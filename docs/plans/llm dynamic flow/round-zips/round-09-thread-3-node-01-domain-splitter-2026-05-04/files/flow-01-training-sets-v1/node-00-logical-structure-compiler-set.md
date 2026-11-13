# Node 00 - Logical Structure Compiler Set

## Purpose

This file teaches the compiler pre-stage for Flow-01 user registration.

Node 00 receives raw text and produces a compiled logical structure. The output is not a label and not a description of the job. It is the actual structure that downstream nodes would receive.

## Shared Output Contract

Each B output uses this shape:

- source_role: what kind of input this is.
- detected_subject: the broad source family.
- local_subject: the strongest Flow-01 subject.
- predicate_counts_22: the full twenty-two predicate count vector.
- local_domain_slots_5: the five local registration slots used in this first set.
- normalized_roots: up to thirty-two roots preserved for matching.
- meaning_graph: the logical units and their dependencies.
- decomposition_count: how many logical pieces the compiler sees.
- confidence: the compiler confidence for this structure.
- missing_coverage_signal: whether the request is covered, boundary, blank, or knowledge edge.

Predicate slot order:

0 NULL_CHECK, 1 RANGE_CHECK, 2 TYPE_CHECK, 3 ASSIGN_OP, 4 RETURN_OP, 5 LOOP_OP, 6 BRANCH_OP, 7 CALL_OP, 8 AWAIT_OP, 9 THROW_OP, 10 CATCH_OP, 11 VALIDATE_OP, 12 CREATE_OP, 13 UPDATE_OP, 14 DELETE_OP, 15 GET_OP, 16 CONVERT_OP, 17 SEND_OP, 18 LOG_OP, 19 AUTH_OP, 20 FILTER_OP, 21 AGGREGATE_OP.

Local domain slot order:

0 provider_entry, 1 email_verification, 2 duplicate_prevention, 3 token_session_safety, 4 knowledge_edge_or_blank.

---

## Pair 001 - Email registration

A: I want to create an account with my email address and a password.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: email_registration_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 0]

normalized_roots: [create, account, email, password, registration]

meaning_graph:

- unit_1: the person asks to begin registration with an email address.
- unit_2: the person supplies or intends to supply a password.
- dependency: account creation may start, but normal completion still requires trusted email ownership and safe credential handling.

decomposition_count: 2

confidence: 0.91

missing_coverage_signal: covered

Reason: Flow-01 contains an email path where account creation and verification are separate. The compiler must preserve both account creation and trust/safety requirements.

## Pair 001 - Phrasing B

A: Let me register by entering my email and choosing a password.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: email_registration_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 0]

normalized_roots: [register, email, password, account]

meaning_graph:

- unit_1: the person wants to enter the registration flow through email.
- unit_2: the person expects password-based account credentials.
- dependency: the created account must remain tied to email verification and safe credential handling before completion.

decomposition_count: 2

confidence: 0.91

missing_coverage_signal: covered

---

## Pair 002 - Google provider registration

A: I want to sign up using my Google account.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: provider_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 0]

normalized_roots: [sign, provider, account, identity, registration]

meaning_graph:

- unit_1: the person chooses an external identity provider as the registration entry.
- unit_2: the provider identity must be read and trusted before local account creation or linking.
- dependency: provider trust comes before account creation, duplicate prevention, and session start.

decomposition_count: 3

confidence: 0.94

missing_coverage_signal: covered

Reason: E51B teaching records and Flow-01 both treat provider entry as a covered registration subject with identity consistency and token safety constraints.

## Pair 002 - Phrasing B

A: Can I join with my Google profile instead of making a password first?

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: provider_entry

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 0]

normalized_roots: [join, provider, account, password, identity]

meaning_graph:

- unit_1: the person wants provider-based entry instead of password-first entry.
- unit_2: the provider result must be trusted before deciding whether to create or link a local account.
- dependency: identity consistency and token safety both attach to the provider result before the session may begin.

decomposition_count: 3

confidence: 0.94

missing_coverage_signal: covered

---

## Pair 003 - Email verification link

A: I registered with email; send me a verification link.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: email_verification

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 0]

normalized_roots: [register, email, send, verification, link, account]

meaning_graph:

- unit_1: an email registration already exists or has just started.
- unit_2: a verification message must be sent to the mailbox.
- unit_3: the link must later be checked before normal account access is completed.
- dependency: sending the link is not the same as completing verification.

decomposition_count: 3

confidence: 0.93

missing_coverage_signal: covered

Reason: Flow-01 distinguishes account creation from verification completion. The compiler must preserve the wait for the verification action.

## Pair 003 - Phrasing B

A: After I make an account with email, I need the system to email me the confirmation link.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: email_verification

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 0]

normalized_roots: [account, email, confirmation, link, verify]

meaning_graph:

- unit_1: the person describes an email-created account.
- unit_2: the system must send a confirmation link to the mailbox.
- unit_3: the confirmation action is a later trust step, not proof already completed.
- dependency: account completion depends on receiving and validating the confirmation action.

decomposition_count: 3

confidence: 0.93

missing_coverage_signal: covered

---

## Pair 004 - Password recovery during unfinished registration

A: I started registering with email, forgot the password I chose, and still have not verified my email.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: registration_recovery_boundary

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 1]

normalized_roots: [start, register, email, password, forgot, verify, pending]

meaning_graph:

- unit_1: the person has a pending email-registration attempt.
- unit_2: the person reports a credential problem before verification is complete.
- unit_3: the email address is still not trusted because verification did not finish.
- dependency: any recovery path must preserve pending-registration state and must not grant normal access before email ownership is proven.

decomposition_count: 3

confidence: 0.72

missing_coverage_signal: boundary_needs_account_recovery_rules

Reason: Flow-01 covers registration and verification, but password recovery during an unfinished registration is only adjacent. The compiler must mark the boundary instead of pretending full coverage.

## Pair 004 - Phrasing B

A: I was signing up by email, I lost the password I entered, and my email confirmation is not done yet.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: registration_recovery_boundary

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [0, 1, 0, 1, 1]

normalized_roots: [sign, email, lost, password, confirmation, pending]

meaning_graph:

- unit_1: the registration attempt is not complete.
- unit_2: the password problem appears before mailbox confirmation.
- unit_3: the flow must avoid treating recovery as proof of account ownership.
- dependency: recovery may be routed, but verified mailbox ownership is still required before normal access.

decomposition_count: 3

confidence: 0.72

missing_coverage_signal: boundary_needs_account_recovery_rules

---

## Pair 005 - Terms and conditions confirmation

A: Before finishing signup, I need to accept the terms and privacy policy.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: registration_completion_condition

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0]

local_domain_slots_5: [0, 1, 0, 0, 1]

normalized_roots: [finish, signup, accept, terms, privacy, condition]

meaning_graph:

- unit_1: the person describes a required acceptance step before registration finishes.
- unit_2: the acceptance must be recorded as a completion condition.
- dependency: registration can continue only after the acceptance condition is satisfied.

decomposition_count: 2

confidence: 0.69

missing_coverage_signal: boundary_needs_policy_acceptance_rules

Reason: The prompt belongs to registration completion, but the existing Flow-01 proof focused on provider entry, email verification, duplicate prevention, and token safety. The compiler must mark this as a boundary condition.

## Pair 005 - Phrasing B

A: Do not complete my account until I agree to the terms and the privacy notice.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: registration_completion_condition

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0]

local_domain_slots_5: [0, 1, 0, 0, 1]

normalized_roots: [complete, account, agree, terms, privacy, condition]

meaning_graph:

- unit_1: the account completion is conditional.
- unit_2: terms and privacy acceptance are the condition.
- dependency: the flow must withhold completed-registration status until the condition is recorded.

decomposition_count: 2

confidence: 0.69

missing_coverage_signal: boundary_needs_policy_acceptance_rules

---

## Pair 006 - Biometric passport verification knowledge edge

A: I want biometric passport verification during signup.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: knowledge_edge_identity_proofing

predicate_counts_22: [0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 0, 0, 1, 1]

normalized_roots: [biometric, passport, verification, signup, identity, proofing]

meaning_graph:

- unit_1: the person asks for a stronger identity-proof step during registration.
- unit_2: passport verification is not the same as provider sign-in or email verification.
- unit_3: biometric proofing likely needs its own source authority, privacy rule, and identity-proof arbiter.
- dependency: local Flow-01 registration knowledge is related, but not sufficient to answer safely.

decomposition_count: 3

confidence: 0.44

missing_coverage_signal: knowledge_edge_missing_biometric_passport_verification

Reason: This is the E51B knowledge-edge shape. The compiler must name the missing competence rather than routing confidently to ordinary registration.

## Pair 006 - Phrasing B

A: During account creation, require a passport scan and face match before the user can continue.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: knowledge_edge_identity_proofing

predicate_counts_22: [0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 0, 0, 1, 1]

normalized_roots: [account, creation, passport, scan, face, match, continue]

meaning_graph:

- unit_1: the registration path adds passport and face matching.
- unit_2: the added proofing step introduces identity-proof and sensitive-data concerns not covered by the current local set.
- unit_3: the flow should prepare a learning gap or external-help context instead of inventing the proofing design.
- dependency: the registration flow may continue only after the missing identity-proofing rules are supplied and reviewed.

decomposition_count: 3

confidence: 0.42

missing_coverage_signal: knowledge_edge_missing_biometric_passport_verification

---

## Pair 007 - Duplicate account creation rejection

A: If the same provider callback arrives twice, create a second account anyway.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: duplicate_prevention_conflict

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 0]

normalized_roots: [provider, callback, duplicate, create, second, account]

meaning_graph:

- unit_1: the prompt asks the system to receive a repeated provider callback.
- unit_2: the prompt asks for a second account even though the provider identity is the same.
- unit_3: this conflicts with identity consistency and duplicate prevention.
- dependency: the compiler must preserve the conflict so downstream arbiters can reject the unsafe instruction.

decomposition_count: 3

confidence: 0.95

missing_coverage_signal: covered_conflict_requires_arbiter_rejection

Reason: This is the E51B arbiter-authority shape. Similarity to valid registration examples must not override the duplicate-prevention rule.

## Pair 007 - Phrasing B

A: When Google sends the same signup response again, make a new user instead of reusing the old one.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: duplicate_prevention_conflict

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 0]

normalized_roots: [provider, signup, response, duplicate, new, user, reuse]

meaning_graph:

- unit_1: the provider response is repeated.
- unit_2: the prompt requests a new user record rather than reuse.
- unit_3: the request contradicts identity consistency because the same provider identity should resolve to one account.
- dependency: downstream review must reject or reprocess the fragment; it must not become an accepted plan.

decomposition_count: 3

confidence: 0.95

missing_coverage_signal: covered_conflict_requires_arbiter_rejection

---

## Pair 008 - Blank input

A:

B:

source_role: blank input

detected_subject: none

local_subject: blank_input

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

local_domain_slots_5: [0, 0, 0, 0, 1]

normalized_roots: []

meaning_graph:

- unit_1: no user intent is present.
- dependency: no downstream registration flow should start from this input.

decomposition_count: 0

confidence: 1.00

missing_coverage_signal: blank_input_requires_user_clarification

Reason: The compiler must handle blank input explicitly so later nodes are not asked to invent a registration request.

## Pair 008 - Phrasing B

A:    

B:

source_role: blank input

detected_subject: none

local_subject: blank_input

predicate_counts_22: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

local_domain_slots_5: [0, 0, 0, 0, 1]

normalized_roots: []

meaning_graph:

- unit_1: the input contains whitespace but no usable request.
- dependency: the flow should ask for a real request instead of routing to registration.

decomposition_count: 0

confidence: 1.00

missing_coverage_signal: blank_input_requires_user_clarification

---

## Pair 009 - Interrupted multi-step registration

A: I started signing up with Google, got sent to a questionnaire, closed the app halfway through, and now I want to continue.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: interrupted_registration_continuation

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 1]

normalized_roots: [start, sign, provider, questionnaire, close, halfway, continue]

meaning_graph:

- unit_1: the person entered through provider registration.
- unit_2: the flow reached the questionnaire stage.
- unit_3: the person stopped before the registration continuation finished.
- unit_4: the person asks to resume rather than start over.
- dependency: continuation must recover known account state, avoid duplicate account creation, and resume only from the last trusted completed step.

decomposition_count: 4

confidence: 0.78

missing_coverage_signal: boundary_needs_resume_state_rules

Reason: Flow-01 describes onboarding continuation, but resuming after the app closes needs state recovery details that are not fully taught by the first node-00 set.

## Pair 009 - Phrasing B

A: My Google signup reached the onboarding questions, then I left the app before finishing. Let me pick up from there.

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: interrupted_registration_continuation

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0]

local_domain_slots_5: [1, 0, 1, 1, 1]

normalized_roots: [provider, signup, onboarding, question, left, finish, resume]

meaning_graph:

- unit_1: the provider signup path already began.
- unit_2: the onboarding questions were reached but not completed.
- unit_3: the user wants a continuation from saved progress.
- dependency: the flow must read existing state before deciding whether to resume, resend, or restart.

decomposition_count: 3

confidence: 0.78

missing_coverage_signal: boundary_needs_resume_state_rules

---

## Pair 010 - Expired token before registration completion

A: What happens if the login token expires before signup finishes?

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: token_session_safety

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 0, 0, 1, 0]

normalized_roots: [login, token, expire, signup, finish, safety]

meaning_graph:

- unit_1: the prompt is about a token-dependent registration step.
- unit_2: the token is expired before registration completion.
- unit_3: the flow must stop the unsafe continuation and require a fresh trusted action.
- dependency: account completion and session start must not use an expired token.

decomposition_count: 3

confidence: 0.92

missing_coverage_signal: covered

Reason: E51B treated expired registration tokens as covered token-session safety. The compiler must preserve the stop condition.

## Pair 010 - Phrasing B

A: If my signup session token is no longer valid, should the account still be completed?

B:

source_role: user prompt

detected_subject: flow-01 user registration

local_subject: token_session_safety

predicate_counts_22: [0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0]

local_domain_slots_5: [0, 0, 0, 1, 0]

normalized_roots: [signup, session, token, invalid, account, complete]

meaning_graph:

- unit_1: the user asks about completing registration from an invalid token.
- unit_2: the invalid token cannot prove a trusted continuation.
- unit_3: the flow should stop completion and request a fresh trusted sign-in or verification action.
- dependency: no completed account or session should be created from the invalid token.

decomposition_count: 3

confidence: 0.92

missing_coverage_signal: covered
