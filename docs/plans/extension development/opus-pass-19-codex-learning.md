# Pass P19 — Codex: обучаемость (learnability) + обновление локатора (locator update)

Честный фрейм: этот проход оценивает РЕАЛЬНОЕ состояние Codex-специфичной
«локальной обучаемости» (adaptive learning / export / import / backup /
continue-training / online write-back) и «обновления локатора» (target/rollback
locator, promotion, IRemoteUnitLocator) по ЖИВОМУ коду и артефактам, а не по
copy-статусу пакета. Уровни зрелости: COPIED_DOC_ONLY != BUILT_NOT_PROVEN !=
PROVEN. Каждая строка привязана к file:line или к явному «not found (searched
X)».

Проверенные источники (bounded reads):
- Codex host-репо `C:\Projects\codex-xiigen-smart-skills`:
  `src/codex-entrypoint.js` (learnability/locator-функции), `package.json`,
  `.codex/xiigen-smart-skills.json` (skeleton-манифест), `.codex-plugin/plugin.json`,
  `skills/xiigen-smart-skills/SKILL.md`, `scripts/`, `prompts/`, весь `reports/`
  (grep learnability/locator/adaptive/checkpoint/promotion/rollback/continue/
  export/import/backup), `RUNTIME-04-codex-live-runtime` mirror,
  `HOST-IMPL-CODEX-01/component-qa/**` core-request/response.
- Core `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`
  (`ResolveRequestLoadState:279-317`, `WithTargetLocatorRefs:320-344`,
  `NormalizeTargetLocatorMode:346-349`, non-claim строки `:664/:796/:818`).

## Что установлено (кратко, человеческим языком)

1. **У Codex НЕТ ни одной поверхности локального обучения / жизненного цикла
   модели.** Поставляемые `scripts` = только `install-smoke` (нет
   export/import/backup/continue-training); в `skills/` — один smoke-SKILL, чей
   own front-matter прямо пишет «does not prove ... learnability, training, DPO,
   checkpoint mutation» (`SKILL.md:3`), а его список booleans помечает
   `learnability_proven`, `training_started`, `checkpoint_mutation_started` как
   «Only the first two may be true» (`SKILL.md:42,:47`). Манифесты декларируют
   `host_training_allowed:false`, `modelTrainingInHost:false`,
   `dpoGenerationInHost:false`, `learnedModelMutationInHost:false`
   (`.codex/xiigen-smart-skills.json:45`, `package.json:52-54`). Рантайм Codex
   read-only: `invokeXiigenCore` возвращает `learnedModelMutationInHost:false`
   (`src/codex-entrypoint.js:380,:467`). В `src/codex-entrypoint.js` НЕТ
   lifecycle/feedback/locator-update подкоманд — только
   `createXiigenExtensionRequest`/`invokeXiigenCore`/debug-дисплеи.

2. **Adaptive-состояние Codex задекларировано символической строкой ВНЕ
   пакета**, а не реальным путём: `.codex/xiigen-smart-skills.json:39`
   `adaptive_state_root_ref:"codex-host:adaptive-state-runtime-root-outside-package"`,
   `:41 adaptive_state_separated_from_host_package:true`. То есть у Codex-репо
   нет ни `.xiigen`, ни adaptive-checkpoint, ни continue-probe (find в репо → 0).

3. **Вся locator-машинерия Codex мертва на поставляемом пути.** Хотя
   `src/codex-entrypoint.js:268-318 createTargetLocator` строит объект
   `TargetLocator` (user-adaptive root, rollback root, locator/target/rollback
   candidate id, `LocatorMode` default `'target'`), КАЖДЫЙ поставляемый
   Codex-запрос отправляет `TargetLocator: null` /
   `target_locator_present:"false"`
   (`reports/HOST-IMPL-CODEX-01/component-qa/20260617-235258/prompt-to-task-debug-off/xiigen-codex-core-request.json:10,:38`).
   В отличие от Claude (P09), где манифест жёстко вшивает test-фикстуру
   `qsg28-two-unit-test-locator.json`, у Codex НЕТ ни одной поставляемой
   locator-фикстуры (find *locator* в репо → 0). Локатор из Codex не
   упражняется вообще.

4. **«Обновление локатора» на границе ядра — это закрытый non-claim, а не
   реализованная возможность.** Поле запроса `TargetLocator` в ядре
   (`XiigenExtensionApiService.ResolveRequestLoadState:279-317`) используется
   ТОЛЬКО чтобы выбрать, из какого каталога ЗАГРУЖАТЬ ассеты (user-adaptive vs
   rollback root → `LoadCommonThenUserAdaptiveAssets`). Никакой регистрации в
   `IRemoteUnitLocator`, promotion или мутации реестра нет. Граница сама
   печатает `"no training, checkpoint promotion, or locator update performed"`
   (`:818`) и `"...locator update claims closed"` (`:664`),
   `"no checkpoint promotion"` (`:796`). То есть слово «locator» в
   extension-контракте означает «грузи из этой папки», а не «обнови реестр
   юнитов».

5. **Rollback-нога локатора Codex = смена каталога, не проверенный откат.**
   `src/codex-entrypoint.js:285-289` выводит `RollbackAssetRoot` через
   `inferAssetRoot(rollbackRefs)` (`:211-220` — режет первый строковый ref по
   `/(prompt-to-task|orchestration-reasoning|skill-selection)/`); ядро в
   rollback-режиме просто грузит ассеты из rollback-root (`:287-289`). Ни одного
   исполненного rollback/load-probe в Codex-репо нет (find *rollback* → 0), а
   live-mirror RUNTIME-04 прямо декларирует
   `production_locator_or_checkpoint_mutated:false` (mirror.json:15) и
   `rollback ... не доказаны` (report-ru:21).

## Коррекция-нюанс к P11 gap-6 / P18 (не новая дыра, а частичная правка)

P11 (Claude, старый probe) и P18 фиксировали сломанную загрузку checkpoint
OrchestrationReasoning (`ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH`).
На БОЛЕЕ НОВОМ Codex-probe (`HOST-IMPL-CODEX-01/component-qa/20260617-235258`,
2026-06-17) OrchestrationReasoning грузится БЕЗ mismatch-ошибки: есть
`orchestration_reasoning_checkpoint_ref` +
`orchestration_reasoning_checkpoint_sha256:dcd7d02b...`, warning про mismatch
отсутствует, а машина отдаёт `baseline_internal_semantic_plan_produced`. Старый
`HOST-IMPL-03-probe` (2026-06-16 и ранее) всё ещё несёт mismatch. Значит на
Codex-пути load-failure был частично починен между probe-датами — это правка
предшествующей дыры, а не отдельная новая (в newGaps не заношу). `semantic
QualityClaimed=false` при этом остаётся, поэтому learnability по-прежнему не
доказана.

## НОВЫЕ дыры P19 (Codex learnability/locator)

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| Codex-host жизненный цикл обучения (export/import/backup/share/continue-training) + online write-back | MISSING WIRING / declared-out-of-package / read-only | `codex-xiigen-smart-skills/package.json:8-11` scripts={source-wiring-check,install-smoke}, `:52-54` modelTraining/dpoGeneration/learnedModelMutation InHost=false; `skills/xiigen-smart-skills/SKILL.md:3,:42,:47` («does not prove learnability/training/checkpoint mutation»; only first two booleans true); `.codex/xiigen-smart-skills.json:39,:41,:45` adaptive_state «outside-package», separated, host_training_allowed:false; `src/codex-entrypoint.js:380,:467` learnedModelMutationInHost:false; в src нет export/import/continue/feedback-функций; в репо нет `.xiigen`/adaptive-checkpoint/continue-probe (find→0) | Codex-аналог Claude P11, но со СВОЕЙ декларацией: adaptive-root — символическая строка вне пакета, нет ни одной lifecycle-команды/скилла/скрипта, рантайм read-only, own SKILL.md отрицает learnability. Пользователь Codex не может backup/export/import/continue-train локальную модель; online-сигнала нет. | Ввести в границу расширения lifecycle-методы (Phase08), выставить Codex-командой/скиллом; задать реальный per-user `.xiigen` adaptive-root вместо символической строки; доказать export→import→continue-train round-trip через Codex с числовыми before/after; убрать `learnedModelMutationInHost:false` из success-пути после реального write-back. |
| Locator-машинерия Codex мертва на поставляемом пути: все запросы `TargetLocator:null`, нет поставляемой locator-фикстуры | DEAD LOCATOR PATH / never-exercised | `src/codex-entrypoint.js:268-318` `createTargetLocator` строит объект, но `reports/HOST-IMPL-CODEX-01/component-qa/20260617-235258/*/xiigen-codex-core-request.json:10` `target_locator_present:"false"`, `:38 TargetLocator:null` (во всех методах); find *locator*-фикстуры в репо → 0 (контраст Claude qsg28-фикстура, P09) | Отдельно от P17 gap-8 (дефект inferAssetRoot-конфляции в КОДЕ entrypoint): здесь РАНТАЙМ показывает, что locator из Codex не заполняется никогда — вся ветка resolve/rollback/user-adaptive load из Codex недостижима; Codex не поставляет ни одной locator-фикстуры, поэтому `target/rollback/user-adaptive` root-выбор не упражняется. | Поставить реальную Codex locator-фикстуру (per-user adaptive + rollback root после реального promotion); прогнать Codex-запрос с непустым `TargetLocator`; доказать resolve-after-promote и rollback-load из Codex; добавить locator-непустоту в Codex-QA как гейт. |
| «Locator update» на extension-границе ядра = закрытый non-claim; `TargetLocator` лишь выбирает load-root, не трогает `IRemoteUnitLocator` | NAMING-VS-CAPABILITY / not-implemented (all hosts) | `XiigenExtensionApiService.cs:279-317` `ResolveRequestLoadState` (locator → выбор каталога user-adaptive/rollback для `LoadCommonThenUserAdaptiveAssets`), `:320-344` только пишет refs в CheckSetRefs; `:664` «locator update claims closed», `:796` «no checkpoint promotion», `:818` «no training, checkpoint promotion, or locator update performed»; grep `RegisterAsync`/`IRemoteUnitLocator` в сервисе → 0 | Отдельно от P05 (реестр `IRemoteUnitLocator` не персистентен — транспорт): здесь суть в том, что поле `TargetLocator` extension-контракта вообще НЕ является механизмом обновления реестра — это switch «грузи из этой папки», и граница сама декларирует locator update закрытым non-claim. «Обновление локатора» как продуктовая возможность на любом (в т.ч. Codex) extension-пути отсутствует by-design. | Продуктовое решение: либо реализовать реальный locator-update-путь (после promotion регистрировать unitId в durable `IRemoteUnitLocator`, доказать locate-after-register), либо переименовать поле/убрать слово «locator update» из lifecycle-описания расширений, честно назвав его «adaptive/rollback load-root selector». |
| Rollback-нога locatorа Codex = смена каталога через `inferAssetRoot`, никогда не заполнена, без верификации отката, self-declared unproven | STUB ROLLBACK / no verified rollback probe | `src/codex-entrypoint.js:285-289` `RollbackAssetRoot ?? inferAssetRoot(rollbackRefs)`, `:211-220 inferAssetRoot`; ядро `XiigenExtensionApiService.cs:287-289` rollback-режим = load из rollback-root; `reports/RUNTIME-04-codex-live-runtime/...mirror.json:15` `production_locator_or_checkpoint_mutated:false`, report-ru:21 «rollback ... не доказаны»; find *rollback* в Codex-репо → 0 | Отдельно от P17 gap-8 (user-adaptive root конфляция): здесь именно ROLLBACK-нога — Codex строит rollback-locator эвристикой, но (а) он всегда null на поставляемом пути, (б) нет rollback-фикстуры, (в) семантически это лишь swap каталога, не откат модели с верификацией, (г) сам live-run декларирует rollback недоказанным. | Задать реальный rollback-checkpoint + фикстуру; выполнить rollback-load probe из Codex с before/after проверкой, что загружена именно откатная версия; не выдавать directory-swap за model rollback; зафиксировать verified-rollback evidence. |

## Итог P19

Codex-специфичная обучаемость и обновление локатора на РЕАЛЬНОМ коде/артефактах
= отсутствуют на уровне host-поверхности и являются закрытыми non-claim на
уровне ядра. 4 новые дыры добавлены в мастер-реестр. Ни одна не дублирует
P05 (core transport), P11 (Claude lifecycle), P17 (Codex entrypoint inferAssetRoot
user-root конфляция), P18 (Codex pipeline). Проект по этому слайсу — не PROVEN и
не BUILT: host-обучаемость MISSING, locator-путь DEAD/never-exercised,
locator-update = закрытый non-claim.
