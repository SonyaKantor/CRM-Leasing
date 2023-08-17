validation = true // used to define if form can be saved or not
function CheckType(executionContext)
{
    try {
        const formContext = executionContext.getFormContext()
        
        let typeChange = formContext.getAttribute("son_accidenttype").addOnChange(()=> {
            var accType = formContext.getControl("son_accidenttype").getAttribute().getValue()
        var vehicleId = formContext.getControl("son_vehicle").getAttribute().getValue()
        var vehicle = vehicleId[0].id.slice(1,-1)
        if(vehicleId===null)
            formContext.getControl("son_vehicle").setNotification("Please choose a vehicle","VehicleError");
        else{
            if(accType === 3)
                var dueDate = getDueDate(vehicle, executionContext)
            else if(accType === 4)
                var kms =  getKMS(vehicle, executionContext)
            else {
                validation = true;
                Xrm.Page.ui.clearFormNotification("1")
                Xrm.Page.ui.clearFormNotification("2")
                Xrm.Page.ui.clearFormNotification("3")
            }
                
        }
        console.log(validation)
        if(validation)
            validateCase(vehicle,accType,executionContext)
    })
    }
    catch(error){
        console.log(error)
    }
}
function getDueDate(vehicleID, executionContext)
{
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/son_vehicles("+vehicleID+")?$select=son_licenseduedate", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=*");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(this.response);
                // Columns
                var son_vehicleid = result["son_vehicleid"]; // Guid
                var son_licenseduedate = result["son_licenseduedate"]; // Date Time
                var son_licenseduedate_formatted = result["son_licenseduedate@OData.Community.Display.V1.FormattedValue"];
                console.log(son_licenseduedate)
                var today  = new Date()
                var date = new Date(son_licenseduedate)
                // miliseconds to days
                if((date - today)/1000/60/60/24 <=60){
                    Xrm.Page.ui.setFormNotification("It's not time for a test yet", "ERROR", "1");                    
                    validation = false
                }
                else{
                    Xrm.Page.ui.clearNotification("1")
                    validation =true
                }
                    
            } else {
                console.log(this.responseText);
            }
        }
    };
    req.send();
    
}
function getKMS(vehicleID, executionContext)
{
    var req = new XMLHttpRequest();
    
    req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/son_vehicles("+vehicleID+")?$select=son_nexttestkms,son_runkms", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=*");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(this.response);
                // Columns
                var son_vehicleid = result["son_vehicleid"]; // Guid
                var son_nexttestkms = result["son_nexttestkms"]; // Decimal
                var son_nexttestkms_formatted = result["son_nexttestkms@OData.Community.Display.V1.FormattedValue"];
                var son_runkms = result["son_runkms"]; // Decimal
                var son_runkms_formatted = result["son_runkms@OData.Community.Display.V1.FormattedValue"];
                if( son_nexttestkms - son_runkms >=3 ){
                    Xrm.Page.ui.setFormNotification("It's not time for a reparation yet", "ERROR", "2");
                    validation = false
                }     
                else{
                    Xrm.Page.ui.clearNotification("2")
                    validation =true
                }
                    
            } else {
                console.log(this.responseText);
            }
        }
    };

req.send();
}
function validateCase(vehicleID,accType,executionContext){
    var req = new XMLHttpRequest();
req.open("GET", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/incidents?$select=son_accidenttype,title&$filter=(son_accidenttype eq "+accType+" and _son_vehicle_value eq "+vehicleID+")", true);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.setRequestHeader("Accept", "application/json");
req.setRequestHeader("Prefer", "odata.include-annotations=*");
req.onreadystatechange = function () {
	if (this.readyState === 4) {
		req.onreadystatechange = null;
		if (this.status === 200) {
			var results = JSON.parse(this.response);
            // if at least one result was found, value is longer than 0 
            if(results.value.length>0){
                Xrm.Page.ui.setFormNotification("Same accident is already registered for this vehicle", "ERROR", "3");
                validation = false
            }
                    
			for (var i = 0; i < results.value.length; i++) {
				var result = results.value[i];
				// Columns
				var incidentid = result["incidentid"]; // Guid
				var son_accidenttype = result["son_accidenttype"]; // Choice
				var son_accidenttype_formatted = result["son_accidenttype@OData.Community.Display.V1.FormattedValue"];
				var title = result["title"]; // Text
                
			}

		} else {
			console.log(this.responseText);
		}
	}
};
req.send();
}
// triggered onSave. If one of the if's is turned validation to false, form not going to be saved
function preventSave(econtext) {
    var saveEvent = econtext.getEventArgs();
    if(!validation)
        saveEvent.preventDefault();
  }
