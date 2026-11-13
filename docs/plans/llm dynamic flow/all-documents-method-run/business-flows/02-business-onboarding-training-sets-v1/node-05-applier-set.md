# Node-05 - Applier Set

## Purpose

Runtime A is one processed piece plus its transformation prompt. Runtime B is the
concrete result Rqi.

## Pair 5.1 - Apply profile creation

A: Processed profile result says required fields are present. Prompt says store profile before publishing BusinessProfileCreated.

B: R1: BusinessProfile created for the user. Profile contains industry, stage, team size, goals, challenges, and available business details. BusinessProfileCreated published after the profile exists.

Pair 5.1 - Phrasing B

A: Profile branch is ready to write.

B: R1: The business profile now exists and downstream services may use it.

## Pair 5.2 - Apply analytics categorization

A: Processed analytics result says user input is primary and model output is secondary.

B: R2: UserProfileAnalyzed produced profile segments and behavior patterns. BusinessCategorized produced categories using user-reported industry as primary evidence. Strong disagreement was marked for review without blocking onboarding.

Pair 5.2 - Phrasing B

A: Analytics found a category conflict.

B: R2: Analytics continued with the user's own category and recorded the conflict for review.

## Pair 5.3 - Apply learning program

A: Processed learning result says unavailable module must be replaced.

B: R3: Learning program generated after replacing unavailable content. Program includes modules, duration, difficulty, skills, and learning path. LearningProgramGenerated published. Learning preferences stored for feed personalization.

Pair 5.3 - Phrasing B

A: Curriculum was corrected before publishing.

B: R3: The user has a valid personalized learning program with no unavailable modules.

## Pair 5.4 - Apply matching

A: Processed matching result says score is 0.725 and candidate is eligible.

B: R4: Candidate business added to ranked matches with score 0.725. Match type selected from peer, mentor, complementary, or collaborative based on evidence. Match reason written in privacy-safe language. BusinessMatchesFound produced when ranked list is ready.

Pair 5.4 - Phrasing B

A: Candidate match score is ready.

B: R4: The candidate appears in match suggestions with a safe reason.

## Pair 5.5 - Apply matching timeout

A: Processed result says matching exceeded thirty seconds and partial results exist.

B: R5: Partial match suggestions returned. User-facing state is "More matches coming soon." Remaining matching work continues asynchronously. Onboarding is not failed.

Pair 5.5 - Phrasing B

A: Matching timed out but has some results.

B: R5: Show available matches now and continue looking in the background.

## Pair 5.6 - Apply feed personalization

A: Processed feed result says feed mix and one-hour cache are ready.

B: R6: UserFeedPersonalized created with 40 percent matched business content, 25 percent industry trending content, 20 percent learning-related content, and 15 percent platform trending content. Feed cache set to one hour.

Pair 5.6 - Phrasing B

A: Feed branch has matches, learning, and categories.

B: R6: Personalized feed configuration is ready for the user interface.

## Pair 5.7 - Apply event recommendations

A: Processed event result says events should use profile, learning path, and matches.

B: R7: EventFeedPersonalized created with event recommendations relevant to the user's industry, stage, interests, and learning path. Event recommendation cache set to four hours.

Pair 5.7 - Phrasing B

A: Events branch has personalization inputs.

B: R7: The user has a personalized events feed.

## Pair 5.8 - Apply opt-out

A: Processed result says the user opted out of matching.

B: R8: Matching suggestions were not created. Feed and event personalization continued using profile, learning, and category signals only. No match results were exposed.

Pair 5.8 - Phrasing B

A: User declined matching.

B: R8: Respect opt-out and continue personalization without business match suggestions.
