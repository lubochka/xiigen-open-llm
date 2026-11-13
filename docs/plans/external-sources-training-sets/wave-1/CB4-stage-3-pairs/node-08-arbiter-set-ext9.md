# CB4 Stage 3 — Node 08 Arbiter Set Extension 9
# Sequences CB4-3.4-241 through CB4-3.4-248
# Stratum: D — adversarial arguments, edge cases, subtle mistakes
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 496 instances

---

## Pair[CB4-3.4-241]

**A:** OAuth 2.0 server assessment. No state parameter is included in authorization requests. The team's response: "RFC 6749 Section 10.12 says state is RECOMMENDED for CSRF prevention — it uses 'SHOULD' language, not 'MUST'. Since RFC 6749 doesn't require it, we treat it as optional. Our threat model assessed the CSRF risk as acceptable."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.6: REPROCESS
Argument rejected: "RFC 6749 makes state RECOMMENDED, not required."

RFC 6749 and ASVS 5.0.0 are separate standards with independent normative
requirements. RFC 6749 Section 10.12 uses RECOMMENDED language for the state
parameter — this is the RFC's normative level. ASVS 5.0.0 V10.2.6 uses MUST
language — this is ASVS's independent normative requirement, which is stricter than
RFC 6749 on this point.

These standards are evaluated independently. A MAY or SHOULD in one standard does
not override a MUST in another. ASVS V10.2.6 compliance requires the state parameter
regardless of RFC 6749's language.

Second argument rejected: "risk acceptance documented in threat model."
Risk acceptance by a team does not modify a normative MUST requirement in ASVS.
ASVS V10.2.6 is not a risk-tiered recommendation — it is a binary requirement.

Evidence gap: state parameter absent from authorization requests.
Overall verdict: REPROCESS
```

**Reason:** RFC 6749 normative language is irrelevant to ASVS compliance. ASVS 5.0.0 V10.2.6 is an independent MUST requirement. Standards are evaluated against their own normative levels; a weaker requirement in one standard does not reduce the requirement in another. Risk acceptance in a threat model does not modify binary ASVS requirements.

**Phrasing-B:** RFC 6749 only recommends the state parameter — Section 10.12 says "SHOULD" not "MUST". We looked at the RFC, assessed the CSRF risk as low given our user base, and documented it as accepted. Why would ASVS V10.2.6 override the RFC's own language on this?

---

## Pair[CB4-3.4-242]

**A:** Internal admin dashboard review. The `/admin/config` endpoint requires a custom HTTP header: `X-Internal-Token: [value]`. The header value is defined in deployment configuration and known to the operations team. No other authentication or authorization check is applied. The team argues: "Only internal services know this header name and value. External attackers wouldn't know to include it."

**B:**
```
Arbiter: CWE-Authz

CWE-288 (Authentication bypass via alternate path): REPROCESS — CRITICAL
Argument rejected: "only internal parties know the header value."

A custom header with a static secret value is not an authentication mechanism.
It is an obscurity control. Security through obscurity provides no protection
against: (1) parties who have accessed deployment configuration (any developer,
CI/CD system, or configuration management tool that has read the config), (2)
network traffic interception on any hop where the header is transmitted, (3)
accidental disclosure through logs, error messages, or documentation, (4) brute
force if the value space is small.

Authentication requires verifying the identity of the requesting party. A static
shared value transmitted in a header verifies only that the caller has the value —
it does not establish caller identity. The endpoint bypasses the application's
authentication framework by using an alternate verification mechanism that provides
no authenticated identity.

CWE-288 requires that alternate code paths not bypass authentication. Replacing
authentication with a static header value is a bypass, not an alternative
implementation.

Severity: CRITICAL — unauthenticated configuration access.
Overall verdict: REPROCESS — CRITICAL
```

**Reason:** Security through obscurity is not authentication. CWE-288 prohibits alternate paths that bypass authentication regardless of how obscure the bypass mechanism is. The "only internal parties know" argument fails because static values in deployment configuration are accessible to anyone with config access, CI/CD systems, and interception, and do not establish caller identity.

**Phrasing-B:** Our internal admin endpoint uses a secret header — it's not public knowledge what the header name and value are, so external attackers can't access it. We've been using this pattern for internal service authentication for years. Is a secret header sufficient for protecting internal-only endpoints?

---

## Pair[CB4-3.4-243]

**A:** Application log review. JWT access token payloads are logged including sub, email, and roles claims. The team's argument: "Our log storage is encrypted with AES-256 at rest. The sensitive data is protected because it can only be accessed by authorized log storage administrators. V8.2.2 is about data protection, and we've protected it."

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.2.2: REPROCESS
Argument rejected: "log encryption at rest satisfies V8.2.2."

V8.2.2 requires that sensitive security-state data including authentication tokens
and their decoded claims MUST NOT appear in application logs. The requirement governs
what data is written to logs — not how log storage is secured after writing.

Encryption at rest protects log data from unauthorized access to log storage
infrastructure. It does not prevent access to JWT claims by: (1) log analysis
tools and dashboards used by developers, (2) log aggregation platforms (Splunk,
Datadog, Elasticsearch) that ingest and index log lines, (3) on-call engineers
reviewing application errors, (4) SIEM systems processing security events,
(5) log shipping intermediaries. All of these operate on decrypted log content
as part of normal operations.

The purpose of V8.2.2 is to prevent sensitive authentication state from entering
the log data pipeline at all. Once written, log contents propagate to numerous
downstream systems. Encryption of the origin storage does not address this
propagation.

Evidence gap: JWT claims (sub, email, roles) written to application logs.
Overall verdict: REPROCESS
```

**Reason:** Log encryption at rest addresses storage-level unauthorized access; it does not address the log data pipeline through which sensitive log content propagates to analysis tools, dashboards, SIEM systems, and on-call review. V8.2.2's requirement is about what is written to logs, not about log storage security. These are different controls serving different purposes.

**Phrasing-B:** We encrypt our logs at rest with AES-256 — they can only be read by authorized log admins. Since the JWT claims are protected by encryption, V8.2.2 should be satisfied, right? We're not leaving them in plaintext.

---

## Pair[CB4-3.4-244]

**A:** JWT service review. Access tokens have a 90-day expiry. The team's response to V9.2.1: "Our JWT library automatically validates the exp claim on every token — it rejects expired tokens. V9.2.1 says tokens must have an expiry claim that is verified; we have both. The 90-day window is set to match our session length and was reviewed by our security team."

**B:**
```
Arbiter: ASVS-V9-Token

V9.2.1: CONCERN
The JWT library correctly verifies the exp claim — this satisfies the verification
component of V9.2.1. However, V9.2.1 requires not only that expiry is verified
but that the expiry window is appropriate for the token type and risk profile.

A 90-day expiry for an access token is an extended duration that significantly
increases the window of exploitability following token compromise. If a token is
stolen (via log exposure, traffic interception, client storage extraction, or
other means), the attacker has a 90-day window to use it. Refresh tokens are
typically used for session continuity; access tokens should use short expiry
windows (minutes to hours) to limit the blast radius of compromise.

Security team approval of the 90-day window is noted but does not change the
technical finding. CONCERN is appropriate because: (1) the exp claim is present
and verified (technical requirement met), (2) the duration is operationally
problematic (significantly elevated risk window), and (3) an explicit business
case ("matches session length") has been stated. This is classified as CONCERN
rather than REPROCESS because expiry is implemented and verified; the concern
is configuration appropriateness.

Overall verdict: CONCERN
```

**Reason:** V9.2.1 requires that the exp claim is present and verified (met) and that it is set appropriately (concern-level gap). The library correctly validates the claim — the technical requirement is satisfied. The 90-day access token window creates elevated compromise risk. This is CONCERN not REPROCESS because the control exists and functions; the configuration creates an operationally elevated risk window.

**Phrasing-B:** V9.2.1 requires an expiry claim that's actually verified — we have both. The JWT library checks exp on every request. We set 90 days to align with our session length; it was reviewed internally. The requirement is met: exp present, exp verified. Is there still a finding?

---

## Pair[CB4-3.4-245]

**A:** Admin reporting endpoint review. `/admin/reports` has no server-side role check. The team's defense: "The admin dashboard UI is only rendered for users with the admin role — we check the role at the UI level in our React component. Non-admin users never see the reports link, so they don't know the endpoint exists. The authorization is enforced in the UI."

**B:**
```
Arbiter: CWE-Authz

CWE-862 (Missing authorization check): REPROCESS
Argument rejected: "authorization enforced in the UI."

Authorization checks must be enforced server-side on the endpoint handling the
request. Client-side controls — including UI rendering conditionals, hidden
navigation elements, and frontend role checks — do not constitute authorization.
Any authenticated user can call /admin/reports directly via: (1) browser developer
tools by constructing a fetch/XHR request, (2) curl or any HTTP client, (3) API
testing tools, (4) another frontend application that calls the same API. The fact
that the reports link is not rendered in the UI for non-admin users does not prevent
direct API calls.

"They don't know the endpoint exists" — security through obscurity. Endpoint paths
are discoverable through: API documentation, browser developer tools, network
traffic analysis, source code access, and automated discovery. Undiscoverability
is not a security control.

The server-side endpoint must verify the requesting user's role on every request,
independent of any client-side rendering logic.

Evidence gap: /admin/reports has no server-side role check.
Overall verdict: REPROCESS
```

**Reason:** Client-side authorization is not authorization. UI-level role checks control what is rendered; they do not prevent direct API calls. CWE-862 requires server-side authorization checks on every request. The "they don't know the endpoint" argument is obscurity, not access control. Both the enforcement location argument and the discoverability argument are rejected.

**Phrasing-B:** Our React frontend only shows the admin reports section to users with the admin role — we check the role in the component before rendering. Non-admin users don't see the link and don't know the API path. The authorization happens at the UI layer. Is a server-side check also required if the UI already handles it?

---

## Pair[CB4-3.4-246]

**A:** Microservice JWT validation review. Access tokens include `aud: "api.example.com"`. The receiving API service does not validate the aud claim during token processing — it trusts any token with a valid signature from the known issuer. The team argues: "We only have one authorization server issuing tokens for our system. Since we control the issuer, there's no way a token issued for a different audience would reach us — validating aud is redundant overhead."

**B:**
```
Arbiter: ASVS-V10-OAuth (for OAuth-context tokens) / ASVS-V9-Token (for standalone JWT)

V10.5.1 / V9.2.1: REPROCESS
Argument rejected: "single issuer makes aud validation redundant."

The aud claim MUST be validated by every service that accepts the token. The
single-issuer argument fails for the following reasons:

(1) Token confusion attack: in a system with multiple services (Service A, Service B,
    Service C) sharing one authorization server, a token issued to Service A with
    aud="service-a.example.com" can be replayed against Service B if Service B does
    not validate aud. The attacker uses a legitimately obtained token from one service
    to access another service within the same infrastructure.

(2) Future architectural change: the argument assumes the single-issuer architecture
    is permanent. Adding a second service, a partner integration, or a migration means
    the assumption changes; by then, missing aud validation may be exploited in production.

(3) Issuer trustworthiness ≠ audience binding: validating iss confirms the token was
    issued by a trusted party; validating aud confirms the token was issued for this
    specific service. Both are required independently.

Evidence gap: aud claim present in tokens but not validated by the receiving service.
Overall verdict: REPROCESS
```

**Reason:** Aud validation protects against token confusion attacks across services sharing the same issuer — a legitimate token from Service A replayed against Service B. The single-issuer argument conflates issuer trustworthiness with audience binding. They are independent claims serving independent purposes. The requirement is binary and does not admit a "single issuer" exception.

**Phrasing-B:** Our tokens include aud="api.example.com" but we don't validate it — we just check the signature and iss. We only have one authorization server across all our services, so there's no scenario where a token from a different issuer would arrive. Validating aud when we're the only relying party seems like unnecessary work. Does it matter?

---

## Pair[CB4-3.4-247]

**A:** Threat model currency review. The current threat model artefact is 18 months old. The team argues: "Our core architecture hasn't changed since the threat model was written. We added a payment processing module, a partner API integration, and a mobile app backend in the last 18 months, but these are feature additions, not architectural changes. The fundamental trust model is the same."

**B:**
```
Arbiter: SSDF-PW

PW.2 (threat model currency): REPROCESS
Argument rejected: "core architecture unchanged; additions are features not
architecture."

SSDF PW.2 requires the threat model to reflect the current system. "Reflect the
current system" means the threat model must cover the actual system as deployed,
including components added since the last update. The distinction between "feature
additions" and "architectural changes" is not a criterion in PW.2.

The additions described change the threat surface materially:
- Payment processing module: introduces financial transaction data as a new asset,
  external payment processor as a new trust boundary, financial fraud and transaction
  manipulation as new threat categories.
- Partner API integration: introduces an external party as a caller with a defined
  trust level; introduces credential management for partner access; introduces
  data-sharing trust boundaries with an external organization.
- Mobile app backend: introduces mobile client as a new actor; introduces mobile-
  specific threats (certificate pinning, device compromise, token storage on mobile);
  introduces public internet exposure patterns distinct from existing web clients.

None of these attack surfaces and trust boundaries are modeled in an 18-month-old
threat model produced before these components existed. The threat model does not
reflect the current system.

Evidence gap: threat model 18 months old, does not cover payment processing,
partner API, or mobile backend components.
Overall verdict: REPROCESS
```

**Reason:** PW.2 requires the threat model to cover the current system. New components (payment processor, partner API, mobile backend) each introduce new assets, trust boundaries, and threat actors not present in the 18-month-old model. The "core architecture unchanged" argument fails because PW.2 is scoped to the current system in its entirety, not its foundational components. Feature additions that introduce external parties and new data assets materially expand the threat surface.

**Phrasing-B:** The threat model from 18 months ago is still accurate for our core architecture — the same services, same database structure, same auth model. We added a payment module, a partner API, and a mobile backend, but those didn't change how the core system works. The fundamental threat model hasn't changed. Does adding features require updating the threat model?

---

## Pair[CB4-3.4-248]

**A:** Admin panel access control review. Admin routes `/admin/*` are accessible only from corporate IP ranges (VPN required for access). The team argues: "V8.1.2 requires deny-by-default authorization. We deny by default at the network level — only VPN-connected corporate IPs can reach the admin panel. Non-corporate IPs are blocked by the firewall. This IS deny-by-default."

**B:**
```
Arbiter: ASVS-V8-AuthZ

V8.1.2: REPROCESS
Argument rejected: "network-level IP restriction satisfies application-layer
deny-by-default."

V8.1.2 requires deny-by-default authorization at the application layer — every
request to a protected resource must be evaluated against the requesting user's
authenticated identity and authorized roles, not their network origin. Network-level
IP restrictions and application-level authorization are separate, complementary
controls; one does not substitute for the other.

Network-level IP whitelisting does not provide deny-by-default authorization
protection against:
(1) Compromised corporate machines — an attacker on a corporate device within the
    allowed IP range bypasses network controls entirely.
(2) VPN-connected attackers — a party with corporate VPN credentials (stolen,
    phished, or authorized-but-not-authorized-for-admin) satisfies the IP check.
(3) SSRF attacks — a server-side request forgery vulnerability on any corporate-
    network service can reach admin routes from an allowed IP.
(4) Lateral movement — an attacker who has compromised any corporate system can
    access admin endpoints from that system.
(5) Shared infrastructure — developer workstations, CI/CD agents, and internal
    tools that share the corporate network range may be able to reach admin routes.

Application-level deny-by-default means: each admin request is checked against the
requesting user's authenticated identity, the admin role is verified, and access is
denied unless the specific identity is authorized. This check must occur at the
application layer on every request.

Evidence gap: admin routes accessible to any VPN-connected party without
application-level identity and role verification.
Overall verdict: REPROCESS
```

**Reason:** Network-level IP whitelisting is a perimeter control that restricts which network addresses can reach an endpoint. Application-level deny-by-default is a per-request identity and role verification at the application layer. These are complementary controls that cannot substitute for each other. V8.1.2 requires the application layer check, and network-level restrictions do not satisfy it.

**Phrasing-B:** Admin routes are only reachable from within our corporate VPN — the firewall blocks all other IP ranges. Nobody without VPN access can reach /admin at all. We've denied access by default at the network level. Does V8.1.2 require something additional beyond network-level access restriction?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-241 | V10.2.6 "RFC says RECOMMENDED not MUST" argument | D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-242 | CWE-288 "secret header = auth" security-through-obscurity | D | CWE-Authz | REPROCESS CRITICAL |
| CB4-3.4-243 | V8.2.2 "encrypted at rest covers V8.2.2" argument | D | ASVS-V8-AuthZ | REPROCESS |
| CB4-3.4-244 | V9.2.1 90-day exp: claim present + verified, duration CONCERN | D | ASVS-V9-Token | CONCERN |
| CB4-3.4-245 | CWE-862 "role checked in UI" client-side authorization | D | CWE-Authz | REPROCESS |
| CB4-3.4-246 | V10.5.1 aud not validated "single issuer makes it redundant" | D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-247 | PW.2 "core architecture unchanged, features added" 18 months | D | SSDF-PW | REPROCESS |
| CB4-3.4-248 | V8.1.2 "network IP whitelist = deny-by-default" argument | D | ASVS-V8-AuthZ | REPROCESS |
