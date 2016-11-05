$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Build Blender textures v1.0"


$imagemagick = Get-ChildItem -Path $env:ProgramW6432 ImageMagick*.*.*
$im_convert = Get-ChildItem -Path $imagemagick.FullName convert.exe
if(-not $im_convert)
{
    $wshell.Popup("Could not find the imagemagick convert.exe", 0, $title, 0x0)
    exit 1
}

$texconv_raw = "C:\users\mafoo\bin\texconv.exe"
$texconv = Get-Item -Path $texconv_raw
if(-not $texconv)
{
    $wshell.Popup("Could not find the texconv in '$texconv_raw'", 0, $title, 0x0)
    exit 1
}

$game_dir_raw = "C:\Games\Steam\steamapps\common\SpaceEngineers"
$game_dir = Get-Item -Path $game_dir_raw -ErrorAction SilentlyContinue

if(-not $game_dir)
{
    $wshell.Popup("Could not find the Game directory at:- $game_dir_raw", 0, $title, 0x0)
    exit 1
}

$resource_dir_raw = "D:\Space Engineers Source"
$resource_dir = Get-Item -Path $resource_dir_raw -ErrorAction SilentlyContinue

if(-not $resource_dir)
{
    $wshell.Popup("Could not find the Game directory at:- $resource_dir_raw", 0, $title, 0x0)
    exit 1
}

Try{
    foreach ($item in Get-ChildItem -Path $game_dir.FullName -Recurse){
		$rel_path = $item.FullName.Replace($game_dir.FullName, "")
		$dst_path = $resource_dir.FullName + $rel_path
		if($item -is [System.IO.DirectoryInfo]){
			#$rel_path
			if($rel_path -like "\Content\Textures\*"){
				"Processing " + $dst_path
				$null = New-Item -Path $dst_path -ItemType Directory -ErrorAction SilentlyContinue
			}
			elseif($rel_path -like "\Content"){
				"Processing " + $dst_path
				$null = New-Item -Path $dst_path -ItemType Directory -ErrorAction SilentlyContinue
			}
		}else{
			if($rel_path -like "\Content\Textures\*"){
				if($item -like "*.dds"){
					$rel_dir = $item.Directory.FullName.Replace($game_dir.FullName, "")
					$dst_dir = $resource_dir.FullName + $rel_dir
					"Loc: "+ $item.FullName +", Rel: " + $rel_dir + ", " + $dst_dir

					& $texconv -f BC3_UNORM -ft dds -o $dst_dir $item.FullName
					#(& $im_convert.FullName -define dds:compression=DXT5 $item.FullName $dst_path) 2> $null
					if($lastExitCode){
					exit 1
					#	"Falling back to copy for $rel_path"
					#	Copy-Item -Path $item -Destination $dst_path
					}
				}
			}
		}
        #$dst_file = $icon.BaseName
        #$dst_file = $dst_path.FullName + "\Icons\" + ($dst_file -replace "(.*)\d+",'$1') + ".dds"
        #$dst_file
        #& $im_convert.FullName -define dds:compression=dxt5 $icon.FullName $dst_file
    }
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Convert Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}