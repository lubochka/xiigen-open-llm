# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one arbiter specialization plus one result or result group. Runtime
B is pass, concern, or fbx reprocess instruction.

## Pair 8.1 - Privacy arbiter: unsafe match reason

A: Arbiter: privacy arbiter. Result: match reason says "This business has low revenue like yours."

B: Flag it. Match reasons must not reveal private business details. Revenue range is classified as confidential business detail in this document.
fbx: Reprocess match reason. Replace private financial details with a safe reason such as shared industry stage or overlapping goals.

Pair 8.1 - Phrasing B

A: Match explanation exposes another business's confidential data.

B: Flag. Send the match reason back for privacy-safe rewriting.

## Pair 8.2 - Matching integrity arbiter: wrong weights

A: Arbiter: matching integrity arbiter. Result: score uses industry 0.50, location 0.10, stage 0.20, interest 0.20.

B: Flag it. The document defines industry 0.30, location 0.25, business stage 0.25, and interest overlap 0.20.
fbx: Reprocess compatibility scoring with the documented weights.

Pair 8.2 - Phrasing B

A: Matching score uses a different formula.

B: Flag. Correct the score formula before publishing matches.

## Pair 8.3 - Content availability arbiter

A: Arbiter: content availability arbiter. Result: learning program references a restricted module.

B: Flag it. The document says the Learning Service validates content availability before publishing the program and replaces unavailable modules.
fbx: Reprocess learning program generation. Replace restricted or deleted content before publishing.

Pair 8.3 - Phrasing B

A: Curriculum includes content the user cannot access.

B: Flag. Replace unavailable learning content.

## Pair 8.4 - Cache arbiter

A: Arbiter: cache arbiter. Result: business matches cached for seven days.

B: Flag it. Business matches have a twelve-hour cache lifetime in the document. Seven days is not the documented lifetime.
fbx: Reprocess match caching. Set business matches cache to twelve hours.

Pair 8.4 - Phrasing B

A: Match cache lifetime is too long.

B: Flag. Use the twelve-hour match cache rule.

## Pair 8.5 - Consent arbiter

A: Arbiter: consent arbiter. Result: user opted out of matching, but match suggestions were still shown.

B: Flag it. The document says users can opt out of matching. Showing match suggestions violates that choice.
fbx: Reprocess matching branch. Skip match suggestions for this user and remove matching language from the final output.

Pair 8.5 - Phrasing B

A: Matching output exists even though the user opted out.

B: Flag. Respect opt-out and reprocess without match suggestions.

## Pair 8.6 - Completion arbiter

A: Arbiter: completion arbiter. Result: OnboardingCompleted published before feed and event personalization finished.

B: Flag it. The flow completes with OnboardingCompleted after the personalized experience is ready. Publishing completion before required visible personalization is misleading.
fbx: Reprocess onboarding completion. Wait for required feed and event outputs or documented degraded states before completion.

Pair 8.6 - Phrasing B

A: Completion was announced too early.

B: Flag. Delay completion until required personalization or valid fallback exists.

## Pair 8.7 - Categorization arbiter

A: Arbiter: categorization arbiter. Result: model category replaced the user's self-reported industry without review.

B: Flag it. The document says when analytics conflicts with self-report, trust user input as primary and use machine categorization as secondary.
fbx: Reprocess categorization. Restore user-reported industry as primary and mark strong divergence for review.

Pair 8.7 - Phrasing B

A: ML output overrode user input.

B: Flag. User input must remain primary.

## Pair 8.8 - Pass case

A: Arbiters: privacy, matching integrity, resilience. Result: match weights match the document, match reasons avoid private details, timeout returns partial results, and final output says more matches may arrive soon.

B: Pass.

Pair 8.8 - Phrasing B

A: Reviewed result follows scoring, privacy, and graceful timeout rules.

B: Pass.
