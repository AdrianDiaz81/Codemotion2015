﻿

namespace Hel10.Oxford.Speak
{
    using System.Runtime.Serialization;
    [DataContract]
    public class AccessTokenInfo
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }
}
