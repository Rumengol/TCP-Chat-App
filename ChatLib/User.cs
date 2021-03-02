using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLib
{
    [Serializable]
    public class User
    {
        public User(string username)
        {
            ID = Guid.NewGuid();
            Name = username;
        }

        public Guid ID { get; set; }
        public string Name { get; set; }
    }
}
