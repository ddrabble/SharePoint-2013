#Specify URL
$Site = "https://intranet.dev-sp.apci.com"
$APFolder = "AP"
$APContentTypeName = "Article Page"
$MasterPagePath = $PSScriptRoot + "\intranet.master"

Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Runtime.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Taxonomy.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Publishing.dll"


$clientContext = New-Object Microsoft.SharePoint.Client.ClientContext($Site)

$clientContext.Load($clientContext.Site)
$clientContext.ExecuteQuery()

$masterPageGallery = $clientContext.Site.RootWeb.Lists.GetByTitle("Master Page Gallery")
$clientContext.Load($masterPageGallery.RootFolder.Folders)
$clientContext.ExecuteQuery()

$apExist = $false
foreach($f in $masterPageGallery.RootFolder.Folders)
{
    if ($f.Name -eq $APFolder)
    {
            $apFolder = $f
            $apExist = $true
            break
    }
}

if (!$apExist)
{
    #create the folder
    $apFolder = $masterPageGallery.RootFolder.Folders.Add($APFolder)
    $clientContext.ExecuteQuery()
}


if ([System.IO.File]::Exists($MasterPagePath))
{
    Write-Host ("Begin uploading masterpage exists: {0}" -f $MasterPagePath) 

    $fci = New-Object Microsoft.SharePoint.Client.FileCreationInformation
    $fci.Content = [System.IO.File]::ReadAllBytes($MasterPagePath)
    $fci.Url = "intranet.master"
    $fci.Overwrite = $true
                
    $fileToUpload = $apFolder.Files.Add($fci)
                
    $clientContext.Load($fileToUpload)

    $fileToUpload.Publish("")

    $clientContext.Site.RootWeb.CustomMasterUrl = "/_catalogs/masterpage/AP/intranet.master"
    $clientContext.Site.RootWeb.Update()
    $clientContext.ExecuteQuery()
}

Write-Host "Upload masterpage completed!" -ForegroundColor Green








Write-Host "Starting retrieve content type info......" -ForegroundColor Yellow
$listContentTypes = $clientContext.Site.RootWeb.ContentTypes;
$clientContext.Load($listContentTypes)
$clientContext.ExecuteQuery()   
foreach($ct in $listContentTypes)
{
    if ($ct.Name -eq "Page Layout")
    {
        $targetDocumentSetContentType = $ct
    }
    elseif ($ct.Name -eq $APContentTypeName)
    {
        $publishingContentType = $ct
    }
}
Write-Host "Starting upload page layout......" -ForegroundColor Yellow

$fileEntries = [IO.Directory]::GetFiles($PSScriptRoot); 
foreach($fileName in $fileEntries) 
{ 
    if ($fileName.EndsWith(".aspx"))
    {
        [Console]::WriteLine($fileName); 
        $fci = New-Object Microsoft.SharePoint.Client.FileCreationInformation
        $fci.Content = [System.IO.File]::ReadAllBytes($fileName)
        $fci.Url = [System.IO.Path]::GetFileNameWithoutExtension($fileName) + ".aspx"
        $fci.Overwrite = $true
                
        $fileToUpload = $apFolder.Files.Add($fci)
        $clientContext.Load($fileToUpload)
        $clientContext.ExecuteQuery()

        $item = $fileToUpload.ListItemAllFields
        $item["ContentTypeId"] = $targetDocumentSetContentType.Id.ToString()
        $item.Update()
        $clientContext.ExecuteQuery()

        $item["PublishingAssociatedContentType"] = ";#" + $publishingContentType.Name + ";#" + $publishingContentType.Id.ToString() + ";#"
        $item.Update()
        $clientContext.ExecuteQuery()

        $fileToUpload.Publish("")
        $clientContext.ExecuteQuery()
    }
}   

Write-Host "Upload page layout completed!" -ForegroundColor Green












