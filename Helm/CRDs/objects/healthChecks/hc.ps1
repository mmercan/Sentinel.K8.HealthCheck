Get-ChildItem -File -Recurse -Filter *.yaml | 
Foreach-Object {

    $file = ".\" + $_.Directory.Name + "\" + $_.Name
    $folder = $_.Directory.Name
    Write-Host $file

    Write-Host " kubectl apply -f  $file -n  $folder "
    kubectl apply -f  $file -n  $folder 

    #     Write-Host $_.Directory.Name
    #     Write-Host $_.Name
}

