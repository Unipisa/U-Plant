using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class UserRole
{
    public Guid Id { get; set; }

    public Guid UserFK { get; set; }

    public Guid RoleFK { get; set; }

    public virtual Roles RoleFKNavigation { get; set; }

    public virtual Users UserFKNavigation { get; set; }
}
