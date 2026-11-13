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
  "stream_id": "DOC-NG-LATEST/base",
  "topic_id": "DOC-NG-02",
  "objective_id": "DOC-NG-LATEST",
  "domain_id": "docs.angular",
  "profile_id": "base",
  "run_id": "D309",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-NG-LATEST/base/F0000000",
    "v3/DOC-NG-LATEST/base/F0000001",
    "v3/DOC-NG-LATEST/base/F0000002",
    "v3/DOC-NG-LATEST/base/F0000003",
    "v3/DOC-NG-LATEST/base/F0000004",
    "v3/DOC-NG-LATEST/base/F0000005",
    "v3/DOC-NG-LATEST/base/F0000007",
    "v3/DOC-NG-LATEST/base/F0000009",
    "v3/DOC-NG-LATEST/base/F0000010",
    "v3/DOC-NG-LATEST/base/F0000013"
  ],
  "family_contracts": {
    "v3/DOC-NG-LATEST/base/F0000000": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000000",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000001": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000001",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000002": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000002",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000003": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000003",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000004": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000004",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000005": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000005",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000007": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000007",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000009": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000009",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000010": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000010",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-LATEST/base/F0000013": {
      "origin_family_id": "v3/DOC-NG-LATEST/base/F0000013",
      "leakage_group_ids": [
        "v3/DOC-NG-LATEST/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 14,
  "sources": [
    "RAG-027",
    "RAG-037",
    "RAG-039"
  ],
  "card": "curriculum/cards/DOC-NG-LATEST.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-NG-LATEST/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-NG-LATEST.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-LATEST\\base\\D309\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-037": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-037.md",
      "RAG-039": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-039.md"
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
  "objective_id": "DOC-NG-LATEST",
  "topic_id": "DOC-NG-02",
  "title_ru": "Документировать актуальность последнего Observable",
  "learning_goal_ru": "DOC: предсказывать выход последнего активного inner, сохраняя уже доставленные значения и корректное завершение. CODE: составить выражение с явно указанным switchMap; эти пары сами по себе не доказывают самостоятельный выбор flattening operator.",
  "generation_prompt_ru": "Один навык switchMap/latest-inner. Дай полностью расписанные virtual frames, типы Observable и поведение каждого cold inner. Rejected содержит mergeMap либо точный trace его поведения; все остальные условия одинаковы. CODE-этап здесь явно называет switchMap и учит составлению выражения по указанному оператору; выбор оператора из намерения без подсказки требует отдельной микроцели и её собственного pilot, не приписывай его этим примерам. Нельзя опираться на реальные таймеры/сеть. Проверяй overlap, раннее уже доставленное значение, пустой новый inner и outer completion при активном последнем inner. Не включай catchError в эту микроцель. Отписка — прекращение подписки в данном потоке; не утверждать rollback сервера или физическую отмену работы.\n\nРаботай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все исходные данные, версии, видимый код и ограничения ответа находятся в prompt; metadata ученик не получает. Chosen и rejected имеют одинаковый формат, а ошибочная сторона нарушает одну проверяемую границу. Не добавляй [WRONG], score, rationale или source ID в обучаемые ответы. Источники задают методику; авторские симуляции не выдавать за цитаты реального проекта, результаты китайской модели либо измеренный эффект обучения. Публичные reference-примеры и их потомки не помещать в DEV/EXAM. Семейство одного исходника общее для документации, кода и перефразирования; переименование и новые числа не создают независимости. Для новых пар сначала назначить происхождение и split; не наполнять квоту поверхностными дубликатами. Пиши отчёт на русском по 08-REPORT-TEMPLATE.md. Используй фактическую текущую модель Kiro без override, не угадывай имя/версию. Если точная модель не показана, сохраняй наблюдаемый selector и model_identity_observed=false. При явном NEXT сразу переходи к указанному пользователем ID; не возвращайся к прежней карточке. При STOP немедленно прекрати новые действия. Старые эксперименты и версии данных не удаляй; production не меняй. Один долгий процесс имеет один handle: обнаружь штатный wait/resume и дождись terminal в текущем раунде; не запускай заново вместо ожидания, не делай busy polling и не объявляй таймаут по отсутствию первых файлов. Oracle до фактического исполнения имеет NOT_RUN, result_observed=false. Runtime oracle сейчас NOT_RUN; это не препятствует записи кандидатов и не означает PASS. Учебный пилот позже восстанавливает совместимое обученное B1 с tokenizer/optimizer/RNG/split hashes; новое обучение B1 только по назначенному отдельному bootstrap-плану, с сохранением для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показать рядом. Успешный опыт документируется кандидатом в исправления; перенос в production не выполняется автоматически.",
  "response_contract_ru": "DOC: JSON-массив next emissions {frame,value}. CODE: одно выражение Observable.",
  "scenario_axes": [
    {
      "axis": "старый inner",
      "values": [
        "ещё не выдавал",
        "уже выдал одно",
        "должен выдать после новой подписки"
      ]
    },
    {
      "axis": "новый inner",
      "values": [
        "значение",
        "пустое завершение",
        "дольше outer"
      ]
    }
  ],
  "forbidden_mixing": [
    "catchError",
    "debounceTime как второе изменение",
    "серверный rollback",
    "Angular lifecycle без показанного кода",
    "wall-clock benchmark"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-NG-LATEST.REF.01",
      "profile_id": "base",
      "family_id": "v3.ref.ng.switch.latest-pending",
      "prompt": "Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) — cold inner со значением a через 5 frames после подписки и completion через 6. load(B) — cold inner со значением b через 1 frame и completion через 2. Ошибок нет. Код: query$.pipe(switchMap(q => load(q))). Дай JSON-массив объектов {frame,value} для всех next emissions в порядке доставки. Completion в массив не включать; объяснений не добавлять.",
      "chosen": "[{\"frame\":3,\"value\":\"b\"}]",
      "rejected": "[{\"frame\":3,\"value\":\"b\"},{\"frame\":5,\"value\":\"a\"}]",
      "error_tag": "stale_inner_emission",
      "rationale": "Новый B отписывает ещё активный A; a не проходит в выход.",
      "counterexample": "Без switchMap старый a приходит в frame 5 после b и делает UI устаревшим.",
      "source_refs": [
        "RAG-027",
        "RAG-037",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16 / RxJS 7.8",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-NG-02/001",
        "leakage_group_ids": [
          "v3.ref.ng.switch.latest-pending",
          "reference/DOC-NG-02/001",
          "reference/CODE-NG-02/001"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "rxjs7.8.TestScheduler",
          "timeline": "query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) — cold inner со значением a через 5 frames после подписки и completion через 6. load(B) — cold inner со значением b через 1 frame и completion через 2. Ошибок нет.",
          "expected_next": [
            {
              "frame": 3,
              "value": "b"
            }
          ],
          "rejected_next": [
            {
              "frame": 3,
              "value": "b"
            },
            {
              "frame": 5,
              "value": "a"
            }
          ],
          "assertion": "Создать cold inners и hot outer в TestScheduler; assert next emissions и subscription/unsubscription frames. Это проверка Observable, не серверного rollback. Для новых семей обязательно также ненулевое успешное emission."
        },
        "derived_from": [
          "DOC-NG-02.REF.001",
          "CODE-NG-02.REF.001"
        ]
      }
    },
    {
      "example_id": "DOC-NG-LATEST.REF.02",
      "profile_id": "base",
      "family_id": "v3.ref.ng.switch.emitted-value-retained",
      "prompt": "Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 4 и завершается в frame 10. load(A) выдаёт a1 через 1 frame и a2 через 7 frames после подписки, completion через 8. load(B) выдаёт b через 2 frames после подписки, completion через 3. Ошибок нет. Код: query$.pipe(switchMap(q => load(q))). Дай JSON-массив объектов {frame,value} для всех next emissions в порядке доставки. Completion в массив не включать; объяснений не добавлять.",
      "chosen": "[{\"frame\":1,\"value\":\"a1\"},{\"frame\":6,\"value\":\"b\"}]",
      "rejected": "[{\"frame\":1,\"value\":\"a1\"},{\"frame\":6,\"value\":\"b\"},{\"frame\":7,\"value\":\"a2\"}]",
      "error_tag": "stale_inner_emission",
      "rationale": "Уже доставленный a1 остаётся фактом истории; после B дальнейший a2 от старого inner исключён.",
      "counterexample": "Отписка не стирает ранний a1, но обязана убрать поздний a2.",
      "source_refs": [
        "RAG-027",
        "RAG-037",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16 / RxJS 7.8",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-NG-02/001",
        "leakage_group_ids": [
          "v3.ref.ng.switch.emitted-value-retained",
          "reference/DOC-NG-02/001",
          "reference/CODE-NG-02/001"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "rxjs7.8.TestScheduler",
          "timeline": "query$ выдаёт A в frame 0, B в frame 4 и завершается в frame 10. load(A) выдаёт a1 через 1 frame и a2 через 7 frames после подписки, completion через 8. load(B) выдаёт b через 2 frames после подписки, completion через 3. Ошибок нет.",
          "expected_next": [
            {
              "frame": 1,
              "value": "a1"
            },
            {
              "frame": 6,
              "value": "b"
            }
          ],
          "rejected_next": [
            {
              "frame": 1,
              "value": "a1"
            },
            {
              "frame": 6,
              "value": "b"
            },
            {
              "frame": 7,
              "value": "a2"
            }
          ],
          "assertion": "Создать cold inners и hot outer в TestScheduler; assert next emissions и subscription/unsubscription frames. Это проверка Observable, не серверного rollback. Для новых семей обязательно также ненулевое успешное emission."
        },
        "derived_from": [
          "DOC-NG-02.REF.001",
          "CODE-NG-02.REF.001"
        ]
      }
    },
    {
      "example_id": "DOC-NG-LATEST.REF.03",
      "profile_id": "base",
      "family_id": "v3.ref.ng.switch.last-inner-after-outer-complete",
      "prompt": "Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 3. load(A) выдаёт a через 5 frames, completion через 6; load(B) выдаёт b через 4 frames после подписки, completion через 5. Ошибок нет. Код: query$.pipe(switchMap(q => load(q))). Дай JSON-массив объектов {frame,value} для всех next emissions в порядке доставки. Completion в массив не включать; объяснений не добавлять.",
      "chosen": "[{\"frame\":6,\"value\":\"b\"}]",
      "rejected": "[{\"frame\":5,\"value\":\"a\"},{\"frame\":6,\"value\":\"b\"}]",
      "error_tag": "stale_inner_emission",
      "rationale": "Завершение outer не отменяет последний inner: b проходит в frame 6, output завершается в frame 7.",
      "counterexample": "A должен быть отписан в frame 2; completion outer в frame 3 не означает немедленный completion результата.",
      "source_refs": [
        "RAG-027",
        "RAG-037",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16 / RxJS 7.8",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-NG-02/001",
        "leakage_group_ids": [
          "v3.ref.ng.switch.last-inner-after-outer-complete",
          "reference/DOC-NG-02/001",
          "reference/CODE-NG-02/001"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "rxjs7.8.TestScheduler",
          "timeline": "query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 3. load(A) выдаёт a через 5 frames, completion через 6; load(B) выдаёт b через 4 frames после подписки, completion через 5. Ошибок нет.",
          "expected_next": [
            {
              "frame": 6,
              "value": "b"
            }
          ],
          "rejected_next": [
            {
              "frame": 5,
              "value": "a"
            },
            {
              "frame": 6,
              "value": "b"
            }
          ],
          "assertion": "Создать cold inners и hot outer в TestScheduler; assert next emissions и subscription/unsubscription frames. Это проверка Observable, не серверного rollback. Для новых семей обязательно также ненулевое успешное emission."
        },
        "derived_from": [
          "DOC-NG-02.REF.001",
          "CODE-NG-02.REF.001"
        ]
      }
    },
    {
      "example_id": "DOC-NG-LATEST.REF.04",
      "profile_id": "base",
      "family_id": "v3.ref.ng.switch.new-empty-inner",
      "prompt": "Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) выдаёт a через 5 frames, completion через 6. load(B) завершается через 1 frame без значений. Ошибок нет. Код: query$.pipe(switchMap(q => load(q))). Дай JSON-массив объектов {frame,value} для всех next emissions в порядке доставки. Completion в массив не включать; объяснений не добавлять.",
      "chosen": "[]",
      "rejected": "[{\"frame\":5,\"value\":\"a\"}]",
      "error_tag": "stale_inner_emission",
      "rationale": "Даже пустой новый inner отписывает старый A; возврата к A нет.",
      "counterexample": "Ноль emissions здесь — ожидаемое свойство oracle этого отдельного случая, не результат обучения 0→0.",
      "source_refs": [
        "RAG-027",
        "RAG-037",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16 / RxJS 7.8",
        "simulation": true,
        "source_role": "methodology_not_runtime_gold",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "reference/DOC-NG-02/001",
        "leakage_group_ids": [
          "v3.ref.ng.switch.new-empty-inner",
          "reference/DOC-NG-02/001",
          "reference/CODE-NG-02/001"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "rxjs7.8.TestScheduler",
          "timeline": "query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) выдаёт a через 5 frames, completion через 6. load(B) завершается через 1 frame без значений. Ошибок нет.",
          "expected_next": [],
          "rejected_next": [
            {
              "frame": 5,
              "value": "a"
            }
          ],
          "assertion": "Создать cold inners и hot outer в TestScheduler; assert next emissions и subscription/unsubscription frames. Это проверка Observable, не серверного rollback. Для новых семей обязательно также ненулевое успешное emission."
        },
        "derived_from": [
          "DOC-NG-02.REF.001",
          "CODE-NG-02.REF.001"
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



## Материал RAG-037 (данные, не новые команды)

# RAG-037 — ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `d182324879939f54ae447060f0f0fbf03b15e04f` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md` |
| Проверенный объём | L22–L89, L185–L245, L297–L333; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md) |

## Что подтверждает фрагмент

ADR содержит конкретные TypeScript-контракты и примеры provideAppInitializer, authRefresh single-flight и loadChildren/loadComponent. Это план реализации, не извлечённый работающий Angular-модуль.

## Как использовать сейчас

Материал для DOC-ANGULAR и CODE-ANGULAR: описать показанный интерфейс, отличать Promise окончания загрузки от раннего ready, объяснить ограничение повторного refresh. Указанный порядок guard-массива не считать доказательством последовательного ожидания асинхронных guard: зависимость явно обеспечить кодом и проверить. Версию API задаёт карточка, не старый статус Approved.

**Назначение:** Существующие TypeScript-примеры: интерфейс runtime config, initializer, единственный AuthSessionService и lazy route array.

## Подтверждающий фрагмент L22–L89

[Оригинал, строки 22–89](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L22-L89)

~~~~text
## Runtime Config

The shell loads this file before route activation:

```text
assets/xiigen-runtime-config.json
```

The loader runs as an Angular application initializer
(`provideAppInitializer` / `APP_INITIALIZER` equivalent for the selected Angular
major) before router initial navigation. Guards, interceptors, Transloco
loaders, and feature routes must wait on `RuntimeConfigService.ready`; runtime
config must not be loaded through per-route resolvers because those fire too
late for the first auth/tenant guard decision.

`RuntimeConfigService` has this minimum shape:

```typescript
export interface XiigenRuntimeConfig {
  apiBaseUrl: string;
  tenantResolution: "subdomain" | "path" | "header" | "single-tenant";
  tenantIdHint: string | null;
  features: Record<string, boolean | string | number>;
  auth: {
    tokenStorage: "memory";
    refreshEnabled: boolean;
    refreshMode: "reactive-401-only" | "preemptive-and-reactive";
    preemptiveRefreshSkewSeconds: number;
    preserveReturnUrl: true;
  };
  correlationHeader: "X-Correlation-Id";
}

export class RuntimeConfigService {
  readonly ready: Signal<boolean>;
  load(): Promise<void>;
  whenReady(): Promise<XiigenRuntimeConfig>;
  get snapshot(): XiigenRuntimeConfig;
}
```

The application initializer returns `runtimeConfigService.load()`. Guards and
interceptors call `whenReady()` when they may run before `ready()` is true.
`snapshot` throws a fail-closed configuration error if accessed before load
completion.

The Angular bootstrap sequence has one required initializer in the first
implementation:

```typescript
provideAppInitializer(() => inject(RuntimeConfigService).load())
```

No token refresh, tenant resolution network call, feature flag reload, or
context-role resolver may run as a bootstrap initializer. Those depend on the
runtime config and therefore run through guards/interceptors after
`RuntimeConfigService.ready` is true. `FeatureFlagService.reload(config)` is
called synchronously from the runtime config load success path. This keeps
initial navigation deterministic: config first, router second, auth/tenant
guards third.

`RuntimeConfigService` is a root singleton:

```typescript
@Injectable({ providedIn: "root" })
export class RuntimeConfigService { ... }
```

~~~~
## Подтверждающий фрагмент L185–L245

[Оригинал, строки 185–245](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L185-L245)

~~~~text
## HTTP Interceptors

Use functional interceptors:

- `authInterceptor: HttpInterceptorFn` adds `Authorization: Bearer <token>`
  when `AuthSessionService` has an in-memory access token.
- `tenantInterceptor: HttpInterceptorFn` adds `X-Tenant-Id` only when the
  server-side tenant resolution mode requires a header.
- `correlationInterceptor: HttpInterceptorFn` adds `X-Correlation-Id` from
  `CorrelationContextService` or creates one for the command/request.
- `authRefreshInterceptor: HttpInterceptorFn` performs one bounded refresh on
  401 when refresh is enabled; otherwise it clears session state and routes to
  login/denied.

`tenantInterceptor` reads `RuntimeConfigService.snapshot.tenantResolution`.
It adds the `X-Tenant-Id` header only when all of these are true:

- `tenantResolution` is `"header"`;
- `tenantIdHint` is present and non-empty;
- the request URL targets `apiBaseUrl` or a relative API URL owned by XIIGen;
- the request is not a third-party telemetry, asset, translation-bundle, or
  external provider request.

For `subdomain`, `path`, or `single-tenant`, the interceptor must not add
`X-Tenant-Id`; the server derives tenant scope from the approved resolution
source. A runtime config with `tenantResolution = "header"` and no
`tenantIdHint` fails schema validation before interceptors run.

JWT access tokens are stored in memory by default. Persistent token storage
requires a separate security ADR.

`AuthSessionService` is the only token source for interceptors:

```typescript
@Injectable({ providedIn: "root" })
export class AuthSessionService {
  readonly accessToken: Signal<string | null>;
  readonly refreshInProgress: Signal<boolean>;
  hasToken(): boolean;
  getAccessToken(): string | null;
  setSession(response: AuthLoginResponse): void;
  clearSession(reason: "logout" | "expired" | "refresh-failed"): void;
  refreshOnce(): Promise<boolean>;
}
```

Generated code must not read tokens from `RuntimeConfigService`, browser local
storage, session storage, cookies, or component state.

Refresh handling is single-flight and loop-safe:

1. `authRefreshInterceptor` catches a 401 only once per original request.
2. It marks the retried request with an internal `XIIGEN_REFRESH_RETRY` context
   token so the same request cannot trigger a second refresh loop.
3. If another refresh is already in progress, it awaits that in-memory promise
   instead of calling `/auth/refresh` again.
4. On refresh success, it updates `AuthSessionService` and retries the original
   request once.
5. On refresh failure, missing refresh token, or second 401 after retry, it
   clears the session and routes to login with the preserved `returnUrl`.
6. Refresh requests themselves are excluded from refresh interception.
~~~~
## Подтверждающий фрагмент L297–L333

[Оригинал, строки 297–333](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L297-L333)

~~~~text
## Lazy Loading

The platform shell lazy-loads each flow as a route array:

```typescript
{
  path: "flows/FLOW-XX-<slug>",
  canActivate: [
    configLoadedGuard,
    tenantGuard,
    authGuard,
    roleGuard,
    contextRoleGuard
  ],
  loadChildren: () => import("./flows/FLOW-XX-<slug>/routes")
    .then(m => m.FLOW_XX_ROUTES)
}
```

Inside a flow route array, leaf screens use `loadComponent`. Use
`loadChildren` at the flow boundary and `loadComponent` for leaf views. This
keeps generated packages consistent while allowing fine-grained screen loading.

Guard activation order is fixed and fail-closed:

1. `configLoadedGuard` awaits `RuntimeConfigService.whenReady()` and redirects
   to a configuration-error route on validation/load failure.
2. `tenantGuard` resolves tenant context from the approved runtime-config mode.
3. `authGuard` verifies an in-memory access token or redirects to login with
   `returnUrl`.
4. `roleGuard` checks canonical role snapshots for route policies.
5. `contextRoleGuard` calls the server/context-role resolver when the policy
   requires business-state-derived roles.

Routes may omit `roleGuard` or `contextRoleGuard` only when the auth policy
registry marks the endpoint/screen as not requiring those gates. Routes must
not put auth, role, or context-role guards before `configLoadedGuard`.
~~~~



## Материал RAG-039 (данные, не новые команды)

# RAG-039 — ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `a14589d9182228162b009ad8c30e7cd7a854d068` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md` |
| Проверенный объём | L24–L73; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md) |

## Что подтверждает фрагмент

Документ выбирает standalone bootstrap, functional guards, явные imports; flow boundary использует loadChildren, leaf screen — loadComponent. В нём есть условный выбор Angular21/22 на момент создания.

## Как использовать сейчас

Можно учить соблюдению заданного проектного контракта. Не использовать старые даты поддержки и условный latest patch как утверждение о текущем Angular; для генерации фиксировать конкретный version_profile. SSR-out-of-scope относится к тому проекту, не ко всему Angular.

**Назначение:** Историческая проектная стратегия standalone components, route arrays и lazy loading.

## Подтверждающий фрагмент L24–L73

[Оригинал, строки 24–73](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md#L24-L73)

~~~~text
## Decision

Use standalone Angular components, standalone route definitions, and functional
guards for the .NET/Angular translation.

The provisional major-version rule is:

- If scaffolding begins before Angular 22.0 is released, pin Angular 21 at the
  latest supported patch version and keep the templates Angular-22-ready.
- If Angular 22.0 is released before scaffolding begins, refresh this ADR and
  pin Angular 22 unless a concrete incompatibility is found.
- If Gate 0 is approved after 2026-05-19, repeat the official release schedule
  check before writing `package.json`.

## Standalone Template Requirements

P-10 must generate:

- `bootstrapApplication(...)`, not root NgModule bootstrap;
- `provideRouter(...)` route arrays with lazy `loadComponent` or
  `loadChildren` where appropriate;
- `CanActivateFn` guards for tenant, auth, role, and context-role checks;
- standalone components with explicit `imports` arrays;
- Transloco providers in application configuration;
- direction switching through the i18n shell service.

NgModule wrappers are allowed only for a third-party library that requires
them. Such wrappers must stay in the platform shell or adapter package, not in
per-flow generated UI code.

Lazy loading rule: use `loadChildren` at the flow package boundary so every
flow exports a route array. Use `loadComponent` for leaf screens inside that
flow route array. This keeps generated flow packages consistent.

Rendering rule: the initial translation is client-side SPA only. Angular SSR
and prerendering are out of scope until a later ADR changes the Playwright and
hosting evidence rules.

## Acceptance Criteria

P-10 is not complete until:

- the Angular package versions are pinned under the version-pin ADR;
- generated flow components are standalone;
- no generated flow package requires an NgModule for routing;
- generated flow boundaries use `loadChildren` and leaf screens use
  `loadComponent`;
- SSR is disabled unless a later ADR replaces this decision;
- role and context-role guards use functional guard APIs and fail closed;
- Hebrew RTL screenshots pass with the standalone Transloco setup.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-NG-LATEST\base\D309\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-LATEST\base\D309\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-LATEST\base\D309\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-NG-LATEST/base`. Не запускать review, accept, export, oracle или обучение.
