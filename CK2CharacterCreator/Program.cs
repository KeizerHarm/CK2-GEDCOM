using System;
using System.Collections.Generic;
using System.IO;

namespace CK2CharacterCreator
{
    class Program
    {
        public static List<Dynasty> Dynasties { get; set; } = new List<Dynasty>();
        public static List<Character> Characters { get; set; } = new List<Character>();

        public static int StartingCharId = -1;
        public static int StartingDynId = -1;
        private static string PathToExistingDyns = "";
        private static string PathToGedcom = "";

        static void Main(string[] args)
        {

            new Program();
            Console.ReadKey();
        }

        private static void SetUpValues()
        {
            
            while (StartingCharId == -1)
            {
                Console.WriteLine("At what character ID do you want to start counting?");
                try
                {
                    StartingCharId = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Please enter a number");
                }
            }
            while (StartingDynId == -1)
            {
                Console.WriteLine("At what dynasty ID do you want to start counting?");
                try
                {
                    StartingDynId = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Please enter a number");
                }
            }
            
        }

        public Program()
        {
            Console.WriteLine("Welcome to Keizer Harm's GEDCOM to CKII tool!" + Environment.NewLine +
                "I will transform a GEDCOM file into CKII character files." + Environment.NewLine +
                "Consult the readme for more specific guidelines.");
            SetUpValues();

            string errormsg = "";
            PathToGedcom = FindGedcom(ref errormsg);
            if(!string.IsNullOrEmpty(errormsg))
            {
                Console.WriteLine("I am having trouble finding your Gedcom file: " + errormsg + Environment.NewLine + "Aborting...");
                return;
            }
            Console.WriteLine("I found a GEDCOM file at this location: " + PathToGedcom);
            PathToExistingDyns = FindDyns(ref errormsg);
            if (!string.IsNullOrEmpty(errormsg))
            {
                Console.WriteLine("I am having trouble finding your dynasties file: " + errormsg + Environment.NewLine + "Aborting...");
                return;
            }
            Console.WriteLine("I found a dynasty file at this location: " + PathToExistingDyns);
            DynReader.ReadDynasties(PathToExistingDyns, ref errormsg);
            if (!string.IsNullOrEmpty(errormsg))
            {
                Console.WriteLine("I am having trouble reading your dynasties file: " + errormsg + Environment.NewLine + "Aborting...");
                return;
            }
            Console.WriteLine("I found " + Dynasties.Count + " dynasties, the last one being \"" + Dynasties[Dynasties.Count - 1].Name + "\".");

            GedcomReader.ReadGedcom(PathToGedcom, ref errormsg);
            if (!string.IsNullOrEmpty(errormsg))
            {
                Console.WriteLine("I am having trouble reading your gedcom file: " + errormsg + Environment.NewLine + "Do you want to continue, and sort out the problems yourself? Y/N.");
                bool answered = false;
                while (!answered)
                {
                    var key = Console.ReadKey().KeyChar;
                    if (key.ToString().ToLower().ToCharArray()[0] == 'y')
                    {
                        answered = true;
                        Console.WriteLine("Okidoki!");
                    }
                    else if (key.ToString().ToLower().ToCharArray()[0] == 'n')
                    {
                        return;
                    } else
                    {
                        Console.WriteLine("Enter y or n to answer.");
                    }
                }
            }
            Console.WriteLine("I found " + Characters.Count + " characters, the last one being \"" + Characters[Characters.Count - 1].Name + "\".");

            var currentdir = Directory.GetCurrentDirectory();
            Console.WriteLine("Writing the dynasties to the file " + currentdir + "\\OutputFiles\\dynasties.txt");
            WriteToFile(Path.Combine(new string[] { currentdir, "OutputFiles", "dynasties.txt" }), Dynasties);

            Console.WriteLine("Writing the characters to the file " + currentdir + "\\OutputFiles\\characters.txt");
            WriteToFile(Path.Combine(new string[] { currentdir, "OutputFiles", "characters.txt" }), Characters);

            Console.WriteLine("That should just about do it! Check your generated files for characters with noreligion or noculture, or any other mishaps that might have occurred.");
        }

        private static readonly string WELCOME = "===========================================================" + Environment.NewLine +
                                                 "Characters/dynasties generated by Keizer Harm's fancy tool!" + Environment.NewLine +
                                                 "Version 1.1!" + Environment.NewLine +
                                                 "===========================================================" + Environment.NewLine;

        private void WriteToFile(string path, IEnumerable<object> stuffToWrite)
        {

            File.WriteAllText(path, WELCOME + string.Join(Environment.NewLine, stuffToWrite));
            
        }

        private string FindGedcom(ref string errormsg)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "InputFiles");
            string[] gedcomfiles = Directory.GetFiles(path, "*.ged", SearchOption.AllDirectories);
            if (gedcomfiles.Length < 1) errormsg = "Could not find any gedcom files!";
            if (gedcomfiles.Length > 1) errormsg = "Found multiple gedcom files!";
            if (string.IsNullOrEmpty(errormsg)) return gedcomfiles[0];
            else return null;

        }

        private string FindDyns(ref string errormsg)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "InputFiles");
            string[] gedcomfiles = Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories);
            if (gedcomfiles.Length < 1) errormsg = "Could not find any dynasty files!";
            if (gedcomfiles.Length > 1) errormsg = "Found multiple dynasty files!";
            if (string.IsNullOrEmpty(errormsg)) return gedcomfiles[0];
            else return null;

        }

        private static void Print()
        {
            foreach (var dyn in Dynasties)
            {
                Console.WriteLine(dyn.ToString());
            }

            foreach (var chara in Characters)
            {
                Console.WriteLine(chara.ToString());
            }
        }
    }
}
