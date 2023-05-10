﻿using System;
using RedisMemoryCacheInvalidation;

namespace SampleInvalidationEmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Invalidation Emitter");

            InvalidationManager.Configure("localhost:6379", new InvalidationSettings());

            Console.WriteLine("IsConnected : " + InvalidationManager.IsConnected);

            Console.WriteLine("enter a key to send invalidation (default is 'mynotifmessage'): ");
            var key = Console.ReadLine();
            var task = InvalidationManager.InvalidateAsync(string.IsNullOrEmpty(key) ? "mynotifmessage" : key);

            Console.WriteLine("message send to {0} clients", task.Result);
            Console.ReadLine();
        }
    }
}
