#!/usr/bin/env bash
set -Eeuo pipefail

repo_root=""
slice_name=""
batches_file=""
only_batches=""
run_root=""
skip_build="false"
batch_timeout="20m"
session_timeout_ms="1200000"
hang_timeout="10m"
plan_only="false"
focused_mode="false"
verification_scope="CANONICAL"
self_check_corrupt_trx="false"
leaf_mode="pack"
leaf_target_rows="38"
leaf_hard_cap_rows="60"
leaf_timeout="8m"
leaf_session_timeout_ms="480000"
leaf_hang_timeout="5m"
leaf_mode_overrides_file=""
leaf_mode_overrides_scope="not_loaded"
leaf_mode_overrides_sha256="not_applicable"
max_leaves="0"
max_elapsed_minutes="0"
selected_leaf_ids=""
selected_leaf_ids_specified="false"
selected_leaf_ids_normalized="none"
selected_leaf_selector_active="false"
self_check_leaf_planner="false"
self_check_leaf_resume="false"
self_check_fingerprint_mismatch="false"
self_check_exact_filter="false"
self_check_leaf_trx_membership="false"
self_check_leaf_parent_timing="false"
self_check_min_rows="false"
self_check_class_first_selection="false"
self_check_empty_residual="false"
self_check_invocation_budget="false"
self_check_hang_guard="false"
run_started_epoch="$(date -u +%s)"
global_incomplete_reason=""
invocation_started_leaf_attempts=0
declare -A leaf_mode_override_by_batch=()
declare -A leaf_mode_override_row_by_batch=()
declare -A selected_leaf_selector_by_key=()

# Входные параметры намеренно простые: этот файл должен запускаться и из WSL,
# и через PowerShell-обертку без скрытых настроек в окружении.
while [[ $# -gt 0 ]]; do
  case "$1" in
    --repo-root) repo_root="$2"; shift 2 ;;
    --slice) slice_name="$2"; shift 2 ;;
    --batches) batches_file="$2"; shift 2 ;;
    --only) only_batches="$2"; shift 2 ;;
    --run-root) run_root="$2"; shift 2 ;;
    --no-build) skip_build="true"; shift ;;
    --batch-timeout) batch_timeout="$2"; shift 2 ;;
    --session-timeout-ms) session_timeout_ms="$2"; shift 2 ;;
    --hang-timeout) hang_timeout="$2"; shift 2 ;;
    --plan-only) plan_only="true"; shift ;;
    --focused) focused_mode="true"; shift ;;
    --self-check-corrupt-trx) self_check_corrupt_trx="true"; shift ;;
    --leaf-mode) leaf_mode="$2"; shift 2 ;;
    --leaf-target-rows) leaf_target_rows="$2"; shift 2 ;;
    --leaf-hard-cap-rows) leaf_hard_cap_rows="$2"; shift 2 ;;
    --leaf-timeout) leaf_timeout="$2"; shift 2 ;;
    --leaf-session-timeout-ms) leaf_session_timeout_ms="$2"; shift 2 ;;
    --leaf-hang-timeout) leaf_hang_timeout="$2"; shift 2 ;;
    --max-leaves) max_leaves="$2"; shift 2 ;;
    --leaf-id|--leaf-ids)
      if [[ $# -lt 2 ]]; then
        echo "Для --leaf-id нужно указать идентификатор маленькой группы, например leaf-005." >&2
        exit 64
      fi
      if [[ "$selected_leaf_ids_specified" == "true" ]]; then
        echo "Повторный --leaf-id неоднозначен: укажите все нужные маленькие группы одним списком через запятую." >&2
        exit 64
      fi
      selected_leaf_ids_specified="true"
      selected_leaf_ids="$2"
      shift 2
      ;;
    --max-elapsed-minutes) max_elapsed_minutes="$2"; shift 2 ;;
    --self-check-leaf-planner) self_check_leaf_planner="true"; shift ;;
    --self-check-leaf-resume) self_check_leaf_resume="true"; shift ;;
    --self-check-fingerprint-mismatch) self_check_fingerprint_mismatch="true"; shift ;;
    --self-check-exact-filter) self_check_exact_filter="true"; shift ;;
    --self-check-leaf-trx-membership) self_check_leaf_trx_membership="true"; shift ;;
    --self-check-leaf-parent-timing) self_check_leaf_parent_timing="true"; shift ;;
    --self-check-min-rows) self_check_min_rows="true"; shift ;;
    --self-check-class-first-selection) self_check_class_first_selection="true"; shift ;;
    --self-check-empty-residual) self_check_empty_residual="true"; shift ;;
    --self-check-invocation-budget) self_check_invocation_budget="true"; shift ;;
    --self-check-hang-guard) self_check_hang_guard="true"; shift ;;
    *) echo "Unknown argument: $1" >&2; exit 64 ;;
  esac
done

case "$leaf_mode" in
  pack|method-diagnostic|class-method-pack|off) ;;
  *) echo "Unknown leaf mode: $leaf_mode" >&2; exit 64 ;;
esac

if [[ ! "$max_leaves" =~ ^[0-9]+$ ]]; then
  echo "Invalid max leaves budget: $max_leaves" >&2
  exit 64
fi

if [[ ! "$max_elapsed_minutes" =~ ^[0-9]+$ ]]; then
  echo "Invalid max elapsed minutes budget: $max_elapsed_minutes" >&2
  exit 64
fi

if [[ -z "$repo_root" ]]; then
  repo_root="$(pwd)"
fi

cd "$repo_root"

if [[ -z "$slice_name" ]]; then
  slice_name="$(git rev-parse --short=10 HEAD 2>/dev/null || date -u +%Y%m%d%H%M)"
fi

default_batches_file="$repo_root/tools/test-batches/batches.tsv"
if [[ -z "$batches_file" ]]; then
  batches_file="$default_batches_file"
elif [[ "$batches_file" != /* ]]; then
  batches_file="$repo_root/$batches_file"
fi

if [[ -z "$run_root" ]]; then
  stamp="$(date -u +%Y%m%dT%H%M%SZ)"
  run_root="$repo_root/TestResults/batches-$slice_name/run-$stamp"
elif [[ "$run_root" != /* ]]; then
  run_root="$repo_root/$run_root"
fi

mkdir -p "$run_root"

journal="$run_root/journal.tsv"
plan_file="$run_root/plan.tsv"
summary="$run_root/summary.md"
run_state="$run_root/state.txt"
run_invocations="$run_root/run-invocations.tsv"

flush_path() {
  local path="$1"

  # На WSL/Linux обычно доступен coreutils sync. Если конкретная файловая
  # система не умеет fsync для каталога или файла, это не отменяет атомарный
  # rename: читатель либо увидит старую цельную версию, либо новую.
  if command -v sync >/dev/null 2>&1; then
    sync -f "$path" 2>/dev/null || sync "$path" 2>/dev/null || true
  fi
}

write_atomic() {
  local target="$1"
  shift
  local temp="$target.tmp.$$"

  # Любой авторитетный маркер пишется через временный файл и атомарное
  # переименование. Так оборванный процесс не оставит "COMPLETED" без тела.
  {
    "$@"
  } > "$temp"
  flush_path "$temp"
  mv -f "$temp" "$target"
  flush_path "$target"
  flush_path "$(dirname "$target")"
}

write_state() {
  local target="$1"
  local value="$2"

  write_atomic "$target" printf '%s\n' "$value"
}

sha256_file() {
  local path="$1"

  sha256sum "$path" | awk '{print $1}'
}

sha256_text() {
  local value="$1"

  printf '%s' "$value" | sha256sum | awk '{print $1}'
}

escape_tsv_value() {
  local value="$1"

  value="${value//$'\t'/\\t}"
  value="${value//$'\r'/\\r}"
  value="${value//$'\n'/\\n}"
  printf '%s\n' "$value"
}

load_leaf_mode_overrides() {
  local test_infrastructure_batch_id="test-infrastructure-evidence-measurement"
  local storage_batch_id="design-conversion-dpo-storage"
  local header=$'# batch_id\tleaf_mode\thuman_reason'
  local line=""
  local line_number=0
  local batch_id=""
  local override_mode=""
  local human_reason=""
  local extra=""
  declare -A known_batch_ids=()

  leaf_mode_overrides_file="$repo_root/tools/test-batches/leaf-mode-overrides.tsv"
  leaf_mode_overrides_scope="skipped_custom_manifest"
  leaf_mode_overrides_sha256="not_applicable"
  leaf_mode_override_by_batch=()
  leaf_mode_override_row_by_batch=()

  if [[ "$custom_batches_file" == "true" || "$focused_mode" == "true" ]]; then
    return 0
  fi

  leaf_mode_overrides_scope="canonical"
  if [[ ! -f "$leaf_mode_overrides_file" ]]; then
    echo "Leaf mode overrides file is missing: $leaf_mode_overrides_file" >&2
    return 65
  fi

  while IFS= read -r line || [[ -n "$line" ]]; do
    line="${line%$'\r'}"
    [[ -z "$line" || "${line:0:1}" == "#" ]] && continue
    IFS=$'\t' read -r _ batch_id _ _ _ _ _ _ _ <<< "$line"
    if [[ -n "${batch_id:-}" ]]; then
      known_batch_ids["$batch_id"]="true"
    fi
  done < "$batches_file"

  while IFS= read -r line || [[ -n "$line" ]]; do
    line_number=$((line_number + 1))
    line="${line%$'\r'}"
    if (( line_number == 1 )); then
      line="${line#$'\xef\xbb\xbf'}"
      if [[ "$line" != "$header" ]]; then
        echo "Leaf mode overrides header mismatch: $leaf_mode_overrides_file" >&2
        return 65
      fi
      continue
    fi

    [[ -z "$line" || "${line:0:1}" == "#" ]] && continue
    IFS=$'\t' read -r batch_id override_mode human_reason extra <<< "$line"
    if [[ -n "${extra:-}" ]]; then
      echo "Leaf mode overrides line $line_number must have 3 columns." >&2
      return 65
    fi
    if [[ -z "${batch_id:-}" || -z "${override_mode:-}" || -z "${human_reason:-}" ]]; then
      echo "Leaf mode overrides line $line_number has a blank required field." >&2
      return 65
    fi
    if [[ ! "$batch_id" =~ ^[A-Za-z0-9_-]+$ ]]; then
      echo "Leaf mode overrides line $line_number has invalid batch id: $batch_id" >&2
      return 65
    fi
    case "$override_mode" in
      pack|method-diagnostic|class-method-pack|off) ;;
      *) echo "Leaf mode overrides line $line_number has unknown mode: $override_mode" >&2; return 65 ;;
    esac
    if [[ "$override_mode" == "class-method-pack" && "$batch_id" != "$storage_batch_id" ]]; then
      echo "Leaf mode overrides may use class-method-pack only for $storage_batch_id, not $batch_id." >&2
      return 65
    fi
    if [[ -n "${leaf_mode_override_by_batch[$batch_id]+x}" ]]; then
      echo "Leaf mode overrides duplicate batch id: $batch_id" >&2
      return 65
    fi
    if [[ -z "${known_batch_ids[$batch_id]+x}" ]]; then
      echo "Leaf mode overrides target batch is missing from canonical batches.tsv: $batch_id" >&2
      return 65
    fi

    leaf_mode_override_by_batch["$batch_id"]="$override_mode"
    leaf_mode_override_row_by_batch["$batch_id"]="$(escape_tsv_value "$line")"
  done < "$leaf_mode_overrides_file"

  if [[ "${leaf_mode_override_by_batch[$test_infrastructure_batch_id]:-}" != "method-diagnostic" ]]; then
    echo "Leaf mode overrides must force $test_infrastructure_batch_id to method-diagnostic." >&2
    return 65
  fi
  if [[ "${leaf_mode_override_by_batch[$storage_batch_id]:-}" != "class-method-pack" ]]; then
    echo "Leaf mode overrides must force $storage_batch_id to class-method-pack." >&2
    return 65
  fi

  leaf_mode_overrides_sha256="$(sha256_file "$leaf_mode_overrides_file")"
}

effective_leaf_mode_for_batch() {
  local batch_id="$1"

  if [[ -n "${leaf_mode_override_by_batch[$batch_id]+x}" ]]; then
    printf '%s\n' "${leaf_mode_override_by_batch[$batch_id]}"
  else
    printf '%s\n' "$leaf_mode"
  fi
}

leaf_mode_override_row_for_batch() {
  local batch_id="$1"

  if [[ -n "${leaf_mode_override_row_by_batch[$batch_id]+x}" ]]; then
    printf '%s\n' "${leaf_mode_override_row_by_batch[$batch_id]}"
  else
    printf 'none\n'
  fi
}

safe_numeric_or_default() {
  local value="$1"
  local fallback="$2"

  if [[ "$value" =~ ^[0-9]+$ ]]; then
    printf '%s\n' "$value"
  else
    printf '%s\n' "$fallback"
  fi
}

invocation_leaf_budget_exhausted() {
  local max_leaf_count

  max_leaf_count="$(safe_numeric_or_default "$max_leaves" "0")"
  (( max_leaf_count > 0 && invocation_started_leaf_attempts >= max_leaf_count ))
}

record_invocation_leaf_attempt_started() {
  # Это общий счётчик одного CLI-захода, а не одной группы. Закрытые leaf-и при
  # resume, plan-only, platform skip и подтверждённо пустой остаток сюда не
  # попадают; неудачный или оборванный реально запущенный leaf уже тратит бюджет.
  invocation_started_leaf_attempts=$((invocation_started_leaf_attempts + 1))
}

write_key_value_file() {
  local target="$1"
  shift

  write_atomic "$target" printf '%s\n' "$@"
}

path_identity() {
  local path="$1"

  python3 - "$path" <<'PY'
import os
import sys

print(os.path.realpath(sys.argv[1]))
PY
}

batches_file_contains_exact_method_filter() {
  local manifest="$1"

  python3 - "$manifest" <<'PY'
import sys

path = sys.argv[1]
try:
    lines = open(path, "r", encoding="utf-8-sig").read().splitlines()
except FileNotFoundError:
    sys.exit(1)

for line in lines:
    if not line.strip() or line.startswith("#"):
        continue
    parts = line.split("\t")
    if len(parts) >= 5 and "FullyQualifiedName=" in parts[4]:
        sys.exit(0)

sys.exit(1)
PY
}

validate_focused_batches_file() {
  local manifest="$1"

  # Focused-manifest обязан быть маленьким явным списком exact method FQN:
  # wildcard/substring/negation здесь запрещены, чтобы checkpoint не расширился
  # до скрытого broad run из-за одной удобной строки фильтра.
  python3 - "$manifest" <<'PY'
import re
import sys

path = sys.argv[1]
method_re = re.compile(r"^FullyQualifiedName=([A-Za-z0-9_.+]+)$")

def split_unescaped_pipe(value: str):
    parts = []
    start = 0
    index = 0
    while index < len(value):
        if value[index] == "|":
            backslashes = 0
            cursor = index - 1
            while cursor >= 0 and value[cursor] == "\\":
                backslashes += 1
                cursor -= 1
            if backslashes % 2 == 0:
                parts.append(value[start:index])
                start = index + 1
        index += 1
    parts.append(value[start:])
    return parts

rows = 0
batch_ids = set()
methods = set()
problems = []

try:
    lines = open(path, "r", encoding="utf-8-sig").read().splitlines()
except FileNotFoundError:
    print(f"Focused batches file not found: {path}", file=sys.stderr)
    sys.exit(64)

for line_number, line in enumerate(lines, 1):
    if not line.strip() or line.startswith("#"):
        continue

    parts = line.split("\t")
    if len(parts) != 9:
        problems.append(f"line {line_number}: expected 9 TSV columns, found {len(parts)}")
        continue

    rows += 1
    batch_id = parts[1]
    filter_text = parts[4].strip()
    if not batch_id:
        problems.append(f"line {line_number}: empty batch id")
    elif batch_id in batch_ids:
        problems.append(f"line {line_number}: duplicate batch id {batch_id}")
    else:
        batch_ids.add(batch_id)

    if not filter_text:
        problems.append(f"line {line_number}: empty focused filter")
        continue
    if "*" in filter_text or "&" in filter_text or "!" in filter_text or "~" in filter_text:
        problems.append(f"line {line_number}: focused filter allows only exact FullyQualifiedName= disjunctions")
        continue

    for term in split_unescaped_pipe(filter_text):
        term = term.strip()
        match = method_re.match(term)
        if not match:
            problems.append(f"line {line_number}: non-exact focused term {term!r}")
            continue
        method = match.group(1)
        if method in methods:
            problems.append(f"line {line_number}: duplicate method {method}")
        methods.add(method)

if rows == 0:
    problems.append("focused manifest has no batch rows")

if problems:
    for problem in problems:
        print(problem, file=sys.stderr)
    sys.exit(64)
PY
}

write_scope_evidence() {
  local scope_file="$run_root/verification-scope.tsv"
  local full_suite_claim_allowed="true"

  if [[ "$verification_scope" == "FOCUSED" || "$selected_leaf_selector_active" == "true" ]]; then
    full_suite_claim_allowed="false"
  fi

  write_key_value_file "$scope_file" \
    $'key\tvalue' \
    "scope	$verification_scope" \
    "full_suite_claim_allowed	$full_suite_claim_allowed" \
    "batches_file	$batches_file" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "focused_mode	$focused_mode" \
    "selected_leaf_ids	$selected_leaf_ids_normalized" \
    "selected_leaf_note	Если выбраны маленькие группы, команда запускает только их; остальные порции остаются незакрытыми до обычного продолжения или полного прогона." \
    "invocation_max_leaves	$max_leaves" \
    "invocation_max_elapsed_minutes	$max_elapsed_minutes" \
    "invocation_budget_note	Эти лимиты описывают только текущий resume-заход; их смена не должна инвалидировать fingerprint плана leaf-ов."
}

append_run_invocation_history() {
  local selected_batches=""
  local effective_modes=""
  local selected_batch_count=0
  local order=""
  local batch_id=""
  local human_name=""
  local dll=""
  local filter=""
  local min_rows=""
  local split_on_repeat_timeout=""
  local platform=""
  local platform_reason=""

  while IFS=$'\t' read -r order batch_id human_name dll filter min_rows split_on_repeat_timeout platform platform_reason; do
    [[ -z "${order:-}" || "${order:0:1}" == "#" ]] && continue

    if contains_batch "$batch_id"; then
      selected_batch_count=$((selected_batch_count + 1))
      if [[ -n "$selected_batches" ]]; then
        selected_batches+=","
        effective_modes+=","
      fi
      selected_batches+="$batch_id"
      effective_modes+="$batch_id:$(effective_leaf_mode_for_batch "$batch_id")"
    fi
  done < "$batches_file"

  if [[ -z "$selected_batches" ]]; then
    selected_batches="none"
    effective_modes="none"
  fi

  python3 - "$run_invocations" "$verification_scope" "$selected_batches" "$selected_batch_count" "$leaf_mode" \
    "$effective_modes" "$max_leaves" "$max_elapsed_minutes" "$plan_only" "$skip_build" "$focused_mode" \
    "$(sha256_file "$batches_file")" <<'PY'
import datetime as _dt
import hashlib
import os
import re
import sys

HEADER = (
    "invocation_order\tinvocation_id\tstarted_utc\tverification_scope\tselected_batches\t"
    "selected_batch_count\trequested_leaf_mode\teffective_leaf_modes\tmax_leaves\t"
    "max_elapsed_minutes\tplan_only\tno_build\tfocused_mode\tbatches_tsv_sha256\t"
    "status_at_append\tprevious_rows_sha256\trow_sha256"
)

path, scope, selected_batches, selected_count, requested_leaf_mode, effective_modes, max_leaves, max_elapsed, plan_only, no_build, focused_mode, batches_sha = sys.argv[1:13]

def fail(message: str) -> None:
    print(f"Run invocation history is invalid: {message}", file=sys.stderr)
    sys.exit(65)

def digest(text: str) -> str:
    return hashlib.sha256(text.encode("utf-8")).hexdigest()

def safe(value: str) -> str:
    return value.replace("\t", "\\t").replace("\r", "\\r").replace("\n", "\\n")

def validate_numeric(name: str, value: str) -> None:
    if not re.fullmatch(r"[0-9]+", value or ""):
        fail(f"{name} is not a non-negative integer: {value}")

def validate_boolean(name: str, value: str) -> None:
    if value not in {"true", "false"}:
        fail(f"{name} must be true or false: {value}")

validate_numeric("selected_batch_count", selected_count)
validate_numeric("max_leaves", max_leaves)
validate_numeric("max_elapsed_minutes", max_elapsed)
if requested_leaf_mode not in {"pack", "method-diagnostic", "class-method-pack", "off"}:
    fail(f"requested leaf mode is unknown: {requested_leaf_mode}")
for name, value in (("plan_only", plan_only), ("no_build", no_build), ("focused_mode", focused_mode)):
    validate_boolean(name, value)

if os.path.exists(path):
    with open(path, "r", encoding="utf-8-sig", newline="") as source:
        text = source.read()
    if text and not text.endswith("\n"):
        fail("file does not end with a newline before append")
    lines = text.splitlines()
    if not lines:
        fail("file is empty")
    if lines[0] != HEADER:
        fail("header mismatch")
else:
    lines = [HEADER]

prior_payload = ""
seen_ids = set()
for expected_order, line in enumerate(lines[1:], 1):
    if not line:
        fail(f"blank data row at order {expected_order}")
    parts = line.split("\t")
    if len(parts) != 17:
        fail(f"row {expected_order} has {len(parts)} columns instead of 17")
    try:
        actual_order = int(parts[0])
    except ValueError:
        fail(f"row {expected_order} order is not numeric: {parts[0]}")
    if actual_order != expected_order:
        fail(f"row order is nonmonotonic: expected {expected_order}, actual {actual_order}")
    expected_id = f"invocation-{expected_order:06d}"
    if parts[1] != expected_id:
        fail(f"row {expected_order} id mismatch: expected {expected_id}, actual {parts[1]}")
    if parts[1] in seen_ids:
        fail(f"duplicate invocation id: {parts[1]}")
    seen_ids.add(parts[1])
    if not re.fullmatch(r"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z", parts[2]):
        fail(f"row {expected_order} timestamp is not UTC ISO-8601: {parts[2]}")
    if parts[3] not in {"CANONICAL", "FOCUSED"}:
        fail(f"row {expected_order} scope is unknown: {parts[3]}")
    validate_numeric(f"row {expected_order} selected_batch_count", parts[5])
    if parts[6] not in {"pack", "method-diagnostic", "class-method-pack", "off"}:
        fail(f"row {expected_order} requested leaf mode is unknown: {parts[6]}")
    validate_numeric(f"row {expected_order} max_leaves", parts[8])
    validate_numeric(f"row {expected_order} max_elapsed_minutes", parts[9])
    validate_boolean(f"row {expected_order} plan_only", parts[10])
    validate_boolean(f"row {expected_order} no_build", parts[11])
    validate_boolean(f"row {expected_order} focused_mode", parts[12])
    if not re.fullmatch(r"[0-9a-f]{64}", parts[13]):
        fail(f"row {expected_order} batches hash is not sha256: {parts[13]}")
    if parts[14] != "STARTED":
        fail(f"row {expected_order} status_at_append is not STARTED: {parts[14]}")
    expected_previous_hash = digest(prior_payload)
    if parts[15] != expected_previous_hash:
        fail(f"row {expected_order} previous rows hash mismatch")
    row_without_hash = "\t".join(parts[:-1])
    if parts[16] != digest(row_without_hash):
        fail(f"row {expected_order} row hash mismatch")
    prior_payload += line + "\n"

next_order = len(lines)
base_parts = [
    str(next_order),
    f"invocation-{next_order:06d}",
    _dt.datetime.now(_dt.UTC).strftime("%Y-%m-%dT%H:%M:%SZ"),
    safe(scope),
    safe(selected_batches),
    safe(selected_count),
    safe(requested_leaf_mode),
    safe(effective_modes),
    safe(max_leaves),
    safe(max_elapsed),
    safe(plan_only),
    safe(no_build),
    safe(focused_mode),
    safe(batches_sha),
    "STARTED",
    digest(prior_payload),
]
row = "\t".join(base_parts)
row_with_hash = row + "\t" + digest(row)

# Русский комментарий: resume обязан дописывать новый заход, а не пересобирать
# старые строки. Если уже существующая история повреждена, код выше падает до
# append и оставляет прежний файл без изменений.
if os.path.exists(path):
    with open(path, "a", encoding="utf-8", newline="\n") as target:
        target.write(row_with_hash + "\n")
else:
    with open(path, "w", encoding="utf-8", newline="\n") as target:
        target.write(HEADER + "\n")
        target.write(row_with_hash + "\n")
PY
  flush_path "$run_invocations"
  flush_path "$(dirname "$run_invocations")"
}

wsl_path_from_maybe_windows() {
  local path="$1"
  local drive
  local rest

  if [[ "$path" =~ ^([A-Za-z]):[/\\](.*)$ ]]; then
    drive="$(printf '%s' "${BASH_REMATCH[1]}" | tr '[:upper:]' '[:lower:]')"
    rest="${BASH_REMATCH[2]//\\//}"
    printf '/mnt/%s/%s\n' "$drive" "$rest"
  else
    printf '%s\n' "$path"
  fi
}

resolve_git_dir() {
  local dotgit="$repo_root/.git"
  local raw
  local gitdir

  if [[ -d "$dotgit" ]]; then
    printf '%s\n' "$dotgit"
    return 0
  fi

  if [[ -f "$dotgit" ]]; then
    raw="$(head -n 1 "$dotgit" | tr -d '\r')"
    if [[ "$raw" == gitdir:* ]]; then
      gitdir="${raw#gitdir: }"
      gitdir="$(wsl_path_from_maybe_windows "$gitdir")"
      if [[ "$gitdir" != /* ]]; then
        gitdir="$repo_root/$gitdir"
      fi
      printf '%s\n' "$gitdir"
      return 0
    fi
  fi

  return 1
}

git_head_identity() {
  local git_dir
  local head_value
  local ref_path
  local common_dir

  if git rev-parse HEAD >/dev/null 2>&1; then
    git rev-parse HEAD
    return 0
  fi

  git_dir="$(resolve_git_dir)"
  head_value="$(head -n 1 "$git_dir/HEAD" | tr -d '\r')"
  if [[ "$head_value" =~ ^[0-9a-f]{40}$ ]]; then
    printf '%s\n' "$head_value"
    return 0
  fi

  if [[ "$head_value" == ref:* ]]; then
    ref_path="${head_value#ref: }"
    if [[ -f "$git_dir/$ref_path" ]]; then
      head -n 1 "$git_dir/$ref_path" | tr -d '\r'
      return 0
    fi

    common_dir="$git_dir"
    if [[ -f "$git_dir/commondir" ]]; then
      common_dir="$(head -n 1 "$git_dir/commondir" | tr -d '\r')"
      common_dir="$(wsl_path_from_maybe_windows "$common_dir")"
      if [[ "$common_dir" != /* ]]; then
        common_dir="$git_dir/$common_dir"
      fi
    fi
    if [[ -f "$common_dir/$ref_path" ]]; then
      head -n 1 "$common_dir/$ref_path" | tr -d '\r'
      return 0
    fi
  fi

  return 1
}

ensure_journal() {
  if [[ ! -f "$journal" ]]; then
    write_atomic "$journal" printf 'order\tbatch_id\thuman_name\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tplatform\tplatform_reason\tsplit_on_repeat_timeout\n'
  fi
}

ensure_plan() {
  # План показывает, какой фильтр реально уйдет в VSTest. Это защита от
  # ошибки, когда поздний остаточный батч запускается отдельно и становится "*".
  write_atomic "$plan_file" printf 'order\tbatch_id\tselected\tdll\traw_filter\teffective_filter\tplatform\tplatform_reason\n'
}

append_plan_row() {
  local order="$1"
  local batch_id="$2"
  local selected="$3"
  local dll="$4"
  local raw_filter="$5"
  local effective_filter="$6"
  local platform="$7"
  local platform_reason="$8"
  local temp="$plan_file.tmp.$$"

  cp "$plan_file" "$temp"
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$order" "$batch_id" "$selected" "$dll" "$raw_filter" "$effective_filter" "$platform" "$platform_reason" >> "$temp"
  flush_path "$temp"
  mv -f "$temp" "$plan_file"
  flush_path "$plan_file"
  flush_path "$(dirname "$plan_file")"
}

contains_batch() {
  local id="$1"

  # Пустой --only означает полный прогон; непустой список выбирает строки, но
  # не отменяет вклад более ранних строк в first-match лестницу.
  if [[ -z "$only_batches" ]]; then
    return 0
  fi

  [[ ",$only_batches," == *",$id,"* ]]
}

trim_space() {
  local value="$1"

  # Небольшая утилита для CLI-списков: пробелы вокруг "leaf-005" не должны
  # ломать запуск, но пробел вместо значения всё равно останется ошибкой.
  value="${value#"${value%%[![:space:]]*}"}"
  value="${value%"${value##*[![:space:]]}"}"
  printf '%s\n' "$value"
}

parse_selected_leaf_ids() {
  local raw="$1"
  local raw_trimmed
  local token
  local batch_part
  local leaf_part
  local key
  local normalized=""

  selected_leaf_selector_by_key=()
  selected_leaf_ids_normalized="none"
  selected_leaf_selector_active="false"

  # Отсутствие --leaf-id означает обычный resume всей основной группы.
  # Явно пустое значение — ошибка оператора: его нельзя превращать в широкий
  # запуск, потому что человек просил маленькую проверку.
  if [[ "$selected_leaf_ids_specified" != "true" ]]; then
    return 0
  fi
  raw_trimmed="$(trim_space "$raw")"
  if [[ -z "$raw_trimmed" ]]; then
    echo "Для --leaf-id нужно указать идентификатор маленькой группы, например leaf-005." >&2
    return 64
  fi
  if [[ "$raw_trimmed" == ,* || "$raw_trimmed" == *, || "$raw_trimmed" == *,,* ]]; then
    echo "Выбор маленькой группы содержит пустой элемент: $raw" >&2
    return 64
  fi

  IFS=',' read -ra tokens <<< "$raw_trimmed"
  for token in "${tokens[@]}"; do
    token="$(trim_space "$token")"
    if [[ -z "$token" ]]; then
      echo "Выбор маленькой группы содержит пустой элемент: $raw" >&2
      return 64
    fi

    if [[ "$token" == */* ]]; then
      # Полная форма нужна для отчётов и будущих ручных команд: batch/leaf
      # читается человеком без поиска по RunRoot.
      batch_part="${token%%/*}"
      leaf_part="${token#*/}"
      if [[ -z "$batch_part" || -z "$leaf_part" || "$leaf_part" == */* ]]; then
        echo "Неверный выбор маленькой группы: $token. Используйте leaf-005 или batch-id/leaf-005." >&2
        return 64
      fi
      if [[ ! "$batch_part" =~ ^[A-Za-z0-9_.+-]+$ ]]; then
        echo "Неверное имя основной группы в выборе маленькой группы: $batch_part" >&2
        return 64
      fi
      key="$batch_part/$leaf_part"
    else
      # Короткая форма удобна для обычного случая, где --only выбрал одну
      # основную группу и номер маленькой группы уже не двусмысленен.
      leaf_part="$token"
      key="$leaf_part"
    fi

    if [[ ! "$leaf_part" =~ ^leaf-[0-9][0-9][0-9]+$ ]]; then
      echo "Неверный идентификатор маленькой группы: $leaf_part. Ожидается leaf-005." >&2
      return 64
    fi
    if [[ -n "${selected_leaf_selector_by_key[$key]+x}" ]]; then
      # Дубликат ловим до планирования и до attempts: повтор не должен
      # случайно списать бюджет или скрыть, какой leaf действительно хотели.
      echo "Одна и та же маленькая группа выбрана дважды: $key" >&2
      return 64
    fi

    selected_leaf_selector_by_key["$key"]="requested"
    if [[ -n "$normalized" ]]; then
      normalized+=","
    fi
    normalized+="$key"
  done

  selected_leaf_ids_normalized="$normalized"
  selected_leaf_selector_active="true"
}

prepare_selected_leaf_selector() {
  local selected_count=0
  local selected_batch=""
  local order
  local batch_id
  local human_name
  local dll
  local filter
  local min_rows
  local split_on_repeat_timeout
  local platform
  local platform_reason

  # Эта проверка относится только к расписанию текущего запуска. Она не меняет
  # сам план leaf-ов и не входит в fingerprint совместимости плана.
  parse_selected_leaf_ids "$selected_leaf_ids" || return "$?"
  if [[ "$selected_leaf_selector_active" != "true" ]]; then
    return 0
  fi

  if [[ "$leaf_mode" == "off" ]]; then
    echo "Нельзя выбрать маленькую группу при --leaf-mode off: этот режим не строит план маленьких групп." >&2
    return 64
  fi

  # Номер leaf повторяется внутри каждой основной группы, поэтому всегда
  # выбираем ровно одну основную группу через --only. Полная форма
  # batch-id/leaf-005 только подтверждает её имя; несколько основных групп
  # одной командой здесь не разрешены.
  while IFS=$'\t' read -r order batch_id human_name dll filter min_rows split_on_repeat_timeout platform platform_reason; do
    [[ -z "${order:-}" || "${order:0:1}" == "#" ]] && continue
    if contains_batch "$batch_id"; then
      selected_count=$((selected_count + 1))
      selected_batch="$batch_id"
    fi
  done < "$batches_file"

  if (( selected_count == 0 )); then
    echo "Выбор маленькой группы не нашёл выбранную основную группу. Проверьте --only и batches.tsv." >&2
    return 64
  fi
  if (( selected_count != 1 )); then
    echo "Выбор маленькой группы неоднозначен: вместе с --leaf-id выберите ровно одну основную группу через --only." >&2
    return 64
  fi

  for key in "${!selected_leaf_selector_by_key[@]}"; do
    if [[ "$key" == */* && "${key%%/*}" != "$selected_batch" ]]; then
      echo "Выбор маленькой группы относится к другой основной группе: $key; выбрана $selected_batch." >&2
      return 64
    fi
  done
}

selected_leaf_key_matches() {
  local batch_id="$1"
  local leaf_id="$2"
  local matches=0

  # Разрешены две формы: короткая leaf-005 и явная batch-id/leaf-005. Если
  # человек указал обе для одной порции, это дубликат, а не два запуска.
  if [[ -n "${selected_leaf_selector_by_key[$leaf_id]+x}" ]]; then
    matches=$((matches + 1))
  fi
  if [[ -n "${selected_leaf_selector_by_key[$batch_id/$leaf_id]+x}" ]]; then
    matches=$((matches + 1))
  fi

  printf '%s\n' "$matches"
}

leaf_selected_for_run() {
  local batch_id="$1"
  local leaf_id="$2"

  # Без selector обычный resume идёт по всему плану: закрытые leaf-и
  # пропускаются штатной resume-проверкой, открытые запускаются дальше.
  if [[ "$selected_leaf_selector_active" != "true" ]]; then
    return 0
  fi

  [[ "$(selected_leaf_key_matches "$batch_id" "$leaf_id")" == "1" ]]
}

validate_selected_leaf_ids_for_batch() {
  local batch_id="$1"
  local leaf_plan="$2"
  local key
  local requested_leaf
  local leaf_id
  local plan_batch_id
  local leaf_order
  local method_count
  local estimated_rows
  local leaf_filter
  local matches
  local total_matches=0
  local selected_file="${leaf_plan%/leaf-plan.tsv}/selected-leaves.tsv"
  local temp_file="$selected_file.tmp.$$"

  # Сначала сверяем имена с уже построенным полным планом. Если человек ошибся
  # в leaf-id, команда падает здесь: до VSTest-attempt и до изменения журнала
  # маленьких групп.
  for key in "${!selected_leaf_selector_by_key[@]}"; do
    if [[ "$key" == */* ]]; then
      requested_leaf="${key#*/}"
    else
      requested_leaf="$key"
    fi
    matches="$(awk -F '\t' -v leaf="$requested_leaf" 'NR > 1 && $1 == leaf { count += 1 } END { print count + 0 }' "$leaf_plan")"
    if [[ "$matches" == "0" ]]; then
      echo "Выбранная маленькая группа не найдена в текущем плане: $key" >&2
      return 64
    fi
    if [[ "$matches" != "1" ]]; then
      echo "Выбранная маленькая группа неоднозначна в текущем плане: $key" >&2
      return 64
    fi
  done

  # selected-leaves.tsv — это текущая карта запуска, а не часть fingerprint.
  # Поэтому ordinary resume без selector перезаписывает старую карту как "все
  # порции выбраны". Уже выполненные порции остаются в leaf-журнале и
  # результатах; run-invocations не хранит точный --leaf-id.
  printf 'leaf_id\tbatch_id\tselected\tfilter\n' > "$temp_file"
  while IFS=$'\t' read -r leaf_id plan_batch_id leaf_order method_count estimated_rows leaf_filter; do
    [[ "$leaf_id" == "leaf_id" || -z "${leaf_id:-}" ]] && continue
    if [[ "$selected_leaf_selector_active" != "true" ]]; then
      total_matches=$((total_matches + 1))
      printf '%s\t%s\tyes\t%s\n' "$leaf_id" "$batch_id" "$leaf_filter" >> "$temp_file"
      continue
    fi

    matches="$(selected_leaf_key_matches "$batch_id" "$leaf_id")"
    if [[ "$matches" == "1" ]]; then
      total_matches=$((total_matches + 1))
      printf '%s\t%s\tyes\t%s\n' "$leaf_id" "$batch_id" "$leaf_filter" >> "$temp_file"
    elif [[ "$matches" == "0" ]]; then
      # Незапрошенная порция не получает каталог leaf-NNN и не попадает в
      # leaf-journal.tsv. Verifier увидит общий корень как incomplete.
      printf '%s\t%s\tno\t%s\n' "$leaf_id" "$batch_id" "$leaf_filter" >> "$temp_file"
    else
      rm -f "$temp_file"
      echo "Одна маленькая группа выбрана двумя способами: $batch_id/$leaf_id" >&2
      return 64
    fi
  done < "$leaf_plan"

  if (( total_matches == 0 )); then
    rm -f "$temp_file"
    echo "Выбор маленькой группы не совпал с текущим планом: $selected_leaf_ids_normalized" >&2
    return 64
  fi

  # Карту текущего выбора пишем через временный файл и rename, чтобы читатель
  # не увидел половину старого и половину нового списка после прерванной команды.
  flush_path "$temp_file"
  mv -f "$temp_file" "$selected_file"
  flush_path "$selected_file"
  flush_path "$(dirname "$selected_file")"
}

current_platform() {
  local kernel

  # Runner обычно живет в WSL, но явная платформа нужна для Windows-only
  # батчей: их нужно учесть как пропуск, а не потерять из плана.
  kernel="$(uname -s | tr '[:upper:]' '[:lower:]')"
  case "$kernel" in
    linux*) echo "linux" ;;
    msys*|mingw*|cygwin*) echo "windows" ;;
    *) echo "$kernel" ;;
  esac
}

platform_can_run() {
  local required="$1"
  local actual="$2"

  [[ "$required" == "any" || "$required" == "$actual" ]]
}

declare -A positive_filter_terms_by_dll=()

remember_positive_terms() {
  local dll="$1"
  local filter="$2"
  local token

  # Лестница описывает правило "первое совпадение". Поэтому даже если сейчас
  # выбран только поздний батч, все предыдущие положительные термины той же DLL
  # должны быть запомнены как уже занятые.
  IFS='|&' read -ra tokens <<< "$filter"
  for token in "${tokens[@]}"; do
    token="${token#"${token%%[![:space:]]*}"}"
    token="${token%"${token##*[![:space:]]}"}"
    token="${token#(}"
    token="${token%)}"
    if [[ "$token" == FullyQualifiedName~* ]]; then
      positive_filter_terms_by_dll["$dll"]+=$'\037'"${token#FullyQualifiedName~}"
    fi
  done
}

remember_batch_terms() {
  local dll="$1"
  local filter="$2"

  if [[ "$focused_mode" == "true" ]]; then
    return 0
  fi

  remember_positive_terms "$dll" "$filter"
}

build_legacy_first_match_filter() {
  local dll="$1"
  local filter="$2"
  local effective=""
  local term
  local stored_terms="${positive_filter_terms_by_dll[$dll]:-}"

  # Непустой исходный фильтр сохраняет смысл строки, а исключения ниже
  # отнимают все более ранние first-match назначения в этой же сборке.
  if [[ "$filter" != "*" ]]; then
    effective="($filter)"
  fi

  IFS=$'\037' read -ra terms <<< "$stored_terms"
  for term in "${terms[@]}"; do
    if [[ -z "$term" ]]; then
      continue
    fi

    if [[ -z "$effective" ]]; then
      effective="FullyQualifiedName!~$term"
    else
      effective="$effective&FullyQualifiedName!~$term"
    fi
  done

  if [[ -z "$effective" ]]; then
    printf '*\n'
  else
    printf '%s\n' "$effective"
  fi
}

class_first_selection_enabled() {
  [[ "$focused_mode" != "true" && "$custom_batches_file" != "true" ]]
}

tsv_data_row_count() {
  local path="$1"

  awk -F '\t' 'NR > 1 && $0 !~ /^[[:space:]]*$/ { count += 1 } END { printf "%d\n", count + 0 }' "$path"
}

tsv_estimated_row_sum() {
  local path="$1"

  awk -F '\t' 'NR > 1 && $0 !~ /^[[:space:]]*$/ { total += $2 } END { printf "%d\n", total + 0 }' "$path"
}

class_first_selected_method_count_from_assignment() {
  local assignment_file="$1"
  local batch_id="$2"

  awk -F '\t' -v wanted="$batch_id" '
    NR == 1 {
      expected = "method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind"
      if ($0 != expected) { invalid = 1; exit }
      next
    }
    $0 !~ /^[[:space:]]*$/ && $4 == wanted { count += 1 }
    END {
      if (invalid) { exit 65 }
      printf "%d\n", count + 0
    }
  ' "$assignment_file"
}

batch_dir_has_prior_nonempty_evidence() {
  local batch_dir="$1"
  local state

  [[ -d "$batch_dir" ]] || return 1
  if [[ -s "$batch_dir/state.txt" ]]; then
    state="$(head -n 1 "$batch_dir/state.txt" | tr -d '\r')"
    [[ -z "$state" || "$state" == "EMPTY_VERIFIED" ]] || return 0
  fi
  [[ -e "$batch_dir/result.trx" || -e "$batch_dir/current-attempt.txt" || -e "$batch_dir/exit.txt" ]] && return 0
  [[ -e "$batch_dir/leaf-plan.tsv" || -e "$batch_dir/leaf-methods.tsv" || -e "$batch_dir/leaf-journal.tsv" || -e "$batch_dir/leaf-discovery.tsv" ]] && return 0
  [[ -e "$batch_dir/leaf-fingerprint.tsv" || -e "$batch_dir/leaf-pack-report.tsv" ]] && return 0
  find "$batch_dir" -maxdepth 1 -type d \( -name 'attempt-*' -o -name 'leaf-*' \) -print -quit | grep -q .
}

key_value_get() {
  local path="$1"
  local key="$2"

  awk -F '\t' -v wanted="$key" '
    NR == 1 && $1 == "key" && $2 == "value" { next }
    $1 == wanted {
      print substr($0, length($1) + 2)
      found = 1
      exit
    }
    END {
      if (!found) { exit 1 }
    }
  ' "$path"
}

canonical_last_wildcard_for_dll() {
  local batch_id="$1"
  local dll="$2"
  local filter="$3"

  [[ "$filter" == "*" ]] || return 1
  python3 - "$batches_file" "$dll" "$batch_id" <<'PY'
import sys

batches_path, target_dll, target_batch = sys.argv[1:4]

def normalize_dll(value: str) -> str:
    return value.replace("\\", "/")

same_dll_rows = []
target_rows = []
with open(batches_path, encoding="utf-8-sig") as source:
    for line_number, line in enumerate(source, start=1):
        if not line.strip() or line.startswith("#"):
            continue
        parts = line.rstrip("\n").rstrip("\r").split("\t")
        if len(parts) != 9:
            raise SystemExit(f"batches.tsv line {line_number} must have 9 columns")
        _order, batch_id, _human, dll, filter_text, _min_rows, _split, _platform, _reason = parts
        if normalize_dll(dll) != normalize_dll(target_dll):
            continue
        row = (line_number, batch_id, filter_text)
        same_dll_rows.append(row)
        if batch_id == target_batch:
            target_rows.append(row)

if len(target_rows) != 1 or not same_dll_rows:
    sys.exit(1)

last_line, last_batch, last_filter = same_dll_rows[-1]
if last_batch != target_batch or last_filter != "*":
    sys.exit(1)
PY
}

next_attempt_dir() {
  local batch_dir="$1"
  local highest=0
  local dir
  local name
  local number

  # Каждый новый запуск батча получает новую попытку. Старые TRX остаются
  # доказательством прошлого, но не могут закрыть текущую попытку.
  for dir in "$batch_dir"/attempt-[0-9][0-9][0-9][0-9]; do
    [[ -d "$dir" ]] || continue
    name="${dir##*/attempt-}"
    number=$((10#$name))
    if (( number > highest )); then
      highest="$number"
    fi
  done

  printf '%s/attempt-%04d\n' "$batch_dir" $((highest + 1))
}

current_attempt_dir() {
  local batch_dir="$1"
  local marker="$batch_dir/current-attempt.txt"
  local attempt_name
  local attempt_dir

  [[ -f "$marker" ]] || return 1
  attempt_name="$(head -n 1 "$marker" | tr -d '\r')"
  [[ "$attempt_name" == attempt-[0-9][0-9][0-9][0-9] ]] || return 1
  attempt_dir="$batch_dir/$attempt_name"
  [[ -d "$attempt_dir" ]] || return 1
  printf '%s\n' "$attempt_dir"
}

is_resume_closed() {
  local batch_dir="$1"
  local min_rows="${2:-1}"
  local state_file="$batch_dir/state.txt"
  local state=""
  local attempt_dir=""

  [[ -f "$state_file" ]] || return 1
  state="$(head -n 1 "$state_file" | tr -d '\r')"

  case "$state" in
    COMPLETED_GREEN|COMPLETED_RED)
      if [[ -f "$batch_dir/leaf-plan.tsv" ]]; then
        # Leaf-pack нельзя пропускать на уровне родителя: только run_leaf_batch
        # заново сверяет fingerprint текущего кода, DLL, фильтра и настроек.
        return 1
      else
        if attempt_dir="$(current_attempt_dir "$batch_dir")"; then
          # Для новых запусков закрытость доказывает именно текущая попытка.
          # Старый result.trx в корне больше не имеет права закрыть новый STARTED.
          [[ -s "$attempt_dir/result.trx" && -s "$batch_dir/result.trx" ]] || return 1
        else
          # Старые smoke-доказательства до введения attempt-NNNN остаются
          # читаемыми: если маркера попытки нет, проверяем legacy root TRX.
          [[ -s "$batch_dir/result.trx" ]] || return 1
        fi
        closed_batch_has_min_rows "$batch_dir" "$min_rows"
      fi
      ;;
    SKIPPED_PLATFORM)
      # Платформенный пропуск - честно учтенная граница среды. Он не зеленый,
      # но повторный запуск той же папки не должен бесконечно переписывать его.
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

closed_batch_physical_rows() {
  local batch_dir="$1"
  local leaf_plan="$batch_dir/leaf-plan.tsv"
  local leaf_id
  local leaf_dir
  local leaf_state
  local attempt_dir
  local trx
  local parse_status
  local parse_reason
  local total
  local passed
  local failed
  local not_executed
  local physical_rows=0

  if [[ -f "$leaf_plan" ]]; then
    while IFS=$'\t' read -r leaf_id _; do
      [[ "$leaf_id" == "leaf_id" || -z "${leaf_id:-}" ]] && continue
      leaf_dir="$batch_dir/$leaf_id"
      leaf_state="$(head -n 1 "$leaf_dir/state.txt" 2>/dev/null | tr -d '\r' || true)"
      case "$leaf_state" in
        COMPLETED_GREEN|COMPLETED_RED)
          attempt_dir=""
          if attempt_dir="$(current_attempt_dir "$leaf_dir")"; then
            trx="$attempt_dir/result.trx"
          else
            trx="$leaf_dir/result.trx"
          fi

          IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(parse_trx_counts "$trx")
          [[ "$parse_status" == "OK" ]] || return 1
          physical_rows=$((physical_rows + total))
          ;;
        LEAF_TIMED_OUT)
          # Таймаут честно называет ожидаемые строки как неисполненные, но не
          # добавляет физических UnitTestResult к проверке нижнего порога.
          ;;
        *)
          return 1
          ;;
      esac
    done < "$leaf_plan"

    printf '%s\n' "$physical_rows"
    return 0
  fi

  if attempt_dir="$(current_attempt_dir "$batch_dir")"; then
    trx="$attempt_dir/result.trx"
  else
    trx="$batch_dir/result.trx"
  fi

  IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(parse_trx_counts "$trx")
  [[ "$parse_status" == "OK" ]] || return 1
  printf '%s\n' "$total"
}

closed_batch_has_min_rows() {
  local batch_dir="$1"
  local min_rows
  local physical_rows

  min_rows="$(safe_numeric_or_default "${2:-1}" "1")"
  physical_rows="$(closed_batch_physical_rows "$batch_dir")" || return 1
  (( physical_rows >= min_rows ))
}

parse_trx_counts() {
  local trx="$1"

  # TRX может содержать текст, похожий на UnitTestResult, но быть обрезанным
  # XML. Поэтому корневой result.trx продвигается только после полноценного
  # разбора XML стандартной библиотекой Python и сверки Counters с исходами.
  if ! command -v python3 >/dev/null 2>&1; then
    printf 'INVALID\tpython3_missing\t0\t0\t0\t0\n'
    return 0
  fi

  python3 - "$trx" <<'PY'
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]

def local_name(tag: str) -> str:
    return tag.rsplit('}', 1)[-1]

def invalid(reason: str) -> None:
    print(f"INVALID\t{reason}\t0\t0\t0\t0")

try:
    tree = ET.parse(path)
except Exception as exc:
    invalid("invalid_xml:" + exc.__class__.__name__)
    sys.exit(0)

root = tree.getroot()
results = [element for element in root.iter() if local_name(element.tag) == "UnitTestResult"]
if not results:
    invalid("no_unit_test_results")
    sys.exit(0)

supported_outcomes = {"Passed", "Failed", "NotExecuted"}
outcomes = {}
for result in results:
    outcome = result.attrib.get("outcome", "")
    if not outcome:
        invalid("unit_test_result_without_outcome")
        sys.exit(0)
    # Пакетный раннер понимает только три исхода, из которых складываются
    # авторитетные числа. Новый или опечатанный исход нельзя молча потерять.
    if outcome not in supported_outcomes:
        invalid("unsupported_outcome:" + outcome)
        sys.exit(0)
    outcomes[outcome] = outcomes.get(outcome, 0) + 1

counters = next((element for element in root.iter() if local_name(element.tag) == "Counters"), None)
if counters is None:
    invalid("missing_counters")
    sys.exit(0)

required = ("total", "executed", "passed", "failed", "notExecuted")
parsed = {}
for name in required:
    raw = counters.attrib.get(name)
    if raw is None:
        invalid("missing_counter_" + name)
        sys.exit(0)
    try:
        parsed[name] = int(raw)
    except ValueError:
        invalid("non_numeric_counter_" + name)
        sys.exit(0)
    if parsed[name] < 0:
        invalid("negative_counter_" + name)
        sys.exit(0)

# VSTest добавляет подробные семейства исходов в Counters. Пока физические
# строки поддерживают только Passed/Failed/NotExecuted, все эти семейства
# обязаны быть числовыми нулями: ненулевое значение означало бы потерянный
# исход, а неизвестный атрибут — непонятную новую семантику результата.
extra_counter_names = {
    "error", "timeout", "aborted", "inconclusive", "passedButRunAborted",
    "notRunnable", "disconnected", "warning", "completed", "inProgress",
    "pending",
}
allowed_counter_names = set(required) | extra_counter_names
for name, raw in counters.attrib.items():
    if name not in allowed_counter_names:
        invalid("unsupported_counter_attribute:" + name)
        sys.exit(0)
    if name in extra_counter_names:
        try:
            value = int(raw)
        except ValueError:
            invalid("non_numeric_counter_" + name)
            sys.exit(0)
        if value != 0:
            invalid("unsupported_counter_nonzero:" + name)
            sys.exit(0)

passed = outcomes.get("Passed", 0)
failed = outcomes.get("Failed", 0)
not_executed = outcomes.get("NotExecuted", 0)
total = len(results)

if parsed["total"] != total or parsed["passed"] != passed or parsed["failed"] != failed:
    invalid("counter_outcome_mismatch")
    sys.exit(0)

# Исполненными считаются только физические Passed/Failed строки. Это точное
# равенство закрывает старую лазейку, где executed проверялся лишь диапазоном.
if parsed["executed"] != passed + failed:
    invalid("counter_executed_outcome_mismatch")
    sys.exit(0)

# Каноническое число неисполненных строк берется из физических NotExecuted.
# Сумма обязана в точности восстановить total. Сам VSTest в части честных TRX
# оставляет одноименный служебный атрибут равным нулю; поэтому допускаются лишь
# ноль или точное физическое число, но никакое третье значение.
if parsed["total"] != parsed["executed"] + not_executed:
    invalid("counter_total_partition_mismatch")
    sys.exit(0)
if parsed["notExecuted"] not in (0, not_executed):
    invalid("counter_not_executed_mismatch")
    sys.exit(0)

print(f"OK\tparsed\t{total}\t{passed}\t{failed}\t{not_executed}")
PY
}

validate_leaf_trx_membership() {
  local trx="$1"
  local method_plan="$2"
  local leaf_id="$3"
  local expected_rows="$4"
  local details_path="$5"

  if ! command -v python3 >/dev/null 2>&1; then
    printf 'INVALID\tpython3_missing\t0\t0\t0\t0\n'
    return 0
  fi

  python3 - "$trx" "$method_plan" "$leaf_id" "$expected_rows" "$details_path.tmp.$$" <<'PY'
import sys
import xml.etree.ElementTree as ET
from collections import Counter, OrderedDict

trx_path, method_plan_path, leaf_id, expected_rows_raw, details_path = sys.argv[1:6]

def local_name(tag: str) -> str:
    return tag.rsplit('}', 1)[-1]

def invalid(reason: str) -> None:
    with open(details_path, "w", encoding="utf-8", newline="\n") as details:
        details.write("method_fqn\texpected_rows\tactual_rows\tstatus\n")
        for method, expected_count in expected.items():
            actual = actual_by_method.get(method, 0)
            status = "OK" if actual == expected_count else ("EXTRA_ROWS" if actual > expected_count else "MISSING_ROWS")
            details.write(f"{method}\t{expected_count}\t{actual}\t{status}\n")
        for method, actual in actual_by_method.items():
            if method not in expected:
                details.write(f"{method}\t0\t{actual}\tFOREIGN\n")
    print(f"INVALID\t{reason}\t0\t0\t0\t0")

expected = OrderedDict()
actual_by_method = Counter()

try:
    expected_rows_from_plan = int(expected_rows_raw)
except ValueError:
    expected_rows_from_plan = -1

try:
    with open(method_plan_path, encoding="utf-8") as source:
        header = source.readline().rstrip("\n")
        if header == "method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows":
            index = {"method": 0, "leaf": 2, "rows": 4}
        elif header == "method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key":
            index = {"method": 0, "leaf": 2, "rows": 4}
        elif header == "method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind":
            index = {"method": 0, "leaf": 2, "rows": 4}
        elif header == "method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind":
            index = {"method": 0, "leaf": -1, "rows": 1}
        else:
            print("INVALID\tleaf_methods_bad_header\t0\t0\t0\t0")
            sys.exit(0)
        for line_number, line in enumerate(source, start=2):
            if not line.strip():
                continue
            parts = line.rstrip("\n").split("\t")
            required_columns = [value for value in index.values() if value >= 0]
            if len(parts) <= max(required_columns):
                print(f"INVALID\tleaf_methods_bad_row:{line_number}\t0\t0\t0\t0")
                sys.exit(0)
            if index["leaf"] >= 0 and leaf_id != "*" and parts[index["leaf"]] != leaf_id:
                continue
            method = parts[index["method"]]
            try:
                rows = int(parts[index["rows"]])
            except ValueError:
                print(f"INVALID\tleaf_methods_non_numeric_rows:{line_number}\t0\t0\t0\t0")
                sys.exit(0)
            if method in expected:
                print(f"INVALID\tleaf_methods_duplicate:{method}\t0\t0\t0\t0")
                sys.exit(0)
            expected[method] = rows
except OSError as exc:
    print(f"INVALID\tleaf_methods_unreadable:{exc.__class__.__name__}\t0\t0\t0\t0")
    sys.exit(0)

if not expected:
    print("INVALID\tleaf_methods_empty_for_leaf\t0\t0\t0\t0")
    sys.exit(0)

if sum(expected.values()) != expected_rows_from_plan:
    print(f"INVALID\tleaf_plan_expected_rows_mismatch:{sum(expected.values())}!={expected_rows_from_plan}\t0\t0\t0\t0")
    sys.exit(0)

try:
    tree = ET.parse(trx_path)
except Exception as exc:
    print(f"INVALID\tinvalid_xml:{exc.__class__.__name__}\t0\t0\t0\t0")
    sys.exit(0)

root = tree.getroot()
definitions = {}
for unit in [element for element in root.iter() if local_name(element.tag) == "UnitTest"]:
    test_id = unit.attrib.get("id")
    if not test_id:
        print("INVALID\tunit_test_without_id\t0\t0\t0\t0")
        sys.exit(0)
    if test_id in definitions:
        print(f"INVALID\tduplicate_unit_test_id:{test_id}\t0\t0\t0\t0")
        sys.exit(0)
    method_element = next((child for child in unit.iter() if local_name(child.tag) == "TestMethod"), None)
    class_name = ""
    method_name = ""
    if method_element is not None:
        class_name = method_element.attrib.get("className", "").split(",", 1)[0].strip()
        method_name = method_element.attrib.get("name", "").split("(", 1)[0].strip()
    fallback_name = unit.attrib.get("name", "").split("(", 1)[0].strip()
    if class_name and method_name:
        if method_name.startswith(class_name + "."):
            fqn = method_name
        else:
            fqn = class_name + "." + method_name
    elif fallback_name and "." in fallback_name:
        fqn = fallback_name
    else:
        print(f"INVALID\tdefinition_without_method_identity:{test_id}\t0\t0\t0\t0")
        sys.exit(0)
    definitions[test_id] = fqn

if not definitions:
    print("INVALID\tno_unit_test_definitions\t0\t0\t0\t0")
    sys.exit(0)

results = [element for element in root.iter() if local_name(element.tag) == "UnitTestResult"]
if not results:
    print("INVALID\tno_unit_test_results\t0\t0\t0\t0")
    sys.exit(0)

supported_outcomes = {"Passed", "Failed", "NotExecuted"}
outcomes = Counter()
for result in results:
    test_id = result.attrib.get("testId")
    if not test_id:
        invalid("unit_test_result_without_test_id")
        sys.exit(0)
    if test_id not in definitions:
        invalid("unit_test_result_unknown_test_id:" + test_id)
        sys.exit(0)
    outcome = result.attrib.get("outcome", "")
    if outcome not in supported_outcomes:
        invalid("unsupported_outcome:" + outcome)
        sys.exit(0)
    method = definitions[test_id]
    actual_by_method[method] += 1
    outcomes[outcome] += 1

foreign = [method for method in actual_by_method if method not in expected]
if foreign:
    invalid("foreign_method:" + foreign[0])
    sys.exit(0)

for method, expected_count in expected.items():
    actual_count = actual_by_method.get(method, 0)
    # estimated_rows — это упаковочная оценка для leaf-пака. VSTest может
    # раскрыть один [Theory] в несколько data rows того же метода; это не чужой
    # тест, поэтому запрещён только недобор строк, а не честный избыток.
    if actual_count < expected_count:
        invalid(f"method_row_count_mismatch:{method}:{actual_count}<{expected_count}")
        sys.exit(0)

total = sum(actual_by_method.values())
if total < expected_rows_from_plan:
    invalid(f"leaf_total_rows_below_plan:{total}<{expected_rows_from_plan}")
    sys.exit(0)

counters = next((element for element in root.iter() if local_name(element.tag) == "Counters"), None)
if counters is None:
    invalid("missing_counters")
    sys.exit(0)
try:
    parsed_total = int(counters.attrib.get("total", ""))
    parsed_executed = int(counters.attrib.get("executed", ""))
    parsed_passed = int(counters.attrib.get("passed", ""))
    parsed_failed = int(counters.attrib.get("failed", ""))
    parsed_not_executed = int(counters.attrib.get("notExecuted", "0"))
except ValueError:
    invalid("non_numeric_counter")
    sys.exit(0)

passed = outcomes.get("Passed", 0)
failed = outcomes.get("Failed", 0)
not_executed = outcomes.get("NotExecuted", 0)
if parsed_total != total or parsed_passed != passed or parsed_failed != failed:
    invalid("counter_outcome_mismatch")
    sys.exit(0)
if parsed_executed != passed + failed:
    invalid("counter_executed_outcome_mismatch")
    sys.exit(0)
if parsed_total != parsed_executed + not_executed:
    invalid("counter_total_partition_mismatch")
    sys.exit(0)
if parsed_not_executed not in (0, not_executed):
    invalid("counter_not_executed_mismatch")
    sys.exit(0)

with open(details_path, "w", encoding="utf-8", newline="\n") as details:
    details.write("method_fqn\texpected_rows\tactual_rows\tstatus\n")
    for method, expected_count in expected.items():
        actual_count = actual_by_method.get(method, 0)
        status = "OK" if actual_count == expected_count else "EXTRA_ROWS"
        details.write(f"{method}\t{expected_count}\t{actual_count}\t{status}\n")

print(f"OK\tmembership_valid\t{total}\t{passed}\t{failed}\t{not_executed}")
PY
  local status=$?
  if [[ -f "$details_path.tmp.$$" ]]; then
    flush_path "$details_path.tmp.$$"
    mv -f "$details_path.tmp.$$" "$details_path"
    flush_path "$details_path"
  fi
  return "$status"
}

copy_result_trx() {
  local source="$1"
  local target="$2"
  local temp="$target.tmp.$$"

  # Копия результата становится авторитетной только после полного копирования
  # во временный файл и атомарного rename.
  cp "$source" "$temp"
  flush_path "$temp"
  mv -f "$temp" "$target"
  flush_path "$target"
  flush_path "$(dirname "$target")"
}

extract_filter_terms() {
  local filter="$1"
  local prefix="$2"
  local token

  # Для split-плана нам нужны сами смысловые куски фильтра, а не строка целиком:
  # их можно честно разделить на две дочерние очереди.
  IFS='|&' read -ra tokens <<< "$filter"
  for token in "${tokens[@]}"; do
    token="${token#"${token%%[![:space:]]*}"}"
    token="${token%"${token##*[![:space:]]}"}"
    token="${token#(}"
    token="${token%)}"
    if [[ "$token" == "$prefix"* ]]; then
      printf '%s\n' "${token#"$prefix"}"
    fi
  done
}

build_child_filter() {
  local positive_list="$1"
  local exclusion_list="$2"
  local positive
  local exclusion
  local filter=""

  # Дочерний фильтр сначала выбирает свою половину положительных терминов, а
  # потом явно исключает вторую половину и все уже занятые first-match термины.
  IFS=$'\037' read -ra positives <<< "$positive_list"
  for positive in "${positives[@]}"; do
    [[ -n "$positive" ]] || continue
    if [[ -z "$filter" ]]; then
      filter="FullyQualifiedName~$positive"
    else
      filter="$filter|FullyQualifiedName~$positive"
    fi
  done

  if [[ -n "$filter" ]]; then
    filter="($filter)"
  fi

  IFS=$'\037' read -ra exclusions <<< "$exclusion_list"
  for exclusion in "${exclusions[@]}"; do
    [[ -n "$exclusion" ]] || continue
    if [[ -z "$filter" ]]; then
      filter="FullyQualifiedName!~$exclusion"
    else
      filter="$filter&FullyQualifiedName!~$exclusion"
    fi
  done

  printf '%s\n' "$filter"
}

write_split_plan() {
  local batch_dir="$1"
  local attempt_dir="$2"
  local effective_filter="$3"
  local temp="$batch_dir/split-plan.tsv.tmp.$$"
  local positive
  local index=0
  local child_a=""
  local child_b=""
  local exclude_for_a=""
  local exclude_for_b=""
  local existing_negative
  local child_a_filter
  local child_b_filter
  local positive_count=0

  while IFS= read -r positive; do
    [[ -n "$positive" ]] || continue
    positive_count=$((positive_count + 1))
    if (( index % 2 == 0 )); then
      child_a+=$'\037'"$positive"
      exclude_for_b+=$'\037'"$positive"
    else
      child_b+=$'\037'"$positive"
      exclude_for_a+=$'\037'"$positive"
    fi
    index=$((index + 1))
  done < <(extract_filter_terms "$effective_filter" "FullyQualifiedName~")

  while IFS= read -r existing_negative; do
    [[ -n "$existing_negative" ]] || continue
    exclude_for_a+=$'\037'"$existing_negative"
    exclude_for_b+=$'\037'"$existing_negative"
  done < <(extract_filter_terms "$effective_filter" "FullyQualifiedName!~")

  {
    printf 'child_id\tfilter\tassignment_reason\n'
    if (( positive_count >= 2 )); then
      child_a_filter="$(build_child_filter "$child_a" "$exclude_for_a")"
      child_b_filter="$(build_child_filter "$child_b" "$exclude_for_b")"
      printf 'A\t%s\tАвтоматическое деление: первая половина положительных терминов, с исключением второй половины и уже занятых строк.\n' "$child_a_filter"
      printf 'B\t%s\tАвтоматическое деление: вторая половина положительных терминов, с исключением первой половины и уже занятых строк.\n' "$child_b_filter"
    else
      printf 'manual\t%s\tРучное деление требуется: effective_filter не содержит минимум двух положительных терминов, которые можно безопасно развести автоматически.\n' "$effective_filter"
    fi
  } > "$temp"
  flush_path "$temp"
  mv -f "$temp" "$batch_dir/split-plan.tsv"
  flush_path "$batch_dir/split-plan.tsv"
  cp "$batch_dir/split-plan.tsv" "$attempt_dir/split-plan.tsv.tmp.$$"
  flush_path "$attempt_dir/split-plan.tsv.tmp.$$"
  mv -f "$attempt_dir/split-plan.tsv.tmp.$$" "$attempt_dir/split-plan.tsv"
  flush_path "$attempt_dir/split-plan.tsv"
  flush_path "$batch_dir"
  flush_path "$attempt_dir"
}

read_timeout_count() {
  local batch_dir="$1"
  local count_file="$batch_dir/timeout-count.txt"
  local value="0"

  if [[ -f "$count_file" ]]; then
    value="$(head -n 1 "$count_file" | tr -d '\r')"
  fi

  if [[ "$value" =~ ^[0-9]+$ ]]; then
    printf '%s\n' "$value"
  else
    printf '0\n'
  fi
}

record_timeout_state() {
  local batch_dir="$1"
  local attempt_dir="$2"
  local split_on_repeat_timeout="$3"
  local effective_filter="$4"
  local count

  count="$(read_timeout_count "$batch_dir")"
  count=$((count + 1))
  write_atomic "$batch_dir/timeout-count.txt" printf '%s\n' "$count"
  write_atomic "$attempt_dir/timeout-count.txt" printf '%s\n' "$count"

  if (( count == 1 )); then
    # Первый timeout не означает, что батч надо дробить: это может быть разовая
    # загрузка среды. Просим повторить тот же батч и ничего не делим.
    write_atomic "$attempt_dir/retry-required.txt" printf 'Первое превышение времени: повторите тот же батч перед делением.\n'
    printf 'RETRY_REQUIRED_TIMEOUT\n'
    return 0
  fi

  if [[ "$split_on_repeat_timeout" == "true" ]]; then
    write_split_plan "$batch_dir" "$attempt_dir" "$effective_filter"
    printf 'SPLIT_REQUIRED\n'
  else
    printf 'TIMED_OUT_BATCH\n'
  fi
}

run_corrupt_trx_self_check() {
  local check_dir="$run_root/self-check-corrupt-trx"
  local fake_trx="$check_dir/regex-shaped-invalid.trx"
  local wrong_executed_trx="$check_dir/well-formed-wrong-executed.trx"
  local parse_status
  local parse_reason
  local total
  local passed
  local failed
  local not_executed
  local corrupt_reason

  mkdir -p "$check_dir"
  # Файл нарочно содержит UnitTestResult и Counters как текст, но XML оборван.
  # Старый grep-счетчик принял бы его за один passed; новый parser обязан
  # отвергнуть его до promotion.
  write_atomic "$fake_trx" printf '<TestRun><Results><UnitTestResult outcome="Passed" /><ResultSummary><Counters total="1" passed="1" failed="0" notExecuted="0"></ResultSummary>\n'
  IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(parse_trx_counts "$fake_trx")
  if [[ "$parse_status" == "OK" ]]; then
    echo "CORRUPT_TRX_ACCEPTED"
    exit 1
  fi

  corrupt_reason="$parse_reason"
  echo "CORRUPT_TRX_REJECTED $parse_reason"

  # Второй файл является корректным XML, но лжет: одна Passed-строка при
  # executed=0. И runner, и отдельный verifier обязаны отвергать именно это
  # смысловое противоречие, а не только синтаксически оборванный XML.
  write_atomic "$wrong_executed_trx" printf '<TestRun><Results><UnitTestResult outcome="Passed" testName="self-check" /></Results><ResultSummary><Counters total="1" executed="0" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" /></ResultSummary></TestRun>\n'
  IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(parse_trx_counts "$wrong_executed_trx")
  if [[ "$parse_status" == "OK" ]]; then
    echo "WRONG_EXECUTED_TRX_ACCEPTED"
    exit 1
  fi

  # Обе строки результата пишутся атомарно через уже проверенный helper.
  write_atomic "$check_dir/result.txt" printf 'CORRUPT_TRX_REJECTED\t%s\nWRONG_EXECUTED_TRX_REJECTED\t%s\t%s\t%s\t%s\t%s\n' "$corrupt_reason" "$parse_reason" "$total" "$passed" "$failed" "$not_executed"
  echo "WRONG_EXECUTED_TRX_REJECTED $parse_reason"
}

write_runsettings() {
  local target="$1"
  local requested_session_timeout_ms="${2:-$session_timeout_ms}"
  local requested_hang_timeout="${3:-$hang_timeout}"
  local requested_test_filter="${4:-*}"
  local filter_input="$target.filter.$$"

  # RunSettings живет внутри конкретной попытки, чтобы повторный запуск не
  # смешивал настройки и результаты разных попыток. Точный список методов
  # передается здесь же: большие группы не должны зависеть от лимита argv.
  write_atomic "$filter_input" printf '%s\n' "$requested_test_filter"
  python3 - "$target.tmp.$$" "$requested_session_timeout_ms" "$requested_hang_timeout" "$filter_input" <<'PY'
import sys
import xml.sax.saxutils as sx

path, session_timeout, hang_timeout, filter_path = sys.argv[1:5]
test_filter = open(filter_path, encoding="utf-8").read()
if test_filter.endswith("\n"):
    test_filter = test_filter[:-1]
with open(path, "w", encoding="utf-8", newline="\n") as out:
    out.write("<RunSettings>\n")
    out.write("  <RunConfiguration>\n")
    out.write(f"    <TestSessionTimeout>{sx.escape(session_timeout)}</TestSessionTimeout>\n")
    if test_filter and test_filter != "*":
        out.write(f"    <TestCaseFilter>{sx.escape(test_filter)}</TestCaseFilter>\n")
    out.write("  </RunConfiguration>\n")
    out.write("  <DataCollectionRunSettings>\n")
    out.write("    <DataCollectors>\n")
    out.write('      <DataCollector friendlyName="blame">\n')
    out.write("        <Configuration>\n")
    out.write(f'          <CollectDumpOnTestSessionHang TestTimeout="{sx.escape(hang_timeout)}" HangDumpType="None" />\n')
    out.write("        </Configuration>\n")
    out.write("      </DataCollector>\n")
    out.write("    </DataCollectors>\n")
    out.write("  </DataCollectionRunSettings>\n")
    out.write("</RunSettings>\n")
PY
  rm -f "$filter_input"
  flush_path "$target.tmp.$$"
  mv -f "$target.tmp.$$" "$target"
  flush_path "$target"
  flush_path "$(dirname "$target")"
}

run_vstest() {
  local dll="$1"
  local filter="$2"
  local attempt_dir="$3"
  local requested_batch_timeout="${4:-$batch_timeout}"
  local requested_session_timeout_ms="${5:-$session_timeout_ms}"
  local requested_hang_timeout="${6:-$hang_timeout}"
  local stdout_file="$attempt_dir/stdout.log"
  local stderr_file="$attempt_dir/stderr.log"
  local stdout_temp="$stdout_file.tmp.$$"
  local stderr_temp="$stderr_file.tmp.$$"
  local settings_file="$attempt_dir/blame-hang.runsettings"
  local command_exit

  write_runsettings "$settings_file" "$requested_session_timeout_ms" "$requested_hang_timeout" "$filter"
  write_atomic "$attempt_dir/effective-filter.txt" printf '%s\n' "$filter"
  if [[ "$filter" == "*" ]]; then
    write_atomic "$attempt_dir/filter-delivery.txt" printf 'none\n'
  else
    write_atomic "$attempt_dir/filter-delivery.txt" printf 'runsettings\n'
  fi
  local command=(
    dotnet vstest "$dll"
    "/Settings:$settings_file"
    "/ResultsDirectory:$attempt_dir"
    "/Logger:trx;LogFileName=result.trx"
  )
  write_atomic "$attempt_dir/command.txt" printf '%s\n' "${command[*]}"

  # Внешний timeout защищает от зависания самого VSTest. Если он сработал,
  # батч остается проблемным, даже если процесс успел оставить частичный файл.
  timeout --foreground --signal=TERM --kill-after=60s "$requested_batch_timeout" \
    "${command[@]}" \
    > "$stdout_temp" 2> "$stderr_temp"
  command_exit="$?"

  flush_path "$stdout_temp"
  flush_path "$stderr_temp"
  mv -f "$stdout_temp" "$stdout_file"
  mv -f "$stderr_temp" "$stderr_file"
  flush_path "$stdout_file"
  flush_path "$stderr_file"
  return "$command_exit"
}

ensure_leaf_journal() {
  local leaf_journal="$1"

  if [[ ! -f "$leaf_journal" ]]; then
    write_atomic "$leaf_journal" printf 'leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter\n'
  fi
}

append_leaf_journal_row() {
  local leaf_journal="$1"
  local leaf_id="$2"
  local batch_id="$3"
  local started_utc="$4"
  local finished_utc="$5"
  local seconds="$6"
  local state="$7"
  local exit_code="$8"
  local total="$9"
  local passed="${10}"
  local failed="${11}"
  local not_executed="${12}"
  local result_path="${13}"
  local method_count="${14}"
  local estimated_rows="${15}"
  local filter="${16}"
  local temp="$leaf_journal.tmp.$$"

  # Leaf-журнал заменяется атомарно целиком, как и batch-журнал: один оборванный
  # append не должен превращать незакрытый leaf в будто бы доказанный.
  cp "$leaf_journal" "$temp"
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$leaf_id" "$batch_id" "$started_utc" "$finished_utc" "$seconds" "$state" "$exit_code" \
    "$total" "$passed" "$failed" "$not_executed" "$result_path" "$method_count" "$estimated_rows" "$filter" >> "$temp"
  flush_path "$temp"
  mv -f "$temp" "$leaf_journal"
  flush_path "$leaf_journal"
  flush_path "$(dirname "$leaf_journal")"
}

is_leaf_resume_closed() {
  local leaf_dir="$1"
  local state_file="$leaf_dir/state.txt"
  local state=""
  local attempt_dir=""

  [[ -f "$state_file" ]] || return 1
  state="$(head -n 1 "$state_file" | tr -d '\r')"
  case "$state" in
    COMPLETED_GREEN|COMPLETED_RED)
      if attempt_dir="$(current_attempt_dir "$leaf_dir")"; then
        [[ -s "$attempt_dir/result.trx" && -s "$leaf_dir/result.trx" ]]
      else
        [[ -s "$leaf_dir/result.trx" ]]
      fi
      ;;
    LEAF_TIMED_OUT)
      # Timeout leaf — закрытая красная единица учёта очереди, но не доказанный
      # TRX. Повтор той же RunRoot-папки не должен заново зависать на нём и
      # стирать остальные результаты; новый canonical прогон создаёт новую папку.
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

write_collection_source_map() {
  local source_root="$1"
  local output_file="$2"
  local exact_output_file="${output_file/source-collections.tsv/source-exact-bindings.tsv}"

  if ! command -v python3 >/dev/null 2>&1; then
    write_atomic "${output_file%/*}/leaf-source-collections.error.txt" printf 'python3_missing\n'
    return 67
  fi

  python3 - "$source_root" "$output_file.tmp.$$" "$exact_output_file.tmp.$$" <<'PY'
import re
import sys
from pathlib import Path

root, output_path, exact_output_path = sys.argv[1:4]

def strip_comments(text: str) -> str:
    out = []
    i = 0
    state = "code"
    while i < len(text):
        ch = text[i]
        nxt = text[i + 1] if i + 1 < len(text) else ""
        if state == "code":
            if ch == "/" and nxt == "/":
                state = "line_comment"
                out.extend("  ")
                i += 2
                continue
            if ch == "/" and nxt == "*":
                state = "block_comment"
                out.extend("  ")
                i += 2
                continue
            if ch == '"':
                state = "string"
            elif ch == "'":
                state = "char"
            out.append(ch)
            i += 1
            continue
        if state == "line_comment":
            out.append("\n" if ch == "\n" else " ")
            if ch == "\n":
                state = "code"
            i += 1
            continue
        if state == "block_comment":
            if ch == "*" and nxt == "/":
                out.extend("  ")
                i += 2
                state = "code"
                continue
            out.append("\n" if ch == "\n" else " ")
            i += 1
            continue
        if state == "string":
            out.append(ch)
            if ch == "\\" and nxt:
                out.append(nxt)
                i += 2
                continue
            if ch == '"':
                state = "code"
            i += 1
            continue
        if state == "char":
            out.append(ch)
            if ch == "\\" and nxt:
                out.append(nxt)
                i += 2
                continue
            if ch == "'":
                state = "code"
            i += 1
            continue
    return "".join(out)

def first_argument(args: str) -> str:
    in_string = False
    escape = False
    for index, ch in enumerate(args):
        if in_string:
            if escape:
                escape = False
            elif ch == "\\":
                escape = True
            elif ch == '"':
                in_string = False
            continue
        if ch == '"':
            in_string = True
            continue
        if ch == ",":
            return args[:index].strip()
    return args.strip()

def safe_name(value: str) -> str:
    value = value.strip().replace("global::", "")
    quoted = re.match(r'^"([^"]+)"$', value)
    if quoted:
        value = quoted.group(1)
    value = re.sub(r"[^A-Za-z0-9_.+:-]+", "_", value).strip("_")
    return value or "unknown"

def class_ranges(text: str):
    pattern = re.compile(r"(?m)^\s*(?:(?:public|internal|private|protected|sealed|abstract|partial|static|file)\s+)*(?:class|record)\s+([A-Za-z_][A-Za-z0-9_]*)\b")
    matches = list(pattern.finditer(text))
    for index, match in enumerate(matches):
        end = matches[index + 1].start() if index + 1 < len(matches) else len(text)
        yield match.group(1), match.start(), end

base = Path(root)
test_roots = [child for child in base.iterdir() if child.is_dir() and child.name.startswith("Tests")]
files = []
for test_root in test_roots:
    for file in test_root.rglob("*.cs"):
        rel = file.relative_to(base)
        if "bin" in rel.parts or "obj" in rel.parts:
            continue
        files.append(file)

file_texts = []
constants: dict[str, str] = {}
simple_constants: dict[str, set[str]] = {}
for file in sorted(files):
    text = strip_comments(file.read_text(encoding="utf-8", errors="replace"))
    file_texts.append((file, text))
    for class_name, start, end in class_ranges(text):
        segment = text[start:end]
        for const_match in re.finditer(r"\bconst\s+string\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*\"([^\"]*)\"", segment):
            name, value = const_match.group(1), const_match.group(2)
            constants[f"{class_name}.{name}"] = value
            simple_constants.setdefault(name, set()).add(value)

def resolve_collection_arg(expr: str, local_class: str | None = None) -> str | None:
    arg = first_argument(expr).replace("global::", "").strip()
    quoted = re.match(r'^"([^"]+)"$', arg)
    if quoted:
        return quoted.group(1)
    if local_class and f"{local_class}.{arg}" in constants:
        return constants[f"{local_class}.{arg}"]
    if arg in constants:
        return constants[arg]
    if "." in arg:
        key = ".".join(arg.split(".")[-2:])
        if key in constants:
            return constants[key]
    values = simple_constants.get(arg)
    if values and len(values) == 1:
        return next(iter(values))
    return None

definitions: dict[str, dict[str, str]] = {}
definition_pattern = re.compile(r"(?m)^\s*\[CollectionDefinition\s*\((.*?)\)\]", re.DOTALL)
class_decl_pattern = re.compile(r"(?ms)^\s*((?:\[[^\]]+\]\s*)*)(?:(?:public|internal|private|protected|sealed|abstract|partial|static|file)\s+)*(?:class|record)\s+([A-Za-z_][A-Za-z0-9_]*)\b([^\{;]*)\{")
collection_attr_pattern = re.compile(r"\[Collection\s*\((.*?)\)\]", re.DOTALL)
collection_fixture_pattern = re.compile(r"\bICollectionFixture\s*<")
class_fixture_pattern = re.compile(r"\bIClassFixture\s*<")
namespace_pattern = re.compile(r"(?m)^\s*namespace\s+([A-Za-z0-9_.]+)\s*[;{]")
method_diagnostic_server_class = "DesignConversionLiveLocalServerTests"
method_diagnostic_server_collection = "DesignConversionLocalHostTests"
storage_method_pack_class = "DesignConversionDpoStorageTests"

def namespace_at(text: str, position: int) -> str:
    namespace = ""
    for match in namespace_pattern.finditer(text):
        if match.start() >= position:
            break
        namespace = match.group(1)
    return namespace

def collection_attr_before_class(text: str, position: int) -> str | None:
    prefix = text[max(0, position - 800):position]
    matches = list(collection_attr_pattern.finditer(prefix))
    if not matches:
        return None
    last = matches[-1]
    # Берём только атрибут, который непосредственно относится к классу.
    # Текст в теле предыдущего класса или строке не должен влиять на карту.
    if prefix[last.end():].strip():
        return None
    return last.group(1)

def inheritance_clause(header: str) -> str:
    before_where = re.split(r"\bwhere\b", header, 1)[0]
    colon = before_where.find(":")
    if colon < 0:
        return ""
    return before_where[colon + 1:].strip()

def has_nontrivial_inheritance(header: str) -> bool:
    return bool(inheritance_clause(header))

class_headers_by_full: dict[str, list[str]] = {}
for file, text in file_texts:
    for cls in class_decl_pattern.finditer(text):
        class_name = cls.group(2)
        class_header = cls.group(3)
        namespace = namespace_at(text, cls.start())
        full = f"{namespace}.{class_name}" if namespace else class_name
        class_headers_by_full.setdefault(full, []).append(class_header)

for file, text in file_texts:
    for attr in definition_pattern.finditer(text):
        class_match = class_decl_pattern.search(text, attr.start())
        if not class_match or class_match.start() - attr.start() > 800:
            continue
        class_name = class_match.group(2)
        class_header = class_match.group(3)
        namespace = namespace_at(text, class_match.start())
        definition_full = f"{namespace}.{class_name}" if namespace else class_name
        all_definition_headers = class_headers_by_full.get(definition_full, [class_header])
        collection_name = resolve_collection_arg(attr.group(1), class_name)
        # Фактическая коллекция медленного server-класса сейчас только
        # запрещает параллельность. Если у неё появится список базовых классов
        # или интерфейсов,
        # без полноценной C#-модели нельзя доказать отсутствие fixture.
        # Проверяем все partial-части definition-класса, чтобы часть с
        # атрибутом не скрыла список базовых классов из соседней части.
        has_guarded_collection_base = (
            collection_name == method_diagnostic_server_collection and
            any(has_nontrivial_inheritance(header) for header in all_definition_headers)
        )
        has_fixture = (
            any(collection_fixture_pattern.search(header) is not None for header in all_definition_headers)
            or has_guarded_collection_base
        )
        if has_fixture and not collection_name:
            print(f"collection_fixture_definition_name_unresolved:{file}:{class_name}", file=sys.stderr)
            sys.exit(67)
        if collection_name:
            definitions[collection_name] = {
                "has_fixture": "true" if has_fixture else "false",
                "definition_class": class_name,
            }

manifest_path = base / "tools" / "test-batches" / "leaf-shared-collections.tsv"
manifest_shared: dict[str, str] = {}
if manifest_path.exists():
    with manifest_path.open(encoding="utf-8") as manifest:
        header = manifest.readline().rstrip("\n")
        if header == "collection_name\treason":
            for line in manifest:
                if not line.strip():
                    continue
                parts = (line.rstrip("\n").split("\t", 1) + [""])[:2]
                if parts[0] and parts[1]:
                    manifest_shared[parts[0]] = parts[1]

kind_rank = {"none": 0, "scheduling_only": 1, "shared_fixture": 2, "manifest_shared": 3}

def merge_class_row(
    old: tuple[str, str, str] | None,
    new: tuple[str, str, str],
) -> tuple[str, str, str]:
    if old is None:
        return new
    # Partial-класс может иметь fixture только в одной части. Для безопасности
    # сохраняем самый строгий смысл, чтобы поздняя часть без base-list не
    # ослабила route до scheduling_only/none.
    if kind_rank[new[2]] > kind_rank[old[2]]:
        return new
    return old

class_full_rows: dict[str, tuple[str, str, str]] = {}
class_source_paths: dict[str, set[str]] = {}
class_declaration_counts: dict[str, int] = {}
for file, text in file_texts:
    for cls in class_decl_pattern.finditer(text):
        attributes = cls.group(1)
        class_name = cls.group(2)
        class_header = cls.group(3)
        namespace = namespace_at(text, cls.start())
        full = f"{namespace}.{class_name}" if namespace else class_name
        attr_matches = list(collection_attr_pattern.finditer(attributes))
        collection_arg = attr_matches[-1].group(1) if attr_matches else collection_attr_before_class(text, cls.start())
        collection_name = ""
        if collection_arg is not None:
            resolved_collection = resolve_collection_arg(collection_arg, class_name)
            if not resolved_collection:
                print(f"collection_name_unresolved:{file}:{class_name}", file=sys.stderr)
                sys.exit(67)
            collection_name = resolved_collection
        definition = definitions.get(collection_name)
        has_class_fixture = class_fixture_pattern.search(class_header) is not None
        has_guarded_server_base = (
            full == method_diagnostic_server_class and
            has_nontrivial_inheritance(class_header)
        )
        has_guarded_storage_base = (
            full == storage_method_pack_class and
            has_nontrivial_inheritance(class_header)
        )
        has_guarded_storage_attributes = (
            full == storage_method_pack_class and
            bool(attributes.strip())
        )
        if collection_name in manifest_shared:
            unit_key = "collection:" + safe_name(collection_name)
            kind = "manifest_shared"
        elif definition and definition["has_fixture"] == "true":
            unit_key = "collection:" + safe_name(collection_name)
            kind = "shared_fixture"
        elif has_class_fixture or has_guarded_server_base or has_guarded_storage_base or has_guarded_storage_attributes:
            # Классовый fixture — общий ресурс самого класса. Pack и так
            # держит класс целиком, а method-diagnostic для новой медленной
            # группы должен отказаться, если такой ресурс появится позже.
            # Для точного DesignConversionLiveLocalServerTests также считаем
            # любой список базовых классов или интерфейсов небезопасным: без полноценного C#
            # compiler нельзя доказать, что base или using-alias не несёт
            # fixture, а текущий реальный класс base-list не имеет. Для
            # DesignConversionDpoStorageTests class-method-pack действует ещё
            # строже: если у точного глобального класса появится base/interface
            # или любой class-level атрибут вроде [Xunit.Collection], методовый
            # распил выключается fail-closed, потому что этот узкий маршрут
            # безопасен только для нынешнего простого partial-класса.
            unit_key = "class:" + full
            kind = "shared_fixture"
        elif full == method_diagnostic_server_class and collection_name and (
            collection_name != method_diagnostic_server_collection or definition is None
        ):
            # Безопасный текущий маршрут знает ровно одну scheduling-only
            # коллекцию. Другая коллекция или отсутствующее описание текущей
            # коллекции — это недостаток source metadata, а не разрешение
            # распилить slow server по методам.
            unit_key = "class:" + full
            kind = "shared_fixture"
        elif collection_name:
            unit_key = "class:" + full
            kind = "scheduling_only"
        else:
            unit_key = "class:" + full
            kind = "none"
        class_full_rows[full] = merge_class_row(
            class_full_rows.get(full),
            (unit_key, collection_name, kind))
        class_source_paths.setdefault(full, set()).add(str(file.relative_to(base)).replace("\\", "/"))
        class_declaration_counts[full] = class_declaration_counts.get(full, 0) + 1

class_rows: dict[str, tuple[str, str, str]] = dict(class_full_rows)
short_aliases: dict[str, list[tuple[str, tuple[str, str, str]]]] = {}
for full, row in class_full_rows.items():
    short_aliases.setdefault(full.rsplit(".", 1)[-1], []).append((full, row))

for short_name, candidates in short_aliases.items():
    merged: tuple[str, str, str] | None = None
    for _, row in candidates:
        merged = merge_class_row(merged, row)
    if merged is not None:
        # Полный ключ важнее короткого alias. Если есть глобальный класс с тем
        # же именем, alias не должен быть затёрт namespaced-классом и не должен
        # ослабить fixture-классификацию.
        class_rows.setdefault(short_name, merged)

with open(output_path, "w", encoding="utf-8", newline="\n") as out:
    out.write("class_fqn\tunit_key\tcollection_name\tcollection_kind\n")
    for class_name in sorted(class_rows):
        unit_key, collection_name, kind = class_rows[class_name]
        out.write(f"{class_name}\t{unit_key}\t{collection_name}\t{kind}\n")

with open(exact_output_path, "w", encoding="utf-8", newline="\n") as exact:
    exact.write("class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\n")
    for class_name in sorted(class_full_rows):
        unit_key, collection_name, kind = class_full_rows[class_name]
        source_paths = ",".join(sorted(class_source_paths.get(class_name, [])))
        declaration_count = class_declaration_counts.get(class_name, 0)
        exact.write(f"{class_name}\t{source_paths}\t{unit_key}\t{collection_name}\t{kind}\t{declaration_count}\texact_declaration\n")
PY

  flush_path "$output_file.tmp.$$"
  flush_path "$exact_output_file.tmp.$$"
  mv -f "$output_file.tmp.$$" "$output_file"
  mv -f "$exact_output_file.tmp.$$" "$exact_output_file"
  flush_path "$output_file"
  flush_path "$exact_output_file"
  flush_path "$(dirname "$output_file")"
}

class_first_cache_dir() {
  local dll="$1"
  local resolved
  local key

  resolved="$(path_identity "$dll")"
  key="$(sha256_text "$resolved")"
  printf '%s\n' "$run_root/class-first-selection/$key"
}

run_bounded_dotnet_evidence() {
  local evidence_dir="$1"
  local evidence_name="$2"
  local timeout_limit="$3"
  shift 3

  local stdout_file="$evidence_dir/$evidence_name.stdout.log"
  local stderr_file="$evidence_dir/$evidence_name.stderr.log"
  local exit_file="$evidence_dir/$evidence_name.exit.txt"
  local command_file="$evidence_dir/$evidence_name.command.txt"
  local started_file="$evidence_dir/$evidence_name.started-utc.txt"
  local finished_file="$evidence_dir/$evidence_name.finished-utc.txt"
  local seconds_file="$evidence_dir/$evidence_name.seconds.txt"
  local timeout_file="$evidence_dir/$evidence_name.timeout.txt"
  local started_utc
  local finished_utc
  local started_epoch
  local finished_epoch
  local command_exit

  mkdir -p "$evidence_dir"
  started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  started_epoch="$(date -u +%s)"
  {
    printf 'timeout --signal=TERM --kill-after=2s %q bash -c %q _' "$timeout_limit" '"$@"'
    local arg
    for arg in "$@"; do
      printf ' %q' "$arg"
    done
    printf '\n'
  } > "$command_file.tmp.$$"
  mv -f "$command_file.tmp.$$" "$command_file"
  write_atomic "$started_file" printf '%s\n' "$started_utc"
  write_atomic "$timeout_file" printf 'timeout\t%s\nkill_after\t2s\n' "$timeout_limit"

  # Полный список тестов может зависнуть до запуска маленького --Only батча.
  # Ограничитель здесь тот же production path, что пишет stdout/stderr/exit.
  set +e
  timeout --signal=TERM --kill-after=2s "$timeout_limit" \
    bash -c '"$@"' _ "$@" > "$stdout_file.tmp.$$" 2> "$stderr_file.tmp.$$"
  command_exit="$?"
  set -e

  finished_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  finished_epoch="$(date -u +%s)"
  flush_path "$stdout_file.tmp.$$"
  flush_path "$stderr_file.tmp.$$"
  mv -f "$stdout_file.tmp.$$" "$stdout_file"
  mv -f "$stderr_file.tmp.$$" "$stderr_file"
  write_atomic "$exit_file" printf '%s\n' "$command_exit"
  write_atomic "$finished_file" printf '%s\n' "$finished_utc"
  write_atomic "$seconds_file" printf '%s\n' "$((finished_epoch - started_epoch))"
  flush_path "$evidence_dir"
  return "$command_exit"
}

ensure_class_first_assignment() {
  local dll="$1"
  local cache_dir
  local source_map_file
  local source_exact_bindings_file
  local identity_file
  local expected_identity
  local assignment_file
  local summary_file
  local stdout_file
  local stderr_file
  local exit_file
  local command_file
  local assignment_sha_file
  local summary_sha_file
  local stdout_sha_file
  local command_exit
  local dotnet_version
  local dotnet_path
  local version_exit

  cache_dir="$(class_first_cache_dir "$dll")"
  source_map_file="$cache_dir/source-collections.tsv"
  source_exact_bindings_file="$cache_dir/source-exact-bindings.tsv"
  identity_file="$cache_dir/identity.tsv"
  expected_identity="$cache_dir/identity.expected.tsv"
  assignment_file="$cache_dir/method-assignments.tsv"
  summary_file="$cache_dir/summary.tsv"
  stdout_file="$cache_dir/list-tests.stdout.log"
  stderr_file="$cache_dir/list-tests.stderr.log"
  exit_file="$cache_dir/list-tests.exit.txt"
  command_file="$cache_dir/list-tests.command.txt"
  assignment_sha_file="$cache_dir/method-assignments.sha256"
  summary_sha_file="$cache_dir/summary.sha256"
  stdout_sha_file="$cache_dir/list-tests.stdout.sha256"

  mkdir -p "$cache_dir"
  if [[ ! -s "$dll" ]]; then
    write_atomic "$cache_dir/error.txt" printf 'test_dll_missing\t%s\n' "$dll"
    return 66
  fi

  dotnet_path="$(command -v dotnet 2>/dev/null || printf 'dotnet_not_found')"
  set +e
  run_bounded_dotnet_evidence "$cache_dir" "dotnet-version" "$leaf_timeout" dotnet --version
  version_exit="$?"
  set -e
  if [[ "$version_exit" != "0" ]]; then
    write_atomic "$cache_dir/error.txt" printf 'dotnet_version_failed\t%s\n' "$version_exit"
    return "$version_exit"
  fi
  dotnet_version="$(head -n 1 "$cache_dir/dotnet-version.stdout.log" | tr -d '\r')"
  dotnet_version="${dotnet_version:-unknown}"
  set +e
  write_collection_source_map "$repo_root" "$source_map_file"
  local source_map_exit="$?"
  set -e
  if [[ "$source_map_exit" != "0" ]]; then
    rm -f "$expected_identity"
    return "$source_map_exit"
  fi
  write_key_value_file "$expected_identity" \
    $'key\tvalue' \
    "selection_semantics	class_first_v1" \
    "head	$(git_head_identity)" \
    "test_dll	$dll" \
    "test_dll_identity	$(path_identity "$dll")" \
    "test_dll_sha256	$(sha256_file "$dll")" \
    "batches_file	$batches_file" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "runner_sha256	$(sha256_file "$0")" \
    "source_collection_map_sha256	$(sha256_file "$source_map_file")" \
    "source_exact_bindings_sha256	$(sha256_file "$source_exact_bindings_file")" \
    "dotnet_path	$dotnet_path" \
    "dotnet_version	$dotnet_version"

  if [[ -s "$assignment_file" && -s "$identity_file" ]] && cmp -s "$identity_file" "$expected_identity"; then
    if [[ -s "$assignment_sha_file" && -s "$summary_sha_file" && -s "$stdout_sha_file" ]] &&
      [[ "$(tr -d '\r\n' < "$assignment_sha_file")" == "$(sha256_file "$assignment_file")" ]] &&
      [[ "$(tr -d '\r\n' < "$summary_sha_file")" == "$(sha256_file "$summary_file")" ]] &&
      [[ "$(tr -d '\r\n' < "$stdout_sha_file")" == "$(sha256_file "$stdout_file")" ]]; then
      rm -f "$expected_identity"
      return 0
    fi
  fi

  set +e
  run_bounded_dotnet_evidence "$cache_dir" "list-tests" "$leaf_timeout" dotnet vstest "$dll" "/ListTests"
  command_exit="$?"
  set -e
  if [[ "$command_exit" != "0" ]]; then
    rm -f "$expected_identity"
    return "$command_exit"
  fi

  # Полный ListTests читается один раз на DLL, а затем каждая строка manifest
  # получает владельца по имени класса. Слова из имени метода больше не могут
  # перетянуть тест в более раннюю группу.
  set +e
  python3 - "$stdout_file" "$batches_file" "$dll" "$source_map_file" "$assignment_file.tmp.$$" "$summary_file.tmp.$$" <<'PY'
import re
import sys
from collections import OrderedDict, defaultdict

stdout_path, batches_path, target_dll, source_map_path, assignment_path, summary_path = sys.argv[1:7]

def normalize_dll(value: str) -> str:
    return value.replace("\\", "/")

def parse_tests(path: str) -> OrderedDict[str, int]:
    lines = open(path, encoding="utf-8", errors="replace").read().splitlines()
    capture = False
    counts: OrderedDict[str, int] = OrderedDict()
    for line in lines:
        if "The following Tests are available" in line:
            capture = True
            continue
        if not capture:
            continue
        text = line.strip()
        if not text:
            continue
        if text.startswith(("Passed!", "Failed!", "Total tests", "Test Run", "Microsoft", "Copyright")):
            continue
        method = text.split("(", 1)[0].strip()
        if not re.fullmatch(r"[A-Za-z0-9_.+]+", method) or "." not in method:
            continue
        counts[method] = counts.get(method, 0) + 1
    return counts

def filter_terms(filter_text: str):
    if filter_text == "*":
        return [["*"]]
    groups = []
    for or_part in filter_text.split("|"):
        terms = []
        for raw in or_part.split("&"):
            term = raw.strip()
            while term.startswith("("):
                term = term[1:].strip()
            while term.endswith(")"):
                term = term[:-1].strip()
            if term:
                terms.append(term)
        groups.append(terms)
    return groups

def class_term_matches(class_fqn: str, term: str) -> bool:
    if term == "*":
        return True
    if term.startswith("FullyQualifiedName~"):
        needle = term[len("FullyQualifiedName~"):]
        if needle.endswith("."):
            expected_class = needle[:-1]
            return class_fqn.casefold() == expected_class.casefold() or class_fqn.casefold().startswith(needle.casefold())
        return needle.casefold() in class_fqn.casefold()
    if term.startswith("FullyQualifiedName!~"):
        return not class_term_matches(class_fqn, "FullyQualifiedName~" + term[len("FullyQualifiedName!~"):])
    raise SystemExit(f"unsupported manifest filter term: {term}")

def filter_matches(class_fqn: str, filter_text: str) -> bool:
    if filter_text == "*":
        return True
    for and_terms in filter_terms(filter_text):
        if all(class_term_matches(class_fqn, term) for term in and_terms):
            return True
    return False

rows = []
with open(batches_path, encoding="utf-8-sig") as batches:
    for line_number, line in enumerate(batches, start=1):
        if not line.strip() or line.startswith("#"):
            continue
        parts = line.rstrip("\n").rstrip("\r").split("\t")
        if len(parts) != 9:
            raise SystemExit(f"batches.tsv line {line_number} must have 9 columns")
        order, batch_id, human_name, dll, filter_text, min_rows, split, platform, reason = parts
        if normalize_dll(dll) == normalize_dll(target_dll):
            rows.append((order, batch_id, filter_text))

if not rows:
    raise SystemExit(f"no manifest rows found for dll: {target_dll}")

collections = {}
with open(source_map_path, encoding="utf-8") as source:
    header = source.readline().rstrip("\n")
    if header != "class_fqn\tunit_key\tcollection_name\tcollection_kind":
        raise SystemExit("source collection map has unexpected header")
    for line in source:
        if not line.strip():
            continue
        class_fqn, unit_key, collection_name, collection_kind = line.rstrip("\n").split("\t")
        collections[class_fqn] = (unit_key, collection_name, collection_kind)
        collections.setdefault(class_fqn.rsplit(".", 1)[-1], (unit_key, collection_name, collection_kind))

counts = parse_tests(stdout_path)
if not counts:
    raise SystemExit("unfiltered VSTest discovery returned no test methods")

batch_stats = defaultdict(lambda: {"methods": 0, "rows": 0, "classes": set()})
with open(assignment_path, "w", encoding="utf-8", newline="\n") as out:
    out.write("method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind\n")
    for method, estimated_rows in counts.items():
        class_fqn = method.rsplit(".", 1)[0]
        owner = None
        for _order, batch_id, filter_text in rows:
            if filter_matches(class_fqn, filter_text):
                owner = batch_id
                break
        if owner is None:
            raise SystemExit(f"class has no first-match batch: {class_fqn}")
        unit_key, collection_name, collection_kind = collections.get(class_fqn) or collections.get(class_fqn.rsplit(".", 1)[-1]) or ("class:" + class_fqn, "", "none")
        out.write(f"{method}\t{estimated_rows}\t{class_fqn}\t{owner}\t{target_dll}\t{unit_key}\t{collection_name}\t{collection_kind}\n")
        batch_stats[owner]["methods"] += 1
        batch_stats[owner]["rows"] += estimated_rows
        batch_stats[owner]["classes"].add(class_fqn)

with open(summary_path, "w", encoding="utf-8", newline="\n") as summary:
    summary.write("batch_id\tmethod_count\testimated_rows\tclass_count\n")
    for _order, batch_id, _filter in rows:
        stats = batch_stats[batch_id]
        summary.write(f"{batch_id}\t{stats['methods']}\t{stats['rows']}\t{len(stats['classes'])}\n")
PY
  local assignment_exit="$?"
  set -e
  if [[ "$assignment_exit" != "0" ]]; then
    rm -f "$assignment_file.tmp.$$" "$summary_file.tmp.$$" "$expected_identity"
    return "$assignment_exit"
  fi
  mv -f "$assignment_file.tmp.$$" "$assignment_file"
  mv -f "$summary_file.tmp.$$" "$summary_file"
  flush_path "$assignment_file"
  flush_path "$summary_file"
  write_atomic "$assignment_sha_file" printf '%s\n' "$(sha256_file "$assignment_file")"
  write_atomic "$summary_sha_file" printf '%s\n' "$(sha256_file "$summary_file")"
  write_atomic "$stdout_sha_file" printf '%s\n' "$(sha256_file "$stdout_file")"
  mv -f "$expected_identity" "$identity_file"
  flush_path "$identity_file"
  flush_path "$cache_dir"
}

write_class_first_batch_evidence() {
  local batch_dir="$1"
  local dll="$2"
  local batch_id="$3"
  local allow_empty="${4:-false}"
  local cache_dir
  local assignment_file
  local output_file="$batch_dir/class-first-methods.tsv"
  local sha_file="$batch_dir/class-first-methods.sha256"
  local raw_stdout_file="$batch_dir/class-first-list-tests.stdout.log"
  local origin_file="$batch_dir/class-first-origin.tsv"
  local selected_count

  # Если полный список тестов недоступен, нельзя брать старый точный список
  # методов: он мог быть построен для другого файла сборки или другого HEAD.
  set +e
  ensure_class_first_assignment "$dll"
  local ensure_exit="$?"
  set -e
  if [[ "$ensure_exit" != "0" ]]; then
    return "$ensure_exit"
  fi

  cache_dir="$(class_first_cache_dir "$dll")"
  assignment_file="$cache_dir/method-assignments.tsv"
  selected_count="$(class_first_selected_method_count_from_assignment "$assignment_file" "$batch_id")" || return "$?"
  if [[ "$selected_count" == "0" ]]; then
    if [[ "$allow_empty" != "true" ]]; then
      printf 'class-first batch has no methods: %s\n' "$batch_id" >&2
      return 65
    fi

    # Переход из старого непустого RunRoot в пустой остаток должен открываться
    # в новом каталоге результатов. Иначе свежая квитанция пустоты перезапишет
    # raw/methods/origin рядом со старым leaf/TRX-журналом и сломает историю.
    if batch_dir_has_prior_nonempty_evidence "$batch_dir"; then
      printf 'Старые непустые доказательства сохранены; для подтверждённо пустого остатка используйте новый --run-root: %s\n' "$run_root" >&2
      printf 'Существующая папка группы не была перезаписана: %s\n' "$batch_dir" >&2
      return 68
    fi
  fi
  mkdir -p "$batch_dir"
  cp "$cache_dir/list-tests.stdout.log" "$raw_stdout_file.tmp.$$" || return "$?"
  mv -f "$raw_stdout_file.tmp.$$" "$raw_stdout_file" || return "$?"
  flush_path "$raw_stdout_file"
  set +e
  python3 - "$assignment_file" "$batch_id" "$output_file.tmp.$$" "$allow_empty" <<'PY'
import sys

assignment_path, batch_id, output_path, allow_empty = sys.argv[1:5]
selected = 0
with open(assignment_path, encoding="utf-8") as source, open(output_path, "w", encoding="utf-8", newline="\n") as out:
    header = source.readline().rstrip("\n")
    expected = "method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind"
    if header != expected:
        raise SystemExit("method-assignments.tsv has unexpected header")
    out.write(expected + "\n")
    for line in source:
        if not line.strip():
            continue
        parts = line.rstrip("\n").split("\t")
        if len(parts) != 8:
            raise SystemExit("method-assignments.tsv row has unexpected columns")
        if parts[3] == batch_id:
            out.write(line if line.endswith("\n") else line + "\n")
            selected += 1
if selected == 0 and allow_empty != "true":
    raise SystemExit(f"class-first batch has no methods: {batch_id}")
PY
  local selection_exit="$?"
  set -e
  if [[ "$selection_exit" != "0" ]]; then
    rm -f "$output_file.tmp.$$"
    return "$selection_exit"
  fi
  mv -f "$output_file.tmp.$$" "$output_file"
  flush_path "$output_file"
  write_atomic "$sha_file" printf '%s\n' "$(sha256_file "$output_file")"
  write_key_value_file "$origin_file" \
    $'key\tvalue' \
    "selection_semantics	class_first_v1" \
    "source	list_tests_unfiltered" \
    "test_dll	$dll" \
    "test_dll_identity	$(path_identity "$dll")" \
    "test_dll_sha256	$(sha256_file "$dll")" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "list_tests_stdout	class-first-list-tests.stdout.log" \
    "list_tests_stdout_sha256	$(sha256_file "$raw_stdout_file")" \
    "method_assignments_sha256	$(sha256_file "$assignment_file")" \
    "cache_identity_sha256	$(sha256_file "$cache_dir/identity.tsv")" \
    "selected_method_count	$(tsv_data_row_count "$output_file")" \
    "selected_estimated_rows	$(tsv_estimated_row_sum "$output_file")"
}

write_class_first_leaf_discovery() {
  local batch_dir="$1"
  local dll="$2"
  local batch_id="$3"
  local output_file="$batch_dir/leaf-discovery.tsv"
  local source_map_file="$batch_dir/leaf-source-collections.tsv"
  local source_exact_bindings_file="$batch_dir/leaf-source-exact-bindings.tsv"
  local class_first_file="$batch_dir/class-first-methods.tsv"
  local cache_dir

  # Ошибка подготовки class-first evidence должна дойти до вызывающего кода,
  # а не превратиться в повторное использование старого leaf-discovery.
  set +e
  write_class_first_batch_evidence "$batch_dir" "$dll" "$batch_id"
  local evidence_exit="$?"
  set -e
  if [[ "$evidence_exit" != "0" ]]; then
    return "$evidence_exit"
  fi
  cache_dir="$(class_first_cache_dir "$dll")"
  cp "$cache_dir/source-collections.tsv" "$source_map_file.tmp.$$" || return "$?"
  mv -f "$source_map_file.tmp.$$" "$source_map_file" || return "$?"
  flush_path "$source_map_file"
  cp "$cache_dir/source-exact-bindings.tsv" "$source_exact_bindings_file.tmp.$$" || return "$?"
  mv -f "$source_exact_bindings_file.tmp.$$" "$source_exact_bindings_file" || return "$?"
  flush_path "$source_exact_bindings_file"

  set +e
  python3 - "$class_first_file" "$output_file.tmp.$$" <<'PY'
import sys

source_path, output_path = sys.argv[1:3]
rows = 0
with open(source_path, encoding="utf-8") as source, open(output_path, "w", encoding="utf-8", newline="\n") as out:
    header = source.readline().rstrip("\n")
    expected = "method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind"
    if header != expected:
        raise SystemExit("class-first-methods.tsv has unexpected header")
    out.write("method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n")
    for line in source:
        if not line.strip():
            continue
        method, estimated_rows, class_fqn, batch_id, dll, unit_key, collection_name, collection_kind = line.rstrip("\n").split("\t")
        out.write(f"{method}\t{estimated_rows}\t{class_fqn}\t{unit_key}\t{collection_name}\t{collection_kind}\n")
        rows += 1
if rows == 0:
    raise SystemExit("class-first leaf discovery is empty")
PY
  local discovery_exit="$?"
  set -e
  if [[ "$discovery_exit" != "0" ]]; then
    rm -f "$output_file.tmp.$$"
    return "$discovery_exit"
  fi
  mv -f "$output_file.tmp.$$" "$output_file"
  flush_path "$output_file"
  write_atomic "$batch_dir/leaf-discovery.command.txt" printf 'class-first assignment from %s\n' "$cache_dir/method-assignments.tsv"
  write_atomic "$batch_dir/leaf-discovery.exit.txt" printf '0\n'
  write_atomic "$batch_dir/leaf-discovery.stdout.log" printf 'Класс-first discovery взят из полного ListTests: %s\n' "$cache_dir/list-tests.stdout.log"
  write_atomic "$batch_dir/leaf-discovery.stderr.log" printf '%s\n' ''
}

class_first_exact_filter_from_evidence() {
  local methods_file="$1"

  set +e
  python3 - "$methods_file" <<'PY'
import sys

path = sys.argv[1]
methods = []
with open(path, encoding="utf-8") as source:
    header = source.readline().rstrip("\n")
    if header != "method_fqn\testimated_rows\tclass_fqn\tbatch_id\tdll\tunit_key\tcollection_name\tcollection_kind":
        raise SystemExit("class-first-methods.tsv has unexpected header")
    for line in source:
        if not line.strip():
            continue
        method = line.rstrip("\n").split("\t", 1)[0]
        methods.append("FullyQualifiedName=" + method)
if not methods:
    raise SystemExit("class-first exact filter is empty")
print("|".join(methods))
PY
  local filter_exit="$?"
  set -e
  return "$filter_exit"
}

class_first_exact_filter() {
  local dll="$1"
  local batch_id="$2"
  local batch_dir="$3"
  local methods_file="$batch_dir/class-first-methods.tsv"

  # Совместимая обёртка для старых внутренних self-check: сначала строим свежую
  # class-first опись, а затем exact-фильтр берётся только из этого файла.
  set +e
  write_class_first_batch_evidence "$batch_dir" "$dll" "$batch_id"
  local evidence_exit="$?"
  set -e
  if [[ "$evidence_exit" != "0" ]]; then
    return "$evidence_exit"
  fi

  class_first_exact_filter_from_evidence "$methods_file"
}

class_first_assignment_totals() {
  local assignment_file="$1"

  awk -F '\t' 'NR > 1 && $0 !~ /^[[:space:]]*$/ { methods += 1; rows += $2 } END { printf "%d\t%d\n", methods + 0, rows + 0 }' "$assignment_file"
}

verifier_sha256_for_empty_evidence() {
  local verifier="$repo_root/tools/test-batches/verify-test-batches.ps1"

  [[ -s "$verifier" ]] || return 1
  sha256_file "$verifier"
}

key_value_equals() {
  local path="$1"
  local key="$2"
  local expected="$3"
  local actual

  actual="$(key_value_get "$path" "$key")" || return 1
  [[ "$actual" == "$expected" ]]
}

write_empty_residual_evidence() {
  local batch_dir="$1"
  local dll="$2"
  local batch_id="$3"
  local filter="$4"
  local evidence_file="$batch_dir/empty-residual.tsv"
  local methods_file="$batch_dir/class-first-methods.tsv"
  local raw_stdout_file="$batch_dir/class-first-list-tests.stdout.log"
  local origin_file="$batch_dir/class-first-origin.tsv"
  local cache_dir
  local assignment_file
  local total_methods
  local total_rows
  local residual_methods
  local residual_rows
  local verifier_sha

  if ! canonical_last_wildcard_for_dll "$batch_id" "$dll" "$filter"; then
    write_atomic "$batch_dir/empty-residual.error.txt" printf 'not_last_wildcard_for_dll\t%s\n' "$batch_id"
    return 65
  fi

  cache_dir="$(class_first_cache_dir "$dll")"
  assignment_file="$cache_dir/method-assignments.tsv"
  [[ -s "$methods_file" && -s "$raw_stdout_file" && -s "$origin_file" && -s "$assignment_file" && -s "$cache_dir/identity.tsv" ]] || return 66

  residual_methods="$(tsv_data_row_count "$methods_file")"
  residual_rows="$(tsv_estimated_row_sum "$methods_file")"
  IFS=$'\t' read -r total_methods total_rows < <(class_first_assignment_totals "$assignment_file")
  if (( total_methods <= 0 )); then
    write_atomic "$batch_dir/empty-residual.error.txt" printf 'raw_list_tests_empty\t%s\n' "$batch_id"
    return 65
  fi

  if (( residual_methods != 0 || residual_rows != 0 )); then
    write_atomic "$batch_dir/empty-residual.error.txt" printf 'residual_has_methods\t%s\t%s\n' "$residual_methods" "$residual_rows"
    return 65
  fi

  verifier_sha="$(verifier_sha256_for_empty_evidence)" || return "$?"
  write_key_value_file "$evidence_file" \
    $'key\tvalue' \
    "empty_semantics	class_first_empty_residual_v1" \
    "head	$(git_head_identity)" \
    "batch_id	$batch_id" \
    "filter	$filter" \
    "test_dll	$dll" \
    "test_dll_identity	$(path_identity "$dll")" \
    "test_dll_sha256	$(sha256_file "$dll")" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "runner_sha256	$(sha256_file "$0")" \
    "verifier_sha256	$verifier_sha" \
    "class_first_methods_sha256	$(sha256_file "$methods_file")" \
    "class_first_origin_sha256	$(sha256_file "$origin_file")" \
    "class_first_list_tests_sha256	$(sha256_file "$raw_stdout_file")" \
    "method_assignments_sha256	$(sha256_file "$assignment_file")" \
    "cache_identity_sha256	$(sha256_file "$cache_dir/identity.tsv")" \
    "total_runtime_methods	$total_methods" \
    "total_runtime_rows	$total_rows" \
    "residual_method_count	$residual_methods" \
    "residual_estimated_rows	$residual_rows"
}

empty_residual_evidence_current() {
  local batch_dir="$1"
  local dll="$2"
  local batch_id="$3"
  local filter="$4"
  local evidence_file="$batch_dir/empty-residual.tsv"
  local methods_file="$batch_dir/class-first-methods.tsv"
  local raw_stdout_file="$batch_dir/class-first-list-tests.stdout.log"
  local origin_file="$batch_dir/class-first-origin.tsv"
  local cache_dir
  local assignment_file
  local total_methods
  local total_rows
  local residual_methods
  local residual_rows
  local verifier_sha

  [[ -s "$evidence_file" ]] || return 1
  canonical_last_wildcard_for_dll "$batch_id" "$dll" "$filter" || return 1
  cache_dir="$(class_first_cache_dir "$dll")"
  assignment_file="$cache_dir/method-assignments.tsv"
  [[ -s "$methods_file" && -s "$raw_stdout_file" && -s "$origin_file" && -s "$assignment_file" && -s "$cache_dir/identity.tsv" ]] || return 1

  residual_methods="$(tsv_data_row_count "$methods_file")"
  residual_rows="$(tsv_estimated_row_sum "$methods_file")"
  IFS=$'\t' read -r total_methods total_rows < <(class_first_assignment_totals "$assignment_file")
  (( total_methods > 0 && residual_methods == 0 && residual_rows == 0 )) || return 1
  verifier_sha="$(verifier_sha256_for_empty_evidence)" || return 1

  key_value_equals "$evidence_file" "empty_semantics" "class_first_empty_residual_v1" || return 1
  key_value_equals "$evidence_file" "head" "$(git_head_identity)" || return 1
  key_value_equals "$evidence_file" "batch_id" "$batch_id" || return 1
  key_value_equals "$evidence_file" "filter" "$filter" || return 1
  key_value_equals "$evidence_file" "test_dll" "$dll" || return 1
  key_value_equals "$evidence_file" "test_dll_identity" "$(path_identity "$dll")" || return 1
  key_value_equals "$evidence_file" "test_dll_sha256" "$(sha256_file "$dll")" || return 1
  key_value_equals "$evidence_file" "batches_tsv_sha256" "$(sha256_file "$batches_file")" || return 1
  key_value_equals "$evidence_file" "runner_sha256" "$(sha256_file "$0")" || return 1
  key_value_equals "$evidence_file" "verifier_sha256" "$verifier_sha" || return 1
  key_value_equals "$evidence_file" "class_first_methods_sha256" "$(sha256_file "$methods_file")" || return 1
  key_value_equals "$evidence_file" "class_first_origin_sha256" "$(sha256_file "$origin_file")" || return 1
  key_value_equals "$evidence_file" "class_first_list_tests_sha256" "$(sha256_file "$raw_stdout_file")" || return 1
  key_value_equals "$evidence_file" "method_assignments_sha256" "$(sha256_file "$assignment_file")" || return 1
  key_value_equals "$evidence_file" "cache_identity_sha256" "$(sha256_file "$cache_dir/identity.tsv")" || return 1
  key_value_equals "$evidence_file" "total_runtime_methods" "$total_methods" || return 1
  key_value_equals "$evidence_file" "total_runtime_rows" "$total_rows" || return 1
  key_value_equals "$evidence_file" "residual_method_count" "0" || return 1
  key_value_equals "$evidence_file" "residual_estimated_rows" "0" || return 1
}

record_empty_residual_batch() {
  local batch_dir="$1"
  local order="$2"
  local batch_id="$3"
  local human_name="$4"
  local dll="$5"
  local filter="$6"
  local platform="$7"
  local platform_reason="$8"
  local split_on_repeat_timeout="$9"
  local started_utc

  mkdir -p "$batch_dir"
  write_empty_residual_evidence "$batch_dir" "$dll" "$batch_id" "$filter" || return "$?"
  started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  # У подтвержденно пустого остатка нет попытки VSTest и нет TRX: это не
  # "ноль прошедших", а доказательство, что полный список тестов ушёл в другие группы.
  rm -f "$batch_dir/result.trx" "$batch_dir/current-attempt.txt" "$batch_dir/exit.txt"
  write_state "$batch_dir/state.txt" "EMPTY_VERIFIED"
  write_atomic "$batch_dir/empty-residual.txt" printf 'Остаточная wildcard-группа подтверждена пустой по полному ListTests этой DLL; VSTest не запускался с пустым фильтром.\n'
  append_journal_row "$order" "$batch_id" "$human_name" "$started_utc" "$started_utc" "0" "EMPTY_VERIFIED" "empty-residual" \
    "0" "0" "0" "0" "empty-residual.tsv" "$platform" "$platform_reason" "$split_on_repeat_timeout"
}

batch_expected_rows_from_class_first() {
  local methods_file="$1"

  awk -F '\t' 'NR > 1 { total += $2 } END { print total + 0 }' "$methods_file"
}

batch_only_trx_matches_class_first() {
  local batch_dir="$1"
  local trx="$2"
  local expected_file="$batch_dir/class-first-methods.tsv"
  local details_file="$batch_dir/class-first-trx-membership.tsv"
  local expected_rows
  local status
  local reason
  local total
  local passed
  local failed
  local not_executed

  [[ -s "$expected_file" ]] || return 1
  expected_rows="$(batch_expected_rows_from_class_first "$expected_file")"
  IFS=$'\t' read -r status reason total passed failed not_executed < <(validate_leaf_trx_membership "$trx" "$expected_file" "*" "$expected_rows" "$details_file")
  write_atomic "$batch_dir/class-first-trx-membership.result.tsv" printf 'status\treason\ttotal\tpassed\tfailed\tnot_executed\n%s\t%s\t%s\t%s\t%s\t%s\n' "$status" "$reason" "$total" "$passed" "$failed" "$not_executed"
  [[ "$status" == "OK" ]]
}

write_batch_only_class_first_attempt_identity() {
  local batch_dir="$1"
  local attempt_dir="$2"
  local dll="$3"
  local effective_filter="$4"
  local expected_file="$batch_dir/class-first-methods.tsv"

  mkdir -p "$attempt_dir"
  cp "$expected_file" "$attempt_dir/class-first-methods.tsv.tmp.$$"
  mv -f "$attempt_dir/class-first-methods.tsv.tmp.$$" "$attempt_dir/class-first-methods.tsv"
  flush_path "$attempt_dir/class-first-methods.tsv"
  write_key_value_file "$attempt_dir/class-first-selection-identity.tsv" \
    $'key\tvalue' \
    "selection_semantics	class_first_v1" \
    "head	$(git_head_identity)" \
    "test_dll	$dll" \
    "test_dll_identity	$(path_identity "$dll")" \
    "test_dll_sha256	$(sha256_file "$dll")" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "runner_sha256	$(sha256_file "$0")" \
    "class_first_methods_sha256	$(sha256_file "$expected_file")" \
    "class_first_list_tests_sha256	$(sha256_file "$batch_dir/class-first-list-tests.stdout.log")" \
    "selected_filter_sha256	$(sha256_text "$effective_filter")"
}

batch_only_attempt_has_current_class_first_identity() {
  local batch_dir="$1"
  local attempt_dir="$2"
  local dll="$3"
  local effective_filter="$4"
  local current="$attempt_dir/class-first-selection-identity.tsv"
  local expected="$batch_dir/class-first-selection-identity.expected.$$"
  local expected_file="$batch_dir/class-first-methods.tsv"

  [[ -s "$current" && -s "$attempt_dir/class-first-methods.tsv" && -s "$expected_file" ]] || return 1
  write_key_value_file "$expected" \
    $'key\tvalue' \
    "selection_semantics	class_first_v1" \
    "head	$(git_head_identity)" \
    "test_dll	$dll" \
    "test_dll_identity	$(path_identity "$dll")" \
    "test_dll_sha256	$(sha256_file "$dll")" \
    "batches_tsv_sha256	$(sha256_file "$batches_file")" \
    "runner_sha256	$(sha256_file "$0")" \
    "class_first_methods_sha256	$(sha256_file "$expected_file")" \
    "class_first_list_tests_sha256	$(sha256_file "$batch_dir/class-first-list-tests.stdout.log")" \
    "selected_filter_sha256	$(sha256_text "$effective_filter")"
  if ! cmp -s "$current" "$expected"; then
    rm -f "$expected"
    return 1
  fi
  rm -f "$expected"
  [[ "$(sha256_file "$attempt_dir/class-first-methods.tsv")" == "$(sha256_file "$expected_file")" ]]
}

discover_leaf_methods() {
  local batch_dir="$1"
  local batch_id="$2"
  local dll="$3"
  local effective_filter="$4"
  local stdout_file="$batch_dir/leaf-discovery.stdout.log"
  local stderr_file="$batch_dir/leaf-discovery.stderr.log"
  local command_file="$batch_dir/leaf-discovery.command.txt"
  local output_file="$batch_dir/leaf-discovery.tsv"
  local source_map_file="$batch_dir/leaf-source-collections.tsv"
  local settings_file="$batch_dir/leaf-discovery.runsettings"
  local exit_file="$batch_dir/leaf-discovery.exit.txt"
  local command_exit
  local command=(dotnet vstest "$dll")

  mkdir -p "$batch_dir"
  if [[ ! -s "$dll" ]]; then
    write_atomic "$batch_dir/leaf-discovery.error.txt" printf 'test_dll_missing\t%s\n' "$dll"
    return 66
  fi

  if class_first_selection_enabled; then
    # Leaf-planning вызывается внутри внешней проверки с подавленным errexit,
    # поэтому статус class-first discovery нужно вернуть явно.
    set +e
    write_class_first_leaf_discovery "$batch_dir" "$dll" "$batch_id"
    local class_first_discovery_exit="$?"
    set -e
    return "$class_first_discovery_exit"
  fi

  write_runsettings "$settings_file" "$leaf_session_timeout_ms" "$leaf_hang_timeout" "$effective_filter"
  command+=("/Settings:$settings_file" "/ListTests")
  write_atomic "$command_file" printf '%s\n' "${command[*]}"
  set +e
  "${command[@]}" > "$stdout_file.tmp.$$" 2> "$stderr_file.tmp.$$"
  command_exit="$?"
  set -e
  flush_path "$stdout_file.tmp.$$"
  flush_path "$stderr_file.tmp.$$"
  mv -f "$stdout_file.tmp.$$" "$stdout_file"
  mv -f "$stderr_file.tmp.$$" "$stderr_file"
  write_atomic "$exit_file" printf '%s\n' "$command_exit"

  if [[ "$command_exit" != "0" ]]; then
    return "$command_exit"
  fi

  write_collection_source_map "$repo_root" "$source_map_file"

  python3 - "$stdout_file" "$output_file.tmp.$$" "$source_map_file" <<'PY'
import re
import sys
from collections import OrderedDict

stdout_path, output_path, source_map_path = sys.argv[1], sys.argv[2], sys.argv[3]
lines = open(stdout_path, encoding="utf-8", errors="replace").read().splitlines()
capture = False
counts = OrderedDict()

for line in lines:
    if "The following Tests are available" in line:
        capture = True
        continue
    if not capture:
        continue

    text = line.strip()
    if not text:
        continue
    if text.startswith(("Passed!", "Failed!", "Total tests", "Test Run", "Microsoft", "Copyright")):
        continue

    # VSTest/xUnit может печатать theory cases с аргументами в скобках.
    # Leaf строится по method FQN: все cases одной theory остаются в одном leaf.
    method = text.split("(", 1)[0].strip()
    if not re.match(r"^[A-Za-z0-9_.+]+$", method) or "." not in method:
        continue
    counts[method] = counts.get(method, 0) + 1

collection_by_class: dict[str, tuple[str, str, str]] = {}
with open(source_map_path, encoding="utf-8") as source:
    header = source.readline().rstrip("\n")
    if header != "class_fqn\tunit_key\tcollection_name\tcollection_kind":
        raise SystemExit("leaf-source-collections.tsv has unexpected header")
    for line in source:
        if not line.strip():
            continue
        class_fqn, unit_key, collection_name, collection_kind = line.rstrip("\n").split("\t")
        collection_by_class[class_fqn] = (unit_key, collection_name, collection_kind)
        collection_by_class[class_fqn.rsplit(".", 1)[-1]] = (unit_key, collection_name, collection_kind)

if not counts:
    print("No test methods were discovered from VSTest output.", file=sys.stderr)
    sys.exit(2)

with open(output_path, "w", encoding="utf-8", newline="\n") as out:
    out.write("method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n")
    for method, count in counts.items():
        class_fqn = method.rsplit(".", 1)[0]
        unit_key, collection_name, collection_kind = collection_by_class.get(class_fqn) or collection_by_class.get(class_fqn.rsplit(".", 1)[-1]) or ("class:" + class_fqn, "", "none")
        out.write(f"{method}\t{count}\t{class_fqn}\t{unit_key}\t{collection_name}\t{collection_kind}\n")
PY
  mv -f "$output_file.tmp.$$" "$output_file"
  flush_path "$output_file"
  flush_path "$batch_dir"
}

write_leaf_plan_from_discovery() {
  local batch_id="$1"
  local discovery_file="$2"
  local plan_file_path="$3"
  local method_plan_file="$4"
  local mode="$5"
  local target_rows="$6"
  local hard_cap_rows="$7"
  local report_file="${plan_file_path%/leaf-plan.tsv}/leaf-pack-report.tsv"
  local oversized_file="${plan_file_path%/leaf-plan.tsv}/leaf-oversized-units.tsv"

  python3 - "$batch_id" "$discovery_file" "$plan_file_path.tmp.$$" "$method_plan_file.tmp.$$" "$report_file.tmp.$$" "$oversized_file.tmp.$$" "$mode" "$target_rows" "$hard_cap_rows" <<'PY'
import re
import sys
from collections import OrderedDict

batch_id, discovery_path, plan_path, method_path, report_path, oversized_path, mode = sys.argv[1:8]
target = int(sys.argv[8])
hard_cap = int(sys.argv[9])
storage_method_pack_batch = "design-conversion-dpo-storage"
storage_method_pack_class = "DesignConversionDpoStorageTests"
storage_method_pack_slow_method = (
    "DesignConversionDpoStorageTests."
    "LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite"
)

rows = []
with open(discovery_path, encoding="utf-8") as source:
    header = source.readline().rstrip("\n")
    if header not in (
        "method_fqn\testimated_rows",
        "method_fqn\testimated_rows\tclass_fqn\tunit_key",
        "method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind",
    ):
        raise SystemExit("leaf-discovery.tsv has unexpected header")
    for line_number, line in enumerate(source, start=2):
        if not line.strip():
            continue
        parts = line.rstrip("\n").split("\t")
        if header == "method_fqn\testimated_rows" and len(parts) == 2:
            method, estimated = parts
            class_fqn = method.rsplit(".", 1)[0]
            unit_key = "class:" + class_fqn
            collection_name = ""
            collection_kind = "none"
        elif header.endswith("\tunit_key") and len(parts) == 4:
            method, estimated, class_fqn, unit_key = parts
            collection_name = ""
            collection_kind = "none" if unit_key.startswith("class:") else "shared_fixture"
        elif header.endswith("\tcollection_kind") and len(parts) == 6:
            method, estimated, class_fqn, unit_key, collection_name, collection_kind = parts
        else:
            raise SystemExit(f"leaf-discovery.tsv line {line_number} has unexpected columns")
        if not re.match(r"^[A-Za-z0-9_.+]+$", method) or "." not in method:
            raise SystemExit(f"method FQN has characters that cannot be safely used in exact VSTest filter: {method}")
        if not re.match(r"^[A-Za-z0-9_.+]+$", class_fqn):
            raise SystemExit(f"class FQN has characters that cannot be safely grouped: {class_fqn}")
        if not re.match(r"^(class|collection):[A-Za-z0-9_.+:-]+$", unit_key):
            raise SystemExit(f"unit key has characters that cannot be safely grouped: {unit_key}")
        if collection_kind not in ("none", "scheduling_only", "shared_fixture", "manifest_shared"):
            raise SystemExit(f"unsupported collection kind: {collection_kind}")
        rows.append((method, int(estimated), class_fqn, unit_key, collection_name, collection_kind))

if not rows:
    raise SystemExit("leaf discovery is empty")

if batch_id == "design-conversion-live-local-server-method-diagnostic":
    missing_metadata = sorted(
        f"{class_fqn}:source_metadata_missing"
        for _, _, class_fqn, _, _, collection_kind in rows
        if class_fqn == "DesignConversionLiveLocalServerTests"
        and collection_kind == "none"
    )
    if missing_metadata:
        # Для этого маршрута отсутствие source-map сведений не считается
        # безопасностью: текущий класс должен быть явно найден как
        # scheduling-only, иначе resume мог бы распилить скрытый fixture.
        raise SystemExit(
            "design_conversion_live_local_server_method_diagnostic_source_metadata_missing:" +
            ",".join(missing_metadata)
        )
    unexpected_collection = sorted(
        f"{class_fqn}:{collection_name or 'no_collection'}"
        for _, _, class_fqn, _, collection_name, collection_kind in rows
        if class_fqn == "DesignConversionLiveLocalServerTests"
        and collection_kind == "scheduling_only"
        and collection_name != "DesignConversionLocalHostTests"
    )
    if unexpected_collection:
        raise SystemExit(
            "design_conversion_live_local_server_method_diagnostic_source_metadata_missing:" +
            ",".join(unexpected_collection)
        )
    shared = sorted(
        f"{class_fqn}:{collection_kind}"
        for _, _, class_fqn, _, _, collection_kind in rows
        if class_fqn == "DesignConversionLiveLocalServerTests"
        and collection_kind in {"shared_fixture", "manifest_shared"}
    )
    if shared:
        raise SystemExit(
            "design_conversion_live_local_server_method_diagnostic_shared_fixture_rejected:" +
            ",".join(shared)
        )

def source_exact_bindings_path(path: str) -> str:
    if path.endswith("leaf-discovery.tsv"):
        return path[:-len("leaf-discovery.tsv")] + "leaf-source-exact-bindings.tsv"
    return path + ".source-exact-bindings.tsv"

def read_exact_source_bindings(path: str):
    bindings = {}
    try:
        with open(path, encoding="utf-8") as source:
            header = source.readline().rstrip("\n")
            expected = "class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind"
            if header != expected:
                raise SystemExit("leaf-source-exact-bindings.tsv has unexpected header")
            for line_number, line in enumerate(source, start=2):
                if not line.strip():
                    continue
                parts = line.rstrip("\n").split("\t")
                if len(parts) != 7:
                    raise SystemExit(f"leaf-source-exact-bindings.tsv line {line_number} has unexpected columns")
                if parts[0] in bindings:
                    raise SystemExit(
                        "design_conversion_dpo_storage_method_pack_source_metadata_mismatch:" +
                        f"duplicate_exact_binding:{parts[0]}"
                    )
                bindings[parts[0]] = {
                    "source_paths": parts[1],
                    "unit_key": parts[2],
                    "collection_name": parts[3],
                    "collection_kind": parts[4],
                    "declaration_count": parts[5],
                    "binding_kind": parts[6],
                }
    except FileNotFoundError:
        raise SystemExit("design_conversion_dpo_storage_method_pack_source_metadata_missing:exact_bindings_file_missing")
    return bindings

def guard_storage_method_pack(rows_to_guard):
    if mode != "class-method-pack":
        return None
    if batch_id != storage_method_pack_batch:
        raise SystemExit(f"class_method_pack_wrong_batch:{batch_id}")
    target_rows = [row for row in rows_to_guard if row[2] == storage_method_pack_class]
    if not target_rows:
        raise SystemExit("design_conversion_dpo_storage_method_pack_missing_target_class")
    exact_bindings = read_exact_source_bindings(source_exact_bindings_path(discovery_path))
    binding = exact_bindings.get(storage_method_pack_class)
    if binding is None or binding.get("binding_kind") != "exact_declaration":
        # Короткий alias от namespaced lookalike не является proof для global class.
        raise SystemExit("design_conversion_dpo_storage_method_pack_source_metadata_missing:exact_global_binding_missing")
    try:
        declaration_count = int(binding.get("declaration_count") or "0")
    except ValueError:
        declaration_count = 0
    source_paths = [item for item in (binding.get("source_paths") or "").split(",") if item]
    if declaration_count <= 0 or not source_paths:
        raise SystemExit(
            "design_conversion_dpo_storage_method_pack_source_metadata_mismatch:" +
            "empty_exact_global_binding"
        )
    if binding.get("unit_key") != "class:" + storage_method_pack_class:
        raise SystemExit(
            "design_conversion_dpo_storage_method_pack_source_metadata_mismatch:" +
            f"{storage_method_pack_class}:{binding.get('unit_key') or 'missing_unit'}"
        )
    if binding.get("collection_name") or binding.get("collection_kind") != "none":
        raise SystemExit(
            "design_conversion_dpo_storage_method_pack_shared_fixture_rejected:" +
            f"{storage_method_pack_class}:{binding.get('collection_kind') or 'unknown'}"
        )
    for _, _, class_fqn, unit_key, collection_name, collection_kind in target_rows:
        if unit_key != "class:" + storage_method_pack_class:
            raise SystemExit(
                "design_conversion_dpo_storage_method_pack_source_metadata_mismatch:" +
                f"{class_fqn}:{unit_key}"
            )
        if collection_name or collection_kind != "none":
            raise SystemExit(
                "design_conversion_dpo_storage_method_pack_shared_fixture_rejected:" +
                f"{class_fqn}:{collection_kind or 'unknown'}"
            )
    slow_rows = [row for row in target_rows if row[0] == storage_method_pack_slow_method]
    if len(slow_rows) != 1:
        raise SystemExit(
            "design_conversion_dpo_storage_method_pack_slow_method_count:" +
            f"expected=1:actual={len(slow_rows)}"
        )
    return binding

def exact_filter(methods):
    # Используем только FullyQualifiedName=...; substring/prefix-фильтры здесь
    # запрещены, чтобы Foo.Test не захватывал Foo.TestLong.
    return "|".join(f"FullyQualifiedName={method}" for method, *_ in methods)

def pack_units(method_rows):
    units = OrderedDict()
    for row in method_rows:
        units.setdefault(row[3], []).append(row)

    leaves = []
    current = []
    current_rows = 0
    oversized = []
    for unit_key, unit_methods in units.items():
        weight = sum(item[1] for item in unit_methods)
        if current and (current_rows + weight > hard_cap or current_rows + weight > target):
            leaves.append(current)
            current = []
            current_rows = 0
        if weight > hard_cap:
            # Нельзя распилить xUnit class/collection ради красивого размера:
            # такой leaf честно oversized, а диагностика method-level включается
            # только явным operator mode.
            leaves.append(unit_methods)
            oversized.append((unit_key, weight, len(unit_methods)))
            continue
        current.extend(unit_methods)
        current_rows += weight
    if current:
        leaves.append(current)
    return leaves, oversized

def method_unit(row):
    method, weight, class_fqn, _, collection_name, collection_kind = row
    return (method, weight, class_fqn, "method:" + method, collection_name, collection_kind)

def pack_method_units(method_rows):
    current = []
    current_rows = 0
    leaves = []
    oversized = []
    for row in method_rows:
        weight = row[1]
        if current and (current_rows + weight > hard_cap or current_rows + weight > target):
            leaves.append(current)
            current = []
            current_rows = 0
        if weight > hard_cap:
            leaves.append([row])
            oversized.append((row[3], weight, 1))
            continue
        current.append(row)
        current_rows += weight
    if current:
        leaves.append(current)
    return leaves, oversized

def class_method_pack(method_rows):
    guard_storage_method_pack(method_rows)
    target_rows = [method_unit(row) for row in method_rows if row[2] == storage_method_pack_class]
    non_target_rows = [row for row in method_rows if row[2] != storage_method_pack_class]
    normal_target_rows = [row for row in target_rows if row[0] != storage_method_pack_slow_method]
    slow_rows = [row for row in target_rows if row[0] == storage_method_pack_slow_method]
    target_leaves, target_oversized = pack_method_units(normal_target_rows)
    other_leaves, other_oversized = pack_units(non_target_rows)
    # Медленный метод идёт после коротких storage leaf'ов: bounded --max-leaves
    # может доказать пару малых leaf без случайного запуска исторически долгого метода.
    return target_leaves + [slow_rows] + other_leaves, target_oversized + other_oversized

def unit_split_count(leaves):
    seen = {}
    for index, methods in enumerate(leaves, start=1):
        for _, _, _, unit_key, _, _ in methods:
            seen.setdefault(unit_key, set()).add(index)
    return sum(1 for owners in seen.values() if len(owners) > 1)

def class_split_count(leaves):
    seen = {}
    for index, methods in enumerate(leaves, start=1):
        for _, _, class_fqn, _, _, _ in methods:
            seen.setdefault(class_fqn, set()).add(index)
    return sum(1 for owners in seen.values() if len(owners) > 1)

def collection_span_count(leaves, kinds):
    seen = {}
    for index, methods in enumerate(leaves, start=1):
        for _, _, _, _, collection_name, collection_kind in methods:
            if collection_name and collection_kind in kinds:
                seen.setdefault(collection_name, set()).add(index)
    return sum(1 for owners in seen.values() if len(owners) > 1)

def largest_unit(method_rows):
    weights = OrderedDict()
    kinds = {}
    for _, weight, _, unit_key, _, collection_kind in method_rows:
        weights[unit_key] = weights.get(unit_key, 0) + weight
        kinds[unit_key] = collection_kind
    key, rows_count = max(weights.items(), key=lambda item: item[1])
    return key, rows_count, kinds.get(key, "none")

def largest_effective_unit(leaves):
    # В class-method-pack storage-класс уже разложен в method:* units, поэтому
    # report должен описывать фактический план, а не исходный class: unit из discovery.
    flattened = [row for methods in leaves for row in methods]
    return largest_unit(flattened)

leaves = []
oversized = []
if mode == "method-diagnostic":
    for item in rows:
        leaves.append([item])
elif mode == "pack":
    leaves, oversized = pack_units(rows)
elif mode == "class-method-pack":
    leaves, oversized = class_method_pack(rows)
else:
    raise SystemExit(f"unsupported leaf mode: {mode}")

with open(plan_path, "w", encoding="utf-8", newline="\n") as plan:
    plan.write("leaf_id\tbatch_id\tleaf_order\tmethod_count\testimated_rows\tfilter\n")
    for index, methods in enumerate(leaves, start=1):
        leaf_id = f"leaf-{index:03d}"
        estimated = sum(weight for _, weight, _, _, _, _ in methods)
        plan.write(f"{leaf_id}\t{batch_id}\t{index:03d}\t{len(methods)}\t{estimated}\t{exact_filter(methods)}\n")

with open(method_path, "w", encoding="utf-8", newline="\n") as method_plan:
    method_plan.write("method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n")
    order = 0
    for index, methods in enumerate(leaves, start=1):
        leaf_id = f"leaf-{index:03d}"
        for method, weight, class_fqn, unit_key, collection_name, collection_kind in methods:
            order += 1
            method_plan.write(f"{method}\t{batch_id}\t{leaf_id}\t{order:05d}\t{weight}\t{class_fqn}\t{unit_key}\t{collection_name}\t{collection_kind}\n")

largest_unit_key, largest_unit_rows, largest_unit_kind = (
    largest_effective_unit(leaves) if mode == "class-method-pack" else largest_unit(rows)
)

with open(report_path, "w", encoding="utf-8", newline="\n") as report:
    report.write("metric\tvalue\n")
    report.write(f"mode\t{mode}\n")
    report.write(f"method_count\t{len(rows)}\n")
    report.write(f"leaf_count\t{len(leaves)}\n")
    report.write(f"estimated_rows\t{sum(item[1] for item in rows)}\n")
    report.write(f"class_splits\t{class_split_count(leaves)}\n")
    report.write(f"unit_splits\t{unit_split_count(leaves)}\n")
    report.write(f"real_shared_collection_splits\t{collection_span_count(leaves, {'shared_fixture', 'manifest_shared'})}\n")
    report.write(f"scheduling_only_collection_spans\t{collection_span_count(leaves, {'scheduling_only'})}\n")
    report.write(f"oversized_units\t{len(oversized)}\n")
    report.write(f"largest_unit_key\t{largest_unit_key}\n")
    report.write(f"largest_unit_rows\t{largest_unit_rows}\n")
    report.write(f"largest_unit_kind\t{largest_unit_kind}\n")
    report.write(f"target_rows\t{target}\n")
    report.write(f"hard_cap_rows\t{hard_cap}\n")
    if mode == "class-method-pack":
        target_method_count = sum(1 for item in rows if item[2] == storage_method_pack_class)
        target_estimated_rows = sum(item[1] for item in rows if item[2] == storage_method_pack_class)
        slow_leaf_count = sum(
            1 for methods in leaves
            if any(item[0] == storage_method_pack_slow_method for item in methods)
        )
        report.write(f"method_pack_class\t{storage_method_pack_class}\n")
        report.write(f"method_pack_method_count\t{target_method_count}\n")
        report.write(f"method_pack_estimated_rows\t{target_estimated_rows}\n")
        report.write(f"method_pack_slow_method\t{storage_method_pack_slow_method}\n")
        report.write(f"method_pack_slow_leaf_count\t{slow_leaf_count}\n")

with open(oversized_path, "w", encoding="utf-8", newline="\n") as oversized_out:
    oversized_out.write("unit_key\testimated_rows\tmethod_count\tunit_kind\treason\n")
    for unit_key, weight, method_count in oversized:
        unit_kind = next((item[5] for item in rows if item[3] == unit_key), "none")
        oversized_out.write(f"{unit_key}\t{weight}\t{method_count}\t{unit_kind}\tunit_exceeds_leaf_hard_cap\n")
PY
  local planner_exit="$?"
  if [[ "$planner_exit" != "0" ]]; then
    # Python-планировщик сам уже напечатал точную причину отказа. Bash обязан
    # вернуть этот код, иначе самопроверка shared fixture выглядела бы зелёной.
    return "$planner_exit"
  fi
  mv -f "$plan_file_path.tmp.$$" "$plan_file_path"
  mv -f "$method_plan_file.tmp.$$" "$method_plan_file"
  mv -f "$report_file.tmp.$$" "$report_file"
  mv -f "$oversized_file.tmp.$$" "$oversized_file"
  flush_path "$plan_file_path"
  flush_path "$method_plan_file"
  flush_path "$report_file"
  flush_path "$oversized_file"
  flush_path "$(dirname "$plan_file_path")"
}

write_leaf_fingerprint() {
  local batch_dir="$1"
  local batch_id="$2"
  local dll="$3"
  local effective_filter="$4"
  local discovery_file="$5"
  local effective_leaf_mode="${6:-$(effective_leaf_mode_for_batch "$batch_id")}"
  local target="$batch_dir/leaf-fingerprint.tsv"
  local base="$target.base.$$"
  local temp="$target.tmp.$$"
  local runsettings_file="$batch_dir/leaf-runsettings.xml"
  local dotnet_info_file="$batch_dir/leaf-dotnet-info.txt"
  local dotnet_runtimes_file="$batch_dir/leaf-dotnet-runtimes.txt"
  local testhost_file="$batch_dir/leaf-testhost-files.tsv"
  local source_map_file="$batch_dir/leaf-source-collections.tsv"
  local source_exact_bindings_file="$batch_dir/leaf-source-exact-bindings.tsv"
  local class_first_methods_file="$batch_dir/class-first-methods.tsv"
  local class_first_origin_file="$batch_dir/class-first-origin.tsv"
  local class_first_list_tests_file="$batch_dir/class-first-list-tests.stdout.log"
  local fingerprint
  local head_sha
  local dll_sha
  local runner_sha
  local verifier_sha
  local batches_sha
  local seed_sha
  local source_map_sha
  local source_exact_bindings_sha
  local class_first_methods_sha
  local class_first_origin_sha
  local class_first_list_tests_sha
  local selected_filter_sha
  local dotnet_version
  local dotnet_path
  local dotnet_sha
  local dotnet_info_sha
  local dotnet_runtimes_sha
  local runsettings_sha
  local testhost_sha
  local testhost_count
  local os_identity
  local full_suite_claim_allowed="true"

  head_sha="$(git_head_identity)"
  dll_sha="$(sha256_file "$dll")"
  runner_sha="$(sha256_file "$0")"
  verifier_sha="$(sha256_file "$repo_root/tools/test-batches/verify-test-batches.ps1")"
  batches_sha="$(sha256_file "$batches_file")"
  seed_sha="$(sha256_file "$discovery_file")"
  if [[ -f "$source_map_file" ]]; then
    source_map_sha="$(sha256_file "$source_map_file")"
  else
    source_map_sha="missing"
  fi
  if [[ -f "$source_exact_bindings_file" ]]; then
    source_exact_bindings_sha="$(sha256_file "$source_exact_bindings_file")"
  else
    source_exact_bindings_sha="missing"
  fi
  if [[ -f "$class_first_methods_file" ]]; then
    class_first_methods_sha="$(sha256_file "$class_first_methods_file")"
  else
    class_first_methods_sha="not_applicable"
  fi
  if [[ -f "$class_first_origin_file" ]]; then
    class_first_origin_sha="$(sha256_file "$class_first_origin_file")"
  else
    class_first_origin_sha="not_applicable"
  fi
  if [[ -f "$class_first_list_tests_file" ]]; then
    class_first_list_tests_sha="$(sha256_file "$class_first_list_tests_file")"
  else
    class_first_list_tests_sha="not_applicable"
  fi
  selected_filter_sha="$(sha256_text "$effective_filter")"
  dotnet_version="$(dotnet --version 2>/dev/null || printf 'unknown')"
  dotnet_path="$(command -v dotnet 2>/dev/null || printf 'dotnet_not_found')"
  if [[ -f "$dotnet_path" ]]; then
    dotnet_sha="$(sha256_file "$dotnet_path")"
  else
    dotnet_sha="unavailable"
  fi
  os_identity="$(uname -a)"
  write_runsettings "$runsettings_file" "$leaf_session_timeout_ms" "$leaf_hang_timeout"
  runsettings_sha="$(sha256_file "$runsettings_file")"

  # Runtime identity пишется в отдельные файлы: многострочный dotnet --info
  # не подходит для стабильного TSV, но его SHA участвует в fail-closed ключе.
  dotnet --info > "$dotnet_info_file.tmp.$$" 2>&1 || true
  flush_path "$dotnet_info_file.tmp.$$"
  mv -f "$dotnet_info_file.tmp.$$" "$dotnet_info_file"
  dotnet --list-runtimes > "$dotnet_runtimes_file.tmp.$$" 2>&1 || true
  flush_path "$dotnet_runtimes_file.tmp.$$"
  mv -f "$dotnet_runtimes_file.tmp.$$" "$dotnet_runtimes_file"
  dotnet_info_sha="$(sha256_file "$dotnet_info_file")"
  dotnet_runtimes_sha="$(sha256_file "$dotnet_runtimes_file")"

  {
    printf 'path\tsha256\n'
    find "$(dirname "$dll")" -maxdepth 1 -type f \( -iname 'testhost*' -o -iname 'Microsoft.TestPlatform*.dll' \) -print 2>/dev/null |
      LC_ALL=C sort |
      while IFS= read -r runtime_file; do
        printf '%s\t%s\n' "${runtime_file#"$repo_root"/}" "$(sha256_file "$runtime_file")"
      done
  } > "$testhost_file.tmp.$$"
  flush_path "$testhost_file.tmp.$$"
  mv -f "$testhost_file.tmp.$$" "$testhost_file"
  testhost_sha="$(sha256_file "$testhost_file")"
  testhost_count=$(( $(wc -l < "$testhost_file") - 1 ))
  if [[ "$verification_scope" == "FOCUSED" ]]; then
    full_suite_claim_allowed="false"
  fi

  {
    printf 'key\tvalue\n'
    printf 'head\t%s\n' "$head_sha"
    printf 'verification_scope\t%s\n' "$verification_scope"
    printf 'full_suite_claim_allowed\t%s\n' "$full_suite_claim_allowed"
    printf 'test_dll\t%s\n' "$dll"
    printf 'test_dll_sha256\t%s\n' "$dll_sha"
    printf 'runner_sha256\t%s\n' "$runner_sha"
    printf 'verifier_sha256\t%s\n' "$verifier_sha"
    printf 'batches_tsv_sha256\t%s\n' "$batches_sha"
    printf 'selected_filter_sha256\t%s\n' "$selected_filter_sha"
    printf 'selected_filter\t%s\n' "$effective_filter"
    printf 'os\t%s\n' "$os_identity"
    printf 'dotnet_path\t%s\n' "$dotnet_path"
    printf 'dotnet_sha256\t%s\n' "$dotnet_sha"
    printf 'dotnet_version\t%s\n' "$dotnet_version"
    printf 'dotnet_info_sha256\t%s\n' "$dotnet_info_sha"
    printf 'dotnet_runtimes_sha256\t%s\n' "$dotnet_runtimes_sha"
    printf 'testhost_file_count\t%s\n' "$testhost_count"
    printf 'testhost_files_sha256\t%s\n' "$testhost_sha"
    printf 'leaf_mode\t%s\n' "$leaf_mode"
    printf 'effective_leaf_mode\t%s\n' "$effective_leaf_mode"
    printf 'leaf_mode_overrides_scope\t%s\n' "$leaf_mode_overrides_scope"
    printf 'leaf_mode_overrides_path\t%s\n' "${leaf_mode_overrides_file#"$repo_root"/}"
    printf 'leaf_mode_overrides_sha256\t%s\n' "$leaf_mode_overrides_sha256"
    printf 'leaf_mode_override_row\t%s\n' "$(leaf_mode_override_row_for_batch "$batch_id")"
    printf 'leaf_target_rows\t%s\n' "$leaf_target_rows"
    printf 'leaf_hard_cap_rows\t%s\n' "$leaf_hard_cap_rows"
    printf 'leaf_timeout\t%s\n' "$leaf_timeout"
    printf 'leaf_session_timeout_ms\t%s\n' "$leaf_session_timeout_ms"
    printf 'leaf_hang_timeout\t%s\n' "$leaf_hang_timeout"
    printf 'runsettings_sha256\t%s\n' "$runsettings_sha"
    printf 'discovery_seed_sha256\t%s\n' "$seed_sha"
    printf 'source_collection_map_sha256\t%s\n' "$source_map_sha"
    printf 'source_exact_bindings_sha256\t%s\n' "$source_exact_bindings_sha"
    printf 'class_first_methods_sha256\t%s\n' "$class_first_methods_sha"
    printf 'class_first_origin_sha256\t%s\n' "$class_first_origin_sha"
    printf 'class_first_list_tests_sha256\t%s\n' "$class_first_list_tests_sha"
  } > "$base"

  fingerprint="$(sha256_file "$base")"
  cp "$base" "$temp"
  printf 'fingerprint_sha256\t%s\n' "$fingerprint" >> "$temp"
  flush_path "$temp"
  mv -f "$temp" "$target"
  rm -f "$base"
  flush_path "$target"
  flush_path "$batch_dir"
}

leaf_fingerprint_value() {
  local path="$1"

  awk -F '\t' '$1 == "fingerprint_sha256" { print $2 }' "$path" | tail -n 1
}

leaf_fingerprint_calculated_value() {
  local path="$1"
  local base="$path.recalc.$$"
  local value

  awk -F '\t' '$1 != "fingerprint_sha256" { print $0 }' "$path" > "$base"
  value="$(sha256_file "$base")"
  rm -f "$base"
  printf '%s\n' "$value"
}

validate_leaf_fingerprint() {
  local batch_dir="$1"
  local batch_id="$2"
  local dll="$3"
  local effective_filter="$4"
  local effective_leaf_mode="${5:-$(effective_leaf_mode_for_batch "$batch_id")}"
  local discovery_file="$batch_dir/leaf-discovery.tsv"
  local source_map_file="$batch_dir/leaf-source-collections.tsv"
  local source_exact_bindings_file="$batch_dir/leaf-source-exact-bindings.tsv"
  local existing="$batch_dir/leaf-fingerprint.tsv"
  local temp_dir="$batch_dir/.fingerprint-check.$$"
  local old_value
  local old_calculated
  local new_value

  [[ -f "$existing" && -f "$discovery_file" ]] || return 1
  old_value="$(leaf_fingerprint_value "$existing")"
  old_calculated="$(leaf_fingerprint_calculated_value "$existing")"
  if [[ -z "$old_value" || "$old_value" != "$old_calculated" ]]; then
    write_atomic "$batch_dir/fingerprint-mismatch.txt" printf 'reason\tstored_fingerprint_metadata_corrupt\nexpected\t%s\nactual\t%s\n' "$old_value" "$old_calculated"
    return 2
  fi

  mkdir -p "$temp_dir"
  cp "$discovery_file" "$temp_dir/leaf-discovery.tsv"
  if [[ -f "$source_map_file" ]]; then
    # Source-map входит в fingerprint так же строго, как discovery seed:
    # resume с тем же планом должен принять тот же файл, а mutation карты
    # class/collection обязана стать current_configuration_mismatch.
    cp "$source_map_file" "$temp_dir/leaf-source-collections.tsv"
  fi
  if [[ -f "$source_exact_bindings_file" ]]; then
    # Exact-binding marker отличает реально найденный global class без fixture
    # от опасного fallback none, где source metadata вообще пропала.
    cp "$source_exact_bindings_file" "$temp_dir/leaf-source-exact-bindings.tsv"
  fi
  if [[ -f "$batch_dir/class-first-methods.tsv" ]]; then
    cp "$batch_dir/class-first-methods.tsv" "$temp_dir/class-first-methods.tsv"
  fi
  if [[ -f "$batch_dir/class-first-origin.tsv" ]]; then
    cp "$batch_dir/class-first-origin.tsv" "$temp_dir/class-first-origin.tsv"
  fi
  if [[ -f "$batch_dir/class-first-list-tests.stdout.log" ]]; then
    cp "$batch_dir/class-first-list-tests.stdout.log" "$temp_dir/class-first-list-tests.stdout.log"
  fi
  write_leaf_fingerprint "$temp_dir" "$batch_id" "$dll" "$effective_filter" "$temp_dir/leaf-discovery.tsv" "$effective_leaf_mode"
  new_value="$(leaf_fingerprint_value "$temp_dir/leaf-fingerprint.tsv")"
  rm -rf "$temp_dir"

  if [[ -z "$old_value" || "$old_value" != "$new_value" ]]; then
    write_atomic "$batch_dir/fingerprint-mismatch.txt" printf 'reason\tcurrent_configuration_mismatch\nexpected\t%s\nactual\t%s\n' "$old_value" "$new_value"
    return 2
  fi
}

ensure_leaf_plan() {
  local batch_dir="$1"
  local batch_id="$2"
  local dll="$3"
  local effective_filter="$4"
  local leaf_plan="$batch_dir/leaf-plan.tsv"
  local leaf_methods="$batch_dir/leaf-methods.tsv"
  local discovery="$batch_dir/leaf-discovery.tsv"
  local effective_leaf_mode

  effective_leaf_mode="$(effective_leaf_mode_for_batch "$batch_id")"

  mkdir -p "$batch_dir"
  if [[ -f "$leaf_plan" && -f "$leaf_methods" && -f "$discovery" ]]; then
    validate_leaf_fingerprint "$batch_dir" "$batch_id" "$dll" "$effective_filter" "$effective_leaf_mode"
    return "$?"
  fi

  # Эти три шага строят новый план leaf-порций. Если любой из них не удался,
  # внешний resume обязан увидеть отказ, а не продолжить со старым файлом.
  discover_leaf_methods "$batch_dir" "$batch_id" "$dll" "$effective_filter" || return "$?"
  write_leaf_plan_from_discovery "$batch_id" "$discovery" "$leaf_plan" "$leaf_methods" "$effective_leaf_mode" "$leaf_target_rows" "$leaf_hard_cap_rows" || return "$?"
  write_leaf_fingerprint "$batch_dir" "$batch_id" "$dll" "$effective_filter" "$discovery" "$effective_leaf_mode" || return "$?"
}

leaf_aggregate() {
  local leaf_plan="$1"
  local leaf_journal="$2"

  awk -F '\t' '
    NR == FNR {
      if (FNR > 1) { planned[$1] = 1; planned_count += 1 }
      next
    }
    FNR > 1 {
      state[$1] = $6
      exit_code[$1] = $7
      total[$1] = $8
      passed[$1] = $9
      failed[$1] = $10
      notrun[$1] = $11
    }
    END {
      for (leaf in planned) {
        if (state[leaf] == "COMPLETED_GREEN" || state[leaf] == "COMPLETED_RED" || state[leaf] == "LEAF_TIMED_OUT") {
          closed += 1
          sum_total += total[leaf]
          sum_passed += passed[leaf]
          sum_failed += failed[leaf]
          sum_notrun += notrun[leaf]
          if (state[leaf] == "COMPLETED_RED" || state[leaf] == "LEAF_TIMED_OUT" || exit_code[leaf] != "0") {
            red += 1
          }
          if (state[leaf] == "LEAF_TIMED_OUT") {
            timed_out += 1
          }
        } else {
          open += 1
        }
      }
      printf "%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", planned_count, closed, open, red, timed_out, sum_total, sum_passed, sum_failed, sum_notrun
    }
  ' "$leaf_plan" "$leaf_journal"
}

validate_leaf_parent_timing() {
  local leaf_plan="$1"
  local leaf_journal="$2"

  # Parent timing публикуется в корневом journal и потом читается людьми. Поэтому
  # timestamp/seconds сначала валидируются как текст, а уже затем участвуют в
  # min/max/sum; мусор вроде "abc" не превращается в awk-овский ноль.
  python3 - "$leaf_plan" "$leaf_journal" <<'PY'
import os
import re
import sys
from datetime import datetime

leaf_plan, leaf_journal = sys.argv[1:3]
timestamp_re = re.compile(r"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$")
integer_re = re.compile(r"^[0-9]+$")
plan_header = "leaf_id\tbatch_id\tleaf_order\tmethod_count\testimated_rows\tfilter"
journal_header = "leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter"

def finish(status: str, reason: str, started: str = "", finished: str = "", seconds: int = 0) -> None:
    print(f"{status}\t{reason}\t{started}\t{finished}\t{seconds}")

def parse_timestamp(value: str, field: str, line_number: int):
    if not timestamp_re.fullmatch(value):
        finish("INVALID", f"{field}_bad_utc:{line_number}")
        sys.exit(0)
    try:
        return datetime.strptime(value, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError:
        finish("INVALID", f"{field}_impossible_utc:{line_number}")
        sys.exit(0)

try:
    with open(leaf_plan, "r", encoding="utf-8") as source:
        header = source.readline().rstrip("\n")
        if header != plan_header:
            finish("INVALID", "leaf_plan_bad_header")
            sys.exit(0)
        planned = set()
        for line_number, line in enumerate(source, start=2):
            if not line.strip():
                continue
            parts = line.rstrip("\n").split("\t")
            if len(parts) != 6 or not parts[0]:
                finish("INVALID", f"leaf_plan_bad_row:{line_number}")
                sys.exit(0)
            planned.add(parts[0])
except OSError as exc:
    finish("INVALID", f"leaf_plan_unreadable:{exc.__class__.__name__}")
    sys.exit(0)

if not planned:
    finish("INVALID", "leaf_plan_empty")
    sys.exit(0)

if not os.path.exists(leaf_journal):
    finish("INVALID", "leaf_journal_missing")
    sys.exit(0)

timings = []
try:
    with open(leaf_journal, "r", encoding="utf-8") as source:
        header = source.readline().rstrip("\n")
        if header != journal_header:
            finish("INVALID", "leaf_journal_bad_header")
            sys.exit(0)

        for line_number, line in enumerate(source, start=2):
            if not line.strip():
                continue
            parts = line.rstrip("\n").split("\t")
            if len(parts) != 15:
                finish("INVALID", f"leaf_journal_bad_column_count:{line_number}:{len(parts)}")
                sys.exit(0)

            leaf_id, batch_id, started_utc, finished_utc, seconds_text, state, exit_code, total, passed, failed, not_executed, _result_path, method_count, estimated_rows, filter_text = parts
            required = {
                "leaf_id": leaf_id,
                "batch_id": batch_id,
                "started_utc": started_utc,
                "finished_utc": finished_utc,
                "seconds": seconds_text,
                "state": state,
                "exit_code": exit_code,
                "total": total,
                "passed": passed,
                "failed": failed,
                "not_executed": not_executed,
                "method_count": method_count,
                "estimated_rows": estimated_rows,
                "filter": filter_text,
            }
            for name, value in required.items():
                if value == "":
                    finish("INVALID", f"{name}_missing:{line_number}")
                    sys.exit(0)

            if leaf_id not in planned:
                finish("INVALID", f"leaf_journal_unplanned_leaf:{line_number}:{leaf_id}")
                sys.exit(0)

            started = parse_timestamp(started_utc, "started_utc", line_number)
            finished = parse_timestamp(finished_utc, "finished_utc", line_number)
            if finished < started:
                finish("INVALID", f"finished_before_started:{line_number}")
                sys.exit(0)

            if not integer_re.fullmatch(seconds_text):
                finish("INVALID", f"seconds_not_nonnegative_integer:{line_number}")
                sys.exit(0)

            for field_name, value in {
                "total": total,
                "passed": passed,
                "failed": failed,
                "not_executed": not_executed,
                "method_count": method_count,
                "estimated_rows": estimated_rows,
            }.items():
                if not integer_re.fullmatch(value):
                    finish("INVALID", f"{field_name}_not_nonnegative_integer:{line_number}")
                    sys.exit(0)

            timings.append((started, started_utc, finished, finished_utc, int(seconds_text)))
except OSError as exc:
    finish("INVALID", f"leaf_journal_unreadable:{exc.__class__.__name__}")
    sys.exit(0)

if not timings:
    finish("INVALID", "leaf_journal_has_no_rows")
    sys.exit(0)

started_utc = min(timings, key=lambda item: item[0])[1]
finished_utc = max(timings, key=lambda item: item[2])[3]
seconds = sum(item[4] for item in timings)
finish("OK", "parent_timing_valid", started_utc, finished_utc, seconds)
PY
}

run_leaf_batch() {
  local order="$1"
  local batch_id="$2"
  local human_name="$3"
  local dll="$4"
  local effective_filter="$5"
  local min_rows="$6"
  local platform="$7"
  local platform_reason="$8"
  local split_on_repeat_timeout="$9"
  local batch_dir="$run_root/batch-$order-$batch_id"
  local leaf_plan="$batch_dir/leaf-plan.tsv"
  local leaf_journal="$batch_dir/leaf-journal.tsv"
  local leaf_id
  local plan_batch_id
  local leaf_order
  local method_count
  local estimated_rows
  local leaf_filter
  local leaf_dir
  local attempt_dir
  local started_epoch
  local started_utc
  local finished_epoch
  local finished_utc
  local seconds
  local exit_code
  local attempt_trx
  local found_trx
  local parse_status
  local parse_reason
  local total
  local passed
  local failed
  local not_executed
  local state
  local result_for_journal
  local max_leaf_count
  local max_elapsed_seconds
  local elapsed_seconds
  local aggregate
  local planned_count
  local closed_count
  local open_count
  local red_count
  local timed_out_count
  local sum_total
  local sum_passed
  local sum_failed
  local sum_notrun
  local min_required
  local physical_total=0
  local physical_rows_valid="true"
  local parent_state
  local parent_started_utc
  local parent_finished_utc
  local parent_seconds
  local timing_status
  local timing_reason

  max_leaf_count="$(safe_numeric_or_default "$max_leaves" "0")"
  max_elapsed_seconds=$(( $(safe_numeric_or_default "$max_elapsed_minutes" "0") * 60 ))

  if ! ensure_leaf_plan "$batch_dir" "$batch_id" "$dll" "$effective_filter"; then
    state="FINGERPRINT_MISMATCH"
    write_state "$batch_dir/state.txt" "$state"
    global_incomplete_reason="fingerprint_mismatch:$batch_id"
    echo "Leaf fingerprint mismatch or discovery failure: $batch_id" >&2
    return 70
  fi
  if ! validate_selected_leaf_ids_for_batch "$batch_id" "$leaf_plan"; then
    return 64
  fi

  if [[ "$plan_only" == "true" ]]; then
    # План без leaf-attempt'ов не является зелёным или красным результатом.
    # Поэтому он маркируется как неполный evidence и принимается verifier'ом
    # только в явном режиме AllowIncomplete.
    ensure_leaf_journal "$leaf_journal"
    write_state "$batch_dir/state.txt" "LEAF_INCOMPLETE"
    append_journal_row "$order" "$batch_id" "$human_name" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "0" "LEAF_INCOMPLETE" "leaf-plan-only" \
      "0" "0" "0" "0" "$leaf_journal" "$platform" "$platform_reason" "$split_on_repeat_timeout"
    global_incomplete_reason="plan_only:$batch_id"
    echo "$leaf_plan"
    return 0
  fi

  ensure_leaf_journal "$leaf_journal"
  while IFS=$'\t' read -r leaf_id plan_batch_id leaf_order method_count estimated_rows leaf_filter; do
    [[ "$leaf_id" == "leaf_id" || -z "${leaf_id:-}" ]] && continue
    leaf_dir="$batch_dir/$leaf_id"

    if is_leaf_resume_closed "$leaf_dir"; then
      echo "Пропускаю закрытый leaf: $human_name ($batch_id/$leaf_id)"
      continue
    fi

    if ! leaf_selected_for_run "$batch_id" "$leaf_id"; then
      if [[ -z "$global_incomplete_reason" ]]; then
        global_incomplete_reason="selected_leaf:$selected_leaf_ids_normalized"
      fi
      echo "Оставляю без запуска другую маленькую группу: $human_name ($batch_id/$leaf_id)"
      continue
    fi

    if invocation_leaf_budget_exhausted; then
      global_incomplete_reason="max_leaves:$max_leaf_count"
      break
    fi

    if (( max_elapsed_seconds > 0 )); then
      elapsed_seconds=$(( $(date -u +%s) - run_started_epoch ))
      if (( elapsed_seconds >= max_elapsed_seconds )); then
        global_incomplete_reason="max_elapsed_minutes:$max_elapsed_minutes"
        break
      fi
    fi

    mkdir -p "$leaf_dir"
    attempt_dir="$(next_attempt_dir "$leaf_dir")"
    mkdir -p "$attempt_dir"
    started_epoch="$(date -u +%s)"
    started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    write_atomic "$leaf_dir/current-attempt.txt" printf '%s\n' "${attempt_dir##*/}"
    write_atomic "$attempt_dir/started.txt" printf '%s\n' "$started_utc"
    write_state "$leaf_dir/state.txt" "STARTED"
    write_state "$attempt_dir/state.txt" "STARTED"
    record_invocation_leaf_attempt_started

    set +e
    run_vstest "$dll" "$leaf_filter" "$attempt_dir" "$leaf_timeout" "$leaf_session_timeout_ms" "$leaf_hang_timeout"
    exit_code="$?"
    set -e

    write_atomic "$attempt_dir/exit.txt" printf '%s\n' "$exit_code"
    write_atomic "$leaf_dir/exit.txt" printf '%s\n' "$exit_code"
    finished_epoch="$(date -u +%s)"
    finished_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    seconds=$((finished_epoch - started_epoch))

    attempt_trx="$attempt_dir/result.trx"
    if [[ ! -s "$attempt_trx" ]]; then
      found_trx="$(find "$attempt_dir" -mindepth 2 -maxdepth 3 -name '*.trx' -size +0c | head -n 1 || true)"
      if [[ -n "$found_trx" ]]; then
        copy_result_trx "$found_trx" "$attempt_trx"
      fi
    fi

    result_for_journal="$leaf_dir/result.trx"
    if [[ ! -s "$attempt_trx" ]]; then
      total=0; passed=0; failed=0; not_executed=0
      result_for_journal=""
      if [[ "$exit_code" == "124" ]]; then
        # Нет валидного TRX — значит leaf нельзя считать прошедшим или упавшим
        # по product-assert'ам. Для полной очереди он учитывается отдельно:
        # ожидаемые строки становятся not_executed, а родительский batch красным.
        total="$(safe_numeric_or_default "$estimated_rows" "0")"
        not_executed="$total"
        state="LEAF_TIMED_OUT"
      else
        state="INVALID_RESULT"
      fi
      write_atomic "$attempt_dir/trx-validation.tsv" printf 'status\treason\ttotal\tpassed\tfailed\tnot_executed\nINVALID\tmissing_result_trx\t0\t0\t0\t0\n'
    else
      IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(validate_leaf_trx_membership "$attempt_trx" "$batch_dir/leaf-methods.tsv" "$leaf_id" "$estimated_rows" "$attempt_dir/trx-membership.tsv")
      write_atomic "$attempt_dir/trx-validation.tsv" printf 'status\treason\ttotal\tpassed\tfailed\tnot_executed\n%s\t%s\t%s\t%s\t%s\t%s\n' "$parse_status" "$parse_reason" "$total" "$passed" "$failed" "$not_executed"
      if [[ "$exit_code" == "124" ]]; then
        # Даже если VSTest успел оставить частичный TRX, timeout делает leaf
        # недоказанным: не продвигаем TRX в корень и считаем весь план leaf'а
        # явным not_executed, чтобы verifier видел красную/blocked строку.
        state="LEAF_TIMED_OUT"
        result_for_journal=""
        total="$(safe_numeric_or_default "$estimated_rows" "$total")"
        passed=0
        failed=0
        not_executed="$total"
      elif [[ "$parse_status" != "OK" ]]; then
        state="INVALID_RESULT"
        result_for_journal=""
      else
        copy_result_trx "$attempt_trx" "$leaf_dir/result.trx"
        if (( total < 1 )); then
          state="INVALID_TOO_FEW_ROWS"
          result_for_journal=""
        elif [[ "$failed" -gt 0 || "$exit_code" -ne 0 ]]; then
          state="COMPLETED_RED"
        else
          state="COMPLETED_GREEN"
        fi
      fi
    fi

    write_state "$attempt_dir/state.txt" "$state"
    write_state "$leaf_dir/state.txt" "$state"
    append_leaf_journal_row "$leaf_journal" "$leaf_id" "$batch_id" "$started_utc" "$finished_utc" "$seconds" "$state" "$exit_code" \
      "$total" "$passed" "$failed" "$not_executed" "$result_for_journal" "$method_count" "$estimated_rows" "$leaf_filter"
  done < "$leaf_plan"

  aggregate="$(leaf_aggregate "$leaf_plan" "$leaf_journal")"
  IFS=$'\t' read -r planned_count closed_count open_count red_count timed_out_count sum_total sum_passed sum_failed sum_notrun <<< "$aggregate"
  min_required="$(safe_numeric_or_default "$min_rows" "1")"

  if (( open_count == 0 && closed_count == planned_count )); then
    if ! physical_total="$(closed_batch_physical_rows "$batch_dir")"; then
      physical_rows_valid="false"
      physical_total=0
    fi

    if [[ "$physical_rows_valid" != "true" ]]; then
      parent_state="INVALID_RESULT"
    elif (( timed_out_count > 0 )); then
      # Оборванный по времени leaf уже доказал проблемное выполнение. Родительская
      # группа должна быть красной из-за зависания, а не маскироваться под спор о
      # нижнем пороге строк, где TRX специально не продвигается.
      parent_state="COMPLETED_RED"
    elif (( physical_total < min_required )); then
      # Закрытый leaf-pack доказывается реальными UnitTestResult, а не числом
      # найденных методов и не ожидаемыми строками таймаута.
      parent_state="INVALID_TOO_FEW_ROWS"
    elif (( red_count > 0 || sum_failed > 0 )); then
      parent_state="COMPLETED_RED"
    else
      parent_state="COMPLETED_GREEN"
    fi
  else
    parent_state="LEAF_INCOMPLETE"
    if [[ -z "$global_incomplete_reason" ]]; then
      global_incomplete_reason="open_leaves:$batch_id"
    fi
  fi

  IFS=$'\t' read -r timing_status timing_reason parent_started_utc parent_finished_utc parent_seconds < <(validate_leaf_parent_timing "$leaf_plan" "$leaf_journal")
  write_atomic "$batch_dir/leaf-parent-timing-validation.tsv" printf 'status\treason\tstarted_utc\tfinished_utc\tseconds\n%s\t%s\t%s\t%s\t%s\n' "$timing_status" "$timing_reason" "$parent_started_utc" "$parent_finished_utc" "$parent_seconds"
  if [[ "$timing_status" != "OK" ]]; then
    # Если leaf-журнал повреждён именно во временных полях, мы не делаем вид,
    # что parent timing можно восстановить. Batch/root остаются incomplete до
    # отдельного разбора, а очередь не превращается в ложный FINISHED.
    parent_state="LEAF_INCOMPLETE"
    if [[ -z "$global_incomplete_reason" ]]; then
      global_incomplete_reason="leaf_parent_timing_invalid:$batch_id:$timing_reason"
    fi
    parent_started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    parent_finished_utc="$parent_started_utc"
    parent_seconds="0"
  fi

  write_state "$batch_dir/state.txt" "$parent_state"

  append_journal_row "$order" "$batch_id" "$human_name" "$parent_started_utc" "$parent_finished_utc" "$parent_seconds" "$parent_state" "leaf-pack" \
    "$sum_total" "$sum_passed" "$sum_failed" "$sum_notrun" "$leaf_journal" "$platform" "$platform_reason" "$split_on_repeat_timeout"
}

run_exact_filter_self_check() {
  local temp_dir="$run_root/self-check-exact-filter"
  local discovery="$temp_dir/leaf-discovery.tsv"
  local plan="$temp_dir/leaf-plan.tsv"
  local methods="$temp_dir/leaf-methods.tsv"
  local filter

  mkdir -p "$temp_dir"
  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.PrefixCase\t1\tExample.Tests\tclass:Example.Tests\t\tnone\nExample.Tests.PrefixCaseLong\t1\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_leaf_plan_from_discovery "self-check" "$discovery" "$plan" "$methods" "method-diagnostic" "38" "60"
  filter="$(awk -F '\t' '$1 == "leaf-001" { print $6 }' "$plan")"
  # Самопроверка защищает от возврата к FullyQualifiedName~: prefix Case не
  # должен поймать PrefixCaseLong, иначе leaf опять станет недоказуемым.
  if [[ "$filter" != "FullyQualifiedName=Example.Tests.PrefixCase" ]]; then
    echo "EXACT_FILTER_PREFIX_COLLISION_SELF_CHECK_FAILED"
    exit 1
  fi

  write_atomic "$temp_dir/result.txt" printf 'EXACT_FILTER_PREFIX_COLLISION_REJECTED\t%s\n' "$filter"
  echo "EXACT_FILTER_PREFIX_COLLISION_REJECTED $filter"
}

run_class_first_selection_self_check() {
  local temp_dir="$run_root/self-check-class-first-selection"
  local old_batches_file="$batches_file"
  local fake_dll="$temp_dir/Fake.Tests.dll"
  local second_fake_dll="$temp_dir/Second.Tests.dll"
  local api_batch_dir="$temp_dir/batch-02-api-dashboard"
  local residual_batch_dir="$temp_dir/batch-04-residual"
  local continuous_batch_dir="$temp_dir/batch-03-continuous-learning"
  local second_batch_dir="$temp_dir/batch-05-second-dll"
  local api_filter
  local residual_filter
  local continuous_filter
  local second_filter
  local api_rows
  local residual_methods
  local lure_methods
  local dll_hash_in_identity
  local first_cache_dir
  local second_cache_dir
  local fail_closed_exit
  local class_first_self_check_dotnet_fail="false"
  local class_first_self_check_dotnet_hang="false"
  local old_leaf_timeout="$leaf_timeout"
  local hang_exit
  local hang_cache_dir
  local hang_seconds
  local hang_sleep_pid
  local class_first_self_check_hang_pid_file="$temp_dir/hang-sleep.pid"

  mkdir -p "$temp_dir"
  write_atomic "$fake_dll" printf 'synthetic test dll identity v1\n'
  write_atomic "$second_fake_dll" printf 'synthetic second test dll identity\n'
  export fake_dll second_fake_dll class_first_self_check_dotnet_fail class_first_self_check_dotnet_hang class_first_self_check_hang_pid_file
  batches_file="$temp_dir/batches.tsv"
  write_atomic "$batches_file" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	earlier-method-lure	Earlier method-word lure	$fake_dll	FullyQualifiedName~XiigenExtension	1	true	any	self-check" \
    "02	api-dashboard	API dashboard class	$fake_dll	FullyQualifiedName~Example.ApiDashboardCatalogTests.	3	true	any	self-check" \
    "03	continuous-learning	Continuous learning class	$fake_dll	FullyQualifiedName~Example.ContinuousLearningTests.	1	true	any	self-check" \
    "04	residual	Residual class	$fake_dll	*	2	true	any	self-check" \
    "05	second-dll	Second DLL same method	$second_fake_dll	FullyQualifiedName~Example.ApiDashboardCatalogTests.	1	true	any	self-check"

  dotnet() {
    if [[ "${1:-}" == "--version" ]]; then
      printf 'self-check-dotnet\n'
      return 0
    fi
    if [[ "${1:-}" == "vstest" && "${3:-}" == "/ListTests" ]]; then
      if [[ "$class_first_self_check_dotnet_hang" == "true" ]]; then
        printf 'fakeListTests ENTER\n' >&2
        sleep 20 &
        printf '%s\n' "$!" > "$class_first_self_check_hang_pid_file"
        wait "$!"
        printf 'fakeListTests EXIT\n' >&2
        return 0
      fi
      if [[ "$class_first_self_check_dotnet_fail" == "true" ]]; then
        printf 'synthetic discovery failure\n' >&2
        return 65
      fi
      if [[ "${2:-}" == "$second_fake_dll" ]]; then
        printf '%s\n' \
          'The following Tests are available:' \
          '    Example.ApiDashboardCatalogTests.ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod'
        return 0
      fi
      printf '%s\n' \
        'The following Tests are available:' \
        '    Example.ApiDashboardCatalogTests.ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod' \
        '    Example.ApiDashboardCatalogTests.TheoryRows(1)' \
        '    Example.ApiDashboardCatalogTests.TheoryRows(2)' \
        '    Example.ApiDashboardCatalogTestsLookalike.LookalikeShouldStayOut' \
        '    Example.ContinuousLearningTests.ContinuousLearning_DpoPair_StoresChosenAndRejected' \
        '    Example.ResidualOnlyTests.ResidualMethod'
      if grep -q 'identity v2' "$fake_dll"; then
        printf '%s\n' '    Example.ApiDashboardCatalogTests.NewMethodAfterDllChange'
      fi
      return 0
    fi
    printf 'unexpected synthetic dotnet call: %s\n' "$*" >&2
    return 64
  }
  export -f dotnet

  api_filter="$(class_first_exact_filter "$fake_dll" "api-dashboard" "$api_batch_dir")"
  continuous_filter="$(class_first_exact_filter "$fake_dll" "continuous-learning" "$continuous_batch_dir")"
  residual_filter="$(class_first_exact_filter "$fake_dll" "residual" "$residual_batch_dir")"
  api_rows="$(awk -F '\t' 'NR > 1 { total += $2 } END { print total + 0 }' "$api_batch_dir/class-first-methods.tsv")"
  residual_methods="$(awk -F '\t' 'NR > 1 { print $1 }' "$residual_batch_dir/class-first-methods.tsv" | sort | tr '\n' ' ')"
  lure_methods="$(awk -F '\t' '$4 == "earlier-method-lure" { print $1 }' "$(class_first_cache_dir "$fake_dll")/method-assignments.tsv" | wc -l | tr -d ' ')"

  if [[ "$api_filter" != *"FullyQualifiedName=Example.ApiDashboardCatalogTests.ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod"* ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_METHOD_LURE_LOST"
    exit 1
  fi
  if [[ "$api_filter" == *"LookalikeShouldStayOut"* || "$residual_filter" != *"FullyQualifiedName=Example.ApiDashboardCatalogTestsLookalike.LookalikeShouldStayOut"* ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_CLASS_BOUNDARY"
    exit 1
  fi
  if [[ "$api_rows" != "3" || "$api_filter" != *"FullyQualifiedName=Example.ApiDashboardCatalogTests.TheoryRows"* ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_THEORY_ROWS rows=$api_rows filter=$api_filter"
    exit 1
  fi
  if [[ "$continuous_filter" != "FullyQualifiedName=Example.ContinuousLearningTests.ContinuousLearning_DpoPair_StoresChosenAndRejected" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_CONTINUOUS_LEARNING filter=$continuous_filter"
    exit 1
  fi
  if [[ "$residual_methods" != *"Example.ResidualOnlyTests.ResidualMethod"* || "$lure_methods" != "0" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_RESIDUAL residual=$residual_methods lure=$lure_methods"
    exit 1
  fi

  python3 - "$(class_first_cache_dir "$fake_dll")/method-assignments.tsv" <<'PY'
import sys
path = sys.argv[1]
lines = open(path, encoding="utf-8").read().splitlines()
open(path, "w", encoding="utf-8", newline="\n").write("\n".join(line for line in lines if "TheoryRows" not in line) + "\n")
PY
  api_filter="$(class_first_exact_filter "$fake_dll" "api-dashboard" "$api_batch_dir")"
  api_rows="$(awk -F '\t' 'NR > 1 { total += $2 } END { print total + 0 }' "$api_batch_dir/class-first-methods.tsv")"
  if [[ "$api_rows" != "3" || "$api_filter" != *"FullyQualifiedName=Example.ApiDashboardCatalogTests.TheoryRows"* ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_CORRUPT_CACHE_REUSED"
    exit 1
  fi

  write_atomic "$fake_dll" printf 'synthetic test dll identity v2\n'
  api_filter="$(class_first_exact_filter "$fake_dll" "api-dashboard" "$api_batch_dir")"
  api_rows="$(awk -F '\t' 'NR > 1 { total += $2 } END { print total + 0 }' "$api_batch_dir/class-first-methods.tsv")"
  dll_hash_in_identity="$(awk -F '\t' '$1 == "test_dll_sha256" { print $2 }' "$(class_first_cache_dir "$fake_dll")/identity.tsv")"
  if [[ "$api_rows" != "4" || "$api_filter" != *"FullyQualifiedName=Example.ApiDashboardCatalogTests.NewMethodAfterDllChange"* || "$dll_hash_in_identity" != "$(sha256_file "$fake_dll")" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_DLL_IDENTITY"
    exit 1
  fi

  second_filter="$(class_first_exact_filter "$second_fake_dll" "second-dll" "$second_batch_dir")"
  first_cache_dir="$(class_first_cache_dir "$fake_dll")"
  second_cache_dir="$(class_first_cache_dir "$second_fake_dll")"
  if [[ "$second_filter" != "FullyQualifiedName=Example.ApiDashboardCatalogTests.ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod" || "$first_cache_dir" == "$second_cache_dir" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_DLL_CACHE_ISOLATION second_filter=$second_filter first_cache=$first_cache_dir second_cache=$second_cache_dir"
    exit 1
  fi

  write_atomic "$fake_dll" printf 'synthetic test dll identity v3\n'
  class_first_self_check_dotnet_fail="true"
  if class_first_exact_filter "$fake_dll" "api-dashboard" "$api_batch_dir" > "$temp_dir/fail-closed-filter.stdout.log" 2> "$temp_dir/fail-closed-filter.stderr.log"; then
    fail_closed_exit=0
  else
    fail_closed_exit="$?"
  fi
  class_first_self_check_dotnet_fail="false"
  if [[ "$fail_closed_exit" == "0" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_DISCOVERY_FAILURE_REUSED_OLD_SELECTION"
    exit 1
  fi

  write_atomic "$fake_dll" printf 'synthetic test dll identity v4\n'
  class_first_self_check_dotnet_hang="true"
  leaf_timeout="2s"
  hang_cache_dir="$(class_first_cache_dir "$fake_dll")"
  if class_first_exact_filter "$fake_dll" "api-dashboard" "$api_batch_dir" > "$temp_dir/hang-filter.stdout.log" 2> "$temp_dir/hang-filter.stderr.log"; then
    hang_exit=0
  else
    hang_exit="$?"
  fi
  class_first_self_check_dotnet_hang="false"
  leaf_timeout="$old_leaf_timeout"
  if [[ "$hang_exit" != "124" && "$hang_exit" != "137" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_NOT_TIMEOUT exit=$hang_exit"
    exit 1
  fi
  if [[ ! -s "$hang_cache_dir/list-tests.exit.txt" || "$(tr -d '\r\n' < "$hang_cache_dir/list-tests.exit.txt")" != "$hang_exit" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_EXIT_EVIDENCE exit=$hang_exit"
    exit 1
  fi
  hang_seconds="$(tr -d '\r\n' < "$hang_cache_dir/list-tests.seconds.txt")"
  if [[ ! "$hang_seconds" =~ ^[0-9]+$ || "$hang_seconds" -gt 6 ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_DURATION seconds=$hang_seconds"
    exit 1
  fi
  if ! grep -q 'fakeListTests ENTER' "$hang_cache_dir/list-tests.stderr.log"; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_NO_STDERR_MARKER"
    exit 1
  fi
  if grep -q 'fakeListTests EXIT' "$hang_cache_dir/list-tests.stderr.log"; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_CHILD_COMPLETED"
    exit 1
  fi
  if [[ -s "$temp_dir/hang-filter.stdout.log" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_PRINTED_FILTER"
    exit 1
  fi
  if [[ ! -s "$class_first_self_check_hang_pid_file" ]]; then
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_NO_CHILD_PID"
    exit 1
  fi
  hang_sleep_pid="$(tr -d '\r\n' < "$class_first_self_check_hang_pid_file")"
  if kill -0 "$hang_sleep_pid" 2>/dev/null; then
    kill "$hang_sleep_pid" 2>/dev/null || true
    echo "CLASS_FIRST_SELF_CHECK_FAILED_HANGING_DISCOVERY_CHILD_STILL_ALIVE pid=$hang_sleep_pid"
    exit 1
  fi

  batches_file="$old_batches_file"
  write_atomic "$temp_dir/result.txt" printf 'CLASS_FIRST_METHOD_WORD_LURE_REJECTED\nCLASS_FIRST_BOUNDARY_LOOKALIKE_EXCLUDED\nCLASS_FIRST_THEORY_ROWS_KEPT_TOGETHER\nCLASS_FIRST_RESIDUAL_ASSIGNED\nCLASS_FIRST_CORRUPT_CACHE_REBUILT\nCLASS_FIRST_DLL_IDENTITY_CHANGED_SELECTION\nCLASS_FIRST_DLL_CACHE_ISOLATED\nCLASS_FIRST_DISCOVERY_FAILURE_REJECTED_OLD_SELECTION\nCLASS_FIRST_HANGING_DISCOVERY_TIMED_OUT\n'
  echo "CLASS_FIRST_METHOD_WORD_LURE_REJECTED"
  echo "CLASS_FIRST_BOUNDARY_LOOKALIKE_EXCLUDED"
  echo "CLASS_FIRST_THEORY_ROWS_KEPT_TOGETHER"
  echo "CLASS_FIRST_RESIDUAL_ASSIGNED"
  echo "CLASS_FIRST_CORRUPT_CACHE_REBUILT"
  echo "CLASS_FIRST_DLL_IDENTITY_CHANGED_SELECTION"
  echo "CLASS_FIRST_DLL_CACHE_ISOLATED"
  echo "CLASS_FIRST_DISCOVERY_FAILURE_REJECTED_OLD_SELECTION"
  echo "CLASS_FIRST_HANGING_DISCOVERY_TIMED_OUT"
}

run_empty_residual_self_check() {
  local temp_dir="$run_root/self-check-empty-residual"
  local old_batches_file="$batches_file"
  local fake_dll="$temp_dir/Fake.Empty.Tests.dll"
  local empty_batch_dir="$temp_dir/batch-03-residual"
  local named_empty_dir="$temp_dir/batch-02-named-empty"
  local empty_self_check_mode="named-only"
  local journal_state
  local journal_total
  local journal_passed
  local journal_failed
  local journal_not_executed
  local summary_file="$temp_dir/summary.md"
  local selected_count
  local failure_exit
  local old_batch_dir
  local old_journal_hash
  local old_leaf_hash
  local old_plan_hash
  local old_attempt_hash
  local old_class_first_hash
  local preserve_exit

  mkdir -p "$temp_dir"
  write_atomic "$fake_dll" printf 'synthetic empty residual dll v1\n'
  export fake_dll empty_self_check_mode
  batches_file="$temp_dir/batches.tsv"
  write_atomic "$batches_file" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	named-tests	Named tests	$fake_dll	FullyQualifiedName~Example.NamedTests.	1	true	any	self-check" \
    "02	named-empty	Named empty	$fake_dll	FullyQualifiedName~Example.MissingTests.	1	true	any	self-check" \
    "03	residual	Residual wildcard	$fake_dll	*	1	true	any	self-check"

  dotnet() {
    if [[ "${1:-}" == "--version" ]]; then
      printf 'self-check-dotnet\n'
      return 0
    fi
    if [[ "${1:-}" == "vstest" && "${3:-}" == "/ListTests" ]]; then
      case "$empty_self_check_mode" in
        failure)
          printf 'synthetic ListTests failure\n' >&2
          return 65
          ;;
        raw-empty)
          printf '%s\n' 'The following Tests are available:'
          return 0
          ;;
        residual-method)
          printf '%s\n' \
            'The following Tests are available:' \
            '    Example.NamedTests.Known' \
            '    Example.ResidualTests.NewMethod'
          return 0
          ;;
        *)
          printf '%s\n' \
            'The following Tests are available:' \
            '    Example.NamedTests.Known'
          return 0
          ;;
      esac
    fi
    printf 'unexpected synthetic dotnet call: %s\n' "$*" >&2
    return 64
  }
  export -f dotnet

  if ! write_class_first_batch_evidence "$empty_batch_dir" "$fake_dll" "residual" "true"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_POSITIVE_EVIDENCE"
    exit 1
  fi
  if ! record_empty_residual_batch "$empty_batch_dir" "03" "residual" "Residual wildcard" "$fake_dll" "*" "any" "self-check" "true"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_POSITIVE_RECORD"
    exit 1
  fi
  if [[ "$(head -n 1 "$empty_batch_dir/state.txt" | tr -d '\r')" != "EMPTY_VERIFIED" || -e "$empty_batch_dir/result.trx" || -e "$empty_batch_dir/current-attempt.txt" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_FAKE_RESULT_LEFT"
    exit 1
  fi
  if ! empty_residual_evidence_current "$empty_batch_dir" "$fake_dll" "residual" "*"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_CURRENT_EVIDENCE"
    exit 1
  fi
  IFS=$'\t' read -r journal_state journal_total journal_passed journal_failed journal_not_executed < <(awk -F '\t' '$2 == "residual" { print $7 "\t" $9 "\t" $10 "\t" $11 "\t" $12 }' "$journal" | tail -n 1)
  if [[ "$journal_state" != "EMPTY_VERIFIED" || "$journal_total" != "0" || "$journal_passed" != "0" || "$journal_failed" != "0" || "$journal_not_executed" != "0" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_JOURNAL_COUNTS state=$journal_state total=$journal_total"
    exit 1
  fi

  write_summary "$temp_dir" "$journal" "$summary_file" "$batches_file"
  if ! grep -q 'Подтверждённо пустых остаточных групп: 1' "$summary_file"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_SUMMARY_EMPTY_COUNT"
    exit 1
  fi

  if write_class_first_batch_evidence "$named_empty_dir" "$fake_dll" "named-empty" "false" > "$temp_dir/named-empty.stdout.log" 2> "$temp_dir/named-empty.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_NAMED_EMPTY_ACCEPTED"
    exit 1
  fi

  printf 'tampered\n' >> "$empty_batch_dir/class-first-list-tests.stdout.log"
  if empty_residual_evidence_current "$empty_batch_dir" "$fake_dll" "residual" "*"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_TAMPERED_RAW_ACCEPTED"
    exit 1
  fi

  empty_self_check_mode="raw-empty"
  write_atomic "$fake_dll" printf 'synthetic empty residual dll v2\n'
  if write_class_first_batch_evidence "$empty_batch_dir" "$fake_dll" "residual" "true" > "$temp_dir/raw-empty.stdout.log" 2> "$temp_dir/raw-empty.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_RAW_EMPTY_ACCEPTED"
    exit 1
  fi
  if empty_residual_evidence_current "$empty_batch_dir" "$fake_dll" "residual" "*"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_STALE_AFTER_RAW_EMPTY"
    exit 1
  fi

  empty_self_check_mode="failure"
  write_atomic "$fake_dll" printf 'synthetic empty residual dll v3\n'
  if write_class_first_batch_evidence "$empty_batch_dir" "$fake_dll" "residual" "true" > "$temp_dir/raw-failure.stdout.log" 2> "$temp_dir/raw-failure.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_RAW_FAILURE_ACCEPTED"
    exit 1
  fi

  empty_self_check_mode="residual-method"
  write_atomic "$fake_dll" printf 'synthetic empty residual dll v4\n'
  if ! write_class_first_batch_evidence "$empty_batch_dir" "$fake_dll" "residual" "true"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_RESIDUAL_METHOD_EVIDENCE"
    exit 1
  fi
  selected_count="$(tsv_data_row_count "$empty_batch_dir/class-first-methods.tsv")"
  if [[ "$selected_count" != "1" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_NEW_METHOD_COUNT count=$selected_count"
    exit 1
  fi
  if empty_residual_evidence_current "$empty_batch_dir" "$fake_dll" "residual" "*"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_NEW_METHOD_RESUME_SKIPPED"
    exit 1
  fi
  if record_empty_residual_batch "$empty_batch_dir" "03" "residual" "Residual wildcard" "$fake_dll" "*" "any" "self-check" "true" > "$temp_dir/residual-method.stdout.log" 2> "$temp_dir/residual-method.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_RESIDUAL_METHOD_RECORDED_EMPTY"
    exit 1
  else
    failure_exit="$?"
  fi
  if [[ "$failure_exit" == "0" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_RESIDUAL_METHOD_EXIT"
    exit 1
  fi

  batches_file="$temp_dir/preserve-batches.tsv"
  old_batch_dir="$temp_dir/batch-04-preserve-residual"
  empty_self_check_mode="named-only"
  write_atomic "$fake_dll" printf 'synthetic empty residual dll v5\n'
  write_atomic "$batches_file" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	named-tests	Named tests	$fake_dll	FullyQualifiedName~Example.NamedTests.	1	true	any	self-check" \
    "04	preserve-residual	Preserve residual	$fake_dll	*	1	true	any	self-check"
  mkdir -p "$old_batch_dir/attempt-0001"
  write_atomic "$old_batch_dir/state.txt" printf 'COMPLETED_GREEN\n'
  write_atomic "$old_batch_dir/current-attempt.txt" printf 'attempt-0001\n'
  write_atomic "$old_batch_dir/result.trx" printf 'old root trx\n'
  write_atomic "$old_batch_dir/leaf-plan.tsv" printf 'leaf_id	batch_id	filter	method_count	estimated_rows\nleaf-001	preserve-residual	FullyQualifiedName=Example.OldResidual.Known	1	1\n'
  write_atomic "$old_batch_dir/leaf-methods.tsv" printf 'method_fqn	estimated_rows	class_fqn	batch_id	dll	unit_key	collection_name	collection_kind\nExample.OldResidual.Known	1	Example.OldResidual	preserve-residual	%s	class:Example.OldResidual		none\n' "$fake_dll"
  write_atomic "$old_batch_dir/leaf-journal.tsv" printf 'leaf_id	batch_id	started_utc	finished_utc	seconds	state	exit_code	total	passed	failed	not_executed	result_path	filter\nleaf-001	preserve-residual	2026-08-31T00:00:00Z	2026-08-31T00:00:01Z	1	COMPLETED_GREEN	0	1	1	0	0	leaf-001/result.trx	FullyQualifiedName=Example.OldResidual.Known\n'
  write_atomic "$old_batch_dir/class-first-list-tests.stdout.log" printf 'old raw listtests\n'
  write_atomic "$old_batch_dir/class-first-methods.tsv" printf 'old class first methods\n'
  write_atomic "$old_batch_dir/class-first-origin.tsv" printf 'old class first origin\n'
  write_atomic "$old_batch_dir/attempt-0001/state.txt" printf 'COMPLETED_GREEN\n'
  write_atomic "$old_batch_dir/attempt-0001/result.trx" printf 'old attempt trx\n'
  append_journal_row "04" "preserve-residual" "Preserve residual" "2026-08-31T00:00:00Z" "2026-08-31T00:00:01Z" "1" \
    "COMPLETED_GREEN" "0" "1" "1" "0" "0" "leaf-journal.tsv" "any" "self-check" "true"
  old_journal_hash="$(sha256_file "$journal")"
  old_leaf_hash="$(sha256_file "$old_batch_dir/leaf-journal.tsv")"
  old_plan_hash="$(sha256_file "$old_batch_dir/leaf-plan.tsv")"
  old_attempt_hash="$(sha256_file "$old_batch_dir/attempt-0001/result.trx")"
  old_class_first_hash="$(sha256_file "$old_batch_dir/class-first-list-tests.stdout.log")"
  if write_class_first_batch_evidence "$old_batch_dir" "$fake_dll" "preserve-residual" "true" > "$temp_dir/preserve-existing.stdout.log" 2> "$temp_dir/preserve-existing.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_PRIOR_NONEMPTY_ACCEPTED"
    exit 1
  else
    preserve_exit="$?"
  fi
  if [[ "$preserve_exit" != "68" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_PRIOR_NONEMPTY_EXIT exit=$preserve_exit"
    exit 1
  fi
  if ! grep -q 'новый --run-root' "$temp_dir/preserve-existing.stderr.log"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_PRIOR_NONEMPTY_NO_NEW_RUNROOT_HINT"
    exit 1
  fi
  if [[ "$(sha256_file "$journal")" != "$old_journal_hash" || "$(sha256_file "$old_batch_dir/leaf-journal.tsv")" != "$old_leaf_hash" ||
        "$(sha256_file "$old_batch_dir/leaf-plan.tsv")" != "$old_plan_hash" || "$(sha256_file "$old_batch_dir/attempt-0001/result.trx")" != "$old_attempt_hash" ||
        "$(sha256_file "$old_batch_dir/class-first-list-tests.stdout.log")" != "$old_class_first_hash" ]]; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_PRIOR_NONEMPTY_REWRITTEN"
    exit 1
  fi
  if [[ -e "$old_batch_dir/empty-residual.tsv" ]] || awk -F '\t' '$2 == "preserve-residual" && $7 == "EMPTY_VERIFIED" { found = 1 } END { exit found ? 0 : 1 }' "$journal"; then
    echo "EMPTY_RESIDUAL_SELF_CHECK_FAILED_PRIOR_NONEMPTY_JOURNALED_EMPTY"
    exit 1
  fi

  batches_file="$old_batches_file"
  write_atomic "$temp_dir/result.txt" printf 'EMPTY_RESIDUAL_POSITIVE_VERIFIED\nEMPTY_RESIDUAL_NAMED_EMPTY_REJECTED\nEMPTY_RESIDUAL_TAMPERED_RAW_REJECTED\nEMPTY_RESIDUAL_RAW_EMPTY_REJECTED\nEMPTY_RESIDUAL_RAW_FAILURE_REJECTED\nEMPTY_RESIDUAL_NEW_METHOD_FORCES_RUN\nEMPTY_RESIDUAL_PRIOR_NONEMPTY_REQUIRES_NEW_RUNROOT\nEMPTY_RESIDUAL_SUMMARY_COUNTED_NOT_PASSED\n'
  echo "EMPTY_RESIDUAL_POSITIVE_VERIFIED"
  echo "EMPTY_RESIDUAL_NAMED_EMPTY_REJECTED"
  echo "EMPTY_RESIDUAL_TAMPERED_RAW_REJECTED"
  echo "EMPTY_RESIDUAL_RAW_EMPTY_REJECTED"
  echo "EMPTY_RESIDUAL_RAW_FAILURE_REJECTED"
  echo "EMPTY_RESIDUAL_NEW_METHOD_FORCES_RUN"
  echo "EMPTY_RESIDUAL_PRIOR_NONEMPTY_REQUIRES_NEW_RUNROOT"
  echo "EMPTY_RESIDUAL_SUMMARY_COUNTED_NOT_PASSED"
}

assert_design_conversion_server_source_route_rejected() {
  local marker="$1"
  local case_root="$2"
  shift 2

  local case_map="$case_root/source-collections.tsv"
  local case_discovery="$case_root/leaf-discovery.tsv"
  local case_plan="$case_root/leaf-plan.tsv"
  local case_methods="$case_root/leaf-methods.tsv"
  local parser_stdout="$case_root/parser.stdout.log"
  local parser_stderr="$case_root/parser.stderr.log"
  local planner_stdout="$case_root/planner.stdout.log"
  local planner_stderr="$case_root/planner.stderr.log"
  local planner_exit

  mkdir -p "$case_root/Tests"
  write_atomic "$case_root/Tests/Counterexample.cs" printf '%s\n' "$@"
  if ! write_collection_source_map "$case_root" "$case_map" > "$parser_stdout" 2> "$parser_stderr"; then
    echo "${marker}_PARSER_FAILED"
    return 1
  fi

  {
    printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n'
    awk -F '\t' '$1 == "DesignConversionLiveLocalServerTests" {
      print "DesignConversionLiveLocalServerTests.Slow\t1\t" $1 "\t" $2 "\t" $3 "\t" $4
    }' "$case_map"
  } > "$case_discovery.tmp.$$"
  mv -f "$case_discovery.tmp.$$" "$case_discovery"
  if [[ "$(awk 'END { print NR + 0 }' "$case_discovery")" -lt 2 ]]; then
    echo "${marker}_TARGET_SOURCE_ROW_MISSING"
    return 1
  fi

  set +e
  write_leaf_plan_from_discovery "design-conversion-live-local-server-method-diagnostic" "$case_discovery" "$case_plan" "$case_methods" "method-diagnostic" "38" "60" > "$planner_stdout" 2> "$planner_stderr"
  planner_exit="$?"
  set -e
  if [[ "$planner_exit" == "0" ]]; then
    echo "${marker}_PLANNER_ACCEPTED"
    return 1
  fi
  if ! grep -Eq 'design_conversion_live_local_server_method_diagnostic_(shared_fixture_rejected|source_metadata_missing)' "$planner_stderr"; then
    echo "${marker}_REASON_MISSING"
    return 1
  fi

  echo "$marker"
}

assert_design_conversion_storage_method_pack_source_route_rejected() {
  local marker="$1"
  local case_root="$2"
  shift 2

  local case_map="$case_root/source-collections.tsv"
  local case_exact_map="$case_root/source-exact-bindings.tsv"
  local case_discovery="$case_root/leaf-discovery.tsv"
  local case_leaf_exact_map="$case_root/leaf-source-exact-bindings.tsv"
  local case_plan="$case_root/leaf-plan.tsv"
  local case_methods="$case_root/leaf-methods.tsv"
  local parser_stdout="$case_root/parser.stdout.log"
  local parser_stderr="$case_root/parser.stderr.log"
  local planner_stdout="$case_root/planner.stdout.log"
  local planner_stderr="$case_root/planner.stderr.log"
  local planner_exit

  mkdir -p "$case_root/Tests"
  write_atomic "$case_root/Tests/Counterexample.cs" printf '%s\n' "$@"
  if ! write_collection_source_map "$case_root" "$case_map" > "$parser_stdout" 2> "$parser_stderr"; then
    echo "${marker}_PARSER_FAILED"
    return 1
  fi
  cp "$case_exact_map" "$case_leaf_exact_map"

  {
    printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n'
    awk -F '\t' '$1 == "DesignConversionDpoStorageTests" {
      print "DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite\t1\t" $1 "\t" $2 "\t" $3 "\t" $4
    }' "$case_map"
  } > "$case_discovery.tmp.$$"
  mv -f "$case_discovery.tmp.$$" "$case_discovery"
  if [[ "$(awk 'END { print NR + 0 }' "$case_discovery")" -lt 2 ]]; then
    echo "${marker}_TARGET_SOURCE_ROW_MISSING"
    return 1
  fi

  set +e
  write_leaf_plan_from_discovery "design-conversion-dpo-storage" "$case_discovery" "$case_plan" "$case_methods" "class-method-pack" "38" "60" > "$planner_stdout" 2> "$planner_stderr"
  planner_exit="$?"
  set -e
  if [[ "$planner_exit" == "0" ]]; then
    echo "${marker}_PLANNER_ACCEPTED"
    return 1
  fi
  if ! grep -Eq 'design_conversion_dpo_storage_method_pack_(shared_fixture_rejected|source_metadata_missing|source_metadata_mismatch)' "$planner_stderr"; then
    echo "${marker}_REASON_MISSING"
    return 1
  fi

  echo "$marker"
}

run_leaf_planner_self_check() {
  local temp_dir="$run_root/self-check-leaf-planner"
  local discovery="$temp_dir/leaf-discovery.tsv"
  local plan="$temp_dir/leaf-plan.tsv"
  local methods="$temp_dir/leaf-methods.tsv"
  local report="$temp_dir/leaf-pack-report.tsv"
  local oversized="$temp_dir/leaf-oversized-units.tsv"
  local source_root="$temp_dir/source-map"
  local source_map="$temp_dir/source-collections.tsv"
  local source_exact_map="$temp_dir/source-exact-bindings.tsv"
  local leaf_source_exact_map="$temp_dir/leaf-source-exact-bindings.tsv"
  local theory_leaf_count
  local theory_rows
  local class_splits
  local unit_splits
  local oversized_count
  local shared_class_leaf_count
  local scheduling_only_unit_count
  local source_scheduling_units
  local source_shared_units
  local source_class_fixture_units
  local source_spaced_shared_units
  local source_partial_fixture_units
  local source_alias_global_kind
  local source_alias_namespaced_kind
  local source_noise_hits
  local source_storage_kind
  local source_storage_exact_declarations
  local source_storage_alias_kind
  local storage_fast_leaf_count
  local storage_theory_leaf_size
  local storage_theory_rows
  local storage_slow_leaf_count
  local storage_slow_leaf_size
  local storage_class_splits
  local storage_unit_splits
  local storage_largest_unit_key
  local storage_oversized_method_count

  mkdir -p "$temp_dir"
  mkdir -p "$source_root/Tests"
  write_atomic "$source_root/Tests/Collections.cs" printf '%s\n' \
    '[CollectionDefinition("SchedulingOnly", DisableParallelization = true)]' \
    'public sealed class SchedulingOnlyCollection { }' \
    '[Collection("SchedulingOnly")] public sealed class SchedulingA { }' \
    '[Collection("SchedulingOnly")] public sealed class SchedulingB { }' \
    '[CollectionDefinition("RealShared", DisableParallelization = true)]' \
    'public sealed class RealSharedCollection : ICollectionFixture<RealFixture> { }' \
    'public sealed class RealFixture { }' \
    '[Collection("RealShared")] public sealed class SharedA { }' \
    '[Collection("RealShared")] public sealed class SharedB { }' \
    '[CollectionDefinition("RealSharedSpaced", DisableParallelization = true)]' \
    'public sealed class RealSharedSpacedCollection' \
    '    : ICollectionFixture < RealFixture > { }' \
    '[Collection("RealSharedSpaced")] public sealed class SharedSpaced { }' \
    'public sealed class ClassFixtureResource { }' \
    'public sealed class ClassFixtureOwner' \
    '    : IClassFixture < ClassFixtureResource > { }' \
    'public sealed partial class PartialFixtureOwner' \
    '    : IClassFixture < ClassFixtureResource > { }' \
    'public sealed partial class PartialFixtureOwner { public void Helper() { } }' \
    'public sealed class AliasCollision { }' \
    'public sealed partial class DesignConversionDpoStorageTests { }' \
    'public sealed partial class DesignConversionDpoStorageTests { public void Helper() { } }' \
    'namespace Example;' \
    'public sealed class AliasCollision' \
    '    : IClassFixture < ClassFixtureResource > { }' \
    'namespace StorageAlias;' \
    'public sealed class DesignConversionDpoStorageTests' \
    '    : IClassFixture < ClassFixtureResource > { }' \
    '// [CollectionDefinition("CommentNoise", DisableParallelization = true)] public sealed class CommentNoise : ICollectionFixture<Fake> { }' \
    'public sealed class StringNoise { private const string Text = "[CollectionDefinition(\"StringNoise\", DisableParallelization = true)] public sealed class StringNoiseFixture : ICollectionFixture<Fake> { }"; }' \
    'public sealed class BodyNoise { public string Text() { return "public sealed class FakeBodyFixture : IClassFixture < Fake > { }"; } }'
  write_collection_source_map "$source_root" "$source_map"
  source_scheduling_units="$(awk -F '\t' '$4 == "scheduling_only" && $3 == "SchedulingOnly" { print $2 }' "$source_map" | sort -u | wc -l | tr -d ' ')"
  source_shared_units="$(awk -F '\t' '$4 == "shared_fixture" && $3 == "RealShared" { print $2 }' "$source_map" | sort -u | wc -l | tr -d ' ')"
  source_spaced_shared_units="$(awk -F '\t' '$4 == "shared_fixture" && $3 == "RealSharedSpaced" { print $2 }' "$source_map" | sort -u | wc -l | tr -d ' ')"
  source_class_fixture_units="$(awk -F '\t' '$1 == "ClassFixtureOwner" && $4 == "shared_fixture" { found = 1 } END { print found + 0 }' "$source_map")"
  source_partial_fixture_units="$(awk -F '\t' '$1 == "PartialFixtureOwner" && $4 == "shared_fixture" { found = 1 } END { print found + 0 }' "$source_map")"
  source_alias_global_kind="$(awk -F '\t' '$1 == "AliasCollision" { print $4 }' "$source_map")"
  source_alias_namespaced_kind="$(awk -F '\t' '$1 == "Example.AliasCollision" { print $4 }' "$source_map")"
  source_noise_hits="$(awk -F '\t' '$0 ~ /CommentNoise|StringNoiseFixture|FakeBodyFixture/ { count += 1 } END { print count + 0 }' "$source_map")"
  source_storage_kind="$(awk -F '\t' '$1 == "DesignConversionDpoStorageTests" { print $4 }' "$source_map")"
  source_storage_exact_declarations="$(awk -F '\t' '$1 == "DesignConversionDpoStorageTests" { print $6 }' "$source_exact_map")"
  source_storage_alias_kind="$(awk -F '\t' '$1 == "StorageAlias.DesignConversionDpoStorageTests" { print $5 }' "$source_exact_map")"
  if [[ "$source_scheduling_units" != "2" || "$source_shared_units" != "1" || "$source_spaced_shared_units" != "1" || "$source_class_fixture_units" != "1" || "$source_partial_fixture_units" != "1" || "$source_alias_global_kind" != "none" || "$source_alias_namespaced_kind" != "shared_fixture" || "$source_noise_hits" != "0" || "$source_storage_kind" != "none" || "$source_storage_exact_declarations" != "2" || "$source_storage_alias_kind" != "shared_fixture" ]]; then
    echo "COLLECTION_SOURCE_MAPPING_SELF_CHECK_FAILED scheduling_units=$source_scheduling_units shared_units=$source_shared_units spaced_shared_units=$source_spaced_shared_units class_fixture_units=$source_class_fixture_units partial_fixture_units=$source_partial_fixture_units alias_global=$source_alias_global_kind alias_namespaced=$source_alias_namespaced_kind noise_hits=$source_noise_hits storage_kind=$source_storage_kind storage_declarations=$source_storage_exact_declarations storage_alias=$source_storage_alias_kind"
    exit 1
  fi

  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.TheoryWithEighteenRows\t18\tExample.Tests\tclass:Example.Tests\t\tnone\nExample.ScheduleA.First\t2\tExample.ScheduleA\tclass:Example.ScheduleA\tSchedulingOnly\tscheduling_only\nExample.ScheduleB.Second\t2\tExample.ScheduleB\tclass:Example.ScheduleB\tSchedulingOnly\tscheduling_only\nExample.SharedA.First\t1\tExample.SharedA\tcollection:RealShared\tRealShared\tshared_fixture\nExample.SharedB.Second\t1\tExample.SharedB\tcollection:RealShared\tRealShared\tshared_fixture\nExample.OtherTests.Small\t1\tExample.OtherTests\tclass:Example.OtherTests\t\tnone\n'
  write_leaf_plan_from_discovery "self-check" "$discovery" "$plan" "$methods" "pack" "2" "3"
  theory_leaf_count="$(awk -F '\t' '$1 != "method_fqn" && $1 == "Example.Tests.TheoryWithEighteenRows" { print $3 }' "$methods" | sort -u | wc -l | tr -d ' ')"
  theory_rows="$(awk -F '\t' '$1 != "method_fqn" && $1 == "Example.Tests.TheoryWithEighteenRows" { print $5 }' "$methods")"
  shared_class_leaf_count="$(awk -F '\t' '$1 != "method_fqn" && $8 == "RealShared" { print $3 }' "$methods" | sort -u | wc -l | tr -d ' ')"
  scheduling_only_unit_count="$(awk -F '\t' '$1 != "method_fqn" && $8 == "SchedulingOnly" { print $7 }' "$methods" | sort -u | wc -l | tr -d ' ')"
  class_splits="$(awk -F '\t' '$1 == "class_splits" { print $2 }' "$report")"
  unit_splits="$(awk -F '\t' '$1 == "unit_splits" { print $2 }' "$report")"
  oversized_count="$(awk -F '\t' 'NR > 1 { count += 1 } END { print count + 0 }' "$oversized")"

  if [[ "$theory_leaf_count" != "1" || "$theory_rows" != "18" ]]; then
    echo "LEAF_PLANNER_THEORY_GROUPING_SELF_CHECK_FAILED"
    exit 1
  fi
  if [[ "$shared_class_leaf_count" != "1" || "$scheduling_only_unit_count" != "2" || "$class_splits" != "0" || "$unit_splits" != "0" || "$oversized_count" != "1" ]]; then
    echo "LEAF_PLANNER_CLASS_UNIT_SELF_CHECK_FAILED shared_leaf_count=$shared_class_leaf_count scheduling_only_units=$scheduling_only_unit_count class_splits=$class_splits unit_splits=$unit_splits oversized=$oversized_count"
    exit 1
  fi

  cp "$source_exact_map" "$leaf_source_exact_map"
  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nDesignConversionDpoStorageTests.FastOne\t2\tDesignConversionDpoStorageTests\tclass:DesignConversionDpoStorageTests\t\tnone\nDesignConversionDpoStorageTests.TheoryWithSeventyFiveRows\t75\tDesignConversionDpoStorageTests\tclass:DesignConversionDpoStorageTests\t\tnone\nDesignConversionDpoStorageTests.FastTwo\t1\tDesignConversionDpoStorageTests\tclass:DesignConversionDpoStorageTests\t\tnone\nDesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite\t1\tDesignConversionDpoStorageTests\tclass:DesignConversionDpoStorageTests\t\tnone\nExample.OtherStorageTests.First\t2\tExample.OtherStorageTests\tclass:Example.OtherStorageTests\t\tnone\nExample.OtherStorageTests.Second\t2\tExample.OtherStorageTests\tclass:Example.OtherStorageTests\t\tnone\n'
  write_leaf_plan_from_discovery "design-conversion-dpo-storage" "$discovery" "$plan" "$methods" "class-method-pack" "3" "60"
  storage_fast_leaf_count="$(awk -F '\t' '$1 != "method_fqn" && $6 == "DesignConversionDpoStorageTests" && $1 ~ /Fast/ { print $3 }' "$methods" | sort -u | wc -l | tr -d ' ')"
  storage_theory_leaf_size="$(awk -F '\t' '$1 != "method_fqn" && $1 == "DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows" { leaf = $3 } $1 != "method_fqn" && $3 == leaf { count += 1 } END { print count + 0 }' "$methods")"
  storage_theory_rows="$(awk -F '\t' '$1 != "method_fqn" && $1 == "DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows" { print $5 }' "$methods")"
  storage_slow_leaf_count="$(awk -F '\t' '$1 != "method_fqn" && $1 == "DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite" { print $3 }' "$methods" | sort -u | wc -l | tr -d ' ')"
  storage_slow_leaf_size="$(awk -F '\t' '$1 != "method_fqn" && $1 == "DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite" { leaf = $3 } $1 != "method_fqn" && $3 == leaf { count += 1 } END { print count + 0 }' "$methods")"
  storage_class_splits="$(awk -F '\t' '$1 == "class_splits" { print $2 }' "$report")"
  storage_unit_splits="$(awk -F '\t' '$1 == "unit_splits" { print $2 }' "$report")"
  storage_largest_unit_key="$(awk -F '\t' '$1 == "largest_unit_key" { print $2 }' "$report")"
  storage_oversized_method_count="$(awk -F '\t' '$1 == "method:DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows" && $3 == "1" { count += 1 } END { print count + 0 }' "$oversized")"
  if [[ "$storage_fast_leaf_count" != "2" || "$storage_theory_leaf_size" != "1" || "$storage_theory_rows" != "75" || "$storage_slow_leaf_count" != "1" || "$storage_slow_leaf_size" != "1" || "$storage_class_splits" != "1" || "$storage_unit_splits" != "0" || "$storage_largest_unit_key" == "class:DesignConversionDpoStorageTests" || "$storage_oversized_method_count" != "1" ]]; then
    echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SELF_CHECK_FAILED fast_leaf_count=$storage_fast_leaf_count theory_leaf_size=$storage_theory_leaf_size theory_rows=$storage_theory_rows slow_leaf_count=$storage_slow_leaf_count slow_leaf_size=$storage_slow_leaf_size class_splits=$storage_class_splits unit_splits=$storage_unit_splits largest_unit=$storage_largest_unit_key oversized_theory=$storage_oversized_method_count"
    exit 1
  fi

  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nDesignConversionLiveLocalServerTests.Slow\t1\tDesignConversionLiveLocalServerTests\tclass:DesignConversionLiveLocalServerTests\t\tshared_fixture\n'
  if write_leaf_plan_from_discovery "design-conversion-live-local-server-method-diagnostic" "$discovery" "$plan" "$methods" "method-diagnostic" "38" "60" > "$temp_dir/shared-method-route.stdout.log" 2> "$temp_dir/shared-method-route.stderr.log"; then
    echo "DESIGN_CONVERSION_LIVE_LOCAL_SERVER_METHOD_ROUTE_SHARED_FIXTURE_SELF_CHECK_FAILED"
    exit 1
  fi
  if ! grep -q 'design_conversion_live_local_server_method_diagnostic_shared_fixture_rejected' "$temp_dir/shared-method-route.stderr.log"; then
    echo "DESIGN_CONVERSION_LIVE_LOCAL_SERVER_METHOD_ROUTE_SHARED_FIXTURE_REASON_MISSING"
    exit 1
  fi
  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nDesignConversionLiveLocalServerTests.Slow\t1\tDesignConversionLiveLocalServerTests\tcollection:RealShared\tRealShared\tmanifest_shared\n'
  if write_leaf_plan_from_discovery "design-conversion-live-local-server-method-diagnostic" "$discovery" "$plan" "$methods" "method-diagnostic" "38" "60" > "$temp_dir/manifest-shared-method-route.stdout.log" 2> "$temp_dir/manifest-shared-method-route.stderr.log"; then
    echo "DESIGN_CONVERSION_LIVE_LOCAL_SERVER_METHOD_ROUTE_MANIFEST_SHARED_SELF_CHECK_FAILED"
    exit 1
  fi
  if ! grep -q 'design_conversion_live_local_server_method_diagnostic_shared_fixture_rejected' "$temp_dir/manifest-shared-method-route.stderr.log"; then
    echo "DESIGN_CONVERSION_LIVE_LOCAL_SERVER_METHOD_ROUTE_MANIFEST_SHARED_REASON_MISSING"
    exit 1
  fi

  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_INHERITED_CLASS_FIXTURE" \
    "$temp_dir/inherited-class-fixture" \
    'using Xunit;' \
    'public sealed class SharedServerFixture { }' \
    'public abstract class LiveServerFixtureBase' \
    '    : IClassFixture<SharedServerFixture> { }' \
    'public sealed class DesignConversionLiveLocalServerTests' \
    '    : LiveServerFixtureBase { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_ALIAS_CLASS_FIXTURE" \
    "$temp_dir/alias-class-fixture" \
    'using LiveServerFixtureContract = Xunit.IClassFixture<SharedServerFixture>;' \
    'public sealed class SharedServerFixture { }' \
    'public sealed class DesignConversionLiveLocalServerTests' \
    '    : LiveServerFixtureContract { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_INHERITED_COLLECTION_FIXTURE" \
    "$temp_dir/inherited-collection-fixture" \
    'using Xunit;' \
    'public sealed class SharedServerFixture { }' \
    'public static class DesignConversionLocalHostTestCollectionNames { public const string CollectionName = "DesignConversionLocalHostTests"; }' \
    'public abstract class LiveServerCollectionBase' \
    '    : ICollectionFixture<SharedServerFixture> { }' \
    '[CollectionDefinition(DesignConversionLocalHostTestCollectionNames.CollectionName, DisableParallelization = true)]' \
    'public sealed class DesignConversionLocalHostTestCollection' \
    '    : LiveServerCollectionBase { }' \
    '[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]' \
    'public sealed class DesignConversionLiveLocalServerTests { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_ALIAS_COLLECTION_FIXTURE" \
    "$temp_dir/alias-collection-fixture" \
    'using LiveServerCollectionContract = Xunit.ICollectionFixture<SharedServerFixture>;' \
    'public sealed class SharedServerFixture { }' \
    '[CollectionDefinition("DesignConversionLocalHostTests", DisableParallelization = true)]' \
    'public sealed class DesignConversionLocalHostTestCollection' \
    '    : LiveServerCollectionContract { }' \
    '[Collection("DesignConversionLocalHostTests")]' \
    'public sealed class DesignConversionLiveLocalServerTests { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_PARTIAL_COLLECTION_FIXTURE" \
    "$temp_dir/partial-collection-fixture" \
    'using Xunit;' \
    'public sealed class SharedServerFixture { }' \
    'public abstract class LiveServerCollectionBase' \
    '    : ICollectionFixture<SharedServerFixture> { }' \
    '[CollectionDefinition("DesignConversionLocalHostTests", DisableParallelization = true)]' \
    'public sealed partial class DesignConversionLocalHostTestCollection { }' \
    'public sealed partial class DesignConversionLocalHostTestCollection' \
    '    : LiveServerCollectionBase { }' \
    '[Collection("DesignConversionLocalHostTests")]' \
    'public sealed class DesignConversionLiveLocalServerTests { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_MISSING_COLLECTION_DEFINITION" \
    "$temp_dir/missing-collection-definition" \
    '[Collection("DesignConversionLocalHostTests")]' \
    'public sealed class DesignConversionLiveLocalServerTests { }' || exit 1
  assert_design_conversion_server_source_route_rejected \
    "DESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_DIFFERENT_COLLECTION" \
    "$temp_dir/different-collection-definition" \
    '[CollectionDefinition("OtherLocalHostTests", DisableParallelization = true)]' \
    'public sealed class OtherLocalHostCollection { }' \
    '[Collection("OtherLocalHostTests")]' \
    'public sealed class DesignConversionLiveLocalServerTests { }' || exit 1

  assert_design_conversion_storage_method_pack_source_route_rejected \
    "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_INHERITED_CLASS_FIXTURE" \
    "$temp_dir/storage-inherited-class-fixture" \
    'using Xunit;' \
    'public sealed class SharedStorageFixture { }' \
    'public abstract class StorageFixtureBase' \
    '    : IClassFixture<SharedStorageFixture> { }' \
    'public sealed partial class DesignConversionDpoStorageTests' \
    '    : StorageFixtureBase { }' || exit 1
  assert_design_conversion_storage_method_pack_source_route_rejected \
    "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_XUNIT_COLLECTION_ATTRIBUTE" \
    "$temp_dir/storage-xunit-collection-attribute" \
    '[Xunit.Collection("SharedStorage")] public sealed partial class DesignConversionDpoStorageTests { }' || exit 1
  assert_design_conversion_storage_method_pack_source_route_rejected \
    "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_HIDDEN_PARTIAL_ATTRIBUTE" \
    "$temp_dir/storage-hidden-partial-attribute" \
    'public sealed partial class DesignConversionDpoStorageTests { }' \
    '[Xunit.Collection("SharedStorage")] public sealed partial class DesignConversionDpoStorageTests { }' || exit 1
  assert_design_conversion_storage_method_pack_source_route_rejected \
    "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_NAMESPACED_LOOKALIKE" \
    "$temp_dir/storage-namespaced-lookalike" \
    'namespace StorageLookalike;' \
    'public sealed class DesignConversionDpoStorageTests { }' || exit 1

  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nDesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite\t1\tDesignConversionDpoStorageTests\tclass:DesignConversionDpoStorageTests\t\tnone\n'
  write_atomic "$leaf_source_exact_map" printf 'class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\nDesignConversionDpoStorageTests\tTests/Collections.cs\tclass:DesignConversionDpoStorageTests\t\tnone\t0\texact_declaration\n'
  if write_leaf_plan_from_discovery "design-conversion-dpo-storage" "$discovery" "$plan" "$methods" "class-method-pack" "38" "60" > "$temp_dir/storage-empty-binding.stdout.log" 2> "$temp_dir/storage-empty-binding.stderr.log"; then
    echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_EMPTY_EXACT_BINDING_ACCEPTED"
    exit 1
  fi
  if ! grep -q 'design_conversion_dpo_storage_method_pack_source_metadata_mismatch' "$temp_dir/storage-empty-binding.stderr.log"; then
    echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_EMPTY_EXACT_BINDING_REASON_MISSING"
    exit 1
  fi
  write_atomic "$leaf_source_exact_map" printf 'class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\nDesignConversionDpoStorageTests\tTests/Collections.cs\tclass:DesignConversionDpoStorageTests\t\tnone\t2\texact_declaration\nDesignConversionDpoStorageTests\tTests/Other.cs\tclass:DesignConversionDpoStorageTests\t\tnone\t1\texact_declaration\n'
  if write_leaf_plan_from_discovery "design-conversion-dpo-storage" "$discovery" "$plan" "$methods" "class-method-pack" "38" "60" > "$temp_dir/storage-duplicate-binding.stdout.log" 2> "$temp_dir/storage-duplicate-binding.stderr.log"; then
    echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_DUPLICATE_EXACT_BINDING_ACCEPTED"
    exit 1
  fi
  if ! grep -q 'design_conversion_dpo_storage_method_pack_source_metadata_mismatch' "$temp_dir/storage-duplicate-binding.stderr.log"; then
    echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_DUPLICATE_EXACT_BINDING_REASON_MISSING"
    exit 1
  fi

  local saved_repo_root="$repo_root"
  local saved_batches_file="$batches_file"
  local saved_custom_batches_file="$custom_batches_file"
  local saved_focused_mode="$focused_mode"
  local override_guard_root="$temp_dir/class-method-pack-override-guard"
  mkdir -p "$override_guard_root/tools/test-batches"
  write_atomic "$override_guard_root/tools/test-batches/batches.tsv" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	test-infrastructure-evidence-measurement	Test infrastructure	Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll	FullyQualifiedName~TestSuiteBatchInventoryGuardTests.	1	true	any	self-check" \
    "02	design-conversion-dpo-storage	Storage	Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll	FullyQualifiedName~DesignConversionDpoStorage	1	true	any	self-check" \
    "03	other-storage	Other	Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll	FullyQualifiedName~Other.	1	true	any	self-check"
  write_atomic "$override_guard_root/tools/test-batches/leaf-mode-overrides.tsv" printf '%s\n' \
    "# batch_id	leaf_mode	human_reason" \
    "test-infrastructure-evidence-measurement	method-diagnostic	guard" \
    "design-conversion-dpo-storage	class-method-pack	guard" \
    "other-storage	class-method-pack	guard"
  repo_root="$override_guard_root"
  batches_file="$override_guard_root/tools/test-batches/batches.tsv"
  custom_batches_file="false"
  focused_mode="false"
  if load_leaf_mode_overrides > "$temp_dir/class-method-pack-override-guard.stdout.log" 2> "$temp_dir/class-method-pack-override-guard.stderr.log"; then
    echo "CLASS_METHOD_PACK_OVERRIDE_WRONG_BATCH_ACCEPTED"
    exit 1
  fi
  if ! grep -q 'class-method-pack only for design-conversion-dpo-storage' "$temp_dir/class-method-pack-override-guard.stderr.log"; then
    echo "CLASS_METHOD_PACK_OVERRIDE_WRONG_BATCH_REASON_MISSING"
    exit 1
  fi
  repo_root="$saved_repo_root"
  batches_file="$saved_batches_file"
  custom_batches_file="$saved_custom_batches_file"
  focused_mode="$saved_focused_mode"

  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nDesignConversionLiveLocalServerTests.Slow\t1\tDesignConversionLiveLocalServerTests\tclass:DesignConversionLiveLocalServerTests\t\tnone\n'
  if write_leaf_plan_from_discovery "design-conversion-live-local-server-method-diagnostic" "$discovery" "$plan" "$methods" "method-diagnostic" "38" "60" > "$temp_dir/missing-source-metadata.stdout.log" 2> "$temp_dir/missing-source-metadata.stderr.log"; then
    echo "DESIGN_CONVERSION_METHOD_ROUTE_MISSING_SOURCE_METADATA_ACCEPTED"
    exit 1
  fi
  if ! grep -q 'design_conversion_live_local_server_method_diagnostic_source_metadata_missing' "$temp_dir/missing-source-metadata.stderr.log"; then
    echo "DESIGN_CONVERSION_METHOD_ROUTE_MISSING_SOURCE_METADATA_REASON_MISSING"
    exit 1
  fi

  write_atomic "$temp_dir/result.txt" printf 'COLLECTION_SOURCE_MAPPING_OK\tscheduling_units=%s\tshared_units=%s\tspaced_shared_units=%s\tclass_fixture_units=%s\tpartial_fixture_units=%s\talias_global=%s\talias_namespaced=%s\tnoise_hits=%s\tstorage_kind=%s\tstorage_declarations=%s\tstorage_alias=%s\nTHEORY_METHOD_GROUPED_IN_ONE_LEAF\testimated_rows=%s\nSCHEDULING_ONLY_COLLECTION_KEEPS_CLASS_UNITS\tunits=%s\nREAL_FIXTURE_COLLECTION_STAYS_ONE_UNIT\tleaf_count=%s\nCLASS_COLLECTION_PACK_SPLITS_ZERO\tclass_splits=%s\tunit_splits=%s\toversized_units=%s\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_POSITIVE\tfast_leaf_count=%s\ttheory_rows=%s\tslow_leaf_size=%s\tlargest_unit=%s\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_INHERITED_CLASS_FIXTURE\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_XUNIT_COLLECTION_ATTRIBUTE\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_HIDDEN_PARTIAL_ATTRIBUTE\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_NAMESPACED_LOOKALIKE\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_REJECTS_EMPTY_EXACT_BINDING\nDESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_REJECTS_DUPLICATE_EXACT_BINDING\nCLASS_METHOD_PACK_OVERRIDE_WRONG_BATCH_REJECTED\nDESIGN_CONVERSION_METHOD_ROUTE_REJECTS_SHARED_FIXTURE\nDESIGN_CONVERSION_METHOD_ROUTE_REJECTS_MANIFEST_SHARED\nDESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_INHERITED_CLASS_FIXTURE\nDESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_ALIAS_CLASS_FIXTURE\nDESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_INHERITED_COLLECTION_FIXTURE\nDESIGN_CONVERSION_METHOD_ROUTE_SOURCE_REJECTS_ALIAS_COLLECTION_FIXTURE\nDESIGN_CONVERSION_METHOD_ROUTE_REJECTS_MISSING_SOURCE_METADATA\n' "$source_scheduling_units" "$source_shared_units" "$source_spaced_shared_units" "$source_class_fixture_units" "$source_partial_fixture_units" "$source_alias_global_kind" "$source_alias_namespaced_kind" "$source_noise_hits" "$source_storage_kind" "$source_storage_exact_declarations" "$source_storage_alias_kind" "$theory_rows" "$scheduling_only_unit_count" "$shared_class_leaf_count" "$class_splits" "$unit_splits" "$oversized_count" "$storage_fast_leaf_count" "$storage_theory_rows" "$storage_slow_leaf_size" "$storage_largest_unit_key"
  echo "COLLECTION_SOURCE_MAPPING_OK scheduling_units=$source_scheduling_units shared_units=$source_shared_units spaced_shared_units=$source_spaced_shared_units class_fixture_units=$source_class_fixture_units partial_fixture_units=$source_partial_fixture_units alias_global=$source_alias_global_kind alias_namespaced=$source_alias_namespaced_kind noise_hits=$source_noise_hits"
  echo "THEORY_METHOD_GROUPED_IN_ONE_LEAF estimated_rows=$theory_rows"
  echo "SCHEDULING_ONLY_COLLECTION_KEEPS_CLASS_UNITS units=$scheduling_only_unit_count"
  echo "REAL_FIXTURE_COLLECTION_STAYS_ONE_UNIT leaf_count=$shared_class_leaf_count"
  echo "CLASS_COLLECTION_PACK_SPLITS_ZERO class_splits=$class_splits unit_splits=$unit_splits oversized_units=$oversized_count"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_POSITIVE fast_leaf_count=$storage_fast_leaf_count theory_rows=$storage_theory_rows slow_leaf_size=$storage_slow_leaf_size largest_unit=$storage_largest_unit_key"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_INHERITED_CLASS_FIXTURE"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_XUNIT_COLLECTION_ATTRIBUTE"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_HIDDEN_PARTIAL_ATTRIBUTE"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_SOURCE_REJECTS_NAMESPACED_LOOKALIKE"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_REJECTS_EMPTY_EXACT_BINDING"
  echo "DESIGN_CONVERSION_DPO_STORAGE_METHOD_PACK_REJECTS_DUPLICATE_EXACT_BINDING"
  echo "CLASS_METHOD_PACK_OVERRIDE_WRONG_BATCH_REJECTED"
  echo "DESIGN_CONVERSION_METHOD_ROUTE_REJECTS_SHARED_FIXTURE"
  echo "DESIGN_CONVERSION_METHOD_ROUTE_REJECTS_MANIFEST_SHARED"
  echo "DESIGN_CONVERSION_METHOD_ROUTE_REJECTS_MISSING_SOURCE_METADATA"
}

run_leaf_resume_self_check() {
  local temp_dir="$run_root/self-check-leaf-resume"
  local closed="$temp_dir/leaf-closed"
  local running="$temp_dir/leaf-running"
  local summary_root="$run_root/self-check-latest-summary"
  local summary_file="$summary_root/summary.md"
  local stale_root="$run_root/self-check-stale-reason"

  mkdir -p "$closed/attempt-0001" "$running/attempt-0001"
  write_atomic "$closed/current-attempt.txt" printf 'attempt-0001\n'
  write_atomic "$closed/state.txt" printf 'COMPLETED_GREEN\n'
  write_atomic "$closed/attempt-0001/state.txt" printf 'COMPLETED_GREEN\n'
  write_atomic "$closed/result.trx" printf '<TestRun />\n'
  write_atomic "$closed/attempt-0001/result.trx" printf '<TestRun />\n'
  write_atomic "$running/current-attempt.txt" printf 'attempt-0001\n'
  write_atomic "$running/state.txt" printf 'STARTED\n'
  write_atomic "$running/attempt-0001/state.txt" printf 'STARTED\n'

  if ! is_leaf_resume_closed "$closed"; then
    echo "LEAF_RESUME_SELF_CHECK_FAILED_CLOSED_NOT_SKIPPED"
    exit 1
  fi
  if is_leaf_resume_closed "$running"; then
    echo "LEAF_RESUME_SELF_CHECK_FAILED_RUNNING_WAS_CLOSED"
    exit 1
  fi

  mkdir -p "$summary_root"
  write_atomic "$summary_root/batches.tsv" printf '# order\tbatch_id\thuman_name\tdll\tfilter\tmin_rows\tsplit_on_repeat_timeout\tplatform\tplatform_reason\n01\tself-check\tSelf check\tTests/bin/Release/net9.0/LlmFromScratch.Tests.dll\tFullyQualifiedName=Example.Tests.One\t1\tfalse\tany\tself-check\n'
  write_atomic "$summary_root/journal.tsv" printf 'order\tbatch_id\thuman_name\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tplatform\tplatform_reason\tsplit_on_repeat_timeout\n01\tself-check\tSelf check\t2026-08-27T00:00:00Z\t2026-08-27T00:00:01Z\t1\tLEAF_INCOMPLETE\tleaf-pack\t1\t1\t0\t0\tleaf-journal.tsv\tany\tself-check\tfalse\n01\tself-check\tSelf check\t2026-08-27T00:00:02Z\t2026-08-27T00:00:03Z\t1\tCOMPLETED_GREEN\t0\t2\t2\t0\t0\tresult.trx\tany\tself-check\tfalse\n'
  write_summary "$summary_root" "$summary_root/journal.tsv" "$summary_file" "$summary_root/batches.tsv"
  if ! grep -q -- '- Батчей по последней строке manifest: 1' "$summary_file" || \
     ! grep -q -- '- Сырых строк journal.tsv: 2' "$summary_file" || \
     ! grep -q -- '- Незакрытых или требующих отдельного статуса: 0' "$summary_file" || \
     ! grep -q -- '- Всего строк результата: 2' "$summary_file"; then
    echo "LATEST_BY_BATCH_SUMMARY_SELF_CHECK_FAILED"
    exit 1
  fi

  mkdir -p "$stale_root"
  write_atomic "$stale_root/incomplete-reason.txt" printf 'old_partial_reason\n'
  write_final_run_state "$stale_root" "$stale_root/state.txt" ""
  if [[ -e "$stale_root/incomplete-reason.txt" || "$(tr -d '\r\n' < "$stale_root/state.txt")" != "FINISHED" ]]; then
    echo "STALE_INCOMPLETE_REASON_SELF_CHECK_FAILED"
    exit 1
  fi

  write_atomic "$temp_dir/result.txt" printf 'CLOSED_SKIPPED_RUNNING_RERUN\n'
  echo "CLOSED_SKIPPED_RUNNING_RERUN"
  echo "LATEST_BY_BATCH_SUMMARY_SELF_CHECK_OK raw_journal_rows=2 projected_rows=1 total=2"
  echo "STALE_INCOMPLETE_REASON_REMOVED_ON_FINISHED"
}

run_leaf_parent_timing_self_check() {
  local temp_dir="$run_root/self-check-leaf-parent-timing"
  local plan="$temp_dir/leaf-plan.tsv"
  local journal="$temp_dir/leaf-journal.tsv"
  local status
  local reason
  local started_utc
  local finished_utc
  local seconds
  local invalid_timestamp_reason
  local invalid_seconds_reason
  local batch_dir
  local leaf_dir
  local attempt_dir

  mkdir -p "$temp_dir"
  write_atomic "$plan" printf 'leaf_id\tbatch_id\tleaf_order\tmethod_count\testimated_rows\tfilter\nleaf-001\tself-check\t001\t1\t1\tFullyQualifiedName=Example.Tests.One\nleaf-002\tself-check\t002\t1\t1\tFullyQualifiedName=Example.Tests.Two\n'
  write_atomic "$journal" printf 'leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter\nleaf-001\tself-check\t2026-08-27T00:00:00Z\t2026-08-27T00:00:02Z\t2\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\tresult.trx\t1\t1\tFullyQualifiedName=Example.Tests.One\nleaf-002\tself-check\t2026-08-27T00:00:03Z\t2026-08-27T00:00:07Z\t4\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\tresult.trx\t1\t1\tFullyQualifiedName=Example.Tests.Two\n'
  IFS=$'\t' read -r status reason started_utc finished_utc seconds < <(validate_leaf_parent_timing "$plan" "$journal")
  if [[ "$status" != "OK" || "$started_utc" != "2026-08-27T00:00:00Z" || "$finished_utc" != "2026-08-27T00:00:07Z" || "$seconds" != "6" ]]; then
    echo "LEAF_PARENT_TIMING_SELF_CHECK_FAILED valid_case status=$status reason=$reason started=$started_utc finished=$finished_utc seconds=$seconds"
    exit 1
  fi

  # Две отрицательные пробы защищают от регресса к lexical min/max и awk '$5+0':
  # плохой timestamp и плохие seconds должны закрывать timing как invalid.
  write_atomic "$journal" printf 'leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter\nleaf-001\tself-check\t2026-08-27 00:00:00Z\t2026-08-27T00:00:02Z\t2\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\tresult.trx\t1\t1\tFullyQualifiedName=Example.Tests.One\n'
  IFS=$'\t' read -r status invalid_timestamp_reason started_utc finished_utc seconds < <(validate_leaf_parent_timing "$plan" "$journal")
  if [[ "$status" != "INVALID" || "$invalid_timestamp_reason" != started_utc_bad_utc:* ]]; then
    echo "LEAF_PARENT_TIMING_SELF_CHECK_FAILED invalid_timestamp status=$status reason=$invalid_timestamp_reason"
    exit 1
  fi

  write_atomic "$journal" printf 'leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter\nleaf-001\tself-check\t2026-08-27T00:00:00Z\t2026-08-27T00:00:02Z\t2x\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\tresult.trx\t1\t1\tFullyQualifiedName=Example.Tests.One\n'
  IFS=$'\t' read -r status invalid_seconds_reason started_utc finished_utc seconds < <(validate_leaf_parent_timing "$plan" "$journal")
  if [[ "$status" != "INVALID" || "$invalid_seconds_reason" != seconds_not_nonnegative_integer:* ]]; then
    echo "LEAF_PARENT_TIMING_SELF_CHECK_FAILED invalid_seconds status=$status reason=$invalid_seconds_reason"
    exit 1
  fi

  batch_dir="$run_root/batch-98-timing-self-check"
  leaf_dir="$batch_dir/leaf-001"
  attempt_dir="$leaf_dir/attempt-0001"
  mkdir -p "$attempt_dir"
  write_atomic "$batch_dir/leaf-discovery.tsv" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.One\t1\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_atomic "$batch_dir/leaf-plan.tsv" printf 'leaf_id\tbatch_id\tleaf_order\tmethod_count\testimated_rows\tfilter\nleaf-001\ttiming-self-check\t001\t1\t1\tFullyQualifiedName=Example.Tests.One\n'
  write_atomic "$batch_dir/leaf-methods.tsv" printf 'method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.One\ttiming-self-check\tleaf-001\t00001\t1\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_leaf_fingerprint "$batch_dir" "timing-self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$batch_dir/leaf-discovery.tsv"
  write_atomic "$batch_dir/leaf-journal.tsv" printf 'leaf_id\tbatch_id\tstarted_utc\tfinished_utc\tseconds\tstate\texit_code\ttotal\tpassed\tfailed\tnot_executed\tresult_path\tmethod_count\testimated_rows\tfilter\nleaf-001\ttiming-self-check\t2026-08-27 00:00:00Z\t2026-08-27T00:00:02Z\t2\tCOMPLETED_GREEN\t0\t1\t1\t0\t0\tresult.trx\t1\t1\tFullyQualifiedName=Example.Tests.One\n'
  write_state "$leaf_dir/state.txt" "COMPLETED_GREEN"
  write_atomic "$leaf_dir/current-attempt.txt" printf 'attempt-0001\n'
  write_state "$attempt_dir/state.txt" "COMPLETED_GREEN"
  write_atomic "$leaf_dir/result.trx" printf '<TestRun />\n'
  write_atomic "$attempt_dir/result.trx" printf '<TestRun />\n'
  global_incomplete_reason=""
  run_leaf_batch "98" "timing-self-check" "Timing self-check" "$0" "FullyQualifiedName=Example.Tests.One" "1" "any" "self-check" "false"
  if [[ "$(tr -d '\r\n' < "$batch_dir/state.txt")" != "LEAF_INCOMPLETE" || "$global_incomplete_reason" != leaf_parent_timing_invalid:timing-self-check:* ]]; then
    echo "LEAF_PARENT_TIMING_SELF_CHECK_FAILED integration_state=$(tr -d '\r\n' < "$batch_dir/state.txt") reason=$global_incomplete_reason"
    exit 1
  fi

  write_atomic "$temp_dir/result.txt" printf 'LEAF_PARENT_TIMING_FAIL_CLOSED\tinvalid_timestamp=%s\tinvalid_seconds=%s\n' "$invalid_timestamp_reason" "$invalid_seconds_reason"
  echo "LEAF_PARENT_TIMING_FAIL_CLOSED invalid_timestamp=$invalid_timestamp_reason invalid_seconds=$invalid_seconds_reason"
}

run_invocation_budget_self_check() {
  local temp_dir="$run_root/self-check-invocation-budget/$(date -u +%Y%m%dT%H%M%SZ)-$$"
  local fake_bin="$temp_dir/bin"
  local fake_dll="$temp_dir/FakeBudget.Tests.dll"
  local batches="$temp_dir/batches.tsv"
  local skip_batches="$temp_dir/batches-with-platform-skip.tsv"
  local runner_path
  local result_file="$run_root/self-check-invocation-budget.txt"
  local old_path="$PATH"
  local limit_root="$temp_dir/limit-three"
  local boundary_root="$temp_dir/boundary-exact"
  local unlimited_root="$temp_dir/unlimited"
  local skip_root="$temp_dir/platform-skip"
  local mutant_runner="$temp_dir/mutant-local-reset.sh"
  local mutant_root="$temp_dir/mutant-limit-three"

  runner_path="$(path_identity "$0")"
  mkdir -p "$fake_bin"
  write_atomic "$fake_dll" printf 'synthetic budget dll\n'
  write_atomic "$batches" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	budget-a	Первая синтетическая группа	$fake_dll	FullyQualifiedName~Example.BudgetA	2	true	any	self-check" \
    "02	budget-b	Вторая синтетическая группа	$fake_dll	FullyQualifiedName~Example.BudgetB.	1	true	any	self-check" \
    "03	budget-c	Третья синтетическая группа	$fake_dll	FullyQualifiedName~Example.BudgetC	2	true	any	self-check"
  write_atomic "$skip_batches" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	platform-skip	Синтетический пропуск платформы	$fake_dll	FullyQualifiedName~Example.PlatformSkip.	1	true	windows	self-check" \
    "02	budget-a	Первая синтетическая группа	$fake_dll	FullyQualifiedName~Example.BudgetA	2	true	any	self-check" \
    "03	budget-b	Вторая синтетическая группа	$fake_dll	FullyQualifiedName~Example.BudgetB.	1	true	any	self-check"

  # Fake dotnet ниже не исполняет продуктовые тесты: он проверяет только
  # расписание leaf-порций, которое должно быть одинаковым для реального VSTest.
  cat > "$fake_bin/dotnet" <<'SH'
#!/usr/bin/env bash
set -Eeuo pipefail

if [[ "${1:-}" == "--version" ]]; then
  printf '9.0.0-invocation-budget-self-check\n'
  exit 0
fi

if [[ "${1:-}" == "--info" ]]; then
  printf 'Synthetic .NET info for invocation budget self-check\n'
  exit 0
fi

if [[ "${1:-}" == "--list-runtimes" ]]; then
  printf 'Microsoft.NETCore.App 9.0.0 [/synthetic]\n'
  exit 0
fi

if [[ "${1:-}" != "vstest" ]]; then
  printf 'unexpected dotnet command: %s\n' "$*" >&2
  exit 64
fi

settings=""
results=""
list_tests="false"
for arg in "$@"; do
  case "$arg" in
    /Settings:*) settings="${arg#/Settings:}" ;;
    /ResultsDirectory:*) results="${arg#/ResultsDirectory:}" ;;
    /ListTests) list_tests="true" ;;
  esac
done

if [[ -z "$settings" || ! -f "$settings" ]]; then
  printf 'missing runsettings for synthetic dotnet\n' >&2
  exit 64
fi

if [[ "$list_tests" == "true" ]]; then
  python3 - "$settings" <<'PY'
import sys
import xml.etree.ElementTree as ET

settings = sys.argv[1]
tree = ET.parse(settings)
filter_text = ""
for element in tree.getroot().iter():
    if element.tag.rsplit("}", 1)[-1] == "TestCaseFilter":
        filter_text = element.text or ""
        break

catalog = {
    "Example.BudgetA": ["Example.BudgetAOne.Run", "Example.BudgetATwo.Run"],
    "Example.BudgetB.": ["Example.BudgetB.One"],
    "Example.BudgetC": ["Example.BudgetCOne.Run", "Example.BudgetCTwo.Run"],
    "Example.PlatformSkip.": ["Example.PlatformSkip.One"],
}

print("The following Tests are available:")
if not filter_text or filter_text == "*":
    for methods in catalog.values():
        for method in methods:
            print("    " + method)
else:
    positive_terms = []
    negative_terms = []
    for token in filter_text.replace("(", "|").replace(")", "|").replace("&", "|").split("|"):
        token = token.strip()
        if token.startswith("FullyQualifiedName~"):
            positive_terms.append(token[len("FullyQualifiedName~"):])
        elif token.startswith("FullyQualifiedName!~"):
            negative_terms.append(token[len("FullyQualifiedName!~"):])
    for marker, methods in catalog.items():
        if any(marker.startswith(term) or term in marker or term in "".join(methods) for term in positive_terms) and not any(term in marker or any(term in method for method in methods) for term in negative_terms):
            for method in methods:
                print("    " + method)
PY
  exit 0
fi

if [[ -z "$results" ]]; then
  printf 'missing results directory for synthetic dotnet\n' >&2
  exit 64
fi
mkdir -p "$results"

python3 - "$settings" "$results/result.trx" <<'PY'
import re
import sys
import xml.etree.ElementTree as ET
import xml.sax.saxutils as sx

settings, trx_path = sys.argv[1:3]
tree = ET.parse(settings)
filter_text = ""
for element in tree.getroot().iter():
    if element.tag.rsplit("}", 1)[-1] == "TestCaseFilter":
        filter_text = element.text or ""
        break

methods = []
for token in re.split(r"[|()]", filter_text):
    token = token.strip()
    if token.startswith("FullyQualifiedName="):
        methods.append(token[len("FullyQualifiedName="):])

if not methods:
    raise SystemExit("synthetic leaf run expected exact FullyQualifiedName filters")

with open(trx_path, "w", encoding="utf-8", newline="\n") as out:
    out.write("<TestRun>\n")
    out.write("  <Results>\n")
    for index, method in enumerate(methods, start=1):
        out.write(f'    <UnitTestResult executionId="execution-{index}" testId="test-{index}" testName="{sx.escape(method)}" outcome="Passed" />\n')
    out.write("  </Results>\n")
    out.write("  <TestDefinitions>\n")
    for index, method in enumerate(methods, start=1):
        class_name, method_name = method.rsplit(".", 1)
        out.write(f'    <UnitTest name="{sx.escape(method)}" id="test-{index}">\n')
        out.write(f'      <Execution id="execution-{index}" />\n')
        out.write(f'      <TestMethod className="{sx.escape(class_name)}" name="{sx.escape(method_name)}" />\n')
        out.write("    </UnitTest>\n")
    out.write("  </TestDefinitions>\n")
    out.write('  <ResultSummary>\n')
    out.write(f'    <Counters total="{len(methods)}" executed="{len(methods)}" passed="{len(methods)}" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />\n')
    out.write("  </ResultSummary>\n")
    out.write("</TestRun>\n")
PY
SH
  chmod +x "$fake_bin/dotnet"

  budget_leaf_attempt_count() {
    local target_root="$1"

    find "$target_root" -type d -path '*/leaf-*/attempt-*' | wc -l | tr -d ' '
  }

  budget_batch_leaf_attempt_count() {
    local target_root="$1"
    local order="$2"
    local batch_id="$3"

    if [[ ! -d "$target_root/batch-$order-$batch_id" ]]; then
      printf '0\n'
      return 0
    fi
    find "$target_root/batch-$order-$batch_id" -type d -path '*/leaf-*/attempt-*' | wc -l | tr -d ' '
  }

  budget_state_text() {
    local state_file="$1"

    if [[ -f "$state_file" ]]; then
      tr -d '\r\n' < "$state_file"
    else
      printf 'missing'
    fi
  }

  run_budget_runner() {
    local script_path="$1"
    local case_root="$2"
    local case_batches="$3"
    local only="$4"
    local limit="$5"
    local label="$6"
    local stdout_file="$temp_dir/$label.stdout.log"
    local stderr_file="$temp_dir/$label.stderr.log"
    local command_file="$temp_dir/$label.command.txt"

    write_atomic "$command_file" printf 'PATH=%s:$PATH bash %s --repo-root %s --batches %s --only %s --run-root %s --no-build --leaf-mode pack --leaf-target-rows 1 --leaf-hard-cap-rows 1 --max-leaves %s\n' \
      "$fake_bin" "$script_path" "$repo_root" "$case_batches" "$only" "$case_root" "$limit"
    PATH="$fake_bin:$old_path" bash "$script_path" \
      --repo-root "$repo_root" \
      --batches "$case_batches" \
      --only "$only" \
      --run-root "$case_root" \
      --no-build \
      --leaf-mode pack \
      --leaf-target-rows 1 \
      --leaf-hard-cap-rows 1 \
      --max-leaves "$limit" \
      > "$stdout_file" 2> "$stderr_file"
  }

  budget_limit_three_is_enforced() {
    local target_root="$1"

    [[ "$(budget_state_text "$target_root/state.txt")" == "INCOMPLETE" ]] &&
      [[ "$(budget_state_text "$target_root/incomplete-reason.txt")" == "max_leaves:3" ]] &&
      [[ "$(budget_leaf_attempt_count "$target_root")" == "3" ]] &&
      [[ "$(budget_batch_leaf_attempt_count "$target_root" "01" "budget-a")" == "2" ]] &&
      [[ "$(budget_batch_leaf_attempt_count "$target_root" "02" "budget-b")" == "1" ]] &&
      [[ "$(budget_batch_leaf_attempt_count "$target_root" "03" "budget-c")" == "0" ]]
  }

  run_budget_runner "$runner_path" "$limit_root" "$batches" "budget-a,budget-b,budget-c" "3" "limit-first"
  if ! budget_limit_three_is_enforced "$limit_root"; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_LIMIT state=$(budget_state_text "$limit_root/state.txt") reason=$(budget_state_text "$limit_root/incomplete-reason.txt") attempts=$(budget_leaf_attempt_count "$limit_root")"
    exit 1
  fi

  run_budget_runner "$runner_path" "$limit_root" "$batches" "budget-a,budget-b,budget-c" "3" "limit-resume"
  if [[ "$(budget_state_text "$limit_root/state.txt")" != "FINISHED" ||
        "$(budget_leaf_attempt_count "$limit_root")" != "5" ||
        "$(budget_batch_leaf_attempt_count "$limit_root" "01" "budget-a")" != "2" ||
        "$(budget_batch_leaf_attempt_count "$limit_root" "02" "budget-b")" != "1" ||
        "$(budget_batch_leaf_attempt_count "$limit_root" "03" "budget-c")" != "2" ]]; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_RESUME state=$(budget_state_text "$limit_root/state.txt") attempts=$(budget_leaf_attempt_count "$limit_root")"
    exit 1
  fi

  run_budget_runner "$runner_path" "$boundary_root" "$batches" "budget-a,budget-b" "3" "boundary-exact"
  if [[ "$(budget_state_text "$boundary_root/state.txt")" != "FINISHED" || "$(budget_leaf_attempt_count "$boundary_root")" != "3" ]]; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_BOUNDARY state=$(budget_state_text "$boundary_root/state.txt") attempts=$(budget_leaf_attempt_count "$boundary_root")"
    exit 1
  fi

  run_budget_runner "$runner_path" "$unlimited_root" "$batches" "budget-a,budget-b,budget-c" "0" "unlimited"
  if [[ "$(budget_state_text "$unlimited_root/state.txt")" != "FINISHED" || "$(budget_leaf_attempt_count "$unlimited_root")" != "5" ]]; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_UNLIMITED state=$(budget_state_text "$unlimited_root/state.txt") attempts=$(budget_leaf_attempt_count "$unlimited_root")"
    exit 1
  fi

  run_budget_runner "$runner_path" "$skip_root" "$skip_batches" "platform-skip,budget-a,budget-b" "3" "platform-skip"
  if [[ "$(budget_state_text "$skip_root/state.txt")" != "FINISHED" ||
        "$(budget_leaf_attempt_count "$skip_root")" != "3" ||
        "$(budget_state_text "$skip_root/batch-01-platform-skip/state.txt")" != "SKIPPED_PLATFORM" ]]; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_SKIP state=$(budget_state_text "$skip_root/state.txt") skip=$(budget_state_text "$skip_root/batch-01-platform-skip/state.txt") attempts=$(budget_leaf_attempt_count "$skip_root")"
    exit 1
  fi

  cp "$runner_path" "$mutant_runner"
  python3 - "$mutant_runner" <<'PY'
import sys

path = sys.argv[1]
text = open(path, encoding="utf-8").read()
needle = '  max_leaf_count="$(safe_numeric_or_default "$max_leaves" "0")"\n  max_elapsed_seconds='
replacement = '  max_leaf_count="$(safe_numeric_or_default "$max_leaves" "0")"\n  invocation_started_leaf_attempts=0\n  max_elapsed_seconds='
if needle not in text:
    raise SystemExit("mutation anchor was not found")
open(path, "w", encoding="utf-8", newline="\n").write(text.replace(needle, replacement, 1))
PY
  chmod +x "$mutant_runner"
  run_budget_runner "$mutant_runner" "$mutant_root" "$batches" "budget-a,budget-b,budget-c" "3" "mutant-local-reset"
  if budget_limit_three_is_enforced "$mutant_root"; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_MUTANT_ACCEPTED"
    exit 1
  fi
  if [[ "$(budget_state_text "$mutant_root/state.txt")" != "FINISHED" || "$(budget_leaf_attempt_count "$mutant_root")" != "5" ]]; then
    echo "INVOCATION_BUDGET_SELF_CHECK_FAILED_MUTANT_SHAPE state=$(budget_state_text "$mutant_root/state.txt") attempts=$(budget_leaf_attempt_count "$mutant_root")"
    exit 1
  fi

  write_atomic "$result_file" printf 'INVOCATION_BUDGET_LIMIT_SPANS_GROUPS\nINVOCATION_BUDGET_RESUME_RUNS_ONLY_REMAINING\nINVOCATION_BUDGET_BOUNDARY_CAN_FINISH\nINVOCATION_BUDGET_ZERO_IS_UNLIMITED\nINVOCATION_BUDGET_PLATFORM_SKIP_NOT_COUNTED\nINVOCATION_BUDGET_LOCAL_RESET_MUTANT_REJECTED\n'
  echo "INVOCATION_BUDGET_LIMIT_SPANS_GROUPS"
  echo "INVOCATION_BUDGET_RESUME_RUNS_ONLY_REMAINING"
  echo "INVOCATION_BUDGET_BOUNDARY_CAN_FINISH"
  echo "INVOCATION_BUDGET_ZERO_IS_UNLIMITED"
  echo "INVOCATION_BUDGET_PLATFORM_SKIP_NOT_COUNTED"
  echo "INVOCATION_BUDGET_LOCAL_RESET_MUTANT_REJECTED"
}

write_synthetic_leaf_trx() {
  local target="$1"
  local class_name="$2"
  local method_name="$3"
  local outcome="$4"
  local row_count="${5:-1}"

  python3 - "$target.tmp.$$" "$class_name" "$method_name" "$outcome" "$row_count" <<'PY'
import sys
import xml.sax.saxutils as sx

path, class_name, method_name, outcome, row_count_raw = sys.argv[1:6]
row_count = int(row_count_raw)
if row_count < 1:
    raise SystemExit("row_count must be positive")
executed = 0 if outcome == "NotExecuted" else row_count
passed = row_count if outcome == "Passed" else 0
failed = row_count if outcome == "Failed" else 0
not_executed = row_count if outcome == "NotExecuted" else 0
with open(path, "w", encoding="utf-8", newline="\n") as out:
    out.write("<TestRun>\n")
    out.write("  <Results>\n")
    for index in range(1, row_count + 1):
        test_id = f"test-{index}"
        execution_id = f"execution-{index}"
        out.write(f'    <UnitTestResult executionId="{execution_id}" testId="{test_id}" testName="{sx.escape(class_name)}.{sx.escape(method_name)}" outcome="{outcome}" />\n')
    out.write("  </Results>\n")
    out.write("  <TestDefinitions>\n")
    for index in range(1, row_count + 1):
        test_id = f"test-{index}"
        execution_id = f"execution-{index}"
        out.write(f'    <UnitTest name="{sx.escape(class_name)}.{sx.escape(method_name)}" id="{test_id}">\n')
        out.write(f'      <Execution id="{execution_id}" />\n')
        out.write(f'      <TestMethod className="{sx.escape(class_name)}" name="{sx.escape(method_name)}" />\n')
        out.write("    </UnitTest>\n")
    out.write("  </TestDefinitions>\n")
    out.write('  <ResultSummary>\n')
    out.write(f'    <Counters total="{row_count}" executed="{executed}" passed="{passed}" failed="{failed}" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="{not_executed}" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />\n')
    out.write("  </ResultSummary>\n")
    out.write("</TestRun>\n")
PY
  flush_path "$target.tmp.$$"
  mv -f "$target.tmp.$$" "$target"
  flush_path "$target"
  flush_path "$(dirname "$target")"
}

run_hang_guard_self_check() {
  local temp_dir="$run_root/self-check-hang-guard/$(date -u +%Y%m%dT%H%M%SZ)-$$"
  local fake_bin="$temp_dir/bin"
  local fake_dll="$temp_dir/HangGuard.Tests.dll"
  local batches="$temp_dir/batches.tsv"
  local safe_root="$temp_dir/safe-run"
  local mutant_root="$temp_dir/mutant-run"
  local stale_leaf
  local stale_attempt
  local runner_path
  local mutant_runner="$temp_dir/mutant-timeout-as-green.sh"
  local result_file="$run_root/self-check-hang-guard.txt"
  local old_path="$PATH"
  local safe_stdout="$temp_dir/safe-runner.stdout.log"
  local safe_stderr="$temp_dir/safe-runner.stderr.log"
  local safe_command="$temp_dir/safe-runner.command.txt"
  local mutant_stdout="$temp_dir/mutant-runner.stdout.log"
  local mutant_stderr="$temp_dir/mutant-runner.stderr.log"
  local safe_exit
  local mutant_exit
  local leaf_dir
  local attempt_dir
  local leaf_state
  local batch_state
  local root_state
  local attempt_exit
  local journal_state
  local journal_result
  local journal_total
  local journal_not_executed
  local settings_file
  local elapsed_seconds
  local started_epoch
  local finished_epoch
  local safe_status
  local mutant_leaf_state
  local mutant_batch_state
  local mutant_root_state

  runner_path="$(path_identity "$0")"
  mkdir -p "$fake_bin"
  write_atomic "$fake_dll" printf 'synthetic hang guard dll\n'
  write_atomic "$batches" printf '%s\n' \
    "# order	batch_id	human_name	dll	filter	min_rows	split_on_repeat_timeout	platform	platform_reason" \
    "01	hang-guard	Самопроверка защиты от зависшего тестового процесса	$fake_dll	FullyQualifiedName=Example.HangGuard.Slow	1	true	any	self-check"

  # Фальшивый dotnet сначала честно отдаёт список тестов, а затем зависает в
  # режиме запуска. Так мы проверяем именно внешний предел времени runner'а,
  # не тратя минуты на реальные продуктовые тесты.
  cat > "$fake_bin/dotnet" <<'SH'
#!/usr/bin/env bash
set -Eeuo pipefail

if [[ "${1:-}" == "--version" ]]; then
  printf '9.0.0-hang-guard-self-check\n'
  exit 0
fi

if [[ "${1:-}" == "--info" ]]; then
  printf 'Synthetic .NET info for hang guard self-check\n'
  exit 0
fi

if [[ "${1:-}" == "--list-runtimes" ]]; then
  printf 'Microsoft.NETCore.App 9.0.0 [/synthetic]\n'
  exit 0
fi

if [[ "${1:-}" != "vstest" ]]; then
  printf 'unexpected dotnet command: %s\n' "$*" >&2
  exit 64
fi

settings=""
results=""
list_tests="false"
for arg in "$@"; do
  case "$arg" in
    /Settings:*) settings="${arg#/Settings:}" ;;
    /ResultsDirectory:*) results="${arg#/ResultsDirectory:}" ;;
    /ListTests) list_tests="true" ;;
  esac
done

if [[ -z "$settings" || ! -f "$settings" ]]; then
  printf 'missing runsettings for synthetic dotnet\n' >&2
  exit 64
fi

if [[ "$list_tests" == "true" ]]; then
  printf 'The following Tests are available:\n'
  printf '    Example.HangGuard.Slow\n'
  exit 0
fi

if [[ -z "$results" ]]; then
  printf 'missing results directory for synthetic dotnet\n' >&2
  exit 64
fi
mkdir -p "$results"
printf 'synthetic hang guard: process entered test body and waits for external timeout\n'
printf 'synthetic hang guard: partial TRX is written before the forced stop\n' >&2
printf '<TestRun><Results><UnitTestResult outcome="Passed" testName="Example.HangGuard.Slow" />\n' > "$results/result.trx"
# Здесь важно заменить shell на долгий процесс: тогда GNU timeout обрывает
# именно synthetic VSTest-host, а самопроверка не ждёт хвостовой child-процесс.
exec sleep 30
SH
  chmod +x "$fake_bin/dotnet"

  prepare_hang_guard_stale_leaf() {
    local target_root="$1"
    stale_leaf="$target_root/batch-01-hang-guard/leaf-001"
    stale_attempt="$stale_leaf/attempt-0001"
    mkdir -p "$stale_attempt"
    write_synthetic_leaf_trx "$stale_leaf/result.trx" "Example.HangGuard" "Slow" "Passed" "1"
    write_synthetic_leaf_trx "$stale_attempt/result.trx" "Example.HangGuard" "Slow" "Passed" "1"
    write_atomic "$stale_leaf/state.txt" printf 'STARTED\n'
    write_atomic "$stale_leaf/current-attempt.txt" printf 'attempt-0001\n'
    write_atomic "$stale_attempt/state.txt" printf 'STARTED\n'
  }

  run_hang_guard_case() {
    local script_path="$1"
    local target_root="$2"
    local label="$3"
    local stdout_file="$temp_dir/$label.stdout.log"
    local stderr_file="$temp_dir/$label.stderr.log"
    local command_file="$temp_dir/$label.command.txt"

    prepare_hang_guard_stale_leaf "$target_root"
    write_atomic "$command_file" printf 'PATH=%s:$PATH bash %s --repo-root %s --batches %s --only hang-guard --run-root %s --no-build --focused --leaf-mode pack --leaf-target-rows 1 --leaf-hard-cap-rows 1 --leaf-timeout 2s --leaf-session-timeout-ms 2000 --leaf-hang-timeout 2s\n' \
      "$fake_bin" "$script_path" "$repo_root" "$batches" "$target_root"
    PATH="$fake_bin:$old_path" bash "$script_path" \
      --repo-root "$repo_root" \
      --batches "$batches" \
      --only "hang-guard" \
      --run-root "$target_root" \
      --no-build \
      --focused \
      --leaf-mode pack \
      --leaf-target-rows 1 \
      --leaf-hard-cap-rows 1 \
      --leaf-timeout 2s \
      --leaf-session-timeout-ms 2000 \
      --leaf-hang-timeout 2s \
      > "$stdout_file" 2> "$stderr_file"
  }

  started_epoch="$(date -u +%s)"
  set +e
  run_hang_guard_case "$runner_path" "$safe_root" "safe-runner"
  safe_exit="$?"
  set -e
  finished_epoch="$(date -u +%s)"
  elapsed_seconds=$((finished_epoch - started_epoch))

  leaf_dir="$safe_root/batch-01-hang-guard/leaf-001"
  attempt_dir="$leaf_dir/attempt-0002"
  settings_file="$attempt_dir/blame-hang.runsettings"
  leaf_state="$(head -n 1 "$leaf_dir/state.txt" | tr -d '\r')"
  batch_state="$(head -n 1 "$safe_root/batch-01-hang-guard/state.txt" | tr -d '\r')"
  root_state="$(head -n 1 "$safe_root/state.txt" | tr -d '\r')"
  attempt_exit="$(head -n 1 "$attempt_dir/exit.txt" | tr -d '\r')"
  # В журнале result_path намеренно пустой для timeout. Bash `read` с IFS-табом
  # сжимает пустые поля, поэтому проверяем колонки через awk, а не через read.
  journal_state="$(awk -F '\t' 'END { print $6 }' "$safe_root/batch-01-hang-guard/leaf-journal.tsv")"
  attempt_exit_from_journal="$(awk -F '\t' 'END { print $7 }' "$safe_root/batch-01-hang-guard/leaf-journal.tsv")"
  journal_total="$(awk -F '\t' 'END { print $8 }' "$safe_root/batch-01-hang-guard/leaf-journal.tsv")"
  journal_not_executed="$(awk -F '\t' 'END { print $11 }' "$safe_root/batch-01-hang-guard/leaf-journal.tsv")"
  journal_result="$(awk -F '\t' 'END { print $12 }' "$safe_root/batch-01-hang-guard/leaf-journal.tsv")"

  if [[ "$safe_exit" != "0" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_RUNNER_EXIT exit=$safe_exit"
    exit 1
  fi
  if (( elapsed_seconds < 1 || elapsed_seconds > 30 )); then
    echo "HANG_GUARD_SELF_CHECK_FAILED_WALL_TIME seconds=$elapsed_seconds"
    exit 1
  fi
  if [[ "$leaf_state" != "LEAF_TIMED_OUT" || "$batch_state" != "COMPLETED_RED" || "$root_state" != "FINISHED" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_STATE leaf=$leaf_state batch=$batch_state root=$root_state"
    exit 1
  fi
  if [[ "$attempt_exit" != "124" || "$attempt_exit_from_journal" != "124" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_EXIT attempt=$attempt_exit journal=$attempt_exit_from_journal"
    exit 1
  fi
  if [[ "$journal_state" != "LEAF_TIMED_OUT" || -n "$journal_result" || "$journal_total" != "1" || "$journal_not_executed" != "1" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_JOURNAL state=$journal_state result=$journal_result total=$journal_total not_executed=$journal_not_executed"
    exit 1
  fi
  if [[ ! -s "$attempt_dir/stdout.log" || ! -s "$attempt_dir/stderr.log" || ! -s "$attempt_dir/result.trx" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_ATTEMPT_EVIDENCE"
    exit 1
  fi
  if ! grep -q 'CollectDumpOnTestSessionHang TestTimeout="2s" HangDumpType="None"' "$settings_file"; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_RUNSETTINGS_DUMP_POLICY"
    exit 1
  fi
  if ! grep -q '<TestSessionTimeout>2000</TestSessionTimeout>' "$settings_file"; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_RUNSETTINGS_SESSION_TIMEOUT"
    exit 1
  fi
  if ! grep -q 'synthetic hang guard: process entered test body' "$attempt_dir/stdout.log"; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_STDOUT_MARKER"
    exit 1
  fi
  if ! grep -q 'partial TRX is written before the forced stop' "$attempt_dir/stderr.log"; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_STDERR_MARKER"
    exit 1
  fi
  if [[ "$(head -n 1 "$leaf_dir/result.trx" | tr -d '\r')" != "<TestRun>" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_STALE_PROOF_MISSING"
    exit 1
  fi

  cp "$runner_path" "$mutant_runner"
  python3 - "$mutant_runner" <<'PY'
import sys

path = sys.argv[1]
text = open(path, encoding="utf-8").read()
needle = '        state="LEAF_TIMED_OUT"\n        result_for_journal=""\n        total="$(safe_numeric_or_default "$estimated_rows" "$total")"'
replacement = '        state="COMPLETED_GREEN"\n        result_for_journal="$leaf_dir/result.trx"\n        total="$(safe_numeric_or_default "$estimated_rows" "$total")"'
if needle not in text:
    raise SystemExit("hang guard mutation anchor was not found")
open(path, "w", encoding="utf-8", newline="\n").write(text.replace(needle, replacement, 1))
PY
  chmod +x "$mutant_runner"
  set +e
  run_hang_guard_case "$mutant_runner" "$mutant_root" "mutant-runner"
  mutant_exit="$?"
  set -e
  mutant_leaf_state="$(head -n 1 "$mutant_root/batch-01-hang-guard/leaf-001/state.txt" | tr -d '\r')"
  mutant_batch_state="$(head -n 1 "$mutant_root/batch-01-hang-guard/state.txt" | tr -d '\r')"
  mutant_root_state="$(head -n 1 "$mutant_root/state.txt" | tr -d '\r')"
  safe_status="leaf=$leaf_state batch=$batch_state root=$root_state exit=$attempt_exit seconds=$elapsed_seconds"
  if [[ "$mutant_exit" == "0" && "$mutant_leaf_state" == "LEAF_TIMED_OUT" && "$mutant_batch_state" == "COMPLETED_RED" ]]; then
    echo "HANG_GUARD_SELF_CHECK_FAILED_MUTANT_ACCEPTED"
    exit 1
  fi

  write_atomic "$result_file" printf 'HANG_GUARD_TIMEOUT_PROVED\t%s\nHANG_GUARD_STALE_TRX_NOT_GREEN\tjournal_result_empty=true\tattempt_partial_trx=true\nHANG_GUARD_RUNSETTINGS_PROVED\tTestSessionTimeout=2000\tCollectDumpOnTestSessionHang=None\nHANG_GUARD_MUTANT_REJECTED\tmutant_exit=%s\tmutant_leaf=%s\tmutant_batch=%s\tmutant_root=%s\nCOMMAND\t%s\nSTDOUT\t%s\nSTDERR\t%s\nRUNSETTINGS\t%s\n' \
    "$safe_status" "$mutant_exit" "$mutant_leaf_state" "$mutant_batch_state" "$mutant_root_state" \
    "$(cat "$safe_command")" "$attempt_dir/stdout.log" "$attempt_dir/stderr.log" "$settings_file"
  echo "HANG_GUARD_TIMEOUT_PROVED $safe_status"
  echo "HANG_GUARD_STALE_TRX_NOT_GREEN journal_result_empty=true attempt_partial_trx=true"
  echo "HANG_GUARD_RUNSETTINGS_PROVED TestSessionTimeout=2000 CollectDumpOnTestSessionHang=None"
  echo "HANG_GUARD_MUTANT_REJECTED mutant_exit=$mutant_exit mutant_leaf=$mutant_leaf_state mutant_batch=$mutant_batch_state mutant_root=$mutant_root_state"
}

run_leaf_trx_membership_self_check() {
  local temp_dir="$run_root/self-check-leaf-trx-membership"
  local method_plan="$temp_dir/leaf-methods.tsv"
  local missing_method_plan="$temp_dir/leaf-methods-missing.tsv"
  local extra_trx="$temp_dir/theory-extra-rows.trx"
  local trx="$temp_dir/foreign-single-row.trx"
  local missing_trx="$temp_dir/missing-row.trx"
  local details="$temp_dir/trx-membership.tsv"
  local status
  local reason
  local foreign_reason
  local missing_reason
  local total
  local passed
  local failed
  local not_executed

  mkdir -p "$temp_dir"
  write_atomic "$method_plan" printf 'method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.Expected\tself-check\tleaf-001\t00001\t1\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_synthetic_leaf_trx "$extra_trx" "Example.Tests" "Expected" "Passed" "3"
  IFS=$'\t' read -r status reason total passed failed not_executed < <(validate_leaf_trx_membership "$extra_trx" "$method_plan" "leaf-001" "1" "$details.extra")
  if [[ "$status" != "OK" || "$total" != "3" || "$passed" != "3" || "$failed" != "0" || "$not_executed" != "0" ]]; then
    echo "LEAF_TRX_MEMBERSHIP_SELF_CHECK_FAILED_THEORY_EXTRA status=$status reason=$reason total=$total passed=$passed failed=$failed not_executed=$not_executed"
    exit 1
  fi

  # Reviewer exploit: если TRX приносит чужой exact method, honest theory
  # expansion уже не применяется — такой leaf не имеет права стать completed.
  write_synthetic_leaf_trx "$trx" "Example.Tests" "Foreign" "Passed"
  IFS=$'\t' read -r status reason total passed failed not_executed < <(validate_leaf_trx_membership "$trx" "$method_plan" "leaf-001" "1" "$details.foreign")
  if [[ "$status" == "OK" ]]; then
    echo "LEAF_TRX_MEMBERSHIP_SELF_CHECK_FAILED_FOREIGN"
    exit 1
  fi
  foreign_reason="$reason"

  write_atomic "$missing_method_plan" printf 'method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.Expected\tself-check\tleaf-001\t00001\t2\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_synthetic_leaf_trx "$missing_trx" "Example.Tests" "Expected" "Passed" "1"
  IFS=$'\t' read -r status reason total passed failed not_executed < <(validate_leaf_trx_membership "$missing_trx" "$missing_method_plan" "leaf-001" "2" "$details.missing")
  if [[ "$status" == "OK" || "$reason" != method_row_count_mismatch:* ]]; then
    echo "LEAF_TRX_MEMBERSHIP_SELF_CHECK_FAILED_MISSING status=$status reason=$reason"
    exit 1
  fi
  missing_reason="$reason"

  write_atomic "$temp_dir/result.txt" printf 'LEAF_TRX_MEMBERSHIP_THEORY_EXTRA_ACCEPTED_AND_FOREIGN_MISSING_REJECTED\tforeign=%s\tmissing=%s\n' "$foreign_reason" "$missing_reason"
  echo "LEAF_TRX_MEMBERSHIP_THEORY_EXTRA_ACCEPTED total=3"
  echo "LEAF_TRX_MEMBERSHIP_FOREIGN_AND_MISSING_REJECTED"
}

write_min_rows_leaf_fixture() {
  local order="$1"
  local batch_id="$2"
  shift 2
  local batch_dir="$run_root/batch-$order-$batch_id"
  local discovery="$batch_dir/leaf-discovery.tsv"
  local plan="$batch_dir/leaf-plan.tsv"
  local methods="$batch_dir/leaf-methods.tsv"
  local temp
  local index=0
  local method
  local class_name
  local leaf_id

  mkdir -p "$batch_dir"

  temp="$discovery.tmp.$$"
  {
    printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n'
    for method in "$@"; do
      class_name="${method%.*}"
      printf '%s\t1\t%s\tclass:%s\t\tnone\n' "$method" "$class_name" "$class_name"
    done
  } > "$temp"
  flush_path "$temp"
  mv -f "$temp" "$discovery"

  temp="$plan.tmp.$$"
  {
    printf 'leaf_id\tbatch_id\tleaf_order\tmethod_count\testimated_rows\tfilter\n'
    for method in "$@"; do
      index=$((index + 1))
      leaf_id="$(printf 'leaf-%03d' "$index")"
      printf '%s\t%s\t%03d\t1\t1\tFullyQualifiedName=%s\n' "$leaf_id" "$batch_id" "$index" "$method"
    done
  } > "$temp"
  flush_path "$temp"
  mv -f "$temp" "$plan"

  temp="$methods.tmp.$$"
  index=0
  {
    printf 'method_fqn\tbatch_id\tleaf_id\tmethod_order\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\n'
    for method in "$@"; do
      index=$((index + 1))
      leaf_id="$(printf 'leaf-%03d' "$index")"
      class_name="${method%.*}"
      printf '%s\t%s\t%s\t%05d\t1\t%s\tclass:%s\t\tnone\n' "$method" "$batch_id" "$leaf_id" "$index" "$class_name" "$class_name"
    done
  } > "$temp"
  flush_path "$temp"
  mv -f "$temp" "$methods"

  flush_path "$discovery"
  flush_path "$plan"
  flush_path "$methods"
  write_leaf_fingerprint "$batch_dir" "$batch_id" "$0" "FullyQualifiedName~Example.Tests." "$discovery"
}

last_batch_journal_field() {
  local batch_id="$1"
  local field="$2"

  awk -F '\t' -v batch="$batch_id" -v field="$field" '
    NR == 1 {
    for (columnIndex = 1; columnIndex <= NF; columnIndex += 1) {
        if ($columnIndex == field) { field_index = columnIndex }
      }
      next
    }
    $2 == batch && field_index > 0 { value = $field_index }
    END { print value }
  ' "$journal"
}

run_leaf_min_rows_self_check() {
  local old_max_leaves="$max_leaves"
  local old_max_elapsed_minutes="$max_elapsed_minutes"
  local old_global_incomplete_reason="$global_incomplete_reason"
  local off_batch
  local off_attempt
  local state
  local total
  local passed
  local not_executed
  local partial_batch
  local partial_attempts
  local parse_status
  local parse_reason

  run_vstest() {
    local _dll="$1"
    local filter="$2"
    local attempt_dir="$3"
    local method="${filter#FullyQualifiedName=}"
    local class_name="${method%.*}"
    local method_name="${method##*.}"
    local outcome="Passed"
    local row_count="1"

    mkdir -p "$attempt_dir"
    case "$method_name" in
      *Fail*) outcome="Failed" ;;
      *Skip*) outcome="NotExecuted" ;;
      *ThreeRows*) row_count="3" ;;
    esac

    write_synthetic_leaf_trx "$attempt_dir/result.trx" "$class_name" "$method_name" "$outcome" "$row_count"
    [[ "$outcome" == "Failed" ]] && return 1
    return 0
  }

  write_min_rows_leaf_fixture "91" "min-rows-exact" \
    "Example.Tests.ExactOne" \
    "Example.Tests.ExactTwo"
  global_incomplete_reason=""
  run_leaf_batch "91" "min-rows-exact" "Min rows exact self-check" "$0" "FullyQualifiedName~Example.Tests." "2" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-91-min-rows-exact/state.txt")"
  total="$(last_batch_journal_field "min-rows-exact" "total")"
  if [[ "$state" != "COMPLETED_GREEN" || "$total" != "2" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_EXACT state=$state total=$total"
    exit 1
  fi

  write_min_rows_leaf_fixture "92" "min-rows-below-green" \
    "Example.Tests.GreenOne" \
    "Example.Tests.GreenTwo"
  global_incomplete_reason=""
  run_leaf_batch "92" "min-rows-below-green" "Min rows below green self-check" "$0" "FullyQualifiedName~Example.Tests." "3" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-92-min-rows-below-green/state.txt")"
  if [[ "$state" != "INVALID_TOO_FEW_ROWS" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_BELOW_GREEN state=$state"
    exit 1
  fi

  write_min_rows_leaf_fixture "93" "min-rows-below-red" \
    "Example.Tests.RedOne" \
    "Example.Tests.RedFail"
  global_incomplete_reason=""
  run_leaf_batch "93" "min-rows-below-red" "Min rows below red self-check" "$0" "FullyQualifiedName~Example.Tests." "3" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-93-min-rows-below-red/state.txt")"
  if [[ "$state" != "INVALID_TOO_FEW_ROWS" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_BELOW_RED state=$state"
    exit 1
  fi

  write_min_rows_leaf_fixture "94" "min-rows-extra-theory" \
    "Example.Tests.ThreeRows"
  global_incomplete_reason=""
  run_leaf_batch "94" "min-rows-extra-theory" "Min rows extra rows self-check" "$0" "FullyQualifiedName~Example.Tests." "3" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-94-min-rows-extra-theory/state.txt")"
  total="$(last_batch_journal_field "min-rows-extra-theory" "total")"
  if [[ "$state" != "COMPLETED_GREEN" || "$total" != "3" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_EXTRA_THEORY state=$state total=$total"
    exit 1
  fi

  write_min_rows_leaf_fixture "95" "min-rows-honest-skip" \
    "Example.Tests.SkipOne" \
    "Example.Tests.PassOne"
  global_incomplete_reason=""
  run_leaf_batch "95" "min-rows-honest-skip" "Min rows honest skip self-check" "$0" "FullyQualifiedName~Example.Tests." "2" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-95-min-rows-honest-skip/state.txt")"
  total="$(last_batch_journal_field "min-rows-honest-skip" "total")"
  passed="$(last_batch_journal_field "min-rows-honest-skip" "passed")"
  not_executed="$(last_batch_journal_field "min-rows-honest-skip" "not_executed")"
  if [[ "$state" != "COMPLETED_GREEN" || "$total" != "2" || "$passed" != "1" || "$not_executed" != "1" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_HONEST_SKIP state=$state total=$total passed=$passed not_executed=$not_executed"
    exit 1
  fi

  write_min_rows_leaf_fixture "96" "min-rows-partial-resume" \
    "Example.Tests.ResumeOne" \
    "Example.Tests.ResumeTwo"
  # Эта часть моделирует отдельный bounded invocation: общий leaf-бюджет не
  # должен наследовать synthetic-запуски из предыдущих проверок этой функции.
  invocation_started_leaf_attempts=0
  max_leaves="1"
  global_incomplete_reason=""
  run_leaf_batch "96" "min-rows-partial-resume" "Min rows partial resume self-check" "$0" "FullyQualifiedName~Example.Tests." "2" "any" "self-check" "false"
  partial_batch="$run_root/batch-96-min-rows-partial-resume"
  partial_attempts="$(find "$partial_batch" -type d -path '*/leaf-*/attempt-*' | wc -l | tr -d ' ')"
  state="$(tr -d '\r\n' < "$run_root/batch-96-min-rows-partial-resume/state.txt")"
  IFS=$'\t' read -r parse_status parse_reason total passed _ not_executed < <(parse_trx_counts "$partial_batch/leaf-001/attempt-0001/result.trx")
  if [[ "$state" != "LEAF_INCOMPLETE" || "$global_incomplete_reason" != "max_leaves:1" || "$partial_attempts" != "1" || "$parse_status" != "OK" || "$total" != "1" || "$passed" != "1" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_PARTIAL state=$state reason=$global_incomplete_reason attempts=$partial_attempts parse=$parse_status total=$total passed=$passed"
    exit 1
  fi

  max_leaves="0"
  global_incomplete_reason=""
  run_leaf_batch "96" "min-rows-partial-resume" "Min rows partial resume self-check" "$0" "FullyQualifiedName~Example.Tests." "2" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-96-min-rows-partial-resume/state.txt")"
  partial_attempts="$(find "$partial_batch" -type d -path '*/leaf-*/attempt-*' | wc -l | tr -d ' ')"
  if [[ "$state" != "COMPLETED_GREEN" || "$partial_attempts" != "2" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_RESUME state=$state attempts=$partial_attempts"
    exit 1
  fi

  write_min_rows_leaf_fixture "98" "min-rows-inflated-journal" \
    "Example.Tests.InflatedOne" \
    "Example.Tests.InflatedTwo"
  global_incomplete_reason=""
  run_leaf_batch "98" "min-rows-inflated-journal" "Min rows inflated journal self-check" "$0" "FullyQualifiedName~Example.Tests." "2" "any" "self-check" "false"
  python3 - "$run_root/batch-98-min-rows-inflated-journal/leaf-journal.tsv" <<'PY'
import sys

path = sys.argv[1]
lines = open(path, encoding="utf-8").read().splitlines()
out = []
changed = False
for line in lines:
    parts = line.split("\t")
    if parts and parts[0] == "leaf-001":
        parts[7] = "2"
        parts[8] = "2"
        changed = True
    out.append("\t".join(parts))
if not changed:
    raise SystemExit("leaf-001 was not found")
open(path, "w", encoding="utf-8", newline="\n").write("\n".join(out) + "\n")
PY
  flush_path "$run_root/batch-98-min-rows-inflated-journal/leaf-journal.tsv"
  global_incomplete_reason=""
  run_leaf_batch "98" "min-rows-inflated-journal" "Min rows inflated journal self-check" "$0" "FullyQualifiedName~Example.Tests." "3" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-98-min-rows-inflated-journal/state.txt")"
  if [[ "$state" != "INVALID_TOO_FEW_ROWS" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_INFLATED_JOURNAL state=$state"
    exit 1
  fi

  off_batch="$run_root/batch-97-min-rows-off-old-green"
  off_attempt="$off_batch/attempt-0001"
  mkdir -p "$off_attempt"
  write_state "$off_batch/state.txt" "COMPLETED_GREEN"
  write_state "$off_attempt/state.txt" "COMPLETED_GREEN"
  write_atomic "$off_batch/current-attempt.txt" printf 'attempt-0001\n'
  write_synthetic_leaf_trx "$off_attempt/result.trx" "Example.Tests" "OffModeTwoRows" "Passed" "2"
  copy_result_trx "$off_attempt/result.trx" "$off_batch/result.trx"
  if ! is_resume_closed "$off_batch" "2"; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_OFF_OLD_GREEN_EXACT_NOT_SKIPPED"
    exit 1
  fi
  if is_resume_closed "$off_batch" "3"; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_OLD_GREEN_BELOW_FLOOR_SKIPPED"
    exit 1
  fi

  global_incomplete_reason=""
  run_leaf_batch "91" "min-rows-exact" "Min rows exact self-check" "$0" "FullyQualifiedName~Example.Tests." "3" "any" "self-check" "false"
  state="$(tr -d '\r\n' < "$run_root/batch-91-min-rows-exact/state.txt")"
  if [[ "$state" != "INVALID_TOO_FEW_ROWS" ]]; then
    echo "MIN_ROWS_SELF_CHECK_FAILED_OLD_GREEN_BELOW_FLOOR_RERUN state=$state"
    exit 1
  fi

  max_leaves="$old_max_leaves"
  max_elapsed_minutes="$old_max_elapsed_minutes"
  global_incomplete_reason="$old_global_incomplete_reason"
  write_atomic "$run_root/self-check-min-rows.txt" printf 'MIN_ROWS_EXACT_ACCEPTED\nMIN_ROWS_BELOW_GREEN_INVALID\nMIN_ROWS_BELOW_RED_INVALID\nMIN_ROWS_EXTRA_THEORY_ROWS_COUNTED\nMIN_ROWS_HONEST_SKIP_NOT_PASSED\nMIN_ROWS_PARTIAL_RESUME_PRESERVED\nMIN_ROWS_INFLATED_JOURNAL_REJECTED_BY_TRX_TOTAL\nMIN_ROWS_OLD_GREEN_BELOW_FLOOR_NOT_SKIPPED\nMIN_ROWS_OLD_GREEN_BELOW_FLOOR_RERUN_INVALID\n'
  echo "MIN_ROWS_EXACT_ACCEPTED"
  echo "MIN_ROWS_BELOW_GREEN_INVALID"
  echo "MIN_ROWS_BELOW_RED_INVALID"
  echo "MIN_ROWS_EXTRA_THEORY_ROWS_COUNTED"
  echo "MIN_ROWS_HONEST_SKIP_NOT_PASSED"
  echo "MIN_ROWS_PARTIAL_RESUME_PRESERVED"
  echo "MIN_ROWS_INFLATED_JOURNAL_REJECTED_BY_TRX_TOTAL"
  echo "MIN_ROWS_OLD_GREEN_BELOW_FLOOR_NOT_SKIPPED"
  echo "MIN_ROWS_OLD_GREEN_BELOW_FLOOR_RERUN_INVALID"
}

run_fingerprint_mismatch_self_check() {
  local temp_dir="$run_root/self-check-fingerprint-mismatch"
  local discovery="$temp_dir/leaf-discovery.tsv"
  local source_map="$temp_dir/leaf-source-collections.tsv"
  local source_exact_map="$temp_dir/leaf-source-exact-bindings.tsv"
  local old_leaf_timeout="$leaf_timeout"
  local old_leaf_session_timeout_ms="$leaf_session_timeout_ms"
  local old_leaf_hang_timeout="$leaf_hang_timeout"
  local old_leaf_mode="$leaf_mode"
  local old_max_leaves="$max_leaves"
  local old_max_elapsed_minutes="$max_elapsed_minutes"
  local timeout_rejected="false"
  local runsettings_rejected="false"
  local verifier_hash_rejected="false"
  local source_map_rejected="false"
  local source_exact_bindings_rejected="false"
  local mode_rejected="false"
  local override_hash_rejected="false"
  local budget_change_accepted="false"

  mkdir -p "$temp_dir"
  write_atomic "$discovery" printf 'method_fqn\testimated_rows\tclass_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests.One\t1\tExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_atomic "$source_map" printf 'class_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests\tclass:Example.Tests\t\tnone\n'
  write_atomic "$source_exact_map" printf 'class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\nExample.Tests\tTests/Example.cs\tclass:Example.Tests\t\tnone\t1\texact_declaration\n'
  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    echo "FINGERPRINT_SAME_SOURCE_MAP_SELF_CHECK_FAILED"
    exit 1
  fi

  max_leaves="2"
  max_elapsed_minutes="1"
  if validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    budget_change_accepted="true"
  fi
  max_leaves="$old_max_leaves"
  max_elapsed_minutes="$old_max_elapsed_minutes"

  write_atomic "$source_map" printf 'class_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests\tcollection:Mutated\tMutated\tmanifest_shared\n'
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    source_map_rejected="true"
  fi
  write_atomic "$source_map" printf 'class_fqn\tunit_key\tcollection_name\tcollection_kind\nExample.Tests\tclass:Example.Tests\t\tnone\n'

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  write_atomic "$source_exact_map" printf 'class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\nExample.Tests\tTests/Mutated.cs\tclass:Example.Tests\t\tnone\t1\texact_declaration\n'
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    source_exact_bindings_rejected="true"
  fi
  write_atomic "$source_exact_map" printf 'class_fqn\tsource_paths\tunit_key\tcollection_name\tcollection_kind\tdeclaration_count\tbinding_kind\nExample.Tests\tTests/Example.cs\tclass:Example.Tests\t\tnone\t1\texact_declaration\n'

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  if validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.Two"; then
    echo "FINGERPRINT_MISMATCH_SELF_CHECK_FAILED"
    exit 1
  fi

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  leaf_timeout="9m"
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    timeout_rejected="true"
  fi
  leaf_timeout="$old_leaf_timeout"

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  leaf_session_timeout_ms="999999"
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    runsettings_rejected="true"
  fi
  leaf_session_timeout_ms="$old_leaf_session_timeout_ms"
  leaf_hang_timeout="$old_leaf_hang_timeout"

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  leaf_mode="method-diagnostic"
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    mode_rejected="true"
  fi
  leaf_mode="$old_leaf_mode"

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  python3 - "$temp_dir/leaf-fingerprint.tsv" <<'PY'
import sys
path = sys.argv[1]
text = open(path, encoding="utf-8").read()
text = text.replace("leaf_mode_overrides_sha256\t", "leaf_mode_overrides_sha256\tmutated-", 1)
open(path, "w", encoding="utf-8", newline="\n").write(text)
PY
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    override_hash_rejected="true"
  fi

  write_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One" "$discovery"
  python3 - "$temp_dir/leaf-fingerprint.tsv" <<'PY'
import sys
path = sys.argv[1]
text = open(path, encoding="utf-8").read()
text = text.replace("verifier_sha256\t", "verifier_sha256\tmutated-", 1)
open(path, "w", encoding="utf-8", newline="\n").write(text)
PY
  if ! validate_leaf_fingerprint "$temp_dir" "self-check" "$0" "FullyQualifiedName=Example.Tests.One"; then
    verifier_hash_rejected="true"
  fi

  if [[ "$budget_change_accepted" != "true" || "$timeout_rejected" != "true" || "$runsettings_rejected" != "true" || "$verifier_hash_rejected" != "true" || "$source_map_rejected" != "true" || "$source_exact_bindings_rejected" != "true" || "$mode_rejected" != "true" || "$override_hash_rejected" != "true" ]]; then
    echo "FINGERPRINT_CONFIG_MUTATION_SELF_CHECK_FAILED budget=$budget_change_accepted timeout=$timeout_rejected runsettings=$runsettings_rejected verifier_hash=$verifier_hash_rejected source_map=$source_map_rejected source_exact_bindings=$source_exact_bindings_rejected mode=$mode_rejected override_hash=$override_hash_rejected"
    exit 1
  fi

  write_atomic "$temp_dir/result.txt" printf 'FINGERPRINT_SAME_SOURCE_MAP_RESUME_ACCEPTED\nFINGERPRINT_BUDGET_CHANGE_RESUME_ACCEPTED\nFINGERPRINT_MISMATCH_REJECTED\nFINGERPRINT_CONFIG_MUTATION_REJECTED\ttimeout,runsettings,verifier_hash,source_map,source_exact_bindings,mode,override_hash\n'
  echo "FINGERPRINT_SAME_SOURCE_MAP_RESUME_ACCEPTED"
  echo "FINGERPRINT_BUDGET_CHANGE_RESUME_ACCEPTED"
  echo "FINGERPRINT_SOURCE_MAP_MUTATION_REJECTED"
  echo "FINGERPRINT_SOURCE_EXACT_BINDINGS_MUTATION_REJECTED"
  echo "FINGERPRINT_MISMATCH_REJECTED"
  echo "FINGERPRINT_CONFIG_MUTATION_REJECTED timeout runsettings verifier_hash source_map source_exact_bindings mode override_hash"
}

append_journal_row() {
  local order="$1"
  local batch_id="$2"
  local human_name="$3"
  local started_utc="$4"
  local finished_utc="$5"
  local seconds="$6"
  local state="$7"
  local exit_code="$8"
  local total="$9"
  local passed="${10}"
  local failed="${11}"
  local not_executed="${12}"
  local result_path="${13}"
  local platform="${14}"
  local platform_reason="${15}"
  local split_on_repeat_timeout="${16}"
  local temp="$journal.tmp.$$"

  # Журнал тоже пишется через полную замену файла, а не прямой append: так
  # оборванная строка не станет похожа на завершенный батч.
  cp "$journal" "$temp"
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' \
    "$order" "$batch_id" "$human_name" "$started_utc" "$finished_utc" "$seconds" "$state" "$exit_code" \
    "$total" "$passed" "$failed" "$not_executed" "$result_path" "$platform" "$platform_reason" "$split_on_repeat_timeout" >> "$temp"
  flush_path "$temp"
  mv -f "$temp" "$journal"
  flush_path "$journal"
  flush_path "$(dirname "$journal")"
}

record_platform_skip() {
  local batch_dir="$1"
  local attempt_dir="$2"
  local order="$3"
  local batch_id="$4"
  local human_name="$5"
  local min_rows="$6"
  local platform="$7"
  local platform_reason="$8"
  local split_on_repeat_timeout="$9"
  local started_utc

  mkdir -p "$batch_dir" "$attempt_dir"
  started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  write_atomic "$batch_dir/current-attempt.txt" printf '%s\n' "${attempt_dir##*/}"
  write_atomic "$attempt_dir/started.txt" printf '%s\n' "$started_utc"
  write_atomic "$attempt_dir/platform-skip.txt" printf 'Батч не запускался в этой среде: требуется %s. Причина: %s\n' "$platform" "$platform_reason"
  write_state "$attempt_dir/state.txt" "SKIPPED_PLATFORM"
  write_state "$batch_dir/state.txt" "SKIPPED_PLATFORM"
  write_atomic "$attempt_dir/exit.txt" printf 'platform-skipped\n'
  write_atomic "$batch_dir/exit.txt" printf 'platform-skipped\n'
  append_journal_row "$order" "$batch_id" "$human_name" "$started_utc" "$started_utc" "0" "SKIPPED_PLATFORM" "platform" \
    "$min_rows" "0" "0" "$min_rows" "" "$platform" "$platform_reason" "$split_on_repeat_timeout"
}

write_summary() {
  local target_run_root="${1:-$run_root}"
  local target_journal="${2:-$journal}"
  local target_summary="${3:-$summary}"
  local target_batches_file="${4:-$batches_file}"
  local temp_summary="$target_summary.tmp.$$"
  local timed_out_leaf_count

  timed_out_leaf_count="$(
    find "$target_run_root" -type f -name 'leaf-journal.tsv' -print0 |
      xargs -0 -r awk -F '\t' 'FNR > 1 && $6 == "LEAF_TIMED_OUT" { count += 1 } END { printf "%d\n", count + 0 }'
  )"
  timed_out_leaf_count="${timed_out_leaf_count:-0}"

  {
    echo "# Сводка батчевого прогона"
    echo
    echo "Папка: \`$target_run_root\`"
    echo
    echo "Смысл: закрытый непустой батч обязан иметь непустой TRX из текущей попытки; платформенный пропуск и подтверждённо пустая остаточная группа перечисляются отдельно и не считаются зелёным прогоном."
    echo
    echo "Область проверки: \`$verification_scope\`."
    if [[ "$verification_scope" == "FOCUSED" ]]; then
      echo
      echo "Важно: focused-прогон доказывает только явно перечисленные exact FQN методы и не имеет права называться полным suite-green."
    fi
    if [[ "$selected_leaf_selector_active" == "true" ]]; then
      echo
      echo "Важно: выбран запуск только выбранных маленьких групп (\`$selected_leaf_ids_normalized\`). Остальные порции остаются открытыми и не считаются прошедшими."
    fi
    echo
    awk -F '\t' '
      FNR == NR {
        if ($0 ~ /^#/ || $0 ~ /^[[:space:]]*$/) { next }
        manifest_count += 1
        manifest_ids[manifest_count] = $2
        next
      }
      FNR == 1 { next }
      {
        raw_rows += 1
        batch_id = $2
        if (batch_id == "") { next }
        state[batch_id] = $7
        total[batch_id] = $9
        passed[batch_id] = $10
        failed[batch_id] = $11
        notrun[batch_id] = $12
      }
      END {
        for (manifest_index = 1; manifest_index <= manifest_count; manifest_index += 1) {
          batch_id = manifest_ids[manifest_index]
          if (!(batch_id in state)) { continue }
          batches += 1
          sum_total += total[batch_id]
          sum_passed += passed[batch_id]
          sum_failed += failed[batch_id]
          sum_notrun += notrun[batch_id]
          if (state[batch_id] == "EMPTY_VERIFIED") empty += 1
          if (state[batch_id] !~ /^COMPLETED_/ && state[batch_id] != "EMPTY_VERIFIED") open += 1
          if (state[batch_id] == "SKIPPED_PLATFORM") platform += 1
          if (state[batch_id] == "INVALID_RESULT" || state[batch_id] == "INVALID_TOO_FEW_ROWS" || state[batch_id] == "RETRY_REQUIRED_TIMEOUT" || state[batch_id] == "SPLIT_REQUIRED" || state[batch_id] ~ /^TIMED_OUT/) invalid += 1
        }
        # journal.tsv остается append-only event log. Сводка считает только
        # последнюю строку каждого manifest batch, а raw_rows показывает,
        # сколько событий реально накопилось при resume.
        printf "- Батчей по последней строке manifest: %d\n", batches
        printf "- Сырых строк journal.tsv: %d\n", raw_rows
        printf "- Незакрытых или требующих отдельного статуса: %d\n", open
        printf "- Платформенных пропусков: %d\n", platform
        printf "- Подтверждённо пустых остаточных групп: %d\n", empty
        printf "- Недействительных или оборванных батчей: %d\n", invalid
        printf "- Всего строк результата: %d\n", sum_total
        printf "- Прошло: %d\n", sum_passed
        printf "- Упало: %d\n", sum_failed
        printf "- Не исполнялось: %d\n", sum_notrun
      }
    ' "$target_batches_file" "$target_journal"
    echo "- Оборванных leaf-паков, учтённых как not_executed: $timed_out_leaf_count"
  } > "$temp_summary"
  flush_path "$temp_summary"
  mv -f "$temp_summary" "$target_summary"
  flush_path "$target_summary"
  flush_path "$(dirname "$target_summary")"
}

write_final_run_state() {
  local target_root="$1"
  local target_state="$2"
  local incomplete_reason="$3"
  local incomplete_reason_file="$target_root/incomplete-reason.txt"

  if [[ -n "$incomplete_reason" ]]; then
    write_atomic "$incomplete_reason_file" printf '%s\n' "$incomplete_reason"
    write_state "$target_state" "INCOMPLETE"
  else
    # Успешный resume не должен оставлять вчерашнюю причину незавершенности:
    # unlink атомарен, и после FINISHED читатель видит либо старое incomplete
    # состояние, либо новый корень без stale incomplete-reason.txt.
    rm -f "$incomplete_reason_file"
    flush_path "$target_root"
    write_state "$target_state" "FINISHED"
  fi
}

resolved_batches_file="$(path_identity "$batches_file")"
resolved_default_batches_file="$(path_identity "$default_batches_file")"
custom_batches_file="false"
if [[ "$resolved_batches_file" != "$resolved_default_batches_file" ]]; then
  custom_batches_file="true"
fi

if [[ "$focused_mode" == "true" ]]; then
  if [[ "$custom_batches_file" != "true" ]]; then
    echo "Focused mode requires a custom batches file; default batches.tsv stays canonical-only." >&2
    exit 64
  fi
  validate_focused_batches_file "$batches_file"
  verification_scope="FOCUSED"
elif [[ "$custom_batches_file" == "true" ]] && batches_file_contains_exact_method_filter "$batches_file"; then
  # Без явного --focused exact-method manifest отклоняется: иначе короткая
  # приемка могла бы выглядеть как обычный полный батчевый прогон.
  echo "Custom exact-method manifest requires --focused." >&2
  exit 64
fi

load_leaf_mode_overrides_exit=0
load_leaf_mode_overrides || load_leaf_mode_overrides_exit="$?"
if [[ "$load_leaf_mode_overrides_exit" != "0" ]]; then
  exit "$load_leaf_mode_overrides_exit"
fi

prepare_selected_leaf_selector_exit=0
prepare_selected_leaf_selector || prepare_selected_leaf_selector_exit="$?"
if [[ "$prepare_selected_leaf_selector_exit" != "0" ]]; then
  exit "$prepare_selected_leaf_selector_exit"
fi

ensure_journal
ensure_plan
write_scope_evidence
append_run_invocation_history

if [[ "$self_check_exact_filter" == "true" ]]; then
  run_exact_filter_self_check
  exit 0
fi

if [[ "$self_check_leaf_planner" == "true" ]]; then
  run_leaf_planner_self_check
  exit 0
fi

if [[ "$self_check_leaf_resume" == "true" ]]; then
  run_leaf_resume_self_check
  exit 0
fi

if [[ "$self_check_fingerprint_mismatch" == "true" ]]; then
  run_fingerprint_mismatch_self_check
  exit 0
fi

if [[ "$self_check_leaf_trx_membership" == "true" ]]; then
  run_leaf_trx_membership_self_check
  exit 0
fi

if [[ "$self_check_leaf_parent_timing" == "true" ]]; then
  run_leaf_parent_timing_self_check
  exit 0
fi

if [[ "$self_check_min_rows" == "true" ]]; then
  run_leaf_min_rows_self_check
  exit 0
fi

if [[ "$self_check_class_first_selection" == "true" ]]; then
  run_class_first_selection_self_check
  exit 0
fi

if [[ "$self_check_empty_residual" == "true" ]]; then
  run_empty_residual_self_check
  exit 0
fi

if [[ "$self_check_invocation_budget" == "true" ]]; then
  run_invocation_budget_self_check
  exit 0
fi

if [[ "$self_check_hang_guard" == "true" ]]; then
  run_hang_guard_self_check
  exit 0
fi

if [[ "$self_check_corrupt_trx" == "true" ]]; then
  run_corrupt_trx_self_check
  exit 0
fi

if [[ "$plan_only" == "true" ]]; then
  actual_platform="$(current_platform)"
else
  write_state "$run_state" "RUNNING"
  actual_platform="$(current_platform)"
fi

if [[ "$plan_only" != "true" && "$skip_build" != "true" && ! -f "$run_root/build.completed" ]]; then
  {
    echo "startUtc=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    dotnet build Tests/LlmFromScratch.Tests.csproj -c Release
    dotnet build Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj -c Release
    # Часть батчей запускает локальный сервер конверсии дизайна как готовую DLL.
    # Собираем инструмент здесь, чтобы красный батч отражал продуктовую логику,
    # а не отсутствие заранее подготовленного серверного исполняемого файла.
    dotnet build tools/DesignConversionLiveLocalServer/DesignConversionLiveLocalServer.csproj -c Release
    if [[ "$actual_platform" == "windows" ]]; then
      dotnet build Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj -c Release
    else
      echo "Tests.ApiDashboardWpf пропущен при сборке: WPF-батч честно учитывается как Windows-only."
    fi
    echo "finishUtc=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  } > "$run_root/build.log" 2> "$run_root/build.stderr.log"
  write_atomic "$run_root/build.completed" printf 'build-completed\n'
fi

while IFS=$'\t' read -r order batch_id human_name dll filter min_rows split_on_repeat_timeout platform platform_reason; do
  [[ -z "${order:-}" || "${order:0:1}" == "#" ]] && continue

  platform="${platform:-any}"
  platform_reason="${platform_reason:-}"
  split_on_repeat_timeout="${split_on_repeat_timeout:-false}"
  # Windows-операторы часто создают custom TSV через PowerShell, где строка
  # заканчивается CRLF. Bash оставляет завершающий CR в последнем поле; без этой
  # нормализации journal получает невидимый перенос внутри строки.
  platform_reason="${platform_reason%$'\r'}"

  safe_name="${order}-${batch_id}"
  batch_dir="$run_root/batch-$safe_name"
  class_first_empty_allowed="false"
  class_first_empty_ready="false"
  class_first_selected_method_count="-1"
  if contains_batch "$batch_id"; then
    selected="yes"
  else
    selected="no"
  fi

  if [[ "$focused_mode" == "true" ]]; then
    effective_filter="$filter"
  elif class_first_selection_enabled && [[ "$selected" == "yes" ]] && platform_can_run "$platform" "$actual_platform"; then
    if canonical_last_wildcard_for_dll "$batch_id" "$dll" "$filter"; then
      class_first_empty_allowed="true"
    fi

    if ! write_class_first_batch_evidence "$batch_dir" "$dll" "$batch_id" "$class_first_empty_allowed"; then
      write_state "$run_state" "FINGERPRINT_MISMATCH"
      echo "Class-first discovery failure: $batch_id" >&2
      echo "$run_root"
      exit 70
    fi
    class_first_selected_method_count="$(tsv_data_row_count "$batch_dir/class-first-methods.tsv")"
    if (( class_first_selected_method_count == 0 )); then
      class_first_empty_ready="true"
      effective_filter="EMPTY_VERIFIED"
    elif ! effective_filter="$(class_first_exact_filter_from_evidence "$batch_dir/class-first-methods.tsv")"; then
      write_state "$run_state" "FINGERPRINT_MISMATCH"
      echo "Class-first discovery failure: $batch_id" >&2
      echo "$run_root"
      exit 70
    fi
  else
    effective_filter="$(build_legacy_first_match_filter "$dll" "$filter")"
  fi

  append_plan_row "$order" "$batch_id" "$selected" "$dll" "$filter" "$effective_filter" "$platform" "$platform_reason"

  if [[ "$selected" == "no" ]]; then
    remember_batch_terms "$dll" "$filter"
    continue
  fi

  if [[ "$class_first_empty_ready" == "true" ]]; then
    if [[ "$(head -n 1 "$batch_dir/state.txt" 2>/dev/null | tr -d '\r' || true)" == "EMPTY_VERIFIED" ]] &&
      empty_residual_evidence_current "$batch_dir" "$dll" "$batch_id" "$filter"; then
      echo "Пропускаю подтверждённо пустую остаточную группу: $human_name ($batch_id)"
    else
      if ! record_empty_residual_batch "$batch_dir" "$order" "$batch_id" "$human_name" "$dll" "$filter" "$platform" "$platform_reason" "$split_on_repeat_timeout"; then
        write_state "$run_state" "FINGERPRINT_MISMATCH"
        echo "Empty residual verification failure: $batch_id" >&2
        echo "$run_root"
        exit 70
      fi
    fi
    if [[ "$plan_only" == "true" && -z "$global_incomplete_reason" ]]; then
      global_incomplete_reason="plan_only:$batch_id"
    fi
    remember_batch_terms "$dll" "$filter"
    continue
  fi

  if [[ "$plan_only" == "true" ]]; then
    # Плановый режим обязан построить leaf-инвентарь выбранного батча, но
    # останавливаться до attempt/TRX, чтобы проверка разбиения не запускала тесты.
    if platform_can_run "$platform" "$actual_platform" && [[ "$leaf_mode" != "off" ]]; then
      if ! run_leaf_batch "$order" "$batch_id" "$human_name" "$dll" "$effective_filter" "$min_rows" "$platform" "$platform_reason" "$split_on_repeat_timeout"; then
        write_state "$run_state" "FINGERPRINT_MISMATCH"
        echo "$run_root"
        exit 70
      fi
    fi
    remember_batch_terms "$dll" "$filter"
    continue
  fi

  if is_resume_closed "$batch_dir" "$min_rows"; then
    if class_first_selection_enabled && [[ "$leaf_mode" == "off" ]] && platform_can_run "$platform" "$actual_platform"; then
      attempt_dir="$(current_attempt_dir "$batch_dir" || true)"
      if [[ -n "$attempt_dir" ]] && batch_only_attempt_has_current_class_first_identity "$batch_dir" "$attempt_dir" "$dll" "$effective_filter" && batch_only_trx_matches_class_first "$batch_dir" "$attempt_dir/result.trx"; then
        echo "Пропускаю закрытый батч: $human_name ($batch_id)"
        remember_batch_terms "$dll" "$filter"
        continue
      fi
      echo "Повторяю закрытый batch-only батч: старый результат не доказывает текущий class-first список ($batch_id)"
    else
      echo "Пропускаю закрытый батч: $human_name ($batch_id)"
      remember_batch_terms "$dll" "$filter"
      continue
    fi
  fi

  attempt_dir="$(next_attempt_dir "$batch_dir")"

  if ! platform_can_run "$platform" "$actual_platform"; then
    record_platform_skip "$batch_dir" "$attempt_dir" "$order" "$batch_id" "$human_name" "$min_rows" "$platform" "$platform_reason" "$split_on_repeat_timeout"
    remember_batch_terms "$dll" "$filter"
    continue
  fi

  if [[ "$leaf_mode" != "off" ]]; then
    # Leaf-pack режим сохраняет прогресс внутри большого батча, но не включает
    # параллельность: leaf'ы идут строго последовательно и тем же effective filter.
    if ! run_leaf_batch "$order" "$batch_id" "$human_name" "$dll" "$effective_filter" "$min_rows" "$platform" "$platform_reason" "$split_on_repeat_timeout"; then
      write_state "$run_state" "FINGERPRINT_MISMATCH"
      echo "$run_root"
      exit 70
    fi
    remember_batch_terms "$dll" "$filter"
    if [[ -n "$global_incomplete_reason" ]]; then
      break
    fi
    continue
  fi

  mkdir -p "$batch_dir" "$attempt_dir"
  started_epoch="$(date -u +%s)"
  started_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  write_atomic "$batch_dir/current-attempt.txt" printf '%s\n' "${attempt_dir##*/}"
  write_atomic "$attempt_dir/started.txt" printf '%s\n' "$started_utc"
  if class_first_selection_enabled; then
    write_batch_only_class_first_attempt_identity "$batch_dir" "$attempt_dir" "$dll" "$effective_filter"
  fi
  write_state "$batch_dir/state.txt" "STARTED"
  write_state "$attempt_dir/state.txt" "STARTED"

  set +e
  run_vstest "$dll" "$effective_filter" "$attempt_dir"
  exit_code="$?"
  set -e

  write_atomic "$attempt_dir/exit.txt" printf '%s\n' "$exit_code"
  write_atomic "$batch_dir/exit.txt" printf '%s\n' "$exit_code"
  finished_epoch="$(date -u +%s)"
  finished_utc="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  seconds=$((finished_epoch - started_epoch))

  attempt_trx="$attempt_dir/result.trx"
  if [[ ! -s "$attempt_trx" ]]; then
    found_trx="$(find "$attempt_dir" -mindepth 2 -maxdepth 3 -name '*.trx' -size +0c | head -n 1 || true)"
    if [[ -n "$found_trx" ]]; then
      copy_result_trx "$found_trx" "$attempt_trx"
    fi
  fi

  result_for_journal="$batch_dir/result.trx"
  if [[ ! -s "$attempt_trx" ]]; then
    total=0; passed=0; failed=0; not_executed=0
    result_for_journal=""
    if [[ "$exit_code" == "124" ]]; then
      state="$(record_timeout_state "$batch_dir" "$attempt_dir" "$split_on_repeat_timeout" "$effective_filter")"
    else
      state="INVALID_RESULT"
      write_atomic "$attempt_dir/trx-validation.tsv" printf 'status\treason\ttotal\tpassed\tfailed\tnot_executed\nINVALID\tmissing_result_trx\t0\t0\t0\t0\n'
    fi
  else
    IFS=$'\t' read -r parse_status parse_reason total passed failed not_executed < <(parse_trx_counts "$attempt_trx")
    write_atomic "$attempt_dir/trx-validation.tsv" printf 'status\treason\ttotal\tpassed\tfailed\tnot_executed\n%s\t%s\t%s\t%s\t%s\t%s\n' "$parse_status" "$parse_reason" "$total" "$passed" "$failed" "$not_executed"
    if [[ "$exit_code" == "124" ]]; then
      state="$(record_timeout_state "$batch_dir" "$attempt_dir" "$split_on_repeat_timeout" "$effective_filter")"
      result_for_journal=""
    elif [[ "$parse_status" != "OK" ]]; then
      state="INVALID_RESULT"
      result_for_journal=""
    else
      # Promotion в корень выполняется только после успешного XML-parse именно
      # текущей попытки. Это закрывает класс ошибок "старый TRX сделал новый
      # запуск готовым".
      if class_first_selection_enabled && ! batch_only_trx_matches_class_first "$batch_dir" "$attempt_trx"; then
        state="INVALID_RESULT"
        result_for_journal=""
      elif [[ "$total" -lt "$min_rows" ]]; then
        state="INVALID_TOO_FEW_ROWS"
      elif [[ "$failed" -gt 0 || "$exit_code" -ne 0 ]]; then
        state="COMPLETED_RED"
      else
        state="COMPLETED_GREEN"
      fi
      if [[ "$state" == "COMPLETED_GREEN" || "$state" == "COMPLETED_RED" ]]; then
        copy_result_trx "$attempt_trx" "$batch_dir/result.trx"
      fi
    fi
  fi

  write_state "$attempt_dir/state.txt" "$state"
  write_state "$batch_dir/state.txt" "$state"
  append_journal_row "$order" "$batch_id" "$human_name" "$started_utc" "$finished_utc" "$seconds" "$state" "$exit_code" \
    "$total" "$passed" "$failed" "$not_executed" "$result_for_journal" "$platform" "$platform_reason" "$split_on_repeat_timeout"
  remember_batch_terms "$dll" "$filter"
done < "$batches_file"

if [[ "$plan_only" == "true" ]]; then
  if [[ -n "$global_incomplete_reason" ]]; then
    write_atomic "$run_root/incomplete-reason.txt" printf '%s\n' "$global_incomplete_reason"
    write_state "$run_state" "INCOMPLETE"
  fi
  echo "$plan_file"
  exit 0
fi

write_summary
if [[ -n "$global_incomplete_reason" ]]; then
  write_final_run_state "$run_root" "$run_state" "$global_incomplete_reason"
else
  write_final_run_state "$run_root" "$run_state" ""
fi
echo "$run_root"
