[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    # Документация зовёт эти параметры по смыслу Changed*, а короткие имена
    # оставлены для ручных вызовов и старых рабочих заметок.
    [Alias('ChangedSince')]
    [string]$Since = '',
    [Alias('ChangedTo')]
    [string]$To = 'HEAD',
    [string[]]$ChangedPath = @(),
    [string]$ChangedPathsFile = '',
    [string]$SnapshotPath = '',
    [string]$BatchesFile = '',
    [ValidateSet('Human', 'Json', 'OnlyArgument')]
    [string]$OutputMode = 'Human',
    [switch]$SelfCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-ToRepoRelativePath {
    param(
        [string]$RepoRootPath,
        [string]$Path
    )

    $trimmed = $Path.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return ''
    }

    # Нормализуем через полный путь, чтобы `../...` и абсолютные пути не могли
    # незаметно стать внутренним repo-relative именем другого файла.
    $repoFull = [System.IO.Path]::GetFullPath($RepoRootPath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)

    if ([System.IO.Path]::IsPathRooted($trimmed)) {
        $candidateFull = [System.IO.Path]::GetFullPath($trimmed)
    }
    else {
        $candidateFull = [System.IO.Path]::GetFullPath((Join-Path $repoFull $trimmed))
    }

    if ($candidateFull.Equals($repoFull, [StringComparison]::OrdinalIgnoreCase)) {
        return ''
    }

    $repoPrefix = $repoFull + [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidateFull.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Изменённый путь находится вне репозитория и не может быть выбран автоматически: $Path"
    }

    return $candidateFull.Substring($repoPrefix.Length) -replace '\\', '/'
}

function Read-ChangedPaths {
    param(
        [string]$RepoRootPath,
        [string]$BaseRef,
        [string]$TargetRef,
        [string[]]$ExplicitPaths,
        [string]$PathsFile
    )

    $paths = New-Object System.Collections.Generic.List[string]
    foreach ($pathGroup in $ExplicitPaths) {
        # Некоторые вызовы `pwsh -File` передают список через запятую одной
        # строкой. Делим такую форму явно, чтобы второй путь не стал странным
        # именем файла.
        foreach ($path in ([string]$pathGroup -split ',')) {
            $relative = Convert-ToRepoRelativePath -RepoRootPath $RepoRootPath -Path $path
            if (-not [string]::IsNullOrWhiteSpace($relative)) {
                [void]$paths.Add($relative)
            }
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($PathsFile)) {
        foreach ($path in Get-Content -LiteralPath $PathsFile -Encoding UTF8) {
            $relative = Convert-ToRepoRelativePath -RepoRootPath $RepoRootPath -Path $path
            if (-not [string]::IsNullOrWhiteSpace($relative)) {
                [void]$paths.Add($relative)
            }
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($BaseRef)) {
        # Git supplies only changed file names here. The selector deliberately
        # avoids diff-body inspection, so a targeted run cannot replace review.
        $gitPaths = @(git -C $RepoRootPath diff --name-only $BaseRef $TargetRef)
        if ($LASTEXITCODE -ne 0) {
            throw "git diff --name-only failed for $BaseRef..$TargetRef"
        }

        foreach ($path in $gitPaths) {
            $relative = Convert-ToRepoRelativePath -RepoRootPath $RepoRootPath -Path $path
            if (-not [string]::IsNullOrWhiteSpace($relative)) {
                [void]$paths.Add($relative)
            }
        }
    }

    $unique = New-Object System.Collections.Generic.List[string]
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($path in $paths) {
        if ($seen.Add($path)) {
            [void]$unique.Add($path)
        }
    }

    return $unique.ToArray()
}

function Normalize-SelectorPath {
    param([string]$Path)

    return $Path.Replace([System.IO.Path]::DirectorySeparatorChar, '/').
        Replace([System.IO.Path]::AltDirectorySeparatorChar, '/')
}

function Normalize-SelectorDll {
    param([string]$Path)

    return (Normalize-SelectorPath -Path $Path).TrimStart('./')
}

function Convert-FullPathToRepoRelativeForSelection {
    param(
        [string]$RepoRootPath,
        [string]$FullPath
    )

    $repoFull = [System.IO.Path]::GetFullPath($RepoRootPath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $candidateFull = [System.IO.Path]::GetFullPath($FullPath)
    $repoPrefix = $repoFull + [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidateFull.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        return ''
    }

    return Normalize-SelectorPath -Path $candidateFull.Substring($repoPrefix.Length)
}

function Read-BatchRowsForSelection {
    param([string]$Path)

    $rows = New-Object System.Collections.Generic.List[object]
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $Path -Encoding UTF8) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) {
            continue
        }

        $parts = $line -split "`t", 9
        if ($parts.Count -ne 9) {
            throw "Строка $lineNumber в batches.tsv должна иметь 9 колонок; найдено $($parts.Count)."
        }

        [void]$rows.Add([pscustomobject]@{
            Order = $parts[0]
            BatchId = $parts[1]
            HumanName = $parts[2]
            Dll = Normalize-SelectorDll -Path $parts[3]
            Filter = $parts[4]
            LineNumber = $lineNumber
        })
    }

    return $rows.ToArray()
}

function Convert-BatchRowsToNames {
    param([object[]]$Rows)

    $names = [ordered]@{}
    foreach ($row in $Rows) {
        $names[$row.BatchId] = $row.HumanName
    }

    return $names
}

function Test-PathHasIgnoredSegment {
    param([string]$Path)

    foreach ($part in ($Path -split '[\\/]')) {
        if ($part -in @('bin', 'obj', 'NuGet')) {
            return $true
        }
    }

    return $false
}

$script:RoslynSyntaxParserLoaded = $false

function Ensure-RoslynSyntaxParser {
    if ($script:RoslynSyntaxParserLoaded) {
        return
    }

    $codeAnalysis = Join-Path $PSHOME 'Microsoft.CodeAnalysis.dll'
    $csharp = Join-Path $PSHOME 'Microsoft.CodeAnalysis.CSharp.dll'
    if (-not (Test-Path -LiteralPath $codeAnalysis) -or -not (Test-Path -LiteralPath $csharp)) {
        throw 'Синтаксический разбор Roslyn недоступен в текущем PowerShell; автоматический выбор тестовой группы небезопасен.'
    }

    Add-Type -Path $codeAnalysis
    Add-Type -Path $csharp
    $script:RoslynSyntaxParserLoaded = $true
}

function Get-AttributeSimpleNameForSelection {
    param([object]$Attribute)

    $name = [string]$Attribute.Name
    $name = $name.Replace('global::', '')
    if ($name.Contains('<')) {
        $name = $name.Substring(0, $name.IndexOf('<'))
    }

    $name = ($name -split '::')[-1]
    $name = ($name -split '\.')[-1]
    if ($name.EndsWith('Attribute', [StringComparison]::Ordinal)) {
        $name = $name.Substring(0, $name.Length - 'Attribute'.Length)
    }

    return $name
}

function Get-BaseTypeSimpleNamesForSelection {
    param([object]$ClassDeclaration)

    $names = New-Object System.Collections.Generic.List[string]
    if ($null -eq $ClassDeclaration.BaseList) {
        return $names.ToArray()
    }

    foreach ($baseType in $ClassDeclaration.BaseList.Types) {
        $name = ([string]$baseType.Type).Replace('global::', '')
        $name = ($name -split '::')[-1]
        $name = ($name -split '\.')[-1]
        if ($name.EndsWith('Attribute', [StringComparison]::Ordinal)) {
            $name = $name.Substring(0, $name.Length - 'Attribute'.Length)
        }

        [void]$names.Add($name)
    }

    return $names.ToArray()
}

function Get-AttributeAliasBaseNamesForSelection {
    param([object]$SyntaxRoot)

    $aliases = [ordered]@{}
    foreach ($usingDirective in ($SyntaxRoot.DescendantNodes() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax] })) {
        if ($null -eq $usingDirective.Alias) {
            continue
        }

        $target = ([string]$usingDirective.Name).Replace('global::', '')
        $targetSimple = ($target -split '::')[-1]
        $targetSimple = ($targetSimple -split '\.')[-1]
        if ($targetSimple.EndsWith('Attribute', [StringComparison]::Ordinal)) {
            $targetSimple = $targetSimple.Substring(0, $targetSimple.Length - 'Attribute'.Length)
        }

        $aliases[[string]$usingDirective.Alias.Name.Identifier.Text] = $targetSimple
    }

    return $aliases
}

function Get-TestAttributeNamesForSelection {
    param([object]$SyntaxRoot)

    $attributeBases = @{}
    foreach ($class in ($SyntaxRoot.DescendantNodes() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax] })) {
        $name = [string]$class.Identifier.Text
        if ($name.EndsWith('Attribute', [StringComparison]::Ordinal)) {
            $name = $name.Substring(0, $name.Length - 'Attribute'.Length)
        }

        $attributeBases[$name] = @(Get-BaseTypeSimpleNamesForSelection -ClassDeclaration $class)
    }

    $attributeAliases = Get-AttributeAliasBaseNamesForSelection -SyntaxRoot $SyntaxRoot
    foreach ($aliasName in $attributeAliases.Keys) {
        # Alias рассматриваем как ещё одно имя атрибута. Так `[Spec]` на
        # локальный `LocalOnlyCLAttribute : FactAttribute` проходит ту же
        # цепочку наследования, а не теряется из-за отсутствия слова Fact.
        $attributeBases[$aliasName] = @([string]$attributeAliases[$aliasName])
    }

    $testAttributes = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    [void]$testAttributes.Add('Fact')
    [void]$testAttributes.Add('Theory')

    $changed = $true
    while ($changed) {
        $changed = $false
        foreach ($name in $attributeBases.Keys) {
            if ($testAttributes.Contains($name)) {
                continue
            }

            foreach ($baseName in $attributeBases[$name]) {
                if ($testAttributes.Contains($baseName)) {
                    [void]$testAttributes.Add($name)
                    $changed = $true
                    break
                }
            }
        }
    }

    $aliasNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($aliasName in $attributeAliases.Keys) {
        [void]$aliasNames.Add([string]$aliasName)
    }

    return [pscustomobject]@{
        TestNames = $testAttributes
        AliasNames = $aliasNames
    }
}

function Test-IsProbablyUnsupportedTestAttributeForSelection {
    param([string]$SimpleName)

    return $SimpleName.Contains('Fact', [StringComparison]::OrdinalIgnoreCase) -or
        $SimpleName.Contains('Theory', [StringComparison]::OrdinalIgnoreCase)
}

function Test-IsKnownNonTestMethodAttributeForSelection {
    param([string]$SimpleName)

    return $SimpleName -in @(
        'Benchmark',
        'CallerArgumentExpression',
        'CallerFilePath',
        'CallerLineNumber',
        'CallerMemberName',
        'Conditional',
        'DoesNotReturn',
        'GeneratedRegex',
        'InlineData',
        'MemberData',
        'MethodImpl',
        'MaybeNull',
        'NotNull',
        'NotNullWhen',
        'Obsolete',
        'RequiresUnreferencedCode',
        'SuppressMessage',
        'Trait',
        'UnconditionalSuppressMessage'
    )
}

function Get-ClassFqnForSelection {
    param([object]$ClassDeclaration)

    $typeNames = New-Object System.Collections.Generic.List[string]
    $node = $ClassDeclaration
    while ($null -ne $node) {
        if ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax]) {
            $typeNames.Insert(0, [string]$node.Identifier.Text)
        }

        $node = $node.Parent
    }

    $namespaceNames = New-Object System.Collections.Generic.List[string]
    $node = $ClassDeclaration.Parent
    while ($null -ne $node) {
        if ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax]) {
            $namespaceNames.Insert(0, [string]$node.Name)
        }

        $node = $node.Parent
    }

    $typeName = ($typeNames -join '+')
    if ($namespaceNames.Count -eq 0) {
        return $typeName
    }

    return ($namespaceNames -join '.') + '.' + $typeName
}

function Get-TestClassFqnsFromSourceFile {
    param([string]$Path)

    Ensure-RoslynSyntaxParser
    $text = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $root = [Microsoft.CodeAnalysis.CSharp.SyntaxFactory]::ParseCompilationUnit($text)
    $parseErrors = @($root.GetDiagnostics() | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
    if ($parseErrors.Count -gt 0) {
        throw "C#-исходник не удалось безопасно разобрать для автоматического выбора группы: $Path"
    }

    $testAttributeInfo = Get-TestAttributeNamesForSelection -SyntaxRoot $root
    $testAttributeNames = $testAttributeInfo.TestNames
    $attributeAliasNames = $testAttributeInfo.AliasNames
    $classes = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $maybeHiddenTests = New-Object System.Collections.Generic.List[string]
    foreach ($method in ($root.DescendantNodes() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] })) {
        $hasTestAttribute = $false
        $unsupportedTestAttribute = ''
        $unknownMethodAttributes = New-Object System.Collections.Generic.List[string]
        foreach ($list in $method.AttributeLists) {
            foreach ($attribute in $list.Attributes) {
                $simpleName = Get-AttributeSimpleNameForSelection -Attribute $attribute
                if ($testAttributeNames.Contains($simpleName)) {
                    $hasTestAttribute = $true
                    continue
                }

                if ($attributeAliasNames.Contains($simpleName) -or
                    (Test-IsProbablyUnsupportedTestAttributeForSelection -SimpleName $simpleName)) {
                    $unsupportedTestAttribute = $simpleName
                    continue
                }

                if (-not (Test-IsKnownNonTestMethodAttributeForSelection -SimpleName $simpleName)) {
                    [void]$unknownMethodAttributes.Add($simpleName)
                }
            }
        }

        if (-not $hasTestAttribute) {
            if (-not [string]::IsNullOrWhiteSpace($unsupportedTestAttribute)) {
                throw "Неподдержанный xUnit-похожий атрибут '$unsupportedTestAttribute' в $Path; автоматический выбор был бы частичным."
            }

            if ($unknownMethodAttributes.Count -gt 0) {
                [void]$maybeHiddenTests.Add("$([string]$method.Identifier.Text):$($unknownMethodAttributes -join ',')")
            }

            continue
        }

        $containingClass = $method.Ancestors() |
            Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax] } |
            Select-Object -First 1
        if ($null -ne $containingClass) {
            [void]$classes.Add((Get-ClassFqnForSelection -ClassDeclaration $containingClass))
        }
    }

    if ($classes.Count -gt 0 -and $maybeHiddenTests.Count -gt 0) {
        throw "Неразрешённые атрибуты методов в файле, где уже есть тесты: $($maybeHiddenTests -join '; '). Автоматический выбор был бы частичным."
    }

    return @($classes | Sort-Object)
}

function Normalize-ProjectIncludePathForSelection {
    param([string]$Path)

    $normalized = $Path.Replace('\\', [string][System.IO.Path]::DirectorySeparatorChar).
        Replace('/', [string][System.IO.Path]::DirectorySeparatorChar)
    return $normalized
}

function Get-TestProjectsForSelection {
    param(
        [string]$RepoRootPath,
        [object[]]$BatchRows
    )

    $repoFull = [System.IO.Path]::GetFullPath($RepoRootPath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $wantedDlls = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($row in $BatchRows) {
        [void]$wantedDlls.Add((Normalize-SelectorDll -Path $row.Dll))
    }

    $projects = New-Object System.Collections.Generic.List[object]
    foreach ($dll in ($wantedDlls | Sort-Object)) {
        $normalizedDll = Normalize-SelectorDll -Path $dll
        $binIndex = $normalizedDll.IndexOf('/bin/Release/', [StringComparison]::OrdinalIgnoreCase)
        if ($binIndex -lt 0) {
            throw "Не удалось вывести каталог тестового проекта из DLL в списке групп: $normalizedDll"
        }

        $projectRelativeDirectory = $normalizedDll.Substring(0, $binIndex)
        $afterRelease = $normalizedDll.Substring($binIndex + '/bin/Release/'.Length)
        $pathParts = @($afterRelease -split '/')
        if ($pathParts.Count -lt 2) {
            throw "Не удалось вывести target framework из DLL в списке групп: $normalizedDll"
        }

        $targetFramework = $pathParts[0]
        $assemblyFileName = [System.IO.Path]::GetFileName($normalizedDll)
        $expectedAssemblyName = [System.IO.Path]::GetFileNameWithoutExtension($assemblyFileName)
        $projectDirectory = [System.IO.Path]::GetFullPath((Join-Path $repoFull (Normalize-ProjectIncludePathForSelection -Path $projectRelativeDirectory)))
        if (-not (Test-Path -LiteralPath $projectDirectory -PathType Container)) {
            throw "Не найден каталог тестового проекта для DLL из списка групп: $normalizedDll"
        }

        $projectFiles = @(Get-ChildItem -LiteralPath $projectDirectory -Filter '*.csproj' -File)
        if ($projectFiles.Count -eq 0) {
            throw "Не найден файл тестового проекта в $projectRelativeDirectory для DLL из списка групп: $normalizedDll"
        }

        $matchedProjects = New-Object System.Collections.Generic.List[object]
        foreach ($projectFile in $projectFiles) {
            if (Test-PathHasIgnoredSegment -Path $projectFile.FullName) {
                continue
            }

            [xml]$projectXml = Get-Content -LiteralPath $projectFile.FullName -Raw -Encoding UTF8
            $targetFrameworkNodes = @($projectXml.GetElementsByTagName('TargetFramework'))
            $targetFrameworksNodes = @($projectXml.GetElementsByTagName('TargetFrameworks'))
            $projectTargetFrameworks = New-Object System.Collections.Generic.List[string]
            foreach ($node in $targetFrameworkNodes) {
                if (-not [string]::IsNullOrWhiteSpace($node.InnerText)) {
                    [void]$projectTargetFrameworks.Add($node.InnerText.Trim())
                }
            }

            foreach ($node in $targetFrameworksNodes) {
                if (-not [string]::IsNullOrWhiteSpace($node.InnerText)) {
                    foreach ($piece in ($node.InnerText -split ';')) {
                        if (-not [string]::IsNullOrWhiteSpace($piece)) {
                            [void]$projectTargetFrameworks.Add($piece.Trim())
                        }
                    }
                }
            }

            if (-not $projectTargetFrameworks.Contains($targetFramework)) {
                continue
            }

            $assemblyNameNode = @($projectXml.GetElementsByTagName('AssemblyName')) | Select-Object -First 1
            $assemblyName = if ($null -ne $assemblyNameNode -and -not [string]::IsNullOrWhiteSpace($assemblyNameNode.InnerText)) {
                $assemblyNameNode.InnerText.Trim()
            }
            else {
                [System.IO.Path]::GetFileNameWithoutExtension($projectFile.FullName)
            }

            if (-not [string]::Equals($assemblyName, $expectedAssemblyName, [StringComparison]::OrdinalIgnoreCase)) {
                continue
            }

            # Смотрим только проекты, выведенные из текущих DLL списка групп.
            # Иначе временные TestResults-деревья могут внезапно стать источником маршрута.
            $linkedSources = New-Object System.Collections.Generic.List[string]
            foreach ($compile in $projectXml.GetElementsByTagName('Compile')) {
                $include = [string]$compile.GetAttribute('Include')
                if ([string]::IsNullOrWhiteSpace($include) -or $include.Contains('*', [StringComparison]::Ordinal)) {
                    continue
                }

                foreach ($piece in ($include -split ';')) {
                    if ([string]::IsNullOrWhiteSpace($piece)) {
                        continue
                    }

                    $includePath = Normalize-ProjectIncludePathForSelection -Path $piece.Trim()
                    $full = [System.IO.Path]::GetFullPath((Join-Path $projectDirectory $includePath))
                    if (-not (Test-PathHasIgnoredSegment -Path $full)) {
                        [void]$linkedSources.Add($full)
                    }
                }
            }

            [void]$matchedProjects.Add([pscustomobject]@{
                ProjectPath = $projectFile.FullName
                ProjectDirectory = $projectDirectory
                Dll = $normalizedDll
                LinkedSources = $linkedSources.ToArray()
            })
        }

        if ($matchedProjects.Count -ne 1) {
            throw "DLL из списка групп $normalizedDll должна однозначно указывать на один тестовый проект; найдено $($matchedProjects.Count)."
        }

        [void]$projects.Add($matchedProjects[0])
    }

    return $projects.ToArray()
}

function Test-ProjectContainsSourcePath {
    param(
        [object]$Project,
        [string]$SourceFullPath
    )

    if ($Project.LinkedSources | Where-Object { [string]::Equals($_, $SourceFullPath, [StringComparison]::OrdinalIgnoreCase) }) {
        return $true
    }

    $projectPrefix = $Project.ProjectDirectory.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    return $SourceFullPath.StartsWith($projectPrefix, [StringComparison]::OrdinalIgnoreCase) -and
        -not (Test-PathHasIgnoredSegment -Path $SourceFullPath)
}

function Get-PositiveFilterTermsForSelection {
    param([string]$Filter)

    foreach ($piece in ($Filter -split '[|&]')) {
        $trimmed = $piece.Trim().Trim('(', ')')
        if ($trimmed.StartsWith('FullyQualifiedName~', [StringComparison]::Ordinal)) {
            $trimmed.Substring('FullyQualifiedName~'.Length)
        }
    }
}

function Test-ClassOrMethodTermForSelection {
    param(
        [string]$ClassFqn,
        [string]$Term
    )

    if ($Term.EndsWith('.', [StringComparison]::Ordinal)) {
        $classBoundary = $Term.Substring(0, $Term.Length - 1)
        return [string]::Equals($ClassFqn, $classBoundary, [StringComparison]::OrdinalIgnoreCase) -or
            $ClassFqn.StartsWith($Term, [StringComparison]::OrdinalIgnoreCase)
    }

    return $ClassFqn.Contains($Term, [StringComparison]::OrdinalIgnoreCase)
}

function Test-FilterTermForSelection {
    param(
        [string]$Term,
        [string]$ClassFqn
    )

    $trimmed = $Term.Trim().Trim('(', ')')
    if ($trimmed -eq '*') {
        return $true
    }

    if ($trimmed.StartsWith('FullyQualifiedName~', [StringComparison]::Ordinal)) {
        return Test-ClassOrMethodTermForSelection -ClassFqn $ClassFqn -Term $trimmed.Substring('FullyQualifiedName~'.Length)
    }

    if ($trimmed.StartsWith('FullyQualifiedName!~', [StringComparison]::Ordinal)) {
        return -not (Test-ClassOrMethodTermForSelection -ClassFqn $ClassFqn -Term $trimmed.Substring('FullyQualifiedName!~'.Length))
    }

    if ($trimmed.StartsWith('FullyQualifiedName=', [StringComparison]::Ordinal)) {
        return [string]::Equals($ClassFqn, $trimmed.Substring('FullyQualifiedName='.Length), [StringComparison]::Ordinal)
    }

    throw "Неподдержанный фильтр группы для автоматического выбора: $trimmed"
}

function Test-BatchFilterForClassSelection {
    param(
        [string]$Filter,
        [string]$ClassFqn
    )

    if ($Filter -eq '*') {
        return $true
    }

    foreach ($orPart in ($Filter -split '\|')) {
        $allMatched = $true
        foreach ($term in ($orPart -split '&')) {
            if (-not (Test-FilterTermForSelection -Term $term -ClassFqn $ClassFqn)) {
                $allMatched = $false
                break
            }
        }

        if ($allMatched) {
            return $true
        }
    }

    return $false
}

function Find-FirstBatchForClassSelection {
    param(
        [object[]]$Rows,
        [string]$Dll,
        [string]$ClassFqn
    )

    foreach ($row in $Rows) {
        if ($row.Dll -ne (Normalize-SelectorDll -Path $Dll)) {
            continue
        }

        if (Test-BatchFilterForClassSelection -Filter $row.Filter -ClassFqn $ClassFqn) {
            return $row
        }
    }

    return $null
}

function Get-SourcePathBatchAssignments {
    param(
        [string]$RepoRootPath,
        [string]$Path,
        [object[]]$BatchRows,
        [object[]]$Projects
    )

    $sourceFull = [System.IO.Path]::GetFullPath((Join-Path ([System.IO.Path]::GetFullPath($RepoRootPath)) $Path))
    if (-not (Test-Path -LiteralPath $sourceFull)) {
        return [pscustomobject]@{ Assignments = @(); ManualReason = 'исходный C# файл удалён; автоматический выбор не может доказать его тестовые классы' }
    }

    if (-not $Path.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase)) {
        return [pscustomobject]@{ Assignments = @(); ManualReason = 'изменённый файл не является C#-исходником тестов' }
    }

    try {
        $classFqns = @(Get-TestClassFqnsFromSourceFile -Path $sourceFull)
    }
    catch {
        return [pscustomobject]@{ Assignments = @(); ManualReason = $_.Exception.Message }
    }

    if ($classFqns.Count -eq 0) {
        return [pscustomobject]@{ Assignments = @(); ManualReason = 'в файле не найден тестовый класс xUnit, поэтому нужен ручной выбор' }
    }

    $projectMatches = @($Projects | Where-Object { Test-ProjectContainsSourcePath -Project $_ -SourceFullPath $sourceFull })
    if ($projectMatches.Count -eq 0) {
        return [pscustomobject]@{ Assignments = @(); ManualReason = 'файл не входит в тестовую сборку из текущего списка групп' }
    }

    $assignments = New-Object System.Collections.Generic.List[object]
    foreach ($project in $projectMatches) {
        foreach ($classFqn in $classFqns) {
            $batch = Find-FirstBatchForClassSelection -Rows $BatchRows -Dll $project.Dll -ClassFqn $classFqn
            if ($null -eq $batch) {
                return [pscustomobject]@{ Assignments = @(); ManualReason = "класс $classFqn в $($project.Dll) не имеет первого совпадающего маршрута в списке групп" }
            }

            [void]$assignments.Add([pscustomobject]@{
                Dll = $project.Dll
                ClassFqn = $classFqn
                BatchId = $batch.BatchId
                HumanName = $batch.HumanName
            })
        }
    }

    return [pscustomobject]@{ Assignments = $assignments.ToArray(); ManualReason = '' }
}

function Assert-BatchExistsForSelection {
    param(
        [hashtable]$BatchNames,
        [string]$BatchId
    )

    if (-not $BatchNames.Contains($BatchId)) {
        throw "Обязательная группа '$BatchId' отсутствует в текущем batches.tsv; автоматический выбор небезопасен."
    }
}

function Add-SelectedBatchForSelection {
    param(
        [System.Collections.Specialized.OrderedDictionary]$Selected,
        [System.Collections.Generic.List[object]]$Reasons,
        [System.Collections.IDictionary]$BatchNames,
        [string]$Path,
        [string]$BatchId,
        [string]$Reason,
        [string]$ClassFqn = '',
        [string]$Dll = ''
    )

    Assert-BatchExistsForSelection -BatchNames $BatchNames -BatchId $BatchId
    $Selected[$BatchId] = $true
    [void]$Reasons.Add([pscustomobject]@{
        Path = $Path
        BatchId = $BatchId
        HumanName = $BatchNames[$BatchId]
        ClassFqn = $ClassFqn
        Dll = $Dll
        Reason = $Reason
    })
}

function New-SelectionResult {
    param(
        [string[]]$Paths,
        [object[]]$BatchRows,
        [System.Collections.IDictionary]$BatchNames,
        [string]$RepoRootPath
    )

    $selected = [ordered]@{}
    $reasons = New-Object System.Collections.Generic.List[object]
    $manual = New-Object System.Collections.Generic.List[object]
    $focused = New-Object System.Collections.Generic.List[string]
    $projects = @(Get-TestProjectsForSelection -RepoRootPath $RepoRootPath -BatchRows $BatchRows)

    foreach ($path in $Paths) {
        if ($path -like 'tools/test-batches/*' -or
            $path -eq 'Tests/TestSuiteBatchManifest.cs' -or
            $path -eq 'Tests/TestSuiteBatchInventoryGuardTests.cs' -or
            $path -eq 'Tests/TestSuiteBatchLegacySnapshot.txt') {
            # Изменение самой лестницы тестовых групп проверяется её живыми
            # стражами, а не историческим снимком классов.
            Add-SelectedBatchForSelection `
                -Selected $selected `
                -Reasons $reasons `
                -BatchNames $BatchNames `
                -Path $path `
                -BatchId 'test-infrastructure-evidence-measurement' `
                -Reason 'изменён файл инфраструктуры групп; нужен основной страж и отдельный короткий список проверок распределения'
            if (-not $focused.Contains('tools/test-batches/test-suite-batch-inventory-guard-focused.tsv')) {
                [void]$focused.Add('tools/test-batches/test-suite-batch-inventory-guard-focused.tsv')
            }
            continue
        }

        if ($path -like 'Tests.ApiDashboardWpf/*') {
            Add-SelectedBatchForSelection `
                -Selected $selected `
                -Reasons $reasons `
                -BatchNames $BatchNames `
                -Path $path `
                -BatchId 'api-dashboard-wpf-windows' `
                -Reason 'изменён файл отдельной WPF-сборки панели наблюдения'
            continue
        }

        if ($path -like 'Tests/DesignConversion.ModelActivation.Tests/*') {
            Add-SelectedBatchForSelection `
                -Selected $selected `
                -Reasons $reasons `
                -BatchNames $BatchNames `
                -Path $path `
                -BatchId 'model-activation' `
                -Reason 'изменён файл отдельной сборки включения модели'
            continue
        }

        $assignmentResult = Get-SourcePathBatchAssignments `
            -RepoRootPath $RepoRootPath `
            -Path $path `
            -BatchRows $BatchRows `
            -Projects $projects
        if (@($assignmentResult.Assignments).Count -gt 0) {
            foreach ($assignment in $assignmentResult.Assignments) {
                Add-SelectedBatchForSelection `
                    -Selected $selected `
                    -Reasons $reasons `
                    -BatchNames $BatchNames `
                    -Path $path `
                    -BatchId $assignment.BatchId `
                    -ClassFqn $assignment.ClassFqn `
                    -Dll $assignment.Dll `
                    -Reason 'класс теста прочитан из C#-дерева и назначен по первому совпадению текущего списка групп'
            }

            continue
        }

        [void]$manual.Add([pscustomobject]@{
            Path = $path
            Reason = $assignmentResult.ManualReason
        })
    }

    $batchIds = @(
        foreach ($key in $selected.Keys) {
            [string]$key
        }
    ) | Sort-Object
    $focusedManifests = @(
        foreach ($manifest in $focused) {
            [string]$manifest
        }
    ) | Sort-Object
    $reasonRows = @(
        foreach ($reason in $reasons) {
            $reason
        }
    )
    $manualRows = @(
        foreach ($entry in $manual) {
            $entry
        }
    )
    return [pscustomobject]@{
        ChangedPathCount = $Paths.Count
        BatchIds = $batchIds
        OnlyArgument = ($batchIds -join ',')
        FocusedManifests = $focusedManifests
        Reasons = $reasonRows
        ManualPaths = $manualRows
    }
}

function Assert-NoManualPathsForAutomaticOutput {
    param([object]$Selection)

    if (@($Selection.ManualPaths).Count -eq 0) {
        return
    }

    $paths = @($Selection.ManualPaths | ForEach-Object { $_.Path }) -join ', '
    # Если хотя бы один путь не имеет безопасной карты, частичный -Only скроет
    # недобор покрытия. Человек должен явно назвать batch или focused manifest.
    throw "Автоматический выбор тестовых групп неполон. Нужен ручной -Only или короткий обязательный список (focused manifest) для: $paths"
}

function Invoke-SelfCheck {
    param(
        [string]$RepoRootPath,
        [string]$Snapshot,
        [string]$Batches
    )

    # SnapshotPath остаётся параметром для совместимости старых команд.
    # Живой владелец тестов теперь выводится из C#-исходника и текущего списка групп.
    [void]$Snapshot
    $batchRows = @(Read-BatchRowsForSelection -Path $Batches)
    $names = Convert-BatchRowsToNames -Rows $batchRows

    function Assert-OnlyArgument {
        param(
            [string]$Name,
            [object]$Selection,
            [string]$Expected
        )

        if ($Selection.OnlyArgument -ne $Expected) {
            throw "$Name выбрал '$($Selection.OnlyArgument)' вместо $Expected"
        }
    }

    $qwen = New-SelectionResult `
        -Paths @('Tests/DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Qwen canary path' -Selection $qwen -Expected 'design-conversion-remainder-a-m'

    $registry = New-SelectionResult `
        -Paths @('Tests/RetainedDomainModelRegistryTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Retained registry path' -Selection $registry -Expected 'retained-domain-model-registry'

    $pairWriter = New-SelectionResult `
        -Paths @('Tests/ResultGapPhase07DpoWriterTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Result-gap pair writer path' -Selection $pairWriter -Expected 'result-gap-training-pair-writer'

    $graph = New-SelectionResult `
        -Paths @('Tests/SingleCombinationGraphCandidateRunnerTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Single combination graph path' -Selection $graph -Expected 'single-combination-graph-runner'

    $linkedActivation = New-SelectionResult `
        -Paths @('Tests/DesignConversionModelActivationRegistryTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Linked model activation source path' -Selection $linkedActivation -Expected 'design-conversion-remainder-a-m,model-activation'

    $infra = New-SelectionResult `
        -Paths @('tools/test-batches/batches.tsv') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'Batch infrastructure path' -Selection $infra -Expected 'test-infrastructure-evidence-measurement'

    if (-not $infra.FocusedManifests.Contains('tools/test-batches/test-suite-batch-inventory-guard-focused.tsv')) {
        throw 'Batch infrastructure path did not request the focused inventory guard manifest.'
    }

    $wpf = New-SelectionResult `
        -Paths @('Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    Assert-OnlyArgument -Name 'WPF test path' -Selection $wpf -Expected 'api-dashboard-wpf-windows'

    $manual = New-SelectionResult `
        -Paths @('docs/TESTING.md') `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    if (@($manual.BatchIds).Count -ne 0 -or @($manual.ManualPaths).Count -ne 1) {
        throw 'Документальная правка не должна выдумывать продуктовую тестовую группу.'
    }

    $mixed = New-SelectionResult `
        -Paths @(
            'Tests/DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs',
            'docs/TESTING.md'
        ) `
        -BatchRows $batchRows `
        -BatchNames $names `
        -RepoRootPath $RepoRootPath
    $mixedFailedClosed = $false
    try {
        Assert-NoManualPathsForAutomaticOutput -Selection $mixed
    }
    catch {
        if ($_.Exception.Message.Contains('docs/TESTING.md')) {
            $mixedFailedClosed = $true
        }
        else {
            throw
        }
    }

    if (-not $mixedFailedClosed) {
        throw 'Смешанный набор известных и ручных путей должен отказать, а не возвращать частичный -Only.'
    }

    $removedInfrastructureFailed = $false
    try {
        $withoutInfraRows = @($batchRows | Where-Object { $_.BatchId -ne 'test-infrastructure-evidence-measurement' })
        $withoutInfraNames = Convert-BatchRowsToNames -Rows $withoutInfraRows
        [void](New-SelectionResult `
            -Paths @('tools/test-batches/batches.tsv') `
            -BatchRows $withoutInfraRows `
            -BatchNames $withoutInfraNames `
            -RepoRootPath $RepoRootPath)
    }
    catch {
        if ($_.Exception.Message.Contains("Обязательная группа 'test-infrastructure-evidence-measurement'")) {
            $removedInfrastructureFailed = $true
        }
        else {
            throw
        }
    }

    if (-not $removedInfrastructureFailed) {
        throw 'Инфраструктурный выбор должен отказать, если его canonical guard batch отсутствует.'
    }

    $syntheticRoot = Join-Path (Join-Path $RepoRootPath 'TestResults') ('select-impacted-selfcheck-' + [Guid]::NewGuid().ToString('N'))
    $syntheticTests = Join-Path $syntheticRoot 'Tests'
    New-Item -ItemType Directory -Force -Path $syntheticTests | Out-Null
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>Fake.Tests</AssemblyName>
  </PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $syntheticTests 'Fake.Tests.csproj') -Encoding UTF8
    @'
using Xunit;
using Spec = LocalOnlyCLAttribute;

namespace Example
{
    namespace Inner
    {
        public sealed class AlphaTests
        {
            [Fact]
            public void Alpha() { }
        }

        public sealed class BetaTests
        {
            [LocalOnlyCL]
            public void Beta() { }
        }

        public sealed class GammaTests
        {
            [Spec]
            public void Gamma() { }
        }

        public sealed class StringFixtureHolder
        {
            public string Text => "public class StringOnlyTests { [Fact] public void Fake() { } }";
        }
    }
}

file sealed class LocalOnlyCLAttribute : FactAttribute { }
'@ | Set-Content -LiteralPath (Join-Path $syntheticTests 'MultiTests.cs') -Encoding UTF8
    @'
using Xunit;

namespace Example.Inner
{
    public sealed class AlphaTestsLookalike
    {
        [Fact]
        public void ShouldNotUseAlphaBoundary() { }
    }
}
'@ | Set-Content -LiteralPath (Join-Path $syntheticTests 'LookalikeTests.cs') -Encoding UTF8
    @'
namespace Example.Inner
{
    public sealed class UnsupportedAttributeTests
    {
        [UnknownTheoryCL]
        public void MaybeTheory() { }
    }
}
'@ | Set-Content -LiteralPath (Join-Path $syntheticTests 'UnsupportedTests.cs') -Encoding UTF8
    @'
using Xunit;

namespace Example.Inner
{
    public sealed class VisibleTests
    {
        [Fact]
        public void Visible() { }
    }

    public sealed class MaybeExternalTests
    {
        [CustomCheck]
        public void HiddenIfCustomCheckIsExternalTestAttribute() { }
    }
}
'@ | Set-Content -LiteralPath (Join-Path $syntheticTests 'AmbiguousExternalTests.cs') -Encoding UTF8

    $syntheticRows = @(
        [pscustomobject]@{ BatchId = 'alpha'; HumanName = 'Alpha'; Dll = 'Tests/bin/Release/net9.0/Fake.Tests.dll'; Filter = 'FullyQualifiedName~Example.Inner.AlphaTests.' },
        [pscustomobject]@{ BatchId = 'beta'; HumanName = 'Beta'; Dll = 'Tests/bin/Release/net9.0/Fake.Tests.dll'; Filter = 'FullyQualifiedName~Example.Inner.BetaTests.' },
        [pscustomobject]@{ BatchId = 'gamma'; HumanName = 'Gamma'; Dll = 'Tests/bin/Release/net9.0/Fake.Tests.dll'; Filter = 'FullyQualifiedName~Example.Inner.GammaTests.' },
        [pscustomobject]@{ BatchId = 'string-only'; HumanName = 'String-only'; Dll = 'Tests/bin/Release/net9.0/Fake.Tests.dll'; Filter = 'FullyQualifiedName~StringOnlyTests.' },
        [pscustomobject]@{ BatchId = 'fallback'; HumanName = 'Fallback'; Dll = 'Tests/bin/Release/net9.0/Fake.Tests.dll'; Filter = '*' }
    )
    $syntheticNames = Convert-BatchRowsToNames -Rows $syntheticRows
    $syntheticMulti = New-SelectionResult `
        -Paths @('Tests/MultiTests.cs') `
        -BatchRows $syntheticRows `
        -BatchNames $syntheticNames `
        -RepoRootPath $syntheticRoot
    Assert-OnlyArgument -Name 'Synthetic multi-class path' -Selection $syntheticMulti -Expected 'alpha,beta,gamma'

    $syntheticLookalike = New-SelectionResult `
        -Paths @('Tests/LookalikeTests.cs') `
        -BatchRows $syntheticRows `
        -BatchNames $syntheticNames `
        -RepoRootPath $syntheticRoot
    Assert-OnlyArgument -Name 'Synthetic class-boundary lookalike path' -Selection $syntheticLookalike -Expected 'fallback'

    $syntheticUnsupported = New-SelectionResult `
        -Paths @('Tests/UnsupportedTests.cs') `
        -BatchRows $syntheticRows `
        -BatchNames $syntheticNames `
        -RepoRootPath $syntheticRoot
    if (@($syntheticUnsupported.ManualPaths).Count -ne 1 -or -not $syntheticUnsupported.ManualPaths[0].Reason.Contains('Неподдержанный xUnit-похожий атрибут')) {
        throw 'Неподдержанный xUnit-похожий атрибут должен требовать ручной выбор вместо частичного batch.'
    }

    $syntheticAmbiguous = New-SelectionResult `
        -Paths @('Tests/AmbiguousExternalTests.cs') `
        -BatchRows $syntheticRows `
        -BatchNames $syntheticNames `
        -RepoRootPath $syntheticRoot
    if (@($syntheticAmbiguous.ManualPaths).Count -ne 1 -or -not $syntheticAmbiguous.ManualPaths[0].Reason.Contains('Неразрешённые атрибуты методов')) {
        throw 'Файл с видимым тестом и неразрешённым атрибутом метода должен требовать ручной выбор.'
    }

    $caseDuplicatePaths = @(Read-ChangedPaths `
        -RepoRootPath $RepoRootPath `
        -BaseRef '' `
        -TargetRef 'HEAD' `
        -ExplicitPaths @(
            'tests/designconversionlocalindependentqwenruntimecanarytests.cs',
            'Tests/DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs'
        ) `
        -PathsFile '')
    if ($caseDuplicatePaths.Count -ne 1) {
        throw 'Нормализация путей должна схлопывать один repo-relative путь без учёта регистра.'
    }

    $outsidePathRejected = $false
    try {
        [void](Read-ChangedPaths `
            -RepoRootPath $RepoRootPath `
            -BaseRef '' `
            -TargetRef 'HEAD' `
            -ExplicitPaths @('../Tests/DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs') `
            -PathsFile '')
    }
    catch {
        if ($_.Exception.Message.Contains('вне репозитория')) {
            $outsidePathRejected = $true
        }
        else {
            throw
        }
    }

    if (-not $outsidePathRejected) {
        throw 'Нормализация путей должна отвергать пути вне репозитория.'
    }

    'SELECT_IMPACTED_BATCHES_SELF_CHECK_OK'
}

if ([string]::IsNullOrWhiteSpace($SnapshotPath)) {
    $SnapshotPath = Join-Path $RepoRoot 'Tests/TestSuiteBatchLegacySnapshot.txt'
}

if ([string]::IsNullOrWhiteSpace($BatchesFile)) {
    $BatchesFile = Join-Path $RepoRoot 'tools/test-batches/batches.tsv'
}

if ($SelfCheck) {
    Invoke-SelfCheck -RepoRootPath $RepoRoot -Snapshot $SnapshotPath -Batches $BatchesFile
    exit 0
}

$paths = @(Read-ChangedPaths `
    -RepoRootPath $RepoRoot `
    -BaseRef $Since `
    -TargetRef $To `
    -ExplicitPaths $ChangedPath `
    -PathsFile $ChangedPathsFile)

if ($paths.Count -eq 0) {
    throw 'Изменённые пути не переданы. Используйте -Since, -ChangedPath или -ChangedPathsFile.'
}

$batchRows = @(Read-BatchRowsForSelection -Path $BatchesFile)
$result = New-SelectionResult `
    -Paths $paths `
    -BatchRows $batchRows `
    -BatchNames (Convert-BatchRowsToNames -Rows $batchRows) `
    -RepoRootPath $RepoRoot

switch ($OutputMode) {
    'OnlyArgument' {
        Assert-NoManualPathsForAutomaticOutput -Selection $result
        if ([string]::IsNullOrWhiteSpace($result.OnlyArgument)) {
            throw 'Для изменённых путей не удалось автоматически выбрать основную группу.'
        }

        $result.OnlyArgument
    }
    'Json' {
        $result | ConvertTo-Json -Depth 6
    }
    default {
        "Для первого короткого запуска выбрано основных групп: $(@($result.BatchIds).Count)."
        foreach ($reason in $result.Reasons) {
            "- $($reason.Path): $($reason.HumanName) ($($reason.BatchId)); причина: $($reason.Reason)."
        }

        if (@($result.BatchIds).Count -gt 0) {
            "Аргумент запускалки: -Only `"$($result.OnlyArgument)`""
        }

        foreach ($manifest in $result.FocusedManifests) {
            "- Также явно запустите короткий обязательный список (focused manifest): $manifest."
        }

        foreach ($manualPath in $result.ManualPaths) {
            "- Для $($manualPath.Path) нужен ручной выбор: $($manualPath.Reason)."
        }
    }
}
