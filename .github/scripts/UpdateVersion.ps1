param(
    [string]$version
)

$setup_link = "https://github.com/Ellendar/Z2Randomizer/releases/download/$version/Z2Randomizer-$version-Windows-Installer.msi"
$pub_date = Get-Date -Format "dddd, dd MMMM yyyy"

$file = "appcast.xml"
$content = Get-Content $file
if ($content -like "*<title>Version $version</title>*") {
    Write-Host "Version $version already exists in appcast.xml"
} else {
    $firstMatch = $content | Select-String -Pattern '^\s*<item>' | Select-Object -First 1
    $index = $firstMatch.LineNumber - 1
    $before = $content[0..($index - 1)]
    $after  = $content[$index..($content.Length - 1)]
    $app_cast_item = @"
		<item>
			<title>Version $version</title>
			<description>
				<![CDATA[
					See the <a href="https://github.com/Ellendar/Z2Randomizer/blob/main/PatchNotes.md">changelog</a> for details about new features.
				]]>
			</description>
			<pubDate>$pub_date</pubDate>
			<enclosure url="$setup_link" sparkle:version="$version" />
		</item>
"@
    Set-Content $file ($before + $app_cast_item + $after)
}
git add $file

$file = "Directory.Build.targets"
(Get-Content $file) |
    ForEach-Object {
        $_ -replace '<Version>.*?</Version>', "<Version>$version</Version>"
    } | Set-Content $file
git add $file

$file = "README.md"
(Get-Content $file) |
    ForEach-Object {
        $_ -replace '\[Download\]\([^)]*\)', "[Download]($setup_link)"
    } | Set-Content $file
git add $file

$file = "Setup1/Setup1.vdproj"
(Get-Content $file) |
    ForEach-Object {
        $_ -replace '"ProductVersion"\s*=\s*"8:.*?"', "`"ProductVersion`" = `"8:$version`""
    } | Set-Content $file
git add $file

# Only commit if some change was actually staged (to prevent setting error level)
if (git diff --cached --name-only) {
    git commit -m "$version Release"
}
