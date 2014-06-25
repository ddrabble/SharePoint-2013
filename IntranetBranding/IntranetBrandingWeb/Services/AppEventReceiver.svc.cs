using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.EventReceivers;

namespace IntranetBrandingWeb.Services
{
    public class AppEventReceiver : IRemoteEventService
    {
        public SPRemoteEventResult ProcessEvent(SPRemoteEventProperties properties)
        {
            SPRemoteEventResult result = new SPRemoteEventResult();

            using (ClientContext clientContext = TokenHelper.CreateAppEventClientContext(properties, false))
            {
                if (clientContext != null)
                {
                    clientContext.Load(clientContext.Site);
                    clientContext.ExecuteQuery();

                    string masterUrl = String.Format("{0}/_catalogs/masterpage/seattle.master", 
                        clientContext.Site.ServerRelativeUrl);

                    clientContext.Web.MasterUrl = masterUrl;
                    clientContext.Web.CustomMasterUrl = masterUrl;
                    clientContext.Web.Update();
                    clientContext.Load(clientContext.Web);
                    clientContext.ExecuteQuery();
                }
            }

            return result;
        }

        public void ProcessOneWayEvent(SPRemoteEventProperties properties)
        {
            // This method is not used by app events
        }
    }
}
