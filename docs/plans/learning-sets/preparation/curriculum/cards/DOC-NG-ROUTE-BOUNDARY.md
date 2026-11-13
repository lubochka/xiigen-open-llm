<!-- CURRENT-PARALLEL-CONTRACT-20261026 -->
## Текущий порядок исполнения — 26.10.2026

Эта карточка используется в CREATE_ONLY-кампании по 1000 кандидатных пар на домен; её микротемы распределяют квоту, не добавляют ещё 1000 каждая. Старые профили/примеры ниже сохранены; текущие количества и IDs задаёт [реестр](../../campaign/registry.json).

**Не завершать работу после одной партии:** сохранить реальные строки, выполнить выданный `finish`, затем `next`; продолжать до квоты назначенной группы. Прямой STOP прекращает действия, прямой NEXT немедленно меняет задание. Не дописывать старый отчёт после переключения. Реальный внешний блок назвать точно, не объявлять DONE.

Общий статус: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`; состояние-владелец: соседний `state.json`. Перед записью получить назначение с уникальным `agent-id`; работать только со своим owner/token. После сохранения полных строк выполнить команду `progress`; после партии — `finish`. Вручную эти JSON не редактировать. Старое время отметки не разрешает захват чужой работы. Сам файл статуса не синхронизирует разные компьютеры/копии папки.

Точные пути к текущему prompt, commands и material берутся из ответа координатора. Для текущего владельца это `coordination-prompt.md`, `coordination-commands.md`, `coordination-material.required.json`; старый request/draft сохраняется. Новые наборы — `C:/Projects/llm_mvp_core/learning-sets`; инструкции — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`.

Reason internally in English; do not output private reasoning. Прогресс по-русски, учебный текст по назначенному профилю, текущая модель Kiro без override. Нужные предметные примеры и локальные RAG-фрагменты включены в назначение. Только проектные источники либо конкретно подтверждённые MIT/Apache-2.0; чужую лицензию не выводить из названия репозитория. Не добавлять checksum, independent review, обучение, export/import и production CI к созданию.

Работающий tool: сохранить один handle, пользоваться его документированным wait/resume до terminal. Не перезапускать по пустому stdout, не подменять ожидание быстрым повтором запросов и не завершать раунд вместо доступного ожидания.

| Назначение | Домен / профиль | Пар в этой доле | Следующее в группе |
|---|---|---:|---|
| `A173` — `DOC-NG-ROUTE-BOUNDARY` | `docs.angular` / `base` | 160 | `A174` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-NG-ROUTE-BOUNDARY — Описать путь, экспорт и границу route/component loader

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A173 | docs.angular | base | 160 | A174 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-NG-ROUTE-BOUNDARY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Назначение

Объяснить фактический путь и то, что загружает каждый callback, не приписывая загрузку по имени файла.

Родитель: `DOC-NG-05`; домен `docs.angular`, профиль `base`. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-028](../../rag/sources/RAG-028.md); [RAG-039](../../rag/sources/RAG-039.md); [RAG-DC-001](../../rag/sources/RAG-DC-001.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Покажи достаточное содержимое root routes, lazy route array и экспортов target modules. Задавай конкретный URL и отключённый preloading. Спрашивай фактический маршрут, имя параметра, выбранный экспорт либо границу ленивой загрузки. Родительский loadChildren может возвращать Routes; leaf loadComponent возвращает компонент. Объяснение не должно выдумывать NgModule, SSR, сетевой запрос до навигации или eager-загрузку по названию каталога. Включай прямой импорт component как eager-контроль и маршруты с разными export names. Если задача о pathMatch/порядке, покажи все competing routes. Не делай вывод о текущей последней версии Angular из старого ADR: использовать только объявленную версию симуляции. Исторический ADR задаёт проектную стратегию, а не единственный допустимый способ для всех Angular-приложений.

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- наблюдение: URL и параметры, route-array loader, leaf component loader, named export, eager import, full redirect.
- поверхность: root route, дочерний route, соседние пути с одинаковым префиксом.

Форма ответа: Русский текст с точным результатом и одним объяснением по показанному коду. Не переписывать код.

## Полные предметные примеры

### 1. route_array_mistaken_for_component

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. app.routes.ts: export const routes: Routes=[{path:'reports',loadChildren:()=>import('./reports.routes').then(m=>m.REPORT_ROUTES)}]; reports.routes.ts: export const REPORT_ROUTES: Routes=[{path:'detail',loadComponent:()=>import('./detail.page').then(m=>m.DetailPage)}]; detail.page.ts экспортирует DetailPage. Для /reports/detail назови, что возвращают два loader callback; код не переписывай.
```

**Chosen**

```text
Первый callback возвращает массив REPORT_ROUTES; второй возвращает компонент DetailPage. URL составлен из родительского reports и дочернего detail.
```

**Rejected**

```text
Первый callback возвращает компонент DetailPage; второй возвращает тот же компонент. URL составлен из родительского reports и дочернего detail.
```

Различие: Один loader описан как компонентный вместо массива маршрутов.

Будущий различающий контроль: В будущем router fixture проверить match и значения callback; сейчас runtime NOT_RUN. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.01`, семейство `v6.ref.doc-ng-route-boundary.flow-then-leaf`; split `reference`, training_allowed `false`.

### 2. route_parameter_renamed

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. export const routes: Routes=[{path:'groups/:groupId/feed',loadComponent:()=>import('./feed.page').then(m=>m.FeedPage)}]; feed.page.ts экспортирует FeedPage. Для URL /groups/17/feed назови компонент и параметр пути; других routes нет.
```

**Chosen**

```text
Выбирается FeedPage; параметр groupId имеет строковое значение "17".
```

**Rejected**

```text
Выбирается FeedPage; параметр id имеет строковое значение "17".
```

Различие: Имя параметра берётся из видимого path, а не из догадки.

Будущий различающий контроль: Проверить paramMap.get("groupId") и отсутствие переименования в id. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.02`, семейство `v6.ref.doc-ng-route-boundary.parameter-name`; split `reference`, training_allowed `false`.

### 3. wrong_export_selected

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. app.routes.ts: export const routes: Routes=[{path:'tools',loadChildren:()=>import('./tools.routes').then(m=>m.ADMIN_ROUTES)}]; tools.routes.ts экспортирует два массива: export const ADMIN_ROUTES: Routes=[{path:'edit',loadComponent:()=>import('./edit.page').then(m=>m.EditPage)}]; export const USER_ROUTES: Routes=[{path:'view',loadComponent:()=>import('./view.page').then(m=>m.ViewPage)}]; Оба page exports корректны. Какой массив выбран родительским callback и какой дочерний URL он объявляет?
```

**Chosen**

```text
Выбран ADMIN_ROUTES; он объявляет /tools/edit.
```

**Rejected**

```text
Выбран USER_ROUTES; он объявляет /tools/view.
```

Различие: Следствие одного неверного выбора export: заменены массив и его путь.

Будущий различающий контроль: Проверить выбранный .then export; не угадывать по имени tools. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.03`, семейство `v6.ref.doc-ng-route-boundary.named-route-export`; split `reference`, training_allowed `false`.

### 4. direct_component_claimed_lazy

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. import { HelpPage } from './help.page'; export const routes: Routes=[{path:'help',component:HelpPage}]; help.page.ts экспортирует HelpPage. Есть ли в этой конфигурации lazy loader для HelpPage? Ответь о видимом коде, без предположений об оптимизаторе сборки.
```

**Chosen**

```text
Lazy loader в маршруте не объявлен: HelpPage импортирован напрямую и передан в component.
```

**Rejected**

```text
Lazy loader объявлен самим полем component: HelpPage будет импортирован callback только при переходе.
```

Различие: Rejected придумывает отсутствующий loader callback.

Будущий различающий контроль: Проверить прямую component-reference; не выдавать это за измерение chunks. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.04`, семейство `v6.ref.doc-ng-route-boundary.eager-component`; split `reference`, training_allowed `false`.

### 5. full_match_treated_as_prefix

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. export const routes: Routes=[{path:'',redirectTo:'home',pathMatch:'full'},{path:'home',loadComponent:()=>import('./home.page').then(m=>m.HomePage)},{path:'settings',loadComponent:()=>import('./settings.page').then(m=>m.SettingsPage)}]; Оба page exports корректны. Для URL /settings какой из показанных маршрутов срабатывает?
```

**Chosen**

```text
Срабатывает settings и выбирается SettingsPage. Пустой redirect с pathMatch:"full" не перехватывает /settings.
```

**Rejected**

```text
Срабатывает пустой redirect и выбирается HomePage, поскольку любой URL начинается с пустого пути.
```

Различие: Одна ошибка — full трактуется как prefix.

Будущий различающий контроль: В будущем navigateByUrl("/settings"); сравнить выбранный компонент. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.05`, семейство `v6.ref.doc-ng-route-boundary.full-empty-redirect`; split `reference`, training_allowed `false`.

### 6. specificity_overrides_route_order

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6. Client-side SPA; preloading отключён, guard/resolver отсутствуют. Routes импортирован из @angular/router. Все упомянутые страницы — корректные standalone-компоненты, пути файлов точны. export const routes: Routes=[{path:'**',loadComponent:()=>import('./missing.page').then(m=>m.MissingPage)},{path:'reports',loadComponent:()=>import('./reports.page').then(m=>m.ReportsPage)}]; Оба page exports корректны. Для URL /reports назови выбранную страницу по этому порядку routes, не исправляя конфигурацию.
```

**Chosen**

```text
Выбирается MissingPage: wildcard стоит раньше reports и первым принимает этот URL.
```

**Rejected**

```text
Выбирается ReportsPage: более конкретный путь автоматически имеет приоритет над раньше заданным wildcard.
```

Различие: Rejected заменяет заданный порядок выдуманным приоритетом специфичности.

Будущий различающий контроль: Проверить первый matching route в конкретном router fixture. Статус: **NOT_RUN**.

Пара `DOC-NG-ROUTE-BOUNDARY.REF.06`, семейство `v6.ref.doc-ng-route-boundary.wildcard-first`; split `reference`, training_allowed `false`.

## Объём, сохранение и последующее обучение

Первая партия — 10 кандидатных пар. План учебного пилота 100 TRAIN / 20 DEV / 20 EXAM; это назначение будущего объёма, не выполненный эксперимент. TRAIN-пилот входит в общий родительский пул 1000, а не прибавляется к нему. Содержание прежней parent-карточки сохранено; исполняется единая очередь без повторной генерации parent поверх objective.

Все создаваемые наборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: тот же путь плюс `/preparation`. Существующие attempts не удалять; продолжение по request/state, новые версии под новым run ID. Точные команды, next ID, пути набора и будущего B1 формирует общий блок ниже. Эти пути не доказывают наличие весов.

При отдельно назначенном учебном опыте восстановить один совместимый обученный B1 для обоих плеч, обеспечить достаточный TRAIN и ненулевую предметную компетенцию, измерить обучение до/после и свежую загрузку. 0→0 не является валидным учебным результатом. Любой измеренный полезный выигрыш — успех по этому показателю и домену; рядом показать остальные дельты, регрессии и стоимость. Предметный learner сначала проверяется при верном маршруте, затем prompt→router→домен→ответ отдельно. Создание JSON само по себе не измеряет обучение. Решение о production-интеграции остаётся за пользователем.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `DOC-NG-ROUTE-BOUNDARY/base` | 134 | `CODE-NG-IMMUTABLE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-NG-ROUTE-BOUNDARY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-NG-ROUTE-BOUNDARY/base" -Run "S605" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-NG-ROUTE-BOUNDARY/base/S605`. Следующий поток: `CODE-NG-IMMUTABLE/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-NG-ROUTE-BOUNDARY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.angular/base/DOC-NG-ROUTE-BOUNDARY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
