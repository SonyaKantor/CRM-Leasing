using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LeasingActions
{
    public class NewCaseAction
    {
        private readonly IOrganizationService service;
        private readonly ITracingService tracingService;

        public NewCaseAction(IOrganizationService service, ITracingService tracingService = null)
        {
            this.service = service;
            this.tracingService = tracingService;

        }
        public Tuple<bool,string, Guid> CreateCase(string privateID, string driverID, string carPlate, int accidentType,string description)
        {
            bool SuccessFailure = true;
            string SuccessFailureDisc="";
            Guid vehicleID = Guid.Empty, VehicleDriverID=Guid.Empty, company = Guid.Empty;
            Entity caseToCreate = new Entity("incident");
            if (carPlate == null)
            {
                SuccessFailure = false;
                SuccessFailureDisc = "No car plate number was provided";
            }
            else
            {
                vehicleID = getCarByPlate(carPlate);
                if (vehicleID == Guid.Empty)
                {
                    SuccessFailure = false;
                    SuccessFailureDisc = "Car with provided number was not found";
                }
                else
                {
                    VehicleDriverID = getDriverByIdOrPlate(driverID, vehicleID);
                    if (VehicleDriverID == Guid.Empty)
                    {
                        SuccessFailure = false;
                        SuccessFailureDisc = "Driver of the car was not found";
                    }
                    else
                    {
                        company = getCompanyByDriver(privateID, driverID);


                    }
                }
            }
            caseToCreate["title"] = "Accident on vehicle " + carPlate + " at " + DateTime.Today.ToString();
            caseToCreate["son_vehicle"] = new EntityReference("son_vehicle", vehicleID);
            caseToCreate["son_driver"] = new EntityReference("son_driver",  VehicleDriverID);
            caseToCreate["son_account"] = new EntityReference("account",  company);
            caseToCreate["son_accidenttype"] = new OptionSetValue(accidentType);
            caseToCreate["customerid"] = new EntityReference("account", company);
            caseToCreate["son_description"] = description;

            Guid caseCreated = service.Create(caseToCreate);
             SuccessFailure = true;
             SuccessFailureDisc = "New incident case created successfully";
            Tuple<bool, string, Guid> result = new Tuple<bool, string, Guid>(SuccessFailure, SuccessFailureDisc, caseCreated);
            return result;
        }
        private Guid getCompanyByDriver(string privateID, string driverID)
        {
            QueryExpression query = new QueryExpression("account");

            query.Criteria.AddCondition(new ConditionExpression("son_privatecompanyid", ConditionOperator.Equal, privateID));
            EntityCollection drivers = service.RetrieveMultiple(query);
            if (drivers.Entities.Count > 0)
                return drivers.Entities[0].Id;
            QueryExpression q = new QueryExpression("son_driver");
            q.Criteria.AddCondition(new ConditionExpression("son_iddriver", ConditionOperator.Equal, driverID));
            q.ColumnSet = new ColumnSet("son_company");
            drivers = service.RetrieveMultiple(q);
            if (drivers.Entities.Count > 0)
            {
                EntityReference driverFound = (EntityReference)drivers[0]["son_company"];
                return driverFound.Id;
            }
                
            return Guid.Empty;
        }
        private Guid getDriverByIdOrPlate(string driverID, Guid vehicleID)
        {
            QueryExpression query = new QueryExpression("son_driver");
            
            query.Criteria.AddCondition(new ConditionExpression("son_iddriver", ConditionOperator.Equal, driverID));
            query.Criteria.FilterOperator = LogicalOperator.Or;
            query.Criteria.AddCondition(new ConditionExpression("son_vehicle", ConditionOperator.Equal,vehicleID));
            EntityCollection drivers = service.RetrieveMultiple(query);
            if(drivers.Entities.Count>0)
               return drivers.Entities[0].Id;
            return Guid.Empty;
        }
        private Guid getCarByPlate(string carPlate)
        {
            QueryExpression query = new QueryExpression("son_vehicle");
            query.Criteria.AddCondition(new ConditionExpression("son_platenumber", ConditionOperator.Equal, carPlate));
            
            EntityCollection vehicles = service.RetrieveMultiple(query);
            if(vehicles.Entities.Count > 0)
                return vehicles.Entities[0].Id;
            return Guid.Empty;
        }
    }
}
