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

