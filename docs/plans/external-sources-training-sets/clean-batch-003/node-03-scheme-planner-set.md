# Node 03 — Scheme Planner — Clean Batch 003

Node 3 receives all formal domain descriptions from the Node 2 array and produces an ordered set of sub-tasks.
Each sub-task is paired with a transformational prompt that tells the applier exactly how to approach it.
The plan must include: ordered steps, parallel steps where allowed, wait states, recovery paths, and review points.

A is the complete set of formal domain descriptions received from Node 2 at runtime.
B is the actual plan with actual steps — not a description of what the plan would contain.
A B that says "create a plan with phases" is wrong — the actual plan with actual steps must appear.

---

Pair CB3-03-001 — Django N+1 scheme plan
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: [D-database-query-efficiency] The N+1 query pattern is active. A single parent query loads N parent objects; one child query is dispatched per parent object to load one related record. Total database round trips: N+1. In this case: 1 query for 100 Post objects, then 100 queries each loading one auth_user row by primary key. Each of the 100 child queries is a SELECT * FROM auth_user WHERE id = %s with a different primary key value. The combined dispatch latency and query execution overhead of 100 individually issued queries is the source of the 4.2-second render time. Required outcome: replace the 101-round-trip pattern with a single JOIN query that retrieves all 100 post rows and their 100 corresponding author rows in one database call. Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. No additional queries triggered by template attribute access.

[E-orm-usage-pattern] Fragment: "The template accesses post.author.name in a loop." The ORM traversal triggers a lazy load on each iteration — Post.author is accessed per row, and the Django ORM fetches the related object with a new SELECT on each access if the relation is not pre-fetched. Domain label present; formal translation not yet produced by Node 2.

[B-backend-response-latency] Fragment: "4.2 seconds to render." Render latency is the observable symptom of the N+1 dispatch overhead. Domain label present; formal translation not yet produced by Node 2.
B:
Step 1 — Confirm ForeignKey relation type [prerequisite; blocks all subsequent steps]
Transformational prompt: Check the Post model definition file (typically models.py in the blog application). Locate the author field. Passing form: author = models.ForeignKey(<UserModel>, on_delete=<any value>) or author = models.OneToOneField(<UserModel>, on_delete=<any value>) — any ForeignKey or OneToOneField confirms select_related is valid for this relation. Failing form: author = models.ManyToManyField(<UserModel>) or a reverse relation accessed through related_name — in the failing case, select_related raises FieldError at query evaluation time; use prefetch_related('author') instead and do not proceed with Steps 2 through 4 as written.
Wait state: Confirm the field type before writing any code change. The mechanism in Step 2 is conditional on this check.

Step 2 — Replace the queryset with the select_related form [depends on Step 1 confirming ForeignKey]
Transformational prompt: In the view function that loads the blog feed, replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100]. The modifier must appear before the slice — Post.objects.all()[:100].select_related('author') is incorrect and Django silently ignores the modifier in that position, leaving the N+1 pattern intact. The corrected queryset generates one SQL query: SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. The ORM caches the auth_user result on each Post instance from the JOIN row. Template access to post.author.name reads from that cached object — no additional database query is triggered.

Step 3 — Verify with Django Debug Toolbar [depends on Step 2]
Transformational prompt: Load the blog feed page with Django Debug Toolbar active. The SQL panel must show exactly 1 query for the full page request, down from 101. If the count is above 1, the select_related modifier was not applied before the slice. Confirm the queryset form matches Post.objects.select_related('author').all()[:100] exactly. If any other template access triggers additional queries (e.g., post.category, post.tags), those are separate N+1 patterns on other relations — record them but do not fix them in this step.
Wait state: Do not proceed to Step 4 until the Toolbar confirms query count = 1 for this request. A test written against a broken queryset asserts incorrect behavior.

Step 4 — Add assertNumQueries regression test [depends on Step 3 confirming query count = 1]
Transformational prompt: In the test suite for the blog feed view, add a test case using Django's assertNumQueries context manager: with self.assertNumQueries(1): response = self.client.get('/feed/'). The assertion target is exactly 1 — not "fewer than 102" and not "under 10." The exact count prevents any future ORM change from silently reintroducing lazy loading on this view without a test failure.

Recovery path — assertNumQueries and Toolbar counts differ: If the Django Debug Toolbar shows 1 query but assertNumQueries returns a different count, the test fixture loading path triggers additional setup queries. Wrap only the view call, not the fixture setup, inside the assertNumQueries context manager. If the count is consistently 2 or 3, inspect the Toolbar panel during a test run to identify which query is additional.

Recovery path — both Toolbar and assertNumQueries pass but production query count remains at 101: If Step 3 and Step 4 both report 1 query but the production feed still shows 101 queries under live traffic, the select_related modifier was applied to a queryset that is not used by the production view. Confirm that the view function tested in the test suite is the same function serving the production feed endpoint. If the application has multiple view functions or class-based view mixins that load Post objects, the queryset change must be applied to the production-path queryset specifically. Deploy to a staging environment and repeat the Toolbar measurement on the production-path URL before closing this fix.

Review point: After Step 4, measure the page render time under representative load (100 posts, typical author data). Target: under 0.5 seconds, down from 4.2 seconds. If render time remains above 1 second despite query count = 1, profile the template for additional lazy-load patterns on other relations. The query count fix eliminates the N+1 overhead; remaining latency comes from a different source.
Reason: Step 1 (relation type check) is mandatory before any code change because select_related and prefetch_related are not interchangeable. The wait state after Step 1 and the wait state after Step 3 prevent two different failure modes: writing the wrong queryset, and writing a regression test that asserts incorrect behavior. The modifier-before-slice rule in Step 2 addresses the specific silent failure mode documented in Django's ORM — Django does not raise an error when select_related appears after a slice; it simply ignores it. Note: the 0.5-second render time target in the review point is sourced from CB2-04-001 (domain processor pair for the select_related sub-task), not from the A input of this pair — the A input states 4.2 seconds as the current render time but does not specify a target. CB2-04-001's success state defines "under 0.5 seconds" as the expected render time after the fix.
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT

Pair CB3-03-001 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: [D-database-query-efficiency] N+1 pattern active. 1 query loads 100 Post objects; 100 queries each load 1 auth_user row by primary key. 101 round trips total. Root cause: per-object lazy load dispatch. Target: 1 JOIN query — SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. Template attribute access must not trigger additional queries after the fix.

[E-orm-usage-pattern] post.author.name accessed in template loop triggers per-row lazy load. Domain label present; formal Node 2 translation not yet available.

[B-backend-response-latency] 4.2-second render time is the symptom. Domain label present; formal Node 2 translation not yet available.
B:
Step 1 — Confirm relation type [prerequisite]
Transformational prompt: Check models.py in the blog application. Locate the author field on the Post model. Passing form: models.ForeignKey(<UserModel>, on_delete=<any value>) or models.OneToOneField(<UserModel>, on_delete=<any value>). Failing form: models.ManyToManyField(<UserModel>) or a reverse relation accessed via related_name — in the failing case, use prefetch_related('author') instead and do not proceed with Steps 2 through 4 as written.
Wait state: Field type confirmed before any code change.

Step 2 — Apply select_related [depends on Step 1]
Transformational prompt: Change Post.objects.all()[:100] to Post.objects.select_related('author').all()[:100]. The modifier must precede the slice — placing it after the slice causes Django to ignore it silently, leaving the N+1 pattern in place. With the modifier in the correct position, Django issues one JOIN query and caches auth_user data on each Post instance. The template loop reads post.author.name from the cache without triggering database calls.

Step 3 — Verify query count [depends on Step 2]
Transformational prompt: Open the feed page with Django Debug Toolbar. SQL panel must show 1 query. If above 1, the modifier is misplaced or missing. Do not write the regression test until this confirms 1.
Wait state: Query count confirmed at 1 before Step 4.

Step 4 — Add regression test [depends on Step 3]
Transformational prompt: Add to the feed view test suite: with self.assertNumQueries(1): response = self.client.get('/feed/'). Target is exactly 1. This test fails if any future change reintroduces lazy loading on this view.

Recovery path — assertNumQueries and Toolbar counts differ: If the Toolbar shows 1 query but assertNumQueries returns a different count, the test fixture is adding setup queries. Isolate the view call inside the context manager — exclude fixture setup. If count is consistently 2 or 3, inspect which query is additional.

Recovery path — both pass but production query count remains at 101: If Step 3 and Step 4 both report 1 query but production still shows 101, the select_related was applied to a queryset not used by the production view. Confirm the tested view function is the same function serving the production feed URL. Deploy to staging and repeat the Toolbar measurement on the production-path URL.

Review point: Measure render time after Step 4. Target: under 0.5 seconds (sourced from CB2-04-001 success state, not from the A input of this pair). Remaining latency above 1 second after query count is confirmed at 1 indicates a separate source — profile the template for other relation traversals.
