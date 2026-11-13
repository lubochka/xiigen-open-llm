# Simulation Checks

## Simulation 1 - Google sign-up with questionnaire

Input prompt:
"I want users to sign up with Google and then receive a quick questionnaire in chat."

Expected node path:
- Node-01: [authentication, onboarding, communication]
- Node-02: Google SSO wording; chat questionnaire wording
- Node-03: provider callback, account creation, session issue, questionnaire delivery,
  onboarding wait
- Node-04: process provider profile, account decision, questionnaire requirement
- Node-05: account created, session issued, questionnaire delivered
- Node-06: user-friendly SSO success and questionnaire message
- Node-07: select security, event sequence, privacy, onboarding arbiters
- Node-08: pass only if token storage, event order, and onboarding gate are correct
- Node-09: one end-user answer: signed in with Google, questionnaire in chat

Missing-if-fail:
If the simulation does not mention questionnaire delivery, Node-03 or Node-05 lacks coverage.
If it says onboarding is complete before answers, Node-08 lacks gate coverage.

## Simulation 2 - Email sign-up with expired verification link

Input prompt:
"A user registers by email but clicks the verification link the next day after it expired."

Expected node path:
- Node-01: [authentication, communication, security, user management]
- Node-02: pending email account, verification token, expired link, resend path
- Node-03: validate credentials, create pending account, send email, wait for callback,
  expired-token branch
- Node-04: process expired token as rejected verification
- Node-05: no activation; resend path available
- Node-06: "This verification link has expired. Ask for a new link."
- Node-07: select security and communication arbiters
- Node-08: pass only if old token is not reused
- Node-09: concise user message with resend instruction

Missing-if-fail:
If the simulation activates the account, Node-04 or Node-08 is wrong.
If it hides the resend path, Node-06 or Node-09 is thin.

## Simulation 3 - Existing email account tries Google SSO

Input prompt:
"I registered with email before, and now I want to sign in with Google using the same email."

Expected node path:
- Node-01: [authentication, user management, event coordination]
- Node-02: account merge wording
- Node-03: normalize email, match existing account, link provider, preserve identity
- Node-04: account merge required
- Node-05: existing user identity preserved, Google linked
- Node-06: "Your Google login has been linked to your existing account."
- Node-07: select data integrity, identity consistency, event sequence
- Node-08: flag if a new primary user identity is created
- Node-09: final user answer: Google added, original login still works

Missing-if-fail:
If the simulation creates a duplicate user, Node-03, Node-04, or Node-08 lacks coverage.

## Simulation 4 - Questionnaire service down

Input prompt:
"Registration succeeds, but the questionnaire service is down. What should happen?"

Expected node path:
- Node-01: [operations, onboarding, communication, user management]
- Node-02: account remains ready; questionnaire work preserved
- Node-03: complete account, preserve questionnaire request, show usable state, resume later
- Node-04: active account, delayed questionnaire, no onboarding completion
- Node-05: retry or dead letter review result
- Node-06: "Your account is ready. Your setup questionnaire will arrive shortly."
- Node-07: select resilience, onboarding gate, operations
- Node-08: flag if the failed questionnaire message is discarded or completion is published
- Node-09: final answer does not expose noisy internal failure to the end user

Missing-if-fail:
If the simulation rolls back the account, Node-03 or Node-05 is wrong.
If it publishes onboarding complete, Node-08 is wrong.

## Final self-check table

| Node | Runtime pairs in this file | Required shape checked |
|---|---:|---|
| Node-01 Domain splitter | 10 | Human prompt -> domain labels |
| Node-02 Domain translator | 10 | Fragment plus domain -> concrete formal wording |
| Node-03 Scheme planner | 10 | Domain wordings -> b1...bn plus fb1...fbn |
| Node-04 Domain processor array | 10 | One planned piece -> processed result |
| Node-05 Applier array | 10 | Processed piece -> concrete Rqi |
| Node-06 Result translator array | 12 | Formal result -> user/stakeholder wording |
| Node-07 Arbiter selector | 10 | Result group -> selected arbiters with reasons |
| Node-08 Arbiter units | 11 | Result review -> pass or flag plus fbx |
| Node-09 Formatter | 10 | Approved fragments -> one final answer |

Total runtime-shaped pairs: 93.

The source document has not been shortened into training input. It is used as evidence
for which runtime situations must be taught.

