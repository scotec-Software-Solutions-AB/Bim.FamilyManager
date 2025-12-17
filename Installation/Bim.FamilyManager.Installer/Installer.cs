using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixSharp;

namespace Bim.FamilyManager.Installer
{
    public class Script
    {
        public static void Main(string[] args)
        {
            var project = new Project("MyProduct",
                new Dir(@"%ProgramFiles%\My Company\My Product",
                    new File(@"Files\Docs\Manual.txt"),
                    new File(@"Files\Bin\MyApp.exe")));

            project.GUID = new Guid("6f330b47-2577-43ad-9095-1861ba25889b");

            Compiler.BuildMsi(project);
        }
    }
}