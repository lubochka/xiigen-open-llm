# Node-06 - Result Translator Array

## Node purpose

Each Node-06 unit receives one formal result Rqi and translates it into natural user or
stakeholder language. It also sees all results from the same run to avoid contradiction.

## Pair 6.1 - Translate SSO success

Runtime A:
R1 says: active user account created, Google linked, platform session issued, questionnaire
delivery can begin.

Expected B:
"You are signed in with Google. Your account is ready, and we are preparing your first
setup questionnaire."

Alternate A:
R1 says: new SSO user signed in and account exists.

Alternate B:
"Your Google sign-up worked. You can enter the platform now."

## Pair 6.2 - Translate merged account

Runtime A:
R2 says: existing account preserved, Google added as another login method, no duplicate
account created.

Expected B:
"Your Google login has been linked to your existing account. You can now sign in with
Google or with your original email and password."

Alternate A:
R2 says: provider linked to existing user identity.

Alternate B:
"We found your existing account and added this sign-in method to it."

## Pair 6.3 - Translate pending email account

Runtime A:
R3 says: pending account created, verification token created, account not active until
verification.

Expected B:
"Almost there. We created your account and sent a verification step. Please check your
email to activate it."

Alternate A:
R3 says: email account is waiting for verification.

Alternate B:
"Your account is waiting for email confirmation before you can use it."

## Pair 6.4 - Translate verification email sent

Runtime A:
R4 says: verification email delivered, link expires in twenty-four hours.

Expected B:
"We sent a verification email to your address. Click the link within twenty-four hours to
activate your account."

Alternate A:
R4 says: confirmation link delivered successfully.

Alternate B:
"Check your inbox for the confirmation link. It expires tomorrow."

## Pair 6.5 - Translate email verified

Runtime A:
R5 says: token accepted and consumed, account activated, questionnaire path may begin.

Expected B:
"Your email is verified and your account is active. We can now start your setup
questionnaire."

Alternate A:
R5 says: valid link clicked and account activation succeeded.

Alternate B:
"Verification complete. Your account is ready."

## Pair 6.6 - Translate questionnaire delivered

Runtime A:
R6 says: questionnaire generated with tenant context and delivered through chat.

Expected B:
"We sent your first setup questionnaire in chat. Your answers help us show you more
relevant business connections."

Alternate A:
R6 says: setup questions are available in the chat channel.

Alternate B:
"Your setup questions are waiting in chat."

## Pair 6.7 - Translate onboarding complete

Runtime A:
R7 says: onboarding marked complete and personalization events may start.

Expected B:
"You are all set. We can now personalize your matches, feed, and dashboard."

Alternate A:
R7 says: profile, questionnaire, and business details are complete.

Alternate B:
"Onboarding is complete. The platform can start tailoring your experience."

## Pair 6.8 - Translate expired link

Runtime A:
R_expired says: verification rejected because token expired; offer resend path.

Expected B:
"This verification link has expired. Ask for a new link and we will send another one."

Alternate A:
R_expired says: twenty-four hour verification window passed.

Alternate B:
"That link is no longer valid. You can request a fresh verification email."

## Pair 6.9 - Translate rate limit

Runtime A:
R8 says: rate limit exceeded, no account changed, wait before retrying.

Expected B:
"Too many sign-up attempts. Please wait fifteen minutes before trying again."

Alternate A:
R8 says: registration blocked before work began.

Alternate B:
"Please pause before trying again. We temporarily blocked registration attempts from this
source."

## Pair 6.10 - Translate recoverable questionnaire failure

Runtime A:
R10 says: account active, questionnaire not delivered yet, work preserved for retry, onboarding
not complete.

Expected B:
"Your account is ready. Your setup questionnaire will arrive shortly."

Alternate A:
R10 says: questionnaire service failed but the user account is usable.

Alternate B:
"You can start using your account now. We will send the setup questions as soon as they are
available."

## Pair 6.11 - Translate provider outage

Runtime A:
R9 says: Google sign-up temporarily disabled; other providers and email remain available.

Expected B:
"Google sign-up is temporarily unavailable. You can continue with another provider or sign
up by email."

Alternate A:
R9 says: one provider down but other registration paths open.

Alternate B:
"That sign-up option is unavailable right now, but the other sign-up options still work."

## Pair 6.12 - Consistency rejection

Runtime A:
All results for one run include: R1 says "registration method: Google SSO" and R3 says
"email verification required before activation."

Expected B:
Do not translate as final user language. Flag the run as internally inconsistent. One
single registration session cannot both be an already-active SSO registration and an
email-pending registration unless the plan explicitly describes a merge or link flow.
Send the inconsistent fragments to arbitration and reprocessing.

Alternate A:
Results disagree about whether the account is SSO-active or email-pending.

Alternate B:
Stop translation for this run. Ask for reprocessing of the branch decision.

---

