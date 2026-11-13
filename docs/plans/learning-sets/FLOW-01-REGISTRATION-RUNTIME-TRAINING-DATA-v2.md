# Flow 01 Registration - Runtime Training Data v2

## What this file is

This is a learning set for the dynamic-flow UML nodes. It is not a line ledger of
the source document.

The source document remains the evidence:

`C:\Users\lubao\Documents\xiigen\business flows\01-user-registration.md`

The input side of each pair is shaped like the real thing that node receives at
runtime. A source-document line may justify a pair, but the source line itself is
not used as the training input unless a human would actually type that line.

Each pair uses this rule:

A -> trained LLM node -> B

The B is always concrete. It does not say "produce a clear statement" or "make a
plan." It shows the actual statement, the actual labels, the actual plan, the
actual review decision, or the actual final answer.

## Correct UML placement used here

The flow is treated as nine training targets:

1. Node-01 Domain splitter: raw user prompt -> domain labels d1...dm.
2. Node-02 Domain translator: one prompt fragment plus one domain -> formal domain wording.
3. Node-03 Scheme planner: all domain wordings -> planned pieces b1...bn and prompts fb1...fbn.
4. Node-04 Domain processor array: each bi is processed by its own unit.
5. Node-05 Applier array: each processed bi and fbi produces one result Rqi.
6. Node-06 Result translator array: each Rqi becomes user-language wording, with all R1...Rn visible for consistency.
7. Node-07 Arbiter selector: all R1...Rn plus context -> selected arbiters.
8. Node-08 Arbiter units: each selected arbiter reviews the relevant result and can pass it or send fbx back for reprocessing.
9. Node-09 Formatter: all approved user-language results -> one readable answer.

The arbiter selector and arbiter units are not after the formatter. They operate
before final formatting. Their feedback returns a reprocessing fragment back into
the front of the flow.

## Document examination method used to build this set

1. Read the single selected document from beginning to end.
2. Mark its real runtime situations: Google sign-up, email sign-up, email verification,
   questionnaire delivery, account merge, provider outage, mobile return, tenant
   registration, abandonment, rate limiting, duplicate callbacks, expired links.
3. Mark the human vocabulary for each situation. This vocabulary becomes Node-01 input.
4. Convert each situation into domain-specific formal wording. This becomes Node-02 output.
5. Decompose the situations into pieces that can run in sequence or in parallel. This
   becomes Node-03 output.
6. For each planned piece, write the result a specialized processor should produce.
   This becomes Node-04 output.
7. For each processed piece, write the concrete action and outcome. This becomes
   Node-05 output.
8. Translate each formal result into what the human user or stakeholder should see.
   This becomes Node-06 output.
9. Decide which reviewers are required for each result group. This becomes Node-07 output.
10. Write actual pass or flag decisions for those reviewers. This becomes Node-08 output.
11. Fold approved translated results into one answer for a real audience. This becomes
    Node-09 output.
12. Simulate the full flow from raw human prompt to final answer. Any missing fact means
    the responsible node needs more training pairs.

## Shared domain labels

These labels are used by Node-01 and Node-02:

- authentication
- communication
- onboarding
- security
- event coordination
- user management
- operations
- privacy and compliance
- client experience
- tenant context
- product metrics

The labels are not final architecture names. They are the training vocabulary extracted
from this one registration document.

---

# Node-01 - Domain Splitter

## Node purpose

A real human prompt enters this node. The node outputs all domains touched by the prompt.
One prompt can produce several labels.

## Pair 1.1 - External provider sign-up

Runtime A:
"I want users to sign up with Google, Facebook, LinkedIn, or Figma instead of creating another password."

Expected B:
[authentication]

Alternate A:
"Let a new user click Continue with Google and enter the platform without filling out a password form."

Alternate B:
[authentication]

Source evidence:
The document defines SSO registration through Google, Facebook, LinkedIn, and Figma.

## Pair 1.2 - Email sign-up with verification

Runtime A:
"Users should also be able to register with email and password, then click a verification link."

Expected B:
[authentication, communication]

Alternate A:
"I need email registration where the user gets a confirmation email before the account becomes active."

Alternate B:
[authentication, communication]

Source evidence:
The document defines email and password registration, a pending user record, and a verification email.

## Pair 1.3 - Chat questionnaire after account creation

Runtime A:
"After registration, send the new user a questionnaire in chat."

Expected B:
[onboarding, communication]

Alternate A:
"When someone creates an account, ask the first setup questions through the in-app chat."

Alternate B:
[onboarding, communication]

Source evidence:
The document says the Questionnaire Service generates the questionnaire and the Messaging Service delivers it via chat.

## Pair 1.4 - Completion triggers personalization

Runtime A:
"When the user finishes the questionnaire, start matching and personalize the feed."

Expected B:
[onboarding, event coordination]

Alternate A:
"Completing onboarding should trigger downstream personalization."

Alternate B:
[onboarding, event coordination]

Source evidence:
The document says UserOnboardingCompleted triggers matching, feed adaptation, learning programs, messaging, and dashboard behavior.

## Pair 1.5 - Abandoned registration reminders

Runtime A:
"If a user creates an account but does not finish the questionnaire, send reminders and later mark the account dormant."

Expected B:
[user management, communication, event coordination]

Alternate A:
"Track users who stop after registration and follow up after 24 hours, 72 hours, and 30 days."

Alternate B:
[user management, communication, event coordination]

Source evidence:
The document defines reminder email timing and dormant account handling.

## Pair 1.6 - Abuse protection

Runtime A:
"Registration must block automated attempts and avoid revealing whether an email already exists."

Expected B:
[security]

Alternate A:
"Protect the sign-up endpoint with rate limiting and account enumeration prevention."

Alternate B:
[security]

Source evidence:
The document defines rate limiting, CSRF protection, strict redirect validation, token rules, and identical responses for email existence checks.

## Pair 1.7 - SSO provider outage

Runtime A:
"If Google sign-up is down during maintenance, show other available providers and let the user continue another way."

Expected B:
[operations, authentication, client experience]

Alternate A:
"One SSO provider may be unavailable. The page should disable that option and offer email or another provider."

Alternate B:
[operations, authentication, client experience]

Source evidence:
The document has a scenario for registration during maintenance.

## Pair 1.8 - Mobile registration

Runtime A:
"Users should be able to register from the mobile app and return to the app after the provider approves SSO."

Expected B:
[client experience, authentication]

Alternate A:
"Mobile SSO should use the system browser or in-app browser tab and deep-link back into the app."

Alternate B:
[client experience, authentication]

Source evidence:
The document defines mobile registration through the React Native app and deep link callback.

## Pair 1.9 - Tenant-specific onboarding

Runtime A:
"During registration, users choose a business community, and the first questionnaire should match that community."

Expected B:
[tenant context, onboarding, user management]

Alternate A:
"Store the selected tenant from the start and vary the questionnaire by tenant."

Alternate B:
[tenant context, onboarding, user management]

Source evidence:
The document defines multi-tenant registration and tenant-specific questionnaire variation.

## Pair 1.10 - Funnel tracking

Runtime A:
"I want to know where people drop off in registration and how many finish by provider."

Expected B:
[product metrics, event coordination, user management]

Alternate A:
"Track registration completion rate, verification rate, questionnaire completion, and provider split."

Alternate B:
[product metrics, event coordination, user management]

Source evidence:
The product manager section defines funnel metrics and drop-off tracking.

---

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

# Node-03 - Scheme Planner

## Node purpose

Node-03 receives the formal domain wordings together. It outputs planned pieces b1...bn
and the instruction fb for each piece. A piece may be sequential, parallel, or a wait
state. The plan must include happy path, branch path, and recovery path when the prompt
requires them.

## Pair 3.1 - Full SSO registration plan

Runtime A:
Domain wordings say: Google SSO sign-up, create or merge account, issue platform session,
send chat questionnaire, complete onboarding.

Expected B:
b1: Confirm provider callback and retrieve the provider profile.
fb1: Accept only the approved provider callback. Extract email and profile data. Normalize
the email before comparison.

b2: Decide whether the account is new or already exists.
fb2: If the normalized email is new, create an active account. If the email exists, use the
account merge rule and preserve the older user identifier.

b3: Issue the platform session after the account decision is complete.
fb3: Create the platform session only after the account exists or has been merged. Store
session tokens in secure cookies.

b4: Publish the registration and creation events in order.
fb4: Announce authentication first, then account creation. Do not announce an event before
the related record exists.

b5: Create and deliver the initial questionnaire.
fb5: After the account is active, generate the questionnaire and deliver it in chat.

b6: Wait for questionnaire completion and close onboarding.
fb6: Wait until profile, questionnaire answers, and business details are all complete.
Then publish the onboarding completion event.

Alternate A:
"Plan the Google SSO path from provider approval to dashboard with questionnaire."

Alternate B:
Identity first, account decision second, platform session third, event handoff fourth,
questionnaire delivery fifth, onboarding completion last. The plan must not deliver the
questionnaire before the account exists.

## Pair 3.2 - Full email registration plan

Runtime A:
Domain wordings say: email and password registration, verification email, pending account,
activation after link click, then questionnaire.

Expected B:
b1: Validate submitted email and password.
fb1: Normalize the email and apply all password strength rules.

b2: Create a pending account and verification token.
fb2: Create only one active verification token per email. New token requests invalidate
older tokens. Token expires after twenty-four hours.

b3: Send the verification email.
fb3: Ask the email service to deliver the link. Keep the message retryable until delivery
succeeds or goes to review.

b4: Wait for the verification callback.
fb4: Accept the token only if it exists, is not expired, and has not been used.

b5: Activate the account.
fb5: After successful verification, mark the user verified and active, then announce
activation.

b6: Continue with questionnaire delivery and onboarding completion.
fb6: Follow the same questionnaire path used by SSO users after activation.

Alternate A:
"Plan email sign-up with verification before onboarding."

Alternate B:
Credential validation, pending record, token, email delivery, verification wait, activation,
questionnaire, completion. The activation stage cannot run before the user clicks a valid
link.

## Pair 3.3 - Account merge plan

Runtime A:
Domain wordings say: user previously registered with email, then tries Google SSO with the
same email.

Expected B:
b1: Compare the provider email to existing email accounts after normalization.
fb1: Ignore letter case when deciding whether the email is the same.

b2: Link the new provider to the existing account.
fb2: Keep the existing password if the account had one. Add the provider as another login
method.

b3: Preserve the older user identifier.
fb3: Do not create a new primary identity. Keep all downstream references valid.

b4: Continue the registration path as the existing user.
fb4: Issue the session and continue onboarding if it is incomplete.

Alternate A:
"Plan what happens when a Google SSO email matches an existing email account."

Alternate B:
Normalize, match, link, preserve identity, continue. The plan must not duplicate the user.

## Pair 3.4 - Provider outage plan

Runtime A:
Domain wordings say: one SSO provider is temporarily unavailable during registration.

Expected B:
b1: Detect the unavailable provider.
fb1: Mark only that provider as unavailable. Do not block all registration.

b2: Show available registration paths.
fb2: Offer other SSO providers and email registration.

b3: Preserve user intent.
fb3: If the user chooses a different method, continue the normal path for that method.

b4: Monitor recovery.
fb4: When the provider becomes available again, re-enable it without changing completed
registrations.

Alternate A:
"Plan registration when Google is down but Facebook and email still work."

Alternate B:
Disable the failed option, keep the rest open, explain the state to the user, and recover
when the provider returns.

## Pair 3.5 - Duplicate callback plan

Runtime A:
Domain wordings say: the provider callback may arrive twice because the browser retried or
the user double-clicked approve.

Expected B:
b1: Detect whether this callback has already produced a user result.
fb1: Use the user and event identity to recognize duplicate handling.

b2: Return the existing successful outcome.
fb2: Do not create another user, send another questionnaire, or publish duplicate events.

b3: Record the duplicate as harmless.
fb3: Keep traceability without changing the flow state.

Alternate A:
"Plan for a repeated SSO callback."

Alternate B:
Recognize the duplicate, reuse the first result, avoid double work, and leave the user in a
normal success state.

## Pair 3.6 - Questionnaire service failure plan

Runtime A:
Domain wordings say: account creation succeeds but the questionnaire service is down.

Expected B:
b1: Complete account creation normally.
fb1: Do not roll back the account because the later questionnaire step failed.

b2: Preserve the questionnaire request for retry.
fb2: Place the failed delivery into the retry path or dead letter review path.

b3: Show the user a usable dashboard state.
fb3: The user may enter the platform even if the questionnaire has not arrived yet.

b4: Resume questionnaire delivery after recovery.
fb4: When the service recovers, deliver the questionnaire and continue onboarding.

Alternate A:
"Plan registration when questionnaire delivery fails after the user is already active."

Alternate B:
Keep the account, preserve the missing questionnaire work, avoid pretending onboarding is
complete, and resume when possible.

## Pair 3.7 - Security control plan

Runtime A:
Domain wordings say: registration needs rate limiting, enumeration prevention, token
security, and password hashing.

Expected B:
b1: Check request rate before accepting registration work.
fb1: Block attempts above five per IP address in fifteen minutes.

b2: Normalize email and hide existence information.
fb2: Use the same user-visible response whether an email exists or not.

b3: Generate secure verification tokens.
fb3: Use a single-use random token with twenty-four hour expiry.

b4: Store passwords and sessions safely.
fb4: Store passwords with the required strong password hashing rule. Store platform tokens
in secure cookies, not browser local storage.

Alternate A:
"Plan the security controls for registration."

Alternate B:
Reject abuse early, avoid leaking account existence, use strong temporary tokens, protect
passwords, and protect sessions.

## Pair 3.8 - Mobile and tenant plan

Runtime A:
Domain wordings say: user registers in the mobile app and selects a business community;
questionnaire may vary by tenant.

Expected B:
b1: Start provider authentication from the mobile client.
fb1: Use a browser-based provider approval path and return to the app after approval.

b2: Store tenant selection with the user profile.
fb2: The selected community is not a late note; it is part of the user's starting context.

b3: Generate tenant-aware questionnaire.
fb3: Use the stored tenant context when choosing first questions.

b4: Continue the standard onboarding completion gate.
fb4: Completion still requires profile, questionnaire answers, and business details.

Alternate A:
"Plan mobile sign-up where the selected community changes the first questionnaire."

Alternate B:
Return the mobile user correctly, store tenant context immediately, personalize the
questionnaire by tenant, and keep the same completion gate.

## Pair 3.9 - Product metrics plan

Runtime A:
Domain wordings say: track registration completion rate, verification rate, questionnaire
completion, provider split, time to onboarding, and drop-off points.

Expected B:
b1: Record registration start.
fb1: Capture method, provider if any, tenant if any, time, and source.

b2: Record path milestones.
fb2: Capture email sent, email verified, account created, questionnaire sent, questionnaire
completed, and onboarding completed.

b3: Measure drop-off.
fb3: Compare the last completed milestone to the expected next milestone.

b4: Report by provider and time period.
fb4: Group outcomes by provider, email path, region if available, and time window.

Alternate A:
"Plan the funnel tracking for registration."

Alternate B:
Track starts, milestones, completion, delays, and drop-off so product can understand where
people leave the flow.

## Pair 3.10 - Full mixed request plan

Runtime A:
Domain wordings say: support SSO and email sign-up, questionnaire, security, reminders,
mobile return, tenant context, and metrics.

Expected B:
b1: Split the registration path into SSO branch and email branch.
fb1: Each branch reaches an active account state by its own evidence.

b2: Apply shared account rules.
fb2: Normalize email, prevent duplicates, merge existing accounts, protect tokens, and
preserve the user identity.

b3: Run shared onboarding after activation.
fb3: Generate the questionnaire, deliver it by chat, and wait for completion.

b4: Run support paths alongside the main path.
fb4: Handle provider outage, duplicate callbacks, expired tokens, abandoned onboarding, and
questionnaire service failure.

b5: Run observation paths.
fb5: Record events for audit, compliance, product metrics, and operational monitoring.

Alternate A:
"Plan the entire registration flow."

Alternate B:
Two entry branches, one shared activation state, one shared onboarding gate, recovery paths
for known failures, and observation paths for audit and metrics.

---

# Node-04 - Domain Processor Array

## Node purpose

Each Node-04 unit receives one planned piece bi and its context. It returns a processed
result that is ready for the applier. The unit is specialized by the piece it receives.

## Pair 4.1 - Process SSO callback

Runtime A:
b1: Confirm provider callback and retrieve provider profile.
Context: Google provider, returned email "Owner@Example.com", returned profile has name
and picture.

Expected B:
Processed result: provider callback accepted. Normalized email is "owner@example.com".
Profile information available: email, name, profile picture. Account decision still
needed: compare normalized email against existing accounts before creating a new record.

Alternate A:
b1 for provider profile retrieval returns "OWNER@example.com" from Facebook.

Alternate B:
Accepted provider callback. Normalize to "owner@example.com". Continue to email match
before account creation.

## Pair 4.2 - Process new SSO account decision

Runtime A:
b2: Decide whether the SSO account is new or existing.
Context: normalized email is not present in existing accounts.

Expected B:
Processed result: new SSO account. Create an active user profile for the normalized email.
Assign a permanent user identity. Continue to platform session issue and registration
event publishing.

Alternate A:
No existing account matches the provider email.

Alternate B:
Treat this as a new user. The account may become active immediately because provider
authentication already proved identity.

## Pair 4.3 - Process SSO merge decision

Runtime A:
b2: Decide whether the SSO account is new or existing.
Context: normalized email matches an older email-password account.

Expected B:
Processed result: account merge required. Preserve the older user identity. Keep the
existing password login method. Add the SSO provider as an additional login method.
Continue as the existing user.

Alternate A:
Google returns an email that already belongs to a password account.

Alternate B:
Do not create a second user. Link Google to the existing user and keep all existing account
relationships intact.

## Pair 4.4 - Process email credentials

Runtime A:
b1: Validate email and password.
Context: email "John@Email.com"; password passes all strength rules.

Expected B:
Processed result: normalized email is "john@email.com". Password is acceptable. If the
email is unused, create a pending account and generate a verification token. If the email
already exists, return a neutral user-visible response while recording the real reason
internally.

Alternate A:
Email sign-up arrives with valid password and mixed-case email.

Alternate B:
Normalize the email, accept the password, continue to pending account creation if unique.

## Pair 4.5 - Process verification link

Runtime A:
b4: Wait for verification callback.
Context: token exists, has not expired, and has not been used.

Expected B:
Processed result: verification accepted. Mark token as used and delete it. Mark the account
verified. Continue to user activation.

Alternate A:
The user clicks a valid verification link.

Alternate B:
Accept the link once, consume the token, and advance the account from pending to verified.

## Pair 4.6 - Process expired verification link

Runtime A:
b4: Wait for verification callback.
Context: token expired after twenty-four hours.

Expected B:
Processed result: verification rejected because the link expired. Offer a resend path. A
new token must invalidate any older active token for the same email.

Alternate A:
The user clicks the verification link after the allowed time.

Alternate B:
Do not verify the account. Tell the next node that a new link is needed.

## Pair 4.7 - Process questionnaire delivery

Runtime A:
b5: Create and deliver initial questionnaire.
Context: account is active; tenant is "local-business-community"; chat channel is available.

Expected B:
Processed result: questionnaire required. Use tenant context when selecting questions.
Deliver through chat. Onboarding remains incomplete until the user answers and submits
business details.

Alternate A:
Active user needs the first setup questionnaire in chat.

Alternate B:
Prepare a questionnaire for this user, deliver it through chat, and keep onboarding open.

## Pair 4.8 - Process onboarding completion

Runtime A:
b6: Wait for questionnaire completion and close onboarding.
Context: profile exists, questionnaire completed, business details submitted.

Expected B:
Processed result: onboarding gate satisfied. All three required conditions are true.
Continue to onboarding completion event.

Alternate A:
The user answers the questionnaire and includes business details.

Alternate B:
Close onboarding because no required onboarding condition is missing.

## Pair 4.9 - Process rate limit breach

Runtime A:
b1: Check request rate before registration work.
Context: six attempts from the same IP address inside fifteen minutes.

Expected B:
Processed result: rate limit breached. Do not process the registration attempt. Return a
wait instruction and record the IP address for monitoring.

Alternate A:
The requester has exceeded five sign-up attempts in the current window.

Alternate B:
Block the attempt, keep the system unchanged, and tell the next node to return a retry
message.

## Pair 4.10 - Process questionnaire service failure

Runtime A:
b2: Preserve questionnaire request for retry.
Context: account active, questionnaire service unavailable.

Expected B:
Processed result: questionnaire work is delayed but not lost. The account remains active.
The questionnaire delivery message must be stored for retry or dead letter review.
Onboarding completion must not be published yet.

Alternate A:
Questionnaire delivery fails after account creation.

Alternate B:
Keep the user account, preserve the missing work, and leave onboarding incomplete.

---

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

# Node-06 - Result Translator Array

## Node purpose

Each Node-06 unit receives one formal result Rqi and translates it into natural user or
stakeholder language. It also sees all results from the same run to avoid contradiction.

## Pair 6.1 - Translate SSO success

Runtime A:
R1 says: active user account created, Google linked, platform session issued, questionnaire
delivery can begin.

Expected B:
"You are signed in with Google. Your account is ready, and we are preparing your first
setup questionnaire."

Alternate A:
R1 says: new SSO user signed in and account exists.

Alternate B:
"Your Google sign-up worked. You can enter the platform now."

## Pair 6.2 - Translate merged account

Runtime A:
R2 says: existing account preserved, Google added as another login method, no duplicate
account created.

Expected B:
"Your Google login has been linked to your existing account. You can now sign in with
Google or with your original email and password."

Alternate A:
R2 says: provider linked to existing user identity.

Alternate B:
"We found your existing account and added this sign-in method to it."

## Pair 6.3 - Translate pending email account

Runtime A:
R3 says: pending account created, verification token created, account not active until
verification.

Expected B:
"Almost there. We created your account and sent a verification step. Please check your
email to activate it."

Alternate A:
R3 says: email account is waiting for verification.

Alternate B:
"Your account is waiting for email confirmation before you can use it."

## Pair 6.4 - Translate verification email sent

Runtime A:
R4 says: verification email delivered, link expires in twenty-four hours.

Expected B:
"We sent a verification email to your address. Click the link within twenty-four hours to
activate your account."

Alternate A:
R4 says: confirmation link delivered successfully.

Alternate B:
"Check your inbox for the confirmation link. It expires tomorrow."

## Pair 6.5 - Translate email verified

Runtime A:
R5 says: token accepted and consumed, account activated, questionnaire path may begin.

Expected B:
"Your email is verified and your account is active. We can now start your setup
questionnaire."

Alternate A:
R5 says: valid link clicked and account activation succeeded.

Alternate B:
"Verification complete. Your account is ready."

## Pair 6.6 - Translate questionnaire delivered

Runtime A:
R6 says: questionnaire generated with tenant context and delivered through chat.

Expected B:
"We sent your first setup questionnaire in chat. Your answers help us show you more
relevant business connections."

Alternate A:
R6 says: setup questions are available in the chat channel.

Alternate B:
"Your setup questions are waiting in chat."

## Pair 6.7 - Translate onboarding complete

Runtime A:
R7 says: onboarding marked complete and personalization events may start.

Expected B:
"You are all set. We can now personalize your matches, feed, and dashboard."

Alternate A:
R7 says: profile, questionnaire, and business details are complete.

Alternate B:
"Onboarding is complete. The platform can start tailoring your experience."

## Pair 6.8 - Translate expired link

Runtime A:
R_expired says: verification rejected because token expired; offer resend path.

Expected B:
"This verification link has expired. Ask for a new link and we will send another one."

Alternate A:
R_expired says: twenty-four hour verification window passed.

Alternate B:
"That link is no longer valid. You can request a fresh verification email."

## Pair 6.9 - Translate rate limit

Runtime A:
R8 says: rate limit exceeded, no account changed, wait before retrying.

Expected B:
"Too many sign-up attempts. Please wait fifteen minutes before trying again."

Alternate A:
R8 says: registration blocked before work began.

Alternate B:
"Please pause before trying again. We temporarily blocked registration attempts from this
source."

## Pair 6.10 - Translate recoverable questionnaire failure

Runtime A:
R10 says: account active, questionnaire not delivered yet, work preserved for retry, onboarding
not complete.

Expected B:
"Your account is ready. Your setup questionnaire will arrive shortly."

Alternate A:
R10 says: questionnaire service failed but the user account is usable.

Alternate B:
"You can start using your account now. We will send the setup questions as soon as they are
available."

## Pair 6.11 - Translate provider outage

Runtime A:
R9 says: Google sign-up temporarily disabled; other providers and email remain available.

Expected B:
"Google sign-up is temporarily unavailable. You can continue with another provider or sign
up by email."

Alternate A:
R9 says: one provider down but other registration paths open.

Alternate B:
"That sign-up option is unavailable right now, but the other sign-up options still work."

## Pair 6.12 - Consistency rejection

Runtime A:
All results for one run include: R1 says "registration method: Google SSO" and R3 says
"email verification required before activation."

Expected B:
Do not translate as final user language. Flag the run as internally inconsistent. One
single registration session cannot both be an already-active SSO registration and an
email-pending registration unless the plan explicitly describes a merge or link flow.
Send the inconsistent fragments to arbitration and reprocessing.

Alternate A:
Results disagree about whether the account is SSO-active or email-pending.

Alternate B:
Stop translation for this run. Ask for reprocessing of the branch decision.

---

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

# Node-08 - Arbiter Units

## Node purpose

Each selected arbiter receives the relevant result. It either passes, or flags a concrete
problem and emits fbx for reprocessing. The fbx text is the fragment sent back into the
front of the flow.

## Pair 8.1 - Security arbiter: wrong token storage

Runtime A:
Arbiter: security arbiter.
Result: platform session tokens are stored in browser local storage.

Expected B:
Flag it. The registration document requires platform tokens to be stored in httpOnly,
Secure, SameSite cookies, not browser local storage. Browser local storage can be reached
by page scripts and is not the required storage location.
fbx: Reprocess token storage. Store platform tokens in httpOnly, Secure, SameSite cookies.

Alternate A:
Result says the access token is saved in local storage after SSO.

Alternate B:
Flag. Send the token storage fragment back for correction to secure cookie storage.

## Pair 8.2 - Security arbiter: wrong rate limit

Runtime A:
Arbiter: security arbiter.
Result: registration allows ten attempts per IP address in fifteen minutes.

Expected B:
Flag it. The document states five registration attempts per IP address per fifteen minutes.
Ten attempts is not the documented control.
fbx: Reprocess rate limiting. Use five attempts per IP address per fifteen-minute window.

Alternate A:
The result doubles the allowed sign-up attempts.

Alternate B:
Flag. Correct the limit to five attempts in the defined window.

## Pair 8.3 - Security arbiter: account enumeration leak

Runtime A:
Arbiter: security arbiter.
Result: the response for an unknown email says "This email is not registered."

Expected B:
Flag it. The visible response reveals whether an email exists. The document requires the
same visible response for "email exists" and "email not found."
fbx: Reprocess the user-visible email response. Use a neutral message that does not reveal
registration status.

Alternate A:
The result tells the user that the email is already registered.

Alternate B:
Flag. Replace email-existence wording with a neutral response.

## Pair 8.4 - Security arbiter: weak verification token

Runtime A:
Arbiter: security arbiter.
Result: verification token is eight random characters.

Expected B:
Flag it. The document requires a cryptographically random 256-bit verification token. An
eight-character token is too weak for this flow.
fbx: Reprocess verification token generation. Use a cryptographically random 256-bit,
single-use token with twenty-four hour expiry.

Alternate A:
The verification link uses a short alphanumeric code.

Alternate B:
Flag. Replace it with the required strong single-use token.

## Pair 8.5 - Security arbiter: wrong password storage

Runtime A:
Arbiter: security arbiter.
Result: password is stored using a fast general hash.

Expected B:
Flag it. The document requires bcrypt with cost factor 12 for the email registration path.
Fast general hashes do not meet the password storage rule.
fbx: Reprocess password storage. Use bcrypt with cost factor 12.

Alternate A:
The result stores the password using MD5 or a plain fast hash.

Alternate B:
Flag. Apply the required password hashing rule.

## Pair 8.6 - Event sequence arbiter: event order reversed

Runtime A:
Arbiter: event sequence arbiter.
Result: UserCreated is published before UserSSOAuthenticated in the SSO path.

Expected B:
Flag it. The SSO path announces authentication before user creation. Reversing the order
means downstream services may react before the authentication fact exists.
fbx: Reprocess SSO event order. Publish UserSSOAuthenticated before UserCreated.

Alternate A:
The event chain starts with user creation instead of SSO authentication.

Alternate B:
Flag. Correct the event sequence before final formatting.

## Pair 8.7 - Data integrity arbiter: merge changes user identity

Runtime A:
Arbiter: data integrity arbiter.
Result: account merge assigns a new user identity and deprecates the old one.

Expected B:
Flag it. The account merge rule says the older user identity must be preserved so existing
references remain valid. Creating a new identity breaks downstream relationships.
fbx: Reprocess merge logic. Preserve the older user identity and add the SSO provider to
that existing account.

Alternate A:
The merge creates a fresh primary account.

Alternate B:
Flag. Keep the existing account identity and attach the provider.

## Pair 8.8 - Resilience arbiter: failed work discarded

Runtime A:
Arbiter: resilience arbiter.
Result: questionnaire delivery failed and the message was logged then discarded.

Expected B:
Flag it. The document requires failed event handlers to write to the dead letter queue for
manual review. Discarding the message loses the questionnaire work permanently.
fbx: Reprocess failure handling. Preserve the failed questionnaire delivery in retry or
dead letter review.

Alternate A:
Questionnaire service failure is treated as final and no retry evidence remains.

Alternate B:
Flag. Keep the failed work recoverable.

## Pair 8.9 - Tenant context arbiter: tenant lost

Runtime A:
Arbiter: tenant context arbiter.
Result: user selected a business community during registration, but the profile and
questionnaire were created without tenant context.

Expected B:
Flag it. The multi-tenant scenario says tenant context is stored with the user profile
from the start and the questionnaire may vary per tenant. Losing tenant context removes a
documented input to onboarding.
fbx: Reprocess tenant handling. Store the selected tenant with the profile before
questionnaire generation, then generate a tenant-aware questionnaire.

Alternate A:
The first questionnaire ignores the community selected at sign-up.

Alternate B:
Flag. Send tenant capture and questionnaire generation back for reprocessing.

## Pair 8.10 - Onboarding gate arbiter: completion too early

Runtime A:
Arbiter: onboarding gate arbiter.
Result: UserOnboardingCompleted published after questionnaire was sent, before the user
answered.

Expected B:
Flag it. The document requires all three completion criteria: profile created,
questionnaire completed, and business details submitted. Sending the questionnaire is not
the same as completing it.
fbx: Reprocess onboarding completion. Wait for questionnaire answers and business details
before publishing UserOnboardingCompleted.

Alternate A:
The flow marks onboarding complete before questionnaire responses exist.

Alternate B:
Flag. Keep onboarding open until all criteria are satisfied.

## Pair 8.11 - Pass case

Runtime A:
Arbiter: security arbiter and event sequence arbiter.
Result: tokens stored in secure cookies, rate limit is five attempts per IP in fifteen
minutes, email existence response is neutral, SSO events are in order, and merge preserves
the old user identity.

Expected B:
Pass. No fbx is needed.

Alternate A:
The reviewed result matches the documented security and event-order rules.

Alternate B:
Pass.

---

# Node-09 - Formatter

## Node purpose

Node-09 receives approved user-language fragments from Node-06 after arbitration. It folds
them into one readable output for the intended audience. It does not expose internal
review decisions unless the audience asked for examination.

## Pair 9.1 - New user after Google sign-up

Runtime A:
Approved fragments:
- "You are signed in with Google."
- "Your account is ready."
- "We sent your first setup questionnaire in chat."
Audience: new end user.

Expected B:
"You are signed in with Google. Welcome! Your account is ready, and your first setup
questionnaire is waiting in chat. Answer it to help us show you better business matches."

Alternate A:
Approved SSO success plus questionnaire delivery fragments for an end user.

Alternate B:
"Welcome. Your Google sign-up is complete, and the setup questions are in chat so we can
personalize your matches."

## Pair 9.2 - Email registration waiting for verification

Runtime A:
Approved fragments:
- "Your account was created as pending."
- "We sent a verification email."
- "The link expires in twenty-four hours."
Audience: new end user.

Expected B:
"Almost there. We sent a verification email to your address. Click the link within
twenty-four hours to activate your account."

Alternate A:
Approved pending account and verification email fragments.

Alternate B:
"Check your inbox to finish registration. The verification link is valid for twenty-four
hours."

## Pair 9.3 - Email verified and questionnaire ready

Runtime A:
Approved fragments:
- "Your email is verified."
- "Your account is active."
- "Your setup questionnaire is waiting in chat."
Audience: new end user.

Expected B:
"Your email is verified and your account is active. Your setup questionnaire is now waiting
in chat."

Alternate A:
Approved activation plus onboarding fragments.

Alternate B:
"Verification complete. You can use your account now, and the setup questions are in chat."

## Pair 9.4 - Onboarding complete

Runtime A:
Approved fragments:
- "Profile exists."
- "Questionnaire completed."
- "Business details submitted."
- "The platform can now personalize matches, feed, and dashboard."
Audience: new end user.

Expected B:
"You are all set. Your profile and business details are complete, so we can now personalize
your matches, feed, and dashboard."

Alternate A:
Approved onboarding completion fragments.

Alternate B:
"Onboarding is complete. The platform can start tailoring your experience."

## Pair 9.5 - Provider unavailable

Runtime A:
Approved fragments:
- "Google sign-up is temporarily unavailable."
- "Other providers and email registration still work."
- "No registration state was lost."
Audience: new end user.

Expected B:
"Google sign-up is temporarily unavailable. You can continue with another provider or sign
up by email."

Alternate A:
Approved provider outage fragments for a registration page.

Alternate B:
"That sign-up option is unavailable right now, but the other sign-up options are still
open."

## Pair 9.6 - Recoverable questionnaire delay

Runtime A:
Approved fragments:
- "Your account is active."
- "The questionnaire was not delivered yet."
- "The work was preserved for retry."
Audience: new end user.

Expected B:
"Your account is ready. Your setup questionnaire will arrive shortly."

Alternate A:
Approved active account plus delayed questionnaire fragments.

Alternate B:
"You can start using your account now. We will send the setup questions as soon as they
are available."

## Pair 9.7 - Product manager summary

Runtime A:
Approved fragments:
- "Registration path: Google SSO."
- "Account created."
- "Questionnaire sent."
- "Onboarding completed."
- "No drop-off detected."
Audience: product manager.

Expected B:
"Registration completed. Path: Google SSO. Account creation succeeded, questionnaire was
sent, and onboarding was completed. No drop-off was detected in this run."

Alternate A:
Approved funnel fragments for product review.

Alternate B:
"Funnel result: completed through Google SSO. The user reached onboarding completion with
no recorded drop-off."

## Pair 9.8 - Security manager summary

Runtime A:
Approved fragments:
- "Provider sign-in was used."
- "Tokens stored in secure cookies."
- "Rate limit applied at five attempts per IP per fifteen minutes."
- "Email existence response is neutral."
- "No security arbiter flags."
Audience: security manager.

Expected B:
"Security review passed. Provider sign-in was used, session tokens were stored in secure
cookies, rate limiting matched the five-attempt rule, and email existence was not exposed
in the user-visible response."

Alternate A:
Approved security fragments.

Alternate B:
"Registration security controls matched the document: secure cookie storage, correct rate
limit, neutral email-existence wording, and no security flags."

## Pair 9.9 - Operations summary

Runtime A:
Approved fragments:
- "Auth path succeeded."
- "Email delivery was delayed but preserved for retry."
- "Dead letter review contains the failed message."
- "Onboarding completion was not published."
Audience: operations team.

Expected B:
"Registration remained recoverable. The account path succeeded, the delayed email work was
preserved for retry or dead letter review, and onboarding completion was not published
early."

Alternate A:
Approved operations fragments for a recoverable failure.

Alternate B:
"Operational state: account ready, follow-up work preserved, no premature completion event."

## Pair 9.10 - Examination answer for a human reviewer

Runtime A:
Approved fragments:
- "Domain splitter selected authentication, onboarding, communication, and event coordination."
- "Planner produced SSO account creation, questionnaire delivery, and onboarding completion."
- "Arbiters passed security and event sequence."
Audience: human examining the flow output.

Expected B:
"The flow understood the request as a registration task touching authentication,
communication, onboarding, and event coordination. It planned the SSO account path, the
chat questionnaire, and the onboarding completion event. Security and event-order review
passed, so the approved result can be shown to the user."

Alternate A:
Approved trace fragments for explainable review.

Alternate B:
"The request was routed to the right registration domains, planned through account creation
and onboarding, reviewed for security and event order, and approved for final presentation."

---

# Simulation Checks

## Simulation 1 - Google sign-up with questionnaire

Input prompt:
"I want users to sign up with Google and then receive a quick questionnaire in chat."

Expected node path:
- Node-01: [authentication, onboarding, communication]
- Node-02: Google SSO wording; chat questionnaire wording
- Node-03: provider callback, account creation, session issue, questionnaire delivery,
  onboarding wait
- Node-04: process provider profile, account decision, questionnaire requirement
- Node-05: account created, session issued, questionnaire delivered
- Node-06: user-friendly SSO success and questionnaire message
- Node-07: select security, event sequence, privacy, onboarding arbiters
- Node-08: pass only if token storage, event order, and onboarding gate are correct
- Node-09: one end-user answer: signed in with Google, questionnaire in chat

Missing-if-fail:
If the simulation does not mention questionnaire delivery, Node-03 or Node-05 lacks coverage.
If it says onboarding is complete before answers, Node-08 lacks gate coverage.

## Simulation 2 - Email sign-up with expired verification link

Input prompt:
"A user registers by email but clicks the verification link the next day after it expired."

Expected node path:
- Node-01: [authentication, communication, security, user management]
- Node-02: pending email account, verification token, expired link, resend path
- Node-03: validate credentials, create pending account, send email, wait for callback,
  expired-token branch
- Node-04: process expired token as rejected verification
- Node-05: no activation; resend path available
- Node-06: "This verification link has expired. Ask for a new link."
- Node-07: select security and communication arbiters
- Node-08: pass only if old token is not reused
- Node-09: concise user message with resend instruction

Missing-if-fail:
If the simulation activates the account, Node-04 or Node-08 is wrong.
If it hides the resend path, Node-06 or Node-09 is thin.

## Simulation 3 - Existing email account tries Google SSO

Input prompt:
"I registered with email before, and now I want to sign in with Google using the same email."

Expected node path:
- Node-01: [authentication, user management, event coordination]
- Node-02: account merge wording
- Node-03: normalize email, match existing account, link provider, preserve identity
- Node-04: account merge required
- Node-05: existing user identity preserved, Google linked
- Node-06: "Your Google login has been linked to your existing account."
- Node-07: select data integrity, identity consistency, event sequence
- Node-08: flag if a new primary user identity is created
- Node-09: final user answer: Google added, original login still works

Missing-if-fail:
If the simulation creates a duplicate user, Node-03, Node-04, or Node-08 lacks coverage.

## Simulation 4 - Questionnaire service down

Input prompt:
"Registration succeeds, but the questionnaire service is down. What should happen?"

Expected node path:
- Node-01: [operations, onboarding, communication, user management]
- Node-02: account remains ready; questionnaire work preserved
- Node-03: complete account, preserve questionnaire request, show usable state, resume later
- Node-04: active account, delayed questionnaire, no onboarding completion
- Node-05: retry or dead letter review result
- Node-06: "Your account is ready. Your setup questionnaire will arrive shortly."
- Node-07: select resilience, onboarding gate, operations
- Node-08: flag if the failed questionnaire message is discarded or completion is published
- Node-09: final answer does not expose noisy internal failure to the end user

Missing-if-fail:
If the simulation rolls back the account, Node-03 or Node-05 is wrong.
If it publishes onboarding complete, Node-08 is wrong.

## Final self-check table

| Node | Runtime pairs in this file | Required shape checked |
|---|---:|---|
| Node-01 Domain splitter | 10 | Human prompt -> domain labels |
| Node-02 Domain translator | 10 | Fragment plus domain -> concrete formal wording |
| Node-03 Scheme planner | 10 | Domain wordings -> b1...bn plus fb1...fbn |
| Node-04 Domain processor array | 10 | One planned piece -> processed result |
| Node-05 Applier array | 10 | Processed piece -> concrete Rqi |
| Node-06 Result translator array | 12 | Formal result -> user/stakeholder wording |
| Node-07 Arbiter selector | 10 | Result group -> selected arbiters with reasons |
| Node-08 Arbiter units | 11 | Result review -> pass or flag plus fbx |
| Node-09 Formatter | 10 | Approved fragments -> one final answer |

Total runtime-shaped pairs: 93.

The source document has not been shortened into training input. It is used as evidence
for which runtime situations must be taught.
