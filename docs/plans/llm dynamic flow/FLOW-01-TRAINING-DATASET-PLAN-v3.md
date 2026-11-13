# Training Dataset Plan — Flow 01: User Registration & SSO Onboarding
## Source: 01-user-registration.md only
## Framework: A → LLM node → B pairs per node

---

## The pipeline, restated precisely

Each node is a trained LLM that receives A and produces B.
The nodes form a chain. The B of one node becomes part of the A of the next.
Where nodes are arrays (n units), each unit receives one element of the array and produces one result.

The nodes in order:

1. **Domain splitter** — A: full user prompt. B: array of domain labels d1...dm, one per territory the prompt touches.
2. **Domain translator** — A: one prompt fragment + one domain label di. B: that fragment expressed in the formal language of domain di. One unit per domain → array ud1...udm.
3. **Scheme planner** — A: the translated domain descriptions ud1...udm. B: an ordered set of sub-tasks b1...bn plus a transformational prompt fb1...fbn for each.
4. **Domain processor** — A: one sub-task bi + its domain context. B: the processed result for that sub-task. One unit per bi → array of results.
5. **Applier** — A: one (bi, fbi) pair + the processed result. B: the applied result Rqi. One unit per bi → array R1...Rn.
6. **Result translator** — A: one result Rbi in formal domain language. B: that result in natural user language. One unit per result → array of translated results. Also sees all R1...Rn (for consistency checking).
7. **Arbiter selector** — A: all results R1...Rn + context. B: which arbiters to fire.
8. **Arbiter units** — A: one result Rbi + the arbiter's specialization. B: pass or flag, with reason. If flagged: fbx sent back to domain splitter for reprocessing.
9. **Formatter** — A: all approved translated results. B: one combined, user-readable output.

---

## What this document teaches each node

The document describes a registration flow. The training pairs must therefore cover:
what a registration-related prompt looks like as A, and what each node should produce as B.

---

## Node 1 — Domain splitter

**A:** A user prompt about registration.
**B:** The set of domain labels that prompt touches.

From this document, a registration prompt touches these domains. Each is a distinct B the splitter must learn to emit:

**Training pair 1.1**
A: "I need users to be able to sign up using Google, Facebook, LinkedIn, or Figma."
B: [authentication]

**Training pair 1.2**
A: "Users should also be able to register with email and password."
B: [authentication, communication]
— authentication because credentials are involved; communication because a verification email must be sent.

**Training pair 1.3**
A: "After signing up, the user should receive a questionnaire via chat."
B: [onboarding, communication]
— onboarding because it is the setup phase; communication because delivery via chat is required.

**Training pair 1.4**
A: "When the user finishes the questionnaire, the system should trigger personalization downstream."
B: [onboarding, event coordination]
— onboarding because completion is tracked; event coordination because a gate event triggers downstream flows.

**Training pair 1.5**
A: "The system should track which step of registration each user is on and send reminders if they abandon."
B: [user management, event coordination]

**Training pair 1.6**
A: "Registration must be rate-limited and protected against account enumeration."
B: [security]

**Training pair 1.7**
A: "I want to sign up quickly with Google so I can start networking."
B: [authentication]
— this is a complete user story mapping to a single domain.

**Training pair 1.8**
A: "As a new user I want a questionnaire via chat so the platform understands my business needs."
B: [onboarding, communication]

**What the splitter must learn from this document:**
- "SSO", "Google", "OAuth", "login", "sign up with provider" → authentication
- "email and password", "verify", "verification link" → authentication + communication
- "questionnaire", "onboarding", "profile setup" → onboarding
- "chat message", "email delivery", "notification" → communication
- "rate limit", "token expiry", "enumeration prevention" → security
- "event published", "downstream trigger", "gate event" → event coordination
- "user profile", "account state", "dormant" → user management
- One prompt can touch multiple domains → B is always an array, never a single label

---

## Node 2 — Domain translator (one unit per domain)

**A:** One prompt fragment + its domain label.
**B:** That fragment restated in the formal language of that domain.

**Training pair 2.1 — authentication domain**
A: "User signs up with Google" + domain: authentication
B: The user presents an OAuth2 authorization code obtained from Google's authorization endpoint. The system exchanges this code for an access token at Google's token endpoint. The system uses the access token to request the user's profile from Google's userinfo endpoint. The system checks whether the returned email already exists in the user store. If not: create a new user record with status active and issue platform JWT (access token TTL 15 minutes, refresh token TTL 7 days, stored in httpOnly Secure SameSite cookie).

**Training pair 2.2 — authentication domain**
A: "User registers with email and password" + domain: authentication
B: The user submits email and plaintext password. The system validates: email format correct, password meets strength rules (minimum 8 characters, one uppercase, one lowercase, one digit, one special character, not in top-10,000 common passwords, not similar to email). The system normalizes email to lowercase and checks uniqueness. The system creates a pending user record. The system generates a cryptographically random 256-bit single-use verification token with 24-hour expiry, invalidating any prior token for this email.

**Training pair 2.3 — communication domain**
A: "Send the user a verification email" + domain: communication
B: The system enqueues a message to the email delivery service via RabbitMQ (queue: email-queue). The message contains the verification link embedding the single-use token. Delivery SLA: within 60 seconds. On failure: message remains in queue for retry; does not enter dead letter queue until retry exhausted.

**Training pair 2.4 — onboarding domain**
A: "After account is active, send the user a questionnaire" + domain: onboarding
B: The Questionnaire Service consumes the UserCreated or UserActivated event and generates a personalized initial questionnaire. The Messaging Service consumes QuestionnaireRequired and delivers the questionnaire via in-app chat. Delivery SLA: within 5 seconds of account activation. Completion criteria: profile created (automatic), questionnaire completed (user action), business details submitted (part of questionnaire). On completion: publish UserOnboardingCompleted.

**Training pair 2.5 — security domain**
A: "Protect the registration endpoint from abuse" + domain: security
B: Rate limit: 5 registration attempts per IP per 15 minutes. On breach: return HTTP 429 with Retry-After header; log IP. OAuth redirect URIs: strict whitelist, no open redirects. CSRF protection on all auth endpoints. Account enumeration prevention: identical response body for "email exists" and "email not found". Verification token: cryptographically random, single-use, 24-hour expiry. Password storage: bcrypt cost factor 12 (email path only).

**Training pair 2.6 — event coordination domain**
A: "The system should publish events between steps" + domain: event coordination
B: SSO path event chain: UserSSOAuthenticated → UserCreated → QuestionnaireRequired → QuestionnaireSent → QuestionnaireCompleted → UserOnboardingCompleted. Email path event chain: UserRegistrationInitiated → VerificationEmailSent → EmailVerified → UserCreated → UserActivated → QuestionnaireRequired → QuestionnaireSent → QuestionnaireCompleted → UserOnboardingCompleted. All handlers must be idempotent (dedup key: userId + eventType). Failed handlers write to dead letter queue.

**Training pair 2.7 — user management domain**
A: "Keep track of where each user is in the registration process" + domain: user management
B: The User Service manages an onboarding state machine. States: pending (email path, pre-verification), active (SSO path or post-verification), onboarding (post-activation, pre-questionnaire), complete (post-questionnaire). On abandonment after 24 hours: send reminder. After 72 hours: send second reminder. After 30 days without completion: mark account dormant.

**What the translator must learn from this document:**
- Each domain has its own vocabulary. Authentication uses: OAuth2, JWT, access token, refresh token, bcrypt, verification token. Communication uses: queue, delivery SLA, retry. Onboarding uses: state machine, completion criteria, gate event. Security uses: rate limit, enumeration prevention, CSRF. Event coordination uses: event chain, idempotency, dead letter queue. User management uses: account state, dormancy.
- The translator must restate the human phrase in that domain's vocabulary without adding concepts not present in A.
- The output B is precise enough that a specialist can act on it without referring back to A.

---

## Node 3 — Scheme planner

**A:** All translated domain descriptions ud1...udm together.
**B:** An ordered set of sub-tasks b1...bn, each with a transformational prompt fb1...fbn.

From this document, the registration flow decomposes into these sub-tasks:

**Training pair 3.1 — SSO path decomposition**
A: [authentication description for SSO] + [event coordination description] + [onboarding description] + [user management description]
B:
- b1: Validate SSO provider callback and exchange authorization code for user profile. fb1: Apply OAuth2 token exchange pattern. Check email uniqueness. Branch: new user → create; existing email match → merge.
- b2: Create user record and publish UserSSOAuthenticated. fb2: Apply idempotency check before write. Publish event after successful write.
- b3: Generate and deliver initial questionnaire. fb3: Apply onboarding delivery pattern. Wait for UserCreated. Generate questionnaire. Deliver via chat within 5 seconds.
- b4: Wait for questionnaire completion. fb4: Apply wait-state pattern. On completion: verify all three criteria met. Publish UserOnboardingCompleted.

**Training pair 3.2 — Email path decomposition**
A: [authentication description for email] + [communication description] + [event coordination description] + [security description] + [onboarding description]
B:
- b1: Validate credentials and create pending account. fb1: Apply password strength rules. Normalize email. Check uniqueness. Generate verification token (256-bit, single-use, 24hr).
- b2: Deliver verification email. fb2: Enqueue to RabbitMQ email-queue. SLA 60 seconds.
- b3: Wait for email verification callback. fb3: Apply wait-state pattern with 24-hour maximum. On token click: validate exists + not expired + single-use. On expiry: allow resend.
- b4: Activate account and continue to onboarding. fb4: Same as SSO path b3 and b4 from here.

**Training pair 3.3 — Error and edge case decomposition**
A: [error handling descriptions] + [security descriptions]
B:
- b_retry: SSO provider timeout → retry up to 3 times with delays 1s, 2s, 4s. fb_retry: Apply exponential backoff pattern.
- b_merge: Duplicate email on SSO → merge existing account. fb_merge: Preserve older userId. Keep password if present. Add SSO provider link.
- b_rate: Rate limit exceeded → return 429 with Retry-After. fb_rate: Check counter before processing. Increment on attempt. Log IP.
- b_dlq: Event handler fails after retries → write to dead letter queue. fb_dlq: Preserve message for manual review. Do not discard.

**What the scheme planner must learn from this document:**
- A registration flow always has at least two distinct paths (SSO and email) that must be decomposed separately.
- Each path has a wait-state: SSO waits for questionnaire completion (human timescale); email path has an additional wait for verification (up to 24 hours).
- Error sub-tasks are decomposed alongside the happy path, not treated as afterthoughts.
- Each sub-task bi comes with a transformational prompt fbi that tells the applier how to approach the task.
- The decomposition is complete when every event in the event chain has a corresponding sub-task.

---

## Node 4 — Domain processor (n units, one per sub-task)

**A:** One sub-task bi + its domain context.
**B:** The processed result for that sub-task — what the system must produce.

**Training pair 4.1**
A: b1 (SSO): "Validate SSO provider callback and exchange code for user profile. Check email uniqueness. Branch on result."
B: If new user: {userId: new, status: active, provider: google, email: normalized, profile: {name, picture}}. If existing email match: {userId: existing, mergeRequired: true, existingMethod: email|sso}. If provider error: {error: provider_timeout, retryable: true}.

**Training pair 4.2**
A: b1 (email): "Validate credentials, normalize email, check uniqueness, generate verification token."
B: If valid and unique: {userId: new, status: pending, verificationToken: {value: random256bit, expiry: now+24h, singleUse: true}}. If email exists: {error: email_already_registered}. If password weak: {error: password_strength, violations: [list]}.

**Training pair 4.3**
A: b3 (email): "Wait for email verification callback. Token must exist, not expired, not used."
B: If valid: {userId, status: verified, verifiedAt: timestamp, ipAddress}. If expired: {error: token_expired, action: offer_resend}. If already used: {status: already_verified, action: redirect_to_login}.

**Training pair 4.4**
A: b4 (both paths): "Wait for questionnaire completion. Verify all three criteria. Publish gate event."
B: If all criteria met: {userId, onboardingSteps: {profileCreated: true, questionnaireCompleted: true, businessDetailsSubmitted: true}, completedAt: timestamp, event: UserOnboardingCompleted}. If abandoned after 30 days: {status: dormant}.

**Training pair 4.5**
A: b_merge: "Merge SSO account with existing email account."
B: {userId: existing (preserved), passwordKept: true|false, providers: [existing + new], foreignKeysIntact: true}.

**What the domain processor must learn from this document:**
- Every sub-task has a finite set of possible outputs — success states and failure states. The processor must know all of them.
- Wait-states have timeout outputs, not just success outputs.
- The output always includes enough information for the next node to act without re-reading the original input.
- Idempotency: if the same sub-task arrives twice (duplicate event), the output must be the same as the first execution.

---

## Node 5 — Applier (n units, one per bi)

**A:** One (bi, fbi) pair + the processed result from node 4.
**B:** The final applied result Rqi — the concrete action taken and its outcome.

**Training pair 5.1**
A: (b1_sso, fb1_sso="Apply OAuth2 token exchange pattern") + result {userId: new, status: active, provider: google}
B: R1 = Auth Service created user record in PostgreSQL. Issued JWT access token (15min) and refresh token (7 days) stored in httpOnly Secure SameSite cookie. Published UserSSOAuthenticated event to Kafka topic user-events. idempotency key: userId+UserSSOAuthenticated checked before publish.

**Training pair 5.2**
A: (b2_email, fb2="Enqueue to RabbitMQ email-queue. SLA 60 seconds.") + result {userId, verificationToken}
B: R2 = Email Service enqueued verification message to email-queue. Message contains single-use link with token. Delivery confirmed within SLA. Published VerificationEmailSent event. Audit Service consumed event and logged.

**Training pair 5.3**
A: (b3_both, fb3="Apply onboarding delivery pattern. Generate questionnaire. Deliver via chat within 5 seconds.") + result {userId, status: active|verified}
B: R3 = Questionnaire Service consumed UserCreated/UserActivated event. Generated personalized questionnaire. Published QuestionnaireRequired event. Messaging Service consumed event. Delivered questionnaire via WebSocket chat. Published QuestionnaireSent. Delivered within 5 seconds of activation.

**Training pair 5.4 — failure case**
A: (b3_both, fb3) + result {userId, status: active} but Questionnaire Service unavailable
B: R3_fail = QuestionnaireRequired event written to dead letter queue. User sees dashboard without questionnaire. Retry will deliver when service recovers. UserOnboardingCompleted not yet published (onboarding incomplete).

**Training pair 5.5**
A: (b_rate, fb_rate="Check counter before processing. Increment on attempt. Log IP.") + result {attemptsCount: 6, windowStart: 15min ago}
B: R_rate = Returned HTTP 429. Retry-After header set. IP logged to security monitoring. Registration attempt not processed.

**What the applier must learn from this document:**
- The applier executes the action described in fbi against the system described in the processed result.
- Every action has a corresponding event that must be published after the action succeeds.
- Write the record first, then publish the event (never the reverse — if the publish fails, the record allows replay).
- Failure outputs are valid results — they get sent to the result translator and arbiters just like success outputs.

---

## Node 6 — Result translator (n units) + Arbiter selector

**Result translator:**
**A:** One result Rqi in system/formal language. Also sees all R1...Rn for consistency.
**B:** That result expressed in natural language the user can read.

**Training pair 6.1**
A: R1 = {userId: new, status: active, provider: google, JWT issued, UserSSOAuthenticated published}
B: "You're signed in with Google. Your account has been created."

**Training pair 6.2**
A: R2 = {verificationToken sent, VerificationEmailSent published, SLA met}
B: "We've sent a verification email to your address. Click the link in the email to activate your account — the link expires in 24 hours."

**Training pair 6.3**
A: R3 = {questionnaire delivered via chat, QuestionnaireSent published}
B: "We've sent you a quick questionnaire in your chat. Your answers help us show you the right business connections."

**Training pair 6.4**
A: R3_fail = {QuestionnaireRequired in dead letter queue, delivery failed}
B: "Your account is ready. We'll send you a getting-started questionnaire shortly." (Do not expose the failure to the user.)

**Training pair 6.5**
A: R_token_expired = {error: token_expired, action: offer_resend}
B: "This verification link has expired. Links are only valid for 24 hours. Click below to get a new one."

**Training pair 6.6**
A: R_rate = {HTTP 429, IP logged}
B: "Too many attempts. Please wait 15 minutes before trying again."

**Training pair 6.7 — consistency check across all R**
A: R1 (SSO path completed) + R3 (questionnaire delivered) — but R1 says provider=google and R3 says registrationMethod=email
B: Flag inconsistency. These two results describe different paths and cannot belong to the same flow run. Send back for reprocessing.

**Arbiter selector:**
**A:** All results R1...Rn + context of the flow run.
**B:** Which arbiter units to fire.

**Training pair 6.8**
A: R1...Rn for a registration that included an SSO token exchange
B: Fire: [security-arbiter, event-sequence-arbiter]
— security because tokens and credentials were handled; event-sequence because the published event chain must be in correct order.

**Training pair 6.9**
A: R1...Rn for a flow run that touched personal data (email, name, profile picture)
B: Fire: [security-arbiter, privacy-arbiter]

**Training pair 6.10**
A: R1...Rn for a flow run that included an account merge
B: Fire: [data-integrity-arbiter]
— to verify the older userId was preserved and foreign keys remain valid.

**Training pair 6.11**
A: R1...Rn for a flow run where questionnaire delivery failed
B: Fire: [resilience-arbiter]
— to verify the failure was correctly routed to the dead letter queue, not silently dropped.

---

## Node 7 — Arbiter units

**A:** One result Rbi + the arbiter's specialization.
**B:** Pass, or flag with reason + the fragment fbx to send back for reprocessing.

**Training pair 7.1 — security arbiter**
A: R1 = {JWT issued, stored in localStorage}
B: Flag. Reason: tokens must be stored in httpOnly Secure SameSite cookie, never localStorage. fbx: reprocess token storage step.

**Training pair 7.2 — security arbiter**
A: R1 = {rate limit: 10 attempts per 15 minutes}
B: Flag. Reason: rate limit must be 5 attempts per IP per 15 minutes, not 10. fbx: reprocess rate limit configuration.

**Training pair 7.3 — security arbiter**
A: R_email = {response body for "email not found": "This email is not registered"}
B: Flag. Reason: account enumeration — response must be identical for "email exists" and "email not found". fbx: reprocess error response wording.

**Training pair 7.4 — security arbiter**
A: R_token = {verification token: 8 characters, alphanumeric}
B: Flag. Reason: verification token must be cryptographically random 256-bit. An 8-character alphanumeric token is not sufficient. fbx: reprocess token generation.

**Training pair 7.5 — security arbiter**
A: R_password = {password stored as MD5 hash}
B: Flag. Reason: password must be hashed with bcrypt at cost factor 12, not MD5. fbx: reprocess password storage.

**Training pair 7.6 — event-sequence arbiter**
A: R_chain = {events published: UserCreated → UserSSOAuthenticated}
B: Flag. Reason: UserSSOAuthenticated must be published before UserCreated, not after. fbx: reprocess event ordering.

**Training pair 7.7 — data-integrity arbiter**
A: R_merge = {merge result: new userId assigned, old userId deprecated}
B: Flag. Reason: merge must preserve the older account's userId. Assigning a new userId breaks foreign key references. fbx: reprocess merge logic.

**Training pair 7.8 — resilience arbiter**
A: R_fail = {questionnaire delivery failed, error discarded}
B: Flag. Reason: failed event handlers must write to dead letter queue, not discard. fbx: reprocess failure handling.

**Training pair 7.9 — pass case**
A: R1 = {JWT issued in httpOnly cookie, rate limit 5/15min, UserSSOAuthenticated published first, userId preserved}
B: Pass.

**What the arbiter units must learn from this document:**
- Security arbiter checks: token storage location, rate limit values, enumeration prevention, token strength, password hashing algorithm.
- Event-sequence arbiter checks: published events appear in the correct order per the event chain definition.
- Data-integrity arbiter checks: userId preservation on merge, idempotency on duplicate events.
- Resilience arbiter checks: failures go to dead letter queue, not silently dropped; retry logic uses exponential backoff with correct delays (1s, 2s, 4s).
- A passing result needs no explanation — just "pass."
- A flagged result must include: what is wrong, which rule it violates, and what fragment to send back for reprocessing.

---

## Node 8 — Formatter

**A:** All approved translated results from node 6 (natural language, passed arbitration).
**B:** One combined, user-readable output shaped for the appropriate audience.

**Training pair 8.1 — user-facing output after SSO registration**
A: ["You're signed in with Google. Your account has been created.", "We've sent you a quick questionnaire in your chat."]
B: "You're signed in with Google. Welcome! Check your chat — we've sent you a quick questionnaire to help us show you the right business connections."

**Training pair 8.2 — user-facing output after email registration**
A: ["Your account has been created.", "We've sent a verification email to your address. Click the link to activate — expires in 24 hours."]
B: "Almost there! We've sent a verification email to your address. Click the link in that email to activate your account. The link expires in 24 hours."

**Training pair 8.3 — product manager audience**
A: [R1...Rn translated results] + audience: product manager
B: Present as funnel metrics: registration path used (SSO/email), steps completed, time at each step, drop-off point if any. Format as a brief structured summary with the key metric at the top.

**Training pair 8.4 — security audience**
A: [R1...Rn translated results] + audience: security manager
B: Present as: authentication method used, token handling decisions made, security controls applied (rate limit, enumeration prevention, bcrypt), any flags raised by security arbiter.

**Training pair 8.5 — operations audience**
A: [R1...Rn translated results] + audience: devops
B: Present as: services involved, events published, SLAs met or missed, any dead letter queue entries, current health of each service in the path.

**What the formatter must learn from this document:**
- The same set of results can be formatted differently for different audiences without changing the underlying facts.
- User-facing output is conversational and brief — one or two sentences.
- Technical audiences get structured summaries organized by their specific concerns.
- When results include a failure that was gracefully handled, do not expose the failure to the user — present the recovery state instead.
- Multiple approved results are folded into one coherent output, not listed separately.

---

## Summary — what each node gets as training data from this document

| Node | What it learns from this document |
|---|---|
| Domain splitter | 8 A→B pairs mapping registration prompt fragments to domain label arrays |
| Domain translator | 7 A→B pairs, one per domain, translating human descriptions into that domain's formal vocabulary |
| Scheme planner | 3 A→B pairs decomposing registration into ordered sub-tasks with transformational prompts |
| Domain processor | 5 A→B pairs covering success states, failure states, and wait-state outcomes per sub-task |
| Applier | 5 A→B pairs showing concrete actions taken, events published, and failure routing |
| Result translator | 7 A→B pairs converting formal results to natural language; 4 A→B pairs for arbiter selector |
| Arbiter units | 9 A→B pairs: 5 flag cases with reasons and reprocess instructions; 1 pass case |
| Formatter | 5 A→B pairs folding results for different audiences into single coherent outputs |

---

*One document. Training pairs defined per node. Stopping here.*
