using Anotar.Serilog;
using Extensions.System.IO;
using System;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class MediaPlayerFilesystem
    {

        public static DirectoryPath BaseDir => AppDomain.CurrentDomain.BaseDirectory;
        public static DirectoryPath ImageDir => BaseDir.Combine("Images");
        public static DirectoryPath DataDir => BaseDir.Combine("Data");
        public static DirectoryPath LuaDir => BaseDir.Combine("Lua");

        public static void EnsureFoldersExist()
        {
            foreach (var dir in new [] { ImageDir, DataDir })
            {
                if (!dir.EnsureExists())
                {
                    LogTo.Debug($"MediaPlayer failed to create directory {dir}");
                }
            }
        }
    }
}
