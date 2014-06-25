using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Publishing.Navigation;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermNavCSOM
{
    class TermNav
    {
        public static void RePin()
        {
            Guid termSetGUID = new Guid("7ab9e8b0-e1e1-4a7c-9b20-d6c5030103df");
            Guid srcTermSetGUID = new Guid("4d8916d8-e226-45f6-83bd-9f2b134cc264");
            Guid srcDepartmentGUID = new Guid("eea07c73-7e2b-418d-96ff-1a2cdb4eb25c");

            string siteUrl = "http://win-f33ohjutmmi/sites/cms";
            ClientContext clientContext = new ClientContext(siteUrl);

            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
            taxonomySession.UpdateCache();

            clientContext.Load(taxonomySession, ts => ts.TermStores);
            clientContext.ExecuteQuery();

            if (taxonomySession.TermStores.Count == 0)
                throw new InvalidOperationException("The Taxonomy Service is offline or missing");

            TermStore termStore = taxonomySession.TermStores[0];
            clientContext.Load(termStore,
            ts => ts.Name,
            ts => ts.WorkingLanguage);
            clientContext.ExecuteQuery();

            // Does the TermSet object already exist?
            TermSet existingTermSet;
            TermGroup siteCollectionGroup;

            siteCollectionGroup = termStore.GetSiteCollectionGroup(clientContext.Site,
                                createIfMissing: true);

            existingTermSet = termStore.GetTermSet(termSetGUID);
            clientContext.Load(existingTermSet);
            clientContext.ExecuteQuery();
            if (!existingTermSet.ServerObjectIsNull.Value)
            {
                existingTermSet.DeleteObject();
                termStore.CommitAll();
                clientContext.ExecuteQuery();
            }

            TermSet termSet = siteCollectionGroup.CreateTermSet("CMSNavigationTermSet", termSetGUID,
                termStore.WorkingLanguage);

            clientContext.Load(termSet);
            
            termStore.CommitAll();
            clientContext.ExecuteQuery();

            //*******below code does not work, so commented it out*******//
            //NavigationTermSet navTermSet = NavigationTermSet.GetAsResolvedByWeb(clientContext,
            //    termSet, clientContext.Web, "GlobalNavigationTaxonomyProvider");
            //navTermSet.IsNavigationTermSet = true;
            //termStore.CommitAll();
            //clientContext.ExecuteQuery();
            //*******                                            *******//

            Term srcTerm = null;
            Term targetTerm = null;
            //get the source termset
            srcTerm = termStore.GetTerm(srcDepartmentGUID);
            clientContext.Load(srcTerm);
            clientContext.ExecuteQuery();
         
            if (!srcTerm.ServerObjectIsNull.Value)
            {
                targetTerm = termSet.ReuseTermWithPinning(srcTerm);
                targetTerm.CustomSortOrder = srcTerm.CustomSortOrder;
                termStore.CommitAll();
                clientContext.ExecuteQuery();
            }


            WebNavigationSettings navigationSettings = new WebNavigationSettings(clientContext, clientContext.Web);

            //******* this is weird, we have to reset the navigation to something else, then change it back to TaxonomyProvider, this way works.
            navigationSettings.GlobalNavigation.Source
                = StandardNavigationSource.PortalProvider;
            navigationSettings.Update(taxonomySession);
            clientContext.ExecuteQuery();
            //*********************************//

            navigationSettings.GlobalNavigation.Source 
                = StandardNavigationSource.TaxonomyProvider;
            navigationSettings.GlobalNavigation.TermStoreId
                = termStore.Id;
            navigationSettings.GlobalNavigation.TermSetId
                = termSet.Id;

            navigationSettings.Update(taxonomySession);
            //clientContext.Load(navigationSettings);
            clientContext.ExecuteQuery();

        }
    }
}
