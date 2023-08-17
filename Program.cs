
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace LeasingJobs
{
    class Program
    { 
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Started on {DateTime.Now.ToString()}");
            VehicleList bl = new VehicleList(GetService());

            await bl.Run(); //Update folders
        //    ActivateAction a = new ActivateAction(GetService());
        //        a.Run();
        }



        public static IOrganizationService GetService()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var client = new CrmServiceClient(connectionString);
            if (client.IsReady == false)
            {
                throw new Exception($"Crm Service Client is not ready: {client.LastCrmError}", client.LastCrmException);
            }
            return client;
        }
         
    }
}
