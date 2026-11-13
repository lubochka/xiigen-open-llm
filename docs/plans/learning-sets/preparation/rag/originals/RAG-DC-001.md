# Angular Route Map Audit

Updated: 2026-05-14

This audit resolves the earlier systemic route-map concern by mapping `client/src/App.tsx` routes through actual component imports instead of relying on nearby FLOW comments.

Machine-readable source: `generated/angular-route-map-audit.json`.

## Summary

| Metric | Value |
| --- | ---: |
| Total flows | 47 |
| Flows with client page dirs | 43 |
| Flows with confirmed routes | 43 |
| Client-dir flows missing routes | 0 |
| Native adapter flows without routes by design | 4 |

## Route Map

| Flow | Slug | Routes | Route components | Status |
| --- | --- | --- | --- | --- |
| FLOW-00 | bundle-activation | `/admin/bundle-activation` | `BundleActivationPage` | confirmed |
| FLOW-01 | user-registration | `/register`<br>`/register/pending-verification`<br>`/verify`<br>`/verify/resend`<br>`/onboarding`<br>`/auth/sso/:provider` | `RegistrationPage`<br>`RegistrationPendingPage`<br>`VerifyTokenPage`<br>`ResendPage`<br>`OnboardingPage`<br>`SsoPage` | confirmed |
| FLOW-02 | profile-enrichment | `/questionnaire`<br>`/matching`<br>`/personalization` | `QuestionnairePage`<br>`MatchingPage`<br>`PersonalizationPage` | confirmed |
| FLOW-03 | event-management | `/events`<br>`/events/create` | `EventListPage`<br>`EventCreationPage` | confirmed |
| FLOW-04 | event-attendance | `/rsvp`<br>`/attendance` | `RsvpPage`<br>`AttendanceDashboardPage` | confirmed |
| FLOW-05 | completion-gamification | `/gamification`<br>`/learning/progress`<br>`/lessons/complete`<br>`/learning/social` | `GamificationDashboardPage`<br>`LearningProgressPage`<br>`LessonCompletionPage`<br>`SocialLearningPage` | confirmed |
| FLOW-06 | user-groups-communities | `/groups`<br>`/groups/:groupId/feed`<br>`/groups/:groupId/membership`<br>`/groups/:groupId/tier`<br>`/groups/:groupId/admin/approvals` | `GroupDiscoveryPage`<br>`GroupFeedPage`<br>`MembershipStatusPage`<br>`TierManagementPage`<br>`GroupApprovalPage` | confirmed |
| FLOW-07 | friend-request-social-feed | `/connections`<br>`/friend-requests`<br>`/social-feed`<br>`/social-graph` | `ConnectionsPage`<br>`FriendRequestPage`<br>`SocialFeedPage`<br>`SocialGraphPage` | confirmed |
| FLOW-08 | marketplace | `/marketplace/bootstrap-status`<br>`/marketplace/discovery`<br>`/marketplace/register/:eventId`<br>`/marketplace/participation`<br>`/marketplace/purchases` | `BootstrapStatusPage`<br>`EventDiscoveryPage`<br>`EventRegistrationPage`<br>`ParticipationStatusPage`<br>`PurchaseHistoryPage` | confirmed |
| FLOW-09 | transactional-event-participation | `/tickets/purchase`<br>`/tickets/waitlist`<br>`/tickets/booking`<br>`/tickets/qr`<br>`/tickets/refund` | `TicketPurchasePage`<br>`WaitlistPage`<br>`BookingConfirmationPage`<br>`QRCodePage`<br>`RefundPage` | confirmed |
| FLOW-10 | reviews-reputation | `/reviews/submit`<br>`/reviews/moderation`<br>`/reviews/reputation/:entityId`<br>`/reviews/respond/:reviewId` | `ReviewSubmissionPage`<br>`ReviewModerationPage`<br>`ReputationDashboardPage`<br>`ReviewResponsePage` | confirmed |
| FLOW-11 | schema-registry-dag | `/schema-registry`<br>`/schema-submission`<br>`/dag-visualization` | `SchemaRegistryPage`<br>`SchemaSubmissionPage`<br>`DagVisualizationPage` | confirmed |
| FLOW-12 | subscription-billing | `/subscription-plans`<br>`/subscribe`<br>`/billing-dashboard` | `SubscriptionPlanPage`<br>`SubscribePage`<br>`BillingDashboardPage` | confirmed |
| FLOW-13 | data-warehouse-analytics | `/admin/data-warehouse-analytics` | `DataWarehouseAnalyticsPage` | confirmed |
| FLOW-14 | etl-data-integration | `/admin/etl-data-integration` | `EtlDataIntegrationPage` | confirmed |
| FLOW-15 | saas-multi-tenancy | `/admin/tenants/lifecycle`<br>`/admin/tenants/provisioning` | `TenantLifecyclePage`<br>`TenantProvisioningPage` | confirmed |
| FLOW-16 | marketplace-payments | `/checkout`<br>`/escrow` | `CheckoutPage`<br>`EscrowDashboardPage` | confirmed |
| FLOW-17 | freelancer-marketplace | `/gigs/post`<br>`/gigs/milestones` | `GigPostingPage`<br>`MilestoneDashboardPage` | confirmed |
| FLOW-18 | visual-flow-engine | `/admin/visual-flow/canvas`<br>`/admin/visual-flow/publisher` | `FlowCanvasPage`<br>`FlowPublisherPage` | confirmed |
| FLOW-19 | durable-sagas-compliance | `/admin/sagas`<br>`/admin/compliance/audit` | `SagaDashboardPage`<br>`ComplianceAuditPage` | confirmed |
| FLOW-20 | ads-platform | `/admin/ads/auction`<br>`/admin/ads/consent`<br>`/admin/ads-platform` | `AuctionDashboardPage`<br>`ConsentGatePage`<br>`AdsPlatformPage` | confirmed |
| FLOW-21 | dynamic-forms-workflows | `/dynamic-forms-workflows` | `DynamicFormsWorkflowsPage` | confirmed |
| FLOW-22 | cms-publishing | `/cms-publishing` | `CmsPublishingPage` | confirmed |
| FLOW-23 | form-builder-templates | `/form-templates` | `TemplateBuilder` | confirmed |
| FLOW-24 | ai-safety-moderation | `/ai-safety-moderation` | `AiSafetyModerationPage` | confirmed |
| FLOW-25 | bfa-cross-flow-governance | `/admin/bfa-cross-flow-governance` | `BfaCrossFlowGovernancePage` | confirmed |
| FLOW-26 | meta-flow-engine | `/admin/meta-flow-engine` | `MetaFlowEnginePage` | confirmed |
| FLOW-27 | human-interaction-gate | `/admin/human-interaction-gate` | `HumanInteractionGatePage` | confirmed |
| FLOW-28 | blog-cms-modules | `/blog-cms-modules` | `BlogCmsModulesPage` | confirmed |
| FLOW-29 | adaptive-rag-deep-research | `/admin/adaptive-rag-deep-research` | `AdaptiveRagDeepResearchPage` | confirmed |
| FLOW-30 | tenant-lifecycle-manager | `/admin/tenant-lifecycle-manager` | `TenantLifecycleManagerPage` | confirmed |
| FLOW-31 | design-intelligence-engine | `/admin/design-intelligence-engine` | `DesignIntelligenceEnginePage` | confirmed |
| FLOW-32 | sharable-flows-marketplace | `/admin/sharable-flows-marketplace` | `SharableFlowsMarketplacePage` | confirmed |
| FLOW-33 | system-initiation-bootstrap | `/admin/system-initiation-bootstrap` | `SystemInitiationBootstrapPage` | confirmed |
| FLOW-34 | marketplace-plugin-adapter | `/admin/marketplace-plugin-adapter` | `MarketplacePluginAdapterPage` | confirmed |
| FLOW-35 | meta-arbitration-engine | `/admin/meta-arbitration-engine` | `MetaArbitrationEnginePage` | confirmed |
| FLOW-36 | feature-registry | `/admin/feature-registry` | `FeatureRegistryPage` | confirmed |
| FLOW-37 | design-system-governance | `/admin/design-system-governance` | `DesignSystemGovernancePage` | confirmed |
| FLOW-38 | rag-quality-feedback | `/admin/rag-quality-feedback` | `RagQualityFeedbackPage` | confirmed |
| FLOW-39 | oss-curriculum | `/admin/oss-curriculum` | `OssCurriculumPage` | confirmed |
| FLOW-40 | client-push | `/admin/client-push` | `ClientPushPage` | confirmed |
| FLOW-41 | canva-text-adapter | none | none | not-applicable-native-adapter |
| FLOW-42 | miro-shape-adapter | none | none | not-applicable-native-adapter |
| FLOW-43 | webflow-designer-extension | none | none | not-applicable-native-adapter |
| FLOW-44 | framer-adapter | none | none | not-applicable-native-adapter |
| FLOW-45 | history-bootstrap | `/history-bootstrap`<br>`/admin/history-bootstrap` | `HistoryBootstrapPage` | confirmed |
| FLOW-46 | platform-agent | `/admin/platform-agent` | `PlatformAgentPage` | confirmed |

## Gate Result

The systemic 22-flow warning from the earlier comment-based inventory is closed by this import-backed audit. All flows with client page directories now have at least one confirmed route. FLOW-41 through FLOW-44 have no React route by design because they are native adapter boundary flows.

