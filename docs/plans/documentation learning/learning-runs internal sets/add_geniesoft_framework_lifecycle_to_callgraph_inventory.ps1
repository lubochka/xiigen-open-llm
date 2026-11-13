$ErrorActionPreference = "Stop"

$projectRoot = "C:\Projects\geniesoftGameMVPServer"
$runRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$methodPath = Join-Path $runRoot "extracted-method-inventory.jsonl"
$edgePath = Join-Path $runRoot "extracted-callgraph-edges.jsonl"
$summaryPath = Join-Path $runRoot "CALLGRAPH-EXTRACTION-SUMMARY.json"

if (!(Test-Path $projectRoot)) {
    throw "Missing project root: $projectRoot"
}
if (!(Test-Path $methodPath)) {
    throw "Missing method inventory: $methodPath"
}
if (!(Test-Path $edgePath)) {
    throw "Missing edge inventory: $edgePath"
}

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 40 -Compress)
}

function New-MethodRow {
    param(
        [string]$relative,
        [string]$class,
        [string]$method,
        [string]$symbol,
        [string]$body,
        [int]$startLine,
        [string]$artifactKind
    )
    return [ordered]@{
        project              = $projectRoot
        file                 = Join-Path $projectRoot $relative
        relative             = $relative
        class                = $class
        method               = $method
        symbol               = $symbol
        body                 = $body
        start_line           = $startLine
        target_framework     = "netcoreapp3.1"
        runtime_family       = "NET_CORE"
        project_style        = "sdk_style"
        web_framework        = "ASP.NET Core WebApi"
        web_framework_version = "Microsoft.AspNetCore.App 3.1"
        artifact_kind        = $artifactKind
        extraction_note      = "framework-lifecycle support row for thin ASP.NET Core template-like projects"
    }
}

function New-EdgeRow {
    param(
        [string]$callerSymbol,
        [string]$calleeSymbol,
        [string]$callerFile,
        [string]$calleeFile,
        [string]$kind
    )
    return [ordered]@{
        project       = $projectRoot
        caller_file   = $callerFile
        caller_symbol = $callerSymbol
        callee_file   = $calleeFile
        callee_symbol = $calleeSymbol
        call_kind     = $kind
        edge_authority = "framework_lifecycle_support"
    }
}

$program = "geniesoftGameMVPServer\geniesoftGameMVPServer\Program.cs"
$startup = "geniesoftGameMVPServer\geniesoftGameMVPServer\Startup.cs"
$controller = "geniesoftGameMVPServer\geniesoftGameMVPServer\Controllers\WeatherForecastController.cs"
$model = "geniesoftGameMVPServer\geniesoftGameMVPServer\WeatherForecast.cs"

$newMethods = New-Object System.Collections.ArrayList
[void]$newMethods.Add((New-MethodRow -relative $program -class "Program" -method "Main" -symbol "geniesoftGameMVPServer::Program.Main" -body "public static void Main(string[] args) { CreateHostBuilder(args).Build().Run(); }" -startLine 14 -artifactKind "framework_lifecycle_entrypoint"))
[void]$newMethods.Add((New-MethodRow -relative $program -class "Program" -method "CreateHostBuilder" -symbol "geniesoftGameMVPServer::Program.CreateHostBuilder" -body "public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });" -startLine 19 -artifactKind "framework_lifecycle_host_builder"))
[void]$newMethods.Add((New-MethodRow -relative $startup -class "Startup" -method "Startup" -symbol "geniesoftGameMVPServer::Startup.Constructor" -body "public Startup(IConfiguration configuration) { Configuration = configuration; }" -startLine 16 -artifactKind "framework_lifecycle_constructor"))
[void]$newMethods.Add((New-MethodRow -relative $startup -class "Startup" -method "ConfigureServices" -symbol "geniesoftGameMVPServer::Startup.ConfigureServices" -body "public void ConfigureServices(IServiceCollection services) { services.AddControllers(); }" -startLine 26 -artifactKind "framework_lifecycle_services"))
[void]$newMethods.Add((New-MethodRow -relative $startup -class "Startup" -method "Configure" -symbol "geniesoftGameMVPServer::Startup.Configure" -body "public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); } app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthorization(); app.UseEndpoints(endpoints => { endpoints.MapControllers(); }); }" -startLine 32 -artifactKind "framework_lifecycle_middleware"))
[void]$newMethods.Add((New-MethodRow -relative $controller -class "WeatherForecastController" -method "WeatherForecastController" -symbol "geniesoftGameMVPServer::WeatherForecastController.Constructor" -body "public WeatherForecastController(ILogger<WeatherForecastController> logger) { _logger = logger; }" -startLine 21 -artifactKind "framework_lifecycle_controller_activation"))
[void]$newMethods.Add((New-MethodRow -relative $controller -class "WeatherForecastController" -method "Get" -symbol "geniesoftGameMVPServer::WeatherForecastController.Get" -body "public IEnumerable<WeatherForecast> Get() { var rng = new Random(); return Enumerable.Range(1, 5).Select(index => new WeatherForecast { Date = DateTime.Now.AddDays(index), TemperatureC = rng.Next(-20, 55), Summary = Summaries[rng.Next(Summaries.Length)] }).ToArray(); }" -startLine 27 -artifactKind "controller_action"))
[void]$newMethods.Add((New-MethodRow -relative $model -class "WeatherForecast" -method "get_TemperatureF" -symbol "geniesoftGameMVPServer::WeatherForecast.TemperatureF.get" -body "public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);" -startLine 11 -artifactKind "expression_bodied_property"))

$newEdges = New-Object System.Collections.ArrayList
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Program.Main" -calleeSymbol "geniesoftGameMVPServer::Program.CreateHostBuilder" -callerFile $program -calleeFile $program -kind "direct_method_call"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Program.CreateHostBuilder" -calleeSymbol "geniesoftGameMVPServer::Startup.Constructor" -callerFile $program -calleeFile $startup -kind "aspnetcore_host_lifecycle"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Program.CreateHostBuilder" -calleeSymbol "geniesoftGameMVPServer::Startup.ConfigureServices" -callerFile $program -calleeFile $startup -kind "aspnetcore_host_lifecycle"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Program.CreateHostBuilder" -calleeSymbol "geniesoftGameMVPServer::Startup.Configure" -callerFile $program -calleeFile $startup -kind "aspnetcore_host_lifecycle"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Startup.ConfigureServices" -calleeSymbol "geniesoftGameMVPServer::WeatherForecastController.Constructor" -callerFile $startup -calleeFile $controller -kind "aspnetcore_controller_activation_contract"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::Startup.Configure" -calleeSymbol "geniesoftGameMVPServer::WeatherForecastController.Get" -callerFile $startup -calleeFile $controller -kind "aspnetcore_endpoint_routing_contract"))
[void]$newEdges.Add((New-EdgeRow -callerSymbol "geniesoftGameMVPServer::WeatherForecastController.Get" -calleeSymbol "geniesoftGameMVPServer::WeatherForecast.TemperatureF.get" -callerFile $controller -calleeFile $model -kind "response_serialization_property_contract"))

$existingMethods = @(Get-Content -LiteralPath $methodPath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json } |
    Where-Object { [string]$_.project -ne $projectRoot })
$existingEdges = @(Get-Content -LiteralPath $edgePath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json } |
    Where-Object { [string]$_.project -ne $projectRoot })

Set-Content -LiteralPath $methodPath -Value (@($existingMethods) + @($newMethods) | ForEach-Object { JsonLine $_ }) -Encoding UTF8
Set-Content -LiteralPath $edgePath -Value (@($existingEdges) + @($newEdges) | ForEach-Object { JsonLine $_ }) -Encoding UTF8

$summary = [ordered]@{
    method_count    = $existingMethods.Count + $newMethods.Count
    edge_count      = $existingEdges.Count + $newEdges.Count
    projects        = [object[]]@(
        "C:\Projects\wizGameserver",
        "C:\Projects\GenieDynamicServer",
        "C:\Projects\geniesoftGameMVPServer",
        "C:\Projects\genietasks",
        "C:\Projects\askapediaweb"
    )
    generated_at    = (Get-Date).ToString("o")
    extraction_note = "Regex/direct-name callgraph plus explicit ASP.NET Core framework lifecycle support rows for thin template-like projects. geniesoftGameMVPServer now contributes host/startup/controller/property lifecycle transit, not only leaf/source/version rows."
}
Set-Content -LiteralPath $summaryPath -Value ($summary | ConvertTo-Json -Depth 20) -Encoding UTF8

Write-Host ("geniesoft lifecycle support added methods={0} edges={1} total_methods={2} total_edges={3}" -f $newMethods.Count, $newEdges.Count, $summary.method_count, $summary.edge_count)
