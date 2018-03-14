using System.Collections.Generic;
using WHP;


namespace loginServer
{
    class LoginUserList
    {
        private static LoginUserList userList = new LoginUserList();
        private List<string> users;

        private LoginUserList()
        {
             users = new List<string>();
        }

        public LoginUserList GetInstance()
        {
            return userList;
        }

        public void AddUser(string id)
        {
            users.Add(id);
        }
    }


}
