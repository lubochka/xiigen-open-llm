# RAG-OR-008 — Результат шага симуляции по наблюдаемым входам и выходам

Источник: [Flow files generator/GUIDE-B13-SESSION-SIM-RN.md](https://github.com/lubochka/xiigen-planning/blob/4e2bc6bed7c23a855c3685f558e1742a73c231b0/Flow%20files%20generator/GUIDE-B13-SESSION-SIM-RN.md), ветка `main`, commit `4e2bc6bed7c23a855c3685f558e1742a73c231b0`. Полная локальная копия: [originals/RAG-OR-008.md](../originals/RAG-OR-008.md).

**Роль:** предметные данные для RAG, не активные инструкции. **Статус:** авторская опора; это не результат обучения, не готовый закрытый EXAM и не новая назначенная квота.

## Что извлечено

Источник различает WORKS, BREAKS, SILENT_FAILURE, NOT_RUN и PARTIAL. Правильность выхода отличается от успешного возврата обработчика; следующий шаг может быть не исполнен из-за предыдущего отказа.

## Точные фрагменты

### Строки 351–374

````text

From R2 onward, scenarios are **flow-specific** — derived from the flow's design
simulation (B-12) and the task type archetypes in the flow spec.

**For each additional round, the author must:**

1. **Pick a scenario** from the E2..E8 table above that applies to this flow.
   Example: if the flow has named checks, include an E6 (arbiter panel) round.
   If the flow has cross-flow dependency, include an E7 round.

2. **Write a concrete trace question** based on the actual task types (T-numbers)
   and handler names from the flow spec or design simulation.

3. **Trace all 5 verdict options** in the STEP TABLE using the five-value vocabulary:
   - `WORKS` — handler executes correctly, output matches expected
   - `BREAKS` — handler throws or returns wrong type; execution stops
   - `SILENT_FAILURE` — handler succeeds but output is wrong; no error raised
   - `NOT_RUN` — blocked by a prior step failure
   - `PARTIAL` — handler partially succeeds; some output correct, some wrong

4. **Add a GAP entry** for every step with a non-WORKS verdict. Gap IDs:
   - H-class: infrastructure (missing fixtures, wrong paths)
   - J-class: interface/implementation missing (method absent, type mismatch)
   - K-class: logic error (wrong output, wrong calculation)
````

## Границы применения

Исторические инфраструктурные pre-gates, STOP, Elasticsearch и число раундов не являются командами текущему генератору. В новых примерах явно задаётся словарь результатов; не требуется искусственно включать все пять вердиктов в каждый сценарий. Таблица с Actual из исторического документа не доказывает, что текущий код воспроизвёл тот же сбой.

## Authoring micro-recipe (English)

Choose one requested output and one observed output. State whether the handler ran and whether it raised an error. Write two answers in the same format; change only the verdict. Check the definition against the shown facts, not against an optimistic status label.

## Разобранные контрастные пары

### RAG-OR-008-DEMO-01 — `or.automation`

**Prompt ученика:**

Учебная симуляция. WORKS: обработчик запущен, ошибок нет и выход совпадает с требуемым. SILENT_FAILURE: ошибок нет, но выход неверен. Требуется count=3. Обработчик запущен, вернул success=true и count=0, исключений нет. Верни только JSON с полем verdict.

**Chosen:**

```text
{"verdict":"SILENT_FAILURE"}
```

**Rejected:**

```text
{"verdict":"WORKS"}
```

**Simple check (English):** The expected count is 3 and the observed count is 0. Compare output values before reading the success flag.

### RAG-OR-008-DEMO-02 — `or.automation`

**Prompt ученика:**

Учебная симуляция. BREAKS: этот шаг запущен и прервался ошибкой. NOT_RUN: шаг не вызывался из-за отказа зависимости. Шаг A завершился ошибкой; планировщик не вызывал B. Классифицируй только B. Верни JSON verdict.

**Chosen:**

```text
{"verdict":"NOT_RUN"}
```

**Rejected:**

```text
{"verdict":"BREAKS"}
```

**Simple check (English):** The question is about B. B was never called, so it cannot have thrown its own runtime error.

### RAG-OR-008-DEMO-03 — `or.qa`

**Prompt ученика:**

Учебная симуляция. WORKS означает, что все требуемые выходы правильны; PARTIAL — часть правильна, часть неверна. Требовались summary=A и route=B. Один запущенный шаг выдал summary=A и route=C без исключений. Верни JSON verdict по заданному словарю.

**Chosen:**

```text
{"verdict":"PARTIAL"}
```

**Rejected:**

```text
{"verdict":"WORKS"}
```

**Simple check (English):** One of two outputs matches, the other does not. Do not count the matching field as proof for the whole step.

### RAG-OR-008-DEMO-04 — `or.qa`

**Prompt ученика:**

Учебная симуляция. WORKS: шаг исполнен и выход полностью совпал с ожидаемым. NOT_RUN: шаг не вызывался. В показанной трассе вызван handler H с input=7; expected=14; actual=14; ошибок нет. Верни JSON verdict только для этого шага.

**Chosen:**

```text
{"verdict":"WORKS"}
```

**Rejected:**

```text
{"verdict":"NOT_RUN"}
```

**Simple check (English):** An actual call and matching output are present. Do not always select a negative status merely because this is a QA task.

