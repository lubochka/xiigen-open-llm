# Node-02 - Domain Translator

## Node purpose

One Node-02 unit runs for one domain fragment. It receives human wording plus a domain
label and returns formal wording that a specialist node can use.

## Pair 2.1 - SSO authentication wording

Runtime A:
Fragment: "I want users to sign up with Google."
Domain: authentication

Expected B:
The registration path uses Google as the identity provider. The user is sent to Google's
sign-in and consent screen. After approval, Google returns an authorization code. The
system exchanges that code for the user's profile, normalizes the returned email address,
checks whether that email already belongs to an account, then either creates a new active
account or links the provider to the existing account.

Alternate A:
Fragment: "Google SSO registration."
Domain: authentication

Alternate B:
Use the Google provider path. Accept the provider callback only after the user approves
consent. Use the returned profile to decide whether this is a new account or an existing
email account. Do not store the provider token as the platform login. Issue the platform's
own session after the account decision is complete.

## Pair 2.2 - Email authentication wording

Runtime A:
Fragment: "Users can register with email and password."
Domain: authentication

Expected B:
The user submits an email address and password. The email address is normalized to
lowercase. The password must pass the strength rules: at least eight characters, uppercase
and lowercase letters, a digit, a special character, not in the common password list, and
not similar to the email. If the email is new, create a pending account and require email
verification before activation.

Alternate A:
Fragment: "Email sign-up with password and verification."
Domain: authentication

Alternate B:
Treat email registration as a two-step identity claim. First accept the email and a valid
password into a pending account. Then prove email ownership with a single-use verification
link before the account becomes active.

## Pair 2.3 - Verification email wording

Runtime A:
Fragment: "Send a verification email."
Domain: communication

Expected B:
Create a delivery request for the email service. The message contains a verification link
with a single-use token. The email should be delivered within sixty seconds. If delivery
fails temporarily, keep the message for retry and do not lose it.

Alternate A:
Fragment: "The user should receive a confirmation link in their inbox."
Domain: communication

Alternate B:
Deliver an email that asks the user to confirm ownership of the submitted address. The
link expires after twenty-four hours. Successful delivery should be recorded for audit.

## Pair 2.4 - Onboarding questionnaire wording

Runtime A:
Fragment: "After registration, ask the user setup questions in chat."
Domain: onboarding

Expected B:
After the account becomes active, create the user's initial questionnaire. Deliver it
through the chat channel. Onboarding is complete only when the profile exists, the
questionnaire is completed, and business details are submitted.

Alternate A:
Fragment: "Send a guided questionnaire once the account is ready."
Domain: onboarding

Alternate B:
Open an onboarding stage after account activation. Generate a questionnaire for the new
user, ask it through chat, store the answers, and close onboarding only after all required
profile and business details are present.

## Pair 2.5 - Event coordination wording

Runtime A:
Fragment: "Registration should trigger the next services automatically."
Domain: event coordination

Expected B:
The flow is coordinated through published events. In the SSO path, authentication leads to
user creation, questionnaire requirement, questionnaire delivery, questionnaire completion,
and onboarding completion. In the email path, registration initiation and email verification
happen before user activation and onboarding. Every handler must be safe to receive the
same event more than once.

Alternate A:
Fragment: "Each step should notify the following step."
Domain: event coordination

Alternate B:
Use event order as the handoff between services. A service publishes a fact after it has
finished its own work, and the next service reacts to that fact. Duplicate delivery must
not create duplicate profiles, duplicate emails, or duplicate questionnaires.

## Pair 2.6 - Security wording

Runtime A:
Fragment: "Make registration safe from attacks."
Domain: security

Expected B:
Apply rate limiting at five registration attempts per IP address per fifteen minutes.
Use strict provider redirect validation. Prevent account enumeration by returning the
same visible response whether an email exists or not. Use single-use random verification
tokens that expire after twenty-four hours. Store platform tokens in secure cookies and
never in browser local storage.

Alternate A:
Fragment: "Do not let attackers abuse sign-up or learn which emails are registered."
Domain: security

Alternate B:
Protect the registration surface with rate limits, identical visible responses for email
existence checks, strong single-use verification tokens, secure token storage, CSRF
protection, and provider callback allow-listing.

## Pair 2.7 - User management wording

Runtime A:
Fragment: "Keep track of where every new user is in registration."
Domain: user management

Expected B:
Maintain the user's registration and onboarding state. Email users may start as pending.
Activated users may still be incomplete until the questionnaire and business details are
finished. Incomplete users receive reminders after twenty-four hours and seventy-two
hours. After thirty days without completion, mark the account dormant.

Alternate A:
Fragment: "Know who is pending, active, onboarding, complete, or dormant."
Domain: user management

Alternate B:
Represent each user's progress as a known state. Move the user forward only when the
required evidence exists. Do not delete abandoned accounts; mark them dormant after the
defined waiting period.

## Pair 2.8 - Operations wording

Runtime A:
Fragment: "Email failure should not lose registrations."
Domain: operations

Expected B:
Treat email delivery as reliable background work. If the email service is unavailable,
keep the delivery request for retry. If repeated retries fail, place the message in the
dead letter queue for review. The account state should remain recoverable.

Alternate A:
Fragment: "If one service is down, registration should recover later."
Domain: operations

Alternate B:
Separate the successful account step from the temporarily failed follow-up step. Preserve
the failed message, expose a recoverable state, and resume the flow when the service is
healthy again.

## Pair 2.9 - Privacy and compliance wording

Runtime A:
Fragment: "Registration collects email, name, profile picture, and business details."
Domain: privacy and compliance

Expected B:
The flow handles personal information. The system must capture consent at registration,
keep an audit trail of authentication events, support deletion rights, and retain failed
attempt logs for the defined period. Verification tokens are temporary and should be
purged after expiry.

Alternate A:
Fragment: "Keep compliance records for registration."
Domain: privacy and compliance

Alternate B:
Record authentication and registration events with the information needed for audit:
address, user agent, time, and event type. Protect confidential credentials and internal
profile data according to their sensitivity.

## Pair 2.10 - Client and tenant wording

Runtime A:
Fragment: "The mobile app should return from SSO, and the questionnaire can change by community."
Domain: client experience and tenant context

Expected B:
The client experience must support mobile provider sign-in through a browser flow and
return the user to the app through a callback. If the user selected a business community,
store that tenant context with the profile from the start and use it when generating the
questionnaire.

Alternate A:
Fragment: "React Native registration plus tenant-specific onboarding."
Domain: client experience and tenant context

Alternate B:
Support mobile sign-up without losing the user after external provider approval. Preserve
the selected tenant immediately and let it influence the first onboarding questions.

---

