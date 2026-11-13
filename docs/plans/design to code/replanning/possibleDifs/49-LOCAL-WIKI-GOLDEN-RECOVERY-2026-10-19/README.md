# 49-GOLD — восстановление успешного поведения `local_wiki_planning` (19.10.2026)

**Первый neural приоритет:** [**E-W00** — не терять `Prompt/Chosen/Rejected` в DPO](03-CAUSAL-MICRO-EXPERIMENTS.md) по [source + аналитической проекции владельца N49](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md): старые 50 prompts превращаются в 4 model-visible groups при переносе через current `RejectedType`. Это **не выполненный current native DPO**; затем E-W01 lost full-stack intent.\n\n**Первый приоритет:** [главная глава восстановления](../00-00-LOCAL-WIKI-GOLDEN-FIRST-2026-10-19.md). Эталон `llm_mvp_core@local_wiki_planning` **`219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`**, current `codex/dpo-implementation` **`4945b8e85c17b5f150eb5b840d16bbdc4b506242`**. Это **документарный, source-pinned audit**; новых .NET training/learning результатов не запускали. Исторические результаты — не fresh current measurements.

## Какой конкретно ответ даёт этот пакет

1. [**01 — Gold источники и рецепты/ошибки интерпретации**](01-BASELINE-RECEIPTS-AND-CLAIMS.md): архивный 50-pair preference accuracy 1.0, wiki-plan held-out DPO accuracy 0.98, synthetic source flag, 5-project evidence accuracy 4665/4665 — три **разных** измерения. Одинаковые blob SHA архивов в обеих ветках; повторный run ещё нужен.
2. [**02 — Новые адресные gold→current различия и условия домена**](02-NEW-DIFFS-AND-DOMAIN-SCOPE.md): **G-W01 новый lost `ClientServerBusinessFlow` route** (gold 15-step, current absent), **G-W05 условная label/enum restoration** (numeric-vs-named), **G-W06 невозможный confidence rejection после `Math.Clamp`**, **G-W08 новый отрицательный контроль — gold читает «without frontend» как наличие frontend**. Плюс **G-W02** routing gate/ordinary consumer — **независимая перепроверка ранее найденного** параллельными агентами, **G-W03** train vs deterministic query-plan gate присутствовал и в gold, **G-W04** archival receipt alias, **G-W07** PairId→project leakage был и в gold.
3. [**03 — 8 маленьких причинных опытов**](03-CAUSAL-MICRO-EXPERIMENTS.md), приоритет **E-W00–04**, затем label remap и confidence, далее source donors E09/E26/E43 в их доменах; каждый имеет A/B, negative controls, causal falsifier, expected consumer, самостоятельный blind quality gate, cold restore. Эти опыты `NOT_RUN`.
4. [**04 — готовое задание роли исполнителя и независимому reviewer**](04-EXECUTOR-AND-REVIEW-WORK-ORDER.md) с конкретными входами/выходами, скилами architect/planning/plan-review/code-review/prompt/human, ограничением на protected path.
5. [**05 — 19 source-predicted cases**](05-CROSS-STACK-FIXTURE.json) для client→API→server бизнес-процессов и отрицательных контролей (включая «without a frontend»). Метки только `SOURCE_PREDICTED`, не `NATIVE_VERIFIED` и не оценки качества.

\n6. [**06 — исполненный pinned source-only readback**](06-SOURCE-READBACK-RECEIPT.json): exact blob SHA gold/current, наличие 15-ступенчатого intent, отсутствие в current, safety gate + clamp + ordinal-order risk. Это **реальная проверка исходников**, но **не native .NET / не проверка качества модели**.\n\n## Не конкурировать с другими агентами за один и тот же claim

На момент сравнения другие агенты **уже создали** два дополнительных пакета по тому же gold donor:

- [**49-DONOR-DELTA**](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/README.md) — **ключевой source-confirmed** перенос содержательных prompt/chosen/rejected в **synthetic rejected-type token** для Wiki Request Router/CodeTrace; аналитический projection на исторических pairs; publication/serving gate. **Использовать как владельца N49-01/N49-02/N49-03**; наш G-W02 независимый cross-check, **не новая гипотеза**.
- [**49-DONOR-CAUSAL-RECOVERY**](../49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/README.md) — D49-01..04 + N49-05/06, трассировка отдельных рецептов и кардинальности пар, unit consumers и маленькие domain опыты. **Использовать как отдельного владельца этих findings**, не переписывать их под наш номер.
- [**core docs/plans/replanning-possible-diffs**](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs) — **только чтение**; особенно core/76 и /80 про более ранние WikiTrace и несовпадение preference vs user answer.

**Новое относительно соседних 49-пакетов:** адресно проверены именно потерянные `ClientServerBusinessFlow` intent+15-step+answer checks, compatibility при named/ordinal model restoration, арифметика confidence и отрицательный контрпример с `without frontend`. Их **не следует смешивать** с предыдущим DPO pair→signal token loss — это разные места цепи. Возможны новые найденные другими агентами совпадения; при поступлении обновлять ownership, а не объявлять доказательства собственностью пакета.

## Одна картина обучения и принятия

```text
human task + domain/profile
   |
   v
source DPO pairs / real examples -----> schema & provenance check
   |                                     (NOT defect-type-only)
   v
semantic labels / tokenizer / graph / LoRA / learned KNN
   |                   |
   |          model or memory actually changes
   v                   v
training recipient -- saved checkpoint -- cold load
   |                                       |
   +--------------> actual ordinary consumer <---- ordinary user question
                                      |
                      truthful answer + grounded citations
                                      |
                       blinded independent semantic evaluation
```

**Границы:**
- Wiki Gold **не эквивалентен** E09 C# KNN, E26 POS translation, E43 replay bigram, хотя они ценны и должны проверяться в своих доменах.
- Детерминистический планировщик/graph index может быть правильным и полезным, даже если LoRA не изменилась. Но это отдельный тип способности, не основание говорить «LoRA обучилась».
- Результат DPO 98% или наличие 4665 точных hits не означает, что свободный ответ уже удовлетворяет пользователю.
- Формула confidence может быть некорректной, но это ещё не установленная причина падения RP01 translation.
- **Никаких изменений** `llm_mvp_core/docs/plans/replanning-possible-diffs` ни напрямую, ни через PR в этом задании.

**Human-readable verdict:** `SOURCE_DIFFS_FOUND`; `GOLD_REPLAY_NOT_RUN`; `CAUSAL_QUALITY_GAIN_NOT_ESTABLISHED`; готовые минимальные опыты опубликованы.
