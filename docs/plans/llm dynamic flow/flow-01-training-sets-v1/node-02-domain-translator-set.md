# Node 02 - Domain Translator Training Set

Node being trained: LLM unit to translate each prompt fragment into domain language

Main edge preserved: Domain splitter -> one domain-language prompt per selected domain

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### ROW DOMAIN_TRANSLATOR-024
Source: 01-user-registration.md:L024
Source section: builder-facing shape
Original source text: «## Persona Descriptions»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-026
Source: 01-user-registration.md:L026
Source section: builder-facing shape
Original source text: «### For AI / Code Generation»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-028
Source: 01-user-registration.md:L028
Source section: builder-facing shape
Original source text: «```yaml»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-029
Source: 01-user-registration.md:L029
Source section: builder-facing shape
Original source text: «flow_id: the user registration flow»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-030
Source: 01-user-registration.md:L030
Source section: builder-facing shape
Original source text: «entry_points:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-031
Source: 01-user-registration.md:L031
Source section: builder-facing shape
Original source text: «  - POST /auth/sso/{provider}  # SSO path (Google, Facebook, LinkedIn, Figma)»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-032
Source: 01-user-registration.md:L032
Source section: builder-facing shape
Original source text: «  - POST /auth/register         # Email/password path»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-033
Source: 01-user-registration.md:L033
Source section: builder-facing shape
Original source text: «  - GET /auth/verify?token=     # Email verification callback»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-035
Source: 01-user-registration.md:L035
Source section: builder-facing shape
Original source text: «services:»
A received by this node: Source line plus its selected domain labels: running and reliability.
B this node must produce: A clear work statement in the language of: running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-036
Source: 01-user-registration.md:L036
Source section: builder-facing shape
Original source text: «  - auth-service:     Validates OAuth/credentials, issues JWT, publishes auth events»
A received by this node: Source line plus its selected domain labels: identity and access; event movement; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; event movement; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-037
Source: 01-user-registration.md:L037
Source section: builder-facing shape
Original source text: «  - user-service:     Creates user profile, manages onboarding state machine»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-038
Source: 01-user-registration.md:L038
Source section: builder-facing shape
Original source text: «  - email-service:    Sends verification emails (consumes UserRegistrationInitiated)»
A received by this node: Source line plus its selected domain labels: identity and access; communication delivery; event movement; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; communication delivery; event movement; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-039
Source: 01-user-registration.md:L039
Source section: builder-facing shape
Original source text: «  - questionnaire-service: Generates initial questionnaire (consumes UserCreated/UserActivated)»
A received by this node: Source line plus its selected domain labels: onboarding and account state; event movement; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; event movement; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-040
Source: 01-user-registration.md:L040
Source section: builder-facing shape
Original source text: «  - messaging-service:     Delivers questionnaire via chat (consumes QuestionnaireRequired)»
A received by this node: Source line plus its selected domain labels: onboarding and account state; communication delivery; event movement; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; communication delivery; event movement; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-042
Source: 01-user-registration.md:L042
Source section: builder-facing shape
Original source text: «event_chain:»
A received by this node: Source line plus its selected domain labels: event movement.
B this node must produce: A clear work statement in the language of: event movement.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-043
Source: 01-user-registration.md:L043
Source section: builder-facing shape
Original source text: «  sso_path:»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-044
Source: 01-user-registration.md:L044
Source section: builder-facing shape
Original source text: «    1. UserSSOAuthenticated → user-service»
A received by this node: Source line plus its selected domain labels: identity and access; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-045
Source: 01-user-registration.md:L045
Source section: builder-facing shape
Original source text: «    2. UserCreated → questionnaire-service, analytics-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability; measurement and personalization.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-046
Source: 01-user-registration.md:L046
Source section: builder-facing shape
Original source text: «    3. QuestionnaireRequired → messaging-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-047
Source: 01-user-registration.md:L047
Source section: builder-facing shape
Original source text: «    4. QuestionnaireSent → audit-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; safety and compliance review; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; safety and compliance review; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-048
Source: 01-user-registration.md:L048
Source section: builder-facing shape
Original source text: «    5. QuestionnaireCompleted → user-service, analytics-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability; measurement and personalization.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-049
Source: 01-user-registration.md:L049
Source section: builder-facing shape
Original source text: «    6. UserOnboardingCompleted → messaging-service, dashboard-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-051
Source: 01-user-registration.md:L051
Source section: builder-facing shape
Original source text: «  email_path:»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-052
Source: 01-user-registration.md:L052
Source section: builder-facing shape
Original source text: «    1. UserRegistrationInitiated → email-service»
A received by this node: Source line plus its selected domain labels: identity and access; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-053
Source: 01-user-registration.md:L053
Source section: builder-facing shape
Original source text: «    2. VerificationEmailSent → audit-service»
A received by this node: Source line plus its selected domain labels: identity and access; safety and compliance review; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; safety and compliance review; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-054
Source: 01-user-registration.md:L054
Source section: builder-facing shape
Original source text: «    3. EmailVerified → user-service»
A received by this node: Source line plus its selected domain labels: identity and access; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-055
Source: 01-user-registration.md:L055
Source section: builder-facing shape
Original source text: «    4. UserCreated → questionnaire-service, analytics-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability; measurement and personalization.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-056
Source: 01-user-registration.md:L056
Source section: builder-facing shape
Original source text: «    5. UserActivated → questionnaire-service, notification-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; communication delivery; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; communication delivery; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-057
Source: 01-user-registration.md:L057
Source section: builder-facing shape
Original source text: «    6. QuestionnaireRequired → messaging-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-058
Source: 01-user-registration.md:L058
Source section: builder-facing shape
Original source text: «    7. QuestionnaireSent → audit-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; safety and compliance review; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; safety and compliance review; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-059
Source: 01-user-registration.md:L059
Source section: builder-facing shape
Original source text: «    8. QuestionnaireCompleted → user-service, analytics-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability; measurement and personalization.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-060
Source: 01-user-registration.md:L060
Source section: builder-facing shape
Original source text: «    9. UserOnboardingCompleted → messaging-service, dashboard-service»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-062
Source: 01-user-registration.md:L062
Source section: builder-facing shape
Original source text: «data_stores:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-063
Source: 01-user-registration.md:L063
Source section: builder-facing shape
Original source text: «  - PostgreSQL: user credentials, tokens, sessions (auth-service)»
A received by this node: Source line plus its selected domain labels: identity and access; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-064
Source: 01-user-registration.md:L064
Source section: builder-facing shape
Original source text: «  - MongoDB: user profiles, questionnaire responses (user-service, questionnaire-service)»
A received by this node: Source line plus its selected domain labels: onboarding and account state; running and reliability.
B this node must produce: A clear work statement in the language of: onboarding and account state; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-065
Source: 01-user-registration.md:L065
Source section: builder-facing shape
Original source text: «  - Redis: session cache, rate limiting, token blacklist (auth-service)»
A received by this node: Source line plus its selected domain labels: identity and access; safety and compliance review; running and reliability.
B this node must produce: A clear work statement in the language of: identity and access; safety and compliance review; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-067
Source: 01-user-registration.md:L067
Source section: builder-facing shape
Original source text: «error_handling:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-068
Source: 01-user-registration.md:L068
Source section: builder-facing shape
Original source text: «  - SSO provider timeout: retry with exponential backoff (3 attempts, 1s/2s/4s)»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-069
Source: 01-user-registration.md:L069
Source section: builder-facing shape
Original source text: «  - Duplicate email on SSO: merge profiles if SSO email matches existing email-registered account»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state; account consistency.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state; account consistency.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-070
Source: 01-user-registration.md:L070
Source section: builder-facing shape
Original source text: «  - Verification token expired: allow re-send with new 24hr token»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-071
Source: 01-user-registration.md:L071
Source section: builder-facing shape
Original source text: «  - Dead letter queue: all failed event handlers write to DLQ for manual review»
A received by this node: Source line plus its selected domain labels: event movement.
B this node must produce: A clear work statement in the language of: event movement.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-073
Source: 01-user-registration.md:L073
Source section: builder-facing shape
Original source text: «idempotency:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-074
Source: 01-user-registration.md:L074
Source section: builder-facing shape
Original source text: «  - All event handlers must be idempotent (use userId + eventType as dedup key)»
A received by this node: Source line plus its selected domain labels: event movement; account consistency.
B this node must produce: A clear work statement in the language of: event movement; account consistency.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-075
Source: 01-user-registration.md:L075
Source section: builder-facing shape
Original source text: «  - UserCreated handler checks if profile already exists before creating»
A received by this node: Source line plus its selected domain labels: onboarding and account state.
B this node must produce: A clear work statement in the language of: onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-076
Source: 01-user-registration.md:L076
Source section: builder-facing shape
Original source text: «```»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-078
Source: 01-user-registration.md:L078
Source section: product and journey
Original source text: «### For Product Manager»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-080
Source: 01-user-registration.md:L080
Source section: product and journey
Original source text: «**Business Value**: This is the funnel entry point — every user's first experience with the platform. Registration friction directly impacts conversion rates.»
A received by this node: Source line plus its selected domain labels: measurement and personalization.
B this node must produce: A clear work statement in the language of: measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-082
Source: 01-user-registration.md:L082
Source section: product and journey
Original source text: «**Key Metrics to Track**:»
A received by this node: Source line plus its selected domain labels: measurement and personalization.
B this node must produce: A clear work statement in the language of: measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-083
Source: 01-user-registration.md:L083
Source section: product and journey
Original source text: «- Registration completion rate (started vs. completed) by provider»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-084
Source: 01-user-registration.md:L084
Source section: product and journey
Original source text: «- Email verification rate and time-to-verify»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-085
Source: 01-user-registration.md:L085
Source section: product and journey
Original source text: «- Questionnaire completion rate after registration»
A received by this node: Source line plus its selected domain labels: onboarding and account state.
B this node must produce: A clear work statement in the language of: onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-086
Source: 01-user-registration.md:L086
Source section: product and journey
Original source text: «- Drop-off points in the onboarding funnel»
A received by this node: Source line plus its selected domain labels: onboarding and account state.
B this node must produce: A clear work statement in the language of: onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-087
Source: 01-user-registration.md:L087
Source section: product and journey
Original source text: «- SSO vs. email registration split»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-088
Source: 01-user-registration.md:L088
Source section: product and journey
Original source text: «- Time from registration start to onboarding complete»
A received by this node: Source line plus its selected domain labels: onboarding and account state.
B this node must produce: A clear work statement in the language of: onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-090
Source: 01-user-registration.md:L090
Source section: product and journey
Original source text: «**Feature Dependencies**: This flow must be complete before any other flow can function. Matching (the business onboarding flow), Events (the event creation flow), and all social features depend on having registered, onboarded users.»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state; event movement; measurement and personalization.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state; event movement; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-092
Source: 01-user-registration.md:L092
Source section: product and journey
Original source text: «**A/B Testing Opportunities**:»
A received by this node: Source line plus its selected domain labels: measurement and personalization.
B this node must produce: A clear work statement in the language of: measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-093
Source: 01-user-registration.md:L093
Source section: product and journey
Original source text: «- Number of questionnaire questions shown initially (5 vs. 10 vs. progressive)»
A received by this node: Source line plus its selected domain labels: onboarding and account state.
B this node must produce: A clear work statement in the language of: onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-094
Source: 01-user-registration.md:L094
Source section: product and journey
Original source text: «- Questionnaire delivery method (modal popup vs. chat message vs. email)»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state; communication delivery.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state; communication delivery.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-095
Source: 01-user-registration.md:L095
Source section: product and journey
Original source text: «- Skip-questionnaire option with reduced matching quality»
A received by this node: Source line plus its selected domain labels: onboarding and account state; measurement and personalization.
B this node must produce: A clear work statement in the language of: onboarding and account state; measurement and personalization.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-097
Source: 01-user-registration.md:L097
Source section: product and journey
Original source text: «**User Journey Context**: User arrives at landing page → sees anonymized stats and upcoming events → clicks "Sign up with Google" → authenticates → lands on dashboard → receives chat message with questionnaire → completes questionnaire → sees first match suggestions.»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state; communication delivery; event movement.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state; communication delivery; event movement.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-099
Source: 01-user-registration.md:L099
Source section: safety and privacy
Original source text: «### For IT Security Manager»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-101
Source: 01-user-registration.md:L101
Source section: safety and privacy
Original source text: «**Authentication Architecture**:»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-102
Source: 01-user-registration.md:L102
Source section: safety and privacy
Original source text: «- OAuth2/OpenID Connect integration with Google, Facebook, LinkedIn, Figma»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-103
Source: 01-user-registration.md:L103
Source section: safety and privacy
Original source text: «- JWT tokens with short-lived access tokens (15 min) and longer refresh tokens (7 days)»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-104
Source: 01-user-registration.md:L104
Source section: safety and privacy
Original source text: «- Refresh token rotation: old refresh token invalidated on each use»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-105
Source: 01-user-registration.md:L105
Source section: safety and privacy
Original source text: «- Tokens stored in httpOnly, Secure, SameSite cookies (not localStorage)»
A received by this node: Source line plus its selected domain labels: identity and access; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-107
Source: 01-user-registration.md:L107
Source section: safety and privacy
Original source text: «**Data Sensitivity**:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-108
Source: 01-user-registration.md:L108
Source section: safety and privacy
Original source text: «- personal data collected: email, name, profile picture (from SSO), business details (from questionnaire)»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-109
Source: 01-user-registration.md:L109
Source section: safety and privacy
Original source text: «- Classification: Confidential (user credentials), Internal (profile data)»
A received by this node: Source line plus its selected domain labels: identity and access; onboarding and account state; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; onboarding and account state; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-110
Source: 01-user-registration.md:L110
Source section: safety and privacy
Original source text: «- Password hashing: bcrypt with cost factor 12 (email registration path)»
A received by this node: Source line plus its selected domain labels: identity and access; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-111
Source: 01-user-registration.md:L111
Source section: safety and privacy
Original source text: «- SSO tokens: never stored long-term, exchanged for platform JWT immediately»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-113
Source: 01-user-registration.md:L113
Source section: safety and privacy
Original source text: «**Attack Surface**:»
A received by this node: Source line plus its selected domain labels: safety and compliance review.
B this node must produce: A clear work statement in the language of: safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-114
Source: 01-user-registration.md:L114
Source section: safety and privacy
Original source text: «- OAuth redirect URI validation (strict whitelist, no open redirects)»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-115
Source: 01-user-registration.md:L115
Source section: safety and privacy
Original source text: «- Rate limiting: 5 registration attempts per IP per 15 minutes»
A received by this node: Source line plus its selected domain labels: safety and compliance review.
B this node must produce: A clear work statement in the language of: safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-116
Source: 01-user-registration.md:L116
Source section: safety and privacy
Original source text: «- CSRF protection on all auth endpoints»
A received by this node: Source line plus its selected domain labels: identity and access; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-117
Source: 01-user-registration.md:L117
Source section: safety and privacy
Original source text: «- Verification token: cryptographically random, single-use, 24hr expiry»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-118
Source: 01-user-registration.md:L118
Source section: safety and privacy
Original source text: «- Account enumeration prevention: same response for "email exists" and "email not found"»
A received by this node: Source line plus its selected domain labels: identity and access; event movement; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; event movement; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-120
Source: 01-user-registration.md:L120
Source section: safety and privacy
Original source text: «**Compliance**:»
A received by this node: Source line plus its selected domain labels: document structure and meaning.
B this node must produce: A clear work statement in the language of: document structure and meaning.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-121
Source: 01-user-registration.md:L121
Source section: safety and privacy
Original source text: «- data protection law: consent capture at registration, right to deletion support»
A received by this node: Source line plus its selected domain labels: safety and compliance review.
B this node must produce: A clear work statement in the language of: safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-122
Source: 01-user-registration.md:L122
Source section: safety and privacy
Original source text: «- Audit trail: all auth events logged with IP, user-agent, timestamp»
A received by this node: Source line plus its selected domain labels: identity and access; event movement; safety and compliance review.
B this node must produce: A clear work statement in the language of: identity and access; event movement; safety and compliance review.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-123
Source: 01-user-registration.md:L123
Source section: safety and privacy
Original source text: «- Data retention: verification tokens purged after 24hrs, failed attempts logged for 90 days»
A received by this node: Source line plus its selected domain labels: identity and access.
B this node must produce: A clear work statement in the language of: identity and access.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### ROW DOMAIN_TRANSLATOR-125
Source: 01-user-registration.md:L125
Source section: safety and privacy
Original source text: «**Audit Trail**: All 10 events in this flow are published to the event bus and can be consumed by an Audit Service for compliance logging.»
A received by this node: Source line plus its selected domain labels: event movement; safety and compliance review; running and reliability.
B this node must produce: A clear work statement in the language of: event movement; safety and compliance review; running and reliability.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: This teaches the domain-language LLM how to say the same source fact in the proper working language without shortening it.

### HISTORY ROW H005
Source: umls.md:L005
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «Mapped out user registration event sequence for system diagram diagram.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H006
Source: umls.md:L006
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «I'll help you define the event sequence for the user registration scenario and create a system diagram sequence diagram to visualize the event flow in your event-driven architecture.  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H007
Source: umls.md:L007
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «Let me break down the event sequence and create a diagram for the user registration flow:  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H008
Source: umls.md:L008
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «User Registration Event Sequence Diagram  »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H009
Source: umls.md:L009
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «Diagram   »
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H010
Source: umls.md:L010
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «Here's the complete event sequence for your user registration scenario. Let me also provide you with the detailed event definitions:»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H011
Source: umls.md:L011
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: Domain splitter -> one domain-language prompt per selected domain
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

