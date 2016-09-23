$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Deploy Mod"


$src_path_raw = $PSScriptRoot
$src_path = Get-Item -Path $src_path_raw -ErrorAction SilentlyContinue
if(-not $src_path)
{
    $wshell.Popup("Could not find the Source directory in:- $src_path_raw", 0, $title, 0x0)
    exit 1
}

$mod_name = $src_path.BaseName;

$mod_path_raw = [String]::Concat([Environment]::GetFolderPath('ApplicationData'), "\SpaceEngineers\mods")
$mod_path = Get-Item -Path $mod_path_raw -ErrorAction SilentlyContinue
if(-not $mod_path)
{
    $wshell.Popup("Could not find the Space Engineers mod directory in:- $mod_path_raw", 0, $title, 0x0)
    exit 1
}
New-Item -Path $mod_path -Name $mod_name -ItemType Directory -ErrorAction SilentlyContinue
$mod_path_raw += "\$mod_name";
$mod_path = Get-Item -Path $mod_path_raw -ErrorAction SilentlyContinue
if(-not $mod_path)
{
    $wshell.Popup("Could not find the deploy directory in:- $mod_path_raw", 0, $title, 0x0)
    exit 1
}

Try{
    $dir = [String]::Concat($src_path, "\Data")
	if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Data" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($src_path, "\Data\*")
		$dst = [String]::Concat($mod_path, "\Data\")
	    Copy-item -Exclude "backup" -Path $src -Destination $dst
	}
    $dir = [String]::Concat($src_path, "\Models")
	if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Models" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($src_path, "\Models\*.mwm")
		$dst = [String]::Concat($mod_path, "\Models\")
		Copy-item -Exclude "backup" -Path $src -Destination $dst
	}
	$dir = [String]::Concat($src_path, "\Textures")
		if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Textures" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($src_path, "\Textures\*")
		$dst = [String]::Concat($mod_path, "\Textures\")
		Copy-item -Exclude "backup" -Path $src -Destination $dst
	}
	$src = [String]::Concat($src_path, "\modinfo.sbmi")
	Copy-item -Exclude "backup" -Path $src -Destination $mod_path
	$src = [String]::Concat($src_path, "\thumb.jpg")
	Copy-item -Exclude "backup" -Path $src -Destination $mod_path
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Copy Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}