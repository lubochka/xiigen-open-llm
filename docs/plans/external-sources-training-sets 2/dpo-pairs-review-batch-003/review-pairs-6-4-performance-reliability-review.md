# Review Pairs — Sub-phase 6.4 Performance and Reliability Review
# Batch: dpo-pairs-review-batch-003
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: latency, load, reliability, resource usage, operational resilience checks
# Pair count: 4

---

## Pair AP3-6.4-001 — performance review confused with load test design

```
source_id: xiigen-review-batch-003-AP3-6.4-001
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.4
rejected_sub_phase: 5.4
```

Prompt: `"review the performance results from the latest load run and decide whether the new search service is ready for production traffic"`

```
CHOSEN — 6.4 Performance and Reliability Review:
Review the load test output against the published performance budget and
disposition each gap.

What the reviewer reads:

Performance budget (the contract):
  p50 latency: < 80 ms.
  p95 latency: < 250 ms.
  p99 latency: < 600 ms.
  Error rate: < 0.1%.
  Throughput target: 2000 RPS sustained for 30 minutes.

Load run output (from the test suite):
  p50 = 65 ms — within budget.
  p95 = 320 ms — over budget.
  p99 = 1100 ms — over budget.
  Error rate = 0.05% — within budget.
  Throughput = 2000 RPS sustained for 28 minutes; the run was stopped
  at 28 minutes because of a memory pressure alert on one node.

Gap disposition:

p95 over budget (320 vs 250):
  The reviewer reads the trace samples for slow requests. Finds that
  95% of slow requests are queries with > 100k matching documents that
  hit the deep-pagination path.
  Disposition: not a blocker for release if deep-pagination queries are
  rare in production (verify against production query logs). Add a
  monitor for deep-pagination query rate; revisit if the rate exceeds
  1% of total queries.

p99 over budget (1100 vs 600):
  The reviewer reads the trace samples. Finds that the slowest queries
  correlate with index merging events on the underlying storage.
  Disposition: blocker. Index merge is a known long pause; the search
  service must shed load during merges or the merges must be scheduled
  outside peak hours. Reviewer requires a fix or a documented mitigation
  before release.

Throughput sustained 28/30 min (memory pressure):
  The reviewer reads the memory-pressure alert. Finds that the heap
  reaches 90% during the run, which triggers GC pauses that contribute
  to the p99 issue.
  Disposition: blocker — same blocker as p99. The fix is the same
  (load shedding or merge scheduling) and the heap headroom must be
  verified after the fix.

Sign-off:
  Reviewer marks the release as blocked, names the two blockers (p99,
  memory pressure during merge) and the required evidence (a clean
  re-run with both metrics within budget for 30 minutes).
  The release-readiness review (6.2) cannot proceed until 6.4 sign-off
  is in place.

REJECTED — 5.4 Performance and Load Tests:
Design a load test that ramps to 2000 RPS over 5 minutes, sustains for
30 minutes, then ramps down. Use k6 with a custom scenario script. Define
thresholds: p95 < 250 ms (warning), p95 < 500 ms (failure); p99 < 600 ms
(warning), p99 < 1000 ms (failure); error rate < 0.1% (failure). Run from
three load generators in different regions. Save the results as a CSV
for trend analysis. Schedule the test to run nightly against staging.
[WRONG: "review the performance results... and decide whether the service
is ready" asks for a review-and-disposition activity. Designing a load
test is a different sub-phase (5.4) that happens before the review can
exist — the test produces the data the review interprets. The review's
output is a go/no-go decision and a list of blockers; it is not a test
specification. Treating "review the results" as "design more tests" skips
the disposition step that the release-readiness review depends on.]
```

---

## Pair AP3-6.4-002 — reliability review confused with monitoring setup

```
source_id: xiigen-review-batch-003-AP3-6.4-002
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.4
rejected_sub_phase: 7.3
```

Prompt: `"review the new payment service's reliability posture before we go live — what failure modes have we tested for and what is still untested?"`

```
CHOSEN — 6.4 Performance and Reliability Review:
Reliability review enumerates known failure modes, the test that exercises
each, and the result. Untested modes are explicitly listed as risks.

Failure-mode register for the payment service:

FM-1: provider API outage:
  Test executed: chaos-injection during a 1-hour load run; the provider
  endpoint was returned a 503 for 5 minutes mid-run.
  Result: the service correctly switched to the secondary provider
  within 30 seconds. p95 latency increased by 40% during the failover
  window.
  Disposition: tested, behaved as designed. The 40% latency bump is
  acceptable per the SLA.

FM-2: database connection exhaustion:
  Test executed: connection pool sized at 50; load generator saturated
  the pool.
  Result: the service queued requests beyond pool capacity; queue depth
  alarmed at 100. Recovery was clean within 30 seconds of the load
  reducing.
  Disposition: tested, behaved as designed. Connection pool monitoring
  is in place.

FM-3: webhook delivery delay:
  Test executed: provider webhooks were delayed by 10 minutes via a
  network proxy.
  Result: the order state remained "pending" until the webhook arrived.
  Customer-visible UI showed a "processing" state for the duration.
  Disposition: tested. Acceptable for the launch window; revisit if
  customer complaints arise.

FM-4: idempotency-key collision:
  Test executed: deliberately reused an idempotency key with different
  request bodies.
  Result: the second request was rejected with the documented error.
  Disposition: tested.

FM-5: double-charge under retry storm:
  Test NOT executed.
  Risk: a network issue causes the client to retry a charge that
  succeeded server-side; without the idempotency check, this could
  double-charge.
  Mitigation: idempotency is enforced (FM-4 tested), but no chaos test
  has run with concurrent retries from the same client.
  Disposition: known untested mode. The reviewer requires either a
  chaos test before launch OR documented mitigation that the on-call
  is aware to monitor for during the first week.

FM-6: provider key rotation in flight:
  Test NOT executed.
  Risk: keys are rotated mid-day during a reload of the service.
  Disposition: untested. Reviewer accepts the risk because key rotation
  is a planned event with a runbook; the on-call team will rehearse
  before the next rotation.

Sign-off:
  Reviewer signs the reliability review with the FM register attached.
  The release-readiness review (6.2) reads this register and includes
  the untested modes in the residual-risk inventory.

REJECTED — 7.3 Monitoring and Feedback Loop:
Configure the dashboards for the payment service: provider-API success
rate, webhook latency p95, connection-pool saturation, error rate by
provider, idempotency-key duplicate rate. Set up alerts for each: page
on-call when provider success rate drops below 95% for 5 minutes,
silent alert when webhook latency p95 exceeds 5 seconds for 10 minutes.
Document the dashboard layout in the runbook.
[WRONG: reliability review enumerates failure modes, names which ones
have been tested, and disposes of the untested ones. Monitoring setup
configures the tooling that will observe production behaviour after launch.
The two activities cover different points in time: review happens before
launch and asks "what could break", monitoring happens after launch and
asks "is something broken right now". The reviewer's output is a
disposition register, not a dashboard configuration.]
```

---

## Pair AP3-6.4-003 — performance review confused with observability design

```
source_id: xiigen-review-batch-003-AP3-6.4-003
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.4
rejected_sub_phase: 3.5
```

Prompt: `"review the production-traffic performance of the order service over the last 30 days and decide whether the SLO is being met"`

```
CHOSEN — 6.4 Performance and Reliability Review:
Read the SLO definition. Pull the actual measurements from the
observability store. Compare. Issue findings.

SLO contract (from the SLO doc):
  Availability: 99.9% over a 30-day window.
  p95 latency for POST /orders: < 400 ms.
  p99 latency for POST /orders: < 1 s.
  Error budget: 43 minutes of unavailability per month.

Measurements from the observability store (last 30 days):

Availability:
  Total request count: 8.2M.
  Failed (5xx) request count: 9100.
  Computed availability: 99.89%.
  Result: SLO BREACHED. Error budget burned: 49 minutes (over the
  43-minute budget).

Per-incident contribution:
  Incident on day 12: provider outage caused 22 minutes of cascading 5xx.
  Incident on day 27: deployment introduced a 15-minute regression
  before rollback.
  Other 12 minutes scattered across small windows.

p95 latency:
  Daily p95 over the 30 days: ranges 280 - 380 ms.
  Result: SLO MET.

p99 latency:
  Daily p99: ranges 800 ms - 1.4 s.
  Worst day was day 12 (incident day).
  Result: SLO BREACHED on 1 of 30 days.

Disposition:

Availability breach:
  Reviewer escalates to the SRE team for an incident review of day 27
  (preventable through canary).
  Reviewer recommends a feature freeze for the next 30 days to recover
  error budget; this is the team's documented response policy when SLO
  burns by more than 10% in a window.

p99 breach:
  Single-day breach driven by the day 12 incident. No standing fix
  required because the incident is closed.

Sign-off:
  Reviewer signs the SLO review report. Recommendations attached:
  feature freeze for 30 days, day 27 incident review by SRE.
  The next monthly review is scheduled.

REJECTED — 3.5 Observability and Operational Design:
Define the observability strategy for the order service. Metrics:
order_request_total, order_latency_ms (histogram), order_failure_total
(by reason). Traces: one span per order, with the provider, amount tier,
and outcome as tags. Logs: structured JSON, INFO on success and ERROR
on failure with correlation_id. Alerts: page on-call when failure rate
exceeds 0.5% for 5 minutes. Document this as the observability strategy.
[WRONG: "review the performance over the last 30 days" asks for a review
of the existing observability data against the SLO. Defining the
observability strategy is what already happened (or should have); the
review uses the data the strategy produces. Returning to the strategy
when an SLO review was asked for skips the SLO disposition entirely and
does not answer whether the SLO was met.]
```

---

## Pair AP3-6.4-004 — resource-usage review confused with load test design

```
source_id: xiigen-review-batch-003-AP3-6.4-004
source_license_state: first_party
pair_kind: dpo_review
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.4
rejected_sub_phase: 5.4
```

Prompt: `"review the resource usage of the new pipeline service in staging — CPU, memory, and network — to decide if the production node sizing is appropriate"`

```
CHOSEN — 6.4 Performance and Reliability Review:
Read the staging telemetry from the last 14 days. Compare against the
proposed production node sizing. Decide.

Proposed production sizing (from the architecture document):
  Pod resources: 2 vCPU request, 4 vCPU limit; 4 GB memory request,
  8 GB memory limit.
  Replica count: 6.

Staging telemetry (from the running pods):

CPU usage:
  Median CPU utilisation: 0.8 cores per pod.
  p95 CPU utilisation: 1.6 cores per pod.
  Peak observed: 2.1 cores per pod (during a batch retry storm).
  Disposition: peak slightly exceeds the request (2.0). Throttling is
  observed in the metrics. Recommendation: increase the request to
  2.5 cores OR keep request at 2 and rely on the limit for peaks but
  document this as accepted throttling.

Memory usage:
  Median: 3.2 GB.
  p95: 4.4 GB.
  Peak: 5.1 GB.
  Disposition: median exceeds the request (3.2 vs 4.0 — wait, the
  request is 4 GB. Median 3.2 GB means usage is below the request,
  which is correct.) p95 4.4 GB is above the request, which means
  occasional memory pressure but well within the 8 GB limit. Acceptable.

Network usage:
  Egress per pod: 30 MB/s sustained, 80 MB/s peak.
  Disposition: well within node capacity; no constraint.

Replica count:
  Staging traffic is 25% of expected production traffic. Linearly
  extrapolated, peak production load on 6 replicas would be near the
  CPU limit. Disposition: increase replica count to 8 OR validate the
  CPU sizing recommendation above.

Reviewer's recommendation:
  Adopt: CPU request 2.5 cores, memory unchanged, replica count 8.
  Or: CPU request 2 cores (accept occasional throttling), memory
  unchanged, replica count 10.
  Either combination satisfies the workload. Reviewer flags both for
  the architecture team's choice.

REJECTED — 5.4 Performance and Load Tests:
Design a load test that simulates production peak load against a single
pod. Use k6 to generate 1000 RPS sustained for 1 hour. Capture CPU and
memory metrics during the run. Repeat the test with different pod sizing
configurations (1 vCPU, 2 vCPU, 4 vCPU) to find the smallest pod that
sustains the load without throttling. Output a sizing recommendation
based on the test results.
[WRONG: the prompt says "review the resource usage in staging... to
decide if production node sizing is appropriate". The data already exists
in staging telemetry; the reviewer interprets it and makes a sizing
recommendation. Designing a new load test is additional work that the
review may recommend but is not the review itself. The output of the
review is a disposition (sizing recommendation with justification), not
a test plan. The two sub-phases produce different artifacts at different
times.]
```
