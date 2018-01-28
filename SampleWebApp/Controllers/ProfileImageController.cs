using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SampleWebApp.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ProfileImageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProfileImageController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET api/<controller>/:userId
        [HttpGet("{userId}")]
        public ActionResult Get(string userId)
        {
            var user = _db.Users.Find(userId);
            if (user.ProfileImage != null)
            {
                Stream stream = new MemoryStream(user.ProfileImage);
                return new FileStreamResult(stream, "image/jpg");
            }

            return new EmptyResult();
        }
    }
}
