# XIIGen Session Export 1000 Oldest-First Learning Set

Generated at: 2026-05-06T11:22:15.2678041+00:00
Source family: `xiigen-session-export`
Source license state: `first_party_private`
Extraction mode: streaming JSON from zip entry; no full `conversations.json` load.

## Source Zips
- `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip`
- `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178382-78288604-batch-0000.zip`

## Counts
- Rows produced: 1000
- Conversations used: 457
- Layer 0-A compiler files: 20
- Layer 0-B router files: 20
- Held-out rephrase seeds: 100

## Date Window
- First row message: `2025-09-03T10:43:59.809174Z`
- Last row message: `2026-03-11T11:01:47.894878Z`

## Domain Distribution
- ARCH: 170
- BUG: 266
- CODE_Q: 339
- PLAN: 213
- REFACTOR: 12

## Sub-Phase Distribution
- 1.3 Interaction and Navigation Model: 24
- 2.1 Business Capability Definition: 28
- 3.1 Architecture and Component Topology: 90
- 3.2 Data Model and Persistence Design: 23
- 3.3 API Contract Definition: 186
- 3.4 Security and Authorization Model: 60
- 3.5 Observability and Operational Design: 30
- 4.1 Backend Services: 121
- 4.2 Frontend and Client Integration: 78
- 4.3 Infrastructure and Deployment Code: 9
- 5.1 Unit Test Design: 21
- 6.1 Manual QA and Acceptance Review: 3
- 6.4 Performance and Reliability Review: 1
- 7.2 CI Pipeline and Quality Gates: 3
- 7.4 Coverage and Reporting: 2
- 7.5 Learning Set and Model Feedback: 321

## Skipped Signals
- conversation_not_relevant: 26
- duplicate_prompt: 483
- no_engine_predicate: 839
- not_enough_text: 1
- too_long_for_first_pass: 61
- too_short: 190
- tool_output_as_prompt: 3

## Acceptance Checks
- [x] Uses real human messages as A-sides.
- [x] Excludes setup-only/session-control messages.
- [x] Preserves conversation UUID and human message UUID per row.
- [x] Uses current engine topology sub-phase IDs.
- [x] Produces Layer 0-A and Layer 0-B files for immediate teaching-run compatibility.
