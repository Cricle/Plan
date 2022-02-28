using System;

namespace Plan.ChannelModel.Helpers
{
    public class RSAFullKey
    {
        public Guid Identity { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}
