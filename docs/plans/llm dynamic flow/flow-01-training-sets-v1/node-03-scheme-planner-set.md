# Node 03 - Scheme Planner Training Set

Node being trained: LLM unit to configure scheme plan A -> db1 db2 db3 ...

Main edge preserved: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### ROW SCHEME_PLANNER-127
Source: 01-user-registration.md:L127
Source section: running and reliability
Original source text: «### For DevOps»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-129
Source: 01-user-registration.md:L129
Source section: running and reliability
Original source text: «**Services to Deploy**:»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-130
Source: 01-user-registration.md:L130
Source section: running and reliability
Original source text: «| Service | Tech Stack | Scaling | Health Check |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-131
Source: 01-user-registration.md:L131
Source section: running and reliability
Original source text: «|---------|-----------|---------|-------------|»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-132
Source: 01-user-registration.md:L132
Source section: running and reliability
Original source text: «| auth-service | Nest.js + TypeScript | Horizontal, CPU-based (>70%) | GET /health + JWT validation test |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-133
Source: 01-user-registration.md:L133
Source section: running and reliability
Original source text: «| user-service | Nest.js + TypeScript | Horizontal, request-based | GET /health + DB connectivity |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-134
Source: 01-user-registration.md:L134
Source section: running and reliability
Original source text: «| email-service | Nest.js + TypeScript | Queue-depth based | GET /health + SMTP connectivity |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-135
Source: 01-user-registration.md:L135
Source section: running and reliability
Original source text: «| questionnaire-service | Nest.js + TypeScript | Horizontal, request-based | GET /health + MongoDB ping |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-136
Source: 01-user-registration.md:L136
Source section: running and reliability
Original source text: «| messaging-service | Python + FastAPI | WebSocket connection count | GET /health + Redis ping |»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-138
Source: 01-user-registration.md:L138
Source section: running and reliability
Original source text: «**Infrastructure Dependencies**:»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-139
Source: 01-user-registration.md:L139
Source section: running and reliability
Original source text: «- PostgreSQL: auth-service primary store (connection pool: 20-50)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-140
Source: 01-user-registration.md:L140
Source section: running and reliability
Original source text: «- MongoDB: user-service, questionnaire-service (replica set recommended)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-141
Source: 01-user-registration.md:L141
Source section: running and reliability
Original source text: «- Redis: session store, rate limiter, token blacklist (Sentinel for HA)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-142
Source: 01-user-registration.md:L142
Source section: running and reliability
Original source text: «- Kafka: event bus for all domain events (topic: `user-events`)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-143
Source: 01-user-registration.md:L143
Source section: running and reliability
Original source text: «- RabbitMQ: email queue (`email-queue`) for reliable delivery»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-145
Source: 01-user-registration.md:L145
Source section: running and reliability
Original source text: «**Monitoring & Alerting**:»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-146
Source: 01-user-registration.md:L146
Source section: running and reliability
Original source text: «- Alert: registration success rate drops below 95%»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-147
Source: 01-user-registration.md:L147
Source section: running and reliability
Original source text: «- Alert: email verification rate drops below 70% (24hr window)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-148
Source: 01-user-registration.md:L148
Source section: running and reliability
Original source text: «- Alert: SSO provider response time > 3s»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-149
Source: 01-user-registration.md:L149
Source section: running and reliability
Original source text: «- Alert: dead letter queue depth > 10 messages»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-150
Source: 01-user-registration.md:L150
Source section: running and reliability
Original source text: «- Dashboard: registration funnel visualization by provider, region, time»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-152
Source: 01-user-registration.md:L152
Source section: running and reliability
Original source text: «**Failure Blast Radius**: Auth service failure = complete registration block. User service failure = registrations queue up in Kafka (auto-recover on restart). Email service failure = verifications delayed but not lost (RabbitMQ durability).»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-154
Source: 01-user-registration.md:L154
Source section: running and reliability
Original source text: «**SLAs**: Registration endpoint: p99 < 2s. Email delivery: within 60s. Questionnaire delivery: within 5s of account activation.»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-158
Source: 01-user-registration.md:L158
Source section: human stories
Original source text: «## User Story»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-160
Source: 01-user-registration.md:L160
Source section: human stories
Original source text: «**Primary**: As a small business owner, I want to sign up quickly using my Google/Facebook/LinkedIn account, so that I can start networking with other business owners without creating yet another username and password.»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-162
Source: 01-user-registration.md:L162
Source section: human stories
Original source text: «**Secondary**: As a business owner who prefers email registration, I want to create an account with my email and password, so that I maintain control over my login credentials without depending on social providers.»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-164
Source: 01-user-registration.md:L164
Source section: human stories
Original source text: «**Onboarding**: As a newly registered user, I want to receive a guided questionnaire via chat, so that the platform can understand my business needs and start showing me relevant matches immediately.»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-166
Source: 01-user-registration.md:L166
Source section: main paths
Original source text: «## Business Flow (Happy Path)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-168
Source: 01-user-registration.md:L168
Source section: main paths
Original source text: «### SSO Path»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-169
Source: 01-user-registration.md:L169
Source section: main paths
Original source text: «1. User clicks "Sign up with Google" on the landing page»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-170
Source: 01-user-registration.md:L170
Source section: main paths
Original source text: «2. Frontend redirects to Google OAuth consent screen»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-171
Source: 01-user-registration.md:L171
Source section: main paths
Original source text: «3. User approves permissions (email, profile)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-172
Source: 01-user-registration.md:L172
Source section: main paths
Original source text: «4. Google redirects back with authorization code»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-173
Source: 01-user-registration.md:L173
Source section: main paths
Original source text: «5. Auth Service exchanges code for access token with Google»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-174
Source: 01-user-registration.md:L174
Source section: main paths
Original source text: «6. Auth Service fetches user profile from Google»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-175
Source: 01-user-registration.md:L175
Source section: main paths
Original source text: «7. Auth Service checks if user email exists in the database»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-176
Source: 01-user-registration.md:L176
Source section: main paths
Original source text: «8. Auth Service creates new user record (status: active)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-177
Source: 01-user-registration.md:L177
Source section: main paths
Original source text: «9. Auth Service generates platform JWT (access + refresh tokens)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-178
Source: 01-user-registration.md:L178
Source section: main paths
Original source text: «10. Auth Service publishes `UserSSOAuthenticated` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-179
Source: 01-user-registration.md:L179
Source section: main paths
Original source text: «11. User Service consumes event, creates full user profile with defaults»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-180
Source: 01-user-registration.md:L180
Source section: main paths
Original source text: «12. User Service publishes `UserCreated` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-181
Source: 01-user-registration.md:L181
Source section: main paths
Original source text: «13. Questionnaire Service consumes event, generates initial questionnaire»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-182
Source: 01-user-registration.md:L182
Source section: main paths
Original source text: «14. Questionnaire Service publishes `QuestionnaireRequired` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-183
Source: 01-user-registration.md:L183
Source section: main paths
Original source text: «15. Messaging Service consumes event, delivers questionnaire via chat»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-184
Source: 01-user-registration.md:L184
Source section: main paths
Original source text: «16. Messaging Service publishes `QuestionnaireSent` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-185
Source: 01-user-registration.md:L185
Source section: main paths
Original source text: «17. User fills out questionnaire in the chat interface»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-186
Source: 01-user-registration.md:L186
Source section: main paths
Original source text: «18. Questionnaire Service validates and stores responses»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-187
Source: 01-user-registration.md:L187
Source section: main paths
Original source text: «19. Questionnaire Service publishes `QuestionnaireCompleted` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-188
Source: 01-user-registration.md:L188
Source section: main paths
Original source text: «20. User Service updates onboarding status to complete»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-189
Source: 01-user-registration.md:L189
Source section: main paths
Original source text: «21. User Service publishes `UserOnboardingCompleted` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-190
Source: 01-user-registration.md:L190
Source section: main paths
Original source text: «22. Dashboard loads with personalized content»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-192
Source: 01-user-registration.md:L192
Source section: main paths
Original source text: «### Email Path»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-193
Source: 01-user-registration.md:L193
Source section: main paths
Original source text: «1. User clicks "Sign up with Email" on the landing page»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-194
Source: 01-user-registration.md:L194
Source section: main paths
Original source text: «2. User enters email, password, and basic profile data»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-195
Source: 01-user-registration.md:L195
Source section: main paths
Original source text: «3. Auth Service validates input (email format, password strength)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-196
Source: 01-user-registration.md:L196
Source section: main paths
Original source text: «4. Auth Service checks email doesn't already exist»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-197
Source: 01-user-registration.md:L197
Source section: main paths
Original source text: «5. Auth Service creates pending user record»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-198
Source: 01-user-registration.md:L198
Source section: main paths
Original source text: «6. Auth Service generates verification token (24hr expiry)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-199
Source: 01-user-registration.md:L199
Source section: main paths
Original source text: «7. Auth Service publishes `UserRegistrationInitiated` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-200
Source: 01-user-registration.md:L200
Source section: main paths
Original source text: «8. Email Service consumes event, sends verification email»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-201
Source: 01-user-registration.md:L201
Source section: main paths
Original source text: «9. Email Service publishes `VerificationEmailSent` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-202
Source: 01-user-registration.md:L202
Source section: main paths
Original source text: «10. User clicks verification link in email»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-203
Source: 01-user-registration.md:L203
Source section: main paths
Original source text: «11. Auth Service validates token (exists, not expired)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-204
Source: 01-user-registration.md:L204
Source section: main paths
Original source text: «12. Auth Service updates user status to verified»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-205
Source: 01-user-registration.md:L205
Source section: main paths
Original source text: «13. Auth Service publishes `EmailVerified` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-206
Source: 01-user-registration.md:L206
Source section: main paths
Original source text: «14. User Service consumes event, activates account»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-207
Source: 01-user-registration.md:L207
Source section: main paths
Original source text: «15. User Service publishes `UserActivated` event»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### ROW SCHEME_PLANNER-208
Source: 01-user-registration.md:L208
Source section: main paths
Original source text: «16. (Steps 13-22 from SSO path continue from here)»
A received by this node: Domain-language material that must become ordered or parallel work.
B this node must produce: A planned piece with its place in the provider path, email path, shared onboarding path, or waiting/recovery path.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: This teaches the planner how to turn source facts into pieces that can run in order or in parallel.

### HISTORY ROW H012
Source: umls.md:L012
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «## **Event Definitions for User Registration Flow**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H013
Source: umls.md:L013
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H014
Source: umls.md:L014
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **1\. UserSSOAuthenticated**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H015
Source: umls.md:L015
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H016
Source: umls.md:L016
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Auth Service after successful SSO authentication  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H017
Source: umls.md:L017
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H018
Source: umls.md:L018
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, provider, email, name, profilePicture, accessToken  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H019
Source: umls.md:L019
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: User Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H020
Source: umls.md:L020
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H021
Source: umls.md:L021
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **2\. UserRegistrationInitiated**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H022
Source: umls.md:L022
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H023
Source: umls.md:L023
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Auth Service for email-based registration  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H024
Source: umls.md:L024
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H025
Source: umls.md:L025
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * email, registrationToken, timestamp  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H026
Source: umls.md:L026
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Email Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H027
Source: umls.md:L027
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H028
Source: umls.md:L028
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **3\. VerificationEmailSent**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H029
Source: umls.md:L029
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H030
Source: umls.md:L030
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Email Service  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H031
Source: umls.md:L031
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H032
Source: umls.md:L032
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * email, emailId, sentAt, verificationLink  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H033
Source: umls.md:L033
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Audit Service (optional)»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H034
Source: umls.md:L034
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H035
Source: umls.md:L035
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **4\. EmailVerified**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H036
Source: umls.md:L036
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H037
Source: umls.md:L037
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Auth Service after token validation  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H038
Source: umls.md:L038
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H039
Source: umls.md:L039
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, email, verifiedAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H040
Source: umls.md:L040
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: User Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H041
Source: umls.md:L041
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H042
Source: umls.md:L042
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **5\. UserCreated**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H043
Source: umls.md:L043
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H044
Source: umls.md:L044
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: User Service for new users  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H045
Source: umls.md:L045
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H046
Source: umls.md:L046
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, email, registrationMethod, createdAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H047
Source: umls.md:L047
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Questionnaire Service, Analytics Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H048
Source: umls.md:L048
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H049
Source: umls.md:L049
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **6\. UserActivated**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H050
Source: umls.md:L050
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H051
Source: umls.md:L051
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: User Service after email verification  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H052
Source: umls.md:L052
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H053
Source: umls.md:L053
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, activatedAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H054
Source: umls.md:L054
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Questionnaire Service, Notification Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H055
Source: umls.md:L055
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H056
Source: umls.md:L056
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **7\. QuestionnaireRequired**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H057
Source: umls.md:L057
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H058
Source: umls.md:L058
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Questionnaire Service  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H059
Source: umls.md:L059
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H060
Source: umls.md:L060
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, questionnaireId, dueDate  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H061
Source: umls.md:L061
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Messaging Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H062
Source: umls.md:L062
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H063
Source: umls.md:L063
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **8\. QuestionnaireSent**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H064
Source: umls.md:L064
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H065
Source: umls.md:L065
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Messaging Service  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H066
Source: umls.md:L066
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H067
Source: umls.md:L067
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, messageId, questionnaireLink, sentAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H068
Source: umls.md:L068
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Audit Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H069
Source: umls.md:L069
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H070
Source: umls.md:L070
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **9\. QuestionnaireCompleted**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H071
Source: umls.md:L071
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H072
Source: umls.md:L072
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: Questionnaire Service  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H073
Source: umls.md:L073
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H074
Source: umls.md:L074
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, questionnaireId, responses, completedAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H075
Source: umls.md:L075
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: User Service, Analytics Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H076
Source: umls.md:L076
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H077
Source: umls.md:L077
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «### **10\. UserOnboardingCompleted**»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H078
Source: umls.md:L078
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H079
Source: umls.md:L079
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Triggered by: User Service  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H080
Source: umls.md:L080
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Payload:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H081
Source: umls.md:L081
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «  * userId, onboardingSteps, completedAt  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H082
Source: umls.md:L082
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «* Consumers: Messaging Service, Dashboard Service»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H083
Source: umls.md:L083
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain-language prompts -> scheme plan A -> b1...bn and fb1...fbn
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

