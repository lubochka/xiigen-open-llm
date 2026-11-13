# Cursor Path B route script

## Статус покрытия сценария Cursor

Это шаг 1 из 3 для Cursor Path B: сценарий маршрута, затем route-proof run, затем prompt-response run.

- Path A принят: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/cursor-path-a-api-boundary-20260627-235243`
- Источник того же сценарного prompt: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-prep/cursor-full-extension-runtime-after-unlock-20260627-090159/scenario.json`
- Path B еще не принят.

## Что показала предыдущая попытка

Предыдущая попытка показала только узкие факты:

- Cursor Extension Development Host открылся.
- В той попытке не был замечен login prompt.
- В той попытке не был замечен storage modal.
- Она не доказала маршрут XIIGen, отправку prompt или видимый ответ.
- Блокер сейчас: UI route/control script, а не auth.

## Следующий executor script

Цель: сначала доказать видимый маршрут XIIGen Smart Skills в Cursor Extension Development Host. Только после этого можно вставлять сценарный prompt.

1. Запустить или сфокусировать Cursor Extension Development Host.
   Ожидаемый видимый результат: активное окно Cursor Extension Development Host видно на экране.

2. Подтвердить target window title.
   Ожидаемый видимый результат: title соответствует Extension Development Host для `cursor-xiigen-smart-skills - Cursor` или явно тому же Extension Development Host окну этого workspace. Если title не подтвержден, остановиться и записать `TARGET_WINDOW_NOT_FOUND`.

3. Если случайно открыт Browser Tab или команда `Open Browser`, закрыть этот tab/panel без ввода сценарного prompt.
   Ожидаемый видимый результат: фокус снова на основном Cursor Extension Development Host окне; сценарный prompt нигде не введен.

4. Безопасно открыть command palette.
   Ожидаемый видимый результат: видна command palette, курсор в ее поле поиска.

5. Перед вводом маршрута найти точные candidate command names из repo/extension manifest, если это возможно в текущем run. Target terms для поиска: `xiigen`, `XIIGen`, `Smart Skills`, `smartSkills`, `chat participant`.
   Ожидаемый видимый результат: executor записывает найденные candidate command names и источник. Если точный command name не найден, остановиться до ввода сценарного prompt и записать `BLOCKED_ROUTE_NOT_PROVEN`.

6. Использовать clipboard paste для точного command text вместо прямого набора текста, потому что прямой набор в прошлой попытке исказил ввод до `9975,`.
   Ожидаемый видимый результат: в command palette виден вставленный точный candidate command text, без лишних символов и без искажения.

7. Выполнить найденный XIIGen Smart Skills route candidate только если он видим и совпадает с найденным command name.
   Ожидаемый видимый результат: видна XIIGen панель, route label, chat participant route или другой явный XIIGen Smart Skills entrypoint. Если вместо этого открывается Browser Tab, command palette теряет фокус, или route label не виден, закрыть лишний UI, остановиться и записать `BLOCKED_ROUTE_NOT_PROVEN`.

8. Только после видимого доказательства XIIGen route вставить точный сценарный prompt из поля `exact_prompt` файла `scenario.json`.
   Ожидаемый видимый результат: prompt виден в XIIGen route input, не в browser/address bar/command palette.

9. Capture before-submit screenshot.
   Ожидаемый видимый результат: screenshot показывает XIIGen route и вставленный prompt до отправки.

10. Отправить prompt.
    Ожидаемый видимый результат: Cursor UI начал обработку запроса в XIIGen route.

11. Дождаться видимого response или timeout по сценарию.
    Ожидаемый видимый результат: после ответа виден структурированный task, orchestration plan, selected skills, core warning/error или другой ответ, связанный с XIIGen Smart Skills.

12. Capture after-response screenshot.
    Ожидаемый видимый результат: screenshot показывает XIIGen route, prompt context и видимый response.

## Запрещено

- Не вводить сценарный prompt до доказательства route.
- Не сообщать, что Path B принят, если не captured visible route + prompt + response.
- Не использовать старую терминологию общей готовности продукта; использовать scenario coverage.
- Не запускать UI automation в рамках этого script artifact.

## Оставшиеся действия

1. Этот artifact: Cursor Path B route script.
2. Следующий bounded run: route-proof run без сценарного prompt, если route не доказан.
3. После принятого route proof: prompt-response run с exact prompt из `scenario.json`.

## Evidence boundary

Этот artifact не доказывает Path B. Он только задает следующий human-readable UI route script и stop conditions для executor.
