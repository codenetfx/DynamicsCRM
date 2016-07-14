###################################################################################################
###
###  build.ps1 -- PowerShell script called in Visual Studio post-build step.
###
###  The post-build step is very simple and looks like this:
###    powershell.exe "$(ProjectDir)\Solution\Build\build.ps1"
param
(
[string]$targetDir,
[string]$targetFile,
[string]$targetName
)
$ErrorActionPreference = "Stop"

try {

$targetDir = $targetDir.Substring(0, $targetDir.length - 3)
$mergeFolderName = "merged"
$mergeFolder = "$targetDir\$mergeFolderName"
$libFolder = $mergeFolder + "\lib"
$buildFolder = Split-Path -path $MyInvocation.MyCommand.Definition


# Clean up the temp folder from possible previous builds
if (Test-Path $mergeFolder)
{
	Remove-Item "$mergeFolder" -Recurse -Force -ErrorAction Stop
}
mkdir $mergeFolder


### Copy all of the output files to the merged folder for ILMerging
Get-ChildItem $targetDir -Recurse -Exclude "$mergeFolderName" | 
	Copy-Item -Destination {Join-Path $mergeFolder $_.FullName.Substring($targetDir.length)}


###  Copy the built plugin assembly and library files to temporary folder
###  SDK dlls that are on the CRM server should be libraries rather than ILMerged in

$toMove = @($targetFile, "$targetName.pdb", "Microsoft.Crm.Sdk.Proxy.dll", "Microsoft.Xrm.Sdk.dll", "Microsoft.Xrm.Sdk.Workflow.dll", "Microsoft.IdentityModel.dll")

$toMove | ForEach-Object {
	$copySource = "$mergeFolder\$_"
	$copyDestination = "$libFolder\$_"
	if (Test-Path $copySource)
	{
		New-Item -ItemType File -Path $copyDestination -Force
		Move-Item $copySource $copyDestination -force -Verbose
	}
}


###  Make sure that any references that need to be ILMerged in are marked as Copy Local
$ilmergeExe = $buildFolder + "\ilmerge.exe"
& $ilmergeExe /wildcards /lib:"$libFolder" /keyfile:"$buildFolder\Key.snk" /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2" /out:"$mergeFolder\$targetFile" "$libFolder\$targetFile"  "$mergeFolder\*.dll"
Write-Host `n


if (Test-Path "_POWERSHELLNOTENABLED.txt")
{
	Remove-Item "_POWERSHELLNOTENABLED.txt"
}

if ($LASTEXITCODE -ne 0) {
	exit $LASTEXITCODE
}

# Error handling
} catch {
	Write-Host `n
	exit 1
}
