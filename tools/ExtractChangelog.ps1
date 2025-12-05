param (
    [string]$tag,
    [string]$changelogPath,
    [string]$extractedChangelogPath
)

# Check if the provided changelog path exists
if (-Not (Test-Path -Path $changelogPath)) {
    Write-Error "The specified CHANGELOG file does not exist at $changelogPath"
    exit
}

# Read the content of the specified changelog
$content = Get-Content -Path $changelogPath -Raw

# Extract version number from tag
if ($tag -match "\d+(\.\d+)*")
{
    $version = $matches[0]
    Write-Host "Extracting chapter from $changelogPath for version $version"
}
else
{
    Write-Error "Could not extract version from tag '$tag'"
    exit
}

# Define the regex pattern to match the desired chapter
# (?..) : Defines modifiers:
#         - m: multi line = ^ and $ match start/end of line
#         - s: single line = dot matches new line
$pattern = "(?ms)^# $version \(\d{4}-\d{2}-\d{2}\)(?:\r\n|\n){2}(.*?)^(?=# )"

# Use regex to extract the matched chapter
if ($content -match $pattern)
{
    # Extract the matched chapter content
    $chapterContent = $matches[1]

    # Create the extracted changelog file (and its directory if needed)
    New-Item -ItemType File -Path $extractedChangelogPath -Force

    # Write the content to the specified extracted changelog path
    Set-Content -Path $extractedChangelogPath -Value $chapterContent

    Write-Host "Chapter for version '$version' extracted to $extractedChangelogPath with content:"
    Get-Content -Path $extractedChangelogPath
}
else
{
    Write-Error "Chapter for version '$version' not found"
    exit
}
