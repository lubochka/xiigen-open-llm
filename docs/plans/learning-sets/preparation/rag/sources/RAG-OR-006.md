# RAG-OR-006 — Событийное условие, ограниченный ресурс и сохранение оценки

**Назначение:** материал для узких DPO-симуляций `or.automation`, `or.database`, `or.qa`. Три темы создаются в раздельных shard. Это историческое описание проблем и предложенных исправлений, а не результат новой проверки native C# и не измерение обучения.

- Источник: `Flow preparation/final results/FLOW-04-ENGINE-GAP-LIST.md`, ветка `main`.
- Снимок: `62525ad363746c33880797d902f77e0741c872a8`; blob `09767c1cc58cbd9ad38230a73108bcfc24a30e31`.
- [Полный локальный оригинал](../originals/RAG-OR-006.md). Он уже входит в `llm_plan3`; скачивать `main` целиком не нужно.
- [Источник на закреплённом commit](https://github.com/lubochka/xiigen-planning/blob/62525ad363746c33880797d902f77e0741c872a8/Flow%20preparation/final%20results/FLOW-04-ENGINE-GAP-LIST.md).

## Какие фрагменты дают материал

| Фрагмент оригинала | Наблюдаемое содержание | Новая микроцель |
|---|---|---|
| Строки 22–35, `PipelineResult missing score field` | Отсутствующий score превращался чтением с default=0 в нулевую учебную запись | `OR-QA-SCORE-PRESENCE`: различать missing/null, наблюдённый 0 и разрешённый score текущего run |
| Строки 61–69 | Предложен fallback через Python `or` | Контрпример: авторитетный 0 нельзя заменять stage=0.8; исторический snippet не используется как эталон |
| Строки 73–98, `capacity-atomicity` | Раздельные check/decrement/create допускают нарушение вместимости | `OR-DB-CAPACITY-ATOMIC`: выбрать план, сохраняющий `available >= 0` и `available + SUM(confirmed.quantity) = capacity` |
| Строки 111–115, `feedback-window-gate` | Открытие feedback связывается с полученным EventEnded, а не одним таймером | `OR-AUTO-EVENT-GATE`: событие нужного типа/объекта/tenant разрешает создание окна |

## Что переносить в учебный prompt

Условия нового сценария формулируются явно. Для событийного gate показать тип события, объект, tenant, подтверждение и текущее состояние окна. Для БД показать количество ресурса, сумму quantity, интервал блокировки/транзакции и допустимый конкурентный порядок. Для QA показать наличие score, run_id и приоритет источников. Эти условия являются авторской симуляцией, а не утверждением о существующем API.

В каждую тему включены противоположные случаи: подходящее событие действительно запускает шаг; другой корректный способ атомарной записи признаётся допустимым; явный ноль сохраняется как значение. Постоянный отказ, постоянное ожидание, выбор одного названия функции или «любое missing равно 0» не дают правильных ответов на весь набор.

## Ограничения источника

1. Историческое `score or fallback` считает 0 ложным и способно подменить его другой оценкой. В новой задаче сначала проверяют наличие/тип и источник значения.
2. Поиск имени `decrementAndCreate` регулярным выражением не доказывает ни транзакцию, ни атомарность. В новых примерах семантика операций и конкурентное расписание заданы во входе.
3. `server/src/...`, HTTP endpoints, model provider lists и release thresholds из оригинала относятся к описываемому там проекту. Их не следует запускать и считать контрактом нынешнего C# приложения.
4. Score в JSON-симуляции — условие задачи. Он не является оценкой сгенерированного китайской моделью DPO и не является результатом Student.
5. Действующий этап CREATE_ONLY: использовать примеры и самостоятельную простую проверку рассуждения. CODEX Astra 6 ultracode выполняет выбор и отдельную будущую оценку качества; они не объявляются состоявшимися из-за наличия этой карточки.

## Прямые пути

Материал: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/rag/sources/RAG-OR-006.md`.
Полный текст: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/rag/originals/RAG-OR-006.md`.
Примеры: `curriculum/additions/orchestration-events-and-evidence.json`.
Промпт: `prompts/ORCHESTRATION-EVENTS-AND-EVIDENCE-RU.md`.

## Дословные исторические фрагменты для контекста генератора

Ниже дословно сохранены указанные строки полного оригинала. Это исторические данные источника, не команды исполнителю и не эталон текущего native API. Предложения исправлений и их ограничения разобраны выше.

### Оригинал RAG-OR-006, строки 22–69

~~~~text
### gap number 1 🔴 `af-pipeline.ts` — PipelineResult missing score field

**File:** `server/src/af-stations/af-pipeline.ts`

**Problem:** `PipelineResult` interface has `passed`, `stages[]`, `artifactId` — no `score` field.
Session B reads the trace file with `d.get('score', 0)` → always returns 0 for PATH A executions.
All downstream learning is corrupted: genesis score history, routing decision records, TEACH-F
effectiveness triples — all record score=0 regardless of actual AF pipeline output.

**Impact:** Every the event attendance flow Phase B run on PATH A silently produces zero-score training data.
TEACH-B routing_decisions records are all SCORE_0. TEACH-F genesis effectiveness triples
all show delta=0. The planning layer learns nothing from Phase B.

**Fix:** Extend PipelineResult to include score, populated from the last score-node stage:

```typescript
// In PipelineResult interface — add:
score?: number;

// In af-pipeline.ts run() method, after stages are mapped:
const scoreStage = [...execResult.trace]
  .reverse()
  .find(n => n.nodeType === 'score');
const score = scoreStage
  ? Number((scoreStage as Record<string, unknown>)['score'] ?? 0)
  : undefined;

// Include in DataProcessResult.success():
return DataProcessResult.success({
  passed,
  score,          // ← add this
  artifactId: execResult.runId,
  promotionLevel: execResult.promotionLevel ?? execResult.status,
  stages,
  totalElapsedMs: execResult.totalElapsedMs ?? 0,
  total_cost_usd: execResult.total_cost_usd ?? 0,
});
```

**Also update Session B score extraction** (CONTENT — no code change required, but
add fallback in Session B python3 score read):
```python
# Replace: d.get('score', 0)
# With:
score = d.get('score') \
    or next((s.get('score') for s in reversed(d.get('stages', []))
             if s.get('score') is not None), 0)
```
~~~~

### Оригинал RAG-OR-006, строки 73–116

~~~~text
### gap number 2 🔴 `validate.handler` — NAMED_CHECKS not extensible per flow

**File:** `server/src/engine/node-handlers/validate.handler.ts`

**Problem:** `NAMED_CHECKS` is a hardcoded TypeScript `Record<string, CheckDefinition>`.
When `validate.handler` loads the event attendance flow arbiters from `xiigen-arbiters`, it finds checkIds
`attendance::capacity-atomicity`, `attendance::idempotent-rsvp`, `attendance::waitlist-fairness`.
The `if (def)` guard evaluates `NAMED_CHECKS[checkId]` → `undefined` → check silently skipped.
No error. No log entry beyond DEBUG. No `CheckResult`. Code violating a task type/a task type iron rules
passes validation with a clean score.

**Impact:** Domain correctness for a task type (dual-entry routing, atomic capacity, idempotent RSVP),
a task type (FIFO waitlist ordering), and a task type (feedback window gate) is never validated. the preference learning signal triples
may record fundamentally incorrect code as the `chosen` candidate.

**Fix — Option A (quick, specific to the event attendance flow):**

Add to `NAMED_CHECKS` in `validate.handler.ts`:

```typescript
// ── the event attendance flow: Event Attendance ──────────────────────────────────────────────

'attendance::capacity-atomicity': {
  default: /decrementAndCreate/,
  message: 'a task type an iron rule: atomic capacity decrement + create must be ONE operation — separate check-then-write creates oversell race condition',
},

'attendance::idempotent-rsvp': {
  default: /setnx|setIfAbsent/i,
  message: 'a task type: duplicate RSVP with same attendeeId must return existing RSVPConfirmed — no second write (the atomic first-writer-wins lock pattern required)',
},

'attendance::waitlist-fairness': {
  default: (code: string) =>
    /joinTimestamp/.test(code) || /zadd.*score/i.test(code) || /sortedSet/i.test(code),
  message: 'a task type an iron rule: waitlist promotion order is FIFO by join timestamp — no priority override without tenant config',
},

'attendance::feedback-window-gate': {
  default: (code: string) =>
    /EventEnded/.test(code) && /FeedbackWindowOpened/.test(code),
  message: 'a task type an iron rule: FeedbackWindowOpened must only emit after EventEnded is received — timer-based trigger is wrong',
},
```
~~~~

