# План проверки: умеют ли плагины оркестрировать сложные задачи

Статус: архитектурный план проверки продукта.

Этот документ отвечает на один продуктовый вопрос:

**Может ли человек открыть Visual Studio или VS Code в реальном проекте,
дать сложную задачу из `orchestrator prompts.txt`, и получить от XIIGen
правильную логистику решения этой задачи?**

Здесь "оркестрация" не означает просто JSON, flow diagram или список действий.

Оркестрация задачи — это способность системы подняться на уровень человека,
который руководит сложной работой:

- понять, какую цель человек на самом деле хочет достичь;
- понять, какие части проекта участвуют в этой цели;
- разложить путь к цели на большие этапы;
- внутри каждого большого этапа разложить работу на мелкие понятные шаги;
- определить, что надо сначала узнать из кода, документации, wiki или RAG;
- понять, где Angular, .NET, SQL, design, deployment, skills и AI-модели
  являются частями одной системы;
- сказать, что можно делать сразу, а где нельзя идти дальше без evidence;
- дать человеку такой маршрут, по которому реально можно вести проект.

Плагин не должен сам "быть мозгом". Плагин должен быть входом в XIIGen:
он принимает задачу из реальной среды разработки, передает проектный контекст,
а затем показывает пользователю человечески понятную оркестрацию решения.

## 1. Что мы проверяем

Мы проверяем два плагина:

- Visual Studio plugin;
- VS Code plugin.

Для каждого плагина надо доказать не то, что он запускается, а то, что через
него пользователь получает правильную верхнеуровневую логистику работы.

Успех выглядит так:

```text
Пользователь дал сложную задачу.
Плагин передал задачу и проект в XIIGen.
XIIGen понял цель.
XIIGen понял проектный контекст.
XIIGen предложил большие этапы решения.
XIIGen разложил каждый этап на конкретные шаги.
XIIGen показал, какие части кода, дизайна, документации, wiki и RAG надо читать.
XIIGen сказал, что можно делать, чего нельзя делать без evidence, и какой результат ожидается.
```

Если система вместо этого возвращает общий текст, generic advice или красивый
технический отчет, продуктовая задача не решена.

## 2. Что не считается оркестрацией

Это не оркестрация:

- "надо проанализировать проект";
- "надо создать план";
- "надо использовать RAG";
- "надо проверить Angular и .NET";
- "надо написать тесты";
- "надо сделать implementation phases";
- `passed=true`;
- "864 rows passed";
- "плагин вернул непустой JSON";
- "ответ содержит слово flow";
- "ответ содержит список units";
- "ответ можно было написать, не открывая проект".

Это все может быть частью результата, но само по себе не доказывает, что система
умеет оркестрировать сложную задачу.

## 3. Человеческая форма правильного ответа

Правильный ответ плагина должен читаться примерно так:

```text
Я понял цель: ты хочешь ...

Это не одиночная правка. Это комплексная задача.

Сначала надо понять:
- какие части текущего проекта относятся к этой цели;
- какие связанные проекты участвуют;
- где клиентская логика;
- где серверная логика;
- где хранилище или stored procedures;
- какие документы или wiki-ответы нужны до решения.

Большой этап 1: ...
Зачем он нужен: ...
Мелкие шаги:
- ...
- ...
Ожидаемый результат этапа: ...

Большой этап 2: ...
Зачем он нужен: ...
Мелкие шаги:
- ...
- ...
Ожидаемый результат этапа: ...

Где нельзя угадывать:
- ...

Что можно делать следующим:
- ...
```

Такой ответ показывает человеку маршрут решения. Не просто "что-то сделать",
а как провести сложную работу от намерения до результата.

## 4. Главная проверка на реальном коде

Проверка должна идти не в абстракции, а на реальных проектах:

```text
C:\Projects\bizguide-client
C:\Projects\GenieDynamicApp
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVP
C:\Projects\geniesoftGameMVPServer
C:\Projects\wizGameserver
C:\Projects\genietasks
C:\Projects\askapediaweb
C:\Projects\xiigen-flows-dot-net-angular
C:\Projects\xiigen-platform-dot-net-angular
```

Почему это важно:

- на Angular-only проекте система должна понять, что видит только клиент;
- на Angular + .NET проектах система должна связать client и server как одну
  бизнес-систему;
- если есть SQL stored procedures, они должны войти в бизнес-логику;
- если SQL evidence нет, система не имеет права притворяться, что знает SQL path;
- на переведенных XIIGen проектах система должна понимать именно flow/platform
  смысл, а не давать generic enterprise план.

## 5. Три главных примера оркестрации из prompt-файла

Эти три примера являются ядром проверки. Они показывают, что значит
"оркестрировать задачу" на человеческом уровне.

### 5.1. Пользователь хочет создать систему из Figma

Что хочет человек:

Он приносит Figma и выбирает client technology, server technology и database.
Но в нашей проверке это не абстрактный новый проект. Чаще всего рядом уже есть
готовый код. Поэтому правильный вопрос не "как сгенерировать систему из Figma",
а:

```text
Как соотнести новый дизайн с уже существующей системой и понять, что можно
переиспользовать, что можно переодеть skin/theme, что надо переделывать, и не
нужен ли upgrade стека перед реализацией?
```

Правильная оркестрация:

Большой этап 1: понять существующий продукт до анализа Figma.

Мелкие шаги:

- найти текущие screens/routes/pages;
- найти текущие components/shared components;
- найти текущую navigation structure;
- найти текущую theme/skin/design tokens/styles;
- найти state management;
- найти API calls из экранов;
- найти backend endpoints/services, если есть linked server;
- найти SQL/stored procedures, если они участвуют в flow;
- понять, какие business flows уже реализованы.

Ожидаемый результат:

- карта существующих экранов;
- карта существующих компонентов;
- карта связи UI -> API -> server -> data;
- список мест, где дизайн можно менять безопасно;
- список мест, где изменение дизайна может затронуть business logic.

Провал:

- плагин сразу анализирует Figma и не смотрит текущий код;
- плагин не знает, какие screens уже есть;
- плагин не понимает, что часть дизайна может быть уже реализована.

Большой этап 2: понять Figma как новый продуктовый intent.

Мелкие шаги:

- пройти по Figma screens;
- понять назначение каждого screen;
- найти повторяющиеся компоненты;
- найти user journeys;
- найти новые states;
- найти новые forms/actions;
- понять, какие действия требуют backend logic;
- понять, какие screens являются новыми, а какие похожи на существующие.

Ожидаемый результат:

- карта Figma screens;
- карта Figma components;
- карта user journeys;
- список новых требований, которых нет в текущей системе.

Провал:

- Figma превращается только в HTML/CSS;
- user journey не выделен;
- backend/data needs не замечены.

Большой этап 3: сопоставить Figma с существующими screens.

Мелкие шаги:

- для каждого Figma screen найти closest existing screen;
- определить: это новый screen, redesign существующего screen или variation;
- для каждого Figma component найти closest existing component;
- определить, можно ли переиспользовать existing component;
- определить, нужен ли новый component;
- определить, какие routes/navigation надо менять;
- определить, какие API contracts уже подходят;
- определить, какие API contracts надо расширить;
- определить, какие server/data changes нужны.

Ожидаемый результат:

```text
Figma screen A -> existing screen X -> redesign only / new behavior / new module
Figma component B -> existing component Y -> reuse / extend / replace
Figma action C -> existing API Z -> reuse / extend / missing backend evidence
```

Провал:

- нет screen-to-screen mapping;
- нет component-to-component mapping;
- плагин не отличает redesign от новой функциональности.

Большой этап 4: оценить, можно ли поменять skin/theme без полной переделки.

Мелкие шаги:

- проверить, есть ли centralized theme;
- проверить, есть ли design tokens;
- проверить, есть ли shared component library;
- проверить, не захардкожены ли цвета/layout/styles в screens;
- проверить, можно ли применить Figma через theme-level changes;
- определить, где нужны component-level changes;
- определить, где нужен screen rewrite;
- определить цену каждого варианта.

Ожидаемый результат:

```text
Skin/theme change possible: да / частично / нет.

Если да:
- какие theme files менять;
- какие shared components обновить;
- какие screens автоматически получат новый стиль.

Если частично:
- что покрывается theme;
- какие components надо переписать;
- какие screens требуют ручной работы.

Если нет:
- почему текущая UI architecture не позволяет skin replacement;
- какой refactor нужен перед применением дизайна.
```

Провал:

- плагин не проверяет, можно ли сменить skin;
- плагин сразу предлагает переписать UI;
- плагин не видит, что архитектура UI может быть препятствием.

Большой этап 5: проверить техническую готовность стека.

Мелкие шаги:

- определить версии Angular/React/client framework;
- определить версии .NET/server framework, если есть backend;
- определить UI libraries;
- определить build tooling;
- определить package/dependency health;
- определить, поддерживает ли текущий stack нужный Figma implementation path;
- определить, нужен ли upgrade перед внедрением дизайна;
- если upgrade нужен, поставить его до redesign или выделить отдельной фазой.

Ожидаемый результат:

```text
Stack readiness:
- client stack: ready / upgrade needed / unknown
- server stack: ready / upgrade needed / not in repo
- UI library: reusable / blocks redesign / unknown
- build tooling: ready / blocks implementation

Upgrade before Figma implementation:
- required: да / нет / unknown
- why: ...
- safest order: upgrade first / redesign first / split into two tracks
```

Провал:

- плагин не проверяет версии и tooling;
- плагин планирует дизайн поверх устаревшего stack;
- upgrade всплывает только во время implementation.

Большой этап 6: построить архитектуру изменения.

Мелкие шаги:

- разделить работу на theme changes, component changes, screen changes,
  backend/API changes, data changes;
- для каждого изменения указать, какой existing code затронут;
- для каждого изменения указать risk level;
- построить порядок: сначала safe reusable base, потом screens, потом behavior;
- если Angular + .NET + SQL участвуют вместе, вести это как один business flow.

Ожидаемый результат:

- план внедрения, который не ломает существующую систему;
- список мест для review;
- список мест, где нужен пользовательский выбор.

Большой этап 7: feedback loop.

Мелкие шаги:

- показать mapping пользователю;
- показать, какие screens совпали, какие нет;
- показать, где дизайн можно применить быстро;
- показать, где нужна переделка;
- принять изменение screen или subscreen;
- пересчитать только затронутые mappings и implementation phases.

Ожидаемый результат:

- пользователь понимает цену дизайна до начала кода;
- изменение Figma не заставляет пересобирать весь план с нуля.

Провал всего сценария:

- плагин говорит "сгенерировать HTML/CSS";
- плагин не сравнивает Figma с существующими screens;
- плагин не проверяет skin/theme возможность;
- плагин не проверяет upgrade стека;
- backend и database не появляются там, где они нужны;
- feedback пользователя не меняет дальнейшую работу.

### 5.2. Пользователь хочет встроить Figma design в существующую систему

Что хочет человек:

Он не хочет новый проект. Он хочет взять дизайн и применить его к уже
существующей системе.

Правильная оркестрация:

Большой этап 1: сначала понять существующую систему как продукт, а не как
набор файлов.

Мелкие шаги:

- найти структуру проекта;
- найти routing;
- найти screen/component structure;
- найти API/server boundaries;
- найти current theme/design system;
- найти места, где UI связан с бизнес-логикой;
- найти, какие screens являются основными пользовательскими сценариями;
- найти, какие screens являются support/admin/settings;
- найти, где состояние хранится на client;
- найти, где состояние приходит от server;
- найти, где database или stored procedures влияют на экран.

Ожидаемый результат:

- документация текущей системы, достаточная для безопасного изменения;
- карта "screen -> component -> API -> server/data";
- список бизнес-сценариев, которые нельзя менять как простую верстку.

Большой этап 2: понять новый дизайн.

Мелкие шаги:

- разобрать Figma screens;
- найти новые components;
- найти отличия от текущего UI;
- понять, это новый module или изменение существующего;
- найти, какие Figma screens заменяют существующие screens;
- найти, какие Figma screens являются новыми;
- найти, какие Figma components уже существуют в коде;
- найти, какие Figma interactions меняют business behavior.

Ожидаемый результат:

- карта design impact.

Большой этап 3: решить, как применять стиль и сколько это стоит.

Мелкие шаги:

- определить, что можно поменять только в theme;
- определить, где нужны component changes;
- определить, где нужен screen rewrite;
- определить, где нужен backend/API change;
- определить, где может сломаться business logic;
- отметить места, где evidence не хватает;
- оценить: skin change, partial redesign или full rebuild;
- объяснить, почему выбран именно этот путь.

Ожидаемый результат:

- безопасный маршрут применения дизайна;
- честная цена изменения: simple skin, medium refactor или large rebuild.

Большой этап 4: проверить техническую готовность до реализации.

Мелкие шаги:

- проверить client framework version;
- проверить server framework version, если backend участвует;
- проверить UI library и build tooling;
- проверить, поддерживает ли текущий stack нужный design implementation;
- определить, нужен ли upgrade;
- если upgrade нужен, решить порядок: upgrade before design, design before
  upgrade или separate track.

Ожидаемый результат:

- решение, можно ли внедрять дизайн сейчас;
- список обязательных upgrades;
- список рисков, если внедрять дизайн без upgrade.

Большой этап 5: спланировать implementation.

Мелкие шаги:

- разделить изменения на маленькие шаги;
- сначала обновить theme/common components;
- потом screens;
- потом integration points;
- потом backend/API changes, если они есть;
- потом tests и visual review;
- потом feedback loop с пересчетом затронутых частей.

Ожидаемый результат:

- план внедрения без переписывания системы с нуля.

Провал:

- плагин предлагает переписать все;
- не читает существующий проект;
- не отличает новый module от изменения существующего;
- не видит риск для бизнес-логики;
- не проверяет, можно ли сменить skin/theme;
- не проверяет, нужен ли upgrade стека.

### 5.3. Пользователь хочет создать систему с нуля по объяснению

Что хочет человек:

У него есть идея, но дизайн неполный или отсутствует. Нужно превратить
объяснение в систему.

Правильная оркестрация:

Большой этап 1: понять, действительно ли это "с нуля".

Мелкие шаги:

- проверить, есть ли существующий проект, который можно расширить;
- проверить, есть ли похожие modules/screens/services;
- проверить, есть ли reusable components;
- проверить, есть ли existing theme/design language;
- проверить, есть ли existing backend capabilities;
- проверить, есть ли старый код, который нельзя потерять;
- определить: new system, new module in existing system или replacement of
  existing flow.

Ожидаемый результат:

- решение, строим ли с нуля или используем существующую основу;
- список того, что можно переиспользовать;
- список того, что надо создать заново.

Провал:

- плагин слышит "с нуля" и игнорирует существующий код;
- плагин не проверяет reuse;
- плагин предлагает новый stack, не объяснив, почему старый не подходит.

Большой этап 2: понять продуктовую цель.

Мелкие шаги:

- выделить, кто пользователь;
- что пользователь должен уметь делать;
- какие основные сценарии нужны;
- какие вопросы еще не решены;
- какие существующие возможности проекта могут закрыть часть цели;
- какие возможности точно отсутствуют.

Ожидаемый результат:

- ясная product scope map.

Большой этап 3: создать design direction на основе того, что уже есть.

Мелкие шаги:

- посмотреть, что уже есть у пользователя;
- определить стиль и theme;
- понять, какие screens нужны;
- отметить, где нужен дополнительный дизайн;
- определить, можно ли использовать существующий skin/theme;
- определить, нужен ли redesign или только extension;
- определить, не блокирует ли current UI architecture новый продукт.

Ожидаемый результат:

- направление дизайна, достаточное для старта.

Большой этап 4: проверить техническую готовность.

Мелкие шаги:

- проверить текущий client stack;
- проверить текущий server stack;
- проверить persistence/database path;
- проверить build/deploy constraints;
- решить, нужен ли upgrade перед новой системой;
- определить, какие upgrades являются prerequisite, а какие можно отложить.

Ожидаемый результат:

- stack readiness decision;
- upgrade route, если он нужен;
- список технических рисков до implementation.

Большой этап 5: создать архитектуру.

Мелкие шаги:

- выделить client;
- выделить server;
- выделить storage;
- выделить flows;
- определить AI/RAG участки;
- определить интеграции.

Ожидаемый результат:

- архитектура системы.

Большой этап 6: создать skills.

Мелкие шаги:

- skills для архитектуры;
- skills для реализации;
- skills для тестов;
- skills для DevOps;
- skills для документации;
- skills для безопасности.

Ожидаемый результат:

- библиотека skills, которую можно переиспользовать.

Большой этап 7: implementation plan.

Мелкие шаги:

- разбить реализацию на recoverable phases;
- для каждой фазы определить результат;
- определить проверки;
- определить state saving;
- определить, как продолжать после остановки.

Ожидаемый результат:

- план, по которому executor может реально строить систему.

Большой этап 8: implementation.

Мелкие шаги:

- реализовать phase by phase;
- проверять каждую часть;
- деплоить или запускать локально;
- debug;
- отдавать source code или держать проект на платформе.

Ожидаемый результат:

- работающая система или честно зафиксированный missing evidence.

Провал:

- плагин сразу пишет код;
- нет product scope;
- нет design direction;
- нет проверки, действительно ли это "с нуля";
- нет reuse analysis;
- нет stack readiness / upgrade decision;
- нет skills;
- нет recoverable phases;
- нет пути к working system.

## 6. Остальные сценарии из `orchestrator prompts.txt`

Остальные сценарии проверяются по той же логике: не "какой ответ дал плагин",
а "какую логистику решения он построил".

### 6.1. Skill set для нового проекта

Нужно увидеть:

- зачем нужны skills;
- какие классы skills нужны;
- какие документы надо прочитать;
- как skills будут использовать разные AI tools;
- какие хорошие и плохие примеры нужны;
- как сохранять состояние работы.

### 6.2. n8n-like flow engine

Нужно увидеть:

- как запускается процесс;
- где trace id;
- где job state;
- где polling;
- где AI steps;
- где сохраняются prompts и intermediate results;
- где review другой моделью;
- где feedback пользователя;
- как feedback попадет в будущую работу.

### 6.3. Приложение с клиентами, диетой и WhatsApp

Нужно увидеть:

- registration flow;
- сохранение client profile;
- AI diet generation;
- несколько моделей и review rounds;
- daily WhatsApp intake;
- расчет calories/supplements;
- adjustment flow;
- daily feedback через app и WhatsApp;
- сохранение feedback для будущих рекомендаций.

### 6.4. Content generation pipeline

Нужно увидеть:

- text или audio intake;
- audio-to-text;
- text improvement;
- image plan;
- video plan;
- music plan;
- voice/avatar generation;
- packaging под разные платформы;
- descriptions/captions;
- scheduling.

### 6.5. UML, automations, entity relations

Нужно увидеть:

- чтение diagrams;
- выделение entities;
- выделение relations;
- выделение automations;
- проверку lifecycle;
- связь design с code evidence.

### 6.6. AI microservices и generated code

Нужно увидеть:

- как previous jobs дают context;
- как flow виден на UI;
- как debugged каждый node;
- как генерируется code;
- как стартует microservice;
- как он тестируется;
- как интегрируется обратно в flow.

### 6.7. RAG и GraphRAG вокруг AI

Нужно увидеть:

- какие вопросы задаются RAG перед AI step;
- какой context package собирается;
- что сохраняется после AI result;
- как feedback попадает обратно в RAG;
- какие GraphRAG alternatives сравниваются и почему.

### 6.8. Alternative stacks, hosting, DevOps

Нужно увидеть:

- как выбираются server/client/database/queue варианты;
- где нужны technology-specific skill folders;
- как выбирается hosting path;
- как создается CI/CD;
- как учитываются security, logging, Swagger, MCP и optimization.

## 7. Проверка на плагинах

Для каждого сценария:

1. Открыть подходящий реальный проект в Visual Studio.
2. Дать задачу через настоящий entrypoint плагина.
3. Проверить, что плагин передал XIIGen активный проект и связанные проекты.
4. Прочитать ответ как человек: есть ли логистика решения комплексной задачи.
5. Повторить в VS Code.
6. Сравнить не по словам, а по смыслу:
   - цель понята;
   - большие этапы есть;
   - мелкие шаги есть;
   - проектный context учтен;
   - Angular/.NET/SQL связаны там, где должны быть связаны;
   - missing evidence назван;
   - результат можно дать человеку как маршрут работы.

## 8. Когда можно сказать "да"

Фраза "да, плагины умеют оркестрировать задачи из `orchestrator prompts.txt`"
разрешена только если:

- каждый главный сценарий проверен на реальном проекте;
- Visual Studio plugin прошел те же сценарии;
- VS Code plugin прошел те же сценарии;
- ответы не generic;
- ответы зависят от реального кода;
- в ответах есть большие этапы и мелкие шаги;
- в ответах видно, как вести задачу до результата;
- для Angular + .NET проектов client и server связаны как одна бизнес-система;
- stored procedures учитываются только при реальном evidence;
- missing evidence не замазан;
- итоговый отчет написан человечески, а не как таблица технических counters.

## 9. Итоговый отчет

В конце нужен отчет такого смысла:

```text
Мы проверили, могут ли Visual Studio и VS Code плагины вести сложные задачи через XIIGen.

На сценарии "Figma -> whole system" плагины смогли / не смогли:
- понять цель;
- разложить работу на этапы;
- связать screens, backend и database;
- показать feedback loop.

На сценарии "Figma -> existing system" плагины смогли / не смогли:
- сначала понять текущую систему;
- отличить новый module от изменения существующего;
- построить безопасный route внедрения.

На сценарии "system from explanation" плагины смогли / не смогли:
- превратить объяснение в product scope;
- создать design direction;
- создать architecture;
- создать skills;
- создать implementation route.

Ограничения:
- ...
```

Такой отчет отвечает на продуктовый вопрос. Все остальное является технической
деталью, а не доказательством оркестрации.

## 10. Конкретная матрица: три задачи на каждом реальном проекте

Ниже главный недостающий слой проверки.

Нельзя проверять плагины общими словами. Нужно взять три базовых пользовательских
задачи и прогнать их на каждом реальном проекте. Для каждого проекта ожидаемый
результат разный, потому что код разный.

Три задачи:

1. Пользователь хочет создать систему из Figma.
2. Пользователь хочет встроить Figma design в существующую систему.
3. Пользователь хочет создать систему с нуля по объяснению, потому что design
   неполный или отсутствует.

### 10.1. `C:\Projects\bizguide-client`

Что это за проверочный контекст:

Angular/client-side проект. Сервер и база данных не доказаны самим этим
репозиторием.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- плагин должен сказать: "Я вижу только client-side часть";
- разобрать Figma screens как будущие Angular routes/components;
- определить, какие screens требуют backend API;
- определить, какие данные нужны каждому screen;
- составить список backend/database вопросов, которые нельзя решить из этого
  репозитория;
- предложить связать этот проект с server repository перед полной архитектурой.

Хороший результат:

```text
По этому репозиторию я могу спланировать Angular часть: routes, components,
state, forms, API calls. Но я не могу честно спланировать server и database,
пока не найден linked backend. Следующий шаг: найти или подключить backend
репозиторий, затем построить whole-system architecture.
```

Плохой результат:

- плагин сразу рисует full-stack architecture;
- выдумывает API;
- выдумывает database;
- говорит "готово" без backend evidence.

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- найти текущую Angular structure;
- найти current routes/screens/components;
- найти current theme/styles;
- сравнить Figma с текущим UI;
- разделить изменения на theme-level, component-level и route-level;
- отметить, где изменение UI требует backend evidence.

Хороший результат:

```text
Сначала документируем существующий Angular UI. Потом сравниваем Figma с текущими
screens. Если это только style/theme, меняем theme и shared components. Если это
новый business flow, без backend evidence не продолжаем.
```

Плохой результат:

- плагин предлагает переписать client;
- не ищет текущие components;
- не различает style change и business-flow change.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- превратить объяснение пользователя в client-side product map;
- определить screens, navigation, forms, state;
- определить, какие API/backend flows нужны;
- честно вынести server/database в missing evidence;
- создать только Angular implementation route до момента, где нужен backend.

Хороший результат:

```text
Я могу начать с client shell: navigation, screens, forms, state и API contracts.
Полную систему из этого репозитория построить нельзя, потому что server/database
часть не найдена.
```

### 10.2. `C:\Projects\GenieDynamicApp` + `C:\Projects\GenieDynamicServer`

Что это за проверочный контекст:

Связка Angular client и .NET server. Проверка должна видеть их как одну
бизнес-систему.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- разобрать Figma screens как client routes/components;
- сопоставить UI actions с server endpoints/services;
- определить, какие server flows нужны для каждого screen;
- определить storage/persistence path;
- построить architecture as flows: screen -> action -> API -> service -> data;
- создать technical design per screen.

Хороший результат:

```text
Для каждого Figma screen строю связку: Angular screen, user action, .NET API,
server service, persistence, validation, error handling. Архитектура выходит не
как client отдельно и server отдельно, а как единый business flow.
```

Плохой результат:

- плагин анализирует Angular без server;
- server планируется generic;
- UI actions не связаны с backend logic.

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- сначала документировать существующую client/server систему;
- найти current Angular screens и matching .NET flows;
- понять, новый design меняет только UI или меняет business behavior;
- если меняется behavior, спланировать client + server changes вместе;
- добавить tests для обеих сторон.

Хороший результат:

```text
Я сначала строю карту текущего flow: Angular component -> API call -> .NET
handler/service -> data. Потом применяю Figma только там, где понятно, что
меняется: theme, component или business flow.
```

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- извлечь product scope;
- создать screen map;
- создать server domain/service map;
- создать API contracts между Angular и .NET;
- определить data model;
- создать skills и implementation phases для обеих сторон;
- вести реализацию как единый поток.

Хороший результат:

```text
Новая система раскладывается на product flows. Для каждого flow есть Angular
screen, API contract, .NET service, persistence и tests. Реализация идет фазами,
где client и server не расходятся.
```

### 10.3. `C:\Projects\geniesoftGameMVP` + `C:\Projects\geniesoftGameMVPServer`

Что это за проверочный контекст:

Client + server для game/product flow. Проверка должна увидеть, что пользовательский
experience и server rules вместе создают одну систему.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- разобрать screens как game/user experience;
- определить client states;
- связать user actions с server game/session/state logic;
- определить, где server является source of truth;
- определить persistence/session storage;
- создать flows для gameplay или user journey.

Хороший результат:

```text
Figma screens превращаются не просто в UI, а в gameplay/user journey. Для каждого
действия определяю client state, server validation, session state и результат
для пользователя.
```

Плохой результат:

- плагин делает только UI plan;
- не видит server rules;
- не отличает визуальное состояние от server state.

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- документировать текущий game/client flow;
- документировать server rules;
- понять, меняет ли дизайн только внешний вид или game behavior;
- если behavior меняется, спланировать server changes;
- проверить, что client не начинает решать то, что должен решать server.

Хороший результат:

```text
Применение дизайна разделено на visual layer и game logic layer. Если Figma
меняет поведение, это идет через server rules, а не только через Angular/client.
```

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- извлечь game/product goal;
- определить user journey;
- определить rules;
- определить client states;
- определить server state;
- определить persistence;
- создать implementation phases с проверкой client/server consistency.

### 10.4. `C:\Projects\wizGameserver`

Что это за проверочный контекст:

Server-only .NET проект. Client/design задачи можно оркестрировать только до
границы server evidence.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- сказать, что Figma/client часть не представлена в этом репозитории;
- из server code определить, какие backend capabilities уже есть;
- определить, какие screens могли бы обращаться к этим capabilities;
- не выдумывать Angular/client implementation;
- запросить client repository или Figma evidence.

Хороший результат:

```text
По этому проекту я могу построить server-side часть будущей системы: endpoints,
services, rules, persistence. Client flow и Figma mapping требуют отдельного
client/design evidence.
```

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- документировать server capabilities;
- понять, какие API/service contracts может затронуть новый design;
- указать, что UI application неизвестен;
- предложить integration questions для client repo.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- извлечь server-side requirements;
- создать domain/service/API plan;
- отметить missing client/design;
- построить implementation phases только для server scope.

### 10.5. `C:\Projects\genietasks`

Что это за проверочный контекст:

Проект с неизвестным или смешанным scope. Первый шаг оркестрации — не решать
задачу, а классифицировать проект.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- сначала определить stack проекта;
- найти, есть ли client;
- найти, есть ли server;
- найти, есть ли persistence;
- только после этого адаптировать Figma route.

Хороший результат:

```text
Я не начинаю whole-system plan, пока не понимаю, что это за проект. Сначала
классифицирую stack и boundaries, потом строю orchestration route.
```

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- классифицировать текущую систему;
- найти existing UI/API/data boundaries;
- если границы не найдены, честно заблокировать внедрение design.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- определить, можно ли этот repo использовать как основу;
- если да, построить route вокруг найденного stack;
- если нет, сказать, какие project artifacts отсутствуют.

### 10.6. `C:\Projects\askapediaweb`

Что это за проверочный контекст:

Проект должен проверять, умеет ли система не угадывать назначение по имени, а
выводить его из реального кода.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- определить actual stack;
- найти, есть ли user-facing web;
- найти backend/data boundaries;
- адаптировать Figma plan только после evidence.

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- документировать existing pages/components/routes;
- понять, можно ли применять design как theme, page changes или new module;
- не делать выводы по названию проекта.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- извлечь product goal;
- сопоставить его с тем, что проект уже делает;
- решить, расширять текущий проект или начинать новый module;
- указать missing evidence.

### 10.7. `C:\Projects\xiigen-flows-dot-net-angular`

Что это за проверочный контекст:

Переведенный XIIGen flow проект. Проверка должна видеть не generic Angular +
.NET, а именно систему flow orchestration.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- разобрать Figma screens как UI для flow management;
- связать screens с flow definitions, flow state, node execution и results;
- определить, какие screens управляют flow authoring;
- определить, какие screens показывают debug или run results;
- связать client UI с .NET orchestration backend.

Хороший результат:

```text
Figma превращается в продуктовые screens для управления flows: создание flow,
запуск, просмотр node execution, debug, results и feedback. Client и server
связаны через понятия XIIGen flow, а не через generic CRUD.
```

Плохой результат:

- плагин говорит generic "dashboard + API";
- не видит flow definitions/state/results;
- не видит orchestration как домен продукта.

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- сначала документировать текущую flow architecture;
- понять, какие screens уже существуют;
- понять, новый design меняет flow authoring, run monitoring или debug;
- спланировать изменения без разрушения existing orchestration logic.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- превратить объяснение в новый XIIGen flow capability;
- определить flow definition;
- определить nodes;
- определить state/results;
- определить UI для управления;
- определить backend execution path;
- определить tests для flow behavior.

### 10.8. `C:\Projects\xiigen-platform-dot-net-angular`

Что это за проверочный контекст:

Переведенная XIIGen platform. Проверка должна видеть platform-level capabilities,
а не только отдельное приложение.

#### Задача 1: Figma -> whole system

Ожидаемая оркестрация:

- понять, какие platform capabilities представлены в дизайне;
- разложить screens на modules;
- связать modules с backend platform services;
- определить shared components, permissions, tenants, navigation;
- определить, какие flows или microservices нужны.

Хороший результат:

```text
Figma анализируется как platform product: modules, users, permissions,
navigation, backend services, shared components и platform flows.
```

#### Задача 2: Figma -> existing system

Ожидаемая оркестрация:

- документировать текущую platform structure;
- понять, где design меняет module, shared shell, permission model или service;
- спланировать rollout без разрушения platform consistency.

#### Задача 3: system from explanation

Ожидаемая оркестрация:

- понять, это новая platform capability или отдельный app;
- определить modules;
- определить backend services;
- определить shared UI;
- определить permissions/tenant impact;
- создать implementation route.

## 11. Как использовать эту матрицу

Для каждого проекта и каждой задачи плагин должен вернуть не один общий ответ,
а адаптированный результат.

Проверяющий должен увидеть:

- проект прочитан по смыслу;
- задача адаптирована под этот проект;
- большие этапы отличаются там, где проекты разные;
- новые design/product requests сопоставлены с существующим кодом;
- existing screens/components/services найдены до предложения переделки;
- reuse path описан до rewrite path;
- skin/theme feasibility проверена для UI задач;
- stack readiness и upgrade needs проверены до implementation;
- цена изменения названа человечески: small skin/theme change, medium refactor,
  large rebuild или blocked by missing evidence;
- missing evidence назван честно;
- full-stack logic появляется только там, где есть full-stack evidence;
- XIIGen-specific logic появляется только в XIIGen проектах;
- server-only проект не получает выдуманный client;
- client-only проект не получает выдуманный server.

Если все проекты получают одинаковый ответ, проверка провалена.

## 12. Где предыдущие версии плана были слабыми

Эти ошибки нельзя повторять в следующих версиях проверки.

### Ошибка 1: Figma трактовалась как генерация с нуля

На самом деле для проверки плагинов важнее brownfield case: Figma приходит к
уже существующему коду. Поэтому первая работа системы — не генерировать, а
сопоставить новый дизайн с текущими screens, components, theme, API и business
flows.

### Ошибка 2: "existing system" был описан слишком бедно

Просто сказать "документировать существующую систему" недостаточно. Нужно
понять, что уже есть, что можно переиспользовать, что можно переодеть skin/theme,
что требует component refactor, что требует backend change, и где нужен stack
upgrade.

### Ошибка 3: "с нуля" принималось буквально

Даже когда пользователь говорит "с нуля", плагин должен проверить, нет ли уже
готовой основы: старого проекта, похожего module, reusable theme, existing API,
skills или deployment setup. Оркестрация должна объяснить, почему мы строим с
нуля или почему лучше расширять существующее.

### Ошибка 4: проектная матрица была недостаточно конкретной

Для каждого реального проекта должны быть ожидаемые результаты. Не одинаковая
логика для всех, а разная:

- Angular-only проект: client route, missing backend evidence.
- Angular + .NET проект: единый client-server business flow.
- Server-only проект: server scope, missing client/design evidence.
- XIIGen flow проект: именно flow orchestration domain.
- XIIGen platform проект: platform modules, permissions, services, shared UI.

### Ошибка 5: не было цены изменения

Хорошая оркестрация должна сказать человеку не только "что делать", но и цену:

- это простая смена skin/theme;
- это refactor components;
- это новый module;
- это изменение business behavior;
- это full rebuild;
- это blocked, потому что evidence не хватает.

### Ошибка 6: не было технической готовности

Перед implementation нужно проверить stack readiness:

- client framework version;
- server framework version;
- UI libraries;
- build tooling;
- dependency health;
- deployment constraints;
- нужен ли upgrade перед задачей.

Без этого плагин может предложить красивый маршрут, который невозможно безопасно
исполнить на текущем коде.
