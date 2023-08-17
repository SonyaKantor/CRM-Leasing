// Parameters
var parameters = {};
parameters.DriveID = "x"; // Edm.String
parameters.PrivateID = "12345"; // Edm.String
parameters.CarPlate = "111"; // Edm.String
parameters.AccidentType = 3; // Edm.Int32
parameters.Description = "Test"; // Edm.String

var req = new XMLHttpRequest();
req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/son_NewCaseAction", true);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.setRequestHeader("Accept", "application/json");
req.onreadystatechange = function () {
	if (this.readyState === 4) {
		req.onreadystatechange = null;
		if (this.status === 200 || this.status === 204) {
			var result = JSON.parse(this.response);
			console.log(result);
			// Return Type: mscrm.son_NewCaseActionResponse
			// Output Parameters
			var successfailure = result["SuccessFailure"]; // Edm.Boolean
			var seccessfailuredisc = result["SeccessFailureDisc"]; // Edm.String
			var incidentid = result["IncidentID"]; // mscrm.incident
		} else {
			console.log(this.responseText);
		}
	}
};
req.send(JSON.stringify(parameters));