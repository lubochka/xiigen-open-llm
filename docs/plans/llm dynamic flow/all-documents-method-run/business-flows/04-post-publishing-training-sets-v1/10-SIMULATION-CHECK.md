# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\04-post-publishing.md`

The simulation asks whether the nine node sets can recreate the core flow described by the source document without using the document as a runtime prompt.

## Test Prompt 1 - Full Post Publishing

Runtime user prompt:

"A business owner publishes a post with text and media. Analyze the content, find relevant business audiences, friends, followers, and group members, rank everyone, put the post into their feeds with diversity controls, and show distribution metrics."

Expected node behavior:

- Node-01 returns: post creation and visibility, content analysis, business audience matching, social-connection matching, group membership matching, recipient list compilation, composite ranking, feed tier placement, feed diversity and reordering, distribution analytics.
- Node-02 writes one precise domain statement for each label.
- Node-03 plans storage first, then content analysis, three audience paths, ranking join point, feed placement, reordering, analytics, and review.
- Node-04 processes each planned piece with success, fallback, and wait-state outcomes.
- Node-05 stores the post before starting downstream distribution; then applies analysis, audience matching, ranking, feed updates, reordering, and analytics.
- Node-06 translates each result into readable language.
- Node-07 selects privacy, safety, ranking, join-point, diversity, and analytics arbiters.
- Node-08 passes results that follow the rules and sends back any private, unsafe, incomplete, or wrongly ranked fragment.
- Node-09 returns one answer explaining publication, analysis, audience selection, ranking, feed placement, diversity, and metrics.

Pass condition:

The simulated flow must contain the three parallel audience branches and the ranking join point. If it becomes one simple matching step, the scheme-planner set is missing the most important edge.

## Test Prompt 2 - Connections-Only Post

Runtime user prompt:

"Publish this post to connections only. Do not show it to unrelated businesses."

Expected node behavior:

- Node-01 returns post creation and visibility, social-connection matching, optional group membership matching, privacy enforcement, feed placement.
- Node-02 treats visibility as a rule that must travel with the post.
- Node-03 skips public business matching and preserves the restricted audience.
- Node-05 places the post only for allowed recipients.
- Node-07 selects privacy and recipient-eligibility review.
- Node-08 rejects any public business-recipient result.
- Node-09 explains that the post was distributed inside the allowed audience only.

Pass condition:

No final answer may claim broad business matching for this prompt.

## Test Prompt 3 - Image-Only Post

Runtime user prompt:

"The post has only an image and no text. If you cannot analyze the image, still show it to friends and groups."

Expected node behavior:

- Node-01 returns content analysis, social-connection matching, group membership matching, fallback handling, feed placement.
- Node-02 marks full text analysis unavailable.
- Node-03 attempts image recognition, then falls back to social and group distribution if no meaning is found.
- Node-04 does not invent business meaning.
- Node-08 rejects any result that creates business matches without usable content meaning.
- Node-09 explains the reduced distribution path.

Pass condition:

The result must not pretend that full NLP analysis succeeded.

## Test Prompt 4 - High-Follower Author

Runtime user prompt:

"A user with many followers posts something that may need to reach fifty thousand people. Distribute it without overloading feeds."

Expected node behavior:

- Node-01 returns high-volume distribution, batching, feed distribution, ranking, analytics.
- Node-03 plans progressive batches and close connections first.
- Node-05 applies controlled feed updates rather than one uncontrolled write burst.
- Node-07 selects batching, throughput, retry-safety, and partial-completion reviews.
- Node-09 explains controlled staged distribution.

Pass condition:

The plan must show batching and progress tracking.

## Test Prompt 5 - Edited or Deleted Post

Runtime user prompt:

"If the author edits the post after it has already been distributed, update the feed item. If the author deletes it, remove it from feeds."

Expected node behavior:

- Node-01 returns edit handling, delete handling, feed update, feed removal, analytics.
- Node-03 separates edit and delete paths.
- Node-05 updates existing feed entries for edits and removes entries for deletions.
- Node-08 rejects duplicate reinjection for edits and missing removal for deletions.
- Node-09 explains the two different post-change behaviors.

Pass condition:

The simulation must not treat edit and delete as the same action.

## Missing-Training Detector

If the simulated output lacks content moderation, add more Node-02 and Node-08 pairs for safety.

If the simulated output lacks the business, social, and group branch split, add more Node-03 pairs for parallel matching.

If the simulated output starts ranking before all audience paths complete or fallback, add more Node-03 and Node-08 join-point pairs.

If the simulated output ranks recipients without all six score factors, add more Node-05 and Node-08 scoring pairs.

If the simulated output describes feed placement but ignores top-ten author and topic diversity, add more Node-04, Node-05, and Node-08 diversity pairs.

If the simulated output says distribution completed without metrics, add more Node-06, Node-07, and Node-08 analytics-completion pairs.

