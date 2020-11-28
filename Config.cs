using System;
using System.IO;

namespace endoimport
{
    public static class Config
    {
        public static string GetConfigFile(string fileName) => Path.Combine(GetConfigDir(), fileName);
        public static string GetConfigDir() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".endoimport");
        
    }
}