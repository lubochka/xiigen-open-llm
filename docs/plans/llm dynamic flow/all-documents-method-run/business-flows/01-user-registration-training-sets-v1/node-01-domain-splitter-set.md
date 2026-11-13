# Node-01 - Domain Splitter

## Node purpose

A real human prompt enters this node. The node outputs all domains touched by the prompt.
One prompt can produce several labels.

## Pair 1.1 - External provider sign-up

Runtime A:
"I want users to sign up with Google, Facebook, LinkedIn, or Figma instead of creating another password."

Expected B:
[authentication]

Alternate A:
"Let a new user click Continue with Google and enter the platform without filling out a password form."

Alternate B:
[authentication]

Source evidence:
The document defines SSO registration through Google, Facebook, LinkedIn, and Figma.

## Pair 1.2 - Email sign-up with verification

Runtime A:
"Users should also be able to register with email and password, then click a verification link."

Expected B:
[authentication, communication]

Alternate A:
"I need email registration where the user gets a confirmation email before the account becomes active."

Alternate B:
[authentication, communication]

Source evidence:
The document defines email and password registration, a pending user record, and a verification email.

## Pair 1.3 - Chat questionnaire after account creation

Runtime A:
"After registration, send the new user a questionnaire in chat."

Expected B:
[onboarding, communication]

Alternate A:
"When someone creates an account, ask the first setup questions through the in-app chat."

Alternate B:
[onboarding, communication]

Source evidence:
The document says the Questionnaire Service generates the questionnaire and the Messaging Service delivers it via chat.

## Pair 1.4 - Completion triggers personalization

Runtime A:
"When the user finishes the questionnaire, start matching and personalize the feed."

Expected B:
[onboarding, event coordination]

Alternate A:
"Completing onboarding should trigger downstream personalization."

Alternate B:
[onboarding, event coordination]

Source evidence:
The document says UserOnboardingCompleted triggers matching, feed adaptation, learning programs, messaging, and dashboard behavior.

## Pair 1.5 - Abandoned registration reminders

Runtime A:
"If a user creates an account but does not finish the questionnaire, send reminders and later mark the account dormant."

Expected B:
[user management, communication, event coordination]

Alternate A:
"Track users who stop after registration and follow up after 24 hours, 72 hours, and 30 days."

Alternate B:
[user management, communication, event coordination]

Source evidence:
The document defines reminder email timing and dormant account handling.

## Pair 1.6 - Abuse protection

Runtime A:
"Registration must block automated attempts and avoid revealing whether an email already exists."

Expected B:
[security]

Alternate A:
"Protect the sign-up endpoint with rate limiting and account enumeration prevention."

Alternate B:
[security]

Source evidence:
The document defines rate limiting, CSRF protection, strict redirect validation, token rules, and identical responses for email existence checks.

## Pair 1.7 - SSO provider outage

Runtime A:
"If Google sign-up is down during maintenance, show other available providers and let the user continue another way."

Expected B:
[operations, authentication, client experience]

Alternate A:
"One SSO provider may be unavailable. The page should disable that option and offer email or another provider."

Alternate B:
[operations, authentication, client experience]

Source evidence:
The document has a scenario for registration during maintenance.

## Pair 1.8 - Mobile registration

Runtime A:
"Users should be able to register from the mobile app and return to the app after the provider approves SSO."

Expected B:
[client experience, authentication]

Alternate A:
"Mobile SSO should use the system browser or in-app browser tab and deep-link back into the app."

Alternate B:
[client experience, authentication]

Source evidence:
The document defines mobile registration through the React Native app and deep link callback.

## Pair 1.9 - Tenant-specific onboarding

Runtime A:
"During registration, users choose a business community, and the first questionnaire should match that community."

Expected B:
[tenant context, onboarding, user management]

Alternate A:
"Store the selected tenant from the start and vary the questionnaire by tenant."

Alternate B:
[tenant context, onboarding, user management]

Source evidence:
The document defines multi-tenant registration and tenant-specific questionnaire variation.

## Pair 1.10 - Funnel tracking

Runtime A:
"I want to know where people drop off in registration and how many finish by provider."

Expected B:
[product metrics, event coordination, user management]

Alternate A:
"Track registration completion rate, verification rate, questionnaire completion, and provider split."

Alternate B:
[product metrics, event coordination, user management]

Source evidence:
The product manager section defines funnel metrics and drop-off tracking.

---

