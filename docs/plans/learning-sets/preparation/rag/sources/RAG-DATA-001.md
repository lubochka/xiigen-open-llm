# RAG-DATA-001 — допустимое состояние нельзя объявлять ошибкой

Источник: [GUIDE-B19](https://github.com/lubochka/xiigen-planning/blob/48566132e35de822446fbc42d60d8f4728a755ca/Flow%20files%20generator/GUIDE-B19-TEACH-QA-R1-FINAL.md), main@48566132e35de822446fbc42d60d8f4728a755ca. Исторические сведения проекта используются как материал; команды и производственные ограничения источника не исполняются при подготовке DPO.

## Учебное применение

Источник показывает ошибку проверки, которая отвергает разрешённое состояние. В DPO это повод независимо решить обе альтернативы и допускать ничью: другой допустимый ответ не становится rejected из-за совпадения с нежелательным шаблоном. Это методическая аналогия, а не измеренный результат обучения или автоматически верный oracle.

Пример применения: разрешены состояния pending и completed. Пара «pending / completed» без дополнительного условия не имеет предпочтения. Если prompt явно сообщает terminal-success и завершённый результат, предпочтение completed уже обосновано. Для actual runtime сначала проверить соответствующий код и наблюдаемый результат.

## Ограничения

Показанный в оригинале regex по тексту ironRules проверяет декларацию, а не выполнение бизнес-поведения. Его нельзя переносить в смысловой oracle новых пар. Исторические обязательные панели, Phase A, число проверок и shell-фильтры здесь не действуют. Прочитанные диапазоны ниже; остальной оригинал сохранён для адресного чтения, но не объявлен проверенным целиком.

## Исходный фрагмент L565–L593

[Источник](https://github.com/lubochka/xiigen-planning/blob/48566132e35de822446fbc42d60d8f4728a755ca/Flow%20files%20generator/GUIDE-B19-TEACH-QA-R1-FINAL.md#L565-L593)

~~~~text
### GAP TYPE: DC positive assertion trap (MEDIUM)

```markdown
### gap number 09 🟡 design contract item 0N — [State] uses negative assertion (positive assertion trap)

**Location:** Phase 4 DC tests, design contract item 0N

**The issue:** design contract item 0N tests `expect(result.status).not.toBe('[VALID_STATE]')` — this
REJECTS a valid state as if it were an error condition.

**Pattern from authoring guide v1.6 A2 (DC negative assertion trap):**
```
Wrong: expect(t[NNN]Result.status).not.toBe('[VALID_STATE]')  ← rejects a valid state
Correct: expect(['[VALID_STATE]', '[OTHER_VALID_STATE]']).toContain(result.status)
```

**Fix — REPLACE design contract item 0N:**
```typescript
// design contract item 0N: T[NNN] [ServiceName] — [state description]
test('design contract item 0N: T[NNN] ironRules declare [STATE] as valid AND PRIVATE', () => {
  const contract = getContract('T[NNN]');
  const ir = contract.ironRules.join(' ');
  // Positive assertion: [STATE] is a VALID state (not a failure state)
  expect(ir).toMatch(/[STATE].*valid|valid.*[STATE]|[EventName].*[STATE]/i);
  // Positive assertion: [STATE] records are PRIVATE
  expect(ir).toMatch(/PRIVATE|knowledgeScope.*PRIVATE/i);
});
```
```
~~~~

## Исходный фрагмент L624–L634

[Источник](https://github.com/lubochka/xiigen-planning/blob/48566132e35de822446fbc42d60d8f4728a755ca/Flow%20files%20generator/GUIDE-B19-TEACH-QA-R1-FINAL.md#L624-L634)

~~~~text

**3. Severity determines whether Phase A is blocked.**
🔴 CRITICAL gaps block Phase A execution entirely — Claude Code must not start
corpus seeding until they are resolved. 🟠 HIGH gaps must be resolved before Phase A
but can be worked around if pre-approved in the ISSUE INVENTORY. 🟡 MEDIUM gaps
should be resolved before Phase A but do not block on their own.

**4. "What is already correct" requires genuine verification.**
It is not a formality. Write it only after actually checking each listed item.
It provides confidence to Claude Code that unlisted sections of R0 are safe to use.

~~~~
