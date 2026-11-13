# R17-REPAIR-2 — разбор и подтверждение применённых изменений

**13.10.2026. Статус:** R17_WORK_ORDER_REPAIRED / DOCUMENTARY_POLICY_APPLIED / PRODUCT_LEARNING_NOT_MEASURED / INDEPENDENT_REVIEW_NOT_PERFORMED.

Это адресный ремонт существующего R17 по запросу Лубы, не выполнение заново R01–R16 и не заявление, что все A–F исследования нового R17 уже исполнены. A–F — внутренние части одного назначения; программа 10+5+5+5 сохранена. Три действующих документа изменены; этот memo только фиксирует evidence, не заменяет изменения.

## 1. Что было сильным и сохранено

Прежние R17 prompt/policy уже различали admission, actual update, learned utility, PC cost и host/native delivery; защищали DPO provenance, четыре архитектурные роли, no-repeat, ordinary consumer и bounded stopping. Неверно говорить, что этого совсем не было. Сохранены existing CodeDocumentation Student, Chinese authorship обеих final DPO sides, отдельные CODEX/Astra lesson selection/pair assessment, independent post-update examination, local S04 / remote S05 и исходный whole-program scope.

## 2. Какие дефекты исправлены

| Дефект/риск | Применённая правка | Как проверяется смысл |
|---|---|---|
| README представлял девять workers как программу, R17 уже допускал более широкий старт | README различает R01–R09 research subgroup, R01–R16 concurrent starts и независимые R17 acceptance deltas | Нет требования общего DONE соседей для независимой правки; shared writes остаются сериализованы |
| Короткая карточка оставляла предметное наполнение детям/следующей модели | Полный R17 A–F: роль, skills, source routes, действия, outputs, приёмка, good/bad/edge, not-task; C1/C2 split; полный child composition/return contract | Не только перечень тем или пустой template; missing runtime facts остаются named bindings |
| Общие learning criteria могли не проверить отдельные механизмы | AC01–AC12: mapping, representation, kNN, pair, objective/recipient, LoRA, Unit, Flow, Orchestrator, Graph, hybrid efficiency, continual learning | Unit success не закрывает Flow/Orchestrator/Graph; value связан с actual consumer |
| Single-factor-only правило не отвечало на вопрос о полезности соединения | Policy §4: локальные ablations отдельно от bounded compatible interaction contrast | Joint benefit не равен interaction; нельзя выдумывать incompatible crossed states или replay пары в четырёх ветках |
| Policy §8 требовал от каждого domain/bulk всех механизмов | Affected capability + необходимые integrity/retention controls; remaining domain/interactions имеют R21/R22 owner/trigger | Первый допустимый урок не ждёт всего curriculum/Graph/ablations |
| Неполная спецификация bulk-to-bulk могла принять смену сложности за gain | Same panel внутри bulk; anchor либо явное cohort/difficulty accounting между bulks; final exam не tuning tool | Более лёгкий следующий bulk не доказательство улучшения |
| Full rollback мог быть прочитан как стирание applied-pair counters/history | Immutable/durable used-pair disposition сохраняется вне отката mutable model state | Ранее применённый урок не получает второй update после rollback/restart |
| Старые skill defaults могли вернуть 30/50 последовательных ревью и coverage gates | Scoped methodology, текущие 25 назначений/parallel authority, no new product effects | Названия skills не считаются load evidence; реальные calls не выдумываются |

## 3. Прямое чтение и его пределы

Прочитаны R17 prompt, полная прежняя policy по частям и её свежая concurrent revision, R17 FINDINGS/SOURCES; README, относящиеся START/AGENTS/PLAN/JOIN sections; начальные доступные разделы `previous rounds thinking results.md`; `SOURCE-ROUTES.md` и содержательный переход `09.09.26WorkLeft-RAG-00-usage-guide.md` → current `09.09.26WorkLeft-02-teaching-chinese-model.md` (строки 1–55).

Это **не** exhaustive read всех RAG/WorkLeft/slices и **не** независимая повторная верификация всех результатов R01–R16. Такая работа остаётся распределённой по соответствующим владельцам. Repaired R17 требует versioned intake всех шестнадцати observed carriers с read depth/limits и exact affected clause; доступность carrier не означает завершённость раунда.

Scoped skills прочитаны непосредственно в `lubochka/llm_mvp_core:codex/dpo-implementation`:

| Путь docs/skills/ | Blob | Прочитанный scope |
|---|---|---|
| trainable-architecture-decision-SKILL.md | 81e96fb8981d4eeb4a3f25f57001e74c453ffdbd | 1–85: repair/child/readiness/learned authority, включая конфликтующий historical 30-pass default |
| phase-planning-SKILL.md | 27ba212c829148158511c7a5a69bc418abfc276c | 1–65: live scope, quoted evidence, fresh execution/bounded work |
| code-examination-SKILL.md | 5f6a9eb87686dfc67ef2eda0086c43e7dce47184 | 1–75: complete-read/caller/consumer and claim boundaries |
| plan-review-SKILL.md | b99f4ce280f48d0242a35f2ba421a2dc1996399f | 90–160: child evidence, historical 50-pass ledger, direct skill-load proof |
| prompt-patch-authoring-SKILL.md | 063fe9ca26b2a7921279dd803db91ac4e2551c2c | 1–85: targeted bottleneck, wrong/correct, root-cause repair |
| human-readable-architecture-audit-SKILL.md | ee70e5f46472b6203382d0b393c72c1c79f96f70 | 1–100: readable architecture, scope/evidence and independent-review limits |

Первичные DPO/LoRA references проверены по arXiv abstracts и добавлены в R17 §8. Они не доказывают эффективность XIIGEN и не заменяют проектные owner decisions. Этот ремонт не является новым полным code audit.

## 4. Параллельная запись и сохранённые corrections

Первая попытка policy update с прежним blob `a9b7353f03eb78447f55f6183d12efdacc8d8378` получила **409**: другой автор уже изменил документ. Запись не была применена. Свежая версия `7c500d6ca7449c91cc405bba340b5dbf1e4b3576` перечитана; её содержательные additions сохранены при совмещении:

1. R09 30-run aggregates — ATTRIBUTED_AGGREGATES / RAW_SAMPLE_NOT_RECONSTRUCTED, не измеренная экономия и не источник ratios 3.2×/5.5×.
2. Three-job sample одного run не восстанавливает 30-run population.
3. Local Windows/bootstrap отличается от GitHub Windows campaign; сохранён pinned source `3c7f5f8c7809936e25579603c96ea6df8598fc40`, `25.ModelServer/Program.cs` 83–88 и protected `C:\Projects\llm_mvp_core` как атрибутированное code evidence. Код этой редакцией не перечитывался и не менялся; Linux-ready entry не выдуман.

Чужие carriers/statuses не перезаписаны. No force-push, no workflow/ruleset mutation.

## 5. Применение и readback

| Действующий файл | Commit применения | Подтверждённый remote blob |
|---|---|---|
| run-prompts/R17.md | 8d9c4e54a3eca61f22877cba32ae96c21b7fd947 | e2462b74c1816a535883ef3cfcaa1852452a23cd |
| R17-EVIDENCE-POLICY-2026-10-13.md | 7777360b08100eec85fcccebbef4ed57b302209f | 2b99b93c55381e8acdf946a575640fa8cb022600 |
| README.md | 395e5258482b92f74f91923f90f78768bf2da8be | dbab5d1bf9efc2f4592495ecec82d7fd7bf9c48d |

Все три blob подтверждены отдельным GitHub fetch после успешной записи. Изменённый README прямо открывает R17/policy. Существующий START уже загружает полную numbered card и scoped policy; WorkLeft-02 уже содержит ссылку на этот же policy. Поэтому их links потребляют новую редакцию без копирования всей policy в каждый файл. START/PLAN/AGENTS/JOIN в этом ремонте не переписаны; историческое число research inputs не трактуется как запрет R17. Получение downstream receipt другим работающим агентом не выдумывается: опубликовано/доступно не равно ACKNOWLEDGED/APPLIED у R18.

## 6. Что ещё не доказано и ближайшая работа

Нет новых product tests, Windows/Actions runs, builds, model calls, DPO generation, optimizer steps, weights, benchmark results или независимой Astra review. GitHub publication успешна; branch-protection read permission и управление CI — отдельные вопросы.

Ближайший разрешённый предмет R18 остаётся existing CodeDocumentation first-learning seam: exact pair → actual conditional objective/parameter owner → isolated candidate → ordinary consumer → independent quality/time/retention. R12/R13 должны связать конкретные недостающие runtime/recipe decisions и applicable reviewer scope; это не требование закончить все домены и всю Graph программу перед независимым clause. Ремонт R17 устраняет неправильные gates, но не выдаёт эту будущую работу за выполненную.
