using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Roles
{
    public Guid Id { get; set; }

    public string Descr { get; set; }

    public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();
}
