using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using UrisU.DataAccess;
using UrisU.Models;
using URISUtil.DataAccess;

namespace UrisU.Controllers
{
    [RoutePrefix("api/User")]
    public class UserControllers : ApiController
    {
        [Route(""), HttpGet]
        public IEnumerable GetAllUsers([FormUri]ActiveStatusEnum active = ActiveStatusEnum.Active)
        {
            return UserDB.GetAllUsers(active);
        }

        [Route("{id}"), HttpGet]
        public User GetUser(Guid id)
        {
            return UserDB.GetUser(id);
        }

        [Route("{id}"), HttpPost]
        public User InsertUser([FromBody]User user)
        {
            return UserDB.InsertUser(user);
        }

        [Route("{id}"), HttpPut]
        public User UpdateUser([FromBody]User user, Guid id)
        {
            return UserDB.UpdateUser(user, id);
        }

        [Route("{id}"), HttpDelete]
        public void DeleteUser(Guid id)
        {
            UserDB.DeleteUser(id);
        }


    }
}