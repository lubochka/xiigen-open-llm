# Next Action Items For Multi-Phase Orchestration
# Date: 2026-05-04

## Immediate Next Action

Start the Phase 3 -> Phase 4 transition experiment Round A.

This is not a learning-set-writing round. It is an oracle-writing round.
The goal is to prove that a technical-design context packet can be carried
into an implementation source packet without losing constraints, knowledge
edges, or rejected decisions.

## Round A Scope

Create the five oracle documents:

1. `T34-TRANSITION-DOMAIN-MAP-v1.md`
2. `T34-TRANSITION-FAILURE-TAXONOMY-v1.md`
3. `T34-TRANSITION-EXPECTED-TRACE-v1.md`
4. `T34-TRANSITION-HUMAN-RESULT-STANDARDS-v1.md`
5. `T34-TRANSITION-EXECUTOR-CONTRACT-v1.md`

No code changes.
No learning pairs.
No broad corpus work.

## Required Source Binding Before Writing The Oracle

The executor must manually select:

- One upstream Phase 3 technical-design source authority.
- One downstream Phase 4 implementation source authority.

The automated registry proposes candidates, but it is not allowed to make the
final authority decision. The final binding must be a human-readable statement:

`Upstream source X is Phase 3 because it resolves [problem]. Downstream source Y
is Phase 4 because it resolves [problem]. The transition tests whether decisions
from X are preserved, acknowledged, or correctly marked out of scope in Y.`

## Round A Pass Gates

The Round A oracle passes review only if:

- The upstream phase context packet contains at least three entries across
  decisions made, constraints inherited, knowledge edges, boundary markers,
  and rejected items.
- The expected trace tests all four transition-gate checks: constraint
  preservation, decision acknowledgment, knowledge-edge continuity, and
  rejection preservation.
- At least one scenario requires semantic judgment rather than keyword
  matching.
- The human result standard states what a technical lead can decide after
  reading the transition report.
- The executor contract says explicitly that learning-set construction starts
  only after Round B evidence exists.

## Corpus Work That Should Wait

Do not build broad learning sets for:

- Integration tests.
- Security and compliance review.
- Test scenario automation.
- Regression-suite maintenance.

Those sub-phases are source-authority gaps in the audit evidence. They should
be handled as knowledge edges until specific source documents are written or
selected.

## Source Documents To Add Next

To remove the strongest knowledge edges, produce or locate these source
documents:

1. `INTEGRATION-TEST-SOURCE-AUTHORITY-v1.md`
   - Defines integration-test scope, real dependencies, pass gates, and known
     failure modes.
2. `SECURITY-COMPLIANCE-REVIEW-SOURCE-AUTHORITY-v1.md`
   - Maps authorization, sensitive data, audit, and compliance checks to QA
     verdicts.
3. `QA-AUTOMATION-SCENARIO-SOURCE-AUTHORITY-v1.md`
   - Defines how manual QA scenarios become automated checks.
4. `REGRESSION-SUITE-MAINTENANCE-SOURCE-AUTHORITY-v1.md`
   - Defines when regression tests are added, retired, owned, and reported.
5. `ACCESSIBILITY-AUDIT-SOURCE-AUTHORITY-v1.md`
   - Strengthens the current thin accessibility-audit boundary.

## Learning-Set Rule

Learning pairs are produced only after an experiment passes or produces
classified gaps.

For every transition experiment:

1. Corrections first.
2. Gap closure pairs second.
3. Coverage extension pairs third.

The B output must always be the actual output the node should produce, not a
description of that output.

