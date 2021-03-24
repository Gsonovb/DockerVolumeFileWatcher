# This is package of app releases


#publish Dir
$publishdir = ".\publish"; # ""

#Realse Version
$version = "";

#FileName Prefix
$prefix = "FileWatcher";


Write-Host "System Info:"
$PSVersionTable



if ($env:publishdir -ne $null ) {
    $publishdir = $env:publishdir
}

if ($env:version -ne $null ) {
    $version = $env:version
}

if ($env:prefix -ne $null ) {
    $prefix = $env:prefix
}


if ($args -ne $null ) {
    foreach ($arg in $args) {

        if ($arg.StartsWith("publishdir:", [StringComparison]::InvariantCultureIgnoreCase)) {
            $publishdir = $arg.Substring("publishdir:".Length)
        }
        elseif ($arg.StartsWith("version:", [StringComparison]::InvariantCultureIgnoreCase)) {
            $version = $arg.Substring("version:".Length)
        }
        elseif ($arg.StartsWith("prefix:", [StringComparison]::InvariantCultureIgnoreCase)) {
            $prefix = $arg.Substring("prefix:".Length)
        }
    }
}


#check vars

if (-not ( Test-Path -Path $publishdir)) {
    Write-Host "Publish Dir does not exist : " $publishdir
}
else {
    
    $dirs = Get-ChildItem -Path $publishdir -Directory

    if ([string]::IsNullOrEmpty($version)) {
        $version = ""
    }
    else {
        $version = "-" + $version
    }

    foreach ($dir in $dirs) {
        
        $filename = "$prefix-$($dir.Name)$version.zip"

        $path = Join-Path -Path $dir.Parent.FullName -ChildPath $filename

        $files = $dir.GetFiles() | Where-Object { $_.Extension -ne ".pdb" }
        
        Write-Host "Compress Files in $dir  to $path"

        Compress-Archive -Path $files  -DestinationPath $path -Update
    }   

}

write-host "Done."