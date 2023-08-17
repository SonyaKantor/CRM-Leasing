using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Graph;
using OrganizationRequest = Microsoft.Xrm.Sdk.OrganizationRequest;
using System.Runtime.Remoting.Contexts;

namespace LeasingJobs
{
    public class ActivateAction
    {
        IOrganizationService service;
        public OrganizationServiceContext svcContext = null;

        public ActivateAction(IOrganizationService m_service)
        {
            service = m_service;
            svcContext = new OrganizationServiceContext(service);
        }
        public void Run()
        {
            OrganizationRequest request = new OrganizationRequest("son_NewCaseAction");
            request.RequestId = Guid.NewGuid();
            request["PrivateID"] = "111111";
            request["DriveID"] = "x";
            request["CarPlate"] = "111";
            request["AccidentType"] = 3;
            request["Description"] = "Test";
            
            Console.WriteLine("Parameters:");
            foreach (var parameter in request.Parameters)
            {
                Console.WriteLine(parameter.Key + " " + parameter.Value);
            }
            OrganizationResponse response = service.Execute(request);
            Console.WriteLine(response["SeccessFailureDisc"]);
        }
    }
}
