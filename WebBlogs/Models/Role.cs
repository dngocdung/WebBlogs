﻿using System;
using System.Collections.Generic;

namespace WebBlogs.Models
{
    public partial class Role
    {
        public Role()
        {
            Accounts = new HashSet<Account>();
        }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? RoleDescription { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
