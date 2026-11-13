# CWE-TOP25 — Expected Orchestration Trace
# Stage 1, Document 3 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: MITRE CWE Top 25 2023 — FREE_COMMERCIAL_USE_WITH_NOTICE
# Produced: 2026-05-05

---

## Scenario S1 — Clean authorization model, no CWE weaknesses present

### S1 Input (to Node 1)

"Our REST API implements the following authorization controls for the user
profile service:
- Every request requires a valid JWT, validated for signature (RS256), exp,
  nbf, and aud claims before any processing occurs.
- Profile retrieval (GET /api/users/{userId}/profile) checks that the JWT
  subject claim ('sub') matches the requested userId, OR that the JWT contains
  an 'admin' scope claim. No other combination is accepted.
- Privilege assignment (POST /api/users/{userId}/role) checks that the JWT
  subject matches the requesting user AND that the requesting user's current
  role is 'admin'. An editor cannot grant any role.
- All admin functions log the caller identity and action before execution.
Please assess whether this implementation exhibits any CWE Top 25
authentication or authorization weaknesses."

### S1 Expected Node 8 output

PASS. CWE-Authz-Arbiter result for the described implementation:

CWE-306 (Missing Authentication for Critical Function): ABSENT. Every endpoint
requires JWT validation before processing. Authentication gate confirmed present.

CWE-862 (Missing Authorization): ABSENT. Profile retrieval checks that the JWT
sub matches the requested userId, or that admin scope is present. This is a
resource-level authorization check — the IDOR pattern is not present because
the endpoint verifies ownership before returning data.

CWE-269 (Improper Privilege Management): ABSENT. Role assignment restricted to
JWT sub matching requesting user AND admin role. Editors cannot grant any role.
Privilege assignment bounded by caller's current role.

CWE-287 (Improper Authentication): ABSENT. JWT validated for signature, exp,
nbf, and aud. Signature validation using RS256 prevents token forgery.
Claim validation prevents replay and wrong-audience attacks.

CWE-863 (Incorrect Authorization): ABSENT. The described authorization conditions
(sub match OR admin scope for reads; admin role required for role assignment)
are correctly scoped.

No weaknesses detected. Implementation satisfies authorization requirements
for the described endpoints.

### S1 Expected Node 9 output

VERDICT: PASS

CWE Top 25 weakness assessment for sub-phase 3.4 (Security and Authorization
Model):

- CWE-306 (Missing Authentication for Critical Function): ABSENT — JWT required
  for all endpoints ✓
- CWE-862 (Missing Authorization): ABSENT — resource-level ownership check
  present on GET /api/users/{userId}/profile ✓
- CWE-269 (Improper Privilege Management): ABSENT — role assignment requires
  admin role, editors cannot grant roles ✓
- CWE-287 (Improper Authentication): ABSENT — JWT signature, exp, nbf, and aud
  validated ✓
- CWE-863 (Incorrect Authorization): ABSENT — authorization conditions correctly
  scoped ✓

No CWE Top 25 authentication or authorization weaknesses identified in the
described implementation.

---

## Scenario S2 — IDOR vulnerability (CWE-862, worst negative)

### S2 Input (to Node 1)

"Our profile service endpoint retrieves user profile data:
GET /api/v1/users/{userId}/profile
The endpoint requires a valid JWT token (authentication is required). The
endpoint handler retrieves the profile for the userId in the URL path and
returns it. The handler does not perform any check relating the JWT subject
to the requested userId. The team's reasoning: since authentication is required,
only logged-in users can make this request.
Please assess whether this endpoint has any security weaknesses."

### S2 Expected Node 8 output

REPROCESS. CWE-Authz-Arbiter result:

CWE-862 (Missing Authorization) — CONFIRMED. Error code: CWE_862_IDOR_MISSING_RESOURCE_CHECK.

The team's reasoning is incorrect. Authentication confirms that the caller is
a known user. Authorization confirms that the known user has permission to
access the specific resource requested. These are distinct checks.

The endpoint authenticates (JWT required — no CWE-306). It does not authorize
(no check that the JWT subject can access the specific userId in the path —
CWE-862 present).

Impact: Any authenticated user can retrieve any other user's profile by
supplying a different userId in the URL path. This is the canonical IDOR
(Insecure Direct Object Reference) pattern for CWE-862.

Required fix: The endpoint handler must verify that the JWT sub claim matches
the requested userId, OR that the JWT contains a scope or role that explicitly
grants cross-user profile access (e.g., admin scope). The authentication check
does not satisfy this requirement and cannot substitute for it.

### S2 Expected Node 9 output

VERDICT: REPROCESS — CWE-862 CONFIRMED (IDOR)

Security weakness found: CWE-862 Missing Authorization
Rank in CWE 2023 Top 25: #11

Weakness: The endpoint GET /api/v1/users/{userId}/profile requires authentication
but does not restrict which userId values an authenticated caller can request.
Any authenticated user can access any other user's profile.

The team's reasoning that "only logged-in users can make this request" conflates
authentication with authorization. Authentication is a precondition for
authorization — it does not substitute for it.

Required remediation: Add a resource-level authorization check in the endpoint
handler before returning profile data. The check must verify one of:
(a) JWT sub claim equals the requested userId (the user is accessing their
    own profile), OR
(b) JWT contains an explicit admin or cross-user-read scope/role claim.

The endpoint must be updated before returning to production. This item is
returned to the development queue.

---

## Scenario S3 — Privilege escalation via role assignment (CWE-269, borderline)

### S3 Input (to Node 1)

"Our workspace management API has a member-add endpoint:
POST /api/workspaces/{workspaceId}/members
The endpoint accepts a JSON body with { userId, role } where role can be
'viewer', 'editor', or 'admin'. The endpoint checks that the calling user
has 'editor' or 'admin' role in the workspace before proceeding. An editor
can add a new member with any role, including 'admin'. The endpoint was
implemented this way because editors manage workspace membership in our product.
Is there a security concern with this authorization check?"

### S3 Expected Node 8 output

CONCERN. CWE-Authz-Arbiter result:

CWE-862 (Missing Authorization): ABSENT. The endpoint checks that the caller
has 'editor' or 'admin' role before proceeding. An authorization check is
present — CWE-862 does not apply.

CWE-269 (Improper Privilege Management) — CONCERN. The authorization check
verifies that the caller has a workspace role (editor or admin), but it does
not restrict the role that the caller can assign to the new member. This allows
a user with 'editor' role to assign 'admin' role to a new member — effectively
granting a privilege level the assigning user does not hold.

This is a privilege management scope concern, not a missing authorization check.
The principle of least privilege requires that a user cannot grant permissions
exceeding their own. An editor should be able to grant only 'viewer' or 'editor'
roles. Only an admin should be able to grant 'admin' role.

This is CONCERN, not REPROCESS. The authorization check is present and
functional for its stated purpose (verifying caller membership role). The
privilege scope limitation is an enhancement to the existing check, not a
replacement of an absent check.

### S3 Expected Node 9 output

VERDICT: CONCERN — CWE-269 PRIVILEGE MANAGEMENT SCOPE

CWE assessment: CWE-269 (Improper Privilege Management) — CONCERN
CWE-862 (Missing Authorization): ABSENT — authorization check present ✓

Security concern: The endpoint allows a caller with 'editor' role to assign
'admin' role to a new member. This violates the principle of least privilege —
a user should not be able to grant a privilege level they do not hold.

Current behavior: editor callers can assign any role (viewer, editor, admin).
Expected behavior: a caller can assign roles up to their own role level only
(editors can assign viewer or editor; only admins can assign admin).

Recommended fix: Add a role-ceiling check to the authorization logic:
`if (requestedRole.level > callerRole.level) return 403 Forbidden`.
This preserves the existing 'editor or admin can add members' check and
adds the privilege ceiling.

This item does not require reprocessing — the existing authorization check
works correctly for its current scope. The privilege ceiling is an enhancement.
This concern is forwarded to the development team.
