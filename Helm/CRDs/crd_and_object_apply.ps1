kubectl apply -f .\healthcheck.yaml


$parentFolder = Resolve-Path  .\objects\healthChecks\
# Set-Location .\objects\healthChecks\
$folders = Get-ChildItem -Path $parentFolder -Directory -Force -ErrorAction SilentlyContinue | Select-Object Name

foreach ($folder in $folders) {
    $folderloc = Join-Path -Path $parentFolder -ChildPath $folder.Name

    Set-Location $folderloc 

    Get-Location
    
    $files = Get-ChildItem -Path .\ -File -Force -ErrorAction SilentlyContinue | Select-Object Name
    foreach ($file in $files) {
        # $fileName = Join-Path -Path $parentFolder -ChildPath $folder.Name -AdditionalChildPath $file.Name
        $fileName = $file.Name
        $foldername = $folder.Name

        kubectl apply -f $fileName -n $foldername
    }
    
}