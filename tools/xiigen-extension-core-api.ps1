param(
    [Parameter(Mandatory = $true)]
    [string]$RequestPath,

    [Parameter(Mandatory = $true)]
    [string]$ResponsePath,

    [string]$TracePath,

    [string]$CoreRoot
)

$ErrorActionPreference = "Stop"

function Resolve-XiigenFullPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Ensure-XiigenDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
}

function Write-XiigenJson {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    Ensure-XiigenDirectory -Path $Path
    $Value | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-XiigenProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $null
}

function Get-XiigenSha256 {
    param([string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha256.ComputeHash($bytes)
    }
    finally {
        $sha256.Dispose()
    }
    return ([BitConverter]::ToString($hash) -replace "-", "").ToLowerInvariant()
}

function Find-XiigenRepoRoot {
    $current = Get-Item -LiteralPath (Get-Location).Path
    while ($current) {
        if ((Test-Path -LiteralPath (Join-Path $current.FullName "43.SmartSkillsDomain")) -and
            (Test-Path -LiteralPath (Join-Path $current.FullName "models"))) {
            return $current.FullName
        }

        $current = $current.Parent
    }

    throw "Could not locate llm_mvp_core repo root from current directory."
}

$startedUtc = (Get-Date).ToUniversalTime().ToString("o")
$resolvedCoreRoot = if ([string]::IsNullOrWhiteSpace($CoreRoot)) {
    Find-XiigenRepoRoot
} else {
    Resolve-XiigenFullPath -Path $CoreRoot
}
$resolvedRequestPath = Resolve-XiigenFullPath -Path $RequestPath
$resolvedResponsePath = Resolve-XiigenFullPath -Path $ResponsePath
$resolvedTracePath = if ([string]::IsNullOrWhiteSpace($TracePath)) {
    $null
} else {
    Resolve-XiigenFullPath -Path $TracePath
}
$scriptPath = $MyInvocation.MyCommand.Path
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("xiigen-extension-core-api-" + [Guid]::NewGuid().ToString("N"))

try {
    if (-not (Test-Path -LiteralPath $resolvedRequestPath)) {
        throw "RequestPath does not exist: $resolvedRequestPath"
    }

    $requestObject = Get-Content -Raw -Encoding UTF8 -LiteralPath $resolvedRequestPath | ConvertFrom-Json
    $promptText = [string](Get-XiigenProperty -Object $requestObject -Name "UserPromptOrTaskText")
    $promptSha256 = Get-XiigenSha256 -Value $promptText

    New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
    $projectPath = Join-Path $tempRoot "XiigenExtensionCoreApiInvoker.csproj"
    $programPath = Join-Path $tempRoot "Program.cs"
    $smartSkillsProject = Join-Path $resolvedCoreRoot "43.SmartSkillsDomain\43.SmartSkillsDomain.csproj"
    if (-not (Test-Path -LiteralPath $smartSkillsProject)) {
        throw "SmartSkillsDomain project not found: $smartSkillsProject"
    }

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="$smartSkillsProject" />
  </ItemGroup>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@ | Set-Content -LiteralPath $projectPath -Encoding UTF8

    @'
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: <request-path> <response-path> <core-root>");
    return 2;
}

var requestPath = Path.GetFullPath(args[0]);
var responsePath = Path.GetFullPath(args[1]);
var coreRoot = Path.GetFullPath(args[2]);
Directory.SetCurrentDirectory(coreRoot);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var requestJson = await File.ReadAllTextAsync(requestPath, Encoding.UTF8);
var request = JsonSerializer.Deserialize<XiigenExtensionRequest>(requestJson, jsonOptions);
if (request is null)
{
    Console.Error.WriteLine("XiigenExtensionRequest deserialized null.");
    return 3;
}

var service = new XiigenExtensionApiService(coreRoot);
var result = service.ProcessHostExtensionRequest(request);
if (!result.IsSuccess || result.Value is null)
{
    Console.Error.WriteLine($"{result.ErrorCode}:{result.ErrorMessage}");
    return 4;
}

var directory = Path.GetDirectoryName(responsePath);
if (!string.IsNullOrWhiteSpace(directory))
{
    Directory.CreateDirectory(directory);
}

await File.WriteAllTextAsync(responsePath, JsonSerializer.Serialize(result.Value, jsonOptions) + Environment.NewLine);
return 0;
'@ | Set-Content -LiteralPath $programPath -Encoding UTF8

    Ensure-XiigenDirectory -Path $resolvedResponsePath
    $dotnetArgs = @("run", "--project", $projectPath, "--", $resolvedRequestPath, $resolvedResponsePath, $resolvedCoreRoot)
    & dotnet @dotnetArgs
    if ($LASTEXITCODE -ne 0) {
        throw "XiigenExtensionApiService invocation failed with exit code $LASTEXITCODE."
    }

    $responseObject = Get-Content -Raw -Encoding UTF8 -LiteralPath $resolvedResponsePath | ConvertFrom-Json
    $runtimeClaimed = Get-XiigenProperty -Object (Get-XiigenProperty -Object $responseObject -Name "CheckSetResults") -Name "runtime_extension_claimed"
    $semanticClaimed = Get-XiigenProperty -Object (Get-XiigenProperty -Object $responseObject -Name "CheckSetResults") -Name "semantic_quality_claimed"

    if ($resolvedTracePath) {
        Write-XiigenJson -Path $resolvedTracePath -Value ([ordered]@{
            schema_version = 1
            endpoint = "xiigen-extension-core-api"
            script_path = "tools/xiigen-extension-core-api.ps1"
            call_target = "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest"
            request_path = $resolvedRequestPath
            response_path = $resolvedResponsePath
            started_utc = $startedUtc
            completed_utc = (Get-Date).ToUniversalTime().ToString("o")
            host_name = Get-XiigenProperty -Object $requestObject -Name "HostName"
            host_type = Get-XiigenProperty -Object $requestObject -Name "HostType"
            method = Get-XiigenProperty -Object $requestObject -Name "Method"
            correlation_id = Get-XiigenProperty -Object $requestObject -Name "CorrelationId"
            user_prompt_sha256 = $promptSha256
            user_prompt_redacted = $true
            raw_private_prompt_persisted = $false
            contains_secrets = $false
            response_success = [bool](Get-XiigenProperty -Object $responseObject -Name "Success")
            response_status = Get-XiigenProperty -Object $responseObject -Name "Status"
            semantic_quality_claimed = $semanticClaimed
            runtime_extension_claimed = $runtimeClaimed
            host_runtime_claimed = $false
            legacy_contract_substitution_used = $false
            external_ai_used = $false
            training_started = $false
            core_asset_loader = "XiigenExtensionApiService common asset root constructor"
            claim_scope = "core-owned endpoint invocation only; no host runtime claim"
        })
    }

    exit 0
}
catch {
    $message = $_.Exception.Message
    Write-XiigenJson -Path $resolvedResponsePath -Value ([ordered]@{
        Success = $false
        Status = "core_endpoint_error"
        ChatDisplayText = "Xiigen core endpoint failed before semantic execution."
        SelectedSkills = @()
        CheckSetResults = [ordered]@{
            core_boundary_executed = "false"
            semantic_quality_claimed = "false"
            runtime_extension_claimed = "false"
        }
        StructuredTaskRefs = @()
        ScenarioRefs = @()
        Warnings = @("semantic_quality_claimed=false", "core_boundary_only_no_host_runtime_claim")
        Errors = @("core_endpoint_invocation_failed")
        DebugTrace = $null
    })
    if ($resolvedTracePath) {
        Write-XiigenJson -Path $resolvedTracePath -Value ([ordered]@{
            schema_version = 1
            endpoint = "xiigen-extension-core-api"
            script_path = "tools/xiigen-extension-core-api.ps1"
            call_target = "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest"
            started_utc = $startedUtc
            completed_utc = (Get-Date).ToUniversalTime().ToString("o")
            response_path = $resolvedResponsePath
            user_prompt_redacted = $true
            raw_private_prompt_persisted = $false
            semantic_quality_claimed = "false"
            runtime_extension_claimed = "false"
            host_runtime_claimed = $false
            legacy_contract_substitution_used = $false
            external_ai_used = $false
            training_started = $false
            passed = $false
            error = $message
        })
    }

    Write-Error $message
    exit 1
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
