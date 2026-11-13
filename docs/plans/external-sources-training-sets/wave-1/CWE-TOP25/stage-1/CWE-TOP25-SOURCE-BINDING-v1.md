# CWE-TOP25 — Source Binding
# Stage 1, Document 1 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: MITRE CWE Top 25 Most Dangerous Software Weaknesses 2023
# License: MITRE CWE Terms of Use — FREE_COMMERCIAL_USE_WITH_NOTICE
# Attribution required: "Copyright © The MITRE Corporation. CWE and CAPEC are
#   registered trademarks of The MITRE Corporation."
# Produced: 2026-05-05

---

## What this document is

CWE-TOP25 is the negative case authority for batch CB4. While OWASP-ASVS
defines what correct security controls look like (positive enforcement),
CWE-TOP25 defines the weakness categories that appear when those controls
are absent or incorrectly implemented. Training pairs from this source
teach the orchestrator to identify specific weakness patterns — not by
CWE ID alone, but by the observable behavioral signature of the weakness
in a runtime description.

---

## Source summary — authentication/authorization weaknesses in 2023 Top 25

| Rank | CWE ID | Name | Sub-phase 3.4 relevance |
|------|--------|------|------------------------|
| 11 | CWE-862 | Missing Authorization | Primary: function or data access with no authorization check |
| 13 | CWE-287 | Improper Authentication | Primary: authentication control that can be bypassed or spoofed |
| 18 | CWE-798 | Hard-coded Credentials | Secondary: static secrets embedded in authorization code |
| 20 | CWE-306 | Missing Authentication for Critical Function | Primary: critical operation exposed without any auth gate |
| 22 | CWE-269 | Improper Privilege Management | Primary: privilege assignment or escalation outside policy |
| 24 | CWE-863 | Incorrect Authorization | Primary: authorization check present but produces wrong result |

Additional relevant (not in top 25 but sub-phase 3.4 scope):
- CWE-384: Session Fixation (V7 session management)
- CWE-613: Insufficient Session Expiration (V7.3.1)
- CWE-347: Improper Verification of Cryptographic Signature (V9.1.1)

---

## Requirement binding per scenario

### Scenario S1 — Clean authorization model with no CWE weaknesses (positive)

**S1 binding:** An API service that:
- Verifies authentication before any resource access (no CWE-306)
- Returns 403 Forbidden with no information about resource existence for
  unauthorized access (no CWE-862)
- Uses RBAC with explicit permission assignment (no CWE-269)
- Validates the caller's identity token signature before trusting any claim (no CWE-287)

**Node 8 arbiter target:** CWE-Authz-Arbiter must return PASS, confirming
none of the four targeted CWEs are present in the described implementation.
A PASS without naming each CWE confirmed absent is a coverage illusion failure.

---

### Scenario S2 — IDOR vulnerability present (worst negative)

**S2 binding:** CWE-862 (Missing Authorization) in a specific form: an API
endpoint that retrieves a user's profile by user ID without verifying the
requesting caller has permission to access that specific user ID. The caller
is authenticated (token valid) but not authorized (no ownership or permission
check for the target resource).

```
GET /api/v1/users/{userId}/profile
Authorization: Bearer [valid token for user A]
userId in path: user B's ID
```

This is the canonical CWE-862 / IDOR (Insecure Direct Object Reference) pattern.
It also maps to ASVS V8.2.2 (data-specific access restriction).

**Node 8 arbiter target:** CWE-Authz-Arbiter must return REPROCESS, naming
CWE-862 and the specific missing check: the endpoint must verify that the
authenticated user has permission to access the requested userId, not just
that the user is authenticated.

---

### Scenario S3 — Elevation of privilege via role manipulation (borderline)

**S3 binding:** CWE-269 (Improper Privilege Management) in a borderline form.
A user with "editor" role can call an endpoint that adds a new member to a
workspace. The endpoint checks that the caller has "editor" or "admin" role —
no CWE-862 present (authorization check exists). However, the endpoint allows
an editor to assign admin role to the new member, effectively allowing privilege
escalation (an editor should not be able to grant a role above their own level).

This is CWE-269 (Improper Privilege Management), not CWE-862 (Missing
Authorization). The authorization check is present — the role assignment
scope is the problem. The orchestrator must distinguish "authorization check
absent" from "authorization check present but scope incorrect."

**Node 8 arbiter target:** CONCERN (not REPROCESS). The authorization check
exists (editor role verified). The scope limitation is a privilege management
concern — role assignment must be bounded by the assignor's own role level.
A REPROCESS verdict for S3 would indicate the orchestrator cannot distinguish
CWE-862 from CWE-269.

---

## Arbiter role names

| Arbiter role | CWEs it enforces | Triggered by |
|-------------|-----------------|-------------|
| `CWE-Authz-Arbiter` | CWE-862, CWE-863, CWE-269, CWE-306 | G-access-control |
| `CWE-Authn-Arbiter` | CWE-287, CWE-306, CWE-798 | G-auth-model |

---

## Acceptance criteria

**AC-1:** For S1, Node 8 must emit PASS. Must name at least three CWE IDs from
the binding list as confirmed absent. "No security issues found" without CWE
IDs fails.

**AC-2:** For S2, Node 8 must emit REPROCESS. Must name CWE-862 explicitly and
describe the specific missing check (resource-level authorization for the userId
parameter). PASS or CONCERN fails.

**AC-3:** For S3, Node 8 must emit CONCERN. Must name CWE-269 (not CWE-862).
REPROCESS fails — the authorization check exists, making this a privilege
management concern, not a missing authorization block.

**AC-4:** For all scenarios, Node 8 output must distinguish between "no
authorization check present" (CWE-862/CWE-306) and "authorization check present
but scope incorrect" (CWE-269/CWE-863). Using CWE-862 for S3 is a failure even
if CONCERN is the verdict code.
