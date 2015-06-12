using System.Web.Mvc;
using System.Web.Routing;

namespace CarbonIT.Snapblob
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "ShortUrlRoute",
               url: "r/{shortUrl}",
               defaults: new { controller = "Home", action = "Redirection" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
