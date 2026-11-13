# Pass P49 — Риски + открытые решения Luba + внешние зависимости (детали)

Честный фрейм. Этот проход НЕ ищет новые дефекты возможностей (их ~230 нашли P01–P48).
Он накладывает три недостающих управленческих слоя, без которых «PLAN до FULL
implementation» неполон: (1) **риск-реестр** (вероятность×воздействие×митигация×владелец×триггер);
(2) **внешние зависимости/действия** — то, что команда не может закрыть одним лишь кодом
(host-платформы вендоров, публикация в маркетплейсы, лицензия/ToS вендора-учителя, RCE-поверхность);
(3) **новые решения только-Luba**, которых НЕТ в списке P48 gap-3.

Метод (bounded reads реального кода, не копий пакета):
- Cursor host `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts` (spawn-блок `:652-694`),
  `package.json` (`:1-11` метаданные/лицензия/engines, отсутствие packaging-скриптов);
- транспорт `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1` (`param($CoreRoot):1-10`,
  генерация temp-проекта `:111-181`, `dotnet run :184`);
- Claude `C:\Projects\claude-xiigen-smart-skills\.claude-plugin\plugin.json` (`:4` version-skeleton,
  `:15` hook `node.exe "<абс. путь>"`);
- дедуп-скан всего мастера P01–P48 по `risk register|probability|mitigation|security|arbitrary code|
  injection|terms of service|ToS|marketplace|publish|vsce|UNLICENSED|installable`.

Честные положительные факты (не в минус):
- у Cursor engine-пин ЕСТЬ (`package.json:9-11` `engines.vscode ^1.90.0`);
- у Codex РЕАЛЬНАЯ marketplace-плагин-система вендора существует (P17), т.е. канал в принципе есть;
- транспорт использует `-NoProfile` и `windowsHide:true` (частичная гигиена запуска);
- redaction-самофлаги и privacy-границы проставлены (P30/P45) — но security-модели RCE это не покрывает.

Ниже — только НОВЫЕ дыры (6), не повторяющие P48 gap-3 (отсутствие сводного decision-ledger как
такового), P48 gap-4 (feasibility from-scratch движка), P26/P05 (публикация чекпоинтов через
25.ModelServer — это про модель, не про дистрибуцию расширения), P45 gap-2 (внутренний teacher-gate),
P30/P20 (divergent redaction / утечка секрета в evidence — не code-execution-вектор).

---

## G49-1 — Формального risk-реестра нет ни в пакете, ни в репо

- **Real-status:** MISSING ARTIFACT / нет риск-менеджмент-слоя.
- **Evidence:** grep по мастеру `risk register|probability|likelihood|mitigation|risk owner` → 0;
  единственный план действий `supporting-files/evidence-extension-full-readiness-dual-path/
  product-readiness-action-plan.md:213-224` §12 = список продуктовых пробелов, без полей
  вероятность/воздействие/митигация/владелец/триггер; P48 добавил ярусы приоритета (P0..P3, M0..M6),
  но приоритет ≠ риск (нет неопределённости/вероятности отказа/плана реагирования).
- **Gap:** «PLAN до FULL implementation» не несёт управляемого списка рисков: ни один узел
  критпути (from-scratch движок P48-4, тёплый транспорт, host-инвокация Codex, легальность
  distillation) не имеет записанной вероятности провала, воздействия на срок/scope, митигации,
  владельца и триггера эскалации. Без этого нельзя ни планировать буферы, ни решать, что делать
  при провале узла.
- **Closure steps:** Завести risk register (колонки: id / риск / вероятность / воздействие /
  экспозиция / митигация / контингенс-план / владелец / триггер эскалации / статус); засеять
  топ-рисками: R-01 неспособность from-scratch движка (P48-4), R-02 host-платформа вендора
  меняет/закрывает поверхность (G49-2), R-03 RCE-поверхность/провал security-review (G49-3),
  R-04 маркетплейс отклоняет публикацию (G49-4), R-05 ToS вендора запрещает distillation (G49-5);
  вести как живой артефакт, ревью гейтит на актуальность.

## G49-2 — Внешняя зависимость от host-API вендоров: нет compat-матрицы/мониторинга дрейфа; Codex-поверхность может быть недоступна извне

- **Real-status:** EXTERNAL DEPENDENCY / uncontrolled-contract-drift.
- **Evidence:** три поверхности принадлежат внешним вендорам и версионированы по-разному:
  Cursor `package.json:9-11` `engines.vscode ^1.90.0` (пин есть, но compat-теста/матрицы нет);
  Claude `.claude-plugin/plugin.json:4` `version 0.0.0-phase40-skeleton` (пина host-API нет);
  Codex `.codex/xiigen-smart-skills.json` `0.0.0-phase40-skeleton` (P17). Острейший случай — P17:
  `reports/HOST-08F-codex-plugin-app-qa.json` `codex_plugin_command_scope:
  "marketplace_management_only_from_help"`, `direct_local_plugin_app_invoke_path_found:false`,
  `CX-04R-host-runtime-path-report.json` `true_codex_host_runtime_path_found:false` — вендор
  может НЕ предоставлять пути инвокации локально-авторского плагина вовсе.
- **Gap:** продукт «три расширения» жёстко зависит от внешне-владеемых контрактов (VS Code/Cursor
  extension API, Claude plugin+hooks API, Codex plugin platform), но нет: (а) compat-матрицы
  «наша версия ↔ поддерживаемые версии host»; (б) теста против пиненой версии; (в) мониторинга
  changelog/deprecation вендоров; (г) контингенс-плана, если вендор изменит/удалит поверхность.
  Для Codex это уже реализованный внешний блокер (нет vendor-supported local-invocation) — один из
  трёх обязательных хостов может оказаться неосуществим как задумано без OpenAI-курации/одобрения.
  Отдельно от P17 gap-1 (там факт «плагин не зарегистрирован») — здесь РИСКОВЫЙ/зависимостный ракурс:
  неуправляемость внешнего контракта на всех трёх хостах + отсутствие мониторинга/контингенса.
- **Closure steps:** Для каждого хоста зафиксировать и протестировать минимальную поддерживаемую
  host-API версию; вести compat-матрицу; подписаться на changelog/deprecation вендоров и добавить
  drift-триггер в risk register; определить контингенс (что делать, если поверхность закрывается);
  эскалировать Luba внешнее ограничение Codex (принять альтернативную поддерживаемую поверхность
  или признать зависимость от OpenAI-маркетплейса) — связать с decision-ledger (P48 gap-3) и G49-6.

## G49-3 — Security: поверхность произвольного выполнения кода (RCE) в хост-адаптерах, нет threat-model/подписи/gating доверия воркспейса

- **Real-status:** SECURITY DEFECT / arbitrary-code-execution surface, no threat model.
- **Evidence:** Cursor `src/extension.ts:661-675` `spawn("powershell.exe", ["-NoProfile",
  "-ExecutionPolicy","Bypass","-File", coreApiScript, ..., "-CoreRoot", coreRoot])`, где
  `coreApiScript`/`coreRoot` берутся из настроек воркспейса (`xiigenSmartSkills.coreApiScriptPath`/
  `coreRoot`, дефолты — абс. dev-пути, P13/P46); вызываемый `tools/xiigen-extension-core-api.ps1`
  `param($CoreRoot):1-10`, `:114` `ProjectReference Include="$smartSkillsProject"` (из
  `$resolvedCoreRoot`), `:181` `Set-Content ... $programPath` (генерирует Program.cs), `:184`
  `dotnet run --project $projectPath` — т.е. КОМПИЛИРУЕТ и ЗАПУСКАЕТ C# из переданного `-CoreRoot`;
  Claude `.claude-plugin/plugin.json:15` на SessionStart запускает `node.exe "<абс. путь к
  claude-entrypoint.js>"` — тот же класс исполнения из манифеста. Нет ни подписи/интегрити-проверки
  скрипта/энтрипоинта, ни валидации пути, ни threat-model в артефактах (grep мастера
  `security|arbitrary code|injection|sandbox|threat` → только privacy-redaction P30 и утечка секрета
  P20, не code-execution-вектор).
- **Gap:** классический VS Code/Cursor вектор: путь скрипта/ядра управляется настройками воркспейса,
  а расширение исполняет по нему код с `-ExecutionPolicy Bypass` и компиляцией произвольного C# —
  открытие вредоносного воркспейса (со своим `.vscode/settings.json`) ведёт к выполнению кода без
  gating доверия воркспейса; нет security-review как гейта. Это не покрыто P30 (расхождение
  редакции) и P20 (секрет в закоммиченном evidence).
- **Closure steps:** Провести threat-model; гейтить исполнение через Workspace Trust; валидировать/
  вайтлистить путь скрипта, запретить произвольный `-CoreRoot`; убрать `-ExecutionPolicy Bypass`;
  подписывать/интегрити-проверять runner+entrypoint; убрать компиляцию-из-пользовательского-пути в
  рантайме (предсобранный подписанный бинарь/сервис — связка с warm-transport P41); ввести
  security-review как блокирующий гейт completion.

## G49-4 — Нет устанавливаемого/публикуемого артефакта расширения и канала дистрибуции (внешнее действие)

- **Real-status:** MISSING DELIVERABLE + EXTERNAL ACTION / not-packageable, not-publishable.
- **Evidence:** Cursor `package.json:5` `version "0.0.0-xiigen-wiring"`, `:6` `"private": true`,
  `:7` `"license": "UNLICENSED"`, `:8` `"publisher": "xiigen"`, `:12-13` `categories ["Other"]`;
  нет `repository`/`icon`/packaging-скриптов (grep `vsce|@vscode/vsce|ovsx` в package.json → 0);
  во всех трёх host-репо нет `LICENSE`/`CHANGELOG`/`.vscodeignore`/`icon` (ls → пусто);
  Claude/Codex манифесты `0.0.0-phase40-skeleton` с зашитыми абс. dev-путями
  (`.claude-plugin/plugin.json:15` `node.exe "C:\Projects\claude-xiigen-smart-skills\...`,
  `-CoreRoot "C:\Projects\llm_mvp_core"`). Мастер-grep `installable|распростран|vsce package|
  open vsx|UNLICENSED|publisher account|packaging` → 0 (совпадения «packaging packet»/
  «packaging-task coverage» — про evidence-пакеты, не про упаковку расширения).
- **Gap:** ни у одного из трёх расширений нет устанавливаемого артефакта (`.vsix`/плагин-пакет):
  `private:true`+`UNLICENSED` блокируют публикацию в маркетплейс; нет метаданных/иконки/лицензии/
  packaging-скрипта; абс. dev-пути делают манифесты непортируемыми. Нет и КАНАЛА публикации
  (VS Code Marketplace / Open VSX для Cursor; Anthropic-плагин-канал для Claude; OpenAI-маркетплейс
  для Codex) и внешних действий: аккаунт издателя, подпись, прохождение ревью маркетплейса. Отдельно
  от P05/P26 (публикация ЧЕКПОИНТОВ модели через 25.ModelServer) — здесь дистрибуция самого
  РАСШИРЕНИЯ конечному пользователю.
- **Closure steps:** Выбрать лицензию + канал дистрибуции (решение Luba, G49-6); добавить
  `repository`/`icon`/`CHANGELOG`/`LICENSE`/`.vscodeignore` и packaging-скрипт (`vsce package`/`ovsx`
  для Cursor; аналог для Claude/Codex); убрать зашитые абс. пути (портируемые дефолты/обнаружение);
  завести аккаунт(ы) издателя; пройти ревью маркетплейса; доказать установку из артефакта на чистой
  машине (Path B). Внешние шаги (аккаунт/ревью) внести как external-action items в risk register.

## G49-5 — Внешне-правовая зависимость: ToS вендора-учителя для distillation не прочитан/не записан (может запретить общее обучение на выводе модели)

- **Real-status:** EXTERNAL LEGAL DEPENDENCY / unresolved vendor-terms.
- **Evidence:** промоутнутый common-корпус построен из вывода вендорской модели Sonnet
  (P45 gap-2: `orchestration-reasoning-common-corpus-manifest.json:33/38/43`
  `source_ref:"sonnet-local-analyzes:*"`, `model_leg:"common"`); CLAUDE.md требует `terms_snapshot`
  для teacher/distillation, но grep мастера `terms of service|ToS|vendor terms|anthropic.*terms|
  openai.*terms|legal sign` → 0 — фактический ToS вендора нигде не прочитан/не зафиксирован.
- **Gap:** разрешает ли ToS Anthropic/OpenAI использовать вывод их моделей для обучения общей
  (потенциально конкурирующей) модели XIIGen — ВНЕШНИЙ юридический неизвестный, требующий чтения
  реальных условий вендора + юридического/Luba sign-off. Многие вендорские ToS прямо запрещают
  обучение конкурирующих моделей на выводе — это может обнулить весь distillation-value-prop, а не
  просто «не пройти внутренний гейт». Отдельно от P45 gap-2 (там — построить ВНУТРЕННИЙ teacher-gate/
  quarantine): здесь ВНЕШНЯЯ правовая зависимость + решение go/no-go.
- **Closure steps:** Прочитать и записать фактический `terms_snapshot` из актуального ToS каждого
  задействованного вендора; провести юридическое ревью; вынести Luba решение go/no-go по distillation;
  если запрещено — исключить vendor-derived строки из common и пересобрать корпус из permissive
  (MIT/Apache-2.0) источников; внести как external-legal-action в risk register (R-05).

## G49-6 — Новые решения только-Luba, отсутствующие в списке P48 gap-3 (дистрибуция/лицензия, security-trust, приём ToS, минимально-жизнеспособный набор хостов)

- **Real-status:** MISSING DECISIONS / вне сводного decision-ledger.
- **Evidence:** P48 gap-3 (`opus-PROJECT-CLOSURE-PLAN.md:665`) перечисляет 7 технических решений
  (движок инференса, remote-vs-local lifecycle, маршрут через оркестратор, мультипроектность,
  4-я host-семья, репо-топология, teacher/distillation), но НЕ содержит: (а) лицензия + канал
  дистрибуции расширения (G49-4; сейчас `package.json:6-7` `private:true`/`UNLICENSED`);
  (б) security/workspace-trust модель и кто вправе задавать пути ядра (G49-3; `extension.ts:661-675`);
  (в) приём внешне-правового риска ToS/distillation (G49-5); (г) минимально-жизнеспособный набор
  хостов — выпускать подмножество (напр. только Cursor как единственную реальную UI-поверхность,
  P46) vs требовать все 3 при внешнем блокере Codex (P17); (д) кто платит/владеет хостингом при
  выборе remote-lifecycle (P26 gap-8 называет local-vs-remote, но не владельца/финансирование).
- **Gap:** пять продуктово-правово-безопасностных решений — только-Luba, каждое переформирует scope
  и критпуть (можно ли вообще публиковать; какова модель доверия; легален ли distillation; сколько
  хостов в MVP; кто платит за remote), но ни одно не внесено в decision-ledger P48 ⇒ финальную
  последовательность нельзя зафиксировать. Отдельно от P48 gap-3 (там — отсутствие ЛЕДЖЕРА как
  такового + 7 иных решений): здесь ДОБАВЛЯЮТСЯ 5 конкретных решений, которых в том списке нет.
- **Closure steps:** Добавить эти 5 строк в decision-gate ledger (вопрос / опции / радиус влияния /
  заблокированные ветки критпути); получить ответы Luba; финализировать секвенс только после;
  правило F0: решения блокируют лишь decision-dependent продуктовое поведение, не независимый
  scaffolding/docs/tests/hygiene.

---

## Итог P49

Добавлено 6 НОВЫХ дыр управленческого/внешнего/рискового слоя: отсутствие risk-реестра (G49-1),
неуправляемая внешняя зависимость от host-API вендоров + внешний блокер Codex (G49-2),
RCE-поверхность без threat-model (G49-3), отсутствие устанавливаемого/публикуемого артефакта и
канала дистрибуции (G49-4), непрочитанный ToS вендора-учителя как внешне-правовой блокер distillation
(G49-5), и 5 новых решений только-Luba вне списка P48 (G49-6). Проект НЕ завершён; эти строки —
дополнительные блокеры до FULL implementation, а не статус готовности.
