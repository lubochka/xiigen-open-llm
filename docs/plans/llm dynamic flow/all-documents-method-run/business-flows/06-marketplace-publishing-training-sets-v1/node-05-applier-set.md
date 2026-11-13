# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete action and outcome.

## Pair 5.1 - Store item before publishing

A: Work piece: marketplace item creation. Transformation instruction: store valid item state before publishing listing or promotion. Processed result: verified seller and valid item details are ready.

B: Action outcome: the item record is stored first with seller, details, pricing, inventory, media, and target audience. Only after that record exists can the listing be published and downstream profiling, post generation, and cooperator matching begin.

Pair 5.1 - Phrasing B

A: Work piece: create commercial item. Transformation instruction: no listing without valid item state. Processed result: item is valid.

B: Action outcome: marketplace distribution starts from a stored, valid item rather than from an incomplete listing request.

## Pair 5.2 - Publish listing

A: Work piece: listing lifecycle. Transformation instruction: activate the listing only when seller and item checks pass. Processed result: listing state is complete.

B: Action outcome: the listing becomes active with status, visibility, listing address, and availability. If it is pre-order, service, sold out, expired, or deactivated, the visible state says so.

Pair 5.2 - Phrasing B

A: Work piece: make item visible. Transformation instruction: preserve listing status. Processed result: listing is ready.

B: Action outcome: marketplace users and feed cards receive a listing whose status is explicit and current.

## Pair 5.3 - Apply parallel profiling, content, and cooperator branches

A: Work piece: start audience profiling, marketplace-post generation, and cooperator scoring. Transformation instruction: keep branches separate until distribution needs their outputs. Processed result: item and listing exist.

B: Action outcome: audience profiling, promotional post creation, and cooperator matching are launched as separate branch results. The flow keeps profile data, post content, and partnership matches distinct so feed distribution can combine them correctly.

Pair 5.3 - Phrasing B

A: Work piece: run marketplace discovery paths. Transformation instruction: preserve branch identity. Processed result: active listing is available.

B: Action outcome: target audience, feed-ready content, and potential cooperators are produced as independent outputs rather than one collapsed result.

## Pair 5.4 - Apply synergy score

A: Work piece: score cooperators. Transformation instruction: use the defined weights and exclude competitors from partnership format. Processed result: candidate businesses have score factors.

B: Action outcome: each cooperator receives a synergy score from audience overlap, product complementarity, market presence, reputation, and collaboration history. Qualified scores create cross-promotion, bundle, referral, or distribution-channel opportunities. Competitors receive no partnership opportunity.

Pair 5.4 - Phrasing B

A: Work piece: identify partner opportunities. Transformation instruction: keep score reasons and cooperation type. Processed result: cooperator candidates are scored.

B: Action outcome: the flow creates partnership matches only for complementary businesses that pass the relevant threshold.

## Pair 5.5 - Apply multi-format distribution

A: Work piece: distribute marketplace post. Transformation instruction: use the correct card and offer for each audience. Processed result: friend, group, and cooperator recipients are ready.

B: Action outcome: friends receive product cards with friend discount, groups receive marketplace-section posts with group discount or volume badge, and cooperators receive partnership opportunity or potential-partner cards according to synergy score.

Pair 5.5 - Phrasing B

A: Work piece: place listing into feeds. Transformation instruction: do not show one generic card to every audience. Processed result: audience-specific card plan exists.

B: Action outcome: the feed distribution reflects why each audience is seeing the listing and what offer or partnership context applies.

## Pair 5.6 - Apply limited-time and pre-order behavior

A: Work piece: special listing behavior. Transformation instruction: honor expiry and availability state. Processed result: listing is limited-time or pre-order.

B: Action outcome: limited-time offers show countdowns, time-sensitive notifications, and removal after expiry. Pre-order listings show coming-soon status, continue cooperator matching, and convert to regular availability when stock arrives.

Pair 5.6 - Phrasing B

A: Work piece: handle temporary or coming-soon listing. Transformation instruction: keep promotion aligned with availability. Processed result: special state is known.

B: Action outcome: the listing is promoted honestly according to time and stock state, not as a normal in-stock item.

## Pair 5.7 - Apply inventory and seller deactivation cleanup

A: Work piece: stock or seller state changes after promotion. Transformation instruction: update feeds and obligations immediately. Processed result: inventory depleted or seller deactivated.

B: Action outcome: if inventory reaches zero, active promotion stops, existing feed entries show sold-out, and cooperator restock discussions can remain. If the seller deactivates, active listings are canceled, feed entries removed, pending cooperators notified, and pre-orders refunded.

Pair 5.7 - Phrasing B

A: Work piece: marketplace state changed after listing was distributed. Transformation instruction: keep feeds and buyers accurate. Processed result: sold-out or inactive-seller condition exists.

B: Action outcome: visible marketplace content is corrected so users do not act on invalid availability or inactive sellers.

## Pair 5.8 - Apply marketplace analytics

A: Work piece: record marketplace performance and seller intelligence. Transformation instruction: measure distribution, partnerships, revenue, discounts, and suggestions. Processed result: distribution and notification outcomes are ready.

B: Action outcome: analytics records listing activity, inquiry conversion, cooperator acceptance, revenue, discount redemption, bundle partnerships, and seller improvement suggestions for pricing and audience targeting.

Pair 5.8 - Phrasing B

A: Work piece: complete marketplace measurement. Transformation instruction: turn outcomes into seller insight. Processed result: performance data exists.

B: Action outcome: the marketplace result becomes measurable and can guide future seller decisions.

