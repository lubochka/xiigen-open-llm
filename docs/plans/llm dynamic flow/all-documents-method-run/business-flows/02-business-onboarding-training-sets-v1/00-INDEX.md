# 00 - Index

## Source Document

Source path:
`C:\Users\lubao\Documents\xiigen\business flows\02-business-onboarding.md`

Chosen because it is the primary business-flow specification for the business
onboarding flow. Deep-search files and engine files are related documents, but
they are not mixed into this round.

Source facts:

- Title: Business Onboarding & Personalization
- Flow ID: the business onboarding flow
- Version: 1.0
- Last updated: 2026-02-25
- Lines: 273
- Bytes: 22,760
- SHA256: 19B202C87CCB758E380465764072C9158270842B29D876A06A6431294AF405EA

First meaningful line:
`# Business Onboarding & Personalization`

Last meaningful line:
`* **Scale Strategy**: Per the documentation, the **Analytics** and **Learning** services should be scaled on **CPU (ML inference)**, while the **Feed** and **Matching** services should prioritize **Memory (Redis cache)** to handle high-frequency personalization requests.`

## Version Evidence

Copies found:

- `C:\Users\lubao\Documents\xiigen\business flows\02-business-onboarding.md`
- `C:\Users\lubao\Documents\xiigen\business-flows\business flows\02-business-onboarding.md`
- `C:\Users\lubao\Documents\xiigen\old\02-business-onboarding.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\business flows\02-business-onboarding.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\business-flows\business flows\02-business-onboarding.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\old\02-business-onboarding.md`

Current hash group:

- Hash prefix: 19B202C87CCB758E
- Lines: 273
- Bytes: 22,760
- Copies: current `business flows`, current `business-flows` mirror, root `old`

Older worktree hash group:

- Hash prefix: 41EACDA6B2664D82
- Lines: 273
- Bytes: 22,524
- Copies: `.claude\worktrees\adoring-davinci-058d7f`

Exact conceptual differences from older worktree version:

- `FLOW-02` was renamed to `the business onboarding flow`.
- `FLOW-01` dependency references were rephrased as `the user registration flow`.
- `FLOW-03`, `FLOW-04`, and `FLOW-07` dependency references were rephrased as named flows.
- `PII` was rephrased as `personal data`.
- `GDPR` was rephrased as `data protection law`.
- `API Gateway` was rephrased as `the programming interface Gateway`.

No creation session was confirmed. `business flows\umls.md` is candidate history
evidence because it contains the early business onboarding event sequence and
mentions `QuestionnaireCompleted`, `BusinessProfileCreated`, `UserProfileAnalyzed`,
and related flow material.

## Domains Defined By This Document

- business profile construction
- analytics and categorization
- learning personalization
- matching
- feed personalization
- event recommendations
- notifications and user experience
- privacy and access control
- operations and resilience
- product metrics

## Package Status

This package follows `document-examination-method-v2.md`. The source document is
used as evidence. Runtime training inputs are written as the things each node
actually receives.
