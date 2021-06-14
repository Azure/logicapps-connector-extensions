$extensionPath=$args[0]
$extensionName = "FTP"
$extensionNameServiceProvider = $extensionName+"ServiceProvider"

$userprofile = [environment]::GetFolderPath('USERPROFILE')
$dll = $extensionPath+"\netcoreapp3.1\Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.dll"

$extensionModulePath = Join-Path -Path $userprofile -ChildPath ".azure-functions-core-tools\Functions\ExtensionBundles\Microsoft.Azure.Functions.ExtensionBundle.Workflows\1.1.16\bin\extensions.json"

$fullAssemlyName = [System.Reflection.AssemblyName]::GetAssemblyName($dll).FullName
write-host "Full assembly name " + $fullAssemlyName
try
{
# Kill all the existing func.exe else modeule extension cannot be modified. 
taskkill /IM "func.exe" /F
}
catch
{
  write-host "func.exe not found"
}

dotnet add package "Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP" --version 0.10.8  --source $extensionPath

write-host 'Full assembly '+ $fullAssemlyName
$typeFullName =  "Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.FTPStartup, $fullAssemlyName"

$newNode =  [pscustomobject] @{ 
  name = $extensionNameServiceProvider
  typeName = $typeFullName}


#  1. Update extensions.json under extension module
$a = Get-Content $extensionModulePath -raw | ConvertFrom-Json
if ( ![bool]($a.extensions.name -match $extensionNameServiceProvider))
{
$a.extensions += $newNode

$a | ConvertTo-Json -depth 32| set-content $extensionModulePath

}

$spl = Split-Path $extensionModulePath
Copy-Item $dll  -Destination $spl


$allextensionjson = Get-ChildItem -Path (Get-Item bin).FullName   -Filter extensions.json -Recurse -ErrorAction SilentlyContinue -Force
 write-host "updating the extension folder $allextensionjson"
foreach ($ext in $allextensionjson)
{
    $extFolder = Split-Path $ext
    write-host "updating the extension file " + $ext.FullName
    $aext = Get-Content $ext.FullName -raw | ConvertFrom-Json
    if ( ![bool]($aext.extensions.name -match $extensionNameServiceProvider))
    {
        $aext.extensions += $newNode
        $aext | ConvertTo-Json -depth 32| set-content $ext.FullName
    }

    Copy-Item $dll  -Destination $extFolder
}

Write-host "Successfully added extension.. "