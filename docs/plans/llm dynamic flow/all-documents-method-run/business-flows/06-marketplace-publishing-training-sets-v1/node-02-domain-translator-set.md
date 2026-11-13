# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label.
Runtime B is the same request in the language of that domain.

## Pair 2.1 - Marketplace item creation

A: Fragment: "Create a marketplace item with title, description, pricing, inventory, media, and target audience." Domain: marketplace item creation.

B: The marketplace item domain must capture seller identity, item title, description, media, target audience, pricing choices, inventory or availability, and seller verification before the listing can be published.

Pair 2.1 - Phrasing B

A: Fragment: "List a product or service with details, stock, price options, and audience settings." Domain: marketplace item creation.

B: The listing request becomes a structured item record that includes the commercial details, audience intent, and availability facts needed by marketplace, inventory, feed, and cooperator paths.

## Pair 2.2 - Listing lifecycle and visibility

A: Fragment: "Publish the listing and make it active in the marketplace." Domain: listing lifecycle and visibility.

B: The listing domain must create an active marketplace listing tied to the item, seller, status, visibility, and listing address, and it must preserve status changes such as active, pre-order, sold out, expired, or deactivated.

Pair 2.2 - Phrasing B

A: Fragment: "Move a marketplace item from created state to active visibility." Domain: listing lifecycle and visibility.

B: Publishing changes the item into a visible listing only when seller status, item validity, and required display information are complete.

## Pair 2.3 - Target-audience profiling

A: Fragment: "Profile buyer personas, industries, regions, and estimated audience size." Domain: target-audience profiling.

B: The audience profiling domain identifies buyer segments, industry fit, geographic reach, audience characteristics, and estimated audience size so matching and marketplace intelligence have structured guidance.

Pair 2.3 - Phrasing B

A: Fragment: "Understand which buyers, industries, and locations the listing should appeal to." Domain: target-audience profiling.

B: The item receives an audience profile that explains who is likely to care about it and why.

## Pair 2.4 - Marketplace post generation

A: Fragment: "Create a social marketplace post with headline, highlights, media, call to action, and special offer." Domain: automated marketplace-post generation.

B: The marketplace-post domain turns the listing into feed-ready promotional content that includes a headline, key highlights, media, call to action, special offer, and the correct card style for the destination audience.

Pair 2.4 - Phrasing B

A: Fragment: "Turn the listing into feed-ready promotional content." Domain: automated marketplace-post generation.

B: The listing is rewritten as a social marketplace post that can be distributed to feeds without losing price, offer, media, or seller context.

## Pair 2.5 - Cooperator synergy scoring

A: Fragment: "Find complementary businesses using synergy score." Domain: cooperator synergy scoring.

B: The cooperator domain scores potential partners using audience overlap at thirty percent, product complementarity at twenty-five percent, market presence at twenty percent, reputation at fifteen percent, and collaboration history at ten percent, then assigns a cooperation type when thresholds are met.

Pair 2.5 - Phrasing B

A: Fragment: "Identify business partners whose products complement this listing." Domain: cooperator synergy scoring.

B: Partner matching must separate complementary businesses from competitors, score synergy, and label the opportunity as cross-promotion, bundle partner, referral partner, or distribution channel when qualified.

## Pair 2.6 - Friend purchase-affinity audience

A: Fragment: "Show the listing to friends with purchase affinity and a friend discount." Domain: friend purchase-affinity audience.

B: The friend audience domain finds friends likely to care about the product, includes purchase affinity and previous purchase signals, and prepares product-card distribution with the seller's friend discount.

Pair 2.6 - Phrasing B

A: Fragment: "Promote the marketplace post to the seller's network with a special friend offer." Domain: friend purchase-affinity audience.

B: Friend distribution uses relationship context and buying likelihood to show the marketplace listing with a network-specific discount.

## Pair 2.7 - Group marketplace distribution

A: Fragment: "Auto-post the listing to marketplace-enabled groups with a group member discount." Domain: group marketplace distribution.

B: The group distribution domain selects relevant groups where marketplace posting is enabled, places the listing in the group marketplace area, and applies the group member discount.

Pair 2.7 - Phrasing B

A: Fragment: "Place the listing in relevant group marketplace sections and show the member discount." Domain: group marketplace distribution.

B: Group recipients are reached through group marketplace sections only when the group allows this kind of listing and the listing is relevant to that group.

## Pair 2.8 - Special listing types

A: Fragment: "Support limited-time offers, pre-order items, service listings, and wholesale pricing." Domain: limited-time offers, pre-orders, services, and wholesale pricing.

B: The listing-type domain handles expiry timestamps and countdowns for limited offers, restock dates and coming-soon display for pre-orders, service cards without stock tracking, and quantity-based wholesale pricing.

Pair 2.8 - Phrasing B

A: Fragment: "A listing may expire, be coming soon, be a service, or use bulk prices." Domain: limited-time offers, pre-orders, services, and wholesale pricing.

B: Marketplace behavior must change according to listing type: expiring offers are removed after expiry, pre-orders wait for stock, services use booking calls to action, and wholesale listings show volume pricing.

## Pair 2.9 - Marketplace safety

A: Fragment: "Block fake listings and duplicates, prevent price manipulation, validate stock, and limit cooperator spam." Domain: marketplace safety.

B: The safety domain requires seller verification, content moderation, auditable price-change history, real-time stock validation before purchase, duplicate-listing similarity checks, cooperator request rate limits, marketplace search throttling, and transaction compliance rules.

Pair 2.9 - Phrasing B

A: Fragment: "Protect the marketplace from fraudulent listings, misleading prices, stock fraud, duplicates, and partnership spam." Domain: marketplace safety.

B: Marketplace protection must prevent fraudulent commercial behavior before listings, purchases, or partnership notifications reach users.

## Pair 2.10 - Marketplace analytics

A: Fragment: "Measure listings, inquiry conversion, cooperator acceptance, revenue, discounts, and bundle partnerships." Domain: marketplace analytics and seller suggestions.

B: The analytics domain records listing volume, active listings, inquiries, cooperator match acceptance, marketplace revenue, discount redemption, bundle partnership creation, and seller suggestions for pricing and audience targeting.

Pair 2.10 - Phrasing B

A: Fragment: "Report marketplace performance and suggest better pricing or audience targets." Domain: marketplace analytics and seller suggestions.

B: Marketplace measurement should help the seller understand visibility, conversion, partnership success, revenue, and how to improve the listing.

