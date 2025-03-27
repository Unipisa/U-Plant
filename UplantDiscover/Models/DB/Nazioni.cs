﻿using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Nazioni
{
    public string codice { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();
}
