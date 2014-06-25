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
    class Program
    {
        static void Main(string[] args)
        {
            Provision.ProvisionFiles();
            //TermNav.RePin();
        }
    }
}
