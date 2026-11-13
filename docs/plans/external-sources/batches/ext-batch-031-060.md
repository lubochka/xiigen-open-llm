# External Source Batch — EXT-031 through EXT-060
## Sources: CWE-TOP25, OWASP-ASVS, OWASP-CHEAT-SHEETS, KUBERNETES-KEPS, MICROSOFT-LEARN-ARCH
## Date: 2026-05-04

---

### EXT-031 | CWE-79 | Cross-Site Scripting — Output Encoding

**Source:** CWE-79 "Improper Neutralization of Input During Web Page Generation (XSS)"; OWASP ASVS V5.3.3
**Source ID:** CWE-TOP25, OWASP-ASVS
**License:** MITRE CWE Terms of Use; CC-BY-SA-4.0
**Trust tier:** 1
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-031",
  "sourceIds": ["CWE-TOP25", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "xss-prevention",
  "architectureChallenge": "A web application renders user-supplied content in an HTML page. A code review finds that the content is inserted as innerHTML without encoding.",
  "input": {
    "a": "Evaluate the user content rendering path for XSS (Cross-Site Scripting) vulnerabilities",
    "b": "Code: `element.innerHTML = userComment;` where userComment comes from a database field populated by user submission; the database stores the raw submitted value without encoding",
    "c": "The page is an admin dashboard visible only to authenticated administrators; the comment field allows formatting"
  },
  "function": "Apply xssArbiter: evaluate whether the rendering is safe and determine remediation",
  "acceptedOutput": {
    "decision": "BLOCK deployment. Use textContent instead of innerHTML for untrusted content: `element.textContent = userComment;`. If HTML formatting must be preserved, pass the content through a sanitization library (DOMPurify) that removes dangerous tags and attributes before using innerHTML. Never use innerHTML with raw user input regardless of who is viewing the page."
  },
  "rejectedOutput": {
    "decision": "The dashboard is only accessible to administrators. XSS affecting only admins is low risk. Leave the innerHTML assignment for now; add sanitization in a future release."
  },
  "arbiters": ["xssOutputEncodingArbiter"],
  "whyAccepted": "CWE-79 rank 2 in CWE Top 25 (2023). Admin-only contexts are still XSS attack surfaces: stored XSS payloads can be injected by regular users and executed in the admin's browser, giving attackers admin-level access. The 'low risk because admin-only' argument inverts the severity — admin context makes XSS more dangerous, not less.",
  "whyRejected": "The 'admin-only' argument underestimates stored XSS. A regular user submits a malicious comment; an admin views the comment page; the payload executes in the admin's session. This is the standard stored XSS privilege escalation path.",
  "sourceAnchors": [
    { "sourceId": "CWE-TOP25", "anchor": "CWE-79 — rank 2 (2023); stored XSS allows attacker-controlled content to execute in victim's browser" },
    { "sourceId": "OWASP-ASVS", "anchor": "V5.3.3: Verify that context-aware output escaping is applied; V5.3.4: innerHTML is not used with untrusted content" }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

### EXT-032 | CWE-22 | Path Traversal in File Operations

**Source:** CWE-22 "Improper Limitation of a Pathname to a Restricted Directory (Path Traversal)"; OWASP ASVS V12.3.1
**Source ID:** CWE-TOP25, OWASP-ASVS
**License:** MITRE CWE Terms of Use; CC-BY-SA-4.0
**Trust tier:** 1
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-032",
  "sourceIds": ["CWE-TOP25", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "file-path-validation",
  "architectureChallenge": "A file download endpoint constructs a file path using a user-supplied filename parameter and serves the file from a designated directory.",
  "input": {
    "a": "Evaluate the file download endpoint for path traversal vulnerabilities",
    "b": "Endpoint: GET /files/download?name=report.pdf; implementation: `filepath = '/var/app/uploads/' + request.params.name`; the file is served if it exists",
    "c": "The uploads directory contains only user-uploaded files; the server's /etc and /var/app/secrets directories are on the same filesystem"
  },
  "function": "Apply pathTraversalArbiter: determine whether the path construction is safe",
  "acceptedOutput": {
    "decision": "BLOCK deployment. Two fixes required: (1) Use os.path.realpath() or Path.resolve() to canonicalize the constructed path; then verify that the canonicalized path starts with the expected base directory — reject any path that does not. (2) Validate the filename against an allowlist of permitted characters (alphanumeric, hyphen, underscore, period) before constructing the path. A request for `../../../../etc/passwd` must produce a rejection, not a file read."
  },
  "rejectedOutput": {
    "decision": "Strip `../` sequences from the filename before constructing the path. This prevents traversal using relative path components."
  },
  "arbiters": ["pathTraversalArbiter"],
  "whyAccepted": "CWE-22 rank 5 in CWE Top 25 (2023). Path canonicalization is the correct defense because traversal can use URL encoding (%2e%2e%2f), double encoding (%252e%252e%252f), or other representations that a string-strip approach misses. Canonicalize first, then verify the prefix — this handles all encoding variants.",
  "whyRejected": "Stripping `../` is bypassed by URL encoding: `..%2f` passes the strip and resolves to `../` by the OS. This is the canonical bypass for naive traversal filtering.",
  "sourceAnchors": [
    { "sourceId": "CWE-TOP25", "anchor": "CWE-22 — rank 5 (2023); canonicalization is the required defense" },
    { "sourceId": "OWASP-ASVS", "anchor": "V12.3.1: Verify that user-submitted filenames are validated and that path traversal characters are not accepted" }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

### EXT-033 | CWE-502 | Deserialization of Untrusted Data

**Source:** CWE-502 "Deserialization of Untrusted Data"; OWASP ASVS V1.5.2
**Source ID:** CWE-TOP25, OWASP-ASVS
**License:** MITRE CWE Terms of Use; CC-BY-SA-4.0
**Trust tier:** 1
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-033",
  "sourceIds": ["CWE-TOP25", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "deserialization-safety",
  "architectureChallenge": "An internal API endpoint deserializes a base64-encoded Java object from an HTTP request body to reconstruct a session context.",
  "input": {
    "a": "Evaluate the session reconstruction mechanism for deserialization vulnerabilities",
    "b": "Implementation: `Object session = ObjectInputStream.readObject(Base64.decode(request.getBody()));` where the request body is supplied by an external HTTP client",
    "c": "The endpoint is on an internal network but accessible from load balancer ingress; the Java application classpath includes several library jars that have known deserialization gadget chains (Log4j, Commons-Collections)"
  },
  "function": "Apply deserializationArbiter: evaluate whether native object deserialization of untrusted input is safe",
  "acceptedOutput": {
    "decision": "BLOCK deployment. Native Java object deserialization of untrusted data is inherently unsafe when the classpath contains gadget chain libraries. Replace with a data-format serialization approach: use JSON (Jackson) or Protocol Buffers for session data. Only deserialize typed, schema-validated data — never arbitrary object graphs from untrusted sources. If native serialization is required, use a deserialization filter (Java's ObjectInputFilter) that allowlists only the expected class types."
  },
  "rejectedOutput": {
    "decision": "The endpoint is on an internal network. External attackers cannot reach it directly. The internal-network placement is sufficient protection against deserialization attacks."
  },
  "arbiters": ["deserializationArbiter"],
  "whyAccepted": "CWE-502 rank 10 in CWE Top 25 (2023). Internal network placement does not prevent deserialization attacks: a compromised internal service, a SSRF (Server-Side Request Forgery) vulnerability in a public service, or a lateral movement chain gives an attacker access to the internal endpoint. Gadget chains (Commons-Collections, Spring, etc.) allow arbitrary code execution when deserialization runs.",
  "whyRejected": "Network perimeter is not a deserialization control. Once an attacker is on the internal network (through any vector), the deserialization endpoint is fully exploitable. Defense in depth requires that the endpoint is safe regardless of network position.",
  "sourceAnchors": [
    { "sourceId": "CWE-TOP25", "anchor": "CWE-502 — rank 10 (2023); gadget chains in common Java libraries enable RCE via deserialization" },
    { "sourceId": "OWASP-ASVS", "anchor": "V1.5.2: Verify that serialization is not used when communicating with untrusted clients; use allowlist-based deserialization" }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

### EXT-034 | OWASP-ASVS V7 | Logging — Sensitive Data Exclusion

**Source:** OWASP ASVS v4.0 V7.1.1 "Log Content Requirements"
**Source ID:** OWASP-ASVS
**License:** CC-BY-SA-4.0
**Trust tier:** 1
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-034",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "log-content-safety",
  "architectureChallenge": "A review of application logs finds that authentication request payloads are logged in full, including the password field.",
  "input": {
    "a": "Evaluate the authentication request logging implementation for sensitive data exposure",
    "b": "Log entry: `{\"timestamp\": \"2026-05-04T10:30:00Z\", \"event\": \"login_attempt\", \"payload\": {\"email\": \"user@example.com\", \"password\": \"Passw0rd!\"}}`; the logs are stored in Elasticsearch and retained for 90 days",
    "c": "The Elasticsearch cluster is accessible to the security team and all backend developers; log export is available to all engineers with production access"
  },
  "function": "Apply logContentArbiter: evaluate whether the log content is safe and determine remediation",
  "acceptedOutput": {
    "decision": "IMMEDIATE remediation required. Remove the password field from all log entries. Add a log sanitization step that strips credential fields (password, token, secret, api_key, credit_card, ssn) before the log entry is written. Rotate any passwords that were logged and may have been viewed. Implement a log content policy that explicitly enumerates which fields are permitted in logs."
  },
  "rejectedOutput": {
    "decision": "Restrict log access to the security team only. The logs are useful for debugging authentication issues. The access control on Elasticsearch mitigates the exposure."
  },
  "arbiters": ["logContentArbiter"],
  "whyAccepted": "ASVS V7.1.1 requires that sensitive data is not logged. Access control on the log store does not prevent credential exposure — it only reduces the audience. Logs are often exported, replicated, and backed up; access control on one store does not cover all destinations. The password appearing in logs means any engineer with log access has harvested a real user password, regardless of the Elasticsearch ACL.",
  "whyRejected": "Restricting access to the log store is a reduction, not an elimination of the risk. Logs are frequently copied to SIEM (Security Information and Event Management) systems, backup stores, and developer workstations for debugging. The credential is present in the log entry and will propagate to wherever the log entry propagates.",
  "sourceAnchors": [
    { "sourceId": "OWASP-ASVS", "anchor": "V7.1.1: Verify that the application does not log credentials or payment details; V7.1.2: Verify that the application does not log other sensitive data" }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-035 | OWASP-ASVS V13 | API Authentication — Service-to-Service

**Source:** OWASP ASVS v4.0 V9.2.1 "Server Communications Security"
**Source ID:** OWASP-ASVS
**License:** CC-BY-SA-4.0
**Trust tier:** 1
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-035",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "service-auth",
  "architectureChallenge": "Two internal microservices communicate over HTTP. The calling service passes a static API key in the X-Service-Key header. The key is shared across all instances of the calling service and never rotates.",
  "input": {
    "a": "Evaluate the service-to-service authentication mechanism",
    "b": "Authentication: static API key in X-Service-Key header; key is hardcoded in the calling service's deployment config; key has not been rotated in 18 months; the receiving service accepts any request with a valid key regardless of the caller's identity",
    "c": "Both services are inside the Kubernetes cluster; the key is stored as a Kubernetes Secret"
  },
  "function": "Apply serviceAuthArbiter: evaluate whether static shared API key is appropriate for service-to-service authentication",
  "acceptedOutput": {
    "decision": "Replace static API key with mTLS (mutual TLS) or OAuth 2.0 client credentials flow. mTLS: each service gets a certificate from an internal CA (Certificate Authority); the receiving service verifies the caller's certificate on every request. Client credentials: the calling service obtains a short-lived access token from an internal OAuth server; the receiving service validates the token. Either approach provides identity, not just authorization, and enables rotation without redeployment."
  },
  "rejectedOutput": {
    "decision": "The static API key is acceptable for internal services. Store it in a Kubernetes Secret and ensure the Secret is RBAC-protected. Rotate the key annually."
  },
  "arbiters": ["serviceAuthArbiter"],
  "whyAccepted": "ASVS V9.2.1 requires that backend services authenticate using certificates or tokens. A static shared API key provides no caller identity — any service that obtains the key can impersonate the calling service. Annual rotation is insufficient; key compromise is not detectable until after the fact. Short-lived tokens or certificate-based auth reduce the blast radius of any key compromise.",
  "whyRejected": "Kubernetes Secrets protect the key from unauthorized reads but do not solve the key-sharing problem. A shared static key cannot distinguish between the legitimate caller and a compromised service that obtained the key. RBAC on Secrets is a storage control, not an authentication control.",
  "sourceAnchors": [
    { "sourceId": "OWASP-ASVS", "anchor": "V9.2.1: Verify that authenticated connections are used between services; certificate-based auth or short-lived tokens preferred" }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-036 | KUBERNETES-KEPS | Readiness vs Liveness Probe Selection

**Source:** Kubernetes documentation — Probes; KEP-1847 PVC protection
**Source ID:** KUBERNETES-KEPS
**License:** Apache-2.0
**Trust tier:** 1
**Domain:** A_TOPOLOGY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-036",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "workload-health",
  "architectureChallenge": "A Kubernetes deployment uses the same HTTP health endpoint for both its liveness probe and its readiness probe. During a database migration, the health endpoint returns 503 for 90 seconds while migration runs.",
  "input": {
    "a": "Evaluate the probe configuration for a deployment whose health endpoint returns 503 during database migrations",
    "b": "Configuration: livenessProbe: GET /health, failureThreshold: 3, periodSeconds: 10; readinessProbe: GET /health, failureThreshold: 3, periodSeconds: 10; /health returns 503 for 90 seconds during migration",
    "c": "The deployment has 3 replicas; the liveness probe restarts any pod whose /health returns non-2xx 3 times in a row"
  },
  "function": "Apply probeSelectionArbiter: evaluate whether the probe configuration is correct for this deployment's behavior",
  "acceptedOutput": {
    "decision": "Separate the liveness and readiness probes onto different endpoints. Readiness probe: GET /ready — returns 503 during migration (correct; removes the pod from the load balancer during migration). Liveness probe: GET /live — returns 200 if the process is running and not deadlocked; never returns 503 during normal migration. This prevents Kubernetes from restarting healthy pods that are temporarily unable to serve traffic."
  },
  "rejectedOutput": {
    "decision": "Increase the liveness probe failure threshold to 15 so that pods are not restarted during the 90-second migration window."
  },
  "arbiters": ["probeSelectionArbiter"],
  "whyAccepted": "Liveness and readiness serve different purposes. Liveness: is the process alive and not deadlocked? Readiness: is the process ready to serve traffic? A pod that is healthy but temporarily unready (during migration) should be removed from the load balancer (readiness = false) but should NOT be restarted (liveness = true). Using the same endpoint for both collapses two orthogonal signals into one, making them impossible to distinguish.",
  "whyRejected": "Increasing the threshold is a workaround, not a fix. If the liveness probe threshold is 15 × 10 seconds = 150 seconds, then a genuinely deadlocked pod is not restarted for 150 seconds either. The threshold increase extends the detection window for real failures to accommodate normal temporary behavior.",
  "sourceAnchors": [
    { "sourceId": "KUBERNETES-KEPS", "anchor": "Kubernetes documentation §Configure Liveness, Readiness and Startup Probes — liveness detects deadlock; readiness controls load balancer membership" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-037 | KUBERNETES-KEPS | Rolling Update vs. Canary Deployment Selection

**Source:** Kubernetes documentation — Deployment strategies; KEP-2221 "Structured logging"
**Source ID:** KUBERNETES-KEPS
**License:** Apache-2.0
**Trust tier:** 1
**Domain:** A_TOPOLOGY
**Family:** 1 — userPrompt_to_machineCommands

**Draft row:**
```json
{
  "rowId": "EXT-037",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "deployment-strategy",
  "architectureChallenge": "A team needs to deploy a new version of a critical service to production. They want to minimize blast radius if the new version has a latent bug.",
  "input": {
    "a": "Choose the deployment strategy for releasing a new service version with minimum blast radius",
    "b": "Service: payment-service; current version: 2.3.1; new version: 2.4.0; change: new payment provider integration; risk: provider API behavior is untested at production load",
    "c": "The cluster has 10 replicas of payment-service; the service handles 500 requests/second; rollback must be possible within 5 minutes if an error spike is detected"
  },
  "function": "Decompose the deployment requirement into a set of commands implementing canary deployment",
  "acceptedOutput": {
    "decision": "Use canary deployment: (1) Deploy v2.4.0 as a new Deployment with 1 replica; (2) Route 10% of traffic to the canary (1 replica out of 10 total); (3) Monitor error rate and latency for 15 minutes; (4) If metrics are within baseline, gradually increase canary replicas to 5, then 10; (5) Decommission v2.3.1 replicas. Rollback: scale canary to 0 replicas and restore v2.3.1 replicas to 10."
  },
  "rejectedOutput": {
    "decision": "Use a rolling update: set maxUnavailable: 0, maxSurge: 2. Kubernetes will replace pods gradually. If errors appear, kubectl rollout undo deployment/payment-service will revert."
  },
  "arbiters": ["deploymentStrategyArbiter"],
  "whyAccepted": "A canary exposes the new version to a controlled traffic fraction before full rollout. A latent bug in the payment provider integration will appear in 10% of traffic first — detectable before it affects 100% of transactions. Rollback is instantaneous (scale canary to 0).",
  "whyRejected": "A rolling update replaces all pods eventually — by the time the full rollout completes, every transaction has been processed by the new version. Rollback requires another rolling update (adding latency to recovery). For a change with untested provider behavior, canary is the appropriate choice.",
  "sourceAnchors": [
    { "sourceId": "KUBERNETES-KEPS", "anchor": "Kubernetes Deployment documentation §Strategy — RollingUpdate vs. Blue/Green vs. Canary; traffic splitting for gradual rollout" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-038 | MICROSOFT-LEARN-ARCH | Event Sourcing — Snapshot Strategy

**Source:** Microsoft Learn Azure Architecture Patterns — Event Sourcing §Issues and considerations — snapshotting
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Domain:** F_REGISTRY_STORAGE
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-038",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "event-sourcing-snapshot",
  "architectureChallenge": "An event-sourced aggregate has accumulated 50,000 events over 3 years. Reconstructing the current state from the event stream takes 8 seconds. The system must decide whether to introduce snapshotting.",
  "input": {
    "a": "Decide whether to introduce snapshotting to the event-sourced aggregate given the current reconstruction latency",
    "b": "Aggregate: Order; event count: 50,000; reconstruction time: 8 seconds on cold read; snapshot has never been implemented; the aggregate handles 200 read requests per day",
    "c": "Read requests that reconstruct from the event stream block the API for 8 seconds; the API SLA is 1 second; the events table is immutable; snapshots would require a new schema and migration"
  },
  "function": "Apply eventSourcingSnapshotArbiter: decide whether the performance problem justifies snapshotting complexity",
  "acceptedOutput": {
    "decision": "INTRODUCE snapshotting. The 8-second reconstruction time exceeds the 1-second SLA by 8x. Define a snapshot schema that captures the aggregate's full state at a point in time. Snapshots are stored alongside events; on read, load the most recent snapshot and replay only events after the snapshot timestamp. Take a snapshot every 1,000 events. The snapshot does not replace the event log — it is a read optimization only."
  },
  "rejectedOutput": {
    "decision": "Cache the reconstructed aggregate in Redis with a 1-hour TTL. Bypass event stream reconstruction on cache hits. This avoids the snapshot schema and migration complexity."
  },
  "arbiters": ["eventSourcingFitArbiter"],
  "whyAccepted": "Snapshotting is the canonical solution to long event stream reconstruction. It is part of the event sourcing pattern design space specifically for this case. The event log remains intact and immutable; the snapshot is an optimization artifact.",
  "whyRejected": "Redis caching solves the read-path latency problem but introduces cache invalidation complexity. When the aggregate receives a new event, the cache must be invalidated immediately or serve stale data. Cache TTL of 1 hour means events applied in the last hour are invisible to cached reads. Snapshotting is structurally correct; caching is a workaround.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "Event Sourcing pattern §Issues and considerations — use snapshots when performance is impacted by loading and replaying a large number of events" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-039 | GOOGLE-AIP | Custom Method vs. Standard Method Selection

**Source:** Google AIP-136 "Custom methods" §Guidance
**Source ID:** GOOGLE-AIP
**License:** CC-BY-4.0
**Trust tier:** 1
**Domain:** B_ROUTING_SELECTION
**Family:** 1 — userPrompt_to_machineCommands

**Draft row:**
```json
{
  "rowId": "EXT-039",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "api-method-selection",
  "architectureChallenge": "An API needs to expose an 'archive' operation on a Document resource that marks the document as archived and removes it from default list results.",
  "input": {
    "a": "Design the API method for archiving a Document resource",
    "b": "Resource: Document; desired operation: mark as archived, exclude from default list results, operation is reversible (unarchive); the Document's data fields are unchanged by archiving",
    "c": "The API follows AIP guidelines; existing methods: Create, Get, List, Update, Delete"
  },
  "function": "Determine whether to use a standard Update method or a custom Archive method for this operation",
  "acceptedOutput": {
    "decision": "Use a custom method: POST /documents/{id}:archive. AIP-136 explicitly lists 'archive' as a use case for custom methods. A custom method name communicates the business intent clearly; an Update would not convey that this is a state transition, not a data edit. The inverse is POST /documents/{id}:unarchive."
  },
  "rejectedOutput": {
    "decision": "Use PATCH /documents/{id} with body `{\"archived\": true}`. Standard Update is sufficient; the 'archived' field is just another field on the Document resource."
  },
  "arbiters": ["apiMethodSelectionArbiter"],
  "whyAccepted": "AIP-136 states that custom methods are appropriate for state transitions that cannot be expressed naturally as a data update. 'Archive' is a state transition, not a field edit. Callers reading PATCH with `archived: true` may not understand the downstream effects (exclusion from list results). Custom methods are self-documenting.",
  "whyRejected": "PATCH with a boolean field treats a business-meaningful state transition as a data mutation. While functionally equivalent, it obscures the semantic meaning and gives no signal to callers that archiving has broader effects than setting one field.",
  "sourceAnchors": [
    { "sourceId": "GOOGLE-AIP", "anchor": "AIP-136 Custom methods §Guidance — use custom methods for operations that do not cleanly map to standard CRUD; archive/unarchive are explicitly listed examples" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-040 | OPENAPI-SPEC | API Schema Breaking Change Detection

**Source:** OpenAPI Specification 3.1.0 §4.8.24 Schema Object — deprecation field; §4.8.11 Path Item compatibility
**Source ID:** OPENAPI-SPEC
**License:** Apache-2.0
**Trust tier:** 1
**Domain:** H_GOVERNANCE
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-040",
  "sourceIds": ["OPENAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "schema-compatibility",
  "architectureChallenge": "A new API version removes a required request field that was previously marked as deprecated for 6 months. The API gateway is about to deploy the new schema. The system must decide whether to proceed or hold.",
  "input": {
    "a": "Validate the new API schema against the previous version before deployment",
    "b": "Previous schema: field `legacyId` is required, deprecated=true for 6 months. New schema: field `legacyId` is removed entirely; a new field `resourceId` is required in its place",
    "c": "The API has 40 known consumers; 5 consumers have been confirmed to have migrated to resourceId; 35 have not confirmed migration; the deployment window is today"
  },
  "function": "Apply apiVersioningArbiter and breakingChangeGate: evaluate whether the removal is safe to deploy today",
  "acceptedOutput": {
    "decision": "HOLD deployment. 35 of 40 consumers have not confirmed migration. Removing a required field is a breaking change regardless of how long it was deprecated. Deploy the field removal only when either: (a) all consumers confirm migration, or (b) the old field's traffic drops to zero in the API gateway metrics. Use API gateway request logging to measure actual usage of legacyId — if 0 requests include it, removal is safe."
  },
  "rejectedOutput": {
    "decision": "Proceed with deployment. The field was deprecated for 6 months, which is a reasonable migration window. Consumers who did not migrate in 6 months have had sufficient notice."
  },
  "arbiters": ["apiVersioningArbiter", "backwardCompatibilityArbiter"],
  "whyAccepted": "A deprecation notice is a communication, not a guarantee that all consumers have migrated. 35 of 40 consumers are unconfirmed — 'sufficient notice' does not mean 'actually migrated'. API gateway traffic metrics are the ground truth: if legacyId is still appearing in production requests, removing it breaks those callers. This is measurable and should be measured before deployment.",
  "whyRejected": "The 6-month window is a convention, not a contract. Removing a required field without confirming zero-traffic is the leading cause of production API breakages despite deprecation warnings. The rejected output treats the deprecation notice as equivalent to confirmed migration.",
  "sourceAnchors": [
    { "sourceId": "OPENAPI-SPEC", "anchor": "§4.8.24 Schema Object — deprecated field signals intent to remove; actual removal requires consumer migration confirmation" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-041 | PYTHON-PEPS | PEP Staged Rollout — Feature Flag Transition

**Source:** Python PEP 479 "Change StopIteration handling inside generators" §Transition plan
**Source ID:** PYTHON-PEPS
**License:** Public domain
**Trust tier:** 2
**Domain:** H_GOVERNANCE
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-041",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "backward-compatibility-migration",
  "architectureChallenge": "A language feature change in a platform breaks existing code that relies on the old behavior. The platform must decide how to roll out the change across its user base without immediate breakage.",
  "input": {
    "a": "Design the rollout strategy for a behavior change that breaks a category of existing code",
    "b": "Change: generators will raise RuntimeError instead of silently swallowing StopIteration; old code that relies on StopIteration propagation from inside a generator will break; new behavior is available as opt-in under `from __future__ import generator_stop`",
    "c": "Python 3.5 is current; the change will take full effect in Python 3.7; there are 2 years for the migration"
  },
  "function": "Apply backwardCompatibilityArbiter: evaluate the rollout strategy and determine whether it is appropriate",
  "acceptedOutput": {
    "decision": "The staged rollout is the correct approach. Phase 1 (Python 3.5): opt-in via `from __future__ import generator_stop`; old behavior remains default; a DeprecationWarning is raised on the old behavior. Phase 2 (Python 3.7): new behavior becomes default; old behavior is gone. This gives code owners 2 versions and 2 years to migrate. The __future__ import is the correct signaling mechanism: it is explicit and searchable."
  },
  "rejectedOutput": {
    "decision": "Make the new behavior the default in Python 3.6 with no opt-in period. Two years between Python versions is a long enough migration window. Code that breaks will be fixed when developers notice."
  },
  "arbiters": ["backwardCompatibilityArbiter"],
  "whyAccepted": "PEP 479's actual rollout (opt-in in 3.5, default in 3.7) follows the established PEP migration pattern. The __future__ import allows early adopters to opt in immediately and catch breakage at their own pace. The DeprecationWarning makes the issue discoverable before the breaking change takes effect. Code owners who ignore DeprecationWarnings for two versions have had full opportunity.",
  "whyRejected": "Making a breaking change default without an opt-in period means code breaks at upgrade time with no advance warning. 'Two years between versions' conflates the version release cadence with the user upgrade cadence — not all users upgrade immediately.",
  "sourceAnchors": [
    { "sourceId": "PYTHON-PEPS", "anchor": "PEP 479 §Transition plan — __future__ import for opt-in; DeprecationWarning in 3.5; default in 3.7" }
  ],
  "licenseConstraint": "none (public domain)",
  "trainingStatus": "candidate"
}
```

---

### EXT-042 | RUST-RFCS | Alternative Design Documentation

**Source:** Rust RFC 2229 "Closures Capture Disjoint Fields" §Alternatives
**Source ID:** RUST-RFCS
**License:** MIT
**Trust tier:** 2
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-042",
  "sourceIds": ["RUST-RFCS"],
  "sourceTrustTier": 2,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "design-alternatives",
  "architectureChallenge": "A technical proposal has been submitted for review. It proposes a specific design approach but the alternatives section lists only 'do nothing' as the alternative considered.",
  "input": {
    "a": "Evaluate the completeness of the alternatives analysis in the submitted proposal",
    "b": "Proposal: a system-level feature change; alternatives section: 'The alternative is to not make this change and keep the current behavior'; no other alternatives listed; no rationale for rejecting approaches like partial adoption, opt-in flags, or different implementation strategies",
    "c": "The proposal is for a production system change that affects all users; the reviewer has identified at least two alternative approaches that are not mentioned"
  },
  "function": "Apply proposalCompletenessArbiter: evaluate whether the alternatives analysis is sufficient for approval",
  "acceptedOutput": {
    "decision": "NOT READY for approval. 'Do nothing' is a valid but insufficient alternative. The alternatives section must include every materially different approach that was considered and explain why each was rejected. In the case of Rust RFC 2229, the RFC documents alternatives including: capturing the whole struct vs. disjoint field capture; the two-phase borrow approach; the opt-in closure annotation approach. Each is rejected with a specific technical reason. This is the standard."
  },
  "rejectedOutput": {
    "decision": "Approve the proposal. The alternatives section contains the required section header and one entry. The substance of the design is sound. Alternatives are inherently subjective."
  },
  "arbiters": ["proposalCompletenessArbiter"],
  "whyAccepted": "The alternatives section serves a specific purpose: to prove that the design space has been explored and the chosen approach is better than its alternatives. 'Do nothing' alone does not prove exploration. Future contributors re-open the same debate when they can't find evidence that their preferred alternative was considered.",
  "whyRejected": "A section header with one entry is not an alternatives analysis. The alternatives section is substantive, not ceremonial. Approving a proposal with an incomplete alternatives section externalizes the exploration cost to the reviewers and to future contributors.",
  "sourceAnchors": [
    { "sourceId": "RUST-RFCS", "anchor": "RFC 2229 §Alternatives — documents 3 alternative approaches each with rejection rationale; sets the standard for completeness" }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

### EXT-043 | MICROSOFT-LEARN-ARCH | Compensating Transaction Design

**Source:** Microsoft Learn Azure Architecture Patterns — Compensating Transaction §Context and problem; §Solution
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Domain:** A_TOPOLOGY
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-043",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "saga-compensation",
  "architectureChallenge": "A 3-step saga has committed steps 1 (inventory reserved) and 2 (payment charged). Step 3 (shipping label created) has failed permanently. The orchestrator must decide how to handle the failure.",
  "input": {
    "a": "Determine the correct response to a permanent failure in step 3 of a 3-step saga where steps 1 and 2 have already committed",
    "b": "Saga state: step 1 COMMITTED (inventory reserved: 3 units of SKU-42); step 2 COMMITTED (payment charged: $99.99 to card ending 1234); step 3 FAILED permanently (shipping provider unreachable; 3 retries exhausted)",
    "c": "Steps 1 and 2 cannot be rolled back using database ROLLBACK; they are committed in separate services; each service has a defined compensation API"
  },
  "function": "Apply sagaCompensationArbiter: determine the correct compensation path and the order of compensation operations",
  "acceptedOutput": {
    "decision": "Execute compensating transactions in reverse order of commitment: (1) Compensate step 2: issue a refund for $99.99 to card ending 1234 (call payment service refund API); (2) Compensate step 1: release inventory reservation for 3 units of SKU-42 (call inventory service release API). Compensation must be idempotent — if the refund call fails, retry it; do not stop compensation at step 2 without completing step 1. Record the saga's final state as COMPENSATED."
  },
  "rejectedOutput": {
    "decision": "Raise an alert to the operations team. They will manually refund the payment and release the inventory. Automated compensation is risky because the payment and inventory services may be in inconsistent states."
  },
  "arbiters": ["sagaCompensationArbiter"],
  "whyAccepted": "The Compensating Transaction pattern defines compensation as the standard recovery path for committed saga steps. Compensation in reverse order ensures that dependencies are respected. Idempotent compensation is required because the compensation call itself may fail and must be retried. Manual compensation via operations team is error-prone, undocumented, and not retried automatically.",
  "whyRejected": "Manual compensation does not scale, is not auditable as a system action, and depends on the operations team being available at the time of failure. The risk argument ('inconsistent states') applies equally to manual compensation — if the services are inconsistent, the manual operator faces the same inconsistency.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "Compensating Transaction pattern §Solution — execute compensating operations in the reverse order of the original operations; compensation must be idempotent" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-044 | OPENTELEMETRY-SPEC | Semantic Convention Naming

**Source:** OpenTelemetry Specification — semantic conventions §Attribute naming; specification/common/README.md
**Source ID:** OPENTELEMETRY-SPEC
**License:** Apache-2.0
**Trust tier:** 1
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-044",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "observability-naming",
  "architectureChallenge": "A service emits spans with custom attribute names that don't follow OTel semantic conventions. A vendor observability tool fails to correlate these spans with its built-in dashboards.",
  "input": {
    "a": "Evaluate the span attribute naming against OpenTelemetry semantic conventions",
    "b": "Current attributes: `Service.Name: payment-service`, `Http.Method: POST`, `Database.Type: postgresql`, `Error.Message: connection refused`; OTel conventions: `service.name`, `http.request.method`, `db.system`, `error.message`",
    "c": "The service uses the OTel Go SDK; the vendor observability tool uses OTel semantic conventions to populate its HTTP dashboard; the HTTP dashboard shows no data for this service"
  },
  "function": "Apply semConvNamingArbiter: evaluate the attribute naming and determine the required change",
  "acceptedOutput": {
    "decision": "Rename attributes to follow OTel semantic conventions. Use the OTel SDK's built-in instrumentation libraries (otelhttptrace, otelgrpc, otelsql) where available — they emit the correct convention-compliant attributes automatically. For custom attributes, follow the naming pattern: namespace.attribute (lowercase, dot-separated). The mapping: `Service.Name` → `service.name`; `Http.Method` → `http.request.method`; `Database.Type` → `db.system`; `Error.Message` → `error.message`."
  },
  "rejectedOutput": {
    "decision": "Create a custom vendor pipeline that maps the custom attribute names to OTel convention names before ingestion. The application attribute names are stable and changing them would require updating all application code."
  },
  "arbiters": ["semConvNamingArbiter"],
  "whyAccepted": "The OTel SDK instrumentation libraries emit convention-compliant attributes by default — adopting them eliminates the naming problem without manual renaming. Convention compliance is the only durable solution: vendor pipelines that remap attributes add a mapping layer that must be maintained as conventions evolve.",
  "whyRejected": "A vendor-side remap pipeline is a workaround that adds operational complexity and must be updated every time a new attribute is added. Using OTel instrumentation libraries directly is simpler and produces correct output without a remap layer.",
  "sourceAnchors": [
    { "sourceId": "OPENTELEMETRY-SPEC", "anchor": "specification/common/README.md §Attribute naming convention — lowercase, dot-separated; semantic conventions are the shared vocabulary for tooling" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-045 | NIST-SSDF | PO Group — Security Roles and Training

**Source:** NIST SSDF v1.1 PO.2 "Implement Roles and Responsibilities" §Practices
**Source ID:** NIST-SSDF
**License:** Public domain (NIST)
**Trust tier:** 1
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-045",
  "sourceIds": ["NIST-SSDF"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "sdlc-governance",
  "architectureChallenge": "A security review finds that the development team has no defined security roles, no security champion, and no documented security training requirement for engineers who write authentication code.",
  "input": {
    "a": "Evaluate the team's software security governance posture against SDLC requirements",
    "b": "Current state: 8-engineer team; no security champion assigned; security reviews are ad-hoc; no documented security training requirement; authentication code is written by whoever is available; penetration testing is done once per year by an external vendor",
    "c": "The team builds a SaaS application handling financial data; the application is subject to SOC 2 Type II compliance requirements"
  },
  "function": "Apply sdlcGovernanceArbiter: evaluate whether the current governance posture meets SSDF PO.2 and determine remediation",
  "acceptedOutput": {
    "decision": "REMEDIATE before next SOC 2 audit. Required actions per SSDF PO.2: (1) Assign a security champion who owns security review for each release; (2) Define a security training requirement for engineers who write authentication, cryptography, or payment code (minimum: OWASP Top 10, secure coding for the language stack); (3) Establish a documented security review gate before any code touching auth or payment is merged; (4) Annual penetration testing is insufficient for SOC 2 Type II — add threat modeling for new features and quarterly internal security reviews."
  },
  "rejectedOutput": {
    "decision": "Annual penetration testing provides sufficient security coverage. Security is everyone's responsibility — no dedicated security champion is needed. SOC 2 Type II requires controls, not specific roles."
  },
  "arbiters": ["sdlcGovernanceArbiter"],
  "whyAccepted": "NIST SSDF PO.2 requires defined and assigned security roles with documented responsibilities. 'Everyone is responsible for security' is equivalent to 'nobody is responsible for security' — when a security task belongs to everyone, it is done by no one. Annual penetration testing is a point-in-time assessment; it does not provide ongoing detection of vulnerabilities introduced between tests.",
  "whyRejected": "The rejected output conflates shared responsibility (good principle) with assigned accountability (required for compliance). SOC 2 Type II controls require evidence of consistent process; ad-hoc security review does not produce auditable evidence.",
  "sourceAnchors": [
    { "sourceId": "NIST-SSDF", "anchor": "PO.2.1: Create new roles and alter responsibilities for existing roles; PO.2.2: Provide role-based security training" }
  ],
  "licenseConstraint": "none (NIST public domain)",
  "trainingStatus": "candidate"
}
```

---

### EXT-046 | DOTNET-MICROSERVICES | Database per Service Ownership

**Source:** .NET Microservices Architecture Guide §Using a database per microservice
**Source ID:** DOTNET-MICROSERVICES
**License:** CC-BY-4.0
**Trust tier:** 1
**Domain:** F_REGISTRY_STORAGE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-046",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "database-ownership",
  "architectureChallenge": "A microservice architecture has two services (Order and Inventory) that both write directly to the same shared database schema. The Order service reads inventory counts from the Inventory table to decide whether to accept an order.",
  "input": {
    "a": "Evaluate the data ownership architecture for a system where two services share a database schema",
    "b": "Architecture: OrderService reads from orders table (owns) and inventory table (reads cross-service); InventoryService writes to inventory table (owns) and reads from orders table (reads for fulfillment checks); shared PostgreSQL database; shared schema",
    "c": "Both services are deployed independently; the inventory schema was changed last month without notifying the order service team; the order service broke on deployment because the column name changed"
  },
  "function": "Apply databaseOwnershipArbiter: evaluate whether cross-service schema access is acceptable and determine remediation",
  "acceptedOutput": {
    "decision": "Separate data ownership. Each service must own its data: OrderService owns the orders schema; InventoryService owns the inventory schema. Cross-service data access must go through the owning service's API, not direct database queries. OrderService calls InventoryService GET /inventory/{sku}/availability — it does not read the inventory table. Schema changes by InventoryService then only require API contract versioning, not cross-service schema coordination."
  },
  "rejectedOutput": {
    "decision": "Keep the shared database but establish a schema change review process. All schema changes must be reviewed by both teams. The review process prevents the coordination failure that caused last month's breakage."
  },
  "arbiters": ["databaseOwnershipArbiter"],
  "whyAccepted": "The .NET Microservices Guide explicitly identifies shared database schema as a primary anti-pattern in microservice architecture. Service independence is the core value of microservices; a shared schema creates tight coupling at the data layer. A schema change by one team cannot be deployed without coordinating with all teams that share the schema.",
  "whyRejected": "A review process addresses the symptom (lack of coordination) not the cause (shared schema). Review processes are compliance overhead; they do not prevent breakage if a schema change is approved but a consuming service does not update in time.",
  "sourceAnchors": [
    { "sourceId": "DOTNET-MICROSERVICES", "anchor": "§Using a database per microservice — each microservice must own its data; cross-service data access through APIs, not shared schema" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-047 | EFFECTIVE-GO | Goroutine Leak Detection

**Source:** Effective Go §Goroutines and channels; §The Go concurrency pattern — leak prevention
**Source ID:** EFFECTIVE-GO
**License:** CC-BY-3.0
**Trust tier:** 1
**Domain:** D_EXECUTION_APPLIER
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-047",
  "sourceIds": ["EFFECTIVE-GO"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "concurrency-correctness",
  "architectureChallenge": "A Go service spawns goroutines to process requests. A goroutine is started but has no mechanism to terminate if the calling context is cancelled. Under load, goroutine count grows unboundedly.",
  "input": {
    "a": "Evaluate a goroutine launch pattern for resource leak risk",
    "b": "Code: `go func() { result := expensiveOperation(); resultChan <- result }()`; the goroutine writes to resultChan; if the caller times out and stops reading from resultChan, the goroutine blocks forever on the channel send",
    "c": "The function is called 500 times per second; timeouts occur in 2% of calls; after 30 minutes, the goroutine count exceeds 10,000"
  },
  "function": "Apply goroutineLeakArbiter: evaluate whether the goroutine pattern is safe and determine remediation",
  "acceptedOutput": {
    "decision": "Fix the goroutine to be cancellable. Use a buffered channel (size 1) so the goroutine can send even if the caller has moved on: `resultChan := make(chan Result, 1)`. Or pass the caller's context into the goroutine and use select to handle cancellation: `select { case resultChan <- result: case <-ctx.Done(): return }`. The goroutine must have a termination path for every case, not just the happy path."
  },
  "rejectedOutput": {
    "decision": "Add a goroutine count metric and alert when it exceeds 5,000. The alert gives the operations team time to restart the service before the leak becomes critical."
  },
  "arbiters": ["goroutineLeakArbiter"],
  "whyAccepted": "Effective Go §Goroutines: every goroutine must have a termination condition. A goroutine blocked on a channel send is a memory leak — the goroutine's stack is allocated and never freed. The buffered channel or context cancellation approach ensures the goroutine terminates in all cases, not just the happy path.",
  "whyRejected": "Alerting on goroutine count treats the symptom, not the cause. The service still leaks goroutines; the alert just tells you when to restart. Restarting does not fix the leak; it only resets the counter until the next spike.",
  "sourceAnchors": [
    { "sourceId": "EFFECTIVE-GO", "anchor": "§Goroutines — goroutines are cheap but not free; a goroutine that never terminates is a resource leak; context.Done() is the standard cancellation mechanism" }
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-048 | AOSA-VOL1 | Nginx Event Loop vs. Thread per Connection

**Source:** Architecture of Open Source Applications vol. 1 — Nginx chapter §The Architecture of Nginx
**Source ID:** AOSA-VOL1
**License:** CC-BY-3.0
**Trust tier:** 2
**Domain:** A_TOPOLOGY
**Family:** 1 — userPrompt_to_machineCommands

**Draft row:**
```json
{
  "rowId": "EXT-048",
  "sourceIds": ["AOSA-VOL1"],
  "sourceTrustTier": 2,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "concurrency-model-selection",
  "architectureChallenge": "A new HTTP proxy service must handle 10,000 concurrent connections with primarily I/O-bound workload (file serving, proxying to backends). The team must choose a concurrency model.",
  "input": {
    "a": "Select the concurrency model for a high-concurrency HTTP proxy with primarily I/O-bound workload",
    "b": "Workload: 10,000 concurrent connections; operations: read file from disk, proxy to backend service, write response; CPU operations: minimal (no heavy computation); team has experience with both thread-per-connection and event loop models",
    "c": "Memory constraint: 8 GB RAM; each OS thread requires 1–8 MB stack; event loop model has ~1 KB overhead per connection"
  },
  "function": "Determine the appropriate concurrency model and the rationale for the choice",
  "acceptedOutput": {
    "decision": "Use an event loop (async I/O) model. 10,000 concurrent threads would require 10–80 GB of RAM for thread stacks alone — exceeding the memory budget. The event loop model handles 10,000 connections with approximately 10 MB overhead. For I/O-bound workloads, threads spend most of their time blocked waiting for I/O; the event loop multiplexes I/O events across a small number of worker threads. This is the architectural decision that enabled Nginx to outperform Apache under high concurrency."
  },
  "rejectedOutput": {
    "decision": "Use a thread-per-connection model. Threads simplify the programming model — each connection has its own call stack and blocking I/O calls are natural. Add connection limiting to cap concurrent connections at a manageable number."
  },
  "arbiters": ["concurrencyModelArbiter"],
  "whyAccepted": "The AOSA Nginx chapter documents the C10K problem: thread-per-connection cannot scale to 10,000 concurrent connections on commodity hardware because thread stack memory requirements are prohibitive. The event loop model was specifically designed for this workload class.",
  "whyRejected": "Thread-per-connection with a connection cap does not achieve the stated requirement of 10,000 concurrent connections. Setting a connection cap defeats the requirement. The thread model is appropriate for CPU-bound workloads; it is wrong for high-concurrency I/O-bound workloads.",
  "sourceAnchors": [
    { "sourceId": "AOSA-VOL1", "anchor": "Nginx chapter §The Architecture of Nginx — event-driven architecture chosen specifically for C10K problem; thread-per-connection prohibitive at scale" }
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-049 | SYSTEM-DESIGN-PRIMER | Cache Invalidation Strategy

**Source:** System Design Primer §Caching — Cache-aside, Write-through, Write-behind; §When to update the cache
**Source ID:** SYSTEM-DESIGN-PRIMER
**License:** CC-BY-4.0
**Trust tier:** 2
**Domain:** F_REGISTRY_STORAGE
**Family:** 1 — userPrompt_to_machineCommands

**Draft row:**
```json
{
  "rowId": "EXT-049",
  "sourceIds": ["SYSTEM-DESIGN-PRIMER"],
  "sourceTrustTier": 2,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "cache-strategy",
  "architectureChallenge": "A product catalog service has high read load and infrequent writes. The team must select a caching strategy that minimizes stale reads after catalog updates.",
  "input": {
    "a": "Select a caching strategy for a product catalog with high read load and infrequent writes",
    "b": "Workload: 10,000 reads/second, 50 writes/second; data: product catalog (name, price, description, availability); write pattern: bulk updates during maintenance windows (10pm–2am); consistency requirement: price updates must be visible within 5 minutes",
    "c": "Current infrastructure: Redis cache, PostgreSQL database; cache TTL is currently 24 hours; price changes are delayed by up to 24 hours reaching customers"
  },
  "function": "Decompose the caching requirement into a caching strategy and cache invalidation approach",
  "acceptedOutput": {
    "decision": "Use cache-aside with explicit invalidation on write. When a product is updated in PostgreSQL, immediately invalidate its cache entry: `redis.delete(product:{id})`. The cache TTL can remain at 24 hours as a safety net, but explicit invalidation ensures updates are visible within the next read. For bulk maintenance window updates: invalidate all affected keys immediately after the batch write completes. This gives consistent visibility within one request cycle, not 24 hours."
  },
  "rejectedOutput": {
    "decision": "Reduce the cache TTL from 24 hours to 5 minutes. This ensures price updates are visible within 5 minutes without changing the invalidation logic."
  },
  "arbiters": ["cacheStrategyArbiter"],
  "whyAccepted": "Explicit invalidation on write is the correct pattern for writes that are infrequent but must be immediately visible. Reducing TTL to 5 minutes means the database receives 12x more read queries — for a 10,000 reads/second workload, a 5-minute TTL causes cache misses that send 10,000 queries to the database per invalidation cycle.",
  "whyRejected": "TTL reduction treats all keys as equally likely to become stale. For a catalog where 99% of products are unchanged in any 5-minute window, a 5-minute TTL causes massive unnecessary database load. The product that changed needs immediate invalidation; the 99% that did not change should keep their long TTL.",
  "sourceAnchors": [
    { "sourceId": "SYSTEM-DESIGN-PRIMER", "anchor": "§Caching — cache-aside: application reads from cache, falls back to database on miss; explicit invalidation on write for write-infrequent data" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-050 | DOCKER-BEST-PRACTICES | Multi-Stage Build Security

**Source:** Docker build best practices §Use multi-stage builds; §Minimize the number of layers
**Source ID:** DOCKER-BEST-PRACTICES
**License:** Apache-2.0
**Trust tier:** 1
**Domain:** D_EXECUTION_APPLIER
**Family:** 2 — machineCommand_to_machineActionItems

**Draft row:**
```json
{
  "rowId": "EXT-050",
  "sourceIds": ["DOCKER-BEST-PRACTICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "container-build",
  "architectureChallenge": "A Dockerfile builds a Node.js application in a single stage. The final image includes the full node_modules tree, npm, the Node.js compiler toolchain, and all development dependencies. The image is 1.8 GB.",
  "input": {
    "a": "Reduce the attack surface and size of the production container image for a Node.js application",
    "b": "Dockerfile: single FROM node:20 stage; RUN npm install (installs all deps including devDependencies); COPY . .; CMD ['node', 'server.js']; final image size: 1.8 GB; image contains: npm, node gyp, Python, make, build-essential",
    "c": "The production image runs in Kubernetes; the application only needs the compiled output and runtime dependencies; devDependencies are only needed during the build step"
  },
  "function": "Decompose the image optimization requirement into multi-stage build action items",
  "acceptedOutput": {
    "decision": "Implement a multi-stage build with three stages: (1) BUILD stage: FROM node:20 as builder; copy package*.json; RUN npm ci --include=dev; copy source; run tests and build. (2) DEPS stage: FROM node:20 as deps; copy package*.json; RUN npm ci --omit=dev (production deps only). (3) PRODUCTION stage: FROM node:20-alpine as production; copy --from=deps /app/node_modules; copy --from=builder /app/dist; CMD ['node', 'dist/server.js']. Final image: ~150 MB. Build tools and devDependencies are not in the production layer."
  },
  "rejectedOutput": {
    "decision": "Run `npm prune --production` at the end of the Dockerfile to remove devDependencies before the final layer. This reduces image size."
  },
  "arbiters": ["containerBuildArbiter"],
  "whyAccepted": "Docker's multi-stage build creates a clean production image that never contains build tools. The production image inherits only the files explicitly copied from prior stages. `npm prune` in a single-stage Dockerfile still creates intermediate layers containing devDependencies; those layers are in the image's history even if not in the final layer.",
  "whyRejected": "Single-stage with `npm prune` leaves the build toolchain in the image layers, increasing attack surface and image size. The pruned layer is smaller but the image history still contains the pre-prune layer with all devDependencies.",
  "sourceAnchors": [
    { "sourceId": "DOCKER-BEST-PRACTICES", "anchor": "§Use multi-stage builds — separating build environment from production environment; production image should not contain build tools" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```
