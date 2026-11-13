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
| `A191` — `CODE-NG-IMMUTABLE` | `code.angular` / `base` | 160 | `A192` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# CODE-NG-IMMUTABLE — Обновить нужную ветвь состояния без мутации соседних объектов

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A191 | code.angular | base | 160 | A192 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/CODE-NG-IMMUTABLE.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Назначение

Создавать ровно требуемые новые ссылки, сохранять исходные данные и незатронутые ветви по заданному контракту.

Родитель: `CODE-NG-03`; домен `code.angular`, профиль `base`. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-028](../../rag/sources/RAG-028.md); [RAG-039](../../rag/sources/RAG-039.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Дай полностью определённые TypeScript-типы, неизменяемый вход, требуемое изменение, правила reference identity и контрольные данные. Различай новый массив и новые изменённые элементы: shallow copy не защищает вложенный объект. Чередуй обновление вложенной карты, перемещение в массиве, удаление элемента и no-op. В no-op явно задай, требуется ли исходная или новая ссылка; не обучай правилу «всегда копировать». Rejected должен быть типокорректным и нарушать одну границу: мутация старого объекта, потеря соседнего поля, неверная идентичность no-op или перестановка другого элемента. Сохранение ссылок незатронутых объектов проверяется отдельным oracle позднее. Эти пары сами не доказывают поведение change detection или выигрыш рендера; это определяется только разрешённым будущим Angular опытом.

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- изменение: nested map, перестановка, удаление, toggle одного id, no-op.
- identity: копировать изменённую ветвь, сохранять соседей, вернуть исходную ссылку при no-op, вернуть новый массив даже при no-op.

Форма ответа: Полное объявление запрошенной функции/запроса, без Markdown и объяснений; типы и окружение заданы в prompt.

## Полные предметные примеры

### 1. nested_cell_mutated_after_shallow_copy

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Cell={value:number;label:string}; type State={cells:Record<string,Cell>;selected:string}; Напиши function setValue(s:State,key:string,v:number):State. key гарантированно существует; новый State, новый cells и новый изменённый Cell; остальные Cell сохраняют ссылки. selected и label сохранить; исходные объекты не менять. Контроль: cells.a={value:1,label:"A"}, cells.b={value:2,label:"B"}, selected="b"; key="a",v=9. Только функцию.
```

**Chosen**

```text
function setValue(s: State, key: string, v: number): State { return { ...s, cells: { ...s.cells, [key]: { ...s.cells[key], value: v } } }; }
```

**Rejected**

```text
function setValue(s: State, key: string, v: number): State { const cells = { ...s.cells }; cells[key].value = v; return { ...s, cells }; }
```

Различие: Rejected сохраняет shared Cell и меняет исходное значение.

Будущий различающий контроль: Сравнить old/new Cell, cells и State; old.cells.a.value должен остаться 1. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.01`, семейство `v6.ref.code-ng-immutable.nested-map-value`; split `reference`, training_allowed `false`.

### 2. target_index_reinterpreted_after_removal

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Item={id:number}; Напиши function move(xs:readonly Item[],from:number,to:number):Item[]. Оба индекса в диапазоне исходного массива. Удалить элемент from и вставить в итоговую позицию to; новый массив, те же Item refs, xs неизменён. Контроль ids=[1,2,3,4],from=0,to=2. Только функцию.
```

**Chosen**

```text
function move(xs: readonly Item[], from: number, to: number): Item[] { const out = [...xs]; const [item] = out.splice(from, 1); out.splice(to, 0, item); return out; }
```

**Rejected**

```text
function move(xs: readonly Item[], from: number, to: number): Item[] { const out = [...xs]; const [item] = out.splice(from, 1); out.splice(to > from ? to - 1 : to, 0, item); return out; }
```

Различие: Rejected считает to позицией исходного gap, хотя контракт задаёт итоговый индекс.

Будущий различающий контроль: Проверить индекс перемещённого id=1, неизменность xs и те же Item refs. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.02`, семейство `v6.ref.code-ng-immutable.move-item-order`; split `reference`, training_allowed `false`.

### 3. noop_identity_lost

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Item={id:number}; Напиши function remove(xs:readonly Item[],id:number):readonly Item[]. Если id отсутствует, вернуть ТОТ ЖЕ массив; если есть, вернуть новый без него. ids уникальны, элементы не менять. Контроль ids=[2,4],id=9. Только функцию.
```

**Chosen**

```text
function remove(xs: readonly Item[], id: number): readonly Item[] { return xs.some(x => x.id === id) ? xs.filter(x => x.id !== id) : xs; }
```

**Rejected**

```text
function remove(xs: readonly Item[], id: number): readonly Item[] { return xs.filter(x => x.id !== id); }
```

Различие: Оба массива содержат те же элементы; rejected нарушает только no-op reference contract.

Будущий различающий контроль: При отсутствующем id проверить result===xs, а при присутствующем — новый массив. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.03`, семейство `v6.ref.code-ng-immutable.absent-delete-policy`; split `reference`, training_allowed `false`.

### 4. noop_copy_contract_ignored

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Item={id:number}; Напиши function remove(xs:readonly Item[],id:number):Item[]. Всегда вернуть НОВЫЙ массив без данного id, даже когда id отсутствует. Item refs сохранить, xs не менять. Контроль ids=[2,4],id=9. Только функцию.
```

**Chosen**

```text
function remove(xs: readonly Item[], id: number): Item[] { return xs.filter(x => x.id !== id); }
```

**Rejected**

```text
function remove(xs: readonly Item[], id: number): Item[] { return xs.some(x => x.id === id) ? xs.filter(x => x.id !== id) : xs as Item[]; }
```

Различие: Обратный контроль: сохранение root reference теперь неверно по явному договору.

Будущий различающий контроль: Проверить result!==xs и сохранение ссылок элементов при no-op. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.04`, семейство `v6.ref.code-ng-immutable.absent-delete-policy`; split `reference`, training_allowed `false`.

### 5. toggle_replaced_by_set_true

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Item={id:number;done:boolean}; Напиши function toggle(xs:readonly Item[],id:number):Item[]. Новый массив; только элемент с данным id заменить новым объектом с инверсией done; остальные объекты сохранить. ids уникальны, id существует. Контроль [(1,false),(2,true)],id=2. xs и его элементы не менять. Только функцию.
```

**Chosen**

```text
function toggle(xs: readonly Item[], id: number): Item[] { return xs.map(x => x.id === id ? { ...x, done: !x.done } : x); }
```

**Rejected**

```text
function toggle(xs: readonly Item[], id: number): Item[] { return xs.map(x => x.id === id ? { ...x, done: true } : x); }
```

Различие: Rejected корректно копирует нужную ветвь, но не выполняет инверсию.

Будущий различающий контроль: Проверить инверсию true→false и исходный xs[1].done=true. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.05`, семейство `v6.ref.code-ng-immutable.single-toggle`; split `reference`, training_allowed `false`.

### 6. neighbor_bucket_dropped

**Задача ученику**

```text
Симуляция Angular 16.2.0 / TypeScript 5.1.6; функция меняет передаваемое как input состояние. Это задача о ссылках и значениях; фактический UI render не измеряется. type Bucket={items:readonly number[];title:string}; type State={buckets:Record<string,Bucket>;busy:boolean}; Напиши function append(s:State,key:string,n:number):State. key существует. Добавить n к items только выбранного bucket, сохранив title, все другие buckets и busy. Создать новые State,buckets,изменённый Bucket и items; старое не менять. Контроль buckets.a={items:[1],title:"A"}, buckets.b={items:[2],title:"B"},busy=true; key="a",n=3. Только функцию.
```

**Chosen**

```text
function append(s: State, key: string, n: number): State { const b = s.buckets[key]; return { ...s, buckets: { ...s.buckets, [key]: { ...b, items: [...b.items, n] } } }; }
```

**Rejected**

```text
function append(s: State, key: string, n: number): State { const b = s.buckets[key]; return { ...s, buckets: { [key]: { ...b, items: [...b.items, n] } } }; }
```

Различие: Rejected не мутирует данные, но теряет незатронутый bucket b.

Будущий различающий контроль: Проверить наличие b, сохранение его ссылки и старое a.items=[1]. Статус: **NOT_RUN**.

Пара `CODE-NG-IMMUTABLE.REF.06`, семейство `v6.ref.code-ng-immutable.map-neighbor-retention`; split `reference`, training_allowed `false`.

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
| `CODE-NG-IMMUTABLE/base` | 135 | `OR-AUTO-TIME-WINDOW/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-NG-IMMUTABLE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-IMMUTABLE/base" -Run "S606" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-IMMUTABLE/base/S606`. Следующий поток: `OR-AUTO-TIME-WINDOW/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-IMMUTABLE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-IMMUTABLE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
