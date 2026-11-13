# Spec Pairs — Sub-phase 2.1 Business Capability Definition
# Batch: dpo-pairs-spec-batch-005
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: capability boundaries, user value, actors, expected business outcomes
# Pair count: 4

---

## Pair AP5-2.1-001 — capability definition confused with user stories

```
source_id: xiigen-spec-batch-005-AP5-2.1-001
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.1
rejected_sub_phase: 2.2
```

Prompt: `"define the business capability for customer self-service refunds — what is in scope, who can use it, and what business outcomes do we expect"`

```
CHOSEN — 2.1 Business Capability Definition:
A capability statement that names the boundary, the actors, the value, and
the outcome expectations. Acceptance criteria are out of scope here.

Capability name: Customer Self-Service Refunds.

In scope:
  Customers can initiate a refund for an order they placed within the
  last 30 days, for items that have not yet shipped or that have been
  returned in receivable condition.
  Refunds may be partial (specific line items) or full (entire order).
  Refunds are credited to the original payment method only.

Out of scope (explicitly):
  Refunds beyond 30 days — these continue to require support agent
  handling.
  Refunds to a different payment method — out of scope; agent path.
  Refunds for digital subscription products — handled by a different
  capability (subscription cancellation), not this one.
  Cross-tenant refunds (refunds initiated by an admin on behalf of a
  customer in a different tenant) — not in this capability.

Actors:
  Primary: end-customer who placed the original order.
  Out: support agents (continue to use the existing support tool for
   the cases that this capability does not cover).

Business value:
  Customer perceived value: faster refunds (target: ≤ 5 min from request
   to payment-method credit, vs current 24-48 hr through support).
  Operational value: reduce refund-related support tickets by 40% within
   six months of launch (current rate: ~1200/month, target ≤ 720/month).
  Revenue protection value: faster refunds increase customer trust and
   repeat purchase rate (the model predicts +1.5 percentage points on
   repeat-purchase rate within 90 days post-refund).

Expected outcomes:
  Within 90 days of launch:
   - 60% of eligible refunds are completed via self-service (rather
     than via support tickets).
   - Customer satisfaction (CSAT) on refund interactions ≥ 4.4 / 5
     (current: 3.8 / 5).
   - Mean time to refund completion ≤ 5 minutes.
  These are outcome targets; the user stories in 2.2 will describe
  the specific user journeys that achieve them.

Dependencies / preconditions:
  Payment provider supports refund-to-original-method API (verified
  for Stripe; verify for Adyen if Adyen is in scope at launch).
  Order data model exposes a "refundable_until" timestamp.

REJECTED — 2.2 User Stories and Acceptance Criteria:
As a customer, I want to request a refund for an order I placed within
the last 30 days, so that I can resolve issues quickly.
Acceptance: I can select the order from my order history. I can choose
"Refund" and pick which items. I see the refund amount before confirming.
I receive a confirmation email within 1 minute. The refund is credited
to my original payment method within 5 minutes.

As a customer, I want to track the status of my refund, so that I know
when the credit will appear.
Acceptance: I can view the refund status from the order detail page.
I see the expected credit date based on my payment method.

As an admin, I want to see refund analytics, so that I can monitor
operational health.
Acceptance: ...
[WRONG: "define the business capability — scope, actors, outcomes"
asks for the capability boundary and the value proposition. User stories
are 2.2's responsibility — they slice the capability into observable
behaviours and acceptance criteria. The capability must exist first; the
stories operate within its scope. Returning user stories when the
capability boundary is asked for skips the scope decision and produces
stories without a frame.]
```

---

## Pair AP5-2.1-002 — capability boundary confused with architecture topology

```
source_id: xiigen-spec-batch-005-AP5-2.1-002
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.1
rejected_sub_phase: 3.1
```

Prompt: `"what is the business capability of our notification feature — who is it for, what does it do, what is it not for?"`

```
CHOSEN — 2.1 Business Capability Definition:
A capability statement framed in business terms. Architecture is out of
scope at this layer.

Capability name: User Notification.

What it is for:
  Letting users know about events that affect them and that they care
  about, in the channel of their choice, with respect for their attention.

What it does (in business terms):
  - Delivers messages to the user's chosen channel (email, push, SMS,
    in-app) for the events the user has subscribed to.
  - Lets users control what they receive: opt-in / opt-out per category,
    quiet hours, frequency caps.
  - Gives the business a way to add new notification categories
    without engineering work for every category.

What it is not for (out of scope):
  Marketing messages — those run through a different capability (Campaign
   Management) and a different consent regime.
  Transactional invoices and receipts — these have legal-required delivery
   and are not optional; they are sent by the Billing capability, not
   this one.
  Real-time chat or collaboration — that is Messaging, a different
   capability with different latency expectations.

Primary actors:
  End-user (recipient).
  Business operator (defining new notification categories).
  Internal automated systems (the events that trigger notifications).

Business value:
  For the user: relevant updates without spam, control over their inbox,
   the right channel at the right time.
  For the business: reach users without becoming the source of fatigue;
   high-signal communication; fewer unsubscribes from marketing because
   transactional and notification messages do not annoy users.

Outcome expectations:
  Notification delivery success ≥ 99% within their respective SLAs
   (push within 30 seconds, email within 5 minutes, SMS within 60
   seconds).
  Unsubscribe rate per category ≤ 0.5% per month (high unsubscribe is
   a signal that the category is too noisy).
  Per-category configurability without code change for the top 80% of
   business needs.

Dependencies:
  Channel providers (push platform, email provider, SMS provider) are
   in place.
  User identity has channel addresses (verified email, push token,
   verified phone).

REJECTED — 3.1 Architecture and Component Topology:
The notification capability comprises five components: NotificationGateway
(public API), CategoryService (manages notification categories and user
subscriptions), DispatchService (selects the channel and constructs the
delivery payload), DeliveryService (talks to the channel providers),
DeliveryTracker (records the delivery state). Dependency direction is
strictly one-way: Gateway → Category → Dispatch → Delivery → Tracker.
No reverse calls. Delivery providers are abstracted behind a single
IChannelProvider interface to allow swapping without ripple effects.
[WRONG: "what is the business capability — who is it for, what does it
do, what is it not for" asks for the business framing — actors, value,
boundary in business terms. Architecture topology is 3.1's responsibility
and is downstream of capability definition. The architecture serves the
capability; without the capability boundary, the architecture is
designing a system whose purpose is not yet defined. Returning a
component diagram skips the business framing.]
```

---

## Pair AP5-2.1-003 — capability scope confused with backend implementation

```
source_id: xiigen-spec-batch-005-AP5-2.1-003
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.1
rejected_sub_phase: 4.1
```

Prompt: `"define the scope and intended outcome of the new role-based access control feature — what business problem are we solving?"`

```
CHOSEN — 2.1 Business Capability Definition:
A scope statement framed in business outcomes. The implementation comes
later.

Capability name: Role-Based Access Control (RBAC) for tenant administrators.

Business problem being solved:
  Today, every tenant administrator has full access to every feature in
  the workspace. This forces customers to share a single login or to
  trust everyone with everything. Two specific consequences:
   1. Mid-market customers (10-50 employees) cannot adopt our product
      because their compliance posture forbids "shared credentials".
   2. Existing customers report that small mistakes (an admin deleting
      data they did not understand) are over-represented in support
      tickets — there is no mechanism for "reduce the blast radius".

Capability scope:
  In scope:
   - Predefined roles: Owner, Admin, Editor, Viewer. The role assignments
     are scoped to the tenant.
   - Per-role permission sets covering the major feature areas
     (data access, billing, user management, reporting).
   - Self-service role assignment by Owners.
  Out of scope (explicit):
   - Custom roles defined by customers — out of scope for v1; tracked
     in the backlog for v2.
   - Field-level permissions ("Editor can edit the order but not the
     price") — out of scope for v1.
   - Cross-tenant roles (a single user with different roles in different
     tenants) — out of scope; one user has one role per tenant.

Actors:
  Owner — the user who set up the tenant; can manage roles for everyone.
  Admin — can manage all features but cannot change roles for other
   admins or owners.
  Editor — can change content within data they own.
  Viewer — read-only.

Business value:
  Compliance: meets the "least-privilege" requirement that mid-market
   customers reference in their security questionnaires.
  Risk reduction: fewer "oops" incidents from over-permissioned users.
   Target: 50% reduction in support tickets categorised as "user
   mistake / data deleted".
  Revenue: unlocks the mid-market segment that has been blocked by the
   missing feature.

Success criteria for the capability (not for the implementation):
  90 days post-launch:
   - 60% of tenants with > 10 active users have used at least one
     non-Owner role.
   - Tenant churn cited "lack of permissions model" reduces from 8%
     to 2% (target).
   - Sales-blocking issues mentioning RBAC reduce from 18% of lost
     deals to ≤ 5%.

Dependencies:
  The user model supports per-tenant membership.
  The audit log captures actor identity (already in place).

REJECTED — 4.1 Backend Services:
Implement the IPermissionService with HasPermission(userId, tenantId,
permissionKey) returning a boolean. Cache permission checks per request
in HttpContext.Items. Apply [HasPermission("orders.edit")] attributes to
controller actions. Define the role-permission mapping in
appsettings.json initially, then move to a database table for
production. Add a PermissionsMiddleware that intercepts every authenticated
request and attaches the user's permission set to the context.
[WRONG: "define the scope and intended outcome — what business problem
are we solving" asks for the capability framing — actors, value, success
criteria in business terms. Implementing the IPermissionService is 4.1's
work and follows from this scope. The implementation cannot be designed
correctly without the scope decision (e.g. v1 has no custom roles, so
the IPermissionService design is simpler than it would be otherwise).
Returning the implementation skips the business framing.]
```

---

## Pair AP5-2.1-004 — capability framing confused with user stories

```
source_id: xiigen-spec-batch-005-AP5-2.1-004
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.1
rejected_sub_phase: 2.2
```

Prompt: `"define the business capability of the team-collaboration feature — what users can do together that they cannot do alone, and what value that produces for the business"`

```
CHOSEN — 2.1 Business Capability Definition:
The capability statement covers the boundary, the actors, the kinds of
collaboration in scope, and the business outcome.

Capability name: Team Collaboration.

What is in scope:
  Real-time co-editing of documents (multiple users see each other's
  edits as they happen).
  Asynchronous comment threads attached to specific document locations.
  Mentions (@user) that surface in the mentioned user's notifications.
  Document sharing within the tenant (link sharing with view / comment /
  edit access controlled per recipient).

What is out of scope:
  Cross-tenant collaboration (sharing documents with users outside the
  tenant) — out of scope; the legal and identity questions are not
  resolved.
  Voice or video calls — out of scope; the product does not enter
  the synchronous-meeting space.
  Document version-history beyond 30 days — out of scope; the product
  is a working tool, not an archive.

Actors:
  Document owner: full control, including the ability to revoke access.
  Collaborator: can edit, comment, mention.
  Viewer: read-only and cannot mention or be mentioned.

Business value:
  For users: faster decision-making because the team can converge in
   the document instead of in side channels (Slack, email).
  For the business: customer reports of "we use your product for
   collaboration" become a retention signal; collaboration usage
   correlates with annual-contract retention.

Outcome expectations:
  Within 90 days of launch:
   - At least one collaborative document per active user per week
     (the activation signal we are betting on).
   - Comment threads on at least 30% of shared documents (a measure
     that the asynchronous mode is being used, not just real-time
     editing).
   - Net retention for collaborative-document users is at least 5
     percentage points higher than for non-collaborative users.

Pre-conditions:
  Document storage supports operational transformation or CRDT-based
  concurrent editing (technical pre-condition; verified separately).
  User identity is unique within the tenant (already in place).

REJECTED — 2.2 User Stories and Acceptance Criteria:
As a document owner, I want to share my document with specific
teammates so we can edit it together. Acceptance: I can pick
collaborators from my team list. I can set view / comment / edit access
per person. I can revoke access at any time.

As a collaborator, I want to see what others are typing in real time
so I can avoid duplicate work. Acceptance: I see other users' cursors
labelled by name. Their edits appear on my screen within 500 ms.

As a collaborator, I want to leave a comment on a specific paragraph
so my colleagues can address my point. Acceptance: I can highlight a
range and click "Comment". My comment appears in the side panel anchored
to that range. Mentioned users get a notification within 1 minute.
[WRONG: the prompt asks for the business capability — boundary, actors,
value, outcome. User stories are 2.2's responsibility and are framed
around observable user behaviour with acceptance criteria. The capability
must be defined first; the stories operate within its scope. Returning
stories when the capability boundary was asked for skips the scope
decision (which is currently undefined: do we share with non-tenant
users? what about voice? the answer is "no" but only after the
capability statement is written).]
```
