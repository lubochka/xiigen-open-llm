# XIIGen Session Export All Eligible Current-Filter Learning Set

Generated at: 2026-05-06T11:28:58.4601374+00:00
Source family: `xiigen-session-export`
Source license state: `first_party_private`
Extraction mode: streaming JSON from zip entry; no full `conversations.json` load.

## Source Zips
- `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip`
- `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178382-78288604-batch-0000.zip`

## Counts
- Rows produced: 2792
- Conversations used: 601
- Layer 0-A compiler files: 56
- Layer 0-B router files: 56
- Held-out rephrase seeds: 279

## Date Window
- First row message: `2025-09-03T10:43:59.809174Z`
- Last row message: `2026-04-24T08:01:10.522913Z`

## Domain Distribution
- ARCH: 315
- BUG: 998
- CODE_Q: 714
- PLAN: 721
- REFACTOR: 44

## Sub-Phase Distribution
- 1.3 Interaction and Navigation Model: 45
- 2.1 Business Capability Definition: 104
- 2.4 Edge Cases and Boundary Conditions: 9
- 3.1 Architecture and Component Topology: 192
- 3.2 Data Model and Persistence Design: 30
- 3.3 API Contract Definition: 272
- 3.4 Security and Authorization Model: 271
- 3.5 Observability and Operational Design: 61
- 4.1 Backend Services: 438
- 4.2 Frontend and Client Integration: 227
- 4.3 Infrastructure and Deployment Code: 32
- 4.4 Refactoring and Code Quality: 3
- 5.1 Unit Test Design: 79
- 6.1 Manual QA and Acceptance Review: 10
- 6.2 Release Readiness and Risk Review: 6
- 6.4 Performance and Reliability Review: 3
- 7.2 CI Pipeline and Quality Gates: 3
- 7.3 Monitoring and Feedback Loop: 2
- 7.4 Coverage and Reporting: 4
- 7.5 Learning Set and Model Feedback: 1001

## Skipped Signals
- conversation_not_relevant: 41
- duplicate_prompt: 2267
- no_engine_predicate: 1787
- not_enough_text: 5
- too_long_for_first_pass: 111
- too_short: 1108
- tool_output_as_prompt: 79

## Acceptance Checks
- [x] Uses real human messages as A-sides.
- [x] Excludes setup-only/session-control messages.
- [x] Preserves conversation UUID and human message UUID per row.
- [x] Uses current engine topology sub-phase IDs.
- [x] Produces Layer 0-A and Layer 0-B files for immediate teaching-run compatibility.
