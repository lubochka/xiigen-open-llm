# Codex Path A API/boundary scenario coverage

Verdict: `PATH_A_ACCEPTED`.

Это evidence-пакет только для `Path A`: тот же сценарий был проверен через Codex installed plugin/adapter boundary path и дошел до `llm_mvp_core`. Это не проверка реального Codex UI.

## Что проверяли

Scenario id: `codex-full-extension-runtime-20260627-090817`.

Смысл проверки: взять тот же prompt из Codex scenario prep и проверить, что Codex adapter формирует request, передает его в core, получает response и не добавляет собственный selector/scorer поверх core logic.

## Что доказано

- `scenario coverage`: проверена только API/boundary часть сценария.
- `Path A`: accepted.
- `same prompt`: true.
- `RequestedSkills`: `xiigen-smart-skills`.
- `AvailableSkills`: `xiigen-smart-skills`.
- Selected skill: `xiigen-smart-skills`.
- Host selector/scorer added: false.
- Core response: success.

## Что осталось

- `Path B local-agent UI path remains not_started`.
- Реальный Codex UI не запускался.
- Prompt не отправлялся через UI.
- Ответ в Codex UI не проверялся.
- Token savings, semantic quality, learnability/training, checkpoint promotion и locator update не заявлялись.

## Почему есть несколько запусков

В папке сохранены все попытки, потому что они важны для честного evidence:

1. CLI invocation дошел до core, но `RequestedSkills` и `AvailableSkills` стали пустыми из-за quote-loss при передаче JSON через Windows PowerShell native command.
2. Direct module invocation сохранил skills, но передал boolean `HostFacts.samePrompt`; core contract требует string values и отклонил request.
3. Final direct module invocation передал HostFacts как strings и сохранил skills. Именно этот запуск является accepted Path A evidence.

## Главные файлы

- `request-final.json`
- `response-final.json`
- `trace-final.json`
- `host-boundary-metadata.json`
- `result.json`
- `verification.json`

## Граница claim

Этот пакет закрывает только `Path A` для Codex scenario coverage. Для полного сценария нужен отдельный `Path B` пакет: local-agent UI path в реальном Codex UI с тем же prompt и видимым результатом.
