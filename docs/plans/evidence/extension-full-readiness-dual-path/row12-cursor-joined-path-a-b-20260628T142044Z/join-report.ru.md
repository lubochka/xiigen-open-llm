# Row12 Cursor Path A + Path B: объединенный отчет

Статус: PASS.

Join accepted: да.

## Область принятого утверждения

Принято только узкое утверждение: для row12 Cursor есть dual-path same-scenario coverage по новой формулировке готовности.

Только row12 Cursor dual-path same-scenario coverage считается принятой в этом отчете. Это не расширяется на другие строки, все extensions или весь продукт.

Примененная формулировка готовности:

- Dual-path same-scenario coverage is product readiness evidence.
- same scenario Path A plus Path B.
- full live host UI path is not the readiness definition unless explicitly re-approved by Luba.

## Проверенные источники

Path A:

`docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/`

Path B:

`docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-path-b-real-ui-20260628T140709Z/`

## Сопоставление одного сценария

Обе стороны относятся к одному row12-сценарию:

- scenario id: `frdp-p01-cursor-token-savings-baseline`
- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- pair id: `frdp-p01-row12-cursor-token-baseline-v1`
- prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`

Сопоставление принято. Path A runner/redacted trace и Path B UI report несут совпадающие row12-идентификаторы. В latest/host traces Path A используется более короткая runtime-метка сценария `positive_request`.

## Что подтверждено

- Path A является автоматизированным путем через host command/API/core trace.
- В Path A успешно разобраны 8 JSON-файлов.
- Path A runner report присутствует и имеет `passed=true`.
- В Path A присутствуют обязательные ссылки на trace, latest response и latest pointer.
- Latest pointer указывает на существующие `coreRequestRef`, `coreResponseRef`, `coreTraceRef`, `hostTraceRef`, `entrypointResponseRef` и `redactedTraceRef`.
- Path A не заявляет Path B, token savings, semantic quality, readiness, training, checkpoint mutation, private content capture или external AI use.
- Path B является реальным маршрутом через Cursor UI со скриншотами.
- В Path B успешно разобраны 2 JSON-файла.
- В Path B присутствуют `human-status.md`, `path-b-ui-report.json` и `window-inventory.json`.
- Path B UI report имеет `pass=true`.
- Path B не отправлял запрос в core: `submitted_to_core=false`.
- В Path B есть 4 скриншота, все файлы существуют и имеют ненулевой размер:
  - `01-target-extension-development-host-top.png`
  - `02-command-palette-xiigen-select-smart-skill-top.png`
  - `03-row12-marker-in-xiigen-input-top.png`
  - `04-after-cancel-no-submit-top.png`
- Path B worker не заявляет product readiness, token savings, semantic quality, training, checkpoint promotion, locator update или full live chat path.
- Privacy-marker scan по именам файлов и содержимому двух исходных папок дал 0 совпадений и прошел проверку.

## Что это доказывает

Это доказывает только row12 Cursor dual-path same-scenario coverage.

Path A доказывает наличие automated/host command/API/core trace evidence для Cursor extension host boundary.

Path B доказывает наличие real Cursor UI route evidence для того же row12 marker, со скриншотами и без отправки в core.

## Что не доказано

Этот отчет не доказывает:

- token savings;
- semantic quality;
- training;
- checkpoint promotion;
- locator update;
- готовность всех extensions;
- готовность всего продукта;
- full live host UI chat submission path.

## Блокеры

Блокеров для узкого принятого утверждения не найдено.

## Итог

Join принят только для row12 Cursor dual-path same-scenario coverage. Все более широкие утверждения остаются не доказанными.
