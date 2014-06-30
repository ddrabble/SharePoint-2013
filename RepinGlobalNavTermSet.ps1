#Specify URL
$Site = "https://intranet.dev-sp.apci.com"
$TermSetName = "GlobalNav"
$TargetTermSetGUID = "7ab9e8b0-e1e1-4a7c-9b20-d6c5030103df"
$SrcTermSetGUID = "3fd033f3-fcf9-4c7b-9a12-07bea7a7e8bb"

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


$existingTermSet = $termStore.GetTermSet($TargetTermSetGUID)
$clientContext.Load($existingTermSet)
$clientContext.ExecuteQuery()
if (!$existingTermSet.ServerObjectIsNull)
{
    Write-Host ("TermSet exists: {0}" -f $existingTermSet.Id)
    $existingTermSet.DeleteObject()
    $termStore.CommitAll()
    $clientContext.ExecuteQuery()
    Write-Host ("Delected existing termset")
}


$siteCollectionGroup = $termStore.GetSiteCollectionGroup($clientContext.Site,$true);

$termSet = $siteCollectionGroup.CreateTermSet($TermSetName, $TargetTermSetGUID,1033);

$clientContext.Load($termSet);
$termStore.CommitAll()
$clientContext.ExecuteQuery()


$navTermSet = [Microsoft.SharePoint.Client.Publishing.Navigation.NavigationTermSet]::GetAsResolvedByWeb($clientContext, $termSet, $clientContext.Web, "GlobalNavigationTaxonomyProvider")
$navTermSet.IsNavigationTermSet = $true
$termStore.CommitAll()
$clientContext.ExecuteQuery()
Write-Host ("Set termset to Navigation Termset")



$srcTermSet = $termStore.GetTermSet($SrcTermSetGUID);
$clientContext.Load($srcTermSet.Terms);
$clientContext.ExecuteQuery();

foreach ($srcTerm in $srcTermSet.Terms)
{

    $clientContext.Load($srcTerm)
    $clientContext.ExecuteQuery()

    if (!$srcTerm.ServerObjectIsNull)
    {
        $targetTerm = $termSet.ReuseTermWithPinning($srcTerm)
        $targetTerm.CustomSortOrder = $srcTerm.CustomSortOrder
        $termStore.CommitAll()
        $clientContext.ExecuteQuery()
    }
}


$navigationSettings = New-Object Microsoft.SharePoint.Client.Publishing.Navigation.WebNavigationSettings($clientContext, $clientContext.Web)
$navigationSettings.GlobalNavigation.Source = [Microsoft.SharePoint.Client.Publishing.Navigation.StandardNavigationSource]::PortalProvider
$navigationSettings.Update($taxonomySession)
$clientContext.ExecuteQuery()
          
$navigationSettings.GlobalNavigation.Source = [Microsoft.SharePoint.Client.Publishing.Navigation.StandardNavigationSource]::TaxonomyProvider
$navigationSettings.GlobalNavigation.TermStoreId = $termStore.Id
$navigationSettings.GlobalNavigation.TermSetId = $termSet.Id

$navigationSettings.Update($taxonomySession)
$clientContext.ExecuteQuery()
Write-Host "Completed successfully!" -ForegroundColor Green