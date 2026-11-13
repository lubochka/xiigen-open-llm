# Training Data — Domain Splitter
## Source document: 01-user-registration.md
## Node: Domain Splitter
## What this node does: reads a user prompt and identifies which functional territories it touches, so each territory can be handled by its own specialist unit

---

**Example 1**

Input: I want to sign up for the platform using my Google account.

Output:
- Who handles authenticating users through external providers like Google: this prompt touches that territory.
- Who creates the user's profile once authentication succeeds: this prompt touches that territory.
- Who tracks where the user is in the registration process: this prompt touches that territory.
- Who delivers the initial questionnaire to the new user via chat: this prompt touches that territory.

---

**Example 2**

Input: I need users to be able to register with their email address and a password they choose themselves.

Output:
- Who handles taking an email and password from a new user, checking that the password is strong enough, and storing the credentials safely: this prompt touches that territory.
- Who sends the user a verification email so they can confirm their address before their account is activated: this prompt touches that territory.
- Who creates the user's profile once the email is confirmed: this prompt touches that territory.
- Who tracks where the user is in the registration process: this prompt touches that territory.
- Who delivers the initial questionnaire once the account is active: this prompt touches that territory.

---

**Example 3**

Input: After a user finishes signing up, they should receive a questionnaire through the chat so the platform can learn about their business needs.

Output:
- Who generates the questionnaire tailored to the new user: this prompt touches that territory.
- Who delivers that questionnaire to the user through an in-app chat message: this prompt touches that territory.
- Who updates the user's onboarding status once the questionnaire is finished: this prompt touches that territory.

---

**Example 4**

Input: When someone tries to sign up but they already have an account registered with their email, the system should merge the two accounts rather than creating a duplicate.

Output:
- Who detects that an incoming SSO authentication uses an email address already registered through another method: this prompt touches that territory.
- Who carries out the merge so the user keeps their original account identifier and gains access through both the old and new login methods: this prompt touches that territory.

---

**Example 5**

Input: The registration endpoint needs to be protected against someone trying to create thousands of accounts automatically.

Output:
- Who enforces a limit on how many registration attempts are allowed from the same address in a short window: this prompt touches that territory.
- Who returns the correct error response that tells the caller to wait before trying again: this prompt touches that territory.
- Who logs the incident for security review: this prompt touches that territory.

---

**Example 6**

Input: I want to know how many users complete registration compared to how many start it, broken down by which sign-in provider they used.

Output:
- Who records registration funnel events so that completion and drop-off rates can be measured: this prompt touches that territory.
- Who tracks which provider — Google, Facebook, LinkedIn, Figma, or email — each registration used: this prompt touches that territory.

---

**Example 7**

Input: If the questionnaire service is down when a user finishes registering, the questionnaire should still reach them once the service comes back up.

Output:
- Who handles the situation where a step in the registration sequence fails after the earlier steps have already succeeded: this prompt touches that territory.
- Who holds the pending questionnaire delivery so it is not lost and will be retried automatically when the service recovers: this prompt touches that territory.
- Who decides what the user sees in the meantime — that their account is ready but the questionnaire has not arrived yet: this prompt touches that territory.

---

**Example 8**

Input: A user clicks the verification link in their email but the link is already 25 hours old.

Output:
- Who checks whether a verification token is still within its allowed time window: this prompt touches that territory.
- Who tells the user the link has expired and offers to send them a fresh one: this prompt touches that territory.
- Who generates a new token and invalidates the old one so only the fresh link will work: this prompt touches that territory.

---

**Example 9**

Input: User data collected at registration — email address, name, profile picture from Google — must be handled under data protection requirements with consent captured and a way to delete the data on request.

Output:
- Who captures consent at the moment of registration: this prompt touches that territory.
- Who classifies the collected data by sensitivity level so it is stored and retained correctly: this prompt touches that territory.
- Who maintains the audit trail of every action taken with the user's data: this prompt touches that territory.
- Who supports the process of removing a user's data if they exercise their right to deletion: this prompt touches that territory.

---

**Example 10**

Input: The auth service needs to scale automatically when registration traffic is high because validating passwords and issuing tokens is computationally expensive.

Output:
- Who monitors resource usage on the authentication service and triggers additional instances when a threshold is crossed: this prompt touches that territory.
- Who ensures the service can be scaled horizontally without losing session state: this prompt touches that territory.

---

**Example 11**

Input: I want to sign up but Google is temporarily unavailable.

Output:
- Who detects that an external authentication provider is not responding: this prompt touches that territory.
- Who decides what the user is shown — that this provider is temporarily unavailable — while keeping the other providers available: this prompt touches that territory.
- Who retries the connection to the provider before giving up, and how many times: this prompt touches that territory.

---

**Example 12**

Input: If a user starts registering with email but never clicks the verification link, remind them after one day and again after three days.

Output:
- Who tracks that a registration was started but the verification step was never completed: this prompt touches that territory.
- Who triggers a reminder message to the user at the right times after the account was created: this prompt touches that territory.
- Who decides when to stop reminding and mark the account as dormant if there is still no activity after thirty days: this prompt touches that territory.

---

**Example 13**

Input: The user double-clicked the approve button on the Google consent screen so the platform received the same authorization code twice.

Output:
- Who detects that the same external authorization event has arrived more than once: this prompt touches that territory.
- Who ensures that processing it a second time does not create a second user account or a second set of events: this prompt touches that territory.

---

**Example 14**

Input: A user registers on their phone through the mobile app rather than a web browser.

Output:
- Who manages the OAuth flow when the redirect back from the provider goes to a mobile app rather than a web page: this prompt touches that territory.
- Who handles the deep link that returns the user to the app after they approve permissions on the provider's screen: this prompt touches that territory.
- Who ensures the rest of the registration sequence — profile creation, questionnaire, onboarding — works the same way regardless of whether the user is on mobile or web: this prompt touches that territory.

---

**Example 15**

Input: The platform serves multiple business communities. A user joining one specific community should have their community context stored from the moment they register, and the questionnaire they receive might be different from what users in other communities see.

Output:
- Who captures which community the user is joining at the point of registration: this prompt touches that territory.
- Who ensures the questionnaire generated for this user is the version appropriate for their community rather than a generic default: this prompt touches that territory.
- Who stores the community association with the user's profile from the start: this prompt touches that territory.

---

**Example 16**

Input: All ten events that happen during a user's registration and onboarding must be recorded with enough detail to satisfy a compliance audit.

Output:
- Who publishes each event to the event bus as it occurs: this prompt touches that territory.
- Who consumes those events and writes them to the audit log with the required details — IP address, user agent, timestamp: this prompt touches that territory.
- Who retains failed audit attempts and routes them for manual review rather than silently discarding them: this prompt touches that territory.

---

**Example 17**

Input: Two people tried to register with the same email address at almost exactly the same time.

Output:
- Who enforces that an email address can only be associated with one account: this prompt touches that territory.
- Who ensures that when two simultaneous requests for the same email arrive, exactly one succeeds and the other receives a clear error rather than creating a duplicate: this prompt touches that territory.

---

**Example 18**

Input: I want to connect my Facebook account to my existing profile that I originally created with my LinkedIn account.

Output:
- Who manages the list of external providers linked to an existing account: this prompt touches that territory.
- Who adds a new provider link without replacing the existing one or creating a second account: this prompt touches that territory.

---

**Example 19**

Input: As a small business owner I want to sign up quickly so I can start networking with other business owners without creating another username and password.

Output:
- Who handles letting users authenticate through an external provider so they do not need to create and remember a new password: this prompt touches that territory.
- Who creates their platform account from the data returned by the provider: this prompt touches that territory.
- Who moves them through onboarding so they reach the networking features as quickly as possible: this prompt touches that territory.

---

**Example 20**

Input: Tokens given to users after login — the short-lived access token and the longer refresh token — must be stored in a way that prevents JavaScript from reading them.

Output:
- Who decides where tokens are stored after they are issued — specifically that they go into a cookie marked httpOnly, Secure, and SameSite rather than into localStorage or sessionStorage: this prompt touches that territory.
- Who rotates the refresh token so that the old one becomes invalid each time a new one is issued: this prompt touches that territory.

---

**Example 21**

Input: I need a health check endpoint on every service so our monitoring system knows which ones are running.

Output:
- Who exposes a health check endpoint on the authentication service that also validates JWT handling is working: this prompt touches that territory.
- Who exposes a health check on the user service that confirms the database connection is live: this prompt touches that territory.
- Who exposes a health check on the email service that confirms the outbound mail connection is reachable: this prompt touches that territory.
- Who exposes a health check on the questionnaire service that confirms the document store is reachable: this prompt touches that territory.
- Who exposes a health check on the messaging service that confirms the session cache is reachable: this prompt touches that territory.

---

**Example 22**

Input: Alert the on-call team if the percentage of successful registrations drops below 95 percent.

Output:
- Who tracks the ratio of registrations that complete successfully versus those that fail or are abandoned: this prompt touches that territory.
- Who sends an alert when that ratio falls below the threshold: this prompt touches that territory.

---

**Example 23**

Input: Alert the team if fewer than 70 percent of users verify their email within 24 hours of registering.

Output:
- Who measures the time between when a verification email is sent and when the user clicks the link: this prompt touches that territory.
- Who calculates the verification completion rate across a rolling 24-hour window: this prompt touches that territory.
- Who fires an alert when that rate falls below 70 percent: this prompt touches that territory.

---

**Example 24**

Input: Alert us if the response time from any SSO provider exceeds three seconds.

Output:
- Who measures how long each call to an external authentication provider takes to respond: this prompt touches that territory.
- Who raises an alert when that duration exceeds the acceptable threshold: this prompt touches that territory.

---

**Example 25**

Input: Alert the team if more than ten messages are sitting unprocessed in the dead letter queue.

Output:
- Who monitors the depth of the dead letter queue where failed event handlers write their failures: this prompt touches that territory.
- Who raises an alert when that depth exceeds ten messages: this prompt touches that territory.

---

**Example 26**

Input: Show a live dashboard of how registration is going right now — broken down by provider, by region, and over time.

Output:
- Who collects the raw registration event data that feeds a dashboard: this prompt touches that territory.
- Who organizes that data by provider, by region, and by time window so it can be displayed: this prompt touches that territory.

---

**Example 27**

Input: If the auth service goes down completely, what happens to users trying to register?

Output:
- Who is the single point through which all registration attempts pass, meaning its failure blocks all new registrations: this prompt touches that territory.
- Who describes what the failure blast radius of the auth service is: this prompt touches that territory.

---

**Example 28**

Input: If the user service goes down, do we lose the registration attempts that were already authenticated?

Output:
- Who holds authenticated registration events in the event queue so that profile creation is not lost even when the user service is temporarily unavailable: this prompt touches that territory.
- Who automatically replays those queued events when the user service comes back: this prompt touches that territory.

---

**Example 29**

Input: If the email service goes down, do users lose their verification emails?

Output:
- Who holds outgoing verification emails in a durable queue so they survive an email service restart: this prompt touches that territory.
- Who ensures the emails are delivered once the service recovers rather than being discarded: this prompt touches that territory.

---

**Example 30**

Input: The registration page endpoint must respond in under two seconds at the 99th percentile.

Output:
- Who is responsible for the response time of the registration endpoint and must be designed to meet the two-second target even under load: this prompt touches that territory.

---

**Example 31**

Input: A verification email must reach the user within 60 seconds of them submitting the registration form.

Output:
- Who enqueues the verification email immediately after the registration is submitted: this prompt touches that territory.
- Who processes the queue and dispatches the email within the required time window: this prompt touches that territory.

---

**Example 32**

Input: The questionnaire must appear in the user's chat within 5 seconds of their account becoming active.

Output:
- Who listens for the account activation event and triggers questionnaire generation without delay: this prompt touches that territory.
- Who delivers the questionnaire to the user's chat session within the five-second window: this prompt touches that territory.

---

**Example 33**

Input: Passwords must be stored using bcrypt and must never be stored in plain text or with a weak hash.

Output:
- Who applies the password hashing algorithm before storing credentials: this prompt touches that territory.
- Who enforces that the cost factor is set to 12 so the hashing is strong enough: this prompt touches that territory.

---

**Example 34**

Input: The system must give the same response whether an email address is already registered or not, so an attacker cannot use the registration form to find out which emails are in the system.

Output:
- Who controls the wording and structure of the response returned when a registration attempt uses an email that already exists: this prompt touches that territory.
- Who ensures that response is identical to the one returned when the email is new, so no information is leaked: this prompt touches that territory.

---

**Example 35**

Input: OAuth redirect URIs must be validated strictly — no open redirects allowed.

Output:
- Who maintains a whitelist of permitted redirect URIs for each OAuth provider: this prompt touches that territory.
- Who validates that the redirect URI in each incoming OAuth callback matches the whitelist before processing it: this prompt touches that territory.

---

**Example 36**

Input: All authentication endpoints need CSRF protection.

Output:
- Who applies cross-site request forgery protection to the endpoints that handle login and registration: this prompt touches that territory.

---

**Example 37**

Input: Verification tokens must be cryptographically random, single-use, and expire after 24 hours.

Output:
- Who generates the verification token using a cryptographically secure random source: this prompt touches that territory.
- Who stores the token with its expiry timestamp: this prompt touches that territory.
- Who marks the token as used immediately when it is redeemed so it cannot be used again: this prompt touches that territory.
- Who rejects tokens that are past their expiry: this prompt touches that territory.

---

**Example 38**

Input: Failed authentication attempts must be logged for 90 days for security review purposes.

Output:
- Who records each failed registration or login attempt with its timestamp and source: this prompt touches that territory.
- Who retains those records for the required 90-day period before they can be discarded: this prompt touches that territory.

---

**Example 39**

Input: Users have the right to have all their data deleted from the platform.

Output:
- Who identifies every place where a user's data is stored when a deletion request arrives: this prompt touches that territory.
- Who removes or anonymizes that data across the authentication store, the profile store, the questionnaire responses, the audit logs, and any other store that holds it: this prompt touches that territory.

---

**Example 40**

Input: Consent must be captured from every user at the point of registration.

Output:
- Who presents the consent form or consent acknowledgement to the user during the registration process: this prompt touches that territory.
- Who records that consent was given, at what time, and under what version of the privacy terms: this prompt touches that territory.

---

**Example 41**

Input: The auth service stores credentials in PostgreSQL and uses Redis for tokens. The user service stores profiles in MongoDB. The questionnaire service also uses MongoDB. The messaging service uses Redis for its session state.

Output:
- Who manages the connection between the authentication service and its PostgreSQL credential store: this prompt touches that territory.
- Who manages the connection between the authentication service and its Redis token store: this prompt touches that territory.
- Who manages the connection between the user service and its MongoDB profile store: this prompt touches that territory.
- Who manages the connection between the questionnaire service and its MongoDB response store: this prompt touches that territory.
- Who manages the connection between the messaging service and its Redis session cache: this prompt touches that territory.

---

**Example 42**

Input: All events between services travel over Kafka. Emails specifically go through RabbitMQ.

Output:
- Who publishes domain events to the Kafka topic so downstream services receive them: this prompt touches that territory.
- Who consumes domain events from Kafka and processes them in each receiving service: this prompt touches that territory.
- Who routes verification email requests specifically through RabbitMQ for reliable delivery: this prompt touches that territory.

---

**Example 43**

Input: I tried to test a skip-questionnaire option where users can skip the questionnaire and get lower-quality matches instead of completing it.

Output:
- Who manages the rule that normally requires questionnaire completion before the user is fully onboarded: this prompt touches that territory.
- Who handles the alternative path where the user proceeds without completing it: this prompt touches that territory.
- Who tracks that the user chose to skip and adjusts downstream matching quality accordingly: this prompt touches that territory.

---

**Example 44**

Input: We want to experiment with showing users five questions during registration instead of ten, or showing them progressively as they use the platform.

Output:
- Who controls how many questionnaire questions are shown to a user and in what order: this prompt touches that territory.
- Who tracks how far through the questionnaire each user has progressed: this prompt touches that territory.
- Who determines when the questionnaire is considered complete enough to trigger the onboarding completion event: this prompt touches that territory.

---

**Example 45**

Input: We want to try delivering the questionnaire as a modal popup instead of through the chat.

Output:
- Who decides how the questionnaire is delivered to the user — through chat, through a popup, or through an email: this prompt touches that territory.
- Who handles the different delivery methods and ensures the user's answers reach the questionnaire service regardless of which method was used: this prompt touches that territory.

---

**Example 46**

Input: The Google token exchange is taking too long and timing out for some users during registration.

Output:
- Who calls the external provider's token endpoint during registration and waits for the response: this prompt touches that territory.
- Who applies the retry policy — three attempts with one-second, two-second, and four-second waits — when the provider does not respond in time: this prompt touches that territory.
- Who reports the timeout to the monitoring system so the on-call team can see it: this prompt touches that territory.

---

**Example 47**

Input: Google returned the user's email but did not include their name or profile picture in the response.

Output:
- Who processes the profile data returned by the external provider and handles the case where some fields are missing: this prompt touches that territory.
- Who creates the user account using only the available fields and leaves the missing ones blank rather than failing the registration: this prompt touches that territory.
- Who prompts the user to fill in the missing profile details manually after their account is created: this prompt touches that territory.

---

**Example 48**

Input: The user's OAuth session timed out because they spent too long on the consent screen before approving.

Output:
- Who detects that the authorization code returned by the provider has already expired by the time it arrives: this prompt touches that territory.
- Who shows the user a message that their session expired and asks them to start the sign-up process again: this prompt touches that territory.

---

**Example 49**

Input: A user clicked the verification link a second time after already verifying their email.

Output:
- Who marks a verification token as consumed the first time it is used: this prompt touches that territory.
- Who handles the second click gracefully — recognizing the account is already verified and redirecting the user to the login page rather than showing an error: this prompt touches that territory.

---

**Example 50**

Input: The platform's matching features, the feed, and learning programs all need to know when a user has finished onboarding so they can start working with that user's data.

Output:
- Who publishes the signal that onboarding is complete so that downstream systems know they can begin: this prompt touches that territory.
- Who receives that signal in the matching system and begins adapting content for the new user: this prompt touches that territory.
- Who receives it in the feed system and begins populating the user's feed: this prompt touches that territory.
- Who receives it in the learning programs system and begins assigning relevant programs: this prompt touches that territory.

---

**Example 51**

Input: The user lands on the platform's home page, sees anonymized statistics and upcoming events, then clicks Sign up with Google.

Output:
- Who handles what happens the moment the user clicks the sign-up button and before they are redirected to Google: this prompt touches that territory.
- Who manages the redirect to the Google consent screen: this prompt touches that territory.

---

**Example 52**

Input: Once the user approves permissions on Google and is sent back to the platform, what happens before they see their dashboard?

Output:
- Who receives the authorization code from Google's redirect and begins processing it: this prompt touches that territory.
- Who exchanges the code for a profile and creates the account: this prompt touches that territory.
- Who triggers the questionnaire sequence: this prompt touches that territory.
- Who loads the personalized dashboard once onboarding reaches a sufficient state: this prompt touches that territory.

---

**Example 53**

Input: The user sees the questionnaire arrive as a chat message and fills it in through the chat interface.

Output:
- Who delivered the questionnaire into the chat interface: this prompt touches that territory.
- Who receives the user's answers as they are submitted through the chat: this prompt touches that territory.
- Who validates and stores those answers: this prompt touches that territory.
- Who marks the questionnaire as completed and publishes the completion event: this prompt touches that territory.

---

**Example 54**

Input: The platform is built on microservices and each service must inherit from MicroserviceBase and use the platform's standard database and queue interfaces.

Output:
- Who ensures the authentication service conforms to the platform's standard service base: this prompt touches that territory.
- Who ensures the user service conforms to it: this prompt touches that territory.
- Who ensures the email service conforms to it: this prompt touches that territory.
- Who ensures the questionnaire service conforms to it: this prompt touches that territory.
- Who ensures the messaging service conforms to it: this prompt touches that territory.

---

**Example 55**

Input: The API gateway needs to extract the user's identity and role from the JWT and pass them into the processing context so downstream services do not have to validate the token themselves.

Output:
- Who sits at the gateway layer and reads the JWT from the incoming request: this prompt touches that territory.
- Who extracts the user identifier and role claims from the token: this prompt touches that territory.
- Who passes those claims into the request context so every downstream service can read them without repeating the validation: this prompt touches that territory.

---

**Example 56**

Input: The questionnaire service is a complex task type in the platform's task catalog because it must satisfy strict output contracts before the flow can continue.

Output:
- Who defines what a valid completed questionnaire looks like as an output contract: this prompt touches that territory.
- Who checks that the questionnaire service's output meets that contract before allowing the flow to proceed to the next step: this prompt touches that territory.

---

**Example 57**

Input: Every registration event needs to feed into the multimodel orchestrator so it can begin adapting the user's experience based on what it learned from the registration.

Output:
- Who routes registration completion events to the orchestrator that drives personalization: this prompt touches that territory.
- Who in the orchestrator decides which matching algorithms or feed adaptations to start based on what the registration events contain: this prompt touches that territory.

---

**Example 58**

Input: Users should be able to register using Facebook, LinkedIn, or Figma, not just Google.

Output:
- Who manages support for multiple external authentication providers rather than just one: this prompt touches that territory.
- Who adds a new provider to the list of supported sign-in options: this prompt touches that territory.
- Who ensures the same account creation and onboarding sequence runs regardless of which provider the user chose: this prompt touches that territory.

---

**Example 59**

Input: I need to see how long each step of the registration funnel takes on average — from clicking sign up to completing the questionnaire.

Output:
- Who records a timestamp at each step of the registration and onboarding sequence: this prompt touches that territory.
- Who aggregates those timestamps so that time-per-step statistics can be calculated: this prompt touches that territory.
- Who surfaces those statistics in a form that can be used to identify where users are spending the most time: this prompt touches that territory.

---

**Example 60**

Input: The registration flow has to be the very first thing built because every other feature — matching, events, social features — depends on having registered and onboarded users.

Output:
- Who establishes and enforces the dependency ordering so that the registration flow is complete and working before any flow that relies on registered users can be activated: this prompt touches that territory.

---

**Example 61**

Input: I want to run an A/B test on the questionnaire — half the users get five questions, half get ten.

Output:
- Who assigns each new user to a test group: this prompt touches that territory.
- Who delivers the questionnaire variant that corresponds to the user's assigned group: this prompt touches that territory.
- Who records which variant each user received so the results can be compared: this prompt touches that territory.

---

**Example 62**

Input: When a user service instance restarts after a crash, it should automatically pick up and process any registration events that arrived while it was down.

Output:
- Who holds registration events durably so they are not lost during a service outage: this prompt touches that territory.
- Who ensures that when the user service restarts it reads and processes all events it missed while it was unavailable: this prompt touches that territory.

---

**Example 63**

Input: The messaging service handles WebSocket connections. When it scales up to handle more connections, how does it avoid losing the session state for existing connected users?

Output:
- Who manages the shared session state store so that any instance of the messaging service can read the session for any connected user regardless of which instance they first connected to: this prompt touches that territory.

---

**Example 64**

Input: Analytics need to track the SSO versus email registration split over time.

Output:
- Who records the registration method — SSO provider name or email — as part of each registration event: this prompt touches that territory.
- Who aggregates those records so the split between methods can be reported over any time period: this prompt touches that territory.

---

**Example 65**

Input: Kafka carries the events between services. What topic do the user registration events go on?

Output:
- Who publishes user registration events to the event bus: this prompt touches that territory.
- Who is responsible for the name of the topic those events travel on and for ensuring all publishing and consuming services agree on that name: this prompt touches that territory.

---

**Example 66**

Input: The user entered an email in mixed case — John@Company.com. Will they be able to log in later with john@company.com?

Output:
- Who normalizes email addresses to a consistent case before storing them or checking for duplicates: this prompt touches that territory.
- Who ensures that future login attempts with any capitalization variation match the stored normalized form: this prompt touches that territory.

---

**Example 67**

Input: If an event handler processes the same event twice — maybe due to a network retry — the system should not create two user profiles.

Output:
- Who checks whether the action triggered by an event has already been performed before performing it again: this prompt touches that territory.
- Who uses the combination of the user identifier and the event type as the key for detecting that a duplicate has arrived: this prompt touches that territory.

---

**Example 68**

Input: Verification tokens that are more than 24 hours old must be automatically cleaned up and not left in the database indefinitely.

Output:
- Who removes expired verification tokens from storage after their validity window has passed: this prompt touches that territory.

---

**Example 69**

Input: An SSO token from Google must never be kept in the platform's database — it should be exchanged for a platform token immediately and the original discarded.

Output:
- Who performs the exchange from the provider's token to the platform's own JWT: this prompt touches that territory.
- Who ensures the provider's token is not written to any persistent store during or after that exchange: this prompt touches that territory.

---

**Example 70**

Input: The connection pool for the auth service's PostgreSQL database should be sized between 20 and 50 connections.

Output:
- Who configures and manages the database connection pool for the authentication service: this prompt touches that territory.
- Who monitors pool usage and adjusts it when the service scales up or down: this prompt touches that territory.

---

**Example 71**

Input: MongoDB should be deployed as a replica set rather than a single node so that profile data is not at risk if one database node goes down.

Output:
- Who is responsible for the deployment topology of the MongoDB instance used for user profiles and questionnaire responses: this prompt touches that territory.
- Who ensures the services that read and write to MongoDB are configured to use the replica set connection string: this prompt touches that territory.

---

**Example 72**

Input: Redis for session caching and rate limiting needs high availability through Sentinel so a single Redis node failure does not take down authentication.

Output:
- Who configures Redis Sentinel for the session and rate-limiting cache used by the authentication service: this prompt touches that territory.
- Who ensures the auth service connects through Sentinel so it automatically fails over to a healthy Redis node: this prompt touches that territory.

---

**Example 73**

Input: How does the platform know whether to show the user the SSO sign-in screen or the email registration form?

Output:
- Who presents the initial registration choice to the user and routes them to the correct path based on what they select: this prompt touches that territory.

---

**Example 74**

Input: The user finishes the questionnaire. What three things have to be true before the system considers onboarding complete?

Output:
- Who tracks whether the user's profile has been created: this prompt touches that territory.
- Who tracks whether the user has completed the questionnaire: this prompt touches that territory.
- Who tracks whether the user has submitted their business details through the questionnaire: this prompt touches that territory.
- Who checks that all three are true before publishing the onboarding completion event: this prompt touches that territory.

---

**Example 75**

Input: The user activates their account via email verification. From that point, what sequence of steps leads to them having a personalized dashboard?

Output:
- Who marks the account as activated when the verification is confirmed: this prompt touches that territory.
- Who generates the questionnaire for the now-activated user: this prompt touches that territory.
- Who delivers it via chat: this prompt touches that territory.
- Who stores the user's answers when they complete it: this prompt touches that territory.
- Who publishes the completion event that unlocks personalization: this prompt touches that territory.
- Who loads the dashboard with the personalized content triggered by that event: this prompt touches that territory.

