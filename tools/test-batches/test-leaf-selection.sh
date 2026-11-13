#!/usr/bin/env bash
set -Eeuo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../.." && pwd)"
evidence_root="${1:-$repo_root/TestResults/leaf-selection-self-check-$(date -u +%Y%m%dT%H%M%SZ)}"
runner="$script_dir/run-test-batches.sh"

mkdir -p "$evidence_root/fake-bin" "$evidence_root/work"

fake_dotnet="$evidence_root/fake-bin/dotnet"
# Это не доказательство работы настоящего .NET: здесь маленькая подмена только
# имитирует список пяти методов и валидный TRX. Так self-test проверяет логику
# выбора порций, ошибки CLI и resume, не выдавая синтетику за product runtime.
cat > "$fake_dotnet" <<'FAKE_DOTNET'
#!/usr/bin/env bash
set -Eeuo pipefail

methods=(
  "Example.LeafSelfCheckTests.Test01"
  "Example.LeafSelfCheckTests.Test02"
  "Example.LeafSelfCheckTests.Test03"
  "Example.LeafSelfCheckTests.Test04"
  "Example.LeafSelfCheckTests.Test05"
)

# Эти короткие ответы нужны fingerprint-слою запускалки. Они стабильны и не
# зависят от установленного SDK, потому что проверяется не SDK, а выбор порции.
if [[ "${1:-}" == "--version" ]]; then
  printf '9.0.self-check\n'
  exit 0
fi

if [[ "${1:-}" == "--info" ]]; then
  printf 'Self-check .NET info\n'
  exit 0
fi

if [[ "${1:-}" == "--list-runtimes" ]]; then
  printf 'Microsoft.NETCore.App 9.0.self-check [/self-check]\n'
  exit 0
fi

if [[ "${1:-}" != "vstest" ]]; then
  printf 'unexpected fake dotnet call: %s\n' "$*" >&2
  exit 64
fi

shift
_dll="${1:-}"
shift || true
settings=""
results_dir=""
list_tests="false"
for arg in "$@"; do
  case "$arg" in
    /Settings:*) settings="${arg#/Settings:}" ;;
    /ResultsDirectory:*) results_dir="${arg#/ResultsDirectory:}" ;;
    /ListTests) list_tests="true" ;;
  esac
done

# Первый путь fake VSTest возвращает discovery-список. В нём пять методов:
# при лимите одна строка на порцию получается полный план из пяти маленьких групп.
if [[ "$list_tests" == "true" ]]; then
  printf 'The following Tests are available:\n'
  for method in "${methods[@]}"; do
    printf '    %s\n' "$method"
  done
  exit 0
fi

if [[ -z "$settings" || -z "$results_dir" ]]; then
  printf 'fake dotnet needs /Settings and /ResultsDirectory\n' >&2
  exit 64
fi

mkdir -p "$results_dir"
python3 - "$settings" "$results_dir/result.trx" <<'PY'
import html
import re
import sys
import xml.sax.saxutils as sx

settings_path, output_path = sys.argv[1:3]
settings = open(settings_path, encoding="utf-8").read()
match = re.search(r"<TestCaseFilter>(.*?)</TestCaseFilter>", settings, re.S)
filter_text = html.unescape(match.group(1)) if match else ""
methods = re.findall(r"FullyQualifiedName=([A-Za-z0-9_.+]+)", filter_text)
if not methods:
    methods = ["Example.LeafSelfCheckTests.Test01"]

# Второй путь fake VSTest пишет ровно те методы, которые пришли через фильтр
# runsettings. Поэтому тест может доказать "запущена leaf-005", не выполняя
# настоящие xUnit assertions.
with open(output_path, "w", encoding="utf-8", newline="\n") as out:
    out.write("<TestRun>\n")
    out.write("  <Results>\n")
    for index, method in enumerate(methods, 1):
        out.write(
            f'    <UnitTestResult executionId="execution-{index}" '
            f'testId="test-{index}" testName="{sx.escape(method)}" outcome="Passed" />\n'
        )
    out.write("  </Results>\n")
    out.write(
        f'  <ResultSummary outcome="Completed"><Counters total="{len(methods)}" '
        f'executed="{len(methods)}" passed="{len(methods)}" failed="0" error="0" '
        'timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" '
        'notRunnable="0" notExecuted="0" disconnected="0" warning="0" '
        'completed="0" inProgress="0" pending="0" /></ResultSummary>\n'
    )
    out.write("  <TestDefinitions>\n")
    for index, method in enumerate(methods, 1):
        class_name, method_name = method.rsplit(".", 1)
        out.write(f'    <UnitTest id="test-{index}" name="{sx.escape(method)}">\n')
        out.write(
            f'      <TestMethod className="{sx.escape(class_name)}" '
            f'name="{sx.escape(method_name)}" />\n'
        )
        out.write("    </UnitTest>\n")
    out.write("  </TestDefinitions>\n")
    out.write("</TestRun>\n")
PY
FAKE_DOTNET
chmod +x "$fake_dotnet"

fake_dll="$evidence_root/work/Fake.LeafSelection.Tests.dll"
printf 'fake dll for selected leaf self-check\n' > "$fake_dll"

manifest_one="$evidence_root/work/batches-one.tsv"
manifest_two="$evidence_root/work/batches-two.tsv"
# Первый список нужен для обычного случая: одна основная группа и пять
# маленьких порций внутри неё.
{
  printf '# order\tbatch_id\thuman_name\tdll\tfilter\tmin_rows\tsplit_on_repeat_timeout\tplatform\tplatform_reason\n'
  printf '01\tleaf-selection\tLeaf selection self-check\t%s\tFullyQualifiedName~Example.LeafSelfCheckTests.\t1\ttrue\tany\tself-check\n' "$fake_dll"
} > "$manifest_one"
# Второй список нужен только для отрицательного случая: короткое leaf-001
# неоднозначно, если выбраны две основные группы сразу.
{
  printf '# order\tbatch_id\thuman_name\tdll\tfilter\tmin_rows\tsplit_on_repeat_timeout\tplatform\tplatform_reason\n'
  printf '01\tleaf-selection-a\tLeaf selection self-check A\t%s\tFullyQualifiedName~Example.LeafSelfCheckTests.\t1\ttrue\tany\tself-check\n' "$fake_dll"
  printf '02\tleaf-selection-b\tLeaf selection self-check B\t%s\tFullyQualifiedName~Example.LeafSelfCheckTests.\t1\ttrue\tany\tself-check\n' "$fake_dll"
} > "$manifest_two"

run_runner() {
  local name="$1"
  shift
  local root="$evidence_root/$name"
  mkdir -p "$root"
  # Общие параметры держат план по одной строке на маленькую группу. Все
  # дополнительные аргументы идут в конец, чтобы негативные проверки могли
  # передать пустое значение или переопределить режим так же, как оператор CLI.
  PATH="$evidence_root/fake-bin:$PATH" bash "$runner" \
    --repo-root "$repo_root" \
    --batches "$manifest_one" \
    --only leaf-selection \
    --run-root "$root" \
    --no-build \
    --leaf-mode method-diagnostic \
    --leaf-target-rows 1 \
    --leaf-hard-cap-rows 1 \
    "$@" \
    > "$root/stdout.log" 2> "$root/stderr.log"
}

run_runner_with_manifest() {
  local name="$1"
  local manifest="$2"
  local only="$3"
  shift 3
  local root="$evidence_root/$name"
  mkdir -p "$root"
  # Эта обёртка нужна для единственного случая с двумя основными группами:
  # она проверяет неоднозначность leaf-номера, не дублируя отдельный runner.
  PATH="$evidence_root/fake-bin:$PATH" bash "$runner" \
    --repo-root "$repo_root" \
    --batches "$manifest" \
    --only "$only" \
    --run-root "$root" \
    --no-build \
    --leaf-mode method-diagnostic \
    --leaf-target-rows 1 \
    --leaf-hard-cap-rows 1 \
    "$@" \
    > "$root/stdout.log" 2> "$root/stderr.log"
}

expect_success() {
  local name="$1"
  shift
  if ! run_runner "$name" "$@"; then
    printf 'leaf selection self-check failed: %s should have succeeded\n' "$name" >&2
    return 1
  fi
}

expect_failure() {
  local name="$1"
  shift
  if run_runner "$name" "$@"; then
    printf 'leaf selection self-check failed: %s should have failed\n' "$name" >&2
    return 1
  fi
}

expect_no_attempts() {
  local name="$1"

  if find "$evidence_root/$name" -type d -name 'attempt-*' | grep -q .; then
    printf 'leaf selection self-check failed: %s started an attempt before validating selector\n' "$name" >&2
    return 1
  fi
}

selected_root="$evidence_root/selected-leaf"
# Первый запуск выбирает только пятую маленькую группу. Остальные четыре
# должны остаться без каталогов и без строк в leaf-журнале.
expect_success selected-leaf --leaf-id leaf-005 --max-leaves 1

selected_journal="$selected_root/batch-01-leaf-selection/leaf-journal.tsv"
selected_batch_dir="$selected_root/batch-01-leaf-selection"
if [[ "$(awk -F '\t' 'NR > 1 { print $1 }' "$selected_journal")" != "leaf-005" ]]; then
  printf 'leaf selection self-check failed: selected run did not execute only leaf-005\n' >&2
  exit 1
fi
if [[ "$(find "$selected_batch_dir" -maxdepth 1 -type d -name 'leaf-*' | wc -l | tr -d ' ')" != "1" ]]; then
  printf 'leaf selection self-check failed: unselected leaf directories were created\n' >&2
  exit 1
fi
if [[ "$(cat "$selected_root/state.txt")" != "INCOMPLETE" ]]; then
  printf 'leaf selection self-check failed: selected one-leaf run should stay incomplete\n' >&2
  exit 1
fi

# Повтор той же выбранной порции — это resume: закрытая leaf-005 не должна
# запускаться ещё раз и не должна получить вторую строку журнала.
expect_success selected-leaf --leaf-id leaf-005 --max-leaves 1
if [[ "$(awk 'NR > 1 { count += 1 } END { print count + 0 }' "$selected_journal")" != "1" ]]; then
  printf 'leaf selection self-check failed: resume reran an already closed selected leaf\n' >&2
  exit 1
fi

# Обычное продолжение без selector должно закрыть остальные маленькие группы,
# не повторяя leaf-005. Старая карта выбора перестаёт быть текущей: уже
# выполненная порция остаётся видна в leaf-журнале и результате, а
# run-invocations не хранит точный --leaf-id.
expect_success selected-leaf
if [[ "$(cat "$selected_root/state.txt")" != "FINISHED" ]]; then
  printf 'leaf selection self-check failed: ordinary resume did not finish the run root\n' >&2
  exit 1
fi
if [[ -e "$selected_root/incomplete-reason.txt" ]]; then
  printf 'leaf selection self-check failed: ordinary resume left stale incomplete reason\n' >&2
  exit 1
fi
if [[ "$(awk 'NR > 1 { count += 1 } END { print count + 0 }' "$selected_batch_dir/leaf-plan.tsv")" != "5" ]]; then
  printf 'leaf selection self-check failed: full five-leaf plan was not preserved\n' >&2
  exit 1
fi
if ! awk -F '\t' 'NR > 1 { count[$1] += 1 } END { for (i = 1; i <= 5; i += 1) { id = sprintf("leaf-%03d", i); if (count[id] != 1) exit 1 } }' "$selected_journal"; then
  printf 'leaf selection self-check failed: ordinary resume did not execute each leaf exactly once\n' >&2
  exit 1
fi
if [[ "$(awk -F '\t' 'NR > 1 && $3 == "yes" { yes += 1 } NR > 1 && $3 == "no" { no += 1 } END { printf "%d/%d", yes + 0, no + 0 }' "$selected_batch_dir/selected-leaves.tsv")" != "5/0" ]]; then
  printf 'leaf selection self-check failed: ordinary resume did not refresh current selected-leaves map\n' >&2
  exit 1
fi
if [[ "$(awk -F '\t' '$1 == "selected_leaf_ids" { print $2 }' "$selected_root/verification-scope.tsv")" != "none" ]]; then
  printf 'leaf selection self-check failed: ordinary resume did not clear current selected leaf scope\n' >&2
  exit 1
fi

expect_failure unknown-leaf --leaf-id leaf-099
expect_no_attempts unknown-leaf

expect_failure duplicate-leaf --leaf-id leaf-003,leaf-003
expect_no_attempts duplicate-leaf

expect_failure empty-leaf --leaf-id ''
expect_no_attempts empty-leaf

expect_failure trailing-comma --leaf-id leaf-003,
expect_no_attempts trailing-comma

expect_failure repeated-option --leaf-id leaf-003 --leaf-id leaf-004
expect_no_attempts repeated-option

expect_failure duplicate-qualified-alias --leaf-id leaf-003,leaf-selection/leaf-003
expect_no_attempts duplicate-qualified-alias

expect_failure wrong-batch --leaf-id other-batch/leaf-003
expect_no_attempts wrong-batch

expect_failure leaf-mode-off --leaf-id leaf-001 --leaf-mode off
expect_no_attempts leaf-mode-off

ambiguous_root="$evidence_root/ambiguous-leaf"
mkdir -p "$ambiguous_root"
if run_runner_with_manifest ambiguous-leaf "$manifest_two" leaf-selection-a,leaf-selection-b --leaf-id leaf-001; then
  printf 'leaf selection self-check failed: ambiguous leaf should have failed\n' >&2
  exit 1
fi
expect_no_attempts ambiguous-leaf

printf 'leaf selection self-check passed: exact small group, empty, repeated, trailing comma, wrong batch, off mode, duplicate alias, ambiguous and ordinary resume paths verified. Evidence: %s\n' "$evidence_root"
