# ARCH Pairs — Sub-phase 3.5 Observability and Operational Design
# Batch: dpo-pairs-arch-plan-batch-001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: logging strategy, metric definitions, trace design, alert thresholds, what must never appear in operational signals
# Pair count: 5

---

## Pair AP1-3.5-001 — observability strategy confused with logger wiring

```
source_id: xiigen-arch-plan-batch-001-AP1-3.5-001
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.5
rejected_sub_phase: 4.1
```

Prompt: `"design the observability strategy for the password reset flow — we need to detect abuse and also debug failures without leaking secrets"`

```
CHOSEN — 3.5 Observability and Operational Design:
Three signal categories with explicit allow-lists and forbid-lists.

Metrics (counters and histograms):
  password_reset_request_total — labels: outcome (requested / rate_limited /
    user_not_found / cooldown_active), tenant_id (hashed).
  password_reset_completion_latency_ms — histogram, labels: outcome
    (success / token_expired / token_invalid).
  password_reset_attempts_per_account — gauge, labels: tenant_id (hashed).
  Forbidden labels: email, userId (raw), IP address.

Traces:
  One span per reset request. Required tags: tenant_id (hashed),
  outcome, request_origin (web / mobile / api).
  Forbidden tags: the new password, the reset token, the email address,
  the user agent in raw form.

Logs:
  Structured JSON. Level INFO on success with correlation_id and outcome.
  Level WARN on rate_limited or cooldown_active.
  Level ERROR on internal failure (token storage write failed,
  email queue send failed).
  Forbidden in any log line: the password (current or new), the reset
  token, the secret_questions answer.

Alert rules:
  password_reset_request_total{outcome="rate_limited"} > 50 in 5 minutes —
    page on-call. Indicates active enumeration attack.
  password_reset_completion_latency_ms p95 > 3 seconds for 10 minutes —
    page on-call. Indicates email queue backlog.
  password_reset_attempts_per_account > 10 for any single account in 1 hour —
    silent log at WARN, no page. Indicates a confused user, not an attack.

Privacy rule: every metric label and trace tag is on the allow-list above.
Anything not on the allow-list is forbidden by default. The allow-list is
auditable; the forbid-list is exemplary.

REJECTED — 4.1 Backend Services:
Inject ILogger<PasswordResetService> through the constructor. Call
_logger.LogInformation("Password reset for {email} succeeded", email) on
success. Call _logger.LogWarning("Rate limited reset for {email}", email)
on throttle. Wrap the reset endpoint in OpenTelemetry's StartActivity and
call activity.SetTag("user.email", email) and activity.SetTag("password",
newPassword) for diagnosis. Add the latest correlation id to every span
through ActivitySource.AddBaggage("correlation_id", id).
[WRONG: "design the observability strategy" asks what to measure, what
thresholds matter, and what is forbidden in operational signals. The rejected
implementation logs the email address and the new password — both of which
must never appear in any signal. Implementation done before the strategy is
decided produces exactly this kind of leak. The allow-list / forbid-list
must be defined first; the logger calls are the implementation that follows.]
```

---

## Pair AP1-3.5-002 — observability design confused with monitoring tool setup

```
source_id: xiigen-arch-plan-batch-001-AP1-3.5-002
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.5
rejected_sub_phase: 7.3
```

Prompt: `"users complain that file uploads silently fail sometimes — we have no idea where in the pipeline the failure happens"`

```
CHOSEN — 3.5 Observability and Operational Design:
The pipeline has six stages and currently emits one log line at the entry
and one at the exit. The required observability emits one structured event
per stage transition with the stage outcome, plus one terminal event with
the overall result.

Six checkpoint events:
  upload.received       — after the multipart parser completes.
  upload.virus_scanned  — after the virus scan returns a result.
  upload.persisted      — after the byte stream is written to object storage.
  upload.thumbnail_made — after thumbnail generation (skipped for non-images).
  upload.indexed        — after the search index update.
  upload.completed      — terminal event, always emitted.

Each event carries: uploadId, stage, outcome (success / skipped / failed),
elapsed_ms_since_received, file_type, file_size_bucket. The
file_size_bucket is a categorical (small / medium / large), not the raw byte
count, because the histograms must be aggregable.

The terminal upload.completed event is required even on failure. Its
final_stage_failed field names which stage was the last one to emit a
non-success outcome. This is the column that diagnoses "silently failed":
the team can group by final_stage_failed and see exactly where the pipeline
breaks.

Alert rule: the ratio of upload.received to upload.completed must be > 0.99
in any 5-minute window. A drop indicates that some stage is silently dropping
the upload without emitting a terminal event — which means the observability
itself has a bug, not just the pipeline.

REJECTED — 7.3 Monitoring and Feedback Loop:
Connect the existing application logs to the Datadog log pipeline. Configure
a Datadog dashboard with charts for upload success rate, error rate, and
latency. Set up a PagerDuty alert when the error rate exceeds 5%. Add a
LogQL query in the dashboard for filtering by uploadId. Train the on-call
engineers on the new dashboard layout and document the runbook.
[WRONG: "we have no idea where in the pipeline the failure happens" means
the signals required to answer that question do not exist yet. Connecting
the existing logs to a dashboard does not help when the existing logs only
emit at entry and exit — the dashboard will show "many failures" but cannot
tell which stage is failing. Observability design (deciding what events to
emit at each stage and what attributes they carry) must precede the
monitoring tool setup.]
```

---

## Pair AP1-3.5-003 — alert design question confused with logger wiring

```
source_id: xiigen-arch-plan-batch-001-AP1-3.5-003
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.5
rejected_sub_phase: 4.1
```

Prompt: `"what alerts should fire for the payment processing service, and at what thresholds?"`

```
CHOSEN — 3.5 Observability and Operational Design:
Five alert rules. Each names the metric, the threshold, the window, the
severity, and the response expectation.

Page on-call (severity 1, immediate response):
  payment_failure_rate{provider!="test"} > 5% for 5 minutes.
    Action: investigate provider outage or our adapter failure.
    False-positive risk: zero — 5% real failures is a real incident.

  payment_provider_latency_p99_ms > 8000 for 10 minutes.
    Action: switch primary provider to secondary if available.
    False-positive risk: low — single provider degrading is the expected
    cause.

Page on-call (severity 2, 30-minute response):
  payment_settlement_lag_minutes > 60 for 30 minutes.
    Action: reconciliation workflow inspection.
    False-positive risk: medium — provider weekend maintenance is common
    cause; verify before paging.

Silent alert (no page, dashboard signal):
  payment_amount_above_threshold_total > 10 in 1 hour.
    Action: fraud team triage during business hours.
    False-positive risk: high — large legitimate transactions are common.

Anomaly alert (statistical, no fixed threshold):
  payment_attempt_rate deviation > 3 standard deviations from baseline for
  this hour-of-week.
    Action: log and notify; do not page.
    False-positive risk: high — promotional events cause large legitimate
    deviations.

Forbidden alerts: payment_attempt_total > N (any fixed N) — total transaction
volume is not a health signal. payment_failure_rate{provider="test"} —
test-provider failures are expected and must not page on-call.

Alert governance rule: every alert must specify the action expected of the
on-call engineer. An alert without a documented action is forbidden — it
trains the on-call to ignore alerts.

REJECTED — 4.1 Backend Services:
Inject IMetrics into PaymentProcessingService. Call
_metrics.Counter("payment_attempts").Increment() before the provider call
and _metrics.Counter("payment_failures").Increment() in the catch block.
Call _metrics.Histogram("payment_latency_ms").Record(elapsed) on the way
out. Wrap the provider call in an Activity span with tags for provider name
and amount. Add a try/catch around the metrics calls themselves so a
metrics outage does not break payment processing.
[WRONG: "what alerts should fire" asks for the alert rule definitions —
which metric, what threshold, what window, what severity, what response. The
rejected wires the metric-emission code, which is the implementation that
follows the alert decisions. Without the alert thresholds defined first, the
metric labels and aggregation chosen at the implementation layer often do not
support the alert that needs to fire — for example, emitting only a counter
without provider labels means a per-provider alert is impossible.]
```

---

## Pair AP1-3.5-004 — trace design question confused with monitoring setup

```
source_id: xiigen-arch-plan-batch-001-AP1-3.5-004
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.5
rejected_sub_phase: 7.3
```

Prompt: `"design the trace structure for the order-to-fulfillment pipeline so that a slow order can be debugged from a single trace id"`

```
CHOSEN — 3.5 Observability and Operational Design:
One trace per order. Spans are nested by causal stage, not by service.

Trace shape:
  Root span: order.placed (orderId, tenantId-hashed, total_amount_bucket).
    Child span: order.validated (validation_outcome).
      Child span: payment.charged (provider, latency, outcome).
        Child span: inventory.reserved (warehouse_id, lines_reserved).
          Child span: shipment.scheduled (carrier, ship_by_date).
            Child span: notification.sent (channel, outcome).

Causal nesting rule: a span is a child of the span that caused it, not of
the span that called it. payment.charged is a child of order.validated
because payment depends on validation passing. notification.sent is a
child of shipment.scheduled because the notification is the consequence of
the shipment being scheduled. This makes a single trace tell the causal
story even when calls cross service boundaries asynchronously.

Span attributes (each span):
  outcome (success / skipped / failed).
  elapsed_since_root_ms — time from order.placed to this span.
  retry_count if the operation was retried.

Forbidden in any span: the customer's full name, address, payment method
details (last four digits is the maximum representation), shipping
address city-and-state-and-zip together (state-and-country only).

Trace propagation rule: the orderId is the trace correlation id. Every
service that handles part of the order pipeline must set orderId as the
trace baggage. A service that loses the orderId on an asynchronous boundary
breaks the entire diagnostic capability — the rule is enforced in CI by
contract tests against each service's outbound integrations.

Sampling rule: all order traces are retained for 7 days; 1% are retained
for 90 days for trend analysis. No retention beyond 90 days because the
trace data contains tenant ids and behavioral patterns that fall under the
data minimization policy.

REJECTED — 7.3 Monitoring and Feedback Loop:
Configure Jaeger as the trace backend. Set up the Jaeger UI on the operations
dashboard. Configure trace sampling at 100% for the first month, then reduce
to 10%. Add a trace search by orderId in the dashboard. Document the trace
viewing procedure for the on-call engineers. Set up an alert when the trace
ingestion rate drops below 95% of the expected rate.
[WRONG: "design the trace structure" asks for the span hierarchy, what
attributes each span carries, what is forbidden, and how trace correlation
flows across asynchronous boundaries. Configuring Jaeger and the dashboard is
infrastructure setup that comes after the trace structure is designed. Without
the structure, every service emits its own span shape and the trace becomes
unreadable — the dashboard shows traces but no single trace tells the order's
causal story.]
```

---

## Pair AP1-3.5-005 — what-to-measure decision confused with logger implementation

```
source_id: xiigen-arch-plan-batch-001-AP1-3.5-005
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 3.5
rejected_sub_phase: 4.1
```

Prompt: `"what should we measure to know that the matching algorithm is working well — not that it is running, but that it is producing good matches?"`

```
CHOSEN — 3.5 Observability and Operational Design:
Three quality signals, each with a defined "good" threshold. None of these
are operational signals — they answer the question "is the algorithm doing
its job", not "is the service running".

Match acceptance rate (counter ratio):
  Numerator: matches that the user accepted within 24 hours (chat opened,
  positive feedback, mutual interaction).
  Denominator: matches presented to the user.
  Target: > 35% over a rolling 7-day window. Below 25% indicates the
  algorithm is producing irrelevant matches.

Time-to-first-mutual-interaction (histogram):
  From match presented to first mutual interaction.
  Target p50 < 2 hours; p95 < 24 hours.
  Drift signal: a rising p50 means matches are getting less compelling.

Match diversity (categorical distribution):
  For each user, the variance in matched-user attributes (age, location,
  interest cluster) over their last 50 matches.
  Target: variance is non-zero for > 90% of users.
  A user with zero variance is being shown a narrow band — either the
  algorithm has overfit to a single signal or the user pool is exhausted.

These are quality signals — they cannot be inferred from operational logs.
Acceptance rate requires correlating "match presented" events with "chat
opened" events that may happen days later. Match diversity requires aggregating
each user's match history. The quality signals must be persisted to a time-
series store with at least 14-day retention, separate from operational logs.

Forbidden: this dataset must not contain user identifiers. Aggregate by
user-cohort hash, not user id. Quality signals do not need per-user precision;
they need cohort-level trends.

Alert rule: match acceptance rate < 25% for 3 days — escalate to algorithm
team. The 3-day window prevents single-day noise from triggering escalation.

REJECTED — 4.1 Backend Services:
Inject ILogger<MatchingService> and IMetrics into MatchingService. Call
_logger.LogInformation("Match generated for {userId} with score {score}",
userId, score) when a match is created. Call _metrics.Counter
("match_generated_total").Increment() in the same path. Add a Polly
fallback policy that increments a "match_generation_failed" counter when
the algorithm throws.
[WRONG: "what should we measure" asks for the quality signals — what numbers
tell us the algorithm is producing good matches, not just running. Logging
"match generated" tells us the algorithm is running. It does not tell us
whether the matches are good. The signals required (acceptance rate over
24 hours, time-to-mutual-interaction, diversity) cannot be expressed by a
counter at the algorithm's emit point — they require correlation across
later events. The decision about which quality signals to track must be
made first; the logger and metric calls are the implementation.]
```
