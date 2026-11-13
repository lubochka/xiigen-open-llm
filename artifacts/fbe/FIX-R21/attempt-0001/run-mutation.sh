#!/usr/bin/env bash
# Проверка чувствительности новых проверок мутацией продукта.
# Мутация: из метода Predict удаляется РОВНО ОДНА строка правила близости.
# Доказательство точности правки: та же строка удаляется из копии исходного файла
# ВНЕ хранилища, и отпечатки копии и мутировавшего файла сверяются.
# Восстановление доказывается совпадением с отпечатком из pre-work-fingerprints.md.
set -u

REPO="/c/Projects/llm_mvp_core"
EV="$REPO/artifacts/fbe/FIX-R21/attempt-0001"
RUN="$EV/runs/mutation"
SCRATCH="/c/Users/lubao/AppData/Local/Temp/claude/b44bbae46294784a/8793fc8d-1332-4588-8161-7473318bae55/scratchpad/fixr21"
PROD="12.SemanticCompiler/Prediction/MethodKnn.cs"
ORIGINAL_SHA="2bb6349423309ad3845e0e3131f120d7724f1e5e16f782c6968067744525a69a"

mkdir -p "$RUN" "$SCRATCH"
cd "$REPO" || exit 90
LOG="$RUN/mutation-journal.txt"
: > "$LOG"

echo "started_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$LOG"

# Шаг 0: отпечаток продуктового файла ДО мутации должен совпасть с записанным ранее.
SHA_BEFORE=$(sha256sum "$PROD" | cut -d' ' -f1)
echo "sha_before_mutation=$SHA_BEFORE" >> "$LOG"
echo "sha_recorded_in_pre_work_fingerprints=$ORIGINAL_SHA" >> "$LOG"
if [ "$SHA_BEFORE" != "$ORIGINAL_SHA" ]; then
  echo "ABORT: продуктовый файл отличается от записанного отпечатка" >> "$LOG"
  exit 91
fi
echo "sha_before_matches_fingerprint=yes" >> "$LOG"

# Шаг 1: копия исходного файла ВНЕ хранилища — она же эталон для сверки мутанта.
cp "$PROD" "$SCRATCH/MethodKnn.original.cs"
echo "external_copy_path=$SCRATCH/MethodKnn.original.cs" >> "$LOG"
echo "external_copy_sha=$(sha256sum "$SCRATCH/MethodKnn.original.cs" | cut -d' ' -f1)" >> "$LOG"

# Шаг 2: строка правила близости должна встречаться РОВНО ОДИН раз.
HITS=$(grep -c 'ThenBy(group => group.Min(item => item.Distance))' "$PROD")
echo "distance_rule_line_occurrences=$HITS" >> "$LOG"
if [ "$HITS" != "1" ]; then
  echo "ABORT: ожидалось ровно одно вхождение строки правила близости" >> "$LOG"
  exit 92
fi
LINE_NO=$(grep -n 'ThenBy(group => group.Min(item => item.Distance))' "$PROD" | cut -d: -f1)
echo "distance_rule_line_number=$LINE_NO" >> "$LOG"

# Шаг 3: мутация продуктового файла — удаление ровно этой одной строки.
grep -v 'ThenBy(group => group.Min(item => item.Distance))' "$SCRATCH/MethodKnn.original.cs" > "$PROD"
SHA_MUTANT=$(sha256sum "$PROD" | cut -d' ' -f1)
echo "sha_after_mutation=$SHA_MUTANT" >> "$LOG"

# Шаг 4: та же правка применяется к копии вне хранилища, независимо.
grep -v 'ThenBy(group => group.Min(item => item.Distance))' "$SCRATCH/MethodKnn.original.cs" > "$SCRATCH/MethodKnn.expected-mutant.cs"
SHA_EXPECTED=$(sha256sum "$SCRATCH/MethodKnn.expected-mutant.cs" | cut -d' ' -f1)
echo "sha_independently_built_expected_mutant=$SHA_EXPECTED" >> "$LOG"
if [ "$SHA_MUTANT" = "$SHA_EXPECTED" ]; then
  echo "mutation_was_exactly_the_declared_edit=yes" >> "$LOG"
else
  echo "mutation_was_exactly_the_declared_edit=NO" >> "$LOG"
fi

# Шаг 5: сборка продукта и тестового проекта с мутантом.
dotnet build 12.SemanticCompiler/SemanticCompiler.csproj -c Release > "$RUN/m1-build-product.log" 2>&1
echo "mutant_build_product_exit=$?" >> "$LOG"
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release > "$RUN/m2-build-tests.log" 2>&1
echo "mutant_build_tests_exit=$?" >> "$LOG"

# Шаг 6: прогон НОВЫХ проверок под мутацией — ожидается ровно один красный случай.
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName~MethodKnnTieReachedThroughFacadeTests" \
  --results-directory "$RUN" --logger "trx;LogFileName=mutant-new-tests.trx" \
  > "$RUN/m3-new-tests-under-mutation.log" 2>&1
echo "mutant_new_tests_exit=$?" >> "$LOG"

# Шаг 7: прогон уже принятой приёмки предсказателя под той же мутацией.
# Её ожидание записано не мной, а в её собственном заголовке: падали три первые.
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName~MethodKnnTieIsResolvedByDistanceTests" \
  --results-directory "$RUN" --logger "trx;LogFileName=mutant-existing-tests.trx" \
  > "$RUN/m4-existing-tests-under-mutation.log" 2>&1
echo "mutant_existing_tests_exit=$?" >> "$LOG"

# Шаг 8: восстановление продуктового файла из копии вне хранилища.
cp "$SCRATCH/MethodKnn.original.cs" "$PROD"
SHA_RESTORED=$(sha256sum "$PROD" | cut -d' ' -f1)
echo "sha_after_restore=$SHA_RESTORED" >> "$LOG"
if [ "$SHA_RESTORED" = "$ORIGINAL_SHA" ]; then
  echo "restore_proven_by_fingerprint_match=yes" >> "$LOG"
else
  echo "restore_proven_by_fingerprint_match=NO" >> "$LOG"
fi

echo "finished_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$LOG"
