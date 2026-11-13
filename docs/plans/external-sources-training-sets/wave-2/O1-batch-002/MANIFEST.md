# MANIFEST — O1-batch-002
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05
# Total files: 101

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 12 (001–012) |
| Layer 0-B (router pairs) | 12 (001–012) |
| Layer 0-C (knowledge edge pairs) | 12 (001–012) |
| Layer 0-D (flow factory pairs) | 12 (001–012) |
| Layer 1 (transition orchestrator pairs) | 12 (001–012) |
| Layer 2 (nine-node 3.4 pairs) | 12 (001–012) |
| Layer 3 (teaching unit pairs) | 12 (001–012) |
| Held-out and corruption pairs | 12 (001–012) |
| Cross-layer synthesis | 4 (001–004) |
| MANIFEST | 1 |
| **Total** | **101** |

---

## Layer 2 Scenario Table

| File | Scenario | Predicates | Flow State |
|------|----------|-----------|-----------|
| layer-2-nine-node-3.4-pairs.md (base) | Full client credentials | SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT | trusted_learned |
| layer-2-nine-node-3.4-pairs-002.md | ClientCredentialsFlow | SECURITY+AUTHORIZATION+IMPLEMENT | trusted_learned |
| layer-2-nine-node-3.4-pairs-003.md | TenantScopeEnforcement | SECURITY+AUTHORIZATION | trusted_learned |
| layer-2-nine-node-3.4-pairs-004.md | ServiceAccountSetup | AUTHORIZATION+API_CONTRACT | trusted_learned |
| layer-2-nine-node-3.4-pairs-005.md | TokenIntrospectionEndpoint | SECURITY+API_CONTRACT | trusted_learned |
| layer-2-nine-node-3.4-pairs-006.md | ScopeManagementTests | SECURITY+AUTHORIZATION+TEST | trusted_learned |
| layer-2-nine-node-3.4-pairs-007.md | OAuthClientRegistration | SECURITY+AUTHORIZATION+IMPLEMENT | trusted_learned |
| layer-2-nine-node-3.4-pairs-008.md | TenantGrantRevoke | AUTHORIZATION+API_CONTRACT | trusted_learned |
| layer-2-nine-node-3.4-pairs-009.md | ProbationaryRejected | SECURITY+TEST | probationary_rejected (5.1 fallback) |
| layer-2-nine-node-3.4-pairs-010.md | OAuthDocumentation | SECURITY+DOCUMENT | trusted_learned + generic 2.1 |
| layer-2-nine-node-3.4-pairs-011.md | FullOAuthSuite | SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT+TEST+DOCUMENT | mixed (5 flows) |
| layer-2-nine-node-3.4-pairs-012.md | Nine-node rule summary | (reference) | — |

---

## Key Teaching Points

1. **SECURITY+AUTHORIZATION dedup rule**: SECURITY and AUTHORIZATION both route to sub-phase 3.4 (SecurityVerification34Flow). When both predicates are present, exactly ONE routing decision to 3.4 is produced (not two). This is demonstrated in 18+ pairs across router files 002, 003, 009, 010.

2. **"service account" → AUTHORIZATION → 3.4**: "service account" is an AUTHORIZATION principal (account with scope), NOT an IMPLEMENT keyword. Routing to 4.1 alone is v6-03 corruption. Demonstrated in router file 011 and base held-out pair 003.

3. **knowledge_edge boundaries**: "login" is explicitly NOT in the SECURITY lexicon. "client portal" is colloquial and fires no predicate. "access denied" / "permission denied" are colloquial error phrases that do not fire AUTHORIZATION. "oauth", "token", "scope", "tenant" DO fire predicates. Demonstrated throughout layer 0-C (60 pairs in 12 files).

4. **v6-01 (predicate invention)**: Never invent OAUTH_FLOW, CLIENT_CREDS, TENANT_SCOPE, CLIENT_AUTH, SERVICE_ACCOUNT_VALIDATOR, RATE_LIMIT. Only canonical predicates exist: ARCHITECTURE, API_CONTRACT, IMPLEMENT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA.

5. **v6-02 (gap suppression)**: "scope", "tenant", "oauth", "authorization" in technical contexts always fire predicates. Do not suppress them when embedded in larger phrases like "client portal has oauth issues" — the technical keyword fires regardless of surrounding colloquial language.

6. **v6-03 (wrong routing)**: SECURITY+AUTHORIZATION → one 3.4 decision (not two). "scope" → AUTHORIZATION → 3.4 (not 4.1). "service account" → AUTHORIZATION → 3.4 (not 4.1). "refactor" verb → TransitionType: refactoring (not feature_addition).

7. **probationary_rejected (5.1)**: UnitTestingFlow51 has not passed Gate B in O1b. When TEST predicate routes to 5.1, GenericFallbackFlow is used. This is correct behavior. The 3.4 and other flows are unaffected.

8. **Node 01 A-side must be SelectedLogicalTransition**: Never use a raw user prompt string as the A-side for Node 01 (DomainSplitterUnit). SelectedLogicalTransition is the structured output of the predicate compiler — it is the correct and only valid input type for the nine-node pipeline.

9. **Canonical adapter names**: SecurityVerification34FlowAdapter (3.4), ApiContract33FlowAdapter (3.3), UniversalNineNodeDevelopmentFlow (4.1), UnitTestingFlow51 (5.1), DocumentationFlow21 (2.1), GenericFallbackFlow (fallback). No other adapter names are valid for O1b.

10. **QA is absolute last**: QA predicate appends QAGatingFlow73 as the final step after all domain flows complete. QA does not produce a routing decision and does not add a factory selection. QA alone produces no flows.
