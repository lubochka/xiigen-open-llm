# Layer 2 — Capability Gap Extended Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-008 — APIContractFlow33 nine-node: LMS + SSO integration

Request: "Integrate the internal LMS with the corporate SSO (Single Sign-On) provider."

### Flow: APIContractFlow33 (API_CONTRACT predicate; "integrate" always fires)

**Node 1**: Parse. "integrate" → API_CONTRACT (always fires regardless of LMS/SSO context).
**Node 2**: Predicate set = ["API_CONTRACT"]. TransitionType: documentation.
**Node 3**: API scope: SAML 2.0 SSO integration — assertion endpoints, attribute mapping, session management.
**Node 4**: Contract spec: IdP (Identity Provider) metadata URL, SP (Service Provider) entity ID, assertion consumer URL, signed assertions required.
**Node 5**: Arbiter: APIContractArbiter33
**Node 6**: Contract review: SAML 2.0 flow documented; attribute mapping (email, role, department) specified; session timeout policy defined.
**Node 7**: APIContractFlow33 GREEN — SSO integration contract approved.
**Node 8**: Output: lms-sso-contract.md | Protocol: SAML 2.0 | Attributes: email/role/department
**Node 9**: Verdict: API_CONTRACT_APPROVED — TransitionType: documentation.
