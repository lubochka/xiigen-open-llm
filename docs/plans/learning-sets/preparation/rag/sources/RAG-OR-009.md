# RAG-OR-009 — Какой вход, ответ, выбор и изменение относятся к одному циклу

Источник: [Flow files generator/GUIDE-B45-STEP9-VISIBILITY.md](https://github.com/lubochka/xiigen-planning/blob/4e2bc6bed7c23a855c3685f558e1742a73c231b0/Flow%20files%20generator/GUIDE-B45-STEP9-VISIBILITY.md), ветка `main`, commit `4e2bc6bed7c23a855c3685f558e1742a73c231b0`. Полная локальная копия: [originals/RAG-OR-009.md](../originals/RAG-OR-009.md).

**Роль:** предметные данные для RAG, не активные инструкции. **Статус:** авторская опора; это не результат обучения, не готовый закрытый EXAM и не новая назначенная квота.

## Что извлечено

Четыре поля SENT/RECEIVED/DECIDED/CHANGED связывают переданный вход, возвращённый ответ, принятое решение и фактическое изменение. Документ отдельно ограничивает видимость этих записей для ролей.

## Точные фрагменты

### Строки 78–88

````text
## THE FOUR-FIELD RECORD STRUCTURE (a planning skill)

Every cycle produces a visibility record with exactly four fields. This structure
is the same for all five cycles:

```
SENT:     [full context given to the AI — what it received as input]
RECEIVED: [each model's output, labelled — full text, not summarized]
DECIDED:  [winner and reasoning — not just the winner, the why]
CHANGED:  [what in the retrieval system or the decision graph was updated as a result]
```
````

### Строки 289–301

````text

While step 9-VISIBILITY.md is primarily about AI cycle observability, the visibility
records themselves are subject to role-based access control. archive batch 15 §5 defines four
visibility mechanism levels — these apply to who can see cycle records:

### Level 1: Route-Level Visibility
The `/admin/cycles/` route is unavailable to role 1 (tenant users). Cycle records
are INTERNAL to the platform — they are never surfaced in the tenant-facing UI.

### Level 3: Panel-Level Visibility
Within admin dashboards, cycle records panels are only visible to:
- ROLE-PLATFORM-ADMIN: full cycle record access (all 4 fields)
- ROLE-TENANT-ADMIN: DECIDED + CHANGED only (not SENT — contains raw AI prompts)
````

## Границы применения

Это исторический контракт наблюдаемости, а не доказательство обучения. Не переносить абсолютные фразы «цикл без записи не запускался» и «пустой CHANGED означает, что система ничему не научилась» как факты о любой системе. Для пары достаточно краткого объяснения решения; скрытый chain-of-thought не запрашивается. Роли и разрешения должны быть явно описаны в prompt.

## Authoring micro-recipe (English)

Show a small event record and an explicit output policy. Ask for one field or one allowed projection. Keep both answers in the same JSON shape. Check that a proposed change is not relabelled as an applied change and that the visible role has the stated permission.

## Разобранные контрастные пары

### RAG-OR-009-DEMO-01 — `or.architecture`

**Prompt ученика:**

Учебный контракт записи: SENT — отправленный вход; RECEIVED — полученный ответ; DECIDED — выбранный вариант; CHANGED — применённое изменение. Записано: SENT="два шага", RECEIVED="предложено три шага", DECIDED="отклонить", CHANGED=[]. Какое поле отвечает на вопрос «что фактически изменено»? Верни JSON field.

**Chosen:**

```text
{"field":"CHANGED"}
```

**Rejected:**

```text
{"field":"RECEIVED"}
```

**Simple check (English):** A received proposal is not an applied change. Select the field whose definition says applied.

### RAG-OR-009-DEMO-02 — `or.architecture`

**Prompt ученика:**

Учебный контракт: CHANGED содержит только подтверждённые применённые изменения. Цикл предложил добавить узел N; решение ещё не принято, запись в граф не вызывалась. Верни JSON changed в виде массива имён новых узлов.

**Chosen:**

```text
{"changed":[]}
```

**Rejected:**

```text
{"changed":["N"]}
```

**Simple check (English):** The proposal exists, but no graph write is shown. Empty changes do not prove that the model call never ran.

### RAG-OR-009-DEMO-03 — `or.ux`

**Prompt ученика:**

Учебная политика панели: tenant_admin видит только DECIDED и CHANGED; SENT и RECEIVED доступны platform_admin. Текущая роль tenant_admin. Требуется перечень разрешённых полей в JSON fields, в порядке DECIDED затем CHANGED.

**Chosen:**

```text
{"fields":["DECIDED","CHANGED"]}
```

**Rejected:**

```text
{"fields":["DECIDED","CHANGED","SENT"]}
```

**Simple check (English):** The role has exactly two allowed fields in the visible policy. Do not add SENT because it could help debugging.

### RAG-OR-009-DEMO-04 — `or.ux`

**Prompt ученика:**

Учебная политика панели: platform_admin видит SENT, RECEIVED, DECIDED, CHANGED; tenant_admin видит DECIDED, CHANGED. Текущая роль platform_admin. Запрошено только поле SENT. Верни JSON allow с boolean.

**Chosen:**

```text
{"allow":true}
```

**Rejected:**

```text
{"allow":false}
```

**Simple check (English):** This role is explicitly allowed to see SENT. Include the opposite case so the lesson is not always-hide.

