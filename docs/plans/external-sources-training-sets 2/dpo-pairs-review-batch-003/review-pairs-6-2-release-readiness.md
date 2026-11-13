# Review Pairs — Sub-phase 6.2 Release Readiness and Risk Review
# Batch: dpo-pairs-review-batch-003
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: release readiness, risk gates, operational readiness, release decision evidence
# Pair count: 4

---

## Pair AP3-6.2-001 — release readiness review confused with CI gate configuration

```
source_id: xiigen-review-batch-003-AP3-6.2-001
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.2
rejected_sub_phase: 7.2
```

Prompt: `"define the release-readiness review for the new payment provider integration before we ship to production"`

```
CHOSEN — 6.2 Release Readiness and Risk Review:
Review checklist with go/no-go decision criteria. Each item names what evidence
is required and what blocks release.

Provider readiness:
  Production credentials are in the secrets vault, not in any source file.
  Vault access is granted to the on-call rotation only.
  Block release if any credential is in plaintext anywhere or if access is
  open beyond the rotation.

Operational readiness:
  Runbook exists for the three highest-likelihood incidents (provider outage,
  webhook signature mismatch, transaction reconciliation failure). Each
  runbook has been walked through by an on-call engineer in the last 30 days.
  On-call rotation has been notified of the new integration.
  Block release if no walkthrough log exists or if on-call has not been
  notified.

Rollback readiness:
  Rollback procedure is documented and the previous release artifact is
  available in the registry. The procedure has been rehearsed in staging
  by running the rollback script against a staging deployment.
  Block release if rehearsal log is missing.

Compliance evidence:
  PCI DSS scope review confirms the new integration does not expand the
  cardholder data environment. Sign-off from compliance lead is in the
  release artifact.
  Block release if compliance sign-off is missing.

Risk register:
  Three named risks documented with mitigation: provider outage (mitigation:
  fall back to secondary provider after 30 seconds), reconciliation drift
  (mitigation: nightly reconciliation report with auto-page on > 0.1%
  drift), key rotation failure (mitigation: monitored expiry alert at
  30/14/7 days).
  Block release if any risk lacks an owner.

Release decision artifact:
  Reviewer documents which checks passed, which were waived (with
  justification and approver), and the final go/no-go decision. The
  artifact is attached to the release ticket.

REJECTED — 7.2 CI Pipeline and Quality Gates:
Add a release-gate stage to the CI pipeline. The stage checks: all unit
tests pass; all integration tests pass against the staging environment;
SonarCloud quality score is above 8.0; no high-severity security findings
in the latest scan. Block the deploy job if any check fails. Configure
the pipeline to require manual approval from a release manager before
running the production deploy job.
[WRONG: release-readiness review is the human go/no-go decision over
operational readiness, rollback rehearsal, compliance evidence, and
risk-register completeness — none of which are CI checks. CI gates verify
that the build is mechanically sound; they cannot verify that the on-call
team has been notified, that the rollback rehearsal happened, or that the
compliance sign-off has been obtained. The CI gate is supporting evidence
for the release-readiness decision; it is not the decision itself.]
```

---

## Pair AP3-6.2-002 — release-readiness review confused with infrastructure deployment

```
source_id: xiigen-review-batch-003-AP3-6.2-002
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.2
rejected_sub_phase: 4.3
```

Prompt: `"the new email provider integration is ready to deploy — what does the release-readiness review check?"`

```
CHOSEN — 6.2 Release Readiness and Risk Review:
Review focuses on the release decision, not the deployment mechanics.

Operational evidence:
  The provider's status page is monitored by the operations team and
  alerts are routed to on-call.
  Provider rate limits are documented and our peak send rate is below
  20% of the limit.
  Bounce-handling and unsubscribe-handling are wired and verified in
  staging — the reviewer reads the wiring code and the verification log.
  Block release if any of these is missing.

Compliance and policy:
  The new provider's data processing addendum (DPA) has been signed and
  is stored in the legal repository.
  GDPR-relevant data flows have been documented (which fields leave our
  systems, where they are stored, retention period).
  Block release if DPA is missing or DPO has not signed off.

Reversibility:
  A rollback to the previous provider is documented: change a feature flag
  + redeploy worker pool. The rollback has been rehearsed within 7 days
  of the planned release.
  Block release if rehearsal is older than 7 days.

Communication:
  The launch is announced to the customer success team 72 hours before
  release with the expected behaviour change (deliverability changes,
  bounce-message format changes).
  Block release if announcement is not in place.

Decision artifact:
  Reviewer documents the go/no-go and the conditions under which a
  rollback would be triggered (e.g. bounce rate > 2x baseline for 1 hour
  post-launch).

REJECTED — 4.3 Infrastructure and Deployment Code:
Update the Terraform module for the email-worker-pool to use the new
provider's connection settings. Add the new provider's API endpoint
to the egress allow-list in the network policy. Update the Helm
chart's values.yaml with the new provider name and credential
references. Add a deployment-time health check that pings the new
provider and fails the deploy if unreachable. Roll the change through
the staging cluster first, then production.
[WRONG: release-readiness review checks the operational, compliance, and
risk dimensions before a deploy is approved — it does not produce
infrastructure code. The Terraform and Helm changes are deployment
implementation that the readiness review approves or blocks. Treating
"release-readiness" as a deployment task collapses the review activity
into the implementation it is meant to gate.]
```

---

## Pair AP3-6.2-003 — release decision confused with manual QA acceptance

```
source_id: xiigen-review-batch-003-AP3-6.2-003
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.2
rejected_sub_phase: 6.1
```

Prompt: `"we have a release candidate — what risks does the release-readiness review evaluate before we promote to production?"`

```
CHOSEN — 6.2 Release Readiness and Risk Review:
Risk evaluation focuses on what could go wrong after release and what is
in place to detect, mitigate, and reverse the issue. Not feature acceptance.

Risks evaluated and mitigation evidence:

Performance risk:
  Risk: the new release introduces a slow query path that degrades p95
  latency under production load.
  Mitigation evidence required: load test against a production-like
  dataset showing p95 within budget; canary deployment plan that
  inspects p95 at 1% rollout before promoting.
  Block if either is missing.

Data risk:
  Risk: a migration in this release touches a table with > 1M rows and
  could lock during deployment.
  Mitigation evidence required: migration plan documented as expand-
  contract or online-schema-change; rehearsal log against a staging
  dataset of comparable size.
  Block if migration runs DROP COLUMN or ALTER on a hot table without
  evidence of online-safe execution.

Security risk:
  Risk: this release adds a new public endpoint that has not been on the
  external surface before.
  Mitigation evidence required: WAF rule update plan; rate-limit rule
  for the new endpoint; security scan against the new endpoint.
  Block if any of these is missing.

Customer impact risk:
  Risk: this release changes a public API contract.
  Mitigation evidence required: deprecation policy applied (the change
  has been announced 90 days ago and is now in the supported deprecation
  window).
  Block if announcement is missing or the window has not elapsed.

Decision:
  Each risk gets a residual-risk rating (low / medium / high) after
  mitigations. A high residual risk requires explicit sign-off from the
  director of engineering. Two or more high residual risks block the
  release.

REJECTED — 6.1 Manual QA and Acceptance Review:
The QA reviewer walks through the new features in the release candidate
on the staging environment. For each feature, the reviewer follows the
acceptance criteria from the user story: try the happy path, try the
documented edge cases, try a representative invalid input. The reviewer
documents pass/fail for each criterion and signs off on the release if
all features pass.
[WRONG: release-readiness review is the risk evaluation before promotion —
performance, data, security, customer impact — and the mitigation evidence
required to accept those risks. Manual QA acceptance is feature-level
acceptance, a different sub-phase (6.1) that runs earlier and answers a
different question. Manual acceptance can pass while release readiness
fails (e.g. all features work but the database migration plan is unsafe).
The two sub-phases are sequential and cover different concerns.]
```

---

## Pair AP3-6.2-004 — risk-gate review confused with monitoring setup

```
source_id: xiigen-review-batch-003-AP3-6.2-004
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.2
rejected_sub_phase: 7.3
```

Prompt: `"we are about to launch a 50% traffic rollout — what does the release-readiness reviewer check before the rollout begins?"`

```
CHOSEN — 6.2 Release Readiness and Risk Review:
Pre-rollout review with rollout-specific risks.

Pre-rollout state:
  The 1% canary has been running for at least 48 hours.
  Canary error rate is within 10% of baseline; canary p95 latency is
  within 10% of baseline. Reviewer reads the canary report.
  Block 50% rollout if either threshold is exceeded.

Rollout plan evidence:
  The 50% increment is documented in the rollout plan. The plan names
  the metric thresholds that would trigger automatic rollback during
  the 50% phase.
  Block if the plan is missing or the rollback thresholds are softer
  than the canary thresholds.

Capacity verification:
  The increase from 1% to 50% means a 50x traffic increase. Reviewer
  confirms capacity headroom for the worker pool, the database
  connection pool, and the cache cluster.
  Block if the database connection pool is sized below 50%-traffic
  expectation or if cache cluster has no headroom.

Customer-visible consequence assessment:
  Identify which customer cohorts are in the 50% slice. If specific
  high-value tenants are included, confirm the launch announcement
  reached them.
  Block if a high-value tenant is in the 50% slice but did not receive
  the heads-up.

Decision artifact:
  Reviewer documents the canary-stage outcomes, the capacity
  verification, the cohort affected, and the go/no-go for 50%.

REJECTED — 7.3 Monitoring and Feedback Loop:
Configure the dashboard to show the new release-version label across all
metrics. Add a panel for canary-vs-baseline comparison. Set up an alert
that pages on-call when canary error rate is more than 10% above baseline
or canary latency is more than 10% above baseline. Document the monitoring
setup in the team wiki.
[WRONG: release-readiness review is the human go/no-go decision based on
canary outcomes, capacity verification, and cohort analysis. Monitoring
setup configures the tooling that reviewers consume — but the review is
the decision, not the configuration. The review reads the canary report
that the monitoring produces; it does not produce the monitoring. The two
activities are different sub-phases ordered such that monitoring is in
place first, and the readiness review uses its output to decide.]
```
