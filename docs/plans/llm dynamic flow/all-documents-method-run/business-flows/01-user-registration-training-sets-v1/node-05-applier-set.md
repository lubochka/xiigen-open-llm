# Node-05 - Applier Array

## Node purpose

Each Node-05 unit receives one processed piece and its fb instruction. It produces the
concrete result Rqi. The result describes what happened and what evidence exists.

## Pair 5.1 - Apply new SSO account

Runtime A:
Processed result says: new Google SSO account for "owner@example.com"; create active user;
publish registration events after the record exists.

Expected B:
R1: Active user account created for "owner@example.com". Google is linked as the login
provider. Platform session issued in secure cookies. Registration authentication event
published after the user record was written. Account is ready for questionnaire delivery.

Alternate A:
New SSO user was accepted and no existing email matched.

Alternate B:
R1: The account exists, the provider is linked, the user is signed in, and the next step is
onboarding.

## Pair 5.2 - Apply account merge

Runtime A:
Processed result says: SSO email matches existing email-password account; preserve older
identity and add provider.

Expected B:
R2: Existing account kept its original user identity. Password login remains available.
Google was added as an additional login method. No duplicate account was created. The
user can continue with either login method.

Alternate A:
Existing user adds Facebook SSO with the same normalized email.

Alternate B:
R2: Facebook was linked to the existing account. The original account identity remains the
primary identity.

## Pair 5.3 - Apply pending email account

Runtime A:
Processed result says: valid email/password registration; unique email; create pending
account; create verification token.

Expected B:
R3: Pending account created for the normalized email. Verification token created with
single-use and twenty-four hour expiry. Registration initiation event published. Account
is not active until verification succeeds.

Alternate A:
Email registration accepted and waiting for the user to confirm the inbox.

Alternate B:
R3: The account is pending, the verification step is open, and no full access has been
granted yet.

## Pair 5.4 - Apply verification email delivery

Runtime A:
Processed result says: delivery request contains verification link; email channel available.

Expected B:
R4: Verification email delivered to the user's inbox within the required window. Delivery
event recorded for audit. The user must click the link before the token expires.

Alternate A:
Email service accepted and sent the confirmation link.

Alternate B:
R4: The verification message was sent successfully and the account is waiting for the
user's click.

## Pair 5.5 - Apply email verification

Runtime A:
Processed result says: verification token accepted, token consumed, account can be
activated.

Expected B:
R5: Email ownership verified. The verification token was consumed and cannot be reused.
The user account was activated. Activation event published. The user can continue to the
questionnaire path.

Alternate A:
The user clicked a valid verification link.

Alternate B:
R5: Verification succeeded, the account is active, and onboarding can begin.

## Pair 5.6 - Apply questionnaire delivery

Runtime A:
Processed result says: active user, tenant-aware questionnaire required, chat channel
available.

Expected B:
R6: Initial questionnaire generated for the user. Tenant context was used when selecting
questions. Questionnaire delivered in chat. Questionnaire-sent event recorded. Onboarding
is open until the user answers.

Alternate A:
The active user needs first setup questions.

Alternate B:
R6: The user now has the questionnaire in chat and can complete onboarding by answering it.

## Pair 5.7 - Apply onboarding completion

Runtime A:
Processed result says: profile exists, questionnaire completed, business details submitted.

Expected B:
R7: Onboarding marked complete. Onboarding completion event published. Downstream
personalization may start: matching, feed adaptation, learning programs, messaging, and
dashboard updates.

Alternate A:
All onboarding criteria are satisfied.

Alternate B:
R7: The user has completed onboarding and the platform can begin personalizing their
experience.

## Pair 5.8 - Apply rate limit block

Runtime A:
Processed result says: requester exceeded five attempts per IP address in fifteen minutes.

Expected B:
R8: Registration attempt rejected because the rate limit was exceeded. No account was
created or changed. The IP address was recorded for security monitoring. The user should
wait before trying again.

Alternate A:
Too many registration attempts arrived from the same source.

Alternate B:
R8: The attempt was blocked before registration work began.

## Pair 5.9 - Apply provider outage handling

Runtime A:
Processed result says: Google provider unavailable, other providers and email path still
available.

Expected B:
R9: Google sign-up was temporarily disabled. Other available SSO providers and email
registration remain visible. The user can continue through another path. No registration
state was lost.

Alternate A:
One provider is down during maintenance.

Alternate B:
R9: The unavailable provider is hidden or disabled, and the user is offered working
registration choices.

## Pair 5.10 - Apply questionnaire failure recovery

Runtime A:
Processed result says: account active, questionnaire service unavailable, preserve
questionnaire request.

Expected B:
R10: Account remains active. Questionnaire was not delivered yet. The delivery request was
preserved for retry or dead letter review. Onboarding completion was not published. The
flow can continue when questionnaire delivery recovers.

Alternate A:
The questionnaire step failed after the user account was already ready.

Alternate B:
R10: User access is preserved, the missing onboarding work is recoverable, and completion
is still waiting.

---

