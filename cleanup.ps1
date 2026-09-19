[CmdletBinding(SupportsShouldProcess)]
param()

$projectRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$gitDirectory = Join-Path $projectRoot '.git'

$targets = Get-ChildItem -LiteralPath $projectRoot -Directory -Recurse -Force |
    Where-Object {
        $_.Name -in @('bin', 'obj') -and
        -not $_.FullName.StartsWith($gitDirectory, [System.StringComparison]::OrdinalIgnoreCase)
    }

if ($targets.Count -eq 0) {
    Write-Host 'No bin or obj directories found.'
    return
}

foreach ($target in $targets) {
    if ($PSCmdlet.ShouldProcess($target.FullName, 'Remove build output directory')) {
        Remove-Item -LiteralPath $target.FullName -Recurse -Force
        Write-Host "Removed $($target.FullName)"
    }
}
