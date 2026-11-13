# RP01 — общий контекст одного задания BATCH500

**15.10.2026 · B500-LIVE-RUN-REPAIR-20261015.** Передать этот блок, [dispatcher](RP01-EXECUTE-NOW.md), [current code map](RP01-CURRENT-CODE-MAP.md) и **одну** полную [карточку](tasks/batch500/README.md). Не загружать весь архив для каждого child. Действующий научно-операционный контракт: [BATCH500](RP01-OFFLINE-TEACHER-BATCH500.md).

## Сначала режим и фактический остаток работы

**CURRENT_FROZEN:** продолжить/reconcile уже выданный Main500. Существуют offline runner, C#500, calibration, C0 и exact F1/F2 repair review. Сначала переиспользовать совпадающие receipts. Не создавать новый trainer или source parser лишь потому, что старый план предполагал их отсутствие. Текущая CLI: exact500, fixed B0, one epoch, continuous reference/optimizer внутри запуска и no-replay marker. Ни first50 streaming, ни generic resume/descendant/ENRU не становятся API от записи в плане.

Frozen25 preference+3 free subset, decoder/tolerance1e-6 и wall none сохраняются. Не добавлять DEV10/40+20/Q_law как новые обязательства, не повторять C0/calibration ради task ID. Old one-pair0/12→0/12 и его исходный порог тоже не переписывать.

**NEXT_CAMPAIGN:** новый ещё не начатый domain/data/algorithm experiment. Здесь до outcomes можно выбрать другие splits, semantic rubric, расписание и budget. Только конкретно отсутствующий source/domain/parent стык получает bounded implementation.500pairs — исследовательский бюджет, не гарантированный gain. Нет обязанности улучшаться после каждой пары.

## Роли и честное происхождение

| Роль | Действительный исполнитель |
|---|---|
| S — делегированный человеческий выбор урока | **CODEX с `astra 6 ultracode`**, не перекладывать этот выбор вручную на пользователя |
| A — обе DPO стороны нового dev урока | Отдельная Astra симулирует роль внешнего китайского автора; сохраняет реальное авторство, raw output и receipt |
| J — оценка exact пары | **Отдельный CODEX с `astra 6 ultracode`**, не автор A |
| Student/trainer | Настоящая локальная XIIGEN, существующий разрешённый state owner |
| E — оценка actual Student outputs | Независимое назначение CODEX с `astra 6 ultracode`; blind score сохраняется до unblind |

Requested model не равна подтверждённому вызову. Если назначения не было — NOT_CALLED; actual identity не подтверждена — NOT_ATTESTED. Разные сессии одной модели не устраняют все общие ошибки, поэтому доменные источники/oracles важны. Не писать себе фиктивный independent PASS.

Current500 имеют действительное `simulation` provenance и программную материализацию25templates×20. Это **не** alias прямого ASTRA_SYNTHETIC_DEV model output и не500 вызовов. Не менять frozen bytes/source/J/hashes. Для будущего direct Astra authoring использовать честный source и реально поддержанный admission, без подмены Chinese grant. Target approved external open Chinese provider остаётся отдельным поздним BOTH-sides author; его нет в current local author/evaluator loop. Источник данных и право training effect не выводятся из названия папки.

## Полный dispatch block для менее сильного исполнителя

```text
CURRENT TASK: один B500 primary/child и наблюдаемый output.
NOT THE TASK: соседние задачи и конкретные запрещённые эффекты.
MODE: CURRENT_FROZEN или NEXT_CAMPAIGN, с evidence почему.
INPUTS: actual refs/hashes, owner handoff, immutable protocol, доступные receipts.
READ: общий блок + полный выбранный child + нужные source excerpts.
SKILLS: sections и конкретное применяемое решение, не только names.
CHANGE: exact owned files/state; reuse либо один отсутствующий стык.
OBSERVE: good/bad/edge из карточки, falsifier, limitation результата.
STOP THIS EFFECT: unknown ownership, invalid material/state/transaction.
RETURN: один packet ниже; одна следующая bounded задача.
```

Пять primary cards содержат **14 inline children**:01S/A/J;02A/B;03P/T/R;04F/D/E;05A/B/C. Это разделение контекста/эффектов, не14 дополнительных обязательных запусков или ревью. S/A/J/E сохраняют разделение назначений и информации; administrative children допускают совместное исполнение в пределах прав. При действительно большом child разделить по causal/file ownership и каждому передать те же поля; не создавать READ_ALL_REPO барьер.

## Точные навигационные anchors

P=`lubochka/xiigen-planning:llm_plan3`, K=`lubochka/llm_mvp_core:codex/dpo-implementation`. D=`llm mvp/design to code session`, F=`llm mvp/llm dynamic flow`.

В D использовать точные имена: `original task.md`; `09.09.26WorkLeft-RAG-MASTER-INDEX.md`; `09.09.26WorkLeft-02-teaching-chinese-model.md`; `09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md`; `replanning/results/R03/SOURCES.csv`. Далее RAG→один относящийся primary→actual caller→наблюдение. Корни других RAG брать из их source map, **не придумывать пути G/V по сокращению**. Индекс не считается чтением всех primaries. Previous rounds, JOIN и R01–R25 используются адресно, их статусы не переобъявлять закрытыми.

`PLAN-DOTNET-LEGACY-DPO-v1.md` — историческая ссылка, не автоматически текущий API/порог. Конкретное решение переносится только после сопоставления actual algorithm, representation/data/consumer. Отсутствующее чтение не выдавать за compatibility review.

K/docs/skills: architect-behavior-classifier + phase-planning для scope; plan-review для полноты задания; generated-code-review для real path; prompt-patch-authoring для literal good/bad; human-readable-architecture-audit для понятного результата. Domain/hybrid owner адресно использует trainable-architecture-decision/code-examination. Не переносить старые generic30/50-pass или test-count quotas.

## Минимальные safeguards и empirical examination

**Docs/read/reuse:0 новых product tests.** При изменённом deterministic риске — ближайшее имеющееся доказательство exact material/masks, finite allowed tensor effect, source rights, transaction/cursor/reload, no replay и no unintended serving. Проверять только затронутое, не весь список каждый раз. Не удалять guards и не менять .github/workflows. Windows matrix/coverage percentage не измеряет качество обучения.

Способность проверяется предметными задачами: domain/intent/version mapping; смысл representation; законный retrieval/consumer; предпочтения и собственные ответы; перенос/retention; качество за фактическое время и полный расход ресурсов. Compiler/oracle может подтвердить предметный факт, но его pass count не доказывает, что Student выучил его.

Один mutable-state owner. Research checkpoint не требует production promotion каждого shard, но требует настоящих state/effect rights. При unknown эффекте — journal owner, не reset/новый ID. S/A/J не получают closed evaluation gold; E не переписывает Student outputs.

## Один packet на выходе

```yaml
task: primary/child
mode_and_protocol: CURRENT_FROZEN или NEXT_CAMPAIGN + immutable ID
sources: repo/ref/path/sections и skills actually applied
inputs: data/state/protocol hashes, actual owner, reused receipts
change: reused / exact changed files и producer-to-consumer
actors: S/A/J/E requested/actual receipt либо NOT_CALLED
counts: authored / accepted_unique / completed_unique / templates / independent_families / tokens / committed_updates
state: parent/candidate/reference/optimizer/RNG/cursor/readback
learning: preference / free_behavior / retention / domains отдельно
cost: measured stages и nested timing; calls; process vs whole-PC; unknowns
integrity: изменённый риск и reused/new observation, не coverage percentage
outcome: observed / inconclusive / invalid / not_measured + limitations
publication: commit/readback либо pending
next_action: одна адресная задача с owner и необходимым входом
```

Код, обучаемое состояние, качество и adoption — разные утверждения. Эта документная редакция не является новым Student experiment или независимым Astra review. [Предыдущий общий блок](https://github.com/lubochka/xiigen-planning/blob/76657d964769914a88556237f28fc47e6d46da01/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-BATCH500-EXECUTOR-REPAIR-2026-10-15.md) сохраняет историю, не current defaults.
