using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB
{
    public partial class UserTokenCache
    {
        public int UserTokenCacheId { get; set; }

        public string webUserUniqueId { get; set; }

        public byte[] cacheBits { get; set; }

        public DateTime LastWrite { get; set; }

        public int myid { get; set; }
    }
}
