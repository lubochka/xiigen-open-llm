# Review Pairs — Sub-phase 6.3 Security and Compliance Review
# Batch: dpo-pairs-review-batch-003
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: implemented-security review, compliance evidence, vulnerability checks, control validation
# Pair count: 5
# Wave 2 priority

---

## Pair AP3-6.3-001 — implemented-security review confused with security model design

```
source_id: xiigen-review-batch-003-AP3-6.3-001
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.3
rejected_sub_phase: 3.4
```

Prompt: `"the auth implementation is complete — review whether what we built actually satisfies our security model"`

```
CHOSEN — 6.3 Security and Compliance Review:
Review compares the implemented system against the security model that
was decided in 3.4. The review is an audit, not a design.

Audit checklist:

Token issuance:
  Model says: 15-minute access token, 7-day refresh token rotated on each
  use, RS256 signature.
  Audit: read the token-issuer code. Confirm jose.SignJWT is called with
  the documented expiry values. Confirm the signing key is RS256 not HS256.
  Confirm the refresh-token rotation function exists and is invoked in
  the refresh path.
  Finding format: control name, expected, actual, status (pass / fail /
  partial), evidence (file path + commit hash).

Bypass paths:
  Model says: only /health, /metrics (internal), and /webhooks/stripe are
  unauthenticated. Every other endpoint requires a valid token.
  Audit: read the route registration files. Enumerate every route that
  is not behind the AuthMiddleware. Compare against the model's allow-list.
  Any extra route is a finding.

Tenant isolation:
  Model says: every authenticated request carries a tenant_id claim;
  every database query is scoped to that tenant; cross-tenant access
  attempts return 404 (not 403) to avoid tenant-existence enumeration.
  Audit: pick five representative endpoints. For each, attempt a cross-
  tenant access in the running staging environment. Record the response
  status and body.
  Finding: any endpoint that returns 403 or 500 instead of 404 is a
  partial pass (need fix).

Audit logging:
  Model says: every privilege-change action emits an audit log entry
  with actor, target, action, timestamp.
  Audit: trigger one of each privilege-change action in staging. Read
  the audit log file (not test fixtures). Confirm an entry exists with
  the documented fields.

Final disposition:
  Reviewer produces an audit report listing each control, the evidence
  observed, and the status. Findings of fail or partial block release.
  The report is signed by the reviewer with the commit hash audited.

REJECTED — 3.4 Security and Authorization Model:
Define the JWT structure: 15-minute access token, 7-day refresh with
rotation on each use, RS256 signature with key rotation every 90 days.
Define the route allow-list of unauthenticated endpoints (/health,
/metrics, /webhooks/stripe). Define the tenant isolation contract:
every authenticated request carries tenant_id; every database query
scopes to it; cross-tenant access returns 404. Define the audit-logging
contract: every privilege-change action emits {actor, target, action,
timestamp}. Document this as the security model for implementation.
[WRONG: "the auth implementation is complete — review whether what we
built actually satisfies our security model" asks for an audit of the
running system against the model. Defining the model is a different
sub-phase (3.4) that happens before implementation. After implementation,
the model is fixed and the question becomes: did we build it correctly?
That is the 6.3 audit. Returning to the design when an audit was asked
for skips the audit and may overwrite the original model with a slightly
different one that the implementation also fails to satisfy.]
```

---

## Pair AP3-6.3-002 — compliance evidence review confused with policy design

```
source_id: xiigen-review-batch-003-AP3-6.3-002
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.3
rejected_sub_phase: 3.4
```

Prompt: `"we have a SOC 2 audit next month — review the controls related to access management and confirm we have evidence the implementation satisfies them"`

```
CHOSEN — 6.3 Security and Compliance Review:
Evidence review for the access-management control family. Evidence is the
artifact the auditor can read; control definitions live elsewhere.

Per control, the reviewer collects evidence:

CC6.1 — logical access provisioning:
  Required evidence: a list of every account created in the last 90 days
  with the approver name and the role granted; the provisioning workflow
  log showing approval before creation.
  Reviewer pulls the report from the IAM system. Confirms every entry
  has an approver. Notes any account created without an approver as an
  exception that requires explanation.

CC6.2 — access removal on termination:
  Required evidence: a list of every employee terminated in the last 90
  days with the timestamp of access removal; SLA: access removed within
  24 hours of termination.
  Reviewer cross-references HR's termination log against the IAM
  removal log. Notes any case where removal exceeded 24 hours.

CC6.3 — periodic access review:
  Required evidence: documented quarterly review by application owners,
  with the list of users reviewed and any access changes made.
  Reviewer confirms the most recent review is within the last quarter.
  Notes if any production system has no review on file.

CC6.7 — restricted access to production:
  Required evidence: production access is granted only to engineers in
  the on-call rotation; access is logged and reviewed.
  Reviewer reads the IAM policy for the production environment. Compares
  against the on-call rotation roster. Confirms the access log exists
  and is reviewed monthly.

Finding disposition:
  Each control gets a status: evidence sufficient / evidence partial /
  evidence missing. Partial and missing are tracked as control gaps with
  remediation owners and target dates. The reviewer's report is the
  artifact the auditor reads.

REJECTED — 3.4 Security and Authorization Model:
Define the access-management policy. Specify that every new account
requires written approval from the manager and the application owner
before provisioning. Specify the access-removal SLA at 24 hours from
termination. Specify quarterly access reviews led by application owners.
Restrict production access to the on-call rotation. Document this in
the access-management-policy.md file.
[WRONG: the prompt says "we have a SOC 2 audit next month — review the
controls". Designing the policy is what already happened (or should have);
the audit reviews the evidence that what is documented is also what is
implemented. Defining a policy a month before the audit does not produce
evidence — auditors need ninety days of operational evidence, not a
policy written today. The 6.3 review collects what already exists; the
3.4 design produces what should exist before the operational period
begins.]
```

---

## Pair AP3-6.3-003 — vulnerability scan review confused with automated test design

```
source_id: xiigen-review-batch-003-AP3-6.3-003
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.3
rejected_sub_phase: 5.1
```

Prompt: `"the SAST and DAST scans completed for this release — review the findings and decide what blocks shipping"`

```
CHOSEN — 6.3 Security and Compliance Review:
Review the scan output and apply the team's risk-acceptance policy. Each
finding gets a disposition.

Finding triage:

For each high/critical finding, the reviewer:
  Reads the finding's location in the source code or runtime path.
  Determines whether the finding is exploitable (false positive, defense-
  in-depth, or real exposure).
  Cross-references against the existing accepted-risk list — known false
  positives that have been triaged previously.
  Documents the disposition: must-fix, accepted-risk (with justification
  + approver), false-positive (with explanation).

Specific finding categories:

SQL injection (DAST):
  Reviewer reads the request that triggered the finding. Confirms whether
  the parameter reaches a parameterised query (false positive) or is
  concatenated into raw SQL (must-fix).

XSS (DAST):
  Reviewer confirms whether the response is rendered as HTML (must-fix
  if user-controlled input is reflected) or as JSON (likely false
  positive in modern frontends).

Vulnerable dependency (SAST):
  Reviewer determines whether the affected code path is reachable in our
  use of the library. If the vulnerability is in a function we do not
  call, document as accepted-risk-not-reachable. Otherwise, must-fix.

Hardcoded secret (SAST):
  Reviewer reads the flagged file. Confirms whether the value is a real
  secret (must-fix immediately, rotate the secret, plus block release)
  or a placeholder/example (false positive).

Decision rule:
  Any unaddressed must-fix finding blocks the release. Accepted-risk and
  false-positive entries are added to the suppression file with the
  reviewer's name and the date.
  An accepted-risk decision over six months old must be re-reviewed
  before this release ships.

Sign-off:
  Reviewer signs the security review artifact attached to the release
  ticket.

REJECTED — 5.1 Unit Test Design:
Write unit tests that cover the security-relevant code paths flagged by
the SAST scan. For each finding, write a test that exercises the path
and asserts the expected secure behaviour (e.g. that a parameterised
query is used, that user input is HTML-escaped, that the affected
library function is not called). Run the tests in CI. The test suite
becomes the security regression coverage for the flagged areas.
[WRONG: the prompt asks for review of the scan findings and a release-
blocking decision. Writing unit tests is implementation work that may
follow the review (a must-fix finding can produce a regression test as
part of the fix). The review itself is the disposition decision —
which findings are real, which are accepted, and whether anything blocks
the release. The disposition cannot be replaced by writing tests; tests
do not exist for findings that are dispositioned as accepted-risk.]
```

---

## Pair AP3-6.3-004 — control validation review confused with security implementation

```
source_id: xiigen-review-batch-003-AP3-6.3-004
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.3
rejected_sub_phase: 3.4
```

Prompt: `"validate that our PCI DSS control for cardholder data masking is actually working in production"`

```
CHOSEN — 6.3 Security and Compliance Review:
Validation is observation of the running system, not redefinition of the
control.

What the reviewer does:

Read the control as documented:
  PCI DSS 3.4 — primary account number (PAN) must be unreadable when
  stored. Our control: PAN is never stored; only the last four digits
  and a tokenized reference are stored.

Validate at storage:
  Pull a sample of 50 rows from the orders table from the last 7 days.
  Confirm none of the rows contain a 16-digit PAN. Confirm only the
  pan_last_four field is populated and the pan_token field is a
  Stripe-issued reference.
  Any row with a PAN-like 16-digit number is a finding.

Validate in logs:
  Pull 1 hour of payment-service application logs. Grep for 16-digit
  number patterns. Any match is a finding.
  Pull 1 hour of API gateway access logs. Grep for credit-card patterns
  in URL parameters or request bodies. Any match is a finding.

Validate in traces:
  Pull a sample of 50 payment-related traces from the trace store.
  Confirm no span attribute or event message contains a PAN.

Validate in third-party signals:
  Pull the most recent week of error reports from the error monitoring
  service. Confirm no error message contains a PAN.

Findings disposition:
  A single occurrence in any data store, log, trace, or error report
  is a control failure. The reviewer documents the finding, the
  remediation owner, and a short-term (rotate the affected card) and
  long-term (fix the leaking code path) plan.

The reviewer's evidence file lists each validation step performed, the
sample size, the result, and the date.

REJECTED — 3.4 Security and Authorization Model:
Define the cardholder data handling model. PAN must not be stored. PAN
must not appear in logs, traces, or error reports. The tokenization
service issues a token reference per card; the application stores only
the last four digits and the token. The token-to-PAN mapping is held
exclusively by the tokenization provider. Document in the security model
file. Reference PCI DSS 3.4 in the model.
[WRONG: "validate that the control is actually working in production" is
a 6.3 review — observe the running system and confirm the model is
satisfied. Restating the model is what 3.4 produced earlier. Without the
observation step, the team has a documented intent and no evidence that
the intent is real. PCI auditors require evidence of operational
validation; restating the model does not produce evidence.]
```

---

## Pair AP3-6.3-005 — security review confused with CI security gate

```
source_id: xiigen-review-batch-003-AP3-6.3-005
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.3
rejected_sub_phase: 7.2
```

Prompt: `"the security team needs to validate the third-party-dependency posture for this release before sign-off"`

```
CHOSEN — 6.3 Security and Compliance Review:
Review of the dependency surface and disposition of findings. The security
sign-off depends on the reviewer's judgment, not on a pipeline pass.

Inputs the reviewer reads:

Dependency tree:
  The full transitive dependency tree at the release commit (npm ls,
  pip freeze, dotnet list package, depending on stack).
  The reviewer notes the count of direct vs transitive dependencies and
  any suspiciously large transitive set.

Vulnerability scan output:
  The latest Snyk / GitHub Advisory / OSV scan findings.
  For each high/critical: location, affected version, fixed version,
  exploitability assessment.

License compliance scan output:
  The full license inventory.
  Confirm no GPL or AGPL dependency appears that would impose copyleft
  on our distributed code. (License policy: MIT, Apache 2.0, BSD, ISC
  acceptable; LGPL conditional; GPL/AGPL forbidden.)

Per-finding disposition (reviewer judgment):

For each high/critical vulnerability:
  Is the affected code path reached by our application?
  If yes: must-update before release. If no: accepted-risk-not-reachable
  with the reasoning recorded.
  If a fixed version exists but introduces a breaking change: weigh
  the risk of upgrading vs the risk of not. Document.

For each transitive dependency that has been flagged for unusual
behaviour (typosquat candidate, recently-transferred maintainer, etc.):
  Reviewer reads recent commits to the dependency. If anything looks
  malicious or low-quality, flag for replacement.

For each licence-policy violation:
  Must be removed or replaced before release. No accepted-risk path.

Sign-off artifact:
  The reviewer attaches the dependency tree snapshot, the scan output,
  the per-finding dispositions, and the signed approval to the release
  ticket. Sign-off is dated and tied to the commit hash.

REJECTED — 7.2 CI Pipeline and Quality Gates:
Add a Snyk vulnerability scan stage to the CI pipeline. Configure the
stage to fail the build on any high or critical finding. Add a license-
compliance scan that fails on GPL or AGPL dependencies. Add a Dependabot
configuration so that dependency updates are proposed as automatic pull
requests. Document the pipeline changes in the team wiki.
[WRONG: the prompt asks for a security-team validation with reviewer
sign-off. CI gates can mechanically fail builds on findings, but they
cannot make the disposition decisions — accepted-risk-not-reachable
versus must-update — that depend on human reading of the affected
code path. CI is a guard rail; the security review is the human
judgment that signs off on the release. A clean CI pass is supporting
evidence; the review is the gate.]
```
