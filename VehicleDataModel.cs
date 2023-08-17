using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeasingJobs
{
    public class VehicleDataModel
    {
        // _id, tozar, tozeret_eretz_nm , tozeret_nm, tozeret_cd, degem_nm, degem_cd,
        // _id
        public int TableId { get; set; } 
        // id in CRM table Manufacturer, lookup 
        public Guid ManufacturerId { get; set; }
        // tozeret_eretz_nm
        public string Country { get; set; }
        // degem_cd
        public int ModelCode { get; set; }
        // degem_nm
        public string ModelName { get; set; }
        // tozeret_cd
        public int ProductCode { get; set; }
        // tozeret_nm
        public string ProductName { get; set; }
        public override string ToString()
        {
            return ("Vehicle:"+ TableId +" "+ Country+" "+Country+" "+ ModelName+" "+ ProductName);
        }
        public void ClearData()
        {
            this.TableId = 0;
            this.ManufacturerId = Guid.Empty;
            this.Country = "";
            this.ModelCode = 0;
            this.ModelName = "";
            this.ProductCode = 0;
            this.ProductName = "";
        }
    }
}
