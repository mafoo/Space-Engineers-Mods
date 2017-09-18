$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Build Blender textures v1.1"

#START config options

# set the following to tell the script where to find/put things
# Microsoft DirectTex - can be obtained from - https://github.com/Microsoft/DirectXTex/releases
$texconv_raw = "C:\users\mafoo\bin\texconv.exe"
# space engineers game directory
# (p.s. you can run this script twice for two differnt sources to build both the Dx11 and Dx9 textures)
$game_dir_raw = "C:\Games\Steam\steamapps\common\SpaceEngineers"
# resource directory - where you would like the output to be kept (what you then tell blender to use)
$resource_dir_raw = "D:\Space Engineers Source"

#END config options

$texconv = Get-Item -Path $texconv_raw
if(-not $texconv)
{
    $wshell.Popup("Could not find the texconv in '$texconv_raw'", 0, $title, 0x0)
    exit 1
}

$game_dir = Get-Item -Path $game_dir_raw -ErrorAction SilentlyContinue

if(-not $game_dir)
{
    $wshell.Popup("Could not find the Game directory at:- $game_dir_raw", 0, $title, 0x0)
    exit 1
}

$resource_dir = Get-Item -Path $resource_dir_raw -ErrorAction SilentlyContinue

if(-not $resource_dir)
{
    $wshell.Popup("Could not find the Game directory at:- $resource_dir_raw", 0, $title, 0x0)
    exit 1
}

Try{
	$se_exe_dir = $resource_dir.FullName + "\bin64"
	$se_exe = $resource_dir.FullName + "\bin64\SpaceEngineers.exe"
	$null = New-Item -ItemType Directory -Path $se_exe_dir -ErrorAction SilentlyContinue
	$null = New-Item -ItemType File -Path $se_exe
    foreach ($item in Get-ChildItem -Path $game_dir.FullName -Recurse){
		$rel_path = $item.FullName.Replace($game_dir.FullName, "")
		$dst_path = $resource_dir.FullName + $rel_path
		if($item -is [System.IO.DirectoryInfo]){
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

					& $texconv -y -f BC3_UNORM -ft dds -o $dst_dir $item.FullName
					if($lastExitCode){
						Write-Output "Somethign went wrong with the last convert"
						exit 1
					}
					$name = $dst_dir + "\" + $item.BaseName
					$uc_name = $name +".DDS"
					$lc_name = $item.BaseName +".dds"
					Rename-Item -Path $uc_name -NewName $lc_name -Force
				}
			}
		}
    }
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Convert Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}