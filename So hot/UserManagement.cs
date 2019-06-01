using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace So_hot
{
    public class UserManagement
    {
        public static Users UserSession;
        public static void CreateSession(Users user)
        {
            if(user!=null)
            {
                UserSession = new Users();
                UserSession.Name = "Clark Kent";
                UserSession.Type = user.Type;
            }
        }
    }
}
