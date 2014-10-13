using DisasterNow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DisasterNow.Controllers
{
    public class VehicleModelController : ApiController
    {
        public VehicleModel[] Get()
        {
            return Broadcaster.Vehicles.ToArray();
        }
    }
}
