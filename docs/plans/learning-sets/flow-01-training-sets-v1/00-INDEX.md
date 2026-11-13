# 00 — Index
## Flow 01: User Registration & SSO Onboarding — Training Sets v1

Produced under: document-examination-method-v2.md
Round: 1 of 1 (one document, one simulation)

---

## Source Document Identity

**Path:** `C:\Users\lubao\Documents\xiigen\business flows\01-user-registration.md`
**File name:** 01-user-registration.md
**Line count:** 357 lines
**Byte size:** 21,094 bytes
**First meaningful line:** `# User Registration & SSO Onboarding`
**Last meaningful line:** "The React Native client will be extended with new registration forms and a chat-based onboarding interface. These components will interact with the the programming interface Gateway, which manages the redirection to SSO providers and handles the secure storage of the issued JWT."

**Why this document was chosen:** It is the first registered business flow in the xiigen platform and the entry point for every user. Every downstream flow — matching, events, social features — depends on having a registered, onboarded user. It is a natural first document to examine because its training data feeds all nine nodes with content that has high coverage of the pipeline's distinct functions.

---

## Version History

Two distinct versions of this document were found.

| Version | Path | Bytes | Line count | Last modified |
|---------|------|-------|------------|---------------|
| v1 (older) | `.claude\worktrees\[branch]\business flows\01-user-registration.md` | 20,950 | ~350 | 2026-04-24 |
| v2 (current, used as source) | `business flows\01-user-registration.md` | 21,094 | 357 | 2026-04-27 |

**Differences between versions:**
The older version (20,950 bytes) uses `flow_id: the user registration flow` in the YAML persona block and references a slightly different event payload structure in the event chain table. The current version (21,094 bytes) adds the Architectural Integration Strategy section (lines 313–357) describing how the new services integrate with the XIIGen platform V61 — including the MicroserviceBase inheritance pattern, the MongoDB/PostgreSQL/Redis interface usage, and the React Native client extensions. This section was not present in the older version.

**Conclusion:** These are two distinct versions of the same document, not duplicates. The current version (April 27) is the one used as the source for this round.

---

## Session Evidence

Searched across session files, history directories, and `.claude\worktrees` for evidence of when this document was created or discussed.

**Finding:** The document is referenced in multiple session files as an existing resource, not being created for the first time. The creation session was not found — the document appears to have existed before the sessions currently available.

**Classification:** Later use. The document is referenced in sessions after it existed. No creation session identified.

---

## Domains Defined by This Document

Six domains are defined by this document. These domain names are used throughout all nine node training set files.

1. **authentication** — The identity-verification domain. Covers OAuth2/OIDC token exchange with external providers, email and password credential validation, JWT issuance and storage, refresh token rotation, and verification token generation and validation.

2. **communication** — The message-delivery domain. Covers the RabbitMQ email queue, SMTP delivery of verification emails, in-app WebSocket chat delivery of questionnaires, delivery SLAs, dead letter queue routing for failed deliveries, and audit event publication after confirmed delivery.

3. **onboarding** — The user-setup domain. Covers personalized questionnaire generation, the three-criteria completion gate (profile created, questionnaire answered, business details included), delivery timing (within 5 seconds of activation), and the UserOnboardingCompleted gate event.

4. **security** — The protection-and-compliance domain. Covers rate limiting (5 attempts per IP per 15-minute window), account enumeration prevention (identical response bodies for matching and non-matching emails), OAuth redirect URI whitelist, CSRF protection on all auth endpoints, bcrypt password hashing at cost factor 12, and audit trail requirements.

5. **event coordination** — The sequencing domain. Covers the ordered event chains for both registration paths (6 events for SSO, 9 events for email), idempotency requirements for all event handlers (deduplication key: userId + eventType), and dead letter queue routing for permanently failed handlers.

6. **user management** — The account-state domain. Covers the onboarding state machine (pending → active → onboarding → complete), dormancy logic (reminders at 24 hours and 72 hours, dormant flag at 30 days), account merge logic for SSO arriving with an existing email match, and the userId preservation rule for merges.

---

## Related Documents (not used as source in this round)

The following documents are related to this flow and may serve as source documents in future examination rounds.

- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\FLOW-01-TRAINING-DATASET-PLAN-v3.md` — The authoritative plan specifying exact pair counts and domain names for the registration document training data. Used to verify this round's output against specification.
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` — The architecture plan for the dynamic flow system itself. Describes the pipeline structure that the training sets are designed to serve.
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\llm_pipeline_corrected.svg` — The UML diagram showing the exact pipeline topology, edge directions, and the arbiter feedback loop back to the domain splitter.

---

## Files in This Round

```
flow-01-training-sets-v1/
  00-INDEX.md                    (this file)
  01-UML-FRAME.md                (edge inventory — binding frame for all node files)
  node-01-domain-splitter-set.md
  node-02-domain-translator-set.md
  node-03-scheme-planner-set.md
  node-04-domain-processor-set.md
  node-05-applier-set.md
  node-06-result-translator-set.md
  node-07-arbiter-selector-set.md
  node-08-arbiter-units-set.md
  node-09-formatter-set.md
  10-SIMULATION-CHECK.md
```
