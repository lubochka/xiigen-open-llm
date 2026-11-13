# Layer 0-C — Knowledge Edge Pairs (036)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-036-001 — Round 2 boundary summary: integration verbs vs construction verbs

```
source_id: xiigen-b12-0c-036-001
pair_kind: knowledge_edge_reference
```

### Integration verbs (fire API_CONTRACT only — NOT IMPLEMENT)

| Verb | Fires | Does NOT fire |
|------|-------|---------------|
| wire up | API_CONTRACT | IMPLEMENT |
| connect | API_CONTRACT | IMPLEMENT |
| expose | API_CONTRACT | IMPLEMENT |
| publish (endpoint/API) | API_CONTRACT | IMPLEMENT |
| integrate (with API co-occurrence) | API_CONTRACT | IMPLEMENT |

### Construction verbs (fire IMPLEMENT — also trigger API_CONTRACT when API context present)

| Verb | Fires | TransitionType effect |
|------|-------|----------------------|
| implement | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |
| build | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |
| create | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |
| write | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |
| develop | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |
| scaffold | IMPLEMENT + API_CONTRACT (if API present) | feature_addition |

### Documentation verbs (fire DOCUMENT — NOT IMPLEMENT)

| Verb | Fires | Does NOT fire |
|------|-------|---------------|
| define | DOCUMENT | IMPLEMENT |
| specify | DOCUMENT | IMPLEMENT |
| document | DOCUMENT | IMPLEMENT |
| describe | DOCUMENT | IMPLEMENT |
| update (specification) | DOCUMENT | IMPLEMENT |

---

## B12-0C-036-002 — Round 2 knowledge edge: rules exercised in files 025–036

```
source_id: xiigen-b12-0c-036-002
pair_kind: knowledge_edge_reference
```

Rules exercised in knowledge-edge files 025–036:

| File | Primary rule |
|------|-------------|
| 025 | GraphQL schema co-occurrence; design dual-fire; wire up integration verb |
| 026 | gRPC connect = integration; gRPC publish = not IMPLEMENT; gRPC implement = both predicates |
| 027 | WebSocket integrate co-occurrence; contract modifier rule; build = IMPLEMENT |
| 028 | event contract modifier; publish schema = not IMPLEMENT; build + define = both predicates |
| 029 | payload not independent; async callback domain words; request in API context |
| 030 | idempotent fires API_CONTRACT; versioned = not IMPLEMENT; authentication unconditional |
| 031 | route fires unconditionally; rate limiting via REST triggers; API key authentication = SECURITY |
| 032 | OAuth fires SECURITY once; contract modifier rule; multi-version strategy |
| 033 | cache-control contract; deprecated not IMPLEMENT; auto-generating not IMPLEMENT |
| 034 | API_CONTRACT+SECURITY+DOCUMENT triple; tenant qualifier; backward compatible upgrade |
| 035 | expose = API_CONTRACT only; OAuth+authentication = one SECURITY; write schema = IMPLEMENT |
| 036 | Integration vs construction vs documentation verb summary; rules index for 025–036 |

---

## B12-0C-036-003 — Key new rule additions in Round 2

```
source_id: xiigen-b12-0c-036-003
pair_kind: knowledge_edge_reference
```

New rules confirmed in Round 2 (files 025–036):

B12-R17b (wire up, connect): These are integration verbs. They fire API_CONTRACT when used with
API context (endpoint, api, service). They do NOT fire IMPLEMENT.

B12-R17c (expose, publish endpoint): These are release/publication verbs. They fire API_CONTRACT.
They do NOT fire IMPLEMENT.

B12-R25 (domain qualifiers): Words like "tenant-scoped", "multi-version", "backward compatible",
"async callback", "bulk import", "paginated" are domain qualifiers. They do NOT independently fire
any predicate. API_CONTRACT fires via protocol terms (api, rest, graphql, endpoint, contract, route).

B12-R26 (request in API context): "request" fires API_CONTRACT only when API co-occurrence
(api/endpoint/rest/graphql/grpc/webhook) is present. "request" alone does NOT fire API_CONTRACT.

B12-R27 (SECURITY multiplicity): Multiple SECURITY triggers ("oauth" + "authentication") in one
request produce ONE SECURITY predicate in CompiledPredicateIds, not two.
