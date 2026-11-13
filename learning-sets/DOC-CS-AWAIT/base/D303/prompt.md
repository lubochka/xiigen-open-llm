# Создание одной партии DPO в Kiro — CREATE_ONLY

## Прямое поручение: продолжать до 192 000 пар

Текущая партия — часть кампании **192 потоков / минимум 192 000 реально созданных пар**, а не отдельная цель с новым запросом разрешения после finish. После `DRAFT_CREATED` немедленно выполняй следующее назначение; после `END` начни следующий обход недовыполненных потоков. Короткий промежуточный отчёт не завершает рабочий ход. STOP/NEXT пользователя имеют приоритет, реальный технический блокер сообщается точно.

**Не задавай вопросы «продолжать?», «коммитить или копить?», «10 или 25–50?»** Размер уже задан request/`candidate_count`; не увеличивай его ради скорости. Если публикация явно назначена — соблюдай её без переспроса; иначе сохраняй локально и продолжай. Не придумывай дополнительные отчёты, скрипты или мониторинг. Для новой партии того же stream используй новый run и `-Previous` от последнего завершённого request; сохраняй `next_start` и полные группы. Подробности, счёт без дублей и округление router 1000→1008: [контракт продолжения](../27-CAMPAIGN-CONTINUATION-CONTRACT.md). Старая accepted-квота 132 000 не является ограничением новой кампании кандидатов.

Уточнение для конечного файла сценариев: `-Previous` продолжает тот же family-plan. Когда подготовлен новый общий chunk, использовать его настоящий путь через `-FamilyPlan`, новый run и `-Start 0` **без `-Previous`**. Если материал ещё недоступен текущей роли, продолжать другие доступные потоки; не обнулять cursor старого файла и не выдавать повторные сюжеты за новые пары. Порядок подготовки следующих chunk сохранён в контракте продолжения.

Visible operational explanations and brief action rationales must remain in English. Do not switch them to Chinese. Reports for Luba and metadata rationale remain in Russian; learner examples follow their assigned language profile. Do not produce long thought monologues. Repetitive or incoherent output is a failed response, not progress or training data; recover from the existing request/state without restarting the campaign. This governs visible output, not hidden reasoning.

Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

## Что ты создаёшь для обучения XIIGEN

Ты — автор учебных данных. Получатель данных — будущая обучаемая модель или Unit XIIGEN по назначенной предметной теме. Твой результат — реальные DPO-тройки `prompt/chosen/rejected` в `draft.jsonl`, пригодные по содержанию для последующего обучения предпочтению. Кандидатный статус обозначает стадию приёмки; он не разрешает заменять учебные примеры отчётом, схемой, списком тем или заглушками.

- `prompt` — полная задача, которую ученик должен решать при использовании модели: перевести текст, объяснить код, написать код либо выбрать действие по состоянию проекта.
- `chosen` — полный желаемый ответ ученика на этот prompt.
- `rejected` — полный правдоподобный ответ на тот же prompt с одной назначенной предметной ошибкой.
- Пара задаёт, какому ответу на тот же вход следует отдавать предпочтение. Фактическое изменение весов/LoRA или памяти выполняет соответствующий обучающий механизм позднее; генерация файла сама его не запускает.

Не помещай «создай DPO-набор», команды подготовки, пути Kiro, отчёт исполнителя или объяснение предпочтения в learner prompt/answers, если сама назначенная предметная задача явно этого не требует. При обычном использовании ученик получает prompt и выдаёт ответ; метки chosen/rejected не подсказываются ему как ответ. Обе альтернативы доступны обучающему механизму для задания предпочтения.

Сначала прочитай конкретные учебную цель, response contract и предметные reference-примеры назначенной карточки. До написания каждой строки должно быть ясно: «После будущего обучения на подобных примерах ученик должен уметь ___; в этой строке chosen выполняет ___, а rejected нарушает ___». Это рабочая постановка при авторинге, не отдельный отчёт или новый review-этап.

### Простая самопроверка на примерах

Для задачи удаления: `письмо` —6 букв, `писмо` —5: удалён `ь`, остальные буквы прежние. `пильмо` —6: это замена `с` на `л`, не удаление. Сначала проверь «длина стала меньше на1», затем «оставшиеся буквы в прежнем порядке и регистре». `Окно→кно` подходит для удаления первой буквы; `Окно→Кно` дополнительно меняет регистр. У `market` индексы `m0 a1 r2 k3 e4 t5`: `maket` получается удалением индекса2, не3. Даже правильная длина не доказывает правильный индекс.

Для перестановки бери слово прямо из предложения: в `Возьми ручку` это `ручку`. Обмен `ч/к` даёт `рукчу`,5→5 букв. `рукча` не подходит: изменилось ещё окончание. Обратная операция должна точно восстановить слово из предложения. Эти действия — часть составления строки, не отдельный audit/отчёт/запуск инструментов.

Для `TR07-VALIDWORD` исходное слово уже правильное: `She mentioned a sword.` → `Она упомянула меч.`; rejected `Она упомянула слово.`. Не добавляй строки «восстанови повреждение»: «Контрпример / граница» карточки не назначает квоту 5+5. Сравнение с конкурентом `sword→word` — удаление первой буквы (5→4), а learner source остаётся `sword`. Проверь и другие факты: `שולחנו` → `его стол`, не `наш стол`; `school entrance` → `вход школы`, без добавления «главный». Подробные разборы и источник форм иврита — в шпаргалке ниже.

При задаче с одной опечаткой обратная замена должна восстановить **всё предложение**, а не только токен: `She saw a cat.` → `She saw a cap.` сохраняет остальной текст; `She saw the cap near the door.` дополнительно меняет артикль и место.

Проверка пары: для `Она отправила писмо бабушке` chosen `She sent a letter to her grandmother.` и rejected `She sent a parcel to her grandmother.` различаются предметом. Rejected `She mailed a letter to her grandmother.` не создаёт такого контраста: это другой допустимый перевод. Rationale не может сделать правильный перевод ошибочным.

Полные хорошие/плохие разборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md`. Это публичная подсказка автору, не новые строки корпуса. Совпадающий F-номер у двух профилей не означает один сюжет. Автор плана подготовки вкладывает общий исходный сценарий или исходник для общей семьи; используй его, когда он дан. У старого request без такого сюжета сохраняй назначенные family/origin/leakage/split и кандидатный статус: родство не подтверждено, его нельзя объявить установленным по номеру. Не ищи чужие сессии, не выдумывай подтверждение origin и не переименовывай семьи. Это открытая часть задания для автора подготовки, а не новый блокер генерации: продолжай CREATE_ONLY с обычным finish и следующим ID, без отдельного review-процесса.

**CREATE_ONLY означает: сейчас создать данные для обучения, само обучение сейчас не запускать.** Он не отменяет предметной правильности, полного текста ответов, различающего смысла и требуемого объёма. `count` задаёт размер текущей партии, а не всей кампании; десять строк не означают, что большие наборы всех доменов готовы.

### Как выглядит учебное содержание

Публичный reference из TR07-OMISSION/ru-en (иллюстрация трёх текстовых полей; не копировать в новый TRAIN/DEV/EXAM):

```json
{
  "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
  "chosen": "The ticket is on the table.",
  "rejected": "The receipt is on the table."
}
```

Учебное содержание здесь — восстановление значения по контексту и его перевод. Ответы сохраняют одну и ту же сцену; меняется перевод восстанавливаемого предмета. Это не урок о том, как оформлять JSON. Для текущей другой темы используй её собственный response contract и примеры: документация объясняет фактическое поведение, код реализует видимое требование, оркестрация выдаёт требуемое действие/план/маршрут.

`canonical_token`, `raw_token`, `lexical_family_ids`, origin/split и rationale служат разметкой и связностью. Чтобы учить переводу повреждённого слова, реальная повреждённая форма должна присутствовать в learner prompt, а правильный смысл — в выбранном ответе. Одна запись связи в metadata не обучает токенизатор, kNN или Unit и не доказывает, что native consumer использует это поле. Не раскрывай скрытый canonical-ответ ученику ради упрощения задачи.

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Наборы создаются только под `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`; инструкции — в соседнем `preparation`. Текущая папка Kiro или `llm_mvp_core` не меняет эти значения. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Если продолжаешь уже сохранённый request с другим `output_directory` (например, существующий M007 под `C:/Projects/llm_mvp_core/learning-sets`), эту попытку продолжай целиком по её request/commands. Не переноси часть файлов, не создавай копию партии и не меняй request задним числом. Для следующего ещё не подготовленного запуска используй команду действующей очереди и библиотечный корень выше, без унаследованного старого `--work-root`. Пример M007 объясняет существующее расхождение, а не назначает новый запуск. [Правило двух ситуаций](../20-CREATE-ONLY-AND-PATHS.md).

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.



## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "DOC-CS-AWAIT/base",
  "topic_id": "DOC-CS-02",
  "objective_id": "DOC-CS-AWAIT",
  "domain_id": "docs.csharp",
  "profile_id": "base",
  "run_id": "D303",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-CS-AWAIT/base/F0000000",
    "v3/DOC-CS-AWAIT/base/F0000001",
    "v3/DOC-CS-AWAIT/base/F0000002",
    "v3/DOC-CS-AWAIT/base/F0000004",
    "v3/DOC-CS-AWAIT/base/F0000005",
    "v3/DOC-CS-AWAIT/base/F0000007",
    "v3/DOC-CS-AWAIT/base/F0000008",
    "v3/DOC-CS-AWAIT/base/F0000009",
    "v3/DOC-CS-AWAIT/base/F0000010",
    "v3/DOC-CS-AWAIT/base/F0000011"
  ],
  "family_contracts": {
    "v3/DOC-CS-AWAIT/base/F0000000": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000000",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000001": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000001",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000002": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000002",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000004": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000004",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000005": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000005",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000007": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000007",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000008": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000008",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000009": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000009",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000010": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000010",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-AWAIT/base/F0000011": {
      "origin_family_id": "v3/DOC-CS-AWAIT/base/F0000011",
      "leakage_group_ids": [
        "v3/DOC-CS-AWAIT/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 12,
  "sources": [
    "RAG-027",
    "RAG-028",
    "RAG-029"
  ],
  "card": "curriculum/cards/DOC-CS-AWAIT.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-CS-AWAIT/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-CS-AWAIT.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-AWAIT\\base\\D303\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-028": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-028.md",
      "RAG-029": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-029.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "role_contract": {
    "selection": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_RUN_IN_THIS_CREATION_STAGE"
    },
    "generation": {
      "actor": "external_open_chinese_model_via_kiro",
      "model_policy": "Use the selected Chinese model; inherit the current configuration without override. Record observed identity or unknown."
    },
    "quality_assessment": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_REQUESTED_IN_CREATE_ONLY"
    },
    "creation_blocked_by_review": false,
    "identity_scope": "Requested roles, not evidence of actual model execution or completed selection/review."
  },
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "DOC-CS-AWAIT",
  "topic_id": "DOC-CS-02",
  "title_ru": "Документировать побочный эффект после await",
  "learning_goal_ru": "Различать эффекты до await, только после успеха, в finally и в конкретном catch; сохранять outcome зависимости.",
  "generation_prompt_ru": "Составляй один маленький async wrapper с полностью показанными trace-эффектами и одним await. Каждая пара учит одну границу success/fault/cancel либо конкретного catch. Fault в fixture — конкретный InvalidOperationException; для typed catch дополнительно FormatException. Cancellation создаётся SetCanceled, а не SetException(OperationCanceledException): async wrapper может преобразовать faulted Task с cancellation exception в Canceled, поэтому общий тезис о неизменности Task status без ограничения не допускается. trace.Add не бросает. Не смешивай с передачей CancellationToken, конкурентностью потоков, сетевым retry или реальной БД. Chosen для DOC описывает каждый путь показанного кода, для CODE реализует явно заданные пути. Rejected компилируется, но переносит ровно один эффект или расширяет один catch. Обязательно pending→success и pending→Faulted(InvalidOperationException)/Canceled, иначе ранний эффект может пройти незамеченным.\n\nРаботай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все исходные данные, версии, видимый код и ограничения ответа находятся в prompt; metadata ученик не получает. Chosen и rejected имеют одинаковый формат, а ошибочная сторона нарушает одну проверяемую границу. Не добавляй [WRONG], score, rationale или source ID в обучаемые ответы. Источники задают методику; авторские симуляции не выдавать за цитаты реального проекта, результаты китайской модели либо измеренный эффект обучения. Публичные reference-примеры и их потомки не помещать в DEV/EXAM. Семейство одного исходника общее для документации, кода и перефразирования; переименование и новые числа не создают независимости. Для новых пар сначала назначить происхождение и split; не наполнять квоту поверхностными дубликатами. Пиши отчёт на русском по 08-REPORT-TEMPLATE.md. Используй фактическую текущую модель Kiro без override, не угадывай имя/версию. Если точная модель не показана, сохраняй наблюдаемый selector и model_identity_observed=false. При явном NEXT сразу переходи к указанному пользователем ID; не возвращайся к прежней карточке. При STOP немедленно прекрати новые действия. Старые эксперименты и версии данных не удаляй; production не меняй. Один долгий процесс имеет один handle: обнаружь штатный wait/resume и дождись terminal в текущем раунде; не запускай заново вместо ожидания, не делай busy polling и не объявляй таймаут по отсутствию первых файлов. Oracle до фактического исполнения имеет NOT_RUN, result_observed=false. Runtime oracle сейчас NOT_RUN; это не препятствует записи кандидатов и не означает PASS. Учебный пилот позже восстанавливает совместимое обученное B1 с tokenizer/optimizer/RNG/split hashes; новое обучение B1 только по назначенному отдельному bootstrap-плану, с сохранением для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показать рядом. Успешный опыт документируется кандидатом в исправления; перенос в production не выполняется автоматически.",
  "response_contract_ru": "DOC: русский текст с trace и outcome. CODE: только требуемое объявление метода.",
  "scenario_axes": [
    {
      "axis": "размещение эффекта",
      "values": [
        "after success",
        "finally",
        "typed catch",
        "before+after await"
      ]
    },
    {
      "axis": "завершение",
      "values": [
        "pending",
        "SetResult success",
        "FormatException in typed-catch case",
        "InvalidOperationException",
        "SetCanceled"
      ]
    }
  ],
  "forbidden_mixing": [
    "retry",
    "thread scheduling performance",
    "настоящие БД/HTTP",
    "подмена outcome только потому, что UI хочет success"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-CS-AWAIT.REF.01",
      "profile_id": "base",
      "family_id": "v3.ref.cs.await.success-audit",
      "prompt": "Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Save(Func<Task> store, List<string> trace) { await store(); trace.Add(\"saved\"); } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.",
      "chosen": "При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved не добавляется; Task Save завершается соответственно faulted/canceled.",
      "rejected": "При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved тоже добавляется; Task Save завершается соответственно faulted/canceled.",
      "error_tag": "wrong_async_side_effect_path",
      "rationale": "Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.",
      "counterexample": "Начальный trace=[]; store — pending TaskCompletionSource. До SetResult trace пуст; после успеха [saved]. SetException(new InvalidOperationException()) даёт trace=[] и Faulted; SetCanceled() даёт trace=[] и Canceled. Faulted Task с OperationCanceledException здесь не создаётся.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-029"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-CS-02/003",
        "leakage_group_ids": [
          "v3.ref.cs.await.success-audit",
          "reference/DOC-CS-02/003",
          "reference/CODE-CS-02/003"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "fixture": "Начальный trace=[]; store — pending TaskCompletionSource. До SetResult trace пуст; после успеха [saved]. SetException(new InvalidOperationException()) даёт trace=[] и Faulted; SetCanceled() даёт trace=[] и Canceled. Faulted Task с OperationCanceledException здесь не создаётся.",
          "allowed_faults": [
            "InvalidOperationException"
          ],
          "cancel_creation": "TaskCompletionSource.SetCanceled, not SetException(OperationCanceledException)",
          "assertion": "Использовать TaskCompletionSource с RunContinuationsAsynchronously; снять trace до завершения, await завершение wrapper, затем снять trace/outcome. Faulted inputs содержат только явно разрешённые non-cancellation exceptions. Без Thread.Sleep и ожидания по времени."
        },
        "derived_from": [
          "DOC-CS-02.REF.003",
          "CODE-CS-02.REF.003"
        ]
      }
    },
    {
      "example_id": "DOC-CS-AWAIT.REF.02",
      "profile_id": "base",
      "family_id": "v3.ref.cs.await.finally-cleanup",
      "prompt": "Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Use(Func<Task> work, List<string> trace) { try { await work(); } finally { trace.Add(\"cleanup\"); } } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.",
      "chosen": "cleanup добавляется один раз в finally при успехе, InvalidOperationException и отменённой Task work. Wrapper завершается соответственно RanToCompletion/Faulted/Canceled.",
      "rejected": "cleanup добавляется один раз только при успехе work. При InvalidOperationException и отменённой Task cleanup отсутствует; wrapper завершается соответственно Faulted/Canceled.",
      "error_tag": "wrong_async_side_effect_path",
      "rationale": "Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.",
      "counterexample": "SetException(new InvalidOperationException()): trace после await равен [cleanup], wrapper Faulted. SetCanceled(): [cleanup], wrapper Canceled. SetResult(): [cleanup], wrapper RanToCompletion. Faulted Task с OperationCanceledException не создаётся.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-029"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-CS-02/003",
        "leakage_group_ids": [
          "v3.ref.cs.await.finally-cleanup",
          "reference/DOC-CS-02/003",
          "reference/CODE-CS-02/003"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "fixture": "SetException(new InvalidOperationException()): trace после await равен [cleanup], wrapper Faulted. SetCanceled(): [cleanup], wrapper Canceled. SetResult(): [cleanup], wrapper RanToCompletion. Faulted Task с OperationCanceledException не создаётся.",
          "allowed_faults": [
            "InvalidOperationException"
          ],
          "cancel_creation": "TaskCompletionSource.SetCanceled, not SetException(OperationCanceledException)",
          "assertion": "Использовать TaskCompletionSource с RunContinuationsAsynchronously; снять trace до завершения, await завершение wrapper, затем снять trace/outcome. Faulted inputs содержат только явно разрешённые non-cancellation exceptions. Без Thread.Sleep и ожидания по времени."
        },
        "derived_from": [
          "DOC-CS-02.REF.003",
          "CODE-CS-02.REF.003"
        ]
      }
    },
    {
      "example_id": "DOC-CS-AWAIT.REF.03",
      "profile_id": "base",
      "family_id": "v3.ref.cs.await.typed-catch",
      "prompt": "Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Только для этой пары дополнительно проверяется SetException(new FormatException()) — Faulted у зависимости. Внутри класса дан метод: async Task Record(Func<Task> work, List<string> trace) { try { await work(); trace.Add(\"ok\"); } catch (FormatException) { trace.Add(\"format\"); } } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.",
      "chosen": "При успехе добавляется ok. FormatException заменяется записью format и успешным завершением Record. InvalidOperationException даёт Faulted, а отменённая Task — Canceled без изменения trace.",
      "rejected": "При успехе добавляется ok. FormatException, InvalidOperationException и отменённая Task заменяются записью format и успешным завершением Record.",
      "error_tag": "wrong_async_side_effect_path",
      "rationale": "Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.",
      "counterexample": "Четыре отдельные pending задачи: SetResult→[ok]/RanToCompletion; SetException(new FormatException())→[format]/RanToCompletion; SetException(new InvalidOperationException())→[]/Faulted; SetCanceled→[]/Canceled. Faulted Task с OperationCanceledException не создаётся.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-029"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-CS-02/003",
        "leakage_group_ids": [
          "v3.ref.cs.await.typed-catch",
          "reference/DOC-CS-02/003",
          "reference/CODE-CS-02/003"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "fixture": "Четыре отдельные pending задачи: SetResult→[ok]/RanToCompletion; SetException(new FormatException())→[format]/RanToCompletion; SetException(new InvalidOperationException())→[]/Faulted; SetCanceled→[]/Canceled. Faulted Task с OperationCanceledException не создаётся.",
          "allowed_faults": [
            "FormatException",
            "InvalidOperationException"
          ],
          "cancel_creation": "TaskCompletionSource.SetCanceled, not SetException(OperationCanceledException)",
          "assertion": "Использовать TaskCompletionSource с RunContinuationsAsynchronously; снять trace до завершения, await завершение wrapper, затем снять trace/outcome. Faulted inputs содержат только явно разрешённые non-cancellation exceptions. Без Thread.Sleep и ожидания по времени."
        },
        "derived_from": [
          "DOC-CS-02.REF.003",
          "CODE-CS-02.REF.003"
        ]
      }
    },
    {
      "example_id": "DOC-CS-AWAIT.REF.04",
      "profile_id": "base",
      "family_id": "v3.ref.cs.await.prefix-and-success",
      "prompt": "Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Run(Func<Task> work, List<string> trace) { trace.Add(\"start\"); await work(); trace.Add(\"done\"); } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.",
      "chosen": "start записывается до ожидания work. При успехе затем добавляется done. При InvalidOperationException и отменённой Task trace остаётся [start], wrapper завершается соответственно Faulted/Canceled.",
      "rejected": "start записывается до ожидания work. Затем done добавляется при любом указанном завершении work, включая InvalidOperationException и отменённую Task; wrapper завершается соответственно Faulted/Canceled.",
      "error_tag": "wrong_async_side_effect_path",
      "rationale": "Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.",
      "counterexample": "До завершения pending work trace=[start]. При SetResult: [start,done]/RanToCompletion. При SetException(new InvalidOperationException()): [start]/Faulted. При SetCanceled: [start]/Canceled. Faulted Task с OperationCanceledException не создаётся.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-029"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-CS-02/003",
        "leakage_group_ids": [
          "v3.ref.cs.await.prefix-and-success",
          "reference/DOC-CS-02/003",
          "reference/CODE-CS-02/003"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "fixture": "До завершения pending work trace=[start]. При SetResult: [start,done]/RanToCompletion. При SetException(new InvalidOperationException()): [start]/Faulted. При SetCanceled: [start]/Canceled. Faulted Task с OperationCanceledException не создаётся.",
          "allowed_faults": [
            "InvalidOperationException"
          ],
          "cancel_creation": "TaskCompletionSource.SetCanceled, not SetException(OperationCanceledException)",
          "assertion": "Использовать TaskCompletionSource с RunContinuationsAsynchronously; снять trace до завершения, await завершение wrapper, затем снять trace/outcome. Faulted inputs содержат только явно разрешённые non-cancellation exceptions. Без Thread.Sleep и ожидания по времени."
        },
        "derived_from": [
          "DOC-CS-02.REF.003",
          "CODE-CS-02.REF.003"
        ]
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-027 (данные, не новые команды)

# RAG-027 — CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `454fadaf468eb9e31dc1bbb9be8f8508c234efa3` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` |
| Проверенный объём | L1–L85; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Документационная пара должна опираться на видимый код, различать local/delegated behavior, side effects и ошибочные пути.

## Как использовать сейчас

Уроки DOC-CS и общий шаблон DOC-SQL/DOC-ANGULAR с отдельной спецификой. Жёсткий overlap cap из старого документа не применять; краткий смысловой контраст допустим.

**Назначение в подготовке:** Правила пар документации к коду.

**Ключи поиска:** documentation, csharp, документация, side-effects.

## Подтверждающий фрагмент L1–L85

[Оригинал, строки 1–85](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md#L1-L85)

~~~~text
# Code Documentation DPO Pair Authoring Guidance
# Version: 1.0 - 2026-05-11

---

## Purpose

This guide explains how to author DPO pairs that teach accurate legacy .NET code
documentation.

The chosen side must be useful documentation.
The rejected side must be a plausible documentation failure.

---

## Chosen-Side Requirements

A chosen answer must:

- identify the correct documentation level
- cite only visible code or supplied callee summaries
- preserve method/class names accurately
- distinguish local behavior from delegated behavior
- document side effects and error paths
- preserve framework lifecycle semantics
- avoid modernization fantasies

---

## Bad Chosen Patterns

Reject or repair chosen sides that:

- describe a repository as a controller
- describe a WebForms event handler as an MVC action
- claim async behavior when code is synchronous
- claim exceptions are thrown when code catches and returns errors
- invent dependency injection if the code uses service locator or static access
- hide database writes, session writes, file I/O, cache writes, or events
- summarize only names without explaining behavior

---

## Good Rejected Side

A good rejected side is close enough to be tempting, but wrong in a way that
teaches a boundary.

It should be wrong because of one primary defect:

- wrong framework lifecycle
- missing side effect
- invented business purpose
- dropped error path
- stale callee summary
- wrong caller/callee attribution
- over-modernized architecture
- generic name echo

---

## Overlap Guidance

Target rejected overlap with chosen:

- 0.25-0.55 for normal pairs
- below 0.60 always

Do not use a rejected side that is just the chosen side with "not" inserted.

---

## A-Side Evidence Requirements

Every A-side must include enough evidence for the chosen side:

- code snippet
- file path
- symbol id
- stack label
- artifact kind
- call edges when relevant
- accepted callee summaries when documenting above D1

If the evidence is not present, the pair must not target that claim.
~~~~



## Материал RAG-028 (данные, не новые команды)

# RAG-028 — CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `6f8f48d7125e24fed8d8caee69954857583ea8b1` |
| Путь | `llm mvp/documentation learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md` |
| Проверенный объём | L7–L54, L111–L161; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md) |

## Что подтверждает фрагмент

Маршрутизация учитывает runtime/framework/version и уровень D1–D4; обобщение caller требует видимых callee summaries.

## Как использовать сейчас

Уроки DOC-CS: не путать .NET Framework и .NET Core, AngularJS и Angular; не выдумывать hidden callee. Source version фиксируется до генерации.

**Назначение в подготовке:** Версионность стека и доказательство переходов вызовов.

**Ключи поиска:** documentation, version, call-graph, версии, csharp, angular.

## Подтверждающий фрагмент L7–L54

[Оригинал, строки 7–54](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L7-L54)

~~~~text
## Purpose

This guide closes the gap between broad "document this project" routing and the
actual documentation behavior needed for legacy C# systems.

The documentation flow must route by:

- server/client side
- .NET runtime family
- target framework version
- inferred C# language version
- web/UI framework and version when present
- artifact kind
- documentation level D1-D4
- bottom-up call transit state

It is not enough to choose "C# server documentation." A .NET Framework 4.6
WebApi2 controller with AngularJS 1.4 client code must not be documented as an
ASP.NET Core 3.1 controller, and an ASP.NET Core 3.1 controller must not be
documented as a WebForms page.

---

## Required Version-Aware Routing Fields

Every source row, request, DPO pair, held-out row, and report must include:

```json
{
  "app_side": "server | client | shared | mixed",
  "runtime_family": "NET_FRAMEWORK | NET_CORE | NET_STANDARD | UNKNOWN",
  "target_framework": "net46 | netcoreapp3.1 | netstandard2.0 | ...",
  "target_framework_source": "csproj | packages.config | project.assets.json | inferred",
  "csharp_language_version": "CSharp5 | CSharp6 | CSharp8 | explicit:<value> | inferred_unknown",
  "language_version_confidence": "exact | inferred_high | inferred_review_required",
  "project_style": "legacy_non_sdk | sdk_style | mixed",
  "web_framework": "WebForms | WebApi2 | ASP.NET Core MVC | ASP.NET Core WebApi | ASMX | none | unknown",
  "web_framework_version": "System.Web.Http 5.2.3 | Microsoft.AspNetCore.App 3.1 | ...",
  "frontend_framework": "AngularJS | React | WPF | WinForms | none | unknown",
  "frontend_framework_version": "1.4.8 | ...",
  "stack_label": "NETFX_WEBAPI | NETCORE_WEBAPI | DATA_ACCESS_EF | ...",
  "artifact_kind": "method | controller_action | helper_method | repository_method | page_event | workflow",
  "documentation_level": "D1 | D2 | D3 | D4"
}
```

If a value is inferred, the evidence must name the file and field that produced
the inference.
~~~~

## Подтверждающий фрагмент L111–L161

[Оригинал, строки 111–161](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L111-L161)

~~~~text
## Bottom-Up Call-Transit Gate

Documentation must be built like a compiler pass.

### Stage T1 - Leaf Method Summary

Document methods whose behavior is locally visible and does not require a known
callee summary. The output becomes an accepted `CalleeSummary`.

### Stage T2 - Helper Or Component Summary

Document a helper or component only after the methods it calls are documented or
included in the A-side. The output must separate:

- local control flow
- delegated behavior
- parse/validation/error decisions inherited from callees

### Stage T3 - Caller / Entry-Point Summary

Document controller actions, ASMX endpoints, page events, and workflow entry
points only after all important helper summaries are available. The caller may
claim callee behavior only by citing the accepted `CalleeSummary`.

### Stage T4 - Workflow / Module Summary

Document the user-visible workflow only after the caller chain is documented.
The workflow summary must preserve data flow, side effects, error paths, and
framework lifecycle constraints.

### Stage T-F - Framework Lifecycle Support

Small customer projects may have almost no project-defined business call graph.
ASP.NET Core template-like projects are still valid documentation targets, but
their transit evidence is the framework lifecycle:

- `Program.Main` -> host builder -> `Startup`
- `Startup.ConfigureServices` -> service/controller activation
- `Startup.Configure` -> middleware and endpoint routing
- endpoint dispatch -> controller action
- response serialization -> expression-bodied properties or model getters

Rows for this case must be marked with:

- `artifact_kind`: `framework_lifecycle_surface`
- `source_root_role`: `framework_lifecycle_root`
- `documentation_level`: usually `D4` for host/startup/endpoint propagation

They must not invent product/domain workflow. If the only code is the default
weather forecast template, the correct documentation is framework lifecycle plus
visible response construction, not a fictional game-server domain.
~~~~



## Материал RAG-029 (данные, не новые команды)

# RAG-029 — CODE-DOC-DPO-REJECTED-PROMPTS-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `ce343a61697d94ffae064373dfce3bb16a80d87b` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-REJECTED-PROMPTS-v1.md` |
| Проверенный объём | L19–L79, L116–L138; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-REJECTED-PROMPTS-v1.md) |

## Что подтверждает фрагмент

Промпты задают одну правдоподобную ошибку документации. Поздние наблюдения предупреждают о подсказке в префиксе chosen/rejected и обучении на prompt tokens.

## Как использовать сейчас

Оставить один основной дефект и общий формат ответов. [WRONG]-подсказку вынести из train text; не заменять изменение полезного показателя обязательным порогом accuracy 0.75.

**Назначение в подготовке:** Ошибки описаний и контрасты rejected.

**Ключи поиска:** documentation, negative, loss-mask, ошибки, подсказки.

## Подтверждающий фрагмент L19–L79

[Оригинал, строки 19–79](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-REJECTED-PROMPTS-v1.md#L19-L79)

~~~~text
## Prompt 1 - Method-Level Rejected Side

```text
Create one rejected documentation answer for this C# method.

The rejected answer must be plausible to a junior developer but wrong in exactly
one primary way.

Choose one rejected type:
- generic_name_echo
- missing_side_effect
- dropped_error_path
- invented_business_purpose
- wrong_async_claim
- wrong_framework_lifecycle

The rejected answer must include:
- a natural documentation paragraph
- [WRONG: weakness - what cannot be learned - functional consequence.]

Do not make the rejected side identical to the chosen side.
Do not use a one-word or joke answer.
```

---

## Prompt 2 - Caller/Callee Rejected Side

```text
Create one rejected documentation answer for a caller method that has supplied
callee summaries.

The rejected answer must make exactly one caller/callee reasoning mistake:
- attributes callee behavior to the caller
- ignores an accepted callee summary
- uses a stale callee summary
- drops failure propagation
- claims a call path that is not present

End with a specific [WRONG: ...] annotation that names the consequence for
maintenance or refactoring.
```

---

## Prompt 3 - Framework Lifecycle Rejected Side

```text
Create one rejected documentation answer that confuses the legacy framework
lifecycle.

Allowed mistakes:
- WebForms Page_Load treated as MVC action
- HttpModule treated as controller middleware
- MVC action treated as background job
- WPF event handler treated as web request handler
- ASP.NET Core controller behavior projected onto ASP.NET Framework MVC

The rejected answer must look realistic and must include the specific
[WRONG: ...] annotation.
```
~~~~

## Подтверждающий фрагмент L116–L138

[Оригинал, строки 116–138](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-REJECTED-PROMPTS-v1.md#L116-L138)

~~~~text
## Runner Findings - 2026-05-15

The code documentation DPO runner found that negative examples fail when the
rejected side is either too weak or not trained through the answer tokens.

Observed failure modes:

- `incomplete_trace` as a short partial list: the rejected answer was obviously
  incomplete, so it did not teach a realistic maintenance mistake.
- `invented_callee` with a generic service name: the fake dependency was too
  generic and did not force source-evidence checking.
- different chosen/rejected labels in the tokenized answer prefix: the model
  could learn the label shape instead of the content defect.
- prompt tokens trained as answer tokens: long prompts diluted the actual
  chosen/rejected contrast.

Required runner behavior:

- use the same answer prefix for chosen and rejected;
- mask prompt tokens and train only answer tokens;
- persist `dpo-pair-diagnostics.jsonl` for every run;
- group diagnostics by rejected type before adding new rows;
- never publish a unit when held-out DPO accuracy is below `0.75`.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-CS-AWAIT\base\D303\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-CS-AWAIT\base\D303\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-CS-AWAIT\base\D303\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-CS-AWAIT/base`. Не запускать review, accept, export, oracle или обучение.
