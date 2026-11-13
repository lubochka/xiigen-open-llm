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

