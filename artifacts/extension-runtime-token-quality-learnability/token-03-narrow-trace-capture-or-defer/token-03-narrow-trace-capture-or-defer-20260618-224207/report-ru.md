# TOKEN-03-NARROW-TRACE-CAPTURE-OR-DEFER-01

## Всего
Нужно доказать token before/after только валидными same-host/same-prompt/same-method/same-evidence-class baseline/adaptive парами и затем отдельной quality non-regression проверкой для того же pair_id. Narrow/core-boundary traces не доказывают live host token savings.

## Сделано
- Создан token-03-narrow-trace-capture-or-defer-20260618-224207.
- Записаны 2 scoped trace rows из уже существующих no-eval artifacts:
  - Claude: narrow direct hook/core-boundary, prompt-to-task, adaptive-like/target-locator trace, evidence_class=narrow_live_host_surface.
  - Codex: installed skill/plugin-cache adapter -> llm_mvp_core core boundary, prompt-to-task, baseline/common trace, evidence_class=installed_skill_surface_plus_direct_installed_plugin_cache_adapter_to_llm_mvp_core_core_boundary.
- Cursor помечен deferred/blocked по existing safe-launch decision: safe no-backend/no-model-metadata launch contract missing, Cursor не relaunch.
- Для orchestration-plan и skill-selection по Claude/Codex нет same-class trace pairs в текущих no-eval proof packets, поэтому они помечены deferral rows.

## Сейчас
Текущий packet является scoped accounting/capture evidence only. Он не содержит complete baseline/adaptive pair, не запускает eval/quality scoring/training/DPO/checkpoint mutation, не вызывает external AI/model и не заявляет token savings.

## Осталось
Следующее действие: TOKEN-03-PAIR-COMPLETION-CAPTURE-OR-DEFER-02. Нужно либо безопасно получить недостающие same-class counterparts для Claude/Codex, либо честно defer; Cursor остаётся blocked до safe launch contract. Quality non-regression prep рано запускать, пока нет хотя бы одной валидной baseline/adaptive same-pair trace set.

## Claim Locks
Все readiness/savings/quality/learnability/product claims остаются false.
