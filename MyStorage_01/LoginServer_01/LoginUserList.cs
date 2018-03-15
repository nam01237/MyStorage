using System;
using System.Collections;
using WHP;


namespace LoginServer
{
    class LoginUserList
    {
        private static LoginUserList instance;
        private Hashtable users;
        private object thisLock;

        private LoginUserList()
        {
            users = new Hashtable();
            thisLock = new object();
        }

        public static LoginUserList GetInstance()
        {
            if (instance == null)
                instance = new LoginUserList();

            return instance;
        }

        public bool CorrectLogin(string key, Guid guid)
        {
            return ((Guid)(users[key]) == guid);
        }

        public bool AddUser(string id, Guid guid)
        {
            bool success;

            try
            {
                lock (thisLock)
                {
                    users.Add(id, guid);
                }

                success = true;
            }
            catch (ArgumentException)
            {
                success = false;
            }

            return success;
        }

        public void RemoveUser(string key)
        {
            users.Remove(key);
        }
    }


}
