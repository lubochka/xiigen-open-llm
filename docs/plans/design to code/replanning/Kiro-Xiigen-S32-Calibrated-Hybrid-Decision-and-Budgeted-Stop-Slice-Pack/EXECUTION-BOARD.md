# S32 — 24 bounded dispatches (NOT 24 executions)
| Task | Unique deliverable | Earliest dependency | Existing owner |
|---|---|---|---|
| [S32.00A](tasks/S32.00A.md) | Обход RAG→первоисточники, прошлые решения и Rxx | сразу / read-only | source architect |
| [S32.00B](tasks/S32.00B.md) | Трассировка настоящего consumer, arbiter и всех разнородных scores | сразу / read-only | K code examiner |
| [S32.00C](tasks/S32.00C.md) | Семь паспортов предметных законов и независимые source families | сразу / read-only | domain-law steward |
| [S32.00D](tasks/S32.00D.md) | Актуальные owners, full-PC бюджет и состояние baseline | сразу / read-only | PC/state investigator |
| [S32.01A](tasks/S32.01A.md) | Типизированный evidence envelope и версия координат | 00B, параллельно 00C | architect RP02/RP03/RP04 |
| [S32.01B](tasks/S32.01B.md) | Предмет, intent и обучаемая единица калибровки | 00C, 00B | RP02/RP11 owner |
| [S32.01C](tasks/S32.01C.md) | Нарушение закона, отказ, пороги риска и safe fallback | 00C, 01A | domain arbiter owner |
| [S32.01D](tasks/S32.01D.md) | Независимая frozen rubric и blind eval split | 00C, 00D | RP09 examiner |
| [S32.02A](tasks/S32.02A.md) | Проверка raw→calibrated score на реальном вызове | 01A,00B | K examiner/RP02 |
| [S32.02B](tasks/S32.02B.md) | Калибровка надежности, права на update и честный cold state | 01B,01D,00D | RP11 owner, optional |
| [S32.02C](tasks/S32.02C.md) | Маржинальная польза ещё одной ветви против full-PC цены | 01C,00B,00D | RP04 scheduling owner |
| [S32.02D](tasks/S32.02D.md) | ОДИН доказанно отсутствующий envelope→arbiter bridge или REUSE | 01A,01C,02A, grant | S32 narrow adapter |
| [S32.03A](tasks/S32.03A.md) | Ранний parent P / deterministic C0 / no-update E | 00B,00C,01D | experiment owner |
| [S32.03B](tasks/S32.03B.md) | Условный learned calibration T1 у существующего RP11 owner | 02B,01D + rights | RP11 learner, optional |
| [S32.03C1](tasks/S32.03C1.md) | Выбор настоящего остаточного Student weakness CODEX | 03A + model grant | CODEX lesson chooser, optional |
| [S32.03C2](tasks/S32.03C2.md) | Обе финальные Chinese стороны, отдельный exact CODEX judge | 03C1 + provider/data grant | Teacher + separate judge, optional |
| [S32.03C3](tasks/S32.03C3.md) | Один admitted Student LoRA/DPO update, snapshot & cold reload | 03C2 + optimizer/PC grant | RP01/RP05 owner, optional |
| [S32.03D](tasks/S32.03D.md) | Реальная обратная связь→новый bulk T2, no-repeat и retention | 03A + feedback consent | RP05/S23, optional |
| [S32.04A](tasks/S32.04A.md) | Blind known/shifted/mixed/novel domain examination | 01D | independent RP09 |
| [S32.04B](tasks/S32.04B.md) | Факторные P/C0/E/T1/T2 сравнения и калибровочный риск | 04A + available arms | independent RP09 |
| [S32.04C](tasks/S32.04C.md) | Обычный ПК: whole-PC, abort/cancel, restart и cold/warm | 00D,04A | S23/PC owner |
| [S32.04D](tasks/S32.04D.md) | Cold ordinary user answer/abstention with accepted pointer | 00B,04A + actual state | RP06/RP10 host |
| [S32.05A](tasks/S32.05A.md) | Один минимальный owner-bound repair по доказанному дефекту | 02D/04B as evidence + grant | existing K owner |
| [S32.05B](tasks/S32.05B.md) | Независимый Astra exact-SHA review; honest handoff | actual receipts only | separate CODEX Astra reviewer |

**Independent READ/EXAM lanes:** 00A,00B,00C,00D; 01D can freeze blind panel as soon as independent law source exists. 03A starts on one real caller and one lawful case, without waiting on all calibration development or Student. Conditional product code ONLY 02D; 05A one proven repair owned by original K module. 03B needs explicit existing RP11 mutable state; 03C1→03C2→03C3 are distinct separately authorized/model-specific calls, none is auto-executed. 04B/04C/04D can grade only real available arms. 05B requires separate reviewer invocation. Logical independence is not authorization for multiple active agents (OP-65).

**Stop-and-divide:** if trace/experiment crosses >one normal consumer, >one mutable state owner, >one law version, or >one producer scoring family, do not broaden card. Return `S32.xx.A/B` *proposal* under current owner; create only after owner scope approval. When one owner has already implemented equivalent, `REUSE_NO_CHANGE` (with real method receipts) is higher-value than duplicate adapter.

**Earliest useful critical path:** `00B + one 00C law + frozen 01D → 03A P/C0/E no-gradient result → 04B on actual arms`; no R01–R25 waterfall; never Windows CI as non-deterministic accuracy proxy.
