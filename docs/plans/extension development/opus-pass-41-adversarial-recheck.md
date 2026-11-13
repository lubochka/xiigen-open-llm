# Pass P41 — Adversarial recheck of prior gaps + blind-spot hunt

Дата: 2026-07-08. Роль: Opus 4.8 REAL-STATE ASSESSOR. Метод: прочитан весь
накопительный реестр `opus-PROJECT-CLOSURE-PLAN.md` (P01…P40); затем выбрана
выборка НЕСУЩИХ (load-bearing) прежних дыр и коррекций, каждая перепроверена
против ЖИВОГО кода `C:\Projects\llm_mvp_core` bounded-инструментами
(grep/glob/targeted read, без полного чтения крупных файлов). Цель прохода:
подтвердить/опровергнуть прежние утверждения и найти слепые зоны, которых не
касался ни один из 40 проходов. Ниже — сперва результаты рекчека
(подтверждения/опровержения), затем 2 НОВЫЕ дыры (слепые зоны).

## Часть 1 — Adversarial recheck прежних дыр (confirm/refute)

Все проверки — против текущего дерева (working tree на ветке
`DesignToCode-clean`).

| Прежняя дыра | Что перепроверял | Вердикт | Доказательство |
|---|---|---|---|
| P10 gap-1 «semantic_quality_claimed — dead-gate; ни одна из 3 машин не эмитит `SEMANTIC_QUALITY_CLAIMED_TRUE`» | grep токена по всему репо, проверка присутствия в файлах машин | **CONFIRMED** | `SEMANTIC_QUALITY_CLAIMED_TRUE` найден в 17 файлах = только Tests/tools/docs/artifacts + сам `XiigenExtensionApiService.cs` (const+проверка). НЕТ ни в `PromptToTaskMachine.cs`, ни в `OrchestrationReasoningMachine.cs`, ни в `SkillSelectionMachine.cs`. Гейт по-прежнему недостижим по построению. |
| P26 gap-1 / P05 «потребление promoted-checkpoint минует весь 25.ModelServer; 0 хитов IModelRepository/IRemoteUnitLocator в 43» | `grep -E "IModelRepository\|IRemoteUnitLocator\|UploadAsync\|LocateAsync\|LoadWeightsAsync\|index.json"` в `43.SmartSkillsDomain` | **CONFIRMED** | No files found — 0 хитов. Слой model-server по-прежнему обойдён и на записи, и на чтении. |
| P02 gap-4 (order-leakage authority) → P23 КОРРЕКЦИЯ «`CandidateOrderLeakageGuardUnit` реализован; `_candidatePositionWeights` низведён до diagnostics-only, fallback возвращает пустой выбор» | чтение `DecideFromTrainedState:107-143` + грепы юнита | **REFUTATION VALID (P02 gap-4 закрыт кодом)** | `SkillSelectionMachine.cs:113-142`: fallback-ветка возвращает `SelectedSkillIds:[]`, `Confidence:0.0`, `RequiresSecondPass:true`, DecisionText «Candidate-position weights are diagnostics and leakage-guard evidence only; final semantic skill selection requires the SS-02..SS-12 semantic pipeline.»; `CandidateOrderLeakageGuardUnit` реально существует в `SkillSelectionLogicalHierarchy.cs:271` и подключён `:146`. Позиционный счётчик БОЛЬШЕ НЕ авторитет выбора. P23-коррекция верна; повторно дыру не открываю. |
| P11 gap-2 / P14 gap-1 «метод `feedback` не обрабатывается сервисом» | grep `feedback` в `XiigenExtensionApiService.cs` | **CONFIRMED** | No matches — сервис не имеет ни одной ветки обработки `feedback`; единственный online-сигнал adaptive DPO по-прежнему падает в дефолт task/plan/skills-цепочку. |
| P40 gap-1 «extension-исходники ядра — незакоммиченное грязное дерево (331 files, +18574/−3576, 545 untracked)» | `git diff --shortstat` + `git status --porcelain` | **CONFIRMED (без изменений)** | 331 files changed, 18574 insertions(+), 3576 deletions(-); 545 untracked; 876 porcelain-записей. Состояние идентично P40; продукт по-прежнему не сохранён в коммитах. |
| P02/P21 anchor «`PromptToTaskMachine.cs:90` `NotReadyTaskType="not_ready_no_semantic_task"`» | точечное чтение `:86-95` | **CONFIRMED (anchor точен)** | `PromptToTaskMachine.cs:90` = `public const string NotReadyTaskType = "not_ready_no_semantic_task";`. |
| P20 gap-5 «утечка секрета в закоммиченном evidence (`contains_secrets:true`)» — проверка на СИСТЕМНОСТЬ | grep `"contains_secrets":true`/`"raw_private_prompt_persisted":true` по `codex-xiigen-smart-skills` | **CONFIRMED, НО НЕ систем­но (bounded)** | Найден ровно 1 файл — `reports/RUNTIME-04G-codex/20260616-235353/runtime04g-...-summary.json` (тот же, что назвал P20). Систем­ной утечки по репо НЕТ ⇒ НОВОЙ дыры не завожу, честно ограничиваю прежним единичным случаем. |

Итог части 1: 6 из 6 проверенных ДЫР подтверждены как всё ещё реальные; 1
проверенная КОРРЕКЦИЯ (P23 закрытие P02 gap-4) подтверждена как валидная —
позиционный счётчик действительно больше не авторитет. Оверклеймов в прежних
проходах по этой выборке не обнаружено; anchor’ы (file:line) точны. Прежний
единичный secret-leak (P20) корректно ограничен одним файлом — раздувать в
«системный» нельзя.

## Часть 2 — Слепые зоны (НОВЫЕ дыры)

Ни один из 40 проходов не проверял два инфраструктурных факта: (а) СТОИМОСТЬ
и природу единственного реального транспорта core-API как исполнения; (б)
членство ключевых проектов-делайверблов в файле решения `LlmFromScratch.sln`.

### G41-1 — Транспорт core-API компилирует ВЕСЬ проект `43.SmartSkillsDomain` через `dotnet run` на КАЖДЫЙ host-запрос

`tools/xiigen-extension-core-api.ps1` — это единственный НЕ-синтетический
транспорт, через который все 3 host-расширения (Claude/Cursor/Codex по P08/P09)
доходят до скомпилированного ядра. Прочитан слайс `:105-189`:

- `:111-131` на каждый вызов создаётся временный `XiigenExtensionCoreApiInvoker.csproj`
  с `<ProjectReference Include="…43.SmartSkillsDomain\43.SmartSkillsDomain.csproj"/>`;
- `:133-181` пишется временный `Program.cs`, десериализующий запрос и зовущий
  `new XiigenExtensionApiService(coreRoot).ProcessHostExtensionRequest(request)`;
- `:184-185` `& dotnet run --project <temp.csproj> -- <req> <resp> <coreRoot>`.

Следствия (не покрыто ни одним gap-row прежних проходов; P09 упомянул симптом
«компиляцией на каждый вызов» ТОЛЬКО в прозе-контексте, без gap-строки и без
closure-шагов):

1. КАЖДЫЙ prompt→answer запускает полный `dotnet run` = restore+build+run всего
   `43.SmartSkillsDomain` заново (cold-start латентность секунды–десятки секунд
   на запрос), а не обращение к запущенному сервису.
2. Требует установленного .NET **SDK** (не только runtime) и полного дерева
   исходников `43.SmartSkillsDomain` на машине конечного пользователя — это не
   поставляемый бинарь.
3. Нет ни одного тёплого/persistent сервис-процесса; архитектурно это
   «скомпилируй-и-запусти по требованию», непригодное для продакшена/латентности.

Отдельно от P08 gap-1 (hardcoded absolute-path PS-shim, deployability по пути) и
P05/P06 (synthetic no-send — ДРУГАЯ ветка): здесь дефект — сама природа
исполнения (per-call compile), с распинной closure (persistent pre-built service
process). Closure: заменить per-request `dotnet run` на предсобранный
долгоживущий сервис-процесс (stdio/HTTP), принимающий request/response без
компиляции; требовать только runtime, не SDK; замерить cold-vs-warm латентность.

### G41-2 — Ядро расширения `43.SmartSkillsDomain` и домен `44.DesignConversionDomain` НЕ являются first-class проектами в `LlmFromScratch.sln`

`LlmFromScratch.sln` содержит 65 `Project(...)`-записей / 41 ссылку на `.csproj`.
Проверено grep-ом по файлу решения:

- `43.SmartSkillsDomain.Dashboard\43.SmartSkillsDomain.Dashboard.csproj` — ЕСТЬ
  как Project() (`LlmFromScratch.sln:134`);
- `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj` (само ядро расширения:
  `XiigenExtensionApiService` + 3 машины) — как Project() **ОТСУТСТВУЕТ**
  (`grep -F` exit 1); файл существует на диске (`43.SmartSkillsDomain/43.SmartSkillsDomain.csproj`, git-tracked);
- `44.DesignConversionDomain\44.DesignConversionDomain.csproj` — как Project()
  тоже **ОТСУТСТВУЕТ** (exit 1).

Честная граница (не переоцениваю в минус): оба проекта ВСЁ РАВНО собираются
ТРАНЗИТИВНО, потому что первоклассный `LlmFromScratch.Tests` (в sln,
`Project(... "LlmFromScratch.Tests")`) их референсит:
`Tests/LlmFromScratch.Tests.csproj:60` → `43.SmartSkillsDomain.csproj`,
`:62` → `44.DesignConversionDomain.csproj`; плюс `25.ModelServer.csproj:10` и
`43.SmartSkillsDomain.Dashboard.csproj:5` референсят их же. Значит `dotnet build`
решения их скомпилирует как зависимости — это НЕ «никогда не собирается».

Но дыра реальна и НОВАЯ (P39 — содержание тестов/нет full-.trx; P40 — git-коммит
статус/worktrees; членство в .sln не касался никто):

- Первоклассный проект-делайвербл (ядро расширения со всей семантикой) не
  зарегистрирован в решении, тогда как его потребитель `…Dashboard` —
  зарегистрирован (асимметрия);
- IDE/навигация: ядро и DesignConversion не видны как проекты решения; выборочная
  сборка/CI по имени проекта усложнена;
- governance/hygiene: основной продукт собирается лишь как побочная транзитивная
  зависимость тестов/дашборда, а не как явный член решения.

Closure: добавить `43.SmartSkillsDomain` и `44.DesignConversionDomain` как
first-class `Project()`-записи в `LlmFromScratch.sln` (с их GUID и build-config);
тест паритета «каждый git-tracked продуктовый .csproj присутствует в sln»;
согласовать с G-коммитом дельты (P40) — оба вопроса относятся к build-graph
целостности делайвербла.

## Сводная таблица НОВЫХ дыр P41

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| Транспорт core-API как исполнение (per-call compile) | STUB TRANSPORT / per-request full compile, SDK-required, no warm process | `tools/xiigen-extension-core-api.ps1:111-131` (temp csproj + ProjectReference на `43.SmartSkillsDomain.csproj`), `:133-181` (temp Program.cs), `:184-185` `dotnet run --project <temp>` | Единственный реальный транспорт компилирует весь `43.SmartSkillsDomain` через `dotnet run` на КАЖДЫЙ prompt→answer: cold-start латентность, требует .NET SDK + дерево исходников у пользователя, нет persistent-сервиса. Не поставляемо/не продакшен. P09 упомянул симптом лишь в прозе, без gap-строки. Отдельно от P08 (absolute-path shim) и P05/P06 (synthetic no-send). | Заменить per-request `dotnet run` предсобранным долгоживущим сервис-процессом (stdio/HTTP) без компиляции на запрос; требовать только runtime; замерить cold-vs-warm латентность; убрать генерацию temp-csproj. |
| Членство проектов-делайверблов в `LlmFromScratch.sln` | BUILD-GRAPH DEFECT / core project not a first-class sln member (transitive-only) | `LlmFromScratch.sln:134` есть только `43.SmartSkillsDomain.Dashboard`; `43.SmartSkillsDomain.csproj` и `44.DesignConversionDomain.csproj` как `Project()` отсутствуют (grep exit 1); собираются транзитивно через `Tests/LlmFromScratch.Tests.csproj:60/:62`, `25.ModelServer.csproj:10`, `Dashboard.csproj:5` | Ядро расширения (`XiigenExtensionApiService`+3 машины) и весь `44.DesignConversionDomain` не зарегистрированы как first-class проекты решения, тогда как их потребитель `…Dashboard` — зарегистрирован (асимметрия). Собираются лишь транзитивно (НЕ «никогда»), но навигация/выборочная CI-сборка/governance-целостность build-graph нарушены. Ново vs P39 (содержание тестов) и P40 (git-коммит/worktrees). | Добавить `43.SmartSkillsDomain` и `44.DesignConversionDomain` как `Project()`-записи в sln с GUID/build-config; тест паритета «каждый продуктовый git-tracked .csproj присутствует в sln»; связать с коммитом дельты (P40 build-graph целостность). |
