# Cursor Path A API/core boundary

Дата пакета: 2026-06-27 23:59:13 +03:00.

Сценарий: `cursor-full-extension-runtime-after-unlock-20260627-090159`.

## Счетчик

Всего Cursor-сценариев в этом пакете: 1.
Path A: `accepted`.
Path B сейчас: `blocked_current_storage_modal` из уже сохраненного blocker evidence.
Осталось: вручную или отдельным bounded worker решить блокер Cursor `Storage Error`, затем повторить Path B same-scenario preflight и UI prompt-run.

## Что было запущено

Запущен не UI и не полный Cursor Extension Host. Запущен прямой fallback boundary: `direct XiigenExtensionRequest API/core boundary fallback`.

Использованный core/API путь: `tools/xiigen-extension-core-api.ps1`.

В request указано:

- `HostName = Cursor`
- `HostType = cursor-api-boundary`
- `HostFacts.host = cursor`
- `RequestedSkills` переданы
- `AvailableSkills` переданы
- `hostSelectorOrScorerAdded = false`

Exact prompt взят из prep `scenario.json`; prompt не переписывался вручную.

## Результат

Path A accepted: `true`.
Core exit code: `0`.
Core response status: `core_boundary_partial_no_semantic_quality_claim`.
Structured response returned: `true`.
Trace prompt hash matches prep prompt: `true`.

Этот пакет доказывает только no-UI API/core boundary для exact Cursor scenario prompt. Он не доказывает живой Cursor UI, token savings, semantic quality improvement, training, DPO, checkpoint promotion или широкую готовность всех сценариев.

## Path B blocker

Path B остается `blocked_current_storage_modal`. В blocker-пакете `cursor-current-state-preflight-20260627-234230` уже зафиксировано, что Cursor показывает `Storage Error`, prompt не вводился, submit не выполнялся, visible response и UI core trace не получены.

## Следующее действие

Точное оставшееся действие: принять отдельное решение по безопасной обработке Cursor `Storage Error`, затем повторить same-scenario Path B preflight и UI prompt-run с тем же exact prompt.

## Parent recommendation

Рекомендация: parent может принять эту fix-loop правку после проверки и затем отдельно решить, commit/push ли всю новую evidence-папку. Это полезное same-scenario Path A evidence, но не claim на широкую готовность всех сценариев.
