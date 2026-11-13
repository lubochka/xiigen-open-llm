# Codex Path B clean UI attempt

claim_scope: scenario coverage evidence for one Codex Path B clean UI attempt only.
evidence_scope: this folder, UI Automation summaries, screenshots, and manual screenshot review.
count_claimed: 1 Path B clean UI attempt packet.
independent_evidence_count: UIA summaries plus 11 screenshots plus JSON run records.
subagent_id_or_NONE: NONE visible; bounded sub-agent continuation in current session.
downgrade_needed: broader claims are downgraded. This packet does not prove product readiness.

## Человеческий статус

Всего сценариев в этом пакете: 1.

Path A уже был принят в отдельном evidence-пакете:
docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/codex-path-a-api-boundary-20260627-223748/.

Path B в этом пакете: `partial_submitted_response_pending_or_timeout`. Точный prompt был виден в отдельном безопасном Codex New chat, затем был отправлен через Enter. По binding parent review сохранённые screenshots поддерживают только prompt visible + submitted + `Thinking`; они не показывают видимый assistant response или видимую ошибку. Поэтому Path B не принят. Это полезный прогресс scenario coverage, но для полного Path B всё ещё нужен screenshot с видимым ответом или видимой ошибкой в Codex UI.

Это scenario coverage evidence; product-readiness count must not be used.

## Точный prompt

```text
Через XIIGen Smart Skills разберись с задачей: пользователь хочет улучшить документацию продукта так, чтобы она была понятна человеку. Переведи запрос в универсальную задачу, найди универсальный план оркестрации и выбери подходящие скилы. Используй только универсальную часть без привязки к конкретному языку программирования, фреймворку или стеку.
```

## Точные UI-действия

1. Прочитаны scenario prep, Path A packet и предыдущий Path B blocker packet.
2. Найдено окно Codex Desktop.
3. Сохранены window/screen/UIA summaries и screenshot до ввода.
4. Через UI Automation нажата доступная кнопка New chat. Кнопка Stop в занятом старом thread не нажималась.
5. В новом chat подтверждён видимый composer ProseMirror.
6. Первая paste-попытка не ввела prompt; это зафиксировано как tooling/UI issue.
7. Один retry paste в тот же подтверждённый composer показал exact prompt в UI.
8. В narrow continuation prompt всё ещё был виден; повторная вставка не потребовалась.
9. Prompt был отправлен клавишей Enter.
10. Сохранены screenshots перед submit, после submit, после ожидания и после extended wait.
11. Binding parent review исправил прежний overclaim: сохранённые screenshots показывают prompt visible + submitted + `Thinking`, но не показывают видимый assistant response или видимую ошибку.

## Скриншоты

- screenshots/codex-window-before-newchat.png
- screenshots/codex-newchat-before-input.png
- screenshots/codex-input-focus-before-paste.png
- screenshots/codex-before-submit-with-prompt.png
- screenshots/codex-after-submit.png
- screenshots/codex-visible-result-or-timeout.png
- screenshots/codex-before-submit-with-prompt-retry.png
- screenshots/codex-continuation-before-submit.png
- screenshots/codex-continuation-after-submit.png
- screenshots/codex-continuation-after-response-error-timeout.png
- screenshots/codex-continuation-final-after-extended-wait.png

Screenshot count: 11.

## Исправление overclaim

Прежняя формулировка была слишком широкой, потому что говорила о видимом response. Binding parent review установил более узкую границу: сохранённые screenshots поддерживают prompt visible + submitted + `Thinking`, но не видимый assistant response и не видимую ошибку. Исправленный статус: `partial_submitted_response_pending_or_timeout`.

## Почему Path B не принят

Path B не принят полностью, потому что не выполнено минимальное условие о видимом ответе или видимой ошибке в Codex UI. Доказано только следующее:

- safe separate Codex New chat был найден;
- exact prompt был виден в подтверждённом composer перед submit;
- exact prompt был отправлен;
- после submit сохранённые screenshots показывают `Thinking`, но не показывают assistant response/error.

## Что не доказано

- Не доказана готовность продукта.
- Не доказаны token savings.
- Не доказана semantic quality.
- Не доказаны learnability, training или DPO.
- Не доказан checkpoint promotion.
- Не доказан locator update.
- Не доказан видимый assistant response или видимая ошибка в Codex UI.
- Не доказано, что ответ полностью завершился.

## Итоговая граница claim

Этот пакет закрывает только Codex Path B local UI scenario coverage для prompt codex-full-extension-runtime-20260627-090817. Его нельзя использовать как product-readiness count.
