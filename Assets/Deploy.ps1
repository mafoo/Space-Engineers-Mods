Param (
	[parameter(Mandatory=$true, Position=0, ParameterSetName="Project")]
	[String]
	$project
)
$wshell = New-Object -ComObject Wscript.Shell
$title = "Space Engineers Deploy Mod v1.2"

$project_path = Get-Item -Path $project
if(-not $project_path)
{
    $wshell.Popup("Could not find the Project '$project'", 0, $title, 0x0)
    exit 1
}

$mod_name = $project_path.BaseName;

$mod_path_raw = [String]::Concat([Environment]::GetFolderPath('ApplicationData'), "\SpaceEngineers\mods")
$mod_path = Get-Item -Path $mod_path_raw -ErrorAction SilentlyContinue
if(-not $mod_path)
{
    $wshell.Popup("Could not find the Space Engineers mod directory in:- $mod_path_raw", 0, $title, 0x0)
    exit 1
}
$null = New-Item -Path $mod_path -Name $mod_name -ItemType Directory -ErrorAction SilentlyContinue
$mod_path_raw += "\$mod_name";
$mod_path = Get-Item -Path $mod_path_raw -ErrorAction SilentlyContinue
if(-not $mod_path)
{
    $wshell.Popup("Could not find the deploy directory in:- $mod_path_raw", 0, $title, 0x0)
    exit 1
}

Try{
    $dir = [String]::Concat($project_path, "\Data")
	if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Data" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($project_path, "\Data\*")
		$dst = [String]::Concat($mod_path, "\Data\")
	    Copy-item -Path $src -Destination $dst
	}
    $dir = [String]::Concat($project_path, "\Models")
	if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Models" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($project_path, "\Models\*.mwm")
		$dst = [String]::Concat($mod_path, "\Models\")
		Copy-item -Path $src -Destination $dst
	}
	$dir = [String]::Concat($project_path, "\Textures")
		if(Get-Item -Path $dir -ErrorAction SilentlyContinue){
		New-Item -Path $mod_path -Name "Textures" -ItemType Directory -ErrorAction SilentlyContinue
		$src = [String]::Concat($project_path, "\Textures\*")
		$dst = [String]::Concat($mod_path, "\Textures\")
		Copy-item -Path $src -Destination $dst -Recurse -Force
	}
	$src = [String]::Concat($project_path, "\modinfo.sbmi")
	Copy-item -Exclude "Backup" -Path $src -Destination $mod_path

	$src = [String]::Concat($project_path, "\thumb.jpg")
	if(Get-Item -Path $src -ErrorAction SilentlyContinue) {
		Copy-item -Path $src -Destination $mod_path
	}
	$src = [String]::Concat($project_path, "\thumb.png")
	if(Get-Item -Path $src -ErrorAction SilentlyContinue) {
		Copy-item -Path $src -Destination $mod_path
	}
}
Catch
{
    $ErrorMessage = $_.Exception.Message
    $wshell.Popup("Copy Failed: $ErrorMessage", 0, $title, 0x0)
    exit 1
}
Write-Output "Deployment complete"