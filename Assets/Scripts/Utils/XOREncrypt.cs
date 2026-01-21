using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class XOREncrypt
    {
        private static readonly byte KEY = 0x59;
        public static byte[] XOR(byte[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] ^= KEY;
            }
            return input;
        }
    }
}
