# Layer 2 — Nine-Node 4.3 Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-019-001 — OktaIdentityConnector: nine-node IntegrationConnectorFlow43 (IMPLEMENT + API_CONTRACT + AUTHORIZATION)

**ConnectorName:** OktaIdentityConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Okta identity connector, implement the OAuth 2.0 PKCE authorization flow, and validate the access token on each request."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY), 3.4 (AUTHORIZATION)
DecisionCount: 4
```

### N02 — DomainTranslatorUnit
```
Connector: OktaIdentityConnector
API: Okta OAuth 2.0 / OIDC API
Scope: Four handlers — PKCEAuthHandler, TokenExchangeHandler, TokenValidationHandler, TokenRefreshHandler
Protocol: OAuth 2.0 PKCE (Proof Key for Code Exchange)
```

### N03 — SchemePlannerUnit
```
IOktaIdentityConnector:
  - BeginAuthorizationAsync(state: string) → OperationResult<AuthorizationUrl>
  - ExchangeCodeAsync(code: string, codeVerifier: string) → OperationResult<TokenSet>
  - ValidateAccessTokenAsync(accessToken: string) → OperationResult<ClaimsPrincipal>
  - RefreshTokenAsync(refreshToken: string) → OperationResult<TokenSet>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IOktaJwksCache, ICodeVerifierStore
Config: OktaDomain, OktaClientId, OktaRedirectUri, OktaScopes
```

### N05 — ApplierUnit
```
Files:
1. OktaIdentityConnector.cs
2. IOktaIdentityConnector.cs
3. OktaPkceHelper.cs — code verifier/challenge generation (S256 method)
4. OktaJwksCache.cs — JWKS caching and JWT validation
5. OktaModels.cs (TokenSet, ClaimsPrincipal, AuthorizationUrl)
6. OktaConfig.cs
7. OktaIdentityConnector.md — OAuth 2.0 PKCE documentation
```

### N06 — ResultTranslatorUnit
```
BeginAuthorizationAsync: Build /oauth2/v1/authorize URL with PKCE challenge (S256)
ExchangeCodeAsync: POST /oauth2/v1/token with code + code_verifier
ValidateAccessTokenAsync: Fetch JWKS → verify JWT signature → validate iss, aud, exp claims
RefreshTokenAsync: POST /oauth2/v1/token with refresh_token grant type
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 4 methods implemented
  - pkce: S256 code challenge method used
  - jwks: token validation against JWKS endpoint
  - authorization-review: OAuth 2.0 PKCE protocol correctness (AUTHORIZATION predicate)
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - BeginAuthorizationAsync: PKCE challenge S256 correct — PASS
  - ExchangeCodeAsync: token exchange with code_verifier — PASS
  - ValidateAccessTokenAsync: JWKS-based JWT validation — PASS
  - RefreshTokenAsync: refresh_token grant — PASS
  - AuthorizationFlow: OAuth 2.0 PKCE protocol compliant — PASS
  - SecurityFlow: token validation and storage reviewed — PASS
```

### N09 — FormatterUnit
```
OktaIdentityConnector complete.
Files generated: 7
ConnectorName: OktaIdentityConnector
SubPhase: 4.3 (primary), 3.4 (SecurityAuditFlow34 and AuthorizationFlow34 also ran)
DecisionCount: 4
```

---

## B17-L2-019-002 — Gate B for OktaIdentityConnector

**Gate B:** Four interface methods implemented, PKCE S256 method used, JWKS token validation present, OAuth 2.0 refresh token support present, filesGenerated >= 6, AuthorizationFlow verdict = PASS.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 6 AND pkceS256Present = true AND jwksValidationPresent = true.
