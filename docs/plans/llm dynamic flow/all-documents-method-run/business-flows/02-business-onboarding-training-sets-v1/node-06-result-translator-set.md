# Node-06 - Result Translator Set

## Purpose

Runtime A is one formal result. Runtime B is natural language for the user or
stakeholder. The translator also sees all results for consistency.

## Pair 6.1 - Profile ready

A: R1 says BusinessProfile exists and BusinessProfileCreated was published.

B: "Your business profile is ready. We can now use it to personalize your matches, feed, events, and learning path."

Pair 6.1 - Phrasing B

A: R1 says profile creation completed.

B: "We turned your answers into a business profile for personalization."

## Pair 6.2 - Analytics ready

A: R2 says profile segments and categories were produced, with user input used as primary evidence.

B: "We analyzed your business details and used your own answers as the main source for your category."

Pair 6.2 - Phrasing B

A: R2 says categorization completed with user answer trusted first.

B: "Your business category is based primarily on what you told us."

## Pair 6.3 - Learning program ready

A: R3 says a valid learning program was generated and unavailable content was replaced.

B: "Your personalized learning program is ready. It includes available content matched to your goals and challenges."

Pair 6.3 - Phrasing B

A: R3 says curriculum is valid.

B: "We created a learning path for you using content you can actually access."

## Pair 6.4 - Matches ready

A: R4 says ranked matches were produced with scores and safe reasons.

B: "We found business matches for you and included reasons to help you decide who to connect with."

Pair 6.4 - Phrasing B

A: R4 says match suggestions are ready.

B: "Your connection suggestions are ready."

## Pair 6.5 - Partial matches

A: R5 says matching timed out after thirty seconds but partial results were returned and more work continues.

B: "We found some matches for you. More suggestions may appear soon as matching continues."

Pair 6.5 - Phrasing B

A: R5 says matching is incomplete but usable.

B: "You can review the matches we found so far while we keep looking."

## Pair 6.6 - Feed ready

A: R6 says UserFeedPersonalized exists with configured content mix.

B: "Your feed has been personalized with business matches, industry content, learning material, and platform trends."

Pair 6.6 - Phrasing B

A: R6 says personalized content feed is ready.

B: "Your content feed is now tailored to your business."

## Pair 6.7 - Event feed ready

A: R7 says EventFeedPersonalized exists using profile, stage, interests, and learning path.

B: "Your events feed now shows events that better fit your industry, stage, interests, and learning path."

Pair 6.7 - Phrasing B

A: R7 says event recommendations are ready.

B: "We personalized your event recommendations."

## Pair 6.8 - Opt-out

A: R8 says matching skipped by user choice and no match results were exposed.

B: "You opted out of business matching. We will still personalize your feed, events, and learning path using your own profile."

Pair 6.8 - Phrasing B

A: R8 says no matching because user declined.

B: "Matching is off for your account, but the rest of personalization can continue."

## Pair 6.9 - Consistency flag

A: Result collection says matching suggestions were created, but another result says the user opted out of matching.

B: Do not translate into final user language. Flag inconsistency. The same flow run cannot both respect a matching opt-out and create match suggestions. Send the matching branch and opt-out branch for arbitration and reprocessing.

Pair 6.9 - Phrasing B

A: One result says no matching; another result shows matches.

B: Stop translation for this run and ask for reprocessing of matching consent.
