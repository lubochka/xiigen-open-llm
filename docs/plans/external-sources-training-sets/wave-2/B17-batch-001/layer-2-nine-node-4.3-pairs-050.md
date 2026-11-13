# Layer 2 — Nine-Node 4.3 Pairs (050)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-050-001 — CognitoConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"], connector: "Cognito", domain: "cloud-identity" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — AWS Cognito connector, OAuth 2.0 + ID/access token
N02: AWS Cognito User Pools API — 3 handlers: AuthenticateUserHandler, RefreshTokenHandler, GetUserProfileHandler
N03: ICognitoConnector: AuthenticateUserAsync, RefreshTokenAsync, GetUserProfileAsync
N04: IAmazonCognitoIdentityProvider (injected), ICognitoTokenStore (ID token + access token), ICognitoOAuthRefresher
N05: 5 files (connector, interface, OAuth refresher, token store, models)
N06: POST /oauth2/token (OAuth 2.0 authorization code), POST /oauth2/token (refresh_token), GET /oauth2/userInfo + ID/access token validation
N08: PASS — 3 handlers implemented, OAuth 2.0 token exchange present, ID token and access token stored securely, filesGenerated: 5
N09: CognitoConnector complete, 5 files generated
```
