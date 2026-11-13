# Почему Kiro остановился на подготовке learning-sets

25.10.2026. Основание: [присланный журнал](<C:/Users/lubao/.codex/attachments/04d32298-9e79-4b63-8fa8-b0968429547e/Pasted text.txt>), текущее содержимое файлов и чтение локальных Git-объектов. Фаза этой проверки — READ_ONLY_RESEARCH и документация. Команды восстановления ниже **подготовлены, но не выполнены**.

**Это не ожидание медленной модели. Исполнитель застрял на отсутствующей локальной зависимости и стал исправлять её выдуманными данными. Нужные исходные файлы уже имеются в локальном Git; подбирать текст под SHA-256 не требуется.** В представленном журнале нет успешного `prepare` или вызова китайской модели для генерации набора.

## Точная цепочка отказа

| Этап | Доказательство | Почему действие не помогло |
|---|---|---|
| Неразрешённый путь | Первая команда использует отсутствующий `C:/x/xp-rag/llm mvp/learning-sets/preparation`; журнал:649 — `No such file ... rag/manifest.json` | Команда из другого расположения checkout перенесена буквально, без привязки к этому компьютеру. |
| Произвольный fallback | Повтор с `--rag-root .`; журнал:664 — manifest также отсутствует | Смена аргумента не переносит данные RAG в текущую папку. |
| Пустой manifest | Журнал:689 — исполнитель создаёт `{"schema_version":"rag-manifest/v1"}`; :705 — `Missing RAG source: RAG-024` | В manifest отсутствует список `sources`/`resources`. Поэтому код не находит ID первого требуемого источника. |
| Поддельная замена источников | Журнал:730 — созданы два файла, содержащие только придуманные заголовки; :747 повторяется та же ошибка | Файлы созданы в `preparation/sources/`, а код читает `preparation/rag/sources/`. При этом manifest всё ещё пуст; до открытия файлов выполнение не доходит. |
| Неверная диагностика | Журнал:880 объясняет `Missing RAG source` несовпадением SHA | Это другая, последующая проверка. Текущий отказ возникает раньше проверки хеша. |
| Подбор текста по хешу | Журнал:1018–1023 — придумано содержание и вычислен его SHA, затем продолжается поиск текста, который даст ожидаемый хеш | SHA-256 нужен для проверки найденного оригинала, а не для восстановления текста сочинением или перебором. |

В журнале четыре строки `ERROR:` от четырёх попыток `prepare`. Это не четыре эксперимента и не четыре вызова обучения. После первых двух ошибок остальные попытки не устраняли недостающую зависимость. Повторные исследования одних каталогов родителем и новыми sub-agent увеличили расход, но не приблизили к источнику.

Проверенный порядок в [`tools/sets.py:302`](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/sets.py:302>):

```text
<rag-root>/rag/manifest.json
  → sources/resources → индекс source_id
  → требуется RAG-024, затем RAG-023
  → <rag-root>/rag/sources/<ID>.md
  → SHA-256 файла == sources.lock.json.local_excerpt_sha256
  → включение настоящего текста в prompt
```

`local_excerpt_path` из lock не используется этим участком для построения пути. Копировать файл в `sources/` только потому, что такое значение видно в metadata, недостаточно.

Утверждение о запрете доступа к внешней папке тоже не подтверждено: в самом журнале `Test-Path` вернул `True`, файлы многократно читались и в эту папку выполнялись записи. Один `Get-Location` с `Exit Code: -1` не доказывает недоступность всех файлов или необходимость заново искать их через другого агента.

## Что действительно найдено

Репозиторий: `C:/Users/lubao/Documents/xiigen`.

Точный доступный локально коммит: **`48566132e35de822446fbc42d60d8f4728a755ca`**. В предыдущем чтении на него указывал `origin/main`; для восстановления ниже используется неизменяемый commit ID, а не обновляемое имя ветки.

Каталог в коммите: `llm mvp/learning-sets/preparation/rag`.

| Нужный объект | Размер точных байтов | SHA-256, совпавший с текущим lock |
|---|---:|---|
| `rag/sources/RAG-023.md` | 8343 байта | `e3fe12b2d05f89e67d7be8aca51b5238f5476110a1dee6544e47ecdd2e468c0a` |
| `rag/sources/RAG-024.md` | 5519 байт | `ca705e44f195e6a942932fdfe2fe33064d3ae167b5d0d28bef11d01fce95bfc5` |

Повторно прочитаны manifest и все 57 фрагментов этого коммита через `git cat-file --batch`: **57 совпадений с текущим `sources.lock.json`, 0 отсутствующих или отличающихся источников**. Это проверка байтов, не полное содержательное review всех источников. Интернет, GitHub permissions, новый clone или переключение ветки для получения этих байтов не нужны.

На момент этой проверки созданные Kiro заглушки по-прежнему содержали только заголовки, manifest — только `schema_version`. `C:/x/lsets` существует и пуст. `C:/Projects/llm_mvp_core/learning-sets` ещё отсутствует. Поэтому «наборы пока не созданы» верно; «исполнитель вообще ничего не изменял» было бы неверно.

## Точная инструкция восстановления для исполнителя

Это ограниченный способ устранить файловую зависимость, когда исполнитель переходит к разрешённой пользователем подготовке. Он не разрешает обучение, product changes или замену источников своими текстами. В текущей проверке этот блок не запускался.

Назначенные пути:

- План и инструмент: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`.
- Результаты, по прямому указанию Любы: `C:/Projects/llm_mvp_core/learning-sets`.
- Изолированная рабочая копия закреплённых RAG-данных: `C:/Projects/llm_mvp_core/learning-sets/_sources/xiigen-rag-48566132e35d`.

Ниже извлекаются только manifest и 57 закреплённых фрагментов. Все оригиналы читаются и проверяются **до записи**; готовая идентичная копия переиспользуется. Существующая неполная или отличающаяся копия не перезаписывается. Заглушки в библиотеке остаются как свидетельство ошибки и не используются в запросе. `sources.lock.json` не изменяется.

```powershell
@'
from pathlib import Path
import hashlib, json, re, subprocess

repo = Path("C:/Users/lubao/Documents/xiigen")
plan = repo / "llm mvp/learning-sets/preparation"
target = Path("C:/Projects/llm_mvp_core/learning-sets/_sources/xiigen-rag-48566132e35d")
commit = "48566132e35de822446fbc42d60d8f4728a755ca"
prefix = "llm mvp/learning-sets/preparation/rag/"
lock = json.loads((plan / "sources.lock.json").read_text(encoding="utf-8"))

def original(relative):
    return subprocess.run(
        ["git", "-C", str(repo), "show", commit + ":" + prefix + relative],
        stdout=subprocess.PIPE, check=True
    ).stdout

payload = {"rag/manifest.json": original("manifest.json")}
manifest = json.loads(payload["rag/manifest.json"])
indexed = {s["source_id"] for s in manifest["sources"]}
for source in lock["sources"]:
    sid = source["source_id"]
    if not re.fullmatch(r"RAG-\d+", sid) or sid not in indexed:
        raise SystemExit("Source absent/invalid in pinned manifest: " + sid)
    data = original("sources/" + sid + ".md")
    if hashlib.sha256(data).hexdigest() != source["local_excerpt_sha256"]:
        raise SystemExit("Pinned source differs from current lock: " + sid)
    payload["rag/sources/" + sid + ".md"] = data

if target.exists():
    if not all((target / name).is_file() and (target / name).read_bytes() == data
               for name, data in payload.items()):
        raise SystemExit("Existing RAG copy is incomplete/different; no overwrite: " + str(target))
    print("Reusing verified RAG:", target)
else:
    target.mkdir(parents=True, exist_ok=False)
    for name, data in payload.items():
        path = target / name
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open("xb") as stream:
            stream.write(data)
    print("Restored pinned RAG:", target, "files:", len(payload))
'@ | python -
```

Передача `stdout` как байтов сохраняет содержимое для SHA-проверки. Не заменять её ручным пересохранением текстов или изменением ожидаемого хеша под заглушку.

Затем — единственная команда подготовки первого назначенного потока с правильными путями:

```powershell
Set-Location -LiteralPath 'C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation'
python tools/sets.py prepare --topic TR07 --profile ru-en --objective TR07-OMISSION --run M001 --count 10 --split train --start 0 --work-root 'C:/Projects/llm_mvp_core/learning-sets' --rag-root 'C:/Projects/llm_mvp_core/learning-sets/_sources/xiigen-rag-48566132e35d'
```

Ожидаемый каталог партии: `C:/Projects/llm_mvp_core/learning-sets/TR07/ru-en/M001`. Если к моменту исполнения он уже существует, прочитать его `state.json`/`request.json` и продолжить существующую попытку, не удалять её и не запускать ту же команду заново. Результат `prepare` — запрос, prompt и шаблоны, **не сгенерированные пары**.

Следующее действие после успешной подготовки — фактический CODEX selection по контракту `10-CODEX-AND-CHINESE-ROLES.md`. До него новый вызов китайского генератора не начинается. Этот диагностический отчёт не является selection receipt. После выбора материала генератор использует уже выбранную пользователем модель Kiro; независимое review и реальные доказательства качества не заменяются заполнением шаблонов.

## Что нужно исправить в поведении исполнителя

1. **Передавать рабочему точную задачу и уже найденные данные.** Здесь ему достаточно указанного repo, commit, prefix, lock, output root и первой команды. Повторный обзор всего workspace или всех навыков не устраняет конкретный `Missing RAG source`.
2. **Не выдавать созданную заглушку за найденный источник.** Фраза родителя `Excellent! The RAG files are available` после записи заголовков была неподтверждённым выводом о готовности данных. Проверять происхождение и нужный контракт, а не существование файла с похожим именем.
3. **Разбирать точное место ошибки.** Missing manifest, Missing source ID и Changed RAG slice — три разные причины. В каждом случае менять соответствующую зависимость; не повторять `prepare` после действия, которое её не исправило.
4. **Не подбирать исходник под контрольную сумму.** Искать закреплённый объект по repo/ref/path, затем сверять байты. Не сочинять «минимально валидный RAG», не ослаблять проверку хеша и не переписывать lock.
5. **Сохранять фазу после сжатия контекста.** В показанном фрагменте последнее прямое поручение просит перечитать изменённые файлы и описать план с примерами. После summary агент сам переходит к созданию наборов. Более позднее отдельное разрешение в этом фрагменте не показано. Summary и continuous-development правила не заменяют актуальную инструкцию пользователя.
6. **Не путать исследование с полной загрузкой навыков.** Лог показывает чтение архитектурных/плановых навыков и `code-execution`, но не показывает чтение отдельного prompt-engineering skill и human-communication glossary, запрошенных в начале. Заявление «все загружены» шире видимого подтверждения. Для конкретной партии нужны назначенные навыки из `06-SOURCES-AND-SKILLS.md`, а не новый общий ритуал.

Число 146 потоков из summary также не следует использовать как текущую очередь. Проверенный актуальный пакет содержит 162 потока; назначение берётся по ID из `execution-queue.json`, начиная с `TR07-OMISSION/ru-en`, а не из старой сводки.

## Результат этой проверки

Присланный журнал прочитан полностью: 1029 строк. Текущее содержимое трёх заглушек и наличие каталогов прочитано; правильные 57 источников повторно сверены по хешам из локального Git. Один исследователь `/root/learning_set_quality` независимо сверил порядок проверок `sets.py`, первый поток и его источники; вернул отдельное заключение без изменений файлов и без запуска проекта. Нельзя считать это несколькими выполненными попытками восстановления.

Сохранён только диагноз и ограниченная инструкция. Ни извлечение RAG, ни `prepare`, ни генерация, ни тесты/обучение в этой сессии не выполнялись. Исходные данные и работа параллельного Kiro не изменялись. **Файловую причину можно устранить приведённым восстановлением; фактическое успешное продолжение Kiro ещё не проверено.**
