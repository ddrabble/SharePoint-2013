#Specify URL
$Site = "https://intranet.dev-sp.apci.com"
$GroupName = "APCI"
$TermSetName = "GlobalNav"
$TermSetGUID = "3fd033f3-fcf9-4c7b-9a12-07bea7a7e8bb"
$Term11GUID = "edab888e-66fd-49cb-a845-e7b2afef4a22"

Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Runtime.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Taxonomy.dll"
Add-Type -Path "C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Client.Publishing.dll"

#Bind to MMS
$clientContext = New-Object Microsoft.SharePoint.Client.ClientContext($Site)

$taxonomySession = [Microsoft.SharePoint.Client.Taxonomy.TaxonomySession]::GetTaxonomySession($clientContext)
$taxonomySession.UpdateCache()
if($taxonomySession.ServerObjectIsNull)
{
    Write-Host "TaxonomySession is not available" -ForegroundColor Red
    exit
}

$clientContext.Load($taxonomySession.TermStores)
$clientContext.ExecuteQuery()

$termStore = $taxonomySession.TermStores[0]

Write-Host ("Connected to TermStore exists: {0}" -f $termStore.Name) -ForegroundColor Green

$existingTermSet = $termStore.GetTermSet($termSetGUID)
$clientContext.Load($existingTermSet)
$clientContext.ExecuteQuery();
if (!$existingTermSet.ServerObjectIsNull)
{
    Write-Host ("TermSet exists: {0}" -f $existingTermSet.Id)
    $existingTermSet.DeleteObject();
    $termStore.CommitAll();
    $clientContext.ExecuteQuery();
    Write-Host ("Delected existing termset")
}

$clientContext.Load($termStore.Groups)
$clientContext.ExecuteQuery()

$termGroup = $termStore.Groups.GetByName($GroupName); 
$termSet = $termGroup.CreateTermSet($TermSetName, $termSetGUID, 1033)
$clientContext.Load($termSet)
$clientContext.ExecuteQuery()
Write-Host ("Created termset: {0}" -f $TermSetName)

$navTermSet = [Microsoft.SharePoint.Client.Publishing.Navigation.NavigationTermSet]::GetAsResolvedByWeb($clientContext, $termSet, $clientContext.Web, "GlobalNavigationTaxonomyProvider")
$navTermSet.IsNavigationTermSet = $true
$termStore.CommitAll()
$clientContext.ExecuteQuery()
Write-Host ("Set termset to Navigation Termset")

$termL11 = $termSet.CreateTerm("Department",1033,[System.Guid]::NewGuid().toString())
    $termL21 = $termL11.CreateTerm("GEO",1033,[System.Guid]::NewGuid().toString())
    $termL22 = $termL11.CreateTerm("HR",1033,[System.Guid]::NewGuid().toString())
    $termL23 = $termL11.CreateTerm("Internet Security",1033,[System.Guid]::NewGuid().toString())

$termStore.CommitAll()
$clientContext.ExecuteQuery()
Write-Host ("Created all children terms, finished") -ForegroundColor Green