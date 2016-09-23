$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Generate Iconsd"


$imagemagick = Get-ChildItem -Path $env:ProgramW6432 ImageMagick*.*.*
$im_convert = Get-ChildItem -Path $imagemagick.FullName convert.exe

if(-not $im_convert)
{
    $wshell.Popup("Could not find the imagemagick convert.exe", 0, $title, 0x0)
    exit 1
}

$src_path_raw = $PSScriptRoot
$src_path = Get-Item -Path $src_path_raw -ErrorAction SilentlyContinue
if(-not $src_path)
{
    $wshell.Popup("Could not find the Project directory at:- $src_path_raw", 0, $title, 0x0)
    exit 1
}
$dst_path = Get-ChildItem -Path $src_path.FullName Textures
if(-not $dst_path)
{
    $wshell.Popup("Could not find the Textures directory in:- $src_path_raw", 0, $title, 0x0)
    exit 1
}

$src_path = Get-ChildItem -Path $src_path.FullName Sources
if(-not $src_path)
{
    $wshell.Popup("Could not find the Sources directory in:- $src_path_raw", 0, $title, 0x0)
    exit 1
}

Try{
    foreach ($icon in Get-ChildItem -Path $src_path.FullName *.PNG){
        $dst_file = $icon.BaseName
        $dst_file = $dst_path.FullName + "\" + ($dst_file -replace "(.*)\d+",'$1') + ".dds"
        $dst_file
        & $im_convert.FullName -define dds:compression=dxt5 $icon.FullName $dst_file
    }
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Convert Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}