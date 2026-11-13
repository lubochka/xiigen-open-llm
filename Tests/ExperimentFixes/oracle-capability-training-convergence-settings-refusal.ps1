#requires -Version 7.0
# Оракул для проверок отказа "настройки сходимости недопустимы" в проекте обучения
# способностей. Полоса сборки и тестов в этой сессии закрыта настройками сеанса,
# поэтому исполняемость логики доказывается здесь: условие переносится ДОСЛОВНО из
# живого продуктового файла (перенос сверяется с файлом в самом прогоне), после чего
# считается, какие строки проверок падают, если убрать каждую ветвь по отдельности.
#
# Что оракул НЕ доказывает: он не запускает xUnit, не компилирует C# и не проверяет
# обучающий проход. Он проверяет только логику самого условия отказа и чувствительность
# написанных строк проверок к удалению каждой ветви.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$productPath = Join-Path $repoRoot '18.CapabilityTraining/LlmIngredientLogicalUnits.cs'
$outPath = Join-Path $PSScriptRoot 'oracle-capability-training-convergence-settings-refusal-output.txt'

$out = [System.Collections.Generic.List[string]]::new()
function Say([string]$line) { $out.Add($line); Write-Host $line }

# --- разряды float без зависимости от новых методов .NET ------------------------
function BitsOf([float]$x) { [BitConverter]::SingleToInt32Bits($x) }
function IsNaNf([float]$x) {
  $b = BitsOf $x
  return ((($b -band 0x7F800000) -eq 0x7F800000) -and (($b -band 0x007FFFFF) -ne 0))
}
function IsFinitef([float]$x) {
  $b = BitsOf $x
  return ((($b -band 0x7F800000)) -ne 0x7F800000)
}
function IsNegativef([float]$x) { return ((BitsOf $x) -lt 0) }
# Семантика оператора C# "x < 0f": с NaN любое сравнение ложно; отрицательный нуль
# нулю равен и меньше нуля НЕ является. Собственное сравнение PowerShell здесь не
# используется намеренно: его Compare ставит NaN меньше всего, и это разошлось бы с C#.
function IsLessThanZerof([float]$x) {
  if (IsNaNf $x) { return $false }
  if (-not (IsNegativef $x)) { return $false }
  return ((BitsOf $x) -ne [int]::MinValue)
}

# --- дословный перенос условия --------------------------------------------------
# Три слагаемых; флаги нужны панели мутаций: $true = слагаемое на месте.
function GuardRefuses([float]$threshold, [int]$windowSteps, [bool]$t1, [bool]$t2, [bool]$t3) {
  $a = $t1 -and (-not (IsFinitef $threshold))
  $b = $t2 -and (IsLessThanZerof $threshold)
  $c = $t3 -and ($windowSteps -lt 1)
  return ($a -or $b -or $c)
}

$portedCondition = 'if (!float.IsFinite(request.Input.ConvergenceThreshold) || ' +
  'request.Input.ConvergenceThreshold < 0f || ' +
  'request.Input.ConvergenceWindowSteps < 1)'

Say '================================================================'
Say 'ОРАКУЛ: отказ "настройки сходимости недопустимы"'
Say '================================================================'
Say ''

# --- 0. Сверка переноса с живым файлом -----------------------------------------
Say '--- 0. Сверка переноса условия с живым продуктовым файлом ---'
$lines = [System.IO.File]::ReadAllLines($productPath)
$refusalLineNumbers = @()
for ($i = 0; $i -lt $lines.Length; $i++) {
  if ($lines[$i].Contains('"DPO_CONVERGENCE_SETTINGS_INVALID"')) { $refusalLineNumbers += ($i + 1) }
}
Say ("файл: " + (Resolve-Path -LiteralPath $productPath).Path)
Say ("мест отказа в файле: " + $refusalLineNumbers.Count + " (ожидалось 1); строки: " + ($refusalLineNumbers -join ', '))
if ($refusalLineNumbers.Count -ne 1) { throw 'Мест отказа не одно: перенос сверять нельзя.' }
$refusalIndex = $refusalLineNumbers[0] - 1
$start = $refusalIndex
while ($start -gt 0 -and -not $lines[$start].TrimStart().StartsWith('if (')) { $start-- }
$end = $start
while ($end -lt $refusalIndex -and -not $lines[$end].TrimEnd().EndsWith(')')) { $end++ }
Say ''
Say 'условие в живом файле, со своими номерами строк:'
for ($i = $start; $i -le $end; $i++) { Say ("  " + ($i + 1) + ": " + $lines[$i].Trim()) }
Say ''
$fileCondition = (($start..$end) | ForEach-Object { $lines[$_].Trim() }) -join ' '
Say ("перенос в оракуле: " + $portedCondition)
Say ("совпадает дословно: " + ($fileCondition -ceq $portedCondition))
if ($fileCondition -cne $portedCondition) { throw 'Перенос расходится с файлом.' }
Say ''
Say ('строки отказа в файле: ' + ($refusalIndex) + ': ' + $lines[$refusalIndex - 1].Trim())
Say ('                      ' + ($refusalIndex + 1) + ': ' + $lines[$refusalIndex].Trim())
Say ''

# --- 1. Самопроверка разрядных помощников --------------------------------------
# Ожидания объявлены ЗДЕСЬ, до прогона, чтобы несовпадение нельзя было подогнать.
Say '--- 1. Самопроверка семантики float (ожидания объявлены до прогона) ---'
$negativeZero = [BitConverter]::Int32BitsToSingle([int]::MinValue)
$selfChecks = @(
  @{ Name = 'NaN';            Value = [float]::NaN;              Finite = $false; LessThanZero = $false }
  @{ Name = 'плюс беск.';     Value = [float]::PositiveInfinity;  Finite = $false; LessThanZero = $false }
  @{ Name = 'минус беск.';    Value = [float]::NegativeInfinity;  Finite = $false; LessThanZero = $true  }
  @{ Name = '-0.001';         Value = [float]-0.001;              Finite = $true;  LessThanZero = $true  }
  @{ Name = 'отриц. нуль';    Value = $negativeZero;              Finite = $true;  LessThanZero = $false }
  @{ Name = 'нуль';           Value = [float]0;                   Finite = $true;  LessThanZero = $false }
  @{ Name = '0.002';          Value = [float]0.002;               Finite = $true;  LessThanZero = $false }
  @{ Name = 'MaxValue';       Value = [float]::MaxValue;          Finite = $true;  LessThanZero = $false }
)
Say 'значение      | разряды      | конечно ожид/изм | меньше нуля ожид/изм | ок'
$selfFailures = 0
foreach ($c in $selfChecks) {
  $v = [float]$c.Value
  $fin = IsFinitef $v
  $ltz = IsLessThanZerof $v
  $ok = (($fin -eq $c.Finite) -and ($ltz -eq $c.LessThanZero))
  if (-not $ok) { $selfFailures++ }
  Say ("{0,-13} | 0x{1,8:X8} | {2,-6}/{3,-6}    | {4,-6}/{5,-6}        | {6}" -f `
    $c.Name, (BitsOf $v), $c.Finite, $fin, $c.LessThanZero, $ltz, $(if ($ok) { 'да' } else { 'НЕТ' }))
}
Say ("расхождений самопроверки: " + $selfFailures + " (ожидалось 0)")
Say ''

# --- 2. Строки проверок и их ожидаемые вердикты ---------------------------------
Say '--- 2. Строки написанных проверок: вердикт условия ---'
Say 'Ожидание до прогона: все 11 строк получают ровно тот вердикт, который утверждает'
Say 'проверка, то есть расхождений 0.'
Say ''
$rows = @(
  @{ Row = 'отказ 1'; Label = 'порог не является числом';                 Threshold = [float]::NaN;             Window = 25; Refuse = $true }
  @{ Row = 'отказ 2'; Label = 'порог равен плюс бесконечности';           Threshold = [float]::PositiveInfinity; Window = 25; Refuse = $true }
  @{ Row = 'отказ 3'; Label = 'порог равен минус бесконечности';          Threshold = [float]::NegativeInfinity; Window = 25; Refuse = $true }
  @{ Row = 'отказ 4'; Label = 'порог конечный и отрицательный';           Threshold = [float]-0.001;             Window = 25; Refuse = $true }
  @{ Row = 'отказ 5'; Label = 'интервал наблюдения равен нулю';           Threshold = [float]0.002;              Window = 0;  Refuse = $true }
  @{ Row = 'отказ 6'; Label = 'интервал наблюдения отрицательный';        Threshold = [float]0.002;              Window = -1; Refuse = $true }
  @{ Row = 'допуск 1'; Label = 'нулевой порог, обычный интервал';         Threshold = [float]0;                  Window = 25; Refuse = $false }
  @{ Row = 'допуск 2'; Label = 'нулевой порог, наименьший интервал';      Threshold = [float]0;                  Window = 1;  Refuse = $false }
  @{ Row = 'допуск 3'; Label = 'обычные рабочие настройки';               Threshold = [float]0.002;              Window = 25; Refuse = $false }
  @{ Row = 'допуск 4'; Label = 'наибольший конечный порог';               Threshold = [float]::MaxValue;         Window = 1;  Refuse = $false }
  @{ Row = 'допуск 5'; Label = 'отрицательный нуль';                      Threshold = $negativeZero;             Window = 1;  Refuse = $false }
)
Say 'строка   | случай                              | ожид.   | изм.    | ок'
$baselineMismatch = 0
foreach ($r in $rows) {
  $actual = GuardRefuses ([float]$r.Threshold) ([int]$r.Window) $true $true $true
  $ok = ($actual -eq $r.Refuse)
  if (-not $ok) { $baselineMismatch++ }
  Say ("{0,-8} | {1,-35} | {2,-7} | {3,-7} | {4}" -f `
    $r.Row, $r.Label, $(if ($r.Refuse) { 'отказ' } else { 'допуск' }), `
    $(if ($actual) { 'отказ' } else { 'допуск' }), $(if ($ok) { 'да' } else { 'НЕТ' }))
}
Say ("расхождений с утверждениями проверок: " + $baselineMismatch + " (ожидалось 0)")
Say ''

# --- 3. Панель мутаций: чувствительность по каждой ветви ------------------------
Say '--- 3. Панель мутаций: что падает, если убрать ветвь ---'
Say 'Ожидания объявлены до прогона:'
Say '  М0 целое условие ................................... ломает 0 строк'
Say '  М1 убрано !float.IsFinite(порог) ................... ломает 2 строки (отказ 1, отказ 2)'
Say '  М2 убрано порог < 0f ............................... ломает 1 строку  (отказ 4)'
Say '  М3 убрано интервал < 1 ............................. ломает 2 строки (отказ 5, отказ 6)'
Say '  М4 убраны все три .................................. ломает 6 строк  (все отказы)'
Say '  Отдельное предсказание: ни одна мутация не ломает строк допуска,'
Say '  потому что удаление слагаемого может только расширить допуск.'
Say '  Отдельное предсказание: отказ 3 (минус бесконечность) НЕ ломается ни при М1,'
Say '  ни при М2 по отдельности, потому что его ловят сразу два слагаемых.'
Say ''
$mutations = @(
  @{ Name = 'М0 целое условие';           T1 = $true;  T2 = $true;  T3 = $true;  Expect = 0 }
  @{ Name = 'М1 без !IsFinite';           T1 = $false; T2 = $true;  T3 = $true;  Expect = 2 }
  @{ Name = 'М2 без порог < 0f';          T1 = $true;  T2 = $false; T3 = $true;  Expect = 1 }
  @{ Name = 'М3 без интервал < 1';        T1 = $true;  T2 = $true;  T3 = $false; Expect = 2 }
  @{ Name = 'М4 без всех трёх';           T1 = $false; T2 = $false; T3 = $false; Expect = 6 }
)
Say 'мутация                | сломано ожид/изм | сломаны строки | из них допуска'
$mutationMismatch = 0
foreach ($m in $mutations) {
  $broken = @()
  $brokenAdmit = 0
  foreach ($r in $rows) {
    $actual = GuardRefuses ([float]$r.Threshold) ([int]$r.Window) $m.T1 $m.T2 $m.T3
    if ($actual -ne $r.Refuse) {
      $broken += $r.Row
      if (-not $r.Refuse) { $brokenAdmit++ }
    }
  }
  $ok = ($broken.Count -eq $m.Expect)
  if (-not $ok) { $mutationMismatch++ }
  Say ("{0,-22} | {1,-7}/{2,-7}  | {3,-14} | {4}" -f `
    $m.Name, $m.Expect, $broken.Count, $(if ($broken.Count -eq 0) { '-' } else { $broken -join ', ' }), $brokenAdmit)
}
Say ("мутаций с неожидаемым числом поломок: " + $mutationMismatch + " (ожидалось 0)")
Say ''

# --- 4. Перепись всей допустимой области настроек -------------------------------
Say '--- 4. Перепись области настроек: вся граница, а не только строки проверок ---'
Say 'Ожидания до прогона для сетки 9 порогов x 6 интервалов = 54 сочетания:'
Say '  М0 отказов 39, допусков 15; М1 отказов 33; М2 отказов 36; М3 отказов 24; М4 отказов 0.'
Say ''
$thresholds = @(
  @{ Name = 'NaN';         Value = [float]::NaN }
  @{ Name = '+беск.';      Value = [float]::PositiveInfinity }
  @{ Name = '-беск.';      Value = [float]::NegativeInfinity }
  @{ Name = '-0.001';      Value = [float]-0.001 }
  @{ Name = '-0.0';        Value = $negativeZero }
  @{ Name = '0.0';         Value = [float]0 }
  @{ Name = '1E-07';       Value = [float]1E-07 }
  @{ Name = '0.002';       Value = [float]0.002 }
  @{ Name = 'MaxValue';    Value = [float]::MaxValue }
)
$windows = @(-2, -1, 0, 1, 2, 25)
Say ("порог    | " + (($windows | ForEach-Object { "{0,5}" -f $_ }) -join ' | '))
foreach ($t in $thresholds) {
  $cells = foreach ($w in $windows) {
    if (GuardRefuses ([float]$t.Value) ([int]$w) $true $true $true) { "{0,5}" -f 'отказ' } else { "{0,5}" -f 'доп.' }
  }
  Say (("{0,-8} | " -f $t.Name) + ($cells -join ' | '))
}
Say ''
Say 'мутация                | отказов ожид/изм | допусков'
$gridExpect = @{ 'М0 целое условие' = 39; 'М1 без !IsFinite' = 33; 'М2 без порог < 0f' = 36; 'М3 без интервал < 1' = 24; 'М4 без всех трёх' = 0 }
$gridMismatch = 0
foreach ($m in $mutations) {
  $refuse = 0
  foreach ($t in $thresholds) {
    foreach ($w in $windows) {
      if (GuardRefuses ([float]$t.Value) ([int]$w) $m.T1 $m.T2 $m.T3) { $refuse++ }
    }
  }
  $expected = $gridExpect[$m.Name]
  if ($refuse -ne $expected) { $gridMismatch++ }
  Say ("{0,-22} | {1,-7}/{2,-7}  | {3}" -f $m.Name, $expected, $refuse, (54 - $refuse))
}
Say ("сочетаний сетки: 54; мутаций с неожидаемым числом отказов: " + $gridMismatch + " (ожидалось 0)")
Say ''

# --- 5. Итог --------------------------------------------------------------------
Say '--- 5. Итог ---'
Say ("самопроверка float, расхождений: " + $selfFailures)
Say ("строки проверок против целого условия, расхождений: " + $baselineMismatch)
Say ("панель мутаций, мутаций с неожидаемым числом поломок: " + $mutationMismatch)
Say ("перепись сетки, мутаций с неожидаемым числом отказов: " + $gridMismatch)
$total = $selfFailures + $baselineMismatch + $mutationMismatch + $gridMismatch
Say ("всего расхождений: " + $total)
Say ''
Say 'Границы этого доказательства, прямо:'
Say '  доказано: каждая из трёх ветвей отказа покрыта строкой проверки, которая'
Say '            падает при удалении именно этой ветви; нулевой порог и отрицательный'
Say '            нуль остаются на принимаемой стороне проверки;'
Say '  НЕ доказано: что xUnit запущен, что проект собирается и что обучающий проход'
Say '            на принимаемой стороне действительно завершается успешно, потому что'
Say '            полоса сборки и тестов в этой сессии закрыта настройками сеанса.'

$text = ($out -join "`n") + "`n"
$enc = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($outPath, $text, $enc)
Write-Host ''
Write-Host ("вывод записан одиночным переводом строки: " + $outPath)
Write-Host ("код выхода оракула: " + $total)
exit $total
