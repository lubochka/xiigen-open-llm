# External Training Rows — Batch 3
## Row IDs: EXT-051 through EXT-070
## Date: 2026-05-04
## Sources: Groups 1–2 (CWE, ASVS), Groups 8–15 (new in expanded plan)

---

## EXT-051

```json
{
  "rowId": "EXT-051",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "memory-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Evaluate this C result for memory safety: a function receives a byte count from a user-supplied length field and passes it directly to memcpy as the copy size without checking whether the destination buffer is large enough to hold that many bytes.",
    "b": "memcpy(dest, src, user_supplied_length); where dest is a fixed 256-byte stack buffer and user_supplied_length comes from a network packet header field with no upper-bound check.",
    "c": "Context: arbitrating a Node 4 result for a packet parser. CWE-787 (Out-of-bounds Write) is rank 1 in CWE Top 25 2023. The arbiter must check whether the write destination has sufficient capacity before the copy executes."
  },
  "function": "Apply outOfBoundsWriteArbiter: verify that every write into a fixed buffer is bounded by the buffer size, not by an externally controlled length value.",
  "acceptedOutput": {
    "decision": "REJECT. The memcpy size operand is user_supplied_length, which is read directly from a network packet header with no upper-bound check. The destination buffer is 256 bytes. Any packet header field value above 256 causes memcpy to write past the end of the stack buffer. CWE-787 (Out-of-bounds Write) applies. Stack buffer overflows at this location enable arbitrary code execution via return address overwrite.",
    "reprocessInstruction": "Add an explicit capacity check: if (user_supplied_length > sizeof(dest)) { return ERROR_BUFFER_TOO_SMALL; } before the memcpy call. The bound must be the destination capacity, not the source length."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The packet parser validates that the length field is non-zero and positive before calling memcpy, which prevents null-size copies."
  },
  "arbiters": ["outOfBoundsWriteArbiter"],
  "whyAccepted": "CWE-787 requires bounding writes by the destination capacity. A non-zero check on the source length does not enforce this — a length of 512 is positive and non-zero but still overflows a 256-byte destination.",
  "whyRejected": "The rejected output conflates 'non-zero length' with 'safe length'. The arbiter criterion is destination-capacity comparison, not source-length sign.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/787.html",
      "section": "CWE-787 Extended Description and Demonstrative Examples",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-052

```json
{
  "rowId": "EXT-052",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "memory-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result: a C function reads from a buffer using an index derived from subtraction of two user-controlled offset values — (end_offset - start_offset) — and uses the result as an array subscript without checking that start_offset is less than end_offset.",
    "b": "data[end_offset - start_offset] where both end_offset and start_offset are read from untrusted input. If start_offset > end_offset the subtraction wraps to a large positive value on unsigned arithmetic, producing an out-of-bounds read.",
    "c": "Context: CWE-125 (Out-of-bounds Read) is rank 4 in CWE Top 25 2023. Out-of-bounds reads expose adjacent memory content, enabling information disclosure and potential heap/stack layout leaks that assist further exploitation."
  },
  "function": "Apply outOfBoundsReadArbiter: verify that array subscripts derived from user-controlled arithmetic are validated for correct ordering before use.",
  "acceptedOutput": {
    "decision": "REJECT. The subscript (end_offset - start_offset) is computed with unsigned arithmetic and no prior ordering check. When start_offset > end_offset, unsigned subtraction wraps to a large value and the resulting read is far outside the buffer. CWE-125 (Out-of-bounds Read) applies. The information disclosure consequence is immediate — the read returns whatever bytes reside at the wrapped address.",
    "reprocessInstruction": "Add an ordering check before computing the subscript: if (start_offset >= end_offset || end_offset - start_offset >= buffer_size) { return ERROR_INVALID_RANGE; }. The check must precede the subtraction."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The code validates that both end_offset and start_offset are less than the total buffer length, so the individual values are safe."
  },
  "arbiters": ["outOfBoundsReadArbiter"],
  "whyAccepted": "CWE-125 requires that the computed subscript — not just the individual inputs — is within bounds. Validating each offset independently does not prevent their combination from producing an out-of-bounds value when ordering is violated.",
  "whyRejected": "The rejected output validates parts but not the computed combination. A subscript from a difference of valid individual offsets can still be invalid when the ordering relation between them is wrong.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/125.html",
      "section": "CWE-125 Extended Description and Relationships",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-053

```json
{
  "rowId": "EXT-053",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "memory-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this C++ result: an object is freed in an error handler, but a second code path in a catch block also calls free on the same pointer under a different exception condition. Both paths can execute in sequence if the first exception is re-thrown.",
    "b": "Widget* w = new Widget(); try { w->init(); } catch (InitError&) { delete w; throw; } catch (NetworkError&) { delete w; } — if init() throws InitError which re-throws and NetworkError is caught by an outer handler that also deletes w, the pointer is freed twice.",
    "c": "Context: CWE-416 (Use After Free) and related double-free condition. Rank 6 in CWE Top 25 2023. Double-free corrupts allocator metadata and enables heap exploitation."
  },
  "function": "Apply useAfterFreeArbiter: verify that every object has exactly one code path responsible for its deallocation, and that no code path can dereference or free a pointer after its owning path has already freed it.",
  "acceptedOutput": {
    "decision": "REJECT. Two catch blocks both call delete on the same pointer w. When InitError is thrown and re-thrown, the outer NetworkError handler also executes delete w. The pointer is freed twice. CWE-416 conditions apply — the allocator metadata is corrupted by the double free, enabling heap manipulation. The correct pattern is single ownership: assign w to a std::unique_ptr<Widget> immediately after construction; the smart pointer destructs it exactly once regardless of which exception path exits.",
    "reprocessInstruction": "Replace raw pointer management with std::unique_ptr<Widget> w = std::make_unique<Widget>(). Remove all manual delete calls. The destructor fires exactly once when the scope exits."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Each catch block handles a different exception type, so they cannot both execute for the same exception."
  },
  "arbiters": ["useAfterFreeArbiter"],
  "whyAccepted": "CWE-416 analysis requires tracing all execution paths through exception handlers, including re-throw chains. The rejected output examines only single-exception scenarios and misses the re-throw path.",
  "whyRejected": "Different catch clauses can execute on different throws of the same object's lifetime. The rejected output assumes mutual exclusion that does not hold when InitError re-throws and the stack unwinds through the NetworkError handler.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/416.html",
      "section": "CWE-416 Extended Description and Common Consequences",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-054

```json
{
  "rowId": "EXT-054",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "log-processing",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this result for log processing conformance: the application forwards all log entries to a centralized logging service. When the logging service is unavailable, the application drops the log call and continues processing the user request without error.",
    "b": "Logger.log(event) is wrapped in try/catch; if the logging endpoint returns a 503 the exception is swallowed and the application returns HTTP 200 to the user. No alert is raised on logging failure.",
    "c": "Context: OWASP ASVS V7.2 — Log Processing requirements. V7.2.1 requires that logging infrastructure failures do not suppress application processing, but V7.2.2 requires that logging failures are themselves logged or alerted on. Silent swallowing of logging errors violates V7.2.2."
  },
  "function": "Apply logProcessingArbiter (ASVS V7.2): verify that logging failures are surfaced via an alternative alerting path, not silently discarded.",
  "acceptedOutput": {
    "decision": "REJECT on V7.2.2. The logging exception is caught and discarded with no alert, no fallback log destination, and no metric increment. The application correctly continues serving users (satisfying V7.2.1), but the logging failure is invisible to operators. A sustained logging outage becomes undetectable, defeating audit trail requirements.",
    "reprocessInstruction": "Add a fallback path on logging failure: increment a 'logging_failure_total' counter in the metrics system, and write a minimal structured entry to stderr or a local file. The fallback does not need to be as rich as the primary log — it needs only to signal that primary logging is down."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. ASVS V7.2.1 requires that logging failures do not prevent application operation. The application continues serving requests when the logging service is unavailable, satisfying this requirement."
  },
  "arbiters": ["logProcessingArbiter"],
  "whyAccepted": "ASVS V7.2 has two distinct requirements. The accepted output correctly cites V7.2.1 (application continues) but ignores V7.2.2 (logging failure itself must be observable). Both must pass.",
  "whyRejected": "Satisfying V7.2.1 alone is not sufficient. The rejected output treats the two-part requirement as a single criterion and declares a pass on partial compliance.",
  "sourceAnchors": [
    {
      "sourceId": "OWASP-ASVS",
      "url": "https://github.com/OWASP/ASVS",
      "section": "V7.2 Log Processing Requirements — V7.2.1 and V7.2.2",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-055

```json
{
  "rowId": "EXT-055",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "log-protection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this log storage result: application logs are written to a flat file on the same server that runs the application. The log directory has write permissions for the application service account. Logs are rotated by logrotate and retained for 30 days.",
    "b": "Log path: /var/log/app/app.log. Permissions: app_service_account rw, others none. Retention: 30 days via logrotate. No integrity protection. No forwarding to external SIEM.",
    "c": "Context: OWASP ASVS V7.3 — Log Protection. V7.3.1 requires that logs be protected from unauthorized modification and deletion. V7.3.2 requires that log data be forwarded to a read-only system if integrity is a requirement. Writing logs to a writable local path on the same host as the application means a compromised application can alter or delete its own logs."
  },
  "function": "Apply logProtectionArbiter (ASVS V7.3): verify that logs are stored in a location where the application runtime cannot modify or delete them after writing.",
  "acceptedOutput": {
    "decision": "REJECT on V7.3.1 and V7.3.2. Logs are written to a local writable directory on the same host as the application. A compromised application process can open and truncate /var/log/app/app.log, removing evidence of its own actions. V7.3.2 requires forwarding to an append-only or read-only log store. Local logrotate retention does not satisfy this — the application can delete the file before logrotate runs.",
    "reprocessInstruction": "Forward log entries to an external append-only destination (SIEM, S3 with object lock, or a syslog server with no inbound write access from the application host) in addition to any local copy. The external destination must be unreachable by write from the application service account."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The log directory is readable only by the application service account and root. Unauthorized external parties cannot read or modify the logs."
  },
  "arbiters": ["logProtectionArbiter"],
  "whyAccepted": "V7.3 protection is against the application itself (after compromise) modifying its own logs, not against external unauthorized access. The rejected output addresses external-access threat but ignores the self-modification threat, which is the primary ASVS concern.",
  "whyRejected": "Access control from external parties does not satisfy V7.3.1 when the application service account has write access to the log directory. After compromise, the attacker controls that account.",
  "sourceAnchors": [
    {
      "sourceId": "OWASP-ASVS",
      "url": "https://github.com/OWASP/ASVS",
      "section": "V7.3 Log Protection Requirements — V7.3.1 and V7.3.2",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-056

```json
{
  "rowId": "EXT-056",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "rollout-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to promote the new version of our payment service to production. It handles financial transactions and we cannot afford more than 30 seconds of elevated error rate during the rollout. Our cluster has 20 replicas and our load balancer can split traffic by percentage.",
    "b": "Current state: payment-service v1.4.2 at 20 replicas, all serving production traffic. Target: v1.4.3 with a database schema change that adds a nullable column (backward-compatible read path, new write path). The new schema migration runs as an init container.",
    "c": "Context: KEP-1847 and KEP-2221 define rollout strategy criteria. The payment service requirement is less than 30 seconds of elevated error rate, not zero. Canary deployments route a percentage of live traffic to the new version before full promotion. Blue/green keeps the old version fully available as a fallback."
  },
  "function": "Decompose the rollout goal into a machine command sequence. Select the rollout strategy that minimizes error exposure within the stated 30-second budget and produces the correct kubectl and Argo Rollouts commands.",
  "acceptedOutput": {
    "commands": [
      "kubectl apply -f payment-service-v1.4.3-canary.yaml",
      "kubectl argo rollouts set weight payment-service 10",
      "kubectl argo rollouts pause payment-service --duration=120s",
      "kubectl argo rollouts promote payment-service"
    ],
    "strategy": "Canary at 10% for 2 minutes. Rationale: the schema migration is backward-compatible so v1.4.2 and v1.4.3 can coexist. Routing 10% to v1.4.3 for 120 seconds limits blast radius — at 10% traffic share, only 10% of transactions are at risk during the observation window. 120 seconds exceeds the 30-second error-rate observation requirement while staying within reasonable rollout duration. If the error rate stays below threshold during the canary window, promote fully.",
    "rollbackTrigger": "If p99 error rate on the canary replica set exceeds 1% during the 120-second window, kubectl argo rollouts abort payment-service."
  },
  "rejectedOutput": {
    "commands": ["kubectl set image deployment/payment-service payment=registry/payment:v1.4.3"],
    "strategy": "Rolling update. All 20 replicas update in sequence, Kubernetes default maxSurge=25% maxUnavailable=25%."
  },
  "arbiters": ["rolloutStrategyArbiter"],
  "whyAccepted": "Canary at low weight satisfies the 30-second error constraint by limiting traffic exposure before full promotion. The backward-compatible schema allows version coexistence.",
  "whyRejected": "Rolling update promotes all replicas regardless of error rate in the new version. There is no observation window and no automatic abort signal. If v1.4.3 has a defect the rolling update propagates it to 100% of traffic before the error rate condition can be assessed.",
  "sourceAnchors": [
    {
      "sourceId": "KUBERNETES-KEPS",
      "url": "https://github.com/kubernetes/enhancements/tree/master/keps/sig-apps",
      "section": "KEP-1847 rollout strategy graduation criteria",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-057

```json
{
  "rowId": "EXT-057",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "list-operations",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I need an endpoint that returns all orders for a given customer, but I want to be able to filter by status (pending, shipped, delivered) and sort by creation date descending. There will be thousands of orders per customer.",
    "b": "API surface: GET /customers/{customer_id}/orders. Expected response: paginated list of Order resources. The client will call repeatedly until all pages are retrieved.",
    "c": "Context: AIP-132 Standard methods: List. AIP-132 defines the canonical filter parameter as filter=<CEL expression>, the order_by parameter for sort, and page_token + page_size for pagination. The filter must not include the parent resource ID — that belongs in the URL path, not the filter string."
  },
  "function": "Decompose the list-endpoint requirement into machine commands: define the URL structure, filter parameter syntax, ordering parameter, and pagination contract per AIP-132.",
  "acceptedOutput": {
    "commands": [
      "GET /v1/customers/{customer_id}/orders?filter=status%3D%22shipped%22&order_by=create_time+desc&page_size=50&page_token={token}",
      "Response: { orders: [...], next_page_token: '...', total_size: 1847 }",
      "If next_page_token is empty string or absent: no more pages."
    ],
    "rationale": "AIP-132 §Filter: filter parameter uses CEL syntax. status=\"shipped\" is a CEL equality expression. order_by=create_time desc matches AIP-132 ordering format. The customer_id is in the URL path — AIP-132 prohibits putting the parent resource in the filter parameter. page_token enables server-side cursor pagination. total_size is optional but recommended for client progress indicators."
  },
  "rejectedOutput": {
    "commands": [
      "GET /v1/orders?customer_id=42&status=shipped&sort=created_at&direction=desc&page=3&per_page=50"
    ],
    "rationale": "Standard query parameter pattern: customer_id as filter param, offset pagination with page number."
  },
  "arbiters": ["apiDesignArbiter"],
  "whyAccepted": "AIP-132 places the parent resource ID in the URL path, uses filter= for CEL expressions, order_by= for sorting, and page_token for cursor-based pagination. All four conventions are correct.",
  "whyRejected": "The rejected output puts customer_id in the filter parameters (AIP-132 violation), uses custom sort parameter names (sort, direction) rather than order_by, and uses offset pagination (page=) rather than cursor pagination (page_token=). Three AIP-132 violations in one endpoint.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://github.com/aip-dev/google.aip.dev/blob/master/aip/general/0132.md",
      "section": "AIP-132 Standard methods: List — Filter, Order, and Pagination",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-058

```json
{
  "rowId": "EXT-058",
  "sourceIds": ["NESTJS-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "request-pipeline-responsibility",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this NestJS result: an HTTP endpoint for updating a user's subscription plan performs the following steps inside the controller method body: (1) validates the new plan ID against the available plans, (2) checks whether the user has an active payment method, (3) updates the subscription record, (4) sends a confirmation email, (5) returns HTTP 200.",
    "b": "@Put(':id/subscription') async updateSubscription(@Param('id') userId, @Body() dto) { const plan = await this.plansRepo.findOne(dto.planId); if (!plan) throw new BadRequestException(); const user = await this.usersRepo.findOne(userId); if (!user.hasPaymentMethod) throw new PaymentRequiredException(); await this.subscriptionsRepo.update(userId, dto.planId); await this.emailService.sendConfirmation(user.email, plan); return { status: 'ok' }; }",
    "c": "Context: NestJS architecture — controller responsibility is routing and HTTP boundary. Business logic, orchestration, and side effects belong in services. A controller that orchestrates multiple service calls, applies business rules, and triggers side effects is a fat-controller anti-pattern. The pipeline's arbiter selector must flag this for reprocessing before the result is sent to the formatter."
  },
  "function": "Apply controllerResponsibilityArbiter: verify that the controller contains only HTTP boundary logic (request parsing, response shaping, exception mapping) and delegates all business logic to injected services.",
  "acceptedOutput": {
    "decision": "REJECT. The controller method contains four distinct business responsibilities: plan existence validation, payment method check, subscription update, and email side effect. None of these is an HTTP boundary concern. The controller should call a single service method — this.subscriptionService.upgrade(userId, dto.planId) — and map the service's domain result to an HTTP response. All four steps belong in SubscriptionService.upgrade().",
    "reprocessInstruction": "Extract all business logic into SubscriptionService.upgrade(userId, planId): PlanId planId; validate(planId); checkPaymentMethod(userId); updateSubscription(userId, planId); sendConfirmation(userId, planId). The controller maps service success to HTTP 200, PaymentRequiredException to 402, NotFoundException to 400."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The controller correctly uses dependency-injected repositories and services. All dependencies are resolved through the NestJS IoC (Inversion of Control) container, satisfying the Fabric First principle."
  },
  "arbiters": ["controllerResponsibilityArbiter"],
  "whyAccepted": "Using injected dependencies satisfies the IoC requirement but does not satisfy the single-responsibility requirement. The controller is orchestrating a multi-step business transaction, which belongs in a service layer.",
  "whyRejected": "Dependency injection is a delivery mechanism, not a responsibility boundary. A fat controller that uses injected dependencies is still a fat controller.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-CORE",
      "url": "https://github.com/nestjs/nest",
      "section": "Controllers documentation — single responsibility and service delegation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-059

```json
{
  "rowId": "EXT-059",
  "sourceIds": ["NESTJS-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "guard-vs-interceptor-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS developer placed a JWT (JSON Web Token) validation step inside an interceptor rather than a guard. The interceptor parses the Authorization header, validates the signature and expiry, and calls next.handle() if valid or throws UnauthorizedException if invalid.",
    "b": "@Injectable() class AuthInterceptor implements NestInterceptor { intercept(context: ExecutionContext, next: CallHandler) { const token = context.switchToHttp().getRequest().headers.authorization; if (!isValid(token)) throw new UnauthorizedException(); return next.handle(); } }",
    "c": "Context: NestJS execution order — guards run before interceptors in the request pipeline. Guards are intended for authorization decisions (can this request proceed?). Interceptors are intended for response transformation and cross-cutting concerns that need access to the observable response stream. Placing authorization logic in an interceptor delays the decision to the wrong phase."
  },
  "function": "Apply nestjsPipelinePhaseArbiter: verify that request gating logic (authorization, authentication) is placed in a Guard, not an Interceptor.",
  "acceptedOutput": {
    "decision": "REJECT. JWT validation is an authorization decision — it determines whether the request is allowed to proceed. This belongs in a Guard (implements CanActivate), not an Interceptor. Guards execute before Interceptors in the NestJS pipeline. Placing authorization in an interceptor means the route handler receives the request before the authorization decision fires if guard logic runs at a different point. Additionally, guards integrate with NestJS's @UseGuards() decorator and Reflector metadata, enabling declarative per-route configuration that interceptors cannot access in the same way.",
    "reprocessInstruction": "Move JWT validation to @Injectable() class JwtAuthGuard implements CanActivate { canActivate(context: ExecutionContext): boolean | Promise<boolean> { ... validate token ... } }. Apply with @UseGuards(JwtAuthGuard) on routes or controllers."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The interceptor performs the same check and produces the same outcome as a guard would — UnauthorizedException if invalid, next.handle() if valid. The functional result is identical."
  },
  "arbiters": ["nestjsPipelinePhaseArbiter"],
  "whyAccepted": "Functional equivalence does not imply architectural correctness. Guards and Interceptors occupy different phases of the pipeline and carry different semantics. The NestJS documentation explicitly assigns authorization to Guards.",
  "whyRejected": "The rejected output evaluates only the functional outcome (same exception, same proceed). It does not evaluate whether the component type matches the concern's designated pipeline phase.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-CORE",
      "url": "https://github.com/nestjs/nest",
      "section": "Guards documentation — execution order and CanActivate contract",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-060

```json
{
  "rowId": "EXT-060",
  "sourceIds": ["LANGGRAPH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "agent-graph-routing",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this LangGraph routing result: a graph node that calls an external API returns a result object. A downstream conditional edge evaluates whether to retry, continue, or escalate. The conditional function checks only result.status_code — if 200 continue, if 429 retry, else escalate — and does not inspect result.body for embedded error flags.",
    "b": "conditional_edge logic: if result.status_code == 200: return 'continue'; elif result.status_code == 429: return 'retry'; else: return 'escalate'. The API can return HTTP 200 with body { 'error': 'QUOTA_EXCEEDED', 'retryable': true }.",
    "c": "Context: LangGraph conditional edges route graph state between nodes. The routing condition must be exhaustive — it must inspect all signals that could change the intended next node. APIs that embed error conditions in HTTP 200 responses (soft errors) require body inspection as a second routing signal."
  },
  "function": "Apply graphRoutingCompletenessArbiter: verify that conditional edge functions inspect all signals that influence the routing decision, including soft errors embedded in success-status responses.",
  "acceptedOutput": {
    "decision": "REJECT. The conditional edge inspects only status_code. The API returns HTTP 200 with body.error='QUOTA_EXCEEDED' and body.retryable=true — a soft error in a success envelope. The routing function routes this case to 'continue' when it should route to 'retry'. The graph will proceed past a retryable error, causing a downstream failure.",
    "reprocessInstruction": "Extend the conditional to: if result.status_code == 200 and result.body.get('error') is None: return 'continue'; elif result.status_code == 429 or (result.status_code == 200 and result.body.get('retryable')): return 'retry'; else: return 'escalate'."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The conditional edge covers all HTTP status codes the API can return — 200, 429, and other 4xx/5xx. The routing is complete for the HTTP layer."
  },
  "arbiters": ["graphRoutingCompletenessArbiter"],
  "whyAccepted": "Completeness for HTTP status codes is not the same as completeness for the API contract. The API contract includes soft errors in HTTP 200 bodies. A routing function that ignores the body is incomplete relative to the API's actual signal set.",
  "whyRejected": "The rejected output scopes completeness to the HTTP layer. The routing function must be complete relative to the API contract, which is a superset of the HTTP status layer.",
  "sourceAnchors": [
    {
      "sourceId": "LANGGRAPH",
      "url": "https://github.com/langchain-ai/langgraph",
      "section": "Conditional Edges documentation — routing function exhaustiveness",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-061

```json
{
  "rowId": "EXT-061",
  "sourceIds": ["GHERKIN"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "bdd-scenario-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this user story into a Gherkin scenario set. User story: As a checkout user, I want to apply a promotional coupon code so that I get a discount on my order total.",
    "b": "The system must accept a valid coupon, reject an expired coupon, reject a coupon already used by this user, and reject a coupon for a different product category than what is in the cart.",
    "c": "Context: Gherkin Given/When/Then syntax. Each scenario covers one acceptance criterion. The Given block names the precondition state. The When block names the single action. The Then block names the observable, verifiable outcome — not internal state, not implementation detail."
  },
  "function": "Translate each acceptance criterion into one complete Gherkin Scenario block. The Given/When/Then must be behaviors, not implementation steps.",
  "acceptedOutput": {
    "scenarios": "Feature: Apply promotional coupon at checkout\n\nScenario: Valid coupon reduces order total\n  Given a cart containing 2 items totalling £80.00\n  And a coupon code 'SUMMER20' that gives 20% off and has not been used by this account\n  When the user applies coupon code 'SUMMER20'\n  Then the order total is £64.00\n  And the coupon 'SUMMER20' is marked as used for this account\n\nScenario: Expired coupon is rejected\n  Given a cart containing 1 item totalling £40.00\n  And a coupon code 'EXPIRED10' whose expiry date is yesterday\n  When the user applies coupon code 'EXPIRED10'\n  Then the order total remains £40.00\n  And the user sees message 'This coupon has expired'\n\nScenario: Already-used coupon is rejected\n  Given a cart with any items\n  And coupon code 'ONCE50' has already been used by this account\n  When the user applies coupon code 'ONCE50'\n  Then the coupon is not applied\n  And the user sees message 'This coupon has already been used on your account'\n\nScenario: Wrong-category coupon is rejected\n  Given a cart containing only electronics items\n  And a coupon code 'SHOES15' that applies only to footwear\n  When the user applies coupon code 'SHOES15'\n  Then the coupon is not applied\n  And the user sees message 'This coupon does not apply to items in your cart'"
  },
  "rejectedOutput": {
    "scenarios": "Scenario: Apply coupon\n  Given the user has a coupon code\n  When they enter it at checkout\n  Then the system validates it in the database\n  And updates the order_total field\n  And returns the new total to the frontend"
  },
  "arbiters": ["gherkinScenarioArbiter"],
  "whyAccepted": "Each scenario tests exactly one acceptance criterion. Given names observable preconditions. When names a single user action. Then names an observable outcome (price change, message) — not internal state (database column, field update).",
  "whyRejected": "The rejected output conflates four scenarios into one, making it untestable for individual failure modes. Then describes internal implementation steps (database update, field name) rather than observable behavior. A test runner cannot observe 'updates the order_total field' without querying the database directly.",
  "sourceAnchors": [
    {
      "sourceId": "GHERKIN",
      "url": "https://github.com/cucumber/gherkin",
      "section": "Gherkin language specification — Scenario structure",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-062

```json
{
  "rowId": "EXT-062",
  "sourceIds": ["CONVENTIONAL-COMMITS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "commit-message-conformance",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this commit message for Conventional Commits conformance: 'fixed the bug where users couldn't log in after password reset, also updated the error message to be clearer'",
    "b": "A single commit that changes auth/password-reset.service.ts (fix for the login bug) and ui/error-messages.ts (updated copy). The commit message has no type prefix, no scope, and bundles two distinct changes.",
    "c": "Context: Conventional Commits specification requires: type(scope): description. Valid types: feat, fix, docs, style, refactor, test, chore. Scope is optional but recommended when the change is bounded to a module. A commit with multiple distinct changes should be split. The message must start with a type token."
  },
  "function": "Apply conventionalCommitArbiter: verify that the commit message uses the correct type prefix, optional scope, and short description; flag if multiple concerns are bundled.",
  "acceptedOutput": {
    "decision": "REJECT. Two violations: (1) No type prefix — the message starts with 'fixed' not 'fix:'. Without the type prefix, automated changelog generation cannot classify this commit and BREAKING CHANGE detection cannot operate. (2) The commit bundles a bug fix and a documentation/copy change — two separate concerns that should be two commits.",
    "reprocessInstruction": "Split into two commits: 'fix(auth): redirect to login after successful password reset' and 'docs(ui): clarify error message wording for login failure'. Each commit addresses one concern and produces one changelog entry."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The message clearly describes what changed and why. It is readable by developers without any tooling."
  },
  "arbiters": ["conventionalCommitArbiter"],
  "whyAccepted": "Conventional Commits is a machine-readable contract, not a human-readability standard. A readable message that lacks the type prefix breaks automated changelog generation, semantic versioning bumps, and BREAKING CHANGE parsing.",
  "whyRejected": "Human readability is not the criterion. The arbiter evaluates machine parseability and single-concern atomicity. A message that passes human review but fails the type-prefix contract fails the arbiter.",
  "sourceAnchors": [
    {
      "sourceId": "CONVENTIONAL-COMMITS",
      "url": "https://github.com/conventional-commits/conventionalcommits.org",
      "section": "Conventional Commits specification v1.0.0 — Structural Elements",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-063

```json
{
  "rowId": "EXT-063",
  "sourceIds": ["MCP-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "mcp-tool-selection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this MCP (Model Context Protocol) tool routing result: a server exposes two tools — 'read_file' and 'search_files'. The agent receives a user request: 'show me the contents of config.yaml'. The agent calls search_files with query='config.yaml' and returns the list of file paths that match, without reading any file content.",
    "b": "Tool call: search_files({ query: 'config.yaml' }). Result: ['./config/config.yaml', './backup/config.yaml']. Agent response: 'I found these config files: ...' without calling read_file.",
    "c": "Context: MCP tool selection — each tool has a distinct contract. search_files finds files by name pattern. read_file returns the content of a file at a specific path. The user asked for contents, not a path listing. Tool selection must match the user's information need, not a superficially related capability."
  },
  "function": "Apply mcpToolSelectionArbiter: verify that the tool selected matches the information need expressed in the user request, not a syntactically similar alternative.",
  "acceptedOutput": {
    "decision": "REJECT. The user requested 'show me the contents of config.yaml' — an information need for file content. The agent called search_files, which returns file paths, not content. The correct sequence is: (1) call search_files to resolve the ambiguous path if needed, (2) call read_file on the resolved path to retrieve the content. Returning path results without the content does not satisfy the user's request.",
    "reprocessInstruction": "Call search_files({ query: 'config.yaml' }) to find the path, then call read_file({ path: './config/config.yaml' }) to retrieve the content. Return the content to the user."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The agent correctly identified config.yaml using the search tool and provided the user with the file locations."
  },
  "arbiters": ["mcpToolSelectionArbiter"],
  "whyAccepted": "Tool selection must match the information need in the request, not the surface form of the request. 'Show me the contents' requires read_file, not search_files. Path discovery is a means to an end, not the end itself.",
  "whyRejected": "The rejected output evaluates whether the agent performed an action related to config.yaml. It does not evaluate whether the action satisfied the user's information need (content, not paths).",
  "sourceAnchors": [
    {
      "sourceId": "MCP-DOCS",
      "url": "https://github.com/modelcontextprotocol/docs",
      "section": "MCP Tools specification — tool selection and information need matching",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-064

```json
{
  "rowId": "EXT-064",
  "sourceIds": ["ASPNET-WEBSTACK"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "legacy-dotnet-modernization",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this ASP.NET MVC 5 controller for modernization readiness: the controller action method opens a SqlConnection directly, executes a raw SQL query to load user data, populates a ViewModel, and calls SmtpClient.Send() to dispatch a welcome email before returning the view.",
    "b": "public ActionResult Register(RegisterModel model) { using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings['db'].ConnectionString)) { conn.Open(); var cmd = new SqlCommand('SELECT * FROM Users WHERE Email=@email', conn); cmd.Parameters.AddWithValue('@email', model.Email); var reader = cmd.ExecuteReader(); /* populate ViewModel */ } SmtpClient smtp = new SmtpClient(); smtp.Send(new MailMessage('noreply@co.com', model.Email, 'Welcome', 'body')); return View(viewModel); }",
    "c": "Context: Legacy ASP.NET MVC 5 controller modernization to ASP.NET Core. Three anti-patterns: (1) Direct SqlConnection in a controller (data access in presentation layer). (2) Raw SQL with string-based connection string lookup (no repository, no ORM). (3) Synchronous SMTP send in a request handler (blocking I/O on the request thread). All three must be resolved before migrating to ASP.NET Core without introducing latent bugs."
  },
  "function": "Apply legacyDotNetModernizationArbiter: identify all responsibility violations, synchronous I/O patterns, and direct infrastructure dependencies that must be resolved before framework migration.",
  "acceptedOutput": {
    "decision": "REJECT — three blocking issues before migration. Issue 1: SqlConnection in controller violates separation of concerns. Extract to IUserRepository with async Task<User> FindByEmailAsync(string email). Issue 2: Raw SqlCommand in controller — replace with EF Core or Dapper via the repository. The controller must not reference System.Data. Issue 3: SmtpClient.Send() is synchronous and blocks the request thread. In ASP.NET Core, this causes thread pool starvation under load. Replace with an async email service: IEmailService.SendWelcomeAsync(string to) backed by a queued background job or SendGrid/Mailgun client with async API.",
    "reprocessInstruction": "Before migration: (1) Create IUserRepository and UserRepository : IUserRepository with async methods. (2) Inject IUserRepository in controller constructor. (3) Create IEmailService with SendWelcomeAsync. (4) Move email dispatch to a background queue (IBackgroundTaskQueue or Hangfire). Then migrate to ASP.NET Core."
  },
  "rejectedOutput": {
    "decision": "ACCEPT for migration. The code uses parameterized queries (preventing SQL injection) and the using block correctly disposes the SqlConnection. The controller is small and readable."
  },
  "arbiters": ["legacyDotNetModernizationArbiter"],
  "whyAccepted": "Parameterized queries and correct disposal address security and resource concerns but do not address architectural concerns. The modernization arbiter evaluates responsibility placement and async readiness, not just SQL safety.",
  "whyRejected": "The rejected output evaluates SQL injection and resource disposal — runtime correctness concerns — but ignores the structural concerns that cause migration failures: synchronous I/O on the request thread and direct infrastructure dependencies in the controller.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNET-WEBSTACK",
      "url": "https://github.com/aspnet/AspNetWebStack",
      "section": "MVC controller patterns — responsibility boundaries",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-065

```json
{
  "rowId": "EXT-065",
  "sourceIds": ["EFCORE-SOURCE", "EF6-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "data-access-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Select and implement the correct data access strategy for this query: retrieve the top 10 active orders for a given customer, including the customer name and the first line item's product SKU, ordered by creation date descending. The query must execute in under 50ms at p95 on a SQL Server database with 2 million orders.",
    "b": "Existing code uses Entity Framework Core 8 for most queries. A benchmark shows that the EF Core Include chain for this query generates a 6-table JOIN with 40 columns selected. Current p95 is 180ms.",
    "c": "Context: EF Core versus Dapper selection. EF Core Include chains perform well for graphs with few relationships and low cardinality. When the generated SQL selects more columns than needed and performance is a hard requirement, Dapper with a raw SQL query is the correct tool. Both are in the codebase and both are Apache 2.0 / MIT licensed."
  },
  "function": "Decompose the query requirement into action items: select the data access tool and produce the implementation.",
  "acceptedOutput": {
    "actionItems": [
      "Use Dapper for this query. EF Core's Include chain selects 40 columns; this query needs 4 fields. The column overhead is causing the p95 miss.",
      "SQL: SELECT TOP 10 o.Id, o.CreatedAt, c.Name AS CustomerName, (SELECT TOP 1 li.ProductSku FROM LineItems li WHERE li.OrderId = o.Id ORDER BY li.Id) AS FirstSku FROM Orders o INNER JOIN Customers c ON o.CustomerId = c.Id WHERE o.CustomerId = @customerId AND o.Status = 'Active' ORDER BY o.CreatedAt DESC",
      "C# implementation: var orders = await connection.QueryAsync<OrderSummaryDto>(sql, new { customerId }); return orders.ToList();",
      "Add covering index: CREATE INDEX IX_Orders_CustomerId_Status_CreatedAt ON Orders(CustomerId, Status, CreatedAt DESC) INCLUDE (Id);",
      "Benchmark after: target p95 < 50ms on production-scale data before merging."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Add .AsSplitQuery() to the EF Core Include chain. Split queries execute multiple SELECT statements instead of one large JOIN, reducing data volume.",
      "Add .AsNoTracking() to disable change tracking overhead."
    ]
  },
  "arbiters": ["dataAccessSelectionArbiter"],
  "whyAccepted": "Dapper with a projection query selects only the 4 fields needed and avoids the 6-table JOIN. The covering index ensures the query uses an index seek rather than a table scan. These two changes address the p95 miss at its root.",
  "whyRejected": "Split queries and AsNoTracking reduce EF Core overhead but do not eliminate the column-selection excess or the JOIN structure. The 40-column SELECT is the binding constraint; making it run in two statements instead of one does not fix the column width problem.",
  "sourceAnchors": [
    {
      "sourceId": "EFCORE-SOURCE",
      "url": "https://github.com/dotnet/efcore",
      "section": "Performance documentation — raw SQL queries and Dapper interoperability",
      "license": "MIT"
    },
    {
      "sourceId": "DAPPER",
      "url": "https://github.com/DapperLib/Dapper",
      "section": "README — query projection and performance",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-066

```json
{
  "rowId": "EXT-066",
  "sourceIds": ["ANGULARJS-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "legacy-frontend-migration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this AngularJS controller for migration readiness to Angular 17: the controller directly manipulates the DOM using jQuery inside a $scope.$watch callback, makes an $http call inside a $scope function triggered by ng-click, and stores all UI state on $scope alongside business data.",
    "b": "$scope.loadUser = function() { $.ajax({ url: '/api/user/' + $scope.userId, success: function(data) { $scope.user = data; $scope.$apply(); $('#user-name').text(data.name); } }); }; $scope.$watch('userId', function(newId) { if (newId) { $('#profile-section').show(); $scope.loadUser(); } });",
    "c": "Context: AngularJS to Angular 17 migration. Three blocking patterns: (1) jQuery DOM manipulation bypasses AngularJS change detection and is incompatible with Angular's renderer abstraction. (2) Raw $.ajax bypasses AngularJS's $http/$q zone (requires manual $apply) and will not work in Angular's Zone.js environment. (3) Mixing UI state and business data on $scope has no equivalent in Angular's component/service model."
  },
  "function": "Apply legacyFrontendMigrationArbiter: identify DOM manipulation, HTTP bypass, and state mixing patterns that are incompatible with the migration target.",
  "acceptedOutput": {
    "decision": "REJECT — three blocking patterns. Pattern 1: jQuery DOM manipulation ($.('#user-name').text()) bypasses the framework renderer. In Angular 17, the equivalent is a template binding {{ user.name }} or @ViewChild with Renderer2 — never direct DOM calls. Pattern 2: $.ajax bypasses Angular's HttpClient and Zone.js change detection. Replace with this.http.get<User>('/api/user/' + userId) which runs inside Angular's zone. Pattern 3: $scope carries both userId (navigation input), user (loaded entity), and implicit show/hide state (the #profile-section toggle). In Angular, these must be separated: userId is an @Input(), user is a component field loaded by a service, and visibility is a template conditional (*ngIf='user !== null').",
    "reprocessInstruction": "Before migrating: (1) Remove all jQuery calls, replace with Angular template bindings. (2) Replace $.ajax with Angular HttpClient injected via constructor. (3) Separate concerns — create UserService for data loading, express visibility as a template condition."
  },
  "rejectedOutput": {
    "decision": "ACCEPT for migration using a hybrid AngularJS/Angular approach. The AngularJS code can run inside the Angular downgrade module, and jQuery can be loaded alongside Angular."
  },
  "arbiters": ["legacyFrontendMigrationArbiter"],
  "whyAccepted": "Hybrid/downgrade mode is a transitional tool, not the migration destination. The goal is Angular 17 components, not a permanent mixed application. The three patterns must be resolved before the component can operate correctly in pure Angular mode.",
  "whyRejected": "The rejected output defers the migration problem by proposing the hybrid approach as a destination. Hybrid mode has maintenance overhead and does not resolve the underlying DOM/HTTP/state incompatibilities.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULARJS-SOURCE",
      "url": "https://github.com/angular/angular.js",
      "section": "Migration guide — $scope, $http, and DOM manipulation patterns",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-067

```json
{
  "rowId": "EXT-067",
  "sourceIds": ["GOVUK-DESIGN-SYSTEM"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "accessibility-conformance",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this form error state result: a form field for a date of birth input shows validation errors using only red border color on the input field. No error message text is displayed. No aria-describedby is set. No role=alert is applied.",
    "b": "<input type='text' id='dob' style='border: 2px solid red;' /> — shown when date format is invalid. No adjacent error paragraph. No ARIA attributes.",
    "c": "Context: GOV.UK Design System error message component requirements. The GOV.UK DS defines three requirements for accessible error states: (1) A visible error message in text adjacent to the field. (2) The error message must be linked to the input via aria-describedby. (3) The error message container must have role=alert or be announced by a live region so screen reader users hear it without moving focus."
  },
  "function": "Apply accessibilityErrorStateArbiter (GOV.UK DS): verify that error states include visible text, ARIA linkage, and live region announcement — not color-only signaling.",
  "acceptedOutput": {
    "decision": "REJECT on three criteria. Criterion 1: color-only error signaling. Red border alone does not communicate the error to users who are color blind or using high-contrast mode. GOV.UK DS requires a visible error message in text. Criterion 2: no aria-describedby. Screen reader users navigating to the input field will not hear the error because the error message (if it existed) is not linked to the input. Criterion 3: no live region announcement. Keyboard users who trigger validation by submitting the form will not hear any indication that an error occurred unless they navigate back to the field.",
    "reprocessInstruction": "Implement: <p id='dob-error' role='alert' class='govuk-error-message'>Enter a date of birth in DD MM YYYY format</p><input type='text' id='dob' aria-describedby='dob-error' aria-invalid='true' />. The error paragraph must appear before the input in DOM order."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The red border is a widely recognized visual convention for errors. Users who see the color change will understand that the field has a problem."
  },
  "arbiters": ["accessibilityErrorStateArbiter"],
  "whyAccepted": "Visual convention is a design shorthand, not an accessibility standard. WCAG 1.4.1 prohibits using color as the sole means of conveying information. The GOV.UK Design System requirement is more specific: text, ARIA linkage, and live region — all three are mandatory.",
  "whyRejected": "The rejected output applies a 'most users see it' standard. Accessibility arbiters apply universal access standards. A solution that works for sighted, non-color-blind users but fails for screen reader users, keyboard-only users, and color-blind users fails the accessibility criterion.",
  "sourceAnchors": [
    {
      "sourceId": "GOVUK-DESIGN-SYSTEM",
      "url": "https://github.com/alphagov/govuk-frontend",
      "section": "Error message component — accessibility requirements",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT — OGL v3.0 for content pairs",
  "trainingStatus": "candidate"
}
```

---

## EXT-068

```json
{
  "rowId": "EXT-068",
  "sourceIds": ["MERMAID"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "sequence-diagram-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A user asks: 'Can you show me the sequence of steps when a user logs in using Google SSO?' The pipeline has produced a formal result describing the OAuth 2.0 authorization code flow: user → browser → Google auth endpoint → redirect with code → app → Google token endpoint → access token → Google userinfo → user record lookup → session creation.",
    "b": "Formal result: 9-step OAuth 2.0 authorization code flow with PKCE. Steps: (1) user initiates, (2) browser redirects to Google, (3) user authenticates at Google, (4) Google returns code, (5) browser sends code to app, (6) app POSTs to Google token endpoint, (7) Google returns access+id tokens, (8) app calls Google userinfo, (9) app creates/updates user record and issues session.",
    "c": "Context: Node 9 (formatter) must produce a Mermaid sequence diagram when the user asks for a visual representation of a flow. The Mermaid sequenceDiagram syntax requires participant declarations before message arrows. Actor labels must be human-readable. Activation blocks show which actor is processing at each step."
  },
  "function": "Format the 9-step OAuth flow as a correct Mermaid sequence diagram. All participants declared. Messages labeled with the protocol action. Activation blocks on the app server during the token exchange steps.",
  "acceptedOutput": {
    "diagram": "```mermaid\nsequenceDiagram\n    actor User\n    participant Browser\n    participant App as Your App\n    participant Google as Google OAuth\n\n    User->>Browser: Click 'Sign in with Google'\n    Browser->>Google: GET /auth?client_id=...&redirect_uri=...&code_challenge=...\n    Google->>User: Show Google login page\n    User->>Google: Enter Google credentials\n    Google->>Browser: 302 redirect to /callback?code=AUTH_CODE\n    Browser->>App: GET /callback?code=AUTH_CODE\n    activate App\n    App->>Google: POST /token {code, code_verifier, client_secret}\n    Google->>App: {access_token, id_token}\n    App->>Google: GET /userinfo (Authorization: Bearer access_token)\n    Google->>App: {email, name, sub}\n    App->>App: Lookup or create user record\n    deactivate App\n    App->>Browser: Set-Cookie: session=..., redirect to dashboard\n    Browser->>User: Show dashboard\n```"
  },
  "rejectedOutput": {
    "diagram": "Steps:\n1. User clicks login\n2. Redirect to Google\n3. User authenticates\n4. Code returned\n5. App exchanges code for token\n6. App fetches user info\n7. Session created"
  },
  "arbiters": ["formatterOutputArbiter"],
  "whyAccepted": "The Mermaid diagram uses correct syntax: participant declarations, actor for the user, activate/deactivate blocks on App during the token exchange, and labeled message arrows. The output can be rendered by any Mermaid engine without modification.",
  "whyRejected": "The rejected output is a numbered text list — valid prose but not a sequence diagram. The user explicitly asked for a visual representation. The formatter must produce the format the user requested, not a text equivalent.",
  "sourceAnchors": [
    {
      "sourceId": "MERMAID",
      "url": "https://github.com/mermaid-js/mermaid",
      "section": "sequenceDiagram syntax — participants, messages, and activation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-069

```json
{
  "rowId": "EXT-069",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "patch-application",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Apply this fix: GitHub issue reports that calling list.sort() on a list containing None values raises TypeError in Python 3. The function should sort non-None values and place None values at the end.",
    "b": "File: utils/sorting.py, function sort_with_nulls_last(items). Current implementation: return sorted(items). Failing test: assert sort_with_nulls_last([3, None, 1, None, 2]) == [1, 2, 3, None, None]",
    "c": "Context: SWE-bench style patch application. The action item specifies the file, the function, and the expected behavior from the failing test. The patch must make the test pass without modifying the test file."
  },
  "function": "Produce the minimal patch to utils/sorting.py that makes the failing test pass. Show the before and after content of the function.",
  "acceptedOutput": {
    "before": "def sort_with_nulls_last(items):\n    return sorted(items)",
    "after": "def sort_with_nulls_last(items):\n    return sorted((x for x in items if x is not None)) + [x for x in items if x is None]",
    "explanation": "The fix separates items into non-None (sorted) and None (appended in original relative order). sorted() on a list with None raises TypeError in Python 3 because None is not orderable against integers. The generator expression filters non-None values for sorting; the list comprehension collects None values in their original relative order.",
    "testResult": "sort_with_nulls_last([3, None, 1, None, 2]) → [1, 2, 3, None, None] ✓"
  },
  "rejectedOutput": {
    "after": "def sort_with_nulls_last(items):\n    return sorted(items, key=lambda x: (x is None, x))",
    "explanation": "Use a key function that places None values after non-None by returning a tuple where None maps to (True, None) and integers map to (False, integer)."
  },
  "arbiters": ["patchCorrectnessArbiter"],
  "whyAccepted": "The accepted patch is correct and minimal. It handles the failing test case exactly. The two-part comprehension is readable and makes the None handling explicit.",
  "whyRejected": "The rejected patch fails at runtime: key=lambda x: (x is None, x) compares tuples and when x is None the comparison (True, None) vs (False, 1) requires comparing None to 1 at the second tuple element, which still raises TypeError in Python 3. The key approach does not solve the ordering problem it introduces.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Dataset format — issue description, failing test, and verified patch",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-070

```json
{
  "rowId": "EXT-070",
  "sourceIds": ["DIATAXIS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "documentation-type-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A user asks: 'Write documentation for our new retry mechanism in the API client.' The pipeline has produced a formal result describing: a configurable retry policy with exponential backoff, jitter, maximum 3 retries, and a set of retryable HTTP status codes (429, 502, 503, 504).",
    "b": "Formal result: RetryPolicy class with properties MaxRetries (int, default 3), BaseDelayMs (int, default 500), Jitter (bool, default true), RetryableStatusCodes (List<int>, default [429, 502, 503, 504]). Method: ShouldRetry(HttpResponseMessage) → bool. Exponential formula: delay = BaseDelayMs * 2^(attempt-1) + random(0, BaseDelayMs/2) when Jitter is true.",
    "c": "Context: Diátaxis documentation framework — four distinct types. Tutorial: learning-oriented, walks through a task. How-to guide: task-oriented, achieves a specific goal. Reference: information-oriented, describes parameters and behavior. Explanation: understanding-oriented, explains why the design is the way it is. A user asking to 'write documentation' without specifying type needs the formatter to produce the correct type for the content."
  },
  "function": "Identify the correct Diátaxis documentation type for a retry mechanism technical description, then produce the document in that format.",
  "acceptedOutput": {
    "documentType": "Reference (information-oriented)",
    "rationale": "The formal result is a parameter specification and behavioral contract — not a tutorial, not a how-to, not an explanation. Reference documentation describes what something is and what it does. The user needs to know the parameters, defaults, and behavior of RetryPolicy.",
    "document": "## RetryPolicy\n\nConfigures automatic retry behavior for HTTP requests that fail with retryable status codes.\n\n### Properties\n\n| Property | Type | Default | Description |\n|---|---|---|---|\n| MaxRetries | int | 3 | Maximum number of retry attempts before the request fails permanently. |\n| BaseDelayMs | int | 500 | Base delay in milliseconds before the first retry. |\n| Jitter | bool | true | Adds random delay (0 to BaseDelayMs/2) to each retry to prevent thundering herd. |\n| RetryableStatusCodes | List\\<int\\> | 429, 502, 503, 504 | HTTP status codes that trigger a retry attempt. |\n\n### Methods\n\n**ShouldRetry(response: HttpResponseMessage) → bool**\n\nReturns `true` if the response status code is in `RetryableStatusCodes` and `MaxRetries` has not been reached.\n\n### Delay Formula\n\nWhen `Jitter` is `true`: `delay = BaseDelayMs × 2^(attempt−1) + random(0, BaseDelayMs/2)`\n\nWhen `Jitter` is `false`: `delay = BaseDelayMs × 2^(attempt−1)`"
  },
  "rejectedOutput": {
    "document": "# Getting Started with Retry\n\nIn this tutorial, you will learn how to add retry behavior to your API client. Follow these steps:\n\n1. Create a RetryPolicy object.\n2. Set MaxRetries to the number of times you want to retry.\n3. Add the status codes you want to retry on.\n4. Pass the policy to your HTTP client."
  },
  "arbiters": ["documentationTypeArbiter"],
  "whyAccepted": "Reference documentation is the correct type for a parameter specification. The accepted document shows all parameters, defaults, types, and the delay formula in a scannable table and definition format. Users consulting it can find any parameter without reading the whole document.",
  "whyRejected": "The rejected output produces a tutorial — learning-oriented, step-by-step. A tutorial teaches how to do something for the first time. A developer who already knows they need RetryPolicy does not want a tutorial; they want the parameter table. Wrong documentation type for the content.",
  "sourceAnchors": [
    {
      "sourceId": "DIATAXIS",
      "url": "https://github.com/evildmp/diataxis-documentation-framework",
      "section": "Reference documentation — information-oriented, not learning-oriented",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```
