Param (
	[parameter(Mandatory=$true, Position=0, ParameterSetName="Project")]
	[String]
	$project,
	[parameter(ParameterSetName="Use-Overlays")]
	[Bool]
	$use_overlays=$false,
	[parameter(ParameterSetName="Use-Extra-Overlays")]
	[Bool]
	$use_extra_overlays=$false
)
$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Generate Icons v1.4"

#Setup
$errorAction = "SilentlyContinue"

$project_path = Get-Item -Path $project
if(-not $project_path)
{
    $wshell.Popup("Could not find the Project '$project'", 0, $title, 0x0)
    exit 1
}

$imagemagick = Get-ChildItem -Path $env:ProgramW6432 ImageMagick*.*.*
$im_convert = Get-ChildItem -Path $imagemagick.FullName convert.exe

if(-not $im_convert)
{
    $wshell.Popup("Could not find the imagemagick convert.exe", 0, $title, 0x0)
    exit 1
}

$im_composite = Get-ChildItem -Path $imagemagick.FullName composite.exe

if(-not $im_composite)
{
    $wshell.Popup("Could not find the imagemagick composite.exe", 0, $title, 0x0)
    exit 1
}

$texconv_raw = "C:\users\mafoo\bin\texconv.exe"
$texconv = Get-Item -Path $texconv_raw
if(-not $texconv)
{
    $wshell.Popup("Could not find the texconv in '$texconv_raw'", 0, $title, 0x0)
    exit 1
}


$asset_path  = Get-ChildItem -Path $project_path.Parent.FullName Assets
if(-not $asset_path)
{
    $wshell.Popup("Could not find the Assets directory at:- " + $project_path.Parent.FullName, 0, $title, 0x0)
    exit 1
}
$asset_src_path  = Get-ChildItem -Path $asset_path.FullName Images
if(-not $asset_src_path)
{
    $wshell.Popup("Could not find the Assets/Images directory at:- " + $asset_path.FullName, 0, $title, 0x0)
    exit 1
}

$src_path = Get-ChildItem -Path $project_path.FullName Sources
if(-not $src_path)
{
    $wshell.Popup("Could not find the Sources directory in:- $src_path_raw", 0, $title, 0x0)
    exit 1
}

New-Item -ItemType Directory -Path $project_path.FullName -Name Textures -ErrorAction SilentlyContinue
$dst_path = Get-ChildItem -Path $project_path.FullName Textures -ErrorAction Stop
New-Item -ItemType Directory -Path $dst_path.FullName -Name Icons -ErrorAction SilentlyContinue
$dst_dir = Get-ChildItem -Path $dst_path.FullName Icons -ErrorAction Stop


$small_overlay = $asset_src_path.FullName + "\small_overlay.png"
$small_overlay = Get-Item -Path $small_overlay

$regular_overlay = $asset_src_path.FullName + "\regular_overlay.png"
$regular_overlay = Get-Item -Path $regular_overlay

$inverted_overlay = $asset_src_path.FullName + "\inverted_overlay.png"
$inverted_overlay = Get-Item -Path $inverted_overlay

$errorAction = "Stop"

Try{
	$tmp_dir = [System.IO.Path]::GetTempFileName()
	$null = Remove-Item -Path $tmp_dir -Force
	$tmp_dir = New-Item -Path $tmp_dir -ItemType Directory
	"Going to use this working Dir: " + $tmp_dir

    foreach ($icon in Get-ChildItem -Path $src_path.FullName *.png){
		if($icon.BaseName -notlike '*_overlay'){
	        $dst_file = $icon.BaseName
			$dst_file = $tmp_dir.FullName + "\" + ($dst_file -replace "(.*)\d+",'$1') + ".png"
			Copy-Item -Path $icon.FullName -Destination $dst_file

			#& $im_convert.FullName $icon.FullName -background none -colorspace gray -sigmoidal-contrast 10,50% -fill blue -tint 50 $dst_file
			#& $im_convert.FullName $icon.FullName -alpha off -fill blue -colorize 10% -alpha on $dst_file
			
			if($LastExitCode){
			  throw "Failed to execute "+$im_convert.FullName.ToLower()+" "+$icon.FullName+" -fill blue -tint 100 $dst_file";
			}


			if($use_extra_overlays -and $icon.BaseName -like "SteelCatwalk*" -and $icon.BaseName -notlike "*Only*"){
				if($icon.BaseName -like "*Inv*"){
					& $im_composite.FullName $inverted_overlay.FullName $icon.FullName $dst_file
				}else{
					& $im_composite.FullName $regular_overlay.FullName $icon.FullName $dst_file
				}
			}
			if($use_overlays -and $icon.BaseName -like "SteelCatwalk*" -and $icon.BaseName -notlike "*Suspended*"){
				$dst_small_overlay_file = $tmp_dir.FullName + "\small_" + ($icon.BaseName -replace "(.*)\d+",'$1') + ".png"
				& $im_composite.FullName $small_overlay.FullName $dst_file $dst_small_overlay_file
			}
		}
	}
	$icon_src = $tmp_dir.FullName + "\*.png"
	& $texconv.FullName -y -f BC3_UNORM -ft dds -o $dst_dir.FullName $icon_src
	if($LastExitCode){
		throw "Failed to execute "+$texconv.FullName+" -y -f BC3_UNORM -ft dds -o "+$dst_dir.FullName+" $icon_src";
	}
	#Remove-Item -Path $tmp_dir -Force -Recurse
    foreach ($icon in Get-ChildItem -Path $dst_dir.FullName *.DDS){
	    $dst_file = $icon.BaseName + ".dds"
		Rename-Item -Force -Path $icon.FullName -NewName $dst_file
	}
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Convert Failed: $ErrorMessage", 0, $title, 0x0)
	Write-Error $ErrorMessage
    exit 1
}
Write-Output "Icon Generation complete"