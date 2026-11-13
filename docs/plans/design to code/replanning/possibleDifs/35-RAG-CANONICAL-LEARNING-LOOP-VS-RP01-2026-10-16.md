# N35-RAG — Разрыв между исходным one-pair циклом обучения и наблюдением RP01

**16 октября 2026 · source-derived gap, НОВАЯ самостоятельная гипотеза об управлении качеством, НЕ доказанная причина RP01 0/3 и 0/4.** Код и `llm_mvp_core/docs/plans/replanning-possible-diffs/**` только читались; в старых RAG и коде ничего не исправлялось.

## 1. От какого исходного обучения нельзя отказаться

- [`original task.md`](../../original%20task.md) и [`Алгоритм создания ДПО.md`](../../Алгоритм%20создания%20ДПО.md) описывают **ILearningUnit** (domain-specific many-to-many), **ILearningFlow** (корректировка между Units), **ILearningOrchestrator** (асинхронная композиция), **ILearningDynamicGraph** (комбинации паттернов), а также поиск **реальной ошибки пользователя**, на основе которой должен возникать следующий учебный сигнал.
- [`09.09.26WorkLeft-RAG-00-usage-guide.md`](../../09.09.26WorkLeft-RAG-00-usage-guide.md) L5–29 предупреждает, что RAG — поисковый указатель, а не свидетельство работы; его восемь gates включают canonical DPO re-open, provider/admission, совместимый checkpoint, applied-once, публикацию, rollback и независимую проверку качества.
- [`09.09.26WorkLeft-RAG-MASTER-INDEX.md`](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) маршрутизирует в [RAG-01 DPO legal](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md), [RAG-02 DesignToCode](../../09.09.26WorkLeft-RAG-02-design-to-code.md) и [RAG-03 ветки](../../09.09.26WorkLeft-RAG-03-merge-and-branches.md). Исторические утверждения `DONE` нельзя переносить без кода/свежего артефакта.
- [`09.09.26WorkLeft-MECH-01-mechanism-validation.md`](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md) L37–54: загрузить **accepted** веса; дать реальную задачу; независимый judge; новый урок; полный снимок до пары; **одно применение новой пары**; затем тот же экзамен; если улучшилось — принять, иначе восстановить parent. **95% — требование, не достигнутый результат.**
- [`09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) L11–45: обе финальные `chosen/rejected` стороны — от допущенной открытой китайской модели, CODEX/Astra 6 UltraCode выбирает cases и отдельно оценивает. Один `pair_id` однократное применение; `bulk` — новые уникальные уроки с observation boundary; исторический replay 70/30 не является автоматически разрешённой политикой. Возможные решения: `IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE`.

## 2. Семь конкретных различий «спецификация → доказательство»

| ID | Что требуется | Что показал RP01 | Что остаётся UNKNOWN |
|---|---|---|---|
| R01 | Совместимый **ранее принятый** checkpoint | В simulation обучалась замороженная random base B0, LoRA менялись | Где обычный продукт загружает accepted parent перед новым уроком |
| R02 | Реальный плохой ответ → reason → законный новый teacher lesson | Замороженные train500 и free3/free4 эксперименты | Участие действительного customer-flow и независимая weakness annotation |
| R03 | Teacher автор обеих финальных DPO сторон, approved provenance | Описаны simulated и repository-owned данные | Exact teacher/provider/version/licence/terms/request receipt на **новую** применяемую пару |
| R04 | One-pair applied-once + полный pre-update snapshot | 50+ DPO optimizer updates на заранее выбранном bulk наборе | Pair-id journal и snapshot/rollback одного нового урока |
| R05 | `chosen` воздействует на **правильный** recipient | RP01 neural Student/LoRA, история E09 KNN и E26 Units — иные состояния | `raw pair → recipient → changed state → ordinary consumer` одного плохого запроса |
| R06 | После одного урока — **тот же независимый** source-family-disjoint экзамен + old-law retention | Численный preference margin и 0/3→0/3, 0/4→0/4 после bulk | Сравнение `parent vs challenger vs no-update` в живом продукте после **одной** пары |
| R07 | `IMPROVE`→commit, иначе rollback | Опубликованы изменённые LoRA tensors без free gain | Конкретный gate при FAIL/INCONCLUSIVE в пользовательском потоке после cold reopen |

**Обязательная оговорка:** RP01 является экспериментом с 500 учебными записями и **не должен автоматически считаться нарушением** производственного applied-once контракта. Впрочем, такой эксперимент **не подтверждает** адаптивный loop принятия одного нового урока. Не выдавать отсутствие прочитанного доказательства за отсутствие исходного кода.

## 3. Новая причинная гипотеза H35-RAG

Если learner последовательно применяет валидные DPO-пары, но **не сравнивает новый обычный ответ с parent после каждой пары** и не откатывает ухудшения, конечная модель может сохранить низкое качество, несмотря на растущий preference margin. One-pair acceptance/rollback может сохранить полезные старые законы лучше, чем ungated bulk. **Эта гипотеза не утверждает, что именно такое случилось в RP01**. Альтернативы: B0 не умеет язык, маскирование DPO не учит начало ответа, chosen/rejected слишком похожи; gate сам по себе их не исправит.

## 4. Минимальный реальный эксперимент: одна новая пара, один update

1. **Source+consumer gate (READ ONLY).** Найти один compatible accepted checkpoint `P0` конкретного домена, его tokenization/model/LoRA/optimizer/index manifests; зафиксировать `P0` SHA и реальные `unit_id/flow_id`. Если parent отсутствует, статус `NO_ACCEPTED_BASE` и переходить к [tiny cold-start/SFT](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) — не подменять продуктивный опыт random B0.
2. **Frozen exam:** 8–12 новых source-family-disjoint вопросов + 4 old-law retention, независимые legal/semantic gold, same ordinary consumer, capture raw answers/recipient/candidate/exception and no-update baseline.
3. **Одна pair:** CODEX/Astra 6 UltraCode в human role выбирает actual error. Если имеется уже разрешённая **неиспользованная** DPO пара, использовать её без teacher call; иначе обе финальные стороны формирует разрешённая внешняя открытая китайская модель при documented provider terms. `pair_id`, content SHA, admission immutable. Прежде чем мутировать, сохранить rollback P0 включая модель, optimizer, RNG/counters и memory/index если они изменяются.
4. **A/B/C:** A — неизменный `P0`; B — один и тот же единственный update `P0→P1` (не дублировать старый `pair_id`); C — решение по существующей B на основании независимого blind exam **без дополнительного обучения**. Учитывать `correct_old`, `correct_new`, false accept, abstain, runtime; не оптимизировать порог на test answers.
5. **Verdict:** `IMPROVE` только при убедительном предметном приросте без критической регрессии, иначе `REGRESS` и полный rollback или `INCONCLUSIVE` с сохранением P0; `INVALID_EVIDENCE` при утечке/неправильном teacher provenance. Fresh-process restore и реальный ordinary flow должны показать тот же результат. Показатель DPO loss или число изменённых weights сами по себе не verdict.

**Сферы:** C#/AST — правильный закон и работоспособный код, а не буквальный match; RU/EN/HE — семантика, направление, полярность, POS/morphology и имена; graph/DSL — законные ordered transitions. Результаты E09/E26 сравнивать как **отдельные получатели**; не писать `RP01 Student repaired` при увеличении KNN retrieval.

## 5. Связь с новыми документами и статус

[Общая карта всех получателей](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md) · [N35 loss-only stop: другой конкретный caller](35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md) · [N35 loss-only короткий доменный A/B](35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md).

**Status:** RAG_SOURCE_CONFIRMED; IMPLEMENTATION_END_TO_END_UNKNOWN; NATIVE_NOT_RUN; HUMAN_AND_TEACHER_NOT_CALLED; IMPACT_HYPOTHESIS; DRAFT_PENDING_INDEPENDENT_REVIEW. Никаких Windows full CI как требование выполнения этого научного one-pair эксперимента.
