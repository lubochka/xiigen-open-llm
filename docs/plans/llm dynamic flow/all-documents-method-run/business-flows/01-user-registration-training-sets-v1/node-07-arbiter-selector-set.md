# Node-07 - Arbiter Selector

## Node purpose

Node-07 receives all formal results R1...Rn and context. It decides which arbiter units
must review the result group before final formatting.

## Pair 7.1 - SSO token handling

Runtime A:
Results include provider callback, platform session issue, token storage, and account
creation through Google SSO.

Expected B:
Select arbiters:
- security arbiter: tokens, provider callback, and session storage are involved.
- event sequence arbiter: account creation and registration events must be in order.
- privacy and compliance arbiter: email, name, and profile picture were collected.

Alternate A:
SSO result group includes profile data and session creation.

Alternate B:
Select [security arbiter, event sequence arbiter, privacy and compliance arbiter].

## Pair 7.2 - Email registration with password

Runtime A:
Results include password validation, password storage, pending account, verification token,
and verification email delivery.

Expected B:
Select arbiters:
- security arbiter: password rules, token strength, and session safety apply.
- communication arbiter: verification email must be sent reliably.
- privacy and compliance arbiter: email and credentials are sensitive information.
- event sequence arbiter: registration initiation must happen before verification sent.

Alternate A:
Email sign-up path with password and verification link.

Alternate B:
Select [security arbiter, communication arbiter, privacy and compliance arbiter, event
sequence arbiter].

## Pair 7.3 - Account merge

Runtime A:
Results include existing email account, new SSO provider, merge decision, and preserved or
changed user identity.

Expected B:
Select arbiters:
- data integrity arbiter: older user identity must be preserved.
- identity consistency arbiter: the provider email and existing email must match after normalization.
- event sequence arbiter: merge and sign-in events must not duplicate account creation.

Alternate A:
Google SSO is linked to an account that already exists.

Alternate B:
Select [data integrity arbiter, identity consistency arbiter, event sequence arbiter].

## Pair 7.4 - Questionnaire failure

Runtime A:
Results include active account, questionnaire service unavailable, delayed questionnaire,
and dead letter or retry handling.

Expected B:
Select arbiters:
- resilience arbiter: failed work must not be discarded.
- onboarding gate arbiter: onboarding completion must not be published early.
- operations arbiter: the failure path must leave recoverable evidence.

Alternate A:
Account is ready but onboarding message failed to send.

Alternate B:
Select [resilience arbiter, onboarding gate arbiter, operations arbiter].

## Pair 7.5 - Rate limit block

Runtime A:
Results include more than five attempts from one IP address in fifteen minutes and a blocked
registration attempt.

Expected B:
Select arbiters:
- security arbiter: rate limit value and enforcement must match the rule.
- user messaging arbiter: blocked user wording must not reveal account existence.
- audit arbiter: security monitoring should record the attempt.

Alternate A:
Registration attempt was rejected because of too many attempts.

Alternate B:
Select [security arbiter, user messaging arbiter, audit arbiter].

## Pair 7.6 - Mobile deep link

Runtime A:
Results include mobile app registration, browser-based SSO, and callback returning to the
app.

Expected B:
Select arbiters:
- client experience arbiter: the user must return to the app after provider approval.
- security arbiter: callback and redirect handling must be restricted.
- authentication arbiter: provider approval must map to the correct account.

Alternate A:
React Native user signs up through external provider and comes back to the app.

Alternate B:
Select [client experience arbiter, security arbiter, authentication arbiter].

## Pair 7.7 - Tenant-specific questionnaire

Runtime A:
Results include selected business community, profile storage, and tenant-aware questionnaire
generation.

Expected B:
Select arbiters:
- tenant context arbiter: tenant selection must be stored from the start.
- privacy and compliance arbiter: tenant and profile data are personal context.
- onboarding arbiter: questionnaire content must use the stored tenant.

Alternate A:
Registration includes a community choice that changes the first questions.

Alternate B:
Select [tenant context arbiter, privacy and compliance arbiter, onboarding arbiter].

## Pair 7.8 - Funnel metrics

Runtime A:
Results include registration start, provider choice, email verification, questionnaire
completion, drop-off point, and time to onboarding.

Expected B:
Select arbiters:
- metrics integrity arbiter: milestone events must support accurate funnel reporting.
- privacy and compliance arbiter: metrics must not expose unnecessary personal data.
- event sequence arbiter: milestone order must reflect the actual flow.

Alternate A:
Product metrics are produced for the registration funnel.

Alternate B:
Select [metrics integrity arbiter, privacy and compliance arbiter, event sequence arbiter].

## Pair 7.9 - Provider outage

Runtime A:
Results include one unavailable provider, disabled sign-up option, alternate providers
available, and no lost registration state.

Expected B:
Select arbiters:
- operations arbiter: outage handling must isolate the failed provider.
- client experience arbiter: the user must see a clear alternative.
- authentication arbiter: unavailable provider must not accept new callbacks.

Alternate A:
Google sign-up is temporarily unavailable but email registration remains available.

Alternate B:
Select [operations arbiter, client experience arbiter, authentication arbiter].

## Pair 7.10 - Full registration result group

Runtime A:
Results include account creation, events, questionnaire delivery, onboarding completion,
security controls, audit logging, and product metrics.

Expected B:
Select arbiters:
- security arbiter
- event sequence arbiter
- onboarding gate arbiter
- privacy and compliance arbiter
- metrics integrity arbiter
- formatter consistency arbiter

Reason: a full result group touches credentials, personal data, event order, completion
rules, reporting, and final audience language.

Alternate A:
The entire registration flow has completed and is ready for final answer formatting.

Alternate B:
Select all arbiters required for safety, order, completion, privacy, metrics, and final
readability.

---

