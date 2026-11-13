$ErrorActionPreference='Stop'
$root='C:/Projects/llm_mvp_core'
$files=@('scripts/experiment-launcher/ExperimentLauncher.psm1','scripts/experiment-launcher/qa/Invoke-LauncherAcceptance.ps1','scripts/experiment-launcher/qa/DummyProgram.ps1','scripts/experiment-launcher/qa/Verify-DummyMaterial.ps1')
$rows=@();$bad=0
foreach($file in $files){$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $root $file),[ref]$tokens,[ref]$errors);$rows+=@{path=$file;errors=@($errors|ForEach-Object{@{message=$_.Message;line=$_.Extent.StartLineNumber}})};$bad+=$errors.Count}
$rows|ConvertTo-Json -Depth 10
if($bad -gt 0){exit 1}else{exit 0}
