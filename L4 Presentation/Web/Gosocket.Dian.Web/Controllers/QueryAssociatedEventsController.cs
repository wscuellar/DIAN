﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class QueryAssociatedEventsController : Controller
    {
        // GET: QueryAssociatedEvents
        public ActionResult Index(string cufe)
        {
            return View();
        }

        public ActionResult EventsView()
        {
            //Response.Headers["InjectingPartialView"] = "true";
            return View();
        }
    }
}