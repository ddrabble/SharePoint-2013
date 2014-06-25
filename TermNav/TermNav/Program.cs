using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermNav
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagedRePin();
        }

        
       

        private static void ManagedRePin()
        {
            using (SPSite site = new SPSite("http://win-f33ohjutmmi/sites/cms"))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    TaxonomySession _TaxonomySession = new TaxonomySession(site);

                    //Get instance of the Term Store 
                    TermStore _TermStore = _TaxonomySession.TermStores["Managed Metadata Service"];

                    Group _Group = _TermStore.GetSiteCollectionGroup(site);


                    ////Create a new Term Set in the new Group
                    TermSet _TermSet = null;
                    try
                    {
                        _TermSet = _Group.TermSets["CMSNavigationTermSet"];
                    }
                    catch { }

                    if (_TermSet != null)
                    {
                        //_TermSet.Delete();
                        //_TermStore.CommitAll();

                        _TermSet.Terms["Department"].Delete();
                        _TermStore.CommitAll();
                    }
                    else
                    {
                        _TermSet = _Group.CreateTermSet("CMSNavigationTermSet");

                        NavigationTermSet navigationTermSet = NavigationTermSet.GetAsResolvedByWeb(_TermSet, site.RootWeb,
                            StandardNavigationProviderNames.CurrentNavigationTaxonomyProvider);
                        navigationTermSet.IsNavigationTermSet = true;
                    }


                    //locate the term in source farm
                    var srcgroup = from g in _TermStore.Groups where g.Name == "NavigationGroup" select g;
                    var srctermSet = srcgroup.FirstOrDefault().TermSets["GlobalNav"];
                    var srcterm = srctermSet.Terms["Department"];



                    var newterm = _TermSet.ReuseTermWithPinning(srcterm);
                    newterm.CustomSortOrder = srcterm.CustomSortOrder;


                    //commit changes
                    _TermStore.CommitAll();

                    var webNavigationSettings = new WebNavigationSettings(web);

                    webNavigationSettings.GlobalNavigation.Source = StandardNavigationSource.TaxonomyProvider;
                    webNavigationSettings.GlobalNavigation.TermStoreId = _TermStore.Id;
                    webNavigationSettings.GlobalNavigation.TermSetId = _TermSet.Id;

                    webNavigationSettings.Update();

                    var pubWeb = PublishingWeb.GetPublishingWeb(web);
                    pubWeb.Update();
                    web.Update();
                }

            }
        }


    }
}
