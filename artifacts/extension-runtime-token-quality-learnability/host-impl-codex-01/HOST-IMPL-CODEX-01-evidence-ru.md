# HOST-IMPL-CODEX-01 evidence

## Что проверено
- Установленный cache-пакет Codex plugin `xiigen-local/codex-xiigen-smart-skills` был точечно обновлен из `C:/Projects/codex-xiigen-smart-skills`.
- Через установленный cache entrypoint выполнен non-AI smoke для метода `skill-selection` с `TargetLocator`, `RequestedSkills` и `AvailableSkills`.
- Положительный smoke дошел до `llm_mvp_core` через `tools/xiigen-extension-core-api.ps1` и вызвал `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Отрицательный smoke без `TargetLocator` проверил узкий контракт `target-locator-required`: core вернул `target_locator_required`.
- Дополнительно проверены Codex CLI help/debug probes и component/source QA. Это handler/component-level доказательство, а не полный запуск Codex chat UI.

## Пример input/output/debug shape
Положительный вход:

```json
{
  "method": "skill-selection",
  "TargetLocator": {
    "mode": "target",
    "userAdaptiveAssetRoot": "C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\qsg28-semantic-host-matrix\\staged-two-unit-target-root"
  },
  "RequestedSkills": ["implementation-integrity"],
  "AvailableSkills": ["implementation-integrity", "test-integrity"]
}
```

Положительный output/debug shape:

```text
xiigen skill-selection :
input:
  TargetLocator: bound
  RequestedSkills: forwarded
  AvailableSkills: forwarded
output:
  Success: true
  core_boundary_executed: true
  target_locator_bound: true
  target_locator_mode: target
```

Отрицательный output/debug shape:

```text
xiigen prompt-to-task :
input:
  TargetLocator: absent
output:
  Success: false
  error: target_locator_required
```

## Что доказано
- Installed `xiigen-local` cache содержит обновленный TargetLocator/debug/core-boundary путь.
- Installed-cache Node entrypoint создает `XiigenExtensionRequest` с `TargetLocator` и передает `RequestedSkills`/`AvailableSkills` в `llm_mvp_core`.
- Core boundary реально вызван через `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Положительный smoke вернул `Success=true`, `core_boundary_executed=true`, `target_locator_bound=true`, `target_locator_mode=target`.
- Debug display имеет форму `xiigen <method> :`, `input:`, `output:` и redaction flags без сырого приватного содержимого.
- Отрицательный smoke без `TargetLocator` вернул `target_locator_required` через тот же core boundary.
- Codex help/debug probes были non-AI; `send-message-v2` не вызывался.
- В trace зафиксировано `external_ai_used=false` и `training_started=false`.

## Что НЕ доказано
- Не доказан полный Codex chat UI/plugin app runtime.
- Не доказан Codex app-server `send-message-v2` conversation path.
- Не заявлены token savings, broad semantic quality, learnability, production promotion, trained-model readiness или final cross-host readiness.
- Не выполнялась и не заявлялась мутация production common model, checkpoint или locator.

## Почему сбой `codex plugin list --json` остается узким разрывом
- `codex plugin list --json` завершился с exit 1 из-за настроенного marketplace snapshot, который не загрузился как поддерживаемый manifest.
- Этот сбой ограничивает только доказательство marketplace/list path и полного Codex plugin discovery через CLI.
- Доказательство текущего slice опирается на установленный cache path, прямой Node entrypoint smoke и core-boundary trace.
- Поэтому failure не является Codex blocker для узкого HOST-IMPL-CODEX-01 installed-cache доказательства, но остается открытым разрывом для будущего полного Codex chat/app proof.

## Артефакты
- Cache refresh: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-cache\20260617-234852\cache-refresh-report.json`
- Runtime smoke root: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\installed-runtime-smoke\20260617-235017`
- Component QA: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-CODEX-01\component-qa\20260617-235258\HOST-IMPL-CODEX-01-codex-component-qa.json`
- Core JSON evidence: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\host-impl-codex-01\host-impl-codex-01-evidence.json`
- JSONL ledger: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\host-impl-codex-01\host-impl-codex-01-evidence-ledger.jsonl`

## Следующий шаг
Parent architect/reviewer может review-ить HOST-IMPL-CODEX-01 artifacts и выполнять отдельный commit/push boundary, если артефакты приняты. Если нужен полный Codex chat/app proof, marketplace/app-server invocation нужно чинить отдельной bounded phase до любых широких runtime/quality claims.
