# Node 01 — Domain Splitter — Clean Batch 001

Node 1 receives a user prompt and emits an array of domain labels.
A is the exact natural-language prompt as a user would type it.
B is the exact array of domain labels the splitter must emit.
A B that says "domains related to performance" is wrong — the label names themselves must appear.

---

Pair CB1-01-001 — React scroll performance
Source: https://react.dev/learn — Advanced patterns: rendering and interaction
License: CC-BY 4.0
A: We have a React social feed that renders up to 2,000 items. As more posts load, scrolling becomes janky — it drops below 30 fps. We currently attach an onScroll listener to the container div and check scrollTop + clientHeight >= scrollHeight minus a threshold to decide when to load the next page. How do we fix the scroll performance while keeping the infinite-loading behavior?
B: ["C-frontend-rendering-performance", "C-dom-node-count", "B-user-interaction-latency", "G-browser-api-selection"]
Reason: The prompt crosses four territories: (1) frontend rendering — the jank is a rendering performance problem; (2) DOM node count — 2,000 rendered items is the root cause; (3) user-interaction latency — the user perceives the jank as a scroll delay; (4) browser API selection — the fix requires choosing between the scroll event API and the IntersectionObserver API, which is a technology-selection decision.

Pair CB1-01-001 — Phrasing B
Source: https://react.dev/learn — Advanced patterns: rendering and interaction
License: CC-BY 4.0
A: Our React feed list has 2,000 items in the DOM and scrolling is slow — under 30 frames per second. We're using an onScroll handler on the list container to detect when the user is near the bottom. What functional areas does this problem touch?
B: ["C-frontend-rendering-performance", "C-dom-node-count", "B-user-interaction-latency", "G-browser-api-selection"]

---

Pair CB1-01-002 — Kubernetes cluster autoscaler for burst workloads
Source: https://github.com/kubernetes/enhancements — keps/sig-autoscaling
License: Apache 2.0
A: We run batch jobs on AWS EKS. During daily processing windows the cluster needs to go from near zero pods to 100 pods in under 5 minutes. The current Cluster Autoscaler takes 2 to 5 minutes just to provision new nodes, which means batch jobs are queued for most of the burst window. We want faster node provisioning and cost-efficient scale-down when the burst ends.
B: ["A-cluster-autoscaling-topology", "B-workload-scheduling-latency", "F-cloud-cost-optimization", "A-node-provisioning-strategy"]
Reason: The prompt crosses four territories: (1) cluster autoscaling topology — the design of the autoscaler itself is being replaced; (2) workload scheduling latency — the burst jobs cannot start until nodes exist; (3) cloud cost optimization — scale-down efficiency is a direct cost concern; (4) node provisioning strategy — the decision between Cluster Autoscaler and Karpenter is a provisioning-strategy decision.

Pair CB1-01-002 — Phrasing B
Source: https://github.com/kubernetes/enhancements — keps/sig-autoscaling
License: Apache 2.0
A: EKS cluster, AWS. Batch jobs need 100 pods provisioned within 5 minutes, but Cluster Autoscaler is too slow — nodes take 2 to 5 minutes to appear. After the batch window we want idle nodes to go away quickly to save money. What domains does this span?
B: ["A-cluster-autoscaling-topology", "B-workload-scheduling-latency", "F-cloud-cost-optimization", "A-node-provisioning-strategy"]

---

Pair CB1-01-003 — Django ORM N+1 query
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: A Django view loads 100 blog posts and their authors for a feed page. The page takes 4.2 seconds to render. Profiling shows 101 queries: one to load the posts and then one per post to load the author's profile. The template accesses post.author.name for every post. How do I fix this?
B: ["D-database-query-efficiency", "E-orm-usage-pattern", "B-backend-response-latency"]
Reason: The prompt crosses three territories: (1) database query efficiency — 101 round trips to the database is an N+1 query problem at the storage layer; (2) ORM usage pattern — the fix requires using the ORM correctly (select_related) rather than changing the database; (3) backend response latency — 4.2 seconds is a response-time problem that the fix must resolve.

Pair CB1-01-003 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Django blog feed, 100 posts, 4.2 seconds to render. Database profiling: 101 queries — 1 for posts, then 1 per post to fetch the author. Template uses post.author.name in a loop. What functional territories does this problem span?
B: ["D-database-query-efficiency", "E-orm-usage-pattern", "B-backend-response-latency"]

---

Pair CB1-01-004 — Multi-tenant API gateway: BFF versus single gateway
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: We have a .NET microservices backend. Three different frontends need to use it: a web app that needs 8 data fields per request, a mobile app that is bandwidth-constrained and only needs 4 fields, and an IoT dashboard that needs 2 fields every 10 seconds. What is the right API gateway design?
B: ["A-api-topology-selection", "C-client-data-shape-translation", "B-bandwidth-and-latency-constraints", "G-system-complexity-tradeoff"]
Reason: The prompt crosses four territories: (1) API topology — gateway versus BFF is a topology decision; (2) client data shape translation — each client needs a different subset of fields, which is a translation problem; (3) bandwidth and latency constraints — the mobile client has a specific constraint; (4) system complexity tradeoff — adding separate BFFs adds services, which is a complexity cost.

Pair CB1-01-004 — Phrasing B
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: Three frontends consume our .NET microservices: web (8 fields per call), mobile (4 fields, bandwidth matters), IoT dashboard (2 fields, 10-second poll). They need different data shapes from the same backend services. What areas does this design decision touch?
B: ["A-api-topology-selection", "C-client-data-shape-translation", "B-bandwidth-and-latency-constraints", "G-system-complexity-tradeoff"]
