# Node-09 - Formatter

## Node purpose

Node-09 receives approved user-language fragments from Node-06 after arbitration. It folds
them into one readable output for the intended audience. It does not expose internal
review decisions unless the audience asked for examination.

## Pair 9.1 - New user after Google sign-up

Runtime A:
Approved fragments:
- "You are signed in with Google."
- "Your account is ready."
- "We sent your first setup questionnaire in chat."
Audience: new end user.

Expected B:
"You are signed in with Google. Welcome! Your account is ready, and your first setup
questionnaire is waiting in chat. Answer it to help us show you better business matches."

Alternate A:
Approved SSO success plus questionnaire delivery fragments for an end user.

Alternate B:
"Welcome. Your Google sign-up is complete, and the setup questions are in chat so we can
personalize your matches."

## Pair 9.2 - Email registration waiting for verification

Runtime A:
Approved fragments:
- "Your account was created as pending."
- "We sent a verification email."
- "The link expires in twenty-four hours."
Audience: new end user.

Expected B:
"Almost there. We sent a verification email to your address. Click the link within
twenty-four hours to activate your account."

Alternate A:
Approved pending account and verification email fragments.

Alternate B:
"Check your inbox to finish registration. The verification link is valid for twenty-four
hours."

## Pair 9.3 - Email verified and questionnaire ready

Runtime A:
Approved fragments:
- "Your email is verified."
- "Your account is active."
- "Your setup questionnaire is waiting in chat."
Audience: new end user.

Expected B:
"Your email is verified and your account is active. Your setup questionnaire is now waiting
in chat."

Alternate A:
Approved activation plus onboarding fragments.

Alternate B:
"Verification complete. You can use your account now, and the setup questions are in chat."

## Pair 9.4 - Onboarding complete

Runtime A:
Approved fragments:
- "Profile exists."
- "Questionnaire completed."
- "Business details submitted."
- "The platform can now personalize matches, feed, and dashboard."
Audience: new end user.

Expected B:
"You are all set. Your profile and business details are complete, so we can now personalize
your matches, feed, and dashboard."

Alternate A:
Approved onboarding completion fragments.

Alternate B:
"Onboarding is complete. The platform can start tailoring your experience."

## Pair 9.5 - Provider unavailable

Runtime A:
Approved fragments:
- "Google sign-up is temporarily unavailable."
- "Other providers and email registration still work."
- "No registration state was lost."
Audience: new end user.

Expected B:
"Google sign-up is temporarily unavailable. You can continue with another provider or sign
up by email."

Alternate A:
Approved provider outage fragments for a registration page.

Alternate B:
"That sign-up option is unavailable right now, but the other sign-up options are still
open."

## Pair 9.6 - Recoverable questionnaire delay

Runtime A:
Approved fragments:
- "Your account is active."
- "The questionnaire was not delivered yet."
- "The work was preserved for retry."
Audience: new end user.

Expected B:
"Your account is ready. Your setup questionnaire will arrive shortly."

Alternate A:
Approved active account plus delayed questionnaire fragments.

Alternate B:
"You can start using your account now. We will send the setup questions as soon as they
are available."

## Pair 9.7 - Product manager summary

Runtime A:
Approved fragments:
- "Registration path: Google SSO."
- "Account created."
- "Questionnaire sent."
- "Onboarding completed."
- "No drop-off detected."
Audience: product manager.

Expected B:
"Registration completed. Path: Google SSO. Account creation succeeded, questionnaire was
sent, and onboarding was completed. No drop-off was detected in this run."

Alternate A:
Approved funnel fragments for product review.

Alternate B:
"Funnel result: completed through Google SSO. The user reached onboarding completion with
no recorded drop-off."

## Pair 9.8 - Security manager summary

Runtime A:
Approved fragments:
- "Provider sign-in was used."
- "Tokens stored in secure cookies."
- "Rate limit applied at five attempts per IP per fifteen minutes."
- "Email existence response is neutral."
- "No security arbiter flags."
Audience: security manager.

Expected B:
"Security review passed. Provider sign-in was used, session tokens were stored in secure
cookies, rate limiting matched the five-attempt rule, and email existence was not exposed
in the user-visible response."

Alternate A:
Approved security fragments.

Alternate B:
"Registration security controls matched the document: secure cookie storage, correct rate
limit, neutral email-existence wording, and no security flags."

## Pair 9.9 - Operations summary

Runtime A:
Approved fragments:
- "Auth path succeeded."
- "Email delivery was delayed but preserved for retry."
- "Dead letter review contains the failed message."
- "Onboarding completion was not published."
Audience: operations team.

Expected B:
"Registration remained recoverable. The account path succeeded, the delayed email work was
preserved for retry or dead letter review, and onboarding completion was not published
early."

Alternate A:
Approved operations fragments for a recoverable failure.

Alternate B:
"Operational state: account ready, follow-up work preserved, no premature completion event."

## Pair 9.10 - Examination answer for a human reviewer

Runtime A:
Approved fragments:
- "Domain splitter selected authentication, onboarding, communication, and event coordination."
- "Planner produced SSO account creation, questionnaire delivery, and onboarding completion."
- "Arbiters passed security and event sequence."
Audience: human examining the flow output.

Expected B:
"The flow understood the request as a registration task touching authentication,
communication, onboarding, and event coordination. It planned the SSO account path, the
chat questionnaire, and the onboarding completion event. Security and event-order review
passed, so the approved result can be shown to the user."

Alternate A:
Approved trace fragments for explainable review.

Alternate B:
"The request was routed to the right registration domains, planned through account creation
and onboarding, reviewed for security and event order, and approved for final presentation."

---

