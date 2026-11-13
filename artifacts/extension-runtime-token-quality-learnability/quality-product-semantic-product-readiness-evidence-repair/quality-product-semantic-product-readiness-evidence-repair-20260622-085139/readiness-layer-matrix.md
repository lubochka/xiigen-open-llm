# Readiness layer matrix

Этот файл разделяет слои готовности. Он не открывает общий product-ready claim.

Product ready: false
Token savings claim allowed: false
Semantic quality claimed: false

| Слой | Статус | Что доказано | Что не доказано |
|---|---|---|---|
| core_api_runtime_readiness | PASS_NARROW_CORE_API_RUNTIME_ROWS | Direct core API runtime quality only. | Live host chat UI, broad semantic correctness, token savings, product readiness. |
| host_adapter_readiness | PASS_NARROW_HOST_ADAPTER_COMMAND_AND_TARGET_BOUND_EVIDENCE | Host adapter or extension command binding, scoped per host. | Full host UI/chat readiness. |
| full_host_runtime_readiness | NEEDS_REPAIR | None for full host readiness. | Full Claude/Cursor/Codex extension readiness. |
| semantic_quality_readiness | PARTIAL_NARROW_ONLY | Narrow staged user-adaptive evidence for PromptToTask and SkillSelection only. | Broad semantic quality; OrchestrationReasoning semantic runtime correctness. |
| learnability_model_readiness | PARTIAL_NARROW_STAGED_USER_ADAPTIVE_ONLY | Narrow staged user-adaptive learnability for named machines only. | Common model readiness, OrchestrationReasoning trained readiness, production promotion. |
| token_savings_readiness | NEEDS_REPAIR | No token savings claim. | Token reduction with quality gates on same pairs. |
| product_readiness | NEEDS_REPAIR_PRODUCT_READY_FALSE | product_ready=false. | product_ready=true. |
