# Node 02 — Domain Translator — Clean Batch 002

Node 2 receives one prompt fragment plus one domain label emitted by Node 1.
It produces a formal, precise description of what that fragment requires, written in the vocabulary of that specific domain.

A is the exact fragment plus the exact label — both as they would arrive at runtime.
B is the formal domain description itself — not a summary of what the description would cover.
A B that says "a formal description using domain vocabulary" is wrong — the formal description must be written out in full.

One pair covers one fragment-label pair. Array nodes receive one element at a time.

---

Pair CB2-02-001 — React scroll fragment: C-frontend-rendering-performance translation
Source: https://react.dev/learn — Advanced patterns: rendering and interaction
License: CC-BY 4.0
A: Fragment: "2,000 items in the DOM, scrolling drops below 30fps"
Label: "C-frontend-rendering-performance"
B: C-frontend-rendering-performance: 2,000 concurrent DOM nodes are mounted and maintained in the browser layout tree. On each scroll event, the browser recalculates layout and style across the full node set before compositing the frame. At 2,000 nodes, this recalculation exceeds the 16ms frame budget required for 60fps. The observed symptom — below 30fps — confirms that layout thrashing occurs on every scroll tick. The DOM node count is the root cause; the scroll event listener mechanism is a secondary concern. Required outcome: reduce the active mounted node count to the viewport-visible count plus a small buffer. Primary mechanism: virtual list windowing — render only the nodes visible within the scrollable container's viewport, plus a prefetch buffer of one screenful above and below. Secondary mechanism: replace the scroll event listener with IntersectionObserver to eliminate per-tick layout queries. A virtual list implementation renders 15–30 nodes regardless of total item count, maintaining layout recalculation well within the 16ms budget.
Reason: The formal description replaces the natural-language fragment with precise rendering-domain terms: layout tree, frame budget, layout thrashing, viewport-visible count, virtual list windowing. These terms allow Node 3 to produce a plan that specifies the mechanism (windowing) and the target state (16ms budget) rather than repeating the symptom.
Source: https://react.dev/learn — Advanced patterns: rendering and interaction
License: CC-BY 4.0

Pair CB2-02-001 — Phrasing B
Source: https://react.dev/learn — Advanced patterns: rendering and interaction
License: CC-BY 4.0
A: Fragment: "We have 2,000 items rendering and scroll is janky — under 30 frames per second"
Label: "C-frontend-rendering-performance"
B: C-frontend-rendering-performance: 2,000 DOM nodes are mounted simultaneously. Browser layout recalculation across 2,000 nodes exceeds the 16ms frame budget on every scroll event, producing the observed frame rate below 30fps. Cause is node count, not the event handler itself. Correction: virtual list windowing reduces the mounted node count to the viewport-visible range plus a one-screen buffer regardless of total item count. Target state: layout recalculation completes within 16ms (60fps) with approximately 20–30 mounted nodes at any scroll position.

---

Pair CB2-02-002 — Django N+1 fragment: D-database-query-efficiency translation
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Fragment: "101 queries: one for the posts and one per post for the author"
Label: "D-database-query-efficiency"
B: D-database-query-efficiency: The N+1 query pattern is active. A single parent query loads N parent objects; one child query is dispatched per parent object to load one related record. Total database round trips: N+1. In this case: 1 query for 100 Post objects, then 100 queries each loading one auth_user row by primary key. Each of the 100 child queries is a SELECT * FROM auth_user WHERE id = %s with a different primary key value. The combined dispatch latency and query execution overhead of 100 individually issued queries is the source of the 4.2-second render time. Required outcome: replace the 101-round-trip pattern with a single JOIN query that retrieves all 100 post rows and their 100 corresponding author rows in one database call. Target state: 1 database query — a JOIN between the blog_post table and the auth_user table on the author foreign key, returning all requested fields in one round trip, limited to 100 rows. No additional queries triggered by template attribute access.
Reason: The formal description replaces "101 queries" with its structural name (N+1 pattern), names the mechanism causing it (per-object lazy load dispatch), and states the target state in query terms. Node 3 can now produce a plan that names the specific ORM call required (select_related) and the acceptance condition (1 query, confirmed by query count assertion).
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT

Pair CB2-02-002 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Fragment: "100 posts load fine but then 100 more queries fire, one per author"
Label: "D-database-query-efficiency"
B: D-database-query-efficiency: N+1 query pattern detected. 1 query returns N parent rows; N queries each return 1 related row. Round trips to the database: N+1 = 101. Each author query is a SELECT by primary key — individually cheap but collectively responsible for the majority of the 4.2-second latency due to 100 sequential round-trip wait times. Correction: collapse to 1 JOIN query returning all parent and related rows in one round trip. ORM mechanism: select_related('author'). Target: 1 query, 0 lazy-load follow-up queries regardless of result set size.

---

Pair CB2-02-003 — BFF fragment: A-api-topology-selection translation
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: Fragment: "three frontends need 8, 4, and 2 fields respectively"
Label: "A-api-topology-selection"
B: A-api-topology-selection: Three distinct client types each require a different subset of the backend data model. Client 1 (web): 8 fields per request. Client 2 (mobile): 4 fields per request, bandwidth-constrained. Client 3 (IoT dashboard): 2 fields per request, 10-second polling interval. A shared API gateway returns a fixed response schema to all clients; it cannot perform client-specific field projection without bespoke filtering logic in a shared layer. Shared filtering logic couples client-specific requirements to the gateway, producing a single-deployment coupling across all three client types. Required topology: three independent API layers, one per client type, each with its own deployment lifecycle and field projection contract covering exactly the fields its client requires. Each layer aggregates from the same downstream microservices but projects a different subset. Deployment of one layer requires no coordination with the other two. Changes to the mobile field set require only a deployment of the mobile-facing layer. The downstream microservices are unchanged.
Reason: The formal description replaces "three frontends need different fields" with the structural name (field projection mismatch), names the failure mode of a shared gateway (single-deployment coupling), and names the correct pattern (BFF) with its deployment-independence property. Node 3 can plan deployment count, independence constraints, and scaling requirements from this description.
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT

Pair CB2-02-003 — Phrasing B
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: Fragment: "web needs 8 fields, mobile needs 4, IoT needs 2 — they can't all use the same API response"
Label: "A-api-topology-selection"
B: A-api-topology-selection: Field projection mismatch across three client types with different data requirements. A single gateway endpoint returns one schema — all clients receive the same superset of fields and must discard what they do not need. For the mobile client this wastes bandwidth; for the IoT client this wastes both bandwidth and processing on a constrained device. Correct topology: three independent API services, one per client type, each aggregating and projecting exactly the field set its client requires. Deployment is independent per service — changes to the mobile field set require deploying only the mobile-facing service. Three new services are introduced; the downstream microservices are not modified.

---

Pair CB2-02-004 — Karpenter fragment: A-node-provisioning-strategy translation
Source: https://github.com/kubernetes/enhancements — keps/sig-autoscaling
License: Apache 2.0
A: Fragment: "nodes take 2-5 minutes to provision"
Label: "A-node-provisioning-strategy"
B: A-node-provisioning-strategy: Cluster Autoscaler node provisioning latency of 2-5 minutes is inherent to the watch-and-scale loop architecture: detect unschedulable pods, select a node group, call the cloud provider API to increment node group size, wait for the cloud provider to fulfill the instance request and register the node. The loop interval is 10-60 seconds; instance request fulfillment adds 60-120 seconds; node registration and kubelet startup adds further time. Total: 2-5 minutes before pods can be scheduled on the new node. For workloads that burst from near-zero to 100 pods, this provisioning window blocks all 100 jobs for its full duration. Required outcome: reduce provisioning latency to under 60 seconds. Replacement: Karpenter. Karpenter observes unschedulable pods via the Kubernetes scheduler's unschedulable queue, selects a specific EC2 instance type matching pod requirements at scheduling time, and calls the EC2 Fleet API directly. Provisioning time: 30-60 seconds from pod unschedulable to node ready. Karpenter selects instance types per workload rather than per pre-defined node group, enabling spot instance selection and cost-aware scheduling without pre-provisioned node pools.
Reason: The formal description replaces "nodes take 2-5 minutes" with the structural cause (watch-loop latency, instance request fulfillment, kubelet startup), names the target state (30-60 seconds with Karpenter), and names the mechanism difference (direct EC2 Fleet API call versus node group expansion). Node 3 can plan the migration: disable Cluster Autoscaler, deploy Karpenter, configure NodePool, validate provisioning speed on a controlled burst. Note: the 30-60 second target is derived from Karpenter's documented provisioning characteristics (direct EC2 Fleet API call latency) — it is not present in the A fragment. This target is domain knowledge added by Node 2, not a translation of the A input.
Source: https://github.com/kubernetes/enhancements — keps/sig-autoscaling
License: Apache 2.0

Pair CB2-02-004 — Phrasing B
Source: https://github.com/kubernetes/enhancements — keps/sig-autoscaling
License: Apache 2.0
A: Fragment: "cluster autoscaler is too slow — 2 to 5 minutes before batch jobs can start"
Label: "A-node-provisioning-strategy"
B: A-node-provisioning-strategy: Cluster Autoscaler provisioning latency of 2-5 minutes is caused by the watch-loop-expand-wait sequence: loop fires, node group size is incremented, cloud provider fulfills the request, kubelet registers. Each step adds latency; the combined path is 2-5 minutes. Replacement: Karpenter provisions in 30-60 seconds by calling the EC2 Fleet API directly with a specific instance type selected at scheduling time, rather than expanding a pre-defined node group. Karpenter also evaluates spot availability at scheduling time, reducing compute cost without pre-configured pool definitions. Migration sequence: deploy Karpenter alongside existing Cluster Autoscaler, configure NodePool with required instance constraints, validate provisioning speed on a controlled burst, then disable Cluster Autoscaler. Note: the 30-60 second target is derived from Karpenter's documented provisioning characteristics, not from the A fragment — it is domain knowledge added by Node 2.
