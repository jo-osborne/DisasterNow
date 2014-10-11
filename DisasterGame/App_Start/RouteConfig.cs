using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;

namespace DisasterGame
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "getshapes",
                routeTemplate: "getshapes",
                defaults: new { controller = "ShapeModel", action = "Get" }
            );

            routes.MapHttpRoute(
                name: "getpeople",
                routeTemplate: "getpeople",
                defaults: new { controller = "PeopleModel", action = "Get" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });


        }
    }
}