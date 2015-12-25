﻿using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.AnatoliUser
{
    public class AnatoliUserModel : BaseDataModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string RoleName { get { return "User"; } set { RoleName = value; } }
        public string PrivateOwnerId { get { return "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C"; } set { PrivateOwnerId = value; } }
    }
}
