using System.IO;
using System.Linq;
using Csv;

namespace CK2CharacterCreator
{
    public class DynReader
    {
        public static void ReadDynasties(string pathToExistingDyns, ref string errormsg)
        {
            var csv = File.ReadAllText(pathToExistingDyns);
            var lines = CsvReader.ReadFromText(csv).ToArray();

            if(lines[0].Headers[0].Trim() != "Dynasty" ||
                lines[0].Headers[1].Trim() != "Culture" ||
                lines[0].Headers[2].Trim() != "Religion" ||
                lines[0].Headers[3].Trim() != "Custom number")
            {
                errormsg = "Headers are incorrect. Consult the readme for formatting guidelines.";
                return;
            }

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line[0]))
                {
                    Dynasty dyn = new Dynasty
                    {
                        Name = line[0],
                        Culture = line[1],
                        Religion = line[2]
                    };
                    if (!string.IsNullOrWhiteSpace(line[3]))
                    {
                        dyn.Id = int.Parse(line[3].Trim());
                    }
                }
            }
        }
    }
}
