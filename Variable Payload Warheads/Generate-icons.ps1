$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Generate Icons v1.3"

$imagemagick = Get-ChildItem -Path $env:ProgramW6432 ImageMagick*.*.*
$im_convert = Get-ChildItem -Path $imagemagick.FullName convert.exe

$im_composite = Get-ChildItem -Path $imagemagick.FullName composite.exe


$texconv_raw = "C:\users\mafoo\bin\texconv.exe"
$texconv = Get-Item -Path $texconv_raw
if(-not $texconv)
{
    $wshell.Popup("Could not find the texconv in '$texconv_raw'", 0, $title, 0x0)
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

New-Item -ItemType Directory -Path $dst_path -Name Icons -ErrorAction SilentlyContinue
$dst_dir_raw = $dst_path.FullName + "\Icons"
$dst_dir = Get-Item -Path $dst_dir_raw -ErrorAction SilentlyContinue
New-Item -Path $dst_dir_raw -ItemType Directory -ErrorAction SilentlyContinue

$small_overlay = $src_path.FullName + "\small_overlay.png"
$small_overlay = Get-Item -Path $small_overlay -ErrorAction SilentlyContinue

$regular_overlay = $src_path.FullName + "\regular_overlay.png"
$regular_overlay = Get-Item -Path $regular_overlay -ErrorAction SilentlyContinue

$inverted_overlay = $src_path.FullName + "\inverted_overlay.png"
$inverted_overlay = Get-Item -Path $inverted_overlay -ErrorAction SilentlyContinue

if($regular_overlay -and $inverted_overlay){
	if(-not $im_convert)
	{
		$wshell.Popup("Could not find the imagemagick convert.exe", 0, $title, 0x0)
		exit 1
	}
	if(-not $im_composite)
	{
		$wshell.Popup("Could not find the imagemagick composite.exe", 0, $title, 0x0)
		exit 1
	}
	Write-Output "Using extra overlays"
	$use_extra_overlays = $true
}

$errorAction = "Stop"

Try{
	$tmp_dir = [System.IO.Path]::GetTempFileName()
	$null = Remove-Item -Path $tmp_dir -Force
	$tmp_dir = New-Item -Path $tmp_dir -ItemType Directory
	"Going to use this working Dir: " + $tmp_dir

    foreach ($icon in Get-ChildItem -Path $src_path.FullName *.PNG){
		if($icon.BaseName -notlike '*_overlay'){
	        $dst_file = $icon.BaseName
			$dst_file = $tmp_dir.FullName + "\" + ($dst_file -replace "(.*)\d+",'$1') + ".png"
			Copy-Item -Path $icon.FullName -Destination $dst_file

			if($use_extra_overlays -and $icon.BaseName -like "SteelCatwalk*" -and $icon.BaseName -notlike "*Only*"){
				if($icon.BaseName -like "*Inv*"){
					& $im_composite.FullName $inverted_overlay.FullName $icon.FullName $dst_file
				}else{
					& $im_composite.FullName $regular_overlay.FullName $icon.FullName $dst_file
				}
			}
			if($small_overlay -and $icon.BaseName -like "SteelCatwalk*" -and $icon.BaseName -notlike "*Suspended*"){
				$dst_small_overlay_file = $tmp_dir.FullName + "\small_" + ($icon.BaseName -replace "(.*)\d+",'$1') + ".png"
				& $im_composite.FullName $small_overlay.FullName $dst_file $dst_small_overlay_file
			}
		}
	}
	$icon_src = $tmp_dir.FullName + "\*.png"
	& $texconv.FullName -y -f BC3_UNORM -ft dds -o $dst_dir.FullName $icon_src
	Remove-Item -Path $tmp_dir -Force -Recurse
    foreach ($icon in Get-ChildItem -Path $dst_dir.FullName *.DDS){
	    $dst_file = $icon.BaseName + ".dds"
		Rename-Item -Force -Path $icon.FullName -NewName $dst_file
	}
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Convert Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}