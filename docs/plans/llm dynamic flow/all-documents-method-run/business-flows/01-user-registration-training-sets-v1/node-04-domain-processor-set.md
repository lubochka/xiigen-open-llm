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

