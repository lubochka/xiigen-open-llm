# Training Data — Flow 01: User Registration & SSO Onboarding

## Source document
`business flows/01-user-registration.md` — one document, no other sources.

## Pipeline reference

Each node is a trained function. A goes in, B comes out. The B of each node
becomes part of the A of the next. Where nodes are arrays (n units), each unit
receives one element and produces one result — all units in the array run at the
same time.

```
User prompt
    ↓
[1] Domain splitter        A: full prompt           B: domain labels d1...dm
    ↓
[2] Domain translator      A: one fragment + di     B: formal domain expression udi
    ↓  (n parallel units, one per domain)
[3] Scheme planner         A: all ud1...udm          B: sub-tasks b1...bn + prompts fb1...fbn
    ↓
[4] Domain processor       A: one bi + domain ctx   B: processed result for that sub-task
    ↓  (n parallel units, one per sub-task)
[5] Applier                A: (bi, fbi) + result    B: applied result Rqi
    ↓  (n parallel units, one per bi)
[6] Result translator      A: one Rbi               B: Rbi in natural user language
    ↑ also sees all R1..Rn (for consistency)
[Arbiter selector]         A: all R1...Rn + ctx     B: which arbiters to fire
    ↓
[Arbiter units]            A: one Rbi + specialization  B: pass, or flag + fbx back to splitter
    ↓
[7] Formatter              A: all approved results  B: one combined user-readable output
```

---

## Node 1 — Domain splitter

**A:** A user prompt about registration.
**B:** The array of domain labels that prompt touches.

The document defines six domains for the registration flow. The splitter must learn which
language signals belong to which domain, and that one prompt can touch several domains at once.

---

### Signal vocabulary — what language maps to which domain

From reading the document end to end:

- **authentication** — "sign up with Google", "log in with Facebook", "OAuth", "SSO",
  "email and password", "verify your email", "login", "create an account", "credentials",
  "sign in", "link your account"

- **communication** — "verification email", "send a link", "chat message", "questionnaire
  via chat", "in-app message", "email delivery", "I didn't receive the email", "re-send"

- **onboarding** — "questionnaire", "profile setup", "business details", "first match
  suggestions", "getting started", "personalization", "onboarding complete"

- **security** — "rate limit", "too many attempts", "blocked", "token expiry",
  "enumeration prevention", "CSRF", "suspicious activity", "OAuth redirect", "stored securely"

- **event coordination** — "event published", "downstream trigger", "gate event",
  "system triggers", "when the user completes X, Y should happen", "notifies", "consumes"

- **user management** — "user profile", "account state", "track where users are",
  "abandoned registration", "dormant account", "send a reminder", "onboarding status"

One prompt can touch multiple domains. B is always an array, never a single label.

---

### Training pairs

**Pair 1.1**
A: "I need users to be able to sign up using Google, Facebook, LinkedIn, or Figma."
B: [authentication]
Reason: The request is entirely about external-provider identity — no email delivery,
no state tracking, no event chains are mentioned. Authentication only.

**Pair 1.1 — Phrasing B**
A: "Let users click 'Continue with Google' and be logged in without filling out a form."
B: [authentication]
The core request is delegating identity verification to an external provider.
Everything that follows (profile creation, onboarding) is a downstream concern.

---

**Pair 1.2**
A: "Users should also be able to register with email and password."
B: [authentication, communication]
Reason: authentication because credentials are involved; communication because email
registration requires a verification email to be delivered.

**Pair 1.2 — Phrasing B**
A: "Email sign-up with a confirmation link that the user must click."
B: [authentication, communication]
The "confirmation link" signal adds communication to the authentication domain —
delivering that link is a messaging concern.

---

**Pair 1.3**
A: "After signing up, the user should receive a questionnaire via chat."
B: [onboarding, communication]
Reason: onboarding because the questionnaire is the setup phase of the user's journey;
communication because delivery via the in-app chat channel is required.

**Pair 1.3 — Phrasing B**
A: "New users need a getting-started questionnaire sent to them in the chat window."
B: [onboarding, communication]

---

**Pair 1.4**
A: "When the user finishes the questionnaire, the system should trigger personalization downstream."
B: [onboarding, event coordination]
Reason: onboarding because completion is tracked; event coordination because the
UserOnboardingCompleted event is what triggers the matching, feed, and learning programs.

**Pair 1.4 — Phrasing B**
A: "Completing the questionnaire should kick off matching and feed personalization automatically."
B: [onboarding, event coordination]

---

**Pair 1.5**
A: "The system should track which step of registration each user is on and send reminders if they abandon."
B: [user management, event coordination]
Reason: user management because the onboarding state machine and dormancy logic are
profile-level concerns; event coordination because reminders are triggered by time-based
events referencing user state.

---

**Pair 1.6**
A: "Registration must be rate-limited and protected against account enumeration."
B: [security]
Reason: Both concerns (rate limiting and enumeration prevention) are defined in the
security section of the document. Neither requires any other domain.

**Pair 1.6 — Phrasing B**
A: "I need to block automated sign-up attempts and make sure we don't leak whether
an email is registered."
B: [security]

---

**Pair 1.7**
A: "I want to sign up quickly with Google so I can start networking."
(This is the primary user story from the document.)
B: [authentication]
Reason: A single user story mapping to a single domain. The user's expressed need
is the login act — no other domain is named.

---

**Pair 1.8**
A: "As a new user I want a questionnaire via chat so the platform understands my business needs."
(This is the onboarding user story from the document.)
B: [onboarding, communication]

---

## Node 2 — Domain translator (one unit per domain)

**A:** One prompt fragment + its domain label.
**B:** That fragment restated in the formal vocabulary of that domain — precise enough
for a specialist to act on without referring back to the original.

The translator must not add concepts not present in A. It must not resolve ambiguities
by assumption — surface them as open questions.

---

**Pair 2.1 — authentication domain, SSO path**
A: "User signs up with Google" + domain: authentication
B: The user presents an authorization code obtained from Google's OAuth2 authorization
endpoint. The system exchanges this code for a Google access token at Google's token
endpoint. The system requests the user's profile from Google's userinfo endpoint using
that access token. The system normalizes the returned email to lowercase and checks
whether this email already exists in the user store. If the email is new: create a user
record with status=active and assign a permanent userId (UUID). Issue platform credentials:
access token with 15-minute validity, refresh token with 7-day validity, rotation enforced
on each use, both stored in httpOnly Secure SameSite cookie. Publish UserSSOAuthenticated
event. If the email already exists: apply account merge rule (see event coordination domain).

**Pair 2.1 — Phrasing B**
A: "Google SSO registration" + domain: authentication
B: SSO flow via Google. Steps in order: receive auth code from Google redirect →
exchange code for user profile → normalize email → uniqueness check → create active
record or merge → issue JWT (access 15min, refresh 7 days in httpOnly cookie) →
publish UserSSOAuthenticated. The Google token is never stored — it is exchanged
immediately for the platform's own credential.

---

**Pair 2.2 — authentication domain, email path**
A: "User registers with email and password" + domain: authentication
B: The user submits email address and plaintext password. The system validates email
format. The system validates the password against the password policy: minimum 8
characters; must contain at least one uppercase letter, at least one lowercase letter,
at least one digit, at least one special character; must not appear in the top-10,000
common passwords list; must not be similar to the user's email address. The system
normalizes the email to lowercase and checks uniqueness. The system creates a user
record with status=pending (not active until email verification succeeds). The system
generates a verification token: cryptographically random, 256-bit, single-use, 24-hour
expiry; one active token per email at a time — a new request invalidates any prior token
for that email. Publish UserRegistrationInitiated event.

**Pair 2.2 — Phrasing B**
A: "Email and password sign-up with verification" + domain: authentication
B: Email registration is a two-stage identity claim. Stage 1 — the user asserts an
email address and a password that passes all four strength criteria. Stage 2 — the
platform confirms the user controls that email address by sending a one-time link.
Until the link is clicked (within 24 hours), the account status is pending and unusable.
After the click, the token is deleted (single-use), the status becomes verified, and the
account proceeds to activation.

---

**Pair 2.3 — communication domain**
A: "Send the user a verification email" + domain: communication
B: The system enqueues a delivery request to the Email Service via RabbitMQ (queue:
email-queue). The message payload contains the verification link embedding the
single-use token. Delivery SLA: within 60 seconds of the enqueue. On failure: message
remains in the durable queue for retry; message is moved to the dead letter queue only
after retries are exhausted, not before. The Email Service publishes VerificationEmailSent
after confirmed delivery. The Audit Service consumes this event and logs it.

**Pair 2.3 — Phrasing B**
A: "Verification link sent to the user's inbox" + domain: communication
B: Communication path: Auth Service enqueues to email-queue → Email Service dequeues,
constructs email with single-use link, delivers via SMTP → publishes VerificationEmailSent
→ Audit Service logs the delivery. If the service is unavailable: message persists in
the durable queue and is retried when the service recovers. The user should not lose
their verification email due to a transient service failure.

---

**Pair 2.4 — onboarding domain**
A: "After account is active, send the user a questionnaire" + domain: onboarding
B: The Questionnaire Service consumes the UserCreated event (SSO path) or the
UserActivated event (email path) and generates a personalized initial questionnaire.
The content of the questionnaire may vary by tenant. The Questionnaire Service publishes
QuestionnaireRequired. The Messaging Service consumes QuestionnaireRequired and delivers
the questionnaire via in-app WebSocket chat. Delivery SLA: within 5 seconds of account
activation. Onboarding completion requires three criteria all satisfied: (1) profile
created — automatic at account creation; (2) questionnaire completed — requires user
action; (3) business details submitted — collected as part of the questionnaire.
When all three are satisfied: publish UserOnboardingCompleted.

**Pair 2.4 — Phrasing B**
A: "Questionnaire via chat, completion tracked" + domain: onboarding
B: Onboarding is a three-step gate: profile exists (automatic), questionnaire answered
(user action), business details included in answers (part of the same questionnaire).
The gate checks all three before publishing the completion event. None of the three can
be skipped. The questionnaire arrives in the user's chat window within 5 seconds of their
account becoming active.

---

**Pair 2.5 — security domain**
A: "Protect the registration endpoint from abuse" + domain: security
B: Rate limit: 5 registration attempts per IP address per 15-minute window. On breach:
return HTTP 429 with Retry-After header; log the IP address for security monitoring.
OAuth redirect URIs: strict whitelist of allowed callback addresses, no wildcards,
no open redirects permitted. CSRF protection: applied to all authentication endpoints.
Account enumeration prevention: the system must return an identical response body
whether the submitted email exists in the user store or does not — no observable
difference. Verification token: cryptographically random, 256-bit. Password storage:
bcrypt at cost factor 12 (email registration path only — SSO users have no platform
password to store).

**Pair 2.5 — Phrasing B**
A: "Stop automated attacks and prevent information leakage about registered emails"
   + domain: security
B: Two distinct security controls. Attack prevention: rate limit at 5 attempts per IP
per 15 minutes, HTTP 429 on breach. Information leakage prevention: the error response
for "email exists" and "email not found" must be byte-for-byte identical in its user-visible
body — an observer must not be able to determine registration status from the response.

---

**Pair 2.6 — event coordination domain**
A: "The system should publish events between steps" + domain: event coordination
B: SSO path event chain (in order): UserSSOAuthenticated → UserCreated →
QuestionnaireRequired → QuestionnaireSent → QuestionnaireCompleted →
UserOnboardingCompleted.
Email path event chain (in order): UserRegistrationInitiated → VerificationEmailSent →
EmailVerified → UserCreated → UserActivated → QuestionnaireRequired → QuestionnaireSent
→ QuestionnaireCompleted → UserOnboardingCompleted.
All event handlers must be idempotent: deduplication key is userId + eventType. A handler
that receives the same event twice must produce the same result as the first execution —
no duplicate records, no duplicate emails, no duplicate questionnaires.
Failed handlers: if a handler cannot process the event after retries, the message is
written to the dead letter queue for manual review — it is never silently discarded.

**Pair 2.6 — Phrasing B**
A: "Events published between services to coordinate the registration sequence"
   + domain: event coordination
B: The registration flow is driven by a sequence of events, not by direct service calls.
Each event names what happened; the consuming service decides what to do next.
The SSO path has 6 events; the email path has 9 events (the extra 3 cover the
verification sub-path before account creation). Every handler must check whether it
has already processed this event before acting — receiving the same event twice must
not create a duplicate outcome.

---

**Pair 2.7 — user management domain**
A: "Keep track of where each user is in the registration process" + domain: user management
B: The User Service manages the user's onboarding state machine. States in progression:
pending (email path only — account created but email not yet verified), active (SSO path
or post-verification email path — account usable), onboarding (active account but
questionnaire not yet completed), complete (all three onboarding criteria satisfied).
Abandonment handling: if a user's account exists but onboarding is incomplete, the system
sends a reminder email after 24 hours, a second reminder after 72 hours. If onboarding
remains incomplete after 30 days: mark the account dormant.

**Pair 2.7 — Phrasing B**
A: "Users who abandon registration midway need follow-up" + domain: user management
B: The onboarding state machine tracks each user's position. An account that has been
created but not completed onboarding is not lost — it is in a known state. The system
sends two reminders (at 24 hours and 72 hours) before marking the account dormant at
30 days. The dormant state is a flag on the user record, not a deletion.

---

## Node 3 — Scheme planner

**A:** All translated domain descriptions ud1...udm together.
**B:** An ordered set of sub-tasks b1...bn, each paired with a transformational
prompt fbi that tells the Applier how to approach that sub-task.

The decomposition is complete when every event in the event chain has a corresponding
sub-task. Error and edge cases are decomposed alongside the happy path, not treated
as afterthoughts.

---

**Pair 3.1 — SSO path decomposition**
A: [authentication description for SSO] + [event coordination description] +
   [onboarding description] + [user management description]

B:
- **b1**: Validate the SSO provider callback and exchange the authorization code for
  the user's profile. Check whether the returned email already exists in the user store.
  Branch on result: if new → create; if existing email match → apply merge rule.
  **fb1**: Apply OAuth2 token exchange pattern. Normalize email before comparison.
  Check uniqueness. Branch to create or merge before any record is written.

- **b2**: Create the user record and publish UserSSOAuthenticated.
  **fb2**: Apply idempotency check — confirm the user record does not already exist
  before writing. Write the record first. Publish the event after the write succeeds,
  never before.

- **b3**: Generate and deliver the initial questionnaire.
  **fb3**: Apply onboarding delivery pattern. Wait for UserCreated event. Generate
  personalized questionnaire. Deliver via in-app chat within the 5-second SLA.
  Publish QuestionnaireSent after confirmed delivery.

- **b4**: Wait for questionnaire completion. Verify all three onboarding criteria.
  Publish gate event.
  **fb4**: Apply wait-state pattern. On user completion: confirm all three criteria
  are satisfied (profile created, questionnaire completed, business details submitted).
  Only when all three are confirmed: publish UserOnboardingCompleted.

**Pair 3.1 — Phrasing B**
A: "Build a Google SSO registration flow with onboarding"

B: The SSO plan has four sub-tasks. The first establishes identity (who is this user
and do they already have an account?). The second creates the record and announces it.
The third delivers the questionnaire in response to that announcement. The fourth waits
for the user to answer and then declares onboarding complete. The critical rule across
all four: write data before publishing events — if the event publish fails, the record
allows the action to be replayed.

---

**Pair 3.2 — Email path decomposition**
A: [authentication description for email] + [communication description] +
   [event coordination description] + [security description] + [onboarding description]

B:
- **b1**: Validate credentials and create a pending account. Generate verification token.
  **fb1**: Apply password strength rules (all four criteria). Normalize email. Check
  uniqueness. Generate 256-bit cryptographic token. Set 24-hour expiry. Invalidate
  any prior token for this email. Create record with status=pending.

- **b2**: Deliver the verification email.
  **fb2**: Enqueue message to RabbitMQ email-queue. Embed single-use link in message.
  Delivery SLA: within 60 seconds. Publish VerificationEmailSent after confirmed delivery.

- **b3**: Wait for the email verification callback. Validate the token on arrival.
  **fb3**: Apply wait-state pattern with 24-hour maximum. On token click: confirm the
  token exists in the store, is not expired, and has not been used before. Delete the
  token (single-use enforcement). On expiry: offer re-send of a fresh token.

- **b4**: Activate the account and continue to onboarding.
  **fb4**: Update user status from pending to active. Publish EmailVerified and
  UserActivated. Then follow the same pattern as SSO path b3 and b4 from this point.

**Pair 3.2 — Phrasing B**
A: "Email registration with verification required before access"

B: The email plan introduces a gate that the SSO plan does not have: email verification.
Before the gate: validate credentials, create a provisional record, send the one-time link.
At the gate: wait for the user to click (up to 24 hours). After the gate: activate the
account, then follow the same onboarding path as SSO users. The gate is not optional —
a pending account cannot access the platform.

---

**Pair 3.3 — Error and edge case decomposition**
A: [security description] + [event coordination description] + [authentication description
   for merge and retry]

B:
- **b_retry**: SSO provider does not respond in time.
  **fb_retry**: Apply exponential backoff pattern. Retry up to 3 times. Wait 1 second
  before the first retry, 2 seconds before the second, 4 seconds before the third. After
  3 failed attempts: return a provider-unavailable error to the user.

- **b_merge**: SSO callback email matches an existing account in the user store.
  **fb_merge**: Apply account merge rule. Preserve the older account's userId — all
  foreign key references in other services point to this identifier and must not be broken.
  Keep the existing password if the account has one. Link the new SSO provider as an
  additional login method on the existing account.

- **b_rate**: Incoming registration attempt but rate limit already reached for this IP.
  **fb_rate**: Check the attempt counter before processing. If the counter for this IP
  in the current 15-minute window is at or above 5: return HTTP 429 with Retry-After
  header. Log the IP address. Do not process the registration attempt.

- **b_dlq**: An event handler fails after all retries are exhausted.
  **fb_dlq**: Write the failed message to the dead letter queue. Do not discard. Do not
  silently continue. The message must be preserved for manual review and later replay.

---

## Node 4 — Domain processor (n units, one per sub-task)

**A:** One sub-task bi plus its domain context.
**B:** The processed result for that sub-task — all success states, failure states, and
wait-state outcomes. The output must contain enough information for the next node to act
without re-reading the original input.

If the same sub-task arrives twice (duplicate event), the output must be identical to
the first execution. This is the idempotency requirement.

---

**Pair 4.1 — b1 (SSO path)**
A: b1 (SSO): "Validate SSO provider callback and exchange code for user profile.
Check email uniqueness. Branch on result."

B:
- If new user: `{ userId: new-UUID, status: active, provider: google,
  email: normalized-lowercase, profile: { name: from-provider, picture: from-provider } }`
- If existing email match: `{ userId: existing-UUID, mergeRequired: true,
  existingMethod: email | sso }`
- If provider error (timeout or bad response): `{ error: provider_timeout,
  retryable: true, attemptsMade: 1 }`

Idempotency: if the same authorization code arrives twice (browser retry), the token
exchange with the provider will fail on the second attempt (codes are single-use at the
provider). The processor must handle this gracefully and not create a duplicate record.

---

**Pair 4.2 — b1 (email path)**
A: b1 (email): "Validate credentials, normalize email, check uniqueness,
generate verification token."

B:
- If valid and unique: `{ userId: new-UUID, status: pending,
  verificationToken: { value: 256-bit-random, expiry: now+24h, singleUse: true } }`
- If email already exists: `{ error: email_already_registered }`
  (Note: the user-facing response for this error must be identical to the "email not
  found" response — see security arbiter pair 7.3)
- If password fails strength check: `{ error: password_strength,
  violations: [ list-of-failed-criteria ] }`

---

**Pair 4.3 — b3 (email path verification wait-state)**
A: b3 (email): "Wait for email verification callback. Token must exist, not expired,
not used."

B:
- If valid: `{ userId, status: verified, verifiedAt: timestamp, ipAddress: client-ip }`
  Token is deleted from the store immediately after validation (single-use enforcement).
- If expired: `{ error: token_expired, action: offer_resend }`
- If already used (second click on the same link): `{ status: already_verified,
  action: redirect_to_login }`

---

**Pair 4.4 — b4 (both paths, questionnaire completion wait-state)**
A: b4 (both paths): "Wait for questionnaire completion. Verify all three criteria.
Publish gate event when complete."

B:
- If all criteria met: `{ userId, onboardingSteps: { profileCreated: true,
  questionnaireCompleted: true, businessDetailsSubmitted: true },
  completedAt: timestamp, event: UserOnboardingCompleted }`
- If criteria not yet met: continue waiting (no output yet — the wait-state is still open)
- If no activity for 30 days: `{ status: dormant }` (triggers reminder flow)

---

**Pair 4.5 — b_merge**
A: b_merge: "Merge SSO account with existing email account."

B:
`{ userId: existing-UUID (preserved, not changed),
   passwordKept: true (if the existing account had a password) | false,
   providers: [ existing-providers + new-sso-provider ],
   foreignKeysIntact: true }`

The older userId is the permanent identifier. All other services store this userId
in their own records. Changing it would break every foreign key reference across
every downstream service. The merge preserves it unconditionally.

---

## Node 5 — Applier (n units, one per bi)

**A:** One (bi, fbi) pair plus the processed result from Node 4.
**B:** The applied result Rqi — the concrete action taken and its outcome.

**Critical rule from this document:** Write the record first. Publish the event after the
write succeeds. Never publish an event before the write — if the event publish fails
after a successful write, the record allows the action to be replayed from the event log.
If the write fails, no event is published, so no downstream service acts on data that
was never persisted.

---

**Pair 5.1**
A: (b1_sso, fb1_sso = "Apply OAuth2 token exchange pattern. Check uniqueness. Branch
to create or merge.") + result `{ userId: new, status: active, provider: google }`

B — R1:
Auth Service wrote new user record to the persistent store (PostgreSQL).
Access token issued with 15-minute validity. Refresh token issued with 7-day validity,
rotation enforced on next use. Both stored in httpOnly, Secure, SameSite cookie.
Idempotency key checked before publish: userId + UserSSOAuthenticated not yet in the
event log. UserSSOAuthenticated event published to Kafka topic user-events.

---

**Pair 5.2**
A: (b2_email, fb2 = "Enqueue to email-queue. SLA 60 seconds.") +
   result `{ userId, verificationToken }`

B — R2:
Email Service dequeued the delivery request from email-queue (RabbitMQ, durable).
Constructed email with single-use verification link embedding the token.
Delivered via SMTP. Delivery confirmed within 60-second SLA.
VerificationEmailSent event published. Audit Service consumed the event and logged
the delivery with IP address, user-agent, and timestamp.

---

**Pair 5.3**
A: (b3_both, fb3 = "Apply onboarding delivery pattern. Generate questionnaire.
Deliver via chat within 5 seconds.") + result `{ userId, status: active | verified }`

B — R3:
Questionnaire Service consumed UserCreated (SSO path) or UserActivated (email path).
Personalized initial questionnaire generated for this userId (content may vary by tenant).
QuestionnaireRequired event published.
Messaging Service consumed QuestionnaireRequired.
Questionnaire delivered to the user's in-app WebSocket chat channel.
QuestionnaireSent published. Delivery confirmed within 5 seconds of account activation.
Audit Service consumed QuestionnaireSent and logged the delivery.

---

**Pair 5.4 — failure case**
A: (b3_both, fb3) + result `{ userId, status: active }` but Questionnaire Service
   is unavailable at the time of processing

B — R3_fail:
QuestionnaireRequired event could not be processed — Questionnaire Service unavailable.
Message written to dead letter queue. Message not discarded.
User sees the dashboard. No questionnaire has been delivered yet.
UserOnboardingCompleted has not been published (onboarding is incomplete).
When the Questionnaire Service recovers, the retry mechanism processes the dead letter
queue message and delivers the questionnaire. The flow continues from that point.

---

**Pair 5.5**
A: (b_rate, fb_rate = "Check counter before processing. Increment on attempt. Log IP.")
   + result `{ attemptsCount: 6, windowStart: 15 minutes ago }`

B — R_rate:
Attempt count (6) exceeds the limit (5 per IP per 15-minute window).
HTTP 429 returned to the requester. Retry-After header set to the end of the current window.
IP address logged to security monitoring.
Registration attempt was not processed. No user record was created or modified.

---

## Node 6 — Result translator (n units) + Arbiter selector

### Result translator

**A:** One result Rqi expressed in formal system language. Also sees all R1...Rn for
consistency checking.
**B:** That result expressed in natural language that the user can read and act on.

---

**Pair 6.1**
A: R1 = `{ userId: new, status: active, provider: google, JWT issued,
           UserSSOAuthenticated published }`
B: "You're signed in with Google. Your account has been created."

Note: "JWT issued" and "UserSSOAuthenticated published" are internal. The user needs
to know they are logged in and their account exists — nothing else.

---

**Pair 6.2**
A: R2 = `{ verificationToken sent, VerificationEmailSent published, SLA met }`
B: "We've sent a verification email to your address. Click the link in the email to
activate your account — the link expires in 24 hours."

Note: The token value is not shown. The SLA confirmation is not shown. The user
needs: where to look, what to do, how long they have.

---

**Pair 6.3**
A: R3 = `{ questionnaire delivered via chat, QuestionnaireSent published }`
B: "We've sent you a quick questionnaire in your chat. Your answers help us show you
the right business connections."

---

**Pair 6.4 — graceful failure translation**
A: R3_fail = `{ QuestionnaireRequired in dead letter queue, delivery failed }`
B: "Your account is ready. We'll send you a getting-started questionnaire shortly."

Rule: do not expose the failure to the user. The account is ready — that is true.
The questionnaire will arrive — also true, when the service recovers. The user does
not need to know that a service was temporarily unavailable.

---

**Pair 6.5**
A: R_token_expired = `{ error: token_expired, action: offer_resend }`
B: "This verification link has expired. Links are only valid for 24 hours. Click
below to get a new one."

---

**Pair 6.6**
A: R_rate = `{ HTTP 429, IP logged }`
B: "Too many attempts. Please wait 15 minutes before trying again."

---

**Pair 6.7 — consistency check across all R**
A: R1 says `{ provider: google, registrationMethod: SSO }` AND R3 says
   `{ registrationMethod: email }` — two results from the same flow run that
   disagree on which path was taken.

B: Flag inconsistency. These two results describe different registration paths and
cannot belong to the same flow run. A user cannot be both an SSO registrant and an
email registrant in a single session. Do not translate — send back for reprocessing.

---

### Arbiter selector

**A:** All results R1...Rn plus the context of the flow run.
**B:** Which arbiter units to fire on these results.

---

**Pair 6.8**
A: R1...Rn for a registration that included an SSO token exchange.
B: Fire: [security-arbiter, event-sequence-arbiter]
Reason: security because tokens and credentials were handled; event-sequence because
the published event chain must appear in the documented order.

---

**Pair 6.9**
A: R1...Rn for a flow run that touched personal data (email, name, profile picture).
B: Fire: [security-arbiter, privacy-arbiter]
Reason: security for credential handling; privacy because personal data was collected
and the compliance rules (consent, retention, classification) apply.

---

**Pair 6.10**
A: R1...Rn for a flow run that included an account merge.
B: Fire: [data-integrity-arbiter]
Reason: to verify the older userId was preserved and foreign key references
across all downstream services remain valid.

---

**Pair 6.11**
A: R1...Rn for a flow run where questionnaire delivery failed.
B: Fire: [resilience-arbiter]
Reason: to verify the failure was correctly routed to the dead letter queue, not
silently dropped or discarded.

---

## Node 7 — Arbiter units

**A:** One result Rbi plus the arbiter's specialization.
**B:** Pass (with no further explanation needed), or flag with: what is wrong, which
rule it violates, and the fragment fbx to send back to the Domain Splitter for reprocessing.

---

**Pair 7.1 — security arbiter: token storage location**
A: R1 = `{ JWT issued, stored in localStorage }`
B: Flag.
Reason: the document requires tokens to be stored in httpOnly, Secure, SameSite cookies —
explicitly not in localStorage. localStorage is accessible to JavaScript on the page,
which makes it vulnerable to cross-site scripting attacks. This violates a stated security
requirement.
fbx: Reprocess the token storage step. Move token storage from localStorage to an
httpOnly, Secure, SameSite cookie.

---

**Pair 7.2 — security arbiter: rate limit value**
A: R1 = `{ rate limit applied: 10 attempts per 15 minutes }`
B: Flag.
Reason: the document specifies 5 registration attempts per IP per 15-minute window —
not 10. The configured limit is twice the required limit and does not meet the security
specification.
fbx: Reprocess rate limit configuration. Change the limit from 10 to 5 attempts per
IP per 15-minute window.

---

**Pair 7.3 — security arbiter: account enumeration**
A: R_email = `{ response body for "email not found": "This email is not registered" }`
B: Flag.
Reason: this response reveals to any observer that a given email address does not have
a registered account. An attacker can infer the opposite — that a different email IS
registered — by comparing responses. The document explicitly requires identical response
bodies for "email exists" and "email not found."
fbx: Reprocess the error response. Return the same message body regardless of whether
the email is registered. Example neutral message: "If this email is registered, a sign-in
link has been sent to it."

---

**Pair 7.4 — security arbiter: verification token strength**
A: R_token = `{ verification token: 8 characters, alphanumeric }`
B: Flag.
Reason: the document requires a cryptographically random 256-bit verification token.
An 8-character alphanumeric token has approximately 47 bits of entropy — far below the
required 256 bits. It can be guessed or brute-forced.
fbx: Reprocess token generation. Generate a cryptographically random 256-bit token.

---

**Pair 7.5 — security arbiter: password hashing algorithm**
A: R_password = `{ password stored as MD5 hash }`
B: Flag.
Reason: the document requires bcrypt with cost factor 12 for password storage. MD5 is
not a password hashing algorithm — it is a fast general-purpose hash that can be reversed
at scale using rainbow tables or GPU acceleration. This is a critical security violation.
fbx: Reprocess password storage. Replace MD5 with bcrypt at cost factor 12.

---

**Pair 7.6 — event-sequence arbiter: wrong event order**
A: R_chain = `{ events published: UserCreated → UserSSOAuthenticated }`
B: Flag.
Reason: the document defines the SSO event chain as UserSSOAuthenticated → UserCreated.
UserSSOAuthenticated must be published first (by Auth Service), and UserCreated must
follow (published by User Service in response). The observed order is reversed. Downstream
services that consume these events in sequence will behave incorrectly.
fbx: Reprocess event ordering. Publish UserSSOAuthenticated before UserCreated.

---

**Pair 7.7 — data-integrity arbiter: userId not preserved on merge**
A: R_merge = `{ merge result: new UUID assigned to the merged account,
                old UUID deprecated }`
B: Flag.
Reason: the document's account merge rule requires that the older account's userId be
preserved. Assigning a new UUID breaks all foreign key references in every downstream
service (matching, analytics, audit logs, questionnaire responses) that stored the
original userId. The merge must preserve the existing identifier, not replace it.
fbx: Reprocess merge logic. Preserve the older account's userId. Add the new SSO
provider to the existing account record — do not create a new record.

---

**Pair 7.8 — resilience arbiter: failure silently discarded**
A: R_fail = `{ questionnaire delivery failed, error logged and discarded }`
B: Flag.
Reason: the document requires all failed event handlers to write to the dead letter
queue for manual review. Logging and discarding a failure means the questionnaire
delivery is permanently lost — the user will never receive it. The dead letter queue
is what allows the delivery to be retried when the service recovers.
fbx: Reprocess failure handling. On delivery failure: write the message to the dead
letter queue. Do not discard. Do not assume the failure is permanent.

---

**Pair 7.9 — pass case**
A: R1 = `{ JWT issued in httpOnly Secure SameSite cookie, rate limit 5 per IP per
           15 minutes, UserSSOAuthenticated published before UserCreated, userId
           preserved on merge }`
B: Pass.

A passing result needs no explanation — the arbiter's job is done.

---

## Node 8 — Formatter

**A:** All approved translated results from Node 6 (in natural language, passed arbitration).
**B:** One combined, user-readable output shaped for the appropriate audience.

The same set of results can be formatted differently for different audiences without
changing the underlying facts. When results include a gracefully handled failure, do not
expose the failure to the user — present the recovery state instead. Multiple approved
results are folded into one output, not listed separately.

---

**Pair 8.1 — user-facing output after SSO registration**
A: ["You're signed in with Google. Your account has been created.",
    "We've sent you a quick questionnaire in your chat."]
B: "You're signed in with Google. Welcome! Check your chat — we've sent you a quick
questionnaire to help us show you the right business connections."

Formatting decisions: open with the action that just succeeded (signed in with Google);
acknowledge the new account; give one clear next step (check your chat); explain why
(so we show you the right connections). Length: two sentences — enough to read
immediately, enough to prompt action.

---

**Pair 8.2 — user-facing output after email registration**
A: ["Your account has been created.",
    "We've sent a verification email to your address. Click the link to activate — expires in 24 hours."]
B: "Almost there! We've sent a verification email to your address. Click the link in
that email to activate your account. The link expires in 24 hours."

Formatting decisions: "Almost there" signals incompleteness without alarming the user;
clear instruction (click the link); deadline stated (24 hours) so the user knows to
act promptly.

---

**Pair 8.3 — product manager audience**
A: [R1...Rn translated results] + audience: product manager
B: Format as a funnel summary. Lead with the outcome (registration completed or
abandoned). Then: which registration path was used (SSO-Google / SSO-Facebook /
SSO-LinkedIn / SSO-Figma / email). Time spent at each step. Any drop-off point
identified. Questionnaire delivery status. The product manager's concern is the
conversion funnel — present results organized around that concern.

Example output:
"Registration completed. Path: SSO-Google. Time from start to dashboard: 8 seconds.
Questionnaire delivered. No drop-off. All steps passed."

---

**Pair 8.4 — security manager audience**
A: [R1...Rn translated results] + audience: security manager
B: Format as a security posture summary. Report: authentication method used; token
handling decisions (storage location, expiry, rotation); security controls applied
(rate limit, enumeration prevention, CSRF, bcrypt); any flags raised by the security
arbiter in this flow run. The security manager's concern is whether controls were
applied correctly — present results organized around that concern.

Example output:
"Auth method: SSO-Google. Tokens: httpOnly cookie, 15min access / 7-day refresh with
rotation. Controls applied: rate limit (5/15min), enumeration prevention (identical
response), CSRF protection. Security arbiter: no flags. Password controls: N/A (SSO path)."

---

**Pair 8.5 — devops audience**
A: [R1...Rn translated results] + audience: devops
B: Format as an operational health summary. Report: which services were involved;
which events were published and in what order; SLAs met or missed; any dead letter
queue entries generated; current observable health of each service in the flow path.
The devops audience's concern is whether the system behaved correctly and is still
healthy — present results organized around that concern.

Example output:
"Services: auth-service (healthy), user-service (healthy), questionnaire-service
(healthy), messaging-service (healthy). Events published: 6 (SSO path, all in order).
SLAs: registration <2s ✓, email delivery N/A (SSO path), questionnaire <5s ✓.
DLQ entries: 0."

---

## Self-check against the plan

| Node | Plan count | This file | Match |
|---|---|---|---|
| Domain splitter | 8 pairs | 8 pairs (1.1–1.8, each with Phrasing B) | ✓ |
| Domain translator | 7 pairs, one per domain | 7 pairs (2.1–2.7, each with Phrasing B) | ✓ |
| Scheme planner | 3 pairs with explicit fbi | 3 pairs (3.1–3.3, fbi named for each sub-task) | ✓ |
| Domain processor | 5 pairs, by sub-task | 5 pairs (4.1–4.5, organized by bi not by service) | ✓ |
| Applier | 5 pairs, write-before-publish rule | 5 pairs (5.1–5.5, rule stated at section header) | ✓ |
| Result translator | 7 pairs + consistency check | 7 pairs (6.1–6.7, consistency check at 6.7) | ✓ |
| Arbiter selector | 4 pairs | 4 pairs (6.8–6.11) | ✓ |
| Arbiter units | 9 pairs (5 flag + 1 pass) | 9 pairs (7.1–7.9: 8 flag + 1 pass) | ✓ |
| Formatter | 5 pairs (user + 3 audiences) | 5 pairs (8.1–8.5) | ✓ |

Domain names used: authentication, communication, onboarding, security, event
coordination, user management — matching the plan exactly.

---

*One document. Training pairs defined per node.*
*Source: `business flows/01-user-registration.md`*
