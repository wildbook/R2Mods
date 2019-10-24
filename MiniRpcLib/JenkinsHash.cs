using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public static class Hash
    {
        public static uint JenkinsOAAT(string data) => JenkinsOAAT(Encoding.UTF8.GetBytes(data));

        public static uint JenkinsOAAT(byte[] data)
        {
            uint hash = 0;
            foreach (var i in data)
            {
                hash += i;
                hash += hash << 10;
                hash ^= hash >> 6;
            }
            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;
            return hash;
        }
    }
}
