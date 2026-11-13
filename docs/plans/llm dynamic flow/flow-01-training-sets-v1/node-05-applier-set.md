# Node 05 - Applier Array Training Set

Node being trained: LLM unit array to apply fbi(bi)->Rqi

Main edge preserved: processed planned part + transformation prompt -> R1...Rn

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### ROW APPLIER-285
Source: 01-user-registration.md:L285
Source section: events
Original source text: «## Event Definitions»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-287
Source: 01-user-registration.md:L287
Source section: events
Original source text: «| Event | Publisher | Consumers | Key Payload Fields |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-288
Source: 01-user-registration.md:L288
Source section: events
Original source text: «|-------|-----------|-----------|-------------------|»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-289
Source: 01-user-registration.md:L289
Source section: events
Original source text: «| `UserSSOAuthenticated` | Auth Service | User Service | userId, provider, email, name, profilePicture, accessToken, isNewUser |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-290
Source: 01-user-registration.md:L290
Source section: events
Original source text: «| `UserRegistrationInitiated` | Auth Service | Email Service | email, registrationToken, userId, tokenExpiry |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-291
Source: 01-user-registration.md:L291
Source section: events
Original source text: «| `VerificationEmailSent` | Email Service | Audit Service | email, emailId, sentAt, verificationLink |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-292
Source: 01-user-registration.md:L292
Source section: events
Original source text: «| `EmailVerified` | Auth Service | User Service | userId, email, verifiedAt, ipAddress |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-293
Source: 01-user-registration.md:L293
Source section: events
Original source text: «| `UserCreated` | User Service | Questionnaire Service, Analytics Service | userId, email, registrationMethod, createdAt, onboardingSteps |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-294
Source: 01-user-registration.md:L294
Source section: events
Original source text: «| `UserActivated` | User Service | Questionnaire Service, Notification Service | userId, activatedAt, enabledFeatures |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-295
Source: 01-user-registration.md:L295
Source section: events
Original source text: «| `QuestionnaireRequired` | Questionnaire Service | Messaging Service | userId, questionnaireId, dueDate |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-296
Source: 01-user-registration.md:L296
Source section: events
Original source text: «| `QuestionnaireSent` | Messaging Service | Audit Service | userId, messageId, questionnaireLink, sentAt |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-297
Source: 01-user-registration.md:L297
Source section: events
Original source text: «| `QuestionnaireCompleted` | Questionnaire Service | User Service, Analytics Service | userId, questionnaireId, responses, completedAt |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-298
Source: 01-user-registration.md:L298
Source section: events
Original source text: «| `UserOnboardingCompleted` | User Service | Messaging Service, Dashboard Service | userId, onboardingSteps, completedAt |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-300
Source: 01-user-registration.md:L300
Source section: services
Original source text: «## Services Involved»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-302
Source: 01-user-registration.md:L302
Source section: services
Original source text: «| Service | Role in Flow | Database | Scales On |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-303
Source: 01-user-registration.md:L303
Source section: services
Original source text: «|---------|-------------|----------|-----------|»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-304
Source: 01-user-registration.md:L304
Source section: services
Original source text: «| Auth Service | Entry point, SSO validation, JWT issuance, email verification | PostgreSQL + Redis | CPU (auth is compute-heavy with bcrypt) |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-305
Source: 01-user-registration.md:L305
Source section: services
Original source text: «| User Service | Profile creation, onboarding state management | MongoDB + PostgreSQL | Request count |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-306
Source: 01-user-registration.md:L306
Source section: services
Original source text: «| Email Service | Sends verification emails | — (stateless) | Queue depth (RabbitMQ `email-queue`) |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-307
Source: 01-user-registration.md:L307
Source section: services
Original source text: «| Questionnaire Service | Generates and manages initial questionnaire | MongoDB | Request count |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-308
Source: 01-user-registration.md:L308
Source section: services
Original source text: «| Messaging Service | Delivers questionnaire via chat | MongoDB + Redis | WebSocket connections |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-309
Source: 01-user-registration.md:L309
Source section: services
Original source text: «| Analytics Service | Tracks registration funnel metrics | Elasticsearch | Event throughput |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-310
Source: 01-user-registration.md:L310
Source section: services
Original source text: «| Audit Service | Logs all events for compliance | Elasticsearch | Event throughput |»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

### ROW APPLIER-313
Source: 01-user-registration.md:L313
Source section: services
Original source text: «To extend the XIIGen platform (V61) with the **User Registration & SSO Onboarding** flow, the integration should follow the existing microservices-based, engine-first architecture. This process involves incorporating several new services while utilizing the established generic interfaces and the unified flow system.»
A received by this node: One processed piece that must become an actual architecture result.
B this node must produce: A domain result preserving publisher, consumer, event, service, storage, or rule details from the line.
Edge this row prepares: processed planned part + transformation prompt -> R1...Rn
Why this row belongs here: This teaches the applier how source requirements become concrete architecture results.

