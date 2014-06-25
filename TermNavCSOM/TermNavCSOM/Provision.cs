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
    class Provision
    {
        public static void ProvisionFiles()
        {
            string srcSiteUrl = "http://win-f33ohjutmmi/sites/cms";
            ClientContext clientContextSRC = new ClientContext(srcSiteUrl);
            Site srcSite = clientContextSRC.Site;

            clientContextSRC.Load(srcSite, s => s.ServerRelativeUrl, s => s.Url);
            clientContextSRC.ExecuteQuery();

            Web srcRootWeb = clientContextSRC.Site.RootWeb;
            Web srcCurrentWeb = clientContextSRC.Web;
            clientContextSRC.Load(srcRootWeb, rw => rw.Id);
            clientContextSRC.ExecuteQuery();
            clientContextSRC.Load(srcCurrentWeb, cw => cw.Id);
            clientContextSRC.ExecuteQuery();

            string srcMasterUrl = String.Format("{0}/_catalogs/masterpage/APCMS.master", srcSite.ServerRelativeUrl);
            File apcmsSrcFile = null;

            string srcLayoutUrl = String.Format("{0}/_catalogs/masterpage/BridgePage.aspx", srcSite.ServerRelativeUrl);
            File apcmsLayoutSrcFile = null;

            string srcColourFileUrl = String.Format("{0}/_catalogs/theme/15/PaletteAPCMS.spcolor", srcSite.ServerRelativeUrl);
            File apcmsColorSrcFile = null;
          

            ClientResult<System.IO.Stream> rs = null;
            ClientResult<System.IO.Stream> rsLayout = null;
            ClientResult<System.IO.Stream> rsColor = null;

            if (srcRootWeb.Id.ToString() == srcCurrentWeb.Id.ToString())
            {
                //load master page and page layout
                List masterPageGallery = srcRootWeb.Lists.GetByTitle("Master Page Gallery");
                Folder rootFolder = masterPageGallery.RootFolder;

                apcmsSrcFile = srcCurrentWeb.GetFileByServerRelativeUrl(srcMasterUrl);
                apcmsLayoutSrcFile = srcCurrentWeb.GetFileByServerRelativeUrl(srcLayoutUrl);

                clientContextSRC.Load(apcmsSrcFile);
                clientContextSRC.Load(apcmsLayoutSrcFile);

                clientContextSRC.ExecuteQuery();

                rs = apcmsSrcFile.OpenBinaryStream();
                rsLayout = apcmsLayoutSrcFile.OpenBinaryStream();

                clientContextSRC.ExecuteQuery();

                //load color file
                List themeGallery = srcRootWeb.Lists.GetByTitle("Theme Gallery");
                rootFolder = themeGallery.RootFolder;

                apcmsColorSrcFile = srcCurrentWeb.GetFileByServerRelativeUrl(srcColourFileUrl);

                clientContextSRC.Load(apcmsColorSrcFile);
                clientContextSRC.ExecuteQuery();
                rsColor = apcmsColorSrcFile.OpenBinaryStream();

                clientContextSRC.ExecuteQuery();
            }


            string siteUrl = "http://win-f33ohjutmmi/sites/pltest";
            ClientContext clientContext = new ClientContext(siteUrl);
            
            Site site = clientContext.Site;

            clientContext.Load(site, s => s.ServerRelativeUrl, s => s.Url);
            clientContext.ExecuteQuery();

            Web rootWeb = clientContext.Site.RootWeb;
            Web currentWeb = clientContext.Web;
            clientContext.Load(rootWeb, rw => rw.Id);
            clientContext.ExecuteQuery();
            clientContext.Load(currentWeb, cw => cw.Id);
            clientContext.ExecuteQuery();

            #region upload and set master page, also upload the page layout

            string masterUrl = String.Format("{0}/_catalogs/masterpage/APCMS.master", site.ServerRelativeUrl);
            string colorUrl = String.Format("{0}/_catalogs/theme/15/PaletteAPCMS.spcolor", site.ServerRelativeUrl);


            if (rootWeb.Id.ToString() == currentWeb.Id.ToString())
            {
                List masterPageGallery = rootWeb.Lists.GetByTitle("Master Page Gallery");
                Folder rootFolder = masterPageGallery.RootFolder;
                //master page
                FileCreationInformation fci = new FileCreationInformation();
                fci.ContentStream = rs.Value;
                fci.Url = "APCMS.master";
                fci.Overwrite = true;
                
                Microsoft.SharePoint.Client.File fileToUpload = rootFolder.Files.Add(fci);
                
                clientContext.Load(fileToUpload);

                fileToUpload.Publish("");

                currentWeb.CustomMasterUrl = masterUrl;
                currentWeb.Update();
                clientContext.ExecuteQuery();

                //page layout
                fci = new FileCreationInformation();
                fci.ContentStream = rsLayout.Value;
                fci.Url = "BridgePage.aspx";
                fci.Overwrite = true;

                fileToUpload = rootFolder.Files.Add(fci);

                fileToUpload.Publish("");
                clientContext.ExecuteQuery();

                ListItem item = fileToUpload.ListItemAllFields;

                ContentType targetDocumentSetContentType = GetContentType(clientContext, rootWeb, "Page Layout");
                item["ContentTypeId"] = targetDocumentSetContentType.Id.ToString();
                item.Update();
                clientContext.ExecuteQuery();

                targetDocumentSetContentType = GetContentType(clientContext, rootWeb, "Article Page");
                item["PublishingAssociatedContentType"] = String.Format(";#{0};#{1};#", targetDocumentSetContentType.Name, targetDocumentSetContentType.Id.ToString());
                item.Update();
                clientContext.ExecuteQuery();

                //color file
                List themeGallery = rootWeb.Lists.GetByTitle("Theme Gallery");
                clientContext.Load(themeGallery.RootFolder.Folders);//load the sub folder first !!!
                clientContext.ExecuteQuery();//must call

                rootFolder = themeGallery.RootFolder.Folders[0];
                fci = new FileCreationInformation();
                fci.ContentStream = rsColor.Value;
                fci.Url = "PaletteAPCMS.spcolor";
                fci.Overwrite = true;

                fileToUpload = rootFolder.Files.Add(fci);

                clientContext.ExecuteQuery();

                clientContext.Load(fileToUpload);

                rootWeb.ApplyTheme(colorUrl, null, null, true);
                rootWeb.Update();

                clientContext.ExecuteQuery();
            }

            #endregion
        }

        private static ContentType GetContentType(ClientContext ctx, Web web, string contentType)
        {
            ContentTypeCollection listContentTypes = web.ContentTypes;
            ctx.Load(listContentTypes, types => types.Include
                     (type => type.Id, type => type.Name,
                       type => type.Parent));
            var result = ctx.LoadQuery(listContentTypes.Where(c => c.Name == contentType));
            ctx.ExecuteQuery();
            ContentType targetDocumentSetContentType = result.FirstOrDefault();
            return targetDocumentSetContentType;
        }

    }
}
