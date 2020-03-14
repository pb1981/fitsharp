﻿using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace fitSharp.StoryTest {
    [TestFixture]
    public class StoryTest {

        [Test]
        public void Run() {
            var current = Environment.CurrentDirectory;
            var root = current.Substring(0, current.IndexOf("source"));
            Console.WriteLine(root);
            var build = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            Console.WriteLine(build);
            var sourcePath = Path.GetDirectoryName(build);
            var destinationPath = Path.Combine(root, "build\\sandbox");
            Copy("fit", sourcePath, destinationPath);
            Copy("fitSharp", sourcePath, destinationPath);
            Copy("fitTest", sourcePath, destinationPath);
            Copy("fitSharpTest", sourcePath, destinationPath);
            Copy("Runner", sourcePath, destinationPath);
            Copy("Samples", sourcePath, destinationPath);
            Environment.CurrentDirectory = root;
            const string config = "storyTest.Config." +
                #if NETCOREAPP
                    "netcore"
                #else
                    "netfx"
                #endif
                + ".xml";
            Assert.AreEqual(0,  Runner.Program.Main(new [] {"-c", config}));
        }

        static void Copy(string project, string sourcePath, string destinationPath) {
            foreach (var file in Directory.GetFiles(sourcePath.Replace("StoryTest", project))) {
                if (Path.GetFileName(file).StartsWith(project)) {
                    Console.WriteLine(file);
                    File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true);
                }
            }
        }
    }
}