# Доска статуса после join Cursor row12

Статус этой доски: узкий статусный пакет после объединения Path A и Path B для Cursor row12. Это не кодовое изменение, не тест продукта, не обучение модели и не выпускная готовность.

## Граница утверждения

Доказано только узкое утверждение: для Cursor row12 есть same scenario Path A plus Path B coverage.

Новая формулировка готовности, примененная здесь:

- `Dual-path same-scenario coverage is product readiness evidence`.
- `same scenario Path A plus Path B`.
- `full live host UI path is not the readiness definition unless explicitly re-approved by Luba`.

Это не расширяется на все extensions, все сценарии, token savings, semantic quality, training, checkpoint promotion, locator update или full product readiness.

## Счетчик крупных групп

Счетчики ниже выведены из предыдущей доски `status-board-20260628-151215` как таксономии крупных групп и из row12 Cursor join evidence как нового узкого факта. Я не читал полный execution state и не заявляю полный пересчет всего плана.

- Всего крупных групп: 8.
- Узко закрыто: 1.
- В работе: 0.
- Осталось: 7.

Текущая фаза: зафиксировать, что Cursor row12 Path A и Path B соединены как один сценарий, и выбрать следующий маленький bounded action.

Смысл текущей фазы: не останавливать цепочку на статусе, а честно отделить узко доказанное от того, что еще требует отдельных evidence packets.

Текущий шаг: записать человекочитаемую доску и машинно-читаемый JSON без изменений кода, governance или skills.

## Узко закрыто

1. Cursor row12 dual-path same-scenario coverage.

Что есть в evidence:

- Path A folder: `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/`.
- Path B folder: `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-path-b-real-ui-20260628T140709Z/`.
- Join folder: `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-joined-path-a-b-20260628T142044Z/`.
- Join status: `PASS`; `join_accepted=true`.
- Scenario id: `frdp-p01-cursor-token-savings-baseline`.
- Pair id: `frdp-p01-row12-cursor-token-baseline-v1`.
- Marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`.
- Prompt hash: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`.
- Path A runner passed and recorded host command/API/core refs.
- Path B real Cursor UI report passed and recorded screenshots for the same row12 marker, without submitting to core.

## Не закрыто

1. All-extensions readiness: не закрыто. Один Cursor row12 scenario не закрывает Claude, Cursor и Codex целиком.
2. Token savings: не закрыто этой доской. Join report explicitly says token savings are not proven.
3. Semantic quality: не закрыто. Нужны rubric, paired scoring, held-out/source-disjoint checks, no-leakage and no-memorization controls.
4. Training and learnability: не закрыто. Нет утверждения, что DPO, training, fresh-load, continue-training или local model update были выполнены.
5. Backup/export/import/share/continue-training lifecycle: не закрыто. Эта доска не проверяла lifecycle для checkpoint или машины.
6. Checkpoint promotion and locator update: не закрыто. Нет mutation, promotion, rollback binding или locator packet.
7. Full product readiness: не закрыто. Текущий пакет является только product readiness evidence для одного same-scenario dual-path coverage unit.

## Следующее bounded action

Следующее действие не является stop.

Выбранный следующий worker: `TOKEN-ROW12-CURSOR-REVIEW-01`.

Маленькая задача: прочитать уже существующий row12 Cursor token evidence packet, если он доступен, например `docs/plans/evidence/extension-full-readiness-dual-path/token-savings-cursor-row12-20260628-153837/`, и выпустить узкий pass/fail packet по тому же `pair_id`.

Что проверить в этом worker:

- baseline/adaptive traces относятся к одному `pair_id`;
- prompt hash совпадает с row12 join;
- token counts и accounting method записаны;
- variance или repeated-run handling явно описаны;
- quality guard не допускает token savings claim при quality regression;
- non-claims сохранены: no semantic quality, no training, no checkpoint promotion, no locator update unless separately proven.

Ограничение worker: без code edits, без training, без checkpoint mutation, без commit и push. Если token packet неполный, worker должен вернуть blocker packet с точным недостающим файлом или измерением и следующим bounded action.
