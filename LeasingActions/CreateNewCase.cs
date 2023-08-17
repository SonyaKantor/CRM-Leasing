using LeasingActions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeasingActions
{
    public class CreateNewCase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("LeasingActions");
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            string PrivateID = context.InputParameters.Contains("PrivateID") ? context.InputParameters["PrivateID"].ToString() : null;
            string DriveID = context.InputParameters.Contains("DriveID") ? context.InputParameters["DriveID"].ToString() : null;

            string CarPlate = context.InputParameters.Contains("CarPlate") ? context.InputParameters["CarPlate"].ToString() : null;
            int AccidentType = context.InputParameters.Contains("AccidentType") ? (int)context.InputParameters["AccidentType"] : 5;
            string Description = context.InputParameters.Contains("Description") ? context.InputParameters["Description"].ToString() : null;
            var caseAction = new NewCaseAction(service, tracingService);
            Tuple<bool, string, Guid> newCaseId = caseAction.CreateCase(PrivateID, DriveID, CarPlate, AccidentType, Description);
            context.OutputParameters["SuccessFailure"] = newCaseId.Item1;
            context.OutputParameters["SeccessFailureDisc"] = newCaseId.Item2;
            context.OutputParameters["IncidentID"] = new EntityReference("son_assetvisit", newCaseId.Item3);

        }

    }
}