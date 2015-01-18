﻿using System;
using System.Text;

namespace IronFoundry.Container
{
    public static class ContainerHandleGenerator
    {
        static readonly string Alphabet = "abcdefghijklmnopqrstuvwyxz0123456789";
        static readonly Random Random = new Random();

        public static string Generate(Random random)
        {
            // TODO: Consider a better algorithm for generating unique handles
            //       It's worth noting that the consumer should be responsible for
            //       generating the handles (and ensuring uniqueness), not the 
            //       container library.
            lock (random)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < 11; i++)
                {
                    var character = Alphabet[random.Next(0, Alphabet.Length)];
                    builder.Append(character);
                }
                return builder.ToString();
            }
        }

        public static string Generate()
        {
            return Generate(Random);
        }
    }
}
