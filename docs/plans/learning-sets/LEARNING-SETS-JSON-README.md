# Learning-sets: обучающие JSON под Git LFS

Добавлено 2026-07-31 по директиве Любы. Назначение — хранить обучающие наборы
(training sets) для моделей как **закоммиченные JSON по прямому, видимому пути**,
с большими файлами под Git LFS. Никаких «runtime data, not committed», никаких
скрытых/незакоммиченных мест — данные лежат здесь, в репозитории, и пушатся.

---

## 1. Где это лежит (прямой путь)

```
xiigen-planning/llm mvp/learning-sets/<имя_набора>.json
```

Это существующая, отслеживаемая git-папка (здесь же лежат FLOW-01 training-data
документы). JSON-наборы кладутся сюда напрямую. Путь **виден** в рабочем дереве и в
`git ls-files`; он **не** покрыт ни одним `.gitignore` (проверено).

Управляет и коммитит `xiigen-planning` **только Люба** — этот файл и `.gitattributes`
подготовлены, но НЕ закоммичены; коммит делает Люба (см. §5).

---

## 2. Соглашение об именовании

```
<domain-name>_<model-type>_<batch-num>_<batch-size>_<date>.json
```

| Поле | Что | Формат | Пример |
|------|-----|--------|--------|
| `domain-name` | доменный слаг флоу | kebab-case (`-`, не `_`) | `user-registration` |
| `model-type`  | тип модели / обучения | kebab-case | `dpo`, `sft`, `lora`, `qwen3-4b` |
| `batch-num`   | номер батча | целое, с ведущими нулями (≥3 знака) | `001`, `012` |
| `batch-size`  | число примеров в батче | целое | `512`, `1024` |
| `date`        | дата набора | ISO `YYYY-MM-DD` | `2026-07-31` |

Разделитель между пятью полями — `_`. Внутри полей используется `-` (не `_`), поэтому
пять полей однозначно разбираются по `_`.

**Пример полного имени:**

```
user-registration_dpo_001_512_2026-07-31.json
marketplace-payments_sft_003_2048_2026-08-02.json
```

**Regex-валидатор имени:**

```
^[a-z0-9]+(-[a-z0-9]+)*_[a-z0-9-]+_[0-9]{3,}_[0-9]+_[0-9]{4}-[0-9]{2}-[0-9]{2}\.json$
```

Тонкие места, которые можно поменять одним словом (сейчас выбраны разумные умолчания):
дата ISO `YYYY-MM-DD` (в репо есть и компактный вид `20260508` в именах папок);
ведущие нули у `batch-num` — 3 знака; строго `.json` (или добавить `.jsonl`, см. §3).

---

## 3. Как это устроено под LFS

`.gitattributes` в этой папке помечает `*.json` (и как страховку `*.jsonl`) под LFS:

```
*.json  filter=lfs diff=lfs merge=lfs -text
*.jsonl filter=lfs diff=lfs merge=lfs -text
```

Что происходит при коммите:
1. По прямому пути `learning-sets/<name>.json` в git коммитится **маленький указатель
   (pointer)** — он виден и отслеживается.
2. Реальные байты кладутся в LFS-хранилище (локально `.git/lfs/objects`, на удалёнке —
   GitHub LFS) и **пушатся** вместе с веткой (pre-push hook уже стоит).

То есть путь остаётся прямым и видимым, файл — закоммичен и запушен; «скрыто» —
только внутреннее хранилище байтов, что и есть штатная работа LFS.

Область действия — **только эта папка и подпапки**. Мелкие конфиг/фикстур JSON в
других местах репо (`STATE.json` и т.п.) не затрагиваются.

`.gitattributes` — это легитимный, необходимый, закоммиченный конфиг git по прямому
пути (зеркалит уже существующий в репо паттерн), а не «скрытый мусор» — правило
гигиены он не нарушает.

---

## 4. Проверки, уже прогнанные при настройке (read-only)

```bash
# путь НЕ игнорируется (пусто = не в .gitignore):
git check-ignore -v "llm mvp/learning-sets/example.json"

# паттерн LFS распознан для .json в этой папке (ожидается filter: lfs):
git check-attr filter -- "llm mvp/learning-sets/example.json"

# .md/.txt в этой же папке под LFS НЕ попадают (ожидается unspecified):
git check-attr filter -- "llm mvp/learning-sets/NODE-01-DOMAIN-SPLITTER.md"
```

---

## 5. Что делает Люба, чтобы финализировать (коммит + push)

```bash
cd "C:/Projects/xiigen-planning"
git add "llm mvp/learning-sets/.gitattributes" "llm mvp/learning-sets/LEARNING-SETS-JSON-README.md"
# + когда появятся сами наборы:
git add "llm mvp/learning-sets/<domain>_<model>_<num>_<size>_<date>.json"
git commit -m "learning-sets: LFS for training JSON + naming convention"
git push
# проверить, что наборы реально ушли в LFS (а не в обычный git):
git lfs ls-files
```

Важно: если в этой папке уже лежат **ранее закоммиченные** `.json` (сейчас таких нет),
атрибут LFS на них задним числом не применяется — их надо перенести
`git lfs migrate import --include="llm mvp/learning-sets/*.json"` или пере-`git add`.

Никогда не добавлять `learning-sets/` (или её `*.json`) в `.gitignore`-правило вида
`snapshots/*.json` — это ровно тот скрыто/незакоммичено анти-паттерн, который здесь
запрещён.
