﻿using MoveShapeDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DisasterGame.Controllers
{
    public class PeopleModelController : ApiController
    {
        public PersonModel[] Get()
        {
            return Broadcaster.People.ToArray();
        }
    }
}