# Script to convert archetype_seed.json coordinates from normalized (0-1) to pixel (0-800 x, 0-400 y)
# and merge with existing seeddata.json

$archetypeFile = Join-Path $PSScriptRoot "..\Data\seed\archetype_seed.json"
$seedFile = Join-Path $PSScriptRoot "..\Data\seed\seeddata.json"

$archData = Get-Content $archetypeFile | ConvertFrom-Json
$seedData = Get-Content $seedFile | ConvertFrom-Json

# Convert coordinates from normalized (0-1) to pixels (0-800 x, 0-400 y)
$WIDTH = 800
$HEIGHT = 400

# Convert archetypes
$convertedArchetypes = @()
foreach ($arch in $archData.archetypes) {
    # Convert normalized SVG path to pixel coordinates
    $pixelPath = $arch.svgPath -replace '0\.(\d+)', { 
        $normalized = "0.$($_.Groups[1].Value)"
        [int]($normalized * $WIDTH)
    }
    
    $convertedArchetypes += @{
        Id = $arch.id
        Name = $arch.name
        Description = $arch.description
        SvgPath = $pixelPath
    }
}

# Convert archetype points
$convertedPoints = @()
$pointId = 1
foreach ($point in $archData.archetypePoints) {
    $convertedPoints += @{
        Id = $pointId++
        ArchetypeId = $point.archetypeId
        StepId = $point.stepId
        Label = $point.label
        Prompt = $point.prompt
        X = [math]::Round($point.x * $WIDTH, 1)
        Y = [math]::Round($point.y * $HEIGHT, 1)
        Align = $point.align
    }
}

# Convert examples
$convertedExamples = @()
$exampleId = 1
foreach ($example in $archData.archetypeExamples) {
    $convertedExamples += @{
        Id = $exampleId++
        ArchetypePointId = $example.archetypePointId
        Title = $example.title
        Content = $example.content
    }
}

# Replace in seed data
$seedData.Archetypes = $convertedArchetypes
$seedData.ArchetypePoints = $convertedPoints
$seedData.ArchetypeExamples = $convertedExamples

# Write back to seeddata.json
$seedData | ConvertTo-Json -Depth 10 | Set-Content $seedFile

Write-Host "✅ Successfully converted and merged archetype seed data!" -ForegroundColor Green
Write-Host "📊 Archetypes: $($convertedArchetypes.Count)" -ForegroundColor Cyan
Write-Host "📍 Points: $($convertedPoints.Count)" -ForegroundColor Cyan
Write-Host "📖 Examples: $($convertedExamples.Count)" -ForegroundColor Cyan
