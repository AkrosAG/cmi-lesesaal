using System;
using System.Web.Http;
using CMI.Web.Common.api;
using Serilog;

namespace CMI.Web.Frontend.api.Controllers
{
    public class NewsController : NewsControllerBase
    {
        [HttpGet]
        public IHttpActionResult GetRelevantNews(string lang)
        {
            try
            {
                var result = newsDataAccess.GetRelevantNews(lang);
                return Ok(result);
            }
            catch (Exception e)
            {
                Log.Error(e, "(NewsController:GetRelevantMessages({lang}))", lang);
                return InternalServerError();
            }
        }
    }
}