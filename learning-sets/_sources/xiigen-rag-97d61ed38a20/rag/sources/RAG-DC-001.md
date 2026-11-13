# RAG-DC-001 — Angular Route Map Audit

Источник: [.net angular translation/ANGULAR-ROUTE-MAP-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/ANGULAR-ROUTE-MAP-AUDIT.md).

Ветка main; commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c; git blob f9333d8decbd717eec6f2c873b32235da5bec0da.

Полностью прочитано L1–L74. Здесь сохранены целевые выдержки L1–L29 и L71–L73.

Карта сопоставляет React App.tsx routes с component imports при подготовке Angular-перехода; в FLOW-06 указан параметризованный путь /groups/:groupId/feed.

Это исторический audit/план миграции, не работающая Angular-реализация и не runtime-тест маршрутизатора. Строки 43 confirmed и 47 flows — утверждения этого источника, не результат нашей проверки. В новых карточках route/компоненты являются авторской симуляцией с закреплённой версией Angular.

## Фрагмент L1–L29

~~~~text
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
~~~~

## Фрагмент L71–L73

~~~~text
## Gate Result

The systemic 22-flow warning from the earlier comment-based inventory is closed by this import-backed audit. All flows with client page directories now have at least one confirmed route. FLOW-41 through FLOW-44 have no React route by design because they are native adapter boundary flows.
~~~~

