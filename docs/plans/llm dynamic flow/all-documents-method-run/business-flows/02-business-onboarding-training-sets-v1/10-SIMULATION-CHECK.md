# 10 - Simulation Check

## Simulation 1 - Full onboarding personalization

Input prompt:
"After the questionnaire, build the user's business profile, find matches, personalize feeds and events, and prepare a learning path."

Expected path:

- Node-01: [business profile construction, matching, feed personalization, event recommendations, learning personalization]
- Node-02: profile wording, matching wording, feed wording, event wording, learning wording
- Node-03: start from QuestionnaireCompleted, create profile, generate learning, run matching, personalize feeds and events, notify completion
- Node-04: process profile fields, learning content, match scoring, feed configuration
- Node-05: produce R1 profile, R3 learning, R4 matches, R6 feed, R7 event feed
- Node-06: translate into profile ready, learning ready, matches ready, feed ready, events ready
- Node-07: select privacy, matching integrity, personalization, event sequence, completion
- Node-08: pass only if privacy, scoring, and completion rules hold
- Node-09: one user answer saying the personalized experience is ready

Missing-if-fail:
If the simulation runs matching before profile creation, Node-03 is wrong.
If the output reveals private match details, Node-08 is wrong.

## Simulation 2 - Matching timeout

Input prompt:
"Matching takes longer than thirty seconds. What should the user see?"

Expected path:

- Node-01: [operations and resilience, matching, notifications and user experience]
- Node-02: timeout and graceful-degradation wording
- Node-03: detect timeout, return partial or delayed state, preserve continuation
- Node-04: process timeout as partial result or check-back-soon state
- Node-05: R5 partial matches or delayed matching
- Node-06: "We found some matches. More suggestions may appear soon."
- Node-07: select resilience, user experience, completion
- Node-08: flag if onboarding hard-fails or completion lies
- Node-09: calm user-facing answer

Missing-if-fail:
If the user sees an internal timeout error, Node-06 or Node-09 is wrong.

## Simulation 3 - User opted out of matching

Input prompt:
"The user does not want business matching but still wants feed, event, and learning personalization."

Expected path:

- Node-01: [privacy and access control, matching, feed personalization, event recommendations, learning personalization]
- Node-02: opt-out wording and non-matching personalization wording
- Node-03: skip matching branch, continue other personalization branches
- Node-04: process matching skipped by user choice
- Node-05: R8 no match suggestions exposed
- Node-06: "Matching is off, but other personalization continues."
- Node-07: select consent, formatter consistency, privacy
- Node-08: flag if match suggestions appear
- Node-09: final answer does not mention matches as available

Missing-if-fail:
If the simulation creates match suggestions, Node-08 must be strengthened.

## Simulation 4 - Analytics conflict

Input prompt:
"The user says Healthcare, but analytics categorizes the business as Tech."

Expected path:

- Node-01: [analytics and categorization, privacy and access control]
- Node-02: user input primary, model output secondary
- Node-03: preserve self-report, mark divergence for review
- Node-04: process conflict without blocking onboarding
- Node-05: R2 analyzed with user category primary
- Node-06: "Your category is based primarily on what you told us."
- Node-07: select categorization, review-routing, privacy
- Node-08: flag if model output replaces user input
- Node-09: reviewer-oriented answer or calm user answer

Missing-if-fail:
If the model overwrites the user answer, Node-08 is wrong.

## Self-check Table

| Node | Runtime pairs | Required shape |
|---|---:|---|
| Node-01 Domain splitter | 10 | Human prompt -> domain labels |
| Node-02 Domain translator | 10 | Fragment plus domain -> formal wording |
| Node-03 Scheme planner | 8 | Domain wordings -> b1...bn and fb1...fbn |
| Node-04 Domain processor array | 8 | One planned piece -> processed result |
| Node-05 Applier array | 8 | Processed result -> concrete Rqi |
| Node-06 Result translator array | 9 | Formal result -> user language or consistency flag |
| Node-07 Arbiter selector | 8 | Result group -> selected arbiters |
| Node-08 Arbiter units | 8 | Review -> pass or flag with fbx |
| Node-09 Formatter | 8 | Approved fragments -> one final answer |

Total runtime-shaped pairs: 77.
