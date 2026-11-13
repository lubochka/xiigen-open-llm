# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility and the formal result or result group to examine.
Runtime B is pass, concern, or reprocess instruction.

## Pair 8.1 - Profile privacy

A: Review responsibility: profile-privacy arbiter. Formal result: private profile field appears in public profile output.

B: Reprocess. The source defines privacy settings per profile field. Send the profile-output fragment back and remove private fields from public display.

Pair 8.1 - Phrasing B

A: Review responsibility: account-deletion review. Formal result: deleted account still appears in suggestions.

B: Reprocess. Account deletion or deactivation must affect discovery. Send the suggestion fragment back.

## Pair 8.2 - Block enforcement

A: Review responsibility: block-enforcement arbiter. Formal result: blocked user's post appears in home feed.

B: Reprocess. Feed filtering must remove blocked relationships. Send the feed-filter fragment back.

Pair 8.2 - Phrasing B

A: Review responsibility: graph-state review. Formal result: blocked user can send normal message.

B: Reprocess. Block state must affect messaging trust and delivery. Send the messaging fragment back.

## Pair 8.3 - Media readiness

A: Review responsibility: media-readiness arbiter. Formal result: post with video publishes final display before transcoding completes.

B: Reprocess. Media readiness controls final publishable display. Send the content-media fragment back and wait or use placeholder behavior.

Pair 8.3 - Phrasing B

A: Review responsibility: audience-rule review. Formal result: friends-only post is feed eligible for public users.

B: Reprocess. Post audience rules must control feed eligibility. Send the post-audience fragment back.

## Pair 8.4 - Feed filters

A: Review responsibility: privacy-filter arbiter. Formal result: private group post appears in a non-member's feed.

B: Reprocess. Candidate filtering must enforce privacy and audience rules. Send the feed-candidate fragment back.

Pair 8.4 - Phrasing B

A: Review responsibility: ad-spacing review. Formal result: five ads appear consecutively in the feed.

B: Concern. Ads should merge with spacing constraints and brand safety controls. Require feed re-ranking to restore spacing.

## Pair 8.5 - Engagement idempotency

A: Review responsibility: idempotent-engagement arbiter. Formal result: tapping like twice creates two likes from the same user.

B: Reprocess. Reaction writes should be idempotent. Send the reaction fragment back and collapse duplicate reaction.

Pair 8.5 - Phrasing B

A: Review responsibility: mention-privacy review. Formal result: mention notification is sent to a user who cannot see the post.

B: Reprocess. Notifications must respect visibility. Send the notification fragment back.

## Pair 8.6 - Messaging trust gate

A: Review responsibility: trust-gating arbiter. Formal result: unconnected sender's message enters inbox as normal conversation.

B: Reprocess. The source requires message request gating for users not already connected. Send the messaging fragment back.

Pair 8.6 - Phrasing B

A: Review responsibility: attachment-safety review. Formal result: attachment bypasses media safety checks.

B: Reprocess. Attachments go through media and safety handling. Send the attachment fragment back.

## Pair 8.7 - Search permissions

A: Review responsibility: permissions-filter arbiter. Formal result: blocked user's private post appears in search.

B: Reprocess. Search must apply visibility and block filters. Send the search-result fragment back.

Pair 8.7 - Phrasing B

A: Review responsibility: social-proximity review. Formal result: people search ignores social proximity entirely.

B: Concern. The source says search ranking uses social proximity alongside text and recency. Require ranking features to include social distance.

## Pair 8.8 - Moderation

A: Review responsibility: moderation-action arbiter. Formal result: reported phishing link remains fully ranked while classifier marks it unsafe.

B: Reprocess. Link safety and harmful content classification must trigger hide, downrank, removal, or review. Send the moderation fragment back.

Pair 8.8 - Phrasing B

A: Review responsibility: audit-log review. Formal result: content is removed with no audit entry or appeal path.

B: Concern. Enforcement needs audit log and appeal flow. Require audit and appeal information before final formatting.

