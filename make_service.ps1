$relativePath = ".\TechKasConnector.exe"
$absolutePath = Resolve-Path $relativePath

New-Service -Name "TechKas Connector 1.1" `
    -BinaryPathName $absolutePath `
    -DisplayName "TechKas Connector 1.1" `
    -StartupType Automatic `
    -Credential (Get-Credential)
	
sc failure "TechKas Connector 1.1" reset= 86400 actions= restart/5000/restart/10000/restart/15000