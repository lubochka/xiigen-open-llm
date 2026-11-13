# Node 01 - Domain Splitter Training Set

Node being trained: LLM unit to split user prompt to domains d1...dm

Main edge preserved: User prompt -> LLM unit to split user prompt to domains d1...dm

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### ROW DOMAIN_SPLITTER-001
Source: 01-user-registration.md:L001
Source section: identity header
Original source text: «# User Registration & SSO Onboarding»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: identity and access; onboarding and account state.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-003
Source: 01-user-registration.md:L003
Source section: identity header
Original source text: «> **Flow ID**: the user registration flow  »
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-004
Source: 01-user-registration.md:L004
Source section: identity header
Original source text: «> **Drawio Diagram**: Registration  »
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-005
Source: 01-user-registration.md:L005
Source section: identity header
Original source text: «> **Version**: 1.0  »
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-006
Source: 01-user-registration.md:L006
Source section: identity header
Original source text: «> **Last Updated**: 2026-02-25  »
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-010
Source: 01-user-registration.md:L010
Source section: overview
Original source text: «## Short Description»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-012
Source: 01-user-registration.md:L012
Source section: overview
Original source text: «Handles new user sign-up through multiple SSO providers (Google, Facebook, LinkedIn, Figma) or email-based registration, including email verification, account creation, initial questionnaire delivery via chat, and onboarding completion tracking.»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: identity and access; onboarding and account state; communication delivery.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-014
Source: 01-user-registration.md:L014
Source section: overview
Original source text: «## Long Description»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: document structure and meaning.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-016
Source: 01-user-registration.md:L016
Source section: overview
Original source text: «This flow is the entry point for every user into the Small Business Networking platform. It supports two primary registration paths: SSO-based (Google, Facebook, LinkedIn, Figma) and traditional email/password registration.»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: identity and access.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-018
Source: 01-user-registration.md:L018
Source section: overview
Original source text: «For SSO registration, the user authenticates with their chosen provider, the Auth Service validates the OAuth token, retrieves the user profile, and creates or merges the account. For email registration, the user submits credentials, receives a verification email with a time-limited token, and must verify before gaining full access.»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: identity and access; onboarding and account state; communication delivery; running and reliability; account consistency.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### ROW DOMAIN_SPLITTER-020
Source: 01-user-registration.md:L020
Source section: overview
Original source text: «Once the account is created and activated, the system triggers the onboarding sequence: the Questionnaire Service generates a personalized initial questionnaire and the Messaging Service delivers it to the user via an in-app chat message. The flow completes when the user finishes the questionnaire, at which point the system publishes `UserOnboardingCompleted` to trigger downstream personalization (matching, feed adaptation, learning programs).»
A received by this node: Raw document line from the user-registration architecture document.
B this node must produce: Domain labels: onboarding and account state; communication delivery; event movement; running and reliability; measurement and personalization.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: This teaches the first LLM which territory a registration sentence belongs to.

### HISTORY ROW H001
Source: umls.md:L001
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «I would like to plan an event driven design \- showing event flow in uml diagram. For that I will need a help defining event sequense for each user scenario \-»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H002
Source: umls.md:L002
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H003
Source: umls.md:L003
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «1. User registration \- check with sso provider \- internal/facebook/google/ figma and others, email verification, first setup questionnaire request to the user \- with a chat message»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

### HISTORY ROW H004
Source: umls.md:L004
Status: candidate history evidence, not proof of final markdown creation.
Original history text: «[blank line]»
A received by this node: history evidence related to the same registration flow.
B this node must produce: keep this history as supporting evidence without mixing it into the source document text.
Edge this row prepares: User prompt -> LLM unit to split user prompt to domains d1...dm
Why this row belongs here: it helps explain where the registration-event wording likely came from, while staying separate from the one source document.

